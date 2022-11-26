using System.Text;
using SlowPokeIMS.Core.Collections;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyDocument: IReadOnlyNode, IAsyncEquatable<IReadOnlyDocument>, IAsyncComparable<IReadOnlyDocument>
{
    Task<string> GetContentType();

    Task<Stream> OpenRead();

    Task<string> ReadAllText(Encoding encoding, int numLines = 0);
}
