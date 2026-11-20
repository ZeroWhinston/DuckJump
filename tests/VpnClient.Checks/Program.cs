using System.Text;
using System.Text.Json;
using VpnClient.Domain;
using VpnClient.Subscriptions;

var checks = new List<(string Name, Action Run)>
{
    ("VLESS share link", ParseVless),
    ("VMess share link", ParseVmess),
    ("Base64 subscription", ParseBase64Subscription),
    ("Duplicate suppression", SuppressDuplicates)
};

var failures = 0;
foreach (var check in checks)
{
    try
    {
        check.Run();
        Console.WriteLine($"PASS {check.Name}");
    }
    catch (Exception exception)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {check.Name}: {exception.Message}");
    }
}

return failures == 0 ? 0 : 1;

static void ParseVless()
{
    const string link = "vless://11111111-1111-1111-1111-111111111111@example.com:443?security=reality&type=tcp#Primary";
    Ensure(ShareLinkParser.TryParse(link, out var profile, out var error), error);
    Ensure(profile!.Protocol == VpnProtocol.Vless, "Wrong protocol.");
    Ensure(profile.Name == "Primary", "Wrong display name.");
    Ensure(profile.Parameters["security"] == "reality", "Security parameter was lost.");
}

static void ParseVmess()
{
    var json = JsonSerializer.Serialize(new
    {
        v = "2",
        ps = "VMess node",
        add = "vmess.example.com",
        port = "8443",
        id = "22222222-2222-2222-2222-222222222222",
        net = "ws",
        tls = "tls"
    });
    var link = "vmess://" + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    Ensure(ShareLinkParser.TryParse(link, out var profile, out var error), error);
    Ensure(profile!.Protocol == VpnProtocol.Vmess, "Wrong protocol.");
    Ensure(profile.Port == 8443, "Wrong port.");
}

static void ParseBase64Subscription()
{
    var links = string.Join('\n',
        "vless://aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa@one.example:443#One",
        "trojan://secret@two.example:443?security=tls#Two");
    var body = Convert.ToBase64String(Encoding.UTF8.GetBytes(links));
    var snapshot = SubscriptionParser.Parse(body);
    Ensure(snapshot.Profiles.Count == 2, "Expected two profiles.");
}

static void SuppressDuplicates()
{
    const string link = "vless://bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb@example.com:443#Same";
    var snapshot = SubscriptionParser.Parse(link + "\n" + link);
    Ensure(snapshot.Profiles.Count == 1, "Duplicate profile was not suppressed.");
}

static void Ensure(bool condition, string? message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message ?? "Check failed.");
    }
}
