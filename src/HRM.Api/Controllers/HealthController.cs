using System.Runtime.InteropServices;
using HRM.Api.Filters;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HRM.Api.Controllers;

[Route("api/[controller]")]
public sealed class HealthController : ApiControllerBase
{
    private readonly IHrmDbContext _db;
    private readonly HrmOptions _options;
    private readonly IHostEnvironment _environment;

    public HealthController(
        IHrmDbContext db,
        IOptions<HrmOptions> options,
        IHostEnvironment environment)
    {
        _db = db;
        _options = options.Value;
        _environment = environment;
    }

    [HttpGet]
    [AllowAnonymousSmartEos]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        bool canConnectDatabase;
        string? databaseError = null;

        try
        {
            canConnectDatabase = await _db.Database.CanConnectAsync(ct);
        }
        catch (Exception ex)
        {
            canConnectDatabase = false;
            databaseError = ex.Message;
        }

        var payload = new
        {
            Status = canConnectDatabase ? "Healthy" : "Degraded",
            Environment = _environment.EnvironmentName,
            Framework = RuntimeInformation.FrameworkDescription,
            ServerTime = DateTime.Now,
            Host = _options.Host,
            SmartEosUrl = _options.URLSmartEOS,
            EnableLocalhostDevFallback = _options.EnableLocalhostDevFallback,
            DatabaseConnected = canConnectDatabase,
            DatabaseError = databaseError
        };

        return Ok(MessageReturn.Success("Hệ thống hoạt động bình thường", payload));
    }
}
