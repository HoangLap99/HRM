#nullable disable
using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using HRM.Api.Services.TuyenDung;
using HRM.Api.Entity;
using HRM.Api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using static HRM.Api.Services.FuncBase;

namespace HRM.Api.Controllers
{
    [Route("TuyenDung")]
    public class TuyenDungController(HrmDbContext db, FuncDeNghiTuyenDung func, FuncTuyenDung funcTD, FuncKeHoachTuyenDung funKH) : ApiControllerBase
    {
        #region tuyển dụng NEW

        [HttpPost("GetListQuyTrinhNhuCauTuyenDung")]
        public IActionResult GetListQuyTrinhNhuCauTuyenDung([FromBody] dynamic data) =>
            Ok(funKH.GetListQuyTrinhNhuCauTuyenDung(db, data));

        [HttpPost("GetDanhSachNhuCauTuyenDungByCongTy")]
        public IActionResult GetDanhSachNhuCauTuyenDungByCongTy([FromBody] dynamic data) =>
            Ok(funKH.GetDanhSachNhuCauTuyenDungByCongTy(db, data));

        [HttpGet("GetNextSoQuyTrinhNhuCauTuyenDung")]
        public IActionResult GetNextSoQuyTrinhNhuCauTuyenDung() => Ok(funKH.GetNextSoQuyTrinhNhuCauTuyenDung(db));

        [HttpGet("LayId")]
        public IActionResult LayId() => Ok(funKH.LayId(db));

        [HttpPost("SetQuyTrinhNhuCauTuyenDung")]
        public IActionResult SetQuyTrinhNhuCauTuyenDung([FromBody] dynamic data) => Ok(funKH.SetQuyTrinhNhuCauTuyenDung(db, data));

        [HttpPost("UpdateQuyTrinhNhuCauTuyenDung")]
        public IActionResult UpdateQuyTrinhNhuCauTuyenDung([FromBody] HRM_TuyenDung_NhuCau entity) =>
            Ok(funKH.SetQuyTrinhNhuCauTuyenDung(db, entity));

        [HttpGet("GetQuyTrinhNhuCauTuyenDung")]
        public IActionResult GetQuyTrinhNhuCauTuyenDung(string Id) => Ok(funKH.GetQuyTrinhNhuCauTuyenDung(db, Id));

        [HttpPost("ChuyenTiepQuyTrinhNhuCauTuyenDung")]
        public IActionResult ChuyenTiepQuyTrinhNhuCauTuyenDung([FromBody] dynamic data) =>
            Ok(funKH.ChuyenTiepQuyTrinhNhuCauTuyenDung(db, data));

        [HttpPost("KhongDuyetQuyTrinhNhuCauTuyenDung")]
        public IActionResult KhongDuyetQuyTrinhNhuCauTuyenDung([FromBody] dynamic data) =>
            Ok(funKH.KhongDuyetQuyTrinhNhuCauTuyenDung(db, data));

        [HttpGet("DeleteQuyTrinhNhuCauTuyenDung")]
        public IActionResult DeleteQuyTrinhNhuCauTuyenDung(string Id) => Ok(funKH.DeleteQuyTrinhNhuCauTuyenDung(db, Id));

        #endregion
        #region tuyển dụng NEW

        [HttpPost("GetListQuyTrinhDeNghiTuyenDung")]
        public IActionResult GetListQuyTrinhDeNghiTuyenDung([FromBody] dynamic data) =>
            Ok(funKH.GetListQuyTrinhDeNghiTuyenDung(db, data));

        [HttpPost("GetDanhSachDeNghiTuyenDungByCongTy")]
        public IActionResult GetDanhSachDeNghiTuyenDungByCongTy([FromBody] dynamic data) =>
            Ok(funKH.GetDanhSachDeNghiTuyenDungByCongTy(db, data));

        [HttpGet("GetNextSoQuyTrinhDeNghiTuyenDung")]
        public IActionResult GetNextSoQuyTrinhDeNghiTuyenDung() => Ok(funKH.GetNextSoQuyTrinhDeNghiTuyenDung(db));

        [HttpPost("SetQuyTrinhDeNghiTuyenDung")]
        public IActionResult SetQuyTrinhDeNghiTuyenDung([FromBody] dynamic data) => Ok(funKH.SetQuyTrinhDeNghiTuyenDung(db, data));

