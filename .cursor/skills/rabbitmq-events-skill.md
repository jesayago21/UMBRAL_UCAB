# Skill: RabbitMQ + MassTransit — Proyecto UMBRAL

## Propósito
Guía canónica para implementar mensajería asíncrona con RabbitMQ y MassTransit
en UMBRAL. Cubre: Integration Events, Consumers, Sagas (si aplica), configuración
de exchanges/queues, retry/outbox y el patrón de separación entre Domain Events
e Integration Events.

---

## 1. Arquitectura de mensajería

```
Domain Event (in-process, MediatR)
        │
        │  INotificationHandler<T>
        ▼
Application EventHandler
        │
        │  IEventBus.PublicarAsync(IntegrationEvent)
        ▼
┌───────────────────────────────┐
│   MassTransit Producer        │
│   (IPublishEndpoint)          │
└──────────────┬────────────────┘
               │
        RabbitMQ Exchange
               │
    ┌──────────┴──────────┐
    │                     │
    ▼                     ▼
Queue A               Queue B
(Notificaciones)  (Estadísticas)
    │                     │
Consumer A           Consumer B
(IConsumer<T>)    (IConsumer<T>)
```

**Separación clave:**

| Tipo | Scope | Transporte |
|------|-------|------------|
| Domain Event | In-process | MediatR (IPublisher) |
| Integration Event | Cross-service / async | RabbitMQ via MassTransit |

---

## 2. Estructura de carpetas

```
src/Umbral.Application/
└── Common/
    └── Interfaces/
        └── IEventPublisher.cs            ← puerto de salida (Ports en Domain)

src/Umbral.Domain/
└── Ports/
    └── IEventPublisher.cs               ← definido en Domain (puerto driven)

src/Umbral.Infrastructure/
└── Messaging/
    ├── Publishers/
    │   └── MassTransitEventPublisher.cs  ← implementación de IEventPublisher
    ├── Consumers/
    │   ├── SesionIniciadaConsumer.cs
    │   ├── SesionFinalizadaConsumer.cs
    │   └── EtapaCompletadaConsumer.cs
    └── DependencyInjection/
        └── MessagingServiceExtensions.cs
```

> **Nota:** Los Integration Events (contratos) se definen como records dentro de
> `Umbral.Infrastructure/Messaging/` o directamente en `Umbral.Application/Common/`.
> No existe un 5to proyecto `Umbral.Contracts` — la solución tiene **exactamente cuatro proyectos**.
> Si se requiere compartir contratos con sistemas externos en el futuro, se crea entonces.

---

## 3. Integration Events — Contratos

```csharp
// Umbral.Contracts/IntegrationEvents/EjecucionSesion/SesionIniciadaIntegrationEvent.cs
namespace Umbral.Contracts.IntegrationEvents.EjecucionSesion;

/// <summary>
/// Evento de integración publicado cuando una sesión es iniciada.
/// Este contrato es compartido entre productores y consumidores.
/// NUNCA modificar campos existentes — solo agregar nuevos (backward compatibility).
/// </summary>
public sealed record SesionIniciadaIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OcurridoEn { get; init; } = DateTimeOffset.UtcNow;
    public int Version { get; init; } = 1;

    // Datos del evento
    public Guid SesionId { get; init; }
    public string NombreSesion { get; init; } = string.Empty;
    public string TipoSesion { get; init; } = string.Empty;    // "BusquedaTesoro" | "Trivia"
    public int TotalEquipos { get; init; }
}

// Umbral.Contracts/IntegrationEvents/EjecucionSesion/SesionFinalizadaIntegrationEvent.cs
public sealed record SesionFinalizadaIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OcurridoEn { get; init; } = DateTimeOffset.UtcNow;
    public int Version { get; init; } = 1;

    public Guid SesionId { get; init; }
    public DateTimeOffset FechaFin { get; init; }
    public IReadOnlyList<PuntajeFinalDto> Puntajes { get; init; } = [];
}

public sealed record PuntajeFinalDto(Guid EquipoId, string NombreEquipo, int Puntaje);

// Umbral.Contracts/IntegrationEvents/EjecucionSesion/EtapaCompletadaIntegrationEvent.cs
public sealed record EtapaCompletadaIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OcurridoEn { get; init; } = DateTimeOffset.UtcNow;
    public int Version { get; init; } = 1;

    public Guid SesionId { get; init; }
    public Guid EtapaId { get; init; }
    public int OrdenEtapa { get; init; }
    public Guid EquipoId { get; init; }
}
```

