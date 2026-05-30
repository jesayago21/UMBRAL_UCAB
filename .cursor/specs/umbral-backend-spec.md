# UMBRAL — Especificación Técnica Backend

> **Trazabilidad:** `docs/TRAZABILIDAD.md` · dominio implementado según HU ERS en `docs/fase-1/TRACKER.md`.

## 1. Stack y versiones

| Tecnología              | Versión   | Uso                                      |
|-------------------------|-----------|------------------------------------------|
| .NET                    | 8.0       | Runtime y SDK                            |
| C#                      | 12        | Lenguaje principal                       |
| ASP.NET Core            | 8.0       | Web API y SignalR                        |
| Entity Framework Core   | 8.x       | ORM y migraciones                        |
| Npgsql EF Provider      | 8.x       | Driver PostgreSQL                        |
| MediatR                 | 12.x      | Implementación CQRS                      |
| FluentValidation        | 11.x      | Validación de Commands                   |
| MassTransit             | 8.x       | Abstracción RabbitMQ                     |
| SignalR                 | 8.0       | WebSockets                               |
| Serilog                 | 3.x       | Logging estructurado                     |
| xUnit                   | 2.x       | Framework de pruebas                     |
| NSubstitute             | 5.x       | Mocking en pruebas (Application)         |
| FluentAssertions        | 6.x       | Aserciones legibles                      |
| Testcontainers          | 4.x       | PostgreSQL en pruebas de integración     |
| BCrypt.Net              | 4.x       | Hash de contraseñas                      |

---

## 2. Domain Layer — implementación

### 2.1 Clase base AggregateRoot

```csharp
// Umbral.Domain/Shared/AggregateRoot.cs
namespace Umbral.Domain.Shared;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### 2.2 Clase base Entity

```csharp
// Umbral.Domain/Shared/Entity.cs
namespace Umbral.Domain.Shared;

public abstract class Entity
{
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && IdEquals(other);
    }

    protected abstract bool IdEquals(Entity other);
    public override int GetHashCode() => GetIdHashCode();
    protected abstract int GetIdHashCode();
}
```

### 2.3 Clase base ValueObject

```csharp
// Umbral.Domain/Shared/ValueObject.cs
namespace Umbral.Domain.Shared;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
```

### 2.4 Agregado Sesion

```csharp
// Umbral.Domain/Sesion/Sesion.cs
namespace Umbral.Domain.Sesion;

public sealed class Sesion : AggregateRoot
{
    public SesionId SesionId { get; private set; }
    public TipoSesion TipoSesion { get; private set; }
    public UsuarioId OperadorId { get; private set; }
    public EstadoSesion Estado { get; private set; }
    public DateTime IniciadaEn { get; private set; }
    public DateTime? FinalizadaEn { get; private set; }

    private readonly List<EquipoSesion> _equipos = [];
    private readonly List<EventoSesion> _historialEventos = [];
    private readonly List<Evidencia> _evidencias = [];
    private readonly List<RespuestaTrivia> _respuestas = [];

    public IReadOnlyList<EquipoSesion> Equipos => _equipos.AsReadOnly();
    public IReadOnlyList<EventoSesion> HistorialEventos
        => _historialEventos.AsReadOnly();
    public IReadOnlyList<Evidencia> Evidencias => _evidencias.AsReadOnly();
    public IReadOnlyList<RespuestaTrivia> Respuestas => _respuestas.AsReadOnly();

    public ContextoBusquedaTesoro? ContextoBT { get; private set; }
    public ContextoTrivia? ContextoTrivia { get; private set; }

    private Sesion() { } // EF Core

    public static Sesion CrearBusquedaTesoro(
        MisionSnapshot snapshot,
        UsuarioId operadorId)
    {
        var sesion = new Sesion
        {
            SesionId   = SesionId.Nuevo(),
            TipoSesion = TipoSesion.BusquedaTesoro,
            OperadorId = operadorId,
            Estado     = EstadoSesion.Programada,
            ContextoBT = ContextoBusquedaTesoro.Crear(snapshot)
        };
        sesion.RaiseDomainEvent(new SesionCreada(
            sesion.SesionId, TipoSesion.BusquedaTesoro, operadorId));
        return sesion;
    }

