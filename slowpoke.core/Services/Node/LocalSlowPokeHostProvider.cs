using System.Net;
using Microsoft.Extensions.Logging;
using slowpoke.core.Client;
using slowpoke.core.Models;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Http;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Node;


public class LocalSlowPokeHostProvider: ISlowPokeHostProvider
{
    public IEnumerable<ISlowPokeHost> All { get; }
    
    public IEnumerable<ISlowPokeHost> AllExceptCurrent { get; }
    
    public Config Config { get; }
    
    public IIdentityAuthenticationService IdentityAuthenticationService { get; }
    
    public IIpAddressHistoryService IpAddressHistoryService { get; }

    // IdentityAuthenticationService.TrustedIdentities.Where(identity => IpAddressHistoryService.GetHistory(CancellationToken.None).Contains(e => e.AuthGuid == identity.AuthGuid))

    public IEnumerable<ISlowPokeHost> Trusted => Config.P2P.TrustedHosts.Select(h => new SlowPokeHostModel { Endpoint = new Uri(h) }).ToList();

    public IEnumerable<ISlowPokeHost> KnownButUntrusted => Config.P2P.KnownButUntrustedHosts.Select(h => new SlowPokeHostModel { Endpoint = new Uri(h) }).ToList();

    public LocalSlowPokeHostProvider(
        Config config,
        IIdentityAuthenticationService identityAuthenticationService,
        IIpAddressHistoryService ipAddressHistoryService)
    {
        Config = config;
        IdentityAuthenticationService = identityAuthenticationService;
        IpAddressHistoryService = ipAddressHistoryService;
    }
    
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

    public SearchForLocalNetworkHostsResult SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken)
    {
        var iteratorResults = new IteratorResults();
        IPAddress? localIp = GetLocalIp();

        return new SearchForLocalNetworkHostsResult
        {
            Hosts = IterateLocalNetworkHosts(localIp, iteratorResults, logger, cancellationToken).ToList(),
        };
    }

    private static IPAddress? GetLocalIp()
    {
        var localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        var localIp = localAddresses.Where(ip => !ip.ToString().StartsWith("127.0.") && !ip.ToString().Equals("::1")).FirstOrDefault();
        return localIp;
    }

    private IEnumerable<ISlowPokeHost> IterateLocalNetworkHosts(IPAddress? localIp, IteratorResults iteratorResults, ILogger logger, CancellationToken cancellationToken)
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
        for (short i = 100; i < 256; i++)
        {
            if (i == ipAddressByteToSkip)
            {
                iteratorResults.successCount++;
                logger.LogInformation($"Skipping {localIpStr}");
                continue;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation($"Task cancelled");
                yield break;
            }

            var timeoutSrc = new CancellationTokenSource(500);
            var ip = $"{ipBase}{i}";
            logger.LogInformation($"Trying {ip}");

            var ping = false;
            ISlowPokeClient resolver = null;
            try
            {
                resolver = HttpSlowPokeClient.Connect($"https://{ip}", Config);
                ping = resolver.Ping(timeoutSrc.Token);
                iteratorResults.noExceptionCount++;
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e, $"HTTP Error from {ip}: {e.Message}");
                iteratorResults.exceptionCount++;
            }
            catch (TaskCanceledException e)
            {
                logger.LogError(e, $"Timeout from {ip}: {e.Message}");
                iteratorResults.timeoutCount++;
            }

            if (ping && resolver != null)
            {
                iteratorResults.successCount++;
                var host = new SlowPokeHost(resolver, this);
                logger.LogInformation($"Successful response from {ip} (label = {host.Label}, guid = {host.Guid})");

                yield return host;
            }
            else
            {
                iteratorResults.softFailCount++;
            }
        }
    }

    public void AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken)
    {
        var newHostEndpoints = hosts.Select(h => h.Endpoint.ToString());
        Config.P2P.KnownButUntrustedHosts = Config.P2P.KnownButUntrustedHosts.Concat(newHostEndpoints).ToArray();
    }
}