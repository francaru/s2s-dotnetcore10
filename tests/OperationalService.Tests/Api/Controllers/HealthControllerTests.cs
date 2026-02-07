using Database;
using Microsoft.AspNetCore.Mvc;
using OperationalService.Api.Controllers;

namespace OperationalService.Tests.Api.Controllers;

[TestClass]
public class HealthControllerTests
{
    [TestMethod]
    public void Index_ReturnsHealthWithUptime()
    {
        var dbContext = new DatabaseContext(new());
        var controller = new HealthController(dbContext);

        var result = controller.Index();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);

        var health = ok.Value as OperationalService.Api.Schemas.Health;
        Assert.IsNotNull(health);

        Assert.IsGreaterThanOrEqualTo(0, health.UpTime.TotalMilliseconds);
    }
}
