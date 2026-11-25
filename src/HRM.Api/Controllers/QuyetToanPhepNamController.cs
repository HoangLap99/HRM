#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.BangLuong;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("QuyetToanPhep")]
    public class QuyetToanPhepNamController(HrmDbContext db, FuncQuyetToanNghiBu func) : ApiControllerBase
    {
        [HttpGet("GetDanhSachNhanVienQuyetToanPhep")]
        public IActionResult GetDanhSachNhanVienQuyetToanPhep(long IdCongTyEOS, int Nam)
            => Ok(func.GetDanhSachNhanVienQuyetToanPhep(IdCongTyEOS, Nam, db));

        [HttpGet("GetNextMaQuyetToanNghiPhep")]
        public IActionResult GetNextMaQuyetToanNghiPhep()
            => Ok(func.GetNextMaQuyetToanNghiPhep(db));

        [HttpPost("GetDanhSachQuyetToanNghiPhep")]
        public IActionResult GetDanhSachQuyetToanNghiPhep([FromBody] dynamic dyn)
            => Ok(func.GetDanhSachQuyetToanNghiPhep(dyn, db));

        [HttpPost("SetQuyetToanNghiPhep")]
        public IActionResult SetQuyetToanNghiPhep([FromBody] HRM_QuyetToanPhepNam model)
            => Ok(func.SetQuyetToanNghiPhep(model, db));

        [HttpGet("GetQuyetToanNghiPhepById")]
        public IActionResult GetQuyetToanNghiPhepById(string Id)
            => Ok(func.GetQuyetToanNghiPhepById(Id, db));

        [HttpGet("DeleteQuyetToanNghiPhep")]
        public IActionResult DeleteQuyetToanNghiPhep(string Id)
            => Ok(func.DeleteQuyetToanNghiPhep(Id, db));

        [HttpPost("ChuyenTiepQuyetToanNghiPhep")]
        public IActionResult ChuyenTiepQuyetToanNghiPhep([FromBody] HRM_QuyetToanPhepNam model)
            => Ok(func.ChuyenTiepQuyetToanNghiPhep(model, db));

        [HttpPost("KhongDuyetQuyetToanNghiPhep")]
        public IActionResult KhongDuyetQuyetToanNghiPhep([FromBody] HRM_QuyetToanPhepNam model)
            => Ok(func.KhongDuyetQuyetToanNghiPhep(model, db));
    }
}
