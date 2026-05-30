# Iteración E1-2 — Frontend CRUD Misiones + Trivia

**Fase:** Entrega 1 — Fase D (frontend)
**Fecha:** 2026-05-29
**Fuentes de verdad:** `umbral-frontend-spec.md`, `PLAN.md §6`, HU-01..04, HU-28..35

> **Objetivo:** app `umbral-web` que consume la API con JWT Keycloak y expone CRUD de
> misiones, categorías y preguntas para rol **Administrador**.

---

## Artefactos

| Ruta | Rol |
|------|-----|
| `src/frontend/umbral-web/` | SPA React + Vite + TS + Tailwind |
| `src/frontend/umbral-web/.env.example` | `VITE_API_URL` |
| `src/backend/Umbral.API/Program.cs` | CORS `FrontendDev` → `localhost:5173` |
| `src/frontend/umbral-web/src/pages/admin/*` | CRUD Misiones, Categorías, Preguntas |
| `src/frontend/umbral-web/src/pages/auth/LoginPage.tsx` | Login dev (pegar token) — **E1-K4** lo reemplaza |
| `src/frontend/umbral-web/src/router/AppRouter.tsx` | Rutas + guard `Administrador` |

---

## Stack (según frontend-spec)

| Tecnología | Uso en E1-2 |
|------------|-------------|
| React 18+ / Vite / TS | UI y build |
| React Router 6+ | `/login`, `/admin/*` |
| TanStack Query | cache + mutations CRUD |
| Zustand + persist | token, rol, username |
| Axios | `apiClient` con interceptor Bearer |
| Tailwind CSS 4 | estilos utilitarios |

Vitest / Playwright quedan para Entrega 2 (gate 80% frontend en quality-spec).

---

## Endpoints consumidos

Todas las rutas requieren `[Authorize(Roles = "Administrador")]`:

| Recurso | Métodos |
|---------|---------|
| `/api/v1/misiones` | GET (filtros), POST, PUT, DELETE (desactivar) |
| `/api/v1/categorias` | GET, POST, PUT, DELETE |
| `/api/v1/preguntas` | GET (filtros), POST, PUT, DELETE |

---

## Arranque local (manual)

1. **Infra:** PostgreSQL + Keycloak (`docker compose up -d`).
2. **API:** `dotnet run --project src/backend/Umbral.API` (puerto 5000).
3. **Token:** usuario `admin` / `Umbral123!` — ver comandos en `iter-e1-0K-keycloak.md` (en Windows usar `curl.exe` o `Invoke-RestMethod`, no `curl` a secas).
4. **Front:**
   ```powershell
   cd src/frontend/umbral-web
   copy .env.example .env
   npm install
   npm run dev
   ```
5. Abrir `http://localhost:5173/login`, pegar el Bearer token, entrar al catálogo.

**Build producción:** `npm run build` → artefactos en `dist/`.

---

## Pantallas

| Ruta | HU | Funcionalidad |
|------|-----|---------------|
| `/admin/misiones` | HU-01..04 | Listar/filtrar, crear (1 etapa mín.), editar nombre/estado, desactivar |
| `/admin/categorias` | HU-28..31 | CRUD categorías trivia |
| `/admin/preguntas` | HU-32..35 | CRUD preguntas (≥3 opciones, 1 correcta), filtros por categoría/dificultad |

---

## Auth (temporal vs E1-K4)

- **E1-2:** `LoginPage` acepta JWT pegado; valida payload y exige rol `Administrador`.
- **E1-K4:** reemplazar por `react-oidc-context` + redirect Keycloak (sin pegar token).

El `apiClient` ya adjunta `Authorization: Bearer` y hace logout en 401.

---

## Verificación

| Check | Estado |
|-------|--------|
| `npm run build` sin errores TS | ✅ |
| CORS API → `:5173` | ✅ |
| Guard rol Administrador | ✅ |
| CRUD misiones / categorías / preguntas wired | ✅ |

Prueba manual end-to-end: crear categoría → pregunta con 3 opciones → misión con etapa.

---

## Siguiente trabajo (plan ampliado 2026-05-30)

| Ítem | Acción |
|------|--------|
| **E1-K4** | Login OIDC — [iter-e1-K4-oidc-web.md](iter-e1-K4-oidc-web.md) |
| **E1-2a** | Afinar UI admin (mensajes, vacíos, navegación) |
| **E1-2b** | Pantalla operador — [iter-e1-02b-operador-sesiones.md](iter-e1-02b-operador-sesiones.md) |
| **E1-M1** | *(Opcional)* Mobile solo login — [iter-e1-M1-mobile-login-opcional.md](iter-e1-M1-mobile-login-opcional.md) |
| **E1-5 / E1-6** | README + guion demo |
| Entrega 2 | Vitest 80%, SignalR, mobile gameplay, E2E |
