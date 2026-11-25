#nullable disable
using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using HRM.Api.Services.DaoTao;
using HRM.Api.Models;
using System.Collections.Generic;
using static HRM.Api.Services.DaoTao.FuncDaoTao;

namespace HRM.Api.Controllers
{
    [Route("DaoTao")]
    public class DaoTaoController(HrmDbContext db, FuncDaoTao func) : ApiControllerBase
    {
        #region kế hoạch đào tao

        [HttpGet("GetNextMaKeHoachDaoTao")]
        public IActionResult GetNextMaKeHoachDaoTao() => Ok(func.GetNextMaKeHoachDaoTao(db));

        [HttpPost("GetListQuyTrinhKeHoachDaoTao")]
        public IActionResult GetListQuyTrinhKeHoachDaoTao([FromBody] dynamic model) =>
            Ok(func.GetListQuyTrinhKeHoachDaoTao(model, db));

        [HttpPost("SetQuyTrinhKeHoachDaoTao")]
        public IActionResult SetQuyTrinhKeHoachDaoTao([FromBody] HRM_KeHoachDaoTao model) =>
            Ok(func.SetQuyTrinhKeHoachDaoTao(model, db));

        [HttpPost("ChuyenTiepQuyTrinhKeHoachDaoTao")]
        public IActionResult ChuyenTiepQuyTrinhKeHoachDaoTao([FromBody] HRM_KeHoachDaoTao model) =>
            Ok(func.ChuyenTiepQuyTrinhKeHoachDaoTao(model, db));

        [HttpPost("KhongDuyetQuyTrinhKeHoachDaoTao")]
        public IActionResult KhongDuyetQuyTrinhKeHoachDaoTao([FromBody] HRM_KeHoachDaoTao model) =>
            Ok(func.KhongDuyetQuyTrinhKeHoachDaoTao(model, db));

        [HttpGet("GetByIdQuyTrinhKeHoachDaoTao")]
        public IActionResult GetByIdQuyTrinhKeHoachDaoTao(string Id) => Ok(func.GetByIdQuyTrinhKeHoachDaoTao(Id, db));

        [HttpGet("DeleteQuyTrinhKeHoachDaoTao")]
        public IActionResult DeleteQuyTrinhKeHoachDaoTao(string Id) => Ok(func.DeleteQuyTrinhKeHoachDaoTao(Id, db));

        #endregion

        [HttpGet("GetListKeHoachChuaTongHop")]
        public IActionResult GetListKeHoachChuaTongHop(int Nam, long IdCongTyEOS, bool isDaoTaoMoi) =>
            Ok(func.GetListKeHoachChuaTongHop(Nam, IdCongTyEOS, isDaoTaoMoi, db));

        [HttpPost("GetTongHopKeHoachDaoTao")]
        public IActionResult GetTongHopKeHoachDaoTao([FromBody] List<string> listId) => Ok(func.GetTongHopKeHoachDaoTao(listId, db));

        #region Tông hợp kế hoạch đào tao

        [HttpGet("GetNextMaTongHopKeHoachDaoTao")]
        public IActionResult GetNextMaTongHopKeHoachDaoTao() => Ok(func.GetNextMaTongHopKeHoachDaoTao(db));

        [HttpPost("GetListQuyTrinhTongHopKeHoachDaoTao")]
        public IActionResult GetListQuyTrinhTongHopKeHoachDaoTao([FromBody] dynamic model) =>
            Ok(func.GetListQuyTrinhTongHopKeHoachDaoTao(model, db));

        [HttpPost("SetQuyTrinhTongHopKeHoachDaoTao")]
        public IActionResult SetQuyTrinhTongHopKeHoachDaoTao([FromBody] HRM_KeHoachDaoTaoTongHop model) =>
            Ok(func.SetQuyTrinhTongHopKeHoachDaoTao(model, db));

        [HttpPost("ChuyenTiepQuyTrinhTongHopKeHoachDaoTao")]
        public IActionResult ChuyenTiepQuyTrinhTongHopKeHoachDaoTao([FromBody] HRM_KeHoachDaoTaoTongHop model) =>
            Ok(func.ChuyenTiepQuyTrinhTongHopKeHoachDaoTao(model, db));

        [HttpPost("KhongDuyetQuyTrinhTongHopKeHoachDaoTao")]
        public IActionResult K([FromBody] HRM_KeHoachDaoTaoTongHop model) =>
            Ok(func.KhongDuyetQuyTrinhTongHopKeHoachDaoTao(model, db));

