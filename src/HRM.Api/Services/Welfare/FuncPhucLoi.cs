#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HRM.Api.Services.PhucLoi
{
    public class FuncPhucLoi : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncPhucLoi(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncPhucLoi> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        public dynamic GetNextKeHoachPhucLoi(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_KeHoachPhucLoi.Select(x => x.SoPhieu), DefineData.MaKEHOACHPHUCLOI));
        public dynamic GetListKeHoachPhucLoi(dynamic dyn, ApplicationDbContext db)
        {

            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_KeHoachPhucLoi> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_KeHoachPhucLoi
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_KeHoachPhucLoi
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
                    result = result.Where(a => a.NgayLap >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayLap < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoPhieu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (string.IsNullOrEmpty(a.Ten) || a.Ten.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);

                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();

                foreach (var item in itemList)
                {
                    //item.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDeNghi);
                    //item.TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == item.IdTongHopDeNghiTuyenDung).FirstOrDefault();
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra " + e.Message);
            }
        }
        public dynamic SetKeHoachPhucLoi(HRM_KeHoachPhucLoi model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    var QuyTrinhdb = db.HRM_KeHoachPhucLoi.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (QuyTrinhdb != null && QuyTrinhdb.IdTrangThai != model.IdTrangThai)
                    {
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = model.IdGuid,
                            eAction = DefineData.eAction.QUYTRINHKEHOACHPHUCLOI.ToString(),
                            IdTrangThai = model.IdTrangThai,
                        };
                        var trangThai = _CallWebService.DoiTrangThaiKhoiTao(audit, db);
                    }
                    string err = FuncKeHoachPhucLoi(model, db);
                    if (err != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, err);
                    }

                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);


                }
                catch (Exception exx)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
                }
            }
        }
        public dynamic ChuyenTiepKeHoachPhucLoi(HRM_KeHoachPhucLoi model, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_KeHoachPhucLoi pcuDatabase = db.HRM_KeHoachPhucLoi.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }
                    var TrangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.QUYTRINHKEHOACHPHUCLOI.ToString(), model.IdTrangThai, model.IdGuid);
                    if (string.IsNullOrEmpty(TrangThai.IdTrangThai))
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, TrangThai.TenTrangThai);
                    }
                    model.IdTrangThai = TrangThai.IdTrangThai;
                    model.TenTrangThai = TrangThai.TenTrangThai;
                    if (TrangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                        model.isKetThuc = true;

                    string Error = FuncKeHoachPhucLoi(model, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, model.IdGuid);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetKeHoachPhucLoi(HRM_KeHoachPhucLoi QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_KeHoachPhucLoi pcuDatabase = db.HRM_KeHoachPhucLoi.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }

                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHKEHOACHPHUCLOI.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                    }
                    QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    string Error = FuncKeHoachPhucLoi(QuyTrinh, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }



                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic GetByIdKeHoachPhucLoi(string Id, ApplicationDbContext db)
        {
            HRM_KeHoachPhucLoi model = db.HRM_KeHoachPhucLoi.Find(Id);
            model.NgayLapUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayLap);
            var listChiTiet = db.HRM_KeHoachPhucLoiChiTiet.Where(r => r.IdKeHoachPhucLoi == Id).ToList();
            foreach (var item in listChiTiet)
            {
                item.HanDangKyUnix = dungchung.DateTimeToUnixTimeStamp(item.HanDangKy);
                item.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDau);
                item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                item.lstNhanVien = db.HRM_KeHachPhucLoi_NhanVien.Where(r => r.IdKeHachPhucLoiChiTiet == item.IdGuid).ToList();
            }

            model.lstChiTiet = listChiTiet;
            return model;
        }

        public dynamic DeleteKeHoachPhucLoi(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_KeHoachPhucLoi model = db.HRM_KeHoachPhucLoi.Find(Id);
                var listChiTiet = db.HRM_KeHoachPhucLoiChiTiet.Where(r => r.IdKeHoachPhucLoi == Id).ToList();
               
                var lstNhanVien = db.HRM_KeHachPhucLoi_NhanVien.Where(r => r.IdKeHoachPhucLoi == Id).ToList();
                db.HRM_KeHachPhucLoi_NhanVien.RemoveRange(lstNhanVien);
                db.HRM_KeHoachPhucLoiChiTiet.RemoveRange(listChiTiet);
                db.HRM_KeHoachPhucLoi.Remove(model);
                

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public string FuncKeHoachPhucLoi(HRM_KeHoachPhucLoi model, ApplicationDbContext db)
        {
            try
            {
                model.NgayLap = dungchung.UnixTimeStampToDateTime(model.NgayLapUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHKEHOACHPHUCLOI.ToString(), model.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        return "Không tìm thấy trạng thái dữ liệu khi chuyển";
                    }

                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;



                    db.HRM_KeHoachPhucLoi.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var listChiTiet = db.HRM_KeHoachPhucLoiChiTiet.Where(r => r.IdKeHoachPhucLoi == model.IdGuid).ToList();

                var lstNhanVien = db.HRM_KeHachPhucLoi_NhanVien.Where(r => r.IdKeHoachPhucLoi == model.IdGuid).ToList();
                db.HRM_KeHachPhucLoi_NhanVien.RemoveRange(lstNhanVien);
                db.HRM_KeHoachPhucLoiChiTiet.RemoveRange(listChiTiet);
                if (model.lstChiTiet != null)
                {
                    List<HRM_KeHachPhucLoi_NhanVien> listNhanVienAdd = new List<HRM_KeHachPhucLoi_NhanVien>();
                    foreach (var item in model.lstChiTiet)
                    {
                        item.HanDangKy = dungchung.UnixTimeStampToDateTime(item.HanDangKyUnix);
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);
                       

                        
                            item.IdKeHoachPhucLoi = model.IdGuid;
                            

                                item.IdGuid = Guid.NewGuid().ToString();
                                db.HRM_KeHoachPhucLoiChiTiet.Add(item);
                            

                            foreach (var itemCT in item.lstNhanVien)
                            {
                               
                                    itemCT.IdKeHoachPhucLoi = model.IdGuid;
                                    itemCT.IdKeHachPhucLoiChiTiet = item.IdGuid;

                                    itemCT.isTinhLuong = item.isTinhLuong;
                                    itemCT.IdGuid = Guid.NewGuid().ToString();
                                    //db.HRM_KeHachPhucLoi_NhanVien.Add(itemCT);
                                    listNhanVienAdd.Add(itemCT);
                                
                            }

                        
                    }
                    db.HRM_KeHachPhucLoi_NhanVien.AddRange(listNhanVienAdd);
                }
                db.SaveChanges();

                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }

        }
        public dynamic GetDanhSachNhanVienPhucLoi(long IdCongTyEOS, double NgayPhucLoiUnix, long IdChucVu, string IddmLoaiHopDong, string GioiTinh, ApplicationDbContext db)
        {
            string IdQuanHeGiaDinh_Con = db.HRM_dmQuanHeGiaDinh.Where(r => r.Ma == "CON").Select(r => r.IdGuid).FirstOrDefault();

            var NhanViens = db.HRM_NhanVien.Where(r => r.TrangThai == "DangLamViec" && (GioiTinh == "" || GioiTinh == null || r.IdGioiTinh == GioiTinh)).ToList();
            List<string> listIdNhanVien = NhanViens.Select(r => r.IdGuid).ToList();
            DateTime DateNow = DateTime.Now;
            //var nhanvienpb = db.HRM_Link_NhanVien_BoPhan_ChucVu.Where(r => (r.NgayKetThuc== null || r.NgayKetThuc > DateNow) && listIdNhanVien.Contains(r.IdNhanVien) && r.IdCongTyEOS == IdCongTyEOS).ToList();
            //listIdNhanVien = nhanvienpb.Select(r => r.IdNhanVien).ToList();
            //var NhanViens = lstNhanViens.Where(r => listIdNhanVien.Contains(r.IdGuid)).ToList();
            DateTime dateTimeNow = DateTime.Now;
            if (NgayPhucLoiUnix > 0)
                dateTimeNow = dungchung.UnixTimeStampToDateTime(NgayPhucLoiUnix);
            //  List<HRM_HopDong> listHopDong = db.HRM_HopDong.Where(r => listIdNhanVien.Contains(r.IdNhanVien)  && r.TrangThai= "HieuLuc" && r.IdCongTyEOS == IdCongTyEOS).ToList();
            List<HRM_HopDong> listHopDong = db.HRM_HopDong.Where(r => listIdNhanVien.Contains(r.IdNhanVien) && (IddmLoaiHopDong == null || IddmLoaiHopDong == "" || r.IdLoaiHopDong == IddmLoaiHopDong) && r.isHuongPhucLoi == true && (r.NgayHetHan == null || r.NgayHetHan > dateTimeNow) && r.TrangThai == "HieuLuc"/* && r.IdCongTyEOS == IdCongTyEOS*/).ToList();

            var NhanVienNguoiThans = db.HRM_NhanVienQuanHeGiaDinh.Where(r => listIdNhanVien.Contains(r.IdNhanVien) && r.IdQuanHe == IdQuanHeGiaDinh_Con).ToList();
            List<DanhSachNhanVienHuongPhucLoiViewModel> lstNhanVienHuongPhucLoi = new List<DanhSachNhanVienHuongPhucLoiViewModel>();
            var DanhMucChucVuEOS = _CallWebService.GetDanhSachChucVu(db);
            var DanhMucBoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
            foreach (var item in NhanViens)
            {
                var HopDongNV = listHopDong.Where(r => r.IdNhanVien == item.IdGuid).OrderByDescending(r => r.NgayHieuLuc).FirstOrDefault();
                if (HopDongNV == null)
                    continue;
                DanhSachNhanVienHuongPhucLoiViewModel nhanvien = new DanhSachNhanVienHuongPhucLoiViewModel
                {
                    IdNhanVien = item.IdGuid,
                    TenNhanVien = item.TenDayDu,
                    MaNhanVien = item.MaNhanVien,
                    IdChucVuEOS = HopDongNV.IddmChucVu,
                    IdBoPhanEOS = HopDongNV.IdBoPhanEOS,
                    IddmLoaiHopDong = HopDongNV.IdLoaiHopDong,
                    TenLoaiHopDong = dmLoaiHopDong.Where(r => r.IdGuid == HopDongNV.IdLoaiHopDong).Select(r => r.Ten).FirstOrDefault(),
                };
                if (NgayPhucLoiUnix > 0)
                {
                    DateTime NgaySinhSau = dateTimeNow.AddYears(-16);
                    nhanvien.SoCon = NhanVienNguoiThans.Where(r =>r.NgaySinh!=null && r.NgaySinh.Value.Year > NgaySinhSau.Year && r.IdNhanVien!=null && r.IdNhanVien == item.IdGuid).Count();
                }
                nhanvien.TenBoPhan = DanhMucBoPhans.Where(r => r.Id == nhanvien.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                nhanvien.TenChucVu = DanhMucChucVuEOS.Where(r => r.Id == nhanvien.IdChucVuEOS).Select(r => r.TenChucVu).FirstOrDefault();
                lstNhanVienHuongPhucLoi.Add(nhanvien);
            }
            lstNhanVienHuongPhucLoi = lstNhanVienHuongPhucLoi.Where(r => (IdChucVu == 0 || r.IdChucVuEOS == IdChucVu)).ToList();
            return lstNhanVienHuongPhucLoi;
        }
        public dynamic GetKeHoachPhucLoiDaDuyetByNam(long IdCongTyEOS, int Nam, ApplicationDbContext db)
        {
            List<HRM_KeHoachPhucLoi> keHoachNams = db.HRM_KeHoachPhucLoi.Where(r => r.IdCongTyEOS == IdCongTyEOS && r.Nam == Nam && r.isKetThuc == true).ToList();
            List<HRM_KeHoachPhucLoi> listRetunr = new List<HRM_KeHoachPhucLoi>();
            foreach (var item in keHoachNams)
            {
                var lstChiTiet = db.HRM_KeHoachPhucLoiChiTiet.Where(r => r.IdKeHoachPhucLoi == item.IdGuid).ToList();
                foreach (var itemCT in lstChiTiet)
                {
                    itemCT.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(itemCT.NgayBatDau);
                    itemCT.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(itemCT.NgayKetThuc);
                    itemCT.HanDangKyUnix = dungchung.DateTimeToUnixTimeStamp(itemCT.HanDangKy);
                }
                item.lstChiTiet = lstChiTiet;
                var listChiTietFalse = lstChiTiet.Where(r => r.isDaSuDung == false).ToList();
                if (listChiTietFalse.Count > 0)
                    listRetunr.Add(item);
            }
            return listRetunr;
        }
        public dynamic GetChiTietKeHoachPhucLoi(ApplicationDbContext db, string Id)
        {
            var model = db.HRM_KeHoachPhucLoiChiTiet.Where(r => r.IdGuid == Id).FirstOrDefault();
            model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
            model.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThuc);
            model.HanDangKyUnix = dungchung.DateTimeToUnixTimeStamp(model.HanDangKy);
            model.lstNhanVien = db.HRM_KeHachPhucLoi_NhanVien.Where(r => r.IdKeHachPhucLoiChiTiet == Id).ToList();
            return model;
        }
        #region Phuc Loi 
        public dynamic GetNextQuyTrinhPhucLoi(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyTrinh_PhucLoi.Select(x => x.SoPhieu), DefineData.MaQUYTRINHPHUCLOI));
        public dynamic GetListQuyTrinhPhucLoi(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_QuyTrinh_PhucLoi> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyTrinh_PhucLoi
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinh_PhucLoi
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
                    result = result.Where(a => a.NgayLap >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayLap < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoPhieu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (string.IsNullOrEmpty(a.Ten) || a.Ten.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);

                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();


                foreach (var item in itemList)
                {
                    //item.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDeNghi);
                    //item.TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == item.IdTongHopDeNghiTuyenDung).FirstOrDefault();
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra " + e.Message);
            }
        }
        public dynamic SetQuyTrinhPhucLoi(HRM_QuyTrinh_PhucLoi model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                { 
                    var QuyTrinhdb = db.HRM_QuyTrinh_PhucLoi.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (QuyTrinhdb != null && QuyTrinhdb.IdTrangThai != model.IdTrangThai)
                    {
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = model.IdGuid,
                            eAction = DefineData.eAction.QUYTRINHPHUCLOI.ToString(),
                            IdTrangThai = model.IdTrangThai,
                        };
                        var trangThai = _CallWebService.DoiTrangThaiKhoiTao(audit, db);
                    }
                    string err = FuncQuyTrinhPhucLoi(model, db);
                    if (err != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, err);
                    }
                    if (string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHPHUCLOI.ToString(), model.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        }

                        model.IdTrangThai = trangThai.IdTrangThai;
                        model.TenTrangThai = trangThai.TenTrangThai;
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);


                }
                catch (Exception exx)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhPhucLoi(HRM_QuyTrinh_PhucLoi model, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinh_PhucLoi pcuDatabase = db.HRM_QuyTrinh_PhucLoi.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }

                    string Error = FuncQuyTrinhPhucLoi(model, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, model.IdGuid);
                    }
                    var TrangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.QUYTRINHPHUCLOI.ToString(), model.IdTrangThai, model.IdGuid);
                    if (string.IsNullOrEmpty(TrangThai.IdTrangThai))
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, TrangThai.TenTrangThai);
                    }

                    model.IdTrangThai = TrangThai.IdTrangThai;
                    model.TenTrangThai = TrangThai.TenTrangThai;
                    if (TrangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                    {
                        model.isKetThuc = true;
                        foreach (var item in model.lstChiTiet)
                        {
                            HRM_KeHoachPhucLoiChiTiet chiTiet = db.HRM_KeHoachPhucLoiChiTiet.Find(item.IdChiTietKeHoach);
                            if (chiTiet != null)
                            {
                                chiTiet.isDaSuDung = true;
                                db.Entry(chiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                        }
                    }
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }


        public dynamic KhongDuyetQuyTrinhPhucLoi(HRM_QuyTrinh_PhucLoi QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinh_PhucLoi pcuDatabase = db.HRM_QuyTrinh_PhucLoi.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }

                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHPHUCLOI.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                    }
                    QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    string Error = FuncQuyTrinhPhucLoi(QuyTrinh, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }



                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic GetByIdQuyTrinhPhucLoi(string Id, ApplicationDbContext db)
        {
            HRM_QuyTrinh_PhucLoi model = db.HRM_QuyTrinh_PhucLoi.Find(Id);
            model.NgayLapUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayLap);
            var listChiTiet = db.HRM_QuyTrinh_PhucLoiChiTiet.Where(r => r.IdQuyTrinhPhucLoi == Id).ToList();
            foreach (var item in listChiTiet)
            {
                item.HanDangKyUnix = dungchung.DateTimeToUnixTimeStamp(item.HanDangKy);
                item.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDau);
                item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                item.lstNhanVien = db.HRM_QuyTrinh_PhucLoi_NhanVien.Where(r => r.IdQuyTrinhLoiChiTiet == item.IdGuid).OrderBy(r => r.TenNhanVien).ToList();
            }
            model.KeHoacPhucLoi = db.HRM_KeHoachPhucLoi.Find(model.IdKeHoachPhucLoi);
            model.lstChiTiet = listChiTiet;
            return model;
        }

        public dynamic DeleteQuyTrinhPhucLoi(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_QuyTrinh_PhucLoi model = db.HRM_QuyTrinh_PhucLoi.Find(Id);
                var listChiTiet = db.HRM_QuyTrinh_PhucLoiChiTiet.Where(r => r.IdQuyTrinhPhucLoi == Id).ToList();
              
                    var lstNhanVien = db.HRM_QuyTrinh_PhucLoi_NhanVien.Where(r => r.IdQuyTrinhPhucLoi == Id).ToList();
                    db.HRM_QuyTrinh_PhucLoi_NhanVien.RemoveRange(lstNhanVien);
                db.HRM_QuyTrinh_PhucLoi.Remove(model);
                db.HRM_QuyTrinh_PhucLoiChiTiet.RemoveRange(listChiTiet);

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public string FuncQuyTrinhPhucLoi(HRM_QuyTrinh_PhucLoi model, ApplicationDbContext db)
        {
            try
            {
                model.NgayLap = dungchung.UnixTimeStampToDateTime(model.NgayLapUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_QuyTrinh_PhucLoi.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var lstChiTiet = db.HRM_QuyTrinh_PhucLoiChiTiet.Where(r => r.IdQuyTrinhPhucLoi == model.IdGuid).ToList();
                db.HRM_QuyTrinh_PhucLoiChiTiet.RemoveRange(lstChiTiet);

                var lstNhanVien = db.HRM_QuyTrinh_PhucLoi_NhanVien.Where(r => r.IdQuyTrinhPhucLoi == model.IdGuid).ToList();
                db.HRM_QuyTrinh_PhucLoi_NhanVien.RemoveRange(lstNhanVien);
                if (model.lstChiTiet != null)
                {
                    var listNhanVienAdd = new List<HRM_QuyTrinh_PhucLoi_NhanVien>();
                    foreach (var item in model.lstChiTiet)
                    {
                        item.HanDangKy = dungchung.UnixTimeStampToDateTime(item.HanDangKyUnix);
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);
                        item.IdQuyTrinhPhucLoi = model.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        db.HRM_QuyTrinh_PhucLoiChiTiet.Add(item);
                        foreach (var itemCT in item.lstNhanVien)
                        {
                                itemCT.isKetThuc = model.isKetThuc;
                                itemCT.IdCongTyEOS = model.IdCongTyEOS;
                                itemCT.IdQuyTrinhPhucLoi = model.IdGuid;
                                itemCT.IdQuyTrinhLoiChiTiet = item.IdGuid;
                                itemCT.IdGuid = Guid.NewGuid().ToString();
                                itemCT.isTinhLuong = item.isTinhLuong;
                                itemCT.Ngay = item.NgayBatDau;
                                listNhanVienAdd.Add(itemCT);
                        }
                    }
                    db.HRM_QuyTrinh_PhucLoi_NhanVien.AddRange(listNhanVienAdd);
                }
                db.SaveChanges();

                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }

        }
        public dynamic ExportDanhSachHuongPhucLoiChiTiet(string Id, ApplicationDbContext db)
        {
            var itemChiTiet = db.HRM_QuyTrinh_PhucLoiChiTiet.Find(Id);
            var PhucLoi = db.HRM_QuyTrinh_PhucLoi.Find(itemChiTiet.IdQuyTrinhPhucLoi);
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var Congty = BoPhans.Where(r => r.Id == PhucLoi.IdCongTyEOS).FirstOrDefault();
            var dmPhucLoi = db.HRM_dmNhomPhucLoi.Find(itemChiTiet.IddmNhomPhucLoi);
            List<HRM_QuyTrinh_PhucLoi_NhanVien> lstNhanVien = db.HRM_QuyTrinh_PhucLoi_NhanVien.Where(r => r.IdQuyTrinhLoiChiTiet == Id).OrderBy(r => r.IdBoPhanEOS).ToList();
            string IdQuanHeGiaDinh_Con = db.HRM_dmQuanHeGiaDinh.Where(r => r.Ma == "CON").Select(r => r.IdGuid).FirstOrDefault();
            var listIdNhanVien = lstNhanVien.Select(r => r.IdNhanVien).ToList();

            if (dmPhucLoi.Ma == "QUOCTETHIEUNHI")
            {
                DateTime NgaySinhSau = itemChiTiet.NgayBatDau.AddYears(-16);
                var listNhanVienCons = (from a in db.HRM_NhanVien
                                        join b in db.HRM_NhanVienQuanHeGiaDinh on a.IdGuid equals b.IdNhanVien
                                        where listIdNhanVien.Contains(a.IdGuid) && b.IdQuanHe == IdQuanHeGiaDinh_Con
                                        && b.NgaySinh.Value.Year >= NgaySinhSau.Year
                                        select new
                                        {
                                            IdNhanVien = a.IdGuid,
                                            a.TenDayDu,
                                            b.HoTen,
                                            b.IdGioiTinh,
                                            b.NgaySinh,
                                        }).ToList();



                string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachTetThieuNhi.xlsx");
                Stream s = File.OpenRead(pathTemplate);
                ExcelPackage package = new ExcelPackage(s);
                s.Close();
                var sheet1 = package.Workbook.Worksheets[1];
                int nRow = 4;
                sheet1.Cells[1, 1].Value = Congty.TenBoPhan.ToUpper();
                int STT = 1;

                foreach (var item in lstNhanVien)
                {
                    var boPhan = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                    var listNhanVienCon = listNhanVienCons.Where(r => r.IdNhanVien == item.IdNhanVien).ToList();
                    //    sheet1.Cells[nRow, 1].Value = STT;
                    sheet1.Cells[nRow, 2].Value = listNhanVienCon[0].TenDayDu;
                    sheet1.Cells[nRow, 3].Value = boPhan;
                    sheet1.Cells[nRow, 4].Value = listNhanVienCon.Count();
                    foreach (var itemCT in listNhanVienCon)
                    {
                        sheet1.Cells[nRow, 5].Value = itemCT.HoTen;
                        sheet1.Cells[nRow, 6].Value = itemCT.NgaySinh.Value.ToString("yyyy");
                        sheet1.Cells[nRow, 7].Value = itemCT.IdGioiTinh;
                        sheet1.Cells[nRow, 1].Value = STT;
                        STT++;
                        nRow++;
                    }
                    nRow++;
                    STT++;

                }
                DrawTableExcel(sheet1, 3, nRow - 1, 1, 7);

                var sheet2 = package.Workbook.Worksheets[2];
                var listIdBoPhan = lstNhanVien.GroupBy(r => r.IdBoPhanEOS).Select(r => r.Key).ToList();
                var lstBoPhans = BoPhans.Where(r => listIdBoPhan.Contains(r.Id)).OrderBy(r => r.TenBoPhan).ToList();
                nRow = 4;

                STT = 1;
                foreach (var item in lstBoPhans)
                {
                    var listNhanVien = lstNhanVien.Where(r => r.IdBoPhanEOS == item.Id).ToList();
                    int nCount = nRow;
                    STT++;
                    nRow++;
                    sheet1.Cells[nRow, 3].Value = item.TenBoPhan;
                    foreach (var itemCT in listNhanVien)
                    {
                        var TenNHanVien = listNhanVienCons.Where(r => r.IdNhanVien == itemCT.IdNhanVien).Select(r => r.TenDayDu).FirstOrDefault();
                        sheet1.Cells[nRow, 1].Value = STT;
                        sheet1.Cells[nRow, 2].Value = TenNHanVien;
                        sheet1.Cells[nRow, 4].Value = itemCT.SoLuong;
                        STT++;
                        nRow++;
                    }
                    int Count = nRow - 1;
                    sheet1.Cells[nCount, 3, Count, 3].Merge = true;
                }

                DrawTableExcel(sheet2, 4, nRow - 1, 1, 5);
                dynamic result = new DynamicObject();

                string name = "DanhSachTetThieuNhi.xlsx";
                var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                string pathresult = PathUploads + filenamegui;
                File.WriteAllBytes(pathresult, package.GetAsByteArray());

                HDFile newfile = new HDFile();
                newfile.Name = filenamegui;
                newfile.NameLocal = name;

                newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                            + "&path=" + dungchung.Base64Encode(pathresult);
                result.file = newfile;
                return result;
            }
            else if (dmPhucLoi.Ma == "TRUNGTHU")
            {
                var lstNhanViens = db.HRM_NhanVien.Where(r => listIdNhanVien.Contains(r.IdGuid)).ToList();


                string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachTetTrungThu.xlsx");
                Stream s = File.OpenRead(pathTemplate);
                ExcelPackage package = new ExcelPackage(s);
                s.Close();
                var sheet1 = package.Workbook.Worksheets[1];
                int nRow = 4;
                sheet1.Cells[1, 1].Value = Congty.TenBoPhan.ToUpper();
                int STT = 1;
                var listIdBoPhan = lstNhanVien.GroupBy(r => r.IdBoPhanEOS).Select(r => r.Key).ToList();
                var lstBoPhans = BoPhans.Where(r => listIdBoPhan.Contains(r.Id)).OrderBy(r => r.TenBoPhan).ToList();


                STT = 1;
                foreach (var item in lstBoPhans)
                {
                    var listNhanVien = lstNhanVien.Where(r => r.IdBoPhanEOS == item.Id).ToList();
                    int nCount = nRow;
                    //  sheet1.Cells[nRow, 3, nRow + countNV, 3].Merge = true;
                    sheet1.Cells[nRow, 3].Value = item.TenBoPhan;
                    foreach (var itemCT in listNhanVien)
                    {
                        var TenNHanVien = lstNhanViens.Where(r => r.IdGuid == itemCT.IdNhanVien).Select(r => r.TenDayDu).FirstOrDefault();
                        sheet1.Cells[nRow, 1].Value = STT;
                        sheet1.Cells[nRow, 2].Value = TenNHanVien;
                        sheet1.Cells[nRow, 4].Value = itemCT.SoLuong;
                        STT++;
                        nRow++;
                    }
                    int Count = nRow - 1;
                    sheet1.Cells[nCount, 3, Count, 3].Merge = true;
                }

                DrawTableExcel(sheet1, 4, nRow - 1, 1, 5);
                dynamic result = new DynamicObject();

                string name = "DanhSachTetTrungThu.xlsx";
                var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                string pathresult = PathUploads + filenamegui;
                File.WriteAllBytes(pathresult, package.GetAsByteArray());

                HDFile newfile = new HDFile();
                newfile.Name = filenamegui;
                newfile.NameLocal = name;
                newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                                      + "&path=" + dungchung.Base64Encode(pathresult);
                result.file = newfile;
                return result;
            }
            else
            {

                var chucVus = _CallWebService.GetDanhSachChucVu(db);
                string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachPhucLoi.xlsx");
                Stream s = File.OpenRead(pathTemplate);
                ExcelPackage package = new ExcelPackage(s);
                s.Close();
                var sheet1 = package.Workbook.Worksheets[1];
                int nRow = 4;
                int STT = 1;
                string ten = "DANH SÁCH THƯỞNG " + dmPhucLoi.Ten.ToUpper();
                sheet1.Cells[1, 1, 1, 5].Merge = true;
                sheet1.Cells[1, 1].Value = Congty.TenBoPhan.ToUpper();
                sheet1.Cells[2, 1].Value = ten;
                var listIdBoPhan = lstNhanVien.GroupBy(r => r.IdBoPhanEOS).Select(r => r.Key).ToList();
                var lstBoPhans = BoPhans.Where(r => listIdBoPhan.Contains(r.Id)).OrderBy(r => r.TenBoPhan).ToList();
                var lstNhanViens = db.HRM_NhanVien.Where(r => listIdNhanVien.Contains(r.IdGuid)).ToList();
                foreach (var item in lstBoPhans)
                {
                    var listNhanVien = lstNhanVien.Where(r => r.IdBoPhanEOS == item.Id).ToList();
                    var countNV = listNhanVien.Count();
                    int nCount = nRow;
                    sheet1.Cells[nRow, 4].Value = item.TenBoPhan;
                    foreach (var itemCT in listNhanVien)
                    {
                        var TenNHanVien = lstNhanViens.Where(r => r.IdGuid == itemCT.IdNhanVien).Select(r => r.TenDayDu).FirstOrDefault();
                        var chucvuTen = chucVus.Where(r => r.Id == itemCT.IdChucVuEOS).Select(r => r.TenChucVu).FirstOrDefault();
                        sheet1.Cells[nRow, 1].Value = STT;
                        sheet1.Cells[nRow, 2].Value = TenNHanVien;
                        sheet1.Cells[nRow, 3].Value = chucvuTen;
                        sheet1.Cells[nRow, 5].Value = itemCT.SoTien;
                        sheet1.Cells[nRow, 5].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        STT++;
                        nRow++;
                    }
                    int Count = nRow - 1;
                    sheet1.Cells[nCount, 4, Count, 4].Merge = true;
                }
                if (nRow > 3)
                {
                    double SumTien = lstNhanVien.Sum(r => r.SoTien);
                    sheet1.Cells[nRow, 5].Value = SumTien;
                    sheet1.Cells[nRow, 5].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, 1].Value = "TỔNG TIỀN";
                    sheet1.Cells[nRow, 1, nRow, 4].Merge = true;
                    DrawTableExcel(sheet1, 3, nRow, 1, 6);
                }

                dynamic result = new DynamicObject();

                string name = "DanhSachPhucLoi.xlsx";
                var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                string pathresult = PathUploads + filenamegui;
                File.WriteAllBytes(pathresult, package.GetAsByteArray());

                HDFile newfile = new HDFile();
                newfile.Name = filenamegui;
                newfile.NameLocal = name;

                newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                         + "&path=" + dungchung.Base64Encode(pathresult);
                result.file = newfile;
                return result;
            }

        }
        #endregion
    }

    public class DanhSachNhanVienHuongPhucLoiViewModel
    {
        public string IdNhanVien { set; get; }
        public string TenNhanVien { set; get; }
        public string MaNhanVien { set; get; }
        public long IdChucVuEOS { set; get; }
        public string TenChucVu { set; get; }
        public long IdBoPhanEOS { set; get; }
        public string TenBoPhan { set; get; }
        public int SoCon { set; get; }
        public string IddmLoaiHopDong { set; get; }
        public string TenLoaiHopDong { set; get; }

    }
}