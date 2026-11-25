﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using HRM.Api.Models.ChamCongModel;
using HRM.Api.Entity;
using HRM.Api.Entity.DinhBien;
using HRM.Api.Helper;
using HRM.Api.Services;
//using HRM.Api.Models.SmartEOEModel;

namespace HRM.Api.Models
{
    #region Chấm Công new

    public class HRM_QuyTrinhXinDiMuonVeSom : AudiQuyTrinh
    {
        public long IdCongTyEOS { get; set; }
        public string IdBoPhan { get; set; }

        public DateTime? NgayDangKy { get; set; }

        public bool DangKyHo { get; set; } = false;
        [NotMapped]
        public List<long> listIdBoPhan { get; set; }

        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public string NoiDungNghi { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public double NgayDangKyUnix { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhXinDiMuonVeSom_NhanVien> listNhanVien { get; set; }
    }
    public class HRM_QuyTrinhXinDiMuonVeSom_NhanVien : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public long IdCongTyEOS { get; set; }
        public long IdBoPhanEOS { get; set; }

        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public double TongGio { get; set; }
        public string LyDo { get; set; }
        public long IdLoai { get; set; }

        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public double TuGioUnix { get; set; }

        [NotMapped]
        public double DenGioUnix { get; set; }

        public void UnixToDateTime()
        {
            if (TuGioUnix == 0)
                TuGio = null;
            else
                TuGio = dungchung.UnixTimeStampToDateTime(TuGioUnix);
            if (DenGioUnix == 0)
                DenGio = null;
            else
                DenGio = dungchung.UnixTimeStampToDateTime(DenGioUnix);

        }
        public void DateTimetoUnix()
        {
            if (TuGio == null)
                TuGioUnix = 0;
            else
                TuGioUnix = dungchung.DateTimeToUnixTimeStamp(TuGio);
            if (DenGio == null)
                DenGioUnix = 0;
            else
                DenGioUnix = dungchung.DateTimeToUnixTimeStamp(DenGio);

        }


    }

    public class HRM_TheoDoiDiMuonVeSom : Audit
    {
        public string IdNhanVien { get; set; }
        public long IdCongTyEOS { get; set; }
        public long IdBoPhanEOS { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public double TongGio { get; set; }
        public string LyDo { get; set; }
        public long IdLoai { get; set; }// 1 đi muộn, 2 về sớm, 3 giữa giờ
        public HRM_TheoDoiDiMuonVeSom()
        {

        }
        public HRM_TheoDoiDiMuonVeSom(HRM_QuyTrinhXinDiMuonVeSom_NhanVien a)
        {
            IdLoai = a.IdLoai;
            LyDo = a.LyDo;
            TongGio = a.TongGio;
            DenGio = a.DenGio;
            TuGio = a.TuGio;
            IdBoPhanEOS = a.IdBoPhanEOS;
            IdCongTyEOS = a.IdCongTyEOS;
            IdNhanVien = a.IdNhanVien;
        }
        public HRM_TheoDoiDiMuonVeSom(HRM_XacNhanDiMuonVeSom_NhanVien a)
        {
            IdLoai = 1;
            TongGio = a.SoPhutChapNhan / 60.0;
            NgayDangKy = a.Ngay.Date;
            IdBoPhanEOS = a.IdBoPhan;
            IdCongTyEOS = a.IdCongTy;
            IdNhanVien = a.IdNhanVien;
        }

    }


    public class HRM_DienGiaiChamCong : AudiQuyTrinh
    {
        //public long IdDuAn { get; set; }
        public string IdUser { get; set; }
        public string IdNhanVien { get; set; }
        public DateTime ThoiGian { get; set; }
        public int Loai { get; set; } //0 Chấm công đến, 1 Chấm công về, 2 đi khách hàng  
        [NotMapped]
        public string TenNguoiTao { get; set; }
        [NotMapped]
        public double ThoiGianUnix { get; set; }
    }
    public class HRM_BangChamCong : AudiQuyTrinh
    {

        public string idChamCongCongTy { get; set; }
        public double NgayCongCoSo { get; set; }
        public long IdCongTyEOS { get; set; }

        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
        [NotMapped]
        public List<NgayChamCong> lstNgayChamCongTrongKy { get; set; }
        [NotMapped]
        public List<HRM_BangChamCong_NhanVien> listNhanVien { get; set; }
        [NotMapped]
        public List<HRM_BangChamCong_NhanVienViewModel> listBoPhan { get; set; }

