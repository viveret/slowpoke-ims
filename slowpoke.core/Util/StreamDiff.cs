using slowpoke.core.Extensions;

namespace slowpoke.core.Util;



public class StreamDiff
{
    public StreamDiff(
        Action<Stream> writer, Stream original,
        CancellationToken cancellationToken)
    {
        using var TmpStream = new MemoryStream();
        writer(TmpStream);
        CompareStreams(original, TmpStream);
    }

    public StreamDiff(Stream old, Stream @new)
    {
        CompareStreams(old, @new);
    }

    private void CompareStreams(Stream original, Stream newStream)
    {
        NewHash = newStream.ComputeMD5FromStream();

        if (original != null)
        {
            OriginalHash = original.ComputeMD5FromStream();

            if (NewHash != OriginalHash)
            {
                HasChanged = true;
            }
        }
        else
        {
            HasChanged = true;
        }
    }

    public bool HasChanged { get; private set; }
    public string NewHash { get; private set; }
    public string OriginalHash { get; private set; }
}