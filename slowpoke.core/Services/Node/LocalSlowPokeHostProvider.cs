using System.Net;
using Microsoft.Extensions.Logging;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Http;
using slowpoke.core.Services.Identity;

namespace slowpoke.core.Services.Node;


public class LocalSlowPokeHostProvider: SlowPokeHostProviderBase
{    
    public IIpAddressHistoryService IpAddressHistoryService { get; }

    public LocalSlowPokeHostProvider(
        Config config,
        IIdentityAuthenticationService identityAuthenticationService,
        IIpAddressHistoryService ipAddressHistoryService): base(config, identityAuthenticationService)
    {
        IpAddressHistoryService = ipAddressHistoryService;
    }
    
    public override ISlowPokeHost Current
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

    public override async Task<SearchForLocalNetworkHostsResult> SearchForLocalNetworkHosts(ILogger logger, CancellationToken cancellationToken)
    {
        var iteratorResults = new IteratorResults();
        IPAddress? localIp = GetLocalIp();

        return new SearchForLocalNetworkHostsResult
        {
            Hosts = (await IterateLocalNetworkHosts(localIp, iteratorResults, logger, cancellationToken)).ToList(),
        };
    }

    private static IPAddress? GetLocalIp()
    {
        var localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        var localIp = localAddresses.Where(ip => !ip.ToString().StartsWith("127.0.") && !ip.ToString().Equals("::1")).FirstOrDefault();
        return localIp;
    }

    private async Task<IEnumerable<ISlowPokeHost>> IterateLocalNetworkHosts(IPAddress? localIp, IteratorResults iteratorResults, ILogger logger, CancellationToken cancellationToken)
    {
        var localIpStr = localIp?.ToString();
        var hasLocalIp = !string.IsNullOrWhiteSpace(localIpStr);

        // first one is this one
        var ret = new List<ISlowPokeHost>();
        var uri = new Uri($"https://{(hasLocalIp ? localIpStr : "localhost")}");
        var client = await OpenClient(uri, cancellationToken: cancellationToken);
        ret.Add(await SlowPokeHost.Resolve(client, this));

        if (!Config.P2P.AllowSearchForLocalNetworkHosts || !hasLocalIp)
        {
            return ret;
        }

        var ipBase = string.Join(".", localIp.GetAddressBytes().Select(b => b.ToString()).Take(3)) + ".";
        var ipAddressByteToSkip = localIp.GetAddressBytes().Last();
        var tasks = new List<Task<ISlowPokeHost>>();
        for (short i = 100; i < 255; i++)
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
                return ret;
            }

            var timeoutSrc = new CancellationTokenSource(500);
            var ip = $"{ipBase}{i}";

            tasks.Add(Task<ISlowPokeHost>.Run(async Task<ISlowPokeHost> () => {
                logger.LogInformation($"Trying {ip}");

                var ping = false;
                ISlowPokeClient? resolver = null;
                try
                {
                    resolver = await OpenClient(new Uri($"https://{ip}"), cancellationToken: cancellationToken);
                    ping = await resolver.Ping(timeoutSrc.Token);
                    iteratorResults.noExceptionCount++;
                }
                catch (HttpRequestException e)
                {
                    logger.LogError(e, $"HTTP Error from {ip}: {e.Message}");
                    iteratorResults.exceptionCount++;
                    return null;
                }
                catch (TaskCanceledException e)
                {
                    logger.LogError(e, $"Timeout from {ip}: {e.Message}");
                    iteratorResults.timeoutCount++;
                    return null;
                }

                if (ping && resolver != null)
                {
                    iteratorResults.successCount++;
                    var host = await SlowPokeHost.Resolve(resolver, this);
                    logger.LogInformation($"Successful response from {ip} (label = {host.Label}, guid = {host.Guid})");
                    return host;
                }
                else
                {
                    iteratorResults.softFailCount++;
                    return null;
                }
            }));
        }

        var awaitedRet = await Task.WhenAll(tasks);
        ret.AddRange(awaitedRet.Where(v => v != null));

        return ret;
    }

    public override Task AddNewKnownButUntrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken)
    {
        var newHostEndpoints = hosts.Select(h => h?.Endpoint?.ToString()).Where(v => v != null);
        Config.P2P.KnownButUntrustedHosts = (Config.P2P.KnownButUntrustedHosts != null ? Config.P2P.KnownButUntrustedHosts.Concat(newHostEndpoints) : newHostEndpoints).ToList();
        Config.Save(cancellationToken);

        return Task.CompletedTask;
    }

    public override Task AddNewTrustedHosts(IEnumerable<ISlowPokeHost> hosts, CancellationToken cancellationToken)
    {
        var newHostEndpoints = hosts.Select(h => h?.Endpoint?.ToString()).Where(v => v != null);
        Config.P2P.KnownButUntrustedHosts = (Config.P2P.TrustedHosts != null ? Config.P2P.TrustedHosts.Concat(newHostEndpoints) : newHostEndpoints).ToList();
        Config.Save(cancellationToken);

        return Task.CompletedTask;
    }

    public override Task<ISlowPokeClient> OpenClient(ISlowPokeHost host, CancellationToken cancellationToken)
    {
        return HttpSlowPokeClient.CreateClient(host.Endpoint, Config, cancellationToken: cancellationToken);
    }

    public override Task<ISlowPokeHost> GetHost(Uri location, CancellationToken cancellationToken)
    {
        return Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel { Endpoint = location });
    }
}