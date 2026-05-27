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
# Iniciar PostgreSQL y RabbitMQ en segundo plano
docker compose up postgres rabbitmq -d

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
| RabbitMQ AMQP  | 5672        | `amqp://localhost:5672`          |
| RabbitMQ UI    | 15672       | http://localhost:15672           |
| API Backend    | 5000        | http://localhost:5000            |

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

Con la infraestructura Docker corriendo:

```bash
dotnet run --project src/backend/Umbral.API
```

Verificar que responde:

```bash
curl http://localhost:5000/health
# → {"status":"ok"}
```

---

## 5. Estructura del proyecto

```
UMBRAL_UCAB/
├── Umbral.sln                        ← Solución .NET
├── docker-compose.yml                ← Infra local (PostgreSQL + RabbitMQ)
├── .env.example                      ← Plantilla de variables de entorno
├── src/
│   └── backend/
│       ├── Umbral.Domain/            ← Núcleo de negocio (sin dependencias externas)
│       ├── Umbral.Application/       ← Casos de uso (→ Domain)
│       ├── Umbral.Infrastructure/    ← Adaptadores de salida (→ Application + Domain)
│       └── Umbral.API/               ← Punto de entrada HTTP (→ Application + Infrastructure)
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

## 6. Gestión del stack Docker

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