---

## 4. Puerto de salida — IEventPublisher

```csharp
// Umbral.Domain/Ports/IEventPublisher.cs
namespace Umbral.Domain.Ports;

/// <summary>
/// Puerto de salida para publicar Integration Events hacia RabbitMQ.
/// Definido en Domain para que Application pueda referenciarlo sin
/// depender de Infrastructure.
/// </summary>
public interface IEventPublisher
{
    Task PublicarAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class;
}
```

---

## 5. Implementación MassTransit

```csharp
// Umbral.Infrastructure/Messaging/Publishers/MassTransitEventPublisher.cs
namespace Umbral.Infrastructure.Messaging.Publishers;

internal sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        => _publishEndpoint = publishEndpoint;

    public Task PublicarAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class
        => _publishEndpoint.Publish(integrationEvent, ct);
}
```

---

## 6. Consumers

### 6.1 Consumer base

```csharp
// Umbral.Infrastructure/Messaging/Consumers/EjecucionSesion/SesionIniciadaConsumer.cs
namespace Umbral.Infrastructure.Messaging.Consumers.EjecucionSesion;

/// <summary>
/// Procesa el evento de integración SesionIniciadaIntegrationEvent.
/// Ejemplo: registrar en auditoría, enviar notificación externa, etc.
/// </summary>
public sealed class SesionIniciadaConsumer
    : IConsumer<SesionIniciadaIntegrationEvent>
{
    private readonly ILogger<SesionIniciadaConsumer> _logger;

    public SesionIniciadaConsumer(ILogger<SesionIniciadaConsumer> logger)
        => _logger = logger;

    public async Task Consume(ConsumeContext<SesionIniciadaIntegrationEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "Procesando SesionIniciada: SesionId={SesionId} Tipo={Tipo} Equipos={Equipos}",
            evento.SesionId, evento.TipoSesion, evento.TotalEquipos);

        // Lógica de procesamiento...
        await Task.CompletedTask;
    }
}
```

### 6.2 Consumer con manejo de errores

```csharp
// Umbral.Infrastructure/Messaging/Consumers/EjecucionSesion/SesionFinalizadaConsumer.cs
public sealed class SesionFinalizadaConsumer
    : IConsumer<SesionFinalizadaIntegrationEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<SesionFinalizadaConsumer> _logger;

    public SesionFinalizadaConsumer(ISender sender, ILogger<SesionFinalizadaConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SesionFinalizadaIntegrationEvent> context)
    {
        var evento = context.Message;

        try
        {
            _logger.LogInformation(
                "Sesión {SesionId} finalizada. Procesando resultados.", evento.SesionId);

            // Puede disparar un Command interno
            await _sender.Send(new GenerarReporteFinalizacionCommand(
                evento.SesionId,
                evento.Puntajes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al procesar SesionFinalizada para {SesionId}", evento.SesionId);
            throw;  // MassTransit reintentará según la política configurada
        }
    }
}
```

---

## 7. Configuración de MassTransit (DI)

```csharp
// Umbral.Infrastructure/Messaging/DependencyInjection/MessagingServiceExtensions.cs
namespace Umbral.Infrastructure.Messaging.DependencyInjection;

public static class MessagingServiceExtensions
{
    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // ── Registrar todos los Consumers del assembly ────────
            x.AddConsumers(typeof(SesionIniciadaConsumer).Assembly);

            // ── Configurar transporte RabbitMQ ─────────────────────
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                // ── Exchange topic único (umbral.domain.events) ───
                // Routing keys: sesion.creada, sesion.iniciada, etc.
                cfg.UseMessageRetry(r =>
                    r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

                // ── Colas especializadas por propósito ────────────
                cfg.ReceiveEndpoint("umbral.auditoria", e =>
                {
                    // Consume todos los eventos de dominio
                    e.ConfigureConsumer<SesionIniciadaConsumer>(ctx);
                    e.ConfigureConsumer<SesionFinalizadaConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("umbral.puntaje", e =>
                {
                    e.ConfigureConsumer<EtapaCompletadaConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("umbral.dlq", e =>
                {
                    // Dead Letter Queue — mensajes con 3 fallos
                });
            });
        });

        // Registrar IEventPublisher (puerto de salida)
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }
}
```

