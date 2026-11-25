#nullable disable
using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using HRM.Api.Services.ChamCong;
using HRM.Api.Entity;
using HRM.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using HRM.Api.Models.SmartEOEModel;
using System.Linq;
using static HRM.Api.Services.FuncBase;
using static HRM.Api.Helper.HelperConstant.LuongHopDong;
namespace HRM.Api.Controllers
{
    [Route("ChamCong")]
    public class ChamCongController(HrmDbContext db, funcChamCongVanPhong func, FuncChamCong funcChamCong) : ApiControllerBase
    {
        #region Xin đi sớm về muộn

        [HttpPost("GetListQuyTrinhXinDiMuonVeSom")]
        public IActionResult GetListQuyTrinhXinDiMuonVeSom([FromBody] dynamic data) =>
            Ok(func.GetListQuyTrinhXinDiMuonVeSom(db, data));

        [HttpGet("GetNextSoQuyTrinhXinDiMuonVeSom")]
        public IActionResult GetNextSoQuyTrinhXinDiMuonVeSom() => Ok(func.GetNextSoQuyTrinhXinDiMuonVeSom(db));

        [HttpPost("SetQuyTrinhXinDiMuonVeSom")]
        public IActionResult SetQuyTrinhXinDiMuonVeSom([FromBody] dynamic data) => Ok(func.SetQuyTrinhXinDiMuonVeSom(db, data));

        [HttpPost("UpdateQuyTrinhXinDiMuonVeSom")]
        public IActionResult UpdateQuyTrinhXinDiMuonVeSom([FromBody] HRM_QuyTrinhXinDiMuonVeSom entity) =>
            Ok(func.SetQuyTrinhXinDiMuonVeSom(db, entity));

        [HttpGet("GetQuyTrinhXinDiMuonVeSom")]
        public IActionResult GetQuyTrinhXinDiMuonVeSom(string Id) => Ok(func.GetQuyTrinhXinDiMuonVeSom(db, Id));

        [HttpPost("ChuyenTiepQuyTrinhXinDiMuonVeSom")]
        public IActionResult ChuyenTiepQuyTrinhXinDiMuonVeSom([FromBody] dynamic data) =>
            Ok(func.ChuyenTiepQuyTrinhXinDiMuonVeSom(db, data));

        [HttpPost("KhongDuyetQuyTrinhXinDiMuonVeSom")]
        public IActionResult KhongDuyetQuyTrinhXinDiMuonVeSom([FromBody] dynamic data) =>
            Ok(func.KhongDuyetQuyTrinhXinDiMuonVeSom(db, data));

        [HttpGet("DeleteQuyTrinhXinDiMuonVeSom")]
        public IActionResult DeleteQuyTrinhXinDiMuonVeSom(string Id) => Ok(func.DeleteQuyTrinhXinDiMuonVeSom(db, Id));

        #endregion

        //    #region Import file Chấm công nhân viên
        //    //[HttpPost]
        //    //[Route("ImportExcelChamCong")]

        //    //public HttpResponseMessage ImportExcelNhanVien()
        //    //{
        //    //    string DateFile = (DateTime.Now).ToString("yyyy-MM-dd-hh-mm-ss");
        //    //    var httpRequest = HttpContext.Current.Request;
        //    //    if (httpRequest.Files.Count > 0)
        //    //    {
        //    //        foreach (string fileName in httpRequest.Files.Keys)
        //    //        {
        //    //            var file = httpRequest.Files[fileName];
        //    //            var filePath = HttpContext.Current.Server.MapPath("~/" + "Upload/" + DateFile + "-BangChamCong.xlsx");
        //    //            file.SaveAs(filePath);

        //    //            var currentUser = getCurrentUser();

        //    //            string IdCurrentUser = currentUser.Id;
        //    //            db.IdUser = IdCurrentUser;
        //    //            db.currentUser = currentUser;

        //    //            var result = func.ImportBangChamCong(filePath, IdCurrentUser, db);
        //    //        }
        //    //        return Request.CreateResponse(new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công"));
        //    //    }

        //    //    return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    //}

