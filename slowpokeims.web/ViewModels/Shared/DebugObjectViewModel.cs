namespace SlowPokeIMS.Web.ViewModels.Shared;



public class DebugObjectViewModel
{
    public object Value { get; set; }

    public int Depth { get; set; }

    public int MaxDepth { get; set; } = 4;

    public DebugObjectViewModel Parent { get; set; }

    public DebugObjectViewModel Copy(object newValue)
    {
        return new DebugObjectViewModel
        {
            Value = newValue,
            Depth = Depth + 1,
            MaxDepth = MaxDepth,
            Parent = this,
        };
    }
}