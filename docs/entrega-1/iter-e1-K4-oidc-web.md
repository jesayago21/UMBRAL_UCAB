# Iteración E1-K4 — Login OIDC web (Keycloak)

**Fase:** Entrega 1 — Fase D
**Depende de:** E1-K1..K3, E1-2
**Siguiente:** E1-2a, E1-2b

> **Objetivo:** reemplazar el textarea de JWT en `LoginPage` por **Authorization Code +
> PKCE** con `react-oidc-context` contra el client `umbral-web`.

---

## Tareas

1. `npm install react-oidc-context oidc-client-ts` (según spec)
2. `.env.example`: `VITE_KEYCLOAK_URL`, `VITE_KEYCLOAK_REALM=umbral`, `VITE_KEYCLOAK_CLIENT_ID=umbral-web`
3. `AuthProvider` + `onSigninCallback` en `main.tsx` / `App.tsx`
4. `LoginPage`: botón “Iniciar sesión con Keycloak” → `signinRedirect()`
5. Ruta `/callback` (o silent renew según config) sincroniza token con `authStore`
6. **Redirect por rol** tras login:
   - `Administrador` → `/admin/misiones`
   - `Operador` → `/operador/sesiones`
7. Eliminar flujo de pegar token (o dejarlo detrás de `import.meta.env.DEV` si se desea)
8. Logout: `signoutRedirect` + limpiar `authStore`

---

## Verificación

- Login `admin` → catálogo CRUD
- Login `operador` → operador (tras E1-2b) o home operador placeholder
- `operador` en `/admin/misiones` → 403
- API recibe `Authorization: Bearer` con `aud: umbral-api`

---

## Doc a actualizar al cerrar

- `iter-e1-0K-keycloak.md` (marcar front ✅)
- `iter-e1-02-frontend-crud.md` (login ya no manual)
- `PLAN.md` fila E1-K4 ✅
