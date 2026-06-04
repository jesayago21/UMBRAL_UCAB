# Iteración 03-05 — Tests de infraestructura (Testcontainers)

## Objetivo

Validar `SesionRepository` y `MisionRepository` contra PostgreSQL real usando Testcontainers, sin depender del `docker-compose` local del desarrollador.

## Cambios implementados

- Proyecto `tests/Umbral.Infrastructure.Tests` agregado a `Umbral.sln`.
- Fixture compartida `PostgresFixture` (`postgres:16-alpine`) que:
  - levanta un contenedor efímero,
  - aplica `Database.MigrateAsync()` con las migraciones de Infrastructure.
- Colección xUnit `PostgresCollection` para reutilizar el contenedor entre clases de test.
- Pruebas de repositorio:
  - `SesionRepositoryTests`: persistencia + `FindById` con equipos; `FindActivas` filtra por estado.
  - `MisionRepositoryTests`: persistencia + `FindById` con etapas/pistas; `FindActivas` filtra por estado.
- Helpers de dominio en `DomainTestData` para construir agregados válidos sin acoplar a `Umbral.Domain.Tests`.

## Validación

Requisito: **Docker Desktop** (o motor Docker compatible) en ejecución.

```powershell
dotnet build Umbral.sln
dotnet test Umbral.sln --filter "FullyQualifiedName~Umbral.Infrastructure.Tests"
```

Solo infraestructura (más rápido):

```powershell
dotnet test tests/Umbral.Infrastructure.Tests/Umbral.Infrastructure.Tests.csproj
```

Suite completa:

```powershell
dotnet test Umbral.sln
```

## Notas

- Los tests no usan el puerto `5433` de `docker-compose`; cada ejecución obtiene su propio PostgreSQL en un puerto aleatorio del contenedor.
- `ContextoBT` sigue ignorado en EF (gap conocido desde 03-02/03-04); las pruebas cubren columnas y relaciones materializadas actuales.
- El publicador RabbitMQ real queda fuera de alcance; no se prueba mensajería en esta iteración.
