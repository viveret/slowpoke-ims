using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Broadcast;
using SlowPokeIMS.Core.Services.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;


public class GenericDocumentProviderResolver: IDocumentProviderResolver
{
    private readonly Config config;

    public GenericDocumentProviderResolver(
        Config config, IBroadcastProviderResolver broadcastProviderResolver)
    {
        this.config = config;

        ReadLocal = Task.FromResult<IReadOnlyDocumentResolver>(new GenericReadWriteDocumentResolver(config, new InMemoryGenericDocumentRepository(), broadcastProviderResolver));
        ReadWriteLocal = Task.FromResult<IWritableDocumentResolver>(new GenericReadWriteDocumentResolver(config, new InMemoryGenericDocumentRepository(), broadcastProviderResolver));

        ReadOnlyLocalDriveProviders = Task.FromResult<IEnumerable<IReadOnlyDocumentResolver>>(Enumerable.Empty<IReadOnlyDocumentResolver>());
        ReadRemotes = Task.FromResult<IEnumerable<IReadOnlyDocumentResolver>>(Enumerable.Empty<IReadOnlyDocumentResolver>());
        ReadWriteRemotes = Task.FromResult<IEnumerable<IWritableDocumentResolver>>(Enumerable.Empty<IWritableDocumentResolver>());
        AllReadonlyProviders = Task.FromResult<IEnumerable<IReadOnlyDocumentResolver>>(Enumerable.Empty<IReadOnlyDocumentResolver>());
        AllReadWriteProviders = Task.FromResult<IEnumerable<IWritableDocumentResolver>>(Enumerable.Empty<IWritableDocumentResolver>());
        ResolveReadable = Task.FromResult<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>>(new NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>());
        ResolveWritable = Task.FromResult<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>>(new NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>());
        UnifiedReadable = Task.FromResult<NodePermissionCategories<IReadOnlyDocumentResolver>>(new NodePermissionCategories<IReadOnlyDocumentResolver>());
        UnifiedWritable = Task.FromResult<NodePermissionCategories<IWritableDocumentResolver>>(new NodePermissionCategories<IWritableDocumentResolver>());
    }

    public Task<ISlowPokeHost> Host { get => Task.FromResult<ISlowPokeHost>(new SlowPokeHostModel()); }
    
    public Task<IReadOnlyDocumentResolver> ReadLocal { get; set; }

    public Task<IWritableDocumentResolver> ReadWriteLocal { get; set; }

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadOnlyLocalDriveProviders { get; set; }

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadRemotes { get; set; }

    public Task<IEnumerable<IWritableDocumentResolver>> ReadWriteRemotes { get; set; }

    public Task<IEnumerable<IReadOnlyDocumentResolver>> AllReadonlyProviders { get; set; }
    
    public Task<IEnumerable<IWritableDocumentResolver>> AllReadWriteProviders { get; set; }

    public Task<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>> ResolveReadable { get; set; }
    
    public Task<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>> ResolveWritable { get; set; }

    public Task<NodePermissionCategories<IReadOnlyDocumentResolver>> UnifiedReadable { get; set; }
    
    public Task<NodePermissionCategories<IWritableDocumentResolver>> UnifiedWritable { get; set; }

    public bool IsForAutomatedTests => true;

    public Task<IReadOnlyDocumentResolver> OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        throw new NotImplementedException();
    }

    public Task<IWritableDocumentResolver> OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        throw new NotImplementedException();
    }
}