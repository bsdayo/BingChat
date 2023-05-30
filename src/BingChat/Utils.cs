namespace BingChat;

internal static class Utils
{
    public static string GenerateRandomHexString()
    {
        var bytes = (stackalloc byte[16]);
        Random.Shared.NextBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}