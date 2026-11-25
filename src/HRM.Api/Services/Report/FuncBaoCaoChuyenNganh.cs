#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using System;

namespace HRM.Api.Services.BaoCao
{
    public class FuncBaoCaoChuyenNganh : FuncBase
    {
        public FuncBaoCaoChuyenNganh(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBaoCaoChuyenNganh> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
       // DefineData.eAction eAction = DefineData.eAction.BAOCAOCHUYENNGANH;
        public class BaoCaoTongHopChuyenNganh
        {
            public string NoiDaoTao { get; set; }

            public string ChuyenNganh { get; set; }
            public string TrinhDoDaoTao { get; set; }
            public string MaNhanVien { get; set; }

            public string TenNhanVien { get; set; }

            public string PhongBan { get; set; }

            public string ChucVu { get; set; }

            public DateTime Created { get; set; }
        }

        //public dynamic GetBaoCaoTongHopChuyenNganh(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
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

        //    DateTime Now = DateTime.Now.Date;

        //    var query = (from bc in db.HRM_NhanVienBangCap
        //                 join td in db.HRM_DM_TrinhDoDaoTao
        //                  on bc.IdTrinhDoDaoTao equals td.IdGuid into grp1
        //                  from ps1 in grp1.DefaultIfEmpty()
        //                 join nv in db.HRM_NhanVien 
        //                 on bc.IdNhanVien equals nv.IdGuid
        //                 join link in db.HRM_Link_NhanVien_BoPhan_ChucVu
        //                 on nv.IdGuid equals link.IdNhanVien
        //                 join cv in db.HRM_DM_ChucVu
        //                 on link.IdChucVu equals cv.IdGuid
        //                 //join bp in db.HRM_DM_BoPhan
        //                 //on link.IdBoPhan equals bp.IdGuid
        //                 where link.NgayBatDau <= Now && (link.NgayKetThuc == dateminsql || link.NgayKetThuc >= Now)
        //                 select new BaoCaoTongHopChuyenNganh
        //                 {
        //                    ChucVu = cv.Ten,
        //                    ChuyenNganh = bc.ChuyenNganh,
        //                    MaNhanVien = nv.MaNhanVien,
        //                    NoiDaoTao = bc.NoiDaoTao,
        //                   // PhongBan = bp.TenBoPhan,
        //                    TenNhanVien = nv.TenDayDu,
        //                    TrinhDoDaoTao = ps1 != null ? ps1.Ten : "",
        //                     Created = nv.Created
        //                 });

        //    if (!string.IsNullOrEmpty(KeyWord))
        //    {
        //        KeyWord = KeyWord.ToLower();
        //        query = query.Where(x => x.MaNhanVien.ToLower().Contains(KeyWord) || x.TenNhanVien.ToLower().Contains(KeyWord)
        //        || x.ChuyenNganh.ToLower().Contains(KeyWord)
        //        || x.ChucVu.ToLower().Contains(KeyWord)
        //        || x.NoiDaoTao.ToLower().Contains(KeyWord)
        //        || x.TrinhDoDaoTao.ToLower().Contains(KeyWord)
        //        );
        //    }

        //    if (CurrentPage == 0)
        //    {
        //        var lstReturn = query.OrderByDescending(x => x.Created).ToList();
        //        return lstReturn;
        //    }
        //    else
        //    {
        //        dynamic result = new DynamicObject();

        //        int count = query.Count();
        //        var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();
        //        result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
        //        result.items = listResult;

        //        return result;
        //    }
        //}

        //public dynamic XuatExcelBaoCaoTongHopChuyenNganh(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        //{
        //    dyn.CurrentPage = 0;
        //    List<BaoCaoTongHopChuyenNganh> lstThongTinChungNhanVien = GetBaoCaoTongHopChuyenNganh(dyn, idCurrentUser, db);

        //    string pathTemplate = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoTongHopChuyenNganh.xlsx");

        //    Stream s = File.OpenRead(pathTemplate);
        //    ExcelPackage package = new ExcelPackage(s);
        //    s.Close();
        //    var sheet1 = package.Workbook.Worksheets[1];
        //    int nRow = 5;
        //    int Dem = 1;

        //    foreach (var NhanVien in lstThongTinChungNhanVien)
        //    {

        //        sheet1.Cells[nRow, 1].Value = Dem.ToString();
        //        sheet1.Cells[nRow, 2].Value = NhanVien.NoiDaoTao;
        //        sheet1.Cells[nRow, 3].Value = NhanVien.ChuyenNganh;
        //        sheet1.Cells[nRow, 4].Value = NhanVien.TrinhDoDaoTao;
        //        sheet1.Cells[nRow, 5].Value = NhanVien.MaNhanVien;
        //        sheet1.Cells[nRow, 6].Value = NhanVien.TenNhanVien;
        //        sheet1.Cells[nRow, 7].Value = NhanVien.PhongBan;
        //        sheet1.Cells[nRow, 8].Value = NhanVien.ChucVu;
        //        nRow++;
        //        Dem++;

        //    }

        //    DrawTableExcel(sheet1, 5, nRow - 1, 1, 8);

        //    dynamic result = new DynamicObject();

        //    string name = "BaoCaoTongHopChuyenNganh.xlsx";
        //    var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
        //    string pathresult = PathUploads + filenamegui;
        //    File.WriteAllBytes(pathresult, package.GetAsByteArray());

        //    HDFile newfile = new HDFile();
        //    newfile.Name = filenamegui;
        //    newfile.NameLocal = name;

        //    newfile.Url = "?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
        //                + "&path=" + dungchung.Base64Encode(pathresult);
        //    result.file = newfile;
        //    return result;

        //}
    
    
    }
}