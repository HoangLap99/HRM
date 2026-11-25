#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.PhucLoi;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("PhucLoi")]
    public class PhucLoiController(HrmDbContext db, FuncPhucLoi func) : ApiControllerBase
    {
        [HttpGet("GetNextKeHoachPhucLoi")]
        public IActionResult GetNextKeHoachPhucLoi() => Ok(func.GetNextKeHoachPhucLoi(db));

        [HttpPost("GetListKeHoachPhucLoi")]
        public IActionResult GetListKeHoachPhucLoi([FromBody] dynamic model) => Ok(func.GetListKeHoachPhucLoi(model, db));

        [HttpPost("SetKeHoachPhucLoi")]
        public IActionResult SetKeHoachPhucLoi([FromBody] HRM_KeHoachPhucLoi model) => Ok(func.SetKeHoachPhucLoi(model, db));

        [HttpPost("ChuyenTiepKeHoachPhucLoi")]
        public IActionResult ChuyenTiepKeHoachPhucLoi([FromBody] HRM_KeHoachPhucLoi model) => Ok(func.ChuyenTiepKeHoachPhucLoi(model, db));

        [HttpPost("KhongDuyetKeHoachPhucLoi")]
        public IActionResult KhongDuyetKeHoachPhucLoi([FromBody] HRM_KeHoachPhucLoi model) => Ok(func.KhongDuyetKeHoachPhucLoi(model, db));

        [HttpGet("GetByIdKeHoachPhucLoi")]
        public IActionResult GetByIdKeHoachPhucLoi(string Id) => Ok(func.GetByIdKeHoachPhucLoi(Id, db));

        [HttpGet("DeleteKeHoachPhucLoi")]
        public IActionResult DeleteKeHoachPhucLoi(string Id) => Ok(func.DeleteKeHoachPhucLoi(Id, db));

        [HttpGet("GetDanhSachNhanVienPhucLoi")]
        public IActionResult GetDanhSachNhanVienPhucLoi(long IdCongTyEOS, double NgayPhucLoiUnix, long IdChucVu, string IddmLoaiHopDong, string GioiTinh) =>
            Ok(func.GetDanhSachNhanVienPhucLoi(IdCongTyEOS, NgayPhucLoiUnix, IdChucVu, IddmLoaiHopDong, GioiTinh, db));

        [HttpGet("GetKeHoachPhucLoiDaDuyetByNam")]
        public IActionResult GetKeHoachPhucLoiDaDuyetByNam(long IdCongTyEOS, int Nam) =>
            Ok(func.GetKeHoachPhucLoiDaDuyetByNam(IdCongTyEOS, Nam, db));

        [HttpGet("GetChiTietKeHoachPhucLoi")]
        public IActionResult GetChiTietKeHoachPhucLoi(string IdChiTiet) => Ok(func.GetChiTietKeHoachPhucLoi(db, IdChiTiet));

        #region Phuc Loi

        [HttpGet("GetNextQuyTrinhPhucLoi")]
        public IActionResult GetNextQuyTrinhPhucLoi() => Ok(func.GetNextQuyTrinhPhucLoi(db));

        [HttpPost("GetListQuyTrinhPhucLoi")]
        public IActionResult GetListQuyTrinhPhucLoi([FromBody] dynamic model) => Ok(func.GetListQuyTrinhPhucLoi(model, db));

        [HttpPost("SetQuyTrinhPhucLoi")]
        public IActionResult SetQuyTrinhPhucLoi([FromBody] HRM_QuyTrinh_PhucLoi model) => Ok(func.SetQuyTrinhPhucLoi(model, db));

        [HttpPost("ChuyenTiepQuyTrinhPhucLoi")]
        public IActionResult ChuyenTiepQuyTrinhPhucLoi([FromBody] HRM_QuyTrinh_PhucLoi model) => Ok(func.ChuyenTiepQuyTrinhPhucLoi(model, db));

        [HttpPost("KhongDuyetQuyTrinhPhucLoi")]
        public IActionResult KhongDuyetQuyTrinhPhucLoi([FromBody] HRM_QuyTrinh_PhucLoi model) => Ok(func.KhongDuyetQuyTrinhPhucLoi(model, db));

        [HttpGet("GetByIdQuyTrinhPhucLoi")]
        public IActionResult GetByIdQuyTrinhPhucLoi(string Id) => Ok(func.GetByIdQuyTrinhPhucLoi(Id, db));

        [HttpGet("ExportDanhSachHuongPhucLoiChiTiet")]
        public IActionResult ExportDanhSachHuongPhucLoiChiTiet(string IdChiTiet) => Ok(func.ExportDanhSachHuongPhucLoiChiTiet(IdChiTiet, db));

        [HttpGet("DeleteQuyTrinhPhucLoi")]
        public IActionResult DeleteQuyTrinhPhucLoi(string Id) => Ok(func.DeleteQuyTrinhPhucLoi(Id, db));

        #endregion
    }
}
