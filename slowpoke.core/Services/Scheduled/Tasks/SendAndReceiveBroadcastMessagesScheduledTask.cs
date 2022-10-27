using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Config;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class SendAndReceiveBroadcastMessagesScheduledTask : IScheduledTask
{
    public Config Config { get; }
    public ISyncStateManager SyncStateManager { get; }
    public IServiceProvider Services { get; }

    private ISlowPokeHostProvider slowPokeHostProvider;

    public string Title => "Send and receive broadcast messages";

    public bool CanRunConcurrently => false;
    
    public bool CanRunManually => true;

    public SendAndReceiveBroadcastMessagesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        ISlowPokeHostProvider slowPokeHostProvider)
    {
        Config = config;
        SyncStateManager = syncStateManager;
        Services = services;
        this.slowPokeHostProvider = slowPokeHostProvider;
    }

    public IScheduledTaskContext CreateContext(IScheduledTaskManager scheduledTaskManager)
    {
        return new GenericScheduledTaskContext(scheduledTaskManager) { Services = Services, TaskTypeName = this.GetType().FullName, SyncState = SyncStateManager.GetForSystem() };
    }

    public Task Execute(IScheduledTaskContext context)
    {
        var knownHosts = slowPokeHostProvider.AllExceptCurrent;
        context.OutputLog.Add($"Hello world! Known hosts: {knownHosts.Count()}");

        var broadcasterProviderResolver = Services.GetRequiredService<IBroadcastProviderResolver>();

        var sendCount = 0;
        var exceptions = new List<Exception>();
        try
        {
            broadcasterProviderResolver.MemCached.SendUnsentMessages(msg =>
            {
                context.OutputLog.Add($"Sending #{sendCount}) {msg.EventGuid} ({msg.Type})");
                broadcasterProviderResolver.HttpKnownHosts.Publish(msg, context.CancellationToken);
                sendCount++;
            });
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }
        context.OutputLog.Add($"Sent {sendCount} (errors: {exceptions.Count})");

        try
        {
            var msgs = broadcasterProviderResolver.HttpKnownHosts.Receive(Guid.Empty, context.CancellationToken).ToList();
            context.OutputLog.Add($"Received {msgs.Count}");
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

        context.OutputLog.Add("Done!");
        return Task.CompletedTask;
    }
}