namespace slowpoke.core.Models.Node.Docs;


public class DocPathRelative : DocPathBase
{
    public DocPathRelative(string pathValue, Config.Config config) : base(pathValue, config)
    {
    }

    public override bool IsUri => false;

    public override bool IsRelative => true;
    
    public override bool IsAbsolute => false;

    public override INodePath ConvertToAbsolutePath()
    {
        var pv = PathValue;
        if (pv.StartsWith("~/"))
        {
            pv = Path.Combine(Config.Paths.HomePath, pv.Substring(2));
        }
        return new DocPathAbsolute(Path.GetFullPath(pv), Config);
    }

    public override INodePath ConvertToUriPath()
    {
        throw new NotImplementedException();
    }
}