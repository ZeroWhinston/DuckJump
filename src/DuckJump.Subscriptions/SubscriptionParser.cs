using DuckJump.Domain;

namespace DuckJump.Subscriptions;

public static class SubscriptionParser
{
    public static SubscriptionSnapshot Parse(string body, DateTimeOffset? loadedAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        var content = body.Trim().TrimStart('\uFEFF');

        if (!ContainsShareLink(content) &&
            Base64Text.TryDecode(content, out var decoded) &&
            ContainsShareLink(decoded))
        {
            content = decoded;
        }

        var profiles = new List<ConnectionProfile>();
        var warnings = new List<string>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var line in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = line.Trim();
            if (candidate.Length == 0 || candidate.StartsWith('#'))
            {
                continue;
            }

            if (!ShareLinkParser.TryParse(candidate, out var profile, out var error) || profile is null)
            {
                warnings.Add(error ?? "Unknown subscription entry.");
                continue;
            }

            if (seenIds.Add(profile.Id))
            {
                profiles.Add(profile);
            }
        }

        if (profiles.Count == 0)
        {
            throw new FormatException("The subscription contains no supported connection profiles.");
        }

        return new SubscriptionSnapshot(
            profiles.AsReadOnly(),
            warnings.AsReadOnly(),
            loadedAtUtc ?? DateTimeOffset.UtcNow);
    }

    private static bool ContainsShareLink(string value) =>
        value.Contains("vless://", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("vmess://", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("trojan://", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("ss://", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("wireguard://", StringComparison.OrdinalIgnoreCase);
}
