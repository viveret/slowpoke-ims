using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Collections;

namespace SlowPokeIMS.Tests.Core.Services;


public class TestDocumentProviderResolver : IDocumentProviderResolver
{
    private readonly IReadOnlyDocumentResolver readLocal;
    private readonly IWritableDocumentResolver readWriteLocal;
    private readonly IEnumerable<IReadOnlyDocumentResolver> readOnlyLocalDriveProviders;
    private readonly IEnumerable<IReadOnlyDocumentResolver> readRemotes;
    private readonly IEnumerable<IWritableDocumentResolver> readWriteRemotes;
    private readonly IEnumerable<IReadOnlyDocumentResolver> allReadonlyProviders;
    private readonly IEnumerable<IWritableDocumentResolver> allReadWriteProviders;
    
    private readonly IEnumerable<IReadOnlyDocumentResolver> readResolvers;
    private readonly IEnumerable<IWritableDocumentResolver> writeResolvers;

    public TestDocumentProviderResolver(
        IReadOnlyDocumentResolver readLocal, IWritableDocumentResolver readWriteLocal,
        IEnumerable<IReadOnlyDocumentResolver> readOnlyLocalDriveProviders,
        IEnumerable<IReadOnlyDocumentResolver> readRemotes,
        IEnumerable<IWritableDocumentResolver> readWriteRemotes,
        IEnumerable<IReadOnlyDocumentResolver> allReadonlyProviders,
        IEnumerable<IWritableDocumentResolver> allReadWriteProviders)
    {
        this.readLocal = readLocal;
        this.readWriteLocal = readWriteLocal;
        this.readOnlyLocalDriveProviders = readOnlyLocalDriveProviders;
        this.readRemotes = readRemotes;
        this.readWriteRemotes = readWriteRemotes;
        this.allReadonlyProviders = allReadonlyProviders;
        this.allReadWriteProviders = allReadWriteProviders;
    }

    public TestDocumentProviderResolver(
        IEnumerable<IReadOnlyDocumentResolver> readResolvers,
        IEnumerable<IWritableDocumentResolver> writeResolvers)
    {
        this.readResolvers = readResolvers;
        this.writeResolvers = writeResolvers;
    }

    public Task<ISlowPokeHost> Host { get; }

    public bool IsForAutomatedTests => true;

    private async Task<IReadOnlyDocumentResolver> FindReadResolver(Func<NodePermissionCategories<bool>, bool> predicate)
    {
        return (await readResolvers.WhereAsync(async r => predicate(await r.Permissions))).Single();
    }

    private async Task<IWritableDocumentResolver> FindReadWriteResolver(Func<NodePermissionCategories<bool>, bool> predicate)
    {
        return (await writeResolvers.WhereAsync(async r => predicate(await r.Permissions))).Single();
    }

    public Task<IReadOnlyDocumentResolver> ReadLocal => readLocal != null ? Task.FromResult(readLocal) : FindReadResolver(perms => (perms.LimitedToMachineOnly || perms.LimitedToUserOnly) && perms.CanRead);

    public Task<IWritableDocumentResolver> ReadWriteLocal => readWriteLocal != null ? Task.FromResult(readWriteLocal) : FindReadWriteResolver(perms => (perms.LimitedToMachineOnly || perms.LimitedToUserOnly) && perms.CanWrite);

        
        // var ReadRemotes = systemReadonlyProviders.WhereAsync(async p => (await p.Permissions).IsRemote).ConcatAsync(cachedReadonlyRemoteProvider.Values).ConcatAsync(ReadOnlyLocalNetworkProviders);
        // var ReadWriteRemotes = systemReadWriteProviders.WhereAsync(async p => (await p.Permissions).IsRemote).ConcatAsync(cachedReadWriteRemoteProvider.Values);

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadOnlyLocalDriveProviders => Task.FromResult(readOnlyLocalDriveProviders);

    public Task<IEnumerable<IReadOnlyDocumentResolver>> ReadRemotes => Task.FromResult(readRemotes);

    public Task<IEnumerable<IWritableDocumentResolver>> ReadWriteRemotes => Task.FromResult(readWriteRemotes);

    public Task<IEnumerable<IReadOnlyDocumentResolver>> AllReadonlyProviders => Task.FromResult(allReadonlyProviders);

    public Task<IEnumerable<IWritableDocumentResolver>> AllReadWriteProviders => Task.FromResult(allReadWriteProviders);

    public Task<NodePermissionCategories<IEnumerable<IReadOnlyDocumentResolver>>> ResolveReadable
    {
        get
        {
            return NodePermissionCategories<IReadOnlyDocumentResolver>.FilterPermissions(
                AllReadonlyProviders, async provider => (await provider.Permissions).CanRead);
        }
    }

    public Task<NodePermissionCategories<IEnumerable<IWritableDocumentResolver>>> ResolveWritable => throw new NotImplementedException();

    public Task<NodePermissionCategories<IReadOnlyDocumentResolver>> UnifiedReadable => throw new NotImplementedException();

    public Task<NodePermissionCategories<IWritableDocumentResolver>> UnifiedWritable => throw new NotImplementedException();

    public Task<IReadOnlyDocumentResolver> OpenReadRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        throw new NotImplementedException();
    }

    public Task<IWritableDocumentResolver> OpenReadWriteRemote(Uri endpoint, TimeSpan? cacheDuration)
    {
        throw new NotImplementedException();
    }
}