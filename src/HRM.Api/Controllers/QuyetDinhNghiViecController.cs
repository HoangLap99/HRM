#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.NhanVien;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("QuyetDinhNghiViec")]
    public class QuyetDinhNghiViecController(HrmDbContext db, FuncNghiViec func) : ApiControllerBase
    {
        [HttpGet("GetNextQuyetDinhNghiViec")]
        public IActionResult GetNextQuyetDinhNghiViec()
            => Ok(func.GetNextQuyetDinhNghiViec(db));

        [HttpPost("GetDanhSachQuyetDinhNghiViec")]
        public IActionResult GetDanhSachQuyetDinhNghiViec([FromBody] dynamic dyn)
            => Ok(func.GetDanhSachQuyetDinhNghiViec(dyn, db));

        [HttpPost("SetQuyetDinhNghiViec")]
        public IActionResult SetQuyetDinhNghiViec([FromBody] HRM_QuyetDinhNghiViec data)
            => Ok(func.SetQuyetDinhNghiViec(data, db));

        [HttpPost("ChuyenTiepQuyetDinhNghiViec")]
        public IActionResult ChuyenTiepQuyetDinhNghiViec([FromBody] HRM_QuyetDinhNghiViec data)
            => Ok(func.ChuyenTiepQuyetDinhNghiViec(data, db));

        [HttpPost("KhongDuyetQuyetDinhNghiViec")]
        public IActionResult KhongDuyetQuyetDinhNghiViec([FromBody] HRM_QuyetDinhNghiViec data)
            => Ok(func.KhongDuyetQuyetDinhNghiViec(data, db));

        [HttpGet("GetQuyetDinhNghiViec")]
        public IActionResult GetQuyetDinhNghiViec(string Id)
            => Ok(func.GetQuyetDinhNghiViec(Id, db));

        [HttpGet("DeleteQuyetDinhNghiViec")]
        public IActionResult DeleteQuyetDinhNghiViec(string Id)
            => Ok(func.DeleteQuyetDinhNghiViec(Id, db));

        [HttpGet("ExportQuyetDinhNghiViec")]
        public IActionResult ExportQuyetDinhNghiViec(string Id)
            => Ok(func.ExportQuyetDinhNghiViec(Id, db));

        [HttpGet("ExportQuyetDinhNghiViecNam")]
        public IActionResult ExportQuyetDinhNghiViecNam(int Nam, int Thang)
            => Ok(func.ExportQuyetDinhNghiViecNam(Nam, Thang, db));

        [HttpGet("ExportBaoCaoTongHopNghi")]
        public IActionResult ExportBaoCaoTongHopNghi(int Nam, int Thang)
            => Ok(func.ExportBaoCaoTongHopNghi(Nam, Thang, db));

        [HttpGet("UpdateNgayHieuLucNghiViec")]
        public IActionResult UpdateNgayHieuLucNghiViec()
        {
            func.UpdateNgayHieuLucNghiViec(db);
            return Ok("OK");
        }
    }
}
