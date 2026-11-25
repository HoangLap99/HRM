#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using HRM.Api.Entity;
using HRM.Api.Helper;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.FunctionCompilers;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace HRM.Api.Services.TuyenDung
{
    public class FuncKeHoachTuyenDung : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncKeHoachTuyenDung(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncKeHoachTuyenDung> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        #region nhu cầu tuyển dụng
        public dynamic GetDanhSachNhuCauTuyenDungByCongTy(ApplicationDbContext db, dynamic data)
        {
            TimKiem itemTiemKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                DateTime dtTuNgay = UnixTimeStampToDateTime(itemTiemKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(itemTiemKiem.DenNgay ?? 0);
                List<HRM_TuyenDung_NhuCau_ChiTiet> listResult = new List<HRM_TuyenDung_NhuCau_ChiTiet>();
                var list = from a in db.HRM_DeNghiTuyenDung
                           where (itemTiemKiem.IdCongTy == null || itemTiemKiem.IdCongTy == 0 || a.IdCongTy == itemTiemKiem.IdCongTy) && a.isKetThuc == true

                           select a;

                if (itemTiemKiem.TuNgay > 0)
                    list = list.Where(a => a.NgayYeuCau != null && a.NgayYeuCau >= dtTuNgay);
                if (itemTiemKiem.DenNgay > 0)
                    list = list.Where(a => a.NgayYeuCau != null && a.NgayYeuCau <= dtDenNgay);
                var listItem = list.ToList();
                if (listItem != null && listItem.Count > 0)
                {
                    List<BoPhan> listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    List<ChucVu> listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    foreach (var item in listItem)
                    {
                        item.NoiDung = item.LyDo;
                        var BP = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhan);
                        if (BP != null)
                            item.TenBoPhan = BP.TenBoPhan;
                        var CD = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (CD != null)
                            item.TenChucVu = CD.TenChucVu;
                    }

                }
                return listItem;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }

        public dynamic GetNextSoQuyTrinhNhuCauTuyenDung(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "NCTD",
                db.HRM_TuyenDung_NhuCau.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault(),
                digits: 4));
        public dynamic LayId(ApplicationDbContext db)
        {

            List<string> strings = db.HRM_TuyenDung_NhuCau.Select(a => a.IdGuid).Distinct().ToList();
            string b = string.Join(" or IdTable=", strings);
            return b;
        }
        public dynamic GetListQuyTrinhNhuCauTuyenDung(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_TuyenDung_NhuCau> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_TuyenDung_NhuCau
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_TuyenDung_NhuCau
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Ngay >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Ngay < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                //if (TimKiem.IdCongTy > 0)
                //{
                //    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                //}
                var itemList = Paginate(result.AsNoTracking().Distinct(), nCurrentPage, PageSize, out PagingInfor paging);

                List<BoPhan> BoPhans = _CallWebService.GetDanhSachCongTy(db);


                foreach (HRM_TuyenDung_NhuCau item in itemList)
                {
                    item.NgayUnix = dungchung.DateTimeToUnixTimeStamp(item.Ngay);
                    BoPhan duAn = BoPhans.Find(x => x.Id == item.IdCongTyEOS);
                    if (duAn != null)
                        item.TenCongty = duAn.TenBoPhan;

                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }


        public dynamic GetQuyTrinhNhuCauTuyenDung(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");

                HRM_TuyenDung_NhuCau QuyTrinh = db.HRM_TuyenDung_NhuCau.FirstOrDefault(a => a.IdGuid == Id);
                if (!GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))
                    return err;
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                QuyTrinh.NgayUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.Ngay);
                QuyTrinh.listChiTiet = new List<HRM_TuyenDung_NhuCau_ChiTiet>();
                QuyTrinh.listChiTiet = db.HRM_TuyenDung_NhuCau_ChiTiet.Where(a => a.IdTuyenDungNhuCau == Id).ToList();
                if (QuyTrinh.listChiTiet.Count > 0)
                {
                    foreach (var item in QuyTrinh.listChiTiet)
                        item.NgayDuKienDiLamUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDuKienDiLam);
                }


                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic SetQuyTrinhNhuCauTuyenDung(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_TuyenDung_NhuCau QuyTrinh = JsonConvert.DeserializeObject<HRM_TuyenDung_NhuCau>(data.ToString());
                    HRM_TuyenDung_NhuCau QuyTrinhdb = db.HRM_TuyenDung_NhuCau.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, QuyTrinh, err))
                        return err;

                    bool check = UpdateQuyTrinhNhuCauTuyenDung(QuyTrinh, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    if (!SetSauHamUpdate(db, QuyTrinhdb, QuyTrinh, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", QuyTrinh.IdGuid);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }

        public bool UpdateQuyTrinhNhuCauTuyenDung(HRM_TuyenDung_NhuCau QuyTrinh, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {


                if (QuyTrinh.NgayUnix > 0)
                    QuyTrinh.Ngay = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayUnix);
                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.IdDuAn = QuyTrinh.IdCongTyEOS;
                    QuyTrinh.eAction = DefineData.eAction.NHUCAUTUYENDUNG.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return false;

                    db.HRM_TuyenDung_NhuCau.Add(QuyTrinh);
                }

                else
                {
                    int Check = db.HRM_TuyenDung_NhuCau.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh && r.IdGuid != QuyTrinh.IdGuid).AsNoTracking().Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhNhuCauTuyenDung(db).SoPhieu.ToString();
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListUser = db.HRM_TuyenDung_NhuCau_ChiTiet.Where(r => r.IdTuyenDungNhuCau == QuyTrinh.IdGuid).ToList();
                db.HRM_TuyenDung_NhuCau_ChiTiet.RemoveRange(ListUser);

                if (QuyTrinh.listChiTiet != null && QuyTrinh.listChiTiet.Count > 0)
                {
                    foreach (var item in QuyTrinh.listChiTiet)
                    {
                        if (item.NgayDuKienDiLamUnix > 0)
                            item.NgayDuKienDiLam = dungchung.UnixTimeStampToDateTime(item.NgayDuKienDiLamUnix);
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdTuyenDungNhuCau = QuyTrinh.IdGuid;
                        item.IdCongTyEOS = QuyTrinh.IdCongTyEOS;

                    }
                    db.HRM_TuyenDung_NhuCau_ChiTiet.AddRange(QuyTrinh.listChiTiet);
                }
                if (!SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return false;
                db.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }

        }
        public dynamic DeleteQuyTrinhNhuCauTuyenDung(ApplicationDbContext db, string Id)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_NhuCau QuyTrinh = db.HRM_TuyenDung_NhuCau.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_TuyenDung_NhuCau_ChiTiet.Where(r => r.IdTuyenDungNhuCau == Id).ToList();
                    db.HRM_TuyenDung_NhuCau_ChiTiet.RemoveRange(ListUser);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công", QuyTrinh);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhNhuCauTuyenDung(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_TuyenDung_NhuCau QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_TuyenDung_NhuCau)) as HRM_TuyenDung_NhuCau;

                    HRM_TuyenDung_NhuCau pcuDatabase = db.HRM_TuyenDung_NhuCau.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    bool Error = UpdateQuyTrinhNhuCauTuyenDung(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return err;


                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhNhuCauTuyenDung(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_TuyenDung_NhuCau QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_TuyenDung_NhuCau)) as HRM_TuyenDung_NhuCau;

                    HRM_TuyenDung_NhuCau pcuDatabase = db.HRM_TuyenDung_NhuCau.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = UpdateQuyTrinhNhuCauTuyenDung(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }



        #endregion
        #region     Dề nghị tuyển dụng
        public dynamic GetDanhSachDeNghiTuyenDungByCongTy(ApplicationDbContext db, dynamic data)
        {
            TimKiem itemTiemKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                DateTime dtTuNgay = UnixTimeStampToDateTime(itemTiemKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(itemTiemKiem.DenNgay ?? 0);
                List<HRM_DeNghiTuyenDung> listResult = new List<HRM_DeNghiTuyenDung>();
                var list = from a in db.HRM_DeNghiTuyenDung
                           where (itemTiemKiem.IdCongTy == null || itemTiemKiem.IdCongTy == 0 || a.IdCongTy == itemTiemKiem.IdCongTy) && a.isKetThuc == true

                           select a;
                if (itemTiemKiem.IdCongTy == null || itemTiemKiem.IdCongTy == 0)
                    return list.OrderBy(a => a.NgayYeuCau).ToList();
                if (itemTiemKiem.TuNgay > 0)
                    list = list.Where(a => a.NgayYeuCau >= dtTuNgay);
                if (itemTiemKiem.DenNgay > 0)
                    list = list.Where(a => a.NgayYeuCau <= dtDenNgay);
                var listItem = list.ToList();
                if (listItem != null && listItem.Count > 0)
                {

                    if (listItem != null && listItem.Count > 0)
                    {
                        List<long> listIdBoPhan = listItem.Select(a => a.IdBoPhan).Distinct().ToList();
                        List<BoPhan> listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                        List<ChucVu> listChucVu = _CallWebService.GetDanhSachChucVu(db);
                        var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                        foreach (var item in listItem)
                        {
                            var BP = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhan);
                            if (BP != null)
                                item.TenBoPhan = BP.TenBoPhan;
                            var CT = listCongTy.FirstOrDefault(a => a.Id == item.IdCongTy);
                            if (CT != null)
                                item.TenCongTy = CT.TenBoPhan;
                            var CD = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                            if (CD != null)
                                item.TenChucVu = CD.TenChucVu;
                        }

                    }
                }
                return listResult;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetNextSoQuyTrinhDeNghiTuyenDung(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "DNTD",
                db.HRM_DeNghiTuyenDung.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault(),
                digits: 4));
        public dynamic GetListQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_DeNghiTuyenDung> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_DeNghiTuyenDung
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_DeNghiTuyenDung
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.NgayYeuCau >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayYeuCau < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.GhiChu) && a.GhiChu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                if (TimKiem.IdChucVu > 0)
                    result = result.Where(a => a.IdChucVu == TimKiem.IdChucVu);

                if (TimKiem.IdBoPhan > 0)
                    result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);

                var itemList = Paginate(result.AsNoTracking().Distinct(), nCurrentPage, PageSize, out PagingInfor paging);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                List<ChucVu> listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                var listId = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listTuyen = db.HRM_DeNghiTuyenDung_BoPhan.Where(a => listId.Contains(a.IdQuyTrinh)).Distinct().ToList();
                foreach (HRM_DeNghiTuyenDung item in itemList)
                {
                    if (listTuyen != null && listTuyen.Count > 0)
                    {
                        var tuyens = listTuyen.Where(a => a.IdQuyTrinh == item.IdGuid).ToList();
                        if (tuyens != null && tuyens.Count > 0)
                        {
                            item.TongSo = "Nam: " + tuyens.Sum(a => a.Nam) + " - Nữ: " + tuyens.Sum(a => a.Nu);
                        }
                    }
                    var BP = BoPhans.FirstOrDefault(a => a.Id == item.IdBoPhan);
                    if (BP != null)
                        item.TenBoPhan = BP.TenBoPhan;
                    var CT = listCongTy.FirstOrDefault(a => a.Id == item.IdCongTy);
                    if (CT != null)
                        item.TenCongTy = CT.TenBoPhan;
                    var CD = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                    if (CD != null)
                        item.TenChucVu = CD.TenChucVu;

                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_DeNghiTuyenDung QuyTrinh = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == Id);
                if (!GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))

                    return err;
                QuyTrinh.listFileDinhKem = FileUploadController.GetListFileDinhKem(db, QuyTrinh.IdGuid, DefineData.eFlagDinhKem.FileDinhKem.ToString());
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                if (QuyTrinh.NgayYeuCau != null)
                    QuyTrinh.NgayYeuCauUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.NgayYeuCau);
                if (QuyTrinh.NgayBatDauDiLam != null)
                    QuyTrinh.NgayBatDauDiLamUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.NgayBatDauDiLam);
                QuyTrinh.NhanVien = db.HRM_DeNghiTuyenDung_NhanVien.FirstOrDefault(a => a.IdQuyTrinh == Id);
                if (QuyTrinh.NhanVien != null && QuyTrinh.NhanVien.ThoiGianTuyen != null)
                    QuyTrinh.NhanVien.ThoiGianTuyenUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.NhanVien.ThoiGianTuyen);
                QuyTrinh.listCaLamViec = db.HRM_DeNghiTuyenDung_CaLamViec.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                QuyTrinh.listCongNhan = db.HRM_DeNghiTuyenDung_CongNhan.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                QuyTrinh.listBoPhan = db.HRM_DeNghiTuyenDung_BoPhan.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_DeNghiTuyenDung_CongNhan_PhuCap.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                if (QuyTrinh.listCongNhan.Count > 0)
                {
                    foreach (var item in QuyTrinh.listCongNhan)
                    {
                        item.listPhuCap = listPhuCap.Where(a => a.Loai == item.Loai).OrderBy(a => a.SapXep).ToList();

                    }
                }

                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic SetQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_DeNghiTuyenDung QuyTrinh = JsonConvert.DeserializeObject<HRM_DeNghiTuyenDung>(data.ToString());
                    HRM_DeNghiTuyenDung QuyTrinhdb = db.HRM_DeNghiTuyenDung.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, QuyTrinh, err))
                        return err;

                    bool check = UpdateQuyTrinhDeNghiTuyenDung(QuyTrinh, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    if (!SetSauHamUpdate(db, QuyTrinhdb, QuyTrinh, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", QuyTrinh.IdGuid);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public bool UpdateQuyTrinhDeNghiTuyenDung(HRM_DeNghiTuyenDung QuyTrinh, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {


                if (QuyTrinh.NgayBatDauDiLamUnix > 0)
                    QuyTrinh.NgayBatDauDiLam = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayBatDauDiLamUnix);
                if (QuyTrinh.NgayYeuCauUnix > 0)
                    QuyTrinh.NgayYeuCau = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayYeuCauUnix);

                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.IdDuAn = QuyTrinh.IdCongTy;
                    QuyTrinh.eAction = DefineData.eAction.DENGHITUYENDUNG.ToString();
                    if (string.IsNullOrEmpty(QuyTrinh.SoQuyTrinh))
                        QuyTrinh.SoQuyTrinh = GetNextSoQuyTrinhDeNghiTuyenDung(db);
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return false;

                    db.HRM_DeNghiTuyenDung.Add(QuyTrinh);
                }

                else
                {
                    int Check = db.HRM_DeNghiTuyenDung.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh && r.IdGuid != QuyTrinh.IdGuid).AsNoTracking().Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhDeNghiTuyenDung(db).SoPhieu.ToString();
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListNhanVien = db.HRM_DeNghiTuyenDung_NhanVien.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_DeNghiTuyenDung_NhanVien.RemoveRange(ListNhanVien);
                var listBoPhan = db.HRM_DeNghiTuyenDung_BoPhan.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_DeNghiTuyenDung_BoPhan.RemoveRange(listBoPhan);

                var ListCaLamViec = db.HRM_DeNghiTuyenDung_CaLamViec.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_DeNghiTuyenDung_CaLamViec.RemoveRange(ListCaLamViec);
                var ListCongNhan = db.HRM_DeNghiTuyenDung_CongNhan.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_DeNghiTuyenDung_CongNhan.RemoveRange(ListCongNhan);
                var ListPhuCap = db.HRM_DeNghiTuyenDung_CongNhan_PhuCap.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_DeNghiTuyenDung_CongNhan_PhuCap.RemoveRange(ListPhuCap);
                if (QuyTrinh.listFileDinhKem != null)
                    FileUploadController.SetFileDinhKem(db, QuyTrinh.listFileDinhKem, DefineData.eFlagDinhKem.FileDinhKem.ToString(), QuyTrinh.IdGuid);
                if (QuyTrinh.NhanVien != null)
                {
                    if (QuyTrinh.NhanVien.ThoiGianTuyenUnix > 0)
                        QuyTrinh.NhanVien.ThoiGianTuyen = dungchung.UnixTimeStampToDateTime(QuyTrinh.NhanVien.ThoiGianTuyenUnix);
                    QuyTrinh.NhanVien.IdGuid = Guid.NewGuid().ToString();
                    QuyTrinh.NhanVien.IdQuyTrinh = QuyTrinh.IdGuid;
                    QuyTrinh.NhanVien.IdCongTy = QuyTrinh.IdCongTy;
                    QuyTrinh.NhanVien.NgayYeuCau = QuyTrinh.NgayYeuCau;
                }
                db.HRM_DeNghiTuyenDung_NhanVien.Add(QuyTrinh.NhanVien);
                if (QuyTrinh.listCaLamViec != null && QuyTrinh.listCaLamViec.Count > 0)
                {
                    int STT = 1;
                    foreach (var item in QuyTrinh.listCaLamViec)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = QuyTrinh.IdGuid;
                        item.IdCongTy = QuyTrinh.IdCongTy;
                        item.NgayYeuCau = QuyTrinh.NgayYeuCau;
                        item.SapXep = STT;
                        STT++;

                    }
                    db.HRM_DeNghiTuyenDung_CaLamViec.AddRange(QuyTrinh.listCaLamViec);
                }
                if (QuyTrinh.listBoPhan != null && QuyTrinh.listBoPhan.Count > 0)
                {
                    int STT = 1;
                    foreach (var item in QuyTrinh.listBoPhan)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = QuyTrinh.IdGuid;
                        item.IdCongTy = QuyTrinh.IdCongTy;
                        item.IsKetThuc = QuyTrinh.isKetThuc;
                        item.SapXep = STT;
                        STT++;

                    }
                    db.HRM_DeNghiTuyenDung_BoPhan.AddRange(QuyTrinh.listBoPhan);
                }

                if (QuyTrinh.listCongNhan != null && QuyTrinh.listCongNhan.Count > 0)
                {
                    int STT = 1;
                    foreach (var item in QuyTrinh.listCongNhan)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = QuyTrinh.IdGuid;
                        item.IdCongTy = QuyTrinh.IdCongTy;
                        item.NgayYeuCau = QuyTrinh.NgayYeuCau;
                        item.SapXep = STT;
                        STT++;
                        foreach (var Pc in item.listPhuCap)
                        {
                            Pc.IdGuid = Guid.NewGuid().ToString();
                            Pc.IdQuyTrinh = QuyTrinh.IdGuid;
                            Pc.Loai = item.Loai;
                            item.SapXep = STT;
                            STT++;
                        }
                        db.HRM_DeNghiTuyenDung_CongNhan_PhuCap.AddRange(item.listPhuCap);
                    }
                    db.HRM_DeNghiTuyenDung_CongNhan.AddRange(QuyTrinh.listCongNhan);
                }

                if (!SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return false;
                db.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }

        }
        public dynamic DeleteQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, string Id)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_DeNghiTuyenDung QuyTrinh = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListNhanVien = db.HRM_DeNghiTuyenDung_NhanVien.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                    db.HRM_DeNghiTuyenDung_NhanVien.RemoveRange(ListNhanVien);
                    var ListCaLamViec = db.HRM_DeNghiTuyenDung_CaLamViec.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                    db.HRM_DeNghiTuyenDung_CaLamViec.RemoveRange(ListCaLamViec);
                    var ListCongNhan = db.HRM_DeNghiTuyenDung_CongNhan.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                    db.HRM_DeNghiTuyenDung_CongNhan.RemoveRange(ListCongNhan);
                    var ListPhuCap = db.HRM_DeNghiTuyenDung_CongNhan_PhuCap.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                    db.HRM_DeNghiTuyenDung_CongNhan_PhuCap.RemoveRange(ListPhuCap);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công", QuyTrinh);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }
        public dynamic ChuyenTiepListQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, List<string> listId)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var listData = db.HRM_DeNghiTuyenDung.Where(x => listId.Contains(x.IdGuid)).ToList();
                    foreach (var entity in listData)
                    {
                        TrangThaiQuyTrinh trangThai = _CallWebService.GetTrangThaiChuyenTiep(db, entity);

                        if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                        {
                            //_dbContext.Database.RollbackTransaction();
                            err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, trangThai.TenTrangThai);
                            return false;
                        }
                        entity.LoaiTrangThai = trangThai.LoaiTrangThai;
                        if (trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc || trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.XuLyDacBiet)
                        {
                            entity.isKetThuc = true;
                        }
                        entity.IdTrangThai = trangThai.IdTrangThai;
                        entity.TenTrangThai = trangThai.TenTrangThai;
                        entity.listKhoiTao = trangThai.listKhoiTao;
                        db.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    dbContextTransaction.Commit();
                    return err;


                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, HRM_DeNghiTuyenDung QuyTrinh)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //HRM_DeNghiTuyenDung QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_DeNghiTuyenDung)) as HRM_DeNghiTuyenDung;

                    HRM_DeNghiTuyenDung pcuDatabase = db.HRM_DeNghiTuyenDung.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    QuyTrinh.NoiDung = QuyTrinh.LyDo;
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    bool Error = UpdateQuyTrinhDeNghiTuyenDung(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return err;


                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhDeNghiTuyenDung(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_DeNghiTuyenDung QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_DeNghiTuyenDung)) as HRM_DeNghiTuyenDung;

                    HRM_DeNghiTuyenDung pcuDatabase = db.HRM_DeNghiTuyenDung.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = UpdateQuyTrinhDeNghiTuyenDung(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic ExportDeNghiTuyenDungNhanVien(HRM_DeNghiTuyenDung QuyTrinh, ApplicationDbContext db)
        {
            string sFile = "DeNghiTuyenDungNhanVien.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "DeNghiTuyenDungNhanVien.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);

            var thongTinDuAn = _CallWebService.GetQuyTrinhDuAn(QuyTrinh.IdCongTy, db);
            var sheet1 = package.Workbook.Worksheets.First();
            if (QuyTrinh.NgayBatDauDiLamUnix > 0)
                QuyTrinh.NgayBatDauDiLam = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayBatDauDiLamUnix);
            else
                QuyTrinh.NgayBatDauDiLam = null;
            if (QuyTrinh.NgayYeuCauUnix > 0)
                QuyTrinh.NgayYeuCau = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayYeuCauUnix);
            else QuyTrinh.NgayYeuCau = null;
            if (QuyTrinh.NhanVien.ThoiGianTuyenUnix > 0)
                QuyTrinh.NhanVien.ThoiGianTuyen = dungchung.UnixTimeStampToDateTime(QuyTrinh.NhanVien.ThoiGianTuyenUnix);



            if (QuyTrinh.NhanVien == null)
                return "";
            var NhanVienYeuCau = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == QuyTrinh.CreatedBy);
            if (NhanVienYeuCau == null)
                NhanVienYeuCau = new HRM_NhanVien();
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var chucVu = listChucVu.FirstOrDefault(a => a.Id == NhanVienYeuCau.IdChucVu);
            if (chucVu == null)
                chucVu = new ChucVu();
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            int nRow = 5;
            sheet1.Cells[nRow, 1].Value = "Người yêu cầu: " + QuyTrinh.CreatedByName;
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Chức vụ: " + chucVu.TenChucVu;
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Căn cứ vào tình hình hoạt động của Công ty hiện nay cũng như việc chuẩn bị nhân sự trong thời gian tới.";
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Công ty cần nhân sự cho vị trí :";
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            int TongSL = (QuyTrinh.SoLuongNam ?? 0) + (QuyTrinh.SoLuongNu ?? 0);
            sheet1.Cells[nRow, 1].Value = "Số lượng yêu cầu : " + TongSL;
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            sheet1.Cells[nRow, 3].Value = "Nam " + (QuyTrinh.SoLuongNam ?? 0) + " Nữ " + (QuyTrinh.SoLuongNu ?? 0);
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Độ tuổi: ";
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            sheet1.Cells[nRow, 3].Value = "Tuổi";
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Thời gian làm việc: ";
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            if (QuyTrinh.NgayYeuCau != null)
                sheet1.Cells[nRow, 3].Value = QuyTrinh.NgayYeuCau.Value.ToString("dd/MM/yyyy");
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Địa điểm làm việc: ";
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            if (QuyTrinh.NgayYeuCau != null)
                sheet1.Cells[nRow, 3].Value = thongTinDuAn.TenBoPhan;
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Yêu cầu công việc: ";
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            int Hight = 1;
            if (QuyTrinh.NhanVien.YeuCauCongViec != null)
            {
                dungchung.SetRichTextFromHtml(sheet1.Cells[nRow, 3], QuyTrinh.NhanVien.YeuCauCongViec, "Times New Roamn", 12);
                Hight = QuyTrinh.NhanVien.YeuCauCongViec.Length / 90 + 1;
            }
            sheet1.Row(nRow).Height = 17.75 * Hight;
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Yêu cầu trình độ: ";
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            Hight = 1;
            if (QuyTrinh.NhanVien.YeuCauTrinhDo != null)
            {
                dungchung.SetRichTextFromHtml(sheet1.Cells[nRow, 1], QuyTrinh.NhanVien.YeuCauTrinhDo, "Times New Roamn", 12);
                Hight = QuyTrinh.NhanVien.YeuCauTrinhDo.Length / 90 + 1;
            }
            sheet1.Row(nRow).Height = 17.75 * Hight;
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Yêu cầu kinh nghiệm: ";
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            Hight = 1;
            if (QuyTrinh.NhanVien.YeuCauKinhNghiem != null)
            {
                dungchung.SetRichTextFromHtml(sheet1.Cells[nRow, 1], QuyTrinh.NhanVien.YeuCauKinhNghiem, "Times New Roamn", 12);
                Hight = QuyTrinh.NhanVien.YeuCauKinhNghiem.Length / 90 + 1;
            }
            sheet1.Row(nRow).Height = 17.75 * Hight;
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Mức lương cho phép: ";
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = string.Format("{0:0,0 VNĐ}", QuyTrinh.NhanVien.MucLuong).Replace(".", ",");
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Quyền lợi được hưởng: ";
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Hưởng theo chế độ công ty";
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Yêu cầu kinh nghiệm: ";
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            Hight = 1;
            if (QuyTrinh.NhanVien.YeuCauKhac != null)
            {
                dungchung.SetRichTextFromHtml(sheet1.Cells[nRow, 1], QuyTrinh.NhanVien.YeuCauKhac, "Times New Roamn", 12);
                Hight = QuyTrinh.NhanVien.YeuCauKhac.Length / 90 + 1;
            }
            sheet1.Row(nRow).Height = 17.75 * Hight;
            sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "Thời gian tuyển";
            sheet1.Cells[nRow, 1, nRow, 2].Merge = true;
            nRow++;
            if (QuyTrinh.NhanVien.ThoiGianTuyen != null)
                sheet1.Cells[nRow, 3].Value = QuyTrinh.NhanVien.ThoiGianTuyen.ToString("dd/MM/yyyy");
            sheet1.Cells[nRow, 3, nRow, 10].Merge = true;
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExporDeNghiTuyenDungCongNhan(HRM_DeNghiTuyenDung QuyTrinh, ApplicationDbContext db)
        {
            if (QuyTrinh.NgayBatDauDiLamUnix > 0)
                QuyTrinh.NgayBatDauDiLam = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayBatDauDiLamUnix);
            else
                QuyTrinh.NgayBatDauDiLam = null;
            if (QuyTrinh.NgayYeuCauUnix > 0)
                QuyTrinh.NgayYeuCau = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayYeuCauUnix);
            else QuyTrinh.NgayYeuCau = null;
            if (QuyTrinh.listCongNhan == null)
                return "";
            string sFile = "DeNghiTuyenDungCongNhan.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "DeNghiTuyenDungNhanVien.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var NhanVienYeuCau = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == QuyTrinh.CreatedBy);
            if (NhanVienYeuCau == null)
                NhanVienYeuCau = new HRM_NhanVien();
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NhanVienYeuCau.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            var thongTinDuAn = _CallWebService.GetQuyTrinhDuAn(QuyTrinh.IdCongTy, db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var chucVu = listChucVu.FirstOrDefault(a => a.Id == NhanVienYeuCau.IdChucVu);
            if (chucVu == null)
                chucVu = new ChucVu();
            var chucVuTD = listChucVu.FirstOrDefault(a => a.Id == QuyTrinh.IdChucVu);
            if (chucVuTD == null)
                chucVuTD = new ChucVu();
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            sheet1.Cells[4, 1].Value = "Người yêu cầu: " + QuyTrinh.CreatedByName;
            sheet1.Cells[4, 2].Value = "Chức vụ: " + chucVu.TenChucVu;
            if (QuyTrinh.NgayYeuCau != null)
                sheet1.Cells[5, 1].Value = "Ngày yêu cầu: " + QuyTrinh.NgayYeuCau.Value.ToString("dd/MM/yyyy");
            if (SDT != null)
                sheet1.Cells[5, 2].Value = "Điện thoại liên hệ: " + SDT;
            sheet1.Cells[7, 1].Value = "Đơn vị: " + thongTinDuAn.TenBoPhan;
            sheet1.Cells[8, 1].Value = "Địa chỉ: " + thongTinDuAn.DiaChi;
            sheet1.Cells[9, 1].Value = "Chức vụ: " + chucVuTD.TenChucVu;
            int SL = (QuyTrinh.SoLuongNam ?? 0) + (QuyTrinh.SoLuongNu ?? 0);
            sheet1.Cells[9, 3].Value = "Số lượng: " + SL + " nhân viên";
            var cellSoLuong = sheet1.Cells[10, 3];
            var r1 = cellSoLuong.RichText.Add("Nam: " + (QuyTrinh.SoLuongNam ?? 0) + "nhân viên" + "\r\n" + "Nữ: " + (QuyTrinh.SoLuongNu ?? 0) + " nhân viên");
            r1.FontName = "Times New Roman";
            r1.Size = 12;
            sheet1.Cells[11, 1].Value = "Lý do: " + QuyTrinh.LyDo;
            sheet1.Cells[12, 1].Value = "Ngày dự kiến bắt đầu làm việc:  " + QuyTrinh.LyDo;
            sheet1.Cells[14, 1].Value = "1) Tổng lương/tháng:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].TongLuong).Replace(".", ",");
            sheet1.Cells[14, 2].Value = "1) Tổng lương/Ngày:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].TongLuong).Replace(".", ",");
            sheet1.Cells[15, 1].Value = "+ Mức lương cơ bản:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].LuongCoBan).Replace(".", ",");
            sheet1.Cells[15, 2].Value = "+ Mức lương cơ bản:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].LuongCoBan).Replace(".", ",");
            var listPhuCapBaoTri = QuyTrinh.listCongNhan[0].listPhuCap;
            var listPhuCapThoiVu = QuyTrinh.listCongNhan[1].listPhuCap;
            int nRow = 17;
            int STT = 0;
            foreach (var item in listPhuCapBaoTri)
            {
                var PhuCap = listPhuCap.FirstOrDefault(a => a.IdGuid == item.IddmPhuCap);
                if (PhuCap != null)
                {
                    sheet1.Cells[nRow, 1].Value = "   " + PhuCap.Ten + ": " + string.Format("{0:0,0 VNĐ}", (item.SoTien ?? 0)).Replace(".", ",");
                    sheet1.Cells[nRow, 2].Value = "   " + PhuCap.Ten + ": " + string.Format("{0:0,0 VNĐ}", (listPhuCapThoiVu[STT].SoTien ?? 0)).Replace(".", ",");
                }
                STT++;
                nRow++;
            }
            sheet1.Cells[nRow, 1].Value = "+ Phép:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].Phep).Replace(".", ",");
            sheet1.Cells[nRow, 2].Value = "+ Phép:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].Phep).Replace(".", ",");
            nRow++;
            sheet1.Cells[nRow, 1].Value = "+ Khác (nếu có):  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].Khac).Replace(".", ",");
            sheet1.Cells[nRow, 2].Value = "+ Khác (nếu có):  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].Khac).Replace(".", ",");
            nRow++;
            sheet1.Cells[nRow, 1].Value = "2) Mức lương làm thêm giờ:";
            sheet1.Cells[nRow, 2].Value = "2) Mức lương làm thêm giờ:";
            sheet1.Cells[nRow, 1, nRow, 3].Style.Font.Bold = true;
            nRow++;
            sheet1.Cells[nRow, 1].Value = "+ Ngày nghỉ:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].NgayNghi).Replace(".", ",");
            sheet1.Cells[nRow, 2].Value = "+ Ngày nghỉ:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].NgayNghi).Replace(".", ",");
            nRow++;
            sheet1.Cells[nRow, 1].Value = "+ Ngày thường:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].NgayNghi).Replace(".", ",");
            sheet1.Cells[nRow, 2].Value = "+ Ngày thường:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].NgayNghi).Replace(".", ",");
            nRow++;
            sheet1.Cells[nRow, 1].Value = "+ Ngày tết:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[0].NgayTet).Replace(".", ",");
            sheet1.Cells[nRow, 2].Value = "+ Ngày tết:  " + string.Format("{0:0,0 VNĐ}", QuyTrinh.listCongNhan[1].NgayTet).Replace(".", ",");

            sheet1.Cells[13, 1, nRow, 1].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            sheet1.Cells[13, 3, nRow, 3].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            sheet1.Cells[nRow, 1, nRow, 3].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportMauNhuCauCauTuyenDung(ApplicationDbContext db)

        {
            string sFile = "ImportNhuCauTuyenDung.xlsx";
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "ImportNhuCauTuyenDung.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            int col = 10;
            foreach (var item in listPhuCap)
            {
                sheet1.Cells[2, col].Value = item.IdGuid;
                sheet1.Cells[3, col, 4, col].Merge = true;
                sheet1.Cells[3, col].Value = item.Ten;
                col++;
            }
            dungchung.DrawTableExcel(sheet1, 3, 4, 1, col - 1);
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells[1, 1, 1, col - 1].Merge = true;
            sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportMauQuyTrinhNhuCauCauTuyenDung(ApplicationDbContext db)

        {
            string sFile = "ImportQuyTrinhNhuCauTuyenDung.xlsx";
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "ImportQuyTrinhNhuCauTuyenDung.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);

            var sheetDM = package.Workbook.Worksheets["Danh mục"];
            var listCty = _CallWebService.GetDanhSachDuAn(db);
            int nRow = 3;
            foreach (var item in listCty)
            {
                sheetDM.Cells[nRow, 1].Value = item.MaBoPhan + "_" + item.TenBoPhan;
                sheetDM.Cells[nRow, 2].Value = item.MaBoPhan;
                nRow++;
            }
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            nRow = 3;
            foreach (var item in listChucVu)
            {
                sheetDM.Cells[nRow, 5].Value = item.MaChucVu + "_" + item.TenChucVu;
                sheetDM.Cells[nRow, 6].Value = item.TenChucVu;
                nRow++;
            }
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            nRow = 3;
            foreach (var item in listBoPhan)
            {
                sheetDM.Cells[nRow, 3].Value = item.MaBoPhan + "_" + item.TenBoPhan;
                sheetDM.Cells[nRow, 4].Value = item.MaBoPhan;
                nRow++;
            }
            var listNhanVien = db.HRM_NhanVien.AsNoTracking().Select(x => new { x.MaNhanVien, x.TenDayDu }).ToList();
            nRow = 3;
            foreach (var item in listNhanVien)
            {
                sheetDM.Cells[nRow, 7].Value = item.MaNhanVien + "_" + item.TenDayDu;
                sheetDM.Cells[nRow, 8].Value = item.MaNhanVien;
                nRow++;
            }

            var sheet1 = package.Workbook.Worksheets["Công nhân"];
            var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            int col = 14;
            foreach (var item in listPhuCap)
            {
                sheet1.Column(col).Width = 15;
                sheet1.Cells[2, col].Value = item.IdGuid;
                sheet1.Cells[3, col, 4, col].Merge = true;
                sheet1.Cells[3, col].Value = item.Ten;
                col++;
            }
            dungchung.DrawTableExcel(sheet1, 3, 4, 1, col - 1);
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells[1, 1, 1, col - 1].Merge = true;
            sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic TongHopDeNghiTuyenDung(ApplicationDbContext db, dynamic dyn)

        {
            string sFile = "TongHopDeNghiTuyenDung.xlsx";
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "TongHopDeNghiTuyenDung.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            IQueryable<HRM_DeNghiTuyenDung> result;
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;

            result = from a in db.HRM_DeNghiTuyenDung
                     where a.isKetThuc == true
                     orderby a.Created descending
                     select a;


            DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
            DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
            if (TimKiem.TuNgay > 0)
                result = result.Where(a => a.NgayYeuCau >= dtTuNgay);
            if (TimKiem.DenNgay > 0)
                result = result.Where(a => a.NgayYeuCau < dtDenNgay.AddDays(1));
            //if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
            //    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.GhiChu) && a.GhiChu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
            if (TimKiem.IdCongTy > 0)
                result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
            //if (TimKiem.IdChucVu > 0)
            //    result = result.Where(a => a.IdChucVu == TimKiem.IdChucVu);

            //if (TimKiem.IdBoPhan > 0)
            //    result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);

            var itemList = result.Distinct().ToList();
            if (itemList.Count > 0)
            {
                var listIdCongTy = itemList.Select(a => a.IdCongTy).Distinct().ToList();
                var listId = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listData = db.HRM_DeNghiTuyenDung_BoPhan.Where(a => listId.Contains(a.IdQuyTrinh)).ToList();
                var listIdBoPhan = listData.Select(a => a.IdBoPhan).Distinct().ToList();
                var listIdChucVu = listData.Select(a => a.IdChucVu).Distinct().ToList();
                var listDuAn = _CallWebService.GetDanhSachDuAnDaDuyet(db).Where(a => listIdCongTy.Contains(a.Id)).ToList();
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Where(a => listIdBoPhan.Contains(a.Id)).ToList(); ;
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);

                var sheet1 = package.Workbook.Worksheets.First();
                int nCol = 3;
                int nRow = 3;

                nCol = 1;
                nRow = 5;
                int STTBP = 1;
                foreach (var BoPhan in listBoPhan)
                {
                    sheet1.Cells[nRow, 1].Value = dungchung.QuyDoiLaMa(STTBP);
                    sheet1.Cells[nRow, 2].Value = BoPhan.TenBoPhan;
                    int STT = 1;
                    var data = listData.Where(a => a.IdBoPhan == BoPhan.Id).ToList();
                    listIdChucVu = data.Select(a => a.IdChucVu).Distinct().ToList();
                    var ChucVus = listChucVu.Where(a => listIdChucVu.Contains(a.Id)).ToList();
                    #region Tổng bộ phận
                    nCol = 3;
                    int RowT = nRow + ChucVus.Count;
                    foreach (var DA in listDuAn)
                    {
                        sheet1.Cells[nRow, nCol].Formula = "=SUM(" + sheet1.Cells[nRow + 1, nCol].Address + ":" + sheet1.Cells[RowT, nCol].Address + ")";
                        DA.ListCT.Add(sheet1.Cells[nRow, nCol].Address);
                        nCol++;
                    }
                    sheet1.Cells[nRow, nCol].Formula = "=SUM(" + sheet1.Cells[nRow, 3].Address + ":" + sheet1.Cells[nRow, nCol - 1].Address + ")";
                    #endregion
                    nRow++;
                    foreach (var item in ChucVus)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenChucVu;
                        nCol++;
                        foreach (var DA in listDuAn)
                        {
                            sheet1.Cells[nRow, nCol].Value = data.Where(a => a.IdChucVu == item.Id && a.IdCongTy == DA.Id).Count();
                            nCol++;
                        }
                        sheet1.Cells[nRow, nCol].Formula = "=SUM(" + sheet1.Cells[nRow, 3].Address + ":" + sheet1.Cells[nRow, nCol - 1].Address + ")";
                    }
                    STTBP++;
                    nRow++;
                }
                nCol = 3;
                foreach (var item in listDuAn)
                {
                    sheet1.Cells[3, nCol].Value = item.MaBoPhan;
                    sheet1.Column(nCol).Width = 12;
                    sheet1.Cells[4, nCol].Formula = "=" + string.Join("+", item.ListCT);
                    nCol++;
                }
                nRow--;
                sheet1.Cells[3, nCol].Value = "TỔNG";
                //sheet1.Cells[3, nCol].Formula = "=SUM(" + sheet1.Cells[3, 3].Address + ":" + sheet1.Cells[3, nCol - 1].Address + ")";
                sheet1.Cells[4, nCol].Formula = "=SUM(" + sheet1.Cells[4, 3].Address + ":" + sheet1.Cells[4, nCol - 1].Address + ")";
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells[1, 1].Value = "ORDER TUYỂN DỤNG TỪ NGÀY " + dtTuNgay.ToString("dd/MM/yyyy") + " ĐẾN NGÀY " + dtDenNgay.ToString("dd/MM/yyyy");
                sheet1.Cells[1, 1].Style.Font.Bold = true;
                sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[4, 2, nRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 3, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        public dynamic ImportQuyTrinhNhuCauCauTuyenDung(string sFile, ApplicationDbContext db)

        {
            try
            {


                string sFileName =PathUploads+ sFile;
                DateTime dt = DateTime.Now;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                var sheetQT = package.Workbook.Worksheets["Quy trình"];
                var sheet1 = package.Workbook.Worksheets["Công nhân"];
                var sheet2 = package.Workbook.Worksheets["Nhân viên VP"];
                var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listCongNhan = new List<HRM_DeNghiTuyenDung_CongNhan>();
                List<HRM_DeNghiTuyenDung> entitys = new List<HRM_DeNghiTuyenDung>();

                int maxrow = sheetQT.Dimension.Rows;
                DateTime time = DateTime.Now;
                for (int i = 4; i <= maxrow; i++)
                {
                    int nCol = 2;
                    HRM_DeNghiTuyenDung item = new HRM_DeNghiTuyenDung();
                    item.MaCongTy = dungchung.GetCell(sheetQT, i, nCol);
                    if (string.IsNullOrEmpty(item.MaCongTy))
                        break;
                    nCol++;
                    nCol++;
                    item.MaBoPhan = dungchung.GetCell(sheetQT, i, nCol);
                    nCol++;
                    nCol++;
                    item.MaNguoiYeuCau = dungchung.GetCell(sheetQT, i, nCol);
                    nCol++;
                    nCol++;
                    item.NgayYeuCau = dungchung.GetCellToDateTime(sheetQT, i, nCol);
                    if (item.NgayYeuCau != null)
                        item.NgayYeuCauUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayYeuCau);
                    nCol++;
                    item.MaChucVu = dungchung.GetCell(sheetQT, i, nCol);
                    nCol++;
                    nCol++;
                    item.SoLuongNam = dungchung.GetCellToInt(sheetQT, i, nCol);
                    nCol++;
                    item.SoLuongNu = dungchung.GetCellToInt(sheetQT, i, nCol);
                    nCol++;
                    item.SoLuongBaoVu = dungchung.GetCellToInt(sheetQT, i, nCol);
                    nCol++;
                    item.SoLuongThoiVu = dungchung.GetCellToInt(sheetQT, i, nCol);
                    nCol++;
                    item.SoLuongThoiVuNgay = dungchung.GetCellToInt(sheetQT, i, nCol);
                    nCol++;
                    item.SoLuongThoiVuNgay = dungchung.GetCellToInt(sheetQT, i, nCol);
                    nCol++;
                    item.NgayBatDauDiLam = dungchung.GetCellToDateTime(sheetQT, i, nCol);
                    if (item.NgayBatDauDiLam != null)
                        item.NgayBatDauDiLamUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDauDiLam);
                    nCol++;
                    item.LyDo = dungchung.GetCell(sheetQT, i, nCol);
                    nCol++;
                    item.GhiChu = dungchung.GetCell(sheetQT, i, nCol);
                    entitys.Add(item);
                }
                maxrow = sheet1.Dimension.Rows;
                for (int i = 5; i <= maxrow; i++)
                {
                    int nCol = 2;
                    HRM_DeNghiTuyenDung_CongNhan item = new HRM_DeNghiTuyenDung_CongNhan();
                    item.MaCongTy = dungchung.GetCell(sheet1, i, nCol);
                    if (string.IsNullOrEmpty(item.MaCongTy))
                        break;
                    nCol++;
                    nCol++;
                    item.MaBoPhan = dungchung.GetCell(sheet1, i, nCol);
                    nCol++;
                    nCol++;
                    item.Loai = dungchung.GetCell(sheet1, i, nCol);
                    if (string.IsNullOrEmpty(item.Loai))
                        break;
                    nCol++;
                    item.TongLuong = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.LuongCoBan = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.Phep = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.Khac = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.NgayThuong = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.NgayNghi = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.NgayTet = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.listPhuCap = new List<HRM_DeNghiTuyenDung_CongNhan_PhuCap>();
                    for (int PC = 0; PC < listPhuCap.Count; PC++)
                    {
                        string IdPhuCap = dungchung.GetCell(sheet1, 2, nCol);
                        if (string.IsNullOrEmpty(IdPhuCap))
                            break;
                        item.listPhuCap.Add(new HRM_DeNghiTuyenDung_CongNhan_PhuCap
                        {
                            IddmPhuCap = IdPhuCap,
                            SoTien = dungchung.GetCellToDouble(sheet1, i, nCol)
                        });
                    }
                    listCongNhan.Add(item);
                }

                var listNhanVien = new List<HRM_DeNghiTuyenDung_NhanVien>();


                maxrow = sheet2.Dimension.Rows;
                for (int i = 4; i <= maxrow; i++)
                {
                    int nCol = 2;
                    var NhanVien = new HRM_DeNghiTuyenDung_NhanVien();
                    NhanVien.MaCongTy = dungchung.GetCell(sheet2, i, nCol);
                    if (string.IsNullOrEmpty(NhanVien.MaCongTy))
                        break;
                    nCol++;
                    nCol++;
                    NhanVien.MaBoPhan = dungchung.GetCell(sheet2, i, nCol);
                    nCol++;
                    nCol++;
                    NhanVien.YeuCauCongViec = dungchung.GetCell(sheet2, i, nCol);
                    nCol++;
                    NhanVien.YeuCauTrinhDo = dungchung.GetCell(sheet2, i, nCol);
                    nCol++;
                    NhanVien.YeuCauKinhNghiem = dungchung.GetCell(sheet2, i, nCol);
                    nCol++;
                    NhanVien.QuyenLoiDuocHuong = dungchung.GetCell(sheet2, i, nCol);
                    nCol++;
                    NhanVien.YeuCauKhac = dungchung.GetCell(sheet2, i, nCol);
                    nCol++;
                    NhanVien.MucLuong = dungchung.GetCellToDouble(sheet2, i, nCol);
                    nCol++;
                    NhanVien.NgayYeuCau = dungchung.GetCellToDateTime(sheet2, i, nCol);
                    listNhanVien.Add(NhanVien);
                };
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                if (entitys.Count > 0)
                {
                    var listCty = _CallWebService.GetDanhSachDuAn(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    List<string> listma = entitys.Where(a => !string.IsNullOrEmpty(a.MaNguoiYeuCau)).Select(a => a.MaNguoiYeuCau).Distinct().ToList();
                    var listNhanViens = db.HRM_NhanVien.Where(a => listma.Contains(a.MaNhanVien)).ToList();
                    foreach (var item in entitys)
                    {
                        item.IdCongTy = listCty.Where(a => a.MaBoPhan == item.MaCongTy).Select(a => a.Id).FirstOrDefault();
                        item.IdBoPhan = listBoPhan.Where(a => a.MaBoPhan == (item.MaBoPhan ?? "")).Select(a => a.Id).FirstOrDefault();
                        item.IdChucVu = listChucVu.Where(a => a.MaChucVu == (item.MaChucVu ?? "")).Select(a => a.Id).FirstOrDefault();
                        item.IdNguoiTaoPhieu = listNhanViens.Where(a => a.MaNhanVien == (item.MaNguoiYeuCau ?? "")).Select(a => a.IdGuid).FirstOrDefault();
                        if (listNhanVien != null)
                            item.NhanVien = listNhanVien.Where(a => a.MaCongTy == item.MaCongTy && (string.IsNullOrEmpty(item.MaBoPhan) || (!string.IsNullOrEmpty(a.MaBoPhan) && a.MaBoPhan == item.MaBoPhan))).FirstOrDefault();
                        if (listCongNhan != null)
                            item.listCongNhan = listCongNhan.Where(a => a.MaCongTy == item.MaCongTy && (string.IsNullOrEmpty(item.MaBoPhan) || (!string.IsNullOrEmpty(a.MaBoPhan) && a.MaBoPhan == item.MaBoPhan))).ToList();
                        bool check = UpdateQuyTrinhDeNghiTuyenDung(item, db, err, eQuyTrinh_LoaiHam.Set);
                        if (check == false)
                        {

                            return err;
                        }
                    }
                }
                s.Close();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thành công");
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra " + ex.ToString());
            }
        }
        public dynamic ImportMauNhuCauCauTuyenDung(string sFile, ApplicationDbContext db)

        {
            try
            {
                HRM_DeNghiTuyenDung entity = new HRM_DeNghiTuyenDung();

                string sFileName =PathUploads+ sFile;
                DateTime dt = DateTime.Now;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                var sheet1 = package.Workbook.Worksheets["Công nhân"];
                var sheet2 = package.Workbook.Worksheets["Nhân viên VP"];
                var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                entity.listCongNhan = new List<HRM_DeNghiTuyenDung_CongNhan>();


                int maxrow = sheet1.Dimension.Rows;
                DateTime time = DateTime.Now;
                for (int i = 5; i <= maxrow; i++)
                {
                    int nCol = 2;
                    HRM_DeNghiTuyenDung_CongNhan item = new HRM_DeNghiTuyenDung_CongNhan();
                    item.Loai = dungchung.GetCell(sheet1, i, nCol);
                    if (string.IsNullOrEmpty(item.Loai))
                        break;
                    nCol++;
                    item.TongLuong = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.LuongCoBan = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.Phep = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.Khac = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.NgayThuong = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.NgayNghi = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.NgayTet = dungchung.GetCellToDouble(sheet1, i, nCol);
                    nCol++;
                    item.listPhuCap = new List<HRM_DeNghiTuyenDung_CongNhan_PhuCap>();
                    for (int PC = 0; PC < listPhuCap.Count; PC++)
                    {
                        string IdPhuCap = dungchung.GetCell(sheet1, 2, nCol);
                        if (string.IsNullOrEmpty(IdPhuCap))
                            break;
                        item.listPhuCap.Add(new HRM_DeNghiTuyenDung_CongNhan_PhuCap
                        {
                            IddmPhuCap = IdPhuCap,
                            SoTien = dungchung.GetCellToDouble(sheet1, i, nCol)
                        });
                    }
                    entity.listCongNhan.Add(item);
                }
                entity.NhanVien = new HRM_DeNghiTuyenDung_NhanVien()
                {
                    YeuCauCongViec = dungchung.GetCell(sheet2, 3, 3),
                    YeuCauTrinhDo = dungchung.GetCell(sheet2, 4, 3),
                    YeuCauKinhNghiem = dungchung.GetCell(sheet2, 5, 3),
                    QuyenLoiDuocHuong = dungchung.GetCell(sheet2, 6, 3),
                    YeuCauKhac = dungchung.GetCell(sheet2, 7, 3),
                    MucLuong = dungchung.GetCellToDouble(sheet2, 8, 3),
                    NgayYeuCau = dungchung.GetCellToDateTime(sheet2, 9, 3),
                };

                s.Close();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thanh công", entity);
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra " + ex.ToString());
            }
        }

        #endregion
        #region Định biên nhân viên cơ sở
        public dynamic GetNextSoQuyTrinhDinhBienCoSo(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "DBCS",
                db.HRM_TuyenDung_DinhBienCoSo.OrderByDescending(a => a.SoQuyTrinh).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault(),
                digits: 4));

        public dynamic GetListQuyTrinhDinhBienCoSo(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_TuyenDung_DinhBienCoSo> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_TuyenDung_DinhBienCoSo
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_TuyenDung_DinhBienCoSo
                             orderby a.Created descending
                             select a;
                //DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                //DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                //if (TimKiem.TuNgay > 0)
                //    result = result.Where(a => a.Ngay >= dtTuNgay);
                //if (TimKiem.DenNgay > 0)
                //    result = result.Where(a => a.Ngay < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                if (listCongTy != null)
                {
                    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                    var listIdQT = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(a => listId.Contains(a.IdCongTyEOS)).Select(a => a.IdDinhBienCoSo).Distinct().ToList();
                    result = result.Where(a => listIdQT.Contains(a.IdGuid));
                }

                if (TimKiem.Nam > 0)
                {
                    result = result.Where(a => a.Nam == TimKiem.Nam);
                }
                var itemList = Paginate(result.AsNoTracking().Distinct(), nCurrentPage, PageSize, out PagingInfor paging);

                foreach (HRM_TuyenDung_DinhBienCoSo item in itemList)
                {
                    item.NgayUnix = dungchung.DateTimeToUnixTimeStamp(item.Ngay);

                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetListQuyTrinhDinhBienCoSoDaKetThuc(ApplicationDbContext db)
        {

            try
            {
                IQueryable<HRM_TuyenDung_DinhBienCoSo>
                    result = from a in db.HRM_TuyenDung_DinhBienCoSo
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where a.isKetThuc == true
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                if (listCongTy != null)
                {
                    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                    List<string> listIdQT = result.Select(a => a.IdGuid).Distinct().ToList();
                    listIdQT = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(a => listIdQT.Contains(a.IdDinhBienCoSo) && listId.Contains(a.IdCongTyEOS)).Select(a => a.IdDinhBienCoSo).Distinct().ToList();
                    result = result.Where(a => listIdQT.Contains(a.IdGuid));
                }

                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                foreach (HRM_TuyenDung_DinhBienCoSo item in itemList)
                {
                    item.NgayUnix = dungchung.DateTimeToUnixTimeStamp(item.Ngay);

                }
                return itemList;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }


        public dynamic GetQuyTrinhDinhBienCoSo(ApplicationDbContext db, string Id)
        {
            try
            {
                HRM_TuyenDung_DinhBienCoSo QuyTrinh = db.HRM_TuyenDung_DinhBienCoSo.FirstOrDefault(a => a.IdGuid == Id);
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                QuyTrinh.NgayUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.Ngay);
                QuyTrinh.listChucDanh = new List<HRM_TuyenDung_dinhBienCoSo_ChucDanh>();
                var listData = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(a => a.IdDinhBienCoSo == Id).OrderBy(a => a.SapXep).ToList();

                if (listData != null)
                {
                    var listIdChucDanh = listData.Select(a => a.IdChucDanh).Distinct().ToList();
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    foreach (var item in listIdChucDanh)
                    {


                        var lstCongTy = listData.Where(a => a.IdChucDanh == item).OrderBy(a => a.SapXep).ToList();
                        if (lstCongTy != null && lstCongTy.Count > 0)
                        {
                            List<long> listIdBoPhan = lstCongTy.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                            foreach (var IdBoPhan in listIdBoPhan)
                            {
                                HRM_TuyenDung_dinhBienCoSo_ChucDanh ChucDanhAdd = new HRM_TuyenDung_dinhBienCoSo_ChucDanh();
                                var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item);
                                ChucDanhAdd.IdChucDanh = item;
                                if (ChucDanh != null)
                                    ChucDanhAdd.TenChucDanh = ChucDanh.Ten;
                                ChucDanhAdd.listCongTy = new List<HRM_TuyenDung_DinhBienCoSo_CongTy>();
                                ChucDanhAdd.listCongTy = lstCongTy.Where(a => a.IdBoPhanEOS == IdBoPhan && a.IdChucDanh == item).OrderBy(a => a.SapXep).ToList();
                                ChucDanhAdd.IdBoPhanEOS = IdBoPhan;

                                QuyTrinh.listChucDanh.Add(ChucDanhAdd);
                            }

                        }
                    }

                }

                QuyTrinh.listCongTy = _CallWebService.GetDanhSachCongTy(db, QuyTrinh.CreatedBy);

                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic SetQuyTrinhDinhBienCoSo(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DinhBienCoSo QuyTrinh = JsonConvert.DeserializeObject<HRM_TuyenDung_DinhBienCoSo>(data.ToString());
                    var QuyTrinhdb = db.HRM_TuyenDung_DinhBienCoSo.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (QuyTrinhdb != null && QuyTrinhdb.IdTrangThai != QuyTrinh.IdTrangThai)
                    {
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = QuyTrinh.IdGuid,
                            eAction = DefineData.eAction.DINHBIENCOSO.ToString(),
                            IdTrangThai = QuyTrinh.IdTrangThai,
                        };
                        var trangThai = _CallWebService.DoiTrangThaiKhoiTao(audit, db);
                    }
                    MessageReturn msg = UpdateQuyTrinhDinhBienCoSo(QuyTrinh, db);
                    if (msg.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return msg;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", QuyTrinh.IdGuid);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }

        public dynamic UpdateQuyTrinhDinhBienCoSo(HRM_TuyenDung_DinhBienCoSo QuyTrinh, ApplicationDbContext db = null)
        {
            try
            {


                if (QuyTrinh.NgayUnix > 0)
                    QuyTrinh.Ngay = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayUnix);
                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.IdGuid = Guid.NewGuid().ToString();
                    if (string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        TrangThaiQuyTrinh QuyTrinhTrangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.DINHBIENCOSO.ToString(), QuyTrinh.IdGuid);
                        if (QuyTrinhTrangThai.IdTrangThai == null || QuyTrinhTrangThai.IdTrangThai == "")
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, QuyTrinhTrangThai.TenTrangThai + " do quy trình số " + QuyTrinh.SoQuyTrinh);
                        }
                        QuyTrinh.IdTrangThai = QuyTrinhTrangThai.IdTrangThai;
                        QuyTrinh.TenTrangThai = QuyTrinhTrangThai.TenTrangThai;

                    }
                    QuyTrinh.Modified = DateTime.Now;

                    db.HRM_TuyenDung_DinhBienCoSo.Add(QuyTrinh);
                }

                else
                {
                    int Check = db.HRM_TuyenDung_DinhBienCoSo.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh && r.IdGuid != QuyTrinh.IdGuid).AsNoTracking().Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhDinhBienCoSo(db).SoPhieu.ToString();
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListUser = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(r => r.IdDinhBienCoSo == QuyTrinh.IdGuid).ToList();
                db.HRM_TuyenDung_DinhBienCoSo_CongTy.RemoveRange(ListUser);

                if (QuyTrinh.listChucDanh != null && QuyTrinh.listChucDanh.Count > 0)
                {
                    int SapXep = 0;
                    foreach (var ChucDanh in QuyTrinh.listChucDanh)
                    {
                        foreach (var item in ChucDanh.listCongTy)
                        {
                            item.IdGuid = Guid.NewGuid().ToString();
                            item.IdDinhBienCoSo = QuyTrinh.IdGuid;
                            item.IdChucDanh = ChucDanh.IdChucDanh;
                            item.IdBoPhanEOS = ChucDanh.IdBoPhanEOS;
                            item.isKetThuc = QuyTrinh.isKetThuc;
                            item.Nam = QuyTrinh.Nam;
                            item.SapXep = SapXep;
                            SapXep++;
                        }
                        db.HRM_TuyenDung_DinhBienCoSo_CongTy.AddRange(ChucDanh.listCongTy);
                    }

                }

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công", QuyTrinh);

            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }

        }
        public dynamic DeleteQuyTrinhDinhBienCoSo(ApplicationDbContext db, string Id)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DinhBienCoSo QuyTrinh = db.HRM_TuyenDung_DinhBienCoSo.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(r => r.IdDinhBienCoSo == QuyTrinh.IdGuid).ToList();
                    db.HRM_TuyenDung_DinhBienCoSo_CongTy.RemoveRange(ListUser);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công", QuyTrinh);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhDinhBienCoSo(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DinhBienCoSo QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_TuyenDung_DinhBienCoSo)) as HRM_TuyenDung_DinhBienCoSo;

                    HRM_TuyenDung_DinhBienCoSo pcuDatabase = db.HRM_TuyenDung_DinhBienCoSo.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }
                    CallWebService callWeb = _CallWebService;
                    ChuyenTrangThai audit = new ChuyenTrangThai()
                    {
                        IdGuid = QuyTrinh.IdGuid,
                        eAction = DefineData.eAction.DINHBIENCOSO.ToString(),
                        IdTrangThai = QuyTrinh.IdTrangThai,

                    };
                    TrangThaiQuyTrinh TrangThaidnm = callWeb.GetTrangThaiChuyenTiep(db, audit);
                    string IdTrangThai = TrangThaidnm.IdTrangThai;
                    string TenTrangThai = TrangThaidnm.TenTrangThai;
                    if (IdTrangThai == "" || IdTrangThai == null)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra _" + TenTrangThai);
                    }
                    else
                    {
                        QuyTrinh.IdTrangThai = TrangThaidnm.IdTrangThai;
                        QuyTrinh.TenTrangThai = TrangThaidnm.TenTrangThai;
                        if (TrangThaidnm.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                        {
                            QuyTrinh.isKetThuc = true;
                        }

                    }
                    MessageReturn msg = UpdateQuyTrinhDinhBienCoSo(QuyTrinh, db);
                    if (msg.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                        dbContextTransaction.Rollback();
                    dbContextTransaction.Commit();
                    return msg;


                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhDinhBienCoSo(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DinhBienCoSo QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_TuyenDung_DinhBienCoSo)) as HRM_TuyenDung_DinhBienCoSo;

                    HRM_TuyenDung_DinhBienCoSo pcuDatabase = db.HRM_TuyenDung_DinhBienCoSo.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }

                    }
                    MessageReturn msg = UpdateQuyTrinhDinhBienCoSo(QuyTrinh, db);
                    if (msg.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return msg;
                    }
                    CallWebService callWeb = _CallWebService;
                    dynamic TrangThaidnm = callWeb.GetTrangThaiKhongDuyet(db, DefineData.eAction.DINHBIENCOSO.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                    if (TrangThaidnm.IdTrangThai == null || TrangThaidnm.IdTrangThai == "")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra _" + TrangThaidnm.TenTrangThai);
                    }
                    else
                    {
                        QuyTrinh.IdTrangThai = TrangThaidnm.IdTrangThai;
                        QuyTrinh.TenTrangThai = TrangThaidnm.TenTrangThai;
                        db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic GetDinhBienCoSo(ApplicationDbContext db, int Nam, long IdCongTy, long IdChucDanh)
        {

            var results = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(a => a.isKetThuc == true && a.Nam == Nam && a.IdCongTyEOS == IdCongTy && a.IdChucDanh == IdChucDanh).ToList();
            if (results != null)
                return results.Sum(a => a.DinhBien ?? 0);
            else
                return 0;

        }
        public dynamic GetTongHopDinhBienCoSo(ApplicationDbContext db, int Nam, List<long> listIdCongTy)
        {
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            List<HRM_TuyenDung_dinhBienCoSo_ChucDanh> listResult = new List<HRM_TuyenDung_dinhBienCoSo_ChucDanh>();
            var listData = db.HRM_TuyenDung_DinhBienCoSo_CongTy.Where(a => a.isKetThuc == true && a.Nam == Nam && (listIdCongTy.Count == 0 || listIdCongTy.Contains(a.IdCongTyEOS))).OrderBy(a => a.SapXep).ToList();
            if (listData != null)
            {

                var listIdChucDanh = listData.Select(a => a.IdChucDanh).Distinct().ToList();
                var listCapBacLuong = db.HRM_dmCapBacLuong.ToList();
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                foreach (var item in listIdChucDanh)
                {
                    HRM_TuyenDung_dinhBienCoSo_ChucDanh ChucDanhAdd = new HRM_TuyenDung_dinhBienCoSo_ChucDanh();
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item);
                    ChucDanhAdd.IdChucDanh = item;

                    if (ChucDanh != null)
                        ChucDanhAdd.TenChucDanh = ChucDanh.Ten;
                    ChucDanhAdd.listCongTy = new List<HRM_TuyenDung_DinhBienCoSo_CongTy>();
                    foreach (var Cty in listCongTy)
                    {
                        HRM_TuyenDung_DinhBienCoSo_CongTy CongTy = listData.Where(a => a.IdCongTyEOS == Cty.Id && a.IdChucDanh == item).OrderBy(a => a.Modified).FirstOrDefault();
                        if (CongTy == null)
                            CongTy = new HRM_TuyenDung_DinhBienCoSo_CongTy();
                        ChucDanhAdd.listCongTy.Add(CongTy);
                    }
                    //ChucDanhAdd.listCongTy = listData.Where(a => a.IdChucDanh == item).OrderBy(a => a.SapXep).ToList();
                    foreach (var cty in ChucDanhAdd.listCongTy)
                    {
                        var capbac = listCapBacLuong.FirstOrDefault(a => a.IdGuid == cty.IdCapBacLuongDeXuat);
                        if (capbac != null)
                            cty.TenCapBacLuong = capbac.Ten;
                    }
                    ChucDanhAdd.IdBoPhanEOS = ChucDanhAdd.listCongTy.FirstOrDefault().IdBoPhanEOS;
                    var boPhan = listBoPhan.FirstOrDefault(a => a.Id == ChucDanhAdd.IdBoPhanEOS);
                    if (boPhan != null)
                        ChucDanhAdd.TenBoPhan = boPhan.TenBoPhan;
                    listResult.Add(ChucDanhAdd);
                }

            }
            return listResult;

        }

        public dynamic ExportFileMauDinhBienCoSo(ApplicationDbContext db)
        {
            string sFile = "DinhBienCoSo.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "DinhBienCoSo.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var listCongTy = _CallWebService.GetDanhSachCongTy(db).OrderBy(a => a.MaBoPhan).ToList();

            int col = 6;
            int countPC = listCongTy.Count;
            sheet1.Cells[1, 1, 1, col + countPC * 5 - 1].Merge = true;
            // header


            foreach (var item in listCongTy)
            {
                sheet1.Cells[2, col].Value = item.MaBoPhan;
                sheet1.Cells[3, col, 3, col + 4].Merge = true;
                sheet1.Cells[3, col].Value = item.TenBoPhan;
                sheet1.Cells[4, col].Value = "Mã cấp bậc lương";
                col++;
                sheet1.Cells[4, col].Value = "Cấp bậc lương";
                col++;
                sheet1.Cells[4, col].Value = "Định biên";

                col++;
                sheet1.Cells[4, col].Value = "Lương theo chức vụ";
                col++;
                sheet1.Cells[4, col].Value = "Quỹ lương";
                col++;

            }
            col--;
            DrawTableExcel(sheet1, 3, 4, 1, col);
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;

            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ImportDieuChinhLuongNhanVien(ApplicationDbContext db, string FileName)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

            string pathTemplate =PathUploads+ FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            var listCongTy = _CallWebService.GetDanhSachCongTy(db).OrderBy(a => a.MaBoPhan).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listCapBacLuong = db.HRM_dmCapBacLuong.ToList();
            int countPC = listCongTy.Count;

            List<HRM_TuyenDung_dinhBienCoSo_ChucDanh> ListResult = new List<HRM_TuyenDung_dinhBienCoSo_ChucDanh>();
            //if (ListData.Count > 0)
            //{
            //    ListResult = ListData.Where(a => listId.Contains(a.IdNhanVien)).ToList();
            //}
            var sheet1 = package.Workbook.Worksheets.First();
            if (GetCell(sheet1, 1, 1) != "ĐỊNH BIÊN CƠ SỞ")
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không đúng định dạng file mẫu");
            for (int row = 5; row <= sheet1.Cells.End.Row; row++)
            {
                HRM_TuyenDung_dinhBienCoSo_ChucDanh itemAdd = new HRM_TuyenDung_dinhBienCoSo_ChucDanh();
                int ncol = 2;
                var MaBoPhan = GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(MaBoPhan))
                    break;
                var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == MaBoPhan);
                if (BoPhan != null)
                    itemAdd.IdBoPhanEOS = BoPhan.Id;
                ncol++;
                ncol++;
                var MaChucDanh = GetCell(sheet1, row, ncol);
                var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == MaChucDanh);
                if (ChucDanh != null)
                {
                    itemAdd.TenChucDanh = ChucDanh.Ten;
                    itemAdd.IdChucDanh = ChucDanh.Id;
                }

                ncol++;
                ncol++;
                itemAdd.listCongTy = new List<HRM_TuyenDung_DinhBienCoSo_CongTy>();
                for (int i = 0; i < countPC; i++)
                {
                    HRM_TuyenDung_DinhBienCoSo_CongTy CtAdd = new HRM_TuyenDung_DinhBienCoSo_CongTy();
                    var maCongTy = GetCell(sheet1, row, ncol);
                    var CongTy = listCongTy.FirstOrDefault(a => a.MaBoPhan == maCongTy);
                    if (CongTy != null)
                    {
                        CtAdd.IdCongTyEOS = CongTy.Id;

                    }
                    CtAdd.IdBoPhanEOS = itemAdd.IdBoPhanEOS;
                    CtAdd.IdChucDanh = itemAdd.IdChucDanh;
                    var maCapBacLuong = GetCell(sheet1, row, ncol);
                    var CapBacLuong = listCapBacLuong.FirstOrDefault(a => a.Ma == maCapBacLuong);
                    if (CapBacLuong != null)
                    {
                        CtAdd.IdCapBacLuongDeXuat = CapBacLuong.IdGuid;
                        CtAdd.Luong = CapBacLuong.Luong;
                    }
                    ncol++;
                    ncol++;
                    CtAdd.DinhBien = GetCellToDouble(sheet1, row, ncol);
                    ncol++;//lương chức vụ
                    ncol++;// quỹ lương
                    CtAdd.QuyLuong = GetCellToDouble(sheet1, row, ncol);
                    itemAdd.listCongTy.Add(CtAdd);
                }
                ListResult.Add(itemAdd);
            }


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", ListResult);
        }


        #endregion



        #region tuyển dụng
        public string GetNextMaKeHoachTuyenDung(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_TuyenDung_KeHoach.Select(x => x.SoPhieu), DefineData.MaKeHoachTuyenDung, digits: 3);

        // view all cho trưởng phòng nhân sự
        public dynamic GetDanhSachKeHoachTuyenDung(dynamic dyn, ApplicationDbContext db = null)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = dyn.KeyWord;
                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int PageSize = itemTimKiem.PageSize ?? 25;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;

                IQueryable<HRM_TuyenDung_KeHoach> query = db.HRM_TuyenDung_KeHoach;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));
                if (itemTimKiem.Nam > 0)
                    query = query.Where(dm => dm.Nam == itemTimKiem.Nam.Value);

                List<HRM_TuyenDung_KeHoach> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;
                List<BoPhan> listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in itemList)
                {
                    if (item.IdBoPhanEOS >= 0)
                    {
                        BoPhan boPhan = listBoPhan.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
                        if (boPhan != null)
                            item.TenPhongBanEOS = boPhan.TenBoPhan;
                    }

                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra " + e.Message);
            }

        }

        public dynamic SetKeHoachTuyenDung(HRM_TuyenDung_KeHoach model, ApplicationDbContext db)
        {

            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    bool Update = FuncSetQuyTrinhKetHoachTuyenDung(model, db);
                    if (Update == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật kế hoạch tuyển dụng không thành công");
                    }
                    if (model.IdTrangThai == null || model.IdTrangThai == "")
                    {


                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.KEHOACHTUYENDUNG.ToString(), model.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {
                            ts.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        }

                        model.IdTrangThai = trangThai.IdTrangThai;
                        model.TenTrangThai = trangThai.TenTrangThai;
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }

                    db.SaveChanges();


                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật kế hoạch tuyển dụng thành công", model.IdGuid);
                }
                catch (Exception ex)
                {
                    ts.Rollback();

                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }


        }

        public bool FuncSetQuyTrinhKetHoachTuyenDung(HRM_TuyenDung_KeHoach item, ApplicationDbContext db)
        {
            try
            {

                if (!CheckID(item.IdGuid))
                {

                    item.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    var listChiTietOlde = db.HRM_TuyenDung_KeHoach_ChiTiet.Where(r => r.IdTuyenDungKeHoach == item.IdGuid).ToList();
                    db.HRM_TuyenDung_KeHoach_ChiTiet.RemoveRange(listChiTietOlde);
                }
                db.SaveChanges();

                var lstChiTiet = item.listChiTiet;
                foreach (var itemChiTiet in lstChiTiet)
                {
                    itemChiTiet.IdTuyenDungKeHoach = item.IdGuid;

                    string IdChiTiet = itemChiTiet.IdGuid;
                    if (itemChiTiet.isDeleted == false)
                    {
                        itemChiTiet.IdBoPhanEOS = item.IdBoPhanEOS;
                        itemChiTiet.ThoiGianDiLamDuKien = dungchung.UnixTimeStampToDateTime(itemChiTiet.ThoiGianDiLamDuKienUnix);
                        itemChiTiet.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }

                }

                db.SaveChanges();


                if (item.lstFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in item.lstFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_TuyenDung_KeHoach, item.IdGuid);
                    }
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public EntitySingleResponse<HRM_TuyenDung_KeHoach> GetKeHoachTuyenDung(string aId, ApplicationDbContext aContext)
        {
            var result = new EntitySingleResponse<HRM_TuyenDung_KeHoach>();



            var model = aContext.HRM_TuyenDung_KeHoach.FirstOrDefault(x => x.IdGuid == aId);
            if (model == null)
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặch đã bị xóa");
                return result;
            }

            var boPhan = _CallWebService.GetBoPhan(aContext, model.IdBoPhanEOS);
            if (boPhan != null)
            {
                model.TenPhongBanEOS = boPhan.TenBoPhan;
            }

            var listChiTiet = aContext.HRM_TuyenDung_KeHoach_ChiTiet.Where(x => x.IdTuyenDungKeHoach == model.IdGuid).ToList();
            foreach (var item in listChiTiet)
            {
                item.ThoiGianDiLamDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianDiLamDuKien);
            }
            if (listChiTiet.IsNotEmpty())
            {
                model.listChiTiet = listChiTiet;
            }

            var listFile = aContext.HRM_FileDinhKem.Where(x => x.idItem == model.IdGuid).ToList();
            if (listFile.IsNotEmpty())
            {
                model.lstFileDinhKem = listFile;
            }

            result.Item = model;

            return result;
        }

        public MessageReturn DeleteKeHoachTuyenDung(List<string> aListId, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu kế hoạch tuyển dụng thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id truyền nên");
            }

            var listTuyenDung = aContext.HRM_TuyenDung_KeHoach.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listTuyenDung.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            var listChiTiet = aContext.HRM_TuyenDung_KeHoach_ChiTiet.Where(x => aListId.Contains(x.IdTuyenDungKeHoach)).ToList();
            if (listChiTiet.IsNotEmpty())
            {
                aContext.HRM_TuyenDung_KeHoach_ChiTiet.RemoveRange(listChiTiet);
            }

            var listFile = aContext.HRM_FileDinhKem.Where(x => aListId.Contains(x.idItem)).ToList();
            if (listFile.IsNotEmpty())
            {
                aContext.HRM_FileDinhKem.RemoveRange(listFile);
            }

            aContext.HRM_TuyenDung_KeHoach.RemoveRange(listTuyenDung);

            aContext.SaveChanges();

            return result;
        }

        public MessageReturn ChuyenTiepKeHoachTuyenDung(HRM_TuyenDung_KeHoach model, ApplicationDbContext db)
        {


            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_KeHoach pcuDatabase = db.HRM_TuyenDung_KeHoach.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            ts.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");

                        }

                    }
                    var check = FuncSetQuyTrinhKetHoachTuyenDung(model, db);
                    if (check == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật dữ liệu không thành công");
                    }
                    var trangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.KEHOACHTUYENDUNG.ToString(), model.IdTrangThai, model.IdGuid);
                    if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, trangThai.TenTrangThai);
                    }


                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;

                    if (trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                        model.isKetThuc = true;
                    db.Entry(model).State = EntityState.Modified;
                    if (model.isKetThuc == true && !string.IsNullOrEmpty(model.IdQuyTrinh))
                    {
                        var quyTrinhOld = db.HRM_TuyenDung_KeHoach.Find(model.IdQuyTrinh);
                        dynamic TrangThaiHuy = GetTrangThaiHuy(db, DefineData.eAction.KEHOACHTUYENDUNG.ToString(), model.IdGuid);
                        quyTrinhOld.IdTrangThai = TrangThaiHuy.IdTrangThai;
                        quyTrinhOld.TenTrangThai = TrangThaiHuy.TenTrangThai;


                    }
                    db.SaveChanges();


                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }


        }

        public MessageReturn KhongDuyetKeHoachTuyenDung(HRM_TuyenDung_KeHoach QuyTrinh, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    var check = FuncSetQuyTrinhKetHoachTuyenDung(QuyTrinh, db);
                    if (check == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật dữ liệu không thành công");
                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.KEHOACHTUYENDUNG.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);

                    if (trangThai == null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu tuyển dụng khi chuyển");
                    }

                    QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    db.Entry(QuyTrinh).State = EntityState.Modified;

                    db.SaveChanges();


                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }

            }
        }


        #endregion

        public string GetNextMaTongHopKeHoachTuyenDung(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_TuyenDung_KeHoach_TongHop.Select(x => x.SoPhieu), DefineData.MaKeHoachTuyenDung, digits: 3);
        public dynamic GetDanhSachKeHoachDaDuyet(TimKiem timKiem, ApplicationDbContext db)
        {

            int CurrentPage = timKiem.CurrentPage ?? 1;
            int PageSize = timKiem.PageSize ?? 15;
            int Nam = timKiem.Nam ?? 0;
            IQueryable<HRM_TuyenDung_KeHoach> listChamCongDaDuyet = from a in db.HRM_TuyenDung_KeHoach
                                                                     where (timKiem.IdCongTyEOS == 0 || a.IdCongTyEOS == timKiem.IdCongTyEOS)
                                                                    && a.Nam == Nam
                                                                 && (timKiem.KeyWord == "" || timKiem.KeyWord == null || a.SoPhieu.Contains(timKiem.KeyWord))
                                                                    && a.isKetThuc == true
                                                                     select a;
            int count = listChamCongDaDuyet.Count();
            var item = listChamCongDaDuyet.OrderByDescending(q => q.Modified).Skip(PageSize * (CurrentPage - 1)).Take(PageSize).ToList();

            dynamic dnm = new DynamicObject();
            dnm.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
            dnm.items = item;
            return dnm;
        }
        #region TongHopKeHoachTuyenDung
        public dynamic GetTongHopKeHoacTuyenDung(dynamic dyn, ApplicationDbContext db)
        {
            List<string> lstIdKeHoach = JsonConvert.DeserializeObject<List<string>>(dyn.lstIdKeHoach.ToString());
            int Nam = dyn.Nam ?? 0;

            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var ChucDanhs = _CallWebService.GetDanhSachChucDanh(db);
            var TrinhDos = db.HRM_dmTrinhDoDaoTao.ToList();

            List<TuyenDungKeHoachTongHopChiTietModel> lstTongHop = db.HRM_TuyenDung_KeHoach_ChiTiet
                .Where(x => lstIdKeHoach.Contains(x.IdTuyenDungKeHoach))
                .GroupBy(x => new { x.IdBoPhanEOS, x.IdChucDanhEOS, x.IddmTrinhDo, x.ThoiGianDiLamDuKien })
                .Select(g => new TuyenDungKeHoachTongHopChiTietModel
                {
                    IdBoPhanEOS = g.Key.IdBoPhanEOS,
                    IdChucDanhEOS = g.Key.IdChucDanhEOS,
                    ThoiGianDiLamDuKien = g.Key.ThoiGianDiLamDuKien,
                    IddmTrinhDo = g.Key.IddmTrinhDo,
                    SoLuong = g.Sum(x => x.SoLuong),
                    ThayThe = g.Sum(x => x.ThayThe),
                    BoSung = g.Sum(x => x.BoSung)
                })
                .ToList();
            var DinhBiens = db.HRM_DinhBien.Where(r => r.Nam == Nam).ToList();
            foreach (var item in lstTongHop)
            {
                item.ThoiGianDiLamDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianDiLamDuKien);
                item.TenBoPhan = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                item.TenChucDanh = ChucDanhs.Where(r => r.Id == item.IdChucDanhEOS).Select(r => r.Ten).FirstOrDefault();
                item.TrinhDo = TrinhDos.Where(r => r.IdGuid == item.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
                item.LuongDinhBien = DinhBiens.Where(r => r.IdChucDanhEOS == item.IdChucDanhEOS && r.IdBoPhanEOS == item.IdBoPhanEOS).Select(r => r.LuongDinhBien).FirstOrDefault();
            }
            return lstTongHop;
        }

        public dynamic SetTongHopKeHoachTuyenDung(HRM_TuyenDung_KeHoach_TongHop model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string Update = FuncUpdateTongHopQuyTrinh(model, db);
                    if (Update != "0")
                    {
                        dbContextTransaction.Rollback();

                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật tổng hơp không thành công " + Update);
                    }
                    if (string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.KEHOACHTUYENDUNGTONGHOP.ToString(), model.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {
                            dbContextTransaction.Rollback();

                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        }

                        model.IdTrangThai = trangThai.IdTrangThai;
                        model.TenTrangThai = trangThai.TenTrangThai;
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }



                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công!");
                }
            }

        }

        public string FuncUpdateTongHopQuyTrinh(HRM_TuyenDung_KeHoach_TongHop model, ApplicationDbContext db)
        {
            try
            {
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_TuyenDung_KeHoach_TongHop.Add(model);
                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                    var listChiTietOlde = db.HRM_TuyenDung_KeHoach_TongHopChiTiet.Where(r => r.IdKetHoachTongHop == model.IdGuid).ToList();
                    db.HRM_TuyenDung_KeHoach_TongHopChiTiet.RemoveRange(listChiTietOlde);
                    var listIdKeHoach = db.HRM_TuyenDung_TongHop_KeHoach.Where(r => r.IdKetHoachTongHop == model.IdGuid).ToList();
                    db.HRM_TuyenDung_TongHop_KeHoach.RemoveRange(listIdKeHoach);

                }
                foreach (var item in model.listIdKeHoach)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdKetHoachTongHop = model.IdGuid;
                    db.HRM_TuyenDung_TongHop_KeHoach.Add(item);
                }
                db.SaveChanges();

                var lstChiTiet = model.lstChiTiet;
                foreach (var itemChiTiet in lstChiTiet)
                {
                    itemChiTiet.IdKetHoachTongHop = model.IdGuid;

                    string IdChiTiet = itemChiTiet.IdGuid;
                    if (itemChiTiet.isDeleted == false)
                    {

                        itemChiTiet.ThoiGianDiLamDuKien = UnixTimeStampToDateTime(itemChiTiet.ThoiGianDiLamDuKienUnix);
                        itemChiTiet.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                        if (itemChiTiet.lstNguonTuyenDung != null)
                        {
                            foreach (var item in itemChiTiet.lstNguonTuyenDung)
                            {
                                item.IdGuid = Guid.NewGuid().ToString();
                                item.IdKetHoachChiTiet = itemChiTiet.IdGuid;
                                db.HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung.Add(item);
                            }
                        }
                    }


                }
                db.SaveChanges();
                return "0";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public dynamic ChuyenTiepTongHopKeHoachTuyenDung(HRM_TuyenDung_KeHoach_TongHop model, ApplicationDbContext db)
        {


            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_KeHoach_TongHop pcuDatabase = db.HRM_TuyenDung_KeHoach_TongHop.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            ts.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");

                        }

                    }
                    string check = FuncUpdateTongHopQuyTrinh(model, db);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công " + check);
                    }
                    var trangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.KEHOACHTUYENDUNGTONGHOP.ToString(), model.IdTrangThai, model.IdGuid);
                    //   var trangThai = GetTrangThaiChuyenTiep(smartdb, db, model.IdTrangThai, model.IdGuid, db.IdUser);
                    if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, trangThai.TenTrangThai);
                    }
                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;

                    if (trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                        model.isKetThuc = true;
                    db.Entry(model).State = EntityState.Modified;

                    db.SaveChanges();

                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp quy trình thành công!");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }


        }

        public dynamic GetListKeHoachTongHop(dynamic dyn, ApplicationDbContext db)
        {
            //int CurrentPage = timKiem.CurrentPage ?? 1;
            //int PageSize = timKiem.PageSize ?? 15;
            //int Nam = timKiem.Nam ?? 0;


            //IEnumerable<HRM_TuyenDung_KeHoach_TongHop> listTongHop = from a in db.HRM_TuyenDung_KeHoach_TongHop
            //                                                         where a.Nam == Nam
            //                                                         select a;
            //int count = listTongHop.Count();
            //var item = listTongHop.OrderByDescending(q => q.Modified).Skip(PageSize * (CurrentPage - 1)).Take(PageSize).ToList();

            //dynamic dnm = new DynamicObject();
            //dnm.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
            //dnm.items = item;
            //return dnm;
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = dyn.KeyWord;
                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int PageSize = itemTimKiem.PageSize ?? 25;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;

                IQueryable<HRM_TuyenDung_KeHoach_TongHop> query = db.HRM_TuyenDung_KeHoach_TongHop;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));
                if (itemTimKiem.Nam > 0)
                    query = query.Where(dm => dm.Nam == itemTimKiem.Nam.Value);

                List<HRM_TuyenDung_KeHoach_TongHop> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;
                //  List<HRM.Api.Models.SmartEOEModel.BoPhan> listBoPhan = smdb.BoPhans.ToList();
                //foreach (var item in itemList)
                //{
                //    if (item.IdBoPhanEOS >= 0)
                //    {
                //        HRM.Api.Models.SmartEOEModel.BoPhan boPhan = listBoPhan.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
                //        if (boPhan != null)
                //            item.TenPhongBanEOS = boPhan.TenBoPhan;
                //    }

                //}
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra " + e.Message);
            }
        }

        public dynamic GetTongHopKeHoachById(string Id, ApplicationDbContext db)
        {
            HRM_TuyenDung_KeHoach_TongHop model = db.HRM_TuyenDung_KeHoach_TongHop.Where(r => r.IdGuid == Id).FirstOrDefault();
            var lstChiTiet = db.HRM_TuyenDung_KeHoach_TongHopChiTiet.Where(r => r.IdKetHoachTongHop == model.IdGuid).ToList();
            model.listIdKeHoach = db.HRM_TuyenDung_TongHop_KeHoach.Where(r => r.IdKetHoachTongHop == model.IdGuid).ToList();
            foreach (var item in lstChiTiet)
            {
                item.ThoiGianDiLamDuKienUnix = DateTimeToUnixTimeStamp(item.ThoiGianDiLamDuKien);
                item.lstNguonTuyenDung = db.HRM_TuyenDung_KeHoach_TongHopChiTiet_NguonTuyenDung.Where(r => r.IdKetHoachChiTiet == item.IdGuid).ToList();
            }
            model.lstChiTiet = lstChiTiet;
            return model;
        }

        public dynamic DeleteTongHopKeHoach(string Id, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {

                    HRM_TuyenDung_KeHoach_TongHop model = db.HRM_TuyenDung_KeHoach_TongHop.Where(r => r.IdGuid == Id).FirstOrDefault();
                    var lstChiTiet = db.HRM_TuyenDung_KeHoach_TongHopChiTiet.Where(r => r.IdKetHoachTongHop == model.IdGuid).ToList();
                    db.HRM_TuyenDung_KeHoach_TongHopChiTiet.RemoveRange(lstChiTiet);
                    db.HRM_TuyenDung_KeHoach_TongHop.Remove(model);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa tổng hợp thành công");

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công!");
                }
            }
        }
        public MessageReturn KhongDuyetTongHopKeHoachTuyenDung(HRM_TuyenDung_KeHoach_TongHop QuyTrinh, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    string check = FuncUpdateTongHopQuyTrinh(QuyTrinh, db);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật dữ liệu không thành công " + check);
                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.KEHOACHTUYENDUNGTONGHOP.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);

                    if (trangThai == null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu tuyển dụng khi chuyển");
                    }

                    QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    db.Entry(QuyTrinh).State = EntityState.Modified;

                    db.SaveChanges();


                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }

        #endregion
    }
    public class TuyenDungKeHoachTongHopChiTietModel
    {


        public int BoSung { get; set; }
        public int ThayThe { get; set; }
        public int SoLuong { set; get; }
        public double LuongDinhBien { set; get; }

        public string IddmTrinhDo { get; set; }
        public string TrinhDo { get; set; }
        public DateTime ThoiGianDiLamDuKien { get; set; }
        public double ThoiGianDiLamDuKienUnix { get; set; }
        public int ThoiGianLamDuKien { get; set; }

        public long IdChucDanhEOS { get; set; }
        public string TenChucDanh { set; get; }
        public long IdBoPhanEOS { get; set; }
        public string TenBoPhan { set; get; }

    }
}