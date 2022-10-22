using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Broadcast.Messages;



public class DocFingerprintBroacastMessage: BroadcastMessageBase
{
    public DocFingerprintBroacastMessage(INodeFingerprint fingerprint)
    {
        Fingerprint = NodeFingerprintModel.CastOrCopy(fingerprint);
        EventGuid = Guid.NewGuid();
    }

    public DocFingerprintBroacastMessage()
    {
    }

    public NodeFingerprintModel Fingerprint { get; set; }
}