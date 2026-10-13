# SPEC-007: Cromado mínimo (solo cerrar), icono y recordar posición

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `007` |
| **Título** | Cromado mínimo (solo cerrar), icono y recordar posición |
| **Rama Git** | `feature/007-chrome-icon-position` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-13` |
| **Dependencias** | `006` (reestiliza su widget) |

## 2. Contexto

El widget de la SPEC-006 aún muestra botones minimizar/maximizar, usa el ancho completo
(230 px) y el título largo "Dofus Switcher". El usuario aportó `Logo/DofusSwitch-It.ico` y
pide: solo botón cerrar, ancho al 75 % (~172 px), título con iniciales `DS-I`, icono visible
(título + exe/barra de tareas) y que la ventana recuerde su posición entre sesiones.

Decisiones acordadas: `ResizeMode="NoResize"` (quita minimizar y maximizar; la ventana queda
de tamaño fijo — alternativa descartada: `CanMinimize`; chromeless custom queda para otra
spec si se pide); icono también en la barra de título; mejora incluida: persistir posición.

## 3. User Stories

- **US-1:** Como jugador, quiero que la ventana solo tenga botón cerrar, para no minimizarla ni maximizarla por accidente.
- **US-2:** Como jugador, quiero ver el logo en la ventana y la barra de tareas, para identificar la app.
- **US-3:** Como jugador, quiero que la ventana reabra donde la dejé, para no recolocarla cada sesión.

## 4. Alcance

### Dentro del alcance (In)
- `ResizeMode="NoResize"`, `Width=172` (75 % de 230), `Height=210`, `Title="DS-I"`, `Icon` con el .ico.
- `ApplicationIcon` en el csproj (icono del .exe) + recurso para el título.
- `WindowPlacementStore` (Core): guarda/carga `{Left, Top}` en `%AppData%/DofusSwitchIt/window.json`; valida coordenadas finitas y visibilidad en pantalla virtual; fallos de IO = no-op.
- Tests: roundtrip guardar/cargar, archivo ausente/corrupto → null.

### Fuera del alcance (Out — No hacer)
- Chromeless total con botón cerrar propio, click-through, recordar tamaño (fijo por `NoResize`).

## 5. Requerimientos Funcionales

- **RF-1:** Solo el botón cerrar es visible en la barra de título.
- **RF-2:** El título muestra `DS-I` y el logo a su izquierda; el .exe muestra el logo.
- **RF-3:** Al cerrar se persiste la posición; al abrir se restaura si cae dentro de la pantalla virtual, si no se usa la posición por defecto.
- **RF-4:** Ancho inicial 172 px; el resto visual (tema, textos, drag & drop) intacto.

## 6. Requerimientos No Funcionales

- **RNF-1:** IO nunca rompe el arranque: todo fallo → posición por defecto.
- **RNF-2:** `dotnet test` en verde (incluye T-23).

## 7. Diseño Técnico Propuesto

```
// csproj: <ApplicationIcon>Logo\DofusSwitch-It.ico</ApplicationIcon> + <Resource Include=...>
// XAML: Title="DS-I" Icon="Logo/DofusSwitch-It.ico" ResizeMode="NoResize"
//       Width="172" WindowStartupLocation="Manual"
// Core/WindowPlacementStore.cs: Load()/Save(path inyectable, default %AppData%)
// MainWindow: ctor restaura (con chequeo pantalla virtual), Closed persiste.
```

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (Persistencia):**
  - **GIVEN** un store con ruta temporal
  - **WHEN** `Save({100, 200})` y luego `Load()`
  - **THEN** devuelve `{100, 200}`; con archivo ausente o corrupto devuelve null (test en verde)
- **CA-2 (Cromado):**
  - **GIVEN** app abierta
  - **WHEN** miro la barra de título
  - **THEN** veo logo + `DS-I` y solo el botón cerrar (sin minimizar ni maximizar)
- **CA-3 (Posición):**
  - **GIVEN** muevo la ventana y la cierro
  - **WHEN** reabro la app
  - **THEN** aparece en la misma posición

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-23` | Unitaria | Roundtrip + ausente/corrupto → null | CA-1 |
| `T-24` | Manual | Solo cerrar, logo, DS-I, ancho 172 | CA-2 |
| `T-25` | Manual | La posición se recuerda entre sesiones | CA-3 |

## 10. Riesgos y Preguntas Abiertas

- [x] `NoResize` = tamaño fijo: aceptado por el usuario.
- [ ] Multi-monitor con pantallas desconectadas: el chequeo de pantalla virtual lo cubre.