        //    //[HttpGet]
        //    //[Route("ImportChamCong")]
        //    //public IActionResult ImportBangChamCong(string aFileName)
        //    //{
        //    //    var currentUser = getCurrentUser();
        //    //    if(currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    //    {
        //    //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //    }
        //    //    db.currentUser = currentUser;
        //    //    var result = func.ImportBangChamCong(aFileName, db);
        //    //    return Ok(result);
        //    //}

        //    //[HttpPost]
        //    //[Route("GetDanhSachBangLuongChamCong")]
        //    //public IActionResult GetDanhSachBangLuongChamCong(dynamic dyn)
        //    //{
        //    //    var currentUser = getCurrentUser();
        //    //    if (currentUser != null)
        //    //    {
        //    //        string IdCurrentUser = currentUser.Id;
        //    //        db.IdUser = IdCurrentUser;
        //    //        db.currentUser = currentUser;

        //    //        var result = func.GetDanhSachChamCong(dyn, IdCurrentUser, db);
        //    //        return Ok(result);

        //    //    }
        //    //    else
        //    //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //}

        //    //[HttpPost]
        //    //[Route("getChamCongChiTietNhanVien")]
        //    //public IActionResult GetChamCongChiTietNhanVien(dynamic dyn)
        //    //{
        //    //    var currentUser = getCurrentUser();
        //    //    if (currentUser != null)
        //    //    {
        //    //        string IdCurrentUser = currentUser.Id;
        //    //        db.IdUser = IdCurrentUser;
        //    //        db.currentUser = currentUser;

        //    //        var result = func.GetChiTietNgayCongNhanVien(dyn, IdCurrentUser, db);
        //    //        return Ok(result);

        //    //    }
        //    //    else
        //    //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //}

        //    //[HttpGet]
        //    //[Route("getLuongPhongBanBoPhan")]
        //    //public IActionResult getLuongPhongBanBoPhan(int Thang, int Nam)
        //    //{
        //    //    //var currentUser = getCurrentUser();
        //    //    //if (currentUser != null)
        //    //    //{
        //    //    //    string IdCurrentUser = currentUser.Id;
        //    //    //    db.IdUser = IdCurrentUser;
        //    //    //    db.currentUser = currentUser;

        //    //        var result = func.getLuongPhongBanBoPhan(Nam, Thang, db);
        //    //        return Ok(result);

        //    //    //}
        //    //    //else
        //    //    //    return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //}

        //    #endregion
        //    #region Chấm công new - dungnn

        //    [HttpPost]
        //    [Route("GetDanhSachChamCongDetail")]
        //    public IActionResult GetDanhSachChamCongDetail(EntityGetDanhSachChamCongDetailRequest dyn)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;

        //            var result = funcChamCong.GetDanhSachChamCongDetail(dyn, IdCurrentUser, db);
        //            return Ok(result);

        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        [HttpPost("GetDanhSachBangChamCongTongHop")]
        public IActionResult GetDanhSachBangChamCongTongHop([FromBody] dynamic dyn) =>
            Ok(funcChamCong.GetDanhSachBangChamCongTongHop(dyn, db));

        [HttpPost("GetDanhSachMaChamCongHangNgay")]
        public IActionResult GetDanhSachChamCongDetail([FromBody] dynamic dyn) =>
            Ok(funcChamCong.GetDanhSachMaChamCongHangNgay(dyn, CurrentUserId, db));

        [HttpGet("GetQuyTrinhChamCongDetail")]
        public IActionResult GetItemChamCongDetail(string Id) => Ok(funcChamCong.GetItemChamCongDetail(Id, CurrentUserId, db));

        //    [HttpPost]
        //    [Route("GetItemChamCongDetailToString")]
        //    public IActionResult GetItemChamCongDetailToString(dynamic dyn)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;

        //            var result = funcChamCong.GetItemChamCongDetailToString(dyn, IdCurrentUser, db);
        //            return Ok(result);

        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        [HttpGet("DeleteQuyTrinhChamCongDetail")]
        public IActionResult DeleteBangChamCongDetail(string Id) => Ok(funcChamCong.DeleteBangChamCongDetail(Id, db));

