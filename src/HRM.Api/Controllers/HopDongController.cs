#nullable disable
using HRM.Api.Persistence;
using HRM.Api.Services.HopDong;
using HRM.Api.Entity;
using HRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static HRM.Api.Services.FuncBase;

namespace HRM.Api.Controllers
{
    [Route("HopDong")]
    public class HopDongController(HrmDbContext db, FuncHopDong func, CallWebService _CallWebService) : ApiControllerBase
    {
        [HttpGet("GetListdmTrangThaiHopDong")]
        public IActionResult GetListdmTrangThaiHopDong() => Ok(func.GetListdmTrangThaiHopDong());

        [HttpGet("GetMaHopDong")]
        public IActionResult GetMaHopDong(string IdLoaiHopDong, string IdNhanVien, string IdHopDong, double NgayUnix = 0, long IdChucVu = 0, long IdBoPhan = 0, long IdCongTy = 0) =>
            Ok(func.GetMaHopDong(db, IdLoaiHopDong, IdNhanVien, IdHopDong, IdChucVu, IdBoPhan, IdCongTy, NgayUnix));

        [HttpGet("GetDanhSachCongViecTheoBoPhanChucVu")]
        public IActionResult GetDanhSachCongViecTheoBoPhanChucVu(long IdChucVuEOS, long IdBoPhanEOS) =>
            Ok(_CallWebService.GetDanhSachCongViecTheoBoPhanChucVu(db, IdChucVuEOS, IdBoPhanEOS));

        [HttpPost("GetDanhSachQuyTrinhHopDong")]
        public IActionResult GetDanhSachQuyTrinhHopDong([FromBody] TimKiem timKiem) => Ok(func.GetDanhSachQuyTrinhHopDong(timKiem, db));

        [HttpPost("GetDanhSachHopDong")]
        public IActionResult GetDanhSachHopDong([FromBody] EntityGetDanhSachHopDongRequest aRequest) => Ok(func.GetDanhSachHopDong(aRequest, db));

        [HttpPost("ExportHopDongHieuLuc")]
        public IActionResult ExportHopDongHieuLuc([FromBody] List<string> listIdHopDong) => Ok(func.ExportHopDongHieuLucNew(listIdHopDong, db));

        [HttpPost("SetHopDong")]
        public IActionResult SetHopDong([FromBody] HRM_HopDong dyn) => Ok(func.SetHopDong(dyn, db));

        [HttpPost("ChuyenTiepQuyTrinhHopDong")]
        public IActionResult ChuyenTiepQuyTrinhHopDong([FromBody] HRM_HopDong data) => Ok(func.ChuyenTiepQuyTrinhHopDong(data, db));

        [HttpGet("ChuyenTiepHopDong")]
        public IActionResult ChuyenTiepHopDong() => Ok(func.ChuyenTiepHopDong(db));

        [HttpGet("ChuyenTiepQuyTrinhHopDongUpdateLuong")]
        public IActionResult ChuyenTiepQuyTrinhHopDongUpdateLuong() => Ok(func.ChuyenTiepQuyTrinhHopDongUpdateLuong(db));

        [HttpGet("GetSoNgayPhepThamNien")]
        public IActionResult GetSoNgayPhepThamNien(string IdNhanVien) => Ok(func.GetSoNgayPhepThamNien(db, IdNhanVien));

        [HttpPost("KhongDuyetQuyTrinhHopDong")]
        public IActionResult KhongDuyetQuyTrinhHopDong([FromBody] HRM_HopDong data) => Ok(func.KhongDuyetQuyTrinhHopDong(data, db));

        [HttpPost("GetDanhSachHopDongHieuLucNhanVien")]
        public IActionResult GetDanhSachHopDongHieuLucNhanVien([FromBody] List<string> lstIdNhanVien) =>
            Ok(func.GetDanhSachHopDongHieuLucNhanVien(lstIdNhanVien, CurrentUserId, db));

        [HttpGet("GetHopDongHieuLucNhanVien")]
        public IActionResult GetHopDongHieuLucNhanVien(string IdNhanVien) => Ok(func.GetHopDongHieuLucNhanVien(IdNhanVien, CurrentUserId, db));

        [HttpGet("ChuyenTrangThaiHopDong")]
        public IActionResult ChuyenTrangThaiHopDong(string Id) => Ok(func.ChuyenTrangThaiHopDong(Id, db));

        [HttpGet("GetHopDong")]
        public IActionResult GetHopDong(string Id) => Ok(func.GetHopDong(Id, db));

        [HttpGet("GetHopDongSaoChep")]
        public IActionResult GetHopDongSaoChep(string Id, bool isPhuLuc) => Ok(func.GetHopDongSaoChep(Id, isPhuLuc, db));

