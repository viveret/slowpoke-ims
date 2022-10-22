using System.Linq;

namespace SlowPokeIMS.Web.Controllers.Attributes;


[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DocControllerAttribute : System.Attribute
{
    public DocControllerAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes;
    }

    public string[] ContentTypes { get; }




    public bool IsContentTypeMatch(string contentType)
    {
        return ContentTypes == null || ContentTypes.Length == 0 || ContentTypes.Contains(contentType);
    }
}
