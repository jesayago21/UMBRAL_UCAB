# Iteración 03-02 — Configuraciones EF Core y converters de IDs tipados

## Objetivo

Mapear entidades/agregados principales de `Sesion` y `Mision` en EF Core, respetando IDs tipados y convenciones de persistencia.

## Cambios implementados

- Se extendió `UmbralDbContext` para registrar converters globales de IDs tipados en `ConfigureConventions`.
- Se crearon `ValueConverters` para:
  - `MisionId`, `EtapaId`, `PistaId`
  - `SesionId`, `EquipoId`, `EvidenciaId`, `UsuarioId`
- Se crearon configuraciones EF Core (`IEntityTypeConfiguration<T>`) para:
  - `Sesion`
  - `EquipoSesion`
  - `EventoSesion`
  - `Evidencia`
  - `Mision`
  - `Etapa`
  - `Pista`
- Enums mapeados como `string` según convención.
- Se ignoró `DomainEvents` del agregado `Sesion` y `Mision`.
- Se definieron relaciones por backing fields para colecciones internas del agregado (`_equipos`, `_historialEventos`, `_evidencias`, `_etapas`, `_pistas`).

## Validación

- `dotnet build Umbral.sln`
- `dotnet test Umbral.sln --no-build`

## Notas

- `ContextoBusquedaTesoro` se dejó ignorado en 03-02 por complejidad del snapshot; **persistencia cerrada en Fase 4** ([iter-04-02b](../fase-4/iter-04-02b-contexto-bt-persistencia.md)).
- Esta iteración no implementa repositorios todavía; corresponde a `03-03`.
