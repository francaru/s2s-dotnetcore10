using CommandLine;
using Messaging;
using Messaging.JobLifecyleEvents;
using Messaging.WorkerLifecycleEvents;

namespace WorkloadService;

/// <summary>
/// Entry point for the workload service.
/// </summary>
public class Workload
{
    /// <summary>
    /// Job execution logic (simulation only includes a 10s delay).
    /// </summary>
    /// <param name="mqClient">An instance of a message handler for producing replies.</param>
    /// <param name="eventInfo">Information on the event that triggered this consumer function.</param>
    /// <param name="eventBody">The body of the event in the expected format.</param>
    /// <param name="delay">Optional delay setting.</param>
    public static async Task JobDoWork(IMessageHandler mqClient, MQEventInfo eventInfo, JobDoWorkEventBody? eventBody, Func<Task>? delay = null)
    {
        // Send an event to the listening consumer to record a state change.
        Console.WriteLine($"Starting job: {eventBody!.JobId}");
        mqClient.Produce(
            toQueues: ["OperationalService.BusinessLogic.Service.JobsService::OnStatusChange"],
            new JobStatusChangeEventBody() {
                JobId = eventBody!.JobId, 
                PreviousStatus = "starting", 
                NewStatus = "running" 
            }
        );

        // Wait.
        // Note: This is where the actual execution logic for jobs will be.
        delay ??= () => Task.Delay(10000);
        await delay();

        // Send another event to the listening consumer to record another state change.
        mqClient.Produce(
            toQueues: ["OperationalService.BusinessLogic.Service.JobsService::OnStatusChange"],
            new JobStatusChangeEventBody()
            {
                JobId = eventBody!.JobId,
                PreviousStatus = "running",
                NewStatus = "complete"
            }
        );

        Console.WriteLine($"Completed job: {eventBody!.JobId}");
    }

    /// <summary>
    /// Definition for console arguments that are needed to launch the service.
    /// </summary>
    class Options
    {
        [Option("service-name", HelpText = "The name of the service.", Required = true)]
        public required string ServiceName { get; set; }

        [Option("rabbitmq-host", HelpText = "Host where to connect to RabbitMQ.", Default = "localhost")]
        public required string RabbitMQHost { get; set; }
    }

    /// <summary>
    /// The main method of this application.
    /// </summary>
    /// <param name="args">Console arguments that are passed when launching the application.</param>
    static async Task Main(string[] args)
    {
        // Arguments are parsed into the expected type.
        var options = Parser.Default.ParseArguments<Options>(args).Value;

        // A message handler instance is created using the information from the provided options.
        using var mqClient = MQClient
            .Connect(hostName: options.RabbitMQHost, serviceName: options.ServiceName)
            .Subscribe();

        // Inform the listening consumer that this service is now ready to accept requests.
        mqClient.Produce(
            toQueues: ["OperationalService.BusinessLogic.Service.WorkersService::OnWorkerStart"],
            new WorkerStartEventBody() 
            { 
                WorkerName = options.ServiceName 
            }
        );

        // The application is kept alive indefinitely, until manual exit.
        await Task.Delay(Timeout.Infinite);
    }
}
