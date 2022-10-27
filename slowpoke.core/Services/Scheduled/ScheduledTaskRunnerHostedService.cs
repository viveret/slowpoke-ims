using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace slowpoke.core.Services.Scheduled;


public class ScheduledTaskRunnerHostedService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<ScheduledTaskRunnerHostedService> _logger;
    private readonly IScheduledTaskManager _scheduledTaskManager;
    private Timer? _timer = null;

    public ScheduledTaskRunnerHostedService(
        ILogger<ScheduledTaskRunnerHostedService> logger,
        IScheduledTaskManager scheduledTaskManager)
    {
        _logger = logger;
        _scheduledTaskManager = scheduledTaskManager;
    }

    private void ExecuteTasks(object? state)
    {
        var count = Interlocked.Increment(ref executionCount);

        _logger.LogInformation(
            "Timed Hosted Service is working. Count: {Count}", count);

        _scheduledTaskManager.ExecuteNextQueuedTask();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(ExecuteTasks, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
}