---

## 8. Configuración de appsettings y Docker

```json
// appsettings.json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "umbral_user",
    "Password": "umbral_pass",
    "VirtualHost": "/"
  }
}

// appsettings.Development.json
{
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "umbral_user",
    "Password": "umbral_pass"
  }
}
```

```yaml
# docker-compose.yml (fragmento)
services:
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
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

volumes:
  rabbitmq_data:
```

---

## 9. Flujo completo: Domain Event → Integration Event

```csharp
// PASO 1: Domain Event Handler (Application Layer)
// dispara Domain Event → publica Integration Event
namespace Umbral.Application.Sesion.EventHandlers;

internal sealed class SesionIniciadaEventHandler
    : INotificationHandler<SesionIniciada>
{
    private readonly IEventPublisher _eventPublisher;   // ← IEventPublisher (no IEventBus)
    private readonly INotificacionRealTime _notifier;   // ← nombre del port en specs

    public SesionIniciadaEventHandler(
        IEventPublisher eventPublisher,
        INotificacionRealTime notifier)
    {
        _eventPublisher = eventPublisher;
        _notifier = notifier;
    }

    public async Task Handle(SesionIniciada notification, CancellationToken ct)
    {
        // A. Notificar en tiempo real via SignalR
        await _notifier.NotificarSesionIniciadaAsync(notification.SesionId, ct);

        // B. Publicar Integration Event en RabbitMQ
        await _eventPublisher.PublicarAsync(
            new SesionIniciadaIntegrationEvent
            {
                SesionId = notification.SesionId.Valor,
                TipoSesion = notification.TipoSesion.ToString()
            }, ct);
    }
}
```

---

## 10. Outbox Pattern (opcional — mayor durabilidad)

```csharp
// Si se requiere garantía at-least-once con transaccionalidad:
// MassTransit Entity Framework Outbox

// Umbral.Infrastructure.csproj
// <PackageReference Include="MassTransit.EntityFrameworkCore" Version="8.*" />

// En MessagingServiceExtensions:
x.AddEntityFrameworkOutbox<UmbralDbContext>(o =>
{
    o.UsePostgres();
    o.UseBusOutbox();
});

// En UmbralDbContext.OnModelCreating:
modelBuilder.AddInboxStateEntity();
modelBuilder.AddOutboxMessageEntity();
modelBuilder.AddOutboxStateEntity();
```

---

## 11. Naming conventions — Queues y Exchanges

| Evento | Exchange (fanout) | Queue |
|--------|-------------------|-------|
| `SesionIniciadaIntegrationEvent` | `umbral.sesion-iniciada` | `umbral.sesion.iniciada` |
| `SesionFinalizadaIntegrationEvent` | `umbral.sesion-finalizada` | `umbral.sesion.finalizada` |
| `EtapaCompletadaIntegrationEvent` | `umbral.etapa-completada` | `umbral.etapa.completada` |
| `PistaBusquedaActualizadaIntegrationEvent` | `umbral.pista-actualizada` | `umbral.pista.actualizada` |

**Convención:** `umbral.<entidad>-<accion>` (exchange), `umbral.<entidad>.<accion>` (queue).

---

## 12. Reglas RabbitMQ/MassTransit — UMBRAL

| # | Regla | Motivo |
|---|-------|--------|
| 1 | **Domain Events** viajan por MediatR (in-process). **Integration Events** viajan por RabbitMQ. | Separa responsabilidades: consistencia interna vs. integración externa. |
| 2 | Los Integration Events son **records inmutables** con `Version` explícita. | Facilita evolución del contrato (backward compatibility). |
| 3 | Los contratos de eventos viven en **`Umbral.Contracts`** (proyecto separado). | Compartible con futuros servicios sin acoplar la implementación. |
| 4 | Los consumers **no deben tener lógica de negocio** compleja; delegan a `ISender`. | Testabilidad y separación de capas. |
| 5 | Configurar **retry incremental** (3 intentos) en todos los endpoints. | Resiliencia ante fallos transitorios. |
| 6 | Usar **Dead Letter Queue** para mensajes que fallan definitivamente. | Observabilidad y recuperación manual. |
| 7 | Los Integration Events se publican **después** de persistir el Domain Event. | Consistencia: no publicar si la transacción falló. |
| 8 | Usar **Outbox Pattern** si se requiere garantía at-least-once. | Evita pérdida de eventos ante caída del broker. |
| 9 | Acceder al Management UI (`localhost:15672`) para monitorear colas en desarrollo. | Visibilidad del estado de la mensajería. |
| 10 | **Nunca modificar** campos existentes de un Integration Event; solo agregar nuevos. | Backward compatibility con consumers existentes. |

