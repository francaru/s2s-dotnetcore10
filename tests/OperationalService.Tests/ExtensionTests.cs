using Database;
using Messaging;
using Messaging.LifecycleEvents;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace OperationalService.Tests;

[TestClass]
public class ExtensionsTests
{
    [TestMethod]
    public void Subscribe_CreatesAndConsumesQueues()
    {
        var mqClient = new Mock<IMessageHandler>();

        mqClient.SetupGet(m => m.ServiceName).Returns("OperationalService");

        mqClient.Object.Subscribe();

        mqClient.Verify(m =>
            m.CreateQueue("OperationalService.BusinessLogic.Service.JobsService::OnStatusChange"),
            Times.Once);

        mqClient.Verify(m =>
            m.Consume(
                It.IsAny<string>(),
                It.IsAny<Action<DatabaseContext, ActivitySource, ILoggerFactory, MQEventInfo, JobStatusChangeEventBody?>>()),
            Times.Once);
    }
}