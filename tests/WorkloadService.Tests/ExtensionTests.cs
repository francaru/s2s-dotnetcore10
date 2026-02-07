using Messaging;
using Moq;

namespace WorkloadService.Tests;

[TestClass]
public class ExtensionsTests
{
    [TestMethod]
    public void Subscribe_CreatesAndConsumesQueues()
    {
        var mqClient = new Mock<IMessageHandler>();

        mqClient.SetupGet(m => m.ServiceName).Returns("WorkloadService");

        mqClient.Object.Subscribe();

        mqClient.Verify(m =>
            m.CreateQueue("WorkloadService.Workload::JobDoWork"),
            Times.Once);
    }
}
