﻿﻿#nullable disable
using HRM.Api.Entity;
using HRM.Api.Models;
using System;
using System.Collections.Generic;
using HRM.Api.Filters;
using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace HRM.Api.Controllers
{

    [Route("DanhMuc")]
    public class DanhMucController(HrmDbContext db, CallWebService _CallWebService, FuncDanhMuc func, ILogger<DanhMucController> _logger) : ApiControllerBase
    {
        [HttpGet("GetDanhMucLoaiLuong")]
        public IActionResult GetDanhMucLoaiLuong() => Ok(func.GetDanhMucLoaiLuong());

        [HttpGet("GetListLoaiXinDiMuonVeSom")]
        public IActionResult GetListLoaiXinDiMuonVeSom() => Ok(func.GetListLoaiXinDiMuonVeSom());

        [HttpGet("GetDanhMucLoaiPhuCap")]
        public IActionResult GetDanhMucLoaiPhuCap() => Ok(func.GetDanhMucLoaiPhuCap());

        [HttpGet("GetDanhMucLoaiChucNang")]
        public IActionResult GetDanhMucLoaiChucNang() => Ok(func.GetDanhMucLoaiChucNang());

        [HttpGet("GetDanhMucLoaiTrangThai")]
        public IActionResult GetDanhMucLoaiTrangThai() => Ok(func.GetDanhMucLoaiTrangThai());

        [HttpPost("SmartDongBoQuyTrinhXuLy")]
        [AllowAnonymousSmartEos]
        public IActionResult SmartDongBoQuyTrinhXuLy([FromBody] List<HRM_QuyTrinhXuLy> listQuyTrinh)
        {
            CurrentUser currUser = new CurrentUser();
            currUser.Id = "";
            //if (!String.IsNullOrEmpty(currUser.Id))
            //{
            db.currentUser = currUser;
            var data = func.SmartDongBoQuyTrinhXuLy(db, listQuyTrinh);
            return Ok(data);
            //}
            //return Ok(new ErrorMessage(ErrorMessage.eState.ChuaDangNhap));
        }

        #region mức đống thuế thu nhập cá nhân

        [HttpGet("GetDanhSachCauHinhThueThuNhapCaNhan")]
        public IActionResult GetDanhSachCauHinhThueThuNhapCaNhan() => Ok(func.GetDanhSachCauHinhThueThuNhapCaNhan(db));

        [HttpPost("UpdatedmCauHinhThueThuNhapCaNhan")]
        public IActionResult UpdateDanhMucQuocTich([FromBody] HRM_dmCauHinhThueThuNhapCaNhan data) =>
            Ok(func.UpdatedmCauHinhThueThuNhapCaNhan(data, db));

        [HttpPost("DeletedmCauHinhThueThuNhapCaNhan")]
        public IActionResult DeletedmCauHinhThueThuNhapCaNhan([FromBody] List<string> data) =>
            Ok(func.DeletedmCauHinhThueThuNhapCaNhan(data, db));

        #endregion

        #region Quốc tịch

        [HttpGet("GetDanhSachQuocTich")]
        public IActionResult GetDanhSachQuocTich() => Ok(func.GetDanhSachQuocTich(db));

        [HttpPost("UpdateDanhMucQuocTich")]
        public IActionResult UpdateDanhMucQuocTich([FromBody] HRM_dmQuocTich data) =>
            Ok(func.UpdateDanhMucQuocTich(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucQuocTich")]
        public IActionResult DeleteDanhMucQuocTich([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucQuocTich(data, CurrentUserId, db));

        #endregion

        #region Dân tộc

        [HttpGet("GetDanhSachDanToc")]
        public IActionResult GetDanhSachDanToc()
        {
            try
            {
                return Ok(func.GetDanhSachDanToc(db));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Ok();
            }
        }

        [HttpPost("UpdateDanhMucDanToc")]
        public IActionResult UpdateDanhMucDanToc([FromBody] HRM_dmDanToc data) =>
            Ok(func.UpdateDanhMucDanToc(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucDanToc")]
        public IActionResult DeleteDanhMucDanToc([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucDanToc(data, CurrentUserId, db));

        #endregion

        #region tình trạng hôn nhân

        [HttpGet("GetDanhSachTinhTrangHonNhan")]
        public IActionResult GetDanhSachTinhTrangHonNhan() => Ok(func.GetDanhSachTinhTrangHonNhan(db));

        [HttpPost("UpdateDanhMucTinhTrangHonNhan")]
        public IActionResult UpdateDanhMucTinhTrangHonNhan([FromBody] HRM_dmTinhTrangHonNhan data) =>
            Ok(func.UpdateDanhMucTinhTrangHonNhan(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucTinhTrangHonNhan")]
        public IActionResult DeleteDanhMucTinhTrangHonNhan([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucTinhTrangHonNhan(data, CurrentUserId, db));

        #endregion
        //#region loại hình khám sức khỏe

        //[HttpGet]
        //[Route("GetDanhSachLoaiHinhKhamSucKhoe")]
        //public IActionResult GetDanhSachLoaiHinhKhamSucKhoe()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetDanhSachLoaiHinhKhamSucKhoe(db);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("UpdateDanhMucLoaiHinhKhamSucKhoe")]
        //public IActionResult UpdateDanhMucLoaiHinhKhamSucKhoe(HRM_dmLoaiHinhKhamSucKhoe data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.UpdateDanhMucLoaiHinhKhamSucKhoe(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucLoaiHinhKhamSucKhoe")]
        //public IActionResult DeleteDanhMucLoaiHinhKhamSucKhoe(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.DeleteDanhMucLoaiHinhKhamSucKhoe(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //#endregion
        #region Loại nhân viên

        [HttpGet("GetDanhSachLoaiNhanVien")]
        public IActionResult GetDanhSachLoaiNhanVien() => Ok(func.GetDanhSachLoaiNhanVien(db));

        [HttpPost("UpdateDanhMucLoaiNhanVien")]
        public IActionResult UpdateDanhMucLoaiNhanVien([FromBody] HRM_dmLoaiNhanVien data) =>
            Ok(func.UpdateDanhMucLoaiNhanVien(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucLoaiNhanVien")]
        public IActionResult DeleteDanhMucLoaiNhanVien([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucLoaiNhanVien(data, CurrentUserId, db));

        #endregion

        #region Chức danh

        //[HttpGet]
        //[Route("GetListDanhSachChucDanh")]
        //public IActionResult GetListDanhSachChucDanh(byte Loai)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetListDanhSachChucDanh(db, Loai);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("UpdateDanhMucChucDanh")]
        //public IActionResult UpdateDanhMucChucDanh(HRM_dmChucDanh data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.UpdateDanhMucChucDanh(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucChucDanh")]
        //public IActionResult DeleteDanhMucChucDanh(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.DeleteDanhMucChucDanh(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        #endregion

        #region  nhóm chuyên môn

        [HttpGet("GetDanhSachNhomChuyenMon")]
        public IActionResult GetDanhSachNhomChuyenMon() => Ok(func.GetDanhSachNhomChuyenMon(db));

        [HttpPost("UpdateDanhMucNhomChuyenMon")]
        public IActionResult UpdateDanhMucNhomChuyenMon([FromBody] HRM_dmNhomChuyenMon data) =>
            Ok(func.UpdateDanhMucNhomChuyenMon(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucNhomChuyenMon")]
        public IActionResult DeleteDanhMucNhomChuyenMon([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucNhomChuyenMon(data, CurrentUserId, db));

        #endregion

        #region Vị trí chuyên môn

        [HttpGet("GetDanhSachViTriChuyenMon")]
        public IActionResult GetDanhSachViTriChuyenMon() => Ok(func.GetDanhSachViTriChuyenMon(db));

        [HttpGet("GetViTriChuyenMon")]
        public IActionResult GetViTriChuyenMon(string Id) => Ok(func.GetViTriChuyenMon(Id, db));

        [HttpPost("UpdateDanhMucChuyenMon")]
        public IActionResult UpdateDanhMucChuyenMon([FromBody] HRM_dmViTriChuyenMon data) =>
            Ok(func.UpdateDanhMucChuyenMon(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucViTriChuyenMon")]
        public IActionResult DeleteDanhMucViTriChuyenMon([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucViTriChuyenMon(data, CurrentUserId, db));

        #endregion

        #region  Cấp bậc

        [HttpGet("GetDanhSachCapBac")]
        public IActionResult GetDanhSachCapBac() => Ok(func.GetDanhSachCapBac(db));

        [HttpPost("UpdateDanhMucCapBac")]
        public IActionResult UpdateDanhMucCapBac([FromBody] HRM_dmCapBac data) =>
            Ok(func.UpdateDanhMucCapBac(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucCapBac")]
        public IActionResult DeleteDanhMucCapBac([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucCapBac(data, CurrentUserId, db));

        #endregion

        #region  quan hệ gia đình

        [HttpGet("GetDanhSachQuanHeGiaDinh")]
        public IActionResult GetDanhSachQuanHeGiaDinh() => Ok(func.GetDanhSachQuanHeGiaDinh(db));

        [HttpPost("UpdateDanhMucQuanHeGiaDinh")]
        public IActionResult UpdateDanhMucQuanHeGiaDinh([FromBody] HRM_dmQuanHeGiaDinh data) =>
            Ok(func.UpdateDanhMucQuanHeGiaDinh(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucQuanHeGiaDinh")]
        public IActionResult DeleteDanhMucQuanHeGiaDinh([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucQuanHeGiaDinh(data, CurrentUserId, db));

        #endregion

        #region  Thời gian làm việc

        [HttpGet("GetDanhSachThoiGianLamViec")]
        public IActionResult GetDanhSachThoiGianLamViec() => Ok(func.GetDanhSachThoiGianLamViec(db));

        [HttpGet("GetDanhSachHinhThucLamViec")]
        public IActionResult GetDanhSachHinhThucLamViec() => Ok(func.GetDanhSachHinhThucLamViec(db));

        [HttpPost("UpdateDanhHinhThucLamViec")]
        public IActionResult UpdateDanhHinhThucLamViec([FromBody] HRM_dmHinhThucLamViec data) =>
            Ok(func.UpdateDanhHinhThucLamViec(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucHinhThucLamViec")]
        public IActionResult DeleteDanhMucHinhThucLamViec([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucHinhThucLamViec(data, CurrentUserId, db));

        [HttpPost("UpdateDanhMucThoiGianLamViec")]
        public IActionResult UpdateDanhMucThoiGianLamViec([FromBody] HRM_dmThoiGianLamViec data) =>
            Ok(func.UpdateDanhMucThoiGianLamViec(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucThoiGianLamViec")]
        public IActionResult DeleteDanhMucThoiGianLamViec([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucThoiGianLamViec(data, CurrentUserId, db));

        #endregion

        #region  Hệ số nghỉ bù

        [HttpGet("GetDanhSachHeSoNghiBu")]
        public IActionResult GetDanhSachHeSoNghiBu() => Ok(func.GetDanhSachHeSoNghiBu(db));

        [HttpPost("UpdateDanhMucHeSoNghiBu")]
        public IActionResult UpdateDanhMucHeSoNghiBu([FromBody] HRM_dmHeSoNghiBu data) =>
            Ok(func.UpdateDanhMucHeSoNghiBu(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucHeSoNghiBu")]
        public IActionResult DeleteDanhMucHeSoNghiBu([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucHeSoNghiBu(data, CurrentUserId, db));

        #endregion

        #region  Ăn ca

        [HttpGet("GetDanhSachAnCa")]
        public IActionResult GetDanhSachAnCa() => Ok(func.GetDanhSachAnCa(db));

        [HttpPost("GetListdmAnCa")]
        public IActionResult GetListdmAnCa([FromBody] dynamic data) => Ok(func.GetListdmAnCa(data, db));

        [HttpPost("UpdateDanhMucAnCa")]
        public IActionResult UpdateDanhMucAnCa([FromBody] HRM_dmAnCa data) => Ok(func.UpdateDanhMucAnCa(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucAnCa")]
        public IActionResult DeleteDanhMucAnCa([FromBody] List<string> data) => Ok(func.DeleteDanhMucAnCa(data, CurrentUserId, db));

        #endregion

        #region  Khu vực

        [HttpGet("GetDanhSachKhuVuc")]
        public IActionResult GetDanhSachKhuVuc() => Ok(func.GetDanhSachKhuVuc(db));

        [HttpPost("UpdateDanhMucKhuVuc")]
        public IActionResult UpdateDanhMucKhuVuc([FromBody] HRM_dmKhuVuc data) =>
            Ok(func.UpdateDanhMucKhuVuc(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucKhuVuc")]
        public IActionResult DeleteDanhMucKhuVuc([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucKhuVuc(data, CurrentUserId, db));

        #endregion

        #region  Khu vực địa điểm

        [HttpGet("GetDanhSachKhuVucDiaDiem")]
        public IActionResult GetDanhSachKhuVucDiaDiem() => Ok(func.GetDanhSachKhuVucDiaDiem(db));

        [HttpPost("UpdateDanhMucKhuVucDiaDiem")]
        public IActionResult UpdateDanhMucKhuVucDiaDiem([FromBody] HRM_dmKhuVucDiaDiem data) =>
            Ok(func.UpdateDanhMucKhuVucDiaDiem(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucKhuVucDiaDiem")]
        public IActionResult DeleteDanhMucKhuVucDiaDiem([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucKhuVucDiaDiem(data, CurrentUserId, db));

        #endregion

        #region  Công tác phí

        [HttpPost("GetListdmCongTacPhi")]
        public IActionResult GetListdmCongTacPhi([FromBody] dynamic dyn) => Ok(func.GetListdmCongTacPhi(db, dyn));

        [HttpPost("SetdmCongTacPhi")]
        public IActionResult SetdmCongTacPhi([FromBody] HRM_dmCongTacPhi data) =>
            Ok(func.UpdateDanhMucCongTacPhi(data, CurrentUserId, db));

        [HttpPost("DeletedmCongTacPhi")]
        public IActionResult DeletedmCongTacPhi([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucCongTacPhi(data, CurrentUserId, db));

        #endregion

        #region Danh mục công ty

        //[HttpGet]
        //[Route("GetListDanhMucCongTy")]
        //public IActionResult GetListDanhMucCongTy()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetListDanhMucCongTy(db);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("SetDanhMucCongTy")]
        //public IActionResult SetDanhMucCongTy(HRM_dmCongTy data)
        //{
        //    var currentUser = getCurrentUser();
        //    if(currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.SetDanhMucCongTy(data, db);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucCongTy")]
        //public IActionResult DeleteDanhMucCongTy(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if(currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.DeleteDanhMucCongTy(data, db);
        //    return Ok(item);
        //}

        #endregion

        #region  Địa điểm làm việc

        [HttpGet("GetDanhSachDiaDiemLamViec")]
        public IActionResult GetDanhSachDiaDiemLamViec() => Ok(func.GetDanhSachDiaDiemLamViec(db));

        [HttpPost("UpdateDiaDiemLamViec")]
        public IActionResult UpdateDiaDiemLamViec([FromBody] HRM_dmDiaDiemLamViec data) =>
            Ok(func.UpdateDiaDiemLamViec(data, CurrentUserId, db));

        [HttpPost("DeleteDiaDiemLamViec")]
        public IActionResult DeleteDiaDiemLamViec([FromBody] List<string> data) =>
            Ok(func.DeleteDiaDiemLamViec(data, CurrentUserId, db));

        #endregion

        #region Danh mục thiết lập ngày nghỉ

        [HttpGet("GetDanhSachDanhMucThietLapNgayNghi")]
        public IActionResult GetDanhSachDanhMucThietLapNgayNghi() => Ok(func.GetDanhSachDanhMucThietLapNgayNghi(db));

        [HttpPost("SetDanhMucThietLapNgayNghi")]
        public IActionResult SetDanhMucThietLapNgayNghi([FromBody] HRM_dmThietLapNgayNghi data) =>
            Ok(func.SetDanhMucThietLapNgayNghi(data, db));

        [HttpPost("DeleteDanhMucThietLapNgayNghi")]
        public IActionResult DeleteDanhMucThietLapNgayNghi([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucThietLapNgayNghi(data, db));

        #endregion

        //#region  Danh sách công ty

        ////[HttpGet]
        ////[Route("GetDanhSachCongTy")]
        ////public IActionResult GetDanhSachCongTy()
        ////{
        ////    var currentUser = getCurrentUser();
        ////    if (currentUser != null)
        ////    {
        ////        string IdCurrentUser = currentUser.Id;
        ////        db.IdUser = IdCurrentUser;
        ////        db.currentUser = currentUser;
        ////        FuncBoPhan func = new FuncBoPhan();
        ////        var item = func.GetDanhSachCongTy(IdCurrentUser, db);
        ////        return Ok(item);
        ////    }
        ////    else
        ////        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        ////}

        //[HttpPost]
        //[Route("UpdateDanhMucCongTy")]
        //public IActionResult UpdateDanhMucCongTy(HRM_dmCongTy data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.UpdateDanhMucCongTy(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucCongTy")]
        //public IActionResult DeleteDanhMucCongTy(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.DeleteDanhMucCongTy(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}
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
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.GetThongTinCongTyByNhanVien(IdUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}
        //#endregion

        #region  Bộ phận

        //[HttpGet]
        //[Route("GetDanhSachBoPhan")]
        //public IActionResult GetDanhSachBoPhan(bool isKhongLayCongTy = false)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.GetDanhSachBoPhan(isKhongLayCongTy, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpGet]
        //[Route("GetDanhSachBoPhanThuoc")]
        //public IActionResult GetDanhSachBoPhanThuoc(string MaLoai, string IdCongTy)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.GetDanhSachBoPhanThuoc(MaLoai, IdCongTy, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpGet]
        //[Route("GetBoPhan")]
        //public IActionResult GetBoPhan(string Id)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.GetBoPhan(Id, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("UpdateBoPhan")]
        //public IActionResult UpdateBoPhan(HRM_dmBoPhan data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.UpdateBoPhan(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteBoPhan")]
        //public IActionResult DeleteBoPhan(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncBoPhan func = new FuncBoPhan();
        //        var item = func.DeleteBoPhan(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        #endregion

        #region  Chức vụ

        //[HttpGet]
        //[Route("GetDanhSachChucVu")]
        //public IActionResult GetDanhSachChucVu(bool isChucVuDuAn)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    FuncChucVu func = new FuncChucVu();
        //    var item = func.GetDanhSachChucVu(isChucVuDuAn, db);
        //    return Ok(item);
        //}

        //[HttpGet]
        //[Route("GetChucVu")]
        //public IActionResult GetChucVu(string Id)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    FuncChucVu func = new FuncChucVu();
        //    var item = func.GetChucVu(Id, db);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("UpdateChucVu")]
        //public IActionResult UpdateChucVu(HRM_dmChucVu data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncChucVu func = new FuncChucVu();
        //        var item = func.UpdateChucVu(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteChucVu")]
        //public IActionResult DeleteChucVu(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncChucVu func = new FuncChucVu();
        //        var item = func.DeleteChucVu(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        #endregion

        #region  Tiên tệ

        [HttpGet("GetDanhSachTienTe")]
        public IActionResult GetDanhSachTienTe() => Ok(func.GetDanhSachTienTe(db));

        [HttpPost("UpdateDanhMucTienTe")]
        public IActionResult UpdateDanhMucTienTe([FromBody] HRM_dmTienTe data) =>
            Ok(func.UpdateDanhMucTienTe(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucTienTe")]
        public IActionResult DeleteDanhMucTienTe([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucTienTe(data, CurrentUserId, db));

        #endregion
        //#region  Tôn giáo

        //[HttpGet]
        //[Route("GetDanhSachTonGiao")]
        //public IActionResult GetDanhSachTonGiao()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var reuslt = func.GetDanhSachTonGiao(db);
        //    return Ok(reuslt);
        //}

        //[HttpPost]
        //[Route("UpdateDanhMucTonGiao")]
        //public IActionResult UpdateDanhMucTonGiao(HRM_dmTonGiao data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.UpdateDanhMucTonGiao(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucTonGiao")]
        //public IActionResult DeleteDanhMucTonGiao(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.DeleteDanhMucTonGiao(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //#endregion
        //#region  Loại nghỉ phép

        //[HttpGet]
        //[Route("GetDanhSachLoaiNghiPhep")]
        //public IActionResult GetDanhSachLoaiNghiPhep()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetDanhSachLoaiNghiPhep(db);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("UpdateDanhMucLoaiNghiPhep")]
        //public IActionResult UpdateDanhMucLoaiNghiPhep(HRM_dmLoaiNghiPhep data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.UpdateDanhMucLoaiNghiPhep(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucLoaiNghiPhep")]
        //public IActionResult DeleteDanhMucLoaiNghiPhep(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.DeleteDanhMucLoaiNghiPhep(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //#endregion

        #region  Trình độ đào tạo

        [HttpGet("GetDanhSachTrinhDoDaoTao")]
        public IActionResult GetDanhSachTrinhDoDaoTao() => Ok(func.GetDanhSachTrinhDoDaoTao(db));

        [HttpPost("UpdateDanhMucTrinhDoDaoTao")]
        public IActionResult UpdateDanhMucTrinhDoDaoTao([FromBody] HRM_dmTrinhDoDaoTao data) =>
            Ok(func.UpdateDanhMucTrinhDoDaoTao(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucTrinhDoDaoTao")]
        public IActionResult DeleteDanhMucTrinhDoDaoTao([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucTrinhDoDaoTao(data, CurrentUserId, db));

        #endregion

        #region  Ngôn ngữ

        [HttpGet("GetDanhSachNgonNgu")]
        public IActionResult GetDanhSachNgonNgu() => Ok(func.GetDanhSachNgonNgu(db));

        [HttpPost("UpdateDanhMucNgonNgu")]
        public IActionResult UpdateDanhMucNgonNgu([FromBody] HRM_dmNgonNgu data) =>
            Ok(func.UpdateDanhMucNgonNgu(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucNgonNgu")]
        public IActionResult DeleteDanhMucNgonNgu([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucNgonNgu(data, CurrentUserId, db));

        #endregion

        #region Danh mục nguồn tuyển dụng

        [HttpGet("GetDanhSachNguonTuyenDung")]
        public IActionResult GetDanhSachNguonTuyenDung() => Ok(func.GetDanhSachNguonTuyenDung(db));

        [HttpPost("SetDanhMucNguonTuyenDung")]
        public IActionResult SetDanhMucNguonTuyenDung([FromBody] HRM_dmNguonTuyenDung data) =>
            Ok(func.SetDanhMucNguonTuyenDung(data, db));

        [HttpPost("DeleteDanhMucNguonTuyenDung")]
        public IActionResult DeleteDanhMucNguonTuyenDung([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucNguonTuyenDung(data, db));

        #endregion

        #region  Xếp loại

        [HttpGet("GetDanhSachXepLoai")]
        public IActionResult GetDanhSachXepLoai() => Ok(func.GetDanhSachXepLoai(db));

        [HttpPost("UpdateDanhMucXepLoai")]
        public IActionResult UpdateDanhMucXepLoai([FromBody] HRM_dmXepLoai data) =>
            Ok(func.UpdateDanhMucXepLoai(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucXepLoai")]
        public IActionResult DeleteDanhMucXepLoai([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucXepLoai(data, CurrentUserId, db));

        #endregion

        #region  Loại hợp đồng

        [HttpGet("GetDanhSachLoaiHopDong")]
        public IActionResult GetDanhSachLoaiHopDong(bool isPhuLuc=false) => Ok(func.GetDanhSachLoaiHopDong(db, isPhuLuc));

        [HttpPost("UpdateDanhMucLoaiHopDong")]
        public IActionResult UpdateDanhMucLoaiHopDong([FromBody] HRM_dmLoaiHopDong data) =>
            Ok(func.UpdateDanhMucLoaiHopDong(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucLoaiHopDong")]
        public IActionResult DeleteDanhMucLoaiHopDong([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucLoaiHopDong(data, CurrentUserId, db));

        #endregion

        #region  Thời hạn hợp đồng

        [HttpGet("GetDanhSachThoiHanHopDong")]
        public IActionResult GetDanhSachThoiHanHopDong() => Ok(func.GetDanhSachThoiHanHopDong(db));

        [HttpPost("UpdateDanhMucThoiHanHopDong")]
        public IActionResult UpdateDanhMucThoiHanHopDong([FromBody] HRM_dmThoiHanHopDong data) =>
            Ok(func.UpdateDanhMucThoiHanHopDong(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucThoiHanHopDong")]
        public IActionResult DeleteDanhMucThoiHanHopDong([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucThoiHanHopDong(data, CurrentUserId, db));

        #endregion

        #region  Hình thức trả lương

        [HttpGet("GetDanhSachHinhThucTraLuong")]
        public IActionResult GetDanhSachHinhThucTraLuong() => Ok(func.GetDanhSachHinhThucTraLuong(db));

        [HttpPost("UpdateDanhMucHinhThucTraLuong")]
        public IActionResult UpdateDanhMucHinhThucTraLuong([FromBody] HRM_dmHinhThucTraLuong data) =>
            Ok(func.UpdateDanhMucHinhThucTraLuong(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucHinhThucTraLuong")]
        public IActionResult DeleteDanhMucHinhThucTraLuong([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucHinhThucTraLuong(data, CurrentUserId, db));

        #endregion

        #region  Nhóm chứng chỉ

        [HttpGet("GetDanhSachNhomChungChi")]
        public IActionResult GetDanhSachNhomChungChi() => Ok(func.GetDanhSachNhomChungChi(db));

        [HttpPost("UpdateDanhMucNhomChungChi")]
        public IActionResult UpdateDanhMucNhomChungChi([FromBody] HRM_dmNhomChungChi data) =>
            Ok(func.UpdateDanhMucNhomChungChi(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucNhomChungChi")]
        public IActionResult DeleteDanhMucNhomChungChi([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucNhomChungChi(data, CurrentUserId, db));

        #endregion

        //#region Bảo hiểm bắt buộc

        //[HttpGet]
        //[Route("GetDanhSachBaoHiemBatBuoc")]
        //public IActionResult GetDanhSachBaoHiemBatBuoc()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.GetDanhSachBaoHiemBatBuoc(db);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("UpdateDanhMucBaoHiemBatBuoc")]
        //public IActionResult UpdateDanhMucBaoHiemBatBuoc(HRM_dmBaoHiemBatBuoc data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.UpdateDanhMucBaoHiemBatBuoc(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucBaoHiemBatBuoc")]
        //public IActionResult DeleteDanhMucBaoHiemBatBuoc(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.DeleteDanhMucBaoHiemBatBuoc(data, IdCurrentUser, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //#endregion

        #region Cài đặt hệ thống

        [HttpGet("GetDanhSachCaiDatHeThong")]
        public IActionResult GetDanhSachCaiDatHeThong() => Ok(func.GetDanhSachCaiDatHeThong(db));

        [HttpPost("SetDanhMucCaiDatHeThong")]
        public IActionResult SetDanhMucCaiDatHeThong([FromBody] HRM_dmCaiDatHeThong aModel) =>
            Ok(func.SetDanhMucCaiDatHeThong(aModel, db));

        [HttpPost("DeleteDanhMucCaiDatHeThong")]
        public IActionResult DeleteDanhMucCaiDatHeThong([FromBody] List<string> data) => Ok(func.DeleteCaiDatHeThong(data, db));

        #endregion

        #region Tỉnh

        [HttpGet("GetDanhSachTinh")]
        public IActionResult GetDanhSachTinh(string KeyWord, int CurrentPage = 0, int NumperPage = 25) =>
            Ok(func.GetDanhSachTinh(KeyWord, CurrentPage, NumperPage, db));

        [HttpPost("UpdateDanhMucTinh")]
        public IActionResult UpdateDanhMucTinh([FromBody] HRM_dmTinh data) => Ok(func.UpdateDanhMucTinh(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucTinh")]
        public IActionResult DeleteDanhMucTinh([FromBody] List<string> data) => Ok(func.DeleteDanhMucTinh(data, CurrentUserId, db));

        #endregion

        #region Quận huyện

        [HttpGet("GetDanhSachQuanHuyen")]
        public IActionResult GetDanhSachQuanHuyen(string KeyWord, int CurrentPage = 0, int NumperPage = 25, string IdTinh = "") =>
            Ok(func.GetDanhSachQuanHuyen(KeyWord, IdTinh, CurrentPage, NumperPage, db));

        [HttpGet("GetDanhMucQuanHuyen")]
        public IActionResult GetDanhMucQuanHuyen(string Id) => Ok(func.GetDanhMucQuanHuyen(Id, db));

        [HttpGet("ExportDanhMucPhuongXa")]
        public IActionResult ExportDanhMucPhuongXa() => Ok(func.ExportDanhMucPhuongXa( db));

        [HttpGet("ImportDanhMucPhuongXa")]
        public IActionResult ImportDanhMucPhuongXa(string FileName) => Ok(func.ImportDanhMucPhuongXa( db, FileName));

        [HttpGet("ExportDanhMucTinh")]
        public IActionResult ExportDanhMucTinh() => Ok(func.ExportDanhMucTinh( db));

        [HttpGet("ImportDanhMucTinh")]
        public IActionResult ImportDanhMucTinh(string FileName) => Ok(func.ImportDanhMucTinh( db, FileName));

        [HttpGet("ExportDanhMucQuanHuyen")]
        public IActionResult ExportDanhMucQuanHuyen() => Ok(func.ExportDanhMucQuanHuyen( db));

        [HttpGet("ImportDanhMucQuanHuyen")]
        public IActionResult ImportDanhMucQuanHuyen(string FileName) => Ok(func.ImportDanhMucQuanHuyen( db, FileName));

        [HttpPost("UpdateDanhMucQuanHuyen")]
        public IActionResult UpdateDanhMucQuanHuyen([FromBody] HRM_dmQuanHuyen data) =>
            Ok(func.UpdateDanhMucQuanHuyen(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucQuanHuyen")]
        public IActionResult DeleteDanhMucQuanHuyen([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucQuanHuyen(data, CurrentUserId, db));

        #endregion

        #region Phường xã

        [HttpGet("GetDanhSachPhuongXa")]
        public IActionResult GetDanhSachPhuongXa(string KeyWord, int CurrentPage = 0, int NumperPage = 25, string IdQuanHuyen = "") =>
            Ok(func.GetDanhSachPhuongXa(KeyWord, IdQuanHuyen, CurrentPage, NumperPage, db));

        [HttpPost("UpdateDanhMucPhuongXa")]
        public IActionResult UpdateDanhMucPhuongXa([FromBody] HRM_dmPhuongXa data) =>
            Ok(func.UpdateDanhMucPhuongXa(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucPhuongXa")]
        public IActionResult DeleteDanhMucPhuongXa([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucPhuongXa(data, CurrentUserId, db));

        #endregion

        #region Loại tính thuế chuyên gia

        [HttpGet("GetDanhSachLoaiTinhThueChuyenGia")]
        public IActionResult GetDanhSachLoaiTinhThueChuyenGia() => Ok(func.GetDanhSachLoaiTinhThueChuyenGia(db));

        [HttpPost("UpdateDanhMucLoaiTinhThueChuyenGia")]
        public IActionResult UpdateDanhMucLoaiTinhThueChuyenGia([FromBody] HRM_dmLoaiTinhThueChuyenGia data) =>
            Ok(func.UpdateDanhMucLoaiTinhThueChuyenGia(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucLoaiTinhThueChuyenGia")]
        public IActionResult DeleteDanhMucLoaiTinhThueChuyenGia([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucLoaiTinhThueChuyenGia(data, CurrentUserId, db));

        #endregion

        #region Danh mục đóng thuế TNCN

        [HttpGet("GetDanhSachDanhMucDongThueTNCN")]
        public IActionResult GetDanhSachDanhMucDongThueTNCN() => Ok(func.GetDanhSachDanhMucDongThueTNCN(db));

        [HttpPost("SetDanhMucDongThueTNCN")]
        public IActionResult SetDanhMucDongThueTNCN([FromBody] HRM_dmMucDongThueTNCN data) =>
            Ok(func.SetDanhMucDongThueTNCN(data, db));

        [HttpPost("DeleteDanhMucDongThueTNCN")]
        public IActionResult DeleteDanhMucDongThueTNCN([FromBody] List<string> data) => Ok(func.DeleteDanhMucDongThueTNCN(data, db));

        #endregion

        #region Danh mục tài sản bàn giao

        [HttpGet("GetDanhMucTaiSanBanGiao")]
        public IActionResult GetDanhMucTaiSanBanGiao() => Ok(func.GetListDanhMucTrangThietBi(db));

        [HttpPost("UpdateDanhMucTaiSanBanGiao")]
        public IActionResult UpdateDanhMucTaiSanBanGiao([FromBody] HRM_dmTaiSanBanGiao data) =>
            Ok(func.UpdateDanhMucTaiSanBanGiao(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucTaiSanBanGiao")]
        public IActionResult DeleteDanhMucTaiSanBanGiao([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucTaiSanBanGiao(data, CurrentUserId, db));

        #endregion

        #region Danh file đính kèm

        [HttpGet("GetDanhSachHoSoDinhKem")]
        public IActionResult GetDanhSachHoSoDinhKem() => Ok(func.GetDanhSachHoSoDinhKem(db));

        [HttpPost("UpdateDanhMucHoSoDinhKem")]
        public IActionResult UpdateDanhMucHoSoDinhKem([FromBody] HRM_HoSoDinhKem data) =>
            Ok(func.UpdateDanhMucHoSoDinhKem(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucHoSoDinhKem")]
        public IActionResult DeleteDanhMucHoSoDinhKem([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucHoSoDinhKem(data, CurrentUserId, db));

        #endregion

        #region Danh MucPhuCap

        [HttpGet("GetDanhMucPhuCap")]
        public IActionResult GetDanhMucPhuCap() => Ok(func.GetDanhMucPhuCap(db));

        [HttpPost("UpdateDanhMucPhuCap")]
        public IActionResult UpdateDanhMucPhuCap([FromBody] HRM_dmPhuCap data) =>
            Ok(func.UpdateDanhMucPhuCap(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucPhuCap")]
        public IActionResult DeleteDanhMucPhuCap([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucPhuCap(data, CurrentUserId, db));

        #endregion

        #region Danh mục trạng thái  hợp đòng

        [HttpGet("GetDanhMucTrangThaiHopDong")]
        public IActionResult GetDanhMucTrangThaiHopDong() => Ok(func.GetDanhMucTrangThaiHopDong(db));

        [HttpPost("UpdatetDanhMucTrangThaiHopDong")]
        public IActionResult UpdatetDanhMucTrangThaiHopDong([FromBody] HRM_dmTrangThaiHopDong data) =>
            Ok(func.UpdatetDanhMucTrangThaiHopDong(data, CurrentUserId, db));

        [HttpPost("DeletetDanhMucTrangThaiHopDong")]
        public IActionResult DeletetDanhMucTrangThaiHopDong([FromBody] List<string> data) =>
            Ok(func.DeletetDanhMucTrangThaiHopDong(data, CurrentUserId, db));

        #endregion

        #region Danh mục nhóm lương

        [HttpGet("GetDanhNhomLuong")]
        public IActionResult GetDanhNhomLuong() => Ok(func.GetDanhNhomLuong(db));

        [HttpPost("UpdateDanhMucNhomLuong")]
        public IActionResult UpdateDanhMucNhomLuong([FromBody] HRM_dmNhomLuong data) =>
            Ok(func.UpdateDanhMucNhomLuong(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucNhomLuong")]
        public IActionResult DeleteDanhMucNhomLuong([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucNhomLuong(data, CurrentUserId, db));

        #endregion

        #region Danh mục nhóm phúc lợi

        [HttpGet("GetDanhSachDanhMucNhomPhucLoi")]
        public IActionResult GetDanhSachDanhMucNhomPhucLoi() => Ok(func.GetDanhSachDanhMucNhomPhucLoi(db));

        [HttpPost("SetDanhSachDanhMucNhomPhucLoi")]
        public IActionResult SetDanhSachDanhMucNhomPhucLoi([FromBody] HRM_dmNhomPhucLoi data) =>
            Ok(func.SetDanhMucNhomPhucLoi(data, db));

        [HttpPost("DeleteDanhMucNhomPhucLoi")]
        public IActionResult DeleteDanhMucNhomPhucLoi([FromBody] List<string> data) => Ok(func.DeleteDanhMucNhomPhucLoi(data, db));

        #endregion

        #region danh mục cơ sở

        [HttpGet("GetListdmCoSo")]
        public IActionResult GetListdmCoSo() => Ok(_CallWebService.GetDanhSachBoPhan(db));

        #endregion

        #region Danh mục chức vụ công việc

        [HttpGet("GetListDanhMucChucVuCongViec")]
        public IActionResult GetListDanhMucChucVuCongViec() => Ok(func.GetListDanhMucChucVuCongViec(db));

        [HttpPost("SetDanhMucChucVuCongViec")]
        public IActionResult SetDanhMucChucVuCongViec([FromBody] HRM_dmChucVuCongViec data) =>
            Ok(func.SetDanhMucChucVuCongViec(data, db));

        [HttpPost("DeleteDanhMucChucVuCongViec")]
        public IActionResult DeleteDanhMucChucVuCongViec([FromBody] List<string> data) =>
            Ok(func.DeleteDanhMucChucVuCongViec(data, db));

        #endregion

        #region Danh mục chức vụ

        //[HttpGet]
        //[Route("GetListDanhMucChucVu")]
        //public IActionResult GetListDanhMucChucVu()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.GetListDanhMucChucVu(db);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("SetDanhMucChucVu")]
        //public IActionResult SetDanhMucChucVu(HRM_dmChucVu data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.SetDanhMucChucVu(data, db);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("DeleteDanhMucChucVu")]
        //public IActionResult DeleteDanhMucChucVu(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.DeleteDanhMucChucVu(data, db);
        //    return Ok(item);
        //}

        #endregion

        #region Danh mục chức vụ kiểm soát nội bộ

        [HttpPost("GetListdmChucVuKiemSoatNoiBo")]
        public IActionResult GetListdmChucVuKiemSoatNoiBo([FromBody] dynamic dnm) => Ok(func.GetListdmChucVuKiemSoatNoiBo(db, dnm));

        [HttpPost("SetdmChucVuKiemSoatNoiBo")]
        public IActionResult SetdmChucVuKiemSoatNoiBo([FromBody] HRM_dmChucVuKiemSoatNoiBo data) =>
            Ok(func.SetdmChucVuKiemSoatNoiBo(data, db));

        [HttpGet("DeletedmChucVuKiemSoatNoiBo")]
        public IActionResult DeletedmChucVuKiemSoatNoiBo(string Id) => Ok(func.DeletedmChucVuKiemSoatNoiBo(Id, db));

        #endregion
        #region Danh mục chức vụ kiểm soát nội bộ

        [HttpPost("GetListdmNganHang")]
        public IActionResult GetListdmNganHang([FromBody] dynamic dnm) => Ok(func.GetListdmNganHang(db, dnm));

        [HttpGet("GetListdmNganHangICT")]
        public IActionResult GetListdmNganHangICT() => Ok(func.GetListdmNganHangICT(db));

        [HttpPost("SetdmNganHang")]
        public IActionResult SetdmNganHang([FromBody] HRM_dmNganHang data) => Ok(func.SetdmNganHang(data, db));

        [HttpGet("DeletedmNganHang")]
        public IActionResult DeletedmNganHang(string Id) => Ok(func.DeletedmNganHang(Id, db));

        #endregion

        #region Danh mục trang thiết bị

        [HttpGet("GetListDanhMucTrangThietBi")]
        public IActionResult GetListDanhMucTrangThietBi() => Ok(func.GetListDanhMucTrangThietBi(db));

        [HttpPost("SetDanhMucTrangThietBi")]
        public IActionResult SetDanhMucTrangThietBi([FromBody] HRM_dmTrangThietBi data) => Ok(func.SetDanhMucTrangThietBi(data, db));

        [HttpPost("DeleteDanhMucTrangThietBi")]
        public IActionResult DeleteDanhMucTrangThietBi([FromBody] List<string> data) => Ok(func.DeleteDanhMucTrangThietBi(data, db));

        #endregion
        #region Danh mục nôi dung đào tạo

        [HttpGet("GetListdmNoiDungDaoTao")]
        public IActionResult GetListdmNoiDungDaoTao() => Ok(func.GetListdmNoiDungDaoTao(db));

        [HttpPost("SetdmNoiDungDaoTao")]
        public IActionResult SetdmNoiDungDaoTao([FromBody] HRM_dmNoiDungDaoTao data) => Ok(func.SetdmNoiDungDaoTao(data, db));

        [HttpGet("DeletedmNoiDungDaoTao")]
        public IActionResult DeletedmNoiDungDaoTao(string Id) => Ok(func.DeletedmNoiDungDaoTao(Id, db));

        #endregion

        #region Danh mục hình thức đào tạo

        [HttpGet("GetListdmHinhThucDaoTao")]
        public IActionResult GetListdmHinhThucDaoTao() => Ok(func.GetListdmHinhThucDaoTao(db));

        [HttpPost("SetdmHinhThucDaoTao")]
        public IActionResult SetdmHinhThucDaoTao([FromBody] HRM_dmHinhThucDaoTao data) => Ok(func.SetdmHinhThucDaoTao(data, db));

        [HttpPost("DeletedmHinhThucDaoTao")]
        public IActionResult DeletedmHinhThucDaoTao([FromBody] List<string> listId) => Ok(func.DeletedmHinhThucDaoTao(listId, db));

        #endregion
        #region Ngân hàng câu hỏi

        [HttpPost("GetListTieuChi")]
        public IActionResult GetListTieuChi([FromBody] dynamic dnm) => Ok(func.GetListTieuChi(db, dnm));

        [HttpPost("SetTieuChi")]
        public IActionResult SetTieuChi([FromBody] HRM_dmTieuChi model) => Ok(func.SetTieuChi(db, model));

        [HttpGet("DeleteTieuChi")]
        public IActionResult DeleteTieuChi(string Id) => Ok(func.DeleteTieuChi(db, Id));

        #endregion

        #region Đợt chấm công

        [HttpPost("GetListDanhMucDotChamCong")]
        public IActionResult GetListDanhMucDotChamCong([FromBody] dynamic dnm) => Ok(func.GetListDanhMucDotChamCong(db));

        #endregion
        #region Danh mục nhân viên thực hiện chấm công

        //[HttpPost]
        //[Route("GetDanhSachUserThucHienChamCong")]
        //public IActionResult GetDanhSachUserThucHienChamCong(dynamic dnm)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        FuncDanhMuc func = new FuncDanhMuc();
        //        var item = func.GetDanhSachUserThucHienChamCong(dnm, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));

        //}

        [HttpPost("UpdateUserThucHienChamCong")]
        public IActionResult UpdateUserThucHienChamCong([FromBody] EntityHRM_DanhMuc_NhanVien_ChamCongBoPhan dnm) =>
            Ok(func.UpdateUserThucHienChamCong(dnm, db));

        [HttpPost("SetDanhMucCauHinhChamCongHRM")]
        public IActionResult SetDanhMucCauHinhChamCongHRM([FromBody] HRM_DanhMuc_NhanVien_ChamCongBoPhan dnm) =>
            Ok(func.SetDanhMucCauHinhChamCongHRM(dnm, db));

        [HttpGet("GetDanhMucCauHinhChamCongHRM")]
        public IActionResult GetDanhMucCauHinhChamCongHRM(long IdCongTyEOS) => Ok(func.GetDanhMucCauHinhChamCongHRM(IdCongTyEOS, db));

        [HttpGet("DeleteDanhMucCauHinhChamCongHRM")]
        public IActionResult DeleteDanhMucCauHinhChamCongHRM(string Id) => Ok(func.DeleteDanhMucCauHinhChamCongHRM(Id, db));

        [HttpGet("GetDanhSachBoPhanChamCongHRMByUser")]
        public IActionResult GetDanhSachBoPhanChamCongHRMByUser(long IdCongTyEOS)
        {
            var item = _CallWebService.GetDanhSachBoPhanTheoCongTy(db, IdCongTyEOS);
            

            //var item = func.GetDanhSachBoPhanChamCongHRMByUser(IdCongTyEOS, db);
            return Ok(item);
        }

        [HttpGet("GetDanhSachBoPhanChamCongByUser")]
        public IActionResult GetDanhSachBoPhanChamCongByUser(long IdCongTyEOS, string IdUser) =>
            Ok(func.GetDanhSachBoPhanChamCongByUser(IdCongTyEOS, IdUser, db));

        #endregion

        #region Ký hiệu chấm công

        [HttpPost("GetListdmKyHieuChamCong")]
        public IActionResult GetListdmKyHieuChamCong([FromBody] dynamic dnm) => Ok(func.GetListdmKyHieuChamCong(db, dnm));

        [HttpPost("SetdmKyHieuChamCong")]
        public IActionResult SetdmKyHieuChamCong([FromBody] HRM_dmKyHieuChamCong entity) => Ok(func.SetdmKyHieuChamCong(entity, db));

        [HttpGet("DeletedmKyHieuChamCong")]
        public IActionResult DeletedmKyHieuChamCong(string Id) => Ok(func.DeletedmKyHieuChamCong(Id, db));

        #endregion
        #region Lịch nghỉ lễ

        [HttpPost("GetListdmLichNghiLe")]
        public IActionResult GetListdmLichNghiLe([FromBody] dynamic dnm) => Ok(func.GetListdmLichNghiLe(db, dnm));

        [HttpPost("SetdmLichNghiLe")]
        public IActionResult SetdmLichNghiLe([FromBody] HRM_dmLichNghiLe entity) => Ok(func.SetdmLichNghiLe(entity, db));

        [HttpGet("DeletedmLichNghiLe")]
        public IActionResult DeletedmLichNghiLe(string Id) => Ok(func.DeletedmLichNghiLe(Id, db));

        [HttpGet("GetdmLichNghiLe")]
        public IActionResult GetdmLichNghiLe(string Id) => Ok(func.GetdmLichNghiLe(Id, db));

        #endregion
        #region Lịch làm việc

        [HttpPost("GetListdmLichLamViec")]
        public IActionResult GetListdmLichLamViec([FromBody] dynamic dnm) => Ok(func.GetListdmLichLamViec(db, dnm));

        [HttpPost("SetdmLichLamViec")]
        public IActionResult SetdmLichLamViec([FromBody] HRM_dmLichLamViec entity) => Ok(func.SetdmLichLamViec(entity, db));

        [HttpGet("DeletedmLichLamViec")]
        public IActionResult DeletedmLichLamViec(string Id) => Ok(func.DeletedmLichLamViec(Id, db));

        [HttpPost("GetdmLichLamViecByListId")]
        public IActionResult GetdmLichLamViecByListId([FromBody] List<string> Id) => Ok(func.GetdmLichLamViecByListId(Id, db));

        [HttpGet("GetdmLichLamViec")]
        public IActionResult GetdmLichLamViec(string Id) => Ok(func.GetdmLichLamViec(Id, db));

        #endregion

        [HttpGet("GetListDanhMucHinhThucNghi")]
        public IActionResult GetListDanhMucHinhThucNghi() => Ok(func.GetListDanhMucHinhThucNghi(db));

        #region Thư viện đánh giá

        #region Danh mục chức vụ kiểm soát nội bộ

        [HttpPost("GetListdmNhomDanhGia")]
        public IActionResult GetListdmNhomDanhGia([FromBody] dynamic dnm) => Ok(func.GetListdmNhomDanhGia(db, dnm));

        [HttpPost("SetdmNhomDanhGia")]
        public IActionResult SetdmNhomDanhGia([FromBody] HRM_dmNhomDanhGia data) => Ok(func.SetdmNhomDanhGia(data, db));

        [HttpGet("DeletedmNhomDanhGia")]
        public IActionResult DeletedmNhomDanhGia(string Id) => Ok(func.DeletedmNhomDanhGia(db, Id));

        //[HttpPost]
        //[Route("DeleteDanhMucChucVu")]
        //public IActionResult DeleteDanhMucChucVu(List<string> data)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.DeleteDanhMucChucVu(data, db);
        //    return Ok(item);
        //}

        #endregion

        [HttpGet("GetListCachDanhGia")]
        public IActionResult GetListCachDanhGia() => Ok(func.GetListCachDanhGia());

        [HttpPost("SetThuVienDanhGia")]
        public IActionResult SetThuVienDanhGia([FromBody] HRM_ThuVienDanhGia entity) => Ok(func.SetThuVienDanhGia(db, entity));

        [HttpGet("GetThuVienDanhGia")]
        public IActionResult GetThuVienDanhGia(string Id, bool CheckQuyen = false) => Ok(func.GetThuVienDanhGia(db, Id, CheckQuyen));

        [HttpPost("DeleteThuVienDanhGia")]
        public IActionResult DeleteThuVienDanhGia([FromBody] List<string> listId) => Ok(func.DeleteThuVienDanhGia(db, listId));

        [HttpPost("GetListThuVienDanhGia")]
        public IActionResult GetListThuVienDanhGia([FromBody] TimKiem timKiem) => Ok(func.GetListThuVienDanhGia(db, timKiem));

        [HttpGet("ImportThuVienDanhGia")]
        public IActionResult ImportThuVienDanhGia(string FileName) => Ok(func.ImportThuVienDanhGia(db, FileName));

        [HttpGet("UpdateThuVienTieuChi")]
        public IActionResult UpdateThuVienTieuChi()
        {
            func.UpdateThuVienTieuChi(db);
            return Ok("OK");
        }

        [HttpGet("ImportQUyTrinhXuLy")]
        public IActionResult ImportQUyTrinhXuLy(string FileName) => Ok(func.ImportQUyTrinhXuLy(db, FileName));

        [HttpGet("ExportThuVienDanhGia")]
        public IActionResult ExportThuVienDanhGia(string Id) => Ok(func.ExportThuVienDanhGia(db, Id));

        #endregion

        #region cấp bậc lương

        [HttpPost("SetdmCapBacLuong")]
        public IActionResult SetdmCapBacLuong([FromBody] HRM_dmCapBacLuong entity) => Ok(func.SetdmCapBacLuong(entity, db));

        [HttpGet("GetdmCapBacLuong")]
        public IActionResult GetdmCapBacLuong(string Id) => Ok(func.GetdmCapBacLuong(Id, db));

        [HttpPost("DeletedmCapBacLuong")]
        public IActionResult DeletedmCapBacLuong([FromBody] List<string> listId) => Ok(func.DeletedmCapBacLuong(listId, db));

        [HttpPost("GetListdmCapBacLuong")]
        public IActionResult GetListdmCapBacLuong([FromBody] TimKiem timKiem) => Ok(func.GetListdmCapBacLuong(timKiem, db));

        #endregion
        #region Bộ Phận chức vụ

        [HttpGet("GetDanhSachBoPhan_ChucDanh")]
        public IActionResult GetDanhSachBoPhan_ChucDanh() => Ok(func.GetDanhSachBoPhan_ChucDanh(db));

        [HttpPost("UpdateDanhMucBoPhan_ChucDanh")]
        public IActionResult UpdateDanhMucBoPhan_ChucDanh([FromBody] HRM_BoPhan_ChucDanhViewModel data) =>
            Ok(func.UpdateDanhMucBoPhan_ChucDanh(data, CurrentUserId, db));

        [HttpPost("DeleteDanhMucBoPhan_ChucDanh")]
        public IActionResult DeleteDanhMucBoPhan_ChucDanh([FromBody] List<long> data) =>
            Ok(func.DeleteDanhMucBoPhan_ChucDanh(data, CurrentUserId, db));

        [HttpGet("GetDanhSachChucDanhTheoIdBoPhan")]
        public IActionResult GetDanhSachChucDanhTheoIdBoPhan(long IdBoPhan) => Ok(func.GetDanhSachChucDanhTheoIdBoPhan(IdBoPhan, db));

        [HttpGet("GetDanhSachBoPhanDanhGia")]
        public IActionResult GetDanhSachBoPhanDanhGia()
        {
            List<DanhMuc> listDanhMuc = new List<DanhMuc>();
            listDanhMuc.Add(new DanhMuc
            {
            Ma = DefineData.eBoPhanDanhGia.KETOAN.ToString(),
            Ten = "Kế toán"
            });
            listDanhMuc.Add(new DanhMuc
            {
            Ma = DefineData.eBoPhanDanhGia.HCNS.ToString(),
            Ten = "Hành chính nhân sự"
            });
            return Ok(listDanhMuc);
        }

        #endregion
        #region ngân hàng câu hỏi

        [HttpPost("GetListdmNhomCauHoi")]
        public IActionResult GetListdmNhomCauHoi([FromBody] dynamic dnm) => Ok(func.GetListdmNhomCauHoi(db, dnm));

        [HttpPost("SetdmNhomCauHoi")]
        public IActionResult SetdmNhomCauHoi([FromBody] HRM_dmNhomCauHoi data) => Ok(func.SetdmNhomCauHoi(data, db));

        [HttpGet("DeletedmNhomCauHoi")]
        public IActionResult DeletedmNhomCauHoi(string Id) => Ok(func.DeletedmNhomCauHoi(Id, db));

        [HttpPost("ExportdmNhomCauHoi")]
        public IActionResult ExportdmNhomCauHoi([FromBody] List<string> listDanhMuc) => Ok(func.ExportdmNhomCauHoi(db, listDanhMuc));

        [HttpGet("ImportdmNhomCauHoi")]
        public IActionResult ImportdmNhomCauHoi(string FileName) => Ok(func.ImportdmNhomCauHoi(db, FileName));

        [HttpPost("GetdmCauHoi")]
        public IActionResult GetListdmCauHoi([FromBody] dynamic dnm) => Ok(func.GetListdmCauHoi(db, dnm));

        [HttpPost("SetdmCauHoi")]
        public IActionResult SetdmCauHoi([FromBody] HRM_dmCauHoi data) => Ok(func.SetdmCauHoi(data, db));

        [HttpGet("DeletedmCauHoi")]
        public IActionResult DeletedmCauHoi(string Id) => Ok(func.DeletedmCauHoi(Id, db));

        [HttpGet("GetListLoaiCauHoi")]
        public IActionResult GetListLoaiCauHoi() => Ok(func.GetListLoaiCauHoi());

        [HttpPost("ExportdmCauHoi")]
        public IActionResult ExportdmCauHoi([FromBody] List<HRM_dmCauHoi> listDanhMuc) => Ok(func.ExportdmCauHoi(db, listDanhMuc));

        [HttpGet("ImportdmCauHoi")]
        public IActionResult ImportdmCauHoi(string FileName) => Ok(func.ImportdmCauHoi(db, FileName));

        [HttpPost("GetdmCauTrucDe")]
        public IActionResult GetListdmCauTrucDe([FromBody] dynamic dnm) => Ok(func.GetListdmCauTrucDe(db, dnm));

        [HttpPost("SetdmCauTrucDe")]
        public IActionResult SetdmCauTrucDe([FromBody] HRM_dmCauTrucDe data) => Ok(func.SetdmCauTrucDe(data, db));

        [HttpGet("DeletedmCauTrucDe")]
        public IActionResult DeletedmCauTrucDe(string Id) => Ok(func.DeletedmCauTrucDe(Id, db));

        [HttpPost("GetListdmPhuongTien")]
        public IActionResult GetListdmPhuongTien([FromBody] dynamic dnm) => Ok(func.GetListdmPhuongTien(db, dnm));

        [HttpPost("SetdmPhuongTien")]
        public IActionResult SetdmPhuongTien([FromBody] HRM_dmPhuongTien data) => Ok(func.SetdmPhuongTien(data, db));

        [HttpGet("DeletedmPhuongTien")]
        public IActionResult DeletedmPhuongTien(string Id) => Ok(func.DeletedmPhuongTien(db, Id));

        [HttpPost("GetListdmLoaiChiPhiCongTac")]
        public IActionResult GetListdmLoaiChiPhiCongTac([FromBody] dynamic dnm) => Ok(func.GetListdmLoaiChiPhiCongTac(db, dnm));

        [HttpPost("SetdmLoaiChiPhiCongTac")]
        public IActionResult SetdmLoaiChiPhiCongTac([FromBody] HRM_dmLoaiChiPhiCongTac data) =>
            Ok(func.SetdmLoaiChiPhiCongTac(data, db));

        [HttpGet("DeletedmLoaiChiPhiCongTac")]
        public IActionResult DeletedmLoaiChiPhiCongTac(string Id) => Ok(func.DeletedmLoaiChiPhiCongTac(db, Id));

        [HttpPost("GetListdmHeSoLamThemGio")]
        public IActionResult GetListdmHeSoLamThemGio([FromBody] dynamic dnm) => Ok(func.GetListdmHeSoLamThemGio(db, dnm));

        [HttpPost("SetdmHeSoLamThemGio")]
        public IActionResult SetdmHeSoLamThemGio([FromBody] HRM_dmHeSoLamThemGio data) => Ok(func.SetdmHeSoLamThemGio(data, db));

        [HttpGet("DeletedmHeSoLamThemGio")]
        public IActionResult DeletedmHeSoLamThemGio(string Id) => Ok(func.DeletedmHeSoLamThemGio(db, Id));

        [HttpPost("GetListdmQuyDinhCongTacPhi")]
        public IActionResult GetListdmQuyDinhCongTacPhi([FromBody] dynamic dnm) => Ok(func.GetListdmQuyDinhCongTacPhi(db, dnm));

        [HttpPost("SetdmQuyDinhCongTacPhi")]
        public IActionResult SetdmQuyDinhCongTacPhi([FromBody] HRM_DanhMucCongTacPhiViewModel data) =>
            Ok(func.SetdmQuyDinhCongTacPhi(data, db));

        [HttpGet("DeletedmQuyDinhCongTacPhi")]
        public IActionResult DeletedmQuyDinhCongTacPhi(long IdchucVu) => Ok(func.DeletedmQuyDinhCongTacPhi(db, IdchucVu));

        [HttpPost("GetListdmNoiCap")]
        public IActionResult GetListdmNoiCap([FromBody] dynamic dnm) => Ok(func.GetListdmNoiCap(db, dnm));

        [HttpPost("SetdmNoiCap")]
        public IActionResult SetdmNoiCap([FromBody] HRM_dmNoiCap data) => Ok(func.SetdmNoiCap(data, db));

        [HttpGet("DeletedmNoiCap")]
        public IActionResult DeletedmNoiCap(string Id) => Ok(func.DeletedmNoiCap(db, Id));

        [HttpPost("GetListdmNoiCapBaoHiem")]
        public IActionResult GetListdmNoiCapBaoHiem([FromBody] dynamic dnm) => Ok(func.GetListdmNoiCapBaoHiem(db, dnm));

        [HttpPost("SetdmNoiCapBaoHiem")]
        public IActionResult SetdmNoiCapBaoHiem([FromBody] HRM_dmNoiCapBaoHiem data) => Ok(func.SetdmNoiCapBaoHiem(data, db));

        [HttpGet("DeletedmNoiCapBaoHiem")]
        public IActionResult DeletedmNoiCapBaoHiem(string Id) => Ok(func.DeletedmNoiCapBaoHiem(db, Id));

        [HttpPost("GetListdmThietLapBangLuong")]
        public IActionResult GetListdmThietLapBangLuong([FromBody] dynamic dnm) => Ok(func.GetListdmThietLapBangLuong(db, dnm));

        [HttpPost("SetdmThietLapBangLuong")]
        public IActionResult SetdmThietLapBangLuong([FromBody] HRM_dmThietLapBangLuong data) =>
            Ok(func.SetdmThietLapBangLuong(data, db));

        [HttpGet("DeletedmThietLapBangLuong")]
        public IActionResult DeletedmThietLapBangLuong(string Id) => Ok(func.DeletedmThietLapBangLuong(db, Id));

        [HttpGet("GetdmThietLapBangLuong")]
        public IActionResult GetdmThietLapBangLuong(string Id = null) => Ok(func.GetdmThietLapBangLuong(db, Id));

        [HttpGet("ImPortDanhMuc")]
        public IActionResult ImPortDanhMuc(string filename)
        {
            func.ImPortDanhMuc(db, filename);
            return Ok("OK");
        }

        [HttpGet("ExportDanhMuc")]
        public IActionResult ExportDanhMuc()
        {
            func.ExportDanhMuc(db);
            return Ok("OK");
        }

        [HttpGet("ImportdmTinhPhuongXas")]
        public IActionResult ImportdmTinhPhuongXas(string filename)
        {
            func.ImportdmTinhPhuongXas(db, filename);
            return Ok("OK");
        }

        #endregion

        [HttpPost("GetListdmThietLapBaoCaoCongViec")]
        public IActionResult GetListdmThietLapBaoCaoCongViec([FromBody] dynamic dnm) =>
            Ok(func.GetListdmThietLapBaoCaoCongViec(db, dnm));

        [HttpPost("SetdmThietLapBaoCaoCongViec")]
        public IActionResult SetdmThietLapBaoCaoCongViec([FromBody] HRM_dmThietLapBaoCaoCongViec data) =>
            Ok(func.SetdmThietLapBaoCaoCongViec(data, db));

        [HttpGet("DeletedmThietLapBaoCaoCongViec")]
        public IActionResult DeletedmThietLapBaoCaoCongViec(string Id) => Ok(func.DeletedmThietLapBaoCaoCongViec(db, Id));

        [HttpGet("GetdmThietLapBaoCaoCongViec")]
        public IActionResult GetdmThietLapBaoCaoCongViec(string Id = null) => Ok(func.GetdmThietLapBaoCaoCongViec(db, Id));
    }
}