        [HttpGet("GetQuyTrinhDeNghiTuyenDung")]
        public IActionResult GetQuyTrinhDeNghiTuyenDung(string Id) => Ok(funKH.GetQuyTrinhDeNghiTuyenDung(db, Id));

        [HttpGet("ImportQuyTrinhNhuCauCauTuyenDung")]
        public IActionResult ImportQuyTrinhNhuCauCauTuyenDung(string fileName) =>
            Ok(funKH.ImportQuyTrinhNhuCauCauTuyenDung(fileName, db));

        [HttpGet("ImportMauNhuCauCauTuyenDung")]
        public IActionResult ImportMauNhuCauCauTuyenDung(string fileName) => Ok(funKH.ImportMauNhuCauCauTuyenDung(fileName, db));

        [HttpGet("ExportMauNhuCauCauTuyenDung")]
        public IActionResult ExportMauNhuCauCauTuyenDung() => Ok(funKH.ExportMauNhuCauCauTuyenDung(db));

        [HttpGet("ExportMauQuyTrinhNhuCauCauTuyenDung")]
        public IActionResult ExportMauQuyTrinhNhuCauCauTuyenDung() => Ok(funKH.ExportMauQuyTrinhNhuCauCauTuyenDung(db));

        [HttpPost("TongHopDeNghiTuyenDung")]
        public IActionResult TongHopDeNghiTuyenDung([FromBody]dynamic dyn) => Ok(funKH.TongHopDeNghiTuyenDung(db,dyn));

        [HttpPost("ChuyenTiepQuyTrinhDeNghiTuyenDung")]
        public IActionResult ChuyenTiepQuyTrinhDeNghiTuyenDung([FromBody] HRM_DeNghiTuyenDung data) =>
            Ok(funKH.ChuyenTiepQuyTrinhDeNghiTuyenDung(db, data));

        [HttpPost("KhongDuyetQuyTrinhDeNghiTuyenDung")]
        public IActionResult KhongDuyetQuyTrinhDeNghiTuyenDung([FromBody] dynamic data) =>
            Ok(funKH.KhongDuyetQuyTrinhDeNghiTuyenDung(db, data));

        [HttpPost("ExporDeNghiTuyenDungCongNhan")]
        public IActionResult ExporDeNghiTuyenDungCongNhan([FromBody] HRM_DeNghiTuyenDung data) =>
            Ok(funKH.ExporDeNghiTuyenDungCongNhan(data, db));

        [HttpPost("ExporDeNghiTuyenDungNhanVien")]
        public IActionResult ExporDeNghiTuyenDungNhanVien([FromBody] HRM_DeNghiTuyenDung data) =>
            Ok(funKH.ExportDeNghiTuyenDungNhanVien(data, db));

        [HttpGet("DeleteQuyTrinhDeNghiTuyenDung")]
        public IActionResult DeleteQuyTrinhDeNghiTuyenDung(string Id) => Ok(funKH.DeleteQuyTrinhDeNghiTuyenDung(db, Id));

        [HttpPost("ChuyenTiepListQuyTrinhDeNghiTuyenDung")]
        public IActionResult ChuyenTiepListQuyTrinhDeNghiTuyenDung([FromBody] List<string> listId) =>
            Ok(funKH.ChuyenTiepListQuyTrinhDeNghiTuyenDung(db, listId));

        #endregion

        #region định biên cơ sở

        [HttpPost("GetListQuyTrinhDinhBienCoSo")]
        public IActionResult GetListQuyTrinhDinhBienCoSo([FromBody] dynamic data) => Ok(funKH.GetListQuyTrinhDinhBienCoSo(db, data));

        [HttpGet("GetNextSoQuyTrinhDinhBienCoSo")]
        public IActionResult GetNextSoQuyTrinhDinhBienCoSo() => Ok(funKH.GetNextSoQuyTrinhDinhBienCoSo(db));

        [HttpPost("SetQuyTrinhDinhBienCoSo")]
        public IActionResult SetQuyTrinhDinhBienCoSo([FromBody] dynamic data) => Ok(funKH.SetQuyTrinhDinhBienCoSo(db, data));

