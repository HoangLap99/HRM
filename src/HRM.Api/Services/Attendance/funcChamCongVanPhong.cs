#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using HRM.Api.Models.SmartEOEModel;

using HRM.Api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
namespace HRM.Api.Services.ChamCong
{

    // Chuyển quy trình sang SMartEOS
        public class funcChamCongVanPhong : FuncBase
    {
        public funcChamCongVanPhong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<funcChamCongVanPhong> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        #region Hành Chính văn Phòng
        public dynamic GetNextSoQuyTrinhXinDiMuonVeSom(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "XDMVS",
                db.HRM_QuyTrinhXinDiMuonVeSom.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListQuyTrinhXinDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_QuyTrinhXinDiMuonVeSom> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyTrinhXinDiMuonVeSom
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhXinDiMuonVeSom
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.NgayDangKy >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayDangKy < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => (!string.IsNullOrEmpty(a.SoQuyTrinh) && a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.GhiChu) && a.GhiChu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));

                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_QuyTrinhXinDiMuonVeSom" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> CongTys = _CallWebService.GetDanhSachCongTy(db);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                List<string> listIdQuyTrinh = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listItem = db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.Where(a => listIdQuyTrinh.Contains(a.IdQuyTrinh)).ToList();
                List<string> listIdNV=(listItem.Select(a => a.IdNhanVien).Distinct().ToList());
                var listNV = db.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdUser) || listIdNV.Contains(a.IdGuid)).Distinct().ToList();
                var listIdUserCreatedBy = listItem.GroupBy(q => q.CreatedBy).Select(q => q.Key).ToList();
                var listUser = _CallWebService.GetDanhSachUserTheoListIdUser(db, listIdUserCreatedBy);
                foreach (HRM_QuyTrinhXinDiMuonVeSom item in itemList)
                {
                    item.ItemUserCreatedBy = listUser.FirstOrDefault(q => q.Id == item.CreatedBy);
                    item.NgayDangKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDangKy);
                    BoPhan duAn = CongTys.Find(x => x.Id == item.IdCongTyEOS);
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
                    if (listItem != null && listItem.Count > 0)
                    {
                        var Items = listItem.Where(a => a.IdQuyTrinh == item.IdGuid).ToList();
                        List<string> list = new List<string>();
                        foreach (var itemchild in Items)
                        {
                            if (listNV != null && listNV.Count > 0)
                            {
                                var nhanVien = listNV.FirstOrDefault(a => a.IdGuid == itemchild.IdNhanVien);
                                if (nhanVien != null)
                                    list.Add(nhanVien.TenDayDu + "(" + itemchild.TongGio + " Giờ)");
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
        public dynamic GetQuyTrinhXinDiMuonVeSom(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                HRM_QuyTrinhXinDiMuonVeSom QuyTrinh = db.HRM_QuyTrinhXinDiMuonVeSom.FirstOrDefault(a => a.IdGuid == Id);
                if (QuyTrinh == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                if (!base.GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))
                    return err;
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
                QuyTrinh.NgayDangKyUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.NgayDangKy);
                QuyTrinh.listNhanVien = new List<HRM_QuyTrinhXinDiMuonVeSom_NhanVien>();
                QuyTrinh.listNhanVien = db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.Where(a => a.IdQuyTrinh == Id).ToList();
                if (QuyTrinh.listNhanVien.Count > 0)
                {
                    List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    foreach (var item in QuyTrinh.listNhanVien)
                    {
                        item.DateTimetoUnix();
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        if (nhanVien != null)
                        {
                            item.MaNhanVien = nhanVien.MaNhanVien;
                            item.TenNhanVien = nhanVien.TenDayDu;

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
                    }
                }
                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }
        public dynamic SetQuyTrinhXinDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhXinDiMuonVeSom model = JsonConvert.DeserializeObject<HRM_QuyTrinhXinDiMuonVeSom>(data.ToString());
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    // DI: use inherited _CallWebService
                    var QuyTrinhdb = db.HRM_QuyTrinhXacNhanNgoaiGio.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = UpdateQuyTrinhXinDiMuonVeSom(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }
                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, model, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }


                    db.SaveChanges();
                   
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
                }
            }
        }

        public bool UpdateQuyTrinhXinDiMuonVeSom(HRM_QuyTrinhXinDiMuonVeSom QuyTrinh, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                if (QuyTrinh.listIdBoPhan != null && QuyTrinh.listIdBoPhan.Count > 0)
                    QuyTrinh.IdBoPhan = string.Join(",", QuyTrinh.listIdBoPhan);
                if (QuyTrinh.NgayDangKyUnix > 0)
                    QuyTrinh.NgayDangKy = dungchung.UnixTimeStampToDateTime(QuyTrinh.NgayDangKyUnix);
                if (string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {
                    QuyTrinh.SoQuyTrinh = GetNextSoQuyTrinhXinDiMuonVeSom(db).SoPhieu.ToString();
                    //model.IdGuid = Guid.NewGuid().ToString();
                    QuyTrinh.eAction = DefineData.eAction.QUYTRINHDIMUONVESOM.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return err.Value;
                    QuyTrinh.Modified = DateTime.Now;
                    db.HRM_QuyTrinhXinDiMuonVeSom.Add(QuyTrinh);
                }
                else
                {
                    int Check = db.HRM_QuyTrinhXinDiMuonVeSom.Where(r => r.SoQuyTrinh == QuyTrinh.SoQuyTrinh && r.IdGuid != QuyTrinh.IdGuid).AsNoTracking().Count();
                    if (Check > 0)
                    {
                        var MaPhieu = GetNextSoQuyTrinhXinDiMuonVeSom(db).SoPhieu.ToString();
                    }
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListUser = db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.Where(r => r.IdQuyTrinh == QuyTrinh.IdGuid).ToList();
                db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.RemoveRange(ListUser);
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
                        bp.TableName = nameof(HRM_QuyTrinhXinDiMuonVeSom);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }

                if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0) 
                {
                    foreach (var item in QuyTrinh.listNhanVien)
                    {
                        item.UnixToDateTime();
                        item.DenGio = new DateTime(item.DenGio.Value.Year, item.DenGio.Value.Month, item.DenGio.Value.Day, item.DenGio.Value.Hour, item.DenGio.Value.Minute, 0);
                        item.TuGio = new DateTime(item.TuGio.Value.Year, item.TuGio.Value.Month, item.TuGio.Value.Day, item.TuGio.Value.Hour, item.TuGio.Value.Minute, 0);
                        TimeSpan timeSpan = item.DenGio.Value - item.TuGio.Value;
                        item.TongGio = timeSpan.TotalHours;
                        if (QuyTrinh.DangKyHo == false && string.IsNullOrEmpty(item.IdNhanVien))
                            item.IdNhanVien = Guid.NewGuid().ToString();
                       
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = QuyTrinh.IdGuid;
                        item.IdCongTyEOS = QuyTrinh.IdCongTyEOS;

                    }
                    db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.AddRange(QuyTrinh.listNhanVien);
                }
                if (QuyTrinh.isKetThuc == true)
                {
                    List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listDLdb = db.HRM_TheoDoiDiMuonVeSom.Where(a => a.NgayDangKy == QuyTrinh.NgayDangKy && listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    if (listDLdb != null && listDLdb.Count > 0)
                        db.HRM_TheoDoiDiMuonVeSom.RemoveRange(listDLdb);
                    List<HRM_TheoDoiDiMuonVeSom> listAdd = new List<HRM_TheoDoiDiMuonVeSom>();
                    foreach (var item in QuyTrinh.listNhanVien)
                    {
                        HRM_TheoDoiDiMuonVeSom itemAdd = new HRM_TheoDoiDiMuonVeSom(item);
                        itemAdd.IdGuid = Guid.NewGuid().ToString();
                        itemAdd.NgayDangKy = QuyTrinh.NgayDangKy;
                        listAdd.Add(itemAdd);
                    }
                    if (listAdd != null && listAdd.Count > 0)
                        db.HRM_TheoDoiDiMuonVeSom.AddRange(listAdd);

                }
                db.SaveChanges();
                if (!base.SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return false;
                return true;

            }
            catch (Exception ex)
            {
                err= new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
                return false;
            }

        }
        public dynamic DeleteQuyTrinhXinDiMuonVeSom(ApplicationDbContext db, string Id)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhXinDiMuonVeSom QuyTrinh = db.HRM_QuyTrinhXinDiMuonVeSom.FirstOrDefault(a => a.IdGuid == Id);
                    if (QuyTrinh == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Quy trình không tồn tại");
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    var ListUser = db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.Where(r => r.IdQuyTrinh == Id).ToList();
                    db.HRM_QuyTrinhXinDiMuonVeSom_NhanVien.RemoveRange(ListUser);
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
        public dynamic ChuyenTiepQuyTrinhXinDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhXinDiMuonVeSom QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_QuyTrinhXinDiMuonVeSom)) as HRM_QuyTrinhXinDiMuonVeSom;

                   
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công danh sách không có lương");
                    HRM_QuyTrinhXinDiMuonVeSom pcuDatabase = db.HRM_QuyTrinhXinDiMuonVeSom.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    QuyTrinh.NoiDung = QuyTrinh.GhiChu;
                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    bool Error = UpdateQuyTrinhXinDiMuonVeSom(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
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
        public dynamic KhongDuyetQuyTrinhXinDiMuonVeSom(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhXinDiMuonVeSom QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_QuyTrinhXinDiMuonVeSom)) as HRM_QuyTrinhXinDiMuonVeSom;

                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_QuyTrinhXinDiMuonVeSom pcuDatabase = db.HRM_QuyTrinhXinDiMuonVeSom.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = UpdateQuyTrinhXinDiMuonVeSom(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
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

        #endregion
        #region Import file Chấm công nhân viên 
        #endregion
    }
}
