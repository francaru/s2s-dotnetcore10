using Database;
using Messaging;
using Messaging.JobLifecyleEvents;
using Messaging.WorkerLifecycleEvents;
using OperationalService.BusinessLogic.Services;
using System.Diagnostics;

namespace OperationalService;

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
        var onStatusChangeQueue = $"{mqClient.ServiceName}.BusinessLogic.Service.JobsService::OnStatusChange";
        mqClient.CreateQueue(queueName: onStatusChangeQueue);
        mqClient.Consume(
            onQueue: onStatusChangeQueue,
            (DatabaseContext dbContext, ActivitySource activitySource, ILoggerFactory loggerFactory, MQEventInfo eventInfo, JobStatusChangeEventBody? eventBody) => {
                new JobsService(dbContext, mqClient).OnStatusChange(activitySource, loggerFactory, eventInfo, eventBody);
            }
        );

        /// 1. Create a queue for the specified queue name.
        /// 2. Create a consumer that listens for events on the specified queue name.
        var onWorkerStartQueue = $"{mqClient.ServiceName}.BusinessLogic.Service.WorkersService::OnWorkerStart";
        mqClient.CreateQueue(queueName: onWorkerStartQueue);
        mqClient.Consume(
            onQueue: onWorkerStartQueue,
            (DatabaseContext dbContext, ActivitySource activitySource, ILoggerFactory loggerFactory, MQEventInfo eventInfo, WorkerStartEventBody? eventBody) => {
                new WorkersService(dbContext, mqClient).OnWorkerStart(activitySource, loggerFactory, eventInfo, eventBody);
            }
        );

        return mqClient;
    }
}
