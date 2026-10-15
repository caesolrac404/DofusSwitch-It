# SPEC-011: Higiene Clean Code (refactor sin cambio de comportamiento)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `011` |
| **Título** | Higiene Clean Code (refactor sin cambio de comportamiento) |
| **Rama Git** | `feature/011-clean-code-hygiene` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It (auditoría con skill `clean-code`) |
| **Fecha** | `2026-10-15` |
| **Dependencias** | `010` (refactor sobre el código ya mergeado) |

## 2. Contexto

Auditoría con el checklist de Clean Code (Martin). El código funciona y está bien nombrado;
se detectaron 4 olores: (1) re-sincronización de `_index`/`FindIndex` repetida ~7 veces;
(2) condición muerta en `MoveTo`; (3) `Refresh()` mezclando niveles de abstracción;
(4) `WindowList_Drop` haciendo 4 cosas con doble búsqueda. Refactor puro: la suite en verde
es la red de seguridad (ningún cambio de comportamiento observable).

## 3. User Stories

- **US-1:** Como mantenedor, quiero que un cambio de regla de índices se toque en un solo lugar.

## 4. Alcance

### Dentro del alcance (In)
- `WindowCycleService`: `IndexOf(hwnd)`, `ResyncIndex(currentHwnd)`, `FetchOrdered()`, `Reconcile(...)`; quitar condición muerta.
- `MainWindow`: `TryGetDroppedWindow(...)`, `IsOnScreen(placement)`.
- Esta spec NO añade tests nuevos (la cobertura existente —35 tests— hace de red).

### Fuera del alcance (Out — No hacer)
- Cambiar `catch` vacíos de `WindowPlacementStore` (deliberados, RNF-1 spec 007).
- Partir el code-behind en ViewModels.

## 5. Requerimientos Funcionales

- **RF-1:** Comportamiento idéntico: misma semántica de `Refresh`, `MoveUp/Down/To`, hotkeys y drag & drop.
- **RF-2:** Ningún método supera ~20 líneas.

## 6. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1:** GIVEN la suite completa WHEN `dotnet test` THEN 35/35 verde sin cambios en los tests
- **CA-2:** GIVEN el código WHEN lo leo THEN la lógica de índices vive en un único helper

## 7. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-29` | Regresión (suite existente) | 35/35 verde = comportamiento intacto | CA-1 |
| `T-30` | Manual | F2/F3, arrastre, doble clic, ayuda siguen operativos | CA-2 |
