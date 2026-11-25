using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.ExternalServices.SmartEos;

public sealed class SmartEosAuthClient : SmartEosHttpClientBase, ISmartEosAuthClient
{
    private const string GetCurrentUserEndpoint = "SmartEOSAPI/QuanTri/GetCurrentUserId";
    private const string GetJwtTokenEndpoint = "SmartEOSAPI/QuanTri/GetJwtTokenForAPI?UserName=admin&App=AuTo";

    public SmartEosAuthClient(
        HttpClient httpClient,
        ICurrentUserContext currentUserContext,
        IOptions<HrmOptions> options,
        ILogger<SmartEosAuthClient> logger)
        : base(httpClient, currentUserContext, options, logger)
    {
    }

    public async Task<CurrentUser?> GetCurrentUserByJwtAsync(string jwtToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            return null;
        }

        var normalizedToken = jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? jwtToken
            : $"Bearer {jwtToken}";

        var user = await GetJsonAsync<CurrentUser>(
            GetCurrentUserEndpoint,
            overrideAuthorization: normalizedToken,
            overrideCookies: Array.Empty<clsCookie>(),
            includeAuth: true,
            ct: ct);

        if (user is null || string.IsNullOrWhiteSpace(user.Id))
        {
            return null;
        }

        user.Authorization = normalizedToken;
        return user;
    }

    public async Task<CurrentUser?> GetCurrentUserByCookiesAsync(IReadOnlyList<clsCookie> cookies, CancellationToken ct = default)
    {
        if (cookies is null || cookies.Count == 0)
        {
            return null;
        }

        var user = await GetJsonAsync<CurrentUser>(
            GetCurrentUserEndpoint,
            overrideAuthorization: string.Empty,
            overrideCookies: cookies,
            includeAuth: true,
            ct: ct);

        if (user is null || string.IsNullOrWhiteSpace(user.Id))
        {
            return null;
        }

        user.listCookie = cookies.ToList();
        return user;
    }

    public async Task<string?> GetSystemJwtTokenAsync(CancellationToken ct = default)
    {
        return await GetJsonAsync<string>(
            GetJwtTokenEndpoint,
            includeAuth: false,
            ct: ct);
    }
}
