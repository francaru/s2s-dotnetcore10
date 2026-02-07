using Database;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace OperationalService.Tests;

public static class TestDbContextFactory
{
    private static DbContextOptions<DatabaseContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;
    }

    public static DatabaseContext CreateEmpty()
    {
        var options = CreateOptions();
        var context = new DatabaseContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static DatabaseContext CreateWithSeed(Action<DatabaseContext> seedAction)
    {
        var context = CreateEmpty();
        seedAction(context);
        context.SaveChanges();
        return context;
    }
}

public static class TestActivityListener
{
    private static ActivityListener? _listener;

    public static void EnsureInitialized()
    {
        if (_listener != null)
            return;

        _listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) =>
                ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(_listener);
    }
}
