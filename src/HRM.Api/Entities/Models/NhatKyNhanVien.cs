﻿#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.Api.Models
{
    public class HRM_NhatKyNhanVien : Audit
    {
        [StringLength(36)]
        public string IdNhanVien { set; get; }
        [NotMapped]
        public string TenNhanVien { set; get; }
        [StringLength(36)]
        public string IdTable { set; get; }
        [MaxLength(100)]
        public string TableName { set; get; }
        [StringLength(100)]
        public string Link { set; get; }
        public int Loai { set; get; }
        [MaxLength(250)]
        public string QuyTrinh { set; get; }
        [MaxLength(250)]
        public string eAction { set; get; }
        public DateTime ThoiGian { set; get; }
        [NotMapped]
        public double ThoiGianUnix { set; get; }
           [NotMapped]
        public string MaNhanVien { set; get; }
        

        [NotMapped]
        public List<HRM_FileDinhKem> listFileDinhKem { get; set; }
    }
}
