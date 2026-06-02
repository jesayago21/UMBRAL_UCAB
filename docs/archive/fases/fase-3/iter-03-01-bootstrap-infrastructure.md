# Iteración 03-01 — Bootstrap Infrastructure + Persistence base

## Objetivo

Dejar preparado el cascarón funcional de `Umbral.Infrastructure` para soportar EF Core y la persistencia de agregados de `Sesion` y `Mision` en las siguientes iteraciones.

## Cambios implementados

- Actualización de `Umbral.Infrastructure.csproj` con paquetes base de persistencia:
  - `Microsoft.EntityFrameworkCore`
  - `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `Microsoft.EntityFrameworkCore.Design`
  - `EFCore.NamingConventions`
- Creación de `InfrastructureAssemblyMarker` para escaneo por assembly en infraestructura.
- Creación de `Persistence/UmbralDbContext` con `DbSet` mínimos para:
  - `Sesion` y entidades de soporte (`ParticipanteSesion`, `EventoSesion`, `Evidencia`)
  - `Mision` y su jerarquía (`Etapa`, `Pista`)
- Creación de estructura base de carpetas:
  - `Persistence/Configurations/`
  - `Persistence/Repositories/`
  - `Messaging/`

## Validación

- `dotnet build` de la solución.

## Notas

- Esta iteración no incluye configuraciones `IEntityTypeConfiguration<T>`, converters de IDs tipados ni implementación de repositorios; eso corresponde a iteraciones 03-02 y 03-03.
- No se añadieron reglas de negocio en infraestructura para mantener la separación de capas.
