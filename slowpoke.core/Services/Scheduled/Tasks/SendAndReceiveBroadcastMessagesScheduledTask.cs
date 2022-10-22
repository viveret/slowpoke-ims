using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Config;
using slowpoke.core.Services.Broadcast;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class SendAndReceiveBroadcastMessagesScheduledTask : IScheduledTask
{
    public Config Config { get; }
    public ISyncStateManager SyncStateManager { get; }
    public IServiceProvider Services { get; }

    public string Title => "Send and receive broadcast messages";

    public bool CanRunConcurrently => false;
    
    public bool CanRunManually => true;

    public SendAndReceiveBroadcastMessagesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services)
    {
        Config = config;
        SyncStateManager = syncStateManager;
        Services = services;
    }

    public IScheduledTaskContext CreateContext(IScheduledTaskManager scheduledTaskManager)
    {
        return new GenericScheduledTaskContext(scheduledTaskManager) { Services = Services, TaskTypeName = this.GetType().FullName, SyncState = SyncStateManager.GetForSystem() };
    }

    public Task Execute(IScheduledTaskContext context)
    {
        context.Log.Add($"Hello world! Known hosts: {Config.P2P.KnownHosts.Length}");

        var broadcasterProviderResolver = Services.GetRequiredService<IBroadcastProviderResolver>();

        var sendCount = 0;
        var exceptions = new List<Exception>();
        try
        {
            broadcasterProviderResolver.MemCached.SendUnsentMessages(msg =>
            {
                context.Log.Add($"Sending #{sendCount}) {msg.EventGuid} ({msg.Type})");
                broadcasterProviderResolver.HttpKnownHosts.Publish(msg, context.CancellationToken);
                sendCount++;
            });
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }
        context.Log.Add($"Sent {sendCount} (errors: {exceptions.Count})");

        try
        {
            var msgs = broadcasterProviderResolver.HttpKnownHosts.Receive(Guid.Empty, context.CancellationToken).ToList();
            context.Log.Add($"Received {msgs.Count}");
            broadcasterProviderResolver.MemCached.AddReceivedMessages(msgs);
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }

        context.Log.Add("Done!");
        return Task.CompletedTask;
    }
}