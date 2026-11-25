﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.Api.Entity.DinhBien;

namespace HRM.Api.Models
{
    public class HRM_QuaTrinhThamGiaBaoHiemXaHoi : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string IdUser { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string DonVi { get; set; }
        public double TienLuong { get; set; }
        public double MucLuongDong { get; set; }
        public string IdChucVu { get; set; }
        public bool isTuThem { get; set; } = false;
        public string TenCongTy { get; set; }
        public string TenBoPhan { get; set; }
        public string TenChucVu { get; set; }
        public double HeSo { get; set; }
        public bool isThamGia { get; set; } = true;
        public string SoBHXH { get; set; }
        public string GhiChu { get; set; }
        [NotMapped]
        public int Thang { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
    }
    public class HRM_QuaTrinhThamGiaBaoHiemThatNghiep : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string IdUser { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string DonVi { get; set; }
        public double TienLuong { get; set; }
        public double MucLuongDong { get; set; }
        public string IdChucVu { get; set; }
        public bool isTuThem { get; set; } = false;
        public string TenCongTy { get; set; }
        public string TenBoPhan { get; set; }
        public string TenChucVu { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
    }
    public class HRM_ThongTinHuongCheDoThaiSan : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string IdUser { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string DonVi { get; set; }
        public double SoTien { get; set; }
        public double LuongBHXH { get; set; }
        public string IdChucVu { get; set; }
        public string CheDo { get; set; }
        public bool isTuThem { get; set; } = false;
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
    }
    public class HRM_ThongTinBaoHiemNgoai : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public string IdUser { get; set; }
        public DateTime NgayMua { get; set; }
        public string DonVi { get; set; }
        public double SoTien { get; set; }
        public string IdChucVu { get; set; }
        public bool isTuThem { get; set; } = false;
        [NotMapped]
        public double NgayMuaUnix { get; set; }
    }
    public class HRM_QuyTrinhXacNhanTangGiamBaoHiem : AudiQuyTrinh
    {
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public string IdNguoiPheDuyet { get; set; }
        public DateTime? NgayPheDuyet { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        [NotMapped]
        public double NgayPheDuyetUnix { get; set; }
        [NotMapped]
        public double NgayBatDauUnix { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenNguoiPheDuyet { get; set; }

        public List<HRM_GiamBaoHiem> listGiam { get; set; }
        [NotMapped]
        public List<HRM_TangBaoHiem> listTang { get; set; }

    }

    public class HRM_GiamBaoHiem : Audit
    {
        [MaxLength(36)]
        public string IdQuyTrinh { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public double MucDong { get; set; }
        public string QuanLy { get; set; }
        public string LyDo { get; set; }
        public string Loai { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayGiam { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool isKetThuc { get; set; }
        public int SapXep { get; set; }
        public bool isChinhSua { get; set; } = false;
        public string IdNguoiPheDuyet { get; set; }
        public DateTime? NgayPheDuyet { get; set; }
        [NotMapped]
        public double NgayGiamUnix { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
      
        public string SoBHXH { get; set; }

    }
    public class HRM_TangBaoHiem : Audit
    {
        [MaxLength(36)]
        public string IdQuyTrinh { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public DateTime? ThoiGianDong { get; set; }
        public string Loai { get; set; }
        public double MucDong { get; set; }
        public double MucDongMoi { get; set; }
        public double PhanTramThu { get; set; }
        public string LyDo { get; set; }
        public string GhiChu { get; set; }
        public bool isKetThuc { get; set; }
        public int SapXep { get; set; }
        public bool isChinhSua { get; set; } = false;
        public string IdNguoiPheDuyet { get; set; }
        public DateTime? NgayPheDuyet { get; set; }
        [NotMapped]
        public double ThoiGianDongUnix { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
       
        public string SoBHXH { get; set; }



    }
}
