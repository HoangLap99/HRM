#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
////using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Words.NET;

namespace HRM.Api.Services.NhanVien
{
    public class FuncKhenThuong : FuncBase
    {
        public FuncKhenThuong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncKhenThuong> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        #region danhmucHinhThucKhenThuong
        public dynamic GetListdmHinhThucKhenThuong(ApplicationDbContext db)
        {
            return db.HRM_dmHinhThucKhenThuong.ToList();
        }
        public dynamic SetdmHinhThucKhenThuong(ApplicationDbContext db, dynamic dnm)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_dmHinhThucKhenThuong dmHinhThucKhenThuong = JsonConvert.DeserializeObject(dnm.ToString(), typeof(HRM_dmHinhThucKhenThuong)) as HRM_dmHinhThucKhenThuong;
                if (string.IsNullOrEmpty(dmHinhThucKhenThuong.IdGuid))
                {
                    dmHinhThucKhenThuong.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(dmHinhThucKhenThuong).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                    db.Entry(dmHinhThucKhenThuong).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return err;
            }
            catch (Exception e)
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
                return err;
            }
        }
        public dynamic DeletedmHinhThucKhenThuong(ApplicationDbContext db, dynamic dnm)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_dmHinhThucKhenThuong dmHinhThucKhenThuong = JsonConvert.DeserializeObject(dnm.ToString(), typeof(HRM_dmHinhThucKhenThuong)) as HRM_dmHinhThucKhenThuong;
                if (string.IsNullOrEmpty(dmHinhThucKhenThuong.IdGuid))
                {
                    err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "không tìm thấy id");
                    return err;
                }
                else
                    db.Entry(dmHinhThucKhenThuong).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                db.SaveChanges();

                return err;
            }
            catch (Exception e)
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
                return err;
            }
        }
        #endregion
        public dynamic GetNextSoPhieuQuyetDinhKhenThuong(ApplicationDbContext db)
        {
            dynamic dnn = new DynamicObject();
            dnn.SoQuyetDinh = GenerateNextCode(db.HRM_QuyetDinhKhenThuong.Select(x => x.SoQuyTrinh), "QTQDKT");
            return dnn;
        }

        public dynamic GetDanhSachQuyetDinhKhenThuong(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = TimKiem.KeyWord;
                int nCurrentPage = TimKiem.CurrentPage ?? 1;
                int PageSize = TimKiem.PageSize ?? 25;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 1;
                IEnumerable<HRM_QuyetDinhKhenThuong> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyetDinhKhenThuong
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyetDinhKhenThuong
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
                    result = result.Where(a => a.NgayHieuLuc >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayHieuLuc < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);

                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_QuyetDinhKhenThuong" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }

                List<HRM_QuyetDinhKhenThuong> itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in itemList)
                {
                    BoPhan CongTy = BoPhans.Find(x => x.Id == item.IdCongTyEOS);
                    if (CongTy != null)
                        item.TenCongty = CongTy.TenBoPhan;

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
        public dynamic SetQuyetDinhKhenThuong(ApplicationDbContext db, HRM_QuyetDinhKhenThuong item)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var quyTrinhDb = db.HRM_QuyetDinhKhenThuong.AsNoTracking().FirstOrDefault(a => a.IdGuid == item.IdGuid);
                if (!SetTruocHamUpdate(db, _CallWebService, quyTrinhDb, item, err))
                    return err;

                if (!UpdateQuyetDinhKhenThuong(item, db, err, eQuyTrinh_LoaiHam.Set))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                if (!SetSauHamUpdate(db, quyTrinhDb, item, err))
                    return err;

                err.Value = item;
                return err;
            });
        public bool UpdateQuyetDinhKhenThuong(Models.HRM_QuyetDinhKhenThuong item, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (item.listIdBoPhan != null && item.listIdBoPhan.Count > 0)
                    item.IdBoPhan = string.Join(",", item.listIdBoPhan);
                if (item.NgayKyUnix > 0)
                    item.NgayKy = UnixTimeStampToDateTime(item.NgayKyUnix);
                if (item.NgayHieuLucUnix > 0)
                    item.NgayHieuLuc = UnixTimeStampToDateTime(item.NgayHieuLucUnix);
                if (item.KyLuongThucHienUnix > 0)
                    item.KyLuongThucHien = UnixTimeStampToDateTime(item.KyLuongThucHienUnix);
                if (string.IsNullOrEmpty(item.IdGuid))
                {
                    item.IdDuAn = item.IdCongTyEOS;
                    if (string.IsNullOrEmpty(item.eAction))
                        item.eAction = DefineData.eAction.QUYTRINHQUYETDINHKHENTHUONG.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, item, err))
                        return false;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                   
                }
                else
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                var listNhanVien = db.HRM_QuyetDinhKhenThuongNhanVien.Where(r => r.IdQuyetDinhKhenThuong == item.IdGuid).ToList();
                db.HRM_QuyetDinhKhenThuongNhanVien.RemoveRange(listNhanVien);
                var listBoPhan = db.HRM_QuyetDinhKhenThuongBoPhan.Where(r => r.IdQuyetDinhKhenThuong == item.IdGuid).ToList();
                db.HRM_QuyetDinhKhenThuongBoPhan.RemoveRange(listBoPhan);
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == item.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                if (item.listIdBoPhan != null && item.listIdBoPhan.Count > 0)
                {
                    foreach (var Id in item.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = item.IdGuid;
                        bp.IdBoPhan = Id;
                        bp.TableName = nameof(HRM_QuyetDinhKhenThuong);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                if (item.lstNhanVien != null)
                {
                    foreach (var NhanVien in item.lstNhanVien)
                    {
                        NhanVien.IdGuid = Guid.NewGuid().ToString();
                        NhanVien.IdQuyetDinhKhenThuong = item.IdGuid;
                        db.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Added;


                    }
                }

                if (item.lstBoPhan != null)
                {
                    foreach (var BoPhan in item.lstBoPhan)
                    {
                        BoPhan.IdGuid = Guid.NewGuid().ToString();
                        BoPhan.IdQuyetDinhKhenThuong = item.IdGuid;
                        db.Entry(BoPhan).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }
                }

                if (item.lstFileDinhKem != null)
                {
                    foreach (var itemFile in item.lstFileDinhKem)
                    {
                        SetFileDinhKem(db, itemFile, DefineData.eTable.HRM_QuyetDinhKhenThuong, item.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, item, eLoaiHam, err))
                    return false;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateQuyetDinhKhenThuong data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return false;//new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }
            return true;//new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", item.IdGuid);
        }

        public dynamic GetQuyetDinhKhenThuong(ApplicationDbContext db, string Id)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            var item = db.HRM_QuyetDinhKhenThuong.FirstOrDefault(x => x.IdGuid == Id);
            if (item.IdBoPhan != null)
            {
                item.listIdBoPhan = new List<long>();
                if (item.IdBoPhan.Contains(","))
                {
                    string[] list = item.IdBoPhan.Split(',');
                    foreach (var bp in list)
                        item.listIdBoPhan.Add(long.Parse(bp));
                }
                else item.listIdBoPhan.Add(long.Parse(item.IdBoPhan));
            }
            string sLoai = DefineData.eTable.HRM_QuyetDinhKhenThuong.ToString();
            item.lstFileDinhKem = db.HRM_FileDinhKem.Where(x => x.idItem == Id && x.TableName == sLoai).ToList();

            item.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKy);
            item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);

            //List<BoPhan> listBoPhan = smdb.BoPhans.ToList();
            List<HRM_QuyetDinhKhenThuongBoPhan> quyetDinhKhenThuongBoPhans = db.HRM_QuyetDinhKhenThuongBoPhan.Where(x => x.IdQuyetDinhKhenThuong == Id).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var quyetDinh in quyetDinhKhenThuongBoPhans)
            {
                BoPhan nv = listBoPhan.FirstOrDefault(x => x.Id == quyetDinh.IdBoPhanEOS);
                if (nv != null)
                    quyetDinh.TenBoPhan = nv.TenBoPhan;
            }
            item.lstBoPhan = quyetDinhKhenThuongBoPhans;

            List<HRM_QuyetDinhKhenThuongNhanVien> quyetDinhKhenThuongNhanViens = db.HRM_QuyetDinhKhenThuongNhanVien.Where(x => x.IdQuyetDinhKhenThuong == Id).ToList();
            var listNhanVien = db.HRM_NhanVien.ToList();
            foreach (var quyetDinh in quyetDinhKhenThuongNhanViens)
            {
                HRM_NhanVien nv = listNhanVien.FirstOrDefault(x => x.IdGuid == quyetDinh.IdNhanVien);
                if (nv != null)
                {
                    quyetDinh.TenNhanVien = nv.TenDayDu;
                    quyetDinh.MaNhanVien = nv.MaNhanVien;
                }
                BoPhan bp = listBoPhan.FirstOrDefault(x => x.Id == nv.IdBoPhanEOS);
                if (bp != null)
                    quyetDinh.TenBoPhan = bp.TenBoPhan;
                var ChucVu = listChucDanh.FirstOrDefault(x => x.Id == nv.IdChucDanh);
                if (ChucVu != null)
                    quyetDinh.TenChucDanh = ChucVu.Ten;

            }
            item.lstNhanVien = quyetDinhKhenThuongNhanViens;

            if (string.IsNullOrEmpty(item.IdNguoiKy) == false)
                item.NguoiKy = db.HRM_NhanVien.FirstOrDefault(x => x.IdGuid == item.IdNguoiKy);
            if (!GetQuyTrinh(db, _CallWebService, item, err, true))
                return err;
            //if (string.IsNullOrEmpty(item.IdChucDanhNguoiKy) == false)
            //    item.ChucDanhNguoiKy = db.HRM_DM_ChucDanh.FirstOrDefault(x => x.IdGuid == item.IdChucDanhNguoiKy);
            return item;
        }

        public dynamic DeleteQuyetDinhKhenThuong(ApplicationDbContext db, dynamic dnm)
            => ExecuteInTransaction(db, () =>
            {
                HRM_QuyetDinhKhenThuong quyetDinh = JsonConvert.DeserializeObject<HRM_QuyetDinhKhenThuong>((string)dnm.ToString());
                var quyetDinhFind = db.HRM_QuyetDinhKhenThuong.FirstOrDefault(x => x.IdGuid == quyetDinh.IdGuid);
                if (quyetDinhFind == null)
                    return MessageReturn.Fail("không tìm thấy quyết định");

                db.Entry(quyetDinhFind).State = EntityState.Deleted;
                db.HRM_QuyetDinhKhenThuongNhanVien.RemoveRange(db.HRM_QuyetDinhKhenThuongNhanVien.Where(r => r.IdQuyetDinhKhenThuong == quyetDinh.IdGuid));
                db.HRM_QuyetDinhKhenThuongBoPhan.RemoveRange(db.HRM_QuyetDinhKhenThuongBoPhan.Where(r => r.IdQuyetDinhKhenThuong == quyetDinh.IdGuid));
                db.SaveChanges();
                return MessageReturn.Success("Xóa dữ liệu thành công");
            });

        public dynamic ChuyenTiepQuyetDinhKhenThuong(ApplicationDbContext db, dynamic dnm)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                HRM_QuyetDinhKhenThuong QuyTrinh = JsonConvert.DeserializeObject<HRM_QuyetDinhKhenThuong>((string)dnm.ToString());
                var pcuDatabase = db.HRM_QuyetDinhKhenThuong.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                    return err;

                if (!UpdateQuyetDinhKhenThuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    return err;

                if (QuyTrinh.isKetThuc)
                {
                    var listAdd = QuyTrinh.lstNhanVien.Select(nv => new HRM_TheoDoiThuongNhanVien
                    {
                        IdGuid = Guid.NewGuid().ToString(),
                        IdNhanVien = nv.IdNhanVien,
                        Ngay = QuyTrinh.NgayHieuLuc.Value,
                        IdCongTyEOS = QuyTrinh.IdCongTyEOS,
                        SoTien = nv.SoTien,
                        isTinhVaoLuong = nv.isTinhVaoLuong
                    }).ToList();
                    db.HRM_TheoDoiThuongNhanVien.AddRange(listAdd);
                }

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });

        public dynamic KhongDuyetQuyetDinhKhenThuong(ApplicationDbContext db, HRM_QuyetDinhKhenThuong QuyTrinh)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var pcuDatabase = db.HRM_QuyetDinhKhenThuong.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                    return err;

                if (!UpdateQuyetDinhKhenThuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    return err;

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });
        public dynamic ExportKhenThuongNhanVien(string IdQuyTrinh, string IdNhanVien, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_KhenThuong_DaiAn.docx");
            string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            HRM_QuyetDinhKhenThuong quytrinh = db.HRM_QuyetDinhKhenThuong.Find(IdQuyTrinh);
            var CongTy = BoPhans.Where(r => r.Id == quytrinh.IdCongTyEOS).FirstOrDefault();
            var listPhongBanLienquan = db.HRM_QuyetDinhKhenThuongBoPhan.Where(r => r.IdQuyetDinhKhenThuong == IdQuyTrinh).ToList();
            string phongBanlienQuan = "";
            var ChucVuNguoiKy = _CallWebService.GetChucVu(db, quytrinh.IdChucVuNguoiKy);
            if (ChucVuNguoiKy != null)
                doc.ReplaceText("@ChucVuNguoiKy", ChucVuNguoiKy.TenChucVu);
            else
                doc.ReplaceText("@ChucVuNguoiKy", "");
            foreach (var item in listPhongBanLienquan)
            {
                var BoPhanItem = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
                phongBanlienQuan = phongBanlienQuan + BoPhanItem.TenBoPhan + "; ";
            }
            doc.ReplaceText("@phongBanlienQuan", phongBanlienQuan);
            HRM_QuyetDinhKhenThuongNhanVien nhanvienKhenThuong = db.HRM_QuyetDinhKhenThuongNhanVien.Where(r => r.IdNhanVien == IdNhanVien && r.IdQuyetDinhKhenThuong == IdQuyTrinh).FirstOrDefault();
            var NguoiLaoDong = db.HRM_NhanVien.Find(IdNhanVien);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);

            string NgayThang = "ngày " + quytrinh.NgayKy.Value.Day + " tháng " + quytrinh.NgayKy.Value.Month + " năm " + quytrinh.NgayKy.Value.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            if (quytrinh.KhenThuong == null)
                quytrinh.KhenThuong = "";
            doc.ReplaceText("@VeViec", quytrinh.KhenThuong);
            if (nhanvienKhenThuong.GhiChu == null)
                nhanvienKhenThuong.GhiChu = "";
            doc.ReplaceText("@NoiDungKhenThuong", "- " + nhanvienKhenThuong.GhiChu);
            var ChucVuNLD = _CallWebService.GetChucVu(db, nhanvienKhenThuong.IdChucVuEOS);
            var BoPhan = _CallWebService.GetBoPhan(db, nhanvienKhenThuong.IdBoPhan);
            string ChucVuPhongBan = ChucVuNLD.TenChucVu + " - " + BoPhan.TenBoPhan;
            doc.ReplaceText("@ChucVuPhongBan", ChucVuPhongBan);
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan);

            string TenNguoiKy = db.HRM_NhanVien.Where(r => r.IdGuid == quytrinh.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
            doc.ReplaceText("@@TenNguoiKy", TenNguoiKy);

            string SoTien = "";
            if (nhanvienKhenThuong.SoTien > 0)
                SoTien = " Thưởng tiền mặt: " + nhanvienKhenThuong.SoTien.ToString("N0") + "đồng";
            doc.ReplaceText("@SoTien", SoTien);
            doc.ReplaceText("@NhanVien", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@NgayKy", quytrinh.NgayKy.Value.ToString("dd/MM/yyyy"));
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_KhenThuong_DaiAn.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportQuyetDinhKhenThuong(string Id, ApplicationDbContext db)
        {
            List<string> results = new List<string>();
            var quytrinh = db.HRM_QuyetDinhKhenThuong.FirstOrDefault(x => x.IdGuid == Id);
            if (quytrinh == null) return results;
            var listNhanVien = db.HRM_QuyetDinhKhenThuongNhanVien.Where(r => r.IdQuyetDinhKhenThuong == quytrinh.IdGuid).ToList();
            if (listNhanVien == null) return results;
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var listCongty = _CallWebService.GetDanhSachCongTy(db);
            List<string> listIdNV = listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
            var lstNhanVien = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).ToList();
            foreach (var item in listNhanVien)
            {
                
                var CongTy = listCongty.Find(x => x.Id == quytrinh.IdCongTyEOS);
                var NguoiLaoDong = lstNhanVien.FirstOrDefault(a=>a.IdGuid==item.IdNhanVien);
                if (CongTy == null || NguoiLaoDong == null) continue;
                var filenamegui = DateTime.Now.ToOADate();
                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_KhenThuong.docx");
                string sFileNameCopy =PathUploads+ "QD_KhenThuong_" + NguoiLaoDong.TenDayDu + "_" + quytrinh.NoiDung + "_" + filenamegui + ".docx";
                dynamic data = new DynamicObject();
                File.Copy(fileName, sFileNameCopy, true);
                var doc = DocX.Load(sFileNameCopy);
                string NgayThang = "ngày " + quytrinh.NgayKy?.Day + " tháng " + quytrinh.NgayKy?.Month + " năm " + quytrinh.NgayKy?.Year;
                doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                doc.ReplaceText("@TENCONGTY", CongTy.TenBoPhan.ToUpper());
                doc.ReplaceText("@NoiDung", quytrinh.NoiDung);
                doc.ReplaceText("@TienThuong", item.SoTien.ToString("N0") + " VNĐ");
                doc.ReplaceText("@NgayThang", NgayThang);
                doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
                var ChucVu = _CallWebService.GetChucDanh(db, NguoiLaoDong.IdChucDanh);
                var BoPhan = BoPhans.FirstOrDefault(a=>a.Id==NguoiLaoDong.IdBoPhanEOS);
                string TenChucVu = "";
                if (ChucVu != null)
                    TenChucVu = ChucVu.Ten;
                if (BoPhan != null)
                    TenChucVu = TenChucVu + " - " + BoPhan.TenBoPhan;
                doc.ReplaceText("@ChucVu", TenChucVu);
                doc.Save();
                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_KhenThuong.docx")
                                + "&path=" + Base64Encode(sFileNameCopy);
                //return data;
                results.Add(data.TenFile);
            }
            return results;
        }
    }

}


