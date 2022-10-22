using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Models.Diff;


public class NodeFingerprintModel : INodeFingerprint
{
    public NodeFingerprintModel(INodePath path, string hash, DateTime lastChangeDate, string metaHash, DateTime metaLastChangeDate): this(path.ConvertToAbsolutePath().PathValue, hash, lastChangeDate, metaHash, metaLastChangeDate)
    {
    }

    public static NodeFingerprintModel CastOrCopy(INodeFingerprint fingerprint)
    {
        if (fingerprint is NodeFingerprintModel model)
        {
            return model;
        }
        else
        {
            return new NodeFingerprintModel(fingerprint.Path, fingerprint.Hash, fingerprint.LastChangeDate, fingerprint.MetaHash, fingerprint.MetaLastChangeDate);
        }
    }

    public NodeFingerprintModel(string path, string hash, DateTime lastChangeDate, string metaHash, DateTime metaLastChangeDate)
    {
        Path = path;
        Hash = hash;
        LastChangeDate = lastChangeDate;
        MetaHash = metaHash;
        MetaLastChangeDate = metaLastChangeDate;
    }

    public NodeFingerprintModel()
    {
        
    }

    public string Path { get; set; }

    public string Hash { get; set; }

    public DateTime LastChangeDate { get; set; }

    public string MetaHash { get; set; }

    public DateTime MetaLastChangeDate { get; set; }
}