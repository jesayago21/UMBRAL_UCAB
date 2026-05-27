# Iteración 02-01 — CrearSesionBusquedaTesoro (Application)

**Fase:** 2 (Application + tests)  
**Fecha:** 2026-05-27  
**Rama:** `feature/fase2-application`  
**Fuentes:** `umbral-backend-spec.md` §3.1, `cqrs-mediatr-skill.md`, `umbral-quality-spec.md` §6, **HU-12**, **RB-01**

---

## Objetivo

Orquestar `Sesion.CrearBusquedaTesoro` desde la capa Application sin duplicar reglas de dominio: cargar misión, validar que esté activa (RB-01), crear snapshot ACL, persistir y publicar eventos.

---

## Artefactos

| Componente | Ubicación |
|------------|-----------|
| `CrearSesionBusquedaTesoroCommand` | `Sesion/Commands/CrearSesionBusquedaTesoro/` |
| `CrearSesionBusquedaTesoroCommandHandler` | internal sealed |
| `CrearSesionBusquedaTesoroValidator` | FluentValidation |
| `Result<T>`, `NotFoundException` | `Common/` |
| `ValidationBehavior` | `Common/Behaviors/` |
| `AddApplication()` | `DependencyInjection/` |

**Tests:** `tests/Umbral.Application.Tests` — NSubstitute, 3 handler + 3 validator.

---

## Flujo del handler

1. `IMisionRepository.FindByIdAsync` → `NotFoundException` si null  
2. `mision.PuedeUsarseParaSesion()` → `DomainException` si false (RB-01)  
3. `MisionSnapshot.Desde(mision)` + `Sesion.CrearBusquedaTesoro`  
4. `ISesionRepository.SaveAsync`  
5. `IEventPublisher.PublishBatchAsync(sesion.DomainEvents)`  
6. `sesion.ClearDomainEvents()`  
7. `Result<Guid>.Ok(sesion.SesionId.Valor)`

---

## Criterios de aceptación (RB-02-01)

| ID | Criterio | Test |
|----|----------|------|
| RB-02-01-01 | Misión activa → sesión `Programada` + `SesionCreada` publicado | `Handle_CuandoMisionActiva_*` |
| RB-02-01-02 | Misión inexistente → `NotFoundException`, sin persistir | `Handle_CuandoMisionNoExiste_*` |
| RB-02-01-03 | Misión borrador → `DomainException`, sin persistir | `Handle_CuandoMisionInactiva_*` |
| RB-02-01-04 | GUIDs vacíos rechazados en validator | `Validar_*` |

---

## Fuera de alcance (esta iteración)

- `Umbral.Infrastructure` / `Umbral.API`  
- Behaviors `Logging` / `Performance`  
- Registro en `Program.cs` (iteración Infra/API posterior)

---

## Comando de verificación

```bash
dotnet test tests/Umbral.Application.Tests/Umbral.Application.Tests.csproj
```
