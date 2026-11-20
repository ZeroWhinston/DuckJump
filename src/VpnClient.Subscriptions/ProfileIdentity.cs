using System.Security.Cryptography;
using System.Text;

namespace VpnClient.Subscriptions;

internal static class ProfileIdentity
{
    public static string FromShareLink(string shareLink)
    {
        var fragmentIndex = shareLink.IndexOf('#', StringComparison.Ordinal);
        var identityInput = fragmentIndex >= 0 ? shareLink[..fragmentIndex] : shareLink;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identityInput.Trim()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
