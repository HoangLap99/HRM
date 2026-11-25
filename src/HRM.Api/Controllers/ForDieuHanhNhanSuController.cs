#nullable disable
using HRM.Api.Filters;
using HRM.Api.Persistence;
using HRM.Api.Services.ForDieuHanhNhanSuICT;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using static HRM.Api.Services.FuncBase;

namespace HRM.Api.Controllers
{
    [Route("ForDieuHanhNhanSu")]
    public class ForDieuHanhNhanSuController(HrmDbContext db, FuncDieuHanhNhanSu func, FuncCanhBao _funcCanhBao) : ApiControllerBase
    {
        #region for Điều hành công việc

        [HttpGet("GetThongTinLuong_NgayNghi")]
        public IActionResult GetThongTinLuong_NgayNghi(string IdUser, long IdCongTy) =>
            Ok(func.GetThongTinLuong_NgayNghi(db, IdUser, IdCongTy));

        [HttpPost("SetChamCong")]
        public IActionResult SetChamCong([FromBody] List<HRM_CheckInOut> entitys) => Ok(func.SetChamCong(db, entitys));

        [HttpPost("SetChamCongCaNhan")]
        public IActionResult SetChamCongCaNhan([FromBody] HRM_CheckInOut entity) => Ok(func.SetChamCongCaNhan(db, entity));

        [HttpPost("GetlistThongTinNhanVien")]
        public IActionResult GetlistThongTinNhanVien([FromBody] dynamic entitys) => Ok(func.GetlistThongTinNhanVien(db, entitys));

        [HttpGet("GetlistThongTinNhanVienCoHuu")]
        public IActionResult GetlistThongTinNhanVienCoHuu() => Ok(func.GetlistThongTinNhanVienCoHuu(db));

        [HttpPost("GetlistBangChamCongOfCongTy")]
        public IActionResult GetlistBangChamCongofCongTy([FromBody] dynamic entitys) => Ok(func.GetlistBangChamCongofCongTy(db, entitys));

        public class GetThongTinNhanVien
        {
            public List<string> IdUser { get; set; }
            public long IdDuAn { get; set; }
        }

        [HttpPost("GetThongTinNhanVienByListIdUser")]
        public IActionResult GetThongTinNhanVienByListIdUser([FromBody] GetThongTinNhanVien entity) =>
            Ok(func.GetThongTinNhanVienByListIdUser(db, entity.IdUser, entity.IdDuAn));

        [HttpPost("SetChamCongHo")]
        public IActionResult SetChamCongHo([FromBody] GetThongTinNhanVien entity) => Ok(func.SetChamCongHo(db, entity.IdUser, entity.IdDuAn));

        [HttpPost("GetNgaySinhNhanVienByListIdUser")]
        public IActionResult GetNgaySinhNhanVienByListIdUser([FromBody] List<string> IdUser) =>
            Ok(func.GetNgaySinhNhanVienByListIdUser(db, IdUser));

        [HttpGet("GetThongTinNhanVienOutsideListIdUser")]
        public IActionResult GetThongTinNhanVienOutsideListIdUser(long IdDuAn) => Ok(func.GetThongTinNhanVienOutsideListIdUser(db, IdDuAn));

        [HttpGet("GetlistNhanSuNghiViecTrongThang")]
        public IActionResult GetlistNhanSuNghiViecTrongThang(long IdDuAn, int Nam, int Thang) =>
            Ok(func.GetlistNhanSuNghiViecTrongThang(db, IdDuAn, Nam, Thang));

        [HttpGet("GetTongLuongTrongThang")]
        public IActionResult GetTongLuongTrongThang(long IdDuAn, int Nam, int Thang) => Ok(func.GetTongLuongTrongThang(db, IdDuAn, Nam, Thang));

        [HttpGet("GetTyLeNhanSuQuayLaiThang")]
        public IActionResult GetTyLeNhanSuQuayLaiThang(long IdDuAn, int Nam, int Thang) =>
            Ok(func.GetTyLeNhanSuQuayLaiThang(db, IdDuAn, Nam, Thang));

        [HttpGet("GetDoTuoiLaoDongDuAn")]
        public IActionResult GetDoTuoiLaoDongDuAn(long IdDuAn = 0) => Ok(func.GetDoTuoiLaoDongDuAn(db, IdDuAn));

