using slowpoke.core.Models.Configuration;

namespace slowpoke.core.Models.Node.Docs;

public abstract class DocPathBase : INodePath
{
    protected Config Config { get; }
    
    public string PathValue { get; }

    protected DocPathBase(string pathValue, Config config)
    {
        this.PathValue = !string.IsNullOrWhiteSpace(pathValue) ? pathValue : throw new ArgumentNullException(nameof(pathValue));
        this.Config = config;
    }

    public abstract bool IsUri { get; }
    public abstract bool IsRelative { get; }
    public abstract bool IsAbsolute { get; }

    public bool IsDocument => File.Exists(PathValue) || (!Directory.Exists(PathValue) && Path.HasExtension(PathValue) && !PathValue.EndsWith(slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension));

    public bool IsMeta => Path.HasExtension(PathValue) && PathValue.EndsWith(slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension);

    public bool IsFolder => PathValue.EndsWith('/') || Directory.Exists(PathValue);

    public abstract INodePath ConvertToAbsolutePath();
    
    public abstract INodePath ConvertToUriPath();

    public override bool Equals(object? obj)
    {
        return object.ReferenceEquals(this, obj) || (obj is INodePath doc && this.Equals(doc));
    }
    
    public bool Equals(INodePath? doc)
    {
        return doc != null && this.ConvertToAbsolutePath().PathValue == doc.ConvertToAbsolutePath().PathValue;
    }

    public override int GetHashCode()
    {
        return this.ConvertToAbsolutePath().PathValue.GetHashCode();
    }

    public override string? ToString()
    {
        return this.PathValue;
    }

    // todo: need to fix this to undo. Maybe a new function called UndoConvertToMetaPath() or ConvertToOriginalPath()
    public INodePath ConvertToMetaPath()
    {
        if (Path.HasExtension(PathValue))
        {
            if (PathValue.EndsWith(slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension))
            {
                return this;
            }
            else
            {
                var metaPath = PathValue;
                metaPath = metaPath + slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension;
                return new DocPathAbsolute(metaPath, Config);
            }
        }
        else
        {
            var metaPath = PathValue;
            metaPath = metaPath + slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension;
            return new DocPathAbsolute(metaPath, Config);
            //throw new Exception($"Path {PathValue} cannot be converted to meta path");
        }
    }

    public INodePath RemoveMetaExtension()
    {
        if (Path.HasExtension(PathValue))
        {
            if (!PathValue.EndsWith(slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension))
            {
                return this;
            }
            else
            {
                var metaPath = PathValue;
                metaPath = metaPath.Substring(0, metaPath.Length - slowpoke.core.Models.Configuration.Config.PathsConfig.DocMetaExtension.Length);
                return new DocPathAbsolute(metaPath, Config);
            }
        }
        else
        {
            throw new Exception($"Path {PathValue} is not meta path");
        }
    }
}
