using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Broadcast.Messages;



public class DocumentChangedBroadcastMessage: DocFingerprintBroacastMessage
{
    public DocumentChangedBroadcastMessage(INodeFingerprint fingerprint): base(fingerprint)
    {
    }

    public DocumentChangedBroadcastMessage(): base()
    {
    }
}