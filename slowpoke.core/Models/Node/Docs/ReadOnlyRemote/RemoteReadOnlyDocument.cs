using System.Data;
using System.Security.Cryptography;
using System.Text;
using slowpoke.core.Extensions;
using slowpoke.core.Models.Diff;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyRemote;


public class RemoteReadOnlyDocument : RemoteReadOnlyNode, IReadOnlyDocument
{
    protected RemoteReadOnlyDocument(IReadOnlyDocument modelDocument, IReadOnlyDocumentResolver documentResolver):
        base(modelDocument, documentResolver)
    {
        // if (!this.Path.IsDocument)
        //     throw new ArgumentException("path is not a document", nameof(this.Path));
    }

    public static async Task<RemoteReadOnlyDocument> CreateDoc(IReadOnlyDocument modelNode, IReadOnlyDocumentResolver documentResolver)
    {
        var ret = new RemoteReadOnlyDocument(modelNode, documentResolver);
        await ret.InitAsync(modelNode);
        return ret;
    }

    public Task<int> CompareTo(IReadOnlyDocument other)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Equals(IReadOnlyDocument other)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetContentType()
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenRead()
    {
        throw new NotImplementedException();
    }

    public Task<string> ReadAllText(Encoding encoding, int numLines = 0)
    {
        throw new NotImplementedException();
    }
}