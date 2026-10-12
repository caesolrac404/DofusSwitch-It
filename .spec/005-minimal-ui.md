# SPEC-005: UI mínima (tira delgada siempre visible, drag & drop)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `005` |
| **Título** | UI mínima (tira delgada siempre visible, drag & drop) |
| **Rama Git** | `feature/005-minimal-ui` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-12` |
| **Dependencias** | `004` (el orden manual ahora se edita arrastrando) |

## 2. Contexto

La UI de la SPEC-002/004 funciona pero está llena de botones (Anterior, Siguiente, Refrescar,
Subir, Bajar) en una ventana grande que estorba al jugar. Se quiere una interfaz mínima e
intuitiva: una tira vertical delgada siempre visible con la lista numerada, etiquetas de ayuda
para F2/F3 en vez de botones, un solo botón de refresco con icono, y reordenamiento por
arrastre en vez de botones Subir/Bajar.

Decisiones acordadas con el usuario: se mantiene el mapeo F2=Anterior / F3=Siguiente;
siempre visible = `Topmost` con comportamiento de clic normal; tira vertical angosta (~230 px);
refresco solo manual (se elimina el auto-refresh de 1 s).

## 3. User Stories

- **US-1:** Como jugador, quiero una ventanita delgada siempre a la vista con mi orden de turnos, para consultarlo sin tapar el juego.
- **US-2:** Como jugador, quiero reordenar arrastrando elementos de la lista, porque es más directo que botones Subir/Bajar.
- **US-3:** Como jugador, quiero ver de un vistazo qué tecla es cuál (etiquetas F2/F3), sin botones redundantes.

## 4. Alcance

### Dentro del alcance (In)
- Ventana `Topmost`, angosta (~230 px), redimensionable, con: etiquetas `F2 ◀ Anterior` / `Siguiente ▶ F3`, lista numerada con drag & drop interno, botón `⟳` solo-icono para refrescar, mini-línea de estado.
- `WindowCycleService.MoveTo(GameWindow, int index)` (nuevo, testeable): mueve a posición absoluta con clamp; `Current` intacto por `Hwnd`; no-op si la ventana no existe.
- Eliminar: botones Anterior/Siguiente/Refrescar/Subir/Bajar y el `DispatcherTimer` de auto-refresh.
- Se conserva: hotkeys F2/F3, doble clic para enfocar, refresco tras cada acción.

### Fuera del alcance (Out — No hacer)
- Ventana click-through o sin foco (se acordó clic normal para permitir el arrastre).
- Recordar posición/tamaño entre sesiones.
- Drag & drop desde/hacia fuera de la app.

## 5. Requerimientos Funcionales

- **RF-1:** La ventana tiene `Topmost=true` y ancho por defecto ~230 px.
- **RF-2:** Arrastrar un elemento sobre otra posición lo mueve ahí (incluido soltar al final); la selección sigue al elemento movido; F2/F3 respetan el nuevo orden.
- **RF-3:** `MoveTo` con índice fuera de rango hace clamp al extremo válido; con ventana inexistente es no-op.
- **RF-4:** El botón `⟳` refresca la lista (conservando el orden manual según SPEC-004).
- **RF-5:** Sin botones de texto: la única ayuda visible son las etiquetas F2/F3 y el tooltip del botón.

## 6. Requerimientos No Funcionales

- **RNF-1 (Testabilidad):** `MoveTo` cubierto con unit tests; el gesto de arrastre solo se verifica manual.
- **RNF-2 (Sin regresión):** Hotkeys, doble-clic-enfocar y orden manual intactos.

## 7. Diseño Técnico Propuesto

```
// Core: MoveTo(w, index): i = FindIndex por Hwnd; si <0 no-op;
//       extraer, clamp de index, insertar, re-sincronizar _index por Hwnd de Current.
// UI: ListBox AllowDrop; PreviewMouseLeftButtonDown registra Hwnd+origen;
//     MouseMove + distancia mínima → DoDragDrop; Drop calcula índice destino vía
//     ContainerFromElement (null = final) → MoveTo + RefreshList(Hwnd movido).
```

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (MoveTo unitario):**
  - **GIVEN** orden `[A, B, C]`
  - **WHEN** `MoveTo(C, 0)`
  - **THEN** el orden es `[C, A, B]` (test en verde)
- **CA-2 (Arrastrar manual):**
  - **GIVEN** app con `[A, B, C]`
  - **WHEN** arrastro `C` hasta la primera posición
  - **THEN** la lista muestra `1. C, 2. A, 3. B` y F3 desde `C` va a `A`
- **CA-3 (Siempre visible y delgada):**
  - **GIVEN** app abierta junto al juego
  - **WHEN** el juego tiene el foco
  - **THEN** la tira (~230 px) sigue visible sin tapar el área de juego
- **CA-4 (Sin botones de texto):**
  - **GIVEN** app abierta
  - **WHEN** miro la ventana
  - **THEN** solo hay etiquetas F2/F3, la lista y el botón `⟳`

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-17` | Unitaria | `MoveTo` a inicio/final/clamp/no-op + `Current` intacto | CA-1 |
| `T-18` | Manual | Arrastre reordena + F2/F3 lo respetan | CA-2 |
| `T-19` | Manual | Topmost + ancho + solo ⟳ y etiquetas | CA-3, CA-4 |

## 10. Riesgos y Preguntas Abiertas

- [x] Mapeo F2/F3: se mantiene F2=Anterior, F3=Siguiente (decisión con usuario).
- [x] Auto-refresh eliminado: si un cliente abre/cierra, la lista se actualiza con ⟳ o el próximo hotkey.
- [ ] El arrastre requiere clic (activa la app); aceptado por decisión de alcance.
