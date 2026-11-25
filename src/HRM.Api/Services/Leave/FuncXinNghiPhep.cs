#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Helper;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;

//using HRM.Api.Models.SmartEOEModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xceed.Words.NET;
using static HRM.Api.Services.CallWebService;
using static HRM.Api.Helper.HelperConstant;

namespace HRM.Api.Services.XinNghiPhep
{
    public class FuncXinNghiPhep : FuncBase
    {
        public FuncXinNghiPhep(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncXinNghiPhep> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        #region lamThemGio
        public dynamic GetQuyTrinhLamThemGio(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");

                HRM_QuyTrinhLamThemGio QuyTrinh = db.HRM_QuyTrinhLamThemGio.FirstOrDefault(x => x.IdGuid == Id);
                QuyTrinh.listUser = new List<HRM_QuyTrinhLamThemGio_NhanVien>();

                if (QuyTrinh.IdBoPhan != null)
                {
                    QuyTrinh.listIdBoPhan = new List<long>();
                    if (QuyTrinh.IdBoPhan.Contains(","))
                    {
                        string[] list = QuyTrinh.IdBoPhan.Split(',');
                        foreach (var bp in list)
                            QuyTrinh.listIdBoPhan.Add(long.Parse(bp));
                    }
                    else QuyTrinh.listIdBoPhan.Add(long.Parse(QuyTrinh.IdBoPhan));
                }
                var listUser = db.HRM_QuyTrinhLamThemGio_ChiTiet.Where(r => r.IdQuyTrinh == Id).ToList();
                List<string> listIdUser = listUser.Select(a => a.IdUser).Distinct().ToList();
                var listnhanVien = db.HRM_NhanVien.Where(a => listIdUser.Contains(a.IdGuid)).ToList();
                CallWebService _callweb = _CallWebService;
                var listBoPhan = _callweb.GetDanhSachBoPhan(db);
                var listChucDanh = _callweb.GetDanhSachChucDanh(db);
                var listChucVu = _callweb.GetDanhSachChucVu(db);

                List<HRM_QuyTrinhLamThemGio_NhanVien> listNhanVienAdd = new List<HRM_QuyTrinhLamThemGio_NhanVien>();
                foreach (var item in listUser)
                    item.DateTimetoUnix();
                foreach (var NhanVien in listnhanVien)
                {
                    HRM_QuyTrinhLamThemGio_NhanVien item = new HRM_QuyTrinhLamThemGio_NhanVien();
                    item.listNgay = new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
                    item.MaNhanVien = NhanVien.MaNhanVien;
                    item.TenNhanVien = NhanVien.TenDayDu;
                    item.IdUser = NhanVien.IdGuid;
                    item.IdBoPhan = NhanVien.IdBoPhanEOS;
                    item.TenBoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS) == null ? null : listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS).TenBoPhan;
                    item.TenChucDanh = listChucDanh.FirstOrDefault(a => a.Id == NhanVien.IdChucDanh) == null ? null : listChucDanh.FirstOrDefault(a => a.Id == NhanVien.IdChucDanh).Ten;
                    item.TenChucVu = listChucVu.Where(a => a.Id == NhanVien.IdChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                    item.listNgay = listUser.Where(a => a.IdUser == NhanVien.IdGuid).ToList();
                    listNhanVienAdd.Add(item);
                }
                QuyTrinh.listUser = listNhanVienAdd;
                if (!GetQuyTrinh(db, _callweb, QuyTrinh, err, true))
                    return err;
                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetNextSoQuyTrinhLamThemGio(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "HRMLTG",
                db.HRM_QuyTrinhLamThemGio.OrderByDescending(a => a.Created).Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListQuyTrinhLamThemGio(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_QuyTrinhLamThemGio> result;

                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyTrinhLamThemGio
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhLamThemGio
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Created >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Created < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => (!string.IsNullOrEmpty(a.SoQuyTrinh) && a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.TenNguoiDeNghi) && a.TenNguoiDeNghi.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (!string.IsNullOrEmpty(TimKiem.IdUser))
                {

                    var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == TimKiem.IdUser);
                    result = result.Where(a => !string.IsNullOrEmpty(a.IdNguoiDeNghi) && a.IdNguoiDeNghi == NhanVien.IdUser);
                }
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                result = result.ToList();
                List<string> listId = new List<string>();
                if (TimKiem.IdBoPhan > 0)
                {

                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == nameof(HRM_QuyTrinhLamThemGio) && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid)).ToList();
                }
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();

                CallWebService webService = _CallWebService;
                List<BoPhan> BoPhans = webService.GetDanhSachBoPhan(db);
                var listIdUserCreatedBy = itemList.GroupBy(q => q.CreatedBy).Select(q => q.Key).ToList();
                var listUser = webService.GetDanhSachUserTheoListIdUser(db, listIdUserCreatedBy);
                listId = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listItem = (from a in db.HRM_QuyTrinhLamThemGio_ChiTiet
                                select a
                                ).ToList();
                var listIdNv = listItem.Select(a => a.IdUser).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();

