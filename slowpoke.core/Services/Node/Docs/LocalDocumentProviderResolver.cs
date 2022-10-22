using System.Collections.Concurrent;
using System.Net;
using slowpoke.core.Client;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node.Docs.ReadOnlyLocal;

namespace slowpoke.core.Services.Node.Docs;


public class LocalDocumentProviderResolver : IDocumentProviderResolver
{
    private static IEnumerable<IReadOnlyDocumentResolver> CachedReadOnlyLocalNetworkProviders { get; set; } = Enumerable.Empty<IReadOnlyDocumentResolver>();
    private static Task? cacheTask;

    public ISlowPokeHost Host { get; private set; }

    public Config Config { get; }

    public void AssignLocalHost(ISlowPokeHost localhost)
    {
        ArgumentNullException.ThrowIfNull(localhost);
        if (Host != null)
        {
            throw new Exception($"{nameof(Host)} already set");
        }
        Host = localhost;
    }

    private readonly IEnumerable<IReadOnlyDocumentResolver> systemReadonlyProviders;
    private readonly IEnumerable<IWritableDocumentResolver> systemReadWriteProviders;

    private readonly ConcurrentDictionary<Uri, IReadOnlyDocumentResolver> cachedReadonlyRemoteProvider;
    private readonly ConcurrentDictionary<Uri, IWritableDocumentResolver> cachedReadWriteRemoteProvider;

    public LocalDocumentProviderResolver(
        IEnumerable<IReadOnlyDocumentResolver> readonlyProviders,
        IEnumerable<IWritableDocumentResolver> readWriteProviders,
        Config config)
    {
        this.systemReadonlyProviders = readonlyProviders ?? throw new ArgumentNullException(nameof(readonlyProviders));
        this.systemReadWriteProviders = readWriteProviders ?? throw new ArgumentNullException(nameof(readWriteProviders));
        this.Config = config ?? throw new ArgumentNullException(nameof(config));
        cachedReadonlyRemoteProvider = new ConcurrentDictionary<Uri, IReadOnlyDocumentResolver>();
        cachedReadWriteRemoteProvider = new ConcurrentDictionary<Uri, IWritableDocumentResolver>();

        if (Config.P2P.AutoCacheLocalNetworkHosts)
        {
            CacheLocalNetworkHosts(CancellationToken.None);
        }
    }

    public IEnumerable<IReadOnlyDocumentResolver> AllReadonlyProviders => systemReadonlyProviders
                                                                            .Concat(cachedReadonlyRemoteProvider.Values)
                                                                            .Concat(ReadOnlyLocalDriveProviders)
                                                                            .Concat(ReadOnlyLocalNetworkProviders);
    
    public IEnumerable<IWritableDocumentResolver> AllReadWriteProviders => systemReadWriteProviders.Concat(cachedReadWriteRemoteProvider.Values);//.Concat(ReadWriteLocalDriveProviders);

