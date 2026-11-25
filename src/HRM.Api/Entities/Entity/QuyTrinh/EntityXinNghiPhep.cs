﻿#nullable disable
using HRM.Api.Models;
using System;

namespace HRM.Api.Entity
{
    public class EntityQT_XinNghiPhep_ChiTietNgayNghi : AuditId
    {
        public string IdQuyTrinh { get; set; }
        public string IdHinhThucNghi { get; set; }//
        public System.DateTime NgayBatDau { get; set; }
        public System.DateTime NgayKetThuc { get; set; }
        public string GhiChu { get; set; }
        public string IdLoaiNghiPhep { get; set; }//QT_DanhMucLoaiNghiPhep: SANG,CHIEU,CANGAY
        public double ThoiGianNghi { get; set; }
        public string IdNguoiDeNghi { get; set; }
        public string MaNghiPhep { get; set; }//IdHinhThucNghi
        public EntityQT_XinNghiPhep_ChiTietNgayNghi() 
        {
        }
        //public EntityQT_XinNghiPhep_ChiTietNgayNghi(HRM.Api.Models.SmartEOEModel.QT_XinNghiPhep_ChiTietNgayNghi input)
        //{
        //    this.Id             = input.Id             ;
        //    this.IdQuyTrinh     = input.IdQuyTrinh     ;
        //    this.IdHinhThucNghi = input.IdHinhThucNghi ;
        //    this.NgayBatDau     = input.NgayBatDau     ;
        //    this.NgayKetThuc    = input.NgayKetThuc    ;
        //    this.Created        = input.Created        ;
        //    this.CreatedBy      = input.CreatedBy      ;
        //    this.Modified       = input.Modified       ;
        //    this.ModifiedBy     = input.ModifiedBy     ;
        //    this.GhiChu         = input.GhiChu         ;
        //    this.IdLoaiNghiPhep = input.IdLoaiNghiPhep ;
        //    this.ThoiGianNghi   = input.ThoiGianNghi   ;
        //}
    }
    public partial class EntityQT_QuyTrinhXinNghiPhep
    {
        public string Id { get; set; }
        public long IdBoPhan { get; set; }
        public string HinhThucNghi { get; set; }
        public string TenNguoiDeNghi { get; set; }
        public string IdNguoiDeNghi { get; set; }
        public string IdTrangThai { get; set; }
        public string TenTrangThai { get; set; }
        public string MucDich { get; set; }
        public Nullable<bool> isDiMuon { get; set; }
        public System.DateTime ThoiGianBatDau { get; set; }
        public System.DateTime ThoiGianKetThuc { get; set; }
        public string GhiChu { get; set; }
        public System.DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }
        public bool NghiSang { get; set; }
        public bool NghiChieu { get; set; }
        public double ThoiGianTong { get; set; }
        public string IdQuyTrinhDieuChinh { get; set; }
        public bool isDieuChinh { get; set; }
        public System.DateTime ThangNam { get; set; }
        public string SoPhieu { get; set; }
        public bool Loai { get; set; }
        public EntityQT_QuyTrinhXinNghiPhep()
        {
        }
        public EntityQT_QuyTrinhXinNghiPhep(HRM.Api.Models.SmartEOEModel.QT_QuyTrinhXinNghiPhep input)
        {
            this.Id                  = input.Id                  ;
            this.IdBoPhan            = input.IdBoPhan            ;
            this.HinhThucNghi        = input.HinhThucNghi        ;
            this.TenNguoiDeNghi      = input.TenNguoiDeNghi      ;
            this.IdNguoiDeNghi       = input.IdNguoiDeNghi       ;
            this.IdTrangThai         = input.IdTrangThai         ;
            this.TenTrangThai        = input.TenTrangThai        ;
            this.MucDich             = input.MucDich             ;
            this.isDiMuon            = input.isDiMuon            ;
            this.ThoiGianBatDau      = input.ThoiGianBatDau      ;
            this.ThoiGianKetThuc     = input.ThoiGianKetThuc     ;
            this.GhiChu              = input.GhiChu              ;
            this.Created             = input.Created             ;
            this.CreatedBy           = input.CreatedBy           ;
            this.Modified            = input.Modified            ;
            this.ModifiedBy          = input.ModifiedBy          ;
            this.NghiSang            = input.NghiSang            ;
            this.NghiChieu           = input.NghiChieu           ;
            this.ThoiGianTong        = input.ThoiGianTong        ;
            this.IdQuyTrinhDieuChinh = input.IdQuyTrinhDieuChinh ;
            this.isDieuChinh         = input.isDieuChinh         ;
            this.ThangNam            = input.ThangNam            ;
            this.SoPhieu             = input.SoPhieu             ;
            this.Loai                = input.Loai                ;
        }
    }
}
