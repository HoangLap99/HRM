#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.NhanVien;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
//using HRM.Api.Models.SmartEOEModel;

namespace HRM.Api.Controllers
{
    [Route("QuyetDinhBoNhiem")]
    public class QuyetDinhBoNhiemController(HrmDbContext db, FuncBoNhiemDieuDong func) : ApiControllerBase
    {
        [HttpGet("GetNextSoBoNhiemDieuDongDuAn"), HttpGet("~/GetNextSoBoNhiemDieuDongDuAn")]
        public IActionResult GetNextMaBoNhiemDieuDongDuAn() => Ok(func.GetNextMaBoNhiemDieuDongDuAn(db));

        [HttpPost("GetDanhSachBoNhiemDieuDongDuAn"), HttpPost("~/GetDanhSachBoNhiemDieuDongDuAn")]
        public IActionResult GetDanhSachBoNhiemDieuDongDuAn([FromBody] dynamic dyn) => Ok(func.GetDanhSachBoNhiemDieuDongDuAn(dyn, db));

        [HttpPost("SetQuyTrinhBoNhiemDieuDongDuAn"), HttpPost("~/SetQuyTrinhBoNhiemDieuDongDuAn")]
        public IActionResult SetQuyTrinhBoNhiemDieuDongDuAn([FromBody] HRM_BoNhiemDieuDongDuAn dyn) => Ok(func.SetQuyTrinhBoNhiemDieuDongDuAn(dyn, db));

        [HttpPost("SetQuyTrinhBoNhiemDieuDongDuAnCheck"), HttpPost("~/SetQuyTrinhBoNhiemDieuDongDuAnCheck")]
        public IActionResult SetQuyTrinhBoNhiemDieuDongDuAnCheck([FromBody] HRM_BoNhiemDieuDongDuAn dyn) =>
            Ok(func.SetQuyTrinhBoNhiemDieuDongDuAnCheck(dyn, db));

        [HttpGet("GetQuyTrinhBoNhiemDieuDongDuAn"), HttpGet("~/GetQuyTrinhBoNhiemDieuDongDuAn")]
        public IActionResult GetQuyTrinhBoNhiemDieuDongDuAn(string Id) => Ok(func.GetQuyTrinhBoNhiemDieuDongDuAn(Id, db));

        [HttpGet("DeleteBoNhiemDieuDongDuAn"), HttpGet("~/DeleteBoNhiemDieuDongDuAn")]
        public IActionResult DeleteBoNhiemDieuDongDuAn(string Id) => Ok(func.DeleteBoNhiemDieuDongDuAn(Id, db));

        [HttpPost("ChuyenTiepQuyTrinhBoNhiemDieuDongDuAn"), HttpPost("~/ChuyenTiepQuyTrinhBoNhiemDieuDongDuAn")]
        public IActionResult ChuyenTiepQuyTrinhBoNhiemDieuDongDuAn([FromBody] HRM_BoNhiemDieuDongDuAn model) =>
            Ok(func.ChuyenTiepQuyTrinhBoNhiemDieuDongDuAn(model, db));

        [HttpPost("KhongDuyetQuyTrinhBoNhiemDieuDongDuAn"), HttpPost("~/KhongDuyetQuyTrinhBoNhiemDieuDongDuAn")]
        public IActionResult KhongDuyetQuyTrinhBoNhiemChucVu([FromBody] HRM_BoNhiemDieuDongDuAn model) =>
            Ok(func.KhongDuyetQuyTrinhBoNhiemDieuDongDuAn(model, db));

        [HttpGet("ExportBoNhiemDieuDongDuAn"), HttpGet("~/ExportBoNhiemDieuDongDuAn")]
        public IActionResult ExportBoNhiemDieuDongDuAn(string Id) => Ok(func.ExportBoNhiemDieuDongDuAn(Id, db));

        #region Bổ nhiệm chức vụ

        [HttpGet("GetNextMaQuyTrinhBoNhiemChucVu"), HttpGet("~/GetNextMaQuyTrinhBoNhiemChucVu")]
        public IActionResult GetNextMaQuyTrinhBoNhiemChucVu() => Ok(func.GetNextMaQuyTrinhBoNhiemChucVu(db));

        [HttpGet("GetQuyTrinhBoNhiemChucVu"), HttpGet("~/GetQuyTrinhBoNhiemChucVu")]
        public IActionResult GetQuyTrinhBoNhiemChucVu(string Id) => Ok(func.GetQuyTrinhBoNhiemChucVu(Id, db));

        [HttpPost("GetDanhSachBoNhiemChucVu"), HttpPost("~/GetDanhSachBoNhiemChucVu")]
        public IActionResult GetDanhSachBoNhiemChucVu([FromBody] dynamic model) => Ok(func.GetDanhSachBoNhiemChucVu(model, db));

        [HttpPost("SetQuyQuyTrinhBoNhiemChucVu"), HttpPost("~/SetQuyQuyTrinhBoNhiemChucVu")]
        public IActionResult SetQuyQuyTrinhBoNhiemChucVu([FromBody] HRM_BoNhiemChucVu model) => Ok(func.SetQuyQuyTrinhBoNhiemChucVu(model, db));

        [HttpPost("ChuyenTiepQuyTrinhBoNhiemChucVu"), HttpPost("~/ChuyenTiepQuyTrinhBoNhiemChucVu")]
        public IActionResult ChuyenTiepQuyTrinhBoNhiemChucVu([FromBody] HRM_BoNhiemChucVu model) => Ok(func.ChuyenTiepQuyTrinhBoNhiemChucVu(model, db));

        [HttpGet("ChuyenTiepQuyTrinhBoNhiemChucVuUpdate"), HttpGet("~/ChuyenTiepQuyTrinhBoNhiemChucVuUpdate")]
        public IActionResult ChuyenTiepQuyTrinhBoNhiemChucVuUpdate()
        {
            func.ChuyenTiepQuyTrinhBoNhiemChucVuUpdate(db);
            return Ok("OK");
        }

        [HttpGet("ChuyenTiepQuyTrinhDieuDongUpdate"), HttpGet("~/ChuyenTiepQuyTrinhDieuDongUpdate")]
        public IActionResult ChuyenTiepQuyTrinhDieuDongUpdate()
        {
            func.ChuyenTiepQuyTrinhDieuDongUpdate(db);
            return Ok("OK");
        }

        [HttpPost("KhongDuyetQuyTrinhBoNhiemChucVu"), HttpPost("~/KhongDuyetQuyTrinhBoNhiemChucVu")]
        public IActionResult KhongDuyetQuyTrinhBoNhiemChucVu([FromBody] HRM_BoNhiemChucVu model) => Ok(func.KhongDuyetQuyTrinhBoNhiemChucVu(model, db));

        [HttpGet("DeleteQuyTrinhBoNhiemChucVu"), HttpGet("~/DeleteQuyTrinhBoNhiemChucVu")]
        public IActionResult DeleteQuyTrinhBoNhiemChucVu(string Id) => Ok(func.DeleteQuyTrinhBoNhiemChucVu(Id, db));

        [HttpGet("ExportBoNhiemChucVu"), HttpGet("~/ExportBoNhiemChucVu")]
        public IActionResult ExportBoNhiemChucVu(string Id) => Ok(func.ExportBoNhiemChucVu(Id, db));

        #endregion
    }
}
