using HRM.Api.Persistence;
using HRM.Api.Services.SmartEOS;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

[Route("ThongTinSmartEOS")]
public sealed class ThongTinSmartEOSController(
    HrmDbContext db,
    CallWebService callWebService,
    FuncThongTinTuSmartEOS funcThongTinSmart) : ApiControllerBase
{
    [HttpGet("GetDanhSachCongTy")]
    public IActionResult GetDanhSachCongTy()
        => Ok(callWebService.GetDanhSachCongTy(db));

    [HttpGet("GetCongTyKyHopDong")]
    public IActionResult GetCongTyKyHopDong()
        => Ok(callWebService.GetCongTyKyHopDong(db));

    [HttpGet("GetThongTinNguoiDaiDien")]
    public IActionResult GetThongTinNguoiDaiDien(long IdNguoiDaiDien)
        => Ok(funcThongTinSmart.GetThongTinNguoiDaiDien(IdNguoiDaiDien, db));

    [HttpGet("GetDanhSachBoPhanTheoCongTy")]
    public IActionResult GetDanhSachBoPhanTheoCongTy(long IdBoPhanEOS)
        => Ok(callWebService.GetDanhSachBoPhanTheoCongTy(db, IdBoPhanEOS));

    [HttpGet("GetDanhSachNhanVienByIdBoPhan")]
    public IActionResult GetDanhSachNhanVienByIdBoPhan(long IdBoPhan)
        => Ok(funcThongTinSmart.GetDanhSachNhanVienByIdBoPhan(IdBoPhan, db));

    [HttpPost("GetDanhSachNhanVienByListIdBoPhan")]
    public IActionResult GetDanhSachNhanVienByListIdBoPhan([FromBody] TimKiemBangLuong timkiem)
        => Ok(funcThongTinSmart.GetDanhSachNhanVienByListIdBoPhan(timkiem.IdCongty, timkiem.listIdBoPhan, db));

    [HttpPost("GetDanhSachNhanVienByListIdBoPhanForNghiPhep")]
    public IActionResult GetDanhSachNhanVienByListIdBoPhanForNghiPhep([FromBody] TimKiemBangLuong timkiem)
        => Ok(funcThongTinSmart.GetDanhSachNhanVienByListIdBoPhanForNghiPhep(timkiem.IdCongty, timkiem.listIdBoPhan, db));

    [HttpGet("GetDanhSachChiBoPhanTheoCongTy")]
    public IActionResult GetDanhSachChiBoPhanTheoCongTy(long IdBoPhanEOS)
        => Ok(callWebService.GetDanhSachBoPhan(db));

    [HttpGet("GetDanhSachBoPhanAll")]
    public IActionResult GetDanhSachBoPhanAll()
        => Ok(callWebService.GetDanhSachBoPhan(db));
}
