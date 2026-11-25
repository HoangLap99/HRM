#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Words.NET;

namespace HRM.Api.Services.NhanVien
{
    public class FuncKyLuat : FuncBase
    {
        public FuncKyLuat(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncKyLuat> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        #region danhmucHinhThucKyLuat
        public dynamic GetListdmHinhThucKyLuat(ApplicationDbContext db)
        {
            return db.HRM_dmHinhThucKyLuat.ToList();
        }
        public dynamic SetdmHinhThucKyLuat(ApplicationDbContext db, dynamic dnm)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_dmHinhThucKyLuat dmHinhThucKyLuat = JsonConvert.DeserializeObject(dnm.ToString(), typeof(HRM_dmHinhThucKyLuat)) as HRM_dmHinhThucKyLuat;
                if (string.IsNullOrEmpty(dmHinhThucKyLuat.IdGuid))
                {
                    dmHinhThucKyLuat.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(dmHinhThucKyLuat).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                    db.Entry(dmHinhThucKyLuat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return err;
            }
            catch (Exception e)
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
                return err;
            }
        }
        public dynamic DeletedmHinhThucKyLuat(ApplicationDbContext db, dynamic dnm)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_dmHinhThucKyLuat dmHinhThucKyLuat = JsonConvert.DeserializeObject(dnm.ToString(), typeof(HRM_dmHinhThucKyLuat)) as HRM_dmHinhThucKyLuat;
                if (string.IsNullOrEmpty(dmHinhThucKyLuat.IdGuid))
                {
                    err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "không tìm thấy id");
                    return err;
                }
                else
                    db.Entry(dmHinhThucKyLuat).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                db.SaveChanges();