        [HttpPost("DeleteHopDong")]
        public IActionResult DeleteHopDong([FromBody] List<string> dyn) => Ok(func.DeleteHopDong(dyn, db));

        [HttpGet("ExportHopDongLaoDong")]
        public IActionResult ExportHopDongLaoDong(string Id)
        {
            return Ok(func.ExportHopDongHieuLucNew([Id], db));
            {  //var HopDong = db.HRM_HopDong.FirstOrDefault(a => a.IdGuid == Id);
               //var LoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
               //if (!string.IsNullOrEmpty(HopDong.IdHopDong) || HopDong.isPhuLuc == true)
               //{
               //    var item = func.ExportPhuLucHopDongICT(Id, db);
               //    return Ok(item);
               //}
               //else
               //{
               //    string Host = ConfigurationManager.AppSettings["Host"] ?? "0";
               //    if (Host == "PHT")
               //    {
               //        if (LoaiHopDong.Ma == "HĐTV")
               //        {
               //            var item = func.ExportHopDongThuViec(Id, db);
               //            return Ok(item);
               //        }
               //        else if (LoaiHopDong.Ma == "TTHV" || LoaiHopDong.Ma == "HĐHV")
               //        {
               //            var item = func.ExportHopDongHocViec(Id, db);
               //            return Ok(item);
               //        }
               //        else if (LoaiHopDong.Ma == "BBTL")
               //        {
               //            var item = func.ExportHopDongThanhLy(Id, db);
               //            return Ok(item);
               //        }
               //        //else if (LoaiHopDong.Ma == "THHĐ" )
               //        //{
               //        //    var item = func.ExportHopDongXacDinhThoiHan(Id, db);
               //        //    return Ok(item);
               //        //}
               //        else if (LoaiHopDong.Ma == "TTHT")
               //        {
               //            var item = func.ExportHopDongHopTac(Id, db);
               //            return Ok(item);
               //        }
               //        else if (LoaiHopDong.Ma == "HĐKHOAN")
               //        {
               //            var item = func.ExportHopDongKhoan(Id, db);
               //            return Ok(item);
               //        }
               //        else if (LoaiHopDong.Ma == "HĐVV")
               //        {
               //            var item = func.ExportHopDongVuViec(Id, db);
               //            return Ok(item);
               //        }

                //        else if (LoaiHopDong.Ma == "HĐLĐ" || LoaiHopDong.Ma == "HĐLĐ." || LoaiHopDong.Ma == "KTHHĐ" || LoaiHopDong.Ma == "HĐLĐ.KXDTH")
                //        {
                //            var item = func.ExportHopDongLaoDongICT(Id, db);
                //            return Ok(item);
                //        }

                //        else if (HopDong.LoaiLuong != 1)
                //        {
                //            var item = func.ExportHopDongCongNhanThoiVu(Id, db);
                //            return Ok(item);
                //        }
                //    }
                //    else
                //    {
                //        if (LoaiHopDong.Ma == "HĐTV")
                //        {
                //            var item = func.ExportHopDongThuViec(Id, db);
                //            return Ok(item);
                //        }
                //        else
                // if (LoaiHopDong.Ma == "TTHV" || LoaiHopDong.Ma == "HĐHV")
                //        {
                //            var item = func.ExportHopDongHocViec(Id, db);
                //            return Ok(item);
                //        }
                //        else
                // if (LoaiHopDong.Ma == "BBTL")
                //        {
                //            var item = func.ExportHopDongThanhLy(Id, db);
                //            return Ok(item);
                //        }

                //        else if (HopDong.LoaiLuong != 1)
                //        {
                //            var item = func.ExportHopDongCongNhanThoiVu(Id, db);
                //            return Ok(item);
                //        }

                //        else if (LoaiHopDong.Ma == "HĐLĐ" || LoaiHopDong.Ma == "THHĐ")
                //        {
                //            var item = func.ExportHopDongXacDinhThoiHan(Id, db);
                //            return Ok(item);
                //        }
                //        else if (LoaiHopDong.Ma == "TTHT")
                //        {
                //            var item = func.ExportHopDongHopTac(Id, db);
                //            return Ok(item);
                //        }
                //        else if (LoaiHopDong.Ma == "HĐKHOAN")
                //        {
                //            var item = func.ExportHopDongKhoan(Id, db);
                //            return Ok(item);
                //        }
                //        else if (LoaiHopDong.Ma == "HĐVV")
                //        {
                //            var item = func.ExportHopDongVuViec(Id, db);
                //            return Ok(item);
                //        }

                //        else if ( LoaiHopDong.Ma == "HĐLĐ." || LoaiHopDong.Ma == "KTHHĐ")
                //        {
                //            var item = func.ExportHopDongLaoDongICT(Id, db);
                //            return Ok(item);
                //        }
                //    }

                //}
            }

            // return Ok("");
        }

