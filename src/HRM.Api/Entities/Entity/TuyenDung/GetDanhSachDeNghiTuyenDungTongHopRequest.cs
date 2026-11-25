﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.TuyenDung
{
    public class GetDanhSachDeNghiTuyenDungTongHopRequest:EntityRequest
    {
        public int Nam { get; set; }
        public string TabTrangThai { get; set; }
    }
}
