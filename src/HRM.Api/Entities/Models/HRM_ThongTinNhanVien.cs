﻿#nullable disable
using HRM.Api.Services;
using HRM.Api.Services.NhanVien;
using HRM.Api.Entity.DinhBien;
using HRM.Api.Helper;
using HRM.Api.Models.ChamCongModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static HRM.Api.Helper.HelperConstant;

namespace HRM.Api.Models
{

    public class DanhSachNhanVien : Audit
    {
        //public string IdGuid { set; get; }
        public string GioiTinh { get; set; }
        public string IdChucVuDuAn { get; set; }
        public string IdDuAn { get; set; }
        public string MaNhanVien { get; set; }
        public string TenChucVuDuAn { get; set; }
        public string TenDayDu { get; set; }
        public string TenDuAn { get; set; }
        public string TrangThai { get; set; }
        public string IdViTriChuyenMon { get; set; }
        public DateTime NgayBatDauLamViec { get; set; }
        public DateTime NgayKetThucLamViec { get; set; }
        public string ViTriChuyenMon { get; set; }

        public string DienThoai { get; set; }

        public string SoChungMinhThu { get; set; }

        public string HoKhauThuongTruDiaChi { get; set; }
        public string HoKhauThuongTruIdTinhThanhPho { get; set; }
        public string HoKhauThuongTruIdQuanHuyen { get; set; }
        public string HoKhauThuongTruIdPhuongXa { get; set; }
        public string NguyenQuan { get; set; }

        public DateTime NgaySinh { get; set; }
        public double NgaySinhUnix { get { return dungchung.DateTimeToUnixTimeStamp(NgaySinh); } }

        //public List<HRM_Link_NhanVien_BoPhan_ChucVu> lstChucVuBoPhan = new List<HRM_Link_NhanVien_BoPhan_ChucVu>();
        public List<string> lstTenPhongBan { get; set; }

