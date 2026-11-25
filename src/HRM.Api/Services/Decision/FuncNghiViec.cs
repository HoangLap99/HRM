#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Wordprocessing;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.FunctionCompilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Words.NET;
using static HRM.Api.Helper.HelperConstant;

namespace HRM.Api.Services.NhanVien
{
    public class FuncNghiViec : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";
        public FuncNghiViec(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncNghiViec> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        public dynamic GetNextQuyetDinhNghiViec(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyetDinhNghiViec.Select(x => x.SoQuyTrinh), DefineData.MaPhieuQuyetDinhNghiViec));
        public dynamic GetDanhSachQuyetDinhNghiViec(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            int nCurrentPage = TimKiem.CurrentPage ?? 0;
            int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
            int PageSize = TimKiem.PageSize ?? 25;
            IEnumerable<HRM_QuyetDinhNghiViec> result;
            if (TimKiem.TabTrangThai != 3)

                result = from a in db.HRM_QuyetDinhNghiViec
                         join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                         where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                         && b.IdUser == db.currentUser.Id
                         orderby a.Created descending
                         select a;
            else
                result = from a in db.HRM_QuyetDinhNghiViec
                         orderby a.Created descending
                         select a;
            DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
            DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);

            //if (listCongTy != null)
            //{
            //    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
            //    result = result.Where(a => listId.Contains(a.IdCongTyEOS));
            //}
            if (TimKiem.TuNgay > 0)
                result = result.Where(a => a.Created >= dtTuNgay);
            if (TimKiem.DenNgay > 0)
                result = result.Where(a => a.Created < dtDenNgay.AddDays(1));