        [HttpPost("GetDanhSachBangChamCongTheoKy")]
        public IActionResult GetDanhSachBangChamCongTheoKy([FromBody] HRM_ChamCong_Detail model) =>
            Ok(funcChamCong.GetDanhSachBangChamCongTheoKy(model, db));

        //    //[HttpPost]
        //    //[Route("AddChamCongDetail")]
        //    //public IActionResult AddChamCongDetail(dynamic dyn)
        //    //{
        //    //    var currentUser = getCurrentUser();
        //    //    if (currentUser != null)
        //    //    {
        //    //        string IdCurrentUser = currentUser.Id;
        //    //        db.IdUser = IdCurrentUser;
        //    //        db.currentUser = currentUser;

        //    //        var result = funcChamCong.AddChamCongDetail(dyn, IdCurrentUser, db);
        //    //        return Ok(result);

        //    //    }
        //    //    else
        //    //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //}

        //    //[HttpPost]
        //    //[Route("UpdateChamCongDetail")]
        //    //public IActionResult UpdateChamCongDetail(HRM_ChamCong_Detail data)
        //    //{
        //    //    var currentUser = getCurrentUser();
        //    //    if (currentUser != null)
        //    //    {
        //    //        string IdCurrentUser = currentUser.Id;
        //    //        db.IdUser = IdCurrentUser;
        //    //        db.currentUser = currentUser;

        //    //        var result = funcChamCong.UpdateChamCongDetail(data, IdCurrentUser, db);
        //    //        return Ok(result);

        //    //    }
        //    //    else
        //    //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //}
        //    [HttpPost]
        //    [Route("GetTongHopChamCong")]
        //    public IActionResult GetTongHopChamCong(dynamic data)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;

        //            var result = funcChamCong.GetTongHopChamCong(data, db);
        //            return Ok(result);

        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        [HttpGet("ImportChamCongTuExcel")]
        public IActionResult ImportChamCongTuExcel(string FileName, string Id)
        {
            var result = funcChamCong.ImportChamCongTuExcel(FileName, Id, db);
            return Ok(funcChamCong.GetItemChamCongDetail(Id, CurrentUserId, db));
        }

        [HttpGet("ExportMauFileChamCong")]
        public IActionResult ExportMauFileChamCong() => Ok(funcChamCong.ExportMauFileChamCong());

        //    [HttpPost]
        //    [Route("ImportChamCongTuMayChamCong")]
        //    public IActionResult ImportChamCongTuMayChamCong(dynamic data)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;

        //            var result = funcChamCong.ImportChamCongTuMayChamCong(data, IdCurrentUser, db);
        //            return Ok(result);

        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    //[HttpGet]
        //    //[Route("TestNgayNghiLe")]
        //    //public IActionResult TestNgayNghiLe()
        //    //{
        //    //    var currentUser = getCurrentUser();
        //    //    if (currentUser != null)
        //    //    {
        //    //        string IdCurrentUser = currentUser.Id;
        //    //        db.IdUser = IdCurrentUser;
        //    //        db.currentUser = currentUser;

        //    //        var lstDotChamCong = db.HRM_DM_DotChamCongThang.AsNoTracking().ToList();
        //    //        var lstCaiDatNghiLe = smdb.CaiDatHeThongs.AsNoTracking().Where(q => q.Ma == "NgayNghiLe" || q.Ma == "NgayNghiBu").ToList();
        //    //        var result = dungchung.GetDanhSachNgayCongTrongThang(2020, 11, "9b8b1abb-c317-47d1-a29f-85126715c54c", lstDotChamCong, lstCaiDatNghiLe);
        //    //        return Ok(result);

        //    //    }
        //    //    else
        //    //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    //}
        //    #endregion
        //    #region Quy trình

        [HttpGet("GetNextMaQuyTrinhChamCongDetail")]
        public IActionResult GetNextMaChamCong() => Ok(funcChamCong.GetNextMaChamCong(db));

        [HttpPost("GetDanhSachQuyTrinhChamCongDetail")]
        public IActionResult GetDanhSachChamCongDetailQuyTrinh([FromBody] EntityGetDanhSachChamCongDetailRequest data) =>
            Ok(funcChamCong.GetDanhSachChamCongDetailQuyTrinh(data, db));

