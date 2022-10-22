using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services
{
    public class NodePathComparer : IComparer<INodePath>
    {
        public int Compare(INodePath? x, INodePath? y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                     return -1;
                }
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                return x.PathValue.CompareTo(y.PathValue);
            }
        }
    }

    public class NodePathFoldersFirstComparer : IComparer<INodePath>
    {
        public int Compare(INodePath? x, INodePath? y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                     return -1;
                }
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                var pathCompared = x.PathValue.CompareTo(y.PathValue);
                if (x.IsFolder)
                {
                    if (y.IsFolder)
                    {
                        return pathCompared;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (y.IsFolder)
                {
                    return 1;
                }
                else
                {
                    return pathCompared;
                }
            }
        }
    }
}