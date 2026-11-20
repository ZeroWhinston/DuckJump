using System.Text;

namespace VpnClient.Subscriptions;

internal static class Base64Text
{
    public static bool TryDecode(string value, out string decoded)
    {
        decoded = string.Empty;
        var normalized = value.Trim().Replace('-', '+').Replace('_', '/');
        normalized = normalized.PadRight(normalized.Length + ((4 - normalized.Length % 4) % 4), '=');

        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
