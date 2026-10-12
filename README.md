# DofusSwitch-It

Utilidad de escritorio nativa para Windows que permite a jugadores multicuenta de **Dofus**
cambiar de forma ultrarrápida y cíclica entre las ventanas abiertas del juego mediante
**hotkeys globales** (Siguiente / Anterior), sin pasar por `Alt+Tab` ni por otras aplicaciones.

- Stack: **.NET 10 · C# 14 · WPF · xUnit + FluentAssertions**
- Win32 nativo (`user32.dll`): `EnumWindows`, `RegisterHotKey` / `UnregisterHotKey`, `SetForegroundWindow`
- Metodología: **Spec-Driven Development** — cada feature vive en `.spec/` y en su rama `feature/<id>-<slug>` (ver `AGENT.md`)

## Estructura

```
AGENT.md                  # Directrices obligatorias del agente (stack, Win32, SDD, tests)
.spec/
  index.md                # Índice y estado de especificaciones
  spec-template.md        # Plantilla SDD para nuevas features
  001-window-detection-and-switching.md   # Core Loop: detección + cambio cíclico
DofusSwitchIt.slnx        # Solución (formato .slnx, SDK .NET 10)
src/DofusSwitchIt/        # App WPF (net10.0-windows)
  Core/                   # Lógica pura testeable: GameWindow, WindowCycleService, interfaces
  Native/                 # P/Invoke: NativeMethods, Win32WindowEnumerator, Win32ForegroundService, HotKeyRegistrar
tests/DofusSwitchIt.Tests/  # Tests xUnit (net10.0-windows, sin APIs WPF)
```

## Requisitos

- Windows 10/11 x64
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.2xx o superior)

## Compilar y ejecutar

```powershell
dotnet build DofusSwitchIt.slnx
dotnet run --project src/DofusSwitchIt/DofusSwitchIt.csproj
```

O abrir `DofusSwitchIt.slnx` en Visual Studio 2026+ y pulsar `F5`.

## Ejecutar pruebas

```powershell
dotnet test DofusSwitchIt.slnx
```

Incluye tests del ciclo siguiente/anterior circular, no-op sin ventanas y refresco
de la lista (SPEC-001, criterios CA-1/CA-2/CA-3/CA-5).

## Flujo de ramas Git por especificación (obligatorio)

1. Cada spec en `.spec/` tiene su rama: `feature/<id>-<slug>`
   (ej. `.spec/001-window-detection-and-switching.md` → `feature/001-window-detection`).
2. Todo el código y tests de esa spec se desarrollan **solo** en su rama. Prohibido commitear a `main` directamente o mezclar specs.
3. Al finalizar: verificar en verde, actualizar el estado en `.spec/index.md`, y subir:
   ```powershell
   git checkout feature/001-window-detection
   dotnet build DofusSwitchIt.slnx
   dotnet test DofusSwitchIt.slnx
   git push -u origin feature/001-window-detection
   ```
4. Abrir PR hacia `main` referenciando la spec. Convención de commits: `<id>: <descripción>` (ej. `001: add cyclic window switcher service + tests`).
