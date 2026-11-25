using HRM.Api.Common.Interfaces;
using HRM.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRM.Api.Filters;

/// <summary>
/// Filter tự động kiểm tra người dùng hiện tại trước mỗi Action.
/// Nếu chưa đăng nhập, trả về HTTP 200 với MessageReturn.Unauthenticated (Error = 18) giống hành vi cũ.
/// </summary>
public sealed class RequireSmartEosUserFilter : IAsyncActionFilter
{
    private readonly ICurrentUserContext _currentUserContext;

    public RequireSmartEosUserFilter(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var metadata = context.ActionDescriptor.EndpointMetadata;
        var allowAnonymous = metadata.OfType<AllowAnonymousSmartEosAttribute>().Any() ||
                             metadata.OfType<IAllowAnonymous>().Any();

        if (!allowAnonymous && !_currentUserContext.IsAuthenticated)
        {
            context.Result = new OkObjectResult(MessageReturn.Unauthenticated());
            return;
        }

        await next();
    }
}
