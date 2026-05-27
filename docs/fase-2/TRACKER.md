# Fase 2 — Tracker Application (CQRS / MediatR)

**Alcance:** `Umbral.Application` + `Umbral.Application.Tests`  
**Rama:** `feature/fase2-application`  
**Última actualización:** 2026-05-27 (iter-02-01)

**Trazabilidad:** HU según `docs/TRAZABILIDAD.md`. Reglas globales **RB-01…RB-32**.  
**NO modificar:** `Umbral.Infrastructure`, `Umbral.API`, frontend, migraciones EF en esta fase.

---

## Leyenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | Command/Query + validator + handler + tests en verde |
| ⬜ | Pendiente |
| — | Fuera de Fase 2 (API, Infra, UI) |

---

## Iteraciones

| Iter | Command / Query | HU | Domain | Estado | Doc |
|------|-----------------|-----|--------|--------|-----|
| 02-01 | `CrearSesionBusquedaTesoroCommand` | **HU-12** | `Sesion.CrearBusquedaTesoro` | ✅ | [iter-02-01](iter-02-01-crear-sesion-busqueda-tesoro.md) |
| 02-02 | `RegistrarEquipoCommand` | **HU-13** | `Sesion.RegistrarEquipo` | ⬜ | — |
| 02-03 | `IniciarSesionCommand`, `PausarSesionCommand`, `ReanudarSesionCommand` | **HU-14**, **HU-15** | Iniciar / Pausar / Reanudar | ⬜ | — |
| 02-04 | `AplicarPenalizacionCommand` | **HU-16** | `AplicarPenalizacion` | ⬜ | — |
| 02-05 | `SubmitEvidenciaCommand` | **HU-18** | `RegistrarEvidencia` | ⬜ | — |
| 02-06 | Event handlers (opc.) | **HU-19**, **HU-20** | dominio listo | ⬜ | — |
| 02-07 | `FinalizarSesionCommand`, `CancelarSesionCommand`, `GetRankingSesionQuery` | **HU-23**, **HU-21** | Finalizar / Cancelar / ranking | ⬜ | — |

### Queries transversales (Fase 2)

| Query | Estado |
|-------|--------|
| `GetSesionByIdQuery` | ⬜ |
| `ListSesionesActivasQuery` | ⬜ |

---

## Progreso Fase 2

```
Iter 02-01 █░░░░░░  1/7 iteraciones (~14%)
```

**Tests Application:** 6/6 (iter-02-01)
