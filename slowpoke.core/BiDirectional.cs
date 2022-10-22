using System.Collections;

namespace slowpoke.core;


public class BiDirectional<TForward, TBackward, TContainerForward, TContainerBackward>
where TContainerForward: IDictionary<TForward, TBackward>, new()
where TContainerBackward: IDictionary<TBackward, TForward>, new()
{
    public TContainerForward Forward { get; set; } = new();
    
    public TContainerBackward Backward { get; set; } = new();
}