        [HttpGet("GetQuyTrinhDinhBienCoSo")]
        public IActionResult GetQuyTrinhDinhBienCoSo(string Id) => Ok(funKH.GetQuyTrinhDinhBienCoSo(db, Id));

        [HttpGet("GetDinhBienCoSo")]
        public IActionResult GetDinhBienCoSo(int Nam, long IdCongTy, long IdChucDanh) =>
            Ok(funKH.GetDinhBienCoSo(db, Nam, IdCongTy, IdChucDanh));

        [HttpPost("ChuyenTiepQuyTrinhDinhBienCoSo")]
        public IActionResult ChuyenTiepQuyTrinhDinhBienCoSo([FromBody] dynamic data) =>
            Ok(funKH.ChuyenTiepQuyTrinhDinhBienCoSo(db, data));

        [HttpPost("KhongDuyetQuyTrinhDinhBienCoSo")]
        public IActionResult KhongDuyetQuyTrinhDinhBienCoSo([FromBody] dynamic data) =>
            Ok(funKH.KhongDuyetQuyTrinhDinhBienCoSo(db, data));

        [HttpGet("DeleteQuyTrinhDinhBienCoSo")]
        public IActionResult DeleteQuyTrinhDinhBienCoSo(string Id) => Ok(funKH.DeleteQuyTrinhDinhBienCoSo(db, Id));

        [HttpGet("GetListQuyTrinhDinhBienCoSoDaKetThuc")]
        public IActionResult GetListQuyTrinhDinhBienCoSoDaKetThuc() => Ok(funKH.GetListQuyTrinhDinhBienCoSoDaKetThuc(db));

        [HttpGet("ExportFileMauDinhBienCoSo")]
        public IActionResult ExportFileMauDinhBienCoSo() => Ok(funKH.ExportFileMauDinhBienCoSo(db));

        [HttpGet("ImportDieuChinhLuongNhanVien")]
        public IActionResult ImportDieuChinhLuongNhanVien(string FileName) => Ok(funKH.ImportDieuChinhLuongNhanVien(db, FileName));

        [HttpPost("GetTongHopDinhBienCoSo")]
        public IActionResult GetTongHopDinhBienCoSo([FromBody] TimKiemBangLuong timkiem) =>
            Ok(funKH.GetTongHopDinhBienCoSo(db, timkiem.Nam, timkiem.listIdCongTy));

        #endregion

        #region Đề nghị tuyển dụng

        [HttpGet("GetMaDeNghiTuyenDung")]
        public IActionResult GetMaDeNghiTuyenDung() => Ok(func.GetNextMaDeNghiTuyenDung(db));

        [HttpPost("GetDanhSachDeNghiTuyenDung")]
        public IActionResult GetDanhSachDeNghiTuyenDung([FromBody] dynamic dyn) => Ok(func.GetDanhSachDeNghiTuyenDung(dyn, db));

        [HttpPost("SetDeNghiTuyenDung")]
        public IActionResult SetDeNghiTuyenDung([FromBody] HRM_TuyenDung_DeNghi model) => Ok(func.SetDeNghiTuyenDung(model, db));

        [HttpGet("GetDeNghiTuyenDung")]
        public IActionResult GetDeNghiTuyenDung(string Id) => Ok(func.GetDeNghiTuyenDung(Id, db));

        [HttpPost("DeleteDeNghiTuyenDung")]
        public IActionResult DeleteDeNghiTuyenDung([FromBody] List<string> lstId) => Ok(func.DeleteDeNghiTuyenDung(lstId, db));

        [HttpPost("ChuyenTiepDeNghiTuyenDung")]
        public IActionResult ChuyenTiepDeNghiTuyenDung([FromBody] HRM_TuyenDung_DeNghi model) =>
            Ok(func.ChuyenTiepDeNghiTuyenDung(model, db));

        [HttpPost("KhongDuyetDeNghiTuyenDung")]
        public IActionResult KhongDuyetDeNghiTuyenDung([FromBody] HRM_TuyenDung_DeNghi model) =>
            Ok(func.KhongDuyetDeNghiTuyenDung(model, db));

        #endregion

        #region TòngHopDeNghiTuyenDung

