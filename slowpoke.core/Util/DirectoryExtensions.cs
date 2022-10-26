namespace slowpoke.core.Util;


public static class DirectoryExtensions
{
    public static IEnumerable<string> EnumerateSafe(
        bool includeFolders,
        //this Func<string, string, EnumerationOptions, IEnumerable<string>> enumerateFileSystemEntries,
        string path, string ext, EnumerationOptions enumOptions,
        HashSet<string> parentPathsTraversed = null)
    {
        parentPathsTraversed ??= new HashSet<string>();
        var hasExt = !string.IsNullOrWhiteSpace(ext);

        var enumOptionsSingleRecursion = new EnumerationOptions
        {
            AttributesToSkip = enumOptions.AttributesToSkip,
            BufferSize = enumOptions.BufferSize,
            IgnoreInaccessible = enumOptions.IgnoreInaccessible,
            MatchCasing = enumOptions.MatchCasing,
            MatchType = enumOptions.MatchType,
            ReturnSpecialDirectories = enumOptions.ReturnSpecialDirectories,
            
            RecurseSubdirectories = false,
            MaxRecursionDepth = 1,
        };
        var entries = Directory.EnumerateFileSystemEntries(path, hasExt ? ext : "*", enumOptionsSingleRecursion);
        foreach (var p in entries)
        {
            if (System.IO.Directory.Exists(p))
            {
                var dirInfo = new DirectoryInfo(p);
                var fullPath = System.IO.Path.GetFullPath(dirInfo.LinkTarget ?? p);
                if (!parentPathsTraversed.Add(fullPath))
                {
                    continue;
                }

                if (includeFolders)
                {
                    yield return p;
                }
                if (enumOptions.RecurseSubdirectories)
                {
                    foreach (var p2 in EnumerateSafe(includeFolders, p, ext, enumOptions, parentPathsTraversed))
                    {
                        yield return p2;
                    }
                }
            }
            else
            {
                yield return p;
            }
        }
    }

    public static IEnumerable<string> EnumerateEntriesSafe(string path, string ext, EnumerationOptions enumOptions)
    {
        return EnumerateSafe(true, path, ext, enumOptions);
    }

    public static IEnumerable<string> EnumerateFilesSafe(string path, string ext, EnumerationOptions enumOptions)
    {
        return EnumerateSafe(false, path, ext, enumOptions);
    }
}