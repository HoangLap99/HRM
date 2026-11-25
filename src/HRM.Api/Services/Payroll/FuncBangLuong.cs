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





        //    public dynamic GetDanhSachBangChamCongDaDuyet(TimKiem TimKiem, ApplicationDbContext db)
        //    {
        //        //   TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
        //        int CurrentPage = TimKiem.CurrentPage ?? 1;
        //        int PageSize = TimKiem.PageSize ?? 15;
        //        CallWebService _CallWebService = new CallWebService();
        //        string IdTrangThaiCuoi = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHCHAMCONG.ToString());

        //        IEnumerable<HRM_ChamCong_Detail> listChamCongDaDuyet = from a in db.HRM_ChamCong_Detail
        //                                                               where (TimKiem.IdCongTyEOS == 0 || a.IdCongTyEOS == TimKiem.IdCongTyEOS)
        //                                                               && (TimKiem.IddmDotChamCong == "" || TimKiem.IddmDotChamCong == null || a.IdDanhMucDotChamCongThang == TimKiem.IddmDotChamCong)
        //                                                             && a.Thang == TimKiem.Thang && a.Nam == TimKiem.Nam && a.IdTrangThai == IdTrangThaiCuoi
        //                                                               select a;
        //        int count = listChamCongDaDuyet.Count();
        //        var item = listChamCongDaDuyet.OrderByDescending(q => q.Modified).Skip(PageSize * (CurrentPage - 1)).Take(PageSize).ToList();

        //        dynamic dnm = new DynamicObject();
        //        dnm.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
        //        dnm.items = item;
        //        return dnm;
        //    }

        //public dynamic GetTongHopBangChamCongCongTy(TimKiem timKiem, ApplicationDbContext db)
        //{
        //    int CurrentPage = timKiem.CurrentPage ?? 0;
        //    int PageSize = timKiem.PageSize ?? 15;
        //    string IdTrangThaiCuoiChamCong = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHCHAMCONG.ToString());
        //    string sWhere = "";
        //    sWhere = " where b.idTrangThai='" + IdTrangThaiCuoiChamCong + "'";
        //    double DenNgayUnix = timKiem.DenNgayUnix ?? 0;
        //    DateTime DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
        //    sWhere = sWhere + " and     Convert(date, '" + DenNgay.ToString(m_sShortDateTime) + "') >= Convert(date, a.NgayCong)";

        //    double TuNgayUnix = timKiem.TuNgayUnix ?? 0;
        //    DateTime TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
        //    sWhere = sWhere + " and   Convert(date, a.NgayCong) >=   Convert(date, '" + TuNgay.ToString(m_sShortDateTime) + "')";

        //    if (timKiem.IdBoPhanEOS > 0)
        //        sWhere = sWhere + " and a.IdBoPhanEOS=" + timKiem.IdBoPhanEOS.Value;
        //    if (timKiem.IdCongTyEOS > 0)
        //        sWhere = sWhere + " and b.IdCongTyEOS=" + timKiem.IdCongTyEOS.Value;

        //    string sSqlSelect = " select a.* from HRM_ChamCong_NhanVien_ThoiGian_BoPhan  a join HRM_ChamCong_Detail b on a.IdChamCongDetail=b.IdGuid " + sWhere;
        //    List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> listChamCongNhanViens = db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.SqlQuery(sSqlSelect).ToList();
        //    List<string> listIdNhanVien = listChamCongNhanViens.Select(r => r.IdNhanVien).Distinct().ToList();


        //    var listHopDongs = db.HRM_HopDong.Where(r => r.isPhuLuc == false && (r.NgayHetHan.Year < 2000 || r.NgayHetHan > TuNgay) && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
        //    List<HRM_NhanVien> nhanViens = new List<HRM_NhanVien>();
        //    var nhanViens_AsEnum = db.HRM_NhanVien.Where(r => listIdNhanVien.Contains(r.IdGuid)).AsEnumerable();
        //    if (CurrentPage == 0)
        //        nhanViens = nhanViens_AsEnum.ToList();//db.HRM_NhanVien.Where(r => listIdNhanVien.Contains(r.IdGuid)).ToList();
        //    else
        //    {


        //        nhanViens = nhanViens_AsEnum.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

        //    }
        //    List<long> listIdNhanVienEOS = nhanViens.Select(r => r.IdNhanVienEOS).ToList();
        //    string IdNhanVienEOS = "";
        //    for (int i = 0; i < listIdNhanVienEOS.Count(); i++)
        //    {
        //        if (i == 0)
        //        {
        //            IdNhanVienEOS = listIdNhanVienEOS[i].ToString();
        //        }
        //        else
        //        {
        //            IdNhanVienEOS = IdNhanVienEOS + "," + listIdNhanVienEOS[i].ToString();
        //        }

        //    }


        //    string IdTrangThaiCuoiDiMuonVeSom = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHDIMUONVESOM.ToString());
        //    string sSelectDiMuonVeSom = "select b.Id,a.ThoiGian,a.HinhThuc,a.GhiChu ,a.ThoiGianDiMuonVeSom ,a.ThoiGianTu,a.ThoiGianDen ,a.SoPhieu ,a.Loai,a.TenNguoiDeNghi   from QT_QuyTrinhDiMuonVeSom a join NhanViens b on a.IdNguoiDeNghi=b.UserPkid  where a.IdTrangThai='" + IdTrangThaiCuoiDiMuonVeSom +
        //        "' and Convert(date, '" + DenNgay.ToString(m_sShortDateTime) + "') >= Convert(date, a.ThoiGian) " +
        //        " and  Convert(date, a.ThoiGian) >=   Convert(date, '" + TuNgay.ToString(m_sShortDateTime) + "') ";
        //    if (IdNhanVienEOS.Length > 0)
        //        sSelectDiMuonVeSom = sSelectDiMuonVeSom + " and b.Id in (" + IdNhanVienEOS + ")";
        //    List<NhanVienDiSomVeMuonViewModel> listDiSomVeMuonNVs = smdb.Database.SqlQuery<NhanVienDiSomVeMuonViewModel>(sSelectDiMuonVeSom).ToList();

        //    string DiMuon = "DiMuon";
        //    string VeSom = "VeSom";

        //    string CN = "CN";
        //    var boPhans = _CallWebService.GetDanhSachBoPhan(db);
        //    List<NhanVienBangChamCongViewModel> listItem = new List<NhanVienBangChamCongViewModel>();
        //    //DateTime dateTimeNow = DateTime.Now;
        //    string IdTrangThaiCuoi = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString());

        //    //var lstHinhThucNghi = smdb.DanhMucHinhThucNghis.ToList();
        //    string IdHinhThucNghi = db.HRM_dmHinhThucNghi.Where(r => r.Ma == DefineData.eMaNghiPhep.GC.ToString()).Select(r => r.IdGuid).FirstOrDefault();
        //    TuNgay = TuNgay.AddDays(-1);
        //    DenNgay = DenNgay.AddDays(1);
        //    var smgaynghis = (from a in smdb.QT_QuyTrinhXinNghiPhep
        //                      join b in smdb.QT_XinNghiPhep_ChiTietNgayNghi on a.Id equals b.IdQuyTrinh
        //                      join c in smdb.NhanViens on a.IdNguoiDeNghi equals c.UserPkid
        //                      where listIdNhanVienEOS.Contains(c.Id) && b.IdHinhThucNghi == IdHinhThucNghi && a.IdTrangThai == IdTrangThaiCuoi && a.Loai != true && a.ThoiGianBatDau > TuNgay && DenNgay > a.ThoiGianBatDau
        //                      select new { b.ThoiGianNghi, c.Id }).ToList();
        //    foreach (var item in nhanViens)
        //    {
        //        var Hopdong = listHopDongs.Where(r => r.IdNhanVien == item.IdGuid).FirstOrDefault();
        //        NhanVienBangChamCongViewModel nhanvien = new NhanVienBangChamCongViewModel();

        //        double SoNgayPhepDauKy = 0;
        //        FuncNhanVien func = new FuncNhanVien();
        //        SoNgayPhepDauKy = func.GetSoNgayPhepNamConLai(item.IdNhanVienEOS, db);


        //        nhanvien.IdBoPhanEOS = Hopdong.IdBoPhanEOS;

        //        var nhanvienChamCong = listChamCongNhanViens.Where(r => r.IdNhanVien == item.IdGuid).ToList();
        //        List<NhanVienDiSomVeMuonViewModel> listDiSomVeMuonNV = listDiSomVeMuonNVs.Where(r => r.Id == item.IdNhanVienEOS && (r.HinhThuc == DiMuon || r.HinhThuc == VeSom)).ToList();
        //        var listDiSomVeMuonNVDiMuon = listDiSomVeMuonNV.Where(r => r.HinhThuc == DiMuon).ToList();
        //        var listDiSomVeMuonNVVeSom = listDiSomVeMuonNV.Where(r => r.HinhThuc == VeSom).ToList();
        //        var sngaynghi = smgaynghis.Where(r => r.Id == item.IdNhanVienEOS).ToList();
        //        nhanvien.IdNhanVien = item.IdGuid;
        //        nhanvien.NgayGianCach = sngaynghi.Sum(r => r.ThoiGianNghi);
        //        nhanvien.TenNhanVien = item.TenDayDu;
        //        nhanvien.MaNhanVien = item.MaNhanVien;


        //        //if (item.IdGuid == "3ec41910-dba0-4954-bed8-4dff599cd5f8")
        //        //    nhanvien.IdNhanVien = nhanvien.IdNhanVien;
        //        nhanvien.NgayCongBT = nhanvienChamCong.Where(r => r.isCheckCa1 == true && r.ThuVietTat != CN).Count() * 0.5 + nhanvienChamCong.Where(r => r.isCheckCa2 == true && r.ThuVietTat != CN).Count() * 0.5;
        //        nhanvien.NgayCongCN = nhanvienChamCong.Where(r => r.isCheckCa1 == true && r.ThuVietTat == CN).Count() * 0.5 + nhanvienChamCong.Where(r => r.isCheckCa2 == true && r.ThuVietTat == CN).Count() * 0.5;
        //        nhanvien.GioCongCN = nhanvienChamCong.Where(r => r.ThuVietTat == CN).Sum(r => r.SoGioSang) + nhanvienChamCong.Where(r => r.ThuVietTat == CN).Sum(r => r.SoGioChieu);
        //        nhanvien.GioCongBT = nhanvienChamCong.Where(r => r.ThuVietTat != CN).Sum(r => r.SoGioSang) + nhanvienChamCong.Where(r => r.ThuVietTat != CN).Sum(r => r.SoGioChieu);

        //        nhanvien.VaoTreSoLan = listDiSomVeMuonNVDiMuon.Count();

        //        foreach (var itemDiMuon in listDiSomVeMuonNVDiMuon)
        //        {

        //            nhanvien.VaoTreSoPhut = nhanvien.VaoTreSoPhut + dungchung.DateTimeToUnixTimeStamp(itemDiMuon.ThoiGianDiMuonVeSom) / 60;
        //        }
        //        nhanvien.RaSomSoLan = listDiSomVeMuonNVVeSom.Count();
        //        foreach (var itemRaSom in listDiSomVeMuonNVVeSom)
        //        {
        //            nhanvien.RaSomSoPhut = nhanvien.RaSomSoPhut + dungchung.DateTimeToUnixTimeStamp(itemRaSom.ThoiGianDiMuonVeSom) / 60;
        //        }
        //        nhanvien.NgayNghiPhep = nhanvienChamCong.Where(r => r.MaChamCongSang == "P").Count() * 0.5 + nhanvienChamCong.Where(r => r.MaChamCongChieu == "P").Count() * 0.5;
        //        nhanvien.NgayNghiLe = nhanvienChamCong.Where(r => r.isNghiLe == true).Count() / 2;
        //        nhanvien.NgayNghiKhongLuong = nhanvienChamCong.Where(r => r.MaChamCongSang == "KL").Count() * 0.5 + nhanvienChamCong.Where(r => r.MaChamCongChieu == "KL").Count() * 0.5;
        //        nhanvien.NgayNghiCoLuong = nhanvienChamCong.Where(r => r.MaChamCongSang == "HL").Count() * 0.5 + nhanvienChamCong.Where(r => r.MaChamCongChieu == "HL").Count() * 0.5;

        //        nhanvien.SoNgayPhepConLai = SoNgayPhepDauKy;
        //        nhanvien.SoNgayPhepDauKy = SoNgayPhepDauKy + nhanvien.NgayNghiCoLuong;
        //        nhanvien.TenBoPhan = boPhans.Where(r => r.Id == nhanvien.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();

        //        listItem.Add(nhanvien);
        //    }

        //    if (CurrentPage == 0)
        //        return listItem;
        //    else
        //    {
        //        dynamic dnm = new DynamicObject();
        //        dnm.items = listItem;

        //        int count = nhanViens_AsEnum.Count();
        //        PagingInfor paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
        //        dnm.paging = paging;
        //        return dnm;
        //    }
        //}
        //public dynamic ExportTongHopBangChamCongCongTy(TimKiem timKiem, ApplicationDbContext db)
        //{
        //    CallWebService _CallWebService = new CallWebService();
        //    string sFile = "BangChamCong.xlsx";

        //    string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);


        //    List<NhanVienBangChamCongViewModel> listBangChamCongNhanVien = GetTongHopBangChamCongCongTy(timKiem, db);
        //    Stream s = File.OpenRead(sFileName);
        //    ExcelPackage package = new ExcelPackage(s);
        //    ExcelWorksheet sheet1 = package.Workbook.Worksheets[1];
        //    s.Close();
        //    int nRow = 8;
        //    var CongTy = _CallWebService.GetBoPhan(db, timKiem.IdCongTyEOS ?? 0);

        //    if (CongTy != null)
        //    {
        //        sheet1.Cells[1, 2].Value = CongTy.TenBoPhan;
        //        sheet1.Cells[2, 2].Value = CongTy.DiaChi;
        //    }
        //    double TuNgayUnix = timKiem.TuNgayUnix ?? 0;
        //    DateTime TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
        //    double DenNgayUnix = timKiem.DenNgayUnix ?? 0;
        //    DateTime DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
        //    string TuNgayDenNgay = "Từ ngày " + TuNgay.ToString("dd/MM/yyyy") + " đến ngày " + DenNgay.ToString("dd/MM/yyyy");
        //    sheet1.Cells[4, 1].Value = TuNgayDenNgay;
        //    var listIdBoPhan = listBangChamCongNhanVien.GroupBy(r => r.IdBoPhanEOS).Select(r => r.Key).ToList();
        //    foreach (var itemPhongBan in listIdBoPhan)
        //    {
        //        sheet1.Cells[nRow, 1, nRow, 6].Merge = true;
        //        var listBangChamCongNhanVienBoPhan = listBangChamCongNhanVien.Where(r => r.IdBoPhanEOS == itemPhongBan).ToList();
        //        if (itemPhongBan == 0)
        //            sheet1.Cells[nRow, 1].Value = "";
        //        else
        //            sheet1.Cells[nRow, 1].Value = listBangChamCongNhanVienBoPhan[0].TenBoPhan;
        //        sheet1.Cells[nRow, 1].Style.Font.Bold = true;
        //        sheet1.Cells[nRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //        int STT = 1;
        //        nRow++;
        //        foreach (var item in listBangChamCongNhanVienBoPhan)
        //        {
        //            sheet1.Cells[nRow, 1].Value = STT;
        //            sheet1.Cells[nRow, 2].Value = item.MaNhanVien;
        //            sheet1.Cells[nRow, 3].Value = item.TenNhanVien;
        //            sheet1.Cells[nRow, 4].Value = item.NgayCongBT;
        //            sheet1.Cells[nRow, 5].Value = item.NgayCongCN;
        //            sheet1.Cells[nRow, 6].Value = item.GioCongBT;
        //            sheet1.Cells[nRow, 7].Value = item.GioCongCN;
        //            sheet1.Cells[nRow, 8].Value = item.VaoTreSoLan;
        //            sheet1.Cells[nRow, 9].Value = item.VaoTreSoPhut;
        //            sheet1.Cells[nRow, 10].Value = item.RaSomSoLan;
        //            sheet1.Cells[nRow, 11].Value = item.RaSomSoPhut;
        //            sheet1.Cells[nRow, 12].Value = item.NgayNghiPhep;
        //            sheet1.Cells[nRow, 13].Value = item.NgayNghiLe;
        //            sheet1.Cells[nRow, 14].Value = item.NgayGianCach;
        //            sheet1.Cells[nRow, 15].Value = item.NgayNghiKhongLuong;
        //            sheet1.Cells[nRow, 16].Value = item.NgayNghiCoLuong;
        //            sheet1.Cells[nRow, 17].Value = item.SoNgayPhepDauKy;
        //            sheet1.Cells[nRow, 18].Value = item.SoNgayPhepConLai;
        //            nRow++;
        //            STT++;
        //        }
        //    }

        //    DrawTableExcel(sheet1, 5, nRow - 1, 1, 18);

        //    nRow = nRow + 1;
        //    sheet1.Cells[nRow, 2, nRow, 3].Merge = true;
        //    sheet1.Cells[nRow, 2].Value = " Phòng TC-HC";
        //    sheet1.Cells[nRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //    sheet1.Cells[nRow, 10, nRow, 14].Merge = true;
        //    sheet1.Cells[nRow, 10].Value = " Người lập biểu";
        //    sheet1.Cells[nRow, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    nRow = nRow + 4;
        //    sheet1.Cells[nRow, 10, nRow, 14].Merge = true;
        //    sheet1.Cells[nRow, 10].Value = db.currentUser.TenNhanVien;
        //    sheet1.Cells[nRow, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    dynamic result = new DynamicObject();

        //    string name = "BangChamCong.xlsx";
        //    var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
        //    string pathresult =PathUploads+ filenamegui;
        //    File.WriteAllBytes(pathresult, package.GetAsByteArray());

        //    HDFile newfile = new HDFile();
        //    newfile.Name = filenamegui;
        //    newfile.NameLocal = name;

        //    newfile.Url = "/uploader/api/hdfiles?filename=" + HttpUtility.UrlEncode(dungchung.Base64Encode(name))
        //                + "&path=" + dungchung.Base64Encode(pathresult);
        //    result.file = newfile;
        //    return result;
        //}
        //    public dynamic GetBangLuongTheoBangChamCong(List<string> ListIdBangChamCong, ApplicationDbContext db)
        //    {
        //        /* loai
        //         * 1 Tháng
        //         * 2 Ngày
        //         * 3 Giờ
        //         * 4 Năm
        //         */

        //        /* 0 Lương gross
        //         * 1 net 
        //         */
        //        List<NhanVienLuongViewModel> listLuongNhanVien = new List<NhanVienLuongViewModel>();
        //        string sqlIdBangChamCong = "";
        //        for (int i = 0; i < ListIdBangChamCong.Count(); i++)
        //        {
        //            if (i == 0)
        //            {
        //                sqlIdBangChamCong = "'" + ListIdBangChamCong[i].ToString() + "'";
        //            }
        //            else
        //            {
        //                sqlIdBangChamCong = sqlIdBangChamCong + "," + "'" + ListIdBangChamCong[i].ToString() + "'";
        //            }

        //        }

        //        var ListBoPhanEOS = _CallWebService.GetDanhSachBoPhan(db);
        //        string NhanVienChamCong = "select * from HRM_ChamCong_NhanVien_ThoiGian_BoPhan where IdChamCongDetail in (" + sqlIdBangChamCong + ")";
        //        List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> lstChamCongNhanViens = db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.SqlQuery(NhanVienChamCong).ToList();
        //        string sqlNgayChamCongNhanVienTheoBangLuong = " select IdNhanVien,MIN(NgayCong) as NgayBatDau, MAX(NgayCong) as NgayKetThuc from HRM_ChamCong_NhanVien_ThoiGian_BoPhan where IdChamCongDetail in (" + sqlIdBangChamCong + ")" + " group by IdNhanVien ";
        //        List<NgayChamCongNhanVienModel> listNgayChamCongNhanVien = db.Database.SqlQuery<NgayChamCongNhanVienModel>(sqlNgayChamCongNhanVienTheoBangLuong).ToList();
        //        DateTime NgayBatDauMin = listNgayChamCongNhanVien.Select(r => r.NgayBatDau).Min();
        //        DateTime NgayKetThucMax = listNgayChamCongNhanVien.Select(r => r.NgayKetThuc).Max();
        //        var lstNhanVien = listNgayChamCongNhanVien.Select(r => r.IdNhanVien).ToList();

        //        var lstCaiDatNghiLe = smdb.CaiDatHeThongs.AsNoTracking().Where(q => q.Ma == "NgayNghiLe").ToList();
        //        List<HRM_NhanVien> listNhanViens = db.HRM_NhanVien.Where(r => lstNhanVien.Contains(r.IdGuid)).ToList();
        //        List<DateTime> lstNgayNghiLe = new List<DateTime>();
        //        foreach (var item in lstCaiDatNghiLe)
        //        {
        //            for (int i = 0; item.ValueEndDateTime.AddDays(1) > item.ValueStartDateTime.AddDays(i); i++)
        //            {
        //                lstNgayNghiLe.Add(item.ValueStartDateTime.AddDays(i));

        //            }
        //        }

        //        var ListNgayT7 = lstChamCongNhanViens.Where(r => r.ThuVietTat == "T7" && !lstNgayNghiLe.Contains(r.NgayCong)).GroupBy(x => x.NgayCong).Select(a => a.Key).ToList();
        //        var ListNgayCN = lstChamCongNhanViens.Where(r => r.ThuVietTat == "CN" && !lstNgayNghiLe.Contains(r.NgayCong)).GroupBy(x => x.NgayCong).Select(a => a.Key).ToList();

        //        NgayKetThucMax = NgayKetThucMax.AddDays(1);

        //        List<HRM_Luong_NhanVien> luongNhanVien = db.HRM_Luong_NhanVien.Where(r => lstNhanVien.Contains(r.IdNhanVien) && ((r.NgayHetHieuLuc.Year < 2000 || NgayKetThucMax > r.NgayBatDauHieuLuc) || (NgayKetThucMax > r.NgayHetHieuLuc && r.NgayHetHieuLuc >= NgayBatDauMin) || (r.NgayHetHieuLuc >= NgayKetThucMax && NgayKetThucMax > r.NgayBatDauHieuLuc))).ToList();

        //        List<string> listIdNhanVien = listNhanViens.Select(r => r.IdGuid).ToList();
        //        List<long> listIdNhanVienEOS = listNhanViens.Select(r => r.IdNhanVienEOS).ToList();
        //        string IdTrangThaiCuoiLamThemGio = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHLAMTHEMGIO.ToString());
        //        var listNhanVienLamThemGio = (from a in smdb.NhanViens
        //                                      join b in smdb.QT_Link_NhanVien_LamThemGio on a.UserPkid equals b.IdUser
        //                                      join c in smdb.QT_QuyTrinhLamThemGio on b.IdQuyTrinh equals c.Id
        //                                      where c.IdTrangThai == IdTrangThaiCuoiLamThemGio && c.Loai != true
        //                                      && NgayKetThucMax > b.ThoiGianKetThuc && b.ThoiGianBatDau > NgayBatDauMin
        //                                      && listIdNhanVienEOS.Contains(a.Id)
        //                                      select new
        //                                      {
        //                                          IdNhanVienEOS = a.Id,
        //                                          b.ThoiGianBatDau,
        //                                          b.ThoiGianKetThuc,
        //                                          b.ThoiGian,
        //                                          b.NgayTrongTuan,
        //                                          b.PhuCap
        //                                      }).ToList();
        //        var dmPhuCaps = db.HRM_DM_PhuCap.ToList();



        //        //List<long> listIdNhanVienEOS = nhanViens.Select(r => r.IdNhanVienEOS).ToList();
        //        string IdNhanVienEOS = "";
        //        for (int i = 0; i < listIdNhanVienEOS.Count(); i++)
        //        {
        //            if (i == 0)
        //            {
        //                IdNhanVienEOS = listIdNhanVienEOS[i].ToString();
        //            }
        //            else
        //            {
        //                IdNhanVienEOS = IdNhanVienEOS + "," + listIdNhanVienEOS[i].ToString();
        //            }

        //        }


        //        string IdTrangThaiCuoiDiMuonVeSom = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHDIMUONVESOM.ToString());
        //        string sSelectDiMuonVeSom = "select b.Id,a.ThoiGian,a.HinhThuc,a.GhiChu ,a.ThoiGianDiMuonVeSom ,a.ThoiGianTu,a.ThoiGianDen ,a.SoPhieu ,a.Loai,a.TenNguoiDeNghi   from QT_QuyTrinhDiMuonVeSom a join NhanViens b on a.IdNguoiDeNghi=b.UserPkid  where a.IdTrangThai='" + IdTrangThaiCuoiDiMuonVeSom +
        //            "' and Convert(date, '" + NgayKetThucMax.ToString(m_sShortDateTime) + "') >= Convert(date, a.ThoiGian) " +
        //            " and  Convert(date, a.ThoiGian) >=   Convert(date, '" + NgayBatDauMin.ToString(m_sShortDateTime) + "') ";
        //        if (IdNhanVienEOS.Length > 0)
        //            sSelectDiMuonVeSom = sSelectDiMuonVeSom + " and b.Id in (" + IdNhanVienEOS + ")";
        //        List<NhanVienDiSomVeMuonViewModel> listDiSomVeMuonNVs = smdb.Database.SqlQuery<NhanVienDiSomVeMuonViewModel>(sSelectDiMuonVeSom).ToList();

        //        string DiMuon = "DiMuon";
        //        string VeSom = "VeSom";


        //        var HopDongs = (from a in db.HRM_HopDong
        //                        join b in db.HRM_BaoHiem_HopDong on a.IdGuid equals b.IdHopDong
        //                        join c in db.HRM_Luong_HopDong on a.IdGuid equals c.IdHopDong
        //                        where lstNhanVien.Contains(a.IdNhanVien) && a.TrangThai == "HieuLuc" && a.isPhuLuc == false
        //                        select new
        //                        {
        //                            a.IdNhanVien,
        //                            b.isBaoHiemXaHoi,
        //                            b.isBaoHiemThatNghiep,
        //                            a.Modified,
        //                            b.isBaoHiemYTe,
        //                            c.isTinhLuong,
        //                            c.TyLeHuongLuong,
        //                            isCongDoan = a.isThamGiaCongDoan,
        //                            a.IdBoPhanEOS,

        //                        }).ToList();






        //        var CaiDatGioLamViec = smdb.CaiDatGioLamViecs.AsNoTracking().FirstOrDefault();
        //        double SoGLV = 8;
        //        if (CaiDatGioLamViec != null)
        //            SoGLV = CaiDatGioLamViec.GioLamNgayThuong;


        //        var listKhenthuongKyLuat = db.HRM_Luong_KhenThuongKyLuat.Where(r => listIdNhanVien.Contains(r.IdNhanVien) && r.KyLuongThucHien >= NgayBatDauMin && NgayKetThucMax > r.KyLuongThucHien).ToList();

        //        List<HRM_DM_CauHinhThueThuNhapCaNhan> listDmCauHinhThueThuNhapCaNhans = db.HRM_DM_CauHinhThueThuNhapCaNhan.ToList();
        //        double SoGiamTruCaNhan = listDmCauHinhThueThuNhapCaNhans.OrderBy(r => r.MucLuong).Select(r => r.MucLuong).FirstOrDefault();
        //        string IddmPhuCap = dmPhuCaps.Where(r => r.Ma == "PCLTG").Select(r => r.IdGuid).FirstOrDefault();
        //        List<string> MaBoPhans = new List<string> { "LX", "MTV - LX" };
        //        List<long> listIdBoPhan = ListBoPhanEOS.Where(r => MaBoPhans.Contains(r.MaBoPhan)).Select(r => r.Id).ToList();



        //        foreach (var item in listNgayChamCongNhanVien)
        //        {


        //            var HopDongNhanVien = HopDongs.Where(r => r.IdNhanVien == item.IdNhanVien).FirstOrDefault();
        //            if (HopDongNhanVien == null)
        //                continue;
        //            if (HopDongNhanVien.isTinhLuong == false)
        //                continue;
        //            var LuongNhanVien = luongNhanVien.Where(r => r.IdNhanVien == item.IdNhanVien && ((r.NgayHetHieuLuc.Year < 2000 && NgayKetThucMax > r.NgayBatDauHieuLuc) || (NgayKetThucMax > r.NgayHetHieuLuc && r.NgayHetHieuLuc >= NgayBatDauMin)) && (NgayBatDauMin >= r.NgayHetHieuLuc)).ToList();
        //            if (LuongNhanVien.Count() == 0)
        //                continue;
        //            HRM_NhanVien nhanVien = listNhanViens.FirstOrDefault(r => r.IdGuid == item.IdNhanVien);

        //            var NhanVienLamThemGio = listNhanVienLamThemGio.Where(r => r.IdNhanVienEOS == nhanVien.IdNhanVienEOS).ToList();

        //            var lstNguoiPhuThuoc = db.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == item.IdNhanVien && r.isLaNguoiPhuThuoc == true).ToList();
        //            var nhanViens = listNhanViens.Where(r => r.IdGuid == item.IdNhanVien).FirstOrDefault();
        //            if (nhanViens == null)
        //                continue;
        //            List<NhanVienDiSomVeMuonViewModel> listDiSomVeMuonNV = listDiSomVeMuonNVs.Where(r => r.Id == nhanViens.IdNhanVienEOS && (r.HinhThuc == DiMuon || r.HinhThuc == VeSom)).ToList();
        //            var listDiSomVeMuonNVDiMuon = listDiSomVeMuonNV.Where(r => (r.HinhThuc == DiMuon || r.HinhThuc == VeSom)).ToList();

        //            double SoPhut = 0;

        //            foreach (var itemMuon in listDiSomVeMuonNVDiMuon)
        //            {
        //                SoPhut = SoPhut + dungchung.DateTimeToUnixTimeStamp(itemMuon.ThoiGianDiMuonVeSom) / 60;
        //            }


        //            if (LuongNhanVien.Count() == 1)
        //            {
        //                long IdBoPhanEOS = HopDongNhanVien.IdBoPhanEOS;// LuongNhanVien[0].IdBoPhanEOS;
        //                string IdLuongNhanVien = LuongNhanVien[0].IdGuid;

        //                string sSqlPhuCapNhanVien = "Select * from HRM_Luong_PhuCap_NhanVien where IdLuong_NhanVien='" + IdLuongNhanVien + "'";
        //                List<HRM_Luong_PhuCap_NhanVien> listPhuCapNhanVien = db.HRM_Luong_PhuCap_NhanVien.SqlQuery(sSqlPhuCapNhanVien).ToList();
        //                foreach (var itemPC in listPhuCapNhanVien)
        //                {
        //                    var PhuCap = dmPhuCaps.FirstOrDefault(r => r.IdGuid == itemPC.IddmLoaiPhuCap);
        //                    if (PhuCap != null)
        //                        itemPC.isTinhThueTNCN = PhuCap.isTinhThueTNCN;

        //                }
        //                double SoNgayLamViec = 0;
        //                var BoPhans = ListBoPhanEOS.Where(r => r.Id == IdBoPhanEOS).FirstOrDefault();
        //                if (BoPhans != null && BoPhans.SoNgayLamViec != 0)
        //                    SoNgayLamViec = BoPhans.SoNgayLamViec;
        //                else
        //                {
        //                    SoNgayLamViec = 26;
        //                }


        //                var listChamCongNhanVien = lstChamCongNhanViens.Where(r => r.IdNhanVien == item.IdNhanVien).ToList();
        //                if (listChamCongNhanVien.Count() == 0)
        //                    continue;
        //                var NgayCongNhanVien = (from a in listChamCongNhanVien
        //                                        group a by a.IdNhanVien into b
        //                                        select new
        //                                        {
        //                                            IdNhanVien = b.Key,

        //                                            SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //                                            SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //                                            SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,
        //                                            SoNgayLamViecDuThoiGian = b.Where(r => r.MaChamCongChieu == "DL" && r.MaChamCongChieu == "DL").Count(),

        //                                        }).FirstOrDefault();
        //                NhanVienLuongViewModel nhanVienLuong = new NhanVienLuongViewModel();
        //                nhanVienLuong.KieuLuong = LuongNhanVien[0].KieuLuong;
        //                nhanVienLuong.IdNhanVien = item.IdNhanVien;

        //                //nhanVienLuong.RaSomSoPhut = SoPhutVeSom;
        //                //nhanVienLuong.VaoTreSoPhut =SoPhutDiMuon;
        //                nhanVienLuong.MaNhanVien = nhanVien.MaNhanVien;
        //                nhanVienLuong.NghiPhep = NgayCongNhanVien.SoNgayNghiPhep;
        //                nhanVienLuong.IdBoPhanEOS = IdBoPhanEOS;
        //                nhanVienLuong.KhenThuong = listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKhenThuong.ToString()).Sum(r => r.SoTien);
        //                nhanVienLuong.TenNhanVien = nhanVien.TenDayDu;
        //                nhanVienLuong.TroCapNgoai = NhanVienLamThemGio.Sum(r => r.PhuCap);
        //                var TangCaNgayT7 = NhanVienLamThemGio.Where(r => ListNgayT7.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                var TangCaNgayCN = NhanVienLamThemGio.Where(r => ListNgayCN.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                var TangCaNgayLe = NhanVienLamThemGio.Where(r => lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                var TangCaNgayThuong = NhanVienLamThemGio.Where(r => !lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date) && !ListNgayT7.Contains(r.ThoiGianBatDau.Date) && !ListNgayCN.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                foreach (var itemTC in TangCaNgayT7)
        //                {
        //                    if (listIdBoPhan.Contains(IdBoPhanEOS))
        //                        nhanVienLuong.TangCaNgayT7 = nhanVienLuong.TangCaNgayT7 + itemTC.ThoiGian; //((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                    else
        //                    {
        //                        nhanVienLuong.TangCaNgayT7 = nhanVienLuong.TangCaNgayT7 + itemTC.ThoiGian;// ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60)) * 1.5;
        //                    }
        //                }
        //                foreach (var itemTC in TangCaNgayCN)
        //                {
        //                    nhanVienLuong.TangCaNgayCN = nhanVienLuong.TangCaNgayCN + itemTC.ThoiGian;// ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                }
        //                foreach (var itemTC in TangCaNgayLe)
        //                {
        //                    nhanVienLuong.TangCaNgayLe = nhanVienLuong.TangCaNgayT7 + itemTC.ThoiGian;//  ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                }
        //                foreach (var itemTC in TangCaNgayThuong)
        //                {
        //                    nhanVienLuong.TangCaNgayThuong = nhanVienLuong.TangCaNgayThuong + itemTC.ThoiGian;//  ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                }
        //                double thoigianMuon = Math.Round(SoPhut / 60, 2);
        //                nhanVienLuong.SoGioLamThucTe = NgayCongNhanVien.SoGioLamViec - thoigianMuon;
        //                nhanVienLuong.SoNgayLamViecDuThoiGian = NgayCongNhanVien.SoNgayLamViecDuThoiGian;

        //                if (NgayCongNhanVien != null)
        //                {
        //                    nhanVienLuong.SoNgayCongThucTe = NgayCongNhanVien.SoNgayDiLam;
        //                    nhanVienLuong.NgayCongCoSo = SoNgayLamViec;
        //                    nhanVienLuong.MucLuong = LuongNhanVien[0].LuongHachToan;
        //                    List<PhuCapNhanVienViewModel> listPhuCap = new List<PhuCapNhanVienViewModel>();
        //                    foreach (var itemPC in listPhuCapNhanVien)
        //                    {
        //                        if (itemPC.isTinhThueTNCN == false)
        //                        {
        //                            if (itemPC.LoaiPhuCap == 1 && itemPC.IddmLoaiPhuCap != IddmPhuCap)
        //                            {
        //                                nhanVienLuong.PhuCapKhongTheoCongChinh = nhanVienLuong.PhuCapKhongTheoCongChinh + itemPC.SoTien;
        //                                nhanVienLuong.PhuCapKhongTinhThue = nhanVienLuong.PhuCapKhongTinhThue + itemPC.SoTien;
        //                            }
        //                            else if (itemPC.LoaiPhuCap == 1 && itemPC.IddmLoaiPhuCap == IddmPhuCap)
        //                            {
        //                                nhanVienLuong.PhuCapLamNgoaiGio = nhanVienLuong.PhuCapLamNgoaiGio + itemPC.SoTien;
        //                            }
        //                            else if (itemPC.LoaiPhuCap == 2 && itemPC.IddmLoaiPhuCap != IddmPhuCap)
        //                            {
        //                                nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                                nhanVienLuong.PhuCapKhongTinhThue = nhanVienLuong.PhuCapKhongTinhThue + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);

        //                            }
        //                            else if (itemPC.LoaiPhuCap == 2 && itemPC.IddmLoaiPhuCap == IddmPhuCap)
        //                            {
        //                                nhanVienLuong.PhuCapLamNgoaiGio = nhanVienLuong.PhuCapLamNgoaiGio + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                            }
        //                            else if (itemPC.LoaiPhuCap == 3 && itemPC.IddmLoaiPhuCap != IddmPhuCap)
        //                            {
        //                                nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                nhanVienLuong.PhuCapKhongTinhThue = nhanVienLuong.PhuCapKhongTinhThue + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                            }
        //                            else if (itemPC.LoaiPhuCap == 3 && itemPC.IddmLoaiPhuCap == IddmPhuCap)
        //                            {
        //                                nhanVienLuong.PhuCapLamNgoaiGio = nhanVienLuong.PhuCapLamNgoaiGio + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                nhanVienLuong.PhuCapKhongTheoCongChinh = nhanVienLuong.PhuCapKhongTheoCongChinh + itemPC.SoTien;
        //                            }

        //                        }
        //                        else
        //                        {
        //                            if (itemPC.LoaiPhuCap == 1)
        //                            {
        //                                nhanVienLuong.PhuCapKhongTheoCongChinh = nhanVienLuong.PhuCapKhongTheoCongChinh + itemPC.SoTien;
        //                                nhanVienLuong.PhuCapTinhThue = nhanVienLuong.PhuCapTinhThue + itemPC.SoTien;
        //                            }
        //                            else if (itemPC.LoaiPhuCap == 2)
        //                            {
        //                                nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                                nhanVienLuong.PhuCapTinhThue = nhanVienLuong.PhuCapTinhThue + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                            }
        //                            else if (itemPC.LoaiPhuCap == 3)
        //                            {
        //                                nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                nhanVienLuong.PhuCapTinhThue = nhanVienLuong.PhuCapTinhThue + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                            }
        //                        }

        //                    }
        //                    if (LuongNhanVien[0].LoaiLuong == 1)
        //                    {
        //                        nhanVienLuong.LuongTheoSoNgayCong = (nhanVienLuong.MucLuong * nhanVienLuong.SoNgayCongThucTe) / SoNgayLamViec;
        //                        double LuongTheoGio = nhanVienLuong.MucLuong / (SoNgayLamViec * SoGLV);
        //                        nhanVienLuong.LuongTangCa = LuongTheoGio * nhanVienLuong.TangCaNgayThuong + LuongTheoGio * nhanVienLuong.TangCaNgayT7 * 1.5 + LuongTheoGio * nhanVienLuong.TangCaNgayCN * 2 + LuongTheoGio * nhanVienLuong.TangCaNgayLe * 3;
        //                        nhanVienLuong.NSD_BHXH = (nhanVienLuong.MucLuong * 17.5) / 100;
        //                        nhanVienLuong.NSD_BHYT = (nhanVienLuong.MucLuong * 3) / 100;
        //                        nhanVienLuong.NSD_BHTN = (nhanVienLuong.MucLuong * 1) / 100;

        //                        nhanVienLuong.LuongNghiPhep = (nhanVienLuong.MucLuong * nhanVienLuong.NghiPhep) / SoNgayLamViec;
        //                        // Nguoi Lao đông
        //                        nhanVienLuong.NLD_BHXH = (nhanVienLuong.MucLuong * 8) / 100;
        //                        nhanVienLuong.NLD_BHYT = (nhanVienLuong.MucLuong * 1.5) / 100;
        //                        nhanVienLuong.NLD_BHTN = (nhanVienLuong.MucLuong * 1) / 100;
        //                        nhanVienLuong.NSD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) / 100;
        //                        nhanVienLuong.NLD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) / 100;

        //                    }
        //                    else if (LuongNhanVien[0].LoaiLuong == 2)
        //                    {
        //                        double LuongTheoGio = nhanVienLuong.MucLuong / SoGLV;
        //                        nhanVienLuong.LuongTangCa = LuongTheoGio * nhanVienLuong.TangCaNgayThuong + LuongTheoGio * nhanVienLuong.TangCaNgayT7 * 1.5 + LuongTheoGio * nhanVienLuong.TangCaNgayCN * 2 + LuongTheoGio * nhanVienLuong.TangCaNgayLe * 3;
        //                        nhanVienLuong.LuongTheoSoNgayCong = (nhanVienLuong.MucLuong * nhanVienLuong.SoNgayCongThucTe);
        //                        nhanVienLuong.NSD_BHXH = (nhanVienLuong.MucLuong * 17.5) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.NSD_BHYT = (nhanVienLuong.MucLuong * 3) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.NSD_BHTN = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.LuongNghiPhep = (nhanVienLuong.MucLuong * nhanVienLuong.NghiPhep);
        //                        // Nguoi Lao đông
        //                        nhanVienLuong.NLD_BHXH = (nhanVienLuong.MucLuong * 8) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.NLD_BHYT = (nhanVienLuong.MucLuong * 1.5) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.NLD_BHTN = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.NSD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        nhanVienLuong.NLD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                    }

        //                    else if (LuongNhanVien[0].LoaiLuong == 3)
        //                    {
        //                        nhanVienLuong.LuongTheoSoNgayCong = (nhanVienLuong.MucLuong * nhanVienLuong.SoGioLamThucTe);

        //                        double LuongTheoGio = nhanVienLuong.MucLuong;
        //                        nhanVienLuong.LuongTangCa = LuongTheoGio * nhanVienLuong.TangCaNgayThuong + LuongTheoGio * nhanVienLuong.TangCaNgayT7 * 1.5 + LuongTheoGio * nhanVienLuong.TangCaNgayCN * 2 + LuongTheoGio * nhanVienLuong.TangCaNgayLe * 3;
        //                        nhanVienLuong.NSD_BHXH = (nhanVienLuong.MucLuong * 17.5) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        nhanVienLuong.NSD_BHYT = (nhanVienLuong.MucLuong * 3) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        nhanVienLuong.NSD_BHTN = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        nhanVienLuong.NSD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoGioLamThucTe / 100;

        //                        // Nguoi Lao đông
        //                        nhanVienLuong.NLD_BHXH = (nhanVienLuong.MucLuong * 8) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        nhanVienLuong.NLD_BHYT = (nhanVienLuong.MucLuong * 1.5) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        nhanVienLuong.NLD_BHTN = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        nhanVienLuong.NLD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) * nhanVienLuong.SoGioLamThucTe / 100;
        //                    }
        //                    if (HopDongNhanVien.isBaoHiemThatNghiep == true)
        //                    {
        //                        nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_BHTN;
        //                        nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_BHTN;
        //                    }
        //                    else
        //                    {
        //                        nhanVienLuong.NSD_BHTN = 0;
        //                        nhanVienLuong.NLD_BHTN = 0;
        //                    }
        //                    if (HopDongNhanVien.isBaoHiemXaHoi == true)
        //                    {
        //                        nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_BHXH;
        //                        nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_BHXH;
        //                    }
        //                    else
        //                    {
        //                        nhanVienLuong.NSD_BHXH = 0;
        //                        nhanVienLuong.NLD_BHXH = 0;
        //                    }
        //                    if (HopDongNhanVien.isBaoHiemYTe == true)
        //                    {
        //                        nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_BHYT;
        //                        nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_BHYT;
        //                    }
        //                    else
        //                    {
        //                        nhanVienLuong.NSD_BHYT = 0;
        //                        nhanVienLuong.NLD_BHYT = 0;
        //                    }
        //                    if (HopDongNhanVien.isCongDoan == true)
        //                    {
        //                        nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_PhiCongDoan;
        //                        nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_PhiCongDoan;
        //                    }
        //                    else
        //                    {
        //                        nhanVienLuong.NSD_PhiCongDoan = 0;
        //                        nhanVienLuong.NLD_PhiCongDoan = 0;
        //                    }
        //                    if (LuongNhanVien[0].KieuLuong == 0)
        //                    {
        //                        nhanVienLuong.TongNhan_NLD = nhanVienLuong.LuongTheoSoNgayCong - nhanVienLuong.Tong_NLD;

        //                    }



        //                    else
        //                    {
        //                        nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD;
        //                        nhanVienLuong.TongNhan_NLD = nhanVienLuong.LuongTheoSoNgayCong;
        //                        // nhanVienLuong.Tong_NLD = 0;
        //                    }
        //                }

        //                nhanVienLuong.TangCa = nhanVienLuong.TangCaNgayCN + nhanVienLuong.TangCaNgayT7 + nhanVienLuong.TangCaNgayThuong + nhanVienLuong.TangCaNgayLe;
        //                double TyLeHuongLuong = HopDongNhanVien.TyLeHuongLuong;
        //                if (TyLeHuongLuong == 0)
        //                    TyLeHuongLuong = 100;
        //                nhanVienLuong.TongNhan_NLD = (nhanVienLuong.TongNhan_NLD) * (TyLeHuongLuong / 100);
        //                nhanVienLuong.LuongTangCa = (nhanVienLuong.LuongTangCa) * (TyLeHuongLuong / 100);
        //                nhanVienLuong.TangCaNgayLe = (nhanVienLuong.TangCaNgayLe) * (TyLeHuongLuong / 100);
        //                nhanVienLuong.LuongNghiPhep = (nhanVienLuong.LuongNghiPhep) * (TyLeHuongLuong / 100);
        //                nhanVienLuong.PhuCapTinhThue = (nhanVienLuong.PhuCapTinhThue) * (TyLeHuongLuong / 100);
        //                nhanVienLuong.PhuCapKhongTinhThue = (nhanVienLuong.PhuCapKhongTinhThue) * (TyLeHuongLuong / 100);
        //                nhanVienLuong.NSD_TongBH = nhanVienLuong.NSD_BHYT + nhanVienLuong.NSD_BHXH + nhanVienLuong.NSD_BHTN;
        //                nhanVienLuong.NLD_TongBH = nhanVienLuong.NSD_BHTN + nhanVienLuong.NSD_BHXH + nhanVienLuong.NSD_BHYT;
        //                var listKyLuat = listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKyLuat.ToString() && r.TyLe > 0).ToList();
        //                foreach (var itemKL in listKyLuat)
        //                {
        //                    nhanVienLuong.KyLuat = nhanVienLuong.KyLuat + (nhanVienLuong.MucLuong * itemKL.TyLe) / 100;
        //                }
        //                nhanVienLuong.KyLuat = nhanVienLuong.KyLuat + listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKyLuat.ToString()).Sum(r => r.SoTien);


        //                nhanVienLuong.TongNhanChuaThueChuaGiamTru = (nhanVienLuong.TongNhan_NLD + nhanVienLuong.PhuCapTinhThue + nhanVienLuong.LuongTangCa + nhanVienLuong.TangCaNgayLe + nhanVienLuong.LuongNghiPhep) - nhanVienLuong.KyLuat + nhanVienLuong.KhenThuong;
        //                double SoTienDuocGiamTru = lstNguoiPhuThuoc.Count() * 4400000;
        //                nhanVienLuong.TongGiamTruGiaCanh = SoTienDuocGiamTru;
        //                nhanVienLuong.SoNguoiDuocGiamTru = lstNguoiPhuThuoc.Count();
        //                nhanVienLuong.TongGiamTruTNNCN = SoTienDuocGiamTru + SoGiamTruCaNhan;
        //                nhanVienLuong.TongNhanChuaThueDaGiamTru = nhanVienLuong.TongNhanChuaThueChuaGiamTru - SoTienDuocGiamTru;
        //                //   đang tính chỗ này mà mãi chưa tính đc
        //                var listTTCNNhanViens = listDmCauHinhThueThuNhapCaNhans.Where(r => nhanVienLuong.TongNhanChuaThueDaGiamTru > r.MucLuong).ToList();

        //                foreach (var itemTTNCN in listTTCNNhanViens)
        //                {
        //                    if (nhanVienLuong.TongNhanChuaThueDaGiamTru > itemTTNCN.MucLuong && itemTTNCN.SoTien == 0)
        //                    {
        //                        double SoTienDongThue = nhanVienLuong.TongNhanChuaThueDaGiamTru - itemTTNCN.MucLuong;
        //                        nhanVienLuong.TongMucTNCN_NLD = nhanVienLuong.TongMucTNCN_NLD + (SoTienDongThue / 100) * itemTTNCN.TyLe;
        //                    }
        //                    else
        //                    {
        //                        if ((itemTTNCN.MucLuong + itemTTNCN.SoTien) > nhanVienLuong.TongNhanChuaThueDaGiamTru)
        //                        {
        //                            double SoTienDongThue = nhanVienLuong.TongNhanChuaThueDaGiamTru - itemTTNCN.MucLuong;
        //                            nhanVienLuong.TongMucTNCN_NLD = nhanVienLuong.TongMucTNCN_NLD + (SoTienDongThue / 100) * itemTTNCN.TyLe;
        //                        }
        //                        else
        //                        {
        //                            double SoTienDongThue = itemTTNCN.SoTien;
        //                            nhanVienLuong.TongMucTNCN_NLD = nhanVienLuong.TongMucTNCN_NLD + (SoTienDongThue / 100) * itemTTNCN.TyLe;
        //                        }

        //                    }

        //                }
        //                nhanVienLuong.TongNhan_NLD = nhanVienLuong.TongNhanChuaThueChuaGiamTru - nhanVienLuong.TongMucTNCN_NLD + nhanVienLuong.PhuCapKhongTinhThue + nhanVienLuong.PhuCapLamNgoaiGio;
        //                listLuongNhanVien.Add(nhanVienLuong);


        //            }
        //            else
        //            {
        //                List<NhanVienLuongViewModel> lstNhanVienNhieuMucLuong = new List<NhanVienLuongViewModel>();
        //                var listChamCongNhanViens = lstChamCongNhanViens.Where(r => r.IdNhanVien == item.IdNhanVien).ToList();
        //                LuongNhanVien = LuongNhanVien.OrderBy(r => r.NgayBatDauHieuLuc).ToList();
        //                for (int i = 0; i < LuongNhanVien.Count(); i++)
        //                {

        //                    //} (var itemLuong in LuongNhanVien)
        //                    //{
        //                    var itemLuong = LuongNhanVien[i];
        //                    double SoNgayLamViec = 0;
        //                    var BoPhans = ListBoPhanEOS.Where(r => r.Id == itemLuong.IdBoPhanEOS).FirstOrDefault();
        //                    if (BoPhans != null)
        //                        SoNgayLamViec = BoPhans.SoNgayLamViec;
        //                    else
        //                    {
        //                        SoNgayLamViec = 26;
        //                    }
        //                    itemLuong.NgayBatDauHieuLuc = itemLuong.NgayBatDauHieuLuc.Date;
        //                    if (itemLuong.NgayHetHieuLuc.Year < 2000)
        //                    {
        //                        if (i + 1 < LuongNhanVien.Count)
        //                        {
        //                            itemLuong.NgayHetHieuLuc = LuongNhanVien[i + 1].NgayBatDauHieuLuc.AddDays(-1).Date;
        //                            if (itemLuong.NgayHetHieuLuc.Year < 2000)
        //                                itemLuong.NgayHetHieuLuc = NgayKetThucMax.AddDays(10);
        //                        }
        //                        else
        //                            itemLuong.NgayHetHieuLuc = NgayKetThucMax.AddDays(10);
        //                    }


        //                    string sSqlPhuCapNhanVien = "Select * from HRM_Luong_PhuCap_NhanVien where IdLuong_NhanVien='" + itemLuong.IdGuid + "'";
        //                    List<HRM_Luong_PhuCap_NhanVien> listPhuCapNhanVien = db.HRM_Luong_PhuCap_NhanVien.SqlQuery(sSqlPhuCapNhanVien).ToList();
        //                    foreach (var itemPC in listPhuCapNhanVien)
        //                    {
        //                        var PhuCap = dmPhuCaps.FirstOrDefault(r => r.IdGuid == itemPC.IddmLoaiPhuCap);
        //                        if (PhuCap != null)
        //                            itemPC.isTinhThueTNCN = PhuCap.isTinhThueTNCN;

        //                    }

        //                    var listChamCongNhanVien = listChamCongNhanViens.Where(r => r.NgayCong >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.NgayCong).ToList();
        //                    if (listChamCongNhanVien.Count == 0)
        //                        continue;
        //                    var NgayCongNhanVien = (from a in listChamCongNhanVien
        //                                            group a by a.IdNhanVien into b
        //                                            select new
        //                                            {
        //                                                IdNhanVien = b.Key,
        //                                                SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NP").Count() + b.Where(r => r.MaChamCongChieu == "NP").Count()) / 2,
        //                                                SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //                                                SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //                                                SoNgayLamViecDuThoiGian = b.Where(r => r.MaChamCongChieu == "DL" && r.MaChamCongChieu == "DL").Count(),

        //                                            }).FirstOrDefault();

        //                    NhanVienLuongViewModel nhanVienLuong = new NhanVienLuongViewModel();
        //                    nhanVienLuong.IdNhanVien = item.IdNhanVien;
        //                    nhanVienLuong.MaNhanVien = nhanVien.MaNhanVien;
        //                    nhanVienLuong.TenNhanVien = nhanVien.TenDayDu;
        //                    nhanVienLuong.NghiPhep = NgayCongNhanVien.SoNgayNghiPhep;
        //                    var TangCaNgayThuong = NhanVienLamThemGio.Where(r => !lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date) && !ListNgayT7.Contains(r.ThoiGianBatDau.Date) && !ListNgayCN.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //                    var TangCaNgayT7 = NhanVienLamThemGio.Where(r => ListNgayT7.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //                    var TangCaNgayCN = NhanVienLamThemGio.Where(r => ListNgayCN.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //                    var TangCaNgayLe = NhanVienLamThemGio.Where(r => lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //                    foreach (var itemTC in TangCaNgayT7)
        //                    {
        //                        nhanVienLuong.TangCaNgayT7 = nhanVienLuong.TangCaNgayT7 + itemTC.ThoiGian;//
        //                    }
        //                    foreach (var itemTC in TangCaNgayCN)
        //                    {
        //                        nhanVienLuong.TangCaNgayCN = nhanVienLuong.TangCaNgayT7 + itemTC.ThoiGian;// ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                    }
        //                    foreach (var itemTC in TangCaNgayLe)
        //                    {
        //                        nhanVienLuong.TangCaNgayLe = nhanVienLuong.TangCaNgayT7 + itemTC.ThoiGian;// ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                    }
        //                    foreach (var itemTC in TangCaNgayThuong)
        //                    {
        //                        nhanVienLuong.TangCaNgayThuong = nhanVienLuong.TangCaNgayThuong + itemTC.ThoiGian;
        //                    }
        //                    nhanVienLuong.SoGioLamThucTe = NgayCongNhanVien.SoGioLamViec;
        //                    nhanVienLuong.SoNgayLamViecDuThoiGian = NgayCongNhanVien.SoNgayLamViecDuThoiGian;
        //                    if (NgayCongNhanVien != null)
        //                    {
        //                        nhanVienLuong.SoNgayCongThucTe = NgayCongNhanVien.SoNgayDiLam;
        //                        nhanVienLuong.NgayCongCoSo = SoNgayLamViec;
        //                        nhanVienLuong.MucLuong = LuongNhanVien[0].LuongHachToan;
        //                        List<PhuCapNhanVienViewModel> listPhuCap = new List<PhuCapNhanVienViewModel>();
        //                        foreach (var itemPC in listPhuCapNhanVien)
        //                        {
        //                            if (itemPC.isTinhThueTNCN == false)
        //                            {
        //                                if (itemPC.LoaiPhuCap == 1)
        //                                {
        //                                    nhanVienLuong.PhuCapKhongTheoCongChinh = nhanVienLuong.PhuCapKhongTheoCongChinh + itemPC.SoTien;
        //                                    nhanVienLuong.PhuCapKhongTinhThue = nhanVienLuong.PhuCapKhongTinhThue + itemPC.SoTien;
        //                                }
        //                                else if (itemPC.LoaiPhuCap == 2)
        //                                {
        //                                    nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                                    nhanVienLuong.PhuCapKhongTinhThue = nhanVienLuong.PhuCapKhongTinhThue + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                                }
        //                                else if (itemPC.LoaiPhuCap == 3)
        //                                {
        //                                    nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                    nhanVienLuong.PhuCapKhongTinhThue = nhanVienLuong.PhuCapKhongTinhThue + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (itemPC.LoaiPhuCap == 1)
        //                                {
        //                                    nhanVienLuong.PhuCapKhongTheoCongChinh = nhanVienLuong.PhuCapKhongTheoCongChinh + itemPC.SoTien;
        //                                    nhanVienLuong.PhuCapTinhThue = nhanVienLuong.PhuCapTinhThue + itemPC.SoTien;
        //                                }
        //                                else if (itemPC.LoaiPhuCap == 2)
        //                                {
        //                                    nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                                    nhanVienLuong.PhuCapTinhThue = nhanVienLuong.PhuCapTinhThue + (itemPC.SoTien * nhanVienLuong.SoNgayLamViecDuThoiGian);
        //                                }
        //                                else if (itemPC.LoaiPhuCap == 3)
        //                                {
        //                                    nhanVienLuong.PhuCapTheoCongChinh = nhanVienLuong.PhuCapTheoCongChinh + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                    nhanVienLuong.PhuCapTinhThue = nhanVienLuong.PhuCapTinhThue + (itemPC.SoTien * nhanVienLuong.SoGioLamThucTe);
        //                                }
        //                            }

        //                        }
        //                        if (itemLuong.LoaiLuong == 1)
        //                        {
        //                            nhanVienLuong.LuongTheoSoNgayCong = (nhanVienLuong.MucLuong * nhanVienLuong.SoNgayCongThucTe) / SoNgayLamViec;
        //                            double LuongTheoGio = nhanVienLuong.MucLuong / (SoNgayLamViec * SoGLV);
        //                            nhanVienLuong.LuongTangCa = LuongTheoGio * nhanVienLuong.TangCaNgayT7 * 1.5 + LuongTheoGio * nhanVienLuong.TangCaNgayCN * 2 + nhanVienLuong.TangCaNgayThuong * LuongTheoGio;
        //                            nhanVienLuong.TangCaNgayLe = nhanVienLuong.TangCaNgayLe * 3;
        //                            nhanVienLuong.NSD_BHXH = (nhanVienLuong.LuongTheoSoNgayCong * 17.5) / 100;
        //                            nhanVienLuong.NSD_BHYT = (nhanVienLuong.MucLuong * 3) / 100;
        //                            nhanVienLuong.NSD_BHTN = (nhanVienLuong.MucLuong * 1) / 100;
        //                            nhanVienLuong.LuongNghiPhep = (nhanVienLuong.MucLuong * nhanVienLuong.NghiPhep) / SoNgayLamViec;
        //                            // Nguoi Lao đông
        //                            nhanVienLuong.NLD_BHXH = (nhanVienLuong.LuongTheoSoNgayCong * 8) / 100;
        //                            nhanVienLuong.NLD_BHYT = (nhanVienLuong.LuongTheoSoNgayCong * 1.5) / 100;
        //                            nhanVienLuong.NLD_BHTN = (nhanVienLuong.LuongTheoSoNgayCong * 1) / 100;
        //                            nhanVienLuong.NSD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) / 100;
        //                            nhanVienLuong.NLD_PhiCongDoan = (nhanVienLuong.MucLuong * 1) / 100;

        //                        }
        //                        else if (itemLuong.LoaiLuong == 2)
        //                        {
        //                            double LuongTheoGio = nhanVienLuong.MucLuong / SoGLV;
        //                            nhanVienLuong.LuongTangCa = LuongTheoGio * nhanVienLuong.TangCaNgayT7 * 1.5 + LuongTheoGio * nhanVienLuong.TangCaNgayCN * 2 + nhanVienLuong.TangCaNgayThuong * LuongTheoGio;
        //                            nhanVienLuong.TangCaNgayLe = LuongTheoGio * nhanVienLuong.TangCaNgayLe * 3;
        //                            nhanVienLuong.LuongTheoSoNgayCong = (nhanVienLuong.MucLuong * nhanVienLuong.SoNgayCongThucTe);
        //                            nhanVienLuong.NSD_BHXH = (nhanVienLuong.LuongTheoSoNgayCong * 17.5) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.NSD_BHYT = (nhanVienLuong.LuongTheoSoNgayCong * 3) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.NSD_BHTN = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.LuongNghiPhep = (nhanVienLuong.MucLuong * nhanVienLuong.NghiPhep);
        //                            // Nguoi Lao đông
        //                            nhanVienLuong.NLD_BHXH = (nhanVienLuong.LuongTheoSoNgayCong * 8) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.NLD_BHYT = (nhanVienLuong.LuongTheoSoNgayCong * 1.5) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.NLD_BHTN = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.NSD_PhiCongDoan = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                            nhanVienLuong.NLD_PhiCongDoan = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoNgayCongThucTe / 100;
        //                        }

        //                        else if (itemLuong.LoaiLuong == 3)
        //                        {
        //                            nhanVienLuong.LuongTheoSoNgayCong = (nhanVienLuong.MucLuong * nhanVienLuong.SoGioLamThucTe);
        //                            //nhanVienLuong.ThoiGianLamViec = nhanVienLuong.SoGioLamThucTe;
        //                            double LuongTheoGio = nhanVienLuong.MucLuong;
        //                            nhanVienLuong.TangCaNgayLe = LuongTheoGio * nhanVienLuong.TangCaNgayLe * 3;
        //                            nhanVienLuong.LuongTangCa = LuongTheoGio * nhanVienLuong.TangCaNgayT7 * 1.5 + LuongTheoGio * nhanVienLuong.TangCaNgayCN * 2 + nhanVienLuong.TangCaNgayThuong * LuongTheoGio;
        //                            nhanVienLuong.NSD_BHXH = (nhanVienLuong.LuongTheoSoNgayCong * 17.5) * nhanVienLuong.SoGioLamThucTe / 100;
        //                            nhanVienLuong.NSD_BHYT = (nhanVienLuong.LuongTheoSoNgayCong * 3) * nhanVienLuong.SoGioLamThucTe / 100;
        //                            nhanVienLuong.NSD_BHTN = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoGioLamThucTe / 100;
        //                            nhanVienLuong.NSD_PhiCongDoan = (nhanVienLuong.LuongTheoSoNgayCong * 4) * nhanVienLuong.SoGioLamThucTe / 100;

        //                            // Nguoi Lao đông
        //                            nhanVienLuong.NLD_BHXH = (nhanVienLuong.LuongTheoSoNgayCong * 8) * nhanVienLuong.SoGioLamThucTe / 100;
        //                            nhanVienLuong.NLD_BHYT = (nhanVienLuong.LuongTheoSoNgayCong * 1.5) * nhanVienLuong.SoGioLamThucTe / 100;
        //                            nhanVienLuong.NLD_BHTN = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoGioLamThucTe / 100;
        //                            nhanVienLuong.NLD_PhiCongDoan = (nhanVienLuong.LuongTheoSoNgayCong * 1) * nhanVienLuong.SoGioLamThucTe / 100;
        //                        }
        //                        if (HopDongNhanVien.isBaoHiemThatNghiep == true)
        //                        {
        //                            nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_BHTN;
        //                            nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_BHTN;
        //                        }
        //                        else
        //                        {
        //                            nhanVienLuong.NSD_BHTN = 0;
        //                            nhanVienLuong.NLD_BHTN = 0;
        //                        }
        //                        if (HopDongNhanVien.isBaoHiemXaHoi == true)
        //                        {
        //                            nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_BHXH;
        //                            nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_BHXH;
        //                        }
        //                        else
        //                        {
        //                            nhanVienLuong.NSD_BHXH = 0;
        //                            nhanVienLuong.NLD_BHXH = 0;
        //                        }
        //                        if (HopDongNhanVien.isBaoHiemYTe == true)
        //                        {
        //                            nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_BHYT;
        //                            nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_BHYT;
        //                            //  nhanVienLuong.Tong_NSD = nhanVienLuong.NSD_BHXH + nhanVienLuong.NSD_BHYT + nhanVienLuong.NSD_BHTN - nhanVienLuong.NSD_PhiCongDoan;
        //                            //nhanVienLuong.Tong_NLD = nhanVienLuong.NLD_BHXH + nhanVienLuong.NLD_BHYT + nhanVienLuong.NLD_BHTN - nhanVienLuong.NLD_PhiCongDoan;
        //                        }
        //                        else
        //                        {
        //                            nhanVienLuong.NSD_BHYT = 0;
        //                            nhanVienLuong.NLD_BHYT = 0;
        //                        }
        //                        if (HopDongNhanVien.isCongDoan == true)
        //                        {
        //                            nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.NSD_PhiCongDoan;
        //                            nhanVienLuong.Tong_NLD = nhanVienLuong.Tong_NLD + nhanVienLuong.NLD_PhiCongDoan;
        //                        }
        //                        else
        //                        {
        //                            nhanVienLuong.NLD_PhiCongDoan = 0;
        //                            nhanVienLuong.NSD_PhiCongDoan = 0;
        //                        }
        //                        if (itemLuong.KieuLuong == 0)
        //                            nhanVienLuong.TongNhan_NLD = nhanVienLuong.LuongTheoSoNgayCong - nhanVienLuong.Tong_NLD;
        //                        else
        //                        {
        //                            nhanVienLuong.Tong_NSD = nhanVienLuong.Tong_NSD + nhanVienLuong.Tong_NLD;
        //                            nhanVienLuong.TongNhan_NLD = nhanVienLuong.LuongTheoSoNgayCong;
        //                            nhanVienLuong.Tong_NLD = 0;
        //                        }
        //                        nhanVienLuong.KieuLuong = itemLuong.KieuLuong;


        //                    }
        //                    nhanVienLuong.TangCa = nhanVienLuong.TangCaNgayCN + nhanVienLuong.TangCaNgayLe + nhanVienLuong.TangCaNgayT7;

        //                    lstNhanVienNhieuMucLuong.Add(nhanVienLuong);
        //                }

        //                NhanVienLuongViewModel nhanVienLuongView = new NhanVienLuongViewModel
        //                {
        //                    IdNhanVien = lstNhanVienNhieuMucLuong[0].IdNhanVien,

        //                    TenNhanVien = lstNhanVienNhieuMucLuong[0].TenNhanVien,
        //                    LuongTangCa = lstNhanVienNhieuMucLuong.Sum(r => r.LuongTangCa),
        //                    SoNgayCongThucTe = lstNhanVienNhieuMucLuong.Sum(r => r.SoNgayCongThucTe),
        //                    SoNgayLamViecDuThoiGian = lstNhanVienNhieuMucLuong.Sum(r => r.SoNgayLamViecDuThoiGian),
        //                    SoGioLamThucTe = lstNhanVienNhieuMucLuong.Sum(r => r.SoGioLamThucTe),
        //                    SoNguoiDuocGiamTru = 0,
        //                    KieuLuong = lstNhanVienNhieuMucLuong[0].KieuLuong,
        //                    LuongTheoSoNgayCong = lstNhanVienNhieuMucLuong.Sum(r => r.LuongTheoSoNgayCong),
        //                    NgayCongCoSo = lstNhanVienNhieuMucLuong[0].NgayCongCoSo,
        //                    NSD_BHTN = lstNhanVienNhieuMucLuong.Sum(r => r.NSD_BHTN),
        //                    NSD_BHXH = lstNhanVienNhieuMucLuong.Sum(r => r.NSD_BHXH),
        //                    NSD_BHYT = lstNhanVienNhieuMucLuong.Sum(r => r.NSD_BHYT),
        //                    NSD_TongBH = lstNhanVienNhieuMucLuong.Sum(r => r.NSD_BHYT) + lstNhanVienNhieuMucLuong.Sum(r => r.NSD_BHXH) + lstNhanVienNhieuMucLuong.Sum(r => r.NSD_BHTN),
        //                    NSD_PhiCongDoan = lstNhanVienNhieuMucLuong.Sum(r => r.NSD_PhiCongDoan),
        //                    TongMucTNCN_NSD = lstNhanVienNhieuMucLuong.Sum(r => r.TongMucTNCN_NSD),
        //                    Tong_NSD = lstNhanVienNhieuMucLuong.Sum(r => r.Tong_NSD),
        //                    MaNhanVien = lstNhanVienNhieuMucLuong[0].MaNhanVien,
        //                    MucLuong = lstNhanVienNhieuMucLuong.Sum(r => r.MucLuong) / (lstNhanVienNhieuMucLuong.Count()),
        //                    TangCa = lstNhanVienNhieuMucLuong.Sum(r => r.TangCa),
        //                    TangCaNgayThuong = lstNhanVienNhieuMucLuong.Sum(r => r.TangCaNgayThuong),
        //                    TangCaNgayCN = lstNhanVienNhieuMucLuong.Sum(r => r.TangCaNgayCN),
        //                    TangCaNgayT7 = lstNhanVienNhieuMucLuong.Sum(r => r.TangCaNgayT7),
        //                    TangCaNgayLe = lstNhanVienNhieuMucLuong.Sum(r => r.TangCaNgayLe),
        //                    NLD_BHXH = lstNhanVienNhieuMucLuong.Sum(r => r.NLD_BHXH),
        //                    NLD_BHTN = lstNhanVienNhieuMucLuong.Sum(r => r.NLD_BHTN),
        //                    NLD_BHYT = lstNhanVienNhieuMucLuong.Sum(r => r.NLD_BHYT),
        //                    NLD_TongBH = lstNhanVienNhieuMucLuong.Sum(r => r.NLD_BHXH) + lstNhanVienNhieuMucLuong.Sum(r => r.NLD_BHTN) + lstNhanVienNhieuMucLuong.Sum(r => r.NLD_BHYT),
        //                    TroCapNgoai = NhanVienLamThemGio.Sum(r => r.PhuCap),
        //                    NLD_PhiCongDoan = lstNhanVienNhieuMucLuong.Sum(r => r.NLD_PhiCongDoan),
        //                    PhuCapKhongTheoCongChinh = lstNhanVienNhieuMucLuong.Sum(r => r.PhuCapKhongTheoCongChinh),
        //                    PhuCapTheoCongChinh = lstNhanVienNhieuMucLuong.Sum(r => r.PhuCapTheoCongChinh),
        //                    PhuCapTinhThue = lstNhanVienNhieuMucLuong.Sum(r => r.PhuCapTinhThue),
        //                    PhuCapKhongTinhThue = lstNhanVienNhieuMucLuong.Sum(r => r.PhuCapKhongTinhThue),
        //                    TongNhan_NLD = lstNhanVienNhieuMucLuong.Sum(r => r.TongNhan_NLD),
        //                    TongGiamTruTNNCN = 0,
        //                    ThoiGianLamViec = lstNhanVienNhieuMucLuong.Sum(r => r.ThoiGianLamViec),
        //                    TongMucTNCN_NLD = lstNhanVienNhieuMucLuong.Sum(r => r.TongMucTNCN_NLD),
        //                    TongNhanChuaThueChuaGiamTru = lstNhanVienNhieuMucLuong.Sum(r => r.TongNhanChuaThueChuaGiamTru),
        //                    Tong_NLD = lstNhanVienNhieuMucLuong.Sum(r => r.Tong_NLD),
        //                    TongNhanChuaThueDaGiamTru = lstNhanVienNhieuMucLuong.Sum(r => r.TongNhanChuaThueDaGiamTru),
        //                    NghiPhep = lstNhanVienNhieuMucLuong.Sum(r => r.NghiPhep),
        //                    LuongNgayLe = lstNhanVienNhieuMucLuong.Sum(r => r.LuongNgayLe),
        //                    TongCong = lstNhanVienNhieuMucLuong.Sum(r => r.TongCong),
        //                };
        //                double thoigianMuon = Math.Round(SoPhut / 60, 2);
        //                nhanVienLuongView.SoGioLamThucTe = nhanVienLuongView.SoGioLamThucTe - thoigianMuon;
        //                //  RaSomSoPhut = SoPhutVeSom,
        //                //VaoTreSoPhut = SoPhutDiMuon,
        //                var listKyLuat = listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKyLuat.ToString() && r.TyLe > 0).ToList();
        //                foreach (var itemKL in listKyLuat)
        //                {
        //                    nhanVienLuongView.KyLuat = nhanVienLuongView.KyLuat + (nhanVienLuongView.MucLuong * itemKL.TyLe) / 100;
        //                }
        //                nhanVienLuongView.KyLuat = nhanVienLuongView.KyLuat + listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKyLuat.ToString()).Sum(r => r.SoTien);

        //                //  nhanVienLuongView.KyLuat = listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKyLuat.ToString()).Sum(r => r.SoTien);
        //                nhanVienLuongView.KhenThuong = listKhenthuongKyLuat.Where(r => r.IdNhanVien == item.IdNhanVien && r.TabName == DefineData.eTable.HRM_QuyetDinhKhenThuong.ToString()).Sum(r => r.SoTien);
        //                nhanVienLuongView.TongNhanChuaThueChuaGiamTru = nhanVienLuongView.TongNhan_NLD + nhanVienLuongView.PhuCapTinhThue + nhanVienLuongView.LuongTangCa + nhanVienLuongView.KhenThuong - nhanVienLuongView.KyLuat;
        //                double SoTienDuocGiamTru = lstNguoiPhuThuoc.Count() * 4400000;

        //                double TyLeHuongLuong = HopDongNhanVien.TyLeHuongLuong;
        //                if (TyLeHuongLuong == 0)
        //                    TyLeHuongLuong = 100;
        //                nhanVienLuongView.TongNhan_NLD = (nhanVienLuongView.TongNhan_NLD) * (TyLeHuongLuong / 100);
        //                nhanVienLuongView.LuongTangCa = (nhanVienLuongView.LuongTangCa) * (TyLeHuongLuong / 100);
        //                nhanVienLuongView.LuongNghiPhep = (nhanVienLuongView.LuongNghiPhep) * (TyLeHuongLuong / 100);
        //                nhanVienLuongView.PhuCapTinhThue = (nhanVienLuongView.PhuCapTinhThue) * (TyLeHuongLuong / 100);
        //                nhanVienLuongView.PhuCapKhongTinhThue = (nhanVienLuongView.PhuCapKhongTinhThue) * (TyLeHuongLuong / 100);


        //                nhanVienLuongView.TongGiamTruGiaCanh = SoTienDuocGiamTru;
        //                nhanVienLuongView.SoNguoiDuocGiamTru = lstNguoiPhuThuoc.Count();
        //                nhanVienLuongView.TongGiamTruTNNCN = SoTienDuocGiamTru + SoGiamTruCaNhan;
        //                nhanVienLuongView.TongNhanChuaThueDaGiamTru = nhanVienLuongView.TongNhanChuaThueChuaGiamTru - SoTienDuocGiamTru;
        //                nhanVienLuongView.TongNhanChuaThueChuaGiamTru = nhanVienLuongView.TongNhan_NLD + nhanVienLuongView.PhuCapTinhThue + nhanVienLuongView.LuongTangCa + nhanVienLuongView.TangCaNgayLe + nhanVienLuongView.LuongNghiPhep;

        //                var listTTCNNhanViens = listDmCauHinhThueThuNhapCaNhans.Where(r => nhanVienLuongView.TongNhanChuaThueDaGiamTru > r.MucLuong).ToList();

        //                foreach (var itemTTNCN in listTTCNNhanViens)
        //                {
        //                    if (nhanVienLuongView.TongNhanChuaThueDaGiamTru > itemTTNCN.MucLuong && itemTTNCN.SoTien == 0)
        //                    {
        //                        double SoTienDongThue = nhanVienLuongView.TongNhanChuaThueDaGiamTru - itemTTNCN.MucLuong;
        //                        nhanVienLuongView.TongMucTNCN_NLD = nhanVienLuongView.TongMucTNCN_NLD + (SoTienDongThue / 100) * itemTTNCN.TyLe;
        //                    }
        //                    else
        //                    {
        //                        if ((itemTTNCN.MucLuong + itemTTNCN.SoTien) > nhanVienLuongView.TongNhanChuaThueDaGiamTru && itemTTNCN.MucLuong > nhanVienLuongView.TongNhanChuaThueDaGiamTru)
        //                        {
        //                            double SoTienDongThue = nhanVienLuongView.TongNhanChuaThueDaGiamTru - itemTTNCN.MucLuong;
        //                            nhanVienLuongView.TongMucTNCN_NLD = nhanVienLuongView.TongMucTNCN_NLD + (SoTienDongThue / 100) * itemTTNCN.TyLe;
        //                        }
        //                        else
        //                        {
        //                            double SoTienDongThue = itemTTNCN.SoTien;
        //                            nhanVienLuongView.TongMucTNCN_NLD = nhanVienLuongView.TongMucTNCN_NLD + (SoTienDongThue / 100) * itemTTNCN.TyLe;
        //                        }

        //                    }

        //                }
        //                nhanVienLuongView.TongNhan_NLD = nhanVienLuongView.TongNhanChuaThueChuaGiamTru - nhanVienLuongView.TongMucTNCN_NLD + nhanVienLuongView.PhuCapKhongTinhThue;
        //                listLuongNhanVien.Add(nhanVienLuongView);

        //            }
        //        }
        //        listLuongNhanVien = listLuongNhanVien.OrderBy(r => r.MaNhanVien).ToList();

        //        return listLuongNhanVien;

        //    }

        //    public dynamic SetQuyTrinhBangLuong(HRM_BangLuong model, ApplicationDbContext db)
        //    {

        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                bool check = FuncUpdateBangLuong(model, db);
        //                if (check == false)
        //                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");

        //                if (string.IsNullOrEmpty(model.IdTrangThai))
        //                {
        //                    CallWebService _CallWebService = new CallWebService();
        //                    var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHLUONG.ToString(), model.IdGuid);

        //                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
        //                    {
        //                        dbContextTransaction.Rollback();
        //                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
        //                    }

        //                    model.IdTrangThai = trangThai.IdTrangThai;
        //                    model.TenTrangThai = trangThai.TenTrangThai;
        //                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //                    db.SaveChanges();
        //                }
        //                dbContextTransaction.Commit();

        //                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);

        //            }
        //            catch (Exception ex)
        //            {
        //                dbContextTransaction.Rollback();
        //                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển " + ex.Message);
        //            }

        //        }
        //    }



        //    public dynamic DeleteQuyTrinhBangLuong(string Id, ApplicationDbContext db)
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                var BangLuong = db.HRM_BangLuong.Find(Id);
        //                if (BangLuong == null)
        //                {
        //                    dbContextTransaction.Rollback();
        //                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy bản ghi yêu cầu xóa");
        //                }

        //                var chitietLuongOld = db.HRM_BangLuong_ChiTietNhanVien.Where(r => r.IdBangLuong == Id).ToList();
        //                db.HRM_BangLuong_ChiTietNhanVien.RemoveRange(chitietLuongOld);
        //                var chitietBangChamCongOld = db.HRM_Link_BangLuong_ChamCong.Where(r => r.IdBangLuong == Id).ToList();
        //                db.HRM_Link_BangLuong_ChamCong.RemoveRange(chitietBangChamCongOld);
        //                db.HRM_BangLuong.Remove(BangLuong);
        //                db.SaveChanges();
        //                dbContextTransaction.Commit();
        //                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa bản ghi thành công");
        //            }
        //            catch
        //            {
        //                dbContextTransaction.Rollback();
        //                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Xóa bản ghi không thành công");
        //            }
        //        }

        //    }
        //    public dynamic ChuyenTiepQuyTrinhBangLuong(HRM_BangLuong model, ApplicationDbContext db)
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                bool check = FuncUpdateBangLuong(model, db);
        //                if (check == false)
        //                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");

        //                var trangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.QUYTRINHLUONG.ToString(), model.IdTrangThai, model.IdGuid);
        //                if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
        //                {
        //                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu chuyển tiếp");
        //                }

        //                model.IdTrangThai = trangThai.IdTrangThai;
        //                model.TenTrangThai = trangThai.TenTrangThai;
        //                db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //                db.SaveChanges();
        //                dbContextTransaction.Commit();
        //                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
        //            }

        //            catch (Exception ex)
        //            {
        //                dbContextTransaction.Rollback();
        //                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Xóa bản ghi không thành công " + ex.Message);
        //            }

        //        }
        //    }

        //    public dynamic KhongDuyetQuyTrinhBangLuong(HRM_BangLuong model, ApplicationDbContext db)
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {

        //                bool check = FuncUpdateBangLuong(model, db);
        //                if (check == false)
        //                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");

        //                var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHLUONG.ToString(), model.IdTrangThai, model.IdGuid);
        //                if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
        //                {
        //                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
        //                }

        //                model.IdTrangThai = trangThai.IdTrangThai;
        //                model.TenTrangThai = trangThai.TenTrangThai;
        //                db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        //                db.SaveChanges();



        //                dbContextTransaction.Commit();

        //                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);

        //            }
        //            catch
        //            {
        //                dbContextTransaction.Rollback();
        //                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //            }
        //        }
        //    }
        //    public bool FuncUpdateBangLuong(HRM_BangLuong model, ApplicationDbContext db)
        //    {
        //        try
        //        {
        //            model.ThoiGian = dungchung.UnixTimeStampToDateTime(model.ThoiGianUnix);
        //            model.NgayTao = dungchung.UnixTimeStampToDateTime(model.NgayTaoUnix);
        //            if (string.IsNullOrEmpty(model.IdGuid))
        //            {
        //                model.IdGuid = Guid.NewGuid().ToString();
        //                db.HRM_BangLuong.Add(model);
        //            }
        //            else
        //            {

        //                db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //                var chitietLuongOld = db.HRM_BangLuong_ChiTietNhanVien.Where(r => r.IdBangLuong == model.IdGuid).ToList();
        //                db.HRM_BangLuong_ChiTietNhanVien.RemoveRange(chitietLuongOld);
        //                var chitietBangChamCongOld = db.HRM_Link_BangLuong_ChamCong.Where(r => r.IdBangLuong == model.IdGuid).ToList();
        //                db.HRM_Link_BangLuong_ChamCong.RemoveRange(chitietBangChamCongOld);
        //                var chitietLuongDAOld = db.HRM_BangLuong_ChiTietNhanVienDA.Where(r => r.IdBangLuong == model.IdGuid).ToList();
        //                var listIdChitietDaiAn = chitietLuongDAOld.Select(r => r.IdGuid).ToList();
        //                db.HRM_BangLuong_ChiTietNhanVienDA.RemoveRange(chitietLuongDAOld);
        //                var chitietPhuCapDAOld = db.HRM_BangLuong_PhuCap.Where(r => listIdChitietDaiAn.Contains( r.IdLuongChiTiet)).ToList();
        //                db.HRM_BangLuong_PhuCap.RemoveRange(chitietPhuCapDAOld);
        //            }
        //            if (model.listBangChamCong != null)
        //            {
        //                foreach (var item in model.listBangChamCong)
        //                {
        //                    item.IdGuid = Guid.NewGuid().ToString();
        //                    item.IdBangLuong = model.IdGuid;

        //                }
        //                db.HRM_Link_BangLuong_ChamCong.AddRange(model.listBangChamCong);
        //            }
        //            if (model.listLuongChiTiet != null)
        //            {
        //                foreach (var item in model.listLuongChiTiet)
        //                {
        //                    item.IdGuid = Guid.NewGuid().ToString();
        //                    item.IdBangLuong = model.IdGuid;

        //                }
        //                db.HRM_BangLuong_ChiTietNhanVien.AddRange(model.listLuongChiTiet);
        //            }
        //            if (model.listLuongChiTietDaiAn != null)
        //            {
        //                List<HRM_BangLuong_PhuCap> listPhuCap = new List<HRM_BangLuong_PhuCap>();
        //                foreach (var item in model.listLuongChiTietDaiAn)
        //                {
        //                    item.IdGuid = Guid.NewGuid().ToString();
        //                    item.IdBangLuong = model.IdGuid;
        //                    if (item.listPhuCap != null)
        //                    {
        //                        foreach (var itemPC in item.listPhuCap)
        //                        {
        //                            itemPC.IdGuid = Guid.NewGuid().ToString();
        //                            itemPC.IdLuongChiTiet = item.IdGuid;
        //                            listPhuCap.Add(itemPC);
        //                        }
        //                    }
        //                }
        //                db.HRM_BangLuong_PhuCap.AddRange(listPhuCap);
        //                db.HRM_BangLuong_ChiTietNhanVienDA.AddRange(model.listLuongChiTietDaiAn);
        //            }
        //            if (model.listFileDinhKem != null)
        //            {
        //                foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
        //                {
        //                    SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_BangLuong, model.IdGuid);
        //                }
        //            }
        //            db.SaveChanges();
        //            return true;
        //        }
        //        catch (Exception)
        //        {

        //            return false;
        //        }
        //    }
        //    public dynamic ExportBangLuongChiTiet(string Id, ApplicationDbContext db)
        //    {
        //        var model = db.HRM_BangLuong.Find(Id);
        //        if (model == null)
        //        {

        //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy bản ghi yêu cầu xóa");
        //        }
        //        model.ThoiGianUnix = dungchung.DateTimeToUnixTimeStamp(model.ThoiGian);
        //        model.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTao);
        //        var chitietLuongOld = db.HRM_BangLuong_ChiTietNhanVien.Where(r => r.IdBangLuong == Id).OrderBy(r => r.MaNhanVien).ToList();
        //        string sFile = "BANGLUONG.xlsx";

        //        string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
        //        DateTime dt = DateTime.Now;
        //        string TenCongTys = _CallWebService.GetBoPhan(db, model.IdCongTyEOS).TenBoPhan;


        //        Stream s = File.OpenRead(sFileName);
        //        ExcelPackage package = new ExcelPackage(s);
        //        ExcelWorksheet sheet1 = package.Workbook.Worksheets[1];
        //        sheet1.Cells[2, 1].Value = "Công ty: " + TenCongTys;
        //        string KyLuong = "";
        //        var listKyLuong = (from a in db.HRM_Link_BangLuong_ChamCong
        //                           join b in db.HRM_ChamCong_Detail on a.IdBangChamCong equals b.IdGuid
        //                           join c in db.HRM_DM_DotChamCongThang on b.IdDanhMucDotChamCongThang equals c.IdGuid
        //                           select c).ToList();
        //        foreach (var item in listKyLuong)
        //        {
        //            if (KyLuong == "")
        //                KyLuong = item.Ten;
        //            else
        //                KyLuong = KyLuong + " - " + item.Ten;
        //        }
        //        KyLuong = KyLuong + " / " + model.ThoiGian.ToString("MM/yyyy");
        //        sheet1.Cells[3, 1].Value = KyLuong;
        //        s.Close();
        //        int nRow = 9;
        //        int STT = 1;
        //        foreach (var item in chitietLuongOld)
        //        {
        //            sheet1.Cells[nRow, 1].Value = STT;
        //            sheet1.Cells[nRow, 2].Value = item.MaNhanVien;
        //            sheet1.Cells[nRow, 3].Value = item.TenNhanVien;
        //            sheet1.Cells[nRow, 4].Value = item.SoGioLamThucTe;
        //            sheet1.Cells[nRow, 5].Value = item.NgayCongCoSo;
        //            sheet1.Cells[nRow, 6].Value = item.MucLuong;
        //            sheet1.Cells[nRow, 7].Value = item.SoNgayCongThucTe;
        //            sheet1.Cells[nRow, 8].Value = item.TangCa;
        //            sheet1.Cells[nRow, 9].Value = item.NghiLe;
        //            sheet1.Cells[nRow, 10].Value = item.NghiPhep;
        //            sheet1.Cells[nRow, 11].Value = item.NghiBu;
        //            sheet1.Cells[nRow, 13].Value = item.LuongTheoSoNgayCong;
        //            sheet1.Cells[nRow, 14].Value = item.LuongTangCa;
        //            sheet1.Cells[nRow, 15].Value = item.LuongNgayLe;
        //            sheet1.Cells[nRow, 16].Value = item.LuongNghiPhep;
        //            sheet1.Cells[nRow, 17].Value = item.LuongNghiBu;
        //            sheet1.Cells[nRow, 18].Value = item.PhuCapTheoCongChinh;
        //            sheet1.Cells[nRow, 19].Value = item.PhuCapKhongTheoCongChinh;
        //            sheet1.Cells[nRow, 20].Value = item.KhenThuong;
        //            sheet1.Cells[nRow, 21].Value = item.KyLuat;
        //            sheet1.Cells[nRow, 22].Value = item.TongCong;
        //            sheet1.Cells[nRow, 23].Value = item.TongGiamTruGiaCanh;
        //            sheet1.Cells[nRow, 24].Value = item.TongMucTNCN_NLD;
        //            sheet1.Cells[nRow, 25].Value = 0;
        //            sheet1.Cells[nRow, 26].Value = item.TongGiamTruTNNCN;
        //            sheet1.Cells[nRow, 27].Value = item.NSD_BHXH;
        //            sheet1.Cells[nRow, 28].Value = item.NSD_BHYT;
        //            sheet1.Cells[nRow, 29].Value = item.NSD_BHTN;
        //            sheet1.Cells[nRow, 30].Value = item.NSD_PhiCongDoan;
        //            sheet1.Cells[nRow, 31].Value = item.Tong_NSD;
        //            sheet1.Cells[nRow, 32].Value = item.NLD_BHXH;
        //            sheet1.Cells[nRow, 33].Value = item.NLD_BHYT;
        //            sheet1.Cells[nRow, 34].Value = item.NLD_BHTN;
        //            sheet1.Cells[nRow, 35].Value = item.NLD_PhiCongDoan;
        //            sheet1.Cells[nRow, 36].Value = item.Tong_NLD;
        //            sheet1.Cells[nRow, 37].Value = item.TongNhan_NLD;
        //            nRow++;
        //            STT++;
        //        }
        //        DrawTableExcel(sheet1, 5, nRow - 1, 1, 37);

        //        dynamic result = new DynamicObject();

        //        string name = "BANGLUONG.xlsx";
        //        var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
        //        string pathresult =PathUploads+ filenamegui;
        //        File.WriteAllBytes(pathresult, package.GetAsByteArray());

        //        HDFile newfile = new HDFile();
        //        newfile.Name = filenamegui;
        //        newfile.NameLocal = name;

        //        newfile.Url = "/uploader/api/hdfiles?filename=" + HttpUtility.UrlEncode(dungchung.Base64Encode(name))
        //                    + "&path=" + dungchung.Base64Encode(pathresult);
        //        result.file = newfile;
        //        return result;
        //    }

        //    public dynamic ExportBangLuong(string Id, ApplicationDbContext db)
        //    {
        //        var model = db.HRM_BangLuong.Find(Id);
        //        if (model == null)
        //        {

        //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy bản ghi yêu cầu xóa");
        //        }
        //        model.ThoiGianUnix = dungchung.DateTimeToUnixTimeStamp(model.ThoiGian);
        //        model.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTao);
        //        var chitietLuongOld = db.HRM_BangLuong_ChiTietNhanVien.Where(r => r.IdBangLuong == Id).OrderBy(r => r.MaNhanVien).ToList();
        //        string sFile = "Export_BangLuong.xlsx";

        //        string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
        //        DateTime dt = DateTime.Now;


        //        Stream s = File.OpenRead(sFileName);
        //        ExcelPackage package = new ExcelPackage(s);
        //        ExcelWorksheet sheet1 = package.Workbook.Worksheets[1];
        //        s.Close();
        //        int nRow = 7;

        //        var listBoPhans = _CallWebService.GetDanhSachBoPhan(db);
        //        var bangLuong = db.HRM_BangLuong.Find(Id);
        //        var CongTy = listBoPhans.Where(r => r.Id == bangLuong.IdCongTyEOS).FirstOrDefault();
        //        if (CongTy != null && CongTy.TenBoPhan != null)
        //            sheet1.Cells[3, 1].Value = "Công Ty: " + CongTy.TenBoPhan;
        //        var listIdBoPhans = chitietLuongOld.GroupBy(r => r.IdBoPhanEOS).Select(r => r.Key).ToList();
        //        foreach (var itemBP in listIdBoPhans)
        //        {
        //            var chitietLuongOldBP = chitietLuongOld.Where(r => r.IdBoPhanEOS == itemBP).ToList();
        //            var BoPhan = listBoPhans.Where(r => r.Id == itemBP).FirstOrDefault();
        //            if (BoPhan != null)
        //                sheet1.Cells[nRow, 1].Value = BoPhan.TenBoPhan;
        //            sheet1.Cells[nRow, 1, nRow, 15].Merge = true;
        //            sheet1.Cells[nRow, 1].Style.Font.Bold = true;
        //            int STT = 1;

        //            nRow++;
        //            foreach (var item in chitietLuongOldBP)
        //            {

        //                sheet1.Cells[nRow, 1].Value = STT;
        //                sheet1.Cells[nRow, 2].Value = item.TenNhanVien;
        //                string TenBoPhan = listBoPhans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
        //                sheet1.Cells[nRow, 3].Value = TenBoPhan;
        //                sheet1.Cells[nRow, 4].Value = item.SoNgayCongThucTe;
        //                sheet1.Cells[nRow, 5].Value = item.MucLuong;
        //                sheet1.Cells[nRow, 6].Value = item.PhuCapKhongTheoCongChinh;
        //                sheet1.Cells[nRow, 7].Value = item.MucLuong + item.PhuCapKhongTheoCongChinh;
        //                sheet1.Cells[nRow, 8].Value = item.PhuCapLamNgoaiGio;
        //                sheet1.Cells[nRow, 9].Value = 0;
        //                string sum10 = "=E" + nRow + "*21.5%";
        //                sheet1.Cells[nRow, 10].FormulaR1C1 = sum10;
        //                string sum11 = "=E" + nRow + "*2%";
        //                sheet1.Cells[nRow, 11].FormulaR1C1 = sum11;
        //                string sum12 = "=E" + nRow + "*10.5%";
        //                sheet1.Cells[nRow, 12].FormulaR1C1 = sum12;
        //                string sum13 = "=E" + nRow + "*1%";
        //                sheet1.Cells[nRow, 13].FormulaR1C1 = sum13;
        //                string sum14 = "=ROUND((G" + nRow + "/" + item.NgayCongCoSo + ")*D" + nRow + ",0)+H" + nRow + "+I" + nRow + "-L" + nRow + "-M" + nRow + "";

        //                sheet1.Cells[nRow, 14].FormulaR1C1 = sum14;
        //                string sum15 = "=L" + nRow + "+M" + nRow + "+N" + nRow;
        //                sheet1.Cells[nRow, 15].FormulaR1C1 = sum15;
        //                nRow++;
        //                STT++;
        //            }
        //        }

        //        DrawTableExcel(sheet1, 5, nRow - 1, 1, 15);

        //        dynamic result = new DynamicObject();

        //        string name = "BANGLUONG.xlsx";
        //        var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
        //        string pathresult =PathUploads+ filenamegui;
        //        File.WriteAllBytes(pathresult, package.GetAsByteArray());

        //        HDFile newfile = new HDFile();
        //        newfile.Name = filenamegui;
        //        newfile.NameLocal = name;

        //        newfile.Url = "/uploader/api/hdfiles?filename=" + HttpUtility.UrlEncode(dungchung.Base64Encode(name))
        //                    + "&path=" + dungchung.Base64Encode(pathresult);
        //        result.file = newfile;
        //        return result;
        //    }
        //    // public dynamic GetDanhSachTableNam(ApplicationDbContext db)
        //    // {
        //    //     var connection =
        //    //System.Configuration.ConfigurationManager.
        //    //ConnectionStrings["HRMAPIConnection"].ConnectionString;
        //    //     List < string >lstTableName= db.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'").ToList();
        //    //     return connection;
        //    // }
        //    public dynamic GetBangLuongTheoBangChamCongDaiAn(dynamic dyn,  ApplicationDbContext db)
        //    {
        //        List<string> lstIdBangChamCong = JsonConvert.DeserializeObject<List<string>>(dyn.lstIdBangChamCong.ToString());
        //        /* loai
        //         * 1 Tháng
        //         * 2 Ngày
        //         * 3 Giờ
        //         * 4 Năm
        //         */

        //        /* 0 Lương gross
        //         * 1 net 
        //         */
        //        long IdCongTyEOS = dyn.IdCongTyEOS;
        //        string IddmDotChamCong = dyn.IddmDotChamCong;
        //        List<NhanVienLuongViewModel> lstLuongNhanViens = new List<NhanVienLuongViewModel>();
        //        string sqlIdBangChamCong = "";
        //        for (int i = 0; i < lstIdBangChamCong.Count(); i++)
        //        {
        //            if (i == 0)
        //            {
        //                sqlIdBangChamCong = "'" + lstIdBangChamCong[i].ToString() + "'";
        //            }
        //            else
        //            {
        //                sqlIdBangChamCong = sqlIdBangChamCong + "," + "'" + lstIdBangChamCong[i].ToString() + "'";
        //            }

        //        }
        //        string MaDotChamCong = db.HRM_DM_DotChamCongThang.Where(r => r.IdGuid == IddmDotChamCong).Select(r => r.Ma).FirstOrDefault();
        //        var ListBoPhanEOS = _CallWebService.GetDanhSachBoPhan(db);
        //        string NhanVienChamCong = "select * from HRM_ChamCong_NhanVien_ThoiGian_BoPhan where IdChamCongDetail in (" + sqlIdBangChamCong + ")";
        //        List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> lstChamCongNhanViens = db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.SqlQuery(NhanVienChamCong).ToList();
        //        string sqlNgayChamCongNhanVienTheoBangLuong = " select IdNhanVien,MIN(NgayCong) as NgayBatDau, MAX(NgayCong) as NgayKetThuc from HRM_ChamCong_NhanVien_ThoiGian_BoPhan where IdChamCongDetail in (" + sqlIdBangChamCong + ")" + " group by IdNhanVien ";
        //        List<NgayChamCongNhanVienModel> listNgayChamCongNhanVien = db.Database.SqlQuery<NgayChamCongNhanVienModel>(sqlNgayChamCongNhanVienTheoBangLuong).ToList();
        //        DateTime NgayBatDauMin = listNgayChamCongNhanVien.Select(r => r.NgayBatDau).Min();
        //        DateTime NgayKetThucMax = listNgayChamCongNhanVien.Select(r => r.NgayKetThuc).Max();
        //        var lstIdNhanVien = listNgayChamCongNhanVien.Select(r => r.IdNhanVien).ToList();

        //        var lstCaiDatNghiLe = smdb.CaiDatHeThongs.AsNoTracking().Where(q => q.Ma == "NgayNghiLe").ToList();
        //        List<HRM_NhanVien> listNhanViens = db.HRM_NhanVien.Where(r => lstIdNhanVien.Contains(r.IdGuid)).ToList();
        //        List<DateTime> lstNgayNghiLe = new List<DateTime>();
        //        foreach (var item in lstCaiDatNghiLe)
        //        {
        //            for (int i = 0; item.ValueEndDateTime.AddDays(1) > item.ValueStartDateTime.AddDays(i); i++)
        //            {
        //                lstNgayNghiLe.Add(item.ValueStartDateTime.AddDays(i));

        //            }
        //        }

        //        var ListNgayT7 = lstChamCongNhanViens.Where(r => r.ThuVietTat == "T7" && !lstNgayNghiLe.Contains(r.NgayCong)).GroupBy(x => x.NgayCong).Select(a => a.Key).ToList();
        //        var ListNgayCN = lstChamCongNhanViens.Where(r => r.ThuVietTat == "CN" && !lstNgayNghiLe.Contains(r.NgayCong)).GroupBy(x => x.NgayCong).Select(a => a.Key).ToList();

        //        NgayKetThucMax = NgayKetThucMax.AddDays(1);

        //        List<HRM_Luong_NhanVien> listLuongNhanVien = db.HRM_Luong_NhanVien.Where(r => lstIdNhanVien.Contains(r.IdNhanVien) && ((r.NgayHetHieuLuc.Year < 2000 || NgayKetThucMax > r.NgayBatDauHieuLuc) || (NgayKetThucMax > r.NgayHetHieuLuc && r.NgayHetHieuLuc >= NgayBatDauMin) || (r.NgayHetHieuLuc >= NgayKetThucMax && NgayKetThucMax > r.NgayBatDauHieuLuc))).ToList();
        //        List<string> listIdLuongNhanVien = listLuongNhanVien.Select(r => r.IdGuid).ToList();
        //        List<string> listIdNhanVien = listNhanViens.Select(r => r.IdGuid).ToList();
        //        List<long> listIdNhanVienEOS = listNhanViens.Select(r => r.IdNhanVienEOS).ToList();
        //        string IdTrangThaiCuoiLamThemGio = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHLAMTHEMGIO.ToString());
        //        var listNhanVienLamThemGio = (from a in smdb.NhanViens
        //                                      join b in smdb.QT_Link_NhanVien_LamThemGio on a.UserPkid equals b.IdUser
        //                                      join c in smdb.QT_QuyTrinhLamThemGio on b.IdQuyTrinh equals c.Id
        //                                      where c.IdTrangThai == IdTrangThaiCuoiLamThemGio && c.Loai != true
        //                                      && NgayKetThucMax > b.ThoiGianKetThuc && b.ThoiGianBatDau > NgayBatDauMin
        //                                      && listIdNhanVienEOS.Contains(a.Id)
        //                                      select new
        //                                      {
        //                                          IdNhanVienEOS = a.Id,
        //                                          b.ThoiGianBatDau,
        //                                          b.ThoiGianKetThuc,
        //                                          b.ThoiGian,
        //                                          b.NgayTrongTuan,
        //                                          b.PhuCap
        //                                      }).ToList();
        //        var HopDongs = (from a in db.HRM_HopDong
        //                        join b in db.HRM_BaoHiem_HopDong on a.IdGuid equals b.IdHopDong
        //                        join c in db.HRM_Luong_HopDong on a.IdGuid equals c.IdHopDong
        //                        where lstIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == "HieuLuc"
        //                        && a.IdCongTyEOS == IdCongTyEOS && c.isTinhLuong == true
        //                        select new
        //                        {
        //                            a.IdNhanVien,
        //                            b.isBaoHiemXaHoi,
        //                            b.isBaoHiemThatNghiep,
        //                            a.Modified,
        //                            b.isBaoHiemYTe,
        //                            c.isTinhLuong,
        //                            c.TyLeHuongLuong,
        //                            isCongDoan = a.isThamGiaCongDoan,
        //                            a.IdBoPhanEOS,

        //                        }).ToList();
        //        var dmPhuCaps = db.HRM_DM_PhuCap.ToList();
        //        List<HRM_Luong_PhuCap_NhanVien> listPhuCapNhanVien = db.HRM_Luong_PhuCap_NhanVien.Where(r => listIdLuongNhanVien.Contains(r.IdLuong_NhanVien)).ToList();
        //        List<ThanhToanLuongViewModel> lstThanhToanLuongNhanVien = new List<ThanhToanLuongViewModel>();
        //        string IddmPhuCapKiemNhiem = dmPhuCaps.Where(r => r.Ma == "KIEMNHIEM").Select(r => r.IdGuid).FirstOrDefault();
        //        foreach (var item in listNgayChamCongNhanVien)
        //        {

        //            var HopDongNhanVien = HopDongs.Where(r => r.IdNhanVien == item.IdNhanVien).FirstOrDefault();
        //            if (HopDongNhanVien == null)
        //                continue;
        //            if (HopDongNhanVien.isTinhLuong == false)
        //                continue;
        //            var LuongNhanVien = listLuongNhanVien.Where(r => r.IdNhanVien == item.IdNhanVien && ((r.NgayHetHieuLuc.Year < 2000 && NgayKetThucMax > r.NgayBatDauHieuLuc) || (NgayKetThucMax > r.NgayHetHieuLuc && r.NgayHetHieuLuc >= NgayBatDauMin)) && (NgayBatDauMin >= r.NgayHetHieuLuc)).ToList();
        //            if (LuongNhanVien.Count() == 0)
        //                continue;
        //            HRM_NhanVien nhanVien = listNhanViens.FirstOrDefault(r => r.IdGuid == item.IdNhanVien);

        //            var NhanVienLamThemGio = listNhanVienLamThemGio.Where(r => r.IdNhanVienEOS == nhanVien.IdNhanVienEOS).ToList();

        //            var lstNguoiPhuThuoc = db.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == item.IdNhanVien && r.isLaNguoiPhuThuoc == true).ToList();
        //            var nhanViens = listNhanViens.Where(r => r.IdGuid == item.IdNhanVien).FirstOrDefault();
        //            if (nhanViens == null)
        //                continue;



        //            if (LuongNhanVien.Count() == 1)
        //            {
        //                List<PhuCapViewModel> listPhuCapLuong = new List<PhuCapViewModel>();
        //                ThanhToanLuongViewModel luongNhanVien = new ThanhToanLuongViewModel();
        //                long IdBoPhanEOS = HopDongNhanVien.IdBoPhanEOS;// LuongNhanVien[0].IdBoPhanEOS;
        //                string IdLuongNhanVien = LuongNhanVien[0].IdGuid;
        //                double SoNgayLamViec = 0;
        //                ChamCongNhanVienViewModel NgayCongNhanVien = new ChamCongNhanVienViewModel();
        //                var BoPhans = ListBoPhanEOS.Where(r => r.Id == IdBoPhanEOS).FirstOrDefault();
        //                if (BoPhans != null && BoPhans.SoNgayLamViec != 0)
        //                    SoNgayLamViec = BoPhans.SoNgayLamViec;
        //                else
        //                {
        //                    SoNgayLamViec = 26;
        //                }


        //                var listChamCongNhanVien = lstChamCongNhanViens.Where(r => r.IdNhanVien == item.IdNhanVien).ToList();
        //                if (listChamCongNhanVien.Count() == 0)
        //                    continue;


        //                if (MaDotChamCong == "1")
        //                {
        //                    NgayCongNhanVien = (from a in listChamCongNhanVien
        //                                        where 16 > a.Ngay
        //                                        group a by a.IdNhanVien into b
        //                                        select new ChamCongNhanVienViewModel
        //                                        {
        //                                            IdNhanVien = b.Key,

        //                                            SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //                                            SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //                                            SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //                                        }).FirstOrDefault();
        //                    luongNhanVien.SoNgayCongKy1 = NgayCongNhanVien.SoNgayDiLam + NgayCongNhanVien.SoNgayNghiPhep;
        //                }
        //                else if (MaDotChamCong == "2")
        //                {
        //                    NgayCongNhanVien = (from a in listChamCongNhanVien
        //                                        where a.Ngay > 16
        //                                        group a by a.IdNhanVien into b
        //                                        select new ChamCongNhanVienViewModel
        //                                        {
        //                                            IdNhanVien = b.Key,

        //                                            SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //                                            SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //                                            SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //                                        }).FirstOrDefault();
        //                    luongNhanVien.SoNgayCongKy2 = NgayCongNhanVien.SoNgayDiLam + NgayCongNhanVien.SoNgayNghiPhep;
        //                }
        //                else
        //                {
        //                    var NgayCongNhanVien1 = (from a in listChamCongNhanVien
        //                                             where 16 > a.Ngay
        //                                             group a by a.IdNhanVien into b
        //                                             select new ChamCongNhanVienViewModel
        //                                             {
        //                                                 IdNhanVien = b.Key,

        //                                                 SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //                                                 SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //                                                 SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //                                             }).FirstOrDefault();
        //                    luongNhanVien.SoNgayCongKy1 = NgayCongNhanVien1.SoNgayDiLam + NgayCongNhanVien1.SoNgayNghiPhep;


        //                    NgayCongNhanVien = (from a in listChamCongNhanVien
        //                                        group a by a.IdNhanVien into b
        //                                        select new ChamCongNhanVienViewModel
        //                                        {
        //                                            IdNhanVien = b.Key,

        //                                            SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //                                            SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //                                            SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //                                        }).FirstOrDefault();
        //                    luongNhanVien.SoNgayCongKy2 = NgayCongNhanVien.SoNgayDiLam + NgayCongNhanVien.SoNgayNghiPhep - luongNhanVien.SoNgayCongKy1;
        //                }



        //                var listPhuCap = listPhuCapNhanVien.Where(r => r.IdLuong_NhanVien == IdLuongNhanVien && NgayKetThucMax > r.NgayBatDauHieuLuc && r.NgayHetHieuLuc > NgayBatDauMin).ToList();
        //                luongNhanVien.KiemNhiem = listPhuCap.Where(r => r.IddmLoaiPhuCap == IddmPhuCapKiemNhiem).Select(r => r.SoTien).FirstOrDefault();
        //                foreach (var itemPC in listPhuCap)
        //                {
        //                    if (itemPC.IddmLoaiPhuCap == IddmPhuCapKiemNhiem)
        //                        continue;

        //                    var phuCap = dmPhuCaps.Where(r => r.IdGuid == itemPC.IddmLoaiPhuCap).FirstOrDefault();
        //                    if (phuCap != null)
        //                    {
        //                        PhuCapViewModel phuCapLuong = new PhuCapViewModel
        //                        {
        //                            IddmLoaiPhuCap = phuCap.IdGuid,
        //                            SoTien = itemPC.SoTien,
        //                            LoaiPhuCap = itemPC.LoaiPhuCap,
        //                            Loai = phuCap.Loai,
        //                            TenLoaiPhuCap = phuCap.Ten,
        //                        };
        //                        listPhuCapLuong.Add(phuCapLuong);
        //                    }
        //                }
        //                luongNhanVien.TroCapNgoai = NhanVienLamThemGio.Sum(r => r.PhuCap);
        //                var TangCaNgayT7 = NhanVienLamThemGio.Where(r => ListNgayT7.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                var TangCaNgayCN = NhanVienLamThemGio.Where(r => ListNgayCN.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                var TangCaNgayLe = NhanVienLamThemGio.Where(r => lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                var TangCaNgayThuong = NhanVienLamThemGio.Where(r => !lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date) && !ListNgayT7.Contains(r.ThoiGianBatDau.Date) && !ListNgayCN.Contains(r.ThoiGianBatDau.Date)).ToList();
        //                foreach (var itemTC in TangCaNgayT7)
        //                {

        //                    luongNhanVien.TangCaNgayT7 = luongNhanVien.TangCaNgayT7 + itemTC.ThoiGian;

        //                }
        //                foreach (var itemTC in TangCaNgayCN)
        //                {
        //                    luongNhanVien.TangCaNgayCN = luongNhanVien.TangCaNgayCN + itemTC.ThoiGian;
        //                }
        //                foreach (var itemTC in TangCaNgayLe)
        //                {
        //                    luongNhanVien.TangCaNgayLe = luongNhanVien.TangCaNgayT7 + itemTC.ThoiGian;
        //                }
        //                foreach (var itemTC in TangCaNgayThuong)
        //                {
        //                    luongNhanVien.TangCaNgayThuong = luongNhanVien.TangCaNgayThuong + itemTC.ThoiGian;//  ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //                }
        //                luongNhanVien.LamThemGio = luongNhanVien.TangCaNgayT7 + luongNhanVien.TangCaNgayLe + luongNhanVien.TangCaNgayThuong + luongNhanVien.TangCaNgayCN;
        //                luongNhanVien.TongLuongPhuCap = LuongNhanVien[0].LuongHachToan + listPhuCapLuong.Sum(r => r.SoTien);
        //                luongNhanVien.listPhuCap = listPhuCapLuong.OrderBy(r => r.TenLoaiPhuCap).ToList();
        //                luongNhanVien.LuongChinh = LuongNhanVien[0].LuongHachToan;
        //                if (MaDotChamCong == "1")
        //                {
        //                    luongNhanVien.ThucNhanKy1 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy1;
        //                }
        //                else if (MaDotChamCong == "2")
        //                {
        //                    luongNhanVien.BHXH_NSD = luongNhanVien.LuongChinh * 0.215;
        //                    luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.02;
        //                    luongNhanVien.BHXH_NLD = luongNhanVien.LuongChinh * 0.105;
        //                    luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.01;
        //                    luongNhanVien.ThucNhanKy2 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy2 - luongNhanVien.BHXH_NLD - luongNhanVien.CPCD_NSD + luongNhanVien.LamThemGio;
        //                }
        //                else
        //                {
        //                    luongNhanVien.BHXH_NSD = luongNhanVien.LuongChinh * 0.215;
        //                    luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.02;
        //                    luongNhanVien.BHXH_NLD = luongNhanVien.LuongChinh * 0.105;
        //                    luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.01;
        //                    luongNhanVien.ThucNhanKy1 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy1;
        //                    luongNhanVien.ThucNhanKy2 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy2 - luongNhanVien.BHXH_NLD - luongNhanVien.CPCD_NSD;
        //                    luongNhanVien.ThucNhanKy = luongNhanVien.ThucNhanKy1 + luongNhanVien.ThucNhanKy2;
        //                }
        //                lstThanhToanLuongNhanVien.Add(luongNhanVien);
        //            }
        //            //else
        //            //{
        //            //    List<ThanhToanLuongViewModel> lstNhanVienNhieuMucLuong = new List<ThanhToanLuongViewModel>();
        //            //    var listChamCongNhanViens = lstChamCongNhanViens.Where(r => r.IdNhanVien == item.IdNhanVien).ToList();
        //            //    LuongNhanVien = LuongNhanVien.OrderBy(r => r.NgayBatDauHieuLuc).ToList(); List<PhuCapViewModel> listPhuCapLuong = new List<PhuCapViewModel>();
        //            //    for (int i = 0; i < LuongNhanVien.Count(); i++)
        //            //    {

        //            //        ThanhToanLuongViewModel luongNhanVien = new ThanhToanLuongViewModel();
        //            //        ChamCongNhanVienViewModel NgayCongNhanVien = new ChamCongNhanVienViewModel();   
        //            //        var itemLuong = LuongNhanVien[i];
        //            //        double SoNgayLamViec = 0;
        //            //        var BoPhans = ListBoPhanEOS.Where(r => r.Id == itemLuong.IdBoPhanEOS).FirstOrDefault();
        //            //        if (BoPhans != null)
        //            //            SoNgayLamViec = BoPhans.SoNgayLamViec;
        //            //        else
        //            //        {
        //            //            SoNgayLamViec = 26;
        //            //        }
        //            //        itemLuong.NgayBatDauHieuLuc = itemLuong.NgayBatDauHieuLuc.Date;
        //            //        if (itemLuong.NgayHetHieuLuc.Year < 2000)
        //            //        {
        //            //            if (i + 1 < LuongNhanVien.Count)
        //            //            {
        //            //                itemLuong.NgayHetHieuLuc = LuongNhanVien[i + 1].NgayBatDauHieuLuc.AddDays(-1).Date;
        //            //                if (itemLuong.NgayHetHieuLuc.Year < 2000)
        //            //                    itemLuong.NgayHetHieuLuc = NgayKetThucMax.AddDays(10);
        //            //            }
        //            //            else
        //            //                itemLuong.NgayHetHieuLuc = NgayKetThucMax.AddDays(10);
        //            //        }

        //            //        var listPhuCap = listPhuCapNhanVien.Where(r => r.IdLuong_NhanVien == itemLuong.IdGuid && NgayKetThucMax > r.NgayBatDauHieuLuc && r.NgayHetHieuLuc > NgayBatDauMin).ToList(); foreach (var itemPC in listPhuCapNhanVien)
        //            //        {
        //            //            var PhuCap = dmPhuCaps.FirstOrDefault(r => r.IdGuid == itemPC.IddmLoaiPhuCap);
        //            //            if (PhuCap != null)
        //            //                itemPC.isTinhThueTNCN = PhuCap.isTinhThueTNCN;

        //            //        }

        //            //        var listChamCongNhanVien = listChamCongNhanViens.Where(r => r.NgayCong >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.NgayCong).ToList();
        //            //        if (listChamCongNhanVien.Count == 0)
        //            //            continue;
        //            //        if (MaDotChamCong == "1")
        //            //        {
        //            //            NgayCongNhanVien = (from a in listChamCongNhanVien
        //            //                                where 16 > a.Ngay
        //            //                                group a by a.IdNhanVien into b
        //            //                                select new ChamCongNhanVienViewModel
        //            //                                {
        //            //                                    IdNhanVien = b.Key,

        //            //                                    SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //            //                                    SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //            //                                    SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //            //                                }).FirstOrDefault();
        //            //            luongNhanVien.SoNgayCongKy1 = NgayCongNhanVien.SoNgayDiLam + NgayCongNhanVien.SoNgayNghiPhep;
        //            //        }
        //            //        else if (MaDotChamCong == "2")
        //            //        {
        //            //            NgayCongNhanVien = (from a in listChamCongNhanVien
        //            //                                where a.Ngay > 16
        //            //                                group a by a.IdNhanVien into b
        //            //                                select new ChamCongNhanVienViewModel
        //            //                                {
        //            //                                    IdNhanVien = b.Key,

        //            //                                    SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //            //                                    SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //            //                                    SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //            //                                }).FirstOrDefault();
        //            //            luongNhanVien.SoNgayCongKy2 = NgayCongNhanVien.SoNgayDiLam + NgayCongNhanVien.SoNgayNghiPhep;
        //            //        }
        //            //        else
        //            //        {
        //            //            var NgayCongNhanVien1 = (from a in listChamCongNhanVien
        //            //                                     where 16 > a.Ngay
        //            //                                     group a by a.IdNhanVien into b
        //            //                                     select new ChamCongNhanVienViewModel
        //            //                                     {
        //            //                                         IdNhanVien = b.Key,

        //            //                                         SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //            //                                         SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //            //                                         SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //            //                                     }).FirstOrDefault();
        //            //            luongNhanVien.SoNgayCongKy1 = NgayCongNhanVien1.SoNgayDiLam + NgayCongNhanVien1.SoNgayNghiPhep;


        //            //            NgayCongNhanVien = (from a in listChamCongNhanVien
        //            //                                group a by a.IdNhanVien into b
        //            //                                select new ChamCongNhanVienViewModel
        //            //                                {
        //            //                                    IdNhanVien = b.Key,

        //            //                                    SoGioLamViec = b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioSang) + b.Where(r => r.MaChamCongChieu == "DL").Sum(r => r.SoGioChieu),
        //            //                                    SoNgayDiLam = (b.Where(r => r.MaChamCongChieu == "DL").Count() + b.Where(r => r.MaChamCongChieu == "DL").Count()) / 2,
        //            //                                    SoNgayNghiPhep = (b.Where(r => r.MaChamCongChieu == "NPHL").Count() + b.Where(r => r.MaChamCongChieu == "NPHL").Count()) / 2,

        //            //                                }).FirstOrDefault();
        //            //            luongNhanVien.SoNgayCongKy2 = NgayCongNhanVien.SoNgayDiLam + NgayCongNhanVien.SoNgayNghiPhep - luongNhanVien.SoNgayCongKy1;
        //            //        }
        //            //        var listPhuCapNBL = listPhuCapNhanVien.Where(r => r.IdLuong_NhanVien == itemLuong.IdGuid && NgayKetThucMax > r.NgayBatDauHieuLuc && r.NgayHetHieuLuc > NgayBatDauMin).ToList();

        //            //        luongNhanVien.KiemNhiem = listPhuCap.Where(r => r.IddmLoaiPhuCap == IddmPhuCapKiemNhiem).Select(r => r.SoTien).FirstOrDefault();
        //            //        foreach (var itemPC in listPhuCapNBL)
        //            //        {
        //            //            if (itemPC.IddmLoaiPhuCap == IddmPhuCapKiemNhiem)
        //            //                continue;

        //            //            var phuCap = dmPhuCaps.Where(r => r.IdGuid == itemPC.IddmLoaiPhuCap).FirstOrDefault();
        //            //            if (phuCap != null)
        //            //            {
        //            //                PhuCapViewModel phuCapLuong = new PhuCapViewModel
        //            //                {
        //            //                    IddmLoaiPhuCap = phuCap.IdGuid,
        //            //                    SoTien = itemPC.SoTien,
        //            //                    LoaiPhuCap = itemPC.LoaiPhuCap,
        //            //                    Loai = phuCap.Loai,
        //            //                    TenLoaiPhuCap = phuCap.Ten,
        //            //                };
        //            //                listPhuCapLuong.Add(phuCapLuong);
        //            //            }
        //            //        }
        //            //        luongNhanVien.TroCapNgoai = NhanVienLamThemGio.Sum(r => r.PhuCap);
        //            //        var TangCaNgayThuong = NhanVienLamThemGio.Where(r => !lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date) && !ListNgayT7.Contains(r.ThoiGianBatDau.Date) && !ListNgayCN.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //            //        var TangCaNgayT7 = NhanVienLamThemGio.Where(r => ListNgayT7.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //            //        var TangCaNgayCN = NhanVienLamThemGio.Where(r => ListNgayCN.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();
        //            //        var TangCaNgayLe = NhanVienLamThemGio.Where(r => lstNgayNghiLe.Contains(r.ThoiGianBatDau.Date) && r.ThoiGianBatDau.Date >= itemLuong.NgayBatDauHieuLuc && itemLuong.NgayHetHieuLuc >= r.ThoiGianBatDau.Date).ToList();

        //            //        foreach (var itemTC in TangCaNgayT7)
        //            //        {

        //            //            luongNhanVien.TangCaNgayT7 = luongNhanVien.TangCaNgayT7 + itemTC.ThoiGian;

        //            //        }
        //            //        foreach (var itemTC in TangCaNgayCN)
        //            //        {
        //            //            luongNhanVien.TangCaNgayCN = luongNhanVien.TangCaNgayCN + itemTC.ThoiGian;
        //            //        }
        //            //        foreach (var itemTC in TangCaNgayLe)
        //            //        {
        //            //            luongNhanVien.TangCaNgayLe = luongNhanVien.TangCaNgayT7 + itemTC.ThoiGian;
        //            //        }
        //            //        foreach (var itemTC in TangCaNgayThuong)
        //            //        {
        //            //            luongNhanVien.TangCaNgayThuong = luongNhanVien.TangCaNgayThuong + itemTC.ThoiGian;//  ((DateTimeToUnixTimeStamp(itemTC.ThoiGianKetThuc) - DateTimeToUnixTimeStamp(itemTC.ThoiGianBatDau)) / (60 * 60));
        //            //        }
        //            //        luongNhanVien.LamThemGio = luongNhanVien.TangCaNgayT7 + luongNhanVien.TangCaNgayLe + luongNhanVien.TangCaNgayThuong + luongNhanVien.TangCaNgayCN;
        //            //        luongNhanVien.TongLuongPhuCap = LuongNhanVien[0].LuongHachToan + listPhuCapLuong.Sum(r => r.SoTien);
        //            //        luongNhanVien.listPhuCap = listPhuCapLuong.OrderBy(r => r.TenLoaiPhuCap).ToList();
        //            //        luongNhanVien.LuongChinh = LuongNhanVien[0].LuongHachToan;
        //            //        if (MaDotChamCong == "1")
        //            //        {
        //            //            luongNhanVien.ThucNhanKy1 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy1;
        //            //        }
        //            //        else if (MaDotChamCong == "2")
        //            //        {
        //            //            luongNhanVien.BHXH_NSD = luongNhanVien.LuongChinh * 0.215;
        //            //            luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.02;
        //            //            luongNhanVien.BHXH_NLD = luongNhanVien.LuongChinh * 0.105;
        //            //            luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.01;
        //            //            luongNhanVien.ThucNhanKy2 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy2 - luongNhanVien.BHXH_NLD - luongNhanVien.CPCD_NSD + luongNhanVien.LamThemGio;
        //            //        }
        //            //        else
        //            //        {
        //            //            luongNhanVien.BHXH_NSD = luongNhanVien.LuongChinh * 0.215;
        //            //            luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.02;
        //            //            luongNhanVien.BHXH_NLD = luongNhanVien.LuongChinh * 0.105;
        //            //            luongNhanVien.CPCD_NSD = luongNhanVien.LuongChinh * 0.01;
        //            //            luongNhanVien.ThucNhanKy1 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy1;
        //            //            luongNhanVien.ThucNhanKy2 = luongNhanVien.TongLuongPhuCap / SoNgayLamViec * luongNhanVien.SoNgayCongKy2 - luongNhanVien.BHXH_NLD - luongNhanVien.CPCD_NSD;
        //            //            luongNhanVien.ThucNhanKy = luongNhanVien.ThucNhanKy1 + luongNhanVien.ThucNhanKy2;
        //            //        }
        //            //        lstNhanVienNhieuMucLuong.Add(luongNhanVien);
        //            //    }


        //            //}

        //        }
        //        return lstThanhToanLuongNhanVien;
        //    }

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
