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
namespace HRM.Api.Models
{
    //public class DanhMuc
    //{
    //}
    #region Danh mục phụ cấp 
    public class HRM_dmPhuCap : Audit
    {
        [MaxLength(50)]
        public string Ma { set; get; }
        [MaxLength(200)]
        public string Ten { set; get; }
        public bool isTinhThueTNCN { set; get; }
        [MaxLength(500)]
        public string GhiChu { set; get; }
        public int SapXep { get; set; } = 1;
        public int Loai { set; get; }// hình thức hưởng
    }

    public class HRM_HopDongPhuCap : Audit
    {

        [MaxLength(36)]
        public string IddmLoaiPhuCap { set; get; }
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinh { set; get; }
        [MaxLength(36)]
        public string IdNhanVien { set; get; }
        [MaxLength(100)]
        public string TableName { set; get; }
        [NotMapped]
        public string SoHopDong { set; get; }
        [NotMapped]
        public string MaNhanVien { set; get; }

        public double SoTienTruocDieuChinh { set; get; }
        public double SoTien { set; get; }
        public double SoTienSauDieuChinh { set; get; }
        public DateTime? NgayBatDau { set; get; }
        [NotMapped]
        public double NgayBatDauUnix { set; get; }
        public DateTime? NgayKetThuc { set; get; }
        [NotMapped]
        public double NgayKetThucUnix { set; get; }

        public int LoaiPhuCap { set; get; }//THEO_GIO,THEO_NGAY....
        public bool isSuDung { set; get; }

        public int SapXep { get; set; }
        [NotMapped]
        public string Ten { get; set; }
        [NotMapped]
        public string TenPhuCap { get; set; }

        [NotMapped]
        public bool isDeleted { set; get; }
        public HRM_HopDongPhuCap Clone()
        {
            return (HRM_HopDongPhuCap)this.MemberwiseClone();
        }

    }
    public class HRM_HopDongPhuCapBaoHiem : Audit
    {

        [MaxLength(36)]
        public string IddmLoaiPhuCap { set; get; }
        [MaxLength(36)]
        public string IdHopDong { set; get; }
        [MaxLength(36)]
        public string IdQuyetDinh { set; get; }
        [MaxLength(100)]
        public string TableName { set; get; }
        [NotMapped]
        public string SoHopDong { set; get; }
        public double SoTienTruocDieuChinh { set; get; }
        public double SoTien { set; get; }
        public double SoTienDieuChinh { set; get; }
        public DateTime NgayBatDau { set; get; }
        [NotMapped]
        public double NgayBatDauUnix { set; get; }
        public DateTime NgayKetThuc { set; get; }
        [NotMapped]
        public double NgayKetThucUnix { set; get; }

        public int LoaiPhuCap { set; get; }//THEO_GIO,THEO_NGAY....
        public bool isSuDung { set; get; }
        public int SapXep { get; set; }

        [NotMapped]
        public bool isDeleted { set; get; }
        public HRM_HopDongPhuCapBaoHiem Clone()
        {
            return (HRM_HopDongPhuCapBaoHiem)this.MemberwiseClone();
        }

    }

    #endregion
    #region  Danh mục hồ sơ đính kèm
    public class HRM_HoSoDinhKem : Audit
    {
        [MaxLength(50)]
        public string Ma { set; get; }
        [MaxLength(200)]
        public string Ten { set; get; }
        [MaxLength(500)]
        public string GhiChu { set; get; }
        public int SapXep { get; set; }
    }

    #endregion
    #region  Danh mục Tai san ban giao
    public class HRM_dmTaiSanBanGiao : Audit
    {
        [MaxLength(50)]
        public string Ma { set; get; }
        [MaxLength(200)]
        public string Ten { set; get; }

        [MaxLength(500)]
        public string GhiChu { set; get; }
        public int SapXep { get; set; }
    }


