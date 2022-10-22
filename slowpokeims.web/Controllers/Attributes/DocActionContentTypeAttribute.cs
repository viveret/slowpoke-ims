using System;
using System.Linq;

namespace SlowPokeIMS.Web.Controllers.Attributes;


[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DocActionContentTypeAttribute : System.Attribute
{
    public DocActionContentTypeAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes;
    }

    public string[] ContentTypes { get; }

    public bool IsContentTypeMatch(string contentType)
    {
        return ContentTypes == null || ContentTypes.Length == 0 || ContentTypes.Contains(contentType);
    }
}
