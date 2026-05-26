# Skill: CQRS + MediatR — Proyecto UMBRAL

## Propósito
Guía completa para implementar el patrón CQRS con MediatR en la capa de
Aplicación del monolito hexagonal de UMBRAL. Cubre Commands, Queries,
Handlers, Behaviors (pipeline), validación con FluentValidation y
despacho de Domain Events.

---

## 1. Flujo de datos CQRS en UMBRAL

```
HTTP Request
     │
     ▼
┌─────────────────────┐
│   Controller        │  ← Capa de Presentación (API)
│   (Thin)            │    solo mapea HTTP → Command/Query
└────────┬────────────┘
         │  mediator.Send(command)
         ▼
┌─────────────────────────────────────────────────────┐
│              MediatR Pipeline (Behaviors)            │
│  LoggingBehavior → ValidationBehavior → Handler      │
└────────┬────────────────────────────────────────────┘
         │
    ┌────┴────┐
    │         │
Commands    Queries
    │         │
    ▼         ▼
┌───────┐  ┌────────────────┐
│  AR   │  │  Read Model    │  ← DbContext directo (Dapper o EF No-Tracking)
│ (Domain│  │  (DTO/Projection)│
└───┬───┘  └────────────────┘
    │
    ▼
Domain Events
    │
    ▼
┌─────────────────┐
│ IPublisher      │  ← MediatR Notification o MassTransit
│ (Notifications) │
└─────────────────┘
```

**Regla cardinal:** Commands mutan estado a través del AR.
Queries leen directamente sin pasar por el AR.

---

## 2. Estructura de carpetas por BC

```
src/
└── Umbral.Application/
    ├── EjecucionSesion/
    │   ├── Commands/
    │   │   ├── CrearSesion/
    │   │   │   ├── CrearSesionCommand.cs
    │   │   │   ├── CrearSesionCommandHandler.cs
    │   │   │   └── CrearSesionCommandValidator.cs
    │   │   ├── IniciarSesion/
    │   │   │   ├── IniciarSesionCommand.cs
    │   │   │   ├── IniciarSesionCommandHandler.cs
    │   │   │   └── IniciarSesionCommandValidator.cs
    │   │   ├── AgregarEquipo/
    │   │   └── FinalizarSesion/
    │   ├── Queries/
    │   │   ├── ObtenerSesionPorId/
    │   │   │   ├── ObtenerSesionPorIdQuery.cs
    │   │   │   ├── ObtenerSesionPorIdQueryHandler.cs
    │   │   │   └── SesionDetalleDto.cs
    │   │   └── ListarSesionesActivas/
    │   │       ├── ListarSesionesActivasQuery.cs
    │   │       ├── ListarSesionesActivasQueryHandler.cs
    │   │       └── SesionResumenDto.cs
    │   └── EventHandlers/
    │       ├── SesionIniciadaEventHandler.cs
    │       └── SesionFinalizadaEventHandler.cs
    ├── CatalogoBusquedaTesoro/
    │   └── (misma estructura)
    ├── CatalogoTrivia/
    │   └── (misma estructura)
    └── Common/
        ├── Behaviors/
        │   ├── LoggingBehavior.cs
        │   ├── ValidationBehavior.cs
        │   └── PerformanceBehavior.cs
        ├── Exceptions/
        │   ├── NotFoundException.cs
        │   └── DomainValidationException.cs
        └── Interfaces/
            └── IUnitOfWork.cs
```

---

## 3. Plantillas canónicas

### 3.1 Command + Result

```csharp
// Commands/CrearSesionBusquedaTesoro/CrearSesionBusquedaTesoroCommand.cs
namespace Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;

/// <summary>
/// Crea una sesión BusquedaTesoro a partir de una Mision activa.
/// Retorna Result<Guid> — Guid = SesionId en caso de éxito.
/// </summary>
public sealed record CrearSesionBusquedaTesoroCommand(
    Guid MisionId,
    Guid OperadorId
) : IRequest<Result<Guid>>;

// Commands/CrearSesionTrivia/CrearSesionTriviaCommand.cs
namespace Umbral.Application.Sesion.Commands.CrearSesionTrivia;

public sealed record CrearSesionTriviaCommand(
    List<Guid> PreguntaIds,
    Guid OperadorId
) : IRequest<Result<Guid>>;
```

