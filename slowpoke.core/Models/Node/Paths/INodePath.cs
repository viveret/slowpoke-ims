namespace slowpoke.core.Models.Node.Docs;


public interface INodePath
{
    string PathValue { get; }

    bool IsDocument { get; }

    bool IsMeta { get; }

    bool IsFolder { get; }

    bool IsUri { get; }
    
    bool IsRelative { get; }
    
    bool IsAbsolute { get; }


    INodePath ConvertToAbsolutePath();
    
    INodePath ConvertToUriPath();

    INodePath ConvertToMetaPath(); // if this is a document, get the meta path for it. otherwise throw exception.

    INodePath RemoveMetaExtension(); // if this is a meta path, get the doc path for it. otherwise throw exception.
}