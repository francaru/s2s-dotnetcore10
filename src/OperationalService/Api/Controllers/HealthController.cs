using Microsoft.AspNetCore.Mvc;
using OperationalService.Api.Schemas;
using OperationalService.BusinessLogic.Services;
using Database;

namespace OperationalService.Api.Controllers;

/// <summary>
/// Controller for managing server health.
/// </summary>
/// <param name="dbContext">The currently connected database context.</param>
[Route("[controller]")]
[ApiController]
public class HealthController(DatabaseContext dbContext) : ControllerBase
{
    /// <summary>
    /// Get the uptime of the server.
    /// </summary>
    /// <returns>The uptime of the server.</returns>
    [HttpGet]
    [Produces<Health>]
    public IActionResult Index()
    {
        /// 1. Create a new instance of health service.
        /// 2. Retrieve the uptime.
        var healthService = new HealthService(dbContext);
        var uptime = healthService.GetUpTime(App.StartTime);

        // Build a new health object with the uptime value.
        return Ok(
            new Health()
            {
                UpTime = uptime
            }
        );
    }
}
