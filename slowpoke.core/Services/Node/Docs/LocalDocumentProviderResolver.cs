using System.Collections.Concurrent;
using System.Net;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node.Docs.ReadOnlyLocal;
using SlowPokeIMS.Core.Collections;
using SlowPokeIMS.Core.Extensions;

namespace slowpoke.core.Services.Node.Docs;


public class LocalDocumentProviderResolver : IDocumentProviderResolver
{
    private static IEnumerable<IReadOnlyDocumentResolver> CachedReadOnlyLocalNetworkProviders { get; set; } = Enumerable.Empty<IReadOnlyDocumentResolver>();
    private static Task? cacheTask;

    public Task<ISlowPokeHost> Host { get; private set; }

    public Config Config { get; }

    public void AssignLocalHost(ISlowPokeHost localhost)
    {
        ArgumentNullException.ThrowIfNull(localhost);
        if (Host != null)
        {
            throw new Exception($"{nameof(Host)} already set");
        }
        Host = Task.FromResult(localhost);
    }

    private readonly IEnumerable<IReadOnlyDocumentResolver> systemReadonlyProviders;
    private readonly IEnumerable<IWritableDocumentResolver> systemReadWriteProviders;
    private readonly ISlowPokeHostProvider slowPokeHostProvider;
    private readonly ConcurrentDictionary<Uri, IReadOnlyDocumentResolver> cachedReadonlyRemoteProvider;
    private readonly ConcurrentDictionary<Uri, IWritableDocumentResolver> cachedReadWriteRemoteProvider;

    public LocalDocumentProviderResolver(
        IEnumerable<IReadOnlyDocumentResolver> readonlyProviders,
        IEnumerable<IWritableDocumentResolver> readWriteProviders,
        ISlowPokeHostProvider slowPokeHostProvider,
        Config config)
    {
        this.systemReadonlyProviders = readonlyProviders ?? throw new ArgumentNullException(nameof(readonlyProviders));
        this.systemReadWriteProviders = readWriteProviders ?? throw new ArgumentNullException(nameof(readWriteProviders));
        this.slowPokeHostProvider = slowPokeHostProvider ?? throw new ArgumentNullException(nameof(slowPokeHostProvider));
        this.Config = config ?? throw new ArgumentNullException(nameof(config));
        cachedReadonlyRemoteProvider = new ConcurrentDictionary<Uri, IReadOnlyDocumentResolver>();
        cachedReadWriteRemoteProvider = new ConcurrentDictionary<Uri, IWritableDocumentResolver>();

        if (Config.P2P.AutoCacheLocalNetworkHosts)
        {
            CacheLocalNetworkHosts(CancellationToken.None);
        }
    }

    public Task<IEnumerable<IReadOnlyDocumentResolver>> AllReadonlyProviders => Task.FromResult(systemReadonlyProviders
                                                                            .Concat(cachedReadonlyRemoteProvider.Values)
                                                                            .Concat(ReadOnlyLocalDriveProviders.Result)
                                                                            .Concat(ReadOnlyLocalNetworkProviders));

    public Task<IEnumerable<IWritableDocumentResolver>> AllReadWriteProviders => Task.FromResult(systemReadWriteProviders.Concat(cachedReadWriteRemoteProvider.Values));//.Concat(ReadWriteLocalDriveProviders);

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadOnlyLocalDriveProviders => Task.FromResult<IEnumerable<IReadOnlyDocumentResolver>>(GetDrives().Select(d => new ReadOnlyLocalDriveDocumentProvider(d, this)));

    public IEnumerable<IReadOnlyDocumentResolver> ReadOnlyLocalNetworkProviders => CachedReadOnlyLocalNetworkProviders;// { get; private set; } = Enumerable.Empty<IReadOnlyDocumentResolver>(); //.Select(d => new ReadOnlyLocalDriveDocumentProvider(d, this));

    private IEnumerable<DriveInfo> GetDrives()
    {
        return DriveInfo.GetDrives().Where(d =>
            (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed || d.DriveType == DriveType.CDRom) &&
            d.TotalSize > 0 && d.AvailableFreeSpace > 0 && !Config.Paths.IsOS(d.Name));
    }

    public void CacheLocalNetworkHosts(CancellationToken cancellationToken)
    {
        if (Config.P2P.AutoCacheLocalNetworkHosts && !CachedReadOnlyLocalNetworkProviders.Any() && cacheTask == null)
        {
            cacheTask = Task.Run(async () =>
            {
                var list = new List<IReadOnlyDocumentResolver>();
                CachedReadOnlyLocalNetworkProviders = list;
                foreach (var resolver in await SearchForLocalNetworkDocumentProviders(CancellationToken.None))
                {
                    list.Add(resolver);
                }
            });
        }
    }

