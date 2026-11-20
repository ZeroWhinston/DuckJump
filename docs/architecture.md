# Architecture

```text
WinUI / MAUI applications
          |
          v
Connection orchestration
          |
          +-- WireGuard engine
          +-- OpenVPN engine
          +-- Xray engine
          |
          v
Privileged platform service

Subscription sources
  +-- one share link
  +-- newline-separated links
  +-- Base64 subscription body
  +-- HTTPS subscription endpoint
          |
          v
Normalized ConnectionProfile records
```

Only one engine may own the system routes in the first release. Switching is
transactional: stop the active engine, restore network state, then start the next
engine. A failed start must leave the machine disconnected rather than silently
bypass an enabled kill switch.

The shared core targets .NET 10. Windows UI, Android UI and future iOS UI consume
the same normalized profiles but use platform-specific privileged tunnel hosts.
