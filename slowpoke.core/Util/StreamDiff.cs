using slowpoke.core.Extensions;

namespace slowpoke.core.Util;



public class StreamDiff
{
    public StreamDiff(
        Action<Stream> writer, Stream original,
        bool canRewind,
        bool fullStreamDiff,
        CancellationToken cancellationToken)
    {
        using var TmpStream = new MemoryStream();
        writer(TmpStream);
        CompareStreams(original, TmpStream, canRewind, fullStreamDiff, cancellationToken);
    }

    public StreamDiff(
        Stream old, Stream @new,
        bool canRewind,
        bool fullStreamDiff,
        CancellationToken cancellationToken)
    {
        CompareStreams(old, @new, canRewind, fullStreamDiff, cancellationToken);
    }

    protected virtual void CompareStreams(
        Stream original, Stream newStream,
        bool canRewind, bool fullStreamDiff,
        CancellationToken cancellationToken)
    {
        NewHash = newStream?.ComputeMD5FromStream(canRewind);

        if (original != null)
        {
            OriginalHash = original.ComputeMD5FromStream(canRewind);

            if (NewHash != OriginalHash)
            {
                if (newStream == null || !canRewind || !fullStreamDiff)
                {
                    HasChanged = true;
                }
                else
                {
                    HasChanged = !AreStreamsEqual(original, newStream, canRewind, cancellationToken);
                }
            }
        }
        else if (NewHash != null)
        {
            HasChanged = true;
        }
    }

    // https://stackoverflow.com/a/67820346/11765486
    public static bool AreStreamsEqual(Stream stream, Stream other, bool rewind, CancellationToken cancellationToken)
    {
        const int bufferSize = 2048;

        // this might not always work, todo: fix this
        if (other.Length != stream.Length)
        {
            return false;
        }

        byte[] buffer = new byte[bufferSize];
        byte[] otherBuffer = new byte[bufferSize];
        while ((_ = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            // cannot return false, would be bad return value and could lead to unexpected results / consequences
            cancellationToken.ThrowIfCancellationRequested();
            var _ = other.Read(otherBuffer, 0, otherBuffer.Length);

            if (!otherBuffer.SequenceEqual(buffer))
            {
                if (rewind)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    other.Seek(0, SeekOrigin.Begin);
                }
                return false;
            }
        }

        if (rewind)
        {
            stream.Seek(0, SeekOrigin.Begin);
            other.Seek(0, SeekOrigin.Begin);
        }
        return true;
    }

    public bool HasChanged { get; protected set; }
    
    public string? NewHash { get; private set; }
    public string? OriginalHash { get; private set; }
}