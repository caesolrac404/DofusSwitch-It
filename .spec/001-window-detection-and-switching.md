# SPEC-001: Detección de ventanas de Dofus y cambio cíclico (Core Loop)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `001` |
| **Título** | Detección de ventanas de Dofus y cambio cíclico (Core Loop) |
| **Rama Git** | `feature/001-window-detection` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-12` |
| **Dependencias** | Ninguna (spec fundacional) |

## 2. Contexto

Los jugadores multicuenta de Dofus mantienen N clientes abiertos simultáneamente. Alternar con `Alt+Tab` es lento y mezcla ventanas ajenas al juego (navegador, Discord, etc.). Se necesita un loop cerrado: detectar **solo** las ventanas del juego Dofus y rotar el foco entre ellas con dos hotkeys globales (**Siguiente** / **Anterior**), de forma ultrarrápida (< 100 ms percibidos) y sin interferir con otras aplicaciones.

## 3. User Stories

- **US-1:** Como jugador multicuenta, quiero pulsar un hotkey global para llevar al frente la siguiente ventana de Dofus, para rotar entre mis cuentas sin usar el ratón ni `Alt+Tab`.
- **US-2:** Como jugador multicuenta, quiero pulsar un hotkey global para volver a la ventana anterior de Dofus, para corregir sobre-rotaciones sin recorrer todo el ciclo.
- **US-3:** Como jugador, quiero que el ciclo ignore ventanas de otras aplicaciones, para no perder el foco en medio de un combate.

## 4. Alcance

### Dentro del alcance (In)
- Enumeración de ventanas top-level visibles del proceso/clase de Dofus vía `EnumWindows` + `GetWindowTextW` + `IsWindowVisible` + `GetWindowThreadProcessId`.
- Modelo `GameWindow { HWND, Title, ProcessId }` y servicio puro `WindowCycleService` (orden estable, índice circular, siguiente/anterior).
- Cambio de foco con `IsIconic` → `ShowWindow(SW_RESTORE)` → `SetForegroundWindow`.
- Registro de dos hotkeys globales (`RegisterHotKey` / `UnregisterHotKey`, `WM_HOTKEY`) con `IDisposable`.
- Tests unitarios xUnit del ciclo (sin UI, con `IWindowEnumerator` fake).

### Fuera del alcance (Out — No hacer)
- UI de configuración de hotkeys personalizados (spec futura `002`).
- Overlay, contador de cuentas, persistencia de settings.
- Soporte para Dofus Retro vs. Dofus 2.x como procesos distintos (se unifica por coincidencia de título/proceso configurable; el matiz va en spec futura si se requiere).
- Instalador / auto-arranque con Windows.

## 5. Requerimientos Funcionales

- **RF-1:** El sistema debe enumerar las ventanas top-level visibles cuyo título o proceso identifique un cliente Dofus, excluyendo ventanas ocultas, tool-windows y la propia ventana de la app.
- **RF-2:** El sistema debe mantener la lista en un orden estable y determinista (orden Z de enumeración o alfabético por título como fallback documentado) para que Siguiente/Anterior sean predecibles.
- **RF-3:** El sistema debe exponer `MoveNext()` / `MovePrevious()` con semántica circular: tras la última ventana vuelve a la primera y viceversa.
- **RF-4:** El sistema debe llevar la ventana objetivo al frente restaurándola si está minimizada.
- **RF-5:** El sistema debe registrar dos hotkeys globales (valores por defecto: `Ctrl+Mayús+D` = siguiente, `Ctrl+Mayús+A` = anterior — configurable en specs posteriores) y responder aunque la app no tenga el foco.
- **RF-6:** El sistema debe refrescar la lista de ventanas antes de cada salto (o ante evento de hotkey) para tolerar aperturas/cierres de clientes sin reiniciar la app.

## 6. Requerimientos No Funcionales

- **RNF-1 (Performance):** El salto de foco debe completarse en < 100 ms percibidos en equipos típicos (enumeración + `SetForegroundWindow`).
- **RNF-2 (Testabilidad):** `WindowCycleService` no referencia `System.Windows` ni P/Invoke; depende de `IWindowEnumerator` / `IForegroundService` inyectados. Cobertura unitaria del ciclo al 100 % de ramas circulares.
- **RNF-3 (Recursos):** `HotKeyRegistrar` implementa `IDisposable` y desregistra todos los hotkeys en `Dispose()`. Sin leaks de hooks tras cerrar la app.
- **RNF-4 (Robustez):** Si no hay ventanas de Dofus, los hotkeys son no-op (sin excepción visible). Si la ventana objetivo se cerró entre enumeración y foco, se intenta con la siguiente válida.

## 7. Diseño Técnico Propuesto

```
[MainWindow (WPF)] --usa--> [HotKeyRegistrar : IDisposable] --> user32.dll (Register/UnregisterHotKey)
        |                              |
        +-- hook WM_HOTKEY             +--> [WindowCycleService] --usa--> [IWindowEnumerator] --> user32.dll (EnumWindows)
                                       +--> [IForegroundService] --> user32.dll (SetForegroundWindow/ShowWindow)