        [HttpPost("SetQuyTrinhChamCongDetail")]
        public IActionResult SetQuyTrinhChamCongDetail([FromBody] HRM_ChamCong_Detail data) =>
            Ok(funcChamCong.SetQuyTrinhChamCongDetail(data, db));

        [HttpPost("ChuyenTiepQuyTrinhChamCongDetail")]
        public IActionResult ChuyenTiepQuyTrinhChamCongDetail([FromBody] HRM_ChamCong_Detail data) =>
            Ok(funcChamCong.ChuyenTiepQuyTrinhChamCongDetail(data, db));

        [HttpPost("KhongDuyetQuyTrinhChamCongDetail")]
        public IActionResult KhongDuyetQuyTrinhChamCongDetail([FromBody] HRM_ChamCong_Detail data) =>
            Ok(funcChamCong.KhongDuyetQuyTrinhChamCongDetail(data, db));

        // new

        [HttpGet("GetNextSoBangChamCong")]
        public IActionResult GetNextMaBangChamCong() => Ok(funcChamCong.GetNextMaBangChamCong(db));

        [HttpPost("GetListBangChamCong")]
        public IActionResult GetListBangChamCong([FromBody] dynamic data) => Ok(funcChamCong.GetListBangChamCong(data, db));

        [HttpPost("GetTongHopLichChamCong")]
        public IActionResult GetTongHopLichChamCong([FromBody] dynamic data) => Ok(funcChamCong.GetTongHopLichChamCong(db, data));

        [HttpPost("GetTongHopLichChamCongTheoCong")]
        public IActionResult GetTongHopLichChamCongTheoCong([FromBody] dynamic data) =>
            Ok(funcChamCong.GetTongHopLichChamCongTheoCong(db, data));

        [HttpPost("SetBangChamCong")]
        public IActionResult SetBangChamCong([FromBody] dynamic dyn)
        {
            HRM_BangChamCong data = JsonConvert.DeserializeObject<HRM_BangChamCong>(dyn.ToString());
            return Ok(funcChamCong.SetBangChamCong(data, db));
        }

        [HttpPost("ExportTongHopBangChamCong")]
        public IActionResult ExportTongHopBangChamCong([FromBody] dynamic dyn) => Ok(funcChamCong.ExportTongHopBangChamCong(db, dyn));

        [HttpPost("ChuyenTiepBangChamCong")]
        public IActionResult ChuyenTiepBangChamCong([FromBody] HRM_BangChamCong data) =>
            Ok(funcChamCong.ChuyenTiepBangChamCong(data, db));

        [HttpPost("GetTongHopLichChamCongTheoCongTangCa")]
        public IActionResult GetTongHopLichChamCongTheoCongTangCa([FromBody] TimKiemBangLuong itemtimkiem) =>
            Ok(funcChamCong.GetTongHopLichChamCongTheoCongTangCa(db, itemtimkiem.IdCongty, itemtimkiem.listIdBoPhan, itemtimkiem.TuNgay, itemtimkiem.DenNgay, itemtimkiem.Keyword));

        [HttpPost("GetTongHopLichChamCongTheoCongTangCaCaNhan")]
        public IActionResult GetTongHopLichChamCongTheoCongTangCaCaNhan([FromBody] TimKiemBangLuong itemtimkiem) =>
            Ok(funcChamCong.GetTongHopLichChamCongTheoCongTangCaCaNhan(db, itemtimkiem.IdCongty, itemtimkiem.listIdBoPhan, itemtimkiem.TuNgay, itemtimkiem.DenNgay, itemtimkiem.Keyword));

        [HttpPost("ExportTongHopCongTangCa")]
        public IActionResult ExportTongHopCongTangCa([FromBody] TimKiemBangLuong itemtimkiem) =>
            Ok(funcChamCong.ExportTongHopCongTangCa(db, itemtimkiem.IdCongty, itemtimkiem.listIdBoPhan, itemtimkiem.TuNgay, itemtimkiem.DenNgay, itemtimkiem.Keyword));

