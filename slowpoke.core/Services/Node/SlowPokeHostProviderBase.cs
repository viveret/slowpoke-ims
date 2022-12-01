using Microsoft.Extensions.Logging;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Identity;

namespace slowpoke.core.Services.Node;


public abstract class SlowPokeHostProviderBase: ISlowPokeHostProvider
{
    public IEnumerable<ISlowPokeHost> All => Trusted.Concat(KnownButUntrusted);
    
    public IEnumerable<ISlowPokeHost> AllExceptCurrent => All.Where(h => h.Guid != Current.Guid);
    
    public Config Config { get; }
    
    public IIdentityAuthenticationService IdentityAuthenticationService { get; }

    public IEnumerable<ISlowPokeHost> Trusted => Config.P2P.TrustedHosts.Select(h => new Uri(h)).Concat(
            IdentityAuthenticationService.TrustedIdentities.Select(identity => IdentityAuthenticationService.GetEndpointForOriginGuid(identity.IdentityGuid, CancellationToken.None))
        ).Select(endpoint => new SlowPokeHostModel { Endpoint = endpoint! }).ToList();

    public IEnumerable<ISlowPokeHost> KnownButUntrusted => Config.P2P.KnownButUntrustedHosts.Select(h => new SlowPokeHostModel { Endpoint = new Uri(h) }).ToList();

    public abstract ISlowPokeHost Current { get; }

    public SlowPokeHostProviderBase(
        Config config,
        IIdentityAuthenticationService identityAuthenticationService)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        IdentityAuthenticationService = identityAuthenticationService ?? throw new ArgumentNullException(nameof(identityAuthenticationService));
    }
    
    public async Task<ISlowPokeClient> OpenClient(Uri location, CancellationToken cancellationToken)
    {
        var host = await GetHost(location, cancellationToken);
        return await OpenClient(host, cancellationToken);
    }

    public abstract Task<ISlowPokeHost> GetHost(Uri location, CancellationToken cancellationToken);
    public abstract Task<ISlowPokeClient> OpenClient(ISlowPokeHost host, CancellationToken cancellationToken);
    public abstract Task<SearchForLocalNetworkHostsResult> SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken);    
    public abstract Task AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken);
    public abstract Task AddNewTrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken);
}