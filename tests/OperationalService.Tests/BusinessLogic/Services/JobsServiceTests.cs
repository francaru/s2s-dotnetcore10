using Database.Entities;
using Messaging;
using Messaging.LifecycleEvents;
using Microsoft.Extensions.Logging;
using Moq;
using OperationalService.BusinessLogic.Services;
using System.Diagnostics;

namespace OperationalService.Tests.BusinessLogic.Services;

[TestClass]
public class JobsServiceTests
{
    private static ActivitySource? _source;
    private static Activity? _root;

    [ClassInitialize]
    public static void Init(TestContext _)
    {
        TestActivityListener.EnsureInitialized();

        _source = new ActivitySource("Test");
        _root = _source.StartActivity("Root");
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        _root?.Dispose();
        _source?.Dispose();
    }

    [TestMethod]
    public void CountJobs_ReturnsCorrectCount()
    {
        var dbContext = TestDbContextFactory.CreateWithSeed(context =>
        {
            for (int i = 0; i < 5; i++)
            {
                var jobId = Guid.NewGuid();

                context.Jobs.Add(new JobEntity
                {
                    Id = jobId,
                    Name = jobId.ToString(),
                    JobInfo = new JobInfoEntity
                    {
                        JobId = jobId,
                        Status = new Random().GetItems<string>(["starting", "running", "complete"], 1)[0]
                    }
                });
            }
        });

        var service = new JobsService(dbContext, new Mock<IMessageHandler>().Object);
        var count = service.CountJobs();

        Assert.AreEqual(5, count);
    }

    [TestMethod]
    public void CollectJobs_ReturnsPagedResults()
    {
        var dbContext = TestDbContextFactory.CreateWithSeed(context =>
        {
            for (int i = 0; i < 20; i++)
            {
                var jobId = Guid.NewGuid();

                context.Jobs.Add(new JobEntity
                {
                    Id = jobId,
                    Name = jobId.ToString(),
                    JobInfo = new JobInfoEntity
                    {
                        JobId = jobId,
                        Status = new Random().GetItems<string>(["starting", "running", "complete"], 1)[0]
                    }
                });
            }
        });

        var service = new JobsService(dbContext, new Mock<IMessageHandler>().Object);
        var results = service.CollectJobs(page: 2, pageSize: 5).ToList();

        Assert.HasCount(5, results);
    }

    [TestMethod]
    public void FetchJob_NotFound_ReturnsNull()
    {
        var dbContext = TestDbContextFactory.CreateEmpty();
        var service = new JobsService(dbContext, new Mock<IMessageHandler>().Object);

        var result = service.FetchJob(Guid.NewGuid());

        Assert.IsNull(result);
    }

    [TestMethod]
    public void FetchJob_Found_ReturnsJob()
    {
        var dbContext = TestDbContextFactory.CreateWithSeed(context =>
        {
            var jobId = Guid.NewGuid();

            context.Jobs.Add(new JobEntity
            {
                Id = jobId,
                Name = jobId.ToString(),
                JobInfo = new JobInfoEntity
                {
                    JobId = jobId,
                    Status = new Random().GetItems<string>(["starting", "running", "complete"], 1)[0]
                }
            });
        });

        var job = dbContext.Jobs.First();
        var service = new JobsService(dbContext, new Mock<IMessageHandler>().Object);
        var result = service.FetchJob(job.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(job.Name, result.Name);
    }

    [TestMethod]
    public void OnStatusChange_UpdatesJobStatus()
    {
        var dbContext = TestDbContextFactory.CreateWithSeed(context =>
        {
            var jobId = Guid.NewGuid();

            context.Jobs.Add(new JobEntity
            {
                Id = jobId,
                Name = jobId.ToString(),
                JobInfo = new JobInfoEntity
                {
                    JobId = jobId,
                    Status = "starting"
                }
            });
        });

        var job = dbContext.JobInfos.First();
        var service = new JobsService(dbContext, new Mock<IMessageHandler>().Object);
        var eventInfo = new MQEventInfo
        {
            QueueName = "queue",
            Sender = new MQEventRecipient { ServiceName = "Test" }
        };

        var body = new JobStatusChangeEventBody
        {
            JobId = job.JobId.ToString(),
            PreviousStatus = "starting",
            NewStatus = "complete"
        };

        service.OnStatusChange(
            Activity.Current!.Source,
            new LoggerFactory(),
            eventInfo,
            body
        );

        Assert.AreEqual("complete", job.Status);
    }
}
