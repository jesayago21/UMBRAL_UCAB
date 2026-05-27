# Fase 4 — Tracker API REST + integración HTTP

**Alcance:** `Umbral.API` (controllers, middleware, DTOs), `tests/Umbral.API.Tests` (WebApplicationFactory + Testcontainers)  
**Rama:** `feature/fase4-api`  
**Última actualización:** 2026-05-27 (iter-04-02)

---

## Leyenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | Iteración implementada, validada y documentada |
| 🔄 | Iteración en progreso |
| ⬜ | Iteración pendiente |

---

## Iteraciones

| Iter | Objetivo | Estado | Doc |
|------|----------|--------|-----|
| 04-01 | Bootstrap API tests, middleware errores, `Result`→HTTP | ✅ | [iter-04-01](iter-04-01-bootstrap-api-tests-y-errores.md) |
| 04-02 | `SesionesController`: crear BT, equipos, iniciar + tests | ✅ | [iter-04-02](iter-04-02-sesiones-crear-equipos-iniciar.md) |
| 04-03 | POST pausar / reanudar + tests | ⬜ | [iter-04-03](iter-04-03-sesiones-pausar-reanudar.md) |
| 04-04 | POST penalización / evidencia + tests | ⬜ | [iter-04-04](iter-04-04-sesiones-penalizacion-evidencia.md) |
| 04-05 | POST finalizar / cancelar, GET ranking + tests | ⬜ | [iter-04-05](iter-04-05-sesiones-cerrar-ranking.md) |
| 04-06 | `MisionesController` CRUD (HU-01..04) + tests | ⬜ | [iter-04-06](iter-04-06-crud-misiones.md) |

---

## Decisiones de fase

### Autenticación (Entrega 1)

**Opción A — auth de prueba (sin JWT real en 04-01..04-05):**

- `TestAuthHandler` registrado en entornos `Development` y `Testing`.
- Usuario ficticio con roles `Operador` y `Administrador` (`ClaimTypes.NameIdentifier` fijo).
- Controllers con `[Authorize]` en iteraciones 04-02+ usarán este esquema en tests vía `WebApplicationFactory` (`UseEnvironment("Testing")`).
- JWT de producción queda como deuda técnica documentada al cierre de fase.

### Formato de error

JSON alineado con `project-rules.md` §5.3: `tipo`, `mensaje`, `errores`, `traceId` (`ApiErrorResponse`).

---

## Progreso Fase 4

```
Iter 04-02 ██░░░  2/6 iteraciones (~33%)
```

---

## Validación mínima por iteración

```powershell
cd "C:\Users\sayag\OneDrive\Desktop\ProyectoDesarrollo\UMBRAL_UCAB"
dotnet build Umbral.sln
dotnet test Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

Tests con Testcontainers requieren **Docker en ejecución**.
