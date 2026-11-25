﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Models.NhanVienModel
{
    public class Import_NhanVienModel
    {

        /// <summary>
        /// Column 1
        /// </summary>
        public string MaNhanVien { set; get; }
        public string Ho { set; get; }
        public string TenDem { set; get; }
        public string Ten { set; get; }
        public string TenDayDu { get; set; }
        public string NamThang_BatDau { set; get; }
        public string CalamViec { set; get; }
        public string ChucVu { set; get; }
        public string ChucDanhTheoNgachLuong { set; get; }
        public string ViTri_ChucDanh { set; get; }
        public string MaPhongBan { set; get; }
        public string BonPhan_Nhom { set; get; }
        public string TinhTrang_LamViec { set; get; }
        /// <summary>
        /// 
        /// Nghỉ phép
        /// </summary>
        public string TuNgay_NghiPhep { set; get; }

        public string NgayNghi_NghiViec { set; get; }
        /// <summary>
        /// Nghỉ việc
        /// </summary>
        public string LyDoNghi_NghiViec { set; get; }
        public string ViTriChuyenMon { set; get; }
        //     public string Nam_ThamNien { set; get; }
        public string CongTyDong_BHXH { set; get; }
        public string NamSinh { set; get; }
        public string NoiSinh { set; get; }
        public string GioiTinh { set; get; }
        public string SCMND_HT { set; get; }
        public string NgayCap_CMND_HT { set; get; }
        public string NoiCap_CMND_HT { set; get; }
        public string CMND_TheCanCuoc { set; get; }
        public string NgayCap_TheCanCuoc { set; get; }
        public string NoiCap_TheCanCuoc { set; get; }
        public string MaSoThueNhanVien { set; get; }
        public string NgayCap_MaSoThue { set; get; }
        //     public string HoSoGiamTruGiaCanh { set; get; }
        public string SoDienThoai { set; get; }
        public string EmailCaNhan { set; get; }
        public string TrinhDoHocVan_ChuyenMon { set; get; }
        public string TruongDaoTao { set; get; }
        public string NganhHoc { set; get; }
        public string Nam_TotNghiep { set; get; }

        #region Thường trú
        public string DiaChi_Thuongtru { set; get; }
        public string Ten_Thon_Xom_Thuongtru { set; get; }
        public string Ten_Xa_Phuong_Thuongtru { set; get; }
        public string Ten_Quan_Huyen_Thuongtru { set; get; }
        public string Ten_Tinh_TP_Thuongtru { set; get; }
        //   public string DiaChiThuongTru_Thuongtru { set; get; }
        #endregion
        #region Hiện tại
        public string DiaChi_HienTai { set; get; }
        public string Ten_Thon_Xom_HienTai { set; get; }
        public string Ten_Xa_Phuong_HienTai { set; get; }
        public string Ten_Quan_Huyen_HienTai { set; get; }
        public string Ten_Tinh_TP_HienTai { set; get; }
        //  public string DiaChiThuongTru_HienTai { set; get; }
        #endregion
        public string NguyenQuan { set; get; }
        public string TinhTrangHonNhan { set; get; }
        public string DanToc { set; get; }
        public string Quoctich { set; get; }
        public string TonGiao { set; get; }
        //   public string ThoiGianLamViec { set; get; }
        #region Người thân
        public string HoTen_NguoiThan { set; get; }
        public string QuanHe_NguoiThan { set; get; }
        public string DiaChi_NguoiThan { set; get; }
        public string SoDienThoai_NguoiThan { set; get; }
        #endregion
        //#region Hồ sơ nhân viên
        //public string TTUV { set; get; }
        ///// <summary>
        ///// Sơ Yếu lý lịch
        ///// </summary>
        //public string SYLL { get; set; }
        ///// <summary>
        ///// Chứng minh thư
        ///// </summary>
        //public string CMT { set; get; }
        ///// <summary>
        ///// Giấy khám sức khỏe
        ///// </summary>
        //public string KSK { set; get; }
        //public string SHK { set; get; }
        //public string Anh { set; get; }
        //public string BTN { set; get; }
        //public string CamKet { set; get; }
        //public string DXV { set; get; }
        //public string CKTNDS { set; get; }
        //#endregion
        #region Thông tin hợp đồng lao động
        //   public string QuaTrinhKy_HDLD { set; get; }
        public string ThoiGianHopDong { get; set; }
        public string NgayBatDau_HDLD { set; get; }
        public string NgayKetThuc_HDLD { set; get; }
        //    public string HDBH_ThoaThuan { set; get; }
        #endregion
        public string HinhThucLamViec { set; get; }
        public string So_HDLD { set; get; }
        public string LoaiHopDong { set; get; }
        public string NgayKy_HDLD { set; get; }
        public string NguoiDaiDien { set; get; }
        public string TrangThaiHopDong { set; get; }
        //#region Khen thưởng kỷ luật

        //#endregion
        #region Thông tin bảo hiểm xã hội
        public string MucDong_BHXH { set; get; }
        public string NgayThayDoiMucDong_BHXH { set; get; }
        public string Dong_BHXH_CongTy_TuThang { set; get; }
        //public string Dong_BHXH_CongTy_DenThang { set; get; }
        //      public string Dong_BHXH_TuThang { set; get; }
        //    public string Dong_BHXH_TNHH_ToiThang { set; get; }
        // public string Dong_BHXH_CP_TuThang { set; get; }
        //   public string Dong_BHXH_CP_ToiThang { set; get; }
        //    public string TinhTrang_So_BHXH { set; get; }
        /// <summary>
        /// Trích  (Có/Không tham gia BH) _Column 98
        /// </summary>
        public string So_So_BHXH { set; get; }

        public string TinhTrangThamGia_BH { set; get; }
        //   public string GhiChu_BHXH { set; get; }
        //  public string Ma_Nhom { set; get; }
        //   public string NgayBoNhiem_MienNhiem_DieuChinh { get; set; }
        //    public string ThoiHanThuThach { set; get; }
        public string HinhThucTraLuong { set; get; }
        public string LuongThucTe { set; get; }
        public string NgayBatDau_HuongLuong { set; get; }
        public string So_TK_NganHang { set; get; }
        public string NganHang { set; get; }
        #endregion
        //  public List<DieuChinhLuongModel> Luong { get; set; }
        public string PhuCap { set; get; }
        public string PhuCap_TrachNhiem { set; get; }
        public string PhuCap_AnTrua { set; get; }
        public string PhuCap_XangXe { set; get; }
        /// <summary>
        /// Phụ cấp nhà ở 
        /// </summary>
        public string PhuCap_Nha_O { set; get; }
        public string PhuCap_Khac { set; get; }
        public string TongPhuCap { set; get; }
        public string MaChamCong { set; get; }
        public string NgayNhap { set; get; }
    }
    public class NhanVienQuyetToanPhepViewModel
    {
        public string IdNhanVien { set; get; }
        public string IdHopDong { set; get; }
        public long IdBoPhanEOS { set; get; }
        public string TenBoPhan { set; get; }
        public long IdChucVuEOS { set; get; }
        public string TenChucVu { set; get; }
        public string TenNhanVien { set; get; }
        public string MaNhanVien { set; get; }
        public DateTime NgayBatDauLamViec { set; get; }
        public double NgayBatDauLamViecUnix { set; get; }
        public int Nam { set; get; }
        public double SoNgayPhep { set; get; }
        public double SoNgayDaNghi { set; get; }
        public double SoNgayPhepConLai { set; get; }
        public int SoNamCongThemNgayPhep { set; get; }
    }
}
