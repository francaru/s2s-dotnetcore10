using Database;
using Database.Entities;
using Messaging;
using Messaging.JobLifecyleEvents;
using OperationalService.BusinessLogic.Exceptions;
using OperationalService.BusinessLogic.Objects;
using System.Diagnostics;

namespace OperationalService.BusinessLogic.Services;

/// <summary>
/// A service for the handling of jobs business logic.
/// </summary>
/// <param name="dbContext">The currently connected database context.</param>
/// <param name="messageHandler">The currently connected message handler.</param>
public class JobsService(DatabaseContext dbContext, IMessageHandler messageHandler)
{
    public void OnStatusChange(ActivitySource activitySource, ILoggerFactory loggerFactory, MQEventInfo eventInfo, JobStatusChangeEventBody? eventBody)
    {
        /// 1. Start a new tracing activity.
        /// 2. Create a new logger.
        using var _ = activitySource.StartActivity("OnStatusChange");
        var logger = loggerFactory.CreateLogger(eventInfo.QueueName);

        logger.LogInformation($"Received job status update from: `{eventInfo.Sender.ServiceName}`.");

        /// 1. Parse the job ID from the event body.
        /// 2. Retrieve the job from the database if it exists.
        var parsedId = Guid.Parse(eventBody!.JobId);
        var jobInfo = dbContext
            .JobInfos
            .SingleOrDefault(ti => ti.JobId == parsedId);

        if (jobInfo is not null)
        {
            logger.LogInformation($"Loaded job information for job with ID: `{eventBody.JobId}`.");

            // Update the status of the job.
            jobInfo.Status = eventBody.NewStatus;
            dbContext.JobInfos.Update(jobInfo);
            dbContext.SaveChanges();

            logger.LogInformation($"Status changed for job `{eventBody.JobId}`: `{eventBody.PreviousStatus}` -> `{eventBody.NewStatus}`.");
        }
    }

    public int CountJobs()
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("CountJobs");

        // Retrieve the total number of job items in the database.
        var totalItems = dbContext.Jobs.Count();

        return totalItems;
    }

    public IEnumerable<JobServiceObject> CollectJobs(int page, int pageSize)
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("CollectJobs");

        // Retrieve a collection of jobs from the database at an offset.
        var jobs = dbContext
            .Jobs
            .Take(page * pageSize)
            .Skip((page - 1) * pageSize)
            .Select(t => new JobServiceObject() { 
                Id = t.Id, 
                Name = t.Name,
                Status = t.JobInfo!.Status
            })
            .ToList();

        return jobs;
    }

    public JobServiceObject? FetchJob(Guid id)
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("FetchJob");

        // Retrieve the job from the database if it exists.
        var job = dbContext.Jobs.SingleOrDefault(t => t.Id == id);

        return (job is null) ? null : new JobServiceObject()
        {
            Id = job.Id,
            Name = job.Name,
            Status = job.JobInfo?.Status
        };
    }

    public JobServiceObject StartJob(JobServiceObject job)
    {
        // Start a new tracing activity.
        using var _ = Activity.Current!.Source.StartActivity("StartJob");

        // Get the next idle worker.
        var workersService = new WorkersService(dbContext, messageHandler);
        var worker = workersService.GetNextIdleWorker() ?? throw new NoWorkersAvailableException();

        // Create a new job database entity.
        var jobId = Guid.NewGuid();
        var jobEntity = new JobEntity()
        {
            Id = jobId,
            Name = job.Name,
            WorkerId = worker.Id,
            JobInfo = new JobInfoEntity()
            {
                JobId = jobId,
                Status = "starting"
            }
        };

        // Save the job in the database.
        dbContext.Jobs.Add(jobEntity);
        dbContext.SaveChanges();

        // Instruct the connected worker to start work on the created job.
        messageHandler.Produce(
            toQueues: ["WorkloadService.Workload::JobDoWork"],
            new JobDoWorkEventBody() { 
                JobId = jobEntity.Id.ToString() 
            }
        );

        return new JobServiceObject()
        {
            Id = jobEntity.Id,
            Name = jobEntity.Name,
            Status = jobEntity.JobInfo!.Status
        };
    }
}
