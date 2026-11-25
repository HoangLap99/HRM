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
    public class FuncBaoCaoHopDong : FuncBase
    {
        public FuncBaoCaoHopDong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBaoCaoHopDong> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
     //   DefineData.eAction eAction = DefineData.eAction.BAOCAOHOPDONGLAODONG;
        public class BaoCaoHopDongLaoDong
        {
            public string IdLoaiHopDong { get; set; }
            public string LoaiHopDong { get; set; }

            public List<BaoCaoHopDongLaoDongChiTiet> lstChiTiet { get; set; }
        }

        public class BaoCaoHopDongLaoDongChiTiet
        {
            public string IdLoaiHopDong { get; set; }

            public string SoHopDong { get; set; }

            public DateTime NgayKy { get; set; }

            public string ThoiHanHopDong { get; set; }

            public DateTime? NgayHetHan { get; set; }

            public string MaNhanVien { get; set; }

            public string TenNhanVien { get; set; }

            public string PhongBan { get; set; }

            public string ChucVu { get; set; }

            public string TrangThai { get; set; }

            public string TenTrangThai
            {
                get
                {
                    if (DefineData.dicTenTrangThaiHopDong.ContainsKey(TrangThai))
                        return DefineData.dicTenTrangThaiHopDong[TrangThai];
                    return "";
                }
            }

            public DateTime Created { get; set; }
        }

        //public dynamic GetBaoCaoHopDongLaoDong(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        //{


        //    #region Quyền
        //    int QuyenView = GetQuyenView(eAction.ToString(), idCurrentUser);
        //    if (QuyenView == (int)DefineData.eGioiHan.NONE)
        //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
        //    #endregion

        //    DateTime dateminsql = dungchung.UnixTimeStampToDateTime(0);
        //    int CurrentPage = dyn.CurrentPage == null ? 0 : dyn.CurrentPage;
        //    int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
        //    string KeyWord = dyn.KeyWord == null ? "" : dyn.KeyWord;

        //    var lstLoaiHopDong = db.HRM_DM_LoaiHopDong.Select(x=>new BaoCaoHopDongLaoDong {
        //        IdLoaiHopDong = x.IdGuid,
        //        LoaiHopDong = x.Ten,
        //    }).ToList();

        //    var lstHopDong = (from hd in db.HRM_HopDong
        //                      join nv in db.HRM_NhanVien
        //                      on hd.IdNhanVien equals nv.IdGuid
        //                      join cv in db.HRM_DM_ChucVu
        //                      on hd.IdChucVu equals cv.IdGuid
        //                      //join bp in db.HRM_DM_BoPhan
        //                      //on hd.IdPhongBan equals bp.IdGuid into grp1
        //                     // from ps1 in grp1.DefaultIfEmpty()
        //                      //join da in db.HRM_DuAn
        //                      //on hd.IdDuAn equals da.IdGuid into grp2
        //                      from ps2 in grp2.DefaultIfEmpty()
        //                      join th in db.HRM_DM_ThoiHanHopDong
        //                      on hd.IdThoiHanHopDong equals th.IdGuid into grp3
        //                      from ps3 in grp3.DefaultIfEmpty()
        //                      orderby hd.Created
        //                      select new BaoCaoHopDongLaoDongChiTiet
        //                      {
        //                          ChucVu = cv.Ten,
        //                          IdLoaiHopDong = hd.IdLoaiHopDong,
        //                          MaNhanVien = nv.MaNhanVien,
        //                          NgayHetHan = hd.NgayHetHan,
        //                          NgayKy = hd.NgayKy,
        //                      //    PhongBan = ps1 != null ? ps1.TenBoPhan : (ps2 != null ? ps2.Ten : ""),
        //                          SoHopDong = hd.SoHopDong,
        //                          TenNhanVien = nv.TenDayDu,
        //                          ThoiHanHopDong = ps3 != null ? ps3.Ten : "",
        //                          TrangThai = hd.TrangThai,
        //                          Created = hd.Created
        //                      }).ToList();

        //    foreach(var itemLoai in lstLoaiHopDong)
        //    {
        //        itemLoai.lstChiTiet = lstHopDong.Where(x => x.IdLoaiHopDong == itemLoai.IdLoaiHopDong).ToList();
        //    }

        //    return lstLoaiHopDong;
        //}

        //    public dynamic XuatExcelBaoCaoHopDongLaoDong(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        //    {
        //        dyn.CurrentPage = 0;
        //        List<BaoCaoHopDongLaoDong> lstLoaiHopDong = GetBaoCaoHopDongLaoDong(dyn, idCurrentUser, db);

        //        string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoHopDongLaoDong.xlsx");

        //        Stream s = File.OpenRead(pathTemplate);
        //        ExcelPackage package = new ExcelPackage(s);
        //        s.Close();
        //        var sheet1 = package.Workbook.Worksheets[1];
        //        int nRow = 5;
        //        int Dem = 1;

        //        foreach (var Loai in lstLoaiHopDong)
        //        {
        //            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
        //            sheet1.Cells[nRow, 1].Value = Loai.LoaiHopDong;
        //            sheet1.Cells[nRow, 1].Style.Font.Bold = true;
        //            nRow++;
        //            Dem = 1;

        //            foreach (var ChiTiet in Loai.lstChiTiet)
        //            {
        //                sheet1.Cells[nRow, 1].Value = Dem.ToString();
        //                sheet1.Cells[nRow, 2].Value = ChiTiet.SoHopDong;
        //                sheet1.Cells[nRow, 3].Value = ChiTiet.NgayKy.ToString("dd/MM/yyyy");
        //                sheet1.Cells[nRow, 4].Value = ChiTiet.ThoiHanHopDong;
        //                sheet1.Cells[nRow, 5].Value = ChiTiet.NgayHetHan.Value.ToString("dd/MM/yyyy");
        //                sheet1.Cells[nRow, 6].Value = ChiTiet.MaNhanVien;
        //                sheet1.Cells[nRow, 7].Value = ChiTiet.TenNhanVien;
        //                sheet1.Cells[nRow, 8].Value = ChiTiet.PhongBan;
        //                sheet1.Cells[nRow, 9].Value = ChiTiet.ChucVu;
        //                sheet1.Cells[nRow, 10].Value = ChiTiet.TenTrangThai;
        //                nRow++;
        //                Dem++;
        //            }
        //        }

        //        DrawTableExcel(sheet1, 5, nRow - 1, 1, 10);

        //        dynamic result = new DynamicObject();

        //        string name = "BaoCaoHopDongLaoDong.xlsx";
        //        var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
        //        string pathresult = PathUploads + filenamegui;
        //        File.WriteAllBytes(pathresult, package.GetAsByteArray());

        //        HDFile newfile = new HDFile();
        //        newfile.Name = filenamegui;
        //        newfile.NameLocal = name;

        //        newfile.Url = "?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
        //                    + "&path=" + dungchung.Base64Encode(pathresult);
        //        result.file = newfile;
        //        return result;

        //    }
        //}
    }
}