using Database;
using OperationalService.BusinessLogic.Services;

namespace OperationalService.Tests.BusinessLogic.Services;

[TestClass]
public class HealthServiceTests
{
    [TestMethod]
    public void GetUpTime_ReturnsPositiveTimeSpan()
    {
        var dbContext = new DatabaseContext(new());
        var service = new HealthService(dbContext);

        var startTime = DateTime.UtcNow.AddSeconds(-5);

        var uptime = service.GetUpTime(startTime);

        Assert.IsGreaterThanOrEqualTo(5, uptime.TotalSeconds);
    }
}