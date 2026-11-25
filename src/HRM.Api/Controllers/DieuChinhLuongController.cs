#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.NhanVien;
using HRM.Api.Services.XinNghiPhep;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
//using HRM.Api.Models.SmartEOEModel;

namespace HRM.Api.Controllers
{
    [Route("DieuChinhLuong")]
    public class DieuChinhLuongController(HrmDbContext db, FuncDieuChinhLuong func) : ApiControllerBase
    {
        [HttpGet("GetNextSoDieuChinhLuong")]
        public IActionResult GetNextMaDieuChinhLuong() => Ok(func.GetNextMaDieuChinhLuong(db));

        [HttpPost("GetListDieuChinhLuong")]
        public IActionResult GetDanhSachDieuChinhLuong([FromBody] dynamic dyn) => Ok(func.GetDanhSachDieuChinhLuong(dyn, db));

        [HttpPost("SetDieuChinhLuong")]
        public IActionResult SetQuyTrinhDieuChinhLuong([FromBody] HRM_QuyetDinhDieuChinhLuong data) => Ok(func.SetQuyTrinhDieuChinhLuong(data, db));

        [HttpPost("ChuyenTiepDieuChinhLuong")]
        public IActionResult ChuyenTiepQuyetDinhDieuChinhLuong([FromBody] HRM_QuyetDinhDieuChinhLuong data) =>
            Ok(func.ChuyenTiepQuyetDinhDieuChinhLuong(data, db));

        [HttpPost("ChuyenTiepQuyetDinhDieuChinhLuongCheck")]
        public IActionResult ChuyenTiepQuyetDinhDieuChinhLuongCheck([FromBody] HRM_QuyetDinhDieuChinhLuong data) =>
            Ok(func.ChuyenTiepQuyetDinhDieuChinhLuongCheck(data, db));

        [HttpPost("KhongDuyetDieuChinhLuong")]
        public IActionResult KhongDuyetQuyetDinhDieuChinhLuong([FromBody] HRM_QuyetDinhDieuChinhLuong data) =>
            Ok(func.KhongDuyetQuyetDinhDieuChinhLuong(data, db));

        [HttpPost("GetTongHopDieuChinhLuong")]
        public IActionResult GetTongHopDieuChinhLuong([FromBody] TimKiemBangLuong itemtimkiem) =>
            Ok(func.GetTongHopDieuChinhLuong(db, itemtimkiem.IdCongty, itemtimkiem.listIdBoPhan, itemtimkiem.Keyword));

        [HttpPost("ImportDieuChinhLuongNhanVien")]
        public IActionResult ImportDieuChinhLuongNhanVien([FromBody] TimKiemImport timKiem) =>
            Ok(func.ImportDieuChinhLuongNhanVienNew(db, timKiem.FileName, timKiem.listIdBoPhan));

        [HttpGet("ImportDieuChinhLuongNhanVienQT")]
        public IActionResult ImportDieuChinhLuongNhanVienQT(string FileName) => Ok(func.ImportDieuChinhLuongNhanVienQT(db, FileName));

        [HttpGet("ExportFileMauDieuChinhLuongNhanVien")]
        public IActionResult ExportFileMauDieuChinhLuongNhanVien(string IdQuyTrinh) => Ok(func.ExportFileMauDieuChinhLuongNhanVienNew(IdQuyTrinh, db));

        [HttpPost("GetMucLuongHienTailistNhanVien")]
        public IActionResult GetMucLuongHienTailistNhanVien([FromBody] TimKiemImport timKiem) =>
            Ok(func.GetMucLuongHienTailistNhanVien(timKiem.listIdNhanVien, timKiem.listData, db));

        [HttpPost("ExportTongHopDieuChinhLuong")]
        public IActionResult ExportTongHopDieuChinhLuong([FromBody] TimKiemBangLuong itemtimkiem) =>
            Ok(func.ExportTongHopDieuChinhLuong(db, itemtimkiem.IdCongty, itemtimkiem.listIdBoPhan, itemtimkiem.Keyword));

        [HttpGet("GetDieuChinhLuong")]
        public IActionResult GetDieuChinhLuong(string Id) => Ok(func.GetDieuChinhLuong(Id, db));

        //    [HttpGet]
        //    [Route("ExportDieuChinhLuong")]
        //    public IActionResult ExportDieuChinhLuong(string Id)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {

        //            db.IdUser = currentUser.Id;
        //            db.currentUser = currentUser;
        //            var item = func.ExportDieuChinhLuong(Id, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        [HttpGet("DeleteDieuChinhLuong")]
        public IActionResult DeleteQuyetDinhDieuChinhLuong(string Id) => Ok(func.DeleteQuyetDinhDieuChinhLuong(Id, db));

        [HttpGet("ExportQuyetDinhDieuChinhLuong")]
        public IActionResult ExportQuyetDinhDieuChinhLuong(string Id) => Ok(func.ExportQuyetDinhDieuChinhLuong(Id, db));

        [HttpGet("ExporTongHopDieuChinhLuongThang")]
        public IActionResult ExporTongHopDieuChinhLuongThang(long IdCongTy, int Nam, int Thang) =>
            Ok(func.ExporTongHopDieuChinhLuongThang(IdCongTy, Nam, Thang, db));

        [HttpGet("ExporQuyDinhDieuChinhLuongThang")]
        public IActionResult ExporQuyDinhDieuChinhLuongThang(long IdCongTy, int Nam, int Thang) =>
            Ok(func.ExporQuyDinhDieuChinhLuongThang(IdCongTy, Nam, Thang, db));
    }
}
