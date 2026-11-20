# DuckJump

Working repository for a Windows-first VPN client with a shared mobile-ready core.

The first milestone supports normalized subscription parsing for VLESS, VMess,
Trojan, Shadowsocks and WireGuard share links. Native tunnel engines and the UI
will be integrated behind stable abstractions in later milestones.

## Build

```powershell
dotnet build DuckJump.slnx
dotnet run --project tests/DuckJump.Checks
```

## Security rules

- Subscription URLs and share links are credentials and must never be logged.
- Imported data is parsed into an allow-listed model; remote executable config is
  never run directly.
- Privileged tunnel operations will live in a small Windows service, separate from
  the unprivileged UI.
- Release binaries and update manifests will be signed and verified.
