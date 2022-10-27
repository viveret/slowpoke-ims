using Microsoft.Extensions.Logging;
using slowpoke.core.Models.Node;

namespace slowpoke.core.Services.Node;


public interface ISlowPokeHostProvider
{
    IEnumerable<ISlowPokeHost> All { get; }
    
    IEnumerable<ISlowPokeHost> Trusted { get; }
    
    IEnumerable<ISlowPokeHost> KnownButUntrusted { get; }
    
    IEnumerable<ISlowPokeHost> AllExceptCurrent { get; }
    
    ISlowPokeHost Current { get; }

    SearchForLocalNetworkHostsResult SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken);
    
    void AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken);
}