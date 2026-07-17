# UMBRAL — UCAB Jesus Sayago

Sistema de gestión de misiones polimórficas (Búsqueda del Tesoro + Trivia por etapas), sesiones en vivo e identidad federada con Keycloak.  
Arquitectura: **Monolito hexagonal** (Ports & Adapters) en .NET 8. Comunicación entre capas **solo por DTOs**.

**Resumen consolidado (fases/iteraciones/HU/RF/RNF):** [`docs/RESUMEN-COMPACTO-E1-E2.md`](docs/RESUMEN-COMPACTO-E1-E2.md)  
**Trazabilidad (RB / RF / HU):** [`docs/TRAZABILIDAD.md`](docs/TRAZABILIDAD.md) · **Fase 1 dominio:** [`docs/fase-1/TRACKER.md`](docs/fase-1/TRACKER.md)

---

## Requisitos previos

**Backend (.NET)** — suficiente para compilar, API, tests y cobertura:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) ≥ 24 (Postgres + Keycloak; **obligatorio para tests de infra/API y cobertura**)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

**Frontend web** (`src/frontend/umbral-web`, opcional si solo trabajas backend):

- [Node.js](https://nodejs.org/) ≥ 20 + npm

---

## Inicio rápido (desarrollo local)

Usa **una terminal por paso**. El gateway (`:8000`) es opcional pero recomendado: el frontend apunta REST ahí; la API sigue en `:5000`.

### Terminal 1 — Infraestructura

```bash
cp .env.example .env
docker compose up postgres mailpit keycloak -d
```

RabbitMQ solo si lo necesitas (`docker compose up rabbitmq -d`); la API arranca sin él hoy.

### Terminal 2 — API monolito (`:5000`)

```bash
dotnet build Umbral.sln
dotnet run --project src/backend/Umbral.API
```

En **Development** la API aplica migraciones EF al arrancar. Si falla por esquema desactualizado:

```bash
dotnet ef database update --project src/backend/Umbral.Infrastructure --startup-project src/backend/Umbral.API
```

### Terminal 3 — Gateway YARP (`:8000`, recomendado)

```bash
dotnet run --project src/backend/Umbral.Gateway
```

El gateway **solo reenvía** tráfico; la API debe estar corriendo en `:5000`.

### Terminal 4 — Frontend web (opcional)

```bash
cd src/frontend/umbral-web
cp .env.example .env    # VITE_API_URL=http://localhost:8000
npm install
npm run dev
```

Abrir **http://localhost:5173/login**.

### Pruebas rápidas (smoke test)

**PowerShell** (en Windows evita `curl`; a veces cuelga):

```powershell
# Health API directa
Invoke-RestMethod http://localhost:5000/health
# → {"status":"ok"}

# Health gateway
Invoke-RestMethod http://localhost:8000/health
# → {"status":"ok","service":"gateway"}

# REST vía gateway (401 sin token; 200 con sesión Keycloak)
Invoke-WebRequest http://localhost:8000/api/v1/misiones -UseBasicParsing
```

**Bash / Git Bash:**

```bash
curl http://localhost:5000/health
curl http://localhost:8000/health
curl -i http://localhost:8000/api/v1/misiones
```

| Qué probar | URL | Resultado esperado |
|------------|-----|-------------------|
| API viva | http://localhost:5000/health | `{"status":"ok"}` |
| Gateway viva | http://localhost:8000/health | `{"status":"ok","service":"gateway"}` |
| Proxy funciona | http://localhost:8000/api/v1/misiones | Mismo status que `:5000/api/v1/misiones` |
| Frontend | http://localhost:5173/login | Login Keycloak |
| Keycloak admin | http://localhost:8080 | `admin` / `admin` |
| Mailpit (emails) | http://localhost:8025 | Bandeja SMTP local (reset password) |

### Resumen de puertos

| Servicio | Puerto | Rol |
|----------|--------|-----|
| PostgreSQL | 5433 | Base de datos |
| Keycloak | 8080 | Autenticación OIDC |
| Mailpit | 1025 / 8025 | SMTP de desarrollo (UI en :8025) |
| RabbitMQ | 5672 / 15672 | Mensajería (UI en :15672) |
| **Umbral.API** | **5000** | Monolito hexagonal (acceso directo) |
| **Umbral.Gateway** | **8000** | REST público `/api/v1/**` |
| Frontend web | 5173 | Admin + Operador (+ participante E1) |

**Credenciales de desarrollo** (PostgreSQL y RabbitMQ): `umbral_user` / `umbral_pass` · BD: `umbral_db`  
**Nota:** Postgres usa `5433` para no chocar con instalaciones locales en `5432`.

### Alternativa: stack en Docker

Levanta infra + API + gateway en contenedores (requiere Docker Desktop):

```bash
docker compose up postgres rabbitmq keycloak api gateway -d
# REST:  http://localhost:8000/api/v1/...
# API:   http://localhost:5000/health
```

### Tests y cobertura (backend)

```bash
dotnet test Umbral.sln
.\scripts\run-coverage.ps1 -Threshold 90 -Open    # Windows
bash scripts/run-coverage.sh --threshold 90     # Linux/macOS/CI
```

> Docker debe estar corriendo para tests de **Infrastructure** y **API** (Testcontainers).

---

## 1. Configuración inicial

```bash
# Clonar el repositorio
git clone <url> && cd UMBRAL_UCAB

# Crear archivo de variables de entorno local (nunca commitear .env)
cp .env.example .env
```

El archivo `.env` ya trae valores seguros para desarrollo local. No lo modifiques a menos que necesites credenciales personalizadas.

---

## 2. Levantar la infraestructura con Docker

```bash
# Mínimo para dev local (API con dotnet run)
docker compose up postgres keycloak -d

# Infra completa (+ RabbitMQ)
docker compose up postgres rabbitmq keycloak -d

# Stack completo: infra + API + gateway en contenedores
docker compose up postgres rabbitmq keycloak api gateway -d

docker compose ps
docker compose logs -f postgres
```

---

## 3. Frontend web (`umbral-web`)

```bash
cd src/frontend/umbral-web
cp .env.example .env
npm install
npm run dev
```

Abrir **http://localhost:5173/login**.

| Variable | Uso |
|----------|-----|
| `VITE_API_URL` | API REST vía gateway (default `http://localhost:8000`; API directa `:5000`) |
| `VITE_KEYCLOAK_*` | Realm `umbral`, client `umbral-web` |
| `VITE_PARTICIPANTE_WEB_ENABLED` | `false` = jugador solo en mobile. `true` = también `/participante` en web |

### Usuarios demo (Keycloak)

| Usuario | Contraseña | Rol | Ruta inicial |
|---------|------------|-----|----------------|
| `admin` | `Umbral123!` | Administrador | `/admin/misiones` |
| `operador` | `Umbral123!` | Operador | `/operador/sesiones` |
| `participante` | `Umbral123!` | Participante | App mobile (`umbral-mobile`) |

Si Keycloak dice **usuario desconocido** o tras login aparece *rol no válido*, el realm se creó antes de añadir `participante` o el rol `Participante`. Con Keycloak en marcha:

```powershell
.\scripts\keycloak-ensure-demo-users.ps1
```

### Registro de usuarios (contraseña por email)

Al crear un usuario **Administrador/Operador** desde Admin, Keycloak envía un correo con link `UPDATE_PASSWORD` (sin contraseña previa). En local:

```bash
docker compose up mailpit keycloak -d
.\scripts\keycloak-configure-smtp.ps1   # si el realm ya existía sin SMTP
```

1. Crear usuario en `/admin/usuarios` (email real o de prueba).
2. Abrir [Mailpit](http://localhost:8025) → abrir el correo → seguir el link.
3. Definir contraseña en Keycloak e iniciar sesión.

### Auto-registro de Participante

Los jugadores se registran solos (rol fijo `Participante`; el admin **no** puede crearlos — RB-35 / HU-42):

1. En Login → **Crear cuenta de participante** (`/registro`).
2. Completar email, usuario, nombre, apellido y contraseña (≥8).
3. Iniciar sesión con Keycloak y unirse a una sesión con el código del operador.

Para reimportar el realm desde cero (borra usuarios del contenedor): `docker compose rm -sf keycloak` y luego `docker compose up mailpit keycloak -d`.

---

## 4. Guion de demo (Entrega 1)

Duración orientativa: **12–15 min**. Requiere Postgres + Keycloak + API + gateway (opcional) + `npm run dev`.

### 4.1 Administrador — catálogo

1. Login **`admin`** / `Umbral123!` → redirige a misiones.
2. Crear o revisar una **misión activa** (nombre, etapas si aplica).
3. Ir a **Trivia** → crear **categoría** y **pregunta** con opciones.
4. Cerrar sesión.

### 4.2 Operador — sesión búsqueda del tesoro

1. Login **`operador`** / `Umbral123!` → `/operador/sesiones`.
2. **Crear sesión** eligiendo la misión activa → anotar el **código de sesión** (único, no por participante).
3. Entrar al **detalle** de la sesión:
   - **Abrir inscripción** (estado `EnPreparacion`).
   - Compartir el código con los jugadores.
   - Ver **participantes inscritos** (solo lectura; se unen solos).
   - Con ≥1 participante: **Iniciar** → temporizador y controles (pausar / reanudar / finalizar).
   - **Refrescar ranking** (poll manual, sin SignalR).
4. *(Opcional)* Intentar `/admin/misiones` → pantalla **403** (rol incorrecto).

### 4.3 Participante — unirse a sesión

**Cliente oficial:** app mobile `src/mobile/umbral-mobile` (`VITE_PARTICIPANTE_WEB_ENABLED=false`).

```bash
cd src/mobile/umbral-mobile
cp .env.example .env && npm install && npx expo start
```

Login (participante) → lobby → partida (QR manual o cámara, trivia, ranking).

Si alguien entra a web con rol Participante, verá el mensaje de acceso deshabilitado (usar mobile).

### 4.4 Qué decir que queda

- Validar escáner QR en teléfono físico.
- Pulido NativeWind / tests mobile / EAS build.

---

## 5. Pendiente de implementar (después de probar el front)

Lista para cerrar E1 / abrir E2, en orden sugerido:

| # | Ítem | Notas |
|---|------|--------|
| 1 | **Afinar E1-2b** | Penalización en UI operador (API ya existe); pulir mensajes/estados vacíos |
| 2 | **Sesiones trivia** | Operador: misión con etapa trivia; participante: mismo listado en `/participante` |
| 3 | **Misiones completas** | CRUD etapas y pistas en admin (si aún incompleto) |
| 4 | **`umbral-mobile`** | Expo + OIDC + mismas APIs de unirse/listar |
| 5 | **Participante web** | ✅ `VITE_PARTICIPANTE_WEB_ENABLED=false` (cliente = mobile) |
| 6 | **SignalR** | Ranking y eventos de sesión en vivo |
| 7 | **Gameplay BT** | Evidencia QR, lobby post-unión, pantallas de juego |
| 8 | **E2E Playwright** | Flujos admin + operador + 403 |
| 9 | **README E1-5 / guion E1-6** | Este README cubre arranque y demo; revisar tras feedback de pruebas |
| 10 | ~~**CI gate cobertura**~~ | ✅ E1-3 — gate ≥90% en CI y local |

---

## 6. Tests y cobertura

### Ejecutar tests

```bash
dotnet test Umbral.sln
# o en Release (como CI):
dotnet test Umbral.sln -c Release
```

**495 tests** en 4 proyectos:

| Proyecto | Qué prueba |
|----------|------------|
| `Umbral.Domain.Tests` | Reglas de dominio (misiones, sesión, identidad) |
| `Umbral.Application.Tests` | Handlers MediatR, validadores |
| `Umbral.Infrastructure.Tests` | Repositorios EF + Postgres (**Testcontainers**) |
| `Umbral.API.Tests` | Controllers HTTP (**Testcontainers** + `TestAuthHandler`, sin JWT real) |

> **Docker debe estar corriendo** para Infrastructure y API tests (levantan Postgres efímero en contenedor).

### Cobertura de código (backend) — RNF-09

Meta: **≥ 90%** de líneas en el backend. Estado actual (última medición): **~96% total**.

| Ensamblado | Cobertura aprox. |
|------------|------------------|
| `Umbral.Domain` | ~91% |
| `Umbral.Application` | ~99% |
| `Umbral.API` | ~96% |
| `Umbral.Infrastructure` | ~98% |

**Windows (recomendado):**

```powershell
# Medir, generar reporte HTML y abrirlo en el navegador
.\scripts\run-coverage.ps1 -Open

# Gate ≥ 90% en total Y en cada ensamblado (Domain, Application, Infrastructure, API)
.\scripts\run-coverage.ps1 -Threshold 90

# Solo exigir 90% en el total (sin gate por ensamblado)
.\scripts\run-coverage.ps1 -Threshold 90 -PerAssembly:$false
```

**Linux / macOS / CI:**

```bash
bash scripts/run-coverage.sh --threshold 90
```

Los scripts hacen todo en un paso: `dotnet test` (Release + coverlet) → XML → reporte HTML.

#### Dónde ver el reporte

| Artefacto | Cómo abrirlo |
|-----------|--------------|
| **`coverage/report/index.html`** | Doble clic, o `.\scripts\run-coverage.ps1 -Open`. En el árbol: **Umbral.Domain**, **Umbral.Application**, **Umbral.Infrastructure**, **Umbral.API** (cada uno con % y clases). |
| **Consola** (`run-coverage.ps1`) | Tabla **Cobertura por ensamblado** con OK/FAIL si usas `-Threshold 90`. |
| **`coverage/report/Summary.txt`** | Texto plano: `Line coverage: XX%` (total) y bloques por ensamblado (líneas sin sangría `  `). |

Con `-Threshold 90` el script falla (exit 1) si **el total** o **cualquiera** de los cuatro ensamblados de producción queda por debajo del 90 %. Eso es independiente del total global: puedes tener 92 % global y fallar en `Umbral.Domain` al 89 %.

La carpeta `coverage/` está en `.gitignore` — **no se commitea**; se regenera en cada corrida. En **GitHub Actions**, el workflow sube el HTML como artefacto `coverage-report` (14 días).

**Comandos manuales** (equivalente a los scripts):

```powershell
dotnet test Umbral.sln -c Release `
  --collect:"XPlat Code Coverage" `
  --settings coverlet.runsettings `
  --results-directory coverage

reportgenerator `
  -reports:"coverage/**/coverage.cobertura.xml" `
  -targetdir:"coverage/report" `
  -reporttypes:"Html;TextSummary"

# Abrir reporte (Windows)
start coverage/report/index.html
```

> Si falta `reportgenerator`: `dotnet tool install -g dotnet-reportgenerator-globaltool`

Más detalle: [`docs/archive/entrega-1/iter-e1-01-cobertura-baseline.md`](docs/archive/entrega-1/iter-e1-01-cobertura-baseline.md) · CI: [`docs/archive/entrega-1/iter-e1-03-ci-coverage.md`](docs/archive/entrega-1/iter-e1-03-ci-coverage.md)

---

## 7. Estructura del proyecto

```
UMBRAL_UCAB/
├── Umbral.sln                        ← Solución .NET
├── docker-compose.yml                ← Infra + api + gateway (Docker)
├── docker/
│   ├── api.Dockerfile
│   └── gateway.Dockerfile
├── .env.example                      ← Plantilla de variables de entorno
├── src/
│   ├── backend/
│   │   ├── Umbral.API/               ← Monolito (:5000)
│   │   └── Umbral.Gateway/           ← YARP edge proxy (:8000)
│   └── frontend/
│       └── umbral-web/               ← React (admin, operador, participante E1)
├── tests/                            ← Domain, Application, Infrastructure, API
├── scripts/
│   ├── run-coverage.ps1              ← Cobertura backend (Windows)
│   └── run-coverage.sh               ← Cobertura backend (Linux/macOS/CI)
├── coverlet.runsettings              ← Exclusiones coverlet (migraciones EF, etc.)
└── docs/
    └── domain-model.md
```

### Regla de dependencias (hexagonal)

```
Umbral.Gateway → (ningún proyecto UMBRAL; solo YARP config)
Umbral.API → Umbral.Application + Umbral.Infrastructure
Umbral.Infrastructure → Umbral.Application + Umbral.Domain
Umbral.Application → Umbral.Domain
Umbral.Domain → (ninguno)
```

---

## 8. Gestión del stack Docker

```bash
# Detener servicios (conserva los volúmenes/datos)
docker compose down

# Detener y eliminar datos (reset completo)
docker compose down -v

# Conectar a PostgreSQL directamente
docker exec -it umbral_postgres psql -U umbral_user -d umbral_db

# Diagnosticar RabbitMQ
curl -u umbral_user:umbral_pass http://localhost:15672/api/healthchecks/node
```