    public static Sesion CrearTrivia(
        List<PreguntaId> preguntasOrdenadas,
        UsuarioId operadorId)
    {
        if (preguntasOrdenadas.Count == 0)
            throw new DomainException(
                "Una sesión de trivia necesita al menos una pregunta.");

        var sesion = new Sesion
        {
            SesionId      = SesionId.Nuevo(),
            TipoSesion    = TipoSesion.Trivia,
            OperadorId    = operadorId,
            Estado        = EstadoSesion.Programada,
            ContextoTrivia = ContextoTrivia.Crear(preguntasOrdenadas)
        };
        sesion.RaiseDomainEvent(new SesionCreada(
            sesion.SesionId, TipoSesion.Trivia, operadorId));
        return sesion;
    }

    public void Iniciar()
    {
        if (Estado != EstadoSesion.EnPreparacion)
            throw new DomainException(
                $"No se puede iniciar una sesión en estado {Estado}.");
        if (!_equipos.Any())
            throw new DomainException(
                "La sesión necesita al menos un equipo registrado.");

        Estado     = EstadoSesion.Activa;
        IniciadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new SesionIniciada(SesionId, TipoSesion));
        RegistrarEvento("SesionIniciada", $"Sesión iniciada por {OperadorId.Valor}");
    }

    public void Pausar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                $"No se puede pausar una sesión en estado {Estado}.");

        Estado = EstadoSesion.Pausada;
        RaiseDomainEvent(new SesionPausada(SesionId, DateTime.UtcNow));
        RegistrarEvento("SesionPausada", string.Empty);
    }

    public void Reanudar()
    {
        if (Estado != EstadoSesion.Pausada)
            throw new DomainException(
                $"No se puede reanudar una sesión en estado {Estado}.");

        Estado = EstadoSesion.Activa;
        RegistrarEvento("SesionReanudada", string.Empty);
    }

    public void Finalizar()
    {
        if (Estado is not (EstadoSesion.Activa or EstadoSesion.Pausada))
            throw new DomainException(
                $"No se puede finalizar una sesión en estado {Estado}.");

        Estado        = EstadoSesion.Finalizada;
        FinalizadaEn  = DateTime.UtcNow;
        RaiseDomainEvent(new SesionFinalizada(SesionId, DateTime.UtcNow));
        RegistrarEvento("SesionFinalizada", string.Empty);
    }

    public void Cancelar(string motivo)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                $"No se puede cancelar una sesión en estado {Estado}.");

        Estado       = EstadoSesion.Cancelada;
        FinalizadaEn = DateTime.UtcNow;
        RegistrarEvento("SesionCancelada", motivo);
    }

    public EquipoSesion RegistrarEquipo(string nombre)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                "No se pueden registrar equipos en una sesión cerrada.");

        if (_equipos.Any(e => e.Nombre.Valor == nombre))
            throw new DomainException(
                $"Ya existe un equipo con el nombre '{nombre}' en esta sesión.");

        var equipo = EquipoSesion.Crear(SesionId, nombre);
        _equipos.Add(equipo);
        RegistrarEvento("EquipoRegistrado", nombre);
        return equipo;
    }

    public void AplicarPenalizacion(EquipoId equipoId, Penalizacion penalizacion)
    {
        if (!EstaEnEstadoActivo())
            throw new DomainException("Solo se pueden aplicar penalizaciones en sesiones activas.");

        var equipo = ObtenerEquipo(equipoId);
        equipo.AplicarPenalizacion(penalizacion);
        RaiseDomainEvent(new PenalizacionAplicada(
            SesionId, equipoId,
            penalizacion.Puntos, penalizacion.Motivo,
            penalizacion.OperadorId, DateTime.UtcNow));
    }

    public bool EstaEnEstadoActivo() => Estado == EstadoSesion.Activa;

    private EquipoSesion ObtenerEquipo(EquipoId equipoId)
        => _equipos.FirstOrDefault(e => e.EquipoId == equipoId)
           ?? throw new DomainException(
               $"El equipo {equipoId.Valor} no pertenece a esta sesión.");

    private void RegistrarEvento(string tipo, string payload)
        => _historialEventos.Add(EventoSesion.Crear(SesionId, tipo, payload));

    protected override bool IdEquals(Entity other)
        => other is Sesion s && s.SesionId == SesionId;

    protected override int GetIdHashCode()
        => SesionId.GetHashCode();
}
```

### 2.5 Value Object Puntaje

```csharp
// Umbral.Domain/Sesion/ValueObjects/Puntaje.cs
namespace Umbral.Domain.Sesion;

