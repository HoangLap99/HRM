#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.NhanVien;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("KhenThuong")]
    public class KhenThuongController(HrmDbContext db, FuncKhenThuong func) : ApiControllerBase
    {
        #region danhmucHinhThucKhenThuong
        [HttpGet("GetListdmHinhThucKhenThuong")]
        public IActionResult GetListdmHinhThucKhenThuong()
            => Ok(func.GetListdmHinhThucKhenThuong(db));

        [HttpPost("SetdmHinhThucKhenThuong")]
        public IActionResult SetdmHinhThucKhenThuong([FromBody] dynamic dnm)
            => Ok(func.SetdmHinhThucKhenThuong(db, dnm));

        [HttpPost("DeletedmHinhThucKhenThuong")]
        public IActionResult DeletedmHinhThucKhenThuong([FromBody] dynamic dnm)
            => Ok(func.DeletedmHinhThucKhenThuong(db, dnm));
        #endregion

        [HttpGet("GetMaQuyetDinhKhenThuong")]
        public IActionResult GetMaQuyetDinhKhenThuong()
            => Ok(func.GetNextSoPhieuQuyetDinhKhenThuong(db));

        [HttpPost("GetDanhSachQuyetDinhKhenThuong")]
        public IActionResult GetDanhSachQuyetDinhKhenThuong([FromBody] dynamic dyn)
            => Ok(func.GetDanhSachQuyetDinhKhenThuong(db, dyn));

        [HttpPost("SetQuyetDinhKhenThuong")]
        public IActionResult SetQuyetDinhKhenThuong([FromBody] HRM_QuyetDinhKhenThuong dyn)
            => Ok(func.SetQuyetDinhKhenThuong(db, dyn));

        [HttpGet("GetQuyetDinhKhenThuong")]
        public IActionResult GetQuyetDinhKhenThuong(string Id)
            => Ok(func.GetQuyetDinhKhenThuong(db, Id));

        [HttpPost("DeleteQuyetDinhKhenThuong")]
        public IActionResult DeleteQuyetDinhKhenThuong([FromBody] dynamic dnm)
            => Ok(func.DeleteQuyetDinhKhenThuong(db, dnm));

        [HttpPost("ChuyenTiepQuyetDinhKhenThuong")]
        public IActionResult ChuyenTiepQuyetDinhKhenThuong([FromBody] dynamic dnm)
            => Ok(func.ChuyenTiepQuyetDinhKhenThuong(db, dnm));

        [HttpPost("KhongDuyetQuyetDinhKhenThuong")]
        public IActionResult KhongDuyetQuyetDinhKhenThuong([FromBody] HRM_QuyetDinhKhenThuong dnm)
            => Ok(func.KhongDuyetQuyetDinhKhenThuong(db, dnm));

        [HttpGet("ExportKhenThuongNhanVien")]
        public IActionResult ExportKhenThuongNhanVien(string Id, string IdNhanVien)
            => Ok(func.ExportKhenThuongNhanVien(Id, IdNhanVien, db));

        [HttpGet("ExportQuyetDinhKhenThuong")]
        public IActionResult ExportQuyetDinhKhenThuong(string Id)
            => Ok(func.ExportQuyetDinhKhenThuong(Id, db));
    }
}