                foreach (HRM_QuyTrinhLamThemGio item in itemList)
                {
                    item.ItemUserCreatedBy = listUser.FirstOrDefault(q => q.Id == item.CreatedBy);
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
                        List<BoPhan> BoPhan = BoPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                        if (BoPhan != null)
                            item.TenBoPhan = string.Join(",", BoPhan.Select(a => a.TenBoPhan));

                        var itemdata = listItem.Where(a => a.IdQuyTrinh == item.IdGuid).ToList();
                        if (itemdata != null && itemdata.Count > 0)
                        {
                            item.listNoiDung = new List<string>();
                            listIdNv = itemdata.Select(a => a.IdUser).Distinct().ToList();
                            foreach (var Id in listIdNv)
                            {
                                var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == Id);
                                double TGio = itemdata.Where(a => a.IdUser == Id).Sum(a => a.ThoiGian);
                                string ThoiGian = string.Join(", ", itemdata.Where(a => a.IdUser == Id).Select(a => a.ThoiGianBatDau.Value.ToString("dd/MM/yyyy") + ":" + a.TuGio.Value.ToString("hh/mm") + "-" + a.ThoiGianKetThuc.Value.ToString("dd/MM/yyyy") + ":" + a.DenGio.Value.ToString("hh/mm")).ToList());
                                if (NhanVien != null)
                                    item.listNoiDung.Add(NhanVien.TenDayDu + "(" + NhanVien.MaNhanVien + "): " + String.Format("{0:0}", TGio) + " Giờ " + ThoiGian);

                            }
                        }

                    }



                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }


        public dynamic SetQuyTrinhLamThemGio(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhLamThemGio QuyTrinh = JsonConvert.DeserializeObject<HRM_QuyTrinhLamThemGio>(data.ToString());

                    var QuyTrinhdb = db.HRM_QuyTrinhLamThemGio.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, QuyTrinh, err))
                        return err;

                    MessageReturn check = UpdateQuyTrinhLamThemGio(QuyTrinh, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check.sError);
                    }

                    if (!SetSauHamUpdate(db, QuyTrinhdb, QuyTrinh, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
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

        public dynamic UpdateQuyTrinhLamThemGio(HRM_QuyTrinhLamThemGio QuyTrinh, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                //string checkHRM = ConfigurationManager.AppSettings["ISHRM"] ?? "0";
                //if (checkHRM == "1")
                //{
                //    List<HRM_QuyTrinhLamThemGio_ChiTiet> listNgay = QuyTrinh.listUser;

                //}
                if (QuyTrinh.listIdBoPhan != null && QuyTrinh.listIdBoPhan.Count > 0)
                    QuyTrinh.IdBoPhan = string.Join(",", QuyTrinh.listIdBoPhan);
                if (!string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {

                    QuyTrinh.Modified = DateTime.Now;

                    db.Entry(QuyTrinh).State = EntityState.Modified;
                    //_CallWebService.AddModify(db, "Xin nghỉ phép", DefineData.eModify.Them.ToString(), DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString(),QuyTrinh.NoiDung, QuyTrinh.IdGuid, "", JsonConvert.SerializeObject(QuyTrinh), QuyTrinh.IdCongTy, db.currentUser.Id, db.currentUser.TenNhanVien);
                }
                else
                {
                    int Check = db.HRM_QuyTrinhLamThemGio.Where(r => r.SoPhieu == QuyTrinh.SoPhieu).Count();
                    if (Check > 0)
                    {
                        QuyTrinh.SoQuyTrinh = GetNextSoQuyTrinhLamThemGio(db).SoPhieu.ToString();
                        QuyTrinh.SoPhieu = QuyTrinh.SoQuyTrinh;
                    }
                    QuyTrinh.eAction = DefineData.eAction.QUYTRINHLAMTHEMGIO.ToString();
                    QuyTrinh.IdDuAn = QuyTrinh.IdCongTy;
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return err.Value;

                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    //_CallWebService.AddModify(db, "Xin nghỉ phép", DefineData.eModify.Sua.ToString(), DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString(), QuyTrinh.NoiDung, QuyTrinh.IdGuid, "", JsonConvert.SerializeObject(QuyTrinh), QuyTrinh.IdCongTy, db.currentUser.Id, db.currentUser.TenNhanVien);

                }
                var ListUser = db.HRM_QuyTrinhLamThemGio_ChiTiet.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_QuyTrinhLamThemGio_ChiTiet.RemoveRange(ListUser);
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                if (QuyTrinh.listIdBoPhan != null && QuyTrinh.listIdBoPhan.Count > 0)
                {
                    foreach (var item in QuyTrinh.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = QuyTrinh.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_QuyTrinhLamThemGio);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                List<HRM_QuyTrinhLamThemGio_ChiTiet> lstAdd = new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
                if (QuyTrinh.listUser != null && QuyTrinh.listUser.Count > 0)
                {

                    foreach (var nhanVien in QuyTrinh.listUser)
                    {
                        if (nhanVien.listNgay != null && nhanVien.listNgay.Count > 0)
                        {
                            foreach (var item in nhanVien.listNgay)
                            {
                                item.UnixToDateTime();
                                item.DenGio = new DateTime(item.DenGio.Value.Year, item.DenGio.Value.Month, item.DenGio.Value.Day, item.DenGio.Value.Hour, item.DenGio.Value.Minute, 0);
                                item.TuGio = new DateTime(item.TuGio.Value.Year, item.TuGio.Value.Month, item.TuGio.Value.Day, item.TuGio.Value.Hour, item.TuGio.Value.Minute, 0);
                                item.ThoiGianBatDau = new DateTime(item.ThoiGianBatDau.Value.Year, item.ThoiGianBatDau.Value.Month, item.ThoiGianBatDau.Value.Day, 0, 0, 0);
                                item.ThoiGianKetThuc = new DateTime(item.ThoiGianKetThuc.Value.Year, item.ThoiGianKetThuc.Value.Month, item.ThoiGianKetThuc.Value.Day, 0, 0, 0);
                                item.IdUser = nhanVien.IdUser;
                                item.IdBoPhan = nhanVien.IdBoPhan;
                                DateTime time = item.ThoiGianBatDau.Value;
                                TimeSpan timeSpan = item.DenGio.Value - item.TuGio.Value;
                                if (item.DenGio < item.TuGio)
                                    timeSpan = item.DenGio.Value.AddDays(1) - item.TuGio.Value;
                                double hours = timeSpan.TotalHours;

                                TimeSpan ngay = (TimeSpan)(item.ThoiGianKetThuc - item.ThoiGianBatDau);

                                item.ThoiGian = (ngay.TotalDays + 1) * hours;
                                item.Id = Guid.NewGuid().ToString();
                                item.IdQuyTrinh = QuyTrinh.IdGuid;
                                lstAdd.Add(item);
                            }
                        }
                    }
                    db.HRM_QuyTrinhLamThemGio_ChiTiet.AddRange(lstAdd);
                }
                if (QuyTrinh.isKetThuc == true && lstAdd != null && lstAdd.Count > 0)
                {
                    List<string> listIdNhanVien = lstAdd.Select(a => a.IdUser).Distinct().ToList();
                    DateTime Tu = lstAdd.OrderBy(a => a.ThoiGianBatDau).FirstOrDefault().ThoiGianBatDau.Value.Date;
                    DateTime Den = lstAdd.OrderByDescending(a => a.ThoiGianKetThuc).FirstOrDefault().ThoiGianKetThuc.Value.Date;

                    var listdbOld = (from a in db.HRM_TheoDoiLamThemGio
                                     where listIdNhanVien.Contains(a.IdNhanVien) && Tu <= a.NgayDangKy && Den >= a.NgayDangKy
                                     select a
                                   ).ToList();
                    var listLLVdbOld = (from a in db.HRM_TheoDoiLichLamViecNhanVien
                                        where listIdNhanVien.Contains(a.IdNhanVien) && Tu <= a.Ngay && Den >= a.Ngay && a.isLamThemGio == true
                                        select a
                                 ).ToList();
                    List<HRM_TheoDoiLamThemGio> listAdd = new List<HRM_TheoDoiLamThemGio>();
                    List<HRM_TheoDoiLamThemGio> listDelete = new List<HRM_TheoDoiLamThemGio>();
                    List<HRM_TheoDoiLichLamViecNhanVien> listLLVAdd = new List<HRM_TheoDoiLichLamViecNhanVien>();
                    List<HRM_TheoDoiLichLamViecNhanVien> listLLVDelete = new List<HRM_TheoDoiLichLamViecNhanVien>();

                    foreach (var item in lstAdd)
                    {
                        item.DenGio = new DateTime(item.DenGio.Value.Year, item.DenGio.Value.Month, item.DenGio.Value.Day, item.DenGio.Value.Hour, item.DenGio.Value.Minute, 0);
                        item.TuGio = new DateTime(item.TuGio.Value.Year, item.TuGio.Value.Month, item.TuGio.Value.Day, item.TuGio.Value.Hour, item.TuGio.Value.Minute, 0);
                        DateTime time = item.ThoiGianBatDau.Value;
                        TimeSpan timeSpan = item.DenGio.Value - item.TuGio.Value;
                        double hours = timeSpan.TotalHours;
                        while (time <= item.ThoiGianKetThuc)
                        {
                            HRM_TheoDoiLamThemGio itemAdd = new HRM_TheoDoiLamThemGio(item);
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            itemAdd.IdBoPhan = item.IdBoPhan;
                            itemAdd.IdCongTy = QuyTrinh.IdCongTy;
                            itemAdd.NgayDangKy = time;
                            itemAdd.ThoiGian = hours;
                            listAdd.Add(itemAdd);

                            var Delete = listdbOld.FirstOrDefault(a => a.IdNhanVien == item.IdUser && a.NgayDangKy == time);
                            if (Delete != null)
                                listDelete.Add(Delete);
                            HRM_TheoDoiLichLamViecNhanVien itemAddLLV = new HRM_TheoDoiLichLamViecNhanVien();
                            itemAddLLV.IdCongTy = QuyTrinh.IdCongTy;
                            itemAddLLV.IdBoPhan = item.IdBoPhan;
                            itemAddLLV.IdNhanVien = item.IdUser;
                            itemAddLLV.IdGuid = Guid.NewGuid().ToString();
                            itemAddLLV.Ngay = time;
                            itemAddLLV.LyDo = item.GhiChu;

                            listLLVAdd.Add(itemAddLLV);

                            var DeleteLLV = listLLVdbOld.FirstOrDefault(a => a.IdNhanVien == item.IdUser && a.Ngay == time);
                            if (DeleteLLV != null)
                                listLLVDelete.Add(DeleteLLV);
                            time = time.AddDays(1);
                        }
                    }
                    if (listAdd != null && listAdd.Count > 0)
                        db.HRM_TheoDoiLamThemGio.AddRange(listAdd);
                    if (listDelete != null && listDelete.Count > 0)
                        db.HRM_TheoDoiLamThemGio.RemoveRange(listDelete);
                    if (listLLVAdd != null && listLLVAdd.Count > 0)
                        db.HRM_TheoDoiLichLamViecNhanVien.AddRange(listLLVAdd);
                    if (listLLVDelete != null && listLLVDelete.Count > 0)
                        db.HRM_TheoDoiLichLamViecNhanVien.RemoveRange(listLLVDelete);

                    var ThoiGianLVOld = (from a in db.HRM_ThoiGianLamViecKeHoach
                                         where listIdNhanVien.Contains(a.IdNhanVien) && Tu <= a.Ngay && Den >= a.Ngay /*&& a.Loai == 1 && a.isLamThemGio == true*/
                                         select a
                                   ).ToList();
                    var XinNghiPhep = (from a in db.HRM_TheoDoiXinNghiPhep
                                       where listIdNhanVien.Contains(a.IdNhanVien) && Tu <= a.NgayNghi && Den >= a.NgayNghi
                                       select a
                                  ).ToList();
                    List<string> listIdCaLamViec = lstAdd.Select(a => a.IddmCaLamViec).Distinct().ToList();
                    var listCaLamViec = db.HRM_dmLichLamViec.Where(a => listIdCaLamViec.Contains(a.IdGuid)).ToList();
                    var listHeSo = db.HRM_dmHeSoLamThemGio.ToList();
                    List<HRM_ThoiGianLamViecKeHoach> listLVAdd = new List<HRM_ThoiGianLamViecKeHoach>();
                    List<HRM_ThoiGianLamViecKeHoach> listLVDelete = new List<HRM_ThoiGianLamViecKeHoach>();
                    foreach (var item in lstAdd)
                    {
                        DateTime time = item.ThoiGianBatDau.Value.Date;
                        TimeSpan timeSpan = item.DenGio.Value - item.TuGio.Value;
                        double hours = timeSpan.TotalHours;
                        var CaLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == item.IddmCaLamViec);
                        var heso = listHeSo.FirstOrDefault(a => a.IdGuid == item.IddmHeSoTangCa);
                        while (time <= item.ThoiGianKetThuc)
                        {
                            HRM_ThoiGianLamViecKeHoach itemAdd = new HRM_ThoiGianLamViecKeHoach();
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            itemAdd.GioVao = new DateTime(time.Year, time.Month, time.Day, item.TuGio.Value.Hour, item.TuGio.Value.Minute, 0);
                            if (item.TuGio < item.DenGio)
                                itemAdd.GioRa = new DateTime(time.Year, time.Month, time.Day, item.DenGio.Value.Hour, item.DenGio.Value.Minute, 0);
                            else
                            {
                                DateTime time1 = time.AddDays(1);
                                itemAdd.GioRa = new DateTime(time1.Year, time1.Month, time1.Day, item.DenGio.Value.Hour, item.DenGio.Value.Minute, 0);
                            }
                            itemAdd.Ngay = time;
                            itemAdd.IdNhanVien = item.IdUser;
                            itemAdd.isCaGay = false;
                            if (heso != null)
                                itemAdd.HeSoTangCa = heso.HeSo;

                            itemAdd.IddmHeSoTangCa = item.IddmHeSoTangCa;
                            if (CaLamViec != null)
                            {
                                itemAdd.IddmCaLamViec = CaLamViec.IdGuid;
                                itemAdd.GioTieuChuan = CaLamViec.SoGioTrongNgay ?? 8;
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 26;
                            }
                            itemAdd.IdCongTy = QuyTrinh.IdCongTy;
                            itemAdd.TongSoGioGiao = hours;
                            itemAdd.SoGioNghi = item.SoGioNghi;
                            itemAdd.Loai = 1;
                            itemAdd.LyDo = item.GhiChu;
                            itemAdd.isLamThemGio = true;
                            var Delete = ThoiGianLVOld.Where(a => a.IdNhanVien == item.IdUser && a.GioVao < itemAdd.GioRa && a.GioRa > itemAdd.GioVao && a.Ngay == time && a.Loai == 1 && a.isLamThemGio == true).ToList();
                            if (Delete != null)
                                listLVDelete.AddRange(Delete);
                            // check làm thêm giờ sau khi xin nghỉ phép
                            {
                                var checkNghi = XinNghiPhep.Where(a => a.NgayNghi == time && a.IdNhanVien == item.IdUser).ToList();
                                if (checkNghi != null && checkNghi.Count > 0)
                                {
                                    var lichLamViecNgay = ThoiGianLVOld.Where(a => a.IdNhanVien == item.IdUser && a.Ngay == time && a.isLamThemGio == false && a.GioVao < itemAdd.GioRa && a.GioRa > itemAdd.GioVao).ToList();
                                    if (lichLamViecNgay != null && lichLamViecNgay.Count > 0)
                                    {
                                        foreach (var item1 in checkNghi)
                                        {
                                            item1.NgayHuy = time;
                                            item1.NgayNghi = new DateTime(1975, 1, 1);
                                            item1.LyDoHuy = "đăng ký làm thêm giờ";
                                            db.Entry(item1).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                        }
                                        foreach (var item1 in lichLamViecNgay)
                                        {
                                            item1.NgayHuy = time;
                                            item1.Ngay = new DateTime(1975, 1, 1);
                                            item1.LyDoHuy = "đăng ký làm thêm giờ và đã có xin nghỉ phép";
                                            db.Entry(item1).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                        }
                                        itemAdd.Loai = 0;
                                        itemAdd.isLamThemGio = false;
                                    }
                                }
                            }
                            listLVAdd.Add(itemAdd);

                            time = time.AddDays(1);

                        }
                    }
                    if (listLVAdd != null && listLVAdd.Count > 0)
                        db.HRM_ThoiGianLamViecKeHoach.AddRange(listLVAdd);
                    if (listLVDelete != null && listLVDelete.Count > 0)
                        db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listLVDelete);
                }
                if (!SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công"); ;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }

        }
        public dynamic DeleteQuyTrinhLamThemGio(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhLamThemGio QuyTrinh = JsonConvert.DeserializeObject<HRM_QuyTrinhLamThemGio>(data.ToString());
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_QuyTrinhLamThemGio_ChiTiet.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                    db.HRM_QuyTrinhLamThemGio_ChiTiet.RemoveRange(ListUser);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", QuyTrinh);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhLamThemGio(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhLamThemGio QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_QuyTrinhLamThemGio)) as HRM_QuyTrinhLamThemGio;

                    HRM_QuyTrinhLamThemGio pcuDatabase = db.HRM_QuyTrinhLamThemGio.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    MessageReturn Error = UpdateQuyTrinhLamThemGio(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.sError, QuyTrinh.IdGuid);
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
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

        public dynamic KhongDuyetQuyTrinhLamThemGio(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhLamThemGio QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_QuyTrinhLamThemGio)) as HRM_QuyTrinhLamThemGio;

                    HRM_QuyTrinhLamThemGio pcuDatabase = db.HRM_QuyTrinhLamThemGio.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    MessageReturn Error = UpdateQuyTrinhLamThemGio(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.Value, QuyTrinh.IdGuid);
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
        public dynamic ExportXuatPhieudeNghiLamThemGio(ApplicationDbContext db, string IdQuyTrinh)
        {
            //
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFile = "Dangkylamthemgio.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "Dangkylamthemgio.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;

                File.Copy(sFileName, sFileNameCopy, true);

                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet SheetQuyTrinh = package.Workbook.Worksheets["Quy Trình"];
                ExcelWorksheet SheetNhanSu = package.Workbook.Worksheets["Nhân sự"];
                //
                HRM_QuyTrinhLamThemGio item = GetQuyTrinhLamThemGio(db, IdQuyTrinh);

                CallWebService callWeb = _CallWebService;
                List<string> listId = new List<string>();
                listId.Add(item.IdNguoiDeNghi);
                if (item.listUser != null && item.listUser.Count > 0)
                {
                    listId.AddRange(item.listUser.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList());
                    listId = listId.Distinct().ToList();
                }
                List<HRM_NhanVien> lstnhanvien = db.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).ToList();
                List<BoPhan> boPhans = callWeb.GetDanhSachBoPhan(db);
                // sheet quy trình
                SheetQuyTrinh.Cells[4, 1].Value = 1;
                BoPhan cty = boPhans.FirstOrDefault(a => a.Id == item.IdCongTy);
                if (cty != null)
                {
                    SheetQuyTrinh.Cells[4, 2].Value = cty.MaBoPhan;
                    //SheetQuyTrinh.Cells[4, 3].Value = cty.TenBoPhan;
                }
                List<BoPhan> BoPhan = boPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                if (BoPhan != null)
                {
                    SheetQuyTrinh.Cells[4, 3].Value = string.Join(",", BoPhan.Select(a => a.MaBoPhan));
                    //SheetQuyTrinh.Cells[4, 5].Value = cty.TenBoPhan;
                }
                if (!string.IsNullOrEmpty(item.IdNguoiDeNghi) && lstnhanvien != null && lstnhanvien.Count > 0)
                {
                    var user = lstnhanvien.FirstOrDefault(a => a.IdGuid == item.IdNguoiDeNghi);
                    if (user != null)
                    {
                        SheetQuyTrinh.Cells[4, 4].Value = user.MaNhanVien;
                        SheetQuyTrinh.Cells[4, 5].Value = user.TenDayDu;

                    }
                }
                SheetQuyTrinh.Cells[4, 6].Value = item.NoiDung;
                SheetQuyTrinh.Cells[4, 7].Value = item.GhiChu;
                dungchung.DrawTableExcel(SheetQuyTrinh, 3, 4, 1, 7);
                // sheet user đăng ký

                if (item.listUser != null && item.listUser.Count > 0)
                {
                    int stt = 1;
                    int nRowBegin = 3;
                    int nRow = nRowBegin;
                    foreach (var user in item.listUser)
                    {
                        int Count = user.listNgay.Count;
                        SheetNhanSu.Cells[nRow, 1, nRow + 1, 1].Merge = true;
                        SheetNhanSu.Cells[nRow, 1].Value = stt;
                        SheetNhanSu.Cells[nRow, 2, nRow + 1, 2].Merge = true;
                        SheetNhanSu.Cells[nRow, 3, nRow + 1, 3].Merge = true;
                        var use = lstnhanvien.FirstOrDefault(a => a.IdGuid == user.IdUser);
                        if (use != null)
                        {
                            SheetNhanSu.Cells[nRow, 2].Value = use.MaNhanVien;
                            SheetNhanSu.Cells[nRow, 3].Value = use.TenDayDu;

                        }
                        foreach (var Ngay in user.listNgay)
                        {
                            SheetNhanSu.Cells[nRow, 4].Value = Ngay.ThoiGianBatDau.Value.ToString("dd/MM/yyyy");
                            SheetNhanSu.Cells[nRow, 5].Value = Ngay.ThoiGianKetThuc.Value.ToString("dd/MM/yyyy");
                            SheetNhanSu.Cells[nRow, 6].Value = Ngay.TuGio.Value.ToString("HH/mm");
                            SheetNhanSu.Cells[nRow, 7].Value = Ngay.DenGio.Value.ToString("HH/mm");
                            SheetNhanSu.Cells[nRow, 8].Value = Ngay.ThoiGian;
                        }
                        stt++;
                        nRow++;
                    }
                    dungchung.DrawTableExcel(SheetNhanSu, nRowBegin, nRow, 1, 8);
                }

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public dynamic ImportXuatPhieudeNghiLamThemGio(ApplicationDbContext db, string FileName)
        {
            //
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                DateTime dt = DateTime.Now;
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet SheetQuyTrinh = package.Workbook.Worksheets["Quy Trình"];
                ExcelWorksheet SheetNhanSu = package.Workbook.Worksheets["Nhân sự"];
                //
                HRM_QuyTrinhLamThemGio item = new HRM_QuyTrinhLamThemGio();
                item.listUser = new List<HRM_QuyTrinhLamThemGio_NhanVien>();

                CallWebService callWeb = _CallWebService;
                //List<string> listId = new List<string>();
                //listId.Add(item.IdNguoiDeNghi);
                //if (item.listUser != null && item.listUser.Count > 0)
                //{
                //    listId.AddRange(item.listUser.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList());
                //    listId = listId.Distinct().ToList();
                //}

                List<BoPhan> boPhans = callWeb.GetDanhSachBoPhan(db);
                // sheet quy trình
                SheetQuyTrinh.Cells[4, 1].Value = 1;
                string MaCty = dungchung.GetCell(SheetQuyTrinh, 4, 2);
                if (MaCty != null)
                {
                    BoPhan cty = boPhans.FirstOrDefault(a => a.MaBoPhan == MaCty);
                    if (cty != null)
                    {
                        item.IdCongTy = cty.Id;
                    }
                }

                string MaNguoiDeNghi = dungchung.GetCell(SheetQuyTrinh, 4, 3);
                if (!string.IsNullOrEmpty(MaNguoiDeNghi))
                {
                    var user = db.HRM_NhanVien.FirstOrDefault(a => a.MaNhanVien == MaNguoiDeNghi);
                    if (user != null)
                    {
                        item.IdNguoiDeNghi = user.IdUser;
                        item.TenNguoiDeNghi = user.TenDayDu;
                    }
                }
                string MaBP = dungchung.GetCell(SheetQuyTrinh, 4, 5);
                if (MaBP != null)
                {
                    item.IdBoPhan = MaBP;
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

                }
                item.NoiDung = dungchung.GetCell(SheetQuyTrinh, 4, 6);
                item.GhiChu = dungchung.GetCell(SheetQuyTrinh, 4, 7);
                // sheet user đăng ký
                List<HRM_QuyTrinhLamThemGio_ChiTiet> listNgay = new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
                for (int row = 3; row <= SheetNhanSu.Cells.End.Row; row++)
                {
                    HRM_QuyTrinhLamThemGio_ChiTiet nv = new HRM_QuyTrinhLamThemGio_ChiTiet();
                    nv.MaNhanVien = dungchung.GetCell(SheetNhanSu, row, 2);
                    if (string.IsNullOrEmpty(nv.MaNhanVien))
                        break;
                    nv.ThoiGianBatDau = dungchung.GetCellToDateTime(SheetNhanSu, row, 6) ?? dt;
                    nv.ThoiGianBatDauUnix = dungchung.DateTimeToUnixTimeStamp(nv.ThoiGianBatDau);
                    nv.ThoiGianKetThuc = dungchung.GetCellToDateTime(SheetNhanSu, row, 7) ?? dt;
                    nv.ThoiGianKetThucUnix = dungchung.DateTimeToUnixTimeStamp(nv.ThoiGianKetThuc);
                    string TuGiosColText = GetCell(SheetNhanSu, row, 8);
                    if (string.IsNullOrEmpty(TuGiosColText))
                        break;
                    double dGioVaoDau = dungchung.ConvertGioPhutSangGio(TuGiosColText);
                    int gioVao = 0, phutVao = 0;
                    dungchung.ConvertGioPhut(TuGiosColText, ref gioVao, ref phutVao);
                    nv.TuGio = new DateTime(nv.ThoiGianBatDau.Value.Year, nv.ThoiGianBatDau.Value.Month, nv.ThoiGianBatDau.Value.Day, gioVao, phutVao, 0);
                    nv.TuGioUnix = dungchung.DateTimeToUnixTimeStamp(nv.TuGio);
                    string DenGiosColText = GetCell(SheetNhanSu, row, 9);
                    if (string.IsNullOrEmpty(DenGiosColText))
                        break;
                    double dGioVaoDen = dungchung.ConvertGioPhutSangGio(DenGiosColText);
                    int gioDen = 0, phutDen = 0;
                    dungchung.ConvertGioPhut(DenGiosColText, ref gioDen, ref phutDen);
                    nv.DenGio = new DateTime(nv.ThoiGianBatDau.Value.Year, nv.ThoiGianBatDau.Value.Month, nv.ThoiGianBatDau.Value.Day, gioDen, phutDen, 0);
                    nv.DenGioUnix = dungchung.DateTimeToUnixTimeStamp(nv.DenGio);
                    nv.ThoiGian = dungchung.GetCellToDouble(SheetNhanSu, row, 10);

                    listNgay.Add(nv);
                }
                List<string> listMa = listNgay.Select(a => a.MaNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listMa.Contains(a.MaNhanVien)).ToList();
                item.listUser = new List<HRM_QuyTrinhLamThemGio_NhanVien>();
                if (listNhanVien != null && listNhanVien.Count > 0)
                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_QuyTrinhLamThemGio_NhanVien NVAdd = new HRM_QuyTrinhLamThemGio_NhanVien();
                        NVAdd.listNgay = new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
                        NVAdd.MaNhanVien = NhanVien.MaNhanVien;
                        NVAdd.TenNhanVien = NhanVien.TenDayDu;
                        NVAdd.IdUser = NhanVien.IdGuid;
                        NVAdd.IdBoPhan = NhanVien.IdBoPhanEOS;
                        NVAdd.TenBoPhan = boPhans.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS) == null ? null : boPhans.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS).TenBoPhan;
                        NVAdd.listNgay = listNgay.Where(a => a.MaNhanVien == NhanVien.MaNhanVien).ToList();
                        item.listUser.Add(NVAdd);
                    }
                return item;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }

        public dynamic ExportYeuCauLamThemGio(HRM_QuyTrinhLamThemGio QuyTrinh, ApplicationDbContext db)
        {
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_YeuCauLamThemGio.xlsx");
                DateTime dt = DateTime.Now;
                var filenamegui = "QD_YeuCauLamThemGio.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Mẫu"];

                if (QuyTrinh.listUser != null && QuyTrinh.listUser.Count > 0)
                {

                    foreach (var item in QuyTrinh.listUser)
                    {
                        ExcelWorksheet sheet2 = package.Workbook.Worksheets.Add(item.TenNhanVien, sheet1);
                        sheet2.Cells[4, 2].Value = "Họ tên: " + item.TenNhanVien + " Phòng: " + item.TenBoPhan;
                        int STT = 1;
                        int nRow = 7;

                        if (item.listNgay != null && item.listNgay.Count > 0)
                            foreach (var Ngay in item.listNgay)
                            {
                                DateTime time = Ngay.ThoiGianBatDau.Value;

                                while (time <= Ngay.ThoiGianKetThuc.Value)
                                {
                                    int nCol = 1;
                                    sheet2.Cells[nRow, nCol].Value = STT;
                                    nCol++;
                                    sheet2.Cells[nRow, nCol].Value = time.ToString("dd/MM/yyyy");
                                    nCol++;
                                    sheet2.Cells[nRow, nCol].Value = Ngay.GhiChu;
                                    nCol++;
                                    sheet2.Cells[nRow, nCol].Value = Ngay.TuGio?.ToString("dd/MM/yyyy");
                                    nCol++;
                                    sheet2.Cells[nRow, nCol].Value = Ngay.DenGio?.ToString("dd/MM/yyyy");
                                    nCol++;
                                    STT++;
                                    nRow++;
                                    time = time.AddDays(1);
                                }
                            }
                        dungchung.DrawTableExcel(sheet2, 5, nRow - 1, 1, 7);
                        sheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet2.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet2.Cells[7, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    }
                    package.Workbook.Worksheets.Delete(sheet1);
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public dynamic ExportMauLamThemGio()
        {
            try
            {
                string sFile = "";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ImportDangKyLamThemGio.xlsx" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "Dangkylamthemgio.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets[1];
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }


        #endregion

        #region xin nghỉ phép
        public dynamic ExportMauXinNghiPhep(ApplicationDbContext db)
        {
            try
            {
                string sFile = "ImportXinNghiPhep.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets[2];
                CallWebService callWeb = _CallWebService;
                var listDmHt = callWeb.GetListDanhMucHinhThucNghi(db);
                int nRow = 3;
                foreach (var item in listDmHt)
                {


                    sheet1.Cells[nRow, 1].Value = item.Ma;
                    sheet1.Cells[nRow, 2].Value = item.Ten;
                    nRow++;
                }
                nRow = 3;
                var listDanhMucLoaiNghiPhep = callWeb.GetDanhMucLoaiNghiPhep(db);
                foreach (var item in listDanhMucLoaiNghiPhep)
                {


                    sheet1.Cells[nRow, 4].Value = item.Ma;
                    sheet1.Cells[nRow, 5].Value = item.Ten;
                    nRow++;
                }
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        //public dynamic ImportDanhSachNghiPhep(ApplicationDbContext db, string FileName)
        //{
        //    using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            string pathTemplate =PathUploads+ FileName; // HostingEnvironmentCompat.MapPath("~/MauBaoCao/MauDataKH.xlsx");
        //                                                                               //  string pathTemplate = @"C:\uploader\App_Data\uploads\MauImport_TasksTemp_1.xlsx";
        //            Stream s = File.OpenRead(pathTemplate);
        //            ExcelPackage package = new ExcelPackage(s);
        //            s.Close();
        //            var sheet1 = package.Workbook.Worksheets[1];
        //            int row = 3;
        //            int maxrow = sheet1.Dimension.Rows;
        //            List<ImportPhieuXinNghiPhepModel> listModels = new List<ImportPhieuXinNghiPhepModel>();
        //            string LoiReturn = "";

        //            CallWebService callWeb = _CallWebService;
        //            var listDmHt = callWeb.GetListDanhMucHinhThucNghi(db);
        //            var listDanhMucLoaiNghiPhep = callWeb.GetDanhMucLoaiNghiPhep(db);
        //            List<BoPhan> BoPhans =  callWeb.GetDanhSachBoPhan(db);
        //            long LoaiBoPhanPkid = db.LoaiBoPhans.Where(r => r.MaLoaiBoPhan == "CONGTY").Select(r => r.Id).FirstOrDefault();

        //            List<NhanVien> nhanViens = db.NhanViens.ToList();
        //            for (int i = row; i <= maxrow; i++)
        //            {
        //                ImportPhieuXinNghiPhepModel model = new ImportPhieuXinNghiPhepModel();

        //                string STT = sheet1.Cells[row, 01].Value == null ? "" : sheet1.Cells[row, 01].Value.ToString();
        //                model.MaNhanVien = sheet1.Cells[row, 02].Value == null ? "" : sheet1.Cells[row, 02].Value.ToString();
        //                var nhanVien = nhanViens.Where(r => r.MaNhanVien == model.MaNhanVien).FirstOrDefault();
        //                if (nhanVien == null || nhanVien.UserPkid == null || nhanVien.UserPkid == "")
        //                    LoiReturn = LoiReturn + " Lỗi thiếu nhân viên thiếu user để đăng ký " + STT;
        //                else
        //                {
        //                    model.TenNhanVien = nhanVien.TenDayDu;
        //                    model.IdUser = nhanVien.UserPkid;
        //                }
        //                model.TenNhanVien = sheet1.Cells[row, 03].Value == null ? "" : sheet1.Cells[row, 03].Value.ToString();
        //                string MaBoPhan = sheet1.Cells[row, 04].Value == null ? "" : sheet1.Cells[row, 04].Value.ToString();
        //                if (MaBoPhan == "")
        //                    LoiReturn = LoiReturn + " Lỗi thiếu bộ phận " + STT;
        //                model.IdBoPhan = BoPhans.Where(r => r.MaBoPhan == MaBoPhan).Select(r => r.Id).FirstOrDefault();
        //                if (model.IdBoPhan == 0)
        //                    LoiReturn = LoiReturn + " Lỗi  bộ phận không tồn tại " + STT;
        //                model.IdCongTy = GetCongTyByBoPhan(model.IdBoPhan, LoaiBoPhanPkid, BoPhans);
        //                model.NoiDung = sheet1.Cells[row, 05].Value == null ? "" : sheet1.Cells[row, 05].Value.ToString();
        //                model.MaLoaiNghi = sheet1.Cells[row, 06].Value == null ? "" : sheet1.Cells[row, 06].Value.ToString();

        //                model.IdLoaiNghi = listDanhMucLoaiNghiPhep.Where(r => r.Ma == model.MaLoaiNghi).Select(r => r.Id).FirstOrDefault();
        //                if (model.IdLoaiNghi.Length < 5)
        //                {
        //                    LoiReturn = LoiReturn + " Lỗi loại nghỉ dòng " + STT;
        //                }
        //                model.MaHinhThuc = sheet1.Cells[row, 07].Value == null ? "" : sheet1.Cells[row, 07].Value.ToString();
        //                model.IddmHinhThuc = listDmHt.Where(r => r.Ma == model.MaHinhThuc).Select(r => r.Id).FirstOrDefault();
        //                if (model.IddmHinhThuc == 0)
        //                {
        //                    LoiReturn = LoiReturn + " Lỗi hình thức nghỉ dòng " + STT;
        //                }
        //                string ThoiGianBatDau = sheet1.Cells[row, 08].Value == null ? "" : sheet1.Cells[row, 08].Value.ToString();
        //                try
        //                {
        //                    if (!string.IsNullOrEmpty(ThoiGianBatDau))
        //                    {

        //                        model.ThoiGianBatDau = DateTime.ParseExact(ThoiGianBatDau.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

        //                    }
        //                    else
        //                    {
        //                        LoiReturn = LoiReturn + " Lỗi thời gian bắt đầu " + STT;
        //                        continue;
        //                    }
        //                    //newtasks.ThoiGianBatDau = dungchung.ConvertStringToDate(NgayBatDau);
        //                }
        //                catch (Exception)
        //                {
        //                    LoiReturn = LoiReturn + " Lỗi thời gian bắt đầu " + STT;
        //                    continue;
        //                }
        //                string ThoiGianKetThuc = sheet1.Cells[row, 09].Value == null ? "" : sheet1.Cells[row, 09].Value.ToString();
        //                try
        //                {
        //                    if (!string.IsNullOrEmpty(ThoiGianKetThuc))
        //                    {

        //                        //{                                //DateTime date;
        //                        //CultureInfo provider = CultureInfo.InvariantCulture;
        //                        //var checkDate = DateTime.TryParseExact(ThoiGianKetThuc, new string[] { "dd/MM/yyyy hh:mm" }, provider, DateTimeStyles.None, out date);
        //                        //if (checkDate == true)
        //                        model.ThoiGianKetThuc = DateTime.ParseExact(ThoiGianKetThuc.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //                        // }
        //                    }
        //                    else
        //                    {
        //                        LoiReturn = LoiReturn + " Lỗi thời gian kết thúc " + STT;
        //                        continue;
        //                    }
        //                    //newtasks.ThoiGianBatDau = dungchung.ConvertStringToDate(NgayBatDau);
        //                }
        //                catch (Exception)
        //                {
        //                    LoiReturn = LoiReturn + " Lỗi thời gian kết thúc " + STT;
        //                    continue;
        //                }
        //                string ThoiGianNghi = sheet1.Cells[row, 10].Value == null ? "" : sheet1.Cells[row, 10].Value.ToString();
        //                try
        //                {
        //                    model.ThoiGianNghi = double.Parse(ThoiGianNghi);
        //                }
        //                catch (Exception)
        //                {

        //                    model.ThoiGianNghi = 0;
        //                }
        //                listModels.Add(model);
        //            }
        //            FuncQuanTriQuyTrinh func = new FuncQuanTriQuyTrinh();
        //            HRM_ThietLapQuyTrinh_TrangThai trangThai = func.GetObijectTrangThaiCuoi(db, DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString());

        //            foreach (var item in listModels)
        //            {


        //                HRM_QuyTrinhXinNghiPhep quytrinh = new HRM_QuyTrinhXinNghiPhep
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    IdBoPhan = item.IdBoPhan,
        //                    IdNguoiDeNghi = item.IdUser,
        //                    ThoiGianBatDau = item.ThoiGianBatDau,
        //                    ThoiGianKetThuc = item.ThoiGianKetThuc,
        //                    ThoiGianTong = item.ThoiGianNghi,
        //                    MucDich = item.NoiDung,
        //                    IdTrangThai = trangThai.Id,
        //                    TenTrangThai = trangThai.Ten,
        //                    IdCongTy = item.IdCongTy,
        //                    TenNguoiDeNghi = item.TenNhanVien,
        //                    ThangNam = dungchung.UnixTimeStampToDateTime(0),
        //                    SoPhieu = GetNextSoQuyTrinhXinNghiPhep(db).SoPhieu.ToString(),
        //                };
        //                HRM_XinNghiPhep_ChiTietNgayNghi model = new HRM_XinNghiPhep_ChiTietNgayNghi
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    IdHinhThucNghi = item.IddmHinhThuc,
        //                    IdQuyTrinh = quytrinh.Id,
        //                    NgayBatDau = item.ThoiGianBatDau,
        //                    NgayKetThuc = item.ThoiGianKetThuc,
        //                    GhiChu = item.NoiDung,
        //                    ThoiGianNghi = item.ThoiGianNghi,
        //                    IdLoaiNghiPhep = item.IdLoaiNghi,


        //                };
        //                db.HRM_QuyTrinhXinNghiPhep.Add(quytrinh);
        //                db.HRM_XinNghiPhep_ChiTietNgayNghi.Add(model);
        //                db.SaveChanges();
        //            }
        //            dbContextTransaction.Commit();
        //            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thành công ");
        //        }
        //        catch (Exception ex)
        //        {
        //            dbContextTransaction.Rollback();
        //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
        //        }
        //    }
        //}

        //public dynamic AddNhanhXinNghiPhep(List<ImportPhieuXinNghiPhepModel> listModels, ApplicationDbContext db)
        //{
        //    FuncQuanTriQuyTrinh func = new FuncQuanTriQuyTrinh();
        //    HRM_ThietLapQuyTrinh_TrangThai trangThai = func.GetObijectTrangThaiCuoi(db, DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString());
        //    using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            List<BoPhan> BoPhans = db.BoPhans.ToList();
        //            long LoaiBoPhanPkid = db.LoaiBoPhans.Where(r => r.MaLoaiBoPhan == "CONGTY").Select(r => r.Id).FirstOrDefault();
        //            foreach (var item in listModels)
        //            {
        //                item.ThoiGianBatDau = dungchung.UnixTimeStampToDateTime(item.ThoiGianBatDauUnix);
        //                item.ThoiGianKetThuc = dungchung.UnixTimeStampToDateTime(item.ThoiGianKetThucUnix);

        //                HRM_QuyTrinhXinNghiPhep quytrinh = new HRM_QuyTrinhXinNghiPhep
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    IdBoPhan = item.IdBoPhan,
        //                    IdNguoiDeNghi = item.IdUser,
        //                    ThoiGianBatDau = item.ThoiGianBatDau,
        //                    ThoiGianKetThuc = item.ThoiGianKetThuc,
        //                    ThoiGianTong = item.ThoiGianNghi,
        //                    MucDich = item.NoiDung,
        //                    IdTrangThai = trangThai.Id,
        //                    TenTrangThai = trangThai.Ten,

        //                    TenNguoiDeNghi = item.TenNhanVien,
        //                    IdCongTy = GetCongTyByBoPhan(item.IdBoPhan, LoaiBoPhanPkid, BoPhans),
        //                    ThangNam = dungchung.UnixTimeStampToDateTime(0),
        //                    SoPhieu = GetNextSoQuyTrinhXinNghiPhep(db).SoPhieu.ToString(),
        //                };
        //                HRM_XinNghiPhep_ChiTietNgayNghi model = new HRM_XinNghiPhep_ChiTietNgayNghi
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    IdHinhThucNghi = item.IddmHinhThuc,
        //                    IdQuyTrinh = quytrinh.Id,
        //                    NgayBatDau = item.ThoiGianBatDau,
        //                    NgayKetThuc = item.ThoiGianKetThuc,
        //                    GhiChu = item.NoiDung,
        //                    ThoiGianNghi = item.ThoiGianNghi,
        //                    IdLoaiNghiPhep = item.IdLoaiNghi,


        //                };
        //                db.HRM_QuyTrinhXinNghiPhep.Add(quytrinh);
        //                db.HRM_XinNghiPhep_ChiTietNgayNghi.Add(model);
        //                db.SaveChanges();
        //            }
        //            dbContextTransaction.Commit();

        //            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thành công ");
        //        }
        //        catch (Exception ex)
        //        {
        //            dbContextTransaction.Rollback();
        //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
        //        }
        //    }
        //}
        //public dynamic GetDanhSachDXinNghiByUser(ApplicationDbContext db, double ThangNamUnix)
        //{
        //    CallWebService callWeb = _CallWebService;
        //    DateTime ThangNam = UnixTimeStampToDateTime(ThangNamUnix);
        //    string TrangThaiCuoi = callWeb.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString());
        //    List<HRM_QuyTrinhXinNghiPhep> list = new List<HRM_QuyTrinhXinNghiPhep>();
        //    list = db.HRM_QuyTrinhXinNghiPhep.Where(r => r.IdTrangThai == TrangThaiCuoi && r.IdNguoiDeNghi == db.IdUser && r.ThoiGianBatDau.Year == ThangNam.Year && r.ThoiGianBatDau.Month == ThangNam.Month).ToList();

        //    var listId = list.Select(r => r.Id).ToList();
        //    var listChiTiet = db.HRM_QuyTrinhXinNghiPhep_ChiTiet.Where(r => listId.Contains(r.IdQuyTrinh)).ToList();
        //    foreach (var item in list)
        //    {
        //        var lstChiTiet = listChiTiet.Where(r => r.IdQuyTrinh == item.Id).ToList();
        //        foreach (var item1 in lstChiTiet)
        //        {
        //            item1.NgayBatDauUnix = DateTimeToUnixTimeStamp(item1.NgayBatDau);
        //            item1.NgayKetThucUnix = DateTimeToUnixTimeStamp(item1.NgayKetThuc);


        //        }
        //        item.listChiTietNgayNghi = listChiTiet.Where(r => r.IdQuyTrinh == item.Id).ToList();

        //    }
        //    return list;
        //}
        public dynamic GetQuyTrinhXinNghiPhep(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_QuyTrinhXinNghiPhep QuyTrinh = db.HRM_QuyTrinhXinNghiPhep.FirstOrDefault(x => x.IdGuid == Id);

                if (QuyTrinh.IdBoPhan != null)
                {
                    QuyTrinh.listIdBoPhan = new List<long>();
                    if (QuyTrinh.IdBoPhan.Contains(","))
                    {
                        string[] list = QuyTrinh.IdBoPhan.Split(',');
                        foreach (var bp in list)
                            QuyTrinh.listIdBoPhan.Add(long.Parse(bp));
                    }
                    else QuyTrinh.listIdBoPhan.Add(long.Parse(QuyTrinh.IdBoPhan));
                }

                QuyTrinh.MucDich = QuyTrinh.NoiDung;
                var nhanSu = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == QuyTrinh.IdNguoiDeNghi);
                if (nhanSu != null)
                    QuyTrinh.TenNguoiDeNghi = nhanSu.TenDayDu;
                var lstChiTiet = db.HRM_QuyTrinhXinNghiPhep_ChiTiet.Where(r => Id == (r.IdQuyTrinh)).ToList();
                List<string> listIdUser = lstChiTiet.Select(a => a.IdNhanVien).Distinct().ToList();
                var listnhanVien = db.HRM_NhanVien.Where(a => listIdUser.Contains(a.IdGuid)).ToList();
                CallWebService _callweb = _CallWebService;
                var listBoPhan = _callweb.GetDanhSachBoPhan(db);
                var listChucDanh = _callweb.GetDanhSachChucDanh(db);
                var listChucVu = _callweb.GetDanhSachChucVu(db);
                foreach (var item in lstChiTiet)
                {
                    item.NgaySinhUnix = DateTimeToUnixTimeStamp(item.NgaySinh);
                    item.NgayBatDauUnix = DateTimeToUnixTimeStamp(item.NgayBatDau);
                    item.NgayKetThucUnix = DateTimeToUnixTimeStamp(item.NgayKetThuc);
                    var NhanVien = listnhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    if (NhanVien != null)
                    {
                        item.MaNhanVien = NhanVien.MaNhanVien;
                        item.TenNhanVien = NhanVien.TenDayDu;
                        item.TenBoPhan = listBoPhan.Where(a => a.Id == NhanVien.IdBoPhanEOS).Select(a=>a.TenBoPhan).FirstOrDefault();
                        item.TenChucDanh = listChucDanh.Where(a => a.Id == NhanVien.IdChucDanh).Select(a=>a.Ten).FirstOrDefault();
                        item.TenChucVu = listChucVu.Where(a => a.Id == NhanVien.IdChucVu).Select(a=>a.TenChucVu).FirstOrDefault();

                    }

                }
                QuyTrinh.listChiTietNgayNghi = lstChiTiet;
                if (!GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))
                    return err;
                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetListQuyTrinhXinNghiPhep(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_QuyTrinhXinNghiPhep> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_QuyTrinhXinNghiPhep
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhXinNghiPhep
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => (!string.IsNullOrEmpty(a.SoQuyTrinh) && a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.GhiChu) && a.GhiChu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Created >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Created < dtDenNgay.AddDays(1));
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == nameof(HRM_QuyTrinhXinNghiPhep) && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                if (!string.IsNullOrEmpty(TimKiem.IdUser))
                    result = result.Where(a => !string.IsNullOrEmpty(a.IdNguoiDeNghi) && a.IdNguoiDeNghi == TimKiem.IdUser).ToList();
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<string> listIdQuyTrinh = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listItem = db.HRM_QuyTrinhXinNghiPhep_ChiTiet.Where(a => listIdQuyTrinh.Contains(a.IdQuyTrinh)).ToList();
                CallWebService webService = _CallWebService;
                List<PSM_DuAn> DuAns = webService.GetDataDuAn(db);
                List<BoPhan> BoPhans = webService.GetDanhSachBoPhan(db);
                List<string> listIdNV = itemList.Select(a => a.IdNguoiDeNghi).Distinct().ToList();
                if (listItem != null && listItem.Count > 0)
                    listIdNV.AddRange(listItem.Select(a => a.IdNhanVien).Distinct().ToList());
                var listNV = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdUser) || listIdNV.Contains(a.IdGuid)).Distinct().ToList();
                var listIdUserCreatedBy = listItem.GroupBy(q => q.CreatedBy).Select(q => q.Key).ToList();
                var listUser = webService.GetDanhSachUserTheoListIdUser(db, listIdUserCreatedBy);
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
                foreach (HRM_QuyTrinhXinNghiPhep item in itemList)
                {
                    item.TenNguoiDeNghi = item.CreatedByName;
                    item.ItemUserCreatedBy = listUser.FirstOrDefault(q => q.Id == item.CreatedBy);
                    if (listNV != null && listNV.Count > 0 && !string.IsNullOrEmpty(item.IdNguoiDeNghi))
                    {
                        var NV = listNV.FirstOrDefault(a => a.IdUser == item.IdNguoiDeNghi);
                        if (NV != null)
                            item.TenNguoiDeNghi = NV.TenDayDu;
                    }
                    item.MucDich = item.NoiDung;
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
                        List<BoPhan> BoPhan = BoPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                        if (BoPhan != null)
                            item.TenBoPhan = string.Join(",", BoPhan.Select(a => a.TenBoPhan));
                    }
                    if (listItem != null && listItem.Count > 0)
                    {
                        var Items = listItem.Where(a => a.IdQuyTrinh == item.IdGuid).ToList();
                        List<string> list = new List<string>();
                        foreach (var itemchild in Items)
                        {
                            string hinhthucNghi = listHinhThucNghi.Where(a => a.Id == itemchild.IdHinhThucNghi).Select(a => a.Ten).FirstOrDefault();
                            if (listNV != null && listNV.Count > 0)
                            {
                                var nhanVien = listNV.FirstOrDefault(a => a.IdGuid == itemchild.IdNhanVien);
                                if (nhanVien != null)
                                    list.Add(nhanVien.TenDayDu + "( " + hinhthucNghi + " " + itemchild.ThoiGianNghi + " ngày) :" + itemchild.NgayBatDau.ToString("dd / MM / yyyy") + " - " + itemchild.NgayKetThuc.ToString("dd / MM / yyyy"));
                            }
                        }
                        item.NoiDungNghi = string.Join(", ", list);
                    }
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
        public dynamic CapNhatTrangThaiHuyQuyTrinhXinNghiPhep(string Id, ApplicationDbContext db)
        {
            HRM_QuyTrinhXinNghiPhep item = db.HRM_QuyTrinhXinNghiPhep.Find(Id);
            try
            {
                CallWebService callWeb = _CallWebService;
                List<string> list = new List<string>();
                list.Add(Id);
                dynamic TrangThaiThayDoidnm = callWeb.SetHuyQuyTrinh(db, list);

                if (TrangThaiThayDoidnm.IdTrangThai != null || TrangThaiThayDoidnm.IdTrangThai == "")
                {

                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trạng thái hủy không tồn tại");
                }
                item.IdTrangThai = TrangThaiThayDoidnm.IdTrangThai;
                item.TenTrangThai = TrangThaiThayDoidnm.TenTrangThai;
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetNextSoQuyTrinhXinNghiPhep(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "QTXNP",
                db.HRM_QuyTrinhXinNghiPhep.OrderByDescending(a => a.Created).Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic SetQuyTrinhXinNghiPhep(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhXinNghiPhep QuyTrinh = JsonConvert.DeserializeObject<HRM_QuyTrinhXinNghiPhep>(data.ToString());
                    var QuyTrinhdb = db.HRM_QuyTrinhXinNghiPhep.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, QuyTrinh, err))
                        return err;

                    MessageReturn check = UpdateQuyTrinhXinNghiPhep(db, QuyTrinh, err, eQuyTrinh_LoaiHam.Set);
                    if (check.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check.sError);
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
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
                }
            }
        }
        public dynamic UpdateQuyTrinhXinNghiPhep(ApplicationDbContext db, HRM_QuyTrinhXinNghiPhep QuyTrinh, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                CallWebService callWeb = _CallWebService;
                QuyTrinh.NoiDung = QuyTrinh.MucDich;
                if (QuyTrinh.listIdBoPhan != null && QuyTrinh.listIdBoPhan.Count > 0)
                    QuyTrinh.IdBoPhan = string.Join(",", QuyTrinh.listIdBoPhan);
                if (!string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    //dungchung.AddModifyDelete(db, DefineData.eTable.HRM_QuyTrinhXinNghiPhep, DefineData.eModify.Add, DefineData.eModify.Add, JsonConvert.SerializeObject(QuyTrinh), "");
                }

                else
                {
                    int Check = db.HRM_QuyTrinhXinNghiPhep.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh).Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhXinNghiPhep(db).SoPhieu.ToString();
                        QuyTrinh.SoQuyTrinh = MaPhieu;
                        // return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra số phiếu quy trình đã tồn tại vui lòng kiểm tra lại");
                    }
                    QuyTrinh.IdDuAn = QuyTrinh.IdCongTy;
                    QuyTrinh.eAction = DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return err;
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    //dungchung.AddModifyDelete(db, DefineData.eTableName.HRM_QuyTrinhXinNghiPhep, DefineData.eModify.Add, DefineData.eModify.Add, JsonConvert.SerializeObject(QuyTrinh), "");

                }
                var listChiTiet = db.HRM_QuyTrinhXinNghiPhep_ChiTiet.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                if (listChiTiet.Count > 0)
                    db.HRM_QuyTrinhXinNghiPhep_ChiTiet.RemoveRange(listChiTiet);
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                if (QuyTrinh.listIdBoPhan != null && QuyTrinh.listIdBoPhan.Count > 0)
                {
                    foreach (var item in QuyTrinh.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = QuyTrinh.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_QuyTrinhXinNghiPhep);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                List<DanhMuc> listDanhMucNP = callWeb.GetDanhMucLoaiNghiPhep(db);
                if (QuyTrinh.listChiTietNgayNghi != null)
                {

                    foreach (var item in QuyTrinh.listChiTietNgayNghi)
                    {

                        item.Id = Guid.NewGuid().ToString();
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix).Date;
                        if (item.NgaySinhUnix != null && item.NgaySinhUnix > 0)
                            item.NgaySinh = dungchung.UnixTimeStampToDateTime(item.NgaySinhUnix);
                        else item.NgaySinh = null;
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix).Date;
                        item.IdNguoiDeNghi = QuyTrinh.IdNguoiDeNghi;
                        int SoNgay = (int)(item.NgayKetThuc - item.NgayBatDau).TotalDays + 1;
                        item.ThoiGianNghi = 1 * SoNgay;
                        if (listDanhMucNP != null && listDanhMucNP.Count > 0)
                        {
                            var dm = listDanhMucNP.FirstOrDefault(a => a.Ma == "CANGAY");
                            if (dm != null && item.IdLoaiNghiPhep != dm.Id)
                                item.ThoiGianNghi = 0.5 * SoNgay;
                        }
                        item.IdQuyTrinh = QuyTrinh.IdGuid;
                        db.HRM_QuyTrinhXinNghiPhep_ChiTiet.Add(item);
                    }

                }
                if (QuyTrinh.isKetThuc == true && QuyTrinh.listChiTietNgayNghi != null && QuyTrinh.listChiTietNgayNghi.Count > 0)
                {
                    var listdbOld = (from a in QuyTrinh.listChiTietNgayNghi
                                     join b in db.HRM_TheoDoiXinNghiPhep on a.IdNhanVien equals b.IdNhanVien
                                     where a.NgayBatDau <= b.NgayNghi && a.NgayKetThuc >= b.NgayNghi && a.IdLoaiNghiPhep == b.IdLoaiNghiPhep && a.IdHinhThucNghi == b.IdHinhThucNghi
                                     select b
                                   ).ToList();
                    if (listdbOld != null && listdbOld.Count > 0)
                        db.HRM_TheoDoiXinNghiPhep.RemoveRange(listdbOld);
                    List<HRM_TheoDoiXinNghiPhep> listAdd = new List<HRM_TheoDoiXinNghiPhep>();
                    foreach (var item in QuyTrinh.listChiTietNgayNghi)
                    {
                        DateTime time = new DateTime(item.NgayBatDau.Year, item.NgayBatDau.Month, item.NgayBatDau.Day, 0, 0, 0);

                        while (time <= item.NgayKetThuc)
                        {
                            HRM_TheoDoiXinNghiPhep itemAdd = new HRM_TheoDoiXinNghiPhep(item);
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            itemAdd.IdBoPhan = item.IdBoPhan;
                            itemAdd.IdCongTy = QuyTrinh.IdCongTy;
                            itemAdd.IdNhanVien = item.IdNhanVien;
                            itemAdd.NgayNghi = time;
                            itemAdd.NgaySinh = item.NgaySinh;
                            itemAdd.NgayBatDau = item.NgayBatDau;
                            itemAdd.ThoiGianNghi = 1;
                            itemAdd.GhiChu = item.GhiChu;
                            if (listDanhMucNP != null && listDanhMucNP.Count > 0)
                            {
                                var dm = listDanhMucNP.FirstOrDefault(a => a.Ma == "CANGAY");
                                if (dm != null && item.IdLoaiNghiPhep != dm.Id)
                                    itemAdd.ThoiGianNghi = 0.5;
                            }
                            listAdd.Add(itemAdd);
                            time = time.AddDays(1);
                        }
                    }
                    if (listAdd != null && listAdd.Count > 0)
                        db.HRM_TheoDoiXinNghiPhep.AddRange(listAdd);
                }
                if (!SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công"); ;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
            }

        }
        public dynamic DeleteQuyTrinhXinNghiPhep(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhXinNghiPhep QuyTrinh = JsonConvert.DeserializeObject<HRM_QuyTrinhXinNghiPhep>(data.ToString());
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var listChiTiet = db.HRM_QuyTrinhXinNghiPhep_ChiTiet.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                    if (listChiTiet.Count > 0)
                        db.HRM_QuyTrinhXinNghiPhep_ChiTiet.RemoveRange(listChiTiet);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", QuyTrinh);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhXinNghiPhep(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhXinNghiPhep QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_QuyTrinhXinNghiPhep)) as HRM_QuyTrinhXinNghiPhep;

                    HRM_QuyTrinhXinNghiPhep pcuDatabase = db.HRM_QuyTrinhXinNghiPhep.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    MessageReturn Error = UpdateQuyTrinhXinNghiPhep(db, QuyTrinh, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.sError, QuyTrinh.IdGuid);
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
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
        public dynamic KhongDuyetQuyTrinhXinNghiPhep(ApplicationDbContext db, dynamic data)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhXinNghiPhep QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_QuyTrinhXinNghiPhep)) as HRM_QuyTrinhXinNghiPhep;

                    HRM_QuyTrinhXinNghiPhep pcuDatabase = db.HRM_QuyTrinhXinNghiPhep.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    MessageReturn Error = UpdateQuyTrinhXinNghiPhep(db, QuyTrinh, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.Value, QuyTrinh.IdGuid);
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
        public dynamic TongHopBaoCaoNgayPhep(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, int Nam, string KeyWord)
        {
            var listData = db.HRM_TheoDoiXinNghiPhep.Where(a => a.NgayNghi.Year == Nam).ToList();
            if (IdCongTy > 0)
                listData = listData.Where(a => a.IdCongTy == IdCongTy).ToList();
            if (listIdBoPhan.Count > 0)
                listData = listData.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
            List<TongHopNgayPhep> listResult = new List<TongHopNgayPhep>();
            CallWebService _callWebService = _CallWebService;
            if (listData != null && listData.Count > 0)
            {
                var dmHinhThucNghis = _callWebService.GetListDanhMucHinhThucNghi(db);
                var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                if (Nghibu != null)
                    listData = listData.Where(a => a.IdHinhThucNghi != Nghibu.Id).ToList();
                List<string> listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                if (!string.IsNullOrEmpty(KeyWord))
                {
                    listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(KeyWord.Trim().ToLower()) || a.TenDayDu.ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
                }
                var listBoPhan = _callWebService.GetDanhSachBoPhan(db);
                List<string> list = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                var listHopDong = db.HRM_HopDong.Where(a => list.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                var listCa = db.HRM_dmLichLamViec.ToList();
                foreach (var item in listNhanVien)
                {
                    TongHopNgayPhep itemAdd = new TongHopNgayPhep();
                    itemAdd.MaNhanVien = item.MaNhanVien;
                    itemAdd.TenNhanVien = item.TenDayDu;
                    var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                    {
                        var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);

                        if (CaLamViec != null)
                        {
                            if (CaLamViec.isCaGay == true)
                                itemAdd.LoaiCong = "P";
                            else
                                itemAdd.LoaiCong = "F";

                        }
                        itemAdd.PhepThamNien = HopDong.SoNgayNghiPhepThamNien;
                    }
                    var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                    if (CongTy != null)
                        itemAdd.TenCongTy = CongTy.TenBoPhan;
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                    if (BoPhan != null)
                        itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                    itemAdd.NgayBatDau = item.NgayBatDauLamViec;
                    if (itemAdd.NgayBatDau != null)
                    {
                        var time = DateTime.Now - itemAdd.NgayBatDau.Value;
                        itemAdd.ThamNien = time.TotalDays / 30.5;
                    }
                    itemAdd.Phep = 12;
                    itemAdd.ConLai = 12 - listData.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                    if (itemAdd.ConLai < 0)
                        itemAdd.ConLai = 0;
                    itemAdd.DaSuDung = listData.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                    itemAdd.listTiet = new List<TongHopNgayPhep_ChiTiet>();
                    for (int i = 1; i <= 12; i++)
                    {
                        TongHopNgayPhep_ChiTiet Thang = new TongHopNgayPhep_ChiTiet();
                        Thang.Thang = i;
                        Thang.Phep = 1;
                        Thang.SuDung = listData.Where(a => a.IdNhanVien == item.IdGuid && a.NgayNghi.Month == i).Sum(a => a.ThoiGianNghi);
                        itemAdd.listTiet.Add(Thang);
                    }
                    listResult.Add(itemAdd);
                }


            }
            return listResult;
        }
        public dynamic ExportTongHopBaoCaoNgayPhep(ApplicationDbContext db, dynamic data)
        {
            List<TongHopNgayPhep> listResult = JsonConvert.DeserializeObject<List<TongHopNgayPhep>>(data.ToString());

            string sFile = "TongHopNgayPhep.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = sFile;
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            int nRow = 4;
            int STT = 1;
            int nCol = 1;
            foreach (var item in listResult)
            {
                nCol = 1;
                sheet1.Cells[nRow, nCol].Value = STT;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.LoaiCong;
                nCol++;
                if (item.NgayBatDau != null)
                    sheet1.Cells[nRow, nCol].Value = item.NgayBatDau.Value.ToString("dd/MM/yyyy");
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.ThamNien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.ConLai;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.PhepThamNien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.Phep;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.DaSuDung;
                nCol++;
                foreach (var Thang in item.listTiet.OrderBy(a => a.Thang))
                {
                    sheet1.Cells[nRow, nCol].Value = Thang.Phep;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = Thang.SuDung;
                    nCol++;

                }
                STT++;
                nRow++;
                nCol--;
            }
            if (nRow >= 5)
            {
                sheet1.Cells[nRow, 1, nRow, 7].Merge = true;
                sheet1.Cells[nRow, 1, nRow, 7].Style.Font.Bold = true;
                sheet1.Cells[nRow, 1].Value = "TỔNG";
                for (int i = 8; i <= nCol; i++)
                {
                    string NameBD = sheet1.Cells[4, i].Address;
                    string NameKT = sheet1.Cells[nRow - 1, i].Address;
                    sheet1.Cells[nRow, i].FormulaR1C1 = "SUM(" + NameBD + ":" + NameKT + ")";
                }
            }
            sheet1.Cells[4, 8, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            if (nRow > 4)
            {
                dungchung.DrawTableExcel(sheet1, 4, nRow, 1, nCol);
                sheet1.Cells[4, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic TongHopBaoCaoNghiBu(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, int Nam, string KeyWord)
        {
            var listData = db.HRM_TheoDoiXinNghiPhep.Where(a => a.NgayNghi.Year == Nam).ToList();
            if (IdCongTy > 0)
                listData = listData.Where(a => a.IdCongTy == IdCongTy).ToList();
            if (listIdBoPhan.Count > 0)
                listData = listData.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
            List<TongHopNgayPhep> listResult = new List<TongHopNgayPhep>();
            CallWebService _callWebService = _CallWebService;
            var dmHinhThucNghis = _callWebService.GetListDanhMucHinhThucNghi(db);
            var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
            if (Nghibu != null)
            {
                listData = listData.Where(a => a.IdHinhThucNghi == Nghibu.Id).ToList();
                if (listData != null && listData.Count > 0)
                {

                    List<string> listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    if (!string.IsNullOrEmpty(KeyWord))
                    {
                        listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(KeyWord.Trim().ToLower()) || a.TenDayDu.ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
                    }
                    var listBoPhan = _callWebService.GetDanhSachBoPhan(db);
                    List<string> list = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listHopDong = db.HRM_HopDong.Where(a => list.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                    List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();

                    foreach (var item in listNhanVien)
                    {
                        TongHopNgayPhep itemAdd = new TongHopNgayPhep();
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                        {
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);

                            if (CaLamViec != null)
                            {
                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCong = "P";
                                else
                                    itemAdd.LoaiCong = "F";
                            }
                        }
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                            itemAdd.TenCongTy = CongTy.TenBoPhan;
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.NgayBatDau = item.NgayBatDauLamViec;
                        if (itemAdd.NgayBatDau != null)
                        {
                            var time = DateTime.Now - itemAdd.NgayBatDau.Value;
                            itemAdd.ThamNien = time.TotalDays / 30.5;
                        }
                        itemAdd.Phep = 0;
                        itemAdd.DaSuDung = listData.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                        itemAdd.listTiet = new List<TongHopNgayPhep_ChiTiet>();
                        for (int i = 1; i <= 12; i++)
                        {
                            TongHopNgayPhep_ChiTiet Thang = new TongHopNgayPhep_ChiTiet();
                            Thang.Thang = i;

                            var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                            var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe &&
                                                                      (a.TuNgay.Month == i || a.DenNgay.Month == i)).ToList();
                            if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                            {
                                itemAdd.Phep += NghiLeChiTiets.Sum(a => a.NgayCongHuong);
                                Thang.Phep = NghiLeChiTiets.Sum(a => a.NgayCongHuong);

                            }

                            Thang.SuDung = listData.Where(a => a.IdNhanVien == item.IdGuid && a.NgayNghi.Month == i).Sum(a => a.ThoiGianNghi);
                            itemAdd.listTiet.Add(Thang);
                        }
                        if (itemAdd.Phep == 0)
                            itemAdd.Phep = null;
                        else
                            itemAdd.ConLai = itemAdd.Phep - listData.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                        if (itemAdd.ConLai < 0)
                            itemAdd.ConLai = 0;
                        listResult.Add(itemAdd);
                    }


                }
            }
            return listResult;
        }
        public dynamic ExportTongHopBaoCaoNghiBu(ApplicationDbContext db, dynamic data)
        {
            List<TongHopNgayPhep> listResult = JsonConvert.DeserializeObject<List<TongHopNgayPhep>>(data.ToString());

            string sFile = "TongHopNghiBu.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = sFile;
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            int nRow = 4;
            int STT = 1;
            int nCol = 1;
            foreach (var item in listResult)
            {
                nCol = 1;
                sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.LoaiCong;
                nCol++;
                if (item.NgayBatDau != null)
                    sheet1.Cells[nRow, nCol].Value = item.NgayBatDau.Value.ToString("dd/MM/yyyy");
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.ThamNien;
                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.ConLai;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.Phep;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.DaSuDung;
                nCol++;
                foreach (var Thang in item.listTiet.OrderBy(a => a.Thang))
                {
                    sheet1.Cells[nRow, nCol].Value = Thang.Phep;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = Thang.SuDung;
                    nCol++;

                }
                STT++;
                nRow++;
                nCol--;
            }
            if (nRow >= 5)
            {
                sheet1.Cells[nRow, 1, nRow, 7].Merge = true;
                sheet1.Cells[nRow, 1, nRow, 7].Style.Font.Bold = true;
                sheet1.Cells[nRow, 1].Value = "TỔNG";
                for (int i = 8; i <= nCol; i++)
                {
                    string NameBD = sheet1.Cells[4, i].Address;
                    string NameKT = sheet1.Cells[nRow - 1, i].Address;
                    sheet1.Cells[nRow, i].FormulaR1C1 = "SUM(" + NameBD + ":" + NameKT + ")";
                }
            }
            sheet1.Cells[4, 8, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            if (nRow > 4)
            {
                dungchung.DrawTableExcel(sheet1, 4, nRow, 1, nCol);
                sheet1.Cells[4, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        public dynamic ExportDonXinNghiNhanVien(HRM_QuyTrinhXinNghiPhep quytrinh, ApplicationDbContext db)
        {
            try
            {
                var filenamegui = DateTime.Now.ToOADate();



                dynamic data = new DynamicObject();
                var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listCongty = _CallWebService.GetDanhSachCongTy(db);
                var CongTy = listCongty.Find(x => x.Id == quytrinh.IdCongTy);
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);


                List<string> listfile = new List<string>();
                if (quytrinh.listChiTietNgayNghi != null && quytrinh.listChiTietNgayNghi.Count > 0)
                {
                    List<string> listIdNV = quytrinh.listChiTietNgayNghi.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => (a.Loai == 1 || a.Loai == 2) && a.isApDung == true && listIdNV.Contains(a.IdNhanVien)).ToList();
                    var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.isApDung == true && listIdNV.Contains(a.IdNhanVien)).ToList();
                    var listNoiCap = db.HRM_dmNoiCap.ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).ToList();
                    foreach (var item in quytrinh.listChiTietNgayNghi)
                    {
                        var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        var GiayTo = listGiayTo.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                        if (GiayTo == null)
                            GiayTo = new HRM_NhanVien_GiayToTuyThan();
                        var SDT = listSDT.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                        if (SDT == null)
                            SDT = new HRM_NhanVien_SoDienThoai();
                        List<string> list = new List<string>();
                        list.Add(DefineData.eKyHieuChamCong.NP.ToString());
                        list.Add(DefineData.eKyHieuChamCong.PN.ToString());
                        list.Add(DefineData.eKyHieuChamCong.NL.ToString());
                        list.Add(DefineData.eKyHieuChamCong.NTL.ToString());
                        list.Add(DefineData.eKyHieuChamCong.NC.ToString());
                        var BoPhan = BoPhans.FirstOrDefault(a => a.Id == item.IdBoPhan);
                        var HinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                        if (BoPhan.MaBoPhan != "CN")
                        {
                            if (list.Contains(HinhThucNghi.Ma))
                            {
                                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_XinNghiPhepGSNV.docx");
                                string sFileNameCopy = PathUploads + "DonXinNghiPhep_" + item.TenNhanVien + ".doc";
                                File.Copy(fileName, sFileNameCopy, true);
                                var doc = DocX.Load(sFileNameCopy);
                                doc.ReplaceText("@HoVaTen", item.TenNhanVien); doc.ReplaceText("@TenBoPhan", item.TenBoPhan);
                                doc.ReplaceText("@NgaySinh  ", NhanVien.NgaySinh.ToString("dd/MM/yyyy"));
                                doc.ReplaceText("@SDT", SDT.SoDienThoai ?? "");
                                doc.ReplaceText("@CCCD", GiayTo.So ?? "");
                                doc.ReplaceText("@SDT", SDT.SoDienThoai ?? "");
                                string NgayThang = "Từ ngày " + item.NgayBatDau.Day + " tháng " + item.NgayBatDau.Month + " năm " + item.NgayBatDau.Year + " Đến ngày " + item.NgayKetThuc.Day + " tháng " + item.NgayKetThuc.Month + " năm " + item.NgayKetThuc.Year;
                                doc.ReplaceText("@TuNgay", NgayThang);
                                doc.ReplaceText("@LyDo", item.GhiChu ?? "");
                                doc.ReplaceText("@LoaiNghi", HinhThucNghi.Ten);
                                doc.Save();
                                //doc.ReplaceText("@NgayQuyetDinh", quytrinh.NgayHieuLuc?.ToString("dd/MM/yyyy"));
                                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_XinNghiPhepGSNV.docx")
                                                + "&path=" + Base64Encode(sFileNameCopy);
                                doc.Save();
                                listfile.Add(sFileNameCopy);
                            }
                            else
                            {
                                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_XinNghiPhepCN.docx");
                                string sFileNameCopy = PathUploads + "DonXinNghiPhep_" + item.TenNhanVien + ".doc";
                                File.Copy(fileName, sFileNameCopy, true);
                                var doc = DocX.Load(sFileNameCopy);
                                doc.ReplaceText("@HoVaTen", item.TenNhanVien);
                                doc.ReplaceText("@TenBoPhan", item.TenBoPhan + " - " + CongTy.TenBoPhan);
                                doc.ReplaceText("@SoNgay", item.ThoiGianNghi.ToString());
                                string NgayThang = "Từ ngày " + item.NgayBatDau.Day + " tháng " + item.NgayBatDau.Month + " năm " + item.NgayBatDau.Year + " Đến ngày " + item.NgayKetThuc.Day + " tháng " + item.NgayKetThuc.Month + " năm " + item.NgayKetThuc.Year;
                                doc.ReplaceText("@TuNgay", NgayThang);
                                doc.ReplaceText("@LyDo", item.GhiChu ?? "");
                                doc.ReplaceText("@LoaiNghi", HinhThucNghi.Ten);
                                doc.Save();
                                //doc.ReplaceText("@NgayQuyetDinh", quytrinh.NgayHieuLuc?.ToString("dd/MM/yyyy"));
                                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_XinNghiPhepCN.docx")
                                                + "&path=" + Base64Encode(sFileNameCopy);
                                doc.Save();
                                listfile.Add(sFileNameCopy);
                            }
                        }
                        else
                        {
                            if (list.Contains(HinhThucNghi.Ma))
                            {
                                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_XinNghiPhepKLGSNV.docx");
                                string sFileNameCopy = PathUploads + "DonXinNghiPhep_" + item.TenNhanVien + ".doc";
                                File.Copy(fileName, sFileNameCopy, true);
                                var doc = DocX.Load(sFileNameCopy);
                                doc.ReplaceText("@Phong", item.TenBoPhan.ToUpper());
                                doc.ReplaceText("@HoVaTen", item.TenNhanVien);
                                doc.ReplaceText("@NgaySinh", NhanVien.NgaySinh.ToString("dd/MM/yyyy"));
                                doc.ReplaceText("@SDT", SDT.SoDienThoai ?? "");
                                doc.ReplaceText("@NoiCapCCCD", listNoiCap.Where(a => a.IdGuid == (GiayTo.NoiCap ?? "")).Select(b => b.Ten).FirstOrDefault() ?? "");
                                doc.ReplaceText("@CCCDNgayCap", GiayTo.NgayCap == null ? "" : GiayTo.NgayCap.Value.ToString("dd/MM/yyyy"));
                                doc.ReplaceText("@CCCD", GiayTo.So ?? "");
                                doc.ReplaceText("@BoPhan", CongTy.TenBoPhan);
                                string NgayThang = "Từ ngày " + item.NgayBatDau.Day + " tháng " + item.NgayBatDau.Month + " năm " + item.NgayBatDau.Year + " Đến ngày " + item.NgayKetThuc.Day + " tháng " + item.NgayKetThuc.Month + " năm " + item.NgayKetThuc.Year;
                                doc.ReplaceText("@TuNgay", NgayThang);
                                doc.ReplaceText("@LyDo", item.GhiChu ?? "");
                                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_XinNghiPhepKLGSNV.docx")
                                              + "&path=" + Base64Encode(sFileNameCopy);
                                doc.Save();
                                listfile.Add(sFileNameCopy);
                            }
                            else
                            {
                                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_XinNghiPhepKLCN.docx");
                                string sFileNameCopy = PathUploads + "DonXinNghiPhep_" + item.TenNhanVien + ".doc";
                                File.Copy(fileName, sFileNameCopy, true);
                                var doc = DocX.Load(sFileNameCopy);
                                doc.ReplaceText("@HoVaTen", item.TenNhanVien);
                                doc.ReplaceText("@NgaySinh", NhanVien.NgaySinh.ToString("dd/MM/yyyy"));
                                doc.ReplaceText("@SDT", SDT.SoDienThoai ?? "");
                                doc.ReplaceText("@NoiCapCCCD", listNoiCap.Where(a => a.IdGuid == (GiayTo.NoiCap ?? "")).Select(b => b.Ten).FirstOrDefault() ?? "");
                                doc.ReplaceText("@CCCDNgayCap", GiayTo.NgayCap == null ? "" : GiayTo.NgayCap.Value.ToString("dd/MM/yyyy"));
                                doc.ReplaceText("@CCCD", GiayTo.So ?? "");
                                doc.ReplaceText("@BoPhan", CongTy.TenBoPhan);
                                string NgayThang = "Từ ngày " + item.NgayBatDau.Day + " tháng " + item.NgayBatDau.Month + " năm " + item.NgayBatDau.Year + " Đến ngày " + item.NgayKetThuc.Day + " tháng " + item.NgayKetThuc.Month + " năm " + item.NgayKetThuc.Year;
                                doc.ReplaceText("@TuNgay", NgayThang);
                                doc.ReplaceText("@LyDo", item.GhiChu ?? "");


                                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_XinNghiPhepKLCN.docx")
                                                + "&path=" + Base64Encode(sFileNameCopy);
                                doc.Save();
                                listfile.Add(sFileNameCopy);
                            }
                        }
                    }
                }
                if (listfile != null && listfile.Count > 1)
                {
                    string sFileNameZip = PathUploads + "DonXinNghiPhepGSNV_" + quytrinh.MucDich + ".zip";
                    using (var zipStream = new FileStream(sFileNameZip, FileMode.Create))
                    {
                        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                        {
                            foreach (var filePath in listfile)
                            {
                                var zipEntry = archive.CreateEntry(Path.GetFileName(filePath));
                                using (var entryStream = zipEntry.Open())
                                {
                                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }
                    }
                    data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("DonXinNghiPhepGSNV_" + quytrinh.MucDich + ".docx")
                                        + "&path=" + Base64Encode(sFileNameZip);


                }

                return data;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        #endregion
    }
}