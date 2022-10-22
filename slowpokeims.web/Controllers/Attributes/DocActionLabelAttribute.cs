namespace SlowPokeIMS.Web.Controllers.Attributes;


[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DocActionLabelAttribute : System.Attribute
{
    public DocActionLabelAttribute(string label)
    {
        Label = label;
    }

    public string Label { get; }
}
