namespace slowpoke.core.Extensions;


public static class StringExtensions
{
    public static string TrimStart(this string str, string substr)
    {
        if (str.StartsWith(substr))
        {
            return str.Substring(substr.Length);
        }
        return str;
    }

    public static string TrimEnd(this string str, string substr)
    {
        if (str.EndsWith(substr))
        {
            return str.Substring(0, str.Length - substr.Length);
        }
        return str;
    }
}