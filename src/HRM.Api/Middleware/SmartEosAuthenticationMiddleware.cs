using System.Security.Cryptography;
using System.Text;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HRM.Api.Middleware;

/// <summary>
/// Middleware xác thực SmartEOS tập trung: trích xuất JWT (?JWT= hoặc Authorization) / Cookies,
/// sử dụng IMemoryCache (TTL 2 phút) và nạp kết quả vào Scoped ICurrentUserContext.
/// </summary>
public sealed class SmartEosAuthenticationMiddleware
{
    private static readonly TimeSpan AuthCacheDuration = TimeSpan.FromMinutes(2);

    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly HrmOptions _options;
    private readonly ILogger<SmartEosAuthenticationMiddleware> _logger;

    public SmartEosAuthenticationMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        IOptions<HrmOptions> options,
        ILogger<SmartEosAuthenticationMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUserContext currentUserContext,
        ISmartEosAuthClient authClient)
    {
        var cookies = ExtractCookies(context.Request);
        var jwt = ExtractJwtToken(context.Request);
        var isLocalhost = IsLocalhost(context.Request.Host.Host);

        CurrentUser? resolvedUser = null;

        if (!string.IsNullOrWhiteSpace(jwt))
        {
            resolvedUser = await ResolveByJwtWithCacheAsync(jwt, authClient, context.RequestAborted);
        }
        else if (cookies.Count > 0 && !(isLocalhost && _options.EnableLocalhostDevFallback))
        {
            resolvedUser = await ResolveByCookiesWithCacheAsync(cookies, authClient, context.RequestAborted);
        }

        if (resolvedUser is null && isLocalhost && _options.EnableLocalhostDevFallback)
        {
            resolvedUser = new CurrentUser(_options.LocalhostDefaultUserId)
            {
                UserName = _options.LocalhostDefaultUserName,
                TenNhanVien = _options.LocalhostDefaultUserName,
                Authorization = jwt,
                listCookie = cookies
            };
        }

        if (resolvedUser is not null && !string.IsNullOrWhiteSpace(resolvedUser.Id))
        {
            currentUserContext.SetUser(resolvedUser, jwt, cookies);
        }

        await _next(context);
    }

    private async Task<CurrentUser?> ResolveByJwtWithCacheAsync(
        string jwt,
        ISmartEosAuthClient authClient,
        CancellationToken ct)
    {
        var cacheKey = $"SmartEOS:Auth:Jwt:{ComputeSha256(jwt)}";
        if (_cache.TryGetValue(cacheKey, out CurrentUser? cachedUser) && cachedUser is not null)
        {
            return CloneUser(cachedUser);
        }

        var user = await authClient.GetCurrentUserByJwtAsync(jwt, ct);
        if (user is not null && !string.IsNullOrWhiteSpace(user.Id))
        {
            _cache.Set(cacheKey, user, AuthCacheDuration);
            return CloneUser(user);
        }

        _logger.LogDebug("SmartEOS JWT verification returned empty user.");
        return null;
    }

    private async Task<CurrentUser?> ResolveByCookiesWithCacheAsync(
        IReadOnlyList<clsCookie> cookies,
        ISmartEosAuthClient authClient,
        CancellationToken ct)
    {
        var rawCookieSignature = string.Join(";", cookies.OrderBy(c => c.name).Select(c => $"{c.name}={c.value}"));
        var cacheKey = $"SmartEOS:Auth:Cookie:{ComputeSha256(rawCookieSignature)}";

        if (_cache.TryGetValue(cacheKey, out CurrentUser? cachedUser) && cachedUser is not null)
        {
            return CloneUser(cachedUser);
        }

        var user = await authClient.GetCurrentUserByCookiesAsync(cookies, ct);
        if (user is not null && !string.IsNullOrWhiteSpace(user.Id))
        {
            _cache.Set(cacheKey, user, AuthCacheDuration);
            return CloneUser(user);
        }

        _logger.LogDebug("SmartEOS Cookie verification returned empty user.");
        return null;
    }

    private static string ExtractJwtToken(HttpRequest request)
    {
        if (request.Query.TryGetValue("JWT", out var queryJwt) && !string.IsNullOrWhiteSpace(queryJwt))
        {
            var token = queryJwt.FirstOrDefault()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(token) && !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return $"Bearer {token}";
            }
            return token;
        }

        if (request.Headers.TryGetValue("Authorization", out var headerAuth) && !string.IsNullOrWhiteSpace(headerAuth))
        {
            return headerAuth.FirstOrDefault()?.Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    private static List<clsCookie> ExtractCookies(HttpRequest request)
    {
        var result = new List<clsCookie>();
        foreach (var cookie in request.Cookies)
        {
            if (string.IsNullOrWhiteSpace(cookie.Value) || cookie.Value.Contains(','))
            {
                continue;
            }

            result.Add(new clsCookie(cookie.Key, cookie.Value));
        }

        return result;
    }

    private static bool IsLocalhost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static CurrentUser CloneUser(CurrentUser src) => new()
    {
        Id = src.Id,
        idNhanSu = src.idNhanSu,
        UserName = src.UserName,
        TenNhanVien = src.TenNhanVien,
        MaNhanVien = src.MaNhanVien,
        Email = src.Email,
        DienThoai = src.DienThoai,
        IsLocked = src.IsLocked,
        ChucVu = src.ChucVu,
        idChucVu = src.idChucVu,
        BoPhan = src.BoPhan,
        Cookies = src.Cookies,
        idBoPhan = src.idBoPhan,
        Authorization = src.Authorization,
        listCookie = src.listCookie?.Select(c => new clsCookie(c.name, c.value)).ToList() ?? new()
    };
}
