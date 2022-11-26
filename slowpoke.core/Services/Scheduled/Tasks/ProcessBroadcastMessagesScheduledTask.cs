using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Broadcast;
using SlowPokeIMS.Core.Services.Scheduled;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ProcessBroadcastMessagesScheduledTask : ScheduledTaskBase
{
    private static Guid lastEventGuidProcessed = Guid.Empty;

    public IBroadcastMessageHandlerResolver BroadcastMessageHandlerResolver { get; }
    public IBroadcastProviderResolver BroadcasterProviderResolver { get; }

    public override string Title => "Process received broadcast messages";

    public override bool CanRunConcurrently => false;
    
    public override bool CanRunManually => true;

    public ProcessBroadcastMessagesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        IBroadcastMessageHandlerResolver broadcastMessageHandlerResolver,
        IBroadcastProviderResolver broadcasterProviderResolver): base(config, syncStateManager, services)
    {
        BroadcastMessageHandlerResolver = broadcastMessageHandlerResolver;
        BroadcasterProviderResolver = broadcasterProviderResolver;
    }

    public override async Task Execute(IScheduledTaskContext context)
    {
        var allowSameOrigin = true;
        var events = await BroadcasterProviderResolver.MemCached.Receive(lastEventGuidProcessed, context.CancellationToken);
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
    }
}