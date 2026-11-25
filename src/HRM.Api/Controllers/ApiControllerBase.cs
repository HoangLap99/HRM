using HRM.Api.Common.Interfaces;
using HRM.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

/// <summary>
/// Controller cơ sở cho toàn bộ 28 Controllers HRM khi migrate sang .NET 8,
/// cung cấp ICurrentUserContext và helper getCurrentUser() tương thích ngược.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected ICurrentUserContext CurrentUserContext =>
        HttpContext.RequestServices.GetRequiredService<ICurrentUserContext>();

    protected CurrentUser currentUser => CurrentUserContext.User ?? new CurrentUser();

    protected string CurrentUserId => CurrentUserContext.UserId;

    protected CurrentUser getCurrentUser(string functionName = "")
    {
        return CurrentUserContext.User ?? new CurrentUser();
    }
}
