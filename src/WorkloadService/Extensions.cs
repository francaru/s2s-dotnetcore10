using Database;
using Messaging;
using Messaging.JobLifecyleEvents;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WorkloadService;

/// <summary>
/// A static class for extending dependency functionality.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Subscribe to a list of queues that pertain to the service.
    /// </summary>
    /// <param name="mqClient">The message handler instance being extended.</param>
    /// <returns>The extended message handler instance.</returns>
    public static IMessageHandler Subscribe(this IMessageHandler mqClient)
    {
        /// 1. Create a queue for the specified queue name.
        /// 2. Create a consumer that listens for events on the specified queue name.
        var doWorkQueue = $"{mqClient.ServiceName}.Workload::JobDoWork";
        mqClient.CreateQueue(queueName: doWorkQueue);
        mqClient.Consume(
            onQueue: doWorkQueue,
            async (DatabaseContext _, ActivitySource _, ILoggerFactory logger, MQEventInfo eventInfo, JobDoWorkEventBody? eventBody) => {
                await Workload.JobDoWork(mqClient, eventInfo, eventBody);
            }
        );

        return mqClient;
    }
}
