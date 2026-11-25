using HRM.Api.Common.Interfaces;
using HRM.Api.Common;

namespace HRM.Api.Identity;

/// <summary>
/// Lưu trữ ngữ cảnh người dùng hiện tại theo từng HTTP Request (Scoped).
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    private CurrentUser? _user;
    private string _authorization = string.Empty;
    private IReadOnlyList<clsCookie> _cookies = Array.Empty<clsCookie>();

    public CurrentUser? User => _user;
    public string UserId => _user?.Id ?? string.Empty;
    public string UserName => _user?.UserName ?? string.Empty;
    public string FullName => _user?.TenNhanVien ?? _user?.UserName ?? string.Empty;
    public string Authorization => _authorization;
    public IReadOnlyList<clsCookie> Cookies => _cookies;
    public bool IsAuthenticated => _user is not null && !string.IsNullOrWhiteSpace(_user.Id);

    public void SetUser(CurrentUser user, string? authorization = null, IEnumerable<clsCookie>? cookies = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        _user = user;
        _authorization = authorization ?? user.Authorization ?? string.Empty;
        _cookies = cookies?.ToList() ?? user.listCookie ?? new List<clsCookie>();

        _user.Authorization = _authorization;
        if (_cookies.Count > 0 && (_user.listCookie is null || _user.listCookie.Count == 0))
        {
            _user.listCookie = _cookies.ToList();
        }
    }
}
