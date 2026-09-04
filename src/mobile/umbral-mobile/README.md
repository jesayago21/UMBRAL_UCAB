# UMBRAL Mobile — cliente Participante (Expo)

App React Native del jugador. Login Keycloak (`umbral-mobile`) + flujo de sesión (lobby, BT con QR manual, trivia, ranking, SignalR).

## Requisitos

- Node 20+
- Backend local: Postgres + Keycloak + API (+ gateway opcional)
- Expo Go (dispositivo/emulador) o `npm run android` / `ios`

## Setup

```bash
cd src/mobile/umbral-mobile
cp .env.example .env
npm install
npx expo start
```

### Variables (`.env`)

| Variable | Default (emulador) | En web |
|----------|-------------------|--------|
| `EXPO_PUBLIC_API_URL` | `http://10.0.2.2:5000` | `localhost:5000` |
| `EXPO_PUBLIC_SIGNALR_URL` | `http://10.0.2.2:5000` | `localhost:5000` |
| `EXPO_PUBLIC_KEYCLOAK_URL` | `http://10.0.2.2:8180` | `localhost:8180` |
| `EXPO_PUBLIC_KEYCLOAK_REALM` | `umbral` | `umbral` |
| `EXPO_PUBLIC_KEYCLOAK_CLIENT_ID` | `umbral-mobile` | `umbral-web` |

Tras cambiar `.env`: `npx expo start --clear`.

## Auth

**Mismo Keycloak** que la web (realm `umbral`, usuario `participante`).

En **Expo Go** no usamos el redirect del navegador (como `signinRedirect` en web): Custom Tabs + `exp://` termina en «Access error 404». Mobile pide usuario/contraseña y obtiene el **mismo access token** vía Direct Access Grants del cliente `umbral-mobile`.

Cuando exista un development build nativo (`umbral://`), se podrá volver al flujo OIDC de navegador.

## Emulador Android

Por defecto `.env` usa `10.0.2.2` y Keycloak en **:8180** (en muchos Windows el :8080/IPv4 lo ocupa `ApplicationWebServer` y devolvía el 404 falso).

Tras cambiar `.env`: `npx expo start --clear`.

Si recreaste el puerto de Keycloak: `docker compose up keycloak -d --force-recreate` (y reinicia la API local si corre fuera de Docker).

## Keycloak

El realm incluye el cliente público `umbral-mobile` (PKCE + Direct Access Grants + audience `umbral-api`).

Si Keycloak ya estaba levantado **antes** de añadir el cliente, reimporta el realm o créalo a mano en Admin Console, y reinicia:

```bash
docker compose up keycloak -d --force-recreate
```

Usuario demo: `participante` / `Umbral123!` (rol Participante).

Redirect URIs: `umbral://*` y `exp://*` (Expo Go).

## Flujo

1. Login OIDC (solo rol Participante).
2. Lobby: sesiones abiertas + código de acceso.
3. Partida: etapa, pistas, QR manual, trivia live, ranking.

## Web participante

Deshabilitado: `VITE_PARTICIPANTE_WEB_ENABLED=false` en `umbral-web`. El jugador usa solo esta app mobile.

## Pendiente (siguiente corte)

- NativeWind / tests RNTL

## Evidencia QR

En etapas de búsqueda del tesoro puedes:
- escribir el código a mano, o
- **Escanear con cámara** (`expo-camera`, QR).

En Expo Go concede el permiso de cámara cuando lo pida el sistema.
