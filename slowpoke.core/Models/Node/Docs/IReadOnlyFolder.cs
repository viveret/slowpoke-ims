using System.Text;

namespace slowpoke.core.Models.Node.Docs;

public interface IReadOnlyFolder: IReadOnlyNode, IEquatable<IReadOnlyFolder>, IComparable<IReadOnlyFolder>
{
    public int SizeFiles { get; }
    
    public int SizeFolders { get; }
}
