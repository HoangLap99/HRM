#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.TuyenDung;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HRM.Api.Controllers
{
    [Route("KeHoachTuyenDung")]
    public class KeHoachTuyenDungController(HrmDbContext db, FuncKeHoachTuyenDung func) : ApiControllerBase
    {
        [HttpGet("GetMaKeHoachTuyenDung")]
        public IActionResult GetMaKeHoachTuyenDung()
            => Ok(func.GetNextMaKeHoachTuyenDung(db));

        [HttpPost("GetDanhSachKeHoachTuyenDung")]
        public IActionResult GetDanhSachKeHoachTuyenDung([FromBody] dynamic dyn)
            => Ok(func.GetDanhSachKeHoachTuyenDung(dyn, db));

        [HttpPost("SetKeHoachTuyenDung")]
        public IActionResult SetKeHoachTuyenDung([FromBody] HRM_TuyenDung_KeHoach aModel)
            => Ok(func.SetKeHoachTuyenDung(aModel, db));

        [HttpGet("GetKeHoachTuyenDung")]
        public IActionResult GetKeHoachTuyenDung(string aId)
            => Ok(func.GetKeHoachTuyenDung(aId, db));

        [HttpPost("DeleteKeHoachTuyenDung")]
        public IActionResult DeleteKeHoachTuyenDung([FromBody] List<string> aListId)
            => Ok(func.DeleteKeHoachTuyenDung(aListId, db));

        [HttpPost("ChuyenTiepKeHoachTuyenDung")]
        public IActionResult ChuyenTiepKeHoachTuyenDung([FromBody] HRM_TuyenDung_KeHoach dyn)
            => Ok(func.ChuyenTiepKeHoachTuyenDung(dyn, db));

        [HttpPost("KhongDuyetKeHoachTuyenDung")]
        public IActionResult KhongDuyetKeHoachTuyenDung([FromBody] HRM_TuyenDung_KeHoach aModel)
            => Ok(func.KhongDuyetKeHoachTuyenDung(aModel, db));

        #region Tổng hợp

        [HttpGet("GetNextMaTongHopKeHoachTuyenDung")]
        public IActionResult GetNextMaTongHopKeHoachTuyenDung()
            => Ok(func.GetNextMaTongHopKeHoachTuyenDung(db));

        [HttpPost("GetDanhSachKeHoachDaDuyet")]
        public IActionResult GetDanhSachKeHoachDaDuyet([FromBody] TimKiem dyn)
            => Ok(func.GetDanhSachKeHoachDaDuyet(dyn, db));

        //[HttpGet]
        //[Route("GetDanhSachKeHoachDaDuyet")]
        //public IActionResult GetDanhSachKeHoachDaDuyet()
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetDanhSachKeHoachDaDuyet(db);
        //    return Ok(result);
        //}

        [HttpPost("GetTongHopKeHoacTuyenDung")]
        public IActionResult GetTongHopKeHoacTuyenDung([FromBody] dynamic dyn)
            => Ok(func.GetTongHopKeHoacTuyenDung(dyn, db));

        [HttpPost("SetTongHopKeHoachTuyenDung")]
        public IActionResult SetTongHopKeHoachTuyenDung([FromBody] HRM_TuyenDung_KeHoach_TongHop model)
            => Ok(func.SetTongHopKeHoachTuyenDung(model, db));

        [HttpPost("ChuyenTiepTongHopKeHoachTuyenDung")]
        public IActionResult ChuyenTiepTongHopKeHoachTuyenDung([FromBody] HRM_TuyenDung_KeHoach_TongHop model)
            => Ok(func.ChuyenTiepTongHopKeHoachTuyenDung(model, db));

        [HttpPost("KhongDuyetTongHopKeHoachTuyenDung")]
        public IActionResult KhongDuyetTongHopKeHoachTuyenDung([FromBody] HRM_TuyenDung_KeHoach_TongHop model)
            => Ok(func.KhongDuyetTongHopKeHoachTuyenDung(model, db));

        [HttpPost("GetListKeHoachTongHop")]
        public IActionResult GetListKeHoachTongHop([FromBody] dynamic timKiem)
            => Ok(func.GetListKeHoachTongHop(timKiem, db));

        [HttpGet("GetTongHopKeHoachById")]
        public IActionResult GetTongHopKeHoachById(string Id)
            => Ok(func.GetTongHopKeHoachById(Id, db));

        [HttpGet("DeleteTongHopKeHoach")]
        public IActionResult DeleteTongHopKeHoach(string Id)
            => Ok(func.DeleteTongHopKeHoach(Id, db));

        #endregion
    }
}
