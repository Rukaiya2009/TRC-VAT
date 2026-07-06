using Microsoft.AspNetCore.Mvc;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(ApiResponse<object>.Ok(new { status = "ok", utc = DateTime.UtcNow }));
}