        [HttpGet("GetNextMaTongDeNghiTuyenDung")]
        public IActionResult GetNextMaTongDeNghiTuyenDung() => Ok(func.GetNextMaTongDeNghiTuyenDung(db));

        [HttpGet("GetDanhSachDeNghiTuyenDungDaDuyet")]
        public IActionResult GetDanhSachDeNghiTuyenDungDaDuyet(long IdCongTyEOS, double TuNgayUnix, double DenNgayUnix) =>
            Ok(func.GetDanhSachDeNghiTuyenDungDaDuyet(IdCongTyEOS, TuNgayUnix, DenNgayUnix, db));

        [HttpGet("DeleteTongHopDeNghiTuyenDung")]
        public IActionResult DeleteTongHopDeNghiTuyenDung(string Id) => Ok(func.DeleteTongHopDeNghiTuyenDung(Id, db));

        [HttpGet("GetTongHopDeNghiQuyTrinhById")]
        public IActionResult GetTongHopDeNghiQuyTrinhById(string Id) => Ok(func.GetTongHopDeNghiQuyTrinhById(Id, db));

        [HttpPost("GetTongHopDeNghiTuyenDung")]
        public IActionResult GetTongHopDeNghiTuyenDung([FromBody] dynamic dyn) => Ok(func.GetTongHopDeNghiTuyenDung(dyn, db));

        [HttpPost("GetDanhSachTopHophDeNghiTuyenDung")]
        public IActionResult GetDanhSachTopHophDeNghiTuyenDung([FromBody] dynamic dyn) =>
            Ok(func.GetDanhSachTopHophDeNghiTuyenDung(dyn, db));

        [HttpPost("SetQuyTrinhTongHopDeNghiTuyenDung")]
        public IActionResult SetQuyTrinhTongHopDeNghiTuyenDung([FromBody] HRM_TuyenDung_DeNghi_TongHop model) =>
            Ok(func.SetQuyTrinhTongHopDeNghiTuyenDung(model, db));

        [HttpPost("ChuyenTiepTongHopDeNghiTuyenDung")]
        public IActionResult ChuyenTiepTongHopDeNghiTuyenDung([FromBody] HRM_TuyenDung_DeNghi_TongHop model) =>
            Ok(func.ChuyenTiepTongHopDeNghiTuyenDung(model, db));

        [HttpPost("KhongTongHopDuyetDeNghiTuyenDung")]
        public IActionResult KhongTongHopDuyetDeNghiTuyenDung([FromBody] HRM_TuyenDung_DeNghi_TongHop model) =>
            Ok(func.KhongTongHopDuyetDeNghiTuyenDung(model, db));

        #endregion

        [HttpGet("GetDanhSachTongHopDeNghiTuyenDungDaDuyet")]
        public IActionResult GetDanhSachTongHopDeNghiTuyenDungDaDuyet(long IdCongTyEOS, double TuNgayUnix, double DenNgayUnix) =>
            Ok(func.GetDanhSachTongHopDeNghiTuyenDungDaDuyet(IdCongTyEOS, TuNgayUnix, DenNgayUnix, db));

        #region Tiep nhận úng viên

        [HttpGet("GetNextMaUngVien")]
        public IActionResult GetNextMaUngVien() => Ok(funcTD.GetNextMaUngVien(db));

        [HttpGet("GetThongTinUngVienById")]
        public IActionResult GetThongTinUngVienById(string Id) => Ok(funcTD.GetThongTinUngVienById(Id, db));

        [HttpGet("GetDanhMucChucDanhTheoDeNghiTuyenDung")]
        public IActionResult GetDanhMucChucDanhTheoDeNghiTuyenDung(string Id) =>
            Ok(funcTD.GetDanhMucChucDanhTheoDeNghiTuyenDung(Id, db));

        [HttpGet("ImportDanhSachUngVien")]
        public IActionResult ImportDanhSachUngVien(string FileName, string IdNhuCauChiTiet = "", bool isCongNhan = false) =>
            Ok(funcTD.ImportDanhSachUngVien(db, FileName, IdNhuCauChiTiet, isCongNhan));

        [HttpGet("ImportDanhSachNhanVien")]
        public IActionResult ImportDanhSachNhanVien(string FileName) => Ok(funcTD.ImportDanhSachNhanVien(db, FileName));

