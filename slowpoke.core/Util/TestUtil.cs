namespace SlowPokeIMS.Core.Util;


public class TestUtil
{
    public static Random _rng = new Random();

    // https://stackoverflow.com/a/1344258/11765486
    public static string GenerateRandomString()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[8];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[_rng.Next(chars.Length)];
        }

        return new String(stringChars);
    }
}