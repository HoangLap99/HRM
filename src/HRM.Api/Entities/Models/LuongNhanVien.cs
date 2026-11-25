﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Models
{
    public class LuongNhanVien
    {

    }
    public class HRM_LuongThucTe : Audit
    {
        public double LuongCoBan { set; get; }
        public double PhuCap { set; get; }
        public double ThuongChuyenCan { set; get; }
        public DateTime NgayBatDau { set; get; }
        //[NotMapped]
        public double NgayBatDauUnix { set; get; }
        public DateTime NgayKetThuc { set; get; }
      //  [NotMapped]
        public double NgayKetThucUnix { set; get; }
        public double HoTroDuAn { set; get; }
    }
}
