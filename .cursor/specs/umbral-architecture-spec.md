# UMBRAL — Especificación de Arquitectura

> **Trazabilidad:** `docs/TRAZABILIDAD.md` · **Producto:** `umbral-product-spec.md`

## 1. Estilo arquitectónico

UMBRAL es un **monolito con arquitectura hexagonal** (Ports & Adapters).
Todo el sistema vive en un único proceso deployable organizado en capas
concéntricas. No hay microservicios, no hay múltiples deployables.

┌─────────────────────────────────────────────────┐
│                    API Layer                     │  ← Adaptadores de entrada
│         (Controllers, Hubs, Middlewares)         │
├─────────────────────────────────────────────────┤
│               Application Layer                  │  ← Casos de uso
│      (Commands, Queries, Handlers, Behaviors)    │
├─────────────────────────────────────────────────┤
│                 Domain Layer                     │  ← Núcleo de negocio
│  (Aggregates, Entities, Value Objects, Services) │
├─────────────────────────────────────────────────┤
│             Infrastructure Layer                 │  ← Adaptadores de salida
│    (EF Core, RabbitMQ, SignalR, Repositories)   │
└─────────────────────────────────────────────────┘

---

## 2. Proyectos .NET y sus responsabilidades

### Umbral.Domain
Núcleo puro del negocio. Sin dependencias externas de ningún tipo.

Contiene:
- Aggregates, Entities, Value Objects
- Domain Services
- Domain Events
- Interfaces de repositorios (puertos driven)
- Interfaces de servicios externos (IEventPublisher, INotificacionRealTime)
- Excepciones de dominio

Dependencias externas permitidas: **ninguna**.
NuGet permitidos: ninguno (solo .NET BCL).

Umbral.Domain.csproj
→ sin ProjectReference
→ sin PackageReference externo

---

### Umbral.Application
Orquesta los casos de uso. Conoce el dominio, ignora la infraestructura.

Contiene:
- Commands y Queries (objetos de entrada MediatR)
- Command Handlers y Query Handlers
- Validators (FluentValidation) por cada Command
- Pipeline Behaviors (Validación, Logging, Transacción)
- DTOs de respuesta (records)
- Interfaces propias de Application (ICurrentUserService, IDateTimeProvider)

Dependencias externas permitidas:
- `MediatR`
- `FluentValidation`

Umbral.Application.csproj
→ ProjectReference: Umbral.Domain
→ PackageReference: MediatR, FluentValidation

---

### Umbral.Infrastructure
Implementa todos los contratos definidos en capas interiores.

Contiene:
- `UmbralDbContext` (EF Core)
- Configuraciones Fluent API por entidad
- Migraciones
- Implementaciones de repositorios
- `EventPublisher` (MassTransit → RabbitMQ)
- Consumers de RabbitMQ
- `NotificacionRealTimeService` (SignalR)
- Hubs de SignalR
- Servicios de identidad y JWT

Dependencias externas permitidas:
- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `MassTransit.RabbitMQ`
- `Microsoft.AspNetCore.SignalR`

Umbral.Infrastructure.csproj
→ ProjectReference: Umbral.Application, Umbral.Domain
→ PackageReference: EFCore, Npgsql, MassTransit, SignalR

---

### Umbral.API
Punto de entrada del sistema. Delgado y sin lógica de negocio.

Contiene:
- `Program.cs` con composición de dependencias
- Controllers REST
- Middlewares (ExceptionHandling, RequestLogging)
- Extensiones de configuración por capa
- Configuración de Swagger, JWT, CORS