        [HttpGet("ImportDanhSachNhanVienUpdate")]
        public IActionResult ImportDanhSachNhanVienUpdate(string FileName) => Ok(funcTD.ImportDanhSachNhanVienUpdate(db, FileName));

        [HttpGet("ExportFileMauDanhSachUngVienChuaSoVan")]
        public IActionResult ExportFileMauDanhSachUngVienChuaSoVan() => Ok(funcTD.ExportFileMauDanhSachUngVienChuaSoVan(db));

        [HttpGet("ExportBaoCaoTuyenDungPHT")]
        public IActionResult ExportBaoCaoTuyenDungPHT(int Nam, int Thang) => Ok(funcTD.ExportBaoCaoTuyenDungPHT(Thang, Nam, db));

        [HttpGet("ExportDanhSachNhanVienTrungTuyen")]
        public IActionResult ExportDanhSachNhanVienTrungTuyen(int Nam, int Thang, bool isCongNhan = false) =>
            Ok(funcTD.ExportDanhSachNhanVienTrungTuyen(Thang, Nam, db, isCongNhan));

        [HttpGet("ExportDanhSachNhanVienTrungTuyenThuCoc")]
        public IActionResult ExportDanhSachNhanVienTrungTuyenThuCoc(int Nam, int Thang, bool isCongNhan) =>
            Ok(funcTD.ExportDanhSachNhanVienTrungTuyenThuCoc(Thang, Nam, isCongNhan, db));

        [HttpGet("ExportBaoCaoTheoQuy")]
        public IActionResult ExportBaoCaoTheoQuy(int Nam,bool isCongNhan=false) => Ok(funcTD.ExportBaoCaoTheoQuy(Nam, db, isCongNhan));

        [HttpPost("ExportBaoCaoTongHopNhanSu")]
        public IActionResult ExportBaoCaoTheoDoTuoi([FromBody] EntityGetDanhSachNhanVienRequest timkiem) =>
            Ok(funcTD.ExportBaoCaoTongHopNhanSu(db, timkiem));

        [HttpGet("GetDanhSachNhanVienSoVanUpdate")]
        public IActionResult GetDanhSachNhanVienSoVanUpdate() => Ok(funcTD.GetDanhSachNhanVienSoVanUpdate(db));

        [HttpGet("DeleteUngVien")]
        public IActionResult DeleteUngVien(string Id) => Ok(funcTD.DeleteUngVien(Id, db));

        [HttpPost("DeletelistUngVien")]
        public IActionResult DeletelistUngVien([FromBody] List<string> Id) => Ok(funcTD.DeletelistUngVien(Id, db));

        [HttpPost("SetUngVienNhanViec")]
        public IActionResult SetUngVienNhanViec([FromBody] List<string> Id) => Ok(funcTD.SetUngVienNhanViec(Id, true, db));

        [HttpPost("SetDaUngVienSoVanBylistId")]
        public IActionResult SetDaUngVienSoVanBylistId([FromBody] List<string> Id) => Ok(funcTD.SetDaUngVienSoVanBylistId(Id, db));

        [HttpPost("SetDatPhongVanBylistId")]
        public IActionResult SetDatPhongVanBylistId([FromBody] List<string> Id) => Ok(funcTD.SetDatPhongVanBylistId(Id, db));

        [HttpGet("HuyNhanViecUngVien")]
        public IActionResult HuyNhanViecUngVien(string Id) => Ok(funcTD.HuyNhanViecUngVien(Id, db));

        [HttpPost("SetUngVienNghiSauNhanViec")]
        public IActionResult SetUngVienNghiSauNhanViec([FromBody] List<string> Id) => Ok(funcTD.SetUngVienNhanViec(Id, false, db));

        [HttpPost("SetThongTinUngVien")]
        public IActionResult SetThongTinUngVien([FromBody] HRM_TuyenDung_UngVien model) => Ok(funcTD.SetThongTinUngVien(model, db));

        #endregion

        #region Sơ vấn

        [HttpPost("GetDanhSachNhanVienSoVan")]
        public IActionResult GetDanhSachNhanVienSoVan([FromBody] dynamic model) => Ok(funcTD.GetDanhSachNhanVienSoVan(model, db));

