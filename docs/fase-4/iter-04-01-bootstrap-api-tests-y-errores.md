# Iteración 04-01 — Bootstrap API tests y manejo global de errores

## Objetivo

Preparar la capa API para controllers delgados: proyecto de tests de integración, middleware de excepciones con contrato JSON estándar, extensión `Result<T>`→HTTP y auth de prueba sin JWT.

## Cambios implementados

- Proyecto `tests/Umbral.API.Tests` con `WebApplicationFactory<Program>` y PostgreSQL efímero (Testcontainers).
- `ExceptionHandlingMiddleware` mapea:
  - `FluentValidation.ValidationException` → 400 `ValidationError`
  - `DomainException` → 400 `DomainError`
  - `NotFoundException` → 404 `NotFound`
  - `UnauthorizedAccessException` → 401 `Unauthorized`
  - Otros → 500 `InternalServerError`
- `ApiErrorResponse` (`tipo`, `mensaje`, `errores`, `traceId`).
- `ResultExtensions` para respuestas de commands con `Result<T>`.
- `TestAuthHandler` en `Development` / `Testing` (sin JWT).
- Rutas `/__test/errors/*` solo en Development/Testing para validar el middleware.
- `docs/fase-4/TRACKER.md` y placeholders de iteraciones 04-02..04-06.

## Validación

```powershell
dotnet build Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

## Notas

- Sin controllers de negocio aún; iteración 04-02 agrega `SesionesController`.
- `AddProblemDetails()` registrado; el contrato expuesto al cliente sigue `ApiErrorResponse` por alineación con project-rules.
- `ContextoBT` y JWT productivo quedan fuera de esta iteración.
