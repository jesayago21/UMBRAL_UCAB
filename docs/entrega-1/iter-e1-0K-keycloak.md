# Iteración E1-K — Keycloak (OIDC) reemplaza el JWT propio

**Fase:** Entrega 1 — autenticación (E1-K1, E1-K2, E1-K3)
**Fecha:** 2026-05-28
**Fuentes de verdad:** `docs/entrega-1/PLAN.md §5.2`, `.cursor/skills/keycloak-auth-skill.md`, `umbral-quality-spec.md §13.1`, `project-rules.md §10`

> **Alcance:** se sustituye el JWT propio (`iter-04-05b`) por **Keycloak** como
> Identity Provider. La API pasa a ser **resource server** (valida tokens contra
> el realm; no emite tokens). El login OIDC del frontend (E1-K4) queda pendiente.

---

## Contexto

Hasta ahora el backend emitía sus propios JWT (`AuthController.Login` +
`JwtTokenIssuer`) contra una tabla `usuarios` con BCrypt. El profesor pidió
**Keycloak** ("clickload"). Esta iteración mueve usuarios y roles a Keycloak y
elimina por completo el JWT propio.

---

## E1-K1 — Keycloak en docker-compose + realm export

- **`docker-compose.yml`**: nuevo servicio `keycloak` (`quay.io/keycloak/keycloak:24.0`),
  `start-dev --import-realm`, puerto `8080`, monta `./docker/keycloak` como
  carpeta de import (`/opt/keycloak/data/import`).
- **`docker/keycloak/umbral-realm.json`**: export del realm `umbral` con:

| Elemento | Detalle |
|----------|---------|
| Roles realm | `Administrador`, `Operador`, `EquipoParticipante` |
| Client `umbral-web` | público, Auth Code + **PKCE (S256)**, `redirectUris` a `localhost:5173/3000`, Direct Access Grants on (para smoke), **audience mapper → `umbral-api`** |
| Client `umbral-api` | `bearerOnly` (resource server) |
| Usuarios demo | `admin` / `operador` / `equipo` — password `Umbral123!` (no temporal) con su rol |

---

## E1-K2 — API valida token Keycloak

- **`Auth/KeycloakOptions.cs`** (nuevo): `Authority`, `Audience`, `RequireHttpsMetadata`.
- **`Extensions/ApiServiceCollectionExtensions.cs`**: la rama no-`Testing` usa
  `AddJwtBearer` con `Authority` (descarga JWKS), valida issuer + audiencia +
  lifetime, y en `OnTokenValidated` **mapea `realm_access.roles` → `ClaimTypes.Role`**
  (Keycloak anida los roles en un JSON; ASP.NET no los reconoce solo).
- **`appsettings.json` / `appsettings.Development.json`**: sección `Jwt` →
  sección `Keycloak` (`Authority`, `Audience`, `RequireHttpsMetadata=false`).
- **`Testing`** sigue usando `TestAuthHandler` (no se levanta Keycloak en CI).

```csharp
RoleClaimType = ClaimTypes.Role,
NameClaimType = "preferred_username",
// OnTokenValidated → parsea realm_access.roles y los agrega como Role claims
```

---

## E1-K3 — Eliminar JWT propio

**Archivos eliminados:**

| Capa | Archivos |
|------|----------|
| API | `Controllers/AuthController.cs`, `Auth/JwtTokenIssuer.cs`, `Auth/JwtOptions.cs`, `Contracts/Auth/LoginRequest.cs`, `Contracts/Auth/LoginResponse.cs` |
| Infrastructure | `Auth/{UsuarioAuthDto, UsuarioAuthRepository, IUsuarioAuthRepository, DemoUsersSeeder, UmbralAuthBootstrapExtensions}.cs`, `Persistence/Entities/Usuario.cs`, `Persistence/Configurations/UsuarioConfiguration.cs` |
| Tests | `Umbral.API.Tests/Controllers/AuthControllerTests.cs` |

