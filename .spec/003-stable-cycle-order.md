# SPEC-003: Orden estable del ciclo (fix: F3 solo alternaba 2 de 3 ventanas)

## 1. Metadata

| Campo | Valor |
|-------|-------|
| **ID** | `003` |
| **Título** | Orden estable del ciclo (fix F3 ping-pong) |
| **Rama Git** | `feature/003-stable-cycle-order` |
| **Estado** | En curso |
| **Autor** | Equipo DofusSwitch-It |
| **Fecha** | `2026-10-12` |
| **Dependencias** | `001` (`WindowCycleService`), `002` (reportado sobre F2/F3) |

## 2. Contexto

Bug reportado con 3 ventanas abiertas: **F3 (siguiente) solo alterna 2 ventanas**,
mientras **F2 (anterior) recorre las 3**. Causa raíz: `EnumWindows` devuelve las ventanas
en **orden Z**, y cada `SetForegroundWindow` reordena ese orden Z. `WindowCycleService.Refresh()`
conservaba la posición por `HWND` sobre la lista recién enumerada, así que `MoveNext()`
calculaba el "siguiente" respecto a la posición nueva (siempre arriba del todo) en vez de
respecto al orden previo al salto → ping-pong `A↔B` sin llegar nunca a `C`. `MovePrevious()`
enmascaraba el bug porque desde el índice 0 siempre cae al último, que con el reordenamiento
resultaba ser una ventana distinta cada vez. Los tests con fakes no lo detectaron porque el
fake devolvía orden fijo, independiente del foco.

## 3. User Stories

- **US-1:** Como jugador con 3+ cuentas, quiero que F3 recorra **todas** las ventanas en ciclo, para no dejar cuentas inalcanzables.

## 4. Alcance

### Dentro del alcance (In)
- Ordenamiento estable y determinista en `WindowCycleService.Refresh()`: por `Title` (insensible a mayúsculas) y desempate por `Hwnd`. Sustituye la opción "orden Z" de RF-2 (001), que demostró ser inestable entre saltos.
- Test de regresión xUnit que simula enumeración en orden Z (el fake reordena al frente la última ventana enfocada): F3 debe visitar las 3 antes de repetir.

### Fuera del alcance (Out — No hacer)
- Sincronizar el índice con la ventana enfocada manualmente por el usuario (mejora futura: leer `GetForegroundWindow` en `Refresh`).
- Cambiar el predicado de detección (ajuste local a "Release" documentado aparte).

## 5. Requerimientos Funcionales

- **RF-1:** El orden del ciclo no debe depender del orden Z de enumeración: dos `Refresh()` consecutivos sin aperturas/cierres devuelven el mismo orden.
- **RF-2:** Con ventanas `[A, B, C]` y actual `A`, tres `MoveNext()` consecutivos enfocan `B`, `C`, `A` aunque la enumeración reordene al frente tras cada foco (y simétricamente `MovePrevious()`).

## 6. Requerimientos No Funcionales

- **RNF-1 (Testabilidad):** La regresión se verifica sin UI ni Win32 (fakes con reordenamiento Z simulado).

## 7. Diseño Técnico Propuesto

```csharp
// WindowCycleService.Refresh()
_windows = _enumerator.EnumerateDofusWindows()
    .OrderBy(w => w.Title, StringComparer.OrdinalIgnoreCase)
    .ThenBy(w => w.Hwnd)
    .ToList();
```

El resto (`FindIndex` por `Hwnd`, aritmética circular) no cambia.

## 8. Criterios de Aceptación (GIVEN / WHEN / THEN)

- **CA-1 (F3 recorre las 3):**
  - **GIVEN** ventanas `[A, B, C]`, enumerador que reordena al frente la última enfocada (como el orden Z real), actual `A`
  - **WHEN** se invoca `MoveNext()` tres veces
  - **THEN** el foco va a `B`, `C`, `A` en ese orden (test de regresión en verde)
- **CA-2 (F2 sigue recorriendo las 3):**
  - **GIVEN** mismo enumerador
  - **WHEN** se invoca `MovePrevious()` tres veces desde `A`
  - **THEN** el foco va a `C`, `B`, `A` (sin regresión)

## 9. Plan de Pruebas

| ID | Tipo | Qué verifica | CA asociado |
|----|------|--------------|-------------|
| `T-10` | Unitaria (xUnit, fakes con reorden Z) | `MoveNext` visita las 3 antes de repetir | CA-1 |
| `T-11` | Unitaria (xUnit, mismos fakes) | `MovePrevious` visita las 3 antes de repetir | CA-2 |
| `T-12` | Manual | Con 3 Dofus reales, F3 rota A→B→C→A y F2 al revés | CA-1, CA-2 |

## 10. Riesgos y Preguntas Abiertas

- [x] **Títulos idénticos:** si los 3 clientes titulan igual, el orden lo define el `Hwnd` (estable durante la vida de la ventana). Verificado en el diseño.
- [ ] **Foco manual intermedio:** si el usuario clickea una ventana directamente, el ciclo continúa desde su propio índice, no desde el foco real. Futura mejora.
