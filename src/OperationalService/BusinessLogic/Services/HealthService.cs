using Database;

namespace OperationalService.BusinessLogic.Services;

/// <summary>
/// A service for the handling of server health business logic.
/// </summary>
/// <param name="dbContext">The currently connected database context.</param>
public class HealthService(DatabaseContext dbContext)
{
    /// <summary>
    /// Get the total uptime of the service.
    /// </summary>
    /// <param name="startTime">The date and time when the service was started.</param>
    /// <returns></returns>
    public TimeSpan GetUpTime(DateTime startTime)
    {
        return DateTime.UtcNow - startTime;
    }
}