```

| Clase / Interfaz | Responsabilidad | Proyecto |
|------------------|-----------------|----------|
| `GameWindow` (record) | DTO `HWND (nint)`, `Title`, `ProcessId` | `src/DofusSwitchIt/Core/` |
| `IWindowEnumerator` | `IReadOnlyList<GameWindow> EnumerateDofusWindows()` | `src/DofusSwitchIt/Core/` |
| `IForegroundService` | `void BringToFront(GameWindow)` | `src/DofusSwitchIt/Core/` |
| `WindowCycleService` | Estado del índice + `MoveNext/MovePrevious/Refresh` (puro, testeable) | `src/DofusSwitchIt/Core/` |
| `NativeMethods` | P/Invoke `LibraryImport` centralizado | `src/DofusSwitchIt/Native/` |
| `Win32WindowEnumerator : IWindowEnumerator` | Implementación real con `EnumWindows` | `src/DofusSwitchIt/Native/` |
| `Win32ForegroundService : IForegroundService` | `IsIconic→ShowWindow→SetForegroundWindow` | `src/DofusSwitchIt/Native/` |
| `HotKeyRegistrar : IDisposable` | `Register/UnregisterHotKey` + hook `WM_HOTKEY` | `src/DofusSwitchIt/Native/` |
| `WindowCycleServiceTests` | Tests xUnit + FluentAssertions del ciclo | `tests/DofusSwitchIt.Tests/` |

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (Enumeración filtra no-Dofus):**
  - **GIVEN** 3 ventanas Dofus visibles + 2 ventanas ajenas (navegador, Discord)
  - **WHEN** se ejecuta la enumeración
  - **THEN** se devuelven exactamente las 3 ventanas Dofus en orden estable
- **CA-2 (Siguiente circular):**
  - **GIVEN** ciclo con ventanas `[A, B, C]` y foco actual en `C`
  - **WHEN** se invoca `MoveNext()`
  - **THEN** el foco va a `A` (vuelta al inicio) vía `SetForegroundWindow`
- **CA-3 (Anterior circular):**
  - **GIVEN** ciclo con ventanas `[A, B, C]` y foco actual en `A`
  - **WHEN** se invoca `MovePrevious()`
  - **THEN** el foco va a `C`
- **CA-4 (Restaurar minimizada):**
  - **GIVEN** la ventana objetivo está minimizada (`IsIconic == true`)
  - **WHEN** se invoca el salto hacia ella
  - **THEN** se llama `ShowWindow(SW_RESTORE)` antes de `SetForegroundWindow`
- **CA-5 (Sin ventanas = no-op):**
  - **GIVEN** cero ventanas Dofus abiertas
  - **WHEN** se pulsan los hotkeys Siguiente/Anterior
  - **THEN** no se produce excepción ni cambio de foco
- **CA-6 (Hotkeys globales + Dispose):**
  - **GIVEN** la app en ejecución con hotkeys registrados
  - **WHEN** se pulsa el hotkey aun con otra app en foco, y luego se cierra la app
  - **THEN** el salto ocurre con la app en segundo plano, y al cerrar no quedan hotkeys registrados (verificable: re-registrar los mismos IDs no falla con `ERROR_HOTKEY_ALREADY_REGISTERED`)

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-1` | Unitaria (xUnit) | Enumeración fake devuelve solo Dofus | CA-1 |
| `T-2` | Unitaria (xUnit) | `MoveNext` circular `C→A` | CA-2 |
| `T-3` | Unitaria (xUnit) | `MovePrevious` circular `A→C` | CA-3 |
| `T-4` | Unitaria (xUnit + mock `IForegroundService`) | Orden `ShowWindow` antes de `SetForegroundWindow` si minimizada | CA-4 |
| `T-5` | Unitaria (xUnit) | Lista vacía → `MoveNext/Previous` no-op | CA-5 |
| `T-6` | Manual | Hotkeys globales funcionan con foco en otra app; sin residuo tras cierre | CA-6 |

## 10. Riesgos y Preguntas Abiertas

- [ ] **Identificador exacto de ventana Dofus:** ¿título (`"Dofus"`), nombre de proceso (`Dofus.exe`), o clase de ventana? A validar con Spy++ en cliente real; el enumerador debe parametrizar el predicado.
- [ ] **Restricciones de `SetForegroundWindow`:** Windows puede rechazar el robo de foco (foreground lock). Mitigación documentada: la llamada proviene de un hotkey del usuario (input válido) y como fallback `AttachThreadInput` (solo si se demuestra necesario).
- [ ] **Anticheat / ToS de Ankama:** verificar que el cambio de foco no viola términos del juego (esta utilidad no inyecta ni automatiza inputs dentro del cliente).
