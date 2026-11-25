using HRM.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

[Route("Home")]
[AllowAnonymousSmartEos]
public sealed class HomeController : ApiControllerBase
{
    [HttpGet]
    [Route("")]
    [Route("~/")]
    [Route("Index")]
    [Route("~/api/Home")]
    public IActionResult Index()
    {
        return Ok(MessageReturn.Success("HRM .NET 8 API Service", new { Title = "Home Page" }));
    }
}
