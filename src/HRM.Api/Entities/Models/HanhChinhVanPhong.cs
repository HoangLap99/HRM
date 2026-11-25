﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace HRM.Api.Models
{
    public class HRM_QuyTrinhQuyetDinhKhenThuong : Audit
    {
        [MaxLength(50)]
        public string SoQuyTrinh { get; set; }
        public long IdCongTyEOS { set; get; }
        public string GhiChu { set; get; }
        public DateTime? NgayKy { get; set; }
        public DateTime? NgayHieuLuc { get; set; }
        public string NoiDung { set; get; }
        [MaxLength(36)]
        public string IdTrangThai { get; set; }
        [MaxLength(500)]
        public string TenTrangThai { get; set; }
        public bool isKetThuc { set; get; } = false;
        public string IdNguoiKy { get; set; }
        public long IddmChucVuNguoiKy { set; get; }    
        public string IddmHinhThucKhenThuong { set; get; }
        public string VeViec { set; get; }
        public DateTime? ThoiGian { get; set; }
        [NotMapped]
        public string TenChucVuNguoiKy { set; get; }
        [NotMapped]
        public string TenBoPhan { get; set; }
        [NotMapped]
        public string TenCongty { get; set; }
        [NotMapped]
        public double NgayUnix { get; set; }
        [NotMapped]
        public List<HRM_QuyTrinhQuyetDinhKhenThuong_ChiTiet> listChiTiet { get; set; }
        [NotMapped]
        public List<HRM_FileDinhKem> lstFileDinhKem { get; set; }
    }

    public class HRM_QuyTrinhQuyetDinhKhenThuong_ChiTiet : Audit
    {
        public string IdQuyetDinhKhenThuong { set; get; }
        public long IdCongTyEOS { set; get; }    
        public DateTime? NgayKy { get; set; }
        public DateTime? NgayHieuLuc { get; set; }
        public string IdNhanVien { set; get; }
        public long IdBoPhanEOS { get; set; }
        public long SoTienThuong { set; get; }
        public string GhiChu { set; get; }
        [NotMapped]
        public string TenChucVu { set; get; }
        [NotMapped]
        public double NgayDangKiUnix { get; set; }
        [NotMapped]
        public string TenBoPhan { get; set; }

    }
}
