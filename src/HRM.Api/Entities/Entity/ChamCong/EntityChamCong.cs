﻿#nullable disable
using HRM.Api.Models;
using HRM.Api.Models.ChamCongModel;
using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Entity
{
    public class EntityGetDanhSachChamCongDetailRequest : EntityRequest
    {
        public long IdCongTyEOS { get; set; }
        public string IdDotChamCong { get; set; }
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public int TabTrangThai { get; set; }//1: Chưa xử lý; 2: Đã xử lý
    }
    public class EntityHRM_DanhMuc_NhanVien_ChamCongBoPhan
    {
        public string IdUserNhanVien { get; set; }
        public long IdCongTyEOS { get; set; }
        public string TenNhanVien { get; set; }
        public List<long> lstIdBoPhanEOS { get; set; }
    }
    public class EntityHRM_ChamCong_TongHop
    {
        public string IdDanhMucDotChamCongThang { get; set; }//HRM_dmDotChamCongThang
        public int Thang { get; set; }
        public int Nam { get; set; }
        //public double NgayCongCoSo { get; set; }
        public long IdCongTyEOS { get; set; }
        public List<NgayChamCong> lstNgayChamCongTrongKy { get; set; }
        public List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongNhanVien { get; set; }
        public void GetDanhSachNgayCongTrongThang(List<HRM_dmDotChamCongThang> lstDotChamCong)
        {
            //lstNgayChamCongTrongKy = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong, lstCaiDatNghiLe);
        }
    }
    public class EntityHRM_ChamCong_NhanVien_BoPhan
    {
      
        public string IdChamCongDetail { get; set; }//HRM_ChamCong_Detail
        public string IdChamCongBoPhan { get; set; }//HRM_ChamCong_BoPhan
        public string IdNhanVien { get; set; }
        public long IdBoPhanEOS { get; set; }
        
        public string TenPhongBan { set; get; }
        public string idChamCongCongTy { get; set; }
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan> lstChamCongThoiGian { get; set; }
    }
    public class EntityHRM_ChamCong_LayTuSQL
    {
        public string MaNhanVien { get; set; }
        public DateTime NgayCong { get; set; }
        public DateTime NgayCong_GioChamCong { get; set; }
    }
    public class EntityHRM_ChamCong_Detail
    {
        public string IdGuid { get; set; }
        public string IdDanhMucDotChamCongThang { get; set; }//HRM_dmDotChamCongThang
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string IdTrangThai { get; set; }
        public string TenTrangThai { get; set; }
        public string idChamCongCongTy { get; set; }
        public string SoPhieu { get; set; }
        public double NgayCongCoSo { get; set; }
        public long IdCongTyEOS { get; set; }
        public DateTime Created { get; set; }
  
        public string CreatedBy { get; set; }//ID User

        public string CreatedByName { get; set; }
        public DateTime Modified { get; set; }
   
        public string ModifiedBy { get; set; }//ID User

        public string ModifiedByName { get; set; }
        public EntityHRM_ChamCong_Detail()
        {

        }
        public EntityHRM_ChamCong_Detail(HRM_ChamCong_Detail input)
        {
            this.IdGuid = input.IdGuid;
            this.IdDanhMucDotChamCongThang = input.IdDanhMucDotChamCongThang;
            this.Thang = input.Thang;
            this.Nam = input.Nam;
            this.IdTrangThai = input.IdTrangThai;
            this.TenTrangThai = input.TenTrangThai;
            this.idChamCongCongTy = input.idChamCongCongTy;
            this.NgayCongCoSo = input.NgayCongCoSo;
            this.IdCongTyEOS = input.IdCongTyEOS;
            this.SoPhieu = input.SoPhieu;
            this.Created= input.Created;

            this.CreatedByName = input.CreatedByName;
            this.CreatedBy = input.CreatedBy;
        }
        public List<NgayChamCong> lstNgayChamCongTrongKy { get; set; }
        public List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongBoPhan { get; set; }
        public List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongNhanVien { get; set; }
        public void GetDanhSachNgayCongTrongThang(List<HRM_dmDotChamCongThang> lstDotChamCong, HRM_dmLichLamViec LichLamViec, HRM_dmLichNghiLe LichNghiLe)
        {
            lstNgayChamCongTrongKy = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong, LichLamViec, LichNghiLe);
        }
        public void GetDanhSachNgayCongTrongThang(List<HRM_dmDotChamCongThang> lstDotChamCong, List<CaiDatHeThong> lstCaiDatNghiLe)
        {
            lstNgayChamCongTrongKy = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong, lstCaiDatNghiLe);
        }
    }
    public class EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan
    {
        public string IdGuid { get; set; }
        public long IdCongTyEOS { get; set; }//BoPhan
        public long IdBoPhanEOS { get; set; }//BoPhan
        //[JsonIgnore]
        public string IdChamCongDetail { get; set; }//HRM_ChamCong_Detail
        //[JsonIgnore]
        public string IdChamCongBoPhan { get; set; }//HRM_ChamCong_BoPhan
        public string IdNhanVien { get; set; }
        //[JsonIgnore]
        public string IdChamCongNhanVienBoPhan { get; set; }//HRM_ChamCong_NhanVien_BoPhan
        //[JsonIgnore]
        public string idChamCongCongTy { get; set; }
        public int Ngay { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public DateTime NgayCong { get; set; }
        public string Thu { get; set; }//Thứ trong tuần
        public string ThuVietTat { get; set; }//Thứ viết tắt
        [MaxLength(36)]
        public string IddmChamCong { get; set; }
        public bool isLuongBaoHiem { get; set; }
        public bool isLuongSanPham { get; set; }
        public double SoGioLamViec { get; set; }
        public double SoGioKhac { get; set; }

        public bool isNghiLe { get; set; }
        public bool isNghiPhep { get; set; }
        public bool isNghiKhongPhep { get; set; }

        public bool isHoTroCongTacXa { get; set; }
        public double SoGioSang { get; set; }
        public double SoGioChieu { get; set; }
        public bool isCheckCa1 { get; set; }//Sangs
        public bool isCheckCa2 { get; set; }//Chieu
        public string MaChamCongSang { get; set; }//eMaNgayChamCong
        public string MaChamCongChieu { get; set; }//eMaNgayChamCong

        public bool isDisable { get; set; }//Hết hiệu lực, điều động, view sẽ không hiển thị
        public string idHopDong { get; set; }//Để tính lương
        public string idQuyetDinhDieuChuyen { get; set; }//Nếu có quyết định sẽ ưu tiên quyết định
    
        public string LyDoDieuChinh { get; set; }//
        public EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan()
        {

        }
        public EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(NgayChamCong input)
        {
            Ngay = input.Ngay;
            Thang = input.Thang;
            Nam = input.Nam;
            //MaChamCongSang = "";
            //MaChamCongChieu = "";
            isDisable = true;
        }
        public EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(HRM_ChamCong_NhanVien_ThoiGian_BoPhan input)
        {
            this.IdGuid = input.IdGuid;
            this.IdBoPhanEOS = input.IdBoPhanEOS;
            this.IdCongTyEOS = input.IdCongTyEOS;
            this.IdChamCongDetail = input.IdChamCongDetail;
            this.IdChamCongBoPhan = input.IdChamCongBoPhan;
            this.IdNhanVien = input.IdNhanVien;
            this.IdChamCongNhanVienBoPhan = input.IdChamCongNhanVienBoPhan;
            this.idChamCongCongTy = input.idChamCongCongTy;
            this.IddmChamCong = input.IddmChamCong;
            this.Ngay = input.Ngay;
            this.Thang = input.Thang;
            this.Nam = input.Nam;
            this.NgayCong = input.NgayCong;
            this.ThuVietTat = input.ThuVietTat;
            this.isLuongBaoHiem = input.isLuongBaoHiem;
            this.isLuongSanPham = input.isLuongSanPham;
            this.IddmChamCong = input.IddmChamCong;
            this.SoGioLamViec = input.SoGioLamViec;
            //this.isNghiLe = input.isNghiLe;
            //this.isNghiPhep = input.isNghiPhep;
            //this.isNghiKhongPhep = input.isNghiKhongPhep;
            //this.isHoTroCongTacXa = input.isHoTroCongTacXa;
            //this.SoGioSang = input.SoGioSang;
            //this.SoGioChieu = input.SoGioChieu;
            //this.isCheckCa1 = input.isCheckCa1;
            //this.isCheckCa2 = input.isCheckCa2;
            //this.Thu = input.Thu;

            this.isDisable = input.isDisable;
            this.idHopDong = input.idHopDong;
            this.idQuyetDinhDieuChuyen = input.idQuyetDinhDieuChuyen;
            //this.MaChamCongSang = input.MaChamCongSang;
            //this.MaChamCongChieu = input.MaChamCongChieu;
        }
        public HRM_ChamCong_NhanVien_ThoiGian_BoPhan CopyToHRM(HRM_ChamCong_NhanVien_ThoiGian_BoPhan input)
        {
            input.IdGuid = this.IdGuid;
            //this.IdChamCongDetail         = this.IdChamCongDetail         ;
            //this.IdChamCongBoPhan         = this.IdChamCongBoPhan         ;
            //this.IdNhanVien               = this.IdNhanVien               ;
            //this.IdChamCongNhanVienBoPhan = this.IdChamCongNhanVienBoPhan ;
            //this.idChamCongCongTy         = this.idChamCongCongTy         ;
            //this.Ngay                     = this.Ngay                     ;
            //this.Thang                    = this.Thang                    ;
            //this.Nam                      = this.Nam                      ;
            //this.NgayCong                 = this.NgayCong                 ;
            //this.Thu                      = this.Thu                      ;
            //this.ThuVietTat               = this.ThuVietTat               ;
            //input.isNghiLe = this.isNghiLe;
            //input.isNghiPhep = this.isNghiPhep;
            //input.isNghiKhongPhep = this.isNghiKhongPhep;
            //input.isHoTroCongTacXa = this.isHoTroCongTacXa;
            //input.SoGioSang = this.SoGioSang;
            //input.SoGioChieu = this.SoGioChieu;
            //input.isCheckCa1 = this.isCheckCa1;
            //input.isCheckCa2 = this.isCheckCa2;
            input.isDisable = this.isDisable;
            input.idHopDong = this.idHopDong;
            input.idQuyetDinhDieuChuyen = this.idQuyetDinhDieuChuyen;
            //input.MaChamCongSang = this.MaChamCongSang;
            //input.MaChamCongChieu = this.MaChamCongChieu;

            this.isLuongBaoHiem = input.isLuongBaoHiem;
            this.isLuongSanPham = input.isLuongSanPham;
            this.IddmChamCong = input.IddmChamCong;
            this.SoGioLamViec = input.SoGioLamViec;
            return input;
        }
        public bool Equals(EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan other)
        {
            return other != null && Nam == other.Nam && Thang == other.Thang && Ngay == other.Ngay && IdNhanVien == other.IdNhanVien && IdCongTyEOS == other.IdCongTyEOS;
        }
        public bool CheckExist(NgayChamCong other)
        {
            return other != null && Nam == other.Nam && Thang == other.Thang && Ngay == other.Ngay;
        }
    }


    #region Chấm công
    public class HRM_ChamCong : Audit
    {
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public DateTime Ngay { get; set; }
        public float SoGioTinhLuong { get; set; }
        [NotMapped]
        public List<HRM_ChamCong_ChiTiet> listChiTiet { get; set; } = new List<HRM_ChamCong_ChiTiet>();
    }

    public class HRM_ChamCong_ChiTiet : Audit
    {
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdChamCong { get; set; }
        [MaxLength(36)]
        public string IddmKyHieuChamCong { get; set; }
        public int Gio { get; set; }
        public int Phut { get; set; }
        public DateTime NgayCong { get; set; }
    }

    public class HRM_ChamCong_TongHop : Audit
    {
        public long IdBoPhan { get; set; }
        [MaxLength(36)]
        public string IdNhanVien { get; set; }
        public DateTime Ngay { get; set; }
        public float SoGioThucTe { get; set; }
        public float SoGioLamThem { get; set; }
        public float GioDiMuon { get; set; }
        public float GioVeSom { get; set; }
        public int SoLanDiMuonKhongPhep { get; set; }
        public int SoLanDiMuonCoPhep { get; set; }
        public int SoLanVeSomKhongPhep { get; set; }
        public int SoLanVeSomCoPhep { get; set; }
    }
    #endregion
}
