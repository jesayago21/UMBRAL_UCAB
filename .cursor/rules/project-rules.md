# UMBRAL — Reglas del Proyecto

> **Normativa:** Códigos **RB**, **RF**, **RNF** y **HU-01…HU-40** en `docs/TRAZABILIDAD.md` y `.cursor/specs/umbral-product-spec.md`. El ERS académico está en `docs/ERS_Proyecto_UMBRAL_UCAB.md` (sincronizado §7–§8).

## 1. Estructura del repositorio

El repositorio tiene la siguiente estructura raíz. No la modifiques sin consenso del equipo:
UMBRAL_UCAB/
├── .cursor/                         → Configuración de Cursor AI
├── .github/
│   └── workflows/
│       └── ci.yml                   → Pipeline de integración continua
src/
├── backend/
│   ├── Umbral.Domain/
│   ├── Umbral.Application/
│   ├── Umbral.Infrastructure/
│   └── Umbral.API/
├── frontend/
│   └── umbral-web/          → React + Vite (Admin + Operador)
└── mobile/
    └── umbral-mobile/       → React Native + Expo (Equipo Participante)
├── tests/
│   ├── Umbral.Domain.Tests/            → Pruebas unitarias del dominio
│   ├── Umbral.Application.Tests/       → Pruebas unitarias de handlers
│   ├── Umbral.Infrastructure.Tests/    → Persistencia + Testcontainers (PostgreSQL)
│   └── Umbral.API.Tests/               → API HTTP + WebApplicationFactory + Testcontainers
│   (Umbral.E2E.Tests con Playwright → Entrega 2, aún no existe en el repo)
├── docker/
│   ├── backend.Dockerfile
│   └── frontend.Dockerfile
├── docker-compose.yml               → Orquestación local completa
├── docker-compose.override.yml      → Overrides para desarrollo local
├── .cursorrules
└── README.md
---

## 2. Reglas de dependencias entre proyectos .NET

La única dirección válida de referencias es la siguiente:

Umbral.API          → Umbral.Application + Umbral.Infrastructure
Umbral.Infrastructure → Umbral.Application
Umbral.Application  → Umbral.Domain
Umbral.Domain       → (ninguno)

**Verificación:**
- `Umbral.Domain.csproj`        → sin referencias a otros proyectos UMBRAL.
- `Umbral.Application.csproj`   → solo referencia a `Umbral.Domain`.
- `Umbral.Infrastructure.csproj`→ referencia a `Umbral.Application` y `Umbral.Domain`.
- `Umbral.API.csproj`            → referencia a todos los anteriores.

Si en algún momento el compilador exige agregar una referencia que viola este orden,
eso es señal de que hay un error de diseño. Corrígelo moviendo la clase al proyecto correcto.

---

## 3. Bounded Contexts — separación por carpetas

Dentro de cada proyecto, el código se organiza por Bounded Context y luego por tipo:

Umbral.Domain/
├── CatalogoBusquedaTesoro/          → BC: Catálogo de Misiones
│   ├── Mision/
│   │   ├── Mision.cs                → AggregateRoot
│   │   ├── Etapa.cs                 → Entity
│   │   ├── Pista.cs                 → Entity
│   │   ├── MisionSnapshot.cs        → ValueObject (copia inmutable)
│   │   ├── EtapaSnapshot.cs         → ValueObject
│   │   ├── MisionId.cs
│   │   ├── EstadoMision.cs
│   │   ├── TipoLiberacion.cs
│   │   └── IMisionRepository.cs     → Port
├── CatalogoTrivia/                  → BC: Catálogo de Trivia
│   ├── Pregunta/
│   │   ├── Pregunta.cs              → AggregateRoot
│   │   ├── OpcionRespuesta.cs       → ValueObject
│   │   ├── PreguntaId.cs
│   │   └── IPreguntaRepository.cs   → Port
│   └── Categoria/
│       ├── Categoria.cs             → AggregateRoot
│       ├── CategoriaId.cs
│       └── ICategoriaRepository.cs  → Port
├── Sesion/                          → BC: Ejecución de Sesión
│   ├── Sesion.cs                    → AggregateRoot
│   ├── ContextoBusquedaTesoro.cs    → Entity (solo si TipoSesion=BusquedaTesoro)
│   ├── ContextoTrivia.cs            → Entity (solo si TipoSesion=Trivia)
│   ├── EquipoSesion.cs              → Entity
│   ├── Evidencia.cs                 → Entity (BusquedaTesoro)
│   ├── RespuestaTrivia.cs           → Entity (Trivia)
│   ├── Penalizacion.cs              → ValueObject
│   ├── EventoSesion.cs              → Entity
│   ├── TipoSesion.cs                → Enum
│   ├── EstadoSesion.cs              → Enum
│   ├── EstadoRespuesta.cs           → Enum
│   ├── ResultadoValidacion.cs       → Enum
│   └── ISesionRepository.cs         → Port
├── Shared/
│   ├── DomainException.cs
│   ├── ValueObject.cs
│   ├── AggregateRoot.cs
│   ├── Entity.cs
│   └── IDomainEvent.cs
└── Ports/
    ├── IEventPublisher.cs
    └── INotificacionRealTime.cs

