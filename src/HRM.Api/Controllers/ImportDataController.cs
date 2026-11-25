#nullable disable
using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using HRM.Api.Services.ChamCong;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;

namespace HRM.Api.Controllers
{

    [Route("ImportData")]
    public class ImportDataController : ApiControllerBase
    {
        private readonly HrmDbContext db;
        private readonly funcChamCongVanPhong func;

        public ImportDataController(HrmDbContext db, funcChamCongVanPhong func)
        {
            this.db = db;
            this.func = func;
        }
        //[HttpGet]
        //[Route("ImportBangChamCongVanPhong")]
        //public IActionResult ImportBangChamCongVanPhong(int Thang, int Nam)
        //{
        //    var currentUser = getCurrentUser();
        //    if (currentUser != null)
        //    {
        //        string IdCurrentUser = currentUser.Id;
        //        db.IdUser = IdCurrentUser;
        //        db.currentUser = currentUser;
        //        var item = func.ImportBangChamCongVanPhong(Thang, Nam, IdCurrentUser,  smdb, db);
        //        return Ok(item);
        //    }
        //    else
        //        return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Chưa đăng nhập"));
        //}
    }
}
