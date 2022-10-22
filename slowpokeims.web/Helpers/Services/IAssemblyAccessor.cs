using System.Reflection;

namespace SlowPokeIMS.Web.Helpers.Services;


public interface IAssemblyAccessor
{
    Assembly Value { get; }
}

public class AssemblyAccessor : IAssemblyAccessor
{
    public Assembly Value => System.Reflection.Assembly.GetExecutingAssembly();
}