# Iteración 02-04 — AplicarPenalizacion (Application)

**Fase:** 2 · **HU-16** · **RB-20**, **RB-24**, **RB-25**  
**Rama:** `feature/fase2-application`

---

## Objetivo

Orquestar `Sesion.AplicarPenalizacion` desde Application sin mover reglas al handler.

---

## Componentes

| Pieza | Ubicación |
|------|-----------|
| `AplicarPenalizacionCommand` | `Sesion/Commands/AplicarPenalizacion/` |
| `AplicarPenalizacionCommandHandler` | `ISesionRepository` + `IEventPublisher` |
| `AplicarPenalizacionValidator` | FluentValidation |

Retorno: `Result<Guid>` con el `SesionId`.

---

## Flujo del handler

1. Buscar sesión por id (`NotFoundException` si no existe).  
2. Construir `Penalizacion(puntos, motivo, operadorId)`.  
3. Ejecutar `sesion.AplicarPenalizacion(participanteId, penalizacion)`.  
4. Persistir y publicar eventos (`PublishBatchAsync`) y limpiar (`ClearDomainEvents`).

---

## Tests

- Handler: happy path, sesión inexistente, sesión no activa.  
- Validator: comando válido, puntos no positivos, motivo vacío.

Resultado acumulado: `dotnet test tests/Umbral.Application.Tests/Umbral.Application.Tests.csproj`.