        [HttpGet("ExportTongHopCongTangCa_TheoQuyTrinhChamCong")]
        public IActionResult ExportTongHopCongTangCa_TheoQuyTrinhChamCong(string Id) =>
            Ok(funcChamCong.ExportTongHopCongTangCa_TheoQuyTrinhChamCong(db, Id));

        [HttpPost("KhongDuyetBangChamCong")]
        public IActionResult KhongDuyetBangChamCong([FromBody] HRM_BangChamCong data) =>
            Ok(funcChamCong.KhongDuyetBangChamCong(data, db));

        [HttpGet("ImportBangChamCong")]
        public IActionResult ImportBangChamCong(string FileName, double TuNgay, double DenNgay, long IdCongTy = 0) =>
            Ok(funcChamCong.ImportBangChamCongICT(FileName, TuNgay, DenNgay, db, IdCongTy));

        [HttpGet("GetChamCongFromMayChamCong")]
        public IActionResult GetChamCongFromMayChamCong(long IdCongTy, double TuNgay, double DenNgay, bool isAll = false, bool isWeb = false) =>
            Ok(funcChamCong.GetChamCongFromMayChamCong(IdCongTy, TuNgay, DenNgay, db, isWeb, isAll));

        [HttpPost("GetThoiGianChamCong")]
        public IActionResult GetThoiGianChamCong([FromBody] TimKiemChung timkiem) => Ok(funcChamCong.GetThoiGianChamCong(timkiem, db));

        [HttpPost("ExportTheoDoiChamCong")]
        public IActionResult ExportTheoDoiChamCong([FromBody] TimKiemChung timkiem) =>
            Ok(funcChamCong.ExportTheoDoiChamCong(db, timkiem));

        [HttpPost("ExportBangChamCongCaNhan")]
        public IActionResult ExportBangChamCong([FromBody] List<HRM_BangChamCong_NhanVien> listNhanVien) =>
            Ok(funcChamCong.ExportBangChamCong(db, listNhanVien));

        [HttpGet("GetChamCongForGiamSat")]
        public IActionResult GetChamCongForGiamSat(long IdCongTy, double TuNgay, double DenNgay) =>
            Ok(funcChamCong.GetChamCongForGiamSat(TuNgay, DenNgay, db));

        [HttpGet("DeleteBangChamCong")]
        public IActionResult DeleteBangChamCong(string Id) => Ok(funcChamCong.DeleteBangChamCong(Id, db));

        [HttpGet("GetBangChamCong")]
        public IActionResult GetBangChamCong(string Id) => Ok(funcChamCong.GetBangChamCong(Id, db));

        // new

        [HttpGet("GetNextSoQuyTrinhTinhLuong")]
        public IActionResult GetNextMaQuyTrinhTinhLuong() => Ok(funcChamCong.GetNextMaQuyTrinhTinhLuong(db));

        [HttpPost("GetListQuyTrinhTinhLuong")]
        public IActionResult GetListQuyTrinhTinhLuong([FromBody] dynamic data) => Ok(funcChamCong.GetListQuyTrinhTinhLuong(data, db));

        [HttpPost("SetQuyTrinhTinhLuong")]
        public IActionResult SetQuyTrinhTinhLuong([FromBody] HRM_QuyTrinhTinhLuong data) =>
            Ok(funcChamCong.SetQuyTrinhTinhLuong(data, db));

        [HttpPost("ChuyenTiepQuyTrinhTinhLuong")]
        public IActionResult ChuyenTiepQuyTrinhTinhLuong([FromBody] HRM_QuyTrinhTinhLuong data) =>
            Ok(funcChamCong.ChuyenTiepQuyTrinhTinhLuong(data, db));

        [HttpPost("KhongDuyetQuyTrinhTinhLuong")]
        public IActionResult KhongDuyetQuyTrinhTinhLuong([FromBody] HRM_QuyTrinhTinhLuong data) =>
            Ok(funcChamCong.KhongDuyetQuyTrinhTinhLuong(data, db));

