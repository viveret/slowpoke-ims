using System;
using slowpoke.core.Models;

namespace SlowPokeIMS.Web.ViewModels;


public interface IPaginatedEnumerableViewModel: IPaginatedEnumerable
{

    Func<int, string> PageToUrl { get; set; }

    Func<ItemViewType, string> ItemViewTypeToUrl { get; set; }

    Func<IReadOnlyNodeViewModel, string> TitleUrlResolver { get; set; }
}