            if (TimKiem.IdCongTy > 0)
                result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy); // bỏ lọc ngày 12/2 do không build được FE để sữa danh sách dự án
            if (TimKiem.IdBoPhan > 0)
            {
                var  listIdNhanViens = new List<string>();
                listIdNhanViens.AddRange(db.HRM_NhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhan).Select(a => a.IdGuid).Distinct().ToList());
                if (listIdNhanViens == null)
                    listIdNhanViens = new List<string>();
                result = result.Where(a => listIdNhanViens.Contains(a.IdNhanVien));
            }
            if (!string.IsNullOrEmpty(TimKiem.KeyWord))
            {
                var listIdNhanViens = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.isApDung == true && a.So.ToLower().Contains(TimKiem.CCCD.ToLower())).Select(a => a.IdNhanVien).ToList();
                if (listIdNhanViens == null)
                    listIdNhanViens = new List<string>();
                listIdNhanViens.AddRange(db.HRM_NhanVien.Where(a => listIdNhanViens.Contains(a.IdGuid) || (a.TenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()))).Select(a => a.IdGuid).Distinct().ToList());
                if (listIdNhanViens == null)
                    listIdNhanViens = new List<string>();
                result = result.Where(a => listIdNhanViens.Contains(a.IdNhanVien) || a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
            }

            var itemList = result.Distinct().ToList();
            int nSum = itemList.Count;
            int nTotalPage = nSum / nCurrentPage;
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listCongTy = _CallWebService.GetDanhSachDuAn(db);
            List<string> listIdNhanVien = itemList.Select(a => a.IdNhanVien).Distinct().ToList();
            listIdNhanVien.AddRange(itemList.Select(a => a.IdNguoiKy).Distinct().ToList());
            var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).Distinct().ToList();
            var listGT = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.isApDung == true && listIdNhanVien.Contains(a.IdNhanVien)).Distinct().ToList();
            foreach (var item in itemList)
            {
                BoPhan CongTy = listCongTy.Find(x => x.Id == item.IdDuAn);
                if (CongTy != null)
                    item.TenCongTy = CongTy.TenBoPhan;
                BoPhan BoPhan = listBoPhan.Find(x => x.Id == item.IdBoPhanEOS);
                if (BoPhan != null)
                    item.TenBoPhan = BoPhan.TenBoPhan;
                var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                if (nhanVien != null)
                    item.TenNhanVien = nhanVien.TenDayDu;
                var NguoiKy = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNguoiKy);
                if (NguoiKy != null)
                    item.TenNguoiKy = NguoiKy.TenDayDu;
                item.CCCD = listGT.Where(a => a.IdNhanVien == item.IdNhanVien).Select(a => a.So).FirstOrDefault();

            }
            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = itemList;
            return dnm;

        }
        public dynamic SetQuyetDinhNghiViec(HRM_QuyetDinhNghiViec item, ApplicationDbContext db = null)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var quyTrinhDb = db.HRM_QuyetDinhNghiViec.AsNoTracking().FirstOrDefault(a => a.IdGuid == item.IdGuid);
                if (!SetTruocHamUpdate(db, _CallWebService, quyTrinhDb, item, err))
                    return err;

                if (!FuncUpDateQuyetDinhNghiViec(item, db, err, eQuyTrinh_LoaiHam.Set))
                    return MessageReturn.Fail(err.sError);

                if (!SetSauHamUpdate(db, quyTrinhDb, item, err))
                    return err;

                return MessageReturn.Success("Cập nhật dữ liệu thành công", item);
            });

        public dynamic GetQuyetDinhNghiViec(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            var item = db.HRM_QuyetDinhNghiViec.FirstOrDefault(x => x.IdGuid == Id);

            if (item == null)
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy bổ nhiệm chứ vụ");
            item.NgayDuyetNghiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDuyetNghi);
            item.NgayNghiViecUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayNghiViec);
            item.NgayBanGiaoUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBanGiao);
            item.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKy);
            item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);

            var lstTaiSan = db.HRM_QuyetDinhNghiViecTaiSanBanGiao.Where(r => r.IdQuyetDinhNghiViec == Id).ToList();
            item.lstFileDinhKem = FileUploadController.GetListFileDinhKem(db, item.IdGuid, nameof(HRM_QuyetDinhNghiViec));
            var listTaiSan = db.HRM_dmTrangThietBi.ToList();
            foreach (var itemTS in lstTaiSan)
            {
                itemTS.NgayBanGiaoUnix = dungchung.DateTimeToUnixTimeStamp(itemTS.NgayBanGiao);
                itemTS.TenTaiSanBanGiao = listTaiSan.FirstOrDefault(a => a.IdGuid == itemTS.IddmTaiSan) == null ? "" : listTaiSan.FirstOrDefault(a => a.IdGuid == itemTS.IddmTaiSan).Ten;
            }
            item.lstTaiSanBanGiao = lstTaiSan;

            List<HRM_QuyetDinhNghiViecNhanVienBoPhan> quyetDinhKhenThuongBoPhans = db.HRM_QuyetDinhNghiViecNhanVienBoPhan.Where(x => x.IdQuyetDinhKhenThuong == Id).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var ListChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var quyetDinh in quyetDinhKhenThuongBoPhans)
            {
                BoPhan nv = listBoPhan.FirstOrDefault(x => x.Id == quyetDinh.IdBoPhanEOS);
                if (nv != null)
                    quyetDinh.TenBoPhan = nv.TenBoPhan;
            }
            item.lstBoPhan = quyetDinhKhenThuongBoPhans;

            if (item.isKetThuc == true)
            {
                var NhanVien = db.HRM_NhanVien.FirstOrDefault(r => r.IdGuid == item.IdNhanVien);
                if (NhanVien != null)
                {
                    item.CCCD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.isApDung == true && a.IdNhanVien == NhanVien.IdGuid).Select(a => a.So).FirstOrDefault();
                    item.TenNhanVien = NhanVien.TenDayDu;
                    var ListBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    var BoPhan = ListBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                    if (BoPhan != null)
                        item.TenBoPhan = BoPhan.TenBoPhan;

                    var ChucDanh = ListChucDanh.FirstOrDefault(a => a.Id == NhanVien.IdChucDanh);
                    if (ChucDanh != null)
                        item.TenChucDanh = ChucDanh.Ten;
                }
            }
            if (!GetQuyTrinh(db, _CallWebService, item, err, true))
                return err;
            return item;
        }


        public dynamic DeleteQuyetDinhNghiViec(string Id, ApplicationDbContext db = null)
            => ExecuteInTransaction(db, () =>
            {
                db.HRM_QuyetDinhNghiViec.RemoveRange(db.HRM_QuyetDinhNghiViec.Where(x => x.IdGuid == Id));
                db.HRM_QuyetDinhNghiViecTaiSanBanGiao.RemoveRange(db.HRM_QuyetDinhNghiViecTaiSanBanGiao.Where(r => r.IdQuyetDinhNghiViec == Id));
                db.SaveChanges();
                return MessageReturn.Success("Xóa dữ liệu thành công");
            });

        public dynamic ChuyenTiepQuyetDinhNghiViec(HRM_QuyetDinhNghiViec QuyTrinh, ApplicationDbContext db)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var pcuDatabase = db.HRM_QuyetDinhNghiViec.AsNoTracking().FirstOrDefault(x => x.IdGuid == QuyTrinh.IdGuid);
                if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdDuAn ?? 0))
                    return err;

                if (!FuncUpDateQuyetDinhNghiViec(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep))
                    return MessageReturn.Fail(err.sError, value: QuyTrinh.IdGuid);

                if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    return err;

                if (QuyTrinh.isKetThuc)
                {
                    var nhanVien = db.HRM_NhanVien.Find(QuyTrinh.IdNhanVien);
                    if (nhanVien != null)
                    {
                        if (QuyTrinh.NgayHieuLuc < DateTime.Now)
                            nhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DaNghiViec.ToString();
                        nhanVien.NgayNghiViec = QuyTrinh.NgayHieuLuc;
                        nhanVien.NgayQuayLai = null;
                        db.Entry(nhanVien).State = EntityState.Modified;
                    }

                    LogLichSuNhanVien(db, QuyTrinh.IdNhanVien, QuyTrinh.IdGuid, "HRM_QuyetDinhNghiViec", DefineData.eAction.QUYTRINHQUYETDINHTHOIVIEC.ToString(), QuyTrinh.NgayNghiViecUnix, "Quyết định nghỉ việc");
                }

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });

        public dynamic KhongDuyetQuyetDinhNghiViec(HRM_QuyetDinhNghiViec QuyTrinh, ApplicationDbContext db)
            => ExecuteInTransaction(db, () =>
            {
                var err = MessageReturn.Success("Cập nhật dữ liệu thành công");
                var pcuDatabase = db.HRM_QuyetDinhNghiViec.AsNoTracking().FirstOrDefault(x => x.IdGuid == QuyTrinh.IdGuid);
                if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                    return err;

                if (!FuncUpDateQuyetDinhNghiViec(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet))
                    return MessageReturn.Fail(err.sError, value: QuyTrinh.IdGuid);

                if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    return err;

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật thành công");
            });
        public bool FuncUpDateQuyetDinhNghiViec(HRM_QuyetDinhNghiViec model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (model.NgayNghiViecUnix > 0)
                    model.NgayNghiViec = dungchung.UnixTimeStampToDateTime(model.NgayNghiViecUnix);
                if (model.NgayDuyetNghiUnix > 0)
                    model.NgayDuyetNghi = dungchung.UnixTimeStampToDateTime(model.NgayDuyetNghiUnix);
                if (model.NgayBanGiaoUnix > 0)
                    model.NgayBanGiao = dungchung.UnixTimeStampToDateTime(model.NgayBanGiaoUnix);
                if (model.NgayKyUnix > 0)
                    model.NgayKy = dungchung.UnixTimeStampToDateTime(model.NgayKyUnix);
                if (model.NgayHieuLucUnix > 0)
                    model.NgayHieuLuc = dungchung.UnixTimeStampToDateTime(model.NgayHieuLucUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    //model.IdDuAn = model.IdCongTyEOS;
                    if (string.IsNullOrEmpty(model.eAction))
                        model.eAction = DefineData.eAction.QUYTRINHQUYETDINHTHOIVIEC.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return false;
                    db.HRM_QuyetDinhNghiViec.Add(model);

                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                }
                var lstTaiSanOld = db.HRM_QuyetDinhNghiViecTaiSanBanGiao.Where(r => r.IdQuyetDinhNghiViec == model.IdGuid).ToList();
                if (lstTaiSanOld != null && lstTaiSanOld.Count > 0)
                    db.HRM_QuyetDinhNghiViecTaiSanBanGiao.RemoveRange(lstTaiSanOld);
                if (model.lstTaiSanBanGiao != null)
                {
                    foreach (var item in model.lstTaiSanBanGiao)
                    {
                        if (item.NgayBanGiaoUnix > 0)
                            item.NgayBanGiao = UnixTimeStampToDateTime(item.NgayBanGiaoUnix);
                        item.IdQuyetDinhNghiViec = model.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();

                    }
                    db.HRM_QuyetDinhNghiViecTaiSanBanGiao.AddRange(model.lstTaiSanBanGiao);
                }

                if (model.lstFileDinhKem != null)
                {
                    FileUploadController.SetFileDinhKem(db, model.lstFileDinhKem, nameof(HRM_QuyetDinhNghiViec).ToString(), model.IdGuid);
                }
                var listBoPhan = db.HRM_QuyetDinhNghiViecNhanVienBoPhan.Where(r => r.IdQuyetDinhKhenThuong == model.IdGuid).ToList();
                db.HRM_QuyetDinhNghiViecNhanVienBoPhan.RemoveRange(listBoPhan);
                if (model.lstBoPhan != null)
                {
                    foreach (var BoPhan in model.lstBoPhan)
                    {
                        BoPhan.IdGuid = Guid.NewGuid().ToString();
                        BoPhan.IdQuyetDinhKhenThuong = model.IdGuid;
                        db.Entry(BoPhan).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }
                }
                db.SaveChanges();
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }
        public dynamic ExportQuyetDinhNghiViec(string Id, ApplicationDbContext db)
        {
            var filenamegui = DateTime.Now.ToOADate();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_NghiViec.docx");
            if (Host == "PHT")
            {
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/QD_NghiViec.docx");
            }
            string sFileNameCopy = PathUploads + "QD_NghiViec" + filenamegui + ".docx";
            HRM_QuyetDinhNghiViec quytrinh = db.HRM_QuyetDinhNghiViec.Find(Id);
            dynamic data = new DynamicObject();
            var lítCongty = _CallWebService.GetCongTyKyHopDong(db);
            var CongTy = lítCongty.Find(x => x.Id == quytrinh.IdCongTyEOS);
            var NguoiLaoDong = db.HRM_NhanVien.Find(quytrinh.IdNhanVien);
            var NguoiKy = db.HRM_NhanVien.Find(quytrinh.IdNguoiKy);
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            string NgayThang = "ngày " + quytrinh.NgayKy?.Day + " tháng " + quytrinh.NgayKy?.Month + " năm " + quytrinh.NgayKy?.Year;
            DateTime time = quytrinh.NgayHieuLuc.Value.AddDays(-1);
            string NgayDuyetNghi = "ngày " + time.Day + " tháng " + time.Month + " năm " + time.Year;
            string NgayQuyetDinh = "ngày " + quytrinh.NgayHieuLuc?.Day + " tháng " + quytrinh.NgayHieuLuc?.Month + " năm " + quytrinh.NgayHieuLuc?.Year;
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            //doc.ReplaceText("@SoQuyetDinh", quytrinh.);
            doc.ReplaceText("@NgayDuyetNghi", NgayDuyetNghi);
            doc.ReplaceText("@LyDo", quytrinh.LyDoTheoDon ?? "");
            doc.ReplaceText("@NgayThang", NgayThang);
            doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@NguoiKy", NguoiKy == null ? "" : NguoiKy.TenDayDu);
            var BoPhan = _CallWebService.GetBoPhan(db, NguoiLaoDong.IdBoPhanEOS);
            var ChucVu = _CallWebService.GetChucVu(db, NguoiLaoDong.IdChucVu);
            string TenChucVu = "";
            if (ChucVu != null)
                TenChucVu = ChucVu.TenChucVu;
            if (BoPhan != null)
                TenChucVu = TenChucVu + " - " + BoPhan.TenBoPhan;

            doc.ReplaceText("@TenChucVu", TenChucVu);
            doc.ReplaceText("@NgayQuyetDinh", NgayQuyetDinh);
            doc.Save();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_NghiViec.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportQuyetDinhNghiViecNam(int Nam, int Thang, ApplicationDbContext db)
        {
            var filenamegui = DateTime.Now.ToOADate();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachQuyetDinhNghiViecNam.xlsx");
            if (Host == "PHT")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/DanhSachQuyetDinhNghiViecNam.xlsx");
            string sFileNameCopy = PathUploads + "DanhSachQuyetDinhNghiViecNam" + filenamegui + ".xlsx";
            dynamic data = new DynamicObject();
            File.Copy(fileName, sFileNameCopy, true);
            Stream s = File.OpenRead(fileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            var listQuyetDinh = db.HRM_QuyetDinhNghiViec.Where(a => a.isKetThuc == true && a.NgayKy.Value.Year == Nam && a.NgayKy.Value.Month == Thang).ToList();
            if (listQuyetDinh != null && listQuyetDinh.Count > 0)
            {
                int STT = 1;
                int nRow = 6;
                List<string> listIdNV = listQuyetDinh.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).Distinct().ToList();
                var listCongTy = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNV.Contains(a.IdQuyTrinh)).ToList();
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                sheet1.Cells[3, 1].Value = "BẢNG TỔNG HỢP THEO DÕI SỐ QUYẾT ĐỊNH CHẤM DỨT HỢP ĐỒNG NĂM " + Nam;
                foreach (var item in listQuyetDinh)
                {
                    int nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    sheet1.Cells[nRow, nCol].Value = NhanVien.TenDayDu;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = NhanVien.NgaySinh.ToString("dd/MM/yyyy");
                    nCol++;
                    var listIdDonVi = listCongTy.Where(a => a.IdQuyTrinh == NhanVien.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
                    var listDonVi = listDuAn.Where(a => listIdDonVi.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                    sheet1.Cells[nRow, nCol].Value = string.Join(", ", listDonVi);
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.SoQuyTrinh;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.NgayKy.Value.ToString("dd/MM/yyyy");
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.LyDoTheoDon;
                    nRow++;
                    STT++;

                }
                nRow--;

                dungchung.DrawTableExcel(sheet1, 5, nRow, 1, 7);
                int Row = nRow + 2;
                sheet1.Cells[Row, 5, Row, 7].Merge = true;
                DateTime time = DateTime.Now;
                sheet1.Cells[Row, 5].Value = "Hà Nội, ngày " + time.Day + " tháng " + time.Month + " năm " + time.Year;
                Row++;
                sheet1.Cells[Row, 5, Row, 7].Merge = true;
                sheet1.Cells[Row, 5].Value = "NGƯỜI LẬP BIỂU";
                Row++;
                Row++;
                Row++;
                sheet1.Cells[Row, 5, Row, 7].Merge = true;
                sheet1.Cells[Row, 5].Value = db.currentUser.TenNhanVien.ToUpper();

                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[6, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[6, 4, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[6, 7, nRow, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            }
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("DanhSachQuyetDinhNghiViecNam.xlsx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportBaoCaoTongHopNghi(int Nam, int Thang, ApplicationDbContext db)
        {
            var filenamegui = DateTime.Now.ToOADate();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoTongHopNghi.xlsx");
            if (Host == "PHT")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/BaoCaoTongHopNghi.xlsx");
            string sFileNameCopy = PathUploads + "BaoCaoTongHopNghi" + filenamegui + ".xlsx";
            File.Copy(fileName, sFileNameCopy, true);
            dynamic data = new DynamicObject();
            Stream s = File.OpenRead(fileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            sheet1.Cells[3, 1].Value = "TỔNG HỢP NHÂN SỰ BHXH - BHBM THÁNG " + Thang + "/" + Nam;
            sheet1.Cells[4, 10].Value = "BHXH T" + Thang + "/" + Nam;
            sheet1.Cells[4, 9].Value = "Giảm BH; nghỉ việc/ nghỉ chế độ " + Thang + "/" + Nam;
            List<string> listIdNhanVien = new List<string>();
            var listNhanVienNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => a.NgayNghi.Year == Nam && a.NgayNghi.Month == Thang).ToList();
            if (listNhanVienNghiPhep != null && listNhanVienNghiPhep.Count > 0)
                listIdNhanVien.AddRange(listNhanVienNghiPhep.Select(a => a.IdNhanVien).Distinct().ToList());
            else
                listNhanVienNghiPhep = new List<HRM_TheoDoiXinNghiPhep>();
            var listQuyetDinh = db.HRM_QuyetDinhNghiViec.Where(a => a.isKetThuc == true && a.NgayNghiViec != null && a.NgayNghiViec.Value.Year == Nam && a.NgayNghiViec.Value.Month == Thang).ToList();
            if (listQuyetDinh != null && listQuyetDinh.Count > 0)
                listIdNhanVien.AddRange(listQuyetDinh.Select(a => a.IdNhanVien).Distinct().ToList());
            else
                listQuyetDinh = new List<HRM_QuyetDinhNghiViec>();
            var listHopDong = new List<HRM_HopDong>();
            var LoaiHopDongThanhLy = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.Ma == "BBTL");
            if (LoaiHopDongThanhLy != null)
                listHopDong = db.HRM_HopDong.Where(a => a.IdLoaiHopDong == LoaiHopDongThanhLy.IdGuid && a.isKetThuc == true && a.NgayHieuLuc.Year == Nam && a.NgayHieuLuc.Month == Thang).ToList();
            if (listHopDong != null && listHopDong.Count > 0)
                listIdNhanVien.AddRange(listHopDong.Select(a => a.IdNhanVien).Distinct().ToList());
            else
                listHopDong = new List<HRM_HopDong>();
            if (listIdNhanVien != null && listIdNhanVien.Count > 0)
            {
                var listDonVi_Nv = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
                var listDuAns = _CallWebService.GetDanhSachDuAn(db);
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).OrderBy(a => a.TenDayDu).ToList();
                var loaiPhep = _CallWebService.GetListDanhMucHinhThucNghi(db);
                var listIdPhepNam = loaiPhep.Where(a => a.Ma == "PN" || a.Ma == "NP").Select(a => a.Id).ToList();
                var IdThaiSan = loaiPhep.Where(a => a.Ma == DefineData.eKyHieuChamCong.NTS.ToString()).Select(a => a.Id).FirstOrDefault();
                int nRow = 7;
                int STT = 1;
                int nCol = 1;
                foreach (var item in listNhanVien)
                {
                    var NgayNghis = listNhanVienNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && a.IdHinhThucNghi != IdThaiSan).ToList();
                    var NghiViec = listQuyetDinh.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    var NghiPhep = NgayNghis.Where(a => a.IdNhanVien == item.IdGuid && listIdPhepNam.Contains(a.IdHinhThucNghi)).ToList();
                    var NghiKL = NgayNghis.Where(a => a.IdNhanVien == item.IdGuid && !listIdPhepNam.Contains(a.IdHinhThucNghi) && a.IdHinhThucNghi != IdThaiSan).ToList();
                    var NghiPhepTS = listNhanVienNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && a.IdHinhThucNghi == IdThaiSan).FirstOrDefault();
                    var HopDongThanhLy = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                    //nCol++;
                    //sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                    nCol++;
                    var listIdDV = listDonVi_Nv.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
                    var listTenDV = listDuAns.Where(a => listIdDV.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                    sheet1.Cells[nRow, nCol].Value = string.Join(", ", listTenDV);
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    nCol++;
                    string LyDo = "";
                    if (NghiViec != null)
                    {
                        sheet1.Cells[nRow, nCol].Value = NghiViec.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                        LyDo = NghiViec.LyDoTheoDon;
                    }
                    else if (HopDongThanhLy != null)
                    {
                        sheet1.Cells[nRow, nCol].Value = HopDongThanhLy.NgayHieuLuc.ToString("dd/MM/yyyy");
                        LyDo = "Thanh lý hợp đồng từ Ngày " + HopDongThanhLy.NgayHieuLuc.ToString("dd/MM/yyyy");
                    }
                    else if (NghiPhepTS != null)
                    {
                        sheet1.Cells[nRow, nCol].Value = NghiPhepTS.NgayBatDau.Value.ToString("dd/MM/yyyy");
                        LyDo = NghiPhepTS.GhiChu;
                    }
                    else if (NgayNghis != null)
                    {
                        sheet1.Cells[nRow, nCol].Value = string.Join(", ", NgayNghis.Select(a => a.NgayNghi.ToString("dd/MM/yyyy")));
                        var text = NgayNghis.Where(a => !string.IsNullOrEmpty(a.GhiChu)).Select(a => a.GhiChu).Distinct().ToList();
                        if (text != null && text.Count > 0)
                            LyDo = string.Join(", ", text);

                    }
                    nCol++;
                    if (item.GiamBaoHiem == 1)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    if (item.GiamBaoHiem == 2)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    nCol++;
                    if (NghiViec != null && item.isThamGiaBHXH == true)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    if (NghiViec != null && item.isThamGiaBHXH == false)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    if (NghiPhep != null && item.isThamGiaBHXH == true)
                        sheet1.Cells[nRow, nCol].Value = NghiPhep.Count;
                    nCol++;
                    if (NghiPhep != null && item.isThamGiaBHXH == false)
                        sheet1.Cells[nRow, nCol].Value = NghiPhep.Count;
                    nCol++;
                    if (NghiPhepTS != null && item.isThamGiaBHXH == true)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    if (NghiPhepTS != null && item.isThamGiaBHXH == false)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    if (NghiKL != null && item.isThamGiaBHXH == true)
                        sheet1.Cells[nRow, nCol].Value = NghiKL.Count;
                    nCol++;
                    if (NghiKL != null && item.isThamGiaBHXH == false)
                        sheet1.Cells[nRow, nCol].Value = NghiKL.Count;
                    nCol++;
                    if (HopDongThanhLy != null && item.isThamGiaBHXH == true)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++;
                    if (HopDongThanhLy != null && item.isThamGiaBHXH == false)
                        sheet1.Cells[nRow, nCol].Value = 1;
                    nCol++; nCol++;
                    sheet1.Cells[nRow, nCol].Value = LyDo;
                    nRow++;
                    STT++;
                }
                nRow--;
                int Row = nRow + 3;
                sheet1.Cells[Row, 1, Row, 3].Merge = true;
                sheet1.Cells[Row, 1].Value = "PHÊ DUYỆT";
                sheet1.Cells[Row, 5, Row, 8].Merge = true;
                sheet1.Cells[Row, 5].Value = "PHÒNG ĐIỀU HÀNH";
                sheet1.Cells[Row, nCol - 7, Row, nCol].Merge = true;
                sheet1.Cells[Row, nCol - 7].Value = "PHÒNG NHÂN SỰ";
                sheet1.Cells[Row, 1, Row, nCol].Style.Font.Bold = true;
                sheet1.Cells[Row, 1, Row, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[Row, 1, Row, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                dungchung.DrawTableExcel(sheet1, 7, nRow, 1, nCol);
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells.Style.WrapText = true;
            }
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("BaoCaoTongHopNghi.xlsx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public void UpdateNgayHieuLucNghiViec(ApplicationDbContext db)
        {
            var listQuyTrinh = db.HRM_QuyetDinhNghiViec.Where(a => a.isKetThuc == true).ToList();
            var listidNhanVien = listQuyTrinh.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listidNhanVien.Contains(a.IdGuid)).ToList();
            foreach (var item in listNhanVien)
            {
                var QuyTrinh = listQuyTrinh.Where(a => a.IdNhanVien == item.IdGuid).OrderByDescending(a => a.NgayHieuLuc).FirstOrDefault();
                if (QuyTrinh.NgayHieuLuc < DateTime.Now)
                    item.TrangThai = DefineData.eTrangThaiNhanVien.DaNghiViec.ToString();
                item.NgayNghiViec = QuyTrinh.NgayHieuLuc;
                item.NgayQuayLai = null;
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

    }
}