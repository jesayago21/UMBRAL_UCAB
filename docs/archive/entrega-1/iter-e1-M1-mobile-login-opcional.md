# Iteración E1-M1 — Panel equipo (web) con login OIDC

**Fase:** Entrega 1 — Fase D
**Depende de:** E1-2b (sesiones operador + código por sesión)

> **Objetivo:** jugador con rol **EquipoParticipante** inicia sesión en web (Keycloak),
> elige Búsqueda del tesoro o Trivia, ve sesiones abiertas a inscripción y se une con el
> **código de la sesión** (uno por sesión, no por equipo).

---

## Flujo

| Rol | Acción |
|-----|--------|
| Operador | Crea sesión → recibe código → **Abrir inscripción** → comparte código |
| Equipo | Login → `/equipo/busqueda` o `/equipo/trivia` → elige sesión → ingresa código → unirse |

---

## API

- `GET /api/v1/sesiones/disponibles/busqueda-tesoro` — EquipoParticipante
- `GET /api/v1/sesiones/disponibles/trivia` — EquipoParticipante (vacío hasta sesiones trivia)
- `POST /api/v1/sesiones/{id}/unirse` — body `{ codigoAcceso }`
- `POST /api/v1/sesiones/{id}/abrir-inscripcion` — Operador

---

## Fuera de alcance E1

- Gameplay QR, trivia jugable, SignalR (E2)
- App mobile nativa (mismo flujo reutilizable)

---

## Criterio “hecho”

- Login `equipo` / `Umbral123!` → panel BT con listado y unirse con código válido
- Operador ve equipos inscritos sin registrar manualmente Alpha/Beta