        [HttpGet("DeleteQuyTrinhTinhLuong")]
        public IActionResult DeleteQuyTrinhTinhLuong(string Id) => Ok(funcChamCong.DeleteQuyTrinhTinhLuong(Id, db));

        [HttpGet("GetQuyTrinhTinhLuong")]
        public IActionResult GetQuyTrinhTinhLuong(string Id) => Ok(funcChamCong.GetQuyTrinhTinhLuong(Id, db));

        [HttpGet("GetTinhLuongNhanVienTheoBoPhan")]
        public IActionResult GetTinhLuongNhanVienTheoBoPhan(long IdBoPhan, int Thang, int Nam) =>
            Ok(funcChamCong.GetTinhLuongNhanVienTheoBoPhan(IdBoPhan, Thang, Nam, db));

        [HttpGet("ExportBangChamCong")]
        public IActionResult ExportBangChamCong() => Ok(funcChamCong.ExportBangChamCong(db));

        [HttpGet("GetTinhLuongNhanVienTheoKyExcel")]
        public IActionResult GetTinhLuongNhanVienTheoKyExcel(string FileName, double TuNgay, double DenNgay) =>
            Ok(funcChamCong.GetTinhLuongNhanVienTheoKyExcel(FileName, TuNgay, DenNgay, db));

        [HttpPost("ExportTongHopBangLuong")]
        public IActionResult ExportTongHopBangLuong([FromBody] dynamic dyn) => Ok(funcChamCong.ExportTongHopBangLuong(db, dyn));

        [HttpPost("ExportBangLuongThang")]
        public IActionResult ExportBangLuongThang([FromBody] HRM_QuyTrinhTinhLuong dyn) =>
            Ok(funcChamCong.ExportBangLuongThangNew(db, dyn));

        [HttpPost("ExportBangLuongThangTongHop")]
        public IActionResult ExportBangLuongThangTongHop([FromBody] TimKiemBangLuong dyn) =>
            Ok(funcChamCong.ExportBangLuongThangTongHop(db, dyn));

        [HttpPost("ExportTongHopLuongThang")]
        public IActionResult ExportTongHopLuongThang([FromBody] dynamic dyn) => Ok(funcChamCong.ExportTongHopLuongThang(db, dyn));

        [HttpPost("GetTongHopLuongThang")]
        public IActionResult GetTongHopLuongThang([FromBody] dynamic dyn) => Ok(funcChamCong.GetTongHopLuongThang(db, dyn));

        [HttpPost("GetTinhLuongNhanVienTheoKyChamCong")]
        public IActionResult GetTinhLuongNhanVienTheoKyChamCong([FromBody] dynamic dyn)
        {
            TimKiemBangLuong enity = JsonConvert.DeserializeObject<TimKiemBangLuong>(dyn.ToString());
            return Ok(funcChamCong.GetTinhLuongNhanVienTheoKyChamCong(enity.IdCongty, enity.listIdBoPhan, enity.listIdKyChamCong, enity.LoaiLuong, enity.eAction, db));
        }

        [HttpPost("GetTinhLuongNhanVienTheoChamCong")]
        public IActionResult GetTinhLuongNhanVienTheoChamCong([FromBody] dynamic dyn)
        {
            TimKiemBangLuong enity = JsonConvert.DeserializeObject<TimKiemBangLuong>(dyn.ToString());
            return Ok(funcChamCong.GetTinhLuongNhanVienTheoChamCong(enity.IdCongty, enity.listIdBoPhan, enity.TuNgay, enity.DenNgay, db));
        }

        [HttpGet("GetDanhSachBangChamCong")]
        public IActionResult GetDanhSachBangChamCong(long IdCongTy) => Ok(funcChamCong.GetDanhSachBangChamCong(IdCongTy, db));

        [HttpGet("GetTongHopLuongCoSo")]
        public IActionResult GetTongHopLuongCoSo(int Thang, int Nam, string KeyWord) =>
            Ok(funcChamCong.GetTongHopLuongCoSo(db, Thang, Nam, KeyWord));

        [HttpGet("ExportTongHopLuongCoSo")]
        public IActionResult ExportTongHopLuongCoSo(int Thang, int Nam, string KeyWord) =>
            Ok(funcChamCong.ExportTongHopLuongCoSo(db, Thang, Nam, KeyWord));