> **Nota:** Los Commands retornan `Result<T>` (no records personalizados).
> Usar una librería de Result como `FluentResults` o implementar la clase `Result<T>` propio.

### 3.2 Command Handler

```csharp
// Commands/CrearSesionBusquedaTesoro/CrearSesionBusquedaTesoroCommandHandler.cs
namespace Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;

internal sealed class CrearSesionBusquedaTesoroCommandHandler
    : IRequestHandler<CrearSesionBusquedaTesoroCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IMisionRepository _misionRepository;
    private readonly IEventPublisher   _eventPublisher;   // ← IEventPublisher (no IPublisher de MediatR)

    public CrearSesionBusquedaTesoroCommandHandler(
        ISesionRepository sesionRepository,
        IMisionRepository misionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _misionRepository = misionRepository;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<Guid>> Handle(
        CrearSesionBusquedaTesoroCommand command,
        CancellationToken ct)
    {
        // 1. Obtener la Mision del catálogo
        var mision = await _misionRepository
            .FindByIdAsync(new MisionId(command.MisionId), ct)
            ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        if (!mision.PuedeUsarseParaSesion())
            throw new DomainException("La misión debe estar Activa para crear una sesión.");

        // 2. Crear el snapshot inmutable (ACL) y el AR Sesion
        var snapshot = MisionSnapshot.Desde(mision);
        var sesion   = Sesion.CrearBusquedaTesoro(snapshot, new UsuarioId(command.OperadorId));

        // 3. Persistir
        await _sesionRepository.SaveAsync(sesion, ct);

        // 4. Despachar Domain Events DESPUÉS de persistir (batch)
        await _eventPublisher.PublishBatchAsync(sesion.DomainEvents, ct);
        sesion.ClearDomainEvents();

        return Result.Ok(sesion.SesionId.Valor);
    }
}
```

### 3.3 Command Validator

```csharp
// Commands/CrearSesionBusquedaTesoro/CrearSesionBusquedaTesoroValidator.cs
namespace Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;

public sealed class CrearSesionBusquedaTesoroValidator
    : AbstractValidator<CrearSesionBusquedaTesoroCommand>
{
    public CrearSesionBusquedaTesoroValidator()
    {
        RuleFor(x => x.MisionId)
            .NotEmpty()
            .WithMessage("El identificador de la misión es obligatorio.");

        RuleFor(x => x.OperadorId)
            .NotEmpty()
            .WithMessage("El identificador del operador es obligatorio.");
    }
}
```

### 3.4 Query + DTO

```csharp
// Queries/ObtenerSesionPorId/ObtenerSesionPorIdQuery.cs
namespace Umbral.Application.Sesion.Queries.ObtenerSesionPorId;

public sealed record ObtenerSesionPorIdQuery(Guid SesionId)
    : IRequest<SesionDetalleDto>;

// Queries/ObtenerSesionPorId/SesionDetalleDto.cs
public sealed record SesionDetalleDto(
    Guid SesionId,
    string TipoSesion,           // "BusquedaTesoro" | "Trivia"
    string Estado,               // "Programada" | "EnPreparacion" | "Activa" | "Pausada" | "Finalizada" | "Cancelada"
    DateTime? IniciadaEn,
    DateTime? FinalizadaEn,
    int TotalEquipos,
    IReadOnlyList<EquipoResumenDto> Equipos
);

public sealed record EquipoResumenDto(
    Guid EquipoId,
    string Nombre,
    int Puntaje,
    int Posicion
);
```

### 3.5 Query Handler

