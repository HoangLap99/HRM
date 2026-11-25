﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Models.ChamCongModel
{
    public class DanhSachChamCongThangViewModel
    {
        public string IdNhanVien { set; get; }
        public string MaNhanVien { set; get; }
        public string TenNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public int Nam { set; get; }
        public int Thang { set; get; }
        public double TongSoGio { set; get; }
        public int NgayCongThucTe { set; get; }
        public string ChucVu { set; get; }
        public double LuongCoBan { set; get; }
        public double LuongThucTe { set; get; }
        public double ThucLinh { set; get; }
        public int? NgayCongCoSo { set; get; }
        public int MaHinhThucLamViec { set; get; }
        public double LuongBaoHiem { set; get; }
        public double TienDongBaoHiem { set; get; }
        public double LuongNgayCongThucTe { set; get; }
        public double PhuCapHoTro { set; get; }
        public string TenBoPhan { set; get; }
        public double TienCongTyDongBaoHiem { set; get; }
    }

    public class LuongBoPhanModel
    {
        public long IdBoPhanEOS { set; get; }
        public double LuongBaoHiem { set; get; }
        public double TienNguoiLaoDongNopBaoHiem { set; get; }
        public double TienCongTyNopBaoHiem { set; get; }
        public double PhuCapHoTro { set; get; }
        public double ThucLinh { set; get; }
        public string TenBoPhan { set; get; }
        public int Nam { set; get; }
        public int Thang { set; get; }
    }
}
