# Iteración E1-2b — Pantalla operador (sesión BT mínima)

**Fase:** Entrega 1 — Fase D (frontend)
**Depende de:** E1-K4 (OIDC), API sesiones ✅ (`SesionesController`)
**HU:** HU-12..16 (subconjunto demostrable en UI)

> **Objetivo:** que el rol **Operador** gestione una sesión de **Búsqueda del tesoro**
> desde `umbral-web` usando solo REST (sin SignalR).

---

## Alcance SÍ (mínimo)

| Pantalla / ruta | Funcionalidad |
|-----------------|---------------|
| `/operador/sesiones` | Crear sesión BT (elegir misión activa), ver `sesionId` |
| `/operador/sesiones/:id` | Registrar participantes (nombre → código acceso), controles sesión |
| Misma página o panel | **Ranking**: `GET /api/v1/sesiones/{id}/ranking` con botón/refresco manual |

**Acciones API** (ya existen, rol `Operador,Administrador`):

- `POST /api/v1/sesiones/busqueda-tesoro`
- `POST .../participantes`, `.../iniciar`, `.../pausar`, `.../reanudar`, `.../finalizar`, `.../cancelar`
- `GET .../ranking`

**Redirect tras login (E1-K4):**

- `Administrador` → `/admin/misiones`
- `Operador` → `/operador/sesiones`
- Intento `/admin/*` como Operador → pantalla 403 (ya previsto)

---

## API auxiliar (incluida en E1-2b)

El operador **no** puede `GET /api/v1/misiones` (solo Administrador). Para el combo de misiones:

- **`GET /api/v1/misiones/activas`** — lista `{ id, nombre }` de misiones en estado Activa.
- `[Authorize(Roles = "Operador,Administrador")]`
- Query/handler pequeño + test API.

---

## Fuera de alcance (E2)

- SignalR / `useSessionSocket` / ranking en vivo.
- Penalización en UI (endpoint existe; opcional si sobra tiempo).
- Trivia sesión, sala de espera, mobile evidencia QR.
- `OperatorDashboardPage` completo de la spec (grid 3 columnas).

---

## Stack front (igual E1-2)

- React Query + `sesionService.ts` + tipos `sesion.types.ts`
- `OperadorLayout` (nav simple: Sesiones, Salir)
- Reutilizar `LoadingState` / `ErrorState`

---

## Verificación manual

1. Login OIDC como `operador` / `Umbral123!`
2. Crear sesión con misión activa (creada antes por `admin`)
3. Registrar 2 participantes, iniciar sesión
4. Refrescar ranking (vacío o con puntajes según dominio)
5. Pausar → reanudar → finalizar
6. Login `admin` → catálogo OK; `operador` → `/admin/misiones` bloqueado

---

## Orden de implementación

1. E1-K4 (OIDC + redirect por rol)
2. Backend `GET misiones/activas` (si no existe)
3. `sesionService` + hooks
4. `OperadorSesionesPage` + `OperadorSesionDetailPage`
5. Router + `OperadorLayout`
6. E1-2a polish admin en paralelo si aplica
