using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Exposes a liveness endpoint for orchestrators such as Kubernetes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
