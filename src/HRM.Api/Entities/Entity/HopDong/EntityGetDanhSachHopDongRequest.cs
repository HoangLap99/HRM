﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity
{
    public class EntityGetDanhSachHopDongRequest:EntityRequest
    {
        public string IdDuAn { get; set; }
        public string IdViTriChuyenMon { get; set; }
        public string IdNhanVien { get; set; }
        public string TrangThai { get; set; }
        public double TuNgayUnix { get; set; }
        public double DenNgayUnix { get; set; }
        public string IdLoaiHopDong { get; set; }
        public string IdThoihanHopDong { get; set; }
        public string TenNhanVien { get; set; }
        public string DienThoai { get; set; }
        public string IdChucDanh { get; set; }
        public string DiaDiemLamViec { get; set; }
        public long IdBoPhanEOS { get; set; }
        public bool isPhuLuc { get; set; }
    }
}
