# SPEC-006: Widget compacto oscuro (chromeless, dark mode, minimalista)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `006` |
| **Título** | Widget compacto oscuro (chromeless, dark mode, minimalista) |
| **Rama Git** | `feature/006-compact-dark-ui` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-13` |
| **Dependencias** | `005` (reestiliza su UI sin tocar la lógica) |

## 2. Contexto

La UI de la SPEC-005 funciona pero es una ventana estándar alta (420 px), blanca y con textos
largos que molesta a la vista en sesiones de juego. Se quiere un widget flotante delgado:
mitad de altura, sin maximizar, sin icono, título limpio "Dofus Switcher", paleta gris
oscura, textos mínimos (`F2 ◀` / `F3 ▶`, contador `N v.`) y lista flat. Solo cambian
`MainWindow.xaml` / `MainWindow.xaml.cs`; la lógica del ciclo queda intacta.

## 3. User Stories

- **US-1:** Como jugador, quiero un widget pequeño y oscuro siempre visible, para que no tape ni deslumbre durante el juego.
- **US-2:** Como jugador, quiero ver de un vistazo el orden (`1. Nombre`) y el conteo (`3 v.`), sin leer párrafos.

## 4. Alcance

### Dentro del alcance (In)
- Ventana: `Title="Dofus Switcher"`, `Height≈210`, `ResizeMode="CanMinimize"` (sin maximizar), `Topmost` (heredado), sin `Icon`.
- Tema oscuro sólido: fondo `#1E1E1E`, lista `#252526`, texto `#D8D8D8`, selección `#37373D`, bordes finos `#3A3A3A`. (Acrílico/mica real exigiría ventana sin marco + P/Invoke: queda fuera.)
- Textos mínimos: `F2 ◀` / `F3 ▶` arriba, `N v.` + `⟳` abajo; lista muestra solo el primer token del título (nombre del personaje) + número de turno. Se conserva el ajuste local del usuario (`Title.Split(' ')[0]`) formalizado en helper testeable.
- Lista flat: `BorderThickness="0"`, items con padding, highlight sutil.

### Fuera del alcance (Out — No hacer)
- Efecto acrílico/mica real, ventana click-through, recordar posición.
- Cambios en el Core o en hotkeys.

## 5. Requerimientos Funcionales

- **RF-1:** La ventana no ofrece maximizar; altura inicial ≈ mitad de la anterior (~210 px).
- **RF-2:** La lista muestra `"{i}. {DisplayName}"` donde `DisplayName` = primer token del título (vacío → título completo como fallback).
- **RF-3:** El pie muestra `"{n} v."`; sin ventanas muestra `Sin ventanas`; error de registro de hotkey agrega `⚠`.
- **RF-4:** Drag & drop, doble-clic-enfocar, F2/F3 y `⟳` funcionan igual que en 005.

## 6. Requerimientos No Funcionales

- **RNF-1:** Cero cambios de comportamiento: `dotnet test` sigue 18/18+ en verde; se agrega test del helper `DisplayName`.
- **RNF-2:** Contraste legible del gris sobre gris (texto `#D8D8D8` sobre `#1E1E1E`).

## 7. Diseño Técnico Propuesto

```
// XAML: recursos inline (SolidColorBrush), ItemContainerStyle flat.
// Core: GameWindow.DisplayName => primer token antes de ' ' (helper estático testeable).
// Code-behind: solo cambia el render (DisplayName) y los literales de estado.
```

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (DisplayName):**
  - **GIVEN** título `"Craette - Dofus 2.73"`
  - **WHEN** se calcula el nombre visible
  - **THEN** es `"Craette"` (test en verde; fallback al título si no hay espacios)
- **CA-2 (Widget):**
  - **GIVEN** app abierta
  - **WHEN** miro la ventana
  - **THEN** es oscura, ~210 px de alto, sin botón maximizar, título "Dofus Switcher", `F2 ◀`/`F3 ▶`, lista `1. X…`, pie `3 v. ⟳`
- **CA-3 (Sin regresión):**
  - **GIVEN** la suite xUnit
  - **WHEN** `dotnet test`
  - **THEN** todo verde y el drag & drop / F2/F3 manual siguen funcionando

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-20` | Unitaria | `DisplayName`: primer token + fallback | CA-1 |
| `T-21` | Manual | Dimensiones, tema, textos mínimos, no-maximizar | CA-2 |
| `T-22` | Manual | F2/F3, arrastre, doble clic, ⟳ intactos | CA-3 |

## 10. Riesgos y Preguntas Abiertas

- [ ] Si Ankama cambia el formato del título, el primer token podría no ser el personaje: el fallback muestra el título completo.
