using Microsoft.Extensions.Logging;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node;

namespace SlowPokeIMS.Integration.Tests.Core;


public class TestSlowPokeHostProvider : SlowPokeHostProviderBase
{
    public TestSlowPokeHostProvider(
        Config config,
        TestServerFixture<TestStartup> fixure,
        IIdentityAuthenticationService identityAuthenticationService) : base(config, identityAuthenticationService)
    {
        Fixture = fixure;
    }

    public override ISlowPokeHost Current => throw new NotImplementedException();

    public TestServerFixture<TestStartup> Fixture { get; }

    public override Task AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<ISlowPokeHost> GetHost(Uri location, CancellationToken cancellationToken)
    {
        return Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel() { Endpoint = location });
    }

    public override Task<ISlowPokeClient> OpenClient(ISlowPokeHost host, CancellationToken cancellationToken)
    {
        // check which host it is host
        if (host.Endpoint.Host.Equals("automatedtest.local.client"))
        {
            if (int.TryParse(host.Endpoint.LocalPath.Substring(1), out var serverIndex))
            {
                switch (serverIndex)
                {
                    case 1:
                        return Fixture.GetClient1();
                    case 2:
                        return Fixture.GetClient2();
                }
            }
        }
        throw new ArgumentOutOfRangeException($"Host endpoint ({host.Endpoint.Host}) must be in the format https://automatedtest.local.client/i where i is the server index");
    }

    public override Task<SearchForLocalNetworkHostsResult> SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SearchForLocalNetworkHostsResult
        {
            Hosts = Fixture.Servers.Select(s => s.ToHostModel()),
        });
    }
}