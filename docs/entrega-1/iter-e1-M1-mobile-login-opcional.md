# Iteración E1-M1 — Mobile login OIDC (opcional)

**Fase:** Entrega 1 — Fase D (opcional, solo si hay tiempo tras E1-2b)
**Depende de:** E1-K4 (misma configuración Keycloak)
**Prioridad:** Baja — no bloquea el DoD de Entrega 1

> **Objetivo:** demostrar que el rol **EquipoParticipante** (o cualquier usuario)
> puede autenticarse en **React Native / Expo** contra el mismo realm `umbral`.
> **Sin** gameplay, QR ni llamadas a sesión.

---

## Alcance SÍ

| Ítem | Detalle |
|------|---------|
| Proyecto | `src/frontend/umbral-mobile` (Expo + TS) |
| Pantalla | Login → redirect Keycloak → guardar token (AsyncStorage) |
| Post-login | Pantalla “Sesión” placeholder: “Login OK — gameplay en E2” |
| Keycloak | Reutilizar client `umbral-web` o añadir `umbral-mobile` con redirect `exp://` / deep link documentado |

---

## Fuera de alcance

- Escáner QR, `submitEvidencia`, trivia, SignalR.
- Publicación en stores.
- Cobertura Jest en E1.

---

## Criterio “hecho”

- Usuario `equipo` / `Umbral123!` completa login y ve confirmación con su `preferred_username`.
- Token válido para API (smoke opcional `GET` health o endpoint protegido de equipo).

---

## Si no hay tiempo

Entrega 1 se cierra sin E1-M1; mobile completo queda en Entrega 2 según `PLAN.md §4.2`.
