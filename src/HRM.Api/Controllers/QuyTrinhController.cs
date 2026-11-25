using HRM.Api.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

[Route("QuyTrinh")]
public sealed class QuyTrinhController(HrmDbContext db, CallWebService callWebService) : ApiControllerBase
{
    [HttpGet("KiemTraButton")]
    public IActionResult KiemTraButton(string? IdTrangThai, string? IdTable)
        => Ok(callWebService.KiemTraButton(db, IdTable, IdTrangThai));
}
