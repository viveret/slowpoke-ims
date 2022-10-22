namespace slowpoke.core.Models.Node.Docs;

public class QueryDocumentOptions
{  
    public static FileCategory[] CategoriesToIncludeOnlyDefault = new FileCategory[] { FileCategory.Regular };

    public QueryDocumentOptions()
    {
    }
    
    public QueryDocumentOptions(QueryDocumentOptions other)
    {
        Query = other.Query;
        SearchDocContents = other.SearchDocContents;
        Recursive = other.Recursive;
        SyncEnabled = other.SyncEnabled;
        IsInFavorites = other.IsInFavorites;
        IncludeHidden = other.IncludeHidden;
        IncludeFolders = other.IncludeFolders;
        Folder = other.Folder;
        Path = other.Path;
        Name = other.Name;
        Extension = other.Extension;
        ExtensionsToIgnore = other.ExtensionsToIgnore;
        ContentType = other.ContentType;
        CreationDateMin = other.CreationDateMin;
        CreationDateMax = other.CreationDateMax;
        LastUpdateMin = other.LastUpdateMin;
        LastUpdateMax = other.LastUpdateMax;
        CategoriesToIncludeOnly = other.CategoriesToIncludeOnly;
        CategoriesToExclude = other.CategoriesToExclude;
        Offset = other.Offset;
        PageSize = other.PageSize;
        OrderByColumn = other.OrderByColumn;
        OrderByAscending = other.OrderByAscending;
        ItemView = other.ItemView;
    }

    public string Query { get; set; }

    public bool SearchDocContents { get; set; }

    public bool Recursive { get; set; } = true;
    
    public bool IncludeHidden { get; set; }

    public bool IncludeFolders { get; set; }
    
    public INodePath Folder { get; set; }

    // this is not able to handle string input binding, so need to write/add converter or new properties to do type conversions
    public INodePath Path { get; set; }
    
    public string Name { get; set; }
    
    public string Extension { get; set; }

    public string ExtensionsToIgnore { get; set; }
    
    public string ContentType { get; set; }
    
    public DateTime? CreationDateMin { get; set; }
    
    public DateTime? CreationDateMax { get; set; }
    
    public DateTime? LastUpdateMin { get; set; }

    public DateTime? LastUpdateMax { get; set; }
    
    public DateTime? LastAccessDateMin { get; set; }

    public DateTime? LastAccessDateMax { get; set; }
    
    public DateTime? LastSyncDateMin { get; set; }

    public DateTime? LastSyncDateMax { get; set; }

    public FileCategory[] CategoriesToIncludeOnly { get; set; } = CategoriesToIncludeOnlyDefault;
    
    public FileCategory[] CategoriesToExclude { get; set; }

    public bool? UseProjectDetection { get; set; } // if project folders / files should be grouped together 

    public int Offset { get; set; }

    public int PageSize { get; set; } = 10;
    
    public string OrderByColumn { get; set; }
    
    public bool OrderByAscending { get; set; }

    public ItemViewType? ItemView { get; set; }
    
    public bool? SyncEnabled { get; set; }

    public bool? IsInFavorites { get; set; }

    public QueryDocumentOptions CopyButChangePage(int pg)
    {
        return new QueryDocumentOptions(this) { Offset = (pg - 1) * PageSize };
    }

    public QueryDocumentOptions CopyButChangeViewType(ItemViewType vt)
    {
        return new QueryDocumentOptions(this) { ItemView = vt };
    }

    public QueryDocumentOptions CopyButChangeFolder(INodePath path)
    {
        return new QueryDocumentOptions(this) { Path = path };
    }
}