        [NotMapped]
        public List<HRM_BangChamCong_NhanVien_Ngay> listChamCong { get; set; } = new List<HRM_BangChamCong_NhanVien_Ngay>();
        [NotMapped]
        public string TenCongty { set; get; }
        public void DateTimeToUnix()
        {
            if (TuNgay == null)
                TuNgayUnix = 0;
            else
                TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(TuNgay);
            if (DenNgay == null)
                DenNgayUnix = 0;
            else
                DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(DenNgay);

        }
        public void UnixToDateTime()
        {
            if (TuNgayUnix == 0)
                TuNgay = null;
            else
                TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
            if (DenNgayUnix == 0)
                DenNgay = null;
            else
                DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
        }
    }
    public class HRM_BangChamCong_NhanVienViewModel
    {
        public string TenBoPhan { get; set; }
        public List<HRM_BangChamCong_NhanVien> listNhanVien { get; set; }
        public List<HRM_BangChamCong_NhanVienViewModel> lischucDanh { get; set; }
    }
    public class HRM_BangChamCong_NhanVien : Audit
    {
        public string IdBangChamCong { get; set; }
        public string IdNhanVien { get; set; }
        public long IdBoPhanEOS { get; set; }
        public long IdCongTyEOS { get; set; }
        public double? TongCongPhep { get; set; }
        public double? TongSoCong { get; set; }
        public string TongSoCongChu { get; set; }
        public double? TongSoCongTangCa { get; set; }
        public double? TongSoCongDem { get; set; }
        public double? TongSoCong150 { get; set; }
        public double? TongSoCong200 { get; set; }
        public string TongSoCong300 { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public bool isKetThuc { get; set; }
        //public int Tong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public DateTime NgaySinh { get; set; }

        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public double SoGioTrongNgay { get; set; }
        [NotMapped]
        public double SoCongTieuChuan { get; set; }
        [NotMapped]
        public double SoGioTangCa { get; set; }

        [NotMapped]
        public bool isDieuDong { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string LinkAnhDaiDien { get; set; }
        [NotMapped]
        public DateTime? NgayVaoLam { get; set; }

        [NotMapped]
        public List<HRM_BangChamCong_NhanVien_ThoiGian> listNgay { get; set; }


        [NotMapped]
        public List<HRM_BangChamCongNhanVien_TangCa> listTangCa { get; set; }
        [NotMapped]
        public List<HRM_BangChamCongNhanVien_SoGioLamViec> listGioTieuChuan { get; set; }

    }
    public class HRM_BangChamCong_NhanVienView
    {

        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public List<HRM_BangChamCong_NhanVien_Ngay> listNgay { get; set; }
    }
    public class HRM_BangChamCong_NhanVien_Ngay
    {
        public string IdBangChamCong { get; set; }
        public string IdNhanVien { get; set; }
        public string LoaiThoiGian { get; set; }
        public string ChucVu { get; set; }
        public DateTime Ngay { get; set; }
        public DateTime? TuGio { get; set; }
        public string NoiChamTuGio { get; set; }
        public string NoiChamDenGio { get; set; }
        public DateTime? DenGio { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string TenLoaiNhanVien { get; set; }
        public string TenBoPhan { get; set; }
        public string TenCongTy { get; set; }
        public string TenChucVu { get; set; }
        public string TenChucDanh { get; set; }
        public string CaLamViec { get; set; }
        public string ThoiGianLamViec { get; set; }
        public double SoGioQuyDoi { get; set; }
        public double SoCongQuyDoi { get; set; }
        public double CongF { get; set; }
        public double CongP { get; set; }
        public double CongTangCa { get; set; }
        public double CongPhep { get; set; }
        public double CongLe { get; set; }
        public string SoGioDiMuon { get; set; }
        public string SoGioVeSom { get; set; }
    }

    public class HRM_BangChamCong_NhanVien_ThoiGian : Audit
    {
        public bool isKetThuc { get; set; }
        public string IdBangChamCong { get; set; }
        public string IdNhanVien { get; set; }
        public long IdBoPhanEOS { get; set; }
        public long IdCongTyEOS { get; set; }
        public DateTime? NgayChamCong { get; set; }
        [NotMapped]
        public int Ngay { get; set; }
        [NotMapped]
        public int Thang { get; set; }
        [NotMapped]
        public int Nam { get; set; }
        public DateTime? GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public string GioLamViec { get; set; }
        public double? SoGioLamViec { get; set; }
        public double? SoGioLamViecCaGay { get; set; }
        public double? SoGioLamViecTangCa { get; set; }
        public string IdKyHieuChamCong { get; set; }

        [NotMapped]
        public double NgayChamCongUnix { get; set; }
        [NotMapped]
        public string IddmHeSoTangCa { get; set; }

        [NotMapped]
        public double GioVaoUnix { get; set; }
        public double? SoGioTieuChuan { get; set; }
        [NotMapped]
        public long IdCongTyTangCuong { get; set; }
        [NotMapped]
        public string KyHieuTC { get; set; }

        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public bool isFake { get; set; } = false;
        [NotMapped]
        public string Color { get; set; }


        [NotMapped]
        public bool isNghiPhep { get; set; } = false;

        public bool isNgayLe { get; set; } = false;
        public bool isCaDem { get; set; } = false;

        [NotMapped]
        public double GioRaUnix { get; set; }
        public void DateTimeToUnix()
        {
            if (NgayChamCong == null)
                NgayChamCongUnix = 0;
            else
                NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(NgayChamCong);
            if (GioVao == null)
                GioVaoUnix = 0;
            else
                GioVaoUnix = dungchung.DateTimeToUnixTimeStamp(GioVao);
            if (GioRa == null)
                GioRaUnix = 0;
            else
                GioRaUnix = dungchung.DateTimeToUnixTimeStamp(GioRa);
            Ngay = NgayChamCong.Value.Day;
            Thang = NgayChamCong.Value.Month;
            Nam = NgayChamCong.Value.Year;
        }
        public void UnixToDateTime()
        {
            if (NgayChamCongUnix == 0)
                NgayChamCong = null;
            else
                NgayChamCong = dungchung.UnixTimeStampToDateTime(NgayChamCongUnix);
            if (GioVaoUnix == 0)
                GioVao = null;
            else
                GioVao = dungchung.UnixTimeStampToDateTime(GioVaoUnix);
            if (GioRaUnix == 0)
                GioRa = null;
            else
                GioRa = dungchung.UnixTimeStampToDateTime(GioRaUnix);


        }
    }
    public class HRM_BangChamCongNhanVien_TangCa : Audit
    {
        public string IdBangChamCong { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmHeSoTangCa { get; set; }
        public double Cong { get; set; }
        public double GioTieuChuan { get; set; }
        public double HeSo { get; set; }
        public double CongTieuChuan { get; set; }
        public double TongSoGio { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public bool isKetThuc { get; set; }
    }
    public class HRM_BangChamCongNhanVien_SoGioLamViec : Audit
    {
        public string IdBangChamCong { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmCaLamViec { get; set; }
        public double GioTieuChuan { get; set; }
        public double CongTieuChuan { get; set; }
        public double TongSoGio { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public bool isKetThuc { get; set; }
    }

    public class HRM_TheoDoiNgayCong
    {
        public string CoSo { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string TenBoPhan { get; set; }
        public string LoaiCa { get; set; }
        public double CaGayP { get; set; }
        public double CaThangF { get; set; }
        public double TangCa { get; set; }
        public double NghiBuLe { get; set; }
        public double Phep { get; set; }
        public double Le { get; set; }
        public double TongCong { get; set; }
        public List<HRM_TheoDoiNgayCong_Ngay> listNgay { get; set; }
    }
    public class HRM_TheoDoiNgayCong_Ngay
    {
        public DateTime Ngay { get; set; }
        public double CaChinh { get; set; }
        public double TangCa { get; set; }
    }
    public class HRM_TheoDoiNgayCongTangCa
    {
        public string CoSo { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string TenBoPhan { get; set; }
        public string LoaiCa { get; set; }
        public double TongGioTangCa { get; set; }
        public double TongCongTangCa { get; set; }
        public List<HRM_TheoDoiNgayCongTangCa_Ngay> listNgay { get; set; }
    }
    public class HRM_TheoDoiNgayCongTangCaCaNhan
    {
        public List<HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien> listNhanVien { get; set; }
        public int Label { get; set; }
    }
    public class HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien
    {
        public string CoSo { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string TenBoPhan { get; set; }
        public string LoaiCa { get; set; }
        public double TongGioTangCa { get; set; }
        public double TongCongTangCa { get; set; }
        public List<HRM_TheoDoiNgayCongTangCaCaNhan_Ngay> listNgay { get; set; }

    }
    public class HRM_TheoDoiNgayCongTangCaCaNhan_Ngay
    {
        public DateTime Ngay { get; set; }
        public string LyDo { get; set; }
        public string TenCaLamViec { get; set; }
        public double SoGio { get; set; }
        public List<HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao> listChiTiet { get; set; }
    }
    public class HRM_TheoDoiNgayCongTangCaCaNhan_Label
    {

        public int Dem { get; set; } = 1;
    }

    public class HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao
    {

        public DateTime? GioRa { get; set; }
        public DateTime? GioVao { get; set; }
    }

    public class HRM_TheoDoiNgayCongTangCa_Ngay
    {
        public DateTime Ngay { get; set; }
        public double? SoGioTangCa { get; set; }
        public double? CongTangCa { get; set; }
    }

    // tính lương
    public class HRM_QuyTrinhTinhLuong : AudiQuyTrinh
    {

        public double NgayCongCoSo { get; set; }
        public long IdCongTyEOS { get; set; }
        public string IdBoPhan { get; set; }

        public string TenBangLuong { get; set; }
        public string IdKyChamCong { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        [NotMapped]
        public List<long> listIdBoPhan { get; set; } = new List<long>();
        [NotMapped]
        public List<string> listIdKyChamCong { get; set; }

        [NotMapped]
        public double TuNgayUnix { set; get; }

        [NotMapped]
        public double DenNgayUnix { set; get; }

        [NotMapped]
        public string TenCongty { set; get; }


        [NotMapped]
        public string TenBoPhan { set; get; }
        [NotMapped]
        public List<HRM_QuyTrinhTinhLuong_NhanVien> listNhanVien { get; set; }
        [NotMapped]
        public List<clsBangTinhLuongHeader> listHeader { get; set; }
        [NotMapped]
        public List<clsBangTinhLuong_Header> listHeader_Data { get; set; }

        [NotMapped]
        public List<clsBangTinhLuong_NhanVien> listGiaTriNhanVien { get; set; }

        [NotMapped]
        public List<clsBangTinhLuong_NhanVienView> listBoPhan { get; set; }



    }
    public class HRM_QuyTrinhTinhLuong_ChamCong : Audit
    {
        public string IdBangChamCong { get; set; }
        public string IdBangTinhLuong { get; set; }
    }
    public class HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu : Audit
    {
        public string IdNhanVien { get; set; }
        public DateTime Ngay { get; set; }
        public double SoTien { get; set; }
    }

    public class HRM_QuyTrinhTinhLuong_NhanVien : Audit
    {
        public bool isKetThuc { get; set; }
        public long IdCongTyEOS { get; set; }
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public int LoaiLuong { get; set; }
        public double NgayCongTieuChuan { get; set; }

        // công
        public double NgayCong { get; set; }
        public double NgayCongDem { get; set; }
        public double NgayCongCaGay { get; set; }
        public double NgayTangCa { get; set; }
        public double Ngay200 { get; set; }
        public double Le_Tet { get; set; }
        public double Le_TetKDL { get; set; }
        public double NghiPhep { get; set; }
        public double NghiBu { get; set; }

        //Lương
        public double LuongCoBan { get; set; }
        public double LuongTheoCong { get; set; }
        public double LuongTheoCongDem { get; set; }
        public double LuongDongBHXH { get; set; }
        public double LuongTangCa { get; set; }
        public double LuongLe { get; set; }
        public double LuongLeKDL { get; set; }
        public double LuongPhep { get; set; }
        public double Luong200 { get; set; }
        public double LuongNghiBu { get; set; }
        public double HoTroKhac { get; set; }
        public double DCKhac { get; set; }
        public double HoTroNghiViec { get; set; }
        public double LuongThamNien { get; set; }

        public double TongLuong { get; set; }
        public double TruyThu { get; set; } = 0;
        public double TamUng { get; set; } = 0;
        public double? LuongDanhGia { get; set; }

        // phụ cấp
        public double PhuCapCongChinh { get; set; }
        public double PhuCapKhongTheoCongChinh { get; set; }
        public List<HRM_QuyTrinhTinhLuong_PhuCap> listPhuCap { get; set; }

        // thuế thu nhập cá nhân
        public string IdLoaiHopDong { get; set; }
        public double? Cong50 { get; set; }
        public double? Luong50 { get; set; }
        public double? Cong100 { get; set; }
        public double? Luong100 { get; set; }
        public double CamKeC02 { get; set; }
        public double SoGiamTruBanThan { get; set; }
        public double SpNguoiDangKyNPT { get; set; }
        public double TongTienGiamTruGiaCanh { get; set; }
        public double ThuNhapMienThue { get; set; }
        public double ThuNhapTinhThueTNCN { get; set; }
        public double ThueTNCNTheoLuyTien { get; set; }

        // bảo hiểm xã hội Người SDLD
        public double MucDongBaoHiemXaHoi { get; set; }
        public double SDLDBHXH { get; set; }
        public double SDLDBHYT { get; set; }
        public double SDLDBHTN { get; set; }
        public double TongNguoiSDLD { get; set; }

        public double NLDBHXH { get; set; }
        public double NLDBHYT { get; set; }
        public double NLDBHTN { get; set; }
        public double TongNguoiNLD { get; set; }

        public double TienThucNhan { get; set; }

        public double? KhauTruTaiNguon { get; set; }
        public double? TongGiamThueTNCN { get; set; }
        public double? DongPhuc { get; set; }
        public double? XeVe { get; set; }
        public double? Khac { get; set; }
        public double? TraTienDongPhuc { get; set; }
        // chi phí thêm
        public double? Phat { get; set; }
        public double? Thuong { get; set; }
        public double? ThuongLuong { get; set; }
        public double? BaoHiemNgoai { get; set; }
        public double? TienCongDoan { get; set; }
        public double? TienTruyHoan { get; set; }
        public double? SoTienPCDiLai { get; set; }// lương thâm niên
        public double? TienBienChe { get; set; }
        public double? CongNghiKeHoach { get; set; }
        public double? LuongNghiKeHoach { get; set; }
        public double? HoTroPhongSach { get; set; }
        public double? TA_SoCong { get; set; }
        public double? TA_DonGia { get; set; } = 15000;
        public double? TA_ThanhTien { get; set; }
        public double? HoTroCapPhatBHLD { get; set; }
        public double? CacKhoanCongTyThanhToan { get; set; }
        public double? CongOm { get; set; }
        public double? LiXiTrucTet { get; set; }
        public double? TK_XuatAn { get; set; }
        public double? TK_DonGia { get; set; } = 15000;
        public double? TK_TruAnCa { get; set; }
        public double? GiamDanhGia { get; set; }
        public double? DanhGia { get; set; }
        public double? CongDem { get; set; }
        public double? LuongTuNhap { get; set; }
        public double? HoTroBHYT { get; set; }
        public double? HoTroNhatCuaRoi { get; set; }
        public double? GiamTruDoViPham { get; set; }
        public double? ThanhToanBHCN { get; set; }
        public double? TruTienBHCN { get; set; }
        public bool isCamKet { get; set; }


        [NotMapped]
        public string MaCoSo { get; set; }
        [NotMapped]
        public string SoTaiKhoan { get; set; }
        [NotMapped]
        public string NganHang { get; set; }
        [NotMapped]
        public string NganHangNguon { get; set; }

        [NotMapped]
        public string TenLoaiHopDong { get; set; }
        [NotMapped]
        public string MaSoThue { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }


        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public int SapXepBoPhan { get; set; }

        [NotMapped]
        public string MaBoPhan { get; set; }
        [NotMapped]
        public string CaLamViec { get; set; }
        [NotMapped]
        public double SoGioMotNgay { get; set; }

        [NotMapped]
        public string LoaiHopDong { get; set; }
        [NotMapped]
        public string NgayNghiViec { get; set; }
        [NotMapped]
        public string NgayVaoLam { get; set; }
        [NotMapped]
        public DateTime NgayApDung { get; set; }

        [NotMapped]
        public bool isDieuDong { get; set; }

        [NotMapped]
        public List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa { get; set; } = new List<HRM_QuyTrinhTinhLuong_TangCa>();
        [NotMapped]
        public List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuan { get; set; } = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
        [NotMapped]
        public List<HRM_QuyTrinhTinhLuong_BaoHiemNgoai> listBaoHiemNgoai { get; set; } = new List<HRM_QuyTrinhTinhLuong_BaoHiemNgoai>();



    }
    public class HRM_QuyTrinhTinhLuong_TangCa : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmHeSoTangCa { get; set; }
        public double Cong { get; set; }
        public double CongTieuChuan { get; set; }
        public double HeSo { get; set; }
        public double TongSoGio { get; set; }
        public double Luong { get; set; }
        [NotMapped]
        public string DiaChi { get; set; }
        [NotMapped]
        public string DiaChiLuong { get; set; }
    }
    public class HRM_QuyTrinhTinhLuong_SoGioLamViec : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmCaLamViec { get; set; }
        public double GioTieuChuan { get; set; }
        public double CongTieuChuan { get; set; }
        public double TongSoGio { get; set; }
        public double Luong { get; set; }
        [NotMapped]
        public string DiaChi { get; set; }
        [NotMapped]
        public string DiaChiLuong { get; set; }

    }
    public class HRM_QuyTrinhTinhLuong_BaoHiemNgoai : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public string IdNoiCap { get; set; }
        public double SoTien { get; set; }
        [NotMapped]
        public string DiaChi { get; set; }


    }

    public class HRM_QuyTrinhTinhLuong_PhuCap : Audit
    {
        public string IdQuyTrinh { get; set; }

        public string IdNhanVien { get; set; }
        public int IdLoaiThoiGian { get; set; }
        public double SoTienPhuCap { get; set; }
        public double SoTienPhuCapHienThi { get; set; }

        public string IddmPhuCap { get; set; }
        [NotMapped]
        public string Ma { get; set; }

    }

    public class HRM_TongHopLuongThangViewModel
    {
        public List<HRM_TongHopLuongThang> listNhanVien { get; set; }
        public string IddmNganHangNguon { get; set; }
    }
    public class HRM_TongHopLuongThang
    {
        public string IdNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime NgaySinh { get; set; }
        public long? IdCongTy { get; set; }
        public double TongLuong { get; set; }
        public string DonViTienTe { get; set; }
        public string SoTaiKhoan { get; set; }
        public string ChiNhanh { get; set; }
        public string NganHang { get; set; }
        public string IdNganHang { get; set; }
        public string MaBank { get; set; }
        public string TKNguon { get; set; }
        public string IdTKNguon { get; set; }
        public string CCCD { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NoiCap { get; set; }
        public List<HRM_TongHopLuongThang_ChiTiet> listChiTiet { get; set; }
    }

    public class HRM_TongHopLuongThang_ChiTiet
    {
        public string TenCongTy { get; set; }
        public double Cong { get; set; }
        public double CongTangCa { get; set; }
        public double Luong { get; set; }
    }

    public class HRM_TongHopLuong
    {
        public string MaBoPhan { get; set; }
        public string TenBoPhan { get; set; }
        public double SoLuongNhanSuVPCT { get; set; }
        public double SoTienVPCT { get; set; }
        public double TongCong { get; set; }
        public double TyLeLuong { get; set; }
        public List<HRM_TongHopLuong_CoSo> listCongty { get; set; }

    }
    public class HRM_TongHopLuong_CoSo
    {
        public string TenCongTy { get; set; }
        public long? IdCongTy { get; set; }
        public double? SoLuongNhanSu { get; set; }
        public double? Cong { get; set; }
        public double? SoTien { get; set; }
        public double? TyLe { get; set; }
        public double? TyLeDoanhThuCoSo { get; set; }

    }
    public class clsBangTinhLuongHeader
    {
        public int Cap { get; set; }
        public List<clsBangTinhLuong_Header> listheader { get; set; }
    }
    public class clsBangTinhLuong_Header
    {
        public string Ma { get; set; }
        public string MaCha { get; set; }
        public string IddmLabel { get; set; }
        public string name { set; get; }
        public int? rowspan { set; get; }
        public int? colspan { set; get; }
        public int SoThapPhan { set; get; }
        public bool isDuocNhap { set; get; }
        public bool isDisable { set; get; }
        public int STT { get; set; }
        public int Cap { get; set; }
        public string Id { get; set; }
        public string IdParent { get; set; }
        public string IdKey { get; set; }
        public double DoRong { get; set; } = 100;
        public List<clsBangTinhLuong_Header> listheader { get; set; }
        public clsBangTinhLuong_Header()
        {

        }
        public clsBangTinhLuong_Header(HRM_dmLabelThietLapBangLuong item)
        {
            Ma = item.Ma;
            name = item.Ten;
            IddmLabel = item.IddmPhuCap;
            STT = item.SapXep;
            Id = item.IdGuid;
            IdParent = item.IdParent;
            Cap = item.Header;
            isDuocNhap = item.isDuocNhap;
            IdKey = item.IdKey;
            DoRong = 100;
            SoThapPhan = item.SoThapPhan;
        }
        public clsBangTinhLuong_Header(HRM_dmThietLapBangLuong_ChiTiet item)
        {
            Ma = item.Ma;
            name = item.Ten;
            IddmLabel = item.IddmLabel;
            STT = item.SapXep;
            Id = item.IdGuid;
            IdParent = item.IdParent;
            Cap = item.Header;
            isDuocNhap = item.isDuocNhap;
            IdKey = item.IdKey;
            SoThapPhan = item.SoThapPhan ?? 0;
            DoRong = 100;

        }

    }
    public class clsBangTinhLuong_NhanVienView
    {
        public string TenBoPhan { get; set; }
        public long IdCongTy { get; set; }
        public int SapXep { get; set; }
        public string TenCongTy { get; set; }
        public List<clsBangTinhLuong_NhanVien> listNhanVien { get; set; }
        public List<clsBangTinhLuong_GiaTri> listGiaTri { get; set; }
        public List<clsBangTinhLuong_NhanVienView> lischucDanh { get; set; }
    }
    public class clsBangTinhLuong_NhanVien
    {
        public string IdNhanVien { get; set; }
        public long IdCongTy { get; set; }
        public string TenCongTy { get; set; }
        public bool isDieuDong { get; set; }
        public string TenBoPhan { get; set; }
        public int SapXep { get; set; }
        public string TenChucDanh { get; set; }
        public DateTime? NgayVaoLam { get; set; }

        public List<clsBangTinhLuong_GiaTri> listGiaTri { get; set; }
    }

    public class clsBangTinhLuong_GiaTri
    {
        public string IddmLabel { get; set; }
        public string Ma { get; set; }
        public string MaCha { get; set; }
        public string Name { get; set; }
        public double heso { get; set; }
        public bool isDuocNhap { get; set; }
        public string NoiDung { set; get; }
        public double? GiaTri { set; get; }
        public int SoThapPhan { set; get; }
        public int LoaiPhuCap { set; get; }
        public int STT { get; set; }
        public clsBangTinhLuong_GiaTri()
        {

        }
        public clsBangTinhLuong_GiaTri(clsBangTinhLuong_Header item, HRM_NhanVien NV, HRM_QuyTrinhTinhLuong_NhanVien data)
        {

        }
        public clsBangTinhLuong_GiaTri(clsBangTinhLuong_Header item)
        {
            Ma = item.Ma;
            SoThapPhan = item.SoThapPhan;
            isDuocNhap = item.isDuocNhap;
            Name = item.name;
        }

    }

    #endregion
    #region Cấp phát đồng phục
    public class HRM_CapPhatThuHoiTaiSan : Audit
    {
        public long IdCongTyEOS { get; set; }
        public string IdBoPhan { get; set; }
        public string GhiChu { get; set; }
        public int Loai { get; set; } // 1 Cấp phát , 2 thu hồi
        [NotMapped]
        public List<long> listIdBoPhan { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }

        [NotMapped]
        public List<HRM_CapPhatThuHoiTaiSan_NhanVien> listNhanVien { get; set; }
    }

    public class HRM_CapPhatThuHoiTaiSan_NhanVien
    {
        public int Loai { get; set; }
        public string TenLoai { get; set; }
        public string IdNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public long IdCongTy { get; set; }
        public string TenChucVu { get; set; }
        public string IdGuid { get; set; }
        public string IdTaiSan { get; set; }
        public double? GiaTri { get; set; }
        public int? SoLuong { get; set; }
        public DateTime? Ngay { get; set; }
        public string GhiChu { get; set; }

        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public double DatCoc { get; set; }
        public bool TinhLuong { get; set; } = false;
        public string TenBoPhan { get; set; }
        public string TenChucDanh { get; set; }

        [NotMapped]
        public clsUser ItemUserCreatedBy { get; set; }
        public string TenCongTy { get; set; }
        public List<HRM_CapPhatThuHoiTaiSan_Item> listTaiSan { get; set; }
        public HRM_CapPhatThuHoiTaiSan_NhanVien() { }
        public HRM_CapPhatThuHoiTaiSan_NhanVien(HRM_CapPhatThuHoiTaiSan_Item a, HRM_NhanVien b)
        {
            MaNhanVien = b.MaNhanVien;
            TenNhanVien = b.TenDayDu;
            IdCongTy = b.IdCongTyEOS;
            IdBoPhan = b.IdBoPhanEOS;

        }

    }
    public class HRM_CapPhatThuHoiTaiSan_Item : Audit
    {
        public string IdQuyTrinh { get; set; }
        public int Loai { get; set; }// 1 Cấp phát , 2 thu hồi
        public string IdNhanVien { get; set; }
        public string IdTaiSan { get; set; }
        public double? GiaTriCapPhat { get; set; }
        public double? GiaTriThuHoi { get; set; }
        public int? SoLuongCapPhat { get; set; }
        public int? SoLuongThuHoi { get; set; }
        public DateTime? NgayCapPhat { get; set; }
        public DateTime? NgayThuHoi { get; set; }
        public string GhiChuCapPhat { get; set; }
        public string GhiChuThuHoi { get; set; }
        public bool TinhLuong { get; set; } = false;
        public double? DatCoc { get; set; }
        public double TinhTrang { get; set; }
        public string IdCapPhat_Item { get; set; }
        [NotMapped]
        public string TenTaiSan { get; set; }
        [NotMapped]
        public long IdBoPhan { get; set; }
        [NotMapped]
        public long IdCongTy { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public double NgayCapPhatUnix { get; set; }
        [NotMapped]
        public double NgayThuHoiUnix { get; set; }
        [NotMapped]
        public long IdChucVu { get; set; }
        public HRM_CapPhatThuHoiTaiSan_Item()
        {

        }
        public HRM_CapPhatThuHoiTaiSan_Item(HRM_CapPhatThuHoiTaiSan_Item a, HRM_NhanVien b)
        {
            IdGuid = a.IdGuid;
            IdQuyTrinh = a.IdQuyTrinh;
            IdNhanVien = a.IdNhanVien;
            IdTaiSan = a.IdTaiSan;
            GiaTriCapPhat = a.GiaTriCapPhat;
            GiaTriThuHoi = a.GiaTriThuHoi;
            SoLuongCapPhat = a.SoLuongCapPhat;
            SoLuongThuHoi = a.SoLuongThuHoi;
            NgayCapPhat = a.NgayCapPhat;
            NgayThuHoi = a.NgayThuHoi;
            GhiChuCapPhat = a.GhiChuCapPhat;
            GhiChuThuHoi = a.GhiChuThuHoi;
            TinhLuong = a.TinhLuong;
            IdCongTy = b.IdCongTyEOS;
            IdBoPhan = b.IdBoPhanEOS;
            IdChucVu = b.IdChucVu;
            MaNhanVien = b.MaNhanVien;
            TenNhanVien = b.TenDayDu;
        }
        public void DateTimeToUnix()
        {
            if (NgayCapPhat == null)
                NgayCapPhatUnix = 0;
            else
                NgayCapPhatUnix = dungchung.DateTimeToUnixTimeStamp(NgayCapPhat);
            if (NgayThuHoi == null)
                NgayThuHoiUnix = 0;
            else
                NgayThuHoiUnix = dungchung.DateTimeToUnixTimeStamp(NgayThuHoi);

        }
        public void UnixToDatetime()
        {
            if (NgayCapPhatUnix == 0)
                NgayCapPhat = null;
            else
                NgayCapPhat = dungchung.UnixTimeStampToDateTime(NgayCapPhatUnix);
            if (NgayThuHoiUnix == 0)
                NgayThuHoi = null;
            else
                NgayThuHoi = dungchung.UnixTimeStampToDateTime(NgayThuHoiUnix);

        }
    }

    public class HRM_BaoCaoCapPhatDongPhuc
    {
        public string CoSo { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string BoPhan { get; set; }
        public string LoaiCong { get; set; }
        public DateTime? NgayBatDauLamViec { get; set; }
        public string ThamNien { get; set; }
        public List<HRM_BaoCaoCapPhatDongPhuc_TaiSan> listTaiSan { get; set; }

    }
    public class HRM_BaoCaoCapPhatDongPhuc_TaiSan
    {
        public string TenTaiSan { get; set; }
        public double SoLuongPhatMoi { get; set; }
        public double GiaPhatMoi { get; set; }
        public DateTime? NgayPhatPhatMoi { get; set; }
        public string NgayThuTienPhatMoi { get; set; }
        public double SoLuongBoSung { get; set; }
        public double GiaBoSung { get; set; }
        public DateTime? NgayPhatBoSung { get; set; }
        public string NgayThuTienBoSung { get; set; }
        public double SoLuongThuHoi { get; set; }
        public double GiaThuHoi { get; set; }
        public DateTime? NgayPhatThuHoi { get; set; }
        public string NgayThuTienThuHoi { get; set; }

    }

    #endregion


    #region Chấm công hàng tháng
    public class HRM_DanhMuc_NhanVien_ChamCongBoPhan : Audit
    {
        public string IdUserNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        public long IdBoPhanEOS { get; set; }
        public long IdCongTyEOS { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
    }
    public class HRM_ChamCong_Detail : Audit
    {
        [MaxLength(36)]
        public string IdDanhMucDotChamCongThang { get; set; }//HRM_dmDotChamCongThang
        [NotMapped]
        public string TenDanhMucDotChamCong { set; get; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        [MaxLength(36)]
        public string IdTrangThai { get; set; }
        public string TenTrangThai { get; set; }
        public bool isKetThuc { get; set; }
        public string idChamCongCongTy { get; set; }
        public double NgayCongCoSo { get; set; }
        public long IdCongTyEOS { get; set; }
        public string SoPhieu { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
        [NotMapped]
        public List<NgayChamCong> lstNgayChamCongTrongKy { get; set; }
        [NotMapped]
        public List<HRM_ChamCong_BoPhan> lstChamCongBoPhan { get; set; }
        [NotMapped]
        public string TenBoPhans { set; get; }
        [NotMapped]
        public List<HRM_ChamCong_NhanVien_BoPhan> lstChamCongNhanVien { get; set; }
        public void GetDanhSachNgayCongTrongThang(List<HRM_dmDotChamCongThang> lstDotChamCong, List<HRM.Api.Models.SmartEOEModel.CaiDatHeThong> lstCaiDatNghiLe)
        {
            lstNgayChamCongTrongKy = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong, lstCaiDatNghiLe);
        }
    }
    public class HRM_ChamCong_BoPhan : Audit
    {
        public string IdChamCongDetail { get; set; }//HRM_ChamCong_Detail
        public long IdBoPhanEOS { get; set; }//Load danh sách nhân viên theo hợp đồng, quyết định điều động (không phải ở chức vụ phòng ban)
        public string IdDanhMucDotChamCongThang { get; set; }//HRM_dmDotChamCongThang
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string TrangThai { get; set; }
        public string idChamCongCongTy { get; set; }
        public double NgayCongCoSo { get; set; }
        [NotMapped]
        public HRM_ChamCong_Detail itemChamCongDetail { get; set; }
        [NotMapped]
        public List<NgayChamCong> lstNgayChamCongTrongKy { get; set; }
        [NotMapped]
        public List<HRM_ChamCong_NhanVien_BoPhan> lstNhanVien { get; set; }
        public void GetDanhSachNgayCongTrongThang(List<HRM_dmDotChamCongThang> lstDotChamCong, List<HRM.Api.Models.SmartEOEModel.CaiDatHeThong> lstCaiDatNghiLe)
        {
            lstNgayChamCongTrongKy = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong, lstCaiDatNghiLe);
        }
    }
    public class HRM_ChamCong_NhanVien_BoPhan : Audit
    {
        public string IdChamCongDetail { get; set; }//HRM_ChamCong_Detail
        public string IdChamCongBoPhan { get; set; }//HRM_ChamCong_BoPhan
        public string IdNhanVien { get; set; }
        public long IdBoPhanEOS { get; set; }
        public string idChamCongCongTy { get; set; }
        //public int Tong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenPhongBan { get; set; }
        [NotMapped]
        public List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan> lstChamCongThoiGian { get; set; }
    }
    public class HRM_ChamCong_NhanVien_ThoiGian_BoPhan : Audit
    {
        [MaxLength(36)]
        public string IdChamCongDetail { get; set; }//HRM_ChamCong_Detail
        [MaxLength(36)]
        public string IdChamCongBoPhan { get; set; }//HRM_ChamCong_BoPhan
        public long IdBoPhanEOS { get; set; }//
        public long IdCongTyEOS { get; set; }//
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(36)]
        public string IdChamCongNhanVienBoPhan { get; set; }//HRM_ChamCong_NhanVien_BoPhan
        public string idChamCongCongTy { get; set; }
        public int Ngay { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public DateTime NgayCong { get; set; }
        //public string Thu { get; set; }//Thứ trong tuần
        public string ThuVietTat { get; set; }//Thứ viết tắt
        [MaxLength(36)]
        public string IddmKyHieuChamCong { get; set; }
        [MaxLength(36)]
        public string IddmChamCong { get; set; }
        public bool isLuongBaoHiem { get; set; }
        public bool isLuongSanPham { get; set; }
        public double SoGioLamViec { get; set; }

        public bool isDisable { get; set; }//Hết hiệu lực, điều động, view sẽ không hiển thị
        public string idHopDong { get; set; }//Để tính lương
        public string idQuyetDinhDieuChuyen { get; set; }//Nếu có quyết định sẽ ưu tiên quyết định

        public string LyDoDieuChinh { get; set; }
        public void Update(HRM_ChamCong_NhanVien_ThoiGian_BoPhan input)
        {
            //this.IdChamCongDetail         = input.IdChamCongDetail         ;
            //this.IdChamCongBoPhan         = input.IdChamCongBoPhan         ;
            //this.IdNhanVien               = input.IdNhanVien               ;
            //this.IdChamCongNhanVienBoPhan = input.IdChamCongNhanVienBoPhan ;
            //this.idChamCongCongTy         = input.idChamCongCongTy         ;
            //this.Ngay                     = input.Ngay                     ;
            //this.Thang                    = input.Thang                    ;
            //this.Nam                      = input.Nam                      ;
            //this.NgayCong                 = input.NgayCong                 ;
            //this.Thu                      = input.Thu                      ;
            //this.ThuVietTat               = input.ThuVietTat               ;


            this.isDisable = input.isDisable;
            this.idHopDong = input.idHopDong;
            this.idQuyetDinhDieuChuyen = input.idQuyetDinhDieuChuyen;
            //this.MaChamCongSang = input.MaChamCongSang;
            //this.MaChamCongChieu = input.MaChamCongChieu;

            //this.isNghiLe = input.isNghiLe;
            //this.isNghiPhep = input.isNghiPhep;
            //this.isNghiKhongPhep = input.isNghiKhongPhep;
            //this.isHoTroCongTacXa = input.isHoTroCongTacXa;
            //this.SoGioSang = input.SoGioSang;
            //this.SoGioChieu = input.SoGioChieu;
            //this.isCheckCa1 = input.isCheckCa1;
            //this.isCheckCa2 = input.isCheckCa2;
        }
        public void Update(EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan input)
        {
            //this.IdChamCongDetail         = input.IdChamCongDetail         ;
            //this.IdChamCongBoPhan         = input.IdChamCongBoPhan         ;
            //this.IdNhanVien               = input.IdNhanVien               ;
            //this.IdChamCongNhanVienBoPhan = input.IdChamCongNhanVienBoPhan ;
            //this.idChamCongCongTy         = input.idChamCongCongTy         ;
            //this.Ngay                     = input.Ngay                     ;
            //this.Thang                    = input.Thang                    ;
            //this.Nam                      = input.Nam                      ;
            //this.NgayCong                 = input.NgayCong                 ;
            //this.Thu                      = input.Thu                      ;
            //this.ThuVietTat               = input.ThuVietTat               ;

            this.isDisable = input.isDisable;
            this.idHopDong = input.idHopDong;
            this.idQuyetDinhDieuChuyen = input.idQuyetDinhDieuChuyen;
            //this.MaChamCongSang = input.MaChamCongSang;
            //this.MaChamCongChieu = input.MaChamCongChieu;
            //this.isNghiLe = input.isNghiLe;
            //this.isNghiPhep = input.isNghiPhep;
            //this.isNghiKhongPhep = input.isNghiKhongPhep;
            //this.isHoTroCongTacXa = input.isHoTroCongTacXa;
            //this.SoGioSang = input.SoGioSang;
            //this.SoGioChieu = input.SoGioChieu;
            //this.isCheckCa1 = input.isCheckCa1;
            //this.isCheckCa2 = input.isCheckCa2;
        }
        //public bool Equals(EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan input)
        //{
        //    return (this.SoGioSang == input.SoGioSang && this.SoGioChieu == input.SoGioChieu && this.MaChamCongSang == input.MaChamCongSang && this.MaChamCongChieu == input.MaChamCongChieu && this.LyDoDieuChinh == input.LyDoDieuChinh);
        //}
        public HRM_ChamCong_NhanVien_ThoiGian_BoPhan()
        {

        }

        public HRM_ChamCong_NhanVien_ThoiGian_BoPhan(NgayChamCong itemNgayChamCong, string _IdNhanVien, string _IdChamCongDetail, string _IdChamCongNhanVienBoPhan)
        {
            Ngay = itemNgayChamCong.Ngay;
            Thang = itemNgayChamCong.Thang;
            Nam = itemNgayChamCong.Nam;
            NgayCong = itemNgayChamCong.NgayCong;
            //Thu = itemNgayChamCong.Thu;
            ThuVietTat = itemNgayChamCong.ThuVietTat;
            idChamCongCongTy = "";
            IdNhanVien = _IdNhanVien;
            IdChamCongDetail = _IdChamCongDetail;
            IdChamCongNhanVienBoPhan = _IdChamCongNhanVienBoPhan;
            //if (itemNgayChamCong.isNghi == false)
            //    isCoLuong = true;
            //isCoLuong = false;


        }
        public HRM_ChamCong_NhanVien_ThoiGian_BoPhan(EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan input)
        {
            this.IdGuid = input.IdGuid;
            this.IdChamCongDetail = input.IdChamCongDetail;
            this.IdChamCongBoPhan = input.IdChamCongBoPhan;
            this.IdBoPhanEOS = input.IdBoPhanEOS;
            this.IdCongTyEOS = input.IdCongTyEOS;
            this.IdNhanVien = input.IdNhanVien;
            this.IdChamCongNhanVienBoPhan = input.IdChamCongNhanVienBoPhan;
            this.idChamCongCongTy = input.idChamCongCongTy;
            this.Ngay = input.Ngay;
            this.Thang = input.Thang;
            this.Nam = input.Nam;
            this.NgayCong = input.NgayCong;
            this.ThuVietTat = input.ThuVietTat;
            this.isDisable = input.isDisable;
            this.idHopDong = input.idHopDong;
            this.idQuyetDinhDieuChuyen = input.idQuyetDinhDieuChuyen;
            this.LyDoDieuChinh = input.LyDoDieuChinh;
            this.IddmChamCong = input.IddmChamCong;
            this.SoGioLamViec = input.SoGioLamViec;

            //this.Thu = input.Thu;
            //this.MaChamCongSang = input.MaChamCongSang;
            //this.MaChamCongChieu = input.MaChamCongChieu;
            //this.isNghiLe = input.isNghiLe;
            //this.isNghiPhep = input.isNghiPhep;
            //this.isNghiKhongPhep = input.isNghiKhongPhep;
            //this.isHoTroCongTacXa = input.isHoTroCongTacXa;
            //this.SoGioSang = input.SoGioSang;
            //this.SoGioChieu = input.SoGioChieu;
            //this.isCheckCa1 = input.isCheckCa1;
            //this.isCheckCa2 = input.isCheckCa2;
        }
        public HRM_ChamCong_NhanVien_ThoiGian_BoPhan(EntityHRM_ChamCong_NhanVien_BoPhan input)
        {
            this.IdNhanVien = input.IdNhanVien;
            this.IdChamCongDetail = input.IdChamCongDetail;
            this.IdChamCongBoPhan = input.IdChamCongBoPhan;
            this.IdBoPhanEOS = input.IdBoPhanEOS;
            //this.IdCongTyEOS = input.IdCongTyEOS;
            this.IdNhanVien = input.IdNhanVien;
            //this.IdChamCongNhanVienBoPhan = input.IdChamCongNhanVienBoPhan;
            this.idChamCongCongTy = input.idChamCongCongTy;
            //this.Ngay = input.Ngay;
            //this.Thang = input.Thang;
            //this.Nam = input.Nam;
            //this.NgayCong = input.NgayCong;
            //this.ThuVietTat = input.ThuVietTat;
            //this.isDisable = input.isDisable;
            //this.idHopDong = input.idHopDong;
            //this.idQuyetDinhDieuChuyen = input.idQuyetDinhDieuChuyen;
            //this.LyDoDieuChinh = input.LyDoDieuChinh;

            //this.Thu = input.Thu;
            //this.MaChamCongSang = input.MaChamCongSang;
            //this.MaChamCongChieu = input.MaChamCongChieu;
            //this.isNghiLe = input.isNghiLe;
            //this.isNghiPhep = input.isNghiPhep;
            //this.isNghiKhongPhep = input.isNghiKhongPhep;
            //this.isHoTroCongTacXa = input.isHoTroCongTacXa;
            //this.SoGioSang = input.SoGioSang;
            //this.SoGioChieu = input.SoGioChieu;
            //this.isCheckCa1 = input.isCheckCa1;
            //this.isCheckCa2 = input.isCheckCa2;
        }
    }
    public class HRM_ChamCong_CongTy : Audit
    {
        public long IdCongTyEOS { get; set; }
        public string IdDanhMucDotChamCongThang { get; set; }//HRM_dmDotChamCongThang
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string TrangThai { get; set; }
        public double NgayCongCoSo { get; set; }
        [NotMapped]
        public List<NgayChamCong> lstNgayChamCongTrongKy { get; set; }
        [NotMapped]
        public List<HRM_ChamCong_BoPhan> lstChamCongBoPhan { get; set; }
        [NotMapped]
        public List<HRM_ChamCong_NhanVien_BoPhan> lstNhanVien { get; set; }
        public void GetDanhSachNgayCongTrongThang(List<HRM_dmDotChamCongThang> lstDotChamCong, List<HRM.Api.Models.SmartEOEModel.CaiDatHeThong> lstCaiDatNghiLe)
        {
            lstNgayChamCongTrongKy = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong, lstCaiDatNghiLe);
        }
    }
    #endregion
}
