using Database.Entities;
using Messaging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OperationalService.Api.Controllers;
using OperationalService.Api.Schemas;
using System.Diagnostics;

namespace OperationalService.Tests.Api.Controllers;

[TestClass]
public class JobsControllerTests
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
    public void GetAll_ReturnsJobList()
    {
        var dbContext = TestDbContextFactory.CreateWithSeed(context =>
        {
            for (int i = 0; i < 3; i++)
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

        var controller = new JobsController(dbContext, new Mock<IMessageHandler>().Object);
        var result = controller.GetAll();
        var ok = result as OkObjectResult;

        Assert.IsNotNull(ok);

        var response = ok.Value as JobList;
        Assert.IsNotNull(response);

        Assert.AreEqual(3, response.Data.Count());
        Assert.AreEqual(1, response.Pagination.PageNumber);
        Assert.AreEqual(10, response.Pagination.PageSize);
        Assert.AreEqual(3, response.Pagination.TotalCount);
    }

    [TestMethod]
    public void Get_InvalidId_ReturnsNotFound()
    {
        var dbContext = TestDbContextFactory.CreateEmpty();
        var controller = new JobsController(dbContext, new Mock<IMessageHandler>().Object);
        var result = controller.Get("not-a-guid");

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public void Get_JobDoesNotExist_ReturnsNotFound()
    {
        var dbContext = TestDbContextFactory.CreateEmpty();
        var controller = new JobsController(dbContext, new Mock<IMessageHandler>().Object);
        var result = controller.Get(Guid.NewGuid().ToString());

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public void Get_JobExists_ReturnsJob()
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

        var controller = new JobsController(dbContext, new Mock<IMessageHandler>().Object);

        var result = controller.Get(job.Id.ToString());

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);

        var dto = ok.Value as Job;
        Assert.IsNotNull(dto);
        Assert.AreEqual(job.Name, dto.Name);
    }

    [TestMethod]
    public void Create_ReturnsCreatedJob()
    {
        var dbContext = TestDbContextFactory.CreateEmpty();
        var controller = new JobsController(dbContext, new Mock<IMessageHandler>().Object);
        var result = controller.Create(new JobCreate { Name = "Test Job" });

        var created = result as CreatedResult;
        Assert.IsNotNull(created);

        var job = created.Value as Job;
        Assert.IsNotNull(job);
        Assert.AreEqual("Test Job", job.Name);
    }
}

