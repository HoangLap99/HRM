﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.NhanVien
{
    public class GetNhanVienKyHopDongModel
    {
        public string IdNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string ChucDanh { get; set; }
        public string QuocTich { get; set; }
    }
}
