#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.BaoCao;
using HRM.Api.Entity;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static HRM.Api.Controllers.NhanVienController;

namespace HRM.Api.Controllers
{
    [Route("BaoHiemXahoi")]
    public class BaoHiemXahoiController(HrmDbContext db, FuncBaoCaoBaoHiemXahoi func) : ApiControllerBase
    {
        [HttpGet("GetNextSoQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult GetNextMaQuyTrinhXacNhanTangGiamBaoHiem() => Ok(func.GetNextMaQuyTrinhXacNhanTangGiamBaoHiem(db));

        [HttpPost("GetListQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult GetListQuyTrinhXacNhanTangGiamBaoHiem([FromBody] dynamic model) => Ok(func.GetListQuyTrinhXacNhanTangGiamBaoHiem(model, db));

        [HttpPost("SetQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult SetQuyTrinhXacNhanTangGiamBaoHiem([FromBody] HRM_QuyTrinhXacNhanTangGiamBaoHiem model) =>
            Ok(func.SetQuyTrinhXacNhanTangGiamBaoHiem(model, db));

        [HttpPost("ChuyenTiepQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult ChuyenTiepQuyTrinhXacNhanTangGiamBaoHiem([FromBody] HRM_QuyTrinhXacNhanTangGiamBaoHiem model) =>
            Ok(func.ChuyenTiepQuyTrinhXacNhanTangGiamBaoHiem(model, db));

        [HttpPost("KhoiTaoQuyTrinhXachNhanTangGiamBaoHiem")]
        public IActionResult KhoiTaoQuyTrinhXachNhanTangGiamBaoHiem([FromBody] XacNhanNgoaiGio model) =>
            Ok(func.KhoiTaoQuyTrinhXachNhanTangGiamBaoHiem(db, model.IdCongTy, model.IdBoPhan, model.TuNgay, model.DenNgay));

        [HttpPost("KhongDuyetQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult KhongDuyetQuyTrinhXacNhanTangGiamBaoHiem([FromBody] HRM_QuyTrinhXacNhanTangGiamBaoHiem model) =>
            Ok(func.KhongDuyetQuyTrinhXacNhanTangGiamBaoHiem(model, db));

        [HttpGet("GetByIdQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult GetByIdQuyTrinhXacNhanTangGiamBaoHiem(string Id) => Ok(func.GetByIdQuyTrinhXacNhanTangGiamBaoHiem(Id, db));

        [HttpGet("GetDanhSachLoaiTangBaoHiem")]
        public IActionResult GetDanhSachLoaiTangBaoHiem() => Ok(func.GetDanhSachLoaiTangBaoHiem());

        [HttpGet("GetDanhSachLoaiGiamBaoHiem")]
        public IActionResult GetDanhSachLoaiGiamBaoHiem() => Ok(func.GetDanhSachLoaiGiamBaoHiem());

        [HttpGet("DeleteQuyTrinhXacNhanTangGiamBaoHiem")]
        public IActionResult DeleteQuyTrinhXacNhanTangGiamBaoHiem(string Id) => Ok(func.DeleteQuyTrinhXacNhanTangGiamBaoHiem(Id, db));

        [HttpPost("GetListGiamBaoHiemXaHoi")]
        public IActionResult GetListGiamBaoHiemXaHoi([FromBody] dynamic dyn) => Ok(func.GetListGiamBaoHiemXaHoi(dyn, db));

        [HttpPost("GetListTangBaoHiemXaHoi")]
        public IActionResult GetListTangBaoHiemXaHoi([FromBody] dynamic dyn) => Ok(func.GetListTangBaoHiemXaHoi(dyn, db));

        [HttpPost("GetListHuongLuongHuu")]
        public IActionResult GetListHuongLuongHuu([FromBody] EntityGetDanhSachNhanVienRequest aRequest) => Ok(func.GetListHuongLuongHuu(aRequest, db));

        [HttpPost("ExportHuongLuongHuu")]
        public IActionResult ExportHuongLuongHuu([FromBody] List<string> Ids) => Ok(func.ExportHuongLuongHuu(Ids, db));

        [HttpGet("UpdateTrangThaiLuongHuu")]
        public IActionResult UpdateTrangThaiLuongHuu(string Ids) => Ok(func.UpdateTrangThaiLuongHuu(Ids, db));

        [HttpPost("ExportGiamBaoHiemXaHoi")]
        public IActionResult ExportGiamBaoHiemXaHoi([FromBody] List<string> Ids) => Ok(func.ExportGiamBaoHiemXaHoi(Ids, db));

        [HttpPost("ExportTangBaoHiemXaHoi")]
        public IActionResult ExportTangBaoHiemXaHoi([FromBody] List<string> Ids) => Ok(func.ExportTangBaoHiemXaHoi(Ids, db));

        [HttpPost("ImportGiamBaoHiemXaHoi")]
        public IActionResult ImportGiamBaoHiemXaHoi(string fileName) => Ok(func.ImportGiamBaoHiemXaHoi(fileName, db));

        [HttpPost("ImportTangBaoHiemXaHoi")]
        public IActionResult ImportTangBaoHiemXaHoi(string fileName) => Ok(func.ImportTangBaoHiemXaHoi(fileName, db));

        [HttpGet("ExportQuaTrinhThamGiaBaoHiemXaHoi")]
        public IActionResult ExportQuaTrinhThamGiaBaoHiemXaHoi(string IdNhanVien = "") =>
            Ok(func.ExportQuaTrinhThamGiaBaoHiemXaHoi(db, IdNhanVien));

        [HttpGet("ExportQuaTrinhThamGiaBaoHiem")]
        public IActionResult ExportQuaTrinhThamGiaBaoHiem(string IdNhanVien) => Ok(func.ExportQuaTrinhThamGiaBaoHiem(db, IdNhanVien));

        [HttpPost("ExportQuaTrinhThamGiaBaoHiemXaHoiThang")]
        public IActionResult ExportQuaTrinhThamGiaBaoHiemXaHoiThang([FromBody] EntityGetDanhSachNhanVienRequest listNhanVien) =>
            Ok(func.ExportQuaTrinhThamGiaBaoHiemXaHoiThang(db, listNhanVien));

        [HttpGet("ImportQuaTrinhThamGiaBaoHiemXaHoi")]
        public IActionResult ImportQuaTrinhThamGiaBaoHiemXaHoi(string fileName, string IdNhanVien = "") =>
            Ok(func.ImportQuaTrinhThamGiaBaoHiemXaHoi(db, fileName, IdNhanVien));

        [HttpPost("UpdateQuaTrinhThamGiaBaoHiemXaHoi")]
        public IActionResult UpdateQuaTrinhThamGiaBaoHiemXaHoi([FromBody] HRM_QuaTrinhThamGiaBaoHiemXaHoi aRequest) =>
            Ok(func.UpdateQuaTrinhThamGiaBaoHiemXaHoi(db, aRequest));

        [HttpGet("GetQuaTrinhThamGiaBaoHiemXaHoi")]
        public IActionResult GetQuaTrinhThamGiaBaoHiemXaHoi(string IdNhanVien) => Ok(func.GetQuaTrinhThamGiaBaoHiemXaHoi(db, IdNhanVien));

        [HttpPost("GetDanhSachNhanVienForBaoHiemXaHoi")]
        public IActionResult GetDanhSachNhanVienForBaoHiemXaHoi([FromBody] EntityGetDanhSachNhanVienRequest aRequest) =>
            Ok(func.GetDanhSachNhanVienForBaoHiemXaHoi(aRequest, db));

        [HttpGet("ExportQuaTrinhThamGiaBaoHiemThatNghiep")]
        public IActionResult ExportQuaTrinhThamGiaBaoHiemThatNghiep() => Ok(func.ExportQuaTrinhThamGiaBaoHiemThatNghiep(db));

        [HttpGet("ImportQuaTrinhThamGiaBaoHiemThatNghiep")]
        public IActionResult ImportQuaTrinhThamGiaBaoHiemThatNghiep(string fileName) => Ok(func.ImportQuaTrinhThamGiaBaoHiemThatNghiep(db, fileName));

        [HttpPost("GetDanhSachNhanVienForBaoHiemThatNghiep")]
        public IActionResult GetDanhSachNhanVienForBaoHiemThatNghiep([FromBody] EntityGetDanhSachNhanVienRequest aRequest) =>
            Ok(func.GetDanhSachNhanVienForBaoHiemThatNghiep(aRequest, db));

        [HttpPost("UpdateQuaTrinhThamGiaBaoHiemThatNghiep")]
        public IActionResult UpdateQuaTrinhThamGiaBaoHiemThatNghiep([FromBody] HRM_QuaTrinhThamGiaBaoHiemThatNghiep aRequest) =>
            Ok(func.UpdateQuaTrinhThamGiaBaoHiemThatNghiep(db, aRequest));

        [HttpGet("GetQuaTrinhThamGiaBaoHiemThatNghiep")]
        public IActionResult GetQuaTrinhThamGiaBaoHiemThatNghiep(string IdNhanVien) => Ok(func.GetQuaTrinhThamGiaBaoHiemThatNghiep(db, IdNhanVien));

        [HttpGet("ExportThongTinHuongCheDoThaiSan")]
        public IActionResult ExportThongTinHuongCheDoThaiSan() => Ok(func.ExportThongTinHuongCheDoThaiSan(db));

        [HttpGet("ImportThongTinHuongCheDoThaiSan")]
        public IActionResult ImportThongTinHuongCheDoThaiSan(string fileName) => Ok(func.ImportThongTinHuongCheDoThaiSan(db, fileName));

        [HttpPost("GetDanhSachNhanVienForThongTinHuong")]
        public IActionResult GetDanhSachNhanVienForThongTinHuong([FromBody] EntityGetDanhSachNhanVienRequest aRequest) =>
            Ok(func.GetDanhSachNhanVienForThongTinHuong(aRequest, db));
    }
}
