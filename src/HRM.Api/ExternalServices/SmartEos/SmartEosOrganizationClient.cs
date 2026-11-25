using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.ExternalServices.SmartEos;

public sealed class SmartEosOrganizationClient : SmartEosHttpClientBase, ISmartEosOrganizationClient
{
    private const string RouterNhanSu = "SmartEOSAPI/CoCauNhanSu/";
    private const string RouterDanhMuc = "SmartEOSAPI/DanhMuc/";
    private const string RouterQuanTri = "SmartEOSAPI/QuanTri/";

    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(10);
    private readonly IMemoryCache _cache;

    public SmartEosOrganizationClient(
        HttpClient httpClient,
        ICurrentUserContext currentUserContext,
        IOptions<HrmOptions> options,
        IMemoryCache cache,
        ILogger<SmartEosOrganizationClient> logger)
        : base(httpClient, currentUserContext, options, logger)
    {
        _cache = cache;
    }

    public async Task<string?> CallGetAsync(
        string relativeUrl,
        string? jwt = null,
        IReadOnlyList<clsCookie>? cookies = null,
        CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get, relativeUrl, null, jwt, cookies, includeAuth: true);
        return await SendAndReadStringAsync(request, ct);
    }

    public async Task<string?> CallPostAsync(
        string relativeUrl,
        object? payload,
        string? jwt = null,
        IReadOnlyList<clsCookie>? cookies = null,
        CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, relativeUrl, payload, jwt, cookies, includeAuth: true);
        return await SendAndReadStringAsync(request, ct);
    }

    public async Task<List<BoPhan>> GetDanhSachBoPhanAsync(bool useCache = true, CancellationToken ct = default)
    {
        const string cacheKey = "SmartEOS:BoPhan:All";
        if (useCache && _cache.TryGetValue(cacheKey, out List<BoPhan>? cached) && cached is not null)
        {
            return cached;
        }

        var items = await GetJsonAsync<List<BoPhan>>($"{RouterNhanSu}GetDanhSachBoPhanNotLogin", ct: ct) ?? new List<BoPhan>();
        var filtered = items.Where(a => a.LoaiBoPhanPkid != 3).ToList();

        if (useCache && filtered.Count > 0)
        {
            _cache.Set(cacheKey, filtered, DefaultCacheDuration);
        }

        return filtered;
    }

    public async Task<List<BoPhan>> GetDanhSachCongTyAsync(string? idUser = null, bool useCache = true, CancellationToken ct = default)
    {
        var targetUserId = string.IsNullOrWhiteSpace(idUser) ? CurrentUserContext.UserId : idUser;
        var cacheKey = $"SmartEOS:CongTy:{targetUserId}";

        if (useCache && _cache.TryGetValue(cacheKey, out List<BoPhan>? cached) && cached is not null)
        {
            return cached;
        }

        var data = await GetJsonAsync<List<HRM_DuAn>>($"{RouterDanhMuc}GetDanhSachDuAnByIdUser?IdUser={Uri.EscapeDataString(targetUserId)}", ct: ct)
            ?? new List<HRM_DuAn>();

        var result = data.Select(MapDuAnToBoPhan).ToList();
        if (useCache && result.Count > 0)
        {
            _cache.Set(cacheKey, result, DefaultCacheDuration);
        }

        return result;
    }

    public async Task<List<BoPhan>> GetDanhSachDuAnAsync(bool useCache = true, CancellationToken ct = default)
    {
        const string cacheKey = "SmartEOS:DuAn:All";
        if (useCache && _cache.TryGetValue(cacheKey, out List<BoPhan>? cached) && cached is not null)
        {
            return cached;
        }

        var data = await GetJsonAsync<List<HRM_DuAn>>($"{RouterDanhMuc}GetDanhSachDuAn", ct: ct) ?? new List<HRM_DuAn>();
        var result = data.Select(MapDuAnToBoPhan).ToList();

        if (useCache && result.Count > 0)
        {
            _cache.Set(cacheKey, result, DefaultCacheDuration);
        }

        return result;
    }

    public async Task<List<ChucVu>> GetDanhSachChucVuAsync(bool useCache = true, CancellationToken ct = default)
    {
        const string cacheKey = "SmartEOS:ChucVu:All";
        if (useCache && _cache.TryGetValue(cacheKey, out List<ChucVu>? cached) && cached is not null)
        {
            return cached;
        }

        var result = await GetJsonAsync<List<ChucVu>>($"{RouterNhanSu}GetDanhSachChucVu", ct: ct) ?? new List<ChucVu>();
        if (useCache && result.Count > 0)
        {
            _cache.Set(cacheKey, result, DefaultCacheDuration);
        }

        return result;
    }

    public async Task<List<ChucDanh>> GetDanhSachChucDanhAsync(bool useCache = true, CancellationToken ct = default)
    {
        const string cacheKey = "SmartEOS:ChucDanh:All";
        if (useCache && _cache.TryGetValue(cacheKey, out List<ChucDanh>? cached) && cached is not null)
        {
            return cached;
        }

        var result = await GetJsonAsync<List<ChucDanh>>($"{RouterNhanSu}GetDanhSachChucDanh", ct: ct) ?? new List<ChucDanh>();
        if (useCache && result.Count > 0)
        {
            _cache.Set(cacheKey, result, DefaultCacheDuration);
        }

        return result;
    }

    public async Task<List<clsUser>> GetDanhSachUserByIdsAsync(IReadOnlyCollection<string> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
        {
            return new List<clsUser>();
        }

        return await PostJsonAsync<List<clsUser>>($"{RouterQuanTri}GetDanhSachUserTheoListIdUser", userIds, ct: ct)
            ?? new List<clsUser>();
    }

    public async Task<List<Link_DanhSachDuAn_NhanSu>> GetListUserNhanSuDuAnTheoThoiGianAsync(
        long idDuAn,
        double tuNgayUnix,
        double denNgayUnix,
        CancellationToken ct = default)
    {
        var url = $"{RouterDanhMuc}GetListUserNhanSuDuAnTheoThoiGian?IdDuAn={idDuAn}&TuNgayUnix={tuNgayUnix}&DenNgayUnix={denNgayUnix}";
        var list = await GetJsonAsync<List<Link_DanhSachDuAn_NhanSu>>(url, ct: ct) ?? new List<Link_DanhSachDuAn_NhanSu>();
        return list.OrderBy(a => a.isDieuDong).ToList();
    }

    private static BoPhan MapDuAnToBoPhan(HRM_DuAn src) => new()
    {
        Id = src.Id,
        MaBoPhan = src.MaDuAn,
        TenBoPhan = src.TenDuAn,
        DiaChi = src.DiaChi,
        IddmCaLamViec = src.IddmCaLamViec,
        isNhaMay = src.isNhaMay,
        IddmLoaiDuAn = src.IddmLoaiDuAn
    };
}
