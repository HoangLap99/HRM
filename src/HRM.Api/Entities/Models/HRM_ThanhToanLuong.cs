﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
    public class HRM_BangLuong_ChiTietNhanVienDA : Audit
    {
        [MaxLength(36)]
        public string IdBangLuong { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(150)]
        public string TenNhanVien { set; get; }
        [MaxLength(36)]
        public string MaNhanVien { set; get; }
        public long IdChuVu { set; get; }
        [MaxLength(250)]
        public string TenChucVu { set; get; }
        public double SoNgayCong { set; get; }
        public double SoNgayCongKy1 { set; get; }
        public double LuongChinh { set; get; }
        public double TongLuongPhuCap { set; get; }
        public double KiemNhiem { set; get; }
        public double LamThemGio { set; get; }
        public double BHXH_NSD { set; get; }
        public double CPCD_NSD { set; get; }
        public double BHXH_NLD { set; get; }
        public double CPCD_NLD { set; get; }
        public double ThucNhanKy1 { set; get; }
        public double ThucNhanKy2 { set; get; }
        [NotMapped]
        public List<HRM_BangLuong_PhuCap> listPhuCap { set; get; }
    }
    public class HRM_BangLuong_PhuCap : Audit
    {
        [MaxLength(36)]
        public string IdLuongChiTiet { set; get; }
        [MaxLength(300)]
        public string TenLoaiPhuCap { set; get; }
        public double SoTien { set; get; }
        public int Loai { set; get; }
        [MaxLength(36)]
        public string IddmLoaiPhuCap { set; get; }
    }
}
