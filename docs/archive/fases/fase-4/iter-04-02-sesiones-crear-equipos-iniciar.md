# Iteración 04-02 — Sesiones: crear, participantes e iniciar

## Objetivo

Exponer el flujo mínimo de operador vía REST: crear sesión BT, registrar participante e iniciar sesión, con tests de integración por endpoint.

## Cambios implementados

- `SesionesController` (`/api/v1/sesiones`):
  - `POST busqueda-tesoro` → `CrearSesionBusquedaTesoroCommand`
  - `POST {id}/participantes` → `RegistrarEquipoCommand`
  - `POST {id}/iniciar` → `IniciarSesionCommand` (204)
- DTOs en `Umbral.API/Contracts/Sesiones/`.
- `OperadorId` desde claim JWT/test (`TestAuthHandler`), no desde el body.
- **Application:** `RegistrarEquipoCommandHandler` llama `AbrirParaRegistro()` si la sesión está en `Programada` (permite flujo Crear → Registrar → Iniciar sin endpoint extra).
- `ResultExtensions` recibe `HttpContext` para `traceId` en errores de negocio.
- Tests API: feliz + validación/not found por endpoint + flujo integrado.

## Rutas

| Método | Ruta | Código éxito |
|--------|------|--------------|
| POST | `/api/v1/sesiones/busqueda-tesoro` | 201 |
| POST | `/api/v1/sesiones/{id}/participantes` | 201 |
| POST | `/api/v1/sesiones/{id}/iniciar` | 204 |

## Validación

```powershell
dotnet build Umbral.sln
dotnet test Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

## Notas

- Auth: `[Authorize(Roles = "Operador,Administrador")]` con `TestAuthHandler` en Testing/Development.
- Sin `GET` sesión por id en esta iteración (04-05+ si aplica).
- **Infrastructure:** `SesionRepository` usa `AsNoTracking` en lecturas y `ExecuteUpdate` + inserción explícita de hijos en actualizaciones. Persistencia de `ContextoBT`: ver [iter-04-02b](iter-04-02b-contexto-bt-persistencia.md).
- Test infra: `SesionRegistrarEquipoPersistenceTests` (dos pasos crear + registrar participante).
