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


        //public List<LuongHoTroLamDuAnXa> GetHoTroDuAnXaChamCongVanPhong(int Thang, int Nam, List<long> lstIdTongHopChamCong, ApplicationDbContext db)
        //{
        //    byte TrangThaiPheDuyet = (byte)DefineData.eTrangThaiQuyTrinhCongTacPhiTongHop.GiamDocDaDuyet;

        //    var lstLuongHoTroLamDuAnXa = (from ccth in smdb.HRM_ChamCongVanPhongTongHop
        //                                  join cc in smdb.HRM_ChamCongVanPhong
        //                                  on ccth.Id equals cc.IdChamCongVanPhongTongHop
        //                                  join ct in smdb.HRM_ChamCongVanPhongChiTiet
        //                                  on cc.Id equals ct.IdChamCongVanPhong
        //                                  join ctn in smdb.HRM_ChamCongVanPhongChiTietNgay
        //                                  on ct.Id equals ctn.IdChamCongVanPhongChiTiet
        //                                  where ctn.isHoTroDuAn == true
        //                                  && ccth.Thang == Thang && ccth.Nam == Nam
        //                                  && ccth.TrangThai == TrangThaiPheDuyet
        //                                  && lstIdTongHopChamCong.Contains(ccth.Id)
        //                                  select new LuongHoTroLamDuAnXa
        //                                  {
        //                                      IdNhanVien = cc.IdNhanVienKeKhai,
        //                                      SoNgay = ctn.NgayLuong,
        //                                      IdHopDong = ct.IdHopDong,
        //                                  }).ToList();

        //    var lstIdHopDong = lstLuongHoTroLamDuAnXa.Select(x => x.IdHopDong).ToList();
        //    var lstHopDongNhanVien = db.HRM_HopDong.Where(x => lstIdHopDong.Contains(x.IdGuid)).ToList();

        //    //foreach (var item in lstLuongHoTroLamDuAnXa)
        //    //{
        //    //    var itemHopDong = lstHopDongNhanVien.FirstOrDefault(x => x.IdGuid == item.IdHopDong);
        //    //    if (itemHopDong != null)
        //    //    {
        //    //        item.TongTienThucTeHoTroDuAn = itemHopDong.HoTroDuAn * item.SoNgay;
        //    //        item.TongTienHachToanHoTroDuAn = itemHopDong.HachToanHoTroDuAn * item.SoNgay;
        //    //    }
        //    //}

        //    return lstLuongHoTroLamDuAnXa;
        //}

        //public List<LuongHoTroLamDuAnXa> GetHoTroDuAnXaChamCongDuAn(int Thang, int Nam, List<long> lstIdChamCongDuAn, ApplicationDbContext db)
        //{
        //    byte TrangThaiPheDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.GiamDocDaDuyet;
        //    var lstLuongHoTroLamDuAnXa = (from cc in smdb.HRM_ChamCongDuAn
        //                                  join ct in smdb.HRM_ChamCongDuAnChiTiet
        //                                  on cc.Id equals ct.IdChamCongDuAn
        //                                  join ctn in smdb.HRM_ChamCongDuAnChiTietNgay
        //                                  on ct.Id equals ctn.IdChamCongDuAnChiTiet
        //                                  where ctn.isHoTroCongTacXa == true
        //                                  && cc.Thang == Thang && cc.Nam == Nam
        //                                  && cc.TrangThai == TrangThaiPheDuyet
        //                                  && lstIdChamCongDuAn.Contains(cc.Id)
        //                                  select new LuongHoTroLamDuAnXa
        //                                  {
        //                                      IdNhanVien = cc.IdNhanVienKeKhai,
        //                                      SoNgay = ctn.NgayLuong,
        //                                      IdHopDongDuAn = ct.IdHopDong,
        //                                      IdBoNhiemDieuDongDuAn = ct.IdBoNhiemDieuDongDuAn
        //                                  }).ToList();

        //    var lstIdHopDong = lstLuongHoTroLamDuAnXa.Select(x => x.IdHopDongDuAn).ToList();
        //    var lstHopDongNhanVien = db.HRM_HopDong.Where(x => lstIdHopDong.Contains(x.IdGuid)).ToList();

        //    var lstIHoNhiemDieuDong = lstLuongHoTroLamDuAnXa.Select(x => x.IdBoNhiemDieuDongDuAn).ToList();
        //    var lstBoNhiemDieuDong = db.HRM_BoNhiemDieuDongDuAn.Where(x => lstIHoNhiemDieuDong.Contains(x.IdGuid)).ToList();

        //    //foreach (var item in lstLuongHoTroLamDuAnXa)
        //    //{
        //    //    if (!string.IsNullOrEmpty(item.IdBoNhiemDieuDongDuAn))
        //    //    {
        //    //        var itemQuyetDinhBoNhiemDieuDong = lstBoNhiemDieuDong.FirstOrDefault(x => x.IdGuid == item.IdHopDong);
        //    //        if (itemQuyetDinhBoNhiemDieuDong != null)
        //    //        {
        //    //            item.TongTienThucTeHoTroDuAn = itemQuyetDinhBoNhiemDieuDong.HoTroLamViecTuXa * item.SoNgay;
        //    //            item.TongTienHachToanHoTroDuAn = itemQuyetDinhBoNhiemDieuDong.HachToanHoTroLamViecTuXa * item.SoNgay;
        //    //        }
        //    //    }
        //    //    else
        //    //    if (!string.IsNullOrEmpty(item.IdHopDongDuAn))
        //    //    {
        //    //        var itemHopDong = lstHopDongNhanVien.FirstOrDefault(x => x.IdGuid == item.IdHopDong);
        //    //        //if (itemHopDong != null)
        //    //        //{
        //    //        //    item.TongTienThucTeHoTroDuAn = itemHopDong.HoTroDuAn * item.SoNgay;
        //    //        //    item.TongTienHachToanHoTroDuAn = itemHopDong.HachToanHoTroDuAn * item.SoNgay;
        //    //        //}
        //    //    }
        //    //}

        //    return lstLuongHoTroLamDuAnXa;
        //}

        //public dynamic ImportBangChamCongVanPhong(int Thang, int Nam, string idCurrentUser, ApplicationDbContext db)
        //{
        //    string pathTemplate = @"C:\uploader\App_Data\uploads\BangChamCong.xlsx";
        //    Stream s = File.OpenRead(pathTemplate);
        //    ExcelPackage package = new ExcelPackage(s);
        //    s.Close();

        //    var lstNhanVien = db.HRM_NhanVien.ToList();
        //    var sheet1 = package.Workbook.Worksheets["Sheet1"];
        //    int endrow1 = sheet1.Dimension.Rows;
        //    //  string sResult = "";

        //    var Now = DateTime.Now;

        //    var CurrentUser = db.HRM_NhanVien.FirstOrDefault(x => x.IdUser == idCurrentUser);

        //    //if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        //    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy user");

        //    var NgayBatDauThang = new DateTime(Nam, Thang, 1);
        //    var NgayKetThucThang = new DateTime(Nam, Thang, 1).AddMonths(1);
        //    var DateMin = dungchung.UnixTimeStampToDateTime(0);

        //    var lstHRMLinkChucVuBoPhan = db.HRM_Link_NhanVien_BoPhan_ChucVu.Where(x => x.NgayBatDau <= NgayBatDauThang
        //    && (x.NgayKetThuc >= NgayBatDauThang || x.NgayKetThuc == DateMin)).ToList();
        //    var lstLinkChucVuBoPhan = smdb.Link_NhanVien_BoPhan_ChucVu
        //        .Where(x => x.NgayBatDau <= NgayBatDauThang && (x.NgayKetThuc >= NgayBatDauThang || x.NgayKetThuc == DateMin)).ToList();

        //    var itemChamCongTongHop = smdb.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.Thang == Thang && x.Nam == Nam);
        //    if (itemChamCongTongHop == null)
        //    {
        //        itemChamCongTongHop = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongTongHop();
        //        itemChamCongTongHop.Nam = Nam;
        //        itemChamCongTongHop.Thang = Thang;
        //        itemChamCongTongHop.TrangThai = (byte)DefineData.eTrangThaiChamCongVanPhongTongHop.GiamDocDaDuyet;
        //        itemChamCongTongHop.Created = Now;
        //        itemChamCongTongHop.Modified = Now;
        //        smdb.Entry(itemChamCongTongHop).State = Microsoft.EntityFrameworkCore.EntityState.Added;
        //        try
        //        {
        //            smdb.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            DefineData.logger.ErrorFormat("ImportBangChamCongVanPhong ChamCongTongHop: {0}, Error {1}", JsonConvert.SerializeObject(itemChamCongTongHop), ex.Message);
        //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //        }
        //    }

        //    long IdChamCongVanPhongTongHop = itemChamCongTongHop.Id;
        //    byte TrangThaiChamCongVanPhong = (byte)DefineData.eTrangThaiChamCongVanPhong.DaTongHop;

        //    Dictionary<long, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhong> dicNhanVienChamCong = new Dictionary<long, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhong>();



        //    for (int row = 4; row <= endrow1; row++)
        //    {
        //        string MaNhanVien = sheet1.Cells[row, 1].Value == null || sheet1.Cells[row, 1].Value.ToString() == "NULL" ? "" : sheet1.Cells[row, 1].Value.ToString();
        //        var itemNhanVien = lstNhanVien.FirstOrDefault(x => x.MaNhanVien == MaNhanVien);
        //        if (itemNhanVien == null)
        //            continue;

        //        string sNgayChamCong = sheet1.Cells[row, 5].Value == null || sheet1.Cells[row, 5].Value.ToString() == "NULL" ? "" : sheet1.Cells[row, 5].Value.ToString();
        //        DateTime NgayChamCong;
        //        if (!DateTime.TryParse(sNgayChamCong, out NgayChamCong))
        //        {
        //            continue;
        //        }

        //        if (NgayChamCong < NgayBatDauThang || NgayChamCong >= NgayKetThucThang)
        //            continue;

        //        string sSoGio = sheet1.Cells[row, 25].Value == null || sheet1.Cells[row, 25].Value.ToString() == "NULL" ? "" : sheet1.Cells[row, 25].Value.ToString();

        //        float SoGio = 0;
        //        float.TryParse(sSoGio, out SoGio);

        //        long IdNhanVien = itemNhanVien.IdNhanVienEOS;
        //        long IdChucVu = 0;
        //        long IdBoPhan = 0;

        //        var ChamCong = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhong();
        //        if (!dicNhanVienChamCong.ContainsKey(IdNhanVien))
        //        {

        //            var itemLink = lstLinkChucVuBoPhan.OrderByDescending(x => x.Created).FirstOrDefault(x => x.NhanVienPkid == itemNhanVien.IdNhanVienEOS);
        //            if (itemLink != null)
        //            {
        //                IdChucVu = itemLink.ChucVuPkid;
        //                IdBoPhan = itemLink.NhanVienPkid;
        //            }
        //            ChamCong.IdBoPhanKeKhai = IdBoPhan;
        //            ChamCong.IdChamCongVanPhongTongHop = IdChamCongVanPhongTongHop;
        //            ChamCong.IdNhanVienKeKhai = itemNhanVien.IdNhanVienEOS;
        //            ChamCong.Nam = Nam;
        //            ChamCong.NgayCongTinhLuong = 25;
        //            ChamCong.TongNgayLamViec = 0;
        //            ChamCong.TrangThai = TrangThaiChamCongVanPhong;
        //            ChamCong.Created = Now;
        //            ChamCong.Modified = Now;
        //            smdb.Entry(ChamCong).State = Microsoft.EntityFrameworkCore.EntityState.Added;
        //            smdb.SaveChanges();
        //            smdb.Entry(ChamCong).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        //            dicNhanVienChamCong.Add(IdNhanVien, ChamCong);
        //        }
        //        else
        //        {
        //            ChamCong = dicNhanVienChamCong[IdNhanVien];
        //        }

        //        long IdChamCong = ChamCong.Id;

        //        HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTiet ChamCongChiTiet = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTiet();
        //        ChamCongChiTiet.IdChamCongVanPhong = IdChamCong;
        //        ChamCongChiTiet.NgayThang = NgayChamCong;
        //        ChamCongChiTiet.Ngay = (int)NgayChamCong.Day;
        //        ChamCongChiTiet.IdChamCongVanPhongTongHop = IdChamCongVanPhongTongHop;

        //        var itemLinkNhanVienChucVuBoPhan = lstLinkChucVuBoPhan.OrderByDescending(x => x.Created)
        //            .FirstOrDefault(x => x.NgayBatDau <= NgayChamCong && (x.NgayKetThuc >= NgayChamCong || x.NgayKetThuc == DateMin));
        //        if (itemLinkNhanVienChucVuBoPhan != null)
        //        {
        //            var itemHRMLinkChucVuBoPhan = lstHRMLinkChucVuBoPhan.FirstOrDefault(x => x.IdLink_NhanVien_BoPhan_ChucVuEOS == itemLinkNhanVienChucVuBoPhan.Id);

        //            if (itemHRMLinkChucVuBoPhan == null)
        //                continue;

        //            ChamCongChiTiet.IdHopDong = itemHRMLinkChucVuBoPhan.IdHopDong;
        //        }

        //        ChamCongChiTiet.Created = Now;
        //        ChamCongChiTiet.Modified = Now;
        //        smdb.Entry(ChamCongChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Added;
        //        smdb.SaveChanges();


        //        var IdChamCongChiTiet = ChamCongChiTiet.Id;

        //        //DateTime NgayChamCong = ChiTiet.NgayThang;
        //        HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgaySang = GetHRMChamCongVanPhongChiTietNgay(ChamCongChiTiet);
        //        ChiTietNgaySang.SoGioLam = DefineData.ThoiGianLamViecMotBuoi;
        //        ChiTietNgaySang.SangChieu = DefineData.eLoaiSangChieu.Sang.ToString();
        //        HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgayChieu = GetHRMChamCongVanPhongChiTietNgay(ChamCongChiTiet);
        //        ChiTietNgayChieu.SoGioLam = DefineData.ThoiGianLamViecMotBuoi;
        //        ChiTietNgayChieu.SangChieu = DefineData.eLoaiSangChieu.Chieu.ToString();

        //        if (SoGio > 0)
        //        {
        //            smdb.Entry(ChiTietNgaySang).State = Microsoft.EntityFrameworkCore.EntityState.Added;
        //            XuLyChamCongChiTietNgay(ChiTietNgaySang, SoGio > DefineData.ThoiGianLamViecMotBuoi ? DefineData.ThoiGianLamViecMotBuoi : SoGio, DefineData.ThoiGianLamViecMotBuoi);

        //            ChamCong.TongNgayLamViec += ChiTietNgaySang.NgayLuong;
        //            if (SoGio > DefineData.ThoiGianLamViecMotBuoi)
        //            {
        //                XuLyChamCongChiTietNgay(ChiTietNgayChieu, SoGio - DefineData.ThoiGianLamViecMotBuoi, DefineData.ThoiGianLamViecMotBuoi);
        //                ChamCong.TongNgayLamViec += ChiTietNgayChieu.NgayLuong;
        //                smdb.Entry(ChiTietNgayChieu).State = Microsoft.EntityFrameworkCore.EntityState.Added;
        //            }
        //        }
        //        smdb.SaveChanges();

        //    }

        //    // cập nhật lại ngày làm việc nhân viên

        //    foreach (var item in dicNhanVienChamCong)
        //    {
        //        smdb.Entry(item.Value).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //    }

        //    smdb.SaveChanges();

        //    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");




        //}

        //public HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay GetHRMChamCongVanPhongChiTietNgay(HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTiet ChiTiet)
        //{
        //    HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay item = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay();
        //    item.IdChamCongVanPhong = ChiTiet.IdChamCongVanPhong;
        //    item.IdChamCongVanPhongChiTiet = ChiTiet.Id;
        //    item.Created = DateTime.Now;
        //    item.Modified = DateTime.Now;
        //    return item;
        //}

        //public void XuLyChamCongChiTietNgay(HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTiet, float ThoiGianLam, float ThoiGianLamMotBuoi)
        //{
        //    ChiTiet.NgayLuong = ThoiGianLam / (ThoiGianLamMotBuoi * 2);
        //    ChiTiet.SoGioLam = ThoiGianLam;
        //}

        //public List<LuongNghiPhepNghiBuTuNgayDenNgay> GetNghiPhepNghiBuTuNgayDenNgay(DateTime TuNgay, DateTime DenNgay, List<long> lstIdTongHopChamCong
        //    , List<string> lstIdLoaiNghiPhepNghiBu, ApplicationDbContext db)
        //{


        //    byte TrangThaiPheDuyet = (byte)DefineData.eTrangThaiQuyTrinhCongTacPhiTongHop.GiamDocDaDuyet;
        //    var lstNghiPhepNghiBu = (from ccth in smdb.HRM_ChamCongVanPhongTongHop
        //                                  join cc in smdb.HRM_ChamCongVanPhong
        //                                  on ccth.Id equals cc.IdChamCongVanPhongTongHop
        //                                  join ct in smdb.HRM_ChamCongVanPhongChiTiet
        //                                  on cc.Id equals ct.IdChamCongVanPhong
        //                                  join ctn in smdb.HRM_ChamCongVanPhongChiTietNgay
        //                                  on ct.Id equals ctn.IdChamCongVanPhongChiTiet
        //                                  where  ct.NgayThang >= TuNgay && ct.NgayThang < DenNgay
        //                                  && ccth.TrangThai == TrangThaiPheDuyet
        //                                  && lstIdTongHopChamCong.Contains(ccth.Id)
        //                                  && lstIdLoaiNghiPhepNghiBu.Contains(ctn.IdLoaiNghiPhep)
        //                                  select new LuongNghiPhepNghiBuTuNgayDenNgay
        //                                  {
        //                                      IdNhanVien = cc.IdNhanVienKeKhai,
        //                                      IdLoaiNghi = ctn.IdLoaiNghiPhep,
        //                                  }).ToList();



        //    return lstNghiPhepNghiBu;
        //}


        // DefineData.eAction eAction = DefineData.eAction.QUYTRINHCHAMCONGCANHAN;

        // byte TrangThaiChuaChuyenDuyet = (byte)DefineData.eTrangThaiChamCongVanPhong.ChuaChuyenDuyet;
        // byte TrangThaiChoTPDuyet = (byte)DefineData.eTrangThaiChamCongVanPhong.ChoTruongPhongDuyet;
        // byte TrangThaiTruongPhongKhongDuyet = (byte)DefineData.eTrangThaiChamCongVanPhong.TruongPhongKhongDuyet;
        // byte TrangThaiChuaTongHop = (byte)DefineData.eTrangThaiChamCongVanPhong.ChuaTongHop;
        // byte TrangThaiDaTongHop = (byte)DefineData.eTrangThaiChamCongVanPhong.DaTongHop;

        // DefineData.eAction eActionTongHop = DefineData.eAction.QUYTRINHCHAMCONGCANHAN;
        // byte TrangThaiTongHopChuaChuyenDuyet = (byte)DefineData.eTrangThaiChamCongVanPhongTongHop.ChuaChuyenDuyet;
        // byte TrangThaiTongHopChoGiamDocDuyet = (byte)DefineData.eTrangThaiChamCongVanPhongTongHop.ChoGiamDocDuyet;
        // byte TrangThaiTongHopGiamDocDaDuyet = (byte)DefineData.eTrangThaiChamCongVanPhongTongHop.GiamDocDaDuyet;
        // byte TrangThaiTongHopGiamDocKhongDuyet = (byte)DefineData.eTrangThaiChamCongVanPhongTongHop.GiamDocKhongDuyet;


        // public dynamic GetDanhSachChamCongCaNhan(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     #region Quyền

        //     #endregion
        //     FuncBoPhan funcBoPhan = new FuncBoPhan();
        //     var lstIdBoPhanQuanLy = funcBoPhan.GetDanhSachIdBoPhanQuanLy(idCurrentUser, db);

        //     var query = (from quytrinh in db.HRM_ChamCongVanPhong
        //                  join nv in db.HRM_NhanVien
        //                  on quytrinh.IdNhanVienKeKhai equals nv.IdGuid
        //                  join bp in db.HRM_BoPhan
        //                  on quytrinh.IdBoPhanKeKhai equals bp.IdGuid
        //                  select new Models.ChamCongVanPhong
        //                  {
        //                      Created = quytrinh.Created,
        //                      CreatedBy = quytrinh.CreatedBy,
        //                      CreatedByName = quytrinh.CreatedByName,
        //                      IdGuid = quytrinh.IdGuid,
        //                      Modified = quytrinh.Modified,
        //                      ModifiedBy = quytrinh.ModifiedBy,
        //                      ModifiedByName = quytrinh.ModifiedByName,
        //                      TrangThai = quytrinh.TrangThai,
        //                      NguoiKeKhai = nv.TenDayDu,
        //                      Nam = quytrinh.Nam,
        //                      PhongBan = bp.TenBoPhan,
        //                      Thang = quytrinh.Thang,
        //                      IdBoPhanKeKhai = quytrinh.IdBoPhanKeKhai,
        //                      IdNhanVienKeKhai = quytrinh.IdNhanVienKeKhai
        //                  });

        //     string KeyWord = dyn.KeyWord;
        //     int CurrentPage = dyn.CurrentPage == null ? 1 : dyn.CurrentPage;
        //     int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
        //     string TabTrangThai = dyn.TabTrangThai;


        //     if (TabTrangThai == DefineData.eTabTrangThai.ChuaXuLy.ToString())
        //     {
        //         if (lstIdBoPhanQuanLy.Count == 0)
        //         {
        //             query = query.Where(x =>
        //             (x.CreatedBy == idCurrentUser && (x.TrangThai == TrangThaiChuaChuyenDuyet || x.TrangThai == TrangThaiTruongPhongKhongDuyet))
        //             );
        //         }
        //         else
        //         {
        //             query = query.Where(x => (x.CreatedBy == idCurrentUser && x.TrangThai == TrangThaiChuaChuyenDuyet)
        //             || (lstIdBoPhanQuanLy.Contains(x.IdBoPhanKeKhai) && (x.TrangThai == TrangThaiChoTPDuyet)
        //             ));
        //         }
        //     }
        //     else
        //     {

        //         if (lstIdBoPhanQuanLy.Count == 0)
        //         {
        //             query = query.Where(x => (x.CreatedBy == idCurrentUser && x.TrangThai > TrangThaiChuaChuyenDuyet
        //             && x.TrangThai != TrangThaiTruongPhongKhongDuyet)
        //             );
        //         }
        //         else
        //         {
        //             query = query.Where(x => lstIdBoPhanQuanLy.Contains(x.IdBoPhanKeKhai)
        //                 && (x.TrangThai > TrangThaiChoTPDuyet)
        //             );
        //         }
        //     }


        //     if (string.IsNullOrEmpty(KeyWord) == false)
        //     {
        //         KeyWord = KeyWord.ToLower();
        //         query = query.Where(x => x.NguoiKeKhai.ToLower().Contains(KeyWord) || x.PhongBan.ToLower().Contains(KeyWord));
        //     }

        //     if (CurrentPage == 0)
        //     {
        //         return query.OrderByDescending(q => q.Created).ToList();
        //     }
        //     else
        //     {
        //         dynamic result = new DynamicObject();

        //         int count = query.Count();
        //         var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

        //         result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
        //         result.items = listResult;
        //         return result;
        //     }


        // }


        // public dynamic TaoChamCongCaNhan(HRM_ChamCongVanPhong item, string idCurrentUser = "", ApplicationDbContext db = null)
        // {

        //     // check xem tháng rồi nhân viên tạo ra bảng chấm công chưa

        //     var itemChamCong = db.HRM_ChamCongVanPhong.FirstOrDefault(x => x.IdNhanVienKeKhai == item.IdNhanVienKeKhai && x.Thang == item.Thang && x.Nam == item.Nam);
        //     if (itemChamCong != null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Đã tồn tại chấm công cá nhân của tháng", itemChamCong.IdGuid);

        //     // lấy ra config hệ thống
        //     var lstCaiDatHeThong = db.HRM_CaiDatHeThong.ToList();

        //     string IdNhanVien = item.IdNhanVienKeKhai;
        //     DateTime TuNgay = new DateTime(item.Nam, item.Thang, 1);
        //     DateTime DenNgay = new DateTime(item.Nam, item.Thang, 1).AddMonths(1);

        //     // lấy ra công tác của nhân viên
        //     FuncCongTac funcCongTac = new FuncCongTac();
        //     var lstQuyetDinhCongTac = funcCongTac.GetQuyetDinhDiCongTacNhanVienTuNgayDenNgay(IdNhanVien, TuNgay, DenNgay, db);

        //     // lấy ra nghỉ phép của nhân viên
        //     FuncXinNghiPhep funcXinNghiPhep = new FuncXinNghiPhep();
        //     var lstXinNghi = funcXinNghiPhep.GetDanhSachNghiPhepNhanVienTuNgayDenNgay(IdNhanVien, TuNgay, DenNgay, db);

        //     // lấy ra danh sách nghỉ lễ trong năm
        //     FuncDanhMuc funcDanhMuc = new FuncDanhMuc();
        //     var lstDanhSachNghiLe = funcDanhMuc.GetDanhSachNgayNghiTrongThang(TuNgay, DenNgay, db);

        //     // danh mục loại nghỉ phép
        //     var lstLoaiNghiPhep = db.HRM_LoaiNghiPhep.ToList();

        //     item.NgayCongTinhLuong = GetNgayCongTinhLuong(item.Thang, item.Nam, lstCaiDatHeThong);
        //     item.TrangThai = TrangThaiChuaChuyenDuyet;
        //     db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;

        //     try
        //     {
        //         db.SaveChanges();
        //         db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("TaoChamCongCaNhan data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     var Id = item.IdGuid;
        //     List<HRM_ChamCongVanPhongChiTiet> lstChiTiet = new List<HRM_ChamCongVanPhongChiTiet>();
        //     for (DateTime Ngay = TuNgay; Ngay < DenNgay; Ngay.AddDays(1))
        //     {
        //         HRM_ChamCongVanPhongChiTiet ChiTiet = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTiet(Id, Ngay);
        //         lstChiTiet.Add(ChiTiet);
        //     }

        //     try
        //     {
        //         db.HRM_ChamCongVanPhongChiTiet.AddRange(lstChiTiet);
        //         db.SaveChanges();
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("TaoChamCongCaNhan ChiTiet data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     List<HRM_ChamCongVanPhongChiTietNgay> lstChamCongChiTietNgay = new List<HRM_ChamCongVanPhongChiTietNgay>();
        //     foreach (var ChiTiet in lstChiTiet)
        //     {
        //         DateTime NgayChamCong = ChiTiet.NgayThang;
        //         HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgaySang = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay(ChiTiet);
        //         ChiTietNgaySang.SoGioLam = DefineData.ThoiGianLamViecMotBuoi;
        //         ChiTietNgaySang.SangChieu = DefineData.eLoaiSangChieu.Sang.ToString();
        //         HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgayChieu = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay(ChiTiet);
        //         ChiTietNgaySang.SoGioLam = DefineData.ThoiGianLamViecMotBuoi;
        //         ChiTietNgaySang.SangChieu = DefineData.eLoaiSangChieu.Chieu.ToString();
        //         //tính toán ngày đó có đi công tác
        //         // Công tác =>  nghỉ thứ 7 chủ nhật, Nghỉ ngày lễ, nghỉ phép
        //         bool isDiCongTac = XuLyNgayCongTac(lstQuyetDinhCongTac, NgayChamCong, ChiTietNgaySang, ChiTietNgayChieu);
        //         if (!isDiCongTac)
        //         {
        //             bool isCuoiTuan = XuLyNgayCuoiTuan(ChiTiet.Ngay, ChiTietNgaySang, ChiTietNgayChieu, lstCaiDatHeThong);
        //             if (!isCuoiTuan)
        //             {
        //                 bool isNghiLe = XuLyNgayNghiLe(NgayChamCong, ChiTietNgaySang, ChiTietNgayChieu, lstDanhSachNghiLe);

        //                 if (!isNghiLe)
        //                 {
        //                     bool isNghiPhep = XuLyNgayNghiPhep(lstXinNghi, NgayChamCong, ChiTietNgaySang, ChiTietNgayChieu, lstLoaiNghiPhep);

        //                 }
        //             }
        //         }


        //         lstChamCongChiTietNgay.Add(ChiTietNgaySang);
        //         lstChamCongChiTietNgay.Add(ChiTietNgayChieu);
        //     }

        //     try
        //     {
        //         db.HRM_ChamCongVanPhongChiTietNgay.AddRange(lstChamCongChiTietNgay);
        //         float SoGioLamViec = lstChamCongChiTietNgay.Sum(x => x.SoGioLam);
        //         item.TongNgayLamViec = SoGioLamViec / DefineData.ThoiGianLamViecMotBuoi;

        //         db.SaveChanges();
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("TaoChamCongCaNhan ChiTiet data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }


        //     return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", Id);

        // }

        // #region Công tác
        // public bool XuLyNgayCongTac(List<HRM_QuyetDinhDiCongTac> lstQuyetDinhDiCongTac, DateTime NgayChamCong, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgaySang
        //     , HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgayChieu)
        // {
        //     var itemCongTac = lstQuyetDinhDiCongTac.FirstOrDefault(x => x.TuNgay <= NgayChamCong && x.DenNgay >= NgayChamCong);
        //     if (itemCongTac != null)
        //     {
        //         SetQuyetDinhChoChamCongNgay(ChiTietNgaySang, itemCongTac);
        //         SetQuyetDinhChoChamCongNgay(ChiTietNgaySang, itemCongTac);
        //         return true;
        //     }
        //     return false;
        // }
        // public void SetQuyetDinhChoChamCongNgay(HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgay, HRM_QuyetDinhDiCongTac itemCongTac)
        // {
        //     ChiTietNgay.IdDuAn = itemCongTac.IdDuAn;
        //     ChiTietNgay.IdQuyetDinhDiCongTac = itemCongTac.IdGuid;
        //     ChiTietNgay.IdKhuVucDiaDiem = itemCongTac.IdKhuVucDiaDiem;
        //     ChiTietNgay.SoGioLam = 0;
        //     ChiTietNgay.ChiTiet = "Công tác";
        // }

        // #endregion

        // #region Nghỉ phép

        // public bool XuLyNgayNghiPhep(List<HRM_QuyTrinhXinNghiPhepChiTiet> lstQuyetDinhXinNghiPhep, DateTime NgayChamCong, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgaySang
        //     , HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgayChieu, List<HRM_LoaiNghiPhep> lstLoaiNghiPhep)
        // {
        //     var itemXinNghiPhep = lstQuyetDinhXinNghiPhep.FirstOrDefault(x => x.TuNgay <= NgayChamCong && x.DenNgay >= NgayChamCong);
        //     if (itemXinNghiPhep != null)
        //     {
        //         string MaNghi = "";
        //         string IdLoaiNghiPhep = itemXinNghiPhep.IdLoaiNghiPhep;
        //         var itemLoaiNghiPhep = lstLoaiNghiPhep.FirstOrDefault(x => x.IdGuid == IdLoaiNghiPhep);
        //         if (itemLoaiNghiPhep != null)
        //             MaNghi = itemLoaiNghiPhep.MaChamCong;

        //         if (NgayChamCong.Date == itemXinNghiPhep.DenNgay.Date)
        //         {
        //             // nếu ngày nghỉ phép là ngày cuối có thể nghỉ 0,5 ngày
        //             var SoNgayNghiLe = itemXinNghiPhep.SoNgay - Math.Truncate(itemXinNghiPhep.SoNgay);
        //             if (SoNgayNghiLe == 0 || SoNgayNghiLe >= 0.5)
        //             {
        //                 //Nghỉ cả ngày
        //                 SetNghiPhepChoChamCongNgay(ChiTietNgaySang, IdLoaiNghiPhep, MaNghi);
        //                 SetNghiPhepChoChamCongNgay(ChiTietNgayChieu, IdLoaiNghiPhep, MaNghi);
        //             }
        //             else
        //             {
        //                 // làm nửa buổi
        //                 SetNghiPhepChoChamCongNgay(ChiTietNgaySang, IdLoaiNghiPhep, MaNghi);
        //                 SetNghiPhepChoChamCongNgay(ChiTietNgayChieu, "", "");
        //             }
        //         }
        //         else
        //         {
        //             SetNghiPhepChoChamCongNgay(ChiTietNgaySang, IdLoaiNghiPhep, MaNghi);
        //             SetNghiPhepChoChamCongNgay(ChiTietNgayChieu, IdLoaiNghiPhep, MaNghi);
        //         }

        //         return true;
        //     }
        //     return false;
        // }



        // public void SetNghiPhepChoChamCongNgay(HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgay, string IdLoaiNghiPhep, string MaLoaiNghiPhep)
        // {
        //     ChiTietNgay.IdLoaiNghiPhep = IdLoaiNghiPhep;
        //     ChiTietNgay.MaChamCong = MaLoaiNghiPhep;

        //     if (string.IsNullOrEmpty(IdLoaiNghiPhep))
        //         ChiTietNgay.SoGioLam = DefineData.ThoiGianLamViecMotBuoi;
        //     else
        //         ChiTietNgay.SoGioLam = 0;
        // }


        // #endregion

        // #region Nghỉ cuối tuần

        // public bool XuLyNgayCuoiTuan(int Ngay, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgaySang
        //   , HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgayChieu, List<HRM_CaiDatHeThong> lstCaiDatHeThong)
        // {

        //     if (Ngay != 7 && Ngay == 8)
        //         return false;

        //     double ThoiGianLamViec = 0;
        //     if (Ngay == 7)
        //     {
        //         var itemCaiDatNgayThu7 = lstCaiDatHeThong.FirstOrDefault(x => x.Ma == DefineData.eMaSetting.ChamCongThu7.ToString());
        //         ThoiGianLamViec = itemCaiDatNgayThu7.ValueDouble;
        //     }
        //     else
        //     if (Ngay == 8)
        //     {
        //         var itemCaiDatNgayChuNhat = lstCaiDatHeThong.FirstOrDefault(x => x.Ma == DefineData.eMaSetting.ChamCongChuNhat.ToString());
        //         ThoiGianLamViec = itemCaiDatNgayChuNhat.ValueDouble;
        //     }

        //     if (ThoiGianLamViec > DefineData.ThoiGianLamViecMotBuoi)
        //     {
        //         SetCuoiTuanChoChamCongNgay(ChiTietNgaySang, 0);
        //         SetCuoiTuanChoChamCongNgay(ChiTietNgayChieu, 0);
        //     }
        //     else
        //     {
        //         SetCuoiTuanChoChamCongNgay(ChiTietNgaySang, DefineData.ThoiGianLamViecMotBuoi);
        //         SetCuoiTuanChoChamCongNgay(ChiTietNgayChieu, 0);
        //     }

        //     return true;
        // }

        // public void SetCuoiTuanChoChamCongNgay(HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgay, float SoGioLam)
        // {
        //     ChiTietNgay.SoGioLam = SoGioLam;
        //     if (SoGioLam == 0)
        //         ChiTietNgay.ChiTiet = "Nghỉ cuối tuần";
        //     else
        //         ChiTietNgay.ChiTiet = "Làm việc";

        // }

        // #endregion

        // #region Xử lý ngày nghỉ lễ

        // public bool XuLyNgayNghiLe(DateTime NgayChamCong, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgaySang
        //, HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongChiTietNgay ChiTietNgayChieu, List<HRM_ThietLapNgayNghi> lstThietLapNgayNghi)
        // {
        //     var itemNgayNghi = lstThietLapNgayNghi.FirstOrDefault(x => x.TuNgay <= NgayChamCong && x.DenNgay >= NgayChamCong);
        //     if (itemNgayNghi != null)
        //     {
        //         ChiTietNgaySang.IdThietLapNgayNghi = ChiTietNgayChieu.IdThietLapNgayNghi = itemNgayNghi.IdGuid;
        //         //ChiTietNgaySang.MaNghi = ChiTietNgayChieu.MaNghi = itemNgayNghi.IdGuid;
        //         ChiTietNgaySang.SoGioLam = ChiTietNgayChieu.SoGioLam = 0;
        //         ChiTietNgaySang.ChiTiet = ChiTietNgayChieu.ChiTiet = "Nghỉ lễ";
        //         return true;
        //     }
        //     else
        //         return false;
        // }

        // #endregion


        // #region Tính ngày công tính lương

        // public float GetNgayCongTinhLuong(int Thang, int Nam, List<HRM_CaiDatHeThong> lstCaiDatHeThong)
        // {
        //     var itemCaiDatNgayThu7 = lstCaiDatHeThong.FirstOrDefault(x => x.Ma == DefineData.eMaSetting.ChamCongThu7.ToString());
        //     bool isTinhNgayThu7 = itemCaiDatNgayThu7.ValueDouble > 0;

        //     DateTime NgayDauThang = new DateTime(Nam, Thang, 1);
        //     DateTime NgayCuoiThang = NgayDauThang.AddMonths(1);

        //     float Dem = 0;
        //     for (DateTime temp = NgayDauThang; temp < NgayCuoiThang; temp.AddDays(1))
        //     {
        //         int DayOfWeek = (int)temp.DayOfWeek;
        //         if (DayOfWeek != 7 && DayOfWeek != 8)
        //             Dem++;
        //         else
        //             if (DayOfWeek == 7 && isTinhNgayThu7)
        //             Dem++;
        //     }

        //     return Dem;
        // }

        // #endregion


        // public dynamic GetChamCongCaNhan(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     var item = db.HRM_ChamCongVanPhong.FirstOrDefault(x => x.IdGuid == Id);

        //     if (item == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy chấm công");

        //     var lstChiTiet = db.HRM_ChamCongVanPhongChiTiet.Where(x => x.IdChamCongVanPhong == Id).ToList();
        //     var lstChiTietNgay = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => x.IdChamCongVanPhong == Id).ToList();

        //     var lstIdDuAn = lstChiTietNgay.Select(x => x.IdDuAn).Distinct().ToList();
        //     var lstIdQuyetDinhCongTac = lstChiTietNgay.Select(x => x.IdQuyetDinhDiCongTac).Distinct().ToList();

        //     var lstDuAn = db.HRM_DuAn.Where(x => lstIdDuAn.Contains(x.IdGuid)).ToList();
        //     var lstQuyetDinhDiCongTac = db.HRM_QuyetDinhDiCongTac.Where(x => lstIdQuyetDinhCongTac.Contains(x.IdGuid)).ToList();

        //     foreach (var ChiTietNgay in lstChiTietNgay)
        //     {
        //         ChiTietNgay.DuAn = lstDuAn.FirstOrDefault(x => x.IdGuid == ChiTietNgay.IdDuAn);
        //         ChiTietNgay.QuyetDinhDiCongTac = lstQuyetDinhDiCongTac.FirstOrDefault(x => x.IdGuid == ChiTietNgay.IdQuyetDinhDiCongTac);
        //     }

        //     foreach (var ChiTiet in lstChiTiet)
        //     {
        //         ChiTiet.lstChamCongChiTietNgay = lstChiTietNgay.Where(x => x.IdChamCongVanPhongChiTiet == ChiTiet.IdGuid).ToList();
        //     }

        //     item.lstChiTiet = lstChiTiet;
        //     return item;
        // }

        // public dynamic UpdateChamCongCaNhanNgay(List<HRM_ChamCongVanPhongChiTietNgay> items, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     if (items.Count == 0)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không có item update");

        //     foreach (var item in items)
        //     {
        //         db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //     }

        //     try
        //     {
        //         db.SaveChanges();
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("UpdateChamCongCaNhanNgay ChiTiet data: {0}, Error {1}", JsonConvert.SerializeObject(items), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     var IdChamCong = items[0].IdChamCongVanPhong;
        //     var itemChamCong = db.HRM_ChamCongVanPhong.FirstOrDefault(x => x.IdGuid == IdChamCong);
        //     float SoGioLamViec = db.HRM_ChamCongVanPhongChiTietNgay.Sum(x => x.SoGioLam);
        //     itemChamCong.TongNgayLamViec = SoGioLamViec / DefineData.ThoiGianLamViecMotBuoi;

        //     try
        //     {
        //         db.SaveChanges();
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("UpdateChamCongCaNhanNgay data: {0}, Error {1}", JsonConvert.SerializeObject(items), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");

        // }

        // public dynamic KiemTraButtonChamCongCaNhan(string Id, string idCurrentUser, ApplicationDbContext db = null)
        // {
        //     bool bChuyenTiep = false;
        //     bool bKhongDuyet = false;
        //     bool bXoa = false;
        //     bool bGhi = true;


        //     // item kế hoạch chi
        //     var item = db.HRM_ChamCongVanPhong.FirstOrDefault(x => x.IdGuid == Id);

        //     byte TrangThai = 0;
        //     if (item != null)
        //         TrangThai = item.TrangThai;

        //     if (TrangThai == TrangThaiChuaChuyenDuyet && item.CreatedBy == idCurrentUser)
        //     {
        //         bXoa = true;
        //     }


        //     FuncBoPhan FuncBoPhan = new FuncBoPhan();
        //     var lstIdBoPhanQuanLy = FuncBoPhan.GetDanhSachIdBoPhanQuanLy(idCurrentUser, db);
        //     if (TrangThai == TrangThaiChoTPDuyet)
        //     {
        //         if (lstIdBoPhanQuanLy.Contains(item.IdBoPhanKeKhai))
        //         {
        //             bChuyenTiep = true;
        //             bKhongDuyet = true;
        //         }
        //     }
        //     else
        //      if (TrangThai == TrangThaiChuaChuyenDuyet || TrangThai == TrangThaiTruongPhongKhongDuyet)
        //     {
        //         if (item.CreatedBy == idCurrentUser)
        //         {
        //             bChuyenTiep = true;
        //         }
        //     }


        //     dynamic dnn = new DynamicObject();
        //     dnn.ChuyenTiep = bChuyenTiep;
        //     dnn.Ghi = bChuyenTiep;
        //     dnn.KhongDuyet = bKhongDuyet;
        //     dnn.Xoa = bXoa;
        //     return dnn;

        // }
        // public dynamic DeleteChamCongCaNhan(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        // {

        //     var lstChamCongVanPhong = db.HRM_ChamCongVanPhong.Where(x => lstId.Contains(x.IdGuid) && x.TrangThai == TrangThaiChuaChuyenDuyet).ToList();
        //     var lstChamCongVanPhongChiTiet = db.HRM_ChamCongVanPhongChiTiet.Where(x => lstId.Contains(x.IdChamCongVanPhong)).ToList();
        //     var lstKeHoachTuyenDungChiTietNgay = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => lstId.Contains(x.IdChamCongVanPhong)).ToList();

        //     foreach (var KeHoach in lstChamCongVanPhong)
        //     {
        //         if (KeHoach.CreatedBy == idCurrentUser)
        //         {
        //             db.Entry(KeHoach).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
        //             var lstChiTiet = lstChamCongVanPhongChiTiet.Where(x => x.IdChamCongVanPhong == KeHoach.IdGuid);
        //             db.HRM_ChamCongVanPhongChiTiet.RemoveRange(lstChiTiet);

        //             var lstChiTietNgay = lstKeHoachTuyenDungChiTietNgay.Where(x => x.IdChamCongVanPhong == KeHoach.IdGuid);
        //             db.HRM_ChamCongVanPhongChiTietNgay.RemoveRange(lstChiTietNgay);
        //         }
        //     }

        //     try
        //     {
        //         db.SaveChanges();
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("DeleteChamCongCaNhan, Error {0}", ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        // }

        // public dynamic ChuyenTiepChamCongCaNhan(string Id, byte TrangThaiHienTai, CurrentUser CurrentUser, ApplicationDbContext db = null)
        // {
        //     var item = db.HRM_ChamCongVanPhong.FirstOrDefault(x => x.IdGuid == Id);

        //     if (item == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy chấm công cá nhân");

        //     if (item.TrangThai != TrangThaiHienTai)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Quy trình đã được chuyển trước đó");

        //     var idCurrentUser = CurrentUser.Id;


        //     // nếu là trưởng phòng thì quy trình tự động được duyệt
        //     FuncBoPhan funcBoPhan = new FuncBoPhan();
        //     DefineData.eRigth eRigth = DefineData.eRigth.PHEDUYET2;
        //     bool isUpdate = false;



        //     var lstIdBoPhanQuanLy = funcBoPhan.GetDanhSachIdBoPhanQuanLy(idCurrentUser, db);
        //     if (lstIdBoPhanQuanLy.Contains(item.IdBoPhanKeKhai))
        //     {
        //         if (item.TrangThai == TrangThaiChuaChuyenDuyet)
        //         {
        //             item.TrangThai = TrangThaiChuaTongHop;
        //             isUpdate = true;
        //         }
        //     }
        //     else
        //     {

        //         if ((item.TrangThai == TrangThaiChuaChuyenDuyet
        //             || item.TrangThai == TrangThaiTruongPhongKhongDuyet)
        //             && item.CreatedBy == idCurrentUser)
        //         {
        //             item.TrangThai = TrangThaiChoTPDuyet;
        //             isUpdate = true;
        //         }
        //         else
        //             if (TrangThaiHienTai == TrangThaiChoTPDuyet)
        //         {
        //             item.TrangThai = TrangThaiChuaTongHop;
        //             isUpdate = true;
        //         }
        //     }



        //     try
        //     {

        //         if (isUpdate)
        //         {
        //             db.SaveChanges();

        //             if (item.TrangThai == TrangThaiChoTPDuyet)
        //             {
        //                 List<string> lstIdUserNhanNotification = funcBoPhan.GetDanhSachUserQuanLyBoPhan(item.IdBoPhanKeKhai, db);
        //                 AddNotification(CurrentUser, eAction, lstIdUserNhanNotification, "Gửi yêu cầu phê duyệt ");
        //             }



        //             return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        //         }
        //         else
        //             return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền thay đổi");


        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("ChuyenTiepChamCongCaNhan data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        // }

        // public dynamic KhongDuyetChamCongCaNhan(string Id, byte TrangThaiHienTai, CurrentUser CurrentUser, ApplicationDbContext db = null)
        // {
        //     var item = db.HRM_ChamCongVanPhong.FirstOrDefault(x => x.IdGuid == Id);

        //     if (item == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy quy trình công tác phí");

        //     if (item.TrangThai != TrangThaiHienTai)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Quy trình đã được chuyển trước đó");

        //     var idCurrentUser = CurrentUser.Id;


        //     FuncBoPhan funcBoPhan = new FuncBoPhan();
        //     bool isUpdate = false;

        //     var lstIdBoPhanQuanLy = funcBoPhan.GetDanhSachIdBoPhanQuanLy(idCurrentUser, db);
        //     if (item.TrangThai == TrangThaiChoTPDuyet && lstIdBoPhanQuanLy.Contains(item.IdBoPhanKeKhai))
        //     {
        //         item.TrangThai = TrangThaiTruongPhongKhongDuyet;
        //         isUpdate = true;
        //     }


        //     try
        //     {

        //         if (isUpdate)
        //         {
        //             db.SaveChanges();

        //             if (item.TrangThai == TrangThaiTruongPhongKhongDuyet)
        //             {
        //                 List<string> lstIdUserNhanNotification = new List<string>() { item.CreatedBy };
        //                 AddNotification(CurrentUser, eAction, lstIdUserNhanNotification, "Không phê duyệt ");
        //             }


        //             return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        //         }
        //         else
        //             return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền thay đổi");


        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("KhongDuyetChamCongCaNhan data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }
        // }



        // #region Tổng hợp

        // public dynamic GetDanhSachChamCongCaNhanTheoTrangThai(dynamic dyn, string idCurrentUser = "", byte TrangThai = 0, ApplicationDbContext db = null
        //     )
        // {

        //     #region Quyền

        //     int QuyenView = GetQuyenView(eActionTongHop.ToString(), idCurrentUser);
        //     if (QuyenView == (int)DefineData.eGioiHan.NONE)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền tổng hợp");

        //     #endregion

        //     string KeyWord = dyn.KeyWord;
        //     int CurrentPage = dyn.CurrentPage == null ? 1 : dyn.CurrentPage;
        //     int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;

        //     var query = (from quytrinh in db.HRM_ChamCongVanPhong
        //                  join nv in db.HRM_NhanVien
        //                  on quytrinh.IdNhanVienKeKhai equals nv.IdGuid
        //                  join bp in db.HRM_BoPhan
        //                  on quytrinh.IdBoPhanKeKhai equals bp.IdGuid
        //                  where quytrinh.TrangThai == TrangThai
        //                  select new ChamCongVanPhong
        //                  {
        //                      Created = quytrinh.Created,
        //                      CreatedBy = quytrinh.CreatedBy,
        //                      CreatedByName = quytrinh.CreatedByName,
        //                      IdGuid = quytrinh.IdGuid,
        //                      Modified = quytrinh.Modified,
        //                      ModifiedBy = quytrinh.ModifiedBy,
        //                      ModifiedByName = quytrinh.ModifiedByName,
        //                      TrangThai = quytrinh.TrangThai,
        //                      NguoiKeKhai = nv.TenDayDu,
        //                      Nam = quytrinh.Nam,
        //                      PhongBan = bp.TenBoPhan,
        //                      Thang = quytrinh.Thang,
        //                      IdBoPhanKeKhai = quytrinh.IdBoPhanKeKhai,
        //                      IdNhanVienKeKhai = quytrinh.IdNhanVienKeKhai
        //                  });

        //     if (string.IsNullOrEmpty(KeyWord) == false)
        //     {
        //         KeyWord = KeyWord.ToLower();
        //         query = query.Where(x => x.NguoiKeKhai.ToLower().Contains(KeyWord) || x.PhongBan.ToLower().Contains(KeyWord));
        //     }

        //     if (CurrentPage == 0)
        //     {
        //         return query.OrderByDescending(q => q.Created).ToList();
        //     }
        //     else
        //     {
        //         dynamic result = new DynamicObject();

        //         int count = query.Count();
        //         var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

        //         result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
        //         result.items = listResult;
        //         return result;
        //     }
        // }

        // public dynamic TongHopQuyTrinhChamCong(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        // {

        //     #region Quyền

        //     int quyenAdd = GetQuyenAdd(eActionTongHop.ToString(), idCurrentUser);
        //     if (quyenAdd == (int)DefineData.eGioiHan.NONE)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền tổng hợp");

        //     #endregion


        //     var lstChamCongVanPhong = db.HRM_ChamCongVanPhong.Where(x => lstId.Contains(x.IdGuid)).ToList();
        //     var lstIdChamCongVanPhong = lstChamCongVanPhong.Select(x => x.IdGuid).ToList();
        //     var lstChamCongVanPhongChiTiet = db.HRM_ChamCongVanPhongChiTiet.Where(x => lstIdChamCongVanPhong.Contains(x.IdChamCongVanPhong)).ToList();

        //     if (lstChamCongVanPhong.Count == 0)
        //     {
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy kế hoạch tuyển dụng");
        //     }

        //     bool isValid = true;

        //     // check xem các kế hoạch có cùng loại và cùng năm o
        //     int Dem = 0;
        //     int Thang = 0;
        //     int Nam = 0;
        //     foreach (var itemKeHoach in lstChamCongVanPhong)
        //     {
        //         if (Dem == 0)
        //         {
        //             Thang = itemKeHoach.Thang;
        //             Nam = itemKeHoach.Nam;
        //         }
        //         else
        //              if (itemKeHoach.Thang != Thang || itemKeHoach.Nam != Nam)
        //         {
        //             isValid = false;
        //             break;
        //         }
        //         Dem++;
        //     }

        //     if (isValid == false)
        //     {
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Các kế hoạch khác loại hoặc năm");
        //     }


        //     HRM_ChamCongVanPhongTongHop itemTongHop = new HRM.Api.Models.SmartEOEModel.HRM_ChamCongVanPhongTongHop(lstChamCongVanPhong[0]);
        //     db.Entry(itemTongHop).State = Microsoft.EntityFrameworkCore.EntityState.Added;

        //     try
        //     {
        //         db.SaveChanges();

        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("TongHopQuyTrinhChamCong data: {0}, Error {1}", JsonConvert.SerializeObject(itemTongHop), ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     var IdTongHop = itemTongHop.IdGuid;

        //     foreach (var QuyTrinh in lstChamCongVanPhong)
        //     {
        //         QuyTrinh.IdChamCongVanPhongTongHop = IdTongHop;
        //         QuyTrinh.TrangThai = TrangThaiDaTongHop;
        //         db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //     }

        //     foreach (var ChiTiet in lstChamCongVanPhongChiTiet)
        //     {
        //         ChiTiet.IdChamCongVanPhongTongHop = IdTongHop;
        //         db.Entry(ChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //     }

        //     try
        //     {
        //         db.SaveChanges();
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("TongHopQuyTrinhChamCong ChiTiet Error {0}", ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        //     return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", IdTongHop);

        // }

        // public dynamic BoTongHopQuyTrinhChamCong(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     #region Quyền

        //     int quyenXoa = GetQuyenXoa(eActionTongHop.ToString(), idCurrentUser);
        //     if (quyenXoa == (int)DefineData.eGioiHan.NONE)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền tổng hợp");
        //     #endregion


        //     var itemTongHOp = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id
        //     && x.TrangThai == TrangThaiTongHopChuaChuyenDuyet);
        //     if (itemTongHOp == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không thể bỏ tổng hợp quy trình chấm công");

        //     db.Entry(itemTongHOp).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;

        //     var lstQuyTrinh = db.HRM_ChamCongVanPhong.Where(x => x.IdChamCongVanPhongTongHop == Id).ToList();
        //     foreach (var itemKeHoach in lstQuyTrinh)
        //     {
        //         itemKeHoach.IdChamCongVanPhongTongHop = "";
        //         db.Entry(itemTongHOp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //     }


        //     var lstQuyTrinhCongTacPhiChiTiet = db.HRM_ChamCongVanPhongChiTiet.Where(x => x.IdChamCongVanPhongTongHop == Id).ToList();
        //     foreach (var itemChiTiet in lstQuyTrinhCongTacPhiChiTiet)
        //     {
        //         itemChiTiet.IdChamCongVanPhongTongHop = "";
        //         db.Entry(itemChiTiet).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //     }

        //     try
        //     {
        //         db.SaveChanges();
        //         return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("BoTongHopQuyTrinhChamCong Error {0}", ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        // }

        // public dynamic GetDanhSachChamCongTongHop(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
        // {

        //     #region Quyền
        //     List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eActionTongHop.ToString(), idCurrentUser);
        //     if (PhanQuyens == null)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền");
        //     #endregion

        //     int Thang = dyn.Thang == null ? 0 : dyn.Thang;
        //     int CurrentPage = dyn.CurrentPage == null ? 1 : dyn.CurrentPage;
        //     int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
        //     int Nam = dyn.NumperPage == null ? 0 : dyn.NumperPage;
        //     string TabTrangThai = dyn.TabTrangThai;

        //     var query = db.HRM_ChamCongVanPhongTongHop.Where(x => string.IsNullOrEmpty(x.IdGuid) == false);


        //     if (TabTrangThai == DefineData.eTabTrangThai.ChuaXuLy.ToString())
        //     {
        //         DefineData.eRigth eRigth = DefineData.eRigth.CHUYENDUYET;
        //         BangPhanQuyen pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());

        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {

        //             query = query.Where(x => x.TrangThai == TrangThaiTongHopChuaChuyenDuyet || x.TrangThai == TrangThaiTongHopGiamDocKhongDuyet);

        //             if (pq.GioiHan == (int)DefineData.eGioiHan.OWNER)
        //                 query = query.Where(x => x.CreatedBy == idCurrentUser);

        //             goto tt;
        //         }

        //         eRigth = DefineData.eRigth.PHEDUYET;
        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {
        //             query = query.Where(x => x.TrangThai == TrangThaiTongHopChoGiamDocDuyet);
        //             goto tt;
        //         }
        //     }
        //     else
        //     {
        //         DefineData.eRigth eRigth = DefineData.eRigth.CHUYENDUYET;
        //         BangPhanQuyen pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());

        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {

        //             query = query.Where(x => x.TrangThai > TrangThaiChuaChuyenDuyet && x.TrangThai != TrangThaiTongHopGiamDocKhongDuyet);

        //             if (pq.GioiHan == (int)DefineData.eGioiHan.OWNER)
        //                 query = query.Where(x => x.CreatedBy == idCurrentUser);

        //             goto tt;
        //         }

        //         eRigth = DefineData.eRigth.PHEDUYET;
        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {
        //             query = query.Where(x => x.TrangThai > TrangThaiTongHopChoGiamDocDuyet);
        //             goto tt;
        //         }
        //     }

        //     tt:

        //     if(Thang > 0)
        //     {
        //         query = query.Where(x => x.Thang == Thang);
        //     }

        //     if (Nam > 0)
        //     {
        //         query = query.Where(x => x.Nam == Nam);
        //     }


        //     if (CurrentPage == 0)
        //     {
        //         return query.OrderByDescending(q => q.Created).ToList();
        //     }
        //     else
        //     {
        //         dynamic result = new DynamicObject();

        //         int count = query.Count();
        //         var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

        //         result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
        //         result.items = listResult;
        //         return result;
        //     }

        // }

        // public dynamic GetChamCongTongHop(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     var item = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id);

        //     if (item == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy quy trình công tác phí tổng hợp");

        //     #region Quyền
        //     int QuyenView = GetQuyenView(eActionTongHop.ToString(), idCurrentUser);
        //     if (QuyenView == (int)DefineData.eGioiHan.NONE)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
        //     else
        //         if (item.CreatedBy != idCurrentUser && QuyenView == (int)DefineData.eGioiHan.OWNER)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
        //     #endregion


        //     var lstChamCongVanPhong = (from ChamCong in  db.HRM_ChamCongVanPhong
        //                               join nv in db.HRM_NhanVien
        //                               on ChamCong.IdNhanVienKeKhai equals nv.IdGuid
        //                               join bp in db.HRM_BoPhan
        //                               on ChamCong.IdBoPhanKeKhai equals bp.IdGuid
        //                               where ChamCong.IdChamCongVanPhongTongHop == Id
        //                               select new ChamCongVanPhongTongHopChiTiet {
        //                                   Created = ChamCong.Created,
        //                                   CreatedByName = ChamCong.CreatedByName,
        //                                   CreatedBy = ChamCong.CreatedBy,
        //                                   IdGuid = ChamCong.IdGuid,
        //                                   MaNhanVien = nv.MaNhanVien,
        //                                   TenNhanVien = nv.TenDayDu,
        //                                   Modified = ChamCong.Modified,
        //                                   ModifiedBy = ChamCong.ModifiedBy,
        //                                   ModifiedByName = ChamCong.ModifiedByName,
        //                                   NgayCongThucTe = ChamCong.TongNgayLamViec,
        //                                   NgayCongTinhLuong = ChamCong.NgayCongTinhLuong,
        //                                   PhongBan = bp.TenBoPhan,
        //                               }).ToList();
        //     var lstIdChamCongVanPhong = lstChamCongVanPhong.Select(x=>x.IdGuid).ToList();

        //     var lstChamCongVanPhongNgay = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => lstIdChamCongVanPhong.Contains(x.IdChamCongVanPhong)).ToList();

        //     var lstLoaiNghiPhep = db.HRM_LoaiNghiPhep.ToList();
        //     foreach (var ChamCong in lstChamCongVanPhong)
        //     {
        //         SetChamCongVanPhongTongHop(ChamCong, lstChamCongVanPhongNgay.Where(x => x.IdChamCongVanPhong == ChamCong.IdGuid).ToList(), lstLoaiNghiPhep);
        //     }

        //     item.lstChiTiet = lstChamCongVanPhong;
        //     item.lstFileDinhKemCu = db.HRM_FileDinhKem.Where(x=>x.idItem == item.IdGuid && x.TableName == DefineData.eTable.HRM_ChamCongVanPhongTongHop.ToString()).ToList();

        //     return item;

        // }

        // public void SetChamCongVanPhongTongHop(ChamCongVanPhongTongHopChiTiet ChamCong, List<HRM_ChamCongVanPhongChiTietNgay> lstChamCongVanPhongNgay
        //     , List<HRM_LoaiNghiPhep> lstLoaiNghiPhep)
        // {
        //    // ChamCong.NgayDuocNhanHoTro = lstChamCongVanPhongNgay.Where(x => string.IsNullOrEmpty(x.IdQuyetDinhDiCongTac) == false).Sum(x=>x.NgayLuong);
        //   //  ChamCong.NghiPhep = lstChamCongVanPhongNgay.Where(x => x.MaChamCong == DefineData.MaChamCongNghiPhep).Sum(x => x.NgayLuong);
        //    // ChamCong.NghiPhep = lstChamCongVanPhongNgay.Where(x => x.MaChamCong == DefineData.MaChamCongNghiPhep).Sum(x => x.NgayLuong);

        //     foreach(var ChamCongNgay in lstChamCongVanPhongNgay)
        //     {
        //         string MaChamCong = ChamCongNgay.MaChamCong;
        //         if (ChamCongNgay.isHoTroDuAn)
        //             ChamCong.NgayDuocNhanHoTro += ChamCongNgay.NgayLuong;
        //         else
        //          if(MaChamCong == DefineData.MaChamCongNghiPhep)
        //             ChamCong.NghiPhep += ChamCongNgay.NgayLuong;
        //         else
        //          if (MaChamCong == DefineData.MaChamCongNghiBu)
        //             ChamCong.NghiBu += ChamCongNgay.NgayLuong;
        //         else
        //          if (MaChamCong == DefineData.MaChamCongNghiOm)
        //             ChamCong.NghiOmTheoCheDo += ChamCongNgay.NgayLuong;
        //         else
        //          if (MaChamCong == DefineData.MaChamCongNghiThaiSan)
        //             ChamCong.NghiThaiSan += ChamCongNgay.NgayLuong;
        //         else
        //          if (MaChamCong == DefineData.MaChamCongNghiKhongLuong)
        //             ChamCong.NghiKhongLuong += ChamCongNgay.NgayLuong;
        //         else
        //          if (MaChamCong == DefineData.MaChamCongNghiTheoNQLD)
        //             ChamCong.NghiViecRiengTheoNQLD += ChamCongNgay.NgayLuong;

        //     }
        // }

        // public dynamic GetChamCongChiTietTheoNhanVien(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     List<ChamCongVanPhongChiTietNhanVien> lstChamCongThangCuaNhanVien = (from cc in db.HRM_ChamCongVanPhong
        //                                                                          join nv in db.HRM_NhanVien
        //                                                                          on cc.IdNhanVienKeKhai equals nv.IdGuid
        //                                                                          join bp in db.HRM_BoPhan
        //                                                                          on cc.IdBoPhanKeKhai equals bp.IdGuid
        //                                                                          where cc.IdGuid == Id
        //                                                                          orderby bp.IdGuid
        //                                                                          select new ChamCongVanPhongChiTietNhanVien
        //                                                                          {
        //                                                                              IdGuid = cc.IdGuid,
        //                                                                              TenNhanVien = nv.TenDayDu,
        //                                                                              PhongBan = bp.TenBoPhan,
        //                                                                              MaNhanVien = nv.MaNhanVien,
        //                                                                          }).ToList();

        //     var lstChamCongChiTietNhanVien = db.HRM_ChamCongVanPhongChiTiet.Where(x => x.IdChamCongVanPhongTongHop == Id).OrderBy(x=>x.NgayThang).ToList();
        //     var lstIdChamCongChiTietNhanVien = lstChamCongChiTietNhanVien.Select(x => x.IdGuid).ToList();
        //     var lstChamCongChiTietNhanVienNgay = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => lstIdChamCongChiTietNhanVien.Contains(x.IdChamCongVanPhongChiTiet)).ToList();

        //     List<ChamCongVanPhongChiTietNhanVienNgay> lstChiTiet = new List<ChamCongVanPhongChiTietNhanVienNgay>();
        //     foreach(var itemChamCongChiTiet in lstChamCongChiTietNhanVien)
        //     {
        //         ChamCongVanPhongChiTietNhanVienNgay ChiTiet = new ChamCongVanPhongChiTietNhanVienNgay(itemChamCongChiTiet);
        //         var lstChiTietNgay = lstChamCongChiTietNhanVienNgay.Where(x => x.IdChamCongVanPhongChiTiet == itemChamCongChiTiet.IdGuid).ToList();
        //         float SoGioLam = 0;
        //         string MaChamCong = "";

        //         foreach(var ChiTietNgay in lstChiTietNgay)
        //         {
        //             SoGioLam += ChiTietNgay.SoGioLam;
        //             MaChamCong = ChiTietNgay.MaChamCong;
        //         }

        //         if (SoGioLam > 0)
        //             ChiTiet.KyHieuLam = SoGioLam.ToString();
        //         else
        //             ChiTiet.KyHieuLam = MaChamCong;

        //         lstChiTiet.Add(ChiTiet);
        //     }

        //     foreach(var itemChamCongNhanVien in lstChamCongThangCuaNhanVien)
        //     {
        //         itemChamCongNhanVien.lstChamCongNgay = lstChiTiet.Where(x => x.IdChamCongVanPhong == itemChamCongNhanVien.IdGuid).ToList();
        //     }

        //     return lstChamCongThangCuaNhanVien;
        // }

        // public List<HRM_ChamCongVanPhongChiTietNgay> GetChamCongChiTietNgay(string IdChamCongChiTiet, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     return db.HRM_ChamCongVanPhongChiTietNgay.Where(x => x.IdChamCongVanPhongChiTiet == IdChamCongChiTiet).ToList();
        // }

        // public dynamic UpdateChamCongChiTietNgay(List<HRM_ChamCongVanPhongChiTietNgay> lstChamCongChiTietNgay, string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     if (lstChamCongChiTietNgay.Count == 0)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không có dữ liệu để cập nhật");

        //     string IdChamCongVanPhong = "";
        //    foreach(var item in lstChamCongChiTietNgay)
        //     {
        //         if (string.IsNullOrEmpty(item.IdGuid))
        //             continue;

        //         IdChamCongVanPhong = item.IdChamCongVanPhong;

        //         db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //     }

        //     var itemChamCong = db.HRM_ChamCongVanPhong.FirstOrDefault(x=>x.IdGuid == IdChamCongVanPhong);

        //     if(itemChamCong != null)
        //     {
        //         itemChamCong.TongNgayLamViec = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => x.IdChamCongVanPhong == IdChamCongVanPhong).Sum(x => x.NgayLuong);
        //     }


        //     try
        //     {
        //         db.SaveChanges();
        //         return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
        //     }
        //     catch (Exception ex)
        //     {
        //         DefineData.logger.ErrorFormat("UpdateChamCongChiTietNgay Error {0}", ex.Message);
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //     }

        // }


        // public dynamic KiemTraTabTrangThaiChamCongVanPhongTongHop(string idCurrentUser = "", ApplicationDbContext db = null)
        // {
        //     bool bChuaXuLy = false;
        //     bool bDaXyLy = false;
        //     bool bTongHop = false;

        //     List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eActionTongHop.ToString(), idCurrentUser);
        //     if (PhanQuyens == null)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền");

        //     DefineData.eRigth eRigth = DefineData.eRigth.CHUYENDUYET;
        //     BangPhanQuyen PQ = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //     if (PQ.GioiHan != (int)DefineData.eGioiHan.NONE)
        //     {
        //         bChuaXuLy = true;
        //         bDaXyLy = true;
        //         goto tt;
        //     }

        //     eRigth = DefineData.eRigth.PHEDUYET;
        //     PQ = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //     if (PQ.GioiHan != (int)DefineData.eGioiHan.NONE)
        //     {
        //         bChuaXuLy = true;
        //         bDaXyLy = true;
        //         goto tt;
        //     }

        //     eRigth = DefineData.eRigth.PHEDUYET2;
        //     PQ = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //     if (PQ.GioiHan != (int)DefineData.eGioiHan.NONE)
        //     {
        //         bChuaXuLy = true;
        //         bDaXyLy = true;
        //         goto tt;
        //     }

        //     bTongHop = true;
        //     tt:
        //     dynamic dnn = new DynamicObject();
        //     dnn.ChuaXuLy = bChuaXuLy;
        //     dnn.DaXyLy = bDaXyLy;
        //     dnn.TongHop = bTongHop;
        //     return dnn;
        // }

        // public dynamic KiemTraButtonChamCongVanPhongTongHop(string Id, string idCurrentUser, ApplicationDbContext db = null)
        // {
        //     bool bChuyenTiep = true;
        //     bool bKhongDuyet = true;
        //     bool bXoa = false;
        //     bool bThemMoiChiPhi = false;
        //     bool bGhi = true;
        //     bool isDuyetThay = false;

        //     // item kế hoạch chi
        //     var item = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id);

        //     byte TrangThai = 0;
        //     if (item != null)
        //         TrangThai = item.TrangThai;


        //     List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eActionTongHop.ToString(), idCurrentUser);
        //     DefineData.eRigth eRigth = DefineData.eRigth.PHEDUYET;
        //     BangPhanQuyen pq = null;

        //     if (TrangThai == TrangThaiTongHopChuaChuyenDuyet)
        //     {
        //         bXoa = true;
        //     }

        //     #region ChuyenTiep
        //     if (TrangThai == TrangThaiTongHopGiamDocDaDuyet)
        //     {
        //         bChuyenTiep = false;
        //         bGhi = false;
        //     }
        //     else
        //     {
        //         eRigth = DefineData.eRigth.CHUYENDUYET;
        //         if (TrangThai == TrangThaiTongHopChoGiamDocDuyet)
        //             eRigth = DefineData.eRigth.PHEDUYET;

        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan == (int)DefineData.eGioiHan.NONE)
        //         {
        //             bChuyenTiep = false;
        //             bGhi = false;
        //         }
        //     }
        //     #endregion

        //     #region KhongDuyet
        //     if (TrangThai == TrangThaiTongHopChoGiamDocDuyet)
        //     {
        //         eRigth = DefineData.eRigth.KHONGDUYET;
        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan == (int)DefineData.eGioiHan.NONE)
        //             bKhongDuyet = false;
        //     }
        //     else
        //         bKhongDuyet = false;
        //     #endregion

        //     if (TrangThai == TrangThaiTongHopChuaChuyenDuyet
        //         || TrangThai == TrangThaiTongHopGiamDocKhongDuyet)
        //     {
        //         isDuyetThay = bChuyenTiep;
        //     }


        //     dynamic dnn = new DynamicObject();
        //     dnn.ChuyenTiep = bChuyenTiep;
        //     dnn.Ghi = bChuyenTiep;
        //     dnn.KhongDuyet = bKhongDuyet;
        //     dnn.Xoa = bXoa;
        //     dnn.ThemMoiChiPhi = bThemMoiChiPhi;
        //     dnn.isDuyetThay = isDuyetThay;
        //     return dnn;
        // }

        // public dynamic ChuyenTiepChamCongVanPhongTongHop(string Id, CurrentUser CurrentUser, ApplicationDbContext db = null)
        // {

        //     //return "";
        //     // Kiểm tra lại quyền của user
        //     //List<PhanQuyen> PhanQuyens = GetPhanQuyens(userid, DefineData.eAction.KEHOACHDOANHTHUNAM);

        //     string idCurrentUser = CurrentUser.Id;

        //     var KeHoachTongHop = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id);
        //     if (KeHoachTongHop == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm đề nghị tuyển dụng");

        //     byte TrangThai = KeHoachTongHop.TrangThai;


        //     List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eActionTongHop.ToString(), idCurrentUser);

        //     BangPhanQuyen pq = null;
        //     DefineData.eRigth eRigth = DefineData.eRigth.CHUYENDUYET;


        //     bool isUpdate = false;
        //     if (TrangThai == TrangThaiTongHopChuaChuyenDuyet || TrangThai == TrangThaiTongHopGiamDocKhongDuyet)
        //     {
        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {
        //             KeHoachTongHop.TrangThai = TrangThaiTongHopGiamDocDaDuyet;
        //             isUpdate = true;
        //             eRigth = DefineData.eRigth.PHEDUYET;
        //         }

        //     }
        //     else
        //         if (TrangThai == TrangThaiTongHopChoGiamDocDuyet)
        //     {
        //         eRigth = DefineData.eRigth.PHEDUYET;
        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {
        //             KeHoachTongHop.TrangThai = TrangThaiTongHopGiamDocDaDuyet;
        //             isUpdate = true;
        //             eRigth = DefineData.eRigth.PHEDUYET2;
        //         }
        //     }


        //     if (isUpdate)
        //     {
        //         try
        //         {
        //             db.SaveChanges();
        //         }
        //         catch (Exception ex)
        //         {
        //             DefineData.logger.ErrorFormat("ChuyenTiepChamCongVanPhongTongHop data: {0}, Error {1}", JsonConvert.SerializeObject(KeHoachTongHop), ex.Message);
        //             return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //         }

        //         if (KeHoachTongHop.TrangThai == TrangThaiTongHopChoGiamDocDuyet)
        //         {
        //             AddNotification(CurrentUser, eActionTongHop, eRigth, "Gửi yêu cầu phê duyệt ");
        //         }

        //         return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp quy trình thành công!");
        //     }
        //     else
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền chuyển tiếp quy trình!");
        // }

        // public dynamic KhongDuyetChamCongVanPhongTongHop(string Id, CurrentUser CurrentUser, ApplicationDbContext db = null)
        // {
        //     var KeHoachTongHop = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id);
        //     if (KeHoachTongHop == null)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy quy trình công tác phí tổng hợp");

        //     byte TrangThai = KeHoachTongHop.TrangThai;

        //     List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eActionTongHop.ToString(), CurrentUser.Id);

        //     BangPhanQuyen pq = null;
        //     DefineData.eRigth eRigth = DefineData.eRigth.KHONGDUYET;

        //     bool isUpdate = false;
        //     if (TrangThai == TrangThaiTongHopChoGiamDocDuyet)
        //     {
        //         pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
        //         if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
        //         {
        //             KeHoachTongHop.TrangThai = TrangThaiTongHopGiamDocKhongDuyet;
        //             isUpdate = true;
        //             eRigth = DefineData.eRigth.CHUYENDUYET;
        //         }

        //     }

        //     if (isUpdate)
        //     {
        //         try
        //         {
        //             db.SaveChanges();
        //         }
        //         catch (Exception ex)
        //         {
        //             DefineData.logger.ErrorFormat("KhongDuyetChamCongVanPhongTongHop data: {0}, Error {1}", JsonConvert.SerializeObject(KeHoachTongHop), ex.Message);
        //             return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //         }

        //         if (KeHoachTongHop.TrangThai == TrangThaiTongHopGiamDocKhongDuyet)
        //         {
        //             List<string> lstIdUserNhanThongBao = new List<string>() { KeHoachTongHop.CreatedBy };
        //             AddNotification(CurrentUser, eActionTongHop, lstIdUserNhanThongBao, "Không duyệt yêu cầu ");
        //         }

        //         return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp quy trình thành công!");
        //     }
        //     else
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền chuyển tiếp quy trình!");

        // }

        // public dynamic DuyetThayChamCongVanPhongTongHop(HRM_QuyTrinhCongTacPhiTongHop item, string idCurrentUser = "", ApplicationDbContext db = null
        //)
        // {
        //     #region Quyền
        //     List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eActionTongHop.ToString(), idCurrentUser);
        //     DefineData.eRigth eRigth = DefineData.eRigth.CHUYENDUYET;
        //     BangPhanQuyen pq = PhanQuyens.FirstOrDefault(x => x.MaRight == eRigth.ToString());
        //     if (pq == null || pq.GioiHan == (int)DefineData.eGioiHan.NONE)
        //         return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền duyệt thay quy trình!");
        //     #endregion

        //     var lstFileDinhKem = item.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
        //     if (lstFileDinhKem.Count == 0)
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Tải lên file duyệt thay");

        //     FileUploadController FileUpload = new FileUploadController();
        //     FileUpload.AddFileFromHDFile(lstFileDinhKem, item.IdGuid, DefineData.eTable.HRM_QuyTrinhCongTacPhiTongHop.ToString(), db
        //         , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());


        //     var itemDb = db.HRM_QuyTrinhCongTacPhiTongHop.FirstOrDefault(x => x.IdGuid == item.IdGuid);
        //     if (itemDb.TrangThai == TrangThaiTongHopChuaChuyenDuyet
        //         || item.TrangThai == TrangThaiTongHopGiamDocKhongDuyet)
        //     {
        //         itemDb.TrangThai = TrangThaiTongHopGiamDocDaDuyet;
        //         try
        //         {
        //             db.SaveChanges();
        //             return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
        //         }
        //         catch (Exception ex)
        //         {
        //             DefineData.logger.ErrorFormat("DuyetThayChamCongVanPhongTongHop data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
        //             return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
        //         }
        //     }
        //     else
        //     {
        //         return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển duyệt trước đó");
        //     }

        // }

        // #endregion

        #region Import file Chấm công nhân viên 

        //public MessageReturn ImportBangChamCong(string aFileName, ApplicationDbContext aContext)
        //{
        //    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu chấm công thành công");

        //    if (aContext.currentUser.Id.IsEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
        //    }

        //    if (aFileName.IsEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu tệp đính kèm");
        //    }

        //    var pathTemPlate =PathUploads+ aFileName;

        //    Stream sTream = File.OpenRead(pathTemPlate);
        //    ExcelPackage package = new ExcelPackage(sTream);
        //    sTream.Close();

        //    var sheet1 = package.Workbook.Worksheets["Sheet1"];

        //    int row = 5;
        //    int maxrow = sheet1.Dimension.Rows;

        //    var toDictNhanVien = aContext.HRM_NhanVien.ToList().ToDict(x => x.MaNhanVien);
        //    var listModelExcel = new List<ImportChamCongModel>();
        //    var toGroupErr = new Dictionary<string, List<string>>();
        //    toGroupErr.Add("NGAY", new List<string>());
        //    toGroupErr.Add("MA_NHAN_VIEN", new List<string>());
        //    toGroupErr.Add("PHONG_BAN", new List<string>());
        //    toGroupErr.Add("CHUC_VU", new List<string>());
        //    for (int index = row; maxrow >= index; index++)
        //    {
        //        var model = new ImportChamCongModel();

        //        int STT = 1;
        //        var ngay = sheet1.Cells[index, STT].Value;
        //        if (ngay == null)
        //        {
        //            toGroupErr["NGAY"].Add(index.ToString());
        //        }

        //        if (ngay != null)
        //        {
        //            DateTime date;
        //            CultureInfo provider = CultureInfo.InvariantCulture;
        //            var checkDate = DateTime.TryParseExact(ngay.ToString(), new string[] { "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy-MM-dd" }, provider, DateTimeStyles.None, out date);
        //            if (checkDate == false)
        //            {
        //                toGroupErr["NGAY"].Add(index.ToString());
        //            }
        //        }

        //        model.Ngay = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        model.Thu = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        var maNhanVien = sheet1.Cells[index, STT].Value;
        //        if(maNhanVien == null)
        //        {
        //            toGroupErr["MA_NHAN_VIEN"].Add(index.ToString());
        //        }
        //        if(maNhanVien != null)
        //        {
        //            if (!toDictNhanVien.ContainsKey(maNhanVien.ToString()))
        //            {
        //                toGroupErr["MA_NHAN_VIEN"].Add(index.ToString());
        //            }
        //        }

        //        model.MaNhanVien = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        model.TenNhanVien = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        var phongBan = sheet1.Cells[index, STT].Value;
        //        if(phongBan == null)
        //        {
        //            toGroupErr["PHONG_BAN"].Add(index.ToString());
        //        }

        //        model.PhongBan = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        var chucVu = sheet1.Cells[index, STT].Value;
        //        if(chucVu == null)
        //        {
        //            toGroupErr["CHUC_VU"].Add(index.ToString());
        //        }

        //        model.ChucVu = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        model.Vao = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        model.Ra = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();
        //        STT++;

        //        model.TongGio = sheet1.Cells[index, STT].Value == null ? "" : sheet1.Cells[index, STT].Value.ToString();

        //        listModelExcel.Add(model);
        //    }

        //    if (toGroupErr["NGAY"].IsNotEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Những dòng sau có giá trị ngày bị null hoặc không đúng định dạng ngày dd/MM/yyyy hoặc dd-MM-yyyy : {toGroupErr["NGAY"].Join(",")}");
        //    }

        //    if (toGroupErr["MA_NHAN_VIEN"].IsNotEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Những dòng sau có giá trị mã nhân viên bị null hoặc mã nhân viên không có trong hệ thống : {toGroupErr["MA_NHAN_VIEN"].Join(",")}");
        //    }

        //    if (toGroupErr["PHONG_BAN"].IsNotEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Những dòng sau có giá trị phòng ban bị null : { toGroupErr["PHONG_BAN"].Join(",")}");
        //    }

        //    if (toGroupErr["CHUC_VU"].IsNotEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Những dòng sau có giá trị chức vụ bị null : {toGroupErr["CHUC_VU"].Join(",")}");
        //    }

        //    var listCreate = new List<HRM_ChamCong>();
        //    foreach (var item in listModelExcel)
        //    {
        //        var model = new HRM_ChamCong();
        //        model.IdGuid = Guid.NewGuid().ToString();
        //        model.ChucVu = item.ChucVu;
        //        model.HoTenNhanVien = item.MaNhanVien.GetValueFromDic(toDictNhanVien, x => x.TenDayDu);

        //        if (item.Vao.IsNotEmpty())
        //        {
        //            var listGioVao = item.Vao.Split(':');
        //            if(listGioVao != null)
        //            {
        //                model.Gio_Vao = int.Parse(listGioVao[0]);
        //                model.Phut_Vao = int.Parse(listGioVao[1]);
        //            }
        //        }

        //        if (item.Ra.IsNotEmpty())
        //        {
        //            var listGioRa = item.Ra.Split(':');
        //            if(listGioRa != null)
        //            {
        //                model.Gio_Ra = int.Parse(listGioRa[0]);
        //                model.Phut_Ra = int.Parse(listGioRa[1]);
        //            }
        //        }

        //        model.PhongBan = item.PhongBan;
        //        model.MaNhanVien = item.MaNhanVien;
        //        model.Ngay = Convert.ToDateTime(item.Ngay);
        //        model.IdNhanVien = item.MaNhanVien.GetValueFromDic(toDictNhanVien,x=>x.IdGuid);
        //        model.Thu = item.Thu;
        //        model.TongGio = Convert.ToDouble(item.TongGio);
        //        model.Thang = model.Ngay.Month;
        //        model.Nam = model.Ngay.Year;

        //        listCreate.Add(model);
        //    }

        //    aContext.HRM_ChamCong.AddRange(listCreate);
        //    aContext.SaveChanges();
        //    return result;
        //}

        #endregion

        //public dynamic GetDanhSachChamCong(dynamic dyn, string idCurrentUser, ApplicationDbContext db = null)
        //{



        //    int CurrentPage = dyn.CurrentPage == null ? 0 : dyn.CurrentPage;
        //    int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
        //    string LoaiBangLuong = dyn.LoaiBangLuong == null ? "" : dyn.LoaiBangLuong;
        //    int Nam = dyn.Nam == null ? DateTime.Now.Year : dyn.Nam;
        //    int Thang = dyn.Thang == null ? DateTime.Now.Month : dyn.Thang;

        //    string TrangThaiHieuLucHopDong = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
        //    string LamViecThang = DefineData.LamViecThang.ToString();
        //    string LamViecTheoGio = DefineData.LamViecTheoGio.ToString();
        //    string LamViecTheoCa = DefineData.LamViecTheoCa.ToString();
        //    var danhSachChamCong = (from a in db.HRM_ChamCong
        //                            where a.Nam == Nam && a.Thang == Thang && a.TongGio != 0
        //                            group a by a.IdNhanVien into g
        //                            select new
        //                            {
        //                                IdNhanVien = g.Key,
        //                                TongGio = g.Sum(r => r.TongGio),
        //                                TongNgay = g.Count(),
        //                                Thang = Thang,
        //                                Nam = Nam
        //                            }).ToList();
        //    List<string> lstIdNhanVien = danhSachChamCong.Select(r => r.IdNhanVien).ToList();

        //    var danhSachNhanVien = (from a in db.HRM_NhanVien
        //                            join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
        //                            join e in db.HRM_DM_HinhThucLamViec on b.IdHinhThucLamViec equals e.IdGuid

        //                            //join c in db.HRM_DM_ChucVu on b.IdChucVu equals c.IdGuid into dc
        //                            //from d in dc.DefaultIfEmpty()
        //                                //join f in db.HRM_DM_BoPhan on b.IdPhongBan equals f.IdGuid into df
        //                                //from fd in df.DefaultIfEmpty()
        //                            where lstIdNhanVien.Contains(a.IdGuid) && b.TrangThai == TrangThaiHieuLucHopDong
        //                            select new
        //                            {
        //                                a.TenDayDu,
        //                                a.MaNhanVien,
        //                             //   ChucVu = d.Ten,
        //                                //  b.LuongDoanhThu,
        //                                a.IdGuid,
        //                                e.Ma,
        //                                //b.LuongCoBan,
        //                                //b.isCoDongBaoHiemXaHoi,
        //                                //b.ThuongChuyenCan,
        //                                //b.HoTroDuAn,
        //                                //b.AnTruaTrangPhuc,
        //                                //fd.TenBoPhan,
        //                                //fd.IdBoPhanEOS,
        //                            }).ToList();

        //    var query = (from a in danhSachChamCong
        //                 join b in danhSachNhanVien on a.IdNhanVien equals b.IdGuid
        //                 select new DanhSachChamCongThangViewModel
        //                 {
        //                     IdNhanVien = a.IdNhanVien,
        //                     MaNhanVien = b.MaNhanVien,
        //                     Nam = a.Nam,
        //                     Thang = a.Thang,
        //                     TongSoGio = a.TongGio,
        //                     NgayCongThucTe = a.TongNgay,
        //                     //LuongThucTe = b.LuongDoanhThu,
        //                   //  ChucVu = b.ChucVu,
        //                     MaHinhThucLamViec = b.Ma == LamViecThang ? 0 : (b.Ma == LamViecTheoGio ? 1 : 2),
        //                     NgayCongCoSo = b.Ma == LamViecThang ? 26 : 0,
        //                     TenNhanVien = b.TenDayDu,
        //                     //LuongCoBan = b.LuongCoBan,
        //                     //TenBoPhan = b.TenBoPhan,
        //                     //LuongNgayCongThucTe = b.Ma == LamViecThang ? ((b.LuongDoanhThu / 26) * a.TongNgay) : (b.Ma == LamViecTheoGio ? (a.TongGio * b.LuongDoanhThu) : 0),
        //                     //ThucLinh = b.Ma == LamViecThang ? ((b.LuongDoanhThu / 26) * a.TongNgay + b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn - (b.isCoDongBaoHiemXaHoi == true ? b.LuongCoBan * 0.105 : 0)) : (b.Ma == LamViecTheoGio ? (a.TongGio * b.LuongDoanhThu + b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn) : (b.LuongDoanhThu * a.TongNgay + +b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn)),
        //                     //TienDongBaoHiem = b.isCoDongBaoHiemXaHoi == true ? b.LuongCoBan * 0.105 : 0,
        //                     //PhuCapHoTro = b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn
        //                 }).ToList();

        //    dynamic result = new DynamicObject();

        //    int count = query.Count();
        //    var listResult = query.OrderByDescending(q => q.MaNhanVien).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

        //    result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
        //    result.items = listResult;

        //    return result;


        //}

        //public dynamic GetChiTietNgayCongNhanVien(dynamic dyn, string idCurrentUser, ApplicationDbContext db = null)

        //{
        //    var Date = DateTime.Now;
        //    int CurrentPage = dyn.CurrentPage == null ? 0 : dyn.CurrentPage;
        //    int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
        //    string IdNhanVien = dyn.IdNhanVien == null ? "" : dyn.IdNhanVien;
        //    DateTime NgayBatDau = dyn.NgayBatDau == null ? Date : dyn.NgayBatDau;
        //    DateTime NgayKetThuc = dyn.NgayKetThuc == null ? Date : dyn.NgayKetThuc;
        //    string TrangThaiHieuLucHopDong = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();

        //    var query = from a in db.HRM_NhanVien
        //                join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
        //                join c in db.HRM_DM_ChucVu on b.IdChucVu equals c.IdGuid
        //                join d in db.HRM_ChamCong on a.IdGuid equals d.IdNhanVien
        //                where a.IdGuid == IdNhanVien && b.TrangThai == TrangThaiHieuLucHopDong
        //                && d.Ngay >= NgayBatDau && NgayKetThuc >= d.Ngay
        //                select new ChiTietChamCongNhanVienViewModel
        //                {
        //                    TenNhanVien = a.TenDayDu,
        //                    TongGio = d.TongGio,
        //                    ChucVu = c.Ten,
        //                    GioRa = d.Gio_Ra + ":" + d.Phut_Ra,
        //                    GioVao = d.Gio_Vao + ":" + d.Phut_Vao,
        //                    IdNhanVien = a.IdGuid,
        //                    MaNhanVien = a.MaNhanVien,
        //                    NgayChamCong = d.Ngay,

        //                };
        //    dynamic result = new DynamicObject();
        //    int count = query.Count();
        //    var listResult = query.OrderByDescending(q => q.TenNhanVien).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

        //    result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
        //    result.items = listResult;
        //    return result;
        //}

        //public dynamic getLuongPhongBanBoPhan(int Nam, int Thang, ApplicationDbContext db = null)
        //{
        //    string LamViecThang = DefineData.LamViecThang.ToString();
        //    string LamViecTheoGio = DefineData.LamViecTheoGio.ToString();
        //    string LamViecTheoCa = DefineData.LamViecTheoCa.ToString();
        //    string TrangThaiHieuLucHopDong = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
        //    var danhSachChamCong = (from a in db.HRM_ChamCong
        //                            where a.Nam == Nam && (a.Thang == Thang) && a.TongGio != 0
        //                            group a by a.IdNhanVien into g
        //                            select new
        //                            {
        //                                IdNhanVien = g.Key,
        //                                TongGio = g.Sum(r => r.TongGio),
        //                                TongNgay = g.Count(),
        //                                Thang = Thang,
        //                                Nam = Nam
        //                            }).ToList();
        //    List<string> lstIdNhanVien = danhSachChamCong.Select(r => r.IdNhanVien).ToList();

        //    var danhSachNhanVien = (from a in db.HRM_NhanVien
        //                            join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
        //                            join e in db.HRM_DM_HinhThucLamViec on b.IdHinhThucLamViec equals e.IdGuid
        //                            join c in db.HRM_DM_ChucVu on b.IdChucVu equals c.IdGuid into dc
        //                            from d in dc.DefaultIfEmpty()
        //                                //join f in db.HRM_DM_BoPhan on b.IdPhongBan equals f.IdGuid into df
        //                                //from fd in df.DefaultIfEmpty()
        //                            where lstIdNhanVien.Contains(a.IdGuid) && b.TrangThai == TrangThaiHieuLucHopDong
        //                            select new
        //                            {
        //                                a.TenDayDu,
        //                                a.MaNhanVien,
        //                                ChucVu = d.Ten,
        //                                //b.LuongDoanhThu,
        //                                //a.IdGuid,
        //                                //e.Ma,
        //                                //b.LuongCoBan,
        //                                //b.isCoDongBaoHiemXaHoi,
        //                                //b.ThuongChuyenCan,
        //                                //b.HoTroDuAn,
        //                                //b.AnTruaTrangPhuc,
        //                                //b.NhomBoPhan,
        //                                //fd.IdBoPhanEOS,
        //                            }).ToList();

        //    //var query = (from a in danhSachChamCong
        //    //             join b in danhSachNhanVien on a.IdNhanVien equals b.IdGuid
        //    //             select new DanhSachChamCongThangViewModel
        //    //             {
        //    //                 IdNhanVien = a.IdNhanVien,
        //    //                 MaNhanVien = b.MaNhanVien,
        //    //                 Nam = a.Nam,
        //    //                 Thang = a.Thang,
        //    //                 TongSoGio = a.TongGio,
        //    //                 NgayCongThucTe = a.TongNgay,
        //    //                 LuongThucTe = b.LuongDoanhThu,
        //    //                 ChucVu = b.ChucVu,
        //    //                 MaHinhThucLamViec = b.Ma == LamViecThang ? 0 : (b.Ma == LamViecTheoGio ? 1 : 2),
        //    //                 NgayCongCoSo = b.Ma == LamViecThang ? 26 : 0,
        //    //                 TenNhanVien = b.TenDayDu,
        //    //                 LuongCoBan = b.LuongCoBan,
        //    //                 TenBoPhan = b.NhomBoPhan,
        //    //               //  IdBoPhanEOS = b.IdBoPhanEOS,
        //    //                 LuongNgayCongThucTe = b.Ma == LamViecThang ? ((b.LuongDoanhThu / 26) * a.TongNgay) : (b.Ma == LamViecTheoGio ? (a.TongGio * b.LuongDoanhThu) : 0),
        //    //                 ThucLinh = b.Ma == LamViecThang ? ((b.LuongDoanhThu / 26) * a.TongNgay + b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn - (b.isCoDongBaoHiemXaHoi == true ? b.LuongCoBan * 0.105 : 0)) : (b.Ma == LamViecTheoGio ? (a.TongGio * b.LuongDoanhThu + b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn) : (b.LuongDoanhThu * a.TongNgay + +b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn)),
        //    //                 TienDongBaoHiem = b.isCoDongBaoHiemXaHoi == true ? b.LuongCoBan * 0.105 : 0,
        //    //                 TienCongTyDongBaoHiem = b.isCoDongBaoHiemXaHoi == true ? b.LuongCoBan * 0.22 : 0,
        //    //                 PhuCapHoTro = b.AnTruaTrangPhuc + b.ThuongChuyenCan + b.HoTroDuAn
        //    //             }).ToList();
        //    var results = new LuongBoPhanModel();
        //    //var results = query.GroupBy(n => new { n.Nam, n.Thang, n.IdBoPhanEOS, n.TenBoPhan })
        //    //    .Select(g => new LuongBoPhanModel
        //    //    {
        //    //        IdBoPhanEOS = g.Key.IdBoPhanEOS,
        //    //        TenBoPhan = g.Key.TenBoPhan,
        //    //        TienNguoiLaoDongNopBaoHiem = g.Sum(r => r.TienDongBaoHiem),
        //    //        LuongBaoHiem = g.Sum(r => r.LuongCoBan),
        //    //        Nam = g.Key.Nam,
        //    //        Thang = g.Key.Thang,
        //    //        ThucLinh=g.Sum(r=>r.ThucLinh),
        //    //        PhuCapHoTro = g.Sum(r => r.PhuCapHoTro),
        //    //        TienCongTyNopBaoHiem   =g.Sum(r=>r.TienCongTyDongBaoHiem)
        //    //    }).ToList();
        //    return results;
        //}


    }
}
