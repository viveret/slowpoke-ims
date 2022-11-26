using System.Text;

namespace slowpoke.core.Models.Node.Docs.ReadOnlyRemote;


public class JsonReadOnlyDocument : JsonReadOnlyNode, IReadOnlyDocument
{
    public JsonReadOnlyDocument(): base() { }
    
    protected JsonReadOnlyDocument(IReadOnlyDocument other): base(other)
    {
    }

    public static async Task<JsonReadOnlyDocument> CreateDoc(IReadOnlyDocument other)
    {
        var ret = new JsonReadOnlyDocument(other);
        await ret.InitAsync(other);
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