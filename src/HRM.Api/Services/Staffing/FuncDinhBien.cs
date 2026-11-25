#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Helper;
using HRM.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services.DinhBien
{
    public class FuncDinhBien : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncDinhBien(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncDinhBien> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        #region DinhBien

        public dynamic GetListQuyTrinhDinhBien(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = itemTimKiem.CurrentPage ?? 1;
                int PageSize = itemTimKiem.PageSize ?? 25;
                int Nam = itemTimKiem.Nam ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 1;

                IQueryable<HRM_QuyTrinhDinhBien> query = db.HRM_QuyTrinhDinhBien;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoQuyTrinh.Contains(itemTimKiem.KeyWord));
                if (Nam > 0)
                    query = query.Where(dm => dm.Nam == Nam);

                List<HRM_QuyTrinhDinhBien> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, PageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;
                foreach (var item in itemList)
                    item.NgayApDungUnix = DateTimeToUnixTimeStamp(item.NgayApDung);

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

        public dynamic GetNextSoQuyTrinhDinhBien(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyTrinhDinhBien.Select(x => x.SoQuyTrinh), "QTDB"));

        public dynamic GetQuyTrinhDinhBien(string Id, ApplicationDbContext db)
        {
            HRM_QuyTrinhDinhBien model = db.HRM_QuyTrinhDinhBien.Find(Id);
            if (model == null) return null;
            model.listQuyTrinhDinhBienChiTiet = db.HRM_QuyTrinhDinhBien_ChiTiet.Where(r => r.IdQuyTrinh == Id).ToList();
            model.NgayApDungUnix = DateTimeToUnixTimeStamp(model.NgayApDung);
            return model;
        }

        public dynamic SetQuyTrinhDinhBien(HRM_QuyTrinhDinhBien model, ApplicationDbContext db)
            => ExecuteInTransaction(db, () =>
            {
                if (!UpdateQuyTrinhDinhBien(model, db))
                    return MessageReturn.Fail("Có lỗi xảy ra trong quá trình xử lý");

                if (string.IsNullOrEmpty(model.IdTrangThai))
                {
                    var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHDINHBIEN.ToString(), model.IdGuid);
                    if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                        return MessageReturn.Fail("Không tìm thấy trạng thái dữ liệu khi chuyển", DefineData.eErrorMessage.LoiLayDuLieu);

                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;
                    db.Entry(model).State = EntityState.Modified;
                }

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật dữ liệu thành công", model.IdGuid);
            });

        public bool UpdateQuyTrinhDinhBien(HRM_QuyTrinhDinhBien model, ApplicationDbContext db)
        {
            try
            {
                model.NgayApDung = UnixTimeStampToDateTime(model.NgayApDungUnix);
                if (model.IdGuid.IsEmpty())
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_QuyTrinhDinhBien.Add(model);
                }
                else
                {
                    var listChiTietOld = db.HRM_QuyTrinhDinhBien_ChiTiet.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
                    db.HRM_QuyTrinhDinhBien_ChiTiet.RemoveRange(listChiTietOld);
                    db.Entry(model).State = EntityState.Modified;
                }

                var lstNew = model.listQuyTrinhDinhBienChiTiet
                    .Where(item => !item.isDeleted)
                    .Select(item =>
                    {
                        item.IdQuyTrinh = model.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        return item;
                    })
                    .ToList();

                db.HRM_QuyTrinhDinhBien_ChiTiet.AddRange(lstNew);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public dynamic ChuyenTiepQuyTrinhDinhBien(ApplicationDbContext db, HRM_QuyTrinhDinhBien QuyTrinh)
            => ExecuteInTransaction(db, () =>
            {
                var pcuDatabase = db.HRM_QuyTrinhDinhBien.AsNoTracking().FirstOrDefault(x => x.IdGuid == QuyTrinh.IdGuid);
                if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai) && QuyTrinh.IdTrangThai != pcuDatabase?.IdTrangThai)
                    return MessageReturn.Fail("Phiếu đã được chuyển trước đó!");

                if (!UpdateQuyTrinhDinhBien(QuyTrinh, db))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                var trangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.QUYTRINHDINHBIEN.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                    return MessageReturn.Fail(trangThai.TenTrangThai, DefineData.eErrorMessage.LoiLayDuLieu);

                QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                if (trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc)
                    QuyTrinh.isKetThuc = true;
                db.Entry(QuyTrinh).State = EntityState.Modified;

                if (QuyTrinh.isKetThuc && !string.IsNullOrEmpty(QuyTrinh.IdQuyTrinh))
                {
                    var quyTrinhOld = db.HRM_QuyTrinhDinhBien.Find(QuyTrinh.IdQuyTrinh);
                    dynamic trangThaiHuy = GetTrangThaiHuy(db, DefineData.eAction.QUYTRINHDINHBIEN.ToString(), QuyTrinh.IdGuid);
                    quyTrinhOld.IdTrangThai = trangThaiHuy.IdTrangThai;
                    quyTrinhOld.TenTrangThai = trangThaiHuy.TenTrangThai;
                    db.Entry(quyTrinhOld).State = EntityState.Modified;
                    db.HRM_DinhBien.RemoveRange(db.HRM_DinhBien.Where(r => r.IdQuyTrinh == QuyTrinh.IdQuyTrinh));
                }

                db.SaveChanges();
                return MessageReturn.Success("Cập nhật dữ liệu thành công");
            });

        public MessageReturn KhongDuyetQuyTrinhDinhBien(ApplicationDbContext db, HRM_QuyTrinhDinhBien QuyTrinh)
            => ExecuteInTransaction(db, () =>
            {
                if (!UpdateQuyTrinhDinhBien(QuyTrinh, db))
                    return MessageReturn.Fail("Cập nhật dữ liệu không thành công");

                var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHDINHBIEN.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);
                if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                    return MessageReturn.Fail("Không tìm thấy trạng thái dữ liệu tuyển dụng khi chuyển", DefineData.eErrorMessage.LoiLayDuLieu);

                QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                db.Entry(QuyTrinh).State = EntityState.Modified;
                db.SaveChanges();
                return MessageReturn.Success();
            });

        public MessageReturn DeleteQuyTrinhDinhBien(ApplicationDbContext aContext, string Id)
        {
            if (aContext.currentUser.Id.IsEmpty())
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            if (Id == null)
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            if (Id.IsEmpty())
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id để xóa");

            var model = aContext.HRM_QuyTrinhDinhBien.FirstOrDefault(x => x.IdGuid == Id);
            if (model == null)
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");

            var listDetail = aContext.HRM_QuyTrinhDinhBien_ChiTiet.Where(x => x.IdQuyTrinh == model.IdGuid).ToList();
            aContext.HRM_QuyTrinhDinhBien_ChiTiet.RemoveRange(listDetail);
            aContext.Entry(model).State = EntityState.Deleted;
            aContext.SaveChanges();

            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa quy trình thành công");
        }

        #endregion

        public dynamic GetDinhBienByCoSoTheoNam(ApplicationDbContext db, int Nam, long IdCoSo)
        {
            var model = db.HRM_QuyTrinhDinhBien.FirstOrDefault(r => r.IdCoSoEOS == IdCoSo && r.Nam == Nam);
            if (model == null)
                return model;

            model.listQuyTrinhDinhBienChiTiet = db.HRM_QuyTrinhDinhBien_ChiTiet.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
            model.NgayApDungUnix = DateTimeToUnixTimeStamp(model.NgayApDung);
            return model;
        }

        public dynamic GetDinhBienBoPhanByChucDanh(ApplicationDbContext db, int Nam, long IdBoPhan, long IdChucDanh)
        {
            var LstBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            List<long> listIdPhongBan = DequyPhongBan(IdBoPhan, LstBoPhan);
            listIdPhongBan.Add(IdBoPhan);
            return db.HRM_DinhBien
                .Where(r => r.Nam == Nam && listIdPhongBan.Contains(r.IdCoSoEOS) && (IdChucDanh == 0 || r.IdChucDanhEOS == IdChucDanh))
                .ToList();
        }

        public dynamic GetDinhBienTheoNam(ApplicationDbContext db, int Nam, long IdCongTy, long IdCoSo)
        {
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var dinhBiens = db.HRM_DinhBien
                .Where(a => a.Nam == Nam && (IdCoSo == 0 || a.IdCoSoEOS == IdCoSo) && a.IdCongTyEOS == IdCongTy)
                .ToList();

            foreach (var item in dinhBiens)
            {
                item.TenCoSo = BoPhans.Where(r => r.Id == item.IdCoSoEOS).Select(r => r.TenBoPhan).FirstOrDefault();
            }

            return dinhBiens;
        }

        //public dynamic GetQuyLuongHienTaiByBoPhan(ApplicationDbContext db, int Nam, long IdBoPhan)
        //{

        //    List<BoPhan> LstBoPhan = _CallWebService.GetDanhSachBoPhan(db);


        //    List<long> listIdPhongBan = DequyPhongBan(IdBoPhan, LstBoPhan);
        //    listIdPhongBan.Add(IdBoPhan);

        //    var nhanvienBoPhanChucVu = db.HRM_Link_NhanVien_BoPhan_ChucVu.Where(r => (r.NgayKetThuc == null || (r.NgayKetThuc != null && r.NgayKetThuc.Value.Year > Nam)) 
        //                                                                                && listIdPhongBan.Contains(r.IdBoPhanEOS) && r.IdChucDanhEOS != 0).ToList();
        //    List<long> listChucDanh = nhanvienBoPhanChucVu.GroupBy(r => r.IdChucDanhEOS).Select(r => r.Key).ToList();
        //    List<string> listIdNhanVien = nhanvienBoPhanChucVu.Select(r => r.IdNhanVien).ToList();

        //    List<HRM_Luong_NhanVien> luong_NhanViens = db.HRM_Luong_NhanVien.Where(r => listIdNhanVien.Contains(r.IdNhanVien) 
        //                                    && (r.NgayHetHieuLuc == null || (r.NgayHetHieuLuc != null && r.NgayHetHieuLuc.Value.Year >= Nam)) && Nam >= r.NgayBatDauHieuLuc.Year).ToList();
        //    List<DinhBien_ChucDanh_PhongBanViewModel> dinhBiens = new List<DinhBien_ChucDanh_PhongBanViewModel>();
        //    foreach (var IdChucDanh in listChucDanh)
        //    {
        //        DinhBien_ChucDanh_PhongBanViewModel dinhBien = new DinhBien_ChucDanh_PhongBanViewModel();
        //        List<string> listIdNhanVienChucDanh = nhanvienBoPhanChucVu.Where(r => r.IdChucDanhEOS == IdChucDanh).Select(r => r.IdNhanVien).ToList();
        //        List<double> luongChucDanhs = new List<double>();
        //        foreach (var Idnhanvien in listIdNhanVienChucDanh)
        //        {
        //            double Luong = 0;
        //            HRM_Luong_NhanVien nhanVienLuong = luong_NhanViens.Where(r => r.IdNhanVien == Idnhanvien).OrderBy(r => r.NgayBatDauHieuLuc).FirstOrDefault();
        //            if (nhanVienLuong == null)
        //                continue;
        //            if (nhanVienLuong.LoaiLuong == 1)
        //            {
        //                Luong = nhanVienLuong.LuongHachToan;
        //            }
        //            if (nhanVienLuong.LoaiLuong == 2)
        //            {
        //                Luong = nhanVienLuong.LuongHachToan*26;
        //            }
        //            if (nhanVienLuong.LoaiLuong == 3)
        //            {
        //                Luong = nhanVienLuong.LuongHachToan * 26*8;
        //            }
        //            if(nhanVienLuong.KieuLuong==1)
        //            {
        //                Luong = Luong + Luong * 0.105;
        //            }
        //            luongChucDanhs.Add(Luong);
        //        }
        //        dinhBien.IdChucDanhEOS = IdChucDanh;
        //        if (luongChucDanhs.Count > 0)
        //        {
        //            dinhBien.DinhBienHienTai = luongChucDanhs.Count();
        //            dinhBien.LuongHienTai = luongChucDanhs.Sum() / dinhBien.DinhBienHienTai;
        //            dinhBien.TongLuongHienTai = luongChucDanhs.Sum();
        //        }
        //        dinhBiens.Add(dinhBien);
        //    }
        //    double TongLuongHienTai = dinhBiens.Sum(r => r.TongLuongHienTai);
        //    dynamic dyn = new DynamicObject();
        //    dyn.TongLuongHienTai = TongLuongHienTai;
        //    dyn.dinhBiens = dinhBiens;
        //    return dyn;
        //}
    }

    public class DinhBien_ChucDanh_PhongBanViewModel
    {
        public long IdChucDanhEOS { set; get; }
        public double LuongHienTai { set; get; }
        public int DinhBienHienTai { set; get; }
        public double TongLuongHienTai { set; get; }
    }
}
