#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Entity;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services.TuyenDung
{
    public class FuncDeNghiTuyenDung : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncDeNghiTuyenDung(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncDeNghiTuyenDung> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        
        
        #region Đề nghị tuyển dụng
        public string GetNextMaDeNghiTuyenDung(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_TuyenDung_DeNghi.Select(x => x.SoPhieu), DefineData.MaDeNghiTuyenDung, digits: 3);

        public dynamic GetDanhSachDeNghiTuyenDung(dynamic dyn, ApplicationDbContext db = null)
        {


            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_TuyenDung_DeNghi> query = db.HRM_TuyenDung_DeNghi;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));

                List<HRM_TuyenDung_DeNghi> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;

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

        public MessageReturn SetDeNghiTuyenDung(HRM_TuyenDung_DeNghi model, ApplicationDbContext db)
        {
            
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string Error = FuncSetDeNghiTuyenDung(model, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, model.IdGuid);
                    }
                    if (string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        dynamic aTrangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.DENGHITUYENDUNG.ToString(), model.IdGuid);
                        if (aTrangThai.IdTrangThai == null || aTrangThai.IdTrangThai == "")
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        }
                        model.IdTrangThai = aTrangThai.IdTrangThai;
                        model.TenTrangThai = aTrangThai.TenTrangThai;
                    }
                    db.SaveChanges();
                        
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển " + ex.Message);
                }
            }

        }


        public string FuncSetDeNghiTuyenDung(HRM_TuyenDung_DeNghi item, ApplicationDbContext db = null)
        {
            try
            {
                if (item.SoLuongTuyenDung == 0)
                    item.SoLuongTuyenDung = item.lstChiTiet.Sum(x => x.BoSung + x.ThayThe);

                item.NgayDeNghi = dungchung.UnixTimeStampToDateTime(item.NgayDeNghiUnix);
                if (!CheckID(item.IdGuid))
                {

                    item.IdGuid = Guid.NewGuid().ToString();
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                db.SaveChanges();
                foreach (var itemChiTiet in item.lstChiTiet)
                {
                    string IdChiTiet = itemChiTiet.IdGuid;
                    if (itemChiTiet.isDeleted)
                    {
                        if (string.IsNullOrEmpty(IdChiTiet) == false)
                        {
                            db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                        }
                    }
                    else
                    {
                        itemChiTiet.IdTuyenDungDeNghi = item.IdGuid; ;
                        itemChiTiet.ThoiGianBatDau = dungchung.UnixTimeStampToDateTime(itemChiTiet.ThoiGianBatDauUnix);
                        itemChiTiet.ThoiGianKetThuc = dungchung.UnixTimeStampToDateTime(itemChiTiet.ThoiGianKetThucUnix);
                        if (!CheckID(IdChiTiet))
                        {
                            itemChiTiet.IdTuyenDungDeNghi = item.IdGuid;
                            itemChiTiet.IdGuid = Guid.NewGuid().ToString();
                            db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        }
                        else
                        {
                            db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                        }
                    }
                }
                db.SaveChanges();
                return "0";
            }
            catch (Exception ex)
            {
                string errs = "Có lỗi trong quá trình xảy ra " + ex.Message;
                return errs;
            }
        }

        public EntitySingleResponse<HRM_TuyenDung_DeNghi> GetDeNghiTuyenDung(string aId, ApplicationDbContext aContext)
        {
            var result = new EntitySingleResponse<HRM_TuyenDung_DeNghi>();

            var model = aContext.HRM_TuyenDung_DeNghi.FirstOrDefault(x => x.IdGuid == aId);
            model.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayDeNghi);
            var listChiTiet = aContext.HRM_TuyenDung_DeNghi_ChiTiet.Where(x => x.IdTuyenDungDeNghi == model.IdGuid).ToList();
            var listDaoTao = aContext.HRM_dmTrinhDoDaoTao.ToList();
            var listThoiGianLamViec = aContext.HRM_dmThoiGianLamViec.ToList();

            foreach (var itemChiTiet in listChiTiet)
            {
                itemChiTiet.ThoiGianBatDauUnix = dungchung.DateTimeToUnixTimeStamp(itemChiTiet.ThoiGianBatDau);
                itemChiTiet.ThoiGianKetThucUnix = dungchung.DateTimeToUnixTimeStamp(itemChiTiet.ThoiGianKetThuc);

                itemChiTiet.TrinhDoDaoTao = listDaoTao.FirstOrDefault(x => x.IdGuid == itemChiTiet.IddmTrinhDoDaoTao);
                itemChiTiet.ThoiGianLamViec = listThoiGianLamViec.FirstOrDefault(x => x.IdGuid == itemChiTiet.IddmThoiGianLamViec);
            }

            model.lstChiTiet = listChiTiet;

            result.Item = model;
            return result;
        }

        public MessageReturn DeleteDeNghiTuyenDung(List<string> aListId, ApplicationDbContext aContext)
        {
            var listDeNghiTuyenDung = aContext.HRM_TuyenDung_DeNghi.Where(x => aListId.Contains(x.IdGuid)).ToList();
            var listIdDeNghiTuyenDung = listDeNghiTuyenDung.Select(x => x.IdGuid);
            var listDeNghiTuyenDungChiTiet = aContext.HRM_TuyenDung_DeNghi_ChiTiet.Where(x => listIdDeNghiTuyenDung.Contains(x.IdTuyenDungDeNghi)).ToList();
            FileUploadController FileUpload = new FileUploadController();

            foreach (var item in listDeNghiTuyenDung)
            {
                aContext.HRM_TuyenDung_DeNghi.Remove(item);
            }

            foreach (var item in listDeNghiTuyenDungChiTiet)
            {
                aContext.HRM_TuyenDung_DeNghi_ChiTiet.Remove(item);
            }

            aContext.SaveChanges();
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu đề nghị tuyển dụng thành công");
        }

        public MessageReturn ChuyenTiepDeNghiTuyenDung(HRM_TuyenDung_DeNghi model, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DeNghi pcuDatabase = db.HRM_TuyenDung_DeNghi.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }

                    string Error = FuncSetDeNghiTuyenDung(model, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, model.IdGuid);
                    }
                    var TrangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.DENGHITUYENDUNG.ToString(), model.IdTrangThai, model.IdGuid);
                    if (string.IsNullOrEmpty(TrangThai.IdTrangThai))
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, TrangThai.TenTrangThai);
                    }
                    model.IdTrangThai = TrangThai.IdTrangThai;
                    model.TenTrangThai = TrangThai.TenTrangThai;

                    if (TrangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                        model.isKetThuc = true;

                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public MessageReturn KhongDuyetDeNghiTuyenDung(HRM_TuyenDung_DeNghi QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DeNghi pcuDatabase = db.HRM_TuyenDung_DeNghi.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }

                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.DENGHITUYENDUNG.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                    }
                    QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    string Error = FuncSetDeNghiTuyenDung(QuyTrinh, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
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

        #endregion
        #region Tổng hợp Đề nghị tuyển dụng
        public string GetNextMaTongDeNghiTuyenDung(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_TuyenDung_DeNghi_TongHop.Select(x => x.SoPhieu), DefineData.MaTHDeNghiTuyenDung, digits: 3);

        public dynamic GetDanhSachDeNghiTuyenDungDaDuyet(long IdCongTyEOS, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db)
        {
            DateTime tuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
            DateTime denNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date;
            return db.HRM_TuyenDung_DeNghi
                .Where(a => a.IdCongTyEOS == IdCongTyEOS
                    && a.NgayDeNghi.Date >= tuNgay
                    && a.NgayDeNghi.Date <= denNgay
                    && a.isKetThuc)
                .ToList();
        }

        public dynamic SetQuyTrinhTongHopDeNghiTuyenDung(HRM_TuyenDung_DeNghi_TongHop model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    string err = FuncUpdateDeNghiTongHop(model, db);
                    if (err != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, err);
                    }
                    if (string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.DENGHITUYENDUNGTONGHOP.ToString(), model.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        }

                        model.IdTrangThai = trangThai.IdTrangThai;
                        model.TenTrangThai = trangThai.TenTrangThai;
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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

        public MessageReturn DeleteTongHopDeNghiTuyenDung(string Id, ApplicationDbContext db)
        {
            var item = db.HRM_TuyenDung_DeNghi_TongHop.Where(x => x.IdGuid == Id).FirstOrDefault();
            var itemChiTiet = db.HRM_TuyenDung_DeNghi_TongHopChiTiet.Where(x => x.IdQuyTrinhTongHop == Id).ToList();

            var itemChiTietDot = db.HRM_TuyenDung_DeNghi_TongHop_Dot.Where(x => x.IdQuyTrinhTongHop == Id).ToList();
            var listIdchitiet = itemChiTiet.Select(r => r.IdGuid).ToList();
            var itemNguon = db.HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon.Where(x => listIdchitiet.Contains(x.IdTongHopChiTiet)).ToList();

            db.HRM_TuyenDung_DeNghi_TongHop.Remove(item);
            db.HRM_TuyenDung_DeNghi_TongHopChiTiet.RemoveRange(itemChiTiet);
            db.HRM_TuyenDung_DeNghi_TongHop_Dot.RemoveRange(itemChiTietDot);
            db.HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon.RemoveRange(itemNguon);
            db.SaveChanges();


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu đề nghị tuyển dụng thành công");
        }

        public dynamic GetTongHopDeNghiQuyTrinhById(string Id, ApplicationDbContext db)
        {
            var model = db.HRM_TuyenDung_DeNghi_TongHop.Where(x => x.IdGuid == Id).FirstOrDefault();
            model.NgayTongHopDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTongHopDeNghi);
            var itemChiTiet = db.HRM_TuyenDung_DeNghi_TongHopChiTiet.Where(x => x.IdQuyTrinhTongHop == Id).ToList();

            var itemChiTietDot = db.HRM_TuyenDung_DeNghi_TongHop_Dot.Where(x => x.IdQuyTrinhTongHop == Id).ToList();
            var listIdchitiet = itemChiTiet.Select(r => r.IdGuid).ToList();
            var itemNguon = db.HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon.Where(x => listIdchitiet.Contains(x.IdTongHopChiTiet)).ToList();
            foreach (var item in itemChiTiet)
            {
                item.listNguonTuyenDung = itemNguon.Where(r => r.IdTongHopChiTiet == item.IdGuid).ToList();
                item.ThoiGianBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianBatDau);
                item.ThoiGianKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianKetThuc);
            }

            model.lstChiTiet = itemChiTiet;
            model.lstDotDeNghi = itemChiTietDot;
            model.lstFileUpload = GetListFileDinhKem(db, DefineData.eTable.HRM_TuyenDung_DeNghi_TongHop.ToString(), model.IdGuid);


            return model;
        }
        public dynamic GetDanhSachTopHophDeNghiTuyenDung(dynamic dyn, ApplicationDbContext db = null)
        {


            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_TuyenDung_DeNghi_TongHop> query = db.HRM_TuyenDung_DeNghi_TongHop;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));

                List<HRM_TuyenDung_DeNghi_TongHop> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;

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
        public MessageReturn ChuyenTiepTongHopDeNghiTuyenDung(HRM_TuyenDung_DeNghi_TongHop model, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DeNghi_TongHop pcuDatabase = db.HRM_TuyenDung_DeNghi_TongHop.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }

                    string Error = FuncUpdateDeNghiTongHop(model, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, model.IdGuid);
                    }
                    var TrangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.DENGHITUYENDUNGTONGHOP.ToString(), model.IdTrangThai, model.IdGuid);
                    if (string.IsNullOrEmpty(TrangThai.IdTrangThai))
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, TrangThai.TenTrangThai);
                    }
                    model.IdTrangThai = TrangThai.IdTrangThai;
                    model.TenTrangThai = TrangThai.TenTrangThai;
                    if (TrangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                        model.isKetThuc = true;

                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }

        public MessageReturn KhongTongHopDuyetDeNghiTuyenDung(HRM_TuyenDung_DeNghi_TongHop QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_TuyenDung_DeNghi_TongHop pcuDatabase = db.HRM_TuyenDung_DeNghi_TongHop.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                        }

                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.DENGHITUYENDUNGTONGHOP.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                    }
                    QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    string Error = FuncUpdateDeNghiTongHop(QuyTrinh, db);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
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


        public string FuncUpdateDeNghiTongHop(HRM_TuyenDung_DeNghi_TongHop model, ApplicationDbContext db)
        {
            try
            {
                model.NgayTongHopDeNghi = dungchung.UnixTimeStampToDateTime(model.NgayTongHopDeNghiUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_TuyenDung_DeNghi_TongHop.Add(model);
                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                    var listChiTietOlde = db.HRM_TuyenDung_DeNghi_TongHopChiTiet.Where(r => r.IdQuyTrinhTongHop == model.IdGuid).ToList();
                    db.HRM_TuyenDung_DeNghi_TongHopChiTiet.RemoveRange(listChiTietOlde);
                    var listIdKeHoach = db.HRM_TuyenDung_DeNghi_TongHop_Dot.Where(r => r.IdQuyTrinhTongHop == model.IdGuid).ToList();
                    db.HRM_TuyenDung_DeNghi_TongHop_Dot.RemoveRange(listIdKeHoach);
                }
                foreach (var item in model.lstDotDeNghi)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdQuyTrinhTongHop = model.IdGuid;
                    db.HRM_TuyenDung_DeNghi_TongHop_Dot.Add(item);
                }
                db.SaveChanges();

                var lstChiTiet = model.lstChiTiet;
                foreach (var itemChiTiet in lstChiTiet)
                {
                    itemChiTiet.IdQuyTrinhTongHop = model.IdGuid;

                    string IdChiTiet = itemChiTiet.IdGuid;
                    if (itemChiTiet.isDeleted == false)
                    {

                        itemChiTiet.ThoiGianBatDau = dungchung.UnixTimeStampToDateTime(itemChiTiet.ThoiGianBatDauUnix);
                        itemChiTiet.ThoiGianKetThuc = dungchung.UnixTimeStampToDateTime(itemChiTiet.ThoiGianKetThucUnix);
                        itemChiTiet.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                        if (itemChiTiet.listNguonTuyenDung != null)
                        {
                            foreach (var item in itemChiTiet.listNguonTuyenDung)
                            {
                                item.IdGuid = Guid.NewGuid().ToString();
                                item.IdTongHopChiTiet = itemChiTiet.IdGuid;
                                db.HRM_TuyenDung_DeNghi_TongHopChiTiet_Nguon.Add(item);
                            }
                        }
                    }
                    else
                    {
                        db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    }
                    if (model.lstFileUpload != null)
                    {
                        foreach (HRM_FileDinhKem FileDinhKem in model.lstFileUpload)
                        {
                            SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_TuyenDung_DeNghi_TongHop, model.IdGuid);
                        }
                    }
                }

                return "0";
            }
            catch (Exception ex)
            {

                string Ms = "Có lỗi xảy ra trong quá trình sử lý " + ex.Message;
                return Ms;
            }
        }
        public dynamic GetTongHopDeNghiTuyenDung(dynamic dyn, ApplicationDbContext db)
        {
            List<string> lstIdKeHoach = JsonConvert.DeserializeObject<List<string>>(dyn.lstIdDeNghi.ToString());

            var ChucDanhs = _CallWebService.GetDanhSachChucDanh(db);
            var TrinhDos = db.HRM_dmTrinhDoDaoTao.ToList();

            List<TuyenDungDeNghiHopChiTietModel> lstTongHop = db.HRM_TuyenDung_DeNghi_ChiTiet
                .Where(x => lstIdKeHoach.Contains(x.IdTuyenDungDeNghi))
                .GroupBy(x => new { x.IddmChucDanh, x.ThoiGianBatDau, x.ThoiGianKetThuc, x.IddmTrinhDoDaoTao })
                .Select(g => new TuyenDungDeNghiHopChiTietModel
                {
                    IdChucDanhEOS = g.Key.IddmChucDanh,
                    IddmTrinhDo = g.Key.IddmTrinhDoDaoTao,
                    ThoiGianBatDau = g.Key.ThoiGianBatDau,
                    ThoiGianKetThuc = g.Key.ThoiGianKetThuc,
                    MucLuong = g.Sum(x => x.MucLuong) / g.Count(),
                    ChiPhiTuyenDung = g.Sum(x => x.ChiPhiTuyenDung),
                    ThayThe = g.Sum(x => x.ThayThe),
                    BoSung = g.Sum(x => x.BoSung)
                })
                .ToList();

            foreach (var item in lstTongHop)
            {
                item.ThoiGianBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianBatDau);
                item.ThoiGianKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianKetThuc);
                item.TenChucDanh = ChucDanhs.Where(r => r.Id == item.IdChucDanhEOS).Select(r => r.Ten).FirstOrDefault();
                item.TrinhDo = TrinhDos.Where(r => r.IdGuid == item.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
            }
            return lstTongHop;
        }
        public dynamic GetDanhSachTongHopDeNghiTuyenDungDaDuyet(long IdCongTyEOS, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db)
        {
            DateTime tuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
            DateTime denNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date;
            return db.HRM_TuyenDung_DeNghi_TongHop
                .Where(a => a.IdCongTyEOS == IdCongTyEOS
                    && a.NgayTongHopDeNghi.Date >= tuNgay
                    && a.NgayTongHopDeNghi.Date <= denNgay
                    && a.isKetThuc)
                .ToList();
        }

        public dynamic GetDanhSachNhuCauTuyenDung(long IdCongTyEOS, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db)
        {
            DateTime tuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
            DateTime denNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date;
            return db.HRM_TuyenDung_DeNghi_TongHop
                .Where(a => a.IdCongTyEOS == IdCongTyEOS
                    && a.Created.Date >= tuNgay
                    && a.Created.Date <= denNgay
                    && a.isKetThuc)
                .ToList();
        }


        #endregion
    }
    public class TuyenDungDeNghiHopChiTietModel
    {


        public int BoSung { get; set; }
        public int ThayThe { get; set; }
        public int SoLuong { set; get; }
        public double MucLuong { set; get; }
        public double ChiPhiTuyenDung { set; get; }


        public string IddmTrinhDo { get; set; }
        public string TrinhDo { get; set; }

        public int ThoiGianLamDuKien { get; set; }
        public long IdBoPhanEOS { get; set; }
        public string TenBoPhan { set; get; }
        public long IdChucDanhEOS { get; set; }
        public string TenChucDanh { set; get; }
        public DateTime ThoiGianBatDau { get; set; }

        public double ThoiGianBatDauUnix { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }

        public double ThoiGianKetThucUnix { get; set; }
    }
}