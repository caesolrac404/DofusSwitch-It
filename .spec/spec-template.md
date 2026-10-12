# Plantilla de Especificación (SDD)

> Copia este archivo como `.spec/NNN-<slug>.md` para cada nueva funcionalidad.
> No escribir código hasta que la spec esté completa y tenga rama asignada (ver `AGENT.md` §3).

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `NNN` (ej. `002`) |
| **Título** | `<Título corto de la feature>` |
| **Rama Git** | `feature/NNN-<slug>` |
| **Estado** | Borrador / En curso / En revisión / Completada |
| **Autor** | `<nombre>` |
| **Fecha** | `YYYY-MM-DD` |
| **Dependencias** | `<IDs de specs previas, o Ninguna>` |

## 2. Contexto

<!-- ¿Qué problema resuelve? ¿Quién lo usa? ¿Qué existe hoy y por qué es insuficiente? 3–8 líneas. -->

## 3. User Stories

- **US-1:** Como `<rol>`, quiero `<acción>`, para `<beneficio>`.
- **US-2:** ...

## 4. Alcance

### Dentro del alcance (In)
- ...

### Fuera del alcance (Out — No hacer)
- ...

## 5. Requerimientos Funcionales

- **RF-1:** El sistema debe ...
- **RF-2:** ...

## 6. Requerimientos No Funcionales

- **RNF-1 (Performance):** ...
- **RNF-2 (Testabilidad):** La lógica debe ser testeable sin UI (interfaces + inyección).
- **RNF-3 (Recursos):** Todo recurso Win32 debe liberarse vía `IDisposable`.

## 7. Diseño Técnico Propuesto

<!-- Componentes, clases, interfaces, APIs Win32 involucradas. Diagrama ASCII si ayuda. -->

```
[Componente A] --> [Componente B] --> user32.dll
```

| Clase / Interfaz | Responsabilidad | Proyecto |
|------------------|-----------------|----------|
| `...` | ... | `src/...` / `tests/...` |

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1:**
  - **GIVEN** `<estado inicial>`
  - **WHEN** `<acción>`
  - **THEN** `<resultado observable y verificable>`
- **CA-2:**
  - **GIVEN** ...
  - **WHEN** ...
  - **THEN** ...

## 9. Plan de Pruebas

| ID | Tipo (unitaria/integración/manual) | Qué verifica | CA asociado |
|----|------------------------------------|--------------|-------------|
| `T-1` | Unitaria (xUnit) | ... | CA-1 |

## 10. Riesgos y Preguntas Abiertas

- [ ] ...