```csharp
// Queries/ObtenerSesionPorId/ObtenerSesionPorIdQueryHandler.cs
namespace Umbral.Application.Sesion.Queries.ObtenerSesionPorId;

internal sealed class ObtenerSesionPorIdQueryHandler
    : IRequestHandler<ObtenerSesionPorIdQuery, SesionDetalleDto>
{
    // Queries leen del DbContext directamente (sin pasar por el AR)
    private readonly IUmbralDbContext _db;

    public ObtenerSesionPorIdQueryHandler(IUmbralDbContext db) => _db = db;

    public async Task<SesionDetalleDto> Handle(
        ObtenerSesionPorIdQuery query,
        CancellationToken ct)
    {
        var sesion = await _db.Sesiones
            .AsNoTracking()
            .Include(s => s.Equipos)
            .FirstOrDefaultAsync(s => s.SesionId == new SesionId(query.SesionId), ct)
            ?? throw new NotFoundException(nameof(Sesion), query.SesionId);

        // Calcular posición para cada equipo
        var equiposOrdenados = sesion.Equipos
            .OrderByDescending(e => e.Puntaje.Valor)
            .Select((e, idx) => new EquipoResumenDto(
                e.EquipoId.Valor,
                e.Nombre.Valor,
                e.Puntaje.Valor,
                idx + 1))
            .ToList();

        return new SesionDetalleDto(
            sesion.SesionId.Valor,
            sesion.TipoSesion.ToString(),
            sesion.Estado.ToString(),
            sesion.IniciadaEn == default ? null : sesion.IniciadaEn,
            sesion.FinalizadaEn,
            sesion.Equipos.Count,
            equiposOrdenados);
    }
}
```

### 3.6 Domain Event Handler (Notification)

```csharp
// EventHandlers/SesionIniciadaEventHandler.cs
namespace Umbral.Application.Sesion.EventHandlers;

/// <summary>
/// Reacciona al Domain Event SesionIniciada.
/// Notifica via SignalR y publica Integration Event en RabbitMQ.
/// </summary>
internal sealed class SesionIniciadaEventHandler
    : INotificationHandler<SesionIniciada>
{
    private readonly INotificacionRealTime _notifier;   // ← port SignalR
    private readonly IEventPublisher _eventPublisher;   // ← port RabbitMQ
    private readonly ILogger<SesionIniciadaEventHandler> _logger;

    public SesionIniciadaEventHandler(
        INotificacionRealTime notifier,
        IEventPublisher eventPublisher,
        ILogger<SesionIniciadaEventHandler> logger)
    {
        _notifier       = notifier;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task Handle(SesionIniciada notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Sesión {SesionId} iniciada (tipo={Tipo}).",
            notification.SesionId, notification.TipoSesion);

        // A. Notificar via SignalR
        await _notifier.NotificarEstadoSesionAsync(
            notification.SesionId.Valor, "Activa", ct);

        // B. Publicar Integration Event en RabbitMQ
        await _eventPublisher.PublicarAsync(
            new SesionIniciadaIntegrationEvent
            {
                SesionId   = notification.SesionId.Valor,
                TipoSesion = notification.TipoSesion.ToString()
            }, ct);
    }
}
```

---

## 4. Pipeline de Behaviors

### 4.1 Logging Behavior

```csharp
// Common/Behaviors/LoggingBehavior.cs
namespace Umbral.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("→ Iniciando {Request}", requestName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();
            _logger.LogInformation(
                "← {Request} completado en {Ms}ms", requestName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "✗ {Request} falló en {Ms}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### 4.2 Validation Behavior

```csharp
// Common/Behaviors/ValidationBehavior.cs
namespace Umbral.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new DomainValidationException(failures);

        return await next();
    }
}
```

### 4.3 Performance Behavior

```csharp
// Common/Behaviors/PerformanceBehavior.cs
namespace Umbral.Application.Common.Behaviors;

