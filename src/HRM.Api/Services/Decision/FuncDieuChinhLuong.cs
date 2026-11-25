#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.VariantTypes;
using HRM.Api.Services.HopDong;
using HRM.Api.Entity;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.FunctionCompilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Xceed.Words.NET;
using static HRM.Api.Services.FuncNhanVien;
using static HRM.Api.Helper.HelperConstant;
using static HRM.Api.Helper.HelperConstant.LuongHopDong;

namespace HRM.Api.Services.NhanVien
{
    public class FuncDieuChinhLuong : FuncBase
    {
        private readonly FuncHopDong _funcHopDong;
        private string Host => HrmOptions.Host ?? "0";

        public FuncDieuChinhLuong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncDieuChinhLuong> logger,
            FuncHopDong funcHopDong)
            : base(callWebService, workflowEngine, options, logger)
        {
            _funcHopDong = funcHopDong;
        }
        public dynamic GetNextMaDieuChinhLuong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyetDinhDieuChinhLuong.Select(x => x.SoQuyTrinh), DefineData.MaQuyetDinhDieuChinhLuong));
        public dynamic GetDanhSachDieuChinhLuong(dynamic dyn, ApplicationDbContext db = null)
        {

            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_QuyetDinhDieuChinhLuong> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyetDinhDieuChinhLuong
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyetDinhDieuChinhLuong
                             orderby a.Created descending
                             select a;

                //var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                //if (listCongTy != null)
                //{
                //    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                //    result = result.Where(a => listId.Contains(a.IdCongTyEOS));
                //}
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);

                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => dtTuNgay <= a.NgayHieuLuc);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => dtDenNgay >= a.NgayHieuLuc);

                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_QuyetDinhDieuChinhLuong" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }
                var itemList = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
                //var lstIdNhanVien = itemList.Select(r => r.IdNhanVien).ToList();
                //var lstNhanVien = db.HRM_NhanVien.Where(r => lstIdNhanVien.Contains(r.IdGuid)).ToList();
                var listBoPhan = _CallWebService.GetDanhSachDuAn(db);
                foreach (var item in itemList)
                {
                    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                    item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                    var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                    if (CongTy != null)
                        item.TenCongTy = CongTy.TenBoPhan;
                    //var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                    //if (BoPhan != null)
                    //    item.TenBoPhan = BoPhan.TenBoPhan;
                    //var nhanVien = lstNhanVien.Where(r => r.IdGuid == item.IdNhanVien).FirstOrDefault();
                    //if (nhanVien != null)
                    //{
                    //    item.TenNhanVien = nhanVien.TenDayDu;
                    //    item.MaNhanVien = nhanVien.MaNhanVien;
                    //}
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
        public dynamic SetQuyTrinhDieuChinhLuong(HRM_QuyetDinhDieuChinhLuong item, ApplicationDbContext db = null)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var listId = item.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var hopDongs = db.HRM_HopDong.AsNoTracking().Where(r => listId.Contains(r.IdNhanVien) && r.isKetThuc == true).GroupBy(a => a.IdNhanVien).Select(a => a.OrderByDescending(x => x.NgayHieuLuc).ThenByDescending(x => x.Modified).FirstOrDefault()).ToList();
                    if (hopDongs.Count != listId.Count)
                    {
                        var listIDCheck = hopDongs.Select(a => a.IdNhanVien).Distinct().ToList();
                        listId = listId.Where(a => !listIDCheck.Contains(a)).ToList();
                        var HopDong = db.HRM_NhanVien.AsNoTracking().Where(a => listId.Contains(a.IdGuid)).ToList();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên chưa có hợp đồng: " + string.Join(",", HopDong.Select(a => a.TenDayDu).ToList()));
                    }
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var QuyTrinhdb = db.HRM_QuyetDinhDieuChinhLuong.AsNoTracking().FirstOrDefault(a => a.IdGuid == item.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, item, err))
                        return err;

                    bool check = UpdateDieuChinhLuong(item, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!SetSauHamUpdate(db, QuyTrinhdb, item, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }


                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", item.IdGuid);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu thành không công " + ex.Message);
                }
            }
        }
        public bool UpdateDieuChinhLuong(HRM_QuyetDinhDieuChinhLuong item, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            if (item.listIdBoPhan != null && item.listIdBoPhan.Count > 0)
                item.IdBoPhan = string.Join(",", item.listIdBoPhan);
            if (item.NgayKyUnix > 0)
                item.NgayKy = dungchung.UnixTimeStampToDateTime(item.NgayKyUnix);
            if (item.NgayHieuLucUnix > 0)
                item.NgayHieuLuc = dungchung.UnixTimeStampToDateTime(item.NgayHieuLucUnix);
            try
            {
                if (string.IsNullOrEmpty(item.IdGuid))
                {
                    item.IdDuAn = item.IdCongTyEOS;
                    if (string.IsNullOrEmpty(item.eAction))
                        item.eAction = DefineData.eAction.QUYTRINHDIEUCHINHLUONG.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, item, err))
                        return false;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {

                    //var listBoPhans = db.HRM_QuyetDinhDieuChinhLuong_BoPhan.Where(r => r.IdQuyetDinh == item.IdGuid).ToList();
                    //db.HRM_QuyetDinhDieuChinhLuong_BoPhan.RemoveRange(listBoPhans);
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                var listPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == item.IdGuid).ToList();
                db.HRM_HopDongPhuCap.RemoveRange(listPhuCap);
                var listNhanVien = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(r => r.IdQuyetDinh == item.IdGuid).ToList();
                db.HRM_QuyetDinhDieuChinhLuong_NhanVien.RemoveRange(listNhanVien);
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
                        bp.TableName = nameof(HRM_QuyetDinhDieuChinhLuong);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                if (item.listNhanVien != null)
                {
                    int STT = 1;
                    foreach (var NhanVien in item.listNhanVien)
                    {
                        NhanVien.NgayHieuLuc = item.NgayHieuLuc;
                        NhanVien.IdQuyetDinh = item.IdGuid;
                        NhanVien.SapXep = STT;
                        NhanVien.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        if (NhanVien.lstPhuCap != null)
                            foreach (var itemPhuCap in NhanVien.lstPhuCap)
                            {
                                itemPhuCap.NgayBatDau = dungchung.UnixTimeStampToDateTime(itemPhuCap.NgayBatDauUnix);
                                itemPhuCap.NgayKetThuc = dungchung.UnixTimeStampToDateTime(itemPhuCap.NgayKetThucUnix);
                                itemPhuCap.IdQuyetDinh = item.IdGuid;
                                itemPhuCap.TableName = DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString();
                                itemPhuCap.IdGuid = Guid.NewGuid().ToString();
                                itemPhuCap.IdNhanVien = NhanVien.IdNhanVien;
                                db.Entry(itemPhuCap).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                            }
                        STT++;

                    }

                }


                if (item.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in item.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_QuyetDinhDieuChinhLuong, item.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, item, eLoaiHam, err))
                    return false;

                db.SaveChanges();
            }

            catch (Exception ex)
            {
                LogErrorFormat("UpdateDieuChinhLuong data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return false;
            }

            return true;
        }
        public dynamic GetDieuChinhLuong(string Id, ApplicationDbContext db)
        {
            var item = db.HRM_QuyetDinhDieuChinhLuong.FirstOrDefault(x => x.IdGuid == Id);
            if (item == null)
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy bổ nhiệm chứ vụ");
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
            item.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKy);
            item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);
            item.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString(), Id);
            item.listNhanVien = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(a => a.IdQuyetDinh == item.IdGuid).OrderBy(a => a.SapXep).ToList();
            var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => string.IsNullOrEmpty(r.IdHopDong) && r.IdQuyetDinh == Id && r.TableName == DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString()).ToList();
            foreach (var itemPC in lstPhuCap)
            {
                itemPC.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(itemPC.NgayBatDau);
                itemPC.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(itemPC.NgayKetThuc);
            }
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            List<string> listIdNhanVien = item.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
            foreach (var nhanVien in item.listNhanVien)
            {
                HRM_NhanVien nv = listNhanVien.FirstOrDefault(x => x.IdGuid == nhanVien.IdNhanVien);
                if (nv != null)
                {
                    nhanVien.MaNhanVien = nv.MaNhanVien;
                    nhanVien.TenNhanVien = nv.TenDayDu;
                }
                BoPhan bp = listBoPhan.FirstOrDefault(x => x.Id == nv.IdBoPhanEOS);
                if (bp != null)
                    nhanVien.TenBoPhan = bp.TenBoPhan;
                var ChucDanh = listChucDanh.FirstOrDefault(x => x.Id == nv.IdChucDanh);
                if (ChucDanh != null)
                    nhanVien.TenChucDanh = ChucDanh.Ten;
                if (nhanVien.NgayHieuLuc != null)
                    nhanVien.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(nhanVien.NgayHieuLuc);
                nhanVien.lstPhuCap = lstPhuCap.Where(a => a.IdNhanVien == nhanVien.IdNhanVien).OrderBy(a => a.Created).ToList();
            }

            return item;
        }

        //public dynamic ExportDieuChinhLuong(string Id, ApplicationDbContext db)
        //{
        //    var filenamegui = Guid.NewGuid().ToString();
        //    string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_TangLuong_DaiAn.docx");
        //    string sFileNameCopy =PathUploads+ filenamegui + ".docx";
        //    HRM_QuyetDinhDieuChinhLuong quytrinh = db.HRM_QuyetDinhDieuChinhLuong.Find(Id);
        //    dynamic data = new DynamicObject();
        //    var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
        //    var CongTy = BoPhans.Find(x => x.Id == quytrinh.IdCongTyEOS);
        //    var NguoiLaoDong = db.HRM_NhanVien.Find(quytrinh.IdNhanVien);
        //    File.Copy(fileName, sFileNameCopy, true);
        //    var doc = DocX.Load(sFileNameCopy);
        //    doc.ReplaceText("@CongTy", CongTy.TenBoPhan);
        //    if (quytrinh.SoQuyetDinh == null)
        //        quytrinh.SoQuyetDinh = "";
        //    string TenNguoiKy = db.HRM_NhanVien.Where(r => r.IdGuid == quytrinh.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
        //    doc.ReplaceText("@@TenNguoiKy", TenNguoiKy);
        //    doc.ReplaceText("@SoQuyetDinh ", quytrinh.SoQuyetDinh);

        //    doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
        //    var chucvuNhanVien = _CallWebService.GetChucVu(db, quytrinh.IdChucVuEOSNhanVien);
        //    var BoPhanNhanVien = BoPhans.Where(r => r.Id == quytrinh.IdBoPhanEOS).FirstOrDefault();
        //    doc.ReplaceText("@PhongBanDeNghi", BoPhanNhanVien.TenBoPhan);
        //    string BoPhanChucVu = "";
        //    if (BoPhanNhanVien != null)
        //        BoPhanChucVu = BoPhanNhanVien.TenBoPhan;
        //    if (chucvuNhanVien != null)
        //    {
        //        BoPhanChucVu = BoPhanChucVu + " - " + chucvuNhanVien.TenChucVu;
        //        doc.ReplaceText("@ChucVuNhanVien", chucvuNhanVien.TenChucVu);

        //    }
        //    else
        //    {
        //        doc.ReplaceText("@ChucVuNhanVien", "");
        //    }

        //    doc.ReplaceText("@BoPhanChucVu", BoPhanChucVu);
        //    doc.ReplaceText("@NgayHieuLuc", quytrinh.NgayHieuLuc.ToString("dd/MM/yyyy"));

        //    var listPhongBanLienquan = db.HRM_QuyetDinhDieuChinhLuong_BoPhan.Where(r => r.IdQuyetDinh == Id).ToList();
        //    string phongBanlienQuan = "";

        //    foreach (var item in listPhongBanLienquan)
        //    {
        //        var BoPhanItem = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
        //        phongBanlienQuan = phongBanlienQuan + BoPhanItem.TenBoPhan + "; ";
        //    }
        //    doc.ReplaceText("@phongbanlienquan", phongBanlienQuan);
        //    double SoTien = quytrinh.LuongHachToanSauDieuChinh - quytrinh.LuongHachToanHienTai;
        //    doc.ReplaceText("@SoTienSo", SoTien.ToString("N0"));

        //    string SoTienbangchu = ReadNum(SoTien);
        //    doc.ReplaceText("@SoTienChu", SoTienbangchu);
        //    string NgayThangNam = "ngày " + quytrinh.NgayKy.ToString("dd") + " tháng " + quytrinh.NgayKy.ToString("MM") + " năm " + quytrinh.NgayKy.ToString("yyyy");
        //    doc.ReplaceText("@ThangNam", NgayThangNam);

        //    doc.Save();
        //    data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_TangLuong_DaiAn.docx")
        //                    + "&path=" + Base64Encode(sFileNameCopy);

        //    return data;
        //}
        public dynamic DeleteQuyetDinhDieuChinhLuong(string Id, ApplicationDbContext db = null)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {

                try
                {
                    var item = db.HRM_QuyetDinhDieuChinhLuong.Where(x => x.IdGuid == Id).ToList();
                    db.HRM_QuyetDinhDieuChinhLuong.RemoveRange(item);


                    var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == Id && r.TableName == DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString()).ToList();
                    db.HRM_HopDongPhuCap.RemoveRange(lstPhuCap);
                    //var listBoPhans = db.HRM_QuyetDinhDieuChinhLuong_BoPhan.Where(r => r.IdQuyetDinh == Id).ToList();
                    //db.HRM_QuyetDinhDieuChinhLuong_BoPhan.RemoveRange(listBoPhans);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    LogErrorFormat("DeleteQuyetDinhDieuChinhLuong, Error {0}", ex.Message);
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                }

                //   var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString()).ToList();

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            }
        }
        public dynamic ChuyenTiepQuyetDinhDieuChinhLuong(HRM_QuyetDinhDieuChinhLuong QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    var listId = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var hopDongs = db.HRM_HopDong.AsNoTracking().Where(r => listId.Contains(r.IdNhanVien) && r.isKetThuc == true).GroupBy(a => a.IdNhanVien).Select(a => a.OrderByDescending(x => x.NgayHieuLuc).ThenByDescending(x => x.Modified).FirstOrDefault()).ToList();
                    if (hopDongs.Count != listId.Count)
                    {
                        var listIDCheck = hopDongs.Select(a => a.IdNhanVien).Distinct().ToList();
                        listId = listId.Where(a => !listIDCheck.Contains(a)).ToList();
                        var HopDong = db.HRM_NhanVien.AsNoTracking().Where(a => listId.Contains(a.IdGuid)).ToList();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên chưa có hợp đồng: " + string.Join(",", HopDong.Select(a => a.TenDayDu).ToList()));
                    }
                    HRM_QuyetDinhDieuChinhLuong pcuDatabase = db.HRM_QuyetDinhDieuChinhLuong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    bool Error = UpdateDieuChinhLuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }


                    if (QuyTrinh.isKetThuc == true)
                    {
                        var listNhanVien = QuyTrinh.listNhanVien.Where(a => a.TrangThai == 1).ToList();
                        List<HRM_GiamBaoHiem> listGiam = new List<HRM_GiamBaoHiem>();
                        List<HRM_TangBaoHiem> listTang = new List<HRM_TangBaoHiem>();
                        foreach (var NhanVien in listNhanVien)
                        {
                            LogLichSuNhanVien(db, NhanVien.IdNhanVien, QuyTrinh.IdGuid, "HRM_QuyetDinhDieuChinhLuong", DefineData.eAction.QUYTRINHDIEUCHINHLUONG.ToString(), QuyTrinh.NgayHieuLucUnix);
                            if (NhanVien.LuongHachToanSauDieuChinh > 0 && NhanVien.LuongHachToanHienTai != NhanVien.LuongHachToanSauDieuChinh)
                            {
                                if (NhanVien.LuongHachToanHienTai < NhanVien.LuongHachToanSauDieuChinh)
                                {
                                    HRM_TangBaoHiem tangBaoHiem = new HRM_TangBaoHiem();
                                    tangBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                    tangBaoHiem.IdNhanVien = NhanVien.IdNhanVien;
                                    tangBaoHiem.MucDong = NhanVien.LuongHachToanSauDieuChinh;
                                    tangBaoHiem.ThoiGianDong = QuyTrinh.NgayHieuLuc;
                                    tangBaoHiem.PhanTramThu = NhanVien.LuongHachToanSauDieuChinh * 10.5 / 100;
                                    tangBaoHiem.Loai = eLoaiBaoHiem.TangMoi.ToString();
                                    listTang.Add(tangBaoHiem);
                                }
                                else
                                {
                                    HRM_GiamBaoHiem GiamBaoHiem = new HRM_GiamBaoHiem();
                                    GiamBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                    GiamBaoHiem.IdNhanVien = NhanVien.IdNhanVien;
                                    GiamBaoHiem.MucDong = NhanVien.LuongHachToanSauDieuChinh;
                                    GiamBaoHiem.NgayGiam = QuyTrinh.NgayHieuLuc;
                                    GiamBaoHiem.Loai = eLoaiBaoHiem.GiamKhongLuong.ToString();
                                    listGiam.Add(GiamBaoHiem);
                                }
                            }

                            var dateTime = QuyTrinh.NgayHieuLuc.Value;
                            HRM_HopDong hopDong = hopDongs.Where(r => r.IdNhanVien == NhanVien.IdNhanVien /*&& r.TrangThai == "HieuLuc" */&& r.isKetThuc == true).OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
                            var hopDongAdd = new HRM_HopDong();
                            if (hopDong != null)
                            {
                                #region HopDong
                                var funcHop = _funcHopDong;
                                EntitySingleResponse<HRM_HopDong> hopdongcheck = funcHop.GetHopDong(hopDong.IdGuid, db);
                                hopDongAdd = hopdongcheck.Item.Clone();
                                hopDongAdd.IdHopDong = hopDong.IdHopDong;
                                if (string.IsNullOrEmpty(hopDong.IdHopDong))
                                    hopDongAdd.IdHopDong = hopDong.IdGuid;
                                hopDongAdd.isKetThuc = false;
                                hopDongAdd.SoHopDong = null;
                                hopDongAdd.IdLoaiHopDong = db.HRM_dmLoaiHopDong.AsNoTracking().Where(a => a.isPhuLuc == true && a.Ma == "DCL").Select(a => a.IdGuid).FirstOrDefault();
                                hopDongAdd.NgayKyUnix = QuyTrinh.NgayKyUnix;
                                //hopDongAdd.NgayKy = QuyTrinh.NgayKy.Value;
                                //hopDongAdd.NgayHieuLuc = QuyTrinh.NgayHieuLuc.Value;
                                hopDongAdd.IdCongTyEOS = QuyTrinh.IdCongTyEOS;
                                hopDongAdd.NgayHieuLucUnix = QuyTrinh.NgayHieuLucUnix;
                                hopDongAdd.LuongHopDong.LuongBaoHiem = NhanVien.LuongThucTeSauDieuChinh;
                                if (NhanVien.LoaiLuong != null && NhanVien.LoaiLuong > 0)
                                    hopDongAdd.LoaiLuong = NhanVien.LoaiLuong ?? 0;
                                hopDongAdd.LuongBaoHiem.LuongBaoHiem = NhanVien.LuongHachToanSauDieuChinh;
                                if (hopDongAdd.LuongBaoHiem.LuongBaoHiem > 0 && hopDongAdd.BaoHiem != null)
                                {
                                    hopDongAdd.LuongBaoHiem.isHienThiLenBangLuong = true;
                                    hopDongAdd.BaoHiem.isBaoHiemXaHoi = true;
                                    hopDongAdd.BaoHiem.isBaoHiemYTe = true;
                                }
                                if (NhanVien.lstPhuCap != null && NhanVien.lstPhuCap.Count > 0)
                                {
                                    hopDongAdd.lstPhuCap = new List<HRM_HopDongPhuCap>();
                                    foreach (var item in NhanVien.lstPhuCap)
                                    {
                                        item.IddmLoaiPhuCap = item.IddmLoaiPhuCap;
                                        item.SoTien = item.SoTienSauDieuChinh;
                                        item.NgayBatDau = hopDongAdd.NgayHieuLuc;
                                        item.NgayBatDauUnix = hopDongAdd.NgayHieuLucUnix;
                                        hopDongAdd.lstPhuCap.Add(item);
                                    }
                                }
                                bool check = funcHop.AddUpdateHopDong(hopDongAdd, db, true);
                                #endregion
                            }
                            List<HRM_Luong_NhanVien> luong_NhanVienOld = db.HRM_Luong_NhanVien.AsNoTracking().Where(r => r.IdNhanVien == NhanVien.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc > dateTime)).OrderBy(r => r.NgayHetHieuLuc).ToList();
                            if (luong_NhanVienOld != null && luong_NhanVienOld.Count > 0)
                                foreach (var item in luong_NhanVienOld)
                                {
                                    item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.Value.AddDays(-1);
                                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                }
                            HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien()
                            {
                                IdGuid = Guid.NewGuid().ToString(),
                                IdCongTyEOS = NhanVien.IdCongTyEOS ?? 0,
                                IdNhanVien = NhanVien.IdNhanVien,
                                IdBoPhanEOS = NhanVien.IdBoPhanEOS ?? 0,
                                Luong = NhanVien.LuongThucTeSauDieuChinh,
                                LoaiLuong = hopDongAdd?.LoaiLuong ?? 0,
                                KieuLuong = NhanVien.KieuLuong,
                                LuongHachToan = NhanVien.LuongHachToanSauDieuChinh > 0
        ? NhanVien.LuongHachToanSauDieuChinh
        : NhanVien.LuongHachToanHienTai,
                                NgayBatDauHieuLuc = NhanVien.NgayHieuLuc.Value,
                                IdNguonNhapLuong = NhanVien.IdGuid,
                                NguonNhapLuong = DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString()
                            };
                            //luong_NhanVien.LuongHachToan = LuongBH;
                            //luong_NhanVien.IdGuid = Guid.NewGuid().ToString();
                            db.HRM_Luong_NhanVien.Add(luong_NhanVien);

                            List<HRM_Luong_PhuCap_NhanVien> lstPhuCapdb = db.HRM_Luong_PhuCap_NhanVien.Where(r => r.IdNhanVien == NhanVien.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc > dateTime)).ToList();
                            foreach (var item in lstPhuCapdb)
                            {
                                item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.Value.AddDays(-1);
                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                            List<HRM_Luong_PhuCap_NhanVien> listPcluong = new List<HRM_Luong_PhuCap_NhanVien>();
                            foreach (var itempc in NhanVien.lstPhuCap)
                            {
                                listPcluong.Add(new HRM_Luong_PhuCap_NhanVien(NhanVien, itempc, luong_NhanVien.IdGuid, nameof(HRM_QuyetDinhDieuChinhLuong)));
                            }
                            db.HRM_Luong_PhuCap_NhanVien.AddRange(listPcluong);

                        }
                        if (listGiam != null && listGiam.Count > 0)
                        {
                            db.HRM_GiamBaoHiem.AddRange(listGiam);
                        }
                        if (listTang != null && listTang.Count > 0)
                        {
                            db.HRM_TangBaoHiem.AddRange(listTang);
                        }

                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic ChuyenTiepQuyetDinhDieuChinhLuongCheck(HRM_QuyetDinhDieuChinhLuong QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_QuyetDinhDieuChinhLuong pcuDatabase = db.HRM_QuyetDinhDieuChinhLuong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    bool Error = UpdateDieuChinhLuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    var listNhanVien = QuyTrinh.listNhanVien.Where(a => a.TrangThai == 1).ToList();
                    List<HRM_GiamBaoHiem> listGiam = new List<HRM_GiamBaoHiem>();
                    List<HRM_TangBaoHiem> listTang = new List<HRM_TangBaoHiem>();
                    foreach (var NhanVien in listNhanVien)
                    {

                        var dateTime = QuyTrinh.NgayHieuLuc.Value;
                        HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == NhanVien.IdNhanVien /*&& r.TrangThai == "HieuLuc" */&& r.isKetThuc == true).OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
                        var hopDongAdd = new HRM_HopDong();

                        List<HRM_Luong_NhanVien> luong_NhanVienOld = db.HRM_Luong_NhanVien.AsNoTracking().Where(r => r.IdNhanVien == NhanVien.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc > dateTime)).OrderBy(r => r.NgayHetHieuLuc).ToList();
                        foreach (var item in luong_NhanVienOld)
                        {
                            item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.Value.AddDays(-1);
                            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien()
                        {
                            IdGuid = Guid.NewGuid().ToString(),
                            IdCongTyEOS = NhanVien.IdCongTyEOS ?? 0,
                            IdNhanVien = NhanVien.IdNhanVien,
                            IdBoPhanEOS = NhanVien.IdBoPhanEOS ?? 0,
                            Luong = NhanVien.LuongThucTeSauDieuChinh,
                            LoaiLuong = hopDong?.LoaiLuong ?? 0,
                            KieuLuong = NhanVien.KieuLuong,
                            LuongHachToan = NhanVien.LuongHachToanSauDieuChinh > 0
    ? NhanVien.LuongHachToanSauDieuChinh
    : NhanVien.LuongHachToanHienTai,
                            NgayBatDauHieuLuc = NhanVien.NgayHieuLuc.Value,
                            IdNguonNhapLuong = NhanVien.IdGuid,
                            NguonNhapLuong = DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString()
                        };
                        //luong_NhanVien.LuongHachToan = LuongBH;
                        //luong_NhanVien.IdGuid = Guid.NewGuid().ToString();
                        db.HRM_Luong_NhanVien.Add(luong_NhanVien);

                        List<HRM_Luong_PhuCap_NhanVien> lstPhuCapdb = db.HRM_Luong_PhuCap_NhanVien.Where(r => r.IdNhanVien == NhanVien.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc > dateTime)).ToList();
                        foreach (var item in lstPhuCapdb)
                        {
                            item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.Value.AddDays(-1);
                            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        List<HRM_Luong_PhuCap_NhanVien> listPcluong = new List<HRM_Luong_PhuCap_NhanVien>();
                        foreach (var itempc in NhanVien.lstPhuCap)
                        {
                            listPcluong.Add(new HRM_Luong_PhuCap_NhanVien(NhanVien, itempc, luong_NhanVien.IdGuid, nameof(HRM_QuyetDinhDieuChinhLuong)));
                        }
                        db.HRM_Luong_PhuCap_NhanVien.AddRange(listPcluong);

                    }
                    db.SaveChanges();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "");

                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }

        public dynamic KhongDuyetQuyetDinhDieuChinhLuong(HRM_QuyetDinhDieuChinhLuong QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyetDinhDieuChinhLuong pcuDatabase = db.HRM_QuyetDinhDieuChinhLuong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = UpdateDieuChinhLuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    db.SaveChanges();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic ImportDieuChinhLuongNhanVien(ApplicationDbContext db, string FileName, List<long> listIdBoPhan)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            int countPC = listPhuCap.Count;
            List<HRM_QuyetDinhDieuChinhLuong_NhanVien> ListResult = new List<HRM_QuyetDinhDieuChinhLuong_NhanVien>();
            //if (ListData.Count > 0)
            //{
            //    ListResult = ListData.Where(a => listId.Contains(a.IdNhanVien)).ToList();
            //}
            var sheet1 = package.Workbook.Worksheets.First();

            for (int row = 5; row <= sheet1.Cells.End.Row; row++)
            {
                HRM_QuyetDinhDieuChinhLuong_NhanVien itemAdd = new HRM_QuyetDinhDieuChinhLuong_NhanVien();
                int ncol = 3;
                itemAdd.MaNhanVien = GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(itemAdd.MaNhanVien))
                    break;
                ncol = 7;
                itemAdd.LuongThucTeHienTai = GetCellToDouble(sheet1, row, ncol);
                ncol++;
                itemAdd.LuongThucTeSauDieuChinh = GetCellToDouble(sheet1, row, ncol);
                ncol++;

                itemAdd.lstPhuCap = new List<HRM_HopDongPhuCap>();
                for (int i = 0; i < countPC; i++)
                {
                    HRM_HopDongPhuCap PCAdd = new HRM_HopDongPhuCap();
                    string Ma = GetCell(sheet1, 2, ncol);
                    var PC = listPhuCap.Where(a => a.Ma == Ma).FirstOrDefault();
                    if (PC != null)
                    {
                        PCAdd.SoTienTruocDieuChinh = GetCellToDouble(sheet1, row, ncol);
                        PCAdd.SoTienSauDieuChinh = GetCellToDouble(sheet1, row, ncol + countPC);
                        PCAdd.IddmLoaiPhuCap = PC.IdGuid;
                        if (PCAdd.SoTienSauDieuChinh > 0 || PCAdd.SoTienTruocDieuChinh > 0)
                            itemAdd.lstPhuCap.Add(PCAdd);
                    }
                    ncol++;
                }
                ncol += countPC;
                ncol++;
                ncol++;
                ncol++;
                itemAdd.LyDo = GetCell(sheet1, row, ncol);
                ListResult.Add(itemAdd);
            }
            if (ListResult != null && ListResult.Count > 0)
            {
                List<string> listIdNhanVien = ListResult.Select(a => a.MaNhanVien).Distinct().ToList();
                var listNhanViendb = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.MaNhanVien)).ToList();
                List<long> listIdNVEOS = listNhanViendb.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in ListResult)
                {

                    var nhanVien = listNhanViendb.FirstOrDefault(a => a.MaNhanVien == item.MaNhanVien);
                    if (nhanVien != null)
                    {
                        item.IdNhanVien = nhanVien.IdGuid;
                        item.MaNhanVien = nhanVien.MaNhanVien;

                        item.TenNhanVien = nhanVien.TenDayDu;
                        if (listChucDanh != null && listChucDanh.Count > 0)
                        {
                            var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                            if (ChucDanh != null)
                                item.TenChucDanh = ChucDanh.Ten;
                        }
                        //if (!listIdBoPhan.Contains(nhanVien.IdBoPhanEOS))
                        //    continue;
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                        if (BoPhan != null)
                            item.TenBoPhan = BoPhan.TenBoPhan;
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdCongTyEOS);
                        if (CongTy != null)
                            item.TenCoSo = CongTy.TenBoPhan;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã nhân viên không tồn tại");
                }
            }


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", ListResult);
        }
        public dynamic ImportDieuChinhLuongNhanVienNew(ApplicationDbContext db, string FileName, List<long> listIdBoPhan)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            int countPC = listPhuCap.Count;
            var listDuAn = _CallWebService.GetDanhSachDuAn(db);
            List<HRM_QuyetDinhDieuChinhLuong_NhanVien> ListResult = new List<HRM_QuyetDinhDieuChinhLuong_NhanVien>();
            List<HRM_HopDongPhuCap> lstPhuCap = new List<HRM_HopDongPhuCap>();

            //if (ListData.Count > 0)
            //{
            //    ListResult = ListData.Where(a => listId.Contains(a.IdNhanVien)).ToList();
            //}
            var sheet1 = package.Workbook.Worksheets["Sheet1"];
            var sheetPC = package.Workbook.Worksheets["Phụ Cấp"];
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");

            err = dungchung.KiemTraMaTrungExcel(sheet1, err, 4, 3);
            if (err.StatusCode != (int)DefineData.eErrorMessage.CapNhatThanhCong)
                return err;
            for (int row = 4; row <= sheet1.Cells.End.Row; row++)
            {
                HRM_QuyetDinhDieuChinhLuong_NhanVien itemAdd = new HRM_QuyetDinhDieuChinhLuong_NhanVien();
                int ncol = 2;
                string MaCS = GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaCS))
                {
                    var cty = listDuAn.FirstOrDefault(a => a.MaBoPhan == MaCS);
                    if (cty != null)
                        itemAdd.IdCongTyEOS = cty.Id;
                }
                ncol++;
                itemAdd.MaNhanVien = GetCell(sheet1, row, ncol);

                if (string.IsNullOrEmpty(itemAdd.MaNhanVien))
                    break;
                ncol = 7;
                itemAdd.LuongThucTeHienTai = GetCellToDouble(sheet1, row, ncol);
                ncol++;
                itemAdd.LuongThucTeSauDieuChinh = GetCellToDouble(sheet1, row, ncol);
                ncol++;
                itemAdd.LuongHachToanHienTai = GetCellToDouble(sheet1, row, ncol);
                ncol++;
                itemAdd.LuongHachToanSauDieuChinh = GetCellToDouble(sheet1, row, ncol);
                ncol++;

                itemAdd.LoaiLuong = GetCellToInt(sheet1, row, ncol);
                ncol++;
                ncol++;
                itemAdd.LyDo = GetCell(sheet1, row, ncol);
                itemAdd.TrangThai = 1;
                ListResult.Add(itemAdd);
            }
            for (int row = 4; row <= sheetPC.Cells.End.Row; row++)
            {
                HRM_HopDongPhuCap itemAdd = new HRM_HopDongPhuCap();
                int ncol = 2;
                itemAdd.MaNhanVien = GetCell(sheetPC, row, ncol);
                if (string.IsNullOrEmpty(itemAdd.MaNhanVien))
                    break;
                ncol++;
                ncol++;
                string MaPC = GetCell(sheetPC, row, ncol);
                if (!string.IsNullOrEmpty(MaPC))
                {
                    var PC = listPhuCap.FirstOrDefault(a => a.Ma == MaPC);
                    if (PC != null)
                        itemAdd.IddmLoaiPhuCap = PC.IdGuid;
                }
                ncol++;
                ncol++;

                itemAdd.SoTien = GetCellToDouble(sheetPC, row, ncol);
                itemAdd.SoTienTruocDieuChinh = GetCellToDouble(sheetPC, row, ncol);
                ncol++;
                itemAdd.SoTienSauDieuChinh = GetCellToDouble(sheetPC, row, ncol);

                ncol++;
                itemAdd.LoaiPhuCap = GetCellToInt(sheetPC, row, ncol);

                lstPhuCap.Add(itemAdd);
            }

            if (ListResult != null && ListResult.Count > 0)
            {
                List<string> listIdNhanVien = ListResult.Select(a => a.MaNhanVien).Distinct().ToList();
                var listNhanViendb = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.MaNhanVien)).ToList();
                List<long> listIdNVEOS = listNhanViendb.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in ListResult)
                {
                    item.lstPhuCap = lstPhuCap.Where(a => a.MaNhanVien == item.MaNhanVien).ToList();
                    var nhanVien = listNhanViendb.FirstOrDefault(a => a.MaNhanVien == item.MaNhanVien);
                    if (nhanVien != null)
                    {
                        item.IdNhanVien = nhanVien.IdGuid;
                        item.MaNhanVien = nhanVien.MaNhanVien;

                        item.TenNhanVien = nhanVien.TenDayDu;
                        if (listChucDanh != null && listChucDanh.Count > 0)
                        {
                            var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                            if (ChucDanh != null)
                                item.TenChucDanh = ChucDanh.Ten;
                        }
                        //if (!listIdBoPhan.Contains(nhanVien.IdBoPhanEOS))
                        //    continue;
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                        if (BoPhan != null)
                            item.TenBoPhan = BoPhan.TenBoPhan;
                        var CongTy = listDuAn.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                            item.TenCoSo = CongTy.TenBoPhan;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã nhân viên không tồn tại " + item.MaNhanVien);
                }
            }


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", ListResult);
        }
        public dynamic ImportDieuChinhLuongNhanVienQT(ApplicationDbContext db, string FileName)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

                    string pathTemplate = PathUploads + FileName;
                    if (File.Exists(pathTemplate) == false)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
                    using Stream s = File.OpenRead(pathTemplate);
                    using ExcelPackage package = new ExcelPackage(s);
                    var listPhuCap = db.HRM_dmPhuCap.ToList();
                    int countPC = listPhuCap.Count;
                    var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                    List<HRM_QuyetDinhDieuChinhLuong_NhanVien> ListResult = new List<HRM_QuyetDinhDieuChinhLuong_NhanVien>();
                    List<HRM_HopDongPhuCap> lstPhuCap = new List<HRM_HopDongPhuCap>();

                    //if (ListData.Count > 0)
                    //{
                    //    ListResult = ListData.Where(a => listId.Contains(a.IdNhanVien)).ToList();
                    //}
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var sheet1 = package.Workbook.Worksheets["Sheet1"];
                    var sheetPC = package.Workbook.Worksheets["Phụ Cấp"];
                    err = dungchung.KiemTraMaTrungExcel(sheet1, err, 4, 3);
                    if (err.StatusCode != (int)DefineData.eErrorMessage.CapNhatThanhCong)
                        return err;
                    for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                    {
                        HRM_QuyetDinhDieuChinhLuong_NhanVien itemAdd = new HRM_QuyetDinhDieuChinhLuong_NhanVien();
                        int ncol = 2;
                        string MaCS = GetCell(sheet1, row, ncol);
                        if (!string.IsNullOrEmpty(MaCS))
                        {
                            var cty = listDuAn.FirstOrDefault(a => a.MaBoPhan == MaCS);
                            if (cty != null)
                                itemAdd.IdCongTyEOS = cty.Id;
                        }
                        ncol++;
                        itemAdd.MaNhanVien = GetCell(sheet1, row, ncol);

                        if (string.IsNullOrEmpty(itemAdd.MaNhanVien))
                            break;
                        ncol = 7;
                        itemAdd.LuongThucTeHienTai = GetCellToDouble(sheet1, row, ncol);
                        ncol++;
                        itemAdd.LuongThucTeSauDieuChinh = GetCellToDouble(sheet1, row, ncol);
                        ncol++;
                        itemAdd.LuongHachToanHienTai = GetCellToDouble(sheet1, row, ncol);
                        ncol++;
                        itemAdd.LuongHachToanSauDieuChinh = GetCellToDouble(sheet1, row, ncol);
                        ncol++;
                        itemAdd.LoaiLuong = GetCellToInt(sheet1, row, ncol);
                        ncol++;
                        ncol++;
                        itemAdd.LyDo = GetCell(sheet1, row, ncol);
                        itemAdd.TrangThai = 1;
                        ListResult.Add(itemAdd);
                    }
                    for (int row = 4; row <= sheetPC.Cells.End.Row; row++)
                    {
                        HRM_HopDongPhuCap itemAdd = new HRM_HopDongPhuCap();
                        int ncol = 2;
                        itemAdd.MaNhanVien = GetCell(sheetPC, row, ncol);
                        if (string.IsNullOrEmpty(itemAdd.MaNhanVien))
                            break;
                        ncol++;
                        ncol++;
                        string MaPC = GetCell(sheetPC, row, ncol);
                        if (!string.IsNullOrEmpty(MaPC))
                        {
                            var PC = listPhuCap.FirstOrDefault(a => a.Ma == MaPC);
                            if (PC != null)
                                itemAdd.IddmLoaiPhuCap = PC.IdGuid;
                        }
                        ncol++;
                        ncol++;

                        itemAdd.SoTienTruocDieuChinh = GetCellToDouble(sheetPC, row, ncol);
                        ncol++;
                        itemAdd.SoTienSauDieuChinh = GetCellToDouble(sheetPC, row, ncol);
                        ncol++;
                        itemAdd.LoaiPhuCap = GetCellToInt(sheetPC, row, ncol);

                        lstPhuCap.Add(itemAdd);
                    }

                    if (ListResult != null && ListResult.Count > 0)
                    {
                        List<string> listIdNhanVien = ListResult.Select(a => a.MaNhanVien).Distinct().ToList();
                        var listNhanViendb = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.MaNhanVien)).ToList();
                        List<long> listIdNVEOS = listNhanViendb.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                        var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                        var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                        foreach (var item in ListResult)
                        {
                            item.lstPhuCap = lstPhuCap.Where(a => a.MaNhanVien == item.MaNhanVien).ToList();
                            var nhanVien = listNhanViendb.FirstOrDefault(a => a.MaNhanVien == item.MaNhanVien);
                            if (nhanVien != null)
                            {
                                item.IdNhanVien = nhanVien.IdGuid;
                                item.MaNhanVien = nhanVien.MaNhanVien;
                                item.IdBoPhanEOS = nhanVien.IdBoPhanEOS;
                                item.IdChucVuEOS = nhanVien.IdChucVu;
                                item.TenNhanVien = nhanVien.TenDayDu;
                                if (listChucDanh != null && listChucDanh.Count > 0)
                                {
                                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                                    if (ChucDanh != null)
                                        item.TenChucDanh = ChucDanh.Ten;
                                }
                                //if (!listIdBoPhan.Contains(nhanVien.IdBoPhanEOS))
                                //    continue;
                                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                                if (BoPhan != null)
                                    item.TenBoPhan = BoPhan.TenBoPhan;
                                var CongTy = listDuAn.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                                if (CongTy != null)
                                    item.TenCoSo = CongTy.TenBoPhan;
                            }
                            else
                                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã nhân viên không tồn tại " + item.MaNhanVien);
                        }
                        var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                        var GS = listChucVu.Where(a => a.MaChucVu == "GS").FirstOrDefault();
                        var CN = listChucVu.Where(a => a.MaChucVu == "CN").FirstOrDefault();
                        // tạo quy trình điều chỉnh lương
                        List<long> listIdCty = ListResult.Select(a => a.IdCongTyEOS ?? 0).Distinct().ToList();
                        foreach (long Id in listIdCty)
                        {
                            {
                                HRM_QuyetDinhDieuChinhLuong entity = new HRM_QuyetDinhDieuChinhLuong();
                                entity.IdCongTyEOS = Id;
                                //entity.SoQuyTrinh = GetNextMaDieuChinhLuong(db);
                                string SQT = JsonConvert.SerializeObject(GetNextMaDieuChinhLuong(db));
                                entity.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoPhieu;
                                entity.listNhanVien = ListResult.Where(a => a.IdCongTyEOS == Id && a.IdChucVuEOS == GS.Id).ToList();
                                entity.eAction = DefineData.eAction.DIEUCHINHLUONGGIAMSAT.ToString();
                                if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                                    UpdateDieuChinhLuong(entity, db, err, eQuyTrinh_LoaiHam.Set);
                            }
                            {
                                HRM_QuyetDinhDieuChinhLuong entity = new HRM_QuyetDinhDieuChinhLuong();
                                entity.IdCongTyEOS = Id;
                                //entity.SoQuyTrinh = GetNextMaDieuChinhLuong(db);
                                string SQT = JsonConvert.SerializeObject(GetNextMaDieuChinhLuong(db));
                                entity.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoPhieu;
                                entity.listNhanVien = ListResult.Where(a => a.IdCongTyEOS == Id && a.IdChucVuEOS == CN.Id).ToList();
                                entity.eAction = DefineData.eAction.DIEUCHINHLUONGCONGNHAN.ToString();
                                if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                                    UpdateDieuChinhLuong(entity, db, err, eQuyTrinh_LoaiHam.Set);
                            }
                            {
                                HRM_QuyetDinhDieuChinhLuong entity = new HRM_QuyetDinhDieuChinhLuong();
                                entity.IdCongTyEOS = Id;
                                //entity.SoQuyTrinh = GetNextMaDieuChinhLuong(db);
                                string SQT = JsonConvert.SerializeObject(GetNextMaDieuChinhLuong(db));
                                entity.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoPhieu;
                                entity.listNhanVien = ListResult.Where(a => a.IdCongTyEOS == Id && a.IdChucVuEOS != GS.Id && a.IdChucVuEOS != CN.Id).ToList();
                                entity.eAction = DefineData.eAction.DIEUCHINHLUONGNHANVIEN.ToString();
                                if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                                    UpdateDieuChinhLuong(entity, db, err, eQuyTrinh_LoaiHam.Set);
                            }

                        }
                        db.SaveChanges();

                    }
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công");
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Import không thành công thành công " + e.ToString());
                }
            }
        }

        public dynamic ExportFileMauDieuChinhLuongNhanVien(string IdQuyTrinh, ApplicationDbContext db)
        {
            string sFile = "ImPortDieuChinhLuong.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "Danh sách nhân viên điều chỉnh lương.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            int col = 9;
            int countPC = listPhuCap.Count;
            sheet1.Cells[1, 1, 1, col + countPC * 2 - 1].Merge = true;
            // header
            sheet1.Cells[3, 9, 3, col + countPC - 1].Merge = true;

            foreach (var item in listPhuCap)
            {
                sheet1.Cells[2, col].Value = item.Ma;
                sheet1.Cells[4, col].Value = item.Ten;
                col++;
            }
            sheet1.Cells[3, col, 3, col + countPC - 1].Merge = true;
            sheet1.Cells[3, col].Value = "Phụ cấp sau điều chỉnh";
            foreach (var item in listPhuCap)
            {
                sheet1.Cells[2, col].Value = item.Ma;
                sheet1.Cells[4, col].Value = item.Ten;
                col++;
            }

            sheet1.Cells[3, col, 3, col + 1].Merge = true;
            sheet1.Cells[3, col].Value = "Tổng lương";

            sheet1.Cells[4, col].Value = "Trước điều chỉnh";
            col++;
            sheet1.Cells[4, col].Value = "Sau điều chỉnh";

            col++;
            sheet1.Cells[3, col, 4, col].Merge = true;
            sheet1.Cells[3, col].Value = "Chênh lệch tổng thu nhập sau điều chỉnh";
            col++;

            sheet1.Cells[3, col, 4, col].Merge = true;
            sheet1.Cells[3, col].Value = "Lý do điều chỉnh";

            int nRow = 5;
            if (!string.IsNullOrEmpty(IdQuyTrinh))
            {
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listNhanVienDC = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(a => a.IdQuyetDinh == IdQuyTrinh).OrderBy(a => a.Created).ToList();
                var listPhuCapdb = db.HRM_HopDongPhuCap.Where(a => a.IdQuyetDinh == IdQuyTrinh).ToList();
                if (listNhanVienDC != null && listNhanVienDC.Count > 0)
                {
                    List<string> listIdNhanVien = listNhanVienDC.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    int nCol = 1;
                    int STT = 1;
                    foreach (var item in listNhanVien)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;//2
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                            sheet1.Cells[nRow, nCol].Value = CongTy.TenBoPhan;
                        nCol++;//3
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;//4
                        sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                        nCol++;//5
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                            sheet1.Cells[nRow, nCol].Value = BoPhan.TenBoPhan;
                        nCol++;//6
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                            sheet1.Cells[nRow, nCol].Value = ChucDanh.Ten;
                        nCol++;//7
                        var NhanVien = listNhanVienDC.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LuongThucTeHienTai;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;//8
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LuongThucTeSauDieuChinh;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;//9
                        foreach (var PC in listPhuCap)
                        {
                            if (listPhuCapdb != null && listPhuCapdb.Count > 0)
                            {
                                var PhuCap = listPhuCapdb.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmLoaiPhuCap == PC.IdGuid);
                                if (PhuCap != null)
                                {
                                    sheet1.Cells[nRow, nCol].Value = PhuCap.SoTienTruocDieuChinh;
                                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                    sheet1.Cells[nRow, nCol + countPC].Value = PhuCap.SoTienSauDieuChinh;
                                    sheet1.Cells[nRow, nCol + countPC].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                }
                            }
                            nCol++;
                        }
                        nCol += countPC;
                        double TongTruoc = (NhanVien.LuongThucTeHienTai) + listPhuCapdb.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.SoTienTruocDieuChinh);
                        sheet1.Cells[nRow, nCol].Value = TongTruoc;
                        nCol++;
                        double TongSau = (NhanVien.LuongThucTeSauDieuChinh) + listPhuCapdb.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.SoTienSauDieuChinh);
                        sheet1.Cells[nRow, nCol].Value = TongSau;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = TongSau - TongTruoc;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LyDo;
                        nCol++;

                        nRow++;
                    }
                }
            }
            nRow--;
            DrawTableExcel(sheet1, 3, nRow, 1, col);
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            if (nRow > 5)
            {
                sheet1.Cells[5, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 4, nRow, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportFileMauDieuChinhLuongNhanVienNew(string IdQuyTrinh, ApplicationDbContext db)
        {
            string sFile = "ImPortDieuChinhLuong.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "Danh sách nhân viên điều chỉnh lương.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets["Sheet1"];
            var sheetPC = package.Workbook.Worksheets["Phụ Cấp"];
            var sheetdt = package.Workbook.Worksheets["definedata"];
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            int nRow = 3;
            foreach (var item in listPhuCap)
            {
                sheetdt.Cells[nRow, 1].Value = item.Ten;
                sheetdt.Cells[nRow, 2].Value = item.Ma;
                nRow++;
            }

            nRow = 4;
            int nCol = 1;
            if (!string.IsNullOrEmpty(IdQuyTrinh))
            {
                int nRowPc = 4;
                int nColPC = 1;
                int STTPC = 1;
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listNhanVienDC = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(a => a.IdQuyetDinh == IdQuyTrinh).OrderBy(a => a.Created).ToList();
                var listPhuCapdb = db.HRM_HopDongPhuCap.Where(a => a.IdQuyetDinh == IdQuyTrinh).ToList();

                if (listNhanVienDC != null && listNhanVienDC.Count > 0)
                {
                    List<string> listIdNhanVien = listNhanVienDC.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

                    int STT = 1;
                    foreach (var item in listNhanVien)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;//2
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                            sheet1.Cells[nRow, nCol].Value = CongTy.TenBoPhan;
                        nCol++;//3
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;//4
                        sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                        nCol++;//5
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                            sheet1.Cells[nRow, nCol].Value = BoPhan.TenBoPhan;
                        nCol++;//6
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                            sheet1.Cells[nRow, nCol].Value = ChucDanh.Ten;
                        nCol++;//7
                        var NhanVien = listNhanVienDC.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LuongThucTeHienTai;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;//8
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LuongThucTeSauDieuChinh;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;//9
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LuongHachToanHienTai;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;//8
                        sheet1.Cells[nRow, nCol].Value = NhanVien.LuongHachToanSauDieuChinh;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;//9

                        nCol++;//9
                        string TenLoaiLuong = "Theo Tháng";
                        if (NhanVien.LoaiLuong == 2)
                            TenLoaiLuong = "Theo ngày công";
                        else if (NhanVien.LoaiLuong == 3)
                            TenLoaiLuong = "Theo Giờ làm việc";
                        sheet1.Cells[nRow, nCol].Value = TenLoaiLuong;
                        nCol++;//9

                        sheet1.Cells[nRow, nCol].Value = NhanVien.LyDo;

                        STT++;
                        nRow++;

                        // phụ cấp
                        var listPCNV = listPhuCapdb.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        if (listPCNV != null && listPCNV.Count > 0)
                        {
                            foreach (var itemPC in listPCNV)
                            {
                                nColPC = 1;
                                sheetPC.Cells[nRowPc, nColPC].Value = STTPC;
                                nColPC++;
                                sheetPC.Cells[nRowPc, nColPC].Value = item.MaNhanVien;
                                nColPC++;
                                sheetPC.Cells[nRowPc, nColPC].Value = item.TenDayDu;
                                nColPC++;
                                var PC = listPhuCap.FirstOrDefault(a => a.IdGuid == itemPC.IddmLoaiPhuCap);
                                if (PC == null)
                                    PC = new HRM_dmPhuCap();

                                nColPC++;
                                sheetPC.Cells[nRowPc, nColPC].Value = PC.Ten;
                                nColPC++;
                                sheetPC.Cells[nRowPc, nColPC].Value = itemPC.SoTienTruocDieuChinh;
                                sheetPC.Cells[nRowPc, nColPC].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;

                                nColPC++;
                                sheetPC.Cells[nRowPc, nColPC].Value = itemPC.SoTienSauDieuChinh;
                                sheetPC.Cells[nRowPc, nColPC].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                nColPC++;
                                nColPC++;
                                TenLoaiLuong = "Phụ cấp theo tháng";
                                if (itemPC.LoaiPhuCap == 2)
                                    TenLoaiLuong = "Phụ cấp theo ngày";
                                else if (itemPC.LoaiPhuCap == 3)
                                    TenLoaiLuong = "Phụ cấp theo giờ";
                                else if (itemPC.LoaiPhuCap == 4)
                                    TenLoaiLuong = "Phụ cấp theo năm";
                                sheetPC.Cells[nRowPc, nColPC].Value = TenLoaiLuong;
                                STTPC++;
                                nRowPc++;
                            }
                        }
                    }
                }
                DrawTableExcel(sheetPC, 3, nRowPc, 1, nColPC);
                sheetPC.Cells.Style.Font.Name = "Times New Roman";
                sheetPC.Cells.Style.Font.Size = 12;
                sheetPC.Cells.Style.WrapText = true;
                sheetPC.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheetPC.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                if (nRow > 4)
                {
                    sheetPC.Cells[4, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetPC.Cells[4, 5, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }
            }
            nRow--;
            DrawTableExcel(sheet1, 3, nRow, 1, nCol);
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            if (nRow > 4)
            {
                sheet1.Cells[4, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 4, nRow, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        public dynamic GetMucLuongHienTailistNhanVien(List<string> listId, List<HRM_QuyetDinhDieuChinhLuong_NhanVien> ListData, ApplicationDbContext db)
        {
            var listNhanVien = db.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).Distinct().ToList();
            List<HRM_QuyetDinhDieuChinhLuong_NhanVien> ListResult = new List<HRM_QuyetDinhDieuChinhLuong_NhanVien>();
            if (ListData.Count > 0)
            {
                ListResult = ListData.Where(a => listId.Contains(a.IdNhanVien)).ToList();
                List<string> listIdNVDaCo = ListResult.Select(a => a.IdNhanVien).Distinct().ToList();
                listNhanVien = listNhanVien.Where(a => !listIdNVDaCo.Contains(a.IdGuid)).Distinct().ToList();
            }
            List<HRM_Luong_NhanVien> listluongNhanVien = db.HRM_Luong_NhanVien.OrderByDescending(cr => cr.NgayBatDauHieuLuc).ThenByDescending(a => a.Modified).Where(r => listId.Contains(r.IdNhanVien)).ToList();
            var listIds = listluongNhanVien.Select(a => a.IdNguonNhapLuong).Distinct().ToList();
            var listPhuCap = (from r in db.HRM_Luong_PhuCap_NhanVien
                              where listIds.Contains(r.IdNguonNhapLuong)
                              //group r by new { r.IdNhanVien, r.IddmLoaiPhuCap } into g
                              //select g.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault()
                              select r
                              ).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);

            foreach (var nhanvien in listNhanVien)
            {
                HRM_QuyetDinhDieuChinhLuong_NhanVien itemAdd = new HRM_QuyetDinhDieuChinhLuong_NhanVien();
                itemAdd.MaNhanVien = nhanvien.MaNhanVien;
                itemAdd.TenNhanVien = nhanvien.TenDayDu;
                itemAdd.IdNhanVien = nhanvien.IdGuid;
                itemAdd.TrangThai = 1;
                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanvien.IdBoPhanEOS);
                if (BoPhan != null)
                    itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                var CongTy = listBoPhan.FirstOrDefault(a => a.Id == nhanvien.IdCongTyEOS);
                if (CongTy != null)
                    itemAdd.TenCoSo = CongTy.TenBoPhan;

                var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanvien.IdChucDanh);
                if (ChucDanh != null)
                    itemAdd.TenChucDanh = ChucDanh.Ten;
                var LuongNhanVien = listluongNhanVien.FirstOrDefault(a => a.IdNhanVien == nhanvien.IdGuid);
                if (LuongNhanVien != null)
                {
                    itemAdd.LuongHachToanHienTai = LuongNhanVien.LuongHachToan;
                    itemAdd.LuongHachToanSauDieuChinh = LuongNhanVien.LuongHachToan;
                    itemAdd.LuongThucTeHienTai = LuongNhanVien.Luong;
                    itemAdd.LuongThucTeSauDieuChinh = LuongNhanVien.Luong;
                    itemAdd.LoaiLuong = LuongNhanVien.LoaiLuong;

                    itemAdd.lstPhuCap = new List<HRM_HopDongPhuCap>();
                    var listPCNV = listPhuCap.Where(a => a.IdNguonNhapLuong == LuongNhanVien.IdNguonNhapLuong).ToList();
                    if (listPCNV != null && listPCNV.Count > 0)
                    {
                        foreach (var PC in listPCNV)
                        {
                            var PCAdd = new HRM_HopDongPhuCap()
                            {
                                SoTien = PC.SoTien,
                                SoTienTruocDieuChinh = PC.SoTien,
                                SoTienSauDieuChinh = PC.SoTien,
                                IddmLoaiPhuCap = PC.IddmLoaiPhuCap,
                                LoaiPhuCap = PC.LoaiPhuCap,
                            };
                            itemAdd.lstPhuCap.Add(PCAdd);
                        }
                    }
                }
                ListResult.Add(itemAdd);
            }
            return ListResult;
        }
        //public string FuncSetUpdatePhuLucHopDong(HRM_QuyetDinhDieuChinhLuong model, ApplicationDbContext db)
        //{
        //    try
        //    {
        //        HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.TrangThai == "HieuLuc").FirstOrDefault();
        //        HRM_Luong_HopDong LuongHopDong = db.HRM_Luong_HopDong.AsNoTracking().FirstOrDefault(r => r.IdHopDong == hopDong.IdGuid);
        //        HRM_HopDong PLuc = hopDong;

        //        string IdHopDong = hopDong.IdGuid;
        //        int CountPL = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.IdHopDong == IdHopDong && r.isPhuLuc == true).Count();
        //        CountPL = CountPL + 1;
        //        PLuc.SoHopDong = "PL-" + CountPL + "-" + hopDong.SoHopDong;
        //        PLuc.IdHopDong = hopDong.IdGuid;
        //        PLuc.isPhuLuc = true;
        //        PLuc.IdGuid = Guid.NewGuid().ToString();
        //        PLuc.NgayKy = model.NgayKy.Value;
        //        PLuc.NgayHieuLuc = model.NgayHieuLuc.Value;
        //        //PLuc.NguoiDaiDienIdChucVu = model.IdChucVuNguoiKy;
        //        PLuc.IdNguoiDaiDien = model.IdNguoiKy;

        //        CallWebService _CallWebService = _CallWebService;
        //        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHHOPDONG.ToString(), PLuc.IdGuid);
        //        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
        //        {
        //            return "";
        //        }

        //        PLuc.IdTrangThai = trangThai.IdTrangThai;
        //        PLuc.TenTrangThai = trangThai.TenTrangThai;
        //        db.HRM_HopDong.Add(PLuc);

        //        HRM_Luong_HopDong LuongPL = new HRM_Luong_HopDong
        //        {
        //            IddmHinhThucTraLuong = LuongHopDong.IddmHinhThucTraLuong,
        //            IdHopDong = PLuc.IdGuid,
        //            IdNhanVien = PLuc.IdNhanVien,
        //            IdQuyetDinh = model.IdGuid,
        //            SoTaiKhoan = LuongHopDong.SoTaiKhoan,
        //            TenNganHang = LuongHopDong.TenNganHang,
        //            TyLeHuongLuong = LuongHopDong.TyLeHuongLuong,
        //            HinhThucLuong = LuongHopDong.HinhThucLuong,
        //            IdGuid = Guid.NewGuid().ToString(),
        //            LuongBaoHiem = model.LuongHachToanSauDieuChinh,
        //        };
        //        db.HRM_Luong_HopDong.Add(LuongPL);
        //        List<HRM_HopDongPhuCap> lstPhuCapPL = new List<HRM_HopDongPhuCap>();
        //        foreach (var item in model.lstPhuCap)
        //        {
        //            HRM_HopDongPhuCap PhuCap = new HRM_HopDongPhuCap
        //            {
        //                IdGuid = Guid.NewGuid().ToString(),
        //                IddmLoaiPhuCap = item.IddmLoaiPhuCap,
        //                IdHopDong = PLuc.IdGuid,
        //                SoTien = item.SoTien,
        //                LoaiPhuCap = item.LoaiPhuCap,
        //                NgayBatDau = item.NgayBatDau,
        //                NgayKetThuc = item.NgayKetThuc,
        //                TableName = "HRM_HopDong"
        //            };
        //            lstPhuCapPL.Add(PhuCap);
        //        }
        //        db.HRM_HopDongPhuCap.AddRange(lstPhuCapPL);

        //        HRM_HopDong_QuyetDinh quyetDinh = new HRM_HopDong_QuyetDinh
        //        {
        //            IdGuid = Guid.NewGuid().ToString(),
        //            IdHopDong = PLuc.IdGuid,
        //            IdQuyetDinh = model.IdGuid,
        //            TableName = "HRM_QuyetDinhDieuChinhLuong",
        //            MaQuyTrinh = DefineData.eAction.QUYTRINHDIEUCHINHLUONG.ToString()
        //        };
        //        db.HRM_HopDong_QuyetDinh.Add(quyetDinh);

        //        List<HRM_HopDongTaiSanBanGiao> HopDongTaiSanBanGiaos = db.HRM_HopDongTaiSanBanGiao.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).ToList();
        //        foreach (var item in HopDongTaiSanBanGiaos)
        //        {
        //            HRM_HopDongTaiSanBanGiao taiSanPL = item;
        //            taiSanPL.IdHopDong = PLuc.IdGuid;
        //            taiSanPL.IdGuid = Guid.NewGuid().ToString();
        //            db.HRM_HopDongTaiSanBanGiao.Add(taiSanPL);
        //        }
        //        HRM_BaoHiem_HopDong BaoHiemOld = db.HRM_BaoHiem_HopDong.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).FirstOrDefault();
        //        if (BaoHiemOld != null)
        //        {
        //            HRM_BaoHiem_HopDong BaoHiemPL = BaoHiemOld;
        //            BaoHiemPL.IdGuid = Guid.NewGuid().ToString();
        //            BaoHiemPL.IdHopDong = PLuc.IdGuid;
        //            db.HRM_BaoHiem_HopDong.Add(BaoHiemPL);
        //        }
        //        List<HRM_HopDongCongViec> listMoTaCongViecOld = db.HRM_HopDongCongViec.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).ToList();
        //        if (listMoTaCongViecOld.Count > 0)
        //        {
        //            List<HRM_HopDongCongViec> listMoTaCongViecPL = new List<HRM_HopDongCongViec>();
        //            foreach (var item in listMoTaCongViecOld)
        //            {
        //                HRM_HopDongCongViec CVPL = item;
        //                CVPL.IdGuid = Guid.NewGuid().ToString();
        //                CVPL.IdHopDong = PLuc.IdGuid;
        //                listMoTaCongViecPL.Add(CVPL);
        //            }
        //            db.HRM_HopDongCongViec.AddRange(listMoTaCongViecOld);
        //        }

        //        db.SaveChanges();
        //        return PLuc.IdGuid;

        //    }
        //    catch (Exception)
        //    {

        //        return "";
        //    }

        //}
        public dynamic GetTongHopDieuChinhLuong(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, string KeyWord)
        {
            try
            {
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                HRM_TongHopDieuChinhLuong result = new HRM_TongHopDieuChinhLuong();

                var listData = (from b in db.HRM_QuyetDinhDieuChinhLuong_NhanVien
                                join a in db.HRM_QuyetDinhDieuChinhLuong on b.IdQuyetDinh equals a.IdGuid
                                where a.isKetThuc == true && b.TrangThai == 1

                                select b
                                 ).ToList();

                if (listIdNhanVien != null && listIdNhanVien.Count > 0)
                {
                    listData = listData.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                }
                if (listData != null && listData.Count > 0)
                {
                    listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    if (!string.IsNullOrEmpty(KeyWord))
                    {
                        listNhanVien = listNhanVien.Where(b => b.TenDayDu.ToLower().Contains(KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    }
                    if (IdCongTy > 0)
                    {
                        listNhanVien = listNhanVien.Where(b => b.IdCongTyEOS == IdCongTy).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    }
                    if (listIdBoPhan != null && listIdBoPhan.Count > 0)
                    {
                        listNhanVien = listNhanVien.Where(b => listIdBoPhan.Contains(b.IdBoPhanEOS)).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    }
                    var listQuyTrinh_BoPhan_1 = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
                    var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isKetThuc == true && a.TrangThai == "HieuLuc").ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                    var listLuong = db.HRM_Luong_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                    var listPhuCapHopDong = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                    List<string> listIdDieuChinh = listData.Select(a => a.IdGuid).Distinct().ToList();
                    var listPhuCapDieuChinh = db.HRM_HopDongPhuCap.Where(a => listIdDieuChinh.Contains(a.IdQuyetDinh)).ToList();
                    var listdmPhuCap = db.HRM_dmPhuCap.ToList();
                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    result.listNhanVien = new List<HRM_TongHopDieuChinhLuong_NhanVien>();
                    result.label = 0;
                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_TongHopDieuChinhLuong_NhanVien itemAdd = new HRM_TongHopDieuChinhLuong_NhanVien();


                        var data = listData.Where(a => a.IdNhanVien == NhanVien.IdGuid).OrderBy(a => a.Modified).ToList();
                        if (data.Count > result.label)
                            result.label = data.Count;
                        // Start Duyệt mới thêm
                        var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == NhanVien.IdGuid).Select(x => x.IdBoPhan);
                        var _listBoPhan = listCongTy.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                        NhanVien.TenCongTy = string.Join("; ", _listBoPhan);

                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                        itemAdd.TenCoSo = NhanVien.TenCongTy;
                        itemAdd.MaNhanVien = NhanVien.MaNhanVien;
                        itemAdd.TenNhanVien = NhanVien.TenDayDu;
                        itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.NgayBatDauLamViec = NhanVien.NgayBatDauLamViec;
                        if (itemAdd.NgayBatDauLamViec != null)
                        {
                            var time = DateTime.Now - itemAdd.NgayBatDauLamViec.Value;
                            itemAdd.ThamNien = time.TotalDays / 30.5;
                        }
                        itemAdd.listChiTiet = new List<HRM_TongHopDieuChinhLuong_ChiTiet>();
                        // lương bắt đầu
                        var Hopdong = listHopDong.Where(a => a.IdNhanVien == NhanVien.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                        HRM_TongHopDieuChinhLuong_ChiTiet LuongBandau = new HRM_TongHopDieuChinhLuong_ChiTiet();
                        if (Hopdong != null)
                        {
                            LuongBandau.Ngay = Hopdong.NgayHieuLuc;
                            var LuongHd = listLuong.FirstOrDefault(a => a.IdHopDong == Hopdong.IdGuid);
                            if (LuongHd != null)
                                LuongBandau.Luong = LuongHd.LuongBaoHiem;
                            var PhuCapHDs = listPhuCapHopDong.Where(a => a.IdHopDong == Hopdong.IdGuid).ToList();
                            if (PhuCapHDs != null && PhuCapHDs.Count() > 0)
                            {
                                List<string> IdPhuCapHd = PhuCapHDs.Select(a => a.IddmLoaiPhuCap).ToList();
                                var Phucaps = listdmPhuCap.Where(a => IdPhuCapHd.Contains(a.IdGuid)).ToList();
                                if (Phucaps != null && Phucaps.Count() > 0)
                                    LuongBandau.PhuCap = PhuCapHDs.Sum(a => a.SoTien);
                            }
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == Hopdong.IdThoiGianLamViec);
                            if (CaLamViec != null)
                            {

                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCong = "P";
                                else
                                    itemAdd.LoaiCong = "F";

                            }
                        }
                        itemAdd.listChiTiet.Add(LuongBandau);
                        foreach (var item in data)
                        {
                            HRM_TongHopDieuChinhLuong_ChiTiet DieuChinh = new HRM_TongHopDieuChinhLuong_ChiTiet();
                            DieuChinh.Ngay = item.NgayHieuLuc;
                            DieuChinh.Luong = item.LuongThucTeSauDieuChinh;
                            var PhuCapHDs = listPhuCapDieuChinh.Where(a => a.IdQuyetDinh == item.IdGuid).ToList();
                            if (PhuCapHDs != null && PhuCapHDs.Count() > 0)
                            {
                                List<string> IdPhuCapHd = PhuCapHDs.Select(a => a.IddmLoaiPhuCap).ToList();
                                var Phucaps = listdmPhuCap.Where(a => IdPhuCapHd.Contains(a.IdGuid)).ToList();
                                if (Phucaps != null && Phucaps.Count() > 0)
                                    DieuChinh.PhuCap = PhuCapHDs.Sum(a => a.SoTienSauDieuChinh);
                            }
                            DieuChinh.LyDo = item.LyDo;
                            itemAdd.listChiTiet.Add(DieuChinh);
                        }
                        result.listNhanVien.Add(itemAdd);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic ExportTongHopDieuChinhLuong(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, string keyWord)
        {

            try
            {
                HRM_TongHopDieuChinhLuong data = GetTongHopDieuChinhLuong(db, IdCongTy, listIdBoPhan, keyWord);

                string sFile = "TongHopDieuChinhLuong.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = sFile;
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                #region header
                // header
                int nRow = 3;
                int nCol = 12;
                int STT = 1;

                for (int i = 1; i <= data.label; i++)
                {
                    sheet1.Cells[nRow, nCol, nRow, nCol + 3].Merge = true;
                    sheet1.Cells[nRow, nCol].Value = "Điều chỉnh lần " + i;
                    sheet1.Cells[nRow + 1, nCol].Value = "Lương";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "Các loại PC";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "Ngày";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "Lý do điều chỉnh";
                    nCol++;

                }
                nCol--;
                #endregion
                dungchung.DrawTableExcel(sheet1, 3, 4, 1, nCol);
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells[1, 1].Value = "TỔNG HỢP ĐIỀU CHỈNH LƯƠNG";
                sheet1.Cells[1, 1].Style.Font.Bold = true;
                nRow = 5;
                if (data != null && data.listNhanVien != null && data.listNhanVien.Count > 0)
                {

                    foreach (var item in data.listNhanVien)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenCoSo;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;
                        if (item.NgayBatDauLamViec != null)
                            sheet1.Cells[nRow, nCol].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThamNien;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        nCol++;

                        if (item.listChiTiet != null && item.listChiTiet.Count > 0)
                        {
                            item.listChiTiet = item.listChiTiet.OrderBy(a => a.Ngay).ToList();
                            foreach (var Ngay in item.listChiTiet)
                            {
                                sheet1.Cells[nRow, nCol].Value = Ngay.Luong;
                                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                                nCol++;
                                sheet1.Cells[nRow, nCol].Value = Ngay.PhuCap;
                                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                                nCol++;
                                if (Ngay.Ngay != null)
                                    sheet1.Cells[nRow, nCol].Value = Ngay.Ngay.Value.ToString("dd/MM/yyyy");
                                nCol++;
                                sheet1.Cells[nRow, nCol].Value = Ngay.LyDo;
                                nCol++;
                            }
                        }
                        nRow++;
                        STT++;

                    }
                }
                // tổng
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (nRow > 4)
                {
                    nRow--;
                    dungchung.DrawTableExcel(sheet1, 3, nRow, 1, 11 + data.label * 4);
                    sheet1.Cells[3, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[3, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                // chữ ký
                nRow += 4;
                sheet1.Cells[nRow, 3, nRow, 6].Merge = true;
                sheet1.Cells[nRow, 3].Value = "Quản lý cơ sở";
                sheet1.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                sheet1.Cells[nRow + 1, 3].Value = "(Ký, ghi rõ họ tên)";


                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic ExportQuyetDinhDieuChinhLuong(string Id, ApplicationDbContext db)
        {
            List<string> results = new List<string>();
            var quytrinh = db.HRM_QuyetDinhDieuChinhLuong.FirstOrDefault(x => x.IdGuid == Id);
            if (quytrinh == null) return results;
            var listNhanVien = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(r => r.IdQuyetDinh == quytrinh.IdGuid).ToList();
            if (listNhanVien == null) return results;
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            foreach (var item in listNhanVien)
            {

                var CongTy = listCongTy.Find(x => x.Id == quytrinh.IdCongTyEOS);
                var NguoiLaoDong = db.HRM_NhanVien.Find(item.IdNhanVien);
                if (CongTy == null || NguoiLaoDong == null) continue;
                var filenamegui = DateTime.Now.ToOADate();
                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_DieuChinhLuong.docx");
                string sFileNameCopy = PathUploads + "QD_DieuChinhLuong_" + NguoiLaoDong.TenDayDu + "_" + quytrinh.NoiDung + "_" + filenamegui + ".docx";
                dynamic data = new DynamicObject();
                File.Copy(fileName, sFileNameCopy, true);
                var doc = DocX.Load(sFileNameCopy);
                string NgayThang = "ngày " + quytrinh.NgayKy?.Day + " tháng " + quytrinh.NgayKy?.Month + " năm " + quytrinh.NgayKy?.Year;
                string Nam = "năm " + quytrinh.NgayKy?.Year;
                doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                doc.ReplaceText("@Nam", Nam);
                doc.ReplaceText("@TENCONGTY", CongTy.TenBoPhan.ToUpper());
                doc.ReplaceText("@NoiDung", quytrinh.NoiDung);
                doc.ReplaceText("@NgayThang", NgayThang);
                doc.ReplaceText("@NgayQuyetDinh", quytrinh.NgayHieuLuc?.ToString("dd/MM/yyyy"));
                doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
                var ChucVu = _CallWebService.GetChucDanh(db, NguoiLaoDong.IdChucDanh);
                var BoPhan = _CallWebService.GetBoPhan(db, NguoiLaoDong.IdBoPhanEOS);
                string TenChucVu = "";
                if (ChucVu != null)
                    TenChucVu = ChucVu.Ten;
                if (BoPhan != null)
                    TenChucVu = TenChucVu + " - " + BoPhan.TenBoPhan;
                doc.ReplaceText("@ChucVu", TenChucVu);
                var luongHienTai = item.LuongThucTeHienTai;
                var luongSauDieuChinh = item.LuongThucTeSauDieuChinh;
                var phantramtang = ((luongSauDieuChinh - luongHienTai) / luongHienTai) * 100;
                var phantramgiam = ((luongHienTai - luongSauDieuChinh) / luongHienTai) * 100;
                if (luongHienTai < luongSauDieuChinh)
                {
                    doc.ReplaceText("@TangGiam", "Tăng");
                    doc.ReplaceText("@PhanTram", "Tăng " + phantramtang.ToString() + "%");
                }
                else
                {
                    doc.ReplaceText("@TangGiam", "Giảm");
                    doc.ReplaceText("@PhanTram", "Giảm " + phantramgiam.ToString() + "%");
                }
                doc.Save();
                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_DieuChinhLuong.docx")
                                + "&path=" + Base64Encode(sFileNameCopy);
                //return data;
                results.Add(data.TenFile);
            }
            return results;
        }
        public dynamic ExporQuyDinhDieuChinhLuongThang(long IdCongTy, int Nam, int Thang, ApplicationDbContext db)
        {
            string sFile = "DieuChinhBaoHiemXaHoiThang.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            if (Host == "PHT")
                sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "DieuChinhBaoHiemXaHoiThang.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var DieuChinhLuongs = db.HRM_QuyetDinhDieuChinhLuong.Where(x => x.isKetThuc == true && x.NgayHieuLuc.Value.Year == Nam && x.NgayHieuLuc.Value.Month == Thang).ToList();
            sheet1.Cells[3, 1].Value = "ĐIỀU CHỈNH MỨC ĐÓNG BHXH TỪ THÁNG " + Thang + "/" + Nam;
            if (DieuChinhLuongs != null && DieuChinhLuongs.Count > 0)
            {
                List<string> listId = DieuChinhLuongs.Select(a => a.IdGuid).Distinct().ToList();
                var listNhanVienQD = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(a => listId.Contains(a.IdQuyetDinh) && a.TrangThai == 1).ToList();
                var IdNvs = listNhanVienQD.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => IdNvs.Contains(a.IdGuid)).ToList();
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listPhuCapQD = db.HRM_HopDongPhuCap.Where(a => listId.Contains(a.IdQuyetDinh)).ToList();
                var IdPCs = listPhuCapQD.Select(a => a.IddmLoaiPhuCap).Distinct().ToList();
                var listPhuCap = db.HRM_dmPhuCap.Where(a => IdPCs.Contains(a.IdGuid)).OrderBy(a => a.SapXep).ToList();
                #region header
                DateTime thangTruoc = new DateTime(Nam, Thang, 1, 0, 0, 0).AddMonths(-1);
                int CountPc = listPhuCap.Count;
                if (CountPc == 0)
                    CountPc = 1;
                sheet1.Cells[6, 5].Value = "Mức đóng tháng " + thangTruoc.Month + "/" + thangTruoc.Year;

                sheet1.Cells[6, 5, 6, 8 + CountPc].Merge = true;
                sheet1.Cells[7, 7, 7, 6 + CountPc].Merge = true;

                int nCol = 7;
                foreach (var item in listPhuCap)
                {
                    sheet1.Cells[8, nCol].Value = item.Ten;
                    sheet1.Column(nCol).Width = 14;
                    sheet1.Cells[8, nCol + 4 + CountPc].Value = item.Ten;
                    sheet1.Column(nCol + 4 + CountPc).Width = 14;
                    nCol++;
                }
                if (listPhuCap.Count == 0)
                    nCol++;
                sheet1.Cells[7, nCol].Value = "Mức đóng đã bao gồm các khoản phụ cấp";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 20;
                nCol++;
                sheet1.Cells[7, nCol].Value = "Vùng";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 14;
                nCol++;

                // điều chỉnh
                sheet1.Cells[6, nCol].Value = "Mức đóng tháng " + Thang + "/" + Nam;
                sheet1.Cells[6, nCol, 6, nCol + CountPc + 3].Merge = true;
                //sheet1.Cells[7, nCol].Value = true;
                //nCol++;
                sheet1.Cells[7, nCol].Value = "Mức đóng chưa bao gồm phụ cấp";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 20;
                nCol++;
                sheet1.Cells[7, nCol].Value = "Bậc";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 14;
                nCol++;
                sheet1.Cells[7, nCol].Value = "Mức phụ cấp";
                sheet1.Cells[7, nCol, 7, nCol - 1 + CountPc].Merge = true;


                nCol = nCol + CountPc;
                sheet1.Cells[7, nCol].Value = "Mức đóng đã bao gồm các khoản phụ cấp";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 20;
                nCol++;
                sheet1.Cells[7, nCol].Value = "Vùng";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 14;
                nCol++;
                sheet1.Cells[6, nCol].Value = "Lý do";
                sheet1.Cells[6, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 14;
                nCol++;
                sheet1.Cells[6, nCol].Value = "Ghi chú";
                sheet1.Cells[6, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 14;
                sheet1.Cells[3, 1, 8, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[3, 1, 8, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[6, 1, 8, nCol].Style.Font.Bold = true;

                #endregion
                #region data
                int nRow = 9;
                int STT = 1;
                foreach (var item in listNhanVienQD)
                {
                    var nhanvien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.TenDayDu;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.NgaySinh.ToString("dd/MM/yyyy");
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listBoPhan.Where(a => a.Id == nhanvien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.LuongHachToanHienTai;
                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[nRow, nCol + 4 + CountPc].Value = item.LuongHachToanSauDieuChinh;
                    sheet1.Cells[nRow, nCol + 4 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol + 4 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    nCol++;
                    nCol++;
                    foreach (var Pc in listPhuCap)
                    {
                        var PhuCap = listPhuCapQD.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien && a.IddmLoaiPhuCap == Pc.IdGuid);
                        if (PhuCap != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = PhuCap.SoTienTruocDieuChinh;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            sheet1.Cells[nRow, nCol + 4 + CountPc].Value = PhuCap.SoTienSauDieuChinh;
                            sheet1.Cells[nRow, nCol + 4 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol + 4 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        }

                        nCol++;
                    }
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listPhuCapQD.Where(a => a.IdNhanVien == item.IdNhanVien).Sum(a => a.SoTienTruocDieuChinh) + item.LuongHachToanHienTai;
                    sheet1.Cells[nRow, nCol + 4 + CountPc].Value = listPhuCapQD.Where(a => a.IdNhanVien == item.IdNhanVien).Sum(a => a.SoTien) + item.LuongHachToanSauDieuChinh;
                    nCol = nCol + 4 + CountPc + 1;
                    sheet1.Cells[nRow, nCol].Value = item.LyDo;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    STT++;
                    nRow++;
                }
                nRow--;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells[3, 1, 5, nCol + 2].Merge = true;
                DrawTableExcel(sheet1, 6, nRow, 1, nCol + 2);
                #endregion

                #region chân ký
                nRow += 4;
                sheet1.Cells[nRow, 1, nRow, 4].Merge = true;
                sheet1.Cells[nRow, 1].Value = "Phê duyệt TGĐ";
                sheet1.Cells[nRow, 5, nRow, 8].Merge = true;
                sheet1.Cells[nRow, 5].Value = "Phê duyệt P.TGĐ";
                sheet1.Cells[nRow, 9, nRow, 12].Merge = true;
                sheet1.Cells[nRow, 9].Value = "Phụ trách nhân sự";
                sheet1.Cells[nRow, 13, nRow, 15].Merge = true;
                sheet1.Cells[nRow, 13].Value = "Người lập";
                sheet1.Cells[nRow, 1, nRow, 13].Style.Font.Bold = true;

                #endregion
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExporTongHopDieuChinhLuongThang(long IdCongTy, int Nam, int Thang, ApplicationDbContext db)
        {
            string sFile = "TongHopBaoHiemXaHoiThang.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            if (Host == "PHT")
                sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "TongHopDieuChinhLuongThang.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var DieuChinhLuongs = db.HRM_QuyetDinhDieuChinhLuong.Where(x =>/* x.IdCongTyEOS == IdCongTy && */x.isKetThuc == true && x.NgayHieuLuc.Value.Year == Nam && x.NgayHieuLuc.Value.Month == Thang).ToList();
            var BoNhiems = db.HRM_BoNhiemChucVu.Where(x => /*x.IdCongTyEOS == IdCongTy &&*/ x.isKetThuc == true && x.NgayBatDau.Year == Nam && x.NgayBatDau.Month == Thang).ToList();
            var DieuDongs = db.HRM_BoNhiemDieuDongDuAn.Where(x => /*(x.IdCongTyEOS == IdCongTy || x.IdCongTyEOSNew == IdCongTy) &&*/ x.isKetThuc == true && x.NgayHieuLuc.Value.Year == Nam && x.NgayHieuLuc.Value.Month == Thang).ToList();
            sheet1.Cells[3, 1].Value = "BÁO CÁO ĐIỀU CHỈNH LƯƠNG THÁNG " + Thang + "/" + Nam;
            if ((DieuChinhLuongs != null && DieuChinhLuongs.Count > 0) || (BoNhiems != null && BoNhiems.Count > 0) || (DieuDongs != null && DieuDongs.Count > 0))
            {
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);

                var IdNvs = new List<string>();
                var IdPCs = new List<string>();
                List<string> listId = DieuChinhLuongs.Select(a => a.IdGuid).Distinct().ToList();
                var listNhanVienDCL = db.HRM_QuyetDinhDieuChinhLuong_NhanVien.Where(a => listId.Contains(a.IdQuyetDinh) && a.TrangThai == 1).ToList();
                var listPhuCapDCL = db.HRM_HopDongPhuCap.Where(a => listId.Contains(a.IdQuyetDinh)).ToList();
                if (listNhanVienDCL != null && listNhanVienDCL.Count > 0)
                    IdNvs.AddRange(listNhanVienDCL.Select(a => a.IdNhanVien).Distinct().ToList());
                if (listPhuCapDCL != null && listPhuCapDCL.Count > 0)
                    IdPCs.AddRange(listPhuCapDCL.Select(a => a.IddmLoaiPhuCap).Distinct().ToList());

                listId = BoNhiems.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapBN = db.HRM_HopDongPhuCap.Where(a => listId.Contains(a.IdQuyetDinh)).ToList();
                if (BoNhiems != null && BoNhiems.Count > 0)
                    IdNvs.AddRange(BoNhiems.Select(a => a.IdNhanVien).Distinct().ToList());
                if (listPhuCapBN != null && listPhuCapBN.Count > 0)
                    IdPCs.AddRange(listPhuCapBN.Select(a => a.IddmLoaiPhuCap).Distinct().ToList());

                listId = DieuDongs.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapDD = db.HRM_HopDongPhuCap.Where(a => listId.Contains(a.IdQuyetDinh)).ToList();
                if (DieuDongs != null && DieuDongs.Count > 0)
                    IdNvs.AddRange(DieuDongs.Select(a => a.IdNhanVien).Distinct().ToList());
                if (listPhuCapDD != null && listPhuCapDD.Count > 0)
                    IdPCs.AddRange(listPhuCapDD.Select(a => a.IddmLoaiPhuCap).Distinct().ToList());



                var listNhanVien = db.HRM_NhanVien.Where(a => IdNvs.Contains(a.IdGuid)).ToList();

                var listPhuCap = db.HRM_dmPhuCap.Where(a => IdPCs.Contains(a.IdGuid)).OrderBy(a => a.SapXep).ToList();
                #region header
                DateTime thangTruoc = new DateTime(Nam, Thang, 1, 0, 0, 0).AddMonths(-1);
                int CountPc = listPhuCap.Count;
                if (CountPc == 0)
                    CountPc = 1;
                sheet1.Cells[6, 5].Value = "Mức đóng tháng " + thangTruoc.Month + "/" + thangTruoc.Year;

                sheet1.Cells[6, 5, 6, 5 + CountPc].Merge = true;
                sheet1.Cells[7, 6, 7, 5 + CountPc].Merge = true;

                int nCol = 6;
                foreach (var item in listPhuCap)
                {
                    sheet1.Cells[8, nCol].Value = item.Ten;
                    sheet1.Column(nCol).Width = 14;
                    sheet1.Cells[8, nCol + 1 + CountPc].Value = item.Ten;
                    sheet1.Column(nCol + 1 + CountPc).Width = 14;
                    nCol++;
                }
                if (listPhuCap.Count == 0)
                    nCol++;
                //sheet1.Cells[7, nCol].Value = "Lương hợp đồng";
                //sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                //sheet1.Column(nCol).Width = 20;
                //nCol++;
                //sheet1.Cells[7, nCol].Value = "Vùng";
                //sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                //sheet1.Column(nCol).Width = 14;
                //nCol++;

                // điều chỉnh
                sheet1.Cells[6, nCol].Value = "Mức đóng tháng " + Thang + "/" + Nam;
                sheet1.Cells[6, nCol, 6, nCol + CountPc].Merge = true;
                //sheet1.Cells[7, nCol].Value = true;
                //nCol++;
                sheet1.Cells[7, nCol].Value = "Lương hợp đồng";
                sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 20;
                //nCol++;
                //sheet1.Cells[7, nCol].Value = "Bậc";
                //sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                //sheet1.Column(nCol).Width = 14;
                nCol++;
                sheet1.Cells[7, nCol].Value = "Mức phụ cấp";
                sheet1.Cells[7, nCol, 7, nCol - 1 + CountPc].Merge = true;
                nCol = nCol + CountPc-1;
                //sheet1.Cells[7, nCol].Value = "Mức đóng đã bao gồm các khoản phụ cấp";
                //sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                //sheet1.Column(nCol).Width = 20;
                //nCol++;
                //sheet1.Cells[7, nCol].Value = "Vùng";
                //sheet1.Cells[7, nCol, 8, nCol].Merge = true;
                //sheet1.Column(nCol).Width = 14;
                nCol++;
                sheet1.Cells[6, nCol].Value = "Lý do";
                sheet1.Cells[6, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 25;
                nCol++;
                sheet1.Cells[6, nCol].Value = "Ghi chú";
                sheet1.Cells[6, nCol, 8, nCol].Merge = true;
                sheet1.Column(nCol).Width = 20;
                sheet1.Cells[3, 1, 8, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[3, 1, 8, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[6, 1, 8, nCol].Style.Font.Bold = true;

                #endregion
                #region data
                int nRow = 9;
                int STT = 1;
                foreach (var item in listNhanVienDCL)
                {
                    var nhanvien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.TenDayDu;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.NgaySinh.ToString("dd/MM/yyyy");
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listBoPhan.Where(a => a.Id == nhanvien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.LuongThucTeHienTai;
                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Value = item.LuongThucTeSauDieuChinh;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                 
                    nCol++;
                    foreach (var Pc in listPhuCap)
                    {
                        var PhuCap = listPhuCapDCL.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien && a.IddmLoaiPhuCap == Pc.IdGuid);
                        if (PhuCap != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = PhuCap.SoTienTruocDieuChinh;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Value = PhuCap.SoTienSauDieuChinh;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        }

                        nCol++;
                    }
                    nCol++;
                    nCol = nCol  + CountPc ;
                    sheet1.Cells[nRow, nCol].Value = "Điều chỉnh lương: " + nhanvien.TenDayDu + " - " + item.LyDo;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    STT++;
                    nRow++;
                }
                foreach (var item in BoNhiems)
                {
                    var nhanvien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.TenDayDu;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.NgaySinh.ToString("dd/MM/yyyy");
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listBoPhan.Where(a => a.Id == nhanvien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.LuongHachToanHienTai;
                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Value = item.LuongHachToanSauDieuChinh;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    nCol++;
                    foreach (var Pc in listPhuCap)
                    {
                        var PhuCap = listPhuCapBN.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien && a.IddmLoaiPhuCap == Pc.IdGuid);
                        if (PhuCap != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = PhuCap.SoTienTruocDieuChinh;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Value = PhuCap.SoTienSauDieuChinh;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        }

                        nCol++;
                    }
                    nCol++;
                    nCol = nCol  + CountPc ;
                    var chucVu = listChucVu.Where(a => a.Id == item.IdChucVuEOSCu).Select(a => a.TenChucVu).FirstOrDefault();
                    var chucVuMoi = listChucVu.Where(a => a.Id == item.IdChucVuEOS).Select(a => a.TenChucVu).FirstOrDefault();
                    var chucDanh = listChucDanh.Where(a => a.Id == item.IdChucDanhEOSCu).Select(a => a.Ten).FirstOrDefault();
                    var chucDanhMoi = listChucDanh.Where(a => a.Id == item.IdChucDanhEOS).Select(a => a.Ten).FirstOrDefault();

                    sheet1.Cells[nRow, nCol].Value = "Bổ nhiệm " + nhanvien.TenDayDu + " từ chức vụ " + chucVu + ", chức danh " + chucDanh + " --> chức vụ " + chucVuMoi + ", chức danh " + chucDanhMoi;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    STT++;
                    nRow++;
                }
                foreach (var item in DieuDongs)
                {
                    var nhanvien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.TenDayDu;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = nhanvien.NgaySinh.ToString("dd/MM/yyyy");
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listBoPhan.Where(a => a.Id == nhanvien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.LuongHachToanHienTai;
                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Value = item.LuongHachToanSauDieuChinh;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol + 1 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    nCol++;
                    foreach (var Pc in listPhuCap)
                    {
                        var PhuCap = listPhuCapDD.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien && a.IddmLoaiPhuCap == Pc.IdGuid);
                        if (PhuCap != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = PhuCap.SoTienTruocDieuChinh;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Value = PhuCap.SoTienSauDieuChinh;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol + 1 + CountPc].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        }

                        nCol++;
                    }
                    nCol++;
                     nCol = nCol  + CountPc ;
                    var DonVi = listCongTy.Where(a => a.Id == item.IdCongTyEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    var DonViMoi = listCongTy.Where(a => a.Id == item.IdCongTyEOSNew).Select(a => a.TenBoPhan).FirstOrDefault();
                    var BoPhan = listBoPhan.Where(a => a.Id == item.IdBoPhanEOSOld).Select(a => a.TenBoPhan).FirstOrDefault();
                    var BoPhanMoi = listBoPhan.Where(a => a.Id == item.IdBoPhanEOSNew).Select(a => a.TenBoPhan).FirstOrDefault();

                    sheet1.Cells[nRow, nCol].Value = "Điều động " + nhanvien.TenDayDu + " từ đơn vị " + DonVi + ", bộ phận " + BoPhan + " --> đơn vị " + DonViMoi + ", bộ phận " + BoPhanMoi;

                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    STT++;
                    nRow++;
                }

                nRow--;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells[3, 1, 5, nCol ].Merge = true;
                DrawTableExcel(sheet1, 6, nRow, 1, nCol );
                #endregion

                #region chân ký
                nRow += 4;
                sheet1.Cells[nRow, 1, nRow, 4].Merge = true;
                sheet1.Cells[nRow, 1].Value = "Phê duyệt TGĐ";
                sheet1.Cells[nRow, 5, nRow, 8].Merge = true;
                sheet1.Cells[nRow, 5].Value = "Phê duyệt P.TGĐ";
                sheet1.Cells[nRow, 9, nRow, 12].Merge = true;
                sheet1.Cells[nRow, 9].Value = "Phụ trách nhân sự";
                sheet1.Cells[nRow, 13, nRow, 15].Merge = true;
                sheet1.Cells[nRow, 13].Value = "Người lập";
                sheet1.Cells[nRow, 1, nRow, 13].Style.Font.Bold = true;

                #endregion
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

    }
}