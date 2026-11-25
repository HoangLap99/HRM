#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using HRM.Api.Services.HopDong;
using HRM.Api.Entity;
using HRM.Api.Helper;
using HRM.Api.Models;
using HRM.Api.Models.NotificationModel;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static HRM.Api.Services.FuncBase;
using static HRM.Api.Services.NhanVien.FuncKeHoachLamViec;
using static HRM.Api.Helper.HelperConstant;

namespace HRM.Api.Services.TuyenDung
{


    public class FuncTuyenDung : FuncBase
    {
        public FuncNhanVien _FuncNhanVien { get; set; }
        private readonly FuncHopDong _funcHopDong;
        private string Host => HrmOptions.Host ?? "0";

        public FuncTuyenDung(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncTuyenDung> logger,
            FuncNhanVien funcNhanVien,
            FuncHopDong funcHopDong)
            : base(callWebService, workflowEngine, options, logger)
        {
            _FuncNhanVien = funcNhanVien;
            _funcHopDong = funcHopDong;
        }

        //  DefineData.eAction eAction = DefineData.eAction.QUYTRINHTUYENDUNG;
        // FileUploadController FileUpload = new FileUploadController();

        #region Tiếp nhận ứng viên 
        public string GetAnTabChuaSoVan()
        {
            string Host = (HrmOptions?.Host ?? "");

            return Host;
        }
        public string GetNextMaUngVien(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_TuyenDung_UngVien.Select(x => x.MaUngVien), "UV");

