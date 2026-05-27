# Iteración 02-05 — SubmitEvidencia (Application)

**Fase:** 2 · **HU-18** · **RB-06**, **RB-19**, **RB-22**  
**Rama:** `feature/fase2-application`

---

## Objetivo

Orquestar `Sesion.RegistrarEvidencia` para el flujo del equipo participante en sesiones BT activas.

---

## Componentes

| Pieza | Ubicación |
|------|-----------|
| `SubmitEvidenciaCommand` | `Sesion/Commands/SubmitEvidencia/` |
| `SubmitEvidenciaCommandHandler` | `ISesionRepository` + `IEventPublisher` |
| `SubmitEvidenciaValidator` | FluentValidation |
| `SubmitEvidenciaResult` | `EvidenciaId` + `ResultadoValidacion` |

---

## Flujo del handler

1. Buscar sesión por id (`NotFoundException` si no existe).  
2. Ejecutar `sesion.RegistrarEvidencia(equipoId, codigoQr)`.  
3. Persistir, publicar `DomainEvents`, limpiar eventos.  
4. Retornar `Result<SubmitEvidenciaResult>`.

---

## Tests

- Handler: happy path, sesión no encontrada, error de dominio.  
- Validator: comando válido, `EquipoId` vacío, `CodigoQr` vacío.
