# AGENTS.md — UMBRAL

Monolito hexagonal .NET 8 (Clean Architecture) + React 19 + Vite 8 + Keycloak.

## Backend commands

```bash
dotnet build Umbral.sln -c Release
dotnet test Umbral.sln -c Release
dotnet test tests/Umbral.Domain.Tests                          # unit
dotnet test tests/Umbral.Application.Tests                     # unit
dotnet test tests/Umbral.Infrastructure.Tests                  # needs Docker (Testcontainers)
dotnet test tests/Umbral.API.Tests                             # needs Docker (Testcontainers)
.\scripts\run-coverage.ps1 -Threshold 90 -Open                 # Windows: coverage + report
bash scripts/run-coverage.sh --threshold 90                    # Linux/macOS/CI
dotnet watch run --project src/backend/Umbral.API              # dev server (auto-migrates Dev)
dotnet run --project src/backend/Umbral.Gateway                # YARP gateway :8000 → API :5000
```

Docker must be running for `Infrastructure` and `API` test projects (they spin up ephemeral Postgres via Testcontainers).

## Frontend commands

```bash
cd src/frontend/umbral-web
cp .env.example .env && npm install && npm run dev     # :5173
npm run lint                                            # ESLint
npm run build                                           # tsc -b && vite build
```

Frontend uses `@/` path alias, Tailwind v4, `react-oidc-context`, Zustand, TanStack Query. No test runner is configured yet. The web app is for Admin + Operador only; the Participant role will move to a React Native (Expo) app in E2.

## Layer structure

| Project | Role |
|---------|------|
| `Umbral.Domain` | Aggregates, VOs, domain events, port interfaces. Zero deps. |
| `Umbral.Application` | MediatR handlers + FluentValidation validators. `internal sealed` handlers (`InternalsVisibleTo` for tests). |
| `Umbral.Infrastructure` | EF Core (Npgsql), Keycloak HTTP client, `NoOpEventPublisher`. References `Application` + `Domain`. |
| `Umbral.API` | Thin controllers, exception middleware, auth wiring (JWT Bearer / TestAuthHandler). |
| `Umbral.Gateway` | YARP reverse proxy (edge). Enruta `/api/v1/**` al monolito. Sin lógica de negocio ni referencias a Domain/Application. |

Dependency direction: `API → Application → Domain` and `Infrastructure → Application → Domain`. Infrastructure is never referenced by Domain or Application. `Umbral.Gateway` es un **deployable de borde** frente al mismo monolito (no es microservicio).

## API Gateway (YARP)

| Puerto | Servicio | Rol |
|--------|----------|-----|
| 8000 | `Umbral.Gateway` | Entrada pública REST (`/api/v1/**`) |
| 5000 | `Umbral.API` | Monolito hexagonal (acceso directo en dev) |

- Config: `src/backend/Umbral.Gateway/appsettings.json` (`ReverseProxy:Routes` + `Clusters`).
- Health del gateway: `GET http://localhost:8000/health` (no proxy).
- Frontend dev: `VITE_API_URL=http://localhost:8000` (REST vía gateway; SignalR sigue en `:5000` cuando se active).
- Docker override cluster: `ReverseProxy__Clusters__umbral-api__Destinations__api__Address=http://api:5000/`

## Domain patterns

- Aggregates use `private` constructors + `static Crear()` factory methods.
- Collections exposed as `IReadOnlyList<T>`; no public setters on aggregate properties.
- All IDs are strongly-typed VOs (`SesionId`, `PreguntaId`, etc.) with EF value converters in `UmbralDbContext.ConfigureConventions`.
- Business invariants enforced inside aggregate methods, never in handlers.
- Domain events: `RaiseDomainEvent()` queues them; `ClearDomainEvents()` called after dispatch. `IEventPublisher` is `NoOpEventPublisher` (real RabbitMQ deferred to E2).

## Application patterns

- Commands modify state → return `Result<T>` (never throw for business errors).
- Queries read only → return DTOs, `AsNoTracking()`.
- `ValidationBehavior<TRequest,TResponse>` auto-runs FluentValidation validators in the MediatR pipeline.
- API maps `Result<T>` → HTTP via `ResultExtensions`.

## API quirks

- `TestAuthHandler` active in `Development` and `Testing` environments (bypasses JWT). Real auth uses Keycloak JWT Bearer (realm `umbral`, client `umbral-api`).
- `ExceptionHandlingMiddleware`: `DomainException` → 400, `NotFoundException` → 404, `ValidationException` → 400 with field errors.
- `/health` endpoint available without auth.
- EF migrations auto-applied in `Development` on startup at `Program.cs:28-30`.
- `EquipoParticipante` realm role is aliased to `Participante` claim during JWT validation (`ApiServiceCollectionExtensions.cs:104-108`).

## Infrastructure services (Docker)

| Service | Port | Notes |
|---------|------|-------|
| PostgreSQL 16 | 5433 | Avoids conflict with local 5432 |
| RabbitMQ 3.13 | 5672 / 15672 | Management UI :15672 |
| Keycloak 24 | 8080 | Realm auto-imported from `docker/keycloak/umbral-realm.json` |
| API Gateway (YARP) | 8000 | `Umbral.Gateway` — REST `/api/v1/**` (opcional en dev local) |

Stack completo con API + gateway en Docker:

```bash
docker compose up postgres rabbitmq keycloak api gateway -d
# REST: http://localhost:8000/api/v1/...
```

Demo users: `admin`/`Umbral123!` (Admin), `operador`/`Umbral123!` (Operador), `participante`/`Umbral123!` (Participante). If missing, run `.\scripts\keycloak-ensure-demo-users.ps1`.

## Testing quirks

- Infrastructure/API tests require Docker (Testcontainers Postgres 16 alpine). The `PostgresFixture` manages the container lifecycle.
- Always call `dbContext.ChangeTracker.Clear()` before re-reading entities after a write in repository tests.
- `UmbralWebAppFactory` (API tests) bootstraps a full web host with Testcontainers Postgres + `TestAuthHandler`.
- Test naming convention: `MetodoOComportamiento_Escenario_ResultadoEsperado`.
- Coverage gate: ≥90% per assembly (Domain, Application, Infrastructure, API). CI enforces via `run-coverage.sh --threshold 90`.

## CI

GitHub Actions runs on push/PR to `main`, `develop`, `feature/**`. Steps: restore → build Release → `run-coverage.sh --threshold 90`. Coverage report uploaded as artifact `coverage-report` (14 days). No `.github/workflows/` workflow files are checked into the repo (they may live on the default branch only).

## Participants in web (E1)

`VITE_PARTICIPANTE_WEB_ENABLED=true` lets participants use the web app at `/participante`. Set to `false` when the React Native mobile app (`umbral-mobile`) is ready (E2).