        [HttpGet("GetTinhHinhBienDongDuAn")]
        public IActionResult GetTinhHinhBienDongDuAn(long IdDuAn, int Nam) => Ok(func.GetTinhHinhBienDongDuAn(db, IdDuAn, Nam));

        [HttpGet("GetCoCauBoPhanDuAn")]
        public IActionResult GetCoCauBoPhanDuAn(long IdDuAn) => Ok(func.GetCoCauBoPhanDuAn(db, IdDuAn));

        [HttpGet("GetBangChamCongCaNhanThang")]
        public IActionResult GetBangChamCongCaNhanThang(long IdDuAn, double NgayUnix) =>
            Ok(func.GetBangChamCongCaNhanThang(db, IdDuAn, NgayUnix));

        [HttpGet("GetChamCongNgayByIdUser")]
        public IActionResult GetChamCongNgayByIdUser(long IdDuAn) => Ok(func.GetChamCongNgayByIdUser(db, IdDuAn));

        [HttpGet("GetBangTongHopCongCaNhan")]
        public IActionResult GetBangTongHopCongCaNhan(string IdNhanVien, double TuNgayUnix, double DenNgayUnix) =>
            Ok(func.GetBangTongHopCongCaNhanNew(db, IdNhanVien, TuNgayUnix, DenNgayUnix));

        [HttpGet("GetBangTongHopCongCaNhanThang")]
        public IActionResult GetBangTongHopCongCaNhanThang(long IdDuAn, double TuNgayUnix, double DenNgayUnix) =>
            Ok(func.GetBangTongHopCongCaNhanThang(db, IdDuAn, TuNgayUnix, DenNgayUnix));

        [HttpGet("GetLichLamViecFromDieuDong")]
        public IActionResult GetLichLamViecFromDieuDong()
        {
            //List<HRM_QuyTrinhLamThemGio_ChiTiet> listData = JsonConvert.DeserializeObject<List<HRM_QuyTrinhLamThemGio_ChiTiet>>(data.ToString());
            return Ok(func.GetLichLamViecFromDieuDong(db));
        }

        [HttpPost("GetLichLamViecFromDieuChinh")]
        public IActionResult GetLichLamViecFromDieuChinh([FromBody] dynamic dyn)
        {
            List<HRM_QuyTrinhLamThemGio_ChiTiet> listData = JsonConvert.DeserializeObject<List<HRM_QuyTrinhLamThemGio_ChiTiet>>(dyn.ToString());
            return Ok(func.GetLichLamViecFromDieuChinh(db, listData));
        }

        [HttpGet("GetLichLamViecFromDieuDongNoLogIn")]
        [AllowAnonymousSmartEos]
        public IActionResult GetLichLamViecFromDieuDongNoLogIn()
        {
            //var currentUser = getCurrentUser();

            //if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
            //{
            //    return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "ChÆ°a Ä‘Äƒng nháº­p"));
            //}
            //db.currentUser = currentUser;
            //List<HRM_QuyTrinhLamThemGio_ChiTiet> listData = JsonConvert.DeserializeObject<List<HRM_QuyTrinhLamThemGio_ChiTiet>>(data.ToString());
            _funcCanhBao.TaoCanhBaoNhapThongKeCongDoan();
            //var item = func.GetLichLamViecFromDieuDong(db);
            return Ok("OK");
        }

        [HttpGet("TaoThongBaoKhaoSat")]
        [AllowAnonymousSmartEos]
        public IActionResult TaoThongBaoKhaoSat()
        {
            //var currentUser = getCurrentUser();

            //if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
            //{
            //    return Ok(new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "ChÆ°a Ä‘Äƒng nháº­p"));
            //}
            //db.currentUser = currentUser;
            //List<HRM_QuyTrinhLamThemGio_ChiTiet> listData = JsonConvert.DeserializeObject<List<HRM_QuyTrinhLamThemGio_ChiTiet>>(data.ToString());
            _funcCanhBao.TaoThongBaoKhaoSat();
            //var item = func.GetLichLamViecFromDieuDong(db);
            return Ok("OK");
        }

        #endregion
    }
}
