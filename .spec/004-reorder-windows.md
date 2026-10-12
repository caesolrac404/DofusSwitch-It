# SPEC-004: Reordenamiento manual de ventanas (orden de turnos)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `004` |
| **Título** | Reordenamiento manual de ventanas (orden de turnos) |
| **Rama Git** | `feature/004-reorder-windows` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-12` |
| **Dependencias** | `003` (el ciclo opera sobre el orden de `WindowCycleService`) |

## 2. Contexto

Dofus es un juego por turnos: en cada combate las cuentas actúan en un orden que no coincide
con el orden alfabético/de detección. Hoy el ciclo impone el orden estable de la SPEC-003 sin
posibilidad de cambio, así que rotar en orden de turnos es imposible. Se necesita reordenar
manualmente la lista (subir/bajar la ventana seleccionada) y que F2/F3 respeten ese orden.

## 3. User Stories

- **US-1:** Como jugador en combate, quiero subir/bajar ventanas en la lista para dejarla en orden de turnos, para rotar con F3 siguiendo el turno real.
- **US-2:** Como jugador, quiero que mi orden manual sobreviva a los refrescos (aperturas/cierres de clientes), para no reordenar a cada rato.

## 4. Alcance

### Dentro del alcance (In)
- `WindowCycleService.MoveUp/MoveDown(GameWindow)`: intercambia con el vecino; no-op en extremos. El orden manual es la fuente de verdad: `Refresh()` conserva posiciones, elimina cerradas y agrega nuevas **al final**.
- UI: botones `▲ Subir` / `▼ Bajar` sobre la selección, lista numerada (`1. Título`) para leer el orden de turnos, la selección sigue al elemento movido.
- Tests unitarios del reordenamiento y de la reconciliación en `Refresh()`.

### Fuera del alcance (Out — No hacer)
- Drag & drop (botones bastan para el caso de uso).
- Persistencia en disco entre reinicios (el orden de turnos cambia por combate; se evalúa en spec futura si se pide).
- Reordenamiento automático según el turno del juego (requeriría leer memoria del cliente: fuera de cuestión).

## 5. Requerimientos Funcionales

- **RF-1:** `MoveUp` sobre la primera y `MoveDown` sobre la última son no-op (sin excepción).
- **RF-2:** Tras mover, la ventana actual (`Current`) sigue siendo la misma ventana (por `Hwnd`), no la que ocupó su índice.
- **RF-3:** `Refresh()` mantiene el orden manual: elimina `Hwnd` cerrados, agrega ventanas nuevas al final (ordenadas por título+`Hwnd` entre ellas), nunca reordena las existentes.
- **RF-4:** Los botones actúan sobre la selección actual de la lista; tras mover, la selección sigue al elemento movido para pulsaciones repetidas.
- **RF-5:** La lista muestra el número de posición (`1.`, `2.`, …) delante del título.

## 6. Requerimientos No Funcionales

- **RNF-1 (Testabilidad):** Todo el reordenamiento vive en el Core con fakes; la UI solo delega.
- **RNF-2 (Sin regresión):** F2/F3 circulares y orden estable inicial (SPEC-003) intactos.

## 7. Diseño Técnico Propuesto

```
// WindowCycleService: _windows pasa a ser fuente de verdad del orden.
Refresh(): fresh = enumerar + ordenar (título+Hwnd)
           _windows = existentes que siguen abiertos (en su posición) + nuevas al final
           re-sincronizar _index por Hwnd de Current
MoveUp/MoveDown(w): swap con vecino + re-sincronizar _index por Hwnd
```

| Clase | Cambio |
|-------|--------|
| `WindowCycleService` | `MoveUp/MoveDown` + reconciliación en `Refresh()` |
| `MainWindow` | Botones Subir/Bajar, lista numerada, selección sigue al movido |
| `WindowReorderingTests` | Swaps, extremos no-op, persistencia del orden en Refresh, altas/bajas |

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (Bajar):**
  - **GIVEN** orden `[A, B, C]`
  - **WHEN** `MoveDown(A)`
  - **THEN** el orden es `[B, A, C]` y F3 desde `B` va a `A`
- **CA-2 (Extremos no-op):**
  - **GIVEN** orden `[A, B, C]`
  - **WHEN** `MoveUp(A)` y `MoveDown(C)`
  - **THEN** el orden sigue `[A, B, C]`, sin excepción
- **CA-3 (Orden sobrevive a Refresh):**
  - **GIVEN** orden manual `[B, A, C]`, se cierra `A` y se abre `D`
  - **WHEN** `Refresh()`
  - **THEN** el orden es `[B, C, D]`
- **CA-4 (UI):**
  - **GIVEN** app con `[A, B, C]`
  - **WHEN** selecciono `C` y pulso `▲ Subir` dos veces
  - **THEN** la lista muestra `1. C, 2. A, 3. B` con `C` seleccionado

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-13` | Unitaria | Swap subir/bajar + extremos no-op | CA-1, CA-2 |
| `T-14` | Unitaria | `Refresh` conserva orden, elimina cerradas, agrega al final | CA-3 |
| `T-15` | Unitaria | `Current` (por `Hwnd`) intacto tras mover | RF-2 |
| `T-16` | Manual | Botones + numeración + F2/F3 respetan el orden manual | CA-4 |

## 10. Riesgos y Preguntas Abiertas

- [ ] Ningún riesgo técnico relevante; el orden es solo memoria de sesión (sin persistencia por decisión de alcance).
