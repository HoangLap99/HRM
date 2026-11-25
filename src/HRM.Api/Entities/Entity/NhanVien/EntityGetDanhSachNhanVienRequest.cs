﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity
{
    public class EntityGetDanhSachNhanVienRequest:EntityRequest
    {
        public string TenNhanVien { get; set; }
        public string IdNhanVien { get; set; }
        //public string ViTriChuyenMon { get; set; }
        //public double NgayBatDauUnix { get; set; }
        //public double NgayKetThucUnix { get; set; }
        public long IdDuAnEOS { get; set; }

        public long IdChucVuEOS { get; set; }
        //public string DienThoai { get; set; }
        //public string GioiTinh { get; set; }
        //public double NgaySinhUnix { get; set; }
        //public string SoChungMinhThu { get; set; }
        //public string HoKhauThuongTruDiaChi { get; set; }
        //public string NguyenQuan { get; set; }
        public string TrangThai { get; set; }
        public long IdBoPhanEOS { get; set; }
        public long IdChucDanhEOS { get; set; }
        public long IdCongTyEOS { set; get; }
        public string IdLoaiNhanVien { get; set; }
        public int Nam { get; set; }
        public int Thang { get; set; }
        public int Ngay { get; set; }
        public double  NgayUnix { get; set; }
        public double  TuNgayUnix { get; set; }
        public double  DenNgayUnix { get; set; }
        public int isCMT { get; set; } //1 có ,2 không

        public List<long> ListIdCongTyEOS { get; set; } = new List<long>();
        public List<long> listIdBoPhan { get; set; } = new List<long>();
        public string GioiTinh { get; set; }
        public bool Ma { set; get; }

    }
}
