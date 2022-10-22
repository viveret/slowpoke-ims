namespace SlowPokeIMS.Web.Controllers.Attributes;


[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DocActionAttribute : System.Attribute
{
    public DocActionAttribute(bool inlineAction = false)
    {
        InlineAction = inlineAction;
    }

    public bool InlineAction { get; }
}
