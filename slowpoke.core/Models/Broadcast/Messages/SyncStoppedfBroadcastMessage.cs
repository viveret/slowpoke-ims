using slowpoke.core.Models.Diff;

namespace slowpoke.core.Models.Broadcast.Messages;



public class SyncStoppedBroadcastMessage: DocFingerprintBroacastMessage
{
    public SyncStoppedBroadcastMessage(NodeFingerprintModel fingerprint): base(fingerprint)
    {
    }

    public SyncStoppedBroadcastMessage(): base()
    {
    }
}