/// <summary>
/// Alerta si un handler supera el umbral de tiempo aceptable.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int UmbralMs = 500;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > UmbralMs)
            _logger.LogWarning(
                "⚠ Handler lento: {Request} tardó {Ms}ms (umbral={Umbral}ms)",
                typeof(TRequest).Name, sw.ElapsedMilliseconds, UmbralMs);

        return response;
    }
}
```

---

## 5. Registro en DI (Program.cs / ServiceCollectionExtensions)

```csharp
// Infrastructure/DependencyInjection/ApplicationServiceExtensions.cs
namespace Umbral.Infrastructure.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        var applicationAssembly = typeof(CrearSesionCommand).Assembly;

        // MediatR — registra Handlers, Validators y Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);

            // Orden del pipeline: Logging → Validation → Handler
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        });

        // FluentValidation — registra todos los validators del assembly
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
```

---

## 6. Controller delgado (API → MediatR)

```csharp
// API/Controllers/SesionesController.cs
namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]   // ← con versión según project-rules.md
[Authorize]
public sealed class SesionesController : ControllerBase
{
    private readonly IMediator _mediator;   // ← IMediator (no ISender) según project-rules.md

    public SesionesController(IMediator mediator) => _mediator = mediator;

    // ── Commands ───────────────────────────────────────────────

    [HttpPost("busqueda-tesoro")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearBusquedaTesoro(
        [FromBody] CrearSesionBusquedaTesoroRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearSesionBusquedaTesoroCommand(request.MisionId, request.OperadorId), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPost("trivia")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearTrivia(
        [FromBody] CrearSesionTriviaRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearSesionTriviaCommand(request.PreguntaIds, request.OperadorId), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPost("{id:guid}/iniciar")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Iniciar(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new IniciarSesionCommand(id), ct);
        return NoContent();
    }

    // ── Queries ────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SesionDetalleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var dto = await _mediator.Send(new ObtenerSesionPorIdQuery(id), ct);
        return Ok(dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SesionResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var dtos = await _mediator.Send(new ListarSesionesQuery(), ct);
        return Ok(dtos);
    }
}
```

---

## 7. Manejo de excepciones global

```csharp
// Infrastructure/Middleware/GlobalExceptionMiddleware.cs
namespace Umbral.Infrastructure.Middleware;

public sealed class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DomainValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Validación fallida.",
                detalles = ex.Errors.Select(f => new { f.PropertyName, f.ErrorMessage })
            });
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor." });
        }
    }
}
```

---

## 8. Reglas CQRS — UMBRAL

| # | Regla | Motivo |
|---|-------|--------|
| 1 | **Commands** retornan `Result<T>` o `Result` (con librería de Result). | Permite manejar errores sin excepciones en la capa de presentación. |
| 2 | **Queries** usan `AsNoTracking()` siempre. | Rendimiento; no hay mutación. |
| 3 | Handlers son `internal sealed`. | Encapsulación; solo MediatR los invoca. |
| 4 | Un Command → un Handler. No hay lógica compartida entre handlers. | Cohesión y testabilidad. |
| 5 | Domain Events se despachan **después** de persistir, via `IEventPublisher.PublishBatchAsync`. | Evita eventos sin persistencia; `IEventPublisher` es el port del proyecto. |
| 6 | Validators viven en el mismo folder que el Command/Query. | Colocalización; fácil de encontrar. |
| 7 | Controllers usan `IMediator` (no `ISender`). Rutas con prefijo `/api/v1/`. | Consistencia con `project-rules.md`. |
| 8 | Queries proyectan **DTOs, no entidades**. | Evita lazy loading y exposición del modelo de dominio. |
| 9 | `INotificacionRealTime` para SignalR y `IEventPublisher` para RabbitMQ en los handlers. | Puertos definidos en Domain; Application no conoce la infraestructura. |
| 10 | BC `Sesion` y BC `CatalogoBusquedaTesoro` tienen sus propias carpetas en `Application`. | Respeta los límites del contexto. |

---

## 9. Checklist al crear un Command/Query

```
□ Command/Query es un record inmutable (record sealed)?
□ El handler es internal sealed?
□ ¿Existe el Validator correspondiente con FluentValidation?
□ ¿El handler persiste primero y luego despacha Domain Events?
□ ¿Se llama ClearDomainEvents() después del dispatch?
□ ¿Las queries usan AsNoTracking()?
□ ¿El DTO de respuesta no expone entidades de dominio directas?
□ ¿El controller solo mapea y delega a ISender?
□ ¿Los errores de dominio se convierten en HTTP apropiados via middleware?
□ ¿El handler está registrado en el assembly correcto para MediatR?
```

---

## 10. Anti-patrones a evitar

```csharp
// ❌ MALO — lógica de negocio en el controller
[HttpPost("{id}/iniciar")]
public async Task<IActionResult> Iniciar(Guid id)
{
    var sesion = await _repo.ObtenerPorIdAsync(new SesionId(id));
    if (sesion.Equipos.Count == 0) return BadRequest("Sin equipos");
    sesion.Estado = EstadoSesion.Activa;    // setter directo
    await _repo.ActualizarAsync(sesion);
    return NoContent();
}