                return err;
            }
            catch (Exception e)
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
                return err;
            }
        }
        #endregion
        #region danhmucLoaiHinhThucKyLuatPhat
        public dynamic GetListdmLoaiHinhThucPhatKyLuat(ApplicationDbContext db)
        {
            return db.HRM_dmLoaiHinhThucPhatKyLuat.ToList();
        }
        public dynamic SetdmLoaiHinhThucPhatKyLuat(ApplicationDbContext db, dynamic dnm)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_dmLoaiHinhThucPhatKyLuat dmHinhThucKyLuat = JsonConvert.DeserializeObject(dnm.ToString(), typeof(HRM_dmLoaiHinhThucPhatKyLuat)) as HRM_dmLoaiHinhThucPhatKyLuat;
                if (string.IsNullOrEmpty(dmHinhThucKyLuat.IdGuid))
                {
                    dmHinhThucKyLuat.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(dmHinhThucKyLuat).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                    db.Entry(dmHinhThucKyLuat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return err;
            }
            catch (Exception e)
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
                return err;
            }
        }
        public dynamic DeletedmLoaiHinhThucPhatKyLuat(ApplicationDbContext db, dynamic dnm)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_dmLoaiHinhThucPhatKyLuat dmHinhThucKyLuat = JsonConvert.DeserializeObject(dnm.ToString(), typeof(HRM_dmLoaiHinhThucPhatKyLuat)) as HRM_dmLoaiHinhThucPhatKyLuat;
                if (string.IsNullOrEmpty(dmHinhThucKyLuat.IdGuid))
                {
                    err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "không tìm thấy id");
                    return err;
                }
                else
                    db.Entry(dmHinhThucKyLuat).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                db.SaveChanges();

                return err;
            }
            catch (Exception e)
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
                return err;
            }
        }
        #endregion
        public dynamic GetNextSoPhieuQuyetDinhKyLuat(ApplicationDbContext db)
        {
            dynamic dnn = new DynamicObject();
            dnn.SoQuyetDinh = GenerateNextCode(db.HRM_QuyetDinhKyLuat.Select(x => x.SoQuyTrinh), "QTQDKL");
            return dnn;
        }
        public dynamic GetDanhSachQuyetDinhKyLuat(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = TimKiem.KeyWord;
                int nCurrentPage = TimKiem.CurrentPage ?? 1;
                int PageSize = TimKiem.PageSize ?? 25;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 1;
                IEnumerable<HRM_QuyetDinhKyLuat> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyetDinhKyLuat
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyetDinhKyLuat
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
               
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.NgayHieuLuc >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayHieuLuc < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_QuyetDinhKyLuat" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }

                List<HRM_QuyetDinhKyLuat> itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in itemList)
                {
                    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                    item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                    item.KyLuongThucHienUnix = DateTimeToUnixTimeStamp(item.KyLuongThucHien);
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra --" + e.Message);
            }
        }
        public dynamic GetQuyetDinhKyLuat(ApplicationDbContext db, string Id)
        {
            try
            {
                Models.HRM_QuyetDinhKyLuat item = db.HRM_QuyetDinhKyLuat.Find(Id);
                if (item.IdBoPhan != null)
                {
                    item.listIdBoPhan = new List<long>();
                    if (item.IdBoPhan.Contains(","))
                    {
                        string[] list = item.IdBoPhan.Split(',');
                        foreach (var bp in list)
                            item.listIdBoPhan.Add(long.Parse(bp));
                    }
                    else item.listIdBoPhan.Add(long.Parse(item.IdBoPhan));
                }
                HRM_dmHinhThucKyLuat dmHinhThucKyLuat = db.HRM_dmHinhThucKyLuat.Find(item.IddmHinhThucKyLuat);
                if (dmHinhThucKyLuat != null)
                {
                    item.TendmHinhThucXuLy = dmHinhThucKyLuat.Ten;
                    if (dmHinhThucKyLuat.Ma == DefineData.edmHinhThucKyLuat.PhatTien.ToString())
                    {
                        HRM_Luong_HopDong luongNhanVien = db.HRM_Luong_HopDong.FirstOrDefault(x => x.IdNhanVien == item.IdNhanVien);
                        // 1 thang co bn ngay c
                    }
                }
                item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                item.ThoiGianXuLyUnix = DateTimeToUnixTimeStamp(item.ThoiGianXuLy);
                item.KyLuongThucHienUnix = DateTimeToUnixTimeStamp(item.KyLuongThucHien);
                List<HRM_QuyetDinhKyLuatBoPhan> quyetDinhKhenThuongBoPhans = db.HRM_QuyetDinhKyLuatBoPhan.Where(x => x.IdQuyetDinhKyLuat == Id).ToList();
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                foreach (var quyetDinh in quyetDinhKhenThuongBoPhans)
                {
                    BoPhan nv = listBoPhan.FirstOrDefault(x => x.Id == quyetDinh.IdBoPhanEOS);
                    if (nv != null)
                        quyetDinh.TenBoPhan = nv.TenBoPhan;


                }
                item.lstBoPhan = quyetDinhKhenThuongBoPhans;

                List<HRM_QuyetDinhKyLuatNhanVien> quyetDinhKhenThuongNhanViens = db.HRM_QuyetDinhKyLuatNhanVien.Where(x => x.IdQuyetDinhKyLuat == Id).ToList();
                var listNhanVien = db.HRM_NhanVien.ToList();
                foreach (var quyetDinh in quyetDinhKhenThuongNhanViens)
                {
                    HRM_NhanVien nv = listNhanVien.FirstOrDefault(x => x.IdGuid == quyetDinh.IdNhanVien);
                    if (nv != null)
                    {
                        quyetDinh.TenNhanVien = nv.TenDayDu;
                        quyetDinh.MaNhanVien = nv.MaNhanVien;
                    }
                    var ChucDanh = listChucDanh.FirstOrDefault(x => x.Id == nv.IdChucDanh);
                    if (ChucDanh != null)
                        quyetDinh.TenChucDanh = ChucDanh.Ten;
                    BoPhan bp = listBoPhan.FirstOrDefault(x => x.Id == nv.IdBoPhanEOS);
                    if (bp != null)
                        quyetDinh.TenBoPhan = bp.TenBoPhan;
                }
                item.lstNhanVien = quyetDinhKhenThuongNhanViens;
                item.lstFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_QuyetDinhKyLuat.ToString(), item.IdGuid);

                return item;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra --" + e.Message);
            }
        }
        public dynamic GetLanKyLuatNhanVien(string IdNhanVien, int Nam, ApplicationDbContext db)
        {
            int LanKyLuat = 0;
            var lankyLuats = from a in db.HRM_QuyetDinhKyLuat
                             join b in db.HRM_QuyetDinhKyLuatNhanVien on a.IdGuid equals b.IdQuyetDinhKyLuat
                             where a.isKetThuc == true && a.NgayHieuLuc.Value.Year == Nam && b.IdNhanVien == IdNhanVien
                             select b.IdGuid;
            LanKyLuat = lankyLuats.Count() + 1;
            return LanKyLuat;
        }
        public dynamic GetdanhSachKyLuatNhanVienNam(dynamic dyn, ApplicationDbContext db)
        {
            int CurrentPage = dyn.CurrentPage ?? 0;
            int PageSize = dyn.PageSize ?? 15;
            long IdCongTyEOS = dyn.IdCongTyEOS ?? 0;
            string KeyWord = dyn.KeyWord;
            int Nam = dyn.Nam ?? 0;

            var query = from a in db.HRM_QuyetDinhKyLuat
                        join b in db.HRM_QuyetDinhKyLuatNhanVien on a.IdGuid equals b.IdQuyetDinhKyLuat
                        join c in db.HRM_NhanVien on b.IdNhanVien equals c.IdGuid
                        where a.isKetThuc
                        select new { a, b, c };

            if (!string.IsNullOrEmpty(KeyWord))
                query = query.Where(x => x.a.SoQuyTrinh.Contains(KeyWord) || x.c.TenDayDu.Contains(KeyWord));
            if (Nam > 0)
                query = query.Where(x => x.a.NgayHieuLuc != null && x.a.NgayHieuLuc.Value.Year == Nam);
            if (IdCongTyEOS > 0)
                query = query.Where(x => x.a.IdCongTyEOS == IdCongTyEOS);

            var orderedQuery = query
                .OrderBy(x => x.c.MaNhanVien)
                .Select(x => new KyLuatNhanVienViewModel
                {
                    SoQuyetDinh = x.a.SoQuyetDinh,
                    SoQuyTrinh = x.a.SoQuyTrinh,
                    TenDayDu = x.c.TenDayDu,
                    IddmHinhThucKyLuat = x.a.IddmHinhThucKyLuat,
                    NgayKy = x.a.NgayKy ?? default,
                    NgayHieuLuc = x.a.NgayHieuLuc ?? default,
                    GhiChu = x.b.GhiChu,
                    NguyenNhan = x.a.NguyenNhan,
                    SoTien = x.b.SoTien,
                    PhanTramPhat = x.b.PhanTramPhat,
                    KyLuatLan = x.b.KyLuatLan,
                    MaNhanVien = x.c.MaNhanVien,
                    IdCongTyEOS = x.a.IdCongTyEOS,
                    IdBoPhanEOS = x.b.IdBoPhanEOS,
                    IdChucVuEOS = x.b.IdChucVuEOS,
                    IdNhanVien = x.b.IdNhanVien
                });

            //var dmHinhThucKyLuats = db.HRM_dmHinhThucKyLuat.ToList();
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var CongTys = _CallWebService.GetDanhSachCongTy(db);
            var ChucVus = _CallWebService.GetDanhSachChucDanh(db);

            PagingInfor paging = null;
            List<KyLuatNhanVienViewModel> itemList = CurrentPage == 0
                ? orderedQuery.ToList()
                : Paginate(orderedQuery, CurrentPage, PageSize, out paging);

            var companySource = CurrentPage == 0 ? BoPhans : CongTys;
            foreach (var item in itemList)
            {
                if (CurrentPage != 0)
                    item.SoQuyetDinh = item.SoQuyTrinh;
                item.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKy);
                item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                item.TenBoPhanEOS = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                item.TenCongTyEOS = companySource.Where(r => r.Id == item.IdCongTyEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                item.TenChucVuEOS = ChucVus.Where(r => r.Id == item.IdChucVuEOS).Select(r => r.Ten).FirstOrDefault();
                //var dmHinhThucKyLuat = dmHinhThucKyLuats.Where(r => r.IdGuid == item.IddmHinhThucKyLuat).FirstOrDefault();
                //if (dmHinhThucKyLuat.Ma != "PHATTIEN")
                //    item.TenHinhThucKyLuat = dmHinhThucKyLuats.Where(r => r.IdGuid == item.IddmHinhThucKyLuat).Select(r => r.Ten).FirstOrDefault();
                //else
                //{
                if (item.PhanTramPhat > 0)
                    item.TenHinhThucKyLuat = "Khấu trừ " + item.PhanTramPhat + "% lương";
                else
                    item.TenHinhThucKyLuat = "Khấu trừ số tiền " + item.SoTien.ToString("N0");
                //}
            }

            if (CurrentPage == 0)
                return itemList;

            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = itemList;
            return dnm;
        }
        public dynamic ExportSachKyLuatNhanVienNam(dynamic dyn, ApplicationDbContext db)
        {
            List<KyLuatNhanVienViewModel> itemList = GetdanhSachKyLuatNhanVienNam(dyn, db);
            long IdCongTyEOS = dyn.IdCongTyEOS ?? 0;
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoQuyetDinhKyLuat.xlsx");
            int Nam = dyn.Nam ?? 0;
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets[1];
            s.Close();
            int nRow = 4;
            int STT = 1;

            sheet1.Cells[1, 1].Value = " DANH SÁCH CBCNV BỊ XỬ LÝ KỶ LUẬT NĂM " + Nam;
            if (IdCongTyEOS > 0)
            {
                var Congty = _CallWebService.GetBoPhan(db, IdCongTyEOS);
                if (Congty != null && Congty.TenBoPhan != null)
                    sheet1.Cells[2, 1].Value = "Công ty: " + Congty.TenBoPhan;
            }
            var listCongTy = itemList.GroupBy(r => r.IdCongTyEOS).Select(r => r.Key).ToList();
            foreach (var itemIdCty in listCongTy)
            {
                List<KyLuatNhanVienViewModel> listNhanVienCongTy = itemList.Where(r => r.IdCongTyEOS == itemIdCty).OrderBy(r => r.MaNhanVien).ToList();
                foreach (var item in listNhanVienCongTy)
                {
                    sheet1.Cells[nRow, 1].Value = STT;
                    sheet1.Cells[nRow, 2].Value = item.TenDayDu;
                    sheet1.Cells[nRow, 3].Value = item.TenChucVuEOS;
                    sheet1.Cells[nRow, 4].Value = item.TenBoPhanEOS;
                    sheet1.Cells[nRow, 5].Value = item.KyLuatLan; ;
                    sheet1.Cells[nRow, 6].Value = item.TenHinhThucKyLuat;
                    sheet1.Cells[nRow, 7].Value = item.NguyenNhan;
                    sheet1.Cells[nRow, 8].Value = item.SoQuyetDinh + " " + item.NgayHieuLuc.ToString("dd/MM/yyyy");
                    nRow++;
                    STT++;
                }
                int nRowMerger = nRow - listNhanVienCongTy.Count();
                sheet1.Cells[nRowMerger, 9, nRow - 1, 9].Merge = true;
                sheet1.Cells[nRowMerger, 9].Value = listNhanVienCongTy[0].TenCongTyEOS;
            }
            DrawTableExcel(sheet1, 4, nRow - 1, 1, 9);
            DateTime dateTimeNow = DateTime.Now;
            string NgayThangNam = "Hải Dương, ngày " + dateTimeNow.Day + " tháng " + dateTimeNow.Month + " năm " + dateTimeNow.Year;
            sheet1.Cells[nRow, 7, nRow, 9].Merge = true;
            sheet1.Cells[nRow, 7].Value = NgayThangNam;
            sheet1.Cells[nRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            dynamic result = new DynamicObject();

            string name = "BaoCaoQuyetDinhKyLuat.xlsx";
            var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            string pathresult =PathUploads+ filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;
        }
        public dynamic SetQuyetDinhKyLuat(ApplicationDbContext db, HRM_QuyetDinhKyLuat model)
        {
            return ExecuteInTransaction(db, () =>
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                var QuyTrinhdb = db.HRM_QuyetDinhKyLuat.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                    return err;

                if (!UpdateQuyetDinhKyLuat(db, model, err, eQuyTrinh_LoaiHam.Set))
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");

                if (!SetSauHamUpdate(db, QuyTrinhdb, model, err))
                    return err;

                err.Value = model;
                return err;
            });
        }
        public bool UpdateQuyetDinhKyLuat(ApplicationDbContext db, Models.HRM_QuyetDinhKyLuat item, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (item.listIdBoPhan != null && item.listIdBoPhan.Count > 0)
                    item.IdBoPhan = string.Join(",", item.listIdBoPhan);
                if (item.NgayKyUnix > 0)
                    item.NgayKy = UnixTimeStampToDateTime(item.NgayKyUnix);
                if (item.NgayHieuLucUnix > 0)
                    item.NgayHieuLuc = UnixTimeStampToDateTime(item.NgayHieuLucUnix);
                if (item.KyLuongThucHienUnix > 0)
                    item.KyLuongThucHien = UnixTimeStampToDateTime(item.KyLuongThucHienUnix);
                if (item.ThoiGianXuLyUnix > 0)
                    item.ThoiGianXuLy = UnixTimeStampToDateTime(item.ThoiGianXuLyUnix);
                if (string.IsNullOrEmpty(item.IdGuid))
                {
                    item.IdDuAn = item.IdCongTyEOS;
                    if (string.IsNullOrEmpty(item.eAction))
                        item.eAction = DefineData.eAction.QUYTRINHQUYETDINHKYLUAT.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, item, err))
                        return false;

                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                }
                else
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                var QuyetDinhKyLuatBoPhans = db.HRM_QuyetDinhKyLuatBoPhan.Where(x => x.IdQuyetDinhKyLuat == item.IdGuid).ToList();
                db.HRM_QuyetDinhKyLuatBoPhan.RemoveRange(QuyetDinhKyLuatBoPhans);


                var QuyetDinhKyLuatNhanViens = db.HRM_QuyetDinhKyLuatNhanVien.Where(x => x.IdQuyetDinhKyLuat == item.IdGuid).ToList();
                db.HRM_QuyetDinhKyLuatNhanVien.RemoveRange(QuyetDinhKyLuatNhanViens);
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == item.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                if (item.listIdBoPhan != null && item.listIdBoPhan.Count > 0)
                {
                    foreach (var Id in item.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = item.IdGuid;
                        bp.IdBoPhan = Id;
                        bp.TableName = nameof(HRM_QuyetDinhKyLuat);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                foreach (var BoPhan in item.lstNhanVien)
                {
                    BoPhan.IdGuid = Guid.NewGuid().ToString();
                    BoPhan.IdQuyetDinhKyLuat = item.IdGuid;
                    BoPhan.IdBoPhanEOS = BoPhan.IdBoPhan;
                    BoPhan.IdChucVuEOS = BoPhan.IddmChucVu;
                    db.Entry(BoPhan).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                }
                if (item.lstBoPhan != null)
                {
                    foreach (var BoPhan in item.lstBoPhan)
                    {
                        BoPhan.IdGuid = Guid.NewGuid().ToString();
                        BoPhan.IdQuyetDinhKyLuat = item.IdGuid;

                        db.Entry(BoPhan).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }
                }
                if (item.lstFileDinhKem != null)
                {
                    foreach (var itemFile in item.lstFileDinhKem)
                    {
                        SetFileDinhKem(db, itemFile, DefineData.eTable.HRM_QuyetDinhKyLuat, item.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, item, eLoaiHam, err))
                    return false;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateQuyetDinhKyLuat data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return false;
            }
            return true;
        }
        public dynamic DeleteQuyetDinhKyLuat(ApplicationDbContext db, string Id)
        {
            return ExecuteInTransaction(db, () =>
            {
                var QuyetDinhdb = db.HRM_QuyetDinhKyLuat.AsNoTracking().FirstOrDefault(x => x.IdGuid == Id);
                if (QuyetDinhdb == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy dữ liệu");

                db.Entry(QuyetDinhdb).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                foreach (var item in db.HRM_QuyetDinhKyLuatBoPhan.AsNoTracking().Where(x => x.IdQuyetDinhKyLuat == Id).ToList())
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                foreach (var item in db.HRM_QuyetDinhKyLuatNhanVien.AsNoTracking().Where(x => x.IdQuyetDinhKyLuat == Id).ToList())
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa thành công");
            });
        }
        public MessageReturn ChuyenTiepQuyetDinhKyLuat(ApplicationDbContext db, dynamic dnm)
        {
            return ExecuteInTransaction(db, () =>
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_QuyetDinhKyLuat QuyTrinh = JsonConvert.DeserializeObject(dnm.ToString(), typeof(Models.HRM_QuyetDinhKyLuat)) as Models.HRM_QuyetDinhKyLuat;
                HRM_QuyetDinhKyLuat pcuDatabase = db.HRM_QuyetDinhKyLuat.AsNoTracking().FirstOrDefault(r => r.IdGuid == QuyTrinh.IdGuid);
                if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                    return err;

                if (!UpdateQuyetDinhKyLuat(db, QuyTrinh, err, eQuyTrinh_LoaiHam.ChuyenTiep))
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, err.sError, QuyTrinh.IdGuid);

                if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    return err;

                if (QuyTrinh.isKetThuc == true)
                {
                    List<HRM_Luong_KhenThuongKyLuat> lstKhenThuong = new List<HRM_Luong_KhenThuongKyLuat>();
                    foreach (var itemNV in QuyTrinh.lstNhanVien)
                    {
                        LogLichSuNhanVien(db, itemNV.IdNhanVien, QuyTrinh.IdGuid, "HRM_QuyetDinhKyLuat", DefineData.eAction.QUYTRINHQUYETDINHKYLUAT.ToString(), QuyTrinh.NgayKyUnix, "Quyết định kỷ luật");
                        if (itemNV.SoTien > 0 || itemNV.PhanTramPhat > 0)
                            lstKhenThuong.Add(new HRM_Luong_KhenThuongKyLuat(QuyTrinh, itemNV));
                    }
                    db.HRM_Luong_KhenThuongKyLuat.AddRange(lstKhenThuong);
                }

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            });
        }
        public dynamic KhongDuyetQuyetDinhKyLuat(ApplicationDbContext db, dynamic dnm)
        {
            return ExecuteInTransaction(db, () =>
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                Models.HRM_QuyetDinhKyLuat QuyTrinh = JsonConvert.DeserializeObject(dnm.ToString(), typeof(Models.HRM_QuyetDinhKyLuat)) as Models.HRM_QuyetDinhKyLuat;
                HRM_QuyetDinhKyLuat pcuDatabase = db.HRM_QuyetDinhKyLuat.AsNoTracking().FirstOrDefault(r => r.IdGuid == QuyTrinh.IdGuid);
                if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                    return err;

                if (!UpdateQuyetDinhKyLuat(db, QuyTrinh, err, eQuyTrinh_LoaiHam.KhongDuyet))
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, err.sError, QuyTrinh.IdGuid);

                if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                {
                    err.Error = DefineData.eErrorMessage.LoiCapNhat;
                    return err;
                }

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            });
        }
        public dynamic ExportKyLuat(string IdQuyTrinh, string IdNhanVien, ApplicationDbContext db)
        {
            HRM_QuyetDinhKyLuat quytrinh = db.HRM_QuyetDinhKyLuat.Find(IdQuyTrinh);
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = "";
            var dmKyLuat = db.HRM_dmHinhThucKyLuat.Find(quytrinh.IddmHinhThucKyLuat);
            if (dmKyLuat.Ma != "PHATTIEN")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuyetDinhKyLuat.docx");
            else
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuyetDinhKyLuat_PhatTien.docx");
            string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);

            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var CongTy = BoPhans.Find(x => x.Id == quytrinh.IdCongTyEOS);

            HRM_QuyetDinhKyLuatNhanVien nhanvienkyLuat = db.HRM_QuyetDinhKyLuatNhanVien.Where(r => r.IdNhanVien == IdNhanVien && r.IdQuyetDinhKyLuat == IdQuyTrinh).FirstOrDefault();
            var NguoiLaoDong = db.HRM_NhanVien.Find(IdNhanVien);
            doc.ReplaceText("@CongTy", CongTy.TenBoPhan);

            string NgayThang = "ngày " + quytrinh.NgayKy.Value.Day + " tháng " + quytrinh.NgayKy.Value.Month + " năm " + quytrinh.NgayKy.Value.Year;
            doc.ReplaceText("@NgayThangNam", NgayThang);
            doc.ReplaceText("@ngayky ", quytrinh.NgayKy.Value.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@SoQuyetDinh", quytrinh.SoQuyetDinh);
            doc.ReplaceText("@LanThu", nhanvienkyLuat.KyLuatLan.ToString());
            string TenNguoiKy = db.HRM_NhanVien.Where(r => r.IdGuid == quytrinh.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
            doc.ReplaceText("@@TenNguoiKy", TenNguoiKy);
            var listPhongBanLienquan = db.HRM_QuyetDinhKyLuatBoPhan.Where(r => r.IdQuyetDinhKyLuat == IdQuyTrinh).ToList();
            string phongBanlienQuan = "";

            foreach (var item in listPhongBanLienquan)
            {
                var BoPhanItem = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
                phongBanlienQuan = phongBanlienQuan + BoPhanItem.TenBoPhan + "; ";
            }
            doc.ReplaceText("@phongBanlienQuan", phongBanlienQuan);
            if (nhanvienkyLuat.GhiChu == null)
                nhanvienkyLuat.GhiChu = "";
            doc.ReplaceText("@LyDo", nhanvienkyLuat.GhiChu);

            var ChucVuNLD = _CallWebService.GetChucVu(db, nhanvienkyLuat.IdChucVuEOS);

            doc.ReplaceText("@ChucVu ", ChucVuNLD.TenChucVu);


            doc.ReplaceText("@NhanVien", NguoiLaoDong.TenDayDu);
            if (dmKyLuat.Ma != "PHATTIEN")
            {
                doc.ReplaceText("@dmKyLuat", dmKyLuat.Ten);
                doc.ReplaceText("@NgayQuyetDinh", quytrinh.NgayHieuLuc.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                string SoNgayCong = "";
                if (nhanvienkyLuat.SoTien > 0 && nhanvienkyLuat.SoTien > 1000)
                    SoNgayCong = nhanvienkyLuat.SoTien.ToString("N0") + " đ";
                else
                {
                    if (nhanvienkyLuat.SoTien > 0)
                        SoNgayCong = nhanvienkyLuat.SoTien + " ngày công";
                    else
                        SoNgayCong = nhanvienkyLuat.PhanTramPhat + " lương";
                }
                doc.ReplaceText("@SoNgayCong", SoNgayCong);
                doc.ReplaceText("@KyLuong", quytrinh.KyLuongThucHien.ToString("MM/yyyy"));
            }
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QuyetDinhKyLuat.docx")
                                + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
    }

    public class KyLuatNhanVienViewModel
    {
        public string GhiChu { set; get; }
        public string SoQuyTrinh { set; get; }
        public string SoQuyetDinh { set; get; }
        public string TenDayDu { set; get; }
        public string IddmHinhThucKyLuat { set; get; }
        public string TenHinhThucKyLuat { set; get; }
        public DateTime NgayKy { set; get; }
        public double NgayKyUnix { set; get; }
        public DateTime NgayHieuLuc { set; get; }
        public double NgayHieuLucUnix { set; get; }
        public string NguyenNhan { set; get; }
        public double SoTien { set; get; }
        public double PhanTramPhat { set; get; }
        public int KyLuatLan { set; get; }
        public long IdCongTyEOS { set; get; }
        public string TenCongTyEOS { set; get; }
        public long IdBoPhanEOS { set; get; }
        public string TenBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public string TenChucVuEOS { set; get; }
        public string IdNhanVien { set; get; }
        public string MaNhanVien { set; get; }


    }
}
