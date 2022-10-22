using System;
using System.Linq;
using slowpoke.core.Models.Node.Docs;

namespace SlowPokeIMS.Web.Controllers.Attributes;


[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DocActionVisibilityAttribute : System.Attribute
{
    public DocActionVisibilityAttribute(Type visibleFnParentType, string visibleFnName)
    {
        var visibleFnMethodInfo = visibleFnParentType.GetMethods().Where(m => m.Name == visibleFnName).Single();
        VisibleFn = (node, services) => (bool) visibleFnMethodInfo.Invoke(null, new object[] { node, services });
    }

    public bool InlineAction { get; }
    public Func<IReadOnlyNode, IServiceProvider, bool> VisibleFn { get; }
}
