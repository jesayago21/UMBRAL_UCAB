# Iteración 03-03 — Repositorios Infrastructure (ports driven)

## Objetivo

Implementar los puertos de persistencia del dominio (`ISesionRepository`, `IMisionRepository`) en `Umbral.Infrastructure`, conservando separación de capas y sin introducir lógica de negocio.

## Cambios implementados

- Se creó `SesionRepository` con implementación de:
  - `FindByIdAsync`
  - `FindActivasAsync`
  - `SaveAsync`
- Se creó `MisionRepository` con implementación de:
  - `FindByIdAsync`
  - `FindActivasAsync`
  - `SaveAsync`
- Estrategia aplicada:
  - `FindByIdAsync`: tracking por defecto para permitir mutación del agregado en comandos.
  - `FindActivasAsync`: `AsNoTracking()` para consultas de lectura.
  - `SaveAsync`: persiste agregado detached con `AddAsync` y confirma con `SaveChangesAsync`.
- Carga de agregado completo por includes sobre backing fields:
  - `Sesion`: `_participantes`, `_historialEventos`, `_evidencias`
  - `Mision`: `_etapas`, `_etapas._pistas`

## Validación

- `dotnet build Umbral.sln`
- `dotnet test Umbral.sln --no-build`

## Notas

- Esta iteración no incluye registro DI de repositorios ni migraciones; corresponde a iteración 03-04.
- Tampoco incluye pruebas de integración con PostgreSQL/Testcontainers; corresponde a iteración 03-05.
- La propiedad `ContextoBT` permanece fuera del materializado EF actual (ya ignorada en configuraciones de 03-02), riesgo a tratar en iteraciones posteriores.