    public IEnumerable<IReadOnlyDocumentResolver> ReadOnlyLocalDriveProviders => GetDrives().Select(d => new ReadOnlyLocalDriveDocumentProvider(d, this));
    
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
            cacheTask = Task.Run(() =>
            {
                var list = new List<IReadOnlyDocumentResolver>();
                CachedReadOnlyLocalNetworkProviders = list;
                foreach (var resolver in SearchForLocalNetworkDocumentProviders(CancellationToken.None))
                {
                    list.Add(resolver);
                }
            });
        }
    }

    private IEnumerable<IReadOnlyDocumentResolver> SearchForLocalNetworkDocumentProviders(CancellationToken cancellationToken)
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
        yield return new HttpReadOnlyRemoteDocumentResolver(new HttpSlowPokeClient(new Uri($"https://{(hasLocalIp ? localIpStr : "localhost")}"), Config), this, Config);

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
                successCount++;
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

            if (ping)
            {
                successCount++;
                yield return new HttpReadOnlyRemoteDocumentResolver(resolver, this, Config);
            }
            else
            {
                softFailCount++;
            }
        }
    }

    //public IEnumerable<IReadOnlyDocumentResolver> ReadWriteLocalDriveProviders => DriveInfo.GetDrives().Select(d => new ReadWriteLocalDriveDocumentProvider(d, this));


    public NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>> ResolveReadable
    {
        get
        {
            return new NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>
            {
                CanRead = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                CanWrite = Enumerable.Empty<IReadOnlyDocumentResolver>(),
                IsEncrypted = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                LimitedToUserOnly = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                LimitedToMachineOnly = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                LimitedToLocalNetworkOnly = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                LimitedToAllowedConnectionsOnly = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                UnlimitedUniversalPublicAccess = AllReadonlyProviders.Where(provider => provider.Permissions.CanRead),
                IsRemote = Enumerable.Empty<IReadOnlyDocumentResolver>(),
            };
        }
    }

    public NodePermissionCategories<IEnumerable<IWritableDocumentResolver>> ResolveWritable
    {
        get
        {
            return new NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>
            {
                CanRead = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                CanWrite = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                IsEncrypted = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                LimitedToUserOnly = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                LimitedToMachineOnly = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                LimitedToLocalNetworkOnly = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                LimitedToAllowedConnectionsOnly = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                UnlimitedUniversalPublicAccess = AllReadWriteProviders.Where(provider => provider.Permissions.CanWrite),
                IsRemote = Enumerable.Empty<IWritableDocumentResolver>(),
            };
        }
    }

    public NodePermissionCategories<IReadOnlyDocumentResolver> UnifiedReadable
    {
        get
        {
            return ResolveReadable.Transform(p => (IReadOnlyDocumentResolver) new UnifiedReadOnlyDocumentProvider(this, p));
        }
    }

    public NodePermissionCategories<IWritableDocumentResolver> UnifiedWritable
    {
        get
        {
            return ResolveWritable.Transform<IWritableDocumentResolver>(p => new UnifiedWritableDocumentProvider(this, p));
        }
    }

    public IReadOnlyDocumentResolver ReadLocal => systemReadonlyProviders.Single(p => p.Permissions.LimitedToUserOnly || p.Permissions.LimitedToMachineOnly);

    public IWritableDocumentResolver ReadWriteLocal => systemReadWriteProviders.Single(p => p.Permissions.LimitedToUserOnly || p.Permissions.LimitedToMachineOnly);

    public IEnumerable<IReadOnlyDocumentResolver> ReadRemotes => systemReadonlyProviders.Where(p => p.Permissions.IsRemote).Concat(cachedReadonlyRemoteProvider.Values).Concat(ReadOnlyLocalNetworkProviders);

    public IEnumerable<IWritableDocumentResolver> ReadWriteRemotes => systemReadWriteProviders.Where(p => p.Permissions.IsRemote).Concat(cachedReadWriteRemoteProvider.Values);

    public IReadOnlyDocumentResolver OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        if (cachedReadonlyRemoteProvider.TryGetValue(endpoint, out var provider))
        {
            return provider;
        }
        else
        {
            return OpenNewReadRemoteAndCache(endpoint, cacheDuration);
        }
    }

    private IReadOnlyDocumentResolver OpenNewReadRemoteAndCache(Uri endpoint, TimeSpan? cacheDuration)
    {
        var provider = new HttpReadOnlyRemoteDocumentResolver(new HttpSlowPokeClient(endpoint, Config), this, Config);
        cachedReadonlyRemoteProvider[endpoint] = provider;
        return provider;
    }

    public IWritableDocumentResolver OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        if (cachedReadWriteRemoteProvider.TryGetValue(endpoint, out var provider))
        {
            return provider;
        }
        else
        {
            return OpenNewReadWriteRemoteAndCache(endpoint, cacheDuration);
        }
    }

    private IWritableDocumentResolver OpenNewReadWriteRemoteAndCache(Uri endpoint, TimeSpan? cacheDuration)
    {
        var provider = new HttpReadWriteRemoteDocumentResolver(new HttpSlowPokeClient(endpoint, Config), this, Config);
        cachedReadWriteRemoteProvider[endpoint] = provider;
        return provider;
    }
}