using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Broadcast.Messages;



public class DocMetaChangedBroacastMessage: DocFingerprintBroacastMessage
{
    public DocMetaChangedBroacastMessage(INodeFingerprint fingerprint): base(fingerprint)
    {
    }

    public DocMetaChangedBroacastMessage(): base()
    {
    }
}