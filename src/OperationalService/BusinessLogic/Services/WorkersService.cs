using Database;
using Database.Entities;
using Messaging;
using Messaging.WorkerLifecycleEvents;
using Microsoft.EntityFrameworkCore;
using OperationalService.BusinessLogic.Objects;
using System.Diagnostics;

namespace OperationalService.BusinessLogic.Services;

/// <summary>
/// A service for the handling of workers business logic.
/// </summary>
/// <param name="dbContext">The currently connected database context.</param>
/// <param name="messageHandler">The currently connected message handler.</param>
public class WorkersService(DatabaseContext dbContext, IMessageHandler messageHandler)
{
    public void OnWorkerStart(ActivitySource activitySource, ILoggerFactory loggerFactory, MQEventInfo eventInfo, WorkerStartEventBody? eventBody)
    {
        /// 1. Start a new tracing activity.
        /// 2. Create a new logger.
        using var _ = activitySource.StartActivity("OnWorkerStart");
        var logger = loggerFactory.CreateLogger(eventInfo.QueueName);

        // Retrieve the worker from the database.
        var worker = dbContext
            .Workers
            .Include(w => w.WorkerInfo)
            .SingleOrDefault(ti => ti.Name == eventBody!.WorkerName);

        if (worker is not null)
        {
            // Mark the worker as idle (ready for work).
            worker.WorkerInfo!.Status = "idle";
            dbContext.WorkerInfos.Update(worker.WorkerInfo);
            dbContext.SaveChanges();

            logger.LogInformation($"Worker `({worker.Name}) {worker.Id}` is now accepting requests.");
        }
    }

    public int CountWorkers()
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("CountWorkers");

        // Retrieve the total number of worker items in the database.
        var totalItems = dbContext.Workers.Count();

        return totalItems;
    }

    public IEnumerable<WorkerServiceObject> CollectWorkers(int page, int pageSize)
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("CollectWorkers");

        // Retrieve a collection of workers from the database at an offset.
        var workers = dbContext
            .Workers
            .Take(page * pageSize)
            .Skip((page - 1) * pageSize)
            .Select(t => new WorkerServiceObject()
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.WorkerInfo!.Status
            })
            .ToList();

        return workers;
    }

    public WorkerServiceObject? FetchWorker(Guid id)
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("FetchWorker");

        // Retrieve the worker from the database if it exists.
        var worker = dbContext.Workers.SingleOrDefault(t => t.Id == id);

        return (worker is null) ? null : new WorkerServiceObject()
        {
            Id = worker.Id,
            Name = worker.Name,
            Status = worker.WorkerInfo?.Status
        };
    }

    public WorkerServiceObject? GetNextIdleWorker()
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("GetNextIdleWorker");

        // Retrieve the worker from the database if it exists.
        var worker = dbContext
            .Workers
            .Include(w => w.WorkerInfo)
            .SingleOrDefault(t => t.WorkerInfo!.Status == "idle");

        // Update the worker status to staged.
        if (worker is not null)
        {
            worker.WorkerInfo!.Status = "staged";

            dbContext
                .WorkerInfos
                .Update(worker.WorkerInfo);

            dbContext.SaveChanges();

            return new WorkerServiceObject()
            {
                Id = worker.Id,
                Name = worker.Name,
                Status = worker.WorkerInfo?.Status
            };
        }

        return null;
    }

    public WorkerServiceObject SpawnWorker(WorkerServiceObject worker)
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("SpawnWorker");

        // Create a new worker database entity.
        var workerId = Guid.NewGuid();
        var workerEntity = new WorkerEntity()
        {
            Id = workerId,
            Name = worker.Name,
            WorkerInfo = new WorkerInfoEntity()
            {
                WorkerId = workerId,
                Status = "starting"
            }
        };

        // Save the worker in the database.
        dbContext.Workers.Add(workerEntity);
        dbContext.SaveChanges();

        // TODO: Start worker via Docker SDK here.

        return new WorkerServiceObject()
        {
            Id = workerEntity.Id,
            Name = workerEntity.Name,
            Status = workerEntity.WorkerInfo!.Status
        };
    }
}
