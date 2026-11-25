#nullable disable
using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using HRM.Api.Services.NhanVien;
using HRM.Api.Entity;
using HRM.Api.Entity.DinhBien;
using HRM.Api.Models;
using Newtonsoft.Json;
//using HRM.Api.Models.SmartEOEModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static HRM.Api.Services.FuncBase;
using static HRM.Api.Services.NhanVien.FuncKeHoachLamViec;

namespace HRM.Api.Controllers
{
    [Route("NhanVien")]
    public class NhanVienController(HrmDbContext db, FuncNhanVien func, FuncKeHoachLamViec funKH, FuncBoPhan funcBP) : ApiControllerBase
    {
        [HttpGet("GetDanhSachNhanVienChuaCoHopDong")]
        public IActionResult GetDanhSachNhanVienChuaCoHopDong() => Ok(func.GetDanhSachNhanVienChuaCoHopDong(db));

        [HttpGet("GetDanhSachNhanVienDaCoHopDong")]
        public IActionResult GetDanhSachNhanVienDaCoHopDong() => Ok(func.GetDanhSachNhanVienDaCoHopDong(db));

        [HttpGet("GetNhanVienTheoPhongBan")]
        public IActionResult GetNhanVienTheoPhongBan(long aIdPhongBan) => Ok(func.GetNhanVienTheoPhongBan(db, aIdPhongBan));

        [HttpGet("GetDanhSachNguoiDaiDienTheoCongTy")]
        public IActionResult GetDanhSachNguoiDaiDienTheoCongTy(long IdCongTyEOS) =>
            Ok(funcBP.GetDanhSachNguoiDaiDienTheoCongTy(IdCongTyEOS, db));

        //[HttpGet]
        //[Route("GetThongTinCongTyByNhanVien")]
        //public IActionResult GetThongTinCongTyByNhanVien(string IdUser)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        var item = funcBP.GetThongTinCongTyByNhanVien(IdUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}
        /// <summary>
        /// CurrentPage, NumperPage,IdBoPhan,KeyWord,TenNhanVien,ViTriChuyenMon,NgayBatDauUnix,NgayKetThucUnix,IdDuAn, IdChucVu,NgaySinhUnix,SoChungMinhThu,
        /// HoKhauThuongTruDiaChi,NguyenQuan,TrangThai,IdCongTy
        /// </summary>
        /// <param name="dyn"></param>
        /// <returns></returns>

        [HttpPost("GetDanhSachNhanVien")]
        public IActionResult GetDanhSachNhanVien([FromBody] dynamic dyn)
        {
            EntityGetDanhSachNhanVienRequest model = JsonConvert.DeserializeObject<EntityGetDanhSachNhanVienRequest>(dyn.ToString());
            return Ok(func.GetDanhSachNhanVien(model, db));
        }

        [HttpPost("GetDanhSachNhanVienAll")]
        public IActionResult GetDanhSachNhanVienAll([FromBody] dynamic dyn)
        {
            EntityGetDanhSachNhanVienRequest model = JsonConvert.DeserializeObject<EntityGetDanhSachNhanVienRequest>(dyn.ToString());
            return Ok(func.GetDanhSachNhanVienAll(model, db));
        }

        [HttpPost("GetDanhSachNhanVienNhanSuDuAn")] //Sử dụng cho nhân sự ở thông tin dự án
        public IActionResult GetDanhSachNhanVienNhanSuDuAn([FromBody] dynamic dyn)
        {
            EntityGetDanhSachNhanVienRequest model = JsonConvert.DeserializeObject<EntityGetDanhSachNhanVienRequest>(dyn.ToString());
            return Ok(func.GetDanhSachNhanVienNhanSuDuAn(model, db));
        }

        [HttpPost("GetDanhSachNhanVienThongBao")]
        public IActionResult GetDanhSachNhanVienThongBao([FromBody] dynamic dyn)
        {
            EntityGetDanhSachNhanVienRequest model = JsonConvert.DeserializeObject<EntityGetDanhSachNhanVienRequest>(dyn.ToString());
            return Ok(func.GetDanhSachNhanVienThongBao(model, db));
        }

        [HttpGet("GetMaNhanVien")]
        public IActionResult GetMaNhanVien() => Ok(func.GetMaNhanVien(db));

        [HttpGet("GetCongTyBoPhanByIdNhanVien")]
        public IActionResult GetCongTyBoPhanByIdNhanVien() => Ok(func.GetCongTyBoPhanByIdNhanVien(db));

        [HttpGet("GetCongTyByIdNhanVien")]
        public IActionResult GetCongTyByIdNhanVien() => Ok(func.GetCongTyByIdNhanVien(db));

        [HttpGet("GetDanhSachDuAnDaDuyet")]
        public IActionResult GetDanhSachDuAnDaDuyet() => Ok(func.GetDanhSachDuAnDaDuyet(db));

        [HttpGet("GetlistBoPhanByIdNhanVien")]
        public IActionResult GetlistBoPhanByIdNhanVien() => Ok(func.GetlistBoPhanByIdNhanVien(db));

        [HttpGet("GetlistBoPhanByIdUser")]
        public IActionResult GetlistBoPhanByIdUser(string IdUser) => Ok(func.GetlistBoPhanByIdUser(db, IdUser));

        [HttpGet("GetDanhSachNguoiKyByIdCongTy")]
        public IActionResult GetDanhSachNguoiKyByIdCongTy(long IdCongTy = 0, long IdBoPhan = 0) =>
            Ok(func.GetDanhSachNguoiKyByIdCongTy(db, IdCongTy, IdBoPhan));

        [HttpGet("UpdateIdUser")]
        public IActionResult UpdateIdUser() => Ok(func.UpdateIdUser(db));

