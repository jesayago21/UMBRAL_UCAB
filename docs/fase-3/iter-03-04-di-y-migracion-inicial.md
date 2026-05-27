# Iteración 03-04 — DI de persistencia y migración inicial

## Objetivo

Dejar la persistencia conectada al arranque de API mediante DI y generar la migración inicial de EF Core para la infraestructura.

## Cambios implementados

- Se creó `InfrastructureServiceCollectionExtensions` en Infrastructure para registrar:
  - `UmbralDbContext` con `UseNpgsql` leyendo `ConnectionStrings:Postgres`.
  - `ISesionRepository` y `IMisionRepository`.
  - `IEventPublisher` con implementación temporal `NoOpEventPublisher` para completar wiring de handlers.
- Se actualizó `Program.cs` de API para usar:
  - `AddApplication()`
  - `AddInfrastructure(builder.Configuration)`
- Se agregó `UmbralDbContextFactory` (`IDesignTimeDbContextFactory`) para soporte de tooling EF en diseño.
- Se generó migración inicial EF Core:
  - `20260527201758_InitialCreate.cs`
  - `20260527201758_InitialCreate.Designer.cs`
  - `UmbralDbContextModelSnapshot.cs`
- Se añadió paquete `Microsoft.EntityFrameworkCore.Design` en `Umbral.API.csproj` para habilitar comandos EF desde startup project.

## Validación

- `dotnet build Umbral.sln`
- `dotnet test Umbral.sln --no-build`
- Smoke de arranque API:
  - `dotnet run --project src/backend/Umbral.API --no-build`
  - `Invoke-RestMethod http://localhost:5000/health` => `{"status":"ok"}`
- Verificación de migración:
  - `dotnet ef migrations add InitialCreate ...` ✅
  - `dotnet ef migrations script ...` ✅
  - Aplicación del SQL generado dentro del contenedor PostgreSQL local ✅

## Notas

- `dotnet ef database update` desde host quedó bloqueado por autenticación en el servicio PostgreSQL resolviendo en `localhost:5432`; para no frenar la iteración se aplicó la migración vía SQL script directamente en el contenedor local.
- El publicador de eventos real (RabbitMQ/MassTransit) queda para una iteración específica de mensajería; en esta fase se usa `NoOpEventPublisher` para mantener el arranque funcional.