        [HttpGet("ExportTiepNhanNhanVienDaiAn")]
        public IActionResult ExportTiepNhanNhanVienDaiAn(string Id) => Ok(func.ExportTiepNhanNhanVienDaiAn(Id, db));

        [HttpGet("ExportThongBaoKhongKyHopDong")]
        public IActionResult ExportThongBaoKhongKyHopDong(string Id) => Ok(func.ExportThongBaoKhongKyHopDong(Id, db));

        [HttpGet("ExportPhuLucHopDongLaoDongDaiAn")]
        public IActionResult ExportPhuLucHopDongLaoDongDaiAn(string Id) => Ok(func.ExportPhuLucHopDongLaoDongDaiAn(Id, db));

        #region Import hợp đồng nhân viên

        [HttpGet("ImportExcelHopDong")]
        public IActionResult ImportExcelHopDong(string aFileName, bool isKetThuc = false) => Ok(func.ImportExcelHopDong(aFileName, db, isKetThuc));

        [HttpGet("ImportExcelHopDongFORLUONG")]
        public IActionResult ImportExcelHopDongFORLUONG(string aFileName) => Ok(func.ImportExcelHopDongFORLUONG(aFileName, db));

        [HttpGet("ImportExcelHopDongUpdate")]
        public IActionResult ImportExcelHopDongUpdate(string aFileName) => Ok(func.ImportExcelHopDongUpdate(aFileName, db));

        [HttpGet("ImportExcelHopDongNhanSu")]
        public IActionResult ImportExcelHopDongNhanSu(string FileName) => Ok(func.ImportExcelHopDongNhanSu(FileName, db));

        [HttpGet("ImportExcelHopDongNhanSuUpdate")]
        public IActionResult ImportExcelHopDongNhanSuUpdate(string FileName) => Ok(func.ImportExcelHopDongNhanSuUpdate(FileName, db));

        [HttpGet("ExportMauPhuCap")]
        public IActionResult ExportMauPhuCap() => Ok(func.ExportMauPhuCap(db));

        [HttpPost("ExportMauExcelHopDong")]
        public IActionResult ExportMauExcelHopDong([FromBody] TimKiem timkiem) => Ok(func.ExportExcelHopDongMau(db, timkiem));

        [HttpGet("ExportHopDongHetHan")]
        public IActionResult ExportHopDongHetHan() => Ok(func.ExportHopDongHetHan(db));

        [HttpGet("ImportPhuCapNhanVien")]
        public IActionResult ImportPhuCapNhanVien(string aFileName) => Ok(func.ImportPhuCapNhanVien(aFileName, db));

        [HttpGet("GetdanhTaiSanBanGiaoNhanVien")]
        public IActionResult GetdanhTaiSanBanGiaoNhanVien(string IdNhanVien) => Ok(func.GetdanhTaiSanBanGiaoNhanVien(IdNhanVien, db));

        #endregion

        [HttpGet("UpdateTrangThaiHetHieuLuc")]
        public IActionResult UpdateTrangThaiHetHieuLuc() => Ok(func.UpdateTrangThaiHetHieuLuc(db));

        [HttpGet("UpdateChucVuChucDanhHopDong")]
        public IActionResult UpdateChucVuChucDanhHopDong() => Ok(func.UpdateChucVuChucDanhHopDong(db));

        [HttpPost("ChuyenTrangThaiHopDong")]
        public IActionResult ChuyenTrangThaiHopDong([FromBody] List<string> listIdHopDong) =>
            Ok(func.ChuyenTrangThaiHopDong(db, listIdHopDong));

        [HttpGet("UpdateLuong_PhuCapHopDong")]
        public IActionResult UpdateLuong_PhuCapHopDong()
        {
            func.UpdateLuong_PhuCapHopDong(db);
            return Ok("");
        }

        [HttpPost("GetTongHopTheoDoiHopDong")]
        public IActionResult GetTongHopTheoDoiHopDong([FromBody] TimKiemBangLuong itemTimKiem) =>
            Ok(func.GetTongHopTheoDoiHopDong(db, itemTimKiem));

        [HttpPost("ExportTongHopTheoDoiHopDong")]
        public IActionResult ExportTongHopTheoDoiHopDong([FromBody] TimKiemBangLuong itemTimKiem) =>
            Ok(func.ExportTongHopTheoDoiHopDong(db, itemTimKiem));
    }
}
