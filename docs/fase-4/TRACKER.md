# Fase 4 — Tracker API REST + integración HTTP

**Alcance:** `Umbral.API` (controllers, middleware, DTOs), `tests/Umbral.API.Tests` (WebApplicationFactory + Testcontainers)  
**Rama:** `feature/fase4-api`  
**Última actualización:** 2026-05-28 (plan auth 04-05b)

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
| 04-02b | Persistencia `ContextoBT` (cierre deuda Fase 3) | ✅ | [iter-04-02b](iter-04-02b-contexto-bt-persistencia.md) |
| 04-03 | POST pausar / reanudar + tests | ✅ | [iter-04-03](iter-04-03-sesiones-pausar-reanudar.md) |
| 04-04 | POST penalización / evidencia + tests | ⬜ | [iter-04-04](iter-04-04-sesiones-penalizacion-evidencia.md) |
| 04-05 | POST finalizar / cancelar, GET ranking + tests | ⬜ | [iter-04-05](iter-04-05-sesiones-cerrar-ranking.md) |
| **04-05b** | **Login, usuarios en BD, JWT por rol (demo profesor)** | ⬜ | [iter-04-05b](iter-04-05b-auth-login-usuarios.md) |
| 04-06 | `MisionesController` CRUD (HU-01..04) + tests | ⬜ | [iter-04-06](iter-04-06-crud-misiones.md) |

---

## Decisiones de fase

### Autenticación — dos fases

| Fase | Cuándo | Qué |
|------|--------|-----|
| **A — Desarrollo rápido** | 04-01 … 04-05 (hecho / en curso) | `TestAuthHandler`: un usuario ficticio con **ambos** roles; sin BD de usuarios; válido para avanzar endpoints de sesión. |
| **B — Demo y diseño real** | **04-05b** (antes de 04-06 y muestra al profesor) | Tabla `usuarios`, BCrypt, `POST /auth/login`, JWT con **un rol por usuario**, seed admin + operador; **sí modifica BD e Infrastructure**. |
| **Producción** | Tras 04-05b | Mismo esquema JWT; sin `TestAuthHandler` en `Production`. |

**Para la muestra al profesor hace falta la fase B:** login distinto admin/operador y poder mostrar 403 por rol (p. ej. operador no crea misiones).

Detalle: [iter-04-05b-auth-login-usuarios.md](iter-04-05b-auth-login-usuarios.md).

### Formato de error

JSON alineado con `project-rules.md` §5.3: `tipo`, `mensaje`, `errores`, `traceId` (`ApiErrorResponse`).

---

## Progreso Fase 4

```
Sesión API   ████░  04-01..03 ✅ | 04-04..05 ⬜
Auth real    ░░░░░  04-05b ⬜ (requerido antes de 04-06 y demo)
Misiones API ░░░░░  04-06 ⬜
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
