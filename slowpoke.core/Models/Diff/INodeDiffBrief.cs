namespace slowpoke.core.Models.Diff;


public interface INodeDiffBrief
{
    public bool HasChanged { get; }
    
    public bool HasMetaChanged { get; }
    
    public bool HasContentChanged { get; }
    
    public bool HasPathChanged { get; }
    
    INodeFingerprint Old { get; }
    
    INodeFingerprint New { get; }
}