using VpnClient.Domain;

namespace VpnClient.Engine.Abstractions;

public interface IVpnEngine
{
    string Name { get; }
    IReadOnlySet<VpnProtocol> SupportedProtocols { get; }
    VpnConnectionState State { get; }

    Task StartAsync(ConnectionProfile profile, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public enum VpnConnectionState
{
    Stopped,
    Starting,
    Connected,
    Stopping,
    Faulted
}
