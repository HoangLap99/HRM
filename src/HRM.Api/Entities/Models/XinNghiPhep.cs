﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
    public partial class HRM_QuyTrinhLamThemGio : AudiQuyTrinh
    {

        public string SoPhieu { set; get; }
        public string IdBoPhan { get; set; }
        public long IdCongTy { set; get; }
        public string TenNguoiDeNghi { get; set; }
        [MaxLength(36)]
        public string IdNguoiDeNghi { get; set; }
        public bool DangKyHo { get; set; }

        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
        public bool Loai { set; get; }

        public string IdQuyTrinhDieuChinh { set; get; }

        //[NotMapped]
        //public string eAction { get; set; } = DefineData.eAction.QUYTRINHLAMTHEMGIO.ToString();


        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhLamThemGio_NhanVien> listUser { set; get; }
        [NotMapped]
        public List<string> listNoiDung { get; set; }
        [NotMapped]
        public HRM_QuyTrinhLamThemGio QuyTrinhSua { set; get; }

    }
    public class HRM_QuyTrinhLamThemGio_NhanVien
    {
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        public long IdBoPhan { get; set; }

        public string IdUser { set; get; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }

        [NotMapped]
        public List<HRM_QuyTrinhLamThemGio_ChiTiet> listNgay { get; set; }
    }
    public class HRM_QuyTrinhLamThemGio_ChiTiet : AuditId
    {

        [MaxLength(36)]
        public string IdQuyTrinh { set; get; }
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdUser { set; get; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public string IddmCaLamViec { get; set; }
        public string IddmHeSoTangCa { get; set; }
        public double HeSoTangCa { get; set; }
        public double TongSoGioGiao { get; set; }
        [NotMapped]
        public double ThoiGianBatDauUnix { get; set; }
        [NotMapped]
        public double ThoiGianKetThucUnix { get; set; }
        [NotMapped]
        public double TuGioUnix { get; set; }

        public double? SoGioNghi { get; set; } = 0;

        [NotMapped]
        public double DenGioUnix { get; set; }
        [NotMapped]
        public double NgayUnix { get; set; }
        [NotMapped]
        public DateTime? Ngay { get; set; }


        [NotMapped]
        public long IdDuAn { get; set; }


        public double ThoiGian { set; get; }
        public string GhiChu { set; get; }
        [NotMapped]
        public string MaNhanVien { set; get; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public List<HRM_KhungGioModel> listKhungGioByCa { get; set; }
        public void UnixToDateTime()
        {
            if (TuGioUnix == 0)
                TuGio = null;
            else
                TuGio = dungchung.UnixTimeStampToDateTime(TuGioUnix);
            if (NgayUnix == 0)
                Ngay = null;
            else
                Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);

            if (DenGioUnix == 0)
                DenGio = null;
            else
                DenGio = dungchung.UnixTimeStampToDateTime(DenGioUnix);
            if (ThoiGianBatDauUnix == 0)
                ThoiGianBatDau = null;
            else
                ThoiGianBatDau = dungchung.UnixTimeStampToDateTime(ThoiGianBatDauUnix);
            if (ThoiGianKetThucUnix == 0)
                ThoiGianKetThuc = null;
            else
                ThoiGianKetThuc = dungchung.UnixTimeStampToDateTime(ThoiGianKetThucUnix);

        }
        public void DateTimetoUnix()
        {
            if (TuGio == null)
                TuGioUnix = 0;
            else
                TuGioUnix = dungchung.DateTimeToUnixTimeStamp(TuGio);
            if (Ngay == null)
                NgayUnix = 0;
            else
                NgayUnix = dungchung.DateTimeToUnixTimeStamp(Ngay);

            if (DenGio == null)
                DenGioUnix = 0;
            else
                DenGioUnix = dungchung.DateTimeToUnixTimeStamp(DenGio);
            if (ThoiGianBatDau == null)
                ThoiGianBatDauUnix = 0;
            else
                ThoiGianBatDauUnix = dungchung.DateTimeToUnixTimeStamp(ThoiGianBatDau);
            if (ThoiGianKetThuc == null)
                ThoiGianKetThucUnix = 0;
            else
                ThoiGianKetThucUnix = dungchung.DateTimeToUnixTimeStamp(ThoiGianKetThuc);

        }
    }
    public class HRM_KhungGioModel
    {
        public string IddmCaLamViec { get; set; }
        public int GioBatDau_Gio { get; set; }
        public int GioBatDau_Phut { get; set; }
        public int GioKetThuc_Gio { get; set; }
        public int GioKetThuc_Phut { get; set; }
        public double TongThoiGian { get; set; }
        public double? SoGioNghi { get; set; } = 0;

    }
    public class HRM_TheoDoiLamThemGio : Audit
    {
        public long IdBoPhan { get; set; }
        public long IdCongTy { set; get; }

        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public DateTime? NgayDangKy { get; set; }
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public double ThoiGian { set; get; }
        public string GhiChu { set; get; }
        public HRM_TheoDoiLamThemGio() { }
        public HRM_TheoDoiLamThemGio(HRM_QuyTrinhLamThemGio_ChiTiet a)
        {
            IdNhanVien = a.IdUser;
            TuGio = a.TuGio;
            DenGio = a.DenGio;
            ThoiGian = a.ThoiGian;
            GhiChu = a.GhiChu;
        }
    }
    public partial class HRM_QuyTrinhXinNghiPhep : AudiQuyTrinh
    {
        //[MaxLength(36)]
        //public string Id { get; set; }
        public long IdCongTy { set; get; }
        public string IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdNguoiDeNghi { get; set; }
        public bool DangKyHo { get; set; } = false;
        [NotMapped]
        public string MucDich { get; set; }
        //[NotMapped]
        //public string eAction { get; set; } = DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString();
        [NotMapped]
        public string TenNguoiDeNghi { get; set; }
        [NotMapped]
        public Nullable<double> ThoiGianBatDauUnix { get; set; }
        [NotMapped]
        public Nullable<double> ThoiGianKetThucUnix { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
       

        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhXinNghiPhep_ChiTiet> listChiTietNgayNghi { set; get; }
        [NotMapped]
        public string SoPhieuQuyTrinhChinhSua { get; set; }

        [NotMapped]
        public string NoiDungNghi { get; set; }
        [NotMapped]
        public bool isDuocDieuChinh { set; get; }
        [NotMapped]
        public HRM_QuyTrinhXinNghiPhep QuyTrinhSua { set; get; }
    }

    public class HRM_QuyTrinhXinNghiPhep_ChiTiet : AuditId
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(36)]
        public string IdQuyTrinh { set; get; }
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdLoaiNghiPhep { set; get; }
        public string IdHinhThucNghi { set; get; }
        [MaxLength(1500)]
        public string GhiChu { set; get; }
        public double ThoiGianNghi { set; get; }
        public double SoPhepConLai { set; get; }
        public double CongLamThemGio { set; get; }
        public DateTime NgayBatDau { set; get; }
        [NotMapped]
        public double NgayBatDauUnix { set; get; }
     
        public DateTime? NgaySinh { set; get; }
        [NotMapped]
        public double NgaySinhUnix { set; get; }

        public DateTime NgayKetThuc { set; get; }
        [NotMapped]
        public string IdNguoiDeNghi { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { set; get; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucDanh { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }


    }
    public class HRM_TheoDoiXinNghiPhep : Audit
    {
        public long IdCongTy { set; get; }
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdLoaiNghiPhep { set; get; }
        public string IdNhanVien { set; get; }
        public string IdHinhThucNghi { set; get; }
        [MaxLength(1500)]
        public string GhiChu { set; get; }
        public double ThoiGianNghi { set; get; }// tính theo ngày
        public DateTime NgayNghi { set; get; }
        public DateTime? NgaySinh { set; get; }
        public DateTime? NgayBatDau { set; get; }
        public DateTime? NgayHuy { set; get; }
        public string LyDoHuy { get; set; }
        [NotMapped]
        public string HinhThucNghi { get; set; }
         [NotMapped]
        public string MaLoaiNghi { get; set; }

        public HRM_TheoDoiXinNghiPhep() { }
        public HRM_TheoDoiXinNghiPhep(HRM_QuyTrinhXinNghiPhep_ChiTiet a)
        {

            IdLoaiNghiPhep = a.IdLoaiNghiPhep;
            IdHinhThucNghi = a.IdHinhThucNghi;
            ThoiGianNghi = a.ThoiGianNghi;
            GhiChu = a.GhiChu;
        }
    }

    public class TongHopNgayPhep
    {
        public string TenCongTy { get; set; }
        public string TenNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public string TenBoPhan { get; set; }
        public string LoaiCong { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public double? ThamNien { get; set; }
        public double? ConLai { get; set; }
        public double? PhepThamNien { get; set; }
        public double? Phep { get; set; }
        public double? DaSuDung { get; set; }
        public List<TongHopNgayPhep_ChiTiet> listTiet { get; set; }
    }
    public class TongHopNgayPhep_ChiTiet
    {
        public int Thang { get; set; }
        public double? Phep { get; set; }
        public double? SuDung { get; set; }
    }
    public class ImportPhieuXinNghiPhepModel
    {
        public string STT { set; get; }
        public string MaNhanVien { set; get; }
        public string IdUser { set; get; }
        public string TenNhanVien { set; get; }
        public string MaBoPhan { set; get; }
        public long IdCongTy { set; get; }
        public long IdBoPhan { set; get; }
        public string NoiDung { set; get; }
        public string MaHinhThuc { set; get; }
        public long IddmHinhThuc { set; get; }
        public string MaLoaiNghi { set; get; }
        public string IdLoaiNghi { set; get; }
        public double ThoiGianNghi { set; get; }
        public DateTime ThoiGianBatDau { set; get; }
        public double ThoiGianBatDauUnix { set; get; }
        public DateTime ThoiGianKetThuc { set; get; }
        public double ThoiGianKetThucUnix { set; get; }
    }
}
