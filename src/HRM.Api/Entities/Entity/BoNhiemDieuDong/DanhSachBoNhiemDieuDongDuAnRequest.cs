﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.BoNhiemDieuDong
{
    public class DanhSachBoNhiemDieuDongDuAnRequest:EntityRequest
    {
        public string LoaiQuyetDinh { get; set; }
        public int Nam { get; set; }
        public string IdNhanVien { get; set; }
        public double TuNgayUnix { get; set; }
        public double DenNgayUnix { get; set; }
    }

    
}
