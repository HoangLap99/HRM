﻿#nullable disable
using HRM.Api.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HRM.Api.Models
{
    #region Tuyển dụng New
    public class HRM_TuyenDung_NhuCau : AudiQuyTrinh
    {
        [MaxLength(50)]
        public string SoPhieu { get; set; }
        public long IdCongTyEOS { set; get; }

        public DateTime? Ngay { get; set; }

        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public double NgayUnix { get; set; }
        [NotMapped]
        public List<HRM_TuyenDung_NhuCau_ChiTiet> listChiTiet { get; set; }
    }

    public class HRM_TuyenDung_NhuCau_ChiTiet : Audit
    {
        public long IdCongTyEOS { set; get; }
        public long IdBoPhanEOS { get; set; }
        public long IdChucDanh { get; set; }
        [MaxLength(36)]
        public string IdTuyenDungNhuCau { set; get; }
        public string IddmLichLamViec { set; get; }
        [MaxLength(36)]
        public string IddmTrinhDo { set; get; }
        [MaxLength(36)]
        public string IdChucVu { set; get; }
        public string GhiChu { set; get; }
        public int SoLuongBoSung { get; set; }
        public int SoLuongThayThe { get; set; }
        public DateTime? NgayDuKienDiLam { get; set; }
        [NotMapped]
        public double NgayDuKienDiLamUnix { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenCaLamViec { get; set; }
        [NotMapped]
        public string TenTrinhDo { get; set; }

    }


    public class HRM_TuyenDung_DinhBienCoSo : Audit
    {
        [MaxLength(50)]
        public string SoQuyTrinh { get; set; }
        public string NoiDung { set; get; }
        public string GhiChu { set; get; }
        public int Nam { set; get; }
        public DateTime? Ngay { get; set; }
        [MaxLength(36)]
        public string IdTrangThai { get; set; }
        [MaxLength(36)]
        public string IdQuyTrinhGoc { get; set; }
        [MaxLength(500)]
        public string TenTrangThai { get; set; }
        public bool isKetThuc { set; get; } = false;
        [NotMapped]
        public double NgayUnix { get; set; }
        [NotMapped]
        public List<BoPhan> listCongTy { get; set; } = new List<BoPhan>();
        [NotMapped]
        public List<HRM_TuyenDung_dinhBienCoSo_ChucDanh> listChucDanh { get; set; }

    }

    public class HRM_TuyenDung_dinhBienCoSo_ChucDanh
    {
        public long IdChucDanh { get; set; }
        public string TenChucDanh { get; set; }
        public long IdBoPhanEOS { get; set; }
        public string TenBoPhan { get; set; }

        public List<HRM_TuyenDung_DinhBienCoSo_CongTy> listCongTy { get; set; }
    }
    public class HRM_TuyenDung_DinhBienCoSo_CongTy : Audit
    {
        [MaxLength(36)]
        public string IdDinhBienCoSo { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }
        public int Nam { get; set; }
        public long IdCongTyEOS { get; set; }
        public long IdChucDanh { get; set; }
        public long IdBoPhanEOS { get; set; }
        public double? DinhBien { get; set; }
        public double? Luong { get; set; }
        public double? QuyLuong { get; set; }
        public bool isKetThuc { set; get; } = false;
        public int SapXep { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenCapBacLuong { get; set; }

    }


    public class HRM_DeNghiTuyenDung : AudiQuyTrinh
    {
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public int? SoLuongNam { get; set; }
        public int? SoLuongNu { get; set; }
        public DateTime? NgayBatDauDiLam { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public string LyDo { get; set; }
        public string IdNguoiTaoPhieu { get; set; }
        public int? SoLuongBaoVu { get; set; }
        public int? SoLuongThoiVu { get; set; }
        public int? SoLuongThoiVuNgay { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
         [NotMapped]
        public string TongSo { get; set; }

        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string MaCongTy { get; set; }
        [NotMapped]
        public string MaBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public string MaChucVu { get; set; }

        [NotMapped]
        public string MaNguoiYeuCau { get; set; }

        [NotMapped]
        public double NgayBatDauDiLamUnix { get; set; }
        [NotMapped]
        public double NgayYeuCauUnix { get; set; }
        [NotMapped]
        public List<HRM_DeNghiTuyenDung_CongNhan> listCongNhan { get; set; }
        [NotMapped]
        public List<HRM_DeNghiTuyenDung_BoPhan> listBoPhan { get; set; }

        [NotMapped]
        public List<HRM_DeNghiTuyenDung_CaLamViec> listCaLamViec { get; set; }

        [NotMapped]
        public HRM_DeNghiTuyenDung_NhanVien NhanVien { get; set; }


    }
    public class HRM_DeNghiTuyenDung_NhanVien : Audit
    {
        [NotMapped]
        public string MaCongTy { get; set; }
        [NotMapped]
        public string MaBoPhan { get; set; }
        public string IdQuyTrinh { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public bool IsKetThuc { get; set; }
        public long IdCongTy { get; set; }
        public string YeuCauCongViec { get; set; }
        public string YeuCauTrinhDo { get; set; }
        public string YeuCauKinhNghiem { get; set; }
        public string QuyenLoiDuocHuong { get; set; }
        public string YeuCauKhac { get; set; }
        public double? MucLuong { get; set; }
        public DateTime ThoiGianTuyen { get; set; }
        [NotMapped]
        public double ThoiGianTuyenUnix { get; set; }

    }
    public class HRM_DeNghiTuyenDung_BoPhan : Audit
    {
        [NotMapped]
        public string MaCongTy { get; set; }
        public string IdQuyTrinh { get; set; }
        public bool IsKetThuc { get; set; }
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public int Nam { get; set; }
        public int Nu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_DeNghiTuyenDung_CaLamViec : Audit
    {
        public string IdQuyTrinh { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public long IdCongTy { get; set; }
        public bool IsKetThuc { get; set; }
        public string IddmCaLamViec { get; set; }
        public int? Nam { get; set; }
        public int? Nu { get; set; }
        public int SapXep { get; set; }

    }

    public class HRM_DeNghiTuyenDung_CongNhan : Audit
    {
        public string IdQuyTrinh { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public long IdCongTy { get; set; }
        public bool IsKetThuc { get; set; }
        public string Loai { get; set; }
        public double? TongLuong { get; set; }
        public double? LuongCoBan { get; set; }
        public double? Phep { get; set; }
        public string GhiChu { get; set; }
        public double? Khac { get; set; }
        public double? NgayThuong { get; set; }
        public double? NgayNghi { get; set; }
        public double? NgayTet { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public string MaCongTy { get; set; }
        [NotMapped]
        public string MaBoPhan { get; set; }

        [NotMapped]
        public double NgayBatDauDiLamUnix { get; set; }
        [NotMapped]
        public List<HRM_DeNghiTuyenDung_CongNhan_PhuCap> listPhuCap { get; set; }
    }
    public class HRM_DeNghiTuyenDung_CongNhan_PhuCap : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string Loai { get; set; }
        public string IddmPhuCap { get; set; }
        public double? SoTien { get; set; }
        public int SapXep { get; set; }
    }

    #endregion
    public class HRM_TuyenDung_KeHoach : Audit
    {
        [MaxLength(50)]
        public string SoPhieu { get; set; }
        [MaxLength(50)]
        public string LoaiKeHoach { get; set; }
        public int Nam { get; set; }
        public long IdCongTyEOS { set; get; }
        public long IdBoPhanEOS { get; set; }
        [NotMapped]
        public string TenPhongBanEOS { set; get; }
        public string Ten { set; get; }
        public int Loai { set; get; }
        public int Thang { set; get; }
        public int Quy { set; get; }
        public string IdKeHoachTuyenDungTongHop { get; set; }

        public int SoLuongTuyenDung { get; set; }
        [MaxLength(36)]
        public string IdQuyTrinh { set; get; }
        [MaxLength(36)]
        public string IdTrangThai { get; set; } //eTrangThaiKeHoachTuyenDung
        [MaxLength(500)]
        public string TenTrangThai { get; set; }
        public bool isKetThuc { set; get; } = false;
        [NotMapped]
        public List<HRM_TuyenDung_KeHoach_ChiTiet> listChiTiet { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
    }






    public class HRM_TuyenDung_KeHoach_ChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungKeHoach { get; set; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucDanhEOS { get; set; }
        [MaxLength(36)]
        public string IddmTrinhDo { set; get; }

        public int SoLuong { get; set; }
        public double LuongDinhBien { get; set; }
        public double MucLuongTu { set; get; }
        public double MucLuongDen { set; get; }
        public int BoSung { get; set; }
        public int ThayThe { get; set; }
        [MaxLength]
        public string TenTrinhDo { get; set; }

        public int ThoiGianLamDuKien { get; set; }
        [MaxLength(2000)]
        public string GhiChu { get; set; }

        public DateTime ThoiGianDiLamDuKien { get; set; }
        [NotMapped]
        public double ThoiGianDiLamDuKienUnix { get; set; }

        [MaxLength(36)]
        public string IddmThoiGianLamViec { set; get; }
        [NotMapped]
        public string TenCaLamViec { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }

        [NotMapped]
        public HRM_dmThoiGianLamViec ThoiGianLamViec { get; set; }
        [NotMapped]
        public HRM_dmTrinhDoDaoTao TrinhDoDaoTao { get; set; }
    }

    public class HRM_TuyenDung_KeHoach_TongHop : Audit
    {

        [MaxLength(50)]
        public string SoPhieu { get; set; }
        [MaxLength(50)]
        public string LoaiKeHoach { get; set; }
        public int Nam { get; set; }
        public int SoLuongTuyenDung { get; set; }
        public double TongSoTien { set; get; }
        [NotMapped]
        public List<HRM_TuyenDung_KeHoach_TongHopChiTiet> lstChiTiet { get; set; }

        [NotMapped]
        public List<HDFile> lstFileDinhKemMoi { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> ListFileDinhKemCu { get; set; }
        [MaxLength(100)]
        public string TenTrangThai { set; get; }
        public bool isKetThuc { set; get; } = false;
        [MaxLength(36)]
        public string IdTrangThai { set; get; }
        [NotMapped]
        public List<HRM_TuyenDung_TongHop_KeHoach> listIdKeHoach = new List<HRM_TuyenDung_TongHop_KeHoach>();

    }

    public class HRM_TuyenDung_TongHop_KeHoach : Audit
    {
        [MaxLength(36)]
        public string IdKetHoachTongHop { get; set; }
        [MaxLength(36)]
        public string IdKetHoach { get; set; }

    }
    public class HRM_TuyenDung_KeHoach_TongHopChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdKetHoachTongHop { get; set; }
        public int BoSung { get; set; }
        public int ThayThe { get; set; }
        public int SoLuong { set; get; }
        [MaxLength(36)]
        public string IddmTrinhDo { get; set; }

        public DateTime ThoiGianDiLamDuKien { get; set; }
        public int ThoiGianLamDuKien { get; set; }
        public long IdBoPhanEOS { get; set; }
        public long IdChucDanhEOS { get; set; }
        public double ChiPhiDuKien { set; get; }
        public string GhiChu { get; set; }
        [NotMapped]
        public double ThoiGianDiLamDuKienUnix { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }

        [NotMapped]
        public HRM_dmTrinhDoDaoTao TrinhDoDaoTao { get; set; }
        [NotMapped]
        public List<HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung> lstNguonTuyenDung = new List<HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung>();
    }

    public class HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung : Audit
    {
        [MaxLength(36)]
        public string IdKetHoachChiTiet { get; set; }
        [MaxLength(36)]
        public string IddmNguonTuyenDung { set; get; }
    }
    // public class HRM_TuyenDung_
    #region Đề nghị tuyển dụng


    public class HRM_TuyenDung_DeNghi : Audit
    {
        public string SoPhieu { get; set; }
        [MaxLength(50)]
        public string DotTuyenDung { get; set; }
        public int Nam { get; set; }
        public string IdUser { set; get; }
        public long IdCongTyEOS { set; get; }
        //public long IdChucVuEOS { get; set; }
        public long IdBoPhanEOS { get; set; }
        public DateTime NgayDeNghi { set; get; }
        [NotMapped]
        public double NgayDeNghiUnix { set; get; }
        [MaxLength(36)]
        public string IdDeNghiTuyenDungTongHop { get; set; }
        public int SoLuongTuyenDung { get; set; }
        public bool isDaTaoTuyenDung { get; set; } // hiển thị hay o hiển thị ở phần tuyển dụng
        [MaxLength(36)]
        public string IdTrangThai { set; get; }
        public bool isKetThuc { set; get; } = false;
        [MaxLength(250)]
        public string TenTrangThai { get; set; } //eTrangThaiKeHoachTuyenDung

        [NotMapped]
        public List<HRM_TuyenDung_DeNghi_ChiTiet> lstChiTiet { get; set; }
    }


    public class HRM_TuyenDung_DeNghi_ChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungDeNghi { get; set; }

        public long IddmChucDanh { get; set; }
        [MaxLength(36)]
        public string IddmThoiGianLamViec { get; set; }
        public int BoSung { get; set; }
        public int ThayThe { get; set; }
        [MaxLength(36)]
        public string IddmTrinhDoDaoTao { get; set; }

        [MaxLength(1500)]
        public string TrinhDo { set; get; }
        public double ChiPhiTuyenDung { set; get; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string GhiChu { get; set; }
        public double MucLuong { get; set; }
        public bool isTuyenDung { get; set; } // giành cho phần tuyển dụng

        [NotMapped]
        public double ThoiGianBatDauUnix { get; set; }
        [NotMapped]
        public double ThoiGianKetThucUnix { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
        [NotMapped]
        public HRM_dmTrinhDoDaoTao TrinhDoDaoTao { get; set; }
        [NotMapped]
        public HRM_dmThoiGianLamViec ThoiGianLamViec { get; set; }
    }


    public class HRM_TuyenDung_DeNghi_TongHop : Audit
    {
        [MaxLength(50)]
        public string SoPhieu { get; set; }
        [MaxLength(50)]
        public string DotTuyenDung { get; set; }
        public int Nam { get; set; }
        public long IdCongTyEOS { set; get; }
        public DateTime NgayTongHopDeNghi { set; get; }
        [NotMapped]
        public double NgayTongHopDeNghiUnix { set; get; }
        public int SoLuongTuyenDung { get; set; }
        [MaxLength(36)]
        public string IdTrangThai { get; set; } //eTrangThaiKeHoachTuyenDungTongHop
        public string TenTrangThai { get; set; } //eTrangThaiKeHoachTuyenDungTongHop
        public bool isKetThuc { set; get; } = false;
        [NotMapped]
        public List<HRM_TuyenDung_DeNghi_TongHopChiTiet> lstChiTiet { get; set; }
        [NotMapped]
        public List<HRM_TuyenDung_DeNghi_TongHop_Dot> lstDotDeNghi { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileUpload { get; set; }
        public double TongSoTien { set; get; }
    }


    public class HRM_TuyenDung_DeNghi_TongHopChiTiet : Audit
    {
        public long IddmChucDanhEOS { get; set; }
        public int BoSung { get; set; }
        public int ThayThe { get; set; }
        [MaxLength(36)]
        public string IddmTrinhDoDaoTao { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        [NotMapped]
        public double ThoiGianBatDauUnix { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        [NotMapped]
        public double ThoiGianKetThucUnix { get; set; }

        public double MucLuong { set; get; }


        public double ChiPhiTuyenDung { get; set; }
        public long IdBoPhanEOS { get; set; }
        [MaxLength(2500)]
        public string GhiChu { get; set; }
        [MaxLength(36)]
        public string IdQuyTrinhTongHop { get; set; }
        [NotMapped]
        public string TrinhDoDaoTao { get; set; }

        [NotMapped]
        public bool isDeleted { get; set; }
        [NotMapped]
        public List<HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon> listNguonTuyenDung { get; set; }
    }
    public class HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon : Audit
    {
        [MaxLength(36)]
        public string IddmNguonTuyenDung { get; set; }
        [MaxLength(36)]
        public string IdTongHopChiTiet { get; set; }
        [NotMapped]
        public bool isXoa { set; get; }
    }
    public class HRM_TuyenDung_DeNghi_TongHop_Dot : Audit
    {
        [MaxLength(36)]
        public string IdQuyTrinhTongHop { set; get; }
        [MaxLength(36)]
        public string IdDeNghi { set; get; }
        [NotMapped]
        public bool isXoa { set; get; }
    }
    #endregion


    #region Tuyển Dụng


    public class HRM_TuyenDung_QuyTrinh : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungDeNghi { get; set; }
        [MaxLength(20)]
        public string SoPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public int Nam { get; set; }
        [StringLength(36)]
        public string IdTrangThai { get; set; }
        [StringLength(150)]
        public string TenTrangThai { set; get; }

        [NotMapped]
        public double NgayLapUnix { get; set; }

        [NotMapped]
        public HRM_TuyenDung_DeNghi DeNghiTuyenDung { get; set; }

        [NotMapped]

        public List<HRM_TuyenDung_DeNghi_ChiTiet> lstDeNghiTuyenDungChiTiet { get; set; }


    }

    public class TuyenDung_QuyTrinh : Audit
    {
        public string IdTuyenDungDeNghi { get; set; }
        public string SoPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public int Nam { get; set; }
        public byte TrangThai { get; set; }

        public string DotTuyenDung { get; set; }

    }


    public class HRM_TuyenDung_UngVien : Audit
    {
        public long? IdCongTyEOS { get; set; }
        public long? IdCoSoTrungTuyen { get; set; }
        public long? IdBoPhanEOS { get; set; }
        [MaxLength(36)]
        public string IdNhanVienGioiThieu { get; set; }
        [MaxLength(50)]

        public string MaUngVien { get; set; }
        public string NguyenQuan { get; set; }
        [MaxLength(50)]
        public string Ho { set; get; }
        [MaxLength(50)]
        public string TenDem { set; get; }
        [MaxLength(50)]
        public string Ten { set; get; }

        public int TrangThai { set; get; }// 0 chưa sở vấn - 1 đã sơ vấn  // 2 sơ vấn pass --3 trúng tuyển --4 trượt -- 5 chờ lịch phỏng vấn
        [MaxLength(100)]
        public string TenTrangThai { set; get; }
        public long IddmTrungTuyen { set; get; }
        public double Luong { set; get; }
        public string DotTuyenDung { get; set; }
        public string GhiChu { get; set; }
        [MaxLength(150)]
        public string HoTenDayDu { get; set; }
        public string SoCMT { get; set; }
        public string MaSoThueCMT { get; set; }
        public string MaSoThueCCCD { get; set; }
        public string CCCD { get; set; }
        [MaxLength(50)]
        public string GioiTinh { get; set; }
        public string IdNhuCauChiTiet { get; set; }
        public string IdQuyTrinhNhuCau { get; set; }
        public string NoiCap { get; set; }
        public DateTime? NgaySoVan { get; set; }
        public DateTime? NgaySinh { get; set; }
        [NotMapped]
        public double NgaySinhUnix { get; set; }
        public DateTime? NgayCap { get; set; }
        [NotMapped]
        public double NgayCapUnix { get; set; }

        [MaxLength(5000)]
        public string HoKhauThuongTru { get; set; }
        [MaxLength(36)]
        public string IddmQuocTich { get; set; }
        [MaxLength(36)]
        public string IddmDanToc { get; set; }
        [MaxLength(36)]
        public string IddmTinhTrangHonNhan { get; set; }
        [MaxLength(36)]
        public string IddmTrinhDoHocVan { get; set; }
        [NotMapped]
        public string TendmTrinhDoHocVan { get; set; }
           [NotMapped]
        public string TenChucVu { get; set; }

        [NotMapped]
        public string TenChucDanh { get; set; }

        [MaxLength(5000)]
        public string ChuyenNganh { get; set; }
        [MaxLength(5000)]
        public string DiaChi { get; set; }
        [MaxLength(36)]
        public string IddmTinhThanhPho { get; set; }
        [MaxLength(36)]
        public string IddmQuanHuyen { get; set; }
        [MaxLength(36)]
        public string IddmPhuongXa { get; set; }

        [MaxLength(36)]
        public string IddmTinhThanhPhoThuongTru { get; set; }
        [MaxLength(36)]
        public string IddmQuanHuyenThuongTru { get; set; }
        [MaxLength(36)]
        public string IddmPhuongXaThuongTru { get; set; }

        public bool IsNghiSauKhiNhanViec { get; set; }
        public bool IsNhanViec { get; set; }
        public string IddmNguonTuyenDung { get; set; }
        public string Loai { get; set; }
        public long IdChucVu { get; set; }
        public DateTime? NgayNhanViecDuKien { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }
        public string MaNhanVien { get; set; }
        public string IdLoaiHopDong { get; set; }
        public int? HinhThucTraLuongDaoTao { get; set; }
        public double? MucLuongUngVienDeNghi { get; set; }
        public double ThoiGianThuViec { get; set; }
        public double PhanTramLuongThuViec { get; set; }
        public bool isCongNhan { get; set; } = false;
        [NotMapped]
        public double NgayNhanViecDuKienUnix { get; set; }
        [NotMapped]
        public string TendmNguonTuyenDung { get; set; }
        [MaxLength(20)]
        public string DienThoaiDiDong { get; set; }
        [MaxLength(150)]
        public string Email { get; set; }
        [MaxLength(20)]
        public string SoDienThoai { set; get; }
        public DateTime? NgayTiepNhan { set; get; }
        [NotMapped]
        public double NgayTiepNhanUnix { set; get; }
        [NotMapped]
        public double NgaySoVanUnix { set; get; }

        [MaxLength(36)]
        public string IdLichPhongVan { set; get; }
        public long IddmChucDanhUngTuyen { get; set; }
        public bool isDaPheDuyetLuong { get; set; } = false;
        public double? ThuCoc { get; set; }
        public string IdLoaiNhanVien { get; set; }
        [NotMapped]
        public string TenChucDanhUngTuyen { get; set; }
        [NotMapped]
        public string TenLoaiNhanVien { get; set; }

        [NotMapped]
        public HRM_dmQuocTich QuocTich { get; set; }

        [NotMapped]
        public HRM_dmDanToc DanToc { get; set; }

        [NotMapped]
        public HRM_dmTinhTrangHonNhan TinhTrangHonNhan { get; set; }

        [NotMapped]
        public HRM_dmTinh TinhThanhPho { get; set; }

        [NotMapped]
        public HRM_dmQuanHuyen QuanHuyen { get; set; }

        [NotMapped]
        public HRM_dmPhuongXa PhuongXa { get; set; }

        [NotMapped]
        public List<HRM_TuyenDung_UngVienKinhNghiemLamViec> lstKinhNghiemLamViec { get; set; }
        [NotMapped]
        public List<HRM_TuyenDung_UngVien_SoVanTieuChi> listSoVanTieuChi { get; set; }

        [NotMapped]
        public List<HRM_UngVienBangCap> ListUngVienBangCap { get; set; }
        [NotMapped]
        public List<HRM_UngVienQuanHeGiaDinh> ListQuanHeGiaDinhUngVien { get; set; }
        [NotMapped]
        public List<HRM_NgonNguUngVien> ListUngVienNgonNgu { get; set; }
        [NotMapped]
        public List<HRM_ChungChiUngVien> ListUngVienChungChi { get; set; }
        [NotMapped]
        public string ViTriChuyenMonUngTuyen { get; set; }

        [MaxLength(36)]
        public string IdTongHopDeNghiTuyenDung { get; set; }

        [NotMapped]
        public HRM_TuyenDung_DeNghi_TongHop TongHopDeNghiTuyenDung { get; set; }

        [MaxLength(36)]
        public string IdTrangThai { get; set; }


        [NotMapped]
        public string IdLinkTuyenDungUngVien { get; set; } //HRM_PheDuyetTuyenDung Sử dụng

        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem { set; get; }
        [NotMapped]
        public List<HRM_UngVien_PhuCap> lisPhuCap { set; get; }


    }
    public class HRM_NgonNguUngVien : Audit
    {
        public string IdUngVien { get; set; }
        public string IdNgonNgu { get; set; }

        public string IdXepLoaiNghe { get; set; }//HRM_dmXepLoai
        public string IdXepLoaiNoi { get; set; } //HRM_dmXepLoai
        public string IdXepLoaiDoc { get; set; } //HRM_dmXepLoai
        public string IdXepLoaiViet { get; set; }//HRM_dmXepLoai



        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }



    }

    public class HRM_ChungChiUngVien : Audit
    {
        public string IdUngVien { get; set; }
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
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }


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

    public class HRM_UngVienBangCap : Audit
    {
        [MaxLength(36)]
        public string IdUngVien { get; set; }
        public string NoiDaoTao { get; set; }
        public string ChuyenNganh { get; set; }
        [MaxLength(36)]
        public string IdTrinhDoDaoTao { get; set; } //eTrinhDoDaoTao
        public int? NamTotNghiep { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }



    }

    public class HRM_UngVienQuanHeGiaDinh : Audit
    {
        [MaxLength(128)]
        public string IdUngVien { get; set; }
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

    // vì có thể chọn lại ứng viên cho quy trình tuyển dụng khác, Dựa vào IdUngVien và IdQuyTrinhTuyenDung để tính chính xác
    public class HRM_Link_TuyenDungQuyTrinh_UngVien : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungQuyTrinh { get; set; }
        public string TrangThai { get; set; } //eTrangThaiUngVien
        [MaxLength(36)]
        public string IdTuyenDungUngVien { get; set; }
        public string IdTuyenDungDeNghiChiTiet { get; set; }
        public string IdTuyenDungPheDuyet { get; set; } // những ứng viên được tiếp nhận sẽ gửi lên tổng giám đốc
        public bool? isPheDuyetGiamDoc { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }

    public class HRM_TuyenDung_UngVien_SoVan : Audit
    {
        [MaxLength(36)]
        public string IdUngVien { set; get; }
        public string IdNguoiSoVan { set; get; }
        public string TenNguoiSoVan { set; get; }
        public DateTime NgaySoVan { set; get; }
        [NotMapped]
        public double NgaySoVanUnix { set; get; }
        public string GhiChu { set; get; }
        public int TrangThai { set; get; } //2 sơ vấn pass --3 sơ vấn lỗi 

        public double TongDiem { set; get; }
        [NotMapped]
        public List<HRM_TuyenDung_UngVien_SoVanTieuChi> listSoVanTieuChi = new List<HRM_TuyenDung_UngVien_SoVanTieuChi>();
    }
    public class HRM_TuyenDung_UngVien_SoVanTieuChi : Audit
    {
        [MaxLength(36)]
        public string IdUngVien { set; get; }
        [MaxLength(36)]
        public string IdTuyenDungSoVan { set; get; }
        [MaxLength(36)]
        public string IddmTieuChi { set; get; }
        public double SoDiem { set; get; }
        public int SapXep { get; set; }
        public string GhiChu { set; get; }
        [NotMapped]
        public List<HRM_TuyenDung_UngVien_SoVanChiTiet> listSoVanChiTiet = new List<HRM_TuyenDung_UngVien_SoVanChiTiet>();
    }
    public class HRM_TuyenDung_UngVien_SoVanChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdSoVanTieuChi { set; get; }
        [MaxLength(36)]
        public string IdTuyenDungSoVan { set; get; }
        [MaxLength(36)]
        public string IdUngVien { set; get; }
        [MaxLength(36)]
        public string IddmCauHoi { set; get; }
        public int SapXep { get; set; }
        public string CauTraLoi { set; get; }
        public string DanhGia { set; get; }
    }
    public class HRM_TuyenDung_UngVienKinhNghiemLamViec : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungUngVien { get; set; }
        [MaxLength(1500)]
        public string ThoiGianLamViec { get; set; }
        [MaxLength(1500)]
        public string DonViCongTac { get; set; }
        [MaxLength(1500)]
        public string ViTri { get; set; }

        [NotMapped]
        public string DienThoaiDiDong { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }

    public class HRM_TuyenDung_QuyTrinhPhongVan : Audit
    {
        [MaxLength(36)]
        public string IdUngVien { set; get; }
        [MaxLength(20)]
        public string MaUngVien { set; get; }
        [MaxLength(50)]
        public string HoTenUngVien { set; get; }
        public string DotTuyenDung { set; get; }
        [MaxLength(20)]
        public string SoPhieu { set; get; }
        public string IdLoaiHopDong { set; get; }
        public string LoaiCongNhan { get; set; } // 1 Bảo trì, 2 thời vụ/ thời vụ ngày
        public long IdCongTyEOS { set; get; }
        public long? IdBoPhan { set; get; }
        public bool isCongNhan { get; set; } = false;
        public long IdCoSoTrungTuyen { set; get; }
        public double? ThuCoc { get; set; }
        public bool IsNghiSauKhiNhanViec { get; set; }
        [MaxLength(36)]
        public string IdTongHopDeNghiTuyenDung { set; get; }
        public string IdLoaiNhanVien { set; get; }
        [NotMapped]
        public HRM_TuyenDung_DeNghi_TongHop TongHopDeNghiTuyenDung { get; set; }
        public long IdChucDanhUngTuyen { set; get; }
        public long IddmTrungTuyen { set; get; }
        public long IdChucVu { set; get; }
        [NotMapped]

        public string IdNhanVienGioiThieu { set; get; }
        [NotMapped]
        public string TenChucDanhUngTuyen { set; get; }
        [NotMapped]
        public string TenChucVu { set; get; }
        [NotMapped]
        public string MaNhanVien { set; get; }


        [NotMapped]
        public string TenChucDanhTrungTuyen { set; get; }

        public DateTime NgayTao { set; get; }
        public double NgayTaoUnix { set; get; }
        public double Luong { set; get; }
        public double MucLuongUngVienDeNghi { get; set; }
        public double MucLuongThoaThuan { get; set; }
        public int TrangThai { set; get; }// 0- Lưu nháp, 5 Chờ phỏng vấn, 10 Phỏng vấn đạt, 15 Phỏng vấn không đạt

        public DateTime? NgayNhanViecDuKien { get; set; }
        public double? LuongDaoTao { get; set; }
        public int? HinhThucTraLuongDaoTao { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }
        public double ThoiGianThuViec { get; set; }
        public double PhanTramLuongThuViec { get; set; }
        [NotMapped]
        public double NgayNhanViecDuKienUnix { get; set; }
        [NotMapped]
        public bool IsNhanViec { get; set; }
        [NotMapped]
        public string NgayTiepNhan { get; set; }
        [NotMapped]
        public DateTime? NgayTiepNhand { get; set; }

        [NotMapped]
        public string TenTrangThai { set; get; }


        [MaxLength(36)]
        public string IdNguoiTao { set; get; }
        [NotMapped]
        public List<HRM_TuyenDung_VongPhongVan> listVongPhongVan = new List<HRM_TuyenDung_VongPhongVan>();
        [NotMapped]
        public List<HRM_TuyenDung_PhuCap> listPhuCap = new List<HRM_TuyenDung_PhuCap>();

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
    }
    public class HRM_TuyenDung_PhuCap : Audit
    {
        public string IdUngVien { get; set; }
        public string IdQuyTrinhPhongVan { get; set; }
        public string IddmPhuCap { get; set; }
        public double SoTien { get; set; }
        public bool isChon { get; set; }
        [NotMapped]
        public string TenPhuCap { get; set; }
        public HRM_TuyenDung_PhuCap() { }
        public HRM_TuyenDung_PhuCap Clone()
        {
            return (HRM_TuyenDung_PhuCap)this.MemberwiseClone();
        }
    }
    public class HRM_UngVien_PhuCap : Audit
    {
        public string IdUngVien { get; set; }

        public string IddmPhuCap { get; set; }
        public double SoTien { get; set; }
    }

    public class HRM_TuyenDung_VongPhongVan : Audit
    {
        //[MaxLength(36)]
        //public string IdTuyenDungQuyTrinh { get; set; }
        [MaxLength(36)]
        public string IdQuyTrinhPhongVan { get; set; }
        [MaxLength(500)]
        public string NguoiPhongVanNgoai { get; set; }
        [MaxLength(36)]
        public string IdNguoiChuTri { set; get; }
        public string IdNguoiPhongVanNgoai { set; get; }
        [MaxLength(500)]
        public string DiaDiem { get; set; }
        public int VongPhongVan { get; set; }
        public double MucLuongUngVienDeNghi { get; set; }
        public double MucLuongThoaThuan { get; set; }
        public DateTime NgayDuKienLamViec { get; set; }
        [MaxLength(5000)]
        public string NhanXet { get; set; }
         [MaxLength(5000)]
        public string NhanXetChung { get; set; }

        public double SoDiem { get; set; }
        public string LoaiCongNhan { get; set; } // 1 Bảo trì, 2 thời vụ/ thời vụ ngày

        public double TyLeDongThuan { get; set; }

        public int TrangThai { get; set; }// 0 lịch nháp    , 5  đồng ý phỏng vấn ,10 không đồng ý pv ,15 pass, 20 fail
        [NotMapped]
        public string TenTrangThai { set; get; }
        public DateTime NgayPhongVan { get; set; }
        [NotMapped]
        public string TenNguoiPhongVanNgoai { set; get; }
        [NotMapped]
        public string Ten { set; get; }
        [NotMapped]
        public double NgayPhongVanUnix { get; set; }

        [NotMapped]
        public List<HRM_TuyenDung_NguoiPhongVan> lstNguoiPhongVan = new List<HRM_TuyenDung_NguoiPhongVan>();
        [NotMapped]
        public double NgayDuKienLamViecUnix { get; set; }
        [NotMapped]
        public bool isXoa { set; get; }
        [NotMapped]
        public HRM_TuyenDung_UngVien UngVien { set; get; }
        [NotMapped]
        public string ViTriUngTuyen { set; get; }
    }

    public class HRM_TuyenDung_NguoiPhongVan : Audit
    {
        [MaxLength(36)]
        public string IdVongPhongVan { get; set; }
        [MaxLength(36)]
        public string IdUngVien { get; set; }
        public int isThamGia { get; set; } /// <summary>
                                           /// 0 là chưa xác nhận
                                           /// 1 Tham gia
                                           /// 2 ko tham gia
                                           /// 3 đat
                                           /// 4 không đạt
                                           /// </summary>
        [NotMapped]
        public string TrangThaiThamGia { set; get; }
        public int TrangThai { get; set; }// 1 đạt, 2 không đạt
        [MaxLength(36)]
        public string IdNguoiThamGia { set; get; }
        public double MucLuongUngVienDeNghi { get; set; }
        public double MucLuongThoaThuan { get; set; }
        [NotMapped]
        public string TenNguoiThamGia { set; get; }
        public double SoDiem { get; set; }
        public string NhanXet { get; set; }
        public string NhanXetChung { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }

        [NotMapped]
        public bool isXoa { set; get; }

    }
    public class HRM_TuyenDung_QuyTrinhPheDuyetLuong : AudiQuyTrinh
    {

        [MaxLength(36)]
        public string IdTongHopDeNghiTuyenDung { set; get; }
        [NotMapped]
        public HRM_TuyenDung_DeNghi_TongHop TongHopDeNghiTuyenDung { get; set; }
        public long IdCongTy { set; get; }
        public DateTime NgayDeNghi { set; get; }
        public double NgayDeNghiUnix { set; get; }

        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngVien = new List<HRM_TuyenDung_PheDuyetLuong_UngVien>();

    }
    public class HRM_TuyenDung_PheDuyetLuong_UngVien : Audit
    {
        [MaxLength(36)]
        public string IdQuyTrinhPheDuyet { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(36)]
        public string IdUngVien { set; get; }
        public long IdBoPhan { set; get; }
        public string MaNhanVien { get; set; }
        public double Luong { set; get; }
        public double LuongBHXH { set; get; }
        public string IdCapBacLuongChapNhan { set; get; }
        public string IddmLoaiHopDong { get; set; }
        public int LoaiLuong { get; set; }
        [MaxLength(1500)]
        public string GhiChu { set; get; }
        public DateTime? NgayNhanViecDuKien { get; set; }
        public DateTime? KetThucThuViec { get; set; }
        public int SapXep { get; set; }
        public string IdCaLamViec { get; set; }
        public string IdCapBacLuongDeXuat { get; set; }
        public long IddmTrungTuyen { set; get; }
        public double ThoiGianThuViec { get; set; }
        public double PhanTramLuongThuViec { get; set; } = 100;
        [NotMapped]
        public double NgayNhanViecDuKienUnix { get; set; }
        [NotMapped]
        public double KetThucThuViecUnix { get; set; }

        [NotMapped]
        public string MaUngVien { get; set; }
        [NotMapped]
        public string HoTenDayDu { get; set; }
        [NotMapped]
        public string HoTenNhanVienCu { get; set; }

        [NotMapped]
        public double? MucLuongThoaThuan { get; set; } = 0;
        [NotMapped]
        public double TongThuNhap { get; set; }
        [NotMapped]
        public double MucLuongThuViec { get; set; }
        [NotMapped]
        public string LoaiHopDong { get; set; }
        [NotMapped]
        public double ThoiHanHopDong { get; set; }

        [NotMapped]
        public string DotTuyenDung { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenCoSo { get; set; }
        [NotMapped]
        public string TenCaLamViec { get; set; }

        public long IdChucDanhUngTuyen { set; get; }
        [NotMapped]
        public string TenChucDanhUngTuyen { set; get; }
        [NotMapped]
        public string TenVitriTrungTuyen { set; get; }

        public int TrangThai { set; get; }// 1 duyệt, 2 không duyệt
        [NotMapped]
        public List<HRM_PheDuyetLuong_UngVien_PhuCap> lisPhuCap { get; set; }
        [NotMapped]
        public bool isXoa { set; get; }
        public HRM_TuyenDung_PheDuyetLuong_UngVien() { }
        public HRM_TuyenDung_PheDuyetLuong_UngVien Clone()
        {
            return (HRM_TuyenDung_PheDuyetLuong_UngVien)this.MemberwiseClone();
        }
    }
    public class HRM_PheDuyetLuong_UngVien_PhuCap : Audit
    {
        public string IdQuyTrinhPheDuyetLuong { get; set; }
        public string IdUngVien { get; set; }

        public string IddmPhuCap { get; set; }
        public double SoTien { get; set; }
        public bool? isChon { get; set; }
        [NotMapped]
        public string TenPhuCap { get; set; }
        public HRM_PheDuyetLuong_UngVien_PhuCap() { }
        public HRM_PheDuyetLuong_UngVien_PhuCap Clone()
        {
            return (HRM_PheDuyetLuong_UngVien_PhuCap)this.MemberwiseClone();
        }
    }
    public class HRM_TuyenDung_PheDuyet : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungQuyTrinh { get; set; }
        public byte TrangThai { get; set; }
        [MaxLength(50)]
        public string SoPhieu { get; set; }


        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKemCu { get; set; }
        [NotMapped]
        public List<HDFile> lstFileDinhKemMoi { get; set; }

        [NotMapped]
        public List<HRM_TuyenDung_UngVien> lstUngVien { get; set; }
    }


    public class DanhSachTrungTuyen : Audit
    {
        public DateTime NgayLap { get; set; }
        [MaxLength(500)]
        public string SoDeNghiTuyenDungDuocPheDuyet { get; set; }
        [MaxLength(500)]
        public string DotTuyenDung { get; set; }
        public int NamTuyenDung { get; set; }
        [MaxLength(36)]
        public string IddmViTriChuyenMon { get; set; }
        public int SoLuongTuyen { get; set; }

        public byte TrangThai { get; set; }
        public string IdDeNghiTuyenDung { get; set; }


    }


    public class HRM_TuyenDung_ThuMoiLamViec : Audit
    {
        [MaxLength(36)]
        public string IdTuyenDungUngVien { get; set; }
        [MaxLength(36)]
        public string IdLinkTuyenDungQuyTrinhUngVien { get; set; }

        public DateTime NgayMoi { get; set; }
        [MaxLength(36)]
        public string IddmLoaiNhanVien { get; set; }
        [MaxLength(1500)]
        public string DonViCongTac { get; set; }

        public DateTime NgayBatDau { get; set; }
        [MaxLength(1500)]
        public string ThoiGianThuViec { get; set; }
        [MaxLength(1500)]
        public string ThoiGianLamViec { get; set; }

        public DateTime NgayXacNhan { get; set; }
        [MaxLength(1500)]
        public string DiaDiem { get; set; }

        public string KhoanPhucLoi { get; set; }


        [MaxLength(50)]
        public string TrangThaiXacNhan { get; set; }



        [NotMapped]
        public double NgayMoiUnix { get; set; }

        [NotMapped]
        public HRM_dmLoaiNhanVien LoaiNhanVien { get; set; }

        [NotMapped]
        public double NgayBatDauUnix { get; set; }

        [NotMapped]
        public double NgayXacNhanUnix { get; set; }

    }

    public class ThuMoiLamViec : Audit
    {

        public string MaUngVien { get; set; }

        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }

        public string DienThoaiDiDong { get; set; }
        public string IddmTrinhDoHocVan { get; set; }

        public string TrangThaiXacNhan { get; set; }

        public string ViTriChuyenMon { get; set; }
    }

    public class LichPhongVanViewModel
    {
        public List<LichPhongVanVongPhongVan> listVongPhongVan { set; get; }
        public List<string> listIdUngVien { set; get; }

        public string IdNguoiTao { set; get; }
        public int TrangThai { set; get; }
    }
    public class LichPhongVanVongPhongVan
    {
        public string NguoiPhongVanNgoai { set; get; }
        public string IdChuTri { set; get; }
        public List<string> listIdNguoiThamGia { set; get; }
        public int VongPhongVan { set; get; }
        public DateTime ThoiGianPhongVan { set; get; }
        public double ThoiGianPhongVanUnix { set; get; }
        public string DiaDiem { set; get; }
    }


    #endregion
}

