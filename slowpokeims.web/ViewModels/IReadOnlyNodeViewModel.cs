using slowpoke.core.Models.Node.Docs;

namespace SlowPokeIMS.Web.ViewModels;


public class IReadOnlyNodeViewModel
{
    public IReadOnlyNodeViewModel(IReadOnlyNode model, IPaginatedEnumerableViewModel parent = null)
    {
        Model = model;
        Parent = parent;
    }

    public IReadOnlyNode Model { get; set; }

    public IPaginatedEnumerableViewModel Parent { get; set; }
    
    public IReadOnlyDocument Doc => (IReadOnlyDocument) Model;
    
    public IReadOnlyFolder Folder => (IReadOnlyFolder) Model;
}