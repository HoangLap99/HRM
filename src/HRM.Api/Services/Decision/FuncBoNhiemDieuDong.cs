#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HRM.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Words.NET;

namespace HRM.Api.Services.NhanVien
{

    public class FuncBoNhiemDieuDong : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";
        public FuncBoNhiemDieuDong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBoNhiemDieuDong> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        #region Bổ nhiệm điều động dự án


        public dynamic GetNextMaBoNhiemDieuDongDuAn(ApplicationDbContext db)
        {
            dynamic dnm = new DynamicObject();
            dnm.SoQuyTrinh = GenerateNextCode(db.HRM_BoNhiemDieuDongDuAn.Select(x => x.SoQuyTrinh), DefineData.MaQuyetDinhBoNhiemDieuDong);
            return dnm;
        }

        public dynamic GetDanhSachBoNhiemDieuDongDuAn(dynamic dyn, ApplicationDbContext db)
        {

            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_BoNhiemDieuDongDuAn> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_BoNhiemDieuDongDuAn
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_BoNhiemDieuDongDuAn
                             orderby a.Created descending
                             select a;
                var list = result.ToList();
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                if (listCongTy != null)
                {
                    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdCongTyEOS ?? 0) || listId.Contains(a.IdCongTyEOSNew ?? 0));
                }
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Created >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Created < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                if (TimKiem.IdBoPhan > 0)
                    result = result.Where(a => a.IdBoPhanEOSOld == TimKiem.IdBoPhan);
                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);

                List<string> lstNguoiKy = itemList.Select(r => r.IdNguoiKy).ToList();
                lstNguoiKy.AddRange(itemList.Select(r => r.IdNhanVien).ToList());
                var Nhanviens = db.HRM_NhanVien.Where(r => lstNguoiKy.Contains(r.IdGuid)).ToList();
                //List<HRM.Api.Models.SmartEOEModel.BoPhan> listBoPhan = smdb.BoPhans.ToList();
                foreach (var item in itemList)
                {
                    item.TenNguoiKy = Nhanviens.Where(r => r.IdGuid == item.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
                    item.TenCongTy = listDuAn.Where(r => r.Id == item.IdCongTyEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                    item.TenNhanVien = Nhanviens.Where(r => r.IdGuid == item.IdNhanVien).Select(r => r.TenDayDu).FirstOrDefault();
                    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                    item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                    item.NgayBatDauUnix = DateTimeToUnixTimeStamp(item.NgayBatDau);
                    item.NgayKetThucUnix = DateTimeToUnixTimeStamp(item.NgayKetThuc);
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


        public dynamic SetQuyTrinhBoNhiemDieuDongDuAn(HRM_BoNhiemDieuDongDuAn model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_BoNhiemDieuDongDuAn QuyTrinhdb = db.HRM_BoNhiemDieuDongDuAn.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncBoNhiemDieuDongDuAn(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!SetSauHamUpdate(db, QuyTrinhdb, model, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model);

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                }
            }

        }
        public dynamic SetQuyTrinhBoNhiemDieuDongDuAnCheck(HRM_BoNhiemDieuDongDuAn model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (model.isKetThuc == true)
                    {

                        //var phuLuc = FuncSetUpdateBoNhiemDieuDongDuAnPhuLucHopDong(model, db);

                        var nhanvien = db.HRM_NhanVien.Find(model.IdNhanVien);
                        if (nhanvien != null)
                        {
                            nhanvien.listIdCongTy = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == nhanvien.IdGuid).Select(a => a.IdBoPhan).ToList();


                            nhanvien.IdCongTyEOS = model.IdCongTyEOSNew ?? 0;

                            //nhanvien.listIdCongTy = new List<long>();
                            nhanvien.listIdCongTy.Add(model.IdCongTyEOSNew ?? 0);
                            nhanvien.listIdCongTy = nhanvien.listIdCongTy.Where(a => a != model.IdCongTyEOS).ToList();
                            nhanvien.IdBoPhanEOS = model.IdBoPhanEOSNew ?? 0;
                            nhanvien.IdChucDanh = model.IdChucDanh ?? 0;
                            nhanvien.IdChucVu = model.IdChucVu ?? 0;
                            nhanvien.IdDuAnCurrent = model.IdCongTyEOS ?? 0;
                            nhanvien.IdDuAnDieuDong = model.IdCongTyEOSNew ?? 0;
                            clsNhanSu_DuAn modelDuAn = new clsNhanSu_DuAn()
                            {
                                IdDuAn = model.IdCongTyEOSNew ?? 0,
                                NgayBatDauUnix = model.NgayBatDauUnix,
                                listIdUser = new List<string> { nhanvien.IdUser }
                            };
                            _CallWebService.SetNhanSuCreateUser(db, nhanvien);

                        }
                    }
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model);

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                }
            }

        }

        public dynamic GetQuyTrinhBoNhiemDieuDongDuAn(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_BoNhiemDieuDongDuAn model = db.HRM_BoNhiemDieuDongDuAn.Find(Id);
            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            model.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKy);
            model.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHieuLuc);
            model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
            model.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThuc);
            model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString(), model.IdGuid);
            var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == Id && r.TableName == DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString()).ToList();
            foreach (var item in lstPhuCap)
            {
                item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                item.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDau);
            }
            model.TenNguoiKy = db.HRM_NhanVien.Where(r => r.IdGuid == model.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();

            var NhanVien = db.HRM_NhanVien.FirstOrDefault(r => r.IdGuid == model.IdNhanVien);
            if (NhanVien != null)
            {
                model.TenNhanVien = NhanVien.TenDayDu;
                var ListBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var BoPhan = ListBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                if (BoPhan != null)
                    model.TenBoPhan = BoPhan.TenBoPhan;
                var ListChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var ChucDanh = ListChucDanh.FirstOrDefault(a => a.Id == NhanVien.IdChucDanh);
                if (ChucDanh != null)
                    model.TenChucDanh = ChucDanh.Ten;
            }

            model.lstPhuCap = lstPhuCap;
            return model;
        }
        public bool FuncBoNhiemDieuDongDuAn(HRM_BoNhiemDieuDongDuAn item, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (item.NgayKyUnix > 0)
                    item.NgayKy = dungchung.UnixTimeStampToDateTime(item.NgayKyUnix);
                if (item.NgayBatDauUnix > 0)
                    item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                if (item.NgayHieuLucUnix > 0)
                    item.NgayHieuLuc = dungchung.UnixTimeStampToDateTime(item.NgayHieuLucUnix);
                if (item.NgayKetThucUnix > 0)
                    item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);
                if (string.IsNullOrEmpty(item.IdGuid))
                {
                    item.IdDuAn = item.IdCongTyEOS;
                    if (string.IsNullOrEmpty(item.eAction))
                        item.eAction = DefineData.eAction.QUYTRINHBONHIEMDIEUDONG.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, item, err))
                        return false;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                var HRM_HopDongPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == item.IdGuid).ToList();
                db.HRM_HopDongPhuCap.RemoveRange(HRM_HopDongPhuCap);

                if (item.lstPhuCap != null)
                {
                    List<HRM_HopDongPhuCap> lstPhuCap = item.lstPhuCap;
                    foreach (var itemPhuCap in lstPhuCap)
                    {
                        itemPhuCap.NgayBatDau = dungchung.UnixTimeStampToDateTime(itemPhuCap.NgayBatDauUnix);
                        itemPhuCap.NgayKetThuc = dungchung.UnixTimeStampToDateTime(itemPhuCap.NgayKetThucUnix);

                        itemPhuCap.IdQuyetDinh = item.IdGuid;
                        itemPhuCap.TableName = DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString();

                        itemPhuCap.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(itemPhuCap).State = Microsoft.EntityFrameworkCore.EntityState.Added;


                    }

                }

                if (item.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in item.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_BoNhiemDieuDongDuAn, item.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, item, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();
                return true;
            }

            catch (Exception ex)
            {
                LogErrorFormat("UpdateDieuChinhLuong data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return false;
            }
        }

        public dynamic DeleteBoNhiemDieuDongDuAn(string Id, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {


                try
                {
                    HRM_BoNhiemDieuDongDuAn model = db.HRM_BoNhiemDieuDongDuAn.FirstOrDefault(r => r.IdGuid == Id);

                    var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == Id && r.TableName == DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString()).ToList();

                    if (lstPhuCap.Count() > 0)
                        db.HRM_HopDongPhuCap.RemoveRange(lstPhuCap);
                    var listFileDinhKem = db.HRM_FileDinhKem.Where(x => x.TableName == DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString() && x.idItem == Id).ToList();// GetListFileDinhKem(db, DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString(), model.IdGuid);
                    if (listFileDinhKem.Count() > 0)
                        db.HRM_FileDinhKem.RemoveRange(listFileDinhKem);
                    db.HRM_BoNhiemDieuDongDuAn.Remove(model);
                    db.SaveChanges();
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Xóa dữ liệu không thành công " + ex.Message);
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhBoNhiemDieuDongDuAn(HRM_BoNhiemDieuDongDuAn model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_BoNhiemDieuDongDuAn pcuDatabase = db.HRM_BoNhiemDieuDongDuAn.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, model, err, model.IdCongTyEOS ?? 0))
                        return err;
                    bool Error = FuncBoNhiemDieuDongDuAn(model, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, model, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    db.SaveChanges();
                    // string IdTrangThaiCuoi = GetTrangThaiCuoi(smartDB, DefineData.eAction.QUYTRINHBONHIEMCHUCVUDUAN.ToString());
                    if (model.isKetThuc == true)
                    {

                        //var phuLuc = FuncSetUpdateBoNhiemDieuDongDuAnPhuLucHopDong(model, db);
                        LogLichSuNhanVien(db, model.IdNhanVien, model.IdGuid, "HRM_BoNhiemDieuDongDuAn", DefineData.eAction.QUYTRINHBONHIEMDIEUDONG.ToString(), model.NgayKyUnix);
                        var nhanvien = db.HRM_NhanVien.Find(model.IdNhanVien);
                        if (nhanvien != null)
                        {
                            nhanvien.listIdCongTy = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == nhanvien.IdGuid).Select(a => a.IdBoPhan).ToList();

                            var dateTime = dungchung.UnixTimeStampToDateTime(0);

                            HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.isKetThuc == true).OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
                            HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien();

                            luong_NhanVien.IdGuid = Guid.NewGuid().ToString();
                            luong_NhanVien.IdCongTyEOS = model.IdCongTyEOSNew ?? 0;
                            luong_NhanVien.IdBoPhanEOS = model.IdBoPhanEOSNew ?? 0;
                            luong_NhanVien.IdNhanVien = model.IdNhanVien;
                            luong_NhanVien.Luong = model.LuongThucTeSauDieuChinh;
                            luong_NhanVien.KieuLuong = model.KieuLuong;
                            if (hopDong != null)
                            {
                                luong_NhanVien.LoaiLuong = hopDong.LoaiLuong;
                                var luong = db.HRM_Luong_HopDong.FirstOrDefault(a => a.IdHopDong == hopDong.IdGuid);
                                if (luong != null)
                                    luong_NhanVien.LuongDanhGia = luong.LuongDanhGia;
                            }
                            luong_NhanVien.LuongHachToan = model.LuongHachToanSauDieuChinh;
                            luong_NhanVien.NgayBatDauHieuLuc = model.NgayHieuLuc.Value;
                            luong_NhanVien.IdNguonNhapLuong = model.IdGuid;
                            luong_NhanVien.NguonNhapLuong = DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString();

                            db.HRM_Luong_NhanVien.Add(luong_NhanVien);

                            List<HRM_HopDongPhuCap> lstPhuCap = db.HRM_HopDongPhuCap.AsNoTracking().Where(r => r.IdQuyetDinh == model.IdGuid && r.TableName == DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString()).ToList();
                            List<HRM_Luong_PhuCap_NhanVien> listPcluong = new List<HRM_Luong_PhuCap_NhanVien>();
                            foreach (var itempc in lstPhuCap)
                            {
                                HRM_Luong_PhuCap_NhanVien pc = new HRM_Luong_PhuCap_NhanVien();


                                pc.NgayBatDauHieuLuc = itempc.NgayBatDau;
                                pc.IddmLoaiPhuCap = itempc.IddmLoaiPhuCap;
                                pc.IdLuong_NhanVien = luong_NhanVien.IdGuid;
                                pc.SoTien = itempc.SoTien;
                                //  IdCongTyEOS = model.IdCongTyEOS;
                                pc.IdNguonNhapLuong = model.IdNhanVien;
                                pc.LoaiPhuCap = itempc.LoaiPhuCap;
                                pc.NguonNhapLuong = DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString();
                                pc.NgayHetHieuLuc = itempc.NgayKetThuc;

                                listPcluong.Add(pc);
                            }
                            if (listPcluong.Count > 0)
                                db.HRM_Luong_PhuCap_NhanVien.AddRange(listPcluong);

                            nhanvien.IdCongTyEOS = model.IdCongTyEOSNew ?? 0;
                            var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == nhanvien.IdGuid && r.IdBoPhan == model.IdCongTyEOS).ToList();
                            db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                            HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                            bp.IdGuid = Guid.NewGuid().ToString();
                            bp.IdQuyTrinh = nhanvien.IdGuid;
                            bp.IdBoPhan = model.IdCongTyEOSNew ?? 0;
                            bp.TableName = nameof(HRM_NhanVien);
                            db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                            //nhanvien.listIdCongTy = new List<long>();
                            nhanvien.listIdCongTy.Add(model.IdCongTyEOSNew ?? 0);
                            nhanvien.listIdCongTy = nhanvien.listIdCongTy.Where(a => a != model.IdCongTyEOS).ToList();
                            nhanvien.IdBoPhanEOS = model.IdBoPhanEOSNew ?? 0;
                            nhanvien.IdChucDanh = model.IdChucDanh ?? 0;
                            nhanvien.IdChucVu = model.IdChucVu ?? 0;
                            nhanvien.IdDuAnCurrent = model.IdCongTyEOS ?? 0;
                            nhanvien.IdDuAnDieuDong = model.IdCongTyEOSNew ?? 0;
                            clsNhanSu_DuAn modelDuAn = new clsNhanSu_DuAn()
                            {
                                IdDuAn = model.IdCongTyEOSNew ?? 0,
                                NgayBatDauUnix = model.NgayBatDauUnix,
                                listIdUser = new List<string> { nhanvien.IdUser }
                            };
                            _CallWebService.SetNhanSuCreateUser(db, nhanvien);
                            db.Entry(nhanvien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    db.SaveChanges();




                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);

                }
                catch
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                }
            }
        }

        public dynamic KhongDuyetQuyTrinhBoNhiemDieuDongDuAn(HRM_BoNhiemDieuDongDuAn model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_BoNhiemDieuDongDuAn pcuDatabase = db.HRM_BoNhiemDieuDongDuAn.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, model, err))
                        return err;
                    bool Error = FuncBoNhiemDieuDongDuAn(model, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!KhongDuyetSauHamUpdate(db, model))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);
                }
                catch
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                }
            }
        }

        #region tuấn anh phạm
        public dynamic ExportBoNhiemDieuDongDuAn(string Id, ApplicationDbContext db)
        {
            try
            {
                var filenamegui = DateTime.Now.ToOADate();
                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_BoNhiemDieuDongDuAn.docx");
                if (Host == "PHT")
                {
                    fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/QD_BoNhiemDieuDongDuAn.docx");
                }
                string sFileNameCopy = PathUploads + "QD_BoNhiemDieuDongDuAn_" + filenamegui + ".docx";
                HRM_BoNhiemDieuDongDuAn quytrinh = db.HRM_BoNhiemDieuDongDuAn.Find(Id);
                dynamic data = new DynamicObject();
                var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listCongty = _CallWebService.GetDanhSachDuAn(db);
                var CongTyCu = listCongty.Find(x => x.Id == quytrinh.IdCongTyEOS);
                var CongTyMoi = listCongty.Find(x => x.Id == quytrinh.IdCongTyEOSNew);
                var NguoiLaoDong = db.HRM_NhanVien.Find(quytrinh.IdNhanVien);
                File.Copy(fileName, sFileNameCopy, true);
                var doc = DocX.Load(sFileNameCopy);
                string NgayThang = "ngày " + quytrinh.NgayKy?.Day + " tháng " + quytrinh.NgayKy?.Month + " năm " + quytrinh.NgayKy?.Year;
                doc.ReplaceText("@CongTyCu", CongTyCu.TenBoPhan ?? "");
                doc.ReplaceText("@CongTyMoi", CongTyMoi.TenBoPhan ?? "");
                doc.ReplaceText("@NgayThang", NgayThang);
                doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
                var ChucVuCu = _CallWebService.GetChucDanh(db, quytrinh.IdChucDanh ?? 0);
                var BoPhanCu = BoPhans.Find(x => x.Id == quytrinh.IdBoPhanEOSOld);
                var ChucVuMoi = _CallWebService.GetChucVu(db, quytrinh.IdChucVu ?? 0);
                var BoPhan = BoPhans.Find(x => x.Id == quytrinh.IdBoPhanEOSNew);
                string TenChucVuCu = "";
                if (ChucVuCu != null)
                    TenChucVuCu = ChucVuCu.Ten;
                if (BoPhanCu != null)
                    TenChucVuCu = TenChucVuCu + " - " + BoPhanCu.TenBoPhan;
                doc.ReplaceText("@ChucVuCu", TenChucVuCu);
                string TenChucVuMoi = "";
                if (ChucVuMoi != null)
                    TenChucVuMoi = ChucVuMoi.TenChucVu;
                if (BoPhan != null)
                    TenChucVuMoi = TenChucVuMoi + " - " + BoPhan.TenBoPhan;
                doc.ReplaceText("@ChucVuMoi", TenChucVuMoi);
                string ngayQuyetDinh = quytrinh.NgayHieuLuc?.ToString("dd/MM/yyyy") ?? "ngày,...tháng,...năm";
                doc.ReplaceText("@NgayQuyetDinh", ngayQuyetDinh);
                doc.Save();
                //doc.ReplaceText("@NgayQuyetDinh", quytrinh.NgayHieuLuc?.ToString("dd/MM/yyyy"));
                ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);

                err.Data = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_BoNhiemDieuDongDuAn.docx")
                                + "&path=" + Base64Encode(sFileNameCopy);
                doc.Save();
                return err;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        #endregion

        //public bool FuncSetUpdateBoNhiemDieuDongDuAnPhuLucHopDong(HRM_BoNhiemDieuDongDuAn model, ApplicationDbContext db)
        //{
        //    try
        //    {

        //        HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.TrangThai == "HieuLuc" && r.isPhuLuc == false).FirstOrDefault();
        //        HRM_Luong_HopDong LuongHopDong = db.HRM_Luong_HopDong.AsNoTracking().FirstOrDefault(r => r.IdHopDong == hopDong.IdGuid);

        //        string IdHopDong = hopDong.IdGuid;
        //        HRM_HopDong PLuc = hopDong;
        //        PLuc.IdHopDong = IdHopDong;
        //        PLuc.isPhuLuc = true;
        //        PLuc.IdGuid = Guid.NewGuid().ToString();
        //        PLuc.NgayKy = model.NgayKy;
        //        PLuc.NgayHieuLuc = model.NgayBatDau;

        //        PLuc.IdCongTyEOS = model.IdCongTyEOS;
        //        PLuc.IdBoPhanEOS = model.IdBoPhanEOSNew;
        //        CallWebService _CallWebService = _CallWebService;
        //        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHHOPDONG.ToString(), PLuc.IdGuid);
        //        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
        //        {
        //            return false;
        //        }
        //        int CountPL = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.IdHopDong == IdHopDong && r.isPhuLuc == true).Count();
        //        CountPL = CountPL + 1;
        //        PLuc.SoHopDong = "PL-" + CountPL + "-" + hopDong.SoHopDong;
        //        PLuc.IdTrangThai = trangThai.IdTrangThai;
        //        PLuc.TenTrangThai = trangThai.TenTrangThai;
        //        db.HRM_HopDong.Add(PLuc);

        //        List<HRM_HopDongTaiSanBanGiao> HopDongTaiSanBanGiaos = db.HRM_HopDongTaiSanBanGiao.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).ToList();
        //        foreach (var item in HopDongTaiSanBanGiaos)
        //        {
        //            HRM_HopDongTaiSanBanGiao taiSanPL = item;
        //            taiSanPL.IdHopDong = PLuc.IdGuid;
        //            taiSanPL.IdGuid = Guid.NewGuid().ToString();
        //            db.HRM_HopDongTaiSanBanGiao.Add(taiSanPL);
        //        }


        //        HRM_BaoHiem_HopDong BaoHiemOld = db.HRM_BaoHiem_HopDong.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).FirstOrDefault();
        //        if (BaoHiemOld != null)
        //        {
        //            HRM_BaoHiem_HopDong BaoHiemPL = BaoHiemOld;
        //            BaoHiemPL.IdGuid = Guid.NewGuid().ToString();
        //            BaoHiemPL.IdHopDong = PLuc.IdGuid;
        //            db.HRM_BaoHiem_HopDong.Add(BaoHiemPL);
        //        }
        //        List<HRM_HopDongCongViec> listMoTaCongViecOld = db.HRM_HopDongCongViec.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).ToList();
        //        if (listMoTaCongViecOld.Count > 0)
        //        {
        //            List<HRM_HopDongCongViec> listMoTaCongViecPL = new List<HRM_HopDongCongViec>();
        //            foreach (var item in listMoTaCongViecOld)
        //            {
        //                HRM_HopDongCongViec CVPL = item;
        //                CVPL.IdGuid = Guid.NewGuid().ToString();
        //                CVPL.IdHopDong = PLuc.IdGuid;
        //                listMoTaCongViecPL.Add(CVPL);
        //            }
        //            db.HRM_HopDongCongViec.AddRange(listMoTaCongViecOld);
        //        }

        //        HRM_Luong_HopDong LuongPL = new HRM_Luong_HopDong
        //        {
        //            IddmHinhThucTraLuong = LuongHopDong.IddmHinhThucTraLuong,
        //            IdHopDong = PLuc.IdGuid,
        //            IdNhanVien = PLuc.IdNhanVien,
        //            IdQuyetDinh = model.IdGuid,
        //            SoTaiKhoan = LuongHopDong.SoTaiKhoan,
        //            TenNganHang = LuongHopDong.TenNganHang,

        //            TyLeHuongLuong = LuongHopDong.TyLeHuongLuong,
        //            HinhThucLuong = LuongHopDong.HinhThucLuong,
        //            NgayBatDau = model.NgayBatDau,
        //            IdGuid = Guid.NewGuid().ToString(),
        //            LuongBaoHiem = model.LuongHachToanSauDieuChinh,
        //            NgayKetThuc = dungchung.UnixTimeStampToDateTime(0),
        //        };
        //        db.HRM_Luong_HopDong.Add(LuongPL);

        //        List<HRM_HopDongPhuCap> lstPhuCapPL = new List<HRM_HopDongPhuCap>();
        //        foreach (var item in model.lstPhuCap)
        //        {
        //            HRM_HopDongPhuCap PhuCap = new HRM_HopDongPhuCap
        //            {
        //                IdGuid = Guid.NewGuid().ToString(),
        //                IddmLoaiPhuCap = item.IddmLoaiPhuCap,
        //                IdHopDong = PLuc.IdGuid,
        //                SoTien = item.SoTien,
        //                LoaiPhuCap = item.LoaiPhuCap,
        //                NgayBatDau = item.NgayBatDau,
        //                NgayKetThuc = item.NgayKetThuc,
        //                TableName = "HRM_HopDong"
        //            };
        //            lstPhuCapPL.Add(PhuCap);
        //        }
        //        db.HRM_HopDongPhuCap.AddRange(lstPhuCapPL);

        //        HRM_HopDong_QuyetDinh quyetDinh = new HRM_HopDong_QuyetDinh
        //        {
        //            IdGuid = Guid.NewGuid().ToString(),
        //            IdHopDong = PLuc.IdGuid,
        //            IdQuyetDinh = model.IdGuid,
        //            TableName = "HRM_BoNhiemChucVu",
        //            MaQuyTrinh = DefineData.eAction.QUYTRINHBONHIEMCHUCVU.ToString()
        //        };
        //        db.HRM_HopDong_QuyetDinh.Add(quyetDinh);
        //        db.SaveChanges();
        //        return true;

        //    }
        //    catch (Exception)
        //    {

        //        return false;
        //    }

        //}
        #endregion

        #region Bổ nhiệm chức vụ
        public dynamic GetNextMaQuyTrinhBoNhiemChucVu(ApplicationDbContext db)
        {
            dynamic dnm = new DynamicObject();
            dnm.SoQuyTrinh = GenerateNextCode(db.HRM_BoNhiemChucVu.Select(x => x.SoQuyTrinh), "BNCV");
            return dnm;
        }

        public dynamic SetQuyQuyTrinhBoNhiemChucVu(HRM_BoNhiemChucVu model, ApplicationDbContext db = null)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var QuyTrinhdb = db.HRM_BoNhiemChucVu.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncUpdateBoNhiemChucVu(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!SetSauHamUpdate(db, QuyTrinhdb, model, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }


                    if (string.IsNullOrEmpty(model.IdTrangThai))
                    {

                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHBONHIEMCHUCVU.ToString(), model.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        }

                        model.IdTrangThai = trangThai.IdTrangThai;
                        model.TenTrangThai = trangThai.TenTrangThai;
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        db.SaveChanges();
                    }
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model);

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
            }
        }
        public dynamic GetDanhSachBoNhiemChucVu(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_BoNhiemChucVu> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_BoNhiemChucVu
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_BoNhiemChucVu
                             orderby a.Created descending
                             select a;

                var listCongTy = _CallWebService.GetDanhSachCongTy(db);

                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Created >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Created < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                if (TimKiem.IdBoPhanEOSCu > 0)
                {
                    result = result.Where(a => a.IdBoPhanEOSCu == TimKiem.IdBoPhanEOSCu);

                }
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                List<string> lstNguoiKy = itemList.Select(r => r.IdNguoiKy).ToList();
                lstNguoiKy.AddRange(itemList.Select(r => r.IdNhanVien).ToList());
                var Nhanviens = db.HRM_NhanVien.Where(r => lstNguoiKy.Contains(r.IdGuid)).ToList();

                foreach (var item in itemList)
                {
                    var NhanVien = Nhanviens.Where(r => r.IdGuid == item.IdNhanVien).FirstOrDefault();
                    item.TenNhanVien = NhanVien.TenDayDu;
                    item.TenNguoiKy = Nhanviens.Where(r => r.IdGuid == item.IdNguoiKy).Select(r => r.TenDayDu).FirstOrDefault();
                    item.TenChucDanhMoi = listChucDanh.Where(r => r.Id == item.IdChucDanhEOS).Select(r => r.Ten).FirstOrDefault();
                    item.TenChucDanhCu = listChucDanh.Where(r => r.Id == NhanVien.IdChucDanh).Select(r => r.Ten).FirstOrDefault();


                    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                    item.NgayBatDauUnix = DateTimeToUnixTimeStamp(item.NgayBatDau);
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
        public bool FuncUpdateBoNhiemChucVu(HRM_BoNhiemChucVu item, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (item.NgayKyUnix > 0)
                    item.NgayKy = dungchung.UnixTimeStampToDateTime(item.NgayKyUnix);
                if (item.NgayBatDauUnix > 0)
                    item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);

                if (string.IsNullOrEmpty(item.IdGuid))
                {
                    item.IdBoPhanEOSCu=db.HRM_NhanVien.Where(r => r.IdGuid == item.IdNhanVien).Select(r => r.IdBoPhanEOS).FirstOrDefault();
                    item.IdDuAn = item.IdCongTyEOS;
                    if (string.IsNullOrEmpty(item.eAction))
                        item.eAction = DefineData.eAction.QUYTRINHBONHIEMCHUCVU.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, item, err))
                        return false;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var listBoPhans = db.HRM_BoNhiemChucVu_BoPhan.Where(r => r.IdQuyetDinh == item.IdGuid).ToList();
                db.HRM_BoNhiemChucVu_BoPhan.RemoveRange(listBoPhans);
                var HRM_HopDongPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == item.IdGuid).ToList();
                db.HRM_HopDongPhuCap.RemoveRange(HRM_HopDongPhuCap);

                if (item.lstPhuCap != null)
                {
                    List<HRM_HopDongPhuCap> lstPhuCap = item.lstPhuCap;
                    foreach (var itemPhuCap in lstPhuCap)
                    {
                        if (itemPhuCap.NgayBatDauUnix > 0)
                            itemPhuCap.NgayBatDau = dungchung.UnixTimeStampToDateTime(itemPhuCap.NgayBatDauUnix);
                        if (itemPhuCap.NgayKetThucUnix > 0)
                            itemPhuCap.NgayKetThuc = dungchung.UnixTimeStampToDateTime(itemPhuCap.NgayKetThucUnix);
                        itemPhuCap.IdQuyetDinh = item.IdGuid;
                        itemPhuCap.TableName = DefineData.eTable.HRM_BoNhiemChucVu.ToString();
                        itemPhuCap.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(itemPhuCap).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }

                }

                foreach (var itemPhongBan in item.lstBoPhan)
                {
                    itemPhongBan.IdQuyetDinh = item.IdGuid;
                    itemPhongBan.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(itemPhongBan).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                if (item.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in item.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_BoNhiemChucVu, item.IdGuid);
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, item, eLoaiHam, err))
                    return false;
                db.SaveChanges();
                return true;
            }

            catch (Exception ex)
            {
                LogErrorFormat("UpdateDieuChinhLuong data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return false;
            }

        }

        public dynamic GetQuyTrinhBoNhiemChucVu(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_BoNhiemChucVu model = db.HRM_BoNhiemChucVu.Find(Id);
            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            if (model.isKetThuc == true)
                model.TenNhanVien = model.NoiDung;
            model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
            model.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKy);
            model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_BoNhiemChucVu.ToString(), model.IdGuid);
            var lstBoPhan = db.HRM_BoNhiemChucVu_BoPhan.Where(r => r.IdQuyetDinh == Id).ToList();
            var bophans = _CallWebService.GetDanhSachBoPhan(db);
            foreach (var item in lstBoPhan)
            {
                item.TenBoPhan = bophans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
            }
            model.lstBoPhan = lstBoPhan;
            var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == Id && r.TableName == DefineData.eTable.HRM_BoNhiemChucVu.ToString()).ToList();
            foreach (var item in lstPhuCap)
            {
                item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                item.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDau);
            }
            model.lstPhuCap = lstPhuCap;
            return model;
        }
        public void ChuyenTiepQuyTrinhBoNhiemChucVuUpdate(ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            var listmodel = db.HRM_BoNhiemChucVu.Where(a => a.isKetThuc == true).OrderByDescending(a => a.Modified).ToList();
            var listId = listmodel.Select(a => a.IdGuid).Distinct().ToList();
            var listIdNV = listmodel.Select(a => a.IdNhanVien).Distinct().ToList();
            var listluongNhanVien = db.HRM_Luong_NhanVien.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();
            var listPCluongNhanVien = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();
            var lstBoPhan = db.HRM_BoNhiemChucVu_BoPhan.Where(r => listId.Contains(r.IdQuyetDinh)).ToList();
            var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => listId.Contains(r.IdQuyetDinh)).ToList();
            List<HRM_Luong_PhuCap_NhanVien> listPcluong = new List<HRM_Luong_PhuCap_NhanVien>();
            List<HRM_Luong_NhanVien> listluong = new List<HRM_Luong_NhanVien>();

            foreach (var model in listmodel)
            {
                model.lstPhuCap = lstPhuCap.Where(a => a.IdQuyetDinh == model.IdGuid).ToList();
                var luongcheck = listluongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdNhanVien && a.NgayBatDauHieuLuc <= model.NgayBatDau && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= model.NgayBatDau));

                var luongcheckhopdong = listluongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdNhanVien && a.NgayBatDauHieuLuc > model.NgayBatDau);
                HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien
                {
                    IdGuid = Guid.NewGuid().ToString(),
                    IdCongTyEOS = model.IdCongTyEOS,
                    KieuLuong = model.KieuLuong,
                    IdNhanVien = model.IdNhanVien,
                    Luong = model.LuongThucTeSauDieuChinh,
                    LuongHachToan = model.LuongHachToanSauDieuChinh,
                    //LuongDanhGia = luongDanhGia,
                    NgayBatDauHieuLuc = model.NgayBatDau,
                    LoaiLuong = luongcheck == null ? 0 : luongcheck.LoaiLuong,
                    IdNguonNhapLuong = model.IdGuid,
                    NgayHetHieuLuc = luongcheckhopdong == null ? dungchung.UnixTimeStampToDateTime(0) : luongcheckhopdong.NgayBatDauHieuLuc.AddDays(-1),
                    NguonNhapLuong = DefineData.eTable.HRM_BoNhiemChucVu.ToString()
                };
                if (luong_NhanVien.NgayHetHieuLuc != null && luong_NhanVien.NgayHetHieuLuc.Value.Year < 2000)
                    luong_NhanVien.NgayHetHieuLuc = null;
                listluong.Add(luong_NhanVien);
                if (luongcheck != null)
                {
                    luongcheck.NgayHetHieuLuc = model.NgayBatDau.AddDays(-1);
                    db.Entry(luongcheck).State = Microsoft.EntityFrameworkCore.EntityState.Modified;


                    var PCcheck = listPCluongNhanVien.Where(a => a.IdNhanVien == model.IdNhanVien && luongcheck.IdNguonNhapLuong == a.IdNguonNhapLuong).ToList();
                    foreach (var item in PCcheck)
                    {
                        item.NgayHetHieuLuc = model.NgayBatDau.AddDays(-1);
                        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }
                foreach (var itempc in model.lstPhuCap)
                {
                    HRM_Luong_PhuCap_NhanVien pc = new HRM_Luong_PhuCap_NhanVien
                    {

                        NgayBatDauHieuLuc = itempc.NgayBatDau,
                        IddmLoaiPhuCap = itempc.IddmLoaiPhuCap,
                        IdLuong_NhanVien = luong_NhanVien.IdGuid,
                        IdNhanVien = model.IdNhanVien,
                        SoTien = itempc.SoTien,
                        IdCongTyEOS = model.IdCongTyEOS,
                        IdNguonNhapLuong = model.IdNhanVien,
                        IdGuid = Guid.NewGuid().ToString(),
                        LoaiPhuCap = itempc.LoaiPhuCap,
                        NguonNhapLuong = DefineData.eTable.HRM_BoNhiemChucVu.ToString(),
                        NgayHetHieuLuc = itempc.NgayKetThuc,
                    };
                    if (pc.NgayHetHieuLuc != null && pc.NgayHetHieuLuc.Value.Year < 2000)
                        pc.NgayHetHieuLuc = null;
                    listPcluong.Add(pc);
                }
                db.HRM_Luong_PhuCap_NhanVien.AddRange(listPcluong);
                db.HRM_Luong_NhanVien.AddRange(listluong);
            }
            db.SaveChanges();
        }
        public void ChuyenTiepQuyTrinhDieuDongUpdate(ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            var listmodel = db.HRM_BoNhiemDieuDongDuAn.Where(a => a.isKetThuc == true).OrderByDescending(a => a.Modified).ToList();
            var listId = listmodel.Select(a => a.IdGuid).Distinct().ToList();
            var listIdNV = listmodel.Select(a => a.IdNhanVien).Distinct().ToList();
            var listluongNhanVien = db.HRM_Luong_NhanVien.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();
            var listPCluongNhanVien = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();
            var lstBoPhan = db.HRM_BoNhiemChucVu_BoPhan.Where(r => listId.Contains(r.IdQuyetDinh)).ToList();
            var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => listId.Contains(r.IdQuyetDinh)).ToList();
            List<HRM_Luong_PhuCap_NhanVien> listPcluong = new List<HRM_Luong_PhuCap_NhanVien>();
            List<HRM_Luong_NhanVien> listluong = new List<HRM_Luong_NhanVien>();

            foreach (var model in listmodel)
            {
                model.lstPhuCap = lstPhuCap.Where(a => a.IdQuyetDinh == model.IdGuid).ToList();
                var luongcheck = listluongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdNhanVien && a.NgayBatDauHieuLuc <= model.NgayHieuLuc && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= model.NgayHieuLuc));

                var luongcheckhopdong = listluongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdNhanVien && a.NgayBatDauHieuLuc > model.NgayHieuLuc);
                HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien
                {
                    IdGuid = Guid.NewGuid().ToString(),
                    IdCongTyEOS = model.IdCongTyEOSNew ?? 0,
                    KieuLuong = model.KieuLuong,
                    IdNhanVien = model.IdNhanVien,
                    Luong = model.LuongThucTeSauDieuChinh,
                    LuongHachToan = model.LuongHachToanSauDieuChinh,
                    //LuongDanhGia = luongDanhGia,
                    NgayBatDauHieuLuc = model.NgayHieuLuc.Value,
                    LoaiLuong = luongcheck == null ? 0 : luongcheck.LoaiLuong,
                    IdNguonNhapLuong = model.IdGuid,
                    NgayHetHieuLuc = luongcheckhopdong == null ? dungchung.UnixTimeStampToDateTime(0) : luongcheckhopdong.NgayBatDauHieuLuc.AddDays(-1),
                    NguonNhapLuong = DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString()
                };
                if (luong_NhanVien.NgayHetHieuLuc != null && luong_NhanVien.NgayHetHieuLuc.Value.Year < 2000)
                    luong_NhanVien.NgayHetHieuLuc = null;
                listluong.Add(luong_NhanVien);
                if (luongcheck != null)
                {
                    luongcheck.NgayHetHieuLuc = model.NgayHieuLuc.Value.AddDays(-1);
                    db.Entry(luongcheck).State = Microsoft.EntityFrameworkCore.EntityState.Modified;


                    var PCcheck = listPCluongNhanVien.Where(a => a.IdNhanVien == model.IdNhanVien && luongcheck.IdNguonNhapLuong == a.IdNguonNhapLuong).ToList();
                    foreach (var item in PCcheck)
                    {
                        item.NgayHetHieuLuc = model.NgayHieuLuc.Value.AddDays(-1);
                        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }
                foreach (var itempc in model.lstPhuCap)
                {
                    HRM_Luong_PhuCap_NhanVien pc = new HRM_Luong_PhuCap_NhanVien
                    {

                        NgayBatDauHieuLuc = itempc.NgayBatDau,
                        IddmLoaiPhuCap = itempc.IddmLoaiPhuCap,
                        IdNhanVien = model.IdNhanVien,
                        IdLuong_NhanVien = luong_NhanVien.IdGuid,
                        SoTien = itempc.SoTien,
                        IdCongTyEOS = model.IdCongTyEOSNew ?? 0,
                        IdNguonNhapLuong = model.IdGuid,
                        IdGuid = Guid.NewGuid().ToString(),
                        LoaiPhuCap = itempc.LoaiPhuCap,
                        NguonNhapLuong = DefineData.eTable.HRM_BoNhiemDieuDongDuAn.ToString(),
                        NgayHetHieuLuc = itempc.NgayKetThuc,
                    };
                    if (pc.NgayHetHieuLuc != null && pc.NgayHetHieuLuc.Value.Year < 2000)
                        pc.NgayHetHieuLuc = null;
                    listPcluong.Add(pc);
                }
                db.HRM_Luong_PhuCap_NhanVien.AddRange(listPcluong);
                db.HRM_Luong_NhanVien.AddRange(listluong);
            }
            db.SaveChanges();
        }

        public dynamic ChuyenTiepQuyTrinhBoNhiemChucVu(HRM_BoNhiemChucVu model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.TrangThai == "HieuLuc").OrderByDescending(a => a.NgayHieuLuc).FirstOrDefault();
                    if (hopDong == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên chưa có hợp đồng");
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_BoNhiemChucVu pcuDatabase = db.HRM_BoNhiemChucVu.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    var nhanvien = db.HRM_NhanVien.Find(model.IdNhanVien);
                    model.NoiDung = nhanvien.MaNhanVien + " - " + nhanvien.TenDayDu;
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, model, err, model.IdCongTyEOS))
                        return err;
                    bool Error = FuncUpdateBoNhiemChucVu(model, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, model, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    //   string IdTrangThaiCuoi = GetTrangThaiCuoi(smartDB, DefineData.eAction.QUYTRINHBONHIEMCHUCVU.ToString());
                    if (model.isKetThuc == true)
                    {
                        LogLichSuNhanVien(db, model.IdNhanVien, model.IdGuid, "HRM_BoNhiemChucVu", DefineData.eAction.QUYTRINHBONHIEMCHUCVU.ToString(), model.NgayKyUnix);

                        if (nhanvien != null)
                        {
                            var dateTime = dungchung.UnixTimeStampToDateTime(0);
                            //HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.TrangThai == "HieuLuc").OrderByDescending(a => a.NgayHieuLuc).FirstOrDefault();

                            List<HRM_Luong_NhanVien> luongs = db.HRM_Luong_NhanVien.Where(r => r.IdNhanVien == model.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc >= model.NgayBatDau)).OrderBy(a => a.Modified).ToList();
                            if (luongs != null)
                            {
                                foreach (var item in luongs)
                                {

                                    item.NgayHetHieuLuc = model.NgayBatDau.AddDays(-1);
                                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                }

                            }
                            double LuongBH = 0;
                            //if (luong_NhanVienOld != null)
                            //{
                            //    LuongBH = luong_NhanVienOld.LuongHachToan;
                            //    db.HRM_Luong_NhanVien.Remove(luong_NhanVienOld);
                            ////}
                            //double luongDanhGia = 0;
                            //if (hopDong != null)
                            //{
                            //    var luong = db.HRM_Luong_HopDong.FirstOrDefault(a => a.IdHopDong == hopDong.IdGuid);
                            //    if (luong != null)
                            //        luongDanhGia = luong.LuongDanhGia ?? 0;
                            //}
                            HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien
                            {
                                IdGuid = Guid.NewGuid().ToString(),
                                IdCongTyEOS = model.IdCongTyEOS,
                                KieuLuong = model.KieuLuong,
                                IdNhanVien = model.IdNhanVien,
                                Luong = model.LuongThucTeSauDieuChinh,
                                LuongHachToan = model.LuongHachToanSauDieuChinh,
                                //LuongDanhGia = luongDanhGia,
                                NgayBatDauHieuLuc = model.NgayBatDau,
                                LoaiLuong = hopDong.LoaiLuong,
                                IdNguonNhapLuong = model.IdGuid,
                                NgayHetHieuLuc = dungchung.UnixTimeStampToDateTime(0),
                                NguonNhapLuong = DefineData.eTable.HRM_BoNhiemChucVu.ToString()
                            };
                            db.HRM_Luong_NhanVien.Add(luong_NhanVien);

                            List<HRM_HopDongPhuCap> lstPhuCap = db.HRM_HopDongPhuCap.AsNoTracking().Where(r => r.IdQuyetDinh == model.IdGuid && r.TableName == DefineData.eTable.HRM_BoNhiemChucVu.ToString()).ToList();
                            List<HRM_Luong_PhuCap_NhanVien> luongPCs = db.HRM_Luong_PhuCap_NhanVien.Where(r => r.IdNhanVien == model.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc >= model.NgayBatDau)).OrderBy(a => a.Modified).ToList();
                            luongPCs.ForEach(item =>
                            {
                                item.NgayHetHieuLuc = model.NgayBatDau.AddDays(-1);
                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            });
                            List<HRM_Luong_PhuCap_NhanVien> listPcluong = new List<HRM_Luong_PhuCap_NhanVien>();
                            foreach (var itempc in lstPhuCap)
                            {
                                HRM_Luong_PhuCap_NhanVien pc = new HRM_Luong_PhuCap_NhanVien
                                {

                                    NgayBatDauHieuLuc = itempc.NgayBatDau,
                                    IddmLoaiPhuCap = itempc.IddmLoaiPhuCap,
                                    IdLuong_NhanVien = luong_NhanVien.IdGuid,
                                    SoTien = itempc.SoTien,
                                    IdCongTyEOS = model.IdCongTyEOS,
                                    IdNguonNhapLuong = model.IdNhanVien,
                                    IdGuid = Guid.NewGuid().ToString(),
                                    LoaiPhuCap = itempc.LoaiPhuCap,
                                    NguonNhapLuong = DefineData.eTable.HRM_BoNhiemChucVu.ToString(),
                                    NgayHetHieuLuc = itempc.NgayKetThuc,
                                };
                                listPcluong.Add(pc);
                            }
                            db.HRM_Luong_PhuCap_NhanVien.AddRange(listPcluong);


                            nhanvien.IdBoPhanEOS = model.IdBoPhanEOS;
                            nhanvien.IdChucDanh = model.IdChucDanhEOS;
                            nhanvien.IdChucVu = model.IdChucVuEOS;
                            db.Entry(nhanvien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);

                }
                catch
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhBoNhiemChucVu(HRM_BoNhiemChucVu model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_BoNhiemChucVu pcuDatabase = db.HRM_BoNhiemChucVu.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, model, err))
                        return err;

                    bool Error = FuncUpdateBoNhiemChucVu(model, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }

                    if (!KhongDuyetSauHamUpdate(db, model))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);

                }
                catch
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                }
            }
        }
        public dynamic DeleteQuyTrinhBoNhiemChucVu(string Id, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_BoNhiemChucVu model = db.HRM_BoNhiemChucVu.Find(Id);

                    var lstPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdQuyetDinh == Id && r.TableName == DefineData.eTable.HRM_BoNhiemChucVu.ToString()).ToList();

                    if (lstPhuCap.Count() > 0)
                        db.HRM_HopDongPhuCap.RemoveRange(lstPhuCap);
                    var listBoPhans = db.HRM_BoNhiemChucVu_BoPhan.Where(r => r.IdQuyetDinh == model.IdGuid).ToList();
                    db.HRM_BoNhiemChucVu_BoPhan.RemoveRange(listBoPhans);
                    db.HRM_BoNhiemChucVu.Remove(model);
                    db.SaveChanges();
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công");
                }
                catch (Exception ex)
                {
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Xóa dữ liệu không thành công " + ex.Message);
                }
            }

        }

        public dynamic ExportBoNhiemChucVu(string Id, ApplicationDbContext db)
        {
            var filenamegui = DateTime.Now.ToOADate();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QD_BoNhiemChucVu.docx");
            if (Host == "PHT")
            {
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/QD_BoNhiemChucVu.docx");
            }
            string sFileNameCopy = PathUploads + "QD_BoNhiemChucVu_" + filenamegui + ".docx";
            HRM_BoNhiemChucVu quytrinh = db.HRM_BoNhiemChucVu.Find(Id);
            dynamic data = new DynamicObject();
            var BoPhans = _CallWebService.GetCongTyKyHopDong(db);
            var CongTy = BoPhans.Find(x => x.Id == quytrinh.IdCongTyEOS);
            var NguoiLaoDong = db.HRM_NhanVien.Find(quytrinh.IdNhanVien);
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            string NgayThang = "ngày " + quytrinh.NgayKy.Day + " tháng " + quytrinh.NgayKy.Month + " năm " + quytrinh.NgayKy.Year;
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@NgayThang", NgayThang);
            doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@SoQuyetDinh", quytrinh.SoQuyetDinh ?? "");

            //    long IdChuVuCu = db.HRM_Link_NhanVien_BoPhan_ChucVu.Where(r => r.IdBoPhanEOS != quytrinh.IdBoPhanEOS && r.IdChucVuEOS != quytrinh.IdChucVu && r.IdCongTyEOS == quytrinh.IdCongTyEOS && quytrinh.Created> r.Modified).OrderByDescending(a=>a.Created).Select(c=>c.IdChucVuEOS).FirstOrDefault();
            var ChucVuCu = _CallWebService.GetChucVu(db, quytrinh.IdChucDanhEOS);
            var BoPhanCu = _CallWebService.GetBoPhan(db, quytrinh.IdBoPhanEOSCu);
            var ChucVuMoi = _CallWebService.GetChucVu(db, quytrinh.IdChucVuEOS);
            var BoPhan = _CallWebService.GetBoPhan(db, quytrinh.IdBoPhanEOS);
            string TenChucVuCu = "";
            if (ChucVuCu != null)
                TenChucVuCu = ChucVuCu.TenChucVu;
            if (BoPhanCu != null)
                TenChucVuCu = TenChucVuCu + " - " + BoPhanCu.TenBoPhan;

            doc.ReplaceText("@ChucVuCu", TenChucVuCu);
            string TenChucVuMoi = "";
            if (ChucVuMoi != null)
                TenChucVuMoi = ChucVuMoi.TenChucVu;
            if (BoPhan != null)
                TenChucVuMoi = TenChucVuMoi + " - " + BoPhan.TenBoPhan;
            doc.ReplaceText("@ChucVuMoi", TenChucVuMoi);
            doc.ReplaceText("@NgayQuyetDinh", quytrinh.NgayBatDau.ToString("dd/MM/yyyy"));
            doc.Save();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QD_BoNhiemChucVu.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);

            return data;
        }
        //public bool FuncSetUpdateBoNhiemChucVuPhuLucHopDong(HRM_BoNhiemChucVu model, ApplicationDbContext db)
        //{
        //    try
        //    {

        //        HRM_HopDong hopDong = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.TrangThai == "HieuLuc" && r.isPhuLuc == false).FirstOrDefault();
        //        string IdHopDong = hopDong.IdGuid;

        //        HRM_Luong_HopDong LuongHopDong = db.HRM_Luong_HopDong.AsNoTracking().FirstOrDefault(r => r.IdHopDong == hopDong.IdGuid);
        //        HRM_HopDong PLuc = hopDong;
        //        PLuc.IdHopDong = hopDong.IdGuid;
        //        PLuc.isPhuLuc = true;
        //        PLuc.IdGuid = Guid.NewGuid().ToString();
        //        PLuc.NgayKy = model.NgayKy;
        //        PLuc.IdCapBac = model.IddmCapBac;
        //        PLuc.NgayHieuLuc = model.NgayBatDau;
        //        PLuc.IdBoPhanEOS = model.IdBoPhanEOS;
        //        PLuc.IdNguoiDaiDien = model.IdNguoiKy;
        //        PLuc.NguoiDaiDienIdChucVu = model.IdChucVuNguoiKy;
        //        CallWebService _CallWebService = _CallWebService;
        //        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHHOPDONG.ToString(), PLuc.IdGuid);
        //        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
        //        {
        //            return false;
        //        }
        //        int CountPL = db.HRM_HopDong.AsNoTracking().Where(r => r.IdNhanVien == model.IdNhanVien && r.IdHopDong == IdHopDong && r.isPhuLuc == true).Count();
        //        CountPL = CountPL + 1;
        //        PLuc.SoHopDong = "PL-" + CountPL + "-" + hopDong.SoHopDong;
        //        PLuc.IdTrangThai = trangThai.IdTrangThai;
        //        PLuc.TenTrangThai = trangThai.TenTrangThai;
        //        db.HRM_HopDong.Add(PLuc);

        //        HRM_Luong_HopDong LuongPL = new HRM_Luong_HopDong
        //        {
        //            IddmHinhThucTraLuong = LuongHopDong.IddmHinhThucTraLuong,
        //            IdHopDong = PLuc.IdGuid,
        //            IdNhanVien = PLuc.IdNhanVien,
        //            IdQuyetDinh = model.IdGuid,
        //            SoTaiKhoan = LuongHopDong.SoTaiKhoan,
        //            TenNganHang = LuongHopDong.TenNganHang,
        //            TyLeHuongLuong = LuongHopDong.TyLeHuongLuong,
        //            HinhThucLuong = LuongHopDong.HinhThucLuong,
        //            NgayBatDau = model.NgayBatDau,
        //            IdGuid = Guid.NewGuid().ToString(),
        //            LuongBaoHiem = model.LuongHachToanSauDieuChinh,
        //            NgayKetThuc = dungchung.UnixTimeStampToDateTime(0),
        //        };
        //        db.HRM_Luong_HopDong.Add(LuongPL);
        //        List<HRM_HopDongPhuCap> lstPhuCapPL = new List<HRM_HopDongPhuCap>();
        //        foreach (var item in model.lstPhuCap)
        //        {
        //            HRM_HopDongPhuCap PhuCap = new HRM_HopDongPhuCap
        //            {
        //                IdGuid = Guid.NewGuid().ToString(),
        //                IddmLoaiPhuCap = item.IddmLoaiPhuCap,
        //                IdHopDong = PLuc.IdGuid,
        //                SoTien = item.SoTien,
        //                LoaiPhuCap = item.LoaiPhuCap,
        //                NgayBatDau = item.NgayBatDau,
        //                NgayKetThuc = item.NgayKetThuc,
        //                TableName = "HRM_HopDong"
        //            };
        //            lstPhuCapPL.Add(PhuCap);
        //        }
        //        db.HRM_HopDongPhuCap.AddRange(lstPhuCapPL);

        //        List<HRM_HopDongTaiSanBanGiao> HopDongTaiSanBanGiaos = db.HRM_HopDongTaiSanBanGiao.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).ToList();
        //        foreach (var item in HopDongTaiSanBanGiaos)
        //        {
        //            HRM_HopDongTaiSanBanGiao taiSanPL = item;
        //            taiSanPL.IdHopDong = PLuc.IdGuid;
        //            taiSanPL.IdGuid = Guid.NewGuid().ToString();
        //            db.HRM_HopDongTaiSanBanGiao.Add(taiSanPL);
        //        }

        //        HRM_BaoHiem_HopDong BaoHiemOld = db.HRM_BaoHiem_HopDong.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).FirstOrDefault();
        //        if (BaoHiemOld != null)
        //        {
        //            HRM_BaoHiem_HopDong BaoHiemPL = BaoHiemOld;
        //            BaoHiemPL.IdGuid = Guid.NewGuid().ToString();
        //            BaoHiemPL.IdHopDong = PLuc.IdHopDong;
        //            db.HRM_BaoHiem_HopDong.Add(BaoHiemPL);
        //        }
        //        List<HRM_HopDongCongViec> listMoTaCongViecOld = db.HRM_HopDongCongViec.AsNoTracking().Where(r => r.IdHopDong == IdHopDong).ToList();
        //        if (listMoTaCongViecOld.Count > 0)
        //        {
        //            List<HRM_HopDongCongViec> listMoTaCongViecPL = new List<HRM_HopDongCongViec>();
        //            foreach (var item in listMoTaCongViecOld)
        //            {
        //                HRM_HopDongCongViec CVPL = item;
        //                CVPL.IdGuid = Guid.NewGuid().ToString();
        //                CVPL.IdHopDong = PLuc.IdGuid;
        //                listMoTaCongViecPL.Add(CVPL);
        //            }
        //            db.HRM_HopDongCongViec.AddRange(listMoTaCongViecOld);
        //        }

        //        HRM_HopDong_QuyetDinh quyetDinh = new HRM_HopDong_QuyetDinh
        //        {
        //            IdGuid = Guid.NewGuid().ToString(),
        //            IdHopDong = PLuc.IdGuid,
        //            IdQuyetDinh = model.IdGuid,
        //            TableName = "HRM_BoNhiemChucVu",
        //            MaQuyTrinh = DefineData.eAction.QUYTRINHBONHIEMCHUCVU.ToString()
        //        };
        //        db.HRM_HopDong_QuyetDinh.Add(quyetDinh);
        //        db.SaveChanges();
        //        return true;

        //    }
        //    catch (Exception)
        //    {

        //        return false;
        //    }

        //}
        #endregion
    }
}