using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node;
using SlowPokeIMS.Core.Services.Scheduled;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class SendAndReceiveBroadcastMessagesScheduledTask : ScheduledTaskBase
{
    private ISlowPokeHostProvider slowPokeHostProvider;

    public override string Title => "Send and receive broadcast messages";

    public override bool CanRunConcurrently => false;
    
    public override bool CanRunManually => true;

    public SendAndReceiveBroadcastMessagesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        ISlowPokeHostProvider slowPokeHostProvider): base(config, syncStateManager, services)
    {
        this.slowPokeHostProvider = slowPokeHostProvider;
    }

    public override async Task Execute(IScheduledTaskContext context)
    {
        var knownHosts = slowPokeHostProvider.AllExceptCurrent;
        context.OutputLog.Add($"Hello world! Known hosts: {knownHosts.Count()}");

        var broadcasterProviderResolver = Services.GetRequiredService<IBroadcastProviderResolver>();

        var sendCount = 0;
        var exceptions = new List<Exception>();
        try
        {
            await broadcasterProviderResolver.MemCached.SendUnsentMessages(msg =>
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
            var msgs = (await broadcasterProviderResolver.HttpKnownHosts.Receive(Guid.Empty, context.CancellationToken)).ToList();
            context.OutputLog.Add($"Received {msgs.Count}");
            await broadcasterProviderResolver.MemCached.AddReceivedMessages(msgs);
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
    }
}