    #endregion
    #region Nhân viên
    public class HRM_dmCauHinhThueThuNhapCaNhan : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(500)]
        public string Ten { get; set; }
        public double TyLe { set; get; }
        public double SoTien { set; get; }
        public double MucLuong { set; get; }
        public double STT { set; get; }
        public int SapXep { get; set; }
    }

    public class HRM_dmDanToc : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }


    public class HRM_dmTinhTrangHonNhan : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmLoaiHinhKhamSucKhoe : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmQuocTich : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }


    //public class HRM_dmChucVu : Audit
    //{
    //    public long IdChucVuEOS { get; set; }
    //    [MaxLength(50)]
    //    public string Ma { get; set; }
    //    [MaxLength(200)]
    //    public string Ten { get; set; }
    //    [MaxLength(500)]
    //    public string GhiChu { get; set; }
    //    public bool isChucVuDuAn { get; set; }
    //}

    public class HRM_dmChucVuCongViec : Audit
    {
        public long IdChucVuEOS { get; set; }
        [NotMapped]
        public string TenChucVu { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    //public class HRM_dmChucDanh : Audit
    //{
    //    public long IdChucVuSmartEOS { get; set; }
    //    [MaxLength(50)]
    //    public string Ma { get; set; }
    //    [MaxLength(200)]
    //    public string Ten { get; set; }
    //    [MaxLength(500)]
    //    public string GhiChu { get; set; }
    //    public byte Loai { get; set; } //eLoaiChucDanh
    //    [NotMapped]
    //    public List<HRM_DinhBien> listDinhBien = new List<HRM_DinhBien>();
    //}

    public class HRM_dmNhomChuyenMon : Audit
    {
        public long IdChucVuSmartEOS { get; set; }
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmViTriChuyenMon : Audit
    {
        public string IdNhomChuyenMon { get; set; }
        public string IdChucVu { get; set; }
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SoNgayNghiDuocHuong { get; set; }
        public int SapXep { get; set; }

        [NotMapped]
        public HRM_dmNhomChuyenMon NhomChuyenMon { get; set; }
    }

    public class HRM_dmCapBac : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }


    // Quan hệ gia đình
    public class HRM_dmQuanHeGiaDinh : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmLoaiNhanVien : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmHinhThucLamViec : Audit
    {
        [MaxLength(25)]
        public string Ma { set; get; }
        [MaxLength(200)]
        public string Ten { set; get; }
        [MaxLength(500)]
        public string MoTa { set; get; }
        public int SapXep { get; set; }
    }

    public class HRM_dmThoiGianLamViec : Audit
    {
        public long IdBoPhan { get; set; }
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public DateTime TuGio { get; set; }
        public DateTime DenGio { get; set; }
        public int SapXep { get; set; }
        [MaxLength(128)]
        public string IdChucDanh { get; set; }
        [NotMapped]
        public string TenCaLamViec { set; get; }

        //[NotMapped]
        //public HRM_dmChucDanh ChucDanh { get; set; }

        [NotMapped]
        public double TuGioUnix { get; set; }

        [NotMapped]
        public double DenGioUnix { get; set; }
    }


    public class HRM_dmHeSoNghiBu : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public float HeSo { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmAnCa : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(128)]
        public string IdLoaiNhanVien { get; set; }
        [MaxLength(128)]
        public string IdChucDanh { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }

        public double SoTien { get; set; }
        public int SapXep { get; set; }

        [NotMapped]
        public HRM_dmLoaiNhanVien LoaiNhanVien { get; set; }
        [NotMapped]
        public List<long> listIdChucDanh { get; set; }
        //[NotMapped]
        //public HRM_dmChucDanh ChucDanh { get; set; }
    }

    public class HRM_dmKhuVuc : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmKhuVucDiaDiem : Audit
    {
        [MaxLength(500)]
        public string DiaDiem { get; set; }
        [MaxLength(128)]
        public string IdKhuVuc { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmCongTacPhi : Audit
    {
        [MaxLength(128)]
        public string Ma { get; set; }
        [MaxLength(128)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string IddmLoaiChiPhiCongTac { get; set; }
        public double MucPhi { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public string TendmLoaiChiPhiCongTac { get; set; }

    }

    public class HRM_dmTinh : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }

    }


    public class HRM_dmQuanHuyen : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        [MaxLength(128)]
        public string IdTinh { get; set; }
        [NotMapped]
        public string MaTinh { get; set; }
        public int SapXep { get; set; }
    }


    public class DanhSachQuanHuyen : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        [MaxLength(200)]
        public string Tinh { get; set; }
        [MaxLength(128)]
        public string IdTinh { get; set; }
        public int SapXep { get; set; }

    }

    public class HRM_dmPhuongXa : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        [MaxLength(128)]
        public string IdQuanHuyen { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public string MaHuyen { get; set; }
    }

    public class DanhSachPhuongXa : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        [MaxLength(200)]
        public string QuanHuyen { get; set; }
        [MaxLength(128)]
        public string IdQuanHuyen { get; set; }
        [MaxLength(200)]
        public string Tinh { get; set; }

        [MaxLength(128)]
        public string IdTinh { get; set; }
        public int SapXep { get; set; }
    }


    public class HRM_dmNgonNgu : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }


    public class HRM_dmXepLoai : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmNhomChungChi : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmTrinhDoDaoTao : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }

        public int SapXep { get; set; }
    }



    #endregion



    #region Hợp đồng

    public class HRM_dmLoaiHopDong : Audit
    {
        //Hợp đồng chính thức, hợp đồng thử việc, phụ lục hợp đồng
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
        public bool isPhuLuc { get; set; }
    }

    public class HRM_dmThoiHanHopDong : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }


        public int SoThang { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }



    public class HRM_dmTrangThaiHopDong : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmHinhThucTraLuong : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmNhomLuong : Audit
    {
        [MaxLength(50)]
        public string Ma { set; get; }

        [MaxLength(200)]
        public string Ten { set; get; }
        [MaxLength(500)]
        public string GhiChu { set; get; }
        public int SapXep { get; set; }
    }


    public class HRM_dmTienTe : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmTonGiao : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmNhomPhucLoi : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }


    public class HRM_dmDiaDiemLamViec : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string DiaChi { get; set; }
        public int SapXep { get; set; }
    }


    public class HRM_dmLoaiTinhThueChuyenGia : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        public bool isDongThue { get; set; }

        public bool isGiamTru { get; set; }
        public double TyLe { get; set; }
        public int SapXep { get; set; }
    }

    #endregion


    #region Tuyển dụng


    public class HRM_dmNguonTuyenDung : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    #endregion

    #region Danh mục thiết bị
    public class HRM_dmTrangThietBi : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    #endregion

    #region Nghỉ phép



    public class HRM_dmThietLapNgayNghi : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        public DateTime TuNgay { get; set; }

        public DateTime DenNgay { get; set; }
        public bool isNghiLe { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }


    #endregion

    #region Đào tạo

    public class HRM_dmNoiDungDaoTao : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }

    }
    public class HRM_dmHinhThucDaoTao : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }

        public int SapXep { get; set; }

    }

    #endregion


    #region Thuế thu nhập cá nhân

    public class HRM_dmMucDongThueTNCN : Audit
    {
        public double TuTien { get; set; }
        public double DenTien { get; set; }
        public double PhanTramChiuThue { get; set; }
        public double SoTienTru { get; set; }
        public bool isGross { get; set; }

        public bool isThueChuyenGia { get; set; }
        public int SapXep { get; set; }
    }
    #endregion


    #region Cấu hình hệ thống

    public class HRM_dmCaiDatHeThong : Audit
    {
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(50)]
        public string Ma { get; set; }//eMaSetting: 
        public long ValueLong { get; set; }
        public double ValueDouble { get; set; }
        [MaxLength(500)]
        public string ValueString { get; set; }
        public DateTime ValueDateTime { get; set; }
        public int SapXep { get; set; }


    }
    #endregion


    #region Cấp bậc lương
    public class HRM_dmCapBacLuong : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public double Luong { get; set; }
        public string GhiChu { get; set; }
        public string IdViTriLamViecs { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public List<long> listIdViTriLamViec { get; set; }
        [NotMapped]
        public string TenViTriLamViecs { get; set; }
        [NotMapped]
        public List<HRM_dmCapBacLuong_PhuCap> listPhuCap { get; set; }
    }
    public class HRM_dmCapBacLuong_PhuCap : Audit
    {
        public string IdCapBacLuong { get; set; }
        public string IdPhuCap { get; set; }
        public double LuongPhuCap { get; set; }
        public int SapXep { get; set; }
        public string IddmCoSoApDungs { get; set; }
        [NotMapped]
        public List<long> ListIdCoSo { get; set; }
        [NotMapped]
        public string TenPhuCap { get; set; }
    }

    #endregion
    //#region Danh mục ngày nghỉ lễ

    //public class HRM_dmNgayNghiLe : Audit
    //{
    //    public string Ma { get; set; }
    //    public string Ten { get; set; }
    //    public DateTime ThoiGian { get; set; }
    //    public string GhiChu { get; set; }
    //}

    //#endregion


    #region Dùng chung


    #endregion

    #region Ngân hàng câu hỏi
    public partial class HRM_dmTieuChi : Audit
    {
        public string Ten { get; set; }
        public Nullable<int> Loai { get; set; }
        public Nullable<double> Diem { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public List<HRM_dmTieuChi_CauHoi> listCauHoi { get; set; }
    }
    public partial class HRM_dmTieuChi_CauHoi : Audit
    {
        public string IDTieuChi { get; set; }
        public string CauHoi { get; set; }
        public string CauTraLoi { get; set; }
        public Nullable<int> KhoiChuyenMon { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public bool isXoa { get; set; }
    }
    #endregion

    #region Chấm công

    public class HRM_dmDotChamCongThang : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int NgayBatDau { get; set; }//1..31 , >=1
        public int NgayKetThuc { get; set; }//Nếu = 31 => Cuối tháng
        public int SapXep { get; set; }
    }
    #endregion

    #region

    public class HRM_dmKyHieuChamCong : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public float TyLeLuong { get; set; }//1..31 , >=1
        public bool isdmNghi { get; set; } = false;
        public string Loai { get; set; } //M1: Muộn ca 1, M2: Muộn ca 2, S1: sớm ca 1, S2: sớm cuối giờ
        public int SapXep { get; set; }
    }

    public class HRM_dmLichLamViec : Audit
    {
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        public double? SoGioTrongNgay { get; set; }
        public double? HeSoLuongTangCa { get; set; }
        public int? SoNgayCongTieuChuan { get; set; }
        public bool? isCaGay { get; set; }
        public string Mau { get; set; }
        public bool isApDung { get; set; }
        public DateTime TuGio { get; set; }
        public DateTime DenGio { get; set; }
        public int LoaiNgayNghi { get; set; } // 1 nghỉ t7,cn ; 2 nghỉ chủ nhật  ; 3 làm 4 ngày nghỉ 2 ngày, 4 làm sáng t7, 5 bảo trì(lấy theo công tiêu chuẩn và không nghỉ t7 cn)
        public int SapXep { get; set; }
        [NotMapped]
        public double TuGioUnix { get; set; }
        [NotMapped]
        public double DenGioUnix { get; set; }

        [NotMapped]
        public List<HRM_dmLichLamViec_Ngay> listNgay { get; set; } = new List<HRM_dmLichLamViec_Ngay>();
    }

    public class HRM_dmLichLamViec_Ngay : Audit
    {
        public int? Thu { get; set; }
        public bool isNgayLamViec { get; set; } = true;
        public string IdLichLamViec { get; set; }
        [NotMapped]
        public string tenThu { get; set; }
        [NotMapped]
        public List<HRM_dmLichLamViec_Gio> listGio { get; set; } = new List<HRM_dmLichLamViec_Gio>();

    }

    public class HRM_dmLichLamViec_Gio : Audit
    {
        [MaxLength(36)]
        public string IdLichLamViecNgay { get; set; }
        [MaxLength(36)]
        public string IdLichLamViec { get; set; }
        public DateTime TuGio { get; set; }
        public DateTime DenGio { get; set; }
        public int? SapXep { get; set; }
        public int Ca { get; set; } //1 ca sáng, 2 ca chiều, 3 tối
        [NotMapped]
        public double TuGioUnix { get; set; }
        [NotMapped]
        public double DenGioUnix { get; set; }
    }

    public class HRM_dmLichNghiLe : Audit
    {
        [MaxLength(200)]
        public string Ten { get; set; }
        public bool isApDung { get; set; } = true;
        public double SoGioTrongNgay { get; set; }
        public long IdCongTy { get; set; }
        public string IdChucVu { get; set; }
        public int Nam { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
         [NotMapped]
        public List<long> listIdChucVu { get; set; }

        [NotMapped]
        public string TenCongTy { get; set; }

        [NotMapped]
        public List<HRM_dmLichNghiLe_Ngay> listNgay { get; set; } = new List<HRM_dmLichNghiLe_Ngay>();
    }
    public class HRM_dmLichNghiLe_BoPhan : Audit
    {
        public string IdLichNghiLe { get; set; }
        public long IdBoPhan { get; set; }
        public long IdCongTy { get; set; }
        public int Nam { get; set; }
    }
    public class HRM_dmLichNghiLe_Ngay : Audit
    {
        [MaxLength(36)]
        public string IdLichNghiLe { get; set; }
        [MaxLength(36)]
        public string IddmKyHieuChamCong { get; set; }
        public string Ten { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int? SapXep { get; set; }
        public float TyLeLuongLamThem { get; set; }//1..31 , >=1
        public float TyLeLuong { get; set; }//1..31 , >=1
        public float NgayCongHuong { get; set; }//1..31 , >=1
        public long IdCongTy { get; set; }
        public int Nam { get; set; }
        [NotMapped]
        public double TuNgayUnix { get; set; }
        [NotMapped]
        public double DenNgayUnix { get; set; }
    }
    public class HRM_dmThietLapBoPhan : Audit
    {
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdLichLamViec { get; set; }
        [MaxLength(36)]
        public string IdLichNghiLe { get; set; }
        public int SoNgayCong { get; set; }
        public int SapXep { get; set; }
    }
    #endregion

    public class HRM_dmHinhThucNghi : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }



    public class HRM_dmChucVuKiemSoatNoiBo : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(500)]
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmNganHang : Audit
    {
        [MaxLength(50)]
        public string Ma { get; set; }
        [MaxLength(200)]
        public string Ten { get; set; }
        [MaxLength(36)]
        public string IdNganHangNguon { get; set; }
        public int LoaiTaiKhoan { get; set; } // 1 công nhân, nhân viên ; 2 ICT
        public string MaBank { get; set; }
        public string ChiNhanh { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmNhomDanhGia : Audit
    {
        [StringLength(255)]
        public string Ma { get; set; }
        [StringLength(1000)]
        public string Ten { get; set; }
        [StringLength(int.MaxValue)]
        public string GhiChu { get; set; }
        public bool isHoatDong { get; set; }
        public int SapXep { get; set; }
        public byte Tui { set; get; }
        public bool IsHienThiTab { set; get; }  //Hiển thị ở tab lựa chọn
    }

    public class HRM_ThuVienDanhGia : Audit
    {
        [StringLength(255)]
        public string Ma { set; get; }
        [StringLength(3000)]
        public string Ten { set; get; }
        [StringLength(int.MaxValue)]
        public string GhiChu { set; get; }
        public string IdBoPhan { set; get; }
        public int SapXep { get; set; }
        //public List<string> IdBoPhan { set; get; }
        [NotMapped]
        public List<long> listIdBoPhan { set; get; }
        [NotMapped]
        public string TenBoPhan { set; get; }

        [NotMapped]
        public List<ReturnData<HRM_ThuVienDanhGia_Item>> listItem { get; set; }
    }

    public class HRM_ThuVienDanhGia_Item : Audit
    {
        [StringLength(36)]
        public string IdParent { set; get; }
        [StringLength(36)]
        public string IddmNhomDanhGia { set; get; }
        [StringLength(255)]
        public string Ma { set; get; }
        [StringLength(3000)]
        public string Ten { set; get; }
        [StringLength(3000)]
        public string YeuCau { set; get; }
        public float? DiemToiDa { get; set; }
        public float? DiemToiThieu { get; set; }
        public string CachDanhGia { get; set; }   //0 Chấm điểm, 1 Đạt - Không Đạt
        [StringLength(3000)]
        public string GhiChu { set; get; }
        public double? TyTrongQLCS { get; set; }
        public int? SapXep { get; set; }
        public bool? isCoSo { get; set; }
        public bool? isTeam { get; set; }
        public string BoPhanDanhGia { get; set; }
        [StringLength(36)]
        public string IdThuVienDanhGia { set; get; }
        [NotMapped]
        public bool? isRoot { get; set; } = false;
        [NotMapped]
        public bool? isShow { get; set; } = false;
        [NotMapped]
        public bool? isXem { get; set; } = false;
        [NotMapped]
        public bool? isCapNhat { get; set; } = false;

        [NotMapped]
        public string TenCachDanhGia { get; set; }   //0 Chấm điểm, 1 Đạt - Không Đạt
        [NotMapped]
        public string MadmNhomDanhGia { set; get; }
        [NotMapped]
        public string TendmNhomDanhGia { set; get; }
        [NotMapped]
        public string MaCha { set; get; }
        [NotMapped]
        public string MaThuVienDanhGia { set; get; }
        [NotMapped]
        public List<NhanSu> listNhanSu { get; set; }
        [NotMapped]
        public List<PhongBanView> listPhongBan { get; set; }
        public HRM_ThuVienDanhGia_Item()
        {

        }
        public HRM_ThuVienDanhGia_Item(string _Ma, string _Ten)
        {
            IdParent = null;
            IdGuid = _Ma;
            Ma = _Ma;
            Ten = _Ten;
        }

    }
    public class HRM_ThuVienDanhGia_NhanSu : Audit
    {
        public string IdThuVienDanhGia { get; set; }
        public string IdThuVienDanhGiaItem { get; set; }
        public int? IdPhongBan { get; set; }
        public string IdUser { get; set; }
        public bool? isXem { get; set; } = false;
        public bool? isCapNhat { get; set; } = false;
        public HRM_ThuVienDanhGia_NhanSu()
        {

        }
        public HRM_ThuVienDanhGia_NhanSu(NhanSu item)
        {
            IdGuid = Guid.NewGuid().ToString();
            IdUser = item.IdUser;
            isXem = item.isXem;
            isCapNhat = item.isCapNhat;
        }

        public HRM_ThuVienDanhGia_NhanSu(PhongBanView item)
        {
            IdGuid = Guid.NewGuid().ToString();
            IdPhongBan = item.Id;
            isXem = item.isXem;
            isCapNhat = item.isCapNhat;
        }
        public HRM_ThuVienDanhGia_NhanSu Clone()
        {
            return (HRM_ThuVienDanhGia_NhanSu)this.MemberwiseClone();
        }
    }
    public class HRM_BoPhan_ChucDanh : Audit
    {
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucDanh { get; set; }
    }
    public class HRM_BoPhan_ChucDanhViewModel
    {
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public string TenCongTy { get; set; }
        public string TenBoPhan { get; set; }
        public string TenChucDanh { get; set; }
        public List<long> listIdChucDanh { get; set; }
    }
    //public class HRM_ThuVienDanhGia_NhanSu : Audit
    //{
    //    public string IdThuVienDanhGia { get; set; }
    //    public string IdThuVienDanhGiaItem { get; set; }
    //    public int? IdPhongBan { get; set; }
    //    public string IdUser { get; set; }
    //    public bool? isXem { get; set; }
    //    public bool? isCapNhat { get; set; }
    //    public HRM_ThuVienDanhGia_NhanSu()
    //    {

    //    }
    //    public HRM_ThuVienDanhGia_NhanSu(NhanSu item)
    //    {
    //        IdGuid = Guid.NewGuid().ToString();
    //        IdUser = item.IdUser;
    //        isXem = item.isXem;
    //        isCapNhat = item.isCapNhat;
    //    }

    //    public HRM_ThuVienDanhGia_NhanSu(PhongBan item)
    //    {
    //        IdGuid = Guid.NewGuid().ToString();
    //        IdPhongBan = item.Id;
    //        isXem = item.isXem;
    //        isCapNhat = item.isCapNhat;
    //    }
    //}

    public class NhanSu
    {
        public int? Id { get; set; }
        public int? IdParent { get; set; }
        public string IdUser { get; set; }
        public string Ten { get; set; }
        public string ChucVu { get; set; }
        public bool? isShow { get; set; } = false;
        public bool? isXem { get; set; } = false;
        public bool? isCapNhat { get; set; } = false;
        public string IdThuVienDanhGiaItem { get; set; }
        public string TenBoPhan { get; set; }

        public NhanSu Clone()
        {
            return (NhanSu)this.MemberwiseClone();
        }

    }

    public class PhongBanView
    {
        public int? Id { get; set; }
        public int? IdParent { get; set; }
        public string TenBoPhan { get; set; }
        public bool? isXem { get; set; } = false;
        public bool? isCapNhat { get; set; } = false;
        public bool? isShow { get; set; } = false;
        public string IdThuVienDanhGiaItem { get; set; }
        public PhongBanView Clone()
        {
            return (PhongBanView)this.MemberwiseClone();
        }


    }

    #region câu hỏi
    public class HRM_dmNhomCauHoi : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public int STT { get; set; }
        public string GhiChu { get; set; }
        public bool isHoatDong { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmCauHoi : Audit
    {
        public string Ma { get; set; }
        public int SapXep { get; set; }
        public string NoiDung { get; set; }
        public string Loai { get; set; }
        public string DapAn { get; set; }
        public bool isTuChamDiem { get; set; }
        public string IdLoaiKhaoSat { get; set; }
        public string IddmNhomCauHoi { get; set; }
        public double Diem { get; set; }
        public bool isKhaoSat { get; set; } = false;
        [NotMapped]
        public string TendmNhomCauHoi { get; set; }
        [NotMapped]
        public string TenLoai { get; set; }

        [NotMapped]
        public List<HRM_dmCauHoi_Item> listCauTraLoi { get; set; }
    }
    public class HRM_dmCauHoi_Item : Audit
    {
        public string Ten { get; set; }
        public string IddmCauHoi { get; set; }
        public string NoiDung { get; set; }
        public int SapXepDapAn { get; set; }
        public int STT { get; set; }

        [NotMapped]
        public string MaCauHoi { get; set; }
    }

    public class HRM_dmCauTrucDe : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string DoiTuong { get; set; }
        public string LoaiHinhDuAn { get; set; }
        public double Diem { get; set; }
        public int SapXep { get; set; }
        [NotMapped]
        public List<HRM_dmCauTrucDe_Item> listItem { get; set; }
    }
    public class HRM_dmCauTrucDe_Item : Audit
    {
        public string IddmCauTrucDe { get; set; }
        public string IddmNhomCauHoi { get; set; }
        public int SoLuong { get; set; }
        public int STT { get; set; }
        [NotMapped]
        public string TendmNhomCauHoi { get; set; }
    }
    public class HRM_dmPhuongTien : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmLoaiChiPhiCongTac : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmNoiCap : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string DiaChi { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmNoiCapBaoHiem : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string DiaChi { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }

    public class HRM_dmHeSoLamThemGio : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public double HeSo { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
    }
    public class HRM_dmQuyDinhCongTacPhi : Audit
    {
        public string IddmLoaiChiPhi { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public double MucPhi { get; set; }
        public string GhiChu { get; set; }
        public int SapXep { get; set; }
        public DateTime? NgayBatDau { get; set; }
        [NotMapped]
        public double NgayBatDauUnix { get; set; }
    }
    public class HRM_DanhMucCongTacPhiViewModel
    {
        public string IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public string GhiChu { get; set; }
        public List<long> listIdBoPhan { get; set; }
        public string TenChucVu { get; set; }
        public string TenBoPhan { get; set; }
        public double MucPhi { get; set; }
        public string TenLoaiChiPhi { get; set; }
        public List<HRM_dmQuyDinhCongTacPhi> listCongTacPhi { get; set; }
    }

    #endregion
    public class HRM_dmThietLapBaoCaoCongViec : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string IdBoPhan { get; set; }
        public string GhiChu { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public List<long> listIdBoPhan { get; set; }

        [NotMapped]
        public List<ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>> listChiTiet { get; set; }
    }

    public class HRM_dmThietLapBaoCaoCongViec_ChiTiet : Audit
    {
        public string IdDanhMuc { get; set; }

        public int SapXep { get; set; }

        public string Ma { get; set; }
        public string Type { get; set; }
        public string CongThuc { get; set; }
        public string Ten { get; set; }
        public string IdParent { get; set; }
        public string IdKey { get; set; }
        public int Header { get; set; }

        [NotMapped]
        public int? rowspan { set; get; }
        [NotMapped]
        public int? colspan { set; get; }
        public HRM_dmThietLapBaoCaoCongViec_ChiTiet() { }


    }
    public class HRM_dmThietLapBangLuong : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string IdCongTy { get; set; }
        public string IdBoPhan { get; set; }
        public string GhiChu { get; set; }
        [NotMapped]
        public string TenCongTy { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

        [NotMapped]
        public List<long> listIdBoPhan { get; set; }
        [NotMapped]
        public List<long> listIdCongTy { get; set; }

        [NotMapped]
        public List<ReturnData<HRM_dmThietLapBangLuong_ChiTiet>> listChiTiet { get; set; }
    }

    public class HRM_dmThietLapBangLuong_ChiTiet : Audit
    {
        public string IdDanhMuc { get; set; }
        public string IddmLabel { get; set; }
        public long IdCongTy { get; set; }

        public int SapXep { get; set; }
        public bool isPhuCap { get; set; } = false;

        public string Ma { get; set; }
        public string IdParent { get; set; }
        public string IdKey { get; set; }
        public bool isDuocNhap { get; set; } = false;
        public int Header { get; set; }
        [NotMapped]
        public bool isThongTinCaNhan { get; set; } = false;
        [NotMapped]
        public bool isKhauTru { get; set; } = false;
        [NotMapped]
        public bool isMacDinh { get; set; } = false;
        [NotMapped]
        public bool isChon { get; set; } = false;
        [NotMapped]
        public int? rowspan { set; get; }
        [NotMapped]
        public int? colspan { set; get; }
        [NotMapped]
        public string Ten { get; set; }
        [NotMapped]
        public int? SoThapPhan { set; get; } = 0;
        public HRM_dmThietLapBangLuong_ChiTiet() { }
        public HRM_dmThietLapBangLuong_ChiTiet(HRM_dmLabelThietLapBangLuong item)
        {
            IddmLabel = item.IdGuid;
            isPhuCap = item.isPhuCap;
            isKhauTru = item.isKhauTru;
            isMacDinh = item.isMacDinh;
            Ma = item.Ma;
            Ten = item.Ten;
            isThongTinCaNhan = item.isThongTinCaNhan;
            isDuocNhap=item.isDuocNhap;
            SapXep = item.SapXep;
            Header = item.Header;
            IdGuid = item.IdGuid;
            IdParent = item.IdParent;
        }
        public HRM_dmThietLapBangLuong_ChiTiet(HRM_dmPhuCap item)
        {
            IddmLabel = item.IdGuid;
            isPhuCap = true;
            Ma = "PhuCap";
            Ten = item.Ten;
            SapXep = item.SapXep;

        }

    }

    public class HRM_dmLabelThietLapBangLuong : Audit
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }

        public string IddmPhuCap { get; set; }
        public bool isThongTinCaNhan { get; set; }
        public bool isPhuCap { get; set; }
        public bool isKhauTru { get; set; }
        public int Header { get; set; }
        public string DinhDang { get; set; }
        public int SoThapPhan { get; set; }
        public double DoRong { get; set; }
        public bool isMacDinh { get; set; } = false;
        public int SapXep { get; set; }
        public bool isDuocNhap { get; set; } = false;
        public string IdParent { get; set; }
        public string IdKey { get; set; }
        [NotMapped]
        public string MaCha { get; set; }
    }

}

