using HRM.Api.Common;

namespace HRM.Api.Common.Interfaces.SmartEos;

/// <summary>
/// Giao tiếp xác thực và lấy thông tin người dùng hiện tại từ SmartEOS.
/// </summary>
public interface ISmartEosAuthClient
{
    Task<CurrentUser?> GetCurrentUserByJwtAsync(string jwtToken, CancellationToken ct = default);
    Task<CurrentUser?> GetCurrentUserByCookiesAsync(IReadOnlyList<clsCookie> cookies, CancellationToken ct = default);
    Task<string?> GetSystemJwtTokenAsync(CancellationToken ct = default);
}
