using System.Text.Json.Serialization;
using slowpoke.core.Models.Configuration;

namespace slowpoke.core.Models.Node.Docs;


public class NodePathModel : INodePath
{
    public NodePathModel()
    {
    }

    public NodePathModel(INodePath other)
    {
        PathValue = other.PathValue;
        IsDocument = other.IsDocument;
        IsMeta = other.IsMeta;
        IsFolder = other.IsFolder;
        IsUri = other.IsUri;
        IsRelative = other.IsRelative;
        IsAbsolute = other.IsAbsolute;
    }

    [JsonPropertyName("pathValue")]
    public string PathValue { get; set; }

    [JsonPropertyName("isDocument")]
    public bool IsDocument { get; set; }

    [JsonPropertyName("isMeta")]
    public bool IsMeta { get; set; }

    [JsonPropertyName("isFolder")]
    public bool IsFolder { get; set; }

    [JsonPropertyName("isUri")]
    public bool IsUri { get; set; }

    [JsonPropertyName("isRelative")]
    public bool IsRelative { get; set; }

    [JsonPropertyName("isAbsolute")]
    public bool IsAbsolute { get; set; }

    public INodePath ConvertToAbsolutePath()
    {
        throw new NotImplementedException();
    }

    public INodePath ConvertToMetaPath()
    {
        throw new NotImplementedException();
    }

    public INodePath ConvertToUriPath()
    {
        throw new NotImplementedException();
    }

    public bool Equals(INodePath? other)
    {
        throw new NotImplementedException();
    }

    public INodePath RemoveMetaExtension()
    {
        throw new NotImplementedException();
    }
}