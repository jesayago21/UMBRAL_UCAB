# Iteración 7 — Cerrar / Cancelar sesión + Ranking final (HU-23)

**Fase:** 1 (Domain + Unit Tests) — **última iteración de Fase 1**  
**Fecha:** 2026-05-27  
**Fuentes:** `umbral-quality-spec.md §4.1`, ERS HU-23, RB-08

---

## Caso de uso

El operador cierra la sesión de dos formas:

1. **Finalizar** — cierre normal desde `Activa` o `Pausada` → `Finalizada`
2. **Cancelar** — abortar desde cualquier estado no terminal → `Cancelada` (con motivo)

Tras cerrar, se puede consultar el **ranking final** ordenado por puntaje (RB-08).

---

## Reglas implementadas

| ID | Regla |
|----|-------|
| — | `Finalizar` solo desde `Activa` o `Pausada` |
| — | `Cancelar` no permitido en `Finalizada` / `Cancelada` |
| — | `Cancelar` requiere motivo no vacío; emite `SesionCancelada` |
| RB-08 | Ranking por puntaje descendente; desempate por nombre |
| — | `ObtenerRankingFinal()` solo en sesiones cerradas |

---

## Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `Sesion/Events/SesionCancelada.cs` | **Nuevo** |
| `Sesion/PosicionRanking.cs` | **Nuevo** |
| `Sesion/RankingService.cs` | **Nuevo** |
| `Sesion/Sesion.cs` | `Cancelar` emite evento + historial; `ObtenerRankingFinal()` |
| `tests/.../SesionCerrarSesionTests.cs` | **Nuevo** — 14 tests |
| `tests/.../RankingServiceTests.cs` | **Nuevo** — 2 tests |

---

## Tests (16 nuevos → 170 total)

| Clase | Tests |
|-------|-------|
| `SesionCerrarSesionTests` | Finalizar×5, Cancelar×6, Ranking×3 |
| `RankingServiceTests` | Orden descendente, desempate por nombre |

---

## Fase 1 completada

| Iter | HU | Estado |
|------|-----|--------|
| 01 | HU-12 | ✅ |
| 02 | HU-13 | ✅ |
| 03 | HU-14, HU-15 | ✅ |
| 04 | HU-16 | ✅ |
| 05 | HU-18 | ✅ |
| 06 | HU-19, HU-20 | ✅ |
| 07 | HU-23 | ✅ |

**Tests dominio:** 170/170  
**Próximo paso:** Fase 2 — Application (CQRS/MediatR, handlers, validators)
