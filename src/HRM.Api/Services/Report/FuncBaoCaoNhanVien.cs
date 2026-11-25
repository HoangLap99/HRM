#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HRM.Api.Services.BaoCao
{
    public class FuncBaoCaoNhanVien : FuncBase
    {
        public FuncBaoCaoNhanVien(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBaoCaoNhanVien> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        //  DefineData.eAction eAction = DefineData.eAction.BAOCAOTHONGTINNHANVIEN;
        public dynamic ExportTongHopNhanSuDaiAn(long IdCongTyEOS, ApplicationDbContext db)
        {

            var nhanViens = (from a in db.HRM_NhanVien
                             join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
                             where b.IdCongTyEOS == IdCongTyEOS && b.isPhuLuc == false 
                             select new ExportBaoCaoNhanVienHopDongViewModel
                             {
                                 IdNhanVien = a.IdGuid,
                                 IdBoPhanEOS = b.IdBoPhanEOS,
                                 IddmTrinhDo = a.IddmTrinhDo,
                                 IddmThoiHanHopDong = b.IdThoiHanHopDong,
                                 IddmLoaiHopDong = b.IdLoaiHopDong,
                                 IdGioiTinh = a.IdGioiTinh,
                                 NgaySinh = a.NgaySinh,
                                 NgayBatDauLamViec = a.NgayBatDauLamViec,

                             }).ToList();
            var listIdBoPhan = nhanViens.GroupBy(r => r.IdBoPhanEOS).Select(r => r.Key).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan).ToList();
            var CongTy= listBoPhan.Where(r => r.Id==IdCongTyEOS).FirstOrDefault();
            var listIdGioiTinh = nhanViens.GroupBy(r => r.IdGioiTinh).Select(r => r.Key).ToList();
            var listIdTrinhdo = nhanViens.GroupBy(r => r.IddmTrinhDo).Select(r => r.Key).ToList();
            var lstdmTrinhdo = db.HRM_dmTrinhDoDaoTao.Where(r => listIdTrinhdo.Contains(r.IdGuid)).ToList();

            var listIddmLoaiHopDong = nhanViens.GroupBy(r => r.IddmLoaiHopDong).Select(r => r.Key).ToList();
            var lstdmLoaiHopDong = db.HRM_dmLoaiHopDong.Where(r => listIddmLoaiHopDong.Contains(r.IdGuid)).ToList();

            var listIddmThoiHanHopDong = nhanViens.GroupBy(r => r.IddmThoiHanHopDong).Select(r => r.Key).ToList();
            var lstdmThoiHanHopDong = db.HRM_dmThoiHanHopDong.Where(r => listIddmThoiHanHopDong.Contains(r.IdGuid)).ToList();

            string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoTongHopNhansu.xlsx");

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var sheet1 = package.Workbook.Worksheets[1];
            int nRow = 5;
            if(CongTy!=null&& CongTy.TenBoPhan!=null)
            {
                sheet1.Cells[2, 1].Value ="Công ty: "+ CongTy.TenBoPhan;
            }    
            sheet1.Cells[4, 4].Value = nhanViens.Count();
            string TyLeNhanVien = "=+D3/$D$3";
            sheet1.Cells[4, 5].FormulaR1C1 = TyLeNhanVien;
            int STT = 1;
            int RowSTT = 0;
            foreach (var boPhan in listBoPhan)
            {
                var nhanvienBoPhan = nhanViens.Where(r => r.IdBoPhanEOS == boPhan.Id).Count();
                sheet1.Cells[nRow, 1].Value = STT;
                sheet1.Cells[nRow, 3].Value = boPhan.TenBoPhan;
                sheet1.Cells[nRow, 4].Value = nhanvienBoPhan;
                string TyLeNhanVienRow = "=+D" + nRow + "/$D$3";
                sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeNhanVienRow;
                nRow++;
                STT++;
            }

            sheet1.Cells[5, 2].Value = "Các Phòng ban";
            sheet1.Cells[5, 2].Style.Font.Bold = true;
            RowSTT = nRow - 1;
            sheet1.Cells[5, 2, RowSTT, 2].Merge = true;

            STT = 1;

            foreach (var thoiHanHopDong in lstdmThoiHanHopDong)
            {
                var danhsachnhanviens = nhanViens.Where(r => r.IddmThoiHanHopDong == thoiHanHopDong.IdGuid).Count();
                sheet1.Cells[nRow, 1].Value = STT;
                sheet1.Cells[nRow, 3].Value = thoiHanHopDong.Ten;
                sheet1.Cells[nRow, 4].Value = danhsachnhanviens;
                string TyLeNhanVienRow = "=+D" + nRow + "/$D$3";
                sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeNhanVienRow;
                nRow++;
                STT++;
            }
            foreach (var loai in lstdmLoaiHopDong)
            {
                var danhsachnhanviens = nhanViens.Where(r => r.IddmLoaiHopDong == loai.IdGuid).Count();
                sheet1.Cells[nRow, 1].Value = STT;
                sheet1.Cells[nRow, 3].Value = loai.Ten;
                sheet1.Cells[nRow, 4].Value = danhsachnhanviens;
                string TyLeNhanVienRow = "=+D" + nRow + "/$D$3";
                sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeNhanVienRow;
                nRow++;
                STT++;
            }

            //foreach (var thoiHanHopDong in lstdmThoiHanHopDong)
            //{
            //    var danhsachnhanviens = nhanViens.Where(r => r.IddmThoiHanHopDong == thoiHanHopDong.IdGuid).Count();
            //    sheet1.Cells[nRow, 1].Value = STT;
            //    sheet1.Cells[nRow, 3].Value = thoiHanHopDong.Ten;
            //    sheet1.Cells[nRow, 4].Value = danhsachnhanviens;
            //    string TyLeNhanVienRow = "=+D" + nRow + "/$D$3";
            //    sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeNhanVienRow;
            //    nRow++;
            //    STT++;
            //}

            sheet1.Cells[RowSTT + 1, 2].Value = "Tình trạng hợp đồng";
            sheet1.Cells[RowSTT + 1, 2].Style.Font.Bold = true;
            sheet1.Cells[RowSTT + 1, 2, nRow - 1, 2].Merge = true;
            RowSTT = nRow - 1;

            STT = 1;
            foreach (var trinhdo in lstdmTrinhdo)
            {
                var danhsachnhanviens = nhanViens.Where(r => r.IddmTrinhDo == trinhdo.IdGuid).Count();
                sheet1.Cells[nRow, 1].Value = STT;
                sheet1.Cells[nRow, 3].Value = trinhdo.Ten;
                sheet1.Cells[nRow, 4].Value = danhsachnhanviens;
                string TyLeNhanVienRow = "=+D" + nRow + "/$D$3";
                sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeNhanVienRow;
                nRow++;
                STT++;
            }
            sheet1.Cells[RowSTT + 1, 2].Value = "Trình độ";
            sheet1.Cells[RowSTT + 1, 2].Style.Font.Bold = true;
            sheet1.Cells[RowSTT + 1, 2, nRow - 1, 2].Merge = true;
            RowSTT = nRow - 1;

            DateTime dateTimeNow = DateTime.Now;
            // Tham niên 
            // 0 tháng đến < 2 Tháng
            DateTime dateTimeBatDau = dateTimeNow.AddMonths(-2);
            var ThamNien1 = nhanViens.Where(r => r.NgayBatDauLamViec >= dateTimeBatDau).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "0 tháng đến < 2 Tháng";
            sheet1.Cells[nRow, 4].Value = ThamNien1;
            string TyLeThamNien1 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeThamNien1;
            nRow++;
            STT++;

            // 2 tháng đến < 1 năm

            DateTime dateTimeKetThuc = dateTimeNow.AddYears(-1);
            dateTimeBatDau = dateTimeNow.AddMonths(-2);
            var ThamNien2 = nhanViens.Where(r => r.NgayBatDauLamViec >= dateTimeKetThuc && dateTimeBatDau >= r.NgayBatDauLamViec).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "2 tháng đến < 1 năm";
            sheet1.Cells[nRow, 4].Value = ThamNien2;
            string TyLeThamNien2 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeThamNien2;
            nRow++;
            STT++;

            // 1 năm đến < 2 năm
            dateTimeKetThuc = dateTimeNow.AddYears(-2);
            dateTimeBatDau = dateTimeNow.AddYears(-1);
            var ThamNien3 = nhanViens.Where(r => r.NgayBatDauLamViec >= dateTimeKetThuc && dateTimeBatDau >= r.NgayBatDauLamViec).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "1 năm đến < 2 năm";
            sheet1.Cells[nRow, 4].Value = ThamNien3;
            string TyLeThamNien3 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeThamNien3;
            nRow++;
            STT++;

            // 2 năm đến < 5 năm
            dateTimeKetThuc = dateTimeNow.AddYears(-5);
            dateTimeBatDau = dateTimeNow.AddYears(-2);
            var ThamNien4 = nhanViens.Where(r => r.NgayBatDauLamViec >= dateTimeKetThuc && dateTimeBatDau >= r.NgayBatDauLamViec).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "2 năm đến < 5 năm";
            sheet1.Cells[nRow, 4].Value = ThamNien4;
            string TyLeThamNien4 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeThamNien4;
            nRow++;
            STT++;


            // Từ 3 năm trở lên
            dateTimeBatDau = dateTimeNow.AddYears(-3);
            var ThamNien5 = nhanViens.Where(r => dateTimeBatDau >= r.NgayBatDauLamViec).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "Từ 3 năm trở lên";
            sheet1.Cells[nRow, 4].Value = ThamNien5;
            string TyLeThamNien5 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeThamNien5;
            nRow++;
            STT++;


            // Từ 5 năm trở lên
            dateTimeBatDau = dateTimeNow.AddYears(-3);
            var ThamNien6 = nhanViens.Where(r => dateTimeBatDau >= r.NgayBatDauLamViec).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "Từ 5 năm trở lên";
            sheet1.Cells[nRow, 4].Value = ThamNien6;
            string TyLeThamNien6 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeThamNien6;
            nRow++;
            STT++;

            sheet1.Cells[RowSTT + 1, 2].Value = "Thâm niên";
            sheet1.Cells[RowSTT + 1, 2].Style.Font.Bold = true;
            sheet1.Cells[RowSTT + 1, 2, nRow - 1, 2].Merge = true;
            RowSTT = nRow - 1;

            STT = 1;
            foreach (var gioiTinh in listIdGioiTinh)
            {
                var nhanvienGT = nhanViens.Where(r => r.IdGioiTinh == gioiTinh).Count();
                sheet1.Cells[nRow, 1].Value = STT;
                sheet1.Cells[nRow, 3].Value = gioiTinh;
                sheet1.Cells[nRow, 4].Value = nhanvienGT;
                string TyLeNhanVienRow = "=+D" + nRow + "/$D$3";
                sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeNhanVienRow;
                nRow++;
                STT++;
            }
            sheet1.Cells[RowSTT + 1, 2].Value = "Giới tính";
            sheet1.Cells[RowSTT + 1, 2].Style.Font.Bold = true;
            sheet1.Cells[RowSTT + 1, 2, nRow - 1, 2].Merge = true;
            RowSTT = nRow - 1;


            STT = 1;

            // Dưới 23
            dateTimeKetThuc = dateTimeNow.AddYears(-5);
            dateTimeBatDau = dateTimeNow.AddYears(-23);
            var DoTuoi1 = nhanViens.Where(r => r.NgaySinh>=   dateTimeBatDau  ).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "Dưới 23";
            sheet1.Cells[nRow, 4].Value = DoTuoi1;
            string TyLeDoTuoi1 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi1;
            nRow++;
            STT++;

            // <30
         //   dateTimeKetThuc = dateTimeNow.AddYears(-5);
            dateTimeBatDau = dateTimeNow.AddYears(-30);
            var DoTuoi2= nhanViens.Where(r => r.NgaySinh >= dateTimeBatDau).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "<30";
            sheet1.Cells[nRow, 4].Value = DoTuoi2;
            string TyLeDoTuoi2 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi2;
            nRow++;
            STT++;



            // 30-40
            dateTimeKetThuc = dateTimeNow.AddYears(-40);
            dateTimeBatDau = dateTimeNow.AddYears(-30);
            var DoTuoi3 = nhanViens.Where(r => r.NgaySinh >= dateTimeKetThuc && dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "30-40";
            sheet1.Cells[nRow, 4].Value = DoTuoi3;
            string TyLeDoTuoi3 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi3;
            nRow++;
            STT++;


            // 40-50
            dateTimeKetThuc = dateTimeNow.AddYears(-50);
            dateTimeBatDau = dateTimeNow.AddYears(-40);
            var DoTuoi4= nhanViens.Where(r => r.NgaySinh >= dateTimeKetThuc && dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "40-50";
            sheet1.Cells[nRow, 4].Value = DoTuoi4;
            string TyLeDoTuoi4 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi4;
            nRow++;
            STT++;

            // >50
          
            dateTimeBatDau = dateTimeNow.AddYears(-50);
            var DoTuoi5 = nhanViens.Where(r =>  dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = ">50";
            sheet1.Cells[nRow, 4].Value = DoTuoi5;
            string TyLeDoTuoi5 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi5;
            nRow++;
            STT++;

            // >40

            dateTimeBatDau = dateTimeNow.AddYears(-50);
            var DoTuoi6 = nhanViens.Where(r => dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = ">40";
            sheet1.Cells[nRow, 4].Value = DoTuoi6;
            string TyLeDoTuoi6 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi6;
            nRow++;
            STT++;

            // Từ 23 < 27
            dateTimeKetThuc = dateTimeNow.AddYears(-27);
            dateTimeBatDau = dateTimeNow.AddYears(-23);
            var DoTuoi7 = nhanViens.Where(r => r.NgaySinh >= dateTimeKetThuc && dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "Từ 23 < 27";
            sheet1.Cells[nRow, 4].Value = DoTuoi7;
            string TyLeDoTuoi7 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi7;
            nRow++;
            STT++;

            // Từ 27 - 35
            dateTimeKetThuc = dateTimeNow.AddYears(-35);
            dateTimeBatDau = dateTimeNow.AddYears(-27);
            var DoTuoi8 = nhanViens.Where(r => r.NgaySinh >= dateTimeKetThuc && dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = "Từ 27 - 35";
            sheet1.Cells[nRow, 4].Value = DoTuoi7;
            string TyLeDoTuoi8= "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi8;
            nRow++;
            STT++;

            // >           35 trở lên

            dateTimeBatDau = dateTimeNow.AddYears(-35);
            var DoTuoi9 = nhanViens.Where(r => dateTimeBatDau >= r.NgaySinh).Count();
            sheet1.Cells[nRow, 1].Value = STT;
            sheet1.Cells[nRow, 3].Value = ">40";
            sheet1.Cells[nRow, 4].Value = DoTuoi9;
            string TyLeDoTuoi9 = "=+D" + nRow + "/$D$3";
            sheet1.Cells[nRow, 5].FormulaR1C1 = TyLeDoTuoi9;
            nRow++;
            STT++;
            sheet1.Cells[RowSTT + 1, 2].Value = "Độ tuổi";
            sheet1.Cells[RowSTT + 1, 2].Style.Font.Bold = true;
            sheet1.Cells[RowSTT + 1, 2, nRow - 1, 2].Merge = true;
            RowSTT = nRow - 1;

            DrawTableExcel(sheet1, 4, nRow - 1, 1, 6);
            dynamic result = new DynamicObject();

            string name = "BaoCaoTongHopNhansu.xlsx";
            //var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            var filenamegui = "BaoCaoTongHopNhansu.xlsx";
            
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;
        }


        #region Thông tin quan hệ gia đình nhân viên



        public dynamic GetBaoCaoThongTinQuanHeGiaDinhNhanVien(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            #region Quyền
            //int QuyenView = GetQuyenView(eAction.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            #endregion

            DateTime dateminsql = dungchung.UnixTimeStampToDateTime(0);
            int CurrentPage = dyn.CurrentPage == null ? 0 : dyn.CurrentPage;
            int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
            string KeyWord = dyn.KeyWord == null ? "" : dyn.KeyWord;

            var query = db.HRM_NhanVien.Select(x => new BaoCaoThongTinGianHeGiaDinhNhanVien
            {
                IdNhanVien = x.IdGuid,
                MaNhanVien = x.MaNhanVien,
                TenNhanVien = x.TenDayDu,
                Created = x.Created
            });

            if (!string.IsNullOrEmpty(KeyWord))
            {
                KeyWord = KeyWord.ToLower();
                query = query.Where(x => x.MaNhanVien.ToLower().Contains(KeyWord) || x.TenNhanVien.ToLower().Contains(KeyWord));
            }


            if (CurrentPage == 0)
            {
                var lstReturn = query.OrderByDescending(x => x.Created).ToList();
                var lstQuanHeGiaDinhNhanVien = GetQuanHeGiaDinhNhanVien(new List<string>(), db);

                foreach (var itemReturn in lstReturn)
                {
                    itemReturn.lstChiTiet = lstQuanHeGiaDinhNhanVien.Where(x => x.IdNhanVien == itemReturn.IdNhanVien).ToList();
                }

                return lstReturn;
            }
            else
            {
                dynamic result = new DynamicObject();

                int count = query.Count();
                var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();
                var lstIdNhanVien = listResult.Select(x => x.IdNhanVien).ToList();
                var lstQuanHeGiaDinhNhanVien = GetQuanHeGiaDinhNhanVien(lstIdNhanVien, db);
                foreach (var itemReturn in listResult)
                {
                    itemReturn.lstChiTiet = lstQuanHeGiaDinhNhanVien.Where(x => x.IdNhanVien == itemReturn.IdNhanVien).ToList();
                }

                result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
                result.items = listResult;

                return result;
            }

        }

        public dynamic XuatExcelBaoCaoThongTinQuanHeGiaDinhNhanVien(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            dyn.CurrentPage = 0;
            List<BaoCaoThongTinGianHeGiaDinhNhanVien> lstThongTinChungNhanVien = GetBaoCaoThongTinQuanHeGiaDinhNhanVien(dyn, idCurrentUser, db);

            string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoThongTinQuanHeGiaDinhNhanVien.xlsx");

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var sheet1 = package.Workbook.Worksheets[1];
            int nRow = 5;
            // int Dem = 1;

            foreach (var NhanVien in lstThongTinChungNhanVien)
            {
                sheet1.Cells[nRow, 1, nRow, 5].Merge = true;
                sheet1.Cells[nRow, 1].Value = string.Format("{0} - {1}", NhanVien.MaNhanVien, NhanVien.TenNhanVien);
                sheet1.Cells[nRow, 1].Style.Font.Bold = true;
                nRow++;

                foreach (var ChiTiet in NhanVien.lstChiTiet)
                {
                    sheet1.Cells[nRow, 1].Value = ChiTiet.QuanHe;
                    sheet1.Cells[nRow, 2].Value = ChiTiet.HoTen;
                    sheet1.Cells[nRow, 3].Value = ChiTiet.NamSinh > 0 ? ChiTiet.NamSinh : ChiTiet.NgaySinh.Value.Year;
                    sheet1.Cells[nRow, 4].Value = ChiTiet.NgheNghiep;
                    sheet1.Cells[nRow, 5].Value = ChiTiet.NoiLamViec;
                    nRow++;
                }
            }

            DrawTableExcel(sheet1, 5, nRow - 1, 1, 5);

            dynamic result = new DynamicObject();

            string name = "BaoCaoThongTinQuanHeGiaDinhNhanVien.xlsx";
            //var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            var filenamegui = "BaoCaoThongTinQuanHeGiaDinhNhanVien.xlsx";
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;

        }

        public List<BaoCaoThongTinGianHeGiaDinhNhanVienChiTiet> GetQuanHeGiaDinhNhanVien(List<string> lstIdNhanVien, ApplicationDbContext db)
        {
            
            var query = (from qh in db.HRM_NhanVienQuanHeGiaDinh
                         join dmqh in db.HRM_dmQuanHeGiaDinh
                         on qh.IdQuanHe equals dmqh.IdGuid
                         where (lstIdNhanVien.Count == 0 || lstIdNhanVien.Contains(qh.IdNhanVien))
                         select new BaoCaoThongTinGianHeGiaDinhNhanVienChiTiet
                         {
                             IdNhanVien = qh.IdNhanVien,
                             NamSinh = qh.NamSinh,
                             NgheNghiep = qh.NgheNghiep,
                             NoiLamViec = qh.NoiLamViec,
                             QuanHe = dmqh.Ten,
                             NgaySinh = qh.NgaySinh,
                             HoTen = qh.HoTen,
                         }).ToList();
            return query;
        }

        #endregion

        #region Bằng cấp chứng chỉ nhân viên



        public dynamic GetBaoCaoBangCapChungChiNhanVien(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            #region Quyền
            //int QuyenView = GetQuyenView(eAction.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            #endregion

            DateTime dateminsql = dungchung.UnixTimeStampToDateTime(0);
            int CurrentPage = dyn.CurrentPage == null ? 0 : dyn.CurrentPage;
            int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
            string KeyWord = dyn.KeyWord == null ? "" : dyn.KeyWord;

            var query = db.HRM_NhanVien.Select(x => new BaoCaoBangCapChungChiNhanVien
            {
                IdNhanVien = x.IdGuid,
                MaNhanVien = x.MaNhanVien,
                TenNhanVien = x.TenDayDu,
                TrinhDoHocVan = x.TrinhDoHocVan,
                Created = x.Created
            });

            if (!string.IsNullOrEmpty(KeyWord))
            {
                KeyWord = KeyWord.ToLower();
                query = query.Where(x => x.MaNhanVien.ToLower().Contains(KeyWord) || x.TenNhanVien.ToLower().Contains(KeyWord));
            }


            if (CurrentPage == 0)
            {
                var lstReturn = query.OrderByDescending(x => x.Created).ToList();
                var lstBangCapNhanVien = GetBangCapNhanVien(new List<string>(), db);
                var lstChungChiNhanVien = GetChungChiNhanVien(new List<string>(), db);

                foreach (var itemReturn in lstReturn)
                {
                    itemReturn.lstBangCapNhanVien = lstBangCapNhanVien.Where(x => x.IdNhanVien == itemReturn.IdNhanVien).ToList();
                    itemReturn.lstChungChiNhanVien = lstChungChiNhanVien.Where(x => x.IdNhanVien == itemReturn.IdNhanVien).ToList();
                }

                return lstReturn;
            }
            else
            {
                dynamic result = new DynamicObject();

                int count = query.Count();
                var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();
                var lstIdNhanVien = listResult.Select(x => x.IdNhanVien).ToList();


                var lstChungChiNhanVien = GetChungChiNhanVien(lstIdNhanVien, db);
                var lstBangCapNhanVien = GetBangCapNhanVien(lstIdNhanVien, db);

                foreach (var itemReturn in listResult)
                {
                    itemReturn.lstBangCapNhanVien = lstBangCapNhanVien.Where(x => x.IdNhanVien == itemReturn.IdNhanVien).ToList();
                    itemReturn.lstChungChiNhanVien = lstChungChiNhanVien.Where(x => x.IdNhanVien == itemReturn.IdNhanVien).ToList();
                }

                result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
                result.items = listResult;

                return result;
            }

        }

        public dynamic XuatExcelBaoCaoBangCapChungChiNhanVien(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            dyn.CurrentPage = 0;
            List<BaoCaoBangCapChungChiNhanVien> lstThongTinChungNhanVien = GetBaoCaoThongTinQuanHeGiaDinhNhanVien(dyn, idCurrentUser, db);

            string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoBangCapChungChiNhanVien.xlsx");

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var sheet1 = package.Workbook.Worksheets[1];
            int nRow = 6;

            foreach (var NhanVien in lstThongTinChungNhanVien)
            {
                //sheet1.Cells[nRow, 1, nRow, 5].Merge = true;
                sheet1.Cells[nRow, 1].Value = NhanVien.MaNhanVien;
                sheet1.Cells[nRow, 2].Value = NhanVien.TenNhanVien;
                sheet1.Cells[nRow, 3].Value = NhanVien.TrinhDoHocVan;
                sheet1.Cells[nRow, 1, nRow, 8].Style.Font.Bold = true;
                nRow++;

                foreach (var ChiTiet in NhanVien.lstBangCapNhanVien)
                {
                    sheet1.Cells[nRow, 4].Value = ChiTiet.NoiDaoTao;
                    sheet1.Cells[nRow, 5].Value = ChiTiet.ChuyenNganh;
                    sheet1.Cells[nRow, 6].Value = ChiTiet.TrinhDoDaoTao;
                    nRow++;
                }

                foreach (var ChiTiet in NhanVien.lstChungChiNhanVien)
                {
                    sheet1.Cells[nRow, 7].Value = ChiTiet.TenChungChi;
                    sheet1.Cells[nRow, 8].Value = ChiTiet.LoaiChungChi;
                    nRow++;
                }
            }

            DrawTableExcel(sheet1, 5, nRow - 1, 1, 8);

            dynamic result = new DynamicObject();

            string name = "BaoCaoBangCapChungChiNhanVien.xlsx";
            //var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            var filenamegui =  "BaoCaoBangCapChungChiNhanVien.xlsx";
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;

        }

        public List<BangCapNhanVien> GetBangCapNhanVien(List<string> lstIdNhanVien, ApplicationDbContext db)
        {

            var query = (from bc in db.HRM_NhanVienBangCap
                         join td in db.HRM_dmTrinhDoDaoTao
                         on bc.IdTrinhDoDaoTao equals td.IdGuid into grp1
                         from ps1 in grp1.DefaultIfEmpty()
                         where (lstIdNhanVien.Count == 0 || lstIdNhanVien.Contains(bc.IdNhanVien))
                         select new BangCapNhanVien
                         {
                             IdNhanVien = bc.IdNhanVien,
                             ChuyenNganh = bc.ChuyenNganh,
                             NoiDaoTao = bc.NoiDaoTao,
                             TrinhDoDaoTao = ps1 != null ? ps1.Ten : ""
                         }).ToList();
            return query;
        }
        public List<ChungChiNhanVien> GetChungChiNhanVien(List<string> lstIdNhanVien, ApplicationDbContext db)
        {

            var query = (from bc in db.HRM_ChungChiNhanVien
                         join td in db.HRM_dmNhomChungChi
                         on bc.IdNhomChungChi equals td.IdGuid into grp1
                         from ps1 in grp1.DefaultIfEmpty()
                         where (lstIdNhanVien.Count == 0 || lstIdNhanVien.Contains(bc.IdNhanVien))
                         select new ChungChiNhanVien
                         {
                             IdNhanVien = bc.IdNhanVien,
                             TenChungChi = bc.TenChungChi,
                             LoaiChungChi = ps1 != null ? ps1.Ten : ""
                         }).ToList();
            return query;
        }

        #endregion

        #region Thống kê nhân viên theo phân loại




        public dynamic XuatExcelBaoCaoNhanVienTheoPhanLoai(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            dyn.CurrentPage = 0;
            List<BaoCaoNhanVienTheoPhanLoai> lstThongTinChungNhanVien = GetBaoCaoThongTinQuanHeGiaDinhNhanVien(dyn, idCurrentUser, db);

            string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoNhanVienTheoPhanLoai.xlsx");

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var sheet1 = package.Workbook.Worksheets[1];
            int nRow = 5;
            int Dem = 1;

            Dictionary<string, string> dicLoai = new Dictionary<string, string>() {
                { "VP", "Văn phòng" }
                , { "DA", "Dự án" }
            };

            foreach (var Loai in dicLoai)
            {
                sheet1.Cells[nRow, 1, nRow, 6].Merge = true;
                sheet1.Cells[nRow, 1].Value = Loai.Value;
                sheet1.Cells[nRow, 1].Style.Font.Bold = true;
                nRow++;
                Dem = 1;

                var lstNhanVien = lstThongTinChungNhanVien.Where(x => x.Loai == Loai.Key);

                foreach (var ChiTiet in lstNhanVien)
                {
                    sheet1.Cells[nRow, 1].Value = Dem.ToString();
                    sheet1.Cells[nRow, 2].Value = ChiTiet.MaNhanVien;
                    sheet1.Cells[nRow, 3].Value = ChiTiet.TenNhanVien;
                    sheet1.Cells[nRow, 4].Value = ChiTiet.PhongBan;
                    sheet1.Cells[nRow, 5].Value = ChiTiet.ChucVu;
                    sheet1.Cells[nRow, 6].Value = ChiTiet.DiaDiemLamViec;
                    nRow++;
                }
            }

            DrawTableExcel(sheet1, 5, nRow - 1, 1, 6);

            dynamic result = new DynamicObject();

            string name = "BaoCaoNhanVienTheoPhanLoai.xlsx";
            //var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            var filenamegui = "BaoCaoNhanVienTheoPhanLoai.xlsx";
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;

        }

        #endregion
    }
    public class ExportBaoCaoNhanVienHopDongViewModel
    {
        public string IdNhanVien { set; get; }
        public string IddmTrinhDo { set; get; }
        public string IdGioiTinh { set; get; }
        public DateTime? NgayBatDauLamViec { set; get; }
        public DateTime? NgaySinh { set; get; }
        public long IdBoPhanEOS { set; get; }
        public string IddmLoaiHopDong { set; get; }
        public string IddmThoiHanHopDong { set; get; }
    }
    public class BaoCaoNhanVienTheoPhanLoai
    {
        public string MaNhanVien { get; set; }

        public string TenNhanVien { get; set; }

        public string PhongBan { get; set; }

        public string ChucVu { get; set; }

        public string DiaDiemLamViec { get; set; }

        public string Loai { get; set; }

        public DateTime Created { get; set; }
    }
    public class BaoCaoThongTinNhanVien
    {
        public string MaNhanVien { get; set; }

        public string GioiTinh { get; set; }

        public string Ten { get; set; }

        public string TenDem { get; set; }

        public string Ho { get; set; }

        public string TenDayDu { get; set; }

        public string TenThuongGoi { get; set; }

        public DateTime NgaySinh { get; set; }

        public string NguyenQuan { get; set; }

        public string SoChungMinhThu { get; set; }

        public DateTime ChungMinhThuNgayCap { get; set; }

        public string ChungMinhThuNoiCap { get; set; }

        public string SoHoChieu { get; set; }

        public DateTime? HoChieuNgayCap { get; set; }

        public string HoChieuNoiCap { get; set; }

        #region Thông tin liên hệ

        public string DienThoai { get; set; }

        public string Email { get; set; }

        #endregion

        public DateTime Created { get; set; }

        public string PhongBan { get; set; }

        public string ChucVu { get; set; }

        public string TaiKhoanMangXaHoi { get; set; }

        public string HoKhauThuongTruDiaChi { get; set; }

        public string ChoOHienNayTruDiaChi { get; set; }

        public string LienHeKhanCapHoTen { get; set; }

        public string LienHeKhanIdQuanHe { get; set; }

        public string LienHeKhanQuanHe { get; set; }

        public string LienHeKhanDienThoai { get; set; }
    }

    public class BaoCaoBangCapChungChiNhanVien
    {
        public string IdNhanVien { get; set; }

        public string MaNhanVien { get; set; }

        public string TenNhanVien { get; set; }

        public string TrinhDoHocVan { get; set; }

        public DateTime Created { get; set; }

        public List<BangCapNhanVien> lstBangCapNhanVien { get; set; }

        public List<ChungChiNhanVien> lstChungChiNhanVien { get; set; }
    }

    public class BangCapNhanVien
    {
        public string IdNhanVien { get; set; }

        public string NoiDaoTao { get; set; }

        public string ChuyenNganh { get; set; }

        public string TrinhDoDaoTao { get; set; }
    }


    public class ChungChiNhanVien
    {
        public string IdNhanVien { get; set; }

        public string TenChungChi { get; set; }

        public string LoaiChungChi { get; set; }
    }
    public class BaoCaoThongTinGianHeGiaDinhNhanVien
    {

        public string IdNhanVien { get; set; }

        public string MaNhanVien { get; set; }

        public string TenNhanVien { get; set; }

        public DateTime Created { get; set; }

        public List<BaoCaoThongTinGianHeGiaDinhNhanVienChiTiet> lstChiTiet { get; set; }
    }

    public class BaoCaoThongTinGianHeGiaDinhNhanVienChiTiet
    {
        public string IdNhanVien { get; set; }

        public string QuanHe { get; set; }

        public string HoTen { get; set; }

        public int NamSinh { get; set; }

        public DateTime? NgaySinh { get; set; }

        public string NgheNghiep { get; set; }

        public string NoiLamViec { get; set; }
    }
}