        public List<CongTyNhanVienViewModel> lstCongTy = new List<CongTyNhanVienViewModel>();
    }
    public class DanhSachNhanVienThongBao
    {
        public List<DanhSachNhanVienViewModel> listHetHanThuViec { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listHetHanHopDong { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listKyNiemNgayVao { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listSinhNhat { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listGiayToHetHan { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listPhuThuoc { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listDoTuoi { get; set; } = new List<DanhSachNhanVienViewModel>();
        public List<DanhSachNhanVienViewModel> listLuongHuu { get; set; } = new List<DanhSachNhanVienViewModel>();
    }
    public class DanhSachNhanVienViewModel
    {
        public string IdGuid { set; get; }
        public string GioiTinh { get; set; }

        public string MaNhanVien { get; set; }

        public string TenDayDu { get; set; }
        public string TrangThai { get; set; }
        public string DiaChi { get; set; }
        public DateTime? NgayBatDauLamViec { get; set; }
        public double NgayBatDauLamViecUnix { get; set; }
        public DateTime? NgayKetThucLamViec { get; set; }
        public double NgayKetThucLamViecUnix { get; set; }
        public string Email { get; set; }
        public string DienThoai { get; set; }

        public string SoChungMinhThu { get; set; }
        public string MaSoThue { get; set; }
        public string SoBHXH { get; set; }
        public string CanhBaoQuaTuoi { get; set; }

        public string HoKhauThuongTruDiaChi { get; set; }
        //public string HoKhauThuongTruIdTinhThanhPho { get; set; }
        //public string HoKhauThuongTruIdQuanHuyen { get; set; }
        //public string HoKhauThuongTruIdPhuongXa { get; set; }
        public string NguyenQuan { get; set; }
        public long IdBoPhanEOS { get; set; }
        public long IdNhanVienEOS { set; get; }
        public string TenBoPhan { get; set; }
        public long IdCongTyEOS { set; get; }

        public string TenCongTy { get; set; }
        public long IdChucVuEOS { get; set; }
        public long IdChucDanhEOS { get; set; }

        public string TenChucVu { get; set; }
        public string TenChucDanh { get; set; }
        // public long IdChucDanhEOS { get; set; }
        //public bool KiemNhiem { get; set; }
        //public bool isTruongBoPhan { get; set; }
        public DateTime? NgayBatDau { get; set; } = DateTimeHelper.DATE_NOW;
        public double NgayBatDauUnix { get; set; }
        public double Luong { get; set; }
        public double SoTien { get; set; }
        public double LuongBaoHiem { get; set; }
        public DateTime? NgayKetThuc { get; set; } = DateTimeHelper.MAX_DATE;
        public double NgayKetThucUnix { get; set; }
        public string CheDo { get; set; }
        public DateTime NgaySinh { get; set; }
        public DateTime NgayDongBHXH { get; set; }
        public string SoNamDongBHXH { get; set; }
        public DateTime NgayKy { get; set; }
        public double NgaySinhUnix { get; set; }
        public bool isLuongHuu { get; set; }
        public double TongThoiGianThamGia { get; set; }
        public string TongThoiGianTheoNam { get; set; }
        public string CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; }
        public string HopDong { get; set; }
        public double DenGio { get; set; }
        public double TuGio { get; set; }
        public double SoGioMotNgay { get; set; }
        public double TongSoCong { get; set; }
        public List<long> listIdCongTy { get; set; }
        public double TongSoNgayNghi { get; set; }
        public string XepHangDanhGia { get; set; }
        public string SoGen { get; set; }
        public string Loai { get; set; }
        public string IdUser { get; set; }
        public string NoiDung { get; set; }
        public DanhSachNhanVienViewModel()
        {

        }
        public DanhSachNhanVienViewModel Clone()
        {
            return (DanhSachNhanVienViewModel)this.MemberwiseClone();
        }

    }
    public class CongTyNhanVienViewModel
    {
        public long IdCongTyEOS { set; get; }

        public string TenCongTyEOS { set; get; }
    }
    public class NhanVienRutGon : Audit
    {
        public string TenDayDu { get; set; }

        public string Ma { get; set; }

        public NhanVienRutGon() { }

        public NhanVienRutGon(string Id, string TenNhanVien)
        {
            this.IdGuid = Id;
            this.TenDayDu = TenNhanVien;
        }
    }
    public class HRM_NhanVien_PhuCap : Audit
    {
        public string IdNhanVien { get; set; }

        public string IddmPhuCap { get; set; }
        public double SoTien { get; set; }
        [NotMapped]
        public string TenPhuCap { get; set; }
    }
    public class HRM_NhanVienDieuChinhThongTin : Audit
    {
        public string IdNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime NgayYeuCau { get; set; }
        public string TrangThai { get; set; }
        [NotMapped]
        public string TenTrangThai { get; set; }
        [NotMapped]
        public HRM_NhanVien_Edit NhanVien { get; set; }
        [NotMapped]
        public List<HRM_NhanVienDieuChinhThongTin_ThongTin> listThayDoi { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem { get; set; }

        [NotMapped]
        public List<string> ListNoiDung { get; set; }
    }
    public class HRM_NhanVienDieuChinhThongTin_ThongTin : Audit
    {
        public string IdPhieuThayDoi { get; set; }
        public string NoiDung { get; set; }
        public string ThongTinCu { get; set; }
        public string ThongTinMoi { get; set; }
        public string Link_Tab { get; set; }// Bản thân, quan hệ gia đình, trình độ bằng cấp,...
        public string IdLoai { get; set; }
    }

    public class HRM_NhanVien_Edit : Audit
    {
        public string IdPhieuThayDoi { get; set; }
        public string IdNhanVien { get; set; }
        public long IdCongTyEOS { get; set; }
        [MaxLength(36)]
        public string IdUser { get; set; }
        public string SoGen { get; set; }

        public long IdNhanVienEOS { get; set; }
        [MaxLength(36)]
        public string IddmTrinhDo { set; get; }
        public string NoiDaoTao { set; get; }
        public double? SoNgayNghiPhepThamNien { get; set; }
        public double? SoNgayNghiPhep { get; set; }
        public string NganhHoc { set; get; }
        [NotMapped]
        public string TenTrinhDo { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { get; set; }
        public long IdBoPhanEOS { set; get; }
        [MaxLength(36)]

        public string IdLoaiNhanVien { get; set; }
        public string IdLichLamViec { get; set; }
        public long IdChucVu { get; set; }
        public long IdChucDanh { get; set; }
        public string ChucDanhLuuHanhNoiBo { get; set; }
        [MaxLength(25)]
        public string Ten { get; set; }
        [MaxLength(50)]
        public string TenDem { get; set; }
        [MaxLength(25)]
        public string Ho { get; set; }
        [MaxLength(100)]
        public string TenDayDu { get; set; }
        [MaxLength(100)]
        public string TenThuongGoi { get; set; }
        [MaxLength(100)]
        public string LienHeKhanEmail { get; set; }
        public string DotTuyenDung { get; set; }
        [MaxLength(128)]
        public string IdGioiTinh { get; set; }
        public DateTime NgaySinh { get; set; }
        [MaxLength(500)]
        public string NoiSinh { set; get; }
        [MaxLength(500)]
        public string NguyenQuan { get; set; }
        [MaxLength(36)]
        public string IdQuocTich { get; set; }
        [MaxLength(36)]
        public string IdDanToc { get; set; }
        [MaxLength(500)]
        public string TonGiao { set; get; }
        [MaxLength(36)]
        public string IdTinhTrangHonNhan { get; set; }

        public double? Luong { get; set; }
        public string ThamNienLamViec { set; get; }
        public string LyDoNghiViec { set; get; }
        public DateTime? NgayNghiViec { get; set; }
        public DateTime? NgayQuayLai { get; set; }
        public bool isLuongHuu { get; set; } = false;
        [NotMapped]
        public double? NgayNghiViecUnix { get; set; }

        #region Thông tin liên hệ

        public string DienThoai { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(20)]
        public string DienThoaiCoQuan { get; set; }
        [MaxLength(100)]
        public string EmailCoQuan { get; set; }
        [MaxLength(20)]
        public string DienThoaiNhaRieng { get; set; }
        #endregion

        #region Hộ khẩu thường trú
        [MaxLength(500)]
        public string HoKhauThuongTruDiaChi { get; set; }
        [MaxLength(500)]
        public string HoKhauThuongTruThonXom { get; set; }
        [MaxLength(36)]
        public string HoKhauThuongTruIdTinhThanhPho { get; set; }
        [MaxLength(36)]
        public string HoKhauThuongTruIdQuanHuyen { get; set; }
        [MaxLength(36)]
        public string HoKhauThuongTruIdPhuongXa { get; set; }

        #endregion

        #region Chỗ ở hiện nay
        [MaxLength(500)]
        public string ChoOHienNayTruDiaChi { get; set; }
        [MaxLength(500)]
        public string ChoOHienNayTruThonXom { get; set; }
        [MaxLength(36)]
        public string ChoOHienNayTruIdTinhThanhPho { get; set; }
        [MaxLength(36)]
        public string ChoOHienNayTruIdQuanHuyen { get; set; }
        [MaxLength(36)]
        public string ChoOHienNayTruIdPhuongXa { get; set; }
        #endregion


        #region Liên hệ khẩn cấp
        [MaxLength(150)]
        public string LienHeKhanCapHoTen { get; set; }
        [MaxLength(36)]
        public string LienHeKhanIdQuanHe { get; set; }
        [MaxLength(50)]
        public string LienHeKhanDienThoai { get; set; }
        [MaxLength(500)]
        public string LienHeKhanDiaChi { get; set; }

        #endregion


        #region thông tin khác
        [MaxLength(50)]
        public string SoBaoHiemXaHoi { get; set; }
        [MaxLength(250)]
        public string TaiKhoanMangXaHoi { get; set; }
        [MaxLength(50)]
        public string SoSoLaoDong { get; set; }
        public string MaSoThueTNCN { get; set; }

        public DateTime? NgayCapMaSTCN { get; set; }

        [NotMapped]
        public double? NgayCapMaSTCNUnix { get; set; }

        public string TrinhDoHocVan { get; set; }
        #endregion

        public string TrangThai { get; set; }

        public string IdCapBacLuongChapNhan { set; get; }

        public DateTime? NgayNhanViecDuKien { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }
        public double ThoiGianThuViec { get; set; }
        public double PhanTramLuongThuViec { get; set; }
        public double PhanTramCongDoan { get; set; }
        public string IdUngVien { get; set; }
        [NotMapped]
        public string Tuoi { get; set; }
        [NotMapped]
        public double NgayNhanViecDuKienUnix { get; set; }

        [NotMapped]
        public List<HRM_NhanVien_PhuCap> listPhuCap { get; set; }
        [NotMapped]
        public List<HRM_QuaTrinhThamGiaBaoHiemXaHoi> listThamGiaBaoHiemXaHoi { get; set; }


        [NotMapped]
        public HDFile AnhDaiDienMoi { get; set; }

        [NotMapped]
        public HRM_FileDinhKem AnhDaiDien { get; set; }


        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }

        //[NotMapped]
        //public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }

        [NotMapped]
        public double NgaySinhUnix { get; set; }
        [NotMapped]
        public double? HoChieuNgayCapUnix { get; set; }
        [NotMapped]
        public string TenChucDanh { set; get; }
        [NotMapped]
        public string TenChucVu { set; get; }
        [NotMapped]
        public string TenQuocTich { set; get; }
        [NotMapped]
        public string GioiTinh { set; get; }

        [NotMapped]
        public HRM_dmQuocTich QuocTich { set; get; }

        [NotMapped]
        public double? NgayHetHanHoChieuUnix { get; set; }

        public DateTime? NgayBatDauLamViec { set; get; }
        public DateTime? NgayBatDauChinhThuc { set; get; }
        [NotMapped]
        public double? NgayBatDauChinhThucUnix { get; set; }
        [NotMapped]
        public double? NgayBatDauLamViecUnix { get; set; }

        public DateTime? NgayKetThucThuViec { set; get; }
        [NotMapped]
        public double? NgayKetThucThuViecUnix { get; set; }

        public DateTime? NgayBatDauThuViec { set; get; }
        [NotMapped]
        public double? NgayBatDauThuViecUnix { get; set; }


        public int NamBatDauCongPhep { set; get; }
        public int SoNamCongThemNgayPhep { set; get; } = 5;
        [NotMapped]
        public string UserName { get; set; }
        [NotMapped]
        public double SoGioMotNgay { get; set; }
        [NotMapped]
        public double TuGio { get; set; }
        [NotMapped]
        public double DenGio { get; set; }

        [NotMapped]
        public double? NgayKetThucLamViecUnix { get; set; }

        //[NotMapped]
        //public List<HRM_Link_NhanVien_BoPhan_ChucVu> lstChucVuBoPhan { get; set; }
        public void UnixToDateTime()
        {
            this.NgaySinh = dungchung.UnixTimeStampToDateTime(this.NgaySinhUnix);

            this.NgayBatDauLamViec = dungchung.UnixTimeStampToDateTime(this.NgayBatDauLamViecUnix);
        }
        public void DateTimeToUnix()
        {
            this.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(this.NgayBatDauLamViec);

            this.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(this.NgaySinh);

        }

        [NotMapped]
        public List<HRM_NhanVienQuanHeGiaDinh> ListQuanHeGiaDinhNhanVien { get; set; }
        [NotMapped]
        public List<HRM_NhanVien_GiayToTuyThan> listGiayToTuyThan { get; set; }

        [NotMapped]
        public List<HRM_NhanVienBangCap> ListNhanVienBangCap { get; set; }
        [NotMapped]
        public List<HRM_NgonNguNhanVien> ListNhanVienNgonNgu { get; set; }
        [NotMapped]
        public List<HRM_ChungChiNhanVien> ListNhanVienChungChi { get; set; }
        [NotMapped]
        public List<HRM_QuaTrinhCongTac> ListQuaTrinhCongTac { get; set; }
        [NotMapped]
        public List<HRM_QuaTrinhCongTacNgoaiCongTy> ListQuaTrinhCongTacNgoaiCongTy { get; set; }

        [NotMapped]
        public List<HRM_NhanVien_TaiKhoanNganHang> ListNganHang { get; set; }
        [NotMapped]
        public clsNhanVien itemNhanVienEOS { get; set; }
        [NotMapped]
        public HRM_dmTinhTrangHonNhan itemTinhTrangHonNhan { get; set; }
        [NotMapped]
        public HRM_dmDanToc itemDanToc { get; set; }
        [NotMapped]
        public HRM_dmQuanHeGiaDinh itemLienHeKhanQuanHe { get; set; }
        [NotMapped]
        public HRM_dmLoaiNhanVien itemLoaiNhanVien { get; set; }
        [NotMapped]
        public List<HRM_NhanVien_SoDienThoai> listSoDienThoai { get; set; }

        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public HRM_HopDong HopDong { get; set; }

        [NotMapped]
        public List<HRM_NhanVien_FileDinhKemHoSo> listFileDinhKemHoSo { get; set; }

        public HRM_NhanVien_Edit() { }
        public HRM_NhanVien_Edit(HRM_TuyenDung_UngVien a)
        {
            if (!string.IsNullOrEmpty(a.HoTenDayDu))
            {
                List<string> liststring = a.HoTenDayDu.Split(' ').ToList();
                Ten = liststring.Last();
                Ho = liststring.First();
                liststring = liststring.Where(b => b != Ten && b != Ho).ToList();
                TenDem = string.Join(" ", liststring);
            }

            TenDayDu = a.HoTenDayDu;
            //SoChungMinhThu = a.CCCD;
            TrangThai = "DangLamViec";
            IdGioiTinh = a.GioiTinh;
            if (a.GioiTinh != "KhongXacDinh")
                IdGioiTinh = a.GioiTinh == "Nu" ? "Nữ" : "Nam";
            if (a.NgaySinh != null && a.NgaySinh.Value.ToString("dd/MM/yyyy") != "01/01/1970")
            {
                NgaySinh = a.NgaySinh.Value;
                NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(a.NgaySinh);
            }
            if (a.NgayCap != null && a.NgayCap.Value.ToString("dd/MM/yyyy") != "01/01/1970")
            {
                //    CanCuocNgayCap = a.NgayCap.Value;
                //    CanCuocNgayCapUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayCap);
                //    ChungMinhThuNgayCap = a.NgayCap.Value;
                //    ChungMinhThuNgayCapUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayCap);

            }

            IdQuocTich = a.IddmQuocTich;
            IdDanToc = a.IddmDanToc;
            IdTinhTrangHonNhan = a.IddmTinhTrangHonNhan;
            IddmTrinhDo = a.IddmTrinhDoHocVan;
            ChoOHienNayTruDiaChi = a.DiaChi;
            ChoOHienNayTruIdTinhThanhPho = a.IddmTinhThanhPho;
            ChoOHienNayTruIdQuanHuyen = a.IddmQuanHuyen;
            ChoOHienNayTruIdPhuongXa = a.IddmPhuongXa;
            HoKhauThuongTruDiaChi = a.HoKhauThuongTru;
            HoKhauThuongTruIdTinhThanhPho = a.IddmTinhThanhPhoThuongTru;
            HoKhauThuongTruIdQuanHuyen = a.IddmQuanHuyenThuongTru;
            HoKhauThuongTruIdPhuongXa = a.IddmPhuongXaThuongTru;
            if (a.NgayNhanViecDuKien != null)
            {
                NgayBatDauLamViec = a.NgayNhanViecDuKien.Value;
                NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayNhanViecDuKien);
            }
            DienThoai = a.DienThoaiDiDong;
            Email = a.Email;
            IdCongTyEOS = a.IdCoSoTrungTuyen ?? 0;
            IdBoPhanEOS = a.IdBoPhanEOS ?? 0;
            DotTuyenDung = a.DotTuyenDung;

            IdUngVien = a.IdGuid;

        }
    }
    public class HRM_NhanVien : Audit
    {
        public long IdCongTyEOS { get; set; }
        [MaxLength(36)]
        public string IdUser { get; set; }
        public string SoGen { get; set; }

        public long IdNhanVienEOS { get; set; }
        [MaxLength(36)]
        public string IddmTrinhDo { set; get; }
        public string NoiDaoTao { set; get; }
        public double? SoNgayNghiPhepThamNien { get; set; }
        public double? SoNgayNghiPhep { get; set; }
        public string NganhHoc { set; get; }
        [NotMapped]
        public string TenTrinhDo { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { get; set; }
        public long IdBoPhanEOS { set; get; }
        [MaxLength(36)]

        public string IdLoaiNhanVien { get; set; }
        //public string IdLichLamViec { get; set; }
        public long IdChucVu { get; set; }
        public long IdChucDanh { get; set; }
        public string ChucDanhLuuHanhNoiBo { get; set; }
        [MaxLength(25)]
        public string Ten { get; set; }
        [MaxLength(50)]
        public string TenDem { get; set; }
        [MaxLength(25)]
        public string Ho { get; set; }
        [MaxLength(100)]
        public string TenDayDu { get; set; }
        [MaxLength(100)]
        public string TenThuongGoi { get; set; }
        [MaxLength(100)]
        public string LienHeKhanEmail { get; set; }
        public string DotTuyenDung { get; set; }
        [MaxLength(128)]
        public string IdGioiTinh { get; set; }
        public DateTime NgaySinh { get; set; }
        [MaxLength(500)]
        public string NoiSinh { set; get; }
        [MaxLength(500)]
        public string NguyenQuan { get; set; }
        [MaxLength(36)]
        public string IdQuocTich { get; set; }
        [MaxLength(36)]
        public string IdDanToc { get; set; }
        [MaxLength(500)]
        public string TonGiao { set; get; }
        [MaxLength(36)]
        public string IdTinhTrangHonNhan { get; set; }

        public double? Luong { get; set; }
        public double? PhanTramTruyThuBHXH { get; set; }
        public double? PhanTramTruyThuBHYT { get; set; }
        public string ThamNienLamViec { set; get; }
        public string LyDoNghiViec { set; get; }
        public int GiamBaoHiem { set; get; }
        public DateTime? NgayNghiViec { get; set; }
        public DateTime? NgayQuayLai { get; set; }
        public DateTime? ThangTruyThuBHYT { get; set; }
        public DateTime? ThangTruyThuBHXH { get; set; }
        public bool isTruyThu { get; set; } = false;
        public bool isLuongHuu { get; set; } = false;
        public bool isCamKet { get; set; } = false;

        [NotMapped]
        public double? NgayNghiViecUnix { get; set; }

        #region Thông tin liên hệ

        public string DienThoai { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(20)]
        public string DienThoaiCoQuan { get; set; }
        [MaxLength(100)]
        public string EmailCoQuan { get; set; }
        [MaxLength(20)]
        public string DienThoaiNhaRieng { get; set; }
        #endregion

        #region Hộ khẩu thường trú
        [MaxLength(500)]
        public string HoKhauThuongTruDiaChi { get; set; }
        [MaxLength(500)]
        public string HoKhauThuongTruThonXom { get; set; }
        [MaxLength(36)]
        public string HoKhauThuongTruIdTinhThanhPho { get; set; }
        [MaxLength(36)]
        public string HoKhauThuongTruIdQuanHuyen { get; set; }
        [MaxLength(36)]
        public string HoKhauThuongTruIdPhuongXa { get; set; }

        #endregion

        #region Chỗ ở hiện nay
        [MaxLength(500)]
        public string ChoOHienNayTruDiaChi { get; set; }
        [MaxLength(500)]
        public string ChoOHienNayTruThonXom { get; set; }
        [MaxLength(36)]
        public string ChoOHienNayTruIdTinhThanhPho { get; set; }
        [MaxLength(36)]
        public string ChoOHienNayTruIdQuanHuyen { get; set; }
        [MaxLength(36)]
        public string ChoOHienNayTruIdPhuongXa { get; set; }
        #endregion


        #region Liên hệ khẩn cấp
        [MaxLength(150)]
        public string LienHeKhanCapHoTen { get; set; }
        [MaxLength(36)]
        public string LienHeKhanIdQuanHe { get; set; }
        [MaxLength(50)]
        public string LienHeKhanDienThoai { get; set; }
        [MaxLength(500)]
        public string LienHeKhanDiaChi { get; set; }

        #endregion


        #region thông tin khác
        [MaxLength(50)]
        public string SoBaoHiemXaHoi { get; set; }
        public bool isThamGiaBHXH { get; set; } = true;
        [MaxLength(250)]
        public string TaiKhoanMangXaHoi { get; set; }
        [MaxLength(50)]
        public string SoSoLaoDong { get; set; }
        public string MaSoThueTNCN { get; set; }
        public string MaSoThueCMT { get; set; }

        public DateTime? NgayCapMaSTCN { get; set; }

        [NotMapped]
        public double? NgayCapMaSTCNUnix { get; set; }

        public string TrinhDoHocVan { get; set; }
        #endregion

        public string TrangThai { get; set; }

        public string IdCapBacLuongChapNhan { set; get; }

        public DateTime? NgayNhanViecDuKien { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }
        public double ThoiGianThuViec { get; set; }
        public double PhanTramLuongThuViec { get; set; }
        public double PhanTramCongDoan { get; set; }
        public string IdUngVien { get; set; }
        [NotMapped]
        public bool isChoPheDuyetThayDoiThongTin { get; set; } = false;
        [NotMapped]
        public string Tuoi { get; set; }
        [NotMapped]
        public double NgayNhanViecDuKienUnix { get; set; }
        [NotMapped]
        public List<long> listIdCongTy { get; set; }
        [NotMapped]
        public List<string> listIdCaLamViec { get; set; }

        [NotMapped]
        public List<HRM_NhanVien_PhuCap> listPhuCap { get; set; }
        [NotMapped]
        public HRM_BaoHiem_HopDong BaoHiemNgoai { set; get; } = new HRM_BaoHiem_HopDong();
        [NotMapped]
        public HRM_NhanVien_BaoHiemTaiNan BaoHiemTaiNan { set; get; } = new HRM_NhanVien_BaoHiemTaiNan();

        [NotMapped]
        public long? IdDuAnDieuDong { get; set; }
        [NotMapped]
        public long? IdDuAnCurrent { get; set; }

        [NotMapped]
        public HDFile AnhDaiDienMoi { get; set; }

        [NotMapped]
        public HRM_FileDinhKem AnhDaiDien { get; set; }


        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public string ThoiGianThamGia { get; set; }
        [NotMapped]
        public List<HRM_QuaTrinhThamGiaBaoHiemXaHoi> listThamGiaBaoHiemXaHoi { get; set; }


        //[NotMapped]
        //public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }
        public bool IsDaThuCoc { get; set; } = true;
        public double? ThuCoc { get; set; }
        [NotMapped]
        public double NgaySinhUnix { get; set; }
        [NotMapped]
        public double ThangTruyThuBHYTUnix { get; set; }
        [NotMapped]
        public double ThangTruyThuBHXHUnix { get; set; }

        [NotMapped]
        public double? HoChieuNgayCapUnix { get; set; }
        [NotMapped]
        public string TenChucDanh { set; get; }
        [NotMapped]
        public string TenChucVu { set; get; }
        [NotMapped]
        public string TenLoaiNhanVien { set; get; }

        [NotMapped]
        public string TenQuocTich { set; get; }
        [NotMapped]
        public string GioiTinh { set; get; }

        [NotMapped]
        public HRM_dmQuocTich QuocTich { set; get; }

        [NotMapped]
        public double? NgayHetHanHoChieuUnix { get; set; }

        public DateTime? NgayBatDauLamViec { set; get; }
        public DateTime? NgayBatDauChinhThuc { set; get; }
        [NotMapped]
        public double? NgayBatDauChinhThucUnix { get; set; }
        [NotMapped]
        public double? NgayBatDauLamViecUnix { get; set; }

        public DateTime? NgayKetThucThuViec { set; get; }
        [NotMapped]
        public double? NgayKetThucThuViecUnix { get; set; }

        public DateTime? NgayBatDauThuViec { set; get; }
        [NotMapped]
        public double? NgayBatDauThuViecUnix { get; set; }

        public string TaiKhoanNganHang { get; set; }
        public string TenNganHang { get; set; }
        public int NamBatDauCongPhep { set; get; }
        public int SoNamCongThemNgayPhep { set; get; } = 5;
        public double PhiCongDoan { set; get; }
        [NotMapped]
        public string UserName { get; set; }
        [NotMapped]
        public double SoGioMotNgay { get; set; }
        [NotMapped]
        public double TuGio { get; set; }
        [NotMapped]
        public bool isChuaCoBHXH { get; set; } = false;
        [NotMapped]
        public double DenGio { get; set; }


        [NotMapped]
        public double? NgayKetThucLamViecUnix { get; set; }

        //[NotMapped]
        //public List<HRM_Link_NhanVien_BoPhan_ChucVu> lstChucVuBoPhan { get; set; }
        public void UnixToDateTime()
        {
            this.NgaySinh = dungchung.UnixTimeStampToDateTime(this.NgaySinhUnix);

            this.NgayBatDauLamViec = dungchung.UnixTimeStampToDateTime(this.NgayBatDauLamViecUnix);
        }
        public void DateTimeToUnix()
        {
            this.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(this.NgayBatDauLamViec);

            this.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(this.NgaySinh);

        }

        [NotMapped]
        public List<HRM_NhanVienQuanHeGiaDinh> ListQuanHeGiaDinhNhanVien { get; set; }
        [NotMapped]
        public List<HRM_NhanVien_GiayToTuyThan> listGiayToTuyThan { get; set; }
        [NotMapped]
        public List<HRM_NhanVien_Email> listEmail { get; set; }

        [NotMapped]
        public List<HRM_NhanVienBangCap> ListNhanVienBangCap { get; set; }
        [NotMapped]
        public List<HRM_NgonNguNhanVien> ListNhanVienNgonNgu { get; set; }
        [NotMapped]
        public List<HRM_NhanVien_KhamSucKhoe> listKhamSucKhoe { get; set; }

        [NotMapped]
        public List<HRM_ChungChiNhanVien> ListNhanVienChungChi { get; set; }
        [NotMapped]
        public List<HRM_QuaTrinhCongTac> ListQuaTrinhCongTac { get; set; }
        [NotMapped]
        public List<HRM_QuaTrinhCongTacNgoaiCongTy> ListQuaTrinhCongTacNgoaiCongTy { get; set; }

        [NotMapped]
        public List<HRM_NhanVien_TaiKhoanNganHang> ListNganHang { get; set; }
        [NotMapped]
        public clsNhanVien itemNhanVienEOS { get; set; }
        [NotMapped]
        public HRM_dmTinhTrangHonNhan itemTinhTrangHonNhan { get; set; }
        [NotMapped]
        public HRM_dmDanToc itemDanToc { get; set; }
        [NotMapped]
        public HRM_dmQuanHeGiaDinh itemLienHeKhanQuanHe { get; set; }
        [NotMapped]
        public HRM_dmLoaiNhanVien itemLoaiNhanVien { get; set; }
        [NotMapped]
        public List<HRM_NhanVien_SoDienThoai> listSoDienThoai { get; set; }

        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public HRM_HopDong HopDong { get; set; }

        [NotMapped]
        public List<HRM_NhanVien_FileDinhKemHoSo> listFileDinhKemHoSo { get; set; }

        public HRM_NhanVien() { }
        public HRM_NhanVien(HRM_TuyenDung_UngVien a)
        {
            if (!string.IsNullOrEmpty(a.HoTenDayDu))
            {
                List<string> liststring = a.HoTenDayDu.Split(' ').ToList();
                Ten = liststring.Last();
                Ho = liststring.First();
                liststring = liststring.Where(b => b != Ten && b != Ho).ToList();
                TenDem = string.Join(" ", liststring);
            }

            TenDayDu = a.HoTenDayDu;
            //SoChungMinhThu = a.CCCD;
            TrangThai = "DangLamViec";
            IdGioiTinh = a.GioiTinh;
            if (a.GioiTinh != "KhongXacDinh")
                IdGioiTinh = a.GioiTinh == "Nu" ? "Nữ" : "Nam";
            if (a.NgaySinh != null && a.NgaySinh.Value.ToString("dd/MM/yyyy") != "01/01/1970")
            {
                NgaySinh = a.NgaySinh.Value;
                NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(a.NgaySinh);
            }
            if (a.NgayCap != null && a.NgayCap.Value.ToString("dd/MM/yyyy") != "01/01/1970")
            {
                //    CanCuocNgayCap = a.NgayCap.Value;
                //    CanCuocNgayCapUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayCap);
                //    ChungMinhThuNgayCap = a.NgayCap.Value;
                //    ChungMinhThuNgayCapUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayCap);

            }

            IdQuocTich = a.IddmQuocTich;
            IdDanToc = a.IddmDanToc;
            IdTinhTrangHonNhan = a.IddmTinhTrangHonNhan;
            IddmTrinhDo = a.IddmTrinhDoHocVan;
            ChoOHienNayTruDiaChi = a.DiaChi;
            ChoOHienNayTruIdTinhThanhPho = a.IddmTinhThanhPho;
            ChoOHienNayTruIdQuanHuyen = a.IddmQuanHuyen;
            ChoOHienNayTruIdPhuongXa = a.IddmPhuongXa;
            HoKhauThuongTruDiaChi = a.HoKhauThuongTru;
            HoKhauThuongTruIdTinhThanhPho = a.IddmTinhThanhPhoThuongTru;
            HoKhauThuongTruIdQuanHuyen = a.IddmQuanHuyenThuongTru;
            HoKhauThuongTruIdPhuongXa = a.IddmPhuongXaThuongTru;
            if (a.NgayNhanViecDuKien != null)
            {
                NgayBatDauLamViec = a.NgayNhanViecDuKien.Value;
                NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayNhanViecDuKien);
            }
            DienThoai = a.DienThoaiDiDong;
            Email = a.Email;
            IdCongTyEOS = a.IdCoSoTrungTuyen ?? 0;
            IdBoPhanEOS = a.IdBoPhanEOS ?? 0;
            DotTuyenDung = a.DotTuyenDung;

            IdUngVien = a.IdGuid;

        }
    }
    public class HRM_NhanVien_BaoHiemTaiNan : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public string SoHopDong { set; get; }
        public DateTime? NgayBatDau { set; get; }
        public string GhiChu { get; set; }
        public double SoTienBaoHiem { get; set; }
        public int Giam { get; set; }// 1 Tăng, 2 Giảm
        [NotMapped]
        public double NgayBatDauUnix { set; get; }


        public DateTime? NgayKetThuc { set; get; }
        [NotMapped]
        public double NgayKetThucUnix { set; get; }
        [NotMapped]
        public string MaNhanVien { set; get; }

        public string SoSoBaoHiem { set; get; }
        public string NoiCapSoBaoHiem { set; get; }
        [NotMapped]
        public bool isDeleted { get; set; }
        public HRM_BaoHiem_HopDong Clone()
        {
            return (HRM_BaoHiem_HopDong)this.MemberwiseClone();
        }
    }
    public class HRM_NhanVien_KhamSucKhoe : Audit
    {
        public string IdNhanVien { get; set; }
        public string NoiKham { get; set; }
        public string LoaiHinhKham { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayKham { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        [NotMapped]
        public double NgayKhamUnix { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string ThuongTru { get; set; }
        [NotMapped]
        public string CCCD { get; set; }
        [NotMapped]
        public DateTime NgayCap { get; set; }
        [NotMapped]
        public DateTime NgaySinh { get; set; }
        public HRM_NhanVien_KhamSucKhoe()
        {

        }
        public HRM_NhanVien_KhamSucKhoe(HRM_HopDong_KhamSucKhoe model)
        {
            IdGuid = Guid.NewGuid().ToString();
            NoiKham = model.NoiKham;
            LoaiHinhKham = model.LoaiHinhKham;
            GhiChu = model.GhiChu;
            IdNhanVien = model.IdNhanVien;
            NgayKham = model.NgayKham;
            NgayKetThuc = model.NgayKetThuc;
        }
    }
    public class HRM_NhanVien_GiayToTuyThan : Audit
    {
        public string IdNhanVien { get; set; }
        public string NoiDung { get; set; }
        public int Loai { get; set; } // 1 CCCD, 2 CMT, 3 Hộ chiếu, 4 khác
        public string So { get; set; }
        public string NoiCap { get; set; }
        public DateTime? NgayCap { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public bool isApDung { get; set; }
        [NotMapped]
        public string TenNoiCap { get; set; }
        [NotMapped]
        public double? NgayCapUnix { get; set; }

        [NotMapped]
        public double? NgayHetHanUnix { get; set; }

    }
    public class HRM_NhanVien_Email : Audit
    {
        public string IdNhanVien { get; set; }
        public string Email { get; set; }
        public bool isApDung { get; set; }
    }

    public class HRM_NhanVien_SoDienThoai : Audit
    {
        public string IdNhanVien { get; set; }
        public string NoiDung { get; set; }
        public string SoDienThoai { get; set; }
        public bool isApDung { get; set; }
    }
    public class HRM_NhanVien_TaiKhoanNganHang : Audit
    {
        public string idNhanVien { get; set; }//HRM_NhanVien
        public string idHopDong { get; set; }//HRM_HopDong
        public string SoTaiKhoan { get; set; }
        public string TenNganHang { get; set; }
        public string MaNganHang { get; set; }
        public string ChiNhanh { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
        public HRM_NhanVien_TaiKhoanNganHang()
        {

        }

        public HRM_NhanVien_TaiKhoanNganHang(HRM_HopDong model)
        {
            IdGuid = Guid.NewGuid().ToString();
            ChiNhanh = "";
            idNhanVien = model.IdNhanVien;
            SoTaiKhoan = model.LuongHopDong.SoTaiKhoan;
            TenNganHang = model.LuongHopDong.TenNganHang;
            idHopDong = model.IdGuid;
        }
    }
    public class HRM_NhanVien_FileDinhKemHoSo : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(5000)]
        public string MoTa { set; get; }
        [MaxLength(36)]
        public string MaLoai { set; get; }
        public int SoLuong { get; set; }
        public DateTime NgayCap { get; set; }

        public bool isDaNop { get; set; }
        [NotMapped]
        public double NgayCapUnix { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }

        //[NotMapped]
        //public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }
    }



    #region Trình độ 

    public class HRM_NhanVienBangCap : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string NoiDaoTao { get; set; }
        public string ChuyenNganh { get; set; }
        [MaxLength(36)]
        public string IdTrinhDoDaoTao { get; set; } //eTrinhDoDaoTao
        public DateTime? NgayTotNghiep { get; set; }
        public string HinhThucDaoTao { get; set; }
        public string XepLoai { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
        [NotMapped]
        public double NgayTotNghiepUnix { get; set; }

        [NotMapped]
        public HRM_dmTrinhDoDaoTao itemTrinhDoDaoTao { get; set; }

    }


    public class HRM_ThanhVienHiepHoi : Audit
    {
        public string IdNhanVien { get; set; }
        public string TenHiepHoi { get; set; }
        public string ViTriThanhVien { get; set; }
        public DateTime ThoiGianhieuLuc { get; set; }


        [NotMapped]
        public double ThoiGianhieuLucUnix { get; set; }


        [NotMapped]
        public List<HDFile> lstFileDinhKemMoi { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }

    }

    public class HRM_NgonNguNhanVien : Audit
    {
        public string IdNhanVien { get; set; }
        public string IdNgonNgu { get; set; }

        public string IdXepLoaiNghe { get; set; }//HRM_dmXepLoai
        public string IdXepLoaiNoi { get; set; } //HRM_dmXepLoai
        public string IdXepLoaiDoc { get; set; } //HRM_dmXepLoai
        public string IdXepLoaiViet { get; set; }//HRM_dmXepLoai

        [NotMapped]
        public HRM_dmNgonNgu NgonNgu { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }

        //[NotMapped]
        //public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
        [NotMapped]
        public HRM_dmXepLoai itemXepLoaiNghe { get; set; }
        [NotMapped]
        public HRM_dmXepLoai itemXepLoaiNoi { get; set; }
        [NotMapped]
        public HRM_dmXepLoai itemXepLoaiDoc { get; set; }
        [NotMapped]
        public HRM_dmXepLoai itemXepLoaiViet { get; set; }

    }

    public class HRM_ChungChiNhanVien : Audit
    {
        public string IdNhanVien { get; set; }
        public string IdNhomChungChi { get; set; }
        public string TenChungChi { get; set; }
        public string TrinhDo { get; set; } //eTrinhDoChungChi
        public DateTime NgayCap { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string NoiCap { get; set; }


        [NotMapped]
        public double NgayCapUnix { get; set; }

        [NotMapped]
        public double NgayHetHanUnix { get; set; }

        [NotMapped]
        public HRM_dmNhomChungChi NhomChungChi { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }

        //[NotMapped]
        //public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
        public void DateTimeToUnix()
        {
            this.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(this.NgayCap);
            this.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(this.NgayHetHan);
        }
        public void UnixToDateTime()
        {
            this.NgayCap = dungchung.UnixTimeStampToDateTime(this.NgayCapUnix);
            this.NgayHetHan = dungchung.UnixTimeStampToDateTime(this.NgayHetHanUnix);
        }
    }

    #endregion

    #region Quá trình công tác

    public class HRM_QuaTrinhCongTac : Audit
    {
        public string IdNhanVien { get; set; }
        public string ThoiGian { get; set; }

        public string DonViCongTac { get; set; }
        public string DiaDiem { get; set; }
        public string ViTriCongViec { get; set; }
        public string IdQuocTich { get; set; }
        public string NguoiXacNhanTen { get; set; }
        public string NguoiXacNhanChucVu { get; set; }
        public string NguoiXacNhanEmail { get; set; }
        public string NguoiXacNhanDienThoai { get; set; }
        public string TomTatCongViec { get; set; }

        [NotMapped]
        public HRM_dmQuocTich QuocTich { get; set; }
        //[NotMapped]
        //public List<HDFile> lstFileDinhKemMoi { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }

    public class HRM_QuaTrinhCongTacNgoaiCongTy : Audit
    {
        public string IdNhanVien { get; set; }
        public string ThoiGian { get; set; }

        public string DonViCongTac { get; set; }
        public string DiaDiem { get; set; }
        public string ViTriCongViec { get; set; }
        public string IdQuocTich { get; set; }
        public string NguoiXacNhanTen { get; set; }
        public string NguoiXacNhanChucVu { get; set; }
        public string NguoiXacNhanEmail { get; set; }
        public string NguoiXacNhanDienThoai { get; set; }
        public string TomTatCongViec { get; set; }

        [NotMapped]
        public HRM_dmQuocTich QuocTich { get; set; }


        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }


    #endregion






    #region Quan hệ gia đình
    public class HRM_NhanVienQuanHeGiaDinh : Audit
    {
        [MaxLength(128)]
        public string IdNhanVien { get; set; }
        [MaxLength(128)]
        public string IdQuanHe { get; set; }
        [MaxLength(500)]
        public string HoTen { get; set; }
        public int NamSinh { get; set; }
        public string IdGioiTinh { set; get; }
        public DateTime? NgaySinh { get; set; }
        [MaxLength(128)]
        public string IdQuocTich { get; set; }

        [MaxLength(20)]
        public string SoCMND { get; set; }
        [MaxLength(50)]
        public string SoHoChieu { get; set; }
        [MaxLength(50)]
        public string SoDinhDanh { get; set; }
        [MaxLength(1000)]
        public string DiaChi { get; set; }
        [MaxLength(50)]
        public string DienThoaiDiDong { get; set; }
        [MaxLength(50)]
        public string DienThoaiNhaRieng { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(200)]
        public string NgheNghiep { get; set; }
        [MaxLength(200)]
        public string MaSoThueCaNhan { get; set; }
        [MaxLength(500)]
        public string NoiLamViec { get; set; }
        public bool isLaNguoiPhuThuoc { get; set; }
        public DateTime ThoiDiemGiamTru { get; set; }
        public DateTime KetThucGiamTru { get; set; }

        [MaxLength(500)]
        public string GhiChu { get; set; }
        public bool isDeleted { get; set; }

        [NotMapped]
        public double NgaySinhUnix { get; set; }

        [NotMapped]
        public double ThoiDiemGiamTruUnix { get; set; }

        [NotMapped]
        public double KetThucGiamTruUnix { get; set; }

        [NotMapped]
        public HRM_dmQuocTich QuocTich { get; set; }

        //[NotMapped]
        //public List<HDFile> lstFileDinhKemMoi { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public HRM_dmQuanHeGiaDinh itemQuanHe { get; set; }

    }
    #endregion

    #region Bổ nhiệm điều đồng

    public class HRM_BoNhiemDieuDongDuAn : AudiQuyTrinh
    {
        [StringLength(36)]
        public string IdNguoiKy { get; set; }
        [NotMapped]
        public string TenNguoiKy { set; get; }
        [NotMapped]
        public string TenCongTy { set; get; }

        [StringLength(36)]
        public string IdChucVuNguoiKy { get; set; }
        [StringLength(36)]
        public string IdNhanVien { get; set; }
        public long IdNhanVienEOS { get; set; }
        public string SoQuyetDinh { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }

        public int KieuLuong { set; get; }// 0 Lương gross/ net 1
        public DateTime? NgayKy { get; set; }
        [NotMapped]
        public double NgayKyUnix { get; set; }
        public string LoaiQuyetDinh { get; set; }
        public long? IdDuAnEOS { get; set; }
        public long? IdCongTyEOS { set; get; }
        public long? IdCongTyEOSNew { set; get; }
        public long? IdBoPhanEOSOld { set; get; }
        public long? IdBoPhanEOSNew { set; get; }
        public long? IdChucDanh { get; set; } // Chức vụ dự án
        public long? IdChucVu { get; set; } // Chức vụ dự án
        [NotMapped]
        public long? IdChucVuOld { get; set; } // Chức vụ dự án
        public DateTime? NgayHieuLuc { get; set; }
        [NotMapped]
        public double NgayHieuLucUnix { get; set; }
        public DateTime? NgayBatDau { get; set; }
        [NotMapped]
        public double NgayBatDauUnix { get; set; }


        public DateTime? NgayKetThuc { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { get; set; }

        public string TrachNhiem { get; set; }

        public string QuyenLoi { get; set; }

        public double LuongThucTeHienTai { set; get; }
        public double LuongHachToanHienTai { set; get; }
        public double SoDieuChinhThucTe { set; get; }
        public double SoDieuChinhHachToan { set; get; }
        public double LuongThucTeSauDieuChinh { set; get; }
        public double LuongHachToanSauDieuChinh { set; get; }
        public double SoGioLamViec { set; get; }

        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }

        [NotMapped]
        public List<HRM_HopDongPhuCap> lstPhuCap { set; get; }


    }

    public class HRM_TheoDoiBoNhiemDieuDongDuAn : Audit
    {
        public string IdNhanVien { get; set; }
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public long IdChucDanh { get; set; }
        public DateTime Ngay { get; set; }
        public bool IsDaUpdate { get; set; }
    }

    public class HRM_BoNhiemChucVu : AudiQuyTrinh
    {
        [StringLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(50)]
        public string MaNhanVien { set; get; }
        [MaxLength(150)]
        public string TenNhanVien { get; set; }
        public string SoQuyetDinh { get; set; }
        public int KieuLuong { set; get; }// 0 Lương gross/ net 1
        public DateTime NgayKy { get; set; }
        [NotMapped]
        public double NgayKyUnix { get; set; }
        public DateTime NgayBatDau { get; set; }
        [NotMapped]
        public double NgayBatDauUnix { get; set; }

        [StringLength(36)]
        public string IdNguoiKy { get; set; }

        [NotMapped]
        public string TenNguoiKy { set; get; }
        [NotMapped]
        public string TenChucDanhMoi { set; get; }
        [NotMapped]
        public string TenChucDanhCu { set; get; }

        public string IdCaLamViec { set; get; }
        public string IdCapBacLuongDeXuat { set; get; }

        public long IdChucVuNguoiKy { get; set; }
        public bool isKiemNhiem { set; get; }
        public long IdBoPhanEOS { get; set; }
        public long IdBoPhanEOSCu { get; set; }
        public long IdChucVuEOSCu { get; set; } // Chức vụ Cux
        public long IdChucVuEOS { get; set; } // Chức vụ 
        public long IdChucDanhEOS { get; set; } // Chức vụ 
        public long IdChucDanhEOSCu { get; set; } // chucDanhCu

        public long IdCongTyEOS { set; get; }


        public double LuongThucTeHienTai { set; get; }
        public double LuongHachToanHienTai { set; get; }
        public double SoDieuChinhThucTe { set; get; }
        public double SoDieuChinhHachToan { set; get; }
        public double LuongThucTeSauDieuChinh { set; get; }
        public double LuongHachToanSauDieuChinh { set; get; }
        public double SoGioLamViec { set; get; }

        [MaxLength(2500)]
        public string TrachNhiem { get; set; }
        [MaxLength(2500)]
        public string QuyenLoi { get; set; }
        [NotMapped]
        public List<HRM_HopDongPhuCap> lstPhuCap { set; get; }

        //public HRM_BoNhiemChucVuLuong_ChiTiet listChiTietLuong { set; get; }
        [NotMapped]
        public List<HRM_BoNhiemChucVu_BoPhan> lstBoPhan = new List<HRM_BoNhiemChucVu_BoPhan>();
    }
    public class HRM_BoNhiemChucVu_BoPhan : Audit
    {
        public string IdQuyetDinh { get; set; }
        public long IdBoPhanEOS { get; set; }
        [MaxLength(5000)]
        public string NoiDung { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        //[NotMapped]
        //public bool isDeleted { get; set; }
    }


    #endregion


    #region Quyet toán phép năm 
    public class HRM_QuyetToanPhepNam : Audit
    {
        [MaxLength(50)]
        public string SoPhieu { set; get; }
        [MaxLength(5000)]
        public string NoiDung { set; get; }
        [MaxLength(36)]
        public string IdNguoiLap { set; get; }
        [MaxLength(100)]
        public string TenNguoiLap { set; get; }
        public int Nam { set; get; }
        public long IdCongTyEOS { set; get; }
        public long IdChucVuNguoiLap { set; get; }
        public long IdBoPhanNguoiLap { set; get; }

        public DateTime NgayQuyetToan { set; get; }
        [MaxLength(36)]
        public string IdTrangThai { set; get; }
        [MaxLength(250)]
        public string TenTrangThai { set; get; }
        public bool isKetThuc { set; get; }
        [NotMapped]
        public double NgayQuyetToanUnix { set; get; }
        [NotMapped]
        public List<HRM_QuyetToanPhepNam_NhanVien> listNhanVien = new List<HRM_QuyetToanPhepNam_NhanVien>();
    }

    public class HRM_QuyetToanPhepNam_NhanVien : Audit
    {
        [MaxLength(36)]
        public string IdQuyetToanPhepNam { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        public long IdBoPhanEOS { set; get; }
        [NotMapped]
        public string TenBoPhan { set; get; }
        public long IdChucVuEOS { set; get; }
        [NotMapped]
        public string TenChucVu { set; get; }
        [MaxLength(150)]
        public string TenNhanVien { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { set; get; }
        public DateTime NgayBatDauLamViec { set; get; }
        [NotMapped]
        public double NgayBatDauLamViecUnix { set; get; }
        public int Nam { set; get; }
        public double SoNgayPhep { set; get; }
        public double SoNgayDaNghi { set; get; }
        public double SoNgayPhepConLai { set; get; }
        public int SoNamCongThemNgayPhep { set; get; }
        public int TrangThai { set; get; }
        [NotMapped]
        public string TenTrangThai { set; get; }
        [NotMapped]
        public bool isXoa { set; get; }
    }
    #endregion
    #region Điều chỉnh lương

    public class HRM_QuyetDinhDieuChinhLuong : AudiQuyTrinh
    {
        public string IdBoPhan { set; get; }
        public long IdChucVuEOS { get; set; }
        public long IdChucDanhEOS { get; set; }

        public string IdCaLamViec { get; set; }
        public string SoQuyetDinh { set; get; }
        public DateTime? NgayKy { get; set; }
        public string IdNguoiKy { get; set; }
        public long IdCongTyEOS { get; set; }



        public DateTime? NgayHieuLuc { get; set; }

        public string LyDo { get; set; }
        public double SoGioLamViec { set; get; }

        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
        [NotMapped]
        public double NgayKyUnix { get; set; }

        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }

        [NotMapped]
        public double NgayHieuLucUnix { get; set; }

        [NotMapped]
        public List<HRM_QuyetDinhDieuChinhLuong_NhanVien> listNhanVien { set; get; }
    }
    public class HRM_QuyetDinhDieuChinhLuong_NhanVien : Audit
    {
        [MaxLength(36)]
        public string IdQuyetDinh { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }

        public long? IdBoPhanEOS { set; get; }
        public long? IdChucVuEOS { get; set; }
        public long? IdChucDanhEOS { get; set; }
        public string IdCaLamViec { get; set; }
        public long? IdCongTyEOS { get; set; }

        public string NoiDung { get; set; }

        public DateTime? NgayHieuLuc { get; set; }
        public string LyDo { get; set; }
        public int KieuLuong { set; get; }// 0 Lương gross/ net 1
        public int? LoaiLuong { set; get; }
        public int TrangThai { set; get; }// 1 Duyệt 2 không duyệt
        public double LuongThucTeHienTai { set; get; }
        public double LuongHachToanHienTai { set; get; }
        public double SoDieuChinhThucTe { set; get; }
        public double SoDieuChinhHachToan { set; get; }
        public double LuongThucTeSauDieuChinh { set; get; }
        public double LuongHachToanSauDieuChinh { set; get; }
        public double SoGioLamViec { set; get; }
        public int SapXep { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenCoSo { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }

        [NotMapped]
        public double NgayHieuLucUnix { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem { get; set; }

        [NotMapped]
        public List<HRM_HopDongPhuCap> lstPhuCap { set; get; }
    }

    //public class HRM_QuyetDinhDieuChinhLuong_BoPhan : Audit
    //{
    //    public string IdQuyetDinh { get; set; }
    //    public long IdBoPhanEOS { get; set; }
    //    [MaxLength(5000)]
    //    public string NoiDung { get; set; }
    //    [NotMapped]
    //    public string TenBoPhan { get; set; }
    //    //[NotMapped]
    //    //public bool isDeleted { get; set; }
    //}
    public class HRM_TongHopDieuChinhLuong
    {
        public List<HRM_TongHopDieuChinhLuong_NhanVien> listNhanVien { get; set; }
        public int label { get; set; }
    }
    public class HRM_TongHopDieuChinhLuong_NhanVien
    {
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string TenCoSo { get; set; }
        public string TenBoPhan { get; set; }
        public string LoaiCong { get; set; }
        public DateTime? NgayBatDauLamViec { get; set; }
        public double ThamNien { get; set; }
        public List<HRM_TongHopDieuChinhLuong_ChiTiet> listChiTiet { get; set; }
    }
    public class HRM_TongHopDieuChinhLuong_ChiTiet
    {
        public double Luong { get; set; }
        public double PhuCap { get; set; }
        public DateTime? Ngay { get; set; }
        public string LyDo { get; set; }
    }
    public class HRM_TongHopDieuChinhLuong_label
    {
        public int SoLanDieuChinh { get; set; } = 0;
    }

    #endregion

    #region Quyết định khen thưởng

    public class HRM_QuyetDinhKhenThuong : AudiQuyTrinh
    {


        [MaxLength(150)]
        public string SoQuyetDinh { get; set; }
        public DateTime? NgayKy { get; set; }


        public long IdCongTyEOS { get; set; }
        [StringLength(36)]
        public string IdNguoiKy { get; set; }
        public long IdChucVuNguoiKy { set; get; }
        public long IdChucDanhNguoiKy { get; set; }
        [NotMapped]
        public string ChucDanhNguoiKy { get; set; }

        public DateTime? NgayHieuLuc { get; set; }
        public long IdBoPhanThucHien { get; set; }
        public double? SoTienKhenThuong { get; set; }
        public string KhenThuong { get; set; }
        public bool isTinhVaoLuong { get; set; }
        [StringLength(36)]
        public string IddmHinhThucKhenThuong { get; set; }
        public int Loai { get; set; } //1 khen thưởng nhân viên, 2 khen thưởng phòng ban
        public string IdBoPhan { set; get; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }

        [NotMapped]
        public double NgayKyUnix { get; set; }

        [NotMapped]
        public double NgayHieuLucUnix { get; set; }

        [NotMapped]
        public HRM_NhanVien NguoiKy { get; set; }

        public DateTime KyLuongThucHien { set; get; }
        [NotMapped]
        public double KyLuongThucHienUnix { set; get; }
        [NotMapped]
        public string TenCongty { set; get; }
        [NotMapped]
        public List<long> listIdBoPhan { set; get; }


        [NotMapped]
        public List<HRM_QuyetDinhKhenThuongNhanVien> lstNhanVien { get; set; }

        [NotMapped]
        public List<HRM_QuyetDinhKhenThuongBoPhan> lstBoPhan { get; set; }

    }
    public class HRM_TheoDoiThuongNhanVien : Audit
    {
        public long IdCongTyEOS { get; set; }
        public string IdNhanVien { get; set; }
        public bool isTinhVaoLuong { get; set; }
        public double SoTien { get; set; }
        public DateTime Ngay { get; set; }
    }
    public class HRM_QuyetDinhKhenThuongNhanVien : Audit
    {
        [StringLength(36)]
        public string IdQuyetDinhKhenThuong { get; set; }
        [StringLength(36)]
        public string IdNhanVien { get; set; }
        public long IdChucVuEOS { set; get; }
        public long IdChucDanhEOS { set; get; }
        public long IdBoPhan { set; get; }
        public double? PhanTramPhat { get; set; }
        [MaxLength(5000)]
        public string GhiChu { set; get; }
        public double SoTien { get; set; }
        public bool isTinhVaoLuong { get; set; }
        public bool isKetThuc { get; set; } = false;
        public DateTime? NgayHieuLuc { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }

        [NotMapped]
        public bool isDeleted { get; set; }

    }
    public class HRM_dmHinhThucKhenThuong : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }
    }
    public class HRM_QuyetDinhKhenThuongBoPhan : Audit
    {
        public string IdQuyetDinhKhenThuong { get; set; }
        public long IdBoPhanEOS { get; set; }
        public string NoiDung { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }
    #endregion

    #region Quyết định kỷ luật
    public class HRM_QuyetDinhKyLuat : AudiQuyTrinh
    {
        public long IdCongTyEOS { set; get; }

        [MaxLength(36)]
        public string IdNguoiKy { get; set; }
        public long IdChucDanhNguoiKy { get; set; }
        [NotMapped]
        public string ChucDanhNguoiKy { get; set; }
        public DateTime? NgayKy { get; set; }
        public DateTime? NgayHieuLuc { get; set; }
        public DateTime? ThoiGianXuLy { get; set; }
        [MaxLength(500)]
        public string NguyenNhan { get; set; }
        [MaxLength(500)]
        public string SoQuyetDinh { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(36)]
        public string IddmHinhThucKyLuat { get; set; }//edmHinhThucKyLuat
        [MaxLength(36)]
        public string TrangThai { get; set; }
        public int? LoaiHinhThucPhat { get; set; }// phantram hay ngaycong
        public double? PhanTramPhat { get; set; }// * tienluong
        public int? KyThucHien { get; set; }
        public int? ThangThucHien { get; set; }
        public long IdBoPhanThucHien { get; set; }

        public string IdBoPhan { set; get; }

        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public HRM_NhanVien NhanVien { get; set; }
        [NotMapped]
        public double NgayKyUnix { get; set; }
        [NotMapped]
        public HRM_NhanVien NguoiKy { get; set; }
        public DateTime KyLuongThucHien { set; get; }
        [NotMapped]
        public double KyLuongThucHienUnix { set; get; }
        [NotMapped]
        public double NgayHieuLucUnix { get; set; }
        [NotMapped]
        public double ThoiGianXuLyUnix { get; set; }
        [NotMapped]
        public List<long> listIdBoPhan { set; get; }
        [NotMapped]
        public List<HRM_QuyetDinhKyLuatBoPhan> lstBoPhan { get; set; }
        [NotMapped]
        public List<HRM_QuyetDinhKyLuatNhanVien> lstNhanVien { get; set; }
        [NotMapped]
        public string TendmHinhThucXuLy { get; set; }
        [NotMapped]
        public double? SoTienPhat { get; set; }// 
    }
    public class HRM_dmHinhThucKyLuat : Audit
    {
        [MaxLength(36)]
        public string Ma { get; set; }
        [MaxLength(500)]
        public string Ten { get; set; }
        [MaxLength(1500)]
        public string GhiChu { get; set; }
    }
    public class HRM_dmLoaiHinhThucPhatKyLuat : Audit
    {
        [MaxLength(36)]
        public string Ma { get; set; }
        [MaxLength(500)]
        public string Ten { get; set; }
        [MaxLength(1500)]
        public string GhiChu { get; set; }
    }
    public class HRM_QuyetDinhKyLuatBoPhan : Audit
    {
        [MaxLength(36)]
        public string IdQuyetDinhKyLuat { get; set; }
        public long IdBoPhanEOS { get; set; }
        [MaxLength(2500)]
        public string NoiDung { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }
    public class HRM_QuyetDinhKyLuatNhanVien : Audit
    {
        [MaxLength(36)]
        public string IdQuyetDinhKyLuat { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public double PhanTramPhat { get; set; }
        public int KyLuatLan { set; get; }
        public double SoTien { get; set; }
        [MaxLength(5000)]
        public string GhiChu { set; get; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public long IdBoPhan { set; get; }
        [NotMapped]
        public long IddmChucVu { set; get; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }
    #endregion

    #region Thôi việc


    public class HRM_QuyetDinhThoiViec : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string SoQuyetDinh { get; set; }
        public DateTime NgayKy { get; set; }
        [MaxLength(36)]
        public string IdNguoiKy { get; set; }
        public long IdChucDanhNguoiKy { get; set; }
        [NotMapped]
        public string ChucDanhNguoiKy { get; set; }
        public DateTime NgayHieuLuc { get; set; }

        public string TrangThai { get; set; }

        [NotMapped]
        public double NgayKyUnix { get; set; }

        [NotMapped]
        public double NgayHieuLucUnix { get; set; }

        [NotMapped]
        public HRM_NhanVien NguoiKy { get; set; }

        [NotMapped]
        public List<HDFile> lstFileDinhKemMoi { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }

        [NotMapped]
        public HRM_NhanVien NhanVien { get; set; }
    }

    public class QuyetDinhThoiViec : Audit
    {
        public string SoQuyetDinh { get; set; }
        public DateTime NgayKy { get; set; }
        public string NguoiKy { get; set; }
        public string NhanVien { get; set; }
        public string TrangThai { get; set; }
        public string IdNhanVien { get; set; }


    }


    #endregion

    #region Sa thải


    public class HRM_QuyetDinhSaThai : AudiQuyTrinh
    {
        public long IdCongTyEOS { get; set; }
        [StringLength(36)]
        public string IdNhanVien { get; set; }


        public string SoQuyetDinh { get; set; }
        public DateTime NgayKy { get; set; }
        [StringLength(36)]
        public string IdNguoiKy { get; set; }
        [NotMapped]
        public string TenNguoiKy { get; set; }
        public long IdChucVuNguoiKy { set; get; }
        [MaxLength(5000)]
        public string TroCapThoiViec { set; get; }
        public long IdChucDanhNguoiKy { get; set; }
        [NotMapped]
        public string ChucDanhNguoiKy { get; set; }
        public DateTime HopNgay { get; set; }
        public DateTime NgayHieuLuc { get; set; }
        public long IdChucVuNhanVien { set; get; }
        public long IdBoPhanNhanVien { set; get; }
        [StringLength(250)]
        public string TrangThai { get; set; }
        public DateTime GiaiQuyetDenNgay { get; set; }
        public DateTime BanGiaoTrongNgay { get; set; }
        [StringLength(250)]
        public string TenChucVu { set; get; }
        [StringLength(250)]
        public string TenBoPhan { set; get; }
        public double SoNgayPhepCon { set; get; }
        [NotMapped]
        public double NgayKyUnix { get; set; }
        [NotMapped]
        public double NgayHieuLucUnix { get; set; }
        [NotMapped]
        public double HopNgayUnix { get; set; }
        [NotMapped]
        public double GiaiQuyetDenNgayUnix { get; set; }

        [NotMapped]
        public double BanGiaoTrongNgayUnix { get; set; }
        [NotMapped]
        public HRM_NhanVien NguoiKy { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
        [NotMapped]
        public HRM_NhanVien NhanVien { get; set; }
        [NotMapped]
        public List<HRM_QuyetDinhSaThaiBoPhan> lstBoPhan { get; set; }
        [NotMapped]
        public List<HRM_SaThaiTaiSanBanGiao> lstTaiSanBanGiao { get; set; }
    }

    public class HRM_SaThaiTaiSanBanGiao : Audit
    {
        [MaxLength(36)]
        public string IddmTaiSan { set; get; }
        [NotMapped]
        public string TenTaiSanBanGiao { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinhSaThai { set; get; }
        [MaxLength(36)]
        public string IdHopDongTaiSan { set; get; }
        public DateTime NgayBanGiao { set; get; }
        [NotMapped]
        public double NgayBanGiaoUnix { set; get; }
        public double SoTienDaThu { set; get; }
        public double SoTienDuocHoan { set; get; }
        public double SoTienPhat { set; get; }
        [MaxLength(100)]
        public string TenNguoiTiepNhan { set; get; }
        [MaxLength(5000)]
        public string GhiChu { set; get; }


    }

    public class HRM_QuyetDinhSaThaiBoPhan : Audit
    {
        public string IdQuyetDinhSaThai { get; set; }
        public long IdBoPhanEOS { get; set; }
        public string NoiDung { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }

    #endregion



    #region Quá trình công tác

    public class HRM_QuaTrinhCongTacNhanVien : Audit
    {
        //Nhật ký
        public string IdNhanVien { get; set; }
        public DateTime NgayHieuLuc { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public string TableName { get; set; }
        public string IdTable { get; set; }
        public string MoTa { get; set; }
        public string LoaiNhatKy { get; set; }
        public string TenNguoiKy { get; set; }
        [NotMapped]
        public string TenLoaiNhatKyNhanVien { get { return DefineData.GetTenLoaiNhatKyNhanVien(this.LoaiNhatKy); } }
    }
    #endregion
    #region Nghi viêc
    public class HRM_QuyetDinhNghiViec : AudiQuyTrinh
    {

        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public string SoQuyetDinh { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdCongTyEOS { set; get; }
        [NotMapped]
        public string TenNhanVien { set; get; }
        [NotMapped]
        public string CCCD { set; get; }

        [MaxLength(36)]
        public string IdNguoiKy { set; get; }
        [NotMapped]
        public string TenNguoiKy { set; get; }

        public long IdChucVuNguoiKy { set; get; }
        [MaxLength(5000)]
        public string LyDoTheoDon { set; get; }
        [MaxLength(5000)]
        public string LyDoThucTe { set; get; }

        [MaxLength(5000)]
        public string NoiDungBanGiao { set; get; }
        public DateTime? NgayNghiViec { set; get; }
        [NotMapped]
        public double NgayNghiViecUnix { set; get; }
        public DateTime? NgayBanGiao { set; get; }
        [NotMapped]
        public double NgayBanGiaoUnix { set; get; }

        public DateTime? NgayHieuLuc { set; get; }
        [NotMapped]
        public double NgayHieuLucUnix { set; get; }

        public DateTime? NgayKy { set; get; }
        [NotMapped]
        public double NgayKyUnix { set; get; }

        public DateTime? NgayDuyetNghi { set; get; }
        public double NgayDuyetNghiUnix { set; get; }
        [MaxLength(36)]
        public string IdNguoiTiepNhan { set; get; }
        [NotMapped]
        public string TenNguoiTiepNhan { set; get; }
        [NotMapped]
        public string TenCongTy { set; get; }
        [NotMapped]
        public string TenBoPhan { set; get; }
        [NotMapped]
        public string TenChucDanh { set; get; }


        [NotMapped]
        public List<HRM_QuyetDinhNghiViecTaiSanBanGiao> lstTaiSanBanGiao { set; get; }

        [NotMapped]
        public List<HRM_QuyetDinhNghiViecNhanVienBoPhan> lstBoPhan { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
    }
    public class HRM_QuyetDinhNghiViecNhanVienBoPhan : Audit
    {
        [StringLength(36)]
        public string IdQuyetDinhKhenThuong { get; set; }
        [StringLength(36)]
        public string IdNhanVien { get; set; }
        public long IdChucVuEOS { set; get; }
        public long IdChucDanhEOS { set; get; }
        public long IdBoPhanEOS { set; get; }
        public double? PhanTramPhat { get; set; }
        [MaxLength(5000)]
        public string GhiChu { set; get; }
        public double SoTien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public bool isDeleted { get; set; }

    }
    public class HRM_QuyetDinhNghiViecTaiSanBanGiao : Audit
    {
        [MaxLength(36)]
        public string IddmTaiSan { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinhNghiViec { set; get; }
        [MaxLength(36)]
        public string IdHopDongTaiSan { set; get; }
        public DateTime NgayBanGiao { set; get; }
        [NotMapped]
        public double NgayBanGiaoUnix { set; get; }
        [NotMapped]
        public string TenTaiSanBanGiao { set; get; }

        public double SoTienDaThu { set; get; }
        public double SoTienDuocHoan { set; get; }
        public double SoTienPhat { set; get; }
        [MaxLength(100)]
        public string TenNguoiTiepNhan { set; get; }
        [MaxLength(5000)]
        public string GhiChu { set; get; }

    }
    #endregion

    #region Lương nhân viên (từ hợp đồng, quyết định điều chỉnh lương)
    public class HRM_Luong_NhanVien : Audit
    {

        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public int LoaiLuong { set; get; }// Theo Ngay, Giờ , Tháng 
        public long IdBoPhanEOS { set; get; }
        public double Luong { get; set; }
        public string HinhThucLuong { set; get; }
        //  public bool isDongBaoHiem { set; get; }
        public int KieuLuong { set; get; }// 0 Lương gross/ net 1
        public double LuongHachToan { get; set; } // lương bảo hiểm
        public double? LuongDanhGia { get; set; } // lương dánh giá hàng tháng
        public DateTime NgayBatDauHieuLuc { get; set; }
        public DateTime? NgayHetHieuLuc { get; set; }
        public long IdCongTyEOS { get; set; }
        public long IdDuAnEOS { get; set; }
        public string NguonNhapLuong { get; set; }//eNguonNhapLuong: Nhập tay, Hợp đồng, Quyết định điều chỉnh
        [MaxLength(36)]
        public string IdNguonNhapLuong { get; set; }
        public long IdNguonNhapLuongEOS { get; set; }
        [NotMapped]
        public List<HRM_Luong_PhuCap_NhanVien> listPhuCap = new List<HRM_Luong_PhuCap_NhanVien>();
        [NotMapped]
        public List<HRM_HopDongTaiSanBanGiao> listTaiSan = new List<HRM_HopDongTaiSanBanGiao>();

        public HRM_Luong_NhanVien()
        {

        }
        // public HRM_Luong_NhanVien Clone()
        //{

        //}
        public HRM_Luong_NhanVien Clone()
        {
            return (HRM_Luong_NhanVien)this.MemberwiseClone();
        }
        public HRM_Luong_NhanVien(HRM_HopDong model)
        {
            IdGuid = Guid.NewGuid().ToString();
            IdDuAnEOS = 0;
            IdNhanVien = model.IdNhanVien;
            IdNguonNhapLuong = model.IdGuid;
            if (model.LuongHopDong != null)
            {
                Luong = model.LuongHopDong.LuongBaoHiem * (model.LuongHopDong.TyLeHuongLuong == 0 ? 100 : model.LuongHopDong.TyLeHuongLuong) / 100;
                LuongDanhGia = model.LuongHopDong.LuongDanhGia;
                KieuLuong = model.LuongHopDong.KieuLuong;
            }
            if (model.LuongBaoHiem != null && model.BaoHiem != null && model.BaoHiem.isBaoHiemXaHoi == true)
                LuongHachToan = model.LuongBaoHiem.LuongBaoHiem;
            LoaiLuong = model.LoaiLuong;

            NguonNhapLuong = "HRM_HopDong";
            NgayBatDauHieuLuc = model.NgayHieuLuc;
            if (model.NgayHetHan != null && model.NgayHetHan.Value.Year > 2000)
                NgayHetHieuLuc = model.NgayHetHan;
        }
        public HRM_Luong_NhanVien(HRM_HopDong hopdong, HRM_QuyetDinhDieuChinhLuong item, DateTime _datetime)
        {
            IdGuid = Guid.NewGuid().ToString();
            IdCongTyEOS = item.IdCongTyEOS;
            ////IdDuAnEOS = item.IdDuAn;
            //IdNhanVien = item.IdNhanVien;
            //IdBoPhanEOS = item.IdBoPhanEOS;
            //Luong = item.LuongThucTeSauDieuChinh;
            LoaiLuong = hopdong.LoaiLuong;
            //KieuLuong = item.KieuLuong;
            //LuongHachToan = item.LuongHachToanSauDieuChinh;
            NgayBatDauHieuLuc = item.NgayHieuLuc.Value;
            NgayHetHieuLuc = _datetime;
            IdNguonNhapLuong = item.IdGuid;
            NguonNhapLuong = DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString();
        }
        public HRM_Luong_NhanVien(HRM_HopDong hopdong, HRM_QuyetDinhDieuChinhLuong_NhanVien item, DateTime? _datetime = null)
        {
            //IdGuid = Guid.NewGuid().ToString();
            IdCongTyEOS = item.IdCongTyEOS ?? 0;
            //IdDuAnEOS = item.IdDuAn;
            IdNhanVien = item.IdNhanVien;
            IdBoPhanEOS = item.IdBoPhanEOS ?? 0;
            Luong = item.LuongThucTeSauDieuChinh;
            if (hopdong != null)
                LoaiLuong = hopdong.LoaiLuong;
            KieuLuong = item.KieuLuong;
            LuongHachToan = item.LuongHachToanHienTai;
            if (item.LuongHachToanSauDieuChinh > 0)
                LuongHachToan = item.LuongHachToanSauDieuChinh;
            NgayBatDauHieuLuc = item.NgayHieuLuc.Value;
            NgayHetHieuLuc = _datetime;
            IdNguonNhapLuong = item.IdGuid;
            NguonNhapLuong = DefineData.eTable.HRM_QuyetDinhDieuChinhLuong.ToString();
        }

    }
    public class HRM_Luong_PhuCap_NhanVien : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(36)]
        public string IdLuong_NhanVien { get; set; }//HRM_Luong_NhanVien
        [MaxLength(36)]
        public string IddmLoaiPhuCap { get; set; }//HRM_dmPhuCap
        [NotMapped]
        public bool isTinhThueTNCN { get; set; }

        [NotMapped]
        public string TenPhuCap { get; set; }
        public double SoTien { get; set; }
        public DateTime? NgayBatDauHieuLuc { get; set; }
        [NotMapped]
        public double NgayBatDauHieuLucUnix { get; set; }
        public DateTime? NgayHetHieuLuc { get; set; }
        [NotMapped]
        public double NgayHetHieuLucUnix { get; set; }
        public long IdCongTyEOS { get; set; }
        public long IdDuAnEOS { get; set; }
        public string NguonNhapLuong { get; set; }//eNguonNhapLuong: Nhập tay, Hợp đồng, Quyết định điều chỉnh
        [MaxLength(36)]
        public string IdNguonNhapLuong { get; set; }
        public long IdNguonNhapLuongEOS { get; set; }
        public int LoaiPhuCap { set; get; }//0 - THEO_GIO, 1 - THEO_NGAY....
        public HRM_Luong_PhuCap_NhanVien()
        {

        }

        public HRM_Luong_PhuCap_NhanVien(HRM_HopDong model, HRM_HopDongPhuCap item, string _IdLuong_NhanVien, string _NguonNhapLuong, string Host)
        {
            if (item.NgayBatDau != null && item.NgayBatDau.Value.Year > 2000)
                NgayBatDauHieuLuc = item.NgayBatDau;
            else
                NgayBatDauHieuLuc = model.NgayHieuLuc;
            IddmLoaiPhuCap = item.IddmLoaiPhuCap;
            IdGuid = Guid.NewGuid().ToString();
            IdLuong_NhanVien = _IdLuong_NhanVien;
            IdNguonNhapLuong = model.IdGuid;
            LoaiPhuCap = item.LoaiPhuCap;
            IdNhanVien = model.IdNhanVien;
            SoTien = item.SoTien;
            if (Host == "PHT" && model.LuongHopDong != null && model.LuongHopDong.TyLeHuongLuong != null && model.LuongHopDong.TyLeHuongLuong > 0)
                SoTien = item.SoTien * model.LuongHopDong.TyLeHuongLuong / 100;
            if (item.NgayKetThuc != null && item.NgayKetThuc.Value.Year > 2000)
                NgayHetHieuLuc = item.NgayKetThuc;
            NguonNhapLuong = _NguonNhapLuong;
            IdCongTyEOS = model.IdCongTyEOS;
            if (model.NgayHetHan != null && model.NgayHetHan.Value.Year > 2000)
                NgayHetHieuLuc = model.NgayHetHan;
        }

        public HRM_Luong_PhuCap_NhanVien(HRM_QuyetDinhDieuChinhLuong model, HRM_HopDongPhuCap item, string _IdLuong_NhanVien, string _NguonNhapLuong)
        {
            NgayBatDauHieuLuc = item.NgayBatDau;
            IddmLoaiPhuCap = item.IddmLoaiPhuCap;
            IdGuid = Guid.NewGuid().ToString();
            IdLuong_NhanVien = _IdLuong_NhanVien;
            IdNguonNhapLuong = model.IdGuid;
            LoaiPhuCap = item.LoaiPhuCap;
            //IdNhanVien = model.IdNhanVien;
            SoTien = item.SoTienSauDieuChinh;
            //NgayHetHieuLuc = item.NgayKetThuc;
            NguonNhapLuong = _NguonNhapLuong;
            IdCongTyEOS = model.IdCongTyEOS;
            //IdDuAnEOS = model.IdDuAn;
        }
        public HRM_Luong_PhuCap_NhanVien(HRM_QuyetDinhDieuChinhLuong_NhanVien model, HRM_HopDongPhuCap item, string _IdLuong_NhanVien, string _NguonNhapLuong)
        {
            NgayBatDauHieuLuc = item.NgayBatDau;
            IddmLoaiPhuCap = item.IddmLoaiPhuCap;
            IdGuid = Guid.NewGuid().ToString();
            IdLuong_NhanVien = _IdLuong_NhanVien;
            IdNguonNhapLuong = model.IdGuid;
            LoaiPhuCap = item.LoaiPhuCap;
            IdNhanVien = model.IdNhanVien;
            SoTien = item.SoTienSauDieuChinh;
            //NgayHetHieuLuc = item.NgayKetThuc;
            NguonNhapLuong = _NguonNhapLuong;
            IdCongTyEOS = model.IdCongTyEOS ?? 0;
            //IdDuAnEOS = model.IdDuAn;
        }

    }
    #endregion

    #region Kế hoạch làm việc theo tuần
    public class HRM_KeHoachLamViecTuan : Audit
    {
        public long IdCongTy { set; get; }
        public string IdBoPhan { set; get; }
        public DateTime? TuNgay { set; get; }
        public DateTime? DenNgay { set; get; }
        public string SoQuyTrinh { set; get; }
        public string GhiChu { set; get; }
        public string NoiDung { set; get; }
        [StringLength(36)]
        public string IdTrangThai { get; set; }
        [MaxLength(500)]
        public string TenTrangThai { get; set; }
        public bool isKetThuc { get; set; }
        [NotMapped]
        public double TuNgayUnix { set; get; }

        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public List<long> listIdBoPhan { set; get; }

        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public double DenNgayUnix { set; get; }
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien> listNhanVien { set; get; }
        public HRM_KeHoachLamViecTuan() { }
        public HRM_KeHoachLamViecTuan(HRM_KeHoachLamViecTuanViewModel a)
        {
            IdGuid = a.IdGuid;
            IdCongTy = a.IdCongTy;
            //IdBoPhan = a.IdBoPhan;
            listIdBoPhan = a.listIdBoPhan;
            if (a.listIdBoPhan != null && a.listIdBoPhan.Count > 0)
                IdBoPhan = string.Join(",", a.listIdBoPhan);
            if (a.TuNgay != null)
            {
                TuNgayUnix = a.TuNgayUnix;
            }
            if (a.DenNgay != null)
            {
                DenNgayUnix = a.DenNgayUnix;
            }
            SoQuyTrinh = a.SoQuyTrinh;
            GhiChu = a.GhiChu;
            NoiDung = a.NoiDung;
            IdTrangThai = a.IdTrangThai;
            TenTrangThai = a.TenTrangThai;
            isKetThuc = a.isKetThuc;
            listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVien>();
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }

        public void DateTimeToUnix()
        {
            if (TuNgay != null)
                TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(TuNgay);
            if (DenNgay != null)
                DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(DenNgay);
            if (IdBoPhan != null)
            {
                listIdBoPhan = new List<long>();
                if (IdBoPhan.Contains(","))
                {
                    string[] list = IdBoPhan.Split(',');
                    foreach (var bp in list)
                        listIdBoPhan.Add(long.Parse(bp));
                }
                else listIdBoPhan.Add(long.Parse(IdBoPhan));
            }
        }
        public void UnixToDateTime()
        {
            if (TuNgayUnix > 0)
            {
                TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
                TuNgay = new DateTime(TuNgay.Value.Year, TuNgay.Value.Month, TuNgay.Value.Day, 0, 0, 0);
            }
            if (DenNgayUnix > 0)
            {
                DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
                DenNgay = new DateTime(DenNgay.Value.Year, DenNgay.Value.Month, DenNgay.Value.Day, 0, 0, 0);
            }
            if (listIdBoPhan != null && listIdBoPhan.Count > 0)
                IdBoPhan = string.Join(",", listIdBoPhan);
        }
    }

    public class HRM_QuyTrinhBoPhan_BoPhan : Audit
    {
        public string IdQuyTrinh { get; set; }
        public long IdBoPhan { get; set; }
        public string TableName { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

    }
    public class HRM_KeHoachLamViecTuan_NhanVien : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        [MaxLength(36)]
        public string IdKeHoachLamViecTuan { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(500)]
        public string IdLichLamViec { set; get; }   //List phân cách bởi dấu phẩy
        public string IdLichLamViecTangCa { set; get; }   //List phân cách bởi dấu phẩy
        public int SapXep { get; set; }
        public DateTime Ngay { set; get; }
        public double? SoNgayDaNghi { get; set; }
        [NotMapped]
        public string TenLichLamViec { set; get; }   //List phân cách bởi dấu phẩy
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }

        [NotMapped]
        public double NgayUnix { set; get; }
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaChinh { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaTangCuong { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();

        public void DateTimeToUnix()
        {
            if (Ngay != null)
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);


        }
        public void UnixToDateTime()
        {
            if (NgayUnix > 0)
                Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);


        }
        public HRM_KeHoachLamViecTuan_NhanVien()
        {

        }
        public HRM_KeHoachLamViecTuan_NhanVien(HRM_KeHoachLamViecTuan_NhanVienViewModel a, HRM_KeHoachLamViecTuan_ChiTietViewModel b)
        {
            IdNhanVien = a.IdNhanVien;
            if (b.listIddmCaLamViec != null && b.listIddmCaLamViec.Count > 0)
                IdLichLamViec = string.Join(",", b.listIddmCaLamViec);
            if (b.listIddmCaLamViecTangCa != null && b.listIddmCaLamViecTangCa.Count > 0)
                IdLichLamViecTangCa = string.Join(",", b.listIddmCaLamViecTangCa);
            Ngay = new DateTime(b.Year, b.Month, b.Day);
            NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);
            listCaChinh = b.listCaChinh;
            listCaTangCuong = b.listCaTangCuong;
            Created = DateTime.Now;
            IdBoPhan = a.IdBoPhan;
            Modified = DateTime.Now;
            MaNhanVien = a.MaNhanVien;
            TenNhanVien = a.TenNhanVien;
            SoNgayDaNghi = a.SoNgayDaNghi;
            TenChucVu = a.ChucVu;
            TenBoPhan = a.TenBoPhan;

        }
        public HRM_KeHoachLamViecTuan_NhanVien(HRM_KeHoachLamViecTuan_NhanVienViewModel a)
        {
            IdNhanVien = a.IdNhanVien;
            Created = DateTime.Now;
            IdBoPhan = a.IdBoPhan;
            Modified = DateTime.Now;
            MaNhanVien = a.MaNhanVien;
            TenNhanVien = a.TenNhanVien;
            SoNgayDaNghi = a.SoNgayDaNghi;
            TenChucVu = a.ChucVu;
            TenBoPhan = a.TenBoPhan;

        }

    }
    public class HRM_KeHoachLamViecTuan_NhanVien_ThoiGian : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        public string IdKeHoachTuan { get; set; }
        public bool isTheoDoi { get; set; } = false;
        public bool isKetThuc { get; set; }
        public string IdNhanVien { get; set; }
        public int Loai { get; set; } //0 kế hoạch tuần, 1 đăng ký làm thêm giờ
        public string IdCaLamViec { get; set; }
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public double? SoGioNghi { get; set; } = 0;

        public int SapXep { get; set; }
        public DateTime Ngay { set; get; }
        [NotMapped]
        public double NgayUnix { set; get; }
        [NotMapped]
        public double TuGioUnix { set; get; }
        [NotMapped]
        public double DenGioUnix { set; get; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string MaCaLamViec { get; set; }
        [NotMapped]
        public string TenCaLamViec { get; set; }

        public void DateTimeToUnix()
        {
            if (Ngay != null)
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);
            if (TuGio != null)
                TuGioUnix = dungchung.DateTimeToUnixTimeStamp(TuGio);
            if (DenGio != null)
                DenGioUnix = dungchung.DateTimeToUnixTimeStamp(DenGio);

        }
        public void UnixToDateTime()
        {
            if (NgayUnix > 0)
            {
                Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);
                Ngay = new DateTime(Ngay.Year, Ngay.Month, Ngay.Day, 0, 0, 0);
            }
            if (TuGioUnix > 0)
            {
                TuGio = dungchung.UnixTimeStampToDateTime(TuGioUnix);
                TuGio = new DateTime(TuGio.Value.Year, TuGio.Value.Month, TuGio.Value.Day, TuGio.Value.Hour, TuGio.Value.Minute, 0);
            }
            if (DenGioUnix > 0)
            {
                DenGio = dungchung.UnixTimeStampToDateTime(DenGioUnix);
                DenGio = new DateTime(DenGio.Value.Year, DenGio.Value.Month, DenGio.Value.Day, DenGio.Value.Hour, DenGio.Value.Minute, 0);
            }

        }
        public HRM_KeHoachLamViecTuan_NhanVien_ThoiGian() { }
        public HRM_KeHoachLamViecTuan_NhanVien_ThoiGian Clone()
        {
            return (HRM_KeHoachLamViecTuan_NhanVien_ThoiGian)this.MemberwiseClone();
        }
    }
    public class HRM_TheoDoiLichLamViecNhanVien : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(500)]
        public string IdLichLamViec { set; get; }   //List phân cách bởi dấu phẩy
        public string IdLichLamViecTangCa { set; get; }   //List phân cách bởi dấu phẩy
        public DateTime Ngay { set; get; }
        [NotMapped]
        public double TuGioUnix { set; get; }
        [NotMapped]
        public double DenGioUnix { set; get; }
        public bool isLamThemGio { get; set; } = false;
        public string LyDo { get; set; }
        [NotMapped]
        public string TenLichLamViec { set; get; }   //List phân cách bởi dấu phẩy
        [NotMapped]

        public string TenNhanVien { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public double NgayUnix { set; get; }
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaChinh { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaTangCuong { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();

        public void DateTimeToUnix()
        {
            if (Ngay != null)
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);

        }
        public void UnixToDateTime()
        {
            if (NgayUnix > 0)
                Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);


        }
        public HRM_TheoDoiLichLamViecNhanVien()
        {

        }
        public HRM_TheoDoiLichLamViecNhanVien(HRM_KeHoachLamViecTuan_NhanVien a)
        {

            IdCongTy = a.IdCongTy;
            IdBoPhan = a.IdBoPhan;
            IdNhanVien = a.IdNhanVien;
            IdLichLamViec = a.IdLichLamViec;
            IdLichLamViecTangCa = a.IdLichLamViecTangCa;
            Ngay = a.Ngay;

        }
        public HRM_TheoDoiLichLamViecNhanVien(HRM_TheoDoiLichLamViecNhanVien a, HRM_NhanVien b)
        {

            IdCongTy = a.IdCongTy;
            IdBoPhan = b.IdBoPhanEOS;
            IdNhanVien = a.IdNhanVien;
            TenNhanVien = b.TenDayDu;
            MaNhanVien = b.MaNhanVien;
            IdLichLamViecTangCa = a.IdLichLamViecTangCa;
            IdLichLamViec = a.IdLichLamViec;
            Ngay = a.Ngay;

        }

    }

    public class HRM_ThoiGianLamViecKeHoach : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public DateTime Ngay { set; get; }
        public DateTime GioVao { set; get; }
        public DateTime GioRa { set; get; }
        public bool isCaGay { get; set; }
        public bool isLamThemGio { get; set; } = false;
        public int Loai { set; get; } //0 kế hoạch tuần, 1 đăng ký làm thêm giờ
        public double HeSoTangCa { get; set; }
        public string LyDo { get; set; }
        public long IdCongTy { get; set; }
        public double? SoGioNghi { get; set; } = 0;
        public double GioTieuChuan { get; set; }// 8h, 9.6h, 10h
        public double NgayCongTieuChuan { get; set; }// 8h, 9.6h, 10h
        public string IddmCaLamViec { get; set; }// 8h, 9.6h, 10h
        public string IddmHeSoTangCa { get; set; }
        public double TongSoGioGiao { get; set; }
        public bool isCongTac { get; set; } = false;
        public string LyDoHuy { get; set; }
        public DateTime? NgayHuy { set; get; }

        [NotMapped]
        public DateTime? GioVaoTT { set; get; }
        [NotMapped]
        public DateTime? GioRaTT { set; get; }
        [NotMapped]
        public DateTime? GioVaoTiep { set; get; }
        [NotMapped]
        public DateTime? GioVaoTiepLT { set; get; }

        [NotMapped]
        public string TenCaLamViec { get; set; }
        [NotMapped]
        public string MaCaLamViec { get; set; }
        [NotMapped]
        public long IdCongTyVe { get; set; }
        [NotMapped]
        public bool isCheckGioVao { get; set; } = false;
        [NotMapped]
        public bool isCheckGioRa { get; set; } = false;
    }
    public class HRM_ThoiGianLamViecKeHoachView : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public DateTime Ngay { set; get; }
        public DateTime? GioVao { set; get; }
        public DateTime? GioRa { set; get; }
        public bool isCaGay { get; set; }
        public bool isLamThemGio { get; set; } = false;
        public int Loai { set; get; } //0 kế hoạch tuần, 1 đăng ký làm thêm giờ
        public double HeSoTangCa { get; set; }
        public string LyDo { get; set; }
        public long IdCongTy { get; set; }
        public double? SoGioNghi { get; set; } = 0;
        public double GioTieuChuan { get; set; }// 8h, 9.6h, 10h
        public double NgayCongTieuChuan { get; set; }// 8h, 9.6h, 10h
        public string IddmCaLamViec { get; set; }// 8h, 9.6h, 10h
        public string IddmHeSoTangCa { get; set; }
        public double TongSoGioGiao { get; set; }
        public bool isCongTac { get; set; } = false;
        [NotMapped]
        public DateTime? GioVaoTT { set; get; }
        [NotMapped]
        public DateTime? GioRaTT { set; get; }
        [NotMapped]
        public DateTime? GioVaoTiep { set; get; }
        [NotMapped]
        public DateTime? GioVaoTiepLT { set; get; }

        [NotMapped]
        public bool isCheckGioVao { get; set; } = false;
        [NotMapped]
        public bool isCheckGioRa { get; set; } = false;
    }

    public class HRM_LichLamViecTuanNhanVien : Audit
    {
        public int Thu { get; set; }
        public string IdNhanVien { get; set; }
        [MaxLength(500)]
        public string IdLichLamViec { set; get; }   //List phân cách bởi dấu phẩy

    }

    public class TimKiemKeHoachTuan
    {
        public List<string> listIdNhanVien { get; set; }
        public List<long> listIdBoPhan { get; set; }
        public double TuNgay { get; set; }
        public double DenNgay { get; set; }
        public long IdBoPhan { get; set; }
        public List<HRM_KeHoachLamViecTuan_NhanVienViewModel> listNhanVien { get; set; }
    }

    public class HRM_KeHoachLamViecTuanViewModel : Audit
    {
        public long IdCongTy { set; get; }

        public List<long> listIdBoPhan { set; get; }
        public DateTime? TuNgay { set; get; }
        public DateTime? DenNgay { set; get; }
        public string SoQuyTrinh { set; get; }
        public string GhiChu { set; get; }
        public string NoiDung { set; get; }
        [StringLength(36)]
        public string IdTrangThai { get; set; }
        [MaxLength(500)]
        public string TenTrangThai { get; set; }
        public bool isKetThuc { get; set; } = false;
        [NotMapped]
        public double TuNgayUnix { set; get; }
        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public double DenNgayUnix { set; get; }
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVienViewModel> listNhanVien { get; set; }
        public HRM_KeHoachLamViecTuanViewModel()
        {

        }
        public HRM_KeHoachLamViecTuanViewModel(HRM_KeHoachLamViecTuan a)
        {
            IdGuid = a.IdGuid;
            IdCongTy = a.IdCongTy;
            if (a.IdBoPhan != null)
            {
                listIdBoPhan = new List<long>();
                if (a.IdBoPhan.Contains(","))
                {
                    string[] list = a.IdBoPhan.Split(',');
                    foreach (var bp in list)
                        listIdBoPhan.Add(long.Parse(bp));
                }
                else listIdBoPhan.Add(long.Parse(a.IdBoPhan));
            }

            if (a.TuNgay != null)
            {
                TuNgay = a.TuNgay;
                TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(a.TuNgay);
            }
            if (a.DenNgay != null)
            {
                DenNgay = a.DenNgay;
                DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(a.DenNgay);
            }
            SoQuyTrinh = a.SoQuyTrinh;
            GhiChu = a.GhiChu;
            NoiDung = a.NoiDung;
            IdTrangThai = a.IdTrangThai;
            TenTrangThai = a.TenTrangThai;
            isKetThuc = a.isKetThuc;
            Created = a.Created;
        }
    }
    public class HRM_KeHoachLamViecTuan_NhanVienViewModel : AuditId
    {
        public string IdKeHoachLamViecTuan { set; get; }
        public string IdNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        public string TenBoPhan { get; set; }

        public long IdBoPhan { get; set; }
        public double? SoNgayDaNghi { get; set; }


        [NotMapped]
        public string IdUser { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string ChucVu { get; set; }
        //[NotMapped]
        //public string TenChucVu { get; set; }
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaChinh { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaTangCuong { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_ChiTietViewModel> listWeek { get; set; }
        public HRM_KeHoachLamViecTuan_NhanVienViewModel()
        {
        }
        public HRM_KeHoachLamViecTuan_NhanVienViewModel(HRM_KeHoachLamViecTuan_NhanVien a)
        {
            IdKeHoachLamViecTuan = a.IdKeHoachLamViecTuan;
            IdNhanVien = a.IdNhanVien;
            IdBoPhan = a.IdBoPhan;
            SoNgayDaNghi = a.SoNgayDaNghi;
            listCaChinh = a.listCaChinh;
            listCaTangCuong = a.listCaTangCuong;
        }
        public HRM_KeHoachLamViecTuan_NhanVienViewModel(HRM_TheoDoiLichLamViecNhanVien a)
        {
            IdNhanVien = a.IdNhanVien;
            TenNhanVien = a.TenNhanVien;
            MaNhanVien = a.MaNhanVien;
        }
    }
    public class HRM_KeHoachLamViecTuan_ChiTietViewModel : AuditId
    {

        public string IdKeHoachLamViecTuan { set; get; }
        public string Rank { set; get; }
        public int TypeRank { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdLichLamViecTangCa { get; set; }
        public string TenCaLamViec { get; set; }
        public string TenCaLamViecTangCuong { get; set; }
        public string Mau { get; set; }
        [NotMapped]
        public List<string> listIddmCaLamViec { get; set; }
        [NotMapped]
        public List<string> listIddmCaLamViecTangCa { get; set; }
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaChinh { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
        [NotMapped]
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaTangCuong { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();

        public HRM_KeHoachLamViecTuan_ChiTietViewModel()
        {
        }
        public HRM_KeHoachLamViecTuan_ChiTietViewModel(HRM_KeHoachLamViecTuan_NhanVien a)
        {
            IdKeHoachLamViecTuan = a.IdKeHoachLamViecTuan;
            Day = a.Ngay.Day;
            Month = a.Ngay.Month;
            Year = a.Ngay.Year;
            IdCaLamViec = a.IdLichLamViec;
            listCaChinh = a.listCaChinh;
            listCaTangCuong = a.listCaTangCuong;
            string ThuTA = a.Ngay.DayOfWeek.ToString();
            TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            Rank = DefineData.QuyDoiThu(TypeRank);
            if (TypeRank == 0)
                Mau = "#FF0000";
            else if (TypeRank == 6)
                Mau = "#0000FF";
        }
        public HRM_KeHoachLamViecTuan_ChiTietViewModel(HRM_BangChamCong_NhanVien_ThoiGian a)
        {
            Day = a.NgayChamCong.Value.Day;
            Month = a.NgayChamCong.Value.Month;
            Year = a.NgayChamCong.Value.Year;
            listIddmCaLamViec = new List<string>();
            TenCaLamViec = a.GioLamViec;
            string ThuTA = a.NgayChamCong.Value.DayOfWeek.ToString();
            TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            Rank = DefineData.QuyDoiThu(TypeRank);
        }

        public HRM_KeHoachLamViecTuan_ChiTietViewModel(HRM_KeHoachLamViecTuan_NhanVien a, List<Tu_DenNgay> listNgayNghi)
        {
            IdKeHoachLamViecTuan = a.IdKeHoachLamViecTuan;
            Day = a.Ngay.Day;
            Month = a.Ngay.Month;
            Year = a.Ngay.Year;
            IdCaLamViec = a.IdLichLamViec;
            IdLichLamViecTangCa = a.IdLichLamViecTangCa;
            listIddmCaLamViec = new List<string>();
            if (!string.IsNullOrEmpty(a.IdLichLamViec))
            {
                string[] list = a.IdLichLamViec.Split(',');
                foreach (var item in list)
                    listIddmCaLamViec.Add(item);
            }
            listIddmCaLamViecTangCa = new List<string>();
            if (!string.IsNullOrEmpty(a.IdLichLamViecTangCa))
            {
                string[] list = a.IdLichLamViecTangCa.Split(',');
                foreach (var item in list)
                    listIddmCaLamViecTangCa.Add(item);
            }
            string ThuTA = a.Ngay.DayOfWeek.ToString();
            TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            Rank = DefineData.QuyDoiThu(TypeRank);
            if (TypeRank == 0)
                Mau = "#FF0000";
            else if (TypeRank == 6)
                Mau = "#0000FF";
            if (listNgayNghi.Count > 0)
            {
                var ngayCheck = listNgayNghi.FirstOrDefault(c => c.TuNgay <= a.Ngay && c.DenNgay >= a.Ngay);
                if (ngayCheck != null)
                    Mau = "#FF0000";
            }
        }

        public HRM_KeHoachLamViecTuan_ChiTietViewModel(HRM_TheoDoiLichLamViecNhanVien a)
        {

            Day = a.Ngay.Day;
            Month = a.Ngay.Month;
            IdCaLamViec = a.IdLichLamViec;
            IdLichLamViecTangCa = a.IdLichLamViecTangCa;
            listIddmCaLamViec = new List<string>();
            if (!string.IsNullOrEmpty(a.IdLichLamViec))
            {
                string[] list = a.IdLichLamViec.Split(',');
                foreach (var item in list)
                    listIddmCaLamViec.Add(item);
            }
            listIddmCaLamViecTangCa = new List<string>();
            if (!string.IsNullOrEmpty(a.IdLichLamViecTangCa))
            {
                string[] list = a.IdLichLamViecTangCa.Split(',');
                foreach (var item in list)
                    listIddmCaLamViecTangCa.Add(item);
            }

            string ThuTA = a.Ngay.DayOfWeek.ToString();
            TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            Rank = DefineData.QuyDoiThuVT(TypeRank);
        }

    }

    public class ThoiDoiLichLamViecNhanVien_Label
    {
        public string label { get; set; }
        public string Thu { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class ThoiDoiLichLamViecNhanVienViewModel
    {
        public List<ThoiDoiLichLamViecNhanVien_Label> listLabel { get; set; }
        public List<HRM_KeHoachLamViecTuan_NhanVienViewModel> listNhanVien { get; set; }
    }
    public class HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel
    {
        public string IdKeHoachLamViecTuan { get; set; }
        public string IdNhanVien { get; set; }
        public string IdNhanVienThayThe { get; set; }
        public long IdCongTy { set; get; }
        public long IdBoPhan { get; set; }
        public long IdCongTyDieuDongDen { set; get; }
        public long IdBoPhanDieuDongDen { get; set; }
        public bool isCaTangCuong { get; set; }
        public DateTime? Ngay { get; set; }
        public double? NgayUnix { get; set; }
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaChinh { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
        public List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listCaTangCuong { get; set; } = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
    }
    #endregion

    #region Xác nhận đi muộn về sớm
    public class HRM_XacNhanDiMuonVeSom : AudiQuyTrinh
    {
        public long IdCongTy { set; get; }
        public string IdBoPhan { get; set; }
        public DateTime? TuNgay { set; get; }
        public DateTime? DenNgay { set; get; }


        [NotMapped]
        public List<string> listNoiDung { get; set; }
        [NotMapped]
        public double TuNgayUnix { set; get; }

        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
        [NotMapped]
        public double DenNgayUnix { set; get; }
        [NotMapped]
        public List<HRM_XacNhanDiMuonVeSom_NhanVien> listNhanVien { set; get; }
        public HRM_XacNhanDiMuonVeSom() { }
        public HRM_XacNhanDiMuonVeSom(HRM_XacNhanDiMuonVeSomViewModel a)
        {
            IdGuid = a.IdGuid;
            IdCongTy = a.IdCongTy;
            if (a.listIdBoPhan != null && a.listIdBoPhan.Count > 0)
                IdBoPhan = string.Join(",", a.listIdBoPhan);

            if (a.TuNgay != null)
            {
                TuNgayUnix = a.TuNgayUnix;
            }
            if (a.DenNgay != null)
            {
                DenNgayUnix = a.DenNgayUnix;
            }
            SoQuyTrinh = a.SoQuyTrinh;
            GhiChu = a.GhiChu;
            NoiDung = a.NoiDung;
            IdTrangThai = a.IdTrangThai;
            TenTrangThai = a.TenTrangThai;
            isKetThuc = a.isKetThuc;
            listNhanVien = new List<HRM_XacNhanDiMuonVeSom_NhanVien>();
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }

        public void DateTimeToUnix()
        {
            if (TuNgay != null)
                TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(TuNgay);
            if (DenNgay != null)
                DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(DenNgay);

        }
        public void UnixToDateTime()
        {

            if (TuNgayUnix > 0)
                TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
            if (DenNgayUnix > 0)
                DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);

        }
    }

    public class HRM_XacNhanDiMuonVeSomViewModel : AudiQuyTrinh
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { get; set; }
        public DateTime? TuNgay { set; get; }
        public DateTime? DenNgay { set; get; }

        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
        [NotMapped]
        public double TuNgayUnix { set; get; }
        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public double DenNgayUnix { set; get; }
        [NotMapped]
        public List<HRM_XacNhanDiMuonVeSom_NhanVienViewModel> listNhanVien { get; set; }
        public HRM_XacNhanDiMuonVeSomViewModel()
        {

        }
        public HRM_XacNhanDiMuonVeSomViewModel(HRM_XacNhanDiMuonVeSom a)
        {
            IdGuid = a.IdGuid;
            IdCongTy = a.IdCongTy;
            listIdBoPhan = new List<long>();
            if (!string.IsNullOrEmpty(a.IdBoPhan))
            {
                if (a.IdBoPhan.Contains(","))
                {
                    string[] list = a.IdBoPhan.Split(',');
                    foreach (var item in list)
                        listIdBoPhan.Add(long.Parse(item));
                }
                else
                    listIdBoPhan.Add(long.Parse(a.IdBoPhan));
            }
            if (a.TuNgay != null)
            {
                TuNgay = a.TuNgay;
                TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(a.TuNgay);
            }
            if (a.DenNgay != null)
            {
                DenNgay = a.DenNgay;
                DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(a.DenNgay);
            }
            eAction = DefineData.eAction.XACNHANDIMUONVESOM.ToString();
            SoQuyTrinh = a.SoQuyTrinh;
            GhiChu = a.GhiChu;
            NoiDung = a.NoiDung;
            IdTrangThai = a.IdTrangThai;
            TenTrangThai = a.TenTrangThai;
            isKetThuc = a.isKetThuc;
        }
    }

    public class HRM_XacNhanDiMuonVeSom_NhanVien : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        [MaxLength(36)]
        public string IdXacNhanDiMuonVeSom { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(36)]
        public string IddmKyHieuChamCong { get; set; }
        public DateTime Ngay { set; get; }
        public int SoPhut { get; set; }
        public string NoiDung { get; set; }
        public int SoPhutChapNhan { get; set; }
        [NotMapped]
        public string KyHieuChamCong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public double NgayUnix { set; get; }
        public void DateTimeToUnix()
        {
            if (Ngay != null)
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);
        }
        public void UnixToDateTime()
        {
            if (NgayUnix > 0)
                Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);
        }
        public HRM_XacNhanDiMuonVeSom_NhanVien()
        {

        }
        public HRM_XacNhanDiMuonVeSom_NhanVien(HRM_XacNhanDiMuonVeSom_NhanVienViewModel a, HRM_XacNhanDiMuonVeSom_ChiTietViewModel b)
        {
            IdNhanVien = a.IdNhanVien;
            //if (b.listIddmCaLamViec != null && b.listIddmCaLamViec.Count > 0)
            //    IdLichLamViec = string.Join(",", b.listIddmCaLamViec);
            //if (b.listIddmCaLamViecTangCa != null && b.listIddmCaLamViecTangCa.Count > 0)
            //    IdLichLamViecTangCa = string.Join(",", b.listIddmCaLamViecTangCa);
            Ngay = new DateTime(b.Year, b.Month, b.Day);
            NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);
            Created = DateTime.Now;
            Modified = DateTime.Now;
            MaNhanVien = a.MaNhanVien;
            TenNhanVien = a.TenNhanVien;
        }
    }

    public class HRM_XacNhanDiMuonVeSom_NhanVienViewModel
    {
        public string IdXacNhanDiMuonVeSom { set; get; }
        public long IdBoPhan { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmKyHieuChamCong { get; set; }
        public string TenNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public string ChucVu { get; set; }
        public string TenChucDanh { get; set; }
        public string TenBoPhan { get; set; }
        public int SoPhut { get; set; }
        public int SoPhutChapNhan { get; set; }
        public List<HRM_XacNhanDiMuonVeSom_ChiTietViewModel> listWeek { get; set; }
        public HRM_XacNhanDiMuonVeSom_NhanVienViewModel()
        {
        }
        public HRM_XacNhanDiMuonVeSom_NhanVienViewModel(HRM_XacNhanDiMuonVeSom_NhanVien a)
        {
            IdXacNhanDiMuonVeSom = a.IdXacNhanDiMuonVeSom;
            IdNhanVien = a.IdNhanVien;
        }
        //public HRM_XacNhanDiMuonVeSom_NhanVienViewModel(HRM_TheoDoiLichLamViecNhanVien a)
        //{
        //    IdNhanVien = a.IdNhanVien;
        //    TenNhanVien = a.TenNhanVien;
        //    MaNhanVien = a.MaNhanVien;
        //}
    }

    public class HRM_XacNhanDiMuonVeSom_ChiTietViewModel : AuditId
    {
        public string IdXacNhanDiMuonVeSom { set; get; }
        public string Rank { set; get; }
        public int SoPhut { get; set; }
        public int SoPhutChapNhan { get; set; }
        public int TypeRank { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string IddmKyHieuChamCong { get; set; }
        public DateTime Ngay { get; set; }
        public string Mau { get; set; }
        [NotMapped]
        public List<string> listIddmKyHieuChamCong { get; set; }
        [NotMapped]
        public List<HRM_XacNhanDiMuonVeSom_NhanVien> listChiTiet { get; set; }

        public HRM_XacNhanDiMuonVeSom_ChiTietViewModel()
        {
        }

        public HRM_XacNhanDiMuonVeSom_ChiTietViewModel(DateTime dtNgay, List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs, List<clsDiMuonVeSom> listDiSomVeMuon)
        {

            Month = dtNgay.Month;
            Day = dtNgay.Day;
            Year = dtNgay.Year;
            Ngay = dtNgay;
            //SoPhut = a.SoPhut;
            string ThuTA = dtNgay.DayOfWeek.ToString();
            TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            Rank = DefineData.QuyDoiThu(TypeRank);
            listIddmKyHieuChamCong = new List<string>();
            listChiTiet = new List<HRM_XacNhanDiMuonVeSom_NhanVien>();
            foreach (var item in listDiSomVeMuon)
            {
                HRM_XacNhanDiMuonVeSom_NhanVien itemChiTiet = new HRM_XacNhanDiMuonVeSom_NhanVien();

                HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.IdGuid == item.IddmKyHieuChamCong);
                if (dmKyHieuChamCong != null)
                {
                    if (item.SoPhut == 0)
                    {
                        IddmKyHieuChamCong += dmKyHieuChamCong.Ma + "; ";
                        itemChiTiet.NoiDung = dmKyHieuChamCong.Ten + "; ";

                    }
                    else
                    {
                        IddmKyHieuChamCong += dmKyHieuChamCong.Ma + ":" + item.SoPhut + "; ";
                        itemChiTiet.NoiDung = dmKyHieuChamCong.Ten;
                    }
                    itemChiTiet.KyHieuChamCong = dmKyHieuChamCong.Ma;
                    itemChiTiet.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                    itemChiTiet.SoPhut = item.SoPhut;
                    itemChiTiet.SoPhutChapNhan = item.SoPhut;

                }
                itemChiTiet.Ngay = item.Ngay;
                itemChiTiet.NgayUnix = dungchung.DateTimeToUnixTimeStamp(item.Ngay);
                itemChiTiet.IdNhanVien = item.IdNhanVien;
                listChiTiet.Add(itemChiTiet);
                listIddmKyHieuChamCong.Add(item.IddmKyHieuChamCong);

            }

        }
        public HRM_XacNhanDiMuonVeSom_ChiTietViewModel(HRM_XacNhanDiMuonVeSom_NhanVien a)
        {
            //IdXacNhanDiMuonVeSom = a.IdXacNhanDiMuonVeSom;
            //Day = a.Ngay.Day;
            //Month = a.Ngay.Month;
            //Year = a.Ngay.Year;
            //IddmKyHieuChamCong = a.IddmKyHieuChamCong;
            //listIddmKyHieuChamCong = new List<string>();
            //if (!string.IsNullOrEmpty(a.IddmKyHieuChamCong))
            //{
            //    string[] list = a.IdLichLamViec.Split(',');
            //    foreach (var item in list)
            //        listIddmCaLamViec.Add(item);
            //}
            //listIddmCaLamViecTangCa = new List<string>();
            //if (!string.IsNullOrEmpty(a.IdLichLamViecTangCa))
            //{
            //    string[] list = a.IdLichLamViecTangCa.Split(',');
            //    foreach (var item in list)
            //        listIddmCaLamViecTangCa.Add(item);
            //}

            //string ThuTA = a.Ngay.DayOfWeek.ToString();
            //TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            //Rank = DefineData.QuyDoiThu(TypeRank);
        }
        public HRM_XacNhanDiMuonVeSom_ChiTietViewModel(HRM_XacNhanDiMuonVeSom_NhanVien a, List<Tu_DenNgay> listNgayNghi)
        {
            //IdXacNhanDiMuonVeSom = a.IdXacNhanDiMuonVeSom;
            //Day = a.Ngay.Day;
            //Month = a.Ngay.Month;
            //Year = a.Ngay.Year;
            //IdCaLamViec = a.IdLichLamViec;
            //listIddmCaLamViec = new List<string>();
            //if (!string.IsNullOrEmpty(a.IdLichLamViec))
            //{
            //    string[] list = a.IdLichLamViec.Split(',');
            //    foreach (var item in list)
            //        listIddmCaLamViec.Add(item);
            //}
            //listIddmCaLamViecTangCa = new List<string>();
            //if (!string.IsNullOrEmpty(a.IdLichLamViecTangCa))
            //{
            //    string[] list = a.IdLichLamViecTangCa.Split(',');
            //    foreach (var item in list)
            //        listIddmCaLamViecTangCa.Add(item);
            //}
            //string ThuTA = a.Ngay.DayOfWeek.ToString();
            //TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
            //Rank = DefineData.QuyDoiThu(TypeRank);
            //if (TypeRank == 0)
            //    Mau = "#FF0000";
            //else if (TypeRank == 6)
            //    Mau = "#0000FF";
            //if (listNgayNghi.Count > 0)
            //{
            //    var ngayCheck = listNgayNghi.FirstOrDefault(c => c.TuNgay <= a.Ngay && c.DenNgay >= a.Ngay);
            //    if (ngayCheck != null)
            //        Mau = "#FF0000";
            //}
        }
    }

    public class HRM_CheckInOut : AuditId
    {
        public string Loai { set; get; } //I, O
        public DateTime Ngay { set; get; }
        public DateTime Gio { set; get; }
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        public string IdNhanVien { set; get; }
        public string IdUser { set; get; }
        public bool isChamCongHo { get; set; } = false;
        [NotMapped]
        public long IdDuAn { set; get; }

        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public double NgayUnix { set; get; }
        [NotMapped]
        public bool isCheckGio { get; set; } = false;
        public void DateTimeToUnix()
        {
            //if (Ngay != null)
            //    NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);
            if (Gio != null)
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Gio);
        }
        public void UnixToDateTime()
        {
            if (NgayUnix > 0)
            {
                Gio = dungchung.UnixTimeStampToDateTime(NgayUnix);
            }
            Ngay = new DateTime(Gio.Year, Gio.Month, Gio.Day, 0, 0, 0);
        }
    }
    #endregion
    public class HRM_BaoCaoCongViec_NhanVien : Audit
    {
        public long? IdCongTy { get; set; }
        public long? IdBoPhan { get; set; }
        public int Nam { get; set; }
        public int Thang { set; get; }
        public string Ma { get; set; }
        public string HoTen { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public List<HRM_HeaderCongViec> listHeader { get; set; } = new List<HRM_HeaderCongViec>();
        [NotMapped]
        public List<HRM_BaoCaoCongViec_Item> listHeaderData { get; set; } = new List<HRM_BaoCaoCongViec_Item>();

        [NotMapped]
        public List<ReturnData<HRM_BaoCaoCongViec_ItemView>> listNoiDung { get; set; } = new List<ReturnData<HRM_BaoCaoCongViec_ItemView>>();
        public HRM_BaoCaoCongViec_NhanVien Clone()
        {
            return (HRM_BaoCaoCongViec_NhanVien)this.MemberwiseClone();
        }
    }
    public class HRM_HeaderCongViec
    {
        public int Header { get; set; }
        public List<HRM_BaoCaoCongViec_Item> listHeader { get; set; }
    }
    public class HRM_BaoCaoCongViec_Item : Audit
    {
        [MaxLength(36)]
        public string IdBaoCao { get; set; }
        public long? IdCongTy { get; set; }
        public long? IdBoPhan { get; set; }
        public int Nam { get; set; }
        public int Thang { set; get; }
        public string STT { get; set; }
        public string STT_Cha { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string NoiDung { get; set; }
        public double? GiaTri { get; set; }
        public string CongThuc { get; set; }
        public string Type { get; set; }
        public string IdKey { get; set; }
        public int Header { get; set; }
        public bool isHeader { get; set; } = false;
        public string IdParent { get; set; }
        public int SapXep { get; set; }
        public string STT_Root { get; set; }
        [NotMapped]
        public int? rowspan { set; get; }
        [NotMapped]
        public int? colspan { set; get; }
        public HRM_BaoCaoCongViec_Item() { }
        public HRM_BaoCaoCongViec_Item Clone()
        {
            return (HRM_BaoCaoCongViec_Item)this.MemberwiseClone();
        }
        public HRM_BaoCaoCongViec_Item(HRM_dmThietLapBaoCaoCongViec_ChiTiet a)
        {
            Ma = a.Ma;
            Ten = a.Ten;
            SapXep = a.SapXep;
            rowspan = a.rowspan;
            colspan = a.colspan;
            Type = a.Type;
            CongThuc = a.CongThuc;
            IdGuid = a.IdGuid;
            IdParent = a.IdParent;
        }
    }
    public class HRM_BaoCaoCongViec_ItemView
    {

        public string STT { get; set; }
        public string STT_Cha { get; set; }
        public string STT_Root { get; set; }
        public List<HRM_BaoCaoCongViec_Item> listGiaTri { get; set; }
    }

    #region xác nhận làm thêm giờ
    public class HRM_QuyTrinhXacNhanNgoaiGio : AudiQuyTrinh
    {
        public long IdCongTy { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public List<string> listNoiDung { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVien> listNhanVien { get; set; }

    }
    public class HRM_QuyTrinhXacNhanNgoaiGio_NhanVien : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public double TongGioCong { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public double NgoaiGioDuocDuyet { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public string SoDienThoai { get; set; }
        [NotMapped]
        public string CCCD { get; set; }
        [NotMapped]
        public string DiaChi { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec> listCaLamViec { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio> listNgay { get; set; }


    }
    public class HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public DateTime? Ngay { get; set; }
        public double Gio { get; set; }

        [NotMapped]
        public double NgayUnix { get; set; }


    }
    public class HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmCaLamViec { get; set; }
        public int SapXep { get; set; }
        public double SoGio { get; set; }
        public double HeSo { get; set; }
        public double GioTieuChuan { get; set; }

        public double CongTieuChuan { get; set; }
        [NotMapped]
        public string TendmCaLamViec { get; set; }


    }
    public class HRM_TongHopXacNhanNgoaiGio : Audit
    {
        public string IdNhanVien { get; set; }
        public string IddmCaLamViec { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public double SoGio { get; set; }

        public double HeSo { get; set; }
        public double GioTieuChuan { get; set; }

        public double CongTieuChuan { get; set; }
    }


    #endregion

    #region Đánh giá nhân viên
    public class HRM_DanhGiaNhanVien : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        public DateTime? Ngay { set; get; }
        public string SoQuyTrinh { set; get; }
        public string GhiChu { set; get; }
        public string NoiDung { set; get; }
        [StringLength(36)]
        public string IdTrangThai { get; set; }
        [StringLength(36)]
        public string IdThuVienDanhGia { get; set; }
        [MaxLength(500)]
        public string TenTrangThai { get; set; }
        public bool isKetThuc { get; set; }
        public bool isCoSo { get; set; }
        [NotMapped]
        public double NgayUnix { set; get; }

        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenTieuChi { get; set; }


        [NotMapped]
        public List<HRM_DanhGiaNhanVien_Item> listNhanVien { set; get; }
        [NotMapped]
        public List<clsTieuChiDanhGia_HeaderRow> listHeader { get; set; } = new List<clsTieuChiDanhGia_HeaderRow>();
        [NotMapped]
        public List<clsTieuChiDanhGia_Data> listData { get; set; } = new List<clsTieuChiDanhGia_Data>();
        [NotMapped]
        public List<clsTieuChiDanhGia_Data_Item> listItem { get; set; }

        public HRM_DanhGiaNhanVien() { }


        public void DateTimeToUnix()
        {
            if (Ngay != null)
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);
        }
        public void UnixToDateTime()
        {
            if (NgayUnix > 0)
                Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);
        }
    }

    public class HRM_DanhGiaNhanVien_Item : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        [MaxLength(36)]
        public string IdDanhGiaNhanVien { set; get; }
        public int SapXep { get; set; }
        public double? Diem { set; get; } = 0;
        //public double? DiemTeamDG { set; get; }
        public double? DiemToiDa { set; get; }
        public string GhiChu { set; get; }
        public DateTime? Ngay { get; set; }
        public bool? isCoSo { get; set; }
        public bool isKetThuc { get; set; }
        public string IdUser { get; set; }
        public string BoPhanDanhGia { get; set; }
        public double? TyTrongQLCS { get; set; } = 0;
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(36)]
        public string IdThuVienTieuChiDanhGia { set; get; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string ChucVu { get; set; }
        public HRM_DanhGiaNhanVien_Item()
        {

        }
        public HRM_DanhGiaNhanVien_Item(clsTieuChiDanhGia_Data_Item a)
        {
            Diem = a.GiaTri;
            GhiChu = a.GhiChu;
            //DiemTeamDG = a.GiaTriTeamDanhGia;
            IdThuVienTieuChiDanhGia = a.IdThuVienTieuChiDanhGia;
            DiemToiDa = a.DiemToiDa;
        }

    }
    public class HRM_TongHopDanhGiaNhanVienThang
    {
        public List<clsTieuChiDanhGia_HeaderRow> listHeader { get; set; }
        public List<HRM_TongHopDanhGiaNhanVienThang_NhanVien> listNhanVien { get; set; }

        public List<clsTieuChiDanhGia_Data_Item> listItem { get; set; }
    }
    public class HRM_TongHopDanhGiaNhanVienThang_NhanVien
    {
        public string TenNhanVien { get; set; }
        public string TenChucDanh { get; set; }
        public string TenBoPhan { get; set; }
        public string MaNhanVien { get; set; }
        public List<HRM_TongHopDanhGiaNhanVienThang_TieuChi> listTieuChi { get; set; }
    }
    public class HRM_TongHopDanhGiaNhanVienThang_TieuChi
    {
        public string IdThuVienTieuChiDanhGia { get; set; }
        public string TenTieuChi { get; set; }
        public string TenCSDG { get; set; } = "CS";
        public string TenTeamDG { get; set; }
        public string Color { get; set; }
        public double? DiemCS { get; set; } = 0;
        public double? DiemTeam { get; set; }
        public List<HRM_TongHopDanhGiaNhanVienThang_Diem> listDiem { get; set; }
    }
    public class HRM_TongHopDanhGiaNhanVienThang_Diem
    {
        public string Ten { get; set; } = "CS";
        public string Color { get; set; }
        public double? Diem { get; set; } = 0;

    }

    public class HRM_BaoCaoKetQuaKPI
    {
        public string ViTri { get; set; }
        public string TenChucDanh { get; set; }
        public string TenBoPhan { get; set; }
        public string TenCoSo { get; set; }
        public string MaNhanVien { get; set; }
        public string IdNhanVien { get; set; }
        public string HoVaTen { get; set; }
        public string NgayBatDau { get; set; }
        public List<HRM_BaoCaoKetQuaKPI_ChiTiet> listChiTiet { get; set; }

    }
    public class HRM_BaoCaoKetQuaKPI_Item
    {
        public int Thang { get; set; }
        public double? Diem { get; set; } = 0;
        public string DanhGia { get; set; }
    }

    public class HRM_BaoCaoKetQuaKPI_ChiTiet
    {
        public string Ten { get; set; }
        public List<HRM_BaoCaoKetQuaKPI_Item> listThang { get; set; }
    }


    public class HRM_BaoCaoTongQuanDanhGiaNhanVien
    {
        public string BoPhan { get; set; }
        public int? SLTrungTuyen { get; set; }
        public double? TyLeTrungTuyen { get; set; }
        public int? SLCTru { get; set; }
        public double? TyLeCTru { get; set; }
        public int? SLC { get; set; }
        public double? TyLeC { get; set; }
        public int? SLBTru { get; set; }
        public double? TyLeBTru { get; set; }
        public int? SLB { get; set; }
        public double? TyLeB { get; set; }
        public int? SLATru { get; set; }
        public double? TyLeATru { get; set; }
        public int? SLA { get; set; }
        public double? TyLeA { get; set; }
        public int? SLACong { get; set; }
        public double? TyLeACong { get; set; }

    }
    #endregion


    #region Đánh giá nhân viên new
    public class HRM_QuyTrinhDanhGia : AudiQuyTrinh
    {
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public DateTime NgayDanhGia { get; set; }
        public int Nam { get; set; }
        public int Thang { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public double NgayDanhGiaUnix { get; set; }
        [NotMapped]
        public List<string> listNoiDung { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhDanhGia_NhanVien> listNhanVien { get; set; }
    }
    public class HRM_QuyTrinhDanhGia_NhanVien : Audit
    {
        public int SapXep { get; set; }
        [MaxLength(36)]
        public string IdQuyTrinh { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string KetQuaTheoNgayCong { get; set; }

        public string KetQuaThucTe { get; set; }
        public string GhiChu { get; set; }
        public string TenCongTy { get; set; }
        public int Nam { get; set; }
        public int Thang { get; set; }
        public bool isKetThuc { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public DateTime NgaySinh { get; set; }

    }

    #endregion
}







