#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Words.NET;

namespace HRM.Api.Services.NhanVien
{
    public class FuncSaThai : FuncBase
    {
        public FuncSaThai(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncSaThai> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        
        public dynamic GetNextSoPhieuQuyetDinhSaThai(ApplicationDbContext db)
        {
            dynamic dnn = new DynamicObject();
            dnn.SoQuyetDinh = GenerateNextCode(db.HRM_QuyetDinhSaThai.Select(x => x.SoQuyetDinh), "QTQDST");
            return dnn;
        }

        public dynamic GetDanhSachQuyetDinhSaThai(dynamic dyn, ApplicationDbContext db = null)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_QuyetDinhSaThai> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyetDinhSaThai
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyetDinhSaThai
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                if (listCongTy != null)
                {
                    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdCongTyEOS));
                }
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.NgayKy >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayKy < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                //if (TimKiem.IdBoPhan > 0)
                //    result = result.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhan);


                var itemList = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
                List<BoPhan> listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                List<string> lstNguoiKy = itemList.Select(r => r.IdNguoiKy).ToList();
                var Nhanviens = db.HRM_NhanVien.Where(r => lstNguoiKy.Contains(r.IdGuid)).ToList();

                //foreach (var item in listItem)
                //{

                foreach (var item in itemList)
                {
                    item.TenNguoiKy = Nhanviens.Where(r => r.IdGuid == item.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
                    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                    item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra --" + e.Message);
            }
        }
        public dynamic GetQuyetDinhSaThai(ApplicationDbContext db, string Id)
        {
            try
            {
                Models.HRM_QuyetDinhSaThai item = db.HRM_QuyetDinhSaThai.FirstOrDefault(x => x.IdGuid == Id);

                item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                item.GiaiQuyetDenNgayUnix = DateTimeToUnixTimeStamp(item.GiaiQuyetDenNgay);
                item.BanGiaoTrongNgayUnix = DateTimeToUnixTimeStamp(item.BanGiaoTrongNgay);
                item.TenNguoiKy = db.HRM_NhanVien.Where(r => r.IdGuid == item.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
                List<Models.HRM_QuyetDinhSaThaiBoPhan> quyetDinhKhenThuongBoPhans = db.HRM_QuyetDinhSaThaiBoPhan.Where(x => x.IdQuyetDinhSaThai == Id).ToList();
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var quyetDinh in quyetDinhKhenThuongBoPhans)
                {
                    BoPhan nv = listBoPhan.FirstOrDefault(x => x.Id == quyetDinh.IdBoPhanEOS);
                    if (nv != null)
                        quyetDinh.TenBoPhan = nv.TenBoPhan;
                }
                item.lstBoPhan = quyetDinhKhenThuongBoPhans;
                var lstTaiSanBanGiao = db.HRM_SaThaiTaiSanBanGiao.Where(r => r.IdQuyetDinhSaThai == Id).ToList();
                var dmTaiSans = db.HRM_dmTrangThietBi.ToList();
                foreach (var itemTS in lstTaiSanBanGiao)
                {
                    itemTS.NgayBanGiaoUnix = dungchung.DateTimeToUnixTimeStamp(itemTS.NgayBanGiao);

                    itemTS.TenTaiSanBanGiao = dmTaiSans.Where(r => r.IdGuid == itemTS.IddmTaiSan).Select(r => r.Ten).FirstOrDefault();
                }
                item.NhanVien = db.HRM_NhanVien.Find(item.IdNhanVien);
                item.lstTaiSanBanGiao = lstTaiSanBanGiao;
                item.lstFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_QuyetDinhSaThai.ToString(), item.IdGuid);
                return item;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra --" + e.Message);
            }
        }

        public dynamic SetQuyetDinhSaThai(HRM_QuyetDinhSaThai model, ApplicationDbContext db)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var quyTrinhDb = db.HRM_QuyetDinhSaThai.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                if (!SetTruocHamUpdate(db, _CallWebService, quyTrinhDb, model, err))
                    return err;

                if (!FuncUpdateQuyetDinhSaThai(model, db, err, eQuyTrinh_LoaiHam.Set))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                if (!SetSauHamUpdate(db, quyTrinhDb, model, err))
                    return err;

                return MessageReturn.Success("Cập nhật thành công", model.IdGuid);
            });
        public bool FuncUpdateQuyetDinhSaThai(HRM_QuyetDinhSaThai item, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                item.NgayKy = UnixTimeStampToDateTime(item.NgayKyUnix);
                item.NgayHieuLuc = UnixTimeStampToDateTime(item.NgayHieuLucUnix);
                if (string.IsNullOrEmpty(item.IdGuid))
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                   

                }

                var lstTaiSanBanGiaoOld = db.HRM_SaThaiTaiSanBanGiao.Where(r => r.IdQuyetDinhSaThai == item.IdGuid).ToList();
                db.HRM_SaThaiTaiSanBanGiao.RemoveRange(lstTaiSanBanGiaoOld);

                var lstiBoPhanOld = db.HRM_QuyetDinhSaThaiBoPhan.Where(r => r.IdQuyetDinhSaThai == item.IdGuid).ToList();
                db.HRM_QuyetDinhSaThaiBoPhan.RemoveRange(lstiBoPhanOld);
                if (item.lstTaiSanBanGiao != null)
                {

                    foreach (var itemTC in item.lstTaiSanBanGiao)
                    {
                        itemTC.NgayBanGiao = dungchung.UnixTimeStampToDateTime(itemTC.NgayBanGiaoUnix);
                        itemTC.IdGuid = Guid.NewGuid().ToString();
                        itemTC.IdQuyetDinhSaThai = item.IdGuid;

                    }
                    db.HRM_SaThaiTaiSanBanGiao.AddRange(item.lstTaiSanBanGiao);
                }
               
                if (item.lstBoPhan != null)
                {

                    foreach (var itemBP in item.lstBoPhan)
                    {

                        itemBP.IdGuid = Guid.NewGuid().ToString();
                        itemBP.IdQuyetDinhSaThai = item.IdGuid;

                    }
                    db.HRM_QuyetDinhSaThaiBoPhan.AddRange(item.lstBoPhan);
                }

                if (item.lstFileDinhKem != null)
                {
                    foreach (var itemFile in item.lstFileDinhKem)
                    {
                        SetFileDinhKem(db, itemFile, DefineData.eTable.HRM_QuyetDinhSaThai, item.IdGuid);
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateQuyetDinhSaThai data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return false;
            }
            return true;
        }
        public dynamic DeleteQuyetDinhSaThai(ApplicationDbContext db, dynamic dnm)
            => ExecuteInTransaction(db, () =>
            {
                HRM_QuyetDinhSaThai quyetDinh = JsonConvert.DeserializeObject<HRM_QuyetDinhSaThai>((string)dnm.ToString());
                var quyetDinhFind = db.HRM_QuyetDinhSaThai.AsNoTracking().FirstOrDefault(x => x.IdGuid == quyetDinh.IdGuid);
                if (quyetDinhFind == null)
                    return MessageReturn.Fail("không tìm thấy quyết định");

                db.Entry(quyetDinhFind).State = EntityState.Deleted;
                db.HRM_SaThaiTaiSanBanGiao.RemoveRange(db.HRM_SaThaiTaiSanBanGiao.Where(r => r.IdQuyetDinhSaThai == quyetDinh.IdGuid));
                db.HRM_QuyetDinhSaThaiBoPhan.RemoveRange(db.HRM_QuyetDinhSaThaiBoPhan.Where(r => r.IdQuyetDinhSaThai == quyetDinh.IdGuid));
                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });

        public MessageReturn ChuyenTiepQuyetDinhSaThai(ApplicationDbContext db, HRM_QuyetDinhSaThai QuyTrinh)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var pcuDatabase = db.HRM_QuyetDinhSaThai.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                    return err;

                if (!FuncUpdateQuyetDinhSaThai(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    return err;

                if (QuyTrinh.isKetThuc)
                {
                    LogLichSuNhanVien(db, QuyTrinh.IdNhanVien, QuyTrinh.IdGuid, "HRM_QuyetDinhSaThai", DefineData.eAction.QUYTRINHQUYETDINHSATHAI.ToString(), QuyTrinh.NgayKyUnix, QuyTrinh.NoiDung);

                    var nhanVien = db.HRM_NhanVien.Find(QuyTrinh.IdNhanVien);
                    if (nhanVien != null)
                    {
                        nhanVien.TrangThai = "DaNghiViec";
                        nhanVien.NgayNghiViec = QuyTrinh.GiaiQuyetDenNgay;
                        db.Entry(nhanVien).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });

        public dynamic KhongDuyetQuyetDinhSaThai(ApplicationDbContext db, HRM_QuyetDinhSaThai QuyTrinh)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var pcuDatabase = db.HRM_QuyetDinhSaThai.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                    return err;

                if (!FuncUpdateQuyetDinhSaThai(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    return err;

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });

        public dynamic ExportSaThai(string Id, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuyetDinhThoiViec.docx");
            string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            HRM_QuyetDinhSaThai quytrinh = db.HRM_QuyetDinhSaThai.Find(Id);
            dynamic data = new DynamicObject();
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var CongTy = BoPhans.Find(x=>x.Id == quytrinh.IdCongTyEOS);
            var NguoiLaoDong = db.HRM_NhanVien.Find(quytrinh.IdNhanVien);
            var HopDOng = db.HRM_HopDong.Where(r => r.IdNhanVien == quytrinh.IdNhanVien).OrderByDescending(r => r.Created).First();
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            string TenNguoiKy=db.HRM_NhanVien.Where(r=>r.IdGuid== quytrinh.IdNguoiKy).Select(r=>r.TenDayDu).FirstOrDefault();
            doc.ReplaceText("@@TenNguoiKy", TenNguoiKy);
            doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
                doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@ThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi);
            string NgayThangNam = "ngày " + quytrinh.NgayKy.ToString("dd") + " tháng " + quytrinh.NgayKy.ToString("MM") + " năm " + quytrinh.NgayKy.ToString("yyyy");
            doc.ReplaceText("@NgayThangNam", NgayThangNam);
            var chucvuNhanVien = _CallWebService.GetChucVu(db, HopDOng.IddmChucVu);
            var BoPhanNhanVien = BoPhans.Where(r => r.Id == quytrinh.IdBoPhanNhanVien).FirstOrDefault();
            if (BoPhanNhanVien != null)
            {
                doc.ReplaceText("@BoPhanNhanVien", BoPhanNhanVien.TenBoPhan);
            }
            else
            {
                doc.ReplaceText("@BoPhanNhanVien", "");
            }
            if (chucvuNhanVien != null)
            {
                doc.ReplaceText("@ChucVuNhanVien", chucvuNhanVien.TenChucVu);
            }
            else
            {
                doc.ReplaceText("@ChucVuNhanVien", "");
            }
            doc.ReplaceText("@SoNgayPhepCongLai", quytrinh.SoNgayPhepCon.ToString("N0"));
            doc.ReplaceText("@NgayXinNghi", quytrinh.NgayKy.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NgayBanGiaoTruoc", quytrinh.BanGiaoTrongNgay.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NgayBanGiaoTruoc", quytrinh.BanGiaoTrongNgay.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NgayBanGiaoToi", quytrinh.GiaiQuyetDenNgay.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NamXinNghi", quytrinh.NgayKy.ToString("yyyy"));
            if (quytrinh.GhiChu == null)
                quytrinh.GhiChu = "";
            if (quytrinh.NoiDung == null)
                quytrinh.NoiDung = "";
            doc.ReplaceText("@LyDoThoiViec", quytrinh.NoiDung);
            if (quytrinh.TroCapThoiViec == null)
                quytrinh.TroCapThoiViec = "";
            doc.ReplaceText("@TroCapThoiViec", quytrinh.TroCapThoiViec);
            var listPhongBanLienquan = db.HRM_QuyetDinhSaThaiBoPhan.Where(r => r.IdQuyetDinhSaThai == Id).ToList();
            string phongBanlienQuan = "";


            foreach (var item in listPhongBanLienquan)
            {
                var BoPhanItem = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
                phongBanlienQuan = phongBanlienQuan + BoPhanItem.TenBoPhan + "; ";
            }
            doc.ReplaceText("@phongbanlienquan", phongBanlienQuan);

            doc.Save();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QuyetDinhThoiViec.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);

            return data;
        }
    }
}