using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public class BroadcastMessageHandlerResolver : IBroadcastMessageHandlerResolver
{
    private readonly IEnumerable<IBroadcastMessageHandler> handlers;
    private readonly Dictionary<Type, IBroadcastMessageHandler> mappedHandlers;

    public BroadcastMessageHandlerResolver(IEnumerable<IBroadcastMessageHandler> handlers)
    {
        this.handlers = handlers;
        this.mappedHandlers = handlers.SelectMany(h => h.MessageTypesAllowed.Select(t => (h, t))).ToDictionary(k => k.t, v => v.h);
    }

    public void Handle(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        ResolveForType(message.GetType()).Process(message, cancellationToken);
    }

    public IBroadcastMessageHandler ResolveForType(Type t)
    {
        ArgumentNullException.ThrowIfNull(t);
        if (mappedHandlers.TryGetValue(t, out var handler))
        {
            return handler;
        }
        else
        {
            foreach (var h in handlers)
            {
                foreach (var handlerType in h.MessageTypesAllowed)
                {
                    if (t.IsAssignableFrom(handlerType))
                    {
                        return h;
                    }
                }
            }

            throw new ArgumentOutOfRangeException($"Could not find handler for {t.FullName}");
        }
    }
}