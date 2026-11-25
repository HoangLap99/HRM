#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HRM.Api.Services.SmartEOS;

namespace HRM.Api.Services.BangLuong
{
    public class ThanhToanLuongViewModel
    {
        public string IdNhanVien { set; get; }
        public string TenNhanVien { set; get; }
        public string MaNhanVien { set; get; }
        public long IdChuVu { set; get; }
        public string TenChucVu { set; get; }
        public double SoNgayCong { set; get; }
        public double SoNgayCongKy1 { set; get; }
        public double SoNgayCongKy2 { set; get; }
        public double LuongChinh { set; get; }
        public double TongLuongPhuCap { set; get; }
        public double KiemNhiem { set; get; }
        public double LamThemGio { set; get; }
        public double TangCaNgayThuong { set; get; }
        public double TangCaNgayLe { set; get; }
        public double TangCaNgayCN { set; get; }
        public double TangCaNgayT7 { set; get; }
        public double TroCapNgoai { set; get; }
        public double BHXH_NSD { set; get; }
        public double CPCD_NSD { set; get; }
        public double BHXH_NLD { set; get; }
        public double CPCD_NLD { set; get; }
        public double ThucNhanKy1 { set; get; }
        public double ThucNhanKy2 { set; get; }
        public double ThucNhanKy { set; get; }
        public List<PhuCapViewModel> listPhuCap { set; get; }
    }
    public class PhuCapViewModel
    {

        public string TenLoaiPhuCap { set; get; }
        public double SoTien { set; get; }

        public int LoaiPhuCap { set; get; }
        public int Loai { set; get; }
        public string IddmLoaiPhuCap { set; get; }
    }
    public class ChamCongNhanVienViewModel
    {
        public string IdNhanVien { set; get; }
        public double SoGioLamViec { set; get; }
        public double SoNgayDiLam { set; get; }
        public double SoNgayNghiPhep { set; get; }
    }

