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

- `ContextoBusquedaTesoro` queda fuera del mapeo canónico de esta iteración por su complejidad de snapshot/VO; se mantiene ignorado en la configuración de `Sesion`.
- Esta iteración no implementa repositorios todavía; corresponde a `03-03`.
