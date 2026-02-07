using Database;
using Messaging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace OperationalService;

/// <summary>
/// Entry point for the operational service.
/// </summary>
public class App
{
    /// <summary>
    /// The name of the service.
    /// </summary>
    const string serviceName = "OperationalService";

    /// <summary>
    /// The name of the service's logger.
    /// </summary>
    const string loggerName = $"{serviceName}.Logger";

    /// <summary>
    /// The name of the service's tracer.
    /// </summary>
    const string tracerName = $"{serviceName}.Tracer";

    /// <summary>
    /// The date and time when the service was started.
    /// </summary>
    public static readonly DateTime StartTime = DateTime.UtcNow;

    /// <summary>
    /// The name of the service's logger.
    /// </summary>
    public static readonly string LoggerName = loggerName;

    /// <summary>
    /// The name of the service's tracer.
    /// </summary>
    public static readonly string TracerName = tracerName;

    /// <summary>
    /// The main method of this application.
    /// </summary>
    /// <param name="args">Console arguments that are passed when launching the application.</param>
    public static void Main(string[] args)
    {
        // Create a web application builder.
        var builder = WebApplication.CreateBuilder(args);

        // Add a logging provider using OpenTelemetery tools.
        builder
            .Logging
            .ClearProviders()
            .AddConsole()
            .AddDebug()
            .AddOpenTelemetry(options =>
            {
                options
                    .AddConsoleExporter()
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(LoggerName)
                    )
                    .AddProcessor(new ActivityEventLogProcessor());
            });

        // Add a tracing provider using OpenTelemetery tools.
        builder.Services.AddOpenTelemetry()
            .WithTracing(otelBuilder =>
            {
                otelBuilder
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(TracerName)
                    )
                    .AddConsoleExporter()
                    .AddJaegerExporter(conf =>
                    {
                        conf.AgentHost = builder.Configuration["Jaeger:Host"]!;
                    });
            });

        // Add DI services to the application.
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton(new ActivitySource(TracerName));
        builder.Services.AddSingleton(sp =>
            MQClient.Connect(
                hostName: builder.Configuration["RabbitMQ:Host"]!,
                serviceName: serviceName,
                serviceProvider: sp
            ).Subscribe()
        );

        // DI a Database context with a connection string provided through appsettings.json.
        builder.Services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
        });

        // Create the application.
        var app = builder.Build();

        // Run the application.
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
