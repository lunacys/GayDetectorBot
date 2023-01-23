namespace GayDetectorBot.WebApi;

public static class Utils
{
    private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";

    public static string GenerateRandomString(int length = 16)
    {
        var stringChars = new char[length];
        var random = new Random();

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = _chars[random.Next(_chars.Length)];
        }

        return new string(stringChars);
    }
}