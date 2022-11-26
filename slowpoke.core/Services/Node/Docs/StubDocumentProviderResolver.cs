using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;
using SlowPokeIMS.Core.Services.Node.Docs.ReadWrite;

namespace slowpoke.core.Services.Node.Docs;


public class StubDocumentProviderResolver: IDocumentProviderResolver
{
    private readonly Config config;

    public StubDocumentProviderResolver(Config config)
    {
        this.config = config;
    }

    public Task<ISlowPokeHost> Host { get => Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel()); }
    
    public Task<IReadOnlyDocumentResolver> ReadLocal => Task.FromResult<IReadOnlyDocumentResolver>(new StubReadOnlyDocumentResolver(config));

    public Task<IWritableDocumentResolver> ReadWriteLocal => Task.FromResult<IWritableDocumentResolver>(new StubWritableDocumentResolver(config));

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadOnlyLocalDriveProviders { get; } = Task.FromResult(Enumerable.Empty<IReadOnlyDocumentResolver>());

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadRemotes { get; } = Task.FromResult(Enumerable.Empty<IReadOnlyDocumentResolver>());

    public Task<IEnumerable<IWritableDocumentResolver>> ReadWriteRemotes { get; } = Task.FromResult(Enumerable.Empty<IWritableDocumentResolver>());

    public Task<IEnumerable<IReadOnlyDocumentResolver>> AllReadonlyProviders { get; } = Task.FromResult(Enumerable.Empty<IReadOnlyDocumentResolver>());
    
    public Task<IEnumerable<IWritableDocumentResolver>> AllReadWriteProviders { get; } = Task.FromResult(Enumerable.Empty<IWritableDocumentResolver>());

    public Task<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>> ResolveReadable => Task.FromResult<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>>(new ()
    {
        CanRead = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        CanWrite = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        IsEncrypted = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        IsRemote = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        LimitedToAllowedConnectionsOnly = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        LimitedToLocalNetworkOnly = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        LimitedToMachineOnly = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        LimitedToUserOnly = Enumerable.Empty<IReadOnlyDocumentResolver>(),
        UnlimitedUniversalPublicAccess = Enumerable.Empty<IReadOnlyDocumentResolver>(),
    });
    
    public Task<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>> ResolveWritable => Task.FromResult<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>>(new ()
    {
        CanRead = Enumerable.Empty<IWritableDocumentResolver>(),
        CanWrite = Enumerable.Empty<IWritableDocumentResolver>(),
        IsEncrypted = Enumerable.Empty<IWritableDocumentResolver>(),
        IsRemote = Enumerable.Empty<IWritableDocumentResolver>(),
        LimitedToAllowedConnectionsOnly = Enumerable.Empty<IWritableDocumentResolver>(),
        LimitedToLocalNetworkOnly = Enumerable.Empty<IWritableDocumentResolver>(),
        LimitedToMachineOnly = Enumerable.Empty<IWritableDocumentResolver>(),
        LimitedToUserOnly = Enumerable.Empty<IWritableDocumentResolver>(),
        UnlimitedUniversalPublicAccess = Enumerable.Empty<IWritableDocumentResolver>(),
    });

    public Task<NodePermissionCategories<IReadOnlyDocumentResolver>> UnifiedReadable => Task.FromResult<NodePermissionCategories<IReadOnlyDocumentResolver>>(new ()
    {
        CanRead = new StubReadOnlyDocumentResolver(config),
        CanWrite = new StubReadOnlyDocumentResolver(config),
        IsEncrypted = new StubReadOnlyDocumentResolver(config),
        IsRemote = new StubReadOnlyDocumentResolver(config),
        LimitedToAllowedConnectionsOnly = new StubReadOnlyDocumentResolver(config),
        LimitedToLocalNetworkOnly = new StubReadOnlyDocumentResolver(config),
        LimitedToMachineOnly = new StubReadOnlyDocumentResolver(config),
        LimitedToUserOnly = new StubReadOnlyDocumentResolver(config),
        UnlimitedUniversalPublicAccess = new StubReadOnlyDocumentResolver(config),
    });
    
    public Task<NodePermissionCategories<IWritableDocumentResolver>> UnifiedWritable => Task.FromResult<NodePermissionCategories<IWritableDocumentResolver>>(new ()
    {
        CanRead = new StubWritableDocumentResolver(config),
        CanWrite = new StubWritableDocumentResolver(config),
        IsEncrypted = new StubWritableDocumentResolver(config),
        IsRemote = new StubWritableDocumentResolver(config),
        LimitedToAllowedConnectionsOnly = new StubWritableDocumentResolver(config),
        LimitedToLocalNetworkOnly = new StubWritableDocumentResolver(config),
        LimitedToMachineOnly = new StubWritableDocumentResolver(config),
        LimitedToUserOnly = new StubWritableDocumentResolver(config),
        UnlimitedUniversalPublicAccess = new StubWritableDocumentResolver(config),
    });

    public bool IsForAutomatedTests => true;

    public Task<IReadOnlyDocumentResolver> OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        return Task.FromResult<IReadOnlyDocumentResolver>(new StubReadOnlyDocumentResolver(config));
    }

    public Task<IWritableDocumentResolver> OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        return Task.FromResult<IWritableDocumentResolver>(new StubWritableDocumentResolver(config));
    }
}