        public class FuncBangLuong : FuncBase
    {
        public FuncBangLuong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBangLuong> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        public dynamic GetNextMaBangLuong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_BangLuong.Select(x => x.SoPhieu), DefineData.MaPhieuBangLuong));
        public dynamic GetDanhSachBangLuong(TimKiem TimKiem, ApplicationDbContext db)
        {
            int CurrentPage = TimKiem.CurrentPage ?? 1;
            //   int PageSize = TimKiem.PageSize ?? 25;
            int TabTrangThai = TimKiem.TabTrangThai ?? 1;
            IQueryable<HRM_BangLuong> query = db.HRM_BangLuong;
            if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                query = query.Where(dm => dm.SoPhieu.Contains(TimKiem.KeyWord));
            if (TimKiem.IdCongTyEOS > 0)
                query = query.Where(dm => dm.IdCongTyEOS == TimKiem.IdCongTyEOS);
            if (TimKiem.Thang > 0)
                query = query.Where(dm => dm.ThoiGian.Month == TimKiem.Thang);
            if (TimKiem.Nam > 0)
                query = query.Where(dm => dm.ThoiGian.Year == TimKiem.Nam);

            List<HRM_BangLuong> listItem = GetPagedWorkflowList(query, db, TabTrangThai, CurrentPage, nPageSize, out PagingInfor paging);
            if (TabTrangThai == -1)
                return paging.TotalItem;
            //List<HRM.Api.Models.SmartEOEModel.BoPhan> listBoPhan = smdb.BoPhans.ToList();
            foreach (var item in listItem)
            {
                item.NgayTaoUnix = DateTimeToUnixTimeStamp(item.NgayTao);
                item.ThoiGianUnix = DateTimeToUnixTimeStamp(item.ThoiGian);

            }
            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = listItem;
            return dnm;

        }

        public dynamic GetQuyTrinhBangLuong(string Id, ApplicationDbContext db)
        {
            var model = db.HRM_BangLuong.Find(Id);
            if (model == null)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy bản ghi yêu cầu xóa");
            }
            model.ThoiGianUnix = dungchung.DateTimeToUnixTimeStamp(model.ThoiGian);
            model.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTao);
            var chitietLuongOld = db.HRM_BangLuong_ChiTietNhanVien.Where(r => r.IdBangLuong == Id).OrderBy(r => r.MaNhanVien).ToList();
            model.listLuongChiTiet = chitietLuongOld;
            var chitietBangChamCongOld = db.HRM_Link_BangLuong_ChamCong.Where(r => r.IdBangLuong == Id).ToList();
            model.listBangChamCong = chitietBangChamCongOld;
            model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_BangLuong.ToString(), model.IdGuid);
            return model;
        }

    }

    public class NhanVienLuongViewModel
    {
        public string IdNhanVien { set; get; }
        public long IdBoPhanEOS { set; get; }
        public double PhuCapLamNgoaiGio { set; get; }
        public string MaNhanVien { set; get; }
        public string TenNhanVien { set; get; }
        public double ThoiGianLamViec { set; get; }
        public double NgayCongCoSo { set; get; }
        public double MucLuong { set; get; }
        public double SoNgayCongThucTe { set; get; }
        public double SoNgayLamViecDuThoiGian { set; get; }
        public double SoGioLamThucTe { set; get; }
        public double TangCa { set; get; }
        public double TangCaNgayThuong { set; get; }
        public double TangCaNgayLe { set; get; }
        public double TangCaNgayCN { set; get; }
        public double TangCaNgayT7 { set; get; }
        public double NghiLe { set; get; }
        public double NghiPhep { set; get; }
        public double NghiBu { set; get; }
        public double DiCongTac { set; get; }
        public double LuongNghiPhep { set; get; }
        public double LuongTheoSoNgayCong { set; get; }
        public double LuongTangCa { set; get; }
        public double LuongNghiBu { set; get; }
        public double LuongNgayLe { set; get; }

        public double PhuCapTheoCongChinh { set; get; }
        public double PhuCapKhongTheoCongChinh { set; get; }
        public double TroCapNgoai { set; get; }
        public double PhuCapTinhThue { set; get; }
        public double PhuCapKhongTinhThue { set; get; }
        public double PhuCapLamThem { set; get; }
        public double TongCong { set; get; }
        public double KhauTruTaiNguon { set; get; }//10% khấu trừ với lao động thử việc cộng tác viên
        public double TongGiamTruGiaCanh { set; get; }
        public double TongGiamTruTNNCN { set; get; }
        public double TongMucTNCN_NLD { set; get; }
        public double TongMucTNCN_NSD { set; get; }
        public double NSD_BHXH { set; get; }
        public double NSD_BHYT { set; get; }
        public double NSD_BHTN { set; get; }
        public double NSD_TongBH { set; get; }
        public double NSD_PhiCongDoan { set; get; }
        public double Tong_NSD { set; get; }


        public double NLD_BHXH { set; get; }
        public double NLD_BHYT { set; get; }
        public double NLD_BHTN { set; get; }
        public double NLD_TongBH { set; get; }
        public double NLD_PhiCongDoan { set; get; }
        public double Tong_NLD { set; get; }
        public int SoNguoiDuocGiamTru { set; get; }
        public double TongNhanChuaThueChuaGiamTru { set; get; }
        public double TongNhanChuaThueDaGiamTru { set; get; }
        public double TongNhan_NLD { set; get; }
        public int KieuLuong { set; get; }
        public List<PhuCapNhanVienViewModel> listPhuCap { set; get; }
        public double KhenThuong { set; get; }
        public double KyLuat { set; get; }
    }
    public class NgayChamCongNhanVienModel
    {
        public string IdNhanVien { set; get; }
        public DateTime NgayBatDau { set; get; }
        public DateTime NgayKetThuc { set; get; }
    }

    public class PhuCapNhanVienViewModel
    {
        public string IddmPhuLuc { set; get; }
        public string Ten { set; get; }
        public Double SoTien { set; get; }
        public int LoaiPhuCap { set; get; }

    }
    public class NhanVienBangChamCongViewModel
    {
        public string IdNhanVien { set; get; }
        public string MaNhanVien { set; get; }
        public string TenNhanVien { set; get; }
        public string TenBoPhan { set; get; }
        public long IdBoPhanEOS { set; get; }
        public double NgayCongBT { set; get; }
        public double NgayCongCN { set; get; }
        public double GioCongBT { set; get; }
        public double GioCongCN { set; get; }
        public double RaSomSoLan { set; get; }
        public double RaSomSoPhut { set; get; }
        public double VaoTreSoLan { set; get; }
        public double VaoTreSoPhut { set; get; }
        public double NgayNghiPhep { set; get; }
        public double NgayGianCach { set; get; }
        public double NgayNghiLe { set; get; }
        public double NgayNghiKhongLuong { set; get; }
        public double NgayNghiCoLuong { set; get; }
        public double SoNgayPhepDauKy { set; get; }
        public double SoNgayPhepConLai { set; get; }
    }
    public class NhanVienDiSomVeMuonViewModel
    {
        public long Id { set; get; }

        public string TenNguoiDeNghi { get; set; }
        public string IdNguoiDeNghi { get; set; }
        public string MucDich { get; set; }
        public System.DateTime ThoiGian { get; set; }
        public string GhiChu { get; set; }
        public string HinhThuc { get; set; }
        public System.DateTime ThoiGianDiMuonVeSom { get; set; }
        public System.DateTime ThoiGianTu { get; set; }
        public System.DateTime ThoiGianDen { get; set; }
        public string LoaiCongViec { get; set; }


        public string SoPhieu { get; set; }
        public bool Loai { get; set; }


    }
}