Dependencias externas permitidas:
- `Microsoft.AspNetCore` (implícito)
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore`

Umbral.API.csproj
→ ProjectReference: Umbral.Application, Umbral.Infrastructure
→ PackageReference: Serilog, Swashbuckle

---

## 3. Regla de dependencias — diagrama

                ┌─────────────┐
                │ Umbral.API  │
                └──────┬──────┘
                       │ referencia
          ┌────────────┴────────────┐
          ▼                         ▼
┌───────────────────┐    ┌────────────────────────┐
│ Umbral.Application│    │ Umbral.Infrastructure  │
└─────────┬─────────┘    └───────────┬────────────┘
          │ referencia               │ referencia
          └──────────────┬───────────┘
                         ▼
                ┌─────────────────┐
                │  Umbral.Domain  │
                └─────────────────┘
PROHIBIDO:
Umbral.Domain       → cualquier otro proyecto UMBRAL
Umbral.Application  → Umbral.Infrastructure
Umbral.Application  → Umbral.API

---

## 4. Flujo de una operación de escritura (Command)

HTTP Request
│
▼
Controller (API)
│ new CrearSesionCommand(...)
▼
MediatR Pipeline
│
├─► ValidationBehavior → FluentValidation → lanza si inválido
├─► LoggingBehavior    → log entrada/salida
│
▼
CrearSesionCommandHandler (Application)
│
├─► IMisionRepository.FindByIdAsync()     → recupera agregado
├─► Sesion.Crear(...)                      → lógica de dominio
├─► ISesionRepository.SaveAsync()          → persiste
└─► IEventPublisher.PublishBatchAsync()    → publica Domain Events
│
▼
RabbitMQ Exchange
│
├─► AuditoriaConsumer
├─► PuntajeConsumer
└─► NotificacionesConsumer
│
▼
SignalR Hub → WebSocket → Clientes React


---

## 5. Flujo de una operación de lectura (Query)

HTTP Request
│
▼
Controller (API)
│ new GetRankingSesionQuery(sesionId)
▼
MediatR Pipeline
│
├─► LoggingBehavior
│
▼
GetRankingSesionQueryHandler (Application)
│
└─► UmbralDbContext (directo, sin repositorio)
│ SELECT optimizado con proyección a DTO
▼
List<PosicionRankingDto>
│
▼
Controller → HTTP 200 + JSON


Las Queries acceden directamente al DbContext para proyecciones optimizadas.
No pasan por repositorios ni reconstruyen agregados del dominio.

---

## 6. Flujo de tiempo real (WebSocket)

Evento de dominio ocurre en un Consumer de RabbitMQ
│
▼
Consumer (Infrastructure)
│ inyecta INotificacionRealTime
▼
NotificacionRealTimeService (Infrastructure)
│ envuelve IHubContext<SesionHub>
▼
SignalR Hub
│
├─► Grupo "sesion-{sesionId}"     → todos los clientes de la sesión
├─► Grupo "operador-{sesionId}"   → solo el operador
└─► Grupo "equipo-{equipoId}"     → solo un equipo específico
│
▼
React Client
│ useSessionSocket() hook
▼
Actualización de estado local → re-render


---

## 7. Organización de carpetas completa

```
umbral/
├── .cursor/
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── backend/
│   │   ├── Umbral.Domain/
│   │   │   ├── CatalogoBusquedaTesoro/
│   │   │   │   └── Mision/
│   │   │   │       ├── Mision.cs
│   │   │   │       ├── Etapa.cs
│   │   │   │       ├── Pista.cs
│   │   │   │       ├── MisionSnapshot.cs
│   │   │   │       ├── EtapaSnapshot.cs
│   │   │   │       ├── MisionId.cs
│   │   │   │       ├── EstadoMision.cs
│   │   │   │       ├── TipoLiberacion.cs
│   │   │   │       └── IMisionRepository.cs
│   │   │   ├── CatalogoTrivia/
│   │   │   │   ├── Pregunta/
│   │   │   │   │   ├── Pregunta.cs
│   │   │   │   │   ├── OpcionRespuesta.cs
│   │   │   │   │   ├── PreguntaId.cs
│   │   │   │   │   ├── Dificultad.cs
│   │   │   │   │   └── IPreguntaRepository.cs
│   │   │   │   └── Categoria/
│   │   │   │       ├── Categoria.cs
│   │   │   │       ├── CategoriaId.cs
│   │   │   │       └── ICategoriaRepository.cs
│   │   │   ├── Sesion/
│   │   │   │   ├── Sesion.cs
│   │   │   │   ├── ContextoBusquedaTesoro.cs
│   │   │   │   ├── ContextoTrivia.cs
│   │   │   │   ├── EquipoSesion.cs
│   │   │   │   ├── Evidencia.cs
│   │   │   │   ├── RespuestaTrivia.cs
│   │   │   │   ├── Penalizacion.cs
│   │   │   │   ├── EventoSesion.cs
│   │   │   │   ├── TipoSesion.cs
│   │   │   │   ├── EstadoSesion.cs
│   │   │   │   ├── EstadoRespuesta.cs
│   │   │   │   ├── ResultadoValidacion.cs
│   │   │   │   └── ISesionRepository.cs
│   │   │   ├── Shared/
│   │   │   │   ├── AggregateRoot.cs
│   │   │   │   ├── Entity.cs
│   │   │   │   ├── ValueObject.cs
│   │   │   │   ├── DomainException.cs
│   │   │   │   └── IDomainEvent.cs
│   │   │   └── Ports/
│   │   │       ├── IEventPublisher.cs
│   │   │       └── INotificacionRealTime.cs
│   │   │
│   │   ├── Umbral.Application/
│   │   │   ├── CatalogoBusquedaTesoro/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CrearMision/
│   │   │   │   │   │   ├── CrearMisionCommand.cs
│   │   │   │   │   │   ├── CrearMisionCommandHandler.cs
│   │   │   │   │   │   └── CrearMisionCommandValidator.cs
│   │   │   │   │   ├── EditarMision/
│   │   │   │   │   ├── ActivarMision/
│   │   │   │   │   └── DesactivarMision/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetMisionById/
│   │   │   │       └── ListMisiones/
│   │   │   ├── CatalogoTrivia/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CrearPregunta/
│   │   │   │   │   ├── EditarPregunta/
│   │   │   │   │   ├── EliminarPregunta/
│   │   │   │   │   ├── CrearCategoria/
│   │   │   │   │   └── EliminarCategoria/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetPreguntaById/
│   │   │   │       └── ListPreguntasByCategoria/
│   │   │   ├── Sesion/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CrearSesionBusquedaTesoro/
│   │   │   │   │   │   ├── CrearSesionBusquedaTesoroCommand.cs
│   │   │   │   │   │   ├── CrearSesionBusquedaTesoroCommandHandler.cs
│   │   │   │   │   │   └── CrearSesionBusquedaTesoroCommandValidator.cs
│   │   │   │   │   ├── CrearSesionTrivia/
│   │   │   │   │   ├── IniciarSesion/
│   │   │   │   │   ├── PausarSesion/
│   │   │   │   │   ├── ReanudarSesion/
│   │   │   │   │   ├── FinalizarSesion/
│   │   │   │   │   ├── CancelarSesion/
│   │   │   │   │   ├── RegistrarEquipo/
│   │   │   │   │   ├── AplicarPenalizacion/
│   │   │   │   │   ├── LiberarPista/
│   │   │   │   │   ├── SubmitEvidencia/
│   │   │   │   │   ├── LanzarPreguntaTrivia/
│   │   │   │   │   └── SubmitRespuestaTrivia/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetSesionById/
│   │   │   │       ├── GetRankingSesion/
│   │   │   │       ├── GetHistorialEventos/
│   │   │   │       └── GetEstadoSesion/
│   │   │   └── Common/
│   │   │       ├── Behaviors/
│   │   │       │   ├── ValidationBehavior.cs
│   │   │       │   └── LoggingBehavior.cs
│   │   │       ├── Interfaces/
│   │   │       │   ├── ICurrentUserService.cs
│   │   │       │   └── IDateTimeProvider.cs
│   │   │       └── ApplicationAssemblyMarker.cs
│   │   │
│   │   ├── Umbral.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── UmbralDbContext.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── MisionConfiguration.cs
│   │   │   │   │   ├── EtapaConfiguration.cs
│   │   │   │   │   ├── PistaConfiguration.cs
│   │   │   │   │   ├── PreguntaConfiguration.cs
│   │   │   │   │   ├── CategoriaConfiguration.cs
│   │   │   │   │   ├── SesionConfiguration.cs
│   │   │   │   │   ├── ContextoBusquedaTesoroConfiguration.cs
│   │   │   │   │   ├── ContextoTriviaConfiguration.cs
│   │   │   │   │   ├── EquipoSesionConfiguration.cs
│   │   │   │   │   ├── EvidenciaConfiguration.cs
│   │   │   │   │   └── RespuestaTriviaConfiguration.cs
│   │   │   │   ├── Migrations/
│   │   │   │   └── Repositories/
│   │   │   │       ├── MisionRepository.cs
│   │   │   │       ├── SesionRepository.cs
│   │   │   │       ├── PreguntaRepository.cs
│   │   │   │       └── CategoriaRepository.cs
│   │   │   ├── Messaging/
│   │   │   │   ├── Publishers/
│   │   │   │   │   └── EventPublisher.cs
│   │   │   │   └── Consumers/
│   │   │   │       ├── AuditoriaConsumer.cs
│   │   │   │       ├── PuntajeBusquedaConsumer.cs
│   │   │   │       ├── PuntajeTriviaConsumer.cs
│   │   │   │       ├── NotificacionesConsumer.cs
│   │   │   │       └── TransicionPreguntaConsumer.cs
│   │   │   ├── RealTime/
│   │   │   │   ├── Hubs/
│   │   │   │   │   ├── SesionHub.cs
│   │   │   │   │   └── TriviaHub.cs
│   │   │   │   └── NotificacionRealTimeService.cs
│   │   │   ├── Identity/
│   │   │   │   ├── JwtTokenService.cs
│   │   │   │   └── CurrentUserService.cs
│   │   │   └── Extensions/
│   │   │       └── InfrastructureServiceExtensions.cs
│   │   │
│   │   └── Umbral.API/
│   │       ├── Controllers/
│   │       │   ├── MisionesController.cs
│   │       │   ├── SesionesController.cs
│   │       │   ├── EquiposController.cs
│   │       │   ├── PreguntasController.cs
│   │       │   ├── CategoriasController.cs
│   │       │   └── AuthController.cs
│   │       ├── Middlewares/
│   │       │   ├── ExceptionHandlingMiddleware.cs
│   │       │   └── RequestLoggingMiddleware.cs
│   │       ├── Extensions/
│   │       │   ├── ApplicationServiceExtensions.cs
│   │       │   └── WebApplicationExtensions.cs
│   │       └── Program.cs
│   │
│   ├── frontend/
│   │   └── umbral-web/              → React + Vite (Admin + Operador)
│   │       ├── src/
│   │       │   ├── components/
│   │       │   │   ├── shared/
│   │       │   │   │   ├── Button/
│   │       │   │   │   ├── RankingList/
│   │       │   │   │   ├── Timer/
│   │       │   │   │   ├── Badge/
│   │       │   │   │   └── LoadingSpinner/
│   │       │   │   ├── operator/
│   │       │   │   │   ├── SessionControl/
│   │       │   │   │   ├── PenaltyForm/
│   │       │   │   │   ├── HintReleasePanel/
│   │       │   │   │   └── TeamStatusCard/
│   │       │   │   └── admin/
│   │       │   │       ├── MisionForm/
│   │       │   │       ├── EtapaForm/
│   │       │   │       └── PreguntaForm/
│   │       │   ├── pages/
│   │       │   │   ├── admin/
│   │       │   │   │   ├── MisionesPage.tsx
│   │       │   │   │   ├── MisionDetailPage.tsx
│   │       │   │   │   ├── PreguntasPage.tsx
│   │       │   │   │   └── CategoriasPage.tsx
│   │       │   │   ├── operator/
│   │       │   │   │   ├── SesionesPage.tsx
│   │       │   │   │   ├── OperatorDashboardPage.tsx
│   │       │   │   │   └── WaitingRoomPage.tsx
│   │       │   │   └── auth/
│   │       │   │       └── LoginPage.tsx
│   │       │   ├── hooks/
│   │       │   │   ├── useSessionSocket.ts
│   │       │   │   ├── useRanking.ts
│   │       │   │   └── useAuth.ts
│   │       │   ├── services/
│   │       │   │   ├── apiClient.ts
│   │       │   │   ├── sesionService.ts
│   │       │   │   ├── misionService.ts
│   │       │   │   ├── preguntaService.ts
│   │       │   │   ├── categoriaService.ts
│   │       │   │   └── authService.ts
│   │       │   ├── store/
│   │       │   │   ├── authStore.ts
│   │       │   │   └── sesionStore.ts
│   │       │   ├── types/               → tipos compartidos con mobile
│   │       │   ├── router/
│   │       │   │   └── AppRouter.tsx
│   │       │   ├── lib/
│   │       │   │   └── signalr.ts
│   │       │   ├── App.tsx
│   │       │   └── main.tsx
│   │       ├── index.html
│   │       ├── vite.config.ts
│   │       └── tsconfig.json
│   │
│   └── mobile/
│       └── umbral-mobile/           → React Native + Expo (Equipo Participante)
│           ├── src/
│           │   ├── components/
│           │   │   ├── shared/
│           │   │   │   ├── RankingList/
│           │   │   │   ├── Timer/
│           │   │   │   ├── ScoreDisplay/
│           │   │   │   └── ConnectionBadge/
│           │   │   ├── busqueda/
│           │   │   │   ├── HintList/
│           │   │   │   └── QRScanner/
│           │   │   └── trivia/
│           │   │       ├── TriviaQuestion/
│           │   │       ├── OptionButton/
│           │   │       └── RoundResult/
│           │   ├── screens/
│           │   │   ├── auth/
│           │   │   │   └── JoinScreen.tsx
│           │   │   ├── busqueda/
│           │   │   │   └── BusquedaDashboardScreen.tsx
│           │   │   ├── trivia/
│           │   │   │   ├── TriviaDashboardScreen.tsx
│           │   │   │   └── WaitingScreen.tsx
│           │   │   └── shared/
│           │   │       ├── RankingScreen.tsx
│           │   │       └── SessionEndScreen.tsx
│           │   ├── hooks/
│           │   │   ├── useSessionSocket.ts
│           │   │   ├── useTimer.ts
│           │   │   ├── useTrivia.ts
│           │   │   └── useBusquedaTesoro.ts
│           │   ├── services/
│           │   │   ├── apiClient.ts
│           │   │   ├── sesionService.ts
│           │   │   └── equipoService.ts
│           │   ├── store/
│           │   │   └── authStore.ts   → AsyncStorage (no localStorage)
│           │   ├── types/             → mismos tipos que umbral-web
│           │   └── navigation/
│           │       └── AppNavigator.tsx
│           ├── app.json
│           ├── babel.config.js
│           └── tsconfig.json
│
├── tests/
│   ├── Umbral.Domain.Tests/
│   ├── Umbral.Application.Tests/
│   ├── Umbral.Integration.Tests/
│   └── Umbral.E2E.Tests/
├── docs/
│   └── domain-model.md
├── docker/
│   ├── backend.Dockerfile
│   └── frontend.Dockerfile
├── docker-compose.yml
├── docker-compose.override.yml
├── .cursorrules
└── README.md
```

## 8. Puertos y adaptadores

### Puertos Driven (definidos en Domain)

| Puerto                  | Propósito                                    | Implementación (Infrastructure)        |
|-------------------------|----------------------------------------------|----------------------------------------|
| `IMisionRepository`     | Persistir y recuperar Misiones               | `MisionRepository` (EF Core)           |
| `ISesionRepository`     | Persistir y recuperar Sesiones               | `SesionRepository` (EF Core)           |
| `IPreguntaRepository`   | Persistir y recuperar Preguntas              | `PreguntaRepository` (EF Core)         |
| `ICategoriaRepository`  | Persistir y recuperar Categorías             | `CategoriaRepository` (EF Core)        |
| `IEventPublisher`       | Publicar Domain Events                       | `EventPublisher` (MassTransit)         |
| `INotificacionRealTime` | Notificar clientes en tiempo real            | `NotificacionRealTimeService` (SignalR)|

### Puertos Driving (adaptadores de entrada en API)
| Adaptador               | Protocolo          | Responsabilidad                          | Consumidor       |
|-------------------------|--------------------|------------------------------------------|------------------|
| `MisionesController`    | HTTP REST          | CRUD de misiones                         | Web (Admin)      |
| `SesionesController`    | HTTP REST          | Ciclo de vida de sesiones                | Web (Operador)   |
| `EquiposController`     | HTTP REST          | Registro y consulta de equipos           | Web + Mobile     |
| `PreguntasController`   | HTTP REST          | CRUD del banco de preguntas              | Web (Admin)      |
| `CategoriasController`  | HTTP REST          | CRUD de categorías                       | Web (Admin)      |
| `AuthController`        | HTTP REST          | Login y generación de JWT                | Web + Mobile     |
| `SesionHub`             | WebSocket/SignalR  | Eventos de sesión y BT en tiempo real    | Web + Mobile     |
| `TriviaHub`             | WebSocket/SignalR  | Eventos de trivia en tiempo real         | Mobile           |

---

## 9. Persistencia — modelo de tablas principal

```sql
-- BC: Catálogo de Misiones (BT)
misiones                (id, nombre, descripcion, nivel_dificultad,
                         tiempo_maximo_seg, estado, created_at)
