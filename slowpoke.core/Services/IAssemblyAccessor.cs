using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace SlowPokeIMS.Core.Services;


public interface IAssemblyAccessor
{
    IEnumerable<Assembly> Value { get; }
}

public class AssemblyAccessor : IAssemblyAccessor
{
    private readonly ApplicationPartManager applicationPartManager;

    public AssemblyAccessor(ApplicationPartManager applicationPartManager)
    {
        this.applicationPartManager = applicationPartManager;
    }

    public IEnumerable<Assembly> Value => applicationPartManager.ApplicationParts.OfType<AssemblyPart>().Select(part => part.Assembly);
}