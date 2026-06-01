# UMBRAL — UCAB Jesus Sayago

Sistema de gestión de sesiones de Búsqueda del Tesoro y Trivia.  
Arquitectura: **Monolito hexagonal** (Ports & Adapters) en .NET 8.

**Trazabilidad (RB / RF / HU):** [`docs/TRAZABILIDAD.md`](docs/TRAZABILIDAD.md) · **Fase 1 dominio:** [`docs/fase-1/TRACKER.md`](docs/fase-1/TRACKER.md)

---

## Requisitos previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) ≥ 24
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

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
# Iniciar PostgreSQL, RabbitMQ y Keycloak
docker compose up postgres rabbitmq keycloak -d

# Verificar que ambos servicios están healthy
docker compose ps

# Ver logs si algo falla
docker compose logs -f postgres
docker compose logs -f rabbitmq
```

### Puertos expuestos

| Servicio       | Puerto local | URL                              |
|----------------|-------------|----------------------------------|
| PostgreSQL     | 5433        | `localhost:5433`                 |
| Keycloak       | 8080        | http://localhost:8080            |
| RabbitMQ AMQP  | 5672        | `amqp://localhost:5672`          |
| RabbitMQ UI    | 15672       | http://localhost:15672           |
| API Backend    | 5000        | http://localhost:5000            |
| Frontend web   | 5173        | http://localhost:5173            |

**Credenciales de desarrollo** (PostgreSQL y RabbitMQ): `umbral_user` / `umbral_pass`  
**Base de datos**: `umbral_db`
**Nota**: se usa `5433` para evitar conflicto con instalaciones locales de PostgreSQL en `5432`.

---

## 3. Compilar el backend

```bash
dotnet build Umbral.sln
```

---

## 4. Ejecutar la API en local

Con la infraestructura Docker corriendo (**PostgreSQL + Keycloak**):

```bash
docker compose up postgres keycloak -d

dotnet run --project src/backend/Umbral.API
```

En **Development** la API aplica migraciones EF al arrancar. Si falla por esquema desactualizado:

```bash
dotnet ef database update --project src/backend/Umbral.Infrastructure --startup-project src/backend/Umbral.API
```

Verificar:

```bash
curl http://localhost:5000/health
# → {"status":"ok"}
```

---

## 5. Frontend web (`umbral-web`)

```bash
cd src/frontend/umbral-web
cp .env.example .env
npm install
npm run dev
```

Abrir **http://localhost:5173/login**.

| Variable | Uso |
|----------|-----|
| `VITE_API_URL` | API REST (default `http://localhost:5000`) |
| `VITE_KEYCLOAK_*` | Realm `umbral`, client `umbral-web` |
| `VITE_EQUIPO_WEB_ENABLED` | `true` = jugador en `/equipo` (E1). `false` cuando exista mobile |

### Usuarios demo (Keycloak)

| Usuario | Contraseña | Rol | Ruta inicial |
|---------|------------|-----|----------------|
| `admin` | `Umbral123!` | Administrador | `/admin/misiones` |
| `operador` | `Umbral123!` | Operador | `/operador/sesiones` |
| `equipo` | `Umbral123!` | EquipoParticipante | `/equipo` (si `VITE_EQUIPO_WEB_ENABLED=true`) |

---

## 6. Guion de demo (Entrega 1)

Duración orientativa: **12–15 min**. Requiere Postgres + Keycloak + API + `npm run dev`.

### 6.1 Administrador — catálogo

1. Login **`admin`** / `Umbral123!` → redirige a misiones.
2. Crear o revisar una **misión activa** (nombre, etapas si aplica).
3. Ir a **Trivia** → crear **categoría** y **pregunta** con opciones.
4. Cerrar sesión.

### 6.2 Operador — sesión búsqueda del tesoro

1. Login **`operador`** / `Umbral123!` → `/operador/sesiones`.
2. **Crear sesión** eligiendo la misión activa → anotar el **código de sesión** (único, no por equipo).
3. Entrar al **detalle** de la sesión:
   - **Abrir inscripción** (estado `EnPreparacion`).
   - Compartir el código con los jugadores.
   - Ver **equipos inscritos** (solo lectura; se unen solos).
   - Con ≥1 equipo: **Iniciar** → temporizador y controles (pausar / reanudar / finalizar).
   - **Refrescar ranking** (poll manual, sin SignalR).
4. *(Opcional)* Intentar `/admin/misiones` → pantalla **403** (rol incorrecto).

### 6.3 Equipo — unirse a sesión (web temporal)

> Cuando exista `umbral-mobile`, poner `VITE_EQUIPO_WEB_ENABLED=false` y repetir el flujo en la app.

