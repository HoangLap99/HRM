﻿using HRM.Api.Common.Interfaces;
using HRM.Api.Common;
using HRM.Api.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRM.Api.Persistence;

/// <summary>
/// DbContext chính của hệ thống HRM trên EF Core 8, ánh xạ 1:1 vào schema SQL Server hiện hữu (HRMAPIConnection).
/// </summary>
public class HrmDbContext : IdentityDbContext<ApplicationUser>, IHrmDbContext
{
    private readonly ICurrentUserContext? _currentUserContext;
    private CurrentUser? _overrideCurrentUser;
    private string? _overrideUserId;

    public HrmDbContext(
        DbContextOptions<HrmDbContext> options,
        ICurrentUserContext? currentUserContext = null) : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    public string IdUser
    {
        get => !string.IsNullOrEmpty(_overrideUserId)
            ? _overrideUserId
            : (_overrideCurrentUser?.Id ?? _currentUserContext?.UserId ?? string.Empty);
        set => _overrideUserId = value;
    }

    public CurrentUser? currentUser
    {
        get => _overrideCurrentUser ?? _currentUserContext?.User;
        set
        {
            _overrideCurrentUser = value;
            if (!string.IsNullOrEmpty(value?.Id))
            {
                _overrideUserId = value.Id;
            }
        }
    }

    #region Quy trình & Dùng chung
    public DbSet<HRM_QT_UserTiepTheo> HRM_QT_UserTiepTheo => Set<HRM_QT_UserTiepTheo>();
    public DbSet<HRM_QT_TrangThaiTiepTheo> HRM_QT_TrangThaiTiepTheo => Set<HRM_QT_TrangThaiTiepTheo>();
    public DbSet<HRM_QuyTrinhKhoiTao> HRM_QuyTrinhKhoiTao => Set<HRM_QuyTrinhKhoiTao>();
    public DbSet<HRM_QuyTrinhLienQuan> HRM_QuyTrinhLienQuan => Set<HRM_QuyTrinhLienQuan>();
    public DbSet<HRM_QuyTrinhXuLy> HRM_QuyTrinhXuLy => Set<HRM_QuyTrinhXuLy>();
    public DbSet<HRM_FileDinhKem> HRM_FileDinhKem => Set<HRM_FileDinhKem>();
    #endregion

