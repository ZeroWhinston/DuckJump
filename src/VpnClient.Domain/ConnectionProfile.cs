using System.Collections.ObjectModel;

namespace VpnClient.Domain;

public sealed record ConnectionProfile
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required VpnProtocol Protocol { get; init; }
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string SecretReference { get; init; }
    public IReadOnlyDictionary<string, string> Parameters { get; init; } =
        ReadOnlyDictionary<string, string>.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new InvalidOperationException("Profile id is required.");
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("Profile host is required.");
        }

        if (Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("Profile port is outside the valid range.");
        }
    }
}
