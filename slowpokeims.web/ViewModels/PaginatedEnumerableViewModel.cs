using System;
using slowpoke.core.Models;

namespace SlowPokeIMS.Web.ViewModels;


public class PaginatedEnumerableViewModel<T>: PaginatedEnumerable<T>, IPaginatedEnumerableViewModel
{

    public Func<int, string> PageToUrl { get; set; }

    public Func<ItemViewType, string> ItemViewTypeToUrl { get; set; }
    
    public Func<IReadOnlyNodeViewModel, string> TitleUrlResolver { get; set; }
}