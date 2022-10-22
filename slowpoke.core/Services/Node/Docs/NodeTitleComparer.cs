namespace slowpoke.core.Services
{
    public class NodeTitleComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
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
                return x.CompareTo(y);
            }
        }
    }
}