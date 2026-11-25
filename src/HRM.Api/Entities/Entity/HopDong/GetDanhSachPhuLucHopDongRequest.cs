﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.HopDong
{
    public class GetDanhSachPhuLucHopDongRequest:EntityRequest
    {
        public string IdHopDong { get; set; }
    }
}
