#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.BangLuong;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("BangLuong")]
    public class BangLuongController(HrmDbContext db, FuncBangLuong func) : ApiControllerBase
    {
        //    [HttpGet]
        //    [Route("GetNextMaBangLuong")]
        //    public IActionResult GetMaBangLuong()
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.GetNextMaBangLuong( db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        [HttpPost("GetDanhSachBangLuong")]
        public IActionResult GetDanhSachBangLuong([FromBody] TimKiem TimKiem)
            => Ok(func.GetDanhSachBangLuong(TimKiem, db));

        //    [HttpPost]
        //    [Route("GetDanhSachBangChamCongDaDuyet")]
        //    public IActionResult GetDanhSachBangChamCongDaDuyet(TimKiem TimKiem)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.GetDanhSachBangChamCongDaDuyet(TimKiem, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }


        //[HttpPost]
        //[Route("GetTongHopBangChamCongCongTy")]
        //public IActionResult GetTongHopBangChamCongCongTy(TimKiem dyn)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        var item = func.GetTongHopBangChamCongCongTy(dyn, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}
        //[HttpPost]
        //[Route("ExportTongHopBangChamCongCongTy")]
        //public IActionResult ExportTongHopBangChamCongCongTy(TimKiem dyn)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        var item = func.ExportTongHopBangChamCongCongTy(dyn, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}

        //   [HttpPost]
        //    [Route("GetBangLuongTheoBangChamCong")]
        //    public IActionResult GetBangLuongTheoBangChamCong(List<string> ListIdBangChamCong)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.GetBangLuongTheoBangChamCong(ListIdBangChamCong, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    [HttpPost]
        //    [Route("GetBangLuongTheoBangChamCongDaiAn")]
        //    public IActionResult GetBangLuongTheoBangChamCongDaiAn(dynamic dyn)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.GetBangLuongTheoBangChamCongDaiAn(dyn, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        //    [HttpPost]
        //    [Route("SetQuyTrinhBangLuong")]
        //    public IActionResult SetQuyTrinhBangLuong(HRM_BangLuong model)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.SetQuyTrinhBangLuong(model, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        //    [HttpGet]
        //    [Route("GetQuyTrinhBangLuong")]
        //    public IActionResult GetQuyTrinhBangLuong(string Id)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.GetQuyTrinhBangLuong(Id, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        //    [HttpGet]
        //    [Route("DeleteQuyTrinhBangLuong")]
        //    public IActionResult DeleteQuyTrinhBangLuong(string Id)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.DeleteQuyTrinhBangLuong(Id, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }

        //    [HttpPost]
        //    [Route("ChuyenTiepQuyTrinhBangLuong")]
        //    public IActionResult ChuyenTiepQuyTrinhBangLuong(HRM_BangLuong model)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.ChuyenTiepQuyTrinhBangLuong(model, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }


        //    [HttpPost]
        //    [Route("KhongDuyetQuyTrinhBangLuong")]
        //    public IActionResult KhongDuyetQuyTrinhBangLuong(HRM_BangLuong model)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.KhongDuyetQuyTrinhBangLuong(model, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    [HttpGet]
        //    [Route("ExportBangLuongChiTiet")]
        //    public IActionResult ExportBangLuongChiTiet(string Id)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.ExportBangLuongChiTiet(Id, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    [HttpGet]
        //    [Route("ExportBangLuong")]
        //    public IActionResult ExportBangLuong(string Id)
        //    {
        //        var currentUser = getCurrentUser();
        //        if (currentUser != null)
        //        {
        //            string IdCurrentUser = currentUser.Id;
        //            db.IdUser = IdCurrentUser;
        //            db.currentUser = currentUser;
        //            var item = func.ExportBangLuong(Id, db);
        //            return Ok(item);
        //        }
        //        else
        //            return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //    }
        //    //[HttpGet]
        //    //[Route("GetDanhSachTableNam")]
        //    //public IActionResult GetDanhSachTableNam()
        //    //{

        //    //        var item = func.GetDanhSachTableNam(db);
        //    //    return Ok(item);
        //    //}
    }
}