        [HttpGet("GetDanhSachNhanVienChuaCoHopDongByCongTy")]
        public IActionResult GetThongTinNhanVienChuaCoHopDong(long IdCongTyEOS) =>
            Ok(func.GetDanhSachNhanVienChuaCoHopDongByCongTy(IdCongTyEOS, db));

        [HttpPost("SetThongTinNhanVien")]
        public IActionResult SetThongTinNhanVien([FromBody] HRM_NhanVien model) => Ok(func.SetThongTinNhanVien(model, db));

        [HttpGet("GetMucLuongHienTaiNhanVien")]
        public IActionResult GetMucLuongHienTaiNhanVien(string IdNhanVien) => Ok(func.GetMucLuongHienTaiNhanVien(IdNhanVien, db));

        [HttpPost("GetMucLuongHienTailistNhanVien")]
        public IActionResult GetMucLuongHienTailistNhanVien([FromBody] List<string> IdNhanVien) =>
            Ok(func.GetMucLuongHienTailistNhanVien(IdNhanVien, db));

        public class TimKiemNhanVien
        {
            public List<string> IdNhanVien { get; set; }
            public string KeyWord { get; set; }
        }


        [HttpPost("GetListNhanVienBylistId")]
        public IActionResult GetListNhanVienBylistId([FromBody] TimKiemNhanVien TimKiem) =>
            Ok(func.GetListNhanVienBylistId(TimKiem.IdNhanVien, TimKiem.KeyWord, db));

        [HttpGet("GetThongTinNhanVien")]
        public IActionResult GetThongTinNhanVien(string IdNhanVien) => Ok(func.GetThongTinNhanVien(IdNhanVien, db));

        [HttpGet("GetThongTinNhanVienByIdUser")]
        public IActionResult GetThongTinNhanVienByIdUser(string IdUser) => Ok(func.GetThongTinNhanVienByIdUser(IdUser, db));

        [HttpGet("DeleteNhanVienById")]
        public IActionResult DeleteNhanVienById(string IdNhanVien) => Ok(func.DeleteNhanVienById(IdNhanVien, db));

        [HttpPost("DeleteNhanVienBylistId")]
        public IActionResult DeleteNhanVienBylistId([FromBody] List<string> lstId) => Ok(func.DeleteNhanVienBylistId(lstId, db));

        [HttpPost("UploadAnhDaiDien")]
        public IActionResult UploadAnhDaiDien([FromBody] List<HRM_FileDinhKem> lstId) => Ok(func.UploadAnhDaiDien(lstId, db));

        [HttpPost("UploadCCCD")]
        public IActionResult UploadCCCD([FromBody] List<HRM_FileDinhKem> lstId) => Ok(func.UploadCCCD(lstId, db));

        #region Quan hệ gia đình

        [HttpGet("GetDanhSachQuanHeGiaDinhNhanVien")]
        public IActionResult GetDanhSachQuanHeGiaDinhNhanVien(string IdNhanVien) =>
            Ok(func.GetDanhSachQuanHeGiaDinhNhanVien(IdNhanVien, CurrentUserId, db));

        [HttpGet("GeQuanHeGiaDinhNhanVien")]
        public IActionResult GeQuanHeGiaDinhNhanVien(string Id) => Ok(func.GeQuanHeGiaDinhNhanVien(Id, CurrentUserId, db));

        [HttpPost("UpdateQuanHeGiaDinhNhanVien")]
        public IActionResult UpdateQuanHeGiaDinhNhanVien([FromBody] HRM_NhanVienQuanHeGiaDinh QuanHeGiaDinh) =>
            Ok(func.UpdateQuanHeGiaDinhNhanVien(QuanHeGiaDinh, CurrentUserId, db));

        [HttpPost("DeleteQuanHeGiaDinhNhanVien")]
        public IActionResult DeleteQuanHeGiaDinhNhanVien([FromBody] List<string> lstId) =>
            Ok(func.DeleteQuanHeGiaDinhNhanVien(lstId, CurrentUserId, db));

        [HttpPost("GetDanhSachQuaTrinhCongTacNhanVien")]
        public IActionResult GetDanhSachQuaTrinhCongTacNhanVien([FromBody] dynamic dyn) =>
            Ok(func.GetDanhSachQuaTrinhCongTacNhanVien(dyn, CurrentUserId, db));

        #endregion

        #region Bằng cấp nhân viên

        [HttpGet("GetDanhSachBangCapNhanVien")]
        public IActionResult GetDanhSachBangCapNhanVien(string IdNhanVien) => Ok(func.GetDanhSachBangCapNhanVien(IdNhanVien, db));

        [HttpGet("GetBangCapNhanVien")]
        public IActionResult GetBangCapNhanVien(string Id) => Ok(func.GetBangCapNhanVien(Id, db));

        [HttpPost("UpdateBangCapNhanVien")]
        public IActionResult UpdateBangCapNhanVien([FromBody] HRM_NhanVienBangCap QuanHeGiaDinh) =>
            Ok(func.UpdateBangCapNhanVien(QuanHeGiaDinh, CurrentUserId, db));

        [HttpPost("DeleteBangCapNhanVien")]
        public IActionResult DeleteBangCapNhanVien([FromBody] List<string> lstId) =>
            Ok(func.DeleteBangCapNhanVien(lstId, CurrentUserId, db));

        #endregion

        #region Ngôn ngữ nhân viên

        [HttpGet("GetDanhSachNgonNguNhanVien")]
        public IActionResult GetDanhSachNgonNguNhanVien(string IdNhanVien) =>
            Ok(func.GetDanhSachNgonNguNhanVien(IdNhanVien, CurrentUserId, db));

