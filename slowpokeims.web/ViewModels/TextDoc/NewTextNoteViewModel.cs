using System.Collections.Generic;
using slowpoke.core;
using slowpoke.core.Models;

namespace SlowPokeIMS.Web.ViewModels.TextDoc;


public class NewTextNoteViewModel
{
    public string path { get; set; }

    public string Value { get; set; }
    
    public string ContentType { get; set; }
}