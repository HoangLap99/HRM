﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HRM.Api.Models
{
    public class HRM_BangLuong : Audit
    {
        [MaxLength(50)]
        public string SoPhieu { set; get; }
        [MaxLength(350)]
        public string Ten { set; get; }
        [MaxLength(350)]
        public string KyLuong { set; get; }
        public DateTime ThoiGian { set; get; }
        [NotMapped]
        public double ThoiGianUnix { set; get; }
        public DateTime NgayTao { set; get; }
        [NotMapped]
        public double NgayTaoUnix { set; get; }
        [MaxLength(36)]
        public string IdNguoiTao { set; get; }
        [MaxLength(150)]
        public string NguoiTao { set; get; }
        [MaxLength(36)]
        public string IdTrangThai { set; get; }
        public bool isKetThuc { set; get; }
        [MaxLength(350)]
        public string TenTrangThai { set; get; }
        public long IdCongTyEOS { set; get; }
        public long IdBoPhanEOS { set; get; }
        [MaxLength(3500)]
        public string NoiDung { set; get; }

        [NotMapped]
        public List<HRM_BangLuong_ChiTietNhanVienDA> listLuongChiTietDaiAn = new List<HRM_BangLuong_ChiTietNhanVienDA>();
        [NotMapped]
        public List<HRM_BangLuong_ChiTietNhanVien> listLuongChiTiet { set; get; }
        [NotMapped]
        public List<HRM_Link_BangLuong_ChamCong> listBangChamCong { set; get; }
        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem { set; get; }
    }
    public class HRM_Link_BangLuong_ChamCong : Audit
    {
        [MaxLength(36)]
        public string IdBangLuong { set; get; }
        [MaxLength(36)]
        public string IdBangChamCong { set; get; }
    }
    public class HRM_BangLuong_ChiTietNhanVien : Audit
    {
        [MaxLength(36)]
        public string IdBangLuong { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(50)]
        public string MaNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public double PhuCapLamNgoaiGio { set; get; }
        [MaxLength(150)]
        public string TenNhanVien { set; get; }
        public double ThoiGianLamViec { set; get; }
        public double NgayCongCoSo { set; get; }
        public double MucLuong { set; get; }
        public double SoNgayCongThucTe { set; get; }
        public double SoNgayLamViecDuThoiGian { set; get; }
        public double TroCapNgoai { set; get; }
        public double SoGioLamThucTe { set; get; }
        public double TangCa { set; get; }
        public double NghiLe { set; get; }
        public double NghiPhep { set; get; }
        public double LuongNgayLe { set; get; }
        public double NghiBu { set; get; }
        public double DiCongTac { set; get; }
        public double LuongNghiPhep { set; get; }
        public double LuongTheoSoNgayCong { set; get; }
        public double LuongTangCa { set; get; }
        public double TangCaNgayLe { set; get; }
        public double TangCaNgayCN { set; get; }
        public double TangCaNgayT7 { set; get; }
        public double LuongNghiBu { set; get; }
        public double PhuCapTheoCongChinh { set; get; }
        public double PhuCapKhongTheoCongChinh { set; get; }
        public double PhuCapTinhThue { set; get; }
        public double PhuCapKhongTinhThue { set; get; }
        public double TongCong { set; get; }
        public double KhauTruTaiNguon { set; get; }//10% khấu trừ với lao động thử việc cộng tác viên
        public double TongGiamTruTNNCN { set; get; }
        public double TongMucTNCN_NLD { set; get; }
        public double TongMucTNCN_NSD { set; get; }
        public double NSD_BHXH { set; get; }
        public double NSD_BHYT { set; get; }
        public double NSD_BHTN { set; get; }
        public double NSD_TongBH { set; get; }
        public double NSD_PhiCongDoan { set; get; }
        public double Tong_NSD { set; get; }
        public double NLD_BHXH { set; get; }
        public double NLD_BHYT { set; get; }
        public double NLD_BHTN { set; get; }
        public double NLD_TongBH { set; get; }
        public double NLD_PhiCongDoan { set; get; }
        public double Tong_NLD { set; get; }
        public int SoNguoiDuocGiamTru { set; get; }
        public double TongNhanChuaThueChuaGiamTru { set; get; }
        public double TongNhanChuaThueDaGiamTru { set; get; }
        public double TongNhan_NLD { set; get; }
        public int KieuLuong { set; get; }
        public double KhenThuong { set; get; }
        public double KyLuat { set; get; }
        public double TongGiamTruGiaCanh { set; get; }

    }
    public class HRM_Luong_KhenThuongKyLuat : Audit
    {
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinh { set; get; }
        public long  IdCongTy { get; set; }
        [MaxLength(100)]
        public string TableName { set; get; }
        public double TyLe { set; get; }
        public double SoThoiGian { set; get; }
        public double SoTien { set; get; }
        public DateTime Ngay { set; get; }
       
        public int Loai { set; get; }// 0 tawng, 1 giam
        public DateTime KyLuongThucHien { set; get; }
        public HRM_Luong_KhenThuongKyLuat()
        {

        }
        public HRM_Luong_KhenThuongKyLuat(HRM_QuyetDinhKyLuat _quyetdinh, HRM_QuyetDinhKyLuatNhanVien itemNV)
        {
            IdGuid = Guid.NewGuid().ToString();
            IdNhanVien = itemNV.IdNhanVien;
            SoTien = itemNV.SoTien;
            IdCongTy = _quyetdinh.IdCongTyEOS;
            Loai = 1;
            TyLe = itemNV.PhanTramPhat;
            IdQuyetDinh = _quyetdinh.IdGuid;
            KyLuongThucHien = _quyetdinh.NgayHieuLuc ?? DateTime.Now;
            TableName = nameof(HRM_QuyetDinhKyLuat);
        }

        public HRM_Luong_KhenThuongKyLuat(HRM_QuyetDinhKhenThuong _quyetdinh, HRM_QuyetDinhKhenThuongNhanVien itemNV)
        {
            IdGuid = Guid.NewGuid().ToString();
            IdNhanVien = itemNV.IdNhanVien;
            SoTien = itemNV.SoTien;
            Loai = 0;
            IdQuyetDinh = _quyetdinh.IdGuid;
            KyLuongThucHien = _quyetdinh.KyLuongThucHien;
            TableName = nameof(HRM_QuyetDinhKhenThuong);
        }
    }
}