        [HttpPost("SetSoVanUngVien")]
        public IActionResult SetSoVanUngVien([FromBody] HRM_TuyenDung_UngVien_SoVan model) => Ok(funcTD.SetSoVanUngVien(model, db));

        [HttpGet("GetSoVanUngVienById")]
        public IActionResult GetSoVanUngVienById(string Id) => Ok(funcTD.GetSoVanUngVienById(Id, db));

        #endregion
        #region PV

        [HttpPost("GetListLichPhongVan")]
        public IActionResult GetListLichPhongVan([FromBody] dynamic dyn) => Ok(funcTD.GetListLichPhongVan(dyn, db));

        [HttpPost("SetLichPhongVanNhieuUngVien")]
        public IActionResult SetLichPhongVanNhieuUngVien([FromBody] LichPhongVanViewModel model) =>
            Ok(funcTD.SetLichPhongVanNhieuUngVien(model, db));

        [HttpPost("SetQuyTrinhDatLichPhongVan")]
        public IActionResult SetQuyTrinhDatLichPhongVan([FromBody] HRM_TuyenDung_QuyTrinhPhongVan model) =>
            Ok(funcTD.SetQuyTrinhDatLichPhongVan(model, db));

        [HttpPost("PheDuyetNhieuQuyTrinhDatLichPhongVanDat")]
        public IActionResult PheDuyetNhieuQuyTrinhDatLichPhongVanDat([FromBody] List<string> listId) =>
            Ok(funcTD.PheDuyetNhieuQuyTrinhDatLichPhongVanDat(listId, db));

        [HttpPost("SetTrangThaiVongPhongVan")]
        public IActionResult SetTrangThaiVongPhongVan([FromBody] HRM_TuyenDung_VongPhongVan model) =>
            Ok(funcTD.SetTrangThaiVongPhongVan(model, db));

        [HttpGet("GetDatLichPhongVanById")]
        public IActionResult GetDatLichPhongVanById(string Id) => Ok(funcTD.GetDatLichPhongVanById(Id, db));

        [HttpGet("GetDatLichPhongVanByIdUngVien")]
        public IActionResult GetDatLichPhongVanByIdUngVien(string IdUngVien) =>
            Ok(funcTD.GetDatLichPhongVanByIdUngVien(IdUngVien, db));

        [HttpGet("DeleteDatLichPhongVan")]
        public IActionResult DeleteDatLichPhongVan(string Id) => Ok(funcTD.DeleteDatLichPhongVan(Id, db));

        [HttpGet("ChuyenDuyetNhieuQuyTrinhDatLichPhongVanDat")]
        public IActionResult ChuyenDuyetNhieuQuyTrinhDatLichPhongVanDat() => Ok(funcTD.ChuyenDuyetNhieuQuyTrinhDatLichPhongVanDat(db));

        [HttpGet("addCongTyNhanVien")]
        public IActionResult addCongTyNhanVien()
        {
            funcTD.addCongTyNhanVien(db);
            return Ok("OK");
        }

        [HttpGet("GetNextMaQuyTrinhPhongVan")]
        public IActionResult GetNextMaQuyTrinhPhongVan() => Ok(funcTD.GetNextMaQuyTrinhPhongVan(db));

        [HttpPost("GetDanhSachLichPhongVanChuTri")]
        public IActionResult GetDanhSachLichPhongVanChuTri([FromBody] dynamic dyn) =>
            Ok(funcTD.GetDanhSachLichPhongVanChuTri(dyn, db));

        [HttpGet("ChuyenTrangThaiLichPhongVan")]
        public IActionResult ChuyenTrangThaiLichPhongVan(string Id, int TrangThai) =>
            Ok(funcTD.ChuyenTrangThaiLichPhongVan(Id, TrangThai, db));

        [HttpGet("GetThongTinVongPhongVan")]
        public IActionResult GetThongTinVongPhongVan(string Id, int Loai) => Ok(funcTD.GetThongTinVongPhongVan(Id, Loai, db));

        [HttpPost("SetNhanXetNguoiPhongVan")]
        public IActionResult SetNhanXetNguoiPhongVan([FromBody] HRM_TuyenDung_NguoiPhongVan model) =>
            Ok(funcTD.SetNhanXetNguoiPhongVan(model, db));