---

## 13. Checklist al agregar un nuevo Integration Event

```
□ ¿El record está en Umbral.Contracts/IntegrationEvents/<BC>/?
□ ¿Tiene EventId, OcurridoEn y Version?
□ ¿El Domain Event Handler publica el Integration Event via IEventBus?
□ ¿Existe un Consumer (IConsumer<T>) para el evento?
□ ¿El Consumer está registrado en MessagingServiceExtensions?
□ ¿El endpoint (queue) tiene nombre consistente con la convención?
□ ¿Se configuró la política de retry?
□ ¿Los campos del contrato son solo tipos primitivos o listas de records simples?
□ ¿No se modificaron campos existentes (solo se agregaron nuevos)?
□ ¿El Consumer delega lógica compleja a ISender (MediatR)?
```

---

## 14. Anti-patrones a evitar

```csharp
// ❌ MALO — publicar Integration Event ANTES de persistir
sesion.Iniciar();
await _eventBus.PublicarAsync(new SesionIniciadaIntegrationEvent { SesionId = sesion.Id.Value });
await _uow.GuardarAsync(ct);  // ← si esto falla, el evento ya fue publicado

// ✅ BUENO — publicar DESPUÉS de persistir (en el Domain Event Handler)
sesion.Iniciar();
await _uow.GuardarAsync(ct);  // ← primero persistir
// El Domain Event SesionIniciadaEvent dispara el handler que publica en RabbitMQ

// ❌ MALO — lógica de negocio compleja en el Consumer
public async Task Consume(ConsumeContext<SesionIniciadaIntegrationEvent> ctx)
{
    var sesion = await _repo.ObtenerPorIdAsync(ctx.Message.SesionId);
    sesion.CalcularPuntajes();   // ← regla de negocio
    await _repo.ActualizarAsync(sesion);
}

// ✅ BUENO — Consumer delega a Command Handler
public async Task Consume(ConsumeContext<SesionIniciadaIntegrationEvent> ctx)
{
    await _sender.Send(new RegistrarInicioSesionCommand(ctx.Message.SesionId));
}

// ❌ MALO — Integration Event con referencia a entidad de dominio
public sealed record SesionIniciadaIntegrationEvent
{
    public Sesion Sesion { get; init; }  // ← acoplamiento de dominio
}

// ✅ BUENO — Integration Event con solo primitivos
public sealed record SesionIniciadaIntegrationEvent
{
    public Guid SesionId { get; init; }
    public string NombreSesion { get; init; } = string.Empty;
    public string TipoSesion { get; init; } = string.Empty;
}

// ❌ MALO — modificar contrato existente (rompe consumers)
public sealed record SesionIniciadaIntegrationEvent
{
    public Guid Id { get; init; }  // ← renombrado de SesionId: rompe compatibilidad
}

// ✅ BUENO — solo agregar campos nuevos
public sealed record SesionIniciadaIntegrationEvent
{
    public Guid SesionId { get; init; }   // ← campo original intacto
    public string Ubicacion { get; init; } = string.Empty;  // ← campo nuevo
    public int Version { get; init; } = 2; // ← versión incrementada
}
```

---

## 15. Guía de uso para Cursor AI

Cuando el usuario pida agregar mensajería asíncrona en UMBRAL:

1. **Crear el Integration Event** en `Umbral.Contracts/IntegrationEvents/<BC>/` con `EventId`, `OcurridoEn` y `Version`.
2. **Agregar el método** en `IEventBus` si se necesita nueva firma (normalmente es genérico).
3. **Llamar `IEventBus.PublicarAsync`** desde el Domain Event Handler (Application), después de que el Command Handler ya persistió.
4. **Crear el Consumer** `IConsumer<T>` en `Infrastructure/Messaging/Consumers/<BC>/`.
5. **Registrar el endpoint** en `MessagingServiceExtensions` con nombre de queue consistente.
6. **Configurar retry** en el endpoint.
7. **Correr checklist** de la sección 13 y advertir si hay anti-patrones de la sección 14.
