#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Features.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using HRM.Api.Entity;
using HRM.Api.Helper;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static HRM.Api.Services.CallWebService;
using static HRM.Api.Helper.HelperConstant;
namespace HRM.Api.Services.NhanVien
{


        public class FuncKeHoachLamViec : FuncBase
    {
        private readonly IAttendancePairingService _attendancePairingService;
        private string Host => HrmOptions.Host;

        public FuncKeHoachLamViec(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncKeHoachLamViec> logger,
            IAttendancePairingService attendancePairingService)
            : base(callWebService, workflowEngine, options, logger)
        {
            _attendancePairingService = attendancePairingService;
        }
        #region Kế hoạch làm việc tuần
        public dynamic GetNextSoQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "KHLTG",
                db.HRM_KeHoachLamViecTuan.Where(a => !string.IsNullOrEmpty(a.SoQuyTrinh)).OrderByDescending(a => a.SoQuyTrinh).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_KeHoachLamViecTuan> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_KeHoachLamViecTuan
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_KeHoachLamViecTuan
                             orderby a.Created descending
                             select a;

                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                if (listCongTy != null)
                {
                    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdCongTy));
                }
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.TuNgay >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.DenNgay < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));

                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_KeHoachLamViecTuan" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }

                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                List<BoPhan> listDuAn = _CallWebService.GetDanhSachDuAn(db);
                foreach (HRM_KeHoachLamViecTuan item in itemList)
                {
                    item.DateTimeToUnix();
                    BoPhan duAn = listDuAn.Find(x => x.Id == item.IdCongTy);
                    if (duAn != null)
                        item.TenCongty = duAn.TenBoPhan;
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
        public dynamic GetQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db, string Id)
        {
            try
            {
                HRM_KeHoachLamViecTuan QuyTrinh = db.HRM_KeHoachLamViecTuan.FirstOrDefault(a => a.IdGuid == Id);
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                QuyTrinh.DateTimeToUnix();
                HRM_KeHoachLamViecTuanViewModel result = new HRM_KeHoachLamViecTuanViewModel(QuyTrinh);

                result.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVienViewModel>();

                var ListUser = db.HRM_KeHoachLamViecTuan_NhanVien.Where(r => r.IdKeHoachLamViecTuan == Id).OrderBy(a => a.SapXep).ToList();
                var listThoiGian = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(r => r.IdKeHoachTuan != null && r.IdKeHoachTuan == Id).OrderBy(a => a.SapXep).ToList();
                if (listThoiGian != null && listThoiGian.Count > 0)
                {
                    foreach (var item in listThoiGian)
                    {
                        item.DateTimeToUnix();
                    }
                }
                if (ListUser != null && ListUser.Count > 0)
                {
                    List<string> listIdNhanVien = ListUser.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);

                    foreach (var IdNv in listIdNhanVien)
                    {
                        var item = new HRM_KeHoachLamViecTuan_NhanVienViewModel();
                        item.IdNhanVien = IdNv;
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        if (nhanVien != null)
                        {
                            item.MaNhanVien = nhanVien.MaNhanVien;
                            item.TenNhanVien = nhanVien.TenDayDu;
                            if (listChucVu != null && listChucVu.Count > 0)
                            {
                                var chucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                                if (chucVu != null)
                                    item.ChucVu = chucVu.TenChucVu;
                            }
                            //if (listChucDanh != null && listChucDanh.Count > 0)
                            //{
                            //    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                            //    if (ChucDanh != null)
                            //        item.TenChucDanh = ChucDanh.Ten;
                            //}

                            if (listBoPhan != null && listBoPhan.Count > 0)
                            {
                                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                                if (BoPhan != null)
                                    item.TenBoPhan = BoPhan.TenBoPhan;
                            }

                        }

                        item.listWeek = new List<HRM_KeHoachLamViecTuan_ChiTietViewModel>();

                        var data = ListUser.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Ngay).ToList();
                        if (data != null && data.Count > 0)
                        {

                            if (listThoiGian != null && listThoiGian.Count > 0)

                            {
                                foreach (var ca in data)
                                {
                                    ca.listCaChinh = listThoiGian.Where(a => a.Ngay == ca.Ngay && a.IdNhanVien == item.IdNhanVien && a.Loai == 0).OrderBy(a => a.SapXep).ToList();
                                    ca.listCaTangCuong = listThoiGian.Where(a => a.Ngay == ca.Ngay && a.IdNhanVien == item.IdNhanVien && a.Loai == 1).OrderBy(a => a.SapXep).ToList();


                                }
                            }
                        }
                        item.SoNgayDaNghi = data.FirstOrDefault().SoNgayDaNghi;
                        item.listWeek = data.Select(a => new HRM_KeHoachLamViecTuan_ChiTietViewModel(a)).ToList();
                        result.listNhanVien.Add(item);
                    }
                }
                result.listNhanVien.OrderBy(x => x.MaNhanVien).ToList();
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic SetQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_KeHoachLamViecTuanViewModel QuyTrinh = JsonConvert.DeserializeObject<HRM_KeHoachLamViecTuanViewModel>(data.ToString());
                    // DI: use inherited _CallWebService
                    var QuyTrinhdb = db.HRM_KeHoachLamViecTuan.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (QuyTrinhdb != null && QuyTrinhdb.IdTrangThai != QuyTrinh.IdTrangThai)
                    {
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = QuyTrinh.IdGuid,
                            eAction = DefineData.eAction.KEHOACHLAMVIECTUAN.ToString(),
                            IdTrangThai = QuyTrinh.IdTrangThai,
                        };
                        var trangThai = _CallWebService.DoiTrangThaiKhoiTao(audit, db);
                    }
                    MessageReturn msg = UpdateQuyTrinhKeHoachLamViecTuan(QuyTrinh, db);
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
        public dynamic UpdateQuyTrinhKeHoachLamViecTuan(HRM_KeHoachLamViecTuanViewModel entity, ApplicationDbContext db = null)
        {
            try
            {
                HRM_KeHoachLamViecTuan QuyTrinh = new HRM_KeHoachLamViecTuan(entity);
                QuyTrinh.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVien>();
                if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                {
                    foreach (var item in entity.listNhanVien)
                    {
                        if (item.listWeek != null && item.listWeek.Count > 0)
                        {
                            foreach (var child in item.listWeek)
                            {
                                HRM_KeHoachLamViecTuan_NhanVien nv = new HRM_KeHoachLamViecTuan_NhanVien(item, child);
                                QuyTrinh.listNhanVien.Add(nv);
                            }
                        }
                    }
                }

                QuyTrinh.UnixToDateTime();
                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.IdGuid = Guid.NewGuid().ToString();
                    if (string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        TrangThaiQuyTrinh QuyTrinhTrangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.KEHOACHLAMVIECTUAN.ToString(), QuyTrinh.IdGuid);
                        if (QuyTrinhTrangThai.IdTrangThai == null || QuyTrinhTrangThai.IdTrangThai == "")
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, QuyTrinhTrangThai.TenTrangThai + " do quy trình số " + QuyTrinh.SoQuyTrinh);
                        }
                        QuyTrinh.IdTrangThai = QuyTrinhTrangThai.IdTrangThai;
                        QuyTrinh.TenTrangThai = QuyTrinhTrangThai.TenTrangThai;
                    }
                    QuyTrinh.Modified = DateTime.Now;
                    db.HRM_KeHoachLamViecTuan.Add(QuyTrinh);
                }
                else
                {
                    int Check = db.HRM_KeHoachLamViecTuan.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh && r.IdGuid != QuyTrinh.IdGuid).AsNoTracking().Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhKeHoachLamViecTuan(db).SoPhieu.ToString();
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                var ListUser = db.HRM_KeHoachLamViecTuan_NhanVien.Where(r => r.IdKeHoachLamViecTuan == QuyTrinh.IdGuid).ToList();
                db.HRM_KeHoachLamViecTuan_NhanVien.RemoveRange(ListUser);
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                var listDataDb = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.IdKeHoachTuan != null && a.IdKeHoachTuan == QuyTrinh.IdGuid).ToList();
                if (listDataDb != null && listDataDb.Count > 0)
                    db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.RemoveRange(listDataDb);
                if (QuyTrinh.listIdBoPhan != null && QuyTrinh.listIdBoPhan.Count > 0)
                {
                    foreach (var item in QuyTrinh.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = QuyTrinh.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_KeHoachLamViecTuan);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }

                if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                {
                    int STT = 1;
                    foreach (var item in QuyTrinh.listNhanVien)
                    {
                        item.SapXep = STT;
                        item.UnixToDateTime();
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdKeHoachLamViecTuan = QuyTrinh.IdGuid;

                        item.IdCongTy = QuyTrinh.IdCongTy;

                        STT++;

                        if (item.listCaChinh != null && item.listCaChinh.Count > 0)
                        {
                            foreach (var ca in item.listCaChinh)
                            {
                                ca.NgayUnix = item.NgayUnix;
                                ca.UnixToDateTime();
                                ca.IdGuid = Guid.NewGuid().ToString();
                                ca.Loai = 0;
                                ca.SapXep = STT;
                                ca.IdNhanVien = item.IdNhanVien;
                                ca.IdBoPhan = item.IdBoPhan;
                                ca.IdCongTy = item.IdCongTy;
                                ca.isKetThuc = QuyTrinh.isKetThuc;
                                ca.IdKeHoachTuan = QuyTrinh.IdGuid;

                                STT++;
                            }
                            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AddRange(item.listCaChinh);
                        }
                        if (item.listCaTangCuong != null && item.listCaTangCuong.Count > 0)
                        {
                            foreach (var ca in item.listCaTangCuong)
                            {
                                ca.NgayUnix = item.NgayUnix;
                                ca.UnixToDateTime();
                                ca.IdGuid = Guid.NewGuid().ToString();
                                ca.Loai = 1;
                                ca.IdBoPhan = item.IdBoPhan;
                                ca.IdCongTy = item.IdCongTy;
                                ca.isKetThuc = QuyTrinh.isKetThuc;
                                ca.IdNhanVien = item.IdNhanVien;
                                ca.IdKeHoachTuan = QuyTrinh.IdGuid;
                                ca.SapXep = STT;
                                STT++;
                            }
                            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AddRange(item.listCaTangCuong);
                        }

                    }
                    db.HRM_KeHoachLamViecTuan_NhanVien.AddRange(QuyTrinh.listNhanVien);
                }
                if (QuyTrinh.isKetThuc == true && QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                {
                    List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    DateTime tuNgay = new DateTime(QuyTrinh.TuNgay.Value.Year, QuyTrinh.TuNgay.Value.Month, QuyTrinh.TuNgay.Value.Day, 0, 0, 0);
                    DateTime DenNgay = new DateTime(QuyTrinh.DenNgay.Value.Year, QuyTrinh.DenNgay.Value.Month, QuyTrinh.DenNgay.Value.Day, 0, 0, 0);
                    var listDataTGDb = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.isTheoDoi == true && a.Ngay >= tuNgay && a.Ngay <= DenNgay
                 && a.IdCongTy == QuyTrinh.IdCongTy && listIdNhanVien.Contains(a.IdNhanVien)).OrderBy(a => a.Ngay).ToList();
                    if (listDataTGDb != null && listDataTGDb.Count() > 0)
                        db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.RemoveRange(listDataTGDb);
                    List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listThoiGianAdd = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
                    foreach (var item in QuyTrinh.listNhanVien)
                    {

                        if (item.listCaChinh != null && item.listCaChinh.Count > 0)
                        {
                            foreach (var ca in item.listCaChinh)
                            {
                                var itemAdd = ca.Clone();
                                itemAdd.IdGuid = Guid.NewGuid().ToString();
                                itemAdd.IdKeHoachTuan = null;
                                itemAdd.isTheoDoi = true;
                                //itemAdd.IdTheoDoi = ca.IdGuid;
                                listThoiGianAdd.Add(itemAdd);

                            }

                        }
                        if (item.listCaTangCuong != null && item.listCaTangCuong.Count > 0)
                        {
                            foreach (var ca in item.listCaTangCuong)
                            {
                                var itemAdd = ca.Clone();
                                itemAdd.IdGuid = Guid.NewGuid().ToString();
                                itemAdd.IdKeHoachTuan = null;
                                itemAdd.isTheoDoi = true;
                                //itemAdd.IdTheoDoi = ca.IdGuid;
                                listThoiGianAdd.Add(itemAdd);

                            }

                        }
                    }
                    db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AddRange(listThoiGianAdd);
                    db.SaveChanges();
                    AddThoiGianLamViecKeHoachNew(db, listIdNhanVien, QuyTrinh.TuNgay ?? DateTime.Now, QuyTrinh.DenNgay ?? DateTime.Now, 0, entity.IdCongTy);
                }
                db.SaveChanges();
                entity.IdGuid = QuyTrinh.IdGuid;
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công", entity);
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public class Add
        {
            public List<string> listIdNhanVien { get; set; }
            public double TuNgay { get; set; }
            public double DenNgay { get; set; }
        }
        public void AddThoiGianLamViecKeHoachNew(ApplicationDbContext db, List<string> listIdNhanVien, DateTime dtTuNgay, DateTime dtDenNgay, int Loai = 0, long IdDuAn = 0)
        {

            DateTime _dtTuNgay = new DateTime(dtTuNgay.Year, dtTuNgay.Month, dtTuNgay.Day, 0, 0, 0);
            DateTime _dtDenNgay = new DateTime(dtDenNgay.Year, dtDenNgay.Month, dtDenNgay.Day, 0, 0, 0);
            _dtDenNgay = _dtDenNgay.AddDays(1);
            bool isLamThemGio = true;
            if (Loai == 0)
                isLamThemGio = false;
            //Add vào thời gian làm việc của nhân viên theo kế hoạch
            List<HRM_ThoiGianLamViecKeHoach> listThoiGianLamViecKeHoachDB = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.isLamThemGio == isLamThemGio && x.IdCongTy == IdDuAn
                                                                                                            && x.Ngay >= _dtTuNgay && x.Ngay < _dtDenNgay).ToList();
            db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listThoiGianLamViecKeHoachDB);

            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listTheoDoiLichLamViecNhanVien = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AsNoTracking().Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien) && string.IsNullOrEmpty(x.IdKeHoachTuan)
                                                                                         && x.Ngay >= _dtTuNgay && x.Ngay < _dtDenNgay && x.IdCongTy == IdDuAn).ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).Distinct().ToList();
            var listIdCaLamViec = listNhanVien.Select(a => a.IdCaLamViec).Distinct().ToList();
            var listCaLamViec = db.HRM_dmLichLamViec.ToList();
            List<HRM_ThoiGianLamViecKeHoach> listAdd = new List<HRM_ThoiGianLamViecKeHoach>();
            if (listTheoDoiLichLamViecNhanVien != null && listTheoDoiLichLamViecNhanVien.Count() > 0)
                foreach (var itemGio in listTheoDoiLichLamViecNhanVien)
                {
                    var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == itemGio.IdNhanVien);
                    var dmLichLamViec = new HRM_dmLichLamViec();
                    if (nhanVien != null)
                    {
                        if (!string.IsNullOrEmpty(itemGio.IdCaLamViec))
                            dmLichLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == itemGio.IdCaLamViec);
                        else
                            dmLichLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == nhanVien.IdCaLamViec);
                        if (dmLichLamViec == null)
                            dmLichLamViec = new HRM_dmLichLamViec();
                    }

                    if (itemGio.TuGio.Value.Year == 1790)
                        continue;
                    if (itemGio.DenGio.Value.Year == 1790)
                        continue;
                    HRM_ThoiGianLamViecKeHoach itemAdd = new HRM_ThoiGianLamViecKeHoach();
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.GioVao = new DateTime(itemGio.Ngay.Year, itemGio.Ngay.Month, itemGio.Ngay.Day, itemGio.TuGio.Value.Hour, itemGio.TuGio.Value.Minute, 0);
                    itemAdd.GioRa = new DateTime(itemGio.Ngay.Year, itemGio.Ngay.Month, itemGio.Ngay.Day, itemGio.DenGio.Value.Hour, itemGio.DenGio.Value.Minute, 0);
                    if (itemAdd.GioVao > itemAdd.GioRa)
                        itemAdd.GioRa = itemAdd.GioRa.AddDays(1);
                    itemAdd.SoGioNghi = itemGio.SoGioNghi;
                    itemAdd.TongSoGioGiao = (itemAdd.GioRa - itemAdd.GioVao).TotalHours;
                    itemAdd.Ngay = itemGio.Ngay;
                    itemAdd.IdNhanVien = itemGio.IdNhanVien;
                    itemAdd.IddmCaLamViec = dmLichLamViec.IdGuid;
                    itemAdd.isCaGay = dmLichLamViec.isCaGay ?? false;
                    itemAdd.HeSoTangCa = dmLichLamViec.HeSoLuongTangCa ?? 1;
                    itemAdd.NgayCongTieuChuan = dmLichLamViec.SoNgayCongTieuChuan ?? 26;
                    itemAdd.GioTieuChuan = dmLichLamViec.SoGioTrongNgay ?? 8;
                    itemAdd.Loai = itemGio.Loai;
                    itemAdd.IdCongTy = itemGio.IdCongTy;
                    itemAdd.isLamThemGio = isLamThemGio;
                    listAdd.Add(itemAdd);
                }
            db.HRM_ThoiGianLamViecKeHoach.AddRange(listAdd);
            db.SaveChanges();
        }
        public List<clsDiMuonVeSom> TinhSomMuon(ApplicationDbContext db, string IdNhanVien, List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs, List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom,
                                                    List<HRM_ThoiGianLamViecKeHoach> listThoiGianLamViec, List<HRM_CheckInOut> listCheckInOut, DateTime Ngay, double SoPhutDuocDiMuon, double SoPhutDuocVeSom, string MaCongTy, List<HRM_TheoDoiXinNghiPhep> listNghiPhep = null, bool XacNhan = false, long IdDuAn = 0, HRM_dmLichLamViec Ca = null)
        {
            Ngay = Ngay.Date;
            listThoiGianLamViec = listThoiGianLamViec.OrderBy(x => x.Ngay).ThenBy(x => x.GioVao).ToList();
            listCheckInOut = listCheckInOut.OrderBy(x => x.Gio).ToList();
            List<clsDiMuonVeSom> listOut = new List<clsDiMuonVeSom>();
            //var listCheckIn = listCheckInOut.FindAll(x => x.Loai == DefineData.eKyHieuChamCong.I.ToString());
            //var listCheckOut = listCheckInOut.FindAll(x => x.Loai == DefineData.eKyHieuChamCong.O.ToString());

            //Thiết lập giờ vào và giờ ra

            //string VeSom = HrmOptions.SoPhutSom.ToString();
            ////double SoPhutDuocVeSom = double.Parse(VeSom);
            //string DiMuon = HrmOptions.SoPhutMuon.ToString();
            //double SoPhutDuocDiMuon = double.Parse(DiMuon);
            List<HRM_ThoiGianLamViecKeHoach> listInOutCD = new List<HRM_ThoiGianLamViecKeHoach>();
            int nCount = listCheckInOut.Count;
            double SoGioLamViecTT = 0;
            for (int i = 0; i < nCount; i++)
            {
                var itemIn = listCheckInOut[i];
                if (itemIn.Loai == DefineData.eKyHieuChamCong.O.ToString())
                    continue;
                HRM_ThoiGianLamViecKeHoach InOutCD = new HRM_ThoiGianLamViecKeHoach();
                InOutCD.IdCongTy = itemIn.IdCongTy;
                var listCheckInOuts = listCheckInOut/*.Where(a => a.IdCongTy == itemIn.IdCongTy)*/.ToList();
                nCount = listCheckInOuts.Count;
                InOutCD.GioVao = itemIn.Gio.AddMinutes(-SoPhutDuocDiMuon);
                if (nCount > i + 1)
                {
                    var itemOut = listCheckInOuts.FirstOrDefault(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Gio > InOutCD.GioVao);
                    if (itemOut != null)
                    {
                        //var itemVaoTiep = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.I.ToString() && a.Gio < itemOut.Gio && a.Gio > InOutCD.GioVao).OrderBy(a => a.Gio).FirstOrDefault();
                        //if (itemVaoTiep != null && itemVaoTiep.Ngay.Date != InOutCD.GioVao.Date)
                        //    break;
                        InOutCD.GioRa = itemOut.Gio.AddMinutes(SoPhutDuocVeSom);

                    }
                    else continue;
                }
                if (i + 2 < nCount && InOutCD.GioRa != null && InOutCD.GioRa.Year > 2024)
                {
                    var itemVaoTiep = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.I.ToString() && a.Gio > InOutCD.GioRa).OrderBy(a => a.Gio).FirstOrDefault();
                    if (itemVaoTiep != null)
                    {
                        InOutCD.GioVaoTiep = itemVaoTiep.Gio.AddMinutes(-SoPhutDuocDiMuon);
                        var itemOut = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Gio > InOutCD.GioVao && a.Gio <= InOutCD.GioVaoTiep).OrderByDescending(a => a.Gio).FirstOrDefault();
                        if (itemOut != null)
                            InOutCD.GioRa = itemOut.Gio.AddMinutes(SoPhutDuocVeSom);
                    }
                    else
                    {
                        var itemOut = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Gio > InOutCD.GioVao).OrderByDescending(a => a.Gio).FirstOrDefault();
                        if (itemOut != null)
                            InOutCD.GioRa = itemOut.Gio.AddMinutes(SoPhutDuocVeSom);
                    }
                    SoGioLamViecTT += (InOutCD.GioRa - InOutCD.GioVao).TotalMinutes;

                }
                else
                    SoGioLamViecTT += (InOutCD.GioRa - InOutCD.GioVao).TotalMinutes;

                listInOutCD.Add(InOutCD);
            }
            if (listInOutCD.Count == 0 || nCount == 1)
                return listOut;

            double SoGioLamViec = 0;
            var listInOutNgay = new List<HRM_ThoiGianLamViecKeHoach>();
            //Xác định giờ vào ra thực tế so với giờ theo kế hoạch, check ra vào liên tục
            foreach (var item in listThoiGianLamViec)
            {
                if (listNghiPhep != null && listNghiPhep.Count > 0)
                    item.SoGioNghi = 0;
                if (item.Loai == 0)
                {
                    if (item.TongSoGioGiao == 0)
                        SoGioLamViec += (item.GioRa - item.GioVao).TotalMinutes - (item.SoGioNghi ?? 0) * 60;
                    else
                        SoGioLamViec += item.TongSoGioGiao * 60 - (item.SoGioNghi ?? 0) * 60;
                    try
                    {
                        double GioCheck = (item.GioRa - item.GioVao).TotalMinutes / 60;
                        double GioCheckTT = (item.GioRaTT.Value - item.GioVaoTT.Value).TotalMinutes / 60;
                        if (GioCheckTT / GioCheck <= 0.5)
                            SoGioLamViec += (item.SoGioNghi ?? 0) * 60;
                    }catch (Exception)
                    { }
                }
                var listInOutCDs = listInOutCD.Where(a => (a.GioVao < item.GioRa && a.GioRa > item.GioVao)).OrderBy(a => a.GioVao).ToList();
                if (listInOutCDs != null && listInOutCDs.Count > 0)
                {
                    foreach (var itemInOut in listInOutCDs/*.Where(a => a.IdCongTy == item.IdCongTy)*/.ToList())
                    {
                        if (itemInOut.GioRa < item.GioVao)
                            continue;
                        if (itemInOut.GioVao > item.GioRa && item.GioVao > item.GioRa)
                            continue;
                        if (item.GioVaoTT == null || item.GioVaoTT > itemInOut.GioVao)
                        {
                            if (item.GioVao >= itemInOut.GioVao && item.GioVao < itemInOut.GioRa)
                                item.GioVaoTT = item.GioVao;
                            else if (item.GioVao <= itemInOut.GioVao && item.GioRa > itemInOut.GioVao)
                            {
                                item.GioVaoTT = itemInOut.GioVao;
                            }
                            //else
                            //{
                            //    item.GioVaoTT = item.GioRa;
                            //}
                        }
                        if (item.GioRaTT == null || (item.GioRaTT <= itemInOut.GioRa))
                            item.GioRaTT = itemInOut.GioRa;
                        if (itemInOut.GioVaoTiep != null)
                            item.GioVaoTiep = itemInOut.GioVaoTiep;
                    }
                    var inOut = new HRM_ThoiGianLamViecKeHoach();
                    if (item.GioRaTT != null && item.GioVaoTT != null)
                    {
                        inOut.GioVao = item.GioVaoTT.Value;
                        if (inOut.GioVao < item.GioVao)
                            inOut.GioVao = item.GioVao;
                        inOut.GioRa = item.GioRaTT.Value;
                        if (inOut.GioRa > item.GioRa)
                            inOut.GioRa = item.GioRa;
                        if (Ca != null && Ca.LoaiNgayNghi == 4 && (int)item.Ngay.DayOfWeek == 5 && inOut.GioRa.Hour > 12)
                            inOut.GioRa = new DateTime(inOut.GioRa.Year, inOut.GioRa.Month, inOut.GioRa.Day, 12, 0, 0);

                        listInOutNgay.Add(inOut);
                    }
                    //if (itemInOut.GioVao != null && itemInOut.GioVao.Year >= 2025)
                    //    item.GioVaoTT = itemInOut.GioVao;
                    //if (itemInOut.GioRa != null && itemInOut.GioRa.Year >= 2025)
                    //    item.GioRaTT = itemInOut.GioRa;
                    //itemInOut.GioVaoTiep = itemInOut.GioVaoTiep;
                }

            }
            if (SoGioLamViecTT >= SoGioLamViec && MaCongTy == "VPCT" && Host == "PHT")
                return listOut;
            listThoiGianLamViec = listThoiGianLamViec.OrderBy(a => a.GioVao).ToList();
            int STT = 0;
            double heSo = 0;
            int SoPhutNghi = 0;
            string MaHinhThucNghi = "";
            string LoaiNghi = "";
            if (listNghiPhep != null && listNghiPhep.Count > 0)
            {
                heSo = listNghiPhep.Sum(a => a.ThoiGianNghi);
                SoPhutNghi = (int)(heSo * SoGioLamViec);
                MaHinhThucNghi = listNghiPhep.FirstOrDefault().HinhThucNghi;
                LoaiNghi = string.Join(",", listNghiPhep.Select(a => a.MaLoaiNghi).Distinct().ToList());
                // check sáng chiều
                DateTime time12 = Ngay.Date.AddHours(12);

                if (!LoaiNghi.Contains("CANGAY"))
                {
                    var listInOut = new List<HRM_ThoiGianLamViecKeHoach>();

                    if (LoaiNghi.Contains("SANG"))
                    {
                        listInOut = listInOutNgay.Where(a => a.GioVao <= time12 && a.GioRa >= Ngay).ToList();

                    }
                    else
                    {
                        listInOut = listInOutNgay.Where(a => a.GioVao <= Ngay.AddDays(1) && a.GioRa >= time12).ToList();

                    }
                    if (listInOut != null && listInOut.Count > 0)
                    {
                        //double SoPhutPhep = 0;
                        foreach (var item in listInOut)
                        {
                            DateTime GioVe = item.GioRa;
                            if (GioVe > time12)
                                GioVe = time12;
                            SoPhutNghi = SoPhutNghi - (int)((GioVe - item.GioVao).TotalMinutes);
                        }
                    }
                    else
                    {
                        SoPhutNghi = 0;
                        //clsDiMuonVeSom itemOut = new clsDiMuonVeSom();

                        //if (XacNhan == false)
                        //{
                        //    HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == MaHinhThucNghi);
                        //    if (NghiPhep != null)
                        //    {
                        //        itemOut.IddmKyHieuChamCong = NghiPhep.IdGuid;
                        //        itemOut.KyHieuChamCong = NghiPhep.Ma;
                        //        if (heSo < 1)
                        //            itemOut.KyHieuChamCong = NghiPhep.Ma + "/2";
                        //        else
                        //            itemOut.KyHieuChamCong = NghiPhep.Ma;
                        //    }
                        //    else
                        //        itemOut.KyHieuChamCong = MaHinhThucNghi;
                        //    itemOut.SoPhut = (int)SoPhutNghi;
                        //    listOut.Add(itemOut);
                        //}
                    }
                }
                else
                {
                    //foreach (var item in listInOutCD)
                    //{
                    //    SoPhutNghi = SoPhutNghi - (int)((item.GioRa - item.GioVao).TotalMinutes);
                    //}
                }

            }
            int SoPhutDiMuon = 0;
            int SoPhutVeSom = 0;
            var checkSoPhutDiMuon = listDiMuonVeSom.Where(a => a.IdLoai == 1 && a.IdNhanVien == IdNhanVien && a.NgayDangKy == Ngay).ToList();
            if (checkSoPhutDiMuon != null)
            {
                double SoGio = checkSoPhutDiMuon.Sum(a => a.TongGio);
                SoPhutDiMuon = (int)Math.Round((SoGio) * 60, 0);
            }
            var checkSoPhutVeSom = listDiMuonVeSom.Where(a => a.IdLoai == 2 && a.IdNhanVien == IdNhanVien && a.NgayDangKy == Ngay);
            if (checkSoPhutVeSom != null)
            {
                //SoPhutVeSom = (int)((checkSoPhutVeSom.Sum(a => a.TongGio)) * 60);
                double SoGio = checkSoPhutVeSom.Sum(a => a.TongGio);
                SoPhutVeSom = (int)Math.Round((SoGio) * 60, 0);
            }
            //double soPhutNghiMS = SoPhutNghi;
            if (SoPhutNghi > 0)
            {
                clsDiMuonVeSom itemOut = new clsDiMuonVeSom();

                if (XacNhan == false)
                {
                    HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == MaHinhThucNghi);
                    if (NghiPhep != null)
                    {
                        itemOut.IddmKyHieuChamCong = NghiPhep.IdGuid;
                        itemOut.KyHieuChamCong = NghiPhep.Ma;
                        if (heSo < 1)
                            itemOut.KyHieuChamCong = NghiPhep.Ma + "/2";
                        else
                            itemOut.KyHieuChamCong = NghiPhep.Ma;
                    }
                    else
                        itemOut.KyHieuChamCong = MaHinhThucNghi;
                    itemOut.SoPhut = (int)SoPhutNghi;
                    listOut.Add(itemOut);
                }
            }
            foreach (var item in listThoiGianLamViec)
            {
                if (item.isCongTac)
                {
                    var check = listOut.FirstOrDefault(a => a.KyHieuChamCong == DefineData.eKyHieuChamCong.CT.ToString());
                    if (check != null)
                        continue;
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.CT.ToString());
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom();
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;
                        itemOut.SoPhut = 0;
                    }

                    listOut.Add(itemOut);
                    continue;
                }
                //if (Host == "ICT")
                //{
                //    if (IdDuAn != 0 && item.IdCongTy != IdDuAn)
                //        continue;
                //}
                if (item.GioVaoTT == null && item.GioRaTT == null)
                {
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                    if (item.TongSoGioGiao == 0)
                        itemOut.SoPhut = (int)(item.GioRa - item.GioVao).TotalMinutes;
                    else
                        itemOut.SoPhut = (int)(item.TongSoGioGiao * 60);
                    if (SoPhutNghi > 0)
                    {
                        if (XacNhan == false)
                        {
                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == MaHinhThucNghi);
                            if (NghiPhep != null)
                            {
                                itemOut.IddmKyHieuChamCong = NghiPhep.IdGuid;
                                itemOut.KyHieuChamCong = NghiPhep.Ma;
                            }
                            itemOut.isCaGay = item.isCaGay;
                            itemOut.Loai = item.Loai;
                            //itemOut.SoPhut = (int)SoPhutNghi;
                            listOut.Add(itemOut);
                        }

                        if (SoPhutNghi >= itemOut.SoPhut)
                        {
                            SoPhutNghi -= itemOut.SoPhut;


                        }
                        else
                        {
                            itemOut.SoPhut = (int)(itemOut.SoPhut - SoPhutNghi);

                        }
                        continue;
                    }
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                    if (dmKyHieuChamCong != null)
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;

                    }

                    itemOut.isCaGay = item.isCaGay;
                    itemOut.Loai = item.Loai;
                    listOut.Add(itemOut);
                    continue;
                }
                if (item.GioVaoTT == null)
                    item.GioVaoTT = item.GioVao;
                if (item.GioRaTT == null)
                    item.GioRaTT = item.GioRa;
                var SoGio = (item.GioVaoTT.Value - item.GioVao).TotalSeconds;
                if (SoGio > 0)
                {
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                    itemOut.GioVao = item.GioVao;
                    itemOut.isCaGay = item.isCaGay;
                    itemOut.Loai = item.Loai;
                    itemOut.SoPhut = (int)(SoGio / 60);
                    if (itemOut.SoPhut == 0)
                        continue;
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.M.ToString());
                    if (dmKyHieuChamCong != null)
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;
                        if (item.Loai == 0)
                        {
                            if (itemOut.SoPhut >= SoPhutNghi)
                            {
                                itemOut.SoPhut = itemOut.SoPhut - SoPhutNghi;
                                SoPhutNghi = 0;
                            }
                            else
                            {
                                SoPhutNghi -= itemOut.SoPhut;
                                itemOut.SoPhut = 0;
                            }
                        }
                        if (itemOut.SoPhut >= SoPhutDiMuon)
                        {
                            itemOut.SoPhut = itemOut.SoPhut - SoPhutDiMuon;
                            SoPhutDiMuon = 0;
                        }
                        else
                        {
                            SoPhutDiMuon -= itemOut.SoPhut;
                            itemOut.SoPhut = 0;
                        }

                    }
                    if (itemOut.SoPhut != 0)
                        listOut.Add(itemOut);
                }
                SoGio = (item.GioRa - item.GioRaTT.Value).TotalSeconds;
                if (SoGio > 0)
                {
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                    itemOut.isCaGay = item.isCaGay;
                    itemOut.Loai = item.Loai;
                    itemOut.SoPhut = (int)(SoGio / 60);
                    if (itemOut.SoPhut == 0)
                        continue;
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.S.ToString());
                    if (dmKyHieuChamCong != null)
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;
                        if (item.Loai == 0)
                        {
                            if (itemOut.SoPhut >= SoPhutNghi)
                            {
                                itemOut.SoPhut = itemOut.SoPhut - SoPhutNghi;
                                SoPhutNghi = 0;
                            }
                            else
                            {
                                SoPhutNghi -= itemOut.SoPhut;
                                itemOut.SoPhut = 0;
                            }
                        }
                        if (itemOut.SoPhut >= SoPhutVeSom)
                        {
                            itemOut.SoPhut = itemOut.SoPhut - SoPhutVeSom;
                            SoPhutVeSom = 0;
                        }
                        else
                        {
                            SoPhutVeSom -= itemOut.SoPhut;
                            itemOut.SoPhut = 0;
                        }

                    }
                    if (itemOut.SoPhut != 0)
                        listOut.Add(itemOut);
                }
                if (item.GioVaoTiep < item.GioRa)
                {
                    var itemInOuts = listInOutCD.Where(a => (a.GioVao <= item.GioRa && a.GioRa >= item.GioVao)).OrderBy(a => a.GioVao).ToList();

                    //Ra ngoài giữa giờ
                    foreach (var itemInOut in itemInOuts)
                    {
                        if (itemInOut.GioVaoTiep == null)
                            continue;
                        if (itemInOut.GioVaoTiep >= item.GioRa)
                            continue;
                        if (itemInOut.GioRa >= item.GioVao && itemInOut.GioRa <= item.GioRa)
                        {
                            SoGio = (itemInOut.GioVaoTiep.Value - itemInOut.GioRa).TotalSeconds;
                            if (SoGio > 0)
                            {
                                clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                                itemOut.isCaGay = item.isCaGay;
                                itemOut.Loai = item.Loai;
                                itemOut.SoPhut = (int)(SoGio / 60);
                                HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.G.ToString());
                                if (dmKyHieuChamCong != null)
                                {
                                    itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                                    itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;
                                    var checkXinPhep = listDiMuonVeSom.FirstOrDefault(a => a.IdLoai == 3 && a.IdNhanVien == item.IdNhanVien && a.NgayDangKy == item.Ngay);
                                    if (checkXinPhep != null)
                                    {
                                        itemOut.SoPhut = itemOut.SoPhut - (int)(checkXinPhep.TongGio * 60);
                                        if (itemOut.SoPhut < 0)
                                            itemOut.SoPhut = 0;
                                    }
                                }
                                if (itemOut.SoPhut != 0)
                                    listOut.Add(itemOut);
                            }
                        }
                    }
                }


                STT++;
            }

            return listOut;
        }
        public dynamic DeleteQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db, string Id)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_KeHoachLamViecTuan QuyTrinh = db.HRM_KeHoachLamViecTuan.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_KeHoachLamViecTuan_NhanVien.Where(r => r.IdKeHoachLamViecTuan == Id).ToList();
                    db.HRM_KeHoachLamViecTuan_NhanVien.RemoveRange(ListUser);
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
        public dynamic ChuyenTiepQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn msg = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhập thành công");
                    HRM_KeHoachLamViecTuanViewModel QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_KeHoachLamViecTuanViewModel)) as HRM_KeHoachLamViecTuanViewModel;
                    if (QuyTrinh.isKetThuc != true)
                    {

                        //HRM_KeHoachLamViecTuan pcuDatabase = db.HRM_KeHoachLamViecTuan.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                        //if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                        //{
                        //    if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        //}
                        var callWeb = _CallWebService;
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = QuyTrinh.IdGuid,
                            eAction = DefineData.eAction.KEHOACHLAMVIECTUAN.ToString(),
                            IdTrangThai = QuyTrinh.IdTrangThai,

                        };
                        TrangThaiQuyTrinh TrangThaidnm = _CallWebService.GetTrangThaiChuyenTiep(db, audit);
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
                        msg = UpdateQuyTrinhKeHoachLamViecTuan(QuyTrinh, db);
                        if (msg.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                        }
                    }
                    //if (QuyTrinh.isKetThuc == true && QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                    //{
                    //    List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    //    AddThoiGianLamViecKeHoachNew(db, listIdNhanVien, QuyTrinh.TuNgay ?? DateTime.Now, QuyTrinh.DenNgay ?? DateTime.Now, 0, QuyTrinh.IdCongTy);

                    //}
                    dbContextTransaction.Commit();
                    return msg;


                }
                catch (Exception e)
                {
                    //dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhKeHoachLamViecTuan(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_KeHoachLamViecTuanViewModel QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_KeHoachLamViecTuanViewModel)) as HRM_KeHoachLamViecTuanViewModel;

                    HRM_KeHoachLamViecTuan pcuDatabase = db.HRM_KeHoachLamViecTuan.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }

                    }
                    MessageReturn msg = UpdateQuyTrinhKeHoachLamViecTuan(QuyTrinh, db);
                    if (msg.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return msg;
                    }
                    var callWeb = _CallWebService;
                    dynamic TrangThaidnm = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.KEHOACHLAMVIECTUAN.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
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
        public dynamic ImportKeHoachLamViecTuan(ApplicationDbContext db, string FileName, double TuNgayUnix, double DenNgayUnix, List<long> listIdBoPhan)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);

            List<HRM_KeHoachLamViecTuan_NhanVienViewModel> listResult = new List<HRM_KeHoachLamViecTuan_NhanVienViewModel>();
            List<HRM_KeHoachLamViecTuan_NhanVien> listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVien>();
            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listNhanVien_ThoiGian = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
            var sheet1 = package.Workbook.Worksheets.First();
            DateTime TuNgay = UnixTimeStampToDateTime(TuNgayUnix);
            TuNgay = new DateTime(TuNgay.Year, TuNgay.Month, TuNgay.Day, 0, 0, 0);
            DateTime DenNgay = UnixTimeStampToDateTime(DenNgayUnix);
            DenNgay = new DateTime(DenNgay.Year, DenNgay.Month, DenNgay.Day, 0, 0, 0);
            int CountDay = (int)((DenNgay - TuNgay).TotalDays) + 1;
            var listCa = db.HRM_dmLichLamViec.ToList();
            for (int row = 6; row <= sheet1.Cells.End.Row; row++)
            {

                int nCol = 2;
                string MaNhanVien = dungchung.GetCell(sheet1, row, nCol);
                if (string.IsNullOrEmpty(MaNhanVien))
                    break;
                nCol = 6;
                DateTime Ngay = TuNgay;
                while (Ngay <= DenNgay)
                {
                    int Col = nCol;
                    DateTime? Ngaycheck = dungchung.GetCellToDateTime(sheet1, 2, Col);

                    if (Ngaycheck != null && Ngay != Ngaycheck)
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Ngày " + Ngaycheck.Value.ToString("dd/MM/yyyy") + " không nằm trong khoảng thời gian đã chọn");
                    }
                    string MaCa = dungchung.GetCell(sheet1, row, Col);
                    var CaLamViec = listCa.FirstOrDefault(a => a.Ma == MaCa);
                    if (CaLamViec == null)
                        CaLamViec = new HRM_dmLichLamViec();
                    Col++;
                    // ca chính
                    for (int i = 1; i <= 2; i++)
                    {
                        /// vào
                        string sColText = GetCell(sheet1, row, Col);
                        if (!string.IsNullOrEmpty(sColText))
                        {
                            HRM_KeHoachLamViecTuan_NhanVien_ThoiGian ItemAdd = new HRM_KeHoachLamViecTuan_NhanVien_ThoiGian();
                            ItemAdd.Ngay = Ngay;
                            ItemAdd.MaNhanVien = MaNhanVien;
                            ItemAdd.Loai = 0;
                            ItemAdd.SapXep = Col;
                            ItemAdd.IdCaLamViec = CaLamViec.IdGuid;
                            try
                            {

                                try
                                {
                                    DateTime? Gio = dungchung.GetCellToDateTime(sheet1, row, Col);
                                    ItemAdd.TuGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, Gio.Value.Hour, Gio.Value.Minute, 0);
                                    ItemAdd.TuGioUnix = DateTimeToUnixTimeStamp(ItemAdd.TuGio);
                                }
                                catch
                                {
                                    double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                                    int gioVao = 0, phutVao = 0;
                                    dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                                    ItemAdd.TuGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, gioVao, phutVao, 0);
                                    ItemAdd.TuGioUnix = DateTimeToUnixTimeStamp(ItemAdd.TuGio);

                                }
                            }
                            catch
                            {
                            }
                            Col++;
                            sColText = GetCell(sheet1, row, Col);
                            if (!string.IsNullOrEmpty(sColText))
                            {
                                try
                                {
                                    try
                                    {
                                        DateTime? Gio = dungchung.GetCellToDateTime(sheet1, row, Col);
                                        ItemAdd.DenGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, Gio.Value.Hour, Gio.Value.Minute, 0);
                                        ItemAdd.DenGioUnix = DateTimeToUnixTimeStamp(ItemAdd.DenGio);

                                    }
                                    catch
                                    {
                                        double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                                        int gioVao = 0, phutVao = 0;
                                        dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                                        ItemAdd.DenGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, gioVao, phutVao, 0);
                                        ItemAdd.DenGioUnix = DateTimeToUnixTimeStamp(ItemAdd.DenGio);

                                    }

                                }
                                catch
                                {
                                }
                            }
                            //Col++;
                            ItemAdd.SoGioNghi = dungchung.GetCellToDouble(sheet1, row, Col + 1);
                            ItemAdd.UnixToDateTime();
                            if (ItemAdd.TuGio < ItemAdd.DenGio)
                                listNhanVien_ThoiGian.Add(ItemAdd);

                        }
                        else
                            Col++;

                        Col++;
                        Col++;

                    }
                    Col++;
                    // tăng ca
                    MaCa = dungchung.GetCell(sheet1, row, Col);
                    CaLamViec = listCa.FirstOrDefault(a => a.Ma == MaCa);
                    if (CaLamViec == null)
                        CaLamViec = new HRM_dmLichLamViec();
                    Col++;
                    for (int i = 1; i <= 2; i++)
                    {
                        string sColText = GetCell(sheet1, row, Col);

                        if (!string.IsNullOrEmpty(sColText))
                        {
                            HRM_KeHoachLamViecTuan_NhanVien_ThoiGian ItemAdd = new HRM_KeHoachLamViecTuan_NhanVien_ThoiGian();
                            ItemAdd.Ngay = Ngay;
                            ItemAdd.MaNhanVien = MaNhanVien;
                            ItemAdd.Loai = 1;
                            ItemAdd.SapXep = Col;
                            ItemAdd.IdCaLamViec = CaLamViec.IdGuid;
                            try
                            {
                                try
                                {
                                    DateTime? Gio = dungchung.GetCellToDateTime(sheet1, row, Col);
                                    ItemAdd.TuGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, Gio.Value.Hour, Gio.Value.Minute, 0);
                                    ItemAdd.TuGioUnix = DateTimeToUnixTimeStamp(ItemAdd.TuGio);

                                }
                                catch
                                {
                                    double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                                    int gioVao = 0, phutVao = 0;
                                    dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                                    ItemAdd.TuGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, gioVao, phutVao, 0);
                                    ItemAdd.TuGioUnix = DateTimeToUnixTimeStamp(ItemAdd.TuGio);

                                }
                            }
                            catch
                            {
                            }
                            Col++;
                            sColText = GetCell(sheet1, row, Col);
                            if (!string.IsNullOrEmpty(sColText))
                            {
                                try
                                {
                                    try
                                    {
                                        DateTime? Gio = dungchung.GetCellToDateTime(sheet1, row, Col);
                                        ItemAdd.DenGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, Gio.Value.Hour, Gio.Value.Minute, 0);
                                        ItemAdd.DenGioUnix = DateTimeToUnixTimeStamp(ItemAdd.DenGio);

                                    }
                                    catch
                                    {
                                        double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                                        int gioVao = 0, phutVao = 0;
                                        dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                                        ItemAdd.DenGio = new DateTime(ItemAdd.Ngay.Year, ItemAdd.Ngay.Month, ItemAdd.Ngay.Day, gioVao, phutVao, 0);
                                        ItemAdd.DenGioUnix = DateTimeToUnixTimeStamp(ItemAdd.DenGio);

                                    }
                                }
                                catch
                                {
                                }
                            }
                            ItemAdd.SoGioNghi = dungchung.GetCellToDouble(sheet1, row, Col + 1);
                            ItemAdd.UnixToDateTime();
                            if (ItemAdd.TuGio < ItemAdd.DenGio)
                                listNhanVien_ThoiGian.Add(ItemAdd);
                        }
                        else
                            Col++;
                        Col++;
                        Col++;
                    }
                    Col++;
                    Ngay = Ngay.AddDays(1);
                    nCol = nCol + 16;
                }


            }
            if (listNhanVien_ThoiGian != null && listNhanVien_ThoiGian.Count > 0)
            {
                List<string> listIdNhanVien = listNhanVien_ThoiGian.Select(a => a.MaNhanVien).Distinct().ToList();
                var listNhanViendb = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.MaNhanVien)).ToList();
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var MaNv in listIdNhanVien)
                {
                    var item = new HRM_KeHoachLamViecTuan_NhanVienViewModel();
                    var nhanVien = listNhanViendb.FirstOrDefault(a => a.MaNhanVien == MaNv);
                    if (nhanVien != null)
                    {
                        item.IdNhanVien = nhanVien.IdGuid;
                        item.MaNhanVien = nhanVien.MaNhanVien;

                        item.TenNhanVien = nhanVien.TenDayDu;
                        if (listChucVu != null && listChucVu.Count > 0)
                        {
                            var chucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                            if (chucVu != null)
                                item.ChucVu = chucVu.TenChucVu;
                        }
                        //if (listChucDanh != null && listChucDanh.Count > 0)
                        //{
                        //    var chucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                        //    if (chucDanh != null)
                        //        item.TenChucDanh = chucDanh.Ten;
                        //}

                        if (listIdBoPhan.Count > 0 && !listIdBoPhan.Contains(nhanVien.IdBoPhanEOS))
                            continue;
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                        if (BoPhan != null)
                            item.TenBoPhan = BoPhan.TenBoPhan;
                    }
                    item.listWeek = new List<HRM_KeHoachLamViecTuan_ChiTietViewModel>();
                    var data = listNhanVien.Where(a => a.MaNhanVien == item.MaNhanVien).OrderBy(a => a.Ngay).ToList();
                    DateTime time = TuNgay;
                    while (time <= DenNgay)
                    {
                        HRM_KeHoachLamViecTuan_ChiTietViewModel Ngay = new HRM_KeHoachLamViecTuan_ChiTietViewModel();
                        Ngay.Day = time.Day;
                        Ngay.Month = time.Month;
                        Ngay.Year = time.Year;
                        string ThuTA = time.DayOfWeek.ToString();
                        Ngay.TypeRank = (int)time.DayOfWeek;
                        Ngay.Rank = DefineData.QuyDoiThu(Ngay.TypeRank);
                        if (Ngay.TypeRank == 0)
                            Ngay.Mau = "#FF0000";
                        else if (Ngay.TypeRank == 6)
                            Ngay.Mau = "#0000FF";
                        Ngay.listCaChinh = listNhanVien_ThoiGian.Where(a => a.Ngay == time && a.MaNhanVien == item.MaNhanVien && a.Loai == 0).OrderBy(a => a.SapXep).ToList();
                        Ngay.listCaTangCuong = listNhanVien_ThoiGian.Where(a => a.Ngay == time && a.MaNhanVien == item.MaNhanVien && a.Loai == 1).OrderBy(a => a.SapXep).ToList();
                        item.listWeek.Add(Ngay);
                        time = time.AddDays(1);
                    }
                    listResult.Add(item);
                }
            }


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", listResult); ;
        }
        public dynamic ExportKeHoachLamViecTuan(dynamic dyn, ApplicationDbContext db)
        {
            string sFile = "KeHoachTuanPHT.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "Kehoachtuan.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            ExcelWorksheet sheet2 = package.Workbook.Worksheets["Mã ca"];
            HRM_KeHoachLamViecTuanViewModel entity = JsonConvert.DeserializeObject<HRM_KeHoachLamViecTuanViewModel>(dyn.ToString());
            HRM_KeHoachLamViecTuan QuyTrinh = new HRM_KeHoachLamViecTuan(entity);
            QuyTrinh.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVien>();
            var listCa = db.HRM_dmLichLamViec.OrderBy(a => a.Ma).ToList();
            //listCa = listCa.OrderBy(a => a.Ma, new NaturalStringComparer()).ToList();
            if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
            {
                foreach (var item in entity.listNhanVien)
                {
                    HRM_KeHoachLamViecTuan_NhanVien nv = new HRM_KeHoachLamViecTuan_NhanVien(item);
                    nv.listCaChinh = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
                    nv.listCaTangCuong = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
                    if (item.listWeek != null && item.listWeek.Count > 0)
                    {
                        foreach (var child in item.listWeek)
                        {
                            if (child.listCaChinh != null && child.listCaChinh.Count > 0)
                                nv.listCaChinh.AddRange(child.listCaChinh);
                            if (child.listCaTangCuong != null && child.listCaTangCuong.Count > 0)
                                nv.listCaTangCuong.AddRange(child.listCaTangCuong);


                        }
                    }
                    QuyTrinh.listNhanVien.Add(nv);
                }
            }
            QuyTrinh.UnixToDateTime();
            int RowBegin = 6;
            int Row = RowBegin;

            sheet1.Cells[1, 1].Value = "KẾ HOẠCH LÀM VIỆC TỪ NGÀY " + QuyTrinh.TuNgay.Value.ToString("dd/MM/yyyy") + "ĐẾN NGÀY " + QuyTrinh.DenNgay.Value.ToString("dd/MM/yyyy");
            sheet1.Cells[1, 1].Style.Font.Bold = true;
            int STT = 1;
            int sttChild = 1;
            // header
            DateTime time = QuyTrinh.TuNgay ?? DateTime.Now;
            int nCol = 6;
            while (time <= QuyTrinh.DenNgay)
            {
                sheet1.Cells[2, nCol].Value = time.ToString("dd/MM/yyyy");
                sheet1.Cells[3, nCol].Value = QuyDoiThuVT((int)time.DayOfWeek);

                if (QuyTrinh.listNhanVien == null || QuyTrinh.listNhanVien.Count == 0)
                {
                    int ColCa = nCol;
                    int Col = nCol + 1;
                    for (int i = 2; i <= 8; i++)
                    {
                        sheet1.Cells[Row, Col].Formula = "=IFERROR(VLOOKUP(" + sheet1.Cells[Row, ColCa].Address + ",'Mã ca'!D8:L1000," + i + ",0),\"-\")";
                        Col++;
                    }
                    ColCa = Col;
                    Col++;
                    for (int i = 2; i <= 8; i++)
                    {
                        sheet1.Cells[Row, Col].Formula = "=IFERROR(VLOOKUP(" + sheet1.Cells[Row, ColCa].Address + ",'Mã ca'!D8:L1000," + i + ",0),\"-\")";
                        Col++;
                    }


                }
                nCol = nCol + 16;
                time = time.AddDays(1);
            }
            if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
            {
                QuyTrinh.listNhanVien = QuyTrinh.listNhanVien.OrderBy(a => a.Ngay).ToList();

                int CountRow = 8 + listCa.Count();
                string addressKG = sheet2.Cells[CountRow, 9].Address;
                foreach (var item in QuyTrinh.listNhanVien)
                {

                    sttChild = 1;
                    sheet1.Cells[Row, sttChild].Value = STT;
                    sttChild++;
                    sheet1.Cells[Row, sttChild].Value = item.MaNhanVien;
                    sttChild++;
                    sheet1.Cells[Row, sttChild].Value = item.TenNhanVien;
                    sttChild++;
                    sheet1.Cells[Row, sttChild].Value = item.TenBoPhan;
                    sttChild++;
                    sheet1.Cells[Row, sttChild].Value = item.TenChucVu;
                    sttChild++;
                    time = QuyTrinh.TuNgay ?? DateTime.Now;
                    while (time <= QuyTrinh.DenNgay)
                    {
                        nCol = sttChild;
                        var listCaChinh = item.listCaChinh.Where(a => a.Ngay == time).ToList();
                        var listTCa = item.listCaTangCuong.Where(a => a.Ngay == time).ToList();
                        string IdCaLamViec = "";
                        if (listCaChinh != null && listCaChinh.Count > 0)
                            IdCaLamViec = listCaChinh.FirstOrDefault().IdCaLamViec;
                        var CaLV = listCa.Where(a => a.IdGuid == IdCaLamViec).Select(a => a.Ma).FirstOrDefault();
                        if (CaLV != null)
                            sheet1.Cells[Row, nCol].Value = CaLV;
                        nCol++;
                        if (listCaChinh == null || listCaChinh.Count == 0)
                        {
                            int Col = nCol;
                            for (int i = 2; i <= 8; i++)
                            {
                                sheet1.Cells[Row, Col].Formula = "=IFERROR(VLOOKUP(" + sheet1.Cells[Row, nCol - 1].Address + ",'Mã ca'!D8:L1000," + i + ",0),\"-\")";
                                Col++;
                            }
                        }
                        foreach (var ca in listCaChinh)
                        {

                            if (ca.TuGio != null)
                                sheet1.Cells[Row, nCol].Value = ca.TuGio.Value.ToString("HH:mm");
                            nCol++;
                            if (ca.DenGio != null)
                                sheet1.Cells[Row, nCol].Value = ca.DenGio.Value.ToString("HH:mm");
                            nCol++;
                            sheet1.Cells[Row, nCol].Value = ca.SoGioNghi;
                            nCol++;

                        }
                        sttChild = sttChild + 7;
                        if (listCaChinh != null && listCaChinh.Count > 0)
                            sheet1.Cells[Row, sttChild].Value = listCaChinh.Sum(a => (a.DenGio.Value - a.TuGio.Value).TotalHours);
                        sttChild++;
                        nCol = sttChild;
                        if (listTCa != null && listTCa.Count > 0)
                            IdCaLamViec = listTCa.FirstOrDefault().IdCaLamViec;
                        CaLV = listCa.Where(a => a.IdGuid == IdCaLamViec).Select(a => a.Ma).FirstOrDefault();
                        if (CaLV != null)
                            sheet1.Cells[Row, nCol].Value = CaLV;
                        nCol++;
                        if (listTCa == null || listTCa.Count == 0)
                        {
                            int Col = nCol;
                            for (int i = 2; i <= 8; i++)
                            {
                                sheet1.Cells[Row, Col].Formula = "=IFERROR(VLOOKUP(" + sheet1.Cells[Row, nCol - 1].Address + ",'Mã ca'!D8:L1000," + i + ",0),\"-\")";
                                Col++;
                            }
                        }
                        foreach (var ca in listTCa)
                        {
                            if (ca.TuGio != null)
                                sheet1.Cells[Row, nCol].Value = ca.TuGio.Value.ToString("HH:mm");
                            nCol++;
                            if (ca.DenGio != null)
                                sheet1.Cells[Row, nCol].Value = ca.DenGio.Value.ToString("HH:mm");
                            nCol++;
                            sheet1.Cells[Row, nCol].Value = ca.SoGioNghi;
                            nCol++;

                        }
                        sttChild = sttChild + 7;
                        if (listTCa != null && listTCa.Count > 0)
                        {
                            var listTCaFind = listTCa.FindAll(x => x.DenGio != null && x.TuGio != null);
                            sheet1.Cells[Row, sttChild].Value = listTCaFind.Sum(a => (a.DenGio.Value - a.TuGio.Value).TotalHours);
                        }
                        sttChild++;



                        time = time.AddDays(1);

                    }
                    Row++;
                    STT++;
                }
                Row--;
                STT--;
            }

            if (Row > 5)
                dungchung.DrawTableExcel(sheet1, 3, Row, 1, sttChild);

            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet1.Cells[5, 3, Row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            var listId = listCa.Select(a => a.IdGuid).ToList();
            var listKhungGio = _CallWebService.GetDanhSachKhungGioByListIdCaLamViec(db, listId);
            int SoKhungGio = 1;
            foreach (var ca in listCa)
            {
                var khungGio = listKhungGio.Where(a => a.IdCaLamViec == ca.IdGuid).Count();
                if (khungGio > SoKhungGio)
                    SoKhungGio = khungGio;
            }
            int nRow = 6;
            nCol = 5;
            sheet2.Cells[nRow, nCol, nRow, (nCol + (SoKhungGio * 2) - 1)].Merge = true;
            nRow++;
            for (int i = 0; i < SoKhungGio; i++)
            {
                sheet2.Cells[nRow, nCol, nRow, nCol + 2].Merge = true;
                sheet2.Cells[nRow, nCol].Value = "Khung giờ " + (int)(i + 1);
                nCol = nCol + 3;
            }
            sheet2.Cells[nRow - 1, nCol, nRow, nCol].Merge = true;
            sheet2.Cells[nRow - 1, nCol].Value = "Số giờ";
            nRow++;
            STT = 1;
            foreach (var ca in listCa)
            {
                nCol = 2;
                sheet2.Cells[nRow, nCol].Value = STT;
                nCol++;
                sheet2.Cells[nRow, nCol].Value = ca.Ten;
                nCol++;

                sheet2.Cells[nRow, nCol].Value = ca.Ma;
                nCol++;
                int ncolKg = nCol;
                var khungGio = listKhungGio.Where(a => a.IdCaLamViec == ca.IdGuid).ToList();
                if (khungGio != null && khungGio.Count > 0)
                    foreach (var KG in khungGio)
                    {
                        time = UnixTimeStampToDateTime(KG.TuGioUnix);
                        sheet2.Cells[nRow, ncolKg].Value = time.ToString("HH:mm");
                        ncolKg++;
                        time = UnixTimeStampToDateTime(KG.DenGioUnix);
                        sheet2.Cells[nRow, ncolKg].Value = time.ToString("HH:mm");
                        ncolKg++;
                        sheet2.Cells[nRow, ncolKg].Value = KG.SoGioNghi;
                        ncolKg++;


                    }

                nCol = SoKhungGio * 2 + nCol + 2;

                sheet2.Cells[nRow, nCol].Value = ca.SoGioTrongNgay;
                nCol++;
                STT++;
                nRow++;
            }
            sheet2.Cells.Style.Font.Name = "Times New Roman";
            sheet2.Cells.Style.Font.Size = 12;
            sheet2.Cells.Style.WrapText = true;
            sheet2.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet2.Cells[8, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public class Tu_DenNgay
        {
            public DateTime TuNgay { get; set; }
            public DateTime DenNgay { get; set; }
        }
        public dynamic GetTinhNhanhKeHoachLamViecTuan(ApplicationDbContext db, TimKiemKeHoachTuan entity)
        {
            try
            {
                if (entity.listIdNhanVien == null || entity.listIdNhanVien.Count == 0)
                    return entity.listNhanVien;
                List<Tu_DenNgay> listNgayNghi = new List<Tu_DenNgay>();
                var LichNghiLe = db.HRM_dmLichNghiLe_BoPhan.FirstOrDefault(a => entity.listIdBoPhan.Contains(a.IdBoPhan));
                if (LichNghiLe != null)
                {
                    var listlichChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => a.IdLichNghiLe == LichNghiLe.IdLichNghiLe).ToList();
                    if (listlichChiTiet != null && listlichChiTiet.Count > 0)
                    {
                        listNgayNghi = listlichChiTiet.Select(a => new Tu_DenNgay()
                        {
                            TuNgay = a.TuNgay,
                            DenNgay = a.DenNgay
                        }).ToList();
                    }
                }
                // lấy id nhân viên chưa có trong mảng
                List<string> listIdNhanVien = new List<string>();
                if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                {
                    List<string> listIdNV = entity.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    foreach (var item in entity.listIdNhanVien)
                    {
                        if (!listIdNV.Contains(item))
                            listIdNhanVien.Add(item);
                    }
                    entity.listNhanVien = entity.listNhanVien.Where(a => entity.listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                }
                else
                {
                    listIdNhanVien = entity.listIdNhanVien;
                    entity.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVienViewModel>();
                }
                if (listIdNhanVien != null && listIdNhanVien.Count > 0)
                {
                    DateTime dtTuNgay = UnixTimeStampToDateTime(entity.TuNgay);
                    DateTime dtDenNgay = UnixTimeStampToDateTime(entity.DenNgay);
                    dtTuNgay = new DateTime(dtTuNgay.Year, dtTuNgay.Month, dtTuNgay.Day, 0, 0, 0);
                    dtDenNgay = new DateTime(dtDenNgay.Year, dtDenNgay.Month, dtDenNgay.Day, 0, 0, 0);
                    var listCaLamViec = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.Ngay >= dtTuNgay && a.Ngay <= dtDenNgay && listIdNhanVien.Contains(a.IdNhanVien) && a.isTheoDoi == true && string.IsNullOrEmpty(a.IdKeHoachTuan)).OrderBy(a => a.SapXep).ToList();
                    if (listCaLamViec != null && listCaLamViec.Count > 0)
                    {
                        foreach (var item in listCaLamViec)
                            item.DateTimeToUnix();
                    }
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    List<long> listIdNVEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);

                    List<int> Thang = new List<int>();
                    Thang.Add(dtTuNgay.Month);
                    if (dtTuNgay.Month != dtDenNgay.Month)
                        Thang.Add(dtDenNgay.Month);
                    var listNgayNghis = db.HRM_TheoDoiXinNghiPhep.Where(a => Thang.Contains(a.NgayNghi.Month) && listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    if (Nghibu != null)
                        listNgayNghis = listNgayNghis.Where(a => a.IdHinhThucNghi != Nghibu.Id).ToList();
                    foreach (var item in listNhanVien)
                    {
                        HRM_KeHoachLamViecTuan_NhanVienViewModel itemAdd = new HRM_KeHoachLamViecTuan_NhanVienViewModel();
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        if (listNgayNghis != null && listNgayNghis.Count() > 0)
                        {
                            var NgayNghis = listNgayNghis.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                            if (NgayNghis != null && NgayNghis.Count() > 0)
                                itemAdd.SoNgayDaNghi = NgayNghis.Sum(a => a.ThoiGianNghi);
                        }
                        var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (chucVu != null)
                            itemAdd.ChucVu = chucVu.TenChucVu;
                        //var chucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        //if (chucDanh != null)
                        //    itemAdd.TenChucDanh = chucDanh.Ten;

                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        itemAdd.IdBoPhan = item.IdBoPhanEOS;
                        if (BoPhan != null)
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.listWeek = new List<HRM_KeHoachLamViecTuan_ChiTietViewModel>();
                        DateTime time = new DateTime(dtTuNgay.Year, dtTuNgay.Month, dtTuNgay.Day);
                        while (time <= dtDenNgay)
                        {
                            HRM_KeHoachLamViecTuan_ChiTietViewModel ngay = new HRM_KeHoachLamViecTuan_ChiTietViewModel();
                            string ThuTA = time.DayOfWeek.ToString();
                            ngay.TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
                            if (ngay.TypeRank == 0)
                                ngay.Mau = "#FF0000";
                            else if (ngay.TypeRank == 6)
                                ngay.Mau = "#0000FF";
                            if (listNgayNghi.Count > 0)
                            {
                                var ngayCheck = listNgayNghi.FirstOrDefault(a => a.TuNgay <= time && a.DenNgay >= time);
                                if (ngayCheck != null)
                                    ngay.Mau = "#FF0000";
                            }
                            ngay.Rank = DefineData.QuyDoiThu(ngay.TypeRank);
                            if (listCaLamViec != null)
                            {
                                ngay.listIddmCaLamViec = new List<string>();
                                ngay.listCaChinh = listCaLamViec.Where(a => a.Loai == 0 && (int)a.Ngay.DayOfWeek == ngay.TypeRank && a.IdNhanVien == item.IdGuid).OrderBy(a => a.SapXep).ToList();
                                ngay.listCaTangCuong = listCaLamViec.Where(a => a.Loai == 1 && (int)a.Ngay.DayOfWeek == ngay.TypeRank && a.IdNhanVien == item.IdGuid).OrderBy(a => a.SapXep).ToList();

                            }
                            ngay.Day = time.Day;
                            ngay.Month = time.Month;
                            ngay.Year = time.Year;



                            time = time.AddDays(1);
                            itemAdd.listWeek.Add(ngay);
                        }
                        entity.listNhanVien.Add(itemAdd);
                    }
                }
                return entity.listNhanVien;
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        // theo dõi lịch làm việc 
        public dynamic GetTheoDoiLichLamViec(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {

                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                ThoiDoiLichLamViecNhanVienViewModel result = new ThoiDoiLichLamViecNhanVienViewModel();
                if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                {
                    listNhanVien = db.HRM_NhanVien.Where(b => b.TenDayDu.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower())).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0)
                        listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    //else
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tồn tại nhân viên");
                }

                if (TimKiem.IdBoPhanEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhanEOS).ToList();
                    listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();

                }

                var listCaLamViec = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.Ngay >= dtTuNgay && a.Ngay <= dtDenNgay
                                    && (string.IsNullOrEmpty(TimKiem.KeyWord) || (!string.IsNullOrEmpty(TimKiem.KeyWord) && listIdNhanVien.Contains(a.IdNhanVien)))
                                    && a.isTheoDoi == true && string.IsNullOrEmpty(a.IdKeHoachTuan)).OrderBy(a => a.SapXep).ToList();
                if (!string.IsNullOrEmpty(TimKiem.IdUser))
                {
                    listNhanVien = db.HRM_NhanVien.Where(b => b.IdUser == TimKiem.IdUser).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0)
                        listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tồn tại nhân viên");
                    listCaLamViec = listCaLamViec.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                }
                if (listCaLamViec != null && listCaLamViec.Count > 0)
                {
                    foreach (var item in listCaLamViec)
                        item.DateTimeToUnix();
                }

                if (TimKiem.IdCongTyEOS > 0)
                    listCaLamViec = listCaLamViec.Where(a => a.IdCongTy == TimKiem.IdCongTyEOS).ToList();
                result.listLabel = new List<ThoiDoiLichLamViecNhanVien_Label>();
                DateTime time = dtTuNgay;
                while (time <= dtDenNgay)
                {
                    ThoiDoiLichLamViecNhanVien_Label label = new ThoiDoiLichLamViecNhanVien_Label();
                    label.Day = time.Day;
                    label.Month = time.Month;
                    label.Year = time.Year;
                    label.label = time.Day.ToString();
                    int thu = DefineData.QuyDoiThuSangSo(time.DayOfWeek.ToString());
                    label.Thu = DefineData.QuyDoiThuVT(thu);
                    result.listLabel.Add(label);
                    time = time.AddDays(1);
                }
                result.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVienViewModel>();
                var listLamViecTangCa = db.HRM_ThoiGianLamViecKeHoach.Where(a => a.Loai == 1 && a.isLamThemGio == true && a.Ngay >= dtTuNgay && a.Ngay <= dtDenNgay && listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                List<string> listIdCa = listCaLamViec.Where(a => a.IdCaLamViec != null).Select(a => a.IdCaLamViec).Distinct().ToList();
                listIdCa.AddRange(listLamViecTangCa.Where(a => a.IddmCaLamViec != null).Select(a => a.IddmCaLamViec).Distinct().ToList());
                var listCa = db.HRM_dmLichLamViec.Where(a => listIdCa.Contains(a.IdGuid)).ToList();
                foreach (var item in listCaLamViec)
                {
                    var Ca = listCa.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                    if (Ca != null)
                    {
                        item.MaCaLamViec = Ca.Ma;
                        item.TenCaLamViec = Ca.Ten;
                    }
                }
                foreach (var item in listLamViecTangCa)
                {
                    var Ca = listCa.FirstOrDefault(a => a.IdGuid == item.IddmCaLamViec);
                    if (Ca != null)
                    {
                        item.MaCaLamViec = Ca.Ma;
                        item.TenCaLamViec = Ca.Ten;
                    }
                }

                if (listNhanVien.Count == 0)
                {
                    listIdNhanVien = listCaLamViec.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }

                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in listIdNhanVien)
                {
                    HRM_KeHoachLamViecTuan_NhanVienViewModel NhanVienDay = new HRM_KeHoachLamViecTuan_NhanVienViewModel();
                    var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == item);
                    if (NV != null)
                    {
                        NhanVienDay.MaNhanVien = NV.MaNhanVien;
                        NhanVienDay.TenNhanVien = NV.TenDayDu;
                        NhanVienDay.IdNhanVien = item;
                        NhanVienDay.IdUser = NV.IdUser;
                        if (listChucVu != null && listChucVu.Count > 0)
                        {
                            var chucVu = listChucVu.FirstOrDefault(a => a.Id == NV.IdChucVu);
                            if (chucVu != null)
                                NhanVienDay.ChucVu = chucVu.TenChucVu;
                        }
                        if (listBoPhan != null && listBoPhan.Count > 0)
                        {
                            var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NV.IdBoPhanEOS);
                            if (BoPhan != null)
                                NhanVienDay.TenBoPhan = BoPhan.TenBoPhan;
                        }

                    }
                    NhanVienDay.listWeek = new List<HRM_KeHoachLamViecTuan_ChiTietViewModel>();
                    time = new DateTime(dtTuNgay.Year, dtTuNgay.Month, dtTuNgay.Day, 0, 0, 0);
                    while (time <= dtDenNgay)
                    {


                        HRM_KeHoachLamViecTuan_ChiTietViewModel day = new HRM_KeHoachLamViecTuan_ChiTietViewModel();
                        day.Day = time.Day;
                        day.Month = time.Month;
                        day.Year = time.Year;
                        string ThuTA = time.DayOfWeek.ToString();
                        day.TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
                        day.Rank = DefineData.QuyDoiThu(day.TypeRank);
                        if (day.TypeRank == 0)
                            day.Mau = "#FF0000";
                        else if (day.TypeRank == 6)
                            day.Mau = "#0000FF";
                        if (listCaLamViec != null)
                        {
                            day.listCaChinh = listCaLamViec.Where(a => a.Loai == 0 && a.Ngay == time && a.IdNhanVien == NhanVienDay.IdNhanVien).OrderBy(a => a.SapXep).ToList();
                            day.listCaTangCuong = listCaLamViec.Where(a => a.Loai == 1 && a.Ngay == time && a.IdNhanVien == NhanVienDay.IdNhanVien).OrderBy(a => a.SapXep).ToList();
                        }
                        if (listLamViecTangCa != null && listLamViecTangCa.Count > 0)
                        {
                            if (day.listCaTangCuong == null)
                                day.listCaTangCuong = new List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian>();
                            var listNgay = listLamViecTangCa.Where(a => a.IdNhanVien == NhanVienDay.IdNhanVien && a.Ngay == time).ToList();
                            if (listNgay != null && listNgay.Count > 0)
                            {
                                day.listCaTangCuong.AddRange(listNgay.Select(a => new HRM_KeHoachLamViecTuan_NhanVien_ThoiGian
                                {
                                    Ngay = a.Ngay,
                                    TuGio = a.GioVao,
                                    DenGio = a.GioRa,
                                    SoGioNghi = a.SoGioNghi,
                                    MaCaLamViec = a.MaCaLamViec,
                                    TenCaLamViec = a.TenCaLamViec,
                                    IdCongTy = a.IdCongTy,
                                }));
                            }
                        }
                        NhanVienDay.listWeek.Add(day);

                        time = time.AddDays(1);
                    }

                    result.listNhanVien.Add(NhanVienDay);
                }
                result.listNhanVien = result.listNhanVien.OrderBy(a => a.MaNhanVien).ToList();


                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic ExportTheoDoiLichLamViec(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                string sFile = "TheoDoiLichLamViecNhanVien.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "Theodoilichlamviec.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                ThoiDoiLichLamViecNhanVienViewModel result = GetTheoDoiLichLamViec(db, dyn);
                if (result != null)
                {
                    // header
                    int nRow = 3;
                    int nCol = 7;
                    int STT = 1;
                    if (result.listLabel != null && result.listLabel.Count > 0)
                        foreach (var item in result.listLabel)
                        {
                            sheet1.Cells[nRow, nCol].Value = item.Day + "/" + item.Month + "/" + item.Year;
                            sheet1.Cells[nRow + 1, nCol].Value = item.Thu;
                            nCol++;
                        }
                    nCol--;
                    dungchung.DrawTableExcel(sheet1, 3, 4, 1, nCol);
                    sheet1.Cells[1, 1, 1, nCol].Merge = true;
                    sheet1.Cells[1, 1].Value = "THEO DÕI KẾ HOẠCH LÀM VIỆC";
                    sheet1.Cells[1, 1].Style.Font.Bold = true;
                    nRow = 5;
                    if (result.listNhanVien != null && result.listNhanVien.Count > 0)
                        foreach (var item in result.listNhanVien)
                        {
                            nCol = 1;
                            sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                            sheet1.Cells[nRow, nCol].Value = STT;
                            nCol++;
                            sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                            sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                            sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                            sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                            nCol++;
                            sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                            sheet1.Cells[nRow, nCol].Value = item.ChucVu;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = "Ca chính";
                            sheet1.Cells[nRow + 1, nCol].Value = "ca tăng cường";
                            nCol++;
                            int HeightCaChinh = 1;
                            int HeightTC = 1;
                            foreach (var ngay in item.listWeek)
                            {
                                if (ngay.listCaChinh.Count > HeightCaChinh)
                                    HeightCaChinh = ngay.listCaChinh.Count;
                                if (ngay.listCaTangCuong.Count > HeightTC)
                                    HeightTC = ngay.listCaTangCuong.Count;
                                if (ngay.listCaChinh.Count > 0)
                                {
                                    var cell = sheet1.Cells[nRow, nCol];
                                    foreach (var ca in ngay.listCaChinh)
                                    {
                                        var r1 = cell.RichText.Add("- " + ca.TuGio.Value.ToString("HH/mm") + "-" + ca.DenGio.Value.ToString("HH/mm") + "\r\n");
                                        r1.Size = 12;
                                        r1.FontName = "Times New Roman";
                                    }
                                    cell.Style.WrapText = true;
                                }
                                if (ngay.listCaTangCuong.Count > 0)
                                {
                                    var cell = sheet1.Cells[nRow + 1, nCol];
                                    foreach (var ca in ngay.listCaTangCuong)
                                    {
                                        var r1 = cell.RichText.Add("- " + ca.TuGio.Value.ToString("HH/mm") + "-" + ca.DenGio.Value.ToString("HH/mm") + "\r\n");
                                        r1.Size = 12;
                                        r1.FontName = "Times New Roman";
                                    }
                                    cell.Style.WrapText = true;
                                }
                                nCol++;
                            }
                            sheet1.Row(nRow).Height = HeightCaChinh * 15.75;
                            sheet1.Row(nRow + 1).Height = HeightTC * 15.75;
                            nRow++;
                            nRow++;
                            STT++;
                            nCol--;
                        }
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (nRow > 4)
                    {
                        nRow--;
                        dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                        sheet1.Cells[5, 3, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    }


                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetLichTrucMobileApp(ApplicationDbContext db, dynamic dyn)
        {
            //Dùng API GetTheoDoiLichLamViec
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {

                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                ThoiDoiLichLamViecNhanVienViewModel result = new ThoiDoiLichLamViecNhanVienViewModel();
                if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                {
                    listNhanVien = db.HRM_NhanVien.AsNoTracking().Where(b => b.TenDayDu.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower())).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0)
                        listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    else
                    {
                        err.SetLoiKhac("Không tồn tại nhân viên");
                        return err;
                    }
                }
                if (TimKiem.IdBoPhanEOS > 0)
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhanEOS).ToList();
                listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                var listCaLamViec = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AsNoTracking().Where(a => a.Ngay >= dtTuNgay && a.Ngay <= dtDenNgay
                                    && listIdNhanVien.Contains(a.IdNhanVien)
                                    && a.isTheoDoi == true && string.IsNullOrEmpty(a.IdKeHoachTuan)).ToList()
                                    .GroupBy(q => new
                                    {
                                        q.IdNhanVien,
                                        q.Ngay,
                                        q.TuGio,
                                        q.DenGio,
                                        q.IdCaLamViec,
                                        q.Loai,
                                        q.IdCongTy,
                                        q.IdBoPhan
                                    }).Select(q => q.OrderByDescending(x => x.Modified).FirstOrDefault()).ToList();
                if (listCaLamViec != null && listCaLamViec.Count > 0)
                {
                    foreach (var item in listCaLamViec)
                        item.DateTimeToUnix();
                }

                if (TimKiem.IdCongTyEOS > 0)
                    listCaLamViec = listCaLamViec.Where(a => a.IdCongTy == TimKiem.IdCongTyEOS).ToList();
                result.listLabel = new List<ThoiDoiLichLamViecNhanVien_Label>();
                DateTime time = dtTuNgay;
                while (time <= dtDenNgay)
                {
                    ThoiDoiLichLamViecNhanVien_Label label = new ThoiDoiLichLamViecNhanVien_Label();
                    label.Day = time.Day;
                    label.Month = time.Month;
                    label.Year = time.Year;
                    label.label = time.Day.ToString();
                    int thu = DefineData.QuyDoiThuSangSo(time.DayOfWeek.ToString());
                    label.Thu = DefineData.QuyDoiThuVT(thu);
                    result.listLabel.Add(label);
                    time = time.AddDays(1);
                }
                result.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVienViewModel>();
                if (listNhanVien.Count == 0)
                {
                    listIdNhanVien = listCaLamViec.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }

                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in listIdNhanVien)
                {
                    HRM_KeHoachLamViecTuan_NhanVienViewModel NhanVienDay = new HRM_KeHoachLamViecTuan_NhanVienViewModel();
                    var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == item);
                    if (NV != null)
                    {
                        NhanVienDay.MaNhanVien = NV.MaNhanVien;
                        NhanVienDay.TenNhanVien = NV.TenDayDu;
                        NhanVienDay.IdNhanVien = item;
                        NhanVienDay.IdUser = NV.IdUser;
                        if (listChucVu != null && listChucVu.Count > 0)
                        {
                            var chucVu = listChucVu.FirstOrDefault(a => a.Id == NV.IdChucVu);
                            if (chucVu != null)
                                NhanVienDay.ChucVu = chucVu.TenChucVu;
                        }

                        if (listBoPhan != null && listBoPhan.Count > 0)
                        {
                            var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NV.IdBoPhanEOS);
                            if (BoPhan != null)
                                NhanVienDay.TenBoPhan = BoPhan.TenBoPhan;
                        }

                    }
                    NhanVienDay.listWeek = new List<HRM_KeHoachLamViecTuan_ChiTietViewModel>();
                    time = new DateTime(dtTuNgay.Year, dtTuNgay.Month, dtTuNgay.Day, 0, 0, 0);
                    while (time <= dtDenNgay)
                    {


                        HRM_KeHoachLamViecTuan_ChiTietViewModel day = new HRM_KeHoachLamViecTuan_ChiTietViewModel();
                        day.Day = time.Day;
                        day.Month = time.Month;
                        day.Year = time.Year;
                        string ThuTA = time.DayOfWeek.ToString();
                        day.TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
                        day.Rank = DefineData.QuyDoiThu(day.TypeRank);
                        if (day.TypeRank == 0)
                            day.Mau = "#FF0000";
                        else if (day.TypeRank == 6)
                            day.Mau = "#0000FF";
                        if (listCaLamViec != null)
                        {
                            day.listCaChinh = listCaLamViec.Where(a => a.Loai == 0 && a.Ngay == time && a.IdNhanVien == NhanVienDay.IdNhanVien).OrderBy(a => a.SapXep).ToList();
                            day.listCaTangCuong = listCaLamViec.Where(a => a.Loai == 1 && a.Ngay == time && a.IdNhanVien == NhanVienDay.IdNhanVien).OrderBy(a => a.SapXep).ToList();
                            NhanVienDay.listWeek.Add(day);
                        }
                        else
                        {

                            NhanVienDay.listWeek.Add(day);
                        }
                        time = time.AddDays(1);
                    }

                    result.listNhanVien.Add(NhanVienDay);
                }
                result.listNhanVien = result.listNhanVien.OrderBy(a => a.MaNhanVien).ToList();

                err.Data = result;
                return err;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
                return err;
            }
        }
        public dynamic DieuChinhLichTrucMobileApp(ApplicationDbContext db, dynamic dyn)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            //Chỉ thực hiện khi đã có phiếu kế hoạch được duyệt
            //Thay thế lại thời gian ca chính
            HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel data = JsonConvert.DeserializeObject<HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel>(dyn.ToString());
            //if(string.IsNullOrEmpty(data.IdKeHoachLamViecTuan))
            //{
            //    err.SetLoiKhac("Không có kế hoạch làm việc tuần");
            //    return err;
            //}

            if (data.NgayUnix == null)
            {
                err.SetLoiKhac("Không có ngày làm việc");
                return err;
            }

            data.Ngay = dungchung.UnixTimeStampToDateTime(data.NgayUnix.Value).Date;
            var dateToCompare = data.Ngay.Value.Date;
            int year = dateToCompare.Year;
            int month = dateToCompare.Month;
            int day = dateToCompare.Day;
            var listThoiGianCu = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.Ngay.Year == year
                                                                                         && a.Ngay.Month == month
                                                                                         && a.Ngay.Day == day
                                                                                         && a.isTheoDoi == true && a.IdNhanVien == data.IdNhanVien && a.IdCongTy == data.IdCongTy
                                                                                    && a.IdBoPhan == data.IdBoPhan).ToList();
            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.RemoveRange(listThoiGianCu);

            data.listCaChinh.ForEach(q => q.Loai = 0);
            data.listCaTangCuong.ForEach(q => q.Loai = 1);
            var listThoiGianMoi = data.listCaChinh.Union(data.listCaTangCuong);
            int STT = 1;
            foreach (var item in listThoiGianMoi)
            {
                item.Ngay = data.Ngay.Value.Date;
                item.TuGio = dungchung.UnixTimeStampToDateTime(item.TuGioUnix);
                item.DenGio = dungchung.UnixTimeStampToDateTime(item.DenGioUnix);
                item.IdCongTy = data.IdCongTy;
                item.IdBoPhan = data.IdBoPhan;
                item.IdNhanVien = data.IdNhanVien;
                item.isTheoDoi = true;
                item.SapXep = STT++;
                item.IdGuid = Guid.NewGuid().ToString();
            }
            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AddRange(listThoiGianMoi);
            var listKeHoachCu = db.HRM_ThoiGianLamViecKeHoach.Where(a => a.Ngay.Year == year
                                                                                         && a.Ngay.Month == month
                                                                                         && a.Ngay.Day == day
                                                                                        && a.IdNhanVien == data.IdNhanVien
                                                                                        && a.IdCongTy == data.IdCongTy
                                                                                        && a.isLamThemGio == false
                                                                                    ).ToList();
            db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listKeHoachCu);

            List<HRM_ThoiGianLamViecKeHoach> listAdd = new List<HRM_ThoiGianLamViecKeHoach>();
            var listCaLamViec = db.HRM_dmLichLamViec.ToList();
            var nhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == data.IdNhanVien);
            if (listThoiGianMoi != null && listThoiGianMoi.Count() > 0)
                foreach (var itemGio in listThoiGianMoi)
                {

                    var dmLichLamViec = new HRM_dmLichLamViec();
                    if (nhanVien != null)
                    {
                        if (!string.IsNullOrEmpty(itemGio.IdCaLamViec))
                            dmLichLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == itemGio.IdCaLamViec);
                        else
                            dmLichLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == nhanVien.IdCaLamViec);
                        if (dmLichLamViec == null)
                            dmLichLamViec = new HRM_dmLichLamViec();
                    }

                    if (itemGio.TuGio.Value.Year == 1790)
                        continue;
                    if (itemGio.DenGio.Value.Year == 1790)
                        continue;
                    HRM_ThoiGianLamViecKeHoach itemAdd = new HRM_ThoiGianLamViecKeHoach();
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.GioVao = new DateTime(itemGio.Ngay.Year, itemGio.Ngay.Month, itemGio.Ngay.Day, itemGio.TuGio.Value.Hour, itemGio.TuGio.Value.Minute, 0);
                    itemAdd.GioRa = new DateTime(itemGio.Ngay.Year, itemGio.Ngay.Month, itemGio.Ngay.Day, itemGio.DenGio.Value.Hour, itemGio.DenGio.Value.Minute, 0);
                    itemAdd.SoGioNghi = itemGio.SoGioNghi;
                    itemAdd.TongSoGioGiao = (itemAdd.GioRa - itemAdd.GioVao).TotalHours;
                    itemAdd.Ngay = itemGio.Ngay;
                    itemAdd.IdNhanVien = itemGio.IdNhanVien;
                    itemAdd.IddmCaLamViec = dmLichLamViec.IdGuid;
                    itemAdd.isCaGay = dmLichLamViec.isCaGay ?? false;
                    itemAdd.HeSoTangCa = dmLichLamViec.HeSoLuongTangCa ?? 1;
                    itemAdd.NgayCongTieuChuan = dmLichLamViec.SoNgayCongTieuChuan ?? 26;
                    itemAdd.GioTieuChuan = dmLichLamViec.SoGioTrongNgay ?? 8;
                    itemAdd.Loai = itemGio.Loai;
                    itemAdd.IdCongTy = data.IdCongTy;
                    itemAdd.isLamThemGio = false;
                    listAdd.Add(itemAdd);
                }
            db.HRM_ThoiGianLamViecKeHoach.AddRange(listAdd);
            db.SaveChanges();
            return err;
        }
        public dynamic DieuDongLichTrucMobileApp(ApplicationDbContext db, dynamic dyn)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            //Chỉ thực hiện khi đã có phiếu kế hoạch được duyệt
            //Thêm kế hoạch của A trong ngày
            //Thêm A vào phiếu nếu chưa có
            HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel data = JsonConvert.DeserializeObject<HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel>(dyn.ToString());
            //if (string.IsNullOrEmpty(data.IdKeHoachLamViecTuan))
            //{
            //    err.SetLoiKhac("Không có kế hoạch làm việc tuần");
            //    return err;
            //}

            //string IdKeHoachLamViecTuanDieuDongDen = db.HRM_KeHoachLamViecTuan.AsNoTracking().Where(q=>q.isKetThuc == true && q.TuNgay <= data.Ngay && q.DenNgay >= data.Ngay && q.IdCongTy == data.IdCongTyDieuDongDen && q.IdBoPhan == data.IdBoPhanDieuDongDen.ToString()).FirstOrDefault()?.IdGuid;
            //if (string.IsNullOrEmpty(IdKeHoachLamViecTuanDieuDongDen))
            //{
            //    err.SetLoiKhac("Không có kế hoạch làm việc tuần");
            //    return err;
            //}
            if (data.NgayUnix == null)
            {
                err.SetLoiKhac("Không có ngày làm việc");
                return err;
            }
            data.Ngay = dungchung.UnixTimeStampToDateTime(data.NgayUnix.Value).Date;
            var dateToCompare = data.Ngay.Value.Date;
            int year = dateToCompare.Year;
            int month = dateToCompare.Month;
            int day = dateToCompare.Day;
            var listThoiGianCu = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.Ngay.Year == year
                                                                                         && a.Ngay.Month == month
                                                                                         && a.Ngay.Day == day
                                                                                         && a.isTheoDoi == true && a.IdNhanVien == data.IdNhanVien && a.IdCongTy == data.IdCongTy
                                                                                    && a.IdBoPhan == data.IdBoPhan).ToList();
            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.RemoveRange(listThoiGianCu);
            data.listCaChinh.ForEach(q => q.Loai = 0);
            data.listCaTangCuong.ForEach(q => q.Loai = 1);
            var listThoiGianMoi = data.listCaChinh.Union(data.listCaTangCuong);
            int STT = 1;
            foreach (var item in listThoiGianMoi)
            {
                item.Ngay = data.Ngay.Value.Date;
                item.IdCongTy = data.IdCongTyDieuDongDen;
                item.IdBoPhan = data.IdBoPhanDieuDongDen;
                item.IdNhanVien = data.IdNhanVien;
                item.isTheoDoi = true;
                item.SapXep = STT++;
                item.TuGio = dungchung.UnixTimeStampToDateTime(item.TuGioUnix);
                item.DenGio = dungchung.UnixTimeStampToDateTime(item.DenGioUnix);
                item.IdGuid = Guid.NewGuid().ToString();
            }
            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AddRange(listThoiGianMoi);
            var listKeHoachCu = db.HRM_ThoiGianLamViecKeHoach.Where(a => a.Ngay.Year == year
                                                                                        && a.Ngay.Month == month
                                                                                        && a.Ngay.Day == day
                                                                                       && a.IdNhanVien == data.IdNhanVien
                                                                                   ).ToList();
            db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listKeHoachCu);

            List<HRM_ThoiGianLamViecKeHoach> listAdd = new List<HRM_ThoiGianLamViecKeHoach>();
            var listCaLamViec = db.HRM_dmLichLamViec.ToList();
            var nhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == data.IdNhanVien);
            if (listThoiGianMoi != null && listThoiGianMoi.Count() > 0)
                foreach (var itemGio in listThoiGianMoi)
                {

                    var dmLichLamViec = new HRM_dmLichLamViec();
                    if (nhanVien != null)
                    {
                        if (!string.IsNullOrEmpty(itemGio.IdCaLamViec))
                            dmLichLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == itemGio.IdCaLamViec);
                        else
                            dmLichLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == nhanVien.IdCaLamViec);
                        if (dmLichLamViec == null)
                            dmLichLamViec = new HRM_dmLichLamViec();
                    }

                    if (itemGio.TuGio.Value.Year == 1790)
                        continue;
                    if (itemGio.DenGio.Value.Year == 1790)
                        continue;
                    HRM_ThoiGianLamViecKeHoach itemAdd = new HRM_ThoiGianLamViecKeHoach();
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.GioVao = new DateTime(itemGio.Ngay.Year, itemGio.Ngay.Month, itemGio.Ngay.Day, itemGio.TuGio.Value.Hour, itemGio.TuGio.Value.Minute, 0);
                    itemAdd.GioRa = new DateTime(itemGio.Ngay.Year, itemGio.Ngay.Month, itemGio.Ngay.Day, itemGio.DenGio.Value.Hour, itemGio.DenGio.Value.Minute, 0);
                    itemAdd.SoGioNghi = itemGio.SoGioNghi;
                    itemAdd.TongSoGioGiao = (itemAdd.GioRa - itemAdd.GioVao).TotalHours;
                    itemAdd.Ngay = itemGio.Ngay;
                    itemAdd.IdNhanVien = itemGio.IdNhanVien;
                    itemAdd.IddmCaLamViec = dmLichLamViec.IdGuid;
                    itemAdd.isCaGay = dmLichLamViec.isCaGay ?? false;
                    itemAdd.HeSoTangCa = dmLichLamViec.HeSoLuongTangCa ?? 1;
                    itemAdd.NgayCongTieuChuan = dmLichLamViec.SoNgayCongTieuChuan ?? 26;
                    itemAdd.GioTieuChuan = dmLichLamViec.SoGioTrongNgay ?? 8;
                    itemAdd.Loai = itemGio.Loai;
                    itemAdd.IdCongTy = data.IdCongTy;

                    itemAdd.isLamThemGio = false;
                    listAdd.Add(itemAdd);
                }
            db.HRM_ThoiGianLamViecKeHoach.AddRange(listAdd);
            db.SaveChanges();
            return err;
        }
        public dynamic ThayTheLichTrucMobileApp(ApplicationDbContext db, dynamic dyn)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            //Thay thế toàn bộ kế hoạch công việc của A=>B trong ngày
            HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel data = JsonConvert.DeserializeObject<HRM_KeHoachLamViecTuan_DieuChinhKeHoachViewModel>(dyn.ToString());

            //if (string.IsNullOrEmpty(data.IdKeHoachLamViecTuan))
            //{
            //    err.SetLoiKhac("Không có kế hoạch làm việc tuần");
            //    return err;
            //}
            if (data.NgayUnix == null)
            {
                err.SetLoiKhac("Không có ngày làm việc");
                return err;
            }
            data.Ngay = dungchung.UnixTimeStampToDateTime(data.NgayUnix.Value).Date;
            var dateToCompare = data.Ngay.Value.Date;
            int year = dateToCompare.Year;
            int month = dateToCompare.Month;
            int day = dateToCompare.Day;
            var listThoiGianCu = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.Ngay.Year == year
                                                                                         && a.Ngay.Month == month
                                                                                         && a.Ngay.Day == day
                                                                                         && a.isTheoDoi == true && a.IdNhanVien == data.IdNhanVien && a.IdCongTy == data.IdCongTy
                                                                                    && a.IdBoPhan == data.IdBoPhan).ToList();
            var listThoiGianCu2 = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(a => a.Ngay.Year == year
                                                                                         && a.Ngay.Month == month
                                                                                         && a.Ngay.Day == day
                                                                                         && a.isTheoDoi == true && a.IdNhanVien == data.IdNhanVienThayThe && a.IdCongTy == data.IdCongTy
                                                                                    && a.IdBoPhan == data.IdBoPhan).ToList();

            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.RemoveRange(listThoiGianCu);
            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.RemoveRange(listThoiGianCu2);

            //if (itemNhanVienThayThe == null)
            //{
            //    itemNhanVienThayThe = new HRM_KeHoachLamViecTuan_NhanVien();
            //    itemNhanVienThayThe.IdGuid = Guid.NewGuid().ToString();
            //    itemNhanVienThayThe.IdKeHoachLamViecTuan = data.IdKeHoachLamViecTuan;
            //    itemNhanVienThayThe.IdNhanVien = data.IdNhanVienThayThe;
            //    itemNhanVienThayThe.IdBoPhan = data.IdBoPhan;
            //    db.HRM_KeHoachLamViecTuan_NhanVien.Add(itemNhanVienThayThe);
            //}
            //else
            //{
            //    itemNhanVienThayThe.IdBoPhan = data.IdBoPhan;
            //    db.Entry(itemNhanVienThayThe).State = EntityState.Modified;
            //}

            data.listCaChinh.ForEach(q => q.Loai = 0);
            data.listCaTangCuong.ForEach(q => q.Loai = 1);
            var listThoiGianMoi = data.listCaChinh.Union(data.listCaTangCuong);
            int STT = 1;
            foreach (var item in listThoiGianMoi)
            {
                item.Ngay = data.Ngay.Value.Date;
                item.IdCongTy = data.IdCongTy;
                item.IdBoPhan = data.IdBoPhan;
                item.IdNhanVien = data.IdNhanVienThayThe;
                //item.IdKeHoachTuan = data.IdKeHoachLamViecTuan;
                item.isTheoDoi = true;
                item.SapXep = STT++;
                //if (string.IsNullOrEmpty(item.IdGuid))
                //{
                //    item.IdGuid = Guid.NewGuid().ToString();
                //}
                item.IdGuid = Guid.NewGuid().ToString();
            }
            db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.AddRange(listThoiGianMoi);

            db.SaveChanges();
            return err;
        }
        public dynamic GetDanhSachNhanVienByIdCongTy(ApplicationDbContext db, long IdCongTy)
        {
            //var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            //var listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
            //return db.HRM_NhanVien.AsNoTracking().Where(a => a.IdCongTyEOS == IdCongTy && listIdUser.Contains(a.IdUser)).Select(a => new { IdNhanVien = a.IdGuid, TenNhanVien = a.TenDayDu, MaNhanVien = a.MaNhanVien }).ToList();
            return db.HRM_NhanVien.AsNoTracking().Where(a => a.IdCongTyEOS == IdCongTy).Select(a => new { IdNhanVien = a.IdGuid, TenNhanVien = a.TenDayDu, MaNhanVien = a.MaNhanVien }).ToList();
        }
        #endregion
        #region Xác nhận đi muộn về sớm trưởng bộ phận
        public dynamic GetNextSoQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "XNDMVS",
                db.HRM_XacNhanDiMuonVeSom.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_XacNhanDiMuonVeSom> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_XacNhanDiMuonVeSom
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_XacNhanDiMuonVeSom
                             orderby a.Created descending
                             select a;

                //var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                //if (listCongTy != null)
                //{
                //    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                //    result = result.Where(a => listId.Contains(a.IdCongTy));
                //}
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.TuNgay >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.DenNgay < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (string.IsNullOrEmpty(a.NoiDung) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));

                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_XacNhanDiMuonVeSom" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                if (itemList.Count > 0)
                {
                    List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                    List<BoPhan> listCongTy = _CallWebService.GetDanhSachCongTy(db);
                    List<string> listId = itemList.Select(a => a.IdGuid).Distinct().ToList();
                    var listItem = db.HRM_XacNhanDiMuonVeSom_NhanVien.Where(a => listId.Contains(a.IdXacNhanDiMuonVeSom)).ToList();
                    var listIdNv = listItem.Select(a => a.IdNhanVien).Distinct().ToList();
                    List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();
                    foreach (var item in listItem)
                    {
                        var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        item.MaNhanVien = NhanVien.MaNhanVien;
                        item.TenNhanVien = NhanVien.TenDayDu;
                    }
                    foreach (HRM_XacNhanDiMuonVeSom item in itemList)
                    {
                        item.DateTimeToUnix();
                        BoPhan duAn = listCongTy.Find(x => x.Id == item.IdCongTy);
                        if (duAn != null)
                            item.TenCongty = duAn.TenBoPhan;
                        item.listIdBoPhan = new List<long>();
                        if (item.IdBoPhan != null)
                        {
                            if (item.IdBoPhan.Contains(","))
                            {
                                string[] list = item.IdBoPhan.Split(',');
                                foreach (var bp in list)
                                    item.listIdBoPhan.Add(long.Parse(bp));
                            }
                            else
                                item.listIdBoPhan.Add(long.Parse(item.IdBoPhan));
                        }
                        List<BoPhan> BoPhan = BoPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                        if (BoPhan != null)
                            item.TenBoPhan = string.Join(",", BoPhan.Select(a => a.TenBoPhan));
                        item.listNhanVien = listItem.Where(a => a.IdXacNhanDiMuonVeSom == item.IdGuid).ToList();
                        if (item.listNhanVien != null && item.listNhanVien.Count > 0)
                        {
                            item.listNoiDung = item.listNhanVien.Select(a => (a.TenNhanVien + "(" + a.MaNhanVien + ") " + a.SoPhut + "/" + a.SoPhut)).ToList();
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
        public dynamic KhoiTaoQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, double TuNgayUnix, double DenNgayUnix)
        {
            try
            {
                DateTime _TuNgay = UnixTimeStampToDateTime(TuNgayUnix);
                DateTime TuNgay = new DateTime(_TuNgay.Year, _TuNgay.Month, _TuNgay.Day, 0, 0, 0);
                DateTime _DenNgay = UnixTimeStampToDateTime(DenNgayUnix).AddDays(1);
                DateTime DenNgay = new DateTime(_DenNgay.Year, _DenNgay.Month, _DenNgay.Day, 0, 0, 0);
                HRM_XacNhanDiMuonVeSomViewModel QuyTrinh = new HRM_XacNhanDiMuonVeSomViewModel();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => (IdCongTy == 0 || (IdCongTy > 0 && x.IdCongTyEOS == IdCongTy))).ToList();
                //List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                //var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                if (listIdBoPhan != null && listIdBoPhan.Count != 0)
                {
                    listNhanVien = listNhanVien.Where(x => listIdBoPhan.Contains(x.IdBoPhanEOS)).ToList();
                }
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                                 && x.Ngay >= TuNgay && x.Ngay <= DenNgay).ToList();
                List<HRM_CheckInOut> listCheckInOut = db.HRM_CheckInOut.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                    && x.Ngay >= TuNgay && x.Ngay <= DenNgay).ToList();
                List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayDangKy >= TuNgay && x.NgayDangKy <= DenNgay).ToList();
                List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                               && x.NgayDangKy >= TuNgay && x.NgayDangKy <= DenNgay).ToList();
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayNghi >= TuNgay && x.NgayNghi <= DenNgay).ToList();
                var DuAn = _CallWebService.GetQuyTrinhDuAn(IdCongTy, db);
                if (DuAn == null)
                    DuAn = new BoPhan();
                string VeSom = HrmOptions.SoPhutSom.ToString();
                double SoPhutDuocVeSom = double.Parse(VeSom);
                string DiMuon = HrmOptions.SoPhutMuon.ToString();
                double SoPhutDuocDiMuon = double.Parse(DiMuon);
                if (DuAn.MaBoPhan == "VPCT")
                {
                    SoPhutDuocDiMuon = 0;
                    SoPhutDuocVeSom = 0;
                }

                List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
                QuyTrinh.listNhanVien = new List<HRM_XacNhanDiMuonVeSom_NhanVienViewModel>();
                List<DateTime> listDate = new List<DateTime>();
                var listCaLamViecs = db.HRM_dmLichLamViec.ToList();
                foreach (var item in listNhanVien)
                {
                    var itemAdd = new HRM_XacNhanDiMuonVeSom_NhanVienViewModel();
                    itemAdd.IdNhanVien = item.IdGuid;
                    itemAdd.MaNhanVien = item.MaNhanVien;
                    itemAdd.TenNhanVien = item.TenDayDu;
                    itemAdd.IdBoPhan = item.IdBoPhanEOS;
                    if (listChucDanh != null && listChucDanh.Count > 0)
                    {
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                        {
                            itemAdd.TenChucDanh = ChucDanh.Ten;
                        }
                    }
                    if (listBoPhan != null && listBoPhan.Count > 0)
                    {
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                    }


                    bool isHave = false;
                    itemAdd.listWeek = new List<HRM_XacNhanDiMuonVeSom_ChiTietViewModel>();
                    var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                    for (DateTime dt = TuNgay; dt < DenNgay; dt = dt.AddDays(1))
                    {

                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                        if (listLichLamViecFind == null)
                            listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();
                        if (dt.DayOfWeek != 0)
                        {
                            var tg = new HRM_ThoiGianLamViecKeHoach
                            {
                                Loai = 0,
                                IdNhanVien = item.IdGuid,
                                Ngay = dt,
                                GioVao = new DateTime(dt.Year, dt.Month, dt.Day, CaLamViec.TuGio.Hour, CaLamViec.TuGio.Minute, CaLamViec.TuGio.Second),
                                GioRa = new DateTime(dt.Year, dt.Month, dt.Day, CaLamViec.DenGio.Hour, CaLamViec.DenGio.Minute, CaLamViec.DenGio.Second),
                                isCaGay = false,
                            };
                            if (CaLamViec.DenGio.Hour < CaLamViec.TuGio.Hour)
                            {
                                DateTime time = dt.AddDays(1);
                                tg.GioRa = new DateTime(time.Year, time.Month, time.Day, CaLamViec.DenGio.Hour, CaLamViec.DenGio.Minute, CaLamViec.DenGio.Second);
                            }
                            listLichLamViecFind.Add(tg);
                        }
                        List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt);
                        List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);
                        if (listCheckInOutFind != null && listCheckInOutFind.Count > 0)
                        {
                            List<clsDiMuonVeSom> listSomMuon = TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, DuAn.MaBoPhan, NghiPheps, true);
                            if (listSomMuon.Count == 0)
                                continue;
                            isHave = true;
                            var itemWeek = new HRM_XacNhanDiMuonVeSom_ChiTietViewModel(dt, dmKyHieuChamCongs, listSomMuon);
                            itemAdd.listWeek.Add(itemWeek);
                            listDate.Add(dt);
                        }
                    }

                    if (isHave)
                        QuyTrinh.listNhanVien.Add(itemAdd);
                }
                if (listDate.Count > 0)
                {
                    listDate = listDate.Distinct().ToList();
                    foreach (var item in QuyTrinh.listNhanVien)
                    {
                        foreach (DateTime dtNgay in listDate)
                        {
                            if (item.listWeek != null && item.listWeek.Count > 0)
                            {
                                var check = item.listWeek.FirstOrDefault(a => a.Day == dtNgay.Day && a.Month == dtNgay.Month && a.Year == dtNgay.Year);
                                if (check == null)
                                {
                                    var itemWeek = new HRM_XacNhanDiMuonVeSom_ChiTietViewModel();
                                    itemWeek.Month = dtNgay.Month;
                                    itemWeek.Day = dtNgay.Day;
                                    itemWeek.Year = dtNgay.Year;
                                    itemWeek.Ngay = dtNgay;
                                    //SoPhut = a.SoPhut;
                                    string ThuTA = dtNgay.DayOfWeek.ToString();
                                    itemWeek.TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
                                    itemWeek.Rank = DefineData.QuyDoiThu(itemWeek.TypeRank);
                                    item.listWeek.Add(itemWeek);
                                }
                            }
                            else
                            {
                                var itemWeek = new HRM_XacNhanDiMuonVeSom_ChiTietViewModel();
                                itemWeek.Month = dtNgay.Month;
                                itemWeek.Day = dtNgay.Day;
                                itemWeek.Year = dtNgay.Year;
                                itemWeek.Ngay = dtNgay;
                                //SoPhut = a.SoPhut;
                                string ThuTA = dtNgay.DayOfWeek.ToString();
                                itemWeek.TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
                                itemWeek.Rank = DefineData.QuyDoiThu(itemWeek.TypeRank);
                                item.listWeek.Add(itemWeek);
                            }
                        }
                        item.listWeek = item.listWeek.OrderBy(a => a.Ngay).ToList();
                    }
                }
                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_XacNhanDiMuonVeSom QuyTrinh = db.HRM_XacNhanDiMuonVeSom.FirstOrDefault(a => a.IdGuid == Id);
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                QuyTrinh.DateTimeToUnix();
                HRM_XacNhanDiMuonVeSomViewModel result = new HRM_XacNhanDiMuonVeSomViewModel(QuyTrinh);
                result.listNhanVien = new List<HRM_XacNhanDiMuonVeSom_NhanVienViewModel>();
                var ListUser = db.HRM_XacNhanDiMuonVeSom_NhanVien.Where(r => r.IdXacNhanDiMuonVeSom == Id).ToList();
                if (ListUser != null && ListUser.Count > 0)
                {
                    List<string> listIdNhanVien = ListUser.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    List<long> listIdNVEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    var dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
                    //result.listNhanVien = ListUser.Select(a => new HRM_XacNhanDiMuonVeSom_NhanVienViewModel(a)).ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    foreach (var IdNv in listIdNhanVien)
                    {
                        var item = new HRM_XacNhanDiMuonVeSom_NhanVienViewModel();
                        item.IdNhanVien = IdNv;
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        if (nhanVien != null)
                        {
                            item.MaNhanVien = nhanVien.MaNhanVien;
                            item.TenNhanVien = nhanVien.TenDayDu;
                            if (listChucVu != null && listChucVu.Count > 0)
                            {
                                var chucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                                if (chucVu != null)
                                    item.ChucVu = chucVu.TenChucVu;
                            }
                            if (listChucDanh != null && listChucDanh.Count > 0)
                            {
                                var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                                if (ChucDanh != null)
                                    item.TenChucDanh = ChucDanh.Ten;
                            }
                            if (listBoPhan != null && listBoPhan.Count > 0)
                            {
                                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                                if (BoPhan != null)
                                    item.TenBoPhan = BoPhan.TenBoPhan;
                            }

                        }
                        item.listWeek = new List<HRM_XacNhanDiMuonVeSom_ChiTietViewModel>();
                        var data = ListUser.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Ngay).ToList();
                        List<DateTime> listDate = ListUser.Select(a => a.Ngay).Distinct().OrderBy(a => a).ToList();
                        foreach (var date in listDate)
                        {
                            HRM_XacNhanDiMuonVeSom_ChiTietViewModel Ngay = new HRM_XacNhanDiMuonVeSom_ChiTietViewModel();
                            Ngay.listChiTiet = new List<HRM_XacNhanDiMuonVeSom_NhanVien>();
                            Ngay.Month = date.Month;
                            Ngay.Day = date.Day;
                            Ngay.Year = date.Year;
                            Ngay.Ngay = date;
                            string ThuTA = date.DayOfWeek.ToString();
                            Ngay.TypeRank = DefineData.QuyDoiThuSangSo(ThuTA);
                            Ngay.Rank = DefineData.QuyDoiThu(Ngay.TypeRank);
                            Ngay.listChiTiet = data.Where(a => a.Ngay == date).ToList();
                            if (Ngay.listChiTiet != null && Ngay.listChiTiet.Count > 0)
                                foreach (var CT in Ngay.listChiTiet)
                                {
                                    CT.DateTimeToUnix();
                                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.IdGuid == CT.IddmKyHieuChamCong);
                                    if (dmKyHieuChamCong != null)
                                    {
                                        double SoPhut = CT.SoPhut - CT.SoPhutChapNhan;
                                        if (SoPhut == 0)
                                        {
                                            Ngay.IddmKyHieuChamCong += dmKyHieuChamCong.Ma + "; ";
                                            CT.NoiDung = dmKyHieuChamCong.Ten;
                                        }
                                        else
                                        {
                                            Ngay.IddmKyHieuChamCong += dmKyHieuChamCong.Ma + ":" + SoPhut + "; ";
                                            CT.NoiDung = dmKyHieuChamCong.Ten;
                                        }
                                        CT.KyHieuChamCong = dmKyHieuChamCong.Ma;
                                    }
                                }
                            item.listWeek.Add(Ngay);
                        }
                        //item.listWeek = data.Select(a => new HRM_XacNhanDiMuonVeSom_ChiTietViewModel(a, listNgayNghi)).ToList();
                        result.listNhanVien.Add(item);

                    }
                }
                if (!base.GetQuyTrinh(db, _CallWebService, result, err, true))
                    return err;
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic SetQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_XacNhanDiMuonVeSomViewModel QuyTrinh = JsonConvert.DeserializeObject<HRM_XacNhanDiMuonVeSomViewModel>(data.ToString());

                    var QuyTrinhdb = db.HRM_XacNhanDiMuonVeSom.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, QuyTrinh, err))
                        return err;

                    MessageReturn check = UpdateQuyTrinhXacNhanDiMuonVeSom(QuyTrinh, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check.sError);
                    }

                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, QuyTrinh, err))
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
        public dynamic UpdateQuyTrinhXacNhanDiMuonVeSom(HRM_XacNhanDiMuonVeSomViewModel entity, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {

                HRM_XacNhanDiMuonVeSom QuyTrinh = new HRM_XacNhanDiMuonVeSom(entity);
                QuyTrinh.listNhanVien = new List<HRM_XacNhanDiMuonVeSom_NhanVien>();
                if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                {
                    foreach (var item in entity.listNhanVien)
                    {
                        if (item.listWeek != null && item.listWeek.Count > 0)
                        {
                            foreach (var child in item.listWeek)
                            {
                                //HRM_XacNhanDiMuonVeSom_NhanVien nv = new HRM_XacNhanDiMuonVeSom_NhanVien(item, child);
                                if (child.listChiTiet != null && child.listChiTiet.Count > 0)
                                    QuyTrinh.listNhanVien.AddRange(child.listChiTiet);
                            }
                        }
                    }
                }

                QuyTrinh.UnixToDateTime();
                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.IdDuAn = QuyTrinh.IdCongTy;
                    QuyTrinh.eAction = DefineData.eAction.XACNHANDIMUONVESOM.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return err;

                    db.HRM_XacNhanDiMuonVeSom.Add(QuyTrinh);
                }

                else
                {
                    int Check = db.HRM_XacNhanDiMuonVeSom.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh && r.IdGuid != QuyTrinh.IdGuid).AsNoTracking().Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhXacNhanDiMuonVeSom(db).SoPhieu.ToString();
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListUser = db.HRM_XacNhanDiMuonVeSom_NhanVien.Where(r => r.IdXacNhanDiMuonVeSom == QuyTrinh.IdGuid).ToList();
                db.HRM_XacNhanDiMuonVeSom_NhanVien.RemoveRange(ListUser);
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
                        bp.TableName = nameof(HRM_XacNhanDiMuonVeSom);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                {
                    foreach (var item in QuyTrinh.listNhanVien)
                    {

                        item.UnixToDateTime();
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdXacNhanDiMuonVeSom = QuyTrinh.IdGuid;
                        item.IdBoPhan = item.IdBoPhan;
                        item.IdCongTy = QuyTrinh.IdCongTy;
                    }
                    db.HRM_XacNhanDiMuonVeSom_NhanVien.AddRange(QuyTrinh.listNhanVien);
                }
                if (QuyTrinh.isKetThuc == true)
                {
                    if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                    {
                        DateTime TuNgay = QuyTrinh.listNhanVien.OrderBy(a => a.Ngay).FirstOrDefault().Ngay;
                        DateTime DenNgay = QuyTrinh.listNhanVien.OrderByDescending(a => a.Ngay).FirstOrDefault().Ngay;
                        List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                        var listDLdb = db.HRM_TheoDoiDiMuonVeSom.Where(a => TuNgay <= a.NgayDangKy && DenNgay >= a.NgayDangKy && listIdNhanVien.Contains(a.IdNhanVien)).ToList();

                        List<HRM_TheoDoiDiMuonVeSom> listAdd = new List<HRM_TheoDoiDiMuonVeSom>();
                        List<HRM_TheoDoiDiMuonVeSom> listDelete = new List<HRM_TheoDoiDiMuonVeSom>();
                        foreach (var item in QuyTrinh.listNhanVien)
                        {
                            HRM_TheoDoiDiMuonVeSom itemAdd = new HRM_TheoDoiDiMuonVeSom(item);
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            listAdd.Add(itemAdd);
                            var Checkdb = listDelete.FirstOrDefault(a => a.NgayDangKy == item.Ngay);
                            if (Checkdb != null)
                            {
                                listDelete.Add(Checkdb);
                            }
                        }
                        if (listAdd != null && listAdd.Count > 0)
                            db.HRM_TheoDoiDiMuonVeSom.AddRange(listAdd);
                        if (listDelete != null && listDelete.Count > 0)
                            db.HRM_TheoDoiDiMuonVeSom.RemoveRange(listDelete);
                    }
                }
                db.SaveChanges();
                if (!base.SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return err.Value;
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công", entity);

            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }

        }
        public dynamic DeleteQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, string Id)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_XacNhanDiMuonVeSom QuyTrinh = db.HRM_XacNhanDiMuonVeSom.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_XacNhanDiMuonVeSom_NhanVien.Where(r => r.IdXacNhanDiMuonVeSom == Id).ToList();
                    db.HRM_XacNhanDiMuonVeSom_NhanVien.RemoveRange(ListUser);
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
        public dynamic ChuyenTiepQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_XacNhanDiMuonVeSomViewModel QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_XacNhanDiMuonVeSomViewModel)) as HRM_XacNhanDiMuonVeSomViewModel;

                    HRM_XacNhanDiMuonVeSom pcuDatabase = db.HRM_XacNhanDiMuonVeSom.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    MessageReturn Error = UpdateQuyTrinhXacNhanDiMuonVeSom(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.sError, QuyTrinh.IdGuid);
                    }
                    if (!base.ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }


                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
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
        public dynamic KhongDuyetQuyTrinhXacNhanDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_XacNhanDiMuonVeSomViewModel QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_XacNhanDiMuonVeSomViewModel)) as HRM_XacNhanDiMuonVeSomViewModel;

                    HRM_XacNhanDiMuonVeSom pcuDatabase = db.HRM_XacNhanDiMuonVeSom.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    MessageReturn Error = UpdateQuyTrinhXacNhanDiMuonVeSom(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.Value, QuyTrinh.IdGuid);
                    }

                    if (!base.KhongDuyetSauHamUpdate(db, QuyTrinh))
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
        public dynamic ExportQuyTrinhXacNhanDiMuonVeSom(dynamic dyn, ApplicationDbContext db)
        {
            string sFile = "XacNhanDiMuonVeSom.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = sFile;
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            ExcelWorksheet sheet2 = package.Workbook.Worksheets["Nhân viên"];
            HRM_XacNhanDiMuonVeSomViewModel entity = JsonConvert.DeserializeObject<HRM_XacNhanDiMuonVeSomViewModel>(dyn.ToString());
            if (entity != null)
            {
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);

                //sheet quy trình
                var congTy = listBoPhan.FirstOrDefault(a => a.Id == entity.IdCongTy);
                if (congTy != null)
                    sheet1.Cells[3, 2].Value = congTy.TenBoPhan;
                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == entity.IdBoPhan);
                if (BoPhan != null)
                    sheet1.Cells[3, 3].Value = BoPhan.TenBoPhan;
                if (entity.TuNgayUnix > 0)
                    sheet1.Cells[3, 4].Value = dungchung.UnixTimeStampToDateTime(entity.TuNgayUnix).ToString("dd/MM/yyyy");
                if (entity.DenNgayUnix > 0)
                    sheet1.Cells[3, 5].Value = dungchung.UnixTimeStampToDateTime(entity.DenNgayUnix).ToString("dd/MM/yyyy");
                sheet1.Cells[3, 6].Value = entity.NoiDung;
                sheet1.Cells[3, 7].Value = entity.GhiChu;
                dungchung.DrawTableExcel(sheet1, 2, 3, 1, 7);
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[3, 2, 3, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[3, 6, 3, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                // sheet nhân viên
                if (entity.listNhanVien != null)
                {
                    int RowBegin = 4;
                    int nRow = RowBegin;
                    int STT = 1;
                    int nCol = 5;
                    // header
                    foreach (var item in entity.listNhanVien.FirstOrDefault().listWeek.OrderBy(a => a.Ngay))
                    {
                        sheet2.Cells[2, nCol].Value = item.Ngay.ToString("dd/MM/yyyy");
                        sheet2.Cells[3, nCol].Value = item.Rank;
                        nCol++;
                    }

                    foreach (var item in entity.listNhanVien)
                    {
                        nCol = 1;
                        sheet2.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet2.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet2.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet2.Cells[nRow, nCol].Value = item.TenChucDanh;
                        nCol++;
                        sheet2.Row(nRow).Height = 3 * 16.5;
                        foreach (var ngay in item.listWeek)
                        {
                            var cell = sheet2.Cells[nRow, nCol];
                            if (ngay.listChiTiet != null)
                            {
                                foreach (var nd in ngay.listChiTiet)
                                {
                                    var r1 = cell.RichText.Add(" - Nội dung: " + nd.NoiDung + "\r\n");
                                    r1.FontName = "Times New Roman";
                                    r1.Size = 12;
                                    var r2 = cell.RichText.Add(" + Tổng số phút: " + nd.SoPhut + "\r\n");
                                    var r3 = cell.RichText.Add(" + Số phút chấp nhận: " + nd.SoPhutChapNhan + "\r\n");
                                }
                            }
                            nCol++;
                            cell.Style.WrapText = true;
                        }

                        STT++;
                        nRow++;

                    }
                    if (nRow > 4)
                    {
                        nRow--;
                        nCol--;
                    }
                    sheet2.Cells[1, 1].Value = "DANH SÁCH PHÊ DUYỆT ĐI MUỘN VỀ SỚM";
                    sheet2.Cells[1, 1].Style.Font.Bold = true;
                    sheet2.Cells[1, 1, 1, nCol].Merge = true;

                    dungchung.DrawTableExcel(sheet2, 2, nRow, 1, nCol);
                    sheet2.Cells.Style.Font.Name = "Times New Roman";
                    sheet2.Cells.Style.Font.Size = 12;
                    sheet2.Cells.Style.WrapText = true;
                    sheet2.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet2.Cells[4, 5, nRow, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    sheet2.Cells[4, 3, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                }

            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        #endregion
        #region Đánh giá nhân viên
        public dynamic GetNextSoQuyTrinhDanhGiaNhanVien(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "KHLTG",
                db.HRM_DanhGiaNhanVien.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_DanhGiaNhanVien> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_DanhGiaNhanVien
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_DanhGiaNhanVien
                             orderby a.Created descending
                             select a;

                //var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                //if (listCongTy != null)
                //{
                //    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                //    result = result.Where(a => a.isCoSo == false || (a.isCoSo == true && listId.Contains(a.IdCongTy)));
                //}
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Ngay >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Ngay < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));

                if (TimKiem.IdBoPhan > 0)
                    result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                List<string> listIdTieuChi = itemList.Select(a => a.IdThuVienDanhGia).Distinct().ToList();
                var listTieuChi = db.HRM_ThuVienDanhGia.Where(a => listIdTieuChi.Contains(a.IdGuid)).ToList();
                foreach (HRM_DanhGiaNhanVien item in itemList)
                {
                    item.DateTimeToUnix();
                    BoPhan duAn = BoPhans.Find(x => x.Id == item.IdCongTy);
                    if (duAn != null)
                        item.TenCongty = duAn.TenBoPhan;
                    BoPhan boPhan = BoPhans.Find(x => x.Id == item.IdBoPhan);
                    if (boPhan != null)
                        item.TenBoPhan = boPhan.TenBoPhan;
                    var TieuChi = listTieuChi.Find(x => x.IdGuid == item.IdThuVienDanhGia);
                    if (TieuChi != null)
                        item.TenTieuChi = TieuChi.Ten;

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
        public List<clsTieuChiDanhGia_HeaderRow> GetTieuDe(ApplicationDbContext db, string IdThuVienDanhGia, bool isCoSo)
        {
            List<clsTieuChiDanhGia_HeaderRow> listTieuDe = new List<clsTieuChiDanhGia_HeaderRow>();
            List<HRM_ThuVienDanhGia_Item> dmTieuChis = db.HRM_ThuVienDanhGia_Item.AsNoTracking().Where(x => x.IdThuVienDanhGia == IdThuVienDanhGia && ((isCoSo == false && x.isTeam == true) || (isCoSo == true && x.isCoSo == true))).OrderBy(x => x.SapXep).ToList();
            List<HRM_ThuVienDanhGia_NhanSu> danhGia_NhanSus = db.HRM_ThuVienDanhGia_NhanSu.Where(x => x.IdThuVienDanhGia == IdThuVienDanhGia).ToList();
            var listPhongBanUser = _CallWebService.GetDanhSachBoPhanByUser(db);
            var nhanSus = _CallWebService.GetListNhanSu(db);
            var phongBans = _CallWebService.GetListPhongBan(db);
            foreach (var item in dmTieuChis)
            {
                List<HRM_ThuVienDanhGia_NhanSu> listNhanSuFind = danhGia_NhanSus.FindAll(x => x.IdThuVienDanhGiaItem == item.IdGuid);
                item.listNhanSu = new List<NhanSu>();
                item.listPhongBan = new List<PhongBanView>();
                //if (listNhanSuFind == null || listNhanSuFind.Count == 0)
                //{
                //    item.isCapNhat = true;
                //    item.isShow = true;
                //}
                //else
                foreach (var itemD in listNhanSuFind)
                {
                    if (!string.IsNullOrEmpty(itemD.IdUser))
                    {
                        NhanSu itemFind = nhanSus.Find(x => x.IdUser == itemD.IdUser);
                        if (itemFind == null)
                            continue;
                        NhanSu itemAdd = new NhanSu();
                        itemAdd = itemFind.Clone();
                        itemAdd.isCapNhat = itemD.isCapNhat;
                        itemAdd.isXem = itemD.isXem;
                        if (itemAdd.isCapNhat == true)
                            itemAdd.isXem = true;
                        itemAdd.IdThuVienDanhGiaItem = itemD.IdGuid;
                        item.listNhanSu.Add(itemAdd);
                    }
                    else
                    {
                        PhongBanView itemFind = phongBans.Find(x => x.Id == itemD.IdPhongBan);
                        if (itemFind == null)
                            continue;
                        PhongBanView itemAdd = new PhongBanView();
                        itemAdd = itemFind.Clone();
                        itemAdd.isCapNhat = itemD.isCapNhat;
                        itemAdd.isXem = itemD.isXem;
                        //if (CheckQuyen == true)
                        //{
                        if (itemAdd.isCapNhat == true)
                            itemAdd.isXem = true;
                        //}
                        itemAdd.IdThuVienDanhGiaItem = itemD.IdGuid;
                        item.listPhongBan.Add(itemAdd);
                    }
                }
                foreach (var itemD in item.listNhanSu)
                {


                    if (itemD.IdUser == db.currentUser.Id)
                    {
                        item.isCapNhat = itemD.isCapNhat;
                        item.isXem = itemD.isXem;
                        item.isShow = true;
                        break;
                    }
                }
                if (item.isShow != true)
                {
                    foreach (var itemD in item.listPhongBan)
                    {
                        if (listPhongBanUser.Find(x => x.Id == itemD.Id) != null)
                        {
                            item.isCapNhat = itemD.isCapNhat;
                            item.isXem = itemD.isXem;
                            item.isShow = true;
                            break;
                        }
                    }
                }
            }



            List<HRM_ThuVienDanhGia_Item> dmParents = dmTieuChis.Where(x => x.IdParent == null).OrderBy(x => x.SapXep).ToList();
            List<string> listIdParent = dmParents.Select(a => a.IdGuid).Distinct().ToList();
            List<HRM_ThuVienDanhGia_Item> dmParentRow2s = dmTieuChis.Where(x => listIdParent.Contains(x.IdParent)).OrderBy(x => x.SapXep).ToList();
            List<string> listIdParent2 = dmParents.Select(a => a.IdGuid).Distinct().ToList();
            List<HRM_ThuVienDanhGia_Item> dmParentRow3s = dmTieuChis.Where(x => listIdParent2.Contains(x.IdParent)).OrderBy(x => x.SapXep).ToList();

            int? rowspan = 0;
            List<string> listIdNhomDanhGia = dmTieuChis.Select(x => x.IddmNhomDanhGia).Distinct().ToList();
            if (dmTieuChis.Find(x => x.IdParent != null) != null)
                rowspan = 2;
            else rowspan = 1;
            if (listIdNhomDanhGia.Count > 0)
                rowspan++;
            clsTieuChiDanhGia_HeaderRow row1 = new clsTieuChiDanhGia_HeaderRow();
            clsTieuChiDanhGia_HeaderRow row2 = new clsTieuChiDanhGia_HeaderRow();
            clsTieuChiDanhGia_HeaderRow row3 = new clsTieuChiDanhGia_HeaderRow();
            clsTieuChiDanhGia_HeaderCol itemSTT = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("STT", "STT"), rowspan, 1);
            row1.listCol.Add(itemSTT);
            clsTieuChiDanhGia_HeaderCol itemMa = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("Ma", "Mã nhân viên"), rowspan, 1);
            row1.listCol.Add(itemMa);
            clsTieuChiDanhGia_HeaderCol itemTen = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("Ten", "Tên nhân viên"), rowspan, 1);
            row1.listCol.Add(itemTen);


            List<HRM_dmNhomDanhGia> dmNhomDanhGias = db.HRM_dmNhomDanhGia.AsNoTracking().Where(x => listIdNhomDanhGia.Contains(x.IdGuid)).OrderBy(x => x.SapXep).ToList();
            int STT = 1;
            foreach (var item in dmNhomDanhGias)
            {
                List<HRM_ThuVienDanhGia_Item> dmParentFinds = dmParents.FindAll(x => x.IddmNhomDanhGia == item.IdGuid);
                if (dmParentFinds.Count == 0)
                    continue;

                int ColParent = 0;
                foreach (var dmParent in dmParentFinds)
                {
                    List<HRM_ThuVienDanhGia_Item> dmChilds = dmTieuChis.Where(x => x.IdParent == dmParent.IdGuid).OrderBy(x => x.SapXep).ToList();
                    clsTieuChiDanhGia_HeaderCol itemParent = new clsTieuChiDanhGia_HeaderCol(dmParent, 1, 1);
                    if (dmChilds.Count > 0)
                    {
                        itemParent.rowspan = 1;
                        itemParent.isHaveChild = true;
                        itemParent.colspan = dmChilds.Count;
                        foreach (var dmChild in dmChilds)
                        {
                            clsTieuChiDanhGia_HeaderCol Add = new clsTieuChiDanhGia_HeaderCol(dmChild, 1, 1);
                            Add.STT = STT;
                            row3.listCol.Add(Add);
                            ColParent++;
                            STT++;
                        }
                    }
                    else
                    {
                        if (dmParentRow3s != null && dmParentRow3s.Count > 0)
                            itemParent.rowspan = 2;
                        else itemParent.rowspan = 1;
                        ColParent++;
                    }
                    if (itemParent.colspan == 1)
                    {
                        itemParent.STT = STT;
                        STT++;
                    }
                    row2.listCol.Add(itemParent);

                }
                if (ColParent == 0)
                    ColParent = dmParentFinds.Count;


                clsTieuChiDanhGia_HeaderCol itemGrandParent = new clsTieuChiDanhGia_HeaderCol(item, 1, ColParent);
                if (ColParent == 1)
                    itemGrandParent.STT = STT;
                itemGrandParent.isHaveChild = true;
                row1.listCol.Add(itemGrandParent);
                STT++;
            }

            clsTieuChiDanhGia_HeaderCol itemTONG = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("TONG", "TỔNG"), rowspan, 1);
            itemTONG.STT = STT;
            itemTONG.isDisable = true;
            row1.listCol.Add(itemTONG);
            STT++;
            //clsTieuChiDanhGia_HeaderCol itemTONGTB = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("TRUNGBINH", "TRUNG BÌNH"), rowspan, 1);
            //itemTONGTB.STT = STT;
            //itemTONGTB.isDisable = true;
            //row1.listCol.Add(itemTONGTB);
            //STT++;
            //clsTieuChiDanhGia_HeaderCol DiemChu = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("DIEMCHU", "ĐIỂM CHỮ"), rowspan, 1);
            //DiemChu.STT = STT;
            //DiemChu.isDisable = true;
            //row1.listCol.Add(DiemChu);
            //STT++;
            clsTieuChiDanhGia_HeaderCol GhiChu = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("GHICHU", "GHI CHÚ"), rowspan, 1);
            GhiChu.STT = STT;
            GhiChu.isDisable = false;
            row1.listCol.Add(GhiChu);

            listTieuDe.Add(row1);
            listTieuDe.Add(row2);
            listTieuDe.Add(row3);
            return listTieuDe;
        }
        public dynamic KhoiTaoQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, long IdCongTy, long IdBoPhan, string IdThuVienDanhGia, bool isCoSo = true)
        {
            try
            {
                HRM_DanhGiaNhanVien QuyTrinh = new HRM_DanhGiaNhanVien();
                QuyTrinh.SoQuyTrinh = GetNextSoQuyTrinhDanhGiaNhanVien(db).SoPhieu;
                QuyTrinh.listHeader = GetTieuDe(db, IdThuVienDanhGia, isCoSo);
                QuyTrinh.isCoSo = isCoSo;
                QuyTrinh.listItem = new List<clsTieuChiDanhGia_Data_Item>();
                int Col = 0;
                foreach (var itemHeader in QuyTrinh.listHeader)
                {
                    foreach (var itemCol in itemHeader.listCol)
                    {
                        if (itemCol.colspan != 1)
                            continue;
                        Col++;
                        int nColBegin = 4;
                        if (Col < nColBegin)
                            continue;
                        if (itemCol.isHaveChild == false)
                        {
                            clsTieuChiDanhGia_Data_Item itemAdd = new clsTieuChiDanhGia_Data_Item();
                            itemAdd.SapXep = itemCol.STT;
                            itemAdd.IdThuVienTieuChiDanhGia = itemCol.IdThuVienDanhGiaTieuChi;
                            itemAdd.isDisable = itemCol.isDisable;
                            itemAdd.DiemToiDa = itemCol.DiemToiDa;
                            itemAdd.SoSauDauPhay = itemCol.SoThapPhan;
                            itemAdd.isRoot = itemCol.isRoot;
                            itemAdd.isCapNhat = itemCol.isCapNhat;
                            itemAdd.isXem = itemCol.isXem;
                            itemAdd.isShow = itemCol.isShow;
                            QuyTrinh.listItem.Add(itemAdd);
                        }

                    }
                }
                QuyTrinh.listItem = QuyTrinh.listItem.OrderBy(a => a.SapXep).ToList();
                QuyTrinh.IdCongTy = IdCongTy;
                QuyTrinh.IdBoPhan = IdBoPhan;
                QuyTrinh.IdThuVienDanhGia = IdThuVienDanhGia;

                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic GetQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, string Id)
        {
            try
            {
                HRM_DanhGiaNhanVien QuyTrinh = db.HRM_DanhGiaNhanVien.FirstOrDefault(a => a.IdGuid == Id);
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                QuyTrinh.DateTimeToUnix();
                var listDGNhanVien = db.HRM_DanhGiaNhanVien_Item.Where(r => r.IdDanhGiaNhanVien == Id).Distinct().ToList();
                //QuyTrinh.listNhanVien = db.HRM_DanhGiaNhanVien_Item.Where(r => r.IdDanhGiaNhanVien == Id).Distinct().ToList();
                QuyTrinh.listHeader = GetTieuDe(db, QuyTrinh.IdThuVienDanhGia, QuyTrinh.isCoSo);
                if (listDGNhanVien != null && listDGNhanVien.Count > 0)
                {

                    QuyTrinh.listItem = new List<clsTieuChiDanhGia_Data_Item>();
                    List<string> listIdNhanVien = listDGNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    List<long> listIdNVEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    //result.listNhanVien = ListUser.Select(a => new HRM_DanhGiaNhanVien_NhanVienViewModel(a)).ToList();
                    List<Link_NhanVien_BoPhan_ChucVu> listChucVu = _CallWebService.GetDanhSachChucVuTheoListIdNhanVien(db, listIdNVEOS);
                    QuyTrinh.listData = new List<clsTieuChiDanhGia_Data>();
                    int STT = 1;
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    foreach (string IdNhanVien in listIdNhanVien)
                    {

                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == IdNhanVien);
                        if (nhanVien == null)
                            continue;
                        var listNhanVienFind = listDGNhanVien.Where(x => x.IdNhanVien == IdNhanVien).OrderBy(a => a.SapXep).ToList();
                        clsTieuChiDanhGia_Data itemData = new clsTieuChiDanhGia_Data(nhanVien, QuyTrinh.listHeader, listNhanVienFind, STT, db.currentUser.Id);
                        //if (listChucVu != null && listChucVu.Count > 0)
                        //{
                        //    var chucVu = listChucVu.FirstOrDefault(a => a.NhanVienPkid == nhanVien.IdNhanVienEOS);
                        //    if (chucVu != null)
                        //        itemData.ChucVu = chucVu.TenChucVu;
                        //}
                        if (listChucDanh != null && listChucDanh.Count > 0)
                        {
                            var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                            if (ChucDanh != null)
                                itemData.TenChucDanh = ChucDanh.Ten;
                        }

                        QuyTrinh.listData.Add(itemData);

                        STT++;
                    }
                    QuyTrinh.listItem = QuyTrinh.listData.FirstOrDefault().listGiaTri.OrderBy(a => a.SapXep).ToList();
                }
                else
                {
                    QuyTrinh.listItem = new List<clsTieuChiDanhGia_Data_Item>();
                    int Col = 0;
                    foreach (var itemHeader in QuyTrinh.listHeader)
                    {
                        foreach (var itemCol in itemHeader.listCol)
                        {
                            if (itemCol.colspan != 1)
                                continue;
                            Col++;
                            int nColBegin = 4;
                            if (Col < nColBegin)
                                continue;
                            if (itemCol.isHaveChild == false)
                            {
                                clsTieuChiDanhGia_Data_Item itemAdd = new clsTieuChiDanhGia_Data_Item();
                                itemAdd.SapXep = itemCol.STT;
                                itemAdd.IdThuVienTieuChiDanhGia = itemCol.IdThuVienDanhGiaTieuChi;
                                itemAdd.isDisable = itemCol.isDisable;
                                itemAdd.SoSauDauPhay = itemCol.SoThapPhan;
                                itemAdd.DiemToiDa = itemCol.DiemToiDa;
                                itemAdd.isRoot = itemCol.isRoot;
                                itemAdd.isCapNhat = itemCol.isCapNhat;
                                itemAdd.isXem = itemCol.isXem;
                                itemAdd.isShow = itemCol.isShow;
                                itemAdd.BoPhanDanhGia = itemCol.BoPhanDanhGia;
                                itemAdd.TyTrongQLCS = itemCol.TyTrongQLCS;
                                QuyTrinh.listItem.Add(itemAdd);
                            }
                        }
                    }
                    QuyTrinh.listItem = QuyTrinh.listItem.OrderBy(a => a.SapXep).ToList();
                }
                return QuyTrinh;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra:" + ex.ToString());
            }
        }
        public dynamic SetQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_DanhGiaNhanVien QuyTrinh = JsonConvert.DeserializeObject<HRM_DanhGiaNhanVien>(data.ToString());
                    // DI: use inherited _CallWebService
                    var QuyTrinhdb = db.HRM_DanhGiaNhanVien.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (QuyTrinhdb != null && QuyTrinhdb.IdTrangThai != QuyTrinh.IdTrangThai)
                    {
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = QuyTrinh.IdGuid,
                            eAction = DefineData.eAction.DANHGIANHANVIEN.ToString(),
                            IdTrangThai = QuyTrinh.IdTrangThai,
                        };
                        var trangThai = _CallWebService.DoiTrangThaiKhoiTao(audit, db);
                    }
                    QuyTrinh.UnixToDateTime();
                    List<string> listIdNhanVien = QuyTrinh.listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var checks = db.HRM_DanhGiaNhanVien_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.Ngay.Value.Year == QuyTrinh.Ngay.Value.Year && a.Ngay.Value.Month == QuyTrinh.Ngay.Value.Month).ToList();
                    if (checks != null && checks.Count > 0)
                    {
                        listIdNhanVien = checks.Select(a => a.IdNhanVien).Distinct().ToList();
                        var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên đang được đánh giá ở phiếu khác :" + string.Join(", ", listNhanVien.Select(a => a.TenDayDu)));
                    }
                    QuyTrinh.listNhanVien = new List<HRM_DanhGiaNhanVien_Item>();

                    foreach (var item in QuyTrinh.listData)
                    {
                        HRM_DanhGiaNhanVien_Item itemAdd = new HRM_DanhGiaNhanVien_Item();
                        itemAdd.IdNhanVien = item.IdNhanVien;
                        for (int i = 0; i < item.listGiaTri.Count; i++)
                        {
                            itemAdd.Diem = item.listGiaTri[i].GiaTri;
                            itemAdd.IdThuVienTieuChiDanhGia = item.listGiaTri[i].IdThuVienTieuChiDanhGia;

                            itemAdd.DiemToiDa = item.listGiaTri[i].DiemToiDa;
                            itemAdd.BoPhanDanhGia = item.listGiaTri[i].BoPhanDanhGia;
                            itemAdd.TyTrongQLCS = item.listGiaTri[i].TyTrongQLCS;
                        }
                        QuyTrinh.listNhanVien.Add(itemAdd);
                    }

                    MessageReturn msg = UpdateQuyTrinhDanhGiaNhanVien(QuyTrinh, db);
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
        public dynamic UpdateQuyTrinhDanhGiaNhanVien(HRM_DanhGiaNhanVien QuyTrinh, ApplicationDbContext db = null)
        {
            try
            {
                QuyTrinh.UnixToDateTime();
                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.IdGuid = Guid.NewGuid().ToString();
                    if (string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        TrangThaiQuyTrinh QuyTrinhTrangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.DANHGIANHANVIEN.ToString(), QuyTrinh.IdGuid);
                        if (QuyTrinhTrangThai.IdTrangThai == null || QuyTrinhTrangThai.IdTrangThai == "")
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, QuyTrinhTrangThai.TenTrangThai + " do quy trình số " + QuyTrinh.SoQuyTrinh);
                        QuyTrinh.IdTrangThai = QuyTrinhTrangThai.IdTrangThai;
                        QuyTrinh.TenTrangThai = QuyTrinhTrangThai.TenTrangThai;
                    }
                    QuyTrinh.Modified = DateTime.Now;
                    QuyTrinh.Ngay = DateTime.Now;
                    db.HRM_DanhGiaNhanVien.Add(QuyTrinh);
                }
                else
                {
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListUser = db.HRM_DanhGiaNhanVien_Item.Where(r => r.IdUser == db.currentUser.Id && r.IdDanhGiaNhanVien == QuyTrinh.IdGuid).ToList();
                db.HRM_DanhGiaNhanVien_Item.RemoveRange(ListUser);

                if (QuyTrinh.listData != null && QuyTrinh.listData.Count > 0)
                {
                    List<HRM_DanhGiaNhanVien_Item> listAdd = new List<HRM_DanhGiaNhanVien_Item>();
                    foreach (var item in QuyTrinh.listData)
                    {

                        if (item.listGiaTri != null && item.listGiaTri.Count > 0)
                        {
                            int STT = 1;
                            double TongDiem = item.listGiaTri.Where(a => a.GiaTri != null && a.IdThuVienTieuChiDanhGia != "TONG" /*&& a.IdThuVienTieuChiDanhGia != "TRUNGBINH"*/).Sum(a => a.GiaTri ?? 0);
                            //double TongDiemTeam = item.listGiaTri.Where(a => a.GiaTri != null && a.IdThuVienTieuChiDanhGia != "TONG" && a.IdThuVienTieuChiDanhGia != "TRUNGBINH").Sum(a => a.GiaTriTeamDanhGia ?? 0);
                            foreach (var GTri in item.listGiaTri)
                            {
                                HRM_DanhGiaNhanVien_Item Add = new HRM_DanhGiaNhanVien_Item(GTri);
                                Add.IdGuid = Guid.NewGuid().ToString();
                                Add.IdNhanVien = item.IdNhanVien;
                                Add.IdBoPhan = QuyTrinh.IdBoPhan;
                                Add.IdCongTy = QuyTrinh.IdCongTy;
                                Add.IdUser = db.currentUser.Id;
                                Add.isKetThuc = QuyTrinh.isKetThuc;
                                Add.isCoSo = QuyTrinh.isCoSo;
                                Add.IdDanhGiaNhanVien = QuyTrinh.IdGuid;
                                Add.Ngay = QuyTrinh.Ngay;
                                Add.SapXep = STT;
                                Add.BoPhanDanhGia = GTri.BoPhanDanhGia;
                                if (Add.IdThuVienTieuChiDanhGia == "TONG")
                                {
                                    Add.Diem = TongDiem;
                                    listAdd.Add(Add);
                                }
                                //if (Add.IdThuVienTieuChiDanhGia == "TRUNGBINH")
                                //{
                                //    Add.Diem = (TongDiem + TongDiemTeam) / 2;
                                //    DiemChu = DefineData.QuyDoiXepLoai(Add.Diem);

                                //}
                                if (Add.IdThuVienTieuChiDanhGia == "DIEMCHU")
                                {
                                    Add.GhiChu = DefineData.QuyDoiXepLoai(Add.Diem);
                                    listAdd.Add(Add);
                                }
                                if (Add.IdThuVienTieuChiDanhGia == "GHICHU")
                                {

                                    listAdd.Add(Add);
                                }

                                if (GTri.isCapNhat == true)
                                    listAdd.Add(Add);
                                STT++;
                            }

                        }
                    }

                    db.HRM_DanhGiaNhanVien_Item.AddRange(listAdd);
                }

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công", QuyTrinh);

            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }

        }
        public dynamic DeleteQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, string Id)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_DanhGiaNhanVien QuyTrinh = db.HRM_DanhGiaNhanVien.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_DanhGiaNhanVien_Item.Where(r => r.IdDanhGiaNhanVien == Id).ToList();
                    db.HRM_DanhGiaNhanVien_Item.RemoveRange(ListUser);
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
        public dynamic ChuyenTiepQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_DanhGiaNhanVien QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_DanhGiaNhanVien)) as HRM_DanhGiaNhanVien;

                    HRM_DanhGiaNhanVien pcuDatabase = db.HRM_DanhGiaNhanVien.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }
                    var callWeb = _CallWebService;
                    ChuyenTrangThai audit = new ChuyenTrangThai()
                    {
                        IdGuid = QuyTrinh.IdGuid,
                        eAction = DefineData.eAction.DANHGIANHANVIEN.ToString(),
                        IdTrangThai = QuyTrinh.IdTrangThai,

                    };
                    TrangThaiQuyTrinh TrangThaidnm = _CallWebService.GetTrangThaiChuyenTiep(db, audit);
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
                    MessageReturn msg = UpdateQuyTrinhDanhGiaNhanVien(QuyTrinh, db);
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
        public dynamic KhongDuyetQuyTrinhDanhGiaNhanVien(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_DanhGiaNhanVien QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_DanhGiaNhanVien)) as HRM_DanhGiaNhanVien;
                    HRM_DanhGiaNhanVien pcuDatabase = db.HRM_DanhGiaNhanVien.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }
                    }
                    MessageReturn msg = UpdateQuyTrinhDanhGiaNhanVien(QuyTrinh, db);
                    if (msg.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return msg;
                    }
                    var callWeb = _CallWebService;
                    dynamic TrangThaidnm = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.DANHGIANHANVIEN.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
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
        //public dynamic ImportDanhGiaNhanVien(ApplicationDbContext db, string FileName, double TuNgayUnix, double DenNgayUnix)
        //{
        //    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

        //    string pathTemplate =PathUploads+ FileName;
        //    if (File.Exists(pathTemplate) == false)
        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
        //    Stream s = File.OpenRead(pathTemplate);
        //    ExcelPackage package = new ExcelPackage(s);

        //    List<HRM_DanhGiaNhanVien_NhanVienViewModel> listResult = new List<HRM_DanhGiaNhanVien_NhanVienViewModel>();
        //    List<HRM_DanhGiaNhanVien_NhanVien> listNhanVien = new List<HRM_DanhGiaNhanVien_NhanVien>();
        //    var sheet1 = package.Workbook.Worksheets.First();
        //    DateTime TuNgay = UnixTimeStampToDateTime(TuNgayUnix);
        //    DateTime DenNgay = UnixTimeStampToDateTime(DenNgayUnix);
        //    var listCa = db.HRM_dmLichLamViec.ToList();
        //    for (int row = 3; row <= sheet1.Cells.End.Row; row++)
        //    {

        //        int nCol = 2;
        //        string MaNhanVien = dungchung.GetCell(sheet1, row, nCol);
        //        if (string.IsNullOrEmpty(MaNhanVien))
        //            break;
        //        nCol++;
        //        string TenNhanVien = dungchung.GetCell(sheet1, row, nCol);
        //        nCol++;
        //        DateTime time = TuNgay;
        //        while (time < DenNgay.AddDays(1))
        //        {
        //            HRM_DanhGiaNhanVien_NhanVien nhanVien = new HRM_DanhGiaNhanVien_NhanVien();
        //            nhanVien.MaNhanVien = MaNhanVien;
        //            nhanVien.TenNhanVien = TenNhanVien;
        //            nhanVien.Ngay = time;
        //            string MaCa = dungchung.GetCell(sheet1, row, nCol);
        //            List<string> listMaCa = new List<string>();
        //            if (!string.IsNullOrEmpty(MaCa))
        //            {
        //                string[] list = MaCa.Split(',');
        //                foreach (var a in list)
        //                    listMaCa.Add(a);
        //                var cas = listCa.Where(a => listMaCa.Contains(a.Ten)).ToList();
        //                if (cas != null && cas.Count > 0)
        //                    nhanVien.IdLichLamViec = string.Join(",", cas.Select(a => a.IdGuid).ToList());
        //            }
        //            listNhanVien.Add(nhanVien);
        //            nCol++;
        //            time = time.AddDays(1);
        //        }
        //    }
        //    if (listNhanVien != null && listNhanVien.Count > 0)
        //    {
        //        List<string> listIdNhanVien = listNhanVien.Select(a => a.MaNhanVien).Distinct().ToList();
        //        var listNhanViendb = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.MaNhanVien)).ToList();
        //        List<long> listIdNVEOS = listNhanViendb.Select(a => a.IdNhanVienEOS).Distinct().ToList();
        //        List<Link_NhanVien_BoPhan_ChucVu> listChucVu = _CallWebService.GetDanhSachChucVuTheoListIdNhanVien(db, listIdNVEOS);
        //        foreach (var MaNv in listIdNhanVien)
        //        {
        //            var item = new HRM_DanhGiaNhanVien_NhanVienViewModel();
        //            var nhanVien = listNhanViendb.FirstOrDefault(a => a.MaNhanVien == MaNv);
        //            if (nhanVien != null)
        //            {
        //                item.IdNhanVien = nhanVien.IdGuid;
        //                item.MaNhanVien = nhanVien.MaNhanVien;

        //                item.TenNhanVien = nhanVien.TenDayDu;
        //                if (listChucVu != null && listChucVu.Count > 0)
        //                {
        //                    var chucVu = listChucVu.FirstOrDefault(a => a.NhanVienPkid == nhanVien.IdNhanVienEOS);
        //                    if (chucVu != null)
        //                        item.ChucVu = chucVu.TenChucVu;
        //                }
        //            }
        //            item.listWeek = new List<HRM_DanhGiaNhanVien_ChiTietViewModel>();
        //            var data = listNhanVien.Where(a => a.MaNhanVien == item.MaNhanVien).OrderBy(a => a.Ngay).ToList();
        //            item.listWeek = data.Select(a => new HRM_DanhGiaNhanVien_ChiTietViewModel(a)).ToList();
        //            listResult.Add(item);
        //        }
        //    }


        //    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", listResult); ;
        //}
        //public dynamic ExportDanhGiaNhanVien(dynamic dyn, ApplicationDbContext db)
        //{
        //    string sFile = "DanhSachNhanVienKeHoachTuan.xlsx";

        //    string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
        //    DateTime dt = DateTime.Now;
        //    var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
        //    string sFileNameCopy =PathUploads+ filenamegui;
        //    File.Copy(sFileName, sFileNameCopy, true);
        //    Stream s = File.OpenRead(sFileName);
        //    ExcelPackage package = new ExcelPackage(s);
        //    ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
        //    HRM_DanhGiaNhanVienViewModel entity = JsonConvert.DeserializeObject<HRM_DanhGiaNhanVienViewModel>(dyn.ToString());
        //    HRM_DanhGiaNhanVien QuyTrinh = new HRM_DanhGiaNhanVien(entity);
        //    QuyTrinh.listNhanVien = new List<HRM_DanhGiaNhanVien_NhanVien>();
        //    if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
        //    {
        //        foreach (var item in entity.listNhanVien)
        //        {
        //            if (item.listWeek != null && item.listWeek.Count > 0)
        //            {
        //                foreach (var child in item.listWeek)
        //                {
        //                    HRM_DanhGiaNhanVien_NhanVien nv = new HRM_DanhGiaNhanVien_NhanVien(item, child);
        //                    QuyTrinh.listNhanVien.Add(nv);
        //                }
        //            }
        //        }
        //    }
        //    QuyTrinh.UnixToDateTime();
        //    int RowBegin = 3;
        //    int Row = RowBegin;
        //    int STT = 4;
        //    if (QuyTrinh.TuNgay != null && QuyTrinh.DenNgay != null)
        //    {
        //        DateTime time = (DateTime)QuyTrinh.TuNgay;

        //        while (time < QuyTrinh.DenNgay.Value.AddDays(1))
        //        {

        //            int DayOfWeek = DefineData.QuyDoiThuSangSo(time.DayOfWeek.ToString());
        //            string Thu = DefineData.QuyDoiThu(DayOfWeek);
        //            sheet1.Cells[1, STT].Value = time.ToString("dd/MM/yyyy");
        //            sheet1.Cells[2, STT].Value = Thu;
        //            STT++;
        //            time = time.AddDays(1);
        //        }
        //    }
        //    STT = 1;
        //    int sttChild = 1;
        //    if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
        //    {
        //        List<string> IdNhanViens = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();

        //        var listCa = db.HRM_dmLichLamViec.ToList();
        //        foreach (var Id in IdNhanViens)
        //        {
        //            var listNgay = QuyTrinh.listNhanVien.Where(a => a.IdNhanVien == Id).OrderBy(a => a.Ngay).ToList();
        //            var item = listNgay.FirstOrDefault();
        //            sttChild = 1;
        //            sheet1.Cells[Row, sttChild].Value = STT;
        //            sttChild++;
        //            sheet1.Cells[Row, sttChild].Value = item.MaNhanVien;
        //            sttChild++;
        //            sheet1.Cells[Row, sttChild].Value = item.TenNhanVien;
        //            sttChild++;
        //            foreach (var ngay in listNgay)
        //            {
        //                List<string> listIddmCaLamViec = new List<string>();
        //                if (!string.IsNullOrEmpty(ngay.IdLichLamViec))
        //                {
        //                    string[] list = ngay.IdLichLamViec.Split(',');
        //                    foreach (var a in list)
        //                        listIddmCaLamViec.Add(a);
        //                    var cas = listCa.Where(a => listIddmCaLamViec.Contains(a.IdGuid)).ToList();
        //                    sheet1.Cells[Row, sttChild].Value = string.Join(",", cas.Select(a => a.Ten).ToList());
        //                }
        //                sttChild++;
        //            }
        //            Row++;
        //            STT++;
        //        }

        //    }

        //    Row--;
        //    sttChild--;
        //    dungchung.DrawTableExcel(sheet1, 1, Row, 1, sttChild);
        //    s.Close();
        //    byte[] bytee = package.GetAsByteArray();
        //    File.WriteAllBytes(sFileNameCopy, bytee);
        //    dynamic dnm = new DynamicObject();
        //    dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
        //                    + "&path=" + Base64Encode(sFileNameCopy);
        //    return dnm;
        //}
        public dynamic GetBaoCaoDanhGiaNhanVien(dynamic dyn, ApplicationDbContext db)
        {
            TimKiemBangLuong TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiemBangLuong)) as TimKiemBangLuong;
            List<HRM_BaoCaoKetQuaKPI> listResult = new List<HRM_BaoCaoKetQuaKPI>();
            var listData = (from a in db.HRM_DanhGiaNhanVien_Item
                            join b in db.HRM_DanhGiaNhanVien on a.IdDanhGiaNhanVien equals b.IdGuid
                            where a.IdThuVienTieuChiDanhGia != "TONG" && b.isKetThuc == true && (TimKiem.Nam == 0 || a.Ngay.Value.Year == TimKiem.Nam) && (TimKiem.Thang == 0 || a.Ngay.Value.Month == TimKiem.Thang)
                            select a).ToList();
            if (TimKiem.IdCongty > 0)
            {
                listData = listData.Where(a => a.IdCongTy == TimKiem.IdCongty).ToList();
            }
            if (TimKiem.listIdBoPhan != null && TimKiem.listIdBoPhan.Count > 0)
            {
                listData = listData.Where(a => TimKiem.listIdBoPhan.Contains(a.IdBoPhan)).ToList();
            }
            if (listData != null && listData.Count > 0)
            {
                List<string> listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid) && (string.IsNullOrEmpty(TimKiem.Keyword) || (!string.IsNullOrEmpty(TimKiem.Keyword) && (a.MaNhanVien.ToLower().Contains(TimKiem.Keyword.ToLower()) || a.TenDayDu.ToLower().Contains(TimKiem.Keyword.ToLower()))))).ToList();
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                List<string> listId = listData.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                var listTieuChiDanhGia = db.HRM_ThuVienDanhGia_Item.Where(a => listId.Contains(a.IdGuid)).ToList();
                foreach (var item in listNhanVien)
                {
                    var listDataDG = listData.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                    var listDataTeamDG = listDataDG.Where(a => a.isCoSo == false && a.IdNhanVien == item.IdGuid).ToList();
                    var listDataQLCSDG = listDataDG.Where(a => a.isCoSo == true && string.IsNullOrEmpty(a.BoPhanDanhGia) && a.IdNhanVien == item.IdGuid).ToList();
                    var listDataKhac = listDataDG.Where(a => a.isCoSo == true && !string.IsNullOrEmpty(a.BoPhanDanhGia) && a.IdNhanVien == item.IdGuid).ToList();

                    HRM_BaoCaoKetQuaKPI itemAdd = new HRM_BaoCaoKetQuaKPI();
                    itemAdd.MaNhanVien = item.MaNhanVien;
                    itemAdd.IdNhanVien = item.IdGuid;
                    itemAdd.HoVaTen = item.TenDayDu;
                    var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                    if (chucVu != null)
                        itemAdd.ViTri = chucVu.TenChucVu;
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                    if (ChucDanh != null)
                        itemAdd.TenChucDanh = ChucDanh.Ten;
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                    if (BoPhan != null)
                        itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                    var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                    if (CongTy != null)
                        itemAdd.TenCoSo = CongTy.TenBoPhan;

                    if (item.NgayBatDauLamViec != null)
                        itemAdd.NgayBatDau = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                    itemAdd.listChiTiet = new List<HRM_BaoCaoKetQuaKPI_ChiTiet>();
                    // QuanLy cơ sở
                    HRM_BaoCaoKetQuaKPI_ChiTiet QLCSDG = new HRM_BaoCaoKetQuaKPI_ChiTiet();
                    QLCSDG.Ten = "CS";
                    QLCSDG.listThang = new List<HRM_BaoCaoKetQuaKPI_Item>();
                    for (int i = 1; i <= 12; i++)
                    {
                        HRM_BaoCaoKetQuaKPI_Item Thang = new HRM_BaoCaoKetQuaKPI_Item();
                        Thang.Thang = i;
                        var data = listDataQLCSDG.Where(a => a.IdNhanVien == item.IdGuid && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                        if (data != null)
                        {
                            List<string> listIdTC = data.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                            foreach (var Id in listIdTC)
                            {
                                var DataTC = data.Where(a => a.IdThuVienTieuChiDanhGia == Id).ToList();
                                if (DataTC != null && DataTC.Count > 0)
                                    Thang.Diem += (DataTC.Sum(a => a.Diem ?? 0) / DataTC.Count);
                            }
                            if (Thang.Diem == 0)
                                Thang.Diem = null;
                            //Thang.DanhGia = DefineData.QuyDoiXepLoai(Thang.Diem);
                        }
                        QLCSDG.listThang.Add(Thang);
                    }
                    itemAdd.listChiTiet.Add(QLCSDG);
                    HRM_BaoCaoKetQuaKPI_ChiTiet Team = new HRM_BaoCaoKetQuaKPI_ChiTiet();
                    Team.Ten = "Team";
                    Team.listThang = new List<HRM_BaoCaoKetQuaKPI_Item>();
                    for (int i = 1; i <= 12; i++)
                    {
                        HRM_BaoCaoKetQuaKPI_Item Thang = new HRM_BaoCaoKetQuaKPI_Item();
                        Thang.Thang = i;
                        var data = listDataTeamDG.Where(a => a.IdNhanVien == item.IdGuid && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                        if (data != null)
                        {
                            List<string> listIdTC = data.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                            foreach (var Id in listIdTC)
                            {
                                var DataTC = data.Where(a => a.IdThuVienTieuChiDanhGia == Id).ToList();
                                if (DataTC != null && DataTC.Count > 0)
                                    Thang.Diem += (DataTC.Sum(a => a.Diem ?? 0) / DataTC.Count);
                            }
                            if (Thang.Diem == 0)
                                Thang.Diem = null;
                            //Thang.DanhGia = DefineData.QuyDoiXepLoai(Thang.Diem);
                        }
                        Team.listThang.Add(Thang);
                    }
                    itemAdd.listChiTiet.Add(Team);
                    if (listDataKhac != null && listDataKhac.Count > 0)
                    {
                        List<string> listBP = listDataKhac.Select(a => a.BoPhanDanhGia).Distinct().ToList();
                        foreach (var BP in listBP)
                        {
                            HRM_BaoCaoKetQuaKPI_ChiTiet BPK = new HRM_BaoCaoKetQuaKPI_ChiTiet();
                            BPK.Ten = DefineData.QuyDoiBoPhanDanhGia(BP);
                            BPK.listThang = new List<HRM_BaoCaoKetQuaKPI_Item>();
                            for (int i = 1; i <= 12; i++)
                            {
                                HRM_BaoCaoKetQuaKPI_Item Thang = new HRM_BaoCaoKetQuaKPI_Item();
                                Thang.Thang = i;
                                var data = listDataKhac.Where(a => a.IdNhanVien == item.IdGuid && a.BoPhanDanhGia == BP && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                                if (data != null)
                                {
                                    List<string> listIdTC = data.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                                    foreach (var Id in listIdTC)
                                    {
                                        var DataTC = data.Where(a => a.IdThuVienTieuChiDanhGia == Id).ToList();
                                        if (DataTC != null && DataTC.Count > 0)
                                            Thang.Diem += (DataTC.Sum(a => a.Diem ?? 0) / DataTC.Count);
                                    }
                                    if (Thang.Diem == 0)
                                        Thang.Diem = null;
                                    //Thang.DanhGia = DefineData.QuyDoiXepLoai(Thang.Diem);
                                }
                                BPK.listThang.Add(Thang);
                            }
                            itemAdd.listChiTiet.Add(BPK);
                        }
                    }

                    HRM_BaoCaoKetQuaKPI_ChiTiet trungBinh = new HRM_BaoCaoKetQuaKPI_ChiTiet();
                    trungBinh.Ten = "Trung bình";
                    trungBinh.listThang = new List<HRM_BaoCaoKetQuaKPI_Item>();
                    for (int i = 1; i <= 12; i++)
                    {
                        HRM_BaoCaoKetQuaKPI_Item Thang = new HRM_BaoCaoKetQuaKPI_Item();
                        Thang.Thang = i;
                        var dataCS = listDataDG.Where(a => a.isCoSo == true && a.IdNhanVien == item.IdGuid && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                        var dataTeam = listDataTeamDG.Where(a => a.IdNhanVien == item.IdGuid && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                        List<string> listIdTC = dataCS.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                        listIdTC.AddRange(dataTeam.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList());
                        var listTieuChi = listTieuChiDanhGia.Where(a => listIdTC.Contains(a.IdGuid)).ToList();
                        Thang.Diem = 0;
                        foreach (var TC in listTieuChi)
                        {
                            double DiemCS = 0;
                            var CS = dataCS.Where(a => a.IdThuVienTieuChiDanhGia == TC.IdGuid).ToList();
                            if (CS != null)
                            {
                                if (CS != null && CS.Count > 0)
                                    DiemCS += (CS.Sum(a => a.Diem ?? 0) / CS.Count);
                            }
                            var TeamDG = dataTeam.Where(a => a.IdThuVienTieuChiDanhGia == TC.IdGuid).ToList();
                            double DiemTeam = 0;
                            if (TeamDG != null)
                            {
                                if (TeamDG != null && TeamDG.Count > 0)
                                    DiemTeam += (TeamDG.Sum(a => a.Diem ?? 0) / TeamDG.Count);
                            }
                            Thang.Diem += DiemCS * TC.TyTrongQLCS / 100.0 + DiemTeam * (100.0 - TC.TyTrongQLCS) / 100.0;
                        }
                        if (Thang.Diem == 0)
                            Thang.Diem = null;
                        Thang.DanhGia = DefineData.QuyDoiXepLoai(Thang.Diem);
                        trungBinh.listThang.Add(Thang);
                    }
                    itemAdd.listChiTiet.Add(trungBinh);
                    listResult.Add(itemAdd);

                }

            }
            return listResult;
        }
        public dynamic GetTongHopDanhGiaNhanVienThang(dynamic dyn, ApplicationDbContext db)
        {
            TimKiemBangLuong TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiemBangLuong)) as TimKiemBangLuong;
            HRM_TongHopDanhGiaNhanVienThang result = new HRM_TongHopDanhGiaNhanVienThang();
            result.listHeader = new List<clsTieuChiDanhGia_HeaderRow>();
            result.listNhanVien = new List<HRM_TongHopDanhGiaNhanVienThang_NhanVien>();
            result.listItem = new List<clsTieuChiDanhGia_Data_Item>();
            List<HRM_ThuVienDanhGia_Item> dmTieuChis = db.HRM_ThuVienDanhGia_Item.AsNoTracking().Where(x => x.IdThuVienDanhGia == TimKiem.IdThuVienDanhGia).OrderBy(x => x.SapXep).ToList();
            result.listHeader = GetTieuDeTongHop(db, TimKiem.IdThuVienDanhGia, dmTieuChis);
            int Col = 0;
            foreach (var itemHeader in result.listHeader)
            {
                foreach (var itemCol in itemHeader.listCol)
                {
                    if (itemCol.colspan != 1)
                        continue;
                    Col++;
                    int nColBegin = 4;
                    if (Col < nColBegin)
                        continue;
                    if (itemCol.isHaveChild == false)
                    {
                        clsTieuChiDanhGia_Data_Item itemAdd = new clsTieuChiDanhGia_Data_Item();
                        itemAdd.SapXep = itemCol.STT;
                        itemAdd.IdThuVienTieuChiDanhGia = itemCol.IdThuVienDanhGiaTieuChi;
                        itemAdd.isDisable = itemCol.isDisable;
                        itemAdd.DiemToiDa = itemCol.DiemToiDa;
                        itemAdd.SoSauDauPhay = itemCol.SoThapPhan;
                        result.listItem.Add(itemAdd);
                    }

                }
            }

            result.listItem = result.listItem.OrderBy(a => a.SapXep).ToList();
            DateTime Ngay = UnixTimeStampToDateTime(TimKiem.Ngay);
            var listData = db.HRM_DanhGiaNhanVien_Item.Where(a => a.IdThuVienTieuChiDanhGia != "TONG" && TimKiem.listIdNhanVien.Contains(a.IdNhanVien) && a.Ngay.Value.Year == Ngay.Year && a.Ngay.Value.Month == Ngay.Month).ToList();
            if (listData != null && listData.Count > 0)
            {
                if (TimKiem.IdCongty > 0)
                {
                    listData = listData.Where(a => a.IdCongTy == TimKiem.IdCongty).ToList();
                }
                if (TimKiem.listIdBoPhan != null && TimKiem.listIdBoPhan.Count > 0)
                {
                    listData = listData.Where(a => TimKiem.listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                }
                List<string> listIdUser = listData.Select(a => a.IdUser).Distinct().ToList();
                var listUser = _CallWebService.GetDanhSachUserTheoListIdUser(db, listIdUser);
                List<string> listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                List<string> listId = listData.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                var listTieuChiDanhGia = db.HRM_ThuVienDanhGia_Item.Where(a => listId.Contains(a.IdGuid)).ToList();
                foreach (var item in listNhanVien)
                {
                    var listDataDG = listData.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                    var listDataTeamDG = listDataDG.Where(a => a.isCoSo == false && a.IdNhanVien == item.IdGuid).ToList();
                    var listDataQLCSDG = listDataDG.Where(a => a.isCoSo == true && a.IdNhanVien == item.IdGuid).ToList();
                    var listDataKhac = listDataDG.Where(a => a.isCoSo == true && !string.IsNullOrEmpty(a.BoPhanDanhGia) && a.IdNhanVien == item.IdGuid).ToList();

                    HRM_TongHopDanhGiaNhanVienThang_NhanVien itemAdd = new HRM_TongHopDanhGiaNhanVienThang_NhanVien();
                    itemAdd.MaNhanVien = item.MaNhanVien;
                    itemAdd.TenNhanVien = item.TenDayDu;
                    //var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                    //if (chucVu != null)
                    //    itemAdd.ViTri = chucVu.TenChucVu;
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                    if (ChucDanh != null)
                        itemAdd.TenChucDanh = ChucDanh.Ten;
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                    if (BoPhan != null)
                        itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                    itemAdd.listTieuChi = new List<HRM_TongHopDanhGiaNhanVienThang_TieuChi>();
                    double TongDiemCS = 0;
                    double TongDiemTeam = 0;
                    foreach (var TC in result.listItem)
                    {

                        var tieuChi = dmTieuChis.FirstOrDefault(a => a.IdGuid == TC.IdThuVienTieuChiDanhGia);

                        HRM_TongHopDanhGiaNhanVienThang_TieuChi itemAddDG = new HRM_TongHopDanhGiaNhanVienThang_TieuChi();
                        if (tieuChi != null)
                            itemAddDG.TenTieuChi = tieuChi.Ten;
                        itemAddDG.listDiem = new List<HRM_TongHopDanhGiaNhanVienThang_Diem>();
                        if (TC.IdThuVienTieuChiDanhGia != "TONG")
                        {

                            var dataCS = listDataQLCSDG.Where(a => a.IdThuVienTieuChiDanhGia == TC.IdThuVienTieuChiDanhGia && a.Diem != null).OrderByDescending(a => a.Modified).ToList();
                            if (dataCS != null && dataCS.Count > 0)
                            {
                                itemAddDG.DiemCS += (dataCS.Sum(a => a.Diem ?? 0) / dataCS.Count);
                                List<string> listIdU = dataCS.Select(a => a.IdUser).Distinct().ToList();

                                foreach (var Id in listIdU)
                                {
                                    HRM_TongHopDanhGiaNhanVienThang_Diem diem = new HRM_TongHopDanhGiaNhanVienThang_Diem();
                                    var DataTC = dataCS.Where(a => a.IdUser == Id).FirstOrDefault();
                                    var user = listUser.FirstOrDefault(a => a.Id == Id);
                                    if (user != null)
                                        diem.Ten = user.TenNhanVien;
                                    if (DataTC != null)
                                        diem.Diem = DataTC.Diem;
                                    if (diem.Diem != null && diem.Diem > 0)
                                        itemAddDG.listDiem.Add(diem);
                                }
                                string BoPhanDanhGia = dataCS.FirstOrDefault().BoPhanDanhGia;
                                if (!string.IsNullOrEmpty(BoPhanDanhGia))
                                    itemAddDG.TenCSDG = DefineData.QuyDoiBoPhanDanhGia(BoPhanDanhGia);
                                TongDiemCS += (itemAddDG.DiemCS ?? 0);
                            }
                            var dataTeam = listDataTeamDG.Where(a => a.IdThuVienTieuChiDanhGia == TC.IdThuVienTieuChiDanhGia && a.Diem != null).OrderByDescending(a => a.Modified).ToList();
                            if (dataTeam != null && dataTeam.Count > 0)
                            {
                                itemAddDG.TenTeamDG = "Team";
                                itemAddDG.DiemTeam = 0;
                                itemAddDG.DiemTeam = (dataTeam.Sum(a => a.Diem ?? 0) / dataTeam.Count);
                                //List<string> listIdTC = dataTeam.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                                //foreach (var Id in listIdTC)
                                //{
                                //    var DataTC = dataTeam.Where(a => a.IdThuVienTieuChiDanhGia == Id).ToList();
                                //    if (DataTC != null && DataTC.Count > 0)

                                //}
                                TongDiemTeam += (itemAddDG.DiemTeam ?? 0);
                            }

                        }
                        else
                        {
                            if (TongDiemCS > 0)
                                itemAddDG.listDiem.Add(new HRM_TongHopDanhGiaNhanVienThang_Diem()
                                {
                                    Ten = "CS",
                                    Diem = TongDiemCS
                                });
                            if (TongDiemTeam > 0)
                            {
                                itemAddDG.TenTeamDG = "Team";
                                itemAddDG.DiemTeam = TongDiemTeam;
                            }
                        }
                        itemAdd.listTieuChi.Add(itemAddDG);
                    }
                    result.listNhanVien.Add(itemAdd);

                }

            }
            return result;
        }
        public List<clsTieuChiDanhGia_HeaderRow> GetTieuDeTongHop(ApplicationDbContext db, string IdThuVienDanhGia, List<HRM_ThuVienDanhGia_Item> dmTieuChis)
        {
            List<clsTieuChiDanhGia_HeaderRow> listTieuDe = new List<clsTieuChiDanhGia_HeaderRow>();
            List<HRM_ThuVienDanhGia_NhanSu> danhGia_NhanSus = db.HRM_ThuVienDanhGia_NhanSu.Where(x => x.IdThuVienDanhGia == IdThuVienDanhGia).ToList();
            var listPhongBanUser = _CallWebService.GetDanhSachBoPhanByUser(db);
            var nhanSus = _CallWebService.GetListNhanSu(db);
            var phongBans = _CallWebService.GetListPhongBan(db);

            List<HRM_ThuVienDanhGia_Item> dmParents = dmTieuChis.Where(x => x.IdParent == null).OrderBy(x => x.SapXep).ToList();
            List<string> listIdParent = dmParents.Select(a => a.IdGuid).Distinct().ToList();
            List<HRM_ThuVienDanhGia_Item> dmParentRow2s = dmTieuChis.Where(x => listIdParent.Contains(x.IdParent)).OrderBy(x => x.SapXep).ToList();
            List<string> listIdParent2 = dmParents.Select(a => a.IdGuid).Distinct().ToList();
            List<HRM_ThuVienDanhGia_Item> dmParentRow3s = dmTieuChis.Where(x => listIdParent2.Contains(x.IdParent)).OrderBy(x => x.SapXep).ToList();

            int? rowspan = 0;
            List<string> listIdNhomDanhGia = dmTieuChis.Select(x => x.IddmNhomDanhGia).Distinct().ToList();
            if (dmTieuChis.Find(x => x.IdParent != null) != null)
                rowspan = 2;
            else rowspan = 1;
            if (listIdNhomDanhGia.Count > 1)
                rowspan++;
            clsTieuChiDanhGia_HeaderRow row1 = new clsTieuChiDanhGia_HeaderRow();
            clsTieuChiDanhGia_HeaderRow row2 = new clsTieuChiDanhGia_HeaderRow();
            clsTieuChiDanhGia_HeaderRow row3 = new clsTieuChiDanhGia_HeaderRow();
            clsTieuChiDanhGia_HeaderCol itemSTT = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("STT", "STT"), rowspan, 1);
            row1.listCol.Add(itemSTT);
            clsTieuChiDanhGia_HeaderCol itemMa = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("Ma", "Mã nhân viên"), rowspan, 1);
            row1.listCol.Add(itemMa);
            clsTieuChiDanhGia_HeaderCol itemTen = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("Ten", "Tên nhân viên"), rowspan, 1);
            row1.listCol.Add(itemTen);


            List<HRM_dmNhomDanhGia> dmNhomDanhGias = db.HRM_dmNhomDanhGia.AsNoTracking().Where(x => listIdNhomDanhGia.Contains(x.IdGuid)).OrderBy(x => x.SapXep).ToList();
            int STT = 1;
            foreach (var item in dmNhomDanhGias)
            {
                List<HRM_ThuVienDanhGia_Item> dmParentFinds = dmParents.FindAll(x => x.IddmNhomDanhGia == item.IdGuid);
                if (dmParentFinds.Count == 0)
                    continue;

                int ColParent = 0;
                foreach (var dmParent in dmParentFinds)
                {
                    List<HRM_ThuVienDanhGia_Item> dmChilds = dmTieuChis.Where(x => x.IdParent == dmParent.IdGuid).OrderBy(x => x.SapXep).ToList();
                    clsTieuChiDanhGia_HeaderCol itemParent = new clsTieuChiDanhGia_HeaderCol(dmParent, 1, 1);
                    if (dmChilds.Count > 0)
                    {
                        itemParent.rowspan = 1;
                        itemParent.isHaveChild = true;
                        itemParent.colspan = dmChilds.Count;
                        foreach (var dmChild in dmChilds)
                        {
                            clsTieuChiDanhGia_HeaderCol Add = new clsTieuChiDanhGia_HeaderCol(dmChild, 1, 1);
                            Add.STT = STT;
                            row3.listCol.Add(Add);
                            ColParent++;
                            STT++;
                        }
                    }
                    else
                    {
                        if (dmParentRow3s != null && dmParentRow3s.Count > 0)
                            itemParent.rowspan = 2;
                        else itemParent.rowspan = 1;
                        ColParent++;
                    }
                    if (itemParent.colspan == 1)
                    {
                        itemParent.STT = STT;
                        STT++;
                    }
                    row2.listCol.Add(itemParent);

                }
                if (ColParent == 0)
                    ColParent = dmParentFinds.Count;


                clsTieuChiDanhGia_HeaderCol itemGrandParent = new clsTieuChiDanhGia_HeaderCol(item, 1, ColParent);
                if (ColParent == 1)
                    itemGrandParent.STT = STT;
                itemGrandParent.isHaveChild = true;
                row1.listCol.Add(itemGrandParent);
                STT++;
            }

            clsTieuChiDanhGia_HeaderCol itemTONG = new clsTieuChiDanhGia_HeaderCol(new HRM_ThuVienDanhGia_Item("TONG", "TỔNG"), rowspan, 1);
            itemTONG.STT = STT;
            itemTONG.isDisable = true;
            row1.listCol.Add(itemTONG);
            STT++;
            listTieuDe.Add(row1);
            listTieuDe.Add(row2);
            listTieuDe.Add(row3);
            return listTieuDe;
        }
        public dynamic ExportBaoCaoDanhGiaNhanVien(dynamic dyn, ApplicationDbContext db)
        {
            string sFile = "BaoCaoDanhGiaNhanVien.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "BaoCaoDanhGiaNhanVien.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            List<HRM_BaoCaoKetQuaKPI> entity = JsonConvert.DeserializeObject<List<HRM_BaoCaoKetQuaKPI>>(dyn.ToString());
            int RowBegin = 5;
            int nRow = RowBegin;
            int STT = 1;
            int nCol = 1;
            foreach (var item in entity)
            {
                int ncountBp = item.listChiTiet.Count - 1;
                nCol = 1;
                sheet1.Cells[nRow, nCol, nRow + ncountBp, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = STT;
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + ncountBp, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = item.ViTri;
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + ncountBp, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + ncountBp, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = item.HoVaTen;
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + ncountBp, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = item.NgayBatDau;
                nCol++;
                int Col = 0;
                foreach (var BP in item.listChiTiet)
                {
                    Col = nCol;
                    sheet1.Cells[nRow, Col].Value = BP.Ten;
                    Col++;
                    BP.listThang = BP.listThang.OrderBy(a => a.Thang).ToList();
                    foreach (var Thang in BP.listThang)
                    {
                        sheet1.Cells[nRow, Col].Value = Thang.Diem;
                        Col++;
                        sheet1.Cells[nRow, Col].Value = Thang.DanhGia;
                        Col++;
                    }
                    nRow++;
                }
                nCol += Col;
                STT++;
            }

            if (nRow > 5)
            {
                nRow--;
                nCol--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
            }
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet1.Cells[5, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet1.Cells[5, 4, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet1.Cells[5, 7, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;

            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic GetBaoCaoTongQuanNhanVien(dynamic dyn, ApplicationDbContext db)
        {
            TimKiemBangLuong TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiemBangLuong)) as TimKiemBangLuong;
            List<HRM_BaoCaoTongQuanDanhGiaNhanVien> listResult = new List<HRM_BaoCaoTongQuanDanhGiaNhanVien>();
            var listDatas = (from a in db.HRM_DanhGiaNhanVien_Item
                             join b in db.HRM_DanhGiaNhanVien on a.IdDanhGiaNhanVien equals b.IdGuid
                             where a.IdThuVienTieuChiDanhGia != "TONG" && b.isKetThuc == true
                             && (TimKiem.Nam == 0 || a.Ngay.Value.Year == TimKiem.Nam) && (TimKiem.Thang == 0 || a.Ngay.Value.Month == TimKiem.Thang)
                             select a).ToList();
            var listTuyenDung = db.HRM_TuyenDung_UngVien.Where(a => a.IsNghiSauKhiNhanViec == false && a.IsNhanViec == true && (TimKiem.Nam == 0 || a.NgayTiepNhan.Value.Year == TimKiem.Nam) && (TimKiem.Thang == 0 || a.NgayTiepNhan.Value.Month == TimKiem.Thang)).ToList();
            var NhuCaus = (from a in db.HRM_TuyenDung_NhuCau_ChiTiet
                           join b in db.HRM_TuyenDung_NhuCau on a.IdTuyenDungNhuCau equals b.IdGuid
                           where b.isKetThuc == true &&
                           (TimKiem.Nam == 0 || b.Ngay.Value.Year == TimKiem.Nam) && (TimKiem.Thang == 0 || b.Ngay.Value.Month == TimKiem.Thang)
                           select a).ToList();

            if (TimKiem.IdCongty > 0)
            {
                listDatas = listDatas.Where(a => a.IdCongTy == TimKiem.IdCongty).ToList();
                listTuyenDung = listTuyenDung.Where(a => a.IdCongTyEOS == TimKiem.IdCongty).ToList();
                NhuCaus = NhuCaus.Where(a => a.IdCongTyEOS == TimKiem.IdCongty).ToList();
            }
            if (TimKiem.listIdBoPhan != null && TimKiem.listIdBoPhan.Count > 0)
            {
                listDatas = listDatas.Where(a => TimKiem.listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                listTuyenDung = listTuyenDung.Where(a => TimKiem.listIdBoPhan.Contains(a.IdBoPhanEOS ?? 0)).ToList();
                NhuCaus = NhuCaus.Where(a => TimKiem.listIdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
            }
            List<long> listIdBoPhan = listDatas.Select(a => a.IdBoPhan).Distinct().ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            listBoPhan = listBoPhan.Where(a => listIdBoPhan.Contains(a.Id)).ToList();
            var listData = new List<HRM_DanhGiaNhanVien_Item>();
            List<string> listIdNhanViens = listDatas.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanViens.Contains(a.IdGuid) /*&& (string.IsNullOrEmpty(TimKiem.Keyword) || (!string.IsNullOrEmpty(TimKiem.Keyword) && (a.MaNhanVien.ToLower().Contains(TimKiem.Keyword.ToLower()) || a.TenDayDu.ToLower().Contains(TimKiem.Keyword.ToLower()))))*/).ToList();
            List<string> listId = listDatas.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
            var listTieuChiDanhGia = db.HRM_ThuVienDanhGia_Item.Where(a => listId.Contains(a.IdGuid)).ToList();
            foreach (var item in listNhanVien)
            {
                var listDataDG = listData.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                var listDataTeamDG = listDataDG.Where(a => a.isCoSo == false && a.IdNhanVien == item.IdGuid).ToList();
                var listDataQLCSDG = listDataDG.Where(a => a.isCoSo == true && string.IsNullOrEmpty(a.BoPhanDanhGia) && a.IdNhanVien == item.IdGuid).ToList();
                var listDataKhac = listDataDG.Where(a => a.isCoSo == true && !string.IsNullOrEmpty(a.BoPhanDanhGia) && a.IdNhanVien == item.IdGuid).ToList();

                HRM_DanhGiaNhanVien_Item Thang = new HRM_DanhGiaNhanVien_Item();
                Thang.IdNhanVien = item.IdGuid;
                Thang.IdBoPhan = item.IdBoPhanEOS;
                for (int i = 1; i <= 12; i++)
                {

                    var dataCS = listDataDG.Where(a => a.isCoSo == true && a.IdNhanVien == item.IdGuid && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                    var dataTeam = listDataTeamDG.Where(a => a.IdNhanVien == item.IdGuid && a.Ngay.Value.Month == i).OrderByDescending(a => a.Modified).ToList();
                    List<string> listIdTC = dataCS.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList();
                    listIdTC.AddRange(dataTeam.Select(a => a.IdThuVienTieuChiDanhGia).Distinct().ToList());
                    var listTieuChi = listTieuChiDanhGia.Where(a => listIdTC.Contains(a.IdGuid)).ToList();
                    Thang.Diem = 0;
                    foreach (var TC in listTieuChi)
                    {
                        double DiemCS = 0;
                        var CS = dataCS.Where(a => a.IdThuVienTieuChiDanhGia == TC.IdGuid).ToList();
                        if (CS != null)
                        {
                            if (CS != null && CS.Count > 0)
                                DiemCS += (CS.Sum(a => a.Diem ?? 0) / CS.Count);
                        }
                        var TeamDG = dataTeam.Where(a => a.IdThuVienTieuChiDanhGia == TC.IdGuid).ToList();
                        double DiemTeam = 0;
                        if (TeamDG != null)
                        {
                            if (TeamDG != null && TeamDG.Count > 0)
                                DiemTeam += (TeamDG.Sum(a => a.Diem ?? 0) / TeamDG.Count);
                        }
                        Thang.Diem += DiemCS * TC.TyTrongQLCS / 100.0 + DiemTeam * (100.0 - TC.TyTrongQLCS) / 100.0;
                    }
                }
                listData.Add(Thang);
            }
            foreach (var item in listBoPhan)
            {
                HRM_BaoCaoTongQuanDanhGiaNhanVien itemAdd = new HRM_BaoCaoTongQuanDanhGiaNhanVien();
                itemAdd.BoPhan = item.TenBoPhan;
                var NhuCau = NhuCaus.Where(a => a.IdBoPhanEOS == item.Id).ToList();
                double SlNhuCau = 0;
                if (NhuCau != null && NhuCau.Count > 0)
                    SlNhuCau = NhuCau.Sum(a => a.SoLuongBoSung);
                var TuyenDung = listTuyenDung.Where(a => a.IdBoPhanEOS == item.Id).ToList();
                itemAdd.SLTrungTuyen = TuyenDung.Count;
                if (itemAdd.SLTrungTuyen == 0)
                    itemAdd.SLTrungTuyen = null;
                if (SlNhuCau > 0 && itemAdd.SLTrungTuyen != null)
                    itemAdd.TyLeTrungTuyen = (itemAdd.SLTrungTuyen / SlNhuCau) * 100;
                if (listData != null)
                {
                    var Data = listData.Where(a => a.IdBoPhan == item.Id).Distinct().ToList();
                    List<string> listIdNhanVien = Data.Select(a => a.IdNhanVien).Distinct().ToList();
                    int SLNV = listIdNhanVien.Count;
                    itemAdd.SLCTru = Data.Where(a => a.Diem == null || a.Diem < 70).Count();
                    if (itemAdd.SLCTru == 0)
                        itemAdd.SLCTru = null;
                    else
                        itemAdd.TyLeCTru = (double)((itemAdd.SLCTru * 100) / SLNV);
                    itemAdd.SLC = Data.Where(a => a.Diem != null && a.Diem < 75 && a.Diem >= 70).Count();
                    if (itemAdd.SLC == 0)
                        itemAdd.SLC = null;
                    else
                        itemAdd.TyLeC = (double)((itemAdd.SLC * 100) / SLNV);
                    itemAdd.SLBTru = Data.Where(a => a.Diem != null && a.Diem < 80 && a.Diem >= 75).Count();
                    if (itemAdd.SLBTru == 0)
                        itemAdd.SLBTru = null;
                    else
                        itemAdd.TyLeBTru = (double)((itemAdd.SLBTru * 100) / SLNV);
                    itemAdd.SLB = Data.Where(a => a.Diem != null && a.Diem < 85 && a.Diem >= 80).Count();
                    if (itemAdd.SLB == 0)
                        itemAdd.SLB = null;
                    else
                        itemAdd.TyLeB = (double)(itemAdd.SLB / SLNV) * 100;
                    itemAdd.SLATru = Data.Where(a => a.Diem != null && a.Diem < 90 && a.Diem >= 85).Count();
                    if (itemAdd.SLATru == 0)
                        itemAdd.SLATru = null;
                    else
                        itemAdd.TyLeATru = (double)((itemAdd.SLATru * 100) / SLNV);
                    itemAdd.SLA = Data.Where(a => a.Diem != null && a.Diem < 95 && a.Diem >= 90).Count();
                    if (itemAdd.SLA == 0)
                        itemAdd.SLA = null;
                    else
                        itemAdd.TyLeA = (double)((itemAdd.SLA * 100) / SLNV);
                    itemAdd.SLACong = Data.Where(a => a.Diem != null && a.Diem >= 95).Count();
                    if (itemAdd.SLACong == 0)
                        itemAdd.SLACong = null;
                    else
                        itemAdd.TyLeACong = (double)((itemAdd.SLACong * 100) / SLNV);
                }
                listResult.Add(itemAdd);
            }
            return listResult;
        }
        public dynamic ExportBaoCaoTongQuanNhanVien(dynamic dyn, ApplicationDbContext db)
        {
            string sFile = "BaoCaoTongQuanNhanVien.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "BaoCaoTongQuanNhanVien.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            List<HRM_BaoCaoTongQuanDanhGiaNhanVien> entity = JsonConvert.DeserializeObject<List<HRM_BaoCaoTongQuanDanhGiaNhanVien>>(dyn.ToString());
            int RowBegin = 4;
            int nRow = RowBegin;
            int STT = 1;
            int nCol = 1;
            foreach (var item in entity)
            {
                nCol = 1;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = STT;
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = item.BoPhan;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLTrungTuyen;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeTrungTuyen;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLCTru;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeCTru;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLC;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeC;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLBTru;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeBTru;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLB;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeB;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLATru;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeATru;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLA;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeA;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SLACong;
                sheet1.Cells[nRow + 1, nCol].Value = item.TyLeACong;
                nRow = nRow + 2;
                STT++;
            }

            if (nRow > 4)
            {
                nRow--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
            }
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet1.Cells[4, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet1.Cells[4, 3, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;

            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportDanhSachDanhGiaNhanVienThang(HRM_TimKiemDanhGia dyn, ApplicationDbContext db)
        {
            string sFile = "DanhSachDanhGiaNhanVienThang.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "DanhSachDanhGiaNhanVienThang" + dt.ToOADate() + ".xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            dyn.CurrentPage = 0;
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            string a = JsonConvert.SerializeObject(GetDanhSachDanhGiaNhanVienThang(db, dyn));
            List<DanhSachNhanVienViewModel> entity = JsonConvert.DeserializeObject<List<DanhSachNhanVienViewModel>>(a.ToString());
            int RowBegin = 7;
            int nRow = RowBegin;
            int STT = 1;
            int nCol = 1;
            sheet1.Cells[4, 1].Value = "Tháng " + dyn.Thang + " năm " + dyn.Nam;
            if (entity != null && entity.Count > 0)
            {
                foreach (var item in entity)
                {
                    sheet1.Cells[nRow, nCol].Value = STT;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.DienThoai;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.CCCD;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TongSoCong;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[4, 3, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TongSoNgayNghi;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[4, 3, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.XepHangDanhGia;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nCol++;
                    nRow++;
                    STT++;
                }
            }
            if (nRow > 7)
            {
                nRow--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
            }
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;

            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic GetDanhSachDanhGiaNhanVienThang(ApplicationDbContext db, HRM_TimKiemDanhGia aRequest)
        {
            List<DanhSachNhanVienViewModel> listResult = new List<DanhSachNhanVienViewModel>();
            DateTime dTuNgay = new DateTime(aRequest.Nam, aRequest.Thang, 1);
            DateTime dDenNgay = dTuNgay.AddMonths(1).AddDays(-1);
            var listNhanVienDA = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(aRequest.IdCongTy, dungchung.DateTimeToUnixTimeStamp(dTuNgay), dungchung.DateTimeToUnixTimeStamp(dDenNgay), db).Where(a => a.isDieuDong == false).ToList();
            var listIdUser = listNhanVienDA.Select(a => a.IdUser).Distinct().ToList();
            List<DanhSachNhanVienViewModel> listNhanVien = (from a in db.HRM_NhanVien
                                                            where listIdUser.Contains(a.IdUser)
                                                            select new DanhSachNhanVienViewModel
                                                            {
                                                                IdGuid = a.IdGuid,
                                                                TenDayDu = a.TenDayDu,
                                                                IdCongTyEOS = a.IdCongTyEOS,
                                                                IdBoPhanEOS = a.IdBoPhanEOS,
                                                                IdChucVuEOS = a.IdChucVu,
                                                                IdNhanVienEOS = a.IdNhanVienEOS,
                                                                NgayBatDauLamViec = a.NgayBatDauLamViec,
                                                                TrangThai = a.TrangThai,
                                                                DienThoai = a.DienThoai,
                                                                GioiTinh = a.IdGioiTinh,
                                                                MaNhanVien = a.MaNhanVien,
                                                                NgaySinh = a.NgaySinh,
                                                                //SoChungMinhThu = a.SoChungMinhThu,
                                                                NguyenQuan = a.NguyenQuan,
                                                                NgayKetThucLamViec = a.NgayNghiViec,
                                                                IdChucDanhEOS = a.IdChucDanh
                                                            }
                                                              ).OrderBy(a => a.MaNhanVien).ToList();
            if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhan > 0)
            {
                listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhan).ToList();
            }
            if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
            {
                listNhanVien = listNhanVien.Where(a => (a.TenDayDu.ToLower().Contains(aRequest.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.DienThoai) && a.DienThoai.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())))).ToList();
            }
            var lstCongTyAll = _CallWebService.GetDanhSachCongTy(db);

            int nCurrentPage = aRequest.CurrentPage;
            int PageSize = aRequest.PageSize ?? 25;
            int count = listNhanVien.Count();
            if (aRequest.CurrentPage > 0)
            {

                listNhanVien = listNhanVien.Take(PageSize).Skip((nCurrentPage - 1) * PageSize).ToList();
            }
            List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
            var listNV_CongTy = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
            var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
            var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true && a.Loai != 3 && a.Loai != 4).ToList();
            List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                 && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
            List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut
                                                   where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.I.ToString()
                                                                 && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                   group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                   select g.OrderBy(a => a.Gio).FirstOrDefault()).ToList();
            listCheckInOut.AddRange((from x in db.HRM_CheckInOut
                                     where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                                                   && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                     group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                     select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());

            List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                          && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
            List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                           && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
            List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                                                                                  && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
            List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                          && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();
            var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
            if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
            {
                foreach (var item in listXinNghiPhep)
                {
                    var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                    if (hinhThucNghi != null)
                        item.HinhThucNghi = hinhThucNghi.Ma;
                }
            }
            var listCaLamViecs = db.HRM_dmLichLamViec.ToList();
            List<HRM_BangChamCong_NhanVien_ThoiGian> listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
            List<HRM_BangChamCongNhanVien_TangCa> listDataTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
            List<HRM_BangChamCongNhanVien_SoGioLamViec> listDataTieuChuan = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
            var funcKeHoach = this;
            List<double> listHeSo = listLichLamViec.Where(a => a.Loai == 1).Select(a => a.HeSoTangCa).Distinct().ToList();
            List<double> listCaTieuChuan = listLichLamViec.Select(a => a.GioTieuChuan).Distinct().ToList();
            DateTime dateNow = DateTime.Now;
            dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
            var listIdHinhThucNghi = listHinhThucNghi.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString()).Select(a => a.Id).ToList();
            string VeSom = HrmOptions.SoPhutSom.ToString();
            double SoPhutDuocVeSom = double.Parse(VeSom);
            string DiMuon = HrmOptions.SoPhutMuon.ToString();
            double SoPhutDuocDiMuon = double.Parse(DiMuon);
            foreach (var item in listNhanVien)
            {
                if (listNV_CongTy != null)
                {
                    var listIdCongTys = listNV_CongTy.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
                    item.TenCongTy = string.Join(",", lstCongTyAll.Where(q => listIdCongTys.Contains(q.Id)).Select(q => q.TenBoPhan).ToList());
                }
                item.DienThoai = listSDT.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.SoDienThoai).FirstOrDefault();
                item.CCCD = listGiayTo.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.So).FirstOrDefault();
                DateTime dt = dTuNgay;
                if (listHeSo != null && listHeSo.Count > 0)
                {
                    foreach (var heso in listHeSo)
                    {
                        HRM_BangChamCongNhanVien_TangCa hesoAdd = new HRM_BangChamCongNhanVien_TangCa();
                        hesoAdd.HeSo = heso;
                        hesoAdd.IdNhanVien = item.IdGuid;
                        var tangca = listLichLamViec.Where(a => a.IdNhanVien == item.IdGuid && a.HeSoTangCa == heso && a.Loai == 1).ToList();
                        if (tangca != null && tangca.Count > 0)
                        {
                            hesoAdd.TongSoGio = tangca.Sum(a => (a.GioRa - a.GioVao).TotalHours);
                            hesoAdd.Cong = tangca.Sum(a => ((a.GioRa - a.GioVao).TotalHours) / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                            hesoAdd.IddmHeSoTangCa = tangca.FirstOrDefault().IddmHeSoTangCa;
                        }
                        listDataTangCa.Add(hesoAdd);
                    }
                }
                if (listCaTieuChuan != null && listCaTieuChuan.Count > 0)
                {
                    foreach (var Id in listCaTieuChuan)
                    {
                        HRM_BangChamCongNhanVien_SoGioLamViec hesoAdd = new HRM_BangChamCongNhanVien_SoGioLamViec();
                        var caLamViec = listCaLamViecs.FirstOrDefault(a => a.SoGioTrongNgay == Id);
                        if (caLamViec != null)
                        {
                            hesoAdd.IddmCaLamViec = caLamViec.IdGuid;
                            hesoAdd.IdNhanVien = item.IdGuid;
                            hesoAdd.GioTieuChuan = caLamViec.SoGioTrongNgay ?? 8;
                            hesoAdd.CongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 26;
                            hesoAdd.TongSoGio = 0;
                            listDataTieuChuan.Add(hesoAdd);
                        }
                    }
                }
                List<long> listIdCongTy = listCheckInOut.Where(x => x.IdNhanVien == item.IdGuid).OrderBy(a => a.Ngay).Select(a => a.IdCongTy).ToList();
                foreach (long IdCongTy in listIdCongTy)
                {
                    var CTY = lstCongTyAll.FirstOrDefault(a => a.Id == IdCongTy);
                    while (dt <= dDenNgay)
                    {
                        if (dt >= dateNow)
                        {
                            dt = dt.AddDays(1);
                            continue;
                        }
                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                        List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt);
                        List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt).OrderBy(a => a.Ngay).ToList();
                        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);


                        if (listCheckInOutFind != null && listCheckInOutFind.Count > 0)
                        {
                            //if (listCheckInOutFind.LastOrDefault().Loai == "I")
                            //{
                            //    DateTime dta = dt.AddDays(1);
                            //    listCheckInOutFind.Add(listCheckInOut.FindAll(x => x.Loai == "O" && x.IdNhanVien == item.IdGuid && x.Ngay == dta).OrderBy(a => a.Ngay).FirstOrDefault());
                            //}

                            if (listLichLamViecFind == null)
                                listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();
                            string IdCaLamViec = listLichLamViecFind.Where(a => a.IdCongTy == IdCongTy).Select(a => a.IddmCaLamViec).FirstOrDefault();
                            var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);
                            List<clsDiMuonVeSom> listSomMuon = new List<clsDiMuonVeSom>();
                            if (CTY.MaBoPhan == "VPCT")
                                listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, 0, 0, "", NghiPheps, false, IdCongTy);
                            else
                                listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, CTY.MaBoPhan, NghiPheps, false, IdCongTy);

                            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecFindNV = listLichLamViecNV.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                            if (listLichLamViecFind.Count >= 1)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                bool TangCaDem = false;
                                foreach (var KeHoach in listLichLamViecFind)
                                {

                                    CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == KeHoach.IddmCaLamViec);
                                    if (KeHoach.IdCongTy != IdCongTy)
                                        continue;
                                    double SoGioLamViec = KeHoach.TongSoGioGiao;
                                    if (KeHoach.Loai == 0)
                                    {
                                        itemAdd.SoGioLamViec += SoGioLamViec;
                                        if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                        {
                                            var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == KeHoach.GioTieuChuan);
                                            if (tangCa != null)
                                                tangCa.TongSoGio += SoGioLamViec;
                                        }
                                    }
                                    else
                                    {
                                        itemAdd.SoGioLamViecTangCa += SoGioLamViec;
                                        if (KeHoach.GioVao.Hour >= 21)
                                        {
                                            TangCaDem = true;

                                        }
                                        if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                        {
                                            var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == KeHoach.IddmHeSoTangCa);
                                            if (tangCa != null)
                                                tangCa.TongSoGio += SoGioLamViec;
                                        }
                                    }
                                    itemAdd.SoGioTieuChuan = CaLamViec.SoGioTrongNgay ?? 8;
                                }
                                List<string> listKyHieu = new List<string>();
                                if (listSomMuon != null && listSomMuon.Count > 0)
                                    foreach (var SomMuon in listSomMuon)
                                    {
                                        //itemAdd.GioLamViec += "; ";
                                        double Gio = SomMuon.SoPhut / 60.0;
                                        if (SomMuon.Loai == 0)
                                        {
                                            //if (SomMuon.isCaGay == true)
                                            //    itemAdd.SoGioLamViecCaGay -= Gio;
                                            //else
                                            itemAdd.SoGioLamViec -= Gio;
                                            if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == SomMuon.GioTieuChuan);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio -= Gio;
                                                if (tangCa.TongSoGio < 0)
                                                    tangCa.TongSoGio = 0;
                                            }
                                        }
                                        else
                                        {
                                            itemAdd.SoGioLamViecTangCa -= Gio;
                                            if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == SomMuon.IddmHeSoTangCa);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio -= Gio;
                                            }
                                        }
                                        listKyHieu.Add(SomMuon.KyHieuChamCong);
                                    }
                                double GioLamViec = (double)(itemAdd.SoGioLamViecCaGay + itemAdd.SoGioLamViec);
                                string KyHieu = "";
                                if (listKyHieu != null && listKyHieu.Count > 0)
                                    KyHieu += string.Join("", listKyHieu);
                                if (GioLamViec > 0)
                                    KyHieu += Math.Round(GioLamViec, 2);
                                if (CaLamViec != null && CaLamViec.TuGio.Hour >= 21)
                                {
                                    itemAdd.isCaDem = true;
                                }
                                if (itemAdd.SoGioLamViecTangCa > 0)
                                {
                                    if (TangCaDem == false)
                                    {
                                        KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "";
                                    }
                                    else
                                    {
                                        if (listLichLamViecFind.Count > 1)
                                            KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                        else if (listLichLamViecFind.Count == 1)
                                        {
                                            if (GioLamViec > 0)
                                                KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                            else
                                                KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                        }
                                    }
                                }
                                itemAdd.GioLamViec = KyHieu;
                                listData.Add(itemAdd);
                            }
                        }
                        else if (NghiPheps.Count > 0)
                        {
                            if (listLichLamViecFind == null)
                                listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();


                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == NghiPheps.FirstOrDefault().HinhThucNghi);
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                double SoGioLamViec = 0;
                                foreach (var KeHoach in listLichLamViecFind)
                                {
                                    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;

                                }
                                itemAdd.GioLamViec = NghiPhep.Ma /*+ SoGioLamViec*/;
                                listData.Add(itemAdd);
                            }
                        }
                        else if ((int)dt.DayOfWeek != 0 && listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                        {
                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                double SoGioLamViec = 0;
                                foreach (var KeHoach in listLichLamViecFind)
                                {
                                    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;
                                }
                                itemAdd.GioLamViec = NghiPhep.Ma;
                                listData.Add(itemAdd);
                            }
                        }
                        dt = dt.AddDays(1);
                    }
                }
                if (listIdCongTy != null && listIdCongTy.Count > 0)
                {
                    var listNgayNghi = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid);
                    item.TongSoCong = listDataTieuChuan.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.TongSoGio / a.GioTieuChuan);

                    item.TongSoNgayNghi = listNgayNghi.Where(a => listIdHinhThucNghi.Contains(a.IdHinhThucNghi)).Sum(a => a.ThoiGianNghi);
                    if (listData != null && listData.Count > 0)
                    {
                        List<string> listTenCty = new List<string>();
                        foreach (var itemCty in lstCongTyAll)
                        {
                            double? data = listData.Where(a => a.IdNhanVien == item.IdGuid && a.IdCongTyEOS == itemCty.Id).Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan == 0 ? 8 : a.SoGioTieuChuan));
                            if (data != null && data > 0)
                            {
                                listTenCty.Add(itemCty.TenBoPhan + ":" + data + " Công");
                            }

                        }
                        item.TenCongTy = string.Join(", ", listTenCty);
                    }
                    var DuAn = _CallWebService.GetThongTinCaLamViecDuAn(listIdCongTy.LastOrDefault(), db);
                    if (DuAn.itemThongTinDuAn.listHoTroChuyenCan != null && DuAn.itemThongTinDuAn.listHoTroChuyenCan.Count() > 0)
                    {
                        var ChuyenCan = DuAn.itemThongTinDuAn.listHoTroChuyenCan.FirstOrDefault(a => a.CongToiThieu <= item.TongSoCong && a.CongToiDa >= item.TongSoCong);
                        if (ChuyenCan == null)
                        {
                            ChuyenCan = DuAn.itemThongTinDuAn.listHoTroChuyenCan.FirstOrDefault(a => a.CongToiDa < item.TongSoCong);
                            if (ChuyenCan != null)
                                item.XepHangDanhGia = ChuyenCan.XepHangDanhGia;
                            else
                                item.XepHangDanhGia = DuAn.itemThongTinDuAn.listHoTroChuyenCan.OrderBy(a => a.CongToiThieu).FirstOrDefault().XepHangDanhGia;
                        }
                        else
                            item.XepHangDanhGia = ChuyenCan.XepHangDanhGia;
                    }

                }
            }
            if (nCurrentPage > 0)
            {
                dynamic result = new DynamicObject();
                result.paging = new PagingInfor(nCurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
                result.items = listNhanVien;
                //result.value = lstBoPhanAll;
                return result;
            }
            else
                return listNhanVien;
        }
        #endregion
        #region Đánh giá nhân viên
        public dynamic GetNextMaQuyTrinhDanhGia(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyTrinhDanhGia.Select(x => x.SoQuyTrinh), DefineData.MaQuyTrinhDanhGia));
        public dynamic GetListQuyTrinhDanhGia(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_QuyTrinhDanhGia> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_QuyTrinhDanhGia
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhDanhGia
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy != null && TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);

                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> CongTys = _CallWebService.GetDanhSachCongTy(db);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                List<ChucVu> ChucVus = _CallWebService.GetDanhSachChucVu(db);
                List<string> listId = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listItem = db.HRM_QuyTrinhDanhGia_NhanVien.Where(a => listId.Contains(a.IdQuyTrinh)).ToList();
                var listIdNv = listItem.Select(a => a.IdNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();
                foreach (var item in listItem)
                {
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    item.MaNhanVien = NhanVien.MaNhanVien;
                    item.TenNhanVien = NhanVien.TenDayDu;
                }
                foreach (HRM_QuyTrinhDanhGia item in itemList)
                {

                    BoPhan duAn = CongTys.Find(x => x.Id == item.IdCongTy);
                    if (duAn != null)
                        item.TenCongTy = duAn.TenBoPhan;

                    BoPhan BoPhan = BoPhans.Find(x => x.Id == item.IdBoPhan);
                    if (BoPhan != null)
                        item.TenBoPhan = BoPhan.TenBoPhan;
                    ChucVu ChucVu = ChucVus.Find(x => x.Id == item.IdChucVu);
                    if (ChucVu != null)
                        item.TenChucVu = ChucVu.TenChucVu;
                    item.listNhanVien = listItem.Where(a => a.IdQuyTrinh == item.IdGuid).ToList();
                    if (item.listNhanVien != null && item.listNhanVien.Count > 0)
                    {
                        item.listNoiDung = item.listNhanVien.Select(a => (a.TenNhanVien + "(" + a.MaNhanVien + ") " + a.KetQuaThucTe + "/" + a.KetQuaTheoNgayCong)).ToList();
                    }

                }

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
        public dynamic SetQuyTrinhDanhGia(HRM_QuyTrinhDanhGia model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    // DI: use inherited _CallWebService
                    var QuyTrinhdb = db.HRM_QuyTrinhDanhGia.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncQuyTrinhDanhGia(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, model, err))
                    {
                        ts.Rollback();
                        return err;
                    }


                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);
                }
                catch (Exception exx)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhDanhGia(HRM_QuyTrinhDanhGia QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_QuyTrinhDanhGia pcuDatabase = db.HRM_QuyTrinhDanhGia.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    bool Error = FuncQuyTrinhDanhGia(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!base.ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");

                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhDanhGia(HRM_QuyTrinhDanhGia QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhDanhGia pcuDatabase = db.HRM_QuyTrinhDanhGia.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = FuncQuyTrinhDanhGia(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!base.KhongDuyetSauHamUpdate(db, QuyTrinh))
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
        public dynamic GetByIdQuyTrinhDanhGia(string Id, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                HRM_QuyTrinhDanhGia model = db.HRM_QuyTrinhDanhGia.Find(Id);
                model.NgayDanhGiaUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayDanhGia);

                model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_QuyTrinhDanhGia.ToString(), Id);
                model.listNhanVien = db.HRM_QuyTrinhDanhGia_NhanVien.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                if (model.listNhanVien != null && model.listNhanVien.Count > 0)
                {
                    List<string> listIdNhanVien = model.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    foreach (var item in model.listNhanVien)
                    {
                        var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        if (NhanVien != null)
                        {
                            item.MaNhanVien = NhanVien.MaNhanVien;
                            item.TenNhanVien = NhanVien.TenDayDu;
                            item.NgaySinh = NhanVien.NgaySinh;
                        }
                    }
                }


                err.Data = model;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }
        public class DanhGiaNhanVien
        {
            public long IdCongTy { get; set; }
            public long IdBoPhan { get; set; }
            public long IdChucvu { get; set; }
            public int Nam { get; set; }
            public int Thang { get; set; }
        }
        public List<HRM_QuyTrinhDanhGia_NhanVien> GetDanhSachNhanVienForDanhGia(ApplicationDbContext db, long IdCongty, long IdBoPhan, long IdChucVu, int Nam, int Thang)
        {
            List<HRM_QuyTrinhDanhGia_NhanVien> listResult = new List<HRM_QuyTrinhDanhGia_NhanVien>();
            {
                DateTime dTuNgay = new DateTime(Nam, Thang, 1, 0, 0, 0);
                DateTime dDenNgay = dTuNgay.AddMonths(1).AddDays(-1);

                var listNhanVienDA = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongty, dungchung.DateTimeToUnixTimeStamp(dTuNgay), dungchung.DateTimeToUnixTimeStamp(dDenNgay), db).Where(a => a.isDieuDong == false).ToList();
                var listIdUser = listNhanVienDA.Select(a => a.IdUser).Distinct().ToList();
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in db.HRM_NhanVien
                                                                where listIdUser.Contains(a.IdUser)
                                                                select new DanhSachNhanVienViewModel
                                                                {
                                                                    IdGuid = a.IdGuid,
                                                                    TenDayDu = a.TenDayDu,
                                                                    IdCongTyEOS = a.IdCongTyEOS,
                                                                    IdBoPhanEOS = a.IdBoPhanEOS,
                                                                    IdChucVuEOS = a.IdChucVu,
                                                                    IdNhanVienEOS = a.IdNhanVienEOS,
                                                                    NgayBatDauLamViec = a.NgayBatDauLamViec,
                                                                    TrangThai = a.TrangThai,
                                                                    DienThoai = a.DienThoai,
                                                                    GioiTinh = a.IdGioiTinh,
                                                                    MaNhanVien = a.MaNhanVien,
                                                                    NgaySinh = a.NgaySinh,
                                                                    //SoChungMinhThu = a.SoChungMinhThu,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh
                                                                }
                                                                  ).OrderBy(a => a.MaNhanVien).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0 && IdBoPhan > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == IdBoPhan).ToList();
                }
                var lstCongTyAll = _CallWebService.GetDanhSachCongTy(db);
                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
                var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true && a.Loai != 3 && a.Loai != 4).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut
                                                       where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.I.ToString()
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                       group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                       select g.OrderBy(a => a.Gio).FirstOrDefault()).ToList();
                listCheckInOut.AddRange((from x in db.HRM_CheckInOut
                                         where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                                                       && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                         group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                         select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());

                List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                               && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
                List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                                                                                      && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                {
                    foreach (var item in listXinNghiPhep)
                    {
                        var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                        if (hinhThucNghi != null)
                            item.HinhThucNghi = hinhThucNghi.Ma;
                    }
                }
                var listCaLamViecs = db.HRM_dmLichLamViec.ToList();
                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                List<HRM_BangChamCongNhanVien_TangCa> listDataTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                List<HRM_BangChamCongNhanVien_SoGioLamViec> listDataTieuChuan = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
                var funcKeHoach = this;
                List<double> listHeSo = listLichLamViec.Where(a => a.Loai == 1).Select(a => a.HeSoTangCa).Distinct().ToList();
                List<double> listCaTieuChuan = listLichLamViec.Select(a => a.GioTieuChuan).Distinct().ToList();
                DateTime dateNow = DateTime.Now;
                dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                var DuAn1 = _CallWebService.GetQuyTrinhDuAn(IdCongty, db);
                if (DuAn1 == null)
                    DuAn1 = new BoPhan();
                string VeSom = HrmOptions.SoPhutSom.ToString();
                double SoPhutDuocVeSom = double.Parse(VeSom);
                string DiMuon = HrmOptions.SoPhutMuon.ToString();
                double SoPhutDuocDiMuon = double.Parse(DiMuon);
                if (DuAn1.MaBoPhan == "VPCT")
                {
                    SoPhutDuocDiMuon = 0;
                    SoPhutDuocVeSom = 0;
                }
                var listIdHinhThucNghi = listHinhThucNghi.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString()).Select(a => a.Id).ToList();
                foreach (var item in listNhanVien)
                {
                    item.TenCongTy = lstCongTyAll.Where(q => q.Id == item.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                    item.DienThoai = listSDT.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.SoDienThoai).FirstOrDefault();
                    item.CCCD = listGiayTo.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.So).FirstOrDefault();
                    DateTime dt = dTuNgay;
                    if (listHeSo != null && listHeSo.Count > 0)
                    {
                        foreach (var heso in listHeSo)
                        {
                            HRM_BangChamCongNhanVien_TangCa hesoAdd = new HRM_BangChamCongNhanVien_TangCa();
                            hesoAdd.HeSo = heso;
                            hesoAdd.IdNhanVien = item.IdGuid;
                            var tangca = listLichLamViec.Where(a => a.IdNhanVien == item.IdGuid && a.HeSoTangCa == heso && a.Loai == 1).ToList();
                            if (tangca != null && tangca.Count > 0)
                            {
                                hesoAdd.TongSoGio = tangca.Sum(a => (a.GioRa - a.GioVao).TotalHours);
                                hesoAdd.Cong = tangca.Sum(a => ((a.GioRa - a.GioVao).TotalHours) / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                                hesoAdd.IddmHeSoTangCa = tangca.FirstOrDefault().IddmHeSoTangCa;
                            }
                            listDataTangCa.Add(hesoAdd);
                        }
                    }
                    if (listCaTieuChuan != null && listCaTieuChuan.Count > 0)
                    {
                        foreach (var Id in listCaTieuChuan)
                        {
                            HRM_BangChamCongNhanVien_SoGioLamViec hesoAdd = new HRM_BangChamCongNhanVien_SoGioLamViec();
                            var caLamViec = listCaLamViecs.FirstOrDefault(a => a.SoGioTrongNgay == Id);
                            if (caLamViec != null)
                            {
                                hesoAdd.IddmCaLamViec = caLamViec.IdGuid;
                                hesoAdd.IdNhanVien = item.IdGuid;
                                hesoAdd.GioTieuChuan = caLamViec.SoGioTrongNgay ?? 8;
                                hesoAdd.CongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 26;
                                hesoAdd.TongSoGio = 0;
                                listDataTieuChuan.Add(hesoAdd);
                            }
                        }
                    }
                    List<long> listIdCongTy = listCheckInOut.Where(x => x.IdNhanVien == item.IdGuid).OrderBy(a => a.Ngay).Select(a => a.IdCongTy).ToList();
                    foreach (long IdCongTy in listIdCongTy)
                    {
                        while (dt <= dDenNgay)
                        {
                            if (dt >= dateNow)
                            {
                                dt = dt.AddDays(1);
                                continue;
                            }
                            List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                            List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt);
                            List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt).OrderBy(a => a.Ngay).ToList();
                            List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);


                            if (listCheckInOutFind != null && listCheckInOutFind.Count > 0)
                            {
                                //if (listCheckInOutFind.LastOrDefault().Loai == "I")
                                //{
                                //    DateTime dta = dt.AddDays(1);
                                //    listCheckInOutFind.Add(listCheckInOut.FindAll(x => x.Loai == "O" && x.IdNhanVien == item.IdGuid && x.Ngay == dta).OrderBy(a => a.Ngay).FirstOrDefault());
                                //}

                                if (listLichLamViecFind == null)
                                    listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();
                                string IdCaLamViec = listLichLamViecFind.Where(a => a.IdCongTy == IdCongTy).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);

                                List<clsDiMuonVeSom> listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, "", NghiPheps, false, IdCongTy);
                                List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecFindNV = listLichLamViecNV.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                                if (listLichLamViecFind.Count >= 1)
                                {
                                    var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    itemAdd.IdNhanVien = item.IdGuid;
                                    itemAdd.MaNhanVien = item.MaNhanVien;
                                    itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                    itemAdd.SoGioLamViec = 0;
                                    itemAdd.SoGioLamViecCaGay = 0;
                                    itemAdd.SoGioLamViecTangCa = 0;
                                    itemAdd.NgayChamCong = dt;
                                    itemAdd.IdCongTyEOS = IdCongTy;
                                    itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                    bool TangCaDem = false;
                                    foreach (var KeHoach in listLichLamViecFind)
                                    {

                                        CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == KeHoach.IddmCaLamViec);
                                        if (KeHoach.IdCongTy != IdCongTy)
                                            continue;
                                        double SoGioLamViec = KeHoach.TongSoGioGiao;
                                        if (KeHoach.Loai == 0)
                                        {
                                            itemAdd.SoGioLamViec += SoGioLamViec;
                                            if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == KeHoach.GioTieuChuan);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio += SoGioLamViec;
                                            }
                                        }
                                        else
                                        {
                                            itemAdd.SoGioLamViecTangCa += SoGioLamViec;
                                            if (KeHoach.GioVao.Hour >= 21)
                                            {
                                                TangCaDem = true;

                                            }
                                            if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == KeHoach.IddmHeSoTangCa);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio += SoGioLamViec;
                                            }
                                        }
                                        itemAdd.SoGioTieuChuan = CaLamViec.SoGioTrongNgay ?? 8;
                                    }
                                    List<string> listKyHieu = new List<string>();
                                    if (listSomMuon != null && listSomMuon.Count > 0)
                                        foreach (var SomMuon in listSomMuon)
                                        {
                                            //itemAdd.GioLamViec += "; ";
                                            double Gio = SomMuon.SoPhut / 60.0;
                                            if (SomMuon.Loai == 0)
                                            {
                                                //if (SomMuon.isCaGay == true)
                                                //    itemAdd.SoGioLamViecCaGay -= Gio;
                                                //else
                                                itemAdd.SoGioLamViec -= Gio;
                                                if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                                {
                                                    var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == SomMuon.GioTieuChuan);
                                                    if (tangCa != null)
                                                        tangCa.TongSoGio -= Gio;
                                                    if (tangCa.TongSoGio < 0)
                                                        tangCa.TongSoGio = 0;
                                                }
                                            }
                                            else
                                            {
                                                itemAdd.SoGioLamViecTangCa -= Gio;
                                                if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                                {
                                                    var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == SomMuon.IddmHeSoTangCa);
                                                    if (tangCa != null)
                                                        tangCa.TongSoGio -= Gio;
                                                }
                                            }
                                            listKyHieu.Add(SomMuon.KyHieuChamCong);
                                        }
                                    double GioLamViec = (double)(itemAdd.SoGioLamViecCaGay + itemAdd.SoGioLamViec);
                                    string KyHieu = "";
                                    if (listKyHieu != null && listKyHieu.Count > 0)
                                        KyHieu += string.Join("", listKyHieu);
                                    if (GioLamViec > 0)
                                        KyHieu += Math.Round(GioLamViec, 2);
                                    if (CaLamViec != null && CaLamViec.TuGio.Hour >= 21)
                                    {
                                        itemAdd.isCaDem = true;
                                    }
                                    if (itemAdd.SoGioLamViecTangCa > 0)
                                    {
                                        if (TangCaDem == false)
                                        {
                                            KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "";
                                        }
                                        else
                                        {
                                            if (listLichLamViecFind.Count > 1)
                                                KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                            else if (listLichLamViecFind.Count == 1)
                                            {
                                                if (GioLamViec > 0)
                                                    KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                                else
                                                    KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                            }
                                        }
                                    }
                                    itemAdd.GioLamViec = KyHieu;
                                    listData.Add(itemAdd);
                                }
                            }
                            else if (NghiPheps.Count > 0)
                            {
                                if (listLichLamViecFind == null)
                                    listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();


                                HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == NghiPheps.FirstOrDefault().HinhThucNghi);
                                if (NghiPhep != null)
                                {
                                    var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    itemAdd.IdNhanVien = item.IdGuid;
                                    itemAdd.MaNhanVien = item.MaNhanVien;
                                    itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                    itemAdd.SoGioLamViec = 0;
                                    itemAdd.SoGioLamViecCaGay = 0;
                                    itemAdd.SoGioLamViecTangCa = 0;
                                    itemAdd.NgayChamCong = dt;
                                    itemAdd.IdCongTyEOS = IdCongTy;
                                    itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                    double SoGioLamViec = 0;
                                    foreach (var KeHoach in listLichLamViecFind)
                                    {
                                        SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;

                                    }
                                    itemAdd.GioLamViec = NghiPhep.Ma /*+ SoGioLamViec*/;
                                    listData.Add(itemAdd);
                                }
                            }
                            else if ((int)dt.DayOfWeek != 0 && listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                            {
                                HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                                if (NghiPhep != null)
                                {
                                    var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    itemAdd.IdNhanVien = item.IdGuid;
                                    itemAdd.MaNhanVien = item.MaNhanVien;
                                    itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                    itemAdd.SoGioLamViec = 0;
                                    itemAdd.SoGioLamViecCaGay = 0;
                                    itemAdd.SoGioLamViecTangCa = 0;
                                    itemAdd.NgayChamCong = dt;
                                    itemAdd.IdCongTyEOS = IdCongTy;
                                    itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                    double SoGioLamViec = 0;
                                    foreach (var KeHoach in listLichLamViecFind)
                                    {
                                        SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;
                                    }
                                    itemAdd.GioLamViec = NghiPhep.Ma;
                                    listData.Add(itemAdd);
                                }
                            }
                            dt = dt.AddDays(1);
                        }
                    }
                    if (listIdCongTy != null && listIdCongTy.Count > 0)
                    {
                        HRM_QuyTrinhDanhGia_NhanVien itemAdd = new HRM_QuyTrinhDanhGia_NhanVien();
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.NgaySinh = item.NgaySinh;
                        var listNgayNghi = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid);
                        item.TongSoCong = listDataTieuChuan.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.TongSoGio / a.GioTieuChuan);

                        item.TongSoNgayNghi = listNgayNghi.Where(a => listIdHinhThucNghi.Contains(a.IdHinhThucNghi)).Sum(a => a.ThoiGianNghi);
                        if (listData != null && listData.Count > 0)
                        {
                            List<string> listTenCty = new List<string>();
                            foreach (var itemCty in lstCongTyAll)
                            {
                                double? data = listData.Where(a => a.IdNhanVien == item.IdGuid && a.IdCongTyEOS == itemCty.Id).Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan == 0 ? 8 : a.SoGioTieuChuan));
                                if (data != null && data > 0)
                                {
                                    listTenCty.Add(itemCty.TenBoPhan + ":" + data + " Công");
                                }

                            }
                            itemAdd.TenCongTy = string.Join(", ", listTenCty);
                        }
                        var DuAn = _CallWebService.GetThongTinCaLamViecDuAn(listIdCongTy.LastOrDefault(), db);
                        if (DuAn.itemThongTinDuAn != null && DuAn.itemThongTinDuAn.listHoTroChuyenCan != null && DuAn.itemThongTinDuAn.listHoTroChuyenCan.Count() > 0)
                        {
                            var ChuyenCan = DuAn.itemThongTinDuAn.listHoTroChuyenCan.FirstOrDefault(a => a.CongToiThieu <= item.TongSoCong && a.CongToiDa >= item.TongSoCong);
                            if (ChuyenCan == null)
                            {
                                ChuyenCan = DuAn.itemThongTinDuAn.listHoTroChuyenCan.FirstOrDefault(a => a.CongToiDa < item.TongSoCong);
                                if (ChuyenCan != null)
                                    itemAdd.KetQuaTheoNgayCong = ChuyenCan.XepHangDanhGia;
                                else
                                    itemAdd.KetQuaTheoNgayCong = DuAn.itemThongTinDuAn.listHoTroChuyenCan.OrderBy(a => a.CongToiThieu).FirstOrDefault().XepHangDanhGia;
                            }
                            else
                                itemAdd.KetQuaTheoNgayCong = ChuyenCan.XepHangDanhGia;
                        }
                        listResult.Add(itemAdd);
                    }
                }
            }
            return listResult;
        }
        public dynamic DeleteQuyTrinhDanhGia(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_QuyTrinhDanhGia model = db.HRM_QuyTrinhDanhGia.Find(Id);
                var listCalamViec = db.HRM_QuyTrinhDanhGia_NhanVien.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                db.HRM_QuyTrinhDanhGia_NhanVien.RemoveRange(listCalamViec);
                db.HRM_QuyTrinhDanhGia.Remove(model);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public bool FuncQuyTrinhDanhGia(HRM_QuyTrinhDanhGia model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.NgayDanhGia = dungchung.UnixTimeStampToDateTime(model.NgayDanhGiaUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.eAction = DefineData.eAction.DANHGIANHANVIEN.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_QuyTrinhDanhGia.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var listCalamViec = db.HRM_QuyTrinhDanhGia_NhanVien.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
                db.HRM_QuyTrinhDanhGia_NhanVien.RemoveRange(listCalamViec);
                if (model.listNhanVien != null && model.listNhanVien.Count > 0)
                {
                    int stt = 1;
                    foreach (var item in model.listNhanVien)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        item.isKetThuc = model.isKetThuc;
                        item.Nam = model.Nam;
                        item.Thang = model.Thang;
                        stt++;
                    }
                    db.HRM_QuyTrinhDanhGia_NhanVien.AddRange(model.listNhanVien);
                }
                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_QuyTrinhDanhGia, model.IdGuid);
                    }
                }
                if (!base.SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return false;
                db.SaveChanges();


                return true;
            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + ex);
                return false;
            }

        }
        #endregion
        #region xác nhận làm ngoài giờ
        public dynamic GetNextMaQuyTrinhXacNhanNgoaiGio(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyTrinhXacNhanNgoaiGio.Select(x => x.SoQuyTrinh), DefineData.MaQuyTrinhXacNhanNgoaiGio));
        public dynamic GetListQuyTrinhXacNhanNgoaiGio(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_QuyTrinhXacNhanNgoaiGio> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_QuyTrinhXacNhanNgoaiGio
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhXacNhanNgoaiGio
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy != null && TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);

                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> CongTys = _CallWebService.GetDanhSachCongTy(db);
                List<string> listId = itemList.Select(a => a.IdGuid).Distinct().ToList();

                foreach (HRM_QuyTrinhXacNhanNgoaiGio item in itemList)
                {

                    BoPhan duAn = CongTys.Find(x => x.Id == item.IdCongTy);
                    if (duAn != null)
                        item.TenCongTy = duAn.TenBoPhan;


                }

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
        public dynamic SetQuyTrinhXacNhanNgoaiGio(HRM_QuyTrinhXacNhanNgoaiGio model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    // DI: use inherited _CallWebService
                    var QuyTrinhdb = db.HRM_QuyTrinhXacNhanNgoaiGio.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncQuyTrinhXacNhanNgoaiGio(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, model, err))
                    {
                        ts.Rollback();
                        return err;
                    }


                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);
                }
                catch (Exception exx)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhXacNhanNgoaiGio(HRM_QuyTrinhXacNhanNgoaiGio QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_QuyTrinhXacNhanNgoaiGio pcuDatabase = db.HRM_QuyTrinhXacNhanNgoaiGio.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    bool Error = FuncQuyTrinhXacNhanNgoaiGio(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!base.ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    if (QuyTrinh.isKetThuc == true && QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                    {

                        List<HRM_TongHopXacNhanNgoaiGio> listAdd = new List<HRM_TongHopXacNhanNgoaiGio>();
                        foreach (var item in QuyTrinh.listNhanVien)
                        {
                            if (item.listCaLamViec != null && item.listCaLamViec.Count > 0)
                                foreach (var Ca in item.listCaLamViec)
                                {
                                    if (Ca.SoGio > 0)
                                    {
                                        var itemAdd = new HRM_TongHopXacNhanNgoaiGio();
                                        itemAdd.IdGuid = Guid.NewGuid().ToString();
                                        itemAdd.IdNhanVien = item.IdNhanVien;
                                        itemAdd.TuNgay = QuyTrinh.TuNgay;
                                        itemAdd.DenNgay = QuyTrinh.DenNgay;
                                        itemAdd.SoGio = Ca.SoGio;
                                        itemAdd.HeSo = Ca.HeSo;
                                        itemAdd.GioTieuChuan = Ca.GioTieuChuan;
                                        itemAdd.CongTieuChuan = Ca.CongTieuChuan;

                                        listAdd.Add(itemAdd);

                                    }
                                }
                        }
                        if (listAdd != null && listAdd.Count > 0)
                            db.HRM_TongHopXacNhanNgoaiGio.AddRange(listAdd);
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");

                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhXacNhanNgoaiGio(HRM_QuyTrinhXacNhanNgoaiGio QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhXacNhanNgoaiGio pcuDatabase = db.HRM_QuyTrinhXacNhanNgoaiGio.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = FuncQuyTrinhXacNhanNgoaiGio(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!base.KhongDuyetSauHamUpdate(db, QuyTrinh))
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
        public dynamic GetByIdQuyTrinhXacNhanNgoaiGio(string Id, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                MessageReturn err1 = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_QuyTrinhXacNhanNgoaiGio model = db.HRM_QuyTrinhXacNhanNgoaiGio.Find(Id);
                if (!base.GetQuyTrinh(db, _CallWebService, model, err1, true))
                    return err1;
                model.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(model.TuNgay);
                model.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(model.DenNgay);
                model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_QuyTrinhXacNhanNgoaiGio.ToString(), Id);
                var listCaLamViec = db.HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                if (listCaLamViec != null && listCaLamViec.Count > 0)
                {
                    Parallel.ForEach(listCaLamViec, item =>
                     {
                         item.TendmCaLamViec = item.GioTieuChuan + "h - " + item.HeSo + " %";
                     });
                }

                if (listCaLamViec != null && listCaLamViec.Count > 0)
                {
                    List<string> listIdNhanVien = listCaLamViec.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.isApDung == true && listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var listGiayto = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.isApDung == true && listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listNgay = GetDanhSachGio(db, model.IdCongTy, dungchung.DateTimeToUnixTimeStamp(model.TuNgay), dungchung.DateTimeToUnixTimeStamp(model.DenNgay), listIdNhanVien);
                    model.listNhanVien = new List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVien>();
                    listNhanVien.ForEach(nhanVien =>
                    {
                        var item = new HRM_QuyTrinhXacNhanNgoaiGio_NhanVien();

                        item.IdNhanVien = nhanVien.IdGuid;
                        item.MaNhanVien = nhanVien.MaNhanVien;
                        item.TenNhanVien = nhanVien.TenDayDu;
                        item.DiaChi = nhanVien.ChoOHienNayTruDiaChi;
                        item.SoDienThoai = listSDT.Where(a => a.IdNhanVien == item.IdNhanVien).Select(a => a.SoDienThoai).FirstOrDefault();
                        item.CCCD = listGiayto.Where(a => a.IdNhanVien == item.IdNhanVien).Select(a => a.So).FirstOrDefault();
                        var boPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                        if (boPhan != null)
                            item.TenBoPhan = boPhan.TenBoPhan;

                        var ChucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                        if (ChucVu != null)
                            item.TenChucVu = ChucVu.TenChucVu;
                        item.listCaLamViec = listCaLamViec.Where(r => !string.IsNullOrEmpty(r.IdNhanVien) && r.IdNhanVien == item.IdNhanVien).OrderBy(a => a.SapXep).ToList();
                        item.listNgay = listNgay.Where(a => a.IdNhanVien == nhanVien.IdGuid).OrderBy(a => a.Ngay).ToList();
                        model.listNhanVien.Add(item);
                    });
                }
                err.Data = model;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }
        public List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio> GetDanhSachGio(ApplicationDbContext db, long IdCongTy, double TuNgay, double DenNgay, List<string> listIdNV)
        {
            List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio> listResult = new List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio>();
            {
                var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongTy, TuNgay, DenNgay, db);
                List<string> listIdUser = new List<string>();
                if (listUserDuAn != null && listUserDuAn.Count > 0)
                    listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList();
                DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgay);
                DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgay);

                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => (!string.IsNullOrEmpty(x.IdUser) && listIdUser.Contains(x.IdUser))).ToList();
                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut
                                                       where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.I.ToString()
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                       group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                       select g.OrderBy(a => a.Gio).FirstOrDefault()).ToList();
                listCheckInOut.AddRange((from x in db.HRM_CheckInOut
                                         where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                                                       && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                         group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                         select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());

                List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                               && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
                List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                                                                                      && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                {
                    foreach (var item in listXinNghiPhep)
                    {
                        var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                        if (hinhThucNghi != null)
                            item.HinhThucNghi = hinhThucNghi.Ma;
                    }
                }
                var listCaLamViecs = db.HRM_dmLichLamViec.ToList();

                double TongT7 = Enumerable.Range(0, (dDenNgay - dTuNgay).Days + 1).Select(offset => dTuNgay.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Saturday);
                double TongCN = Enumerable.Range(0, (dDenNgay - dTuNgay).Days + 1).Select(offset => dTuNgay.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Sunday);
                int TongSoNgay = (dDenNgay - dTuNgay).Days + 1;
                double TongSoCongKhac = (((int)(TongSoNgay / 6)) + (int)((TongSoNgay % 6) / 4)) * 4 + (TongSoNgay % 6) % 4;
                foreach (var item in listCaLamViecs)
                {
                    if (item.LoaiNgayNghi == 1)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongT7 - TongCN);
                    if (item.LoaiNgayNghi == 2)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongCN);
                    if (item.LoaiNgayNghi == 1)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongSoCongKhac);
                }
                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                List<HRM_BangChamCongNhanVien_TangCa> listDataTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                List<HRM_BangChamCongNhanVien_SoGioLamViec> listDataTieuChuan = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
                var funcKeHoach = this;
                List<double> listHeSo = listLichLamViec.Where(a => a.Loai == 1).Select(a => a.HeSoTangCa).Distinct().ToList();
                List<double> listCaTieuChuan = listLichLamViec.Select(a => a.GioTieuChuan).Distinct().ToList();
                var DuAn = _CallWebService.GetQuyTrinhDuAn(IdCongTy, db);
                if (DuAn == null)
                    DuAn = new BoPhan();
                string VeSom = HrmOptions.SoPhutSom.ToString();
                double SoPhutDuocVeSom = double.Parse(VeSom);
                string DiMuon = HrmOptions.SoPhutMuon.ToString();
                double SoPhutDuocDiMuon = double.Parse(DiMuon);
                if (DuAn.MaBoPhan == "VPCT")
                {
                    SoPhutDuocDiMuon = 0;
                    SoPhutDuocVeSom = 0;
                }
                DateTime dateNow = DateTime.Now;
                dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                foreach (var item in listNhanVien)
                {
                    var NhanSuDA = listUserDuAn.FirstOrDefault(a => a.IdUser == item.IdUser);
                    DateTime dt = dTuNgay;
                    if (listHeSo != null && listHeSo.Count > 0)
                    {
                        foreach (var heso in listHeSo)
                        {
                            HRM_BangChamCongNhanVien_TangCa hesoAdd = new HRM_BangChamCongNhanVien_TangCa();
                            hesoAdd.HeSo = heso;
                            hesoAdd.IdNhanVien = item.IdGuid;
                            var tangca = listLichLamViec.Where(a => a.IdNhanVien == item.IdGuid && a.HeSoTangCa == heso && a.Loai == 1).ToList();
                            if (tangca != null && tangca.Count > 0)
                            {
                                hesoAdd.TongSoGio = tangca.Sum(a => (a.GioRa - a.GioVao).TotalHours);
                                hesoAdd.Cong = tangca.Sum(a => ((a.GioRa - a.GioVao).TotalHours) / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                                hesoAdd.IddmHeSoTangCa = tangca.FirstOrDefault().IddmHeSoTangCa;
                            }
                            listDataTangCa.Add(hesoAdd);
                        }
                    }
                    if (listCaTieuChuan != null && listCaTieuChuan.Count > 0)
                    {
                        foreach (var Id in listCaTieuChuan)
                        {
                            HRM_BangChamCongNhanVien_SoGioLamViec hesoAdd = new HRM_BangChamCongNhanVien_SoGioLamViec();
                            var caLamViec = listCaLamViecs.FirstOrDefault(a => a.SoGioTrongNgay == Id);
                            if (caLamViec != null)
                            {
                                hesoAdd.IddmCaLamViec = caLamViec.IdGuid;
                                hesoAdd.IdNhanVien = item.IdGuid;
                                hesoAdd.GioTieuChuan = caLamViec.SoGioTrongNgay ?? 8;
                                hesoAdd.CongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 26;
                                hesoAdd.TongSoGio = 0;
                                listDataTieuChuan.Add(hesoAdd);
                            }
                        }
                    }

                    while (dt <= dDenNgay)
                    {
                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                        List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt);
                        List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt).OrderBy(a => a.Ngay).ToList();
                        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);
                        if (dt >= dateNow)
                        {
                            dt = dt.AddDays(1);
                            continue;
                        }
                        if (listCheckInOutFind != null && listCheckInOutFind.Count > 0)
                        {
                            //if (listCheckInOutFind.LastOrDefault().Loai == "I")
                            //{
                            //    DateTime dta = dt.AddDays(1);
                            //    listCheckInOutFind.Add(listCheckInOut.FindAll(x => x.Loai == "O" && x.IdNhanVien == item.IdGuid && x.Ngay == dta).OrderBy(a => a.Ngay).FirstOrDefault());
                            //}

                            if (listLichLamViecFind == null)
                                listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();
                            string IdCaLamViec = listLichLamViecFind.Where(a => a.IdCongTy == IdCongTy).Select(a => a.IddmCaLamViec).FirstOrDefault();
                            var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);

                            List<clsDiMuonVeSom> listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, DuAn.MaBoPhan, NghiPheps, false, IdCongTy);
                            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecFindNV = listLichLamViecNV.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                            if (listLichLamViecFind.Count >= 1)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                bool TangCaDem = false;
                                foreach (var KeHoach in listLichLamViecFind)
                                {

                                    if (KeHoach.IdCongTy != IdCongTy)
                                        continue;
                                    double SoGioLamViec = KeHoach.TongSoGioGiao;
                                    if (KeHoach.Loai == 0)
                                    {
                                        itemAdd.SoGioLamViec += SoGioLamViec;
                                        if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                        {
                                            var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == KeHoach.GioTieuChuan);
                                            if (tangCa != null)
                                                tangCa.TongSoGio += SoGioLamViec;

                                        }
                                    }
                                    else
                                    {
                                        itemAdd.SoGioLamViecTangCa += SoGioLamViec;
                                        if (KeHoach.GioVao.Hour >= 21)
                                        {
                                            TangCaDem = true;

                                        }
                                        if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                        {
                                            var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == KeHoach.IddmHeSoTangCa);
                                            if (tangCa != null)
                                                tangCa.TongSoGio += SoGioLamViec;
                                        }
                                    }

                                }
                                List<string> listKyHieu = new List<string>();
                                if (listSomMuon != null && listSomMuon.Count > 0)
                                    foreach (var SomMuon in listSomMuon)
                                    {
                                        //itemAdd.GioLamViec += "; ";
                                        double Gio = SomMuon.SoPhut / 60.0;
                                        if (SomMuon.Loai == 0)
                                        {
                                            //if (SomMuon.isCaGay == true)
                                            //    itemAdd.SoGioLamViecCaGay -= Gio;
                                            //else
                                            itemAdd.SoGioLamViec -= Gio;
                                            if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == SomMuon.GioTieuChuan);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio -= Gio;
                                                if (tangCa.TongSoGio < 0)
                                                    tangCa.TongSoGio = 0;
                                            }
                                        }
                                        else
                                        {
                                            itemAdd.SoGioLamViecTangCa -= Gio;
                                            if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == SomMuon.IddmHeSoTangCa);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio -= Gio;
                                            }
                                        }
                                        listKyHieu.Add(SomMuon.KyHieuChamCong);
                                    }
                                double GioLamViec = (double)(itemAdd.SoGioLamViecCaGay + itemAdd.SoGioLamViec);
                                string KyHieu = "";
                                if (listKyHieu != null && listKyHieu.Count > 0)
                                    KyHieu += string.Join("", listKyHieu);
                                if (GioLamViec > 0)
                                    KyHieu += Math.Round(GioLamViec, 2);
                                if (CaLamViec != null && CaLamViec.TuGio.Hour >= 21)
                                {
                                    itemAdd.isCaDem = true;
                                }
                                if (itemAdd.SoGioLamViecTangCa > 0)
                                {
                                    if (TangCaDem == false)
                                    {
                                        KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "";
                                    }
                                    else
                                    {
                                        if (listLichLamViecFind.Count > 1)
                                            KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                        else if (listLichLamViecFind.Count == 1)
                                        {
                                            if (GioLamViec > 0)
                                                KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                            else
                                                KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Đ";
                                        }
                                    }
                                }
                                itemAdd.GioLamViec = KyHieu;
                                if (NhanSuDA.isDieuDong == false)
                                {
                                    var checkIn = listCheckInOutFind.FirstOrDefault(a => a.IdCongTy != IdCongTy);
                                    if (checkIn != null)
                                        itemAdd.GioLamViec = DefineData.eKyHieuChamCong.XTC.ToString();
                                }
                                listData.Add(itemAdd);
                            }
                        }
                        else if (NghiPheps.Count > 0)
                        {
                            if (listLichLamViecFind == null)
                                listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();


                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == NghiPheps.FirstOrDefault().HinhThucNghi);
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                double SoGioLamViec = 0;
                                foreach (var KeHoach in listLichLamViecFind)
                                {
                                    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;

                                }
                                itemAdd.GioLamViec = NghiPhep.Ma /*+ SoGioLamViec*/;
                                listData.Add(itemAdd);
                            }
                        }
                        else if ((int)dt.DayOfWeek != 0 && listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                        {
                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                double SoGioLamViec = 0;
                                foreach (var KeHoach in listLichLamViecFind)
                                {
                                    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;
                                }
                                itemAdd.GioLamViec = NghiPhep.Ma;
                                listData.Add(itemAdd);
                            }
                        }

                        dt = dt.AddDays(1);
                    }

                }


                if (listData != null && listData.Count > 0)
                {
                    foreach (var item in listNhanVien)
                    {
                        {
                            var check = listData.Where(a => a.IdNhanVien == item.IdGuid).FirstOrDefault();
                            if (check == null)
                                continue;
                        }
                        HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio itemAdd = new HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio();

                        DateTime time = dTuNgay;
                        var listNgay = listData.Where(a => a.IdNhanVien == item.IdGuid /*&& a.IdCongTyEOS == IdCongTy*/).OrderBy(a => a.NgayChamCong).ToList();
                        listResult.AddRange(listNgay.Select(a => new HRM_QuyTrinhXacNhanNgoaiGio_NhanVienNgoaiGio
                        {
                            IdNhanVien = a.IdNhanVien,
                            Ngay = a.NgayChamCong,
                            NgayUnix = dungchung.DateTimeToUnixTimeStamp(a.NgayChamCong),
                            Gio = a.SoGioLamViec ?? 0

                        }));



                    }
                }
            }
            return listResult;
        }
        public dynamic KhoiTaoQuyTrinhXachNhanNgoaiGio(ApplicationDbContext db, long IdCongTy, double TuNgay, double DenNgay, List<string> listIdNV)
        {
            List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVien> listResult = new List<HRM_QuyTrinhXacNhanNgoaiGio_NhanVien>();
            var listNhanVien = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).ToList();
            var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.isApDung == true && listIdNV.Contains(a.IdGuid)).ToList();
            var listGiayto = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.isApDung == true && listIdNV.Contains(a.IdGuid)).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var ThongTinDuAn = _CallWebService.GetThongTinCaLamViecDuAn(IdCongTy, db);
            var listdmCa = db.HRM_dmLichLamViec.OrderBy(a => a.SapXep).ToList();
            if (ThongTinDuAn.itemThongTinDuAn.listCaLamViec != null && ThongTinDuAn.itemThongTinDuAn.listCaLamViec.Count > 0)
            {
                var listIdCa = ThongTinDuAn.itemThongTinDuAn.listCaLamViec.Select(a => a.IddmCaLamViec).Distinct().ToList();
                listdmCa = listdmCa.Where(a => listIdCa.Contains(a.IdGuid)).OrderBy(a => a.SapXep).ToList();
            }
            var listNgay = GetDanhSachGio(db, IdCongTy, TuNgay, DenNgay, listIdNV);
            var listHeSo = db.HRM_dmHeSoLamThemGio.OrderBy(a => a.SapXep).ToList();
            List<double> listGioTieuChuan = listdmCa.Select(a => a.SoGioTrongNgay ?? 0).Distinct().ToList();
            listNhanVien.ForEach(nhanVien =>
              {
                  var item = new HRM_QuyTrinhXacNhanNgoaiGio_NhanVien();
                  item.IdNhanVien = nhanVien.IdGuid;
                  item.MaNhanVien = nhanVien.MaNhanVien;
                  item.TenNhanVien = nhanVien.TenDayDu;
                  item.DiaChi = nhanVien.ChoOHienNayTruDiaChi;
                  item.SoDienThoai = listSDT.Where(a => a.IdNhanVien == item.IdNhanVien).Select(a => a.SoDienThoai).FirstOrDefault();
                  item.CCCD = listGiayto.Where(a => a.IdNhanVien == item.IdNhanVien).Select(a => a.So).FirstOrDefault();
                  var boPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                  if (boPhan != null)
                      item.TenBoPhan = boPhan.TenBoPhan;

                  var ChucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                  if (ChucVu != null)
                      item.TenChucVu = ChucVu.TenChucVu;

                  item.listCaLamViec = new List<HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec>();
                  foreach (var Gio in listGioTieuChuan)
                  {
                      var caLV = listdmCa.FirstOrDefault(a => a.SoGioTrongNgay == Gio);
                      foreach (var heso in listHeSo)
                      {
                          item.listCaLamViec.Add(new HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec
                          {
                              TendmCaLamViec = caLV.SoGioTrongNgay + "h - " + heso.HeSo + " %",
                              GioTieuChuan = caLV.SoGioTrongNgay ?? 8,
                              CongTieuChuan = caLV.SoNgayCongTieuChuan ?? 26,
                              HeSo = heso.HeSo,
                          });
                      }
                  }
                  listdmCa.Select(a => new HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec
                  {
                      TendmCaLamViec = a.Ten,
                      GioTieuChuan = a.SoGioTrongNgay ?? 8,
                      CongTieuChuan = a.SoNgayCongTieuChuan ?? 26,
                  }).ToList();
                  item.listNgay = listNgay.Where(a => a.IdNhanVien == nhanVien.IdGuid).ToList();
                  listResult.Add(item);
              });
            return listResult;
        }
        public dynamic DeleteQuyTrinhXacNhanNgoaiGio(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_QuyTrinhXacNhanNgoaiGio model = db.HRM_QuyTrinhXacNhanNgoaiGio.Find(Id);
                var listCalamViec = db.HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                db.HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec.RemoveRange(listCalamViec);
                db.HRM_QuyTrinhXacNhanNgoaiGio.Remove(model);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public bool FuncQuyTrinhXacNhanNgoaiGio(HRM_QuyTrinhXacNhanNgoaiGio model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.TuNgay = dungchung.UnixTimeStampToDateTime(model.TuNgayUnix);
                model.DenNgay = dungchung.UnixTimeStampToDateTime(model.DenNgayUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    //model.IdGuid = Guid.NewGuid().ToString();
                    model.eAction = DefineData.eAction.XACNHANNGOAIGIO.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_QuyTrinhXacNhanNgoaiGio.Add(model);


                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var listCalamViec = db.HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
                db.HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec.RemoveRange(listCalamViec);
                if (model.listNhanVien != null && model.listNhanVien.Count > 0)
                {
                    int stt = 1;
                    foreach (var item in model.listNhanVien)
                    {

                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        stt++;
                        if (item.listCaLamViec != null && item.listCaLamViec.Count > 0)
                        {
                            foreach (var itemCP in item.listCaLamViec)
                            {
                                itemCP.IdGuid = Guid.NewGuid().ToString();
                                itemCP.IdQuyTrinh = model.IdGuid;
                                itemCP.IdNhanVien = item.IdNhanVien;
                                itemCP.SapXep = stt;
                                stt++;
                            }
                            db.HRM_QuyTrinhXacNhanNgoaiGio_CaLamViec.AddRange(item.listCaLamViec);

                        }
                    }
                }

                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_QuyTrinhXacNhanNgoaiGio, model.IdGuid);
                    }
                }
                if (!base.SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return false;
                db.SaveChanges();


                return true;
            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + ex);
                return false;
            }

        }
        #endregion
        public void BaoCaoCongViec_ItemToTree(List<HRM_BaoCaoCongViec_ItemView> ListAll, ReturnData<HRM_BaoCaoCongViec_ItemView> parent)
        {
            parent.children = ListAll.Where(x => x.STT_Cha == parent.data.STT && x.STT_Root == parent.data.STT_Root).Select(x => new ReturnData<HRM_BaoCaoCongViec_ItemView>
            {
                data = x
            }).ToList();

            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {

                    BaoCaoCongViec_ItemToTree(ListAll, item);
                }

            }
        }
        public void BaoCaoCongViec_ItemToList(List<ReturnData<HRM_BaoCaoCongViec_ItemView>> DanhGias, List<HRM_BaoCaoCongViec_ItemView> result, string IdQuyTrinh)
        {
            foreach (var item in DanhGias)
            {
                if (string.IsNullOrEmpty(item.data.STT_Cha))
                    item.data.STT_Root = item.data.STT;
                //item.data.IdGuid = Guid.NewGuid().ToString();
                result.Add(item.data);
                if (item.children != null)
                {
                    foreach (var child in item.children)
                    {
                        child.data.STT_Cha = item.data.STT;
                        child.data.STT_Root = item.data.STT_Root;
                    }
                    BaoCaoCongViec_ItemToList(item.children, result, IdQuyTrinh);
                }
            }
        }
        public dynamic GetBaoCaoCongViec(string Id, ApplicationDbContext db)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                var QuyTrinhdb = db.HRM_BaoCaoCongViec_NhanVien.AsNoTracking().FirstOrDefault(a => a.IdGuid == Id);

                var listData = db.HRM_BaoCaoCongViec_Item.Where(a => a.IdBaoCao == Id).OrderBy(a => a.SapXep).ToList();
                var listNoiDung = listData.Where(a => a.isHeader == false && a.IdBaoCao == Id).OrderBy(a => a.SapXep).ToList();
                var listheader = listData.Where(a => a.isHeader == true && a.IdBaoCao == Id).OrderBy(a => a.SapXep).ToList();
                QuyTrinhdb = GetListHeaderData(listheader, QuyTrinhdb);
                QuyTrinhdb.listNoiDung = new List<ReturnData<HRM_BaoCaoCongViec_ItemView>>();
                var HRM_BaoCaoCongViec_ItemView = new List<HRM_BaoCaoCongViec_ItemView>();
                var listSTT = listNoiDung.Select(a => a.STT).Distinct().ToList();
                foreach (var STT in listSTT)
                {

                    var listGiaTri = listNoiDung.Where(a => a.STT == STT).OrderBy(a => a.SapXep).ToList();
                    var listSTT_Root = listGiaTri.Where(a => !string.IsNullOrEmpty(a.STT_Root)).Select(a => a.STT_Root).Distinct().ToList();
                    if (listSTT_Root != null && listSTT_Root.Count > 0)
                    {
                        foreach (var Stt_Root in listSTT_Root)
                        {
                            var listSTT_Cha = listGiaTri.Where(a => !string.IsNullOrEmpty(a.STT_Cha) && a.STT_Root == Stt_Root).Select(a => a.STT_Cha).Distinct().ToList();
                            if (listSTT_Cha != null && listSTT_Cha.Count > 0)
                            {
                                foreach (var STT_Cha in listSTT_Cha)
                                {
                                    var item = new HRM_BaoCaoCongViec_ItemView();

                                    item.STT = STT;
                                    item.STT_Cha = STT_Cha;
                                    item.STT_Root = Stt_Root;
                                    item.listGiaTri = listNoiDung.Where(a => a.STT == item.STT && a.STT_Root == Stt_Root).OrderBy(a => a.SapXep).ToList();
                                    item.STT_Cha = item.listGiaTri.FirstOrDefault().STT_Cha;
                                    HRM_BaoCaoCongViec_ItemView.Add(item);
                                }
                            }
                            else
                            {
                                var item = new HRM_BaoCaoCongViec_ItemView();
                                item.STT = STT;
                                item.STT_Root = Stt_Root;
                                item.listGiaTri = listNoiDung.Where(a => a.STT == item.STT).OrderBy(a => a.SapXep).ToList();
                                item.STT_Cha = item.listGiaTri.FirstOrDefault().STT_Cha;
                                HRM_BaoCaoCongViec_ItemView.Add(item);
                            }
                        }
                    }
                }
                QuyTrinhdb.listNoiDung = HRM_BaoCaoCongViec_ItemView.Where(a => string.IsNullOrEmpty(a.STT_Cha)).Select(a => new ReturnData<HRM_BaoCaoCongViec_ItemView>
                {
                    data = a,
                    children = new List<ReturnData<HRM_BaoCaoCongViec_ItemView>>()
                }).ToList();
                if (QuyTrinhdb.listNoiDung != null && QuyTrinhdb.listNoiDung.Count > 0)
                {
                    foreach (var item in QuyTrinhdb.listNoiDung)
                        BaoCaoCongViec_ItemToTree(HRM_BaoCaoCongViec_ItemView, item);
                }

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu thành công", QuyTrinhdb);
            }
            catch (Exception exx)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
            }

        }
        public static void ResetIdCongViec(List<HRM_BaoCaoCongViec_Item> listDanhGia)
        {
            foreach (var item in listDanhGia)
            {
                string Id = item.IdGuid;
                item.IdGuid = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(item.IdParent))
                    item.IdKey = item.IdGuid;
                else
                    item.IdKey = item.IdKey + "," + item.IdGuid;
                foreach (var child in listDanhGia)
                {
                    if (child.IdParent != Id)
                        continue;
                    child.IdParent = item.IdGuid;
                    child.IdKey = item.IdKey;
                }
            }
        }

        public dynamic SaoChepBaoCaoCongViec(string Id, ApplicationDbContext db)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                var QuyTrinhdb = db.HRM_BaoCaoCongViec_NhanVien.AsNoTracking().FirstOrDefault(a => a.IdGuid == Id);
                DateTime time = new DateTime(QuyTrinhdb.Nam, QuyTrinhdb.Thang, 1);
                time = time.AddMonths(1);

                var listData = db.HRM_BaoCaoCongViec_Item.AsNoTracking().Where(a => a.IdBaoCao == Id).OrderBy(a => a.SapXep).ToList();
                var listNoiDung = listData.Where(a => a.isHeader == false).OrderBy(a => a.SapXep).ToList();
                var listheader = listData.Where(a => a.isHeader == true).OrderBy(a => a.Header).ThenBy(a => a.SapXep).ToList();
                var QuyTrinh = QuyTrinhdb.Clone();
                QuyTrinh.Nam = time.Year;
                QuyTrinh.Thang = time.Month;
                QuyTrinh.IdGuid = Guid.NewGuid().ToString();
                List<HRM_BaoCaoCongViec_Item> listAdd = new List<HRM_BaoCaoCongViec_Item>();
                foreach (var item in listNoiDung)
                {
                    var itemAdd = item.Clone();
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.IdBaoCao = QuyTrinh.IdGuid;
                    listAdd.Add(itemAdd);
                }
                ResetIdCongViec(listheader);
                foreach (var item in listheader)
                {
                    item.IdBaoCao = QuyTrinh.IdGuid;
                    listAdd.Add(item);
                }
                //db.HRM_BaoCaoCongViec_Item.AddRange(listheader);
                db.HRM_BaoCaoCongViec_Item.AddRange(listAdd);
                db.HRM_BaoCaoCongViec_NhanVien.Add(QuyTrinh);
                db.SaveChanges();
                QuyTrinh = GetListHeaderData(listheader, QuyTrinh);
                QuyTrinh.listNoiDung = new List<ReturnData<HRM_BaoCaoCongViec_ItemView>>();
                var HRM_BaoCaoCongViec_ItemView = new List<HRM_BaoCaoCongViec_ItemView>();
                var listSTT = listNoiDung.Select(a => a.STT).Distinct().ToList();
                foreach (var STT in listSTT)
                {

                    var listGiaTri = listNoiDung.Where(a => a.STT == STT).OrderBy(a => a.SapXep).ToList();
                    var listSTT_Root = listGiaTri.Where(a => !string.IsNullOrEmpty(a.STT_Root)).Select(a => a.STT_Root).Distinct().ToList();
                    if (listSTT_Root != null && listSTT_Root.Count > 0)
                    {
                        foreach (var Stt_Root in listSTT_Root)
                        {
                            var listSTT_Cha = listGiaTri.Where(a => !string.IsNullOrEmpty(a.STT_Cha) && a.STT_Root == Stt_Root).Select(a => a.STT_Cha).Distinct().ToList();
                            if (listSTT_Cha != null && listSTT_Cha.Count > 0)
                            {
                                foreach (var STT_Cha in listSTT_Cha)
                                {
                                    var item = new HRM_BaoCaoCongViec_ItemView();

                                    item.STT = STT;
                                    item.STT_Cha = STT_Cha;
                                    item.STT_Root = Stt_Root;
                                    item.listGiaTri = listNoiDung.Where(a => a.STT == item.STT && a.STT_Root == Stt_Root).OrderBy(a => a.SapXep).ToList();
                                    item.STT_Cha = item.listGiaTri.FirstOrDefault().STT_Cha;
                                    HRM_BaoCaoCongViec_ItemView.Add(item);
                                }
                            }
                            else
                            {
                                var item = new HRM_BaoCaoCongViec_ItemView();
                                item.STT = STT;
                                item.STT_Root = Stt_Root;
                                item.listGiaTri = listNoiDung.Where(a => a.STT == item.STT).OrderBy(a => a.SapXep).ToList();
                                item.STT_Cha = item.listGiaTri.FirstOrDefault().STT_Cha;
                                HRM_BaoCaoCongViec_ItemView.Add(item);
                            }
                        }
                    }
                }
                QuyTrinh.listNoiDung = HRM_BaoCaoCongViec_ItemView.Where(a => string.IsNullOrEmpty(a.STT_Cha)).Select(a => new ReturnData<HRM_BaoCaoCongViec_ItemView>
                {
                    data = a,
                    children = new List<ReturnData<HRM_BaoCaoCongViec_ItemView>>()
                }).ToList();
                if (QuyTrinh.listNoiDung != null && QuyTrinh.listNoiDung.Count > 0)
                {
                    foreach (var item in QuyTrinh.listNoiDung)
                        BaoCaoCongViec_ItemToTree(HRM_BaoCaoCongViec_ItemView, item);
                }


                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Sao chép dữ liệu thành công", QuyTrinh);
            }
            catch (Exception exx)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
            }

        }

        public dynamic DeleteBaoCaoCongViec(string Id, ApplicationDbContext db)
        {
            try
            {
                var QuyTrinhdb = db.HRM_BaoCaoCongViec_NhanVien.FirstOrDefault(a => a.IdGuid == Id);
                var listNoiDung = db.HRM_BaoCaoCongViec_Item.Where(a => a.IdBaoCao == Id).OrderBy(a => a.Created).ToList();
                db.HRM_BaoCaoCongViec_NhanVien.Remove(QuyTrinhdb);
                db.HRM_BaoCaoCongViec_Item.RemoveRange(listNoiDung);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công");
            }
            catch (Exception exx)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
            }

        }
        public dynamic SetBaoCaoCongViec(HRM_BaoCaoCongViec_NhanVien model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var QuyTrinhdb = db.HRM_BaoCaoCongViec_NhanVien.AsNoTracking().FirstOrDefault(a => a.Ma == model.Ma && a.Nam == model.Nam && a.Thang == model.Thang && a.IdBoPhan == model.IdBoPhan && a.IdGuid != model.IdGuid);
                    if (QuyTrinhdb != null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên đã thiết lập báo cáo Tháng " + model.Thang + "/" + model.Nam);
                    }
                    if (string.IsNullOrEmpty(model.IdGuid))
                    {
                        model.IdGuid = Guid.NewGuid().ToString();
                        db.HRM_BaoCaoCongViec_NhanVien.Add(model);
                    }
                    else
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                    var noiDungold = db.HRM_BaoCaoCongViec_Item.Where(a => a.IdBaoCao == model.IdGuid).ToList();
                    db.HRM_BaoCaoCongViec_Item.RemoveRange(noiDungold);
                    db.SaveChanges();
                    if (model.listNoiDung != null && model.listNoiDung.Count > 0)
                    {
                        var listGiaTri = new List<HRM_BaoCaoCongViec_Item>();
                        var listNoiDung = new List<HRM_BaoCaoCongViec_ItemView>();
                        BaoCaoCongViec_ItemToList(model.listNoiDung, listNoiDung, model.IdGuid);
                        int SapXep = 1;
                        foreach (var item in listNoiDung)
                        {
                            foreach (var itemChild in item.listGiaTri)
                            {
                                itemChild.IdGuid = Guid.NewGuid().ToString();
                                itemChild.IdBaoCao = model.IdGuid;
                                itemChild.Thang = model.Thang;
                                itemChild.Nam = model.Nam;
                                itemChild.STT = item.STT;
                                itemChild.STT_Cha = item.STT_Cha;
                                itemChild.STT_Root = item.STT_Root;
                                itemChild.SapXep = SapXep;
                                SapXep++;

                            }
                            listGiaTri.AddRange(item.listGiaTri);
                        }
                        if (listGiaTri.Count > 0)
                            db.HRM_BaoCaoCongViec_Item.AddRange(listGiaTri);
                        db.SaveChanges();
                    }
                    if (model.listHeader != null && model.listHeader.Count > 0)
                    {
                        var listGiaTri = new List<HRM_BaoCaoCongViec_Item>();
                        var listGiaTriAdd = new List<HRM_BaoCaoCongViec_Item>();
                        //var listNoiDung = new List<HRM_BaoCaoCongViec_ItemView>();
                        //BaoCaoCongViec_ItemToList(model.listHeader, listNoiDung, model.IdGuid);
                        int SapXep = 1;
                        foreach (var item in model.listHeader)
                        {
                            foreach (var itemChild in item.listHeader)
                            {
                                itemChild.IdBaoCao = model.IdGuid;
                                itemChild.Thang = model.Thang;
                                itemChild.Nam = model.Nam;
                                itemChild.isHeader = true;
                                itemChild.Header = item.Header;
                                itemChild.SapXep = SapXep;
                                SapXep++;

                            }
                            listGiaTri.AddRange(item.listHeader);
                        }
                        if (listGiaTri != null && listGiaTri.Count > 0)
                        {
                            foreach (var itemGT in listGiaTri)
                            {
                                var listChild = listGiaTri.Where(a => a.IdParent == itemGT.IdGuid).ToList();
                                //item = item.Clone();
                                var item = itemGT.Clone();
                                item.IdGuid = Guid.NewGuid().ToString();
                                string IdKey = "";
                                if (!string.IsNullOrEmpty(item.IdKey))
                                    IdKey = item.IdKey + "-" + item.IdGuid;
                                else
                                    IdKey = item.IdGuid;

                                if (listChild != null && listChild.Count > 0)
                                {
                                    Parallel.ForEach(listChild, child =>
                                    {
                                        child.IdParent = item.IdGuid;
                                        child.IdKey = IdKey;
                                    });
                                }
                                listGiaTriAdd.Add(item);
                            }
                        }
                        if (listGiaTri.Count > 0)
                            db.HRM_BaoCaoCongViec_Item.AddRange(listGiaTriAdd);
                        db.SaveChanges();
                    }


                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);
                }
                catch (Exception exx)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xử lý " + exx);
                }
            }
        }
        public dynamic GetListBaoCaoCongViec(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 1;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_BaoCaoCongViec_NhanVien> result = from a in db.HRM_BaoCaoCongViec_NhanVien
                                                                  orderby a.Nam descending, a.Thang descending
                                                                  select a
                                                                  ;

                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.IdBoPhan > 0)
                    result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);
                if (TimKiem.Nam > 0)
                    result = result.Where(a => a.Nam == TimKiem.Nam);

                if (TimKiem.Thang > 0)
                    result = result.Where(a => a.Thang == TimKiem.Thang);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.HoTen.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                bool ischeck = _CallWebService.KiemTraTab(db, "P_BaoCaoCongViec");
                if (ischeck == false)
                    result = result.Where(a => a.CreatedBy == db.currentUser.Id);
                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                var listboPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in itemList)
                    item.TenBoPhan = listboPhan.Where(a => a.Id == item.IdBoPhan).Select(a => a.TenBoPhan).FirstOrDefault();
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
        public int ExportHeader_ChiTietHeader(ExcelWorksheet Sheet1, int nRow, int nCol, int Rowspan, int colspan, string Name, double DoRong)
        {
            //try
            //{
            if (colspan > 1 || Rowspan > 1)
            {
                bool merge = Sheet1.Cells[nRow, nCol].Merge;
                if (merge == true)
                {
                    nCol++;
                    nCol = ExportHeader_ChiTietHeader(Sheet1, nRow, nCol, Rowspan, colspan, Name, DoRong);
                }
                else
                {
                    Sheet1.Cells[nRow, nCol, nRow + Rowspan - 1, nCol + colspan - 1].Merge = true;
                    Sheet1.Cells[nRow, nCol].Value = Name;
                    //Sheet1.Cells[nRow, nCol, nRow + Rowspan - 1, nCol + colspan - 1].AutoFitColumns();
                    if (colspan == 1)
                        Sheet1.Column(nCol).Width = DoRong / 8;
                    nCol += colspan;
                }
            }
            else
            {
                bool merge = Sheet1.Cells[nRow, nCol].Merge;
                if (merge == true)
                {
                    nCol++;
                    nCol = ExportHeader_ChiTietHeader(Sheet1, nRow, nCol, Rowspan, colspan, Name, DoRong);
                    return nCol;
                }
                else
                {
                    Sheet1.Cells[nRow, nCol].Value = Name;
                    if (colspan == 1)
                        Sheet1.Column(nCol).Width = DoRong / 8;
                    //Sheet1.Cells[nRow, nCol].AutoFitColumns();
                    nCol += colspan;
                }
            }

            return nCol;
        }
        public dynamic ExportBaoCaoCongViec(dynamic dyn, ApplicationDbContext db)
        {
            string sFile = "BaoCaoCongViec.xlsx";
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "BaoCaoCongViec" + dt.ToOADate() + ".xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            dyn.CurrentPage = 0;
            ExcelWorksheet sheet1 = package.Workbook.Worksheets["Mẫu"];
            IEnumerable<HRM_BaoCaoCongViec_NhanVien> result = from a in db.HRM_BaoCaoCongViec_NhanVien

                                                              select a;
            sheet1.Cells[1, 3].Value = "BÁO CÁO CÔNG VIỆC THÁNG " + TimKiem.Thang + "/" + TimKiem.Nam;
            DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
            DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
            if (TimKiem.Nam > 0)
                result = result.Where(a => a.Nam == TimKiem.Nam);
            if (TimKiem.IdBoPhan > 0)
                result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);
            if (TimKiem.Thang > 0)
                result = result.Where(a => a.Thang == TimKiem.Thang);
            if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                result = result.Where(a => a.HoTen.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
            var itemList = result.ToList();
            List<string> listId = itemList.Select(a => a.IdGuid).Distinct().ToList();
            var listNoiDung = db.HRM_BaoCaoCongViec_Item.Where(q => listId.Contains(q.IdBaoCao) && q.isHeader == false).OrderBy(a => a.SapXep).ToList();
            HRM_BaoCaoCongViec_NhanVien QuyTrinh = GetListHeader(TimKiem.IdBoPhan ?? 0, db);
            var BoPhan = _CallWebService.GetBoPhan(db, TimKiem.IdBoPhan ?? 0);
            if (itemList != null && itemList.Count > 0)
            {
                int ncol = 1;
                foreach (var item in itemList)
                {
                    if (sheet1 != null && sheet1.Dimension != null)
                        try
                        {
                            ExcelWorksheet NhanVien = package.Workbook.Worksheets.Add(item.Ma, sheet1);
                            #region header
                            int nRow = 4;
                            int nCol = 1;
                            NhanVien.Cells[nRow, nCol].Value = "STT";
                            NhanVien.Cells[2, 1].Value = BoPhan.TenBoPhan;
                            int nTCol = QuyTrinh.listHeader.FirstOrDefault().listHeader.Sum(a => a.colspan ?? 1) + 1;
                            int Cap = QuyTrinh.listHeader.OrderByDescending(a => a.Header).FirstOrDefault().Header;
                            NhanVien.Cells[nRow, nCol, nRow + Cap - 1, nCol].Merge = true;
                            nCol++;
                            int Col = 2;
                            foreach (var header in QuyTrinh.listHeader)
                            {
                                Col = 2;
                                foreach (var itemHeader in header.listHeader)
                                {

                                    int colspan = itemHeader.colspan ?? 1;
                                    int Rowspan = itemHeader.rowspan ?? 1;
                                    //try
                                    //{
                                    if (colspan > 1 || Rowspan > 1)
                                    {
                                        bool merge = NhanVien.Cells[nRow, Col].Merge;
                                        if (merge == true)
                                        {
                                            Col++;
                                            if (colspan == 1)
                                                NhanVien.Column(Col).Width = 30;
                                            Col = ExportHeader_ChiTietHeader(NhanVien, nRow, Col, Rowspan, colspan, itemHeader.Ten, 100);

                                        }
                                        else
                                        {
                                            NhanVien.Cells[nRow, Col, nRow + Rowspan - 1, Col + colspan - 1].Merge = true;
                                            NhanVien.Cells[nRow, Col].Value = itemHeader.Ten;
                                            if (colspan == 1)
                                                NhanVien.Column(Col).Width = 30;
                                            //NhanVien.Cells[nRow, Col, nRow + Rowspan - 1, Col + colspan - 1].AutoFitColumns();
                                            Col += colspan;
                                        }
                                    }
                                    else
                                    {
                                        bool merge = NhanVien.Cells[nRow, Col].Merge;
                                        if (merge == true)
                                        {
                                            Col++;
                                            if (colspan == 1)
                                                NhanVien.Column(Col).Width = 30;
                                            Col = ExportHeader_ChiTietHeader(NhanVien, nRow, Col, Rowspan, colspan, itemHeader.Ten, 100);
                                        }
                                        else
                                        {
                                            NhanVien.Cells[nRow, Col].Value = itemHeader.Ten;
                                            NhanVien.Column(Col).Width = 30;
                                            //NhanVien.Cells[nRow, Col].AutoFitColumns();
                                            Col += colspan;
                                        }
                                    }

                                }
                                nRow++;


                            }

                            //NhanVien.Cells[1, 1, 1, nTCol].Merge = true;
                            NhanVien.Cells[3, 1, nRow + 1, nTCol].Style.Font.Bold = true;
                            NhanVien.Cells[nRow, nCol, nRow + 1, nTCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                            //nRow++;
                            int RowBegin = nRow;

                            #endregion
                            NhanVien.Cells[2, 3].Value = "Nhân viên: " + item.HoTen;


                            var listNd = listNoiDung.Where(a => a.IdBaoCao == item.IdGuid).OrderBy(a => a.SapXep).ToList();
                            var HRM_BaoCaoCongViec_ItemView = new List<HRM_BaoCaoCongViec_ItemView>();
                            var listSTT = listNd.Select(a => a.STT).Distinct().ToList();
                            foreach (var STT in listSTT)
                            {

                                var listGiaTri = listNd.Where(a => a.STT == STT).OrderBy(a => a.SapXep).ToList();
                                var listSTT_Root = listGiaTri.Where(a => !string.IsNullOrEmpty(a.STT_Root)).Select(a => a.STT_Root).Distinct().ToList();
                                if (listSTT_Root != null && listSTT_Root.Count > 0)
                                {
                                    foreach (var Stt_Root in listSTT_Root)
                                    {
                                        var listSTT_Cha = listGiaTri.Where(a => !string.IsNullOrEmpty(a.STT_Cha) && a.STT_Root == Stt_Root).Select(a => a.STT_Cha).Distinct().ToList();
                                        if (listSTT_Cha != null && listSTT_Cha.Count > 0)
                                        {
                                            foreach (var STT_Cha in listSTT_Cha)
                                            {
                                                var itemT = new HRM_BaoCaoCongViec_ItemView();

                                                itemT.STT = STT;
                                                itemT.STT_Cha = STT_Cha;
                                                itemT.STT_Root = Stt_Root;
                                                itemT.listGiaTri = listNd.Where(a => a.STT == itemT.STT && a.STT_Root == Stt_Root).OrderBy(a => a.SapXep).ToList();
                                                itemT.STT_Cha = itemT.listGiaTri.FirstOrDefault().STT_Cha;
                                                HRM_BaoCaoCongViec_ItemView.Add(itemT);
                                            }
                                        }
                                        else
                                        {
                                            var itemT = new HRM_BaoCaoCongViec_ItemView();
                                            itemT.STT = STT;
                                            itemT.STT_Root = Stt_Root;
                                            itemT.listGiaTri = listNd.Where(a => a.STT == itemT.STT).OrderBy(a => a.SapXep).ToList();
                                            itemT.STT_Cha = itemT.listGiaTri.FirstOrDefault().STT_Cha;
                                            HRM_BaoCaoCongViec_ItemView.Add(itemT);
                                        }
                                    }
                                }
                            }
                            var listNoiDungQT = HRM_BaoCaoCongViec_ItemView.Where(a => string.IsNullOrEmpty(a.STT_Cha)).Select(a => new ReturnData<HRM_BaoCaoCongViec_ItemView>
                            {
                                data = a,
                                children = new List<ReturnData<HRM_BaoCaoCongViec_ItemView>>()
                            }).ToList();
                            if (listNoiDungQT != null && listNoiDungQT.Count > 0)
                            {
                                foreach (var nd in listNoiDungQT)
                                    BaoCaoCongViec_ItemToTree(HRM_BaoCaoCongViec_ItemView, nd);
                            }
                            foreach (var STT in listNoiDungQT)
                            {

                                ncol = 1;
                                NhanVien.Cells[nRow, ncol].Value = STT.data.STT;
                                ncol++;
                                foreach (var nd in STT.data.listGiaTri)
                                {
                                    if (!string.IsNullOrEmpty(nd.NoiDung))
                                    {
                                        NhanVien.Cells[nRow, ncol].Value = nd.NoiDung;
                                    }
                                    else if (nd.GiaTri != null && nd.GiaTri > 0)
                                    {
                                        NhanVien.Cells[nRow, ncol].Value = nd.GiaTri;
                                        NhanVien.Cells[nRow, ncol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                                    }
                                    //NhanVien.Column(nCol).Width = 100;
                                    ncol++;
                                }
                                if (STT.children != null && STT.children.Count > 0)
                                    NhanVien.Cells[nRow, 1, nRow, ncol].Style.Font.Bold = true;
                                nRow++;
                                nRow = ExportBaoCaoCongViecChildren(NhanVien, STT.children, nRow);
                            }
                            //ncol--;
                            nRow--;
                            DrawTableExcel(NhanVien, 4, nRow, 1, ncol);

                            NhanVien.Cells.Style.Font.Name = "Times New Roman";
                            NhanVien.Cells.Style.Font.Size = 12;
                            NhanVien.Cells.Style.WrapText = true;
                            NhanVien.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            NhanVien.Cells[5, 2, nRow, ncol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            NhanVien.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi để debug
                            Console.WriteLine("Lỗi khi clone sheet: " + ex.Message);
                            return ex.ToString();
                        }

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
        public HRM_BaoCaoCongViec_NhanVien GetListHeader(long IdBoPhan, ApplicationDbContext db)
        {
            HRM_BaoCaoCongViec_NhanVien result = new HRM_BaoCaoCongViec_NhanVien();
            List<clsBangTinhLuong_Header> listResult = new List<clsBangTinhLuong_Header>();
            var BoPhan_QT = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBaoCaoCongViec" && a.IdBoPhan == IdBoPhan).ToList();
            List<string> listIds = BoPhan_QT.Select(a => a.IdQuyTrinh).Distinct().ToList();
            var list = db.HRM_dmThietLapBaoCaoCongViec.Where(a => listIds.Contains(a.IdGuid)).OrderByDescending(a => a.Created).FirstOrDefault();
            if (list == null)
                return result;
            var listLabel = db.HRM_dmThietLapBaoCaoCongViec_ChiTiet.Where(a => a.IdDanhMuc == list.IdGuid).OrderBy(a => a.SapXep).ToList();

            int CapHeader = listLabel.OrderByDescending(a => a.Header).Select(a => a.Header).FirstOrDefault();
            for (int i = CapHeader; i >= 1; i--)
            {

                HRM_HeaderCongViec Cap = new HRM_HeaderCongViec();
                Cap.Header = i;
                Cap.listHeader = new List<HRM_BaoCaoCongViec_Item>();
                var listCapheader = listLabel.Where(a => a.Header == i).OrderBy(a => a.SapXep).ToList();
                foreach (var itemAdd in listCapheader)
                {
                    //var itemAdd = new clsBangTinhLuong_Header(item);
                    var child = listLabel.Where(a => !string.IsNullOrEmpty(a.IdKey) && a.IdKey.Contains(itemAdd.IdGuid) && a.IdGuid != itemAdd.IdGuid).ToList();
                    if (child == null || child.Count == 0)
                        itemAdd.rowspan = CapHeader - i + 1;
                    else
                    {
                        itemAdd.rowspan = 1;

                    }
                    child = listLabel.Where(a => a.IdParent == itemAdd.IdGuid).ToList();
                    if (child == null || child.Count == 0)
                    {
                        itemAdd.colspan = 1;
                        result.listHeaderData.Add(new HRM_BaoCaoCongViec_Item(itemAdd));
                    }
                    else
                    {
                        itemAdd.colspan = child.Sum(a => a.colspan);
                        //itemAdd.DoRong = 100;
                    }



                    Cap.listHeader.Add(new HRM_BaoCaoCongViec_Item(itemAdd));
                }
                result.listHeader.Add(Cap);

            }
            result.listHeaderData = result.listHeaderData.OrderBy(a => a.STT).ToList();
            result.listHeader = result.listHeader.OrderBy(a => a.Header).ToList();

            return result;
        }
        public HRM_BaoCaoCongViec_NhanVien GetListHeaderData(List<HRM_BaoCaoCongViec_Item> listLabel, HRM_BaoCaoCongViec_NhanVien result)
        {
            List<clsBangTinhLuong_Header> listResult = new List<clsBangTinhLuong_Header>();

            int CapHeader = listLabel.OrderByDescending(a => a.Header).Select(a => a.Header).FirstOrDefault();
            for (int i = CapHeader; i >= 1; i--)
            {

                HRM_HeaderCongViec Cap = new HRM_HeaderCongViec();
                Cap.Header = i;
                Cap.listHeader = new List<HRM_BaoCaoCongViec_Item>();
                var listCapheader = listLabel.Where(a => a.Header == i).OrderBy(a => a.SapXep).ToList();
                foreach (var itemAdd in listCapheader)
                {
                    //var itemAdd = new clsBangTinhLuong_Header(item);
                    var child = listLabel.Where(a => !string.IsNullOrEmpty(a.IdKey) && a.IdKey.Contains(itemAdd.IdGuid) && a.IdGuid != itemAdd.IdGuid).ToList();
                    if (child == null || child.Count == 0)
                        itemAdd.rowspan = CapHeader - i + 1;
                    else
                    {
                        itemAdd.rowspan = 1;

                    }
                    child = listLabel.Where(a => a.IdParent == itemAdd.IdGuid).ToList();
                    if (child == null || child.Count == 0)
                    {
                        itemAdd.colspan = 1;
                        result.listHeaderData.Add(itemAdd);
                    }
                    else
                    {
                        itemAdd.colspan = child.Sum(a => a.colspan);
                        //itemAdd.DoRong = 100;
                    }



                    Cap.listHeader.Add(itemAdd);
                }
                result.listHeader.Add(Cap);

            }
            result.listHeaderData = result.listHeaderData.OrderBy(a => a.STT).ToList();
            result.listHeader = result.listHeader.OrderBy(a => a.Header).ToList();

            return result;
        }

        public int ExportBaoCaoCongViecChildren(ExcelWorksheet NhanVien, List<ReturnData<HRM_BaoCaoCongViec_ItemView>> listNd, int nRow)
        {

            foreach (var STT in listNd)
            {

                int ncol = 1;
                NhanVien.Cells[nRow, ncol].Value = STT.data.STT;
                ncol++;
                foreach (var nd in STT.data.listGiaTri)
                {
                    if (!string.IsNullOrEmpty(nd.NoiDung))
                    {
                        NhanVien.Cells[nRow, ncol].Value = nd.NoiDung;
                    }
                    else if (nd.GiaTri != null && nd.GiaTri > 0)
                    {
                        NhanVien.Cells[nRow, ncol].Value = nd.GiaTri;
                        NhanVien.Cells[nRow, ncol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                    }
                    //NhanVien.Column(nCol).Width = 100;
                    ncol++;
                }
                if (STT.children != null && STT.children.Count > 0)
                    NhanVien.Cells[nRow, 1, nRow, ncol].Style.Font.Bold = true;
                else
                    NhanVien.Cells[nRow, 1, nRow, ncol].Style.Font.Bold = false;

                nRow++;
                nRow = ExportBaoCaoCongViecChildren(NhanVien, STT.children, nRow);

            }
            return nRow;
        }
    }
    public class HRM_TimKiemDanhGia : EntityRequest
    {
        public long IdCongTy { get; set; }
        public long IdBoPhan { get; set; }
        public int Nam { get; set; }
        public int Thang { get; set; }
    }

    #region Điều chỉnh lịch trực

    #endregion
}