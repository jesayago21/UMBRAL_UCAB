# Iteración 02-02 — RegistrarEquipo (Application)

**Fase:** 2 (Application + tests)  
**Fecha:** 2026-05-27  
**Rama:** `feature/fase2-application`  
**Fuentes:** Fase 1 [iter-02](../fase-1/iter-02-registrar-participante.md), **HU-13**, **RB-02**, **RB-03**

---

## Objetivo

Orquestar `Sesion.RegistrarEquipo` devolviendo `ParticipanteId` y `CodigoAcceso` al operador.

---

## Artefactos

| Componente | Ubicación |
|------------|-----------|
| `RegistrarEquipoCommand` | `Sesion/Commands/RegistrarEquipo/` |
| `RegistrarEquipoResult` | `ParticipanteId` + `CodigoAcceso` |
| `RegistrarEquipoCommandHandler` | `ISesionRepository` + `IEventPublisher` |
| `RegistrarEquipoValidator` | GUID sesión + nombre obligatorios |

**Tests:** 4 handler + 3 validator (NSubstitute).

---

## Flujo del handler

1. `FindByIdAsync` → `NotFoundException`  
2. `sesion.RegistrarEquipo(nombre)` — reglas RB-02/RB-03 en dominio  
3. `SaveAsync` → `PublishBatchAsync` → `ClearDomainEvents`  
4. `Result<RegistrarEquipoResult>.Ok(...)`

---

## Criterios (RB-02-02)

| ID | Criterio | Test |
|----|----------|------|
| RB-02-02-01 | Nombre único → participante + código | `Handle_CuandoSesionExisteYNombreUnico_*` |
| RB-02-02-02 | Sesión inexistente → `NotFoundException` | `Handle_CuandoSesionNoExiste_*` |
| RB-02-02-03 | Duplicado → `DomainException` | `Handle_CuandoNombreDuplicado_*` |
| RB-02-02-04 | Sesión cerrada → `DomainException` | `Handle_CuandoSesionFinalizada_*` |

---

## Verificación

```bash
dotnet test tests/Umbral.Application.Tests/Umbral.Application.Tests.csproj
```