// ✅ BUENO — controller thin, lógica en handler/dominio
[HttpPost("{id:guid}/iniciar")]
public async Task<IActionResult> Iniciar(Guid id, CancellationToken ct)
{
    await _sender.Send(new IniciarSesionCommand(id), ct);
    return NoContent();
}

// ❌ MALO — query retorna entidad de dominio
public async Task<Sesion> Handle(ObtenerSesionPorIdQuery q, CancellationToken ct)
    => await _db.Sesiones.FindAsync(new SesionId(q.SesionId));

// ✅ BUENO — query proyecta DTO directamente
public async Task<SesionDetalleDto> Handle(ObtenerSesionPorIdQuery q, CancellationToken ct)
    => await _db.Sesiones.AsNoTracking()
           .Where(s => s.Id == new SesionId(q.SesionId))
           .Select(s => new SesionDetalleDto(s.Id.Value, s.Nombre, ...))
           .FirstOrDefaultAsync(ct)
       ?? throw new NotFoundException(nameof(Sesion), q.SesionId);

// ❌ MALO — Domain Events antes de persistir
sesion.Iniciar();
await _publisher.Publish(new SesionIniciadaEvent(sesion.Id));  // ← sin persistir aún
await _uow.GuardarAsync(ct);

// ✅ BUENO — Domain Events después de persistir
sesion.Iniciar();
await _uow.GuardarAsync(ct);                                   // ← primero persistir
foreach (var e in sesion.DomainEvents) await _publisher.Publish(e, ct);
sesion.ClearDomainEvents();
```

---

## 11. Guía de uso para Cursor AI

Cuando el usuario pida crear un Command, Query o Handler en UMBRAL:

1. **Identificar tipo** → ¿Command (muta estado, retorna `Result<T>`) o Query (solo lee, retorna DTO)?
2. **Identificar BC** → ¿Sesion, CatalogoBusquedaTesoro, CatalogoTrivia?
3. **Para Sesion**: hay dos factory methods → `CrearBusquedaTesoro` y `CrearTrivia`. Nunca `Sesion.Crear` genérico.
4. **Crear los 3 archivos del Command** → `*Command.cs`, `*CommandHandler.cs`, `*CommandValidator.cs`
   — o los 3 del Query → `*Query.cs`, `*QueryHandler.cs`, `*Dto.cs`
5. **Verificar** que el handler sigue: obtener AR → llamar comportamiento → persistir → `IEventPublisher.PublishBatchAsync` → `ClearDomainEvents`.
6. **Verificar** que la query usa `AsNoTracking()`, proyecta DTO y usa `SesionId` (no `Id`).
7. **Agregar el endpoint** en el controller con ruta `/api/v1/` y atributo `[Authorize(Roles = "...")]`.
8. **Correr checklist** sección 9.
9. **Advertir** si el código cae en anti-patrones de sección 10.