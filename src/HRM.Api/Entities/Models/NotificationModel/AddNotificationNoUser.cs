﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Models.NotificationModel
{
    public class AddNotificationNoUserModel
    {
        public List<string> listNguoiNhan { set; get; }
        public NotificationNoUserModel Item { set; get; }
        public string NguoiGui { set; get; }
    }
    public class NotificationNoUserModel
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string Link { get; set; }
        public string LoaiThongBao { get; set; }
        public string IdQuyTrinh { get; set; }//id của quy trình
        public long idItemLienQuan { get; set; }//id của item trực tiếp (item trao đổi ...)
        public string DuLieuLienQuan { get; set; }//{idTasks:9,TenCongViec:"abc"}
    }
}
