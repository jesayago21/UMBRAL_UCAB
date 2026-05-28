# Iteración 04-05b — Login, usuarios y JWT por rol (demo profesor)

> **Estado:** ⬜ Pendiente — **obligatoria antes de 04-06 (misiones)** y antes de la muestra al profesor.

## Por qué no basta `TestAuthHandler`

`TestAuthHandler` asigna **Operador + Administrador al mismo usuario ficticio**. No permite demostrar:

- Login como admin vs operador
- `403 Forbidden` si un operador llama `POST /misiones`
- `OperadorId` real distinto por usuario en `CrearSesion`

El diseño canónico (`project-rules` §10, `backend-spec` §7) exige **usuarios en BD**, **login** y **JWT** con claim `role` y `sub` (userId).

## Cuándo se hace

| Momento | Iteración |
|---------|-----------|
| Después de cerrar flujo de sesión API (04-04, 04-05) | — |
| **Antes de** `MisionesController` (04-06) | **04-05b** |
| Antes de demo / entrega académica | **04-05b** |

Orden recomendado: `04-04` → `04-05` → **`04-05b` (auth)** → `04-06` (misiones solo Admin).

## Alcance técnico (sí toca BD e Infrastructure)

### Base de datos (nueva migración EF)

Tabla **`usuarios`** (nombre orientativo):

| Columna | Tipo | Notas |
|---------|------|--------|
| `id` | uuid PK | = `UsuarioId` / claim `sub` |
| `email` | varchar unique | login |
| `password_hash` | varchar | BCrypt (`project-rules` §10.4) |
| `rol` | varchar | `Administrador` \| `Operador` (enum string) |
| `activo` | bool | opcional |

**No** reemplaza `sesiones.operador_id`: sigue guardando quién creó la sesión; el valor vendrá del JWT del operador logueado.

Seed de desarrollo/demo (migración o script):

- `admin@umbral.local` → rol **Administrador**
- `operador@umbral.local` → rol **Operador**

### Infrastructure

- `Persistence/Configurations/UsuarioConfiguration.cs`
- `Persistence/Repositories/UsuarioRepository.cs` (o puerto en Domain si se define `IUsuarioRepository`)
- `Identity/` (o `Auth/`): emisión/validación JWT, BCrypt
- DI en `InfrastructureServiceCollectionExtensions`

### API

- `POST /api/v1/auth/login` — **público** (`[AllowAnonymous]`)
- Request: email + password → Response: access token (JWT)
- `Program.cs`: `AddAuthentication(JwtBearer)` en todos los entornos de demo; **retirar o desactivar** `TestAuthHandler` cuando exista JWT (tests pueden usar `TestAuthHandler` solo en `Testing` con helper que emite token, o login real en fixture)
- Controllers existentes: siguen `[Authorize(Roles = ...)]`; sin token → **401**

### Application

- Opcional: `LoginCommand` + handler (o servicio en Infrastructure invocado desde controller delgado)
- `ICurrentUserService` (spec): leer `sub` y `role` del `HttpContext` para commands que lo necesiten

### Tests

- Login OK → token con rol correcto
- Operador con token → `POST /misiones` → **403** (cuando exista 04-06)
- Admin con token → `POST /misiones` → **201**
- Tests de integración: obtener token vía login en fixture (no dual-role fake)

## Equipo participante (mobile)

Para la muestra web (admin/operador), **04-05b** basta con Admin + Operador.

`EquipoParticipante` (JWT con `sesionId` + código acceso) puede quedar para iteración posterior (evidencia mobile / 04-04 con auth de equipo), salvo que el profesor exija ese flujo en la misma demo.

## Validación demo (checklist profesor)

1. Login operador → crear sesión BT → equipo → iniciar.
2. Login admin → crear/editar misión (04-06).
3. Mismo token operador → `POST /misiones` → **403** (si aplica).
4. Sin token → **401** en endpoint protegido.

## Commit sugerido

`feat(api): iter-04-05b login JWT usuarios y roles`
