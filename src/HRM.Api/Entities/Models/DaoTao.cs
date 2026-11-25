﻿#nullable disable
using HRM.Api.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
    public class HRM_KeHoachDaoTaoTongHop : AudiQuyTrinh
    {
        public int Nam { get; set; }
        [MaxLength(20)]
        public string SoPhieu { get; set; }
        public string IdNguoiTongHop { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public long IdCongTyEOS { set; get; }
        public double TongKinhPhiKeHoach { get; set; }
        public bool isDaoTaoMoi { get; set; } = false;


        [MaxLength(250)]
        public string DotTongHop { set; get; }
        public DateTime NgayTongHop { set; get; }
        [NotMapped]
        public Double NgayTongHopUnix { set; get; }

        [NotMapped]
        public List<HRM_TongHop_KeHoachDaoTaoChiTiet> listChiTiet = new List<HRM_TongHop_KeHoachDaoTaoChiTiet>();
        [NotMapped]
        public List<HRM_KeHoachDaoTao> listKeHoach = new List<HRM_KeHoachDaoTao>();
    }

    public class HRM_KeHoachDaoTao_TongHop : Audit
    {
        [MaxLength(36)]
        public string IdKeHoach { set; get; }
        [MaxLength(36)]
        public string IdTongHop { set; get; }
    }
    public class HRM_TongHop_KeHoachDaoTaoChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdTongHop { set; get; }
        public int SoNhanVien { get; set; }
        public double KeHoachKinhPhi { get; set; }

        public string IddmNoiDungDaoTao { set; get; }
        [MaxLength(36)]
        public string IdChiTietDot { set; get; }
        public bool isDaoTao { set; get; }
        [MaxLength(36)]
        public string IddmHinhThucDaoTao { set; get; }
        [MaxLength(1000)]
        public string NoiDung { set; get; }
        public bool isThang1 { get; set; }
        public bool isThang2 { get; set; }
        public bool isThang3 { get; set; }
        public bool isThang4 { get; set; }
        public bool isThang5 { get; set; }
        public bool isThang6 { get; set; }
        public bool isThang7 { get; set; }
        public bool isThang8 { get; set; }
        public bool isThang9 { get; set; }
        public bool isThang10 { get; set; }
        public bool isThang11 { get; set; }
        public bool isThang12 { get; set; }
        [NotMapped]
        public bool isXoa { get; set; }
        [NotMapped]
        public List<string> listIdNoiDung { get; set; }
        [NotMapped]
        public List<HRM_TongHop_KeHoachDaoTaoNhanVien> listNhanVien = new List<HRM_TongHop_KeHoachDaoTaoNhanVien>();
    }

    public class HRM_TongHop_KeHoachDaoTaoNhanVien : Audit
    {
        [MaxLength(36)]
        public string IdChiTietKeHoach { set; get; }

        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { set; get; }
        [MaxLength(250)]
        public string TenNhanVien { set; get; }


        [NotMapped]
        public bool isXoa { set; get; }

    }

    #region Đào tao
    public class HRM_KeHoachDaoTao : AudiQuyTrinh
    {
        [MaxLength(36)]
        public string IdNguoiDeNghi { get; set; }
        [MaxLength(36)]
        public string IdTongHopKeHoach { set; get; }
        [MaxLength(50)]
        public string SoPhieu { get; set; }
        public int Nam { get; set; }
        public DateTime NgayLap { set; get; }
        [NotMapped]
        public double NgayLapUnix { set; get; }
        //public double KeHoachKinhPhi { get; set; }

        public long IdBoPhanEOS { get; set; } // Bộ phận yêu cầu
        public long IdChucVuEOS { get; set; } // chức vụ
        public bool isDaoTaoMoi { get; set; } = false;
        public long IdCongTyEOS { set; get; }
        //[NotMapped]
        //public double ThucTeKinhPhi { get; set; }


        [NotMapped]
        public List<HRM_KeHoachDaoTaoChiTiet> listChiTiet = new List<HRM_KeHoachDaoTaoChiTiet>();
    }


    public class HRM_KeHoachDaoTaoChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdKeHoach { get; set; }

        public string IddmNoiDungDaoTao { get; set; }


        public int IddmHinhThucDaoTao { get; set; }


        public string IdPhuongThucDaoTao { get; set; }
        public int KeHoachSoNhanVien { get; set; }

        public bool isThang1 { get; set; }
        public bool isThang2 { get; set; }
        public bool isThang3 { get; set; }
        public bool isThang4 { get; set; }
        public bool isThang5 { get; set; }
        public bool isThang6 { get; set; }
        public bool isThang7 { get; set; }
        public bool isThang8 { get; set; }
        public bool isThang9 { get; set; }
        public bool isThang10 { get; set; }
        public bool isThang11 { get; set; }
        public bool isThang12 { get; set; }


        [NotMapped]
        public List<string> listIdNoiDung { get; set; }
        [NotMapped]
        public bool isXoa { get; set; }
        public List<HRM_KeHoachDaoTaoNhanVien> listNhanVien = new List<HRM_KeHoachDaoTaoNhanVien>();
        [NotMapped]
        public List<HRM_Dot_DaoTao_NoiDung> listNoiDungChung = new List<HRM_Dot_DaoTao_NoiDung>();
    }


    public class HRM_KeHoachDaoTaoNhanVien : Audit
    {
        [MaxLength(36)]
        public string IdChiTietKeHoach { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { set; get; }
        [MaxLength(250)]
        public string TenNhanVien { set; get; }
        [NotMapped]
        public bool isXoa { set; get; }
        [NotMapped]
        public string TenChucVu { get; set; }
    }



    #endregion

    #region Đợt đào tạo
    public class HRM_Dot_DaoTao : AudiQuyTrinh
    {
        public int Nam { get; set; }
        [MaxLength(20)]
        public string SoPhieu { get; set; }
        [MaxLength(36)]
        public string IdNguoiTongHop { set; get; }
        [MaxLength(36)]
        public string IdKeHoachTongHop { set; get; }
        public DateTime NgayTao { set; get; }
        [NotMapped]
        public double NgayTaoUnix { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public bool isDaoTaoMoi { get; set; } = false;

        public double KinhPhi { get; set; }
        public long IdCongTyEOS { set; get; }
        [NotMapped]
        public string TenCongTyEOS { get; set; }

        [MaxLength(250)]
        public string DotDaoTao { set; get; }


        [NotMapped]
        public HRM_KeHoachDaoTaoTongHop KeHoachDaoTaoTongHop { set; get; }
        [NotMapped]
        public List<HRM_Dot_DaoTaoChiTiet> listChiTiet = new List<HRM_Dot_DaoTaoChiTiet>();
        [NotMapped]
        public List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listChiPhi = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>();

    }
    public class HRM_Dot_DaoTaoChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdDotDaoTao { set; get; }
        [MaxLength(36)]
        public string IdKeHoachDaoTaoChiTiet { set; get; }
        public int SoNhanVien { get; set; }
        public double KeHoachKinhPhi { get; set; }
        [MaxLength(1000)]
        public string MoTa { set; get; }

        public string IddmNoiDungDaoTao { set; get; }
        [MaxLength(36)]
        public string IddmHinhThucDaoTao { set; get; }
        [MaxLength(10000)]
        public string NoiDung { set; get; }
        public DateTime NgayBatDau { set; get; }
        [NotMapped]
        public double NgayBatDauUnix { get; set; }

        public DateTime NgayKetThuc { set; get; }

        [NotMapped]
        public double NgayKetThucUnix { get; set; }

        [NotMapped]
        public List<string> listIdNoiDung { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem = new List<HRM_FileDinhKem>();
        [NotMapped]
        public bool isXoa { get; set; }
        [NotMapped]
        public List<HRM_Dot_DaoTaoNhanVien> listNhanVien = new List<HRM_Dot_DaoTaoNhanVien>();
        [NotMapped]
        public List<HRM_Dot_DaoTao_NoiDung> listNoiDungChung = new List<HRM_Dot_DaoTao_NoiDung>();


        [NotMapped]
        public HRM_TongHop_KeHoachDaoTaoChiTiet KeHoachTongHopDaoTaoChiTiet { set; get; }
    }
    public class HRM_Dot_DaoTaoNhanVien : Audit
    {
        [MaxLength(36)]
        public string IdChiTietDotDaoTao { set; get; }
        [MaxLength(36)]
        public string IdDotDaoTao { set; get; }

        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { set; get; }
        [MaxLength(250)]
        public string TenNhanVien { set; get; }
        public bool isHienThi { set; get; }
        [NotMapped]
        public bool isXoa { set; get; }
        [NotMapped]
        public List<HRM_Dot_DaoTao_NoiDung> listNoiDung = new List<HRM_Dot_DaoTao_NoiDung>();
    }
    public class HRM_Dot_DaoTao_NoiDung : Audit
    {
        [MaxLength(36)]
        public string IdChiTietDotDaoTao { set; get; }
        [MaxLength(36)]
        public string IdDotDaoTao { set; get; }

        [MaxLength(36)]
        public string IddmNoiDungDaoTao { set; get; }
        public int HinhThuc { get; set; }// 1 lý thuyế, 2 thực hành , 3 lý thuyết + thực hành

        public DateTime NgayDaoTao { set; get; }
        public string Ghichu { set; get; }
        public string KetQua { set; get; }
        [NotMapped]
        public double NgayDaoTaoUnix { set; get; }
        [NotMapped]
        public bool isXoa { set; get; }
    }
    public class HRM_ChuongTrinhDaoTao : AudiQuyTrinh
    {
        public long IdCongTy { get; set; }
        public int Nam { get; set; }
        public string IdKeHoachDaoTao { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public bool isDaoTaoMoi { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenKeHoach { get; set; }

        [NotMapped]
        public double DenNgayUnix { get; set; }
        [NotMapped]
        public List<HRM_ChuongTrinhDaoTao_NoiDung> ListNoiDung { get; set; }
        [NotMapped]
        public List<HRM_ChuongTrinhDaoTao_NhanVien> ListNhanVien { get; set; }
        [NotMapped]
        public List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listChiPhi = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>();
    }

    public class HRM_ChuongTrinhDaoTao_NoiDung : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IddmNoiDung { get; set; }
        public int HinhThucDaoTao { get; set; }  //1 lý thuyết, 2 thực hành, 3lý thuyết + thực hành, 4 cầm tay chỉ việc

        public string DiaDiemDaoTao { get; set; }
        public string NguoiDaoTao { get; set; }
        public string IdPhuongThucDaoTao { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public string TenNoiDung { get; set; }

        [NotMapped]
        public string TenHinhThuc { get; set; }
        [NotMapped]
        public List<string> ThoiGianDaoTao { get; set; }
        [NotMapped]
        public List<HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian> listThoiGianDaoTao { get; set; }


    }
    public class HRM_ChuongTrinhDaoTao_NhanVien : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public int SapXep { get; set; }

        public string TenBoPhan { get; set; }

        public string MaNhanVien { get; set; }

        public string TenNhanVien { get; set; }
        [NotMapped]
        public int LyThuyet { get; set; }
        [NotMapped]
        public int ThucHanh { get; set; }
        [NotMapped]
        public List<HRM_ChuongTrinhDaoTao_NhanVien_NoiDung> listDanhGia { get; set; }


    }
    public class HRM_ChuongTrinhDaoTao_NhanVien_NoiDung : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IddmNoiDung { get; set; }
        public string IdNhanVien { get; set; }
        public int HinhThucDaoTao { get; set; }//1 lý thuyế, 2 thực hành, 3,4 lý thuyết + thực hành   , 4 cầm tay chỉ việc
        public int LyThuyet { get; set; }// 1 đạt, 2 không đạt
        public int ThucHanh { get; set; }// 1 đạt, 2 không đạt
        [NotMapped]
        public string TenNoiDung { get; set; }
        [NotMapped]
        public string TenHinhThuc { get; set; }


    }
    public class HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IddmNoiDung { get; set; }
        public DateTime? Ngay { get; set; }
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        [NotMapped]
        public double? NgayUnix { get; set; }
        [NotMapped]
        public double? TuGioUnix { get; set; }
        [NotMapped]
        public double? DenGioUnix { get; set; }

    }

    public class HRM_Dot_DaoTaoChiPhiViewModel : Audit
    {

        public string Noidung { set; get; }
        public int Nam { get; set; }
        public int Thang { get; set; }
        [NotMapped]
        public List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listChiPhi { get; set; }
    }
    public class HRM_Dot_DaoTaoChiPhi : Audit
    {
        [MaxLength(36)]
        public string IdChiTietDotDaoTao { set; get; }
        public string KhoanChiPhi { set; get; }
        public string DonViTinh { set; get; }
        public int? SoLuong { set; get; }
        public double? DonGia { set; get; }
        public string GhiChu { set; get; }
        [MaxLength(36)]
        public string IdParent { get; set; }
        public int SapXep { set; get; }
        public string STT { get; set; }
        [NotMapped]
        public double ThanhTien { get; set; }
        [NotMapped]
        public string STT_Cha { get; set; }
    }

    #endregion

    #region đợi thi
    public class HRM_DotThi : Audit
    {
        [MaxLength(36)]
        public string IdChuongTrinhDaoTao { get; set; }
        public string Ma { get; set; }
        public string NoiDung { get; set; }
        public string GhiChu { get; set; }
        public DateTime Ngay { get; set; }
        public bool ChungDe { get; set; } // 0 một đề, 1 mỗi người một đề
        public string IddmCauTrucDe { get; set; }
        public int SoLuongOnline { get; set; }
        public int SoLuongOffline { get; set; }
        public int Dat { get; set; }
        public int Truot { get; set; }
        public string TrangThai { get; set; }// ChuaThi, DangThi, KetThuc
        public bool isKetThuc { get; set; }
        public int ThoiGianLamBai { get; set; }// phút
        [NotMapped]
        public double NgayUnix { get; set; }
        [NotMapped]
        public string TenTrangThai { get; set; }

        [NotMapped]
        public List<HRM_BaiThi> listBaiThi { get; set; }
        [NotMapped]
        public List<HRM_DotThi_NhanVien> listNhanVien { get; set; }

    }
    public class HRM_DotThi_NhanVien : Audit
    {
        [MaxLength(36)]
        public string IdDotThi { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string SoBaoDanh { get; set; }
        public double Diem { get; set; }
        public bool isKetThuc { get; set; } = false;
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public DateTime NgaySinh { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string GioiTinh { get; set; }

        [NotMapped]
        public string QueQuan { get; set; }
    }
    public class HRM_BaiThi : Audit
    {
        [MaxLength(36)]
        public string IdDotThi { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(36)]
        public string IdNguoiLamHo { get; set; }
        [MaxLength(36)]
        public string IdNguoiChamBai { get; set; }
        public string MaDe { get; set; }
        public string SoBaoDanh { get; set; }
        public bool isOnline { get; set; }
        public DateTime Ngay { get; set; }
        public double Diem { get; set; }
        public int ThoiGianLamBai { get; set; }// phút
        public string IddmCauTrucDe { get; set; }
        public double DiemDat { get; set; }
        public bool isKetThuc { get; set; }
        public int STT { get; set; }

        [NotMapped]
        public List<HRM_BaiThi_Item> listCauHoi { get; set; }

    }
    public class HRM_BaiThi_Item : Audit
    {
        public string Cau { get; set; }
        public string IdDotThi { get; set; }
        public string IdBaiThi { get; set; }
        public string IddmCauHoi { get; set; }
        public string IddmNhomCauHoi { get; set; }
        public string DapAn { get; set; }
        public string CauTraLoi { get; set; }
        public string Loai { get; set; }
        public double Diem { get; set; }
        public double DiemToiDa { get; set; }
        public int STT { get; set; }

        [NotMapped]
        public string NoiDung { get; set; }
        [NotMapped]
        public List<HRM_dmCauHoi_Item> listDapAn { get; set; }

        public HRM_BaiThi_Item() { }
        public HRM_BaiThi_Item(HRM_dmCauHoi a)
        {
            NoiDung = a.NoiDung;
            IddmCauHoi = a.IdGuid;
            DapAn = a.DapAn;
            Loai = a.Loai;
            IddmNhomCauHoi = a.IddmNhomCauHoi;
            DiemToiDa = a.Diem;
        }
    }
    public class HRM_LaySoBaoDanh
    {
        public List<string> listIdNhanVien { get; set; }
        public string MaDotThi { get; set; }
    }

    public class HRM_KhaoSatDaoTao : Audit
    {

        public string NoiDung { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayBDKhaoSat { get; set; }
        public DateTime NgayKTKhaoSat { get; set; }
        public string TrangThai { get; set; }// ChuaKhaoSat, BatDauKhaoSat, KetThuc
        public string LoaiKhaoSat { get; set; }// KhaoSatDaoTao. KhaoSatKetQua
        public string IdNhanVien { get; set; }
        public bool isKetThuc { get; set; }
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public int ThoiGianLamBai { get; set; }// phút
        [NotMapped]
        public string TenLoaiKhaoSat { get; set; }
        [NotMapped]
        public double NgayBDKhaoSatUnix { get; set; }
        [NotMapped]
        public double NgayKTKhaoSatUnix { get; set; }

        [NotMapped]
        public string TenTrangThai { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public List<HRM_KhaoSatDaoTao_CauHoi> listCauHoi { get; set; }
        [NotMapped]
        public List<string> listIdNhanVien { get; set; }
        [NotMapped]
        public List<HRM_KhaoSatDaoTao_NhanVienView> listNhanVien { get; set; }


    }
    public class HRM_KhaoSatDaoTao_CauHoi : Audit
    {
        public string IdKhaoSat { get; set; }
        public string IddmCauHoi { get; set; }
        public string IddmNhomCauHoi { get; set; }
        public string Loai { get; set; }
        public string NoiDung { get; set; }
        public string Cau { get; set; }
        // khảo sát kết quả
        public string DapAn { get; set; }
        // khảo sát đào tạo
        public int NhuCauDaoTao { get; set; }// 1 đã đào tạo, 2 đã đào tạo cần đào tạo lại, 3 chưa đào tạo
        public string MucTieuDatDuoc { get; set; }
        public int MucDo { get; set; } // 1 nâng cao, 2 cơ bản
        public DateTime ThoiGianDuKien { get; set; }
        public string IdHinhThucDaoTao { get; set; }
        public string DeXuatThem { get; set; }
        public string IdParent { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public double ThoiGianDuKienUnix { get; set; }
    }
    public class HRM_KhaoSatDaoTao_NhanVienView : Audit
    {
        public string IdNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public DateTime NgaySinh { get; set; }
        public string LoaiKhaoSat { get; set; }
        public string TrangThai { get; set; }
        public string NoiDung { get; set; }
        public string TenViTri { get; set; }
        public string TenBoPhan { get; set; }
        public List<HRM_KhaoSatDaoTao_NhanVien> listCauHoi { get; set; }
    }

    public class HRM_KhaoSatDaoTao_NhanVien : Audit
    {
        public string IdNhanVien { get; set; }
        public string IdKhaoSat { get; set; }
        public string IddmCauHoi { get; set; }
        public string IddmNhomCauHoi { get; set; }
        public string Loai { get; set; }
        public string TrangThai { get; set; }// DaKhaoSat, ChuaKhaoSat
        public string NoiDung { get; set; }
        public string Cau { get; set; }
        // khảo sát kết quả
        public string DapAn { get; set; }
        // khảo sát đào tạo
        public int NhuCauDaoTao { get; set; }// 1 đã đào tạo, 2 đã đào tạo cần đào tạo lại, 3 chưa đào tạo
        public string MucTieuDatDuoc { get; set; }

        public int MucDo { get; set; } // 1 nâng cao, 2 cơ bản
        public DateTime ThoiGianDuKien { get; set; }
        [NotMapped]
        public double ThoiGianDuKienUnix { get; set; }
        public string IdHinhThucDaoTao { get; set; }
        public string DeXuatThem { get; set; }
        public string IdParent { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public List<HRM_dmCauHoi_Item> listDapAn { get; set; }
        public HRM_KhaoSatDaoTao_NhanVien() { }
        public HRM_KhaoSatDaoTao_NhanVien(HRM_KhaoSatDaoTao_CauHoi a, string IdNhanVien)
        {
            this.IdNhanVien = IdNhanVien;
            IdKhaoSat = a.IdKhaoSat;
            IddmCauHoi = a.IddmCauHoi;
            IddmNhomCauHoi = a.IddmNhomCauHoi;
            Loai = a.Loai;
            NoiDung = a.NoiDung;
            Cau = a.Cau;
            //DapAn = a.DapAn;

        }
    }


    #endregion
}
