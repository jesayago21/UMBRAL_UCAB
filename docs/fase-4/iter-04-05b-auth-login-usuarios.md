# Iteración 04-05b — Login, usuarios y JWT por rol (demo profesor)

> **Estado:** ✅ Implementada y validada.

## Objetivo

Reemplazar el esquema temporal de autenticación para demo (`TestAuthHandler` con doble rol) por un flujo real:

- usuarios persistidos en BD
- login por email/password
- JWT firmado con rol por usuario

Esto deja lista la base para la 04-06, donde `MisionesController` exigirá `Administrador`.

---

## Cambios implementados

### 1) Base de datos

Se agregó la tabla `usuarios` con migración EF:

- `src/backend/Umbral.Infrastructure/Persistence/Migrations/20260528013932_AddUsuariosAuth.cs`

Campos:

| Columna | Tipo | Notas |
|---------|------|-------|
| `id` | `uuid` | PK |
| `email` | `varchar(200)` | único |
| `password_hash` | `varchar(200)` | BCrypt |
| `rol` | `varchar(50)` | `Administrador` / `Operador` / `EquipoParticipante` |
| `activo` | `bool` | default `true` |

### 2) Infrastructure (repositorio + seed)

Se incorporó capa de auth en Infrastructure:

- `IUsuarioAuthRepository` + `UsuarioAuthRepository` para búsqueda por email.
- `DemoUsersSeeder` para crear usuarios de demo con password BCrypt.
- `SeedDemoUsersAsync(...)` para bootstrap automático en `Development` y `Testing`.

Credenciales de demo sembradas:

- `admin@umbral.local` / `Umbral123!` → `Administrador`
- `operador@umbral.local` / `Umbral123!` → `Operador`
- `equipo@umbral.local` / `Umbral123!` → `EquipoParticipante`

### 3) API (JWT + login)

Se agregó JWT bearer como esquema principal fuera de `Testing`:

- `ApiServiceCollectionExtensions` ahora configura:
  - `JwtBearer` en `Development` / `Production`
  - `TestAuthHandler` únicamente en `Testing`

Nuevas piezas:

- `JwtOptions`
- `JwtTokenIssuer`
- `AuthController` con `POST /api/v1/auth/login` (`[AllowAnonymous]`)
- contratos `LoginRequest` / `LoginResponse`

`Program.cs` ahora:

- siempre ejecuta `UseAuthentication()` + `UseAuthorization()`
- si el entorno permite seed, migra y siembra usuarios demo al iniciar

---

## Flujo de login

1. `POST /api/v1/auth/login` recibe email/password.
2. API busca usuario en `usuarios` por email normalizado.
3. Valida hash con `BCrypt.Verify`.
4. Emite JWT con claims:
   - `sub` y `nameidentifier`: id usuario
   - `email`
   - `role`
5. Retorna `200 OK` con:
   - `accessToken`
   - `tokenType = Bearer`
   - `expiresIn`
   - `role`

Si credenciales inválidas o usuario inactivo: `401 Unauthorized`.

---

## Compatibilidad con iteraciones previas

- Los endpoints de sesión siguen igual (`[Authorize(Roles = ...)]`).
- En pruebas de integración se mantiene `TestAuthHandler` (`Testing`) para no romper el suite existente.
- En ejecución normal (dev/prod), el acceso protegido requiere token JWT real.

---

## Tests y validación

Se añadieron pruebas de integración para auth:

- `POST_login_CredencialesValidas_RetornaToken`
- `POST_login_PasswordInvalida_Retorna401`

Comandos ejecutados:

```powershell
dotnet build Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

Resultado: **40/40 tests API en verde**.

---

## Checklist demo (actualizado)

1. Login como operador y administrar sesión BT.
2. Login como admin para operaciones administrativas (04-06).
3. Sin token en endpoint protegido ⇒ `401`.
4. Con token de rol no autorizado (cuando esté 04-06) ⇒ `403`.

---

## Commit sugerido

`feat(api): iter-04-05b login JWT usuarios y roles`
