#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using HRM.Api.Models;
using HRM.Api.Models.NotificationModel;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static HRM.Api.Services.NhanVien.FuncKeHoachLamViec;
using static HRM.Api.Helper.HelperConstant;

namespace HRM.Api.Services.DaoTao
{
    public class FuncDaoTao : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncDaoTao(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncDaoTao> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

        #region kế hoạch đào tạo
        public dynamic GetNextMaKeHoachDaoTao(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_KeHoachDaoTao.Select(x => x.SoPhieu), DefineData.MaKeHoachDaoTao));
        public dynamic GetListQuyTrinhKeHoachDaoTao(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_KeHoachDaoTao> query = db.HRM_KeHoachDaoTao;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));

                List<HRM_KeHoachDaoTao> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;

                foreach (var item in itemList)
                {
                    item.NgayLapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayLap);
                    //item.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDeNghi);
                    //item.TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == item.IdTongHopDeNghiTuyenDung).FirstOrDefault();
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
        public dynamic SetQuyTrinhKeHoachDaoTao(HRM_KeHoachDaoTao model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachDaoTao entityDB = db.HRM_KeHoachDaoTao.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (!SetTruocHamUpdate(db, _CallWebService, entityDB, model, err))
                        return err;

                    string check = FuncQuyTrinhKeHoachDaoTao(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);
                    }

                    if (!SetSauHamUpdate(db, entityDB, model, err))
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
        public dynamic ChuyenTiepQuyTrinhKeHoachDaoTao(HRM_KeHoachDaoTao QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachDaoTao pcuDatabase = db.HRM_KeHoachDaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    string Error = FuncQuyTrinhKeHoachDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
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
        public dynamic KhongDuyetQuyTrinhKeHoachDaoTao(HRM_KeHoachDaoTao QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachDaoTao pcuDatabase = db.HRM_KeHoachDaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    string Error = FuncQuyTrinhKeHoachDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }

                    if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }




                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Không duyệt thành công!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
                }
            }
        }
        public dynamic GetByIdQuyTrinhKeHoachDaoTao(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_KeHoachDaoTao model = db.HRM_KeHoachDaoTao.Find(Id);
            if (model == null) return null;
            model.NgayLapUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayLap);
            var listChiTiet = db.HRM_KeHoachDaoTaoChiTiet.Where(r => r.IdKeHoach == Id).ToList();
            var listid = listChiTiet.Select(a => a.IdGuid).Distinct().ToList();
            var lstNhanVien = db.HRM_KeHoachDaoTaoNhanVien.Where(r => listid.Contains(r.IdChiTietKeHoach)).ToList();
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            foreach (var item in lstNhanVien)
            {
                var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVuEOS);
                if (chucVu != null)
                    item.TenChucVu = chucVu.TenChucVu;
            }
            //var listIdNv = lstNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
            //var nhanViens = db.HRM_NhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();


            foreach (var item in listChiTiet)
            {
                if (!string.IsNullOrEmpty(item.IddmNoiDungDaoTao))
                    item.listIdNoiDung = item.IddmNoiDungDaoTao.Split(',').ToList();
                item.listNhanVien = lstNhanVien.Where(r => r.IdChiTietKeHoach == item.IdGuid).ToList();
            }

            model.listChiTiet = listChiTiet;
            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            return model;
        }

        public dynamic DeleteQuyTrinhKeHoachDaoTao(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_KeHoachDaoTao model = db.HRM_KeHoachDaoTao.Find(Id);
                var listChiTiet = db.HRM_KeHoachDaoTaoChiTiet.Where(r => r.IdKeHoach == Id).ToList();
                foreach (var item in listChiTiet)
                {

                    var lstNhanVien = db.HRM_KeHoachDaoTaoNhanVien.Where(r => r.IdChiTietKeHoach == item.IdGuid).ToList();
                    db.HRM_KeHoachDaoTaoNhanVien.RemoveRange(lstNhanVien);
                }
                db.HRM_KeHoachDaoTao.Remove(model);
                db.HRM_KeHoachDaoTaoChiTiet.RemoveRange(listChiTiet);

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public string FuncQuyTrinhKeHoachDaoTao(HRM_KeHoachDaoTao model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.NgayLap = dungchung.UnixTimeStampToDateTime(model.NgayLapUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdDuAn = model.IdCongTyEOS;
                    if (string.IsNullOrEmpty(model.eAction))
                        model.eAction = DefineData.eAction.QUYTRINHKEHOACHDAOTAO.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_KeHoachDaoTao.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var listChiTiet = db.HRM_KeHoachDaoTaoChiTiet.Where(r => r.IdKeHoach == model.IdGuid).ToList();
                var listid = listChiTiet.Select(a => a.IdGuid).Distinct().ToList();
                var lstNhanVien = db.HRM_KeHoachDaoTaoNhanVien.Where(r => listid.Contains(r.IdChiTietKeHoach)).ToList();
                db.HRM_KeHoachDaoTaoNhanVien.RemoveRange(lstNhanVien);
                db.HRM_KeHoachDaoTaoChiTiet.RemoveRange(listChiTiet);
                if (model.listChiTiet != null)
                {
                    foreach (var item in model.listChiTiet)
                    {
                        if (item.listIdNoiDung != null && item.listIdNoiDung.Count > 0)
                            item.IddmNoiDungDaoTao = string.Join(",", item.listIdNoiDung);
                        item.IdKeHoach = model.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        db.HRM_KeHoachDaoTaoChiTiet.Add(item);
                        if (item.listNhanVien != null && item.listNhanVien.Count > 0)
                            foreach (var itemCT in item.listNhanVien)
                            {
                                itemCT.IdChiTietKeHoach = item.IdGuid;
                                itemCT.IdGuid = Guid.NewGuid().ToString();
                                db.HRM_KeHoachDaoTaoNhanVien.Add(itemCT);
                            }

                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();

                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }

        }
        #endregion
        #region Tổng hợp
        public dynamic GetListKeHoachChuaTongHop(int Nam, long IdCongTyEOS, bool isDaoTaoMoi, ApplicationDbContext db)
        {
            List<HRM_KeHoachDaoTao> listKeHoach = new List<HRM_KeHoachDaoTao>();
            listKeHoach = db.HRM_KeHoachDaoTao.Where(r => r.Nam == Nam && r.isDaoTaoMoi == isDaoTaoMoi && (r.IdTongHopKeHoach == "" || r.IdTongHopKeHoach == null) && r.IdCongTyEOS == IdCongTyEOS && r.isKetThuc == true).ToList();
            return listKeHoach;
        }
        public dynamic GetTongHopKeHoachDaoTao(List<string> listIdKeHoach, ApplicationDbContext db)
        {
            var listKeHoachDaoTaoChiTiet = db.HRM_KeHoachDaoTaoChiTiet.Where(r => listIdKeHoach.Contains(r.IdKeHoach)).ToList();
            //HRM_KeHoachDaoTao KHDT = db.HRM_KeHoachDaoTao.FirstOrDefault(a => listIdKeHoach.Contains(a.IdGuid));
            //bool isDaoTaoMoi = KHDT.isDaoTaoMoi;
            List<string> listIdKeHoachDaoTaoChiTiet = listKeHoachDaoTaoChiTiet.Select(r => r.IdGuid).ToList();
            var listNhanVien = db.HRM_KeHoachDaoTaoNhanVien.Where(r => listIdKeHoachDaoTaoChiTiet.Contains(r.IdChiTietKeHoach)).ToList();
            List<string> ListIddmNoiDung = listKeHoachDaoTaoChiTiet.GroupBy(r => r.IddmNoiDungDaoTao).Select(r => r.Key).ToList();
            var lstTongHopKeHoach = new List<HRM_TongHop_KeHoachDaoTaoChiTiet>();
            var dmNoidung = db.HRM_dmNoiDungDaoTao.ToList();

            foreach (var item in ListIddmNoiDung)
            {
                HRM_TongHop_KeHoachDaoTaoChiTiet itemTongHop = new HRM_TongHop_KeHoachDaoTaoChiTiet
                {
                    IddmNoiDungDaoTao = item,
                };


                var lstKeHoachByIdNoiDung = listKeHoachDaoTaoChiTiet.Where(r => r.IddmNoiDungDaoTao == item).ToList();
                foreach (var itemCT in lstKeHoachByIdNoiDung)
                {

                    if (itemCT.isThang1 == true)
                        itemTongHop.isThang1 = true;
                    if (itemCT.isThang2 == true)
                        itemTongHop.isThang2 = true;
                    if (itemCT.isThang3 == true)
                        itemTongHop.isThang3 = true;
                    if (itemCT.isThang4 == true)
                        itemTongHop.isThang4 = true;
                    if (itemCT.isThang5 == true)
                        itemTongHop.isThang5 = true;
                    if (itemCT.isThang6 == true)
                        itemTongHop.isThang6 = true;
                    if (itemCT.isThang7 == true)
                        itemTongHop.isThang7 = true;
                    if (itemCT.isThang8 == true)
                        itemTongHop.isThang8 = true;
                    if (itemCT.isThang9 == true)
                        itemTongHop.isThang9 = true;
                    if (itemCT.isThang10 == true)
                        itemTongHop.isThang10 = true;
                    if (itemCT.isThang11 == true)
                        itemTongHop.isThang11 = true;
                    if (itemCT.isThang12 == true)
                        itemTongHop.isThang12 = true;
                }

                List<string> lstIdKeHoachByIdNoiDung = lstKeHoachByIdNoiDung.Select(r => r.IdGuid).ToList();
                var listNhanVienChiTiet = listNhanVien.Where(r => lstIdKeHoachByIdNoiDung.Contains(r.IdChiTietKeHoach)).ToList();
                List<HRM_TongHop_KeHoachDaoTaoNhanVien> listNhanVienTongHop = new List<HRM_TongHop_KeHoachDaoTaoNhanVien>();
                foreach (var itemNV in listNhanVienChiTiet)
                {
                    HRM_TongHop_KeHoachDaoTaoNhanVien nhanVien = new HRM_TongHop_KeHoachDaoTaoNhanVien
                    {
                        IdChucVuEOS = itemNV.IdChucVuEOS,
                        IdBoPhanEOS = itemNV.IdBoPhanEOS,
                        TenNhanVien = itemNV.TenNhanVien,
                        IdNhanVien = itemNV.IdNhanVien,
                        MaNhanVien = itemNV.MaNhanVien,
                    };
                    listNhanVienTongHop.Add(nhanVien);

                }
                itemTongHop.listNhanVien = listNhanVienTongHop;
                itemTongHop.NoiDung = dmNoidung.Where(r => r.IdGuid == itemTongHop.IddmNoiDungDaoTao).Select(r => r.Ten).FirstOrDefault();
                lstTongHopKeHoach.Add(itemTongHop);
            }


            return lstTongHopKeHoach;

        }
        public dynamic GetNextMaTongHopKeHoachDaoTao(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_KeHoachDaoTaoTongHop.Select(x => x.SoPhieu), DefineData.MaKeHoachDaoTaoTongHop));
        public dynamic GetListQuyTrinhTongHopKeHoachDaoTao(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_KeHoachDaoTaoTongHop> query = db.HRM_KeHoachDaoTaoTongHop;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));

                List<HRM_KeHoachDaoTaoTongHop> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;

                foreach (var item in itemList)
                {
                    item.NgayTongHopUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTongHop);
                    //item.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDeNghi);
                    //item.TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == item.IdTongHopDeNghiTuyenDung).FirstOrDefault();
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
        public dynamic SetQuyTrinhTongHopKeHoachDaoTao(HRM_KeHoachDaoTaoTongHop model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachDaoTaoTongHop entityDB = db.HRM_KeHoachDaoTaoTongHop.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, entityDB, model, err))
                        return err;

                    string check = FuncQuyTrinhTongHopKeHoachDaoTao(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);
                    }

                    if (!SetSauHamUpdate(db, entityDB, model, err))
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
        public dynamic ChuyenTiepQuyTrinhTongHopKeHoachDaoTao(HRM_KeHoachDaoTaoTongHop QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_KeHoachDaoTaoTongHop pcuDatabase = db.HRM_KeHoachDaoTaoTongHop.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();



                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    string Error = FuncQuyTrinhTongHopKeHoachDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
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
        public dynamic KhongDuyetQuyTrinhTongHopKeHoachDaoTao(HRM_KeHoachDaoTaoTongHop QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_Dot_DaoTao pcuDatabase = db.HRM_Dot_DaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    string Error = FuncQuyTrinhTongHopKeHoachDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
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
        public dynamic GetByIdQuyTrinhTongHopKeHoachDaoTao(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_KeHoachDaoTaoTongHop model = db.HRM_KeHoachDaoTaoTongHop.Find(Id);
            if (model == null) return null;
            model.NgayTongHopUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTongHop);
            var listChiTiet = db.HRM_TongHop_KeHoachDaoTaoChiTiet.Where(r => r.IdTongHop == Id).ToList();
            foreach (var item in listChiTiet)
            {
                item.listNhanVien = db.HRM_TongHop_KeHoachDaoTaoNhanVien.Where(r => r.IdChiTietKeHoach == item.IdGuid).ToList();
            }
            var listIdKeHoach = db.HRM_KeHoachDaoTao_TongHop.Where(r => r.IdTongHop == Id).Select(r => r.IdKeHoach).ToList();
            model.listKeHoach = db.HRM_KeHoachDaoTao.Where(r => listIdKeHoach.Contains(r.IdGuid)).ToList();
            model.listChiTiet = listChiTiet;
            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            return model;
        }
        public dynamic DeleteQuyTrinhTongHopKeHoachDaoTao(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_KeHoachDaoTaoTongHop model = db.HRM_KeHoachDaoTaoTongHop.Find(Id);
                var listChiTiet = db.HRM_TongHop_KeHoachDaoTaoChiTiet.Where(r => r.IdTongHop == Id).ToList();
                foreach (var item in listChiTiet)
                {

                    var lstNhanVien = db.HRM_KeHoachDaoTaoNhanVien.Where(r => r.IdChiTietKeHoach == item.IdGuid).ToList();
                    db.HRM_KeHoachDaoTaoNhanVien.RemoveRange(lstNhanVien);
                }

                var listIdKeHoach = db.HRM_KeHoachDaoTao_TongHop.Where(r => r.IdTongHop == model.IdGuid).ToList();
                db.HRM_KeHoachDaoTaoTongHop.Remove(model);
                db.HRM_TongHop_KeHoachDaoTaoChiTiet.RemoveRange(listChiTiet);
                db.HRM_KeHoachDaoTao_TongHop.RemoveRange(listIdKeHoach);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public string FuncQuyTrinhTongHopKeHoachDaoTao(HRM_KeHoachDaoTaoTongHop model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.NgayTongHop = dungchung.UnixTimeStampToDateTime(model.NgayTongHopUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdDuAn = model.IdCongTyEOS;
                    model.eAction = DefineData.eAction.KEHOACHDAOTAOTONGHOP.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_KeHoachDaoTaoTongHop.Add(model);


                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    var listIdKeHoach = db.HRM_KeHoachDaoTao_TongHop.Where(r => r.IdTongHop == model.IdGuid).ToList();
                    db.HRM_KeHoachDaoTao_TongHop.RemoveRange(listIdKeHoach);
                }
                foreach (var item in model.listKeHoach)
                {
                    HRM_KeHoachDaoTao_TongHop KeHoach = new HRM_KeHoachDaoTao_TongHop
                    {
                        IdGuid = Guid.NewGuid().ToString(),
                        IdTongHop = model.IdGuid,
                        IdKeHoach = item.IdGuid
                    };
                    db.HRM_KeHoachDaoTao_TongHop.Add(KeHoach);
                }
                if (model.listChiTiet != null)
                {
                    foreach (var item in model.listChiTiet)
                    {
                        if (item.isXoa == true)
                            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                        else
                        {
                            item.IdTongHop = model.IdGuid;
                            if (string.IsNullOrEmpty(item.IdGuid))
                            {

                                item.IdGuid = Guid.NewGuid().ToString();
                                db.HRM_TongHop_KeHoachDaoTaoChiTiet.Add(item);
                            }
                            else
                            {
                                var chitietNhanVien = db.HRM_TongHop_KeHoachDaoTaoNhanVien.Where(r => r.IdChiTietKeHoach == item.IdGuid).ToList();
                                db.HRM_TongHop_KeHoachDaoTaoNhanVien.RemoveRange(chitietNhanVien);
                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }

                            foreach (var itemCT in item.listNhanVien)
                            {
                                if (itemCT.isXoa == true)
                                    db.Entry(itemCT).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                                else
                                {

                                    itemCT.IdChiTietKeHoach = item.IdGuid;
                                    itemCT.IdGuid = Guid.NewGuid().ToString();
                                    db.HRM_TongHop_KeHoachDaoTaoNhanVien.Add(itemCT);


                                }
                            }
                        }
                    }
                }
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();

                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }

        }
        #endregion
        #region Đơt đào tạo
        public dynamic GetKeHoachDaoTaoTongHop(int Nam, long IdCongTyEOS, ApplicationDbContext db)
        {
            List<HRM_KeHoachDaoTaoTongHop> lstItem = new List<HRM_KeHoachDaoTaoTongHop>();
            var lstChitiets = (from a in db.HRM_KeHoachDaoTaoTongHop
                               join b in db.HRM_TongHop_KeHoachDaoTaoChiTiet on a.IdGuid equals b.IdTongHop
                               where a.IdCongTyEOS == IdCongTyEOS && a.Nam == Nam && b.isDaoTao == false
                               select b).ToList();
            var listIdTongHop = lstChitiets.Select(r => r.IdTongHop).ToList();
            var listId = lstChitiets.Select(r => r.IdGuid).ToList();
            lstItem = db.HRM_KeHoachDaoTaoTongHop.Where(r => listIdTongHop.Contains(r.IdGuid)).ToList();
            var listNhanViens = db.HRM_TongHop_KeHoachDaoTaoNhanVien.Where(r => listId.Contains(r.IdChiTietKeHoach)).ToList();
            foreach (var item in lstItem)
            {
                var lstChitiet = lstChitiets.Where(r => r.IdTongHop == item.IdGuid).ToList();
                foreach (var itemCT in lstChitiet)
                {
                    itemCT.listNhanVien = listNhanViens.Where(r => r.IdChiTietKeHoach == itemCT.IdGuid).ToList();

                }
                item.listChiTiet = lstChitiet;
            }
            return lstItem;
        }
        public dynamic GetListKeHoachDaoTaoTongHopChiTiet(int Nam, long IdCongTyEOS, ApplicationDbContext db)
        {
            var lstChitiets = (from a in db.HRM_KeHoachDaoTaoTongHop
                               join b in db.HRM_TongHop_KeHoachDaoTaoChiTiet on a.IdGuid equals b.IdTongHop
                               join c in db.HRM_dmNoiDungDaoTao on b.IddmNoiDungDaoTao equals c.IdGuid
                               where a.IdCongTyEOS == IdCongTyEOS && a.Nam == Nam && b.isDaoTao == false
                               select new KeHoachDaoTaoChiTietViewModel
                               {
                                   IdChiTiet = b.IdGuid,
                                   IddmNoiDungDaoTao = b.IddmNoiDungDaoTao,
                                   IddmHinhThucDaoTao = b.IddmHinhThucDaoTao,
                                   isThang1 = b.isThang1,
                                   isThang2 = b.isThang2,
                                   isThang3 = b.isThang3,
                                   isThang4 = b.isThang4,
                                   isThang5 = b.isThang5,
                                   isThang6 = b.isThang6,
                                   isThang7 = b.isThang7,
                                   isThang8 = b.isThang8,
                                   isThang9 = b.isThang9,
                                   isThang10 = b.isThang10,
                                   isThang11 = b.isThang11,
                                   isThang12 = b.isThang12,
                                   IdQuyTrinhTongHop = a.IdGuid,
                                   KeHoachKinhPhi = b.KeHoachKinhPhi,
                                   Nam = a.Nam,
                                   NgayTongHop = a.NgayTongHop,
                                   SoNhanVien = b.SoNhanVien,
                                   NoiDung = b.NoiDung,
                                   SoPhieu = a.SoPhieu
                               }).ToList();
            var listId = lstChitiets.Select(r => r.IdChiTiet).ToList();
            var listNhanViens = db.HRM_TongHop_KeHoachDaoTaoNhanVien.Where(r => listId.Contains(r.IdChiTietKeHoach)).ToList();
            var dmNoiDungDaoTao = db.HRM_dmNoiDungDaoTao.ToList();
            foreach (var itemCT in lstChitiets)
            {
                itemCT.NgayTongHopUnix = dungchung.DateTimeToUnixTimeStamp(itemCT.NgayTongHop);
                itemCT.listNhanVien = listNhanViens.Where(r => r.IdChiTietKeHoach == itemCT.IdChiTiet).ToList();
                itemCT.TendmNoiDungDaoTao = dmNoiDungDaoTao.Where(r => r.IdGuid == itemCT.IddmNoiDungDaoTao).Select(r => r.Ten).FirstOrDefault();
            }

            return lstChitiets;
        }
        public dynamic GetNextMaDotDaoTao(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_Dot_DaoTao.Select(x => x.SoPhieu), DefineData.DotDaoTao));
        public dynamic GetListQuyTrinhDotDaoTao(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                IQueryable<HRM_Dot_DaoTao> query = db.HRM_Dot_DaoTao;
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(itemTimKiem.KeyWord));

                List<HRM_Dot_DaoTao> itemList = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;
                var listIdKeHoach = itemList.Select(r => r.IdKeHoachTongHop).ToList();
                var listKeHoach = db.HRM_KeHoachDaoTaoTongHop.Where(r => listIdKeHoach.Contains(r.IdGuid)).ToList();
                var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in itemList)
                {
                    item.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTao);
                    var BoPhan = BoPhans.Find(r => r.Id == item.IdCongTyEOS);
                    if (BoPhan != null)
                        item.TenCongTyEOS = BoPhan.TenBoPhan;
                    item.KeHoachDaoTaoTongHop = listKeHoach.Where(r => r.IdGuid == item.IdKeHoachTongHop).FirstOrDefault();
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
        public dynamic SetQuyTrinhDotDaoTao(HRM_Dot_DaoTao model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                try
                {
                    HRM_Dot_DaoTao entityDB = db.HRM_Dot_DaoTao.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, entityDB, model, err))
                        return err;

                    string check = FuncQuyTrinhDotDaoTao(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);
                    }

                    if (!SetSauHamUpdate(db, entityDB, model, err))
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
        public dynamic ChuyenTiepQuyTrinhDotDaoTao(HRM_Dot_DaoTao QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
                    HRM_Dot_DaoTao pcuDatabase = db.HRM_Dot_DaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTyEOS))
                        return err;
                    string Error = FuncQuyTrinhDotDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    if (QuyTrinh.isKetThuc == true)
                    {

                        foreach (var item in QuyTrinh.listChiTiet)
                        {
                            foreach (var itemNV in item.listNhanVien)
                            {
                                itemNV.isHienThi = true;
                            }
                            NotificationDaoTao(item, db);
                        }
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
        public dynamic KhongDuyetQuyTrinhDotDaoTao(HRM_Dot_DaoTao QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_Dot_DaoTao pcuDatabase = db.HRM_Dot_DaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;
                    string Error = FuncQuyTrinhDotDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
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
        public dynamic GetByIdQuyTrinhDotDaoTao(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_Dot_DaoTao model = db.HRM_Dot_DaoTao.Find(Id);
            if (model == null) return null;
            model.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTao);
            var listChiTiet = db.HRM_Dot_DaoTaoChiTiet.AsNoTracking().Where(r => r.IdDotDaoTao == Id).ToList();
            model.listFileDinhKem = FileUploadController.GetListFileDinhKem(db, model.IdGuid, nameof(HRM_Dot_DaoTao));
            var chiTietIds = listChiTiet.Select(x => x.IdGuid).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var keHoachChiTietIds = listChiTiet.Select(x => x.IdKeHoachDaoTaoChiTiet).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var keHoachChiTietById = keHoachChiTietIds.Count > 0
                ? db.HRM_TongHop_KeHoachDaoTaoChiTiet.AsNoTracking().Where(x => keHoachChiTietIds.Contains(x.IdGuid)).ToDictionary(x => x.IdGuid)
                : new Dictionary<string, HRM_TongHop_KeHoachDaoTaoChiTiet>();
            var nhanVienByChiTiet = chiTietIds.Count > 0
                ? db.HRM_Dot_DaoTaoNhanVien.AsNoTracking().Where(r => chiTietIds.Contains(r.IdChiTietDotDaoTao)).ToLookup(r => r.IdChiTietDotDaoTao)
                : Enumerable.Empty<HRM_Dot_DaoTaoNhanVien>().ToLookup(r => r.IdChiTietDotDaoTao);
            var noiDungByChiTiet = chiTietIds.Count > 0
                ? db.HRM_Dot_DaoTao_NoiDung.AsNoTracking().Where(r => chiTietIds.Contains(r.IdChiTietDotDaoTao)).ToLookup(r => r.IdChiTietDotDaoTao)
                : Enumerable.Empty<HRM_Dot_DaoTao_NoiDung>().ToLookup(r => r.IdChiTietDotDaoTao);
            var fileByChiTiet = GetFileDinhKemByIds(db, "HRM_Dot_DaoTaoChiTiet", chiTietIds);
            foreach (var item in listChiTiet)
            {
                if (!string.IsNullOrEmpty(item.IddmNoiDungDaoTao))
                    item.listIdNoiDung = item.IddmNoiDungDaoTao.Split(',').ToList();
                item.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDau);
                item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                item.KeHoachTongHopDaoTaoChiTiet = !string.IsNullOrEmpty(item.IdKeHoachDaoTaoChiTiet) && keHoachChiTietById.TryGetValue(item.IdKeHoachDaoTaoChiTiet, out var khct) ? khct : null;
                item.listNhanVien = nhanVienByChiTiet[item.IdGuid].ToList();
                item.listNoiDungChung = noiDungByChiTiet[item.IdGuid].ToList();
                if (item.listNoiDungChung != null && item.listNoiDungChung.Count > 0)
                {
                    foreach (var itemNd in item.listNoiDungChung)
                        if (itemNd.NgayDaoTao != null)
                            itemNd.NgayDaoTaoUnix = dungchung.DateTimeToUnixTimeStamp(itemNd.NgayDaoTao);
                }
                item.listFileDinhKem = fileByChiTiet[item.IdGuid].ToList();
            }
            List<HRM_Dot_DaoTaoChiPhi> listData = db.HRM_Dot_DaoTaoChiPhi.Where(a => a.IdChiTietDotDaoTao == Id).OrderBy(a => a.Created).ToList();
            if (listData != null)
            {
                model.listChiPhi = listData.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_Dot_DaoTaoChiPhi>
                {
                    data = a,
                    children = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>()
                }).ToList();
                Parallel.ForEach(model.listChiPhi, item =>
                {
                    Dot_DaoTaoChiPhiToTree(listData, item);
                });
            }
            model.listChiTiet = listChiTiet;
            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            return model;
        }
        public dynamic GetListNhanVienDotDaoTao(string Id, ApplicationDbContext db)
        {

            var listNhanVien = db.HRM_Dot_DaoTaoNhanVien.Where(r => r.IdDotDaoTao == Id).ToList();
            List<string> listId = listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
            var listResult = db.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).ToList();
            var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var model in listResult)
            {

                model.TenCongTy = lstBoPhanAll.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                //model.TenChucVu = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                //model.TenChucDam = listChucVu.Where(q => q.Id == model.IdChucVuEOS).Select(q => q.TenChucVu).FirstOrDefault();
                model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);

                if (listChucVu != null && listChucVu.Count > 0)
                {
                    var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == model.IdChucVu);
                    if (boPhan_NhanVien != null)
                    {
                        model.TenChucVu = boPhan_NhanVien.TenChucVu;

                    }

                }
                if (listChucDanh != null && listChucDanh.Count > 0)
                {
                    var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == model.IdChucDanh);
                    if (boPhan_NhanVien != null)
                    {
                        model.TenChucDanh = boPhan_NhanVien.Ten;
                    }
                }
                if (model.IdGioiTinh == "KhongXacDinh")
                    model.GioiTinh = "Khác";
                else model.GioiTinh = model.IdGioiTinh;
            }
            return listResult;
        }
        public dynamic GetlistChuongTrinhDaoTaoDaDuyet(ApplicationDbContext db, int Nam = 0)
        {
            List<HRM_ChuongTrinhDaoTao> model = db.HRM_ChuongTrinhDaoTao.Where(a => a.Nam == Nam && a.isKetThuc == true).OrderBy(a => a.Created).ToList();

            return model;
        }
        public dynamic GetDanhSachChiPhiForDaoTao(int Nam, int Thang, ApplicationDbContext db)
        {

            List<HRM_Dot_DaoTao> listdata = (from a in db.HRM_Dot_DaoTaoChiTiet
                                             join b in db.HRM_Dot_DaoTao on a.IdDotDaoTao equals b.IdGuid
                                             where a.NgayBatDau.Year == Nam && a.NgayBatDau.Month == Thang && b.isKetThuc == true
                                             select b).ToList();
            var listId = listdata.Select(a => a.IdGuid).Distinct().ToList();

            var listresult = new List<HRM_Dot_DaoTaoChiPhiViewModel>();
            var listChiPhi = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>();
            List<HRM_Dot_DaoTaoChiPhi> listData = db.HRM_Dot_DaoTaoChiPhi.Where(a => listId.Contains(a.IdChiTietDotDaoTao)).OrderBy(a => a.Created).ToList();
            listChiPhi = listData.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_Dot_DaoTaoChiPhi>
            {
                data = a,
                children = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>()
            }).ToList();
            Parallel.ForEach(listChiPhi, item =>
            {
                Dot_DaoTaoChiPhiToTree(listData, item);
            });
            Dot_DaoTaoChiPhiTinhGia(listChiPhi);
            if (listData != null)
            {
                foreach (var item in listdata)
                {
                    HRM_Dot_DaoTaoChiPhiViewModel itemAdd = new HRM_Dot_DaoTaoChiPhiViewModel()
                    {
                        Noidung = item.DotDaoTao,
                        Thang = Thang,
                        Nam = Nam

                    };
                    itemAdd.listChiPhi = listChiPhi.Where(a => a.data.IdChiTietDotDaoTao == item.IdGuid).ToList();
                    listresult.Add(itemAdd);
                }

            }

            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu thành công ", listresult);
        }
        public dynamic DeleteQuyTrinhDotDaoTao(string Id, ApplicationDbContext db)
        {

            try
            {
                HRM_Dot_DaoTao model = db.HRM_Dot_DaoTao.Find(Id);
                var listChiTiet = db.HRM_Dot_DaoTaoChiTiet.Where(r => r.IdDotDaoTao == Id).ToList();
                var lstNhanVien = db.HRM_Dot_DaoTaoNhanVien.Where(r => r.IdDotDaoTao == Id).ToList();
                db.HRM_Dot_DaoTaoNhanVien.RemoveRange(lstNhanVien);
                db.HRM_Dot_DaoTaoChiTiet.RemoveRange(listChiTiet);
                db.HRM_Dot_DaoTao.Remove(model);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public string FuncQuyTrinhDotDaoTao(HRM_Dot_DaoTao model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.NgayTao = dungchung.UnixTimeStampToDateTime(model.NgayTaoUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdDuAn = model.IdCongTyEOS;
                    if (string.IsNullOrEmpty(model.eAction))
                        model.eAction = DefineData.eAction.QUYTRINHDOTDAOTAO.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_Dot_DaoTao.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var listChiTiet = db.HRM_Dot_DaoTaoChiTiet.Where(r => r.IdDotDaoTao == model.IdGuid).ToList();
                var lstNhanVien = db.HRM_Dot_DaoTaoNhanVien.Where(r => r.IdDotDaoTao == model.IdGuid).ToList();
                db.HRM_Dot_DaoTaoNhanVien.RemoveRange(lstNhanVien);
                var lstNoiDung = db.HRM_Dot_DaoTao_NoiDung.Where(r => r.IdDotDaoTao == model.IdGuid).ToList();
                db.HRM_Dot_DaoTao_NoiDung.RemoveRange(lstNoiDung);

                db.HRM_Dot_DaoTaoChiTiet.RemoveRange(listChiTiet);
                if (model.listChiTiet != null)
                {
                    foreach (var item in model.listChiTiet)
                    {
                        if (item.listIdNoiDung != null && item.listIdNoiDung.Count > 0)
                            item.IddmNoiDungDaoTao = string.Join(",", item.listIdNoiDung);
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);
                        item.IdDotDaoTao = model.IdGuid;
                        if (string.IsNullOrEmpty(item.IdGuid))
                        {

                            item.IdGuid = Guid.NewGuid().ToString();
                            db.HRM_Dot_DaoTaoChiTiet.Add(item);
                        }
                        else
                        {
                            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        foreach (var itemDK in item.listFileDinhKem)
                        {
                            SetFileDinhKem(db, itemDK, DefineData.eTable.HRM_Dot_DaoTaoChiTiet, item.IdGuid);
                        }


                        foreach (var itemCT in item.listNhanVien)
                        {
                            itemCT.IdDotDaoTao = model.IdGuid;
                            itemCT.IdChiTietDotDaoTao = item.IdGuid;
                            itemCT.IdGuid = Guid.NewGuid().ToString();
                            db.HRM_Dot_DaoTaoNhanVien.Add(itemCT);

                        }
                        foreach (var itemCT in item.listNoiDungChung)
                        {
                            itemCT.IdDotDaoTao = model.IdGuid;
                            if (itemCT.NgayDaoTaoUnix > 0)
                                itemCT.NgayDaoTao = dungchung.UnixTimeStampToDateTime(itemCT.NgayDaoTaoUnix);

                            itemCT.IdChiTietDotDaoTao = item.IdGuid;
                            itemCT.IdGuid = Guid.NewGuid().ToString();
                            db.HRM_Dot_DaoTao_NoiDung.Add(itemCT);
                        }

                    }
                }
                List<HRM_Dot_DaoTaoChiPhi> listData = db.HRM_Dot_DaoTaoChiPhi.Where(a => a.IdChiTietDotDaoTao == model.IdGuid).OrderBy(a => a.Created).ToList();
                db.HRM_Dot_DaoTaoChiPhi.RemoveRange(listData);
                if (model.listChiPhi != null)
                {

                    List<HRM_Dot_DaoTaoChiPhi> listAdd = new List<HRM_Dot_DaoTaoChiPhi>();
                    Dot_DaoTaoChiPhiToList(model.listChiPhi, listAdd);
                    Parallel.ForEach(listAdd, item =>
                    {
                        item.IdChiTietDotDaoTao = model.IdGuid;
                    });
                    db.HRM_Dot_DaoTaoChiPhi.AddRange(listAdd);
                }
                if (model.listFileDinhKem != null && model.listFileDinhKem.Count > 0)
                    FileUploadController.SetFileDinhKem(db, model.listFileDinhKem, nameof(HRM_Dot_DaoTao), model.IdGuid);
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return err.Value;

                db.SaveChanges();

                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }

        }

        public void Dot_DaoTaoChiPhiToList(List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listPhanKhu, List<HRM_Dot_DaoTaoChiPhi> ListAdd, HRM_Dot_DaoTaoChiPhi parent = null)
        {
            foreach (var item in listPhanKhu)
            {
                item.data.IdGuid = Guid.NewGuid().ToString();


                if (parent != null)
                {
                    item.data.IdParent = parent.IdGuid;

                }

                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                    Dot_DaoTaoChiPhiToList(item.children, ListAdd, item.data);
            }
        }
        public void Dot_DaoTaoChiPhiTinhGia(List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listPhanKhu)
        {
            foreach (var item in listPhanKhu)
            {
                if (item.children.Count() > 0)
                {
                    Dot_DaoTaoChiPhiTinhGia(item.children);
                    item.data.ThanhTien = item.children.Sum(a => a.data.ThanhTien);
                }
                else
                {
                    item.data.ThanhTien = (item.data.DonGia ?? 0) * (item.data.SoLuong ?? 0);
                }
            }
        }

        public void Dot_DaoTaoChiPhiToTree(List<HRM_Dot_DaoTaoChiPhi> ListAll, ReturnData<HRM_Dot_DaoTaoChiPhi> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_Dot_DaoTaoChiPhi>
            {
                data = x
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    Dot_DaoTaoChiPhiToTree(ListAll, item);
                }
            }

        }
        public void NotificationDaoTao(HRM_Dot_DaoTaoChiTiet item, ApplicationDbContext db)
        {
            //var noidungdaoTao = db.HRM_dmNoiDungDaoTao.Where(r => r.IdGuid == item.IddmNoiDungDaoTao).Select(r => r.Ten).FirstOrDefault();
            //var listNhanVien = item.listNhanVien.Select(r => r.IdNhanVien).ToList();
            //List<long> lstIdNhanVienEOS = db.HRM_NhanVien.Where(r => listNhanVien.Contains(r.IdGuid)).Select(r => r.IdNhanVienEOS).ToList();
            //List<clsNhanVien> listNhanSu = _CallWebService.GetDanhSachNhanSuTheoBoPhan(db, 0);
            //List<string> lstIdUserEOS = smdb.NhanViens.Where(r => lstIdNhanVienEOS.Contains(r.Id)).Select(r => r.UserPkid).ToList();
            AddNotificationNoUserModel notification = new AddNotificationNoUserModel();

            //notification.NguoiGui = db.IdUser;
            //notification.listNguoiNhan = lstIdUserEOS;

            //NotificationNoUserModel notificationModel = new NotificationNoUserModel
            //{
            //    TieuDe = "Bạn nhận được 1 lịch tham gia đào tạo từ ngày " + item.NgayBatDau.ToString("dd/MM/yyyy") + " tới ngày " + item.NgayKetThuc.ToString("dd/MM/yyyy"),
            //    IdQuyTrinh = "",
            //    LoaiThongBao = "LICHDAOTAO",
            //    NoiDung = "Bạn nhận được 1 lịch tham gia đào tạo " + noidungdaoTao + " từ ngày " + item.NgayBatDau.ToString("dd/MM/yyyy") + " tới ngày " + item.NgayKetThuc.ToString("dd/MM/yyyy"),

            //};
            //notification.Item = notificationModel;
            //AddNotification(notification);
        }
        public ErrorMessage ExportDot_DaoTaoChiPhi(ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/Dot_DaoTaoChiPhi.xlsx");
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
                err.SetLoiVang("ExportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }


        public ErrorMessage ExportBaoCaoDot_DaoTaoChiPhi(List<HRM_Dot_DaoTaoChiPhiViewModel> listData, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCao_Dot_DaoTaoChiPhi.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "BaoCao_Dot_DaoTaoChiPhiDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                if (listData != null && listData.Count > 0)
                {
                    var data = listData.FirstOrDefault();
                    sheet1.Cells[3, 3].Value = "KHÓA ĐÀO TẠO: tháng " + data.Thang + " năm " + data.Nam;
                    int nRow = 6;
                    int STT = 1;
                    int ncol = 1;
                    foreach (var item in listData)
                    {
                        ncol = 1;
                        sheet1.Cells[nRow, ncol].Value = dungchung.QuyDoiLaMa(STT);
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.Noidung;
                        sheet1.Cells[nRow, ncol].Style.Font.Bold = true;
                        STT++;
                        nRow++;
                        foreach (var itemChild in item.listChiPhi)
                        {

                            ncol = 1;
                            sheet1.Cells[nRow, ncol].Value = dungchung.QuyDoiLaMa(STT);
                            ncol++;
                            sheet1.Cells[nRow, ncol].Value = itemChild.data.KhoanChiPhi;
                            ncol++;
                            sheet1.Cells[nRow, ncol].Value = itemChild.data.DonViTinh;
                            ncol++;
                            sheet1.Cells[nRow, ncol].Value = itemChild.data.SoLuong;
                            ncol++;
                            sheet1.Cells[nRow, ncol].Value = itemChild.data.DonGia;
                            ncol++;
                            sheet1.Cells[nRow, ncol].Value = itemChild.data.ThanhTien;
                            ncol++;
                            sheet1.Cells[nRow, ncol].Value = itemChild.data.GhiChu;
                            sheet1.Cells[nRow, 1, nRow, ncol].Style.Font.Bold = true;
                            nRow++;
                            STT++;
                            if (itemChild.children != null && itemChild.children.Count > 0)
                            {
                                nRow = ExportBaoCaoDot_DaoTaoChiPhi_child(sheet1, itemChild.children, nRow, "");
                            }
                        }

                    }
                    nRow--;
                    dungchung.DrawTableExcel(sheet1, 6, nRow, 1, ncol);
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[6, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[6, ncol, nRow, ncol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[6, 5, nRow, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheet1.Cells[6, 4, nRow, 6].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("BaoCao_Dot_DaoTaoChiPhi.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }
        public class BaoCaoDot_DaoTaoChiPhi
        {
            public List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listData { get; set; }
            public int STT { get; set; }
            public int nRow { get; set; }
        }
        public int ExportBaoCaoDot_DaoTaoChiPhi_child(ExcelWorksheet sheet1, List<ReturnData<HRM_Dot_DaoTaoChiPhi>> listData, int nRow, string STT)
        {
            int stt = 1;
            foreach (var item in listData)
            {
                string stt_row = "";
                if (!string.IsNullOrEmpty(STT))
                    stt_row = STT + "." + stt;
                else
                    stt_row = stt.ToString();
                int ncol = 1;
                sheet1.Cells[nRow, ncol].Value = stt_row;
                ncol++;
                sheet1.Cells[nRow, ncol].Value = item.data.KhoanChiPhi;
                ncol++;
                sheet1.Cells[nRow, ncol].Value = item.data.DonViTinh;
                ncol++;
                sheet1.Cells[nRow, ncol].Value = item.data.SoLuong;
                ncol++;
                sheet1.Cells[nRow, ncol].Value = item.data.DonGia;
                ncol++;
                sheet1.Cells[nRow, ncol].Value = item.data.ThanhTien;
                ncol++;
                sheet1.Cells[nRow, ncol].Value = item.data.GhiChu;
                if (item.children != null && item.children.Count > 0)
                {
                    sheet1.Cells[nRow, 1, nRow, ncol].Style.Font.Bold = true;
                    nRow++;
                    nRow = ExportBaoCaoDot_DaoTaoChiPhi_child(sheet1, item.children, nRow, stt_row);
                }
                else nRow++;
                stt++;

            }
            return nRow;
        }
        public ErrorMessage ImportDot_DaoTaoChiPhi(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = PathUploads + FileName;
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                string[] ColumnNames = new string[] { "STT", "STT Cha", "KHOẢN CHI PHÍ", "ĐƠN VỊ TÍNH", "SỐ LƯỢNG ", "ĐƠN GIÁ", "THÀNH TIỀN", "GHI CHÚ" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "Thông tin Hưởng chế độ"))
                {
                    return err;
                }
                var listdaTa = _dbContext.HRM_Dot_DaoTaoChiPhi.AsNoTracking().ToList();
                var listNhanVien = _dbContext.HRM_NhanVien.ToList();
                List<HRM_Dot_DaoTaoChiPhi> listAdd = new List<HRM_Dot_DaoTaoChiPhi>();
                List<string> listMa = new List<string>();
                DateTime dt = DateTime.Now;
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_Dot_DaoTaoChiPhi thuvien = new HRM_Dot_DaoTaoChiPhi();
                    int ncol = 1;
                    thuvien.SapXep = row;
                    thuvien.STT = dungchung.GetCell(sheet1, row, ncol);
                    if (string.IsNullOrEmpty(thuvien.STT))
                        break;
                    thuvien.IdGuid = Guid.NewGuid().ToString();
                    ncol++;
                    thuvien.STT_Cha = dungchung.GetCell(sheet1, row, ncol);
                    ncol++;
                    thuvien.KhoanChiPhi = dungchung.GetCell(sheet1, row, ncol);
                    ncol++;
                    thuvien.DonViTinh = dungchung.GetCell(sheet1, row, ncol);
                    ncol++;
                    thuvien.SoLuong = dungchung.GetCellToInt(sheet1, row, ncol);
                    ncol++;
                    thuvien.DonGia = dungchung.GetCellToDouble(sheet1, row, ncol);
                    ncol++;

                    ncol++;
                    thuvien.GhiChu = dungchung.GetCell(sheet1, row, ncol);

                    listAdd.Add(thuvien);
                }

                foreach (var thuvien in listAdd)
                {
                    var parent = listAdd.FirstOrDefault(a => a.STT == a.STT_Cha);
                    if (parent != null)
                        thuvien.IdParent = parent.IdGuid;
                }
                var parents = listAdd.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_Dot_DaoTaoChiPhi>
                {
                    data = a,
                    children = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>()
                });
                foreach (var thuvien in parents)
                    Dot_DaoTaoChiPhiToTree(listAdd, thuvien);
                s.Close();

                err.Data = parents;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }
        #endregion

        #region chương trình đào tạo new
        public dynamic GetNextMaChuongTrinhDaoTao(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_ChuongTrinhDaoTao.Select(x => x.SoQuyTrinh), DefineData.HRM_ChuongTrinhDaoTao));
        public dynamic GetListChuongTrinhDaoTao(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_ChuongTrinhDaoTao> result;
                if (TimKiem.TabTrangThai != 3)
                    result = from a in db.HRM_ChuongTrinhDaoTao
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_ChuongTrinhDaoTao
                             orderby a.Created descending
                             select a;
                var itemList = result.Distinct().ToList();
                int nSum = itemList.Count;
                int nTotalPage = CalculateTotalPages(nSum, PageSize);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                var listIdKeHoach = itemList.Select(r => r.IdKeHoachDaoTao).ToList();
                var listKeHoach = db.HRM_KeHoachDaoTao.Where(r => listIdKeHoach.Contains(r.IdGuid)).ToList();
                var CongTys = _CallWebService.GetDanhSachCongTy(db);
                foreach (var item in itemList)
                {
                    var CongTy = CongTys.Find(r => r.Id == item.IdCongTy);
                    if (CongTy != null)
                        item.TenCongTy = CongTy.TenBoPhan;
                    item.TenKeHoach = listKeHoach.Where(r => r.IdGuid == item.IdKeHoachDaoTao).Select(a => a.NoiDung).FirstOrDefault();
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
        public dynamic GetlistKeHoachDaoTaoDaDuyet(ApplicationDbContext db, int Nam = 0)
        {
            List<HRM_KeHoachDaoTao> model = db.HRM_KeHoachDaoTao.Where(a => a.Nam == Nam && a.isKetThuc == true).OrderBy(a => a.Created).ToList();

            return model;
        }
        public dynamic SetChuongTrinhDaoTao(HRM_ChuongTrinhDaoTao model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                try
                {
                    HRM_ChuongTrinhDaoTao entityDB = db.HRM_ChuongTrinhDaoTao.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, entityDB, model, err))
                        return err;

                    string check = FuncChuongTrinhDaoTao(model, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);
                    }

                    if (!SetSauHamUpdate(db, entityDB, model, err))
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
        public dynamic ChuyenTiepChuongTrinhDaoTao(HRM_ChuongTrinhDaoTao QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
                    HRM_ChuongTrinhDaoTao pcuDatabase = db.HRM_ChuongTrinhDaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    string Error = FuncChuongTrinhDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    if (QuyTrinh.isKetThuc == true && QuyTrinh.ListNhanVien != null && QuyTrinh.ListNhanVien.Count > 0)
                    {
                        foreach (var item in QuyTrinh.ListNhanVien)
                            LogLichSuNhanVien(db, item.IdNhanVien, QuyTrinh.IdGuid, "HRM_ChuongTrinhDaoTao", DefineData.eAction.CHUONGTRINHDAOTAO.ToString(), QuyTrinh.TuNgayUnix);
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
        public dynamic KhongDuyetChuongTrinhDaoTao(HRM_ChuongTrinhDaoTao QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_ChuongTrinhDaoTao pcuDatabase = db.HRM_ChuongTrinhDaoTao.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;
                    string Error = FuncChuongTrinhDaoTao(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error, QuyTrinh.IdGuid);
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
        public dynamic GetByIdChuongTrinhDaoTao(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_ChuongTrinhDaoTao model = db.HRM_ChuongTrinhDaoTao.Find(Id);
            if (model == null) return null;
            model.listFileDinhKem = FileUploadController.GetListFileDinhKem(db, model.IdGuid, "HRM_ChuongTrinhDaoTao");
            model.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(model.TuNgay);
            model.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(model.DenNgay);
            model.ListNoiDung = db.HRM_ChuongTrinhDaoTao_NoiDung.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            var listKeHoach = db.HRM_KeHoachDaoTao.Where(r => r.IdGuid == model.IdGuid).ToList();
            model.isDaoTaoMoi = model.isDaoTaoMoi;
            var ListNoiDung_ThoiGian = db.HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian.Where(a => a.IdQuyTrinh == Id).ToList();
            var listNoiDung = db.HRM_dmNhomCauHoi.ToList();
            if (ListNoiDung_ThoiGian != null && ListNoiDung_ThoiGian.Count > 0)
            {

                Parallel.ForEach(ListNoiDung_ThoiGian, item =>
                {
                    item.NgayUnix = dungchung.DateTimeToUnixTimeStamp(item.Ngay);
                    item.TuGioUnix = dungchung.DateTimeToUnixTimeStamp(item.TuGio);
                    item.DenGioUnix = dungchung.DateTimeToUnixTimeStamp(item.DenGio);
                });
            }
            foreach (var item in model.ListNoiDung)
            {
                if (ListNoiDung_ThoiGian != null && ListNoiDung_ThoiGian.Count > 0)
                    item.listThoiGianDaoTao = ListNoiDung_ThoiGian.Where(a => a.IddmNoiDung == item.IddmNoiDung).OrderBy(a => a.Ngay).ToList();
                var NoiDung = listNoiDung.FirstOrDefault(a => a.IdGuid == item.IddmNoiDung);
                if (NoiDung != null)
                    item.TenNoiDung = NoiDung.Ten;
                item.TenHinhThuc = DefineData.QuyDoiHinhThucDaoTao(item.HinhThucDaoTao);
            }

            model.ListNhanVien = db.HRM_ChuongTrinhDaoTao_NhanVien.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
            var ListNhanVien_ThoiGian = db.HRM_ChuongTrinhDaoTao_NhanVien_NoiDung.Where(a => a.IdQuyTrinh == Id).ToList();
            if (ListNhanVien_ThoiGian != null && ListNhanVien_ThoiGian.Count > 0)
            {
                foreach (var item in model.ListNhanVien)
                {
                    item.listDanhGia = ListNhanVien_ThoiGian.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Created).ToList();
                    foreach (var DG in item.listDanhGia)
                    {
                        var NoiDung = listNoiDung.FirstOrDefault(a => a.IdGuid == DG.IddmNoiDung);
                        if (NoiDung != null)
                            DG.TenNoiDung = NoiDung.Ten;
                        DG.TenHinhThuc = DefineData.QuyDoiHinhThucDaoTao(DG.HinhThucDaoTao);
                    }
                }
            }
            List<HRM_Dot_DaoTaoChiPhi> listData = db.HRM_Dot_DaoTaoChiPhi.Where(a => a.IdChiTietDotDaoTao == Id).OrderBy(a => a.Created).ToList();
            if (listData != null)
            {
                model.listChiPhi = listData.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_Dot_DaoTaoChiPhi>
                {
                    data = a,
                    children = new List<ReturnData<HRM_Dot_DaoTaoChiPhi>>()
                }).ToList();
                Parallel.ForEach(model.listChiPhi, item =>
                {
                    Dot_DaoTaoChiPhiToTree(listData, item);
                });
            }

            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            return model;
        }
        public dynamic GetDanhSachNoiDung_NhanVien(string Id, ApplicationDbContext db)
        {
            HRM_ChuongTrinhDaoTao entity = new HRM_ChuongTrinhDaoTao();
            entity.ListNoiDung = new List<HRM_ChuongTrinhDaoTao_NoiDung>();
            entity.ListNhanVien = new List<HRM_ChuongTrinhDaoTao_NhanVien>();
            var listChiTiet = db.HRM_KeHoachDaoTaoChiTiet.Where(a => a.IdKeHoach == Id).ToList();
            var listNoiDung = db.HRM_dmNhomCauHoi.ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            List<string> listId = listChiTiet.Select(a => a.IdGuid).Distinct().ToList();
            var listNhanVien = db.HRM_KeHoachDaoTaoNhanVien.Where(a => listId.Contains(a.IdChiTietKeHoach)).GroupBy(a => a.IdNhanVien).Select(a => a.FirstOrDefault()).ToList();
            foreach (var item in listChiTiet)
            {
                List<string> listidNoiDung = item.IddmNoiDungDaoTao.Split(',').ToList();
                foreach (string IdNoiDung in listidNoiDung)
                {
                    var itemAdd = new HRM_ChuongTrinhDaoTao_NoiDung();
                    itemAdd.IddmNoiDung = IdNoiDung;
                    itemAdd.HinhThucDaoTao = item.IddmHinhThucDaoTao;
                    itemAdd.IdPhuongThucDaoTao = item.IdPhuongThucDaoTao;
                    var NoiDung = listNoiDung.FirstOrDefault(a => a.IdGuid == itemAdd.IddmNoiDung);
                    if (NoiDung != null)
                        itemAdd.TenNoiDung = NoiDung.Ten;
                    itemAdd.TenHinhThuc = DefineData.QuyDoiHinhThucDaoTao(itemAdd.HinhThucDaoTao);
                    entity.ListNoiDung.Add(itemAdd);
                }
            }
            entity.ListNoiDung = entity.ListNoiDung.Distinct().ToList();
            foreach (var Nv in listNhanVien)
            {
                HRM_ChuongTrinhDaoTao_NhanVien itemAdd = new HRM_ChuongTrinhDaoTao_NhanVien();
                itemAdd.IdNhanVien = Nv.IdNhanVien;
                itemAdd.TenNhanVien = Nv.TenNhanVien;
                itemAdd.MaNhanVien = Nv.MaNhanVien;
                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == Nv.IdBoPhanEOS);
                if (BoPhan != null)
                    itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                itemAdd.listDanhGia = entity.ListNoiDung.Select(a => new HRM_ChuongTrinhDaoTao_NhanVien_NoiDung
                {
                    IddmNoiDung = a.IddmNoiDung,
                    HinhThucDaoTao = a.HinhThucDaoTao,
                    TenNoiDung = a.TenNoiDung,
                    TenHinhThuc = a.TenHinhThuc
                }).ToList();
                entity.ListNhanVien.Add(itemAdd);
            }
            return entity;
        }
        public dynamic DeleteChuongTrinhDaoTao(string Id, ApplicationDbContext db)
        {
            try
            {
                HRM_ChuongTrinhDaoTao model = db.HRM_ChuongTrinhDaoTao.Find(Id);
                var ListNoiDung = db.HRM_ChuongTrinhDaoTao_NoiDung.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                var ListNoiDung_ThoiGian = db.HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian.Where(a => a.IdQuyTrinh == Id).ToList();
                var ListNhanVien = db.HRM_ChuongTrinhDaoTao_NhanVien.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.SapXep).ToList();
                var ListNhanVien_ThoiGian = db.HRM_ChuongTrinhDaoTao_NhanVien_NoiDung.Where(a => a.IdQuyTrinh == Id).ToList();
                db.HRM_ChuongTrinhDaoTao_NoiDung.RemoveRange(ListNoiDung);
                db.HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian.RemoveRange(ListNoiDung_ThoiGian);
                db.HRM_ChuongTrinhDaoTao_NhanVien.RemoveRange(ListNhanVien);
                db.HRM_ChuongTrinhDaoTao_NhanVien_NoiDung.RemoveRange(ListNhanVien_ThoiGian);
                db.HRM_ChuongTrinhDaoTao.Remove(model);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!", model.IdGuid);
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public dynamic GetDanhSachHinhThucDaoTao()
        {
            List<HRM_DanhMuc> list = new List<HRM_DanhMuc>();
            for (int i = 1; i <= 4; i++)
                list.Add(new HRM_DanhMuc
                {
                    Id = i,
                    Ten = DefineData.QuyDoiHinhThucDaoTao(i)
                });
            return list;
        }
        public string FuncChuongTrinhDaoTao(HRM_ChuongTrinhDaoTao model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.TuNgay = dungchung.UnixTimeStampToDateTime(model.TuNgayUnix);
                model.DenNgay = dungchung.UnixTimeStampToDateTime(model.DenNgayUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdDuAn = model.IdCongTy; 
                    if (string.IsNullOrEmpty(model.eAction))
                        model.eAction = DefineData.eAction.CHUONGTRINHDAOTAO.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return err.Value;
                    db.HRM_ChuongTrinhDaoTao.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var ListNoiDung = db.HRM_ChuongTrinhDaoTao_NoiDung.Where(a => a.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                var ListNoiDung_ThoiGian = db.HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                var ListNhanVien = db.HRM_ChuongTrinhDaoTao_NhanVien.Where(a => a.IdQuyTrinh == model.IdGuid).OrderBy(a => a.SapXep).ToList();
                var ListNhanVien_ThoiGian = db.HRM_ChuongTrinhDaoTao_NhanVien_NoiDung.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                db.HRM_ChuongTrinhDaoTao_NoiDung.RemoveRange(ListNoiDung);
                db.HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian.RemoveRange(ListNoiDung_ThoiGian);
                db.HRM_ChuongTrinhDaoTao_NhanVien.RemoveRange(ListNhanVien);
                db.HRM_ChuongTrinhDaoTao_NhanVien_NoiDung.RemoveRange(ListNhanVien_ThoiGian);
                if (model.ListNoiDung != null && model.ListNoiDung.Count > 0)
                {
                    int SapXep = 1;
                    foreach (var item in model.ListNoiDung)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = SapXep;
                        if (item.listThoiGianDaoTao != null && item.listThoiGianDaoTao.Count > 0)
                        {
                            foreach (var child in item.listThoiGianDaoTao)
                            {
                                child.IdGuid = Guid.NewGuid().ToString();
                                child.IdQuyTrinh = model.IdGuid;
                                child.IddmNoiDung = item.IddmNoiDung;
                                child.Ngay = dungchung.UnixTimeStampToDateTime(child.NgayUnix);
                                child.TuGio = dungchung.UnixTimeStampToDateTime(child.TuGioUnix);
                                child.DenGio = dungchung.UnixTimeStampToDateTime(child.DenGioUnix);
                            }
                            db.HRM_ChuongTrinhDaoTao_NoiDung_ThoiGian.AddRange(item.listThoiGianDaoTao);
                        }

                        SapXep++;
                    }
                    db.HRM_ChuongTrinhDaoTao_NoiDung.AddRange(model.ListNoiDung);
                }
                if (model.ListNhanVien != null && model.ListNhanVien.Count > 0)
                {
                    int SapXep = 1;
                    foreach (var item in model.ListNhanVien)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.SapXep = SapXep;
                        if (item.listDanhGia != null && item.listDanhGia.Count > 0)
                        {
                            foreach (var child in item.listDanhGia)
                            {
                                child.IdGuid = Guid.NewGuid().ToString();
                                child.IdQuyTrinh = model.IdGuid;
                                child.IdNhanVien = item.IdNhanVien;

                            }
                            db.HRM_ChuongTrinhDaoTao_NhanVien_NoiDung.AddRange(item.listDanhGia);
                        }

                        SapXep++;
                    }
                    db.HRM_ChuongTrinhDaoTao_NhanVien.AddRange(model.ListNhanVien);
                }

                List<HRM_Dot_DaoTaoChiPhi> listData = db.HRM_Dot_DaoTaoChiPhi.Where(a => a.IdChiTietDotDaoTao == model.IdGuid).OrderBy(a => a.Created).ToList();
                db.HRM_Dot_DaoTaoChiPhi.RemoveRange(listData);
                if (model.listChiPhi != null)
                {

                    List<HRM_Dot_DaoTaoChiPhi> listAdd = new List<HRM_Dot_DaoTaoChiPhi>();
                    Dot_DaoTaoChiPhiToList(model.listChiPhi, listAdd);
                    Parallel.ForEach(listAdd, item =>
                    {
                        item.IdChiTietDotDaoTao = model.IdGuid;
                    });
                    db.HRM_Dot_DaoTaoChiPhi.AddRange(listAdd);
                }

                if (model.listFileDinhKem != null && model.listFileDinhKem.Count > 0)
                    FileUploadController.SetFileDinhKem(db, model.listFileDinhKem, "HRM_ChuongTrinhDaoTao", model.IdGuid);
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))

                    return err.Value;

                db.SaveChanges();

                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }

        }
        public ErrorMessage ExportBaoCaoChuongTrinhDaoTao(HRM_ChuongTrinhDaoTao model, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ChuongTrinhDaoTao.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ChuongTrinhDaoTaoDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                model.TuNgay = dungchung.UnixTimeStampToDateTime(model.TuNgayUnix);
                model.DenNgay = dungchung.UnixTimeStampToDateTime(model.DenNgayUnix);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                sheet1.Cells[2, 3].Value = "Đơn vị: " + _CallWebService.GetDanhSachCongTy(_dbContext).FirstOrDefault(a => a.Id == model.IdCongTy).TenBoPhan;
                sheet1.Cells[3, 3].Value = "Thời gian: từ Ngày " + model.TuNgay.ToString("dd/MM/yyyy") + " - " + model.DenNgay.ToString("dd/MM/yyyy");
                //foreach (var item in model.ListNhanVien)
                //{
                //    ExcelWorksheet sheetNV = package.Workbook.Worksheets.Add(item.MaNhanVien + item.TenNhanVien, sheet1);
                //sheetNV.Cells[4, 1].Value = "Họ và Tên: " + item.TenNhanVien;
                int Stt = 1;
                int nRow = 6;
                int nCol = 1;
                foreach (var Nd in model.ListNoiDung)
                {
                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = Stt;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = Nd.TenNoiDung;
                    nCol++;
                    var cellNd = sheet1.Cells[nRow, nCol];
                    if (Nd.listThoiGianDaoTao != null && Nd.listThoiGianDaoTao.Count > 0)
                    {
                        foreach (var time in Nd.listThoiGianDaoTao)
                        {
                            time.TuGio = dungchung.UnixTimeStampToDateTime(time.TuGioUnix);
                            time.DenGio = dungchung.UnixTimeStampToDateTime(time.DenGioUnix);
                            time.Ngay = dungchung.UnixTimeStampToDateTime(time.NgayUnix);
                            var r1 = cellNd.RichText.Add(time.Ngay.Value.ToString("dd/mm/yyyy") + " " + time.TuGio.Value.ToString("hh/mm") + "-" + time.DenGio.Value.ToString("hh/mm") + " \r\n");
                            r1.Size = 12;
                            r1.FontName = "Times New Roman";
                        }
                        cellNd.Style.WrapText = true;
                    }

                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = Nd.DiaDiemDaoTao;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = Nd.TenHinhThuc;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = Nd.NguoiDaoTao;
                    nRow++;
                    Stt++;

                }
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[6, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[6, 4, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                nRow--;
                dungchung.DrawTableExcel(sheet1, 5, nRow, 1, 6);
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("KeHoachDaoTao.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage ExportBaoCaoChuongTrinhDaoTaoNhanVien(HRM_ChuongTrinhDaoTao model, ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ChuongTrinhDaoTaoDanhGia.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ChuongTrinhDaoTaoDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                model.TuNgay = dungchung.UnixTimeStampToDateTime(model.TuNgayUnix);
                model.DenNgay = dungchung.UnixTimeStampToDateTime(model.DenNgayUnix);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Mẫu"];
                sheet1.Cells[2, 3].Value = "Đơn vị: " + _CallWebService.GetDanhSachCongTy(_dbContext).FirstOrDefault(a => a.Id == model.IdCongTy).TenBoPhan;
                sheet1.Cells[3, 3].Value = "Thời gian: từ Ngày " + model.TuNgay.ToString("dd/MM/yyyy") + " - " + model.DenNgay.ToString("dd/MM/yyyy");
                foreach (var item in model.ListNhanVien)
                {
                    ExcelWorksheet sheetNV = package.Workbook.Worksheets.Add(item.MaNhanVien + item.TenNhanVien, sheet1);
                    sheetNV.Cells[4, 1].Value = "Họ và Tên: " + item.TenNhanVien;
                    sheetNV.Cells[4, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                    int Stt = 1;
                    int nRow = 7;
                    int nCol = 1;
                    foreach (var Nd in item.listDanhGia)
                    {

                        nCol = 1;
                        sheetNV.Cells[nRow, nCol].Value = Stt;
                        nCol++;
                        sheetNV.Cells[nRow, nCol].Value = Nd.TenNoiDung;
                        nCol++;
                        sheetNV.Cells[nRow, nCol].Value = Nd.TenHinhThuc;
                        nCol++;
                        if (Nd.HinhThucDaoTao != 2)
                            sheetNV.Cells[nRow, nCol].Value = Nd.LyThuyet == 1 ? "Đạt" : "Không đạt";
                        nCol++;
                        if (Nd.HinhThucDaoTao != 1)
                            sheetNV.Cells[nRow, nCol].Value = Nd.ThucHanh == 1 ? "Đạt" : "Không đạt";
                        nRow++;
                        Stt++;
                    }
                    sheetNV.Cells.Style.Font.Name = "Times New Roman";
                    sheetNV.Cells.Style.Font.Size = 12;
                    sheetNV.Cells.Style.WrapText = true;
                    sheetNV.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheetNV.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetNV.Cells[7, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    //sheet1.Cells[5, , nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    nRow--;
                    dungchung.DrawTableExcel(sheetNV, 6, nRow, 1, 5);

                }
                package.Workbook.Worksheets.Delete(sheet1);
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("PhieuTheoDoiDaoTao.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }

        public class DaoTaoTongHopNhanVien
        {
            public List<string> listChuongTrinh { get; set; }
            public List<DaoTaoTongHopNhanVien_NhanVien> listNhanVien { get; set; }
        }
        public class DaoTaoTongHopNhanVien_NhanVien
        {
            public string MaSoThue { get; set; }
            public string MaNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public string TenChucVu { get; set; }
            public DateTime? NgayVao { get; set; }
            public List<int> listChuongTrinh { get; set; }
        }

        public ErrorMessage GetListNhanVienDaoTao(ApplicationDbContext _dbContext, dynamic dyn)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
                DaoTaoTongHopNhanVien result = new DaoTaoTongHopNhanVien();
                result.listNhanVien = new List<DaoTaoTongHopNhanVien_NhanVien>();
                var Listdata = from a in _dbContext.HRM_ChuongTrinhDaoTao
                               where a.isKetThuc == true
                               select a;
                if (TimKiem.Nam > 0)
                    Listdata = Listdata.Where(a => a.Nam == TimKiem.Nam);
                if (TimKiem.IdCongTy > 0)
                    Listdata = Listdata.Where(a => a.IdCongTy == TimKiem.IdCongTy);
                if (TimKiem.TuNgayUnix > 0)
                {
                    DateTime time = UnixTimeStampToDateTime(TimKiem.TuNgayUnix) ?? DateTime.Now;
                    Listdata = Listdata.Where(a => (a.TuNgay != null && a.TuNgay <= time) || (a.DenNgay != null && a.DenNgay >= time));
                }
                if (TimKiem.DenNgayUnix > 0)
                {
                    DateTime time = UnixTimeStampToDateTime(TimKiem.DenNgayUnix) ?? DateTime.Now;
                    Listdata = Listdata.Where(a => (a.TuNgay != null && a.TuNgay <= time) || (a.DenNgay != null && a.DenNgay <= time));
                }
                var ItemList = Listdata.OrderByDescending(a => a.Nam).ToList();
                if (ItemList.Count > 0)
                {
                    List<string> listId = ItemList.Select(a => a.IdGuid).ToList();
                    result.listChuongTrinh = ItemList.Select(a => a.NoiDung).ToList();
                    var listNV_QT = _dbContext.HRM_ChuongTrinhDaoTao_NhanVien.Where(a => listId.Contains(a.IdQuyTrinh)).ToList();

                    if (listNV_QT.Count > 0)
                    {
                        var listIdNV = listNV_QT.Select(a => a.IdNhanVien).ToList();
                        var listNhanVien = _dbContext.HRM_NhanVien.Where(a => listIdNV.Contains(a.IdGuid)).ToList();
                        if (TimKiem.IdBoPhan > 0)
                            listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhan).ToList(); ;
                        var listIdUngVien = new List<string>();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdUngVien = listNhanVien.Where(a => !string.IsNullOrEmpty(a.IdUngVien)).Select(a => a.IdUngVien).Distinct().ToList();
                        var listNhanVienMoi = _dbContext.HRM_TuyenDung_UngVien.Where(a => listIdNV.Contains(a.IdGuid) && (listIdUngVien.Count == 0 || !listIdUngVien.Contains(a.IdGuid))).ToList();
                        var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                        foreach (var item in listNhanVien)
                        {
                            DaoTaoTongHopNhanVien_NhanVien NVAdd = new DaoTaoTongHopNhanVien_NhanVien();
                            NVAdd.MaSoThue = item.MaSoThueTNCN;
                            NVAdd.MaNhanVien = item.MaNhanVien;
                            NVAdd.TenNhanVien = item.TenDayDu;
                            NVAdd.TenChucVu = listChucVu.Where(a => a.Id == item.IdChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                            NVAdd.NgayVao = item.NgayBatDauLamViec;
                            NVAdd.listChuongTrinh = new List<int>();
                            foreach (var CT in ItemList)
                            {
                                int count = listNV_QT.Where(a => (a.IdNhanVien == item.IdGuid || (!string.IsNullOrEmpty(item.IdUngVien) && a.IdNhanVien == item.IdUngVien)) && a.IdQuyTrinh == CT.IdGuid).Count();
                                if (count > 0)
                                    NVAdd.listChuongTrinh.Add(1);
                                else
                                    NVAdd.listChuongTrinh.Add(0);
                            }
                            result.listNhanVien.Add(NVAdd);
                        }
                        foreach (var item in listNhanVienMoi)
                        {
                            DaoTaoTongHopNhanVien_NhanVien NVAdd = new DaoTaoTongHopNhanVien_NhanVien();
                            NVAdd.MaSoThue = "";
                            NVAdd.MaNhanVien = item.MaNhanVien;
                            NVAdd.TenNhanVien = item.HoTenDayDu;
                            NVAdd.TenChucVu = listChucVu.Where(a => a.Id == item.IdChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                            NVAdd.NgayVao = item.NgayNhanViecDuKien;
                            NVAdd.listChuongTrinh = new List<int>();
                            foreach (var CT in ItemList)
                            {
                                int count = listNV_QT.Where(a => a.IdNhanVien == item.IdGuid).Count();
                                if (count > 0)
                                    NVAdd.listChuongTrinh.Add(1);
                                else
                                    NVAdd.listChuongTrinh.Add(0);
                            }
                            result.listNhanVien.Add(NVAdd);
                        }

                    }
                }

                err.Data = result;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }
        public ErrorMessage ExportTongHopNhanVienDaoTao(ApplicationDbContext _dbContext, DaoTaoTongHopNhanVien entity)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoNhanVienTongHopDaoTao.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                if (entity.listChuongTrinh != null && entity.listChuongTrinh.Count > 0)
                {
                    sheet1.Cells[3, 1, 3, 6 + entity.listChuongTrinh.Count].Merge = true;
                    int nRow = 4;
                    int nCol = 7;
                    foreach (var Ct in entity.listChuongTrinh)
                    {
                        sheet1.Cells[nRow, nCol].Value = Ct;
                        nCol++;
                    }
                    nRow++;
                    nRow++;
                    int STT = 1;
                    foreach (var item in entity.listNhanVien)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaSoThue;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenChucVu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NgayVao == null ? "" : item.NgayVao.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        foreach (var Ct in item.listChuongTrinh)
                        {
                            if (Ct > 0)
                                sheet1.Cells[nRow, nCol].Value = 1;
                            nCol++;
                        }
                        STT++;
                        nRow++;
                    }
                    nRow--;
                    nCol--;
                    for (int i = 7; i <= nCol; i++)
                    {
                        sheet1.Cells[5, i].Formula = "=Sum(" + sheet1.Cells[6, i].Address + ":" + sheet1.Cells[nRow, i].Address + ")";
                    }
                    DrawTableExcel(sheet1, 4, nRow, 1, nCol);
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.Font.Size = 12;
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
                        sheet1.Cells[6, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                }

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("BaoCaoNhanVienTongHopDaoTao.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportDot_DaoTaoChiPhi " + e.ToString());
            }
            return err;
        }


        #endregion
        #region đợi thi
        public dynamic GetListDotThi(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 20;
            IQueryable<HRM_DotThi> result = from a in db.HRM_DotThi
                                             where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || ((a.NoiDung.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()))))
                                             orderby a.Ngay descending
                                             select a;

            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
            foreach (var item in listItem)
            {
                if (item.TrangThai == DefineData.eTrangThaiDotThi.ChuaThi.ToString())
                    item.TenTrangThai = "Chưa thi";
                else if (item.TrangThai == DefineData.eTrangThaiDotThi.DangThi.ToString())
                    item.TenTrangThai = "Đang thi";
                else if (item.TrangThai == DefineData.eTrangThaiDotThi.KetThuc.ToString())
                    item.TenTrangThai = "Kết thúc";


            }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic GetListDotThiDangThi(ApplicationDbContext db)
        {

            List<HRM_DotThi> result = (from a in db.HRM_DotThi
                                       where a.TrangThai == DefineData.eTrangThaiDotThi.DangThi.ToString()
                                       orderby a.Created
                                       select a).ToList();


            return result;
        }
        public dynamic SetDotThi(HRM_DotThi entity, ApplicationDbContext _dbContext)
        {
            var check = _dbContext.HRM_DotThi.FirstOrDefault(a => a.Ma == entity.Ma && a.IdGuid != entity.IdGuid);
            if (check != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Không được trùng mã đợt thi");
            }
            entity.Ngay = dungchung.UnixTimeStampToDateTime(entity.NgayUnix);

            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                entity.TrangThai = DefineData.eTrangThaiDotThi.ChuaThi.ToString();
                _dbContext.HRM_DotThi.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            var CauTLOld = _dbContext.HRM_BaiThi.Where(a => a.IdDotThi == entity.IdGuid).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                _dbContext.HRM_BaiThi.RemoveRange(CauTLOld);
            var NhanVienOld = _dbContext.HRM_DotThi_NhanVien.Where(a => a.IdDotThi == entity.IdGuid).ToList();
            if (NhanVienOld != null && NhanVienOld.Count > 0)
                _dbContext.HRM_DotThi_NhanVien.RemoveRange(NhanVienOld);

            var TLOld = _dbContext.HRM_BaiThi_Item.Where(a => a.IdDotThi == entity.IdGuid).ToList();
            if (TLOld != null && TLOld.Count > 0)
                _dbContext.HRM_BaiThi_Item.RemoveRange(TLOld);
            if (entity.listBaiThi != null && entity.listBaiThi.Count > 0)
            {
                int STT = 1;
                List<HRM_BaiThi_Item> listItemAdd = new List<HRM_BaiThi_Item>();
                foreach (var item in entity.listBaiThi)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdDotThi = entity.IdGuid;
                    item.ThoiGianLamBai = entity.ThoiGianLamBai;
                    item.STT = STT;
                    STT++;
                    if (item.listCauHoi != null)
                    {
                        foreach (var itemCH in item.listCauHoi)
                        {
                            itemCH.IdGuid = Guid.NewGuid().ToString();
                            itemCH.IdBaiThi = item.IdGuid;
                            itemCH.IdDotThi = entity.IdGuid;
                            itemCH.STT = STT;
                            STT++;
                        }
                        listItemAdd.AddRange(item.listCauHoi);
                    }
                }

                _dbContext.HRM_BaiThi_Item.AddRange(listItemAdd);
                _dbContext.HRM_BaiThi.AddRange(entity.listBaiThi);
            }
            if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
            {
                if (entity.listNhanVien.Where(a => a.isKetThuc == true).Count() == entity.listNhanVien.Count())
                    entity.TrangThai = DefineData.eTrangThaiDotThi.KetThuc.ToString();
                Parallel.ForEach(entity.listNhanVien, item =>
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdDotThi = entity.IdGuid;
                });
                _dbContext.HRM_DotThi_NhanVien.AddRange(entity.listNhanVien);
            }
            _CallWebService.AddModify(_dbContext, "Đợt thi", DefineData.eModify.Sua.ToString(), nameof(HRM_DotThi), "Đợt thi", entity.IdGuid, "", JsonConvert.SerializeObject(entity), 0, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);

            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }
        public dynamic SetBaiThi(HRM_BaiThi entity, ApplicationDbContext _dbContext, bool isChamDiem = false)
        {
            var CauTLOld = _dbContext.HRM_BaiThi_Item.Where(a => a.IdBaiThi == entity.IdGuid).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                _dbContext.HRM_BaiThi_Item.RemoveRange(CauTLOld);
            int STT = 1;
            if (entity.listCauHoi != null)
            {
                foreach (var itemCH in entity.listCauHoi)
                {
                    if (string.IsNullOrEmpty(itemCH.IdGuid))
                        itemCH.IdGuid = Guid.NewGuid().ToString();
                    itemCH.IdBaiThi = entity.IdGuid;
                    itemCH.IdDotThi = entity.IdDotThi;
                    if (itemCH.Loai == DefineData.eLoaiCauhoi.SapXep.ToString())
                        itemCH.CauTraLoi = string.Join(",", itemCH.listDapAn.Select(a => a.SapXepDapAn));
                    if (itemCH.Loai != DefineData.eLoaiCauhoi.NhapText.ToString() && itemCH.CauTraLoi == itemCH.DapAn)
                        itemCH.Diem = itemCH.DiemToiDa;
                    itemCH.STT = STT;
                    STT++;
                }
                entity.isKetThuc = true;
                if (isChamDiem == true)
                {
                    entity.Diem = entity.listCauHoi.Where(a => a.Loai == DefineData.eLoaiCauhoi.NhapText.ToString() || (a.Loai != DefineData.eLoaiCauhoi.NhapText.ToString() && a.DapAn == a.CauTraLoi)).Sum(a => a.Diem);
                    entity.isKetThuc = true;
                    var NhanVien = _dbContext.HRM_DotThi_NhanVien.FirstOrDefault(a => a.IdNhanVien == entity.IdNhanVien && a.IdDotThi == entity.IdDotThi);
                    NhanVien.Diem = entity.Diem;
                    NhanVien.isKetThuc = true;
                    _dbContext.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

            }
            _dbContext.HRM_BaiThi_Item.AddRange(entity.listCauHoi);
            _dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }
        public dynamic GetDotThi(string Id, ApplicationDbContext db)
        {
            HRM_DotThi DotThi = db.HRM_DotThi.FirstOrDefault(r => r.IdGuid == Id);
            DotThi.NgayUnix = dungchung.DateTimeToUnixTimeStamp(DotThi.Ngay);
            var BaiThi = db.HRM_BaiThi.Where(a => a.IdDotThi == Id).ToList();
            if (BaiThi != null && BaiThi.Count > 0)
                DotThi.listBaiThi = BaiThi.Where(a => a.IdDotThi == Id).OrderBy(a => a.STT).ToList();
            var CauTL = db.HRM_BaiThi_Item.Where(a => a.IdDotThi == Id).OrderBy(a => a.STT).ToList();
            List<string> listIdCauHoi = CauTL.Select(a => a.IddmCauHoi).Distinct().ToList();
            var listCauHoi = db.HRM_dmCauHoi.Where(a => listIdCauHoi.Contains(a.IdGuid)).ToList();
            Parallel.ForEach(CauTL, item =>
            {
                var CauHoi = listCauHoi.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi);
                if (CauHoi != null)
                    item.NoiDung = CauHoi.NoiDung;

            });
            if (DotThi.listBaiThi != null && CauTL != null && CauTL.Count > 0)
                foreach (var item in DotThi.listBaiThi)
                {
                    item.listCauHoi = CauTL.Where(a => a.IdBaiThi == item.IdGuid).ToList();
                }
            DotThi.listNhanVien = db.HRM_DotThi_NhanVien.Where(a => a.IdDotThi == Id).OrderBy(a => a.Modified).ToList();
            List<string> listId = DotThi.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).ToList();
            var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var model in DotThi.listNhanVien)
            {
                var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == model.IdNhanVien);
                model.NgaySinh = nhanVien.NgaySinh;
                model.MaNhanVien = nhanVien.MaNhanVien;
                model.TenNhanVien = nhanVien.TenDayDu;
                model.TenCongTy = lstBoPhanAll.Where(q => q.Id == nhanVien.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == nhanVien.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();

                if (listChucVu != null && listChucVu.Count > 0)
                {
                    var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                    if (boPhan_NhanVien != null)
                    {
                        model.TenChucVu = boPhan_NhanVien.TenChucVu;
                    }

                }
                if (listChucDanh != null && listChucDanh.Count > 0)
                {
                    var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                    if (boPhan_NhanVien != null)
                    {
                        model.TenChucDanh = boPhan_NhanVien.Ten;

                    }

                }

                if (nhanVien.IdGioiTinh == "KhongXacDinh")
                    model.GioiTinh = "Khác";
                else
                    model.GioiTinh = nhanVien.IdGioiTinh;
            }
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", DotThi);
        }
        public dynamic GetBaiThiByMaDe(string MaDe, string IdDotThi, string IdNhanVien, ApplicationDbContext db)
        {
            var BaiThi = db.HRM_BaiThi.Where(a => a.IdDotThi == IdDotThi && a.MaDe == MaDe && (string.IsNullOrEmpty(IdNhanVien) || a.IdNhanVien == IdNhanVien)).OrderByDescending(a => a.isKetThuc).FirstOrDefault();
            if (BaiThi.isKetThuc == false)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Bài thi chưa làm");
            }
            //else if (BaiThi.isKetThuc == true)
            //    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Đề " + MaDe + " đã được chấm.");
            BaiThi.listCauHoi = db.HRM_BaiThi_Item.Where(a => a.IdBaiThi == BaiThi.IdGuid).OrderBy(a => a.STT).ToList();
            List<string> listIdCauTL = BaiThi.listCauHoi.Select(a => a.IddmCauHoi).Distinct().ToList();
            var CauTLs = db.HRM_dmCauHoi_Item.Where(a => listIdCauTL.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
            var CauHois = db.HRM_dmCauHoi.Where(a => listIdCauTL.Contains(a.IdGuid)).ToList();
            Parallel.ForEach(BaiThi.listCauHoi, item =>
            {
                item.NoiDung = CauHois.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi).NoiDung;
                item.listDapAn = CauTLs.Where(a => a.IddmCauHoi == item.IddmCauHoi).ToList();
                if (item.listDapAn.Where(a => string.IsNullOrEmpty(a.Ten)).Count() > 0)
                {
                    int i = 1;
                    foreach (var tl in item.listDapAn)
                    {
                        tl.Ten = dungchung.GetAlphabeticalOrder(i);
                        i++;
                    }
                }
            });

            db.Entry(BaiThi).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", BaiThi);
        }
        public dynamic GetBaiThiByIdNhanVien(string Id, string IdDotThi, ApplicationDbContext db, string MaDe = "")
        {
            var NhanVienDT = db.HRM_DotThi_NhanVien.FirstOrDefault(a => a.IdNhanVien == Id && a.IdDotThi == IdDotThi && a.isKetThuc == false);
            if (NhanVienDT == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Nhân viên không có bài thi hoặc đã làm bài");
            }
            var BaiThi = new HRM_BaiThi();
            if (!string.IsNullOrEmpty(MaDe))
                BaiThi = db.HRM_BaiThi.Where(a => a.isKetThuc == false && a.isOnline == false && a.IdDotThi == IdDotThi && a.MaDe == MaDe).FirstOrDefault();
            else
                BaiThi = db.HRM_BaiThi.Where(a => a.isKetThuc == false && a.isOnline == true && a.IdDotThi == IdDotThi && a.IdNhanVien == Id).FirstOrDefault();
            if (BaiThi == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không có bài Thi");
            }
            else if (BaiThi.isKetThuc == true)
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Đề " + MaDe + " đã được làm.");
            BaiThi.listCauHoi = db.HRM_BaiThi_Item.Where(a => a.IdBaiThi == BaiThi.IdGuid).OrderBy(a => a.STT).ToList();
            List<string> listIdCauTL = BaiThi.listCauHoi.Select(a => a.IddmCauHoi).Distinct().ToList();
            var CauTLs = db.HRM_dmCauHoi_Item.Where(a => listIdCauTL.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
            var CauHois = db.HRM_dmCauHoi.Where(a => listIdCauTL.Contains(a.IdGuid)).ToList();
            Parallel.ForEach(BaiThi.listCauHoi, item =>
            {
                item.NoiDung = CauHois.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi).NoiDung;
                item.listDapAn = CauTLs.Where(a => a.IddmCauHoi == item.IddmCauHoi).ToList();
                if (item.listDapAn.Where(a => string.IsNullOrEmpty(a.Ten)).Count() > 0)
                {
                    int i = 1;
                    foreach (var tl in item.listDapAn)
                    {
                        tl.Ten = dungchung.GetAlphabeticalOrder(i);
                        i++;
                    }
                }
            });
            BaiThi.IdNhanVien = NhanVienDT.IdNhanVien;
            BaiThi.SoBaoDanh = NhanVienDT.SoBaoDanh;

            db.Entry(BaiThi).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", BaiThi);
        }
        public dynamic GetThongTinNhanVienBySoBaoDanh(string TenNhanVien, string IdDotThi, ApplicationDbContext db)
        {
            var listnhanVien = db.HRM_NhanVien.Where(a => a.TenDayDu.ToLower().Contains(TenNhanVien.ToLower())).ToList();
            if (listnhanVien == null)
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Tên nhân viên không tồn tại");
            List<string> listIdNhanVien = listnhanVien.Select(a => a.IdGuid).Distinct().ToList();
            var models = db.HRM_DotThi_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.IdDotThi == IdDotThi && a.isKetThuc == false).ToList();

            if (models == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Bạn không có bài thi hoặc đã làm bài");
            }
            var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var model in models)
            {
                var nhanVien = listnhanVien.FirstOrDefault(a => a.IdGuid == model.IdNhanVien);
                model.TenNhanVien = nhanVien.TenDayDu;
                model.TenCongTy = lstBoPhanAll.Where(q => q.Id == nhanVien.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == nhanVien.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                model.NgaySinh = nhanVien.NgaySinh;
                if (listChucVu != null && listChucVu.Count > 0)
                {
                    var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
                    if (boPhan_NhanVien != null)
                    {
                        model.TenChucVu = boPhan_NhanVien.TenChucVu;

                    }

                }
                if (listChucDanh != null && listChucDanh.Count > 0)
                {
                    var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
                    if (boPhan_NhanVien != null)
                    {
                        model.TenChucDanh = boPhan_NhanVien.Ten;
                    }
                }

                if (nhanVien.IdGioiTinh == "KhongXacDinh")
                    model.GioiTinh = "Khác";
                else model.GioiTinh = nhanVien.IdGioiTinh;
            }
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", models);
        }
        public class HRM_BaiThiRanDom
        {
            public List<string> listIdNhanVien { get; set; }
            public string MaDotThi { get; set; }
            public string IddmCauTrucDe { get; set; }
            public bool ChungDe { get; set; }
            public int SoLuongOffline { get; set; }
        }
        public dynamic GetBaiThiRandom(List<string> listIdNhanVien, string MaDotThi, string IddmCauTrucDe, bool ChungDe, int SoLuongOffline, ApplicationDbContext db) // loai 0: 1 đề, 1 mội người 1 đề
        {
            var CauTruc = db.HRM_dmCauTrucDe.Where(a => a.IdGuid == IddmCauTrucDe).FirstOrDefault();
            var listCauTruc_Item = db.HRM_dmCauTrucDe_Item.Where(a => a.IddmCauTrucDe == IddmCauTrucDe).OrderBy(a => a.STT).ToList();
            List<string> listIdNhomCauHoi = listCauTruc_Item.Select(a => a.IddmNhomCauHoi).Distinct().ToList();
            var listNhomCauHois = db.HRM_dmNhomCauHoi.Where(a => listIdNhomCauHoi.Contains(a.IdGuid)).OrderBy(a => a.IdGuid).ToList();
            var listCauHois = db.HRM_dmCauHoi.Where(a => listIdNhomCauHoi.Contains(a.IddmNhomCauHoi)).ToList();
            List<HRM_BaiThi> listBaiThi = new List<HRM_BaiThi>();

            if (ChungDe == false)
            {

                Random random = new Random();
                int i = 1;
                foreach (string Id in listIdNhanVien)
                {
                    HRM_BaiThi itemAdd = new HRM_BaiThi();
                    if (i < 10)
                        itemAdd.MaDe = "BM-0" + i;
                    else
                        itemAdd.MaDe = "BM-" + i;
                    itemAdd.IddmCauTrucDe = IddmCauTrucDe;
                    itemAdd.IdNhanVien = Id;
                    itemAdd.DiemDat = CauTruc.Diem;
                    itemAdd.listCauHoi = new List<HRM_BaiThi_Item>();
                    itemAdd.isOnline = true;
                    if (i <= SoLuongOffline)
                        itemAdd.isOnline = false;
                    int STT = 1;
                    foreach (var cautruc in listCauTruc_Item)
                    {
                        itemAdd.listCauHoi.AddRange(listCauHois.Where(a => a.IddmNhomCauHoi == cautruc.IddmNhomCauHoi).OrderBy(a => random.Next()).Take(cautruc.SoLuong).Select(a => new HRM_BaiThi_Item(a)).ToList());
                    }
                    foreach (var item in itemAdd.listCauHoi)
                    {
                        item.Cau = "Câu " + STT;
                        STT++;
                    }
                    listBaiThi.Add(itemAdd);
                    i++;
                }
            }
            else if (ChungDe == true)
            {
                Random random = new Random();
                List<HRM_dmCauHoi> listCauHoi = new List<HRM_dmCauHoi>();
                foreach (var cautruc in listCauTruc_Item)
                {
                    listCauHoi.AddRange(listCauHois.Where(a => a.IddmNhomCauHoi == cautruc.IddmNhomCauHoi).OrderBy(a => random.Next()).Take(cautruc.SoLuong).ToList());
                }
                int i = 1;
                foreach (string Id in listIdNhanVien)
                {
                    HRM_BaiThi itemAdd = new HRM_BaiThi();
                    itemAdd.MaDe = "BM_01";
                    itemAdd.IddmCauTrucDe = IddmCauTrucDe;
                    itemAdd.IdNhanVien = Id;
                    itemAdd.DiemDat = CauTruc.Diem;
                    itemAdd.isOnline = true;
                    if (i <= SoLuongOffline)
                        itemAdd.isOnline = false;
                    itemAdd.listCauHoi = new List<HRM_BaiThi_Item>();
                    itemAdd.listCauHoi.AddRange(listCauHoi.Select(a => new HRM_BaiThi_Item(a)).ToList());
                    int STT = 1;
                    foreach (var item in itemAdd.listCauHoi)
                    {
                        item.Cau = "Câu " + STT;
                        STT++;
                    }
                    listBaiThi.Add(itemAdd);
                    i++;
                }
            }

            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", listBaiThi);
        }
        public dynamic DeleteDotThi(string Id, ApplicationDbContext db)
        {
            HRM_DotThi DotThi = db.HRM_DotThi.FirstOrDefault(r => r.IdGuid == Id);
            var CauOld = db.HRM_BaiThi.Where(a => a.IdDotThi == Id).ToList();
            if (CauOld != null && CauOld.Count > 0)
                db.HRM_BaiThi.RemoveRange(CauOld);
            var CauTLOld = db.HRM_BaiThi_Item.Where(a => a.IdDotThi == Id).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                db.HRM_BaiThi_Item.RemoveRange(CauTLOld);

            if (DotThi == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_DotThi.Remove(DotThi);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        public ErrorMessage ExportDanhSachDeThi(ApplicationDbContext _dbContext, HRM_DotThi entity)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaiKiemTraICT.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "BaiKiemTraICTDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheetMau = package.Workbook.Worksheets["Mẫu"];
                if (entity.listBaiThi != null && entity.listBaiThi.Count > 0)
                {
                    var NhomCauHoidbs = _dbContext.HRM_dmNhomCauHoi.ToList();
                    List<string> listIdCauHois = entity.listBaiThi.SelectMany(a => a.listCauHoi.Select(b => b.IddmCauHoi)).ToList();
                    var listCauTraLoidb = _dbContext.HRM_dmCauHoi_Item.Where(a => listIdCauHois.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
                    var listCauHoidb = _dbContext.HRM_dmCauHoi.Where(a => listIdCauHois.Contains(a.IdGuid)).ToList();
                    if (entity.ChungDe == true)
                    {
                        ExcelWorksheet Sheet1 = package.Workbook.Worksheets.Add("Đề 1", sheetMau);
                        HRM_BaiThi baithi = entity.listBaiThi.FirstOrDefault();
                        List<string> listIdNhom = baithi.listCauHoi.Select(a => a.IddmNhomCauHoi).Distinct().ToList();
                        var NhomCauHois = NhomCauHoidbs.Where(a => listIdNhom.Contains(a.IdGuid)).OrderBy(a => a.IdGuid).ToList();
                        List<string> listIdCauHoi = baithi.listCauHoi.Select(a => a.IddmCauHoi).Distinct().ToList();
                        var listCauTraLoi = listCauTraLoidb.Where(a => listIdCauHoi.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
                        var listCauHoi = listCauHoidb.Where(a => listIdCauHoi.Contains(a.IdGuid)).ToList();
                        int nRow = 11;
                        int STT = 1;
                        Sheet1.Cells[1, 9].Value = baithi.MaDe;
                        foreach (var nhom in NhomCauHois)
                        {
                            Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                            Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                            Sheet1.Cells[nRow, 1].Value = "Phần " + STT + ": " + nhom.Ten;
                            Sheet1.Row(nRow).Height = ((nhom.Ten.Length / 80) + 1) * 15.75;
                            nRow++;
                            var listCauHois = baithi.listCauHoi.Where(a => a.IddmNhomCauHoi == nhom.IdGuid).OrderBy(a => a.STT).ToList();
                            int STT_Cau = 1;
                            foreach (var item in listCauHois)
                            {
                                Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                var CauHoi = listCauHoi.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi);
                                Sheet1.Cells[nRow, 1].Value = "Câu " + STT_Cau + ":" + CauHoi.NoiDung;
                                Sheet1.Row(nRow).Height = ((CauHoi.NoiDung.Length / 80) + 1) * 15.75;
                                Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                                nRow++;
                                var listCauTL = listCauTraLoi.Where(a => a.IddmCauHoi == item.IddmCauHoi).ToList();
                                if (listCauTL.Where(a => string.IsNullOrEmpty(a.Ten)).Count() > 0)
                                {
                                    int i = 1;
                                    foreach (var tl in listCauTL)
                                    {
                                        tl.Ten = dungchung.GetAlphabeticalOrder(i);
                                        i++;
                                    }
                                }

                                foreach (var itemTL in listCauTL)
                                {
                                    Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                    Sheet1.Cells[nRow, 1].Value = itemTL.Ten + " " + itemTL.NoiDung;
                                    Sheet1.Row(nRow).Height = ((itemTL.NoiDung.Length / 80) + 1) * 15.75;
                                    nRow++;

                                }
                                STT_Cau++;
                            }
                            STT++;
                        }
                        Sheet1.Cells.Style.Font.Name = "Times New Roman";
                        Sheet1.Cells.Style.Font.Size = 12;
                        Sheet1.Cells.Style.WrapText = true;
                        Sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        Sheet1.Cells[11, 1, nRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        entity.listBaiThi = entity.listBaiThi.Where(a => string.IsNullOrEmpty(a.SoBaoDanh)).ToList();
                        foreach (var baithi in entity.listBaiThi)
                        {
                            ExcelWorksheet Sheet1 = package.Workbook.Worksheets.Add(baithi.MaDe, sheetMau);
                            Sheet1.Cells[1, 9].Value = baithi.MaDe;
                            List<string> listIdNhom = baithi.listCauHoi.Select(a => a.IddmNhomCauHoi).Distinct().ToList();
                            var NhomCauHois = NhomCauHoidbs.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
                            List<string> listIdCauHoi = baithi.listCauHoi.Select(a => a.IddmCauHoi).Distinct().ToList();
                            var listCauTraLoi = listCauTraLoidb.Where(a => listIdCauHoi.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
                            var listCauHoi = listCauHoidb.Where(a => listIdCauHoi.Contains(a.IdGuid)).ToList();
                            int nRow = 11;
                            int STT = 1;
                            foreach (var nhom in NhomCauHois)
                            {
                                Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                                Sheet1.Cells[nRow, 1].Value = "Phần " + STT + ": " + nhom.Ten;
                                Sheet1.Row(nRow).Height = ((nhom.Ten.Length / 80) + 1) * 15.75;
                                nRow++;
                                var listCauHois = baithi.listCauHoi.Where(a => a.IddmNhomCauHoi == nhom.IdGuid).ToList();
                                int STT_Cau = 1;
                                foreach (var item in listCauHois)
                                {
                                    Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                    var CauHoi = listCauHoi.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi);
                                    Sheet1.Cells[nRow, 1].Value = "Câu " + STT_Cau + ":" + CauHoi.NoiDung;
                                    Sheet1.Row(nRow).Height = ((CauHoi.NoiDung.Length / 80) + 1) * 15.75;
                                    Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                                    nRow++;
                                    var listCauTL = listCauTraLoi.Where(a => a.IddmCauHoi == item.IddmCauHoi).ToList();
                                    foreach (var itemTL in listCauTL)
                                    {
                                        Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                        Sheet1.Cells[nRow, 1].Value = itemTL.Ten + " " + itemTL.NoiDung;
                                        Sheet1.Row(nRow).Height = ((itemTL.NoiDung.Length / 80) + 1) * 15.75;
                                        nRow++;
                                    }
                                    STT_Cau++;
                                }
                                STT++;

                            }
                            Sheet1.Cells.Style.Font.Name = "Times New Roman";
                            Sheet1.Cells.Style.Font.Size = 12;
                            Sheet1.Cells.Style.WrapText = true;
                            Sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            Sheet1.Cells[11, 1, nRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        }
                    }
                    package.Workbook.Worksheets.Delete(sheetMau);
                }
                else
                {
                    err.SetLoiKhac("Chưa tạo đề thi");
                    return err;
                }

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("BaiKiemTraICT.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }
        #endregion
        public dynamic GetDanhSachDotDaoTaoNhanVien(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = itemTimKiem.KeyWord;
                int nCurrentPage = itemTimKiem.CurrentPage ?? 1;
                int PageSize = itemTimKiem.PageSize ?? 25;
                var IdNhanVienEOS = _CallWebService.GetNhanSuFromUserPkid(db).Id;
                var IdNhanVien = db.HRM_NhanVien.Where(r => r.IdNhanVienEOS == IdNhanVienEOS).Select(r => r.IdGuid).FirstOrDefault();

                var query = from a in db.HRM_Dot_DaoTaoNhanVien
                            join b in db.HRM_Dot_DaoTaoChiTiet on a.IdChiTietDotDaoTao equals b.IdGuid
                            join c in db.HRM_dmNoiDungDaoTao on b.IddmNoiDungDaoTao equals c.IdGuid
                            where a.IdNhanVien == IdNhanVien
                            select new { a, b, c };

                if (!string.IsNullOrEmpty(KeyWord))
                    query = query.Where(x => x.c.Ten.Contains(KeyWord));
                if (itemTimKiem.TuNgayUnix > 0)
                {
                    DateTime TuNgay = UnixTimeStampToDateTime(itemTimKiem.TuNgayUnix.Value).Date;
                    query = query.Where(x => x.b.NgayBatDau.Date >= TuNgay);
                }
                if (itemTimKiem.DenNgayUnix > 0)
                {
                    DateTime DenNgay = UnixTimeStampToDateTime(itemTimKiem.DenNgayUnix.Value).Date;
                    query = query.Where(x => x.b.NgayBatDau.Date <= DenNgay);
                }

                var projected = query
                    .OrderByDescending(x => x.b.NgayBatDau)
                    .Select(x => new NhanVienDaoTaoViewModel
                    {
                        NoiDungDaoTao = x.c.Ten,
                        NoiDung = x.b.NoiDung,
                        MoTa = x.b.MoTa,
                        NgayBatDau = x.b.NgayBatDau,
                        NgayKetThuc = x.b.NgayKetThuc
                    });

                List<NhanVienDaoTaoViewModel> listItem = Paginate(projected, nCurrentPage, PageSize > 0 ? PageSize : nPageSize, out PagingInfor paging);
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = listItem;
                return dnm;
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                return msg;
            }
        }



        #region Khảo sát
        public dynamic GetListKhaoSatDaoTao(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 20;
            IQueryable<HRM_KhaoSatDaoTao> result = from a in db.HRM_KhaoSatDaoTao
                                                    where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || ((a.NoiDung.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()))))
                                                    orderby a.Modified descending
                                                    select a;

            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
            var listDuAn = _CallWebService.GetDanhSachDuAn(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            foreach (var item in listItem)
            {
                item.TenCongTy = listDuAn.Where(a => a.Id == item.IdCongTy).Select(a => a.TenBoPhan).FirstOrDefault();
                item.TenBoPhan = listBoPhan.Where(a => a.Id == item.IdBoPhan).Select(a => a.TenBoPhan).FirstOrDefault();
                item.TenLoaiKhaoSat = item.LoaiKhaoSat == "KHAOSATDAOTAO" ? "Khảo sát đào tạo" : "Khảo sát kết quả đào tạo";
            }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetKhaoSatDaoTao(HRM_KhaoSatDaoTao entity, ApplicationDbContext _dbContext)
        {

            entity.NgayBDKhaoSat = dungchung.UnixTimeStampToDateTime(entity.NgayBDKhaoSatUnix);
            entity.NgayKTKhaoSat = dungchung.UnixTimeStampToDateTime(entity.NgayKTKhaoSatUnix);
            if (entity.listIdNhanVien != null && entity.listIdNhanVien.Count > 0)
            {
                entity.IdNhanVien = string.Join(",", entity.listIdNhanVien);
            }
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                entity.TrangThai = "ChuaKhaoSat";
                _dbContext.HRM_KhaoSatDaoTao.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            var CauTLOld = _dbContext.HRM_KhaoSatDaoTao_CauHoi.Where(a => a.IdKhaoSat == entity.IdGuid).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                _dbContext.HRM_KhaoSatDaoTao_CauHoi.RemoveRange(CauTLOld);
            var NhanVienOld = _dbContext.HRM_KhaoSatDaoTao_NhanVien.Where(a => a.IdKhaoSat == entity.IdGuid).ToList();
            if (NhanVienOld != null && NhanVienOld.Count > 0)
                _dbContext.HRM_KhaoSatDaoTao_NhanVien.RemoveRange(NhanVienOld);

            if (entity.listCauHoi != null && entity.listCauHoi.Count > 0)
            {
                int STT = 1;
                foreach (var item in entity.listCauHoi)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdKhaoSat = entity.IdGuid;

                    item.SapXep = STT;
                    STT++;
                }
                _dbContext.HRM_KhaoSatDaoTao_CauHoi.AddRange(entity.listCauHoi);
            }
            if (entity.listNhanVien != null && entity.listNhanVien.Count > 0)
            {
                List<HRM_KhaoSatDaoTao_NhanVien> listAdd = new List<HRM_KhaoSatDaoTao_NhanVien>();
                entity.listNhanVien.ForEach(item =>
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    if (item.listCauHoi != null && item.listCauHoi.Count > 0)
                    {
                        int STT = 1;
                        foreach (var CauHoi in item.listCauHoi)
                        {
                            CauHoi.IdGuid = Guid.NewGuid().ToString();
                            CauHoi.IdKhaoSat = entity.IdGuid;
                            CauHoi.IdNhanVien = item.IdNhanVien;
                            CauHoi.SapXep = STT;
                            STT++;
                            listAdd.Add(CauHoi);
                        }
                    }
                    _dbContext.HRM_KhaoSatDaoTao_NhanVien.AddRange(listAdd);
                });

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }
        public dynamic SetKhaoSatDaoTao_NhanVien(HRM_KhaoSatDaoTao_NhanVienView entity, ApplicationDbContext _dbContext, bool isChamDiem = false)
        {

            if (entity.listCauHoi != null)
            {
                foreach (var itemCH in entity.listCauHoi)
                {
                    if (!string.IsNullOrEmpty(entity.TrangThai))
                        itemCH.TrangThai = entity.TrangThai;
                    if (itemCH.ThoiGianDuKienUnix > 0)
                        itemCH.ThoiGianDuKien = dungchung.UnixTimeStampToDateTime(itemCH.ThoiGianDuKienUnix);
                    if (entity.LoaiKhaoSat == "KhaoSatKetQua" && itemCH.Loai == DefineData.eLoaiCauhoi.SapXep.ToString() && itemCH.listDapAn.Where(a => a.SapXepDapAn != 0).Count() > 0)
                        itemCH.DapAn = string.Join(",", itemCH.listDapAn.Select(a => a.SapXepDapAn));
                    if (string.IsNullOrEmpty(itemCH.IdGuid))
                    {
                        itemCH.IdGuid = Guid.NewGuid().ToString();
                        _dbContext.Entry(itemCH).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                    }
                    else
                        _dbContext.Entry(itemCH).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }
        public dynamic GetKhaoSatDaoTao(string Id, ApplicationDbContext db)
        {
            HRM_KhaoSatDaoTao KhaoSatDaoTao = db.HRM_KhaoSatDaoTao.FirstOrDefault(r => r.IdGuid == Id);
            KhaoSatDaoTao.NgayBDKhaoSatUnix = dungchung.DateTimeToUnixTimeStamp(KhaoSatDaoTao.NgayBDKhaoSat);
            KhaoSatDaoTao.NgayKTKhaoSatUnix = dungchung.DateTimeToUnixTimeStamp(KhaoSatDaoTao.NgayKTKhaoSat);
            var KhaoSatDaoTao_NhanVien = db.HRM_KhaoSatDaoTao_NhanVien.Where(a => a.IdKhaoSat == Id).ToList();
            KhaoSatDaoTao.listCauHoi = db.HRM_KhaoSatDaoTao_CauHoi.Where(a => a.IdKhaoSat == Id).OrderBy(a => a.SapXep).ToList();
            KhaoSatDaoTao.listNhanVien = new List<HRM_KhaoSatDaoTao_NhanVienView>();
            if (!string.IsNullOrEmpty(KhaoSatDaoTao.IdNhanVien))
                KhaoSatDaoTao.listIdNhanVien = KhaoSatDaoTao.IdNhanVien.Split(',').ToList();
            //List<string> listId = KhaoSatDaoTao.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
            //var listNhanVien = db.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).ToList();
            //var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(db);
            //var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            //var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            //foreach (var model in KhaoSatDaoTao.listNhanVien)
            //{
            //    var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == model.IdNhanVien);
            //    model.NgaySinh = nhanVien.NgaySinh;
            //    model.MaNhanVien = nhanVien.MaNhanVien;
            //    model.TenNhanVien = nhanVien.TenDayDu;
            //    model.TenCongTy = lstBoPhanAll.Where(q => q.Id == nhanVien.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
            //    model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == nhanVien.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();

            //    if (listChucVu != null && listChucVu.Count > 0)
            //    {
            //        var boPhan_NhanVien = listChucVu.FirstOrDefault(a => a.Id == nhanVien.IdChucVu);
            //        if (boPhan_NhanVien != null)
            //        {
            //            model.TenChucVu = boPhan_NhanVien.TenChucVu;
            //        }

            //    }
            //    if (listChucDanh != null && listChucDanh.Count > 0)
            //    {
            //        var boPhan_NhanVien = listChucDanh.FirstOrDefault(a => a.Id == nhanVien.IdChucDanh);
            //        if (boPhan_NhanVien != null)
            //        {
            //            model.TenChucDanh = boPhan_NhanVien.Ten;

            //        }

            //    }

            //    if (nhanVien.IdGioiTinh == "KhongXacDinh")
            //        model.GioiTinh = "Khác";
            //    else
            //        model.GioiTinh = nhanVien.IdGioiTinh;
            //}
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", KhaoSatDaoTao);
        }
        public dynamic GetKhaoSatDaoTaoForWebById(string Id, ApplicationDbContext db)
        {
            var KhaoSat = db.HRM_KhaoSatDaoTao.FirstOrDefault(a => a.IdGuid == Id);

            var NhanVien = new HRM_KhaoSatDaoTao_NhanVienView();
            var nhanViendb = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
            var KhaoSatDaoTao_NhanVien = db.HRM_KhaoSatDaoTao_NhanVien.Where(a => a.IdKhaoSat == Id && a.IdNhanVien == nhanViendb.IdGuid).ToList();
            NhanVien.NgaySinh = nhanViendb.NgaySinh;
            NhanVien.MaNhanVien = nhanViendb.MaNhanVien;
            NhanVien.TenNhanVien = nhanViendb.TenDayDu;
            NhanVien.TenViTri = _CallWebService.GetChucVu(db, nhanViendb.IdChucVu).TenChucVu;
            NhanVien.TenBoPhan = _CallWebService.GetBoPhan(db, nhanViendb.IdBoPhanEOS).TenBoPhan;
            NhanVien.LoaiKhaoSat = KhaoSat.LoaiKhaoSat;
            NhanVien.TrangThai = KhaoSat.LoaiKhaoSat;
            if (KhaoSatDaoTao_NhanVien == null || KhaoSatDaoTao_NhanVien.Count == 0)
            {
                var CauHoi = db.HRM_KhaoSatDaoTao_CauHoi.Where(a => a.IdKhaoSat == Id).OrderBy(a => a.SapXep).ToList();
                NhanVien.listCauHoi = CauHoi.Select(a => new HRM_KhaoSatDaoTao_NhanVien(a, nhanViendb.IdGuid)).ToList();
            }
            else
            {
                NhanVien.listCauHoi = KhaoSatDaoTao_NhanVien.OrderBy(a => a.SapXep).ToList();
                foreach (var item in NhanVien.listCauHoi)
                    if (item.ThoiGianDuKien.Year > 2000)
                        item.ThoiGianDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiGianDuKien);
            }
            if (NhanVien.LoaiKhaoSat == "KhaoSatKetQua" && NhanVien.listCauHoi != null)
            {
                var listIdCauHoi = NhanVien.listCauHoi.Select(a => a.IddmCauHoi).ToList();
                var listDapAn = db.HRM_dmCauHoi_Item.Where(a => listIdCauHoi.Contains(a.IddmCauHoi)).ToList();
                foreach (var item in NhanVien.listCauHoi)
                {
                    item.listDapAn = listDapAn.Where(a => a.IddmCauHoi == item.IddmCauHoi).OrderBy(a => a.STT).ToList();
                    if (!string.IsNullOrEmpty(item.DapAn) && item.listDapAn != null && item.listDapAn.Count > 0 && item.Loai == DefineData.eLoaiCauhoi.SapXep.ToString())
                    {
                        int i = 0;
                        foreach (var da in item.listDapAn)
                        {

                            da.SapXepDapAn = int.Parse(item.DapAn.Split(',')[i]);
                            i++;
                        }
                    }
                    else
                    {
                        foreach (var da in item.listDapAn)
                        {

                            da.SapXepDapAn = 0;
                        }
                    }


                }
            }
            NhanVien.TrangThai = NhanVien.listCauHoi.FirstOrDefault().TrangThai;
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Lấy dữ liệu  thành công", NhanVien);
        }
        public ErrorMessage ExportKhaoSatDaoTao(ApplicationDbContext _dbContext, string Id)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/KhaoSatKetQua.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = PathUploads + "KhaoSatDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileNameCopy);
                ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheetMau = package.Workbook.Worksheets["Mẫu"];
                ExcelWorksheet sheetMauNC = package.Workbook.Worksheets["Mẫu Nhu Cầu"];
                HRM_KhaoSatDaoTao entity = _dbContext.HRM_KhaoSatDaoTao.FirstOrDefault(r => r.IdGuid == Id);
                entity.listCauHoi = _dbContext.HRM_KhaoSatDaoTao_CauHoi.Where(a => a.IdKhaoSat == Id).OrderBy(a => a.SapXep).ToList();
                entity.listNhanVien = new List<HRM_KhaoSatDaoTao_NhanVienView>();
                if (!string.IsNullOrEmpty(entity.IdNhanVien))
                    entity.listIdNhanVien = entity.IdNhanVien.Split(',').ToList();
                if (entity.listCauHoi != null && entity.listCauHoi.Count > 0)
                {
                    var listNhanVien = _dbContext.HRM_NhanVien.Where(a => entity.listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var NhomCauHoidbs = _dbContext.HRM_dmNhomCauHoi.ToList();
                    var KhaoSatDaoTao_NhanVien = _dbContext.HRM_KhaoSatDaoTao_NhanVien.Where(a => a.IdKhaoSat == Id).ToList();
                    List<string> listIdCauHois = entity.listCauHoi.Select(b => b.IddmCauHoi).ToList();
                    var listCauTraLoidb = _dbContext.HRM_dmCauHoi_Item.Where(a => listIdCauHois.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
                    var listCauHoidb = _dbContext.HRM_dmCauHoi.Where(a => listIdCauHois.Contains(a.IdGuid)).ToList();

                    if (entity.LoaiKhaoSat == "KhaoSatKetQua")
                    {
                        foreach (var NhanVien in listNhanVien)
                        {
                            ExcelWorksheet Sheet1 = package.Workbook.Worksheets.Add(NhanVien.MaNhanVien, sheetMau);
                            Sheet1.Cells[5, 3].Value = NhanVien.TenDayDu;
                            int nRow = 8;
                            int STT = 1;
                            foreach (var nhom in NhomCauHoidbs)
                            {
                                Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                                Sheet1.Cells[nRow, 1].Value = "Phần " + STT + ": " + nhom.Ten;
                                Sheet1.Row(nRow).Height = ((nhom.Ten.Length / 80) + 1) * 15.75;
                                nRow++;
                                var listCauHois = entity.listCauHoi.Where(a => a.IddmNhomCauHoi == nhom.IdGuid).OrderBy(a => a.SapXep).ToList();
                                int STT_Cau = 1;
                                foreach (var item in listCauHois)
                                {
                                    Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                    var CauHoi = listCauHoidb.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi);
                                    Sheet1.Cells[nRow, 1].Value = "Câu " + STT_Cau + ":" + CauHoi.NoiDung;
                                    Sheet1.Row(nRow).Height = ((CauHoi.NoiDung.Length / 80) + 1) * 15.75;
                                    Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                                    nRow++;
                                    var listCauTL = listCauTraLoidb.Where(a => a.IddmCauHoi == item.IddmCauHoi).ToList();
                                    if (listCauTL.Where(a => string.IsNullOrEmpty(a.Ten)).Count() > 0)
                                    {
                                        int i = 1;
                                        foreach (var tl in listCauTL)
                                        {
                                            tl.Ten = dungchung.GetAlphabeticalOrder(i);
                                            i++;
                                        }
                                    }

                                    foreach (var itemTL in listCauTL)
                                    {
                                        Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                        Sheet1.Cells[nRow, 1].Value = itemTL.Ten + " " + itemTL.NoiDung;
                                        Sheet1.Row(nRow).Height = ((itemTL.NoiDung.Length / 80) + 1) * 15.75;
                                        nRow++;

                                    }
                                    // câu trả lời
                                    var cautl = KhaoSatDaoTao_NhanVien.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.IddmCauHoi == item.IddmCauHoi).FirstOrDefault();
                                    Sheet1.Cells[nRow, 1, nRow, 10].Merge = true;
                                    if (cautl != null)
                                        Sheet1.Cells[nRow, 1].Value = "Trả lời: " + cautl.DapAn;
                                    nRow++;

                                    STT_Cau++;
                                }
                                STT++;
                            }
                            Sheet1.Cells.Style.Font.Name = "Times New Roman";
                            Sheet1.Cells.Style.Font.Size = 12;
                            Sheet1.Cells.Style.WrapText = true;
                            Sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            Sheet1.Cells[5, 1, nRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        }
                    }
                    else
                    {
                        var listHinhThucDaoTao = _dbContext.HRM_dmHinhThucDaoTao.ToList();
                        foreach (var NhanVien in listNhanVien)
                        {
                            ExcelWorksheet Sheet1 = package.Workbook.Worksheets.Add(NhanVien.MaNhanVien, sheetMauNC);
                            Sheet1.Cells[5, 3].Value = NhanVien.TenDayDu;
                            int nRow = 9;
                            int STT = 1;
                            foreach (var nhom in NhomCauHoidbs)
                            {
                                Sheet1.Cells[nRow, 1].Value = "Phần " + STT + ": " + nhom.Ten;
                                Sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                                nRow++;
                                var listCauHois = entity.listCauHoi.Where(a => a.IddmNhomCauHoi == nhom.IdGuid).OrderBy(a => a.SapXep).ToList();
                                int STT_Cau = 1;
                                foreach (var item in listCauHois)
                                {
                                    int nCol = 1;
                                    var CauHoi = listCauHoidb.FirstOrDefault(a => a.IdGuid == item.IddmCauHoi);
                                    Sheet1.Cells[nRow, nCol].Value = STT_Cau;
                                    nCol++;
                                    Sheet1.Cells[nRow, nCol].Value = CauHoi.NoiDung;
                                    nCol++;
                                    // câu trả lời
                                    var cautl = KhaoSatDaoTao_NhanVien.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.IddmCauHoi == item.IddmCauHoi).FirstOrDefault();
                                    if (cautl != null)
                                    {
                                        if (cautl.NhuCauDaoTao == 1)
                                            Sheet1.Cells[nRow, nCol].Value = "ĐÃ ĐƯỢC ĐÀO TẠO";
                                        else if (cautl.NhuCauDaoTao == 2)
                                            Sheet1.Cells[nRow, nCol].Value = "ĐÃ ĐƯỢC ĐÀO TẠO CẦN ĐÀO TẠO LẠI";
                                        else if (cautl.NhuCauDaoTao == 3)
                                            Sheet1.Cells[nRow, nCol].Value = "CHƯA ĐƯỢC ĐÀO TẠO";

                                        nCol++;
                                        Sheet1.Cells[nRow, nCol].Value = cautl.MucTieuDatDuoc;
                                        nCol++;
                                        if (cautl.MucDo > 0)
                                            Sheet1.Cells[nRow, nCol].Value = cautl.MucDo == 1 ? "Cơ bản" : "Nâng cao";
                                        nCol++;
                                        if (cautl.ThoiGianDuKien != null && cautl.ThoiGianDuKien.Year > 2000)
                                            Sheet1.Cells[nRow, nCol].Value = cautl.ThoiGianDuKien.ToString("dd/MM/yyyy");
                                        nCol++;
                                        if (!string.IsNullOrEmpty(cautl.IdHinhThucDaoTao))
                                            Sheet1.Cells[nRow, nCol].Value = listHinhThucDaoTao.Where(a => a.IdGuid == cautl.IdHinhThucDaoTao).Select(a => a.Ten).FirstOrDefault();
                                        nCol++;

                                        Sheet1.Cells[nRow, nCol].Value = cautl.DeXuatThem;
                                        nCol++;


                                    }
                                    nRow++;

                                    STT_Cau++;
                                }
                                STT++;
                            }
                            dungchung.DrawTableExcel(Sheet1, 8, nRow - 1, 1, 8);
                            Sheet1.Cells.Style.Font.Name = "Times New Roman";
                            Sheet1.Cells.Style.Font.Size = 12;
                            Sheet1.Cells.Style.WrapText = true;
                            Sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            Sheet1.Cells[9, 1, nRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            Sheet1.Cells[9, 2, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            Sheet1.Cells[9, 7, nRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        }
                        package.Workbook.Worksheets.Delete(sheetMau);
                        package.Workbook.Worksheets.Delete(sheetMauNC);
                    }


                    s.Close();
                    byte[] bytee = package.GetAsByteArray();
                    File.WriteAllBytes(sFileNameCopy, bytee);
                    err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("BaiKiemTraICT.xlsx")
                                    + "&path=" + dungchung.Base64Encode(sFileNameCopy);
                }
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }
        #endregion
    }
    public class NhanVienDaoTaoViewModel
    {
        public string NoiDungDaoTao { set; get; }
        public string NoiDung { set; get; }
        public string MoTa { set; get; }
        public DateTime NgayBatDau { set; get; }
        public double NgayBatDauUnix { set; get; }
        public DateTime NgayKetThuc { set; get; }
        public double NgayKetThucUnix { set; get; }
    }
    public class KeHoachDaoTaoChiTietViewModel
    {
        public string IdChiTiet { set; get; }
        public string IdQuyTrinhTongHop { set; get; }
        public int SoNhanVien { get; set; }
        public double KeHoachKinhPhi { get; set; }
        public string SoPhieu { set; get; }
        public int Nam { set; get; }
        public string IddmNoiDungDaoTao { set; get; }
        public string TendmNoiDungDaoTao { set; get; }

        public DateTime NgayTongHop { set; get; }
        public double NgayTongHopUnix { set; get; }
        public string IddmHinhThucDaoTao { set; get; }

        public string NoiDung { set; get; }
        public bool isThang1 { get; set; }
        public bool isThang2 { get; set; }
        public bool isThang3 { get; set; }
        public bool isThang4 { get; set; }
        public bool isThang5 { get; set; }
        public bool isThang6 { get; set; }
        public bool isThang7 { get; set; }
        public bool isThang8 { get; set; }
        public bool isThang9 { get; set; }
        public bool isThang10 { get; set; }
        public bool isThang11 { get; set; }
        public bool isThang12 { get; set; }


        public List<HRM_TongHop_KeHoachDaoTaoNhanVien> listNhanVien = new List<HRM_TongHop_KeHoachDaoTaoNhanVien>();
    }
}