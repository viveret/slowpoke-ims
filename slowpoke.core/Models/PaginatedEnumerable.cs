using System.Collections;

namespace slowpoke.core.Models;



public class PaginatedEnumerable<T>: IPaginatedEnumerable
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    
    public int Offset { get; set; }
    
    public int PageSize { get; set; }
    
    public int Total { get; set; }

    IEnumerable IPaginatedEnumerable.Items { get => Items; set => Items = (IEnumerable<T>) value; }
}