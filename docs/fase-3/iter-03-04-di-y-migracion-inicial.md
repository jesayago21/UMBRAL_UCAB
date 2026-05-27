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
  - `dotnet ef database update ...` ✅ (después de estandarizar puerto `5433` y alinear `UmbralDbContextFactory`)

## Notas

- Se detectó conflicto local: coexistían Docker y PostgreSQL local escuchando en `5432`, lo que desviaba la conexión de `dotnet ef` al servidor incorrecto.
- Se estandarizó PostgreSQL Docker en `localhost:5433` para evitar colisión de puertos con instalaciones locales.
- Se ajustó la factory de diseño (`UmbralDbContextFactory`) para usar `5433` por defecto cuando no existe `ConnectionStrings__Postgres` en entorno.
- El publicador de eventos real (RabbitMQ/MassTransit) queda para una iteración específica de mensajería; en esta fase se usa `NoOpEventPublisher` para mantener el arranque funcional.

## Troubleshooting — credenciales PostgreSQL en local

Si `dotnet ef database update` falla con `password authentication failed for user "umbral_user"`:

1. Reiniciar el stack local de infraestructura:
   - `docker compose down -v`
   - `docker compose up -d postgres rabbitmq`
2. Verificar login dentro del contenedor:
   - `docker exec umbral_postgres psql -U umbral_user -d umbral_db -c "SELECT current_user;"`
3. Reintentar migración EF desde host:
   - `dotnet ef database update --project src/backend/Umbral.Infrastructure --startup-project src/backend/Umbral.API`
4. Verificar puertos/listeners:
   - `Get-NetTCPConnection -LocalPort 5432,5433 -State Listen`
5. Si hay PostgreSQL local en `5432`, mantener Docker en `5433` y validar `ConnectionStrings:Postgres` con puerto `5433`.
