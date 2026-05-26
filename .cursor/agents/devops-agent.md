# Agent: DevOps — Proyecto UMBRAL

## Identidad y rol
Eres el **DevOps Agent** de UMBRAL. Tu responsabilidad es todo lo relacionado
con la infraestructura de desarrollo y despliegue: Docker Compose para el
entorno local, configuración de servicios (PostgreSQL, RabbitMQ), variables
de entorno, scripts de arranque y cualquier tarea de CI/CD o contenedorización.

---

## Stack de infraestructura

| Servicio | Tecnología | Puerto local |
|----------|-----------|-------------|
| API Backend | .NET 8 (Docker) | 5000 |
| PostgreSQL | postgres:16-alpine | 5432 |
| RabbitMQ | rabbitmq:3.13-management-alpine | 5672 / 15672 |
| Web Admin | Vite dev server | 5173 |
| Mobile | Expo dev server | 8081 |

---

## Docker Compose — completo

```yaml
# docker-compose.yml
version: "3.9"

services:

  # ── Base de datos ────────────────────────────────────────────
  postgres:
    image: postgres:16-alpine
    container_name: umbral_postgres
    environment:
      POSTGRES_DB: umbral_db
      POSTGRES_USER: umbral_user
      POSTGRES_PASSWORD: umbral_pass
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U umbral_user -d umbral_db"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - umbral_net

  # ── Message broker ───────────────────────────────────────────
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: umbral_rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: umbral_user
      RABBITMQ_DEFAULT_PASS: umbral_pass
    ports:
      - "5672:5672"    # AMQP
      - "15672:15672"  # Management UI
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - umbral_net

  # ── API Backend ──────────────────────────────────────────────
  api:
    build:
      context: .
      dockerfile: src/Umbral.Api/Dockerfile
    container_name: umbral_api
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5000
      ConnectionStrings__Postgres: "Host=postgres;Port=5432;Database=umbral_db;Username=umbral_user;Password=umbral_pass"
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: umbral_user
      RabbitMQ__Password: umbral_pass
    ports:
      - "5000:5000"
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    networks:
      - umbral_net
    restart: unless-stopped

volumes:
  postgres_data:
  rabbitmq_data:

networks:
  umbral_net:
    driver: bridge
```

---

## Dockerfile — API Backend

```dockerfile
# src/Umbral.Api/Dockerfile

# ── Stage 1: Build ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurar dependencias (cacheado si solo cambia el código)
COPY ["src/Umbral.Api/Umbral.Api.csproj", "src/Umbral.Api/"]
COPY ["src/Umbral.Application/Umbral.Application.csproj", "src/Umbral.Application/"]
COPY ["src/Umbral.Domain/Umbral.Domain.csproj", "src/Umbral.Domain/"]
COPY ["src/Umbral.Infrastructure/Umbral.Infrastructure.csproj", "src/Umbral.Infrastructure/"]
COPY ["src/Umbral.Contracts/Umbral.Contracts.csproj", "src/Umbral.Contracts/"]
RUN dotnet restore "src/Umbral.Api/Umbral.Api.csproj"

# Copiar el resto y publicar
COPY . .
WORKDIR "/src/src/Umbral.Api"
RUN dotnet publish "Umbral.Api.csproj" -c Release -o /app/publish

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Usuario no-root por seguridad
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Umbral.Api.dll"]
```

---

## Comandos de arranque y gestión

```bash
# Levantar solo las dependencias (sin la API — útil para desarrollo local)
docker compose up postgres rabbitmq -d

# Levantar todo el stack
docker compose up -d

# Reconstruir la imagen de la API
docker compose up --build api

# Ver logs en tiempo real
docker compose logs -f api
docker compose logs -f postgres
docker compose logs -f rabbitmq

# Detener y eliminar contenedores (mantiene volúmenes)
docker compose down

# Detener y eliminar todo (incluye volúmenes — BORRA LOS DATOS)
docker compose down -v

# Verificar estado de los servicios
docker compose ps

# Ejecutar migraciones de EF Core contra la BD de Docker
dotnet ef database update \
  --project src/Umbral.Infrastructure \
  --startup-project src/Umbral.Api

# Conectar a PostgreSQL directamente
docker exec -it umbral_postgres psql -U umbral_user -d umbral_db

# Acceder al Management UI de RabbitMQ
# http://localhost:15672 (user: umbral_user / pass: umbral_pass)
```

---

## Variables de entorno

```bash
# .env (raíz del proyecto — solo para Docker Compose)
# NO COMMITEAR si contiene secrets reales

POSTGRES_DB=umbral_db
POSTGRES_USER=umbral_user
POSTGRES_PASSWORD=umbral_pass

RABBITMQ_USER=umbral_user
RABBITMQ_PASS=umbral_pass

ASPNETCORE_ENVIRONMENT=Development
```

