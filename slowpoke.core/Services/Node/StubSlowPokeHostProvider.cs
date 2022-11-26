using Microsoft.Extensions.Logging;
using slowpoke.core.Client;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node;

namespace SlowPokeIMS.Core.Services.Node;


public class StubSlowPokeHostProvider : ISlowPokeHostProvider
{
    public IEnumerable<ISlowPokeHost> All => Enumerable.Empty<ISlowPokeHost>();

    public IEnumerable<ISlowPokeHost> Trusted => Enumerable.Empty<ISlowPokeHost>();

    public IEnumerable<ISlowPokeHost> KnownButUntrusted => Enumerable.Empty<ISlowPokeHost>();

    public IEnumerable<ISlowPokeHost> AllExceptCurrent => Enumerable.Empty<ISlowPokeHost>();

    public ISlowPokeHost Current => new SlowPokeHostModel();

    public Task AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<ISlowPokeHost> GetHost(Uri location, CancellationToken cancellationToken)
    {
        return Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel());
    }

    public Task<ISlowPokeClient> OpenClient(Uri location, CancellationToken cancellationToken)
    {
        return Task.FromResult<ISlowPokeClient>(new StubSlowPokeClient(location));
    }

    public Task<ISlowPokeClient> OpenClient(ISlowPokeHost host, CancellationToken cancellationToken)
    {
        return OpenClient(host.Endpoint, cancellationToken);
    }

    public Task<SearchForLocalNetworkHostsResult> SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SearchForLocalNetworkHostsResult());
    }
}