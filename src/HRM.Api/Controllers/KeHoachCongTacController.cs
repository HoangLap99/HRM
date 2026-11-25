#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.CongTacPhi;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
using static HRM.Api.Services.CongTacPhi.FuncCongTacPhi;

namespace HRM.Api.Controllers
{
    [Route("KeHoachCongTac")]
    public class KeHoachCongTacController(HrmDbContext db, FuncCongTacPhi func) : ApiControllerBase
    {
        [HttpGet("GetNextMaKeHoachCongTac")]
        public IActionResult GetNextMaKeHoachCongTac() => Ok(func.GetNextMaKeHoachCongTac(db));

        [HttpPost("GetListQuyTrinhKeHoachCongTac")]
        public IActionResult GetListQuyTrinhKeHoachCongTac([FromBody] dynamic model) => Ok(func.GetListQuyTrinhKeHoachCongTac(model, db));

        [HttpPost("SetQuyTrinhKeHoachCongTac")]
        public IActionResult SetQuyTrinhKeHoachCongTac([FromBody] HRM_KeHoachCongTac model) => Ok(func.SetQuyTrinhKeHoachCongTac(model, db));

        [HttpPost("ChuyenTiepQuyTrinhKeHoachCongTac")]
        public IActionResult ChuyenTiepQuyTrinhKeHoachCongTac([FromBody] HRM_KeHoachCongTac model) => Ok(func.ChuyenTiepQuyTrinhKeHoachCongTac(model, db));

        [HttpPost("ExporQuyTrinhKeHoachCongTac")]
        public IActionResult ExporQuyTrinhKeHoachCongTac([FromBody] HRM_KeHoachCongTac model) => Ok(func.ExporQuyTrinhKeHoachCongTac(model, db));

        [HttpPost("KhongDuyetQuyTrinhKeHoachCongTac")]
        public IActionResult KhongDuyetQuyTrinhKeHoachCongTac([FromBody] HRM_KeHoachCongTac model) => Ok(func.KhongDuyetQuyTrinhKeHoachCongTac(model, db));

        [HttpGet("GetByIdQuyTrinhKeHoachCongTac")]
        public IActionResult GetByIdQuyTrinhKeHoachCongTac(string Id) => Ok(func.GetByIdQuyTrinhKeHoachCongTac(Id, db));

        [HttpGet("GetDanhSachChiPhiQuyDinh")]
        public IActionResult GetDanhSachChiPhiQuyDinh(long IdBoPhanEOS, long IdChucVuEOS, double NgayDi) =>
            Ok(func.GetDanhSachChiPhiQuyDinh(IdBoPhanEOS, IdChucVuEOS, NgayDi, db));

        [HttpGet("DeleteQuyTrinhKeHoachCongTac")]
        public IActionResult DeleteQuyTrinhKeHoachCongTac(string Id) => Ok(func.DeleteQuyTrinhKeHoachCongTac(Id, db));

        [HttpGet("GetNextMaKetQuaCongTac")]
        public IActionResult GetNextMaKetQuaCongTac() => Ok(func.GetNextMaKetQuaCongTac(db));

        [HttpPost("GetListQuyTrinhKetQuaCongTac")]
        public IActionResult GetListQuyTrinhKetQuaCongTac([FromBody] dynamic model) => Ok(func.GetListQuyTrinhKetQuaCongTac(model, db));

        [HttpPost("SetQuyTrinhKetQuaCongTac")]
        public IActionResult SetQuyTrinhKetQuaCongTac([FromBody] HRM_KetQuaCongTac model) => Ok(func.SetQuyTrinhKetQuaCongTac(model, db));

        [HttpPost("ChuyenTiepQuyTrinhKetQuaCongTac")]
        public IActionResult ChuyenTiepQuyTrinhKetQuaCongTac([FromBody] HRM_KetQuaCongTac model) => Ok(func.ChuyenTiepQuyTrinhKetQuaCongTac(model, db));

        [HttpPost("KhongDuyetQuyTrinhKetQuaCongTac")]
        public IActionResult KhongDuyetQuyTrinhKetQuaCongTac([FromBody] HRM_KetQuaCongTac model) => Ok(func.KhongDuyetQuyTrinhKetQuaCongTac(model, db));

        [HttpPost("ExportQuyTrinhKetQuaCongTac")]
        public IActionResult ExportQuyTrinhKetQuaCongTac([FromBody] HRM_KetQuaCongTac model) => Ok(func.ExportQuyTrinhKetQuaCongTac(model, db));

        [HttpGet("GetByIdQuyTrinhKetQuaCongTac")]
        public IActionResult GetByIdQuyTrinhKetQuaCongTac(string Id) => Ok(func.GetByIdQuyTrinhKetQuaCongTac(Id, db));

        [HttpGet("DeleteQuyTrinhKetQuaCongTac")]
        public IActionResult DeleteQuyTrinhKetQuaCongTac(string Id) => Ok(func.DeleteQuyTrinhKetQuaCongTac(Id, db));

        [HttpGet("GetBaoCaoChiPhiCongTac")]
        public IActionResult GetBaoCaoChiPhiCongTac(int Nam) => Ok(func.GetBaoCaoChiPhiCongTacNewNew(db, Nam));

        [HttpGet("GetBaoCaoChiPhiCongTacNew")]
        public IActionResult GetBaoCaoChiPhiCongTacNew(int Nam) => Ok(func.GetBaoCaoChiPhiCongTacNew(db, Nam));

        [HttpGet("ExportBaoCaoChiPhiCongTac")]
        public IActionResult ExportBaoCaoChiPhiCongTac(int Nam) => Ok(func.ExportBaoCaoChiPhiCongTac(db, Nam));
    }
}
