using slowpoke.core.Models.Node.Docs;

namespace SlowPokeIMS.Web.ViewModels;


public class QueryDocumentsResult
{
    public PaginatedEnumerableViewModel<IReadOnlyNodeViewModel> Documents { get; set; }
    
    public QueryDocumentOptions q { get; set; }
}