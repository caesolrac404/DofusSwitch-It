# SPEC-010: Predicado de detección por versión (regex generado + tests)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `010` |
| **Título** | Predicado de detección por versión (regex generado + tests) |
| **Rama Git** | `feature/010-detection-predicate` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It (+ aporte del usuario: regex por versión) |
| **Fecha** | `2026-10-14` |
| **Dependencias** | `001` (refina su RF-1); parte del regex propuesto por el usuario en la rama 009 |

## 2. Contexto

El predicado inicial (`Contains("Release")`) daba falsos positivos (ventanas de VS, archivos).
El usuario propuso matchear el formato real del cliente: versión + `- Release` al final.
Esta spec lo formaliza: regex generado en compilación, tolerante a espacios finales,
predicado público y tests con títulos reales/negativos para que un cambio de formato de
Ankama se detecte en la suite y no en silencio.

## 3. User Stories

- **US-1:** Como jugador, quiero que solo mis clientes Dofus aparezcan en la lista, sin intrusos.

## 4. Alcance

### Dentro del alcance (In)
- `[GeneratedRegex(@"\d+(\.\d+){2,3}\s*-\s*Release\s*$", IgnoreCase)]` (clase `partial`).
- `DefaultPredicate` público + `using` faltante.
- `DetectionPredicateTests`: positivos (3 y 4 partes, espacio final), negativos (`Release Notes`, `- Release` sin versión, la propia app, vacío).

### Fuera del alcance (Out — No hacer)
- Soportar versiones de 2 partes (`X.Y`): documentado como límite conocido.

## 5. Requerimientos Funcionales

- **RF-1:** Solo matchean títulos con versión de 3–4 partes + `- Release` al final (con espacios tolerados).
- **RF-2:** `DofusSwitch-It` (la propia app) nunca matchea, en defensa además de la exclusión por PID.

## 6. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1:** GIVEN títulos reales y señuelos WHEN `DefaultPredicate` THEN acierta todos (test en verde)
- **CA-2:** GIVEN la suite WHEN `dotnet test` THEN todo verde

## 7. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-28` | Unitaria | Positivos/negativos del predicado | CA-1 |