etapas                  (id, mision_id, orden, tiempo_maximo_seg,
                         tiempo_sin_ganador_seg, codigo_qr_solucion)
pistas                  (id, etapa_id, contenido, orden, tipo_liberacion)

-- BC: Catálogo de Trivia
categorias              (id, nombre, descripcion, total_preguntas)
preguntas               (id, enunciado, categoria_id, dificultad,
                         tiempo_respuesta_ms, is_deleted)
opciones_respuesta      (id, pregunta_id, texto, is_correcta)

-- BC: Ejecución de Sesión
sesiones                (id, tipo_sesion, operador_id, estado,
                         iniciada_en, finalizada_en)
contextos_bt            (id, sesion_id, mision_snapshot_json,
                         etapa_actual_index, pistas_liberadas_json)
contextos_trivia        (id, sesion_id, preguntas_ordenadas_json,
                         pregunta_actual_index, timer_cerrado_en)
equipos_sesion          (id, sesion_id, nombre, codigo_acceso,
                         puntaje_total, tiempo_acumulado_ms,
                         bloqueado_para_ronda_actual)
evidencias              (id, sesion_id, equipo_id, etapa_id,
                         codigo_qr, timestamp_servidor, resultado)
respuestas_trivia       (id, sesion_id, equipo_id, pregunta_id,
                         opcion_seleccionada, timestamp_servidor,
                         es_correcta, puntos_obtenidos, estado)
