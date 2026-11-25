#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using HRM.Api.Services.NhanVien;
using HRM.Api.Entity;
using HRM.Api.Entity.NhanVien;
using HRM.Api.Helper;
using HRM.Api.Models;
using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xceed.Document.NET;
using static HRM.Api.Services.FuncBase;
using static HRM.Api.Helper.HelperConstant;

namespace HRM.Api.Services
{
    public class FuncNhanVien : FuncBase
    {
        private readonly FuncNghiViec _funcNghiViec;
        private readonly FuncDieuChinhLuong _funcDieuChinhLuong;
        private readonly FuncKhenThuong _funcKhenThuong;
        private readonly FuncKyLuat _funcKyLuat;

        public FuncNhanVien(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncNhanVien> logger,
            FuncNghiViec funcNghiViec,
            FuncDieuChinhLuong funcDieuChinhLuong,
            FuncKhenThuong funcKhenThuong,
            FuncKyLuat funcKyLuat)
            : base(callWebService, workflowEngine, options, logger)
        {
            _funcNghiViec = funcNghiViec;
            _funcDieuChinhLuong = funcDieuChinhLuong;
            _funcKhenThuong = funcKhenThuong;
            _funcKyLuat = funcKyLuat;
        }

        #region Thông tin chung nhân viên
        public dynamic GetCongTyBoPhanByIdNhanVien(ApplicationDbContext db)
        {
            var result = (from a in db.HRM_NhanVien
                          where a.IdUser == db.currentUser.Id
                          select new
                          {
                              IdCongTyEOS = a.IdCongTyEOS,
                              IdBoPhanEOS = a.IdBoPhanEOS
                          }).FirstOrDefault();
            return result;
        }

        public List<BoPhan> GetCongTyByIdNhanVien(ApplicationDbContext db)
        {
            //var listBoPhan = _CallWebService.GetDanhSachCongTy(db);

            //long result = 0;
            //var nhanVien = db.HRM_NhanVien.Where(a => !string.IsNullOrEmpty(a.IdUser) && a.IdUser == db.currentUser.Id).FirstOrDefault();
            //if (nhanVien != null)
            //    result = nhanVien.IdCongTyEOS;
            //if (result != 8)
            var listBoPhan = _CallWebService.GetDanhSachCongTy(db);
            return listBoPhan;
        }
        public List<BoPhan> GetDanhSachDuAnDaDuyet(ApplicationDbContext db)
        {
            //var listBoPhan = _CallWebService.GetDanhSachCongTy(db);

            //long result = 0;
            //var nhanVien = db.HRM_NhanVien.Where(a => !string.IsNullOrEmpty(a.IdUser) && a.IdUser == db.currentUser.Id).FirstOrDefault();
            //if (nhanVien != null)
            //    result = nhanVien.IdCongTyEOS;
            //if (result != 8)
            var listBoPhan = _CallWebService.GetDanhSachDuAnDaDuyet(db);
            return listBoPhan;
        }
        public List<BoPhan> GetlistBoPhanByIdNhanVien(ApplicationDbContext db)
        {
            List<BoPhan> listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Distinct().OrderBy(a => a.MaBoPhan).ToList();

            return listBoPhan;
        }
        public string GetlistBoPhanByIdUser(ApplicationDbContext db, string IdUser)
        {
            var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == IdUser);
            List<BoPhan> listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Distinct().OrderBy(a => a.MaBoPhan).ToList();
            string BoPhan = listBoPhan.Where(a => a.Id == NhanVien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
            return BoPhan;
        }

        public dynamic GetDanhSachNguoiKyByIdCongTy(ApplicationDbContext db, long IdCongTy, long IdBoPhan)
        {
            //var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            List<clsNhanVien> listResult = new List<clsNhanVien>();
            //if (listNhanSuDuAn != null)
            //{
            var BoPhan = _CallWebService.GetDanhSachBoPhan(db).FirstOrDefault(a => a.MaBoPhan == "BLD");
            //List<string> listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
            var listNhanVien = (from a in db.HRM_NhanVien
                                where /*listIdUser.Contains(a.IdUser) &&*/ a.IdBoPhanEOS == BoPhan.Id
                                orderby a.MaNhanVien
                                select a).ToList();
            if (listNhanVien != null && listNhanVien.Count > 0)
            {
                foreach (var item in listNhanVien)
                {
                    clsNhanVien nVien = new clsNhanVien();

                    nVien.IdNhanVien = item.IdGuid;
                    nVien.IdUser = item.IdGuid;
                    nVien.TenNhanVien = item.TenDayDu;
                    nVien.TenDayDu = item.TenDayDu;
                    nVien.MaNhanVien = item.MaNhanVien;
                    nVien.IddmChucDanh = item.IdChucDanh;

                    listResult.Add(nVien);
                }
            }
            //}
            return listResult;
        }
        public dynamic UpdateIdUser(ApplicationDbContext db)
        {
            var listBoPhan = _CallWebService.GetDanhSachCongTy(db);
            List<clsNhanVien> DanhSachNhanVien = _CallWebService.GetDanhSachNhanVien(db);
            var userByEosId = DanhSachNhanVien?
                .GroupBy(a => a.Id)
                .ToDictionary(g => g.Key, g => g.First().UserPkid)
                ?? new Dictionary<long, string>();
            var result = db.HRM_NhanVien.ToList();
            foreach (var item in result)
            {
                if (userByEosId.TryGetValue(item.IdNhanVienEOS, out var userPkid))
                {
                    item.IdUser = userPkid;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            db.SaveChanges();
            return listBoPhan;
        }

        public List<GetNhanVienChuaCoHopDongModel> GetNhanVienTheoPhongBan(ApplicationDbContext _dbContext, long aIdPhongBan)
        {
            var result = new List<GetNhanVienChuaCoHopDongModel>();

            var listModel = _dbContext.HRM_NhanVien.Where(x => x.IdBoPhanEOS == aIdPhongBan).ToList();
            if (listModel.IsEmpty())
            {
                return result;
            }

            var toDictQuocTich = _dbContext.HRM_dmQuocTich.ToList().ToDict(x => x.IdGuid);
            foreach (var item in listModel)
            {
                var model = new GetNhanVienChuaCoHopDongModel();
                model.IdGuid = item.IdGuid;
                model.TenNhanVien = item.TenDayDu;
                model.NgaySinh = item.NgaySinh;
                model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);
                model.QuocTich = item.IdQuocTich.GetValueFromDic(toDictQuocTich, x => x.Ten);
                model.NguyenQuan = item.NguyenQuan;
                model.DiaChi = item.DienThoaiNhaRieng;
                model.SoDienThoai = item.DienThoai;
                model.Email = item.Email;
                model.TrinhDoHocVan = item.TrinhDoHocVan;
                model.NgayCapUnix = DateTimeToUnixTimeStamp(model.NgayCap);


                result.Add(model);
            }

            return result;
        }

        public List<GetNhanVienChuaCoHopDongModel> GetDanhSachNhanVienChuaCoHopDong(ApplicationDbContext _dbContext)
        {
            var result = new List<GetNhanVienChuaCoHopDongModel>();

            if (_dbContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            var listNhanVien = _dbContext.HRM_NhanVien.ToList();
            if (listNhanVien.IsEmpty())
            {
                return result;
            }

            var toDictHopDong = _dbContext.HRM_HopDong.ToList().ToDict(x => x.IdNhanVien);
            var toDictQuocTich = _dbContext.HRM_dmQuocTich.ToList().ToDict(x => x.IdGuid);
            foreach (var item in listNhanVien)
            {
                var model = new GetNhanVienChuaCoHopDongModel();
                model.IdGuid = item.IdGuid;
                model.TenNhanVien = item.TenDayDu;
                model.NgaySinh = item.NgaySinh;
                model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);
                model.QuocTich = item.IdQuocTich.GetValueFromDic(toDictQuocTich, x => x.Ten);
                model.NguyenQuan = item.NguyenQuan;
                model.DiaChi = item.DienThoaiNhaRieng;
                model.SoDienThoai = item.DienThoai;
                model.Email = item.Email;
                model.TrinhDoHocVan = item.TrinhDoHocVan;
                model.NgayCapUnix = DateTimeToUnixTimeStamp(model.NgayCap);

                //result.Add(model);

                if (toDictHopDong.ContainsKey(item.IdGuid))
                {
                    if (toDictHopDong[item.IdGuid].TrangThai == DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString())
                    {
                        result.Add(model);
                    }
                }
                else
                {
                    result.Add(model);
                }
            }

            return result;
        }

        public List<GetNhanVienChuaCoHopDongModel> GetDanhSachNhanVienDaCoHopDong(ApplicationDbContext _dbContext)
        {
            var result = new List<GetNhanVienChuaCoHopDongModel>();

            if (_dbContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            var listNhanVien = _dbContext.HRM_NhanVien.ToList();
            if (listNhanVien.IsEmpty())
            {
                return result;
            }

            var toDictHopDong = _dbContext.HRM_HopDong.ToList().ToDict(x => x.IdNhanVien);
            var toDictQuocTich = _dbContext.HRM_dmQuocTich.ToList().ToDict(x => x.IdGuid);
            foreach (var item in listNhanVien)
            {
                var model = new GetNhanVienChuaCoHopDongModel();
                model.IdGuid = item.IdGuid;
                model.TenNhanVien = item.TenDayDu;
                model.NgaySinh = item.NgaySinh;
                model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);
                model.QuocTich = item.IdQuocTich.GetValueFromDic(toDictQuocTich, x => x.Ten);
                model.NguyenQuan = item.NguyenQuan;
                model.DiaChi = item.DienThoaiNhaRieng;
                model.SoDienThoai = item.DienThoai;
                model.Email = item.Email;
                model.TrinhDoHocVan = item.TrinhDoHocVan;
                model.NgayCapUnix = DateTimeToUnixTimeStamp(model.NgayCap);

                //result.Add(model);

                if (toDictHopDong.ContainsKey(item.IdGuid))
                {
                    if (toDictHopDong[item.IdGuid].TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString())
                    {
                        result.Add(model);
                    }
                }
            }

            return result;
        }

        public dynamic GetDanhSachNhanVien(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                DateTime DateNow1970 = dungchung.UnixTimeStampToDateTime(0);

                //var lstCongTyAll = _CallWebService.GetDanhSachDuAn(_dbContext);
                var lstCongTyAll = _CallWebService.GetDanhSachDuAnQuanLyHoSoNhanVien(_dbContext);
                List<long> listIdDuAn = lstCongTyAll.Select(x => x.Id).ToList();
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                select new DanhSachNhanVienViewModel
                                                                {
                                                                    IdUser = a.IdUser,
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
                                                                    SoBHXH = a.SoBaoHiemXaHoi,
                                                                    //SoChungMinhThu = a.SoChungMinhThu,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh,
                                                                    SoGen = a.SoGen
                                                                }
                                                                ).OrderBy(a => a.MaNhanVien).ToList();

                // Nếu có biến ListIdCongTyEOS thì sẽ lọc theo đống này - Duyệt mới thêm
                if (aRequest.ListIdCongTyEOS != null && aRequest.ListIdCongTyEOS.Count > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.ListIdCongTyEOS.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (aRequest.listIdBoPhan != null && aRequest.listIdBoPhan.Count > 0)
                    listNhanVien = listNhanVien.Where(x => aRequest.listIdBoPhan.Contains(x.IdBoPhanEOS)).ToList();

                if (aRequest.IdCongTyEOS > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.IdCongTyEOS == x.IdBoPhan).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                else
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdDuAn.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                    listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                if (aRequest.Thang != 0)
                    listNhanVien = listNhanVien.Where(a => a.NgaySinh.Month == aRequest.Thang).ToList();
                if (aRequest.Ngay != 0)
                    listNhanVien = listNhanVien.Where(a => a.NgaySinh.Day == aRequest.Ngay).ToList();
                List<string> listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                var listSDTs = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                foreach (var model in listNhanVien)
                    model.DienThoai = listSDTs.Where(a => a.isApDung == true && a.IdNhanVien == model.IdGuid).Select(a => a.SoDienThoai).FirstOrDefault();

                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
                {
                    string KeyWord = aRequest.KeyWord.ToLower().Trim();
                    listNhanVien = listNhanVien.Where(a => (!string.IsNullOrEmpty(a.TenDayDu) && a.TenDayDu.ToLower().Trim().Contains(KeyWord))
                                                            || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(KeyWord))
                                                            || (!string.IsNullOrEmpty(a.DienThoai) && a.DienThoai.ToLower().Contains(KeyWord))
                                                            || (!string.IsNullOrEmpty(a.SoGen) && a.SoGen.ToLower().Contains(KeyWord))
                                                            ).ToList();
                }
                if (!string.IsNullOrEmpty(aRequest.TrangThai))
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == aRequest.TrangThai).ToList();
                if (!string.IsNullOrEmpty(aRequest.GioiTinh))
                    listNhanVien = listNhanVien.Where(a => a.GioiTinh == aRequest.GioiTinh).ToList();

                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    int count = listNhanVien.Count();

                    if (aRequest.CurrentPage == 0 && aRequest.TrangThai == "DaNghiViec")
                        return listNhanVien;
                    var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                    int CurrentPage = aRequest.CurrentPage;
                    if (CurrentPage == 0 && string.IsNullOrEmpty(aRequest.TrangThai))
                    {
                        listNhanVien = listNhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();
                        var listGTTH = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                        var listemails = _dbContext.HRM_NhanVien_Email.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();

                        // Start Duyệt mới thêm
                        var listIdQuyTrinh_NhanVien_1 = listNhanVien.Select(x => x.IdGuid).ToList();
                        var listQuyTrinh_BoPhan_1 = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdQuyTrinh)).ToList();
                        // End Duyệt mới thêm
                        foreach (var model in listNhanVien)
                        {

                            // Start Duyệt mới thêm
                            var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                            var _listBoPhan = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                            model.TenCongTy = string.Join("; ", _listBoPhan);
                            // End Duyệt mới thêm


                            model.Email = listemails.Where(a => a.isApDung == true && a.IdNhanVien == model.IdGuid).Select(a => a.Email).FirstOrDefault();
                            model.DienThoai = listSDTs.Where(a => a.isApDung == true && a.IdNhanVien == model.IdGuid).Select(a => a.SoDienThoai).FirstOrDefault();
                            model.CCCD = listGTTH.Where(a => a.isApDung == true && a.IdNhanVien == model.IdGuid).Select(a => a.So).FirstOrDefault();
                            //model.TenCongTy = lstCongTyAll.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
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

                            if (model.GioiTinh == "KhongXacDinh")
                                model.GioiTinh = "Khác";
                        }
                        return listNhanVien;
                    }
                    List<DanhSachNhanVienViewModel> listNhanViens = listNhanVien.OrderBy(r => r.MaNhanVien).ThenByDescending(r => r.NgayBatDau).Skip(aRequest.NumperPage * (aRequest.CurrentPage - 1)).Take(aRequest.NumperPage).ToList();
                    var listIdNV = listNhanViens.Select(a => a.IdGuid).ToList();
                    var listSDT = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => a.isApDung == true && listIdNV.Contains(a.IdNhanVien)).ToList();
                    var QHGD_Con = _dbContext.HRM_dmQuanHeGiaDinh.FirstOrDefault(a => a.Ma.ToLower() == "con");
                    DateTime time = DateTime.Now;
                    time = time.AddYears(-18);
                    var lisGiaDinh = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(a => a.isLaNguoiPhuThuoc == true && a.NgaySinh != null && a.NgaySinh <= time && listIdNV.Contains(a.IdNhanVien)).ToList();

                    // Start Duyệt mới thêm
                    var listIdQuyTrinh_NhanVien = listNhanViens.Select(x => x.IdGuid).ToList();
                    var listQuyTrinh_BoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien.Contains(a.IdQuyTrinh)).ToList();
                    // End Duyệt mới thêm

                    foreach (var model in listNhanViens)
                    {

                        // Start Duyệt mới thêm
                        var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                        var _listBoPhan = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                        model.TenCongTy = string.Join("; ", _listBoPhan);
                        // End Duyệt mới thêm


                        //model.TenCongTy = lstCongTyAll.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.DienThoai = listSDT.Where(q => q.IdNhanVien == model.IdGuid).Select(q => q.SoDienThoai).FirstOrDefault();
                        model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                        model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                        model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                        var Check = lisGiaDinh.Where(a => a.IdNhanVien == model.IdGuid).ToList();
                        if (Check != null && Check.Count > 0)
                        {
                            model.CanhBaoQuaTuoi = "Quá tuổi phụ thuôc: " + string.Join(", ", Check.Select(a => a.HoTen).ToList());
                        }
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
                    int PageSize = aRequest.NumperPage;
                    result.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
                    result.items = listNhanViens;
                    //result.value = lstBoPhanAll;
                    return result;
                }
                return listNhanVien;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }
        public dynamic GetDanhSachNhanVienAll(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                DateTime DateNow1970 = dungchung.UnixTimeStampToDateTime(0);
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien

                                                                select new DanhSachNhanVienViewModel
                                                                {
                                                                    IdUser = a.IdUser,
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
                                                                    SoBHXH = a.SoBaoHiemXaHoi,
                                                                    //SoChungMinhThu = a.SoChungMinhThu,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh,
                                                                    SoGen = a.SoGen
                                                                }

                                                                ).OrderBy(a => a.MaNhanVien).ToList();

                // Nếu có biến ListIdCongTyEOS thì sẽ lọc theo đống này - Duyệt mới thêm
                if (aRequest.ListIdCongTyEOS != null && aRequest.ListIdCongTyEOS.Count > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.ListIdCongTyEOS.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (aRequest.listIdBoPhan != null && aRequest.listIdBoPhan.Count > 0)
                {
                    listNhanVien = listNhanVien.Where(x => aRequest.listIdBoPhan.Contains(x.IdBoPhanEOS)).ToList();
                }

                if (aRequest.IdCongTyEOS > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.IdCongTyEOS == x.IdBoPhan).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                }

                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
                {
                    listNhanVien = listNhanVien.Where(a => (!string.IsNullOrEmpty(a.TenDayDu) && a.TenDayDu.ToLower().Contains(aRequest.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.DienThoai) && a.DienThoai.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.SoGen) && a.SoGen.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())))).ToList();
                }
                if (!string.IsNullOrEmpty(aRequest.TrangThai))
                {
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == aRequest.TrangThai).ToList();
                }
                else
                {
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() || a.TrangThai == DefineData.eTrangThaiNhanVien.NghiThaiSan.ToString() || a.TrangThai == DefineData.eTrangThaiNhanVien.NghiKhongLuong.ToString()).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    int count = listNhanVien.Count();
                    var lstCongTyAll = _CallWebService.GetDanhSachDuAn(_dbContext);
                    var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                    int CurrentPage = aRequest.CurrentPage;
                    if (CurrentPage == 0)
                    {
                        //listNhanVien = listNhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();

                        var listIdQuyTrinh_NhanVien_1 = listNhanVien.Select(x => x.IdGuid).ToList();
                        var listGTTH = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdNhanVien)).ToList();

                        var listQuyTrinh_BoPhan_1 = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdQuyTrinh)).ToList();
                        var listluong = _dbContext.HRM_Luong_NhanVien.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdNhanVien)).ToList();
                        // End Duyệt mới thêm
                        foreach (var model in listNhanVien)
                        {
                            model.CCCD = listGTTH.Where(a => a.isApDung == true && a.IdNhanVien == model.IdGuid).Select(a => a.So).FirstOrDefault();
                            // Start Duyệt mới thêm
                            var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                            var _listBoPhan = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                            var Luong = listluong.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                            if (Luong != null)
                            {
                                model.Luong = Luong.Luong;
                                model.LuongBaoHiem = Luong.LuongHachToan;
                            }
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
                            //listNhanVien = listNhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() || a.TrangThai == DefineData.eTrangThaiNhanVien.NghiThaiSan.ToString() || a.TrangThai == DefineData.eTrangThaiNhanVien.NghiKhongLuong.ToString()).ToList();
                            if (model.TrangThai == DefineData.eTrangThaiNhanVien.NghiThaiSan.ToString())
                                model.Loai = DefineData.eLoaiBaoHiem.TangLaiSauThaiSan.ToString();
                            else if (model.TrangThai == DefineData.eTrangThaiNhanVien.NghiKhongLuong.ToString())
                                model.Loai = DefineData.eLoaiBaoHiem.TangSauNghiKhongLuong.ToString();

                            if (model.GioiTinh == "KhongXacDinh")
                                model.GioiTinh = "Khác";
                        }
                        return listNhanVien.OrderBy(a => a.NgayBatDauLamViec);
                    }

                }
                return listNhanVien.OrderBy(a => a.NgayBatDauLamViec);
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }

        public dynamic GetDanhSachNhanVienNhanSuDuAn(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                dynamic result = new DynamicObject();
                DateTime DateNow1970 = dungchung.UnixTimeStampToDateTime(0);
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien

                                                                select new DanhSachNhanVienViewModel
                                                                {
                                                                    IdUser = a.IdUser,
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
                                                                    SoBHXH = a.SoBaoHiemXaHoi,
                                                                    //SoChungMinhThu = a.SoChungMinhThu,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh,
                                                                    SoGen = a.SoGen
                                                                }

                                                                ).OrderBy(a => a.MaNhanVien).ToList();
                listNhanVien = listNhanVien.Where(x => x.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();

                // Nếu có biến ListIdCongTyEOS thì sẽ lọc theo đống này - Duyệt mới thêm
                if (aRequest.ListIdCongTyEOS != null && aRequest.ListIdCongTyEOS.Count > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.ListIdCongTyEOS.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (aRequest.listIdBoPhan != null && aRequest.listIdBoPhan.Count > 0)
                {
                    listNhanVien = listNhanVien.Where(x => aRequest.listIdBoPhan.Contains(x.IdBoPhanEOS)).ToList();
                }

                if (aRequest.IdCongTyEOS > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.IdCongTyEOS == x.IdBoPhan).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                }

                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
                {
                    listNhanVien = listNhanVien.Where(a => (a.TenDayDu.ToLower().Contains(aRequest.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.DienThoai) && a.DienThoai.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.SoGen) && a.SoGen.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())))).ToList();
                }
                if (!string.IsNullOrEmpty(aRequest.TrangThai))
                {
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == aRequest.TrangThai).ToList();
                }
                else
                {
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() || a.TrangThai == DefineData.eTrangThaiNhanVien.NghiThaiSan.ToString() || a.TrangThai == DefineData.eTrangThaiNhanVien.NghiKhongLuong.ToString()).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    int CurrentPage = aRequest.CurrentPage;
                    if (CurrentPage == 0)
                    {
                        return listNhanVien;
                    }

                }
                return listNhanVien;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }

        public int WeekOfYear(DateTime date)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            int weekNumber = culture.Calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek, // Quy tắc: Tuần đầu tiên có ít nhất 4 ngày
                DayOfWeek.Monday // Tuần bắt đầu từ thứ Hai
            );
            return weekNumber;
        }
        public dynamic GetDanhSachNhanVienThongBao(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                DanhSachNhanVienThongBao result = new DanhSachNhanVienThongBao();
                DateTime DateNow1970 = dungchung.UnixTimeStampToDateTime(0);
                DateTime TuNgay = UnixTimeStampToDateTime(aRequest.TuNgayUnix);
                DateTime DenNgay = UnixTimeStampToDateTime(aRequest.DenNgayUnix);
                DateTime timeNow = DateTime.Today;
                DateTime timeEnd = timeNow.AddDays(1);
                DateTime timeCheck = timeEnd.AddDays(2);
                double time = dungchung.DateTimeToUnixTimeStamp(timeEnd);
                var listGiayToHethan = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => a.NgayHetHan != null && ((aRequest.TuNgayUnix == 0 && aRequest.DenNgayUnix == 0 && a.NgayHetHan <= timeCheck)
                //|| (aRequest.TuNgayUnix > 0 && WeekOfYear(TuNgay)<= WeekOfYear(a.NgayHetHan.Value)) 
                //|| (aRequest.DenNgayUnix > 0 && WeekOfYear( DenNgay) >= WeekOfYear(a.NgayHetHan.Value))
                )
                && a.isApDung == true).ToList();
                var listIdNv = new List<string>();
                if (listGiayToHethan != null && listGiayToHethan.Count > 0)
                    listIdNv = listGiayToHethan.Select(a => a.IdNhanVien).Distinct().ToList();
                var listKhamSucKhoeHethan = (from a in _dbContext.HRM_NhanVien_KhamSucKhoe
                                             where a.NgayKetThuc != null
                                             group a by a.IdNhanVien into g
                                             select g.OrderByDescending(c => c.NgayKham).FirstOrDefault()
                                          ).Where(a => ((aRequest.TuNgayUnix == 0 && aRequest.DenNgayUnix == 0 && a.NgayKetThuc <= timeCheck)
                                          //|| (aRequest.TuNgayUnix > 0 && WeekOfYear(TuNgay) <= WeekOfYear(a.NgayKetThuc.Value)) 
                                          //|| (aRequest.DenNgayUnix > 0 &&  WeekOfYear(DenNgay) >= WeekOfYear(a.NgayKetThuc.Value))
                                          )
                                          ).ToList();
                if (listKhamSucKhoeHethan != null && listKhamSucKhoeHethan.Count > 0)
                    listIdNv.AddRange(listKhamSucKhoeHethan.Select(a => a.IdNhanVien).Distinct().ToList());
                var LoaiHopDongThuViec = _dbContext.HRM_dmLoaiHopDong.FirstOrDefault(a => a.Ma == "HĐTV");
                var listHopDongHethan = (from a in _dbContext.HRM_HopDong
                                         where a.isKetThuc == true && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()
                                         group a by a.IdNhanVien into g
                                         select g.OrderByDescending(a => a.NgayHieuLuc).FirstOrDefault()).ToList();
                if (listHopDongHethan != null && listHopDongHethan.Count > 0)
                    listIdNv.AddRange(listHopDongHethan.Select(a => a.IdNhanVien).Distinct().ToList());
                DateTime timeT = timeCheck.AddYears(-18);
                var lisGiaDinh = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(a => a.isLaNguoiPhuThuoc == true && a.NgaySinh != null &&
                ((aRequest.TuNgayUnix == 0 && aRequest.DenNgayUnix == 0 && a.NgaySinh <= timeT)
                //|| (aRequest.TuNgayUnix > 0 && WeekOfYear(TuNgay) <= WeekOfYear(a.NgaySinh.Value))
                //|| (aRequest.DenNgayUnix > 0 && WeekOfYear(DenNgay) >= WeekOfYear(a.NgaySinh.Value))
                )).ToList();
                if (lisGiaDinh != null && lisGiaDinh.Count > 0)
                    listIdNv.AddRange(lisGiaDinh.Select(a => a.IdNhanVien).Distinct().ToList());
                // đủ năm BaoHiemXaHoi
                DateTime timeDong = timeNow.AddYears(-(15));
                var listBHXH = _dbContext.HRM_BaoHiem_HopDong.Where(a => a.isBaoHiemXaHoi == true).ToList();
                var listIdNvBHXH = listBHXH.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNvBHXH = (from a in _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi
                                      //where a.isThamGia == false
                                  select a
                                  /*&& listIdNvBHXH.Contains(a.IdNhanVien)*/
                                  //group a by new { a.IdNhanVien } into g
                                  //select new
                                  //{
                                  //    IdNhanVien = g.Key.IdNhanVien,
                                  //    ThoiGian = g.Sum(b => (b.DenNgay - b.TuNgay).TotalDays)
                                  //}
                      ).ToList();
                if (listNvBHXH != null)
                    listIdNvBHXH = listNvBHXH.Where(a => a.isThamGia == true).Select(a => a.IdNhanVien).Distinct().ToList();


                listIdNv = listIdNv.Distinct().ToList();
                DateTime timeNam = timeNow.AddMonths(-(61 * 12 + 3));
                DateTime timeNu = timeNow.AddMonths(-(56 * 12 + 8));

                List<DanhSachNhanVienViewModel> listNhanVien = (from a in _dbContext.HRM_NhanVien
                                                                where a.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString() &&
                                                                (((a.NgayBatDauLamViec != null && ((aRequest.TuNgayUnix == 0 && aRequest.DenNgayUnix == 0 && a.NgayBatDauLamViec.Value.Month == timeNow.Month && a.NgayBatDauLamViec.Value.Day == timeNow.Day && a.NgayBatDauLamViec.Value.Year != timeNow.Year)))
                                                                || (a.NgaySinh != null && ((aRequest.TuNgayUnix == 0 && aRequest.DenNgayUnix == 0 && a.NgaySinh.Month == timeNow.Month && a.NgaySinh.Day == timeNow.Day && a.NgaySinh.Year == timeNow.Year))))
                                                               || listIdNv.Contains(a.IdGuid)
                                                              || a.isThamGiaBHXH == true
                                                               || (a.NgaySinh != null && a.IdGioiTinh != null && ((a.IdGioiTinh == "Nam" && a.NgaySinh <= timeNam) || a.NgaySinh <= timeNu)))
                                                                select new DanhSachNhanVienViewModel
                                                                {
                                                                    IdUser = a.IdUser,
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
                                                                    SoBHXH = a.SoBaoHiemXaHoi,
                                                                    //SoChungMinhThu = a.SoChungMinhThu,
                                                                    NguyenQuan = a.NguyenQuan,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh,
                                                                    SoGen = a.SoGen
                                                                }

                                                                ).OrderBy(a => a.MaNhanVien).ToList();

                // Nếu có biến ListIdCongTyEOS thì sẽ lọc theo đống này - Duyệt mới thêm
                if (aRequest.ListIdCongTyEOS != null && aRequest.ListIdCongTyEOS.Count > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.ListIdCongTyEOS.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (aRequest.listIdBoPhan != null && aRequest.listIdBoPhan.Count > 0)
                {
                    listNhanVien = listNhanVien.Where(x => aRequest.listIdBoPhan.Contains(x.IdBoPhanEOS)).ToList();
                }

                if (aRequest.IdCongTyEOS > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.IdCongTyEOS == x.IdBoPhan).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                {
                    listNhanVien = listNhanVien.Where(a => a.IdChucVuEOS == aRequest.IdChucVuEOS).ToList();
                }

                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
                {
                    listNhanVien = listNhanVien.Where(a => (a.TenDayDu.ToLower().Contains(aRequest.KeyWord.ToLower().Trim()) || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.DienThoai) && a.DienThoai.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.SoGen) && a.SoGen.ToLower().Contains(aRequest.KeyWord.ToLower().Trim())))).ToList();
                }
                if (!string.IsNullOrEmpty(aRequest.TrangThai))
                {
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == aRequest.TrangThai).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0)
                {

                    var lstCongTyAll = _CallWebService.GetDanhSachDuAn(_dbContext);
                    var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(_dbContext);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);

                    var listIdNV = listNhanVien.Select(a => a.IdGuid).ToList();
                    var listSDT = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => a.isApDung == true && listIdNV.Contains(a.IdNhanVien)).ToList();
                    // Start Duyệt mới thêm
                    var listIdQuyTrinh_NhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                    var listQuyTrinh_BoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien.Contains(a.IdQuyTrinh)).ToList();
                    // End Duyệt mới thêm
                    DateTime dt = DateTime.Today;
                    foreach (var model in listNhanVien)
                    {
                        if (model.MaNhanVien == "0056")
                            model.MaNhanVien = "0056";
                        // Start Duyệt mới thêm
                        var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                        var _listBoPhan = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).Select(x => x.TenBoPhan);
                        model.TenCongTy = string.Join("; ", _listBoPhan);
                        // End Duyệt mới thêm


                        //model.TenCongTy = lstCongTyAll.Where(q => q.Id == model.IdCongTyEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.TenBoPhan = lstBoPhanAll.Where(q => q.Id == model.IdBoPhanEOS).Select(q => q.TenBoPhan).FirstOrDefault();
                        model.DienThoai = listSDT.Where(q => q.IdNhanVien == model.IdGuid).Select(q => q.SoDienThoai).FirstOrDefault();
                        model.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(model.NgaySinh);
                        model.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
                        model.NgayKetThucLamViecUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThucLamViec);
                        var NamBHXH = listNvBHXH.Where(a => a.isThamGia == true && a.IdNhanVien == model.IdGuid).ToList();
                        double SoThangDong = 0;
                        if (NamBHXH != null)
                        {
                            foreach (var ngay in NamBHXH)
                            {

                                if (ngay.DenNgay.Year <= 1970)
                                    ngay.DenNgay = dt;
                                double Day = (ngay.DenNgay - ngay.TuNgay).TotalDays + 1;
                                SoThangDong += ((ngay.DenNgay.Year - ngay.TuNgay.Year) * 12) + ngay.DenNgay.Month - ngay.TuNgay.Month + 1;
                            }
                            if (SoThangDong < 12)
                                model.TongThoiGianTheoNam = SoThangDong + " Tháng";
                            else
                            {
                                int Nam = (int)(SoThangDong / 12);
                                model.SoNamDongBHXH = Nam + " năm " + String.Format("{0:0}", (SoThangDong - (Nam * 12))) + " tháng";
                            }
                        }
                        var Check = lisGiaDinh.Where(a => a.IdNhanVien == model.IdGuid).ToList();
                        if (Check != null && Check.Count > 0)
                        {
                            model.CanhBaoQuaTuoi = "Quá tuổi phụ thuôc: " + string.Join(", ", Check.Select(a => a.HoTen).ToList());
                        }
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


                        if (model.NgayBatDauLamViec != null && model.NgayBatDauLamViec.Value.Month == timeNow.Month && model.NgayBatDauLamViec.Value.Day == timeNow.Day && model.NgayBatDauLamViec.Value.Year != timeNow.Year)
                        {
                            result.listKyNiemNgayVao.Add(model);
                        }
                        if (model.NgaySinh != null && model.NgaySinh.Month == timeNow.Month && model.NgaySinh.Day == timeNow.Day && model.NgaySinh.Year != timeNow.Year)
                        {
                            result.listSinhNhat.Add(model);
                        }
                        if (model.NgaySinh != null && model.GioiTinh != null && ((model.GioiTinh == "Nam" && model.NgaySinh <= timeNam) || (model.GioiTinh != "Nam" && model.NgaySinh <= timeNu)))
                        {

                            result.listDoTuoi.Add(model);
                        }
                        else if (listIdNvBHXH.Contains(model.IdGuid))
                        {
                            if (listBHXH != null && listBHXH.Count > 0)
                            {
                                if (model.GioiTinh == "Nam")
                                {
                                    if (SoThangDong >= 420)
                                        result.listLuongHuu.Add(model);
                                }
                                else if (model.GioiTinh != "Nam")
                                {
                                    if (SoThangDong >= 360)

                                        result.listLuongHuu.Add(model);
                                }
                            }
                        }

                        if (listGiayToHethan != null && listGiayToHethan.Count > 0)
                        {
                            var GiayTo = listGiayToHethan.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                            if (GiayTo != null)
                            {
                                model.NoiDung = "Hết hạn căn cước công dân";
                                result.listGiayToHetHan.Add(model);
                            }
                            var GiayToSK = listKhamSucKhoeHethan.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                            if (GiayToSK != null)
                            {
                                model.NoiDung = (model.NoiDung ?? "") + " Hết hạn căn cước công dân";

                            }
                            if (GiayTo != null || GiayToSK != null)
                            {
                                var itemadd = model.Clone();
                                result.listGiayToHetHan.Add(itemadd);
                            }
                        }
                        if (listHopDongHethan != null && listHopDongHethan.Count > 0)
                        {
                            var HopDong = listHopDongHethan.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                            if (HopDong != null && HopDong.NgayHetHan != null && HopDong.NgayHetHan.Value.Year > 2000 && HopDong.NgayHetHan <= dt)
                            {
                                if (HopDong.IdLoaiHopDong == LoaiHopDongThuViec.IdGuid)
                                    result.listHetHanThuViec.Add(model);
                                else
                                    result.listHetHanHopDong.Add(model);

                            }
                        }
                        if (lisGiaDinh != null && lisGiaDinh.Count > 0)
                        {
                            var HopDong = lisGiaDinh.FirstOrDefault(a => a.IdNhanVien == model.IdGuid);
                            if (HopDong != null)
                            {
                                model.NoiDung = HopDong.HoTen;
                                var itemadd = model.Clone();
                                result.listPhuThuoc.Add(itemadd);


                            }
                        }

                    }


                }
                return result;
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }

        public dynamic GetDanhSachNhanVienChuaCoHopDongByCongTy(long IdCongTyEOS, ApplicationDbContext db)
        {
            var lstNhanViens = db.HRM_NhanVien.Where(r => r.IdCongTyEOS == IdCongTyEOS && r.TrangThai != "DaNghiViec").ToList();
            var listIdNhanVien = lstNhanViens.Select(a => a.IdGuid).Distinct().ToList();
            var listPhuCap = db.HRM_NhanVien_PhuCap.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
            var dmTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
            var listdmPhuCap = db.HRM_dmPhuCap.ToList();
            foreach (var nhanVien in lstNhanViens)
            {
                nhanVien.NgayCapMaSTCNUnix = dungchung.DateTimeToUnixTimeStamp(nhanVien.NgayCapMaSTCN);
                nhanVien.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(nhanVien.NgaySinh);

                if (nhanVien.TrinhDoHocVan == null)
                    nhanVien.TrinhDoHocVan = dmTrinhDo.Where(r => r.IdGuid == nhanVien.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
                nhanVien.listPhuCap = new List<HRM_NhanVien_PhuCap>();
                if (listPhuCap != null && listPhuCap.Count > 0)
                {
                    nhanVien.listPhuCap = listPhuCap.Where(a => a.IdNhanVien == nhanVien.IdUngVien).OrderBy(a => a.Created).ToList();
                    if (nhanVien.listPhuCap != null & nhanVien.listPhuCap.Count > 0)
                    {

                        foreach (var pc in nhanVien.listPhuCap)
                        {
                            var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                            if (Phucap != null)
                                pc.TenPhuCap = Phucap.Ten;
                        }
                    }
                }
            }

            return lstNhanViens;
        }
        public MessageReturn SetThongTinNhanVien(HRM_NhanVien aModel, ApplicationDbContext _dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới nhân viên thành công");

            var lstHoTen = new List<string>();
            if (!string.IsNullOrEmpty(aModel.Ho))
                lstHoTen.Add(aModel.Ho);
            if (!string.IsNullOrEmpty(aModel.TenDem))
                lstHoTen.Add(aModel.TenDem);
            if (!string.IsNullOrEmpty(aModel.Ten))
                lstHoTen.Add(aModel.Ten);
            if (lstHoTen.Any() == false)
            {

            }
            else
                //Tên đầy đủ
                aModel.TenDayDu = lstHoTen.Join(" ");
            if (aModel.listIdCaLamViec != null && aModel.listIdCaLamViec.Count > 0)
                aModel.IdCaLamViec = string.Join(",", aModel.listIdCaLamViec);

            aModel.NgayNghiViec = UnixTimeStampToDateTime(aModel.NgayNghiViecUnix);
            aModel.NgaySinh = UnixTimeStampToDateTime(aModel.NgaySinhUnix);
            aModel.NgayCapMaSTCN = UnixTimeStampToDateTime(aModel.NgayCapMaSTCNUnix);

            if (aModel.ThangTruyThuBHYTUnix > 0)
                aModel.ThangTruyThuBHYT = UnixTimeStampToDateTime(aModel.ThangTruyThuBHYTUnix);
            if (aModel.ThangTruyThuBHXHUnix > 0)
                aModel.ThangTruyThuBHXH = UnixTimeStampToDateTime(aModel.ThangTruyThuBHXHUnix);
            aModel.NgayBatDauLamViec = UnixTimeStampToDateTime(aModel.NgayBatDauLamViecUnix);
            aModel.NgayBatDauThuViec = UnixTimeStampToDateTime(aModel.NgayBatDauThuViecUnix);
            aModel.NgayKetThucThuViec = UnixTimeStampToDateTime(aModel.NgayKetThucThuViecUnix);
            if (aModel.NgayBatDauChinhThucUnix > 0)
                aModel.NgayBatDauChinhThuc = UnixTimeStampToDateTime(aModel.NgayBatDauChinhThucUnix);

            //var listFileCreate = new List<EntityAddFileDinhKemModel>();
            //var listFileCreateNoMove = new List<EntityAddFileDinhKemModel>();
            //var listFileDelete = new List<HRM_FileDinhKem>();
            FileUploadController FileUpload = new FileUploadController();

            using (var ts = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var check = _dbContext.HRM_NhanVien.AsNoTracking().FirstOrDefault(r => r.MaNhanVien == aModel.MaNhanVien);
                    if (check != null)
                    {
                        if (check.IdGuid != aModel.IdGuid)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã nhân viên đã tồn tại");
                        else
                        {
                            if (check.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString() && aModel.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString())
                                aModel.NgayQuayLai = null;

                        }
                    }
                    if (string.IsNullOrEmpty(aModel.UserName))
                        aModel.UserName = aModel.MaNhanVien;
                    if (aModel.IdGuid.IsEmpty())
                    {

                        clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(_dbContext, aModel);
                        aModel.IdNhanVienEOS = _clsNhanVien.Id;
                        aModel.IdUser = _clsNhanVien.UserPkid;
                        aModel.IdGuid = Guid.NewGuid().ToString();
                        _dbContext.HRM_NhanVien.Add(aModel);
                        _dbContext.SaveChanges();
                        _CallWebService.AddModify(_dbContext, "Hồ sơ nhân viên", DefineData.eModify.Them.ToString(), DefineData.eAction.HOSONHANVIEN.ToString(), "Hồ sơ nhân viên - " + aModel.TenDayDu + "(" + aModel.MaNhanVien + ")", aModel.IdGuid, "", JsonConvert.SerializeObject(aModel), aModel.IdCongTyEOS, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);
                    }
                    else
                    {
                        clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(_dbContext, aModel);
                        aModel.IdUser = _clsNhanVien.UserPkid;
                        aModel.IdNhanVienEOS = _clsNhanVien.Id;


                        _dbContext.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _CallWebService.AddModify(_dbContext, "Hồ sơ nhân viên", DefineData.eModify.Sua.ToString(), DefineData.eAction.HOSONHANVIEN.ToString(), "Hồ sơ nhân viên - " + aModel.TenDayDu + "(" + aModel.MaNhanVien + ")", aModel.IdGuid, JsonConvert.SerializeObject(check), JsonConvert.SerializeObject(aModel), aModel.IdCongTyEOS, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);

                    }

                    clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSuNhanVien(_dbContext, aModel.IdNhanVienEOS, aModel.IdCongTyEOS, aModel.IdBoPhanEOS, aModel.IdChucVu, DateTime.Now, null);

                    var bangCaps = _dbContext.HRM_NhanVienBangCap.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var quanheGiadinh = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var ngonNgus = _dbContext.HRM_NgonNguNhanVien.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var CongTac = _dbContext.HRM_QuaTrinhCongTac.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var CongTacNgoaiCty = _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var chucChi = _dbContext.HRM_ChungChiNhanVien.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var HRM_NhanVien_TaiKhoanNganHang = _dbContext.HRM_NhanVien_TaiKhoanNganHang.Where(r => r.idNhanVien == aModel.IdGuid).ToList();
                    var HRM_NhanVien_SoDienThoais = _dbContext.HRM_NhanVien_SoDienThoai.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var HRM_NhanVien_GiayToTuyThans = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var HRM_NhanVien_Email = _dbContext.HRM_NhanVien_Email.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var listThamGiaBaoHiemXaHoi = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    var HRM_NhanVien_KhamSucKhoe = _dbContext.HRM_NhanVien_KhamSucKhoe.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    _dbContext.HRM_NhanVien_KhamSucKhoe.RemoveRange(HRM_NhanVien_KhamSucKhoe);
                    var ListBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == aModel.IdGuid).ToList();
                    _dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);

                    _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.RemoveRange(listThamGiaBaoHiemXaHoi);
                    _dbContext.HRM_NhanVien_GiayToTuyThan.RemoveRange(HRM_NhanVien_GiayToTuyThans);
                    _dbContext.HRM_NhanVien_Email.RemoveRange(HRM_NhanVien_Email);
                    _dbContext.HRM_NhanVienBangCap.RemoveRange(bangCaps);
                    _dbContext.HRM_NhanVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                    _dbContext.HRM_NgonNguNhanVien.RemoveRange(ngonNgus);
                    _dbContext.HRM_ChungChiNhanVien.RemoveRange(chucChi);
                    _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.RemoveRange(CongTacNgoaiCty);
                    _dbContext.HRM_QuaTrinhCongTac.RemoveRange(CongTac);
                    _dbContext.HRM_NhanVien_TaiKhoanNganHang.RemoveRange(HRM_NhanVien_TaiKhoanNganHang);
                    _dbContext.HRM_NhanVien_SoDienThoai.RemoveRange(HRM_NhanVien_SoDienThoais);
                    var BaoHiemNgoai = _dbContext.HRM_BaoHiem_HopDong.Where(x => x.IdNhanVien == aModel.IdGuid && string.IsNullOrEmpty(x.IdHopDong) && x.isNgoai == true);
                    if (BaoHiemNgoai != null)
                    {
                        _dbContext.HRM_BaoHiem_HopDong.RemoveRange(BaoHiemNgoai);
                    }
                    if (aModel.BaoHiemNgoai != null)
                    {

                        aModel.BaoHiemNgoai.IdGuid = Guid.NewGuid().ToString();
                        aModel.BaoHiemNgoai.IdNhanVien = aModel.IdGuid;
                        if (aModel.BaoHiemNgoai.NgayBatDauDongBHXHUnix > 0)
                            aModel.BaoHiemNgoai.NgayBatDauDongBHXH = dungchung.UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayBatDauDongBHXHUnix);
                        if (aModel.BaoHiemNgoai.NgayBatDauUnix > 0)
                        {
                            aModel.BaoHiemNgoai.NgayBatDau = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayBatDauUnix);
                        }
                        if (aModel.BaoHiemNgoai.NgayKetThucUnix > 0)
                        {
                            aModel.BaoHiemNgoai.NgayKetThuc = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayKetThucUnix);
                        }
                        if (aModel.BaoHiemNgoai.NgayCapSoBaoHiemUnix > 0)
                        {
                            aModel.BaoHiemNgoai.NgayCapSoBaoHiem = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayCapSoBaoHiemUnix);
                        }
                        aModel.BaoHiemNgoai.isNgoai = true;
                        _dbContext.HRM_BaoHiem_HopDong.Add(aModel.BaoHiemNgoai);
                    }
                    var BaoHiemTaiNan = _dbContext.HRM_NhanVien_BaoHiemTaiNan.Where(x => x.IdNhanVien == aModel.IdGuid);
                    if (BaoHiemTaiNan != null)
                    {
                        _dbContext.HRM_NhanVien_BaoHiemTaiNan.RemoveRange(BaoHiemTaiNan);
                    }
                    if (aModel.BaoHiemTaiNan != null)
                    {

                        aModel.BaoHiemTaiNan.IdGuid = Guid.NewGuid().ToString();
                        aModel.BaoHiemTaiNan.IdNhanVien = aModel.IdGuid;

                        if (aModel.BaoHiemTaiNan.NgayBatDauUnix > 0)
                        {
                            aModel.BaoHiemTaiNan.NgayBatDau = UnixTimeStampToDateTime(aModel.BaoHiemTaiNan.NgayBatDauUnix);
                        }
                        if (aModel.BaoHiemTaiNan.NgayKetThucUnix > 0)
                        {
                            aModel.BaoHiemTaiNan.NgayKetThuc = UnixTimeStampToDateTime(aModel.BaoHiemTaiNan.NgayKetThucUnix);
                        }
                        _dbContext.HRM_NhanVien_BaoHiemTaiNan.Add(aModel.BaoHiemTaiNan);
                    }

                    if (aModel.listIdCongTy != null)
                        foreach (var item in aModel.listIdCongTy)
                        {
                            HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                            bp.IdGuid = Guid.NewGuid().ToString();
                            bp.IdQuyTrinh = aModel.IdGuid;
                            bp.IdBoPhan = item;
                            bp.TableName = nameof(HRM_NhanVien);
                            _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                        }

                    #region FileDinhKemHoSo
                    if (aModel.AnhDaiDien != null && aModel.AnhDaiDien.FileName.IsNotEmpty())
                    {
                        List<HRM_FileDinhKem> listAnhDaiDien = new List<HRM_FileDinhKem>();
                        listAnhDaiDien.Add(aModel.AnhDaiDien);
                        FileUploadController.SetFileDinhKem(_dbContext, listAnhDaiDien, DefineData.eFlagDinhKem.AnhDaiDien.ToString(), aModel.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(aModel.AnhDaiDienMoi, aModel.IdGuid, nameof(HRM_NhanVien), DefineData.eFlagDinhKem.AnhDaiDien));
                        //var anhDaiDienCu = _dbContext.HRM_FileDinhKem.FirstOrDefault(x => x.idItem == aModel.IdGuid && x.TableName == nameof(HRM_NhanVien) && x.FlagDinhKem == DefineData.eFlagDinhKem.AnhDaiDien.ToString());
                        //if (anhDaiDienCu != null)
                        //    _dbContext.Entry(anhDaiDienCu).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                    }

                    if (aModel.lstFileDinhKem.IsNotEmpty())
                    {
                        FileUploadController.SetFileDinhKem(_dbContext, aModel.lstFileDinhKem, DefineData.eFlagDinhKem.FileDinhKem.ToString(), aModel.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(aModel.lstFileDinhKemMoi, aModel.IdGuid, nameof(HRM_NhanVien), DefineData.eFlagDinhKem.FileDinhKem));
                        //if (aModel.lstFileDinhKemCu.IsNotEmpty())
                        //    foreach (var file in aModel.lstFileDinhKemCu)
                        //        listFileDelete.Add(file);
                    }
                    var HRM_NhanVien_FileDinhKemHoSo = _dbContext.HRM_NhanVien_FileDinhKemHoSo.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                    _dbContext.HRM_NhanVien_FileDinhKemHoSo.RemoveRange(HRM_NhanVien_FileDinhKemHoSo);
                    if (aModel.listFileDinhKemHoSo != null)
                    {
                        foreach (var item in aModel.listFileDinhKemHoSo)
                        {
                            if (item.NgayCapUnix > 0)
                                item.NgayCap = dungchung.UnixTimeStampToDateTime(item.NgayCapUnix);
                            item.IdNhanVien = aModel.IdGuid;
                            if (string.IsNullOrEmpty(item.IdGuid))
                                item.IdGuid = Guid.NewGuid().ToString();
                            _dbContext.HRM_NhanVien_FileDinhKemHoSo.Add(item);


                            if (item.lstFileDinhKem != null)
                                FileUploadController.SetFileDinhKem(_dbContext, item.lstFileDinhKem, nameof(HRM_NhanVien_FileDinhKemHoSo).ToString(), item.IdGuid);
                        }


                    }
                    #endregion
                    #region giấy tờ tuy thân
                    if (aModel.listGiayToTuyThan.IsNotEmpty())
                    {
                        foreach (var bangCap in aModel.listGiayToTuyThan)
                        {
                            if (string.IsNullOrEmpty(bangCap.IdGuid))
                                bangCap.IdGuid = Guid.NewGuid().ToString();
                            if (bangCap.NgayCapUnix > 0)
                                bangCap.NgayCap = UnixTimeStampToDateTime(bangCap.NgayCapUnix);
                            if (bangCap.NgayHetHanUnix > 0)
                                bangCap.NgayHetHan = UnixTimeStampToDateTime(bangCap.NgayHetHanUnix);

                            bangCap.IdNhanVien = aModel.IdGuid;
                        }
                        _dbContext.HRM_NhanVien_GiayToTuyThan.AddRange(aModel.listGiayToTuyThan);
                    }
                    #endregion
                    #region Khám Sức Khỏe
                    if (aModel.listKhamSucKhoe.IsNotEmpty())
                    {
                        foreach (var bangCap in aModel.listKhamSucKhoe)
                        {
                            if (string.IsNullOrEmpty(bangCap.IdGuid))
                                bangCap.IdGuid = Guid.NewGuid().ToString();
                            if (bangCap.NgayKhamUnix > 0)
                                bangCap.NgayKham = UnixTimeStampToDateTime(bangCap.NgayKhamUnix);
                            if (bangCap.NgayKetThucUnix > 0)
                                bangCap.NgayKetThuc = UnixTimeStampToDateTime(bangCap.NgayKetThucUnix);

                            bangCap.IdNhanVien = aModel.IdGuid;
                        }
                        _dbContext.HRM_NhanVien_KhamSucKhoe.AddRange(aModel.listKhamSucKhoe);
                    }
                    #endregion

                    #region ThamGiaBaoHiemXaHoi
                    if (aModel.listThamGiaBaoHiemXaHoi.IsNotEmpty())
                    {
                        foreach (var bangCap in aModel.listThamGiaBaoHiemXaHoi)
                        {

                            bangCap.IdGuid = Guid.NewGuid().ToString();
                            bangCap.TuNgay = dungchung.UnixTimeStampToDateTime(bangCap.TuNgayUnix);
                            bangCap.DenNgay = dungchung.UnixTimeStampToDateTime(bangCap.DenNgayUnix);

                            bangCap.IdNhanVien = aModel.IdGuid;
                        }
                        _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AddRange(aModel.listThamGiaBaoHiemXaHoi);
                    }
                    #endregion

                    #region Số điện thoại
                    if (aModel.listSoDienThoai.IsNotEmpty())
                    {
                        foreach (var bangCap in aModel.listSoDienThoai)
                        {
                            if (string.IsNullOrEmpty(bangCap.IdGuid))
                                bangCap.IdGuid = Guid.NewGuid().ToString();
                            bangCap.IdNhanVien = aModel.IdGuid;
                        }
                        _dbContext.HRM_NhanVien_SoDienThoai.AddRange(aModel.listSoDienThoai);
                    }
                    #endregion
                    #region Số điện thoại
                    if (aModel.listEmail.IsNotEmpty())
                    {
                        foreach (var bangCap in aModel.listEmail)
                        {
                            if (string.IsNullOrEmpty(bangCap.IdGuid))
                                bangCap.IdGuid = Guid.NewGuid().ToString();
                            bangCap.IdNhanVien = aModel.IdGuid;
                        }
                        _dbContext.HRM_NhanVien_Email.AddRange(aModel.listEmail);
                    }
                    #endregion

                    #region Bằng cấp
                    if (aModel.ListNhanVienBangCap.IsNotEmpty())
                    {
                        foreach (var bangCap in aModel.ListNhanVienBangCap)
                        {
                            if (string.IsNullOrEmpty(bangCap.IdGuid))
                                bangCap.IdGuid = Guid.NewGuid().ToString();
                            bangCap.IdNhanVien = aModel.IdGuid;
                            if (bangCap.lstFileDinhKem.IsNotEmpty())
                                FileUploadController.SetFileDinhKem(_dbContext, bangCap.lstFileDinhKem, nameof(HRM_NhanVienBangCap).ToString(), bangCap.IdGuid);
                            //listFileCreate.Add(new EntityAddFileDinhKemModel(, bangCap.IdGuid, nameof(HRM_NhanVienBangCap), DefineData.eFlagDinhKem.FileDinhKem));
                            if (bangCap.NgayTotNghiepUnix > 0)
                                bangCap.NgayTotNghiep = dungchung.UnixTimeStampToDateTime(bangCap.NgayTotNghiepUnix);
                        }
                        _dbContext.HRM_NhanVienBangCap.AddRange(aModel.ListNhanVienBangCap);
                    }
                    #endregion
                    #region Ngôn ngữ
                    if (aModel.ListNhanVienNgonNgu.IsNotEmpty())
                    {
                        foreach (var ngonNgu in aModel.ListNhanVienNgonNgu)
                        {
                            if (string.IsNullOrEmpty(ngonNgu.IdGuid))
                                ngonNgu.IdGuid = Guid.NewGuid().ToString();
                            ngonNgu.IdNhanVien = aModel.IdGuid;
                            if (ngonNgu.lstFileDinhKem.IsNotEmpty())
                                FileUploadController.SetFileDinhKem(_dbContext, ngonNgu.lstFileDinhKem, nameof(HRM_NgonNguNhanVien).ToString(), ngonNgu.IdGuid);
                            //listFileCreate.Add(new EntityAddFileDinhKemModel(ngonNgu.lstFileDinhKemMoi, ngonNgu.IdGuid, nameof(HRM_NgonNguNhanVien), DefineData.eFlagDinhKem.FileDinhKem));

                        }
                        _dbContext.HRM_NgonNguNhanVien.AddRange(aModel.ListNhanVienNgonNgu);
                    }
                    #endregion
                    #region Chứng chỉ
                    if (aModel.ListNhanVienChungChi.IsNotEmpty())
                    {
                        foreach (var chungChi in aModel.ListNhanVienChungChi)
                        {
                            chungChi.UnixToDateTime();
                            if (string.IsNullOrEmpty(chungChi.IdGuid))
                                chungChi.IdGuid = Guid.NewGuid().ToString();
                            chungChi.IdNhanVien = aModel.IdGuid;
                            if (chungChi.lstFileDinhKem.IsNotEmpty())
                                FileUploadController.SetFileDinhKem(_dbContext, chungChi.lstFileDinhKem, nameof(HRM_ChungChiNhanVien).ToString(), chungChi.IdGuid);
                            //listFileCreate.Add(new EntityAddFileDinhKemModel(chungChi.lstFileDinhKemMoi, chungChi.IdGuid, nameof(HRM_ChungChiNhanVien), DefineData.eFlagDinhKem.FileDinhKem));


                        }
                        _dbContext.HRM_ChungChiNhanVien.AddRange(aModel.ListNhanVienChungChi);
                    }
                    #endregion
                    #region  Quá trình công tác
                    if (aModel.ListQuaTrinhCongTac.IsNotEmpty())
                    {
                        foreach (var congTac in aModel.ListQuaTrinhCongTac)
                        {
                            if (string.IsNullOrEmpty(congTac.IdGuid))
                                congTac.IdGuid = Guid.NewGuid().ToString();
                            congTac.IdNhanVien = aModel.IdGuid;
                            if (congTac.lstFileDinhKem.IsNotEmpty())
                                FileUploadController.SetFileDinhKem(_dbContext, congTac.lstFileDinhKem, nameof(HRM_QuaTrinhCongTac).ToString(), congTac.IdGuid);
                            //listFileCreate.Add(new EntityAddFileDinhKemModel(congTac.lstFileDinhKemMoi, congTac.IdGuid, nameof(HRM_QuaTrinhCongTac), DefineData.eFlagDinhKem.FileDinhKem));

                        }
                        _dbContext.HRM_QuaTrinhCongTac.AddRange(aModel.ListQuaTrinhCongTac);
                    }
                    #endregion
                    #region  Quá trình công tác nước ngoài
                    if (aModel.ListQuaTrinhCongTacNgoaiCongTy.IsNotEmpty())
                    {
                        foreach (var congTac in aModel.ListQuaTrinhCongTacNgoaiCongTy)
                        {
                            if (string.IsNullOrEmpty(congTac.IdGuid))
                                congTac.IdGuid = Guid.NewGuid().ToString();
                            congTac.IdNhanVien = aModel.IdGuid;
                            if (congTac.lstFileDinhKem.IsNotEmpty())
                                FileUploadController.SetFileDinhKem(_dbContext, congTac.lstFileDinhKem, nameof(HRM_QuaTrinhCongTacNgoaiCongTy).ToString(), congTac.IdGuid);
                            //listFileCreate.Add(new EntityAddFileDinhKemModel(congTac.lstFileDinhKemMoi, congTac.IdGuid, nameof(HRM_QuaTrinhCongTacNgoaiCongTy), DefineData.eFlagDinhKem.FileDinhKem));

                        }
                        _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.AddRange(aModel.ListQuaTrinhCongTacNgoaiCongTy);
                    }
                    #endregion
                    #region Ngân hàng
                    if (aModel.ListNganHang.IsNotEmpty())
                    {
                        foreach (var nganHang in aModel.ListNganHang)
                        {
                            nganHang.IdGuid = Guid.NewGuid().ToString();
                            nganHang.idNhanVien = aModel.IdGuid;

                        }
                        _dbContext.HRM_NhanVien_TaiKhoanNganHang.AddRange(aModel.ListNganHang);
                    }
                    #endregion
                    #region Quan hệ gia đình
                    if (aModel.ListQuanHeGiaDinhNhanVien.IsNotEmpty())
                    {
                        foreach (var giaDinh in aModel.ListQuanHeGiaDinhNhanVien)
                        {
                            giaDinh.NgaySinh = dungchung.UnixTimeStampToDateTime(giaDinh.NgaySinhUnix);
                            if (string.IsNullOrEmpty(giaDinh.IdGuid))
                                giaDinh.IdGuid = Guid.NewGuid().ToString();
                            giaDinh.IdNhanVien = aModel.IdGuid;
                            if (giaDinh.lstFileDinhKem.IsNotEmpty())
                                FileUploadController.SetFileDinhKem(_dbContext, giaDinh.lstFileDinhKem, nameof(HRM_NhanVienQuanHeGiaDinh).ToString(), giaDinh.IdGuid);
                            //listFileCreate.Add(new EntityAddFileDinhKemModel(giaDinh.lstFileDinhKemMoi, giaDinh.IdGuid, nameof(HRM_NhanVienQuanHeGiaDinh), DefineData.eFlagDinhKem.FileDinhKem));

                        }
                        _dbContext.HRM_NhanVienQuanHeGiaDinh.AddRange(aModel.ListQuanHeGiaDinhNhanVien);
                    }
                    #endregion
                    #region trạng thái
                    var baohiem_hopdong = _dbContext.HRM_BaoHiem_HopDong.Where(x => x.IdNhanVien == aModel.IdGuid).ToList();
                    foreach (var item in baohiem_hopdong)
                    {
                        if (item.isBaoHiemXaHoi == true && item.isNgoai == false)
                        {
                            if (aModel.TrangThai == eTinhTrangLamViec.DaNghi.ToString() || aModel.TrangThai == eTinhTrangLamViec.NghiThaiSan.ToString() || aModel.TrangThai == eTinhTrangLamViec.NghiKhongLuong.ToString())
                            {
                                var giamBH = _dbContext.HRM_GiamBaoHiem.Where(x => x.IdNhanVien == aModel.IdGuid).FirstOrDefault();
                                HRM_GiamBaoHiem giamBHNhanVien;
                                if (giamBH == null)
                                {
                                    giamBHNhanVien = new HRM_GiamBaoHiem();
                                    {
                                        giamBHNhanVien.IdGuid = Guid.NewGuid().ToString();
                                        giamBHNhanVien.IdNhanVien = aModel.IdGuid;
                                    }
                                    if (aModel.TrangThai == eTinhTrangLamViec.DaNghi.ToString())
                                    {
                                        giamBHNhanVien.Loai = eLoaiBaoHiem.GiamDoNghiViec.ToString();
                                    }
                                    if (aModel.TrangThai == eTinhTrangLamViec.NghiThaiSan.ToString())
                                    {
                                        giamBHNhanVien.Loai = eLoaiBaoHiem.GiamThaiSan.ToString();
                                    }
                                    else
                                        giamBHNhanVien.Loai = eLoaiBaoHiem.GiamKhongLuong.ToString();
                                    _dbContext.HRM_GiamBaoHiem.Add(giamBHNhanVien);
                                }
                                //else
                                //{
                                //    giamBHNhanVien = giamBH;
                                //    if (aModel.TrangThai == eTinhTrangLamViec.DaNghi.ToString())
                                //    {
                                //        giamBHNhanVien.Loai = eLoaiBaoHiem.GiamDoNghiViec.ToString();
                                //    }
                                //    else if (aModel.TrangThai == eTinhTrangLamViec.NghiThaiSan.ToString())
                                //    {
                                //        giamBHNhanVien.Loai = eLoaiBaoHiem.GiamThaiSan.ToString();
                                //    }
                                //    else
                                //    {
                                //        giamBHNhanVien.Loai = eLoaiBaoHiem.GiamKhongLuong.ToString();
                                //    }
                                //    _dbContext.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                //}
                            }
                        }
                    }
                    #endregion

                    _dbContext.SaveChanges();

                    ts.Commit();
                    result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu nhân viên thành công", aModel.IdGuid);

                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }

            return result;
        }
        public MessageReturn SetThongTinNhanVienTransaction(HRM_NhanVien aModel, ApplicationDbContext _dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới nhân viên thành công");

            var lstHoTen = new List<string>();
            if (!string.IsNullOrEmpty(aModel.Ho))
                lstHoTen.Add(aModel.Ho);
            if (!string.IsNullOrEmpty(aModel.TenDem))
                lstHoTen.Add(aModel.TenDem);
            if (!string.IsNullOrEmpty(aModel.Ten))
                lstHoTen.Add(aModel.Ten);
            if (lstHoTen.Any() == false)
            {

            }
            else
                //Tên đầy đủ
                aModel.TenDayDu = lstHoTen.Join(" ");
            if (aModel.ThangTruyThuBHYTUnix > 0)
                aModel.ThangTruyThuBHYT = UnixTimeStampToDateTime(aModel.ThangTruyThuBHYTUnix);
            if (aModel.ThangTruyThuBHXHUnix > 0)
                aModel.ThangTruyThuBHXH = UnixTimeStampToDateTime(aModel.ThangTruyThuBHXHUnix);
            if (aModel.NgayNghiViecUnix > 0)
                aModel.NgayNghiViec = UnixTimeStampToDateTime(aModel.NgayNghiViecUnix);
            if (aModel.NgaySinhUnix > 0)
                aModel.NgaySinh = UnixTimeStampToDateTime(aModel.NgaySinhUnix);
            if (aModel.NgayCapMaSTCNUnix > 0)
                aModel.NgayCapMaSTCN = UnixTimeStampToDateTime(aModel.NgayCapMaSTCNUnix);
            if (aModel.NgayBatDauLamViecUnix > 0)
                aModel.NgayBatDauLamViec = UnixTimeStampToDateTime(aModel.NgayBatDauLamViecUnix);
            if (aModel.NgayBatDauThuViecUnix > 0)
                aModel.NgayBatDauThuViec = UnixTimeStampToDateTime(aModel.NgayBatDauThuViecUnix);
            if (aModel.NgayKetThucThuViecUnix > 0)
                aModel.NgayKetThucThuViec = UnixTimeStampToDateTime(aModel.NgayKetThucThuViecUnix);
            if (aModel.NgayBatDauChinhThucUnix > 0)
                aModel.NgayBatDauChinhThuc = UnixTimeStampToDateTime(aModel.NgayBatDauChinhThucUnix);
            //var listFileCreate = new List<EntityAddFileDinhKemModel>();
            //var listFileCreateNoMove = new List<EntityAddFileDinhKemModel>();
            //var listFileDelete = new List<HRM_FileDinhKem>();
            FileUploadController FileUpload = new FileUploadController();


            try
            {
                var check = _dbContext.HRM_NhanVien.AsNoTracking().FirstOrDefault(r => r.MaNhanVien == aModel.MaNhanVien);
                if (check != null)
                {
                    if (check.IdGuid != aModel.IdGuid)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã nhân viên đã tồn tại");
                    else
                    {
                        if (check.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString() && aModel.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString())
                            aModel.NgayQuayLai = null;

                    }
                }
                if (string.IsNullOrEmpty(aModel.UserName))
                    aModel.UserName = aModel.MaNhanVien;
                if (aModel.IdGuid.IsEmpty())
                {

                    clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(_dbContext, aModel);
                    aModel.IdNhanVienEOS = _clsNhanVien.Id;
                    aModel.IdUser = _clsNhanVien.UserPkid;
                    aModel.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_NhanVien.Add(aModel);
                    _dbContext.SaveChanges();
                    _CallWebService.AddModify(_dbContext, "Hồ sơ nhân viên", DefineData.eModify.Them.ToString(), DefineData.eAction.HOSONHANVIEN.ToString(), "Hồ sơ nhân viên - " + aModel.TenDayDu + "(" + aModel.MaNhanVien + ")", aModel.IdGuid, "", JsonConvert.SerializeObject(aModel), aModel.IdCongTyEOS, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);
                }
                else
                {
                    clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(_dbContext, aModel);
                    aModel.IdUser = _clsNhanVien.UserPkid;
                    aModel.IdNhanVienEOS = _clsNhanVien.Id;


                    _dbContext.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _CallWebService.AddModify(_dbContext, "Hồ sơ nhân viên", DefineData.eModify.Sua.ToString(), DefineData.eAction.HOSONHANVIEN.ToString(), "Hồ sơ nhân viên - " + aModel.TenDayDu + "(" + aModel.MaNhanVien + ")", aModel.IdGuid, "", JsonConvert.SerializeObject(aModel), aModel.IdCongTyEOS, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);

                }

                clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSuNhanVien(_dbContext, aModel.IdNhanVienEOS, aModel.IdCongTyEOS, aModel.IdBoPhanEOS, aModel.IdChucVu, DateTime.Now, null);


                var listPhuCapold = _dbContext.HRM_NhanVien_PhuCap.Where(a => a.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_NhanVien_PhuCap.RemoveRange(listPhuCapold);
                if (aModel.listPhuCap != null && aModel.listPhuCap.Count > 0)
                {
                    foreach (var item in aModel.listPhuCap)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_NhanVien_PhuCap.AddRange(aModel.listPhuCap);
                }
                var bangCaps = _dbContext.HRM_NhanVienBangCap.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var quanheGiadinh = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var ngonNgus = _dbContext.HRM_NgonNguNhanVien.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var CongTac = _dbContext.HRM_QuaTrinhCongTac.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var CongTacNgoaiCty = _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var chucChi = _dbContext.HRM_ChungChiNhanVien.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var HRM_NhanVien_TaiKhoanNganHang = _dbContext.HRM_NhanVien_TaiKhoanNganHang.Where(r => r.idNhanVien == aModel.IdGuid).ToList();
                var HRM_NhanVien_SoDienThoais = _dbContext.HRM_NhanVien_SoDienThoai.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var HRM_NhanVien_GiayToTuyThans = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_NhanVien_GiayToTuyThan.RemoveRange(HRM_NhanVien_GiayToTuyThans);
                var listThamGiaBaoHiemXaHoi = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.RemoveRange(listThamGiaBaoHiemXaHoi);
                var HRM_NhanVien_Email = _dbContext.HRM_NhanVien_Email.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_NhanVien_Email.RemoveRange(HRM_NhanVien_Email);
                var ListBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == aModel.IdGuid).ToList();
                _dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                _dbContext.HRM_NhanVienBangCap.RemoveRange(bangCaps);
                _dbContext.HRM_NhanVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                _dbContext.HRM_NgonNguNhanVien.RemoveRange(ngonNgus);
                _dbContext.HRM_ChungChiNhanVien.RemoveRange(chucChi);
                _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.RemoveRange(CongTacNgoaiCty);
                _dbContext.HRM_QuaTrinhCongTac.RemoveRange(CongTac);
                _dbContext.HRM_NhanVien_TaiKhoanNganHang.RemoveRange(HRM_NhanVien_TaiKhoanNganHang);
                _dbContext.HRM_NhanVien_SoDienThoai.RemoveRange(HRM_NhanVien_SoDienThoais);
                var HRM_NhanVien_KhamSucKhoe = _dbContext.HRM_NhanVien_KhamSucKhoe.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_NhanVien_KhamSucKhoe.RemoveRange(HRM_NhanVien_KhamSucKhoe);
                if (aModel.listIdCongTy != null)
                    foreach (var item in aModel.listIdCongTy)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = aModel.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_NhanVien);
                        _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }

                #region FileDinhKemHoSo
                if (aModel.AnhDaiDien != null && aModel.AnhDaiDien.FileName.IsNotEmpty())
                {
                    List<HRM_FileDinhKem> listAnhDaiDien = new List<HRM_FileDinhKem>();
                    listAnhDaiDien.Add(aModel.AnhDaiDien);
                    FileUploadController.SetFileDinhKem(_dbContext, listAnhDaiDien, DefineData.eFlagDinhKem.AnhDaiDien.ToString(), aModel.IdGuid);
                    //listFileCreate.Add(new EntityAddFileDinhKemModel(aModel.AnhDaiDienMoi, aModel.IdGuid, nameof(HRM_NhanVien), DefineData.eFlagDinhKem.AnhDaiDien));
                    //var anhDaiDienCu = _dbContext.HRM_FileDinhKem.FirstOrDefault(x => x.idItem == aModel.IdGuid && x.TableName == nameof(HRM_NhanVien) && x.FlagDinhKem == DefineData.eFlagDinhKem.AnhDaiDien.ToString());
                    //if (anhDaiDienCu != null)
                    //    _dbContext.Entry(anhDaiDienCu).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }

                if (aModel.lstFileDinhKem.IsNotEmpty())
                {
                    FileUploadController.SetFileDinhKem(_dbContext, aModel.lstFileDinhKem, DefineData.eFlagDinhKem.FileDinhKem.ToString(), aModel.IdGuid);
                    //listFileCreate.Add(new EntityAddFileDinhKemModel(aModel.lstFileDinhKemMoi, aModel.IdGuid, nameof(HRM_NhanVien), DefineData.eFlagDinhKem.FileDinhKem));
                    //if (aModel.lstFileDinhKemCu.IsNotEmpty())
                    //    foreach (var file in aModel.lstFileDinhKemCu)
                    //        listFileDelete.Add(file);
                }
                #region Khám Sức Khỏe
                if (aModel.listKhamSucKhoe.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listKhamSucKhoe)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        if (bangCap.NgayKhamUnix > 0)
                            bangCap.NgayKham = UnixTimeStampToDateTime(bangCap.NgayKhamUnix);
                        if (bangCap.NgayKetThucUnix > 0)
                            bangCap.NgayKetThuc = UnixTimeStampToDateTime(bangCap.NgayKetThucUnix);

                        bangCap.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_NhanVien_KhamSucKhoe.AddRange(aModel.listKhamSucKhoe);
                }
                #endregion
                #region ThamGiaBaoHiemXaHoi
                if (aModel.listThamGiaBaoHiemXaHoi.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listThamGiaBaoHiemXaHoi)
                    {

                        bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.TuNgay = dungchung.UnixTimeStampToDateTime(bangCap.TuNgayUnix);
                        bangCap.DenNgay = dungchung.UnixTimeStampToDateTime(bangCap.DenNgayUnix);

                        bangCap.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AddRange(aModel.listThamGiaBaoHiemXaHoi);
                }
                #endregion

                if (aModel.listFileDinhKemHoSo != null)
                {
                    foreach (var item in aModel.listFileDinhKemHoSo)
                    {
                        //if (item.IdGuid.IsEmpty())
                        //{
                        //    item.IdNhanVien = aModel.IdGuid;
                        //    item.IdGuid = Guid.NewGuid().ToString();
                        //    if (item.lstFileDinhKemMoi != null)
                        //        listFileCreateNoMove.Add(new EntityAddFileDinhKemModel(item.lstFileDinhKemMoi, item.IdGuid, nameof(HRM_NhanVien_FileDinhKemHoSo), DefineData.eFlagDinhKem.FileDinhKem));
                        //    _dbContext.HRM_NhanVien_FileDinhKemHoSo.Add(item);
                        //}
                        //else
                        //{
                        //    item.IdNhanVien = aModel.IdGuid;
                        //    if (item.lstFileDinhKemMoi != null)
                        //        listFileCreateNoMove.Add(new EntityAddFileDinhKemModel(item.lstFileDinhKemMoi, item.IdGuid, nameof(HRM_NhanVien_FileDinhKemHoSo), DefineData.eFlagDinhKem.FileDinhKem));
                        //    _dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        //}
                        FileUploadController.SetFileDinhKem(_dbContext, item.lstFileDinhKem, nameof(HRM_NhanVien_FileDinhKemHoSo).ToString(), aModel.IdGuid);
                    }


                }
                #endregion
                #region Email
                if (aModel.listEmail.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listEmail)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_NhanVien_Email.AddRange(aModel.listEmail);
                }
                #endregion
                #region giấy tờ tuy thân
                if (aModel.listGiayToTuyThan.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listGiayToTuyThan)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                        if (bangCap.NgayCapUnix > 0)
                            bangCap.NgayCap = UnixTimeStampToDateTime(bangCap.NgayCapUnix);
                        if (bangCap.NgayHetHanUnix > 0)
                            bangCap.NgayHetHan = UnixTimeStampToDateTime(bangCap.NgayHetHanUnix);

                    }
                    _dbContext.HRM_NhanVien_GiayToTuyThan.AddRange(aModel.listGiayToTuyThan);
                }
                #endregion
                #region Số điện thoại
                if (aModel.listSoDienThoai.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listSoDienThoai)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_NhanVien_SoDienThoai.AddRange(aModel.listSoDienThoai);
                }
                #endregion
                #region Bằng cấp
                if (aModel.ListNhanVienBangCap.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.ListNhanVienBangCap)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                        if (bangCap.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, bangCap.lstFileDinhKem, nameof(HRM_NhanVienBangCap).ToString(), bangCap.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(, bangCap.IdGuid, nameof(HRM_NhanVienBangCap), DefineData.eFlagDinhKem.FileDinhKem));
                        if (bangCap.NgayTotNghiepUnix > 0)
                            bangCap.NgayTotNghiep = dungchung.UnixTimeStampToDateTime(bangCap.NgayTotNghiepUnix);
                    }
                    _dbContext.HRM_NhanVienBangCap.AddRange(aModel.ListNhanVienBangCap);
                }
                #endregion

                #region Ngôn ngữ
                if (aModel.ListNhanVienNgonNgu.IsNotEmpty())
                {
                    foreach (var ngonNgu in aModel.ListNhanVienNgonNgu)
                    {
                        if (string.IsNullOrEmpty(ngonNgu.IdGuid))
                            ngonNgu.IdGuid = Guid.NewGuid().ToString();
                        ngonNgu.IdNhanVien = aModel.IdGuid;
                        if (ngonNgu.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, ngonNgu.lstFileDinhKem, nameof(HRM_NgonNguNhanVien).ToString(), ngonNgu.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(ngonNgu.lstFileDinhKemMoi, ngonNgu.IdGuid, nameof(HRM_NgonNguNhanVien), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_NgonNguNhanVien.AddRange(aModel.ListNhanVienNgonNgu);
                }
                #endregion
                #region Chứng chỉ
                if (aModel.ListNhanVienChungChi.IsNotEmpty())
                {
                    foreach (var chungChi in aModel.ListNhanVienChungChi)
                    {
                        chungChi.UnixToDateTime();
                        if (string.IsNullOrEmpty(chungChi.IdGuid))
                            chungChi.IdGuid = Guid.NewGuid().ToString();
                        chungChi.IdNhanVien = aModel.IdGuid;
                        if (chungChi.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, chungChi.lstFileDinhKem, nameof(HRM_ChungChiNhanVien).ToString(), chungChi.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(chungChi.lstFileDinhKemMoi, chungChi.IdGuid, nameof(HRM_ChungChiNhanVien), DefineData.eFlagDinhKem.FileDinhKem));


                    }
                    _dbContext.HRM_ChungChiNhanVien.AddRange(aModel.ListNhanVienChungChi);
                }
                #endregion
                #region  Quá trình công tác
                if (aModel.ListQuaTrinhCongTac.IsNotEmpty())
                {
                    foreach (var congTac in aModel.ListQuaTrinhCongTac)
                    {
                        if (string.IsNullOrEmpty(congTac.IdGuid))
                            congTac.IdGuid = Guid.NewGuid().ToString();
                        congTac.IdNhanVien = aModel.IdGuid;
                        if (congTac.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, congTac.lstFileDinhKem, nameof(HRM_QuaTrinhCongTac).ToString(), congTac.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(congTac.lstFileDinhKemMoi, congTac.IdGuid, nameof(HRM_QuaTrinhCongTac), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_QuaTrinhCongTac.AddRange(aModel.ListQuaTrinhCongTac);
                }
                #endregion
                #region  Quá trình công tác nước ngoài
                if (aModel.ListQuaTrinhCongTacNgoaiCongTy.IsNotEmpty())
                {
                    foreach (var congTac in aModel.ListQuaTrinhCongTacNgoaiCongTy)
                    {
                        if (string.IsNullOrEmpty(congTac.IdGuid))
                            congTac.IdGuid = Guid.NewGuid().ToString();
                        congTac.IdNhanVien = aModel.IdGuid;
                        if (congTac.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, congTac.lstFileDinhKem, nameof(HRM_QuaTrinhCongTacNgoaiCongTy).ToString(), congTac.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(congTac.lstFileDinhKemMoi, congTac.IdGuid, nameof(HRM_QuaTrinhCongTacNgoaiCongTy), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.AddRange(aModel.ListQuaTrinhCongTacNgoaiCongTy);
                }
                #endregion

                #region Ngân hàng
                if (aModel.ListNganHang.IsNotEmpty())
                {
                    foreach (var nganHang in aModel.ListNganHang)
                    {
                        nganHang.IdGuid = Guid.NewGuid().ToString();
                        nganHang.idNhanVien = aModel.IdGuid;

                    }
                    _dbContext.HRM_NhanVien_TaiKhoanNganHang.AddRange(aModel.ListNganHang);
                }
                #endregion
                #region Quan hệ gia đình
                if (aModel.ListQuanHeGiaDinhNhanVien.IsNotEmpty())
                {
                    foreach (var giaDinh in aModel.ListQuanHeGiaDinhNhanVien)
                    {
                        if (giaDinh.NgaySinhUnix > 0)
                            giaDinh.NgaySinh = dungchung.UnixTimeStampToDateTime(giaDinh.NgaySinhUnix);
                        if (string.IsNullOrEmpty(giaDinh.IdGuid))
                            giaDinh.IdGuid = Guid.NewGuid().ToString();
                        giaDinh.IdNhanVien = aModel.IdGuid;
                        if (giaDinh.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, giaDinh.lstFileDinhKem, nameof(HRM_NhanVienQuanHeGiaDinh).ToString(), giaDinh.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(giaDinh.lstFileDinhKemMoi, giaDinh.IdGuid, nameof(HRM_NhanVienQuanHeGiaDinh), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_NhanVienQuanHeGiaDinh.AddRange(aModel.ListQuanHeGiaDinhNhanVien);
                }
                #endregion
                _dbContext.SaveChanges();


                result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu nhân viên thành công", aModel.IdGuid);

            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }


            return result;
        }
        public string GetMaNhanVien(ApplicationDbContext db)
        {
            //string sMaPhieu = DefineData.MaNhanVien;

            //string sSql = "select max(right(MaNhanVien,4)) + 1 from " + DefineData.eTable.HRM_NhanVien + " where MaNhanVien like '" + sMaPhieu + "%'";
            //int nID = db.Database.SqlQuery<int?>(sSql).FirstOrDefault() ?? 1;

            //string sMaDuoi = nID.ToString();

            //while (sMaDuoi.Length < 5)
            //{
            //    sMaDuoi = "0" + sMaDuoi;
            //}
            string sTT = "";
            var QuyTrinh = db.HRM_NhanVien.Where(a => !string.IsNullOrEmpty(a.MaNhanVien)).OrderByDescending(a => a.MaNhanVien).AsNoTracking().FirstOrDefault();
            string SoQuyTrinh = "";
            if (QuyTrinh != null)
                SoQuyTrinh = QuyTrinh.MaNhanVien;

            int nID = 1;
            try
            {
                if (!string.IsNullOrEmpty(SoQuyTrinh))
                    nID = int.Parse(SoQuyTrinh.Split('_')[1].ToString() ?? "1") + 1;
            }
            catch
            {
                try
                {
                    nID = int.Parse(SoQuyTrinh) + 1;
                }
                catch { }
            }
            string s = nID.ToString();
            while (s.Length < 6)
            {
                s = "0" + s;
            }
            string sSoPhieu = sTT + s;
            return sSoPhieu;

        }

        #endregion
        public dynamic GetMucLuongHienTaiNhanVien(string Id, ApplicationDbContext db)
        {

            HRM_Luong_NhanVien luongNhanVien = db.HRM_Luong_NhanVien.OrderByDescending(cr => cr.NgayBatDauHieuLuc).FirstOrDefault(r => r.IdNhanVien == Id);
            if (luongNhanVien != null)
            {
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(r => r.IdNhanVien == luongNhanVien.IdNhanVien).ToList();
                foreach (var item in listPhuCap)
                {
                    item.NgayBatDauHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDauHieuLuc);
                    item.NgayHetHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHieuLuc);
                }

                luongNhanVien.listPhuCap = listPhuCap;
                var HopDong = db.HRM_HopDong.FirstOrDefault(a => a.IdNhanVien == Id && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString());
                if (HopDong != null)
                {
                    var lisTaiSan = db.HRM_dmTrangThietBi.ToList();
                    luongNhanVien.listTaiSan = db.HRM_HopDongTaiSanBanGiao.Where(a => a.IdHopDong == HopDong.IdGuid).ToList();
                    if (luongNhanVien.listTaiSan != null)
                        foreach (var item in luongNhanVien.listTaiSan)
                        {
                            item.TenTaiSanBanGiao = lisTaiSan.FirstOrDefault(a => a.IdGuid == item.IdTaiSanBanGiao) == null ? "" : lisTaiSan.FirstOrDefault(a => a.IdGuid == item.IdTaiSanBanGiao).Ten;
                        }
                }
                return luongNhanVien;

            }
            return new HRM_Luong_NhanVien();
        }
        public dynamic GetMucLuongHienTailistNhanVien(List<string> Id, ApplicationDbContext db)
        {
            DateTime time = DateTime.Now;
            var luongNhanVien = (from a in db.HRM_Luong_NhanVien
                                 join b in db.HRM_NhanVien on a.IdNhanVien equals b.IdGuid
                                 where Id.Contains(b.IdUser) && a.NgayBatDauHieuLuc <= time && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc > time)
                                 group a by new { IdNhanVien = a.IdNhanVien, IdUser = b.IdUser, Luong = a.Luong, NgayHieuLuc = a.NgayBatDauHieuLuc } into c
                                 select new { IdUser = c.Key.IdUser, Luong = c.Key.Luong, NgayHieuLuc = c.Key.NgayHieuLuc }
                                                    ).ToList();
            if (luongNhanVien != null)
            {

                return luongNhanVien;

            }
            return new HRM_Luong_NhanVien();
        }
        public EntitySingleResponse<HRM_NhanVien> GetThongTinNhanVien(string IdNhanVien, ApplicationDbContext db)
        {
            var result = new EntitySingleResponse<HRM_NhanVien>();

            if (IdNhanVien.IsEmpty())
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id truyền nên");
                return result;
            }

            var model = db.HRM_NhanVien.FirstOrDefault(x => x.IdGuid == IdNhanVien);
            if (model == null)
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Không tìm thấy dữ liệu nhân viên với Id {IdNhanVien}");
                return result;
            }
            var HopDong = db.HRM_HopDong.Where(r => r.IdNhanVien == IdNhanVien).OrderByDescending(r => r.Created).FirstOrDefault();
            if (HopDong != null)
            {
                var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
                if (BoPhan != null)
                    HopDong.TenBoPhanEOS = BoPhan.TenBoPhan;
                model.HopDong = HopDong;
            }
            model.listIdCaLamViec = new List<string>();
            if (!string.IsNullOrEmpty(model.IdCaLamViec))
            {
                if (model.IdCaLamViec.Contains(","))
                    model.listIdCaLamViec = model.IdCaLamViec.Split(',').ToList();
                else
                    model.listIdCaLamViec.Add(model.IdCaLamViec);
            }
            var toDictDMQuocTich = db.HRM_dmQuocTich.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMDanToc = db.HRM_dmDanToc.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMQuanHeGiaDinh = db.HRM_dmQuanHeGiaDinh.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMLoaiNhanVien = db.HRM_dmLoaiNhanVien.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            //   var toDictDMBoPhan = db.AsNoTracking().ToList().ToDict(x => x.Id);
            var toDictDMTrinhDoDaoTao = db.HRM_dmTrinhDoDaoTao.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMNgonNgu = db.HRM_dmNgonNgu.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMNhomChungChi = db.HRM_dmNhomChungChi.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMXepLoaiNgonNgu = db.HRM_dmXepLoai.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            if (model.NgayBatDauChinhThuc != null)
            {
                model.NgayBatDauChinhThucUnix = DateTimeToUnixTimeStamp(model.NgayBatDauChinhThuc);

            }
            var BaoHiemNgoai = db.HRM_BaoHiem_HopDong.FirstOrDefault(x => x.IdNhanVien == model.IdGuid && string.IsNullOrEmpty(x.IdHopDong));
            if (BaoHiemNgoai != null)
            {
                BaoHiemNgoai.NgayBatDauDongBHXHUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDauDongBHXH);
                BaoHiemNgoai.NgayBatDauUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDau);
                BaoHiemNgoai.NgayCapSoBaoHiemUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayCapSoBaoHiem);
                BaoHiemNgoai.NgayKetThucUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
                model.BaoHiemNgoai = BaoHiemNgoai;
            }
            var BaoHiemTaiNan = db.HRM_NhanVien_BaoHiemTaiNan.FirstOrDefault(x => x.IdNhanVien == model.IdGuid);
            if (BaoHiemTaiNan != null)
            {
                BaoHiemTaiNan.NgayBatDauUnix = DateTimeToUnixTimeStamp(BaoHiemTaiNan.NgayBatDau);
                BaoHiemTaiNan.NgayKetThucUnix = DateTimeToUnixTimeStamp(BaoHiemTaiNan.NgayKetThuc);
                model.BaoHiemTaiNan = BaoHiemTaiNan;
            }

            var listKhamSK = db.HRM_NhanVien_KhamSucKhoe.Where(r => r.IdNhanVien == model.IdGuid).ToList();
            if (listKhamSK != null && listKhamSK.Count > 0)
            {
                foreach (var item in listKhamSK)
                {
                    if (item.NgayKetThuc != null)
                        item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                    if (item.NgayKham != null)
                        item.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKham);

                }
                model.listKhamSucKhoe = listKhamSK;
            }
            model.NgayNghiViecUnix = DateTimeToUnixTimeStamp(model.NgayNghiViec);
            model.NgayBatDauLamViecUnix = DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
            if (model.NgayBatDauLamViec != null)
            {
                DateTime time = DateTime.Now;
                if (model.NgayBatDauLamViec.Value.Year != time.Year)
                {
                    int Nam = (int)((time - model.NgayBatDauLamViec.Value).TotalDays / 365) / 5;
                    model.SoNgayNghiPhepThamNien = Nam;
                }
            }
            model.ThangTruyThuBHYTUnix = DateTimeToUnixTimeStamp(model.ThangTruyThuBHYT);
            model.ThangTruyThuBHXHUnix = DateTimeToUnixTimeStamp(model.ThangTruyThuBHXH);
            model.NgayKetThucThuViecUnix = DateTimeToUnixTimeStamp(model.NgayKetThucThuViec);
            model.NgayBatDauThuViecUnix = DateTimeToUnixTimeStamp(model.NgayBatDauThuViec);
            model.NgayCapMaSTCNUnix = DateTimeToUnixTimeStamp(model.NgayCapMaSTCN);
            model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);
            if (model.NgaySinh != null)
            {
                DateTime time = DateTime.Now;
                try
                {
                    var Ages = CalculateAge(model.NgaySinh, time);
                    model.Tuoi = Ages.Years + " tuổi " + Ages.Months + " tháng " + Ages.Days + " ngày";
                }
                catch
                {

                }
                //double Day = (time - model.NgaySinh).TotalDays;
                //int Tuoi = (int)(Day / 365);
                //int Thang = (int)((Day - Tuoi * 365) / 30.5);
                //int Ngay = (int)(Day - Tuoi * 365 - Thang * 30.5);
                //model.Tuoi = Tuoi + " tuổi " + Thang + " tháng " + Ngay + " ngày";
            }
            var anhDaiDien = FileUploadController.GetListFileDinhKem(db, model.IdGuid, DefineData.eFlagDinhKem.AnhDaiDien.ToString());
            //db.HRM_FileDinhKem.AsNoTracking().FirstOrDefault(x => x.idItem == model.IdGuid && x.TableName == );
            if (anhDaiDien != null)
            {
                model.AnhDaiDien = anhDaiDien.FirstOrDefault();
            }
            if (model.TenTrinhDo == null)
                model.TenTrinhDo = db.HRM_dmTrinhDoDaoTao.Where(r => r.IdGuid == model.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
            var listTepDinhKem = FileUploadController.GetListFileDinhKem(db, model.IdGuid, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            //db.HRM_FileDinhKem.AsNoTracking().Where(x => x.idItem == model.IdGuid && x.TableName == nameof(HRM_NhanVien)
            //                                                                    && x.FlagDinhKem == DefineData.eFlagDinhKem.FileDinhKem.ToString()).ToList();
            if (listTepDinhKem.IsNotEmpty())
            {
                model.lstFileDinhKem = listTepDinhKem;
            }
            var fileDinhkemHoSo = db.HRM_NhanVien_FileDinhKemHoSo.Where(r => r.IdNhanVien == IdNhanVien).OrderBy(a => a.Created).ToList();
            var filesHoSo = FileUploadController.GetListFileDinhKemByIds(db, fileDinhkemHoSo.Select(x => x.IdGuid), nameof(HRM_NhanVien_FileDinhKemHoSo));
            foreach (var item in fileDinhkemHoSo)
            {
                if (item.NgayCap != null)
                    item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                item.lstFileDinhKem = filesHoSo[item.IdGuid].ToList();
            }
            model.listFileDinhKemHoSo = new List<HRM_NhanVien_FileDinhKemHoSo>();
            model.listFileDinhKemHoSo = fileDinhkemHoSo.Where(a => a.NgayCap != null).OrderByDescending(a => a.NgayCap).ToList(); ;
            model.listFileDinhKemHoSo.AddRange(fileDinhkemHoSo.Where(a => a.NgayCap == null).OrderByDescending(a => a.NgayCap).ToList());
            model.listIdCongTy = db.HRM_QuyTrinhBoPhan_BoPhan.AsNoTracking().Where(a => a.IdQuyTrinh == model.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
            if ((model.listIdCongTy == null || model.listIdCongTy.Count == 0) && model.IdCongTyEOS > 0)
            {
                model.listIdCongTy = new List<long>();
                model.listIdCongTy.Add(model.IdCongTyEOS);
            }
            model.QuocTich = model.IdQuocTich.GetValueFromDic(toDictDMQuocTich);
            model.itemTinhTrangHonNhan = model.IdTinhTrangHonNhan.GetValueFromDic(toDictDMTinhTrangHonNhan);
            model.itemDanToc = model.IdDanToc.GetValueFromDic(toDictDMDanToc);
            model.itemLienHeKhanQuanHe = model.LienHeKhanIdQuanHe.GetValueFromDic(toDictDMQuanHeGiaDinh);
            model.itemLoaiNhanVien = model.IdLoaiNhanVien.GetValueFromDic(toDictDMLoaiNhanVien);
            model.TenQuocTich = model.IdQuocTich.GetValueFromDic(toDictDMQuocTich, x => x.Ten);


            model.listEmail = db.HRM_NhanVien_Email.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            var listQuanHeGiaDinh = db.HRM_NhanVienQuanHeGiaDinh.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listQuanHeGiaDinh.IsNotEmpty())
            {
                var filesQuanHe = FileUploadController.GetListFileDinhKemByIds(db, listQuanHeGiaDinh.Select(x => x.IdGuid), nameof(HRM_NhanVienQuanHeGiaDinh));
                foreach (var item in listQuanHeGiaDinh)
                {
                    if (item.NgaySinh != null)
                        item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                    item.itemQuanHe = item.IdQuanHe.GetValueFromDic(toDictDMQuanHeGiaDinh);
                    item.lstFileDinhKem = filesQuanHe[item.IdGuid].ToList();
                }
            }
            model.ListQuanHeGiaDinhNhanVien = listQuanHeGiaDinh;
            var listGiayToTuyThan = db.HRM_NhanVien_GiayToTuyThan.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).OrderByDescending(a => a.NgayCap).ToList();
            if (listGiayToTuyThan.IsNotEmpty())
            {
                var listNoiCap = db.HRM_dmNoiCap.AsNoTracking().ToList();
                foreach (var item in listGiayToTuyThan)
                {
                    var noiCap = listNoiCap.FirstOrDefault(a => a.IdGuid == item.NoiCap);
                    if (noiCap != null)
                        item.TenNoiCap = noiCap.Ten;
                    if (item.NgayCap != null)
                        item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                    if (item.NgayHetHan != null)
                        item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
                }
            }
            model.listGiayToTuyThan = listGiayToTuyThan;

            var listThamGiaBaoHiemXaHoi = db.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).OrderByDescending(a => a.TuNgay).ToList();
            if (listThamGiaBaoHiemXaHoi.IsNotEmpty())
            {
                DateTime dt = DateTime.Today;
                foreach (var item in listThamGiaBaoHiemXaHoi)
                {
                    if (item.TuNgay != null)
                        item.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.TuNgay);
                    if (item.DenNgay != null && item.DenNgay.Year > 1970)
                        item.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.DenNgay);
                    if (item.DenNgay.Year <= 1970)
                        item.DenNgay = dt;
                    double Day = (item.DenNgay - item.TuNgay).TotalDays + 1;
                    item.Thang = ((item.DenNgay.Year - item.TuNgay.Year) * 12) + item.DenNgay.Month - item.TuNgay.Month + 1;

                }
                int TThang = listThamGiaBaoHiemXaHoi.Where(a => a.isThamGia == true).Sum(a => a.Thang);
                if (TThang < 12)
                    model.ThoiGianThamGia = TThang + " Tháng";
                else
                {
                    int Nam = (int)(TThang / 12);
                    model.ThoiGianThamGia = Nam + " năm " + (TThang - (Nam * 12)) + " tháng";
                }
            }
            model.listThamGiaBaoHiemXaHoi = listThamGiaBaoHiemXaHoi;

            var listKhamSucKhoe = db.HRM_NhanVien_KhamSucKhoe.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).OrderByDescending(a => a.NgayKham).ToList();
            if (listKhamSucKhoe.IsNotEmpty())
            {
                foreach (var item in listKhamSucKhoe)
                {
                    if (item.NgayKham != null)
                        item.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKham);
                    if (item.NgayKetThuc != null)
                        item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);

                }

            }
            model.listKhamSucKhoe = listKhamSucKhoe;

            var listBangCap = db.HRM_NhanVienBangCap.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).OrderByDescending(a => a.NgayTotNghiep).ToList();
            if (listBangCap.IsNotEmpty())
            {
                var filesBangCap = FileUploadController.GetListFileDinhKemByIds(db, listBangCap.Select(x => x.IdGuid), nameof(HRM_NhanVienBangCap));
                foreach (var item in listBangCap)
                {
                    item.itemTrinhDoDaoTao = item.IdTrinhDoDaoTao.GetValueFromDic(toDictDMTrinhDoDaoTao);
                    if (item.NgayTotNghiep != null)
                        item.NgayTotNghiepUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTotNghiep);
                    item.lstFileDinhKem = filesBangCap[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienBangCap = listBangCap;

            var listNgonNgu = db.HRM_NgonNguNhanVien.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listNgonNgu.IsNotEmpty())
            {
                var filesNgonNgu = FileUploadController.GetListFileDinhKemByIds(db, listNgonNgu.Select(x => x.IdGuid), nameof(HRM_NgonNguNhanVien));
                foreach (var item in listNgonNgu)
                {
                    item.NgonNgu = item.IdNgonNgu.GetValueFromDic(toDictDMNgonNgu);
                    item.itemXepLoaiNghe = item.IdXepLoaiNghe.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.itemXepLoaiNoi = item.IdXepLoaiNoi.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.itemXepLoaiDoc = item.IdXepLoaiDoc.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.itemXepLoaiViet = item.IdXepLoaiViet.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.lstFileDinhKem = filesNgonNgu[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienNgonNgu = listNgonNgu;

            var listChungChi = db.HRM_ChungChiNhanVien.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).OrderByDescending(a => a.NgayCap).ToList();
            if (listChungChi.IsNotEmpty())
            {
                var filesChungChi = FileUploadController.GetListFileDinhKemByIds(db, listChungChi.Select(x => x.IdGuid), nameof(HRM_ChungChiNhanVien));
                foreach (var item in listChungChi)
                {
                    item.NhomChungChi = item.IdNhomChungChi.GetValueFromDic(toDictDMNhomChungChi);
                    item.DateTimeToUnix();
                    item.lstFileDinhKem = filesChungChi[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienChungChi = listChungChi;

            var listCongTac = db.HRM_QuaTrinhCongTac.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listCongTac.IsNotEmpty())
            {
                var filesCongTac = FileUploadController.GetListFileDinhKemByIds(db, listCongTac.Select(x => x.IdGuid), nameof(HRM_QuaTrinhCongTac));
                foreach (var item in listCongTac)
                {
                    item.lstFileDinhKem = filesCongTac[item.IdGuid].ToList();
                }
            }
            model.ListQuaTrinhCongTac = listCongTac;
            model.ListQuaTrinhCongTacNgoaiCongTy = new List<HRM_QuaTrinhCongTacNgoaiCongTy>();
            var listCongTacNuocNgoai = db.HRM_QuaTrinhCongTacNgoaiCongTy.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listCongTacNuocNgoai.IsNotEmpty())
            {
                var filesCongTacNgoai = FileUploadController.GetListFileDinhKemByIds(db, listCongTacNuocNgoai.Select(x => x.IdGuid), nameof(HRM_QuaTrinhCongTacNgoaiCongTy));
                foreach (var item in listCongTacNuocNgoai)
                {
                    item.lstFileDinhKem = filesCongTacNgoai[item.IdGuid].ToList();
                }
            }
            model.ListQuaTrinhCongTacNgoaiCongTy = listCongTacNuocNgoai;


            var listNganHang = db.HRM_NhanVien_TaiKhoanNganHang.AsNoTracking().Where(x => x.idNhanVien == model.IdGuid).ToList();
            if (listNganHang.IsNotEmpty())
            {
            }
            model.ListNganHang = listNganHang;
            model.listSoDienThoai = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == model.IdGuid).ToList();
            var listPhuCap = db.HRM_NhanVien_PhuCap.Where(a => a.IdNhanVien == model.IdGuid).ToList();
            var listdmPhuCap = db.HRM_dmPhuCap.ToList();
            model.listPhuCap = new List<HRM_NhanVien_PhuCap>();
            if (listPhuCap != null && listPhuCap.Count > 0)
            {
                model.listPhuCap = listPhuCap.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.Created).ToList();
                if (model.listPhuCap != null & model.listPhuCap.Count > 0)
                {

                    foreach (var pc in model.listPhuCap)
                    {
                        var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                        if (Phucap != null)
                            pc.TenPhuCap = Phucap.Ten;
                    }
                }
            }
            result.Item = model;
            return result;
        }
        public EntitySingleResponse<HRM_NhanVien> GetThongTinNhanVienByIdUser(string IdUser, ApplicationDbContext db)
        {
            var result = new EntitySingleResponse<HRM_NhanVien>();

            if (IdUser.IsEmpty())
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id truyền nên");
                return result;
            }

            var model = db.HRM_NhanVien.FirstOrDefault(x => x.IdUser == IdUser);
            if (model == null)
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Không tìm thấy dữ liệu nhân viên với Id {IdUser}");
                return result;
            }
            var checkTTTT = db.HRM_NhanVienDieuChinhThongTin.FirstOrDefault(a => a.IdNhanVien == model.IdGuid && a.TrangThai == DefineData.eTrangThaiThayDoiThongTin.ChoDuyet.ToString());
            if (checkTTTT != null)
                model.isChoPheDuyetThayDoiThongTin = true;
            string IdNhanVien = model.IdGuid;
            //var HopDong = db.HRM_HopDong.Where(r => r.IdNhanVien == IdNhanVien).OrderByDescending(r => r.Created).FirstOrDefault();
            //if (HopDong != null)
            //{
            //    var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            //    if (BoPhan != null)
            //        HopDong.TenBoPhanEOS = BoPhan.TenBoPhan;
            //    model.HopDong = HopDong;
            //}
            model.listEmail = db.HRM_NhanVien_Email.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            model.listIdCongTy = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == model.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
            if ((model.listIdCongTy == null || model.listIdCongTy.Count > 0) && model.IdCongTyEOS > 0)
            {
                model.listIdCongTy = new List<long>();
                model.listIdCongTy.Add(model.IdCongTyEOS);
            }
            var toDictDMQuocTich = db.HRM_dmQuocTich.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMDanToc = db.HRM_dmDanToc.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMQuanHeGiaDinh = db.HRM_dmQuanHeGiaDinh.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMLoaiNhanVien = db.HRM_dmLoaiNhanVien.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            //   var toDictDMBoPhan = db.AsNoTracking().ToList().ToDict(x => x.Id);
            var toDictDMTrinhDoDaoTao = db.HRM_dmTrinhDoDaoTao.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMNgonNgu = db.HRM_dmNgonNgu.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMNhomChungChi = db.HRM_dmNhomChungChi.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictDMXepLoaiNgonNgu = db.HRM_dmXepLoai.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            if (model.NgayBatDauChinhThucUnix > 0)
                model.NgayBatDauChinhThuc = UnixTimeStampToDateTime(model.NgayBatDauChinhThucUnix);
            model.NgayNghiViecUnix = DateTimeToUnixTimeStamp(model.NgayNghiViec);
            model.NgayBatDauLamViecUnix = DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
            model.ThangTruyThuBHYTUnix = DateTimeToUnixTimeStamp(model.ThangTruyThuBHYT);
            model.ThangTruyThuBHXHUnix = DateTimeToUnixTimeStamp(model.ThangTruyThuBHXH);
            model.NgayKetThucThuViecUnix = DateTimeToUnixTimeStamp(model.NgayKetThucThuViec);
            model.NgayBatDauThuViecUnix = DateTimeToUnixTimeStamp(model.NgayBatDauThuViec);
            model.NgayCapMaSTCNUnix = DateTimeToUnixTimeStamp(model.NgayCapMaSTCN);
            model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);
            if (model.NgaySinh != null)
            {
                DateTime time = DateTime.Now;
                double Day = (time - model.NgaySinh).TotalDays;
                int Tuoi = (int)(Day / 365);
                int Thang = (int)((Day - Tuoi * 365) / 30.5);
                int Ngay = (int)(Day - Tuoi * 365 - Thang * 30.5);
                model.Tuoi = Tuoi + " tuổi " + Thang + " tháng " + Ngay + " ngày";
            }
            var anhDaiDien = FileUploadController.GetListFileDinhKem(db, model.IdGuid, DefineData.eFlagDinhKem.AnhDaiDien.ToString());
            if (anhDaiDien != null)
            {
                model.AnhDaiDien = anhDaiDien.FirstOrDefault();
            }
            if (model.TenTrinhDo == null)
                model.TenTrinhDo = db.HRM_dmTrinhDoDaoTao.Where(r => r.IdGuid == model.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
            var listTepDinhKem = FileUploadController.GetListFileDinhKem(db, model.IdGuid, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            var listKhamSK = db.HRM_NhanVien_KhamSucKhoe.Where(r => r.IdNhanVien == model.IdGuid).ToList();
            if (listKhamSK != null && listKhamSK.Count > 0)
            {
                foreach (var item in listKhamSK)
                {
                    if (item.NgayKetThuc != null)
                        item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                    if (item.NgayKham != null)
                        item.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKham);

                }
                model.listKhamSucKhoe = listKhamSK;
            }
            if (listTepDinhKem.IsNotEmpty())
            {
                model.lstFileDinhKem = listTepDinhKem;
            }
            var fileDinhkemHoSo = db.HRM_NhanVien_FileDinhKemHoSo.Where(r => r.IdNhanVien == IdNhanVien).ToList();
            var filesHoSo2 = FileUploadController.GetListFileDinhKemByIds(db, fileDinhkemHoSo.Select(x => x.IdGuid), nameof(HRM_NhanVien_FileDinhKemHoSo));
            foreach (var item in fileDinhkemHoSo)
            {
                if (item.NgayCap != null)
                    item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                item.lstFileDinhKem = filesHoSo2[item.IdGuid].ToList();
            }
            model.listFileDinhKemHoSo = fileDinhkemHoSo;

            model.QuocTich = model.IdQuocTich.GetValueFromDic(toDictDMQuocTich);
            model.itemTinhTrangHonNhan = model.IdTinhTrangHonNhan.GetValueFromDic(toDictDMTinhTrangHonNhan);
            model.itemDanToc = model.IdDanToc.GetValueFromDic(toDictDMDanToc);
            model.itemLienHeKhanQuanHe = model.LienHeKhanIdQuanHe.GetValueFromDic(toDictDMQuanHeGiaDinh);
            model.itemLoaiNhanVien = model.IdLoaiNhanVien.GetValueFromDic(toDictDMLoaiNhanVien);
            model.TenQuocTich = model.IdQuocTich.GetValueFromDic(toDictDMQuocTich, x => x.Ten);


            var listQuanHeGiaDinh = db.HRM_NhanVienQuanHeGiaDinh.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listQuanHeGiaDinh.IsNotEmpty())
            {
                foreach (var item in listQuanHeGiaDinh)
                {

                    if (item.NgaySinh != null)
                        item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);


                    item.itemQuanHe = item.IdQuanHe.GetValueFromDic(toDictDMQuanHeGiaDinh);
                }
            }
            model.ListQuanHeGiaDinhNhanVien = listQuanHeGiaDinh;
            var listGiayToTuyThan = db.HRM_NhanVien_GiayToTuyThan.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listGiayToTuyThan.IsNotEmpty())
            {
                var listNoiCap = db.HRM_dmNoiCap.AsNoTracking().ToList();
                foreach (var item in listGiayToTuyThan)
                {
                    var noiCap = listNoiCap.FirstOrDefault(a => a.IdGuid == item.NoiCap);
                    if (noiCap != null)
                        item.TenNoiCap = noiCap.Ten;
                    if (item.NgayCap != null)
                        item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                    if (item.NgayHetHan != null)
                        item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
                }
            }
            model.listGiayToTuyThan = listGiayToTuyThan;

            var listBangCap = db.HRM_NhanVienBangCap.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listBangCap.IsNotEmpty())
            {
                var filesBangCap2 = FileUploadController.GetListFileDinhKemByIds(db, listBangCap.Select(x => x.IdGuid), nameof(HRM_NhanVienBangCap));
                foreach (var item in listBangCap)
                {
                    item.itemTrinhDoDaoTao = item.IdTrinhDoDaoTao.GetValueFromDic(toDictDMTrinhDoDaoTao);
                    if (item.NgayTotNghiep != null)
                        item.NgayTotNghiepUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTotNghiep);
                    item.lstFileDinhKem = filesBangCap2[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienBangCap = listBangCap;

            var listNgonNgu = db.HRM_NgonNguNhanVien.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listNgonNgu.IsNotEmpty())
            {
                var filesNgonNgu2 = FileUploadController.GetListFileDinhKemByIds(db, listNgonNgu.Select(x => x.IdGuid), nameof(HRM_NgonNguNhanVien));
                foreach (var item in listNgonNgu)
                {
                    item.NgonNgu = item.IdNgonNgu.GetValueFromDic(toDictDMNgonNgu);
                    item.itemXepLoaiNghe = item.IdXepLoaiNghe.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.itemXepLoaiNoi = item.IdXepLoaiNoi.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.itemXepLoaiDoc = item.IdXepLoaiDoc.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.itemXepLoaiViet = item.IdXepLoaiViet.GetValueFromDic(toDictDMXepLoaiNgonNgu);
                    item.lstFileDinhKem = filesNgonNgu2[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienNgonNgu = listNgonNgu;

            var listChungChi = db.HRM_ChungChiNhanVien.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listChungChi.IsNotEmpty())
            {
                var filesChungChi2 = FileUploadController.GetListFileDinhKemByIds(db, listChungChi.Select(x => x.IdGuid), nameof(HRM_ChungChiNhanVien));
                foreach (var item in listChungChi)
                {
                    item.NhomChungChi = item.IdNhomChungChi.GetValueFromDic(toDictDMNhomChungChi);
                    item.DateTimeToUnix();
                    item.lstFileDinhKem = filesChungChi2[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienChungChi = listChungChi;

            var listCongTac = db.HRM_QuaTrinhCongTac.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listCongTac.IsNotEmpty())
            {
                var filesCongTac2 = FileUploadController.GetListFileDinhKemByIds(db, listCongTac.Select(x => x.IdGuid), nameof(HRM_QuaTrinhCongTac));
                foreach (var item in listCongTac)
                {
                    item.lstFileDinhKem = filesCongTac2[item.IdGuid].ToList();
                }
            }
            model.ListQuaTrinhCongTac = listCongTac;
            model.ListQuaTrinhCongTacNgoaiCongTy = new List<HRM_QuaTrinhCongTacNgoaiCongTy>();
            var listCongTacNuocNgoai = db.HRM_QuaTrinhCongTacNgoaiCongTy.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listCongTacNuocNgoai.IsNotEmpty())
            {
                var filesCongTacNgoai2 = FileUploadController.GetListFileDinhKemByIds(db, listCongTacNuocNgoai.Select(x => x.IdGuid), nameof(HRM_QuaTrinhCongTacNgoaiCongTy));
                foreach (var item in listCongTacNuocNgoai)
                {
                    item.lstFileDinhKem = filesCongTacNgoai2[item.IdGuid].ToList();
                }
            }
            model.ListQuaTrinhCongTacNgoaiCongTy = listCongTacNuocNgoai;


            var listNganHang = db.HRM_NhanVien_TaiKhoanNganHang.AsNoTracking().Where(x => x.idNhanVien == model.IdGuid).ToList();
            if (listNganHang.IsNotEmpty())
            {
            }
            model.ListNganHang = listNganHang;
            model.listSoDienThoai = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == model.IdGuid).ToList();
            var listPhuCap = db.HRM_NhanVien_PhuCap.Where(a => a.IdNhanVien == model.IdGuid).ToList();
            var listdmPhuCap = db.HRM_dmPhuCap.ToList();
            model.listPhuCap = new List<HRM_NhanVien_PhuCap>();
            if (listPhuCap != null && listPhuCap.Count > 0)
            {
                model.listPhuCap = listPhuCap.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.Created).ToList();
                if (model.listPhuCap != null & model.listPhuCap.Count > 0)
                {

                    foreach (var pc in model.listPhuCap)
                    {
                        var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                        if (Phucap != null)
                            pc.TenPhuCap = Phucap.Ten;
                    }
                }
            }
            result.Item = model;
            return result;
        }
        public dynamic DeleteNhanVienById(string IdNhanVien, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    var checkHopDong = db.HRM_HopDong.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    if (checkHopDong.Count > 0)
                    {
                        List<string> listErr = checkHopDong.Select(r => r.SoHopDong).ToList();


                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, $"Nhân viên đã có những hợp đồng : {listErr.Join(",")}");
                    }
                    HRM_NhanVien nhanVien = db.HRM_NhanVien.Find(IdNhanVien);
                    if (nhanVien == null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên không tồn tại hoặc bị xóa trước đó");
                    }

                    var bangCap = db.HRM_NhanVienBangCap.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var quanheGiadinh = db.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var ngonNgu = db.HRM_NgonNguNhanVien.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var CongTac = db.HRM_QuaTrinhCongTac.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var CongTacNgoaiCty = db.HRM_QuaTrinhCongTacNgoaiCongTy.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var chucChi = db.HRM_ChungChiNhanVien.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var HRM_NhanVien_KhamSucKhoe = db.HRM_NhanVien_KhamSucKhoe.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    db.HRM_NhanVien_KhamSucKhoe.RemoveRange(HRM_NhanVien_KhamSucKhoe);
                    db.HRM_NhanVienBangCap.RemoveRange(bangCap);
                    db.HRM_NhanVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                    db.HRM_NgonNguNhanVien.RemoveRange(ngonNgu);
                    db.HRM_ChungChiNhanVien.RemoveRange(chucChi);
                    db.HRM_QuaTrinhCongTacNgoaiCongTy.RemoveRange(CongTacNgoaiCty);
                    db.HRM_QuaTrinhCongTac.RemoveRange(CongTac);
                    db.HRM_NhanVien.Remove(nhanVien);
                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xóa giữ liệu");
                }
            }
        }
        public dynamic DeleteNhanVienBylistId(List<string> IdNhanVien, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {
                    var checkHopDong = db.HRM_HopDong.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    if (checkHopDong.Count > 0)
                    {
                        List<string> listErr = checkHopDong.Select(r => r.SoHopDong).ToList();


                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, $"Nhân viên đã có những hợp đồng : {listErr.Join(",")}");
                    }
                    var nhanVien = db.HRM_NhanVien.Where(r => IdNhanVien.Contains(r.IdGuid)).ToList();
                    if (nhanVien == null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên không tồn tại hoặc bị xóa trước đó");
                    }

                    var bangCap = db.HRM_NhanVienBangCap.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    var quanheGiadinh = db.HRM_NhanVienQuanHeGiaDinh.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    var ngonNgu = db.HRM_NgonNguNhanVien.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    var CongTac = db.HRM_QuaTrinhCongTac.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    var CongTacNgoaiCty = db.HRM_QuaTrinhCongTacNgoaiCongTy.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    var chucChi = db.HRM_ChungChiNhanVien.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    var HRM_NhanVien_KhamSucKhoe = db.HRM_NhanVien_KhamSucKhoe.Where(r => IdNhanVien.Contains(r.IdNhanVien)).ToList();
                    db.HRM_NhanVien_KhamSucKhoe.RemoveRange(HRM_NhanVien_KhamSucKhoe);
                    db.HRM_NhanVienBangCap.RemoveRange(bangCap);
                    db.HRM_NhanVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                    db.HRM_NgonNguNhanVien.RemoveRange(ngonNgu);
                    db.HRM_ChungChiNhanVien.RemoveRange(chucChi);
                    db.HRM_QuaTrinhCongTacNgoaiCongTy.RemoveRange(CongTacNgoaiCty);
                    db.HRM_QuaTrinhCongTac.RemoveRange(CongTac);
                    db.HRM_NhanVien.RemoveRange(nhanVien);
                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xóa giữ liệu");
                }
            }
        }
        public dynamic GetListNhanVienBylistId(List<string> IdNhanVien, string KeyWord, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                List<HRM_NhanVien> nhanVien = new List<HRM_NhanVien>();
                try
                {
                    nhanVien = db.HRM_NhanVien.Where(r => IdNhanVien.Contains(r.IdUser)).ToList();
                    if (!string.IsNullOrEmpty(KeyWord))
                        nhanVien = nhanVien.Where(a => a.MaNhanVien.Trim().ToLower().Contains(KeyWord.Trim().ToLower()) || a.TenDayDu.Trim().ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
                    ts.Commit();
                    return nhanVien;
                }
                catch (Exception)
                {
                    ts.Rollback();
                    return nhanVien;
                }
            }
        }
        public MessageReturn DeleteNhanVien(List<string> aListId, ApplicationDbContext _dbContext = null)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa nhân viên thành công");
            if (_dbContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            //int QuyenView = GetQuyenXoa(DefineData.eAction.THONGTINNHANVIEN.ToString(), _dbContext.currentUser.Id, aSmart);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //{
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //}

            if (aListId.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Thiếu dữ liệu để xóa");
            }

            var listNhanVien = _dbContext.HRM_NhanVien.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listNhanVien.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }
            var listErrStatus = new List<string>();
            var listErr = new List<string>();
            foreach (var item in listNhanVien)
            {
                if (item.TrangThai != "DaNghiViec")
                {
                    listErrStatus.Add(item.TenDayDu);
                }
                if (item.CreatedBy != _dbContext.currentUser.Id)
                {
                    listErr.Add(item.TenDayDu);
                }
                //if (QuyenView == (int)DefineData.eGioiHan.OWNER && item.CreatedBy != _dbContext.currentUser.Id)
                //{
                //    listErr.Add(item.TenDayDu);
                //}
                _dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, $"Bạn không có quyền xóa thông tin nhân viên : {listErr.Join(",")}");
            }
            if (listErrStatus.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, $"Nhân viên này không đang trong trạng thái đã nghỉ việc nên không được xóa : {listErrStatus.Join(",")}");
            }

            _dbContext.SaveChanges();

            return result;
        }
        public MessageReturn UploadAnhDaiDien(List<HRM_FileDinhKem> listFile, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);

            List<string> listMaNhanVien = listFile.Select(a => a.FileName.Split('.')[0]).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listMaNhanVien.Contains(a.SoGen)).ToList();
            List<string> listMaCheck = listMaNhanVien;
            if (listMaNhanVien.Count != listNhanVien.Count)
            {
                var list = listNhanVien.Select(a => a.SoGen).Distinct().ToList();
                listMaCheck = listMaNhanVien.Where(a => !list.Contains(a)).ToList();
            }
            if ((listNhanVien == null || listNhanVien.Count == 0) && listMaCheck.Count > 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số gen không tồn tại : " + string.Join(",", listMaCheck));
            }
            var listIdNhanVien = listNhanVien.Select(a => a.IdGuid).ToList();
            var listFileDinhKemOld = db.HRM_FileDinhKem.Where(a => listIdNhanVien.Contains(a.idItem) && a.TableName == DefineData.eFlagDinhKem.AnhDaiDien.ToString()).ToList();
            if (listFileDinhKemOld != null && listFileDinhKemOld.Count > 0)
                db.HRM_FileDinhKem.RemoveRange(listFileDinhKemOld);
            foreach (var item in listNhanVien)
            {
                var listAnhDaiDien = listFile.Where(a => a.FileName.Split('.')[0] == item.SoGen).ToList();
                FileUploadController.SetFileDinhKem(db, listAnhDaiDien, DefineData.eFlagDinhKem.AnhDaiDien.ToString(), item.IdGuid);
            }
            foreach (var item in listFile)
            {
                var NhanVien = listNhanVien.FirstOrDefault(a => item.FileName.Split('.')[0] == a.SoGen);
                if (string.IsNullOrEmpty(NhanVien.IdUser))
                    continue;
                item.IdUser = NhanVien.IdUser;
                item.Name = item.FileNameGUI;
                item.NameLocal = item.FileName;

            }
            _CallWebService.UpdateAnhDaiDienByMultiUser(db, listFile);
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhập ảnh đại diện thành công"); ;
        }
        public MessageReturn UploadCCCD(List<HRM_FileDinhKem> listFile, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);

            List<string> listMaNhanVien = new List<string>();
            foreach (var item in listFile)
            {
                if (item.FileName.Contains("_"))
                    item.Name = item.FileName.Split('_')[0];
                else
                    item.Name = item.FileName;
                listMaNhanVien.Add(item.Name);
            }
            //listFile.Select(a => a.FileName.Split('.')[0]).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listMaNhanVien.Contains(a.MaNhanVien)).ToList();
            List<string> listMaCheck = listMaNhanVien;
            if (listMaNhanVien.Count != listNhanVien.Count)
            {
                var list = listNhanVien.Select(a => a.MaNhanVien).Distinct().ToList();
                listMaCheck = listMaNhanVien.Where(a => !list.Contains(a)).ToList();
            }
            if ((listNhanVien == null || listNhanVien.Count == 0) && listMaCheck.Count > 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã nhân viên không tồn tại : " + string.Join(",", listMaCheck));
            }
            //var listIdNhanVien = listNhanVien.Select(a => a.IdGuid).ToList();
            //var listFileDinhKemOld = db.HRM_FileDinhKem.Where(a => listIdNhanVien.Contains(a.idItem) && a.TableName == DefineData.eFlagDinhKem.AnhDaiDien.ToString()).ToList();
            //if (listFileDinhKemOld != null && listFileDinhKemOld.Count > 0)
            //    db.HRM_FileDinhKem.RemoveRange(listFileDinhKemOld);
            foreach (var item in listNhanVien)
            {
                HRM_NhanVien_FileDinhKemHoSo itemAdd = new HRM_NhanVien_FileDinhKemHoSo();
                var listAnhDaiDien = listFile.Where(a => a.Name == item.MaNhanVien).ToList();
                if (itemAdd.NgayCapUnix > 0)
                    itemAdd.NgayCap = dungchung.UnixTimeStampToDateTime(itemAdd.NgayCapUnix);
                itemAdd.IdNhanVien = item.IdGuid;
                itemAdd.MoTa = "CCCD";
                if (string.IsNullOrEmpty(itemAdd.IdGuid))
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                db.HRM_NhanVien_FileDinhKemHoSo.Add(itemAdd);
                if (listAnhDaiDien != null)
                    FileUploadController.SetFileDinhKem(db, listAnhDaiDien, nameof(HRM_NhanVien_FileDinhKemHoSo).ToString(), itemAdd.IdGuid);
            }
            db.SaveChanges();
            //_CallWebService.UpdateAnhDaiDienByMultiUser(db, listFile);
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhập tài liệu thành công"); ;
        }
        #region Bằng cấp nhân viên

        public List<HRM_NhanVienBangCap> GetDanhSachBangCapNhanVien(string aIdNhanVien, ApplicationDbContext _dbContext)
        {
            var result = new List<HRM_NhanVienBangCap>();

            if (_dbContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            if (aIdNhanVien.IsEmpty())
            {
                return result;
            }

            //var quyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), _dbContext.currentUser.Id, aSMart);
            //if (quyenView == (int)DefineData.eGioiHan.NONE)
            //{
            //    return result;
            //}

            var listBangCap = _dbContext.HRM_NhanVienBangCap.Where(x => x.IdNhanVien == aIdNhanVien).ToList();
            if (listBangCap.IsEmpty())
            {
                return result;
            }

            var listId = listBangCap.Select(x => x.IdGuid).ToList();
            listId = listId.ClearIsEmpty();
            if (listId.IsEmpty())
            {
                return result;
            }

            var toGroupFile = _dbContext.HRM_FileDinhKem.Where(x => listId.Contains(x.idItem)).ToList().ToGroup(x => x.idItem);
            foreach (var item in listBangCap)
            {
                if (toGroupFile.ContainsKey(item.IdGuid))
                {
                    item.lstFileDinhKem = toGroupFile[item.IdGuid];
                }
                result.Add(item);
            }

            return result;
        }
        public EntitySingleResponse<HRM_NhanVienBangCap> GetBangCapNhanVien(string aId, ApplicationDbContext _dbContext)
        {
            var result = new EntitySingleResponse<HRM_NhanVienBangCap>();

            if (_dbContext.currentUser.Id.IsEmpty())
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
                return result;
            }

            if (aId.IsEmpty())
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id truyền nên");
                return result;
            }

            var model = _dbContext.HRM_NhanVienBangCap.FirstOrDefault(x => x.IdGuid == aId);
            if (model == null)
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
                return result;
            }

            model.lstFileDinhKem = _dbContext.HRM_FileDinhKem.Where(x => x.idItem == model.IdGuid).ToList();

            return result;
        }
        public dynamic UpdateBangCapNhanVien(HRM_NhanVienBangCap data, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //if (string.IsNullOrEmpty(data.IdGuid))
            //{
            //    int quyenAdd = GetQuyenAdd(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền thêm mới");
            //}
            //else
            //{
            //    int quyenAdd = GetQuyenEdit(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền sửa");
            //}
            //#endregion


            if (string.IsNullOrEmpty(data.IdGuid))
            {
                db.Entry(data).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
                db.Entry(data).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateBangCapNhanVien HRM data: {0}, Error {1}", JsonConvert.SerializeObject(data), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }


            FileUploadController FileUpload = new FileUploadController();

            if (data.lstFileDinhKem != null && data.lstFileDinhKem.Count > 0)
            {
                FileUploadController.SetFileDinhKem(db, data.lstFileDinhKem, nameof(HRM_NhanVienBangCap).ToString(), data.IdGuid);
                //var lstFileMoi = data.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
                //if (lstFileMoi.Count > 0)
                //    FileUpload.AddFileFromHDFile(data.lstFileDinhKemMoi, data.IdGuid, DefineData.eTable.HRM_BangCap.ToString(), db
                //        , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            }

            //var lstFileCuCanXoa = data.lstFileDinhKemCu.Where(x => x.isDeleted == true).ToList();
            //FileUpload.DeleteFiles(lstFileCuCanXoa, db, new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công"));


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", data.IdGuid);
        }
        public dynamic DeleteBangCapNhanVien(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //int QuyenView = GetQuyenXoa(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền xóa");
            //#endregion

            var lstBangCapNhanVien = db.HRM_NhanVienBangCap.Where(x => lstId.Contains(x.IdGuid)).ToList();
            FileUploadController FileUpload = new FileUploadController();
            foreach (var BangCapNhanVien in lstBangCapNhanVien)
            {
                db.Entry(BangCapNhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteBangCapNhanVien, Error {0}", ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }

            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == DefineData.eTable.HRM_BangCap.ToString()).ToList();
            FileUpload.DeleteFiles(lstFileDinhKem, db, new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công"));


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        #endregion
        #region Ngôn ngữ nhân viên
        public object GetDanhSachNgonNguNhanVien(string IdNhanVien, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion

            var lstBangCap = db.HRM_NgonNguNhanVien.Where(x => x.IdNhanVien == IdNhanVien).ToList();

            var lstId = lstBangCap.Select(x => x.IdGuid);
            string TenBang = DefineData.eTable.HRM_NgonNguNhanVien.ToString();
            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == TenBang).Distinct();


            // danh mục ngôn ngữ
            var lstDanhMucNgonNgu = db.HRM_dmNgonNgu.ToList();
            foreach (var item in lstBangCap)
            {
                item.lstFileDinhKem = lstFileDinhKem.Where(x => x.idItem == item.IdGuid).ToList();
                item.NgonNgu = lstDanhMucNgonNgu.FirstOrDefault(x => x.IdGuid == item.IdNgonNgu);
            }
            return lstBangCap;
        }
        public dynamic GetNgonNguNhanVien(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion

            var HiepHoi = db.HRM_NgonNguNhanVien.FirstOrDefault(x => x.IdGuid == Id);

            if (HiepHoi == null)
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy Bằng cấp nhân viên");
            string sLoai = DefineData.eTable.HRM_NgonNguNhanVien.ToString();
            HiepHoi.lstFileDinhKem = db.HRM_FileDinhKem.Where(x => x.IdGuid == Id && x.TableName == sLoai).ToList();

            HiepHoi.NgonNgu = db.HRM_dmNgonNgu.FirstOrDefault(x => x.IdGuid == HiepHoi.IdNgonNgu);

            return HiepHoi;
        }
        public dynamic UpdateNgonNguNhanVien(HRM_NgonNguNhanVien data, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //if (string.IsNullOrEmpty(data.IdGuid))
            //{
            //    int quyenAdd = GetQuyenAdd(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền thêm mới");
            //}
            //else
            //{
            //    int quyenAdd = GetQuyenEdit(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền sửa");
            //}
            //#endregion

            if (string.IsNullOrEmpty(data.IdGuid))
            {
                db.Entry(data).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
                db.Entry(data).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateNgonNguNhanVien HRM data: {0}, Error {1}", JsonConvert.SerializeObject(data), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }

            FileUploadController FileUpload = new FileUploadController();

            if (data.lstFileDinhKem != null && data.lstFileDinhKem.Count > 0)
            {

                FileUploadController.SetFileDinhKem(db, data.lstFileDinhKem, nameof(HRM_NgonNguNhanVien).ToString(), data.IdGuid);
                //var lstFileMoi = data.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
                //if (lstFileMoi.Count > 0)
                //    FileUpload.AddFileFromHDFile(data.lstFileDinhKemMoi, data.IdGuid, DefineData.eTable.HRM_NgonNguNhanVien.ToString(), db
                //        , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            }

            //var lstFileCuCanXoa = data.lstFileDinhKemCu.Where(x => x.isDeleted == true).ToList();
            //FileUpload.DeleteFiles(lstFileCuCanXoa, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", data.IdGuid);
        }
        public dynamic DeleteNgonNguNhanVien(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //int QuyenView = GetQuyenXoa(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền xóa");
            //#endregion

            var lstBangCapNhanVien = db.HRM_NgonNguNhanVien.Where(x => lstId.Contains(x.IdGuid)).ToList();
            FileUploadController FileUpload = new FileUploadController();
            foreach (var BangCapNhanVien in lstBangCapNhanVien)
            {
                db.Entry(BangCapNhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteNgonNguNhanVien, Error {0}", ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }

            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == DefineData.eTable.HRM_NgonNguNhanVien.ToString()).ToList();
            FileUpload.DeleteFiles(lstFileDinhKem, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        #endregion
        #region Chứng chỉ nhân viên
        public object GetDanhSachChungChiNhanVien(string IdNhanVien, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion

            var lstBangCap = db.HRM_ChungChiNhanVien.Where(x => x.IdNhanVien == IdNhanVien).ToList();

            var lstId = lstBangCap.Select(x => x.IdGuid);
            string TenBang = DefineData.eTable.HRM_ChungChiNhanVien.ToString();
            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == TenBang).Distinct();


            // danh mục nhóm chứng chỉ
            var lstDanhMucNhomChungChi = db.HRM_dmNhomChungChi.ToList();
            foreach (var item in lstBangCap)
            {
                item.lstFileDinhKem = lstFileDinhKem.Where(x => x.idItem == item.IdGuid).ToList();
                item.NhomChungChi = lstDanhMucNhomChungChi.FirstOrDefault(x => x.IdGuid == item.IdNhomChungChi);
                item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
            }
            return lstBangCap;
        }
        public dynamic GetChungChiNhanVien(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion

            var item = db.HRM_ChungChiNhanVien.FirstOrDefault(x => x.IdGuid == Id);

            if (item == null)
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy Bằng cấp nhân viên");
            string sLoai = DefineData.eTable.HRM_ChungChiNhanVien.ToString();
            item.lstFileDinhKem = db.HRM_FileDinhKem.Where(x => x.IdGuid == Id && x.TableName == sLoai).ToList();

            var lstDanhMucNhomChungChi = db.HRM_dmNhomChungChi.ToList();
            item.NhomChungChi = lstDanhMucNhomChungChi.FirstOrDefault(x => x.IdGuid == item.IdNhomChungChi);

            item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
            item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);

            return item;
        }
        public dynamic UpdateChungChiNhanVien(HRM_ChungChiNhanVien item, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //if (string.IsNullOrEmpty(item.IdGuid))
            //{
            //    int quyenAdd = GetQuyenAdd(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền thêm mới");
            //}
            //else
            //{
            //    int quyenAdd = GetQuyenEdit(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền sửa");
            //}
            //#endregion

            item.NgayCap = dungchung.UnixTimeStampToDateTime(item.NgayCapUnix);
            item.NgayHetHan = dungchung.UnixTimeStampToDateTime(item.NgayHetHanUnix);
            if (string.IsNullOrEmpty(item.IdGuid))
            {
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateChungChiNhanVien data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }


            FileUploadController FileUpload = new FileUploadController();

            if (item.lstFileDinhKem != null && item.lstFileDinhKem.Count > 0)
            {
                FileUploadController.SetFileDinhKem(db, item.lstFileDinhKem, nameof(HRM_ChungChiNhanVien).ToString(), item.IdGuid);

                //var lstFileMoi = item.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
                //if (lstFileMoi.Count > 0)
                //    FileUpload.AddFileFromHDFile(item.lstFileDinhKemMoi, item.IdGuid, DefineData.eTable.HRM_ChungChiNhanVien.ToString(), db
                //        , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            }

            //var lstFileCuCanXoa = item.lstFileDinhKemCu.Where(x => x.isDeleted == true).ToList();
            //FileUpload.DeleteFiles(lstFileCuCanXoa, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", item.IdGuid);
        }
        public dynamic DeleteChungChiNhanVien(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenXoa(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền xóa");
            //#endregion


            var lstBangCapNhanVien = db.HRM_ChungChiNhanVien.Where(x => lstId.Contains(x.IdGuid)).ToList();
            FileUploadController FileUpload = new FileUploadController();
            foreach (var BangCapNhanVien in lstBangCapNhanVien)
            {
                db.Entry(BangCapNhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }



            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteChungChiNhanVien, Error {0}", ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }

            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == DefineData.eTable.HRM_ChungChiNhanVien.ToString()).ToList();
            FileUpload.DeleteFiles(lstFileDinhKem, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        #endregion
        #region Quá trình công tác
        public object GetDanhSachQuaTrinhCongTacNhanVien(string IdNhanVien, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion


            var lstQuaTrinhCongTac = db.HRM_QuaTrinhCongTac.Where(x => x.IdNhanVien == IdNhanVien).ToList();

            var lstId = lstQuaTrinhCongTac.Select(x => x.IdGuid);
            string TenBang = DefineData.eTable.HRM_QuaTrinhCongTac.ToString();
            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == TenBang).Distinct();

            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            foreach (var item in lstQuaTrinhCongTac)
            {
                item.lstFileDinhKem = lstFileDinhKem.Where(x => x.idItem == item.IdGuid).ToList();
                //  item.QuocTich = lstQuocTich.FirstOrDefault(x=>x.IdGuid == item.IdQuocTich);
            }
            return lstQuaTrinhCongTac;
        }
        public dynamic GetQuaTrinhCongTacNhanVien(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //    #region Quyền
            //    int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //    #endregion

            var item = db.HRM_QuaTrinhCongTac.FirstOrDefault(x => x.IdGuid == Id);

            if (item == null)
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy Bằng cấp nhân viên");
            string sLoai = DefineData.eTable.HRM_QuaTrinhCongTac.ToString();
            item.lstFileDinhKem = db.HRM_FileDinhKem.Where(x => x.IdGuid == Id && x.TableName == sLoai).ToList();
            //item.QuocTich = db.HRM_QuocTich.FirstOrDefault(x => x.IdGuid == item.IdQuocTich);

            var lstDanhMucNhomChungChi = db.HRM_dmNhomChungChi.ToList();
            return item;
        }
        public dynamic UpdateQuaTrinhCongTacNhanVien(HRM_QuaTrinhCongTac item, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //if (string.IsNullOrEmpty(item.IdGuid))
            //{
            //    int quyenAdd = GetQuyenAdd(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền thêm mới");
            //}
            //else
            //{
            //    int quyenAdd = GetQuyenEdit(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //    if (quyenAdd == (int)DefineData.eGioiHan.NONE)
            //        return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền sửa");
            //}
            //#endregion

            if (string.IsNullOrEmpty(item.IdGuid))
            {
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateQuaTrinhCongTacNhanVien data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }
            FileUploadController FileUpload = new FileUploadController();

            if (item.lstFileDinhKem != null && item.lstFileDinhKem.Count > 0)
            {
                FileUploadController.SetFileDinhKem(db, item.lstFileDinhKem, nameof(HRM_QuaTrinhCongTac).ToString(), item.IdGuid);

                //var lstFileMoi = item.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
                //if (lstFileMoi.Count > 0)
                //    FileUpload.AddFileFromHDFile(item.lstFileDinhKemMoi, item.IdGuid, DefineData.eTable.HRM_QuaTrinhCongTac.ToString(), db
                //        , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            }

            //var lstFileCuCanXoa = item.lstFileDinhKemCu.Where(x => x.isDeleted == true).ToList();
            //FileUpload.DeleteFiles(lstFileCuCanXoa, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", item.IdGuid);
        }
        public dynamic DeleteQuaTrinhCongTacNhanVien(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //int QuyenView = GetQuyenXoa(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền xóa");
            //#endregion

            var lstBangCapNhanVien = db.HRM_QuaTrinhCongTac.Where(x => lstId.Contains(x.IdGuid)).ToList();
            FileUploadController FileUpload = new FileUploadController();
            foreach (var BangCapNhanVien in lstBangCapNhanVien)
            {
                db.Entry(BangCapNhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteQuaTrinhCongTacNhanVien, Error {0}", ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }

            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == DefineData.eTable.HRM_QuaTrinhCongTac.ToString()).ToList();
            FileUpload.DeleteFiles(lstFileDinhKem, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        #endregion     
        #region Quan hệ gia đình
        public object GetDanhSachQuanHeGiaDinhNhanVien(string IdNhanVien, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion

            var lstQuanHeGiaDinh = db.HRM_NhanVienQuanHeGiaDinh.Where(x => x.IdNhanVien == IdNhanVien).ToList();

            var lstId = lstQuanHeGiaDinh.Select(x => x.IdGuid);
            string TenBang = DefineData.eTable.HRM_QuanHeGiaDinhNhanVien.ToString();
            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == TenBang).Distinct();

            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            foreach (var item in lstQuanHeGiaDinh)
            {
                item.lstFileDinhKem = lstFileDinhKem.Where(x => x.idItem == item.IdGuid).ToList();
                item.QuocTich = lstQuocTich.FirstOrDefault(x => x.IdGuid == item.IdQuocTich);

                item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                item.ThoiDiemGiamTruUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiDiemGiamTru);
                item.KetThucGiamTruUnix = dungchung.DateTimeToUnixTimeStamp(item.KetThucGiamTru);
            }
            return lstQuanHeGiaDinh;
        }
        public dynamic GeQuanHeGiaDinhNhanVien(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
        {

            //#region Quyền
            //int QuyenView = GetQuyenView(DefineData.eAction.THONGTINNHANVIEN.ToString(), idCurrentUser);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền view");
            //#endregion

            var item = db.HRM_NhanVienQuanHeGiaDinh.FirstOrDefault(x => x.IdGuid == Id);

            if (item == null)
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy Bằng cấp nhân viên");
            string sLoai = DefineData.eTable.HRM_QuanHeGiaDinhNhanVien.ToString();
            item.lstFileDinhKem = db.HRM_FileDinhKem.Where(x => x.IdGuid == Id && x.TableName == sLoai).ToList();
            item.QuocTich = db.HRM_dmQuocTich.FirstOrDefault(x => x.IdGuid == item.IdQuocTich);
            item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
            item.ThoiDiemGiamTruUnix = dungchung.DateTimeToUnixTimeStamp(item.ThoiDiemGiamTru);
            item.KetThucGiamTruUnix = dungchung.DateTimeToUnixTimeStamp(item.KetThucGiamTru);

            var lstDanhMucNhomChungChi = db.HRM_dmNhomChungChi.ToList();
            return item;
        }
        public dynamic UpdateQuanHeGiaDinhNhanVien(HRM_NhanVienQuanHeGiaDinh item, string idCurrentUser = "", ApplicationDbContext db = null)
        {



            item.NgaySinh = dungchung.UnixTimeStampToDateTime(item.NgaySinhUnix);
            item.ThoiDiemGiamTru = dungchung.UnixTimeStampToDateTime(item.ThoiDiemGiamTruUnix);
            item.KetThucGiamTru = dungchung.UnixTimeStampToDateTime(item.KetThucGiamTruUnix);

            if (string.IsNullOrEmpty(item.IdGuid))
            {
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateQuanHeGiaDinhNhanVien data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }


            FileUploadController FileUpload = new FileUploadController();

            if (item.lstFileDinhKem != null && item.lstFileDinhKem.Count > 0)
            {
                FileUploadController.SetFileDinhKem(db, item.lstFileDinhKem, nameof(HRM_NhanVienQuanHeGiaDinh).ToString(), item.IdGuid);

                //var lstFileMoi = item.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
                //if (lstFileMoi.Count > 0)
                //    FileUpload.AddFileFromHDFile(item.lstFileDinhKemMoi, item.IdGuid, DefineData.eTable.HRM_QuanHeGiaDinhNhanVien.ToString(), db
                //        , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());
            }

            //var lstFileCuCanXoa = item.lstFileDinhKemCu.Where(x => x.isDeleted == true).ToList();
            //FileUpload.DeleteFiles(lstFileCuCanXoa, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", item.IdGuid);
        }
        public dynamic DeleteQuanHeGiaDinhNhanVien(List<string> lstId, string idCurrentUser = "", ApplicationDbContext db = null)
        {



            var lstBangCapNhanVien = db.HRM_NhanVienQuanHeGiaDinh.Where(x => lstId.Contains(x.IdGuid)).ToList();
            FileUploadController FileUpload = new FileUploadController();
            foreach (var BangCapNhanVien in lstBangCapNhanVien)
            {
                db.Entry(BangCapNhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteQuanHeGiaDinhNhanVien, Error {0}", ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
            }

            var lstFileDinhKem = db.HRM_FileDinhKem.Where(x => lstId.Contains(x.idItem) && x.TableName == DefineData.eTable.HRM_QuanHeGiaDinhNhanVien.ToString()).ToList();
            FileUpload.DeleteFiles(lstFileDinhKem, db);


            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        #endregion
        #region Quá trình công tác nhân viên
        public dynamic GetDanhSachQuaTrinhCongTacNhanVien(dynamic dyn, string idCurrentUser, ApplicationDbContext db = null)
        {
            #region data

            #endregion

            int CurrentPage = dyn.CurrentPage == null ? 0 : dyn.CurrentPage;
            int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
            string IdNhanVien = dyn.IdNhanVien == null ? "" : dyn.IdNhanVien;
            string KeyWord = dyn.KeyWord == null ? "" : dyn.KeyWord;
            double TuNgayUnix = dyn.TuNgayUnix == null ? 0 : dyn.TuNgayUnix;
            double DenNgayUnix = dyn.DenNgayUnix == null ? 0 : dyn.DenNgayUnix;

            if (string.IsNullOrEmpty(IdNhanVien))
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Truyền lên nhân viên");
            var query = db.HRM_QuaTrinhCongTacNhanVien.Where(x => x.IdNhanVien == IdNhanVien);
            if (string.IsNullOrEmpty(KeyWord) == false)
            {
                KeyWord = KeyWord.ToLower();
                query = query.Where(x => x.MoTa.ToLower().Contains(KeyWord));
            }

            if (TuNgayUnix > 0)
            {
                DateTime TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
                query = query.Where(x => x.NgayHieuLuc >= TuNgay);
            }

            if (DenNgayUnix > 0)
            {
                DateTime DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date.AddDays(1);
                query = query.Where(x => x.NgayHieuLuc < DenNgay);
            }

            if (CurrentPage == 0)
            {
                return query.OrderBy(q => q.Created).ToList();
            }
            else
            {
                dynamic result = new DynamicObject();

                int count = query.Count();
                var listResult = query.OrderBy(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

                result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
                result.items = listResult;
                return result;
            }

        }
        #endregion
        #region Import nhân viên
        public MessageReturn ImportExcelNhanVien(ApplicationDbContext _dbContext, string FileName)
        {
            using (var ts = _dbContext.Database.BeginTransaction())
            {
                int rowerror = 0;
                try
                {
                    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu nhân viên thành công");
                    //string pathTemplate = @"F:\DevWork\HRM2020\Backend\HRMNew\MauImportExcel\Import thông tin nhân viên.xlsx";
                    string pathTemplate = PathUploads + FileName;
                    // pathTemplate = @"D:\importThongTinLienQuanNhanVienDA (1).xlsx";
                    if (File.Exists(pathTemplate) == false)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
                    Stream s = File.OpenRead(pathTemplate);
                    string sMa = "";
                    ExcelPackage package = new ExcelPackage(s);
                    s.Close();
                    var sheet1 = package.Workbook.Worksheets["Thông tin nhân viên"];
                    int row = 3;
                    int maxrow = sheet1.Dimension.Rows;
                    var listTinhTrangHonNhan = _dbContext.HRM_dmTinhTrangHonNhan.ToList();
                    var listQuocTich = _dbContext.HRM_dmQuocTich.ToList();
                    var listDanToc = _dbContext.HRM_dmDanToc.ToList();
                    var listTinh = _dbContext.HRM_dmTinh.ToList();
                    var listQuan = _dbContext.HRM_dmQuanHuyen.ToList();
                    var listPhuongXa = _dbContext.HRM_dmPhuongXa.ToList();
                    var listQuanHe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
                    var lstNhanVien_Old = _dbContext.HRM_NhanVien.AsNoTracking().Where(q => string.IsNullOrEmpty(q.MaNhanVien) == false).Distinct().ToList();
                    var lstMaNhanVien_Old = lstNhanVien_Old.Select(q => q.MaNhanVien).Distinct().ToList();
                    //var listNhanVienEOS = _CallWebService.GetDanhSachNhanSuTheoBoPhan(_dbContext, 0);
                    var listNhanVien = new List<HRM_NhanVien>();
                    var ListdmTrinhDo = _dbContext.HRM_dmTrinhDoDaoTao.ToList();
                    var listNganHang = _dbContext.HRM_dmNganHang.ToList();
                    var listLoaiNhanVien = _dbContext.HRM_dmLoaiNhanVien.ToList();
                    var listCongTy = _CallWebService.GetDanhSachDuAn(_dbContext);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(_dbContext);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                    List<HRM_QuyTrinhBoPhan_BoPhan> listDonVi_NV = new List<HRM_QuyTrinhBoPhan_BoPhan>();
                    for (int i = row; i <= maxrow; i++)
                    {
                        int STT = 2;
                        var model = new HRM_NhanVien();
                        model.IdGuid = Guid.NewGuid().ToString();
                        string MaNhanVien = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (string.IsNullOrEmpty(MaNhanVien))
                            break;
                        model.MaNhanVien = MaNhanVien;
                        var lstHoTen = new List<string>();
                        model.SoGen = dungchung.GetCell(sheet1, i, STT);
                        STT++;
                        model.Ho = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(model.Ho))
                            lstHoTen.Add(model.Ho);
                        model.TenDem = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(model.TenDem))
                            lstHoTen.Add(model.TenDem);
                        model.Ten = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(model.Ten))
                            lstHoTen.Add(model.Ten);
                        if (lstHoTen.Any() == false)
                            continue;

                        //Tên đầy đủ
                        model.TenDayDu = lstHoTen.Join(" ");
                        model.TenThuongGoi = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var gioiTinh = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (gioiTinh.IsNotEmpty())
                        {
                            if (gioiTinh.ToLower() == HelperConstant.ddNhanVien.GioiTinh.KHONG_XAC_DINH.Code.ToLower())
                                model.IdGioiTinh = HelperConstant.ddNhanVien.GioiTinh.KHONG_XAC_DINH.Name;
                            if (gioiTinh.ToLower() == HelperConstant.ddNhanVien.GioiTinh.NAM.Code.ToLower())
                                model.IdGioiTinh = HelperConstant.ddNhanVien.GioiTinh.NAM.Name;
                            if (gioiTinh.ToLower() == HelperConstant.ddNhanVien.GioiTinh.NU.Code.ToLower())
                                model.IdGioiTinh = HelperConstant.ddNhanVien.GioiTinh.NU.Name;
                        }
                        try
                        {
                            model.NgaySinh = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        }
                        catch
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày sinh dòng số " + i + " Thông tin nhân viên ");
                        }
                        STT++;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var dmTrinhDo = ListdmTrinhDo.FirstOrDefault(x => x.Ma == sMa);
                        if (dmTrinhDo != null)
                            model.IddmTrinhDo = dmTrinhDo.IdGuid;
                        STT++;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var ttHonNhan = listTinhTrangHonNhan.FirstOrDefault(x => x.Ma == sMa);
                        if (ttHonNhan != null)
                            model.IdTinhTrangHonNhan = ttHonNhan.IdGuid;
                        STT++;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var quocTich = listQuocTich.FirstOrDefault(x => x.Ma == sMa);
                        if (quocTich != null)
                            model.IdQuocTich = quocTich.IdGuid;
                        STT++;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var danToc = listDanToc.FirstOrDefault(x => x.Ma == sMa);
                        if (danToc != null)
                            model.IdDanToc = danToc.IdGuid;
                        STT++;
                        model.TrangThai = dungchung.GetCellTangCol(sheet1, i, ref STT);

                        //model.Email = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var skyZaloFB = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        model.MaSoThueTNCN = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        model.MaSoThueCMT = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        model.NguyenQuan = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        model.HoKhauThuongTruDiaChi = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(sMa))
                            sMa = sMa.Split('-')[0];
                        var tinhThuongTru = listTinh.FirstOrDefault(x => x.Ma == sMa);
                        if (tinhThuongTru != null)
                            model.HoKhauThuongTruIdTinhThanhPho = tinhThuongTru.IdGuid;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(sMa))
                            sMa = sMa.Split('-')[0];
                        var huyenThuongTru = listQuan.FirstOrDefault(x => x.Ma == sMa);
                        if (huyenThuongTru != null)
                        {
                            if (huyenThuongTru.IdTinh == model.HoKhauThuongTruIdTinhThanhPho)
                                model.HoKhauThuongTruIdQuanHuyen = huyenThuongTru.IdGuid;
                        }
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(sMa))
                            sMa = sMa.Split('-')[0];
                        var phuongXaThuongTru = listPhuongXa.FirstOrDefault(x => x.Ma == sMa);
                        if (phuongXaThuongTru != null)
                        {
                            if (phuongXaThuongTru.IdQuanHuyen == model.HoKhauThuongTruIdQuanHuyen)
                                model.HoKhauThuongTruIdPhuongXa = phuongXaThuongTru.IdGuid;
                        }
                        model.ChoOHienNayTruDiaChi = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(sMa))
                            sMa = sMa.Split('-')[0];
                        var tinhHienTai = listTinh.FirstOrDefault(x => x.Ma == sMa);
                        if (tinhHienTai != null)
                            model.ChoOHienNayTruIdTinhThanhPho = tinhHienTai.IdGuid;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(sMa))
                            sMa = sMa.Split('-')[0];
                        var quanHienTai = listQuan.FirstOrDefault(x => x.Ma == sMa);
                        if (quanHienTai != null)
                        {
                            if (quanHienTai.IdTinh == model.ChoOHienNayTruIdTinhThanhPho)
                                model.ChoOHienNayTruIdQuanHuyen = quanHienTai.IdGuid;
                        }
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        if (!string.IsNullOrEmpty(sMa))
                            sMa = sMa.Split('-')[0];
                        var xaHienTai = listPhuongXa.FirstOrDefault(x => x.Ma == sMa);
                        if (xaHienTai != null)
                        {
                            if (xaHienTai.IdQuanHuyen == model.ChoOHienNayTruIdQuanHuyen)
                                model.ChoOHienNayTruIdPhuongXa = xaHienTai.IdGuid;
                        }
                        model.LienHeKhanCapHoTen = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var quanHe = listQuanHe.FirstOrDefault(x => x.Ma == sMa);
                        if (quanHe != null)
                            model.LienHeKhanIdQuanHe = quanHe.IdGuid;
                        model.LienHeKhanDienThoai = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        model.LienHeKhanDiaChi = dungchung.GetCellTangCol(sheet1, i, ref STT);

                        model.SoBaoHiemXaHoi = dungchung.GetCellTangCol(sheet1, i, ref STT);

                        model.TaiKhoanNganHang = dungchung.GetCellTangCol(sheet1, i, ref STT);

                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var NganHang = listNganHang.FirstOrDefault(a => a.Ma == sMa);
                        if (NganHang != null)
                            model.TenNganHang = NganHang.IdGuid;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        List<string> listMaCT = sMa.Split(';').ToList();
                        var CongTys = listCongTy.Where(a => listMaCT.Contains(a.MaBoPhan)).ToList();
                        if (CongTys != null)
                            foreach (var item in CongTys)
                            {
                                HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                                bp.IdGuid = Guid.NewGuid().ToString();
                                bp.MaNhanVien = model.MaNhanVien;
                                bp.IdBoPhan = item.Id;
                                bp.TableName = nameof(HRM_NhanVien);
                                listDonVi_NV.Add(bp);
                            }
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == sMa);
                        if (BoPhan != null)
                            model.IdBoPhanEOS = BoPhan.Id;
                        STT++;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var ChucVu = listChucVu.FirstOrDefault(a => a.MaChucVu == sMa);
                        if (ChucVu != null)
                            model.IdChucVu = ChucVu.Id;
                        STT++;
                        sMa = dungchung.GetCellTangCol(sheet1, i, ref STT);
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == sMa);
                        if (ChucDanh != null)
                            model.IdChucDanh = ChucDanh.Id;
                        STT++;
                        try
                        {
                            model.NgayBatDauLamViec = dungchung.GetCellToDateTime(sheet1, i, STT);
                        }
                        catch
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày bắt đầu làm việc dòng số " + i + " Thông tin nhân viên ");
                        }
                        STT++;
                        string LoaiNhanVien = dungchung.GetCell(sheet1, i, STT);
                        STT++;
                        STT++;
                        model.UserName = dungchung.GetCell(sheet1, i, STT);

                        if (!string.IsNullOrEmpty(LoaiNhanVien))
                            model.IdLoaiNhanVien = listLoaiNhanVien.Where(a => a.Ma == LoaiNhanVien).Select(a => a.IdGuid).FirstOrDefault();
                        bool checkEsist = listNhanVien.Where(q => q.MaNhanVien.ToLower() == model.MaNhanVien.ToLower()).Any();
                        if (checkEsist)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Trùng mã trong file: " + model.MaNhanVien);
                        if (string.IsNullOrEmpty(model.MaNhanVien) == false)
                            listNhanVien.Add(model);
                    }

                    rowerror = 1;

                    foreach (var nhanVien in listNhanVien)
                    {
                        var listDV = listDonVi_NV.Where(a => a.MaNhanVien == nhanVien.MaNhanVien).ToList();
                        nhanVien.listIdCongTy = listDV.Select(a => a.IdBoPhan).Distinct().ToList();
                        if (lstMaNhanVien_Old.Contains(nhanVien.MaNhanVien))
                        {
                            var NVOld = lstNhanVien_Old.FirstOrDefault(a => a.MaNhanVien == nhanVien.MaNhanVien);
                            if (listDV != null)
                                Parallel.ForEach(listDV, item =>
                                {
                                    item.IdQuyTrinh = NVOld.IdGuid;
                                });
                            _dbContext.HRM_QuyTrinhBoPhan_BoPhan.AddRange(listDV);

                            continue;

                        }
                        if (string.IsNullOrEmpty(nhanVien.UserName))
                            nhanVien.UserName = nhanVien.MaNhanVien;

                        var NhanVienFind = _CallWebService.SetNhanSuCreateUser(_dbContext, nhanVien);
                        if (NhanVienFind != null)
                        {
                            nhanVien.IdNhanVienEOS = NhanVienFind.Id;
                            nhanVien.IdUser = NhanVienFind.UserPkid;
                        }
                        //if (nhanVien.listSoDienThoai != null && nhanVien.listSoDienThoai.Count > 0)
                        //    _dbContext.HRM_NhanVien_SoDienThoai.AddRange(nhanVien.listSoDienThoai);
                        _dbContext.HRM_NhanVien.Add(nhanVien);
                        if (listDV != null)
                            Parallel.ForEach(listDV, item =>
                            {
                                item.IdQuyTrinh = nhanVien.IdGuid;
                            });
                        _dbContext.HRM_QuyTrinhBoPhan_BoPhan.AddRange(listDV);
                        clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSuNhanVien(_dbContext, nhanVien.IdNhanVienEOS, nhanVien.IdCongTyEOS, nhanVien.IdBoPhanEOS, nhanVien.IdChucVu, DateTime.Now, null);
                        rowerror++;
                    }
                    _dbContext.SaveChanges();
                    _CallWebService.AddModify(_dbContext, "import Thông tin nhân viên", DefineData.eModify.Import.ToString(), DefineData.eAction.HOSONHANVIEN.ToString(), "import Thông tin nhân viên", "", FileName, JsonConvert.SerializeObject(listNhanVien), 0, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);


                    //foreach (var item in listNhanVien)
                    //{
                    //    var err = SeThongTinNhanVien(item, _dbContext, aSMart);
                    //    if (err.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    //    {
                    //        return err;
                    //    }
                    //}
                    ts.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString(), rowerror);
                }
            }
        }
        public MessageReturn ImportExcelThongTinLienQuanNhanVien(string aFileName, ApplicationDbContext _dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu thông tin nhân viên thành công");

            //   string pathTemplate = @"F:\DevWork\HRM2020\Backend\HRMNew\MauImportExcel\Import thông tin liên quan nhân viên.xlsx";
            string pathTemplate = PathUploads + aFileName;
            //pathTemplate = @"D:\importThongTinLienQuanNhanVienDA (1).xlsx";
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            DateTime dt = DateTime.Now;
            Stream s = File.OpenRead(pathTemplate);
            ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var listNhanVien = _dbContext.HRM_NhanVien.AsNoTracking().ToList();
            var listErr = new List<string>();
            if (listNhanVien.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu nhân viên");
            }
            //if (listId.FirstOrDefault(a => a == 1) != null)
            //{
            var ThongTinChung = package.Workbook.Worksheets["Thông tin nhân viên"];
            var listTinhTrangHonNhan = _dbContext.HRM_dmTinhTrangHonNhan.ToList();
            var listQuocTich = _dbContext.HRM_dmQuocTich.ToList();
            var listDanToc = _dbContext.HRM_dmDanToc.ToList();
            var listTinh = _dbContext.HRM_dmTinh.ToList();
            var listQuan = _dbContext.HRM_dmQuanHuyen.ToList();
            var listPhuongXa = _dbContext.HRM_dmPhuongXa.ToList();
            var listQuanHe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
            var ListdmTrinhDo = _dbContext.HRM_dmTrinhDoDaoTao.ToList();
            var dmQuanhe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
            var listNganHang = _dbContext.HRM_dmNganHang.ToList();
            var listLoaiNhanVien = _dbContext.HRM_dmLoaiNhanVien.ToList();
            var listCongTy = _CallWebService.GetDanhSachDuAn(_dbContext);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(_dbContext);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
            var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
            List<HRM_NhanVien_SoDienThoai> listSDT = new List<HRM_NhanVien_SoDienThoai>();
            List<HRM_QuyTrinhBoPhan_BoPhan> listDonVi = new List<HRM_QuyTrinhBoPhan_BoPhan>();

            for (int i = 3; i <= ThongTinChung.Dimension.Rows; i++)
            {
                int STT = 2;
                string MaNhanVien = dungchung.GetCell(ThongTinChung, i, STT);
                if (MaNhanVien.IsEmpty())
                {
                    break;
                }
                var model = listNhanVien.FirstOrDefault(a => a.MaNhanVien == MaNhanVien);
                if (model == null)
                    listErr.Add(MaNhanVien);
                else
                {
                    STT++;
                    model.MaNhanVien = MaNhanVien;
                    var lstHoTen = new List<string>();
                    model.SoGen = dungchung.GetCell(ThongTinChung, i, STT);
                    STT++;
                    model.Ho = dungchung.GetCell(ThongTinChung, i, STT);
                    if (!string.IsNullOrEmpty(model.Ho))
                        lstHoTen.Add(model.Ho);
                    STT++;
                    model.TenDem = dungchung.GetCell(ThongTinChung, i, STT);
                    if (!string.IsNullOrEmpty(model.TenDem))
                        lstHoTen.Add(model.TenDem);
                    STT++;
                    model.Ten = dungchung.GetCell(ThongTinChung, i, STT);
                    if (!string.IsNullOrEmpty(model.Ten))
                        lstHoTen.Add(model.Ten);
                    if (lstHoTen.Any() == false)
                        continue;
                    STT++;
                    //Tên đầy đủ
                    model.TenDayDu = lstHoTen.Join(" ");
                    model.TenThuongGoi = dungchung.GetCell(ThongTinChung, i, STT);
                    STT++;
                    var gioiTinh = dungchung.GetCell(ThongTinChung, i, STT);
                    if (gioiTinh.IsNotEmpty())
                    {
                        if (gioiTinh.ToLower() == HelperConstant.ddNhanVien.GioiTinh.KHONG_XAC_DINH.Code.ToLower())
                            model.IdGioiTinh = HelperConstant.ddNhanVien.GioiTinh.KHONG_XAC_DINH.Name;
                        if (gioiTinh.ToLower() == HelperConstant.ddNhanVien.GioiTinh.NAM.Code.ToLower())
                            model.IdGioiTinh = HelperConstant.ddNhanVien.GioiTinh.NAM.Name;
                        if (gioiTinh.ToLower() == HelperConstant.ddNhanVien.GioiTinh.NU.Code.ToLower())
                            model.IdGioiTinh = HelperConstant.ddNhanVien.GioiTinh.NU.Name;
                    }
                    STT++;
                    try
                    {
                        model.NgaySinh = dungchung.GetCellToDateTime(ThongTinChung, i, STT) ?? DateTime.Now;
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày sinh dòng số " + i + " Thông tin nhân viên ");
                    }
                    STT++;
                    string sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var dmTrinhDo = ListdmTrinhDo.FirstOrDefault(x => x.Ma == sMa);
                    if (dmTrinhDo != null)
                        model.IddmTrinhDo = dmTrinhDo.IdGuid;
                    STT++;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var ttHonNhan = listTinhTrangHonNhan.FirstOrDefault(x => x.Ma == sMa);
                    if (ttHonNhan != null)
                        model.IdTinhTrangHonNhan = ttHonNhan.IdGuid;
                    STT++;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var quocTich = listQuocTich.FirstOrDefault(x => x.Ma == sMa);
                    if (quocTich != null)
                        model.IdQuocTich = quocTich.IdGuid;
                    STT++;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var danToc = listDanToc.FirstOrDefault(x => x.Ma == sMa);
                    if (danToc != null)
                        model.IdDanToc = danToc.IdGuid;
                    STT++;
                    model.TrangThai = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);

                    //model.Email = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var skyZaloFB = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    model.MaSoThueTNCN = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    model.MaSoThueCMT = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    model.NguyenQuan = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    model.HoKhauThuongTruDiaChi = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(sMa))
                        sMa = sMa.Split('-')[0];
                    var tinhThuongTru = listTinh.FirstOrDefault(x => x.Ma == sMa);
                    if (tinhThuongTru != null)
                        model.HoKhauThuongTruIdTinhThanhPho = tinhThuongTru.IdGuid;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(sMa))
                        sMa = sMa.Split('-')[0];
                    var huyenThuongTru = listQuan.FirstOrDefault(x => x.Ma == sMa);
                    if (huyenThuongTru != null)
                    {
                        if (huyenThuongTru.IdTinh == model.HoKhauThuongTruIdTinhThanhPho)
                            model.HoKhauThuongTruIdQuanHuyen = huyenThuongTru.IdGuid;
                    }
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(sMa))
                        sMa = sMa.Split('-')[0];
                    var phuongXaThuongTru = listPhuongXa.FirstOrDefault(x => x.Ma == sMa);
                    if (phuongXaThuongTru != null)
                    {
                        if (phuongXaThuongTru.IdQuanHuyen == model.HoKhauThuongTruIdQuanHuyen)
                            model.HoKhauThuongTruIdPhuongXa = phuongXaThuongTru.IdGuid;
                    }
                    model.ChoOHienNayTruDiaChi = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(sMa))
                        sMa = sMa.Split('-')[0];
                    var tinhHienTai = listTinh.FirstOrDefault(x => x.Ma == sMa);
                    if (tinhHienTai != null)
                        model.ChoOHienNayTruIdTinhThanhPho = tinhHienTai.IdGuid;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(sMa))
                        sMa = sMa.Split('-')[0];
                    var quanHienTai = listQuan.FirstOrDefault(x => x.Ma == sMa);
                    if (quanHienTai != null)
                    {
                        if (quanHienTai.IdTinh == model.ChoOHienNayTruIdTinhThanhPho)
                            model.ChoOHienNayTruIdQuanHuyen = quanHienTai.IdGuid;
                    }
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(sMa))
                        sMa = sMa.Split('-')[0];
                    var xaHienTai = listPhuongXa.FirstOrDefault(x => x.Ma == sMa);
                    if (xaHienTai != null)
                    {
                        if (xaHienTai.IdQuanHuyen == model.ChoOHienNayTruIdQuanHuyen)
                            model.ChoOHienNayTruIdPhuongXa = xaHienTai.IdGuid;
                    }
                    model.LienHeKhanCapHoTen = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var quanHe = listQuanHe.FirstOrDefault(x => x.Ma == sMa);
                    if (quanHe != null)
                        model.LienHeKhanIdQuanHe = quanHe.IdGuid;
                    model.LienHeKhanDienThoai = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    model.LienHeKhanDiaChi = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    string SoBaoHiemXaHoi = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    if (!string.IsNullOrEmpty(SoBaoHiemXaHoi))
                        model.SoBaoHiemXaHoi = SoBaoHiemXaHoi;
                    if (!string.IsNullOrEmpty(model.SoBaoHiemXaHoi))
                        model.isChuaCoBHXH = false;
                    model.TaiKhoanNganHang = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);

                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var NganHang = listNganHang.FirstOrDefault(a => a.Ma == sMa);
                    if (NganHang != null)
                        model.TenNganHang = NganHang.IdGuid;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    List<string> listMaCT = sMa.Split(';').ToList();
                    var CongTys = listCongTy.Where(a => listMaCT.Contains(a.MaBoPhan)).ToList();
                    if (CongTys != null)
                        foreach (var item in CongTys)
                        {
                            HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                            bp.IdGuid = Guid.NewGuid().ToString();
                            bp.IdQuyTrinh = model.IdGuid;
                            bp.IdBoPhan = item.Id;
                            bp.TableName = nameof(HRM_NhanVien);
                            listDonVi.Add(bp);
                        }
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan == sMa);
                    if (BoPhan != null)
                        model.IdBoPhanEOS = BoPhan.Id;
                    STT++;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var ChucVu = listChucVu.FirstOrDefault(a => a.MaChucVu == sMa);
                    if (ChucVu != null)
                        model.IdChucVu = ChucVu.Id;
                    STT++;
                    sMa = dungchung.GetCellTangCol(ThongTinChung, i, ref STT);
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == sMa);
                    if (ChucDanh != null)
                        model.IdChucDanh = ChucDanh.Id;
                    STT++;
                    try
                    {
                        model.NgayBatDauLamViec = dungchung.GetCellToDateTime(ThongTinChung, i, STT);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày bắt đầu làm việc dòng số " + i + " Thông tin nhân viên ");
                    }
                    STT++;
                    string LoaiNhanVien = dungchung.GetCell(ThongTinChung, i, STT);
                    if (!string.IsNullOrEmpty(LoaiNhanVien))
                        model.IdLoaiNhanVien = listLoaiNhanVien.Where(a => a.Ma == LoaiNhanVien).Select(a => a.IdGuid).FirstOrDefault();
                    STT++;
                    STT++;
                    model.UserName = dungchung.GetCell(ThongTinChung, i, STT);


                    _dbContext.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSuNhanVien(_dbContext, model.IdNhanVienEOS, model.IdCongTyEOS, model.IdBoPhanEOS, model.IdChucVu, DateTime.Now, null);
                }


            }
            List<string> listIdNhanViens = listDonVi.Select(a => a.IdQuyTrinh).ToList();
            var listDonViOld = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanViens.Contains(a.IdQuyTrinh)).ToList();
            if (listDonVi != null && listDonVi.Count > 0)
                _dbContext.HRM_QuyTrinhBoPhan_BoPhan.AddRange(listDonVi);

            if (listDonViOld != null && listDonViOld.Count > 0)
            {
                _dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(listDonViOld);
            }

            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau không tồn tại ở sheet Thông tin nhân viên : {listErr.Join(",")}");
            }
            listErr = new List<string>();


            List<string> listIdNhanVien = listNhanVien.Select(a => a.IdGuid).ToList();
            var listSDTDBNV = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
            var SDTSheet = package.Workbook.Worksheets["Số điện thoại"];
            for (int i = 3; i <= int.MaxValue; i++)
            {
                int ncol = 2;
                string MaNhanVien = dungchung.GetCell(SDTSheet, i, ncol);
                if (MaNhanVien.IsEmpty())
                {
                    break;
                }
                var nhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == MaNhanVien);
                if (nhanVien == null)
                    listErr.Add(MaNhanVien);
                else
                {
                    HRM_NhanVien_SoDienThoai sdt = new HRM_NhanVien_SoDienThoai();
                    sdt.IdGuid = Guid.NewGuid().ToString();
                    sdt.IdNhanVien = nhanVien.IdGuid;
                    sdt.SoDienThoai = dungchung.GetCell(SDTSheet, i, 4);
                    string ApDung = dungchung.GetCell(SDTSheet, i, 5);
                    if (!string.IsNullOrEmpty(ApDung) && (ApDung.ToLower() == "true" || ApDung == "1"))
                    {
                        sdt.isApDung = true;

                        var SDTDB = listSDTDBNV.FirstOrDefault(a => a.IdNhanVien == sdt.IdNhanVien);
                        if (SDTDB != null)
                        {
                            SDTDB.isApDung = false;
                            _dbContext.Entry(SDTDB).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    listSDT.Add(sdt);
                }

            }
            _dbContext.HRM_NhanVien_SoDienThoai.AddRange(listSDT);
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau không tồn tại ở sheet Số điện thoại : {listErr.Join(",")}");
            }
            var listNoiCap = _dbContext.HRM_dmNoiCap.ToList();
            var CCCD = package.Workbook.Worksheets["CCCD"];
            var listGiayToNV = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
            List<HRM_NhanVien_GiayToTuyThan> listGiayTo = new List<HRM_NhanVien_GiayToTuyThan>();
            listErr = new List<string>();

            for (int i = 5; i <= int.MaxValue; i++)
            {
                int ncol = 2;
                string MaNhanVien = dungchung.GetCell(CCCD, i, ncol);
                if (MaNhanVien.IsEmpty())
                {
                    break;
                }
                var nhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == MaNhanVien);
                if (nhanVien == null)
                    listErr.Add(MaNhanVien);
                else
                {
                    {
                        HRM_NhanVien_GiayToTuyThan GiayTo = new HRM_NhanVien_GiayToTuyThan();
                        GiayTo.IdGuid = Guid.NewGuid().ToString();
                        try
                        {
                            GiayTo.IdNhanVien = nhanVien.IdGuid;
                        }
                        catch
                        {
                            GiayTo.IdNhanVien = nhanVien.IdGuid;
                        }
                        ncol = 4;
                        GiayTo.So = dungchung.GetCell(CCCD, i, ncol);
                        ncol++;
                        try
                        {
                            GiayTo.NgayCap = dungchung.GetCellToDateTime(CCCD, i, ncol);
                            GiayTo.NgayCapUnix = DateTimeToUnixTimeStamp(GiayTo.NgayCap);

                        }
                        catch
                        {

                        }
                        ncol++;
                        var MaNoiCap = dungchung.GetCell(CCCD, i, ncol);
                        if (!string.IsNullOrEmpty(MaNoiCap))
                        {
                            var Noicap = listNoiCap.FirstOrDefault(a => a.Ma == MaNoiCap);
                            if (Noicap != null)
                                GiayTo.NoiCap = Noicap.IdGuid;
                        }
                        ncol++;
                        GiayTo.NgayHetHan = dungchung.GetCellToDateTime(CCCD, i, ncol);
                        GiayTo.NgayHetHanUnix = DateTimeToUnixTimeStamp(GiayTo.NgayHetHan);

                        ncol++;
                        GiayTo.Loai = 1;
                        ncol++;
                        string ApDung = dungchung.GetCell(CCCD, i, 16);
                        if (!string.IsNullOrEmpty(ApDung) && (ApDung.ToLower() == "true" || ApDung == "1"))
                        {
                            GiayTo.isApDung = true;
                            var SDTDB = listGiayToNV.FirstOrDefault(a => a.IdNhanVien == GiayTo.IdNhanVien && a.Loai == GiayTo.Loai);
                            if (SDTDB != null)
                            {
                                SDTDB.isApDung = false;
                                _dbContext.Entry(SDTDB).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                        }
                        listGiayTo.Add(GiayTo);
                    }
                    ncol++;
                    {
                        HRM_NhanVien_GiayToTuyThan GiayTo = new HRM_NhanVien_GiayToTuyThan();
                        GiayTo.IdGuid = Guid.NewGuid().ToString();
                        try
                        {
                            GiayTo.IdNhanVien = nhanVien.IdGuid;
                        }
                        catch
                        {
                            GiayTo.IdNhanVien = nhanVien.IdGuid;
                        }

                        GiayTo.So = dungchung.GetCell(CCCD, i, ncol);
                        ncol++;
                        try
                        {
                            GiayTo.NgayCap = dungchung.GetCellToDateTime(CCCD, i, ncol);
                            GiayTo.NgayCapUnix = DateTimeToUnixTimeStamp(GiayTo.NgayCap);
                        }
                        catch
                        {

                        }
                        ncol++;
                        var MaNoiCap = dungchung.GetCell(CCCD, i, ncol);
                        if (!string.IsNullOrEmpty(MaNoiCap))
                        {
                            var Noicap = listNoiCap.FirstOrDefault(a => a.Ma == MaNoiCap);
                            if (Noicap != null)
                                GiayTo.NoiCap = Noicap.IdGuid;
                        }
                        ncol++;
                        GiayTo.NgayHetHan = dungchung.GetCellToDateTime(CCCD, i, ncol);
                        GiayTo.NgayHetHanUnix = DateTimeToUnixTimeStamp(GiayTo.NgayHetHan);
                        ncol++;
                        GiayTo.Loai = 2;

                        GiayTo.isApDung = false;


                        if (!string.IsNullOrEmpty(GiayTo.So))
                            listGiayTo.Add(GiayTo);
                    }
                    ncol++;
                    {
                        HRM_NhanVien_GiayToTuyThan GiayTo = new HRM_NhanVien_GiayToTuyThan();
                        GiayTo.IdGuid = Guid.NewGuid().ToString();
                        try
                        {
                            GiayTo.IdNhanVien = nhanVien.IdGuid;
                        }
                        catch
                        {
                            GiayTo.IdNhanVien = nhanVien.IdGuid;
                        }

                        GiayTo.So = dungchung.GetCell(CCCD, i, ncol);
                        ncol++;
                        try
                        {
                            GiayTo.NgayCap = dungchung.GetCellToDateTime(CCCD, i, ncol);
                            GiayTo.NgayCapUnix = DateTimeToUnixTimeStamp(GiayTo.NgayCap);
                        }
                        catch
                        {

                        }
                        ncol++;
                        var MaNoiCap = dungchung.GetCell(CCCD, i, ncol);
                        if (!string.IsNullOrEmpty(MaNoiCap))
                        {
                            var Noicap = listNoiCap.FirstOrDefault(a => a.Ma == MaNoiCap);
                            if (Noicap != null)
                                GiayTo.NoiCap = Noicap.IdGuid;
                        }
                        ncol++;
                        GiayTo.NgayHetHan = dungchung.GetCellToDateTime(CCCD, i, ncol);
                        ncol++;
                        GiayTo.Loai = 3;
                        string ApDung = dungchung.GetCell(CCCD, i, 16);
                        if (!string.IsNullOrEmpty(ApDung) && (ApDung.ToLower() == "true" || ApDung == "1"))
                        {
                            GiayTo.isApDung = true;
                            var SDTDB = listGiayToNV.FirstOrDefault(a => a.IdNhanVien == GiayTo.IdNhanVien && a.Loai == GiayTo.Loai);
                            if (SDTDB != null)
                            {
                                SDTDB.isApDung = false;
                                _dbContext.Entry(SDTDB).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                        }

                        if (!string.IsNullOrEmpty(GiayTo.So))
                            listGiayTo.Add(GiayTo);
                    }

                }

            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau không tồn tại ở sheet CCCD : {listErr.Join(",")}");
            }
            if (listGiayTo.Count > 0)
                _dbContext.HRM_NhanVien_GiayToTuyThan.AddRange(listGiayTo);
            var Email = package.Workbook.Worksheets["Email"];
            var listEmailNV = _dbContext.HRM_NhanVien_Email.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
            List<HRM_NhanVien_Email> listEmail = new List<HRM_NhanVien_Email>();
            listErr = new List<string>();

            for (int i = 3; i <= int.MaxValue; i++)
            {
                int ncol = 2;
                string MaNhanVien = dungchung.GetCell(Email, i, ncol);
                if (MaNhanVien.IsEmpty())
                {
                    break;
                }
                var nhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == MaNhanVien);
                if (nhanVien == null)
                    listErr.Add(MaNhanVien);
                else
                {
                    HRM_NhanVien_Email GiayTo = new HRM_NhanVien_Email();
                    GiayTo.IdGuid = Guid.NewGuid().ToString();
                    GiayTo.IdNhanVien = nhanVien.IdGuid;
                    GiayTo.Email = dungchung.GetCell(Email, i, 4);
                    string ApDung = dungchung.GetCell(Email, i, 5);
                    if (!string.IsNullOrEmpty(ApDung) && (ApDung.ToLower() == "true" || ApDung == "1"))
                    {
                        GiayTo.isApDung = true;
                        var SDTDB = listEmailNV.FirstOrDefault(a => a.IdNhanVien == GiayTo.IdNhanVien);
                        if (SDTDB != null)
                        {
                            SDTDB.isApDung = false;
                            _dbContext.Entry(SDTDB).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    listEmail.Add(GiayTo);
                }

            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau không tồn tại ở sheet CCCD : {listErr.Join(",")}");
            }
            if (listEmail.Count > 0)
                _dbContext.HRM_NhanVien_Email.AddRange(listEmail);

            //}

            var quanHeGiaDinh = package.Workbook.Worksheets["Quan hệ gia đình"];

            var bangCap = package.Workbook.Worksheets["Bằng cấp"];

            var chungChi = package.Workbook.Worksheets["Chứng chỉ"];
            var quaTrinhCongTac = package.Workbook.Worksheets["Quá trình công tác"];
            var quaTrinhCongTacNgoaiCongTy = package.Workbook.Worksheets["QT công tác ngoài công ty"];
            var ngonNguABC = package.Workbook.Worksheets["Ngôn ngữ"];
            var SheetBHXH = package.Workbook.Worksheets["BHXH"];
            var SheetSK = package.Workbook.Worksheets["Khám Sức khỏe"];
            var SheetNK = package.Workbook.Worksheets["Nhật ký"];


            var listDMQuanHe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
            var listDMTrinhDo = _dbContext.HRM_dmTrinhDoDaoTao.ToList();


            var listQuanHeGiaDinh = new List<HRM_NhanVienQuanHeGiaDinh>();
            listErr = new List<string>();
            for (int i = 3; i <= quanHeGiaDinh.Dimension.Rows; i++)
            {
                int STT = 2;
                var model = new HRM_NhanVienQuanHeGiaDinh();
                model.IdGuid = Guid.NewGuid().ToString();
                var maNhanVien = quanHeGiaDinh.Cells[i, STT].Text == null ? "" : quanHeGiaDinh.Cells[i, STT].Text.ToString();

                if (maNhanVien.IsEmpty())
                {
                    break;
                }
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    listErr.Add(maNhanVien);
                }
                if (nhanVien != null)
                {
                    model.IdNhanVien = nhanVien.IdGuid;
                }
                STT++; //STT=3
                STT++;//STT=4
                var maQuanHe = quanHeGiaDinh.Cells[i, STT].Text == null ? "" : quanHeGiaDinh.Cells[i, STT].Text.ToString();

                if (maQuanHe.IsNotEmpty() && listDMQuanHe.IsNotEmpty())
                {
                    var quanHe = listDMQuanHe.FirstOrDefault(x => x.Ma == maQuanHe);
                    if (quanHe != null)
                    {
                        model.IdQuanHe = quanHe.IdGuid;
                    }
                    else
                        continue;
                }
                else
                    continue;
                STT++; //STT=5
                model.HoTen = quanHeGiaDinh.Cells[i, STT].Value == null ? "" : quanHeGiaDinh.Cells[i, STT].Value.ToString();
                STT++;  //STT=6
                var namSinh = quanHeGiaDinh.Cells[i, STT].Value == null ? "" : quanHeGiaDinh.Cells[i, STT].Value.ToString();

                if (namSinh.IsNotEmpty())
                {
                    try
                    {
                        model.NgaySinh = dungchung.ToDateTime(namSinh);
                    }
                    catch (Exception)
                    {

                        continue;
                    }


                }
                else
                    continue;
                STT++;//STT=7
                model.SoDinhDanh = quanHeGiaDinh.Cells[i, STT].Value == null ? "" : quanHeGiaDinh.Cells[i, STT].Value.ToString();
                STT++;//STT=8
                model.NgheNghiep = quanHeGiaDinh.Cells[i, STT].Value == null ? "" : quanHeGiaDinh.Cells[i, STT].Value.ToString();
                STT++;////STT=9
                model.NoiLamViec = quanHeGiaDinh.Cells[i, STT].Value == null ? "" : quanHeGiaDinh.Cells[i, STT].Value.ToString();
                STT++;
                var phuthuoc = quanHeGiaDinh.Cells[i, STT].Value == null ? "" : quanHeGiaDinh.Cells[i, STT].Value.ToString();
                if (phuthuoc == "1")
                    model.isLaNguoiPhuThuoc = true;
                listQuanHeGiaDinh.Add(model);
            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau không tồn tại ở sheet quan hệ gia đình : {listErr.Join(",")}");
            }
            var listNhatKy = new List<HRM_NhatKyNhanVien>();
            listErr = new List<string>();
            if (SheetNK != null)
                for (int i = 4; i <= SheetNK.Dimension.Rows; i++)
                {
                    int STT = 2;
                    var model = new HRM_NhatKyNhanVien();
                    model.IdGuid = Guid.NewGuid().ToString();
                    var maNhanVien = SheetNK.Cells[i, STT].Text == null ? "" : SheetNK.Cells[i, STT].Text.ToString();

                    if (maNhanVien.IsEmpty())
                    {
                        break;
                    }
                    var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                    if (nhanVien == null)
                    {
                        listErr.Add(maNhanVien);
                    }
                    if (nhanVien != null)
                    {
                        model.IdNhanVien = nhanVien.IdGuid;
                    }
                    STT++; //STT=3
                    STT++; //STT=4
                    model.QuyTrinh = dungchung.GetCell(SheetNK, i, STT);
                    STT++;//STT=5
                    model.ThoiGian = dungchung.GetCellToDateTime(SheetNK, i, STT) ?? DateTime.Now;
                    listNhatKy.Add(model);
                }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau không tồn tại ở sheet NHẬT KÝ : {listErr.Join(",")}");
            }

            listErr = new List<string>();
            var listBangCap = new List<HRM_NhanVienBangCap>();
            for (int i = 2; i <= bangCap.Dimension.Rows; i++)
            {
                int STT = 2;
                var model = new HRM_NhanVienBangCap();
                model.IdGuid = Guid.NewGuid().ToString();
                var maNhanVien = bangCap.Cells[i, STT].Text == null ? "" : bangCap.Cells[i, STT].Text.ToString();
                if (maNhanVien.IsEmpty())
                {
                    continue;
                }
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    listErr.Add(maNhanVien);
                }
                if (nhanVien != null)
                {
                    model.IdNhanVien = nhanVien.IdGuid;
                }
                STT++;
                STT++;// STT=4
                model.NoiDaoTao = bangCap.Cells[i, STT].Value == null ? "" : bangCap.Cells[i, STT].Value.ToString();
                STT++;// STT=5
                model.ChuyenNganh = bangCap.Cells[i, STT].Value == null ? "" : bangCap.Cells[i, STT].Value.ToString();
                STT++;// STT=6
                var maTrinhDo = bangCap.Cells[i, STT].Value == null ? "" : bangCap.Cells[i, STT].Value.ToString();

                if (maTrinhDo.IsNotEmpty() && listDMTrinhDo.IsNotEmpty())
                {
                    var trinhDo = listDMTrinhDo.FirstOrDefault(x => x.Ma == maTrinhDo);
                    if (trinhDo != null)
                    {
                        model.IdTrinhDoDaoTao = trinhDo.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "mã trình độ đào tạo của nhân viên " + maNhanVien + " không tồn tại");
                }
                STT++;// STT=7
                STT++;
                model.NgayTotNghiep = dungchung.GetCellToDateTime(bangCap, i, STT);
                if (!string.IsNullOrEmpty(model.NoiDaoTao) || !string.IsNullOrEmpty(model.ChuyenNganh) || !string.IsNullOrEmpty(model.IdTrinhDoDaoTao) || model.NgayTotNghiep != null)
                    listBangCap.Add(model);
            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở trong sheet bằng cấp không tồn tại trong dữ liệu : {listErr.Join(",")}");
            }
            var listDMNgonNgu = _dbContext.HRM_dmNgonNgu.ToList();
            var listNgonNgu = new List<HRM_NgonNguNhanVien>();
            //int aaa = quanHeGiaDinh.Dimension.Rows;
            //aaa = ngonNguABC.Dimension.Rows; 
            var listDmXepLoai = _dbContext.HRM_dmXepLoai.ToList();
            for (int i = 2; i <= ngonNguABC.Dimension.Rows; i++)
            {
                int STT = 2;
                var model = new HRM_NgonNguNhanVien();
                model.IdGuid = Guid.NewGuid().ToString();
                var maNhanVien = ngonNguABC.Cells[i, STT].Text == null ? "" : ngonNguABC.Cells[i, STT].Text.ToString();
                STT++;//3
                if (maNhanVien.IsEmpty())
                {
                    continue;
                }
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    listErr.Add(maNhanVien);
                }
                if (nhanVien != null)
                {
                    model.IdNhanVien = nhanVien.IdGuid;
                }
                STT++;//4
                var maNgonNgu = ngonNguABC.Cells[i, STT].Value == null ? "" : ngonNguABC.Cells[i, STT].Value.ToString();
                if (string.IsNullOrEmpty(maNgonNgu))
                    continue;
                if (maNgonNgu.IsNotEmpty() && listDMNgonNgu.IsNotEmpty())
                {
                    var dmNgonNgu = listDMNgonNgu.FirstOrDefault(x => x.Ma == maNgonNgu);
                    if (dmNgonNgu != null)
                    {
                        model.IdNgonNgu = dmNgonNgu.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã ngôn ngữ ở nhân viên " + maNhanVien + " không tồn tại");
                }
                STT++;//5
                STT++;
                var IdXepLoaiNghe = ngonNguABC.Cells[i, STT].Value == null ? "" : ngonNguABC.Cells[i, STT].Value.ToString();
                if (IdXepLoaiNghe.IsNotEmpty() && listDmXepLoai.IsNotEmpty())
                {
                    var dmNgonNgu = listDmXepLoai.FirstOrDefault(x => x.Ma == IdXepLoaiNghe);
                    if (dmNgonNgu != null)
                    {
                        model.IdXepLoaiNghe = dmNgonNgu.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã xếp loại nghe ngôn ngữ ở nhân viên " + maNhanVien + " không tồn tại");
                }
                STT++;//6
                var IdXepLoaiNoi = ngonNguABC.Cells[i, STT].Value == null ? "" : ngonNguABC.Cells[i, STT].Value.ToString();
                if (IdXepLoaiNoi.IsNotEmpty() && listDmXepLoai.IsNotEmpty())
                {
                    var dmNgonNgu = listDmXepLoai.FirstOrDefault(x => x.Ma == IdXepLoaiNoi);
                    if (dmNgonNgu != null)
                    {
                        model.IdXepLoaiNoi = dmNgonNgu.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã xếp loại nói ngôn ngữ ở nhân viên " + maNhanVien + " không tồn tại");
                }
                STT++;////7
                var IdXepLoaiDoc = ngonNguABC.Cells[i, STT].Value == null ? "" : ngonNguABC.Cells[i, STT].Value.ToString();
                if (IdXepLoaiDoc.IsNotEmpty() && listDmXepLoai.IsNotEmpty())
                {
                    var dmNgonNgu = listDmXepLoai.FirstOrDefault(x => x.Ma == IdXepLoaiDoc);
                    if (dmNgonNgu != null)
                    {
                        model.IdXepLoaiDoc = dmNgonNgu.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã xếp loại đọc ngôn ngữ ở nhân viên " + maNhanVien + " không tồn tại");
                }
                STT++;////8
                var IdXepLoaiViet = ngonNguABC.Cells[i, STT].Value == null ? "" : ngonNguABC.Cells[i, STT].Value.ToString();
                if (IdXepLoaiViet.IsNotEmpty() && listDmXepLoai.IsNotEmpty())
                {
                    var dmNgonNgu = listDmXepLoai.FirstOrDefault(x => x.Ma == IdXepLoaiViet);
                    if (dmNgonNgu != null)
                    {
                        model.IdXepLoaiViet = dmNgonNgu.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã xếp loại viết ngôn ngữ ở nhân viên " + maNhanVien + " không tồn tại");
                }
                STT++;
                listNgonNgu.Add(model);
            }

            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở trong sheet ngôn ngữ không tồn tại trong dữ liệu : {listErr.Join(",")}");
            }
            var listDMNhomChungChi = _dbContext.HRM_dmNhomChungChi.ToList();
            var listChungChi = new List<HRM_ChungChiNhanVien>();
            for (int i = 2; i <= chungChi.Dimension.Rows; i++)
            {
                int STT = 2;
                var model = new HRM_ChungChiNhanVien();
                model.IdGuid = Guid.NewGuid().ToString();
                var maNhanVien = chungChi.Cells[i, STT].Text == null ? "" : chungChi.Cells[i, STT].Text.ToString();

                if (maNhanVien.IsEmpty())
                {
                    continue;
                }
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    listErr.Add(maNhanVien);
                }
                if (nhanVien != null)
                {
                    model.IdNhanVien = nhanVien.IdGuid;
                }
                STT++;//stt=3
                STT++;//stt=4
                model.TenChungChi = chungChi.Cells[i, STT].Value == null ? "" : chungChi.Cells[i, STT].Value.ToString();
                if (string.IsNullOrEmpty(model.TenChungChi))
                    continue;
                STT++;//stt=5
                var maNhomChungChi = chungChi.Cells[i, STT].Value == null ? "" : chungChi.Cells[i, STT].Value.ToString();
                STT++;//stt=6
                if (maNhomChungChi.IsNotEmpty() && listDMNhomChungChi.IsNotEmpty())
                {
                    var nhomChungChi = listDMNhomChungChi.FirstOrDefault(x => x.Ma == maNhomChungChi);
                    if (nhomChungChi != null)
                    {
                        model.IdNhomChungChi = nhomChungChi.IdGuid;
                    }
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "mã nhóm chứng chỉ " + maNhanVien + " không tồn tại");
                }
                STT++;
                model.TrinhDo = chungChi.Cells[i, STT].Value == null ? "" : chungChi.Cells[i, STT].Value.ToString();

                STT++;//stt=7

                var ngayCap = chungChi.Cells[i, STT].Value == null ? "" : chungChi.Cells[i, STT].Value.ToString();

                if (ngayCap.IsNotEmpty())
                {
                    DateTime date;
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    var checkDate = DateTime.TryParseExact(ngayCap, new string[] { "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy-MM-dd" }, provider, DateTimeStyles.None, out date);
                    if (checkDate == true)
                    {
                        model.NgayCap = date;
                        model.NgayCapUnix = DateTimeToUnixTimeStamp(date);
                    }
                }
                STT++;//stt=8
                var ngayHetHan = chungChi.Cells[i, STT].Value == null ? "" : chungChi.Cells[i, STT].Value.ToString();
                STT++;//stt=0
                if (ngayHetHan.IsNotEmpty())
                {
                    DateTime date;
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    var checkDate = DateTime.TryParseExact(ngayHetHan, new string[] { "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy-MM-dd" }, provider, DateTimeStyles.None, out date);
                    if (checkDate == true)
                    {
                        model.NgayHetHan = date;
                        model.NgayHetHanUnix = DateTimeToUnixTimeStamp(date);
                    }
                }
                model.NoiCap = chungChi.Cells[i, STT].Value == null ? "" : chungChi.Cells[i, STT].Value.ToString();
                STT++;
                listChungChi.Add(model);
            }

            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet chứng chỉ không tồn tại trong dữ liệu : {listErr.Join(",")}");
            }

            var listQuaTrinhCongTac = new List<HRM_QuaTrinhCongTac>();
            for (int i = 2; i <= quaTrinhCongTac.Dimension.Rows; i++)
            {
                int STT = 2;
                var model = new HRM_QuaTrinhCongTac();
                model.IdGuid = Guid.NewGuid().ToString();
                var maNhanVien = quaTrinhCongTac.Cells[i, STT].Text == null ? "" : quaTrinhCongTac.Cells[i, STT].Text.ToString();
                STT++;
                if (maNhanVien.IsEmpty())
                {
                    continue;
                }
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    listErr.Add(maNhanVien);
                }
                if (nhanVien != null)
                {
                    model.IdNhanVien = nhanVien.IdGuid;
                }
                STT++;
                model.ThoiGian = quaTrinhCongTac.Cells[i, STT].Value == null ? "" : quaTrinhCongTac.Cells[i, STT].Value.ToString();
                if (string.IsNullOrEmpty(model.ThoiGian))
                    continue;
                STT++;
                model.DonViCongTac = quaTrinhCongTac.Cells[i, STT].Value == null ? "" : quaTrinhCongTac.Cells[i, STT].Value.ToString();
                STT++;
                model.ViTriCongViec = quaTrinhCongTac.Cells[i, STT].Value == null ? "" : quaTrinhCongTac.Cells[i, STT].Value.ToString();
                STT++;
                model.DiaDiem = quaTrinhCongTac.Cells[i, STT].Value == null ? "" : quaTrinhCongTac.Cells[i, STT].Value.ToString();
                STT++;

                listQuaTrinhCongTac.Add(model);
            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet Quá trình công tác không tồn tại trong dữ liệu : {listErr.Join(",")}");
            }
            var listQuaTrinhCongTacNgoaiCongTy = new List<HRM_QuaTrinhCongTacNgoaiCongTy>();
            for (int i = 2; i <= quaTrinhCongTacNgoaiCongTy.Dimension.Rows; i++)
            {
                int STT = 2;
                var model = new HRM_QuaTrinhCongTacNgoaiCongTy();
                model.IdGuid = Guid.NewGuid().ToString();
                var maNhanVien = quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Text == null ? "" : quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Text.ToString();
                STT++;
                if (maNhanVien.IsEmpty())
                {
                    continue;
                }
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    listErr.Add(maNhanVien);
                }
                if (nhanVien != null)
                {
                    model.IdNhanVien = nhanVien.IdGuid;
                }
                STT++;
                model.ThoiGian = quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value == null ? "" : quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value.ToString();
                if (string.IsNullOrEmpty(model.ThoiGian))
                    continue;
                STT++;
                model.DonViCongTac = quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value == null ? "" : quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value.ToString();
                STT++;
                model.ViTriCongViec = quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value == null ? "" : quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value.ToString();
                STT++;
                model.DiaDiem = quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value == null ? "" : quaTrinhCongTacNgoaiCongTy.Cells[i, STT].Value.ToString();
                STT++;

                listQuaTrinhCongTacNgoaiCongTy.Add(model);
            }
            if (listErr.IsNotEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet Công tác ngoài công ty không tồn tại trong dữ liệu : {listErr.Join(",")}");
            }

            List<HRM_QuaTrinhThamGiaBaoHiemXaHoi> listAdd = new List<HRM_QuaTrinhThamGiaBaoHiemXaHoi>();
            List<string> listMa = new List<string>();
            if (SheetBHXH != null)
            {
                for (int row = 4; row <= SheetBHXH.Cells.End.Row; row++)
                {
                    HRM_QuaTrinhThamGiaBaoHiemXaHoi thuvien = new HRM_QuaTrinhThamGiaBaoHiemXaHoi();
                    string Ma = dungchung.GetCell(SheetBHXH, row, 2);
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
                    thuvien.TenBoPhan = dungchung.GetCell(SheetBHXH, row, 5);
                    thuvien.TenChucVu = dungchung.GetCell(SheetBHXH, row, 6);
                    thuvien.TenCongTy = dungchung.GetCell(SheetBHXH, row, 7);
                    {
                        string Thang = dungchung.GetCell(SheetBHXH, row, 8);
                        if (!string.IsNullOrEmpty(Thang))
                        {
                            var list = Thang.Split('/');
                            thuvien.TuNgay = new DateTime(int.Parse(list[1]), int.Parse(list[0]), 1);
                        }
                    }
                    {
                        string Thang = dungchung.GetCell(SheetBHXH, row, 9);
                        if (!string.IsNullOrEmpty(Thang))
                        {
                            var list = Thang.Split('/');
                            thuvien.DenNgay = new DateTime(int.Parse(list[1]), int.Parse(list[0]), 1);
                            thuvien.DenNgay.AddMonths(1).AddDays(-1);
                        }
                    }
                    thuvien.SoBHXH = dungchung.GetCell(SheetBHXH, row, 10);
                    if (!string.IsNullOrEmpty(thuvien.SoBHXH) && (string.IsNullOrEmpty(NhanVien.SoBaoHiemXaHoi) || NhanVien.isChuaCoBHXH == true))
                    {
                        NhanVien.SoBaoHiemXaHoi = thuvien.SoBHXH;
                        NhanVien.isChuaCoBHXH = false;
                        _dbContext.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    thuvien.MucLuongDong = dungchung.GetCellToDouble(SheetBHXH, row, 11);
                    thuvien.TienLuong = dungchung.GetCellToDouble(SheetBHXH, row, 12);
                    thuvien.HeSo = dungchung.GetCellToDouble(SheetBHXH, row, 13);
                    thuvien.GhiChu = dungchung.GetCell(SheetBHXH, row, 14);
                    listAdd.Add(thuvien);
                }
            }
            if (listMa.Count > 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet Bảo hiểm xã hội không tồn tại trong dữ liệu " + string.Join(",", listMa));

            }

            List<HRM_NhanVien_KhamSucKhoe> listAddSK = new List<HRM_NhanVien_KhamSucKhoe>();
            listMa = new List<string>();
            if (SheetSK != null)
            {
                for (int row = 4; row <= SheetSK.Cells.End.Row; row++)
                {
                    HRM_NhanVien_KhamSucKhoe thuvien = new HRM_NhanVien_KhamSucKhoe();
                    string Ma = dungchung.GetCell(SheetSK, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    thuvien.IdGuid = Guid.NewGuid().ToString();

                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == Ma);
                    if (NhanVien != null)
                    {
                        thuvien.IdNhanVien = NhanVien.IdGuid;
                    }
                    else
                        listMa.Add(Ma);
                    thuvien.NgayKham = dungchung.GetCellToDateTime(SheetSK, row, 4);
                    thuvien.NgayKetThuc = dungchung.GetCellToDateTime(SheetSK, row, 5);
                    thuvien.NoiKham = dungchung.GetCell(SheetSK, row, 6);
                    thuvien.LoaiHinhKham = dungchung.GetCell(SheetSK, row, 7);
                    thuvien.GhiChu = dungchung.GetCell(SheetSK, row, 8);
                    listAddSK.Add(thuvien);
                }
            }
            if (listMa.Count > 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet Bảo hiểm xã hội không tồn tại trong dữ liệu " + string.Join(",", listMa));
            }

            if (listAddSK != null && listAddSK.Count > 0)
            {
                var ListIdNV = listAddSK.Select(a => a.IdNhanVien).Distinct().ToList();
                var lisKSK = _dbContext.HRM_NhanVien_KhamSucKhoe.Where(a => ListIdNV.Contains(a.IdNhanVien)).ToList();
                if (lisKSK != null && lisKSK.Count > 0)
                    _dbContext.HRM_NhanVien_KhamSucKhoe.RemoveRange(lisKSK);
            }
            var lstBaoHiemNgoai = new List<HRM_BaoHiem_HopDong>();
            var listNoiCapBH = _dbContext.HRM_dmNoiCapBaoHiem.ToList();
            var SheetbaoHiemCN = package.Workbook.Worksheets["Bảo hiểm con người"];
            listMa = new List<string>();
            if (SheetbaoHiemCN != null)
            {
                for (int i = 3; i <= SheetbaoHiemCN.Dimension.Rows; i++)
                {
                    int STT = 2;
                    var BaoHiemNgoai = new HRM_BaoHiem_HopDong();
                    BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                    if (string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
                        break;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == BaoHiemNgoai.MaNhanVien);
                    if (NhanVien != null)
                    {
                        BaoHiemNgoai.IdNhanVien = NhanVien.IdGuid;
                    }
                    else
                        listMa.Add(BaoHiemNgoai.MaNhanVien);
                    STT = 4;
                    BaoHiemNgoai.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
                    BaoHiemNgoai.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDau);
                    STT++;
                    BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
                    BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
                    STT++;
                    BaoHiemNgoai.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                    STT++;
                    string MaDonVi = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                    if (!string.IsNullOrEmpty(MaDonVi))
                        BaoHiemNgoai.NoiCapSoBaoHiem = listNoiCapBH.Where(a => a.Ma == MaDonVi).Select(a => a.IdGuid).FirstOrDefault();
                    STT++;
                    STT++;
                    BaoHiemNgoai.MucDongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
                    STT++;
                    BaoHiemNgoai.SoTienBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
                    STT++;
                    string Giam = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                    if (Giam.ToUpper() == "CÓ")
                        BaoHiemNgoai.Giam = 1;
                    else
                        BaoHiemNgoai.Giam = 2;
                    STT++;
                    BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                    STT++;
                    lstBaoHiemNgoai.Add(BaoHiemNgoai);
                }
            }
            if (lstBaoHiemNgoai != null && lstBaoHiemNgoai.Count > 0)
            {
                var ListIdNV = lstBaoHiemNgoai.Select(a => a.IdNhanVien).Distinct().ToList();
                var lisKSK = _dbContext.HRM_BaoHiem_HopDong.Where(a => ListIdNV.Contains(a.IdNhanVien) && string.IsNullOrEmpty(a.IdHopDong)).ToList();
                if (lisKSK != null && lisKSK.Count > 0)
                    _dbContext.HRM_BaoHiem_HopDong.RemoveRange(lisKSK);
            }
            if (listMa.Count > 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet Bảo hiểm con người không tồn tại trong dữ liệu " + string.Join(",", listMa));

            }
            var lstBaoHiemTN = new List<HRM_NhanVien_BaoHiemTaiNan>();
            var SheetbaoHiemTN = package.Workbook.Worksheets["Bảo hiểm tai nạn"];
            listMa = new List<string>();
            if (SheetbaoHiemTN != null)
            {
                for (int i = 2; i <= SheetbaoHiemTN.Dimension.Rows; i++)
                {
                    int STT = 2;
                    var BaoHiemTaiNan = new HRM_NhanVien_BaoHiemTaiNan();
                    BaoHiemTaiNan.MaNhanVien = dungchung.GetCell(SheetbaoHiemTN, i, STT);
                    if (string.IsNullOrEmpty(BaoHiemTaiNan.MaNhanVien))
                        break;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == BaoHiemTaiNan.MaNhanVien);
                    if (NhanVien != null)
                    {
                        BaoHiemTaiNan.IdNhanVien = NhanVien.IdGuid;
                    }
                    else
                        listMa.Add(BaoHiemTaiNan.MaNhanVien);
                    STT = 4;

                    BaoHiemTaiNan.SoTienBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemTN, i, STT);
                    STT++;
                    BaoHiemTaiNan.SoHopDong = dungchung.GetCell(SheetbaoHiemTN, i, STT);
                    STT++;
                    BaoHiemTaiNan.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiemTN, i, STT);
                    BaoHiemTaiNan.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemTaiNan.NgayBatDau);
                    STT++;
                    BaoHiemTaiNan.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiemTN, i, STT);
                    BaoHiemTaiNan.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemTaiNan.NgayKetThuc);
                    STT++;

                    string MaDonVi = dungchung.GetCell(SheetbaoHiemTN, i, STT);
                    if (!string.IsNullOrEmpty(MaDonVi))
                        BaoHiemTaiNan.NoiCapSoBaoHiem = listNoiCapBH.Where(a => a.Ma == MaDonVi).Select(a => a.IdGuid).FirstOrDefault();
                    STT++;
                    STT++;
                    string Giam = dungchung.GetCell(SheetbaoHiemTN, i, STT);
                    if (Giam.ToUpper() == "TĂNG")
                        BaoHiemTaiNan.Giam = 1;
                    else
                        BaoHiemTaiNan.Giam = 2;
                    STT++;
                    BaoHiemTaiNan.GhiChu = dungchung.GetCell(SheetbaoHiemTN, i, STT);
                    STT++;
                    lstBaoHiemTN.Add(BaoHiemTaiNan);
                }
            }
            if (listMa.Count > 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"Các mã nhân viên sau ở sheet Bảo hiểm tai nạn không tồn tại trong dữ liệu " + string.Join(",", listMa));

            }
            if (lstBaoHiemTN != null && lstBaoHiemTN.Count > 0)
            {
                var ListIdNV = lstBaoHiemTN.Select(a => a.IdNhanVien).Distinct().ToList();
                var lisKSK = _dbContext.HRM_NhanVien_BaoHiemTaiNan.Where(a => ListIdNV.Contains(a.IdNhanVien)).ToList();
                if (lisKSK != null && lisKSK.Count > 0)
                    _dbContext.HRM_NhanVien_BaoHiemTaiNan.RemoveRange(lisKSK);
            }
            _CallWebService.AddModify(_dbContext, "import Thông tin nhân viên", DefineData.eModify.Import.ToString(), DefineData.eAction.HOSONHANVIEN.ToString(), "import Thông tin nhân viên", "", aFileName, JsonConvert.SerializeObject(listNhanVien), 0, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);

            _dbContext.HRM_NhanVien_BaoHiemTaiNan.AddRange(lstBaoHiemTN);
            _dbContext.HRM_BaoHiem_HopDong.AddRange(lstBaoHiemNgoai);
            _dbContext.HRM_NhanVien_KhamSucKhoe.AddRange(listAddSK);
            _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AddRange(listAdd);


            _dbContext.HRM_NhanVienQuanHeGiaDinh.AddRange(listQuanHeGiaDinh);
            _dbContext.HRM_NhanVienBangCap.AddRange(listBangCap);

            _dbContext.HRM_NgonNguNhanVien.AddRange(listNgonNgu);
            _dbContext.HRM_NhatKyNhanVien.AddRange(listNhatKy);
            _dbContext.HRM_ChungChiNhanVien.AddRange(listChungChi);
            _dbContext.HRM_QuaTrinhCongTac.AddRange(listQuaTrinhCongTac);
            _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.AddRange(listQuaTrinhCongTacNgoaiCongTy);
            _dbContext.SaveChanges();

            return result;
        }
        public dynamic ExportExcelThongTinLienQuanNhanVien(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext _dbContext)
        {
            try
            {
                #region data
                var listTinhTrangHonNhan = _dbContext.HRM_dmTinhTrangHonNhan.ToList();
                var listQuocTich = _dbContext.HRM_dmQuocTich.ToList();
                var listDanToc = _dbContext.HRM_dmDanToc.ToList();
                var listTinh = _dbContext.HRM_dmTinh.ToList();
                var listQuan = _dbContext.HRM_dmQuanHuyen.ToList();
                var listPhuongXa = _dbContext.HRM_dmPhuongXa.ToList();
                var listQuanHe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
                var ListdmTrinhDo = _dbContext.HRM_dmTrinhDoDaoTao.ToList();
                var dmQuanhe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
                var listNganHang = _dbContext.HRM_dmNganHang.ToList();
                var listLoaiNhanVien = _dbContext.HRM_dmLoaiNhanVien.ToList();
                var listCongTy = _CallWebService.GetDanhSachDuAn(_dbContext);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(_dbContext);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(_dbContext);
                var listChucVu = _CallWebService.GetDanhSachChucVu(_dbContext);
                var lstCongTyAll = _CallWebService.GetDanhSachDuAnQuanLyHoSoNhanVien(_dbContext);
                List<long> listIdDuAn = lstCongTyAll.Select(x => x.Id).ToList();
                var listNhanVien = (from a in _dbContext.HRM_NhanVien
                                    select a
                                                                 ).OrderBy(a => a.MaNhanVien).ToList();

                // Nếu có biến ListIdCongTyEOS thì sẽ lọc theo đống này - Duyệt mới thêm
                if (aRequest.ListIdCongTyEOS != null && aRequest.ListIdCongTyEOS.Count > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.ListIdCongTyEOS.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (aRequest.listIdBoPhan != null && aRequest.listIdBoPhan.Count > 0)
                    listNhanVien = listNhanVien.Where(x => aRequest.listIdBoPhan.Contains(x.IdBoPhanEOS)).ToList();

                if (aRequest.IdCongTyEOS > 0)
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => aRequest.IdCongTyEOS == x.IdBoPhan).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                else
                {
                    var _listBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdDuAn.Contains(x.IdBoPhan)).ToList();
                    var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                }
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdBoPhanEOS > 0)
                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == aRequest.IdBoPhanEOS).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0 && aRequest.IdChucVuEOS > 0)
                    listNhanVien = listNhanVien.Where(a => a.IdChucVu == aRequest.IdChucVuEOS).ToList();
                if (aRequest.Thang != 0)
                    listNhanVien = listNhanVien.Where(a => a.NgaySinh.Month == aRequest.Thang).ToList();
                if (aRequest.Ngay != 0)
                    listNhanVien = listNhanVien.Where(a => a.NgaySinh.Day == aRequest.Ngay).ToList();
                List<string> listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                var listSDTs = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                foreach (var model in listNhanVien)
                    model.DienThoai = listSDTs.Where(a => a.isApDung == true && a.IdNhanVien == model.IdGuid).Select(a => a.SoDienThoai).FirstOrDefault();

                if (listNhanVien != null && listNhanVien.Count > 0 && !string.IsNullOrEmpty(aRequest.KeyWord))
                {
                    string KeyWord = aRequest.KeyWord.ToLower().Trim();
                    listNhanVien = listNhanVien.Where(a => (!string.IsNullOrEmpty(a.TenDayDu) && a.TenDayDu.ToLower().Trim().Contains(KeyWord))
                                                            || (!string.IsNullOrEmpty(a.MaNhanVien) && a.MaNhanVien.ToLower().Contains(KeyWord))
                                                            || (!string.IsNullOrEmpty(a.DienThoai) && a.DienThoai.ToLower().Contains(KeyWord))
                                                            || (!string.IsNullOrEmpty(a.SoGen) && a.SoGen.ToLower().Contains(KeyWord))
                                                            ).ToList();
                }
                if (!string.IsNullOrEmpty(aRequest.TrangThai))
                    listNhanVien = listNhanVien.Where(a => a.TrangThai == aRequest.TrangThai).ToList();
                if (!string.IsNullOrEmpty(aRequest.GioiTinh))
                    listNhanVien = listNhanVien.Where(a => a.GioiTinh == aRequest.GioiTinh).ToList();
                #endregion
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/importThongTinLienQuanNhanVienDA.xlsx");
                Stream s = File.OpenRead(sFileName);
                ExcelPackage package = new ExcelPackage(s);
                if (listNhanVien != null && listNhanVien.Count > 0)
                {


                    var listIdNV = listNhanVien.Select(a => a.IdGuid).ToList();
                    //var listSDT = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => a.isApDung == true && listIdNV.Contains(a.IdNhanVien)).ToList();
                    var QHGD_Con = _dbContext.HRM_dmQuanHeGiaDinh.FirstOrDefault(a => a.Ma.ToLower() == "con");
                    DateTime time = DateTime.Now;
                    time = time.AddYears(-18);
                    var lisGiaDinh = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(a => a.isLaNguoiPhuThuoc == true && a.NgaySinh != null && a.NgaySinh <= time && listIdNV.Contains(a.IdNhanVien)).ToList();

                    // Start Duyệt mới thêm
                    var listNoiCap = _dbContext.HRM_dmNoiCap.ToList();
                    listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                    var listQuyTrinh_BoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
                    // End Duyệt mới thêm

                    var ThongTinChung = package.Workbook.Worksheets["Thông tin nhân viên"];
                    int STT = 1;
                    int nRow = 3;
                    foreach (var item in listNhanVien)
                    {
                        int nCol = 1;
                        ThongTinChung.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.Ho;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.TenDem;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.Ten;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.TenDayDu;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.TenThuongGoi;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.IdGioiTinh == "Nam" ? "Nam" : "Nữ";
                        nCol++;
                        if (item.NgaySinh != null)
                            ThongTinChung.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                        nCol++;
                        var dmTrinhDoa = ListdmTrinhDo.FirstOrDefault(x => x.IdGuid == item.IddmTrinhDo);
                        if (dmTrinhDoa != null)
                            ThongTinChung.Cells[nRow, nCol].Value = dmTrinhDoa.Ma;
                        nCol++;
                        if (dmTrinhDoa != null)
                            ThongTinChung.Cells[nRow, nCol].Value = dmTrinhDoa.Ten;
                        nCol++;

                        var ttHonNhan = listTinhTrangHonNhan.FirstOrDefault(x => x.IdGuid == item.IdTinhTrangHonNhan);
                        if (ttHonNhan != null)
                            ThongTinChung.Cells[nRow, nCol].Value = ttHonNhan.Ma;
                        nCol++;
                        if (ttHonNhan != null)
                            ThongTinChung.Cells[nRow, nCol].Value = ttHonNhan.Ten;
                        nCol++;

                        var quocTich = listQuocTich.FirstOrDefault(x => x.IdGuid == item.IdQuocTich);
                        if (quocTich != null)
                            ThongTinChung.Cells[nRow, nCol].Value = quocTich.Ma;
                        nCol++;
                        if (quocTich != null)
                            ThongTinChung.Cells[nRow, nCol].Value = quocTich.Ten;
                        nCol++;

                        var danToc = listDanToc.FirstOrDefault(x => x.IdGuid == item.IdDanToc);
                        if (danToc != null)
                            ThongTinChung.Cells[nRow, nCol].Value = danToc.Ma;
                        nCol++;
                        if (danToc != null)
                            ThongTinChung.Cells[nRow, nCol].Value = danToc.Ten;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.TrangThai;
                        nCol++;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.MaSoThueTNCN;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.MaSoThueCMT;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.NguyenQuan;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.HoKhauThuongTruDiaChi;
                        nCol++;
                        var tinhThuongTru = listTinh.FirstOrDefault(x => x.IdGuid == item.HoKhauThuongTruIdTinhThanhPho);
                        if (tinhThuongTru != null)
                            ThongTinChung.Cells[nRow, nCol].Value = tinhThuongTru.Ma + "-" + tinhThuongTru.Ten;
                        nCol++;
                        var huyenThuongTru = listQuan.FirstOrDefault(x => x.IdGuid == item.HoKhauThuongTruIdQuanHuyen);
                        if (huyenThuongTru != null)
                        {
                            ThongTinChung.Cells[nRow, nCol].Value = huyenThuongTru.Ma + "-" + huyenThuongTru.Ten;
                        }
                        nCol++;
                        var phuongXaThuongTru = listPhuongXa.FirstOrDefault(x => x.IdGuid == item.HoKhauThuongTruIdPhuongXa);
                        if (phuongXaThuongTru != null)
                        {
                            ThongTinChung.Cells[nRow, nCol].Value = phuongXaThuongTru.Ma + "-" + phuongXaThuongTru.Ten;
                        }
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.ChoOHienNayTruDiaChi;
                        nCol++;
                        tinhThuongTru = listTinh.FirstOrDefault(x => x.IdGuid == item.ChoOHienNayTruIdTinhThanhPho);
                        if (tinhThuongTru != null)
                            ThongTinChung.Cells[nRow, nCol].Value = tinhThuongTru.Ma + "-" + tinhThuongTru.Ten;
                        nCol++;
                        huyenThuongTru = listQuan.FirstOrDefault(x => x.IdGuid == item.ChoOHienNayTruIdQuanHuyen);
                        if (huyenThuongTru != null)
                        {
                            ThongTinChung.Cells[nRow, nCol].Value = huyenThuongTru.Ma + "-" + huyenThuongTru.Ten;
                        }
                        nCol++;
                        phuongXaThuongTru = listPhuongXa.FirstOrDefault(x => x.IdGuid == item.ChoOHienNayTruIdPhuongXa);
                        if (phuongXaThuongTru != null)
                        {
                            ThongTinChung.Cells[nRow, nCol].Value = phuongXaThuongTru.Ma + "-" + phuongXaThuongTru.Ten;
                        }
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.LienHeKhanCapHoTen;
                        nCol++;
                        var quanHe = listQuanHe.FirstOrDefault(x => x.IdGuid == item.LienHeKhanIdQuanHe);
                        if (quanHe != null)
                            ThongTinChung.Cells[nRow, nCol].Value = quanHe.Ma;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.LienHeKhanDienThoai;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.LienHeKhanDiaChi;

                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.SoBaoHiemXaHoi;
                        nCol++;
                        ThongTinChung.Cells[nRow, nCol].Value = item.TaiKhoanNganHang;
                        nCol++;
                        var NganHang = listNganHang.FirstOrDefault(a => a.IdGuid == item.TenNganHang);
                        if (NganHang != null)
                            ThongTinChung.Cells[nRow, nCol].Value = NganHang.Ten;
                        nCol++;
                        var lsstIdCt = listQuyTrinh_BoPhan.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
                        var CongTys = listCongTy.Where(a => lsstIdCt.Contains(a.Id)).Select(a => a.MaBoPhan).ToList();
                        if (CongTys != null)
                            ThongTinChung.Cells[nRow, nCol].Value = string.Join(",", CongTys);
                        nCol++;
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                            ThongTinChung.Cells[nRow, nCol].Value = BoPhan.MaBoPhan;
                        nCol++;
                        if (BoPhan != null)
                            ThongTinChung.Cells[nRow, nCol].Value = BoPhan.TenBoPhan;
                        nCol++;
                        var ChucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (ChucVu != null)
                            ThongTinChung.Cells[nRow, nCol].Value = ChucVu.MaChucVu;
                        nCol++;
                        if (ChucVu != null)
                            ThongTinChung.Cells[nRow, nCol].Value = ChucVu.TenChucVu;
                        nCol++;
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                            ThongTinChung.Cells[nRow, nCol].Value = ChucDanh.Ma;
                        nCol++;
                        if (ChucDanh != null)
                            ThongTinChung.Cells[nRow, nCol].Value = ChucDanh.Ten;
                        nCol++;
                        if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.Year >= 1970 != null)
                            ThongTinChung.Cells[nRow, nCol].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        var LoaiNhanVien = listLoaiNhanVien.FirstOrDefault(a => a.IdGuid == item.IdLoaiNhanVien);
                        if (LoaiNhanVien != null)
                            ThongTinChung.Cells[nRow, nCol].Value = LoaiNhanVien.Ma;
                        nCol++;
                        if (LoaiNhanVien != null)
                            ThongTinChung.Cells[nRow, nCol].Value = LoaiNhanVien.Ten;
                        nRow++;
                        STT++;
                    }
                    var listSDTDBNV = _dbContext.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var SDTSheet = package.Workbook.Worksheets["Số điện thoại"];
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listSDTDBNV)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        SDTSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        SDTSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        SDTSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        SDTSheet.Cells[nRow, ncol].Value = item.SoDienThoai;
                        ncol++;
                        SDTSheet.Cells[nRow, ncol].Value = item.isApDung == false ? "0" : "1";
                        ncol++;
                        nRow++;
                        STT++;
                    }
                    var listGiayToTuyThan = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var GiayToTuyThanSheet = package.Workbook.Worksheets["CCCD"];
                    nRow = 5;
                    STT = 1;
                    var listid = listGiayToTuyThan.Select(a => a.IdNhanVien).Distinct().ToList();
                    var lstNV = listNhanVien.Where(a => listid.Contains(a.IdGuid)).ToList();

                    foreach (var item in lstNV)
                    {
                        int ncol = 1;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = item.MaNhanVien;
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = item.TenDayDu;
                        ncol++;
                        var CCCD = listGiayToTuyThan.Where(a => a.IdNhanVien == item.IdGuid && a.Loai == 1).FirstOrDefault();
                        if (CCCD == null)
                            CCCD = new HRM_NhanVien_GiayToTuyThan();
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = CCCD.So;
                        ncol++;
                        if (CCCD.NgayCap != null)
                            GiayToTuyThanSheet.Cells[nRow, ncol].Value = CCCD.NgayCap.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = listNoiCap.Where(a => a.IdGuid == CCCD.NoiCap).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        if (CCCD.NgayHetHan != null)
                            GiayToTuyThanSheet.Cells[nRow, ncol].Value = CCCD.NgayHetHan.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        var CMT = listGiayToTuyThan.Where(a => a.IdNhanVien == item.IdGuid && a.Loai == 2).FirstOrDefault();
                        if (CMT == null)
                            CMT = new HRM_NhanVien_GiayToTuyThan();
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = CMT.So;
                        ncol++;
                        if (CMT.NgayCap != null)
                            GiayToTuyThanSheet.Cells[nRow, ncol].Value = CMT.NgayCap.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = listNoiCap.Where(a => a.IdGuid == CMT.NoiCap).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        if (CMT.NgayHetHan != null)
                            GiayToTuyThanSheet.Cells[nRow, ncol].Value = CMT.NgayHetHan.Value.ToString("dd/MM/yyyy");
                        var HC = listGiayToTuyThan.Where(a => a.IdNhanVien == item.IdGuid && a.Loai == 3).FirstOrDefault();
                        if (HC == null)
                            HC = new HRM_NhanVien_GiayToTuyThan();
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = HC.So;
                        ncol++;
                        if (HC.NgayCap != null)
                            GiayToTuyThanSheet.Cells[nRow, ncol].Value = HC.NgayCap.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = listNoiCap.Where(a => a.IdGuid == HC.NoiCap).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        if (HC.NgayHetHan != null)
                            GiayToTuyThanSheet.Cells[nRow, ncol].Value = HC.NgayHetHan.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        GiayToTuyThanSheet.Cells[nRow, ncol].Value = CCCD.isApDung == true ? "1" : "0";
                        nRow++;
                        STT++;
                    }
                    var listEmail = _dbContext.HRM_NhanVien_Email.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var EmailSheet = package.Workbook.Worksheets["Email"];
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listEmail)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        EmailSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        EmailSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        EmailSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        EmailSheet.Cells[nRow, ncol].Value = item.Email;
                        ncol++;
                        EmailSheet.Cells[nRow, ncol].Value = item.isApDung == false ? "0" : "1";
                        ncol++;
                        nRow++;
                        STT++;
                    }
                    var listQHGD = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var dmQuanHe = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
                    var QHGDSheet = package.Workbook.Worksheets["Quan hệ gia đình"];
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listQHGD)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        QHGDSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = dmQuanHe.Where(a => a.IdGuid == item.IdQuanHe).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = item.HoTen;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = item.NgaySinh.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = item.SoCMND;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = item.NgheNghiep;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = item.NoiLamViec;
                        ncol++;
                        QHGDSheet.Cells[nRow, ncol].Value = item.isLaNguoiPhuThuoc == true ? "1" : "0";
                        ncol++;

                        nRow++;
                        STT++;
                    }
                    var listBC = _dbContext.HRM_NhanVienBangCap.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var BCSheet = package.Workbook.Worksheets["Bằng cấp"];
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listBC)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        BCSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = item.NoiDaoTao;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = item.ChuyenNganh;
                        ncol++;
                        var TrinhDo = ListdmTrinhDo.FirstOrDefault(a => a.IdGuid == item.IdTrinhDoDaoTao);
                        if (TrinhDo == null)
                            TrinhDo = new HRM_dmTrinhDoDaoTao();
                        BCSheet.Cells[nRow, ncol].Value = TrinhDo.Ma;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = TrinhDo.Ten;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = item.NgayTotNghiep != null ? item.NgayTotNghiep.Value.ToString("dd/MM/yyyy") : "";

                        nRow++;
                        STT++;
                    }
                    var listCC = _dbContext.HRM_ChungChiNhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var CCSheet = package.Workbook.Worksheets["Chứng chỉ"];
                    var listNhomChungChi = _dbContext.HRM_dmNhomChungChi.ToList();
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listCC)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        CCSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        CCSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        CCSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        CCSheet.Cells[nRow, ncol].Value = item.TenChungChi;

                        ncol++;
                        var TrinhDo = listNhomChungChi.FirstOrDefault(a => a.IdGuid == item.IdNhomChungChi);
                        if (TrinhDo == null)
                            TrinhDo = new HRM_dmNhomChungChi();
                        CCSheet.Cells[nRow, ncol].Value = TrinhDo.Ma;
                        ncol++;
                        CCSheet.Cells[nRow, ncol].Value = TrinhDo.Ten;
                        ncol++;
                        CCSheet.Cells[nRow, ncol].Value = item.TrinhDo;
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = item.NgayCap != null ? item.NgayCap.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = item.NgayHetHan != null ? item.NgayHetHan.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        BCSheet.Cells[nRow, ncol].Value = item.NoiCap;

                        nRow++;
                        STT++;
                    }
                    var listNN = _dbContext.HRM_NgonNguNhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var NNSheet = package.Workbook.Worksheets["Ngôn ngữ"];
                    var listdmNN = _dbContext.HRM_dmNgonNgu.ToList();
                    var xepLoai = _dbContext.HRM_dmXepLoai.ToList();
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listNN)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        NNSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;

                        ncol++;
                        var TrinhDo = listdmNN.FirstOrDefault(a => a.IdGuid == item.IdNgonNgu);
                        if (TrinhDo == null)
                            TrinhDo = new HRM_dmNgonNgu();
                        NNSheet.Cells[nRow, ncol].Value = TrinhDo.Ma;
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = TrinhDo.Ten;
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = xepLoai.Where(a => a.IdGuid == item.IdXepLoaiNghe).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = xepLoai.Where(a => a.IdGuid == item.IdXepLoaiNoi).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = xepLoai.Where(a => a.IdGuid == item.IdXepLoaiDoc).Select(a => a.Ma).FirstOrDefault();
                        ncol++;
                        NNSheet.Cells[nRow, ncol].Value = xepLoai.Where(a => a.IdGuid == item.IdXepLoaiViet).Select(a => a.Ma).FirstOrDefault();
                        nRow++;
                        STT++;
                    }

                    var listKSK = _dbContext.HRM_NhanVien_KhamSucKhoe.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    var KSKSheet = package.Workbook.Worksheets["Khám Sức khỏe"];
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listKSK)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        KSKSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = item.NgayKham != null ? item.NgayKham.Value.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = item.NgayKetThuc != null ? item.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = item.NoiKham;
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = item.LoaiHinhKham;
                        ncol++;
                        KSKSheet.Cells[nRow, ncol].Value = item.GhiChu;
                        nRow++;
                        STT++;
                    }
                    var listBHXH = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderBy(a => a.IdNhanVien).ThenBy(a => a.TuNgay).ToList();
                    var BHXHSheet = package.Workbook.Worksheets["BHXH"];
                    nRow = 4;
                    STT = 1;
                    foreach (var item in listBHXH)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        BHXHSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = nhanVien.MaSoThueTNCN;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.TenBoPhan;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.TenChucVu;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.TenCongTy;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.TuNgay.ToString("dd/MM/yyyy");
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.DenNgay.Year > 1970 ? item.DenNgay.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = nhanVien.SoBaoHiemXaHoi;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.MucLuongDong;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.TienLuong;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.HeSo;
                        ncol++;
                        BHXHSheet.Cells[nRow, ncol].Value = item.GhiChu;

                        nRow++;

                        STT++;
                    }
                    var listBHCN = _dbContext.HRM_BaoHiem_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && string.IsNullOrEmpty(a.IdHopDong)).OrderBy(a => a.IdNhanVien).ToList();
                    var BHCNSheet = package.Workbook.Worksheets["Bảo hiểm con người"];
                    var listNoiCapBH = _dbContext.HRM_dmNoiCapBaoHiem.ToList();
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listBHCN)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        BHCNSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        BHCNSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        BHCNSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;
                        ncol++;
                        if (item.NgayBatDau != null)
                            BHCNSheet.Cells[nRow, ncol].Value = item.NgayBatDau.Value.Year > 1970 ? item.NgayBatDau.Value.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        if (item.NgayKetThuc != null)
                            BHCNSheet.Cells[nRow, ncol].Value = item.NgayKetThuc.Value.Year > 1970 ? item.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "";

                        ncol++;
                        BHCNSheet.Cells[nRow, ncol].Value = item.SoSoBaoHiem;
                        ncol++;
                        var noiCap = listNoiCapBH.Where(a => a.IdGuid == item.NoiCapSoBaoHiem).FirstOrDefault();
                        if (noiCap != null)
                            BHCNSheet.Cells[nRow, ncol].Value = noiCap.Ma;
                        ncol++;
                        if (noiCap != null)
                            BHCNSheet.Cells[nRow, ncol].Value = noiCap.Ten;
                        ncol++;

                        BHCNSheet.Cells[nRow, ncol].Value = item.MucDongBaoHiem;
                        ncol++;
                        BHCNSheet.Cells[nRow, ncol].Value = item.SoTienBaoHiem;
                        ncol++;
                        BHCNSheet.Cells[nRow, ncol].Value = item.Giam == 1 ? "Có" : "Không";
                        ncol++;
                        BHCNSheet.Cells[nRow, ncol].Value = item.GhiChu;

                        nRow++;

                        STT++;
                    }
                    var listBHTN = _dbContext.HRM_NhanVien_BaoHiemTaiNan.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderBy(a => a.IdNhanVien).ToList();
                    var BHTNSheet = package.Workbook.Worksheets["Bảo hiểm tai nạn"];
                    nRow = 3;
                    STT = 1;
                    foreach (var item in listBHTN)
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        int ncol = 1;
                        BHTNSheet.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        BHTNSheet.Cells[nRow, ncol].Value = nhanVien.MaNhanVien;
                        ncol++;
                        BHTNSheet.Cells[nRow, ncol].Value = nhanVien.TenDayDu;

                        ncol++;
                        BHTNSheet.Cells[nRow, ncol].Value = item.SoTienBaoHiem;
                        ncol++;
                        BHTNSheet.Cells[nRow, ncol].Value = item.SoHopDong;
                        ncol++;
                        if (item.NgayBatDau != null)
                            BHTNSheet.Cells[nRow, ncol].Value = item.NgayBatDau.Value.Year > 1970 ? item.NgayBatDau.Value.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        if (item.NgayBatDau != null)
                            BHTNSheet.Cells[nRow, ncol].Value = item.NgayKetThuc.Value.Year > 1970 ? item.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "";
                        ncol++;
                        var noiCap = listNoiCapBH.Where(a => a.IdGuid == item.NoiCapSoBaoHiem).FirstOrDefault();
                        if (noiCap != null)
                            BHTNSheet.Cells[nRow, ncol].Value = noiCap.Ma;
                        ncol++;
                        if (noiCap != null)
                            BHTNSheet.Cells[nRow, ncol].Value = noiCap.Ten;
                        ncol++;
                        BHTNSheet.Cells[nRow, ncol].Value = item.Giam == 1 ? "TĂNG" : "GIẢM";
                        ncol++;
                        BHTNSheet.Cells[nRow, ncol].Value = item.GhiChu;

                        nRow++;

                        STT++;
                    }
                    var dmLoaiNhanViens = _dbContext.HRM_dmLoaiNhanVien.ToList();
                    var dmQuanHeGiaDinhs = _dbContext.HRM_dmQuanHeGiaDinh.ToList();
                    var dmTinhs = _dbContext.HRM_dmTinh.ToList();
                    var dmQuan = _dbContext.HRM_dmQuanHuyen.ToList();
                    var dmXaPhuong = _dbContext.HRM_dmPhuongXa.ToList();
                    var dmQuocTinh = _dbContext.HRM_dmQuocTich.ToList();
                    var dmDanToc = _dbContext.HRM_dmDanToc.ToList();
                    var dmTrinhDo = _dbContext.HRM_dmTrinhDoDaoTao.ToList();
                    var dmChungChi = _dbContext.HRM_dmNhomChungChi.ToList();
                    var dmNgonNgu = _dbContext.HRM_dmNgonNgu.ToList();
                    var dmTinhTrangHonNhan = _dbContext.HRM_dmTinhTrangHonNhan.ToList();
                    var DmNoiCap = _dbContext.HRM_dmNoiCap.ToList();
                    var DmNoiCapBH = _dbContext.HRM_dmNoiCapBaoHiem.ToList();

                    ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh Mục"];
                    nRow = 3;

                    foreach (var item in dmDanToc)
                    {
                        sheet1.Cells[nRow, 1].Value = item.Ten;
                        sheet1.Cells[nRow, 2].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in dmTrinhDo)
                    {
                        sheet1.Cells[nRow, 3].Value = item.Ten;
                        sheet1.Cells[nRow, 4].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in dmTinhTrangHonNhan)
                    {
                        sheet1.Cells[nRow, 5].Value = item.Ten;
                        sheet1.Cells[nRow, 6].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in dmChungChi)
                    {
                        sheet1.Cells[nRow, 7].Value = item.Ten;
                        sheet1.Cells[nRow, 8].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;

                    foreach (var item in dmQuocTinh)
                    {
                        sheet1.Cells[nRow, 9].Value = item.Ten;
                        sheet1.Cells[nRow, 10].Value = item.Ma;
                        nRow++;

                    }

                    nRow = 3;
                    foreach (var item in dmNgonNgu)
                    {
                        sheet1.Cells[nRow, 11].Value = item.Ten;
                        sheet1.Cells[nRow, 12].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in dmQuanHeGiaDinhs)
                    {
                        sheet1.Cells[nRow, 13].Value = item.Ten;
                        sheet1.Cells[nRow, 14].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in dmLoaiNhanViens)
                    {
                        sheet1.Cells[nRow, 15].Value = item.Ten;
                        sheet1.Cells[nRow, 16].Value = item.Ma;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in DmNoiCap)
                    {
                        sheet1.Cells[nRow, 19].Value = item.Ten;
                        sheet1.Cells[nRow, 20].Value = item.Ma;
                        nRow++;
                    }
                    var listCongTY = _CallWebService.GetDanhSachDuAn(_dbContext);
                    nRow = 3;
                    foreach (var item in listCongTY)
                    {
                        sheet1.Cells[nRow, 21].Value = item.TenBoPhan;
                        sheet1.Cells[nRow, 22].Value = item.MaBoPhan;
                        nRow++;
                    }

                    nRow = 3;
                    foreach (var item in listBoPhan)
                    {
                        sheet1.Cells[nRow, 23].Value = item.TenBoPhan;
                        sheet1.Cells[nRow, 24].Value = item.MaBoPhan;
                        nRow++;
                    }

                    nRow = 3;

                    foreach (var item in listChucVu)
                    {
                        sheet1.Cells[nRow, 25].Value = item.TenChucVu;
                        sheet1.Cells[nRow, 26].Value = item.MaChucVu;
                        nRow++;
                    }

                    nRow = 3;

                    foreach (var item in listChucDanh)
                    {
                        sheet1.Cells[nRow, 27].Value = item.Ten;
                        sheet1.Cells[nRow, 28].Value = item.Ma;
                        nRow++;
                    }
                    nRow = 3;

                    foreach (var item in DmNoiCapBH)
                    {
                        sheet1.Cells[nRow, 29].Value = item.Ten;
                        sheet1.Cells[nRow, 30].Value = item.Ma;
                        nRow++;
                    }


                    dynamic result = new DynamicObject();

                    string name = "importThongTinLienQuanNhanVienDA.xlsx";
                    var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                    string pathresult = PathUploads + filenamegui;
                    File.WriteAllBytes(pathresult, package.GetAsByteArray());

                    HDFile newfile = new HDFile();
                    newfile.Name = filenamegui;
                    newfile.NameLocal = name;

                    newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                                + "&path=" + dungchung.Base64Encode(pathresult);
                    result.file = newfile;
                    s.Close();
                    return result;
                }
                else

                {
                    dynamic result = new DynamicObject();

                    string name = "importThongTinLienQuanNhanVienDA.xlsx";
                    var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                    string pathresult = PathUploads + filenamegui;
                    File.WriteAllBytes(pathresult, package.GetAsByteArray());

                    HDFile newfile = new HDFile();
                    newfile.Name = filenamegui;
                    newfile.NameLocal = name;

                    newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                                + "&path=" + dungchung.Base64Encode(pathresult);
                    result.file = newfile;
                    return result;
                }

            }
            catch(Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "lỗi xuất excel" + ex.ToString());
            }


        }

        #endregion
        public dynamic UpdateCapNhatThongTinHopDong(HRM_NhanVien model, ApplicationDbContext db)
        {
            MessageReturn message = SetThongTinNhanVien(model, db);

            if (message.Error == DefineData.eErrorMessage.CapNhatThanhCong)
            {
                string HieuLuc = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
                var listHopDongHieuLuc = db.HRM_HopDong.Where(r => r.IdNhanVien == model.IdGuid && r.TrangThai == HieuLuc).ToList();
                foreach (var item in listHopDongHieuLuc)
                {
                    item.TenNhanVien = model.TenDayDu;
                    item.IddmTrinhDoDaoTao = model.IddmTrinhDo;
                    item.DienThoai = model.DienThoai;
                    db.Entry(item).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
            return message;
        }
        public dynamic GetDanhSachNhanViecByBoPhan(long IdCongTy, ApplicationDbContext db, long IdBoPhan = 0, long IdChucVu = 0)
        {
            List<string> listIdNhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == IdCongTy).Select(a => a.IdQuyTrinh).Distinct().ToList();
            var lstNhanVienBoPhan = (from a in db.HRM_NhanVien
                                         //join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
                                     where listIdNhanVien.Contains(a.IdGuid) && (IdBoPhan == 0 || a.IdBoPhanEOS == IdBoPhan) && (IdChucVu == 0 || a.IdChucVu == IdChucVu) && /*b.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() &&*/ a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() /*&& b.isPhuLuc == false*/
                                     select new NhanVienBoPhanViewModel
                                     {
                                         MaNhanVien = a.MaNhanVien,
                                         IdNhanVien = a.IdGuid,
                                         //IdHopDong = b.IdGuid,
                                         TenNhanVien = a.TenDayDu,
                                         IdBoPhanEOS = a.IdBoPhanEOS,
                                         IddmChucVu = a.IdChucVu,
                                         IddmChucDanh = a.IdChucDanh,
                                         IdChucVuEOS = a.IdChucVu,
                                         IdUser = a.IdUser

                                     }).ToList();
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            foreach (var item in lstNhanVienBoPhan)
            {
                item.TenChucVu = listChucVu.Where(r => r.Id == item.IddmChucVu).Select(r => r.TenChucVu).FirstOrDefault();
                item.TenBoPhan = listBoPhan.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
            }
            return lstNhanVienBoPhan;
        }
        //public double GetSoNgayPhepNamConLai(long IdNhanVienEOS, ApplicationDbContext db)
        //{
        //    double SoNgayPhepConLai = 0;
        //    double SoNgayPhep = 0;
        //    double SoNgayPhepDaSuDung = 0;
        //    DateTime DateNow = DateTime.Now;
        //    var HopDongNhanVien = (from a in db.HRM_HopDong
        //                           where a.IdNhanVienEOS == IdNhanVienEOS && a.TrangThai == "HieuLuc"
        //                           select new
        //                           {
        //                               a.NgayHieuLuc,
        //                               a.NgayKy,
        //                               a.IdNhanVienEOS,
        //                               a.NgayHetHan,
        //                           }).FirstOrDefault();
        //    double month = DateNow.Month;
        //    if (HopDongNhanVien != null)
        //    {
        //        if (HopDongNhanVien.NgayHetHan.Year < 2000)
        //        {

        //            SoNgayPhep = month;
        //        }
        //        if (HopDongNhanVien.NgayHieuLuc.Year < DateNow.Year && HopDongNhanVien.NgayHetHan.Year > 2000)
        //        {
        //            SoNgayPhep = month;
        //        }
        //        else if (HopDongNhanVien.NgayHieuLuc.Year < DateNow.Year && HopDongNhanVien.NgayHetHan.Year > 2000)
        //        {
        //            if (HopDongNhanVien.NgayHieuLuc.Day < 15)
        //                SoNgayPhep = month - HopDongNhanVien.NgayHieuLuc.Month;
        //            else
        //                SoNgayPhep = month - HopDongNhanVien.NgayHieuLuc.Month;
        //        }
        //    }
        //    var lstHinhThucNghi = db.HRM_dmHinhThucNghi.ToList();

        //    string IdHinhThucNghi = lstHinhThucNghi.FirstOrDefault(r => r.Ma == DefineData.eMaNghiPhep.HL.ToString()).IdGuid;

        //    string IdTrangThaiCuoi = _CallWebService.GetTrangThaiCuoi(db, DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString());
        //    var smgaynghi = (from a in smartdb.QT_QuyTrinhXinNghiPhep
        //                     join b in smartdb.QT_XinNghiPhep_ChiTietNgayNghi on a.Id equals b.IdQuyTrinh
        //                     join c in smartdb.NhanViens on a.IdNguoiDeNghi equals c.UserPkid
        //                     where c.Id == IdNhanVienEOS && b.IdHinhThucNghi == IdHinhThucNghi && a.IdTrangThai == IdTrangThaiCuoi && a.Loai != true && a.ThoiGianBatDau.Year == DateNow.Year
        //                     select new { b.ThoiGianNghi }).ToList();
        //    SoNgayPhepDaSuDung = smgaynghi.Sum(r => r.ThoiGianNghi);

        //    SoNgayPhepConLai = SoNgayPhep - SoNgayPhepDaSuDung;
        //    if (SoNgayPhepConLai < 0)
        //        SoNgayPhepConLai = 0;
        //    return SoNgayPhepConLai;

        //}
        #region log nhân viên 
        public dynamic GetDanhSachLichSuNhanVien(dynamic dyn, ApplicationDbContext db)
        {
            string IdNhanVien = dyn.IdNhanVien;
            string TableName = dyn.TableName;
            int CurrentPage = dyn.CurrentPage;
            int nPageSize = dyn.nPageSize;
            double TuNgayUnix = dyn.TuNgayUnix ?? 0;
            double ToiNgayUnix = dyn.ToiNgayUnix ?? 0;

            string KeyWord = dyn.KeyWord;
            IQueryable<HRM_NhatKyNhanVien> query = db.HRM_NhatKyNhanVien.Where(a => a.IdNhanVien == IdNhanVien);
            if (TuNgayUnix > 0)
            {
                DateTime TuNgay = UnixTimeStampToDateTime(TuNgayUnix).Date;
                query = query.Where(a => a.ThoiGian.Date >= TuNgay);
            }
            if (ToiNgayUnix > 0)
            {
                DateTime ToiNgay = UnixTimeStampToDateTime(ToiNgayUnix).Date;
                query = query.Where(a => a.ThoiGian.Date <= ToiNgay);
            }
            if (!string.IsNullOrEmpty(KeyWord))
                query = query.Where(a => a.QuyTrinh.Contains(KeyWord));
            if (!string.IsNullOrEmpty(TableName))
                query = query.Where(a => a.TableName.Contains(TableName));

            var projected = query
                .OrderByDescending(a => a.ThoiGian)
                .Select(a => new NhatKyNhanVienViewModel
                {
                    IdGuid = a.IdGuid,
                    IdNhanVien = a.IdNhanVien,
                    IdTable = a.IdTable,
                    QuyTrinh = a.QuyTrinh,
                    ThoiGian = a.ThoiGian,
                    TableName = a.TableName,
                    Link = a.Link,
                    eAction = a.eAction
                });

            List<NhatKyNhanVienViewModel> lstNhatKy = Paginate(projected, CurrentPage, nPageSize, out PagingInfor paging);

            var fileLookup = GetFileDinhKemByIds(db, DefineData.eTable.HRM_NhatKyNhanVien.ToString(), lstNhatKy.Select(x => x.IdGuid));
            foreach (var item in lstNhatKy)
            {
                item.ThoiGianUnix = DateTimeToUnixTimeStamp(item.ThoiGian);
                item.listFileDinhKem = fileLookup[item.IdGuid].ToList();
            }
            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = lstNhatKy;
            return dnm;


        }
        public dynamic GetLichSuNhanVien(ApplicationDbContext db, string Id)
        {
            var item = db.HRM_NhatKyNhanVien.Find(Id);
            if (item == null) return null;
            item.ThoiGianUnix = DateTimeToUnixTimeStamp(item.ThoiGian);
            item.listFileDinhKem = GetListFileDinhKem(db, DefineData.eTable.HRM_NhatKyNhanVien.ToString(), item.IdGuid);
            return item;
        }
        public dynamic SetLichSuNhanVien(HRM_NhatKyNhanVien model, ApplicationDbContext db)
        {
            try
            {
                if (model.listFileDinhKem != null)
                {
                    foreach (HRM_FileDinhKem FileDinhKem in model.listFileDinhKem)
                    {
                        SetFileDinhKem(db, FileDinhKem, DefineData.eTable.HRM_NhatKyNhanVien, model.IdGuid);
                    }
                }

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật  thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật không thành công " + ex.Message);
            }


        }
        public dynamic ExportMauNhatKyNhanVien(ApplicationDbContext db)
        {

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ImportNhatkyNhanVien.xlsx");
            var filenamegui = "ImportNhatkyNhanVien.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();


            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportNhanVienDongBHXH(List<DanhSachNhanVienViewModel> listNhanVien)
        {

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ExportDanhSachNhanVienLuongHuu.xlsx");
            var filenamegui = "ExportDanhSachNhanVienDongDuBHXH.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            int nRow = 4;
            int STT = 1;
            int nCol = 1;
            foreach (var item in listNhanVien)
            {
                nCol = 1;
                sheet1.Cells[nRow, nCol].Value = STT;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SoGen;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.GioiTinh;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenChucVu;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenChucDanh;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.DienThoai;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SoNamDongBHXH;

                nRow++;
                STT++;
            }
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            if (nRow > 4)
            {
                nRow--;
                DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                sheet1.Cells[4, 4, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 7, nRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, nCol, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            }
            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportDanhSachNhanVienHetTuoiLaoDong(List<DanhSachNhanVienViewModel> listNhanVien)
        {

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ExportDanhSachNhanVienHetTuoiLaoDong.xlsx");
            var filenamegui = "ExportDanhSachNhanVienHetTuoiLaoDong.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            int nRow = 4;
            int STT = 1;
            int nCol = 1;
            foreach (var item in listNhanVien)
            {
                nCol = 1;
                sheet1.Cells[nRow, nCol].Value = STT;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.SoGen;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.GioiTinh;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenChucVu;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenChucDanh;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                nCol++;
                sheet1.Cells[nRow, nCol].Value = item.DienThoai;

                nRow++;
                STT++;
            }
            sheet1.Cells.Style.Font.Name = "Times New Roman";
            sheet1.Cells.Style.Font.Size = 12;
            sheet1.Cells.Style.WrapText = true;
            sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            if (nRow > 4)
            {
                nRow--;
                DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                sheet1.Cells[4, 4, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 7, nRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            }
            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public MessageReturn ImportNhatkyNhanVien(ApplicationDbContext _dbContext, string FileName, string IdNhanVien = "")
        {
            using (var ts = _dbContext.Database.BeginTransaction())
            {
                int rowerror = 0;
                try
                {
                    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu nhân viên thành công");
                    //string pathTemplate = @"F:\DevWork\HRM2020\Backend\HRMNew\MauImportExcel\Import thông tin nhân viên.xlsx";
                    string pathTemplate = PathUploads + FileName;
                    // pathTemplate = @"D:\importThongTinLienQuanNhanVienDA (1).xlsx";
                    if (File.Exists(pathTemplate) == false)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
                    Stream s = File.OpenRead(pathTemplate);
                    ExcelPackage package = new ExcelPackage(s);
                    s.Close();
                    var sheet1 = package.Workbook.Worksheets.First();
                    List<HRM_NhatKyNhanVien> listAdd = new List<HRM_NhatKyNhanVien>();
                    int maxrow = sheet1.Dimension.Rows;
                    DateTime time = DateTime.Now;
                    for (int i = 4; i <= maxrow; i++)
                    {
                        HRM_NhatKyNhanVien item = new HRM_NhatKyNhanVien();
                        item.MaNhanVien = dungchung.GetCell(sheet1, i, 2).Trim();
                        item.TableName = nameof(HRM_QuyetDinhDieuChinhLuong);
                        item.QuyTrinh = dungchung.GetCell(sheet1, i, 4);
                        if (string.IsNullOrEmpty(item.QuyTrinh))
                            break;
                        item.ThoiGian = dungchung.GetCellToDateTime(sheet1, i, 5) ?? time;
                        item.IdNhanVien = IdNhanVien;
                        item.IdGuid = Guid.NewGuid().ToString();
                        double NoiDung = dungchung.GetCellToDouble(sheet1, i, 6);
                        item.QuyTrinh = item.QuyTrinh + " - Mức lương cũ:" + string.Format("{0:0,0 VNĐ}", NoiDung);
                        NoiDung = dungchung.GetCellToDouble(sheet1, i, 7);
                        item.QuyTrinh = item.QuyTrinh + " - Mức lương mới:" + string.Format("{0:0,0 VNĐ}", NoiDung);
                        NoiDung = dungchung.GetCellToDouble(sheet1, i, 8);
                        item.QuyTrinh = item.QuyTrinh + " - chêch lệch:" + string.Format("{0:0,0 VNĐ}", NoiDung);

                        listAdd.Add(item);
                    }
                    if (string.IsNullOrEmpty(IdNhanVien))
                    {
                        var listMa = listAdd.Select(a => a.MaNhanVien).Distinct().ToList();
                        var listNhanVien = _dbContext.HRM_NhanVien.Where(a => listMa.Contains(a.MaNhanVien)).ToList();
                        if (listNhanVien == null)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã Nhân viên không tồn tại : " + listMa.Join(", "));
                        if (listMa.Count > listNhanVien.Count)
                        {

                            List<string> listMaNv = listNhanVien.Select(a => a.MaNhanVien).Distinct().ToList();
                            var list = listMa.Where(a => !listMaNv.Contains(a)).ToList();
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã Nhân viên không tồn tại : " + list.Join(", "));
                        }
                        foreach (var item in listAdd)
                        {
                            item.IdNhanVien = listNhanVien.FirstOrDefault(a => a.MaNhanVien == item.MaNhanVien).IdGuid;
                        }

                    }
                    _dbContext.HRM_NhatKyNhanVien.AddRange(listAdd);
                    _dbContext.SaveChanges();

                    ts.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString(), rowerror);
                }
            }
        }
        public dynamic GetDanhSachQuyTrinhNhatKy(ApplicationDbContext db)
        {
            try
            {
                List<DanhMuc> list = new List<DanhMuc>();
                list.Add(new DanhMuc { Ma = nameof(HRM_HopDong), Ten = "Ký kết hợp đồng" });
                list.Add(new DanhMuc { Ma = nameof(HRM_QuyetDinhKyLuat), Ten = "Quyết định kỷ luật" });
                list.Add(new DanhMuc { Ma = nameof(HRM_QuyetDinhSaThai), Ten = "Quyết định sa thải" });
                list.Add(new DanhMuc { Ma = nameof(HRM_QuyetDinhDieuChinhLuong), Ten = "Quyết định điều chỉnh lương" });
                list.Add(new DanhMuc { Ma = nameof(HRM_BoNhiemChucVu), Ten = "Bổ nhiệm chức vụ" });
                list.Add(new DanhMuc { Ma = nameof(HRM_QuyetDinhNghiViec), Ten = "Xin nghỉ việc" });
                list.Add(new DanhMuc { Ma = nameof(HRM_BoNhiemDieuDongDuAn), Ten = "Bổ nhiệm điều động dự án" });
                list.Add(new DanhMuc { Ma = "HRM_ChuongTrinhDaoTao", Ten = "Chương trình đào tạo" });
                list.Add(new DanhMuc { Ma = "HRM_QuyTrinhXacNhanTangGiamBaoHiem", Ten = "Xác nhận tăng giảm bảo hiểm" });


                return list;
            }
            catch (Exception)
            {

                return false;
            }

        }
        #endregion
        #region getnhanVien 
        public dynamic GetDanhSachNhanVienByCongTy(long IdCongTyEOS, ApplicationDbContext db)
        {
            List<string> listIdNhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
            string TrangThaiDangThucHien = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var nhanviens = (from a in db.HRM_NhanVien
                             where a.TrangThai != "DaNghiViec" && listIdNhanVien.Contains(a.IdGuid)

                             select
                             new NhanVienCongTyViewModel
                             {
                                 IdGuid = a.IdGuid,
                                 TenDayDu = a.TenDayDu,
                                 MaNhanVien = a.MaNhanVien,
                                 IdUser = a.IdUser,


                             }).ToList();
            listIdNhanVien = nhanviens.Select(r => r.IdGuid).ToList();
            var listIdNhanVienEOS = nhanviens.Select(r => r.IdNhanVienEOS).ToList();
            if (nhanviens.Count > 0)
                return nhanviens;
            else
                return new List<NhanVienCongTyViewModel>();
        }
        #endregion
        public dynamic ExportSoYeuLyLich(string Id, ApplicationDbContext db)
        {
            try
            {
                var listDMQuanHe = db.HRM_dmQuanHeGiaDinh.ToList();
                var filenamegui = Guid.NewGuid().ToString();
                string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/MauSoYeuLyLich.docx");
                string sFileNameCopy = PathUploads + filenamegui + ".docx";
                HRM_NhanVien nhanVien = db.HRM_NhanVien.Find(Id);
                File.Copy(fileName, sFileNameCopy, true);

                var doc = Xceed.Words.NET.DocX.Load(sFileNameCopy);

                doc.ReplaceText("@HoTenDayDu", nhanVien.TenDayDu.ToUpper());
                doc.ReplaceText("@GioiTinh", nhanVien.IdGioiTinh);

                doc.ReplaceText("@NgaySinh", nhanVien.NgaySinh.ToString("dd"));
                doc.ReplaceText("@ThangSinh", nhanVien.NgaySinh.ToString("MM"));
                doc.ReplaceText("@NamSinh", nhanVien.NgaySinh.ToString("yyyy"));

                //doc.ReplaceText("@SCMTND", nhanVien.SoChungMinhThu);
                //doc.ReplaceText("@NoiCapCMTND", nhanVien.ChungMinhThuNoiCap);
                //if (nhanVien.ChungMinhThuNgayCap != null)
                //{
                //    doc.ReplaceText("@NgayCMT", nhanVien.ChungMinhThuNgayCap.Value.ToString("dd"));
                //    doc.ReplaceText("@ThangCMT", nhanVien.ChungMinhThuNgayCap.Value.ToString("MM"));
                //    doc.ReplaceText("@NamCMT", nhanVien.ChungMinhThuNgayCap.Value.ToString("yyyy"));
                //}
                if (nhanVien.DienThoaiNhaRieng == null)
                    nhanVien.DienThoaiNhaRieng = "";

                doc.ReplaceText("@SoDienThoaiNR", nhanVien.DienThoaiNhaRieng);
                var SDT = db.HRM_NhanVien_SoDienThoai.FirstOrDefault(a => a.IdNhanVien == nhanVien.IdGuid && a.isApDung == true);
                var GiayTo = db.HRM_NhanVien_GiayToTuyThan.FirstOrDefault(a => a.IdNhanVien == nhanVien.IdGuid && a.isApDung == true);
                if (GiayTo != null)
                {
                    doc.ReplaceText("@SCMTND", GiayTo.So);
                    var noiCap = db.HRM_dmNoiCap.FirstOrDefault(a => a.IdGuid == GiayTo.NoiCap);
                    if (noiCap != null)
                        doc.ReplaceText("@NoiCapCMTND", noiCap.Ten);
                    else
                        doc.ReplaceText("@NoiCapCMTND", "");
                    doc.ReplaceText("@NgayCapCMT", GiayTo.NgayCap == null ? "" : GiayTo.NgayCap.Value.ToString("dd/MM/yyyy"));
                }
                if (SDT != null)
                    doc.ReplaceText("@SoDienThoaiDD", SDT.SoDienThoai);
                doc.ReplaceText("@NamSinh", nhanVien.NgaySinh.ToString("yyyy"));
                doc.ReplaceText("@NoiDangKyHoKhauThuongTru", nhanVien.HoKhauThuongTruDiaChi);
                string QuanHe = listDMQuanHe.Where(r => r.IdGuid == nhanVien.LienHeKhanIdQuanHe).Select(r => r.Ten).FirstOrDefault();
                string LienHe = QuanHe + " " + nhanVien.LienHeKhanCapHoTen + " điện thoại: " + nhanVien.LienHeKhanDienThoai + " địa chỉ: " + nhanVien.LienHeKhanDiaChi;
                doc.ReplaceText("@NguoiLienHeKhanCap", LienHe);
                doc.ReplaceText("@HoTenNhanVien", nhanVien.TenDayDu);
                if (nhanVien.ChoOHienNayTruThonXom == null)
                    nhanVien.ChoOHienNayTruThonXom = "";
                doc.ReplaceText("@ChoOHienTai", nhanVien.ChoOHienNayTruDiaChi);
                string DanToc = db.HRM_dmDanToc.Where(r => r.IdGuid == nhanVien.IdDanToc).Select(r => r.Ten).FirstOrDefault();
                if (DanToc == null)
                    DanToc = "";
                doc.ReplaceText("@DanToc", DanToc);

                if (nhanVien.TonGiao == null)
                    nhanVien.TonGiao = "";
                doc.ReplaceText("@TonGiao", nhanVien.TonGiao);

                string NoiSinhNhanVien = db.HRM_dmTinh.Where(r => r.IdGuid == nhanVien.HoKhauThuongTruIdTinhThanhPho).Select(r => r.Ten).FirstOrDefault();
                if (NoiSinhNhanVien == null)
                    NoiSinhNhanVien = "";
                doc.ReplaceText("@NoiSinhNhanVien", NoiSinhNhanVien);
                string TrinhDoVanHoa = db.HRM_dmTrinhDoDaoTao.Where(r => r.IdGuid == nhanVien.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
                if (TrinhDoVanHoa == null)
                    TrinhDoVanHoa = "";

                doc.ReplaceText("@TrinhDoVanHoa", TrinhDoVanHoa);


                var listNgonNguNhanVien = db.HRM_NgonNguNhanVien.Where(r => r.IdNhanVien == nhanVien.IdGuid).ToList();
                string NgoaiNgu = "";
                foreach (var item in listNgonNguNhanVien)
                {
                    string TenNgonNgu = db.HRM_dmNgonNgu.Where(r => r.IdGuid == item.IdNgonNgu).Select(r => r.Ten).FirstOrDefault();
                    NgoaiNgu = NgoaiNgu + "; " + TenNgonNgu;
                }
                doc.ReplaceText("@NgoaiNgu", NgoaiNgu);


                doc.ReplaceText("@TrinhDo", TrinhDoVanHoa);
                var listTrinhDoNhanVien = db.HRM_NhanVienBangCap.Where(r => r.IdNhanVien == nhanVien.IdGuid).ToList();
                string ChuyenNganh = "";
                if (listTrinhDoNhanVien.Count > 0)
                    ChuyenNganh = listTrinhDoNhanVien.Where(r => r.IdTrinhDoDaoTao == nhanVien.IddmTrinhDo).Select(r => r.ChuyenNganh).FirstOrDefault();
                if (ChuyenNganh == null)
                    ChuyenNganh = "";
                doc.ReplaceText("@ChuyenNganh", ChuyenNganh);
                var listNVQuaHeGiaDinh = db.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == nhanVien.IdGuid).ToList();

                /// Cha
                string IddmQHGD_Cha = listDMQuanHe.Where(r => r.Ma == "BO").Select(r => r.IdGuid).FirstOrDefault();
                var NHanVienCha = listNVQuaHeGiaDinh.Where(r => r.IdQuanHe == IddmQHGD_Cha).FirstOrDefault();
                string HoTenBo = "";
                string TuoiBo = "";
                string NgheNghiepBo = "";
                DateTime dateTimenow = DateTime.Now;
                if (NHanVienCha != null)
                {
                    if (NHanVienCha.HoTen != null)
                        HoTenBo = NHanVienCha.HoTen;
                    TuoiBo = (dateTimenow.Year - NHanVienCha.NgaySinh.Value.Year).ToString();
                    if (NHanVienCha.NgheNghiep != null)
                        NgheNghiepBo = NHanVienCha.NgheNghiep;
                }
                doc.ReplaceText("@HoTenBo", HoTenBo);
                doc.ReplaceText("@TuoiBo", TuoiBo);
                doc.ReplaceText("@NgheNghiepBo", NgheNghiepBo);


                /// Me
                string IddmQHGD_ME = listDMQuanHe.Where(r => r.Ma == "ME").Select(r => r.IdGuid).FirstOrDefault();
                var NHanVienMe = listNVQuaHeGiaDinh.Where(r => r.IdQuanHe == IddmQHGD_ME).FirstOrDefault();
                string HoTenMe = "";
                string TuoiMe = "";
                string NgheNghiepMe = "";

                if (NHanVienMe != null)
                {
                    if (NHanVienMe.HoTen != null)
                        HoTenMe = NHanVienMe.HoTen;
                    TuoiMe = (dateTimenow.Year - NHanVienMe.NgaySinh.Value.Year).ToString();
                    if (NHanVienMe.NgheNghiep != null)
                        NgheNghiepMe = NHanVienMe.NgheNghiep;
                }
                doc.ReplaceText("@HoTenMe", HoTenMe);
                doc.ReplaceText("@TuoiMe", TuoiMe);
                doc.ReplaceText("@NgheNghiepMe", NgheNghiepMe);


                /// Vo

                string IddmQHGD_VO = "";
                if (nhanVien.IdGioiTinh != "Nam" || nhanVien.IdGioiTinh != "NAM" || nhanVien.IdGioiTinh != "nam")
                    IddmQHGD_VO = listDMQuanHe.Where(r => r.Ma == "VO").Select(r => r.IdGuid).FirstOrDefault();
                else
                    IddmQHGD_VO = listDMQuanHe.Where(r => r.Ma == "CHONG").Select(r => r.IdGuid).FirstOrDefault();

                var NHanVienVo = listNVQuaHeGiaDinh.Where(r => r.IdQuanHe == IddmQHGD_VO).FirstOrDefault();
                string HoTenVo = "";
                string TuoiVo = "";
                string NgheNghiepVo = "";
                string NoiLamViecVo = "";
                if (NHanVienVo != null)
                {
                    if (NHanVienVo.HoTen != null)
                        HoTenVo = NHanVienVo.HoTen;
                    TuoiVo = (dateTimenow.Year - NHanVienVo.NamSinh).ToString();
                    if (NHanVienVo.NgheNghiep != null)
                        NgheNghiepVo = NHanVienVo.NgheNghiep;
                    if (NHanVienVo.NoiLamViec != null)
                        NoiLamViecVo = NHanVienVo.NoiLamViec;
                }
                doc.ReplaceText("@HoTenVo", HoTenVo);
                doc.ReplaceText("@TuoiVo", TuoiVo);
                doc.ReplaceText("@NgheNghiepVo", NgheNghiepVo);
                doc.ReplaceText("@NoiLamViecVo", NoiLamViecVo);

                string IddmQHGD_CON = listDMQuanHe.Where(r => r.Ma == "CON").Select(r => r.IdGuid).FirstOrDefault();

                var nhanVienConCais = listNVQuaHeGiaDinh.Where(r => r.IdQuanHe == IddmQHGD_CON).ToList();
                int nCountCC = nhanVienConCais.Count();
                if (nCountCC > 0)
                {
                    List<int> Mas = doc.FindAll("@QuanHeGiaDinhConCaiNhanVien");
                    int index = Mas[0];
                    var t = doc.AddTable((nCountCC + 1), 4);
                    int nRow = 1;
                    t.SetColumnWidth(0, 40);
                    t.SetColumnWidth(1, 200);
                    t.SetColumnWidth(2, 70);
                    t.SetColumnWidth(3, 180);
                    Paragraph headerTable_STT = t.Rows[0].Cells[0].Paragraphs.First().Append("STT");
                    //headerTable_STT.Alignment = Alignment.center;
                    Paragraph headerTable_HoTen = t.Rows[0].Cells[1].Paragraphs.First().Append("Họ tên");
                    //headerTable_HoTen.Alignment = Alignment.center;
                    Paragraph headerTable_Tuoi = t.Rows[0].Cells[2].Paragraphs.First().Append("Tuổi");
                    //headerTable_Tuoi.Alignment = Alignment.center;

                    Paragraph headerTable_ngheNghiep = t.Rows[0].Cells[3].Paragraphs.First().Append("Nghề nghiệp");
                    //headerTable_ngheNghiep.Alignment = Alignment.center;
                    foreach (var item in nhanVienConCais)
                    {
                        t.Rows[nRow].Cells[0].Paragraphs.First().Append((nRow).ToString());
                        t.Rows[nRow].Cells[1].Paragraphs.First().Append(item.HoTen);
                        string Tuoi = (dateTimenow.Year - item.NgaySinh.Value.Year + 1).ToString() + " Tuổi";
                        t.Rows[nRow].Cells[2].Paragraphs.First().Append(Tuoi);
                        string NgheNghiep = item.NgheNghiep;
                        t.Rows[nRow].Cells[3].Paragraphs.First().Append(NgheNghiep);
                        nRow++;
                    }

                    doc.InsertTable(index, t);
                }
                doc.ReplaceText("@QuanHeGiaDinhConCaiNhanVien", "");
                var nhanVienCongTacNhanVien = db.HRM_QuaTrinhCongTac.Where(r => r.IdNhanVien == nhanVien.IdGuid).ToList();
                int nCountCT = nhanVienCongTacNhanVien.Count();
                if (nCountCT > 0)
                {
                    List<int> Mas = doc.FindAll("@QuaTrinhHoatDongNhanVien");
                    int index = Mas[0];
                    var t = doc.AddTable((nCountCT + 1), 4);
                    int nRow = 1;
                    t.SetColumnWidth(0, 100);
                    t.SetColumnWidth(1, 150);
                    t.SetColumnWidth(2, 70);
                    t.SetColumnWidth(3, 130);
                    Paragraph headerTable_STT = t.Rows[0].Cells[0].Paragraphs.First().Append("Từ tháng năm đến tháng năm");
                    //headerTable_STT.Alignment = Alignment.center;
                    Paragraph headerTable_HoTen = t.Rows[0].Cells[1].Paragraphs.First().Append("Làm công tác gì?");
                    //headerTable_HoTen.Alignment = Alignment.center;
                    Paragraph headerTable_Tuoi = t.Rows[0].Cells[2].Paragraphs.First().Append("Ở đâu?");
                    //headerTable_Tuoi.Alignment = Alignment.center;

                    Paragraph headerTable_ngheNghiep = t.Rows[0].Cells[3].Paragraphs.First().Append("Giữ chức vụ gì?");
                    //headerTable_ngheNghiep.Alignment = Alignment.center;
                    foreach (var item in nhanVienCongTacNhanVien)
                    {
                        t.Rows[nRow].Cells[0].Paragraphs.First().Append((nRow).ToString());
                        t.Rows[nRow].Cells[1].Paragraphs.First().Append(item.ThoiGian);

                        t.Rows[nRow].Cells[2].Paragraphs.First().Append(item.DonViCongTac);

                        t.Rows[nRow].Cells[3].Paragraphs.First().Append(item.ViTriCongViec);

                        nRow++;
                    }

                    doc.InsertTable(index, t);
                }
                doc.ReplaceText("@QuaTrinhHoatDongNhanVien", "");
                doc.Save();
                dynamic data = new DynamicObject();
                data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("SoYeuLyLich.docx")
                                + "&path=" + Base64Encode(sFileNameCopy);
                return data;
            }
            catch (Exception exp)
            {
                return exp;
            }

        }
        public dynamic BaoCaoDanhSachNhanVien(SearchNhanVienViewModel model, ApplicationDbContext db)
        {
            int PageSize = model.PageSize;
            DateTime dateTimeNow = DateTime.Now;
            DateTime dateTimeNowMonth = DateTime.Now.AddMonths(1);
            int TuoiNghiHuuNam = HrmOptions?.TuoiNghiHuuNam ?? 60;
            DateTime dateTimeNowNam = dateTimeNowMonth.AddYears(-TuoiNghiHuuNam).Date;
            int TuoiNghiHuuNU = HrmOptions?.TuoiNghiHuuNu ?? 55;
            DateTime dateTimeNowNu = dateTimeNowMonth.AddYears(-TuoiNghiHuuNU).Date;

            var query = from a in db.HRM_NhanVien
                        join b in db.HRM_HopDong on a.IdGuid equals b.IdNhanVien
                        where !b.isPhuLuc && b.isKetThuc
                        select new { a, b };

            if (!string.IsNullOrEmpty(model.KeyWord))
                query = query.Where(x => x.a.TenDayDu.Contains(model.KeyWord) || x.a.MaNhanVien.Contains(model.KeyWord));
            if (model.IdCongTyEOS != 0)
                query = query.Where(x => x.b.IdCongTyEOS == model.IdCongTyEOS);
            if (model.TrangThaiNhanVien == 1)
                query = query.Where(x => x.a.TrangThai == "DangLamViec");
            else if (model.TrangThaiNhanVien == 2)
                query = query.Where(x => x.a.TrangThai == "DaNghiViec");
            else if (model.TrangThaiNhanVien == 3)
                query = query.Where(x => x.a.TrangThai != "DaNghiViec" && (
                    (x.a.IdGioiTinh == "Nữ" && x.a.NgaySinh.Date <= dateTimeNowNu) ||
                    (x.a.IdGioiTinh == "Nam" && x.a.NgaySinh.Date <= dateTimeNowNam)));
            //if (model.TrangThaiHopDong > 0)
            //{
            //    sWhere = sWhere + " and (Year(b.NgayHetHan)<1980 )";
            //}

            var projected = query
                .OrderByDescending(x => x.a.MaNhanVien)
                .Select(x => new BaoCaoNhanVienViewModel
                {
                    TenNhanVien = x.a.TenDayDu,
                    Email = x.a.Email,
                    DiaChi = x.a.HoKhauThuongTruDiaChi,
                    IddmTrinhDo = x.a.IddmTrinhDo,
                    IdCongTyEOS = x.a.IdCongTyEOS,
                    NgayBatDauLamViec = x.a.NgayBatDauLamViec,
                    MaNhanVien = x.a.MaNhanVien,
                    IdNhanVien = x.a.IdGuid,
                    DienThoai = x.a.DienThoai,
                    NgaySinh = x.a.NgaySinh,
                    NgayNghiViec = x.a.NgayNghiViec,
                    TrangThai = x.a.TrangThai,
                    GioiTinh = x.a.IdGioiTinh,
                    NgayHieuLuc = x.b.NgayHieuLuc,
                    NgayHetHan = x.b.NgayHetHan,
                    IddmChucVu = x.b.IddmChucVu
                });

            List<BaoCaoNhanVienViewModel> lsNhanVien = Paginate(projected, model.CurrentPage, PageSize, out PagingInfor paging);
            var BoPhans = _CallWebService.GetDanhSachCongTy(db);
            var dmChucVus = _CallWebService.GetDanhSachChucVu(db);
            var dmTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
            foreach (var item in lsNhanVien)
            {
                if (item.NgaySinh != null)
                    item.NgaySinhUnix = DateTimeToUnixTimeStamp(item.NgaySinh);
                if (item.NgayHieuLuc != null)
                    item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                if (item.NgayHetHan != null)
                    item.NgayHetHanUnix = DateTimeToUnixTimeStamp(item.NgayHetHan);
                if (item.NgayBatDauLamViec != null)
                    item.NgayBatDauLamViecUnix = DateTimeToUnixTimeStamp(item.NgayBatDauLamViec);
                if (item.NgayNghiViec != null)
                    item.NgayNghiViecUnix = DateTimeToUnixTimeStamp(item.NgayNghiViec);
                if (item.TrangThai == "DaNghiViec")
                    item.TenTrangThaiNhanVien = "Đã nghỉ việc";
                if (item.TrangThai == "DangLamViec")
                    item.TenTrangThaiNhanVien = "Đang làm việc";
                if (item.NgayHetHan != null && item.NgayHetHan.Value.Year > 2000 && item.NgayHetHan > dateTimeNow && dateTimeNowMonth > item.NgayHetHan)
                {
                    item.TrangThaiHopDong = 2;
                    item.TenTrangThaiHopDong = "Hợp đồng sắp hết hạn";
                }

                item.TrinhDoChuyenMon = dmTrinhDo.Where(r => r.IdGuid == item.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
                item.TenChucVu = dmChucVus.Where(r => r.Id == item.IddmChucVu).Select(r => r.TenChucVu).FirstOrDefault();
                item.TenCongTyEOS = BoPhans.Where(r => r.Id == item.IdCongTyEOS).Select(r => r.TenBoPhan).FirstOrDefault();
            }
            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = lsNhanVien;
            return dnm;

        }
        public dynamic ExportDanhSachNhanVienByCongTy(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext db)
        {

            int NamNghiViec = 0;
            if (aRequest.TrangThai == "DaNghiViec")
            {
                aRequest.TrangThai = "DaNghiViec";
                NamNghiViec = aRequest.Nam;
            }

            var listNhanVien = (from a in db.HRM_NhanVien
                                where (aRequest.TrangThai == "" || aRequest.TrangThai == null | a.TrangThai == aRequest.TrangThai)
                                && (NamNghiViec == 0 || (a.NgayNghiViec != null && a.NgayNghiViec.Value.Year == NamNghiViec))
                                && (aRequest.IdBoPhanEOS == 0 || a.IdBoPhanEOS == aRequest.IdBoPhanEOS)
                                select a).Distinct().OrderBy(r => r.MaNhanVien).ToList();
            DateTime DateTimeNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            var listIdNhanVien = listNhanVien.Select(r => r.IdGuid).ToList();
            if (aRequest.IdCongTyEOS > 0)
            {
                listIdNhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
            }
            if (aRequest.IdChucDanhEOS > 0)
                listNhanVien = listNhanVien.Where(a => a.IdChucDanh == aRequest.IdChucDanhEOS).ToList();
            if (aRequest.IdChucVuEOS > 0)
                listNhanVien = listNhanVien.Where(a => a.IdChucVu == aRequest.IdChucVuEOS).ToList();
            if (!string.IsNullOrEmpty(aRequest.IdLoaiNhanVien))
                listNhanVien = listNhanVien.Where(a => a.IdLoaiNhanVien == aRequest.IdLoaiNhanVien).ToList();

            var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(r => (r.Loai == 1 || r.Loai == 2) && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
            if (aRequest.isCMT > 0)
            {
                var listGiayTos = listGiayTo.Where(a => a.Loai == 2).ToList();
                listIdNhanVien = listGiayTos.Select(r => r.IdNhanVien).ToList();
                if (aRequest.isCMT == 1)
                {

                    listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }
                else
                {
                    listNhanVien = listNhanVien.Where(a => !listIdNhanVien.Contains(a.IdGuid)).ToList();

                }
                listIdNhanVien = listNhanVien.Select(r => r.IdGuid).ToList();
            }
            if (aRequest.TuNgayUnix > 0)
            {
                DateTime timecheck = dungchung.UnixTimeStampToDateTime(aRequest.TuNgayUnix);
                listNhanVien = listNhanVien.Where(a => a.NgayBatDauLamViec != null && timecheck <= a.NgayBatDauLamViec).ToList();
            }
            if (aRequest.DenNgayUnix > 0)
            {
                DateTime timecheck = dungchung.UnixTimeStampToDateTime(aRequest.DenNgayUnix);
                listNhanVien = listNhanVien.Where(a => a.NgayBatDauLamViec != null && timecheck >= a.NgayBatDauLamViec).ToList();
            }

            var listCongTy_NhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).Distinct().ToList();

            var listHopDong = db.HRM_HopDong.Where(r => listIdNhanVien.Contains(r.IdNhanVien) && r.isKetThuc == true).ToList();
            var listIdHD = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
            var listBaoHiem = db.HRM_BaoHiem_HopDong.Where(a => listIdHD.Contains(a.IdHopDong)).Distinct().ToList();
            var listLuongNhanVien = db.HRM_Luong_NhanVien.Where(r => listIdNhanVien.Contains(r.IdNhanVien) && r.NgayBatDauHieuLuc < DateTimeNow).ToList();
            var listQuaTrinhDaoTaoNhanVien = db.HRM_NhanVienBangCap.Where(r => listIdNhanVien.Contains(r.IdNhanVien)).ToList();

            var listSDT = db.HRM_NhanVien_SoDienThoai.Where(r => r.isApDung == true && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
            var listEmail = db.HRM_NhanVien_Email.Where(r => r.isApDung == true && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var DuAns = _CallWebService.GetDanhSachDuAn(db);
            var ChucVus = _CallWebService.GetDanhSachChucVu(db);
            var ChucDanhs = _CallWebService.GetDanhSachChucDanh(db);
            var listLoaiDuAn = _CallWebService.GetListdmLoaiDuAn(db);
            var dmTinhs = db.HRM_dmTinh.ToList();
            var dmNoiCaps = db.HRM_dmNoiCap.ToList();
            var dmNoiCapBHs = db.HRM_dmNoiCapBaoHiem.ToList();
            var dmQuanHuyens = db.HRM_dmQuanHuyen.ToList();
            var dmXaPhuongs = db.HRM_dmPhuongXa.ToList();
            var dmTrinhDoDaoTaos = db.HRM_dmTrinhDoDaoTao.ToList();
            var dmThoiHanHopDongs = db.HRM_dmThoiHanHopDong.ToList();
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ExportDanhSachNhanVien.xlsx");

            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            DateTime time = DateTime.Now;
            time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            s.Close();
            int nRow = 4;
            int STT = 1;
            int colum = 41;
            // cột R là ngày bắt đâu vào công ty nhớ cộng
            foreach (var item in listNhanVien)
            {
                HRM_HopDong hopDongNhanVienofPhuLuc = listHopDong.Where(r => r.IdNhanVien == item.IdGuid).OrderByDescending(r => r.Created).FirstOrDefault();
                //if (hopDongNhanVienofPhuLuc == null)
                //    continue;
                string NhanVienChucDanh = ChucDanhs.Where(r => r.Id == item.IdChucDanh).Select(r => r.Ten).FirstOrDefault();
                string NhanVienChucVu = ChucVus.Where(r => r.Id == item.IdChucVu).Select(r => r.TenChucVu).FirstOrDefault();
                string NhanVienBoPhans = BoPhans.Where(r => r.Id == item.IdBoPhanEOS).Select(r => r.TenBoPhan).FirstOrDefault();
                var listCty = listCongTy_NhanVien.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).ToList();
                var ctys = DuAns.Where(a => listCty.Contains(a.Id)).ToList();
                var Tenctys = ctys.Select(a => a.TenBoPhan).Distinct().ToList();
                colum = 1;
                sheet1.Cells[nRow, colum].Value = STT;
                colum++; //2
                sheet1.Cells[nRow, colum].Value = item.MaNhanVien;
                colum++; //2
                sheet1.Cells[nRow, colum].Value = item.SoGen;

                colum++;//3
                sheet1.Cells[nRow, colum].Value = item.TenDayDu;

                colum++;//4
                sheet1.Cells[nRow, colum].Value = NhanVienChucVu;
                colum++;//5
                sheet1.Cells[nRow, colum].Value = NhanVienBoPhans;
                colum++;//5
                if (ctys != null && ctys.Count > 0)
                {
                    var listIdLoaiDuAn = ctys.Select(a => a.IddmLoaiDuAn).ToList();
                    var loaiCty = listLoaiDuAn.Where(a => listIdLoaiDuAn.Contains(a.Id)).Select(a => a.Ten).Distinct().ToList();
                    if (loaiCty != null && loaiCty.Count > 0)
                        sheet1.Cells[nRow, colum].Value = string.Join(", ", loaiCty); 
                }
                colum++;//5
                if (Tenctys != null && Tenctys.Count > 0)
                    sheet1.Cells[nRow, colum].Value = string.Join(", ", Tenctys);

                colum++;//6
                sheet1.Cells[nRow, colum].Value = item.IdGioiTinh;
                if (item.IdGioiTinh == "KhongXacDinh")
                    sheet1.Cells[nRow, colum].Value = "Khác";
                colum++;//7
                if (item.NgaySinh != null)
                    sheet1.Cells[nRow, colum].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                colum++;//8
                if (item.NgaySinh != null)
                    sheet1.Cells[nRow, colum].Value = item.NgaySinh.Day;
                colum++;//9
                if (item.NgaySinh != null)
                    sheet1.Cells[nRow, colum].Value = item.NgaySinh.Month;
                colum++;//10
                if (item.NgaySinh != null)
                    sheet1.Cells[nRow, colum].Value = item.NgaySinh.Year;
                colum++;//11
                if (item.NgaySinh != null)
                    sheet1.Cells[nRow, colum].Value = (DateTimeNow.Year - item.NgaySinh.Year + 1);
                colum++;//12

                sheet1.Cells[nRow, colum].Value = item.MaSoThueTNCN;
                colum++;//12

                sheet1.Cells[nRow, colum].Value = item.MaSoThueCMT;
                colum++;//12

                sheet1.Cells[nRow, colum].Value = item.HoKhauThuongTruDiaChi;

                colum++;//13
                var XaPhuongHoKhauThuongTru = dmXaPhuongs.FirstOrDefault(a => a.IdGuid == item.HoKhauThuongTruIdPhuongXa);
                if (XaPhuongHoKhauThuongTru != null)
                    sheet1.Cells[nRow, colum].Value = XaPhuongHoKhauThuongTru.Ten;
                colum++;//14
                var QuanHuyenHoKhauThuongTru = dmQuanHuyens.FirstOrDefault(a => a.IdGuid == item.HoKhauThuongTruIdQuanHuyen);
                if (QuanHuyenHoKhauThuongTru != null)
                    sheet1.Cells[nRow, colum].Value = QuanHuyenHoKhauThuongTru.Ten;
                colum++;//15
                var TinhThanhPhoHoKhauThuongTru = dmTinhs.FirstOrDefault(a => a.IdGuid == item.HoKhauThuongTruIdTinhThanhPho);
                if (TinhThanhPhoHoKhauThuongTru != null)
                    sheet1.Cells[nRow, colum].Value = TinhThanhPhoHoKhauThuongTru.Ten;

                colum++;//16
                sheet1.Cells[nRow, colum].Value = item.ChoOHienNayTruDiaChi;
                colum++;//17
                var XaPhuongChoOHienNayTru = dmXaPhuongs.FirstOrDefault(a => a.IdGuid == item.ChoOHienNayTruIdPhuongXa);
                if (XaPhuongChoOHienNayTru != null)
                    sheet1.Cells[nRow, colum].Value = XaPhuongChoOHienNayTru.Ten;
                colum++;//14
                var QuanHuyenChoOHienNayTru = dmQuanHuyens.FirstOrDefault(a => a.IdGuid == item.ChoOHienNayTruIdQuanHuyen);
                if (QuanHuyenChoOHienNayTru != null)
                    sheet1.Cells[nRow, colum].Value = QuanHuyenChoOHienNayTru.Ten;
                colum++;//15
                var TinhThanhPhoChoOHienNayTru = dmTinhs.FirstOrDefault(a => a.IdGuid == item.ChoOHienNayTruIdTinhThanhPho);
                if (TinhThanhPhoChoOHienNayTru != null)
                    sheet1.Cells[nRow, colum].Value = TinhThanhPhoChoOHienNayTru.Ten;

                colum++;//20
                sheet1.Cells[nRow, colum].Value = listSDT.Where(a => a.IdNhanVien == item.IdGuid).Select(a => a.SoDienThoai).FirstOrDefault();
                colum++;//21
                sheet1.Cells[nRow, colum].Value = listEmail.Where(a => a.IdNhanVien == item.IdGuid).Select(a => a.Email).FirstOrDefault();
                colum++;//22
                string NoiSinh = dmTinhs.Where(r => r.IdGuid == item.HoKhauThuongTruIdTinhThanhPho).Select(r => r.Ten).FirstOrDefault();
                sheet1.Cells[nRow, colum].Value = NoiSinh;
                colum++;//23
                string DaoTaoNhanVien = listQuaTrinhDaoTaoNhanVien.Where(r => r.IdNhanVien == item.IdGuid).Select(r => r.ChuyenNganh).FirstOrDefault();
                sheet1.Cells[nRow, colum].Value = DaoTaoNhanVien;

                colum++;//24
                string TrinhDoDaoTao = dmTrinhDoDaoTaos.Where(r => r.IdGuid == item.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
                sheet1.Cells[nRow, colum].Value = TrinhDoDaoTao;
                colum++;//25

                if (item.NgayBatDauLamViec != null)

                    sheet1.Cells[nRow, colum].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                colum++;//26
                if (item.NgayBatDauLamViec != null)
                    sheet1.Cells[nRow, colum].Value = item.NgayBatDauLamViec.Value.ToString("dd");
                colum++;//27
                if (item.NgayBatDauLamViec != null)
                    sheet1.Cells[nRow, colum].Value = item.NgayBatDauLamViec.Value.ToString("MM");
                colum++;//28
                if (item.NgayBatDauLamViec != null)

                    sheet1.Cells[nRow, colum].Value = item.NgayBatDauLamViec.Value.ToString("yyyy");

                colum++;//29 cột R là ngày bắt đâu vào công ty nhớ cộng 
                double TongSoNgay = 0;
                int SoNam = 0;
                int soThang = 0;
                int SoNgay = 0;
                if (item.NgayBatDauLamViec != null)
                {

                    TongSoNgay = (DateTimeNow - item.NgayBatDauLamViec.Value).TotalDays;
                    if (TongSoNgay > 365)
                        SoNam = (int)(TongSoNgay / 365);
                    int SoNgayConLaiNam = (int)TongSoNgay - SoNam * 365;
                    if (SoNgayConLaiNam > 30.5)
                        soThang = (int)(SoNgayConLaiNam / 30.5);
                    SoNgay = (int)(SoNgayConLaiNam - soThang * 30.5);

                    //string TongSoThang = "=IF(" + item.NgayBatDauLamViec.Value.ToString("MM/dd/yyyy") + ">TODAY(),0,DATEDIF($R" + nRow + ",TODAY(),\"m\"))";
                    //if (item.NgayNghiViec != null)
                    //{
                    //    TongSoThang = "=IF(" + item.NgayBatDauLamViec.Value.ToString("MM/dd/yyyy") + ">TODAY(),0,DATEDIF($R" + nRow + ",\"" + item.NgayNghiViec.Value.ToString("MM/dd/yyyy") + "\",\"m\"))";
                    //}
                    double Thang = TongSoNgay / 30.5;
                    sheet1.Cells[nRow, colum].Value = Thang;
                    sheet1.Cells[nRow, colum].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;

                    //FormulaR1C1 = sum14;
                }
                colum++;//30
                sheet1.Cells[nRow, colum].Value = SoNam;
                colum++;//31
                        //int SoThang = 0;=
                sheet1.Cells[nRow, colum].Value = soThang;
                colum++;//32
                        //int SoNgay = 0;
                sheet1.Cells[nRow, colum].Value = SoNgay;
                colum++;//33
                if (item.NgayBatDauLamViec != null)
                {
                    //string Thamnien = "=DATEDIF(R" + nRow + ",TODAY(),\"y\")&\" năm \"&DATEDIF(R" + nRow + ",TODAY(),\"ym\")&\" tháng \"&DATEDIF(R" + nRow + ",TODAY(),\"md\")&\" ngày\"";
                    //if (item.NgayNghiViec != null)
                    //    Thamnien = "=DATEDIF(R" + nRow + ",\"" + item.NgayNghiViec.Value.ToString("MM/dd/yyyy") + "\",\"y\")&\" năm \"&DATEDIF(R" + nRow + ",\"" + item.NgayNghiViec.Value.ToString("MM/dd/yyyy") + "\",\"ym\")&\" tháng \"&DATEDIF(R" + nRow + ",\"" + item.NgayNghiViec.Value.ToString("MM/dd/yyyy") + "\",\"md\")&\" ngày\"";
                    string ThamNien = "";
                    if (SoNam > 0)
                        ThamNien = SoNam + " năm ";
                    if (soThang > 0)
                        ThamNien += soThang + " tháng ";
                    if (SoNgay > 0)
                        ThamNien += SoNgay + " ngày ";

                    sheet1.Cells[nRow, colum].Value = ThamNien;
                }


                HRM_HopDong hopDongNhanViens = listHopDong.Where(r => r.IdNhanVien == item.IdGuid && r.isPhuLuc == false).OrderByDescending(r => r.Created).FirstOrDefault();
                colum++;//34
                if (hopDongNhanViens != null && item.NgayNghiViec != null)
                    sheet1.Cells[nRow, colum].Value = hopDongNhanViens.SoHopDong;
                colum++;//35
                if (hopDongNhanViens != null && hopDongNhanViens.NgayHieuLuc != null)
                    sheet1.Cells[nRow, colum].Value = hopDongNhanViens.NgayHieuLuc.ToString("dd/MM/yyyy");
                colum++;//36
                if (hopDongNhanViens != null && hopDongNhanViens.NgayHetHan != null)
                    sheet1.Cells[nRow, colum].Value = hopDongNhanViens.NgayHetHan.Value.ToString("dd/MM/yyyy");
                string ThoiHanHopDongs = hopDongNhanViens != null ? dmThoiHanHopDongs.Where(r => r.IdGuid == hopDongNhanViens.IdThoiGianLamViec).Select(r => r.Ten).FirstOrDefault() : "";
                colum++;//37
                sheet1.Cells[nRow, colum].Value = ThoiHanHopDongs;
                colum++;//38
                var GiayToCMT = listGiayTo.Where(a => a.Loai == 2 && a.IdNhanVien == item.IdGuid).FirstOrDefault();
                if (GiayToCMT != null)
                    sheet1.Cells[nRow, colum].Value = GiayToCMT.So;
                colum++;//39
                        //if (item.ChungMinhThuNgayCap != null)
                if (GiayToCMT != null && GiayToCMT.NgayCap != null && GiayToCMT.NgayCap.Value.Year > 1970)
                    sheet1.Cells[nRow, colum].Value = GiayToCMT.NgayCap.Value.ToString("dd/MM/yyyy");
                colum++;//40
                if (GiayToCMT != null)
                    sheet1.Cells[nRow, colum].Value = dmNoiCaps.Where(a => a.IdGuid == GiayToCMT.NoiCap).Select(a => a.Ten).FirstOrDefault();
                colum++;//41
                var GiayToCCCD = listGiayTo.Where(a => a.Loai == 1 && a.IdNhanVien == item.IdGuid).FirstOrDefault();
                if (GiayToCCCD != null)
                    sheet1.Cells[nRow, colum].Value = GiayToCCCD.So;
                colum++;//39
                        //if (item.ChungMinhThuNgayCap != null)
                if (GiayToCCCD != null && GiayToCCCD.NgayCap != null && GiayToCCCD.NgayCap.Value.Year > 1970)
                    sheet1.Cells[nRow, colum].Value = GiayToCCCD.NgayCap.Value.ToString("dd/MM/yyyy");
                colum++;//40
                if (GiayToCCCD != null)
                    sheet1.Cells[nRow, colum].Value = dmNoiCaps.Where(a => a.IdGuid == GiayToCCCD.NoiCap).Select(a => a.Ten).FirstOrDefault();
                colum++;//41
                sheet1.Cells[nRow, colum].Value = item.SoBaoHiemXaHoi;
                colum++;//41
                if (hopDongNhanViens != null)
                {
                    var BaoHiem = listBaoHiem.Where(a => a.isNgoai == false && a.IdHopDong == hopDongNhanViens.IdGuid).FirstOrDefault();
                    if (BaoHiem != null && BaoHiem.NgayBatDauDongBHXH != null && BaoHiem.NgayBatDauDongBHXH.Value.Year > 1970)
                    {
                        int Thang = ((time.Year - BaoHiem.NgayBatDauDongBHXH.Value.Year) * 12) + time.Month - BaoHiem.NgayBatDauDongBHXH.Value.Month + 1;
                        sheet1.Cells[nRow, colum].Value = Thang + " Tháng";
                    }
                }
                colum++;//41
                if (hopDongNhanViens != null)
                {
                    var BaoHiem = listBaoHiem.Where(a => a.isNgoai == true && a.IdHopDong == hopDongNhanViens.IdGuid).FirstOrDefault();
                    if (BaoHiem != null && BaoHiem.NgayBatDauDongBHXH != null && BaoHiem.NgayBatDauDongBHXH.Value.Year > 1970)
                    {
                        int Thang = ((time.Year - BaoHiem.NgayBatDauDongBHXH.Value.Year) * 12) + time.Month - BaoHiem.NgayBatDauDongBHXH.Value.Month + 1;
                        var NoiCap = dmNoiCapBHs.FirstOrDefault(a => a.IdGuid == BaoHiem.NoiCapSoBaoHiem);
                        sheet1.Cells[nRow, colum].Value = "Tên Đơn vị: " + NoiCap.Ten + "(" + Thang + " Tháng)";
                    }
                }
                colum++;//41
                var luongNhanVien = listLuongNhanVien.Where(r => r.IdNhanVien == item.IdGuid).OrderByDescending(r => r.Created).FirstOrDefault();
                if (luongNhanVien != null)
                    sheet1.Cells[nRow, colum].Value = luongNhanVien.LuongHachToan;
                colum++;//41
                colum++;//41
                sheet1.Cells[nRow, colum].Value = QuyDoiTrangThaiNhanVien(item.TrangThai);

                nRow++;
                STT++;
            }

            DrawTableExcel(sheet1, 1, nRow - 1, 1, colum);
            sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            dynamic result = new DynamicObject();

            string name = "ExportDanhSachNhanVien.xlsx";
            var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;
        }
        public dynamic ExportDanhSachNhanVienTangGiamBaoHiem(EntityGetDanhSachNhanVienRequest aRequest, ApplicationDbContext db)
        {

            int NamNghiViec = 0;
            if (aRequest.TrangThai == "DaNghiViec")
            {
                aRequest.TrangThai = "DaNghiViec";
                NamNghiViec = aRequest.Nam;
            }

            var listNhanVien = (from a in db.HRM_NhanVien
                                where (aRequest.TrangThai == "" || aRequest.TrangThai == null | a.TrangThai == aRequest.TrangThai)
                                && (NamNghiViec == 0 || (a.NgayNghiViec != null && a.NgayNghiViec.Value.Year == NamNghiViec))
                                && (aRequest.IdBoPhanEOS == 0 || a.IdBoPhanEOS == aRequest.IdBoPhanEOS)
                                select a).Distinct().OrderBy(r => r.MaNhanVien).ToList();
            DateTime DateTimeNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            var listIdNhanVien = listNhanVien.Select(r => r.IdGuid).ToList();
            if (aRequest.IdCongTyEOS > 0)
            {
                listIdNhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == aRequest.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
            }
            if (aRequest.IdChucDanhEOS > 0)
                listNhanVien = listNhanVien.Where(a => a.IdChucDanh == aRequest.IdChucDanhEOS).ToList();
            if (aRequest.IdChucVuEOS > 0)
                listNhanVien = listNhanVien.Where(a => a.IdChucVu == aRequest.IdChucVuEOS).ToList();
            if (!string.IsNullOrEmpty(aRequest.IdLoaiNhanVien))
                listNhanVien = listNhanVien.Where(a => a.IdLoaiNhanVien == aRequest.IdLoaiNhanVien).ToList();

            var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(r => (r.Loai == 1 || r.Loai == 2 || r.Loai == 4) && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
            if (aRequest.isCMT > 0)
            {
                var listGiayTos = listGiayTo.Where(a => a.Loai == 2).ToList();
                listIdNhanVien = listGiayTos.Select(r => r.IdNhanVien).ToList();
                if (aRequest.isCMT == 1)
                {

                    listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }
                else
                {
                    listNhanVien = listNhanVien.Where(a => !listIdNhanVien.Contains(a.IdGuid)).ToList();

                }
                listIdNhanVien = listNhanVien.Select(r => r.IdGuid).ToList();
            }
            if (aRequest.TuNgayUnix > 0)
            {
                DateTime timecheck = dungchung.UnixTimeStampToDateTime(aRequest.TuNgayUnix);
                listNhanVien = listNhanVien.Where(a => a.NgayBatDauLamViec != null && timecheck <= a.NgayBatDauLamViec).ToList();
            }
            if (aRequest.DenNgayUnix > 0)
            {
                DateTime timecheck = dungchung.UnixTimeStampToDateTime(aRequest.DenNgayUnix);
                listNhanVien = listNhanVien.Where(a => a.NgayBatDauLamViec != null && timecheck >= a.NgayBatDauLamViec).ToList();
            }

            var listCongTy_NhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).Distinct().ToList();

            var listBaoHiem = db.HRM_BaoHiem_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && string.IsNullOrEmpty(a.IdHopDong)).Distinct().ToList();
            var listBaoHiemTN = db.HRM_NhanVien_BaoHiemTaiNan.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).Distinct().ToList();
            listIdNhanVien = listBaoHiem.Select(a => a.IdNhanVien).Distinct().ToList();
            listIdNhanVien.AddRange(listBaoHiem.Select(a => a.IdNhanVien).Distinct().ToList());
            listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

            var listSDT = db.HRM_NhanVien_SoDienThoai.Where(r => r.isApDung == true && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
            var listEmail = db.HRM_NhanVien_Email.Where(r => r.isApDung == true && listIdNhanVien.Contains(r.IdNhanVien)).ToList();
            var DuAns = _CallWebService.GetDanhSachDuAn(db);
            var dmNoiCaps = db.HRM_dmNoiCap.ToList();
            var dmNoiCapBHs = db.HRM_dmNoiCapBaoHiem.ToList();
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/MauTangGiamBHICT.xlsx");

            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheetBH = package.Workbook.Worksheets["BHSKTD"];
            ExcelWorksheet sheetTN = package.Workbook.Worksheets["BH TAI NẠN"];
            DateTime time = DateTime.Today;
            s.Close();
            int nRow = 4;
            int nRowTN = 4;
            int STT = 1;
            int STTTN = 1;
            int ncol = 1;
            int ncolTN = 1;
            foreach (var item in listNhanVien)
            {
                var listCty = listCongTy_NhanVien.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).ToList();
                var ctys = DuAns.Where(a => listCty.Contains(a.Id)).Select(a => a.TenBoPhan).Distinct().ToList();
                var Mactys = DuAns.Where(a => listCty.Contains(a.Id)).Select(a => a.MaBoPhan).Distinct().ToList();
                ncol = 1;
                var BaoHiem = listBaoHiem.Where(a => a.IdNhanVien == item.IdGuid && a.SoTienBaoHiem > 0).FirstOrDefault();
                if (BaoHiem != null)
                {
                    sheetBH.Cells[nRow, ncol].Value = STT;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ncol++;
                    sheetBH.Cells[nRow, ncol].Value = item.MaNhanVien;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetBH.Cells[nRow, ncol].Value = item.SoGen;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetBH.Cells[nRow, ncol].Value = item.TenDayDu;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    int DoTuoi = 0;
                    if (item.NgaySinh != null)
                    {
                        sheetBH.Cells[nRow, ncol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                        DoTuoi = (int)((time - item.NgaySinh.Date).TotalDays / 365);
                    }
                    ncol++;
                    sheetBH.Cells[nRow, ncol].Value = DoTuoi;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ncol++;
                    sheetBH.Cells[nRow, ncol].Value = item.HoKhauThuongTruDiaChi;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    var GiayTo = listGiayTo.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    if (GiayTo != null)
                        sheetBH.Cells[nRow, ncol].Value = GiayTo.So;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (GiayTo != null && GiayTo.NgayCap != null)
                        sheetBH.Cells[nRow, ncol].Value = GiayTo.NgayCap.Value.ToString("dd/MM/yyyy");
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    if (GiayTo != null)
                        sheetBH.Cells[nRow, ncol].Value = dmNoiCaps.Where(a => a.IdGuid == GiayTo.NoiCap).Select(a => a.Ten).FirstOrDefault();
                    ncol++;

                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheetBH.Cells[nRow, ncol].Value = BaoHiem.SoTienBaoHiem;
                    sheetBH.Cells[nRow, ncol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetBH.Cells[nRow, ncol].Value = BaoHiem.SoSoBaoHiem;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (BaoHiem.NgayBatDau != null)
                        sheetBH.Cells[nRow, ncol].Value = BaoHiem.NgayBatDau.Value.ToString("dd/MM/yyyy");
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (BaoHiem.NgayKetThuc != null)
                        sheetBH.Cells[nRow, ncol].Value = BaoHiem.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheetBH.Cells[nRow, ncol].Value = BaoHiem.MucDongBaoHiem;
                    sheetBH.Cells[nRow, ncol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    ncol++;
                    var DonVi = dmNoiCapBHs.FirstOrDefault(a => a.IdGuid == BaoHiem.NoiCapSoBaoHiem);

                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetBH.Cells[nRow, ncol].Value = string.Join(",", Mactys);
                    ncol++;

                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetBH.Cells[nRow, ncol].Value = string.Join(",", ctys);

                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetBH.Cells[nRow, ncol].Value = BaoHiem.Giam == 1 ? "Giảm" : "Tăng";
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    if (DonVi != null)
                        sheetBH.Cells[nRow, ncol].Value = DonVi.Ten;
                    ncol++;
                    sheetBH.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetBH.Cells[nRow, ncol].Value = BaoHiem.GhiChu;

                    STT++;

                    nRow++;

                }
                ncolTN = 1;
                var BaoHiemTN = listBaoHiemTN.Where(a => a.IdNhanVien == item.IdGuid && a.SoTienBaoHiem > 0).FirstOrDefault();
                if (BaoHiemTN != null)
                {
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetTN.Cells[nRowTN, ncolTN].Value = STT;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetTN.Cells[nRowTN, ncolTN].Value = item.MaNhanVien;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetTN.Cells[nRowTN, ncolTN].Value = item.SoGen;
                    ncolTN++;
                    sheetTN.Cells[nRowTN, ncolTN].Value = item.TenDayDu;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    int DoTuoi = 0;
                    if (item.NgaySinh != null)
                    {
                        sheetTN.Cells[nRowTN, ncolTN].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                        DoTuoi = (int)((time - item.NgaySinh.Date).TotalDays / 365);
                    }
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetTN.Cells[nRowTN, ncolTN].Value = DoTuoi;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetTN.Cells[nRowTN, ncolTN].Value = item.HoKhauThuongTruDiaChi;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    var GiayTo = listGiayTo.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    if (GiayTo != null)
                        sheetTN.Cells[nRowTN, ncolTN].Value = GiayTo.So;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (GiayTo != null && GiayTo.NgayCap != null)
                        sheetTN.Cells[nRowTN, ncolTN].Value = GiayTo.NgayCap.Value.ToString("dd/MM/yyyy");
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    if (GiayTo != null)
                        sheetTN.Cells[nRowTN, ncolTN].Value = dmNoiCaps.Where(a => a.IdGuid == GiayTo.NoiCap).Select(a => a.Ten).FirstOrDefault();
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    sheetTN.Cells[nRowTN, ncolTN].Value = BaoHiemTN.SoTienBaoHiem;
                    sheetTN.Cells[nRowTN, ncolTN].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetTN.Cells[nRowTN, ncolTN].Value = BaoHiemTN.SoHopDong;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (BaoHiemTN.NgayBatDau != null)
                        sheetTN.Cells[nRowTN, ncolTN].Value = BaoHiemTN.NgayBatDau.Value.ToString("dd/MM/yyyy");
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (BaoHiemTN.NgayKetThuc != null)
                        sheetTN.Cells[nRowTN, ncolTN].Value = BaoHiemTN.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    var DonVi = dmNoiCapBHs.FirstOrDefault(a => a.IdGuid == BaoHiemTN.NoiCapSoBaoHiem);
                    sheetTN.Cells[nRowTN, ncolTN].Value = string.Join(",", Mactys);
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetTN.Cells[nRowTN, ncolTN].Value = string.Join(",", ctys);

                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheetTN.Cells[nRowTN, ncolTN].Value = BaoHiemTN.Giam == 1 ? "Tăng" : "Giảm";
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    if (DonVi != null)
                        sheetTN.Cells[nRowTN, ncolTN].Value = DonVi.Ten;
                    ncolTN++;
                    sheetTN.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheetTN.Cells[nRowTN, ncolTN].Value = BaoHiemTN.GhiChu;
                    STTTN++;


                    nRowTN++;

                }

            }


            DrawTableExcel(sheetBH, 1, nRow - 1, 1, ncol);
            sheetBH.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            DrawTableExcel(sheetTN, 1, nRow - 1, 1, ncol);
            sheetTN.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            dynamic result = new DynamicObject();

            string name = "MauTangGiamBHICT.xlsx";
            var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;
        }

        public dynamic ExportDanhSachNhanVienThue(ApplicationDbContext db, EntityGetDanhSachNhanVienRequest timkiem)
        {

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachNhanVienThue.xlsx");
            var filenamegui = "DanhSachNhanVienThue.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

            var listNhanVien = db.HRM_NhanVien.AsNoTracking().Where(a => (timkiem.IdBoPhanEOS == 0 || a.IdBoPhanEOS == timkiem.IdBoPhanEOS)
                                              && (timkiem.IdChucVuEOS == 0 || a.IdChucVu == timkiem.IdChucVuEOS)
                                              && (timkiem.IdChucDanhEOS == 0 || a.IdChucDanh == timkiem.IdChucDanhEOS)
                                              && a.NgayBatDauLamViec != null
                                              && ((timkiem.Ma == true && !string.IsNullOrEmpty(a.MaSoThueTNCN)) || (timkiem.Ma == false && string.IsNullOrEmpty(a.MaSoThueTNCN)))
                                              && (string.IsNullOrEmpty(timkiem.TrangThai) || a.TrangThai == timkiem.TrangThai));


            //var listDaTa = (from a in db.HRM_NhanVien
            //                where a.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString()
            //                && (IdCongTyEOS == 0 || (IdCongTyEOS > 0 && a.IdCongTyEOS == IdCongTyEOS))
            //                && (IdBoPhanEOS == 0 || (IdBoPhanEOS > 0 && a.IdBoPhanEOS == IdBoPhanEOS))
            //                && ((Ma == true && !string.IsNullOrEmpty(a.MaSoThueTNCN)) || (Ma == false && string.IsNullOrEmpty(a.MaSoThueTNCN)))
            //                orderby a.MaNhanVien
            //                select a).ToList();

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
            // Start Duyệt mới thêm
            List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
            var listCongTy_BoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
            // End Duyệt mới thêm
            var lstCongTyAll = _CallWebService.GetDanhSachDuAn(db);

            foreach (var model in listNhanVien)
            {
                // Start Duyệt mới thêm
                var _listQuyTrinh_NhanVien_BoPhan = listCongTy_BoPhan.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                var listCongTy = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).ToList();
                model.listIdCongTy = listCongTy.Select(x => x.Id).ToList();
                model.TenCongTy = string.Join("; ", listCongTy.Select(x => x.TenBoPhan));
                // End Duyệt mới thêm
            }
            var listDaTa = listNhanVien.OrderBy(a => a.MaNhanVien).ToList();
            if (timkiem.IdCongTyEOS > 0)
                listDaTa = listDaTa.FindAll(x => x.listIdCongTy != null && x.listIdCongTy.Contains(timkiem.IdCongTyEOS));



            listIdNhanVien = listDaTa.Select(x => x.IdGuid).ToList();
            List<HRM_NhanVien_GiayToTuyThan> listGiayToTuyThan = db.HRM_NhanVien_GiayToTuyThan.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.isApDung == true).ToList();
            List<HRM_dmNoiCap> dmNoiCaps = db.HRM_dmNoiCap.ToList();

            var listSDTs = db.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();

            var listQuocTich = db.HRM_dmQuocTich.ToList();
            var listPhuongXa = db.HRM_dmPhuongXa.ToList();
            var listQuanHuyen = db.HRM_dmQuanHuyen.ToList();
            var listTinhThanhPho = db.HRM_dmTinh.ToList();

            if (listDaTa != null && listDaTa.Count > 0)
            {
                int nRow = 4;
                int STT = 1;
                int nCol = 1;
                foreach (var item in listDaTa)
                {
                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.MaSoThueTNCN;
                    nCol++;
                    if (item.NgaySinh != null)
                        sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    nCol++;
                    int i = 1;
                    if (item.IdGioiTinh != null)
                    {
                        if (item.IdGioiTinh == "Nam")
                            i = 2;
                        else if (item.IdGioiTinh == "KhongXacDinh")
                            i = 3;
                        sheet1.Cells[nRow, nCol].Value = i;
                    }
                    nCol++;
                    if (item.IdGioiTinh != null)
                        sheet1.Cells[nRow, nCol].Value = item.IdGioiTinh != "KhongXacDinh" ? item.IdGioiTinh : "Khác";
                    nCol++;
                    HRM_dmQuocTich quoctich = new HRM_dmQuocTich();
                    if (!string.IsNullOrEmpty(item.IdQuocTich))
                    {
                        quoctich = listQuocTich.FirstOrDefault(a => a.IdGuid == item.IdQuocTich);
                        if (quoctich != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = quoctich.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = quoctich.Ten;
                            nCol++;

                        }
                        else
                        {
                            nCol++; nCol++;
                        }
                    }
                    else
                    {
                        nCol++; nCol++;
                    }
                    //Người Việt Nam (sống ở nước ngoài) 
                    nCol++;
                    //CMND kê khai sai
                    HRM_NhanVien_GiayToTuyThan CCCD = listGiayToTuyThan.Find(x => x.IdNhanVien == item.IdGuid);
                    //Mã Loại giấy tờ tùy thân của cá nhân (CMND/CCCD/Hộ chiếu)
                    if (CCCD != null)
                    {
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = CCCD.Loai;
                        nCol++;
                        if (CCCD.Loai == 1)
                            sheet1.Cells[nRow, nCol].Value = "CCCD";
                        else if (CCCD.Loai == 2)
                            sheet1.Cells[nRow, nCol].Value = "CMT";
                        else if (CCCD.Loai == 3)
                            sheet1.Cells[nRow, nCol].Value = "Hộ chiếu";
                        else
                            sheet1.Cells[nRow, nCol].Value = "Khác";
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = CCCD.So;
                        nCol++;
                        if (CCCD.NgayCap != null)
                            sheet1.Cells[nRow, nCol].Value = CCCD.NgayCap.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        HRM_dmNoiCap dmNoiCap = dmNoiCaps.Find(x => x.IdGuid == CCCD.NoiCap);
                        if (dmNoiCap != null)
                            sheet1.Cells[nRow, nCol].Value = dmNoiCap.Ma;
                        nCol++;
                        if (dmNoiCap != null)
                            sheet1.Cells[nRow, nCol].Value = dmNoiCap.Ten;
                        nCol++;
                    }
                    else
                    {
                        nCol = nCol + 7;
                    }

                    sheet1.Cells[nRow, nCol].Value = item.HoKhauThuongTruDiaChi;
                    nCol++;
                    if (!string.IsNullOrEmpty(item.HoKhauThuongTruIdPhuongXa))
                    {
                        var PhuongXa = listPhuongXa.FirstOrDefault(a => a.IdGuid == item.HoKhauThuongTruIdPhuongXa);
                        if (PhuongXa != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = PhuongXa.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = PhuongXa.Ten;
                            nCol++;
                        }
                        else
                        {
                            nCol++;
                            nCol++;
                        }
                    }
                    else
                    {
                        nCol++;
                        nCol++;
                    }
                    if (!string.IsNullOrEmpty(item.HoKhauThuongTruIdQuanHuyen))
                    {
                        var QuanHuyen = listQuanHuyen.FirstOrDefault(a => a.IdGuid == item.HoKhauThuongTruIdQuanHuyen);
                        if (QuanHuyen != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = QuanHuyen.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = QuanHuyen.Ten;
                            nCol++;
                        }
                        else
                        {
                            nCol++; nCol++;
                        }
                    }
                    else
                    {
                        nCol++; nCol++;
                    }
                    if (!string.IsNullOrEmpty(item.HoKhauThuongTruIdTinhThanhPho))
                    {
                        var TinhThanhPho = listTinhThanhPho.FirstOrDefault(a => a.IdGuid == item.HoKhauThuongTruIdTinhThanhPho);
                        if (TinhThanhPho != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = TinhThanhPho.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = TinhThanhPho.Ten;
                            nCol++;
                        }
                        else
                        {
                            nCol++; nCol++;
                        }
                    }
                    else
                    {
                        nCol++; nCol++;
                    }
                    sheet1.Cells[nRow, nCol].Value = quoctich.Ma;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = quoctich.Ten;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.ChoOHienNayTruDiaChi;
                    nCol++;
                    if (!string.IsNullOrEmpty(item.ChoOHienNayTruIdPhuongXa))
                    {
                        var PhuongXa = listPhuongXa.FirstOrDefault(a => a.IdGuid == item.ChoOHienNayTruIdPhuongXa);
                        if (PhuongXa != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = PhuongXa.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = PhuongXa.Ten;
                            nCol++;
                        }
                        else
                        {
                            nCol++; nCol++;
                        }
                    }
                    else
                    {
                        nCol++; nCol++;
                    }
                    if (!string.IsNullOrEmpty(item.ChoOHienNayTruIdQuanHuyen))
                    {
                        var QuanHuyen = listQuanHuyen.FirstOrDefault(a => a.IdGuid == item.ChoOHienNayTruIdQuanHuyen);
                        if (QuanHuyen != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = QuanHuyen.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = QuanHuyen.Ten;
                            nCol++;
                        }
                        else
                        {
                            nCol++; nCol++;
                        }
                    }
                    else
                    {
                        nCol++; nCol++;
                    }
                    if (!string.IsNullOrEmpty(item.ChoOHienNayTruIdTinhThanhPho))
                    {
                        var TinhThanhPho = listTinhThanhPho.FirstOrDefault(a => a.IdGuid == item.ChoOHienNayTruIdTinhThanhPho);
                        if (TinhThanhPho != null)
                        {
                            sheet1.Cells[nRow, nCol].Value = TinhThanhPho.Ma;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = TinhThanhPho.Ten;
                            nCol++;
                        }
                        else
                        {
                            nCol++; nCol++;
                        }
                    }
                    else
                    {
                        nCol++; nCol++;
                    }

                    sheet1.Cells[nRow, nCol].Value = listSDTs.Where(a => a.IdNhanVien == item.IdGuid).Select(a => a.SoDienThoai).FirstOrDefault();
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.Email;

                    STT++;
                    nRow++;
                }
                if (nRow > 4)
                    nRow--;
                DrawTableExcel(sheet1, 4, nRow, 1, nCol);
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                //sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[4, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            }
            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportDanhSachNhanVienHetHanPhuThuoc(ApplicationDbContext db, EntityGetDanhSachNhanVienRequest timkiem)
        {

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachNhanVienHetHanPhuThuoc.xlsx");
            var filenamegui = "DanhSachNhanVienPhuThuoc.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            DateTime time = DateTime.Today;
            if (timkiem.Nam > 0 && timkiem.Thang > 0)
            {
                time = new DateTime(timkiem.Nam, timkiem.Thang, 1);
                time = time.AddMonths(1).AddDays(-1);
            }
            time = time.AddYears(-18);
            var lisGiaDinh = db.HRM_NhanVienQuanHeGiaDinh.Where(a => a.isLaNguoiPhuThuoc == true).ToList();
            if (lisGiaDinh != null && lisGiaDinh.Count > 0)
            {
                List<string> listIdNhanVien = lisGiaDinh.Select(a => a.IdNhanVien).Distinct().ToList();

                var listNhanVien = db.HRM_NhanVien.Where(a => (timkiem.IdBoPhanEOS == 0 || a.IdBoPhanEOS == timkiem.IdBoPhanEOS)
                                                   && (timkiem.IdChucVuEOS == 0 || a.IdChucVu == timkiem.IdChucVuEOS)
                                                   && (timkiem.IdChucDanhEOS == 0 || a.IdChucDanh == timkiem.IdChucDanhEOS)
                                                   && a.NgayBatDauLamViec != null && listIdNhanVien.Contains(a.IdGuid)
                                                   && (string.IsNullOrEmpty(timkiem.TrangThai) || a.TrangThai == timkiem.TrangThai));
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
                // Start Duyệt mới thêm
                listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                var listCongTy_BoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
                // End Duyệt mới thêm
                var lstCongTyAll = _CallWebService.GetDanhSachDuAn(db);

                foreach (var model in listNhanVien)
                {
                    // Start Duyệt mới thêm
                    var _listQuyTrinh_NhanVien_BoPhan = listCongTy_BoPhan.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                    var listCongTy = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).ToList();
                    model.listIdCongTy = listCongTy.Select(x => x.Id).ToList();
                    model.TenCongTy = string.Join("; ", listCongTy.Select(x => x.TenBoPhan));
                    // End Duyệt mới thêm
                }
                var listDaTa = listNhanVien.ToList();
                if (timkiem.IdCongTyEOS > 0)
                    listDaTa = listDaTa.FindAll(x => x.listIdCongTy.Contains(timkiem.IdCongTyEOS));

                if (listDaTa != null && listDaTa.Count > 0)
                {
                    int nRow = 4;
                    int STT = 1;
                    int nCol = 1;
                    foreach (var item in listDaTa)
                    {
                        var listPT = lisGiaDinh.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        int Count = listPT.Count;
                        if (Count == 0)
                            Count = 1;
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        sheet1.Cells[nRow, nCol, nRow + Count - 1, nCol].Merge = true;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        sheet1.Cells[nRow, nCol, nRow + Count - 1, nCol].Merge = true;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                        sheet1.Cells[nRow, nCol, nRow + Count - 1, nCol].Merge = true;
                        nCol++;
                        if (item.NgaySinh != null)
                            sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                        sheet1.Cells[nRow, nCol, nRow + Count - 1, nCol].Merge = true;

                        nCol++;
                        sheet1.Cells[nRow, nCol, nRow + Count - 1, nCol].Merge = true;
                        sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                        sheet1.Cells[nRow, nCol, nRow + Count - 1, nCol].Merge = true;
                        nCol++;

                        int col = nCol;
                        foreach (var GD in listPT)
                        {
                            col = nCol;
                            sheet1.Cells[nRow, col].Value = GD.HoTen;
                            col++;
                            if (GD.NgaySinh != null)
                                sheet1.Cells[nRow, col].Value = GD.NgaySinh.Value.ToString("dd/MM/yyyy");
                            col++;
                            sheet1.Cells[nRow, col].Value = GD.MaSoThueCaNhan;
                            col++;
                            if (GD.NgaySinh != null)
                            {
                                if (GD.NgaySinh < time)
                                    sheet1.Cells[nRow, col].Value = "Quá 18 tuổi";
                                else
                                    sheet1.Cells[nRow, col].Value = "Phụ Thuộc";

                            }
                            nRow++;
                        }



                        STT++;

                    }
                    if (nRow > 4)
                        nRow--;
                    DrawTableExcel(sheet1, 4, nRow + 1, 1, 9);
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[4, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[4, 5, nRow, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                }
            }
            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportFileMauNhanVienLienQuan(ApplicationDbContext db)
        {
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/importThongTinLienQuanNhanVienDA.xlsx");

            var dmLoaiNhanViens = db.HRM_dmLoaiNhanVien.ToList();
            var dmQuanHeGiaDinhs = db.HRM_dmQuanHeGiaDinh.ToList();
            var dmTinhs = db.HRM_dmTinh.ToList();
            var dmQuan = db.HRM_dmQuanHuyen.ToList();
            var dmXaPhuong = db.HRM_dmPhuongXa.ToList();
            var dmQuocTinh = db.HRM_dmQuocTich.ToList();
            var dmDanToc = db.HRM_dmDanToc.ToList();
            var dmTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
            var dmChungChi = db.HRM_dmNhomChungChi.ToList();
            var dmNgonNgu = db.HRM_dmNgonNgu.ToList();
            var dmTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var DmNoiCap = db.HRM_dmNoiCap.ToList();
            var DmNoiCapBH = db.HRM_dmNoiCapBaoHiem.ToList();
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh Mục"];
            ExcelWorksheet sheet2 = package.Workbook.Worksheets["Thông tin nhân viên"];
            s.Close();
            int nRow = 3;

            foreach (var item in dmDanToc)
            {
                sheet1.Cells[nRow, 1].Value = item.Ten;
                sheet1.Cells[nRow, 2].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in dmTrinhDo)
            {
                sheet1.Cells[nRow, 3].Value = item.Ten;
                sheet1.Cells[nRow, 4].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in dmTinhTrangHonNhan)
            {
                sheet1.Cells[nRow, 5].Value = item.Ten;
                sheet1.Cells[nRow, 6].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in dmChungChi)
            {
                sheet1.Cells[nRow, 7].Value = item.Ten;
                sheet1.Cells[nRow, 8].Value = item.Ma;
                nRow++;
            }

            nRow = 3;

            foreach (var item in dmQuocTinh)
            {
                sheet1.Cells[nRow, 9].Value = item.Ten;
                sheet1.Cells[nRow, 10].Value = item.Ma;
                nRow++;

            }

            nRow = 3;
            foreach (var item in dmNgonNgu)
            {
                sheet1.Cells[nRow, 11].Value = item.Ten;
                sheet1.Cells[nRow, 12].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in dmQuanHeGiaDinhs)
            {
                sheet1.Cells[nRow, 13].Value = item.Ten;
                sheet1.Cells[nRow, 14].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in dmLoaiNhanViens)
            {
                sheet1.Cells[nRow, 15].Value = item.Ten;
                sheet1.Cells[nRow, 16].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in DmNoiCap)
            {
                sheet1.Cells[nRow, 19].Value = item.Ten;
                sheet1.Cells[nRow, 20].Value = item.Ma;
                nRow++;
            }
            var listCongTY = _CallWebService.GetDanhSachDuAn(db);
            nRow = 3;
            foreach (var item in listCongTY)
            {
                sheet1.Cells[nRow, 21].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 22].Value = item.MaBoPhan;
                nRow++;
            }
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            nRow = 3;
            foreach (var item in listBoPhan)
            {
                sheet1.Cells[nRow, 23].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 24].Value = item.MaBoPhan;
                nRow++;
            }
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            nRow = 3;

            foreach (var item in listChucVu)
            {
                sheet1.Cells[nRow, 25].Value = item.TenChucVu;
                sheet1.Cells[nRow, 26].Value = item.MaChucVu;
                nRow++;
            }
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            nRow = 3;

            foreach (var item in listChucDanh)
            {
                sheet1.Cells[nRow, 27].Value = item.Ten;
                sheet1.Cells[nRow, 28].Value = item.Ma;
                nRow++;
            }
            nRow = 3;

            foreach (var item in DmNoiCapBH)
            {
                sheet1.Cells[nRow, 29].Value = item.Ten;
                sheet1.Cells[nRow, 30].Value = item.Ma;
                nRow++;
            }


            dynamic result = new DynamicObject();

            string name = "importThongTinLienQuanNhanVienDA.xlsx";
            var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
            string pathresult = PathUploads + filenamegui;
            File.WriteAllBytes(pathresult, package.GetAsByteArray());

            HDFile newfile = new HDFile();
            newfile.Name = filenamegui;
            newfile.NameLocal = name;

            newfile.Url = "/uploader/api/hdfiles?filename=" + System.Web.HttpUtility.UrlEncode(dungchung.Base64Encode(name))
                        + "&path=" + dungchung.Base64Encode(pathresult);
            result.file = newfile;
            return result;
        }
        public dynamic ExportDanhSachNhanVienKhamSucKhoe(ApplicationDbContext db, EntityGetDanhSachNhanVienRequest timkiem)
        {
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoKhamSucKhoe.xlsx");


            var filenamegui = "BaoCaoKhamSucKhoe.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            int nRow = 4;

            //DateTime dt = DateTime.Now;
            //DateTime time = new DateTime(dt.Year, timkiem.Thang, 1);
            //time = time.AddMonths(1).AddDays(-1);


            var listNhanVien = db.HRM_NhanVien.Where(a => (timkiem.IdBoPhanEOS == 0 || a.IdBoPhanEOS == timkiem.IdBoPhanEOS)
                                                        && (timkiem.IdChucDanhEOS == 0 || a.IdChucDanh == timkiem.IdChucDanhEOS)
                                                        && (timkiem.IdChucVuEOS == 0 || a.IdChucVu == timkiem.IdChucVuEOS)
                                                        && a.NgayBatDauLamViec != null
                                                        && (string.IsNullOrEmpty(timkiem.IdLoaiNhanVien) || a.IdLoaiNhanVien == timkiem.IdLoaiNhanVien)

                                                        && (string.IsNullOrEmpty(timkiem.TrangThai) || a.TrangThai == timkiem.TrangThai));
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
            // Start Duyệt mới thêm
            var listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
            var listQuyTrinh_BoPhan_1 = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
            // End Duyệt mới thêm
            var lstCongTyAll = _CallWebService.GetDanhSachDuAn(db);
            foreach (var model in listNhanVien)
            {
                // Start Duyệt mới thêm
                var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                var listCongTy = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).ToList();
                model.listIdCongTy = listCongTy.Select(x => x.Id).ToList();
                model.TenCongTy = string.Join("; ", listCongTy.Select(x => x.TenBoPhan));
                // End Duyệt mới thêm
            }
            var listData = listNhanVien.ToList();


            if (timkiem.IdCongTyEOS > 0)
                listData = listData.FindAll(x => x.listIdCongTy.Contains(timkiem.IdCongTyEOS));
            listIdNhanVien = listData.Select(x => x.IdGuid).ToList();
            List<HRM_NhanVien_KhamSucKhoe> listKhamSucKhoe = db.HRM_NhanVien_KhamSucKhoe.Where(x => listIdNhanVien.Contains(x.IdNhanVien)).ToList();
            listIdNhanVien = listKhamSucKhoe.Select(x => x.IdNhanVien).ToList();
            listData = listData.FindAll(x => listIdNhanVien.Contains(x.IdGuid));
            if (listData != null && listData.Count > 0)
            {
                var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                int STT = 1;
                foreach (var item in listData)
                {
                    sheet1.Cells[nRow, 1].Value = STT;
                    sheet1.Cells[nRow, 2].Value = item.MaNhanVien;
                    sheet1.Cells[nRow, 3].Value = item.TenDayDu;
                    sheet1.Cells[nRow, 4].Value = item.TenCongTy;
                    List<HRM_NhanVien_KhamSucKhoe> listKhamSucKhoeFind = listKhamSucKhoe.FindAll(x => x.IdNhanVien == item.IdGuid);
                    foreach (var itemChild in listKhamSucKhoeFind)
                    {
                        if (itemChild.NgayKham != null)
                            sheet1.Cells[nRow, 5].Value = itemChild.NgayKham.Value.ToString("dd/MM/yyyy");
                        if (itemChild.NgayKetThuc != null)
                            sheet1.Cells[nRow, 6].Value = itemChild.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                        sheet1.Cells[nRow, 7].Value = itemChild.NoiKham;
                        sheet1.Cells[nRow, 8].Value = itemChild.LoaiHinhKham;
                        sheet1.Cells[nRow, 9].Value = itemChild.GhiChu;
                        nRow++;
                    }
                    STT++;
                }
                nRow--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, 9);
                sheet1.Cells[3, 1, nRow, 9].Style.Font.Name = "Times New Roman";
            }

            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportDanhSachNhanVienSinhNhat(ApplicationDbContext db, EntityGetDanhSachNhanVienRequest timkiem)
        {
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachSinhNhatTrongThang.xlsx");


            var filenamegui = "DanhSachSinhNhatTrongThang.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            int nRow = 4;

            DateTime dt = DateTime.Now;
            DateTime time = new DateTime(dt.Year, timkiem.Thang, 1);
            time = time.AddMonths(1).AddDays(-1);


            var listNhanVien = db.HRM_NhanVien.Where(a => (timkiem.IdBoPhanEOS == 0 || a.IdBoPhanEOS == timkiem.IdBoPhanEOS)
                                                        && (timkiem.IdChucDanhEOS == 0 || a.IdChucDanh == timkiem.IdChucDanhEOS)
                                                        && (timkiem.IdChucVuEOS == 0 || a.IdChucVu == timkiem.IdChucVuEOS)
                                                        && a.NgaySinh != null && timkiem.Thang == a.NgaySinh.Month
                                                        && (string.IsNullOrEmpty(timkiem.TrangThai) || a.TrangThai == timkiem.TrangThai));
            if (!string.IsNullOrEmpty(timkiem.IdLoaiNhanVien))
                listNhanVien = listNhanVien.Where(a => !string.IsNullOrEmpty(a.IdLoaiNhanVien) && a.IdLoaiNhanVien == timkiem.IdLoaiNhanVien);
            // Start Duyệt mới thêm
            var listIdQuyTrinh_NhanVien_1 = listNhanVien.Select(x => x.IdGuid).ToList();
            var listQuyTrinh_BoPhan_1 = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdQuyTrinh)).ToList();
            // End Duyệt mới thêm
            var lstCongTyAll = _CallWebService.GetDanhSachDuAn(db);

            foreach (var model in listNhanVien)
            {
                // Start Duyệt mới thêm
                var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                var listCongTy = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).ToList();
                model.listIdCongTy = listCongTy.Select(x => x.Id).ToList();
                model.TenCongTy = string.Join("; ", listCongTy.Select(x => x.TenBoPhan));
                // End Duyệt mới thêm
            }
            var listData = listNhanVien.ToList();


            if (timkiem.IdCongTyEOS > 0)
                listData = listData.FindAll(x => x.listIdCongTy.Contains(timkiem.IdCongTyEOS));

            if (listData != null && listData.Count > 0)
            {
                var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                int STT = 1;
                int nCol = 1;
                sheet1.Cells[1, 1].Value = "DANH SÁCH SINH NHẬT TRONG THÁNG " + timkiem.Thang;
                foreach (var item in listData)
                {
                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                    nCol++;
                    var chucVu = ChucVus.FirstOrDefault(a => a.Id == item.IdChucVu);
                    if (chucVu != null)
                        sheet1.Cells[nRow, nCol].Value = chucVu.TenChucVu;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                    nCol++;
                    double NgayLV = 0;
                    if (item.NgayBatDauLamViec != null)
                    {
                        sheet1.Cells[nRow, nCol].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        NgayLV = (time - item.NgayBatDauLamViec.Value).TotalDays / 30.5;
                        NgayLV = Math.Floor(NgayLV);
                    }

                    nCol++;
                    if (NgayLV != 0)
                        sheet1.Cells[nRow, nCol].Value = NgayLV;
                    nCol++;
                    if (item.IdCaLamViec != null)
                    {
                        var Ca = caLamViecs.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                        if (Ca != null)
                            sheet1.Cells[nRow, nCol].Value = Ca.Ma;
                    }
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = "Đang làm việc";
                    nCol++;
                    STT++;
                    nRow++;
                }
                nRow--;
                nCol--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, 13);
                sheet1.Cells[3, 1, nRow, nCol].Style.Font.Name = "Times New Roman";
            }

            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }
        public dynamic ExportDanhSachNhanVienNgayKyNiemVao(ApplicationDbContext db, EntityGetDanhSachNhanVienRequest timkiem)
        {
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachNhanVienNgayKyNiemVao.xlsx");


            var filenamegui = "DanhSachNhanVienNgayKyNiemVao.xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            Stream s = File.OpenRead(sFileName);
            ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
            DateTime dt = DateTime.Now;
            //DateTime time = new DateTime(dt.Year, Thang, 1);
            //time = time.AddMonths(1).AddDays(-1);
            DateTime time = dt;

            int nRow = 4;
            var lstCongTyAll = _CallWebService.GetDanhSachDuAn(db);

            var listNhanVien = db.HRM_NhanVien.Where(a => (timkiem.IdBoPhanEOS == 0 || a.IdBoPhanEOS == timkiem.IdBoPhanEOS)
                                                        && (timkiem.IdChucDanhEOS == 0 || a.IdChucDanh == timkiem.IdChucDanhEOS)
                                                        && (timkiem.IdChucVuEOS == 0 || a.IdChucVu == timkiem.IdChucVuEOS)
                                                        && a.NgayBatDauLamViec != null
                                                        && (string.IsNullOrEmpty(timkiem.TrangThai) || a.TrangThai == timkiem.TrangThai));
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
            // Start Duyệt mới thêm
            var listIdQuyTrinh_NhanVien_1 = listNhanVien.Select(x => x.IdGuid).ToList();
            var listQuyTrinh_BoPhan_1 = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdQuyTrinh_NhanVien_1.Contains(a.IdQuyTrinh)).ToList();
            // End Duyệt mới thêm


            foreach (var model in listNhanVien)
            {
                // Start Duyệt mới thêm
                var _listQuyTrinh_NhanVien_BoPhan = listQuyTrinh_BoPhan_1.FindAll(x => x.IdQuyTrinh == model.IdGuid).Select(x => x.IdBoPhan);
                var listCongTy = lstCongTyAll.FindAll(x => _listQuyTrinh_NhanVien_BoPhan.Contains(x.Id)).ToList();
                model.listIdCongTy = listCongTy.Select(x => x.Id).ToList();
                model.TenCongTy = string.Join("; ", listCongTy.Select(x => x.TenBoPhan));
                // End Duyệt mới thêm
            }
            var listData = listNhanVien.ToList();
            if (timkiem.IdCongTyEOS > 0)
                listData = listData.FindAll(x => x.listIdCongTy.Contains(timkiem.IdCongTyEOS));

            if (listData != null && listData.Count > 0)
            {
                //var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                //var caLamViecs = db.HRM_dmLichLamViec.ToList();
                int STT = 1;
                int nCol = 1;
                foreach (var item in listData)
                {
                    nCol = 1;
                    sheet1.Cells[nRow, nCol].Value = STT;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenDayDu;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                    double NgayLV = (time - item.NgayBatDauLamViec.Value).TotalDays / 365;
                    nCol++;
                    if (NgayLV != 0)
                        sheet1.Cells[nRow, nCol].Value = Math.Floor(NgayLV);

                    STT++;
                    nRow++;
                }
                nRow--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                sheet1.Cells[3, 1, nRow, nCol].Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[4, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[4, 5, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }
            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;

        }
        public void UpdateNhanSu(ApplicationDbContext db)
        {
            var list = db.HRM_NhanVien.ToList();
            var cccdByNv = db.HRM_NhanVien_GiayToTuyThan.AsNoTracking().Where(a => a.isApDung == true).ToLookup(a => a.IdNhanVien);
            var emailByNv = db.HRM_NhanVien_Email.AsNoTracking().Where(a => a.isApDung == true).ToLookup(a => a.IdNhanVien);
            var sdtByNv = db.HRM_NhanVien_SoDienThoai.AsNoTracking().Where(a => a.isApDung == true).ToLookup(a => a.IdNhanVien);
            foreach (var aModel in list)
            {
                aModel.listEmail = emailByNv[aModel.IdGuid].ToList();
                aModel.listSoDienThoai = sdtByNv[aModel.IdGuid].ToList();
                aModel.listGiayToTuyThan = cccdByNv[aModel.IdGuid].ToList();
                clsNhanVien _clsNhanVien = _CallWebService.SetNhanSuCreateUser(db, aModel);
                aModel.IdUser = _clsNhanVien.UserPkid;
                aModel.IdNhanVienEOS = _clsNhanVien.Id;


                db.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();
        }
        public dynamic KhoiTaoQuyTrinhSauDanhGia(ApplicationDbContext db, string IdnhanVien, string eAction)
        {
            try
            {
                HRM_NhanVien nhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == IdnhanVien);
                //HRM_NhanVien nhanVienDB = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
                var HopDong = db.HRM_HopDong.FirstOrDefault(a => a.IdNhanVien == IdnhanVien && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString());
                HRM_Luong_NhanVien luongNhanVien = db.HRM_Luong_NhanVien.OrderByDescending(cr => cr.NgayBatDauHieuLuc).FirstOrDefault(r => r.IdNhanVien == IdnhanVien);
                List<HRM_HopDongTaiSanBanGiao> listTs = new List<HRM_HopDongTaiSanBanGiao>();
                if (HopDong != null)
                {
                    var lisTaiSan = db.HRM_dmTrangThietBi.ToList();
                    listTs = db.HRM_HopDongTaiSanBanGiao.Where(a => a.IdHopDong == HopDong.IdGuid).ToList();
                }
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(r => r.IdLuong_NhanVien == IdnhanVien).ToList();
                foreach (var item in listPhuCap)
                {
                    item.NgayBatDauHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDauHieuLuc);
                    item.NgayHetHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHieuLuc);
                }
                if (eAction == DefineData.eAction.QUYTRINHQUYETDINHTHOIVIEC.ToString())
                {
                    var func = _funcNghiViec;
                    HRM_QuyetDinhNghiViec itemAdd = new HRM_QuyetDinhNghiViec();
                    itemAdd.IdCongTyEOS = nhanVien.IdCongTyEOS;
                    itemAdd.IdBoPhanEOS = nhanVien.IdBoPhanEOS;
                    //itemAdd.IdNguoiKy = nhanVienDB.IdGuid;
                    itemAdd.IdNhanVien = nhanVien.IdGuid;
                    //itemAdd.IdChucVuNguoiKy = nhanVienDB.IdChucVu;
                    itemAdd.lstTaiSanBanGiao = new List<HRM_QuyetDinhNghiViecTaiSanBanGiao>();
                    foreach (var item in listTs)
                    {
                        HRM_QuyetDinhNghiViecTaiSanBanGiao ts = new HRM_QuyetDinhNghiViecTaiSanBanGiao();
                        ts.IddmTaiSan = item.IdGuid;
                        itemAdd.lstTaiSanBanGiao.Add(ts);
                    }

                    string SQT = JsonConvert.SerializeObject(func.GetNextQuyetDinhNghiViec(db));
                    itemAdd.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoPhieu;
                    func.SetQuyetDinhNghiViec(itemAdd, db);
                }
                else
                     if (eAction == DefineData.eAction.QUYTRINHDIEUCHINHLUONG.ToString())
                {
                    var func = _funcDieuChinhLuong;
                    HRM_QuyetDinhDieuChinhLuong itemAdd = new HRM_QuyetDinhDieuChinhLuong();
                    itemAdd.IdCongTyEOS = nhanVien.IdCongTyEOS;
                    itemAdd.listIdBoPhan = new List<long>();
                    itemAdd.listIdBoPhan.Add(nhanVien.IdBoPhanEOS);
                    //itemAdd.IdNguoiKy = nhanVienDB.IdGuid;
                    itemAdd.listNhanVien = new List<HRM_QuyetDinhDieuChinhLuong_NhanVien>();
                    var NhanVien = new HRM_QuyetDinhDieuChinhLuong_NhanVien
                    {
                        IdNhanVien = IdnhanVien,
                        IdCongTyEOS = nhanVien.IdCongTyEOS,
                        IdBoPhanEOS = nhanVien.IdBoPhanEOS,
                        LuongThucTeHienTai = luongNhanVien.Luong,
                    };

                    NhanVien.lstPhuCap = new List<HRM_HopDongPhuCap>();
                    foreach (var item in listPhuCap)
                    {
                        HRM_HopDongPhuCap ts = new HRM_HopDongPhuCap();
                        ts.IddmLoaiPhuCap = item.IddmLoaiPhuCap;
                        ts.SoTien = item.SoTien;
                        NhanVien.lstPhuCap.Add(ts);
                    }
                    itemAdd.listNhanVien.Add(NhanVien);
                    string SQT = JsonConvert.SerializeObject(func.GetNextMaDieuChinhLuong(db));
                    itemAdd.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoPhieu;
                    func.SetQuyTrinhDieuChinhLuong(itemAdd, db);
                }
                if (eAction == DefineData.eAction.QUYTRINHQUYETDINHKHENTHUONG.ToString())
                {
                    var func = _funcKhenThuong;
                    HRM_QuyetDinhKhenThuong itemAdd = new HRM_QuyetDinhKhenThuong();
                    itemAdd.IdCongTyEOS = nhanVien.IdCongTyEOS;
                    itemAdd.listIdBoPhan = new List<long>();
                    itemAdd.listIdBoPhan.Add(nhanVien.IdBoPhanEOS);
                    //itemAdd.IdNguoiKy = nhanVienDB.IdGuid;
                    itemAdd.lstNhanVien = new List<HRM_QuyetDinhKhenThuongNhanVien>();
                    itemAdd.lstNhanVien.Add(new HRM_QuyetDinhKhenThuongNhanVien()
                    {
                        IdNhanVien = nhanVien.IdGuid,
                        IdBoPhan = nhanVien.IdBoPhanEOS,
                        IdChucVuEOS = nhanVien.IdChucVu,
                        IdChucDanhEOS = nhanVien.IdChucDanh
                    });
                    string SQT = JsonConvert.SerializeObject(func.GetNextSoPhieuQuyetDinhKhenThuong(db));
                    itemAdd.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoQuyetDinh;
                    func.SetQuyetDinhKhenThuong(db, itemAdd);
                }
                if (eAction == DefineData.eAction.QUYTRINHQUYETDINHKYLUAT.ToString())
                {
                    var func = _funcKyLuat;
                    HRM_QuyetDinhKyLuat itemAdd = new HRM_QuyetDinhKyLuat();
                    itemAdd.IdCongTyEOS = nhanVien.IdCongTyEOS;
                    itemAdd.IdBoPhan = nhanVien.IdBoPhanEOS.ToString();
                    //itemAdd.IdNguoiKy = nhanVienDB.IdGuid;
                    itemAdd.lstNhanVien = new List<HRM_QuyetDinhKyLuatNhanVien>();
                    itemAdd.lstNhanVien.Add(new HRM_QuyetDinhKyLuatNhanVien()
                    {
                        IdNhanVien = nhanVien.IdGuid,
                        IdChucVuEOS = nhanVien.IdChucVu
                    });
                    string SQT = JsonConvert.SerializeObject(func.GetNextSoPhieuQuyetDinhKyLuat(db));
                    itemAdd.SoQuyTrinh = JsonConvert.DeserializeObject<SoQuyTrinh>(SQT.ToString()).SoQuyetDinh;
                    func.SetQuyetDinhKyLuat(db, itemAdd);
                }

                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật  thành công ");
            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật không thành công " + ex.Message);
            }
        }

        public string GetIdNhanVienByIdUser(ApplicationDbContext db)
        {
            try
            {
                HRM_NhanVien nhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);

                if (nhanVien == null)
                    return "";

                return nhanVien.IdGuid;
            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public HRM_QuyTrinhXinNghiPhep_ChiTiet GetIdNhanVienByIdUserNghiPhep(ApplicationDbContext db)
        {
            HRM_QuyTrinhXinNghiPhep_ChiTiet item = new HRM_QuyTrinhXinNghiPhep_ChiTiet();
            try
            {
                HRM_NhanVien nhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString()).ToList();
                var listIdHinhThucNghi = listHinhThucNghi.Select(a => a.Id).ToList();
                DateTime TuNgay = DateTime.Now;
                TuNgay = new DateTime(TuNgay.Year, TuNgay.Month, 1, 0, 0, 0);
                DateTime DenNgay = TuNgay.AddMonths(1).AddDays(-1);
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => nhanVien.IdGuid == x.IdNhanVien && listIdHinhThucNghi.Contains(x.IdHinhThucNghi)
                                                                                 && x.NgayNghi.Year == TuNgay.Year).ToList();
                if (listXinNghiPhep == null)
                    listXinNghiPhep = new List<HRM_TheoDoiXinNghiPhep>();
                item.MaNhanVien = nhanVien.MaNhanVien;
                item.TenNhanVien = nhanVien.TenDayDu;
                item.IdNhanVien = nhanVien.IdGuid;
                item.TenBoPhan = _CallWebService.GetBoPhan(db, nhanVien.IdBoPhanEOS).TenBoPhan;
                item.TenChucDanh = _CallWebService.GetChucDanh(db, nhanVien.IdBoPhanEOS).Ten;
                item.SoPhepConLai = (nhanVien.SoNgayNghiPhep ?? 0) + (nhanVien.SoNgayNghiPhepThamNien ?? 0);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                    item.SoPhepConLai -= listXinNghiPhep.Sum(a => a.ThoiGianNghi);

                if (nhanVien == null)
                    return item;

                return item;
            }
            catch (Exception ex)
            {

                return item;
            }
        }

        public class SoQuyTrinh
        {
            public string SoQuyetDinh { get; set; }
            public string SoPhieu { get; set; }
        }
        public void UpdateIdUserNhanSu(ApplicationDbContext db, string IdUser, long IdNhanVienEOS)
        {
            var nhanVien = db.HRM_NhanVien.Where(a => a.IdNhanVienEOS == IdNhanVienEOS).FirstOrDefault();
            if (nhanVien != null)
            {
                nhanVien.IdUser = IdUser;
                db.Entry(nhanVien).State = EntityState.Modified;
                db.SaveChanges();
            }

        }
        /// Tự động
        public dynamic SetThongTinNhanVienNghiViec(ApplicationDbContext db)
        {
            DateTime time = DateTime.Now;
            var listNhanVien = db.HRM_NhanVien.Where(a => a.NgayNghiViec != null && a.NgayNghiViec.Value.Year == time.Year && a.NgayNghiViec.Value.Month == time.Month && a.NgayNghiViec.Value.Day == time.Day).ToList();
            if (listNhanVien != null && listNhanVien.Count() > 0)
            {
                List<string> listIdNhanVien = listNhanVien.Select(a => a.IdGuid).ToList();
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                Parallel.ForEach(listHopDong, item =>
                {
                    item.TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                    item.NgayHetHan = time;
                    db.Entry(item).State = EntityState.Modified;
                });
                Parallel.ForEach(listNhanVien, item =>
              {
                  item.TrangThai = DefineData.eTrangThaiNhanVien.DaNghiViec.ToString();

                  db.Entry(item).State = EntityState.Modified;
              });

                db.SaveChanges();
            }
            return "Ok";
        }
        public dynamic GetListYeuCauThayDoiThongTinCaNhan(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_NhanVienDieuChinhThongTin> result;
                if (TimKiem.TabTrangThai == 1)

                    result = from a in db.HRM_NhanVienDieuChinhThongTin

                             where a.TrangThai == DefineData.eTrangThaiThayDoiThongTin.ChoDuyet.ToString()

                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_NhanVienDieuChinhThongTin
                             where a.TrangThai != DefineData.eTrangThaiThayDoiThongTin.ChoDuyet.ToString()
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0 && TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayYeuCau >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayYeuCau <= dtDenNgay);
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.TenNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));

                var itemList = Paginate(result.AsNoTracking(), nCurrentPage, PageSize, out PagingInfor paging);
                List<string> listId = result.Select(a => a.IdGuid).ToList();
                var listThayDoi = db.HRM_NhanVienDieuChinhThongTin_ThongTin.Where(a => listId.Contains(a.IdPhieuThayDoi)).ToList();
                //List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                foreach (HRM_NhanVienDieuChinhThongTin item in itemList)
                {
                    if (item.TrangThai == DefineData.eTrangThaiThayDoiThongTin.ChoDuyet.ToString())
                        item.TenTrangThai = "Chờ duyệt";
                    else if (item.TrangThai == DefineData.eTrangThaiThayDoiThongTin.DaDuyet.ToString())
                        item.TenTrangThai = "Đã duyệt";
                    else if (item.TrangThai == DefineData.eTrangThaiThayDoiThongTin.KhongDuyet.ToString())
                        item.TenTrangThai = "Không duyệt";
                    item.ListNoiDung = listThayDoi.Where(a => a.IdPhieuThayDoi == item.IdGuid).Select(a => a.NoiDung).Distinct().ToList();
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
        public dynamic SetYeuCauThayDoiThongTinCaNhan(HRM_NhanVienDieuChinhThongTin entity, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            try
            {

                if (entity.listThayDoi == null || entity.listThayDoi.Count == 0)
                    return err;
                var listThayDoi = entity.listThayDoi.Where(a => a.ThongTinCu != a.ThongTinMoi).FirstOrDefault();
                if (listThayDoi == null)
                    return err;
                if (string.IsNullOrEmpty(entity.IdGuid))
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    DateTime dt = DateTime.Now;
                    entity.NgayYeuCau = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                    entity.MaNhanVien = entity.NhanVien.MaNhanVien;
                    entity.TenNhanVien = entity.NhanVien.TenDayDu;
                    entity.TrangThai = DefineData.eTrangThaiThayDoiThongTin.ChoDuyet.ToString();
                    db.HRM_NhanVienDieuChinhThongTin.Add(entity);
                    entity.NhanVien.IdGuid = "";
                }
                else
                    db.Entry(entity).State = EntityState.Modified;
                List<HRM_NhanVienDieuChinhThongTin_ThongTin> listOld = db.HRM_NhanVienDieuChinhThongTin_ThongTin.Where(a => a.IdPhieuThayDoi == entity.IdGuid).ToList();
                if (listOld != null && listOld.Count > 0)
                {
                    db.HRM_NhanVienDieuChinhThongTin_ThongTin.RemoveRange(listOld);
                }
                entity.NhanVien.IdPhieuThayDoi = entity.IdGuid;
                if (entity.TrangThai == DefineData.eTrangThaiThayDoiThongTin.ChoDuyet.ToString())
                {
                    entity.NhanVien.IdNhanVien = entity.IdNhanVien;

                    DeleteNhanVienThayDoiById(entity.NhanVien.IdGuid, db);
                    SetThayDoiThongTinCaNhan(entity.NhanVien, db);
                }
                else
                {
                    DeleteNhanVienThayDoiById(entity.NhanVien.IdGuid, db);

                    if (entity.TrangThai == DefineData.eTrangThaiThayDoiThongTin.DaDuyet.ToString())
                    {
                        string json = JsonConvert.SerializeObject(entity.NhanVien);
                        HRM_NhanVien nhanvien = JsonConvert.DeserializeObject<HRM_NhanVien>(json.ToString());
                        nhanvien.IdGuid = entity.IdNhanVien;
                        SetThongTinNhanVien(nhanvien, db);
                    }
                }
                List<HRM_NhanVienDieuChinhThongTin_ThongTin> listadd = new List<HRM_NhanVienDieuChinhThongTin_ThongTin>();
                foreach (var item in entity.listThayDoi)
                {
                    if (item.ThongTinCu == item.ThongTinMoi)
                        continue;
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdPhieuThayDoi = entity.IdGuid;
                    listadd.Add(item);
                }
                db.HRM_NhanVienDieuChinhThongTin_ThongTin.AddRange(listadd);
                FileUploadController.SetFileDinhKem(db, entity.listFileDinhKem, nameof(HRM_NhanVienDieuChinhThongTin), entity.IdGuid);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }

            return err;
        }
        public dynamic GetYeuCauThayDoiThongTinCaNhan(string Id, ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            try
            {
                HRM_NhanVienDieuChinhThongTin entity = db.HRM_NhanVienDieuChinhThongTin.FirstOrDefault(a => a.IdGuid == Id);
                entity.listThayDoi = db.HRM_NhanVienDieuChinhThongTin_ThongTin.Where(a => a.IdPhieuThayDoi == entity.IdGuid).OrderBy(a => a.Created).ToList();
                entity.NhanVien = GetThongTinNhanVienThayDoi(entity.IdNhanVien, db);
                entity.listFileDinhKem = FileUploadController.GetListFileDinhKem(db, Id, nameof(HRM_NhanVienDieuChinhThongTin));
                err.Data = entity;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }

            return err;
        }
        public dynamic DeleteNhanVienThayDoiById(string IdNhanVien, ApplicationDbContext db)
        {
            using (var ts = db.Database.BeginTransaction())
            {
                try
                {

                    HRM_NhanVien_Edit nhanVien = db.HRM_NhanVien_Edit.Find(IdNhanVien);
                    if (nhanVien == null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Nhân viên không tồn tại hoặc bị xóa trước đó");
                    }
                    var bangCap = db.HRM_NhanVienBangCap.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var quanheGiadinh = db.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var ngonNgu = db.HRM_NgonNguNhanVien.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var CongTac = db.HRM_QuaTrinhCongTac.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var CongTacNgoaiCty = db.HRM_QuaTrinhCongTacNgoaiCongTy.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var chucChi = db.HRM_ChungChiNhanVien.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    var listThamGiaBaoHiemXaHoi = db.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(r => r.IdNhanVien == IdNhanVien).ToList();
                    db.HRM_QuaTrinhThamGiaBaoHiemXaHoi.RemoveRange(listThamGiaBaoHiemXaHoi);
                    db.HRM_NhanVienBangCap.RemoveRange(bangCap);
                    db.HRM_NhanVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                    db.HRM_NgonNguNhanVien.RemoveRange(ngonNgu);
                    db.HRM_ChungChiNhanVien.RemoveRange(chucChi);
                    db.HRM_QuaTrinhCongTacNgoaiCongTy.RemoveRange(CongTacNgoaiCty);
                    db.HRM_QuaTrinhCongTac.RemoveRange(CongTac);
                    db.HRM_NhanVien_Edit.Remove(nhanVien);
                    db.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra trong quá trình xóa giữ liệu");
                }
            }
        }
        public HRM_NhanVien_Edit GetThongTinNhanVienThayDoi(string IdNhanVien, ApplicationDbContext db)
        {
            var result = new HRM_NhanVien_Edit();
            var model = db.HRM_NhanVien_Edit.FirstOrDefault(x => x.IdNhanVien == IdNhanVien);
            if (model.NgayBatDauChinhThucUnix > 0)
                model.NgayBatDauChinhThuc = UnixTimeStampToDateTime(model.NgayBatDauChinhThucUnix);
            model.NgayNghiViecUnix = DateTimeToUnixTimeStamp(model.NgayNghiViec);
            model.NgayBatDauLamViecUnix = DateTimeToUnixTimeStamp(model.NgayBatDauLamViec);
            model.NgayKetThucThuViecUnix = DateTimeToUnixTimeStamp(model.NgayKetThucThuViec);
            model.NgayBatDauThuViecUnix = DateTimeToUnixTimeStamp(model.NgayBatDauThuViec);
            model.NgayCapMaSTCNUnix = DateTimeToUnixTimeStamp(model.NgayCapMaSTCN);
            model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);

            var anhDaiDien = FileUploadController.GetListFileDinhKem(db, model.IdGuid, DefineData.eFlagDinhKem.AnhDaiDien.ToString());

            if (anhDaiDien != null)
            {
                model.AnhDaiDien = anhDaiDien.FirstOrDefault();
            }
            if (model.TenTrinhDo == null)
                model.TenTrinhDo = db.HRM_dmTrinhDoDaoTao.Where(r => r.IdGuid == model.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
            var listTepDinhKem = FileUploadController.GetListFileDinhKem(db, model.IdGuid, DefineData.eFlagDinhKem.FileDinhKem.ToString());

            if (listTepDinhKem.IsNotEmpty())
            {
                model.lstFileDinhKem = listTepDinhKem;
            }
            var fileDinhkemHoSo = db.HRM_NhanVien_FileDinhKemHoSo.Where(r => r.IdNhanVien == IdNhanVien).ToList();
            var filesHoSo3 = FileUploadController.GetListFileDinhKemByIds(db, fileDinhkemHoSo.Select(x => x.IdGuid), nameof(HRM_NhanVien_FileDinhKemHoSo));
            foreach (var item in fileDinhkemHoSo)
            {
                if (item.NgayCap != null)
                    item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                item.lstFileDinhKem = filesHoSo3[item.IdGuid].ToList();
            }
            model.listFileDinhKemHoSo = fileDinhkemHoSo;
            var listThamGiaBaoHiemXaHoi = db.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).OrderByDescending(a => a.TuNgay).ToList();
            if (listThamGiaBaoHiemXaHoi.IsNotEmpty())
            {
                foreach (var item in listThamGiaBaoHiemXaHoi)
                {
                    if (item.TuNgay != null)
                        item.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.TuNgay);
                    if (item.DenNgay != null)
                        item.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.DenNgay);
                }
            }
            model.listThamGiaBaoHiemXaHoi = listThamGiaBaoHiemXaHoi;
            var listQuanHeGiaDinh = db.HRM_NhanVienQuanHeGiaDinh.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listQuanHeGiaDinh.IsNotEmpty())
            {
                foreach (var item in listQuanHeGiaDinh)
                {

                    if (item.NgaySinh != null)
                        item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                }
            }
            model.ListQuanHeGiaDinhNhanVien = listQuanHeGiaDinh;
            var listGiayToTuyThan = db.HRM_NhanVien_GiayToTuyThan.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listGiayToTuyThan.IsNotEmpty())
            {
                foreach (var item in listGiayToTuyThan)
                {
                    if (item.NgayCap != null)
                        item.NgayCapUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayCap);
                    if (item.NgayHetHan != null)
                        item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
                }
            }
            model.listGiayToTuyThan = listGiayToTuyThan;

            var listBangCap = db.HRM_NhanVienBangCap.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listBangCap.IsNotEmpty())
            {
                var filesBangCap3 = FileUploadController.GetListFileDinhKemByIds(db, listBangCap.Select(x => x.IdGuid), nameof(HRM_NhanVienBangCap));
                foreach (var item in listBangCap)
                {
                    if (item.NgayTotNghiep != null)
                        item.NgayTotNghiepUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayTotNghiep);
                    item.lstFileDinhKem = filesBangCap3[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienBangCap = listBangCap;

            var listNgonNgu = db.HRM_NgonNguNhanVien.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listNgonNgu.IsNotEmpty())
            {
                var filesNgonNgu3 = FileUploadController.GetListFileDinhKemByIds(db, listNgonNgu.Select(x => x.IdGuid), nameof(HRM_NgonNguNhanVien));
                foreach (var item in listNgonNgu)
                {
                    item.lstFileDinhKem = filesNgonNgu3[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienNgonNgu = listNgonNgu;

            var listChungChi = db.HRM_ChungChiNhanVien.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listChungChi.IsNotEmpty())
            {
                var filesChungChi3 = FileUploadController.GetListFileDinhKemByIds(db, listChungChi.Select(x => x.IdGuid), nameof(HRM_ChungChiNhanVien));
                foreach (var item in listChungChi)
                {
                    item.lstFileDinhKem = filesChungChi3[item.IdGuid].ToList();
                }
            }
            model.ListNhanVienChungChi = listChungChi;

            var listCongTac = db.HRM_QuaTrinhCongTac.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listCongTac.IsNotEmpty())
            {
                var filesCongTac3 = FileUploadController.GetListFileDinhKemByIds(db, listCongTac.Select(x => x.IdGuid), nameof(HRM_QuaTrinhCongTac));
                foreach (var item in listCongTac)
                {
                    item.lstFileDinhKem = filesCongTac3[item.IdGuid].ToList();
                }
            }
            model.ListQuaTrinhCongTac = listCongTac;
            model.ListQuaTrinhCongTacNgoaiCongTy = new List<HRM_QuaTrinhCongTacNgoaiCongTy>();
            var listCongTacNuocNgoai = db.HRM_QuaTrinhCongTacNgoaiCongTy.AsNoTracking().Where(x => x.IdNhanVien == model.IdGuid).ToList();
            if (listCongTacNuocNgoai.IsNotEmpty())
            {
                var filesCongTacNgoai3 = FileUploadController.GetListFileDinhKemByIds(db, listCongTacNuocNgoai.Select(x => x.IdGuid), nameof(HRM_QuaTrinhCongTacNgoaiCongTy));
                foreach (var item in listCongTacNuocNgoai)
                {
                    item.lstFileDinhKem = filesCongTacNgoai3[item.IdGuid].ToList();
                }
            }
            model.ListQuaTrinhCongTacNgoaiCongTy = listCongTacNuocNgoai;


            var listNganHang = db.HRM_NhanVien_TaiKhoanNganHang.AsNoTracking().Where(x => x.idNhanVien == model.IdGuid).ToList();
            if (listNganHang.IsNotEmpty())
            {
            }
            model.ListNganHang = listNganHang;
            model.listSoDienThoai = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == model.IdGuid).ToList();
            var listPhuCap = db.HRM_NhanVien_PhuCap.Where(a => a.IdNhanVien == model.IdGuid).ToList();
            var listdmPhuCap = db.HRM_dmPhuCap.ToList();
            model.listPhuCap = new List<HRM_NhanVien_PhuCap>();
            if (listPhuCap != null && listPhuCap.Count > 0)
            {
                model.listPhuCap = listPhuCap.Where(a => a.IdNhanVien == model.IdGuid).OrderBy(a => a.Created).ToList();
                if (model.listPhuCap != null & model.listPhuCap.Count > 0)
                {

                    foreach (var pc in model.listPhuCap)
                    {
                        var Phucap = listdmPhuCap.FirstOrDefault(a => !string.IsNullOrEmpty(pc.IddmPhuCap) && a.IdGuid == pc.IddmPhuCap);
                        if (Phucap != null)
                            pc.TenPhuCap = Phucap.Ten;
                    }
                }
            }

            return result;
        }
        public MessageReturn SetThayDoiThongTinCaNhan(HRM_NhanVien_Edit aModel, ApplicationDbContext _dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới nhân viên thành công");

            var lstHoTen = new List<string>();
            if (!string.IsNullOrEmpty(aModel.Ho))
                lstHoTen.Add(aModel.Ho);
            if (!string.IsNullOrEmpty(aModel.TenDem))
                lstHoTen.Add(aModel.TenDem);
            if (!string.IsNullOrEmpty(aModel.Ten))
                lstHoTen.Add(aModel.Ten);
            if (lstHoTen.Any() == false)
            {

            }
            else
                //Tên đầy đủ
                aModel.TenDayDu = lstHoTen.Join(" ");

            if (aModel.NgayNghiViecUnix > 0)
                aModel.NgayNghiViec = UnixTimeStampToDateTime(aModel.NgayNghiViecUnix);
            if (aModel.NgaySinhUnix > 0)
                aModel.NgaySinh = UnixTimeStampToDateTime(aModel.NgaySinhUnix);
            if (aModel.NgayCapMaSTCNUnix > 0)
                aModel.NgayCapMaSTCN = UnixTimeStampToDateTime(aModel.NgayCapMaSTCNUnix);
            if (aModel.NgayBatDauLamViecUnix > 0)
                aModel.NgayBatDauLamViec = UnixTimeStampToDateTime(aModel.NgayBatDauLamViecUnix);
            if (aModel.NgayBatDauThuViecUnix > 0)
                aModel.NgayBatDauThuViec = UnixTimeStampToDateTime(aModel.NgayBatDauThuViecUnix);
            if (aModel.NgayKetThucThuViecUnix > 0)
                aModel.NgayKetThucThuViec = UnixTimeStampToDateTime(aModel.NgayKetThucThuViecUnix);
            if (aModel.NgayBatDauChinhThucUnix > 0)
                aModel.NgayBatDauChinhThuc = UnixTimeStampToDateTime(aModel.NgayBatDauChinhThucUnix);
            //var listFileCreate = new List<EntityAddFileDinhKemModel>();
            //var listFileCreateNoMove = new List<EntityAddFileDinhKemModel>();
            //var listFileDelete = new List<HRM_FileDinhKem>();
            FileUploadController FileUpload = new FileUploadController();


            try
            {
                if (string.IsNullOrEmpty(aModel.UserName))
                    aModel.UserName = aModel.MaNhanVien;
                if (aModel.IdGuid.IsEmpty())
                {
                    aModel.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_NhanVien_Edit.Add(aModel);
                    _dbContext.SaveChanges();
                }
                else
                {
                    var check = _dbContext.HRM_NhanVien.FirstOrDefault(r => r.MaNhanVien == aModel.MaNhanVien && r.IdGuid != aModel.IdGuid);
                    if (check != null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã nhân viên đã tồn tại");

                    _dbContext.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var listPhuCapold = _dbContext.HRM_NhanVien_PhuCap.Where(a => a.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_NhanVien_PhuCap.RemoveRange(listPhuCapold);
                if (aModel.listPhuCap != null && aModel.listPhuCap.Count > 0)
                {
                    foreach (var item in aModel.listPhuCap)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_NhanVien_PhuCap.AddRange(aModel.listPhuCap);
                }
                var bangCaps = _dbContext.HRM_NhanVienBangCap.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var quanheGiadinh = _dbContext.HRM_NhanVienQuanHeGiaDinh.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var ngonNgus = _dbContext.HRM_NgonNguNhanVien.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var CongTac = _dbContext.HRM_QuaTrinhCongTac.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var CongTacNgoaiCty = _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var chucChi = _dbContext.HRM_ChungChiNhanVien.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var HRM_NhanVien_TaiKhoanNganHang = _dbContext.HRM_NhanVien_TaiKhoanNganHang.Where(r => r.idNhanVien == aModel.IdGuid).ToList();
                var HRM_NhanVien_SoDienThoais = _dbContext.HRM_NhanVien_SoDienThoai.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                var HRM_NhanVien_GiayToTuyThans = _dbContext.HRM_NhanVien_GiayToTuyThan.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_NhanVien_GiayToTuyThan.RemoveRange(HRM_NhanVien_GiayToTuyThans);
                var listThamGiaBaoHiemXaHoi = _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.Where(r => r.IdNhanVien == aModel.IdGuid).ToList();
                _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.RemoveRange(listThamGiaBaoHiemXaHoi);

                _dbContext.HRM_NhanVienBangCap.RemoveRange(bangCaps);
                _dbContext.HRM_NhanVienQuanHeGiaDinh.RemoveRange(quanheGiadinh);
                _dbContext.HRM_NgonNguNhanVien.RemoveRange(ngonNgus);
                _dbContext.HRM_ChungChiNhanVien.RemoveRange(chucChi);
                _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.RemoveRange(CongTacNgoaiCty);
                _dbContext.HRM_QuaTrinhCongTac.RemoveRange(CongTac);
                _dbContext.HRM_NhanVien_TaiKhoanNganHang.RemoveRange(HRM_NhanVien_TaiKhoanNganHang);
                _dbContext.HRM_NhanVien_SoDienThoai.RemoveRange(HRM_NhanVien_SoDienThoais);
                #region FileDinhKemHoSo
                if (aModel.AnhDaiDien != null && aModel.AnhDaiDien.FileName.IsNotEmpty())
                {
                    List<HRM_FileDinhKem> listAnhDaiDien = new List<HRM_FileDinhKem>();
                    listAnhDaiDien.Add(aModel.AnhDaiDien);
                    FileUploadController.SetFileDinhKem(_dbContext, listAnhDaiDien, DefineData.eFlagDinhKem.AnhDaiDien.ToString(), aModel.IdGuid);
                    //listFileCreate.Add(new EntityAddFileDinhKemModel(aModel.AnhDaiDienMoi, aModel.IdGuid, nameof(HRM_NhanVien), DefineData.eFlagDinhKem.AnhDaiDien));
                    //var anhDaiDienCu = _dbContext.HRM_FileDinhKem.FirstOrDefault(x => x.idItem == aModel.IdGuid && x.TableName == nameof(HRM_NhanVien) && x.FlagDinhKem == DefineData.eFlagDinhKem.AnhDaiDien.ToString());
                    //if (anhDaiDienCu != null)
                    //    _dbContext.Entry(anhDaiDienCu).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
                #region ThamGiaBaoHiemXaHoi
                if (aModel.listThamGiaBaoHiemXaHoi.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listThamGiaBaoHiemXaHoi)
                    {
                        if (string.IsNullOrEmpty(bangCap.IdGuid))
                            bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.TuNgay = dungchung.UnixTimeStampToDateTime(bangCap.TuNgayUnix);
                        bangCap.DenNgay = dungchung.UnixTimeStampToDateTime(bangCap.DenNgayUnix);

                        bangCap.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_QuaTrinhThamGiaBaoHiemXaHoi.AddRange(aModel.listThamGiaBaoHiemXaHoi);
                }
                #endregion

                if (aModel.lstFileDinhKem.IsNotEmpty())
                {
                    FileUploadController.SetFileDinhKem(_dbContext, aModel.lstFileDinhKem, DefineData.eFlagDinhKem.FileDinhKem.ToString(), aModel.IdGuid);
                    //listFileCreate.Add(new EntityAddFileDinhKemModel(aModel.lstFileDinhKemMoi, aModel.IdGuid, nameof(HRM_NhanVien), DefineData.eFlagDinhKem.FileDinhKem));
                    //if (aModel.lstFileDinhKemCu.IsNotEmpty())
                    //    foreach (var file in aModel.lstFileDinhKemCu)
                    //        listFileDelete.Add(file);
                }

                if (aModel.listFileDinhKemHoSo != null)
                {
                    foreach (var item in aModel.listFileDinhKemHoSo)
                    {
                        //if (item.IdGuid.IsEmpty())
                        //{
                        //    item.IdNhanVien = aModel.IdGuid;
                        //    item.IdGuid = Guid.NewGuid().ToString();
                        //    if (item.lstFileDinhKemMoi != null)
                        //        listFileCreateNoMove.Add(new EntityAddFileDinhKemModel(item.lstFileDinhKemMoi, item.IdGuid, nameof(HRM_NhanVien_FileDinhKemHoSo), DefineData.eFlagDinhKem.FileDinhKem));
                        //    _dbContext.HRM_NhanVien_FileDinhKemHoSo.Add(item);
                        //}
                        //else
                        //{
                        //    item.IdNhanVien = aModel.IdGuid;
                        //    if (item.lstFileDinhKemMoi != null)
                        //        listFileCreateNoMove.Add(new EntityAddFileDinhKemModel(item.lstFileDinhKemMoi, item.IdGuid, nameof(HRM_NhanVien_FileDinhKemHoSo), DefineData.eFlagDinhKem.FileDinhKem));
                        //    _dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        //}
                        FileUploadController.SetFileDinhKem(_dbContext, item.lstFileDinhKem, nameof(HRM_NhanVien_FileDinhKemHoSo).ToString(), aModel.IdGuid);
                    }


                }
                #endregion
                #region giấy tờ tuy thân
                if (aModel.listGiayToTuyThan.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listGiayToTuyThan)
                    {
                        //if (string.IsNullOrEmpty(bangCap.IdGuid))
                        bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                        if (bangCap.NgayCapUnix > 0)
                            bangCap.NgayCap = UnixTimeStampToDateTime(bangCap.NgayCapUnix);
                        if (bangCap.NgayHetHanUnix > 0)
                            bangCap.NgayHetHan = UnixTimeStampToDateTime(bangCap.NgayHetHanUnix);

                    }
                    _dbContext.HRM_NhanVien_GiayToTuyThan.AddRange(aModel.listGiayToTuyThan);
                }
                #endregion
                #region Số điện thoại
                if (aModel.listSoDienThoai.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.listSoDienThoai)
                    {
                        //if (string.IsNullOrEmpty(bangCap.IdGuid))
                        bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                    }
                    _dbContext.HRM_NhanVien_SoDienThoai.AddRange(aModel.listSoDienThoai);
                }
                #endregion
                #region Bằng cấp
                if (aModel.ListNhanVienBangCap.IsNotEmpty())
                {
                    foreach (var bangCap in aModel.ListNhanVienBangCap)
                    {
                        //if (string.IsNullOrEmpty(bangCap.IdGuid))
                        bangCap.IdGuid = Guid.NewGuid().ToString();
                        bangCap.IdNhanVien = aModel.IdGuid;
                        if (bangCap.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, bangCap.lstFileDinhKem, nameof(HRM_NhanVienBangCap).ToString(), bangCap.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(, bangCap.IdGuid, nameof(HRM_NhanVienBangCap), DefineData.eFlagDinhKem.FileDinhKem));
                        if (bangCap.NgayTotNghiepUnix > 0)
                            bangCap.NgayTotNghiep = dungchung.UnixTimeStampToDateTime(bangCap.NgayTotNghiepUnix);
                    }
                    _dbContext.HRM_NhanVienBangCap.AddRange(aModel.ListNhanVienBangCap);
                }
                #endregion

                #region Ngôn ngữ
                if (aModel.ListNhanVienNgonNgu.IsNotEmpty())
                {
                    foreach (var ngonNgu in aModel.ListNhanVienNgonNgu)
                    {
                        //if (string.IsNullOrEmpty(ngonNgu.IdGuid))
                        ngonNgu.IdGuid = Guid.NewGuid().ToString();
                        ngonNgu.IdNhanVien = aModel.IdGuid;
                        if (ngonNgu.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, ngonNgu.lstFileDinhKem, nameof(HRM_NgonNguNhanVien).ToString(), ngonNgu.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(ngonNgu.lstFileDinhKemMoi, ngonNgu.IdGuid, nameof(HRM_NgonNguNhanVien), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_NgonNguNhanVien.AddRange(aModel.ListNhanVienNgonNgu);
                }
                #endregion
                #region Chứng chỉ
                if (aModel.ListNhanVienChungChi.IsNotEmpty())
                {
                    foreach (var chungChi in aModel.ListNhanVienChungChi)
                    {
                        chungChi.UnixToDateTime();
                        //if (string.IsNullOrEmpty(chungChi.IdGuid))
                        chungChi.IdGuid = Guid.NewGuid().ToString();
                        chungChi.IdNhanVien = aModel.IdGuid;
                        if (chungChi.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, chungChi.lstFileDinhKem, nameof(HRM_ChungChiNhanVien).ToString(), chungChi.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(chungChi.lstFileDinhKemMoi, chungChi.IdGuid, nameof(HRM_ChungChiNhanVien), DefineData.eFlagDinhKem.FileDinhKem));


                    }
                    _dbContext.HRM_ChungChiNhanVien.AddRange(aModel.ListNhanVienChungChi);
                }
                #endregion
                #region  Quá trình công tác
                if (aModel.ListQuaTrinhCongTac.IsNotEmpty())
                {
                    foreach (var congTac in aModel.ListQuaTrinhCongTac)
                    {
                        //if (string.IsNullOrEmpty(congTac.IdGuid))
                        congTac.IdGuid = Guid.NewGuid().ToString();
                        congTac.IdNhanVien = aModel.IdGuid;
                        if (congTac.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, congTac.lstFileDinhKem, nameof(HRM_QuaTrinhCongTac).ToString(), congTac.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(congTac.lstFileDinhKemMoi, congTac.IdGuid, nameof(HRM_QuaTrinhCongTac), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_QuaTrinhCongTac.AddRange(aModel.ListQuaTrinhCongTac);
                }
                #endregion
                #region  Quá trình công tác nước ngoài
                if (aModel.ListQuaTrinhCongTacNgoaiCongTy.IsNotEmpty())
                {
                    foreach (var congTac in aModel.ListQuaTrinhCongTacNgoaiCongTy)
                    {
                        //if (string.IsNullOrEmpty(congTac.IdGuid))
                        congTac.IdGuid = Guid.NewGuid().ToString();
                        congTac.IdNhanVien = aModel.IdGuid;
                        if (congTac.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, congTac.lstFileDinhKem, nameof(HRM_QuaTrinhCongTacNgoaiCongTy).ToString(), congTac.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(congTac.lstFileDinhKemMoi, congTac.IdGuid, nameof(HRM_QuaTrinhCongTacNgoaiCongTy), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_QuaTrinhCongTacNgoaiCongTy.AddRange(aModel.ListQuaTrinhCongTacNgoaiCongTy);
                }
                #endregion

                #region Ngân hàng
                if (aModel.ListNganHang.IsNotEmpty())
                {
                    foreach (var nganHang in aModel.ListNganHang)
                    {
                        nganHang.IdGuid = Guid.NewGuid().ToString();
                        nganHang.idNhanVien = aModel.IdGuid;

                    }
                    _dbContext.HRM_NhanVien_TaiKhoanNganHang.AddRange(aModel.ListNganHang);
                }
                #endregion
                #region Quan hệ gia đình
                if (aModel.ListQuanHeGiaDinhNhanVien.IsNotEmpty())
                {
                    foreach (var giaDinh in aModel.ListQuanHeGiaDinhNhanVien)
                    {
                        if (giaDinh.NgaySinhUnix > 0)
                            giaDinh.NgaySinh = dungchung.UnixTimeStampToDateTime(giaDinh.NgaySinhUnix);
                        //if (string.IsNullOrEmpty(giaDinh.IdGuid))
                        giaDinh.IdGuid = Guid.NewGuid().ToString();
                        giaDinh.IdNhanVien = aModel.IdGuid;
                        if (giaDinh.lstFileDinhKem.IsNotEmpty())
                            FileUploadController.SetFileDinhKem(_dbContext, giaDinh.lstFileDinhKem, nameof(HRM_NhanVienQuanHeGiaDinh).ToString(), giaDinh.IdGuid);
                        //listFileCreate.Add(new EntityAddFileDinhKemModel(giaDinh.lstFileDinhKemMoi, giaDinh.IdGuid, nameof(HRM_NhanVienQuanHeGiaDinh), DefineData.eFlagDinhKem.FileDinhKem));

                    }
                    _dbContext.HRM_NhanVienQuanHeGiaDinh.AddRange(aModel.ListQuanHeGiaDinhNhanVien);
                }
                #endregion
                _dbContext.SaveChanges();


                result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu nhân viên thành công", aModel.IdGuid);

            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }


            return result;
        }
    }
    public class DH_TimKiemNhanVien
    {
        public long IdCongTy { get; set; }
        public List<string> IdUsers { get; set; }
    }
    public class SearchNhanVienViewModel
    {
        public string KeyWord { set; get; }
        public int CurrentPage { set; get; }
        public int PageSize { set; get; }
        public long IdCongTyEOS { set; get; }
        //public double TuNgayUnix { set; get; }
        //public double ToiNgayUnix { set; get; }
        public int TrangThaiNhanVien { set; get; } // 0 tất cả,1 "DangLamViec" ,2 "DaNghiViec" 3, Sắp nghỉ hưu
        public int TrangThaiHopDong { set; get; } ///0 tất cả- 1 săp hết hạn hợp đồng
        //  public string GioiTinh { set; get; }
    }
    public class BaoCaoNhanVienViewModel
    {

        public string IdNhanVien { set; get; }
        public string IddmTrinhDo { set; get; }
        public string TrinhDoChuyenMon { set; get; }
        public string DiaChi { set; get; }
        public DateTime? NgayBatDauLamViec { set; get; }
        public double NgayBatDauLamViecUnix { set; get; }
        public string TenNhanVien { set; get; }
        public string MaNhanVien { set; get; }
        public string Email { set; get; }
        public string TrangThai { set; get; }
        public long IdCongTyEOS { set; get; }
        public string TenCongTyEOS { set; get; }
        public long IddmChucVu { set; get; }
        public string TenChucVu { set; get; }
        public string DienThoai { set; get; }
        public DateTime? NgaySinh { set; get; }
        public double NgaySinhUnix { set; get; }
        public DateTime? NgayNghiViec { set; get; }
        public double NgayNghiViecUnix { set; get; }
        public string GioiTinh { set; get; }
        public DateTime? NgayHieuLuc { set; get; }
        public double NgayHieuLucUnix { set; get; }
        public DateTime? NgayHetHan { set; get; }
        public double NgayHetHanUnix { set; get; }
        public string TenTrangThaiNhanVien { set; get; }
        public int TrangThaiHopDong { set; get; } ///0 tất cả- 1 Sắp về hưu - 2 săp hết hạn hợp đồng
        public string TenTrangThaiHopDong { set; get; }

    }
    public class NhanVienBoPhanViewModel
    {

        public string IdNhanVien { set; get; }
        public string IdHopDong { set; get; }
        public long IdBoPhanEOS { set; get; }
        public long IdChucVuEOS { set; get; }
        public long IddmChucVu { set; get; }
        public long IddmChucDanh { set; get; }
        public string TenChucVu { set; get; }
        public string IdUser { set; get; }
        public string TenNhanVien { set; get; }
        public string TenBoPhan { set; get; }
        public string MaNhanVien { set; get; }

    }

    public class NhatKyNhanVienViewModel
    {
        public string IdGuid { set; get; }
        public string IdNhanVien { set; get; }

        public string TenNhanVien { set; get; }

        public string IdTable { set; get; }

        public string TableName { set; get; }
        public string eAction { set; get; }

        public string Link { set; get; }
        public int Loai { set; get; }

        public string QuyTrinh { set; get; }
        public DateTime ThoiGian { set; get; }

        public double ThoiGianUnix { set; get; }
        public List<HRM_FileDinhKem> listFileDinhKem { get; set; }
    }

    public class NhanVienCongTyViewModel
    {
        public string IdUser { set; get; }
        public string IdGuid { set; get; }
        public string TenDayDu { set; get; }
        public string MaNhanVien { set; get; }

        public long IdCongTyEOS { set; get; }
        public long IdBoPhanEOS { set; get; }
        public string TenBoPhan { set; get; }
        public long IddmChucVu { set; get; }
        public string TenChucVu { set; get; }
        public long IddmChucDanh { set; get; }
        public string IdGioiTinh { set; get; }
        public string IddmTrinhDo { set; get; }
        public long IdNhanVienEOS { set; get; }
        public string IdLoaiNhanVien { set; get; }
        public string IdQuocTich { set; get; }

    }
}
