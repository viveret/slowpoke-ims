using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Broadcast.Messages;



public class SyncStartedBroadcastMessage: DocFingerprintBroacastMessage
{
    public SyncStartedBroadcastMessage(INodeFingerprint fingerprint): base(fingerprint)
    {
    }

    public SyncStartedBroadcastMessage(): base()
    {
    }
}