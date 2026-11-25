﻿#nullable disable
//using HRM.Api.Models.SmartEOEModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
    public class TinhLuong
    {
    }

    public class ChamCongChiTietNgay
    {
        public long IdNhanVien { get; set; }
        public DateTime Ngay { get; set; }
        public float NgayLuong { get; set; }
    }

    public class DanhSachBangChamCongTongHopDuocDuyet
    {
        public long Id { get; set; }
        public string Ten { get; set; }

        public int Nam { get; set; }
        public int Thang { get; set; }
        public string PhongBan { get; set; }

        public int SoNhanVien { get; set; }
        public byte TrangThai { get; set; }
      
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }//ID User
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }//ID User
    }

    public class LuongLaoDongVietNam : Audit
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string TieuDe { get; set; }
        public int SoNhanVien { get; set; }
        public string IdLuongLaoDongVietNamTongHop { get; set; }
    }

    //public class HRM_LuongLaoDongVietNamTongHop : Audit
    //{
    //    public int Thang { get; set; }
    //    public int Nam { get; set; }
    //    public string TieuDe { get; set; }


    //    public HRM_LuongLaoDongVietNamTongHop() { }

    //    public HRM_LuongLaoDongVietNamTongHop(HRM_LuongLaoDongVietNam LuongLaoDongVietNam)
    //    {
    //        this.Thang = LuongLaoDongVietNam.Thang;
    //        this.Nam = LuongLaoDongVietNam.Nam;
    //    }


    //    [NotMapped]
    //    public int SoNhanVien { get; set; }

    //}

    //public class HRM_LuongLaoDongVietNam : Audit
    //{
    //    public int Thang { get; set; }
    //    public int Nam { get; set; }
    //    public DateTime NgayThang { get; set; }
    //    public string TieuDe { get; set; }
    //    public string Loai { get; set; } //eLoaiBangLuong

    //    public string IdLuongLaoDongVietNamTongHop { get; set; }

    //    [NotMapped]
    //    public List<HRM_LuongLaoDongVietNamChamCong> lstLuongLaoDongVietNamChamCong { get; set; }

    //    [NotMapped]
    //    public List<HRM_LuongLaoDongVietNamNhanVien> lstLuongLaoDongVietNamNhanVien { get; set; }

    //    [NotMapped]
    //    public int SoNhanVien { get; set; }
    //}


    //public class HRM_LuongLaoDongVietNamChamCong : Audit
    //{
    //    public string IdLuongLaoDongVietNam { get; set; }
    //    public long IdChamCongVanPhongTongHop { get; set; }

    //    public long IdChamCongDuAn { get; set; }
    //}


    //public class HRM_LuongLaoDongVietNamNhanVien : Audit
    //{
    //    public string IdLuongLaoDongVietNam { get; set; }
    //    public string IdLuongLaoDongVietNamTongHop { get; set; }
    //    public long IdChamCongVanPhong { get; set; }
    //    public long IdChamCongDuAnChiTiet { get; set; }
    //    public long IdNhanVienEOS { get; set; }
    //    public string IdNhanVien { get; set; }
    //    public string IdHopDong { get; set; }
    //    public string IdBoNhiemDieuDong { get; set; }

    //    public bool isDaTraLuong { get; set; }
    //    public float NgayCongCoSo { get; set; }
    //    public float NgayCongThucTe { get; set; }
    //    public double LuongCoBan { get; set; }
    //    public double ThuongChuyenCan { get; set; }
    //    public double AnTruaTrangPhuc { get; set; }
    //    public double LuongGop { get; set; } // = LuongCoBan + ThuongChuyenCan + AnTruaTrangPhuc


    //    #region Ngoài giờ
    //    public double NgoaiGioKhongChiuThue { get; set; }
    //    public double NgoaiGioChiuThue { get; set; }

    //    #endregion

    //    #region Công tác phí
    //    public double CongTacPhiChiuThue { get; set; }
    //    public double CongTacPhiKhongChiuThue { get; set; }

    //    #endregion



    //    public double HoTroDuAnSoNgay { get; set; }
    //    public double HoTroDuAnSoTien { get; set; }
    //  //  public double ThuNhapKhacKhac { get; set; }

    //    #region Khoản thu nhập khác
    //    public double ThangLuong13 { get; set; }
    //    public double ThuongNangSuat { get; set; }
    //    public double ThuongTet { get; set; }
    //    public double ThuNhapKhac { get; set; }
    //    public double SoNgayPhepNam { get; set; }
    //    public double ThanhToanPhepNam { get; set; }
    //    public double ThanhToanNghiBu { get; set; }
    //    public double HoanThueTNCN { get; set; }

    //    #endregion


    //    public double LuongThucTe { get; set; }

    //    #region Chi phí công ty
    //    public double ChiPhiCongTyBHXH { get; set; }
    //    public double ChiPhiCongTyBHYT { get; set; }
    //    public double ChiPhiCongTyBHTN { get; set; }
    //    public double ChiPhiCongTyKPCD { get; set; }
    //    #endregion

    //    #region Các khoản trừ vào lương

    //    public double CaNhanBHXH { get; set; }
    //    public double CaNhanBHYT { get; set; }
    //    public double CaNhanBHTN { get; set; }
    //    public double CaNhanPhiCongDoan { get; set; }
    //    public double CaNhanQuyTuNguyen { get; set; }
    //    public double CaNhanThueTNCN { get; set; } // Phần mềm tính ra cho người dùng chỉnh sửa
    //    public double CaNhanThueTNCNPhanMem { get; set; }// Không chỉnh sửa
    //    public double KhauTruTamUng { get; set; }
    //    #endregion


    //    #region Thuế thu nhập cá nhân
    //    //public double ThueTNCNHoanThue { get; set; }
    //    public double ThueTNCNThuNhapChiuThue { get; set; }
    //    public double ThueTNCNTGiamTruCaNhan { get; set; }
    //    public double ThueTNCNTGiamTruPhuThuocSoNguoi { get; set; }
    //    public double ThueTNCNTGiamTruPhuThuocSoGiam { get; set; }
    //    public double ThueTNCNThuNhapTinhThue { get; set; }  // Bằng thu nhập chịu thuế trừ đi các khoản giảm trừ
       

    //    #endregion

    //    public double ThucLinh { get; set; }

    //    public double ThuNhapChiuThueSauGross { get; set; }

    //    public bool isLuongHachToan { get; set; }

    //    public HRM_LuongLaoDongVietNamNhanVien() { }

    //    //public HRM_LuongLaoDongVietNamNhanVien(HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhong ChamCongVanPhongChiTiet, List<HRM_NhanVien> lstNhanVien, string IdLuongLaoDongVietNam)
    //    //{
    //    //    var itemNhanVienHrm = lstNhanVien.FirstOrDefault(x => x.IdNhanVienEOS == ChamCongVanPhongChiTiet.IdNhanVienKeKhai);
    //    //    if (itemNhanVienHrm != null)
    //    //        this.IdNhanVien = itemNhanVienHrm.IdGuid;
    //    //    this.IdNhanVienEOS = ChamCongVanPhongChiTiet.IdNhanVienKeKhai;
    //    //    this.NgayCongCoSo = ChamCongVanPhongChiTiet.NgayCongTinhLuong;
    //    //    this.NgayCongThucTe =  dungchung.LamTronTinhLuong(ChamCongVanPhongChiTiet.TongNgayLamViec);
    //    //    this.IdChamCongVanPhong = ChamCongVanPhongChiTiet.Id;
    //    //    this.IdLuongLaoDongVietNam = IdLuongLaoDongVietNam;
    //    //    //this.IdHopDong = IdHopDong;
    //    //}


    //    //public HRM_LuongLaoDongVietNamNhanVien(HRM.Api.Models.SmartEOEModel.HRM_ChamCongDuAnChiTiet ChamCongDuAnChiTiet, List<HRM_NhanVien> lstNhanVien, string IdLuongLaoDongVietNam)
    //    //{
    //    //    var itemNhanVienHrm = lstNhanVien.FirstOrDefault(x => x.IdNhanVienEOS == ChamCongDuAnChiTiet.IdNhanVien);
    //    //    if (itemNhanVienHrm != null)
    //    //        this.IdNhanVien = itemNhanVienHrm.IdGuid;
    //    //    this.IdNhanVienEOS = ChamCongDuAnChiTiet.IdNhanVien;
    //    //    this.NgayCongCoSo = ChamCongDuAnChiTiet.NgayCongCoSo;
    //    //    this.NgayCongThucTe = ChamCongDuAnChiTiet.NgayCongThucTe;
    //    //    this.IdChamCongDuAnChiTiet = ChamCongDuAnChiTiet.Id;
    //    //    this.IdLuongLaoDongVietNam = IdLuongLaoDongVietNam;
    //    //    //this.IdHopDong = IdHopDong;
    //    //    //this.IdQuyetDinhBoNhiemDieuDong = IdQuyetDinhBoNhiemDieuDong;
    //    //}







    //    [NotMapped]
    //    public List<HRM_LuongLaoDongVietNamNhanVienThuNhapKhac> lstThuNhapKhac { get; set; }

    //    [NotMapped]
    //    public NhanVienRutGon NhanVien { get; set; }

    //    [NotMapped]
    //    public string ChucDanh { get; set; }
    //}


    //public class HRM_LuongLaoDongVietNamNhanVienThuNhapKhac : Audit
    //{
    //    public string IdLuongLaoDongVietNamNhanVien { get; set; }
    //    public string NoiDung { get; set; }
    //    public double SoTien { get; set; }

    //    [NotMapped]
    //    public bool isDeleted { get; set; }
    //}

    //public class HRM_QuyetToanNghiBuNam : Audit
    //{
    //    public int Nam { get; set; }
    //    public float NgayCongCoSo { get; set; }
    //}

    //public class HRM_QuyetToanNghiBuNamNhanVien : Audit
    //{
    //    public string IdNhanVien { get; set; }
    //    public string IdQuyetToanNghiBuNam { get; set; }
    //    public float TongSoGioDuocHuong { get; set; } // lấy từ quy trình làm thêm giờ
    //    public float TongSoGioDaNghiBu { get; set; }// lấy từ quy trình xin nghỉ phé loại nghỉ bù
    //    public float SoGioConLai { get; set; }
    //    public float SoGioQuyetToan { get; set; } // Cho người dùng nhập
    //    public double TongTienTheoLuongThucTe { get; set; }
    //    public double TongTienTheoLuongHachToan { get; set; }
    //    public double LuongHachToan { get; set; }
    //    public double LuongThucTe { get; set; }

    //    [NotMapped]
    //    public int IdTemp { get; set; }
    //    public HRM_QuyetToanNghiBuNamNhanVien() {  }

    //    public HRM_QuyetToanNghiBuNamNhanVien(string IdNhanVien, string IdQuyetToanNghiBuNam)
    //    {
    //        this.IdNhanVien = IdNhanVien;
    //        this.IdQuyetToanNghiBuNam = this.IdQuyetToanNghiBuNam;
    //    }
    //}

    //public class HRM_QuyetToanNghiBuNhanVienThang : Audit
    //{
    //    public string IdQuyetToanNghiBuNam { get; set; }
    //    public string IdQuyetToanNghiBuNhanVien { get; set; }
    //    public int Thang { get; set; }
    //    public int Nam { get; set; }
    //    public float SoGioLamNgoaiGio { get; set; }
    //    public float SoGioNghiBu { get; set; }

    //    [NotMapped]
    //    public int IdTempQuyetToanNghiBuNhanVien { get; set; }

    //    public HRM_QuyetToanNghiBuNhanVienThang() { }

    //    public HRM_QuyetToanNghiBuNhanVienThang(LuongSoGioLamThemTuNgayDenNgay LamThem, string IdQuyetToanNghiBuNam
    //        , int IdTempQuyetToanNghiBuNhanVien, float SoGioDaNghiBu)
    //    {
    //        DateTime NgayLam = LamThem.NgayLam;
    //        this.IdQuyetToanNghiBuNam = IdQuyetToanNghiBuNam;
    //        this.IdQuyetToanNghiBuNhanVien = IdQuyetToanNghiBuNhanVien;
    //        this.Thang = NgayLam.Month;
    //        this.Nam = NgayLam.Year;
    //        this.SoGioLamNgoaiGio = LamThem.SoGioLamThem;
    //        this.SoGioNghiBu = SoGioDaNghiBu;
    //        this.IdTempQuyetToanNghiBuNhanVien = IdTempQuyetToanNghiBuNhanVien;
    //    }


    //    public HRM_QuyetToanNghiBuNhanVienThang(LuongNghiPhepTuNgayDenNgay NghiBu)
    //    {
    //        DateTime NgayLam = NghiBu.NgayNghi;
    //        this.IdQuyetToanNghiBuNam = IdQuyetToanNghiBuNam;
    //        this.IdQuyetToanNghiBuNhanVien = IdQuyetToanNghiBuNhanVien;
    //        this.Thang = NgayLam.Month;
    //        this.Nam = NgayLam.Year;
    //        this.SoGioLamNgoaiGio = 0;
    //        this.SoGioNghiBu = NghiBu.SoGioNghi;
    //        this.IdTempQuyetToanNghiBuNhanVien = IdTempQuyetToanNghiBuNhanVien;
    //    }
    //}



    

    //public class LuongLamNgoaiGioNhanVien
    //{
    //    public long IdNhanVien { get; set; }
    //    public double SoTienChiuThue { get; set; }
    //    public double SoTienMienThue { get; set; }
    //}


    //public class LuongSoGioLamThemTuNgayDenNgay
    //{
    //    public long IdNhanVien { get; set; }
    //    public float SoGioLamThem { get; set; }

    //    public float SoNgayLamThem { get; set; }
    //    public DateTime NgayLam { get; set; }
    //} 


    //public class LuongNghiPhepTuNgayDenNgay
    //{
    //    public long IdNhanVien { get; set; }
    //    public float SoNgay { get; set; }
    //    public DateTime NgayNghi { get; set; }

    //    public float SoGioNghi {
    //        get
    //        {
    //            return SoNgay * DefineData.ThoiGianLamViecMotBuoi * 2;
    //        }
    //    }
    //}

    //public class LuongCongTacPhiNhanVien
    //{
    //    public long IdNhanVien { get; set; }
    //    public double SoTienChiuThue { get; set; }
    //    public double SoTienMienThue { get; set; }

    //}


    //public class LuongHoTroLamDuAnXa
    //{
    //    public long IdNhanVien { get; set; }
    //    public double SoNgay { get; set; }
    //   // public double SoTien { get; set; }
    //    public double TongTienThucTeHoTroDuAn { get; set; }
    //    public double TongTienHachToanHoTroDuAn { get; set; }
    //    public string IdHopDong { get; set; }
    //    public string IdHopDongDuAn { get; set; }
    //    public string IdBoNhiemDieuDongDuAn { get; set; }
    //   //public string IdHopDongDuAn { get; set; }
    //}


    //public class DanhSachLuongTongHopChiTiet
    //{
    //    public string  Ten { get; set; }
    //    public float NgayCongCoSo { get; set; }
    //    public float NgayCongThucTe { get; set; }
    //    public double LuongCoBan { get; set; }
    //    public double LuongThucTe { get; set; }
    //    public double CaNhanThueTNCN { get; set; }
    //    public double KhauTruTamUng { get; set; }
    //    public double ThucLinh { get; set; }

    //    public long IdBoPhan { get; set; }

    //    public int Level { get; set; }


    //    public DanhSachLuongTongHopChiTiet() {  }
    //    public DanhSachLuongTongHopChiTiet(HRM_LuongLaoDongVietNamNhanVien LuongNhanVien, DanhSachLuongTongHopChiTiet itemBoPhan = null, int Level = 0)
    //    {
    //        this.Ten = LuongNhanVien.NhanVien.TenDayDu;
    //        this.NgayCongCoSo = LuongNhanVien.NgayCongCoSo;
    //        this.NgayCongThucTe = LuongNhanVien.NgayCongThucTe;
    //        this.LuongCoBan = LuongNhanVien.LuongCoBan;
    //        this.LuongThucTe = LuongNhanVien.LuongThucTe;
    //        this.CaNhanThueTNCN = LuongNhanVien.CaNhanThueTNCN;
    //        this.KhauTruTamUng = LuongNhanVien.KhauTruTamUng;
    //        this.ThucLinh = LuongNhanVien.ThucLinh;
    //        this.Level = Level;

    //        if(itemBoPhan != null)
    //        {
    //            itemBoPhan.NgayCongCoSo += LuongNhanVien.NgayCongCoSo;
    //           // itemBoPhan.NgayCongThucTe += LuongNhanVien.NgayCongThucTe;
    //            itemBoPhan.LuongCoBan += LuongNhanVien.LuongCoBan;
    //            itemBoPhan.LuongThucTe += LuongNhanVien.LuongThucTe;
    //            itemBoPhan.CaNhanThueTNCN += LuongNhanVien.CaNhanThueTNCN;
    //            itemBoPhan.KhauTruTamUng += LuongNhanVien.KhauTruTamUng;
    //            itemBoPhan.ThucLinh += LuongNhanVien.ThucLinh;
    //        }
    //    }

    //    public DanhSachLuongTongHopChiTiet(string Ten, long IdBoPhan = 0, int Level = 0)
    //    {
    //        this.Ten = Ten;
    //        this.IdBoPhan = IdBoPhan;
    //        this.Level = Level;
    //    }

    //    public static void TinhTongChoBoPhanCha(DanhSachLuongTongHopChiTiet itemBoPhanCha, DanhSachLuongTongHopChiTiet itemBoPhanCon)
    //    {
    //        itemBoPhanCha.NgayCongCoSo += itemBoPhanCon.NgayCongCoSo;
    //        itemBoPhanCha.LuongCoBan += itemBoPhanCon.LuongCoBan;
    //        itemBoPhanCha.LuongThucTe += itemBoPhanCon.LuongThucTe;
    //        itemBoPhanCha.CaNhanThueTNCN += itemBoPhanCon.CaNhanThueTNCN;
    //        itemBoPhanCha.KhauTruTamUng += itemBoPhanCon.KhauTruTamUng;
    //        itemBoPhanCha.ThucLinh += itemBoPhanCon.ThucLinh;
           
    //    }

    //}


    //#region Lương chuyên gia

    ////public class HRM_LuongChuyenGia : Audit
    ////{
    ////    public string IdLuongChuyenGiaTongHop { get; set; }
    ////    public int Thang { get; set; }
    ////    public int Nam { get; set; }
    ////    public double TyGia { get; set; }

    ////    [NotMapped]
    ////    public List<HRM_LuongChuyenGiaChamCongDuAn> lstLuongChuyenGiaChamCongDuAn { get; set; }

    ////    [NotMapped]
    ////    public int SoNhanVien { get; set; }

       
    ////    [NotMapped]
    ////    public List<HRM_LuongChuyenGiaNhanVien> lstLuongChuyenGiaNhanVien { get; set; }
    ////}

    ////public class HRM_LuongChuyenGiaChamCongDuAn : Audit
    ////{
    ////    public string IdLuongChuyenGia { get; set; }
    ////    public long IdChamCongDuAn { get; set; }
    ////}

    ////public class HRM_LuongChuyenGiaNhanVien : Audit
    ////{

    ////    public string IdLuongChuyenGiaTongHop { get; set; }
    ////    public string IdLuongChuyenGia { get; set; }
    ////    public long IdChamCongDuAnChiTiet { get; set; }
    ////    public long IdNhanVienEOS { get; set; }
    ////    public string IdNhanVien { get; set; }
    ////    public string IdHopDongChuyenGia { get; set; }
    ////    public double LuongHopDong { get; set; }

    ////    #region Tính toán
    ////    public double LuongMotNgayUSD { get; set; }
    ////    public double LuongMotNgayVND { get; set; }

    ////    public double CongTacPhiMotNgayUSD { get; set; }
    ////    public double CongTacPhiMotNgayVND { get; set; }

    ////    public double TienKhacMotNgayUSD { get; set; }
    ////    public double TienKhacMotNgayVND { get; set; }
    ////    #endregion
    ////    public double CongTacPhiHopDong { get; set; }
    ////    public float NgayCongCoSo { get; set; }
    ////    public float NgayCongThucTe { get; set; }
        
    ////    public double LuongCoBanUSD { get; set; }
    ////    public double LuongCoBanVND { get; set; }
    ////    public float NgayPhuCap { get; set; }
    ////    public double CongTacPhiUSD { get; set; }
    ////    public double CongTacPhiVND { get; set; }
    ////    public double TienKhacUSD { get; set; }
    ////    public double TienKhacVND { get; set; }
    ////    public double ChiPhiHoanTraUSD { get; set; }
    ////    public double ChiPhiHoanTraVND { get; set; }
    ////    public double TongUSD { get; set; }
    ////    public double TongVND { get; set; }
    ////    public double TienThueNhaVND { get; set; }
    ////    #region Thuế
    ////    public double GiamTruBanThan { get; set; }
    ////    public double GiamTruNguoiPhuThuoc { get; set; }
    ////    public double ThuNhapNetSauGiamTru { get; set; }
    ////    public double ThuNhapGross { get; set; }
    ////    public double ThuNhapChiuThue { get; set; }
    ////    public double SoThuePhaiNop { get; set; }
    ////    public double TongChiPhi { get; set; }

    ////    #endregion

    ////    public bool isDaTraLuong { get; set; }
    ////    public HRM_LuongChuyenGiaNhanVien(){}

    ////    //public HRM_LuongChuyenGiaNhanVien(HRM.Api.Models.SmartEOEModel.HRM_ChamCongDuAnChiTiet ChamCongDuAnChiTiet, List<HRM_NhanVien> lstNhanVien, string IdLuongChuyenGia)
    ////    //{
    ////    //    var itemNhanVienHrm = lstNhanVien.FirstOrDefault(x => x.IdNhanVienEOS == ChamCongDuAnChiTiet.IdNhanVien);
    ////    //    if (itemNhanVienHrm != null)
    ////    //        this.IdNhanVien = itemNhanVienHrm.IdGuid;
    ////    //    this.IdNhanVienEOS = ChamCongDuAnChiTiet.IdNhanVien;
    ////    //    this.NgayCongCoSo = ChamCongDuAnChiTiet.NgayCongCoSo;
    ////    //    this.NgayCongThucTe = ChamCongDuAnChiTiet.NgayCongThucTe;
    ////    //    this.IdChamCongDuAnChiTiet = ChamCongDuAnChiTiet.Id;
    ////    //    this.IdLuongChuyenGia = IdLuongChuyenGia;
    ////    //    this.IdHopDongChuyenGia = ChamCongDuAnChiTiet.IdHopDongChuyenGia;
    ////    //    this.NgayPhuCap = this.NgayCongThucTe;
    ////    //}


    ////    [NotMapped]
    ////    public List<HRM_LuongChuyenGiaNhanVienChiPhiHoanTra> lstChiPhiHoanTra { get; set; }

    ////    [NotMapped]
    ////    public NhanVienRutGon NhanVien { get; set; }

    ////}

    ////public class HRM_LuongChuyenGiaNhanVienChiPhiHoanTra : Audit
    ////{
    ////    public string IdLuongChuyenGiaNhanVien { get; set; }
    ////    public string NoiDung { get; set; }
    ////    public double SoTien { get; set; }

    ////    [NotMapped]
    ////    public bool isDeleted { get; set; }
    ////}


    ////public class HRM_LuongChuyenGiaTongHop : Audit
    ////{
    ////    public int Thang { get; set; }
    ////    public int Nam { get; set; }
    ////    public string TieuDe { get; set; }


    ////    public HRM_LuongChuyenGiaTongHop() { }

    ////    public HRM_LuongChuyenGiaTongHop(HRM_LuongChuyenGia LuongChuyenGia)
    ////    {
    ////        this.Thang = LuongChuyenGia.Thang;
    ////        this.Nam = LuongChuyenGia.Nam;
    ////    }


    ////    [NotMapped]
    ////    public int SoNhanVien { get; set; }

    ////}

    //#endregion

}
