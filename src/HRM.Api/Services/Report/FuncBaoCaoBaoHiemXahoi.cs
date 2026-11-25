#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using HRM.Api.Services.NhanVien;
using HRM.Api.Entity;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;






//using HRM.Api.Models.SmartEOEModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace HRM.Api.Services.BaoCao
{
    public class FuncBaoCaoBaoHiemXahoi : FuncBase
    {
        public FuncBaoCaoBaoHiemXahoi(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBaoCaoBaoHiemXahoi> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        #region QUY trình xác nhận tăng giảm bảo hiểm xã hội
        #region xác nhận làm ngoài giờ

        public dynamic GetNextMaQuyTrinhXacNhanTangGiamBaoHiem(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.Select(x => x.SoQuyTrinh), DefineData.MaQuyTrinhXacNhanTangGiamBaoHiem));
        public dynamic GetListQuyTrinhXacNhanTangGiamBaoHiem(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IEnumerable<HRM_QuyTrinhXacNhanTangGiamBaoHiem> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_QuyTrinhXacNhanTangGiamBaoHiem
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhXacNhanTangGiamBaoHiem
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                {
                  
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())); 
                }
                if (TimKiem.IdCongTy != null && TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == TimKiem.IdCongTy);

                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> CongTys = _CallWebService.GetDanhSachDuAn(db);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                List<string> listidNv = itemList.Select(a => a.IdNguoiPheDuyet).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listidNv.Contains(a.IdGuid)).ToList();
                foreach (HRM_QuyTrinhXacNhanTangGiamBaoHiem item in itemList)
                {

                    if (listNhanVien != null && !string.IsNullOrEmpty(item.IdNguoiPheDuyet))
                        item.TenNguoiPheDuyet = listNhanVien.Where(a => a.IdGuid == item.IdNguoiPheDuyet).Select(a => a.TenDayDu).FirstOrDefault();
                    BoPhan duAn = CongTys.Find(x => x.Id == item.IdCongTy);
                    if (duAn != null)
                        item.TenCongTy = duAn.TenBoPhan;
                    duAn = BoPhans.Find(x => x.Id == item.IdBoPhan);
                    if (duAn != null)
                        item.TenBoPhan = duAn.TenBoPhan;

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
        public dynamic SetQuyTrinhXacNhanTangGiamBaoHiem(HRM_QuyTrinhXacNhanTangGiamBaoHiem model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    var QuyTrinhdb = db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncQuyTrinhXacNhanTangGiamBaoHiem(model, db, err, eQuyTrinh_LoaiHam.Set);
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

        public dynamic ChuyenTiepQuyTrinhXacNhanTangGiamBaoHiem(HRM_QuyTrinhXacNhanTangGiamBaoHiem QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_QuyTrinhXacNhanTangGiamBaoHiem pcuDatabase = db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    QuyTrinh.NoiDung = QuyTrinh.GhiChu;
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    bool Error = FuncQuyTrinhXacNhanTangGiamBaoHiem(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
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
                        List<string> listIdNhanVien = QuyTrinh.listTang.Select(a => a.IdNhanVien).ToList();
                        listIdNhanVien.AddRange(QuyTrinh.listGiam.Select(a => a.IdNhanVien).ToList());
                        var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                        List<HRM_Luong_NhanVien> listAdd = new List<HRM_Luong_NhanVien>();
                        var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                        foreach (var item in QuyTrinh.listTang)
                        {
                            item.ThoiGianDong = new DateTime(item.ThoiGianDong.Value.Year, item.ThoiGianDong.Value.Month, 1, 0, 0, 0);
                            var Luong = listLuong.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien && a.LuongHachToan == item.MucDong);
                            //if(Luong.NgayBatDauHieuLuc)
                            if (Luong != null)
                            {
                                Luong.LuongHachToan = item.MucDongMoi;
                                db.Entry(Luong).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                            var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                            if (string.IsNullOrEmpty(NhanVien.SoBaoHiemXaHoi) && !string.IsNullOrEmpty(item.SoBHXH))
                            {
                                NhanVien.SoBaoHiemXaHoi = item.SoBHXH;
                                db.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                            LogLichSuNhanVien(db, item.IdNhanVien, QuyTrinh.IdGuid, "HRM_QuyTrinhXacNhanTangGiamBaoHiem", DefineData.eAction.XACNHANTANGGIAMBAOHIEM.ToString(), QuyTrinh.NgayBatDauUnix);

                        }
                        foreach (var item in QuyTrinh.listGiam)
                        {
                            var Luong = listLuong.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien && a.LuongHachToan == item.MucDong);
                            //if(Luong.NgayBatDauHieuLuc)
                            if (Luong != null)
                            {
                                Luong.LuongHachToan = item.MucDong;
                                db.Entry(Luong).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                            var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                            if (string.IsNullOrEmpty(NhanVien.SoBaoHiemXaHoi) && !string.IsNullOrEmpty(item.SoBHXH))
                            {
                                NhanVien.SoBaoHiemXaHoi = item.SoBHXH;
                                db.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                            LogLichSuNhanVien(db, item.IdNhanVien, QuyTrinh.IdGuid, "HRM_QuyTrinhXacNhanTangGiamBaoHiem", DefineData.eAction.XACNHANTANGGIAMBAOHIEM.ToString(), QuyTrinh.NgayBatDauUnix);

                        }

                        db.SaveChanges();
                    }
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
        public dynamic KhongDuyetQuyTrinhXacNhanTangGiamBaoHiem(HRM_QuyTrinhXacNhanTangGiamBaoHiem QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhXacNhanTangGiamBaoHiem pcuDatabase = db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = FuncQuyTrinhXacNhanTangGiamBaoHiem(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
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
        public dynamic GetByIdQuyTrinhXacNhanTangGiamBaoHiem(string Id, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                HRM_QuyTrinhXacNhanTangGiamBaoHiem model = db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.Find(Id);
                if (model.NgayBatDau != null)
                    model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
                if (model.NgayKetThuc != null)
                    model.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThuc);
                if (model.NgayPheDuyet != null)
                    model.NgayPheDuyetUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayPheDuyet);
                model.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_QuyTrinhXacNhanTangGiamBaoHiem.ToString(), Id);
                model.listTang = db.HRM_TangBaoHiem.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                model.listGiam = db.HRM_GiamBaoHiem.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                List<string> listIdNhanVien = new List<string>();
                if (model.listTang != null && model.listTang.Count > 0)
                    listIdNhanVien = model.listTang.Select(a => a.IdNhanVien).Distinct().ToList();
                if (model.listGiam != null && model.listGiam.Count > 0)
                    listIdNhanVien.AddRange(model.listGiam.Select(a => a.IdNhanVien).Distinct().ToList());
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var listNhanVien_DuAn = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                if (model.listTang != null && model.listTang.Count > 0)
                {
                    foreach (var tang in model.listTang)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == tang.IdNhanVien);
                        tang.MaNhanVien = nhanVien.MaNhanVien;
                        tang.TenNhanVien = nhanVien.TenDayDu;
                        //tang.SoBHXH = nhanVien.SoBaoHiemXaHoi;
                        tang.TenBoPhan = listBoPhan.Where(a => a.Id == nhanVien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                        if (listNhanVien_DuAn != null && listNhanVien_DuAn.Count > 0)
                        {
                            var IdDuAns = listNhanVien_DuAn.Where(a => a.IdQuyTrinh == nhanVien.IdGuid).Select(a => a.IdBoPhan).ToList();
                            var DuAns = listDuAn.Where(a => IdDuAns.Contains(a.Id)).Select(a => a.TenBoPhan).Distinct().ToList();
                            tang.TenCongTy = string.Join(", ", DuAns);
                        }
                        if (tang.ThoiGianDong != null)
                            tang.ThoiGianDongUnix = dungchung.DateTimeToUnixTimeStamp(tang.ThoiGianDong);
                    }
                }
                if (model.listGiam != null && model.listGiam.Count > 0)
                {
                    foreach (var Giam in model.listGiam)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == Giam.IdNhanVien);
                        Giam.MaNhanVien = nhanVien.MaNhanVien;
                        Giam.TenNhanVien = nhanVien.TenDayDu;
                        //Giam.SoBHXH = nhanVien.SoBaoHiemXaHoi;
                        Giam.TenBoPhan = listBoPhan.Where(a => a.Id == nhanVien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                        if (listNhanVien_DuAn != null && listNhanVien_DuAn.Count > 0)
                        {
                            var IdDuAns = listNhanVien_DuAn.Where(a => a.IdQuyTrinh == nhanVien.IdGuid).Select(a => a.IdBoPhan).ToList();
                            var DuAns = listDuAn.Where(a => IdDuAns.Contains(a.Id)).Select(a => a.TenBoPhan).Distinct().ToList();
                            Giam.TenCongTy = string.Join(", ", DuAns);
                        }
                        if (Giam.NgayGiam != null)
                            Giam.NgayGiamUnix = dungchung.DateTimeToUnixTimeStamp(Giam.NgayGiam);
                        if (Giam.NgayKetThuc != null)
                            Giam.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(Giam.NgayKetThuc);

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
        public dynamic GetDanhSachLoaiTangBaoHiem()
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                List<DanhMuc> list = new List<DanhMuc>();
                list.Add(new DanhMuc
                {
                    Ma = eLoaiBaoHiem.TangMoi.ToString(),
                    Ten = DefineData.GetLoaiTangGiamBH(eLoaiBaoHiem.TangMoi.ToString())
                });
                list.Add(new DanhMuc
                {
                    Ma = eLoaiBaoHiem.TangLaiSauThaiSan.ToString(),
                    Ten = DefineData.GetLoaiTangGiamBH(eLoaiBaoHiem.TangLaiSauThaiSan.ToString())
                });
                list.Add(new DanhMuc
                {
                    Ma = eLoaiBaoHiem.TangSauNghiKhongLuong.ToString(),
                    Ten = DefineData.GetLoaiTangGiamBH(eLoaiBaoHiem.TangSauNghiKhongLuong.ToString())
                });

                err.Data = list;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }
        public dynamic GetDanhSachLoaiGiamBaoHiem()
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                List<DanhMuc> list = new List<DanhMuc>();
                list.Add(new DanhMuc
                {
                    Ma = eLoaiBaoHiem.GiamThaiSan.ToString(),
                    Ten = DefineData.GetLoaiTangGiamBH(eLoaiBaoHiem.GiamThaiSan.ToString())
                });
                list.Add(new DanhMuc
                {
                    Ma = eLoaiBaoHiem.GiamDoNghiViec.ToString(),
                    Ten = DefineData.GetLoaiTangGiamBH(eLoaiBaoHiem.GiamDoNghiViec.ToString())
                });
                list.Add(new DanhMuc
                {
                    Ma = eLoaiBaoHiem.GiamKhongLuong.ToString(),
                    Ten = DefineData.GetLoaiTangGiamBH(eLoaiBaoHiem.GiamKhongLuong.ToString())
                });

                err.Data = list;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }

        public dynamic KhoiTaoQuyTrinhXachNhanTangGiamBaoHiem(ApplicationDbContext _dbContext, long IdCongTy, long IdBoPhan, double TuNgay, double DenNgay)
        {
            HRM_QuyTrinhXacNhanTangGiamBaoHiem result = new HRM_QuyTrinhXacNhanTangGiamBaoHiem();
            DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgay);
            DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgay);
            var listGiam = (from a in _dbContext.HRM_GiamBaoHiem
                            where a.isKetThuc == false && string.IsNullOrEmpty(a.IdQuyTrinh) && a.NgayGiam >= dTuNgay && a.NgayGiam <= dDenNgay
                            select a
                            );
            var listTang = (from a in _dbContext.HRM_TangBaoHiem
                            where a.isKetThuc == false && string.IsNullOrEmpty(a.IdQuyTrinh) && a.ThoiGianDong >= dTuNgay && a.ThoiGianDong <= dDenNgay
                            select a
                           ).Distinct();
            if (listGiam != null && IdCongTy > 0)
            {
                var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == IdCongTy).Select(a => a.IdQuyTrinh).Distinct().ToList();
                listGiam = listGiam.Where(a => listIdNhanVien.Contains(a.IdNhanVien));
                listTang = listTang.Where(a => listIdNhanVien.Contains(a.IdNhanVien));
            }
            List<string> listId = listGiam.Select(a => a.IdNhanVien).Distinct().ToList();
            listId.AddRange(listTang.Select(a => a.IdNhanVien).Distinct().ToList());
            var ListNhanVien = _dbContext.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).Distinct().ToList();
            if (ListNhanVien != null && ListNhanVien.Count > 0 && IdBoPhan > 0)
            {
                ListNhanVien = ListNhanVien.Where(a => a.IdBoPhanEOS == IdBoPhan).ToList();
                listId = ListNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                listGiam = listGiam.Where(a => listId.Contains(a.IdNhanVien));
                listTang = listTang.Where(a => listId.Contains(a.IdNhanVien));
            }
            result.listGiam = listGiam.ToList();
            result.listTang = listTang.ToList();
            var listCongTy = _CallWebService.GetDanhSachDuAn(_dbContext);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(_dbContext);
            var listGiayTo = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => listId.Contains(a.IdNhanVien) && (a.Loai == 1 || a.Loai == 2) && a.isApDung == true).ToList();

            if (result.listTang != null && result.listTang.Count > 0)
            {
                foreach (var tang in result.listTang)
                {

                    var NhanVien = ListNhanVien.FirstOrDefault(a => a.IdGuid == tang.IdNhanVien);
                    tang.MaNhanVien = NhanVien.MaNhanVien;
                    tang.TenNhanVien = NhanVien.TenDayDu;
                    tang.SoBHXH = NhanVien.SoBaoHiemXaHoi;
                    //tang.TenCongTy = listCongTy.Where(q => q.Id == tang.idc).Select(q => q.TenBoPhan).FirstOrDefault();
                    tang.TenBoPhan = listBoPhan.Where(q => q.Id == NhanVien.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();

                }
            }
            if (result.listGiam != null && result.listGiam.Count > 0)
            {
                foreach (var Giam in result.listGiam)
                {
                    var NhanVien = ListNhanVien.FirstOrDefault(a => a.IdGuid == Giam.IdNhanVien);
                    Giam.MaNhanVien = NhanVien.MaNhanVien;
                    Giam.TenNhanVien = NhanVien.TenDayDu;
                    Giam.SoBHXH = NhanVien.SoBaoHiemXaHoi;
                    //tang.TenCongTy = listCongTy.Where(q => q.Id == tang.idc).Select(q => q.TenBoPhan).FirstOrDefault();
                    Giam.TenBoPhan = listBoPhan.Where(q => q.Id == NhanVien.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                }
            }
            return result;
        }
        public dynamic DeleteQuyTrinhXacNhanTangGiamBaoHiem(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_QuyTrinhXacNhanTangGiamBaoHiem model = db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.Find(Id);

                model.listTang = db.HRM_TangBaoHiem.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                model.listGiam = db.HRM_GiamBaoHiem.Where(r => r.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                if (model.listTang != null && model.listTang.Count > 0)
                {
                    foreach (var tang in model.listTang)
                    {

                        if (tang.isChinhSua == false)
                        {
                            tang.IdQuyTrinh = "";
                            db.Entry(tang).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        else
                        {
                            db.Entry(tang).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                        }
                    }
                }
                if (model.listGiam != null && model.listGiam.Count > 0)
                {
                    foreach (var Giam in model.listGiam)
                    {
                        if (Giam.isChinhSua == false)
                        {
                            Giam.IdQuyTrinh = "";
                            db.Entry(Giam).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        else
                        {
                            db.Entry(Giam).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                        }
                    }
                }
                db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.Remove(model);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public bool FuncQuyTrinhXacNhanTangGiamBaoHiem(HRM_QuyTrinhXacNhanTangGiamBaoHiem model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (model.NgayKetThucUnix > 0)
                    model.NgayKetThuc = dungchung.UnixTimeStampToDateTime(model.NgayKetThucUnix);
                if (model.NgayKetThucUnix > 0)
                    model.NgayKetThuc = dungchung.UnixTimeStampToDateTime(model.NgayKetThucUnix);
                if (model.NgayPheDuyetUnix > 0)
                    model.NgayPheDuyet = dungchung.UnixTimeStampToDateTime(model.NgayPheDuyetUnix);

                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    //model.IdGuid = Guid.NewGuid().ToString();
                    if (string.IsNullOrEmpty(model.eAction))
                        model.eAction = DefineData.eAction.XACNHANTANGGIAMBAOHIEM.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_QuyTrinhXacNhanTangGiamBaoHiem.Add(model);


                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var listTang = db.HRM_TangBaoHiem.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                db.HRM_TangBaoHiem.RemoveRange(listTang);
                var HRM_GiamBaoHiem = db.HRM_GiamBaoHiem.Where(r => r.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                db.HRM_GiamBaoHiem.RemoveRange(HRM_GiamBaoHiem);

                if (model.listTang != null && model.listTang.Count > 0)
                {
                    int Stt = 1;
                    foreach (var tang in model.listTang)
                    {

                        if (tang.ThoiGianDongUnix > 0)
                            tang.ThoiGianDong = dungchung.UnixTimeStampToDateTime(tang.ThoiGianDongUnix);
                        tang.SapXep = Stt;
                        tang.isKetThuc = model.isKetThuc;
                        tang.IdQuyTrinh = model.IdGuid;
                        tang.NgayPheDuyet = model.NgayPheDuyet;
                        tang.IdNguoiPheDuyet = model.IdNguoiPheDuyet;
                        if (string.IsNullOrEmpty(tang.IdGuid))
                        {
                            tang.IdGuid = Guid.NewGuid().ToString();

                        }
                        db.Entry(tang).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        //else
                        //    db.Entry(tang).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        Stt++;
                    }
                }
                if (model.listGiam != null && model.listGiam.Count > 0)
                {
                    int Stt = 1;
                    foreach (var Giam in model.listGiam)
                    {

                        if (Giam.NgayGiamUnix > 0)
                            Giam.NgayGiam = dungchung.UnixTimeStampToDateTime(Giam.NgayGiamUnix);
                        if (Giam.NgayKetThucUnix > 0)
                            Giam.NgayKetThuc = dungchung.UnixTimeStampToDateTime(Giam.NgayKetThucUnix);
                        Giam.SapXep = Stt;
                        Giam.isKetThuc = model.isKetThuc;
                        Giam.NgayPheDuyet = model.NgayPheDuyet;
                        Giam.IdNguoiPheDuyet = model.IdNguoiPheDuyet;
                        Giam.IdQuyTrinh = model.IdGuid;
                        if (string.IsNullOrEmpty(Giam.IdGuid))
                            //{
                            Giam.IdGuid = Guid.NewGuid().ToString();
                        db.Entry(Giam).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        //}
                        //else
                        //    db.Entry(Giam).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        Stt++;
                    }
                }

                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_QuyTrinhXacNhanTangGiamBaoHiem, model.IdGuid);
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

        #endregion
        #endregion
        public dynamic GetListGiamBaoHiemXaHoi(dynamic dyn, ApplicationDbContext _dbContext)
        {
            try
            {
                var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                int nCurrentPage = TimKiem.CurrentPage ?? 1;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                int monthFilter = TimKiem.Thang ?? DateTime.Now.Month;
                var result = (from a in _dbContext.HRM_GiamBaoHiem
                              join b in _dbContext.HRM_NhanVien on a.IdNhanVien equals b.IdGuid
                              join c in _dbContext.HRM_Luong_NhanVien on a.IdNhanVien equals c.IdNhanVien
                              join d in _dbContext.HRM_BaoHiem_HopDong on a.IdNhanVien equals d.IdNhanVien
                              where (monthFilter == 0 || a.Created.Month == monthFilter) && (TimKiem.Nam == null || TimKiem.Nam == 0 || a.Created.Year == TimKiem.Nam)
                              select new NhanVienTangGiamBHXH
                              {
                                  IdGiamBH = a.IdGuid,
                                  Id = a.IdGuid,
                                  IdGuid = a.IdGuid,
                                  MaNV = b.MaNhanVien,
                                  MaSoThue = b.MaSoThueTNCN,
                                  HoTenNhanVien = b.TenDayDu,
                                  //DonVi = 
                                  NgaySinhNV = b.NgaySinh,
                                  SoBHXH = a.SoBHXH ?? b.SoBaoHiemXaHoi,
                                  MucDongBH = a.MucDong,
                                  PhanTramThu = ((a.MucDong * 10.5) / 100),
                                  NoiKCB = d.NoiDangKy,
                                  LoaiGiam = a.Loai,
                                  QuanLyGiamSat = a.QuanLy,
                                  LyDoGiam = a.LyDo,
                                  GhiChuGiam = a.GhiChu,
                                  IdBoPhanEOS = b.IdBoPhanEOS,
                              }).ToList();
                if (result != null && result.Count > 0 && TimKiem.IdCongTyEOS > 0)
                {
                    var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == TimKiem.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                }

                if (result != null && result.Count > 0 && TimKiem.IdBoPhanEOS > 0)
                {
                    result = result.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhanEOS).ToList();
                }
                if (result != null && result.Count > 0 && !string.IsNullOrEmpty(TimKiem.IdNhanVien))
                {
                    result = result.Where(a => a.IdGuid == TimKiem.IdNhanVien).ToList();
                }
                if (result != null && result.Count > 0 && !string.IsNullOrEmpty(TimKiem.LoaiBH))
                {
                    result = result.Where(a => a.LoaiGiam == TimKiem.LoaiBH).ToList();
                }
                var itemList = result.ToList();
                Parallel.ForEach(itemList, item =>
                {
                    item.LoaiGiam = DefineData.GetLoaiTangGiamBH(item.LoaiGiam);
                });

                int nSum = itemList.Count();
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi xảy ra --" + e.Message);
            }
        }
        public dynamic GetListTangBaoHiemXaHoi(dynamic dyn, ApplicationDbContext _dbContext)
        {
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                int nCurrentPage = TimKiem.CurrentPage ?? 1;
                int PageSize = TimKiem.PageSize ?? 25;
                int monthFilter = TimKiem.Thang ?? DateTime.Now.Month;
                var result = (from a in _dbContext.HRM_TangBaoHiem
                              join b in _dbContext.HRM_NhanVien on a.IdNhanVien equals b.IdGuid
                              where (TimKiem.Thang == 0 || a.ThoiGianDong.Value.Month == TimKiem.Thang) && (TimKiem.Nam == null || TimKiem.Nam == 0 || a.ThoiGianDong.Value.Year == TimKiem.Nam)
                              select new NhanVienTangGiamBHXH
                              {
                                  IdNhanVien = b.IdGuid,
                                  IdTangBH = a.IdGuid,
                                  Id = a.IdGuid,
                                  IdGuid = a.IdGuid,
                                  MaNV = b.MaNhanVien,
                                  MaSoThue = b.MaSoThueTNCN,
                                  HoTenNhanVien = b.TenDayDu,
                                  NgaySinhNV = b.NgaySinh,
                                  DiaChiNhanVien = b.HoKhauThuongTruDiaChi,
                                  SoDienThoai = b.DienThoai,
                                  NgayVao = b.NgayBatDauLamViec,
                                  SoBHXH = a.SoBHXH ?? b.SoBaoHiemXaHoi,
                                  ThoiGianDongBH = a.ThoiGianDong,
                                  MucDongBH = a.MucDong,
                                  PhanTramThu = ((a.MucDong * 10.5) / 100),
                                  LoaiGiam = a.Loai,
                                  PhanTramTangBH = a.PhanTramThu,
                                  LyDoGiam = a.LyDo,
                                  IdBoPhanEOS = b.IdBoPhanEOS,

                              }).Distinct().ToList();
                if (result != null && result.Count > 0 && TimKiem.IdCongTyEOS > 0)
                {
                    var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == TimKiem.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                }

                if (result != null && result.Count > 0 && TimKiem.IdBoPhanEOS > 0)
                {
                    result = result.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhanEOS).ToList();
                }
                if (result != null && result.Count > 0 && !string.IsNullOrEmpty(TimKiem.IdNhanVien))
                {
                    result = result.Where(a => a.IdGuid == TimKiem.IdNhanVien).ToList();
                }
                if (result != null && result.Count > 0 && !string.IsNullOrEmpty(TimKiem.LoaiBH))
                {
                    result = result.Where(a => a.LoaiGiam == TimKiem.LoaiBH).ToList();
                }

                var itemList = (from a in result
                                group a by new { a.MaNV, a.ThoiGianDongBH } into g
                                select g.FirstOrDefault()).ToList();
                List<string> listId = itemList.Select(a => a.IdNhanVien).Distinct().ToList();
                var listGiayTo = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => listId.Contains(a.IdNhanVien) && (a.Loai == 1 || a.Loai == 2) && a.isApDung == true).ToList();
                var listSDT = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => listId.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
                var listBH = (from a in _dbContext.HRM_BaoHiem_HopDong
                              join b in _dbContext.HRM_HopDong on a.IdHopDong equals b.IdGuid
                              where listId.Contains(b.IdNhanVien) && b.isKetThuc == true && b.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && a.isNgoai == false
                              group a by new { a.IdNhanVien } into g
                              select g.OrderByDescending(a => a.Modified).FirstOrDefault()).ToList();
                var danhMucNoicap = _dbContext.HRM_dmNoiCap.ToList();
                foreach (var item in itemList)
                {
                    var GiayTo = listGiayTo.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                    if (GiayTo != null)
                    {
                        item.SoCCCD = GiayTo.So;
                        item.NgayCapCCCD = GiayTo.NgayCap;
                        item.NoiCapCCCD = danhMucNoicap.Where(a => a.IdGuid == GiayTo.NoiCap).Select(a => a.Ten).FirstOrDefault();
                    }
                    item.SoDienThoai = listSDT.Where(a => a.IdNhanVien == item.IdNhanVien).Select(a => a.SoDienThoai).FirstOrDefault();
                    var BH = listBH.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                    if (BH != null)
                    {
                        item.NoiKCB = BH.NoiDangKy;
                        item.SoBHXH = BH.SoSoBaoHiem;

                    }
                    item.LoaiGiam = DefineData.GetLoaiTangGiamBH(item.LoaiGiam);
                }
                int nSum = itemList.Count();
                int nTotalPage = nSum / nCurrentPage;
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi xảy ra --" + e.Message);
            }
        }
        public class NhanVienTangGiamBHXH
        {
            public string IdNhanVien { get; set; }
            public string IdGiamBH { get; set; }
            public string IdTangBH { get; set; }
            public string Id { get; set; }
            public string IdGuid { get; set; }
            public string MaNV { get; set; }
            public string MaSoThue { get; set; }
            public string HoTenNhanVien { get; set; }
            public string QuanLyGiamSat { get; set; }
            public string GhiChuGiam { get; set; }
            public DateTime NgaySinhNV { get; set; }
            public string SoCCCD { get; set; }
            public DateTime? NgayCapCCCD { get; set; }
            public DateTime? NgayVao { get; set; }
            public DateTime? ThoiGianDongBH { get; set; }
            public string NoiCapCCCD { get; set; }
            public string DiaChiNhanVien { get; set; }
            public string SoDienThoai { get; set; }
            public string SoBHXH { get; set; }
            public string NoiKCB { get; set; }
            public string LoaiGiam { get; set; }
            public double MucDongBH { get; set; }
            public double PhanTramThu { get; set; }
            public double PhanTramTangBH { get; set; }
            public string LyDoGiam { get; set; }
            public long IdBoPhanEOS { get; set; }
        }
        public dynamic GetListHuongLuongHuu(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                DateTime DateNow = DateTime.Today;
                DateTime timeNam = DateNow.AddYears(-60).AddMonths(-6);
                DateTime timeNu = DateNow.AddYears(-55).AddMonths(-8);
                DateTime timeDong = DateNow.AddYears(-(15));


                List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                join b in _dbContext.HRM_HopDong on a.IdGuid equals b.IdNhanVien
                                                                join c in _dbContext.HRM_BaoHiem_HopDong on b.IdGuid equals c.IdHopDong
                                                                where b.isKetThuc == true && b.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && c.NgayBatDauDongBHXH != null && /*((gender == "nam" && timeNam >= a.NgaySinh) || (gender == "nữ" && timeNu >= a.NgaySinh) ||*/ (c.NgayCapSoBaoHiem.Value.Year > 1970 && timeDong >= c.NgayCapSoBaoHiem)

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
                                                                    MaSoThue = a.MaSoThueTNCN,
                                                                    HoKhauThuongTruDiaChi = a.HoKhauThuongTruDiaChi,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh,
                                                                    SoBHXH = a.SoBaoHiemXaHoi,
                                                                    isLuongHuu = a.isLuongHuu,
                                                                    DiaChi = a.ChoOHienNayTruDiaChi,
                                                                    NgayKy = b.Modified,
                                                                    NgayDongBHXH = c.NgayBatDauDongBHXH.Value

                                                                }
                                                                ).OrderBy(a => a.MaNhanVien).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    var listIdNv = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    var listNv = (from a in _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi
                                  where a.isThamGia == false && listIdNv.Contains(a.IdNhanVien)
                                  group a by new { a.IdNhanVien } into g
                                  select new
                                  {
                                      IdNhanVien = g.Key.IdNhanVien,
                                      ThoiGian = g.Sum(b => (b.DenNgay - b.TuNgay).TotalDays)
                                  }
                          ).ToList();
                    listIdNv = new List<string>();
                    if (listNv != null && listNv.Count > 0)
                        foreach (var item in listNhanVien)
                        {
                            var NgatQuang = listNv.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                            if (NgatQuang != null)
                            {
                                DateTime check = timeDong.AddDays(-NgatQuang.ThoiGian);
                                if (check >= item.NgayDongBHXH)
                                    listIdNv.Add(item.IdGuid);
                            }
                        }
                    listNhanVien = listNhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdCongTyEOS > 0)
                {
                    var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }

                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.IdNhanVien))
                {
                    listNhanVien = listNhanVien.Where(a => a.IdGuid == aRequest.IdNhanVien).ToList();
                }

                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    listNhanVien = (from a in listNhanVien
                                    group a by new { a.IdGuid } into g
                                    select g.OrderByDescending(a => a.NgayKy).FirstOrDefault()).ToList();
                    int count = listNhanVien.Count();

                    int CurrentPage = aRequest.CurrentPage;
                    int PageSize = aRequest.PageSize ?? 20;
                    List<DanhSachNhanVienViewModel> listNhanViens = listNhanVien.OrderBy(r => r.MaNhanVien).ThenByDescending(r => r.NgayBatDau).Skip(PageSize * (aRequest.CurrentPage - 1)).Take(PageSize).ToList();
                    var listCOngTy = _CallWebService.GetDanhSachDuAn(_dbContext);
                    var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                    foreach (var model in listNhanViens)
                    {

                        model.TenCongTy = listCOngTy.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                        model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                        model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                        if (listChucVu != null && listChucVu.Count > 0)
                        {
                            var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == model.IdChucVuEOS);
                            if (boPhan_NhanVien != null)
                            {
                                model.TenChucVu = boPhan_NhanVien.TenChucVu;
                            }
                        }
                        if (listChucDanh != null && listChucDanh.Count > 0)
                        {
                            var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == model.IdChucDanhEOS);
                            if (boPhan_NhanVien != null)
                            {
                                model.TenChucDanh = boPhan_NhanVien.Ten;
                            }

                        }

                        if (model.GioiTinh == "KhongXacDinh")
                            model.GioiTinh = "Khác";
                    }
                    result.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
                    result.items = listNhanViens;
                    //result.value = lstBoPhanAll;
                }

                return result;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }
        public ErrorMessage ExportHuongLuongHuu(List<string> Ids, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachHuongLuongHuu.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "DanhSachHuongLuongHuu_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                int stt = 1;
                int nRow = 5;
                int nCol = 1;
                List<HRM_NhanVien> listnhanvien = _dbContext.HRM_NhanVien.Where(x => x.isLuongHuu == true).ToList();

                if (listnhanvien != null && listnhanvien.Count > 0)
                {
                    foreach (var item in listnhanvien)
                    {
                        sheet1.Row(nRow).Height = 55;
                        var BoPhan = _CallWebService.GetBoPhan(_dbContext, item.IdBoPhanEOS);
                        var ChucVu = _CallWebService.GetChucVu(_dbContext, item.IdChucVu);
                        var CongTy = _CallWebService.GetBoPhan(_dbContext, item.IdCongTyEOS);
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = stt;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaSoThueTNCN;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoBaoHiemXaHoi;
                        nCol++;
                        if (BoPhan != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = CongTy.TenBoPhan;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = BoPhan.TenBoPhan;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = ChucVu.TenChucVu;
                            nCol++;
                        }
                        nRow++;
                        stt++;
                    }
                    if (nRow > 4)
                    {
                        dungchung.DrawTableExcel(sheet1, 5, nRow - 1, 1, 12);
                        sheet1.Cells[5, 1, nRow, nCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet1.Cells[5, 1, nRow, nCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                }


                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/SmartEOSAPI/FileUploader/Get?filename=" + dungchung.Base64Encode("DanhSachHuongLuongHuu.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportHuongLuongHuu " + e.ToString());
            }
            return err;
        }
        public ErrorMessage UpdateTrangThaiLuongHuu(string Ids, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            try
            {
                var nhanVien = _dbContext.HRM_NhanVien.Where(x => Ids.Contains(x.IdGuid)).ToList();
                foreach (var item in nhanVien)
                {
                    item.isLuongHuu = true;
                }

                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                err.SetLoiVang("UpdateTrangThaiLuongHuu " + e.ToString());

            }
            return err;
        }
        public ErrorMessage ExportGiamBaoHiemXaHoi(List<string> Ids, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachGiamBHXH.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "DanhSachGiamBHXH_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                int stt = 1;
                int nRow = 5;
                int nCol = 1;
                List<HRM_GiamBaoHiem> listgiambaohiem = _dbContext.HRM_GiamBaoHiem.Where(x => Ids.Contains(x.IdGuid)).OrderBy(a => a.Loai).ToList();
                var idNhanVienSet = listgiambaohiem.Select(nv => nv.IdNhanVien);
                List<HRM_NhanVien> listnhanvien = _dbContext.HRM_NhanVien.Where(x => idNhanVienSet.Contains(x.IdGuid)).ToList();
                var listNoiCapBH = _dbContext.HRM_dmNoiCapBaoHiem.ToList();
                if (listgiambaohiem != null && listgiambaohiem.Count > 0)
                {
                    foreach (var giambh in listgiambaohiem)
                    {
                        nCol = 1;
                        sheet1.Row(nRow).Height = 55;
                        var nvien = listnhanvien.FirstOrDefault(x => x.IdGuid == giambh.IdNhanVien);
                        if (nvien != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = stt;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = nvien.MaNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = nvien.MaSoThueTNCN;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = nvien.TenDayDu;
                            nCol++;
                            var BoPhan = _CallWebService.GetBoPhan(_dbContext, nvien.IdBoPhanEOS);
                            sheet1.Cells[nRow, nCol].Value = BoPhan.TenBoPhan;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = nvien.NgaySinh.ToString("dd/MM/yyyy");
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = nvien.SoBaoHiemXaHoi;
                            nCol++;
                        }
                        else nCol += 8;
                        sheet1.Cells[nRow, nCol].Value = giambh.MucDong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = giambh.Loai;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = giambh.QuanLy;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = giambh.LyDo;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = giambh.GhiChu;
                        nCol++;
                        //if (nvien != null)
                        //{
                        //    sheet1.Cells[nRow, 1].Value = stt;
                        //    sheet1.Cells[nRow, 2].Value = nvien.MaNhanVien;
                        //    sheet1.Cells[nRow, 3].Value = nvien.MaSoThueTNCN;
                        //    sheet1.Cells[nRow, 4].Value = nvien.TenDayDu;
                        //    var BoPhan = _CallWebService.GetBoPhan(_dbContext, nvien.IdBoPhanEOS);
                        //    sheet1.Cells[nRow, 5].Value = BoPhan.TenBoPhan;
                        //    sheet1.Cells[nRow, 6].Value = nvien.NgaySinh.ToString("dd/MM/yyyy");
                        //    sheet1.Cells[nRow, 7].Value = nvien.SoBaoHiemXaHoi;
                        //}
                        //sheet1.Cells[nRow, 8].Value = giambh.MucDong;
                        //sheet1.Cells[nRow, 9].Value = giambh.Loai;
                        //sheet1.Cells[nRow, 10].Value = giambh.QuanLy;
                        //sheet1.Cells[nRow, 11].Value = giambh.LyDo;
                        //sheet1.Cells[nRow, 12].Value = giambh.GhiChu;
                        nRow++;
                        stt++;
                    }
                    if (nRow > 4)
                    {
                        dungchung.DrawTableExcel(sheet1, 5, nRow - 1, 1, 12);
                        sheet1.Cells[5, 1, nRow, nCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet1.Cells[5, 1, nRow, nCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                }

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/SmartEOSAPI/FileUploader/Get?filename=" + dungchung.Base64Encode("DanhSachGiamBHXH.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportGiamBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ExportTangBaoHiemXaHoi(List<string> Ids, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachTangBHXH.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "DanhSachTangBHXH_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                List<HRM_TangBaoHiem> listtangbaohiem = _dbContext.HRM_TangBaoHiem.Where(x => Ids.Contains(x.IdGuid)).OrderBy(a => a.Loai).ToList();
                List<HRM_NhanVien_GiayToTuyThan> listgiaytonhanvien = _dbContext.HRM_NhanVien_GiayToTuyThan.ToList();
                var listNoiCapBH = _dbContext.HRM_dmNoiCapBaoHiem.ToList();
                //var idNhanVienSet = listtangbaohiem.Select(nv => nv.IdNhanVien);
                //List<HRM_NhanVien_GiayToTuyThan> filteredList = listgiaytonhanvien.Where(x => idNhanVienSet.Contains(x.IdNhanVien)).ToList();
                //List<HRM_NhanVien> listnhanvien = _dbContext.HRM_NhanVien.Where(x => idNhanVienSet.Contains(x.IdGuid)).ToList();
                List<HRM_NhanVien> listnhanvien = _dbContext.HRM_NhanVien.ToList();

                int stt = 1;
                int nRow = 5;
                if (listtangbaohiem != null && listtangbaohiem.Count > 0)
                {
                    foreach (var item in listtangbaohiem)
                    {
                        sheet1.Row(nRow).Height = 55;
                        var nhanVien = listnhanvien.FirstOrDefault(x => x.IdGuid == item.IdNhanVien);
                        if (nhanVien != null)
                        {
                            var BoPhan = _CallWebService.GetBoPhan(_dbContext, nhanVien.IdBoPhanEOS);
                            sheet1.Cells[nRow, 1].Value = stt;
                            sheet1.Cells[nRow, 2].Value = nhanVien.MaNhanVien;
                            sheet1.Cells[nRow, 3].Value = nhanVien.MaSoThueTNCN;
                            sheet1.Cells[nRow, 4].Value = nhanVien.TenDayDu;
                            sheet1.Cells[nRow, 5].Value = nhanVien.NgaySinh.ToString("dd/MM/yyyy");
                            sheet1.Cells[nRow, 9].Value = nhanVien.HoKhauThuongTruDiaChi;
                            sheet1.Cells[nRow, 9].Style.WrapText = true;
                            sheet1.Cells[nRow, 10].Value = nhanVien.DienThoai;
                            sheet1.Cells[nRow, 11].Value = BoPhan.TenBoPhan;
                            sheet1.Cells[nRow, 11].Style.WrapText = true;
                            sheet1.Cells[nRow, 12].Value = nhanVien.NgayBatDauLamViec;
                            sheet1.Cells[nRow, 13].Value = stt;
                            sheet1.Cells[nRow, 14].Value = nhanVien.SoBaoHiemXaHoi;
                        }
                        var giayTo = listgiaytonhanvien.FirstOrDefault(x => x.IdNhanVien == item.IdNhanVien && x.isApDung == true);
                        if (giayTo != null)
                        {
                            sheet1.Cells[nRow, 6].Value = giayTo.So;
                            sheet1.Cells[nRow, 7].Value = giayTo.NgayCap?.ToString("dd/MM/yyyy");
                            sheet1.Cells[nRow, 8].Value = listNoiCapBH.Where(a => a.IdGuid == giayTo.NoiCap).Select(a => a.Ten).FirstOrDefault();
                        }
                        sheet1.Cells[nRow, 15].Value = item.ThoiGianDong;
                        sheet1.Cells[nRow, 16].Value = item.Loai;
                        sheet1.Cells[nRow, 16].Style.WrapText = true;
                        sheet1.Cells[nRow, 17].Value = item.MucDong;
                        sheet1.Cells[nRow, 17].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        sheet1.Cells[nRow, 18].Value = item.PhanTramThu;
                        sheet1.Cells[nRow, 18].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;

                        sheet1.Cells[nRow, 19].Value = item.LyDo;
                        if (!string.IsNullOrEmpty(item.IdNguoiPheDuyet))
                        {
                            sheet1.Cells[nRow, 20].Value = listnhanvien.Where(a => a.IdGuid == item.IdNguoiPheDuyet).Select(a => a.TenDayDu).FirstOrDefault();
                        }
                        if (item.NgayPheDuyet != null && item.NgayPheDuyet.Value.Year > 1970)
                            sheet1.Cells[nRow, 21].Value = item.NgayPheDuyet.Value.ToString("dd/MM/yyyy");
                        nRow++;
                        stt++;
                    }
                    if (nRow > 4)
                    {
                        dungchung.DrawTableExcel(sheet1, 5, nRow - 1, 1, 21);
                        sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        sheet1.Cells[5, 9, nRow, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        sheet1.Cells[5, 19, nRow, 20].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/SmartEOSAPI/FileUploader/Get?filename=" + dungchung.Base64Encode("DanhSachTangBHXH.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportTangBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportGiamBaoHiemXaHoi(string FileName, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                HashSet<string> validLoaiValues = new HashSet<string>
                {
                  eLoaiBaoHiem.GiamDoNghiViec.ToString(),
                  eLoaiBaoHiem.GiamThaiSan.ToString(),
                  eLoaiBaoHiem.GiamKhongLuong.ToString()
                };
                List<string> listNhanVienIds = new List<string>();
                for (int row = 5; row <= sheet1.Cells.End.Row; row++)
                {
                    string nhanVienId = dungchung.GetCell(sheet1, row, 2);
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        listNhanVienIds.Add(nhanVienId);
                    }
                }

                var listNhanVien = _dbContext.HRM_NhanVien.Where(nv => listNhanVienIds.Contains(nv.MaNhanVien)).ToDictionary(nv => nv.MaNhanVien, nv => nv.IdGuid);
                List<HRM_GiamBaoHiem> listGiamBaoHiemXaHoiImport = new List<HRM_GiamBaoHiem>();
                for (int row = 5; row <= sheet1.Cells.End.Row; row++)
                {
                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    HRM_GiamBaoHiem dm = new HRM_GiamBaoHiem();
                    dm.IdGuid = Guid.NewGuid().ToString();
                    dm.MucDong = dungchung.GetCellToDouble(sheet1, row, 8);
                    string loaiString = dungchung.GetCell(sheet1, row, 9);
                    if (!validLoaiValues.Contains(loaiString))
                    {
                        err.SetLoiVang($"Row {row}: Invalid Loai value '{loaiString}'.");
                    }
                    if (listNhanVien.ContainsKey(Ma))
                    {
                        dm.IdNhanVien = listNhanVien[Ma];
                    }
                    dm.Loai = loaiString;
                    dm.QuanLy = dungchung.GetCell(sheet1, row, 10);
                    dm.LyDo = dungchung.GetCell(sheet1, row, 11);
                    dm.GhiChu = dungchung.GetCell(sheet1, row, 12);
                    listGiamBaoHiemXaHoiImport.Add(dm);
                }
                _dbContext.HRM_GiamBaoHiem.AddRange(listGiamBaoHiemXaHoiImport);
                s.Close();
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportGiamBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage ImportTangBaoHiemXaHoi(string FileName, ApplicationDbContext _dbContext)
        {
            int RowError = 0;
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                HashSet<string> validLoaiValues = new HashSet<string>
                {
                  eLoaiBaoHiem.TangMoi.ToString(),
                  eLoaiBaoHiem.TangLaiSauThaiSan.ToString(),
                  eLoaiBaoHiem.TangSauNghiKhongLuong.ToString()
                };
                List<string> listNhanVienIds = new List<string>();
                for (int row = 5; row <= sheet1.Cells.End.Row; row++)
                {
                    string nhanVienId = dungchung.GetCell(sheet1, row, 2);
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        listNhanVienIds.Add(nhanVienId);
                    }
                }

                var listNhanVien = _dbContext.HRM_NhanVien.Where(nv => listNhanVienIds.Contains(nv.MaNhanVien)).ToDictionary(nv => nv.MaNhanVien, nv => nv.IdGuid);
                List<HRM_TangBaoHiem> listGiamBaoHiemXaHoiImport = new List<HRM_TangBaoHiem>();
                for (int row = 5; row <= sheet1.Cells.End.Row; row++)
                {
                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    RowError = row;
                    HRM_TangBaoHiem dm = new HRM_TangBaoHiem();
                    dm.IdGuid = Guid.NewGuid().ToString();
                    string nhanVienId = dungchung.GetCell(sheet1, row, 2);
                    if (listNhanVien.ContainsKey(nhanVienId))
                    {
                        dm.IdNhanVien = listNhanVien[Ma];
                    }
                    dm.ThoiGianDong = dungchung.GetCellToDateTime(sheet1, row, 15);
                    string loaiString = dungchung.GetCell(sheet1, row, 16);
                    if (!validLoaiValues.Contains(loaiString))
                    {
                        err.SetLoiVang($"Row {row}: Invalid Loai value '{loaiString}'.");
                    }
                    dm.Loai = loaiString;
                    dm.MucDong = dungchung.GetCellToDouble(sheet1, row, 17);
                    dm.PhanTramThu = dungchung.GetCellToDouble(sheet1, row, 18);
                    dm.LyDo = dungchung.GetCell(sheet1, row, 19);
                    listGiamBaoHiemXaHoiImport.Add(dm);
                }
                _dbContext.HRM_TangBaoHiem.AddRange(listGiamBaoHiemXaHoiImport);
                s.Close();
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportTangBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportQuaTrinhThamGiaBaoHiemXaHoi(ApplicationDbContext _dbContext, string FileName, string IdNhanVien = "")
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                string[] ColumnNames = new string[] { "STT", "Mã nhân viên", "Tên nhân viên", "Mã số thuế", "Đơn vị", "Bộ phận", "Chức vụ" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "QUÁ TRÌNH THAM GIA BẢO HIỂM XÃ HỘI"))
                {
                    return err;
                }
                var listdaTa = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AsNoTracking().ToList();
                var listNhanVien = _dbContext.HRM_NhanVien.ToList();
                if (!string.IsNullOrEmpty(IdNhanVien))
                    listNhanVien = listNhanVien.Where(a => a.IdGuid == IdNhanVien).ToList();
                List<HRM_QuaTrinhThamGiaBaoHiemXaHoi> listAdd = new List<HRM_QuaTrinhThamGiaBaoHiemXaHoi>();
                List<string> listMa = new List<string>();
                DateTime dt = DateTime.Now;
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_QuaTrinhThamGiaBaoHiemXaHoi thuvien = new HRM_QuaTrinhThamGiaBaoHiemXaHoi();
                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    thuvien.IdGuid = Guid.NewGuid().ToString();
                    thuvien.isTuThem = true;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == Ma || (!string.IsNullOrEmpty(IdNhanVien) && a.IdGuid == IdNhanVien));
                    if (NhanVien != null)
                    {
                        thuvien.IdNhanVien = NhanVien.IdGuid;
                        thuvien.IdUser = NhanVien.IdUser;
                    }
                    else
                    {

                        listMa.Add(Ma);
                    }
                    thuvien.TenBoPhan = dungchung.GetCell(sheet1, row, 5);
                    thuvien.TenChucVu = dungchung.GetCell(sheet1, row, 6);
                    thuvien.TenCongTy = dungchung.GetCell(sheet1, row, 7);
                    {
                        string Thang = dungchung.GetCell(sheet1, row, 8);
                        if (!string.IsNullOrEmpty(Thang))
                        {
                            var list = Thang.Split('/');
                            thuvien.TuNgay = new DateTime(int.Parse(list[1]), int.Parse(list[0]), 1);
                        }
                    }
                    {
                        string Thang = dungchung.GetCell(sheet1, row, 9);
                        if (!string.IsNullOrEmpty(Thang))
                        {
                            var list = Thang.Split('/');
                            thuvien.DenNgay = new DateTime(int.Parse(list[1]), int.Parse(list[0]), 1);
                            thuvien.DenNgay.AddMonths(1).AddDays(-1);
                        }
                    }
                    if (thuvien.TuNgay != null && thuvien.TuNgay.Year > 1970)
                        thuvien.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(thuvien.TuNgay);
                    if (thuvien.DenNgay != null && thuvien.DenNgay.Year > 1970)
                        thuvien.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(thuvien.DenNgay);
                    thuvien.SoBHXH = dungchung.GetCell(sheet1, row, 10);
                    thuvien.MucLuongDong = dungchung.GetCellToDouble(sheet1, row, 11);
                    thuvien.TienLuong = dungchung.GetCellToDouble(sheet1, row, 12);
                    thuvien.HeSo = dungchung.GetCellToDouble(sheet1, row, 13);
                    string thamGia = dungchung.GetCell(sheet1, row, 14);
                    thuvien.GhiChu = dungchung.GetCell(sheet1, row, 15);
                    if (thamGia.ToLower() == "có")
                        thuvien.isThamGia = true;
                    else
                        thuvien.isThamGia = false;
                    listAdd.Add(thuvien);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("mã nhân viên không đúng: " + string.Join(", ", listAdd));
                    return err;
                }

                if (string.IsNullOrEmpty(IdNhanVien))
                    _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AddRange(listAdd);

                s.Close();
                _dbContext.SaveChanges();
                err.Data = listAdd;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportQuaTrinhThamGiaBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage ExportQuaTrinhThamGiaBaoHiemXaHoi(ApplicationDbContext _dbContext, string IdNhanVien = "")
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuaTrinhThamGiaBaoHiemXaHoi.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                if (!string.IsNullOrEmpty(IdNhanVien))
                {
                    var NhanVien = _dbContext.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == IdNhanVien);
                    sheet1.Cells[4, 1].Value = 1;
                    sheet1.Cells[4, 2].Value = NhanVien.MaNhanVien;
                    sheet1.Cells[4, 3].Value = NhanVien.TenDayDu;
                    sheet1.Cells[4, 4].Value = NhanVien.MaSoThueTNCN;
                    sheet1.Cells[4, 5].Value = _CallWebService.GetDanhSachBoPhan(_dbContext).Where(a => a.Id == NhanVien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    sheet1.Cells[4, 6].Value = _CallWebService.GetDanhSachChucVu(_dbContext).Where(a => a.Id == NhanVien.IdChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                    sheet1.Cells[4, 7].Value = _CallWebService.GetDanhSachCongTy(_dbContext).Where(a => a.Id == NhanVien.IdCongTyEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("QuaTrinhThamGiaBaoHiemXaHoi.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportQuaTrinhThamGiaBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage ExportQuaTrinhThamGiaBaoHiemXaHoiThang(ApplicationDbContext _dbContext, EntityGetDanhSachNhanVienRequest aRequest)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                DateTime dt = DateTime.Now;
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/MauBCquatrinhthamgiaBHXH.xlsx");
                string sFileNameCopy = PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var QuaTrinhThamGias = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.ToList();
                DateTime time = dungchung.UnixTimeStampToDateTime(aRequest.NgayUnix);
                if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                {

                    if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                    {

                        QuaTrinhThamGias = QuaTrinhThamGias.Where(a => (a.TuNgay != null && a.TuNgay <= time && (a.DenNgay != null || (a.DenNgay != null && a.DenNgay >= time)))).ToList();
                    }
                    List<string> listId = QuaTrinhThamGias.Select(a => a.IdNhanVien).Distinct().ToList();
                    DateTime DateNow = DateTime.Now;
                    List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                    where a.isLuongHuu == false
                                                                    && listId.Contains(a.IdGuid)
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
                                                                        MaSoThue = a.MaSoThueTNCN,
                                                                        SoBHXH = a.SoBaoHiemXaHoi,
                                                                        NguyenQuan = a.NguyenQuan,
                                                                        NgayKetThucLamViec = a.NgayNghiViec,
                                                                        IdChucDanhEOS = a.IdChucDanh
                                                                    }
                                                                    ).OrderBy(a => a.MaNhanVien).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdCongTyEOS > 0)
                    {
                        var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                        listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.IdNhanVien))
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdGuid == aRequest.IdNhanVien).ToList();
                    }

                    if (listNhanVien != null && listNhanVien.Count > 0)
                    {
                        int count = listNhanVien.Count();

                        int CurrentPage = aRequest.CurrentPage;
                        int PageSize = aRequest.PageSize ?? 20;
                        List<DanhSachNhanVienViewModel> listNhanViens = listNhanVien.OrderBy(r => r.MaNhanVien).ThenByDescending(r => r.NgayBatDau).Skip(PageSize * (aRequest.CurrentPage - 1)).Take(PageSize).ToList();

                        var listLuongNhanVien = _dbContext.HRM_Luong_NhanVien.Where(a => listId.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= DateNow && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= DateNow)).ToList();

                        dt = new DateTime(dt.Year, dt.Month, 1);
                        dt = dt.AddMonths(1).AddDays(-1);
                        var lstCongTyAll = _CallWebService.GetDanhSachCongTy(_dbContext);
                        var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                        var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                        var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                        var listIdQuyTrinh_NhanVien_1 = listNhanVien.Select(x => x.IdGuid).ToList();
                        var listQuyTrinh_BoPhan_1 = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdQuyTrinh)).ToList();
                        foreach (var model in listNhanViens)
                        {

                            var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                            var _listBoPhan = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                            model.TenCongTy = string.Join("; ", _listBoPhan);
                            model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                            model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                            model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                            model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                            if (listChucVu != null && listChucVu.Count > 0)
                            {
                                var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == model.IdChucVuEOS);
                                if (boPhan_NhanVien != null)
                                {
                                    model.TenChucVu = boPhan_NhanVien.TenChucVu;

                                }

                            }
                            if (listChucDanh != null && listChucDanh.Count > 0)
                            {
                                var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == model.IdChucDanhEOS);
                                if (boPhan_NhanVien != null)
                                {
                                    model.TenChucDanh = boPhan_NhanVien.Ten;

                                }

                            }
                            if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                            {

                                var LuongNV = QuaTrinhThamGias.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                                if (LuongNV != null)
                                {
                                    model.Luong = LuongNV.TienLuong;
                                    model.LuongBaoHiem = LuongNV.MucLuongDong;
                                }

                            }
                            if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                            {
                                var qt = QuaTrinhThamGias.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.TuNgay).ToList();
                                var BatDau = qt.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.TuNgay).FirstOrDefault();
                                if (BatDau != null)
                                {
                                    model.NgayBatDau = BatDau.TuNgay;

                                }
                                var KetThuc = qt.Where(a => a.IdNhanVien == model.IdGuid).OrderByDescending(a => a.TuNgay).FirstOrDefault();
                                if (KetThuc != null)
                                {
                                    model.NgayKetThuc = KetThuc.DenNgay;
                                    if (KetThuc.DenNgay.Year <= 1970)
                                        model.NgayKetThuc = dt;
                                }
                                else
                                    model.NgayKetThuc = dt;
                                model.TongThoiGianThamGia = 0;
                                foreach (var ngay in qt)
                                {
                                    if (ngay.isThamGia == false)
                                        continue;
                                    if (ngay.DenNgay.Year <= 1970)
                                        ngay.DenNgay = dt;
                                    double Day = (ngay.DenNgay - ngay.TuNgay).TotalDays + 1;
                                    int Thang = ((ngay.DenNgay.Year - ngay.TuNgay.Year) * 12) + ngay.DenNgay.Month - ngay.TuNgay.Month + 1;
                                    model.TongThoiGianThamGia += Thang;
                                }
                                if (model.TongThoiGianThamGia < 12)
                                    model.TongThoiGianTheoNam = model.TongThoiGianThamGia + " Tháng";
                                else
                                {
                                    int Nam = (int)(model.TongThoiGianThamGia / 12);
                                    model.TongThoiGianTheoNam = Nam + " năm " + (model.TongThoiGianThamGia - (Nam * 12)) + " tháng";
                                }
                            }

                            if (model.GioiTinh == "KhongXacDinh")
                                model.GioiTinh = "Khác";
                        }
                    }



                    sheet1.Cells[1, 1].Value = "QUÁ TRÌNH THAM GIA BHXH THÁNG " + time.Month + " năm " + time.Year;
                    int STT = 1;
                    int nRow = 3;
                    int ncol = 1;
                    foreach (var item in listNhanVien)
                    {
                        ncol = 1;
                        sheet1.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.MaNhanVien;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.TenDayDu;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.SoBHXH;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.TenCongTy;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.TenBoPhan;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.TenChucVu;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.LuongBaoHiem;



                        STT++;
                        nRow++;
                    }
                    nRow--;
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    dungchung.DrawTableExcel(sheet1, 2, nRow, 1, ncol);
                    sheet1.Cells[3, 3, nRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells[3, 5, nRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells[3, 8, nRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet1.Cells[3, 8, nRow, 8].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;





                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("QuaTrinhThamGiaBaoHiemXaHoi.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportQuaTrinhThamGiaBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage ExportQuaTrinhThamGiaBaoHiem(ApplicationDbContext _dbContext, string IdNhanVien)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuaTrinhThamGiaBaoHiem.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var NhanVien = _dbContext.HRM_NhanVien.Find(IdNhanVien);
                var congty = _CallWebService.GetDanhSachCongTy(_dbContext).FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                var BoPhan = _CallWebService.GetBoPhan(_dbContext, NhanVien.IdBoPhanEOS);
                var ChucVu = _CallWebService.GetChucVu(_dbContext, NhanVien.IdChucVu);
                var listdaTa = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(a => a.IdNhanVien == IdNhanVien).OrderBy(a => a.TuNgay).ToList();
                sheet1.Cells[2, 3].Value = NhanVien.MaNhanVien;
                sheet1.Cells[3, 3].Value = NhanVien.TenDayDu;
                if (congty != null)
                    sheet1.Cells[4, 3].Value = congty.TenBoPhan;
                if (ChucVu != null)
                    sheet1.Cells[2, 6].Value = ChucVu.TenChucVu;
                if (BoPhan != null)
                    sheet1.Cells[3, 6].Value = BoPhan.TenBoPhan;
                sheet1.Cells[4, 6].Value = NhanVien.SoBaoHiemXaHoi;
                int STT = 1;
                int nRow = 6;
                int ncol = 1;
                int Tong = 0;
                nRow++;
                foreach (var item in listdaTa)
                {
                    ncol = 1;
                    sheet1.Cells[nRow, ncol].Value = STT;
                    ncol++;
                    if (item.TuNgay != null)
                        sheet1.Cells[nRow, ncol].Value = item.TuNgay.ToString("MM/yyyy");
                    ncol++;
                    if (item.DenNgay != null && item.DenNgay.ToString("dd/MM/yyyy") != "01/01/1970")
                        sheet1.Cells[nRow, ncol].Value = item.DenNgay.ToString("MM/yyyy");
                    ncol++;
                    if (item.TuNgay != null)
                    {
                        if (item.DenNgay == null || item.DenNgay.ToString("dd/MM/yyyy") == "01/01/1970")
                        {
                            item.DenNgay = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                        }
                        int Thang = ((item.DenNgay.Year - item.TuNgay.Year) * 12) + item.DenNgay.Month - item.TuNgay.Month + 1;
                        sheet1.Cells[nRow, ncol].Value = Thang;
                        if (item.isThamGia == true)
                            Tong = Tong + Thang;
                        ncol++;
                        if (Thang < 12)
                            sheet1.Cells[nRow, ncol].Value = Thang + " Tháng";
                        else
                        {
                            int Nam = (int)(Thang / 12);
                            int SThang = Thang - (Nam * 12);
                            sheet1.Cells[nRow, ncol].Value = Nam + " năm " + SThang + " tháng";
                        }
                    }
                    else
                        ncol++;
                    ncol++;
                    sheet1.Cells[nRow, ncol].Value = item.TenCongTy;
                    ncol++;
                    sheet1.Cells[nRow, ncol].Value = item.MucLuongDong;
                    ncol++;
                    sheet1.Cells[nRow, ncol].Value = item.HeSo;
                    ncol++;

                    sheet1.Cells[nRow, ncol].Value = item.TienLuong;

                    STT++;
                    nRow++;
                }

                nRow--;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells[5, 1, nRow, ncol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[5, 1, nRow, ncol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                dungchung.DrawTableExcel(sheet1, 2, nRow, 1, ncol);
                sheet1.Cells[3, 4, nRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[6, ncol - 2, nRow, ncol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet1.Cells[6, ncol - 2, nRow, ncol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                sheet1.Cells[6, ncol - 1, nRow, ncol - 1].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                sheet1.Cells[6, 1, 6, 3].Merge = true;
                sheet1.Cells[6, 1].Value = "Tổng cộng";
                sheet1.Cells[6, 4].Value = Tong;
                if (Tong < 12)
                    sheet1.Cells[6, 5].Value = Tong + " Tháng";
                else
                {
                    int Nam = (int)(Tong / 12);
                    int SThang = Tong - (Nam * 12);
                    sheet1.Cells[6, 5].Value = Nam + " năm " + SThang + " tháng";
                }
                sheet1.Cells[6, 1, 6, 5].Style.Font.Bold = true;


                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("QuaTrinhThamGiaBaoHiem.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportQuaTrinhThamGiaBaoHiemXaHoi " + e.ToString());
            }
            return err;
        }

        public ErrorMessage GetQuaTrinhThamGiaBaoHiemXaHoi(ApplicationDbContext _dbContext, string IdNhanVien)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                var listdaTa = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(a => a.IdNhanVien == IdNhanVien).OrderBy(a => a.TuNgay).ToList();
                Parallel.ForEach(listdaTa, item =>
                {
                    if (item.TuNgay != null)
                        item.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.TuNgay);
                    if (item.DenNgay != null && item.DenNgay.Year > 1970)
                        item.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.DenNgay);
                }
                    );
                err.Data = listdaTa;

            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportQuaTrinhThamGiaBaoHiemThatNghiep " + e.ToString());
            }
            return err;
        }
        public ErrorMessage UpdateQuaTrinhThamGiaBaoHiemXaHoi(ApplicationDbContext _dbContext, HRM_QuaTrinhThamGiaBaoHiemXaHoi NhanVien)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                NhanVien.TuNgay = dungchung.UnixTimeStampToDateTime(NhanVien.TuNgayUnix);
                NhanVien.DenNgay = dungchung.UnixTimeStampToDateTime(NhanVien.DenNgayUnix);


                _dbContext.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportQuaTrinhThamGiaBaoHiemThatNghiep " + e.ToString());
            }
            return err;
        }

        public dynamic GetDanhSachNhanVienForBaoHiemXaHoi(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                var QuaTrinhThamGias = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.ToList();
                if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                {

                    if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0 && aRequest.NgayUnix > 0)
                    {
                        DateTime time = dungchung.UnixTimeStampToDateTime(aRequest.NgayUnix);
                        QuaTrinhThamGias = QuaTrinhThamGias.Where(a => (a.TuNgay != null && a.TuNgay <= time && (a.DenNgay != null || (a.DenNgay != null && a.DenNgay >= time)))).ToList();
                    }
                    List<string> listId = QuaTrinhThamGias.Select(a => a.IdNhanVien).Distinct().ToList();
                    DateTime DateNow = DateTime.Now;
                    List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                    where a.isLuongHuu == false
                                                                    && listId.Contains(a.IdGuid)
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
                                                                        MaSoThue = a.MaSoThueTNCN,
                                                                        SoBHXH = a.SoBaoHiemXaHoi,
                                                                        NguyenQuan = a.NguyenQuan,
                                                                        NgayKetThucLamViec = a.NgayNghiViec,
                                                                        IdChucDanhEOS = a.IdChucDanh
                                                                    }
                                                                    ).OrderBy(a => a.MaNhanVien).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdCongTyEOS > 0)
                    {
                        var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                        listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.IdNhanVien))
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdGuid == aRequest.IdNhanVien).ToList();
                    }
                      if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
                    {
                        listNhanVien = listNhanVien.Where(a =>a.MaNhanVien.ToUpper().Contains( aRequest.KeyWord.ToUpper().Trim()) ||a.TenDayDu.ToUpper().Contains( aRequest.KeyWord.ToUpper().Trim()) ||(!string.IsNullOrEmpty(a.SoBHXH)&& a.SoBHXH.ToUpper().Contains( aRequest.KeyWord.ToUpper().Trim())) ).ToList();
                    }

                    if (listNhanVien != null && listNhanVien.Count > 0)
                    {
                        int count = listNhanVien.Count();

                        int CurrentPage = aRequest.CurrentPage;
                        int PageSize = aRequest.PageSize ?? 20;
                        List<DanhSachNhanVienViewModel> listNhanViens = listNhanVien.OrderBy(r => r.MaNhanVien).ThenByDescending(r => r.NgayBatDau).Skip(PageSize * (aRequest.CurrentPage - 1)).Take(PageSize).ToList();

                        var listLuongNhanVien = _dbContext.HRM_Luong_NhanVien.Where(a => listId.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= DateNow && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= DateNow)).ToList();
                        DateTime dt = DateTime.Now;
                        dt = new DateTime(dt.Year, dt.Month, 1);
                        dt = dt.AddMonths(1).AddDays(-1);
                        var lstCongTyAll = _CallWebService.GetDanhSachDuAn(_dbContext);
                        var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                        var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                        var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                        var listIdQuyTrinh_NhanVien_1 = listNhanVien.Select(x => x.IdGuid).ToList();
                        var listQuyTrinh_BoPhan_1 = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdQuyTrinh)).ToList();
                        foreach (var model in listNhanViens)
                        {
                            if (aRequest.NgayUnix > 0)
                            {
                                DateTime time = dungchung.UnixTimeStampToDateTime(aRequest.NgayUnix);
                            }
                            var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                            var _listBoPhan = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                            model.TenCongTy = string.Join("; ", _listBoPhan);
                            model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                            //model.TenChucVu = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                            //model.TenChucDam = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                            model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                            model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                            model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                            if (listChucVu != null && listChucVu.Count > 0)
                            {
                                var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == model.IdChucVuEOS);
                                if (boPhan_NhanVien != null)
                                {
                                    model.TenChucVu = boPhan_NhanVien.TenChucVu;

                                }

                            }
                            if (listChucDanh != null && listChucDanh.Count > 0)
                            {
                                var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == model.IdChucDanhEOS);
                                if (boPhan_NhanVien != null)
                                {
                                    model.TenChucDanh = boPhan_NhanVien.Ten;

                                }

                            }
                            if (listLuongNhanVien != null && listLuongNhanVien.Count > 0)
                            {

                                var LuongNV = listLuongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                                if (LuongNV != null)
                                {
                                    model.Luong = LuongNV.Luong;
                                    model.LuongBaoHiem = LuongNV.LuongHachToan;
                                }

                            }
                            if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                            {
                                var qt = QuaTrinhThamGias.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.TuNgay).ToList();
                                var BatDau = qt.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.TuNgay).FirstOrDefault();
                                if (BatDau != null)
                                {
                                    model.NgayBatDau = BatDau.TuNgay;

                                }
                                var KetThuc = qt.Where(a => a.IdNhanVien == model.IdGuid).OrderByDescending(a => a.TuNgay).FirstOrDefault();
                                if (KetThuc != null)
                                {
                                    model.NgayKetThuc = KetThuc.DenNgay;
                                    if (KetThuc.DenNgay.Year <= 1970)
                                        model.NgayKetThuc = dt;
                                }
                                else
                                    model.NgayKetThuc = dt;
                                model.TongThoiGianThamGia = 0;
                                foreach (var ngay in qt)
                                {
                                    if (ngay.isThamGia == false)
                                        continue;
                                    if (ngay.DenNgay.Year <= 1970)
                                        ngay.DenNgay = dt;
                                    double Day = (ngay.DenNgay - ngay.TuNgay).TotalDays + 1;
                                    int Thang = ((ngay.DenNgay.Year - ngay.TuNgay.Year) * 12) + ngay.DenNgay.Month - ngay.TuNgay.Month + 1;
                                    model.TongThoiGianThamGia += Thang;
                                }
                                if (model.TongThoiGianThamGia < 12)
                                    model.TongThoiGianTheoNam = model.TongThoiGianThamGia + " Tháng";
                                else
                                {
                                    int Nam = (int)(model.TongThoiGianThamGia / 12);
                                    model.TongThoiGianTheoNam = Nam + " năm " + (model.TongThoiGianThamGia - (Nam * 12)) + " tháng";
                                }
                            }

                            if (model.GioiTinh == "KhongXacDinh")
                                model.GioiTinh = "Khác";
                        }
                        result.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
                        result.items = listNhanViens;
                        result.value = lstBoPhanAll;
                    }
                }
                return result;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }
        public ErrorMessage ExportQuaTrinhThamGiaBaoHiemThatNghiep(ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuaTrinhThamGiaBaoHiemThatNghiep.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);


                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("QuaTrinhThamGiaBaoHiemThatNghiep.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportQuaTrinhThamGiaBaoHiemThatNghiep " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportQuaTrinhThamGiaBaoHiemThatNghiep(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                string[] ColumnNames = new string[] { "STT", "Mã nhân viên", "Tên nhân viên", "Mã số thuế", "Công ty", "Bộ phận", "Chức vụ" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "QUÁ TRÌNH THAM GIA BẢO HIỂM THẤT NGHIỆP"))
                {
                    return err;
                }
                var listdaTa = _dbContext.HRM_QuaTrinhThamGiaBaoHiemThatNghiep.AsNoTracking().ToList();
                var listNhanVien = _dbContext.HRM_NhanVien.ToList();
                List<HRM_QuaTrinhThamGiaBaoHiemThatNghiep> listAdd = new List<HRM_QuaTrinhThamGiaBaoHiemThatNghiep>();
                List<string> listMa = new List<string>();
                DateTime dt = DateTime.Now;
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_QuaTrinhThamGiaBaoHiemThatNghiep thuvien = new HRM_QuaTrinhThamGiaBaoHiemThatNghiep();

                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    thuvien.IdGuid = Guid.NewGuid().ToString();
                    thuvien.isTuThem = true;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == Ma);
                    if (NhanVien != null)
                    {
                        thuvien.IdNhanVien = NhanVien.IdGuid;
                        thuvien.IdUser = NhanVien.IdUser;
                    }
                    else
                        listMa.Add(Ma);
                    thuvien.TenCongTy = dungchung.GetCell(sheet1, row, 5);
                    thuvien.TenBoPhan = dungchung.GetCell(sheet1, row, 6);
                    thuvien.TenChucVu = dungchung.GetCell(sheet1, row, 7);
                    thuvien.TuNgay = dungchung.GetCellToDateTime(sheet1, row, 8) ?? dt;
                    thuvien.DenNgay = dungchung.GetCellToDateTime(sheet1, row, 9) ?? dt;
                    listAdd.Add(thuvien);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("mã nhân viên không tồn tại: " + string.Join(", ", listAdd));
                    return err;
                }


                _dbContext.HRM_QuaTrinhThamGiaBaoHiemThatNghiep.AddRange(listAdd);

                s.Close();
                _dbContext.SaveChanges();
                //err.Data = thuvien;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportQuaTrinhThamGiaBaoHiemThatNghiep " + e.ToString());
            }
            return err;
        }
        public ErrorMessage GetQuaTrinhThamGiaBaoHiemThatNghiep(ApplicationDbContext _dbContext, string IdNhanVien)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {

                var listdaTa = _dbContext.HRM_QuaTrinhThamGiaBaoHiemThatNghiep.Where(a => a.IdNhanVien == IdNhanVien).OrderBy(a => a.TuNgay).ToList();
                Parallel.ForEach(listdaTa, item =>
                {
                    if (item.TuNgay != null)
                        item.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.TuNgay);
                    if (item.DenNgay != null)
                        item.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.DenNgay);

                }
                    );
                err.Data = listdaTa;

            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportQuaTrinhThamGiaBaoHiemThatNghiep " + e.ToString());
            }
            return err;
        }

        public dynamic GetDanhSachNhanVienForBaoHiemThatNghiep(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                var QuaTrinhThamGias = _dbContext.HRM_QuaTrinhThamGiaBaoHiemThatNghiep.ToList();
                if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                {
                    List<string> listId = QuaTrinhThamGias.Select(a => a.IdNhanVien).Distinct().ToList();

                    DateTime DateNow = DateTime.Now;
                    List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                    where a.isLuongHuu == false
                                                                    && listId.Contains(a.IdGuid)
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
                                                                        MaSoThue = a.MaSoThueTNCN,
                                                                        SoBHXH = a.SoBaoHiemXaHoi,
                                                                        NguyenQuan = a.NguyenQuan,
                                                                        NgayKetThucLamViec = a.NgayNghiViec,
                                                                        IdChucDanhEOS = a.IdChucDanh
                                                                    }
                                                                    ).OrderBy(a => a.MaNhanVien).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdCongTyEOS > 0)
                    {
                        var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                        listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                    }
                    if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.IdNhanVien))
                    {
                        listNhanVien = listNhanVien.Where(a => a.IdGuid == aRequest.IdNhanVien).ToList();
                    }

                    if (listNhanVien != null && listNhanVien.Count > 0)
                    {
                        int count = listNhanVien.Count();

                        int CurrentPage = aRequest.CurrentPage;
                        int PageSize = aRequest.PageSize ?? 20;
                        List<DanhSachNhanVienViewModel> listNhanViens = listNhanVien.OrderBy(r => r.MaNhanVien).ThenByDescending(r => r.NgayBatDau).Skip(PageSize * (aRequest.CurrentPage - 1)).Take(PageSize).ToList();

                        var listLuongNhanVien = _dbContext.HRM_Luong_NhanVien.Where(a => listId.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= DateNow && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= DateNow)).ToList();

                        var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                        var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                        var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                        foreach (var model in listNhanViens)
                        {

                            model.TenCongTy = lstBoPhanAll.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                            model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                            //model.TenChucVu = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                            //model.TenChucDam = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                            model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                            model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                            model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                            if (listChucVu != null && listChucVu.Count > 0)
                            {
                                var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == model.IdChucVuEOS);
                                if (boPhan_NhanVien != null)
                                {
                                    model.TenChucVu = boPhan_NhanVien.TenChucVu;

                                }

                            }
                            if (listChucDanh != null && listChucDanh.Count > 0)
                            {
                                var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == model.IdChucDanhEOS);
                                if (boPhan_NhanVien != null)
                                {
                                    model.TenChucDanh = boPhan_NhanVien.Ten;

                                }

                            }
                            if (listLuongNhanVien != null && listLuongNhanVien.Count > 0)
                            {

                                var LuongNV = listLuongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                                if (LuongNV != null)
                                {
                                    model.Luong = LuongNV.Luong;
                                    model.LuongBaoHiem = LuongNV.LuongHachToan;
                                }

                            }
                            if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                            {

                                var BatDau = QuaTrinhThamGias.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.TuNgay).FirstOrDefault();
                                if (BatDau != null)
                                {
                                    model.NgayBatDau = BatDau.TuNgay;

                                }
                                var KetThuc = QuaTrinhThamGias.Where(a => a.IdNhanVien == model.IdGuid).OrderByDescending(a => a.DenNgay).FirstOrDefault();
                                if (KetThuc != null)
                                {
                                    model.NgayKetThuc = KetThuc.DenNgay;

                                }

                            }

                            if (model.GioiTinh == "KhongXacDinh")
                                model.GioiTinh = "Khác";
                        }



                        result.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
                        result.items = listNhanViens;
                        //result.value = lstBoPhanAll;
                    }
                }
                return result;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }
        public ErrorMessage UpdateQuaTrinhThamGiaBaoHiemThatNghiep(ApplicationDbContext _dbContext, HRM_QuaTrinhThamGiaBaoHiemThatNghiep NhanVien)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            try
            {

                NhanVien.TuNgay = dungchung.UnixTimeStampToDateTime(NhanVien.TuNgayUnix);
                NhanVien.DenNgay = dungchung.UnixTimeStampToDateTime(NhanVien.DenNgayUnix);



                _dbContext.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportQuaTrinhThamGiaBaoHiemThatNghiep " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ExportThongTinHuongCheDoThaiSan(ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ThongTinHuongCheDoThaiSan.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);


                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("ThongTinHuongCheDo.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportThongTinHuongCheDoThaiSan " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportThongTinHuongCheDoThaiSan(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                string[] ColumnNames = new string[] { "STT", "Mã nhân viên", "Tên nhân viên", "Mã số thuế", "Công ty", "Bộ phận", "Chức vụ" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "Thông tin Hưởng chế độ"))
                {
                    return err;
                }
                var listdaTa = _dbContext.HRM_ThongTinHuongCheDoThaiSan.AsNoTracking().ToList();
                var listNhanVien = _dbContext.HRM_NhanVien.ToList();
                List<HRM_ThongTinHuongCheDoThaiSan> listAdd = new List<HRM_ThongTinHuongCheDoThaiSan>();
                List<string> listMa = new List<string>();
                DateTime dt = DateTime.Now;
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_ThongTinHuongCheDoThaiSan thuvien = new HRM_ThongTinHuongCheDoThaiSan();

                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    thuvien.IdGuid = Guid.NewGuid().ToString();
                    thuvien.isTuThem = true;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == Ma);
                    if (NhanVien != null)
                    {
                        thuvien.IdNhanVien = NhanVien.IdGuid;
                        thuvien.IdUser = NhanVien.IdUser;
                    }
                    else
                        listMa.Add(Ma);
                    thuvien.TuNgay = dungchung.GetCellToDateTime(sheet1, row, 8) ?? dt;
                    thuvien.DenNgay = dungchung.GetCellToDateTime(sheet1, row, 9) ?? dt;
                    thuvien.CheDo = dungchung.GetCell(sheet1, row, 10);
                    thuvien.SoTien = dungchung.GetCellToDouble(sheet1, row, 11);
                    thuvien.LuongBHXH = dungchung.GetCellToDouble(sheet1, row, 12);

                    listAdd.Add(thuvien);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("mã nhân viên không tồn tại: " + string.Join(", ", listAdd));
                    return err;
                }


                _dbContext.HRM_ThongTinHuongCheDoThaiSan.AddRange(listAdd);

                s.Close();
                _dbContext.SaveChanges();
                //err.Data = thuvien;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThongTinHuongCheDoThaiSan " + e.ToString());
            }
            return err;
        }
        public ErrorMessage GetThongTinHuongCheDoThaiSan(ApplicationDbContext _dbContext, string IdNhanVien)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {

                var listdaTa = _dbContext.HRM_ThongTinHuongCheDoThaiSan.Where(a => a.IdNhanVien == IdNhanVien).OrderBy(a => a.TuNgay).ToList();
                Parallel.ForEach(listdaTa, item =>
                {
                    if (item.TuNgay != null)
                        item.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.TuNgay);
                    if (item.DenNgay != null)
                        item.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.DenNgay);

                }
                    );
                err.Data = listdaTa;

            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThongTinHuongCheDoThaiSan " + e.ToString());
            }
            return err;
        }
        public dynamic GetDanhSachNhanVienForThongTinHuong(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                DateTime DateNow = DateTime.Now;
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                join b in _dbContext.HRM_ThongTinHuongCheDoThaiSan on a.IdGuid equals b.IdNhanVien
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
                                                                    MaSoThue = a.MaSoThueTNCN,
                                                                    SoBHXH = a.SoBaoHiemXaHoi,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh,
                                                                    NgayBatDau = b.TuNgay,
                                                                    NgayKetThuc = b.DenNgay,
                                                                    CheDo = b.CheDo,
                                                                    SoTien = b.SoTien,
                                                                }
                                                                ).OrderBy(a => a.MaNhanVien).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdCongTyEOS > 0)
                {
                    var listIdNhanVien = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.IdNhanVien))
                {
                    listNhanVien = listNhanVien.Where(a => a.IdGuid == aRequest.IdNhanVien).ToList();
                }

                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    int count = listNhanVien.Count();

                    int CurrentPage = aRequest.CurrentPage;
                    int PageSize = aRequest.PageSize ?? 20;
                    List<DanhSachNhanVienViewModel> listNhanViens = listNhanVien.OrderBy(r => r.MaNhanVien).ThenByDescending(r => r.NgayBatDau).Skip(PageSize * (aRequest.CurrentPage - 1)).Take(PageSize).ToList();
                    var listId = listNhanViens.Select(a => a.IdGuid).Distinct().ToList();
                    var listLuongNhanVien = _dbContext.HRM_Luong_NhanVien.Where(a => listId.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= DateNow && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= DateNow)).ToList();

                    var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                    foreach (var model in listNhanViens)
                    {

                        model.TenCongTy = lstBoPhanAll.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        //model.TenChucVu = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                        //model.TenChucDam = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                        model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                        model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                        model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                        if (listChucVu != null && listChucVu.Count > 0)
                        {
                            var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == model.IdChucVuEOS);
                            if (boPhan_NhanVien != null)
                            {
                                model.TenChucVu = boPhan_NhanVien.TenChucVu;

                            }

                        }
                        if (listChucDanh != null && listChucDanh.Count > 0)
                        {
                            var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == model.IdChucDanhEOS);
                            if (boPhan_NhanVien != null)
                            {
                                model.TenChucDanh = boPhan_NhanVien.Ten;

                            }

                        }
                        if (listLuongNhanVien != null && listLuongNhanVien.Count > 0)
                        {

                            var LuongNV = listLuongNhanVien.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                            if (LuongNV != null)
                            {
                                model.Luong = LuongNV.Luong;
                                model.LuongBaoHiem = LuongNV.LuongHachToan;
                            }

                        }
                        //if (QuaTrinhThamGias != null && QuaTrinhThamGias.Count > 0)
                        //{

                        //    var BatDau = QuaTrinhThamGias.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                        //    if (BatDau != null)
                        //    {
                        //        model.NgayBatDau = BatDau.TuNgay;

                        //    }
                        //    var KetThuc = QuaTrinhThamGias.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                        //    if (KetThuc != null)
                        //    {
                        //        model.NgayKetThuc = KetThuc.DenNgay;

                        //    }

                        //}

                        if (model.GioiTinh == "KhongXacDinh")
                            model.GioiTinh = "Khác";
                    }



                    result.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
                    result.items = listNhanViens;
                    result.value = lstBoPhanAll;
                }

                return result;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }
    }
}