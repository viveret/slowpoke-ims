using System.Text;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyDocument: IReadOnlyNode, IEquatable<IReadOnlyDocument>, IComparable<IReadOnlyDocument>
{
    string ContentType { get; }

    Stream OpenRead();

    string ReadAllText(Encoding encoding, int numLines = 0);
}