        [HttpPost("GetListCapPhatThuHoiTaiSan")]
        public IActionResult GetListCapPhatThuHoiTaiSan([FromBody] dynamic data) =>
            Ok(funcChamCong.GetListCapPhatThuHoiTaiSan(data, db));

        [HttpPost("GetDanhSanhNhanVienDaCapPhatTaiSan")]
        public IActionResult GetDanhSanhNhanVienDaCapPhatTaiSan([FromBody] TimKiemBangLuong timkiem) =>
            Ok(funcChamCong.GetDanhSanhNhanVienDaCapPhatTaiSan(timkiem.IdCongty, timkiem.listIdBoPhan, db));

        [HttpGet("UpdateLuongNhanVien")]
        public IActionResult UpdateLuongNhanVien()
        {
            funcChamCong.UpdateLuongNhanVien(db);
            return Ok("0");
        }

        [HttpPost("SetCapPhatThuHoiTaiSan")]
        public IActionResult SetCapPhatThuHoiTaiSan([FromBody] HRM_CapPhatThuHoiTaiSan data) =>
            Ok(funcChamCong.SetCapPhatThuHoiTaiSan(data, db));

        [HttpGet("DeleteCapPhatThuHoiTaiSan")]
        public IActionResult DeleteCapPhatThuHoiTaiSan(string Id) => Ok(funcChamCong.DeleteCapPhatThuHoiTaiSan(Id, db));

        [HttpGet("GetCapPhatThuHoiTaiSan")]
        public IActionResult GetCapPhatThuHoiTaiSan(string Id) => Ok(funcChamCong.GetCapPhatThuHoiTaiSan(Id, db));

        [HttpPost("GetTongHopTheoDoiCapPhatTaiSan")]
        public IActionResult GetTongHopTheoDoiCapPhatTaiSan([FromBody] TimKiemBangLuong itemTimKiem) =>
            Ok(funcChamCong.GetTongHopTheoDoiCapPhatTaiSan(db, itemTimKiem));

        [HttpPost("ExportTongHopTheoDoiCapPhatTaiSanICT")]
        public IActionResult ExportTongHopTheoDoiCapPhatTaiSan([FromBody] dynamic itemTimKiem) =>
            Ok(funcChamCong.ExportTongHopTheoDoiCapPhatTaiSan(db, itemTimKiem));

        #region dienx giải chấm công

        [HttpPost("GetListDienGiaiChamCong")]
        public IActionResult GetListDienGiaiChamCong([FromBody] dynamic data) => Ok(funcChamCong.GetListDienGiaiChamCong(db, data));

        [HttpGet("GetNextSoDienGiaiChamCong")]
        public IActionResult GetNextSoDienGiaiChamCong() => Ok(funcChamCong.GetNextSoDienGiaiChamCong(db));

        [HttpPost("SetDienGiaiChamCong")]
        public IActionResult SetDienGiaiChamCong([FromBody] HRM_DienGiaiChamCong QuyTrinh) =>
            Ok(funcChamCong.SetDienGiaiChamCong(db, QuyTrinh));

        [HttpGet("GetDienGiaiChamCong")]
        public IActionResult GetDienGiaiChamCong(string Id) => Ok(funcChamCong.GetDienGiaiChamCong(db, Id));

        [HttpPost("ChuyenTiepDienGiaiChamCong")]
        public IActionResult ChuyenTiepDienGiaiChamCong([FromBody] dynamic data) =>
            Ok(funcChamCong.ChuyenTiepDienGiaiChamCong(db, data));

        [HttpPost("ExportDienGiaiChamCong")]
        public IActionResult ExportDienGiaiChamCong([FromBody] dynamic data) => Ok(funcChamCong.ExportDienGiaiChamCong(db, data));

        [HttpPost("DeleteDienGiaiChamCong")]
        public IActionResult DeleteDienGiaiChamCong([FromBody] dynamic data) => Ok(funcChamCong.DeleteDienGiaiChamCong(db, data));

        #endregion
    }
}
