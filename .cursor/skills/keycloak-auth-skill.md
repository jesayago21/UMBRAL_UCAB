# Skill: Autenticación con Keycloak (OIDC) — UMBRAL

> **Vigente desde Entrega 1.** Keycloak reemplaza al JWT propio (login casero +
> `JwtTokenIssuer` + tabla `usuarios` + BCrypt). Los usuarios y roles viven en el
> **realm de Keycloak**; la API solo **valida** tokens emitidos por Keycloak.

---

## 1. Decisiones de arquitectura (acordadas)

| Tema | Decisión |
|------|----------|
| Flujo | **Authorization Code + PKCE (OIDC)**. El frontend redirige al login de Keycloak. |
| Usuarios / roles | En el **realm** (`umbral`). No hay tabla `usuarios` propia. |
| Rol de la API | **Resource server**: valida el `access_token` vía la metadata OIDC del realm (JWKS). No emite tokens. |
| Tests | Integración/handlers siguen con **`TestAuthHandler`** (no se levanta Keycloak en CI). Keycloak se valida con smoke manual. |
| JWT propio | **Eliminado** (ver §6 migración). |

---

## 2. Modelo en Keycloak

```
Realm: umbral
├── Roles (realm roles)
│   ├── Administrador
│   ├── Operador
│   └── Participante
├── Clients
│   ├── umbral-web      → public, Standard Flow (Auth Code + PKCE), redirect a la SPA
│   └── umbral-api      → bearer-only / confidential (solo valida; sin login propio)
└── Users (demo)
    ├── admin     → rol Administrador
    ├── operador  → rol Operador
    └── participante    → rol Participante
```

- Los roles del realm viajan en el claim **`realm_access.roles`** del access token.
- `sub` = id del usuario en Keycloak (Guid/UUID).
- Para reproducibilidad de la demo se importa un **realm export** (`docker/keycloak/umbral-realm.json`) al arrancar el contenedor.

---

## 3. API .NET — validación del token (resource server)

`AddJwtBearer` apuntando al realm (descubre claves por `Authority`):

```csharp
// ApiServiceCollectionExtensions (entorno != Testing)
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloak.Authority;     // p.ej. http://localhost:8080/realms/umbral
        options.Audience  = keycloak.Audience;       // "umbral-api" (o "account" segun config)
        options.RequireHttpsMetadata = false;        // solo dev/local
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloak.Authority,
            ValidateAudience = true,
            ValidateLifetime = true,
            RoleClaimType = "roles"                   // tras aplanar realm_access.roles
        };
    });
```

### Mapear `realm_access.roles` → claims `role`

Keycloak anida los roles en `realm_access.roles`. ASP.NET no los reconoce como
`ClaimTypes.Role` por defecto. Aplanarlos con `OnTokenValidated`:

```csharp
options.Events = new JwtBearerEvents
{
    OnTokenValidated = ctx =>
    {
        if (ctx.Principal?.Identity is ClaimsIdentity id)
        {
            var realmAccess = ctx.Principal.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmAccess))
            {
                using var doc = JsonDocument.Parse(realmAccess);
                if (doc.RootElement.TryGetProperty("roles", out var roles))
                    foreach (var r in roles.EnumerateArray())
                        id.AddClaim(new Claim("roles", r.GetString()!));
            }
        }
        return Task.CompletedTask;
    }
};
```

> Los `[Authorize(Roles = "Administrador")]` existentes siguen funcionando si
> `RoleClaimType = "roles"` y los roles del realm coinciden con esos nombres.

### Configuración

```json
// appsettings.json
"Keycloak": {
  "Authority": "http://localhost:8080/realms/umbral",
  "Audience": "umbral-api"
}
```

---

## 4. Frontend web (OIDC Authorization Code + PKCE)

- Usar `oidc-client-ts` o `react-oidc-context`.
- El botón "Iniciar sesión" redirige a Keycloak; al volver, la SPA guarda el
  `access_token` y lo manda en `Authorization: Bearer` (igual que hoy en
  `umbral-frontend-spec` §interceptors).
- `authStore` deja de recibir el token de un `POST /auth/login` propio; lo recibe
  del callback OIDC. El resto (`estaAutenticado`, `rol`, `ProtectedRoute`) se mantiene;
  `rol` se lee de `realm_access.roles`.

---

## 5. Tests (no cambia la suite)

- **`Umbral.Domain.Tests` / `Umbral.Application.Tests`**: sin impacto (no tocan auth).
- **`Umbral.API.Tests`**: el entorno `Testing` sigue usando **`TestAuthHandler`**
  (con header `X-Test-Role`). **No** se levanta Keycloak en CI.
- Smoke manual de Keycloak: documentar en el README/guion de demo (login real,
  403 por rol). No cuenta para el 90% automático.

> Regla: la cobertura ≥ 90% se mide sobre dominio/aplicación/infra/controllers,
> no sobre el wiring de Keycloak (excluido como configuración, igual que `Program.cs`).

---

## 6. Migración desde el JWT propio (qué se elimina)

| Artefacto | Acción |
|-----------|--------|
| `Umbral.API/Controllers/AuthController.cs` (`POST /auth/login`) | **Eliminar** (login lo hace Keycloak) |
| `Umbral.API/Auth/JwtTokenIssuer.cs`, `JwtOptions.cs` | **Eliminar** |
| `Umbral.API/Contracts/Auth/*` (`LoginRequest/Response`) | **Eliminar** |
| `Umbral.Infrastructure/Auth/*` (`UsuarioAuthRepository`, `IUsuarioAuthRepository`, `UsuarioAuthDto`, `DemoUsersSeeder`, bootstrap) | **Eliminar** |
| Entidad `Usuario` + `UsuarioConfiguration` + `DbSet<Usuario>` | **Eliminar** + migración que dropea `usuarios` |
| Paquete `BCrypt.Net-Next` | Quitar referencia |
| `appsettings*.json` sección `Jwt` | Reemplazar por `Keycloak` |
| `AddJwtBearer` con clave simétrica | Reemplazar por `Authority`/JWKS de Keycloak |
| `TestAuthHandler` | **Conservar** (solo `Testing`) |

---

## 7. Anti-patrones

- ❌ Validar el token de Keycloak con clave simétrica propia. Usar `Authority`/JWKS.
- ❌ Duplicar usuarios en BD y en Keycloak. La fuente de verdad es el realm.
- ❌ Levantar Keycloak en los tests unitarios/integración de CI (lento, frágil).
- ❌ `RequireHttpsMetadata = true` en local sin TLS (rompe el discovery).
- ❌ Loguear access tokens.
