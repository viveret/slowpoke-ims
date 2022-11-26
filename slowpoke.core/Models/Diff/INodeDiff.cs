using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Models.Diff;


public interface INodeDiff: INodeDiffBrief
{
    public IReadOnlyDocument OldDoc { get; }
    
    public IReadOnlyDocument NewDoc { get; }
}