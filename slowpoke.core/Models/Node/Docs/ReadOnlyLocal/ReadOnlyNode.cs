using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyLocal;



public abstract class ReadOnlyNode: IReadOnlyNode
{
    protected readonly IReadOnlyDocumentResolver documentResolver;
    protected readonly IBroadcastProvider BroadcastProvider;

    public INodePath Path { get; protected set; }

    public ReadOnlyNode(IReadOnlyDocumentResolver documentResolver, IBroadcastProvider broadcastProvider, INodePath path)
    {
        this.documentResolver = documentResolver ?? throw new ArgumentNullException(nameof(documentResolver));
        this.BroadcastProvider = broadcastProvider ?? throw new ArgumentNullException(nameof(broadcastProvider));
        this.Path = path ?? throw new ArgumentNullException(nameof(path));
    }


    public virtual Task<IReadOnlyDocumentMeta> Meta { get => documentResolver.GetMeta(this, CancellationToken.None); }
    
    public virtual Task<bool> HasMeta => documentResolver.HasMeta(this, CancellationToken.None);

    public abstract Task<bool> Exists { get; }
    
    public abstract Task<long> SizeBytes { get; }
    
    public abstract Task<NodePermissionCategories<bool>> Permissions { get; }

    public Task<ISlowPokeHost> Host => documentResolver.Host;

    public ISlowPokeId SlowPokeId => new SlowPokeIdModel { RawId = Path.PathValue, RawIdType = GetType().Name, Endpoint = new Uri($"file://{Path.ConvertToAbsolutePath().PathValue}") };

    public Task<bool> CanSync => documentResolver.CanSync;

    public async Task<bool> Equals(IReadOnlyNode? other) => await CompareTo(other) == 0;

    public Task<int> CompareTo(IReadOnlyNode? other)
    {
        throw new NotImplementedException();
    }

    protected T OnUnauthorizedReturn0<T>(Func<T> value) where T: struct
    {
        try
        {
            return value();
        }
        catch (System.IO.IOException e) when (e.Message.StartsWith("Permission denied"))
        {
            return default;
        }
        catch (UnauthorizedAccessException)
        {
            return default;
        }
    }

    public abstract Task Sync(CancellationToken cancellationToken);
    public abstract Task BroadcastChanges(CancellationToken cancellationToken);
    public abstract Task PollForChanges(CancellationToken cancellationToken);
    public abstract Task TurnOnSync(CancellationToken cancellationToken);
    public abstract Task TurnOffSync(CancellationToken cancellationToken);
    public abstract Task MergeChanges(CancellationToken cancellationToken);
    public abstract Task<string> ComputeHash();
    public abstract Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken);
    public abstract Task<INodeFingerprint> GetFingerprint(CancellationToken cancellationToken);
}