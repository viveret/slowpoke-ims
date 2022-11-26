using slowpoke.core.Models.Configuration;
using slowpoke.core.Services;
using slowpoke.core.Services.Scheduled;

namespace SlowPokeIMS.Core.Services.Scheduled;


public abstract class ScheduledTaskBase : IScheduledTask
{
    public Config Config { get; }
    public ISyncStateManager SyncStateManager { get; }
    public IServiceProvider Services { get; }

    protected ScheduledTaskBase(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services)
    {
        Config = config;
        SyncStateManager = syncStateManager;
        Services = services;
    }

    public abstract string Title { get; }
    public abstract bool CanRunConcurrently { get; }
    public abstract bool CanRunManually { get; }

    public virtual IScheduledTaskContext CreateContext(IScheduledTaskManager scheduledTaskManager)
    {
        var ctx = new GenericScheduledTaskContext(scheduledTaskManager)
        {
            Services = Services,
            TaskTypeName = this.GetType().FullName!,
            Id = Guid.NewGuid(),
        };
        ctx.SyncState = SyncStateManager.GetForAction(ctx.Id);
        return ctx;
    }

    public abstract Task Execute(IScheduledTaskContext context);

    public virtual Task OnEnd(IScheduledTaskContext t)
    {
        t.WhenCompleted = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public virtual Task OnStart(IScheduledTaskContext t)
    {
        t.WhenStarted = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}