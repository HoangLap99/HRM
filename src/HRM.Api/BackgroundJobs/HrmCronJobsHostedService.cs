using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.BackgroundJobs;

/// <summary>
/// Dich vu nen thay the Quartz FuncCronJobs tren .NET 8 (an toan luong voi IServiceScopeFactory).
/// </summary>
public sealed class HrmCronJobsHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HrmOptions _options;
    private readonly ILogger<HrmCronJobsHostedService> _logger;

    private DateTime _lastKeepAliveUtc = DateTime.MinValue;
    private DateOnly _lastDailyCongDoanDate = DateOnly.MinValue;
    private DateTime _lastKhaoSatUtc = DateTime.MinValue;
    private DateOnly _lastDailyHopDongDate = DateOnly.MinValue;

    public HrmCronJobsHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<HrmOptions> options,
        ILogger<HrmCronJobsHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunScheduledJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HrmCronJobsHostedService tick encountered an unhandled exception.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunScheduledJobsAsync(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var nowLocal = DateTime.Now;
        var todayLocal = DateOnly.FromDateTime(nowLocal);
        var isPht = string.Equals(_options.Host, "PHT", StringComparison.OrdinalIgnoreCase);

        if ((nowUtc - _lastKeepAliveUtc) >= TimeSpan.FromMinutes(29))
        {
            _lastKeepAliveUtc = nowUtc;
            await ExecuteKeepAliveAsync(ct);
        }

        if (isPht && nowLocal.Hour == 23 && nowLocal.Minute >= 30 && _lastDailyCongDoanDate != todayLocal)
        {
            _lastDailyCongDoanDate = todayLocal;
            ExecuteScopedCanhBao(svc => svc.TaoCanhBaoNhapThongKeCongDoan());
        }

        if (isPht && (nowUtc - _lastKhaoSatUtc) >= TimeSpan.FromHours(12))
        {
            _lastKhaoSatUtc = nowUtc;
            ExecuteScopedCanhBao(svc => svc.TaoThongBaoKhaoSat());
        }

        if (isPht && nowLocal.Hour == 14 && nowLocal.Minute >= 5 && _lastDailyHopDongDate != todayLocal)
        {
            _lastDailyHopDongDate = todayLocal;
            await ExecuteContractStatusJobAsync(ct);
        }
    }

    private async Task ExecuteKeepAliveAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orgClient = scope.ServiceProvider.GetRequiredService<ISmartEosOrganizationClient>();
            await orgClient.CallGetAsync("HRMAPI/DanhMuc/GetListCachDanhGia", ct: ct);
            await orgClient.CallGetAsync("/SmartEOSAPI/QuanTri/GetJwtTokenForAPI?UserName=admin&App=AuTo", ct: ct);
            await orgClient.CallGetAsync("/DieuHanhCOngViec/BangChamCong/GetNextSoQuyTrinhDangKiDienThoaiChamCong", ct: ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "KeepAlive background sync failed.");
        }
    }

    private void ExecuteScopedCanhBao(Action<FuncCanhBao> action)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var canhBao = scope.ServiceProvider.GetRequiredService<FuncCanhBao>();
            action(canhBao);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Scoped FuncCanhBao background execution failed.");
        }
    }

    private async Task ExecuteContractStatusJobAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HrmDbContext>();
            db.currentUser = new CurrentUser { Id = "admin" };

            var time = DateTime.Now.AddDays(1);
            var dangLamViec = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
            var listNhanVien = await db.HRM_NhanVien
                .Where(a => a.TrangThai == dangLamViec && (a.NgayNghiViec == null || a.NgayNghiViec <= time))
                .ToListAsync(ct);

            if (listNhanVien.Count == 0)
            {
                return;
            }

            var listIdNhanVien = listNhanVien
                .Where(a => a.NgayNghiViec != null && a.NgayNghiViec.Value.Date == time.Date)
                .Select(a => a.IdGuid)
                .ToList();

            if (listIdNhanVien.Count > 0)
            {
                var listHopDong = await db.HRM_HopDong
                    .Where(a => listIdNhanVien.Contains(a.IdNhanVien))
                    .ToListAsync(ct);

                foreach (var hd in listHopDong)
                {
                    hd.TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                    hd.NgayHetHan = time;
                    db.Entry(hd).State = EntityState.Modified;
                }
            }

            foreach (var item in listNhanVien)
            {
                if (item.NgayNghiViec != null && item.NgayNghiViec <= time)
                {
                    item.TrangThai = DefineData.eTrangThaiNhanVien.DaNghiViec.ToString();
                }

                if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.Year != time.Year)
                {
                    item.SoNgayNghiPhepThamNien = (int)((time - item.NgayBatDauLamViec.Value).TotalDays / 365) / 5;
                }

                db.Entry(item).State = EntityState.Modified;
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "ExecuteContractStatusJobAsync failed.");
        }
    }
}