using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Util;

namespace slowpoke.core.Models.Diff;


public class DocumentDiff: StreamDiff
{    
    public IReadOnlyDocument Old { get; }
    
    public IReadOnlyDocument New { get; }

    public DocumentDiff(IReadOnlyDocument old, IReadOnlyDocument @new)
    : base(
            (old ?? throw new ArgumentNullException(nameof(old))).OpenRead(),
            (@new ?? throw new ArgumentNullException(nameof(@new))).OpenRead())
    {
        Old = old;
        New = @new;
    }
}