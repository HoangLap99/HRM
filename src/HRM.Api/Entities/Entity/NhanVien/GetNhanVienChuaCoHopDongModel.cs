﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.NhanVien
{
    public class GetNhanVienChuaCoHopDongModel
    {
        public string IdGuid { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime? NgaySinh { get; set; }
        public double NgaySinhUnix { get; set; }
        public string QuocTich { get; set; }
        public string NguyenQuan { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TrinhDoHocVan { get; set; }
        public string SoChungMinhThu { get; set; }
        public DateTime? NgayCap { get; set; }
        public double NgayCapUnix { get; set; }
        public string NoiCap { get; set; }
    }
}
