using Microsoft.AspNetCore.Mvc;

namespace WorldCitiesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetHealth() => "Running";

}