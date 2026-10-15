# SPEC-012: Documentar las skills del agente (code-review) en README y AGENT

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `012` |
| **Título** | Documentar las skills del agente (code-review) en README y AGENT |
| **Rama Git** | `feature/012-docs-code-review-skill` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-15` |
| **Dependencias** | Ninguna (documentación; se apoya en la regla SDD de `AGENT.md` §3) |

## 2. Contexto

Se instalaron skills de agente en `.agents/skills/` (gestionadas por `skills-lock.json`):
`clean-code` y `code-review`. La `code-review` hace una revisión en dos ejes (Standards vs Spec)
del diff `git diff <punto-fijo>...HEAD` usando sub-agentes en paralelo; su paso 2 busca la spec
en `docs/`, `specs/` o `.scratch/`, pero **este repo la tiene en `.spec/`**. Sin documentarlo,
ni el equipo ni el agente saben que la skill existe ni dónde encontrar la spec. Esta spec
actualiza el README (sección de skills) y añade a `AGENT.md` el paso de code-review en el flujo
por spec, incluyendo la nota del path `.spec/`.

## 3. User Stories

- **US-1:** Como mantenedor, quiero que el README explique las skills instaladas y cómo invocar `code-review`, para usarla antes de cada PR.

## 4. Alcance

### Dentro del alcance (In)
- README: nueva sección "Skills del agente" con tabla (`code-review`, `clean-code`), qué hace `code-review`, comando de ejemplo (`review since main`), el eje Standards/Spec y la nota de que la spec vive en `.spec/`; añadir `.agents/` y `skills-lock.json` a la estructura.
- `AGENT.md` §3: paso de verificación con `code-review` antes del `push`/PR y nota del path de spec.
- Tareas de seguimiento (transparencia): corregir la numeración del `index.md` (entradas `011`/`012` duplicadas por el merge de la `010`) y el bloque de estructura del README que aún lista solo `001`.

### Fuera del alcance (Out — No hacer)
- Instalar/actualizar las skills (las gestiona el usuario con la CLI `skills`).
- Escribir una spec por cada skill existente; solo se documentan.

## 5. Requerimientos Funcionales

- **RF-1:** El README describe ambas skills y el flujo de `code-review` con un comando reproducible.
- **RF-2:** El README explica que la spec origen está en `.spec/` (para el paso 2 de la skill).
- **RF-3:** `AGENT.md` recomienda `code-review` antes de subir la rama.

## 6. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1:** GIVEN el README WHEN lo leo THEN encuentro la sección de skills con `code-review` y su comando
- **CA-2:** GIVEN `AGENT.md` WHEN leo el flujo por spec THEN veo el paso de code-review antes del push
- **CA-3:** GIVEN `.spec/index.md` WHEN lo leo THEN no hay IDs duplicados

## 7. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-31` | Manual | README documenta las skills y el comando | CA-1 |
| `T-32` | Manual | `AGENT.md` incluye el paso de code-review | CA-2 |
| `T-33` | Manual | `index.md` sin duplicados | CA-3 |

## 8. Riesgos y Preguntas Abiertas

- [ ] Si en el futuro se instalan más skills, la tabla del README debe crecer (mantener al día).
