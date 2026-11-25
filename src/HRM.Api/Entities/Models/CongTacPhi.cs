﻿#nullable disable
using HRM.Api.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.IO;
using System.ComponentModel;
namespace HRM.Api.Models
{
    public class HRM_KeHoachCongTac : AudiQuyTrinh
    {
        [MaxLength(50)]
        public string SoPhieu { set; get; }

        public DateTime NgayDi { get; set; }
        public DateTime NgayVe { get; set; }
        public string NguoiLienHe { get; set; }
        public string SoDienThoai { get; set; }
        public string NoiDen { get; set; }

        public string IdNguoiTaoPhieu { get; set; }
        public long IdBoPhan { get; set; }
        public int Nam { set; get; }

        public double? LuongTuVan { set; get; }

        public int CachThucHien { set; get; }// 1 trực tiếp, 2 gián tiếp, 3 số tờ thông báo TD phát

        public string LyDo { get; set; }
        public string IddmPhuongTien { get; set; }
        public double ChiPhiDuKien { get; set; }
        public int TongNhanSu { get; set; }
        [MaxLength(36)]
        public string IdDotTuyenDung { set; get; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenNguoiTao { get; set; }

        [NotMapped]
        public List<string> listIddmPhuongTien { get; set; }

        [NotMapped]
        public double NgayDiUnix { get; set; }
        [NotMapped]
        public double NgayVeUnix { get; set; }
        [NotMapped]
        public List<HRM_KeHoachCongTac_NhanSuCongTac> listNhanVienCongTac { get; set; }
        [NotMapped]
        public List<HRM_KeHoachCongTac_ChiPhi> listChiPhi { get; set; }

    }


    public class HRM_KeHoachCongTac_NhanSuCongTac : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public DateTime NgayDi { get; set; }
        public DateTime NgayVe { get; set; }
        public double? CongTacPhi_1_Ngay { get; set; }
        public bool isKetThuc { set; get; } = false;
        public int SapXep { get; set; }
        public bool isLanhDao { get; set; } = false;
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public long? IdBoPhanEOS { get; set; }
        [NotMapped]
        public long? IdChucVuEOS { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }

        [NotMapped]
        public double NgayDiUnix { get; set; }
        [NotMapped]
        public double NgayVeUnix { get; set; }
        [NotMapped]
        public List<HRM_KeHoachCongTac_ChiPhi> listChiPhi { get; set; }
    }


    public class HRM_KeHoachCongTac_ChiPhi : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string NoiDung { get; set; }
        public int? SoLuong { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmQuyDinhChiPhi { get; set; }
        public string IddmLoaiChiPhi { get; set; }
        public string IddmCongTacPhi { get; set; }
        public double? DonGia { get; set; }
        public string GhiChu { get; set; }
        public int SoNgayDi { get; set; } = 1;
        public double HeSo { get; set; } = 1;
        public string IdParent { get; set; }
        public int SapXep { get; set; }

        public double ThanhTien { get; set; }
        [NotMapped]
        public string TenLoaiChiPhi { get; set; }

    }


    public class HRM_KetQuaCongTac : AudiQuyTrinh
    {
        [MaxLength(50)]
        public string SoPhieu { set; get; }
        public string IdKeHoachCongTac { get; set; }
        public DateTime NgayDi { get; set; }
        public DateTime NgayVe { get; set; }
        public DateTime NgayBaoCao { get; set; }
        public string NguoiLienHe { get; set; }
        public string SoDienThoai { get; set; }
        public string NoiDen { get; set; }
        public long IdBoPhan { get; set; }
        public string LyDo { get; set; }
        public string IddmPhuongTien { get; set; }
        public double? LuongTuVan { set; get; }

        public int CachThucHien { set; get; }// 1 trực tiếp, 2 gián tiếp, 3 số tờ thông báo TD phát
        public string KetQua { get; set; }
        public int Nam { set; get; }
        public double ChiPhiDuKien { get; set; }
        public int TongNhanSu { get; set; }
        [MaxLength(36)]
        public string IdDotTuyenDung { set; get; }

