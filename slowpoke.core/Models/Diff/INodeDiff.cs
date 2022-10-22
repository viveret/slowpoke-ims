using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Models.Diff;


public interface INodeDiff: INodeDiffBrief
{
    public IReadOnlyDocument Old { get; }
    
    public IReadOnlyDocument New { get; }
}