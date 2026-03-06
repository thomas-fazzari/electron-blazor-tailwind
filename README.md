# Electron-Blazor-Tailwind

Personal template to build desktop apps with Electron.NET, Blazor, Tailwind v4 and Fluxor

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Bun](https://bun.sh/) (recommended) or [Node.js](https://nodejs.org/)
- [Electron.NET CLI](https://github.com/ElectronNET/Electron.NET): `dotnet tool install ElectronNET.CLI -g`

## Run

```bash
# First time only
make install

# Start as Electron app
make run

# Start as a regular web app
make run-web
```

## Build & Test

```bash
make build
make test
```

## Stack

| Layer                  | Tech                                      |
| ---------------------- | ----------------------------------------- |
| Desktop shell          | Electron.NET                              |
| UI framework & Styling | Blazor (Interactive Server) + Tailwind v4 |
| State management       | Fluxor                                    |
| Logging                | Serilog (file sink, daily rolling)        |
| Testing                | xUnit, bUnit, NSubstitute                 |

## Architecture

- **Vertical slices**: each feature owns its pages, state, and persistence
- **Fluxor (Redux)**: unidirectional data flow with actions, reducers, and effects
- **Options pattern**: validated configuration with `IValidateOptions` and `ValidateOnStart`
- **Desktop abstraction**: Electron APIs behind `IDesktopBridge` for testability; window bounds persisted across sessions
- **Logging**: Serilog file sink under `%LocalAppData%/{app}/logs/`, daily rolling, 7-file retention
- **Custom titlebar**: OS-aware draggable and customizable titlebar
- **Security**: CSP headers, external link allowlist, Electron sandbox enabled
- **Testing**: built-in unit (xUnit v3), component (bUnit), and integration (WebApplicationFactory)
- **Hybrid mode**: runs as a web app or desktop app (uncomment the guard in `Program.cs` to restrict to desktop only)