        [HttpGet("GetNgonNguNhanVien")]
        public IActionResult GetNgonNguNhanVien(string Id) => Ok(func.GetNgonNguNhanVien(Id, CurrentUserId, db));

        [HttpPost("UpdateNgonNguNhanVien")]
        public IActionResult UpdateNgonNguNhanVien([FromBody] HRM_NgonNguNhanVien QuanHeGiaDinh) =>
            Ok(func.UpdateNgonNguNhanVien(QuanHeGiaDinh, CurrentUserId, db));

        [HttpPost("DeleteNgonNguNhanVien")]
        public IActionResult DeleteNgonNguNhanVien([FromBody] List<string> lstId) =>
            Ok(func.DeleteNgonNguNhanVien(lstId, CurrentUserId, db));

        #endregion

        #region Chứng chỉ nhân viên

        [HttpGet("GetDanhSachChungChiNhanVien")]
        public IActionResult GetDanhSachChungChiNhanVien(string IdNhanVien) =>
            Ok(func.GetDanhSachChungChiNhanVien(IdNhanVien, CurrentUserId, db));

        [HttpGet("GetChungChiNhanVien")]
        public IActionResult GetChungChiNhanVien(string Id) => Ok(func.GetChungChiNhanVien(Id, CurrentUserId, db));

        [HttpPost("UpdateChungChiNhanVien")]
        public IActionResult UpdateChungChiNhanVien([FromBody] HRM_ChungChiNhanVien QuanHeGiaDinh) =>
            Ok(func.UpdateChungChiNhanVien(QuanHeGiaDinh, CurrentUserId, db));

        [HttpPost("DeleteChungChiNhanVien")]
        public IActionResult DeleteChungChiNhanVien([FromBody] List<string> lstId) =>
            Ok(func.DeleteChungChiNhanVien(lstId, CurrentUserId, db));

        #endregion

        #region Quá trình công tác cũ nhân viên

        [HttpGet("GetDanhSachQuaTrinhCongTacNhanVien")]
        public IActionResult GetDanhSachQuaTrinhCongTacNhanVien(string IdNhanVien) =>
            Ok(func.GetDanhSachQuaTrinhCongTacNhanVien(IdNhanVien, CurrentUserId, db));

        [HttpGet("GetQuaTrinhCongTacNhanVien")]
        public IActionResult GetQuaTrinhCongTacNhanVien(string Id) => Ok(func.GetQuaTrinhCongTacNhanVien(Id, CurrentUserId, db));

        [HttpPost("UpdateQuaTrinhCongTacNhanVien")]
        public IActionResult UpdateQuaTrinhCongTacNhanVien([FromBody] HRM_QuaTrinhCongTac QuanHeGiaDinh) =>
            Ok(func.UpdateQuaTrinhCongTacNhanVien(QuanHeGiaDinh, CurrentUserId, db));

        [HttpPost("DeleteQuaTrinhCongTacNhanVien")]
        public IActionResult DeleteQuaTrinhCongTacNhanVien([FromBody] List<string> lstId) =>
            Ok(func.DeleteQuaTrinhCongTacNhanVien(lstId, CurrentUserId, db));

        #endregion

        #region Import nhân viên

        [HttpGet("ImportExcelNhanVien")]
        public IActionResult ImportExcelNhanVien(string aFileName)
        {
            //CurrentUser currUser = GetRequest("GetListdmLoaiBaoLanh");
            var result = func.ImportExcelNhanVien(db, aFileName);
            return Ok(result);
        }

        public class importThongTinNhanVien
        {
            public string FileName { get; set; }
            public List<int> listId { get; set; }
        }


        [HttpGet("ImportExcelThongTinLienQuanNhanVien")]
        public IActionResult ImportExcelThongTinLienQuanNhanVien(string aFileName) =>
            Ok(func.ImportExcelThongTinLienQuanNhanVien(aFileName, db));

        //  [HttpPost]
        //[Route("ImportExcelThongTinLienQuanNhanVien")]
        //public IActionResult ImportExcelThongTinLienQuanNhanVien([FromBody] importThongTinNhanVien entity)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.ImportExcelThongTinLienQuanNhanVien(entity.FileName, entity.listId, db);
        //    return Ok(result);
        //}

        [HttpPost("UpdateCapNhatThongTinHopDong")]
        public IActionResult UpdateCapNhatThongTinHopDong([FromBody] HRM_NhanVien model) =>
            Ok(func.UpdateCapNhatThongTinHopDong(model, db));

        [HttpGet("GetDanhSachNhanViecByBoPhan")]
        public IActionResult GetDanhSachNhanViecByBoPhan(long IdCongTy, long IdBoPhan = 0, long IdChucVu = 0) =>
            Ok(func.GetDanhSachNhanViecByBoPhan(IdCongTy, db, IdBoPhan, IdChucVu));

        //[HttpGet]
        //[Route("GetSoNgayPhepNamConLai")]
        //public IActionResult GetSoNgayPhepNamConLai(long IdNhanVienEOS)
        //{

        //    var result = func.GetSoNgayPhepNamConLai(IdNhanVienEOS, db);
        //    return Ok(result);
        //}

        //[HttpGet]
        //[Route("GetSoNgayPhepNamConLaiNhanVien")]
        //public IActionResult GetSoNgayPhepNamConLaiNhanVien(string IdNhanVien)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    long IdNhanVienEOS = 0;
        //    var nhanVien = db.HRM_NhanVien.Find(IdNhanVien);
        //    IdNhanVienEOS = nhanVien == null ? 0 : nhanVien.IdNhanVienEOS;
        //    var result = func.GetSoNgayPhepNamConLai(IdNhanVienEOS, db);
        //    return Ok(result);
        //}
        #endregion