        public string IdNguoiTaoPhieu { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenNguoiTao { get; set; }

        [NotMapped]
        public List<string> listIddmPhuongTien { get; set; }


        [NotMapped]
        public double NgayDiUnix { get; set; }
        [NotMapped]
        public double NgayVeUnix { get; set; }
        [NotMapped]
        public double NgayBaoCaoUnix { get; set; }
        [NotMapped]
        public List<HRM_KetQuaCongTac_NhanSuCongTac> listNhanVienCongTac { get; set; }
        [NotMapped]
        public List<HRM_KetQuaCongTac_NhanSuTuyen> listNhanVienTuyen { get; set; }

        [NotMapped]
        public List<HRM_KetQuaCongTac_ChiPhi> listChiPhi { get; set; }

    }
    public class HRM_KetQuaCongTac_NhanSuTuyen : Audit
    {

        public string MaNhanVien { get; set; }
        public string IdQuyTrinh { get; set; }
        public string Ho { get; set; }
        public string Ten { get; set; }
        public string TenDem { get; set; }
        public string TenDayDu { get; set; }
        public string SoDienThoai { get; set; }
        public string GioiTinh { get; set; }
        public string CanCuocCongDan { get; set; }
        public string NguoiGioiThieu { get; set; }
        public DateTime NgaySinh { get; set; }
        public double? CongTacPhi_1_Ngay { get; set; }
        public bool isKetThuc { set; get; } = false;
        public int SapXep { get; set; }
        public long IdCongTyEOS { get; set; }

        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }

        [NotMapped]
        public double NgayDiUnix { get; set; }
        [NotMapped]
        public double NgaySinhUnix { get; set; }

        [NotMapped]
        public double NgayVeUnix { get; set; }
    }
    public class HRM_KetQuaCongTac_NhanSuCongTac : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public DateTime NgayDi { get; set; }
        public string KetQua { get; set; }
        public DateTime NgayVe { get; set; }
        public double? CongTacPhi_1_Ngay { get; set; }
        public bool isKetThuc { set; get; } = false;
        public int SapXep { get; set; }
        public bool isLanhDao { get; set; } = false;

        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public long? IdBoPhanEOS { get; set; }
        [NotMapped]
        public long? IdChucVuEOS { get; set; }

        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [NotMapped]
        public double NgayDiUnix { get; set; }
        [NotMapped]
        public double NgayVeUnix { get; set; }
        [NotMapped]
        public List<HRM_KetQuaCongTac_ChiPhi> listChiPhi { get; set; }
    }


    public class HRM_KetQuaCongTac_ChiPhi : Audit
    {
        public string IdQuyTrinh { get; set; }
        public string IdNhanVien { get; set; }
        public int? SoLuong { get; set; }
        public string NoiDung { get; set; }
        public string IddmLoaiChiPhi { get; set; }
        public string IddmCongTacPhi { get; set; }
        public int SoNgayDi { get; set; } = 1;
        public double HeSo { get; set; } = 1;
        public double? DonGia { get; set; }
        public string GhiChu { get; set; }
        public string IdParent { get; set; }
        public bool isKetThuc { get; set; }
        public DateTime NgayBaoCao { get; set; }
        public int SapXep { get; set; }

        public double ThanhTien { get; set; }
    }
    public class HRM_BaoCaoCongTacPhi
    {
        public string TenChiPhi { get; set; }
        public string IdQuyTrinh { get; set; }
        public string NoiDung { get; set; }
        public List<HRM_BaoCaoCongTacPhi_Thang> listThang { get; set; }
    }
    public class HRM_BaoCaoCongTacPhi_Thang
    {
        public int Thang { get; set; }
        public double ChiPhi { get; set; } = 0;
        public List<HRM_BaoCaoCongTacPhi_NhanVien> ListNhanVien { get; set; }
    }
    public class HRM_BaoCaoCongTacPhi_NhanVien
    {
        public string TenNhanVien { get; set; }
        public double SoTien { get; set; }
    }
}

