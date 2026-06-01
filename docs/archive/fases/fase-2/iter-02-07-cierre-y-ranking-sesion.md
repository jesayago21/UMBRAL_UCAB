# Iteración 02-07 — Cierre y ranking de sesión (Application)

**Fase:** 2 · **HU-23**, **HU-21**  
**Rama:** `feature/fase2-application`

---

## Objetivo

Completar los casos de uso de cierre de sesión y consulta de ranking desde Application, manteniendo reglas en dominio.

---

## Componentes

| Pieza | Ubicación |
|------|-----------|
| `FinalizarSesionCommand` + handler + validator | `Sesion/Commands/FinalizarSesion/` |
| `CancelarSesionCommand` + handler + validator | `Sesion/Commands/CancelarSesion/` |
| `GetRankingSesionQuery` + handler + DTO | `Sesion/Queries/GetRankingSesion/` |

---

## Flujo

- **Finalizar:** buscar sesión -> `sesion.Finalizar()` -> persistir -> `PublishBatchAsync` -> `ClearDomainEvents`.
- **Cancelar:** buscar sesión -> `sesion.Cancelar(motivo)` -> persistir -> `PublishBatchAsync` -> `ClearDomainEvents`.
- **Ranking:** buscar sesión -> `RankingService.Calcular(sesion.Equipos)` -> map a `PosicionRankingDto`.

---

## Tests

- Finalizar handler: happy, not found, error de dominio.
- Cancelar handler: happy, not found, error de dominio.
- Validators de cierre.
- Query ranking: orden correcto y not found.