        [HttpGet("GetByIdQuyTrinhTongHopKeHoachDaoTao")]
        public IActionResult GetByIdQuyTrinhTongHopKeHoachDaoTao(string Id) => Ok(func.GetByIdQuyTrinhTongHopKeHoachDaoTao(Id, db));

        [HttpGet("DeleteQuyTrinhTongHopKeHoachDaoTao")]
        public IActionResult DeleteQuyTrinhTongHopKeHoachDaoTao(string Id) => Ok(func.DeleteQuyTrinhTongHopKeHoachDaoTao(Id, db));

        #endregion

        #region đợt đào tao

        [HttpGet("GetKeHoachDaoTaoTongHop")]
        public IActionResult GetKeHoachDaoTaoTongHop(int Nam, long IdCongTyEOS) =>
            Ok(func.GetKeHoachDaoTaoTongHop(Nam, IdCongTyEOS, db));

        [HttpGet("GetListKeHoachDaoTaoTongHopChiTiet")]
        public IActionResult GetListKeHoachDaoTaoTongHopChiTiet(int Nam, long IdCongTyEOS) =>
            Ok(func.GetListKeHoachDaoTaoTongHopChiTiet(Nam, IdCongTyEOS, db));

        [HttpGet("GetNextMaDotDaoTao")]
        public IActionResult GetNextMaDotDaoTao() => Ok(func.GetNextMaDotDaoTao(db));

        [HttpPost("GetListQuyTrinhDotDaoTao")]
        public IActionResult GetListQuyTrinhDotDaoTao([FromBody] dynamic model) => Ok(func.GetListQuyTrinhDotDaoTao(model, db));

        [HttpPost("SetQuyTrinhDotDaoTao")]
        public IActionResult SetQuyTrinhDotDaoTao([FromBody] HRM_Dot_DaoTao model) => Ok(func.SetQuyTrinhDotDaoTao(model, db));

        [HttpPost("ChuyenTiepQuyTrinhDotDaoTao")]
        public IActionResult ChuyenTiepQuyTrinhDotDaoTao([FromBody] HRM_Dot_DaoTao model) =>
            Ok(func.ChuyenTiepQuyTrinhDotDaoTao(model, db));

        [HttpPost("KhongDuyetQuyTrinhDotDaoTao")]
        public IActionResult KhongDuyetQuyTrinhDotDaoTao([FromBody] HRM_Dot_DaoTao model) =>
            Ok(func.KhongDuyetQuyTrinhDotDaoTao(model, db));

        [HttpGet("GetByIdQuyTrinhDotDaoTao")]
        public IActionResult GetByIdQuyTrinhDotDaoTao(string Id) => Ok(func.GetByIdQuyTrinhDotDaoTao(Id, db));

        [HttpGet("GetListNhanVienDotDaoTao")]
        public IActionResult GetListNhanVienDotDaoTao(string IdChuongTrinhDaoTao) =>
            Ok(func.GetListNhanVienDotDaoTao(IdChuongTrinhDaoTao, db));

        [HttpGet("GetlistChuongTrinhDaoTaoDaDuyet")]
        public IActionResult GetlistChuongTrinhDaoTaoDaDuyet(int Nam = 0) => Ok(func.GetlistChuongTrinhDaoTaoDaDuyet(db, Nam));

        [HttpGet("GetDanhSachChiPhiForDaoTao")]
        public IActionResult GetDanhSachChiPhiForDaoTao(int Nam, int Thang) => Ok(func.GetDanhSachChiPhiForDaoTao(Nam, Thang, db));

        [HttpGet("DeleteQuyTrinhDotDaoTao")]
        public IActionResult DeleteQuyTrinhDotDaoTao(string Id) => Ok(func.DeleteQuyTrinhDotDaoTao(Id, db));

        [HttpGet("ImportDot_DaoTaoChiPhi")]
        public IActionResult ImportDot_DaoTaoChiPhi(string FileName) => Ok(func.ImportDot_DaoTaoChiPhi(db, FileName));

        [HttpGet("ExportDot_DaoTaoChiPhi")]
        public IActionResult ExportDot_DaoTaoChiPhi() => Ok(func.ExportDot_DaoTaoChiPhi(db));

