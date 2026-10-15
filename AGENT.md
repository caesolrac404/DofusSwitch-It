# AGENT.md — DofusSwitch-It

> Directrices obligatorias para cualquier agente (humano o IA) que contribuya a este repositorio.
> Metodología: **Spec-Driven Development (SDD)** + ramas Git por especificación.

## 1. Stack Tecnológico

| Capa | Tecnología / Versión |
|------|----------------------|
| Lenguaje | **C# 14** |
| Framework | **.NET 10** (`net10.0-windows`) |
| UI | **WPF** (`UseWPF: true`) |
| Tests unitarios | **xUnit** + `FluentAssertions` |
| OS objetivo | Windows 10/11 x64 (nativo, sin dependencias cross-platform para el core Win32) |
| Nullable / Implícitos | `Nullable: enable`, `ImplicitUsings: enable` activados en todos los proyectos |

## 2. APIs de Windows obligatorias (Win32 nativas vía `user32.dll`)

Toda interacción con ventanas del juego y hotkeys globales **DEBE** usar P/Invoke contra `user32.dll`. Queda prohibido usar paquetes de terceros para esto en el core.

| Objetivo | API | Notas |
|----------|-----|-------|
| Enumerar ventanas | `EnumWindows`, `GetWindowTextW`, `GetWindowTextLengthW`, `IsWindowVisible`, `GetWindowThreadProcessId` | Filtrar por título de proceso Dofus y ventanas visibles no minimizadas según spec |
| Registrar hotkeys globales | `RegisterHotKey`, `UnregisterHotKey` | IDs únicos por acción (Siguiente / Anterior). Manejar `WM_HOTKEY` en el message loop WPF (`HwndSource.AddHook`) |
| Cambiar el foco | `SetForegroundWindow`, `ShowWindow`, `IsIconic`, `GetForegroundWindow` | Si la ventana está minimizada (`IsIconic`), restaurar con `ShowWindow(hWnd, SW_RESTORE)` antes de `SetForegroundWindow` |

Firma de referencia (ubicación canónica: `src/DofusSwitchIt/Native/NativeMethods.cs`):

```csharp
internal static partial class NativeMethods
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetForegroundWindow(nint hWnd);
}
```

- Usar `LibraryImport` (source-generated P/Invoke, .NET 7+) en lugar de `DllImport` clásico.
- Centralizar todas las declaraciones en `Native/NativeMethods.cs`. Prohibido dispersar `DllImport` por el código.

## 3. Flujo de Trabajo Git por Especificación (SDD Rule — OBLIGATORIO)

1. **Una spec = una rama.** Por cada especificación definida en `.spec/` se crea obligatoriamente una rama nueva antes de escribir código:
   ```
   feature/<id>-<slug>
   # Ejemplo: feature/001-window-detection
   ```
   El `<id>` y `<slug>` deben coincidir con el nombre del archivo de spec (ej. `.spec/001-window-detection-and-switching.md` → `feature/001-window-detection` o `feature/001-window-detection-and-switching`).
2. **Todo el código y pruebas de la spec viven en su rama.** Prohibido mezclar dos specs en una misma rama o commitear a `main` directamente.
3. **Al finalizar**, ejecutar el skill **`code-review`** sobre el diff de la rama
   (`review since main`, indicando la spec en `.spec/<id>-<slug>.md`), resolver los hallazgos de
   ambos ejes (Standards y Spec), y luego subir la rama con `git push -u origin <rama>` y abrir
   PR hacia `main` referenciando la spec.
   > Skills disponibles del agente: `code-review` y `clean-code`, en `.agents/skills/`
   > (registradas en `skills-lock.json`). Ver la sección "Skills del agente" del `README.md`.
4. **Estado de la spec** en `.spec/index.md` debe actualizarse en la misma rama (`Borrador → En curso → En revisión → Completada`).
5. **Convención de commits:** `<id>: <descripción corta>` (ej. `001: add cyclic window switcher service + tests`).

## 4. Reglas de código y pruebas

### 4.1 `IDisposable` para recursos Win32
- Toda clase que registre hotkeys (`RegisterHotKey`) o retenga `HWND` / hooks (`HwndSource`) **DEBE** implementar `IDisposable` (o `IAsyncDisposable` si aplica) y liberar con `UnregisterHotKey` / `RemoveHook` en `Dispose()`.
- Usar el patrón `Dispose(bool disposing)` + `GC.SuppressFinalize(this)` cuando haya recursos no gestionados. Nunca depender del finalizador como vía principal.

### 4.2 Separación testeable (lógica sin UI)
- La lógica de negocio — **ciclo de ventanas, ordenamiento, filtrado, selección siguiente/anterior** — vive en servicios puros bajo `src/DofusSwitchIt/Core/` (ej. `WindowCycleService`), sin referencias a `System.Windows`, `HwndSource` ni P/Invoke directo.
- El acceso a Win32 se abstrae tras interfaces (ej. `IWindowEnumerator`, `IForegroundService`, `IHotKeyRegistrar`) inyectadas por constructor. Los tests sustituyen estas interfaces con fakes/stubs.
- **Prohibido acoplar la UI en las pruebas:** los proyectos de test no referencian WPF ni crean ventanas. Solo testean el Core + ViewModels desacoplados.

### 4.3 Estándares C#
- `Nullable: enable` — cero warnings de nulabilidad en `dotnet build`.
- Nombres en inglés para código; comentarios/docs de spec en español o inglés según la spec.
- Formato con `dotnet format` antes de cada push.

## 5. Estructura canónica del repo

```
AGENT.md
README.md
DofusSwitchIt.sln
.spec/
  index.md
  spec-template.md
  001-window-detection-and-switching.md
src/
  DofusSwitchIt/            # WPF (net10.0-windows, UseWPF)
    Core/                   # Lógica pura testeable (WindowCycleService, modelos)
    Native/                 # P/Invoke (NativeMethods, HotKeyRegistrar)
    ViewModels/ / Views/    # UI WPF
tests/
  DofusSwitchIt.Tests/      # xUnit (net10.0, sin referencia WPF)
```

## 6. Comandos de verificación (Definition of Done por spec)

```powershell
dotnet build DofusSwitchIt.sln
dotnet test DofusSwitchIt.sln
```

Ambos deben pasar en verde antes de `git push` y del PR. Si la spec añade hotkeys o foco, incluir al menos un test del ciclo GIVEN/WHEN/THEN correspondiente.