penalizaciones          (id, equipo_sesion_id, puntos, motivo,
                         operador_id, aplicada_en)
eventos_sesion          (id, sesion_id, tipo, payload_json,
                         ocurrido_en, originado_por)
```

---

## 10. Infraestructura de mensajería

RabbitMQ
├── Exchange: umbral.domain.events (type: topic, durable: true)
│
├── Routing keys:
│   ├── sesion.creada
│   ├── sesion.iniciada
│   ├── sesion.pausada
│   ├── sesion.finalizada
│   ├── sesion.etapa-completada          ← BT
│   ├── sesion.evidencia-validada        ← BT
│   ├── sesion.pista-liberada            ← BT
│   ├── sesion.penalizacion-aplicada
│   ├── trivia.pregunta-lanzada          ← Trivia
│   ├── trivia.respuesta-recibida        ← Trivia
│   └── trivia.tiempo-agotado            ← Trivia
│
└── Colas:
├── umbral.auditoria       → binding: #
├── umbral.puntaje         → binding: sesion.evidencia-validada,
│                                     sesion.penalizacion-aplicada,
│                                     trivia.respuesta-recibida
├── umbral.notificaciones  → binding: sesion.pista-liberada,
│                                     sesion.estado-cambiado
├── umbral.trivia          → binding: trivia.*
└── umbral.dlq             → Dead Letter Queue


---

## 11. Docker Compose — servicios

```yaml
services:

  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: umbral
      POSTGRES_USER: umbral_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    ports: ["5432:5432"]
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U umbral_user"]
      interval: 5s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"    # AMQP
      - "15672:15672"  # Management UI
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: .
      dockerfile: docker/backend.Dockerfile
    ports: ["5000:5000"]
    environment:
      ConnectionStrings__Default: "Host=db;Database=umbral;..."
      RabbitMQ__Host: rabbitmq
    depends_on:
      db:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  frontend:
    build:
      context: .
      dockerfile: docker/frontend.Dockerfile
    ports: ["3000:80"]
    depends_on:
      - backend
