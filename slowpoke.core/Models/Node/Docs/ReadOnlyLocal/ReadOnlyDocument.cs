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

    public async Task<Stream> OpenRead() => await Exists ? File.OpenRead(this.GetPathValidated()) : System.IO.Stream.Null;

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
    public async Task<bool> Equals(IReadOnlyDocument? other) => await CompareTo(other) == 0;


    // todo: allow compareTo to be async
    public async Task<int> CompareTo(IReadOnlyDocument? other)
    {
        if (other != null)
        {
            using var otherStream = await other.OpenRead();
            using var stream = await OpenRead();
            var otherStreamMD5 = otherStream.ComputeMD5FromStream(false);
            var thisStreamMD5 = stream.ComputeMD5FromStream(false);

            return thisStreamMD5.CompareTo(otherStreamMD5);
        }
        return -1;
    }

    public async Task<string> ReadAllText(Encoding encoding, int numLines = 0)
    {
        using var stream = new System.IO.StreamReader(await OpenRead(), encoding);
        if (numLines > 0)
        {
            var sb = new StringBuilder();
            int numLinesRead = 0;
            const int numCharsPerline = 200;
            while (!stream.EndOfStream && numLinesRead < numLines)
            {
                var line = (await stream.ReadLineAsync()) ?? string.Empty;
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
            return await stream.ReadToEndAsync();
        }
    }

    public override Task Sync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task BroadcastChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //BroadcastProvider.Publish(new ChangedMessage());
    }

    public override Task PollForChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //BroadcastProvider.Receive();
    }

    public override Task TurnOnSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
    }

    public override Task TurnOffSync(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        // Meta.SyncEnabled = false;
    }

    public override async Task<string> ComputeHash()
    {
        using var stream = await OpenRead();
        return stream.ComputeMD5FromStream(false);
    }

    public override Task MergeChanges(CancellationToken cancellationToken)
    {
        throw new ReadOnlyException();
        //documentResolver.MergeChangesForNode(this, cancellationToken);
    }

    public override async Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken)
    {
        var currentVersion = await GetFingerprint(cancellationToken);
        var otherVersions = await documentResolver.FetchFingerprintsForNode(this, cancellationToken);
        return otherVersions.Select(v => new NodeDiffBrief(currentVersion, v));
    }

    public override async Task<INodeFingerprint> GetFingerprint(CancellationToken cancellationToken)
    {
        var meta = await Meta;
        return new NodeFingerprintModel(Path, await ComputeHash(), meta.LastUpdate, meta.ComputeMetaHash(), meta.LastMetaUpdate);
    }

    public override Task<bool> Exists => Task.FromResult(File.Exists(this.Path.ConvertToAbsolutePath().PathValue));

    public override Task<long> SizeBytes => GetSizeBytesAsync();

    private async Task<long> GetSizeBytesAsync()
    {
        return await Exists ? OnUnauthorizedReturn0(() => new FileInfo(this.GetPathValidated()).Length) : 0L;
    }

    public Task<string> GetContentType() => documentResolver.GetContentTypeFromExtension(this.Path.PathValue.GetFullExtension());

    public override Task<NodePermissionCategories<bool>> Permissions
    {
        get
        {
            var accessControl = new FileInfo(this.Path.ConvertToAbsolutePath().PathValue).GetAccessControl();
            return Task.FromResult(new NodePermissionCategories<bool>
            {
                CanRead = Exists.Result && accessControl != null,
                CanWrite = Exists.Result && accessControl != null,
                LimitedToUserOnly = false,
                LimitedToMachineOnly = true,
                LimitedToAllowedConnectionsOnly = false,
                LimitedToLocalNetworkOnly = false,
                UnlimitedUniversalPublicAccess = false,
            });
        }
    }
}