namespace slowpoke.core.Models.Configuration.Attributes;

[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DefaultAttribute : System.Attribute
{
    // See the attribute guidelines at
    //  http://go.microsoft.com/fwlink/?LinkId=85236
    public object DefaultValue { get; }
    
    // This is a positional argument
    public DefaultAttribute(object defaultValue)
    {
        this.DefaultValue = defaultValue;
    }
}