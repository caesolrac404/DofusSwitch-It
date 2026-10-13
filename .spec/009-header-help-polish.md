# SPEC-009: Pulido de cabecera, ayuda singleton y negritas

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `009` |
| **Título** | Pulido de cabecera, ayuda singleton y negritas |
| **Rama Git** | `feature/009-header-help-polish` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-13` |
| **Dependencias** | `008` (solo toca la UI del widget y la ayuda) |

## 2. Contexto

Seis ajustes de pulido: (1) flechas verticales `▲ F2` / `F3 ▼`; (2) título de vuelta a
`DofusSwitch-It`; (3) evitar múltiples ventanas de ayuda (singleton con foco); (4) botones
`?`/`⟳` en gris más claro; (5) todo el texto en bold excepto la lista de personajes;
(6) icono más grande — **no viable**: el tamaño del icono en la barra de título lo fija
Windows (16 px) y WPF no puede agrandarlo; taskbar/exe ya usan tamaños mayores automáticamente.

## 3. User Stories

- **US-1:** Como jugador, quiero pulsar `?` tranquilo sin acumular ventanas de ayuda.

## 4. Alcance

### Dentro del alcance (In)
- Cabecera `▲ F2` / `F3 ▼`; `Title="DofusSwitch-It"`.
- Ayuda singleton: una sola instancia; si está abierta, se trae al frente.
- Recurso `BtnLight #3E3E42` para `?` y `⟳`.
- `FontWeight="Bold"` en hints, estado, botones y textos de ayuda; lista regular.
- Sin cambios en el Core (suite en verde sin tests nuevos).

### Fuera del alcance (Out — No hacer)
- Agrandar el icono de la barra de título (limitación del SO, documentada).

## 5. Requerimientos Funcionales

- **RF-1:** Pulsar `?` N veces abre como máximo una ayuda; pulsarlo con la ayuda abierta la activa.
- **RF-2:** Al cerrar la ayuda, el siguiente `?` abre una nueva normalmente.

## 6. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1:** GIVEN app abierta WHEN pulso `?` tres veces THEN hay una sola ventana de ayuda visible
- **CA-2:** GIVEN ayuda abierta detrás WHEN pulso `?` THEN la ayuda pasa al frente
- **CA-3:** GIVEN la ventana WHEN la miro THEN leo `▲ F2` / `F3 ▼`, título `DofusSwitch-It`, botones claros y negritas (lista regular)

## 7. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-27` | Manual | Singleton de ayuda + resto visual | CA-1, CA-2, CA-3 |
