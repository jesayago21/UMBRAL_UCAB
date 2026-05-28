# Iteración 04-03 — Sesiones: pausar y reanudar

## Objetivo

Exponer HU-15 vía REST: pausar y reanudar sesión activa, con tests de integración.

## Cambios implementados

- `SesionesController`:
  - `POST /api/v1/sesiones/{id}/pausar` → `PausarSesionCommand` (204)
  - `POST /api/v1/sesiones/{id}/reanudar` → `ReanudarSesionCommand` (204)
- Tests API: feliz, dominio inválido y not found por endpoint.

## Validación

```powershell
dotnet build Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

## Notas

- Auth local: `TestAuthHandler` en Development/Testing (sin JWT).
- Prerrequisito pausar: sesión en estado `Activa` (crear → equipo → iniciar).
