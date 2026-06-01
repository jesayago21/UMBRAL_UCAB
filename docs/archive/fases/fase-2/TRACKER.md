# Fase 2 — Tracker Application (CQRS / MediatR)

**Alcance:** `Umbral.Application` + `Umbral.Application.Tests`  
**Rama:** `feature/fase2-application`  
**Última actualización:** 2026-05-27 (iter-02-07)

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
| 02-02 | `RegistrarEquipoCommand` | **HU-13** | `Sesion.RegistrarEquipo` | ✅ | [iter-02-02](iter-02-02-registrar-equipo.md) |
| 02-03 | `IniciarSesionCommand`, `PausarSesionCommand`, `ReanudarSesionCommand` | **HU-14**, **HU-15** | Iniciar / Pausar / Reanudar | ✅ | [iter-02-03](iter-02-03-iniciar-pausar-reanudar.md) |
| 02-04 | `AplicarPenalizacionCommand` | **HU-16** | `AplicarPenalizacion` | ✅ | [iter-02-04](iter-02-04-aplicar-penalizacion.md) |
| 02-05 | `SubmitEvidenciaCommand` | **HU-18** | `RegistrarEvidencia` | ✅ | [iter-02-05](iter-02-05-submit-evidencia.md) |
| 02-06 | Consolidación eventos HU-19/20 sobre `SubmitEvidencia` | **HU-19**, **HU-20** | ganador único + transición de etapa | ✅ | [iter-02-06](iter-02-06-hu19-hu20-eventos-sesiones.md) |
| 02-07 | `FinalizarSesionCommand`, `CancelarSesionCommand`, `GetRankingSesionQuery` | **HU-23**, **HU-21** | Finalizar / Cancelar / ranking | ✅ | [iter-02-07](iter-02-07-cierre-y-ranking-sesion.md) |

### Queries transversales (Fase 2)

| Query | Estado |
|-------|--------|
| `GetSesionByIdQuery` | ⬜ |
| `ListSesionesActivasQuery` | ⬜ |

---

## Progreso Fase 2

```
Iter 02-07 ███████  7/7 iteraciones (100%)
```

**Tests Application:** 50/50  
**Roles / auth:** ver [roles-y-autenticacion.md](roles-y-autenticacion.md) (no aplica en handlers Fase 2)
