namespace DuckJump.Domain;

public sealed record SubscriptionSnapshot(
    IReadOnlyList<ConnectionProfile> Profiles,
    IReadOnlyList<string> Warnings,
    DateTimeOffset LoadedAtUtc);