```

---

## 12. Pipeline CI (.github/workflows/ci.yml)

Trigger: push a main, pull_request a main
Jobs:
backend:
1. actions/checkout
2. actions/setup-dotnet@v4 (version: 8.x)
3. dotnet restore
4. dotnet build --no-restore --configuration Release
5. dotnet test --no-build
--collect:"XPlat Code Coverage"
--results-directory ./coverage
6. Reportgenerator → HTML + Cobertura XML
7. Codecov/codecov-action (umbral de 90%)
8. Fallar si cobertura < 90%
frontend:
1. actions/setup-node@v4 (version: 20.x)
2. npm ci
3. npm run type-check
4. npm run lint
5. npm run test (Vitest --coverage)
6. Fallar si tests fallan
docker-compose-smoke:
needs: [backend, frontend]
1. docker compose up -d
2. Esperar health checks
3. curl /health del backend
4. docker compose down


---

## 13. Decisiones de arquitectura registradas

| ID  | Decisión                                                  | Razón                                                        |
|-----|-----------------------------------------------------------|--------------------------------------------------------------|
| DA-01 | Monolito hexagonal, no microservicios               | Alcance académico. Facilita pruebas, despliegue y coherencia.|
| DA-02 | MassTransit sobre RabbitMQ directo                  | Abstracción sobre el broker, retry y DLQ incluidos.          |
| DA-03 | SignalR para WebSockets                             | Integración nativa con .NET, fallback automático.            |
| DA-04 | CQRS sin Event Sourcing                             | Simplicidad. Queries directo al DbContext sin reconstruir agregados. |
| DA-05 | MisionSnapshot inmutable en Sesion                  | Aislar sesiones activas de cambios futuros en la misión.     |
| DA-06 | ContextoBusquedaTesoro y ContextoTrivia separados   | Una sesión es de un único tipo. Evita campos nulos mezclados.|
| DA-07 | Validación via timestamp servidor, no cliente       | Elimina posibilidad de trampas por manipulación del cliente. |
| DA-08 | Queries acceden directo al DbContext                | Evita overhead de reconstruir agregados para lecturas puras. |
| DA-09 | Domain Events publicados batch al finalizar el handler | Consistencia: solo se publican si el handler completa sin error.|