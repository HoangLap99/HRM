#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.XinNghiPhep;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("HanhChinhVanPhong")]
    public class XinNghiPhepController(HrmDbContext db, FuncXinNghiPhep func) : ApiControllerBase
    {
        #region QuyTrinhLamThemGio

        [HttpPost("GetListQuyTrinhLamThemGio")]
        public IActionResult GetListQuyTrinhLamThemGio([FromBody] dynamic data) => Ok(func.GetListQuyTrinhLamThemGio(db, data));

        [HttpGet("GetNextSoQuyTrinhLamThemGio")]
        public IActionResult GetNextSoQuyTrinhLamThemGio() => Ok(func.GetNextSoQuyTrinhLamThemGio(db));

        [HttpPost("SetQuyTrinhLamThemGio")]
        public IActionResult SetQuyTrinhLamThemGio([FromBody] dynamic data) => Ok(func.SetQuyTrinhLamThemGio(db, data));

        [HttpGet("GetQuyTrinhLamThemGio")]
        public IActionResult GetQuyTrinhLamThemGio(string Id) => Ok(func.GetQuyTrinhLamThemGio(db, Id));

        [HttpPost("ChuyenTiepQuyTrinhLamThemGio")]
        public IActionResult ChuyenTiepQuyTrinhLamThemGio([FromBody] dynamic data) => Ok(func.ChuyenTiepQuyTrinhLamThemGio(db, data));

        [HttpPost("KhongDuyetQuyTrinhLamThemGio")]
        public IActionResult KhongDuyetQuyTrinhLamThemGio([FromBody] dynamic data) => Ok(func.KhongDuyetQuyTrinhLamThemGio(db, data));

        [HttpPost("ExportYeuCauLamThemGio")]
        public IActionResult ExportYeuCauLamThemGio([FromBody] HRM_QuyTrinhLamThemGio data) => Ok(func.ExportYeuCauLamThemGio(data, db));

        [HttpPost("DeleteQuyTrinhLamThemGio")]
        public IActionResult DeleteQuyTrinhLamThemGio([FromBody] dynamic data) => Ok(func.DeleteQuyTrinhLamThemGio(db, data));

        [HttpGet("ExportXuatPhieudeNghiLamThemGio")]
        public IActionResult ExportXuatPhieudeNghiLamThemGio(string IdQuyTrinh) => Ok(func.ExportXuatPhieudeNghiLamThemGio(db, IdQuyTrinh));

        [HttpGet("ImportXuatPhieudeNghiLamThemGio")]
        public IActionResult ImportXuatPhieudeNghiLamThemGio(string fileName) => Ok(func.ImportXuatPhieudeNghiLamThemGio(db, fileName));

        [HttpGet("ExportMauLamThemGio")]
        public IActionResult ExportMauLamThemGio() => Ok(func.ExportMauLamThemGio());

        //[HttpGet]
        //[Route("GetDanhSachLamThemGioUser")]
        //public IActionResult GetDanhSachLamThemGioUser(double ThangNamUnix)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.GetDanhSachLamThemGioUser(db, ThangNamUnix);
        //    return Ok(item);
        //}

        #endregion

        #region QuyTrinhXinPhepNghi

        [HttpGet("ExportMauXinNghiPhep")]
        public IActionResult ExportMauXinNghiPhep() => Ok(func.ExportMauXinNghiPhep(db));

        //[HttpGet]
        //[Route("ImportDanhSachNghiPhep")]
        //public IActionResult ImportDanhSachNghiPhep(string FileName)
        //{

        //        var item = func.ImportDanhSachNghiPhep(db, FileName);
        //        return Ok(item);

        //}
        /// <summary>
        /// CurrentPage, TabTrangThai, TuNgay, DenNgay, KeyWord
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("GetListQuyTrinhXinNghiPhep")]
        public IActionResult GetListQuyTrinhXinNghiPhep([FromBody] dynamic data) => Ok(func.GetListQuyTrinhXinNghiPhep(db, data));

        //[HttpPost]
        //[Route("ExportQuyTrinhXinNghiPhep")]
        //public IActionResult ExportQuyTrinhXinNghiPhep([FromBody] dynamic data)
        //{
        //        var item = func.ExportQuyTrinhXinNghiPhep(db, data);
        //        return Ok(item);

        //}

        /// <summary>
        /// {"Id":"451f57b5-995e-45ff-99e0-08151eab7ba5","IdDuAn":0,"IdBoPhan":2,"IdTrangThai":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","TenTrangThai":"NhanVien","MucDich":"mục đích","ThoiGianBatDau":"2020-06-10T09:48:37.077","ThoiGianKetThuc":"2020-06-10T09:48:37.077","GhiChu":"31","ThoiGianBatDauUnix":null,"ThoiGianKetThucUnix":null,"Created":"2020-06-10T09:48:37.077","CreatedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","Modified":"2020-06-10T09:48:37.077","ModifiedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d"}
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNextSoQuyTrinhXinNghiPhep")]
        public IActionResult GetNextSoQuyTrinhXinNghiPhep() => Ok(func.GetNextSoQuyTrinhXinNghiPhep(db));

        /// <summary>
        /// {"Id":"451f57b5-995e-45ff-99e0-08151eab7ba5","IdDuAn":0,"IdBoPhan":2,"IdTrangThai":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","TenTrangThai":"NhanVien","MucDich":"mục đích","ThoiGianBatDau":"2020-06-10T09:48:37.077","ThoiGianKetThuc":"2020-06-10T09:48:37.077","GhiChu":"31","ThoiGianBatDauUnix":null,"ThoiGianKetThucUnix":null,"Created":"2020-06-10T09:48:37.077","CreatedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","Modified":"2020-06-10T09:48:37.077","ModifiedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d"}
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("SetQuyTrinhXinNghiPhep")]
        public IActionResult SetQuyTrinhXinNghiPhep([FromBody] dynamic data) => Ok(func.SetQuyTrinhXinNghiPhep(db, data));

        //[HttpPost]
        //[Route("AddNhanhXinNghiPhep")]
        //public IActionResult AddNhanhXinNghiPhep([FromBody] List<ImportPhieuXinNghiPhepModel> listQuyTrinh)
        //{


        //        var item = func.AddNhanhXinNghiPhep(listQuyTrinh, db);
        //        return Ok(item);

        //}

        [HttpGet("GetQuyTrinhXinNghiPhep")]
        public IActionResult GetQuyTrinhXinNghiPhep(string Id) => Ok(func.GetQuyTrinhXinNghiPhep(db, Id));

        [HttpGet("CapNhatTrangThaiHuyQuyTrinhXinNghiPhep")]
        public IActionResult CapNhatTrangThaiHuyQuyTrinhXinNghiPhep(string Id) => Ok(func.CapNhatTrangThaiHuyQuyTrinhXinNghiPhep(Id, db));

        /// <summary>
        /// {"Id":"451f57b5-995e-45ff-99e0-08151eab7ba5","IdDuAn":0,"IdBoPhan":2,"IdTrangThai":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","TenTrangThai":"NhanVien","MucDich":"mục đích","ThoiGianBatDau":"2020-06-10T09:48:37.077","ThoiGianKetThuc":"2020-06-10T09:48:37.077","GhiChu":"31","ThoiGianBatDauUnix":null,"ThoiGianKetThucUnix":null,"Created":"2020-06-10T09:48:37.077","CreatedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","Modified":"2020-06-10T09:48:37.077","ModifiedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d"}
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("ChuyenTiepQuyTrinhXinNghiPhep")]
        public IActionResult ChuyenTiepQuyTrinhXinNghiPhep([FromBody] dynamic data) => Ok(func.ChuyenTiepQuyTrinhXinNghiPhep(db, data));

        [HttpPost("ExportDonXinNghiNhanVien")]
        public IActionResult ExportDonXinNghiNhanVien([FromBody] HRM_QuyTrinhXinNghiPhep data) => Ok(func.ExportDonXinNghiNhanVien(data, db));

        /// <summary>
        /// {"Id":"451f57b5-995e-45ff-99e0-08151eab7ba5","IdDuAn":0,"IdBoPhan":2,"IdTrangThai":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","TenTrangThai":"NhanVien","MucDich":"mục đích","ThoiGianBatDau":"2020-06-10T09:48:37.077","ThoiGianKetThuc":"2020-06-10T09:48:37.077","GhiChu":"31","ThoiGianBatDauUnix":null,"ThoiGianKetThucUnix":null,"Created":"2020-06-10T09:48:37.077","CreatedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","Modified":"2020-06-10T09:48:37.077","ModifiedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d"}
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("KhongDuyetQuyTrinhXinNghiPhep")]
        public IActionResult KhongDuyetQuyTrinhXinNghiPhep([FromBody] dynamic data) => Ok(func.KhongDuyetQuyTrinhXinNghiPhep(db, data));

        [HttpPost("TongHopBaoCaoNgayPhep")]
        public IActionResult TongHopBaoCaoNgayPhep([FromBody] TimKiemBangLuong timkiem) =>
            Ok(func.TongHopBaoCaoNgayPhep(db, timkiem.IdCongty, timkiem.listIdBoPhan, timkiem.Nam, timkiem.Keyword));

        [HttpPost("ExportTongHopBaoCaoNgayPhep")]
        public IActionResult ExportTongHopBaoCaoNgayPhep([FromBody] dynamic data) => Ok(func.ExportTongHopBaoCaoNgayPhep(db, data));

        [HttpPost("TongHopBaoCaoNghiBu")]
        public IActionResult TongHopBaoCaoNghiBu([FromBody] TimKiemBangLuong timkiem) =>
            Ok(func.TongHopBaoCaoNghiBu(db, timkiem.IdCongty, timkiem.listIdBoPhan, timkiem.Nam, timkiem.Keyword));

        [HttpPost("ExportTongHopBaoCaoNghiBu")]
        public IActionResult ExportTongHopBaoCaoNghiBu([FromBody] dynamic data) => Ok(func.ExportTongHopBaoCaoNghiBu(db, data));

        /// <summary>
        /// {"Id":"451f57b5-995e-45ff-99e0-08151eab7ba5","IdDuAn":0,"IdBoPhan":2,"IdTrangThai":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","TenTrangThai":"NhanVien","MucDich":"mục đích","ThoiGianBatDau":"2020-06-10T09:48:37.077","ThoiGianKetThuc":"2020-06-10T09:48:37.077","GhiChu":"31","ThoiGianBatDauUnix":null,"ThoiGianKetThucUnix":null,"Created":"2020-06-10T09:48:37.077","CreatedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d","Modified":"2020-06-10T09:48:37.077","ModifiedBy":"00ef0b92-ee9e-4fea-963d-3734f4772b3d"}
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("DeleteQuyTrinhXinNghiPhep")]
        public IActionResult DeleteQuyTrinhXinNghiPhep([FromBody] dynamic data) => Ok(func.DeleteQuyTrinhXinNghiPhep(db, data));

        #endregion
    }
}
