﻿#nullable disable
using HRM.Api.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HRM.Api.Models
{

    public class HRM_HopDong : AudiQuyTrinh
    {
        public long IdCongTyEOS { get; set; }
        //  public long IdDuAnEOS { get; set; }
        public long? IdCongTyKyHopDong { get; set; }
        [MaxLength(128)]
        public string SoHopDong { get; set; }
        [MaxLength(36)]
        public string IdLoaiHopDong { get; set; }
        [NotMapped]
        public string TenLoaiHopDong { set; get; }
        [MaxLength(36)]
        public string IdThoiHanHopDong { get; set; }
        [NotMapped]
        public string TenThoiHanHopDong { get; set; }
        public DateTime NgayKy { get; set; }
        [NotMapped]
        public double NgayKyUnix { get; set; }
        public int LoaiLuong { set; get; }
        #region Người đại diện 
        [MaxLength(36)]
        public string IdNguoiDaiDien { get; set; }
        [NotMapped]
        public string TeNguoiDaiDien { set; get; }

        [MaxLength(36)]
        public string NguoiDaiDienIdQuocTich { get; set; }
        public long NguoiDaiDienIdChucDanh { get; set; }
        public long NguoiDaiDienIdChucVu { get; set; }
        public DateTime? NgayChuyenKhongThoiHan { get; set; }
        [MaxLength(1000)]
        public string NguoiDaiDienDiaChi { get; set; }
        [MaxLength(50)]
        public string NguoiDaiDienMaSoThue { get; set; }

        #endregion

        #region Người lao động
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        [NotMapped]
        public string TenNhanVien { set; get; }
        [NotMapped]
        public string MaNhanVien { set; get; }
        public string ChucVuKiemSoatNoiBo { set; get; }
        public long IdNhanVienEOS { get; set; }
        [MaxLength(36)]
        public string IdQuocTichNhanVien { get; set; }
        public DateTime NgaySinh { get; set; }
        [MaxLength(500)]
        public string NguyenQuan { get; set; }
        [MaxLength(500)]
        public string DiaChi { get; set; }
        [MaxLength(250)]
        public string DienThoai { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(25)]
        public string SoCMND { get; set; }
        public DateTime? ChungMinhThuNgayCap { get; set; }
        [MaxLength(500)]
        public string ChungMinhThuNoiCap { get; set; }
        [MaxLength(36)]
        public string IdViTriChuyenMon { get; set; }
        public long IdBoPhanEOS { get; set; }

        public long IddmChucDanh { get; set; }

        public long IddmChucVu { get; set; }
        [NotMapped]
        public List<long> listIdCongTy { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        [MaxLength(36)]
        public string IdChucVuCongViec { get; set; }
        [MaxLength(36)]
        public string IdCapBac { get; set; }
        public string IddmTrinhDoDaoTao { get; set; }

        #endregion

        #region Mô tả công việc
        [MaxLength(50)]
        public string MaSoThueTNCN { get; set; }
        [MaxLength(1500)]
        public string DiaDiemLamViec { get; set; }
        [MaxLength(500)]
        public string DiaChiLamViec { set; get; }
        [MaxLength(500)]
        public string MoTaCongViec { get; set; }
        public string IdTrangThietBi { get; set; }
        [NotMapped]
        public string TrangThietBiLamViec { get; set; }
        #endregion
        public bool isThamGiaCongDoan { get; set; }
        public double PhiCongDoan { set; get; }
        public bool isNghiPhep { set; get; }
        public bool isHuongPhucLoi { set; get; }
        public double SoNgayNghiPhep { set; get; } = 12;
        public double SoNgayNghiPhepThamNien { set; get; }
        public bool isQuyetToanPhepNam { get; set; }//Có áp dụng quyết toán phép năm hay không
        public DateTime NgayHieuLuc { get; set; }
        [MaxLength(200)]
        public string NgayTraLuong { set; get; }
        [NotMapped]
        public double NgayHieuLucUnix { get; set; }
        public DateTime? NgayHetHan { get; set; }
        [NotMapped]
        public double? NgayHetHanUnix { get; set; }
        [NotMapped]
        public double? NgayChuyenKhongThoiHanUnix { get; set; }


        public string IdThoiGianLamViec { set; get; }

        [MaxLength(500)]
        public string LoaiHopDongNhanVien { get; set; } //eLoaiHopDongNhanVien
        [MaxLength(36)]
        public string TrangThai { get; set; }// nếu ký 1 hợp đồng mới thì hợp đồng cũ sẽ hết hiệu lực
        [MaxLength(36)]
        public string IdHinhThucLamViec { set; get; }
        [MaxLength(36)]
        public string IdHopDong { get; set; }

        public bool isPhuLuc { get; set; }
        [NotMapped]
        public string SoHopDongGoc { get; set; }

        [NotMapped]
        public virtual HRM_dmLoaiHopDong LoaiHopDong { get; set; }
        [NotMapped]
        public HRM_HopDong HopDongCha { set; get; }
        [NotMapped]
        public HRM_dmThoiHanHopDong ThoiHanHopDong { get; set; }
        [NotMapped]
        public HRM_NhanVien NguoiDaiDien { get; set; }

        [NotMapped]
        public HRM_dmQuocTich NguoiDaiDienQuocTich { get; set; }

        [NotMapped]
        public virtual HRM_NhanVien NguoiLaoDong { get; set; }

        [NotMapped]
        public HRM_dmQuocTich NguoiLaoDongQuocTich { get; set; }

        [NotMapped]
        public double NgaySinhUnix { get; set; }

        [NotMapped]
        public double ChungMinhThuNgayCapUnix { get; set; }

        [NotMapped]
        public HRM_dmViTriChuyenMon ViTriChuyenMon { get; set; }


        [NotMapped]
        public HRM_dmChucVuCongViec ChucVuCongViec { get; set; }

        [NotMapped]
        public HRM_dmCapBac CapBac { get; set; }
        [NotMapped]
        public HRM_dmHinhThucTraLuong HinhThucTraLuong { get; set; }
        [NotMapped]
        public HRM_dmHinhThucTraLuong HachToanHinhThucTraLuong { get; set; }
        [NotMapped]
        public double BaoHiemXaHoiNgayCapUnix { get; set; }
        [NotMapped]
        public double BaoHiemNgayBatDauUnix { get; set; }
        [NotMapped]
        public double BaoHiemNgayKetThucUnix { get; set; }

        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; } = new List<HRM_FileDinhKem>();
        [NotMapped]
        public List<HRM_FileDinhKem> listFileMoTaCongViec { get; set; } = new List<HRM_FileDinhKem>();


        [NotMapped]
        public List<HRM_HopDongPhuCap> lstPhuCap { set; get; }
        [NotMapped]
        public List<HRM_HopDongPhuCapBaoHiem> lstPhuCapBaoHiem { set; get; }

        [NotMapped]
        public BoPhan BoPhanEOS { get; set; }
        [NotMapped]
        public string TenBoPhanEOS { get; set; }


        [NotMapped]
        public BoPhan CongTyEOS { get; set; }
        [NotMapped]
        public HRM_Luong_HopDong LuongHopDong { set; get; }
        [NotMapped]
        public HRM_LuongBaoHiem_HopDong LuongBaoHiem { set; get; }

        [NotMapped]
        public HRM_BaoHiem_HopDong BaoHiem { set; get; } = new HRM_BaoHiem_HopDong();
        [NotMapped]
        public HRM_BaoHiem_HopDong BaoHiemNgoai { set; get; } = new HRM_BaoHiem_HopDong();
        [NotMapped]
        public List<HRM_HopDong_KhamSucKhoe> listKhamSucKhoe { set; get; } = new List<HRM_HopDong_KhamSucKhoe>();
        [NotMapped]
        public int TrangThaiHetHan { set; get; }// 0 chưa cảnh báo 1 bô vàng, 2 bôi đỏ
        [NotMapped]
        public bool isDeleted { get; set; }
        [NotMapped]
        public List<HRM_HopDongTaiSanBanGiao> listTrangThietBi { set; get; }

        [NotMapped]
        public List<ReturnData<HRM_HopDongCongViec>> listMoTaCongViec { set; get; }
        [NotMapped]
        public List<HRM_HopDong> listPhuLucHopDong { set; get; }
        [NotMapped]
        public List<string> listIdCaLamViec { set; get; }

        [NotMapped]
        public HRM_HopDong_QuyetDinh QuyetDinh { set; get; }


        [NotMapped]
        public bool isGiaHan { set; get; }
        [NotMapped]
        public bool isDoiTrangThai { set; get; }
        public HRM_HopDong Clone()
        {
            return (HRM_HopDong)this.MemberwiseClone();
        }
    }

    public class HRM_HopDongHoSoDinhKem : Audit
    {
        [MaxLength(128)]
        public string IdHoSoDinhKem { set; get; }
        [MaxLength(128)]
        public string IdHopDong { set; get; }
        [NotMapped]
        public string FileName { set; get; }
        [MaxLength(500)]
        public string MoTa { set; get; }

        public bool TrangThai { set; get; }
        public HRM_HopDongHoSoDinhKem Clone()
        {
            return (HRM_HopDongHoSoDinhKem)this.MemberwiseClone();
        }
    }
    public class HRM_HopDongTaiSanBanGiao : Audit
    {
        [MaxLength(36)]
        public string IdTaiSanBanGiao { set; get; }
        [NotMapped]
        public string TenTaiSanBanGiao { set; get; }
        [MaxLength(36)]
        public string IddmTrangThaiThietBi { set; get; }

        [MaxLength(36)]
        public string IdHopDong { set; get; }
        public DateTime NgayBanGiao { set; get; }
        [NotMapped]
        public double NgayBanGiaoUnix { set; get; }
        public DateTime NgayHoanTra { set; get; }
        [NotMapped]
        public double NgayHoanTraUnix { set; get; }
        public double SoLuong { set; get; }
        public double GiaTri { set; get; }
        [MaxLength(500)]
        public string MucDichSuDung { set; get; }
        [MaxLength(500)]
        public string MoTa { set; get; }
        [NotMapped]
        public string SoHopDong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public bool isDelete { set; get; }
        public HRM_HopDongTaiSanBanGiao Clone()
        {
            return (HRM_HopDongTaiSanBanGiao)this.MemberwiseClone();
        }
    }

    public class HRM_HopDongCongViec : Audit
    {

        [MaxLength(1500)]
        public string TenCongViec { set; get; }
        [MaxLength(36)]
        public string IdParent { get; set; }
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(1500)]
        public string MoTa { set; get; }
        [NotMapped]
        public string SoHopDong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string STT { get; set; }
        [NotMapped]
        public string STT_Cha { get; set; }


        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem { get; set; }
        public HRM_HopDongCongViec Clone()
        {
            return (HRM_HopDongCongViec)this.MemberwiseClone();
        }
    }

    public class HRM_Luong_HopDong : Audit
    {
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public double TyLeHuongLuong { get; set; }//Lương thực tế, lương hoạch toán
        public int KieuLuong { set; get; }// 0 Lương gross/ net 1
        public string HinhThucLuong { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinh { set; get; }
        public string TenNganHang { set; get; }
        [MaxLength(36)]
        public string IddmHinhThucTraLuong { set; get; }
        [MaxLength(1500)]
        public string ChiNhanh { set; get; }
        [MaxLength(150)]
        public string SoTaiKhoan { set; get; }
        public string MaSoThueTNCN { set; get; }
        public double LuongThucTe { set; get; }
        public double LuongBaoHiem { set; get; }
        public double? LuongDanhGia { set; get; }
        public bool isTinhLuong { set; get; }
        public bool isHienThiLenBangLuong { set; get; }

        public DateTime NgayBatDau { set; get; }
        [NotMapped]
        public double NgayBatDauUnix { set; get; }
        [NotMapped]
        public int LoaiLuong { set; get; }

        public DateTime? NgayKetThuc { set; get; }
        [NotMapped]
        public string SoHopDong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public double NgayKetThucUnix { set; get; }
        public HRM_Luong_HopDong Clone()
        {
            return (HRM_Luong_HopDong)this.MemberwiseClone();
        }

    }
    public class HRM_LuongBaoHiem_HopDong : Audit
    {
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        public double TyLeHuongLuong { get; set; }//Lương thực tế, lương hoạch toán
        public int KieuLuong { set; get; }// 0 Lương gross/ net 1
        public string HinhThucLuong { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinh { set; get; }
        public string TenNganHang { set; get; }
        [MaxLength(36)]
        public string IddmHinhThucTraLuong { set; get; }
        [MaxLength(1500)]
        public string ChiNhanh { set; get; }
        [MaxLength(150)]
        public string SoTaiKhoan { set; get; }
        public string MaSoThueTNCN { set; get; }
        public double LuongThucTe { set; get; }
        public double LuongBaoHiem { set; get; }
        public bool isHienThiLenBangLuong { set; get; }
        public bool isTinhLuong { set; get; }
        public DateTime NgayBatDau { set; get; }
        [NotMapped]
        public double NgayBatDauUnix { set; get; }
        public DateTime? NgayKetThuc { set; get; }
        [NotMapped]
        public string SoHopDong { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public double NgayKetThucUnix { set; get; }
        public HRM_LuongBaoHiem_HopDong() { }
        public HRM_LuongBaoHiem_HopDong(HRM_Luong_HopDong item)
        {
            IdNhanVien = item.IdNhanVien;
            TyLeHuongLuong = item.TyLeHuongLuong;
            KieuLuong = item.KieuLuong;
            HinhThucLuong = item.HinhThucLuong;
            TenNganHang = item.TenNganHang;
            IddmHinhThucTraLuong = item.IddmHinhThucTraLuong;
            ChiNhanh = item.ChiNhanh;
            SoTaiKhoan = item.SoTaiKhoan;
            MaSoThueTNCN = item.MaSoThueTNCN;
            NgayBatDau = item.NgayBatDau;
            NgayBatDauUnix = item.NgayBatDauUnix;
            NgayKetThuc = item.NgayKetThuc;
            NgayKetThucUnix = item.NgayKetThucUnix;
        }
        public HRM_LuongBaoHiem_HopDong Clone()
        {
            return (HRM_LuongBaoHiem_HopDong)this.MemberwiseClone();
        }
    }

    public class HRM_HopDong_KhamSucKhoe : Audit
    {
        public string IdNhanVien { get; set; }
        public string IdHopDong { get; set; }
        public string NoiKham { get; set; }
        public string LoaiHinhKham { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayKham { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        [NotMapped]
        public double NgayKhamUnix { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { get; set; }
        [NotMapped]
        public string MaNhanVien { get; set; }
        [NotMapped]
        public string SoHopDong { get; set; }
        [NotMapped]
        public string TenNhanVien { get; set; }
        [NotMapped]
        public string ThuongTru { get; set; }
        [NotMapped]
        public string CCCD { get; set; }
        [NotMapped]
        public DateTime NgayCap { get; set; }
        [NotMapped]
        public DateTime NgaySinh { get; set; }
    }

    public class HRM_BaoHiem_HopDong : Audit
    {
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }

        public bool isNgoai { set; get; } = false;
        //public double PhiCongDoan { set; get; }
        public bool isBaoHiemXaHoi { set; get; }
        public bool isBaoHiemTaiNan { set; get; }
        public double TyLeBaoHiemXaHoi { set; get; }
        public bool isBaoHiemThatNghiep { set; get; }
        public double TyLeBaoHiemThatNghiep { set; get; }
        public bool isBaoHiemYTe { set; get; }
        public double TyLeBaoHiemYTe { set; get; }
        public double MucDongBaoHiem { set; get; }
        public DateTime? NgayBatDau { set; get; }
        public DateTime? NgayBatDauDongBHXH { set; get; }
        public string SoTheBHYT { get; set; }
        public string NoiDangKy { get; set; }
        public string ThoiGian { get; set; }
        public string GhiChu { get; set; }
        public string LoaiThoiGian { get; set; }
        public double SoTienBaoHiem { get; set; }
        public double PhiBaoHiem { get; set; }
        public int Giam { get; set; } //1 giảm, 2 tăng
        [NotMapped]
        public double NgayBatDauUnix { set; get; }
        [NotMapped]
        public double NgayBatDauDongBHXHUnix { set; get; }

        public DateTime? NgayThayDoiMucDongBHXH { set; get; }
        [NotMapped]
        public double NgayThayDoiMucDongBHXHUnix { set; get; }

        public DateTime? NgayKetThuc { set; get; }
        [NotMapped]
        public double NgayKetThucUnix { set; get; }
        [NotMapped]
        public string SoHopDong { set; get; }
        [NotMapped]
        public string MaNhanVien { set; get; }

        public string SoSoBaoHiem { set; get; }
        public string NoiCapSoBaoHiem { set; get; }
        public DateTime? NgayCapSoBaoHiem { set; get; }
        [NotMapped]
        public double NgayCapSoBaoHiemUnix { set; get; }
        public bool isSuDung { set; get; }
        [NotMapped]
        public bool isDeleted { get; set; }
        public HRM_BaoHiem_HopDong Clone()
        {
            return (HRM_BaoHiem_HopDong)this.MemberwiseClone();
        }
    }
    public class HRM_HopDong_QuyetDinh : Audit
    {
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinh { set; get; }
        [MaxLength(100)]
        public string TableName { set; get; }
        [MaxLength(100)]
        public string MaQuyTrinh { set; get; }
        public HRM_HopDong_QuyetDinh Clone()
        {
            return (HRM_HopDong_QuyetDinh)this.MemberwiseClone();
        }
    }
    public class Link_NhanVien_BoPhan_ChucVu
    {
        public long Id { get; set; }
        public long NhanVienPkid { get; set; }
        public long IdCongTy { set; get; }
        public long BoPhanPkid { get; set; }
        public long ChucVuPkid { get; set; }
        public bool KiemNhiem { get; set; }
        public bool isTruongBoPhan { get; set; }
        public DateTime? NgayBatDau { get; set; }
        [NotMapped]
        public double NgayBatDauUnix { get; set; }
        [NotMapped]
        public double NgayKetThucUnix { get; set; }
        public bool unActivated { set; get; }
        public DateTime? NgayKetThuc { get; set; }
        public string TenChucVu { get; set; }//ID User
        public DateTime Created { get; set; }
    }
    public class Link_NhanVien_BoPhan_ChucVuNonSQL
    {
        public long Id { get; set; }
        public long NhanVienPkid { get; set; }
        public string TenNhanVienDayDu { get; set; }
        public long BoPhanPkid { get; set; }
        public string TenBoPhan { get; set; }
        public string MaBoPhan { get; set; }
        public long ChucVuPkid { get; set; }
        public string TenChucVu { get; set; }
        public string MaChucVu { get; set; }
        public bool KiemNhiem { get; set; }
        public bool isTruongBoPhan { get; set; }
        public DateTime NgayBatDau { get; set; }
        public double NgayBatDauUnix { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public double NgayKetThucUnix { get; set; }
        public Link_NhanVien_BoPhan_ChucVuNonSQL(HRM_HopDong data, long IdNVEOS)
        {
            Id = 0;
            NhanVienPkid = IdNVEOS;
            BoPhanPkid = data.IdBoPhanEOS;
            ChucVuPkid = data.IddmChucVu;
            KiemNhiem = false;
            isTruongBoPhan = false;
            NgayBatDau = data.NgayHieuLuc;
            NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(data.NgayHieuLuc);
            NgayKetThuc = data.NgayHetHan ?? DateTime.Now.AddYears(1);
            NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(data.NgayHetHan);
        }
        public Link_NhanVien_BoPhan_ChucVuNonSQL()
        {
        }

    }



    public class HRM_TheoDoiHopDong_NhanVien
    {
        public string CoSo { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string LoaiCong { get; set; }
        public string BoPhan { get; set; }
        public DateTime? NgayBatDauLamViec { get; set; }
        public string ThamNien { get; set; }

        // học viêc
        public string LoaiHopDong { get; set; }
        public string ThoiHan { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        // vụ việc khoán việc
        public HRM_TheoDoiHopDong_NhanVien_HopDong HopDongVuViec { get; set; }
        public HRM_TheoDoiHopDong_NhanVien_HopDong HopDong12Thang { get; set; }
        public HRM_TheoDoiHopDong_NhanVien_HopDong HopDong36Thang { get; set; }
        public HRM_TheoDoiHopDong_NhanVien_HopDong HopDongKhongThoiHan { get; set; }
    }
    public class HRM_TheoDoiHopDong_NhanVien_HopDong
    {
        public double Luong { get; set; }
        public double CacLoaiPhuCap { get; set; }
        public DateTime? Ngay { get; set; }
        public string LyDoDieuChinh { get; set; }
    }
}
