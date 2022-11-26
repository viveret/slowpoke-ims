using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Util;

namespace slowpoke.core.Models.Diff;


public class DocumentDiff: StreamDiff
{    
    public IReadOnlyDocument Old { get; }
    
    public IReadOnlyDocument New { get; }

    public async Task<bool> CreatedOrDeleted() => (await Old.Exists) != (await New.Exists);

    public bool ChangedPath => Old.Path?.PathValue != New.Path?.PathValue;

    protected DocumentDiff(
        Stream oldStream, Stream newStream,
        IReadOnlyDocument old, IReadOnlyDocument @new,
        bool canRewind,
        bool fullStreamDiff,
        CancellationToken cancellationToken)
    : base(oldStream, newStream, canRewind, fullStreamDiff, cancellationToken)
    {
        Old = old;
        New = @new;
    }

    protected virtual async Task CompareDocs(IReadOnlyDocument old, IReadOnlyDocument @new)
    {
        HasChanged = HasChanged || (await CreatedOrDeleted()) || ChangedPath;
    }

    public static async Task<DocumentDiff> Create(IReadOnlyDocument old, IReadOnlyDocument @new, bool fullStreamDiff, CancellationToken cancellationToken)
    {
        var streamOld = await (old ?? throw new ArgumentNullException(nameof(old))).Exists ? await old.OpenRead() : Stream.Null;
        var streamNew = await (@new ?? throw new ArgumentNullException(nameof(@new))).Exists ? await @new.OpenRead() : Stream.Null;
        var diff = new DocumentDiff(
            streamOld,
            streamNew,
            old, @new,
            streamOld.CanSeek && streamNew.CanSeek,
            fullStreamDiff,
            cancellationToken);
        await diff.CompareDocs(old, @new);
        return diff;
    }
}