        public dynamic GetThongTinUngVienById(string Id, ApplicationDbContext db)
        {
            HRM_TuyenDung_UngVien model = db.HRM_TuyenDung_UngVien.Find(Id);
            if (model == null) return null;
            model.IdTongHopDeNghiTuyenDung = model.IdQuyTrinhNhuCau ?? model.IdNhuCauChiTiet;
            if (model.NgaySinh != null)
                model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
            if (model.NgayCap != null)
                model.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayCap);
            if (model.NgayNhanViecDuKien != null)
                model.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayNhanViecDuKien);
            if (model.IdCoSoTrungTuyen == null || model.IdCoSoTrungTuyen == 0)
                model.IdCoSoTrungTuyen = model.IdCongTyEOS;
            model.NgayTiepNhanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTiepNhan);
            var lstKinhNghiem = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(r => r.IdTuyenDungUngVien == model.IdGuid).ToList();
            model.lstKinhNghiemLamViec = lstKinhNghiem;
            var TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == model.IdTongHopDeNghiTuyenDung).FirstOrDefault();
            if (TongHopDeNghiTuyenDung != null)
                model.TongHopDeNghiTuyenDung = TongHopDeNghiTuyenDung;


            var QuanHuyen = db.HRM_dmQuanHuyen.Find(model.IddmQuanHuyen);
            if (QuanHuyen != null)
                model.QuanHuyen = QuanHuyen;
            var Tinh = db.HRM_dmTinh.Find(model.IddmTinhThanhPho);
            if (Tinh != null)
                model.TinhThanhPho = Tinh;
            var PhuongXa = db.HRM_dmPhuongXa.Find(model.IddmPhuongXa);
            if (Tinh != null)
                model.PhuongXa = PhuongXa;
            var listQuanHeGiaDinh = db.HRM_UngVienQuanHeGiaDinh.AsNoTracking().Where(x => x.IdUngVien == model.IdGuid).ToList();
            if (listQuanHeGiaDinh.IsNotEmpty())
            {
                foreach (var item in listQuanHeGiaDinh)
                {
                    if (item.NgaySinh != null)
                        item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                }
            }
            model.ListQuanHeGiaDinhUngVien = listQuanHeGiaDinh;
            var listBangCap = db.HRM_UngVienBangCap.AsNoTracking().Where(x => x.IdUngVien == model.IdGuid).ToList();
            if (listBangCap.IsNotEmpty())
            {
                var filesBangCap = FileUploadController.GetListFileDinhKemByIds(db, listBangCap.Select(x => x.IdGuid), nameof(HRM_UngVienBangCap));
                foreach (var item in listBangCap)
                {
                    item.lstFileDinhKem = filesBangCap[item.IdGuid].ToList();
                }
            }

            model.ListUngVienBangCap = listBangCap;
            var listNgonNgu = db.HRM_NgonNguUngVien.AsNoTracking().Where(x => x.IdUngVien == model.IdGuid).ToList();
            if (listNgonNgu.IsNotEmpty())
            {
                var filesNgonNgu = FileUploadController.GetListFileDinhKemByIds(db, listNgonNgu.Select(x => x.IdGuid), nameof(HRM_NgonNguUngVien));
                foreach (var item in listNgonNgu)
                {
                    item.lstFileDinhKem = filesNgonNgu[item.IdGuid].ToList();
                }
            }

            model.ListUngVienNgonNgu = listNgonNgu;
            var listChungChi = db.HRM_ChungChiUngVien.AsNoTracking().Where(x => x.IdUngVien == model.IdGuid).ToList();
            if (listChungChi.IsNotEmpty())
            {
                var filesChungChi = FileUploadController.GetListFileDinhKemByIds(db, listChungChi.Select(x => x.IdGuid), nameof(HRM_ChungChiUngVien));
                foreach (var item in listChungChi)
                {
                    if (item.NgayCap != null)
                        item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                    if (item.NgayHetHan != null)
                        item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
                    item.lstFileDinhKem = filesChungChi[item.IdGuid].ToList();
                }
            }

            model.ListUngVienChungChi = listChungChi;

            model.listFileDinhKem = GetListFileDinhKem(db, "HRM_TuyenDung_UngVien", Id);
            return model;
        }
        public dynamic DeleteUngVien(string Id, ApplicationDbContext db)
        {
            try
            {
                HRM_TuyenDung_UngVien model = db.HRM_TuyenDung_UngVien.Find(Id);
                db.HRM_TuyenDung_UngVien.Remove(model);
                var bangCaps = db.HRM_UngVienBangCap.Where(r => r.IdUngVien == model.IdGuid).ToList();
                var quanheGiadinh = db.HRM_UngVienQuanHeGiaDinh.Where(r => r.IdUngVien == model.IdGuid).ToList();
                db.HRM_UngVienBangCap.RemoveRange(bangCaps);
                db.HRM_UngVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
            }
        }
        public dynamic DeletelistUngVien(List<string> listId, ApplicationDbContext db)
        {
            try
            {
                List<HRM_TuyenDung_UngVien> model = db.HRM_TuyenDung_UngVien.Where(a => listId.Contains(a.IdGuid)).ToList();
                db.HRM_TuyenDung_UngVien.RemoveRange(model);
                var bangCaps = db.HRM_UngVienBangCap.Where(r => listId.Contains(r.IdUngVien)).ToList();
                var quanheGiadinh = db.HRM_UngVienQuanHeGiaDinh.Where(r => listId.Contains(r.IdUngVien)).ToList();
                db.HRM_UngVienBangCap.RemoveRange(bangCaps);
                db.HRM_UngVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
            }
        }
        public dynamic SetUngVienNhanViec(List<string> listId, bool IsNhanViec, ApplicationDbContext db)
        {
            try
            {
                List<HRM_TuyenDung_UngVien> model = db.HRM_TuyenDung_UngVien.Where(a => listId.Contains(a.IdGuid)).ToList();
                foreach (var item in model)
                {
                    if (IsNhanViec == true)
                        item.IsNhanViec = true;
                    else
                        item.IsNghiSauKhiNhanViec = true;
                    db.Entry(item).State = EntityState.Modified;
                }
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
            }
        }
        public dynamic SetDaUngVienSoVanBylistId(List<string> listId, ApplicationDbContext db)
        {
            try
            {
                List<HRM_TuyenDung_UngVien> model = db.HRM_TuyenDung_UngVien.Where(a => listId.Contains(a.IdGuid)).ToList();
                foreach (var item in model)
                {
                    item.TrangThai = 2;
                    db.Entry(item).State = EntityState.Modified;
                }
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
            }
        }
        public dynamic SetDatPhongVanBylistId(List<string> listId, ApplicationDbContext db)
        {
            try
            {
                List<HRM_TuyenDung_UngVien> model = db.HRM_TuyenDung_UngVien.Where(a => listId.Contains(a.IdGuid)).ToList();
                foreach (var item in model)
                {
                    item.TrangThai = 3;
                    db.Entry(item).State = EntityState.Modified;
                }
                List<HRM_TuyenDung_QuyTrinhPhongVan> PVs = db.HRM_TuyenDung_QuyTrinhPhongVan.Where(a => listId.Contains(a.IdUngVien)).ToList();
                foreach (var item in PVs)
                {
                    item.TrangThai = 10;
                    db.Entry(item).State = EntityState.Modified;
                }

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
            }
        }

        public dynamic HuyNhanViecUngVien(string Id, ApplicationDbContext db)
        {
            try
            {
                List<HRM_TuyenDung_UngVien> model = db.HRM_TuyenDung_UngVien.Where(a => Id == a.IdGuid).ToList();
                foreach (var item in model)
                {

                    item.IsNhanViec = false;

                    db.Entry(item).State = EntityState.Modified;
                }
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
            }
        }
        public dynamic SetThongTinUngVien(HRM_TuyenDung_UngVien model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    var UngVien = db.HRM_TuyenDung_UngVien.Where(a => a.DienThoaiDiDong == model.DienThoaiDiDong && a.IdGuid != model.IdGuid).FirstOrDefault();
                    if (UngVien != null && UngVien.TrangThai != 4 /*&& UngVien.TrangThai!=3*/)
                    {
                        //if (UngVien.TrangThai == 3)
                        //{
                            bool check = false;
                            if (UngVien.IsNhanViec == true)
                            {
                                if (UngVien.IsNghiSauKhiNhanViec == true)
                                    check = true;
                                else
                                {
                                    var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUngVien == UngVien.IdGuid);
                                    if (NhanVien == null)
                                        check = true;
                                    else
                                    {
                                        if (NhanVien.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString())
                                            check = true;
                                    }
                                }
                            }

                            if (check == true)
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số điện thoại đã tồn tại ở ứng viên: " + UngVien.HoTenDayDu);
                        //}
                        //else
                        //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số điện thoại đã tồn tại ở ứng viên: " + UngVien.HoTenDayDu);

                    }
                    var checkUpdate = FuncUpdateThongTinUngVien(model, db);
                    if (checkUpdate == false)
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý ");
                    db.SaveChanges();
                    ts.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ", model.IdGuid);
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình sử lý " + ex.Message);
                }

            }
        }
        public bool FuncUpdateThongTinUngVien(HRM_TuyenDung_UngVien model, ApplicationDbContext db)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Ho))
                    model.Ho = model.Ho.Substring(0, 1).ToUpper() + model.Ho.Substring(1);
                if (!string.IsNullOrEmpty(model.Ten))
                    model.Ten = model.Ten.Substring(0, 1).ToUpper() + model.Ten.Substring(1);
                if (!string.IsNullOrEmpty(model.Ho) && !string.IsNullOrEmpty(model.Ten))
                {
                    if (string.IsNullOrEmpty(model.TenDem))
                    {
                        model.HoTenDayDu = model.Ho + " " + model.Ten;
                    }
                    else
                    {
                        model.TenDem = model.TenDem.Substring(0, 1).ToUpper() + model.TenDem.Substring(1);
                        model.HoTenDayDu = model.Ho + " " + model.TenDem + " " + model.Ten;
                    }
                }
                if (model.NgaySinhUnix != 0)
                    model.NgaySinh = dungchung.UnixTimeStampToDateTime(model.NgaySinhUnix);
                if (model.NgayCapUnix != 0)
                    model.NgayCap = dungchung.UnixTimeStampToDateTime(model.NgayCapUnix);

                if (model.NgayTiepNhanUnix != 0)
                    model.NgayTiepNhan = dungchung.UnixTimeStampToDateTime(model.NgayTiepNhanUnix);

                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    model.isDaPheDuyetLuong = false;
                    if (!string.IsNullOrEmpty(model.IdNhuCauChiTiet))
                    {
                        var NhuCauTuyenDung = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == model.IdNhuCauChiTiet);
                        if (NhuCauTuyenDung != null)
                        {
                            model.IdNhuCauChiTiet = NhuCauTuyenDung.IdGuid;
                            model.IdQuyTrinhNhuCau = NhuCauTuyenDung.IdGuid;
                            model.IdBoPhanEOS = NhuCauTuyenDung.IdBoPhan;
                            //model.IdChucVu = NhuCauTuyenDung.IdChucVu;
                            model.IdCongTyEOS = NhuCauTuyenDung.IdCongTy;
                            model.DotTuyenDung = NhuCauTuyenDung.LyDo;
                        }
                    }
                    else
                    {
                        var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
                        if (NhanVien != null)
                        {
                            model.IdCongTyEOS = NhanVien.IdCongTyEOS;
                            model.IdChucVu = NhanVien.IdChucVu;
                        }


                    }
                    string Host = (HrmOptions?.Host ?? "");
                    if (Host == "ICT")
                        model.TrangThai = 2;
                    db.HRM_TuyenDung_UngVien.Add(model);
                    _CallWebService.AddModify(db, "Tuyển dụng nhân viên", DefineData.eModify.Them.ToString(), DefineData.eAction.TUYENDUNGUNGVIEN.ToString(), model.HoTenDayDu + "(" + model.MaNhanVien + ")", model.IdGuid, "", JsonConvert.SerializeObject(model), model.IdCongTyEOS, db.currentUser.Id, db.currentUser.TenNhanVien);

                }
                else
                {
                    var lstKinhNghiem = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(r => r.IdTuyenDungUngVien == model.IdGuid).ToList();
                    db.HRM_TuyenDung_UngVienKinhNghiemLamViec.RemoveRange(lstKinhNghiem);
                    //if (!string.IsNullOrEmpty(model.IdNhuCauChiTiet))
                    //{
                    //    var NhuCauTuyenDung = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == model.IdNhuCauChiTiet);
                    //    if (NhuCauTuyenDung != null)
                    //    {
                    //        model.IdNhuCauChiTiet = NhuCauTuyenDung.IdGuid;
                    //        model.IdQuyTrinhNhuCau = NhuCauTuyenDung.IdGuid;
                    //        model.IdBoPhanEOS = NhuCauTuyenDung.IdBoPhan;
                    //        model.IdChucVu = NhuCauTuyenDung.IdChucVu;
                    //        model.IdCongTyEOS = NhuCauTuyenDung.IdCongTy;
                    //        model.DotTuyenDung = NhuCauTuyenDung.LyDo;
                    //    }
                    //}
                    //else
                    //{
                    //    var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
                    //    if (NhanVien != null)
                    //    {
                    //        model.IdCongTyEOS = NhanVien.IdCongTyEOS;
                    //        model.IdChucVu = NhanVien.IdChucVu;
                    //    }


                    //}
                    db.Entry(model).State = EntityState.Modified;
                    _CallWebService.AddModify(db, "Tuyển dụng nhân viên", DefineData.eModify.Sua.ToString(), DefineData.eAction.TUYENDUNGUNGVIEN.ToString(), model.HoTenDayDu + "(" + model.MaNhanVien + ")", model.IdGuid, "", JsonConvert.SerializeObject(model), model.IdCongTyEOS, db.currentUser.Id, db.currentUser.TenNhanVien);


                }
                if (model.lstKinhNghiemLamViec != null)
                {
                    foreach (var item in model.lstKinhNghiemLamViec)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdTuyenDungUngVien = model.IdGuid;
                    }
                    db.HRM_TuyenDung_UngVienKinhNghiemLamViec.AddRange(model.lstKinhNghiemLamViec);
                }
                var bangCaps = db.HRM_UngVienBangCap.Where(r => r.IdUngVien == model.IdGuid).ToList();
                var quanheGiadinh = db.HRM_UngVienQuanHeGiaDinh.Where(r => r.IdUngVien == model.IdGuid).ToList();
                db.HRM_UngVienBangCap.RemoveRange(bangCaps);
                db.HRM_UngVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                var NgonNgu = db.HRM_NgonNguUngVien.Where(r => r.IdUngVien == model.IdGuid).ToList();

                db.HRM_NgonNguUngVien.RemoveRange(NgonNgu);
                var ChungChi = db.HRM_ChungChiUngVien.Where(r => r.IdUngVien == model.IdGuid).ToList();

                db.HRM_ChungChiUngVien.RemoveRange(ChungChi);


                #region Bằng cấp
                if (model.ListUngVienBangCap.IsNotEmpty())
                {
                    foreach (var bangCap in model.ListUngVienBangCap)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdUngVien = model.IdGuid;
                        if (bangCap.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(db, bangCap.lstFileDinhKem, nameof(HRM_UngVienBangCap).ToString(), bangCap.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(, bangCap.IdGuid, nameof(HRM_NhanVienBangCap), DefineData.eFlagDinhKem.FileDinhKem));
                        db.HRM_UngVienBangCap.Add(bangCap);
                    }
                }
                #endregion
                #region Ngôn ngữ
                if (model.ListUngVienNgonNgu.IsNotEmpty())
                {
                    foreach (var bangCap in model.ListUngVienNgonNgu)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdUngVien = model.IdGuid;
                        if (bangCap.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(db, bangCap.lstFileDinhKem, nameof(HRM_NgonNguUngVien).ToString(), bangCap.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(, bangCap.IdGuid, nameof(HRM_NhanVienBangCap), DefineData.eFlagDinhKem.FileDinhKem));
                        db.HRM_NgonNguUngVien.Add(bangCap);
                    }
                }
                #endregion
                #region Chứng chỉ
                if (model.ListUngVienChungChi.IsNotEmpty())
                {
                    foreach (var bangCap in model.ListUngVienChungChi)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        if (bangCap.NgayHetHanUnix > 0)
                            bangCap.NgayHetHan = dungchung.UnixTimeStampToDateTime(bangCap.NgayHetHanUnix);
                        if (bangCap.NgayCapUnix > 0)
                            bangCap.NgayCap = dungchung.UnixTimeStampToDateTime(bangCap.NgayCapUnix);
                        bangCap.IdUngVien = model.IdGuid;
                        if (bangCap.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(db, bangCap.lstFileDinhKem, nameof(HRM_ChungChiUngVien).ToString(), bangCap.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(, bangCap.IdGuid, nameof(HRM_NhanVienBangCap), DefineData.eFlagDinhKem.FileDinhKem));
                        db.HRM_ChungChiUngVien.Add(bangCap);
                    }
                }
                #endregion

                #region Quan hệ gia đình
                if (model.ListQuanHeGiaDinhUngVien.IsNotEmpty())
                {
                    foreach (var giaDinh in model.ListQuanHeGiaDinhUngVien)
                    {
                        if (giaDinh.NgaySinhUnix > 0)
                            giaDinh.NgaySinh = dungchung.UnixTimeStampToDateTime(giaDinh.NgaySinhUnix);
                        if (string.IsNullOrEmpty(giaDinh.IdGuid))
                            giaDinh.IdGuid = Guid.NewGuid().ToString();
                        giaDinh.IdUngVien = model.IdGuid;
                        if (giaDinh.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(db, giaDinh.lstFileDinhKem, nameof(HRM_UngVienQuanHeGiaDinh).ToString(), giaDinh.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(giaDinh.lstFileDinhKemMoi, giaDinh.IdGuid, nameof(HRM_NhanVienQuanHeGiaDinh), DefineData.eFlagDinhKem.FileDinhKem));
                        db.HRM_UngVienQuanHeGiaDinh.Add(giaDinh);
                    }
                }
                #endregion
                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_TuyenDung_UngVien, model.IdGuid);
                    }
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public dynamic GetDanhMucChucDanhTheoDeNghiTuyenDung(string Id, ApplicationDbContext db)
        {
            var listId = db.HRM_TuyenDung_DeNghi_TongHopChiTiet.Where(r => r.IdQuyTrinhTongHop == Id).Select(r => r.IddmChucDanhEOS).ToList();
            var dmChucDanhss = _CallWebService.GetDanhSachChucDanhFromListId(db, listId).ToList();

            return dmChucDanhss;
        }
        public dynamic ImportDanhSachUngVien(ApplicationDbContext db, string FileName, string IdNhuCauChiTiet, bool isCongNhan = false)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            List<HRM_TuyenDung_UngVien> listResult = new List<HRM_TuyenDung_UngVien>();
            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            string Host = (HrmOptions?.Host ?? "");

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var sheet2 = package.Workbook.Worksheets["Kinh nghiệm làm việc"];
            var listColumnName = new List<string>();
            String[] ColumnNames = new String[] { "STT", "Mã đơn vị", "Đơn vị (*)", "Mã Bộ phận", "Bộ phận", "Mã nhân viên", "Họ Tên", "Ngày sinh", "Giới tính", "Số điện thoại", "Số CMT", "Mã số thuế theo CMT", "Mã số thuế theo CCCD", "CCCD", "CCCD Ngày cấp", "Mã nơi cấp", "CCCD nơi cấp" };
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            listColumnName.AddRange(ColumnNames);
            if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "DANH SÁCH ỨNG VIÊN", 2))
            {
                //err.SetLoi("File không đúng định dạng!" + err.);
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, err.message); ;
            }
            if (!dungchung.KiemTraSDTTrungExcel(sheet1, err,3,10))
            {
                //err.SetLoi("File không đúng định dạng!" + err.);
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, err.message); ;
            }

            var listQuocTich = db.HRM_dmQuocTich.ToList();
            var listTTHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var listDanToc = db.HRM_dmDanToc.ToList();
            var listTrinhDoHocVan = db.HRM_dmTrinhDoDaoTao.ToList();
            var listThanhPho = db.HRM_dmTinh.ToList();
            var listQuanHuyen = db.HRM_dmQuanHuyen.ToList();
            var listPhuongXa = db.HRM_dmPhuongXa.ToList();
            var listViTriTuyenDung = _CallWebService.GetDanhSachChucDanh(db);
            var listNguonTuyenDung = db.HRM_dmNguonTuyenDung.ToList();
            var listNoiCap = db.HRM_dmNoiCap.ToList();
            var listLoaiNhanVien = db.HRM_dmLoaiNhanVien.ToList();
            var listLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();

            HRM_DeNghiTuyenDung NhuCauChiTiet = new HRM_DeNghiTuyenDung();
            if (!string.IsNullOrEmpty(IdNhuCauChiTiet))
            {
                NhuCauChiTiet = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == IdNhuCauChiTiet);

            }
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listCongTy = _CallWebService.GetDanhSachDuAn(db);
            //var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();
            string sTT = "UV";
            DateTime dt = DateTime.Now;
            if (dt.Month < 10)
                sTT += "0" + dt.Month.ToString() + dt.Year.ToString() + "_";
            else
                sTT += dt.Month.ToString() + dt.Year.ToString() + "_";
            var ungVien = db.HRM_TuyenDung_UngVien.OrderByDescending(a => a.MaUngVien).FirstOrDefault();
            int nID = 1;

            if (ungVien != null)
            {
                nID = int.Parse(ungVien.MaUngVien.Split('_')[1].ToString() ?? "1") + 1;
            }
            List<string> listLoi = new List<string>();
            for (int row = 3; row <= sheet1.Cells.End.Row; row++)
            {

                int ncol = 2;
                HRM_TuyenDung_UngVien itemAdd = new HRM_TuyenDung_UngVien();
                itemAdd.lstKinhNghiemLamViec = new List<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
                itemAdd.IdGuid = Guid.NewGuid().ToString();
                itemAdd.isCongNhan = isCongNhan;
                if (!string.IsNullOrEmpty(IdNhuCauChiTiet))
                {

                    itemAdd.IdCongTyEOS = NhuCauChiTiet.IdCongTy;
                    itemAdd.IdCoSoTrungTuyen = NhuCauChiTiet.IdCongTy;
                    itemAdd.IdBoPhanEOS = NhuCauChiTiet.IdBoPhan;
                    itemAdd.IdChucVu = NhuCauChiTiet.IdChucVu;
                    if (NhuCauChiTiet != null)
                    {
                        itemAdd.IdNhuCauChiTiet = NhuCauChiTiet.IdGuid;
                        itemAdd.IdQuyTrinhNhuCau = NhuCauChiTiet.IdGuid;
                        itemAdd.IdTongHopDeNghiTuyenDung = NhuCauChiTiet.IdGuid;

                        itemAdd.DotTuyenDung = NhuCauChiTiet.LyDo;
                    }

                }
                var MaCongTy = dungchung.GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(MaCongTy))
                    break;
                if (!string.IsNullOrEmpty(MaCongTy))
                {
                    var BoPhan = listCongTy.FirstOrDefault(a => a.MaBoPhan == MaCongTy);
                    if (BoPhan != null)
                    {
                        itemAdd.IdCoSoTrungTuyen = BoPhan.Id;
                        itemAdd.IdCongTyEOS = BoPhan.Id;

                        if (!string.IsNullOrEmpty(IdNhuCauChiTiet) && BoPhan.Id != NhuCauChiTiet.IdCongTy)
                            listLoi.Add("dòng " + row + " Mã cơ sở không đúng với Nhu Cầu tuyển dụng: " + MaCongTy);
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã cơ sở không tồn tại: " + MaCongTy);
                }
                ncol++;
                ncol++;
                var MaBoPhan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaBoPhan))
                {
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == MaBoPhan);
                    if (BoPhan != null)
                        itemAdd.IdBoPhanEOS = BoPhan.Id;
                    else
                        listLoi.Add("dòng " + row + " Mã bộ phận không tồn tại: " + MaBoPhan);
                }

                ncol++;
                ncol++;
                itemAdd.MaNhanVien = dungchung.GetCell(sheet1, row, ncol);
                string ThuTu = nID.ToString();
                while (ThuTu.Length < 4)
                {
                    ThuTu = "0" + ThuTu;
                }
                itemAdd.MaUngVien = sTT + ThuTu;
                nID++;
                ncol++;
                itemAdd.HoTenDayDu = dungchung.GetCell(sheet1, row, ncol);
                if (itemAdd.HoTenDayDu.Length <= 2)
                    listLoi.Add("dòng " + row + " Nhập đầy đủ thông tin Họ và Tên ");
                ncol++;
                try
                {
                    DateTime? NgaySinh = dungchung.GetCellToDateTime(sheet1, row, ncol);
                    if (NgaySinh != null)
                        itemAdd.NgaySinh = NgaySinh.Value;
                }
                catch
                {
                    listLoi.Add("dòng " + row + " ngày sinh sai định dạng ");
                }
                ncol++;
                itemAdd.GioiTinh = dungchung.GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(itemAdd.GioiTinh))
                    listLoi.Add("dòng " + row + " Nhập đầy đủ thông tin giới tính ");
                ncol++;
                itemAdd.DienThoaiDiDong = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                itemAdd.SoCMT = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                itemAdd.MaSoThueCMT = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                itemAdd.MaSoThueCCCD = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                itemAdd.CCCD = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                try
                {
                    itemAdd.NgayCap = dungchung.GetCellToDateTime(sheet1, row, ncol);
                }
                catch
                {
                    listLoi.Add("dòng " + row + " Ngày cấp sai định dạng");
                }
                ncol++;
                string NoiCap = dungchung.GetCell(sheet1, row, ncol);
                itemAdd.NoiCap = listNoiCap.Where(a => a.Ma == NoiCap).Select(a => a.IdGuid).FirstOrDefault();
                ncol++;
                ncol++;
                itemAdd.NguyenQuan = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                itemAdd.HoKhauThuongTru = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                string maTTHonNhan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maTTHonNhan) && listTTHonNhan != null && listTTHonNhan.Count > 0)
                {
                    var TTHonNhan = listTTHonNhan.FirstOrDefault(a => a.Ma == maTTHonNhan);
                    if (TTHonNhan != null)
                    {
                        itemAdd.IddmTinhTrangHonNhan = TTHonNhan.IdGuid;
                        itemAdd.TinhTrangHonNhan = TTHonNhan;
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã tình trạng hôn nhân không đúng");
                }
                ncol++;
                ncol++;
                string maDanToc = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maDanToc) && listDanToc != null && listDanToc.Count > 0)
                {
                    var DanToc = listDanToc.FirstOrDefault(a => a.Ma == maDanToc);
                    if (DanToc != null)
                    {
                        itemAdd.IddmDanToc = DanToc.IdGuid;
                        itemAdd.DanToc = DanToc;
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã dân tộc không đúng");
                }
                ncol++;
                ncol++;
                string maTrinhDohocVan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maTrinhDohocVan) && listTrinhDoHocVan != null && listTrinhDoHocVan.Count > 0)
                {
                    var TrinhDohocVan = listTrinhDoHocVan.FirstOrDefault(a => a.Ma == maTrinhDohocVan);
                    if (TrinhDohocVan != null)
                    {
                        itemAdd.IddmTrinhDoHocVan = TrinhDohocVan.IdGuid;
                        itemAdd.TendmTrinhDoHocVan = TrinhDohocVan.Ten;
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã trình độ học vấn không tồn tại: " + maTrinhDohocVan);
                }
                ncol++;
                ncol++;
                itemAdd.ChuyenNganh = dungchung.GetCell(sheet1, row, ncol);

                ncol++;
                itemAdd.DiaChi = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                itemAdd.Email = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                string maNguonTuyenDung = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maNguonTuyenDung) && listNguonTuyenDung != null && listNguonTuyenDung.Count > 0)
                {
                    var NguonTuyenDung = listNguonTuyenDung.FirstOrDefault(a => a.Ma == maNguonTuyenDung);
                    if (NguonTuyenDung != null)
                    {
                        itemAdd.IddmNguonTuyenDung = NguonTuyenDung.IdGuid;
                        itemAdd.TendmNguonTuyenDung = NguonTuyenDung.Ten;
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã nguồn tuyển dụng không đúng");
                }
                ncol++;
                ncol++;
                try
                {
                    itemAdd.NgayTiepNhan = dungchung.GetCellToDateTime(sheet1, row, ncol);
                }
                catch
                {
                    listLoi.Add("dòng " + row + " Ngày tiếp nhận sau định dạng");
                }
                if (isCongNhan == true)
                    itemAdd.NgayNhanViecDuKien = itemAdd.NgayTiepNhan;
                ncol++;
                string maLoaiNhanVien = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maLoaiNhanVien) && listLoaiNhanVien != null && listLoaiNhanVien.Count > 0)
                {
                    var LoaiNhanVien = listLoaiNhanVien.FirstOrDefault(a => a.Ma == maLoaiNhanVien);
                    if (LoaiNhanVien != null)
                    {
                        itemAdd.IdLoaiNhanVien = LoaiNhanVien.IdGuid;
                        itemAdd.TenLoaiNhanVien = LoaiNhanVien.Ten;
                    }
                    else listLoi.Add("dòng " + row + " Mã loại nhân viên không đúng");
                }
                ncol++;
                ncol++;
                string maNhanVien = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maNhanVien) && listNhanVien != null && listNhanVien.Count() > 0)
                {
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == maNhanVien);
                    if (NhanVien != null)
                    {
                        itemAdd.IdNhanVienGioiThieu = NhanVien.IdGuid;
                        //itemAdd.ngu = NhanVien.Ten;
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã nhân viên giới thiệu không đúng");
                }
                ncol++;
                string maChucVu = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maChucVu) && listChucVu != null && listChucVu.Count > 0)
                {
                    var ChucVu = listChucVu.FirstOrDefault(a => a.MaChucVu == maChucVu);
                    if (ChucVu != null)
                    {
                        itemAdd.IdChucVu = ChucVu.Id;
                        //itemAdd.ngu = ChucVu.Ten;
                    }
                    else
                        listLoi.Add("dòng " + row + " Mã chức vụ không đúng");
                }
                ncol++;
                ncol++;
                string maLoaiHopDong = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maLoaiHopDong) && listLoaiHopDong != null && listLoaiHopDong.Count > 0)
                {
                    var LoaiHopDong = listLoaiHopDong.FirstOrDefault(a => a.Ma == maLoaiHopDong);
                    if (LoaiHopDong != null)
                    {
                        itemAdd.IdLoaiHopDong = LoaiHopDong.IdGuid;
                        //itemAdd.ngu = LoaiHopDong.Ten;
                    }
                    else listLoi.Add("dòng " + row + " Mã loại hợp đồng không đúng");
                }
                ncol++;
                ncol++;
                string HinhThucTraLuongDaoTao = dungchung.GetCell(sheet1, row, ncol);
                if (HinhThucTraLuongDaoTao != "#N/A")
                    itemAdd.HinhThucTraLuongDaoTao = dungchung.GetCellToInt(sheet1, row, ncol);
                ncol++;
                ncol++;
                itemAdd.MucLuongUngVienDeNghi = dungchung.GetCellToDouble(sheet1, row, ncol);
                ncol++;
                itemAdd.ThuCoc = dungchung.GetCellToDouble(sheet1, row, ncol);



                itemAdd.TrangThai = 0;
                if (Host == "ICT")
                    itemAdd.TrangThai = 2;
                listResult.Add(itemAdd);
            }

            if (listResult.Count > 0)
            {
                List<string> listSDT = listResult.Where(a => !string.IsNullOrEmpty(a.DienThoaiDiDong)).Select(a => a.DienThoaiDiDong).Distinct().ToList();
                var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => listSDT.Contains(a.DienThoaiDiDong)).ToList();
                if (listUngVien != null && listUngVien.Count > 0)
                {
                    List<string> listIdUngVien = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdUngVien.Contains(a.IdUngVien) && a.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString()).ToList();
                    if (listNhanVien == null || listNhanVien.Count == 0)
                        listLoi.Add("Số điện thoại đã tồn tại : " + string.Join(", ", listUngVien.Select(a => a.DienThoaiDiDong).Distinct().ToList()));
                    else
                    {
                        List<string> listIdUngVienTrung = listNhanVien.Select(a => a.IdUngVien).Distinct().ToList();
                        listUngVien = listUngVien.Where(a => !listIdUngVienTrung.Contains(a.IdGuid)).ToList();
                        if (listUngVien != null && listUngVien.Count > 0)
                            listLoi.Add("Số điện thoại đã tồn tại : " + string.Join(", ", listUngVien.Select(a => a.DienThoaiDiDong).Distinct().ToList()));

                    }
                }
            }
            if (listLoi.Count > 0)
            {
                var sheetLoi = package.Workbook.Worksheets.Add("Lỗi");
                int i = 1;
                foreach (var item in listLoi)
                {
                    sheetLoi.Cells[i, 1].Value = item;
                    sheetLoi.Cells[i, 1, i, 20].Merge = true;
                    i++;
                }
                var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                string pathresult = PathUploads + filenamegui;
                File.WriteAllBytes(pathresult, package.GetAsByteArray());
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Import Không thành công", "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(FileName))
                            + "&path=" + dungchung.Base64Encode(pathresult));
            }
            List<HRM_TuyenDung_UngVienKinhNghiemLamViec> listKinhNghiem = new List<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
            for (int row = 2; row <= sheet2.Cells.End.Row; row++)
            {
                HRM_TuyenDung_UngVienKinhNghiemLamViec itemAdd = new HRM_TuyenDung_UngVienKinhNghiemLamViec();
                itemAdd.IdGuid = Guid.NewGuid().ToString();
                int ncol = 2;
                itemAdd.DienThoaiDiDong = dungchung.GetCell(sheet2, row, ncol);
                if (string.IsNullOrEmpty(itemAdd.DienThoaiDiDong))
                    break;
                var UngVien = listResult.FirstOrDefault(a => a.DienThoaiDiDong == itemAdd.DienThoaiDiDong);
                if (UngVien != null)
                    itemAdd.IdTuyenDungUngVien = UngVien.IdGuid;
                ncol++;
                itemAdd.ThoiGianLamViec = dungchung.GetCell(sheet2, row, ncol);
                ncol++;
                itemAdd.DonViCongTac = dungchung.GetCell(sheet2, row, ncol);
                ncol++;
                itemAdd.ViTri = dungchung.GetCell(sheet2, row, ncol);
                listKinhNghiem.Add(itemAdd);
            }
            listKinhNghiem = listKinhNghiem.Where(a => !string.IsNullOrEmpty(a.IdTuyenDungUngVien)).ToList();
            if (listKinhNghiem != null && listKinhNghiem.Count > 0)
                db.HRM_TuyenDung_UngVienKinhNghiemLamViec.AddRange(listKinhNghiem);
            if (string.IsNullOrEmpty(IdNhuCauChiTiet))
            {
                List<long> listIdCongTy = listResult.Select(a => a.IdCongTyEOS ?? 0).Distinct().ToList();
                var NhuCaus = db.HRM_DeNghiTuyenDung.Where(a => listIdCongTy.Contains(a.IdCongTy) && a.isKetThuc == true).OrderByDescending(a => a.Modified).ToList();
                foreach (var item in listResult)
                {
                    var NhuCauCty = NhuCaus.Where(a => a.IdCongTy == item.IdCongTyEOS && ((item.IdBoPhanEOS == null && a.IdBoPhan == 0) || a.IdBoPhan == item.IdBoPhanEOS)).FirstOrDefault();
                    if (NhuCauCty == null)
                        NhuCauCty = NhuCaus.Where(a => a.IdCongTy == item.IdCongTyEOS).FirstOrDefault();
                    if (NhuCauCty != null)
                    {
                        item.IdNhuCauChiTiet = NhuCauCty.IdGuid;
                        item.IdQuyTrinhNhuCau = NhuCauCty.IdGuid;
                        item.DotTuyenDung = NhuCauCty.LyDo;
                    }
                }
            }
            if (listResult != null && listResult.Count > 0)
                db.HRM_TuyenDung_UngVien.AddRange(listResult);
            db.SaveChanges();
            foreach (var item in listResult)
            {
                item.lstKinhNghiemLamViec = new List<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
                item.lstKinhNghiemLamViec = listKinhNghiem.Where(a => a.IdTuyenDungUngVien == item.IdGuid).ToList();
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", listResult);
        }
        public dynamic ImportDanhSachUngVienOld(ApplicationDbContext db, string FileName, string IdNhuCauChiTiet, bool isCongNhan = false)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            List<HRM_TuyenDung_UngVien> listResult = new List<HRM_TuyenDung_UngVien>();
            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            string Host = (HrmOptions?.Host ?? "");

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var sheet2 = package.Workbook.Worksheets["Kinh nghiệm làm việc"];
            var listColumnName = new List<string>();
            String[] ColumnNames = new String[] { "STT", "Điện thoại", "Mã công ty", "Mã bộ phận", "Mã vị trí ứng tuyển", "Mã nhân viên", "Họ", "Tên đệm", "Tên", "Giới tính", "Ngày sinh", "Quốc tịch", "CCCD", "CCCD Ngày cấp", "CCCD nơi cấp", "Hộ khẩu thường trú", "TT hôn nhân", "Dân tộc", "Trình độ học vấn", "Chuyên ngành", "Chổ ở hiện tại", "Tỉnh/Thành phố", "Quận/Huyện", "Phường/Xã", "Email", "Nguồn tuyển dụng" };
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            listColumnName.AddRange(ColumnNames);
            if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "DANH SÁCH ỨNG VIÊN", 2))
            {
                //err.SetLoi("File không đúng định dạng!" + err.);
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, err.message); ;
            }

            var listQuocTich = db.HRM_dmQuocTich.ToList();
            var listTTHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var listDanToc = db.HRM_dmDanToc.ToList();
            var listTrinhDoHocVan = db.HRM_dmTrinhDoDaoTao.ToList();
            var listThanhPho = db.HRM_dmTinh.ToList();
            var listQuanHuyen = db.HRM_dmQuanHuyen.ToList();
            var listPhuongXa = db.HRM_dmPhuongXa.ToList();
            var listViTriTuyenDung = _CallWebService.GetDanhSachChucDanh(db);
            var listNguonTuyenDung = db.HRM_dmNguonTuyenDung.ToList();
            var listNoiCap = db.HRM_dmNoiCap.ToList();

            HRM_DeNghiTuyenDung NhuCauChiTiet = new HRM_DeNghiTuyenDung();
            if (!string.IsNullOrEmpty(IdNhuCauChiTiet))
            {
                NhuCauChiTiet = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == IdNhuCauChiTiet);

            }
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listCongTy = _CallWebService.GetDanhSachDuAn(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            string sTT = "UV";
            DateTime dt = DateTime.Now;
            if (dt.Month < 10)
                sTT += "0" + dt.Month.ToString() + dt.Year.ToString() + "_";
            else
                sTT += dt.Month.ToString() + dt.Year.ToString() + "_";
            var ungVien = db.HRM_TuyenDung_UngVien.OrderByDescending(a => a.MaUngVien).FirstOrDefault();
            int nID = 1;

            if (ungVien != null)
            {
                nID = int.Parse(ungVien.MaUngVien.Split('_')[1].ToString() ?? "1") + 1;
            }

            for (int row = 3; row <= sheet1.Cells.End.Row; row++)
            {
                int ncol = 2;
                HRM_TuyenDung_UngVien itemAdd = new HRM_TuyenDung_UngVien();
                itemAdd.lstKinhNghiemLamViec = new List<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
                itemAdd.IdGuid = Guid.NewGuid().ToString();
                itemAdd.Ten = dungchung.GetCell(sheet1, row, 9);
                itemAdd.DienThoaiDiDong = dungchung.GetCell(sheet1, row, ncol);
                itemAdd.isCongNhan = isCongNhan;

                if (string.IsNullOrEmpty(itemAdd.Ten))
                    break;

                if (!string.IsNullOrEmpty(IdNhuCauChiTiet))
                {

                    itemAdd.IdCongTyEOS = NhuCauChiTiet.IdCongTy;
                    itemAdd.IdBoPhanEOS = NhuCauChiTiet.IdBoPhan;
                    itemAdd.IdChucVu = NhuCauChiTiet.IdChucVu;
                    if (NhuCauChiTiet != null)
                    {
                        itemAdd.IdNhuCauChiTiet = NhuCauChiTiet.IdGuid;
                        itemAdd.IdQuyTrinhNhuCau = NhuCauChiTiet.IdGuid;

                        itemAdd.DotTuyenDung = NhuCauChiTiet.LyDo;
                    }

                }

                ncol++;
                var MaCongTy = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaCongTy))
                {
                    var BoPhan = listCongTy.FirstOrDefault(a => a.MaBoPhan == MaCongTy);
                    if (BoPhan != null)
                        itemAdd.IdCongTyEOS = BoPhan.Id;
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã cơ sở không tồn tại: " + MaCongTy);
                }
                ncol++;
                var MaBoPhan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaBoPhan))
                {
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == MaBoPhan);
                    if (BoPhan != null)
                        itemAdd.IdBoPhanEOS = BoPhan.Id;
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã bộ phận không tồn tại: " + MaBoPhan);
                }

                ncol++;
                var MaChucDanh = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucDanh))
                {
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == MaChucDanh);
                    if (ChucDanh != null)
                    {
                        itemAdd.IddmChucDanhUngTuyen = ChucDanh.Id;
                        itemAdd.TenChucDanhUngTuyen = ChucDanh.Ten;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã vị trí ứng tuyển không tồn tại: " + MaChucDanh);
                }
                ncol++;
                itemAdd.MaNhanVien = dungchung.GetCell(sheet1, row, ncol);


                string ThuTu = nID.ToString();
                while (ThuTu.Length < 4)
                {
                    ThuTu = "0" + ThuTu;
                }
                itemAdd.MaUngVien = sTT + ThuTu;
                nID++;
                ncol++;


                itemAdd.HoTenDayDu = dungchung.GetCell(sheet1, row, ncol);
                if (itemAdd.HoTenDayDu.Length <= 2)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Nhập đầy đủ thông tin Họ và Tên ");

                ncol++;
                itemAdd.GioiTinh = dungchung.GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(itemAdd.GioiTinh))
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Nhập đầy đủ thông tin giới tính ");
                ncol++;
                DateTime? NgaySinh = dungchung.GetCellToDateTime(sheet1, row, ncol);
                if (NgaySinh != null)
                    itemAdd.NgaySinh = NgaySinh.Value;
                ncol++;
                string maQuocTich = dungchung.GetCell(sheet1, row, ncol);
                if (maQuocTich != null && listQuocTich != null && listQuocTich.Count > 0)
                {
                    var QuocTich = listQuocTich.FirstOrDefault(a => a.Ma == maQuocTich);
                    if (QuocTich != null)
                    {
                        itemAdd.IddmQuocTich = QuocTich.IdGuid;
                        itemAdd.QuocTich = QuocTich;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã quốc tịch không tồn tại: " + maQuocTich);
                }
                else
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Nhập đầy đủ thông tin mã quốc tịch  ");
                ncol++;
                itemAdd.CCCD = dungchung.GetCell(sheet1, row, ncol);
                itemAdd.NgayCap = dungchung.GetCellToDateTime(sheet1, row, ncol);
                string NoiCap = dungchung.GetCell(sheet1, row, ncol);
                itemAdd.NoiCap = listNoiCap.Where(a => a.Ma == NoiCap).Select(a => a.IdGuid).FirstOrDefault();
                ncol++;
                itemAdd.HoKhauThuongTru = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                string maTTHonNhan = dungchung.GetCell(sheet1, row, ncol);
                if (maTTHonNhan != null && listTTHonNhan != null && listTTHonNhan.Count > 0)
                {
                    var TTHonNhan = listTTHonNhan.FirstOrDefault(a => a.Ma == maTTHonNhan);
                    if (TTHonNhan != null)
                    {
                        itemAdd.IddmTinhTrangHonNhan = TTHonNhan.IdGuid;
                        itemAdd.TinhTrangHonNhan = TTHonNhan;
                    }
                }
                ncol++;
                string maDanToc = dungchung.GetCell(sheet1, row, ncol);
                if (maDanToc != null && listDanToc != null && listDanToc.Count > 0)
                {
                    var DanToc = listDanToc.FirstOrDefault(a => a.Ma == maDanToc);
                    if (DanToc != null)
                    {
                        itemAdd.IddmDanToc = DanToc.IdGuid;
                        itemAdd.DanToc = DanToc;
                    }
                }
                //ncol++;
                //itemAdd.DienThoaiDiDong = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                string maTrinhDohocVan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(maTrinhDohocVan) && listTrinhDoHocVan != null && listTrinhDoHocVan.Count > 0)
                {
                    var TrinhDohocVan = listTrinhDoHocVan.FirstOrDefault(a => a.Ma == maTrinhDohocVan);
                    if (TrinhDohocVan != null)
                    {
                        itemAdd.IddmTrinhDoHocVan = TrinhDohocVan.IdGuid;
                        itemAdd.TendmTrinhDoHocVan = TrinhDohocVan.Ten;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã trình độ học vấn không tồn tại: " + maTrinhDohocVan);
                }
                ncol++;
                itemAdd.ChuyenNganh = dungchung.GetCell(sheet1, row, ncol);

                ncol++;
                itemAdd.DiaChi = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                string maThanhPho = dungchung.GetCell(sheet1, row, ncol);
                if (maThanhPho != null && listThanhPho != null && listThanhPho.Count > 0)
                {
                    var ThanhPho = listThanhPho.FirstOrDefault(a => a.Ma == maThanhPho);
                    if (ThanhPho != null)
                    {
                        itemAdd.IddmTinhThanhPho = ThanhPho.IdGuid;
                        itemAdd.TinhThanhPho = ThanhPho;
                    }
                }
                ncol++;
                string maQuanHuyen = dungchung.GetCell(sheet1, row, ncol);
                if (maQuanHuyen != null && listQuanHuyen != null && listQuanHuyen.Count > 0)
                {
                    var QuanHuyen = listQuanHuyen.FirstOrDefault(a => a.Ma == maQuanHuyen);
                    if (QuanHuyen != null)
                    {
                        itemAdd.IddmQuanHuyen = QuanHuyen.IdGuid;
                        itemAdd.QuanHuyen = QuanHuyen;
                    }
                }
                ncol++;
                string maPhuongXa = dungchung.GetCell(sheet1, row, ncol);
                if (maPhuongXa != null && listPhuongXa != null && listPhuongXa.Count > 0)
                {
                    var PhuongXa = listPhuongXa.FirstOrDefault(a => a.Ma == maPhuongXa);
                    if (PhuongXa != null)
                    {
                        itemAdd.IddmPhuongXa = PhuongXa.IdGuid;
                        itemAdd.PhuongXa = PhuongXa;
                    }
                }
                ncol++;
                itemAdd.Email = dungchung.GetCell(sheet1, row, ncol);
                ncol++;
                string maNguonTuyenDung = dungchung.GetCell(sheet1, row, ncol);
                if (maNguonTuyenDung != null && listNguonTuyenDung != null && listNguonTuyenDung.Count > 0)
                {
                    var NguonTuyenDung = listNguonTuyenDung.FirstOrDefault(a => a.Ma == maNguonTuyenDung);
                    if (NguonTuyenDung != null)
                    {
                        itemAdd.IddmNguonTuyenDung = NguonTuyenDung.IdGuid;
                        itemAdd.TendmNguonTuyenDung = NguonTuyenDung.Ten;
                    }
                }
                ncol++;
                itemAdd.NgayTiepNhan = dungchung.GetCellToDateTime(sheet1, row, ncol);

                itemAdd.TrangThai = 0;
                if (Host == "ICT")
                    itemAdd.TrangThai = 2;
                listResult.Add(itemAdd);
            }
            if (listResult.Count > 0)
            {
                List<string> listSDT = listResult.Select(a => a.DienThoaiDiDong).Distinct().ToList();
                var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => listSDT.Contains(a.DienThoaiDiDong)).ToList();
                if (listUngVien != null && listUngVien.Count > 0)
                {
                    List<string> listIdUngVien = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdUngVien.Contains(a.IdUngVien) && a.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString()).ToList();
                    if (listNhanVien == null || listNhanVien.Count == 0)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số điện thoại đã tồn tại : " + string.Join(", ", listUngVien.Select(a => a.DienThoaiDiDong).Distinct().ToList()));
                    else
                    {
                        List<string> listIdUngVienTrung = listNhanVien.Select(a => a.IdUngVien).Distinct().ToList();
                        listUngVien = listUngVien.Where(a => !listIdUngVienTrung.Contains(a.IdGuid)).ToList();
                        if (listUngVien != null && listUngVien.Count > 0)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số điện thoại đã tồn tại : " + string.Join(", ", listUngVien.Select(a => a.DienThoaiDiDong).Distinct().ToList()));

                    }
                }
            }

            List<HRM_TuyenDung_UngVienKinhNghiemLamViec> listKinhNghiem = new List<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
            for (int row = 2; row <= sheet2.Cells.End.Row; row++)
            {
                HRM_TuyenDung_UngVienKinhNghiemLamViec itemAdd = new HRM_TuyenDung_UngVienKinhNghiemLamViec();
                itemAdd.IdGuid = Guid.NewGuid().ToString();
                int ncol = 2;
                itemAdd.DienThoaiDiDong = dungchung.GetCell(sheet2, row, ncol);
                if (string.IsNullOrEmpty(itemAdd.DienThoaiDiDong))
                    break;
                var UngVien = listResult.FirstOrDefault(a => a.DienThoaiDiDong == itemAdd.DienThoaiDiDong);
                if (UngVien != null)
                    itemAdd.IdTuyenDungUngVien = UngVien.IdGuid;
                ncol++;
                itemAdd.ThoiGianLamViec = dungchung.GetCell(sheet2, row, ncol);
                ncol++;
                itemAdd.DonViCongTac = dungchung.GetCell(sheet2, row, ncol);
                ncol++;
                itemAdd.ViTri = dungchung.GetCell(sheet2, row, ncol);
                listKinhNghiem.Add(itemAdd);
            }
            listKinhNghiem = listKinhNghiem.Where(a => !string.IsNullOrEmpty(a.IdTuyenDungUngVien)).ToList();
            if (listKinhNghiem != null && listKinhNghiem.Count > 0)
                db.HRM_TuyenDung_UngVienKinhNghiemLamViec.AddRange(listKinhNghiem);
            if (listResult != null && listResult.Count > 0)
                db.HRM_TuyenDung_UngVien.AddRange(listResult);
            db.SaveChanges();
            foreach (var item in listResult)
            {
                item.lstKinhNghiemLamViec = new List<HRM_TuyenDung_UngVienKinhNghiemLamViec>();
                item.lstKinhNghiemLamViec = listKinhNghiem.Where(a => a.IdTuyenDungUngVien == item.IdGuid).ToList();
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", listResult);
        }
        public dynamic ImportDanhSachNhanVien(ApplicationDbContext db, string FileName)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            List<HRM_NhanVien> listResult = new List<HRM_NhanVien>();
            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            DateTime dt = DateTime.Now;

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();


            var listQuocTich = db.HRM_dmQuocTich.ToList();
            var listTTHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var listDanToc = db.HRM_dmDanToc.ToList();
            var listTrinhDoHocVan = db.HRM_dmTrinhDoDaoTao.ToList();
            var listThanhPho = db.HRM_dmTinh.ToList();
            var listQuanHuyen = db.HRM_dmQuanHuyen.ToList();
            var listPhuongXa = db.HRM_dmPhuongXa.ToList();

            var listNguonTuyenDung = db.HRM_dmNguonTuyenDung.ToList();
            var listCalamViec = db.HRM_dmLichLamViec.ToList();
            var listQuanHeGiaDinh = db.HRM_dmQuanHeGiaDinh.ToList();



            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);

            //db.HRM_NhanVienQuanHeGiaDinh.RemoveRange(olddb);
            //db.SaveChanges();
            //var listnhanVien = db.HRM_NhanVien.ToList();
            List<string> listCheckMaTrung = new List<string>();
            List<HRM_NhanVienQuanHeGiaDinh> listQuanHeGiaDinhAdd = new List<HRM_NhanVienQuanHeGiaDinh>();
            for (int row = 7; row <= sheet1.Cells.End.Row; row++)
            {
                int ncol = 2;
                var MaNhanVien = dungchung.GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(MaNhanVien))
                    break;
                if (listCheckMaTrung.Contains(MaNhanVien))
                    continue;
                else
                    listCheckMaTrung.Add(MaNhanVien);


                HRM_NhanVien itemAdd = new HRM_NhanVien();
                itemAdd.IdGuid = Guid.NewGuid().ToString();
                itemAdd.MaNhanVien = MaNhanVien;
                //if (itemAdd == null)
                //    continue;
                #region
                ncol++;//3
                itemAdd.TenDayDu = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//4
                itemAdd.Ho = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//5
                itemAdd.TenDem = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//6
                itemAdd.Ten = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//7
                itemAdd.NgayBatDauLamViec = dungchung.GetCellToDateTime(sheet1, row, ncol);
                if (itemAdd.NgayBatDauLamViec != null)
                    itemAdd.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(itemAdd.NgayBatDauLamViec);
                ncol++;//8
                string MaCaLamViec = dungchung.GetCell(sheet1, row, ncol);
                var CaLamViec = listCalamViec.FirstOrDefault(a => a.Ma == MaCaLamViec);
                if (CaLamViec != null)
                    itemAdd.IdCaLamViec = CaLamViec.IdGuid;
                ncol++;//9
                string MaChucVu = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucVu) && !MaChucVu.Contains("#N/A"))
                {
                    var ChucVu = listChucVu.FirstOrDefault(a => a.MaChucVu == MaChucVu);
                    if (ChucVu != null)
                        itemAdd.IdChucVu = ChucVu.Id;
                }
                ncol++;//10
                ncol++;//11
                string MaChucDanh = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucDanh) && !MaChucDanh.Contains("#N/A"))
                {
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == MaChucDanh);
                    if (ChucDanh != null)
                        itemAdd.IdChucDanh = ChucDanh.Id;
                }

                ncol++;//12
                ncol++;//13
                itemAdd.ChucDanhLuuHanhNoiBo = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//14
                var MaCongTy = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaCongTy))
                {
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == MaCongTy);
                    if (BoPhan != null)
                        itemAdd.IdCongTyEOS = BoPhan.Id;
                }
                ncol += 3;//17
                var MaBoPhan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaBoPhan))
                {
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == MaBoPhan);
                    if (BoPhan != null)
                        itemAdd.IdBoPhanEOS = BoPhan.Id;
                }
                ncol++;//18
                ncol++;//19
                itemAdd.TrangThai = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//20
                ncol++;//21
                if (itemAdd.TrangThai == "DaNghiViec")
                {
                    itemAdd.NgayNghiViec = dungchung.GetCellToDateTime(sheet1, row, ncol);
                    if (itemAdd.NgayNghiViec != null)
                        itemAdd.NgayNghiViecUnix = dungchung.DateTimeToUnixTimeStamp(itemAdd.NgayNghiViec);
                }
                ncol++;//22
                itemAdd.LyDoNghiViec = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//23
                itemAdd.NgaySinh = dungchung.GetCellToDateTime(sheet1, row, ncol) ?? dt;
                if (itemAdd.NgaySinh != null)
                    itemAdd.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(itemAdd.NgaySinh);
                ncol++;//24
                itemAdd.NoiSinh = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//25
                itemAdd.IdGioiTinh = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//26
                       //itemAdd.SoChungMinhThu = dungchung.GetCell(sheet1, row, ncol);
                       //ncol++;//27
                       ////string ab = dungchung.GetCell(sheet1, row, ncol);
                       ////if (!string.IsNullOrEmpty(ab) && ab.Contains("/"))
                       ////    ab = ab;
                       //itemAdd.ChungMinhThuNgayCap = dungchung.GetCellToDateTime(sheet1, row, ncol);
                       //if (itemAdd.ChungMinhThuNgayCap != null)
                       //    itemAdd.ChungMinhThuNgayCapUnix = dungchung.DateTimeToUnixTimeStamp(itemAdd.ChungMinhThuNgayCap);
                       //ncol++;//28
                       //itemAdd.ChungMinhThuNoiCap = dungchung.GetCell(sheet1, row, ncol);
                       //ncol++;//29
                       //itemAdd.SoCanCuoc = dungchung.GetCell(sheet1, row, ncol);
                       //ncol++;//30
                       //itemAdd.CanCuocNgayCap = dungchung.GetCellToDateTime(sheet1, row, ncol);
                       //if (itemAdd.CanCuocNgayCap != null)
                       //    itemAdd.CanCuocNgayCapUnix = dungchung.DateTimeToUnixTimeStamp(itemAdd.CanCuocNgayCap);
                       //ncol++;//31
                       //itemAdd.CanCuocNoiCap = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//32
                itemAdd.MaSoThueTNCN = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//33
                itemAdd.NgayCapMaSTCN = dungchung.GetCellToDateTime(sheet1, row, ncol);
                if (itemAdd.NgayCapMaSTCN != null)
                    itemAdd.NgayCapMaSTCNUnix = dungchung.DateTimeToUnixTimeStamp(itemAdd.NgayCapMaSTCN);
                ncol++;//34
                #endregion
                ncol = 37;
                itemAdd.DienThoai = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//38
                itemAdd.Email = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//39
                string MaTrinhDoChuyenMon = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaTrinhDoChuyenMon) && !MaTrinhDoChuyenMon.Contains("#N/A"))
                {
                    var TrinhDo = listTrinhDoHocVan.FirstOrDefault(a => a.Ma == MaTrinhDoChuyenMon);
                    if (TrinhDo != null)
                    {
                        itemAdd.IddmTrinhDo = TrinhDo.IdGuid;
                        itemAdd.TrinhDoHocVan = TrinhDo.IdGuid;
                    }
                }

                ncol++;//40
                ncol++;//41
                itemAdd.NoiDaoTao = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//42
                itemAdd.NganhHoc = dungchung.GetCell(sheet1, row, ncol);

                ncol++;//43

                ncol++;//44
                itemAdd.HoKhauThuongTruDiaChi = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//45
                itemAdd.HoKhauThuongTruThonXom = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//46
                string HoKhauThuongTruIdPhuongXa = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(HoKhauThuongTruIdPhuongXa) && !HoKhauThuongTruIdPhuongXa.Contains("#N/A"))
                {
                    var PhuongXa = listPhuongXa.FirstOrDefault(a => a.Ma == HoKhauThuongTruIdPhuongXa);
                    if (PhuongXa != null)
                    {
                        itemAdd.HoKhauThuongTruIdPhuongXa = PhuongXa.IdGuid;

                    }
                }
                ncol++;//47
                ncol++;//48

                string HoKhauThuongTruIdQuanHuyen = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(HoKhauThuongTruIdQuanHuyen) && !HoKhauThuongTruIdQuanHuyen.Contains("#N/A"))
                {
                    var QuanHuyen = listQuanHuyen.FirstOrDefault(a => a.Ma == HoKhauThuongTruIdQuanHuyen);
                    if (QuanHuyen != null)
                    {
                        itemAdd.HoKhauThuongTruIdQuanHuyen = QuanHuyen.IdGuid;

                    }
                }
                ncol++;//49
                ncol++;//50

                string HoKhauThuongTruIdTinhThanhPho = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(HoKhauThuongTruIdTinhThanhPho) && !HoKhauThuongTruIdTinhThanhPho.Contains("#N/A"))
                {
                    var TinhThanhPho = listThanhPho.FirstOrDefault(a => a.Ma == HoKhauThuongTruIdTinhThanhPho);
                    if (TinhThanhPho != null)
                    {
                        itemAdd.HoKhauThuongTruIdTinhThanhPho = TinhThanhPho.IdGuid;

                    }
                }
                ncol++;//51
                ncol++;//52
                itemAdd.ChoOHienNayTruDiaChi = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//53
                itemAdd.ChoOHienNayTruThonXom = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//54
                string ChoOHienNayTruIdPhuongXa = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(ChoOHienNayTruIdPhuongXa) && !ChoOHienNayTruIdPhuongXa.Contains("#N/A"))
                {
                    var PhuongXa = listPhuongXa.FirstOrDefault(a => a.Ma == ChoOHienNayTruIdPhuongXa);
                    if (PhuongXa != null)
                    {
                        itemAdd.ChoOHienNayTruIdPhuongXa = PhuongXa.IdGuid;

                    }
                }
                ncol++;//55
                ncol++;//56
                string ChoOHienNayTruIdQuanHuyen = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(ChoOHienNayTruIdQuanHuyen) && !ChoOHienNayTruIdQuanHuyen.Contains("#N/A"))
                {
                    var QuanHuyen = listQuanHuyen.FirstOrDefault(a => a.Ma == ChoOHienNayTruIdQuanHuyen);
                    if (QuanHuyen != null)
                    {
                        itemAdd.ChoOHienNayTruIdQuanHuyen = QuanHuyen.IdGuid;

                    }
                }
                ncol++;//57
                ncol++;//58
                string ChoOHienNayTruIdTinhThanhPho = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(ChoOHienNayTruIdTinhThanhPho) && !ChoOHienNayTruIdTinhThanhPho.Contains("#N/A"))
                {
                    var TinhThanhPho = listThanhPho.FirstOrDefault(a => a.Ma == ChoOHienNayTruIdTinhThanhPho);
                    if (TinhThanhPho != null)
                    {
                        itemAdd.ChoOHienNayTruIdTinhThanhPho = TinhThanhPho.IdGuid;

                    }
                }
                ncol++;//59
                ncol++;//60
                itemAdd.NguyenQuan = dungchung.GetCell(sheet1, row, ncol);

                ncol++;//61
                string maHonNhan = dungchung.GetCell(sheet1, row, ncol);
                if (maHonNhan != null && !maHonNhan.Contains("#N/A") && listTTHonNhan != null && listTTHonNhan.Count > 0)
                {
                    var HonNhan = listTTHonNhan.FirstOrDefault(a => a.Ma == maHonNhan);
                    if (HonNhan != null)
                    {
                        itemAdd.IdTinhTrangHonNhan = HonNhan.IdGuid;

                    }
                }
                ncol++;//62
                ncol++;//63
                string maDanToc = dungchung.GetCell(sheet1, row, ncol);
                if (maDanToc != null && listDanToc != null && listDanToc.Count > 0)
                {
                    var DanToc = listDanToc.FirstOrDefault(a => a.Ma == maDanToc);
                    if (DanToc != null)
                    {
                        itemAdd.IdDanToc = DanToc.IdGuid;
                    }
                }
                ncol++;//64
                ncol++;//65
                string maQuocTich = dungchung.GetCell(sheet1, row, ncol);
                if (maQuocTich != null && listQuocTich != null && listQuocTich.Count > 0)
                {
                    var QuocTich = listQuocTich.FirstOrDefault(a => a.Ma == maQuocTich);
                    if (QuocTich != null)
                    {
                        itemAdd.IdQuocTich = QuocTich.IdGuid;
                    }
                }
                ncol++;//66
                ncol++;//67
                itemAdd.TonGiao = dungchung.GetCell(sheet1, row, ncol);
                itemAdd.UserName = dungchung.GetCell(sheet1, row, 151);
                ncol++;//68
                HRM_NhanVienQuanHeGiaDinh QuanHeGiaDinh = new HRM_NhanVienQuanHeGiaDinh();
                QuanHeGiaDinh.IdGuid = Guid.NewGuid().ToString();
                QuanHeGiaDinh.IdNhanVien = itemAdd.IdGuid;
                QuanHeGiaDinh.HoTen = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//69
                string MaQuanHe = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaQuanHe))
                {
                    var QuanHe = listQuanHeGiaDinh.FirstOrDefault(a => a.Ma == MaQuanHe);
                    if (QuanHe != null)
                        QuanHeGiaDinh.IdQuanHe = QuanHe.IdGuid;
                }
                ncol++;//70
                ncol++;//71
                QuanHeGiaDinh.DiaChi = dungchung.GetCell(sheet1, row, ncol);
                ncol++;//72
                QuanHeGiaDinh.DienThoaiDiDong = dungchung.GetCell(sheet1, row, ncol);
                listQuanHeGiaDinhAdd.Add(QuanHeGiaDinh);


                listResult.Add(itemAdd);

            }
            db.HRM_NhanVienQuanHeGiaDinh.AddRange(listQuanHeGiaDinhAdd);

            if (listResult.Count > 0)
            {
                List<string> listChechk = new List<string>();
                var func = _FuncNhanVien;
                foreach (var item in listResult)
                {
                    clsNhanVien _clsNhanVien = new clsNhanVien();
                    //if (string.IsNullOrEmpty(item.UserName))
                    //    _clsNhanVien = _CallWebService.SetNhanSu(db, item);
                    //else
                    //{
                    //    if (!listChechk.Contains(item.UserName))
                    _clsNhanVien = _CallWebService.SetNhanSuCreateUser(db, item);
                    //    else
                    //        _clsNhanVien = _CallWebService.SetNhanSu(db, item);
                    //    listChechk.Add(item.UserName);
                    //}
                    item.IdNhanVienEOS = _clsNhanVien.Id;
                    item.IdUser = _clsNhanVien.UserPkid;
                }
            }
            db.HRM_NhanVien.AddRange(listResult);
            db.SaveChanges();
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", listResult);
        }
        public dynamic ImportDanhSachNhanVienUpdate(ApplicationDbContext db, string FileName)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            List<HRM_NhanVien> listResult = new List<HRM_NhanVien>();
            string pathTemplate = PathUploads + FileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            DateTime dt = DateTime.Now;

            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listCalamViec = db.HRM_dmLichLamViec.ToList();
            var listnhanVien = db.HRM_NhanVien.ToList();
            var listHopDong = db.HRM_HopDong.ToList();
            List<string> listCheckMaTrung = new List<string>();
            List<HRM_NhanVienQuanHeGiaDinh> listQuanHeGiaDinhAdd = new List<HRM_NhanVienQuanHeGiaDinh>();
            List<string> listMa = new List<string>();
            for (int row = 6; row <= sheet1.Cells.End.Row; row++)
            {
                int ncol = 2;
                var MaNhanVien = dungchung.GetCell(sheet1, row, ncol);
                if (string.IsNullOrEmpty(MaNhanVien))
                    break;
                while (MaNhanVien.Length < 6)
                {
                    MaNhanVien = "0" + MaNhanVien;
                }
                HRM_NhanVien itemAdd = listnhanVien.FirstOrDefault(a => a.MaNhanVien == MaNhanVien);
                if (itemAdd == null)
                {
                    listMa.Add(MaNhanVien);
                    continue;
                }

                var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == itemAdd.IdGuid);
                ncol = 8;//8
                         //string MaCaLamViec = dungchung.GetCell(sheet1, row, ncol);
                         //var CaLamViec = listCalamViec.FirstOrDefault(a => a.Ten == MaCaLamViec);
                         //if (CaLamViec != null)
                         //    itemAdd.IdLichLamViec = CaLamViec.IdGuid;
                         //HopDong.IdThoiGianLamViec = itemAdd.IdLichLamViec;
                ncol++;//9
                string MaChucVu = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucVu) && !MaChucVu.Contains("#N/A"))
                {
                    var ChucVu = listChucVu.FirstOrDefault(a => a.MaChucVu == MaChucVu);
                    if (ChucVu != null)
                        itemAdd.IdChucVu = ChucVu.Id;
                    HopDong.IddmChucVu = itemAdd.IdChucVu;

                }
                ncol++;//10
                ncol++;//11
                string MaChucDanh = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucDanh) && !MaChucDanh.Contains("#N/A"))
                {
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == MaChucDanh);
                    if (ChucDanh != null)
                        itemAdd.IdChucDanh = ChucDanh.Id;
                    HopDong.IddmChucDanh = itemAdd.IdChucDanh;
                }

                ncol++;//12
                ncol++;//13
                itemAdd.ChucDanhLuuHanhNoiBo = dungchung.GetCell(sheet1, row, ncol);

                ncol++;//14

                ncol += 3;//17
                var MaBoPhan = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaBoPhan))
                {
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == MaBoPhan);
                    if (BoPhan != null)
                        itemAdd.IdBoPhanEOS = BoPhan.Id;
                    HopDong.IdBoPhanEOS = itemAdd.IdBoPhanEOS;
                }
                db.Entry(itemAdd).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.Entry(HopDong).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                HopDong.IdNhanVienEOS = itemAdd.IdNhanVienEOS;
                clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSu(db, HopDong);
            }
            db.SaveChanges();
            return string.Join(",", listMa);

            //return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thành công", listResult);
        }
        public dynamic ExportFileMauDanhSachUngVienChuaSoVan(ApplicationDbContext db)
        {
            string sFile = "DanhSachUngVienChuaSoVan.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "ImportTuyenDung.xlsx";
            //var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            var listQuocTich = db.HRM_dmQuocTich.ToList();
            var listTTHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var listDanToc = db.HRM_dmDanToc.ToList();
            var listNoiCap = db.HRM_dmNoiCap.ToList();
            var listTrinhDoHocVan = db.HRM_dmTrinhDoDaoTao.ToList();
            var listThanhPho = db.HRM_dmTinh.ToList();
            var listQuanHuyen = db.HRM_dmQuanHuyen.ToList();
            var listPhuongXa = db.HRM_dmPhuongXa.ToList();
            var listNguonTuyenDung = db.HRM_dmNguonTuyenDung.ToList();
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listCongTy = _CallWebService.GetDanhSachDuAn(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listQuanHeGD = db.HRM_dmQuanHeGiaDinh.ToList();
            var listLoaiNhanVien = db.HRM_dmLoaiNhanVien.ToList();
            var sheet1 = package.Workbook.Worksheets["Danh mục"];

            int nRow = 3;
            foreach (var item in listNoiCap)
            {
                sheet1.Cells[nRow, 1].Value = item.Ten;
                sheet1.Cells[nRow, 2].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in listTTHonNhan)
            {
                sheet1.Cells[nRow, 3].Value = item.Ten;
                sheet1.Cells[nRow, 4].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in listDanToc)
            {
                sheet1.Cells[nRow, 5].Value = item.Ten;
                sheet1.Cells[nRow, 6].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in listTrinhDoHocVan)
            {
                sheet1.Cells[nRow, 7].Value = item.Ten;
                sheet1.Cells[nRow, 8].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in listNguonTuyenDung)
            {
                sheet1.Cells[nRow, 9].Value = item.Ten;
                sheet1.Cells[nRow, 10].Value = item.Ma;
                nRow++;
            }
            nRow = 3;
            foreach (var item in listCongTy)
            {
                sheet1.Cells[nRow, 11].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 12].Value = item.MaBoPhan;
                nRow++;
            }
            nRow = 3;
            foreach (var item in listBoPhan)
            {
                sheet1.Cells[nRow, 13].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 14].Value = item.MaBoPhan;
                nRow++;
            }
            nRow = 3;
            foreach (var item in listLoaiNhanVien)
            {
                sheet1.Cells[nRow, 15].Value = item.Ten;
                sheet1.Cells[nRow, 16].Value = item.Ma;
                nRow++;
            }
            nRow = 3;
            foreach (var item in listChucVu)
            {
                sheet1.Cells[nRow, 17].Value = item.TenChucVu;
                sheet1.Cells[nRow, 18].Value = item.MaChucVu;
                nRow++;
            }
            nRow = 3;
            var listLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
            foreach (var item in listLoaiHopDong)
            {
                sheet1.Cells[nRow, 19].Value = item.Ten;
                sheet1.Cells[nRow, 20].Value = item.Ma;
                nRow++;
            }

            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportBaoCaoTuyenDungPHT(int Thang, int Nam, ApplicationDbContext db)
        {
            string sFile = "TongHopTuyenDung.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = " DATA TUYỂN DỤNG.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            //var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Where(a => a.LoaiBoPhanPkid == 1).ToList();
            var sheet1 = package.Workbook.Worksheets.First();

            var listDataNhanVienUngTuyens = db.HRM_TuyenDung_UngVien.Where(a => a.NgayTiepNhan.Value.Year == Nam && a.NgayTiepNhan.Value.Month == Thang).Distinct().ToList();
            if (listDataNhanVienUngTuyens != null && listDataNhanVienUngTuyens.Count > 0)
            {
                List<long> listIdCongTy = listDataNhanVienUngTuyens.Where(a => a.IdCoSoTrungTuyen != null && a.IdCoSoTrungTuyen != 0).Select(a => a.IdCoSoTrungTuyen ?? 0).Distinct().ToList();
                listIdCongTy.AddRange(listDataNhanVienUngTuyens.Where(a => a.IdCongTyEOS != null && a.IdCongTyEOS != 0).Select(a => a.IdCongTyEOS ?? 0).Distinct().ToList());
                listIdCongTy = listIdCongTy.Distinct().ToList();
                var listCongTys = listCongTy.Where(a => listIdCongTy.Contains(a.Id)).ToList();

                sheet1.Cells[3, 6, 3, 10 + listCongTys.Count].Merge = true;
                sheet1.Cells[4, 11, 4, 10 + listCongTys.Count].Merge = true;
                sheet1.Cells[4, 11].Value = "Cơ sở";
                int Col = 11;
                foreach (var item in listCongTys)
                {
                    sheet1.Cells[5, Col].Value = item.TenBoPhan;
                    Col++;
                }
                sheet1.Cells[3, 1, 5, 10 + listCongTys.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[3, 1, 5, 10 + listCongTys.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                List<string> listIdUngVien = listDataNhanVienUngTuyens.Select(a => a.IdGuid).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(a => listIdUngVien.Contains(a.IdUngVien)).ToList();
                List<HRM_TuyenDung_QuyTrinhPhongVan> listQuyTrinhPhongVan = db.HRM_TuyenDung_QuyTrinhPhongVan.Where(a => a.TrangThai != 0 && a.TrangThai != 5 && listIdUngVien.Contains(a.IdUngVien)).ToList();

                List<long> listIdChucDanh = listDataNhanVienUngTuyens.Where(a => a.IddmChucDanhUngTuyen != 0).Select(a => a.IddmChucDanhUngTuyen).Distinct().ToList();
                listIdChucDanh.AddRange(listDataNhanVienUngTuyens.Where(a => a.IddmTrungTuyen != 0).Select(a => a.IddmTrungTuyen).Distinct().ToList());
                listIdChucDanh = listIdChucDanh.Distinct().ToList();
                int STT = 1;
                int Row = 6;
                string IdNBCT = "";
                var NoiBoCongTy = db.HRM_dmNguonTuyenDung.FirstOrDefault(a => a.Ma == "NBCT");
                if (NoiBoCongTy != null)
                    IdNBCT = NoiBoCongTy.IdGuid;
                foreach (var item in listIdChucDanh)
                {
                    var chucDanh = listChucDanh.FirstOrDefault(a => a.Id == item);
                    if (chucDanh != null)
                    {
                        var DataNhanVienUngTuyens = listDataNhanVienUngTuyens.Where(a => (a.IddmChucDanhUngTuyen == item) || (a.IddmTrungTuyen == item)).ToList();
                        sheet1.Cells[Row, 1].Value = STT;

                        sheet1.Cells[Row, 2].Value = chucDanh.Ten;

                        sheet1.Cells[Row, 3].Value = DataNhanVienUngTuyens.Where(a => a.IddmChucDanhUngTuyen == item && (string.IsNullOrEmpty(a.IddmNguonTuyenDung) || a.IddmNguonTuyenDung != IdNBCT)).Count();
                        sheet1.Cells[Row, 4].Value = DataNhanVienUngTuyens.Where(a => a.IddmChucDanhUngTuyen == item && (!string.IsNullOrEmpty(a.IddmNguonTuyenDung) && a.IddmNguonTuyenDung == IdNBCT)).Count();
                        sheet1.Cells[Row, 5].Value = DataNhanVienUngTuyens.Where(a => a.IddmChucDanhUngTuyen == item).Count();
                        sheet1.Cells[Row, 6].Value = listQuyTrinhPhongVan.Where(a => a.IdChucDanhUngTuyen == item).Count();
                        sheet1.Cells[Row, 7].Value = listQuyTrinhPhongVan.Where(a => a.IddmTrungTuyen == item && a.TrangThai == 10).Count();

                        int NhanViec = DataNhanVienUngTuyens.Where(a => a.IddmTrungTuyen == item && a.IsNhanViec == true).Count();

                        int DangLamViec = DataNhanVienUngTuyens.Where(a => a.IddmTrungTuyen == item && a.IsNhanViec == true && a.IsNghiSauKhiNhanViec == false).Count();
                        if (NhanViec > 0)
                            sheet1.Cells[Row, 8].Value = NhanViec;

                        sheet1.Cells[Row, 9].Value = NhanViec - DangLamViec;
                        if (DangLamViec > 0)
                            sheet1.Cells[Row, 10].Value = DangLamViec;
                        Col = 11;
                        foreach (var cty in listCongTys)
                        {
                            int count = DataNhanVienUngTuyens.Where(a => a.IddmTrungTuyen == item && a.IdCoSoTrungTuyen == cty.Id && a.IsNhanViec == true && a.IsNghiSauKhiNhanViec == false).Count();
                            if (count > 0)
                                sheet1.Cells[Row, Col].Value = count;
                            Col++;
                        }
                    }

                    STT++;
                    Row++;
                }
                sheet1.Cells[Row, 1, Row, 2].Merge = true;
                sheet1.Cells[Row, 1, Row, 10 + listCongTys.Count].Style.Font.Bold = true;
                sheet1.Cells[Row, 1].Value = "Tổng :";
                for (int i = 3; i <= 10 + listCongTys.Count; i++)
                {
                    string DCbd = sheet1.Cells[6, i].Address;
                    string DCkt = sheet1.Cells[Row - 1, i].Address;
                    sheet1.Cells[Row, i].Formula = "=Sum(" + DCbd + ":" + DCkt + ")";
                }

                dungchung.DrawTableExcel(sheet1, 4, Row, 1, 10 + listCongTys.Count);
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[6, 2, Row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }


            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportBaoCaoTheoQuy(int Nam, ApplicationDbContext db, bool isCongNhan = false)
        {
            string sFile = "BaoCaoQuyICT.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "3.Báo cáo tuyển dụng Quý 1+2 hàng năm.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var NhanVienCD = listChucDanh.FirstOrDefault(a => a.Ma == DefineData.eMaChucDanh.NV.ToString());
            if (NhanVienCD == null)
                NhanVienCD = new ChucDanh();
            var GiamSatCD = listChucDanh.FirstOrDefault(a => a.Ma == DefineData.eMaChucDanh.GS.ToString());
            if (GiamSatCD == null)
                GiamSatCD = new ChucDanh();
            var CongNhanCD = listChucDanh.FirstOrDefault(a => a.Ma == DefineData.eMaChucDanh.CN.ToString());
            if (CongNhanCD == null)
                CongNhanCD = new ChucDanh();
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Where(a => a.LoaiBoPhanPkid == 1).ToList();
            var sheet1 = package.Workbook.Worksheets["Tháng 1-6"];
            sheet1.Cells[3, 4].Value = "Tháng 01 đến tháng 06 năm " + Nam;
            var sheet2 = package.Workbook.Worksheets["Tháng 7-12"];
            sheet2.Cells[3, 4].Value = "Tháng 07 đến tháng 12 năm " + Nam;

            var NhuCauTuyenDung = (from b in db.HRM_TuyenDung_NhuCau_ChiTiet
                                   join a in db.HRM_TuyenDung_NhuCau on b.IdTuyenDungNhuCau equals a.IdGuid
                                   where a.isKetThuc == true && a.Ngay.Value.Year == Nam
                                   && (b.IdChucDanh == NhanVienCD.Id || b.IdChucDanh == CongNhanCD.Id || b.IdChucDanh == GiamSatCD.Id)
                                   select new
                                   {
                                       IdChucDanh = b.IdChucDanh,
                                       Ngay = a.Ngay.Value,
                                       SoLuong = (b.SoLuongBoSung) + (b.SoLuongThayThe)
                                   }
                                  ).ToList();
            var listDataNhanVienUngTuyens = db.HRM_TuyenDung_UngVien.Where(a => a.TrangThai == 3 && a.NgayTiepNhan.Value.Year == Nam && a.isCongNhan == isCongNhan).Distinct().ToList();
            // tháng 1-6
            {
                int nCol = 4;
                for (int i = 1; i <= 6; i++)
                {
                    int nRow = 8;
                    // yêu cầu tuyển dụng
                    if (NhuCauTuyenDung != null && NhuCauTuyenDung.Count > 0)
                    {
                        sheet1.Cells[nRow, nCol].Value = NhuCauTuyenDung.Where(a => a.Ngay.Month == i && a.IdChucDanh == NhanVienCD.Id).Sum(a => a.SoLuong);
                        nRow++;
                        sheet1.Cells[nRow, nCol].Value = NhuCauTuyenDung.Where(a => a.Ngay.Month == i && a.IdChucDanh == GiamSatCD.Id).Sum(a => a.SoLuong);
                        nRow++;
                        sheet1.Cells[nRow, nCol].Value = NhuCauTuyenDung.Where(a => a.Ngay.Month == i && a.IdChucDanh == CongNhanCD.Id).Sum(a => a.SoLuong);
                        nRow++;
                    }
                    nRow = 12;

                    if (listDataNhanVienUngTuyens != null && listDataNhanVienUngTuyens.Count > 0)
                    {
                        // <--kết quả tuyển dụng-->
                        {
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id).Count();

                        }
                        //< -- Giới tính-->
                        nRow = 21;
                        {
                            // nhân viên
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.GioiTinh == "Nu").Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.GioiTinh == "Nam").Count();
                            nRow++;
                            nRow++;
                            // giám sát
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.GioiTinh == "Nu").Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.GioiTinh == "Nam").Count();
                            nRow++;
                            nRow++;
                            /// công nhân
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.GioiTinh == "Nu").Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.GioiTinh == "Nam").Count();

                        }

                        //< -- Độ tuổi-->
                        nRow = 31;
                        {
                            DateTime Time = DateTime.Now;
                            DateTime DuoiLaoDong = Time.AddYears(-18);
                            DateTime TrenLaoDongNam = Time.AddYears(-60);
                            DateTime TrenLaoDongNu = Time.AddYears(-55);
                            // nhân viên
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.NgaySinh > DuoiLaoDong).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.NgaySinh < DuoiLaoDong && ((a.GioiTinh == "Nam" && a.NgaySinh > TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh > TrenLaoDongNu))).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && ((a.GioiTinh == "Nam" && a.NgaySinh < TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh < TrenLaoDongNu))).Count();
                            nRow++;
                            nRow++;
                            // giám sát
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.NgaySinh > DuoiLaoDong).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.NgaySinh < DuoiLaoDong && ((a.GioiTinh == "Nam" && a.NgaySinh > TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh > TrenLaoDongNu))).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && ((a.GioiTinh == "Nam" && a.NgaySinh < TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh < TrenLaoDongNu))).Count();
                            nRow++;
                            nRow++;
                            /// công nhân
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.NgaySinh > DuoiLaoDong).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.NgaySinh < DuoiLaoDong && ((a.GioiTinh == "Nam" && a.NgaySinh > TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh > TrenLaoDongNu))).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && ((a.GioiTinh == "Nam" && a.NgaySinh < TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh < TrenLaoDongNu))).Count();

                            // <--nghỉ việc-->
                            nRow = 52;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.IsNhanViec == false).Count();
                            nRow++;
                            sheet1.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.IsNghiSauKhiNhanViec == true).Count();

                        }
                    }
                    nCol++;
                }
            }
            // tháng 7-12
            {
                int nCol = 4;
                for (int i = 7; i <= 12; i++)
                {
                    int nRow = 8;
                    // yêu cầu tuyển dụng
                    if (NhuCauTuyenDung != null && NhuCauTuyenDung.Count > 0)
                    {
                        sheet2.Cells[nRow, nCol].Value = NhuCauTuyenDung.Where(a => a.Ngay.Month == i && a.IdChucDanh == NhanVienCD.Id).Sum(a => a.SoLuong);
                        nRow++;
                        sheet2.Cells[nRow, nCol].Value = NhuCauTuyenDung.Where(a => a.Ngay.Month == i && a.IdChucDanh == GiamSatCD.Id).Sum(a => a.SoLuong);
                        nRow++;
                        sheet2.Cells[nRow, nCol].Value = NhuCauTuyenDung.Where(a => a.Ngay.Month == i && a.IdChucDanh == CongNhanCD.Id).Sum(a => a.SoLuong);
                        nRow++;
                    }
                    nRow = 12;

                    if (listDataNhanVienUngTuyens != null && listDataNhanVienUngTuyens.Count > 0)
                    {
                        // <--kết quả tuyển dụng-->
                        {
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id).Count();

                        }
                        //< -- Giới tính-->
                        nRow = 21;
                        {
                            // nhân viên
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.GioiTinh == "Nu").Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.GioiTinh == "Nam").Count();
                            nRow++;
                            nRow++;
                            // giám sát
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.GioiTinh == "Nu").Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.GioiTinh == "Nam").Count();
                            nRow++;
                            nRow++;
                            /// công nhân
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.GioiTinh == "Nu").Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.GioiTinh == "Nam").Count();

                        }

                        //< -- Độ tuổi-->
                        nRow = 31;
                        {
                            DateTime Time = DateTime.Now;
                            DateTime DuoiLaoDong = Time.AddYears(-18);
                            DateTime TrenLaoDongNam = Time.AddYears(-60);
                            DateTime TrenLaoDongNu = Time.AddYears(-55);
                            // nhân viên
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.NgaySinh > DuoiLaoDong).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && a.NgaySinh < DuoiLaoDong && ((a.GioiTinh == "Nam" && a.NgaySinh > TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh > TrenLaoDongNu))).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == NhanVienCD.Id && ((a.GioiTinh == "Nam" && a.NgaySinh < TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh < TrenLaoDongNu))).Count();
                            nRow++;
                            nRow++;
                            // giám sát
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.NgaySinh > DuoiLaoDong).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && a.NgaySinh < DuoiLaoDong && ((a.GioiTinh == "Nam" && a.NgaySinh > TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh > TrenLaoDongNu))).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == GiamSatCD.Id && ((a.GioiTinh == "Nam" && a.NgaySinh < TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh < TrenLaoDongNu))).Count();
                            nRow++;
                            nRow++;
                            /// công nhân
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.NgaySinh > DuoiLaoDong).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.NgaySinh < DuoiLaoDong && ((a.GioiTinh == "Nam" && a.NgaySinh > TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh > TrenLaoDongNu))).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && ((a.GioiTinh == "Nam" && a.NgaySinh < TrenLaoDongNam) || (a.GioiTinh == "Nu" && a.NgaySinh < TrenLaoDongNu))).Count();

                            // <--nghỉ việc-->
                            nRow = 52;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.IsNhanViec == false).Count();
                            nRow++;
                            sheet2.Cells[nRow, nCol].Value = listDataNhanVienUngTuyens.Where(a => a.NgayTiepNhan.Value.Month == i && a.IddmChucDanhUngTuyen == CongNhanCD.Id && a.IsNghiSauKhiNhanViec == true).Count();

                        }
                    }
                    nCol++;
                }
            }




            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public void _ExportBaoCaoTongHopNhanSu(IEnumerable<HRM_NhanVien> listdata, int nRow, ExcelWorksheet sheet1)
        {
            if (listdata == null)
                return;
            DateTime dt = DateTime.Today;
            DateTime T18 = dt.AddYears(-18);
            DateTime T41 = dt.AddYears(-41);
            DateTime T51 = dt.AddYears(-51);
            DateTime T56 = dt.AddYears(-56);
            DateTime T61 = dt.AddYears(-61);
            DateTime T66 = dt.AddYears(-66);
            DateTime T71 = dt.AddYears(-71);
            int nCol = 3;
            sheet1.Cells[nRow, nCol].Value = listdata.Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.IdGioiTinh == "Nam").Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.IdGioiTinh != "Nam").Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year > T18.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T18.Year && a.NgaySinh.Year > T41.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T41.Year && a.NgaySinh.Year > T51.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T51.Year && a.NgaySinh.Year > T56.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T56.Year && a.NgaySinh.Year > T61.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T61.Year && a.NgaySinh.Year > T66.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T66.Year && a.NgaySinh.Year > T71.Year).Count();
            nCol++;
            sheet1.Cells[nRow, nCol].Value = listdata.Where(a => a.NgaySinh.Year <= T71.Year).Count();
            nCol++;
        }

        public dynamic ExportBaoCaoTongHopNhanSu(ApplicationDbContext db, EntityGetDanhSachNhanVienRequest timkiem)
        //int Quy, ApplicationDbContext db, int Nam = 0, double TuNgayUnix = 0, double DenNgayUnix = 0)//1 quý 1,2 quý 2, 3 quý 3, 4 quý 4, 5 t1->t6, 6 t7->t2
        {
            string sFile = "BaoCaoDoTuoi.xlsx";
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Today;
            var filenamegui = "TongHopNhanSuToanCongTy.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var BoPhanCNDN = listBoPhan.FirstOrDefault(a => a.MaBoPhan == "DN");
            if (BoPhanCNDN == null)
                BoPhanCNDN = new BoPhan();
            var BoPhanNVDN = _CallWebService.GetDanhSachBoPhanTheoCongTy(db, BoPhanCNDN.Id).Select(a => a.Id).ToList();
            if (BoPhanNVDN == null)
                BoPhanNVDN = new List<long>();
            BoPhanNVDN.Add(BoPhanCNDN.Id);
            var BoPhanCNHCM = listBoPhan.FirstOrDefault(a => a.MaBoPhan == "HCM");
            if (BoPhanCNHCM == null)
                BoPhanCNHCM = new BoPhan();
            var BoPhanNVHCM = _CallWebService.GetDanhSachBoPhanTheoCongTy(db, BoPhanCNHCM.Id).Select(a => a.Id).ToList();
            if (BoPhanNVHCM == null)
                BoPhanNVHCM = new List<long>();
            BoPhanNVHCM.Add(BoPhanCNHCM.Id);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var GiamSatCD = listChucVu.FirstOrDefault(a => a.MaChucVu == DefineData.eMaChucVu.GS.ToString());
            if (GiamSatCD == null)
                GiamSatCD = new ChucVu();
            var CongNhanCD = listChucVu.FirstOrDefault(a => a.MaChucVu == DefineData.eMaChucVu.CN.ToString());
            if (CongNhanCD == null)
                CongNhanCD = new ChucVu();

            var listCongTy = _CallWebService.GetDanhSachDuAnDaDuyet(db);
            var listLoaiDuAn = _CallWebService.GetListdmLoaiDuAn(db);
            var LoaiNhaMay = listLoaiDuAn.FirstOrDefault(a => a.Ma == "005");
            var LoaiCongTy = listLoaiDuAn.FirstOrDefault(a => a.Ma == "006");
            List<long> listIdNhaMay = new List<long>();
            List<long> listIdBenhVien = new List<long>();
            if (LoaiNhaMay != null)
            {
                var listDuAn = listCongTy.Where(a => a.IddmLoaiDuAn == LoaiNhaMay.Id).ToList();
                listIdNhaMay = listDuAn.Select(a => a.Id).Distinct().ToList();
            }
            if (LoaiCongTy != null)
            {
                var listDuAn = listCongTy.Where(a =>/* a.IddmLoaiDuAn != LoaiCongTy.Id &&*/ a.IddmLoaiDuAn != LoaiNhaMay.Id).ToList();
                listIdBenhVien = listDuAn.Select(x => x.Id).Distinct().ToList();
            }
            List<HRM_NhanVien> list1 = new List<HRM_NhanVien>();

            var sheetTH = package.Workbook.Worksheets["TH NS"];

            var sheet1 = package.Workbook.Worksheets["Độ tuổi"];
            if (Host == "PHT")
            {
                sheetTH.Cells[2, 1].Value = "PHÒNG NHÂN SỰ";
                sheet1.Cells[2, 1].Value = "PHÒNG NHÂN SỰ";
            }
            sheet1.Cells[4, 1].Value = "Ngày " + dt.ToString("dd/MM/yyyy");


            var listNhanVien = (from a in db.HRM_NhanVien
                                where a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()
                                && a.IdChucVu != null && a.IdChucVu > 0 && a.NgayBatDauLamViec != null
                                //&& (Nam == 0 || (a.NgayBatDauLamViec.Value.Year <= Nam))
                                select a);
            if (listNhanVien != null)
            {
                var listIdNhanVien = listNhanVien.Select(a => a.IdGuid).ToList();
                var listCongTy_NS = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == nameof(HRM_NhanVien) && listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
                #region Tổng hợp nhân sự
                listNhanVien = listNhanVien.Where(x => x.NgayBatDauLamViec != null && x.NgayBatDauLamViec.Value.Year > 1970);

                if (timkiem.TuNgayUnix > 0)
                {
                    DateTime Ngay = dungchung.UnixTimeStampToDateTime(timkiem.TuNgayUnix);
                    Ngay = new DateTime(Ngay.Year, Ngay.Month, Ngay.Day, 0, 0, 0);
                    listNhanVien = listNhanVien.Where(a => a.NgayBatDauLamViec != null && a.NgayBatDauLamViec >= Ngay);
                }
                if (timkiem.DenNgayUnix > 0)
                {
                    DateTime Ngay = dungchung.UnixTimeStampToDateTime(timkiem.DenNgayUnix);
                    Ngay = new DateTime(Ngay.Year, Ngay.Month, Ngay.Day, 0, 0, 0).AddDays(1);
                    listNhanVien = listNhanVien.Where(a => a.NgayBatDauLamViec != null && a.NgayBatDauLamViec < Ngay);
                }
                var listNhanVienTH = listNhanVien.ToList();
                var listCN = listNhanVien.Where(a => a.IdChucVu == CongNhanCD.Id).ToList();
                var listNV = listNhanVien.Where(a => a.IdChucVu != CongNhanCD.Id && a.IdChucVu != GiamSatCD.Id).ToList();
                var listNVHCMs = listNhanVienTH.Where(a => BoPhanNVHCM.Contains(a.IdBoPhanEOS)).ToList();
                DateTime dtNgayBatDau = listNhanVienTH.OrderBy(x => x.NgayBatDauLamViec).Select(x => x.NgayBatDauLamViec.Value).FirstOrDefault();
                DateTime dtNgayKetThuc = listNhanVienTH.OrderByDescending(x => x.NgayBatDauLamViec).Select(x => x.NgayBatDauLamViec.Value).FirstOrDefault();
                int SoNam = dtNgayKetThuc.Year - dtNgayBatDau.Year + 1;
                List<HRM_BaoCaoCongViec_NhanVien> listThoiGian = new List<HRM_BaoCaoCongViec_NhanVien>();
                if (SoNam == 1)
                {
                    for (int iThang = dtNgayBatDau.Month; iThang <= dtNgayKetThuc.Month; iThang++)
                    {
                        HRM_BaoCaoCongViec_NhanVien itemAdd = new HRM_BaoCaoCongViec_NhanVien();
                        itemAdd.Thang = iThang;
                        itemAdd.Nam = dtNgayKetThuc.Year;
                        listThoiGian.Add(itemAdd);
                    }
                }
                else if (SoNam == 2)
                {
                    int SoThang = ((dtNgayKetThuc.Year - dtNgayBatDau.Year) * 12) + dtNgayKetThuc.Month - dtNgayBatDau.Month;
                    for (int iThang = dtNgayBatDau.Month; iThang <= 12; iThang++)
                    {
                        HRM_BaoCaoCongViec_NhanVien itemAdd = new HRM_BaoCaoCongViec_NhanVien();
                        itemAdd.Thang = iThang;
                        itemAdd.Nam = dtNgayBatDau.Year;
                        listThoiGian.Add(itemAdd);
                    }
                    for (int iThang = 1; iThang <= dtNgayKetThuc.Month; iThang++)
                    {
                        HRM_BaoCaoCongViec_NhanVien itemAdd = new HRM_BaoCaoCongViec_NhanVien();
                        itemAdd.Thang = iThang;
                        itemAdd.Nam = dtNgayKetThuc.Year;
                        listThoiGian.Add(itemAdd);
                    }
                }
                else
                {
                    for (int iThang = dtNgayBatDau.Month; iThang <= 12; iThang++)
                    {
                        HRM_BaoCaoCongViec_NhanVien itemAdd = new HRM_BaoCaoCongViec_NhanVien();
                        itemAdd.Thang = iThang;
                        itemAdd.Nam = dtNgayBatDau.Year;
                        listThoiGian.Add(itemAdd);
                    }
                    for (int iNam = dtNgayBatDau.Year + 1; iNam < dtNgayKetThuc.Year; iNam++)
                    {
                        for (int iThang = 1; iThang <= 12; iThang++)
                        {
                            HRM_BaoCaoCongViec_NhanVien itemAdd = new HRM_BaoCaoCongViec_NhanVien();
                            itemAdd.Thang = iThang;
                            itemAdd.Nam = iNam;
                            listThoiGian.Add(itemAdd);
                        }
                    }
                    for (int iThang = 1; iThang <= dtNgayKetThuc.Month; iThang++)
                    {
                        HRM_BaoCaoCongViec_NhanVien itemAdd = new HRM_BaoCaoCongViec_NhanVien();
                        itemAdd.Thang = iThang;
                        itemAdd.Nam = dtNgayKetThuc.Year;
                        listThoiGian.Add(itemAdd);
                    }
                }

                {
                    int nRow = 5;
                    int nCol = 1;
                    // // header
                    {
                        sheetTH.Cells[5, 3].Value = "Từ " + dtNgayBatDau.ToString("dd/MM/yyyy") + "-" + dtNgayKetThuc.ToString("dd/MM/yyyy");
                        nCol = 4;
                        for (var iNam = dtNgayBatDau.Year; iNam <= dtNgayKetThuc.Year; iNam++)
                        {
                            sheetTH.Cells[nRow, nCol].Value = "Năm " + iNam;
                            int nCount = listThoiGian.FindAll(x => x.Nam == iNam).Count;
                            if (nCount > 1)
                            {
                                //sheetTH.Cells[nRow, nCol, nRow, nCol + nCount].Merge = true;
                                nCol += nCount;
                            }
                            else
                                nCol++;
                        }
                        sheetTH.Cells[nRow, nCol].Value = "Ghi chú";
                        sheetTH.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                        sheetTH.Column(nCol).Width = 30;
                        nCol--;
                        for (var iNam = dtNgayKetThuc.Year; iNam >= dtNgayBatDau.Year; iNam--)
                        {
                            int nCount = listThoiGian.FindAll(x => x.Nam == iNam).Count;
                            if (nCount > 1)
                            {
                                sheetTH.Cells[nRow, nCol - nCount + 1, nRow, nCol].Merge = true;
                                nCol -= nCount;
                            }
                            else
                                nCol--;
                        }

                        nRow++;
                        nCol = 4;
                        foreach (var iThoiGian in listThoiGian)
                        {
                            sheetTH.Cells[nRow, nCol].Value = "Tháng " + iThoiGian.Thang;
                            nCol++;
                        }
                        sheetTH.Cells[3, 1, 3, nCol].Merge = true;

                    }
                    #region Nhân viên
                    nRow += 2;
                    // nhân viên miền bắc
                    {
                        nCol = 4;
                        foreach (var iThoiGian in listThoiGian)
                        {
                            var listdata = listNV.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                       && !BoPhanNVDN.Contains(a.IdBoPhanEOS) && !BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                            var count = listdata.Count();
                            if (listdata != null)
                                sheetTH.Cells[nRow, nCol].Value = listdata.Count();
                            listdata = listNV.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                        && BoPhanNVDN.Contains(a.IdBoPhanEOS));
                            if (listdata != null)
                                sheetTH.Cells[nRow + 1, nCol].Value = listdata.Count();
                            listdata = listNV.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                       && BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                            if (listdata != null)
                                sheetTH.Cells[nRow + 2, nCol].Value = listdata.Count();
                            sheetTH.Cells[7, nCol].Formula = "SUM(" + sheetTH.Cells[nRow, nCol].Address + ":" + sheetTH.Cells[nRow + 2, nCol].Address + ")";
                            nCol++;
                        }
                    }
                    nRow++;
                    nRow++;
                    #endregion
                    nRow += 2;
                    #region Giám sát
                    {
                        nCol = 4;
                        foreach (var iThoiGian in listThoiGian)
                        {
                            var listdata = listNhanVienTH.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                            && a.IdChucVu == GiamSatCD.Id && !BoPhanNVDN.Contains(a.IdBoPhanEOS) && !BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                            if (listdata != null)
                                sheetTH.Cells[nRow, nCol].Value = listdata.Count();
                            listdata = listNhanVienTH.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                      && a.IdChucVu == GiamSatCD.Id && BoPhanNVDN.Contains(a.IdBoPhanEOS));
                            if (listdata != null)
                                sheetTH.Cells[nRow + 1, nCol].Value = listdata.Count();
                            listdata = listNhanVienTH.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                        && a.IdChucVu == GiamSatCD.Id && BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                            if (listdata != null)
                                sheetTH.Cells[nRow + 2, nCol].Value = listdata.Count();
                            sheetTH.Cells[11, nCol].Formula = "SUM(" + sheetTH.Cells[nRow, nCol].Address + ":" + sheetTH.Cells[nRow + 2, nCol].Address + ")";
                            nCol++;
                        }
                    }
                    #endregion
                    nRow += 2;
                    #region Công nhân
                    nRow = 16;

                    // bệnh viện ...
                    listIdNhanVien = listCongTy_NS.Where(a => listIdBenhVien.Contains(a.IdBoPhan)).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    var listNVBV = listCN.Where(a => listIdNhanVien.Contains(a.IdGuid) && a.IdBoPhanEOS != BoPhanCNDN.Id && a.IdBoPhanEOS != BoPhanCNHCM.Id);
                    var listIdNvBV = listNVBV.Select(a => a.IdGuid).ToList();
                    int CountBV = listNVBV.Count();
                    {
                        nCol = 4;
                        foreach (var iThoiGian in listThoiGian)
                        {
                            var listdata = listNVBV.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                                            );
                            if (listdata != null)
                                sheetTH.Cells[nRow, nCol].Value = listdata.Count();
                            nCol++;
                        }
                    }
                    nRow++;



                    listIdNhanVien = listCongTy_NS.Where(a => !listIdBenhVien.Contains(a.IdBoPhan)).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    nCol = 4;
                    var listNVNhaMay = listCN.Where(a => listIdNhanVien.Contains(a.IdGuid) && a.IdBoPhanEOS != BoPhanCNDN.Id && a.IdBoPhanEOS != BoPhanCNHCM.Id && !listIdNvBV.Contains(a.IdGuid));
                    int CountNV = listNVNhaMay.Count();
                    foreach (var iThoiGian in listThoiGian)
                    {
                        var listdata = listNVNhaMay.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                                     );
                        sheetTH.Cells[nRow, nCol].Value = listdata.Count();
                        nCol++;
                    }
                    nRow++;
                    //Đà nẵng
                    sheetTH.Cells[nRow, 1].Value = "3";
                    sheetTH.Cells[nRow, 2].Value = "Đà Nẵng";
                    nCol = 4;
                    var listNVDN = listCN.Where(a => a.IdBoPhanEOS == BoPhanCNDN.Id);

                    foreach (var iThoiGian in listThoiGian)
                    {
                        var listdata = listNVDN.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                                            );
                        if (listdata != null)
                            sheetTH.Cells[nRow, nCol].Value = listdata.Count();
                        nCol++;
                    }
                    nRow++;
                    //Hồ chí minh
                    sheetTH.Cells[nRow, 1].Value = "4";
                    sheetTH.Cells[nRow, 2].Value = "TP HCM";
                    var listNVHCM = listCN.Where(a => a.IdBoPhanEOS == BoPhanCNHCM.Id);

                    nCol = 4;
                    foreach (var iThoiGian in listThoiGian)
                    {
                        var listdata = listNVHCM.Where(a => a.NgayBatDauLamViec.Value.Month == iThoiGian.Thang && a.NgayBatDauLamViec.Value.Year == iThoiGian.Nam
                                                                                             );
                        if (listdata != null)
                            sheetTH.Cells[nRow, nCol].Value = listdata.Count();
                        nCol++;
                    }
                    sheetTH.Cells[nRow + 1, 1].Value = "Tổng";
                    for (int i = 3; i < nCol; i++)
                    {

                        string addBV = sheetTH.Cells[16, i].Address;
                        string addNM = sheetTH.Cells[17, i].Address;
                        string addDN = sheetTH.Cells[18, i].Address;
                        string addHCM = sheetTH.Cells[19, i].Address;
                        sheetTH.Cells[15, i].Formula = "=(" + addBV + "+" + addNM + "+" + addDN + "+" + addHCM + ")";
                        sheetTH.Cells[nRow + 1, i].Formula = "=(" + sheetTH.Cells[7, i].Address + "+" + sheetTH.Cells[11, i].Address + "+" + sheetTH.Cells[15, i].Address + ")";
                        if (i > 3)
                            sheetTH.Column(i).Hidden = true;
                    }
                    for (int i = 7; i <= nRow; i++)
                    {
                        sheetTH.Cells[i, 3].Formula = "=SUM(" + sheetTH.Cells[i, 4].Address + ":" + sheetTH.Cells[i, nCol].Address + ")";
                    }
                    nRow++;
                    sheetTH.Cells[nRow, 1, nRow, 14].Style.Font.Bold = true;
                    sheetTH.Cells[7, 3, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    dungchung.DrawTableExcel(sheetTH, 5, nRow, 1, nCol);
                    nRow++;
                    nRow++;
                    sheetTH.Cells[nRow, nCol - 3, nRow, nCol].Merge = true;
                    sheetTH.Cells[nRow, nCol - 3].Value = "Ngày " + dt.Day + " tháng " + dt.Month + " năm " + dt.Year;
                    sheetTH.Cells[nRow, 1, nRow, 14].Style.Font.Bold = true;
                    nRow++;
                    sheetTH.Cells[nRow, nCol - 3, nRow, nCol].Merge = true;
                    sheetTH.Cells[nRow, nCol - 3].Value = "Lập Biểu";
                    sheetTH.Cells[nRow, 1, nRow, 14].Style.Font.Bold = true;

                    sheetTH.Cells.Style.Font.Name = "Times New Roman";
                    sheetTH.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheetTH.Cells[8, 2, nRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheetTH.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheetTH.Cells.Style.Font.Size = 12;
                    sheetTH.Cells.Style.WrapText = true;

                    #endregion

                }
                #endregion
                #region độ tuổi
                {
                    int nRow = 9;
                    #region Nhân viên
                    // nhân viên miền bắc
                    {
                        var listdata = listNV.Where(a => !BoPhanNVDN.Contains(a.IdBoPhanEOS) && !BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    // nhân viên Đà Nẵng
                    {
                        var listdata = listNV.Where(a => BoPhanNVDN.Contains(a.IdBoPhanEOS));
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    // nhân viên HCM
                    {
                        var listdata = listNV.Where(a => BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    #endregion
                    nRow += 2;
                    #region Giám sát
                    // Giám sát miền bắc
                    {
                        var listdata = listNhanVienTH.Where(a => a.IdChucVu == GiamSatCD.Id && !BoPhanNVDN.Contains(a.IdBoPhanEOS) && !BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    // Giám sát Đà Nẵng
                    {
                        var listdata = listNhanVienTH.Where(a => a.IdChucVu == GiamSatCD.Id && BoPhanNVDN.Contains(a.IdBoPhanEOS));
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    // Giám sát HCM
                    {
                        var listdata = listNhanVienTH.Where(a => a.IdChucVu == GiamSatCD.Id && BoPhanNVHCM.Contains(a.IdBoPhanEOS));
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    #endregion
                    nRow += 2;

                    #region Công nhân
                    //int Row = 16;
                    // bệnh viện ...
                    var listIdNvBV = new List<string>();
                    {
                        listIdNhanVien = listCongTy_NS.Where(a => listIdBenhVien.Contains(a.IdBoPhan)).Select(a => a.IdQuyTrinh).Distinct().ToList();
                        var listdata = listNhanVienTH.Where(a => listIdNhanVien.Contains(a.IdGuid) && a.IdChucVu == CongNhanCD.Id && a.IdBoPhanEOS != BoPhanCNDN.Id && a.IdBoPhanEOS != BoPhanCNHCM.Id);
                        listIdNvBV = listdata.Select(a => a.IdGuid).ToList();
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    {
                        listIdNhanVien = listCongTy_NS.Where(a => !listIdBenhVien.Contains(a.IdBoPhan)).Select(a => a.IdQuyTrinh).Distinct().ToList();
                        var listdata = listNhanVienTH.Where(a => !listIdNvBV.Contains(a.IdGuid) && listIdNhanVien.Contains(a.IdGuid) && a.IdChucVu == CongNhanCD.Id && a.IdBoPhanEOS != BoPhanCNDN.Id && a.IdBoPhanEOS != BoPhanCNHCM.Id);
                        var lista = listdata.ToList();
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    //Đà nẵng
                    sheet1.Cells[nRow, 1].Value = "3";
                    sheet1.Cells[nRow, 2].Value = "Đà Nẵng";
                    if (LoaiNhaMay != null && LoaiCongTy != null)
                    {
                        //var listDuAn = listCongTy.Where(a => a.IddmLoaiDuAn != LoaiCongTy.Id && a.IddmLoaiDuAn != LoaiNhaMay.Id).ToList();
                        //List<long> listId = listDuAn.Select(a => a.Id).Distinct().ToList();
                        listIdNhanVien = listCongTy_NS.Select(a => a.IdQuyTrinh).Distinct().ToList();
                        var listdata = listNhanVienTH.Where(a => listIdNhanVien.Contains(a.IdGuid) && a.IdChucVu == CongNhanCD.Id && a.IdBoPhanEOS == BoPhanCNDN.Id);
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    nRow++;
                    //Hồ chí minh
                    sheet1.Cells[nRow, 1].Value = "4";
                    sheet1.Cells[nRow, 2].Value = "TP HCM";
                    if (LoaiNhaMay != null && LoaiCongTy != null)
                    {
                        //var listDuAn = listCongTy.Where(a => a.IddmLoaiDuAn != LoaiCongTy.Id && a.IddmLoaiDuAn != LoaiNhaMay.Id).ToList();
                        //List<long> listId = listDuAn.Select(a => a.Id).Distinct().ToList();
                        listIdNhanVien = listCongTy_NS.Select(a => a.IdQuyTrinh).Distinct().ToList();
                        var listdata = listNhanVienTH.Where(a => listIdNhanVien.Contains(a.IdGuid) && a.IdChucVu == CongNhanCD.Id && a.IdBoPhanEOS == BoPhanCNHCM.Id);
                        _ExportBaoCaoTongHopNhanSu(listdata, nRow, sheet1);
                    }
                    sheet1.Cells[nRow + 1, 1].Value = "Tổng";
                    for (int i = 3; i <= 13; i++)
                    {
                        string addBV = sheet1.Cells[17, i].Address;
                        string addNM = sheet1.Cells[18, i].Address;
                        string addDN = sheet1.Cells[nRow - 1, i].Address;
                        string addHCM = sheet1.Cells[nRow, i].Address;
                        sheet1.Cells[16, i].Formula = "=(" + addBV + "+" + addNM + "+" + addDN + "+" + addHCM + ")";
                        sheet1.Cells[nRow + 1, i].Formula = "=(" + sheet1.Cells[8, i].Address + "+" + sheet1.Cells[12, i].Address + "+" + sheet1.Cells[16, i].Address + ")";
                    }
                    nRow++;
                    sheet1.Cells[nRow, 1, nRow, 14].Style.Font.Bold = true;
                    dungchung.DrawTableExcel(sheet1, 6, nRow, 1, 14);
                    sheet1.Cells[8, 3, nRow, 13].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    nRow++;
                    nRow++;
                    sheet1.Cells[nRow, 8, nRow, 14].Merge = true;
                    sheet1.Cells[nRow, 8].Value = "Ngày " + dt.Day + " tháng " + dt.Month + " năm " + dt.Year;
                    sheet1.Cells[nRow, 1, nRow, 14].Style.Font.Bold = true;
                    nRow++;
                    sheet1.Cells[nRow, 8, nRow, 14].Merge = true;
                    sheet1.Cells[nRow, 8].Value = "Lập Biểu";
                    sheet1.Cells[nRow, 1, nRow, 14].Style.Font.Bold = true;
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet1.Cells[8, 2, nRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.Font.Size = 12;
                    sheet1.Cells.Style.WrapText = true;
                    #endregion
                }
                #endregion
            }
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(filenamegui))
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        public dynamic ExportDanhSachNhanVienTrungTuyen(int Thang, int Nam, ApplicationDbContext db, bool isCongNhan = false)
        {
            try
            {
                string sFile = "DanhSachTuyenDungHangThang.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "Danh sách tuyển dụng hàng tháng.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                //var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Where(a => a.LoaiBoPhanPkid == 1).ToList();
                var listQuocTich = db.HRM_dmQuocTich.ToList();
                var listTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
                var listNguonTuyenDung = db.HRM_dmNguonTuyenDung.ToList();
                var listDanToc = db.HRM_dmDanToc.ToList();
                var listNoiCap = db.HRM_dmNoiCap.ToList();
                var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listCaLamViec = db.HRM_dmLichLamViec.ToList();
                var listTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
                var sheet1 = package.Workbook.Worksheets.First();
                int Row = 9;
                int nCol = 6;
                if (isCongNhan == false)
                    sheet1.Cells[8, 6].Value = "NHÂN VIÊN TUYỂN MỚI";
                var listDataNhanVienUngTuyens = db.HRM_TuyenDung_UngVien.Where(a => a.NgayNhanViecDuKien != null && a.NgayNhanViecDuKien.Value.Year == Nam && a.NgayNhanViecDuKien.Value.Month == Thang && a.TrangThai == 3 && a.isCongNhan == isCongNhan && a.IsNhanViec == true).Distinct().ToList();
                if (listDataNhanVienUngTuyens != null && listDataNhanVienUngTuyens.Count > 0)
                {
                    List<string> listIdUngVien = listDataNhanVienUngTuyens.Select(a => a.IdGuid).Distinct().ToList();
                    var listPCUngVien = db.HRM_UngVien_PhuCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).Distinct().ToList();
                    var listBCUngVien = db.HRM_UngVienBangCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).Distinct().ToList();
                    int STT = 1;
                    Row++;
                    if (Host == "PHT")
                    {
                        sheet1.Cells[1, 6].Value = "DANH SÁCH TUYỂN DỤNG CÁC ĐƠN VỊ PHT";
                        sheet1.Cells[1, 1].Value = "";
                    }
                    foreach (var item in listDataNhanVienUngTuyens)
                    {
                        nCol = 6;
                        sheet1.Cells[Row, 1].Value = STT;
                        sheet1.Cells[Row, 2].Value = item.MaSoThueCCCD;
                        sheet1.Cells[Row, 3].Value = item.MaSoThueCMT;
                        sheet1.Cells[Row, nCol].Value = item.HoTenDayDu;
                        nCol++;
                        if (item.NgaySinh != null && item.NgaySinh.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            sheet1.Cells[Row, nCol].Value = item.NgaySinh.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        if (item.GioiTinh == "Nam")
                            sheet1.Cells[Row, nCol].Value = "Nam";
                        nCol++;
                        if (item.GioiTinh == "Nu")
                            sheet1.Cells[Row, nCol].Value = "Nữ";
                        nCol++;
                        var TrinhDo = listTrinhDo.FirstOrDefault(a => a.IdGuid == item.IddmTrinhDoHocVan);
                        if (TrinhDo != null)
                            sheet1.Cells[Row, nCol].Value = TrinhDo.Ten;
                        nCol++;
                        // bằng cấp 
                        var listBC = listBCUngVien.Where(a => a.IdUngVien == item.IdGuid).ToList();
                        if (listBC != null && listBC.Count > 0)
                        {
                            var cellBC = sheet1.Cells[Row, nCol];
                            foreach (var BC in listBC)
                            {
                                TrinhDo = listTrinhDo.FirstOrDefault(a => a.IdGuid == item.IddmTrinhDoHocVan);
                                if (TrinhDo != null)
                                {
                                    var r1 = cellBC.RichText.Add("- " + BC.NoiDaoTao + " - " + TrinhDo.Ten + "\r\n");
                                }
                            }
                            cellBC.Style.Font.Name = "Times New Roman";
                            cellBC.Style.Font.Size = 12;
                            cellBC.Style.WrapText = true;
                            sheet1.Row(Row).Height = (listBC.Count * 1.5 * 15.75);
                        }
                        else
                            nCol++;
                        sheet1.Cells[Row, nCol].Value = item.DienThoaiDiDong;
                        nCol++;
                        sheet1.Cells[Row, nCol].Value = item.NguyenQuan;
                        nCol++;
                        sheet1.Cells[Row, nCol].Value = item.HoKhauThuongTru;
                        nCol++;
                        sheet1.Cells[Row, nCol].Value = item.CCCD;
                        nCol++;
                        nCol++;
                        if (item.NgayCap != null && item.NgayCap.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            sheet1.Cells[Row, nCol].Value = item.NgayCap.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        if (!string.IsNullOrEmpty(item.NoiCap))
                            sheet1.Cells[Row, nCol].Value = listNoiCap.Where(a => a.IdGuid == item.NoiCap).Select(a => a.Ten).FirstOrDefault();
                        nCol++;
                        var NguonTuyenDung = listNguonTuyenDung.FirstOrDefault(a => a.IdGuid == item.IddmNguonTuyenDung);
                        if (NguonTuyenDung != null)
                            sheet1.Cells[Row, nCol].Value = NguonTuyenDung.Ten;
                        nCol++;
                        var CongTy = listCongTy.FirstOrDefault(a => a.Id == item.IdCoSoTrungTuyen);
                        if (CongTy != null)
                            sheet1.Cells[Row, nCol].Value = CongTy.TenBoPhan;
                        nCol++;
                        if (item.NgayNhanViecDuKien != null && item.NgayNhanViecDuKien.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            sheet1.Cells[Row, nCol].Value = item.NgayNhanViecDuKien.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[Row, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[Row, nCol].Value = item.ThuCoc;
                        sheet1.Cells[Row, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;
                        nCol++;
                        nCol++;
                        sheet1.Cells[Row, nCol].Value = item.GhiChu;

                        STT++;
                        Row++;
                    }
                    Row--;
                    dungchung.DrawTableExcel(sheet1, 4, Row, 1, nCol);
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet1.Cells[9, 6, Row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells[9, 10, Row, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells[9, 13, Row, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }


                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                    + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception ex)
            {

                return ex.ToString();
            }
        }
        public dynamic ExportDanhSachNhanVienTrungTuyenThuCoc(int Thang, int Nam, bool isCongNhan, ApplicationDbContext db)
        {
            string sFile = "29.07.2025 Mẫu PT.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "mau-phieu-thu-C20-BB.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            //var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listCongTy = _CallWebService.GetDanhSachDuAn(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).ToList();
            int Row = 9;

            var listDataNhanVienUngTuyens = db.HRM_TuyenDung_UngVien.Where(a => a.isCongNhan == isCongNhan && a.NgayNhanViecDuKien != null && a.NgayNhanViecDuKien.Value.Year == Nam && a.NgayNhanViecDuKien.Value.Month == Thang && a.TrangThai == 3 && a.ThuCoc > 0).Distinct().ToList();
            if (listDataNhanVienUngTuyens != null && listDataNhanVienUngTuyens.Count > 0)
            {
                var sheetM = package.Workbook.Worksheets["Mẫu"];
                List<string> listIdUngVien = listDataNhanVienUngTuyens.Select(a => a.IdGuid).Distinct().ToList();
                var listPCUngVien = db.HRM_UngVien_PhuCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).Distinct().ToList();
                var listBCUngVien = db.HRM_UngVienBangCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).Distinct().ToList();

                Row++;
                foreach (var item in listDataNhanVienUngTuyens)
                {
                    var sheet1 = package.Workbook.Worksheets.Add(item.MaNhanVien, sheetM);

                    var CongTy = listCongTy.FirstOrDefault(a => a.Id == item.IdCoSoTrungTuyen);
                    if (CongTy != null)
                    {
                        sheet1.Cells[1, 2].Value = CongTy.TenBoPhan;
                        sheet1.Cells[37, 2].Value = CongTy.TenBoPhan;
                    }

                    sheet1.Cells[1, 9].Value = "Số phiếu thu: " + item.MaNhanVien;
                    sheet1.Cells[37, 9].Value = "Số phiếu thu: " + item.MaNhanVien;
                    sheet1.Cells[5, 3].Value = item.HoTenDayDu;
                    sheet1.Cells[41, 3].Value = item.HoTenDayDu;
                    sheet1.Cells[22, 6].Value = item.HoTenDayDu;
                    sheet1.Cells[58, 6].Value = item.HoTenDayDu;
                    sheet1.Cells[6, 2].Value = item.DiaChi;
                    sheet1.Cells[42, 2].Value = item.DiaChi;
                    sheet1.Cells[9, 2].Value = string.Format("{0:0,0 VNĐ}", item.ThuCoc).Replace(".", ",");
                    sheet1.Cells[45, 2].Value = string.Format("{0:0,0 VNĐ}", item.ThuCoc).Replace(".", ",");
                    sheet1.Cells[9, 4].Value = dungchung.NumberToText(item.ThuCoc ?? 0) + " đồng";
                    sheet1.Cells[45, 4].Value = dungchung.NumberToText(item.ThuCoc ?? 0) + " đồng";
                    sheet1.Cells[9, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells[9, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet1.Cells[45, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet1.Cells[45, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    if (item.NgayTiepNhan != null && item.NgayTiepNhan.Value.Year > 2020)
                    {
                        sheet1.Cells[2, 4].Value = "Ngày " + item.NgayTiepNhan.Value.Day + " tháng " + item.NgayTiepNhan.Value.Month + " năm " + item.NgayTiepNhan.Value.Year;
                        sheet1.Cells[38, 4].Value = "Ngày " + item.NgayTiepNhan.Value.Day + " tháng " + item.NgayTiepNhan.Value.Month + " năm " + item.NgayTiepNhan.Value.Year;
                        sheet1.Cells[10, 8].Value = "Ngày " + item.NgayTiepNhan.Value.Day + " tháng " + item.NgayTiepNhan.Value.Month + " năm " + item.NgayTiepNhan.Value.Year;
                        sheet1.Cells[46, 8].Value = "Ngày " + item.NgayTiepNhan.Value.Day + " tháng " + item.NgayTiepNhan.Value.Month + " năm " + item.NgayTiepNhan.Value.Year;
                        sheet1.Cells[23, 4].Value = item.NgayTiepNhan.Value.ToString("MM/yyyy");
                        sheet1.Cells[59, 4].Value = item.NgayTiepNhan.Value.ToString("MM/yyyy");

                    }


                }

                package.Workbook.Worksheets.Delete(sheetM);
            }


            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        #endregion
        #region Sơ vấn 

        public dynamic GetDanhSachNhanVienSoVanUpdate(ApplicationDbContext db)
        {
            var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => !string.IsNullOrEmpty(a.IdNhuCauChiTiet)).ToList();
            List<string> IdUngVien = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
            var listPV = db.HRM_TuyenDung_QuyTrinhPhongVan.Where(a => IdUngVien.Contains(a.IdUngVien)).ToList();
            var nhuCaus = db.HRM_TuyenDung_NhuCau.Where(a => a.isKetThuc).ToList();
            List<string> IdNhuCau = nhuCaus.Select(a => a.IdGuid).Distinct().ToList();
            var NhuCauChiTiets = db.HRM_TuyenDung_NhuCau_ChiTiet.Where(a => IdNhuCau.Contains(a.IdTuyenDungNhuCau)).ToList();
            foreach (var item in listUngVien)
            {
                var NhuCauChiTiet = NhuCauChiTiets.FirstOrDefault(a => a.IdGuid == item.IdNhuCauChiTiet);
                if (NhuCauChiTiet != null)
                {
                    var NhuCau = nhuCaus.FirstOrDefault(a => a.IdGuid == NhuCauChiTiet.IdTuyenDungNhuCau);
                    item.IdBoPhanEOS = NhuCauChiTiet.IdBoPhanEOS;
                    item.IdCongTyEOS = NhuCau.IdCongTyEOS;
                    var PV = listPV.FirstOrDefault(a => a.IdUngVien == item.IdGuid);
                    if (PV != null)
                    {
                        PV.IdCongTyEOS = item.IdCongTyEOS ?? 0;
                        db.Entry(PV).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }

                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                };

            }
            db.SaveChanges();
            return "0";
        }
        public dynamic GetDanhSachNhanVienSoVan(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            string KeyWord = dyn.KeyWord;
            int nCurrentPage = dyn.CurrentPage ?? 0;
            int PageSize = dyn.PageSize ?? 25;
            int TrangThai = dyn.TrangThai ?? 0;
            long IddmChucDanh = dyn.IddmChucDanh ?? 0;
            // 0 chưa sở vấn - 2 đã sơ vấn  


            var listBoPhan = _FuncNhanVien.GetCongTyByIdNhanVien(db);
            List<long> IdCongTyKiemNhiem = new List<long>();
            if (listBoPhan != null && listBoPhan.Count > 0)
            {
                IdCongTyKiemNhiem = listBoPhan.Select(a => a.Id).Distinct().ToList();
                IdCongTyKiemNhiem.AddRange(listBoPhan.Select(a => a.BoPhanChaPkid).Distinct().ToList());

            }
            IQueryable<HRM_TuyenDung_UngVien> result;

            result = from a in db.HRM_TuyenDung_UngVien
                     where ((TimKiem.TrangThai == 4 && (a.TrangThai == 4)) || a.TrangThai == TimKiem.TrangThai)
                     //&& (IdCongTyKiemNhiem.Count == 0 || (IdCongTyKiemNhiem.Contains(a.IdCongTyEOS ?? 0) || IdCongTyKiemNhiem.Contains(a.IdCoSoTrungTuyen ?? 0) || IdCongTyKiemNhiem.Contains(a.IdBoPhanEOS ?? 0)))
                    && a.isCongNhan == TimKiem.isCongNhan
                     orderby a.MaUngVien descending
                     select a;


            if (TimKiem.TuNgay > 0)
            {
                DateTime? Ngay = dungchung.UnixTimeStampToDateTime(TimKiem.TuNgay);
                result = result.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.ToString("dd/MM/yyyy") != "01/01/1970" && a.NgayTiepNhan >= Ngay);
            }
            if (TimKiem.DenNgay > 0)
            {
                DateTime? Ngay = dungchung.UnixTimeStampToDateTime(TimKiem.DenNgay);
                result = result.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.ToString("dd/MM/yyyy") != "01/01/1970" && a.NgayTiepNhan <= Ngay);
            }

            if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                result = result.Where(a => ((!string.IsNullOrEmpty(a.MaUngVien) && a.MaUngVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())))
                || ((!string.IsNullOrEmpty(a.HoTenDayDu) && a.HoTenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())))
                || ((!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())))
                || ((!string.IsNullOrEmpty(a.DienThoaiDiDong) && a.DienThoaiDiDong.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()))));

            if (TimKiem.IdBoPhan > 0)
                result = result.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhan);
            if (TimKiem.IdCongTy > 0)
                result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
            if (TimKiem.IddmChucDanh > 0)
                result = result.Where(a => a.IddmChucDanhUngTuyen == TimKiem.IddmChucDanh);

            var itemList = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);

            List<string> listIdTopHopDenNghi = itemList.Select(r => r.IdTongHopDeNghiTuyenDung).ToList();
            List<HRM_TuyenDung_DeNghi_TongHop> listTopHopDenNghi = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => listIdTopHopDenNghi.Contains(r.IdGuid)).ToList();
            List<string> listIdUngVien = itemList.Select(r => r.IdGuid).ToList();
            List<HRM_TuyenDung_UngVienKinhNghiemLamViec> UngVienKinhNghiemLamViecs = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(r => listIdUngVien.Contains(r.IdTuyenDungUngVien)).ToList();
            List<HRM_TuyenDung_UngVien_SoVanTieuChi> listSoVanTieuChi = db.HRM_TuyenDung_UngVien_SoVanTieuChi.Where(r => listIdUngVien.Contains(r.IdUngVien)).ToList();
            List<HRM_TuyenDung_UngVien_SoVanChiTiet> listSoVanTieuChiTiet = db.HRM_TuyenDung_UngVien_SoVanChiTiet.Where(r => listIdUngVien.Contains(r.IdUngVien)).ToList();
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
            foreach (var item in itemList)
            {
                item.TenChucDanhUngTuyen = listChucDanh.Where(r => r.Id == item.IddmChucDanhUngTuyen).Select(r => r.Ten).FirstOrDefault();
                item.TendmTrinhDoHocVan = listTrinhDo.Where(r => r.IdGuid == item.IddmTrinhDoHocVan).Select(r => r.Ten).FirstOrDefault();
                item.lstKinhNghiemLamViec = UngVienKinhNghiemLamViecs.Where(r => r.IdTuyenDungUngVien == item.IdGuid).ToList();
                item.listSoVanTieuChi = listSoVanTieuChi.Where(r => r.IdUngVien == item.IdGuid).OrderBy(a => a.SapXep).ToList();
                item.TenChucVu = listChucVu.Where(r => r.Id == item.IdChucVu).Select(r => r.TenChucVu).FirstOrDefault();
                if (item.listSoVanTieuChi != null && item.listSoVanTieuChi.Count > 0)
                {
                    foreach (var child in item.listSoVanTieuChi)
                        child.listSoVanChiTiet = listSoVanTieuChiTiet.Where(r => r.IdUngVien == item.IdGuid).OrderBy(a => a.SapXep).ToList();
                }
                if (item.NgaySinh != null && item.NgaySinh.Value.ToString("dd/MM/yyyy") == "01/01/1970")
                    item.NgaySinh = null;
                if (item.NgayTiepNhan != null)
                {
                    if (item.NgayTiepNhan.Value.ToString("dd/MM/yyyy") == "01/01/1970")
                    {
                        item.NgayTiepNhanUnix = 0;
                        item.NgayTiepNhan = null;
                    }
                    else
                        item.NgayTiepNhanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTiepNhan);
                }
                if (item.NgayNhanViecDuKien != null)
                {
                    if (item.NgayNhanViecDuKien.Value.ToString("dd/MM/yyyy") == "01/01/1970")
                    {
                        item.NgayNhanViecDuKienUnix = 0;
                        item.NgayNhanViecDuKien = null;
                    }
                    else
                        item.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayNhanViecDuKien);
                }

                if (item.NgaySoVan != null)
                {
                    if (item.NgaySoVan.Value.ToString("dd/MM/yyyy") == "01/01/1970")
                        item.NgaySoVanUnix = 0;
                    else
                        item.NgaySoVanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySoVan);
                }
                item.IdTongHopDeNghiTuyenDung = item.IdQuyTrinhNhuCau ?? item.IdNhuCauChiTiet; ;
                // item.TrinhDoHocVan= db.HRM_dmTrinhDoDaoTao.Where(r => r.IdGuid == item.IddmTrinhDoHocVan).Select(r=>r.Ten).FirstOrDefault();
            }


            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = itemList;
            return dnm;
        }
        public dynamic SetSoVanUngVien(HRM_TuyenDung_UngVien_SoVan model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    if (model.TrangThai == 3)
                        model.TrangThai = 2;
                    model.NgaySoVan = UnixTimeStampToDateTime(model.NgaySoVanUnix);
                    if (string.IsNullOrEmpty(model.IdGuid))
                    {
                        model.IdGuid = Guid.NewGuid().ToString();
                        db.HRM_TuyenDung_UngVien_SoVan.Add(model);
                    }
                    else
                    {
                        db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    var listSoVanTieuChiOld = db.HRM_TuyenDung_UngVien_SoVanTieuChi.Where(r => r.IdUngVien == model.IdUngVien).ToList();
                    db.HRM_TuyenDung_UngVien_SoVanTieuChi.RemoveRange(listSoVanTieuChiOld);

                    var listSoVanChiTietOld = db.HRM_TuyenDung_UngVien_SoVanChiTiet.Where(r => r.IdUngVien == model.IdUngVien).ToList();
                    db.HRM_TuyenDung_UngVien_SoVanChiTiet.RemoveRange(listSoVanChiTietOld);

                    if (model.listSoVanTieuChi != null && model.listSoVanTieuChi.Count > 0)
                    {
                        int Stt = 1;
                        foreach (var item in model.listSoVanTieuChi)
                        {
                            item.IdGuid = Guid.NewGuid().ToString();
                            item.IdTuyenDungSoVan = model.IdGuid;
                            item.IdUngVien = model.IdUngVien;
                            item.SapXep = Stt;
                            Stt++;
                            db.HRM_TuyenDung_UngVien_SoVanTieuChi.Add(item);
                            if (item.listSoVanChiTiet != null && item.listSoVanChiTiet.Count > 0)
                            {
                                foreach (var itemTC in item.listSoVanChiTiet)
                                {
                                    itemTC.IdGuid = Guid.NewGuid().ToString();
                                    itemTC.IdUngVien = model.IdUngVien;
                                    itemTC.IdTuyenDungSoVan = model.IdGuid;
                                    itemTC.IdSoVanTieuChi = item.IdGuid;
                                    itemTC.SapXep = Stt;

                                    db.HRM_TuyenDung_UngVien_SoVanChiTiet.Add(itemTC);
                                    Stt++;
                                }
                            }
                        }
                    }

                    HRM_TuyenDung_UngVien nhanVien = db.HRM_TuyenDung_UngVien.Find(model.IdUngVien);
                    if (nhanVien != null && model.TrangThai != 0)
                    {
                        if (model.TrangThai == 4)
                            nhanVien.TenTrangThai = "Ứng viên không qua vòng sơ vấn";
                        nhanVien.TrangThai = model.TrangThai;
                    }
                    nhanVien.GhiChu = model.GhiChu;
                    nhanVien.NgaySoVan = model.NgaySoVan;

                    db.Entry(nhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                    ts.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thông tin ứng viên thành công ");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình xử lý " + ex.Message);
                }

            }
        }
        public dynamic GetSoVanUngVienById(string Id, ApplicationDbContext db)
        {
            var model = db.HRM_TuyenDung_UngVien_SoVan.Where(r => r.IdUngVien == Id).FirstOrDefault();
            model.NgaySoVanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySoVan);
            List<HRM_TuyenDung_UngVien_SoVanTieuChi> listSoVanTieuChi = db.HRM_TuyenDung_UngVien_SoVanTieuChi.Where(r => r.IdTuyenDungSoVan == Id).ToList();

            model.listSoVanTieuChi = listSoVanTieuChi;
            return model;
        }
        #endregion
        #region phỏng vấn 
        public dynamic GetListLichPhongVan(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            string KeyWord = TimKiem.KeyWord;
            int nCurrentPage = dyn.CurrentPage ?? 0;
            int PageSize = dyn.PageSize ?? 25;
            int TrangThai = dyn.TrangThai ?? 0;
            List<string> listIdUngVien = new List<string>();
            List<HRM_TuyenDung_UngVien> listUngVien = new List<HRM_TuyenDung_UngVien>();
            if (TimKiem.TuNgay > 0 && TimKiem.DenNgay > 0)
            {
                DateTime? TuNgay = dungchung.UnixTimeStampToDateTime(TimKiem.TuNgay);
                DateTime? DenNgay = dungchung.UnixTimeStampToDateTime(TimKiem.DenNgay);
                listUngVien = db.HRM_TuyenDung_UngVien.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.Year > 1970 && a.NgayTiepNhan >= TuNgay && a.NgayTiepNhan <= DenNgay).ToList();
                listIdUngVien = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
            }
            else
            if (TimKiem.TuNgay > 0)
            {
                DateTime? Ngay = dungchung.UnixTimeStampToDateTime(TimKiem.TuNgay);
                listUngVien = db.HRM_TuyenDung_UngVien.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.Year > 1970 && a.NgayTiepNhan >= Ngay).ToList();
                listIdUngVien = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
            }
            else
            if (TimKiem.DenNgay > 0)
            {
                DateTime? Ngay = dungchung.UnixTimeStampToDateTime(TimKiem.DenNgay);
                listUngVien = db.HRM_TuyenDung_UngVien.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.Year > 1970 && a.NgayTiepNhan <= Ngay).ToList();
                listIdUngVien = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
            }
            //var listBoPhan = _FuncNhanVien.GetCongTyByIdNhanVien(db);
            var listPhongBanUser = _CallWebService.GetDanhSachBoPhanByUser(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            List<long> listIdBoPhan = new List<long>();

            if (listPhongBanUser != null)
            {
                string Host = (HrmOptions?.Host ?? "");
                if (Host == "ICT")
                {
                    string MaBoPhan = listPhongBanUser.FirstOrDefault().MaBoPhan.Substring(0, 2);
                    foreach (var item in listBoPhan)
                    {
                        if (item.MaBoPhan.Length < 2)
                            continue;
                        if (item.MaBoPhan.Substring(0, 2) == MaBoPhan)
                            listIdBoPhan.Add(item.Id);
                    }
                }

            }

            //if (listBoPhan != null && listBoPhan.Count > 0)
            //{
            //    IdCongTyKiemNhiem = listBoPhan.Select(a => a.Id).Distinct().ToList();
            //    IdCongTyKiemNhiem.AddRange(listBoPhan.Select(a => a.BoPhanChaPkid).Distinct().ToList());

            //}
            List<string> listIdUngVienSDT = new List<string>();
            if (!string.IsNullOrEmpty(KeyWord))
            {
                var listUngVienSDT = db.HRM_TuyenDung_UngVien.Where(a => (a.DienThoaiDiDong.ToLower().Contains(KeyWord.ToLower().Trim())) || (a.HoTenDayDu.ToLower().Contains(KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.DotTuyenDung) && a.DotTuyenDung.ToLower().Contains(KeyWord.ToLower().Trim()))).ToList();
                if (listUngVienSDT != null)
                    listIdUngVienSDT = listUngVienSDT.Select(a => a.IdGuid).Distinct().ToList();
            }
            List<HRM_TuyenDung_QuyTrinhPhongVan> result = (from a in db.HRM_TuyenDung_QuyTrinhPhongVan
                                                           where ((TrangThai != 0 && TrangThai != 5 && a.TrangThai == TrangThai) || ((TrangThai == 0 || TrangThai == 5) && (a.TrangThai == 0 || a.TrangThai == 5)))
                                                           && ((string.IsNullOrEmpty(KeyWord)) || (a.SoPhieu.ToLower().Contains(KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.DotTuyenDung) && a.DotTuyenDung.ToLower().Contains(KeyWord.ToLower().Trim())) || listIdUngVienSDT.Contains(a.IdUngVien))
                                                           && (listIdBoPhan.Count == 0 || ((a.IdBoPhan == 0 && listIdBoPhan.Contains(a.IdBoPhan ?? 0)) || (a.IdBoPhan > 0 && listIdBoPhan.Contains(a.IdBoPhan ?? 0))))
                                                           && a.isCongNhan == TimKiem.isCongNhan
                                                           orderby a.MaUngVien descending
                                                           select a).ToList();
            if (TimKiem.TuNgay > 0 || TimKiem.DenNgay > 0)
            {
                result = result.Where(a => listIdUngVien.Contains(a.IdUngVien)).ToList();
            }
            if (TimKiem.IddmChucDanh != null && TimKiem.IddmChucDanh > 0)
            {
                result = result.Where(a => a.IdChucDanhUngTuyen == (TimKiem.IddmChucDanh ?? 0)).ToList();
            }
            var itemList = result.ToList();
            if (TimKiem.TrangThai == 10)
            {
                if (listIdUngVien.Count == 0)
                {
                    listIdUngVien = itemList.Select(a => a.IdUngVien).Distinct().ToList();
                    listUngVien = db.HRM_TuyenDung_UngVien.Where(a => listIdUngVien.Contains(a.IdGuid)).ToList();
                }
                foreach (var item in itemList)
                {
                    var UngVien = listUngVien.FirstOrDefault(a => a.IdGuid == item.IdUngVien);
                    if (UngVien != null)
                    {
                        if (UngVien.NgayTiepNhan != null && UngVien.NgayTiepNhan.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                        {
                            item.NgayTiepNhan = UngVien.NgayTiepNhan.Value.ToString("dd/MM/yyyy");
                            item.NgayTiepNhand = UngVien.NgayTiepNhan;
                        }
                    }

                }
                itemList = itemList.OrderByDescending(a => a.NgayTiepNhand).ToList();
            }
              
            int nSum = itemList.Count;
            int nTotalPage = CalculateTotalPages(nSum, PageSize);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            if (itemList != null && itemList.Count > 0)
            {
                if (listIdUngVien.Count == 0)
                {
                    listIdUngVien = itemList.Select(a => a.IdUngVien).Distinct().ToList();
                    listUngVien = db.HRM_TuyenDung_UngVien.Where(a => listIdUngVien.Contains(a.IdGuid)).ToList();
                }
                foreach (var item in itemList)
                {
                    var UngVien = listUngVien.FirstOrDefault(a => a.IdGuid == item.IdUngVien);
                    if (UngVien != null)
                    {
                        item.MaNhanVien = UngVien.MaNhanVien;
                        item.IsNhanViec = UngVien.IsNhanViec;
                        if (UngVien.NgayTiepNhan != null && UngVien.NgayTiepNhan.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                        {
                            item.NgayTiepNhan = UngVien.NgayTiepNhan.Value.ToString("dd/MM/yyyy");
                            item.NgayTiepNhand = UngVien.NgayTiepNhan;
                        }
                        item.HoTenUngVien = UngVien.HoTenDayDu;
                    }

                    item.TenChucDanhUngTuyen = listChucDanh.Where(r => r.Id == item.IdChucDanhUngTuyen).Select(r => r.Ten).FirstOrDefault();
                    item.TenChucVu = listChucVu.Where(r => r.Id == item.IdChucVu).Select(r => r.TenChucVu).FirstOrDefault();
                    if (item.IddmTrungTuyen > 0)
                        item.TenChucDanhTrungTuyen = listChucDanh.Where(r => r.Id == item.IddmTrungTuyen).Select(r => r.Ten).FirstOrDefault();

                    item.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTao);
                    //var TongHopDeNghi = (from a in db.HRM_TuyenDung_UngVien
                    //                     join b in db.HRM_TuyenDung_DeNghi_TongHop on a.IdTongHopDeNghiTuyenDung equals b.IdGuid
                    //                     where a.IdGuid == item.IdUngVien
                    //                     select b).FirstOrDefault();
                    //if (TongHopDeNghi != null)
                    //    item.TongHopDeNghiTuyenDung = TongHopDeNghi;
                    if (item.TrangThai == 0)
                        item.TenTrangThai = "Lưu nháp";
                    if (item.TrangThai == 5)
                        item.TenTrangThai = "Chờ phỏng vấn";
                    if (item.TrangThai == 10)
                        item.TenTrangThai = "Phỏng vấn đạt";
                    if (item.TrangThai == 15)
                        item.TenTrangThai = "Phỏng vấn không đạt";
                }
            }
            itemList=itemList.OrderByDescending(a => a.SoPhieu).ToList();
            if(TimKiem.TrangThai==10)
                itemList = itemList.OrderByDescending(a => a.NgayTiepNhand).ToList();

            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = itemList;
            return dnm;
        }
        public dynamic SetLichPhongVanNhieuUngVien(LichPhongVanViewModel model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    var listIdUngVien = model.listIdUngVien;
                    var listUngVien = db.HRM_TuyenDung_UngVien.Where(r => listIdUngVien.Contains(r.IdGuid)).ToList();
                    foreach (var item in listUngVien)
                    {

                        HRM_TuyenDung_QuyTrinhPhongVan lichPv = new HRM_TuyenDung_QuyTrinhPhongVan
                        {
                            IdGuid = Guid.NewGuid().ToString(),
                            MaUngVien = item.MaUngVien,
                            HoTenUngVien = item.HoTenDayDu,
                            IdChucDanhUngTuyen = item.IddmChucDanhUngTuyen,
                            IdNguoiTao = db.IdUser,
                            NgayTao = DateTime.Now,
                            TrangThai = 0,
                            IdUngVien = item.IdGuid,
                            SoPhieu = GetNextMaQuyTrinhPhongVan(db),
                        };
                        db.HRM_TuyenDung_QuyTrinhPhongVan.Add(lichPv);
                        foreach (var vongPV in model.listVongPhongVan)
                        {
                            vongPV.ThoiGianPhongVan = dungchung.UnixTimeStampToDateTime(vongPV.ThoiGianPhongVanUnix);
                            HRM_TuyenDung_VongPhongVan vongPhongVan = new HRM_TuyenDung_VongPhongVan
                            {
                                IdGuid = Guid.NewGuid().ToString(),
                                IdQuyTrinhPhongVan = item.IdGuid,
                                DiaDiem = vongPV.DiaDiem,
                                IdNguoiChuTri = vongPV.IdChuTri,
                                VongPhongVan = vongPV.VongPhongVan,

                            };
                            foreach (var itemNguoiThamGia in vongPV.listIdNguoiThamGia)
                            {
                                HRM_TuyenDung_NguoiPhongVan itemNTG = new HRM_TuyenDung_NguoiPhongVan();
                                itemNTG.IdGuid = Guid.NewGuid().ToString();
                                itemNTG.IdVongPhongVan = vongPhongVan.IdGuid;
                                itemNTG.IdUngVien = item.IdGuid;

                                db.HRM_TuyenDung_NguoiPhongVan.Add(itemNTG);
                            }
                        }
                        HRM_TuyenDung_UngVien ungVien = db.HRM_TuyenDung_UngVien.Find(lichPv.IdUngVien);
                        if (ungVien != null)
                        {
                            ungVien.IdLichPhongVan = lichPv.IdGuid;
                            ungVien.TrangThai = 3;
                            db.Entry(ungVien).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }

                    ts.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
                }
            }
        }
        public dynamic SetQuyTrinhDatLichPhongVan(HRM_TuyenDung_QuyTrinhPhongVan model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    string check = FuncUpdateDatLichPhongVan(model, db);
                    if (check != "0")
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + check);
                    }
                    //if (model.TrangThai == 5)
                    //{
                    //    HRM_TuyenDung_VongPhongVan VongPhongVan1 = model.listVongPhongVan.Where(r => r.TrangThai == 0).OrderBy(r => r.VongPhongVan).FirstOrDefault();

                    //}
                    db.SaveChanges();


                    ts.Commit();
                    try
                    {
                        if (model.TrangThai == 5)
                        {
                            HRM_TuyenDung_VongPhongVan VongPhongVan = model.listVongPhongVan.Where(r => r.TrangThai == 0).OrderBy(r => r.VongPhongVan).FirstOrDefault();
                            VongPhongVan.TrangThai = 5;
                            if (VongPhongVan != null)
                            {
                                var listNguoiThamGia = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdVongPhongVan == VongPhongVan.IdGuid).ToList();
                                foreach (var item in listNguoiThamGia)
                                {
                                    long IdNhanVienEOS = db.HRM_NhanVien.Where(r => r.IdGuid == item.IdNguoiThamGia).Select(r => r.IdNhanVienEOS).FirstOrDefault();
                                    string IdUserEOS = _CallWebService.GetNhanSu_IdUserEOS(db, IdNhanVienEOS);
                                    if (IdUserEOS != null && IdUserEOS != "")
                                    {
                                        string Vitri = _CallWebService.GetChucDanh_Ten(db, model.IdChucDanhUngTuyen);
                                        AddNotificationNoUserModel notification = new AddNotificationNoUserModel();
                                        List<string> listNguoiNhan = new List<string>();
                                        listNguoiNhan.Add(IdUserEOS);
                                        notification.NguoiGui = db.IdUser;
                                        notification.listNguoiNhan = listNguoiNhan;

                                        NotificationNoUserModel notificationModel = new NotificationNoUserModel
                                        {
                                            TieuDe = "Bạn nhận được 1 lịch mời tham gia phỏng vấn vào thời gian " + VongPhongVan.NgayPhongVan.ToString("dd/MM/yyyy"),
                                            IdQuyTrinh = VongPhongVan.IdGuid,
                                            LoaiThongBao = "LICHMOIPHONGVAN",
                                            NoiDung = " Phỏng vấn ứng viên: " + model.HoTenUngVien + " ứng tuyển vị trí " + Vitri,

                                        };
                                        notification.Item = notificationModel;
                                        AddNotification(notification);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception)
                    {


                    }



                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
                }
            }
        }
        public dynamic PheDuyetNhieuQuyTrinhDatLichPhongVanDat(List<string> listId, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    List<HRM_TuyenDung_QuyTrinhPhongVan> listTuyenDung = db.HRM_TuyenDung_QuyTrinhPhongVan.Where(x => listId.Contains(x.IdUngVien)).ToList();
                    listTuyenDung = listTuyenDung.FindAll(x => x.IdBoPhan != null && x.IdBoPhan != 0);
                    List<HRM_TuyenDung_UngVien> listUngVien = db.HRM_TuyenDung_UngVien.Where(x => listId.Contains(x.IdGuid)).ToList();
                    var listIdLNV = db.HRM_dmLoaiNhanVien.Where(a => a.Ma == "TV." || a.Ma == "TVN").Select(a => a.IdGuid).Distinct().ToList();
                    foreach (var ungVien in listUngVien)
                    {
                        ungVien.TrangThai = 3;
                        var model = listTuyenDung.FirstOrDefault(a => a.IdUngVien == ungVien.IdGuid);
                        if (model != null)
                        {
                            model.TrangThai = 10;
                            db.Entry(model).State = EntityState.Modified;




                        }
                        else
                        {
                            model = new HRM_TuyenDung_QuyTrinhPhongVan();
                            model.TrangThai = 10;
                            model.IdGuid = Guid.NewGuid().ToString();
                            ungVien.IdLichPhongVan = model.IdGuid;
                            model.IdUngVien = ungVien.IdGuid;
                            model.MaUngVien = ungVien.MaUngVien;
                            model.HoTenUngVien = ungVien.HoTenDayDu;
                            model.DotTuyenDung = ungVien.DotTuyenDung;
                            if (!string.IsNullOrEmpty(ungVien.IdLoaiNhanVien))
                            {

                                model.IdLoaiNhanVien = ungVien.IdLoaiNhanVien;
                                if (listIdLNV.Contains(ungVien.IdLoaiNhanVien))
                                    model.LoaiCongNhan = "Thời vụ/Thời vụ ngày";
                                else

                                    model.LoaiCongNhan = "Bảo trì";
                            }
                            model.IdTongHopDeNghiTuyenDung = ungVien.IdQuyTrinhNhuCau;
                            model.NgayNhanViecDuKien = ungVien.NgayNhanViecDuKien;
                            model.isCongNhan = ungVien.isCongNhan;
                            model.IdCongTyEOS = ungVien.IdCongTyEOS ?? 0;
                            model.IdCoSoTrungTuyen = ungVien.IdCoSoTrungTuyen ?? 0;
                            model.IdBoPhan = ungVien.IdBoPhanEOS;
                            model.IdCaLamViec = ungVien.IdCaLamViec;
                            model.IdCapBacLuongDeXuat = ungVien.IdCapBacLuongDeXuat;
                            model.Luong = ungVien.Luong;
                            model.HinhThucTraLuongDaoTao = ungVien.HinhThucTraLuongDaoTao;
                            model.IdLoaiNhanVien = ungVien.IdLoaiNhanVien;
                            model.IdLoaiHopDong = ungVien.IdLoaiHopDong;
                            model.ThoiGianThuViec = ungVien.ThoiGianThuViec;
                            model.IdChucVu = ungVien.IdChucVu;
                            model.IdChucDanhUngTuyen = ungVien.IddmChucDanhUngTuyen;
                            model.PhanTramLuongThuViec = ungVien.PhanTramLuongThuViec;
                            model.ThuCoc = ungVien.ThuCoc;
                            model.IdNhanVienGioiThieu = ungVien.IdNhanVienGioiThieu;
                            db.Entry(model).State = EntityState.Added;

                        }
                        double Luong = model.Luong;
                        var LuongTuyenDung = db.HRM_DeNghiTuyenDung_CongNhan.FirstOrDefault(a => a.IdQuyTrinh == ungVien.IdQuyTrinhNhuCau && model.LoaiCongNhan == a.Loai);
                        if (LuongTuyenDung != null && LuongTuyenDung.TongLuong >= Luong)
                        {
                            var fcNhanVien = _FuncNhanVien;
                            var fcHopDong = _funcHopDong;
                            ungVien.isDaPheDuyetLuong = true;
                            HRM_NhanVien NhanVien = new HRM_NhanVien(ungVien);
                            try
                            {
                                NhanVien.MaNhanVien = fcNhanVien.GetMaNhanVien(db);
                            }
                            catch
                            {

                            }
                            if (ungVien.ThuCoc != null && ungVien.ThuCoc > 0)
                            {
                                NhanVien.ThuCoc = ungVien.ThuCoc;
                                NhanVien.IsDaThuCoc = false;
                            }
                            NhanVien.NgayNhanViecDuKien = model.NgayNhanViecDuKien;
                            NhanVien.NgayNhanViecDuKienUnix = model.NgayNhanViecDuKienUnix;
                            NhanVien.IdCaLamViec = model.IdCaLamViec;
                            NhanVien.IdCapBacLuongChapNhan = model.IdCapBacLuongDeXuat;
                            NhanVien.IdCapBacLuongDeXuat = model.IdCapBacLuongDeXuat;
                            NhanVien.IdBoPhanEOS = model.IdBoPhan ?? 0;
                            NhanVien.listIdCongTy = new List<long>();
                            NhanVien.listIdCongTy.Add(ungVien.IdCoSoTrungTuyen ?? 0);
                            NhanVien.ThoiGianThuViec = model.ThoiGianThuViec;
                            NhanVien.IdLoaiNhanVien = model.IdLoaiNhanVien;
                            if (!string.IsNullOrEmpty(ungVien.MaNhanVien))
                                NhanVien.MaNhanVien = ungVien.MaNhanVien;
                            NhanVien.IdChucDanh = model.IddmTrungTuyen;
                            NhanVien.IdChucVu = model.IdChucVu;
                            NhanVien.IdUngVien = ungVien.IdGuid;
                            NhanVien.IdLoaiNhanVien = model.IdLoaiNhanVien;
                            NhanVien.IddmTrinhDo = ungVien.IddmTrinhDoHocVan;
                            NhanVien.PhanTramLuongThuViec = model.PhanTramLuongThuViec;
                            NhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                            NhanVien.listPhuCap = new List<HRM_NhanVien_PhuCap>();
                            NhanVien.Luong = model.Luong;
                            NhanVien.listGiayToTuyThan = new List<HRM_NhanVien_GiayToTuyThan>();
                            NhanVien.listGiayToTuyThan.Add(new HRM_NhanVien_GiayToTuyThan()
                            {
                                Loai = 1,
                                So = ungVien.CCCD,
                                NoiCap = ungVien.NoiCap,
                                NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(ungVien.NgayCap),
                                isApDung = true
                            });
                            NhanVien.listSoDienThoai = new List<HRM_NhanVien_SoDienThoai>();
                            NhanVien.listSoDienThoai.Add(new HRM_NhanVien_SoDienThoai()
                            {
                                SoDienThoai = ungVien.DienThoaiDiDong,
                                isApDung = true
                            });
                            NhanVien.listEmail = new List<HRM_NhanVien_Email>();
                            NhanVien.listEmail.Add(new HRM_NhanVien_Email()
                            {
                                Email = ungVien.Email,
                                isApDung = true
                            });
                            var listKinhNghiemUngVien = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(a => a.IdTuyenDungUngVien == ungVien.IdGuid).ToList();
                            if (listKinhNghiemUngVien != null && listKinhNghiemUngVien.Count > 0)
                            {
                                NhanVien.ListQuaTrinhCongTacNgoaiCongTy = listKinhNghiemUngVien.Select(a => new HRM_QuaTrinhCongTacNgoaiCongTy
                                {
                                    ThoiGian = a.ThoiGianLamViec,
                                    DonViCongTac = a.DonViCongTac,
                                    ViTriCongViec = a.ViTri
                                }).ToList();

                            }
                            var listChungChiUngVien = db.HRM_ChungChiUngVien.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                            if (listChungChiUngVien != null && listChungChiUngVien.Count > 0)
                            {
                                NhanVien.ListNhanVienChungChi = listChungChiUngVien.Select(a => new HRM_ChungChiNhanVien
                                {
                                    TenChungChi = a.TenChungChi,
                                    IdNhomChungChi = a.IdNhomChungChi,
                                    TrinhDo = a.TrinhDo,
                                    NgayCap = a.NgayCap,
                                    NgayHetHan = a.NgayHetHan,
                                    NgayCapUnix = DateTimeToUnixTimeStamp(a.NgayCap),
                                    NgayHetHanUnix = DateTimeToUnixTimeStamp(a.NgayHetHan),
                                    NoiCap = a.NoiCap,
                                    lstFileDinhKem = a.lstFileDinhKem
                                }).ToList();

                            }
                            var listBangCapUngVien = db.HRM_UngVienBangCap.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                            if (listBangCapUngVien != null && listBangCapUngVien.Count > 0)
                            {
                                NhanVien.ListNhanVienBangCap = listBangCapUngVien.Select(a => new HRM_NhanVienBangCap
                                {
                                    NoiDaoTao = a.NoiDaoTao,
                                    ChuyenNganh = a.ChuyenNganh,
                                    IdTrinhDoDaoTao = a.IdTrinhDoDaoTao,
                                    NgayTotNghiep = new DateTime(a.NamTotNghiep ?? DateTime.Now.Year, 5, 1),
                                    lstFileDinhKem = a.lstFileDinhKem
                                }).ToList();

                            }
                            var listNgonNguUngVien = db.HRM_NgonNguUngVien.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                            if (listNgonNguUngVien != null && listNgonNguUngVien.Count > 0)
                            {
                                NhanVien.ListNhanVienNgonNgu = listNgonNguUngVien.Select(a => new HRM_NgonNguNhanVien
                                {
                                    IdNgonNgu = a.IdNgonNgu,
                                    IdXepLoaiNghe = a.IdXepLoaiNghe,
                                    IdXepLoaiNoi = a.IdXepLoaiNoi,
                                    IdXepLoaiViet = a.IdXepLoaiViet,
                                    IdXepLoaiDoc = a.IdXepLoaiDoc,
                                    lstFileDinhKem = a.lstFileDinhKem
                                }).ToList();

                            }

                            fcNhanVien.SetThongTinNhanVienTransaction(NhanVien, db);
                            HRM_HopDong HopDong = new HRM_HopDong();
                            HopDong.lstPhuCap = new List<HRM_HopDongPhuCap>();
                            if (ungVien.lisPhuCap != null && ungVien.lisPhuCap.Count > 0)
                            {
                                foreach (var Pc in ungVien.lisPhuCap)
                                {
                                    HRM_NhanVien_PhuCap phuCap = new HRM_NhanVien_PhuCap();
                                    phuCap.IddmPhuCap = Pc.IddmPhuCap;
                                    phuCap.SoTien = Pc.SoTien;
                                    NhanVien.listPhuCap.Add(phuCap);
                                    HRM_HopDongPhuCap PhuCapHopDong = new HRM_HopDongPhuCap();
                                    PhuCapHopDong.IddmLoaiPhuCap = Pc.IddmPhuCap;
                                    PhuCapHopDong.SoTien = Pc.SoTien;
                                    HopDong.lstPhuCap.Add(PhuCapHopDong);
                                }
                            }

                            HopDong.IdNhanVien = NhanVien.IdGuid;
                            HopDong.listIdCongTy = NhanVien.listIdCongTy;
                            HopDong.IdBoPhanEOS = NhanVien.IdBoPhanEOS;
                            HopDong.IddmChucDanh = NhanVien.IdChucDanh;
                            HopDong.IddmChucVu = NhanVien.IdChucVu;
                            HopDong.IdThoiGianLamViec = model.IdCaLamViec;
                            HopDong.IdLoaiHopDong = model.IdLoaiHopDong;
                            HopDong.LoaiLuong = model.HinhThucTraLuongDaoTao ?? 0;
                            HopDong.LuongHopDong = new HRM_Luong_HopDong();
                            HopDong.LuongHopDong.LuongBaoHiem = model.Luong;
                            HopDong.LuongHopDong.TyLeHuongLuong = model.PhanTramLuongThuViec;
                            if (Host != "ICT" || ungVien.isCongNhan == false)
                                fcHopDong.SetHopDongNoTransaction(HopDong, db);
                        }
                        db.Entry(ungVien).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công " + listUngVien.Count + " ứng viên");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
                }
            }
        }
        public dynamic ChuyenDuyetNhieuQuyTrinhDatLichPhongVanDat(ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    List<HRM_TuyenDung_UngVien> listUngVien = db.HRM_TuyenDung_UngVien.Where(x => x.TrangThai == 3 && x.isCongNhan == true && x.isDaPheDuyetLuong == false).ToList();
                    var listId = listUngVien.Select(a => a.IdGuid).Distinct().ToList();
                    List<HRM_TuyenDung_QuyTrinhPhongVan> listTuyenDung = db.HRM_TuyenDung_QuyTrinhPhongVan.Where(x => listId.Contains(x.IdUngVien)).ToList();
                    listTuyenDung = listTuyenDung.FindAll(x => x.IdBoPhan != null && x.IdBoPhan != 0);
                    var listIdLNV = db.HRM_dmLoaiNhanVien.Where(a => a.Ma == "TV." || a.Ma == "TVN").Select(a => a.IdGuid).Distinct().ToList();
                    foreach (var ungVien in listUngVien)
                    {
                        ungVien.TrangThai = 3;
                        var model = listTuyenDung.FirstOrDefault(a => a.IdUngVien == ungVien.IdGuid);
                        if (model != null)
                        {
                            model.TrangThai = 10;
                            db.Entry(model).State = EntityState.Modified;




                        }
                        else
                        {
                            model = new HRM_TuyenDung_QuyTrinhPhongVan();
                            model.TrangThai = 10;
                            model.IdGuid = Guid.NewGuid().ToString();
                            ungVien.IdLichPhongVan = model.IdGuid;
                            model.IdUngVien = ungVien.IdGuid;
                            model.MaUngVien = ungVien.MaUngVien;
                            model.HoTenUngVien = ungVien.HoTenDayDu;
                            model.DotTuyenDung = ungVien.DotTuyenDung;
                            if (!string.IsNullOrEmpty(ungVien.IdLoaiNhanVien))
                            {

                                model.IdLoaiNhanVien = ungVien.IdLoaiNhanVien;
                                if (listIdLNV.Contains(ungVien.IdLoaiNhanVien))
                                    model.LoaiCongNhan = "Thời vụ/Thời vụ ngày";
                                else

                                    model.LoaiCongNhan = "Bảo trì";
                            }
                            model.IdTongHopDeNghiTuyenDung = ungVien.IdQuyTrinhNhuCau;
                            model.NgayNhanViecDuKien = ungVien.NgayNhanViecDuKien;
                            model.isCongNhan = ungVien.isCongNhan;
                            model.IdCongTyEOS = ungVien.IdCongTyEOS ?? 0;
                            model.IdCoSoTrungTuyen = ungVien.IdCoSoTrungTuyen ?? 0;
                            model.IdBoPhan = ungVien.IdBoPhanEOS;
                            model.IdCaLamViec = ungVien.IdCaLamViec;
                            model.IdCapBacLuongDeXuat = ungVien.IdCapBacLuongDeXuat;
                            model.Luong = ungVien.Luong;
                            model.HinhThucTraLuongDaoTao = ungVien.HinhThucTraLuongDaoTao;
                            model.IdLoaiNhanVien = ungVien.IdLoaiNhanVien;
                            model.IdLoaiHopDong = ungVien.IdLoaiHopDong;
                            model.ThoiGianThuViec = ungVien.ThoiGianThuViec;
                            model.IdChucVu = ungVien.IdChucVu;
                            model.IdChucDanhUngTuyen = ungVien.IddmChucDanhUngTuyen;
                            model.PhanTramLuongThuViec = ungVien.PhanTramLuongThuViec;
                            model.ThuCoc = ungVien.ThuCoc;
                            model.IdNhanVienGioiThieu = ungVien.IdNhanVienGioiThieu;
                            db.Entry(model).State = EntityState.Added;

                        }
                        double Luong = model.Luong;
                        var LuongTuyenDung = db.HRM_DeNghiTuyenDung_CongNhan.FirstOrDefault(a => a.IdQuyTrinh == ungVien.IdQuyTrinhNhuCau && model.LoaiCongNhan == a.Loai);
                        if (LuongTuyenDung != null && LuongTuyenDung.TongLuong >= Luong)
                        {
                            var fcNhanVien = _FuncNhanVien;
                            var fcHopDong = _funcHopDong;
                            ungVien.isDaPheDuyetLuong = true;
                            HRM_NhanVien NhanVien = new HRM_NhanVien(ungVien);
                            try
                            {
                                NhanVien.MaNhanVien = fcNhanVien.GetMaNhanVien(db);
                            }
                            catch
                            {

                            }
                            if (ungVien.ThuCoc != null && ungVien.ThuCoc > 0)
                            {
                                NhanVien.ThuCoc = ungVien.ThuCoc;
                                NhanVien.IsDaThuCoc = false;
                            }
                            NhanVien.NgayNhanViecDuKien = model.NgayNhanViecDuKien;
                            NhanVien.NgayNhanViecDuKienUnix = model.NgayNhanViecDuKienUnix;
                            NhanVien.IdCaLamViec = model.IdCaLamViec;
                            NhanVien.IdCapBacLuongChapNhan = model.IdCapBacLuongDeXuat;
                            NhanVien.IdCapBacLuongDeXuat = model.IdCapBacLuongDeXuat;
                            NhanVien.IdBoPhanEOS = model.IdBoPhan ?? 0;
                            NhanVien.ThoiGianThuViec = model.ThoiGianThuViec;
                            NhanVien.IdLoaiNhanVien = model.IdLoaiNhanVien;
                            if (!string.IsNullOrEmpty(ungVien.MaNhanVien))
                                NhanVien.MaNhanVien = ungVien.MaNhanVien;
                            NhanVien.IdChucDanh = model.IddmTrungTuyen;
                            NhanVien.listIdCongTy = new List<long>();
                            NhanVien.listIdCongTy.Add(ungVien.IdCoSoTrungTuyen ?? 0);
                            NhanVien.IdChucVu = model.IdChucVu;
                            NhanVien.IdUngVien = ungVien.IdGuid;
                            NhanVien.IdLoaiNhanVien = model.IdLoaiNhanVien;
                            NhanVien.IddmTrinhDo = ungVien.IddmTrinhDoHocVan;
                            NhanVien.PhanTramLuongThuViec = model.PhanTramLuongThuViec;
                            NhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                            NhanVien.listPhuCap = new List<HRM_NhanVien_PhuCap>();
                            NhanVien.Luong = model.Luong;
                            NhanVien.listGiayToTuyThan = new List<HRM_NhanVien_GiayToTuyThan>();
                            NhanVien.listGiayToTuyThan.Add(new HRM_NhanVien_GiayToTuyThan()
                            {
                                Loai = 1,
                                So = ungVien.CCCD,
                                NoiCap = ungVien.NoiCap,
                                NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(ungVien.NgayCap),
                                isApDung = true
                            });
                            NhanVien.listSoDienThoai = new List<HRM_NhanVien_SoDienThoai>();
                            NhanVien.listSoDienThoai.Add(new HRM_NhanVien_SoDienThoai()
                            {
                                SoDienThoai = ungVien.DienThoaiDiDong,
                                isApDung = true
                            });
                            NhanVien.listEmail = new List<HRM_NhanVien_Email>();
                            NhanVien.listEmail.Add(new HRM_NhanVien_Email()
                            {
                                Email = ungVien.Email,
                                isApDung = true
                            });
                            var listKinhNghiemUngVien = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(a => a.IdTuyenDungUngVien == ungVien.IdGuid).ToList();
                            if (listKinhNghiemUngVien != null && listKinhNghiemUngVien.Count > 0)
                            {
                                NhanVien.ListQuaTrinhCongTacNgoaiCongTy = listKinhNghiemUngVien.Select(a => new HRM_QuaTrinhCongTacNgoaiCongTy
                                {
                                    ThoiGian = a.ThoiGianLamViec,
                                    DonViCongTac = a.DonViCongTac,
                                    ViTriCongViec = a.ViTri
                                }).ToList();

                            }
                            var listChungChiUngVien = db.HRM_ChungChiUngVien.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                            if (listChungChiUngVien != null && listChungChiUngVien.Count > 0)
                            {
                                NhanVien.ListNhanVienChungChi = listChungChiUngVien.Select(a => new HRM_ChungChiNhanVien
                                {
                                    TenChungChi = a.TenChungChi,
                                    IdNhomChungChi = a.IdNhomChungChi,
                                    TrinhDo = a.TrinhDo,
                                    NgayCap = a.NgayCap,
                                    NgayHetHan = a.NgayHetHan,
                                    NgayCapUnix = DateTimeToUnixTimeStamp(a.NgayCap),
                                    NgayHetHanUnix = DateTimeToUnixTimeStamp(a.NgayHetHan),
                                    NoiCap = a.NoiCap,
                                    lstFileDinhKem = a.lstFileDinhKem
                                }).ToList();

                            }
                            var listBangCapUngVien = db.HRM_UngVienBangCap.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                            if (listBangCapUngVien != null && listBangCapUngVien.Count > 0)
                            {
                                NhanVien.ListNhanVienBangCap = listBangCapUngVien.Select(a => new HRM_NhanVienBangCap
                                {
                                    NoiDaoTao = a.NoiDaoTao,
                                    ChuyenNganh = a.ChuyenNganh,
                                    IdTrinhDoDaoTao = a.IdTrinhDoDaoTao,
                                    NgayTotNghiep = new DateTime(a.NamTotNghiep ?? DateTime.Now.Year, 5, 1),
                                    lstFileDinhKem = a.lstFileDinhKem
                                }).ToList();

                            }
                            var listNgonNguUngVien = db.HRM_NgonNguUngVien.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                            if (listNgonNguUngVien != null && listNgonNguUngVien.Count > 0)
                            {
                                NhanVien.ListNhanVienNgonNgu = listNgonNguUngVien.Select(a => new HRM_NgonNguNhanVien
                                {
                                    IdNgonNgu = a.IdNgonNgu,
                                    IdXepLoaiNghe = a.IdXepLoaiNghe,
                                    IdXepLoaiNoi = a.IdXepLoaiNoi,
                                    IdXepLoaiViet = a.IdXepLoaiViet,
                                    IdXepLoaiDoc = a.IdXepLoaiDoc,
                                    lstFileDinhKem = a.lstFileDinhKem
                                }).ToList();

                            }

                            fcNhanVien.SetThongTinNhanVienTransaction(NhanVien, db);
                            HRM_HopDong HopDong = new HRM_HopDong();
                            HopDong.lstPhuCap = new List<HRM_HopDongPhuCap>();
                            if (ungVien.lisPhuCap != null && ungVien.lisPhuCap.Count > 0)
                            {
                                foreach (var Pc in ungVien.lisPhuCap)
                                {
                                    HRM_NhanVien_PhuCap phuCap = new HRM_NhanVien_PhuCap();
                                    phuCap.IddmPhuCap = Pc.IddmPhuCap;
                                    phuCap.SoTien = Pc.SoTien;
                                    NhanVien.listPhuCap.Add(phuCap);
                                    HRM_HopDongPhuCap PhuCapHopDong = new HRM_HopDongPhuCap();
                                    PhuCapHopDong.IddmLoaiPhuCap = Pc.IddmPhuCap;
                                    PhuCapHopDong.SoTien = Pc.SoTien;
                                    HopDong.lstPhuCap.Add(PhuCapHopDong);
                                }
                            }

                            HopDong.IdNhanVien = NhanVien.IdGuid;
                            HopDong.IdCongTyEOS = NhanVien.IdCongTyEOS;
                            HopDong.listIdCongTy = NhanVien.listIdCongTy;
                            HopDong.IdBoPhanEOS = NhanVien.IdBoPhanEOS;
                            HopDong.IddmChucDanh = NhanVien.IdChucDanh;
                            HopDong.IddmChucVu = NhanVien.IdChucVu;
                            HopDong.IdThoiGianLamViec = model.IdCaLamViec;
                            HopDong.IdLoaiHopDong = model.IdLoaiHopDong;
                            HopDong.LoaiLuong = model.HinhThucTraLuongDaoTao ?? 0;
                            HopDong.LuongHopDong = new HRM_Luong_HopDong();
                            HopDong.LuongHopDong.LuongBaoHiem = model.Luong;
                            HopDong.LuongHopDong.TyLeHuongLuong = model.PhanTramLuongThuViec;
                            fcHopDong.SetHopDongNoTransaction(HopDong, db);
                        }
                        db.Entry(ungVien).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
                }
            }
        }
        public void addCongTyNhanVien(ApplicationDbContext db)
        {
            DateTime time = new DateTime(2025, 10, 12);
            time = time.Date;
            DateTime timeEnd = time.AddDays(1);
            var listNhanVien = db.HRM_NhanVien.Where(a => a.Created >= time && a.Created < timeEnd && !string.IsNullOrEmpty(a.IdUngVien)).ToList();
            var listIdUngVien = listNhanVien.Select(a => a.IdUngVien).Distinct().ToList();
            var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => listIdUngVien.Contains(a.IdGuid)).ToList();
            foreach (var aModel in listNhanVien)
            {
                var UngVien = listUngVien.FirstOrDefault(a => a.IdGuid == aModel.IdUngVien);
                aModel.listIdCongTy = new List<long>();
                aModel.listIdCongTy.Add(UngVien.IdCoSoTrungTuyen ?? 0);
                aModel.UserName = aModel.MaNhanVien;
                clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(db, aModel);
                aModel.IdUser = _clsNhanVien.UserPkid;
                aModel.IdNhanVienEOS = _clsNhanVien.Id;


                db.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                bp.IdGuid = Guid.NewGuid().ToString();
                bp.IdQuyTrinh = aModel.IdGuid;
                bp.IdBoPhan = UngVien.IdCoSoTrungTuyen ?? 0;
                bp.TableName = nameof(HRM_NhanVien);
                db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
            }
            db.SaveChanges();
        }
        public dynamic SetTrangThaiVongPhongVan(HRM_TuyenDung_VongPhongVan model, ApplicationDbContext db)
        {
            try
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
            }
        }

        public string FuncUpdateDatLichPhongVan(HRM_TuyenDung_QuyTrinhPhongVan model, ApplicationDbContext db)
        {
            try
            {
                if (model.TrangThai == 1)   //Đạt
                    model.TrangThai = 10;
                if (model.TrangThai == 2)   //Không đạt
                    model.TrangThai = 15;
                if (model.TrangThai == 0)
                {
                    HRM_TuyenDung_QuyTrinhPhongVan modelDB = db.HRM_TuyenDung_QuyTrinhPhongVan.AsNoTracking().Where(x => x.IdGuid == model.IdGuid).FirstOrDefault();
                    if (modelDB != null)
                        model.TrangThai = modelDB.TrangThai;
                }
                HRM_TuyenDung_UngVien ungVien = db.HRM_TuyenDung_UngVien.Find(model.IdUngVien);
                if (ungVien != null)
                {
                    model.IdCongTyEOS = ungVien.IdCongTyEOS ?? 0;
                    model.DotTuyenDung = ungVien.DotTuyenDung;
                    if (string.IsNullOrEmpty(model.IdTongHopDeNghiTuyenDung))
                        model.IdTongHopDeNghiTuyenDung = ungVien.IdQuyTrinhNhuCau;
                    //model.IdChucVu = ungVien.IdChucVu;
                }
                if (!string.IsNullOrEmpty(model.IdTongHopDeNghiTuyenDung))
                {
                    var NhuCauTuyenDung = db.HRM_DeNghiTuyenDung.FirstOrDefault(a => a.IdGuid == model.IdTongHopDeNghiTuyenDung);
                    if (NhuCauTuyenDung != null)
                    {

                        model.DotTuyenDung = NhuCauTuyenDung.LyDo;
                    }
                }
                model.NgayTao = dungchung.UnixTimeStampToDateTime(model.NgayTaoUnix);
                if (model.NgayNhanViecDuKienUnix > 0)
                    model.NgayNhanViecDuKien = dungchung.UnixTimeStampToDateTime(model.NgayNhanViecDuKienUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_TuyenDung_QuyTrinhPhongVan.Add(model);
                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                }
                var listVongPhongVanOld = db.HRM_TuyenDung_VongPhongVan.Where(a => a.IdQuyTrinhPhongVan == model.IdGuid).ToList();
                if (listVongPhongVanOld != null && listVongPhongVanOld.Count > 0)
                    db.HRM_TuyenDung_VongPhongVan.RemoveRange(listVongPhongVanOld);
                var lstNguoiPhongVanOld = db.HRM_TuyenDung_NguoiPhongVan.Where(a => a.IdUngVien == model.IdUngVien).ToList();
                if (lstNguoiPhongVanOld != null && lstNguoiPhongVanOld.Count > 0)
                    db.HRM_TuyenDung_NguoiPhongVan.RemoveRange(lstNguoiPhongVanOld);
                var lstPhuCapOld = db.HRM_TuyenDung_PhuCap.Where(a => a.IdUngVien == model.IdUngVien).ToList();
                if (lstPhuCapOld != null && lstPhuCapOld.Count > 0)
                    db.HRM_TuyenDung_PhuCap.RemoveRange(lstPhuCapOld);

                if (model.listVongPhongVan != null)
                {
                    List<HRM_TuyenDung_VongPhongVan> listVongPhongVan = model.listVongPhongVan;
                    foreach (var item in listVongPhongVan)
                    {
                        item.NgayPhongVan = dungchung.UnixTimeStampToDateTime(item.NgayPhongVanUnix);

                        item.NgayPhongVan = dungchung.UnixTimeStampToDateTime(item.NgayPhongVanUnix);
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinhPhongVan = model.IdGuid;
                        if (item.lstNguoiPhongVan != null)
                        {
                            foreach (var itemNTG in item.lstNguoiPhongVan)
                            {
                                itemNTG.IdGuid = Guid.NewGuid().ToString();
                                itemNTG.IdVongPhongVan = item.IdGuid;
                                itemNTG.IdUngVien = model.IdUngVien;
                                db.HRM_TuyenDung_NguoiPhongVan.Add(itemNTG);
                            }
                        }

                        db.HRM_TuyenDung_VongPhongVan.Add(item);
                    }
                }
                if (model.listPhuCap != null)
                {
                    List<HRM_TuyenDung_PhuCap> listPhuCap = model.listPhuCap;
                    foreach (var item in listPhuCap)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinhPhongVan = model.IdGuid;
                        item.IdUngVien = model.IdUngVien;
                        db.HRM_TuyenDung_PhuCap.Add(item);
                    }
                }

                if (model.lstFileDinhKem != null)
                {
                    foreach (var item in model.lstFileDinhKem)
                    {
                        SetFileDinhKem(db, item, DefineData.eTable.HRM_TuyenDung_QuyTrinhPhongVan, model.IdGuid);
                    }
                }
                ungVien.IdNhanVienGioiThieu = model.IdNhanVienGioiThieu;
                ungVien.IdLichPhongVan = model.IdGuid;
                if (model.TrangThai != 0)
                    ungVien.TrangThai = model.TrangThai;
                if (model.TrangThai == 10)
                    ungVien.TrangThai = 3;
                else if (model.TrangThai == 15)
                {
                    ungVien.TrangThai = 4;
                    ungVien.TenTrangThai = "Ứng viên không qua vòng phỏng vấn";
                }
                ungVien.NgayNhanViecDuKien = model.NgayNhanViecDuKien;
                ungVien.IdCaLamViec = model.IdCaLamViec;
                ungVien.IdCapBacLuongDeXuat = model.IdCapBacLuongDeXuat;
                ungVien.IdCoSoTrungTuyen = model.IdCoSoTrungTuyen;
                ungVien.ThoiGianThuViec = model.ThoiGianThuViec;
                ungVien.PhanTramLuongThuViec = model.PhanTramLuongThuViec;
                ungVien.IddmTrungTuyen = model.IddmTrungTuyen;
                ungVien.Luong = model.Luong;
                ungVien.Loai = model.LoaiCongNhan;
                ungVien.IdBoPhanEOS = model.IdBoPhan;
                ungVien.IdLoaiHopDong = model.IdLoaiHopDong;
                ungVien.IdChucVu = model.IdChucVu;
                ungVien.IddmChucDanhUngTuyen = model.IdChucDanhUngTuyen;
                ungVien.IdLoaiNhanVien = model.IdLoaiNhanVien;
                ungVien.IdQuyTrinhNhuCau = model.IdTongHopDeNghiTuyenDung;

                if (ungVien.TrangThai == 3)
                {
                    List<HRM_UngVien_PhuCap> listDelete = db.HRM_UngVien_PhuCap.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                    if (listDelete != null && listDelete.Count > 0)
                    {
                        db.HRM_UngVien_PhuCap.RemoveRange(listDelete);
                    }
                    double Luong = model.Luong;
                    if (model.listPhuCap != null && model.listPhuCap.Count > 0)
                    {
                        Luong += model.listPhuCap.Sum(a => a.SoTien);
                        foreach (var item in model.listPhuCap.Where(a => a.isChon == true).ToList())
                        {
                            HRM_UngVien_PhuCap itemAdd = new HRM_UngVien_PhuCap();
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            itemAdd.IdUngVien = ungVien.IdGuid;
                            itemAdd.IddmPhuCap = item.IddmPhuCap;
                            itemAdd.SoTien = item.SoTien;
                            db.HRM_UngVien_PhuCap.Add(itemAdd);
                        }
                    }
                    ungVien.ThuCoc = model.ThuCoc;
                    var LuongTuyenDung = db.HRM_DeNghiTuyenDung_CongNhan.FirstOrDefault(a => a.IdQuyTrinh == ungVien.IdQuyTrinhNhuCau && model.LoaiCongNhan == a.Loai);
                    if (LuongTuyenDung != null && LuongTuyenDung.TongLuong >= Luong)
                    {
                        var fcNhanVien = _FuncNhanVien;
                        var fcHopDong = _funcHopDong;
                        ungVien.isDaPheDuyetLuong = true;
                        HRM_NhanVien NhanVien = new HRM_NhanVien(ungVien);
                        try
                        {
                            NhanVien.MaNhanVien = fcNhanVien.GetMaNhanVien(db);
                        }
                        catch
                        {

                        }
                        if (ungVien.ThuCoc != null && ungVien.ThuCoc > 0)
                        {
                            NhanVien.ThuCoc = ungVien.ThuCoc;
                            NhanVien.IsDaThuCoc = false;
                        }
                        var CongTy = _CallWebService.GetCongTyKyHopDong(db).FirstOrDefault();
                        NhanVien.NgayNhanViecDuKien = model.NgayNhanViecDuKien;
                        NhanVien.NgayNhanViecDuKienUnix = model.NgayNhanViecDuKienUnix;
                        NhanVien.NgayBatDauLamViec = model.NgayNhanViecDuKien;
                        NhanVien.NgayBatDauLamViecUnix = model.NgayNhanViecDuKienUnix;
                        NhanVien.IdCaLamViec = model.IdCaLamViec;
                        NhanVien.IdCapBacLuongChapNhan = model.IdCapBacLuongDeXuat;
                        NhanVien.IdCapBacLuongDeXuat = model.IdCapBacLuongDeXuat;
                        NhanVien.IdBoPhanEOS = model.IdBoPhan ?? 0;
                        NhanVien.ThoiGianThuViec = model.ThoiGianThuViec;
                        if (!string.IsNullOrEmpty(ungVien.MaNhanVien))
                            NhanVien.MaNhanVien = ungVien.MaNhanVien;
                        NhanVien.IdChucDanh = model.IddmTrungTuyen;
                        NhanVien.IdChucVu = model.IdChucVu;
                        NhanVien.IdLoaiNhanVien = model.IdLoaiNhanVien;
                        NhanVien.IddmTrinhDo = ungVien.IddmTrinhDoHocVan;
                        NhanVien.PhanTramLuongThuViec = model.PhanTramLuongThuViec;
                        NhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                        NhanVien.listPhuCap = new List<HRM_NhanVien_PhuCap>();
                        NhanVien.Luong = model.Luong;
                        NhanVien.MaSoThueTNCN = ungVien.MaSoThueCCCD;
                        NhanVien.MaSoThueCMT = ungVien.MaSoThueCMT;
                        NhanVien.listIdCongTy = new List<long>();
                        NhanVien.listIdCongTy.Add(ungVien.IdCoSoTrungTuyen ?? 0);
                        NhanVien.listGiayToTuyThan = new List<HRM_NhanVien_GiayToTuyThan>();
                        NhanVien.listGiayToTuyThan.Add(new HRM_NhanVien_GiayToTuyThan()
                        {
                            Loai = 1,
                            So = ungVien.CCCD,
                            NoiCap = ungVien.NoiCap,
                            NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(ungVien.NgayCap),
                            isApDung = true
                        });
                        if (!string.IsNullOrEmpty(ungVien.MaSoThueCMT))
                            NhanVien.listGiayToTuyThan.Add(new HRM_NhanVien_GiayToTuyThan()
                            {
                                Loai = 2,
                                So = ungVien.MaSoThueCMT,
                                //NoiCap = ungVien.NoiCap,
                                //NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(ungVien.NgayCap),
                                isApDung = false
                            });

                        NhanVien.listSoDienThoai = new List<HRM_NhanVien_SoDienThoai>();
                        NhanVien.listSoDienThoai.Add(new HRM_NhanVien_SoDienThoai()
                        {
                            SoDienThoai = ungVien.DienThoaiDiDong,
                            isApDung = true
                        });
                        NhanVien.listEmail = new List<HRM_NhanVien_Email>();
                        NhanVien.listEmail.Add(new HRM_NhanVien_Email()
                        {
                            Email = ungVien.Email,
                            isApDung = true
                        });
                        var listKinhNghiemUngVien = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(a => a.IdTuyenDungUngVien == ungVien.IdGuid).ToList();
                        if (listKinhNghiemUngVien != null && listKinhNghiemUngVien.Count > 0)
                        {
                            NhanVien.ListQuaTrinhCongTacNgoaiCongTy = listKinhNghiemUngVien.Select(a => new HRM_QuaTrinhCongTacNgoaiCongTy
                            {
                                ThoiGian = a.ThoiGianLamViec,
                                DonViCongTac = a.DonViCongTac,
                                ViTriCongViec = a.ViTri
                            }).ToList();

                        }
                        var listChungChiUngVien = db.HRM_ChungChiUngVien.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                        if (listChungChiUngVien != null && listChungChiUngVien.Count > 0)
                        {
                            NhanVien.ListNhanVienChungChi = listChungChiUngVien.Select(a => new HRM_ChungChiNhanVien
                            {
                                TenChungChi = a.TenChungChi,
                                IdNhomChungChi = a.IdNhomChungChi,
                                TrinhDo = a.TrinhDo,
                                NgayCap = a.NgayCap,
                                NgayHetHan = a.NgayHetHan,
                                NgayCapUnix = DateTimeToUnixTimeStamp(a.NgayCap),
                                NgayHetHanUnix = DateTimeToUnixTimeStamp(a.NgayHetHan),
                                NoiCap = a.NoiCap,
                                lstFileDinhKem = a.lstFileDinhKem
                            }).ToList();

                        }
                        var listBangCapUngVien = db.HRM_UngVienBangCap.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                        if (listBangCapUngVien != null && listBangCapUngVien.Count > 0)
                        {
                            NhanVien.ListNhanVienBangCap = listBangCapUngVien.Select(a => new HRM_NhanVienBangCap
                            {
                                NoiDaoTao = a.NoiDaoTao,
                                ChuyenNganh = a.ChuyenNganh,
                                IdTrinhDoDaoTao = a.IdTrinhDoDaoTao,
                                NgayTotNghiep = new DateTime(a.NamTotNghiep ?? DateTime.Now.Year, 5, 1),
                                lstFileDinhKem = a.lstFileDinhKem
                            }).ToList();

                        }
                        var listNgonNguUngVien = db.HRM_NgonNguUngVien.Where(a => a.IdUngVien == ungVien.IdGuid).ToList();
                        if (listNgonNguUngVien != null && listNgonNguUngVien.Count > 0)
                        {
                            NhanVien.ListNhanVienNgonNgu = listNgonNguUngVien.Select(a => new HRM_NgonNguNhanVien
                            {
                                IdNgonNgu = a.IdNgonNgu,
                                IdXepLoaiNghe = a.IdXepLoaiNghe,
                                IdXepLoaiNoi = a.IdXepLoaiNoi,
                                IdXepLoaiViet = a.IdXepLoaiViet,
                                IdXepLoaiDoc = a.IdXepLoaiDoc,
                                lstFileDinhKem = a.lstFileDinhKem
                            }).ToList();

                        }

                        fcNhanVien.SetThongTinNhanVienTransaction(NhanVien, db);
                        HRM_HopDong HopDong = new HRM_HopDong();
                        HopDong.lstPhuCap = new List<HRM_HopDongPhuCap>();
                        if (ungVien.lisPhuCap != null && ungVien.lisPhuCap.Count > 0)
                        {
                            foreach (var Pc in ungVien.lisPhuCap)
                            {
                                HRM_NhanVien_PhuCap phuCap = new HRM_NhanVien_PhuCap();
                                phuCap.IddmPhuCap = Pc.IddmPhuCap;
                                phuCap.SoTien = Pc.SoTien;
                                NhanVien.listPhuCap.Add(phuCap);
                                HRM_HopDongPhuCap PhuCapHopDong = new HRM_HopDongPhuCap();
                                PhuCapHopDong.IddmLoaiPhuCap = Pc.IddmPhuCap;
                                PhuCapHopDong.SoTien = Pc.SoTien;
                                HopDong.lstPhuCap.Add(PhuCapHopDong);
                            }
                        }
                        if (Host == "PHT")
                            HopDong.IdCongTyKyHopDong = CongTy.Id;
                        HopDong.IdNhanVien = NhanVien.IdGuid;
                        HopDong.IdCongTyEOS = NhanVien.IdCongTyEOS;
                        HopDong.IdBoPhanEOS = NhanVien.IdBoPhanEOS;
                        HopDong.IddmChucDanh = NhanVien.IdChucDanh;
                        HopDong.IddmChucVu = NhanVien.IdChucVu;
                        HopDong.IdThoiGianLamViec = model.IdCaLamViec;
                        HopDong.NgayHieuLuc = model.NgayNhanViecDuKien ?? DateTime.Now;
                        HopDong.NgayKy = model.NgayNhanViecDuKien ?? DateTime.Now;
                        HopDong.IdThoiGianLamViec = model.IdCaLamViec;
                        HopDong.IdLoaiHopDong = model.IdLoaiHopDong;
                        if (model.ThoiGianThuViec > 0)
                            HopDong.IdThoiHanHopDong = db.HRM_dmThoiHanHopDong.Where(a => a.SoThang == model.ThoiGianThuViec).Select(a => a.IdGuid).FirstOrDefault();
                        string IdHInhThucTraLuong = db.HRM_dmHinhThucTraLuong.Where(a => a.Ma == "TM").Select(a => a.IdGuid).FirstOrDefault();
                        HopDong.LoaiLuong = model.HinhThucTraLuongDaoTao ?? 0;
                        HopDong.LuongHopDong = new HRM_Luong_HopDong();
                        HopDong.LuongHopDong.LuongBaoHiem = model.Luong;
                        HopDong.LuongHopDong.TyLeHuongLuong = model.PhanTramLuongThuViec;
                        HopDong.LuongHopDong.IddmHinhThucTraLuong = IdHInhThucTraLuong;
                        HopDong.LuongBaoHiem = new HRM_LuongBaoHiem_HopDong();
                        HopDong.LuongBaoHiem.LuongBaoHiem = model.Luong;
                        HopDong.LuongBaoHiem.TyLeHuongLuong = model.PhanTramLuongThuViec;
                        HopDong.LuongBaoHiem.IddmHinhThucTraLuong = IdHInhThucTraLuong;

                        if (ungVien.isCongNhan == false)
                            fcHopDong.SetHopDongNoTransaction(HopDong, db);
                    }
                }
                db.Entry(ungVien).State = EntityState.Modified;
                db.SaveChanges();
                return "0";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        public dynamic GetDatLichPhongVanById(string Id, ApplicationDbContext db)
        {
            HRM_TuyenDung_QuyTrinhPhongVan model = db.HRM_TuyenDung_QuyTrinhPhongVan.Find(Id);
            var listVongPhongVan = db.HRM_TuyenDung_VongPhongVan.Where(r => r.IdQuyTrinhPhongVan == Id).ToList();
            if (model.NgayTao != null)
                model.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTao);
            if (model.NgayNhanViecDuKien != null)
                model.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayNhanViecDuKien);
            model.listPhuCap = new List<HRM_TuyenDung_PhuCap>();
            var UngVien = db.HRM_TuyenDung_UngVien.Find(model.IdUngVien);
            if (UngVien != null)
                model.IdCongTyEOS = UngVien.IdCongTyEOS ?? 0;
            if (UngVien.IdChucVu > 0)
                model.IdChucVu = UngVien.IdChucVu;
            if (UngVien.IddmChucDanhUngTuyen > 0)
                model.IdChucDanhUngTuyen = UngVien.IddmChucDanhUngTuyen;
            if (string.IsNullOrEmpty(UngVien.IdLoaiNhanVien))
                model.IdLoaiNhanVien = UngVien.IdLoaiNhanVien;
            model.HoTenUngVien = UngVien.HoTenDayDu;
            model.IdCoSoTrungTuyen = UngVien.IdCoSoTrungTuyen ?? 0;
            model.IdNhanVienGioiThieu = UngVien.IdNhanVienGioiThieu;
            model.IdTongHopDeNghiTuyenDung = UngVien.IdQuyTrinhNhuCau;
            var lstNguoiPhongVans = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdUngVien == model.IdUngVien).ToList();
            model.listPhuCap = db.HRM_TuyenDung_PhuCap.Where(r => r.IdUngVien == model.IdUngVien).OrderBy(a => a.Created).ToList();
            if (model.listPhuCap != null & model.listPhuCap.Count > 0)
            {
                var listdmPhuCap = db.HRM_dmPhuCap.ToList();
                foreach (var pc in model.listPhuCap)
                {
                    var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                    if (Phucap != null)
                        pc.TenPhuCap = Phucap.Ten;
                }
            }
            List<string> listId = lstNguoiPhongVans.Select(a => a.IdNguoiThamGia).Distinct().ToList();
            listId.AddRange(listVongPhongVan.Select(a => a.IdNguoiChuTri).Distinct().ToList());
            listId.AddRange(listVongPhongVan.Select(a => a.IdNguoiPhongVanNgoai).Distinct().ToList());

            var listTenNguoiThamGias = db.HRM_NhanVien.Where(r => listId.Contains(r.IdGuid)).ToList();
            foreach (var item in listVongPhongVan)
            {
                item.NgayPhongVanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayPhongVan);
                var lstNguoiPhongVan = lstNguoiPhongVans.Where(r => r.IdVongPhongVan == item.IdGuid).ToList();
                var chuTri = listTenNguoiThamGias.FirstOrDefault(a => a.IdGuid == item.IdNguoiChuTri);
                if (chuTri != null)
                    item.Ten = chuTri.TenDayDu;
                var NguoiPVNgoai = listTenNguoiThamGias.FirstOrDefault(a => a.IdGuid == item.IdNguoiPhongVanNgoai);
                if (NguoiPVNgoai != null)
                    item.TenNguoiPhongVanNgoai = NguoiPVNgoai.TenDayDu;

                foreach (var itemNPV in lstNguoiPhongVan)
                {
                    if (itemNPV.isThamGia == 0)
                        itemNPV.TrangThaiThamGia = "Chưa xác nhận tham gia";
                    if (itemNPV.isThamGia == 1)
                        itemNPV.TrangThaiThamGia = "Tham gia phỏng vấn";
                    if (itemNPV.isThamGia == 2)
                        itemNPV.TrangThaiThamGia = "Từ chối gia phỏng vấn";
                    if (itemNPV.isThamGia == 3)
                        itemNPV.TrangThaiThamGia = "Ứng viên phỏng vấn đạt";
                    if (itemNPV.isThamGia == 4)
                        itemNPV.TrangThaiThamGia = "Ứng viên phỏng vấn không đạt";
                    var NV = listTenNguoiThamGias.FirstOrDefault(a => a.IdGuid == itemNPV.IdNguoiThamGia);
                    if (NV != null)
                        itemNPV.TenNguoiThamGia = NV.TenDayDu;
                }
                item.lstNguoiPhongVan = lstNguoiPhongVan;
            }
            model.listVongPhongVan = listVongPhongVan;
            model.lstFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_TuyenDung_QuyTrinhPhongVan.ToString(), Id);
            return model;
        }
        public dynamic GetDatLichPhongVanByIdUngVien(string Id, ApplicationDbContext db)
        {
            HRM_TuyenDung_QuyTrinhPhongVan model = db.HRM_TuyenDung_QuyTrinhPhongVan.FirstOrDefault(a => a.IdUngVien == Id);
            var listVongPhongVan = db.HRM_TuyenDung_VongPhongVan.Where(r => r.IdQuyTrinhPhongVan == Id).ToList();
            if (model.NgayTao != null)
                model.NgayTaoUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayTao);
            if (model.NgayNhanViecDuKien != null)
                model.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayNhanViecDuKien);
            model.listPhuCap = new List<HRM_TuyenDung_PhuCap>();
            var UngVien = db.HRM_TuyenDung_UngVien.Find(model.IdUngVien);
            if (UngVien != null)
                model.IdCongTyEOS = UngVien.IdCongTyEOS ?? 0;
            if (UngVien.IdChucVu > 0)
                model.IdChucVu = UngVien.IdChucVu;
            if (UngVien.IddmChucDanhUngTuyen > 0)
                model.IdChucDanhUngTuyen = UngVien.IddmChucDanhUngTuyen;
            if (string.IsNullOrEmpty(UngVien.IdLoaiNhanVien))
                model.IdLoaiNhanVien = UngVien.IdLoaiNhanVien;
            model.HoTenUngVien = UngVien.HoTenDayDu;
            model.IdNhanVienGioiThieu = UngVien.IdNhanVienGioiThieu;
            model.IdTongHopDeNghiTuyenDung = UngVien.IdQuyTrinhNhuCau;
            var lstNguoiPhongVans = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdUngVien == model.IdUngVien).ToList();
            model.listPhuCap = db.HRM_TuyenDung_PhuCap.Where(r => r.IdUngVien == model.IdUngVien).OrderBy(a => a.Created).ToList();
            if (model.listPhuCap != null & model.listPhuCap.Count > 0)
            {
                var listdmPhuCap = db.HRM_dmPhuCap.ToList();
                foreach (var pc in model.listPhuCap)
                {
                    var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                    if (Phucap != null)
                        pc.TenPhuCap = Phucap.Ten;
                }
            }
            List<string> listId = lstNguoiPhongVans.Select(a => a.IdNguoiThamGia).Distinct().ToList();
            listId.AddRange(listVongPhongVan.Select(a => a.IdNguoiChuTri).Distinct().ToList());
            listId.AddRange(listVongPhongVan.Select(a => a.IdNguoiPhongVanNgoai).Distinct().ToList());

            var listTenNguoiThamGias = db.HRM_NhanVien.Where(r => listId.Contains(r.IdGuid)).ToList();
            foreach (var item in listVongPhongVan)
            {
                item.NgayPhongVanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayPhongVan);
                var lstNguoiPhongVan = lstNguoiPhongVans.Where(r => r.IdVongPhongVan == item.IdGuid).ToList();
                var chuTri = listTenNguoiThamGias.FirstOrDefault(a => a.IdGuid == item.IdNguoiChuTri);
                if (chuTri != null)
                    item.Ten = chuTri.TenDayDu;
                var NguoiPVNgoai = listTenNguoiThamGias.FirstOrDefault(a => a.IdGuid == item.IdNguoiPhongVanNgoai);
                if (NguoiPVNgoai != null)
                    item.TenNguoiPhongVanNgoai = NguoiPVNgoai.TenDayDu;

                foreach (var itemNPV in lstNguoiPhongVan)
                {
                    if (itemNPV.isThamGia == 0)
                        itemNPV.TrangThaiThamGia = "Chưa xác nhận tham gia";
                    if (itemNPV.isThamGia == 1)
                        itemNPV.TrangThaiThamGia = "Tham gia phỏng vấn";
                    if (itemNPV.isThamGia == 2)
                        itemNPV.TrangThaiThamGia = "Từ chối gia phỏng vấn";
                    if (itemNPV.isThamGia == 3)
                        itemNPV.TrangThaiThamGia = "Ứng viên phỏng vấn đạt";
                    if (itemNPV.isThamGia == 4)
                        itemNPV.TrangThaiThamGia = "Ứng viên phỏng vấn không đạt";
                    var NV = listTenNguoiThamGias.FirstOrDefault(a => a.IdGuid == itemNPV.IdNguoiThamGia);
                    if (NV != null)
                        itemNPV.TenNguoiThamGia = NV.TenDayDu;
                }
                item.lstNguoiPhongVan = lstNguoiPhongVan;
            }
            model.listVongPhongVan = listVongPhongVan;
            model.lstFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_TuyenDung_QuyTrinhPhongVan.ToString(), Id);
            return model;
        }

        public dynamic DeleteDatLichPhongVan(string Id, ApplicationDbContext db)
        {
            try
            {
                HRM_TuyenDung_QuyTrinhPhongVan model = db.HRM_TuyenDung_QuyTrinhPhongVan.Find(Id);
                db.HRM_TuyenDung_QuyTrinhPhongVan.Remove(model);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
            }
        }

        public string GetNextMaQuyTrinhPhongVan(ApplicationDbContext db)
            => GenerateNextCode(db.HRM_TuyenDung_QuyTrinhPhongVan.Select(x => x.SoPhieu), "PVQT");


        public dynamic GetDanhSachLichPhongVanChuTri(dynamic dyn, ApplicationDbContext db)
        {
            string KeyWord = dyn.KeyWord;
            int nCurrentPage = dyn.CurrentPage ?? 0;
            int PageSize = dyn.PageSize ?? 25;
            int TrangThai = dyn.TrangThai ?? 0;
            double TuNgayUnix = dyn.TuNgayUnix ?? 0;
            double DenNgayUnix = dyn.DenNgayUnix ?? 0;
            DateTime TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
            DateTime DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date;
            long IdEOSDangNhap = _CallWebService.GetNhanSuFromUserPkid(db).Id;
            string IdNhanVien = db.HRM_NhanVien.Where(r => r.IdNhanVienEOS == IdEOSDangNhap).Select(r => r.IdGuid).FirstOrDefault();

            var query = from a in db.HRM_TuyenDung_VongPhongVan
                        join b in db.HRM_TuyenDung_QuyTrinhPhongVan on a.IdQuyTrinhPhongVan equals b.IdGuid
                        join c in db.HRM_TuyenDung_NguoiPhongVan on a.IdGuid equals c.IdVongPhongVan
                        where a.TrangThai == TrangThai && b.TrangThai != 0
                        select new { a, b, c };

            if (!string.IsNullOrEmpty(IdNhanVien))
                query = query.Where(x => x.c.IdNguoiThamGia == IdNhanVien);
            if (!string.IsNullOrEmpty(KeyWord))
                query = query.Where(x => x.b.HoTenUngVien.Contains(KeyWord) || x.b.MaUngVien.Contains(KeyWord) || x.a.DiaDiem.Contains(KeyWord));
            if (TuNgayUnix > 0)
                query = query.Where(x => x.a.NgayPhongVan.Date >= TuNgay);
            if (DenNgayUnix > 0)
                query = query.Where(x => x.a.NgayPhongVan.Date <= DenNgay);

            var projected = query
                .OrderByDescending(x => x.a.NgayPhongVan)
                .Select(x => new LichPhongVanChuTriViewModel
                {
                    Id = x.b.IdGuid,
                    IdTuyenDungPhongVan = x.c.IdGuid,
                    DiaDiem = x.a.DiaDiem,
                    NgayPhongVan = x.a.NgayPhongVan,
                    HoTenUngVien = x.b.HoTenUngVien,
                    MaUngVien = x.b.MaUngVien,
                    VongPhongVan = x.a.VongPhongVan,
                    IdChucDanhUngTuyen = x.b.IdChucDanhUngTuyen,
                    isThamGia = x.c.isThamGia
                });

            List<LichPhongVanChuTriViewModel> itemList = Paginate(projected, nCurrentPage, PageSize > 0 ? PageSize : nPageSize, out PagingInfor paging);
            var listChucDanhs = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var item in itemList)
            {
                item.NgayPhongVanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayPhongVan);
                item.TenChucDanhUngTuyen = listChucDanhs.Where(r => r.Id == item.IdChucDanhUngTuyen).Select(r => r.Ten).FirstOrDefault();
                if (item.isThamGia == 0)
                    item.TrangThaiThamGia = "Chưa xác nhận tham gia";
                if (item.isThamGia == 1)
                    item.TrangThaiThamGia = "Tham gia phỏng vấn";
                if (item.isThamGia == 2)
                    item.TrangThaiThamGia = "Từ chối gia phỏng vấn";
                if (item.isThamGia == 3)
                    item.TrangThaiThamGia = "Ứng viên phỏng vấn đạt";
                if (item.isThamGia == 4)
                    item.TrangThaiThamGia = "Ứng viên phỏng vấn không đạt";
            }
            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = itemList;
            return dnm;

        }

        public dynamic ChuyenTrangThaiLichPhongVan(string Id, int TrangThai, ApplicationDbContext db)
        {

            try
            {
                HRM_TuyenDung_NguoiPhongVan itemNguoiPhongVan = db.HRM_TuyenDung_NguoiPhongVan.Find(Id);
                if (itemNguoiPhongVan == null)
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
                itemNguoiPhongVan.isThamGia = TrangThai;

                HRM_TuyenDung_VongPhongVan VongPhongVan = db.HRM_TuyenDung_VongPhongVan.Where(r => r.IdGuid == itemNguoiPhongVan.IdVongPhongVan).FirstOrDefault();
                var model = db.HRM_TuyenDung_QuyTrinhPhongVan.Where(r => r.IdGuid == VongPhongVan.IdQuyTrinhPhongVan).FirstOrDefault();
                string TenDayDu = db.HRM_NhanVien.Where(r => r.IdGuid == itemNguoiPhongVan.IdNguoiThamGia).Select(r => r.TenDayDu).FirstOrDefault();
                if (VongPhongVan != null)
                {
                    var listNguoiThamGia = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdVongPhongVan == VongPhongVan.IdGuid).Select(r => r.IdNguoiThamGia).ToList();

                    string Vitri = _CallWebService.GetChucDanh_Ten(db, model.IdChucDanhUngTuyen);
                    listNguoiThamGia.Add(model.IdNguoiTao);

                    foreach (var item in listNguoiThamGia)
                    {
                        HRM_NhanVien NhanVien = db.HRM_NhanVien.Where(r => r.IdGuid == item).FirstOrDefault();
                        if (NhanVien == null)
                            continue;
                        string IdUserEOS = _CallWebService.GetNhanSu_IdUserEOS(db, NhanVien.IdNhanVienEOS);
                        if (IdUserEOS != null && IdUserEOS != "" && IdUserEOS != db.IdUser)
                        {

                            AddNotificationNoUserModel notification = new AddNotificationNoUserModel();
                            List<string> listNguoiNhan = new List<string>();
                            listNguoiNhan.Add(IdUserEOS);
                            notification.NguoiGui = db.IdUser;
                            notification.listNguoiNhan = listNguoiNhan;
                            string TieuDe = "";
                            string NoiDung = "";
                            if (TrangThai == 1)
                            {
                                TieuDe = "Nhân viên " + TenDayDu + " Xác nhận đồng ý tham gia phỏng vấn ";
                                NoiDung = " Đồng ý tham vấn ứng viên: " + model.HoTenUngVien + " ứng tuyển vị trí " + Vitri;
                            }
                            else
                            {
                                TieuDe = "Nhân viên " + TenDayDu + " từ chối tham gia phỏng vấn ";
                                NoiDung = " Từ chối ý tham vấn ứng viên: " + model.HoTenUngVien + " ứng tuyển vị trí " + Vitri;
                            }
                            NotificationNoUserModel notificationModel = new NotificationNoUserModel
                            {
                                TieuDe = TieuDe,
                                IdQuyTrinh = model.IdGuid,
                                LoaiThongBao = "XACNHANPHONGVAN",
                                NoiDung = NoiDung,

                            };
                            notification.Item = notificationModel;
                            AddNotification(notification);
                        }
                    }
                }
                db.SaveChanges();




                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
            }

        }

        public dynamic GetThongTinVongPhongVan(string Id, int Loai, ApplicationDbContext db)
        {
            HRM_TuyenDung_VongPhongVan model = new HRM_TuyenDung_VongPhongVan();
            if (Loai == 1)
            {
                long IdNhanVienEOS = _CallWebService.GetNhanSuFromUserPkid(db).Id;
                string IdNhanVien = db.HRM_NhanVien.AsNoTracking().Where(r => r.IdNhanVienEOS == IdNhanVienEOS).Select(r => r.IdGuid).FirstOrDefault();
                model = db.HRM_TuyenDung_VongPhongVan.Find(Id);
                if (model == null) return null;
                var lstNguoiPhongVan = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdVongPhongVan == model.IdGuid && r.IdNguoiThamGia == IdNhanVien).ToList();
                var filesByNguoi = GetFileDinhKemByIds(db, "HRM_TuyenDung_VongPhongVan", lstNguoiPhongVan.Select(x => x.IdGuid));
                foreach (var item in lstNguoiPhongVan)
                {
                    item.lstFileDinhKem = filesByNguoi[item.IdGuid].ToList();
                }
                model.lstNguoiPhongVan = lstNguoiPhongVan;
                var phongvan = db.HRM_TuyenDung_QuyTrinhPhongVan.Find(model.IdQuyTrinhPhongVan);
                var UngVien = phongvan != null ? db.HRM_TuyenDung_UngVien.Find(phongvan.IdUngVien) : null;
                if (UngVien != null)
                {
                    model.ViTriUngTuyen = _CallWebService.GetChucDanh_Ten(db, UngVien.IddmChucDanhUngTuyen);
                    model.UngVien = UngVien;
                }

            }

            else
            {
                var nguoithamGia = db.HRM_TuyenDung_NguoiPhongVan.Find(Id);
                if (nguoithamGia == null) return null;
                List<HRM_TuyenDung_NguoiPhongVan> lstNguoiPhongVan = new List<HRM_TuyenDung_NguoiPhongVan>();
                lstNguoiPhongVan.Add(nguoithamGia);
                var filesByNguoi = GetFileDinhKemByIds(db, "HRM_TuyenDung_VongPhongVan", lstNguoiPhongVan.Select(x => x.IdGuid));
                foreach (var item in lstNguoiPhongVan)
                {
                    item.lstFileDinhKem = filesByNguoi[item.IdGuid].ToList();
                }
                model = db.HRM_TuyenDung_VongPhongVan.Find(nguoithamGia.IdVongPhongVan);
                if (model == null) return null;
                model.lstNguoiPhongVan = lstNguoiPhongVan;
                var phongvan = db.HRM_TuyenDung_QuyTrinhPhongVan.Find(model.IdQuyTrinhPhongVan);
                var UngVien = phongvan != null ? db.HRM_TuyenDung_UngVien.Find(phongvan.IdUngVien) : null;
                if (UngVien != null)
                {
                    model.ViTriUngTuyen = _CallWebService.GetChucDanh_Ten(db, UngVien.IddmChucDanhUngTuyen);
                    model.UngVien = UngVien;
                }
            }

            return model;

        }
        public dynamic SetNhanXetNguoiPhongVan(HRM_TuyenDung_NguoiPhongVan model, ApplicationDbContext db)
        {
            try
            {
                db.Entry(model).State = EntityState.Modified;
                if (model.lstFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.lstFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_TuyenDung_NguoiPhongVan, model.IdGuid);
                    }
                }
                var vongPV = db.HRM_TuyenDung_VongPhongVan.Find(model.IdVongPhongVan);
                if (vongPV.IdNguoiChuTri == model.IdNguoiThamGia && model.isThamGia > 2)
                {
                    var QuyTrinhPhongvan = db.HRM_TuyenDung_QuyTrinhPhongVan.Find(vongPV.IdQuyTrinhPhongVan);
                    vongPV.MucLuongThoaThuan = model.MucLuongThoaThuan;
                    vongPV.MucLuongUngVienDeNghi = model.MucLuongUngVienDeNghi;
                    if (model.isThamGia == 3)
                    {
                        vongPV.MucLuongThoaThuan = model.MucLuongThoaThuan;
                        QuyTrinhPhongvan.MucLuongThoaThuan = vongPV.MucLuongThoaThuan;
                        if (model.MucLuongUngVienDeNghi > 0)
                            vongPV.MucLuongUngVienDeNghi = model.MucLuongUngVienDeNghi;
                        QuyTrinhPhongvan.MucLuongUngVienDeNghi = vongPV.MucLuongUngVienDeNghi;

                        vongPV.TrangThai = 15;
                        HRM_TuyenDung_VongPhongVan VongPhongVan = db.HRM_TuyenDung_VongPhongVan.Where(r => r.TrangThai == 0 && r.VongPhongVan > vongPV.VongPhongVan && r.IdGuid != vongPV.IdGuid && r.IdQuyTrinhPhongVan == vongPV.IdQuyTrinhPhongVan).OrderBy(r => r.VongPhongVan).FirstOrDefault();
                        //QuyTrinhPhongvan.TrangThai = 5;


                        if (VongPhongVan != null)
                        {

                            var listNguoiThamGia = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdVongPhongVan == VongPhongVan.IdGuid).ToList();
                            foreach (var item in listNguoiThamGia)
                            {
                                long IdNhanVienEOS = db.HRM_NhanVien.Where(r => r.IdGuid == item.IdNguoiThamGia).Select(r => r.IdNhanVienEOS).FirstOrDefault();
                                string IdUserEOS = _CallWebService.GetNhanSu_IdUserEOS(db, IdNhanVienEOS);
                                if (IdUserEOS != null && IdUserEOS != "")
                                {
                                    string Vitri = _CallWebService.GetChucDanh_Ten(db, QuyTrinhPhongvan.IdChucDanhUngTuyen);
                                    AddNotificationNoUserModel notification = new AddNotificationNoUserModel();
                                    List<string> listNguoiNhan = new List<string>();
                                    listNguoiNhan.Add(IdUserEOS);
                                    notification.NguoiGui = db.IdUser;
                                    notification.listNguoiNhan = listNguoiNhan;

                                    NotificationNoUserModel notificationModel = new NotificationNoUserModel
                                    {
                                        TieuDe = "Bạn nhận được 1 lịch mời tham gia phỏng vấn vào thời gian " + VongPhongVan.NgayPhongVan.ToString("dd/MM/yyyy"),
                                        IdQuyTrinh = VongPhongVan.IdGuid,
                                        LoaiThongBao = "LICHMOIPHONGVAN",
                                        NoiDung = " Phỏng vấn ứng viên: " + QuyTrinhPhongvan.HoTenUngVien + " ứng tuyển vị trí " + Vitri,

                                    };
                                    notification.Item = notificationModel;
                                    AddNotification(notification);
                                }
                            }

                        }
                        else
                        {
                            QuyTrinhPhongvan.TrangThai = 10;
                            // QuyTrinhPhongvan.TrangThai = 15;
                            var NhanVien = db.HRM_NhanVien.Where(r => r.IdGuid == model.IdNguoiThamGia).FirstOrDefault();
                            long IdNhanVienEOS = db.HRM_NhanVien.Where(r => r.IdGuid == QuyTrinhPhongvan.IdNguoiTao).Select(r => r.IdNhanVienEOS).FirstOrDefault();
                            string IdUserEOS = _CallWebService.GetNhanSu_IdUserEOS(db, IdNhanVienEOS);
                            if (IdUserEOS != null && IdUserEOS != "")
                            {
                                string Vitri = _CallWebService.GetChucDanh_Ten(db, QuyTrinhPhongvan.IdChucDanhUngTuyen);
                                AddNotificationNoUserModel notification = new AddNotificationNoUserModel();
                                List<string> listNguoiNhan = new List<string>();
                                listNguoiNhan.Add(IdUserEOS);
                                notification.NguoiGui = db.IdUser;
                                notification.listNguoiNhan = listNguoiNhan;

                                NotificationNoUserModel notificationModel = new NotificationNoUserModel
                                {
                                    TieuDe = " Xác nhận phỏng vấn đạt cho ứng viên " + QuyTrinhPhongvan.HoTenUngVien,
                                    IdQuyTrinh = QuyTrinhPhongvan.IdGuid,
                                    LoaiThongBao = "PHONGVANDAT",
                                    NoiDung = NhanVien.TenDayDu + " Xác nhận phỏng vấn đạt cho ứng viên: " + QuyTrinhPhongvan.HoTenUngVien + " ứng tuyển vị trí " + Vitri + " đã vượt qua quy trình phỏng vấn",

                                };
                                notification.Item = notificationModel;
                                AddNotification(notification);
                            }
                        }
                    }
                    else
                    {
                        HRM_TuyenDung_UngVien ungVien = db.HRM_TuyenDung_UngVien.Find(QuyTrinhPhongvan.IdUngVien);
                        if (ungVien != null)
                        {
                            ungVien.TrangThai = 5;
                            ungVien.TenTrangThai = " Ứng viên phỏng vấn không đạt ở vòng số " + vongPV.VongPhongVan;
                        }
                        vongPV.TrangThai = 20;
                        QuyTrinhPhongvan.TrangThai = 15;
                        var listIDNguoiThamGia = db.HRM_TuyenDung_NguoiPhongVan.Where(r => r.IdVongPhongVan == model.IdVongPhongVan && r.IdNguoiThamGia != model.IdNguoiThamGia).Select(r => r.IdNguoiThamGia).ToList();

                        if (QuyTrinhPhongvan.CreatedBy != null)
                        {
                            //    var NhanVien = db.HRM_NhanVien.Where(r => r.IdGuid == item).FirstOrDefault();
                            //  long IdNhanVienEOS = db.HRM_NhanVien.Where(r => r.IdGuid== item).Select(r => r.IdNhanVienEOS).FirstOrDefault();
                            var NhanVien = _CallWebService.GetNhanSuFromUserPkid(db, QuyTrinhPhongvan.CreatedBy);
                            if (NhanVien != null)
                            {
                                string Vitri = _CallWebService.GetChucDanh_Ten(db, QuyTrinhPhongvan.IdChucDanhUngTuyen);
                                AddNotificationNoUserModel notification = new AddNotificationNoUserModel();
                                List<string> listNguoiNhan = new List<string>();
                                listNguoiNhan.Add(NhanVien.UserPkid);

                                notification.NguoiGui = db.IdUser;
                                notification.listNguoiNhan = listNguoiNhan;

                                NotificationNoUserModel notificationModel = new NotificationNoUserModel
                                {
                                    TieuDe = " Xác nhận phỏng vấn không đạt cho ứng viên " + QuyTrinhPhongvan.HoTenUngVien,
                                    IdQuyTrinh = QuyTrinhPhongvan.IdGuid,
                                    LoaiThongBao = "PHONGVANKHONGDAT",
                                    NoiDung = NhanVien.TenDayDu + " Xác nhận phỏng vấn đạt không cho ứng viên: " + QuyTrinhPhongvan.HoTenUngVien + " ứng tuyển vị trí " + Vitri + " đã vượt không qua quy trình phỏng vấn",

                                };
                                notification.Item = notificationModel;
                                AddNotification(notification);
                            }
                        }


                    }
                }
                db.SaveChanges();

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", model.IdGuid);
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật thành không công " + ex.Message);
            }
        }

        public void UpdateDonVi(ApplicationDbContext db)
        {
            var NhanViens = (from a in db.HRM_NhanVien
                             join b in db.HRM_QuyTrinhBoPhan_BoPhan on a.IdGuid equals b.IdQuyTrinh into g
                             where !string.IsNullOrEmpty(a.IdUngVien) && !g.Any()
                             select a
                           );
            List<string> IdUngVien = NhanViens.Select(a => a.IdUngVien).Distinct().ToList();
            var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => IdUngVien.Contains(a.IdGuid)).ToList();
            List<HRM_QuyTrinhBoPhan_BoPhan> listAdd = new List<HRM_QuyTrinhBoPhan_BoPhan>();
            foreach (var aModel in NhanViens)
            {
                if (string.IsNullOrEmpty(aModel.UserName))
                    aModel.UserName = aModel.MaNhanVien;
                aModel.listIdCongTy = listUngVien.Where(a => a.IdGuid == aModel.IdUngVien).Select(a => a.IdCoSoTrungTuyen ?? 0).ToList();
                if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                {
                    if (aModel.listIdCongTy != null)
                        foreach (var item in aModel.listIdCongTy)
                        {
                            HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                            bp.IdGuid = Guid.NewGuid().ToString();
                            bp.IdQuyTrinh = aModel.IdGuid;
                            bp.IdBoPhan = item;
                            bp.TableName = nameof(HRM_NhanVien);
                            db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                        }
                    clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(db, aModel);
                    aModel.IdUser = _clsNhanVien.UserPkid;
                    aModel.IdNhanVienEOS = _clsNhanVien.Id;
                    db.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            db.SaveChanges();
        }
        #endregion
        public dynamic GetNextPheDuyetLuong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.Select(x => x.SoQuyTrinh), "PDL"));
        public dynamic SetQuyTrinhPheDuyetLuong(HRM_TuyenDung_QuyTrinhPheDuyetLuong model, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    if (model.listUngVien != null && model.listUngVien.Count > 0)
                    {
                        List<string> listMaNV = model.listUngVien.Where(a => !string.IsNullOrEmpty(a.MaNhanVien) && string.IsNullOrEmpty(a.IdNhanVien)).Select(a => a.MaNhanVien).Distinct().ToList();
                        var NhanVien = db.HRM_NhanVien.Where(a => listMaNV.Contains(a.MaNhanVien)).ToList();
                        if (NhanVien != null && NhanVien.Count > 0)
                        {
                            List<string> listMaTrung = NhanVien.Select(a => a.MaNhanVien).Distinct().ToList();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, string.Join(", ", listMaTrung));
                        }
                        var UngVienTrung = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(a => !string.IsNullOrEmpty(a.MaNhanVien) && a.IdQuyTrinhPheDuyet != model.IdGuid && listMaNV.Contains(a.MaNhanVien)).ToList();
                        if (UngVienTrung != null && UngVienTrung.Count > 0)
                        {
                            List<string> listMaTrung = UngVienTrung.Select(a => a.MaNhanVien).Distinct().ToList();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã Nhân Viên đã có: " + string.Join(", ", listMaTrung));
                        }
                    }
                    var QuyTrinhdb = db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.AsNoTracking().FirstOrDefault(a => a.IdGuid == model.IdGuid);
                    if (!SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, model, err))
                        return err;

                    bool check = FuncQuyTrinhPheDuyetLuong(model, db, err, eQuyTrinh_LoaiHam.Set);
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
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công", model.IdGuid);

                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Có lỗi trong quá trình xử lý " + ex.Message);
                }
            }
        }
        public dynamic GetDanhSachQuyTrinhPheDuyetLuong(dynamic dyn, ApplicationDbContext db = null)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {

                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
                int PageSize = itemTimKiem.PageSize ?? 25;
                IQueryable<HRM_TuyenDung_QuyTrinhPheDuyetLuong> result;
                if (itemTimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_TuyenDung_QuyTrinhPheDuyetLuong
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (itemTimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_TuyenDung_QuyTrinhPheDuyetLuong
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(itemTimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(itemTimKiem.DenNgay ?? 0);
                //var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                //if (listCongTy != null)
                //{
                //    List<long> listId = listCongTy.Select(a => a.Id).Distinct().ToList();
                //    result = result.Where(a => listId.Contains(a.IdCongTy));
                //}
                if (itemTimKiem.KeyWord != null && itemTimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim())||(!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim())));
                if (itemTimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTy == itemTimKiem.IdCongTy);
                var itemList = Paginate(result.AsNoTracking().Distinct(), nCurrentPage, PageSize, out PagingInfor paging);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachDuAn(db);
                foreach (var item in itemList)
                {
                    item.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayDeNghi);
                    item.TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == item.IdTongHopDeNghiTuyenDung).FirstOrDefault();
                    BoPhan duAn = BoPhans.Find(x => x.Id == item.IdCongTy);
                    if (duAn != null)
                        item.TenCongty = duAn.TenBoPhan;

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
        public dynamic GetDanhSachUngVienPheDuyetLuong(long IdCongTy, long IdBoPhan = 0, ApplicationDbContext db = null)
        {
            List<HRM_TuyenDung_UngVien> listresult = new List<HRM_TuyenDung_UngVien>();
            try
            {
                listresult = (from a in db.HRM_TuyenDung_UngVien
                              where a.IdCoSoTrungTuyen == IdCongTy && (IdBoPhan == 0 || a.IdBoPhanEOS == IdBoPhan) && a.isDaPheDuyetLuong == false && a.IsNhanViec == true && a.TrangThai == 3
                              select a
                            ).ToList();
                if (listresult != null && listresult.Count > 0)
                {
                    List<string> listIdUngVien = listresult.Select(a => a.IdGuid).Distinct().ToList();
                    List<HRM_UngVien_PhuCap> listPhuCap = db.HRM_UngVien_PhuCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).ToList();
                    foreach (var item in listresult)
                    {
                        item.lisPhuCap = new List<HRM_UngVien_PhuCap>();
                        if (item.NgaySinh != null)
                            item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                        if (item.NgayTiepNhan != null)
                            item.NgayTiepNhanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTiepNhan);
                        if (item.NgayNhanViecDuKien != null)
                            item.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayNhanViecDuKien);
                        if (listPhuCap != null && listPhuCap.Count > 0)
                            item.lisPhuCap = listPhuCap.Where(a => a.IdUngVien == item.IdGuid).OrderBy(a => a.Created).ToList();
                    }
                }
            }
            catch (Exception e)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
            return listresult;
        }
        public MessageReturn ChuyenTiepQuyTrinhPheDuyetLuong(HRM_TuyenDung_QuyTrinhPheDuyetLuong QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    List<string> listIdUngVien = QuyTrinh.listUngVien.Select(a => a.IdUngVien).Distinct().ToList();
                    var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => a.isDaPheDuyetLuong == true && listIdUngVien.Contains(a.IdGuid)).ToList();
                    if (listUngVien != null && listUngVien.Count > 0)
                    {
                        string loi = string.Join(",", listUngVien.Select(a => a.HoTenDayDu));
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Ứng viên đã được phê duyệt lương trước đó :" + loi);
                    }

                    if (QuyTrinh.listUngVien != null && QuyTrinh.listUngVien.Count > 0)
                    {
                        List<string> listMaNV = QuyTrinh.listUngVien.Where(a => !string.IsNullOrEmpty(a.MaNhanVien) && string.IsNullOrEmpty(a.IdNhanVien)).Select(a => a.MaNhanVien).Distinct().ToList();
                        var NhanVien = db.HRM_NhanVien.Where(a => listMaNV.Contains(a.MaNhanVien)).ToList();
                        if (NhanVien != null && NhanVien.Count > 0)
                        {
                            List<string> listMaTrung = NhanVien.Select(a => a.MaNhanVien).Distinct().ToList();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã Nhân Viên đã tồn tại: " + string.Join(", ", listMaTrung));
                        }
                        var UngVienTrung = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(a => !string.IsNullOrEmpty(a.MaNhanVien) && a.IdQuyTrinhPheDuyet != QuyTrinh.IdGuid && listMaNV.Contains(a.MaNhanVien)).ToList();
                        if (UngVienTrung != null && UngVienTrung.Count > 0)
                        {
                            List<string> listMaTrung = UngVienTrung.Select(a => a.MaNhanVien).Distinct().ToList();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã Nhân Viên đã có: " + string.Join(", ", listMaTrung));
                        }
                    }
                    var listUngVienKhongDuyet = QuyTrinh.listUngVien.Where(a => a.TrangThai == 2).ToList();
                    QuyTrinh.listUngVien = QuyTrinh.listUngVien.Where(a => a.TrangThai == 1).ToList();

                    HRM_TuyenDung_QuyTrinhPheDuyetLuong pcuDatabase = db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdCongTy))
                        return err;
                    bool Error = FuncQuyTrinhPheDuyetLuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
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
                    if (listUngVienKhongDuyet != null && listUngVienKhongDuyet.Count > 0)
                    {
                        var QuyTrinhNew = new HRM_TuyenDung_QuyTrinhPheDuyetLuong();
                        QuyTrinhNew.IdCongTy = QuyTrinh.IdCongTy;
                        QuyTrinhNew.NgayDeNghi = DateTime.Now;
                        QuyTrinhNew.isKetThuc = false;
                        string dyn = JsonConvert.SerializeObject(GetNextPheDuyetLuong(db));
                        SoPhieuView soPhieu = JsonConvert.DeserializeObject<SoPhieuView>(dyn.ToString());
                        QuyTrinhNew.SoQuyTrinh = soPhieu.SoPhieu;
                        QuyTrinhNew.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(DateTime.Now);
                        QuyTrinhNew.listUngVien = new List<HRM_TuyenDung_PheDuyetLuong_UngVien>();
                        QuyTrinhNew.listUngVien = listUngVienKhongDuyet;
                        string Errord = FuncQuyTrinhPheDuyetLuongKhongDuyet(QuyTrinhNew, db);
                        if (Errord != "0")
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Errord, QuyTrinhNew.IdGuid);
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
        public class SoPhieuView
        {
            public string SoPhieu { get; set; }
        }
        public MessageReturn KhongDuyetQuyTrinhPheDuyetLuong(HRM_TuyenDung_QuyTrinhPheDuyetLuong QuyTrinh, ApplicationDbContext db)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
                    HRM_TuyenDung_QuyTrinhPheDuyetLuong pcuDatabase = db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    bool Error = FuncQuyTrinhPheDuyetLuong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
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
        public bool FuncQuyTrinhPheDuyetLuong(HRM_TuyenDung_QuyTrinhPheDuyetLuong model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                model.NgayDeNghi = dungchung.UnixTimeStampToDateTime(model.NgayDeNghiUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdDuAn = model.IdCongTy;
                    model.eAction = DefineData.eAction.QUYTRINHPHEDUYETLUONGUNGVIEN.ToString();
                    if (!KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return false;
                    db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngViendb = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(a => a.IdQuyTrinhPheDuyet == model.IdGuid).ToList();
                if (listUngViendb.Count > 0)
                    db.HRM_TuyenDung_PheDuyetLuong_UngVien.RemoveRange(listUngViendb);
                List<HRM_PheDuyetLuong_UngVien_PhuCap> HRM_PheDuyetLuong_UngVien_PhuCap = db.HRM_PheDuyetLuong_UngVien_PhuCap.Where(a => a.IdQuyTrinhPheDuyetLuong == model.IdGuid).ToList();
                if (HRM_PheDuyetLuong_UngVien_PhuCap.Count > 0)
                    db.HRM_PheDuyetLuong_UngVien_PhuCap.RemoveRange(HRM_PheDuyetLuong_UngVien_PhuCap);
                int Stt = 1;
                List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngVienAdd = new List<HRM_TuyenDung_PheDuyetLuong_UngVien>();
                foreach (var UngVien in model.listUngVien)
                {
                    HRM_TuyenDung_PheDuyetLuong_UngVien item = UngVien.Clone();
                    item.IdQuyTrinhPheDuyet = model.IdGuid;
                    if (item.NgayNhanViecDuKienUnix > 0)
                        item.NgayNhanViecDuKien = dungchung.UnixTimeStampToDateTime(item.NgayNhanViecDuKienUnix);
                    if (item.KetThucThuViecUnix > 0)
                        item.KetThucThuViec = dungchung.UnixTimeStampToDateTime(item.KetThucThuViecUnix);

                    item.IdGuid = Guid.NewGuid().ToString();
                    item.SapXep = Stt;
                    Stt++;
                    if (item.lisPhuCap != null)
                    {
                        List<HRM_PheDuyetLuong_UngVien_PhuCap> listAdd = new List<HRM_PheDuyetLuong_UngVien_PhuCap>();
                        foreach (var phuCap in item.lisPhuCap)
                        {
                            var Pc = new HRM_PheDuyetLuong_UngVien_PhuCap();
                            Pc.IdGuid = Guid.NewGuid().ToString();
                            Pc.IdQuyTrinhPheDuyetLuong = model.IdGuid;
                            Pc.IdUngVien = item.IdUngVien;
                            Pc.SoTien = phuCap.SoTien;
                            Pc.IddmPhuCap = phuCap.IddmPhuCap;
                            Pc.isChon = phuCap.isChon;
                            listAdd.Add(Pc);

                        }
                        db.HRM_PheDuyetLuong_UngVien_PhuCap.AddRange(listAdd);
                    }

                    listUngVienAdd.Add(item);
                }
                db.HRM_TuyenDung_PheDuyetLuong_UngVien.AddRange(listUngVienAdd);
                if (model.isKetThuc == true)
                {
                    var CongTy = _CallWebService.GetCongTyKyHopDong(db).FirstOrDefault();

                    List<string> listIdUngVien = model.listUngVien.Where(a => a.TrangThai == 1).Select(a => a.IdUngVien).Distinct().ToList();
                    var listUngVien = db.HRM_TuyenDung_UngVien.Where(a => listIdUngVien.Contains(a.IdGuid)).ToList();
                    var listCapBacLuong = db.HRM_dmCapBacLuong.ToList();
                    var fcNhanVien = _FuncNhanVien;
                    var fcHopDong = _funcHopDong;
                    foreach (var item in listUngVien)
                    {
                        item.isDaPheDuyetLuong = true;
                        db.Entry(item).State = EntityState.Modified;
                        HRM_NhanVien NhanVien = new HRM_NhanVien(item);

                        if (item.ThuCoc != null && item.ThuCoc > 0)
                        {
                            NhanVien.ThuCoc = item.ThuCoc;
                            NhanVien.IsDaThuCoc = false;
                        }
                        var pheDuyet = model.listUngVien.Where(a => a.IdUngVien == item.IdGuid).FirstOrDefault();
                        if (!string.IsNullOrEmpty(pheDuyet.IdNhanVien))
                            NhanVien.IdGuid = pheDuyet.IdNhanVien;
                        NhanVien.MaNhanVien = pheDuyet.MaNhanVien;
                        NhanVien.NgayNhanViecDuKien = pheDuyet.NgayNhanViecDuKien;
                        NhanVien.NgayNhanViecDuKienUnix = pheDuyet.NgayNhanViecDuKienUnix;
                        NhanVien.IdCaLamViec = pheDuyet.IdCaLamViec;
                        NhanVien.IdCapBacLuongChapNhan = pheDuyet.IdCapBacLuongChapNhan;
                        NhanVien.IdCapBacLuongDeXuat = pheDuyet.IdCapBacLuongDeXuat;
                        NhanVien.IdBoPhanEOS = pheDuyet.IdBoPhan;
                        NhanVien.listIdCongTy = new List<long>();
                        NhanVien.listIdCongTy.Add(item.IdCoSoTrungTuyen ?? 0);
                        NhanVien.IdChucDanh = pheDuyet.IddmTrungTuyen;
                        NhanVien.IdChucVu = item.IdChucVu;
                        NhanVien.PhanTramLuongThuViec = pheDuyet.PhanTramLuongThuViec;
                        NhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                        NhanVien.listPhuCap = new List<HRM_NhanVien_PhuCap>();
                        var UngVien = model.listUngVien.Where(a => a.IdUngVien == item.IdGuid).FirstOrDefault();
                        NhanVien.listGiayToTuyThan = new List<HRM_NhanVien_GiayToTuyThan>();
                        NhanVien.listGiayToTuyThan.Add(new HRM_NhanVien_GiayToTuyThan()
                        {
                            Loai = 1,
                            So = item.CCCD,
                            NoiCap = item.NoiCap,
                            NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap),
                            isApDung = true
                        });
                        NhanVien.listSoDienThoai = new List<HRM_NhanVien_SoDienThoai>();
                        NhanVien.listSoDienThoai.Add(new HRM_NhanVien_SoDienThoai()
                        {
                            SoDienThoai = item.DienThoaiDiDong,
                            isApDung = true
                        });
                        NhanVien.listEmail = new List<HRM_NhanVien_Email>();
                        NhanVien.listEmail.Add(new HRM_NhanVien_Email()
                        {
                            Email = item.Email,
                            isApDung = true
                        });
                        var listKinhNghiemUngVien = db.HRM_TuyenDung_UngVienKinhNghiemLamViec.Where(a => a.IdTuyenDungUngVien == item.IdGuid).ToList();
                        if (listKinhNghiemUngVien != null && listKinhNghiemUngVien.Count > 0)
                        {
                            NhanVien.ListQuaTrinhCongTacNgoaiCongTy = listKinhNghiemUngVien.Select(a => new HRM_QuaTrinhCongTacNgoaiCongTy
                            {
                                ThoiGian = a.ThoiGianLamViec,
                                DonViCongTac = a.DonViCongTac,
                                ViTriCongViec = a.ViTri
                            }).ToList();

                        }
                        var listChungChiUngVien = db.HRM_ChungChiUngVien.Where(a => a.IdUngVien == item.IdGuid).ToList();
                        if (listChungChiUngVien != null && listChungChiUngVien.Count > 0)
                        {
                            NhanVien.ListNhanVienChungChi = listChungChiUngVien.Select(a => new HRM_ChungChiNhanVien
                            {
                                TenChungChi = a.TenChungChi,
                                IdNhomChungChi = a.IdNhomChungChi,
                                TrinhDo = a.TrinhDo,
                                NgayCap = a.NgayCap,
                                NgayHetHan = a.NgayHetHan,
                                NgayCapUnix = DateTimeToUnixTimeStamp(a.NgayCap),
                                NgayHetHanUnix = DateTimeToUnixTimeStamp(a.NgayHetHan),
                                NoiCap = a.NoiCap,
                                lstFileDinhKem = a.lstFileDinhKem
                            }).ToList();

                        }
                        var listBangCapUngVien = db.HRM_UngVienBangCap.Where(a => a.IdUngVien == item.IdGuid).ToList();
                        if (listBangCapUngVien != null && listBangCapUngVien.Count > 0)
                        {
                            NhanVien.ListNhanVienBangCap = listBangCapUngVien.Select(a => new HRM_NhanVienBangCap
                            {
                                NoiDaoTao = a.NoiDaoTao,
                                ChuyenNganh = a.ChuyenNganh,
                                IdTrinhDoDaoTao = a.IdTrinhDoDaoTao,
                                NgayTotNghiep = new DateTime(a.NamTotNghiep ?? DateTime.Now.Year, 5, 1),
                                lstFileDinhKem = a.lstFileDinhKem
                            }).ToList();

                        }
                        var listNgonNguUngVien = db.HRM_NgonNguUngVien.Where(a => a.IdUngVien == item.IdGuid).ToList();
                        if (listNgonNguUngVien != null && listNgonNguUngVien.Count > 0)
                        {
                            NhanVien.ListNhanVienNgonNgu = listNgonNguUngVien.Select(a => new HRM_NgonNguNhanVien
                            {
                                IdNgonNgu = a.IdNgonNgu,
                                IdXepLoaiNghe = a.IdXepLoaiNghe,
                                IdXepLoaiNoi = a.IdXepLoaiNoi,
                                IdXepLoaiViet = a.IdXepLoaiViet,
                                IdXepLoaiDoc = a.IdXepLoaiDoc,
                                lstFileDinhKem = a.lstFileDinhKem
                            }).ToList();

                        }

                        HRM_HopDong HopDong = new HRM_HopDong();
                        if (Host == "PHT")
                            HopDong.IdCongTyKyHopDong = CongTy.Id;
                        HopDong.lstPhuCap = new List<HRM_HopDongPhuCap>();
                        if (UngVien.lisPhuCap != null && UngVien.lisPhuCap.Count > 0)
                        {
                            foreach (var Pc in UngVien.lisPhuCap.Where(a => a.isChon == true).ToList())
                            {
                                HRM_NhanVien_PhuCap phuCap = new HRM_NhanVien_PhuCap();
                                phuCap.IddmPhuCap = Pc.IddmPhuCap;
                                phuCap.SoTien = Pc.SoTien;
                                NhanVien.listPhuCap.Add(phuCap);
                                HRM_HopDongPhuCap PhuCapHopDong = new HRM_HopDongPhuCap();
                                PhuCapHopDong.IddmLoaiPhuCap = Pc.IddmPhuCap;
                                PhuCapHopDong.SoTien = Pc.SoTien;
                                HopDong.lstPhuCap.Add(PhuCapHopDong);
                            }
                        }
                        NhanVien.Luong = item.Luong;
                        fcNhanVien.SetThongTinNhanVienTransaction(NhanVien, db);
                        HopDong.IdNhanVien = NhanVien.IdGuid;
                        HopDong.IdCongTyEOS = NhanVien.IdCongTyEOS;
                        HopDong.IdBoPhanEOS = NhanVien.IdBoPhanEOS;
                        HopDong.IddmChucDanh = NhanVien.IdChucDanh;
                        HopDong.IddmChucVu = NhanVien.IdChucVu;
                        HopDong.IdThoiGianLamViec = UngVien.IdCaLamViec;
                        HopDong.listIdCongTy = new List<long>();
                        HopDong.listIdCongTy.Add(item.IdCoSoTrungTuyen ?? 0);
                        HopDong.IdLoaiHopDong = UngVien.IddmLoaiHopDong;
                        HopDong.NgayKy = item.NgayNhanViecDuKien ?? DateTime.Now;
                        HopDong.NgayHieuLuc = item.NgayNhanViecDuKien ?? DateTime.Now;
                        if (item.ThoiGianThuViec > 0 && item.NgayNhanViecDuKien != null && item.NgayNhanViecDuKien.Value.Year > 2000)
                            HopDong.NgayHetHan = item.NgayNhanViecDuKien.Value.AddDays(item.ThoiGianThuViec * 30 - 1);

                        HopDong.LuongHopDong = new HRM_Luong_HopDong();
                        if (UngVien.ThoiGianThuViec > 0)
                            HopDong.IdThoiHanHopDong = db.HRM_dmThoiHanHopDong.Where(a => a.SoThang == UngVien.ThoiGianThuViec).Select(a => a.IdGuid).FirstOrDefault();
                        string IdHInhThucTraLuong = db.HRM_dmHinhThucTraLuong.Where(a => a.Ma == "TM").Select(a => a.IdGuid).FirstOrDefault();
                        HopDong.LoaiLuong = UngVien.LoaiLuong;
                        HopDong.LuongHopDong = new HRM_Luong_HopDong();
                        HopDong.LuongHopDong.LuongBaoHiem = UngVien.Luong;
                        HopDong.LuongHopDong.TyLeHuongLuong = UngVien.PhanTramLuongThuViec;
                        HopDong.LuongHopDong.IddmHinhThucTraLuong = IdHInhThucTraLuong;
                        HopDong.LuongBaoHiem = new HRM_LuongBaoHiem_HopDong();
                        HopDong.LuongBaoHiem.LuongBaoHiem = UngVien.LuongBHXH;
                        HopDong.LuongBaoHiem.TyLeHuongLuong = UngVien.PhanTramLuongThuViec;
                        HopDong.LuongBaoHiem.IddmHinhThucTraLuong = IdHInhThucTraLuong;
                        var CapBacLuong = listCapBacLuong.FirstOrDefault(a => a.IdGuid == pheDuyet.IdCapBacLuongChapNhan);
                        if (CapBacLuong != null)
                            HopDong.LuongHopDong.LuongBaoHiem = CapBacLuong.Luong;
                        HopDong.LuongHopDong.TyLeHuongLuong = pheDuyet.PhanTramLuongThuViec;
                        fcHopDong.SetHopDongNoTransaction(HopDong, db);

                    }


                }
                if (!SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return false;
                db.SaveChanges();



                return true;
            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }
        }
        public string FuncQuyTrinhPheDuyetLuongKhongDuyet(HRM_TuyenDung_QuyTrinhPheDuyetLuong model, ApplicationDbContext db)
        {
            try
            {
                model.NgayDeNghi = dungchung.UnixTimeStampToDateTime(model.NgayDeNghiUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHPHEDUYETLUONGUNGVIEN.ToString(), model.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        return "Không tìm thấy trạng thái dữ liệu khi chuyển";
                    }
                    model.IdTrangThai = trangThai.IdTrangThai;
                    model.TenTrangThai = trangThai.TenTrangThai;

                    db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.Add(model);
                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngViendb = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(a => a.IdQuyTrinhPheDuyet == model.IdGuid).ToList();
                if (listUngViendb.Count > 0)
                    db.HRM_TuyenDung_PheDuyetLuong_UngVien.RemoveRange(listUngViendb);
                List<HRM_PheDuyetLuong_UngVien_PhuCap> HRM_PheDuyetLuong_UngVien_PhuCap = db.HRM_PheDuyetLuong_UngVien_PhuCap.Where(a => a.IdQuyTrinhPheDuyetLuong == model.IdGuid).ToList();
                if (HRM_PheDuyetLuong_UngVien_PhuCap.Count > 0)
                    db.HRM_PheDuyetLuong_UngVien_PhuCap.RemoveRange(HRM_PheDuyetLuong_UngVien_PhuCap);

                int Stt = 1;
                List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngVienAdd = new List<HRM_TuyenDung_PheDuyetLuong_UngVien>();
                foreach (var UngVien in model.listUngVien)
                {
                    HRM_TuyenDung_PheDuyetLuong_UngVien item = UngVien.Clone();
                    item.IdQuyTrinhPheDuyet = model.IdGuid;
                    if (item.NgayNhanViecDuKienUnix > 0)
                        item.NgayNhanViecDuKien = dungchung.UnixTimeStampToDateTime(item.NgayNhanViecDuKienUnix);
                    if (item.KetThucThuViecUnix > 0)
                        item.KetThucThuViec = dungchung.UnixTimeStampToDateTime(item.KetThucThuViecUnix);

                    item.IdGuid = Guid.NewGuid().ToString();
                    item.SapXep = Stt;
                    Stt++;
                    if (item.lisPhuCap != null)
                    {
                        List<HRM_PheDuyetLuong_UngVien_PhuCap> listAdd = new List<HRM_PheDuyetLuong_UngVien_PhuCap>();
                        foreach (var phuCap in item.lisPhuCap)
                        {
                            var Pc = new HRM_PheDuyetLuong_UngVien_PhuCap();
                            Pc.IdGuid = Guid.NewGuid().ToString();
                            Pc.IdQuyTrinhPheDuyetLuong = model.IdGuid;
                            Pc.IdUngVien = item.IdUngVien;
                            Pc.SoTien = phuCap.SoTien;
                            Pc.IddmPhuCap = phuCap.IddmPhuCap;
                            Pc.isChon = phuCap.isChon;
                            listAdd.Add(Pc);

                        }
                        db.HRM_PheDuyetLuong_UngVien_PhuCap.AddRange(listAdd);
                    }

                    listUngVienAdd.Add(item);
                }
                db.HRM_TuyenDung_PheDuyetLuong_UngVien.AddRange(listUngVienAdd);

                db.SaveChanges();



                return "0";
            }
            catch (Exception ex)
            {
                string msg = "Có lỗi trong quá trình xử lý " + ex.Message;
                return msg;
            }
        }

        public dynamic GetByIdQuyTrinhPheDuyetLuongUngVien(string Id, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            HRM_TuyenDung_QuyTrinhPheDuyetLuong model = db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.Find(Id);
            if (!GetQuyTrinh(db, _CallWebService, model, err, true))
                return err;
            model.NgayDeNghiUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayDeNghi);
            model.TongHopDeNghiTuyenDung = db.HRM_TuyenDung_DeNghi_TongHop.Where(r => r.IdGuid == model.IdTongHopDeNghiTuyenDung).FirstOrDefault();
            var listUngVien = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(r => r.IdQuyTrinhPheDuyet == Id).OrderBy(a => a.SapXep).ToList();
            List<string> listIdUngVien = listUngVien.Select(a => a.IdUngVien).Distinct().ToList();
            var listTuyenDungUngVien = db.HRM_TuyenDung_UngVien.Where(a => listIdUngVien.Contains(a.IdGuid)).ToList();
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listPhuCap = db.HRM_PheDuyetLuong_UngVien_PhuCap.Where(a => a.IdQuyTrinhPheDuyetLuong == Id).ToList();
            var listdmPhuCap = db.HRM_dmPhuCap.ToList();
            List<string> listIdNhanVien = listUngVien.Where(a => !string.IsNullOrEmpty(a.IdNhanVien)).Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = new List<HRM_NhanVien>();
            if (listIdNhanVien != null && listIdNhanVien.Count > 0 && model.isKetThuc == true)
                listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
            foreach (var item in listUngVien)
            {
                item.lisPhuCap = new List<HRM_PheDuyetLuong_UngVien_PhuCap>();
                var ChucDanh = listChucDanh.Where(r => r.Id == item.IdChucDanhUngTuyen).FirstOrDefault();
                if (ChucDanh != null)
                    item.TenChucDanhUngTuyen = ChucDanh.Ten;
                var UngVien = listTuyenDungUngVien.Where(a => a.IdGuid == item.IdUngVien).FirstOrDefault();
                if (UngVien != null)
                {
                    item.MaUngVien = UngVien.MaUngVien;
                    item.HoTenDayDu = UngVien.HoTenDayDu;
                    item.DotTuyenDung = UngVien.DotTuyenDung;
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(item.IdNhanVien))
                {
                    var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    if (nhanVien != null)
                        item.HoTenNhanVienCu = nhanVien.TenDayDu;
                }
                if (item.NgayNhanViecDuKien != null)
                    item.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayNhanViecDuKien);
                if (item.KetThucThuViec != null)
                    item.KetThucThuViecUnix = dungchung.DateTimeToUnixTimeStamp(item.KetThucThuViec);

                if (listPhuCap != null && listPhuCap.Count > 0)
                {
                    item.lisPhuCap = listPhuCap.Where(a => a.IdUngVien == item.IdUngVien).OrderBy(a => a.Created).ToList();
                    if (item.lisPhuCap != null & item.lisPhuCap.Count > 0)
                    {

                        foreach (var pc in item.lisPhuCap)
                        {
                            var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                            if (Phucap != null)
                                pc.TenPhuCap = Phucap.Ten;
                        }
                    }
                }

            }
            model.listUngVien = listUngVien;
            return model;

        }
        public dynamic DeleteQuyTrinhPheDuyetLuongUngVien(string Id, ApplicationDbContext db)
        {
            try
            {
                HRM_TuyenDung_QuyTrinhPheDuyetLuong model = db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.Find(Id);

                var listUngVien = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(r => r.IdQuyTrinhPheDuyet == Id).ToList();

                db.HRM_TuyenDung_QuyTrinhPheDuyetLuong.Remove(model);
                db.HRM_TuyenDung_PheDuyetLuong_UngVien.RemoveRange(listUngVien);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp thành công!");
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }



        }

        public dynamic GetDanhSachTongHopDeXuatLuongDaDuyet(TimKiem itemTimKiem, ApplicationDbContext db)
        {

            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int nTabTrangThai = itemTimKiem.TabTrangThai ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 25;
            List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngVien = new List<HRM_TuyenDung_PheDuyetLuong_UngVien>();
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            List<long> listId = new List<long>();
            if (listCongTy != null)
            {
                listId = listCongTy.Select(a => a.Id).Distinct().ToList();

            }
            IQueryable<HRM_TuyenDung_PheDuyetLuong_UngVien> result = from a in db.HRM_TuyenDung_PheDuyetLuong_UngVien
                                                                      join b in db.HRM_TuyenDung_QuyTrinhPheDuyetLuong on a.IdQuyTrinhPheDuyet equals b.IdGuid
                                                                      where b.isKetThuc == true && ((itemTimKiem.IdCongTy == null && listId.Contains(b.IdCongTy)) || (itemTimKiem.IdCongTy == 0 && listId.Contains(b.IdCongTy)) || itemTimKiem.IdCongTy == b.IdCongTy)
                                                                      && (itemTimKiem.Nam == null || itemTimKiem.Nam == 0 || itemTimKiem.Nam == b.NgayDeNghi.Year)
                                                                      orderby a.Created descending
                                                                      select a;
            DateTime dtTuNgay = UnixTimeStampToDateTime(itemTimKiem.TuNgay ?? 0);
            DateTime dtDenNgay = UnixTimeStampToDateTime(itemTimKiem.DenNgay ?? 0);

            if (itemTimKiem.TuNgay > 0)
                result = result.Where(a => a.NgayNhanViecDuKien >= dtTuNgay);
            if (itemTimKiem.DenNgay > 0)
                result = result.Where(a => a.NgayNhanViecDuKien <= dtDenNgay);
            if (itemTimKiem.listIdBoPhan != null && itemTimKiem.listIdBoPhan.Count > 0)
                result = result.Where(a => itemTimKiem.listIdBoPhan.Contains(a.IdBoPhan));
            if (itemTimKiem.IdBoPhan > 0)
                result = result.Where(a => a.IdBoPhan == itemTimKiem.IdBoPhan);
            List<string> listIdUngVien = result.Select(a => a.IdUngVien).Distinct().ToList();
            var listUngViendb = db.HRM_TuyenDung_UngVien.Where(a => listIdUngVien.Contains(a.IdGuid)).Distinct().ToList();
            if (!string.IsNullOrEmpty(itemTimKiem.Keyword))
            {
                var listUngViencheck = listUngViendb.Where(a => a.HoTenDayDu.ToLower().Contains(itemTimKiem.Keyword.Trim().ToLower()) || a.MaUngVien.ToLower().Contains(itemTimKiem.Keyword.Trim().ToLower())).ToList();
                listIdUngVien = listUngViencheck.Select(a => a.IdGuid).Distinct().ToList();
                result = result.Where(a => listIdUngVien.Contains(a.IdUngVien) || a.MaNhanVien.ToLower().Contains(itemTimKiem.Keyword.Trim().ToLower()));

            }
            else
            {
                listIdUngVien = listUngViendb.Select(a => a.IdGuid).Distinct().ToList();
                result = result.Where(a => listIdUngVien.Contains(a.IdUngVien));
            }
            listUngVien = result.ToList();
            PagingInfor paging = new PagingInfor();
            if (nCurrentPage > 0)
            {
                int nSum = listUngVien.Count;

                int nTotalPage = CalculateTotalPages(nSum, PageSize);
                paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                listUngVien = listUngVien.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            }
            if (listUngVien != null && listUngVien.Count > 0)
            {
                List<string> listIdpd = listUngVien.Select(a => a.IdQuyTrinhPheDuyet).Distinct().ToList();
                var listPhuCap = db.HRM_PheDuyetLuong_UngVien_PhuCap.Where(r => listIdpd.Contains(r.IdQuyTrinhPheDuyetLuong) && r.isChon == true).ToList();
                var listdmPhuCap = db.HRM_dmPhuCap.ToList();
                var listdmCaLamViec = db.HRM_dmLichLamViec.ToList();
                var listMucLuong = db.HRM_dmCapBacLuong.ToList();
                var listLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in listUngVien)
                {
                    if (item.NgayNhanViecDuKien != null)
                        item.NgayNhanViecDuKienUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayNhanViecDuKien);
                    var ChucDanh = listChucDanh.Where(r => r.Id == item.IddmTrungTuyen).FirstOrDefault();
                    if (ChucDanh != null)
                        item.TenVitriTrungTuyen = ChucDanh.Ten;
                    var UngVien = listUngViendb.Where(a => a.IdGuid == item.IdUngVien).FirstOrDefault();
                    if (UngVien != null)
                    {
                        item.HoTenDayDu = UngVien.HoTenDayDu;
                        var CongTy = listCongTy.FirstOrDefault(a => a.Id == UngVien.IdCoSoTrungTuyen);
                        if (CongTy != null)
                            item.TenCoSo = CongTy.TenBoPhan;
                        var LoaiHopDong = listLoaiHopDong.FirstOrDefault(a => a.IdGuid == UngVien.IdLoaiHopDong);
                        if (LoaiHopDong != null)
                            item.LoaiHopDong = LoaiHopDong.Ten;
                        item.ThoiHanHopDong = UngVien.ThoiGianThuViec;
                        //var CapBacLuong = listMucLuong.FirstOrDefault(a => a.IdGuid == item.IdCapBacLuongChapNhan);
                        //if (CapBacLuong != null)
                        item.MucLuongThoaThuan = UngVien.Luong;
                    }
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhan);
                    if (BoPhan != null)
                        item.TenBoPhan = BoPhan.TenBoPhan;
                    var LichLamViec = listdmCaLamViec.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                    if (LichLamViec != null)
                        item.TenCaLamViec = LichLamViec.Ma;




                    item.lisPhuCap = listPhuCap.Where(a => a.IdUngVien == item.IdUngVien).ToList();
                    if (item.lisPhuCap != null && item.lisPhuCap.Count > 0)
                    {
                        foreach (var pc in item.lisPhuCap)
                        {
                            var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                            if (Phucap != null)
                                pc.TenPhuCap = Phucap.Ten;
                        }
                    }
                    else
                        item.lisPhuCap = new List<HRM_PheDuyetLuong_UngVien_PhuCap>();

                    item.TongThuNhap = (item.MucLuongThoaThuan ?? 0) + item.lisPhuCap.Sum(a => a.SoTien);
                    item.MucLuongThuViec = (item.MucLuongThoaThuan ?? 0) * item.PhanTramLuongThuViec / 100;


                }
            }
            if (nCurrentPage > 0)
            {
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = listUngVien;
                return dnm;
            }
            else
                return listUngVien;
        }

        public dynamic ExportDanhSachTongHopDeXuatLuongDaDuyet(TimKiem itemTimKiem, ApplicationDbContext db)

        {
            string sFile = "DanhSachPheDuyetLuongUngVien.xlsx";
            itemTimKiem.CurrentPage = 0;
            var data = GetDanhSachTongHopDeXuatLuongDaDuyet(itemTimKiem, db);
            string body = JsonConvert.SerializeObject(data);
            List<HRM_TuyenDung_PheDuyetLuong_UngVien> listUngVien = JsonConvert.DeserializeObject<List<HRM_TuyenDung_PheDuyetLuong_UngVien>>(body.ToString());
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "DanhSachPheDuyetLuongUngVien.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            int Row = 5;
            int nCol = 1;


            if (listUngVien != null && listUngVien.Count > 0)
            {

                int STT = 1;
                foreach (var item in listUngVien)
                {
                    int CounPhuCap = item.lisPhuCap.Count;
                    if (CounPhuCap > 0)
                        CounPhuCap--;

                    nCol = 1;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.TenCoSo;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.MaNhanVien;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.HoTenDayDu;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    if (!string.IsNullOrEmpty(item.IdNhanVien))
                        sheet1.Cells[Row, nCol].Value = item.HoTenDayDu;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.TenBoPhan;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.TenVitriTrungTuyen;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.TenCaLamViec;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    if (item.NgayNhanViecDuKien != null)
                        sheet1.Cells[Row, nCol].Value = item.NgayNhanViecDuKien.Value.ToString("dd/MM/yyyy");
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    if (item.KetThucThuViec != null)
                        sheet1.Cells[Row, nCol].Value = item.KetThucThuViec.Value.ToString("dd/MM/yyyy");
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.MucLuongThoaThuan;
                    nCol++;
                    int SttPc = 0;
                    foreach (var PC in item.lisPhuCap)
                    {
                        if (PC.SoTien > 0)
                            sheet1.Cells[Row + SttPc, nCol].Value = PC.TenPhuCap + ": " + string.Format("{0:0,0 VNĐ}", PC.SoTien).Replace(".", ",");
                        else
                            sheet1.Cells[Row + SttPc, nCol].Value = PC.TenPhuCap;
                        SttPc++;
                    }
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.TongThuNhap;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.PhanTramLuongThuViec;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.MucLuongThuViec;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.LoaiHopDong;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.ThoiHanHopDong;
                    nCol++;
                    sheet1.Cells[Row, nCol, Row + CounPhuCap, nCol].Merge = true;
                    sheet1.Cells[Row, nCol].Value = item.GhiChu;

                    if (item.lisPhuCap.Count == 0)
                        Row++;
                    else
                        Row = Row + item.lisPhuCap.Count;
                    STT++;
                }
                Row--;
                dungchung.DrawTableExcel(sheet1, 4, Row, 1, nCol);
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[5, 2, Row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 4, Row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 12, Row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 16, Row, 18].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 11, Row, 11].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                sheet1.Cells[5, 13, Row, 15].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }


            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }



        public dynamic ExportDanhSachNhanVienSoVan(TimKiem TimKiem, ApplicationDbContext db)

        {

            DateTime dt = DateTime.Now;
            var filenamegui = "DanhSachNhanVienChuaSoVan.xlsx";
            string NameSheet = "DANH SÁCH NHÂN VIÊN CHƯA SƠ VẤN";
            if (TimKiem.TrangThai == 2)
            {
                filenamegui = "DanhSachNhanVienDaSoVan.xlsx";
                NameSheet = "DANH SÁCH NHÂN VIÊN ĐÃ SƠ VẤN";
            }
            else if (TimKiem.TrangThai == 4)
            {
                filenamegui = "DanhSachNhanVienBiLoai.xlsx";
                NameSheet = "DANH SÁCH NHÂN VIÊN BỊ LOẠI";
            }
            else if (TimKiem.TrangThai == 5)
            {
                filenamegui = "DanhSachNhanVienChoPhongVan.xlsx";
                NameSheet = "DANH SÁCH NHÂN VIÊN CHỜ PHÒNG VẤN";
            }

            string sFile = "DanhSachNhanVienSoVan.xlsx";
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/" + sFile);
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();
            //var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Where(a => a.LoaiBoPhanPkid == 1).ToList();
            var listQuocTich = db.HRM_dmQuocTich.ToList();
            var listTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var listNguonTuyenDung = db.HRM_dmNguonTuyenDung.ToList();
            var listDanToc = db.HRM_dmDanToc.ToList();
            var listNoiCap = db.HRM_dmNoiCap.ToList();
            var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            var listCaLamViec = db.HRM_dmLichLamViec.ToList();
            var listTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
            int Row = 5;
            int nCol = 6;
            IQueryable<HRM_TuyenDung_UngVien> result;

            result = from a in db.HRM_TuyenDung_UngVien
                     where a.TrangThai == TimKiem.TrangThai
                     && a.isCongNhan == TimKiem.isCongNhan
                     orderby a.MaUngVien descending
                     select a;
            if (TimKiem.TuNgay > 0)
            {
                DateTime? Ngay = dungchung.UnixTimeStampToDateTime(TimKiem.TuNgay);
                result = result.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.ToString("dd/MM/yyyy") != "01/01/1970" && a.NgayTiepNhan >= Ngay);
            }
            if (TimKiem.DenNgay > 0)
            {
                DateTime? Ngay = dungchung.UnixTimeStampToDateTime(TimKiem.DenNgay);
                result = result.Where(a => a.NgayTiepNhan != null && a.NgayTiepNhan.Value.ToString("dd/MM/yyyy") != "01/01/1970" && a.NgayTiepNhan <= Ngay);
            }

            if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                result = result.Where(a => ((!string.IsNullOrEmpty(a.MaUngVien) && a.MaUngVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())))
                || ((!string.IsNullOrEmpty(a.HoTenDayDu) && a.HoTenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())))
                || ((!string.IsNullOrEmpty(a.DienThoaiDiDong) && a.DienThoaiDiDong.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()))));

            if (TimKiem.IdBoPhan > 0)
                result = result.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhan);
            if (TimKiem.IdCongTy > 0)
                result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
            if (TimKiem.IddmChucDanh > 0)
                result = result.Where(a => a.IddmChucDanhUngTuyen == TimKiem.IddmChucDanh);
            var itemList = result.ToList();
            if (itemList != null && itemList.Count > 0)
            {
                List<string> listIdUngVien = itemList.Select(a => a.IdGuid).Distinct().ToList();
                var listPCUngVien = db.HRM_UngVien_PhuCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).Distinct().ToList();
                var listBCUngVien = db.HRM_UngVienBangCap.Where(a => listIdUngVien.Contains(a.IdUngVien)).Distinct().ToList();
                int STT = 1;

                Row++;
                sheet1.Cells[1, 1].Value = NameSheet;
                foreach (var item in itemList)
                {
                    nCol = 6;
                    sheet1.Cells[Row, 1].Value = STT;
                    sheet1.Cells[Row, nCol].Value = item.HoTenDayDu;
                    nCol++;
                    if (item.NgaySinh != null && item.NgaySinh.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                        sheet1.Cells[Row, nCol].Value = item.NgaySinh.Value.ToString("dd/MM/yyyy");
                    nCol++;
                    if (item.GioiTinh == "Nam")
                        sheet1.Cells[Row, nCol].Value = "Nam";
                    nCol++;
                    if (item.GioiTinh == "Nu")
                        sheet1.Cells[Row, nCol].Value = "Nữ";
                    nCol++;
                    var TrinhDo = listTrinhDo.FirstOrDefault(a => a.IdGuid == item.IddmTrinhDoHocVan);
                    if (TrinhDo != null)
                        sheet1.Cells[Row, nCol].Value = TrinhDo.Ten;
                    nCol++;
                    // bằng cấp 
                    var listBC = listBCUngVien.Where(a => a.IdUngVien == item.IdGuid).ToList();
                    if (listBC != null && listBC.Count > 0)
                    {
                        var cellBC = sheet1.Cells[Row, nCol];
                        foreach (var BC in listBC)
                        {
                            TrinhDo = listTrinhDo.FirstOrDefault(a => a.IdGuid == item.IddmTrinhDoHocVan);
                            if (TrinhDo != null)
                            {
                                var r1 = cellBC.RichText.Add("- " + BC.NoiDaoTao + " - " + TrinhDo.Ten + "\r\n");
                            }
                        }
                        cellBC.Style.Font.Name = "Times New Roman";
                        cellBC.Style.Font.Size = 12;
                        cellBC.Style.WrapText = true;
                        sheet1.Row(Row).Height = (listBC.Count * 1.5 * 15.75);
                    }
                    else
                        nCol++;
                    sheet1.Cells[Row, nCol].Value = item.DienThoaiDiDong;
                    nCol++;
                    nCol++;
                    sheet1.Cells[Row, nCol].Value = item.HoKhauThuongTru;
                    nCol++;
                    sheet1.Cells[Row, nCol].Value = item.CCCD;
                    nCol++;
                    nCol++;
                    if (item.NgayCap != null && item.NgayCap.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                        sheet1.Cells[Row, nCol].Value = item.NgayCap.Value.ToString("dd/MM/yyyy");
                    nCol++;
                    if (!string.IsNullOrEmpty(item.NoiCap))
                        sheet1.Cells[Row, nCol].Value = listNoiCap.Where(a => a.IdGuid == item.NoiCap).Select(a => a.Ten).FirstOrDefault();
                    nCol++;
                    var NguonTuyenDung = listNguonTuyenDung.FirstOrDefault(a => a.IdGuid == item.IddmNguonTuyenDung);
                    if (NguonTuyenDung != null)
                        sheet1.Cells[Row, nCol].Value = NguonTuyenDung.Ten;
                    nCol++;
                    var CongTy = listCongTy.FirstOrDefault(a => a.Id == item.IdCoSoTrungTuyen);
                    if (CongTy != null)
                        sheet1.Cells[Row, nCol].Value = CongTy.TenBoPhan;
                    nCol++;
                    if (item.NgayNhanViecDuKien != null && item.NgayNhanViecDuKien.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                        sheet1.Cells[Row, nCol].Value = item.NgayNhanViecDuKien.Value.ToString("dd/MM/yyyy");
                    nCol++;
                    nCol++;
                    sheet1.Cells[Row, nCol].Value = item.Luong;
                    sheet1.Cells[Row, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                    nCol++;
                    nCol++;
                    nCol++;
                    sheet1.Cells[Row, nCol].Value = item.GhiChu;

                    STT++;
                    Row++;
                }
                Row--;
                dungchung.DrawTableExcel(sheet1, 4, Row, 1, nCol);
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[5, 6, Row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 10, Row, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells[5, 13, Row, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet1.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }


            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }


        #region Thư mời phỏng vấn
        public string GetMaTuyenDung(ApplicationDbContext db = null)
            => GenerateNextCodeWithPrefix(db.HRM_TuyenDung_QuyTrinh.Select(x => x.SoPhieu), $"{DefineData.MaTuyenDung}_{DateTime.Now.Year}_", 4, 5);
        public MessageReturn TaoThuMoiPhongVan(string aIdPheDuyetTuyenDung, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Tạo thư mời phỏng vấn thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aIdPheDuyetTuyenDung.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id phê duyệt tuyển dụng truyền nên");
            }

            var listTuyenDungPheDuyet = aContext.HRM_Link_TuyenDungQuyTrinh_UngVien.Where(x => x.IdTuyenDungPheDuyet == aIdPheDuyetTuyenDung && x.isPheDuyetGiamDoc == true).ToList();
            if (listTuyenDungPheDuyet.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
            }

            var listCreate = new List<HRM_TuyenDung_ThuMoiLamViec>();
            foreach (var item in listTuyenDungPheDuyet)
            {
                var model = new HRM_TuyenDung_ThuMoiLamViec();
                model.IdGuid = Guid.NewGuid().ToString();
                model.IdTuyenDungUngVien = item.IdTuyenDungUngVien;
                model.IdLinkTuyenDungQuyTrinhUngVien = item.IdGuid;
                model.NgayMoi = DateTime.Now;
                model.NgayBatDau = UnixTimeStampToDateTime(DefineData.DateMinUnix);
                model.NgayXacNhan = UnixTimeStampToDateTime(DefineData.DateMinUnix);

                listCreate.Add(model);
            }

            aContext.HRM_TuyenDung_ThuMoiLamViec.AddRange(listCreate);
            aContext.SaveChanges();

            return result;
        }

        public EntityResponse<ThuMoiLamViec> GetDanhSachThuMoiLamViec(EntityRequest aRequest, ApplicationDbContext aContext)
        {
            var result = new EntityResponse<ThuMoiLamViec>();

            //var quyenView = GetQuyenView(DefineData.eAction.THUMOILAMVIEC.ToString(), aContext.currentUser.Id, aSMart);
            //if (quyenView == (int)DefineData.eGioiHan.NONE)
            //{
            //    return result;
            //}

            var listModel = aContext.HRM_TuyenDung_ThuMoiLamViec.ToList();
            if (listModel.IsEmpty())
            {
                return result;
            }

            var listUngVien = aContext.HRM_TuyenDung_UngVien.ToList();

            var listResult = new List<ThuMoiLamViec>();
            foreach (var item in listModel)
            {
                var model = new ThuMoiLamViec();
                model.Created = item.Created;
                model.CreatedBy = item.CreatedBy;
                model.CreatedByName = item.CreatedByName;
                if (listUngVien.IsNotEmpty())
                {
                    var ungVien = listUngVien.FirstOrDefault(x => x.IdGuid == item.IdTuyenDungUngVien);
                    if (ungVien != null)
                    {
                        model.DienThoaiDiDong = ungVien.DienThoaiDiDong;
                        model.HoTen = ungVien.HoTenDayDu;
                        model.MaUngVien = ungVien.MaUngVien;
                        model.IddmTrinhDoHocVan = ungVien.IddmTrinhDoHocVan;
                        model.NgaySinh = ungVien.NgaySinh;
                        model.ViTriChuyenMon = ungVien.ViTriChuyenMonUngTuyen;
                    }
                }
                model.IdGuid = item.IdGuid;
                model.Modified = item.Modified;
                model.ModifiedBy = item.ModifiedBy;
                model.ModifiedByName = item.ModifiedByName;
                model.TrangThaiXacNhan = item.TrangThaiXacNhan;

                listResult.Add(model);
            }

            if (aRequest.KeyWord.IsNotEmpty())
            {
                listResult = listResult.Where(x => x.HoTen.ToLower().Contains(aRequest.KeyWord.ToLower()) ||
                                                   x.DienThoaiDiDong.ToLower().Contains(aRequest.KeyWord.ToLower()) ||
                                                   x.MaUngVien.ToLower().Contains(aRequest.KeyWord.ToLower()) ||
                                                   x.ViTriChuyenMon.ToLower().Contains(aRequest.KeyWord.ToLower())).ToList();
            }

            if (listResult.IsEmpty())
            {
                return result;
            }

            int count = listResult.Count();
            listResult = listResult.OrderByDescending(q => q.Created).Skip(aRequest.NumperPage * (aRequest.CurrentPage - 1)).Take(aRequest.NumperPage).ToList();

            result.paging = new PagingInfor(aRequest.CurrentPage, count, (int)count / aRequest.NumperPage + (count % aRequest.NumperPage > 0 ? 1 : 0));
            result.items = listResult;

            return result;
        }

        public MessageReturn UpdateThuMoiLamViec(HRM_TuyenDung_ThuMoiLamViec aModel, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới thư mời phỏng vấn thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.NgayBatDauUnix > 0)
            {
                aModel.NgayBatDau = UnixTimeStampToDateTime(aModel.NgayBatDauUnix);
            }
            if (aModel.NgayMoiUnix > 0)
            {
                aModel.NgayMoi = UnixTimeStampToDateTime(aModel.NgayMoiUnix);
            }
            if (aModel.NgayXacNhanUnix > 0)
            {
                aModel.NgayXacNhan = UnixTimeStampToDateTime(aModel.NgayXacNhanUnix);
            }
            if (aModel.IdGuid.IsEmpty())
            {
                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_TuyenDung_ThuMoiLamViec.Add(aModel);
            }
            else
            {
                aContext.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu tuyển dụng thư mời làm việc thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        #endregion
    }
    public class LichPhongVanChuTriViewModel
    {
        public string Id { set; get; }
        public string IdTuyenDungPhongVan { set; get; }
        public string DiaDiem { set; get; }
        public DateTime NgayPhongVan { set; get; }
        public double NgayPhongVanUnix { set; get; }
        public string HoTenUngVien { set; get; }
        public string MaUngVien { set; get; }
        public int VongPhongVan { set; get; }
        public long IdChucDanhUngTuyen { set; get; }
        public string TenChucDanhUngTuyen { set; get; }
        public int isThamGia { get; set; }
        public string TrangThaiThamGia { set; get; }
    }
}
