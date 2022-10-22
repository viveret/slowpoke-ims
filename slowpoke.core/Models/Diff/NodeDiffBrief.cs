using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Models.Diff;


public class NodeDiffBrief : INodeDiffBrief
{
    public NodeDiffBrief(IReadOnlyNode old, IReadOnlyNode @new, CancellationToken cancellationToken): this(old?.GetFingerprint(cancellationToken), @new?.GetFingerprint(cancellationToken))
    {
        if (old is IReadOnlyDocument document)
        {
            if (@new is IReadOnlyDocument newDoc)
            {
            }
            else
            {
                throw new ArgumentException(nameof(@new));
            }
        }
        else if (old is IReadOnlyFolder folder)
        {
            if (@new is IReadOnlyFolder newFolder)           
            {
                throw new NotSupportedException();
            }
            else
            {
                throw new ArgumentException(nameof(@new));
            }
        }
    }

    public NodeDiffBrief(INodeFingerprint old, INodeFingerprint @new)
    {
        Old = old;
        New = @new;

        HasContentChanged = Old.Hash != New.Hash || Old.LastChangeDate < New.LastChangeDate;
        HasMetaChanged = Old.MetaHash != New.MetaHash || Old.MetaLastChangeDate < New.MetaLastChangeDate;
        HasPathChanged = Old.Path != New.Path;
        HasChanged = HasPathChanged || HasContentChanged || HasMetaChanged;
    }

    public bool HasChanged { get; }

    public bool HasMetaChanged { get; }

    public bool HasContentChanged { get; }

    public bool HasPathChanged { get; }

    public INodeFingerprint Old { get; }

    public INodeFingerprint New { get; }
}