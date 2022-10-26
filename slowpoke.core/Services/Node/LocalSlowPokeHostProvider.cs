using System.Net;
using slowpoke.core.Client;
using slowpoke.core.Models;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Node;


public class LocalSlowPokeHostProvider: ISlowPokeHostProvider
{
    public LocalSlowPokeHostProvider(
        Config config,
        IIdentityAuthenticationService identityAuthenticationService)
    {
        Config = config;
        IdentityAuthenticationService = identityAuthenticationService;
    }

    public IEnumerable<ISlowPokeHost> All { get; }
    
    public IEnumerable<ISlowPokeHost> AllExceptCurrent { get; }
    
    public ISlowPokeHost Current
    {
        get
        {
            var localIp = GetLocalIp();
            var localIpStr = localIp?.ToString();
            var hasLocalIp = !string.IsNullOrWhiteSpace(localIpStr);

            return new SlowPokeHostModel()
            {
                Endpoint = new Uri($"https://{(hasLocalIp ? localIp : "localhost")}"),
                Guid = IdentityAuthenticationService.CurrentIdentity.AuthGuid,
            };
        }
    }
    
    public Config Config { get; }
    
    public IIdentityAuthenticationService IdentityAuthenticationService { get; }

    public SearchForLocalNetworkHostsResult SearchForLocalNetworkHosts(CancellationToken cancellationToken)
    {
        var iteratorResults = new IteratorResults();
        IPAddress? localIp = GetLocalIp();

        return new SearchForLocalNetworkHostsResult
        {
            Hosts = IterateLocalNetworkHosts(localIp, iteratorResults, cancellationToken).ToList(),
        };
    }

    private static IPAddress? GetLocalIp()
    {
        var localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        var localIp = localAddresses.Where(ip => !ip.ToString().StartsWith("127.0.") && !ip.ToString().Equals("::1")).FirstOrDefault();
        return localIp;
    }

    private IEnumerable<ISlowPokeHost> IterateLocalNetworkHosts(IPAddress? localIp, IteratorResults iteratorResults, CancellationToken cancellationToken)
    {
        var localIpStr = localIp?.ToString();
        var hasLocalIp = !string.IsNullOrWhiteSpace(localIpStr);

        // first one is this one
        yield return new SlowPokeHost(new HttpSlowPokeClient(new Uri($"https://{(hasLocalIp ? localIpStr : "localhost")}"), Config), this);

        if (!Config.P2P.AllowSearchForLocalNetworkHosts || !hasLocalIp)
        {
            yield break;
        }

        var ipBase = string.Join(".", localIp.GetAddressBytes().Select(b => b.ToString()).Take(3)) + ".";
        var ipAddressByteToSkip = localIp.GetAddressBytes().Last();

        for (short i = 0; i < 256; i++)
        {
            if (i == ipAddressByteToSkip)
            {
                iteratorResults.successCount++;
                continue;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var timeoutSrc = new CancellationTokenSource(500);
            var ip = $"{ipBase}{i}";
            var ping = false;
            ISlowPokeClient resolver = null;
            try
            {
                resolver = HttpSlowPokeClient.Connect($"https://{ip}", Config);
                ping = resolver.Ping(timeoutSrc.Token);
                iteratorResults.noExceptionCount++;
            }
            catch (HttpRequestException)
            {
                iteratorResults.exceptionCount++;
            }
            catch (TaskCanceledException)
            {
                iteratorResults.timeoutCount++;
            }

            if (ping && resolver != null)
            {
                iteratorResults.successCount++;
                yield return new SlowPokeHost(resolver, this);
            }
            else
            {
                iteratorResults.softFailCount++;
            }
        }
    }
}