1. Login **`equipo`** / `Umbral123!` → `/equipo`.
2. **Búsqueda del tesoro** → listado de sesiones abiertas a inscripción.
3. Ingresar el **código de la sesión** del operador → **Unirse**.
4. El operador ve el equipo en el detalle de la sesión.
5. **Trivia** en `/equipo/trivia`: placeholder (sin sesiones trivia creadas aún).

### 6.4 Qué decir que queda para E2

- Ranking en **tiempo real** (SignalR).
- **Gameplay** BT: escanear QR, enviar evidencias desde mobile.
- **Trivia jugable** y sesiones trivia desde operador.
- App **React Native** (`umbral-mobile`) como cliente definitivo del equipo.

---

## 7. Pendiente de implementar (después de probar el front)

Lista para cerrar E1 / abrir E2, en orden sugerido:

| # | Ítem | Notas |
|---|------|--------|
| 1 | **Afinar E1-2b** | Penalización en UI operador (API ya existe); pulir mensajes/estados vacíos |
| 2 | **Sesiones trivia** | Operador: crear/orquestar sesión trivia; equipo: listado en `/equipo/trivia` |
| 3 | **Misiones completas** | CRUD etapas y pistas en admin (si aún incompleto) |
| 4 | **`umbral-mobile`** | Expo + OIDC + mismas APIs de unirse/listar |
| 5 | **Deshabilitar equipo en web** | `VITE_EQUIPO_WEB_ENABLED=false` al tener mobile |
| 6 | **SignalR** | Ranking y eventos de sesión en vivo |
| 7 | **Gameplay BT** | Evidencia QR, lobby post-unión, pantallas de juego |
| 8 | **E2E Playwright** | Flujos admin + operador + 403 |
| 9 | **README E1-5 / guion E1-6** | Este README cubre arranque y demo; revisar tras feedback de pruebas |
| 10 | **CI gate cobertura** | E1-3 si falta automatizar ≥90% |

---

## 8. Tests

```bash
dotnet test Umbral.sln
```

**389 tests** (Domain, Application, Infrastructure, API). Los de infraestructura usan Testcontainers/Postgres; API usa `TestAuthHandler` sin Keycloak.

### Cobertura de código (backend)

Requisito **RNF-09**: cobertura de líneas ≥ **90%**. Requiere **Docker en marcha** (mismos contenedores que los tests de infra/API).

**Windows (recomendado):**

```powershell
# Medir, generar reporte HTML y abrirlo en el navegador
.\scripts\run-coverage.ps1 -Open

# Verificar el gate ≥ 90% (exit 1 si no cumple; mismo criterio que CI)
.\scripts\run-coverage.ps1 -Threshold 90
```

**Linux / macOS / CI:**

```bash
bash scripts/run-coverage.sh --threshold 90
```

**Salida:**

| Artefacto | Descripción |
|-----------|-------------|
| `coverage/report/index.html` | Reporte HTML por ensamblado (no se commitea; se regenera) |
| `coverage/report/Summary.txt` | Resumen en texto; busca la línea `Line coverage: XX%` |

**Comandos manuales** (sin script):

```powershell
dotnet test Umbral.sln -c Release `
  --collect:"XPlat Code Coverage" `
  --settings coverlet.runsettings `
  --results-directory coverage

reportgenerator `
  -reports:"coverage/**/coverage.cobertura.xml" `
  -targetdir:"coverage/report" `
  -reporttypes:"Html;TextSummary"
```

> Si falta `reportgenerator`: `dotnet tool install -g dotnet-reportgenerator-globaltool`

Más detalle: [`docs/entrega-1/iter-e1-01-cobertura-baseline.md`](docs/entrega-1/iter-e1-01-cobertura-baseline.md) · CI: [`iter-e1-03-ci-coverage.md`](docs/entrega-1/iter-e1-03-ci-coverage.md)

---

## 9. Estructura del proyecto

```
UMBRAL_UCAB/
├── Umbral.sln                        ← Solución .NET
├── docker-compose.yml                ← Infra local (PostgreSQL + RabbitMQ + Keycloak)
├── .env.example                      ← Plantilla de variables de entorno
├── src/
│   ├── backend/                      ← API .NET 8
│   └── frontend/
│       └── umbral-web/               ← React (admin, operador, equipo E1)
├── tests/                            ← Proyectos de prueba (Fase 2+)
└── docs/
    └── domain-model.md
```

### Regla de dependencias (hexagonal)

```
Umbral.API → Umbral.Application + Umbral.Infrastructure
Umbral.Infrastructure → Umbral.Application + Umbral.Domain
Umbral.Application → Umbral.Domain
Umbral.Domain → (ninguno)
```

---

## 10. Gestión del stack Docker

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
