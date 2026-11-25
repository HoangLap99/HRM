#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.NhanVien;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
////using HRM.Api.Models.SmartEOEModel;

namespace HRM.Api.Controllers
{
    [Route("KyLuat")]
    public class KyLuatController(HrmDbContext db, FuncKyLuat func) : ApiControllerBase
    {
        #region danhmucHinhThucKyLuat

        [HttpGet("GetListdmHinhThucKyLuat")]
        public IActionResult GetListdmHinhThucKyLuat() => Ok(func.GetListdmHinhThucKyLuat(db));

        [HttpPost("SetdmHinhThucKyLuat")]
        public IActionResult SetdmHinhThucKyLuat([FromBody] dynamic dnm) => Ok(func.SetdmHinhThucKyLuat(db, dnm));

        [HttpPost("DeletedmHinhThucKyLuat")]
        public IActionResult DeletedmHinhThucKyLuat([FromBody] dynamic dnm) => Ok(func.DeletedmHinhThucKyLuat(db, dnm));

        #endregion

        #region danhmucLoaiHinhThucKyLuatPhat

        [HttpGet("GetListdmLoaiHinhThucPhatKyLuat")]
        public IActionResult GetListdmLoaiHinhThucPhatKyLuat() => Ok(func.GetListdmLoaiHinhThucPhatKyLuat(db));

        [HttpPost("SetdmLoaiHinhThucPhatKyLuat")]
        public IActionResult SetdmLoaiHinhThucPhatKyLuat([FromBody] dynamic dnm) => Ok(func.SetdmLoaiHinhThucPhatKyLuat(db, dnm));

        [HttpPost("DeletedmLoaiHinhThucPhatKyLuat")]
        public IActionResult DeletedmLoaiHinhThucPhatKyLuat([FromBody] dynamic dnm) => Ok(func.DeletedmLoaiHinhThucPhatKyLuat(db, dnm));

        #endregion

        [HttpGet("GetNextSoPhieuQuyetDinhKyLuat")]
        public IActionResult GetNextSoPhieuQuyetDinhKyLuat() => Ok(func.GetNextSoPhieuQuyetDinhKyLuat(db));

        [HttpPost("GetDanhSachQuyetDinhKyLuat")]
        public IActionResult GetDanhSachQuyetDinhKyLuat([FromBody] dynamic dyn) => Ok(func.GetDanhSachQuyetDinhKyLuat(db, dyn));

        [HttpGet("GetLanKyLuatNhanVien")]
        public IActionResult GetLanKyLuatNhanVien(string IdNhanVien, int Nam) => Ok(func.GetLanKyLuatNhanVien(IdNhanVien, Nam, db));

        [HttpPost("GetdanhSachKyLuatNhanVienNam")]
        public IActionResult GetdanhSachKyLuatNhanVienNam([FromBody] dynamic dyn) => Ok(func.GetdanhSachKyLuatNhanVienNam(dyn, db));

        [HttpPost("ExportSachKyLuatNhanVienNam")]
        public IActionResult ExportSachKyLuatNhanVienNam([FromBody] dynamic dyn) => Ok(func.ExportSachKyLuatNhanVienNam(dyn, db));

        [HttpPost("SetQuyetDinhKyLuat")]
        public IActionResult SetQuyetDinhKyLuat([FromBody] HRM_QuyetDinhKyLuat model) => Ok(func.SetQuyetDinhKyLuat(db, model));

        [HttpGet("GetQuyetDinhKyLuat")]
        public IActionResult GetQuyetDinhKyLuat(string Id) => Ok(func.GetQuyetDinhKyLuat(db, Id));

        [HttpGet("DeleteQuyetDinhKyLuat")]
        public IActionResult DeleteQuyetDinhKyLuat(string Id) => Ok(func.DeleteQuyetDinhKyLuat(db, Id));

        [HttpPost("ChuyenTiepQuyetDinhKyLuat")]
        public IActionResult ChuyenTiepQuyetDinhKyLuat([FromBody] dynamic dnm) => Ok(func.ChuyenTiepQuyetDinhKyLuat(db, dnm));

        [HttpPost("KhongDuyetQuyetDinhKyLuat")]
        public IActionResult KhongDuyetQuyetDinhKyLuat([FromBody] dynamic dnm) => Ok(func.KhongDuyetQuyetDinhKyLuat(db, dnm));

        [HttpGet("ExportKyLuat")]
        public IActionResult ExportKyLuat(string Id, string IdNhanvien) => Ok(func.ExportKyLuat(Id, IdNhanvien, db));
    }
}