        [HttpPost("ExportBaoCaoDot_DaoTaoChiPhi")]
        public IActionResult ExportBaoCaoDot_DaoTaoChiPhi([FromBody] List<HRM_Dot_DaoTaoChiPhiViewModel> listData) =>
            Ok(func.ExportBaoCaoDot_DaoTaoChiPhi(listData, db));

        [HttpPost("GetDanhSachDotDaoTaoNhanVien")]
        public IActionResult GetDanhSachDotDaoTaoNhanVien([FromBody] dynamic dyn) => Ok(func.GetDanhSachDotDaoTaoNhanVien(dyn, db));

        #endregion

        #region chương trình đào tạo new

        [HttpGet("GetNextSoChuongTrinhDaoTao")]
        public IActionResult GetNextMaChuongTrinhDaoTao() => Ok(func.GetNextMaChuongTrinhDaoTao(db));

        [HttpPost("GetListChuongTrinhDaoTao")]
        public IActionResult GetListChuongTrinhDaoTao([FromBody] dynamic model) => Ok(func.GetListChuongTrinhDaoTao(model, db));

        [HttpPost("SetChuongTrinhDaoTao")]
        public IActionResult SetChuongTrinhDaoTao([FromBody] HRM_ChuongTrinhDaoTao model) => Ok(func.SetChuongTrinhDaoTao(model, db));

        [HttpPost("ChuyenTiepChuongTrinhDaoTao")]
        public IActionResult ChuyenTiepChuongTrinhDaoTao([FromBody] HRM_ChuongTrinhDaoTao model) =>
            Ok(func.ChuyenTiepChuongTrinhDaoTao(model, db));

        [HttpPost("KhongDuyetChuongTrinhDaoTao")]
        public IActionResult KhongDuyetChuongTrinhDaoTao([FromBody] HRM_ChuongTrinhDaoTao model) =>
            Ok(func.KhongDuyetChuongTrinhDaoTao(model, db));

        [HttpPost("ExportBaoCaoChuongTrinhDaoTaoNhanVien")]
        public IActionResult ExportBaoCaoChuongTrinhDaoTaoNhanVien([FromBody] HRM_ChuongTrinhDaoTao model) =>
            Ok(func.ExportBaoCaoChuongTrinhDaoTaoNhanVien(model, db));

        [HttpPost("ExportTongHopNhanVienDaoTao")]
        public IActionResult ExportTongHopNhanVienDaoTao([FromBody] DaoTaoTongHopNhanVien model) =>
            Ok(func.ExportTongHopNhanVienDaoTao(db, model));

        [HttpPost("GetListNhanVienDaoTao")]
        public IActionResult GetListNhanVienDaoTao([FromBody] dynamic model) => Ok(func.GetListNhanVienDaoTao(db, model));

        [HttpPost("ExportBaoCaoChuongTrinhDaoTao")]
        public IActionResult ExportBaoCaoChuongTrinhDaoTao([FromBody] HRM_ChuongTrinhDaoTao model) =>
            Ok(func.ExportBaoCaoChuongTrinhDaoTao(model, db));

        [HttpGet("GetByIdChuongTrinhDaoTao")]
        public IActionResult GetByIdChuongTrinhDaoTao(string Id) => Ok(func.GetByIdChuongTrinhDaoTao(Id, db));

        [HttpGet("GetlistKeHoachDaoTaoDaDuyet")]
        public IActionResult GetlistKeHoachDaoTaoDaDuyet(int Nam) => Ok(func.GetlistKeHoachDaoTaoDaDuyet(db, Nam));

        [HttpGet("GetDanhSachNoiDung_NhanVien")]
        public IActionResult GetDanhSachNoiDung_NhanVien(string Id) => Ok(func.GetDanhSachNoiDung_NhanVien(Id, db));

        [HttpGet("DeleteChuongTrinhDaoTao")]
        public IActionResult DeleteChuongTrinhDaoTao(string Id) => Ok(func.DeleteChuongTrinhDaoTao(Id, db));

        [HttpGet("GetDanhSachHinhThucDaoTao")]
        public IActionResult GetDanhSachHinhThucDaoTao() => Ok(func.GetDanhSachHinhThucDaoTao());

        #endregion
        #region Khảo thí

        [HttpPost("GetListDotThi")]
        public IActionResult GetListDotThi([FromBody] dynamic model) => Ok(func.GetListDotThi(db, model));

