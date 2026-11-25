﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.HopDong
{
    public class GetDanhSachHopDongChuyenGiaRequest:EntityRequest
    {
        public string IdDuAn { get; set; }
        public string IdNhanVien { get; set; }
        public string TrangThai { get; set; }
    }
}
