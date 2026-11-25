#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
using HRM.Api.Models.NhanVienModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Services.BangLuong
{


        public class FuncQuyetToanNghiBu : FuncBase
    {
        public FuncQuyetToanNghiBu(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncQuyetToanNghiBu> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        public string GetNextMaQuyetToanNghiPhep(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_QuyetToanPhepNam.Select(x => x.SoPhieu), DefineData.MAQUYETTOANPHEP, digits: 3);
        public dynamic GetDanhSachNhanVienQuyetToanPhep(long IdCongTyEOS, int Nam, ApplicationDbContext db)
        {
            var lstNhanviens = (from a in db.HRM_NhanVien
                                join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
                                where b.isKetThuc == true && a.IdCongTyEOS == IdCongTyEOS && a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() && b.isHuongPhucLoi == true /*&& b.isPhuLuc == false && b.isQuyetToanPhepNam == true*/
                                select new
                                {
                                    IdNhanVien = a.IdGuid,
                                    HoTen = a.TenDayDu,
                                    a.MaNhanVien,
                                    a.NgayBatDauLamViec,
                                    b.IdHopDong,
                                    a.SoNamCongThemNgayPhep,
                                    a.NamBatDauCongPhep,
                                    b.SoNgayNghiPhep,
                                    a.IdNhanVienEOS,
                                    b.NgayHieuLuc,
                                    b.SoNgayNghiPhepThamNien,
                                    a.IdChucVu,
                                    a.IdBoPhanEOS

                                }).ToList();
            List<string> listIdNhanVien = lstNhanviens.Select(r => r.IdNhanVien).Distinct().ToList();
            var listIdEOSNhanVien = lstNhanviens.Select(r => r.IdNhanVienEOS).Distinct().ToList();
            var listHopDong = db.HRM_HopDong.Where(r => listIdNhanVien.Contains(r.IdNhanVien) && r.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && r.isHuongPhucLoi == true && r.isKetThuc == true).ToList();

            var lstHinhThucNghi = db.HRM_dmHinhThucNghi.ToList();
            List<string> IdHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(r => r.Ma == DefineData.eKyHieuChamCong.NP.ToString() || r.Ma == DefineData.eMaNghiPhep.PN.ToString()).Select(a => a.Id).Distinct().ToList();

            //string IdTrangThaiCuoi = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString());
            var listSoNgaynghi = (from a in db.HRM_TheoDoiXinNghiPhep
                                  where listIdNhanVien.Contains(a.IdNhanVien) && IdHinhThucNghi.Contains(a.IdHinhThucNghi) && a.NgayNghi.Year == Nam
                                  select new { a.ThoiGianNghi, a.IdNhanVien }).ToList();

            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var ChucVus = _CallWebService.GetDanhSachChucVu(db);
            List<NhanVienQuyetToanPhepViewModel> listItem = new List<NhanVienQuyetToanPhepViewModel>();
            foreach (var item in listIdNhanVien)
            {
                var NhanVien = lstNhanviens.Where(r => r.IdNhanVien == item).FirstOrDefault();
                var HopDong = listHopDong.Where(r => r.IdNhanVien == item).OrderByDescending(r => r.Modified).FirstOrDefault();
                var listSoNgaynghiNhanVien = listSoNgaynghi.Where(r => r.IdNhanVien == NhanVien.IdNhanVien).ToList();

                double SoNgayPhep = 0;
                if (NhanVien.NgayBatDauLamViec.Value.Year == Nam)
                {
                    SoNgayPhep = NhanVien.SoNgayNghiPhep - NhanVien.NgayBatDauLamViec.Value.Month;
                    if (SoNgayPhep < 0)
                        SoNgayPhep = 0;
                }
                else
                {
                    SoNgayPhep = (NhanVien.SoNgayNghiPhep) + (NhanVien.SoNgayNghiPhepThamNien);
                    //int SoNam = Nam - NhanVien.NamBatDauCongPhep;
                    //if (NhanVien.SoNamCongThemNgayPhep > 0)
                    //{
                    //    //double trouble = Nam / NhanVien.SoNamCongThemNgayPhep;
                    //    //double dbl = System.Math.Truncate(trouble);
                    //    SoNgayPhep = NhanVien.SoNgayNghiPhep;
                    //}
                    //else
                    //{
                    //    SoNgayPhep = NhanVien.SoNgayNghiPhep;
                    //}
                }
                if (HopDong == null)
                    continue;
                NhanVienQuyetToanPhepViewModel nhanVienItem = new NhanVienQuyetToanPhepViewModel
                {
                    IdNhanVien = NhanVien.IdNhanVien,
                    IdHopDong = HopDong.IdGuid,
                    IdChucVuEOS = NhanVien.IdChucVu,
                    IdBoPhanEOS = NhanVien.IdBoPhanEOS,
                    NgayBatDauLamViec = NhanVien.NgayBatDauLamViec.Value,
                    MaNhanVien = NhanVien.MaNhanVien,
                    TenNhanVien = NhanVien.HoTen,
                    SoNamCongThemNgayPhep = NhanVien.SoNamCongThemNgayPhep,
                    SoNgayPhep = SoNgayPhep,
                    SoNgayDaNghi = listSoNgaynghiNhanVien.Sum(r => r.ThoiGianNghi),
                    NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(NhanVien.NgayBatDauLamViec),
                };
                nhanVienItem.SoNgayPhepConLai = nhanVienItem.SoNgayPhep - nhanVienItem.SoNgayDaNghi;
                if (nhanVienItem.SoNgayPhepConLai < 0)
                    nhanVienItem.SoNgayPhepConLai = 0;
                nhanVienItem.TenBoPhan = BoPhans.Where(r => r.Id == nhanVienItem.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                nhanVienItem.TenChucVu = ChucVus.Where(r => r.Id == nhanVienItem.IdChucVuEOS).Select(r => r.TenChucVu).FirstOrDefault();
                listItem.Add(nhanVienItem);
            }

            return listItem;
        }

        public dynamic GetDanhSachQuyetToanNghiPhep(dynamic dyn, ApplicationDbContext db = null)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = dyn.KeyWord;
                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int PageSize = itemTimKiem.PageSize ?? 25;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_QuyetToanPhepNam> query = db.HRM_QuyetToanPhepNam;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));
                if (itemTimKiem.Nam > 0)
                    query = query.Where(dm => dm.Nam == itemTimKiem.Nam.Value);
                if (itemTimKiem.IdCongTyEOS > 0)
                    query = query.Where(dm => dm.IdCongTyEOS == itemTimKiem.IdCongTyEOS.Value);

                List<HRM_QuyetToanPhepNam> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;

                foreach (var item in itemList)
                {
                    item.NgayQuyetToanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayQuyetToan);

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
        public dynamic SetQuyetToanNghiPhep(HRM_QuyetToanPhepNam model, ApplicationDbContext db)

        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string Update = FuncQuyetToanNghiPhep(model, db);
                    if (Update != "0")
                    {

                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, Update);
                    }
                    if (model.IdTrangThai == null || model.IdTrangThai == "")
                    {

                        // DI: use inherited _CallWebService
                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHQUYETTOANPHEP.ToString(), model.IdGuid);
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

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật quyết toán nghỉ phép thành công", model.IdGuid);
                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();

                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }

            }
        }


        public MessageReturn ChuyenTiepQuyetToanNghiPhep(HRM_QuyetToanPhepNam model, ApplicationDbContext db)
        {


            using (var ts = db.Database.BeginTransaction())
            {

                try
                {
                    HRM_QuyetToanPhepNam pcuDatabase = db.HRM_QuyetToanPhepNam.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(model.IdTrangThai))
                    {
                        if (model.IdTrangThai != pcuDatabase.IdTrangThai)
                        {
                            ts.Rollback();

                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");

                        }

                    }
                    string Update = FuncQuyetToanNghiPhep(model, db);
                    if (Update != "0")
                    {

                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, Update);
                    }
                    var trangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.QUYTRINHQUYETTOANPHEP.ToString(), model.IdTrangThai, model.IdGuid);
                    //   var trangThai = GetTrangThaiChuyenTiep(smartdb, db, model.IdTrangThai, model.IdGuid, db.IdUser);
                    if (trangThai == null)
                    {
                        ts.Rollback();

                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");

                    }

                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;
                    if (trangThai.LoaiTrangThai == 2)
                        model.isKetThuc = true;
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

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

        public MessageReturn KhongDuyetQuyetToanNghiPhep(HRM_QuyetToanPhepNam model, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {

                try
                {
                    string Update = FuncQuyetToanNghiPhep(model, db);
                    if (Update != "0")
                    {

                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, Update);
                    }
                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHQUYETTOANPHEP.ToString(), model.IdTrangThai, model.IdGuid);

                    if (trangThai == null)
                    {

                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu tuyển dụng khi chuyển");
                    }

                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    db.SaveChanges();


                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }

            }
        }

        public dynamic GetQuyetToanNghiPhepById(string Id, ApplicationDbContext db)
        {
            var model = db.HRM_QuyetToanPhepNam.Find(Id);
            if (model == null) return null;
            model.NgayQuyetToanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayQuyetToan);
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var ChucVus = _CallWebService.GetDanhSachChucVu(db);
            var listNhanVien = db.HRM_QuyetToanPhepNam_NhanVien.Where(r => r.IdQuyetToanPhepNam == Id).ToList();
            foreach (var item in listNhanVien)
            {
                item.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDauLamViec);
                item.TenBoPhan = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                item.TenChucVu = ChucVus.Where(r => r.Id == item.IdChucVuEOS).Select(r => r.TenChucVu).FirstOrDefault();
                if (item.TrangThai == 0)
                    item.TenTrangThai = "Chưa quyết toán";
                else
                    item.TenTrangThai = "Đã quyết toán";
            }
            model.listNhanVien = listNhanVien;
            return model;
        }
        public dynamic DeleteQuyetToanNghiPhep(string Id, ApplicationDbContext db)
        {
            try
            {
                var model = db.HRM_QuyetToanPhepNam.Find(Id);

                var listNhanVien = db.HRM_QuyetToanPhepNam_NhanVien.Where(r => r.IdQuyetToanPhepNam == Id).ToList();
                db.HRM_QuyetToanPhepNam.Remove(model);
                db.HRM_QuyetToanPhepNam_NhanVien.RemoveRange(listNhanVien);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công");
            }
            catch (Exception ex)
            {


                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public string FuncQuyetToanNghiPhep(HRM_QuyetToanPhepNam model, ApplicationDbContext db)
        {
            try
            {
                model.NgayQuyetToan = dungchung.UnixTimeStampToDateTime(model.NgayQuyetToanUnix);
                if (model.IdGuid == "" || model.IdGuid == null)
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_QuyetToanPhepNam.Add(model);
                    foreach (var item in model.listNhanVien)
                    {

                        if (item.isXoa == false)
                        {
                            item.Nam = model.Nam;
                            item.IdGuid = Guid.NewGuid().ToString(); item.IdQuyetToanPhepNam = model.IdGuid;
                            db.HRM_QuyetToanPhepNam_NhanVien.Add(item);
                        }

                    }
                }
                else
                {

                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    foreach (var item in model.listNhanVien)
                    {
                        item.Nam = model.Nam;
                        item.IdQuyetToanPhepNam = model.IdGuid;
                        if (item.isXoa == false)
                        {
                            if (model.IdGuid == "" || model.IdGuid == null)
                            {
                                item.IdGuid = Guid.NewGuid().ToString();

                                db.HRM_QuyetToanPhepNam_NhanVien.Add(item);
                            }
                            else
                            {
                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }

                        }
                        else
                        {
                            if (item.IdGuid != "" && item.IdGuid != null)
                            {


                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                            }
                        }

                    }
                }

                return "0";

            }
            catch (System.Exception ex)
            {

                string returnMes = "Cập nhật quyết toán phép không thành công " + ex.Message;
                return returnMes;

            }
        }
    }
}