        [HttpGet("GetListDotThiDangThi")]
        public IActionResult GetListDotThiDangThi() => Ok(func.GetListDotThiDangThi(db));

        [HttpPost("SetDotThi")]
        public IActionResult SetDotThi([FromBody] HRM_DotThi model) => Ok(func.SetDotThi(model, db));

        [HttpPost("SetBaiThi")]
        public IActionResult SetBaiThi([FromBody] HRM_BaiThi entity) => Ok(func.SetBaiThi(entity, db));

        [HttpPost("ChamDiemBaiThi")]
        public IActionResult ChamDiemBaiThi([FromBody] HRM_BaiThi entity) => Ok(func.SetBaiThi(entity, db, true));

        [HttpPost("ExportDanhSachDeThi")]
        public IActionResult ExportDanhSachDeThi([FromBody] HRM_DotThi model) => Ok(func.ExportDanhSachDeThi(db, model));

        //[HttpPost]
        //[Route("ChuyenTiepDotThi")]
        //public IActionResult ChuyenTiepDotThi(HRM_KeHoachDaoTao model)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        var item = func.ChuyenTiepDotThi(model, db);

        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //[HttpPost]
        //[Route("KhongDuyetDotThi")]
        //public IActionResult KhongDuyetDotThi(HRM_KeHoachDaoTao model)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        var item = func.KhongDuyetDotThi(model, db);

        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        [HttpGet("GetDotThi")]
        public IActionResult GetDotThi(string Id) => Ok(func.GetDotThi(Id, db));

        [HttpGet("GetBaiThiByMaDe")]
        public IActionResult GetBaiThiByMaDe(string MaDe, string IdDotThi, string IdNhanVien = "") =>
            Ok(func.GetBaiThiByMaDe(MaDe, IdDotThi, IdNhanVien, db));

        [HttpGet("DeleteDotThi")]
        public IActionResult DeleteDotThi(string Id) => Ok(func.DeleteDotThi(Id, db));

        [HttpPost("GetBaiThiRandom")]
        public IActionResult GetBaiThiRandom([FromBody] HRM_BaiThiRanDom entity) =>
            Ok(func.GetBaiThiRandom(entity.listIdNhanVien, entity.MaDotThi, entity.IddmCauTrucDe, entity.ChungDe, entity.SoLuongOffline, db));

        [HttpGet("GetBaiThiByIdNhanVien")]
        public IActionResult GetBaiThiByIdNhanVien(string IdNhanVien, string IdDotThi, string MaDe = "") =>
            Ok(func.GetBaiThiByIdNhanVien(IdNhanVien, IdDotThi, db, MaDe));

        [HttpGet("GetThongTinNhanVienBySoBaoDanh")]
        public IActionResult GetThongTinNhanVienBySoBaoDanh(string SoBaoDanh, string IdDotThi) =>
            Ok(func.GetThongTinNhanVienBySoBaoDanh(SoBaoDanh, IdDotThi, db));

        #endregion

        [HttpPost("GetListKhaoSatDaoTao")]
        public IActionResult GetListKhaoSatDaoTao([FromBody] dynamic model) => Ok(func.GetListKhaoSatDaoTao(db, model));

        [HttpPost("SetKhaoSatDaoTao")]
        public IActionResult SetKhaoSatDaoTao([FromBody] HRM_KhaoSatDaoTao model) => Ok(func.SetKhaoSatDaoTao(model, db));

        [HttpPost("SetKhaoSatDaoTao_NhanVien")]
        public IActionResult SetKhaoSatDaoTao_NhanVien([FromBody] HRM_KhaoSatDaoTao_NhanVienView entity) =>
            Ok(func.SetKhaoSatDaoTao_NhanVien(entity, db));

        [HttpGet("GetKhaoSatDaoTao")]
        public IActionResult GetKhaoSatDaoTao(string Id) => Ok(func.GetKhaoSatDaoTao(Id, db));

        [HttpGet("GetKhaoSatDaoTaoForWebById")]
        public IActionResult GetKhaoSatDaoTaoForWebById(string Id) => Ok(func.GetKhaoSatDaoTaoForWebById(Id, db));

        [HttpGet("ExportKhaoSatDaoTao")]
        public IActionResult ExportKhaoSatDaoTao(string Id) => Ok(func.ExportKhaoSatDaoTao(db, Id));
    }
}
