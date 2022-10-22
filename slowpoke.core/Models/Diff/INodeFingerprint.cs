using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Models.Diff;


public interface INodeFingerprint
{
    string Path { get; }
    
    string Hash { get; }
    
    DateTime LastChangeDate { get; }
    
    string MetaHash { get; }
    
    DateTime MetaLastChangeDate { get; }
}