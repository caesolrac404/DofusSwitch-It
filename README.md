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
  index.md                # Índice y estado de especificaciones (001–012)
  spec-template.md        # Plantilla SDD para nuevas features
  001..012-*.md           # Una spec por funcionalidad (ver index.md)
.agents/skills/           # Skills de agente instaladas (code-review, clean-code)
skills-lock.json          # Versiones/registro de las skills instaladas
DofusSwitchIt.slnx        # Solución (formato .slnx, SDK .NET 10)
src/DofusSwitchIt/        # App WPF (net10.0-windows)
  Core/                   # Lógica pura testeable (WindowCycleService, stores, modelos)
  Native/                 # P/Invoke: NativeMethods, Win32WindowEnumerator/Foreground, HotKeyRegistrar
  Logo/                   # DofusSwitch-It.ico (icono de app)
  MainWindow / HelpWindow # UI del widget y ventana de ayuda
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

Incluye tests del ciclo siguiente/anterior circular, no-op sin ventanas, orden estable,
reordenamiento por arrastre, persistencia de posición y del predicado de detección.

## Skills del agente

El repo incluye skills de agente gestionadas por la CLI `skills` y registradas en
`skills-lock.json` (carpeta `.agents/skills/`):

| Skill | Para qué sirve |
|-------|----------------|
| `code-review` | Revisa el diff de una rama en dos ejes — **Standards** (¿respeta las normas del repo?) y **Spec** (¿implementa lo que pedía la spec?) — con dos sub-agentes en paralelo, y reporta ambos lados por separado. |
| `clean-code` | Principios de Clean Code (Martin): nombres, funciones pequeñas, comentarios útiles, smells. |

### Cómo usar `code-review`

Se invoca en el chat pasando el punto fijo (commit, rama, tag o `HEAD~N`) desde el que comparar:

```
review since main
review since feature/010-detection-predicate
```

La skill ejecuta `git diff <punto-fijo>...HEAD` y lanza dos sub-agentes: uno valida el
**Standards** (contra `AGENT.md` y el baseline de smells de Fowler) y otro el **Spec**.
> Nota: la spec origen de este proyecto vive en **`.spec/`** (no en `docs/`, `specs/` ni
> `.scratch/`); indica la spec concreta si la skill pide dónde encontrarla (ej. `.spec/012-...md`).

Conclusión esperada: ambas listas de hallazgos lado a lado, sin mezclarlas, más un resumen
de una línea por eje.

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
5. **Antes del `push`/PR**, ejecutar el skill `code-review` contra el punto fijo de la rama
   (ej. `review since main` apuntando a `.spec/<id>-<slug>.md`) y resolver los hallazgos del eje
   Standards y del eje Spec (ver [Skills del agente](#skills-del-agente)).