public sealed class Puntaje : ValueObject
{
    public int Valor { get; }

    private Puntaje(int valor) => Valor = valor;

    public static Puntaje Crear(int valor)
    {
        if (valor < 0)
            throw new DomainException("El puntaje no puede ser negativo.");
        return new Puntaje(valor);
    }

    public static Puntaje Zero() => new(0);

    public Puntaje Sumar(int cantidad)
        => new(Valor + cantidad);

    public Puntaje Restar(int cantidad)
        => new(Math.Max(0, Valor - cantidad));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
```

---

## 3. Application Layer — implementación

### 3.1 Command y Handler: CrearSesionBusquedaTesoro

```csharp
// Command
public sealed record CrearSesionBusquedaTesoroCommand(
    Guid MisionId,
    Guid OperadorId) : IRequest<Result<Guid>>;

// Validator
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

// Handler
public sealed class CrearSesionBusquedaTesoroCommandHandler
    : IRequestHandler<CrearSesionBusquedaTesoroCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IMisionRepository _misionRepository;
    private readonly IEventPublisher   _eventPublisher;

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
        CancellationToken cancellationToken)
    {
        var mision = await _misionRepository
            .FindByIdAsync(new MisionId(command.MisionId), cancellationToken)
            ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        if (!mision.PuedeUsarseParaSesion())
            throw new DomainException(
                "La misión debe estar activa para crear una sesión.");

        var snapshot = MisionSnapshot.Desde(mision);
        var sesion   = Sesion.CrearBusquedaTesoro(
            snapshot, new UsuarioId(command.OperadorId));

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents, cancellationToken);
        sesion.ClearDomainEvents();

        return Result.Ok(sesion.SesionId.Valor);
    }
}
```

### 3.2 Pipeline Behaviors

```csharp
// ValidationBehavior
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
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}

// LoggingBehavior
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Ejecutando {Request}", name);

        var response = await next();

        _logger.LogInformation("Completado {Request}", name);
        return response;
    }
}
```

### 3.3 Query Handler con proyección directa

```csharp
// Query
public sealed record GetRankingSesionQuery(Guid SesionId)
    : IRequest<List<PosicionRankingDto>>;

// DTO
public sealed record PosicionRankingDto(
    int Posicion,
    string NombreEquipo,
    int PuntajeTotal,
    long TiempoAcumuladoMs);

// Handler
public sealed class GetRankingSesionQueryHandler
    : IRequestHandler<GetRankingSesionQuery, List<PosicionRankingDto>>
{
    private readonly UmbralDbContext _db;

    public GetRankingSesionQueryHandler(UmbralDbContext db)
        => _db = db;

    public async Task<List<PosicionRankingDto>> Handle(
        GetRankingSesionQuery query,
        CancellationToken cancellationToken)
    {
        var equipos = await _db.EquiposSesion
            .Where(e => e.SesionId == query.SesionId)
            .OrderByDescending(e => e.PuntajeTotal)
            .ThenBy(e => e.TiempoAcumuladoMs)
            .ToListAsync(cancellationToken);

        return equipos
            .Select((e, i) => new PosicionRankingDto(
                i + 1,
                e.Nombre,
                e.PuntajeTotal,
                e.TiempoAcumuladoMs))
            .ToList();
    }
}
```

---

## 4. Infrastructure Layer — implementación

### 4.1 DbContext

```csharp
// Umbral.Infrastructure/Persistence/UmbralDbContext.cs
namespace Umbral.Infrastructure.Persistence;

public sealed class UmbralDbContext : DbContext
{
    public UmbralDbContext(DbContextOptions<UmbralDbContext> options)
        : base(options) { }