    private async Task<IEnumerable<IReadOnlyDocumentResolver>> SearchForLocalNetworkDocumentProviders(CancellationToken cancellationToken)
    {
        var noExceptionCount = 0;
        var exceptionCount = 0;
        var timeoutCount = 0;
        var softFailCount = 0;
        var successCount = 0;

        var localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        var localIp = localAddresses.Where(ip => !ip.ToString().StartsWith("127.0.") && !ip.ToString().Equals("::1")).FirstOrDefault();
        var localIpStr = localIp?.ToString();
        var hasLocalIp = !string.IsNullOrWhiteSpace(localIpStr);

        // first one is this one
        var ret = new List<IReadOnlyDocumentResolver>()
        {
            new HttpReadOnlyRemoteDocumentResolver(
                await slowPokeHostProvider.OpenClient(
                    new Uri($"https://{(hasLocalIp ? localIpStr : "localhost")}"),
                    cancellationToken: cancellationToken), Config, this)
        };

        if (!Config.P2P.AllowSearchForLocalNetworkHosts || !hasLocalIp)
        {
            return ret;
        }

        var ipBase = string.Join(".", localIp.GetAddressBytes().Select(b => b.ToString()).Take(3)) + ".";
        var ipAddressByteToSkip = localIp.GetAddressBytes().Last();

        for (short i = 0; i < 256; i++)
        {
            if (i == ipAddressByteToSkip)
            {
                successCount++;
                continue;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return ret;
            }

            var timeoutSrc = new CancellationTokenSource(500);
            var ip = $"{ipBase}{i}";
            var ping = false;
            ISlowPokeClient? slowPokeClient = null;
            try
            {
                slowPokeClient = await slowPokeHostProvider.OpenClient(new Uri($"https://{ip}"), cancellationToken: cancellationToken);
                ping = await slowPokeClient.Ping(timeoutSrc.Token);
                noExceptionCount++;
            }
            catch (HttpRequestException)
            {
                exceptionCount++;
            }
            catch (TaskCanceledException)
            {
                timeoutCount++;
            }

            if (ping && slowPokeClient != null)
            {
                successCount++;
                ret.Add(new HttpReadOnlyRemoteDocumentResolver(slowPokeClient, Config, this));
            }
            else
            {
                softFailCount++;
            }
        }

        return ret;
    }

    //public IEnumerable<IReadOnlyDocumentResolver> ReadWriteLocalDriveProviders => DriveInfo.GetDrives().Select(d => new ReadWriteLocalDriveDocumentProvider(d, this));


    public Task<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>> ResolveReadable
    {
        get
        {
            return NodePermissionCategories<IReadOnlyDocumentResolver>.FilterPermissions(
                AllReadonlyProviders, async provider => (await provider.Permissions).CanRead);
        }
    }

    public Task<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>> ResolveWritable
    {
        get
        {
            return NodePermissionCategories<IWritableDocumentResolver>.FilterPermissions(
                AllReadWriteProviders, async provider => (await provider.Permissions).CanWrite);
        }
    }

    public Task<NodePermissionCategories<IReadOnlyDocumentResolver>> UnifiedReadable
    {
        get
        {
            return ResolveReadable.TransformAsync(p => Task.FromResult<IReadOnlyDocumentResolver>(new UnifiedReadOnlyDocumentProvider(this, p)));
        }
    }

    public Task<NodePermissionCategories<IWritableDocumentResolver>> UnifiedWritable
    {
        get
        {
            return ResolveWritable.TransformAsync(p => Task.FromResult<IWritableDocumentResolver>(new UnifiedWritableDocumentProvider(this, p)));
        }
    }

    public Task<IReadOnlyDocumentResolver> ReadLocal => systemReadonlyProviders.SingleAsync(FilterLocal);

    public Task<IWritableDocumentResolver> ReadWriteLocal => systemReadWriteProviders.SingleAsync(FilterLocal);

    private static async Task<bool> FilterLocal<T>(T p) where T: IReadOnlyDocumentResolver
    {
        var perms = await p.Permissions;
        return perms.LimitedToUserOnly || perms.LimitedToMachineOnly;
    }

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadRemotes => systemReadonlyProviders.WhereAsync(async p => (await p.Permissions).IsRemote).ConcatAsync(cachedReadonlyRemoteProvider.Values).ConcatAsync(ReadOnlyLocalNetworkProviders);

    public Task<IEnumerable<IWritableDocumentResolver>> ReadWriteRemotes => systemReadWriteProviders.WhereAsync(async p => (await p.Permissions).IsRemote).ConcatAsync(cachedReadWriteRemoteProvider.Values);

    public bool IsForAutomatedTests => false;

    public Task<IReadOnlyDocumentResolver> OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        if (cachedReadonlyRemoteProvider.TryGetValue(endpoint, out var provider))
        {
            return Task.FromResult(provider);
        }
        else
        {
            return OpenNewReadRemoteAndCache(endpoint, cacheDuration);
        }
    }

    private async Task<IReadOnlyDocumentResolver> OpenNewReadRemoteAndCache(Uri endpoint, TimeSpan? cacheDuration)
    {
        var provider = new HttpReadOnlyRemoteDocumentResolver(await slowPokeHostProvider.OpenClient(endpoint, CancellationToken.None), Config, this);
        cachedReadonlyRemoteProvider[endpoint] = provider;
        return provider;
    }

    public Task<IWritableDocumentResolver> OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        if (cachedReadWriteRemoteProvider.TryGetValue(endpoint, out var provider))
        {
            return Task.FromResult(provider);
        }
        else
        {
            return OpenNewReadWriteRemoteAndCache(endpoint, cacheDuration);
        }
    }

    private async Task<IWritableDocumentResolver> OpenNewReadWriteRemoteAndCache(Uri endpoint, TimeSpan? cacheDuration)
    {
        var provider = new HttpReadWriteRemoteDocumentResolver(await slowPokeHostProvider.OpenClient(endpoint, CancellationToken.None), this, Config);
        cachedReadWriteRemoteProvider[endpoint] = provider;
        return provider;
    }
}