Umbral.Application/
├── Catalogo/
│   ├── Commands/
│   └── Queries/
├── Sesion/
│   ├── Commands/
│   └── Queries/
└── Common/
├── Behaviors/
│   ├── ValidationBehavior.cs
│   └── LoggingBehavior.cs
└── Interfaces/
└── ICurrentUserService.cs

Umbral.Infrastructure/
├── Persistence/
│   ├── UmbralDbContext.cs
│   ├── Configurations/
│   ├── Migrations/
│   └── Repositories/
├── Messaging/
│   ├── Publishers/
│   └── Consumers/
├── RealTime/
│   └── Hubs/
└── Identity/

Umbral.API/
├── Controllers/
│   ├── MisionesController.cs
│   ├── SesionesController.cs
│   ├── EquiposController.cs
│   └── PreguntasController.cs
├── Middlewares/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── WebApplicationExtensions.cs
└── Program.cs

---

## 4. Registro de dependencias — regla de organización

Cada capa registra sus propias dependencias mediante métodos de extensión:

```csharp
// En Umbral.Application
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(ApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(
            typeof(ApplicationAssemblyMarker).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));
        return services;
    }
}

// En Umbral.Infrastructure
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UmbralDbContext>(...);
        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IMisionRepository, MisionRepository>();
        // RabbitMQ, SignalR, etc.
        return services;
    }
}

// En Program.cs de API (limpio)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

---

## 5. Reglas de la API REST

### 5.1 Rutas
Todas las rutas siguen el patrón:

/api/v1/{recurso-plural}
/api/v1/{recurso-plural}/{id}
/api/v1/{recurso-plural}/{id}/{sub-recurso}

Ejemplos:

GET    /api/v1/misiones
POST   /api/v1/misiones
GET    /api/v1/misiones/{id}
PUT    /api/v1/misiones/{id}
POST   /api/v1/sesiones
POST   /api/v1/sesiones/{id}/iniciar
POST   /api/v1/sesiones/{id}/equipos
POST   /api/v1/sesiones/{id}/pistas/{pistaId}/liberar
GET    /api/v1/sesiones/{id}/ranking

### 5.2 Códigos HTTP
| Situación                        | Código         |
|----------------------------------|----------------|
| Creación exitosa                 | 201 Created    |
| Consulta exitosa                 | 200 OK         |
| Operación exitosa sin respuesta  | 204 No Content |
| Error de validación              | 400 Bad Request|
| No autenticado                   | 401 Unauthorized|
| Sin permiso                      | 403 Forbidden  |
| Recurso no encontrado            | 404 Not Found  |
| Error interno                    | 500 Internal Server Error |

### 5.3 Formato de error estándar
```json
{
  "tipo": "ValidationError",
  "mensaje": "La solicitud contiene errores de validación.",
  "errores": {
    "misionId": ["El identificador de la misión es obligatorio."],
    "nombre": ["El nombre no puede estar vacío."]
  },
  "traceId": "00-abc123-def456-00"
}
```

### 5.4 Controllers delgados
```csharp
[ApiController]
[Route("api/v1/sesiones")]
[Authorize]
public class SesionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SesionesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Operador,Administrador")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearSesionRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new CrearSesionCommand(
            request.MisionId,
            request.OperadorId), ct);

        return CreatedAtAction(nameof(ObtenerPorId),
            new { id = result.Value }, result.Value);
    }
}
```

---

## 6. Reglas de WebSockets / SignalR

### 6.1 Hubs disponibles

/hubs/sesion     → Eventos de sesión (estado, etapa, ranking, pistas)
/hubs/trivia     → Eventos de trivia (pregunta lanzada, tiempo, respuestas)

### 6.2 Grupos por sesión
Cada cliente se une a un grupo identificado por `sesionId` al conectarse.
El operador además se une al grupo `operador-{sesionId}`.

### 6.3 Eventos que el servidor emite al cliente

sesion:estado-cambiado    → { sesionId, nuevoEstado }
sesion:etapa-avanzada     → { sesionId, etapaIndex }
sesion:pista-liberada     → { sesionId, equipoId, pistaId, contenido }
sesion:ranking-actualizado→ { sesionId, ranking: PosicionDto[] }
sesion:penalizacion       → { sesionId, equipoId, puntos, motivo }
trivia:pregunta-lanzada   → { preguntaId, enunciado, opciones, timerMs }
trivia:tiempo-agotado     → { preguntaId }
trivia:resultado-ronda    → { respuestaCorrectaId, ranking: PosicionDto[] }

### 6.4 Regla de emisión
Los hubs **NO** ejecutan lógica de negocio. Solo emiten.
La lógica ocurre en el handler → publica evento de dominio →
consumer de RabbitMQ o pipeline behavior → llama al hub para notificar.

---

## 7. Reglas de RabbitMQ

### 7.1 Exchanges y colas

Exchange: umbral.domain.events   (type: topic)
Routing keys:
sesion.creada
sesion.iniciada
sesion.pausada
sesion.finalizada
sesion.etapa-completada
sesion.evidencia-validada
sesion.pista-liberada
sesion.penalizacion-aplicada
trivia.pregunta-lanzada
trivia.respuesta-recibida
trivia.tiempo-agotado
Colas:
umbral.auditoria          → consume todos los eventos
umbral.puntaje            → consume evidencia-validada, penalizacion-aplicada
umbral.notificaciones     → consume pista-liberada, estado-cambiado
umbral.trivia             → consume trivia.*
umbral.dlq                → mensajes con error de procesamiento

### 7.2 Regla de publicación
Los events publishers son llamados desde los handlers de Application
a través del puerto `IEventPublisher`. Nunca directamente desde el dominio.

### 7.3 Consumers
Cada consumer tiene una única responsabilidad.
Si el procesamiento falla, el mensaje va a la DLQ después de 3 reintentos.

---

## 8. Docker Compose — reglas de configuración

### 8.1 Servicios obligatorios en docker-compose.yml
```yaml
services:
  db:         # PostgreSQL 16
  rabbitmq:   # RabbitMQ 3 con management plugin
  backend:    # Umbral.API
  frontend:   # umbral-web (nginx en prod, vite dev server en dev)