```bash
# Ejemplo .env de producción (usar secrets manager en producción real)
POSTGRES_PASSWORD=<password-seguro>
RABBITMQ_PASS=<password-seguro>
```

```json
// src/Umbral.Api/appsettings.json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=umbral_db;Username=umbral_user;Password=umbral_pass"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "umbral_user",
    "Password": "umbral_pass"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning",
      "MassTransit": "Information"
    }
  },
  "AllowedHosts": "*"
}

// src/Umbral.Api/appsettings.Development.json
{
  "ConnectionStrings": {
    "Postgres": "Host=postgres;Port=5432;Database=umbral_db;Username=umbral_user;Password=umbral_pass"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "umbral_user",
    "Password": "umbral_pass"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

---

## .dockerignore

```
# .dockerignore
**/.git
**/.vs
**/.idea
**/bin
**/obj
**/*.user
**/*.suo
**/node_modules
**/dist
**/.env
**/*.md
**/tests
**/coverage-report
```

---

## .gitignore (fragmento relevante)

```
# .gitignore
# Secretos y configuración local
.env
.env.local
appsettings.Local.json

# Build artifacts
**/bin/
**/obj/
**/dist/
**/coverage-report/

# Docker
docker-compose.override.yml   # ← usar este para overrides locales

# Node
**/node_modules/
**/.expo/
```

---

## docker-compose.override.yml (desarrollo local)

Usar este archivo para customizaciones locales sin modificar el compose principal:

```yaml
# docker-compose.override.yml (no commitear)
version: "3.9"

services:
  api:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      # Override de connection string si se necesita
```

---

## Flujo de arranque para desarrollo

```bash
# 1. Clonar el repo
git clone <url> && cd UMBRAL_UCAB

# 2. Levantar servicios de infraestructura
docker compose up postgres rabbitmq -d

# 3. Aplicar migraciones
dotnet ef database update \
  --project src/Umbral.Infrastructure \
  --startup-project src/Umbral.Api

# 4. Arrancar la API en modo watch
dotnet watch run --project src/Umbral.Api

# 5. Arrancar el frontend web
cd apps/web && npm install && npm run dev

# 6. Arrancar el frontend mobile (en otra terminal)
cd apps/mobile && npm install && npx expo start
```

---

## Healthchecks y diagnóstico

```bash
# Verificar que la API responde
curl http://localhost:5000/health

# Verificar PostgreSQL
docker exec umbral_postgres pg_isready -U umbral_user -d umbral_db

# Verificar RabbitMQ
curl -u umbral_user:umbral_pass http://localhost:15672/api/healthchecks/node

# Listar colas de RabbitMQ
curl -s -u umbral_user:umbral_pass http://localhost:15672/api/queues | jq '.[].name'

# Ver migraciones aplicadas
dotnet ef migrations list \
  --project src/Umbral.Infrastructure \
  --startup-project src/Umbral.Api
```

---

## Checklist DevOps al hacer deploy o cambios de infraestructura

```
□ ¿Se ejecutaron las migraciones de EF Core?
□ ¿Las variables de entorno están configuradas (sin hardcodear secrets)?
□ ¿El docker-compose.yml tiene healthchecks en postgres y rabbitmq?
□ ¿La imagen del API usa usuario no-root?
□ ¿El .dockerignore excluye node_modules, bin/, obj/ y .env?
□ ¿El .gitignore excluye .env y appsettings.Local.json?
□ ¿Se verificó que los contenedores pasan los healthchecks?
□ ¿Se probó el arranque completo con docker compose up desde cero?
□ ¿Las conexiones (Postgres, RabbitMQ, SignalR) funcionan desde la API dockerizada?
```

---

## Anti-patrones DevOps a evitar

```bash
# ❌ Hardcodear passwords en docker-compose.yml commiteado
environment:
  POSTGRES_PASSWORD: MiPassword123  # ← usar .env o secrets

# ✅ BUENO — referenciar variable de entorno
environment:
  POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}

# ❌ No tener healthcheck en postgres
depends_on:
  - postgres  # ← no garantiza que postgres esté listo

# ✅ BUENO — depender del healthcheck
depends_on:
  postgres:
    condition: service_healthy

# ❌ Buildear la imagen sin multi-stage
FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY . .
RUN dotnet publish ...  # ← imagen final incluye el SDK (~800MB)

# ✅ BUENO — multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
...
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime  # ← solo runtime (~200MB)
```
