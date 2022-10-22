using System.Text;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Node;


public class UserSpecialFoldersProvider : IUserSpecialFoldersProvider
{
    public IDocumentProviderResolver DocumentProviderResolver { get; }
    
    public Config Config { get; }
    
    public UserSpecialFoldersProvider(
        IDocumentProviderResolver documentProviderResolver,
        Config config)
    {
        DocumentProviderResolver = documentProviderResolver;
        Config = config;
    }

    // public IEnumerable<INodePath> Favorites
    // {
    //     get
    //     {
    //         var ct = CancellationToken.None;
    //         var rl = DocumentProviderResolver.ReadLocal;
    //         var favoritesPath = "~/.sp-favorites.csv".AsIDocPath(Config).ConvertToAbsolutePath();
    //         if (rl.NodeExistsAtPath(favoritesPath, ct))
    //         {
    //             return (rl.GetNodeAtPath(favoritesPath, ct) as IReadOnlyDocument)!
    //                         .ReadAllText(Encoding.ASCII).Split("\n")
    //                         .Where(line => !string.IsNullOrWhiteSpace(line))
    //                         .Select(path => path.AsIDocPath(Config));
    //         }
    //         else
    //         {
    //             return Enumerable.Empty<INodePath>();
    //         }
    //     }
    //     set
    //     {
    //         var ct = CancellationToken.None;
    //         var rwl = DocumentProviderResolver.ReadWriteLocal;
    //         var favoritesPath = "~/.sp-favorites.csv".AsIDocPath(Config).ConvertToAbsolutePath();

    //         var doc = rwl.GetNodeAtPath(favoritesPath, ct) as IWritableDocument;
    //         doc ??= rwl.NewDocument(new NewDocumentOptions { fileName = favoritesPath.PathValue, contentType = "text/csv" }, ct);

    //         doc.WriteIfChanged(stream =>
    //         {
    //             foreach (var line in value)
    //             {
    //                 stream.Write(System.Text.Encoding.ASCII.GetBytes(line.PathValue));
    //                 stream.WriteByte((byte)'\n');
    //             }
    //         }, ct);
    //     }
    // }

    // public void AddFavorite(INodePath node)
    // {
    //     var favorites = Favorites.ToList();
    //     if (!favorites.Contains(node))
    //     {
    //         favorites.Add(node);
    //         Favorites = favorites;
    //     }
    // }

    // public void RemoveFavorite(INodePath node)
    // {
    //     var favorites = Favorites.ToList();
    //     if (favorites.Contains(node))
    //     {
    //         Favorites = favorites.Where(f => f.Equals(node));
    //     }
    // }
}