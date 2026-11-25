﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
    public class LamNgoaiGio : Audit
    {

        public int Thang { get; set; }
        public int Nam { get; set; }
        public byte TrangThai { get; set; } //eTrangThaiQuyTrinhLamNgoaiGio
        public string IdNhanVienKeKhai { get; set; }
        public string IdBoPhanKeKhai { get; set; }
        public string NguoiKeKhai { get; set; }
        public string PhongBan { get; set; }
    }

    //public class HRM_QuyTrinhLamNgoaiGio : Audit
    //{
    //    public string IdQuyTrinhLamNgoaiGioTongHop { get; set; }
    //    public int Thang { get; set; }
    //    public int Nam { get; set; }

    //    public byte TrangThai { get; set; } //eTrangThaiQuyTrinhLamNgoaiGio
    //    public string IdNhanVienKeKhai { get; set; }

    //    public string IdBoPhanKeKhai { get; set; }

    //    [NotMapped]
    //    public string NhanVienKeKhai { get; set; }

    //    [NotMapped]
    //    public string KeKhaiTenBoPhan { get; set; }

    //    [NotMapped]
    //    public string KeKhaiTenChucVu { get; set; }


    //    [NotMapped]
    //    public string NhanVienChuyen { get; set; }

    //    [NotMapped]
    //    public string NguoiChuyenTenBoPhan { get; set; }

    //    [NotMapped]
    //    public string NguoiChuyenTenChucVu { get; set; }

    //    [NotMapped]
    //    public List<HRM_QuyTrinhLamNgoaiGioChiTiet> lstChiTiet { get; set; }
    //}


    //public class HRM_QuyTrinhLamNgoaiGioChiTiet : Audit
    //{
    //    public string IdQuyTrinhLamNgoaiGioTongHop { get; set; }
    //    public string IdQuyTrinhLamNgoaiGio { get; set; }
    //    public string IdDuAn { get; set; }
    //    public string LyDo { get; set; }
    //    public DateTime TuNgay { get; set; }
    //    public DateTime DenNgay { get; set; }

    //    public float SoGioLamThem { get; set; }
    //    public float SoGioLamLienTucCa { get; set; }

    //    public float SoGioQuyDoi { get; set; }
    //    public double MucHoTroTienAn { get; set; }
    //    public double TongTienAnCa { get; set; }
    //    public double TienChiuThueCa { get; set; }
    //    public double TongTienChiuThue { get; set; }

    //    [NotMapped]
    //    public double TuNgayUnix { get; set; }

    //    [NotMapped]
    //    public double DenNgayUnix { get; set; }

    //    [NotMapped]
    //    public bool isDeleted { get; set; }

    //    [NotMapped]
    //    public HRM_DuAn DuAn { get; set; }

    //}


    //public class HRM_QuyTrinhLamNgoaiGioTongHop : Audit
    //{
    //    public int Thang { get; set; }
    //    public int Nam { get; set; }
    //    public byte TrangThai { get; set; } //eTrangThaiQuyTrinhLamNgoaiGioTongHop

    //    [NotMapped]
    //    public string TenNhanVien { get; set; }

    //    [NotMapped]
    //    public string TenBoPhan { get; set; }

    //    [NotMapped]
    //    public string TenChucVu { get; set; }
    
    //    public HRM_QuyTrinhLamNgoaiGioTongHop() { }

    //    public HRM_QuyTrinhLamNgoaiGioTongHop(HRM_QuyTrinhLamNgoaiGio QuyTrinh)
    //    {
    //        this.Thang = QuyTrinh.Thang;
    //        this.Nam = QuyTrinh.Nam;
    //        this.TrangThai = (byte)DefineData.eTrangThaiQuyTrinhLamNgoaiGioTongHop.ChuaChuyenDuyet;
    //    }


    //    [NotMapped]
    //    public List<HDFile> lstFileDinhKemMoi { get; set; }

    //    [NotMapped]
    //    public List<QuyTrinhLamNgoaiGioTongHopChiTiet> lstChiTiet { get; set; }
    //}


    //public class QuyTrinhLamNgoaiGioTongHopChiTiet
    //{
    //    public string TenNhanVien { get; set; }
    //    public string PhongBan { get; set; }

    //    public float SoGioOT { get; set; }
    //    public float SoQuyDoi { get; set; }
    //    public double TongTienChiuThue { get; set; }

    //    public double TongTienMienThue { get; set; }

    //    public double TongCong
    //    {
    //        get
    //        {
    //            return TongTienChiuThue + TongTienMienThue;
    //        }
    //    } 
    //}

    //public class ThongKeLamNgoaiGio : Audit
    //{
        
    //    public string MaNhanVien { get; set; }
    //    public string TenNhanVien { get; set; }
    //    public string BoPhan { get; set; }

    //    public DateTime TuNgay { get; set; }

    //    public DateTime DenNgay { get; set; }

    //    public float Gio { get; set; }

    //    public string DuAn { get; set; }

    //    public string LyDo { get; set; }

    //    public string IdNhanVien { get; set; }

    //    public string IdBoPhan { get; set; }
    //    public int Nam { get; set; }
    //    public int Thang { get; set; }
    //}
}
