using System.Collections.Generic;
using slowpoke.core.Services;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Web.ViewModels.System;



public class DocumentProvidersViewModel
{
    public IEnumerable<IReadOnlyDocumentResolver> ReadOnly { get; set; }
    
    public IEnumerable<IWritableDocumentResolver> Readwrite { get; set; }
}