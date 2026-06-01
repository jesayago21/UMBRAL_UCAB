# Iteración E1-K4 — Login OIDC web (Keycloak)

**Fase:** Entrega 1 — Fase D
**Fecha:** 2026-05-30
**Depende de:** E1-K1..K3, E1-2
**Siguiente:** E1-2a, E1-2b

> **Objetivo:** Authorization Code + PKCE con `react-oidc-context` (client `umbral-web`).
> Sustituye pegar JWT en `LoginPage`.

---

## Implementación

| Artefacto | Rol |
|-----------|-----|
| `react-oidc-context` + `oidc-client-ts` | Flujo OIDC en la SPA |
| `src/auth/oidcConfig.ts` | Authority, client, redirect `/callback` |
| `src/auth/OidcAuthBridge.tsx` | Sincroniza `access_token` → `authStore` |
| `src/auth/useOidcLogout.ts` | `signoutRedirect` + limpiar store |
| `src/pages/auth/LoginPage.tsx` | Botón → Keycloak |
| `src/pages/auth/CallbackPage.tsx` | Post-login redirect por rol |
| `src/router/guards.tsx` | `RequireRoles`, `HomeRedirect` |
| `/operador/sesiones` | Placeholder hasta E1-2b |

**Variables** (`.env.example`):

```env
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_REALM=umbral
VITE_KEYCLOAK_CLIENT_ID=umbral-web
```

---

## Redirect por rol

| Rol | Ruta inicial |
|-----|----------------|
| `Administrador` | `/admin/misiones` |
| `Operador` | `/operador/sesiones` |
| `EquipoParticipante` | `/equipo` (web si `VITE_EQUIPO_WEB_ENABLED=true`; bloqueado si `false` para mobile) |

**403 demo:** `operador` en `/admin/*` → pantalla acceso denegado.

---

## Verificación manual

1. `docker compose up -d keycloak`
2. API + `npm run dev` en `umbral-web` (copiar `.env.example` → `.env`)
3. `http://localhost:5173/login` → Keycloak → `admin` / `Umbral123!` → catálogo CRUD
4. Logout → vuelve a login
5. `operador` / `Umbral123!` → `/operador/sesiones` (placeholder)
6. `operador` intenta `/admin/misiones` → 403

**Tests automáticos:** no se añaden en E1 (Keycloak no en CI; ver `keycloak-auth-skill` §5).

---

## Pendiente

- **E1-2b:** UI real de sesiones operador
- **E1-2a:** afinado admin
