using System.Net;
using HRM.Api.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HRM.Api.Middleware;

/// <summary>
/// Middleware xử lý ngoại lệ toàn cục: ghi log chi tiết và trả về chuẩn MessageReturn (HTTP 200) tương thích Frontend cũ.
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request {Method} {Path} was cancelled by client.", context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json; charset=utf-8";

            var detail = _environment.IsDevelopment()
                ? $"Có lỗi xảy ra: {ex.Message}"
                : "Có lỗi xảy ra trong quá trình xử lý!";

            var payload = JsonConvert.SerializeObject(
                MessageReturn.Fail(detail, DefineData.eErrorMessage.LoiCapNhat),
                SerializerSettings);

            await context.Response.WriteAsync(payload);
        }
    }
}
