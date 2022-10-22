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


    public virtual IReadOnlyDocumentMeta Meta { get => documentResolver.GetMeta(this, CancellationToken.None); }
    
    public virtual bool HasMeta => documentResolver.HasMeta(this, CancellationToken.None);

    public abstract bool Exists { get; }
    
    public abstract long SizeBytes { get; }
    
    public abstract NodePermissionCategories<bool> Permissions { get; }

    public ISlowPokeHost Host => documentResolver.Host;

    public ISlowPokeId SlowPokeId => new SlowPokeIdModel { RawId = Path.PathValue, RawIdType = GetType().Name, Endpoint = new Uri($"file:{Path.ConvertToAbsolutePath().PathValue}") };

    public bool CanSync => documentResolver.CanSync;

    public bool Equals(IReadOnlyNode? other) => CompareTo(other) == 0;

    public int CompareTo(IReadOnlyNode? other)
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

    public abstract void Sync(CancellationToken cancellationToken);
    public abstract void BroadcastChanges(CancellationToken cancellationToken);
    public abstract void PollForChanges(CancellationToken cancellationToken);
    public abstract void TurnOnSync(CancellationToken cancellationToken);
    public abstract void TurnOffSync(CancellationToken cancellationToken);
    public abstract string ComputeHash();
    public abstract void MergeChanges(CancellationToken cancellationToken);
    public abstract IEnumerable<INodeDiffBrief> FetchChanges(CancellationToken cancellationToken);
    public abstract INodeFingerprint GetFingerprint(CancellationToken cancellationToken);
}