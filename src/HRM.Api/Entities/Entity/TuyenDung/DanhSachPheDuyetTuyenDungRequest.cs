﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.TuyenDung
{
    public class DanhSachPheDuyetTuyenDungRequest:EntityRequest
    {
        public double TuNgayUnix { get; set; }
        public double DenNgayUnix { get; set; }
        public string TabTrangThai { get; set; }
    }
}
