#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using HRM.Api.Services.NhanVien;
using HRM.Api.Helper;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static HRM.Api.Services.NhanVien.FuncKeHoachLamViec;
namespace HRM.Api.Services.ForDieuHanhNhanSuICT
{
        public class FuncDieuHanhNhanSu : FuncBase
    {
        private readonly FuncChamCong _funcChamCong;
        private readonly FuncKeHoachLamViec _funcKeHoach;
        private string Host => HrmOptions.Host;

        public FuncDieuHanhNhanSu(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncDieuHanhNhanSu> logger,
            FuncChamCong funcChamCong,
            FuncKeHoachLamViec funcKeHoach)
            : base(callWebService, workflowEngine, options, logger)
        {
            _funcChamCong = funcChamCong;
            _funcKeHoach = funcKeHoach;
        }
        public ErrorMessage SetChamCong(ApplicationDbContext _dbContext, List<HRM_CheckInOut> entitys)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            try
            {
                var listIdUser = entitys.Select(a => a.IdUser).Distinct().ToList();
                var listNhanVien = _dbContext.HRM_NhanVien.Where(a => listIdUser.Contains(a.IdUser)).ToList();
                var listId = listNhanVien.Select(a => a.IdGuid).ToList();
                long IdDuAn = entitys.FirstOrDefault().IdCongTy;
                DateTime Ngay = entitys.FirstOrDefault().Ngay;
                var ChamCongOld = _dbContext.HRM_CheckInOut.Where(a => listId.Contains(a.IdUser) && a.IdCongTy == IdDuAn && a.Ngay == Ngay).ToList();

                var listRemove = new List<HRM_CheckInOut>();
                foreach (var entity in entitys)
                {

                    entity.UnixToDateTime();
                    entity.Id = Guid.NewGuid().ToString();
                    var nhanVien = listNhanVien.FirstOrDefault(a => a.IdUser == entity.IdUser);
                    entity.IdNhanVien = nhanVien.IdGuid;
                    //if (entity.Loai == DefineData.eKyHieuChamCong.O.ToString())
                    //{
                    //    var gioVao = _dbContext.HRM_CheckInOut.FirstOrDefault(a => a.Ngay == entity.Ngay);

                    //}
                    entity.Gio = DateTime.Now;
                    entity.Gio = new DateTime(entity.Gio.Year, entity.Gio.Month, entity.Gio.Day, entity.Gio.Hour, entity.Gio.Minute, 0);
                    entity.Ngay = entity.Gio.Date;

                    var check = ChamCongOld.FirstOrDefault(a => a.Loai == entity.Loai && a.IdNhanVien == entity.IdNhanVien);
                    if (check != null)
                        listRemove.Add(check);
                }
                if (listRemove != null && listRemove.Count > 0)
                {
                    _dbContext.HRM_CheckInOut.RemoveRange(listRemove);
                }
                _dbContext.HRM_CheckInOut.AddRange(entitys);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());

            }
            return err;
        }
        public ErrorMessage SetChamCongCaNhan(ApplicationDbContext _dbContext, HRM_CheckInOut entity)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            try
            {
                var nhanVien = _dbContext.HRM_NhanVien.Where(a => a.IdUser == _dbContext.currentUser.Id).FirstOrDefault();
                entity.UnixToDateTime();
                entity.Id = Guid.NewGuid().ToString();
                entity.IdNhanVien = nhanVien.IdGuid;
                entity.Gio = DateTime.Now;
                entity.Ngay = entity.Gio.Date;
                entity.IdCongTy = entity.IdDuAn;
                _dbContext.HRM_CheckInOut.Add(entity);
                _dbContext.SaveChanges();
                _CallWebService.AddModify(_dbContext, "Chấm công", DefineData.eModify.Them.ToString(), nameof(HRM_CheckInOut), "Chấm công", entity.Id, "", JsonConvert.SerializeObject(entity), 0, _dbContext.currentUser.Id, _dbContext.currentUser.TenNhanVien);

            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());

            }
            return err;
        }

        public DashboardCongNhanVien GetThongTinLuong_NgayNghi(ApplicationDbContext db, string IdUser, long IdCongTy)
        {
            DashboardCongNhanVien result = new DashboardCongNhanVien();
            DateTime NgayKT = DateTime.Now;
            DateTime NgayBD = new DateTime(NgayKT.Year, NgayKT.Month, 1, 0, 0, 0);
            var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            var thongTinDuAn = _CallWebService.GetQuyTrinhDuAn(IdCongTy, db);
            List<string> listIdUser = new List<string>();
            if (listUserDuAn != null && listUserDuAn.Count > 0)
                listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser) && a.IdUser == IdUser).Select(a => a.IdUser).Distinct().ToList();
            //List<HRM_BangChamCong_NhanVien> listData = GetBangChamCongICT(listIdUser, dungchung.DateTimeToUnixTimeStamp(NgayBD), dungchung.DateTimeToUnixTimeStamp(NgayKT), db, IdCongTy);
            var funcChamCong = _funcChamCong;
            var chucDanhGS = _CallWebService.GetDanhSachChucVu(db).Where(a => a.MaChucVu == "GS").FirstOrDefault();
            var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == IdUser);
            List<HRM_BangChamCong_NhanVien> listData = new List<HRM_BangChamCong_NhanVien>();
            if (Host == "ICT")
            {
                if (NhanVien.IdChucVu == chucDanhGS.Id)
                    listData = funcChamCong.GetChamCongForGiamSat_ChiTiet(dungchung.DateTimeToUnixTimeStamp(NgayBD), dungchung.DateTimeToUnixTimeStamp(NgayKT), db, listIdUser);
                else
                    listData = funcChamCong.GetChamCongFromMayChamCongICT(IdCongTy, dungchung.DateTimeToUnixTimeStamp(NgayBD), dungchung.DateTimeToUnixTimeStamp(NgayKT), db, false, false, listIdUser);
            }
            else
                listData = funcChamCong.GetChamCongFromMayChamCongPHT(IdCongTy, dungchung.DateTimeToUnixTimeStamp(NgayBD), dungchung.DateTimeToUnixTimeStamp(NgayKT), db, listIdUser);


            if (NhanVien != null)
            {

                result.IdUser = IdUser;
                result.IdNhanVien = NhanVien.IdGuid;
                var HopDong = db.HRM_HopDong.Where(a => NhanVien.IdGuid == a.IdNhanVien && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Created).FirstOrDefault();
                var Luong = db.HRM_Luong_NhanVien.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                if (Luong != null)
                {
                    result.LuongCoBan = Luong.Luong;
                }
                if (listData != null && listData.Count > 0)
                {
                    var Data = listData.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                    if (Data != null)
                    {
                        result.TongSoCongLeTrongThang = (Data.TongSoCong300);
                        if (Data.listGioTieuChuan != null && Data.listGioTieuChuan.Count > 0)
                        {
                            result.TongSoCongTrongThang = Data.listGioTieuChuan.Sum(a => a.TongSoGio / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                            result.LuongNgayCong = Data.listGioTieuChuan.Sum(a => (result.LuongCoBan / (a.CongTieuChuan == 0 ? 26 : a.CongTieuChuan)) * (a.TongSoGio / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan)));
                            result.TongSoGioTrongThang = Data.listGioTieuChuan.Sum(a => a.TongSoGio);
                            double GioTieuChuan = Data.listGioTieuChuan.FirstOrDefault().GioTieuChuan;
                            double CongTieuChuan = Data.listGioTieuChuan.FirstOrDefault().CongTieuChuan;
                            if (thongTinDuAn.CachTinhNgoaiGio == DefineData.eCachTinhNgoaiGio.TheoCongTieuChuan.ToString())
                            {
                                double soGioTieuChuan = CongTieuChuan * GioTieuChuan;
                                if (result.TongSoGioTrongThang > soGioTieuChuan)
                                    result.TongSoGioVuotThang = result.TongSoGioTrongThang - soGioTieuChuan;

                            }
                            else if (thongTinDuAn.CachTinhNgoaiGio == DefineData.eCachTinhNgoaiGio.TheoNgay.ToString() && Data.listTangCa != null && Data.listTangCa.Count > 0)
                            {
                                result.TongSoGioVuotThang = Data.listTangCa.Sum(a => a.TongSoGio);

                            }
                            if (result.TongSoGioVuotThang > 0)
                                result.LuongNgoaiGio = (result.LuongCoBan / (CongTieuChuan == 0 ? 26 : CongTieuChuan)) * (result.TongSoGioVuotThang / (GioTieuChuan == 0 ? 8 : GioTieuChuan));

                        }
                        else
                        {
                            result.TongSoCongTrongThang = Data.TongSoCong ?? 0;
                            result.TongSoGioTrongThang = Data.TongSoCong * Data.SoGioTrongNgay ?? 0;
                            result.LuongNgayCong = (result.LuongCoBan / (Data.SoCongTieuChuan)) * (Data.TongSoCong ?? 0);
                            result.TongSoGioVuotThang = Data.SoGioTangCa;
                            result.LuongNgoaiGio = (result.LuongCoBan / (Data.SoCongTieuChuan)) * (result.TongSoGioVuotThang / (Data.SoGioTrongNgay));
                        }
                    }
                    if (Host == "ICT")
                        Parallel.ForEach(listData, item =>
                        {
                            result.TongSoCong += item.listGioTieuChuan.Sum(a => a.TongSoGio / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                        });
                    else
                        result.TongSoCong += listData.Sum(a => (a.TongSoCong ?? 0));
                }
                var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NP");
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.IdHinhThucNghi == Nghibu.Id && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT).ToList();
                result.SoNgayNgayPhep = (NhanVien.SoNgayNghiPhep ?? 0) + (NhanVien.SoNgayNghiPhepThamNien ?? 0);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                    result.DaNghi = listXinNghiPhep.Sum(a => a.ThoiGianNghi);
                result.ConLai = result.SoNgayNgayPhep - result.DaNghi;
                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => NhanVien.IdBoPhanEOS == a.IdBoPhan).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => a.TuNgay.Month <= NgayKT.Month && listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                {
                    result.listNgayLe = new List<int>();
                    result.isNgayLe = true;
                    foreach (var item in listLichNghiChiTiet)
                    {
                        while (item.TuNgay <= item.DenNgay)
                        {
                            result.listNgayLe.Add(item.TuNgay.Day);
                            item.TuNgay = item.TuNgay.AddDays(1);
                        }
                    }
                }
            }
            return result;
        }
        public dynamic GetThongTinNhanVienByListIdUser(ApplicationDbContext db, List<string> IdUsers, long IdCongTy)
        {
            DateTime time = DateTime.Now;
            time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            List<DanhSachNhanVienViewModel> listNhanVien = (from a in db.HRM_NhanVien
                                                            where IdUsers.Contains(a.IdUser)
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
                                                                IdUser = a.IdUser,
                                                                DiaChi = a.ChoOHienNayTruDiaChi,
                                                                NgayKetThucLamViec = a.NgayNghiViec,
                                                                IdChucDanhEOS = a.IdChucDanh
                                                            }
                                                            ).OrderBy(a => a.MaNhanVien).ToList();
            var listIdNv = listNhanVien.Select(a => a.IdGuid).ToList();
            var listHopDong = db.HRM_HopDong.Where(a => listIdNv.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && a.isKetThuc == true).ToList();
            var listCaLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(a => listIdNv.Contains(a.IdNhanVien) && time == a.Ngay && a.IdCongTy == IdCongTy).ToList();
            var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNv.Contains(a.IdNhanVien) && a.isApDung == true && a.Loai != 3 && a.Loai != 4).ToList();
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var item in listNhanVien)
            {
                item.HopDong = listHopDong.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.SoHopDong).FirstOrDefault();
                item.TenChucDanh = listChucDanh.Where(q => q.Id == item.IdChucDanhEOS).Select(q => q.Ten).FirstOrDefault();
                item.CCCD = listGiayTo.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.So).FirstOrDefault();
                item.NgayCapCCCD = listGiayTo.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.NgayCap).FirstOrDefault();
                if (IdCongTy > 0)
                {
                    var caLamViec = listCaLamViec.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    if (caLamViec != null)
                    {
                        item.SoGioMotNgay = caLamViec.TongSoGioGiao;
                        item.TuGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioVao);
                        item.DenGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioRa);
                    }
                }
                if (item.NgayBatDauLamViec != null)
                    item.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDauLamViec);

                if (item.NgaySinh != null)
                    item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
            }
            return listNhanVien;
        }
        public dynamic GetlistThongTinNhanVien(ApplicationDbContext db, dynamic data)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                DateTime time = DateTime.Now;
                time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in db.HRM_NhanVien
                                                                where a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()
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
                                                                    IdUser = a.IdUser,
                                                                    DiaChi = a.ChoOHienNayTruDiaChi,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh
                                                                }
                                                                ).OrderBy(a => a.MaNhanVien).ToList();

                if (TimKiem.IdBoPhanEOS > 0)
                {

                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhanEOS).ToList();
                }
                if (TimKiem.IdCongTyEOS > 0)
                {
                    var listIdNhanVien = new List<string>();
                    listIdNhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdBoPhan == TimKiem.IdCongTyEOS).Select(a => a.IdQuyTrinh).Distinct().ToList();
                    listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                }

                if (TimKiem.IddmChucDanh > 0)
                {

                    listNhanVien = listNhanVien.Where(a => a.IdChucDanhEOS == TimKiem.IddmChucDanh).ToList();
                }


                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())).ToList();
                int totalRow = listNhanVien.Select(x => x.IdGuid).Count();
                listNhanVien = listNhanVien.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();

                var listIdNv = listNhanVien.Select(a => a.IdGuid).ToList();
                var listHopDong = db.HRM_HopDong.Where(a => listIdNv.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && a.isKetThuc == true).ToList();

                var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNv.Contains(a.IdNhanVien) && a.isApDung == true && a.Loai != 3 && a.Loai != 4).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                foreach (var item in listNhanVien)
                {
                    item.HopDong = listHopDong.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.SoHopDong).FirstOrDefault();
                    item.TenChucDanh = listChucDanh.Where(q => q.Id == item.IdChucDanhEOS).Select(q => q.Ten).FirstOrDefault();
                    item.CCCD = listGiayTo.Where(q => q.IdNhanVien == item.IdGuid).Select(q => q.So).FirstOrDefault();

                    if (item.NgayBatDauLamViec != null)
                        item.NgayBatDauLamViecUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBatDauLamViec);

                    if (item.NgaySinh != null)
                        item.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(item.NgaySinh);
                }
                var paginationSet = new PaginationSet<DanhSachNhanVienViewModel>()
                {
                    Items = listNhanVien,
                    Page = nCurrentPage,
                    TotalCount = totalRow,
                    PageSize = PageSize
                };
                return paginationSet;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());

            }
            return err;
        }
        public dynamic GetlistThongTinNhanVienCoHuu(ApplicationDbContext db)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {

                List<DanhSachNhanVienViewModel> listNhanVien = (from a in db.HRM_NhanVien
                                                                where a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()
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
                                                                    IdUser = a.IdUser,
                                                                    DiaChi = a.ChoOHienNayTruDiaChi,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh
                                                                }
                                                                ).OrderBy(a => a.MaNhanVien).ToList();
                List<string> listIdLoaiHopDong = db.HRM_dmLoaiHopDong.Where(a => a.Ma == "THHĐ" || a.Ma == "KTHHĐ" || a.Ma == "HĐLĐ" || a.Ma == "HĐLĐ.KXDTH").Select(a => a.IdGuid).Distinct().ToList();
                var listHopDong = db.HRM_HopDong.Where(a => a.isKetThuc == true && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).ToList();
                var listCongTy = _CallWebService.GetDanhSachCongTy(db);
                HRM_NhanVienCoHuu result = new HRM_NhanVienCoHuu();

                long IdDuAnTruSo = 0;
                var truSo = listCongTy.FirstOrDefault(a => a.MaBoPhan == "TSHN" || a.MaBoPhan == "HN");
                if (truSo != null)
                    IdDuAnTruSo = truSo.Id;
                List<string> IdNhanVien = listHopDong.Select(a => a.IdNhanVien).Distinct().ToList();
                List<string> IdNhanVienCoHuu = listHopDong.Where(a => listIdLoaiHopDong.Contains(a.IdLoaiHopDong)).Select(a => a.IdNhanVien).Distinct().ToList();
                List<string> IdNhanVienThoiVu = listHopDong.Where(a => a.LoaiLuong != 1).Select(a => a.IdNhanVien).Distinct().ToList();
                // cơ hữu
                {

                    result.CoHuuDuAn = listNhanVien.Where(a => a.IdCongTyEOS != IdDuAnTruSo && IdNhanVienCoHuu.Contains(a.IdGuid)).Count();
                    result.ThoiVuDuAn = listNhanVien.Where(a => a.IdCongTyEOS != IdDuAnTruSo && IdNhanVienThoiVu.Contains(a.IdGuid)).Count();
                    result.CoHuuToanCongTy = listNhanVien.Where(a => IdNhanVienCoHuu.Contains(a.IdGuid)).Count();
                    result.ThoiVuToanCongTy = listNhanVien.Where(a => IdNhanVienThoiVu.Contains(a.IdGuid)).Count();
                }
                return result;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }
        public class HRM_NhanVienCoHuu
        {
            public int CoHuuToanCongTy { get; set; }
            public int ThoiVuToanCongTy { get; set; }
            public int CoHuuDuAn { get; set; }
            public int ThoiVuDuAn { get; set; }
        }
        public dynamic GetlistBangChamCongofCongTy(ApplicationDbContext db, dynamic data)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                DateTime time = new DateTime(TimKiem.Nam ?? 0, TimKiem.Thang ?? 0, 1, 0, 0, 0);
                DateTime timeEnd = time.AddMonths(1).AddDays(-1);
                List<DanhSachNhanVienViewModel> listNhanVien = (from a in db.HRM_NhanVien
                                                                where a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()
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
                                                                    IdUser = a.IdUser,
                                                                    DiaChi = a.ChoOHienNayTruDiaChi,
                                                                    NgayKetThucLamViec = a.NgayNghiViec,
                                                                    IdChucDanhEOS = a.IdChucDanh
                                                                }
                                                                ).OrderBy(a => a.MaNhanVien).ToList();

                if (TimKiem.IdBoPhanEOS > 0)
                {

                    listNhanVien = listNhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhanEOS).ToList();
                }
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())).ToList();
                int totalRow = listNhanVien.Select(x => x.IdGuid).Count();
                listNhanVien = listNhanVien.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();

                var listIdNv = listNhanVien.Select(a => a.IdUser).ToList();
                //var dataresult = GetBangChamCongICT(listIdNv, dungchung.DateTimeToUnixTimeStamp(time), dungchung.DateTimeToUnixTimeStamp(timeEnd), db);
                var funcChamCong = _funcChamCong;
                List<HRM_BangChamCong_NhanVien> listData = new List<HRM_BangChamCong_NhanVien>();
                if (Host == "ICT")
                    listData = funcChamCong.GetChamCongFromMayChamCongICT(0, dungchung.DateTimeToUnixTimeStamp(time), dungchung.DateTimeToUnixTimeStamp(timeEnd), db, false, false, listIdNv);
                else
                    listData = funcChamCong.GetChamCongFromMayChamCongPHT(0, dungchung.DateTimeToUnixTimeStamp(time), dungchung.DateTimeToUnixTimeStamp(timeEnd), db, listIdNv);
                err.Data = listData.FirstOrDefault();
                var paginationSet = new PaginationSet<HRM_BangChamCong_NhanVien>()
                {
                    Items = listData,
                    Page = nCurrentPage,
                    TotalCount = totalRow,
                    PageSize = PageSize
                };
                return paginationSet;
            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }

        public dynamic SetChamCongHo(ApplicationDbContext db, List<string> IdUsers, long IdCongTy)
        {
            DateTime time = DateTime.Now;
            time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            var NhanViens = db.HRM_NhanVien.Where(a => IdUsers.Contains(a.IdUser)).ToList();
            var listIdNv = NhanViens.Select(a => a.IdGuid).ToList();
            var ChamCongOld = db.HRM_CheckInOut.Where(a => listIdNv.Contains(a.IdNhanVien) && a.IdCongTy == IdCongTy && a.Ngay == time).ToList();

            var listRemove = new List<HRM_CheckInOut>();
            var listCaLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(a => listIdNv.Contains(a.IdNhanVien) && time == a.Ngay && a.IdCongTy == IdCongTy).ToList();
            List<HRM_CheckInOut> listCheckInAdd = new List<HRM_CheckInOut>();
            foreach (var item in NhanViens)
            {
                var caLamViec = listCaLamViec.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                if (caLamViec != null && caLamViec.Count > 0)
                {
                    //item.SoGioMotNgay = caLamViec.TongSoGioGiao;
                    //item.TuGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioVao);
                    //item.DenGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioRa);

                    HRM_CheckInOut In = new HRM_CheckInOut();
                    In.Id = Guid.NewGuid().ToString();
                    In.Loai = DefineData.eKyHieuChamCong.I.ToString();
                    In.Ngay = time;
                    In.IdUser = item.IdUser;
                    In.IdNhanVien = item.IdGuid;
                    In.IdCongTy = IdCongTy;
                    DateTime TuGio = caLamViec.OrderBy(a => a.GioVao).FirstOrDefault().GioVao;/* dungchung.UnixTimeStampToDateTime(item.TuGio);*/
                    In.Gio = new DateTime(time.Year, time.Month, time.Day, TuGio.Hour, TuGio.Minute, TuGio.Second);
                    listCheckInAdd.Add(In);
                    HRM_CheckInOut Out = new HRM_CheckInOut();
                    Out.Id = Guid.NewGuid().ToString();
                    Out.Loai = DefineData.eKyHieuChamCong.O.ToString();
                    Out.Ngay = time;
                    Out.IdUser = item.IdUser;
                    Out.IdNhanVien = item.IdGuid;
                    Out.IdCongTy = IdCongTy;
                    DateTime DenGio = caLamViec.OrderByDescending(a => a.GioRa).FirstOrDefault().GioRa; /*dungchung.UnixTimeStampToDateTime(item.DenGio);*/
                    Out.Gio = new DateTime(time.Year, time.Month, time.Day, DenGio.Hour, DenGio.Minute, DenGio.Second);
                    listCheckInAdd.Add(Out);
                    var check = ChamCongOld.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    if (check != null)
                        listRemove.Add(check);
                }
            }
            if (listRemove != null && listRemove.Count > 0)
            {
                db.HRM_CheckInOut.RemoveRange(listRemove);
            }
            db.HRM_CheckInOut.AddRange(listCheckInAdd);
            db.SaveChanges();
            return NhanViens;
        }

        public dynamic GetNgaySinhNhanVienByListIdUser(ApplicationDbContext db, List<string> IdUsers)
        {

            var NhanViens = db.HRM_NhanVien.Where(a => IdUsers.Contains(a.IdUser)).Select(a => new
            {
                IdUser = a.IdUser,
                TenDayDu = a.TenDayDu,
                NgaySinh = a.NgaySinh,
                NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(a.NgaySinh)
            }).ToList();

            return NhanViens;
        }
        public class HRM_NhanVienViewModel
        {
            public string IdGuid { get; set; }
            public string IdUser { get; set; }
            public long IdNhanVienEOS { get; set; }
            public string MaNhanVien { get; set; }

            public string TenDayDu { get; set; }
            public double SoCongLam { get; set; }
            public double SoGioMotNgay { get; set; }
            public double TuGio { get; set; }
            public double DenGio { get; set; }

        }
        public dynamic GetThongTinNhanVienOutsideListIdUser(ApplicationDbContext _dbContext, long IdDuAn)
        {

            DateTime time = DateTime.Now;
            time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            var ChamCongOld = _dbContext.HRM_CheckInOut.Where(a => a.IdCongTy == IdDuAn && a.Ngay == time).ToList();
            List<string> listIdNV = ChamCongOld.Select(a => a.IdNhanVien).ToList();
            var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdDuAn, _dbContext);
            List<string> listIdUserDA = listNhanSuDuAn.Select(a => a.IdUser).ToList();
            var NhanViens = _dbContext.HRM_NhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() && listIdUserDA.Contains(a.IdUser) && !listIdNV.Contains(a.IdGuid)).Select(a => new HRM_NhanVienViewModel
            {
                IdGuid = a.IdGuid,
                IdUser = a.IdUser,
                TenDayDu = a.TenDayDu,
                MaNhanVien = a.MaNhanVien
            }).ToList();
            if (NhanViens != null && NhanViens.Count > 0)
            {
                var listIdNhanVien = NhanViens.Select(a => a.IdGuid).Distinct().ToList();
                var listlichLamViec = _dbContext.HRM_ThoiGianLamViecKeHoach.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.Ngay == time).ToList();


                //var listCaLamViec = _dbContext.HRM_dmLichLamViec.ToList();
                foreach (var item in NhanViens)
                {
                    item.TenDayDu = item.TenDayDu + "(" + item.MaNhanVien + ")";
                    var caLamViec = listlichLamViec.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                    if (caLamViec != null)
                    {
                        item.SoGioMotNgay = caLamViec.GioTieuChuan;
                        item.TuGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioVao);
                        item.DenGio = dungchung.DateTimeToUnixTimeStamp(caLamViec.GioRa);
                    }
                }
            }
            return NhanViens;
        }
        public dynamic GetlistNhanSuNghiViecTrongThang(ApplicationDbContext db, long IdCongTy, int Nam, int Thang)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            if (listNhanSuDuAn != null)
            {

                List<string> listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
                var listNhanVien = (from a in db.HRM_NhanVien
                                    where !string.IsNullOrEmpty(a.IdUser) && listIdUser.Contains(a.IdUser) && a.NgayNghiViec != null && a.NgayNghiViec.Value.Year == Nam && a.NgayNghiViec.Value.Month == Thang
                                    select a
                                  ).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listresult = listNhanVien.Select(a => new DDNS_NhanSu(a)).ToList();
                    foreach (var item in listresult)
                    {
                        var chucdanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (chucdanh != null)
                            item.TenChucDanh = chucdanh.Ten;
                    }
                    err.Data = listresult;
                }
            }

            return err;

        }
        public dynamic GetTongLuongTrongThang(ApplicationDbContext db, long IdCongTy, int Nam, int Thang)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            DateTime time = DateTime.Now;
            time = new DateTime(Nam, Thang, time.Day, 0, 0, 0);
            DateTime ThangTruoc = time.AddMonths(-1);
            DateTime ThangCu = ThangTruoc;

            var BangChamCongs = db.HRM_BangChamCong.Where(a => a.IdCongTyEOS == IdCongTy && a.isKetThuc == true && a.TuNgay <= time && a.DenNgay >= time).ToList();
            if (BangChamCongs == null || BangChamCongs.Count == 0)
            {
                ThangCu = ThangTruoc.AddMonths(-1);
                BangChamCongs = db.HRM_BangChamCong.Where(a => a.IdCongTyEOS == IdCongTy && a.isKetThuc == true && ((a.TuNgay <= ThangTruoc && a.DenNgay >= ThangTruoc) || (a.TuNgay <= ThangCu && a.DenNgay >= ThangCu))).ToList();
            }
            else
            {
                ThangTruoc = time;
                ThangCu = ThangTruoc.AddMonths(-1);
                BangChamCongs.AddRange(db.HRM_BangChamCong.Where(a => a.IdCongTyEOS == IdCongTy && a.isKetThuc == true && a.TuNgay <= ThangCu && a.DenNgay >= ThangCu).ToList());
            }
            DDNS_TongHopLuongDuAn result = new DDNS_TongHopLuongDuAn();
            if (BangChamCongs != null && BangChamCongs.Count > 0)
            {
                List<string> listIdBangChamCong = BangChamCongs.Select(a => a.IdGuid).Distinct().ToList();
                var listBangLuong_C = db.HRM_QuyTrinhTinhLuong_ChamCong.Where(a => listIdBangChamCong.Contains(a.IdGuid)).Distinct().ToList();
                List<string> listIdBangLuong = listBangLuong_C.Select(a => a.IdBangTinhLuong).Distinct().ToList();
                var listDataLuong = (from a in db.HRM_QuyTrinhTinhLuong_NhanVien
                                     where a.isKetThuc == true && listIdBangLuong.Contains(a.IdGuid)
                                     group a by new { a.IdNhanVien, a.IdCongTyEOS } into b
                                     select b.FirstOrDefault()
                                   ).ToList();

                if (listDataLuong != null && listDataLuong.Count > 0)
                {
                    // lương hiện tại
                    var bangChamCongHT = BangChamCongs.Where(a => a.IdCongTyEOS == IdCongTy && a.isKetThuc == true && a.TuNgay <= ThangTruoc && a.DenNgay >= ThangTruoc).ToList();
                    listIdBangChamCong = bangChamCongHT.Select(a => a.IdGuid).Distinct().ToList();
                    listIdBangLuong = listBangLuong_C.Where(a => listIdBangChamCong.Contains(a.IdGuid)).Select(a => a.IdBangTinhLuong).Distinct().ToList();
                    var listLuong = listDataLuong.Where(a => listIdBangLuong.Contains(a.IdGuid)).ToList();
                    if (listLuong != null)
                        result.LuongThangHienTai = listLuong.Sum(a => a.TienThucNhan);


                    // lương tháng trước
                    var bangChamCongTT = BangChamCongs.Where(a => a.IdCongTyEOS == IdCongTy && a.isKetThuc == true && a.TuNgay <= ThangCu && a.DenNgay >= ThangCu).ToList();
                    listIdBangChamCong = bangChamCongTT.Select(a => a.IdGuid).Distinct().ToList();
                    listIdBangLuong = listBangLuong_C.Where(a => listIdBangChamCong.Contains(a.IdGuid)).Select(a => a.IdBangTinhLuong).Distinct().ToList();
                    listLuong = listDataLuong.Where(a => listIdBangLuong.Contains(a.IdGuid)).ToList();
                    if (listLuong != null)
                        result.LuongThangTruoc = listLuong.Sum(a => a.TienThucNhan);

                    if (result.LuongThangHienTai > result.LuongThangTruoc)
                    {
                        result.isTang = true;
                        double tyle = 0;
                        if (result.LuongThangTruoc > 0)
                            tyle = (result.LuongThangHienTai / result.LuongThangTruoc) * 100;
                        else tyle = 100;
                        result.KetLuan = "Tăng " + tyle + "%";

                    }
                    else if (result.LuongThangHienTai < result.LuongThangTruoc)
                    {
                        result.isTang = true;
                        double tyle = 0;
                        if (result.LuongThangHienTai > 0)
                            tyle = (result.LuongThangTruoc / result.LuongThangHienTai) * 100;
                        else tyle = 100;
                        result.KetLuan = "Giảm " + tyle + "%";
                    }

                }
            }
            err.Data = result;
            return err;

        }

        public dynamic GetTyLeNhanSuQuayLaiThang(ApplicationDbContext db, long IdCongTy, int Nam, int Thang)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            //var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            DDNS_TyLeQuayLaiLam result = new DDNS_TyLeQuayLaiLam();
            //if (listNhanSuDuAn != null)
            //{

            //    //List<string> listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
            var listNhanVien = (from a in db.HRM_NhanVien
                                where !string.IsNullOrEmpty(a.IdUser) && a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() || ((a.NgayNghiViec != null && a.NgayNghiViec.Value.Year > 1970) || (a.NgayQuayLai.Value.Year == Nam && a.NgayQuayLai.Value.Month == Thang))
                                select a
                              ).ToList();
            if (listNhanVien != null && listNhanVien.Count > 0)
            {
                var listNhanSuNghi = listNhanVien.Where(a => (a.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString() && a.NgayNghiViec.Value.Year > 1970)).ToList();
                var listNhanSuQuayLai = listNhanVien.Where(a => a.NgayQuayLai != null && a.NgayQuayLai.Value.Year == Nam && a.NgayQuayLai.Value.Month == Thang).ToList();
                int TongNS = listNhanSuNghi.Count() + listNhanSuQuayLai.Count();
                result.DuAn = (listNhanSuQuayLai.Count / (TongNS == 0 ? 1 : TongNS)) * 100;
                int NhanVienNAm = listNhanVien.Where(a => a.IdGioiTinh == "Nam" && a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).Count();
                result.Nam = ((double)NhanVienNAm / (double)listNhanVien.Count) * 100;
                result.Nu = 100 - result.Nam;
            }
            //}

            err.Data = result;
            return err;

        }

        public dynamic GetDoTuoiLaoDongDuAn(ApplicationDbContext db, long IdCongTy)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);

            var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            DDNS_DoTuoiLaoDao result = new DDNS_DoTuoiLaoDao();
            DateTime time = DateTime.Now;
            time = time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            DateTime time20 = time.AddMonths(-(12 * 20));
            DateTime time30 = time.AddMonths(-(12 * 30));
            DateTime time40 = time.AddMonths(-(12 * 40));
            DateTime time50 = time.AddMonths(-(12 * 50));
            if (IdCongTy > 0)
            {
                if (listNhanSuDuAn != null)
                {
                    List<string> listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
                    var listNhanSu = db.HRM_NhanVien.Where(a => listIdUser.Contains(a.IdUser) && a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();

                    if (listNhanSu != null && listNhanSu.Count > 0)
                    {
                        result.Tuoi20_30 = listNhanSu.Where(a => a.NgaySinh <= time20 && a.NgaySinh > time30).Count();
                        result.Tuoi30_40 = listNhanSu.Where(a => a.NgaySinh <= time30 && a.NgaySinh > time40).Count();
                        result.Tuoi40_50 = listNhanSu.Where(a => a.NgaySinh <= time40 && a.NgaySinh > time50).Count();
                        result.TuoiTren50 = listNhanSu.Where(a => a.NgaySinh <= time50).Count();
                    }
                }
            }
            else
            {
                var listNhanSu = db.HRM_NhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();
                if (listNhanSu != null && listNhanSu.Count > 0)
                {
                    result.Tuoi20_30 = listNhanSu.Where(a => a.NgaySinh <= time20 && a.NgaySinh > time30).Count();
                    result.Tuoi30_40 = listNhanSu.Where(a => a.NgaySinh <= time30 && a.NgaySinh > time40).Count();
                    result.Tuoi40_50 = listNhanSu.Where(a => a.NgaySinh <= time40 && a.NgaySinh > time50).Count();
                    result.TuoiTren50 = listNhanSu.Where(a => a.NgaySinh <= time50).Count();
                }
            }
            err.Data = result;
            return err;

        }

        public dynamic GetTinhHinhBienDongDuAn(ApplicationDbContext db, long IdCongTy, int Nam)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            List<DDNS_BienDongNSDuAn> result = new List<DDNS_BienDongNSDuAn>();
            if (listNhanSuDuAn != null)
            {
                List<string> listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
                var listNhanSu = db.HRM_NhanVien.Where(a => listIdUser.Contains(a.IdUser)).ToList();
                //var listNhanSu = db.HRM_NhanVien.ToList();
                if (listNhanSu != null && listNhanSu.Count > 0)
                {
                    for (int i = 1; i < 13; i++)
                    {

                        DDNS_BienDongNSDuAn itemAdd = new DDNS_BienDongNSDuAn();
                        itemAdd.Thang = i;
                        DateTime time = new DateTime(Nam, i, 1);
                        time = time.AddMonths(1).AddDays(-1);
                        itemAdd.TongNhanSu = listNhanSu.Count();
                        itemAdd.NhanSuQuayLai = listNhanSu.Where(a => a.NgayQuayLai != null && a.NgayQuayLai.Value.Year == Nam && a.NgayQuayLai.Value.Month == i).Count();
                        itemAdd.NhanSuNghiViec = listNhanSu.Where(a => a.NgayNghiViec != null && a.NgayNghiViec.Value.Year == Nam && a.NgayNghiViec.Value.Month == i).Count();
                        itemAdd.NhanSuKyMoi = listNhanSu.Where(a => a.NgayQuayLai == null && (a.NgayNghiViec == null || (a.NgayNghiViec >= time)) && a.NgayBatDauLamViec != null && a.NgayBatDauLamViec.Value.Year == Nam && a.NgayBatDauLamViec.Value.Month == i).Count();
                        result.Add(itemAdd);
                    }
                }
            }
            err.Data = result;
            return err;

        }
        public dynamic GetCoCauBoPhanDuAn(ApplicationDbContext db, long IdCongTy)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            List<DDNS_CoCauBoPhanDuAn> result = new List<DDNS_CoCauBoPhanDuAn>();
            if (listNhanSuDuAn != null)
            {
                List<string> listIdUser = listNhanSuDuAn.Where(a => a.isDieuDong == false).Select(a => a.IdUser).Distinct().ToList();
                var listNhanSu = db.HRM_NhanVien.Where(a => listIdUser.Contains(a.IdUser)).ToList();
                if (listNhanSu != null && listNhanSu.Count > 0)
                {
                    List<long> listIdBoPhan = listNhanSu.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    List<string> listId = listNhanSu.Select(a => a.IdGuid).Distinct().ToList();
                    var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                    List<string> listIdLoaiHopDong = db.HRM_dmLoaiHopDong.Where(a => a.Ma == "THHĐ" || a.Ma == "KTHHĐ" || a.Ma == "HĐLĐ").Select(a => a.IdGuid).Distinct().ToList();
                    List<HRM_HopDong> listHopDong = db.HRM_HopDong.Where(a => listId.Contains(a.IdNhanVien) && a.isKetThuc == true && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).GroupBy(a => a.IdNhanVien).Select(a => a.OrderByDescending(b => b.NgayHieuLuc).FirstOrDefault()).ToList();
                    List<string> IdNhanVienCoHuu = listHopDong.Where(a => listIdLoaiHopDong.Contains(a.IdLoaiHopDong)).Select(a => a.IdNhanVien).Distinct().ToList();
                    List<string> IdNhanVienThoiVu = listHopDong.Where(a => a.LoaiLuong != 1).Select(a => a.IdNhanVien).Distinct().ToList();
                    if (listBoPhan != null && listBoPhan.Count > 0)
                        foreach (var bp in listBoPhan)
                        {
                            DDNS_CoCauBoPhanDuAn item = new DDNS_CoCauBoPhanDuAn();
                            item.IdBoPhan = bp.Id;
                            item.TenBoPhan = bp.TenBoPhan;
                            if (listHopDong != null && listHopDong.Count > 0)
                            {
                                var listHD = listHopDong.Where(a => a.IdBoPhanEOS == bp.Id && IdNhanVienCoHuu.Contains(a.IdNhanVien)).ToList();
                                listId = listHD.Select(a => a.IdNhanVien).Distinct().ToList();
                                var listNhanSus = db.HRM_NhanVien.Where(a => listId.Contains(a.IdGuid)).ToList();

                                item.NhanSuThoiVu = listHopDong.Where(a => a.IdBoPhanEOS == bp.Id && IdNhanVienThoiVu.Contains(a.IdNhanVien)).Count();
                                item.NhanSuCoHuu = listHopDong.Where(a => a.IdBoPhanEOS == bp.Id && IdNhanVienCoHuu.Contains(a.IdNhanVien)).Count();
                                item.TongNhanSu = item.NhanSuThoiVu + item.NhanSuCoHuu;
                            }
                            result.Add(item);
                        }
                }
            }
            err.Data = result;
            return err;

        }
        public ErrorMessage GetBangChamCongCaNhanThang(ApplicationDbContext db, long IdCongTy, double NgayUnix)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                if (IdCongTy == 0)
                    return err;
                DateTime time = dungchung.UnixTimeStampToDateTime(NgayUnix);
                var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
                time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                var listCheckIn = db.HRM_CheckInOut.Where(a => a.IdCongTy == IdCongTy && a.IdNhanVien == NhanVien.IdGuid && a.Ngay == time).OrderBy(a => a.Gio).ToList();
                err.Data = listCheckIn;

            }
            catch (Exception ex)
            {
                err.SetLoiVang(ex.ToString());
            }
            return err;
        }
        public List<clsDiMuonVeSom> TinhSomMuon(ApplicationDbContext db, string IdNhanVien, List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs, List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom,
                                                    List<HRM_ThoiGianLamViecKeHoach> listThoiGianLamViec, List<HRM_CheckInOut> listCheckInOut, DateTime Ngay, double SoPhutDuocDiMuon, double SoPhutDuocVeSom, List<HRM_TheoDoiXinNghiPhep> listNghiPhep = null, long IdDuAn = 0)
        {
            listThoiGianLamViec = listThoiGianLamViec.OrderBy(x => x.Ngay).ThenBy(x => x.GioVao).ToList();
            listCheckInOut = listCheckInOut.Where(a => a.IdCongTy == IdDuAn).OrderBy(x => x.Gio).ToList();
            List<clsDiMuonVeSom> listOut = new List<clsDiMuonVeSom>();
            var listCheckIn = listCheckInOut.FindAll(x => x.Loai == DefineData.eKyHieuChamCong.I.ToString());
            var listCheckOut = listCheckInOut.FindAll(x => x.Loai == DefineData.eKyHieuChamCong.O.ToString());

            //string VeSom = HrmOptions.SoPhutSom.ToString();
            //double SoPhutDuocVeSom = double.Parse(VeSom);
            //string DiMuon = HrmOptions.SoPhutMuon.ToString();
            //double SoPhutDuocDiMuon = double.Parse(DiMuon);
            //Thiết lập giờ vào và giờ ra
            List<HRM_ThoiGianLamViecKeHoach> listInOutCD = new List<HRM_ThoiGianLamViecKeHoach>();
            int nCount = listCheckInOut.Count;
            for (int i = 0; i < nCount; i++)
            {
                var itemIn = listCheckInOut[i];
                if (itemIn.Loai == DefineData.eKyHieuChamCong.O.ToString())
                    continue;
                HRM_ThoiGianLamViecKeHoach InOutCD = new HRM_ThoiGianLamViecKeHoach();
                InOutCD.IdCongTy = itemIn.IdCongTy;
                var listCheckInOuts = listCheckInOut.Where(a => a.IdCongTy == itemIn.IdCongTy).ToList();
                nCount = listCheckInOuts.Count;
                InOutCD.GioVao = itemIn.Gio.AddMinutes(-SoPhutDuocDiMuon);
                if (nCount > i + 1)
                {
                    var itemOut = listCheckInOuts[i + 1];
                    if (itemOut != null)
                        InOutCD.GioRa = itemOut.Gio.AddMinutes(SoPhutDuocVeSom);
                }

                if (i + 2 < nCount)
                {
                    var itemVaoTiep = listCheckInOuts[i + 2];
                    if (itemVaoTiep != null)
                        InOutCD.GioVaoTiep = itemVaoTiep.Gio.AddMinutes(-SoPhutDuocDiMuon);
                }
                listInOutCD.Add(InOutCD);
            }
            double SoGioLamViec = 0;
            //Xác định giờ vào ra thực tế so với giờ theo kế hoạch, check ra vào liên tục
            foreach (var item in listThoiGianLamViec)
            {
                SoGioLamViec += (item.GioRa - item.GioVao).TotalMinutes;
                foreach (var itemInOut in listInOutCD.Where(a => a.IdCongTy == item.IdCongTy).ToList())
                {
                    if (itemInOut.GioRa < item.GioVao)
                        continue;
                    if (itemInOut.GioVao > item.GioRa && item.GioVao > item.GioRa)
                        continue;
                    if (item.GioVaoTT == null || item.GioVaoTT > itemInOut.GioVao)
                    {
                        if (item.GioVao > itemInOut.GioVao)
                            item.GioVaoTT = item.GioVao;
                        else
                            item.GioVaoTT = itemInOut.GioVao;

                    }
                    if (item.GioRaTT == null || item.GioRaTT <= itemInOut.GioRa)
                        item.GioRaTT = itemInOut.GioRa;
                    if (itemInOut.GioVaoTiep != null)
                        item.GioVaoTiep = itemInOut.GioVaoTiep;
                }
            }
            listThoiGianLamViec = listThoiGianLamViec.OrderBy(a => a.GioVao).ToList();
            int STT = 0;
            double heSo = 0;
            double SoPhutNghi = 0;
            string MaHinhThucNghi = "";
            if (listNghiPhep != null && listNghiPhep.Count > 0)
            {
                heSo = listNghiPhep.Sum(a => a.ThoiGianNghi);
                SoPhutNghi = heSo * SoGioLamViec;
                MaHinhThucNghi = listNghiPhep.FirstOrDefault().HinhThucNghi;
            }
            int SoPhutDiMuon = 0;
            int SoPhutVeSom = 0;
            var checkSoPhutDiMuon = listDiMuonVeSom.FirstOrDefault(a => a.IdLoai == 1 && a.IdNhanVien == IdNhanVien && a.NgayDangKy == Ngay);
            if (checkSoPhutDiMuon != null)
            {
                SoPhutDiMuon = (int)((checkSoPhutDiMuon.TongGio) * 60);

            }
            var checkSoPhutVeSom = listDiMuonVeSom.FirstOrDefault(a => a.IdLoai == 2 && a.IdNhanVien == IdNhanVien && a.NgayDangKy == Ngay);
            if (checkSoPhutVeSom != null)
            {
                SoPhutVeSom = (int)((checkSoPhutVeSom.TongGio) * 60);

            }

            foreach (var item in listThoiGianLamViec)
            {
                if (IdDuAn != 0 && item.IdCongTy != IdDuAn)
                    continue;
                if (item.GioVaoTT == null && item.GioRaTT == null)
                {
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                    itemOut.SoPhut = (int)(item.GioRa - item.GioVao).TotalMinutes;
                    if (SoPhutNghi > 0)
                    {

                        {
                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == MaHinhThucNghi);
                            if (NghiPhep != null)
                            {
                                itemOut.IddmKyHieuChamCong = NghiPhep.IdGuid;
                                itemOut.KyHieuChamCong = NghiPhep.Ma;
                            }
                            itemOut.isCaGay = item.isCaGay;
                            itemOut.Loai = item.Loai;
                            itemOut.SoPhut = (int)SoPhutNghi;
                            listOut.Add(itemOut);
                        }

                        if (SoPhutNghi >= itemOut.SoPhut)
                        {
                            SoPhutNghi -= itemOut.SoPhut;

                            continue;
                        }
                        else
                        {
                            itemOut.SoPhut = (int)(itemOut.SoPhut - SoPhutNghi);

                        }
                    }
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                    if (dmKyHieuChamCong != null)
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;

                    }

                    itemOut.isCaGay = item.isCaGay;
                    itemOut.Loai = item.Loai;
                    listOut.Add(itemOut);
                    continue;
                }
                if (item.GioVaoTT == null)
                    item.GioVaoTT = item.GioVao;
                if (item.GioRaTT == null)
                    item.GioRaTT = item.GioRa;
                var SoGio = (item.GioVaoTT.Value - item.GioVao).TotalSeconds;
                if (SoGio > 0)
                {
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                    itemOut.GioVao = item.GioVao;
                    itemOut.isCaGay = item.isCaGay;
                    itemOut.Loai = item.Loai;
                    itemOut.SoPhut = (int)(SoGio / 60);
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.M.ToString());
                    if (dmKyHieuChamCong != null)
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;

                        if (itemOut.SoPhut >= SoPhutDiMuon)
                        {
                            itemOut.SoPhut = itemOut.SoPhut - SoPhutDiMuon;
                            SoPhutDiMuon = 0;
                        }
                        else
                        {
                            SoPhutDiMuon -= itemOut.SoPhut;
                            itemOut.SoPhut = 0;
                        }
                    }
                    if (itemOut.SoPhut != 0)
                        listOut.Add(itemOut);
                }
                SoGio = (item.GioRa - item.GioRaTT.Value).TotalSeconds;
                if (SoGio > 0)
                {
                    clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                    itemOut.isCaGay = item.isCaGay;
                    itemOut.Loai = item.Loai;
                    itemOut.SoPhut = (int)(SoGio / 60);
                    HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.S.ToString());
                    if (dmKyHieuChamCong != null)
                    {
                        itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                        itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;
                        if (itemOut.SoPhut >= SoPhutVeSom)
                        {
                            itemOut.SoPhut = itemOut.SoPhut - SoPhutVeSom;
                            SoPhutVeSom = 0;
                        }
                        else
                        {
                            SoPhutVeSom -= itemOut.SoPhut;
                            itemOut.SoPhut = 0;
                        }
                    }
                    if (itemOut.SoPhut != 0)
                        listOut.Add(itemOut);
                }

                //Ra ngoài giữa giờ
                foreach (var itemInOut in listInOutCD)
                {
                    if (itemInOut.GioVaoTiep == null)
                        continue;
                    if (listThoiGianLamViec.Count > STT + 1)
                    {
                        if (itemInOut.GioVaoTiep > item.GioRa && (itemInOut.GioVaoTiep == listThoiGianLamViec[STT + 1].GioVao || (listThoiGianLamViec[STT + 1].GioVaoTT != null && itemInOut.GioVaoTiep <= listThoiGianLamViec[STT + 1].GioVaoTT)))
                            continue;
                    }
                    if (listThoiGianLamViec.Count == STT + 1 && itemInOut.GioVaoTiep >= item.GioRa)
                        continue;
                    if (itemInOut.GioRa >= item.GioVao && itemInOut.GioRa <= item.GioRa)
                    {
                        SoGio = (itemInOut.GioVaoTiep.Value - itemInOut.GioRa).TotalSeconds;
                        if (SoGio > 0)
                        {
                            clsDiMuonVeSom itemOut = new clsDiMuonVeSom(item);
                            itemOut.isCaGay = item.isCaGay;
                            itemOut.Loai = item.Loai;
                            itemOut.SoPhut = (int)(SoGio / 60);
                            HRM_dmKyHieuChamCong dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.G.ToString());
                            if (dmKyHieuChamCong != null)
                            {
                                itemOut.IddmKyHieuChamCong = dmKyHieuChamCong.IdGuid;
                                itemOut.KyHieuChamCong = dmKyHieuChamCong.Ma;
                                var checkXinPhep = listDiMuonVeSom.FirstOrDefault(a => a.IdLoai == 3 && a.IdNhanVien == item.IdNhanVien && a.NgayDangKy == item.Ngay);
                                if (checkXinPhep != null)
                                {
                                    itemOut.SoPhut = itemOut.SoPhut - (int)(checkXinPhep.TongGio * 60);
                                    if (itemOut.SoPhut < 0)
                                        itemOut.SoPhut = 0;
                                }
                            }
                            if (itemOut.SoPhut != 0)
                                listOut.Add(itemOut);
                        }
                    }
                }
                STT++;
            }
            return listOut;
        }

        public dynamic GetBangTongHopCongCaNhanNew(ApplicationDbContext db, string IdNhanVien, double TuNgayUnix, double DenNgayUnix)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);

            DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
            DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
            DDNS_CongCaNhan result = new DDNS_CongCaNhan();
            result.listDuAn = new List<DDNS_DuAn>();
            result.listNgay = new List<DDNS_CongCaNhan_Ngay>();
            var ChucVus = _CallWebService.GetDanhSachChucVu(db);
            var listDuAn = _CallWebService.GetDanhSachDuAn(db);
            var NhanVien = db.HRM_NhanVien.Where(x => x.IdGuid == IdNhanVien).FirstOrDefault();
            DateTime today = DateTime.Today;
            var listdmLoaiNhanVien = db.HRM_dmLoaiNhanVien.ToList();

            List<HRM_ThoiGianLamViecKeHoach> listThoiGianLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => x.IdNhanVien == IdNhanVien
                                                                 && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
            List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut
                                                   where x.IdNhanVien == IdNhanVien && x.Loai == DefineData.eKyHieuChamCong.I.ToString()
                                                                 && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                   //group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                   //select g.OrderBy(a => a.Gio).FirstOrDefault()
                                                   select x
                                                   ).ToList();
            listCheckInOut.AddRange((from x in db.HRM_CheckInOut
                                     where x.IdNhanVien == IdNhanVien && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                                                   && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                     //group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                     //select g.OrderByDescending(a => a.Gio).FirstOrDefault()
                                     select x).ToList());
            List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => x.IdNhanVien == IdNhanVien
                                                                           && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && x.IdNhanVien == IdNhanVien
                                                                                  && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
            var listLoaiNghi = _CallWebService.GetDanhMucLoaiNghiPhep(db);
            var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);

            var listCa = db.HRM_dmLichLamViec.ToList();
            listCheckInOut = listCheckInOut.OrderBy(a => a.Gio).ToList();
            List<HRM_BangChamCong_NhanVien_Ngay> listResult = new List<HRM_BangChamCong_NhanVien_Ngay>();
            List<HRM_ThoiGianLamViecKeHoachView> listInOutCD = new List<HRM_ThoiGianLamViecKeHoachView>();
            int nCount = listCheckInOut.Count;
            string VeSom = HrmOptions.SoPhutSom.ToString();
            double SoPhutDuocVeSom = double.Parse(VeSom);
            string DiMuon = HrmOptions.SoPhutMuon.ToString();

            double SoPhutDuocDiMuon = double.Parse(DiMuon);
            int Count = listCheckInOut.Count;

            for (int i = 0; i < nCount; i++)
            {
                var itemIn = listCheckInOut[i];
                if (itemIn.Loai == DefineData.eKyHieuChamCong.O.ToString())
                    continue;
                HRM_ThoiGianLamViecKeHoachView InOutCD = new HRM_ThoiGianLamViecKeHoachView();
                InOutCD.IdCongTy = itemIn.IdCongTy;
                InOutCD.Ngay = itemIn.Ngay;
                InOutCD.IdNhanVien = itemIn.IdNhanVien;
                //var listCheckInOuts = listCheckInOut.Where(a => a.Ngay == itemIn.Ngay).ToList();
                InOutCD.GioVao = itemIn.Gio;
                if (Count > i + 1)
                {
                    var itemOut = listCheckInOut[i + 1];
                    if (itemOut != null)
                        InOutCD.GioRa = itemOut.Gio;
                }
                if (i + 2 < Count)
                {
                    var itemVaoTiep = listCheckInOut[i + 2];
                    if (itemVaoTiep != null)
                        InOutCD.GioVaoTiep = itemVaoTiep.Gio;
                }
                listInOutCD.Add(InOutCD);
            }


            var LichLamViecs = listThoiGianLamViec.Where(a => a.IdNhanVien == NhanVien.IdGuid).OrderBy(a => a.Ngay).ToList();
            var ListCheckIns = listInOutCD.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
            List<DateTime> listDate = ListCheckIns.Select(a => a.Ngay).Distinct().ToList();
            string LoaiNhanVien = listdmLoaiNhanVien.Where(a => a.IdGuid == NhanVien.IdLoaiNhanVien).Select(a => a.Ten).FirstOrDefault();
            var listIdDuAN = listCheckInOut.Select(a => a.IdCongTy).Distinct().ToList();
            listDuAn = listDuAn.Where(a => listIdDuAN.Contains(a.Id)).ToList();
            foreach (var dA in listDuAn)
            {
                result.listDuAn.Add(new DDNS_DuAn
                {
                    TenDuAn = dA.TenBoPhan,
                    IdDuAn = dA.Id
                });
            }
            result.SoLanChamCong = 1;
            foreach (var date in listDate)
            {
                if (date >= today)
                    break;
                var LamViecNgays = LichLamViecs.Where(a => a.Ngay == date).OrderBy(a => a.GioVao).ToList();
                if (LamViecNgays.Count > result.SoLanChamCong)
                    result.SoLanChamCong = LamViecNgays.Count();
                var ListCheckIn = ListCheckIns.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.Ngay == date).ToList();
                int i = 0;
                if (ListCheckIn == null || ListCheckIn.Count == 0)
                    continue;
                DDNS_CongCaNhan_Ngay ItemNgay = new DDNS_CongCaNhan_Ngay();
                ItemNgay.Ngay = date;
                ItemNgay.Thu = QuyDoiThuVT((int)date.DayOfWeek);
                ItemNgay.listDuAn = new List<DDNS_DuAn>();
                ItemNgay.listChamCong = new List<DDNS_CongCaNhan_ChiTiet>();
                ItemNgay.listDuAn = new List<DDNS_DuAn>();
                foreach (var dA in listDuAn)
                {
                    var itemAddda = new DDNS_DuAn
                    {
                        TenDuAn = dA.TenBoPhan,
                        IdDuAn = dA.Id
                    };
                    double congDuAn = 0;
                    var LamViecNgay = LamViecNgays.Where(a => a.IdCongTy == dA.Id).ToList();
                    if (LamViecNgay != null && LamViecNgay.Count > 0)
                    {
                        foreach (var KH in LamViecNgay)
                        {
                            DDNS_CongCaNhan_ChiTiet itemChamCong = new DDNS_CongCaNhan_ChiTiet();


                            HRM_BangChamCong_NhanVien_Ngay itemAdd = new HRM_BangChamCong_NhanVien_Ngay();
                            var CaLamViec = listCa.Where(a => a.IdGuid == KH.IddmCaLamViec).FirstOrDefault();
                            if (CaLamViec == null)
                                CaLamViec = listCa.Where(a => a.IdGuid == NhanVien.IdCaLamViec).FirstOrDefault();
                            if (CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                                CaLamViec.SoGioTrongNgay = 8;
                            if ((ListCheckIn.Count - 1) < i)
                                continue;
                            var checkIn = ListCheckIn.Where(a => a.GioVao < KH.GioRa && (a.GioRa == null || a.GioRa > KH.GioRa)).FirstOrDefault();
                            if (checkIn != null)
                            {
                                itemChamCong.TuGio = checkIn.GioVao;
                                if (checkIn.GioRa != null && checkIn.GioRa.Value.Year > 2020)
                                {
                                    itemAdd.DenGio = checkIn.GioRa;
                                    itemChamCong.DenGio = checkIn.GioRa;
                                }
                                else
                                {
                                    itemAdd.SoGioQuyDoi = 0;
                                }
                                if (itemChamCong.DenGio != null && itemChamCong.TuGio != null)
                                    itemAdd.SoGioQuyDoi = KH.TongSoGioGiao - (KH.SoGioNghi ?? 0);

                                if (itemAdd.DenGio != null && itemChamCong.DenGio < KH.GioRa && itemChamCong.DenGio > KH.GioVao)
                                {

                                    TimeSpan times = (KH.GioRa - itemChamCong.DenGio.Value);

                                    ItemNgay.Som = times.TotalHours;

                                    itemAdd.SoGioQuyDoi = itemAdd.SoGioQuyDoi - times.TotalHours;
                                }
                                if (itemAdd.TuGio != null && itemAdd.TuGio > KH.GioVao && itemAdd.TuGio < KH.GioRa)
                                {
                                    TimeSpan times = (itemAdd.TuGio.Value - KH.GioVao);
                                    ItemNgay.Tre = times.TotalHours;
                                    itemAdd.SoGioQuyDoi = itemAdd.SoGioQuyDoi - times.TotalHours;

                                }
                                if (itemAdd.SoGioQuyDoi < 0)
                                {
                                    itemAdd.SoGioQuyDoi = 0;
                                }
                                if (KH.Loai == 0)
                                {
                                    ItemNgay.TongGio += itemAdd.SoGioQuyDoi;

                                }
                                else
                                {
                                    ItemNgay.TangCa += itemAdd.SoGioQuyDoi;
                                }

                                itemChamCong.SoGioLamViec = itemAdd.SoCongQuyDoi;
                                ItemNgay.TongCong += (double)(itemAdd.SoGioQuyDoi / CaLamViec.SoGioTrongNgay);
                                congDuAn += (double)(itemAdd.SoGioQuyDoi / CaLamViec.SoGioTrongNgay);
                            }
                            i++;
                            ItemNgay.listChamCong.Add(itemChamCong);
                        }
                        if (congDuAn > 0)
                            itemAddda.Cong = congDuAn.ToString();
                        ItemNgay.listDuAn.Add(itemAddda);
                    }
                }
                result.listNgay.Add(ItemNgay);




            }

            err.Data = result;
            return err;

        }

        public dynamic GetBangTongHopCongCaNhanThang(ApplicationDbContext db, long IdDuAn, double TuNgayUnix, double DenNgayUnix)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            var listDuAn = _CallWebService.GetDanhSachCongTy(db);
            DateTime TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
            DateTime DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
            DDNS_CongCaNhan result = new DDNS_CongCaNhan();
            result.listDuAn = new List<DDNS_DuAn>();
            result.listNgay = new List<DDNS_CongCaNhan_Ngay>();
            List<string> listIdUser = new List<string>();
            listIdUser.Add(db.currentUser.Id);
            var funcChamCong = _funcChamCong;
            List<HRM_BangChamCong_NhanVien> listData = new List<HRM_BangChamCong_NhanVien>();
            var chucDanhGS = _CallWebService.GetDanhSachChucVu(db).Where(a => a.MaChucVu == "GS").FirstOrDefault();
            var NhanVien = db.HRM_NhanVien.Where(a => a.IdUser == db.currentUser.Id).FirstOrDefault();
            if (Host == "ICT")
            {
                if (NhanVien.IdChucVu == chucDanhGS.Id)
                    listData = funcChamCong.GetChamCongForGiamSat_ChiTiet(TuNgayUnix, DenNgayUnix, db, listIdUser);
                else
                    listData = funcChamCong.GetChamCongFromMayChamCongICT(IdDuAn, TuNgayUnix, DenNgayUnix, db, false, false, listIdUser);
            }
            else
                listData = funcChamCong.GetChamCongFromMayChamCongPHT(IdDuAn, TuNgayUnix, DenNgayUnix, db, listIdUser);

            err.Data = listData.FirstOrDefault();
            return err;

        }

        public dynamic GetChamCongNgayByIdUser(ApplicationDbContext db, long IdDuAn)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            DDNS_ChamCongCaNhan result = new DDNS_ChamCongCaNhan();
            DateTime time = DateTime.Today;
            var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
            if (NhanVien != null)
            {
                var listCheckIn = db.HRM_CheckInOut.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.IdCongTy == IdDuAn && a.Ngay == time).ToList();
                if (listCheckIn != null && listCheckIn.Count > 0)
                {
                    var In = listCheckIn.Where(a => a.Loai == DefineData.eKyHieuChamCong.I.ToString()).OrderBy(a => a.Gio).FirstOrDefault();
                    if (In != null)
                        result.GioDen = In.Gio;
                    var Out = listCheckIn.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString()).OrderByDescending(a => a.Gio).FirstOrDefault();
                    if (Out != null)
                        result.GioVe = Out.Gio;

                }
            }
            err.Data = result;
            return err;

        }

        public dynamic GetLichLamViecFromDieuDong(ApplicationDbContext db)
        {
            try
            {
                db.currentUser.Authorization = _CallWebService.GetJwtTokenForAPI();
                DateTime timeNow = DateTime.Today;
                var listCongTy = _CallWebService.GetDanhSachDuAnDaDuyet(db).Where(a=>a.Id== 105 || a.Id== 105).ToList();
                DateTime Tu = new DateTime(2026, 11, 02, 0, 0, 0);
                DateTime Den = new DateTime(2026, 11, 02, 0, 0, 0);
                List<HRM_QuyTrinhLamThemGio_ChiTiet> listData = new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
                foreach (var item in listCongTy)
                {
                    listData.AddRange(_CallWebService.GetKeHoachLamViecFromDieuHanh(db, item.Id, dungchung.DateTimeToUnixTimeStamp(timeNow)));

                }
                //var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                //var ChucVuGS = ChucVus.Where(a => /*a.MaChucVu == "GS" || */a.MaChucVu == "CN").Select(a => a.Id).ToList();
                //listData = listData.Where(a => a.IdUser == "98daf0a9-fb60-4b20-ae73-b651a3231437").ToList();
                var listIdNhanVien = listData.Select(a => a.IdUser).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdUser)).ToList();
                listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                var listIdNUser = listNhanVien.Select(a => a.IdUser).Distinct().ToList();
                listData = listData.Where(a => listIdNUser.Contains(a.IdUser)).ToList();

                var ThoiGianLVOld = (from a in db.HRM_ThoiGianLamViecKeHoach
                                     where listIdNhanVien.Contains(a.IdNhanVien) && Tu <= a.Ngay && Den >= a.Ngay && a.Loai == 0
                                     orderby a.Ngay, a.GioVao
                                     select a
                               ).ToList();
                var listCaLamViec = db.HRM_dmLichLamViec/*.Where(a => listIdCaLamViec.Contains(a.IdGuid))*/.ToList();
                var listHeSo = db.HRM_dmHeSoLamThemGio.ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLVAdd = new List<HRM_ThoiGianLamViecKeHoach>();
                List<HRM_ThoiGianLamViecKeHoach> listLVDelete = new List<HRM_ThoiGianLamViecKeHoach>();
                DateTime time = Tu;
                while (time <= Den)
                {

                    foreach (var item in listData)
                    {
                        item.UnixToDateTime();
                        var NhanVien = listNhanVien.FirstOrDefault(a => a.IdUser == item.IdUser);
                        if (NhanVien == null)
                            continue;
                        if (NhanVien.IdGuid == "1ec76d87-ce55-4c2a-a2e5-a4bd0d05c552")
                            NhanVien.IdGuid = "1ec76d87-ce55-4c2a-a2e5-a4bd0d05c552";
                        var CaLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == item.IddmCaLamViec);
                        if (CaLamViec.LoaiNgayNghi == 1)
                        {
                            if (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday)
                            {
                                time = time.AddDays(1);
                                continue;
                            }
                        }
                        else if (CaLamViec.LoaiNgayNghi == 2 || CaLamViec.LoaiNgayNghi == 4)
                        {
                            if (time.DayOfWeek == DayOfWeek.Sunday)
                            {
                                time = time.AddDays(1);
                                continue;
                            }
                        }
                        var heso = listHeSo.FirstOrDefault(a => string.IsNullOrEmpty(item.IddmHeSoTangCa) && a.IdGuid == item.IddmHeSoTangCa);
                        foreach (var KG in item.listKhungGioByCa)
                        {
                            //TimeSpan timeSpan = item.DenGio.Value - item.TuGio.Value;
                            //double hours = timeSpan.TotalHours;
                            int Day = 0;
                            if (KG.GioKetThuc_Gio > 24)
                            {
                                Day = 1;
                                KG.GioKetThuc_Gio -= 24;
                            }
                            else if (KG.GioKetThuc_Gio < KG.GioBatDau_Gio)
                            {
                                Day = 1;
                            }    
                            HRM_ThoiGianLamViecKeHoach itemAdd = new HRM_ThoiGianLamViecKeHoach();
                            itemAdd.IdCongTy = item.IdDuAn;
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            itemAdd.GioVao = new DateTime(time.Year, time.Month, time.Day, KG.GioBatDau_Gio, KG.GioBatDau_Phut, 0);

                            itemAdd.GioRa = new DateTime(time.Year, time.Month, time.Day , KG.GioKetThuc_Gio, KG.GioKetThuc_Phut, 0);
                            itemAdd.GioRa = itemAdd.GioRa.AddDays(Day);
                            itemAdd.Ngay = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0); ;
                            itemAdd.IdNhanVien = NhanVien.IdGuid;
                            itemAdd.isCaGay = false;
                            itemAdd.Loai = 0;
                            itemAdd.SoGioNghi = KG.SoGioNghi;
                            itemAdd.TongSoGioGiao = (itemAdd.GioRa - itemAdd.GioVao).TotalHours;
                            if (heso != null)
                            {
                                itemAdd.HeSoTangCa = heso.HeSo;
                                if (heso.HeSo != 100)
                                    itemAdd.Loai = 1;
                                itemAdd.IddmHeSoTangCa = item.IddmHeSoTangCa;
                            }
                            if (CaLamViec != null)
                            {
                                itemAdd.IddmCaLamViec = CaLamViec.IdGuid;
                                itemAdd.GioTieuChuan = CaLamViec.SoGioTrongNgay ?? 8;
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 26;
                            }
                            itemAdd.LyDo = item.GhiChu;
                            itemAdd.isLamThemGio = false;
                            listLVAdd.Add(itemAdd);
                        }
                        var Delete = ThoiGianLVOld.Where(a => a.IdCongTy == item.IdDuAn && a.IdNhanVien == NhanVien.IdGuid && a.Ngay == time).ToList();
                        if (Delete != null)
                            listLVDelete.AddRange(Delete);
                    }
                    time = time.AddDays(1);
                }
                if (listLVAdd != null && listLVAdd.Count > 0)
                    db.HRM_ThoiGianLamViecKeHoach.AddRange(listLVAdd);
                if (listLVDelete != null && listLVDelete.Count > 0)
                    db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listLVDelete);


                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công"); ;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
            }
        }
        public dynamic GetLichLamViecFromDieuChinh(ApplicationDbContext db, List<HRM_QuyTrinhLamThemGio_ChiTiet> listData)
        {
            try
            {
                db.currentUser.Authorization = _CallWebService.GetJwtTokenForAPI();
                DateTime timeNow = DateTime.Today;
                DateTime Tu = new DateTime(2025, 11, 26, 0, 0, 0);
                DateTime Den = timeNow;
                var listIdNhanVien = listData.Select(a => a.IdUser).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdUser)).ToList();
                listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                var listIdNUser = listNhanVien.Select(a => a.IdUser).Distinct().ToList();
                listData = listData.Where(a => listIdNUser.Contains(a.IdUser)).ToList();
                Tu = dungchung.UnixTimeStampToDateTime(listData.OrderBy(a => a.NgayUnix).FirstOrDefault().NgayUnix);
                var ThoiGianLVOld = (from a in db.HRM_ThoiGianLamViecKeHoach
                                     where listIdNhanVien.Contains(a.IdNhanVien) && Tu <= a.Ngay && Den >= a.Ngay && a.Loai == 0
                                     orderby a.Ngay, a.GioVao
                                     select a
                               ).ToList();
                var listCaLamViec = db.HRM_dmLichLamViec/*.Where(a => listIdCaLamViec.Contains(a.IdGuid))*/.ToList();
                var listHeSo = db.HRM_dmHeSoLamThemGio.ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLVAdd = new List<HRM_ThoiGianLamViecKeHoach>();
                List<HRM_ThoiGianLamViecKeHoach> listLVDelete = new List<HRM_ThoiGianLamViecKeHoach>();
                DateTime time = Tu;


                foreach (var item in listData)
                {
                    item.UnixToDateTime();
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.IdUser == item.IdUser);
                    if (NhanVien == null)
                        continue;
                    var CaLamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == item.IddmCaLamViec);
                    time = item.Ngay ?? Tu;
                    while (time <= Den)
                    {

                        if (CaLamViec.LoaiNgayNghi == 1)
                        {
                            if (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday)
                            {
                                time = time.AddDays(1);
                                continue; 
                            
                            }
                        }
                        else if (CaLamViec.LoaiNgayNghi == 2 || CaLamViec.LoaiNgayNghi == 4)
                        {
                            if (time.DayOfWeek == DayOfWeek.Sunday)
                            {
                                time = time.AddDays(1);
                                continue;
                            }
                        }
                        var heso = listHeSo.FirstOrDefault(a => string.IsNullOrEmpty(item.IddmHeSoTangCa) && a.IdGuid == item.IddmHeSoTangCa);
                        foreach (var KG in item.listKhungGioByCa)
                        {
                            //TimeSpan timeSpan = item.DenGio.Value - item.TuGio.Value;
                            //double hours = timeSpan.TotalHours;
                            int Day = 0;
                            if (KG.GioKetThuc_Gio > 24)
                            {
                                Day = 1;
                                KG.GioKetThuc_Gio -= 24;
                            }
                            else if (KG.GioKetThuc_Gio < KG.GioBatDau_Gio)
                            {
                                Day = 1;
                            }
                            HRM_ThoiGianLamViecKeHoach itemAdd = new HRM_ThoiGianLamViecKeHoach();
                            itemAdd.IdCongTy = item.IdDuAn;
                            itemAdd.IdGuid = Guid.NewGuid().ToString();
                            itemAdd.GioVao = new DateTime(time.Year, time.Month, time.Day, KG.GioBatDau_Gio, KG.GioBatDau_Phut, 0);

                            itemAdd.GioRa = new DateTime(time.Year, time.Month, time.Day, KG.GioKetThuc_Gio, KG.GioKetThuc_Phut, 0);
                            itemAdd.GioRa = itemAdd.GioRa.AddDays(Day);
                            itemAdd.Ngay = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0); ;
                            itemAdd.IdNhanVien = NhanVien.IdGuid;
                            itemAdd.isCaGay = false;
                            itemAdd.Loai = 0;
                            itemAdd.SoGioNghi = KG.SoGioNghi;
                            itemAdd.TongSoGioGiao = (itemAdd.GioRa - itemAdd.GioVao).TotalHours;
                            if (heso != null)
                            {
                                itemAdd.HeSoTangCa = heso.HeSo;
                                if (heso.HeSo != 100)
                                    itemAdd.Loai = 1;
                                itemAdd.IddmHeSoTangCa = item.IddmHeSoTangCa;
                            }
                            if (CaLamViec != null)
                            {
                                itemAdd.IddmCaLamViec = CaLamViec.IdGuid;
                                itemAdd.GioTieuChuan = CaLamViec.SoGioTrongNgay ?? 8;
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 26;
                            }
                            itemAdd.LyDo = item.GhiChu;
                            itemAdd.isLamThemGio = false;
                            listLVAdd.Add(itemAdd);
                        }
                        var Delete = ThoiGianLVOld.Where(a => a.IdCongTy == item.IdDuAn && a.IdNhanVien == NhanVien.IdGuid && a.Ngay == time).ToList();
                        if (Delete != null)
                            listLVDelete.AddRange(Delete);
                        time = time.AddDays(1);
                    }
                   
                }
                if (listLVAdd != null && listLVAdd.Count > 0)
                    db.HRM_ThoiGianLamViecKeHoach.AddRange(listLVAdd);
                if (listLVDelete != null && listLVDelete.Count > 0)
                    db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listLVDelete);


                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thành công"); ;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
            }
        }



        public class DDNS_ChamCongCaNhan
        {
            public DateTime? GioDen { get; set; }
            public DateTime? GioVe { get; set; }
        }
        public class DDNS_CongCaNhan
        {
            public List<DDNS_CongCaNhan_Ngay> listNgay { get; set; }
            public List<DDNS_DuAn> listDuAn { get; set; }
            public string TenNhanVien { get; set; }
            public int SoLanChamCong { get; set; }
        }

        public class DDNS_CongCaNhan_Ngay
        {
            public DateTime Ngay { get; set; }
            public string Thu { get; set; }
            public string TongCong { get; set; }
            public string CongLe { get; set; }
            public string CongPhep { get; set; }
            public string NgoaiGio { get; set; }
            public double Tre { get; set; } = 0;
            public double Som { get; set; } = 0;
            public double TongGio { get; set; } = 0;
            public double TangCa { get; set; } = 0;
            public List<DDNS_DuAn> listDuAn { get; set; }
            public List<DDNS_CongCaNhan_ChiTiet> listChamCong { get; set; }
        }
        public class DDNS_CongCaNhan_ChiTiet
        {
            public DateTime? TuGio { get; set; } = null;
            public DateTime? DenGio { get; set; } = null;
            public double SoGioLamViec { get; set; }
        }
        public class DDNS_DuAn
        {
            public string TenDuAn { get; set; }
            public long IdDuAn { get; set; }
            public string Cong { get; set; }

        }


        public class DDNS_CoCauBoPhanDuAn
        {
            public string TenBoPhan { get; set; }
            public long IdBoPhan { get; set; }
            public int NhanSuCoHuu { get; set; } = 0;
            public int NhanSuThoiVu { get; set; } = 0;
            public int TongNhanSu { get; set; } = 0;

        }

        public class DDNS_BienDongNSDuAn
        {
            public int Thang { get; set; }
            public int NhanSuNghiViec { get; set; } = 0;
            public int NhanSuKyMoi { get; set; } = 0;
            public int NhanSuQuayLai { get; set; } = 0;
            public int TongNhanSu { get; set; } = 0;

        }

        public class DDNS_TongHopLuongDuAn
        {
            public double LuongThangHienTai { get; set; } = 0;
            public double LuongQLThangHienTai { get; set; } = 0;
            public double LuongThangTruoc { get; set; } = 0;
            public string KetLuan { get; set; }
            public bool? isTang { get; set; }
        }
        public class DDNS_TyLeQuayLaiLam
        {
            public double Nam { get; set; } = 0;
            public double Nu { get; set; } = 0;
            public double? DuAn { get; set; }
        }
        public class DDNS_DoTuoiLaoDao
        {
            public int Tuoi20_30 { get; set; }
            public int Tuoi30_40 { get; set; }
            public int Tuoi40_50 { get; set; }
            public int TuoiTren50 { get; set; }

        }
        public class DDNS_NhanSu
        {
            public string MaNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public DateTime NgaySinh { get; set; }
            public string DiaChi { get; set; }
            public string IdUser { get; set; }
            public long IdChucDanh { get; set; }
            public long IdBoPhan { get; set; }
            public string TenChucDanh { get; set; }
            public string TenBoPhan { get; set; }
            public DateTime? NgayNghiViec { get; set; }
            public string LyDoNghiViec { get; set; }
            public DDNS_NhanSu() { }
            public DDNS_NhanSu(HRM_NhanVien a)
            {
                MaNhanVien = a.MaNhanVien;
                TenNhanVien = a.TenDayDu;
                NgaySinh = a.NgaySinh;
                DiaChi = a.ChoOHienNayTruDiaChi;
                IdChucDanh = a.IdChucDanh;
                TenChucDanh = "";
                IdBoPhan = a.IdBoPhanEOS;
                TenBoPhan = "";
                IdUser = a.IdUser;
                NgayNghiViec = a.NgayNghiViec;
                LyDoNghiViec = a.LyDoNghiViec;
            }
        }
    }
}