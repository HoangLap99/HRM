﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
   


    public class HRM_KeHoachPhucLoi : Audit
    {
        [MaxLength(50)]
        public string SoPhieu { set; get; }
        [MaxLength(500)]
        public string Ten { get; set; }
        public long IdCongTyEOS { set; get; }
        public DateTime NgayLap { get; set; }
        [MaxLength(2500)]
        public string NoiDung { get; set; }
        public int Nam { set; get; }
        public double ChiPhiDuKien { get; set; }
        [MaxLength(36)]
        public string IdTrangThai { set; get; }
        public bool isKetThuc { set; get; } = false;
        [MaxLength(500)]
        public string TenTrangThai { set; get; }

        [NotMapped]
        public double NgayLapUnix { get; set; }

        [NotMapped]
        public List<HRM_KeHoachPhucLoiChiTiet> lstChiTiet { get; set; }

    }


    public class HRM_KeHoachPhucLoiChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdKeHoachPhucLoi { get; set; }

        public string TenChuongTrinh { get; set; }
        public string DiaDiem { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public DateTime HanDangKy { get; set; }

        public double ChiPhiDuKien { get; set; }

        public double SoNguoi { get; set; }
        public double ChiPhiMotNguoi { get; set; }
        public bool isDaSuDung { set; get; }
        public bool isTinhLuong { get; set; }

        [MaxLength(36)]
        public string IddmNhomPhucLoi { get; set; }

        [NotMapped]
        public double NgayBatDauUnix { get; set; }

        [NotMapped]
        public double NgayKetThucUnix { get; set; }

        [NotMapped]
        public double HanDangKyUnix { get; set; }

        [NotMapped]
        public bool isXoa { get; set; }

        [NotMapped]
        public HRM_dmNhomPhucLoi NhomPhucLoi { get; set; }
        [NotMapped]
        public List<HRM_KeHachPhucLoi_NhanVien> lstNhanVien = new List<HRM_KeHachPhucLoi_NhanVien>();
    }

    public class HRM_KeHachPhucLoi_NhanVien : Audit
    {
        [MaxLength(36)]
        public string IdKeHachPhucLoiChiTiet { set; get; }
        [MaxLength(36)]
        public string IdKeHoachPhucLoi { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [MaxLength(150)]
        public string TenNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public int SoLuong { set; get; }
        public double SoTien { set; get; }
        public double TongTien { set; get; }
        public bool isTinhLuong { get; set; }

        [NotMapped]
        public bool isXoa { get; set; }

    }

    public class HRM_QuyTrinh_PhucLoi: Audit
    {
        [MaxLength(50)]
        public string SoPhieu { set; get; }
        [MaxLength(500)]
        public string Ten { get; set; }
        public int Nam { set; get; }
        public long IdCongTyEOS { set; get; }
        public DateTime NgayLap { get; set; }
        [MaxLength(2500)]
        public string NoiDung { get; set; }
        [MaxLength(36)]
        public string IdKeHoachPhucLoi { set; get; }
        public double ChiPhiDuKien { get; set; }
        [MaxLength(36)]
        public string IdTrangThai { set; get; }
        [MaxLength(500)]
        public string TenTrangThai { set; get; }
        public bool isKetThuc { set; get; } = false;
        [NotMapped]
        public HRM_KeHoachPhucLoi KeHoacPhucLoi { set; get; }
        [NotMapped]
        public double NgayLapUnix { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinh_PhucLoiChiTiet> lstChiTiet { get; set; }
    }
    public class HRM_QuyTrinh_PhucLoiChiTiet : Audit
    {
        [MaxLength(36)]
        public string IdQuyTrinhPhucLoi { get; set; }
        [MaxLength(1500)]
        public string TenChuongTrinh { get; set; }
        public string DiaDiem { get; set; }
        [MaxLength(36)]
        public string IdChiTietKeHoach { set; get; }
        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public DateTime HanDangKy { get; set; }

        public double ChiPhiDuKien { get; set; }

        public double SoNguoi { get; set; }
        public double ChiPhiMotNguoi { get; set; }
        public bool isTinhLuong { get; set; }

        [MaxLength(36)]
        public string IddmNhomPhucLoi { get; set; }

        [NotMapped]
        public double NgayBatDauUnix { get; set; }

        [NotMapped]
        public double NgayKetThucUnix { get; set; }

        [NotMapped]
        public double HanDangKyUnix { get; set; }

        [NotMapped]
        public bool isXoa { get; set; }

        [NotMapped]
        public HRM_dmNhomPhucLoi NhomPhucLoi { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinh_PhucLoi_NhanVien> lstNhanVien = new List<HRM_QuyTrinh_PhucLoi_NhanVien>();
    }
    public class HRM_QuyTrinh_PhucLoi_NhanVien : Audit
    {
        [MaxLength(36)]
        public string IdQuyTrinhLoiChiTiet { set; get; }
        [MaxLength(36)]
        public string IdQuyTrinhPhucLoi { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
      
        public bool isKetThuc { get; set; }
        public string TenNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public long IdCongTyEOS { set; get; }
        public int SoLuong { set; get; }
        public bool isTinhLuong { get; set; }
        public DateTime Ngay { get; set; }
        public double SoTien { set; get; }
        public double TongTien { set; get; }
        [NotMapped]
        public bool isXoa { get; set; }

    }

    public class HRM_DieuKienPhucLoi : Audit
    {
        [MaxLength(36)]
        public string IdKeHoachPhucLoiChiTiet { get; set; }
        public bool isToanBoNhanVien { get; set; }

        public long IdBoPhan { get; set; }
        public string IdQuanHeGiaDinh { get; set; }
    }

    public class HRM_PhucLoiChiPhi : Audit
    {
        [MaxLength(36)]
        public string IdKeHoachPhucLoiChiTiet { get; set; }
        [MaxLength(500)]
        public string TenKhoanChiPhi { get; set; }

        public string SoTien { get; set; }

        public string GhiChu { get; set; }
    }

    public class KeHoachPhucLoi : Audit
    {
        [MaxLength(36)]
        public string IdKeHoachPhucLoi { get; set; }

        public string TenChuongTrinh { get; set; }
        public string DiaDiem { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public DateTime HanDangKy { get; set; }

        public double ChiPhiDuKien { get; set; }

        public string TrangThai { get; set; }
        [MaxLength(36)]
        public string IdNhomPhucLoi { get; set; }


        public double NgayBatDauUnix
        {
            get
            {
                return dungchung.DateTimeToUnixTimeStamp(NgayBatDau);
            }
        } 

        public double NgayKetThucUnix
        {
            get
            {
                return dungchung.DateTimeToUnixTimeStamp(NgayKetThuc);
            }
        }

        public double HanDangKyUnix
        {
            get
            {
                return dungchung.DateTimeToUnixTimeStamp(HanDangKy);
            }
        }

        public string TenNhomPhucLoi { get; set; }
    }
}