    #region Các bảng nghiệp vụ HRM (~140 DbSets)
    public DbSet<HRM_BoPhan_ChucDanh> HRM_BoPhan_ChucDanh => Set<HRM_BoPhan_ChucDanh>();
    public DbSet<HRM_dmDanToc> HRM_dmDanToc => Set<HRM_dmDanToc>();
    public DbSet<HRM_dmTinhTrangHonNhan> HRM_dmTinhTrangHonNhan => Set<HRM_dmTinhTrangHonNhan>();
    public DbSet<HRM_dmCauHinhThueThuNhapCaNhan> HRM_dmCauHinhThueThuNhapCaNhan => Set<HRM_dmCauHinhThueThuNhapCaNhan>();
    public DbSet<HRM_dmQuocTich> HRM_dmQuocTich => Set<HRM_dmQuocTich>();
    public DbSet<HRM_dmNhomChuyenMon> HRM_dmNhomChuyenMon => Set<HRM_dmNhomChuyenMon>();
    public DbSet<HRM_dmViTriChuyenMon> HRM_dmViTriChuyenMon => Set<HRM_dmViTriChuyenMon>();
    public DbSet<HRM_dmCapBac> HRM_dmCapBac => Set<HRM_dmCapBac>();
    public DbSet<HRM_dmQuanHeGiaDinh> HRM_dmQuanHeGiaDinh => Set<HRM_dmQuanHeGiaDinh>();
    public DbSet<HRM_dmLoaiNhanVien> HRM_dmLoaiNhanVien => Set<HRM_dmLoaiNhanVien>();
    public DbSet<HRM_dmLoaiHopDong> HRM_dmLoaiHopDong => Set<HRM_dmLoaiHopDong>();
    public DbSet<HRM_dmThoiHanHopDong> HRM_dmThoiHanHopDong => Set<HRM_dmThoiHanHopDong>();
    public DbSet<HRM_dmTrangThaiHopDong> HRM_dmTrangThaiHopDong => Set<HRM_dmTrangThaiHopDong>();
    public DbSet<HRM_dmHinhThucTraLuong> HRM_dmHinhThucTraLuong => Set<HRM_dmHinhThucTraLuong>();
    public DbSet<HRM_dmNguonTuyenDung> HRM_dmNguonTuyenDung => Set<HRM_dmNguonTuyenDung>();
    public DbSet<HRM_dmThietLapNgayNghi> HRM_dmThietLapNgayNghi => Set<HRM_dmThietLapNgayNghi>();
    public DbSet<HRM_dmThoiGianLamViec> HRM_dmThoiGianLamViec => Set<HRM_dmThoiGianLamViec>();
    public DbSet<HRM_dmHeSoNghiBu> HRM_dmHeSoNghiBu => Set<HRM_dmHeSoNghiBu>();
    public DbSet<HRM_dmAnCa> HRM_dmAnCa => Set<HRM_dmAnCa>();
    public DbSet<HRM_dmNgonNgu> HRM_dmNgonNgu => Set<HRM_dmNgonNgu>();
    public DbSet<HRM_dmXepLoai> HRM_dmXepLoai => Set<HRM_dmXepLoai>();
    public DbSet<HRM_dmNhomChungChi> HRM_dmNhomChungChi => Set<HRM_dmNhomChungChi>();
    public DbSet<HRM_dmTrinhDoDaoTao> HRM_dmTrinhDoDaoTao => Set<HRM_dmTrinhDoDaoTao>();
    public DbSet<HRM_dmNhomLuong> HRM_dmNhomLuong => Set<HRM_dmNhomLuong>();
    public DbSet<HRM_dmKhuVuc> HRM_dmKhuVuc => Set<HRM_dmKhuVuc>();
    public DbSet<HRM_dmKhuVucDiaDiem> HRM_dmKhuVucDiaDiem => Set<HRM_dmKhuVucDiaDiem>();
    public DbSet<HRM_dmCongTacPhi> HRM_dmCongTacPhi => Set<HRM_dmCongTacPhi>();
    public DbSet<HRM_dmTinh> HRM_dmTinh => Set<HRM_dmTinh>();
    public DbSet<HRM_dmQuanHuyen> HRM_dmQuanHuyen => Set<HRM_dmQuanHuyen>();
    public DbSet<HRM_dmPhuongXa> HRM_dmPhuongXa => Set<HRM_dmPhuongXa>();
    public DbSet<HRM_dmDiaDiemLamViec> HRM_dmDiaDiemLamViec => Set<HRM_dmDiaDiemLamViec>();
    public DbSet<HRM_dmLoaiTinhThueChuyenGia> HRM_dmLoaiTinhThueChuyenGia => Set<HRM_dmLoaiTinhThueChuyenGia>();
    public DbSet<HRM_dmCaiDatHeThong> HRM_dmCaiDatHeThong => Set<HRM_dmCaiDatHeThong>();
    public DbSet<HRM_dmTienTe> HRM_dmTienTe => Set<HRM_dmTienTe>();
    public DbSet<HRM_dmMucDongThueTNCN> HRM_dmMucDongThueTNCN => Set<HRM_dmMucDongThueTNCN>();
    public DbSet<HRM_dmHinhThucLamViec> HRM_dmHinhThucLamViec => Set<HRM_dmHinhThucLamViec>();
    public DbSet<HRM_dmChucVuCongViec> HRM_dmChucVuCongViec => Set<HRM_dmChucVuCongViec>();
    public DbSet<HRM_dmTrangThietBi> HRM_dmTrangThietBi => Set<HRM_dmTrangThietBi>();
    public DbSet<HRM_dmHinhThucDaoTao> HRM_dmHinhThucDaoTao => Set<HRM_dmHinhThucDaoTao>();
    public DbSet<HRM_dmNoiDungDaoTao> HRM_dmNoiDungDaoTao => Set<HRM_dmNoiDungDaoTao>();
    public DbSet<HRM_dmLichLamViec> HRM_dmLichLamViec => Set<HRM_dmLichLamViec>();
    public DbSet<HRM_dmLichLamViec_Ngay> HRM_dmLichLamViec_Ngay => Set<HRM_dmLichLamViec_Ngay>();
    public DbSet<HRM_dmLichLamViec_Gio> HRM_dmLichLamViec_Gio => Set<HRM_dmLichLamViec_Gio>();
    public DbSet<HRM_dmLichNghiLe> HRM_dmLichNghiLe => Set<HRM_dmLichNghiLe>();
    public DbSet<HRM_dmLichNghiLe_BoPhan> HRM_dmLichNghiLe_BoPhan => Set<HRM_dmLichNghiLe_BoPhan>();
    public DbSet<HRM_dmLichNghiLe_Ngay> HRM_dmLichNghiLe_Ngay => Set<HRM_dmLichNghiLe_Ngay>();
    public DbSet<HRM_dmThietLapBoPhan> HRM_dmThietLapBoPhan => Set<HRM_dmThietLapBoPhan>();
    public DbSet<HRM_dmKyHieuChamCong> HRM_dmKyHieuChamCong => Set<HRM_dmKyHieuChamCong>();
    public DbSet<HRM_dmHinhThucNghi> HRM_dmHinhThucNghi => Set<HRM_dmHinhThucNghi>();
    public DbSet<HRM_dmChucVuKiemSoatNoiBo> HRM_dmChucVuKiemSoatNoiBo => Set<HRM_dmChucVuKiemSoatNoiBo>();
    public DbSet<HRM_dmNganHang> HRM_dmNganHang => Set<HRM_dmNganHang>();
    public DbSet<HRM_ThuVienDanhGia> HRM_ThuVienDanhGia => Set<HRM_ThuVienDanhGia>();
    public DbSet<HRM_ThuVienDanhGia_Item> HRM_ThuVienDanhGia_Item => Set<HRM_ThuVienDanhGia_Item>();
    public DbSet<HRM_ThuVienDanhGia_NhanSu> HRM_ThuVienDanhGia_NhanSu => Set<HRM_ThuVienDanhGia_NhanSu>();
    public DbSet<HRM_dmNhomDanhGia> HRM_dmNhomDanhGia => Set<HRM_dmNhomDanhGia>();
    public DbSet<HRM_dmCapBacLuong_PhuCap> HRM_dmCapBacLuong_PhuCap => Set<HRM_dmCapBacLuong_PhuCap>();
    public DbSet<HRM_dmCapBacLuong> HRM_dmCapBacLuong => Set<HRM_dmCapBacLuong>();
    public DbSet<HRM_dmNhomCauHoi> HRM_dmNhomCauHoi => Set<HRM_dmNhomCauHoi>();
    public DbSet<HRM_dmCauHoi> HRM_dmCauHoi => Set<HRM_dmCauHoi>();
    public DbSet<HRM_dmCauHoi_Item> HRM_dmCauHoi_Item => Set<HRM_dmCauHoi_Item>();
    public DbSet<HRM_dmCauTrucDe> HRM_dmCauTrucDe => Set<HRM_dmCauTrucDe>();
    public DbSet<HRM_dmCauTrucDe_Item> HRM_dmCauTrucDe_Item => Set<HRM_dmCauTrucDe_Item>();
    public DbSet<HRM_dmPhuongTien> HRM_dmPhuongTien => Set<HRM_dmPhuongTien>();
    public DbSet<HRM_dmLoaiChiPhiCongTac> HRM_dmLoaiChiPhiCongTac => Set<HRM_dmLoaiChiPhiCongTac>();
    public DbSet<HRM_dmNoiCapBaoHiem> HRM_dmNoiCapBaoHiem => Set<HRM_dmNoiCapBaoHiem>();
    public DbSet<HRM_dmNoiCap> HRM_dmNoiCap => Set<HRM_dmNoiCap>();
    public DbSet<HRM_dmHeSoLamThemGio> HRM_dmHeSoLamThemGio => Set<HRM_dmHeSoLamThemGio>();
    public DbSet<HRM_dmQuyDinhCongTacPhi> HRM_dmQuyDinhCongTacPhi => Set<HRM_dmQuyDinhCongTacPhi>();
    public DbSet<HRM_NhanVien> HRM_NhanVien => Set<HRM_NhanVien>();
    public DbSet<HRM_NhanVien_FileDinhKemHoSo> HRM_NhanVien_FileDinhKemHoSo => Set<HRM_NhanVien_FileDinhKemHoSo>();
    public DbSet<HRM_NhanVienBangCap> HRM_NhanVienBangCap => Set<HRM_NhanVienBangCap>();
    public DbSet<HRM_NhanVienDieuChinhThongTin_ThongTin> HRM_NhanVienDieuChinhThongTin_ThongTin => Set<HRM_NhanVienDieuChinhThongTin_ThongTin>();
    public DbSet<HRM_NhanVien_Edit> HRM_NhanVien_Edit => Set<HRM_NhanVien_Edit>();
    public DbSet<HRM_NhanVienDieuChinhThongTin> HRM_NhanVienDieuChinhThongTin => Set<HRM_NhanVienDieuChinhThongTin>();
    public DbSet<HRM_NhanVien_PhuCap> HRM_NhanVien_PhuCap => Set<HRM_NhanVien_PhuCap>();
    public DbSet<HRM_ThanhVienHiepHoi> HRM_ThanhVienHiepHoi => Set<HRM_ThanhVienHiepHoi>();
    public DbSet<HRM_NgonNguNhanVien> HRM_NgonNguNhanVien => Set<HRM_NgonNguNhanVien>();
    public DbSet<HRM_NhanVien_KhamSucKhoe> HRM_NhanVien_KhamSucKhoe => Set<HRM_NhanVien_KhamSucKhoe>();
    public DbSet<HRM_NhanVien_BaoHiemTaiNan> HRM_NhanVien_BaoHiemTaiNan => Set<HRM_NhanVien_BaoHiemTaiNan>();
    public DbSet<HRM_QuaTrinhCongTac> HRM_QuaTrinhCongTac => Set<HRM_QuaTrinhCongTac>();
    public DbSet<HRM_QuaTrinhCongTacNgoaiCongTy> HRM_QuaTrinhCongTacNgoaiCongTy => Set<HRM_QuaTrinhCongTacNgoaiCongTy>();
    public DbSet<HRM_NhanVien_TaiKhoanNganHang> HRM_NhanVien_TaiKhoanNganHang => Set<HRM_NhanVien_TaiKhoanNganHang>();
    public DbSet<HRM_NhanVien_SoDienThoai> HRM_NhanVien_SoDienThoai => Set<HRM_NhanVien_SoDienThoai>();
    public DbSet<HRM_NhanVien_Email> HRM_NhanVien_Email => Set<HRM_NhanVien_Email>();
    public DbSet<HRM_NhanVien_GiayToTuyThan> HRM_NhanVien_GiayToTuyThan => Set<HRM_NhanVien_GiayToTuyThan>();
    public DbSet<HRM_ChungChiNhanVien> HRM_ChungChiNhanVien => Set<HRM_ChungChiNhanVien>();
    public DbSet<HRM_NhanVienQuanHeGiaDinh> HRM_NhanVienQuanHeGiaDinh => Set<HRM_NhanVienQuanHeGiaDinh>();
    public DbSet<HRM_BoNhiemDieuDongDuAn> HRM_BoNhiemDieuDongDuAn => Set<HRM_BoNhiemDieuDongDuAn>();
    public DbSet<HRM_BoNhiemChucVu> HRM_BoNhiemChucVu => Set<HRM_BoNhiemChucVu>();
    public DbSet<HRM_TheoDoiBoNhiemDieuDongDuAn> HRM_TheoDoiBoNhiemDieuDongDuAn => Set<HRM_TheoDoiBoNhiemDieuDongDuAn>();
    public DbSet<HRM_BoNhiemChucVu_BoPhan> HRM_BoNhiemChucVu_BoPhan => Set<HRM_BoNhiemChucVu_BoPhan>();
    public DbSet<HRM_QuyetDinhDieuChinhLuong> HRM_QuyetDinhDieuChinhLuong => Set<HRM_QuyetDinhDieuChinhLuong>();
    public DbSet<HRM_QuyetDinhDieuChinhLuong_NhanVien> HRM_QuyetDinhDieuChinhLuong_NhanVien => Set<HRM_QuyetDinhDieuChinhLuong_NhanVien>();
    public DbSet<HRM_QuyetDinhKhenThuong> HRM_QuyetDinhKhenThuong => Set<HRM_QuyetDinhKhenThuong>();
    public DbSet<HRM_dmHinhThucKhenThuong> HRM_dmHinhThucKhenThuong => Set<HRM_dmHinhThucKhenThuong>();
    public DbSet<HRM_QuyetDinhKhenThuongNhanVien> HRM_QuyetDinhKhenThuongNhanVien => Set<HRM_QuyetDinhKhenThuongNhanVien>();
    public DbSet<HRM_TheoDoiThuongNhanVien> HRM_TheoDoiThuongNhanVien => Set<HRM_TheoDoiThuongNhanVien>();
    public DbSet<HRM_QuyetDinhKhenThuongBoPhan> HRM_QuyetDinhKhenThuongBoPhan => Set<HRM_QuyetDinhKhenThuongBoPhan>();
    public DbSet<HRM_QuyetDinhKyLuat> HRM_QuyetDinhKyLuat => Set<HRM_QuyetDinhKyLuat>();
    public DbSet<HRM_dmHinhThucKyLuat> HRM_dmHinhThucKyLuat => Set<HRM_dmHinhThucKyLuat>();
    public DbSet<HRM_dmLoaiHinhThucPhatKyLuat> HRM_dmLoaiHinhThucPhatKyLuat => Set<HRM_dmLoaiHinhThucPhatKyLuat>();
    public DbSet<HRM_QuyetDinhKyLuatBoPhan> HRM_QuyetDinhKyLuatBoPhan => Set<HRM_QuyetDinhKyLuatBoPhan>();
    public DbSet<HRM_QuyetDinhKyLuatNhanVien> HRM_QuyetDinhKyLuatNhanVien => Set<HRM_QuyetDinhKyLuatNhanVien>();
    public DbSet<HRM_QuyetDinhSaThai> HRM_QuyetDinhSaThai => Set<HRM_QuyetDinhSaThai>();
    public DbSet<HRM_QuyetDinhSaThaiBoPhan> HRM_QuyetDinhSaThaiBoPhan => Set<HRM_QuyetDinhSaThaiBoPhan>();
    public DbSet<HRM_SaThaiTaiSanBanGiao> HRM_SaThaiTaiSanBanGiao => Set<HRM_SaThaiTaiSanBanGiao>();
    public DbSet<HRM_QuyetDinhNghiViec> HRM_QuyetDinhNghiViec => Set<HRM_QuyetDinhNghiViec>();
    public DbSet<HRM_QuyetDinhNghiViecTaiSanBanGiao> HRM_QuyetDinhNghiViecTaiSanBanGiao => Set<HRM_QuyetDinhNghiViecTaiSanBanGiao>();
    public DbSet<HRM_QuyetDinhNghiViecNhanVienBoPhan> HRM_QuyetDinhNghiViecNhanVienBoPhan => Set<HRM_QuyetDinhNghiViecNhanVienBoPhan>();
    public DbSet<HRM_QuaTrinhCongTacNhanVien> HRM_QuaTrinhCongTacNhanVien => Set<HRM_QuaTrinhCongTacNhanVien>();
    public DbSet<HRM_Luong_NhanVien> HRM_Luong_NhanVien => Set<HRM_Luong_NhanVien>();
    public DbSet<HRM_Luong_PhuCap_NhanVien> HRM_Luong_PhuCap_NhanVien => Set<HRM_Luong_PhuCap_NhanVien>();
    public DbSet<HRM_BangLuong> HRM_BangLuong => Set<HRM_BangLuong>();
    public DbSet<HRM_Link_BangLuong_ChamCong> HRM_Link_BangLuong_ChamCong => Set<HRM_Link_BangLuong_ChamCong>();
    public DbSet<HRM_BangLuong_ChiTietNhanVien> HRM_BangLuong_ChiTietNhanVien => Set<HRM_BangLuong_ChiTietNhanVien>();
    public DbSet<HRM_Luong_KhenThuongKyLuat> HRM_Luong_KhenThuongKyLuat => Set<HRM_Luong_KhenThuongKyLuat>();
    public DbSet<HRM_dmPhuCap> HRM_dmPhuCap => Set<HRM_dmPhuCap>();
    public DbSet<HRM_HopDongPhuCap> HRM_HopDongPhuCap => Set<HRM_HopDongPhuCap>();
    public DbSet<HRM_HopDongPhuCapBaoHiem> HRM_HopDongPhuCapBaoHiem => Set<HRM_HopDongPhuCapBaoHiem>();
    public DbSet<HRM_HopDong_QuyetDinh> HRM_HopDong_QuyetDinh => Set<HRM_HopDong_QuyetDinh>();
    public DbSet<HRM_BaoHiem_HopDong> HRM_BaoHiem_HopDong => Set<HRM_BaoHiem_HopDong>();
    public DbSet<HRM_HopDong_KhamSucKhoe> HRM_HopDong_KhamSucKhoe => Set<HRM_HopDong_KhamSucKhoe>();
    public DbSet<HRM_HopDong> HRM_HopDong => Set<HRM_HopDong>();
    public DbSet<HRM_HoSoDinhKem> HRM_HoSoDinhKem => Set<HRM_HoSoDinhKem>();
    public DbSet<HRM_HopDongHoSoDinhKem> HRM_HopDongHoSoDinhKem => Set<HRM_HopDongHoSoDinhKem>();
    public DbSet<HRM_dmTaiSanBanGiao> HRM_dmTaiSanBanGiao => Set<HRM_dmTaiSanBanGiao>();
    public DbSet<HRM_HopDongTaiSanBanGiao> HRM_HopDongTaiSanBanGiao => Set<HRM_HopDongTaiSanBanGiao>();
    public DbSet<HRM_HopDongCongViec> HRM_HopDongCongViec => Set<HRM_HopDongCongViec>();
    public DbSet<HRM_DeNghiTuyenDung> HRM_DeNghiTuyenDung => Set<HRM_DeNghiTuyenDung>();
    public DbSet<HRM_DeNghiTuyenDung_CaLamViec> HRM_DeNghiTuyenDung_CaLamViec => Set<HRM_DeNghiTuyenDung_CaLamViec>();
    public DbSet<HRM_DeNghiTuyenDung_NhanVien> HRM_DeNghiTuyenDung_NhanVien => Set<HRM_DeNghiTuyenDung_NhanVien>();
    public DbSet<HRM_DeNghiTuyenDung_BoPhan> HRM_DeNghiTuyenDung_BoPhan => Set<HRM_DeNghiTuyenDung_BoPhan>();
    public DbSet<HRM_DeNghiTuyenDung_CongNhan_PhuCap> HRM_DeNghiTuyenDung_CongNhan_PhuCap => Set<HRM_DeNghiTuyenDung_CongNhan_PhuCap>();
    public DbSet<HRM_DeNghiTuyenDung_CongNhan> HRM_DeNghiTuyenDung_CongNhan => Set<HRM_DeNghiTuyenDung_CongNhan>();
    public DbSet<HRM_TuyenDung_DinhBienCoSo> HRM_TuyenDung_DinhBienCoSo => Set<HRM_TuyenDung_DinhBienCoSo>();
    public DbSet<HRM_TuyenDung_DinhBienCoSo_CongTy> HRM_TuyenDung_DinhBienCoSo_CongTy => Set<HRM_TuyenDung_DinhBienCoSo_CongTy>();
    public DbSet<HRM_TuyenDung_NhuCau> HRM_TuyenDung_NhuCau => Set<HRM_TuyenDung_NhuCau>();
    public DbSet<HRM_TuyenDung_NhuCau_ChiTiet> HRM_TuyenDung_NhuCau_ChiTiet => Set<HRM_TuyenDung_NhuCau_ChiTiet>();
    public DbSet<HRM_TuyenDung_KeHoach> HRM_TuyenDung_KeHoach => Set<HRM_TuyenDung_KeHoach>();
    public DbSet<HRM_TuyenDung_KeHoach_ChiTiet> HRM_TuyenDung_KeHoach_ChiTiet => Set<HRM_TuyenDung_KeHoach_ChiTiet>();
    public DbSet<HRM_TuyenDung_KeHoach_TongHop> HRM_TuyenDung_KeHoach_TongHop => Set<HRM_TuyenDung_KeHoach_TongHop>();
    public DbSet<HRM_TuyenDung_KeHoach_TongHopChiTiet> HRM_TuyenDung_KeHoach_TongHopChiTiet => Set<HRM_TuyenDung_KeHoach_TongHopChiTiet>();
    public DbSet<HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung> HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung => Set<HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung>();
    public DbSet<HRM_TuyenDung_TongHop_KeHoach> HRM_TuyenDung_TongHop_KeHoach => Set<HRM_TuyenDung_TongHop_KeHoach>();
    public DbSet<HRM_TuyenDung_DeNghi> HRM_TuyenDung_DeNghi => Set<HRM_TuyenDung_DeNghi>();
    public DbSet<HRM_TuyenDung_DeNghi_ChiTiet> HRM_TuyenDung_DeNghi_ChiTiet => Set<HRM_TuyenDung_DeNghi_ChiTiet>();
    public DbSet<HRM_TuyenDung_DeNghi_TongHop> HRM_TuyenDung_DeNghi_TongHop => Set<HRM_TuyenDung_DeNghi_TongHop>();
    public DbSet<HRM_TuyenDung_DeNghi_TongHopChiTiet> HRM_TuyenDung_DeNghi_TongHopChiTiet => Set<HRM_TuyenDung_DeNghi_TongHopChiTiet>();
    public DbSet<HRM_TuyenDung_DeNghi_TongHop_Dot> HRM_TuyenDung_DeNghi_TongHop_Dot => Set<HRM_TuyenDung_DeNghi_TongHop_Dot>();
    public DbSet<HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon> HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon => Set<HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon>();
    public DbSet<HRM_TuyenDung_QuyTrinh> HRM_TuyenDung_QuyTrinh => Set<HRM_TuyenDung_QuyTrinh>();
    public DbSet<HRM_TuyenDung_UngVien> HRM_TuyenDung_UngVien => Set<HRM_TuyenDung_UngVien>();
    public DbSet<HRM_UngVienQuanHeGiaDinh> HRM_UngVienQuanHeGiaDinh => Set<HRM_UngVienQuanHeGiaDinh>();
    public DbSet<HRM_UngVienBangCap> HRM_UngVienBangCap => Set<HRM_UngVienBangCap>();
    public DbSet<HRM_NgonNguUngVien> HRM_NgonNguUngVien => Set<HRM_NgonNguUngVien>();
    public DbSet<HRM_ChungChiUngVien> HRM_ChungChiUngVien => Set<HRM_ChungChiUngVien>();
    public DbSet<HRM_TuyenDung_UngVienKinhNghiemLamViec> HRM_TuyenDung_UngVienKinhNghiemLamViec => Set<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
    public DbSet<HRM_Link_TuyenDungQuyTrinh_UngVien> HRM_Link_TuyenDungQuyTrinh_UngVien => Set<HRM_Link_TuyenDungQuyTrinh_UngVien>();
    public DbSet<HRM_TuyenDung_UngVien_SoVan> HRM_TuyenDung_UngVien_SoVan => Set<HRM_TuyenDung_UngVien_SoVan>();
    public DbSet<HRM_TuyenDung_UngVien_SoVanTieuChi> HRM_TuyenDung_UngVien_SoVanTieuChi => Set<HRM_TuyenDung_UngVien_SoVanTieuChi>();
    public DbSet<HRM_TuyenDung_UngVien_SoVanChiTiet> HRM_TuyenDung_UngVien_SoVanChiTiet => Set<HRM_TuyenDung_UngVien_SoVanChiTiet>();
    public DbSet<HRM_TuyenDung_VongPhongVan> HRM_TuyenDung_VongPhongVan => Set<HRM_TuyenDung_VongPhongVan>();
    public DbSet<HRM_TuyenDung_PhuCap> HRM_TuyenDung_PhuCap => Set<HRM_TuyenDung_PhuCap>();
    public DbSet<HRM_UngVien_PhuCap> HRM_UngVien_PhuCap => Set<HRM_UngVien_PhuCap>();
    public DbSet<HRM_TuyenDung_QuyTrinhPhongVan> HRM_TuyenDung_QuyTrinhPhongVan => Set<HRM_TuyenDung_QuyTrinhPhongVan>();
    public DbSet<HRM_TuyenDung_NguoiPhongVan> HRM_TuyenDung_NguoiPhongVan => Set<HRM_TuyenDung_NguoiPhongVan>();
    public DbSet<HRM_TuyenDung_QuyTrinhPheDuyetLuong> HRM_TuyenDung_QuyTrinhPheDuyetLuong => Set<HRM_TuyenDung_QuyTrinhPheDuyetLuong>();
    public DbSet<HRM_TuyenDung_PheDuyetLuong_UngVien> HRM_TuyenDung_PheDuyetLuong_UngVien => Set<HRM_TuyenDung_PheDuyetLuong_UngVien>();
    public DbSet<HRM_PheDuyetLuong_UngVien_PhuCap> HRM_PheDuyetLuong_UngVien_PhuCap => Set<HRM_PheDuyetLuong_UngVien_PhuCap>();
    public DbSet<HRM_TuyenDung_PheDuyet> HRM_TuyenDung_PheDuyet => Set<HRM_TuyenDung_PheDuyet>();
    public DbSet<HRM_TuyenDung_ThuMoiLamViec> HRM_TuyenDung_ThuMoiLamViec => Set<HRM_TuyenDung_ThuMoiLamViec>();
    public DbSet<HRM_KeHoachDaoTao> HRM_KeHoachDaoTao => Set<HRM_KeHoachDaoTao>();
    public DbSet<HRM_KeHoachDaoTaoChiTiet> HRM_KeHoachDaoTaoChiTiet => Set<HRM_KeHoachDaoTaoChiTiet>();
    public DbSet<HRM_KeHoachDaoTaoNhanVien> HRM_KeHoachDaoTaoNhanVien => Set<HRM_KeHoachDaoTaoNhanVien>();
    public DbSet<HRM_KeHoachDaoTaoTongHop> HRM_KeHoachDaoTaoTongHop => Set<HRM_KeHoachDaoTaoTongHop>();
    public DbSet<HRM_TongHop_KeHoachDaoTaoChiTiet> HRM_TongHop_KeHoachDaoTaoChiTiet => Set<HRM_TongHop_KeHoachDaoTaoChiTiet>();
    public DbSet<HRM_KeHoachDaoTao_TongHop> HRM_KeHoachDaoTao_TongHop => Set<HRM_KeHoachDaoTao_TongHop>();
    public DbSet<HRM_TongHop_KeHoachDaoTaoNhanVien> HRM_TongHop_KeHoachDaoTaoNhanVien => Set<HRM_TongHop_KeHoachDaoTaoNhanVien>();
    public DbSet<HRM_Dot_DaoTao> HRM_Dot_DaoTao => Set<HRM_Dot_DaoTao>();
    public DbSet<HRM_Dot_DaoTaoChiTiet> HRM_Dot_DaoTaoChiTiet => Set<HRM_Dot_DaoTaoChiTiet>();
    public DbSet<HRM_Dot_DaoTaoNhanVien> HRM_Dot_DaoTaoNhanVien => Set<HRM_Dot_DaoTaoNhanVien>();
    public DbSet<HRM_Dot_DaoTao_NoiDung> HRM_Dot_DaoTao_NoiDung => Set<HRM_Dot_DaoTao_NoiDung>();
    public DbSet<HRM_Dot_DaoTaoChiPhi> HRM_Dot_DaoTaoChiPhi => Set<HRM_Dot_DaoTaoChiPhi>();
    public DbSet<HRM_ChuongTrinhDaoTao> HRM_ChuongTrinhDaoTao => Set<HRM_ChuongTrinhDaoTao>();
    public DbSet<HRM_ChuongTrinhDaoTao_NoiDung> HRM_ChuongTrinhDaoTao_NoiDung => Set<HRM_ChuongTrinhDaoTao_NoiDung>();
    public DbSet<HRM_ChuongTrinhDaoTao_NhanVien> HRM_ChuongTrinhDaoTao_NhanVien => Set<HRM_ChuongTrinhDaoTao_NhanVien>();
    public DbSet<HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian> HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian => Set<HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian>();
    public DbSet<HRM_ChuongTrinhDaoTao_NhanVien_NoiDung> HRM_ChuongTrinhDaoTao_NhanVien_NoiDung => Set<HRM_ChuongTrinhDaoTao_NhanVien_NoiDung>();
    public DbSet<HRM_DotThi> HRM_DotThi => Set<HRM_DotThi>();
    public DbSet<HRM_DotThi_NhanVien> HRM_DotThi_NhanVien => Set<HRM_DotThi_NhanVien>();
    public DbSet<HRM_BaiThi> HRM_BaiThi => Set<HRM_BaiThi>();
    public DbSet<HRM_BaiThi_Item> HRM_BaiThi_Item => Set<HRM_BaiThi_Item>();
    public DbSet<HRM_KhaoSatDaoTao> HRM_KhaoSatDaoTao => Set<HRM_KhaoSatDaoTao>();
    public DbSet<HRM_KhaoSatDaoTao_CauHoi> HRM_KhaoSatDaoTao_CauHoi => Set<HRM_KhaoSatDaoTao_CauHoi>();
    public DbSet<HRM_KhaoSatDaoTao_NhanVien> HRM_KhaoSatDaoTao_NhanVien => Set<HRM_KhaoSatDaoTao_NhanVien>();
    public DbSet<HRM_NhatKyNhanVien> HRM_NhatKyNhanVien => Set<HRM_NhatKyNhanVien>();
    public DbSet<HRM_KeHoachPhucLoi> HRM_KeHoachPhucLoi => Set<HRM_KeHoachPhucLoi>();
    public DbSet<HRM_KeHoachPhucLoiChiTiet> HRM_KeHoachPhucLoiChiTiet => Set<HRM_KeHoachPhucLoiChiTiet>();
    public DbSet<HRM_KeHachPhucLoi_NhanVien> HRM_KeHachPhucLoi_NhanVien => Set<HRM_KeHachPhucLoi_NhanVien>();
    public DbSet<HRM_QuyTrinh_PhucLoi> HRM_QuyTrinh_PhucLoi => Set<HRM_QuyTrinh_PhucLoi>();
    public DbSet<HRM_QuyTrinh_PhucLoiChiTiet> HRM_QuyTrinh_PhucLoiChiTiet => Set<HRM_QuyTrinh_PhucLoiChiTiet>();
    public DbSet<HRM_QuyTrinh_PhucLoi_NhanVien> HRM_QuyTrinh_PhucLoi_NhanVien => Set<HRM_QuyTrinh_PhucLoi_NhanVien>();
    public DbSet<HRM_DieuKienPhucLoi> HRM_DieuKienPhucLoi => Set<HRM_DieuKienPhucLoi>();
    public DbSet<HRM_PhucLoiChiPhi> HRM_PhucLoiChiPhi => Set<HRM_PhucLoiChiPhi>();
    public DbSet<HRM_dmNhomPhucLoi> HRM_dmNhomPhucLoi => Set<HRM_dmNhomPhucLoi>();
    public DbSet<HRM_ChamCong_Detail> HRM_ChamCong_Detail => Set<HRM_ChamCong_Detail>();
    public DbSet<HRM_ChamCong_BoPhan> HRM_ChamCong_BoPhan => Set<HRM_ChamCong_BoPhan>();
    public DbSet<HRM_ChamCong_NhanVien_BoPhan> HRM_ChamCong_NhanVien_BoPhan => Set<HRM_ChamCong_NhanVien_BoPhan>();
    public DbSet<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> HRM_ChamCong_NhanVien_ThoiGian_BoPhan => Set<HRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
    public DbSet<HRM_ChamCong_CongTy> HRM_ChamCong_CongTy => Set<HRM_ChamCong_CongTy>();
    public DbSet<HRM_DanhMuc_NhanVien_ChamCongBoPhan> HRM_DanhMuc_NhanVien_ChamCongBoPhan => Set<HRM_DanhMuc_NhanVien_ChamCongBoPhan>();
    public DbSet<HRM_dmDotChamCongThang> HRM_dmDotChamCongThang => Set<HRM_dmDotChamCongThang>();
    public DbSet<HRM_QuyTrinhXinDiMuonVeSom_NhanVien> HRM_QuyTrinhXinDiMuonVeSom_NhanVien => Set<HRM_QuyTrinhXinDiMuonVeSom_NhanVien>();
    public DbSet<HRM_QuyTrinhXinDiMuonVeSom> HRM_QuyTrinhXinDiMuonVeSom => Set<HRM_QuyTrinhXinDiMuonVeSom>();
    public DbSet<HRM_TheoDoiDiMuonVeSom> HRM_TheoDoiDiMuonVeSom => Set<HRM_TheoDoiDiMuonVeSom>();
    public DbSet<HRM_BangLuong_ChiTietNhanVienDA> HRM_BangLuong_ChiTietNhanVienDA => Set<HRM_BangLuong_ChiTietNhanVienDA>();
    public DbSet<HRM_BangLuong_PhuCap> HRM_BangLuong_PhuCap => Set<HRM_BangLuong_PhuCap>();
    public DbSet<HRM_Luong_HopDong> HRM_Luong_HopDong => Set<HRM_Luong_HopDong>();
    public DbSet<HRM_LuongBaoHiem_HopDong> HRM_LuongBaoHiem_HopDong => Set<HRM_LuongBaoHiem_HopDong>();
    public DbSet<HRM_QuyetToanPhepNam> HRM_QuyetToanPhepNam => Set<HRM_QuyetToanPhepNam>();
    public DbSet<HRM_QuyetToanPhepNam_NhanVien> HRM_QuyetToanPhepNam_NhanVien => Set<HRM_QuyetToanPhepNam_NhanVien>();
    public DbSet<HRM_DinhBien> HRM_DinhBien => Set<HRM_DinhBien>();
    public DbSet<HRM_QuyTrinhDinhBien> HRM_QuyTrinhDinhBien => Set<HRM_QuyTrinhDinhBien>();
    public DbSet<HRM_QuyTrinhDinhBien_ChiTiet> HRM_QuyTrinhDinhBien_ChiTiet => Set<HRM_QuyTrinhDinhBien_ChiTiet>();
    public DbSet<HRM_dmTieuChi> HRM_dmTieuChi => Set<HRM_dmTieuChi>();
    public DbSet<HRM_dmTieuChi_CauHoi> HRM_dmTieuChi_CauHoi => Set<HRM_dmTieuChi_CauHoi>();
    public DbSet<HRM_QuyTrinhBoPhan_BoPhan> HRM_QuyTrinhBoPhan_BoPhan => Set<HRM_QuyTrinhBoPhan_BoPhan>();
    public DbSet<HRM_KeHoachLamViecTuan_NhanVien> HRM_KeHoachLamViecTuan_NhanVien => Set<HRM_KeHoachLamViecTuan_NhanVien>();
    public DbSet<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> HRM_KeHoachLamViecTuan_NhanVien_ThoiGian => Set<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
    public DbSet<HRM_TheoDoiLichLamViecNhanVien> HRM_TheoDoiLichLamViecNhanVien => Set<HRM_TheoDoiLichLamViecNhanVien>();
    public DbSet<HRM_ThoiGianLamViecKeHoach> HRM_ThoiGianLamViecKeHoach => Set<HRM_ThoiGianLamViecKeHoach>();
    public DbSet<HRM_KeHoachLamViecTuan> HRM_KeHoachLamViecTuan => Set<HRM_KeHoachLamViecTuan>();
    public DbSet<HRM_XacNhanDiMuonVeSom> HRM_XacNhanDiMuonVeSom => Set<HRM_XacNhanDiMuonVeSom>();
    public DbSet<HRM_XacNhanDiMuonVeSom_NhanVien> HRM_XacNhanDiMuonVeSom_NhanVien => Set<HRM_XacNhanDiMuonVeSom_NhanVien>();
    public DbSet<HRM_CheckInOut> HRM_CheckInOut => Set<HRM_CheckInOut>();
    public DbSet<HRM_QuyTrinhDanhGia> HRM_QuyTrinhDanhGia => Set<HRM_QuyTrinhDanhGia>();
    public DbSet<HRM_QuyTrinhDanhGia_NhanVien> HRM_QuyTrinhDanhGia_NhanVien => Set<HRM_QuyTrinhDanhGia_NhanVien>();
    public DbSet<HRM_DanhGiaNhanVien> HRM_DanhGiaNhanVien => Set<HRM_DanhGiaNhanVien>();
    public DbSet<HRM_DanhGiaNhanVien_Item> HRM_DanhGiaNhanVien_Item => Set<HRM_DanhGiaNhanVien_Item>();
    public DbSet<HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec> HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec => Set<HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec>();
    public DbSet<HRM_TongHopXacNhanNgoaiGio> HRM_TongHopXacNhanNgoaiGio => Set<HRM_TongHopXacNhanNgoaiGio>();
    public DbSet<HRM_QuyTrinhXacNhanNgoaiGio> HRM_QuyTrinhXacNhanNgoaiGio => Set<HRM_QuyTrinhXacNhanNgoaiGio>();
    public DbSet<HRM_BaoCaoCongViec_NhanVien> HRM_BaoCaoCongViec_NhanVien => Set<HRM_BaoCaoCongViec_NhanVien>();
    public DbSet<HRM_BaoCaoCongViec_Item> HRM_BaoCaoCongViec_Item => Set<HRM_BaoCaoCongViec_Item>();
    public DbSet<HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio> HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio => Set<HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio>();
    public DbSet<HRM_TheoDoiLamThemGio> HRM_TheoDoiLamThemGio => Set<HRM_TheoDoiLamThemGio>();
    public DbSet<HRM_QuyTrinhLamThemGio_ChiTiet> HRM_QuyTrinhLamThemGio_ChiTiet => Set<HRM_QuyTrinhLamThemGio_ChiTiet>();
    public DbSet<HRM_QuyTrinhLamThemGio> HRM_QuyTrinhLamThemGio => Set<HRM_QuyTrinhLamThemGio>();
    public DbSet<HRM_QuyTrinhXinNghiPhep> HRM_QuyTrinhXinNghiPhep => Set<HRM_QuyTrinhXinNghiPhep>();
    public DbSet<HRM_QuyTrinhXinNghiPhep_ChiTiet> HRM_QuyTrinhXinNghiPhep_ChiTiet => Set<HRM_QuyTrinhXinNghiPhep_ChiTiet>();
    public DbSet<HRM_TheoDoiXinNghiPhep> HRM_TheoDoiXinNghiPhep => Set<HRM_TheoDoiXinNghiPhep>();
    public DbSet<HRM_QuyTrinhQuyetDinhKhenThuong> HRM_QuyTrinhQuyetDinhKhenThuong => Set<HRM_QuyTrinhQuyetDinhKhenThuong>();
    public DbSet<HRM_QuyTrinhQuyetDinhKhenThuong_ChiTiet> HRM_QuyTrinhQuyetDinhKhenThuong_ChiTiet => Set<HRM_QuyTrinhQuyetDinhKhenThuong_ChiTiet>();
    public DbSet<HRM_BangChamCong_NhanVien_ThoiGian> HRM_BangChamCong_NhanVien_ThoiGian => Set<HRM_BangChamCong_NhanVien_ThoiGian>();
    public DbSet<HRM_BangChamCongNhanVien_TangCa> HRM_BangChamCongNhanVien_TangCa => Set<HRM_BangChamCongNhanVien_TangCa>();
    public DbSet<HRM_BangChamCongNhanVien_SoGioLamViec> HRM_BangChamCongNhanVien_SoGioLamViec => Set<HRM_BangChamCongNhanVien_SoGioLamViec>();
    public DbSet<HRM_DienGiaiChamCong> HRM_DienGiaiChamCong => Set<HRM_DienGiaiChamCong>();
    public DbSet<HRM_BangChamCong> HRM_BangChamCong => Set<HRM_BangChamCong>();
    public DbSet<HRM_BangChamCong_NhanVien> HRM_BangChamCong_NhanVien => Set<HRM_BangChamCong_NhanVien>();
    public DbSet<HRM_QuyTrinhTinhLuong> HRM_QuyTrinhTinhLuong => Set<HRM_QuyTrinhTinhLuong>();
    public DbSet<HRM_QuyTrinhTinhLuong_NhanVien> HRM_QuyTrinhTinhLuong_NhanVien => Set<HRM_QuyTrinhTinhLuong_NhanVien>();
    public DbSet<HRM_QuyTrinhTinhLuong_TangCa> HRM_QuyTrinhTinhLuong_TangCa => Set<HRM_QuyTrinhTinhLuong_TangCa>();
    public DbSet<HRM_QuyTrinhTinhLuong_SoGioLamViec> HRM_QuyTrinhTinhLuong_SoGioLamViec => Set<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
    public DbSet<HRM_QuyTrinhTinhLuong_BaoHiemNgoai> HRM_QuyTrinhTinhLuong_BaoHiemNgoai => Set<HRM_QuyTrinhTinhLuong_BaoHiemNgoai>();
    public DbSet<HRM_QuyTrinhTinhLuong_ChamCong> HRM_QuyTrinhTinhLuong_ChamCong => Set<HRM_QuyTrinhTinhLuong_ChamCong>();
    public DbSet<HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu> HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu => Set<HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu>();
    public DbSet<HRM_QuyTrinhTinhLuong_PhuCap> HRM_QuyTrinhTinhLuong_PhuCap => Set<HRM_QuyTrinhTinhLuong_PhuCap>();
    public DbSet<HRM_CapPhatThuHoiTaiSan> HRM_CapPhatThuHoiTaiSan => Set<HRM_CapPhatThuHoiTaiSan>();
    public DbSet<HRM_CapPhatThuHoiTaiSan_Item> HRM_CapPhatThuHoiTaiSan_Item => Set<HRM_CapPhatThuHoiTaiSan_Item>();
    public DbSet<HRM_KeHoachCongTac> HRM_KeHoachCongTac => Set<HRM_KeHoachCongTac>();
    public DbSet<HRM_KetQuaCongTac> HRM_KetQuaCongTac => Set<HRM_KetQuaCongTac>();
    public DbSet<HRM_KetQuaCongTac_NhanSuTuyen> HRM_KetQuaCongTac_NhanSuTuyen => Set<HRM_KetQuaCongTac_NhanSuTuyen>();
    public DbSet<HRM_KetQuaCongTac_NhanSuCongTac> HRM_KetQuaCongTac_NhanSuCongTac => Set<HRM_KetQuaCongTac_NhanSuCongTac>();
    public DbSet<HRM_KetQuaCongTac_ChiPhi> HRM_KetQuaCongTac_ChiPhi => Set<HRM_KetQuaCongTac_ChiPhi>();
    public DbSet<HRM_KeHoachCongTac_NhanSuCongTac> HRM_KeHoachCongTac_NhanSuCongTac => Set<HRM_KeHoachCongTac_NhanSuCongTac>();
    public DbSet<HRM_KeHoachCongTac_ChiPhi> HRM_KeHoachCongTac_ChiPhi => Set<HRM_KeHoachCongTac_ChiPhi>();
    public DbSet<HRM_QuaTrinhThamGiaBaoHiemXaHoi> HRM_QuaTrinhThamGiaBaoHiemXaHoi => Set<HRM_QuaTrinhThamGiaBaoHiemXaHoi>();
    public DbSet<HRM_QuaTrinhThamGiaBaoHiemThatNghiep> HRM_QuaTrinhThamGiaBaoHiemThatNghiep => Set<HRM_QuaTrinhThamGiaBaoHiemThatNghiep>();
    public DbSet<HRM_ThongTinHuongCheDoThaiSan> HRM_ThongTinHuongCheDoThaiSan => Set<HRM_ThongTinHuongCheDoThaiSan>();
    public DbSet<HRM_ThongTinBaoHiemNgoai> HRM_ThongTinBaoHiemNgoai => Set<HRM_ThongTinBaoHiemNgoai>();
    public DbSet<HRM_QuyTrinhXacNhanTangGiamBaoHiem> HRM_QuyTrinhXacNhanTangGiamBaoHiem => Set<HRM_QuyTrinhXacNhanTangGiamBaoHiem>();
    public DbSet<HRM_GiamBaoHiem> HRM_GiamBaoHiem => Set<HRM_GiamBaoHiem>();
    public DbSet<HRM_TangBaoHiem> HRM_TangBaoHiem => Set<HRM_TangBaoHiem>();
    public DbSet<HRM_dmThietLapBaoCaoCongViec_ChiTiet> HRM_dmThietLapBaoCaoCongViec_ChiTiet => Set<HRM_dmThietLapBaoCaoCongViec_ChiTiet>();
    public DbSet<HRM_dmThietLapBaoCaoCongViec> HRM_dmThietLapBaoCaoCongViec => Set<HRM_dmThietLapBaoCaoCongViec>();
    public DbSet<HRM_dmThietLapBangLuong> HRM_dmThietLapBangLuong => Set<HRM_dmThietLapBangLuong>();
    public DbSet<HRM_dmThietLapBangLuong_ChiTiet> HRM_dmThietLapBangLuong_ChiTiet => Set<HRM_dmThietLapBangLuong_ChiTiet>();
    public DbSet<HRM_dmLabelThietLapBangLuong> HRM_dmLabelThietLapBangLuong => Set<HRM_dmLabelThietLapBangLuong>();
    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Giữ nguyên tên bảng gốc trong SQL Server giống EF6 (tránh tự động pluralize)
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var tableName = entityType.ClrType.Name;
            if (tableName.StartsWith("HRM_", StringComparison.Ordinal))
            {
                entityType.SetTableName(tableName);
            }
        }
    }
}