        [HttpPost("GetDanhSachLichSuNhanVien")]
        public IActionResult GetDanhSachLichSuNhanVien([FromBody] dynamic dyn) => Ok(func.GetDanhSachLichSuNhanVien(dyn, db));

        [HttpGet("GetLichSuNhanVien")]
        public IActionResult GetLichSuNhanVien(string Id) => Ok(func.GetLichSuNhanVien(db, Id));

        [HttpGet("ExportMauNhatKyNhanVien")]
        public IActionResult ExportMauNhatKyNhanVien() => Ok(func.ExportMauNhatKyNhanVien(db));

        [HttpGet("ImportNhatkyNhanVien")]
        public IActionResult ImportNhatkyNhanVien(string FileName, string IdNhanVien = "") =>
            Ok(func.ImportNhatkyNhanVien(db, FileName, IdNhanVien));

        [HttpPost("SetLichSuNhanVien")]
        public IActionResult SetLichSuNhanVien([FromBody]HRM_NhatKyNhanVien model) => Ok(func.SetLichSuNhanVien(model, db));

        [HttpPost("ExportNhanVienDongBHXH")]
        public IActionResult ExportNhanVienDongBHXH([FromBody] List<DanhSachNhanVienViewModel> listNhanVien) =>
            Ok(func.ExportNhanVienDongBHXH( listNhanVien));

        [HttpPost("ExportDanhSachNhanVienHetTuoiLaoDong")]
        public IActionResult ExportDanhSachNhanVienHetTuoiLaoDong([FromBody] List<DanhSachNhanVienViewModel> listNhanVien) =>
            Ok(func.ExportDanhSachNhanVienHetTuoiLaoDong(listNhanVien));

        [HttpGet("LogLichSuNhanVien")]
        public IActionResult LogLichSuNhanVien(string IdUser, long IdNhanVienEOS, string IdTable, string TableName, string QuyTrinh, double ThoiGianUnix = 0)
        {
            var currentUser = new CurrentUser(IdUser);
            db.currentUser = currentUser;
            db.IdUser = IdUser;
            string IdNhanVien = db.HRM_NhanVien.Where(r => r.IdNhanVienEOS == IdNhanVienEOS).Select(r => r.IdGuid).FirstOrDefault();
            if (IdNhanVien != null && IdNhanVien != "")
            {
                var result = func.LogLichSuNhanVien(db, IdNhanVien, IdTable, TableName, QuyTrinh, ThoiGianUnix);
                return Ok(result);
            }
            return Ok(true);
        }

        [HttpGet("GetDanhSachQuyTrinhNhatKy")]
        public IActionResult GetDanhSachQuyTrinhNhatKy() => Ok(func.GetDanhSachQuyTrinhNhatKy(db));

        //[HttpGet]
        //[Route("GetdanhSachNhanVienUserQuanLy")]
        //public IActionResult GetdanhSachNhanVienUserQuanLy(string IdUser)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetdanhSachNhanVienUserQuanLy(IdUser, db);
        //    return Ok(result);
        //}

        [HttpGet("GetDanhSachNhanVienByCongTy")]
        public IActionResult GetDanhSachNhanVienByCongTy(long IdCongTyEOS) => Ok(func.GetDanhSachNhanVienByCongTy(IdCongTyEOS, db));

        [HttpGet("ExportSoYeuLyLich")]
        public IActionResult ExportSoYeuLyLich(string Id) => Ok(func.ExportSoYeuLyLich(Id, db));

        [HttpPost("BaoCaoDanhSachNhanVien")]
        public IActionResult BaoCaoDanhSachNhanVien([FromBody] SearchNhanVienViewModel model) =>
            Ok(func.BaoCaoDanhSachNhanVien(model, db));

        [HttpPost("ExportDanhSachNhanVienByCongTy")]
        public IActionResult ExportDanhSachNhanVienByCongTy([FromBody] EntityGetDanhSachNhanVienRequest aRequest) =>
            Ok(func.ExportDanhSachNhanVienByCongTy(aRequest, db));

        [HttpPost("ExportDanhSachNhanVienTangGiamBaoHiem")]
        public IActionResult ExportDanhSachNhanVienTangGiamBaoHiem([FromBody] EntityGetDanhSachNhanVienRequest aRequest) =>
            Ok(func.ExportDanhSachNhanVienTangGiamBaoHiem(aRequest, db));

