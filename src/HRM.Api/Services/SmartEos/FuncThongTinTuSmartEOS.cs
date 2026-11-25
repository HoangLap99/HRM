﻿#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.Services.SmartEOS
{
    public class FuncThongTinTuSmartEOS : FuncBase
    {
        private readonly IAttendanceSummaryProvider _attendanceProvider;

        public FuncThongTinTuSmartEOS(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IAttendanceSummaryProvider attendanceProvider,
            IOptions<HrmOptions> options,
            ILogger<FuncThongTinTuSmartEOS> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
            _attendanceProvider = attendanceProvider;
        }

        public dynamic GetThongTinNguoiDaiDien(long IdNguoiPhuTrach, ApplicationDbContext db)
        {
            HRM_NhanVien nhanVien = db.HRM_NhanVien.FirstOrDefault(r => r.IdNhanVienEOS == IdNguoiPhuTrach);
            if (nhanVien != null)
            {
                nhanVien.TenChucDanh = _CallWebService.GetTenChucVu_BoPhan(db, nhanVien.IdNhanVienEOS);
                var quocTich = db.HRM_dmQuocTich.Find(nhanVien.IdQuocTich);
                if (quocTich != null)
                {
                    nhanVien.TenQuocTich = quocTich.Ten;
                    nhanVien.QuocTich = quocTich;
                }
                return nhanVien;
            }

            return new HRM_NhanVien();
        }

        public dynamic GetDanhSachNhanVienByIdBoPhan(long IdBoPhan, ApplicationDbContext db)
        {
            var listResult = new List<clsNhanVien>();
            var nhanViendb = db.HRM_NhanVien
                .Where(a => IdBoPhan == a.IdBoPhanEOS && a.TrangThai == eTrangThaiNhanVien.DangLamViec.ToString())
                .ToList();

            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);

            foreach (var item in nhanViendb)
            {
                var nVien = new clsNhanVien
                {
                    IdNhanVien = item.IdGuid,
                    IdUser = item.IdGuid,
                    TenNhanVien = item.TenDayDu,
                    TenDayDu = item.TenDayDu,
                    MaNhanVien = item.MaNhanVien,
                    IddmChucDanh = item.IdChucDanh,
                    IddmChucVu = item.IdChucVu
                };

                var boPhan = listBoPhan.Find(a => a.Id == item.IdBoPhanEOS);
                if (boPhan != null) nVien.TenBoPhan = boPhan.TenBoPhan;

                var chucDanh = listChucDanh.Find(a => a.Id == item.IdChucDanh);
                if (chucDanh != null) nVien.TenChucDanh = chucDanh.Ten;

                var chucVu = listChucVu.Find(a => a.Id == item.IdChucVu);
                if (chucVu != null) nVien.TenChucVu = chucVu.TenChucVu;

                listResult.Add(nVien);
            }

            return listResult;
        }

        public dynamic GetDanhSachNhanVienByListIdBoPhan(long IdCongTy, List<long> IdBoPhans, ApplicationDbContext db)
        {
            var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
            var listIdUser = (listUserDuAn != null && listUserDuAn.Count > 0)
                ? listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList()
                : new List<string>();

            var listResult = new List<clsNhanVien>();
            var nhanViendb = db.HRM_NhanVien
                .Where(a => listIdUser.Contains(a.IdUser)
                            && (IdBoPhans.Count == 0 || IdBoPhans.Contains(a.IdBoPhanEOS))
                            && a.TrangThai == eTrangThaiNhanVien.DangLamViec.ToString())
                .OrderBy(a => a.IdBoPhanEOS)
                .ThenBy(a => a.TenDayDu)
                .ToList();

            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            DateTime time = DateTime.Now;

            foreach (var item in nhanViendb)
            {
                var nVien = new clsNhanVien
                {
                    IdNhanVien = item.IdGuid,
                    IdUser = item.IdGuid,
                    TenNhanVien = item.TenDayDu,
                    TenDayDu = item.TenDayDu,
                    MaNhanVien = item.MaNhanVien,
                    IdBoPhan = item.IdBoPhanEOS,
                    IddmChucDanh = item.IdChucDanh,
                    IddmChucVu = item.IdChucVu
                };

                var boPhan = listBoPhan.Find(a => a.Id == item.IdBoPhanEOS);
                if (boPhan != null) nVien.TenBoPhan = boPhan.TenBoPhan;

                var chucDanh = listChucDanh.Find(a => a.Id == item.IdChucDanh);
                if (chucDanh != null) nVien.TenChucDanh = chucDanh.Ten;

                var chucVu = listChucVu.Find(a => a.Id == item.IdChucVu);
                if (chucVu != null) nVien.TenChucVu = chucVu.TenChucVu;

                if (item.NgayBatDauChinhThuc != null)
                    nVien.ThamNien = (time - item.NgayBatDauChinhThuc.Value).TotalDays / 365;

                listResult.Add(nVien);
            }

            return listResult;
        }

        public dynamic GetDanhSachNhanVienByListIdBoPhanForNghiPhep(long IdCongTy, List<long> IdBoPhans, ApplicationDbContext db)
        {
            try
            {
                var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
                var listIdUser = (listUserDuAn != null && listUserDuAn.Count > 0)
                    ? listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList()
                    : new List<string>();

                var listResult = new List<clsNhanVien>();
                DateTime tuNgay = new(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
                DateTime denNgay = tuNgay.AddMonths(1).AddDays(-1);

                var listdata = new List<HRM_BangChamCong_NhanVien>();
                if (HrmOptions.Host == "ICT")
                {
                    listdata = _attendanceProvider
                        .GetChamCongFromMayChamCongAsync(IdCongTy, DateTimeToUnixTimeStamp(tuNgay), DateTimeToUnixTimeStamp(denNgay), listIdUser)
                        .GetAwaiter()
                        .GetResult();
                }

                var nhanViendb = db.HRM_NhanVien
                    .Where(a => listIdUser.Contains(a.IdUser)
                                && (IdBoPhans.Count == 0 || IdBoPhans.Contains(a.IdBoPhanEOS))
                                && a.TrangThai == eTrangThaiNhanVien.DangLamViec.ToString())
                    .OrderBy(a => a.IdBoPhanEOS)
                    .ThenBy(a => a.TenDayDu)
                    .ToList();

                var listIdNV = nhanViendb.Select(a => a.IdGuid).ToList();
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db)
                    .Where(a => a.Ma == eKyHieuChamCong.PN.ToString()
                             || a.Ma == eKyHieuChamCong.NP.ToString()
                             || a.Ma == eKyHieuChamCong.NQĐ.ToString())
                    .ToList();

                var listIdHinhThucNghi = listHinhThucNghi.Select(a => a.Id).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep
                    .Where(x => listIdNV.Contains(x.IdNhanVien)
                             && listIdHinhThucNghi.Contains(x.IdHinhThucNghi)
                             && x.NgayNghi.Year == tuNgay.Year)
                    .ToList();

                var nghiQuyDoi = listHinhThucNghi.FirstOrDefault(a => a.Ma == eKyHieuChamCong.NQĐ.ToString());
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);

                foreach (var item in nhanViendb)
                {
                    var nVien = new clsNhanVien
                    {
                        IdNhanVien = item.IdGuid,
                        IdUser = item.IdGuid,
                        TenNhanVien = item.TenDayDu,
                        TenDayDu = item.TenDayDu,
                        MaNhanVien = item.MaNhanVien,
                        IdBoPhan = item.IdBoPhanEOS,
                        IddmChucDanh = item.IdChucDanh,
                        IddmChucVu = item.IdChucVu
                    };

                    var boPhan = listBoPhan.Find(a => a.Id == item.IdBoPhanEOS);
                    if (boPhan != null) nVien.TenBoPhan = boPhan.TenBoPhan;

                    var chucDanh = listChucDanh.Find(a => a.Id == item.IdChucDanh);
                    if (chucDanh != null) nVien.TenChucDanh = chucDanh.Ten;

                    var chucVu = listChucVu.Find(a => a.Id == item.IdChucVu);
                    if (chucVu != null) nVien.TenChucVu = chucVu.TenChucVu;

                    double nghiPhep = listXinNghiPhep
                        .Where(a => a.IdNhanVien == item.IdGuid && (nghiQuyDoi == null || a.IdHinhThucNghi != nghiQuyDoi.Id))
                        .Sum(a => a.ThoiGianNghi);

                    nVien.SoPhepConLai = (item.SoNgayNghiPhep ?? 0) + (item.SoNgayNghiPhepThamNien ?? 0) - nghiPhep;

                    if (listdata is { Count: > 0 })
                    {
                        var data = listdata.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        if (data?.listTangCa is { Count: > 0 } && nghiQuyDoi != null)
                        {
                            nVien.CongLamThemGio = data.listTangCa.Sum(a => a.Cong * a.HeSo / 100)
                                - listXinNghiPhep
                                    .Where(a => a.IdNhanVien == item.IdGuid && a.IdHinhThucNghi == nghiQuyDoi.Id)
                                    .Sum(a => a.ThoiGianNghi);
                        }
                    }

                    listResult.Add(nVien);
                }

                return listResult;
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex);
            }
        }
    }
}
