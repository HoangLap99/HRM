namespace HRM.Api.Filters;

/// <summary>
/// Đánh dấu Controller hoặc Action không yêu cầu xác thực SmartEOS User (ví dụ: Healthcheck, Cronjob nội bộ).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowAnonymousSmartEosAttribute : Attribute
{
}
