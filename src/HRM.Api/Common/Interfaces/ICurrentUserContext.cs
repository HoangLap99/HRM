using HRM.Api.Common;

namespace HRM.Api.Common.Interfaces;

/// <summary>
/// Ngữ cảnh người dùng hiện tại theo vòng đời Scoped của mỗi HTTP Request.
/// </summary>
public interface ICurrentUserContext
{
    CurrentUser? User { get; }
    string UserId { get; }
    string UserName { get; }
    string FullName { get; }
    string Authorization { get; }
    IReadOnlyList<clsCookie> Cookies { get; }
    bool IsAuthenticated { get; }

    void SetUser(CurrentUser user, string? authorization = null, IEnumerable<clsCookie>? cookies = null);
}
