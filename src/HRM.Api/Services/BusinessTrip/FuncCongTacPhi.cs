#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace HRM.Api.Services.CongTacPhi
{
    public class FuncCongTacPhi : FuncBase
    {
        private readonly FuncNhanVien _funcNhanVien;
        private string Host => HrmOptions.Host ?? "0";

        public FuncCongTacPhi(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncCongTacPhi> logger,
            FuncNhanVien funcNhanVien)
            : base(callWebService, workflowEngine, options, logger)
        {
            _funcNhanVien = funcNhanVien;
        }
        public dynamic GetNextMaKeHoachCongTac(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_KeHoachCongTac.Select(x => x.SoPhieu), DefineData.MaKeHoachCongTac));
        public dynamic GetListQuyTrinhKeHoachCongTac(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_KeHoachCongTac> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_KeHoachCongTac
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_KeHoachCongTac
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoPhieu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdBoPhan != null && TimKiem.IdBoPhan > 0)
                    result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);

                var itemList = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listIdNV = itemList.Select(a => a.IdNguoiTaoPhieu).Distinct().ToList().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).Distinct().ToList();
                foreach (HRM_KeHoachCongTac item in itemList)
                {
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNguoiTaoPhieu);
                    if (NhanVien != null)
                    {
                        item.TenNguoiTao = NhanVien.TenDayDu;
                        BoPhan duAn = BoPhans.Find(x => x.Id == NhanVien.IdBoPhanEOS);
                        if (duAn != null)
                            item.TenBoPhan = duAn.TenBoPhan;
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
        public dynamic SetQuyTrinhKeHoachCongTac(HRM_KeHoachCongTac model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var QuyTrinhdb = db.HRM_KeHoachCongTac.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncQuyTrinhKeHoachCongTac(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!SetSauHamUpdate(db, QuyTrinhdb, model, err))
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
        public dynamic ChuyenTiepQuyTrinhKeHoachCongTac(HRM_KeHoachCongTac QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_KeHoachCongTac pcuDatabase = db.HRM_KeHoachCongTac.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdDuAn??0))
                        return err;
                    bool Error = FuncQuyTrinhKeHoachCongTac(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
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



                    if (QuyTrinh.isKetThuc == true)
                    {
                        string data = JsonConvert.SerializeObject(QuyTrinh);
                        HRM_KetQuaCongTac Kquactac = JsonConvert.DeserializeObject<HRM_KetQuaCongTac>(data.ToString());
                        Kquactac.IdGuid = "";
                        Kquactac.isKetThuc = false;
                        Kquactac.IdKeHoachCongTac = QuyTrinh.IdGuid;
                        Kquactac.SoPhieu = GenerateNextCode(db.HRM_KetQuaCongTac.Select(x => x.SoPhieu), DefineData.MaKetQuaCongTac);
                        FuncQuyTrinhKetQuaCongTac(Kquactac, db, err, eQuyTrinh_LoaiHam.Set);
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
        public dynamic KhongDuyetQuyTrinhKeHoachCongTac(HRM_KeHoachCongTac QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachCongTac pcuDatabase = db.HRM_KeHoachCongTac.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = FuncQuyTrinhKeHoachCongTac(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
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
        public dynamic GetByIdQuyTrinhKeHoachCongTac(string Id, ApplicationDbContext db)
        {
            HRM_KeHoachCongTac model = db.HRM_KeHoachCongTac.Find(Id);
            if (model == null) return null;
            model.NgayVeUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayVe);
            model.NgayDiUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayDi);
            model.listNhanVienCongTac = db.HRM_KeHoachCongTac_NhanSuCongTac.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_KeHoachCongTac.ToString(), Id);
            var listChiPhi = db.HRM_KeHoachCongTac_ChiPhi.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            model.listChiPhi = listChiPhi.Where(r => string.IsNullOrEmpty(r.IdNhanVien)).OrderBy(a => a.SapXep).ToList();
            if (!string.IsNullOrEmpty(model.IddmPhuongTien))
            {
                model.listIddmPhuongTien = (model.IddmPhuongTien.Split(',').ToList());
            }
            if (model.listNhanVienCongTac != null && model.listNhanVienCongTac.Count > 0)
            {
                List<string> listIdNhanVien = model.listNhanVienCongTac.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                model.listNhanVienCongTac.ForEach(item =>
                {
                    item.NgayVeUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayVe);
                    item.NgayDiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDi);
                    var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    item.TenNhanVien = nhanVien.TenDayDu;
                    item.IdBoPhanEOS = nhanVien.IdBoPhanEOS;
                    var boPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                    if (boPhan != null)
                        item.TenBoPhan = boPhan.TenBoPhan;
                    item.IdChucVuEOS = nhanVien.IdChucVu;
                    var ChucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                    if (ChucVu != null)
                        item.TenChucVu = ChucVu.TenChucVu;
                    item.listChiPhi = listChiPhi.Where(r => !string.IsNullOrEmpty(r.IdNhanVien) && r.IdNhanVien == item.IdNhanVien).OrderBy(a => a.SapXep).ToList();

                });
            }

            //if (listChiPhi != null && listChiPhi.Count > 0)
            //{
            //    model.listChiPhi = listChiPhi.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_KeHoachCongTac_ChiPhi> { data = a, children = new List<ReturnData<HRM_KeHoachCongTac_ChiPhi>>() }).ToList();
            //    foreach (var item in model.listChiPhi)
            //    {
            //        KeHoachCongTac_ChiPhiToTree(listChiPhi, item);

            //    }
            //    KeHoachCongTac_ChiPhiTinhGia(model.listChiPhi);

            //}

            return model;
        }
        public void KeHoachCongTac_ChiPhiToList(List<ReturnData<HRM_KeHoachCongTac_ChiPhi>> listPhanKhu, List<HRM_KeHoachCongTac_ChiPhi> ListAdd, HRM_KeHoachCongTac_ChiPhi parent = null)
        {
            foreach (var item in listPhanKhu)
            {
                item.data.IdGuid = Guid.NewGuid().ToString();
                if (parent != null)
                {
                    item.data.IdParent = parent.IdGuid;
                    item.data.IddmLoaiChiPhi = parent.IddmLoaiChiPhi;
                }
                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                    KeHoachCongTac_ChiPhiToList(item.children, ListAdd, item.data);
            }
        }
        public void KeHoachCongTac_ChiPhiTinhGia(List<ReturnData<HRM_KeHoachCongTac_ChiPhi>> listPhanKhu)
        {
            foreach (var item in listPhanKhu)
            {
                if (item.children.Count() > 0)
                {
                    KeHoachCongTac_ChiPhiTinhGia(item.children);
                    item.data.ThanhTien = item.children.Sum(a => a.data.ThanhTien);
                }
                else
                {
                    item.data.ThanhTien = (item.data.DonGia ?? 0) * (item.data.SoLuong ?? 0);
                }
            }
        }

        public void KeHoachCongTac_ChiPhiToTree(List<HRM_KeHoachCongTac_ChiPhi> ListAll, ReturnData<HRM_KeHoachCongTac_ChiPhi> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_KeHoachCongTac_ChiPhi>
            {
                data = x
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    KeHoachCongTac_ChiPhiToTree(ListAll, item);
                }
            }

        }
        public dynamic GetDanhSachChiPhiQuyDinh(long IdBoPhan, long IdChucVu, double NgayDi, ApplicationDbContext db)
        {

            try
            {
                DateTime time = dungchung.UnixTimeStampToDateTime(NgayDi);
                List<HRM_KeHoachCongTac_ChiPhi> Listresult = new List<HRM_KeHoachCongTac_ChiPhi>();
                var listData = (from a in db.HRM_dmQuyDinhCongTacPhi
                                where a.IdChucVu == IdChucVu && a.IdBoPhan == a.IdBoPhan && (a.NgayBatDau == null || a.NgayBatDau <= time)
                                group a by a.IddmLoaiChiPhi into g
                                select g.OrderBy(r => r.Created).FirstOrDefault()
                             ).ToList();
                var listdanhmucChiPhi = db.HRM_dmCongTacPhi.ToList();
                var listdmLoaiChiPhi = db.HRM_dmLoaiChiPhiCongTac.ToList();

                foreach (var itemdata in listData)
                {
                    var dmCp = listdanhmucChiPhi.FirstOrDefault(a => a.IdGuid == itemdata.IddmLoaiChiPhi);
                    var dmloaiCp = listdmLoaiChiPhi.FirstOrDefault(a => a.IdGuid == dmCp.IddmLoaiChiPhiCongTac);
                    Listresult.Add(new HRM_KeHoachCongTac_ChiPhi()
                    {
                        IddmQuyDinhChiPhi = itemdata.IdGuid,
                        IddmCongTacPhi = itemdata.IddmLoaiChiPhi,
                        IddmLoaiChiPhi = dmCp.IddmLoaiChiPhiCongTac,
                        NoiDung = dmCp.Ten,
                        TenLoaiChiPhi = dmloaiCp.Ten,
                        DonGia = itemdata.MucPhi,
                    });
                }



                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", Listresult);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public dynamic DeleteQuyTrinhKeHoachCongTac(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_KeHoachCongTac model = db.HRM_KeHoachCongTac.Find(Id);
                var listNhanVienCongTac = db.HRM_KeHoachCongTac_NhanSuCongTac.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                var listChiPhi = db.HRM_KeHoachCongTac_ChiPhi.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                db.HRM_KeHoachCongTac.Remove(model);
                db.HRM_KeHoachCongTac_NhanSuCongTac.RemoveRange(listNhanVienCongTac);
                db.HRM_KeHoachCongTac_ChiPhi.RemoveRange(listChiPhi);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public bool FuncQuyTrinhKeHoachCongTac(HRM_KeHoachCongTac model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.TongNhanSu = model.listNhanVienCongTac.Count;
                if (model.listIddmPhuongTien != null && model.listIddmPhuongTien.Count > 0)
                    model.IddmPhuongTien = string.Join(",", model.listIddmPhuongTien);
                model.NgayDi = dungchung.UnixTimeStampToDateTime(model.NgayDiUnix);
                model.NgayVe = dungchung.UnixTimeStampToDateTime(model.NgayVeUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {

                    model.eAction = DefineData.eAction.KEHOACHCONGTAC.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_KeHoachCongTac.Add(model);


                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var listNhanVienCongTacdb = db.HRM_KeHoachCongTac_NhanSuCongTac.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                var listChiPhidb = db.HRM_KeHoachCongTac_ChiPhi.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();

                db.HRM_KeHoachCongTac_NhanSuCongTac.RemoveRange(listNhanVienCongTacdb);
                db.HRM_KeHoachCongTac_ChiPhi.RemoveRange(listChiPhidb);
                if (model.listNhanVienCongTac != null && model.listNhanVienCongTac.Count > 0)
                {
                    int stt = 1;
                    int isLanhDao = model.listNhanVienCongTac.Where(a => a.isLanhDao == true).Count();
                    foreach (var item in model.listNhanVienCongTac)
                    {
                        item.NgayDi = dungchung.UnixTimeStampToDateTime(item.NgayDiUnix);
                        item.NgayVe = dungchung.UnixTimeStampToDateTime(item.NgayVeUnix);
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        stt++;
                        if (item.listChiPhi != null && item.listChiPhi.Count > 0)
                        {
                            foreach (var itemCP in item.listChiPhi)
                            {
                                itemCP.IdGuid = Guid.NewGuid().ToString();
                                itemCP.IdQuyTrinh = model.IdGuid;
                                itemCP.IdNhanVien = item.IdNhanVien;
                                itemCP.SoNgayDi = (int)(item.NgayVe - item.NgayDi).TotalDays + 1;
                                itemCP.HeSo = 1;
                                if (item.isLanhDao == false && isLanhDao > 0)
                                    itemCP.HeSo = 0.5;
                                itemCP.SapXep = stt;
                                stt++;
                            }
                            db.HRM_KeHoachCongTac_ChiPhi.AddRange(item.listChiPhi);

                        }
                    }
                    db.HRM_KeHoachCongTac_NhanSuCongTac.AddRange(model.listNhanVienCongTac);
                }
                if (model.listChiPhi != null && model.listChiPhi.Count > 0)
                {
                    int stt = 1;
                    foreach (var item in model.listChiPhi)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        stt++;
                    }
                    db.HRM_KeHoachCongTac_ChiPhi.AddRange(model.listChiPhi);

                }
                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_KeHoachCongTac, model.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
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
        public dynamic ExporQuyTrinhKeHoachCongTac(HRM_KeHoachCongTac QuyTrinh, ApplicationDbContext db)
        {
            string sFile = "MauKeHoachCongTac.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "MauKeHoachCongTac.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            if (QuyTrinh.NoiDen == null)
                QuyTrinh.NoiDen = "";
            sheet1.Cells[2, 1].Value = "KẾ HOẠCH CÔNG TÁC TẠI " + QuyTrinh.NoiDen.ToUpper();
            QuyTrinh.NgayDi = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayDiUnix);
            QuyTrinh.NgayVe = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayVeUnix);
            if (QuyTrinh.NgayDiUnix > 0 && QuyTrinh.NgayVeUnix > 0)
                sheet1.Cells[3, 1].Value = "Từ Ngày " + QuyTrinh.NgayDi.ToString("dd/MM/yyyy") + " Đến ngày " + QuyTrinh.NgayVe.ToString("dd/MM/yyyy");
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            List<HRM_KeHoachCongTac_ChiPhi> listCpRieng_NSu = new List<HRM_KeHoachCongTac_ChiPhi>();
            if (QuyTrinh.listNhanVienCongTac != null && QuyTrinh.listNhanVienCongTac.Count > 0)
            {
                listCpRieng_NSu = QuyTrinh.listNhanVienCongTac.SelectMany(a => a.listChiPhi).ToList();
            }
            var listdanhmucChiPhi = db.HRM_dmCongTacPhi.ToList();
            var listIddmCongTacPhiRieng = listCpRieng_NSu.Select(b => b.IddmCongTacPhi).ToList();
            var dmChiPhiRieng = listdanhmucChiPhi.Where(a => listIddmCongTacPhiRieng.Contains(a.IdGuid)).ToList();
            var listIddmCongTacPhiChung = QuyTrinh.listChiPhi.Select(b => b.IddmCongTacPhi).ToList();
            var dmChiPhiChung = listdanhmucChiPhi.Where(a => listIddmCongTacPhiChung.Contains(a.IdGuid)).ToList();
            List<string> listPhuongTien = new List<string>();
            if (QuyTrinh.listIddmPhuongTien != null && QuyTrinh.listIddmPhuongTien.Count > 0)
            {
                listPhuongTien = db.HRM_dmPhuongTien.Where(a => QuyTrinh.listIddmPhuongTien.Contains(a.IdGuid)).Select(a => a.Ten).Distinct().ToList();
            }
            #region header
            int nCol = 12;
            foreach (var item in dmChiPhiRieng)
            {
                sheet1.Cells[6, nCol].Value = item.Ten;
                nCol++;
            }

            foreach (var item in dmChiPhiChung)
            {
                sheet1.Cells[6, nCol].Value = item.Ten;
                nCol++;
            }
            if (nCol > 12)
                sheet1.Cells[5, 12, 5, nCol - 1].Merge = true;
            else
            {
                sheet1.Cells[5, 12, 6, 12].Merge = true;
                nCol++;
            }
            sheet1.Cells[5, nCol].Value = "Phương tiện đi lại";
            sheet1.Cells[5, nCol, 6, nCol].Merge = true;
            sheet1.Cells[5, 1, 6, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells[2, 1, 2, nCol].Merge = true;
            sheet1.Cells[3, 1, 3, nCol].Merge = true;

            #endregion
            int nRow = 7;
            int STT = 1;

            nCol = 1;
            sheet1.Cells[nRow, nCol].Value = STT;
            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            nCol++;
            sheet1.Cells[nRow, nCol].Value = QuyTrinh.CreatedByName;
            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            nCol++;
            string Ngay = "";
            if (QuyTrinh.NgayDiUnix > 0)
            {
                QuyTrinh.NgayDi = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayDiUnix);
                Ngay = QuyTrinh.NgayDi.ToString("dd/MM/yyyy") + " - ";
            }
            if (QuyTrinh.NgayVeUnix > 0)
            {
                QuyTrinh.NgayVe = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayVeUnix);
                Ngay = Ngay + QuyTrinh.NgayVe.ToString("dd/MM/yyyy");
            }
            sheet1.Cells[nRow, nCol].Value = Ngay;
            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            nCol++;
            sheet1.Cells[nRow, nCol].Value = QuyTrinh.NoiDung;
            nCol++;
            sheet1.Cells[nRow, nCol].Value = QuyTrinh.LuongTuVan;
            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
            nCol++;
            sheet1.Cells[nRow, nCol].Value = QuyTrinh.NoiDen;
            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            nCol++;
            sheet1.Cells[nRow, nCol].Value = QuyTrinh.NguoiLienHe;
            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            nCol++;
            sheet1.Cells[nRow, nCol].Value = QuyTrinh.SoDienThoai;
            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            nCol++;
            if (QuyTrinh.CachThucHien == 1)
                sheet1.Cells[nRow, nCol].Value = 1;
            nCol++;
            if (QuyTrinh.CachThucHien == 2)
                sheet1.Cells[nRow, nCol].Value = 1;
            nCol++;
            if (QuyTrinh.CachThucHien == 3)
                sheet1.Cells[nRow, nCol].Value = 1;
            nCol++;
            if (dmChiPhiRieng != null && dmChiPhiRieng.Count > 0)
            {
                foreach (var Cp in dmChiPhiRieng)
                {
                    var ChiPhi = listCpRieng_NSu.Where(a => a.IddmCongTacPhi == Cp.IdGuid);
                    if (ChiPhi != null)
                    {

                        sheet1.Cells[nRow, nCol].Value = (ChiPhi.Sum(a => a.SoLuong * a.DonGia * a.SoNgayDi * a.HeSo));
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    }
                    nCol++;
                }
            }

            foreach (var Cp in dmChiPhiChung)
            {
                var ChiPhi = QuyTrinh.listChiPhi.FirstOrDefault(a => a.IddmCongTacPhi == Cp.IdGuid);
                if (ChiPhi != null)
                {
                    sheet1.Cells[nRow, nCol].Value = ChiPhi.ThanhTien;
                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                }
                nCol++;
            }
            if (dmChiPhiRieng.Count == 0 && dmChiPhiChung.Count == 0)
                nCol++;
            sheet1.Cells[nRow, nCol].Value = string.Join(", ", listPhuongTien);

            STT++;
            dungchung.DrawTableExcel(sheet1, 5, nRow, 1, nCol);
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int Row = nRow += 3;
            sheet1.Cells[Row, 8, Row, nCol].Merge = true;
            sheet1.Cells[Row, 8].Value = "Hà Nội, ngày " + dt.Day + " tháng " + dt.Month + " năm " + dt.Year;
            Row++;
            sheet1.Cells[Row, 1, Row, 2].Merge = true;
            sheet1.Cells[Row, 1].Value = "PHÊ DUYỆT";
            sheet1.Cells[Row, 4, Row, 6].Merge = true;
            sheet1.Cells[Row, 4].Value = "PHỤ TRÁCH NHÂN SỰ";
            sheet1.Cells[Row, 8, Row, 10].Merge = true;
            sheet1.Cells[Row, 8].Value = "BỘ PHẬN TUYỂN DỤNG";
            sheet1.Cells[Row, 12, Row, 14].Merge = true;
            sheet1.Cells[Row, 12].Value = "LẬP BIỂU";
            sheet1.Cells[nRow, 1, Row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells[nRow, 1, Row, 14].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;



            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        // kết quả công tác
        public dynamic GetNextMaKetQuaCongTac(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_KetQuaCongTac.Select(x => x.SoPhieu), DefineData.MaKetQuaCongTac));
        public dynamic GetListQuyTrinhKetQuaCongTac(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_KetQuaCongTac> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_KetQuaCongTac
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_KetQuaCongTac
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoPhieu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdBoPhan != null && TimKiem.IdBoPhan > 0)
                    result = result.Where(a => a.IdBoPhan == TimKiem.IdBoPhan);
                var itemList = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listIdNV = itemList.Select(a => a.IdNguoiTaoPhieu).Distinct().ToList().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).Distinct().ToList();
                foreach (HRM_KetQuaCongTac item in itemList)
                {

                    var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNguoiTaoPhieu);
                    if (NhanVien != null)
                    {
                        item.TenNguoiTao = NhanVien.TenDayDu;
                        BoPhan duAn = BoPhans.Find(x => x.Id == NhanVien.IdBoPhanEOS);
                        if (duAn != null)
                            item.TenBoPhan = duAn.TenBoPhan;
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
        public dynamic SetQuyTrinhKetQuaCongTac(HRM_KetQuaCongTac model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var QuyTrinhdb = db.HRM_KeHoachCongTac.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncQuyTrinhKetQuaCongTac(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!SetSauHamUpdate(db, QuyTrinhdb, model, err))
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
        public dynamic ChuyenTiepQuyTrinhKetQuaCongTac(HRM_KetQuaCongTac QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_KetQuaCongTac pcuDatabase = db.HRM_KetQuaCongTac.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdDuAn??0))
                        return err;
                    bool Error = FuncQuyTrinhKetQuaCongTac(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
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

                    if (QuyTrinh.isKetThuc == true && QuyTrinh.listNhanVienTuyen != null && QuyTrinh.listNhanVienTuyen.Count > 0)
                    {
                        var BoPhan = _CallWebService.GetDanhSachBoPhan(db).FirstOrDefault(a => a.MaBoPhan == "CN");
                        if (BoPhan == null)
                            BoPhan = new BoPhan();
                        var ChucDanh = _CallWebService.GetDanhSachChucDanh(db).FirstOrDefault(a => a.Ma == "CN");
                        if (ChucDanh == null)
                            ChucDanh = new ChucDanh();

                        var ChucVu = _CallWebService.GetDanhSachChucVu(db).FirstOrDefault(a => a.MaChucVu == "CN");
                        if (ChucVu == null)
                            ChucVu = new ChucVu();

                        var funcNhanVien = _funcNhanVien;
                        List<HRM_NhanVien> listNhanSuAdd = new List<HRM_NhanVien>();
                        foreach (var item in QuyTrinh.listNhanVienTuyen)
                        {
                            HRM_NhanVien nhanVien = new HRM_NhanVien();
                            nhanVien.MaNhanVien = item.MaNhanVien;
                            nhanVien.Ho = item.Ho;
                            nhanVien.Ten = item.Ten;
                            nhanVien.TenDayDu = item.TenDayDu;
                            nhanVien.TenDem = item.TenDem;
                            nhanVien.IdCongTyEOS = item.IdCongTyEOS;
                            nhanVien.IdBoPhanEOS = BoPhan.Id;
                            nhanVien.IdChucDanh = ChucDanh.Id;
                            nhanVien.IdChucVu = ChucVu.Id;
                            nhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                            if (item.NgaySinh != null)
                                nhanVien.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                            nhanVien.listSoDienThoai = new List<HRM_NhanVien_SoDienThoai>();
                            if (item.SoDienThoai != null)
                                nhanVien.listSoDienThoai.Add(new HRM_NhanVien_SoDienThoai { SoDienThoai = item.SoDienThoai });
                            nhanVien.listGiayToTuyThan = new List<HRM_NhanVien_GiayToTuyThan>();
                            if (item.CanCuocCongDan != null)
                                nhanVien.listGiayToTuyThan.Add(new HRM_NhanVien_GiayToTuyThan()
                                {
                                    Loai = 1,
                                    So = item.CanCuocCongDan
                                });


                            nhanVien.IdGioiTinh = item.GioiTinh;
                            nhanVien.GioiTinh = item.GioiTinh;
                            err = funcNhanVien.SetThongTinNhanVienTransaction(nhanVien, db);
                            if (err.sError == DefineData.eErrorMessage.LoiCapNhat.ToString())
                                return err;
                        }
                    }
                    if (QuyTrinh.isKetThuc == true && QuyTrinh.listNhanVienCongTac != null && QuyTrinh.listNhanVienCongTac.Count > 0)
                    {
                        var listIdNv = QuyTrinh.listNhanVienCongTac.Select(a => a.IdNhanVien).Distinct().ToList();
                        var NhanViens = db.HRM_NhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();
                        DateTime timeStart = QuyTrinh.listNhanVienCongTac.OrderBy(a => a.NgayDi).FirstOrDefault().NgayDi;
                        timeStart = new DateTime(timeStart.Year, timeStart.Month, timeStart.Day, 0, 0, 0);
                        DateTime timeEnd = QuyTrinh.listNhanVienCongTac.OrderByDescending(a => a.NgayVe).FirstOrDefault().NgayVe;
                        timeEnd = new DateTime(timeEnd.Year, timeEnd.Month, timeEnd.Day, 0, 0, 0);
                        var listCaLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(a => listIdNv.Contains(a.IdNhanVien) && timeStart <= a.Ngay && timeEnd >= a.Ngay).ToList();
                        List<HRM_CheckInOut> listCheckInAdd = new List<HRM_CheckInOut>();
                        foreach (var item in NhanViens)
                        {

                            var caLamViec = QuyTrinh.listNhanVienCongTac.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                            //if (caLamViec != null)
                            //{
                            //    item.SoGioMotNgay = caLamViec.TongSoGioGiao;
                            //    item.TuGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioVao);
                            //    item.DenGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioRa);
                            //}
                            var listLichLamViec = listCaLamViec.Where(a => a.IdNhanVien == item.IdGuid && caLamViec.NgayDi <= a.Ngay && caLamViec.NgayVe >= a.Ngay).ToList();
                            foreach (var lich in listLichLamViec)
                            {
                                DateTime time = lich.Ngay;
                                HRM_CheckInOut In = new HRM_CheckInOut();
                                In.Id = Guid.NewGuid().ToString();
                                In.Loai = DefineData.eKyHieuChamCong.I.ToString();
                                In.Ngay = time;
                                In.IdUser = item.IdUser;
                                In.IdNhanVien = item.IdGuid;
                                In.IdCongTy = lich.IdCongTy;
                                DateTime TuGio = lich.GioVao;
                                In.Gio = new DateTime(time.Year, time.Month, time.Day, TuGio.Hour, TuGio.Minute, TuGio.Second);
                                listCheckInAdd.Add(In);
                                HRM_CheckInOut Out = new HRM_CheckInOut();
                                Out.Id = Guid.NewGuid().ToString();
                                Out.Loai = DefineData.eKyHieuChamCong.O.ToString();
                                Out.Ngay = time;
                                Out.IdUser = item.IdUser;
                                Out.IdNhanVien = item.IdGuid;
                                Out.IdCongTy = lich.IdCongTy;
                                DateTime DenGio = lich.GioRa;
                                Out.Gio = new DateTime(time.Year, time.Month, time.Day, DenGio.Hour, DenGio.Minute, DenGio.Second);
                                listCheckInAdd.Add(Out);
                                lich.isCongTac = true;
                                db.Entry(lich).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                        }

                        db.HRM_CheckInOut.AddRange(listCheckInAdd);
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
        public dynamic KhongDuyetQuyTrinhKetQuaCongTac(HRM_KetQuaCongTac QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachCongTac pcuDatabase = db.HRM_KeHoachCongTac.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = FuncQuyTrinhKetQuaCongTac(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
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
        public dynamic GetByIdQuyTrinhKetQuaCongTac(string Id, ApplicationDbContext db)
        {
            HRM_KetQuaCongTac model = db.HRM_KetQuaCongTac.Find(Id);
            if (model == null) return null;
            model.NgayVeUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayVe);
            model.NgayDiUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayDi);
            model.listNhanVienCongTac = db.HRM_KetQuaCongTac_NhanSuCongTac.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            model.listNhanVienTuyen = db.HRM_KetQuaCongTac_NhanSuTuyen.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            var listChiPhi = db.HRM_KetQuaCongTac_ChiPhi.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            model.listChiPhi = listChiPhi.Where(r => string.IsNullOrEmpty(r.IdNhanVien)).OrderBy(a => a.SapXep).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            if (!string.IsNullOrEmpty(model.IddmPhuongTien))
            {
                model.listIddmPhuongTien = (model.IddmPhuongTien.Split(',').ToList());
            }
            if (model.listNhanVienTuyen != null && model.listNhanVienTuyen.Count > 0)
            {
                model.listNhanVienTuyen.ForEach(item =>
                {
                    if (item.NgaySinh != null)
                        item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                    var boPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                    if (boPhan != null)
                        item.TenCongTy = boPhan.TenBoPhan;
                });
            }
            if (model.listNhanVienCongTac != null && model.listNhanVienCongTac.Count > 0)
            {
                List<string> listIdNhanVien = model.listNhanVienCongTac.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                model.listNhanVienCongTac.ForEach(item =>
                {
                    item.NgayVeUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayVe);
                    item.NgayDiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDi);
                    var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    item.TenNhanVien = nhanVien.TenDayDu;
                    item.IdBoPhanEOS = nhanVien.IdBoPhanEOS;
                    var boPhan = listBoPhan.FirstOrDefault(a => a.Id == nhanVien.IdBoPhanEOS);
                    if (boPhan != null)
                        item.TenBoPhan = boPhan.TenBoPhan;
                    item.IdChucVuEOS = nhanVien.IdChucVu;
                    var ChucVu = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                    if (ChucVu != null)
                        item.TenChucVu = ChucVu.TenChucVu;
                    item.listChiPhi = listChiPhi.Where(r => !string.IsNullOrEmpty(r.IdNhanVien) && r.IdNhanVien == item.IdNhanVien).OrderBy(a => a.SapXep).ToList();
                });
            }

            //model.listChiPhi = db.HRM_KetQuaCongTac_ChiPhi.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();


            return model;
        }
        public void KetQuaCongTac_ChiPhiToList(List<ReturnData<HRM_KetQuaCongTac_ChiPhi>> listPhanKhu, List<HRM_KetQuaCongTac_ChiPhi> ListAdd, HRM_KetQuaCongTac_ChiPhi parent = null)
        {
            foreach (var item in listPhanKhu)
            {
                item.data.IdGuid = Guid.NewGuid().ToString();
                if (parent != null)
                {
                    item.data.IdParent = parent.IdGuid;
                    item.data.IddmLoaiChiPhi = parent.IddmLoaiChiPhi;
                }
                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                    KetQuaCongTac_ChiPhiToList(item.children, ListAdd, item.data);
            }
        }
        public void KetQuaCongTac_ChiPhiTinhGia(List<ReturnData<HRM_KetQuaCongTac_ChiPhi>> listPhanKhu)
        {
            foreach (var item in listPhanKhu)
            {
                if (item.children.Count() > 0)
                {
                    KetQuaCongTac_ChiPhiTinhGia(item.children);
                    item.data.ThanhTien = item.children.Sum(a => a.data.ThanhTien);
                }
                else
                {
                    item.data.ThanhTien = (item.data.DonGia ?? 0) * (item.data.SoLuong ?? 0);
                }
            }
        }

        public void KetQuaCongTac_ChiPhiToTree(List<HRM_KetQuaCongTac_ChiPhi> ListAll, ReturnData<HRM_KetQuaCongTac_ChiPhi> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_KetQuaCongTac_ChiPhi>
            {
                data = x
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    KetQuaCongTac_ChiPhiToTree(ListAll, item);
                }
            }

        }
        public dynamic DeleteQuyTrinhKetQuaCongTac(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_KetQuaCongTac model = db.HRM_KetQuaCongTac.Find(Id);
                var listNhanVienCongTac = db.HRM_KetQuaCongTac_NhanSuCongTac.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                var listChiPhi = db.HRM_KetQuaCongTac_ChiPhi.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                var listNhanVienTuyendb = db.HRM_KetQuaCongTac_NhanSuTuyen.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                db.HRM_KetQuaCongTac.Remove(model);
                db.HRM_KetQuaCongTac_NhanSuCongTac.RemoveRange(listNhanVienCongTac);
                db.HRM_KetQuaCongTac_NhanSuTuyen.RemoveRange(listNhanVienTuyendb);
                db.HRM_KetQuaCongTac_ChiPhi.RemoveRange(listChiPhi);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public bool FuncQuyTrinhKetQuaCongTac(HRM_KetQuaCongTac model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.NgayDi = dungchung.UnixTimeStampToDateTime(model.NgayDiUnix);
                model.NgayVe = dungchung.UnixTimeStampToDateTime(model.NgayVeUnix);
                if (model.listIddmPhuongTien != null && model.listIddmPhuongTien.Count > 0)
                    model.IddmPhuongTien = string.Join(",", model.listIddmPhuongTien);
                if (string.IsNullOrEmpty(model.IdGuid))
                {


                    model.eAction = DefineData.eAction.KETQUACONGTAC.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_KetQuaCongTac.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var listNhanVienCongTacdb = db.HRM_KetQuaCongTac_NhanSuCongTac.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                var listNhanVienTuyendb = db.HRM_KetQuaCongTac_NhanSuTuyen.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                var listChiPhidb = db.HRM_KetQuaCongTac_ChiPhi.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                db.HRM_KetQuaCongTac_NhanSuTuyen.RemoveRange(listNhanVienTuyendb);
                db.HRM_KetQuaCongTac_NhanSuCongTac.RemoveRange(listNhanVienCongTacdb);
                db.HRM_KetQuaCongTac_ChiPhi.RemoveRange(listChiPhidb);
                if (model.listNhanVienCongTac != null && model.listNhanVienCongTac.Count > 0)
                {
                    int stt = 1;
                    int isLanhDao = model.listNhanVienCongTac.Where(a => a.isLanhDao == true).Count();

                    foreach (var item in model.listNhanVienCongTac)
                    {
                        item.NgayDi = dungchung.UnixTimeStampToDateTime(item.NgayDiUnix);
                        item.NgayDi = new DateTime(item.NgayDi.Year, item.NgayDi.Month, item.NgayDi.Day, 0, 0, 0);
                        item.NgayVe = dungchung.UnixTimeStampToDateTime(item.NgayVeUnix);
                        item.NgayVe = new DateTime(item.NgayVe.Year, item.NgayVe.Month, item.NgayVe.Day, 0, 0, 0);

                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        stt++;
                        if (item.listChiPhi != null && item.listChiPhi.Count > 0)
                        {
                            foreach (var itemCP in item.listChiPhi)
                            {
                                itemCP.IdGuid = Guid.NewGuid().ToString();
                                itemCP.IdQuyTrinh = model.IdGuid;
                                itemCP.NgayBaoCao = model.NgayDi;
                                itemCP.IdNhanVien = item.IdNhanVien;
                                itemCP.SoNgayDi = (int)(item.NgayVe - item.NgayDi).TotalDays + 1;
                                itemCP.HeSo = 1;
                                if (item.isLanhDao == false && isLanhDao > 0)
                                    itemCP.HeSo = 0.5;
                                itemCP.SapXep = stt;
                                stt++;
                            }
                            db.HRM_KetQuaCongTac_ChiPhi.AddRange(item.listChiPhi);
                        }
                    }
                    db.HRM_KetQuaCongTac_NhanSuCongTac.AddRange(model.listNhanVienCongTac);
                }
                if (model.listNhanVienTuyen != null && model.listNhanVienTuyen.Count > 0)
                {
                    int stt = 1;
                    foreach (var item in model.listNhanVienTuyen)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        if (item.NgaySinhUnix > 0)
                            item.NgaySinh = dungchung.UnixTimeStampToDateTime(item.NgaySinhUnix);
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        stt++;
                    }
                    db.HRM_KetQuaCongTac_NhanSuTuyen.AddRange(model.listNhanVienTuyen);
                }

                if (model.listChiPhi != null && model.listChiPhi.Count > 0)
                {
                    int stt = 1;
                    foreach (var item in model.listChiPhi)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = stt;
                        item.NgayBaoCao = model.NgayDi;
                        item.isKetThuc = model.isKetThuc;
                        stt++;
                    }
                    db.HRM_KetQuaCongTac_ChiPhi.AddRange(model.listChiPhi);

                }
                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_KeHoachCongTac, model.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return false;
                db.SaveChanges();


                return true;
            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
                return true;
            }

        }

        public ErrorMessage GetBaoCaoChiPhiCongTac(ApplicationDbContext db, int Nam)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                List<ReturnData<HRM_BaoCaoCongTacPhi>> listResult = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                var listdata = db.HRM_KetQuaCongTac_ChiPhi.Where(a => a.NgayBaoCao.Year == Nam && a.isKetThuc == true).ToList();
                if (listdata != null && listdata.Count > 0)
                {
                    List<string> listIdQuyTrinh = listdata.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    var listQuyTrinh = db.HRM_KetQuaCongTac.Where(a => listIdQuyTrinh.Contains(a.IdGuid)).ToList();

                    List<string> listIdNhom = listdata.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                    var listNhomLoaiChiPhi = db.HRM_dmLoaiChiPhiCongTac.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                    List<string> listIdCongTacPhi = listdata.Select(a => a.IddmCongTacPhi).Distinct().ToList();
                    var listCongTacPhi = db.HRM_dmCongTacPhi.Where(a => listIdCongTacPhi.Contains(a.IdGuid)).ToList();
                    foreach (var itemQT in listQuyTrinh)
                    {
                        ReturnData<HRM_BaoCaoCongTacPhi> itemAddQT = new ReturnData<HRM_BaoCaoCongTacPhi>();
                        itemAddQT.data = new HRM_BaoCaoCongTacPhi();
                        itemAddQT.data.TenChiPhi = (itemQT.NoiDen ?? "") + "-" + (itemQT.NoiDung ?? "");
                        itemAddQT.data.IdQuyTrinh = itemQT.IdGuid;
                        itemAddQT.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                        itemAddQT.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                        var listDataQT = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid).ToList();
                        listIdNhom = listDataQT.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                        var listNhomLoaiChiPhiQT = listNhomLoaiChiPhi.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                        listIdCongTacPhi = listdata.Select(a => a.IddmCongTacPhi).Distinct().ToList();
                        var listCongTacPhiQT = listCongTacPhi.Where(a => listIdCongTacPhi.Contains(a.IdGuid)).ToList();
                        foreach (var item in listNhomLoaiChiPhiQT)
                        {
                            ReturnData<HRM_BaoCaoCongTacPhi> itemAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();
                            itemAdd.data = new HRM_BaoCaoCongTacPhi();
                            itemAdd.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                            itemAdd.data.TenChiPhi = item.Ten;
                            itemAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                            var listCTPhi = listCongTacPhiQT.Where(a => a.IddmLoaiChiPhiCongTac == item.IdGuid).ToList();
                            foreach (var CTPhi in listCTPhi)
                            {
                                ReturnData<HRM_BaoCaoCongTacPhi> itemChildAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();

                                itemChildAdd.data = new HRM_BaoCaoCongTacPhi();
                                itemChildAdd.data.TenChiPhi = CTPhi.Ten;
                                itemChildAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                                for (int i = 1; i <= 12; i++)
                                {
                                    HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                    thang.Thang = i;
                                    var data = listdata.Where(a => a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                                    if (data != null && data.Count > 0)
                                        thang.ChiPhi = data.Sum(a => a.ThanhTien);
                                    itemChildAdd.data.listThang.Add(thang);
                                }
                                itemAdd.children.Add(itemChildAdd);
                            }
                            for (int i = 1; i <= 12; i++)
                            {
                                HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                thang.Thang = i;
                                var data = itemAdd.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                                if (data != null && data.Count > 0)
                                    thang.ChiPhi = data.Sum(a => a.ChiPhi);
                                itemAdd.data.listThang.Add(thang);
                            }
                            itemAddQT.children.Add(itemAdd);

                        }
                        for (int i = 1; i <= 12; i++)
                        {
                            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                            thang.Thang = i;
                            var data = itemAddQT.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                            if (data != null && data.Count > 0)
                                thang.ChiPhi = data.Sum(a => a.ChiPhi);
                            itemAddQT.data.listThang.Add(thang);
                        }
                        listResult.Add(itemAddQT);
                    }
                    //foreach (var item in listNhomLoaiChiPhi)
                    //{
                    //    ReturnData<HRM_BaoCaoCongTacPhi> itemAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();
                    //    itemAdd.data = new HRM_BaoCaoCongTacPhi();
                    //    itemAdd.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                    //    itemAdd.data.TenChiPhi = item.Ten;
                    //    itemAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                    //    var listCTPhi = listCongTacPhi.Where(a => a.IddmLoaiChiPhiCongTac == item.IdGuid).ToList();
                    //    foreach (var CTPhi in listCTPhi)
                    //    {
                    //        ReturnData<HRM_BaoCaoCongTacPhi> itemChildAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();

                    //        itemChildAdd.data = new HRM_BaoCaoCongTacPhi();
                    //        itemChildAdd.data.TenChiPhi = CTPhi.Ten;
                    //        itemChildAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                    //        for (int i = 1; i <= 12; i++)
                    //        {
                    //            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                    //            thang.Thang = i;
                    //            var data = listdata.Where(a => a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                    //            if (data != null && data.Count > 0)
                    //                thang.ChiPhi = data.Sum(a => a.ThanhTien);
                    //            itemChildAdd.data.listThang.Add(thang);
                    //        }
                    //        itemAdd.children.Add(itemChildAdd);
                    //    }
                    //    for (int i = 1; i <= 12; i++)
                    //    {
                    //        HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                    //        thang.Thang = i;
                    //        var data = itemAdd.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                    //        if (data != null && data.Count > 0)
                    //            thang.ChiPhi = data.Sum(a => a.ChiPhi);
                    //        itemAdd.data.listThang.Add(thang);
                    //    }
                    //    listResult.Add(itemAdd);

                    //}
                }
                err.Data = listResult;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage GetBaoCaoChiPhiCongTacNew(ApplicationDbContext db, int Nam, List<long> listIdBoPhan = null)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                List<ReturnData<HRM_BaoCaoCongTacPhi>> listResult = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                var listdata = db.HRM_KetQuaCongTac_ChiPhi.Where(a => a.NgayBaoCao.Year == Nam && a.isKetThuc == true).ToList();
                if (listdata != null && listdata.Count > 0)
                {
                    List<string> listIdQuyTrinh = listdata.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    var listQuyTrinh = db.HRM_KetQuaCongTac.Where(a => listIdQuyTrinh.Contains(a.IdGuid)).ToList();
                    var listQuyTrinh_Nsu = db.HRM_KetQuaCongTac_NhanSuCongTac.Where(a => listIdQuyTrinh.Contains(a.IdQuyTrinh)).ToList();
                    List<string> listIdNhanVien = listQuyTrinh_Nsu.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

                    if (listIdBoPhan != null && listIdBoPhan.Count > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => listIdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
                    }
                    listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    listdata = listdata.Where(a => string.IsNullOrEmpty(a.IdNhanVien) || listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    List<string> listIdNhom = listdata.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                    var listNhomLoaiChiPhi = db.HRM_dmLoaiChiPhiCongTac.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                    List<string> listIdCongTacPhi = listdata.Select(a => a.IddmCongTacPhi).Distinct().ToList();
                    var listCongTacPhi = db.HRM_dmCongTacPhi.Where(a => listIdCongTacPhi.Contains(a.IdGuid)).ToList();
                    foreach (var itemQT in listQuyTrinh)
                    {
                        int CountNV = listQuyTrinh_Nsu.Where(a => a.IdQuyTrinh == itemQT.IdGuid).Count();
                        if (CountNV == 0)
                            CountNV = 1;
                        var ListNV_QT = listQuyTrinh_Nsu.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).Select(a => a.IdNhanVien).ToList();
                        int CountNV_BP = ListNV_QT.Count();
                        ReturnData<HRM_BaoCaoCongTacPhi> itemAddQT = new ReturnData<HRM_BaoCaoCongTacPhi>();
                        itemAddQT.data = new HRM_BaoCaoCongTacPhi();
                        itemAddQT.data.TenChiPhi = (itemQT.NoiDen ?? "") + "-" + (itemQT.NoiDung ?? "");
                        itemAddQT.data.IdQuyTrinh = itemQT.IdGuid;
                        itemAddQT.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                        itemAddQT.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                        var listDataQT = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid).ToList();
                        listIdNhom = listDataQT.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                        var listNhomLoaiChiPhiQT = listNhomLoaiChiPhi.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                        listIdCongTacPhi = listdata.Select(a => a.IddmCongTacPhi).Distinct().ToList();
                        var listCongTacPhiQT = listCongTacPhi.Where(a => listIdCongTacPhi.Contains(a.IdGuid)).ToList();
                        foreach (var item in listNhomLoaiChiPhiQT)
                        {
                            ReturnData<HRM_BaoCaoCongTacPhi> itemAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();
                            itemAdd.data = new HRM_BaoCaoCongTacPhi();
                            itemAdd.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                            itemAdd.data.TenChiPhi = item.Ten;
                            itemAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                            var listCTPhi = listCongTacPhiQT.Where(a => a.IddmLoaiChiPhiCongTac == item.IdGuid).ToList();
                            foreach (var CTPhi in listCTPhi)
                            {
                                ReturnData<HRM_BaoCaoCongTacPhi> itemChildAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();

                                itemChildAdd.data = new HRM_BaoCaoCongTacPhi();
                                itemChildAdd.data.TenChiPhi = CTPhi.Ten;
                                itemChildAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                                for (int i = 1; i <= 12; i++)
                                {
                                    HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                    thang.Thang = i;
                                    thang.ChiPhi = 0;
                                    var dataR = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid && !string.IsNullOrEmpty(a.IdNhanVien) && ListNV_QT.Contains(a.IdNhanVien) && a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                                    if (dataR != null && dataR.Count > 0)
                                        thang.ChiPhi = dataR.Sum(a => a.ThanhTien);
                                    var dataC = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid && string.IsNullOrEmpty(a.IdNhanVien) && a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                                    if (dataC != null && dataC.Count > 0)
                                        thang.ChiPhi = thang.ChiPhi + (dataC.Sum(a => a.ThanhTien) * CountNV_BP / CountNV);

                                    thang.ListNhanVien = new List<HRM_BaoCaoCongTacPhi_NhanVien>();
                                    foreach (var IDNV in ListNV_QT)
                                    {
                                        var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == IDNV);
                                        thang.ListNhanVien.Add(new HRM_BaoCaoCongTacPhi_NhanVien
                                        {
                                            TenNhanVien = NhanVien.MaNhanVien + " - " + NhanVien.TenDayDu,
                                            SoTien = dataR.Where(a => a.IdNhanVien == IDNV).Sum(a => a.ThanhTien) + (dataC.Sum(a => a.ThanhTien) / CountNV)
                                        });
                                    }
                                    itemChildAdd.data.listThang.Add(thang);
                                }
                                itemAdd.children.Add(itemChildAdd);
                            }
                            for (int i = 1; i <= 12; i++)
                            {
                                HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                thang.Thang = i;
                                var data = itemAdd.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                                if (data != null && data.Count > 0)
                                    thang.ChiPhi = data.Sum(a => a.ChiPhi);
                                itemAdd.data.listThang.Add(thang);
                            }
                            itemAddQT.children.Add(itemAdd);

                        }
                        for (int i = 1; i <= 12; i++)
                        {
                            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                            thang.Thang = i;
                            var data = itemAddQT.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                            if (data != null && data.Count > 0)
                                thang.ChiPhi = data.Sum(a => a.ChiPhi);
                            itemAddQT.data.listThang.Add(thang);
                        }
                        listResult.Add(itemAddQT);
                    }
                    //foreach (var item in listNhomLoaiChiPhi)
                    //{
                    //    ReturnData<HRM_BaoCaoCongTacPhi> itemAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();
                    //    itemAdd.data = new HRM_BaoCaoCongTacPhi();
                    //    itemAdd.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                    //    itemAdd.data.TenChiPhi = item.Ten;
                    //    itemAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                    //    var listCTPhi = listCongTacPhi.Where(a => a.IddmLoaiChiPhiCongTac == item.IdGuid).ToList();
                    //    foreach (var CTPhi in listCTPhi)
                    //    {
                    //        ReturnData<HRM_BaoCaoCongTacPhi> itemChildAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();

                    //        itemChildAdd.data = new HRM_BaoCaoCongTacPhi();
                    //        itemChildAdd.data.TenChiPhi = CTPhi.Ten;
                    //        itemChildAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                    //        for (int i = 1; i <= 12; i++)
                    //        {
                    //            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                    //            thang.Thang = i;
                    //            var data = listdata.Where(a => a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                    //            if (data != null && data.Count > 0)
                    //                thang.ChiPhi = data.Sum(a => a.ThanhTien);
                    //            itemChildAdd.data.listThang.Add(thang);
                    //        }
                    //        itemAdd.children.Add(itemChildAdd);
                    //    }
                    //    for (int i = 1; i <= 12; i++)
                    //    {
                    //        HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                    //        thang.Thang = i;
                    //        var data = itemAdd.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                    //        if (data != null && data.Count > 0)
                    //            thang.ChiPhi = data.Sum(a => a.ChiPhi);
                    //        itemAdd.data.listThang.Add(thang);
                    //    }
                    //    listResult.Add(itemAdd);

                    //}
                }
                err.Data = listResult;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage GetBaoCaoChiPhiCongTacNewNew(ApplicationDbContext db, int Nam)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                List<ReturnData<HRM_BaoCaoCongTacPhi>> listResult = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                var listdata = db.HRM_KetQuaCongTac_ChiPhi.Where(a => a.NgayBaoCao.Year == Nam && a.isKetThuc == true).ToList();
                if (listdata != null && listdata.Count > 0)
                {
                    List<string> listIdQuyTrinh = listdata.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    var listQuyTrinh = db.HRM_KetQuaCongTac.Where(a => listIdQuyTrinh.Contains(a.IdGuid)).ToList();
                    var listQuyTrinh_Nsu = db.HRM_KetQuaCongTac_NhanSuCongTac.Where(a => listIdQuyTrinh.Contains(a.IdQuyTrinh)).ToList();
                    List<string> listIdNhanVien = listQuyTrinh_Nsu.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    listdata = listdata.Where(a => string.IsNullOrEmpty(a.IdNhanVien) || listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    List<string> listIdNhom = listdata.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                    var listNhomLoaiChiPhi = db.HRM_dmLoaiChiPhiCongTac.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                    List<string> listIdCongTacPhi = listdata.Select(a => a.IddmCongTacPhi).Distinct().ToList();
                    var listCongTacPhi = db.HRM_dmCongTacPhi.Where(a => listIdCongTacPhi.Contains(a.IdGuid)).ToList();
                    foreach (var itemQT in listQuyTrinh)
                    {
                        //int CountNV = listQuyTrinh_Nsu.Where(a => a.IdQuyTrinh == itemQT.IdGuid).Count();
                        //if (CountNV == 0)
                        //    CountNV = 1;
                        var ListNV_QT = listQuyTrinh_Nsu.Where(a => a.IdQuyTrinh == itemQT.IdGuid && listIdNhanVien.Contains(a.IdNhanVien)).Select(a => a.IdNhanVien).ToList();
                        int CountNV_BP = ListNV_QT.Count();
                        ReturnData<HRM_BaoCaoCongTacPhi> itemAddQT = new ReturnData<HRM_BaoCaoCongTacPhi>();
                        itemAddQT.data = new HRM_BaoCaoCongTacPhi();
                        itemAddQT.data.TenChiPhi = (itemQT.NoiDen ?? "") + "-" + (itemQT.NoiDung ?? "");
                        itemAddQT.data.IdQuyTrinh = itemQT.IdGuid;
                        itemAddQT.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                        itemAddQT.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                        var listDataQT = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid).ToList();
                        listIdNhom = listDataQT.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                        var listNhomLoaiChiPhiQT = listNhomLoaiChiPhi.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                        listIdCongTacPhi = listdata.Select(a => a.IddmCongTacPhi).Distinct().ToList();
                        var listCongTacPhiQT = listCongTacPhi.Where(a => listIdCongTacPhi.Contains(a.IdGuid)).ToList();
                        foreach (var IDNV in ListNV_QT)
                        {

                            var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == IDNV);
                            if (NhanVien != null)
                            {
                                ReturnData<HRM_BaoCaoCongTacPhi> itemAddNV = new ReturnData<HRM_BaoCaoCongTacPhi>();
                                itemAddNV.data = new HRM_BaoCaoCongTacPhi();
                                itemAddNV.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                                itemAddNV.data.TenChiPhi = NhanVien.MaNhanVien + " - " + NhanVien.TenDayDu;
                                itemAddNV.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                                foreach (var item in listNhomLoaiChiPhiQT)
                                {
                                    ReturnData<HRM_BaoCaoCongTacPhi> itemAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();
                                    itemAdd.data = new HRM_BaoCaoCongTacPhi();
                                    itemAdd.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                                    itemAdd.data.TenChiPhi = item.Ten;
                                    itemAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                                    var listCTPhi = listCongTacPhiQT.Where(a => a.IddmLoaiChiPhiCongTac == item.IdGuid).ToList();
                                    foreach (var CTPhi in listCTPhi)
                                    {
                                        ReturnData<HRM_BaoCaoCongTacPhi> itemChildAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();

                                        itemChildAdd.data = new HRM_BaoCaoCongTacPhi();
                                        itemChildAdd.data.TenChiPhi = CTPhi.Ten;
                                        itemChildAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                                        for (int i = 1; i <= 12; i++)
                                        {
                                            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                            thang.Thang = i;
                                            thang.ChiPhi = 0;
                                            var dataR = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid && !string.IsNullOrEmpty(a.IdNhanVien) && a.IdNhanVien == IDNV && a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                                            if (dataR != null && dataR.Count > 0)
                                                thang.ChiPhi = dataR.Sum(a => a.ThanhTien);
                                            var dataC = listdata.Where(a => a.IdQuyTrinh == itemQT.IdGuid && string.IsNullOrEmpty(a.IdNhanVien) && a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                                            if (dataC != null && dataC.Count > 0)
                                                thang.ChiPhi = thang.ChiPhi + (dataC.Sum(a => a.ThanhTien) / CountNV_BP /*/ CountNV*/);

                                            itemChildAdd.data.listThang.Add(thang);
                                        }
                                        itemAdd.children.Add(itemChildAdd);
                                    }
                                    for (int i = 1; i <= 12; i++)
                                    {
                                        HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                        thang.Thang = i;
                                        var data = itemAdd.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                                        if (data != null && data.Count > 0)
                                            thang.ChiPhi = data.Sum(a => a.ChiPhi);
                                        itemAdd.data.listThang.Add(thang);
                                    }
                                    itemAddNV.children.Add(itemAdd);
                                  
                                }
                                for (int i = 1; i <= 12; i++)
                                {
                                    HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                                    thang.Thang = i;
                                    var data = itemAddNV.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                                    if (data != null && data.Count > 0)
                                        thang.ChiPhi = data.Sum(a => a.ChiPhi);
                                    itemAddNV.data.listThang.Add(thang);
                                }
                                itemAddQT.children.Add(itemAddNV);
                            }

                        }

                        for (int i = 1; i <= 12; i++)
                        {
                            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                            thang.Thang = i;
                            var data = itemAddQT.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                            if (data != null && data.Count > 0)
                                thang.ChiPhi = data.Sum(a => a.ChiPhi);
                            itemAddQT.data.listThang.Add(thang);
                        }
                        listResult.Add(itemAddQT);
                    }
                    //foreach (var item in listNhomLoaiChiPhi)
                    //{
                    //    ReturnData<HRM_BaoCaoCongTacPhi> itemAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();
                    //    itemAdd.data = new HRM_BaoCaoCongTacPhi();
                    //    itemAdd.children = new List<ReturnData<HRM_BaoCaoCongTacPhi>>();
                    //    itemAdd.data.TenChiPhi = item.Ten;
                    //    itemAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                    //    var listCTPhi = listCongTacPhi.Where(a => a.IddmLoaiChiPhiCongTac == item.IdGuid).ToList();
                    //    foreach (var CTPhi in listCTPhi)
                    //    {
                    //        ReturnData<HRM_BaoCaoCongTacPhi> itemChildAdd = new ReturnData<HRM_BaoCaoCongTacPhi>();

                    //        itemChildAdd.data = new HRM_BaoCaoCongTacPhi();
                    //        itemChildAdd.data.TenChiPhi = CTPhi.Ten;
                    //        itemChildAdd.data.listThang = new List<HRM_BaoCaoCongTacPhi_Thang>();
                    //        for (int i = 1; i <= 12; i++)
                    //        {
                    //            HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                    //            thang.Thang = i;
                    //            var data = listdata.Where(a => a.IddmCongTacPhi == CTPhi.IdGuid && a.NgayBaoCao.Month == i).ToList();
                    //            if (data != null && data.Count > 0)
                    //                thang.ChiPhi = data.Sum(a => a.ThanhTien);
                    //            itemChildAdd.data.listThang.Add(thang);
                    //        }
                    //        itemAdd.children.Add(itemChildAdd);
                    //    }
                    //    for (int i = 1; i <= 12; i++)
                    //    {
                    //        HRM_BaoCaoCongTacPhi_Thang thang = new HRM_BaoCaoCongTacPhi_Thang();
                    //        thang.Thang = i;
                    //        var data = itemAdd.children.SelectMany(a => a.data.listThang.Where(b => b.Thang == i)).ToList();
                    //        if (data != null && data.Count > 0)
                    //            thang.ChiPhi = data.Sum(a => a.ChiPhi);
                    //        itemAdd.data.listThang.Add(thang);
                    //    }
                    //    listResult.Add(itemAdd);

                    //}
                }
                err.Data = listResult;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }

        public dynamic ExportBaoCaoChiPhiCongTac(ApplicationDbContext db, int Nam)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoChiPhiCongTac.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "BaoCaoChiPhiCongTacDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                sheet1.Cells[5, 3].Value = "Thực chi năm " + Nam + " (đồng)";
                err = GetBaoCaoChiPhiCongTacNewNew(db, Nam);
                string json = JsonConvert.SerializeObject(err.Data);
                List<ReturnData<HRM_BaoCaoCongTacPhi>> listdata = JsonConvert.DeserializeObject<List<ReturnData<HRM_BaoCaoCongTacPhi>>>(json.ToString());
                if (listdata.Count > 0)
                {
                    int nRow = 7;
                    int STT = 1;
                    int nCol = 1;
                    foreach (var item in listdata)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.data.TenChiPhi;
                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        nCol++;
                        item.data.listThang = item.data.listThang.OrderBy(a => a.Thang).ToList();
                        foreach (var itemCP in item.data.listThang)
                        {
                            sheet1.Cells[nRow, nCol].Value = itemCP.ChiPhi;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            nCol++;
                        }
                        sheet1.Cells[nRow, nCol].Value = item.data.listThang.Sum(a => a.ChiPhi);
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        sheet1.Cells[nRow, 1, nRow, nCol].Style.Font.Bold = true;
                        nRow++;
                        if (item.children != null && item.children.Count > 0)
                        {
                            nRow = ExportBaoCaoChiPhiCongTac_Child(sheet1, item.children, nRow);
                        }
                        STT++;
                    }
                    DrawTableExcel(sheet1, 5, nRow, 1, 12);
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.Font.Size = 12;
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("BaoCaoChiPhiCongTac.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }
        public int ExportBaoCaoChiPhiCongTac_Child(ExcelWorksheet sheet1, List<ReturnData<HRM_BaoCaoCongTacPhi>> listdata, int nRow)
        {
            int nCol = 1;
            foreach (var item in listdata)
            {
                nCol = 1;

                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.data.TenChiPhi;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                nCol++;
                item.data.listThang = item.data.listThang.OrderBy(a => a.Thang).ToList();
                foreach (var itemCP in item.data.listThang)
                {
                    sheet1.Cells[nRow, nCol].Value = itemCP.ChiPhi;
                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    nCol++;
                }
                sheet1.Cells[nRow, nCol].Value = item.data.listThang.Sum(a => a.ChiPhi);
                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                if (item.children != null && item.children.Count > 0)
                {
                    sheet1.Cells[nRow, 1, nRow, nCol].Style.Font.Bold = true;
                    nRow++;
                    nRow = ExportBaoCaoChiPhiCongTac_Child(sheet1, item.children, nRow);

                }
                else
                    nRow++;
            }
            return nRow;
        }
        public dynamic ExportQuyTrinhKetQuaCongTac(HRM_KetQuaCongTac QuyTrinh, ApplicationDbContext db)
        {
            string sFile = "MauKetQuaCongTac.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "KetQuaCongTac.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            if (QuyTrinh.NoiDen == null)
                QuyTrinh.NoiDen = "";
            sheet1.Cells[2, 1].Value = "KẾT QUẢ CÔNG TÁC TẠI " + QuyTrinh.NoiDen.ToUpper();
            QuyTrinh.NgayDi = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayDiUnix);
            QuyTrinh.NgayVe = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayVeUnix);
            if (QuyTrinh.NgayDiUnix > 0 && QuyTrinh.NgayVeUnix > 0)
                sheet1.Cells[3, 1].Value = "Từ Ngày " + QuyTrinh.NgayDi.ToString("dd/MM/yyyy") + " Đến ngày " + QuyTrinh.NgayVe.ToString("dd/MM/yyyy");
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            int nCol = 14;
            int nRow = 8;
            if (QuyTrinh.listNhanVienCongTac != null && QuyTrinh.listNhanVienCongTac.Count > 0)
            {
                var listdanhmucChiPhi = db.HRM_dmCongTacPhi.ToList();
                var listIddmCongTacPhiRieng = QuyTrinh.listNhanVienCongTac.SelectMany(a => a.listChiPhi.Select(b => b.IddmCongTacPhi)).ToList();
                var dmChiPhiRieng = listdanhmucChiPhi.Where(a => listIddmCongTacPhiRieng.Contains(a.IdGuid)).ToList();
                var listIddmCongTacPhiChung = QuyTrinh.listChiPhi.Select(b => b.IddmCongTacPhi).ToList();
                var dmChiPhiChung = listdanhmucChiPhi.Where(a => listIddmCongTacPhiChung.Contains(a.IdGuid)).ToList();
                List<string> listPhuongTien = new List<string>();
                if (QuyTrinh.listIddmPhuongTien != null && QuyTrinh.listIddmPhuongTien.Count > 0)
                {
                    listPhuongTien = db.HRM_dmPhuongTien.Where(a => QuyTrinh.listIddmPhuongTien.Contains(a.IdGuid)).Select(a => a.Ten).Distinct().ToList();
                }
                #region header
                int CountCp = dmChiPhiChung.Count() + dmChiPhiRieng.Count();
                foreach (var item in dmChiPhiRieng)
                {
                    sheet1.Cells[7, nCol].Value = item.Ten;
                    nCol++;
                }
                foreach (var item in dmChiPhiChung)
                {
                    sheet1.Cells[7, nCol].Value = item.Ten;
                    nCol++;
                }
                if (nCol > 14)
                    sheet1.Cells[6, 14, 6, nCol - 1].Merge = true;
                else
                {
                    sheet1.Cells[6, 14, 7, 14].Merge = true;
                    nCol++;
                }
                sheet1.Cells[6, nCol].Value = "Phương tiện đi lại";
                sheet1.Cells[6, nCol, 7, nCol].Merge = true;
                sheet1.Cells[5, 1, 7, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                nCol++;
                sheet1.Cells[6, nCol].Value = "Note";
                sheet1.Cells[6, nCol, 7, nCol].Merge = true;
                sheet1.Cells[5, 1, 7, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet1.Cells[5, 12, 5, nCol].Merge = true;
                sheet1.Cells[2, 1, 2, nCol].Merge = true;
                sheet1.Cells[3, 1, 3, nCol].Merge = true;
                #endregion

                int STT = 1;
                int CountNSU = QuyTrinh.listNhanVienCongTac.Count();

                nCol = 1;
                sheet1.Cells[nRow, nCol].Value = STT;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = QuyTrinh.CreatedByName;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                nCol++;
                string Ngay = "";
                if (QuyTrinh.NgayDiUnix > 0)
                {
                    QuyTrinh.NgayDi = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayDiUnix);
                    Ngay = QuyTrinh.NgayDi.ToString("dd/MM/yyyy") + " - ";
                }
                if (QuyTrinh.NgayVeUnix > 0)
                {
                    QuyTrinh.NgayVe = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayVeUnix);
                    Ngay = Ngay + QuyTrinh.NgayVe.ToString("dd/MM/yyyy");
                }
                sheet1.Cells[nRow, nCol].Value = Ngay;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                nCol++;
                sheet1.Cells[nRow, nCol].Value = QuyTrinh.NoiDung;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = QuyTrinh.LuongTuVan;
                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = QuyTrinh.NoiDen;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = QuyTrinh.NguoiLienHe;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = QuyTrinh.SoDienThoai;
                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                nCol++;
                if (QuyTrinh.CachThucHien == 1)
                    sheet1.Cells[nRow, nCol].Value = 1;
                nCol++;
                if (QuyTrinh.CachThucHien == 2)
                    sheet1.Cells[nRow, nCol].Value = 1;
                nCol++;
                if (QuyTrinh.CachThucHien == 3)
                    sheet1.Cells[nRow, nCol].Value = 1;
                nCol++;
                int Col = nCol;
                foreach (var item in QuyTrinh.listNhanVienCongTac)
                {
                    Col = nCol;
                    sheet1.Cells[nRow, Col].Value = item.TenNhanVien;
                    sheet1.Cells[nRow, Col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    Col++;

                    dungchung.SetRichTextFromHtml(sheet1.Cells[nRow, Col], item.KetQua, "Times New Roman", 12);
                    sheet1.Cells[nRow, Col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    Col++;
                    int colCp = Col;
                    if (dmChiPhiRieng != null && dmChiPhiRieng.Count > 0)
                    {
                        foreach (var Cp in dmChiPhiRieng)
                        {
                            var ChiPhi = item.listChiPhi.FirstOrDefault(a => a.IddmCongTacPhi == Cp.IdGuid);
                            if (ChiPhi != null)
                            {
                                sheet1.Cells[nRow, colCp].Value = ChiPhi.SoLuong * ChiPhi.DonGia * ChiPhi.SoNgayDi * ChiPhi.HeSo;
                                sheet1.Cells[nRow, colCp].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                sheet1.Cells[nRow, colCp].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            }
                        }
                        colCp++;
                    }

                    foreach (var Cp in dmChiPhiChung)
                    {
                        var ChiPhi = QuyTrinh.listChiPhi.FirstOrDefault(a => a.IddmCongTacPhi == Cp.IdGuid);
                        if (ChiPhi != null)
                        {
                            sheet1.Cells[nRow, colCp].Value = ChiPhi.ThanhTien / CountNSU;
                            sheet1.Cells[nRow, colCp].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            sheet1.Cells[nRow, colCp].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        }
                        colCp++;
                    }
                    if (CountCp <= 1)
                        Col = 15;
                    else
                        Col = CountCp + 14;
                    sheet1.Cells[nRow, Col].Value = string.Join(", ", listPhuongTien);
                    Col++;
                    nRow++;
                    STT++;

                }
                nCol = Col;

                dungchung.DrawTableExcel(sheet1, 5, nRow, 1, nCol);
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            }
            int Row = nRow += 3;
            sheet1.Cells[Row, 8, Row, nCol].Merge = true;
            sheet1.Cells[Row, 8].Value = "Hà Nội, ngày " + dt.Day + " tháng " + dt.Month + " năm " + dt.Year;
            Row++;
            sheet1.Cells[Row, 1, Row, 2].Merge = true;
            sheet1.Cells[Row, 1].Value = "PHÊ DUYỆT";
            sheet1.Cells[Row, 4, Row, 6].Merge = true;
            sheet1.Cells[Row, 4].Value = "PHỤ TRÁCH NHÂN SỰ";
            sheet1.Cells[Row, 8, Row, 10].Merge = true;
            sheet1.Cells[Row, 8].Value = "BỘ PHẬN TUYỂN DỤNG";
            sheet1.Cells[Row, 12, Row, 14].Merge = true;
            sheet1.Cells[Row, 12].Value = "LẬP BIỂU";
            sheet1.Cells[nRow, 1, Row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells[nRow, 1, Row, 14].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
    }
}