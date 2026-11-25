#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.NhanVien;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("SaThai")]
    public class SaThaiController(HrmDbContext db, FuncSaThai func) : ApiControllerBase
    {
        [HttpGet("GetSoQuyetDinhSaThai"), Route("~/GetSoQuyetDinhSaThai")]
        public IActionResult GetMaQuyetDinhSaThai()
            => Ok(func.GetNextSoPhieuQuyetDinhSaThai(db));

        [HttpPost("GetDanhSachQuyetDinhSaThai"), Route("~/GetDanhSachQuyetDinhSaThai")]
        public IActionResult GetDanhSachQuyetDinhSaThai([FromBody] dynamic dyn)
            => Ok(func.GetDanhSachQuyetDinhSaThai(dyn, db));

        [HttpPost("SetQuyetDinhSaThai"), Route("~/SetQuyetDinhSaThai")]
        public IActionResult SetQuyetDinhSaThai([FromBody] HRM_QuyetDinhSaThai model)
            => Ok(func.SetQuyetDinhSaThai(model, db));

        [HttpGet("GetQuyetDinhSaThai"), Route("~/GetQuyetDinhSaThai")]
        public IActionResult GetQuyetDinhSaThai(string Id)
            => Ok(func.GetQuyetDinhSaThai(db, Id));

        [HttpPost("DeleteQuyetDinhSaThai"), Route("~/DeleteQuyetDinhSaThai")]
        public IActionResult DeleteQuyetDinhSaThai([FromBody] dynamic dnm)
            => Ok(func.DeleteQuyetDinhSaThai(db, dnm));

        [HttpPost("ChuyenTiepQuyetDinhSaThai"), Route("~/ChuyenTiepQuyetDinhSaThai")]
        public IActionResult ChuyenTiepQuyetDinhSaThai([FromBody] HRM_QuyetDinhSaThai model)
            => Ok(func.ChuyenTiepQuyetDinhSaThai(db, model));

        [HttpPost("KhongDuyetQuyetDinhSaThai"), Route("~/KhongDuyetQuyetDinhSaThai")]
        public IActionResult KhongDuyetQuyetDinhSaThai([FromBody] HRM_QuyetDinhSaThai model)
            => Ok(func.KhongDuyetQuyetDinhSaThai(db, model));

        [HttpGet("ExportSaThai"), Route("~/ExportSaThai")]
        public IActionResult ExportSaThai(string Id)
            => Ok(func.ExportSaThai(Id, db));
    }
}