        #endregion

        [HttpGet("GetNextSoQuyTrinhPheDuyetLuong")]
        public IActionResult GetNextPheDuyetLuong() => Ok(funcTD.GetNextPheDuyetLuong(db));

        [HttpGet("UpdateDonVi")]
        public IActionResult UpdateDonVi()
        {
            funcTD.UpdateDonVi(db);
            
            return Ok("OK");
        }

        [HttpPost("GetDanhSachQuyTrinhPheDuyetLuong")]
        public IActionResult GetDanhSachQuyTrinhPheDuyetLuong([FromBody] dynamic model) =>
            Ok(funcTD.GetDanhSachQuyTrinhPheDuyetLuong(model, db));

        [HttpPost("SetQuyTrinhPheDuyetLuong")]
        public IActionResult SetQuyTrinhPheDuyetLuong([FromBody] HRM_TuyenDung_QuyTrinhPheDuyetLuong model) =>
            Ok(funcTD.SetQuyTrinhPheDuyetLuong(model, db));

        [HttpPost("FuncQuyTrinhPheDuyetLuongKhongDuyet")]
        public IActionResult FuncQuyTrinhPheDuyetLuongKhongDuyet([FromBody] HRM_TuyenDung_QuyTrinhPheDuyetLuong model) =>
            Ok(funcTD.FuncQuyTrinhPheDuyetLuongKhongDuyet(model, db));

        [HttpPost("ChuyenTiepQuyTrinhPheDuyetLuong")]
        public IActionResult ChuyenTiepQuyTrinhPheDuyetLuong([FromBody] HRM_TuyenDung_QuyTrinhPheDuyetLuong model) =>
            Ok(funcTD.ChuyenTiepQuyTrinhPheDuyetLuong(model, db));

        [HttpPost("KhongDuyetQuyTrinhPheDuyetLuong")]
        public IActionResult KhongDuyetQuyTrinhPheDuyetLuong([FromBody] HRM_TuyenDung_QuyTrinhPheDuyetLuong model) =>
            Ok(funcTD.KhongDuyetQuyTrinhPheDuyetLuong(model, db));

        [HttpGet("GetByIdQuyTrinhPheDuyetLuongUngVien")]
        public IActionResult GetByIdQuyTrinhPheDuyetLuongUngVien(string Id) => Ok(funcTD.GetByIdQuyTrinhPheDuyetLuongUngVien(Id, db));

        [HttpGet("GetDanhSachUngVienPheDuyetLuong")]
        public IActionResult GetDanhSachUngVienPheDuyetLuong(long IdCongTy, long IdBoPhan = 0) =>
            Ok(funcTD.GetDanhSachUngVienPheDuyetLuong(IdCongTy, IdBoPhan, db));

        [HttpGet("DeleteQuyTrinhPheDuyetLuongUngVien")]
        public IActionResult DeleteQuyTrinhPheDuyetLuongUngVien(string Id) => Ok(funcTD.DeleteQuyTrinhPheDuyetLuongUngVien(Id, db));

        [HttpPost("GetDanhSachTongHopDeXuatLuongDaDuyet")]
        public IActionResult GetDanhSachTongHopDeXuatLuongDaDuyet([FromBody] dynamic dyn)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            return Ok(funcTD.GetDanhSachTongHopDeXuatLuongDaDuyet(itemTimKiem, db));
        }

        [HttpPost("ExportDanhSachTongHopDeXuatLuongDaDuyet")]
        public IActionResult ExportDanhSachTongHopDeXuatLuongDaDuyet([FromBody] dynamic dyn)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            return Ok(funcTD.ExportDanhSachTongHopDeXuatLuongDaDuyet(itemTimKiem, db));
        }

        [HttpPost("ExportDanhSachNhanVienSoVan")]
        public IActionResult ExportDanhSachNhanVienSoVan([FromBody] dynamic dyn)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            return Ok(funcTD.ExportDanhSachNhanVienSoVan(itemTimKiem, db));
        }

        [HttpGet("GetAnTabChuaSoVan")]
        public IActionResult GetAnTabChuaSoVan() => Ok(funcTD.GetAnTabChuaSoVan());
    }
}
