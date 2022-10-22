using System.Collections;

namespace slowpoke.core.Models;

public interface IPaginatedEnumerable
{
    IEnumerable Items { get; set; }
    
    int Offset { get; set; }
    
    int PageSize { get; set; }
    
    int Total { get; set; }
}