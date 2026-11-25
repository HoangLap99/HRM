﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.Api.Models
{

    public partial class HRM_QuyTrinhDinhBien : Audit
    {
        [MaxLength(50)]
        public string SoQuyTrinh { get; set; }
        public long IdCoSoEOS { get; set; }
        public long IdCongTyEOS { get; set; }

        // public long IdCoSoEOS { get; set; }
        [StringLength(36)]
        public string IdNguoiKy { set; get; }
        public long IdChucDanhNguoiKy { set; get; }
        [MaxLength(2500)]
        public string NoiDung { get; set; }
        [MaxLength(2500)]
        public string GhiChu { get; set; }
        public double TongDinhBienCu { set; get; }
        public double TongDinhBienMoi { set; get; }
        public int Nam { set; get; }
        public int SoLuong { set; get; }
        public double QuyLuong { set; get; }
        [StringLength(36)]
        public string IdTrangThai { set; get; }
        [MaxLength(250)]
        public string TenTrangThai { get; set; }
        [StringLength(36)]
        public string IdQuyTrinh { set; get; }
        public DateTime NgayApDung { get; set; }
        public bool isKetThuc { get; set; } = false;
        [NotMapped]
        public double NgayApDungUnix { get; set; }

        [NotMapped]
        public List<HRM_QuyTrinhDinhBien_ChiTiet> listQuyTrinhDinhBienChiTiet = new List<HRM_QuyTrinhDinhBien_ChiTiet>();
    }
    public partial class HRM_QuyTrinhDinhBien_ChiTiet : Audit
    {
        [StringLength(36)]
        public string IdQuyTrinh { get; set; }

        public long IdChucDanhEOS { get; set; }
        public long IdCoSoEOS { get; set; }
        public int SoLuong { get; set; }
        public double LuongDinhBien { get; set; }
        public int SoLuongMoi { get; set; }
        public double LuongDinhBienMoi { get; set; }
        //[StringLength(36)]
        //public string IdTrinhDoDaoTao { get; set; }
        public int Nam { get; set; }
        [NotMapped]
        public bool isDeleted { get; set; }
    }
    public class HRM_DinhBien : Audit
    {
        public string IdQuyTrinh { get; set; }
        public long IdCoSoEOS { get; set; }
        [NotMapped]
        public string TenCoSo { set; get; }
        public long IdCongTyEOS { set; get; }
        public long IdChucDanhEOS { get; set; }
        public long IdBoPhanEOS { set; get; }
        public int SoLuong { get; set; }

        public int SoLuongDaCo { set; get; }
        public double LuongDinhBien { get; set; }

        public int Nam { get; set; }

        public HRM_DinhBien()
        {

        }

        public HRM_DinhBien(HRM_QuyTrinhDinhBien model, HRM_QuyTrinhDinhBien_ChiTiet item, int _SoLuongDaCo)
        {
            IdQuyTrinh = model.IdGuid;
            IdGuid = Guid.NewGuid().ToString();
            Nam = model.Nam;
            IdCongTyEOS = model.IdCongTyEOS;
            IdChucDanhEOS = item.IdChucDanhEOS;
            IdCoSoEOS = model.IdCoSoEOS;
            SoLuong = item.SoLuongMoi;
            SoLuongDaCo = _SoLuongDaCo;
            LuongDinhBien = item.LuongDinhBienMoi;
        }
    }
}



