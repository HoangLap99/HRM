﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Models.ChamCongModel
{
    public class ChiTietChamCongNhanVienViewModel
    {
        public string TenNhanVien { get; set; }
        public string MaNhanVien { set; get; }
        public string IdNhanVien { set; get; }
        public DateTime NgayChamCong { set; get; }
        public string ChucVu { set; get; }
        public string GioVao { set; get; }
        public string GioRa { set; get; }
        public double TongGio { set; get; }
    }
    public class NgayChamCong
    {
        public int Ngay { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public DateTime NgayCong { get { return new DateTime(Nam, Thang, Ngay); } set { } }
        public string Thu 
        { get {
                var mapDayOfWeek = new Dictionary<DayOfWeek, string>();

                mapDayOfWeek.Add(DayOfWeek.Monday, "Thứ 2");
                mapDayOfWeek.Add(DayOfWeek.Tuesday, "Thứ 3");
                mapDayOfWeek.Add(DayOfWeek.Wednesday, "Thứ 4");
                mapDayOfWeek.Add(DayOfWeek.Thursday, "Thứ 5");
                mapDayOfWeek.Add(DayOfWeek.Friday, "Thứ 6");
                mapDayOfWeek.Add(DayOfWeek.Saturday, "Thứ 7");
                mapDayOfWeek.Add(DayOfWeek.Sunday, "Chủ nhật");
                
                return mapDayOfWeek[NgayCong.DayOfWeek];
            } }//Thứ trong tuần
        public string ThuVietTat
        { get {
                var mapDayOfWeek = new Dictionary<DayOfWeek, string>();

                mapDayOfWeek.Add(DayOfWeek.Monday, "T2");
                        mapDayOfWeek.Add(DayOfWeek.Tuesday, "T3");
                        mapDayOfWeek.Add(DayOfWeek.Wednesday, "T4");
                        mapDayOfWeek.Add(DayOfWeek.Thursday, "T5");
                        mapDayOfWeek.Add(DayOfWeek.Friday, "T6");
                        mapDayOfWeek.Add(DayOfWeek.Saturday, "T7");
                        mapDayOfWeek.Add(DayOfWeek.Sunday, "CN");
                
                        return mapDayOfWeek[NgayCong.DayOfWeek];
                    }
        }//Thứ viết tắt
        public NgayChamCong(DateTime input)
        {
            this.Ngay = input.Day;
            this.Thang = input.Month;
            this.Nam = input.Year;
        }
        public bool isNghiLe { get; set; }
        public bool isNghi { get; set; }
        //public bool isNghiBuNgayLe { get; set; }
    }
}
