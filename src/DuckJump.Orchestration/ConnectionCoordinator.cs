using DuckJump.Domain;
using DuckJump.Engine.Abstractions;

namespace DuckJump.Orchestration;

public sealed class ConnectionCoordinator
{
    private readonly IReadOnlyList<IVpnEngine> _engines;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IVpnEngine? _activeEngine;

    public ConnectionCoordinator(IEnumerable<IVpnEngine> engines)
    {
        _engines = engines.ToArray();
    }

    public async Task ConnectAsync(ConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        profile.Validate();
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var target = _engines.FirstOrDefault(engine =>
                engine.SupportedProtocols.Contains(profile.Protocol));

            if (target is null)
            {
                throw new NotSupportedException($"No engine supports {profile.Protocol}.");
            }

            if (_activeEngine is not null)
            {
                await _activeEngine.StopAsync(cancellationToken).ConfigureAwait(false);
                _activeEngine = null;
            }

            await target.StartAsync(profile, cancellationToken).ConfigureAwait(false);
            _activeEngine = target;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_activeEngine is null)
            {
                return;
            }

            await _activeEngine.StopAsync(cancellationToken).ConfigureAwait(false);
            _activeEngine = null;
        }
        finally
        {
            _gate.Release();
        }
    }
}
