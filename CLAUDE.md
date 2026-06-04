# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (.NET 8)

```bash
# Build
dotnet build Umbral.sln -c Release

# Run all tests
dotnet test Umbral.sln -c Release

# Run a single test project
dotnet test tests/Umbral.Domain.Tests
dotnet test tests/Umbral.Application.Tests
dotnet test tests/Umbral.Infrastructure.Tests   # requires Docker (Testcontainers)
dotnet test tests/Umbral.API.Tests              # requires Docker (Testcontainers)

# Coverage with gate (≥90% per assembly, CI standard)
.\scripts\run-coverage.ps1 -Threshold 90        # Windows
bash scripts/run-coverage.sh --threshold 90     # Linux/macOS/CI

# Run API locally (auto-applies migrations in Development)
dotnet watch run --project src/backend/Umbral.API

# EF Core migrations
dotnet ef migrations add <Name> --project src/backend/Umbral.Infrastructure --startup-project src/backend/Umbral.API
dotnet ef database update --project src/backend/Umbral.Infrastructure --startup-project src/backend/Umbral.API
```

### Frontend (React + Vite, TypeScript)

```bash
cd src/frontend/umbral-web
npm install
npm run dev           # Vite dev server on :5173
npm run build
npm run lint
```

### Infrastructure (Docker)

```bash
# Start dependencies only (for local backend dev)
docker compose up postgres rabbitmq keycloak -d

# Full stack
docker compose up -d

# Teardown (keeps volumes)
docker compose down
```

## Architecture

### Clean Architecture — four backend layers

```
Umbral.Domain          → Aggregates, Value Objects, Domain Events, ports (interfaces)
Umbral.Application     → MediatR handlers, FluentValidation validators, Result<T>
Umbral.Infrastructure  → EF Core (Npgsql/PostgreSQL), Keycloak HTTP client, repositories
Umbral.API             → ASP.NET Core 8 controllers, exception middleware, auth wiring
```

Dependency rule: each layer only references layers to its left.

### Domain model — key aggregates

| Aggregate | Location | Notes |
|-----------|----------|-------|
| `Sesion` | `Domain/Sesion/` | Polymorphic contexts (BusquedaTesoro, Trivia, Mision). All session lifecycle lives here. |
| `Mision` | `Domain/CatalogoMision/` | Composed of `Etapa` + `Pista` entities. |
| `Pregunta` | `Domain/CatalogoTrivia/Pregunta/` | AR for trivia questions; ≥3 options, exactly one correct. |
| `Categoria` | `Domain/CatalogoTrivia/Categoria/` | Groups questions. |
| `UsuarioAdministrable` | `Domain/IdentidadYAccesos/` | Mirror of Keycloak users; synced via `UsuariosEspejoSeeder`. |

All IDs are strongly-typed value objects (e.g. `SesionId`, `PreguntaId`) with EF value converters registered in `UmbralDbContext.ConfigureConventions`.

### Application layer patterns

- Every command/query is handled by a MediatR `IRequestHandler`.
- `ValidationBehavior<TRequest, TResponse>` runs all FluentValidation validators in the pipeline automatically.
- Handlers are `internal sealed`. The `InternalsVisibleTo` attribute exposes them to test projects.
- Command handlers return `Result<T>` (never throw for business errors). API layer maps `Result<T>` → HTTP via `ResultExtensions`.
- Query handlers use `AsNoTracking()`.

### Infrastructure patterns

- `UmbralDbContext` applies all `IEntityTypeConfiguration<T>` via `ApplyConfigurationsFromAssembly`.
- Enum columns stored as strings. Domain events are `Ignore()`d in EF configurations.
- Repository tests use Testcontainers (Postgres 16 alpine) via `PostgresFixture`; always call `ChangeTracker.Clear()` before re-reading from the DB in tests.
- `KeycloakIdentityService` is an `HttpClient`-backed service registered via `AddHttpClient<>`.

### API layer patterns

- Controllers are thin: parse HTTP input → send MediatR command/query → map result → HTTP response.
- `ExceptionHandlingMiddleware` maps `DomainException` → 400, `NotFoundException` → 404, `ValidationException` → 400 (field errors), unhandled → 500.
- Authentication: Keycloak JWT Bearer (realm `umbral`, client `umbral-api`). Configured via `Keycloak__Authority` and `Keycloak__Audience` environment variables.
- `TestAuthHandler` is active in `Development` and `Testing` environments only, enabling unauthenticated local calls.
- `UmbralWebAppFactory` (in `Umbral.API.Tests`) bootstraps integration tests against a real Testcontainers Postgres and wires a test auth handler.

### Frontend (React + TypeScript)

- Entry: `src/frontend/umbral-web/src/main.tsx`
- Auth: `react-oidc-context` + Keycloak OIDC. `OidcAuthBridge` syncs the OIDC token into `authStore` (Zustand).
- HTTP: Axios `apiClient` (`src/services/apiClient.ts`) attaches Bearer token from `authStore` and redirects to `/login` on 401.
- Server state: TanStack Query (`QueryClient` in `App.tsx`).
- Routes: `AppRouter.tsx` with per-role layouts (`AdminLayout`, `OperadorLayout`, `ParticipanteLayout`, `TriviaLayout`).
- URL: Vite env var `VITE_API_URL` (defaults to `http://localhost:5000`).

### Infrastructure services

| Service | Port | Purpose |
|---------|------|---------|
| PostgreSQL 16 | 5433 (local) | Primary database |
| RabbitMQ 3.13 | 5672 / 15672 | Message broker (management UI at :15672) |
| Keycloak 24 | 8080 | OIDC identity provider (realm `umbral` auto-imported from `docker/keycloak/`) |

### CI (GitHub Actions)

Pipeline runs on push/PR to `main`, `develop`, `feature/**`. Steps: restore → build Release → `run-coverage.sh --threshold 90`. Infrastructure and API tests require Docker (Testcontainers). Coverage report uploaded as artifact.

## Domain rules / constraints

- Aggregates use private constructors + static `Crear()` factory methods.
- Collections are exposed as `IReadOnlyList<T>`; no public setters on aggregate properties.
- Business invariants are enforced inside the aggregate, not in handlers.
- `AggregateRoot.RaiseDomainEvent()` queues domain events; `ClearDomainEvents()` is called after dispatch in handlers.
- `IEventPublisher` is currently a `NoOpEventPublisher` (RabbitMQ wiring deferred).
