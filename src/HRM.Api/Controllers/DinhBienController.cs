#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.DinhBien;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("DinhBien")]
    public class DinhBienController(HrmDbContext db, FuncDinhBien func) : ApiControllerBase
    {
        #region định biên

        //[HttpPost]
        //[Route("GetListDinhBien")]
        //public IActionResult GetListDinhBien(dynamic dnm)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetListDinhBien(db, dnm);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("SetDinhBien")]
        //public IActionResult SetDinhBien([FromBody] HRM_DinhBien aModel)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.SetDinhBien(db, aModel);
        //    return Ok(item);
        //}

        //[HttpPost]
        //[Route("DeleteDinhBien")]
        //public IActionResult DeleteDinhBien(dynamic dnm)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var item = func.DeleteDinhBien(db, dnm);
        //    return Ok(item);
        //}

        [HttpPost("GetListQuyTrinhDinhBien")]
        public IActionResult GetListQuyTrinhDinhBien([FromBody] dynamic dyn)
            => Ok(func.GetListQuyTrinhDinhBien(db, dyn));

        [HttpGet("GetQuyTrinhDinhBien")]
        public IActionResult GetQuyTrinhDinhBien(string Id)
            => Ok(func.GetQuyTrinhDinhBien(Id, db));

        [HttpGet("GetNextSoQuyTrinhDinhBien")]
        public IActionResult GetNextSoQuyTrinhDinhBien()
            => Ok(func.GetNextSoQuyTrinhDinhBien(db));

        //[HttpGet]
        //[Route("KiemTraButtonQuyTrinhDinhBien")]
        //public IActionResult KiemTraButtonQuyTrinhDinhBien(string IDQuyTrinh, int TrangThai)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.KiemTraButtonQuyTrinhDinhBien(db, IDQuyTrinh, TrangThai);
        //    return Ok(result);
        //}

        [HttpPost("SetQuyTrinhDinhBien")]
        public IActionResult SetQuyTrinhDinhBien([FromBody] HRM_QuyTrinhDinhBien data)
            => Ok(func.SetQuyTrinhDinhBien(data, db));

        [HttpPost("ChuyenTiepQuyTrinhDinhBien")]
        public IActionResult ChuyenTiepQuyTrinhDinhBien([FromBody] HRM_QuyTrinhDinhBien data)
            => Ok(func.ChuyenTiepQuyTrinhDinhBien(db, data));

        [HttpPost("KhongDuyetQuyTrinhDinhBien")]
        public IActionResult KhongDuyetQuyTrinhDinhBien([FromBody] HRM_QuyTrinhDinhBien data)
            => Ok(func.KhongDuyetQuyTrinhDinhBien(db, data));

        [HttpGet("DeleteQuyTrinhDinhBien")]
        public IActionResult DeleteQuyTrinhDinhBien(string Id)
            => Ok(func.DeleteQuyTrinhDinhBien(db, Id));

        [HttpGet("GetDinhBienTheoNam")]
        public IActionResult GetDinhBienTheoNam(int Nam, long IdCongTyEOS, long IdCoSoEOS)
            => Ok(func.GetDinhBienTheoNam(db, Nam, IdCongTyEOS, IdCoSoEOS));

        [HttpGet("GetDinhBienBoPhanByChucDanh")]
        public IActionResult GetDinhBienBoPhanByChucDanh(int Nam, long IdBoPhan, long IdChucDanh)
            => Ok(func.GetDinhBienBoPhanByChucDanh(db, Nam, IdBoPhan, IdChucDanh));

        //[HttpGet]
        //[Route("GetQuyLuongHienTaiByBoPhan")]
        //public IActionResult GetQuyLuongHienTaiByBoPhan(int Nam, long IdBoPhan)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    {
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    db.currentUser = currentUser;
        //    var result = func.GetQuyLuongHienTaiByBoPhan(db, Nam, IdBoPhan);
        //    return Ok(result);
        //}

        #endregion
    }
}
