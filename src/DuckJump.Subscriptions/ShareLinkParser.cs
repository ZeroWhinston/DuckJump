using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using DuckJump.Domain;

namespace DuckJump.Subscriptions;

public static class ShareLinkParser
{
    private static readonly IReadOnlyDictionary<string, VpnProtocol> Protocols =
        new ReadOnlyDictionary<string, VpnProtocol>(
            new Dictionary<string, VpnProtocol>(StringComparer.OrdinalIgnoreCase)
            {
                ["vless"] = VpnProtocol.Vless,
                ["vmess"] = VpnProtocol.Vmess,
                ["trojan"] = VpnProtocol.Trojan,
                ["ss"] = VpnProtocol.Shadowsocks,
                ["wireguard"] = VpnProtocol.WireGuard
            });

    public static bool TryParse(string shareLink, out ConnectionProfile? profile, out string? error)
    {
        profile = null;
        error = null;
        var value = shareLink.Trim();
        var schemeEnd = value.IndexOf("://", StringComparison.Ordinal);

        if (schemeEnd <= 0)
        {
            error = "The value is not a supported share link.";
            return false;
        }

        var scheme = value[..schemeEnd];
        if (!Protocols.TryGetValue(scheme, out var protocol))
        {
            error = $"Unsupported share-link scheme: {scheme}.";
            return false;
        }

        try
        {
            profile = protocol == VpnProtocol.Vmess
                ? ParseVmess(value)
                : ParseUriProfile(value, protocol);
            profile.Validate();
            return true;
        }
        catch (Exception exception) when (
            exception is FormatException or JsonException or UriFormatException or InvalidOperationException)
        {
            error = exception.Message;
            return false;
        }
    }

    private static ConnectionProfile ParseUriProfile(string shareLink, VpnProtocol protocol)
    {
        var uri = new Uri(shareLink, UriKind.Absolute);
        var parameters = ParseQuery(uri.Query);
        var secret = Uri.UnescapeDataString(uri.UserInfo.Split(':', 2)[0]);
        var name = string.IsNullOrWhiteSpace(uri.Fragment)
            ? $"{protocol} {uri.Host}"
            : Uri.UnescapeDataString(uri.Fragment.TrimStart('#'));
        var port = uri.IsDefaultPort ? DefaultPort(protocol) : uri.Port;

        return new ConnectionProfile
        {
            Id = ProfileIdentity.FromShareLink(shareLink),
            Name = name,
            Protocol = protocol,
            Host = uri.Host,
            Port = port,
            SecretReference = secret,
            Parameters = new ReadOnlyDictionary<string, string>(parameters)
        };
    }

    private static ConnectionProfile ParseVmess(string shareLink)
    {
        var encoded = shareLink[(shareLink.IndexOf("://", StringComparison.Ordinal) + 3)..];
        if (!Base64Text.TryDecode(encoded, out var json))
        {
            throw new FormatException("VMess payload is not valid Base64.");
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var host = RequiredString(root, "add");
        var portText = RequiredString(root, "port");
        if (!int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out var port))
        {
            throw new FormatException("VMess port is invalid.");
        }

        var id = RequiredString(root, "id");
        var name = OptionalString(root, "ps") ?? $"VMess {host}";
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in new[] { "net", "type", "host", "path", "tls", "sni", "alpn", "fp", "scy" })
        {
            var value = OptionalString(root, property);
            if (!string.IsNullOrWhiteSpace(value))
            {
                parameters[property] = value;
            }
        }

        return new ConnectionProfile
        {
            Id = ProfileIdentity.FromShareLink(shareLink),
            Name = name,
            Protocol = VpnProtocol.Vmess,
            Host = host,
            Port = port,
            SecretReference = id,
            Parameters = new ReadOnlyDictionary<string, string>(parameters)
        };
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }

    private static string RequiredString(JsonElement root, string property)
    {
        return OptionalString(root, property)
            ?? throw new FormatException($"VMess property '{property}' is required.");
    }

    private static string? OptionalString(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static int DefaultPort(VpnProtocol protocol) => protocol switch
    {
        VpnProtocol.Shadowsocks => 8388,
        _ => 443
    };
}
