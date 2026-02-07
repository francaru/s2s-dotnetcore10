using Microsoft.EntityFrameworkCore;

namespace Database;

/// <summary>
/// Entry point for triggering Database migrations using Package Manager Console.
/// </summary>
public class Configure
{
    /// <summary>
    /// The main method of this application.
    /// </summary>
    /// <param name="args">Console arguments that are passed when launching the application.</param>
    public static void Main(string[] args)
    {
        // Create a web application builder.
        var builder = WebApplication.CreateBuilder(args);

        // DI a Database context with a connection string provided through appsettings.json.
        builder.Services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnectionString"));
        });

        // Create the application.
        var app = builder.Build();
        
        // Run the application.
        app.Run();
    }
}