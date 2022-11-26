using System;
using System.Linq;
using System.Threading.Tasks;
using slowpoke.core.Models.Node.Docs;

namespace SlowPokeIMS.Web.Controllers.Attributes;


[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DocActionVisibilityAttribute : System.Attribute
{
    public DocActionVisibilityAttribute(Type visibleFnParentType, string visibleFnName)
    {
        var sameNameMethods = visibleFnParentType.GetMethods().Where(m => m.Name == visibleFnName);
        var visibleFnMethodInfo = sameNameMethods.Where(m => m.ReturnType.IsAssignableFrom(typeof(bool))).SingleOrDefault();
        if (visibleFnMethodInfo != null)
        {
            VisibleFn = (node, services) => (bool) visibleFnMethodInfo.Invoke(null, new object[] { node, services });
        }

        var visibleFnAsyncMethodInfo = sameNameMethods.Where(m => m.ReturnType.IsAssignableFrom(typeof(Task<bool>))).SingleOrDefault();
        if (visibleFnAsyncMethodInfo != null)
        {
            VisibleFnAsync = (node, services) => (Task<bool>) visibleFnAsyncMethodInfo.Invoke(null, new object[] { node, services });
        }
    }

    public bool InlineAction { get; }
    public Func<IReadOnlyNode, IServiceProvider, bool> VisibleFn { get; }
    public Func<IReadOnlyNode, IServiceProvider, Task<bool>> VisibleFnAsync { get; }
}