        [HttpPost("ExportDanhSachNhanVienThue")]
        public IActionResult ExportDanhSachNhanVienThue([FromBody] EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(func.ExportDanhSachNhanVienThue(db, timkiem));

        [HttpPost("ExportDanhSachNhanVienHetHanPhuThuoc")]
        public IActionResult ExportDanhSachNhanVienHetHanPhuThuoc([FromBody] EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(func.ExportDanhSachNhanVienHetHanPhuThuoc(db, timkiem));

        [HttpPost("ExportFileMauNhanVienLienQuan")]
        public IActionResult ExportFileMauNhanVienLienQuan([FromBody]EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(func.ExportExcelThongTinLienQuanNhanVien(timkiem,db));

        [HttpPost("ExportDanhSachNhanVienKhamSucKhoe")]
        public IActionResult ExportDanhSachNhanVienKhamSucKhoe([FromBody] EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(func.ExportDanhSachNhanVienKhamSucKhoe(db, timkiem));

        [HttpPost("ExportDanhSachNhanVienSinhNhat")]
        public IActionResult ExportDanhSachNhanVienSinhNhat([FromBody] EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(func.ExportDanhSachNhanVienSinhNhat(db, timkiem));

        [HttpPost("ExportDanhSachNhanVienNgayKyNiemVao")]
        public IActionResult ExportDanhSachNhanVienNgayKyNiemVao([FromBody] EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(func.ExportDanhSachNhanVienNgayKyNiemVao(db, timkiem));

        [HttpGet("UpdateNhanSu")]
        public IActionResult UpdateNhanSu()
        {
            func.UpdateNhanSu(db);
            return Ok("ok");
        }

        [HttpGet("KhoiTaoQuyTrinhSauDanhGia")]
        public IActionResult KhoiTaoQuyTrinhSauDanhGia(string IdnhanVien, string eAction) =>
            Ok(func.KhoiTaoQuyTrinhSauDanhGia(db, IdnhanVien, eAction));

        [HttpGet("GetIdNhanVienByIdUser")]
        public IActionResult GetIdNhanVienByIdUser() => Ok(func.GetIdNhanVienByIdUser(db));

        [HttpGet("GetIdNhanVienByIdUserNghiPhep")]
        public IActionResult GetIdNhanVienByIdUserNghiPhep() => Ok(func.GetIdNhanVienByIdUserNghiPhep(db));

        [HttpGet("SetThongTinNhanVienNghiViec")]
        public IActionResult SetThongTinNhanVienNghiViec() => Ok(func.SetThongTinNhanVienNghiViec(db));

        //        [Route("UpdateIdUserNhanSu")]
        //        public IActionResult UpdateIdUserNhanSu(ApplicationDbContext db, string IdUser, long IdNhanVienEOS)

        //        {
        //            var currentUser = getCurrentUser();
        //            if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //            {
        //                return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //            }
        //            db.currentUser = currentUser;
        //            func.UpdateIdUserNhanSu(db, IdUser, IdNhanVienEOS);
        //            return Ok("ok");
        //=
        //        }

        #region kế hoạch làm việc

        [HttpPost("GetListQuyTrinhKeHoachLamViecTuan")]
        public IActionResult GetListQuyTrinhKeHoachLamViecTuan([FromBody] dynamic data) =>
            Ok(funKH.GetListQuyTrinhKeHoachLamViecTuan(db, data));

        [HttpPost("ExportTheoDoiLichLamViec")]
        public IActionResult ExportTheoDoiLichLamViec([FromBody] dynamic data) => Ok(funKH.ExportTheoDoiLichLamViec(db, data));

        [HttpPost("GetTheoDoiLichLamViec")]
        public IActionResult GetTheoDoiLichLamViec([FromBody] dynamic data) => Ok(funKH.GetTheoDoiLichLamViec(db, data));

        [HttpPost("GetLichTrucMobileApp")]
        public IActionResult GetLichTrucMobileApp([FromBody] dynamic data) => Ok(funKH.GetLichTrucMobileApp(db, data));

        [HttpPost("DieuChinhLichTrucMobileApp")]
        public IActionResult DieuChinhLichTrucMobileApp([FromBody] dynamic data) => Ok(funKH.DieuChinhLichTrucMobileApp(db, data));

        [HttpPost("DieuDongLichTrucMobileApp")]
        public IActionResult DieuDongLichTrucMobileApp([FromBody] dynamic data) => Ok(funKH.DieuDongLichTrucMobileApp(db, data));

        [HttpPost("ThayTheLichTrucMobileApp")]
        public IActionResult ThayTheLichTrucMobileApp([FromBody] dynamic data) => Ok(funKH.ThayTheLichTrucMobileApp(db, data));

        [HttpGet("GetDanhSachNhanVienByIdCongTy")]
        public IActionResult GetDanhSachNhanVienByIdCongTy(long IdCongTy) => Ok(funKH.GetDanhSachNhanVienByIdCongTy(db, IdCongTy));

        [HttpGet("GetNextSoQuyTrinhKeHoachLamViecTuan")]
        public IActionResult GetNextSoQuyTrinhKeHoachLamViecTuan() => Ok(funKH.GetNextSoQuyTrinhKeHoachLamViecTuan(db));

        [HttpPost("SetQuyTrinhKeHoachLamViecTuan")]
        public IActionResult SetQuyTrinhKeHoachLamViecTuan([FromBody] dynamic data) =>
            Ok(funKH.SetQuyTrinhKeHoachLamViecTuan(db, data));

        [HttpPost("UpdateQuyTrinhKeHoachLamViecTuan")]
        public IActionResult UpdateQuyTrinhKeHoachLamViecTuan([FromBody] ThoiDoiLichLamViecNhanVienViewModel entity) =>
            Ok(funKH.SetQuyTrinhKeHoachLamViecTuan(db, entity));

        [HttpGet("GetQuyTrinhKeHoachLamViecTuan")]
        public IActionResult GetQuyTrinhKeHoachLamViecTuan(string Id) => Ok(funKH.GetQuyTrinhKeHoachLamViecTuan(db, Id));

        [HttpPost("ChuyenTiepQuyTrinhKeHoachLamViecTuan")]
        public IActionResult ChuyenTiepQuyTrinhKeHoachLamViecTuan([FromBody] dynamic data) =>
            Ok(funKH.ChuyenTiepQuyTrinhKeHoachLamViecTuan(db, data));

        [HttpPost("AddThoiGianLamViecKeHoach")]
        public IActionResult AddThoiGianLamViecKeHoach([FromBody] Add data)
        {
            funKH.AddThoiGianLamViecKeHoachNew(db, data.listIdNhanVien, dungchung.UnixTimeStampToDateTime(data.TuNgay), dungchung.UnixTimeStampToDateTime(data.DenNgay));
            return Ok("");
        }

        [HttpPost("KhongDuyetQuyTrinhKeHoachLamViecTuan")]
        public IActionResult KhongDuyetQuyTrinhKeHoachLamViecTuan([FromBody] dynamic data) =>
            Ok(funKH.KhongDuyetQuyTrinhKeHoachLamViecTuan(db, data));

        [HttpGet("DeleteQuyTrinhKeHoachLamViecTuan")]
        public IActionResult DeleteQuyTrinhKeHoachLamViecTuan(string Id) => Ok(funKH.DeleteQuyTrinhKeHoachLamViecTuan(db, Id));

        [HttpPost("ImportKeHoachLamViecTuan")]
        public IActionResult ImportKeHoachLamViecTuan([FromBody] TimKiemKeHoachTuanImport timkiem) =>
            Ok(funKH.ImportKeHoachLamViecTuan(db, timkiem.FileName, timkiem.TuNgayUnix, timkiem.DenNgayUnix, timkiem.listIdBoPhan));

        [HttpPost("ExportKeHoachLamViecTuan")]
        public IActionResult ExportKeHoachLamViecTuan([FromBody] dynamic data) => Ok(funKH.ExportKeHoachLamViecTuan(data, db));

        [HttpPost("GetTinhNhanhKeHoachLamViecTuan")]
        public IActionResult GetTinhNhanhKeHoachLamViecTuan([FromBody] TimKiemKeHoachTuan entity) =>
            Ok(funKH.GetTinhNhanhKeHoachLamViecTuan(db, entity));

        #endregion
        #region Quy trình xác nhận thời gian làm việc

        [HttpPost("GetListQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult GetListQuyTrinhXacNhanDiMuonVeSom([FromBody] dynamic data) =>
            Ok(funKH.GetListQuyTrinhXacNhanDiMuonVeSom(db, data));

        [HttpGet("GetNextSoQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult GetNextSoQuyTrinhXacNhanDiMuonVeSom() => Ok(funKH.GetNextSoQuyTrinhXacNhanDiMuonVeSom(db));

        [HttpPost("KhoiTaoQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult SetQuyTrinhXacNhanDiMuonVeSom([FromBody] TimKiemBangLuong timkiem) =>
            Ok(funKH.KhoiTaoQuyTrinhXacNhanDiMuonVeSom(db, timkiem.IdCongTy, timkiem.listIdBoPhan, timkiem.TuNgay, timkiem.DenNgay));

        [HttpPost("SetQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult SetQuyTrinhXacNhanDiMuonVeSom([FromBody] dynamic data) =>
            Ok(funKH.SetQuyTrinhXacNhanDiMuonVeSom(db, data));

        [HttpPost("UpdateQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult UpdateQuyTrinhXacNhanDiMuonVeSom([FromBody] ThoiDoiLichLamViecNhanVienViewModel entity) =>
            Ok(funKH.SetQuyTrinhXacNhanDiMuonVeSom(db, entity));

        [HttpGet("GetQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult GetQuyTrinhXacNhanDiMuonVeSom(string Id) => Ok(funKH.GetQuyTrinhXacNhanDiMuonVeSom(db, Id));

        [HttpPost("ChuyenTiepQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult ChuyenTiepQuyTrinhXacNhanDiMuonVeSom([FromBody] dynamic data) =>
            Ok(funKH.ChuyenTiepQuyTrinhXacNhanDiMuonVeSom(db, data));

        [HttpPost("KhongDuyetQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult KhongDuyetQuyTrinhXacNhanDiMuonVeSom([FromBody] dynamic data) =>
            Ok(funKH.KhongDuyetQuyTrinhXacNhanDiMuonVeSom(db, data));

        [HttpPost("ExportQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult ExportQuyTrinhXacNhanDiMuonVeSom([FromBody] dynamic data) =>
            Ok(funKH.ExportQuyTrinhXacNhanDiMuonVeSom(data, db));

        [HttpGet("DeleteQuyTrinhXacNhanDiMuonVeSom")]
        public IActionResult DeleteQuyTrinhXacNhanDiMuonVeSom(string Id) => Ok(funKH.DeleteQuyTrinhXacNhanDiMuonVeSom(db, Id));

        //[HttpGet]
        //[Route("ImportXacNhanDiMuonVeSom")]
        //public IActionResult ImportXacNhanDiMuonVeSom(string FileName, double TuNgayUnix, double DenNgayUnix)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = funKH.ImportXacNhanDiMuonVeSom(db, FileName, TuNgayUnix, DenNgayUnix);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("ExportXacNhanDiMuonVeSom")]
        //public IActionResult ExportXacNhanDiMuonVeSom([FromBody] dynamic data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = funKH.ExportXacNhanDiMuonVeSom(data, db);
        //    return Ok(item);
        //}

        #endregion
        #region Quy trình đánh giá nhân viên

        [HttpPost("GetListQuyTrinhDanhGiaNhanVien")]
        public IActionResult GetListQuyTrinhDanhGiaNhanVien([FromBody] dynamic data) =>
            Ok(funKH.GetListQuyTrinhDanhGiaNhanVien(db, data));

        [HttpGet("GetNextSoQuyTrinhDanhGiaNhanVien")]
        public IActionResult GetNextSoQuyTrinhDanhGiaNhanVien() => Ok(funKH.GetNextSoQuyTrinhDanhGiaNhanVien(db));

        [HttpGet("KhoiTaoQuyTrinhDanhGiaNhanVien")]
        public IActionResult SetQuyTrinhDanhGiaNhanVien(long IdCongTy, long IdBoPhan, string IdThuVienDanhGia, bool isCoSo = true) =>
            Ok(funKH.KhoiTaoQuyTrinhDanhGiaNhanVien(db, IdCongTy, IdBoPhan, IdThuVienDanhGia, isCoSo));

        [HttpPost("SetQuyTrinhDanhGiaNhanVien")]
        public IActionResult SetQuyTrinhDanhGiaNhanVien([FromBody] dynamic data) => Ok(funKH.SetQuyTrinhDanhGiaNhanVien(db, data));

        [HttpPost("UpdateQuyTrinhDanhGiaNhanVien")]
        public IActionResult UpdateQuyTrinhDanhGiaNhanVien([FromBody] ThoiDoiLichLamViecNhanVienViewModel entity) =>
            Ok(funKH.SetQuyTrinhDanhGiaNhanVien(db, entity));

        [HttpGet("GetQuyTrinhDanhGiaNhanVien")]
        public IActionResult GetQuyTrinhDanhGiaNhanVien(string Id) => Ok(funKH.GetQuyTrinhDanhGiaNhanVien(db, Id));

        [HttpPost("ChuyenTiepQuyTrinhDanhGiaNhanVien")]
        public IActionResult ChuyenTiepQuyTrinhDanhGiaNhanVien([FromBody] dynamic data) =>
            Ok(funKH.ChuyenTiepQuyTrinhDanhGiaNhanVien(db, data));

        [HttpPost("KhongDuyetQuyTrinhDanhGiaNhanVien")]
        public IActionResult KhongDuyetQuyTrinhDanhGiaNhanVien([FromBody] dynamic data) =>
            Ok(funKH.KhongDuyetQuyTrinhDanhGiaNhanVien(db, data));

        [HttpGet("DeleteQuyTrinhDanhGiaNhanVien")]
        public IActionResult DeleteQuyTrinhDanhGiaNhanVien(string Id) => Ok(funKH.DeleteQuyTrinhDanhGiaNhanVien(db, Id));

        //[HttpGet]
        //[Route("ImportDanhGiaNhanVien")]
        //public IActionResult ImportDanhGiaNhanVien(string FileName, double TuNgayUnix, double DenNgayUnix)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = funKH.ImportDanhGiaNhanVien(db, FileName, TuNgayUnix, DenNgayUnix);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("ExportDanhGiaNhanVien")]
        //public IActionResult ExportDanhGiaNhanVien([FromBody] dynamic data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = funKH.ExportDanhGiaNhanVien(data, db);
        //    return Ok(item);
        //}

        [HttpPost("GetBaoCaoDanhGiaNhanVien")]
        public IActionResult GetBaoCaoDanhGiaNhanVien([FromBody] dynamic data) => Ok(funKH.GetBaoCaoDanhGiaNhanVien(data, db));

        [HttpPost("GetTongHopDanhGiaNhanVienThang")]
        public IActionResult GetTongHopDanhGiaNhanVienThang([FromBody] dynamic data) =>
            Ok(funKH.GetTongHopDanhGiaNhanVienThang(data, db));

        [HttpPost("ExportBaoCaoDanhGiaNhanVien")]
        public IActionResult ExportBaoCaoDanhGiaNhanVien([FromBody] dynamic data) => Ok(funKH.ExportBaoCaoDanhGiaNhanVien(data, db));

        [HttpPost("GetBaoCaoTongQuanNhanVien")]
        public IActionResult GetBaoCaoTongQuanNhanVien([FromBody] dynamic data) => Ok(funKH.GetBaoCaoTongQuanNhanVien(data, db));

        [HttpPost("ExportBaoCaoTongQuanNhanVien")]
        public IActionResult ExportBaoCaoTongQuanNhanVien([FromBody] dynamic data) => Ok(funKH.ExportBaoCaoTongQuanNhanVien(data, db));

        [HttpPost("ExportDanhSachDanhGiaNhanVienThang")]
        public IActionResult ExportDanhSachDanhGiaNhanVienThang([FromBody] HRM_TimKiemDanhGia data) =>
            Ok(funKH.ExportDanhSachDanhGiaNhanVienThang(data, db));

        [HttpPost("GetDanhSachDanhGiaNhanVienThang")]
        public IActionResult GetDanhSachDanhGiaNhanVienThang([FromBody] HRM_TimKiemDanhGia data) =>
            Ok(funKH.GetDanhSachDanhGiaNhanVienThang(db, data));

        #endregion

        [HttpPost("GetListYeuCauThayDoiThongTinCaNhan")]
        public IActionResult GetListYeuCauThayDoiThongTinCaNhan([FromBody] dynamic data) =>
            Ok(func.GetListYeuCauThayDoiThongTinCaNhan(db, data));

        [HttpPost("SetYeuCauThayDoiThongTinCaNhan")]
        public IActionResult SetYeuCauThayDoiThongTinCaNhan([FromBody] HRM_NhanVienDieuChinhThongTin data) =>
            Ok(func.SetYeuCauThayDoiThongTinCaNhan(data, db));

        [HttpGet("GetYeuCauThayDoiThongTinCaNhan")]
        public IActionResult GetYeuCauThayDoiThongTinCaNhan(string Id) => Ok(func.GetYeuCauThayDoiThongTinCaNhan(Id, db));

        #region xác nhận ngoài giờ.

        [HttpGet("GetNextSoQuyTrinhXacNhanNgoaiGio")]
        public IActionResult GetNextMaQuyTrinhXacNhanNgoaiGio() => Ok(funKH.GetNextMaQuyTrinhXacNhanNgoaiGio(db));

        [HttpPost("GetListQuyTrinhXacNhanNgoaiGio")]
        public IActionResult GetListQuyTrinhXacNhanNgoaiGio([FromBody] dynamic model) =>
            Ok(funKH.GetListQuyTrinhXacNhanNgoaiGio(model, db));

        [HttpPost("SetQuyTrinhXacNhanNgoaiGio")]
        public IActionResult SetQuyTrinhXacNhanNgoaiGio([FromBody] HRM_QuyTrinhXacNhanNgoaiGio model) =>
            Ok(funKH.SetQuyTrinhXacNhanNgoaiGio(model, db));

        [HttpPost("ChuyenTiepQuyTrinhXacNhanNgoaiGio")]
        public IActionResult ChuyenTiepQuyTrinhXacNhanNgoaiGio([FromBody] HRM_QuyTrinhXacNhanNgoaiGio model) =>
            Ok(funKH.ChuyenTiepQuyTrinhXacNhanNgoaiGio(model, db));

        [HttpPost("KhoiTaoQuyTrinhXachNhanNgoaiGio")]
        public IActionResult KhoiTaoQuyTrinhXachNhanNgoaiGio([FromBody] XacNhanNgoaiGio model) =>
            Ok(funKH.KhoiTaoQuyTrinhXachNhanNgoaiGio(db, model.IdCongTy, model.TuNgay, model.DenNgay, model.listIdNhanVien));

        public class XacNhanNgoaiGio
        {
            public long IdCongTy { get; set; }
            public long IdBoPhan { get; set; }
            public double TuNgay { get; set; }
            public double DenNgay { get; set; }
            public List<string> listIdNhanVien { get; set; }
        }


        [HttpPost("KhongDuyetQuyTrinhXacNhanNgoaiGio")]
        public IActionResult KhongDuyetQuyTrinhXacNhanNgoaiGio([FromBody] HRM_QuyTrinhXacNhanNgoaiGio model) =>
            Ok(funKH.KhongDuyetQuyTrinhXacNhanNgoaiGio(model, db));

        [HttpGet("GetByIdQuyTrinhXacNhanNgoaiGio")]
        public IActionResult GetByIdQuyTrinhXacNhanNgoaiGio(string Id) => Ok(funKH.GetByIdQuyTrinhXacNhanNgoaiGio(Id, db));

        [HttpGet("DeleteQuyTrinhXacNhanNgoaiGio")]
        public IActionResult DeleteQuyTrinhXacNhanNgoaiGio(string Id) => Ok(funKH.DeleteQuyTrinhXacNhanNgoaiGio(Id, db));

        #endregion
        #region xác nhận ngoài giờ.

        [HttpGet("GetNextSoQuyTrinhDanhGia")]
        public IActionResult GetNextMaQuyTrinhDanhGia() => Ok(funKH.GetNextMaQuyTrinhDanhGia(db));

        [HttpPost("GetListQuyTrinhDanhGia")]
        public IActionResult GetListQuyTrinhDanhGia([FromBody] dynamic model) => Ok(funKH.GetListQuyTrinhDanhGia(model, db));

        [HttpPost("SetQuyTrinhDanhGia")]
        public IActionResult SetQuyTrinhDanhGia([FromBody] HRM_QuyTrinhDanhGia model) => Ok(funKH.SetQuyTrinhDanhGia(model, db));

        [HttpPost("ChuyenTiepQuyTrinhDanhGia")]
        public IActionResult ChuyenTiepQuyTrinhDanhGia([FromBody] HRM_QuyTrinhDanhGia model) =>
            Ok(funKH.ChuyenTiepQuyTrinhDanhGia(model, db));

        [HttpPost("KhongDuyetQuyTrinhDanhGia")]
        public IActionResult KhongDuyetQuyTrinhDanhGia([FromBody] HRM_QuyTrinhDanhGia model) =>
            Ok(funKH.KhongDuyetQuyTrinhDanhGia(model, db));

        [HttpPost("GetDanhSachNhanVienForDanhGia")]
        public IActionResult GetDanhSachNhanVienForDanhGia([FromBody] DanhGiaNhanVien model) =>
            Ok(funKH.GetDanhSachNhanVienForDanhGia(db, model.IdCongTy, model.IdBoPhan, model.IdChucvu, model.Nam, model.Thang));

        [HttpGet("GetByIdQuyTrinhDanhGia")]
        public IActionResult GetByIdQuyTrinhDanhGia(string Id) => Ok(funKH.GetByIdQuyTrinhDanhGia(Id, db));

        [HttpGet("DeleteQuyTrinhDanhGia")]
        public IActionResult DeleteQuyTrinhDanhGia(string Id) => Ok(funKH.DeleteQuyTrinhDanhGia(Id, db));

        #endregion

        [HttpGet("GetBaoCaoCongViec")]
        public IActionResult GetBaoCaoCongViec(string Id) => Ok(funKH.GetBaoCaoCongViec(Id, db));

        [HttpGet("SaoChepBaoCaoCongViec")]
        public IActionResult SaoChepBaoCaoCongViec(string Id) => Ok(funKH.SaoChepBaoCaoCongViec(Id, db));

        [HttpGet("GetListHeader")]
        public IActionResult GetListHeader(long IdBoPhan) => Ok(funKH.GetListHeader(IdBoPhan, db));

        [HttpGet("DeleteBaoCaoCongViec")]
        public IActionResult DeleteBaoCaoCongViec(string Id) => Ok(funKH.DeleteBaoCaoCongViec(Id, db));

        [HttpPost("SetBaoCaoCongViec")]
        public IActionResult SetBaoCaoCongViec([FromBody] dynamic dyn)
        {
            HRM_BaoCaoCongViec_NhanVien model = JsonConvert.DeserializeObject<HRM_BaoCaoCongViec_NhanVien>(dyn.ToString());
            return Ok(funKH.SetBaoCaoCongViec(model, db));
        }

        [HttpPost("GetListBaoCaoCongViec")]
        public IActionResult GetListBaoCaoCongViec([FromBody] dynamic model) => Ok(funKH.GetListBaoCaoCongViec(model, db));

        [HttpPost("ExportBaoCaoCongViec")]
        public IActionResult ExportBaoCaoCongViec([FromBody] dynamic model) => Ok(funKH.ExportBaoCaoCongViec(model, db));
    }
}
