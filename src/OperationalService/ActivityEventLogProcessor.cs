using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Diagnostics;

namespace OperationalService;

/// <summary>
/// An implementation of BaseProcessor for adding logs to a connected activity service.
/// </summary>
public class ActivityEventLogProcessor : BaseProcessor<LogRecord>
{
    /// <summary>
    /// Add logs to the current activity.
    /// </summary>
    /// <param name="data">The log record to add.</param>
    public override void OnEnd(LogRecord data)
    {
        // Keep original logging functionality.
        base.OnEnd(data);

        // Add the log as a string to the connected activity service (e.g., OpenTelemetery).
        var currentActivity = Activity.Current;
        if (currentActivity is not null)
        {
            currentActivity.AddEvent(new ActivityEvent(data!.Attributes!.ToString()!));
        }
    }
}
