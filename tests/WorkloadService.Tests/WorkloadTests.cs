using Messaging;
using Messaging.LifecycleEvents;
using Messaging.WorkEvents;
using Moq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WorkloadService.Tests;

[TestClass]
public sealed class WorkloadTests
{
    [TestMethod]
    public async Task JobDoWork_ProducesRunningAndCompleteEvents()
    {
        var jobId = Guid.NewGuid().ToString();
        var publisherMock = new Mock<IMessageHandler>();
        var eventBody = new JobDoWorkEventBody
        {
            JobId = jobId
        };

        await Workload.JobDoWork(
            publisherMock.Object,
            eventInfo: null!,
            eventBody,
            delay: () => Task.CompletedTask
        );

        publisherMock.Verify(m =>
            m.Produce(
                It.IsAny<string[]>(),
                It.Is<JobStatusChangeEventBody>(b =>
                    b.JobId == jobId &&
                    b.PreviousStatus == "starting" &&
                    b.NewStatus == "running"
                )),
            Times.Once);

        publisherMock.Verify(m =>
            m.Produce(
                It.IsAny<string[]>(),
                It.Is<JobStatusChangeEventBody>(b =>
                    b.JobId == jobId &&
                    b.PreviousStatus == "running" &&
                    b.NewStatus == "complete"
                )),
            Times.Once);
    }

    [TestMethod]
    public void JobStatusChangeEventBody_RoundtripSerialization()
    {
        var payload = new JobStatusChangeEventBody
        {
            JobId = Guid.NewGuid().ToString(),
            PreviousStatus = "running",
            NewStatus = "complete"
        };

        var mqEvent = new MQEvent<JobStatusChangeEventBody>
        {
            Body = payload,
            Recipient = new MQEventRecipient
            {
                ServiceName = "WorkloadService"
            }
        };

        var json = JsonSerializer.Serialize(mqEvent);
        var deserialized = JsonSerializer.Deserialize<MQEvent<JobStatusChangeEventBody>>(json);

        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.Body);

        Assert.AreEqual(payload.JobId, deserialized.Body.JobId);
        Assert.AreEqual(payload.PreviousStatus, deserialized.Body.PreviousStatus);
        Assert.AreEqual(payload.NewStatus, deserialized.Body.NewStatus);
    }

    [TestMethod]
    public void JobStatusChangeEventBody_ContainsExpectedProperties()
    {
        var payload = new JobStatusChangeEventBody
        {
            JobId = "test",
            PreviousStatus = "starting",
            NewStatus = "running"
        };

        var json = JsonSerializer.Serialize(payload);
        var node = JsonNode.Parse(json)!;

        Assert.IsNotNull(node["JobId"]);
        Assert.IsNotNull(node["PreviousStatus"]);
        Assert.IsNotNull(node["NewStatus"]);
    }

}
