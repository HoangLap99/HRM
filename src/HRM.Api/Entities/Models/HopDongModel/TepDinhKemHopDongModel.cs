﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Models.HopDongModel
{
    public class TepDinhKemHopDongModel
    {
        public string IdHopDong { set; get; }
        public List<HRM_HopDongHoSoDinhKem> HopDongHoSoDinhKem { set; get; }
        public List<HDFile> lstFileDinhKemMoi { get; set; }
    }
}
