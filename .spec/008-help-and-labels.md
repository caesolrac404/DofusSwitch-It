# SPEC-008: Etiquetas claras, ayuda integrada y textos +1px

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `008` |
| **Título** | Etiquetas claras, ayuda integrada y textos +1px |
| **Rama Git** | `feature/008-help-and-labels` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-13` |
| **Dependencias** | `007` (solo toca la UI del widget) |

## 2. Contexto

Cuatro ajustes de pulido sobre el widget: (1) el contador `3 v.` no se entiende → `X pjs.`;
(2) falta ayuda descubrible → botón `?` centrado abajo que abre ventana emergente con todas
las funcionalidades; (3) textos +1 px sin agrandar la ventana fija (172×210); (4) flechas de
F2/F3 al exterior (`◀ F2 … F3 ▶`).

## 3. User Stories

- **US-1:** Como jugador, quiero leer `3 pjs.` y entender que son mis personajes detectados.
- **US-2:** Como jugador, quiero un botón `?` que me explique todo lo que la app hace, sin salir de ella.

## 4. Alcance

### Dentro del alcance (In)
- Contador `"{n} pjs."` (vacío: `0 pjs.`); se mantiene `⚠` de hotkey.
- Botón `?` centrado en la barra inferior → `HelpWindow` no modal (Owner = principal, tema oscuro) con la lista de funcionalidades.
- Fuentes +1 px: hints 11→12, lista 12→13, estado 11→12, `⟳` 13→14. Ventana fija intacta.
- Cabecera: izquierda `◀ F2`, derecha `F3 ▶`.
- Sin cambios en el Core (sin tests nuevos de lógica; la suite debe seguir verde).

### Fuera del alcance (Out — No hacer)
- Contenido editable/online de la ayuda; redimensionar la ventana.

## 5. Requerimientos Funcionales

- **RF-1:** El pie muestra `"{n} pjs."`; con cero ventanas, `0 pjs.`.
- **RF-2:** `?` abre la ayuda; la principal sigue operable (no modal); la ayuda documenta F2/F3, arrastre, doble clic, `⟳`, contador y posición recordada.
- **RF-3:** Ningún texto crece la ventana: todo cabe en 172×210.

## 6. Requerimientos No Funcionales

- **RNF-1:** `dotnet test` en verde sin regresión.

## 7. Diseño Técnico Propuesto

```
// XAML: barra inferior Grid de 3 columnas (estado | ? centrado | ⟳).
// HelpWindow.xaml/.cs nuevos (tema oscuro, ScrollViewer + bullets).
// Code-behind: HelpButton_Click → new HelpWindow { Owner = this }.Show().
```

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1:** GIVEN 3 ventanas WHEN miro el pie THEN leo `3 pjs.`
- **CA-2:** GIVEN app abierta WHEN pulso `?` THEN se abre la ayuda con todas las funciones y puedo seguir usando la principal
- **CA-3:** GIVEN la ventana WHEN comparo con antes THEN los textos se ven 1 px más grandes y la ventana mide lo mismo; cabecera `◀ F2` / `F3 ▶`

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-26` | Manual | Contador `pjs.`, `?` + contenido, tamaños, flechas exteriores | CA-1, CA-2, CA-3 |

## 10. Riesgos y Preguntas Abiertas

- [ ] Ninguno relevante.
