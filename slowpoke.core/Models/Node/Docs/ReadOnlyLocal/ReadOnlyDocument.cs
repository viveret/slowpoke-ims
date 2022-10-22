using System.Data;
using System.Security.Cryptography;
using System.Text;
using slowpoke.core.Extensions;
using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyLocal;


public class ReadOnlyDocument : ReadOnlyNode, IReadOnlyDocument
{
    public ReadOnlyDocument(IReadOnlyDocumentResolver documentResolver, IBroadcastProvider broadcastProvider, INodePath path): base(documentResolver, broadcastProvider, path)
    {
        if (!this.Path.IsDocument)
            throw new ArgumentException("path is not a document", nameof(this.Path));
    }

    public Stream OpenRead() => Exists ? File.OpenRead(this.GetPathValidated()) : System.IO.Stream.Null;

    protected string GetPathValidated()
    {
        var p = this.Path.ConvertToAbsolutePath().PathValue;

        if (string.IsNullOrWhiteSpace(p))
            throw new ArgumentNullException(nameof(p));
        else if (File.Exists(p))
            return p;
        else
            throw new FileNotFoundException(p);
    }

    // different types of equals, this one performs hash equality (not byte equality)
    public bool Equals(IReadOnlyDocument? other) => CompareTo(other) == 0;

    public int CompareTo(IReadOnlyDocument? other)
    {
        if (other != null)
        {
            using var otherStream = other.OpenRead();
            using var stream = OpenRead();
            var otherStreamMD5 = otherStream.ComputeMD5FromStream();
            var thisStreamMD5 = stream.ComputeMD5FromStream();

            return thisStreamMD5.CompareTo(otherStreamMD5);
        }
        return -1;
    }

    public string ReadAllText(Encoding encoding, int numLines = 0)
    {
        using var stream = new System.IO.StreamReader(OpenRead(), encoding);
        if (numLines > 0)
        {
            var sb = new StringBuilder();
            int numLinesRead = 0;
            const int numCharsPerline = 200;
            while (!stream.EndOfStream && numLinesRead < numLines)
            {
                var line = stream.ReadLine();
                if (line.Length < numCharsPerline)
                {
                    sb.AppendLine(line);
                    numLinesRead++;
                }
                else
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        var c = line[i];
                        if (c == '\n')
                        {
                            numLinesRead++;
                        }
                        else if (i > 0 && i % numCharsPerline == 0)
                        {
                            numLinesRead++;
                        }
                        sb.Append(c);
                        if (numLinesRead >= numLines)
                        {
                            break;
                        }
                    }
                }
            }
            return sb.ToString();
        }
        else
        {
            return stream.ReadToEnd();
        }
    }

    public override void Sync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override void BroadcastChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //BroadcastProvider.Publish(new ChangedMessage());
    }

    public override void PollForChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //BroadcastProvider.Receive();
    }

    public override void TurnOnSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override void TurnOffSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        // Meta.SyncEnabled = false;
    }

    public override string ComputeHash()
    {
        using var stream = OpenRead();
        return stream.ComputeMD5FromStream();
    }

    public override void MergeChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //documentResolver.MergeChangesForNode(this, cancellationToken);
    }

    public override IEnumerable<INodeDiffBrief> FetchChanges(CancellationToken cancellationToken)
    {
        var currentVersion = GetFingerprint(cancellationToken);
        var otherVersions = documentResolver.FetchFingerprintsForNode(this, cancellationToken);
        return otherVersions.Select(v => new NodeDiffBrief(currentVersion, v));
    }

    public override INodeFingerprint GetFingerprint(CancellationToken cancellationToken)
    {
        return new NodeFingerprintModel(Path, ComputeHash(), Meta.LastUpdate, Meta.ComputeMetaHash(), Meta.LastMetaUpdate);
    }

    public override bool Exists => File.Exists(this.Path.ConvertToAbsolutePath().PathValue);

    public override long SizeBytes => Exists ? OnUnauthorizedReturn0(() => new FileInfo(this.GetPathValidated()).Length) : 0L;

    public string ContentType { get => documentResolver.GetContentTypeFromExtension(this.Path.PathValue.GetFullExtension()); }

    public override NodePermissionCategories<bool> Permissions
    {
        get
        {
            return new NodePermissionCategories<bool>
            {
                CanRead = Exists && new FileInfo(this.Path.ConvertToAbsolutePath().PathValue).GetAccessControl() != null,
                CanWrite = Exists && new FileInfo(this.Path.ConvertToAbsolutePath().PathValue).GetAccessControl() != null,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToAllowedConnectionsOnly = false,
                LimitedToLocalNetworkOnly = false,
                UnlimitedUniversalPublicAccess = false,
            };
        }
    }
}