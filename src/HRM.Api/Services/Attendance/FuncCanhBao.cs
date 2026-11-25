#nullable disable
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Persistence;
using Microsoft.Extensions.Logging;

namespace HRM.Api.Services;

/// <summary>
/// Service cảnh báo & đồng bộ lịch làm việc / khảo sát đào tạo (tách từ FuncCronJobs.FuncCanhBao sang DI).
/// </summary>
public sealed class FuncCanhBao
{
    private readonly HrmDbContext _db;
    private readonly CallWebService _callWebService;
    private readonly ISmartEosOrganizationClient _orgClient;
    private readonly ILogger<FuncCanhBao> _logger;

    public FuncCanhBao(
        HrmDbContext db,
        CallWebService callWebService,
        ISmartEosOrganizationClient orgClient,
        ILogger<FuncCanhBao> logger)
    {
        _db = db;
        _callWebService = callWebService;
        _orgClient = orgClient;
        _logger = logger;
    }

    public void TaoCanhBaoNhapThongKeCongDoan()
    {
        try
        {
            _db.currentUser = new CurrentUser
            {
                Id = "admin",
                Authorization = _callWebService.GetJwtTokenForAPI()
            };

            var timeNow = DateTime.Today;
            var listData = new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
            var listCongTy = _callWebService.GetDanhSachDuAnDaDuyet(_db);
            var ngayUnix = dungchung.DateTimeToUnixTimeStamp(timeNow);

            foreach (var item in listCongTy)
            {
                var items = _callWebService.GetKeHoachLamViecFromDieuHanh(_db, item.Id, ngayUnix);
                if (items is { Count: > 0 })
                {
                    listData.AddRange(items);
                }
            }

            var listIdUser = listData.Select(a => a.IdUser).Distinct().ToList();
            var listNhanVien = _db.HRM_NhanVien
                .Where(a => listIdUser.Contains(a.IdUser) && a.TrangThai == eTrangThaiNhanVien.DangLamViec.ToString())
                .ToList();
            var listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();

            var thoiGianLvOld = _db.HRM_ThoiGianLamViecKeHoach
                .Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.Ngay == timeNow)
                .OrderBy(a => a.Ngay)
                .ThenBy(a => a.GioVao)
                .ToList();

            var listIdCaLamViec = listData.Select(a => a.IddmCaLamViec).Distinct().ToList();
            var listCaLamViec = _db.HRM_dmLichLamViec.Where(a => listIdCaLamViec.Contains(a.IdGuid)).ToList();
            var listHeSo = _db.HRM_dmHeSoLamThemGio.ToList();

            var empByUser = listNhanVien.GroupBy(e => e.IdUser).ToDictionary(g => g.Key, g => g.First());
            var shiftById = listCaLamViec.GroupBy(s => s.IdGuid).ToDictionary(g => g.Key, g => g.First());

            var listLvAdd = new List<HRM_ThoiGianLamViecKeHoach>();
            var listLvDelete = new List<HRM_ThoiGianLamViecKeHoach>();

            foreach (var item in listData)
            {
                item.UnixToDateTime();
                if (!empByUser.TryGetValue(item.IdUser ?? string.Empty, out var nhanVien) || item.Ngay == null)
                {
                    continue;
                }

                var time = item.Ngay.Value;
                shiftById.TryGetValue(item.IddmCaLamViec ?? string.Empty, out var caLamViec);

                if (caLamViec?.LoaiNgayNghi == 1 && (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday))
                {
                    continue;
                }
                if ((caLamViec?.LoaiNgayNghi == 2 || caLamViec?.LoaiNgayNghi == 4) && time.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                var heso = !string.IsNullOrEmpty(item.IddmHeSoTangCa)
                    ? listHeSo.FirstOrDefault(a => a.IdGuid == item.IddmHeSoTangCa)
                    : null;

                if (item.listKhungGioByCa != null)
                {
                    foreach (var kg in item.listKhungGioByCa)
                    {
                        int dayOffset = 0;
                        int endHour = kg.GioKetThuc_Gio;
                        if (endHour > 24)
                        {
                            dayOffset = 1;
                            endHour -= 24;
                        }
                        else if (endHour < kg.GioBatDau_Gio)
                        {
                            dayOffset = 1;
                        }

                        var gioVao = new DateTime(time.Year, time.Month, time.Day, kg.GioBatDau_Gio, kg.GioBatDau_Phut, 0);
                        var gioRa = new DateTime(time.Year, time.Month, time.Day, endHour, kg.GioKetThuc_Phut, 0).AddDays(dayOffset);

                        var itemAdd = new HRM_ThoiGianLamViecKeHoach
                        {
                            IdCongTy = item.IdDuAn,
                            IdGuid = Guid.NewGuid().ToString(),
                            GioVao = gioVao,
                            GioRa = gioRa,
                            Ngay = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0),
                            IdNhanVien = nhanVien.IdGuid,
                            isCaGay = false,
                            Loai = 0,
                            SoGioNghi = kg.SoGioNghi,
                            TongSoGioGiao = (gioRa - gioVao).TotalHours,
                            LyDo = item.GhiChu,
                            isLamThemGio = false
                        };

                        if (heso != null)
                        {
                            itemAdd.HeSoTangCa = heso.HeSo;
                            if (heso.HeSo != 100)
                                itemAdd.Loai = 1;
                            itemAdd.IddmHeSoTangCa = item.IddmHeSoTangCa;
                        }

                        if (caLamViec != null)
                        {
                            itemAdd.IddmCaLamViec = caLamViec.IdGuid;
                            itemAdd.GioTieuChuan = caLamViec.SoGioTrongNgay ?? 8;
                            itemAdd.NgayCongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 26;
                        }

                        listLvAdd.Add(itemAdd);
                    }
                }

                var toDelete = thoiGianLvOld.Where(a => a.GioVao == item.TuGio && a.IdNhanVien == nhanVien.IdGuid && a.Ngay == time).ToList();
                if (toDelete.Count > 0)
                {
                    listLvDelete.AddRange(toDelete);
                }
            }

            if (listLvAdd.Count > 0)
                _db.HRM_ThoiGianLamViecKeHoach.AddRange(listLvAdd);
            if (listLvDelete.Count > 0)
                _db.HRM_ThoiGianLamViecKeHoach.RemoveRange(listLvDelete);

            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TaoCanhBaoNhapThongKeCongDoan failed.");
        }
    }

    public void TaoThongBaoKhaoSat()
    {
        try
        {
            _db.currentUser = new CurrentUser { Id = "admin" };
            var today = DateTime.Today;

            var listKhaoSat = _db.HRM_KhaoSatDaoTao
                .Where(a => a.NgayBDKhaoSat != null && a.NgayBDKhaoSat == today)
                .ToList();

            if (listKhaoSat.Count == 0)
            {
                return;
            }

            var listIdNv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ks in listKhaoSat)
            {
                if (string.IsNullOrWhiteSpace(ks.IdNhanVien)) continue;
                foreach (var id in ks.IdNhanVien.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    listIdNv.Add(id);
                }
            }

            var listNhanVien = _db.HRM_NhanVien
                .Where(a => !string.IsNullOrEmpty(a.IdUser) && listIdNv.Contains(a.IdGuid))
                .ToList();

            if (listNhanVien.Count == 0)
            {
                return;
            }

            foreach (var khaoSat in listKhaoSat)
            {
                var targets = listNhanVien.Where(a => khaoSat.IdNhanVien != null && khaoSat.IdNhanVien.Contains(a.IdGuid));
                foreach (var nv in targets)
                {
                    var thongBao = new EOS_ThongBao
                    {
                        LoaiThongBao = "3",
                        TieuDe = khaoSat.LoaiKhaoSat == "KhaoSatDaoTao" ? "Khảo sát nhu cầu đào tạo" : "Khảo sát kết quả đào tạo",
                        NoiDung = "<p>" + khaoSat.NoiDung + " </p>",
                        eAction = "KHAOSATDAOTAO",
                        Idlink = khaoSat.IdGuid,
                        NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(khaoSat.NgayKTKhaoSat),
                        listNhanVienNhan = new List<string> { nv.IdUser }
                    };

                    _orgClient.CallPostAsync("/SmartEOSAPI/Reminder/SetThongBaoChungNonLogin", thongBao)
                        .GetAwaiter()
                        .GetResult();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TaoThongBaoKhaoSat failed.");
        }
    }
}