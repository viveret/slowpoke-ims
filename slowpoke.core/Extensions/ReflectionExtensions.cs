using System.Reflection;

namespace slowpoke.core.Extensions;


public static class ReflectionExtensions
{
    public static async Task<object?> PossiblyAwaitGetPropertyValue(this PropertyInfo property, object obj)
    {
        var v = property.GetValue(obj);

        var isTask = property.PropertyType.IsAssignableTo(typeof(Task));
        return isTask && v != null ? await (((Task) v).ContinueWith((t) => { return (object) (((dynamic)t).Result); })) : v;
    }
}