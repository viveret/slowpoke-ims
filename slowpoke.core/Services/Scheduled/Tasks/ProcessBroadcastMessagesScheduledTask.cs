using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Config;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ProcessBroadcastMessagesScheduledTask : IScheduledTask
{
    private static Guid lastEventGuidProcessed = Guid.Empty;

    public Config Config { get; }
    public ISyncStateManager SyncStateManager { get; }
    public IServiceProvider Services { get; }
    public IBroadcastMessageHandlerResolver BroadcastMessageHandlerResolver { get; }
    public IBroadcastProviderResolver BroadcasterProviderResolver { get; }

    public string Title => "Process received broadcast messages";

    public bool CanRunConcurrently => false;
    
    public bool CanRunManually => true;

    public ProcessBroadcastMessagesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        IBroadcastMessageHandlerResolver broadcastMessageHandlerResolver,
        IBroadcastProviderResolver broadcasterProviderResolver)
    {
        Config = config;
        SyncStateManager = syncStateManager;
        Services = services;
        BroadcastMessageHandlerResolver = broadcastMessageHandlerResolver;
        BroadcasterProviderResolver = broadcasterProviderResolver;
    }

    public IScheduledTaskContext CreateContext(IScheduledTaskManager scheduledTaskManager)
    {
        var ctx = new GenericScheduledTaskContext(scheduledTaskManager) { Services = Services, TaskTypeName = this.GetType().FullName };
        ctx.SyncState = SyncStateManager.GetForAction(ctx.Id);
        return ctx;
    }

    public Task Execute(IScheduledTaskContext context)
    {
        var allowSameOrigin = true;
        var events = BroadcasterProviderResolver.MemCached.Receive(lastEventGuidProcessed, context.CancellationToken);
        context.OutputLog.Add($"Processing {events.Count()} received events");

        var processedCount = 0;
        var sameOrigin = 0;
        foreach (var ev in events)
        {
            if (ev.OriginGuid == Guid.Empty || ev.OriginGuid == BroadcasterProviderResolver.HttpKnownHosts.OriginGuid)
            {
                sameOrigin++;
                if (!allowSameOrigin)
                    continue; // skip since same origin
            }

            // todo: should have written log of processed event guids so that events can't be
            // double processed in the event that the process is restarted and the cached
            // message data is lost, forcing complete reprocessing. should save to main config
            // or file.
            context.OutputLog.Add($"Processing {ev.EventGuid} ({ev.Type})");

            var msg = ev.ConvertToTrueType();
            BroadcastMessageHandlerResolver.Handle(msg, context.CancellationToken);
            lastEventGuidProcessed = ev.EventGuid;
            // ^^^ should save this in case it is lost, which it definitely will be
        }
        context.OutputLog.Add($"Processed {processedCount} with {sameOrigin} from self (same origin)");
        
        context.OutputLog.Add("Done!");
        return Task.CompletedTask;
    }
}