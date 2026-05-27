# Fase 3 — Tracker Infrastructure

**Alcance:** `Umbral.Infrastructure` (persistencia, adaptadores de salida, DI y pruebas de infraestructura)  
**Rama:** `feature/fase3-infrastructure`  
**Última actualización:** 2026-05-27 (iter-03-01)

---

## Leyenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | Iteración implementada, validada y documentada |
| 🔄 | Iteración en progreso |
| ⬜ | Iteración pendiente |

---

## Iteraciones

| Iter | Objetivo | Estado | Doc |
|------|----------|--------|-----|
| 03-01 | Bootstrap Infrastructure + Persistence base | ✅ | [iter-03-01](iter-03-01-bootstrap-infrastructure.md) |
| 03-02 | Configuraciones EF Core (mapeo canónico) | ⬜ | — |
| 03-03 | Repositorios Infrastructure (ports driven) | ⬜ | — |
| 03-04 | Migración inicial + wiring DI | ⬜ | — |
| 03-05 | Tests de infraestructura (Testcontainers) | ⬜ | — |

---

## Estado actual por alcance

- `03-01` deja listo el esqueleto: `UmbralDbContext` base, paquetes EF Core/Npgsql y estructura de carpetas.
- `03-02` agrega mapeos canónicos (`IEntityTypeConfiguration<T>`, converters de IDs tipados, `Ignore(DomainEvents)`).
- `03-03` implementa `SesionRepository` y `MisionRepository`.
- `03-04` agrega wiring DI y migración inicial.
- `03-05` crea `tests/Umbral.Infrastructure.Tests` y pruebas con Testcontainers.

---

## Progreso Fase 3

```
Iter 03-01 █░░░░  1/5 iteraciones (20%)
```