```

### 8.2 Variables de entorno
- Nunca hardcodees connection strings en el código.
- Siempre usar variables de entorno con valores por defecto para desarrollo local.
- El archivo `.env` va en `.gitignore`. Se provee `.env.example` en el repo.

### 8.3 Health checks
Todos los servicios tienen `healthcheck` configurado.
El backend espera que `db` y `rabbitmq` estén healthy antes de iniciar:
```yaml
depends_on:
  db:
    condition: service_healthy
  rabbitmq:
    condition: service_healthy
```

---

## 9. Pipeline de CI — reglas mínimas

El archivo `.github/workflows/ci.yml` debe ejecutar en cada PR a `main`:

1. Checkout del código
2. Setup .NET 8
3. dotnet restore
4. dotnet build --no-restore
5. dotnet test --no-build --collect:"XPlat Code Coverage"
6. Publicar reporte de cobertura (Codecov o similar)
7. Setup Node.js
8. npm ci (en frontend)
9. npm run type-check
10. npm run test (Vitest)

Si cualquier paso falla, el PR no puede mergearse.
La cobertura mínima del backend es **90%**. El pipeline falla si no se alcanza.

---

## 10. Seguridad básica — reglas

### 10.1 Roles del sistema

Administrador      → gestión de misiones, catálogo, usuarios
Operador           → gestión y ejecución de sesiones
EquipoParticipante → acceso solo a su sesión activa

### 10.2 JWT
- El token incluye: `userId`, `rol`, `sesionId` (para equipos).
- Expiración: 8 horas para Admin/Operador, duración de la sesión para Equipos.
- Refresh token implementado para equipos durante sesiones activas.

### 10.3 Autorización por endpoint
- Todo endpoint requiere autenticación excepto: login y health check.
- Los endpoints de operación requieren rol `Operador` o `Administrador`.
- Los endpoints de participación validan que el equipo pertenece a la sesión.

### 10.4 Datos sensibles
- Passwords hasheados con BCrypt. Nunca almacenados en texto plano.
- Códigos QR nunca expuestos completos en logs ni en respuestas de listing.
- Los tokens JWT no se loguean nunca.

---

## 11. Lo que NO está en el alcance (no implementar)

Si el contexto de la conversación te lleva hacia alguna de estas áreas, detente y avisa:

- ❌ Cobros en línea o integración con pasarelas de pago.
- ❌ Geolocalización o integración con dispositivos físicos.
- ❌ Módulos de analítica histórica o dashboards complejos.
- ❌ Inteligencia artificial aplicada al contenido de misiones.
- ❌ Apps nativas puras (Swift/Kotlin sin Expo). **Sí está en alcance:** `umbral-mobile` (React Native + Expo) para el equipo participante.
- ❌ Múltiples deployables o arquitectura de microservicios.
- ❌ Multi-tenancy o soporte para múltiples organizaciones.