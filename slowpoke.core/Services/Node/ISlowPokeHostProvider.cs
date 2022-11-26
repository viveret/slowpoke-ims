using Microsoft.Extensions.Logging;
using slowpoke.core.Client;
using slowpoke.core.Models.Node;

namespace slowpoke.core.Services.Node;


public interface ISlowPokeHostProvider
{
    Task<ISlowPokeClient> OpenClient(Uri location, CancellationToken cancellationToken);
    
    Task<ISlowPokeClient> OpenClient(ISlowPokeHost host, CancellationToken cancellationToken);

    Task<ISlowPokeHost> GetHost(Uri location, CancellationToken cancellationToken);

    IEnumerable<ISlowPokeHost> All { get; }
    
    IEnumerable<ISlowPokeHost> Trusted { get; }
    
    IEnumerable<ISlowPokeHost> KnownButUntrusted { get; }
    
    IEnumerable<ISlowPokeHost> AllExceptCurrent { get; }
    
    ISlowPokeHost Current { get; }

    Task<SearchForLocalNetworkHostsResult> SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken);
    
    Task AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken);
}