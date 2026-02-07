using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Messaging;

/// <summary>
/// An implemenetation of a message handler, based on RabbitMQ.
/// </summary>
public class MQClient : IMessageHandler
{
    /// <summary>
    /// The singleton instance of the client.
    /// </summary>
    private static MQClient? instance;

    /// <summary>
    /// A connection factory obejct that is used to establish the connection with the RabbitMQ server.
    /// </summary>
    private readonly ConnectionFactory factory;

    /// <summary>
    /// A successfully established connection with the RabbitMQ server.
    /// </summary>
    private readonly IConnection connection;

    /// <summary>
    /// The channel on which queues are created.
    /// </summary>
    private readonly IChannel channel;

    /// <summary>
    /// The service provider creating the client.
    /// </summary>
    private IServiceProvider? ServiceProvider { get; init; }

    /// <summary>
    /// The name of the service that owns the client.
    /// </summary>
    public string ServiceName { get; init; }

    /// <summary>
    /// The host where the connection is established.
    /// </summary>
    public string HostName { get; init; }

    /// <summary>
    /// Create a new client instance.
    /// </summary>
    /// <param name="hostName">The host where the connection is established.</param>
    /// <param name="serviceName">The name of the service that owns the client.</param>
    /// <param name="serviceProvider"><The service provider creating the client./param>
    MQClient(string hostName, string serviceName, IServiceProvider? serviceProvider) 
    {
        /// 1. Create a new instance of a connection factory.
        /// 2. Establish a new connection using the instantiated factory.
        /// 3. Connect to a channel.
        factory = new() { HostName = hostName };
        connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Publicly/Privately set instance level properties.
        HostName = hostName;
        ServiceName = serviceName;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Close all connections opened the client.
    /// </summary>
    public virtual void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
    }

    /// <summary>
    /// Get the singleton instance containing the client.
    /// </summary>
    /// <returns>The singleton instance of the client.</returns>
    /// <exception cref="InvalidOperationException">If an instance has not yet been created via the Connect method.</exception>
    public static MQClient GetInstance()
    {
        if (instance == null)
        {
            throw new InvalidOperationException("No connection has been created. Call Connect() to establish a new connection.");
        }

        return Connect(instance.HostName, instance.ServiceName, instance.ServiceProvider);
    }

    /// <summary>
    /// The entry point to the client for creating a new singleton instance.
    /// </summary>
    /// <param name="hostName">The host where the connection is established.</param>
    /// <param name="serviceName">The name of the service that owns the client.</param>
    /// <param name="serviceProvider"><The service provider creating the client./param>
    /// <returns>The created singleton instance.</returns>
    public static MQClient Connect(string hostName, string serviceName, IServiceProvider? serviceProvider = null)
    {
        // Set a new value to instance only if it is null.
        instance ??= new(hostName: hostName, serviceName: serviceName, serviceProvider: serviceProvider);

        return instance;
    }

    /// <summary>
    /// Create a queue to be registered by consumers.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    public async void CreateQueue(string queueName)
    {
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    /// <summary>
    /// Subscribe to a queue.
    /// </summary>
    /// <typeparam name="T">The type of the object that is expected to be received when an event enters the queue.</typeparam>
    /// <param name="onQueue">The name of the queue on which to subscribe.</param>
    /// <param name="consumerActions">A collection of consumer functions that are triggered when an event enters the queue.</param>
    public async void Consume<T>(string onQueue, params Action<DatabaseContext, ActivitySource, ILoggerFactory, MQEventInfo, T?>[] consumerActions) where T : MQEventBody
    {
        // Create a new consumer.
        var consumer = new AsyncEventingBasicConsumer(channel);

        // Add an async event to the consumer that is lifted when a message is received.
        consumer.ReceivedAsync += (_, eventArgs) =>
        {
            /// 1. The body from the event is obtained in byte[] format.
            /// 2. The byte[] is converted into a string and then into a Json object.
            var body = eventArgs.Body.ToArray();
            var message = JsonNode.Parse(Encoding.UTF8.GetString(body));

            if (message is not null)
            {
                // The Json object in the message is deserialized against the specified type <T>.
                var mqEvent = JsonSerializer.Deserialize<MQEvent<T>>(message);

                if (mqEvent is not null)
                {
                    // A single instance of an event information is created to be shared by all consumer actions.
                    var queueEventInfo = new MQEventInfo() { 
                        QueueName = onQueue, 
                        Sender = mqEvent.Recipient 
                    };

                    /// 1. A thread is opened for each consumer action.
                    /// 2. Each thread creates a new scope for it to be independent.
                    /// 3. Service providers are retrieved from the scope.
                    /// 4. The consumer action is invoked with the retrieved service providers.
                    foreach (var consumerAction in consumerActions)
                    {
                        new Thread(() =>
                        {
                            using var scope = (ServiceProvider is null) ? null : ServiceProvider.CreateScope();

                            var dbContext = (scope is null) ? null : scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            var activitySource = (scope is null) ? null : scope.ServiceProvider.GetRequiredService<ActivitySource>();
                            var loggerFactory = (scope is null) ? null : new LoggerFactory([scope.ServiceProvider.GetRequiredService<ILoggerProvider>()]);

                            consumerAction(dbContext!, activitySource!, loggerFactory!, queueEventInfo, mqEvent.Body);
                        }).Start();
                    }
                }
            }

            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(onQueue, autoAck: true, consumer: consumer);
    }

    /// <summary>
    /// Send out an event to consumers that are subscribed to the specified queues.
    /// </summary>
    /// <typeparam name="T">The type of the object being transmitted from the producer.</typeparam>
    /// <param name="toQueues">A collection of queue names that are eligible to receive the event.</param>
    /// <param name="mqEventBody">The object to transmit from the producer.</param>
    public async void Produce<T>(string[] toQueues, T mqEventBody) where T : MQEventBody
    {
        // A recipient is created, containing information on the service that is producing the message.
        var mqRecipient = new MQEventRecipient() {
            ServiceName = ServiceName
        };

        // An event is created using the recipient, and the provided event body.
        var mqEvent = new MQEvent<T>() { 
            Body = mqEventBody, 
            Recipient = mqRecipient 
        };

        /// 1. The event object is JSON serialized into a string.
        /// 2. The string is converted to a byte[].
        var message = JsonSerializer.Serialize(mqEvent);
        var body = Encoding.UTF8.GetBytes(message);

        // The message is sent to all specified queues.
        foreach (var toQueue in toQueues)
        {
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: toQueue, body: body);
        }
    }
}
