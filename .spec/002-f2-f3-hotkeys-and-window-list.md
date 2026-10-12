# SPEC-002: Hotkeys F2/F3 y lista visible de ventanas de Dofus

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `002` |
| **Título** | Hotkeys F2/F3 y lista visible de ventanas de Dofus |
| **Rama Git** | `feature/002-f2-f3-hotkeys-and-window-list` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-12` |
| **Dependencias** | `001` (reutiliza `WindowCycleService`, `Win32WindowEnumerator`, `Win32ForegroundService`, `HotKeyRegistrar`) |

## 2. Contexto

La SPEC-001 dejó el Core y las clases Win32 implementadas pero sin cablear: `MainWindow` es
el template vacío de WPF y la app no hace nada al ejecutarse. Además, los hotkeys por defecto
de 001 (`Ctrl+Mayús+D` / `Ctrl+Mayús+A`) no sirven al usuario: su teclado no tiene botones
dedicados de siguiente/anterior y necesita teclas directas. Esta spec cablea la UI mínima
didáctica: lista visible de ventanas Dofus detectadas + hotkeys globales **F2 (Anterior/atrás)**
y **F3 (Siguiente/adelante)** sin modificadores.

## 3. User Stories

- **US-1:** Como jugador sin botones multimedia, quiero pulsar **F3** para avanzar a la siguiente ventana de Dofus, para rotar con una sola tecla.
- **US-2:** Como jugador, quiero pulsar **F2** para retroceder a la ventana anterior, para corregir sobre-rotaciones.
- **US-3:** Como jugador, quiero ver dentro de la app la lista de ventanas Dofus detectadas (con la actual resaltada), para confirmar que la detección funciona sin tener que adivinarlo.

## 4. Alcance

### Dentro del alcance (In)
- Registro global de `F2` (anterior) y `F3` (siguiente) vía `HotKeyRegistrar` (sin modificadores), con `IDisposable` al cerrar.
- `MainWindow` funcional: `ListBox` de ventanas detectadas, resaltado de la actual, botones `Anterior (F2)` / `Siguiente (F3)` / `Refrescar`, y texto de estado (conteo + errores de registro).
- Refresco de la lista al abrir, ante cada hotkey/clic y con temporizador periódico (~1 s, solo lectura, sin robar foco).
- Mapeo de teclas centralizado y testeable (`HotKeyDefaults`: F2 = anterior, F3 = siguiente).

### Fuera del alcance (Out — No hacer)
- Configuración de hotkeys personalizados por el usuario (spec futura).
- Persistencia, overlay, auto-arranque, instalador.
- MVVM completo con bindings (se acepta code-behind mínimo; la lógica sigue en el Core testeado).

## 5. Requerimientos Funcionales

- **RF-1:** Al iniciar, la app registra `F2` (id 1, sin modificadores, `VK_F2 = 0x71`) como Anterior y `F3` (id 2, `VK_F3 = 0x72`) como Siguiente. Si el registro falla (tecla ocupada por otra app), la app no se cae: lo informa en la barra de estado y los botones siguen funcionando.
- **RF-2:** Cada hotkey ejecuta `MovePrevious()` / `MoveNext()` sobre el `WindowCycleService` compartido y actualiza la lista.
- **RF-3:** La lista muestra `Título` de cada ventana Dofus detectada, en el orden del ciclo, con la ventana actual seleccionada/resaltada.
- **RF-4:** Botones `Anterior`, `Siguiente` y `Refrescar` replican el comportamiento de los hotkeys y del refresco manual.
- **RF-5:** Clic (o doble clic) sobre un elemento de la lista lleva esa ventana al frente.
- **RF-6:** Al cerrar la ventana principal se liberan ambos hotkeys (`Dispose`).

## 6. Requerimientos No Funcionales

- **RNF-1 (Recursos):** `HotKeyRegistrar` se crea con el `HwndSource` real y se dispone en `Closed`. Cero hotkeys residuales.
- **RNF-2 (Testabilidad):** El mapeo F2/F3 vive en `HotKeyDefaults` (Core, sin WPF) con test unitario. El code-behind no se testea con UI.
- **RNF-3 (Robustez):** Lista vacía → mensaje didáctico ("No se detectaron ventanas de Dofus") en lugar de lista vacía silenciosa; hotkeys no-op.

## 7. Diseño Técnico Propuesto

```
[MainWindow (code-behind mínimo)]
   |-- HotKeyRegistrar (HwndSource, Dispose en Closed): F2→MovePrevious, F3→MoveNext
   |-- WindowCycleService (Win32WindowEnumerator + Win32ForegroundService)
   |-- DispatcherTimer 1 s → Refresh() + repintar lista (solo lectura)
   +-- ObservableCollection<string> + SelectedIndex = índice de Current
[HotKeyDefaults (Core)]  NextKey=F3/0x72, PreviousKey=F2/0x71, Modifiers=0
```

| Clase | Responsabilidad |
|-------|-----------------|
| `HotKeyDefaults` (nuevo, `Core/`) | Constantes `PreviousKey/NextKey/Modifiers/Ids`, sin WPF |
| `MainWindow` | Cableado UI ↔ servicio, lista, botones, timer, Dispose |
| `HotKeyDefaultsTests` | Assert del mapeo F2↔anterior, F3↔siguiente |

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (F3 avanza):**
  - **GIVEN** app abierta con ventanas `[A, B, C]` y actual `A`
  - **WHEN** se pulsa `F3` (incluso con otra app en foco)
  - **THEN** el foco pasa a `B` y la lista resalta `B`
- **CA-2 (F2 retrocede):**
  - **GIVEN** actual `A`
  - **WHEN** se pulsa `F2`
  - **THEN** el foco pasa a `C` (circular) y la lista resalta `C`
- **CA-3 (Lista didáctica):**
  - **GIVEN** 2 clientes Dofus abiertos
  - **WHEN** se abre la app
  - **THEN** la lista muestra 2 títulos, uno resaltado como actual; el estado indica "2 ventanas detectadas"
- **CA-4 (Sin ventanas):**
  - **GIVEN** cero clientes Dofus
  - **WHEN** se abre la app y se pulsa `F3`
  - **THEN** se muestra el mensaje de no detectadas y no hay excepción
- **CA-5 (Mapeo testeado):**
  - **GIVEN** `HotKeyDefaults`
  - **WHEN** se leen sus constantes
  - **THEN** `PreviousKey == VK_F2 (0x71)`, `NextKey == VK_F3 (0x72)`, modificadores `0` (test xUnit en verde)

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-6` | Unitaria (xUnit) | Mapeo F2/F3 en `HotKeyDefaults` | CA-5 |
| `T-7` | Manual | F3/F2 globales rotan foco + resaltado (con 2–3 Dofus abiertos) | CA-1, CA-2 |
| `T-8` | Manual | Lista y estado correctos al abrir; mensaje sin ventanas | CA-3, CA-4 |
| `T-9` | Manual | Cierre libera hotkeys (reabrir no falla con `ERROR_HOTKEY_ALREADY_REGISTERED`) | RF-6 |

## 10. Riesgos y Preguntas Abiertas

- [ ] **F2/F3 sin modificadores son globales agresivas:** interceptan esas teclas en todo el sistema mientras la app corre (pueden molestar en otras apps/juegos). Mitigación actual: documentado; la configuración de teclas queda para spec futura.
- [ ] Si otro programa ya registró F2/F3 globalmente, `RegisterHotKey` falla: la app lo informa y sigue operable por botones (RF-1).
