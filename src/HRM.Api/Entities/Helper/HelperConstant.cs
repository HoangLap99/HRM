﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using static HRM.Api.Entity.CodeName;

namespace HRM.Api.Helper
{
    public static class HelperConstant
    {
        public static class HopDongPhuCap
        {
            public static class LoaiPhuCap
            {
                public static StringCodeName THEO_GIO = new StringCodeName("THEO_GIO", "Phụ cấp theo giờ");
                public static StringCodeName THEO_NGAY = new StringCodeName("THEO_NGAY", "Phụ cấp theo ngày");
                public static StringCodeName THEO_TUAN = new StringCodeName("THEO_TUAN", "Phụ cấp theo tuần");
                public static StringCodeName THEO_THANG = new StringCodeName("THEO_THANG","Phụ cấp theo tháng");
                public static StringCodeName THEO_NAM = new StringCodeName("THEO_NAM", "Phụ cấp theo năm");

                public static List<string> ListCode = new List<string>()
                {
                    THEO_GIO.Code,
                    THEO_NGAY.Code,
                    THEO_TUAN.Code,
                    THEO_THANG.Code,
                    THEO_NAM.Code
                };
            }
        }

        public static class DanhMucChucVuCongViec
        {
            public static StringCodeName KHAC = new StringCodeName("KHAC", "Danh mục chức vụ công việc khác");
        }

        public static class LuongHopDong
        {
            public static class LoaiLuong
            {
                public static StringCodeName LUONG_HOACH_TOAN = new StringCodeName("LUONG_HOACH_TOAN", "Lương hoạch toán");
                public static StringCodeName LUONG_THUC_LINH = new StringCodeName("LUONG_THUC_LINH", "Lương thực lĩnh");
            }
            public static class HinhThucLuong
            {
                public static StringCodeName LUONG_GROSS = new StringCodeName("LUONG_GROSS", "Lương gross");
                public static StringCodeName LUONG_NET = new StringCodeName("LUONG_NET", "Lương net");
            }
            public static class HinhThucTraLuong
            {
                public static StringCodeName TIEN_MAT = new StringCodeName("TIEN_MAT", "Bằng tiền mặt");
                public static StringCodeName CHUYEN_KHOAN = new StringCodeName("CHUYEN_KHOAN", "Chuyển khoản");

                public static List<string> ListCode = new List<string>
                {
                    TIEN_MAT.Code,
                    CHUYEN_KHOAN.Code
                };
            }
        }

        public static class ddNhanVien
        {
            public static class TinhTrangLamViec
            {
                public static StringCodeName DANG_LAM = new StringCodeName("DangLamViec", "Đang làm việc");
                public static StringCodeName DA_NGHI_VIEC = new StringCodeName("DaNghi", "Đã nghỉ việc");
                public static StringCodeName NGHI_KHONG_LUONG = new StringCodeName("NghiKhongLuong", "Nghỉ không lương");
                public static StringCodeName NGHI_THAI_SAN = new StringCodeName("NghiThaiSan", "Nghỉ thai sản");
            }

            public static class GioiTinh
            {
                public static StringCodeName NAM = new StringCodeName("Nam", "Nam");
                public static StringCodeName NU = new StringCodeName("Nu", "Nữ");
                public static StringCodeName KHONG_XAC_DINH = new StringCodeName("KhongXacDinh", "Không xác định");
            }
        }

       
    }
}
