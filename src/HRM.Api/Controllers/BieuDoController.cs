#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.BaoCao;
using HRM.Api.Services.BieuDo;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers
{
    [Route("BieuDo")]
    public class BieuDoController(HrmDbContext db, FuncBieuDoNhanVien func, FuncBaoCaoNhanVien funcBaoCao) : ApiControllerBase
    {
        [HttpGet("GetBieuDoBienDongNhanSuTuNamDenNam")]
        public IActionResult GetBieuDoBienDongNhanSuTuNamDenNam(int TuNam, int DenNam, long IdCongTyEOS)
            => Ok(func.GetBieuDoBienDongNhanSuTuNamDenNam(TuNam, DenNam, IdCongTyEOS, db));

        [HttpGet("ExportTongHopNhanSuDaiAn")]
        public IActionResult ExportTongHopNhanSuDaiAn(long IdCongTyEOS)
            => Ok(funcBaoCao.ExportTongHopNhanSuDaiAn(IdCongTyEOS, db));

        [HttpGet("GetBieuDoNhanSuTheoTrinhDoChuyenMon")]
        public IActionResult GetBieuDoNhanSuTheoTrinhDoChuyenMon(long IdBoPhan)
            => Ok(func.GetBieuDoNhanSuTheoTrinhDoChuyenMon(IdBoPhan, db));

        [HttpGet("GetBieuDoNhanSuTheoThanhPhan")]
        public IActionResult GetBieuDoNhanSuTheoThanhPhan(long IdCongTyEOS)
            => Ok(func.GetBieuDoNhanSuTheoThanhPhan(IdCongTyEOS, db));

        [HttpGet("GetBieuDoNhanSuTheoTinhTrangHonNhan")]
        public IActionResult GetBieuDoNhanSuTheoTinhTrangHonNhan(long IdCongTyEOS)
            => Ok(func.GetBieuDoNhanSuTheoTinhTrangHonNhan(IdCongTyEOS, db));

        [HttpGet("GetBieuDoNhanSuTheoDiaPhuong")]
        public IActionResult GetBieuDoNhanSuTheoDiaPhuong(long IdCongTyEOS)
            => Ok(func.GetBieuDoNhanSuTheoDiaPhuong(IdCongTyEOS, db));

        [HttpGet("GetBieuDoNhanSuTheoLoaiHopDong")]
        public IActionResult GetBieuDoNhanSuTheoLoaiHopDong(long IdCongTyEOS)
            => Ok(func.GetBieuDoNhanSuTheoLoaiHopDong(IdCongTyEOS, db));

        [HttpGet("GetBieuDoNhanSuTheoDoTuoi")]
        public IActionResult GetBieuDoNhanSuTheoDoTuoi(long IdCongTyEOS)
            => Ok(func.GetBieuDoNhanSuTheoDoTuoi(IdCongTyEOS, db));
    }
}