    public DbSet<Mision>      Misiones      => Set<Mision>();
    public DbSet<Pregunta>    Preguntas     => Set<Pregunta>();
    public DbSet<Categoria>   Categorias    => Set<Categoria>();
    public DbSet<Sesion>      Sesiones      => Set<Sesion>();
    public DbSet<EquipoSesion> EquiposSesion => Set<EquipoSesion>();
    public DbSet<Evidencia>   Evidencias    => Set<Evidencia>();
    public DbSet<RespuestaTrivia> RespuestasTrivia => Set<RespuestaTrivia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InfrastructureAssemblyMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### 4.2 Configuración EF Core — Sesion

```csharp
// Umbral.Infrastructure/Persistence/Configurations/SesionConfiguration.cs
public sealed class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("sesiones");

        builder.HasKey(s => s.SesionId);

        builder.Property(s => s.SesionId)
            .HasConversion(id => id.Valor, val => new SesionId(val))
            .HasColumnName("id");

        builder.Property(s => s.TipoSesion)
            .HasConversion<string>()
            .HasColumnName("tipo_sesion")
            .IsRequired();

        builder.Property(s => s.Estado)
            .HasConversion<string>()
            .HasColumnName("estado")
            .IsRequired();

        builder.Property(s => s.OperadorId)
            .HasConversion(id => id.Valor, val => new UsuarioId(val))
            .HasColumnName("operador_id");

        builder.Property(s => s.IniciadaEn)
            .HasColumnName("iniciada_en");

        builder.Property(s => s.FinalizadaEn)
            .HasColumnName("finalizada_en")
            .IsRequired(false);

        builder.HasMany(s => s.Equipos)
            .WithOne()
            .HasForeignKey("sesion_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(s => s.ContextoBT, bt =>
        {
            bt.ToTable("contextos_bt");
            bt.Property(c => c.EtapaActualIndex)
                .HasColumnName("etapa_actual_index");
            bt.Property(c => c.MisionSnapshot)
                .HasConversion(
                    snap => JsonSerializer.Serialize(snap, null),
                    json => JsonSerializer.Deserialize<MisionSnapshot>(json, null)!)
                .HasColumnName("mision_snapshot_json");
        });

        builder.OwnsOne(s => s.ContextoTrivia, tv =>
        {
            tv.ToTable("contextos_trivia");
            tv.Property(c => c.PreguntaActualIndex)
                .HasColumnName("pregunta_actual_index");
            tv.Property(c => c.TimerCerradoEn)
                .HasColumnName("timer_cerrado_en")
                .IsRequired(false);
            tv.Property(c => c.PreguntasOrdenadas)
                .HasConversion(
                    list => JsonSerializer.Serialize(
                        list.Select(p => p.Valor), null),
                    json => JsonSerializer
                        .Deserialize<List<Guid>>(json, null)!
                        .Select(g => new PreguntaId(g)).ToList())
                .HasColumnName("preguntas_ordenadas_json");
        });
    }
}
```

### 4.3 Repositorio

```csharp
// Umbral.Infrastructure/Persistence/Repositories/SesionRepository.cs
public sealed class SesionRepository : ISesionRepository
{
    private readonly UmbralDbContext _db;

    public SesionRepository(UmbralDbContext db) => _db = db;

    public async Task SaveAsync(Sesion sesion, CancellationToken ct = default)
    {
        var exists = await _db.Sesiones
            .AnyAsync(s => s.SesionId == sesion.SesionId, ct);

        if (exists)
            _db.Sesiones.Update(sesion);
        else
            await _db.Sesiones.AddAsync(sesion, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<Sesion?> FindByIdAsync(
        SesionId id, CancellationToken ct = default)
        => await _db.Sesiones
            .Include(s => s.Equipos)
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
            .FirstOrDefaultAsync(s => s.SesionId == id, ct);

    public async Task<List<Sesion>> FindByOperadorAsync(
        UsuarioId operadorId, CancellationToken ct = default)
        => await _db.Sesiones
            .Where(s => s.OperadorId == operadorId)
            .OrderByDescending(s => s.IniciadaEn)
            .ToListAsync(ct);
}
```

### 4.4 Consumer RabbitMQ

```csharp
// Umbral.Infrastructure/Messaging/Consumers/PuntajeBusquedaConsumer.cs
public sealed class PuntajeBusquedaConsumer
    : IConsumer<EvidenciaValidada>
{
    private readonly ISesionRepository       _sesionRepository;
    private readonly ICalculoPuntajeStrategy _estrategia;
    private readonly ILogger<PuntajeBusquedaConsumer> _logger;

    public PuntajeBusquedaConsumer(
        ISesionRepository sesionRepository,
        ICalculoPuntajeStrategy estrategia,
        ILogger<PuntajeBusquedaConsumer> logger)
    {
        _sesionRepository = sesionRepository;
        _estrategia       = estrategia;
        _logger           = logger;
    }

    public async Task Consume(ConsumeContext<EvidenciaValidada> context)
    {
        var msg    = context.Message;
        var sesion = await _sesionRepository
            .FindByIdAsync(msg.SesionId, context.CancellationToken)
            ?? throw new InvalidOperationException(
                $"Sesión {msg.SesionId} no encontrada.");

        var equipo = sesion.Equipos
            .First(e => e.EquipoId == msg.EquipoGanadorId);

        var puntaje = _estrategia.Calcular(msg.EsGanador);
        equipo.SumarPuntaje(puntaje.Valor);

        await _sesionRepository.SaveAsync(sesion, context.CancellationToken);

        _logger.LogInformation(
            "Puntaje {Puntos} asignado al equipo {EquipoId} en sesión {SesionId}",
            puntaje.Valor, msg.EquipoGanadorId, msg.SesionId);
    }
}
```

### 4.5 Hub SignalR

```csharp
// Umbral.Infrastructure/RealTime/Hubs/SesionHub.cs
[Authorize]
public sealed class SesionHub : Hub
{
    public async Task UnirseASesion(string sesionId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"sesion-{sesionId}");
    }

    public async Task UnirseComoOperador(string sesionId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"operador-{sesionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // SignalR limpia grupos automáticamente
        await base.OnDisconnectedAsync(exception);
    }
}

// Umbral.Infrastructure/RealTime/NotificacionRealTimeService.cs
public sealed class NotificacionRealTimeService : INotificacionRealTime
{
    private readonly IHubContext<SesionHub> _hubContext;

    public NotificacionRealTimeService(IHubContext<SesionHub> hubContext)
        => _hubContext = hubContext;

    public async Task BroadcastSesionAsync(
        Guid sesionId, string evento, object payload,
        CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"sesion-{sesionId}")
            .SendAsync(evento, payload, ct);

    public async Task NotificarOperadorAsync(
        Guid sesionId, string evento, object payload,
        CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"operador-{sesionId}")
            .SendAsync(evento, payload, ct);

    public async Task NotificarEquipoAsync(
        Guid equipoId, string evento, object payload,
        CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"equipo-{equipoId}")
            .SendAsync(evento, payload, ct);
}
```

---

## 5. API Layer — implementación

### 5.1 Controller delgado

```csharp
// Umbral.API/Controllers/SesionesController.cs
[ApiController]
[Route("api/v1/sesiones")]
[Authorize]
public sealed class SesionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SesionesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("busqueda-tesoro")]
    [Authorize(Roles = "Operador,Administrador")]
    public async Task<IActionResult> CrearBusquedaTesoro(
        [FromBody] CrearSesionBusquedaTesoroRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearSesionBusquedaTesoroCommand(
                request.MisionId,
                ObtenerUsuarioId()), ct);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = result.Value },
            new { id = result.Value });
    }

    [HttpPost("{id:guid}/iniciar")]
    [Authorize(Roles = "Operador,Administrador")]
    public async Task<IActionResult> Iniciar(
        Guid id, CancellationToken ct)
    {
        await _mediator.Send(new IniciarSesionCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/ranking")]
    public async Task<IActionResult> ObtenerRanking(
        Guid id, CancellationToken ct)
    {
        var ranking = await _mediator.Send(
            new GetRankingSesionQuery(id), ct);
        return Ok(ranking);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(
        Guid id, CancellationToken ct)
    {
        var sesion = await _mediator.Send(
            new GetSesionByIdQuery(id), ct);
        return Ok(sesion);
    }

    private Guid ObtenerUsuarioId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
```

### 5.2 Middleware de excepciones

```csharp
// Umbral.API/Middlewares/ExceptionHandlingMiddleware.cs
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(
        HttpContext context, Exception ex)
    {
        var (statusCode, tipo) = ex switch
        {
            ValidationException  => (400, "ValidationError"),
            DomainException      => (400, "DomainError"),
            NotFoundException    => (404, "NotFound"),
            UnauthorizedException=> (403, "Forbidden"),
            _                    => (500, "InternalServerError")
        };

        if (statusCode == 500)
            _logger.LogError(ex, "Error interno no controlado");
        else
            _logger.LogWarning(ex, "Error controlado: {Tipo}", tipo);

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            tipo,
            mensaje  = ex.Message,
            traceId  = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(body);
    }
}
```

### 5.3 Program.cs

```csharp
// Umbral.API/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddAuthentication(...)
    .AddAuthorization(options =>
    {
        options.AddPolicy("SoloAdmin",
            p => p.RequireRole("Administrador"));
        options.AddPolicy("OperadorOAdmin",
            p => p.RequireRole("Operador", "Administrador"));
    })
    .AddControllers()
    .AddSignalR()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SesionHub>("/hubs/sesion");
app.MapHub<TriviaHub>("/hubs/trivia");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
```

---

## 6. Patrones de diseño — implementación concreta

### 6.1 Strategy — cálculo de puntaje

```csharp
// Umbral.Domain/Shared/ICalculoPuntajeStrategy.cs
public interface ICalculoPuntajeStrategy
{
    Puntaje Calcular(ParametrosPuntaje parametros);
}

// BusquedaTesoroStrategy
public sealed class BusquedaTesoroStrategy : ICalculoPuntajeStrategy
{
    private const int PuntajePrimerLugar  = 100;
    private const int PuntajeSinPremio    = 0;

    public Puntaje Calcular(ParametrosPuntaje parametros)
        => parametros.EsGanador
            ? Puntaje.Crear(PuntajePrimerLugar)
            : Puntaje.Zero();
}

// TriviaStrategy
public sealed class TriviaStrategy : ICalculoPuntajeStrategy
{
    private const int PuntajeBase       = 100;
    private const int BonusVelocidadMax = 50;

    public Puntaje Calcular(ParametrosPuntaje parametros)
    {
        if (!parametros.EsCorrecta) return Puntaje.Zero();

        var bonus = CalcularBonusVelocidad(
            parametros.TiempoRespuestaMs,
            parametros.TimerTotalMs);

        return Puntaje.Crear(PuntajeBase + bonus);
    }

    private static int CalcularBonusVelocidad(
        long tiempoMs, long timerTotalMs)
    {
        if (timerTotalMs <= 0) return 0;
        var fraccion = 1.0 - ((double)tiempoMs / timerTotalMs);
        return (int)(BonusVelocidadMax * Math.Max(0, fraccion));
    }
}
```

### 6.2 Chain of Responsibility — validación de evidencia

```csharp
// Umbral.Domain/Sesion/Validacion/EvidenciaValidationChain.cs
public abstract class EvidenciaValidator
{
    private EvidenciaValidator? _siguiente;

    public EvidenciaValidator SetSiguiente(EvidenciaValidator siguiente)
    {
        _siguiente = siguiente;
        return siguiente;
    }

    public abstract ResultadoValidacion Validar(
        ContextoValidacionEvidencia ctx);

    protected ResultadoValidacion SiguienteOValida(
        ContextoValidacionEvidencia ctx)
        => _siguiente?.Validar(ctx) ?? ResultadoValidacion.Valida;
}

public sealed class ValidarSesionActivaHandler : EvidenciaValidator
{
    public override ResultadoValidacion Validar(
        ContextoValidacionEvidencia ctx)
    {
        if (!ctx.Sesion.EstaEnEstadoActivo())
            return ResultadoValidacion.Rechazada;
        return SiguienteOValida(ctx);
    }
}

public sealed class ValidarQRCoincideHandler : EvidenciaValidator
{
    public override ResultadoValidacion Validar(
        ContextoValidacionEvidencia ctx)
    {
        var etapa = ctx.Sesion.ContextoBT!.ObtenerEtapaActual();
        if (!etapa.CodigoQRSolucion.CoincideCon(ctx.CodigoQR))
            return ResultadoValidacion.Invalida;
        return SiguienteOValida(ctx);
    }
}
```

### 6.3 Template Method — procesamiento de evidencia

```csharp
// Umbral.Application/Sesion/Commands/SubmitEvidencia/EvidenciaProcessorBase.cs
public abstract class EvidenciaProcessorBase
{
    public async Task<ResultadoValidacion> ProcesarAsync(
        ContextoProcesamiento ctx,
        CancellationToken ct)
    {
        var resultado = Validar(ctx);
        if (resultado != ResultadoValidacion.Valida)
        {
            await PersistirResultadoAsync(ctx, resultado, ct);
            return resultado;
        }

        var puntaje = CalcularPuntaje(ctx);
        AplicarPuntaje(ctx, puntaje);
        await PersistirResultadoAsync(ctx, resultado, ct);
        await NotificarAsync(ctx, puntaje, ct);

        return resultado;
    }

    protected abstract ResultadoValidacion Validar(
        ContextoProcesamiento ctx);
    protected abstract Puntaje CalcularPuntaje(
        ContextoProcesamiento ctx);
    protected abstract void AplicarPuntaje(
        ContextoProcesamiento ctx, Puntaje puntaje);
    protected abstract Task PersistirResultadoAsync(
        ContextoProcesamiento ctx,
        ResultadoValidacion resultado,
        CancellationToken ct);
    protected abstract Task NotificarAsync(
        ContextoProcesamiento ctx,
        Puntaje puntaje,
        CancellationToken ct);
}
```

---

## 7. Seguridad

### 7.1 Roles y claims

> Identidad gestionada por **Keycloak** (realm `umbral`). La API valida el token
> (resource server). Ver `.cursor/skills/keycloak-auth-skill.md`.

Claim: realm_access.roles = ["Administrador" | "Operador" | "EquipoParticipante"]
       (se aplana a claims `role` en la API)
Claim: sub  = userId en Keycloak (Guid/UUID)
Claim: sesionId = sesionId (solo para EquipoParticipante; via mapper de Keycloak)

### 7.2 Endpoints por rol

| Endpoint                              | Roles autorizados              |
|---------------------------------------|--------------------------------|
| POST /misiones                        | Administrador                  |
| PUT  /misiones/{id}                   | Administrador                  |
| POST /sesiones/busqueda-tesoro        | Operador, Administrador        |
| POST /sesiones/trivia                 | Operador, Administrador        |
| POST /sesiones/{id}/iniciar           | Operador, Administrador        |
| POST /sesiones/{id}/pistas/{pId}/liberar | Operador, Administrador     |
| POST /sesiones/{id}/penalizaciones    | Operador, Administrador        |
| GET  /sesiones/{id}/ranking           | Todos los autenticados         |
| POST /sesiones/{id}/evidencias        | EquipoParticipante             |
| POST /sesiones/{id}/respuestas-trivia | EquipoParticipante             |

---

## 8. Registro de dependencias por capa

```csharp
// ApplicationServiceExtensions
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(ApplicationAssemblyMarker).Assembly));
services.AddValidatorsFromAssembly(
    typeof(ApplicationAssemblyMarker).Assembly);
services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));
services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

// InfrastructureServiceExtensions
services.AddDbContext<UmbralDbContext>(opts =>
    opts.UseNpgsql(config.GetConnectionString("Default")));

services.AddScoped<ISesionRepository,   SesionRepository>();
services.AddScoped<IMisionRepository,   MisionRepository>();
services.AddScoped<IPreguntaRepository, PreguntaRepository>();
services.AddScoped<ICategoriaRepository,CategoriaRepository>();
services.AddScoped<INotificacionRealTime, NotificacionRealTimeService>();

// Strategy registrada por TipoSesion
services.AddKeyedScoped<ICalculoPuntajeStrategy,
    BusquedaTesoroStrategy>("BusquedaTesoro");
services.AddKeyedScoped<ICalculoPuntajeStrategy,
    TriviaStrategy>("Trivia");

services.AddMassTransit(x =>
{
    x.AddConsumer<AuditoriaConsumer>();
    x.AddConsumer<PuntajeBusquedaConsumer>();
    x.AddConsumer<PuntajeTriviaConsumer>();
    x.AddConsumer<NotificacionesConsumer>();
    x.AddConsumer<TransicionPreguntaConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(config["RabbitMQ:Host"]);
        cfg.ConfigureEndpoints(ctx);
    });
});
```