**Cambios de wiring:**

- `Program.cs`: se quita `SeedDemoUsersAsync(...)` y su `using`.
- `InfrastructureServiceCollectionExtensions`: se quitan registros de
  `IUsuarioAuthRepository` y `DemoUsersSeeder`.
- `UmbralDbContext`: se elimina `DbSet<Usuario>` (y el `using ...Entities`).
- `Umbral.Infrastructure.csproj`: se elimina el paquete `BCrypt.Net-Next`.
- **Migración `DropUsuariosAuth`**: dropea la tabla `usuarios` (con `Down` reversible).

> ⚠️ **`UsuarioId` (value object del dominio) NO se toca**: es la identidad del
> operador en `Sesion`, distinta de la entidad `Usuario` de auth. Su value
> converter sigue activo.

---

## Verificación (smoke real contra Keycloak)

```powershell
docker compose up -d keycloak
```

**Discovery del realm** (en PowerShell usar `curl.exe` o `Invoke-RestMethod`; `curl` solo es alias de `Invoke-WebRequest`):

```powershell
curl.exe http://localhost:8080/realms/umbral/.well-known/openid-configuration
```

**Token Bearer** (usuario `admin` para el catálogo; `operador` para probar rol Operador):

```powershell
# Opción A — curl real en Windows (recomendado)
curl.exe -s -X POST "http://localhost:8080/realms/umbral/protocol/openid-connect/token" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "grant_type=password&client_id=umbral-web&username=admin&password=Umbral123!"

# Opción B — solo el access_token en consola
$r = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/realms/umbral/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body "grant_type=password&client_id=umbral-web&username=admin&password=Umbral123!"
$r.access_token
```

Copia el valor de **`access_token`** (empieza con `eyJ...`) y pégalo en `http://localhost:5173/login`.

Resultado del token decodificado:

| Claim | Valor |
|-------|-------|
| `aud` | `umbral-api` ✅ (audience mapper) |
| `realm_access.roles` | `Operador` ✅ |
| `preferred_username` | `operador` ✅ |

**Tests:** `dotnet test` → **302/302 ✅** (Domain 198, Application 51,
Infrastructure 7, API 46). Se eliminaron 3 tests del `AuthController`.

---

## Decisiones de diseño

1. **API como resource server puro**: valida por `Authority`/JWKS; no hay endpoint
   de login en el backend (lo sirve Keycloak).
2. **Mapeo de roles en `OnTokenValidated`**: Keycloak emite `realm_access.roles`
   (JSON anidado); se proyecta a `ClaimTypes.Role` para que `[Authorize(Roles=…)]`
   funcione sin cambios en los controllers.
3. **Audience mapper en el realm**: el client `umbral-web` inyecta `umbral-api`
   en `aud`, así la API puede validar audiencia (`ValidateAudience=true`).
   Si `Keycloak:Audience` queda vacío, la validación de audiencia se desactiva.
4. **`TestAuthHandler` intacto**: los tests de integración no dependen de Keycloak.
5. **Realm versionado** (`umbral-realm.json`): reproducible, sin clicks manuales.

---

## Historias / criterios cubiertos

| Criterio DoD | Estado |
|--------------|--------|
| PostgreSQL **y Keycloak** levantan vía docker-compose | ✅ (compose + realm import) |
| Login real con Keycloak distinto admin/operador | 🔶 backend listo; falta UI (E1-K4) |
| 403 por rol (operador no administra catálogo) | 🔶 mapeo de roles listo; se demuestra con E1-10 + E1-K4 |

---

## Pendiente — siguientes iteraciones

- **E1-K4**: login OIDC en el frontend (`react-oidc-context` / `oidc-client-ts`)
  contra `umbral-web`, adjuntar `Bearer` a las llamadas a la API.
- **E1-5/E1-6**: README + guion de demo incluyen arranque de Keycloak y el smoke
  de login real + 403 por rol.
