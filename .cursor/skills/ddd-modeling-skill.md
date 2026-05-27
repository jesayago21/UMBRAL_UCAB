# Skill: DDD Modeling — Proyecto UMBRAL

> **Trazabilidad:** `docs/TRAZABILIDAD.md` (RB/HU/RNF). En código, `RB-xx-yy` = criterio de iteración Fase 1; `RB-xx` = regla global.

## Propósito
Guía paso a paso para modelar, implementar y evolucionar elementos de
Domain-Driven Design dentro del monolito hexagonal de UMBRAL.
Aplica a los tres Bounded Contexts: **CatalogoBusquedaTesoro**,
**CatalogoTrivia** y **Sesion** (Ejecución de Sesión).

> **Fuente de verdad:** `.cursor/specs/umbral-backend-spec.md` y
> `.cursor/specs/umbral-product-spec.md`. Este skill refleja exactamente esos specs.

---

## 1. Mapa de Bounded Contexts

```
┌─────────────────────────────────────────────────────────────────┐
│                        UMBRAL  Monolith                         │
│                                                                 │
│  ┌──────────────────────┐   ┌──────────────────────┐           │
│  │ CatalogoBusqueda     │   │ CatalogoTrivia        │           │
│  │ Tesoro               │   │                       │           │
│  │  AR: Mision          │   │  AR: Pregunta         │           │
│  │    ├── Etapa (E)     │   │    ├── OpcionRespuesta│           │
│  │    │    └── Pista(E) │   │  AR: Categoria        │           │
│  │  VO: MisionSnapshot  │   │                       │           │
│  │  VO: CodigoQR        │   │                       │           │
│  └──────────┬───────────┘   └──────────┬────────────┘          │
│             │  MisionSnapshot (ACL)    │  PreguntaId (ACL)      │
│             └──────────┬──────────────┘                        │
│                        ▼                                        │
│         ┌──────────────────────────┐                           │
│         │    Sesion BC             │                           │
│         │  AR: Sesion              │                           │
│         │    TipoSesion (enum)     │ ← BusquedaTesoro | Trivia │
│         │    ContextoBT? (E)       │ ← solo si BusquedaTesoro  │
│         │    ContextoTrivia? (E)   │ ← solo si Trivia          │
│         │    EquipoSesion (E)      │                           │
│         │    Evidencia (E)         │ ← solo BusquedaTesoro     │
│         │    RespuestaTrivia (E)   │ ← solo Trivia             │
│         │    Penalizacion (VO)     │                           │
│         │    EventoSesion (E)      │                           │
│         └──────────────────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
```

**Patrón Composite en CatalogoBusquedaTesoro:**
`Mision` contiene `List<Etapa>`, cada `Etapa` contiene `List<Pista>`.

**Regla de oro:** ningún BC importa directamente las entidades de otro.
`EjecucionSesion` recibe un `MisionSnapshot` (copia inmutable) al crear la sesión BT.
Las preguntas de Trivia se referencian solo por `PreguntaId`.

---

## 2. Estructura de carpetas por BC

```
src/
└── Umbral.Domain/
    ├── CatalogoBusquedaTesoro/
    │   └── Mision/
    │       ├── Mision.cs              ← AggregateRoot (contiene Etapas → Pistas)
    │       ├── Etapa.cs               ← Entity hija de Mision
    │       ├── Pista.cs               ← Entity hija de Etapa
    │       ├── MisionSnapshot.cs      ← ValueObject (copia inmutable para Sesion)
    │       ├── EtapaSnapshot.cs       ← ValueObject
    │       ├── MisionId.cs            ← strongly-typed ID
    │       ├── EstadoMision.cs        ← enum
    │       ├── TipoLiberacion.cs      ← enum (PorTiempo | PorGanador)
    │       └── IMisionRepository.cs  ← port de salida
    ├── CatalogoTrivia/
    │   ├── Pregunta/
    │   │   ├── Pregunta.cs            ← AggregateRoot
    │   │   ├── OpcionRespuesta.cs     ← ValueObject (inmutable, con IsCorrecta)
    │   │   ├── PreguntaId.cs
    │   │   └── IPreguntaRepository.cs
    │   └── Categoria/
    │       ├── Categoria.cs           ← AggregateRoot
    │       ├── CategoriaId.cs
    │       └── ICategoriaRepository.cs
    ├── Sesion/                         ← BC: Ejecución de Sesión
    │   ├── Sesion.cs                  ← AggregateRoot principal
    │   ├── ContextoBusquedaTesoro.cs  ← Entity (solo si TipoSesion=BusquedaTesoro)
    │   ├── ContextoTrivia.cs          ← Entity (solo si TipoSesion=Trivia)
    │   ├── EquipoSesion.cs            ← Entity
    │   ├── Evidencia.cs               ← Entity (BusquedaTesoro)
    │   ├── RespuestaTrivia.cs         ← Entity (Trivia)
    │   ├── EventoSesion.cs            ← Entity (historial)
    │   ├── ValueObjects/
    │   │   ├── SesionId.cs
    │   │   ├── EquipoId.cs
    │   │   ├── TipoSesion.cs         ← enum: BusquedaTesoro | Trivia
    │   │   ├── EstadoSesion.cs       ← enum: Programada|EnPreparacion|Activa|Pausada|Finalizada|Cancelada
    │   │   ├── Puntaje.cs            ← ValueObject (no record — hereda ValueObject)
    │   │   ├── Penalizacion.cs       ← ValueObject
    │   │   └── CodigoAcceso.cs       ← ValueObject
    │   └── ISesionRepository.cs      ← port de salida
    └── Shared/
        ├── AggregateRoot.cs           ← base sin genérico: AggregateRoot : Entity
        ├── Entity.cs
        ├── ValueObject.cs             ← con GetEqualityComponents()
        ├── DomainException.cs
        └── IDomainEvent.cs
```

---

## 3. Plantillas canónicas

### 3.1 Clases base compartidas

```csharp
// Shared/AggregateRoot.cs
namespace Umbral.Domain.Shared;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Shared/Entity.cs
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

// Shared/ValueObject.cs
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

### 3.2 Aggregate Root — Mision (CatalogoBusquedaTesoro)

```csharp
// CatalogoBusquedaTesoro/Mision/Mision.cs
namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

/// <summary>
/// Aggregate Root del BC CatalogoBusquedaTesoro.
/// Implementa el patrón Composite: Mision → List<Etapa> → List<Pista>.
/// </summary>
public sealed class Mision : AggregateRoot
{
    public MisionId MisionId { get; private set; }
    public string Nombre { get; private set; }
    public EstadoMision Estado { get; private set; }

    private readonly List<Etapa> _etapas = [];
    public IReadOnlyList<Etapa> Etapas => _etapas.AsReadOnly();

    private Mision() { }

    public static Mision Crear(string nombre)
    {
        Guard.Against.NullOrWhiteSpace(nombre);
        var mision = new Mision
        {
            MisionId = MisionId.Nuevo(),
            Nombre   = nombre,
            Estado   = EstadoMision.Borrador
        };
        mision.RaiseDomainEvent(new MisionCreada(mision.MisionId));
        return mision;
    }

    public void AgregarEtapa(string descripcion, string codigoQRSolucion)
    {
        Guard.Against.NullOrWhiteSpace(descripcion);
        var orden = _etapas.Count + 1;
        var etapa = Etapa.Crear(MisionId, orden, descripcion, codigoQRSolucion);
        _etapas.Add(etapa);
    }

    public void Activar()
    {
        if (_etapas.Count == 0)
            throw new DomainException("Una misión necesita al menos una etapa para activarse.");
        if (Estado != EstadoMision.Borrador)
            throw new DomainException("Solo se puede activar una misión en Borrador.");
        Estado = EstadoMision.Activa;
        RaiseDomainEvent(new MisionActivada(MisionId));
    }

    public bool PuedeUsarseParaSesion() => Estado == EstadoMision.Activa;

    protected override bool IdEquals(Entity other)
        => other is Mision m && m.MisionId == MisionId;
    protected override int GetIdHashCode() => MisionId.GetHashCode();
}
```

### 3.3 Entity hija — Etapa y Pista (Composite)

```csharp
// CatalogoBusquedaTesoro/Mision/Etapa.cs
namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed class Etapa : Entity
{
    public EtapaId EtapaId { get; private set; }
    public MisionId MisionId { get; private set; }
    public int Orden { get; private set; }
    public string Descripcion { get; private set; }
    public string CodigoQRSolucion { get; private set; }

    private readonly List<Pista> _pistas = [];
    public IReadOnlyList<Pista> Pistas => _pistas.AsReadOnly();

    private Etapa() { }

    internal static Etapa Crear(
        MisionId misionId, int orden, string descripcion, string codigoQR)
    {
        return new Etapa
        {
            EtapaId = EtapaId.Nuevo(),
            MisionId = misionId,
            Orden = orden,
            Descripcion = descripcion,
            CodigoQRSolucion = codigoQR
        };
    }

    public void AgregarPista(string contenido, TipoLiberacion tipoLiberacion, int? segundosLiberacion = null)
    {
        var pista = Pista.Crear(EtapaId, contenido, tipoLiberacion, segundosLiberacion);
        _pistas.Add(pista);
    }

    protected override bool IdEquals(Entity other)
        => other is Etapa e && e.EtapaId == EtapaId;
    protected override int GetIdHashCode() => EtapaId.GetHashCode();
}

// CatalogoBusquedaTesoro/Mision/Pista.cs
public sealed class Pista : Entity
{
    public PistaId PistaId { get; private set; }
    public EtapaId EtapaId { get; private set; }
    public string Contenido { get; private set; }
    public TipoLiberacion TipoLiberacion { get; private set; }
    public int? SegundosLiberacion { get; private set; }  // solo si PorTiempo

    private Pista() { }

    internal static Pista Crear(
        EtapaId etapaId, string contenido,
        TipoLiberacion tipo, int? segundos)
    {
        if (tipo == TipoLiberacion.PorTiempo && (segundos is null or <= 0))
            throw new DomainException("PorTiempo requiere segundos de liberación.");

        return new Pista
        {
            PistaId = PistaId.Nuevo(),
            EtapaId = etapaId,
            Contenido = contenido,
            TipoLiberacion = tipo,
            SegundosLiberacion = segundos
        };
    }

    protected override bool IdEquals(Entity other)
        => other is Pista p && p.PistaId == PistaId;
    protected override int GetIdHashCode() => PistaId.GetHashCode();
}
```

### 3.4 Value Object MisionSnapshot (inmutable, ACL hacia Sesion)

```csharp
// CatalogoBusquedaTesoro/Mision/MisionSnapshot.cs
namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

/// <summary>
/// Copia inmutable de la misión en el momento de crear la sesión.
/// Los cambios posteriores a la Mision NO afectan sesiones en curso (RB-11).
/// Es el contrato de Anti-Corruption Layer entre CatalogoBusquedaTesoro y Sesion BC.
/// </summary>
public sealed class MisionSnapshot : ValueObject
{
    public MisionId MisionId { get; }
    public string Nombre { get; }
    public IReadOnlyList<EtapaSnapshot> Etapas { get; }

    private MisionSnapshot(MisionId misionId, string nombre, IReadOnlyList<EtapaSnapshot> etapas)
    {
        MisionId = misionId;
        Nombre   = nombre;
        Etapas   = etapas;
    }

    public static MisionSnapshot Desde(Mision mision)
        => new(
            mision.MisionId,
            mision.Nombre,
            mision.Etapas
                .OrderBy(e => e.Orden)
                .Select(EtapaSnapshot.Desde)
                .ToList()
                .AsReadOnly());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MisionId;
        yield return Nombre;
    }
}

public sealed class EtapaSnapshot : ValueObject
{
    public EtapaId EtapaId { get; }
    public int Orden { get; }
    public string Descripcion { get; }
    public string CodigoQRSolucion { get; }

    private EtapaSnapshot(EtapaId id, int orden, string desc, string qr)
    {
        EtapaId = id; Orden = orden; Descripcion = desc; CodigoQRSolucion = qr;
    }

    public static EtapaSnapshot Desde(Etapa etapa)
        => new(etapa.EtapaId, etapa.Orden, etapa.Descripcion, etapa.CodigoQRSolucion);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EtapaId;
        yield return Orden;
    }
}
```

### 3.5 Aggregate Root — Sesion (BC Sesion)

```csharp
// Sesion/Sesion.cs
namespace Umbral.Domain.Sesion;

public sealed class Sesion : AggregateRoot
{
    // ── Identidad ──────────────────────────────────────────────
    public SesionId SesionId { get; private set; }

    // ── Propiedades de negocio ─────────────────────────────────
    public TipoSesion TipoSesion { get; private set; }   // BusquedaTesoro | Trivia
    public UsuarioId OperadorId { get; private set; }
    public EstadoSesion Estado { get; private set; }
    public DateTime IniciadaEn { get; private set; }
    public DateTime? FinalizadaEn { get; private set; }

    // ── Contexto específico de tipo ────────────────────────────
    public ContextoBusquedaTesoro? ContextoBT { get; private set; }     // solo BT
    public ContextoTrivia? ContextoTrivia { get; private set; }          // solo Trivia

    // ── Colecciones ────────────────────────────────────────────
    private readonly List<EquipoSesion> _equipos = [];
    private readonly List<EventoSesion> _historialEventos = [];
    private readonly List<Evidencia> _evidencias = [];
    private readonly List<RespuestaTrivia> _respuestas = [];

    public IReadOnlyList<EquipoSesion> Equipos => _equipos.AsReadOnly();
    public IReadOnlyList<EventoSesion> HistorialEventos => _historialEventos.AsReadOnly();
    public IReadOnlyList<Evidencia> Evidencias => _evidencias.AsReadOnly();
    public IReadOnlyList<RespuestaTrivia> Respuestas => _respuestas.AsReadOnly();

    private Sesion() { }

    // ── Factory methods (uno por tipo) ─────────────────────────
    public static Sesion CrearBusquedaTesoro(MisionSnapshot snapshot, UsuarioId operadorId)
    {
        Guard.Against.Null(snapshot);
        Guard.Against.Null(operadorId);

        var sesion = new Sesion
        {
            SesionId   = SesionId.Nuevo(),
            TipoSesion = TipoSesion.BusquedaTesoro,
            OperadorId = operadorId,
            Estado     = EstadoSesion.Programada,
            ContextoBT = ContextoBusquedaTesoro.Crear(snapshot)
        };
        sesion.RaiseDomainEvent(new SesionCreada(sesion.SesionId, TipoSesion.BusquedaTesoro, operadorId));
        return sesion;
    }

    public static Sesion CrearTrivia(List<PreguntaId> preguntasOrdenadas, UsuarioId operadorId)
    {
        Guard.Against.Null(operadorId);
        if (preguntasOrdenadas.Count == 0)
            throw new DomainException("Una sesión de trivia necesita al menos una pregunta.");

        var sesion = new Sesion
        {
            SesionId       = SesionId.Nuevo(),
            TipoSesion     = TipoSesion.Trivia,
            OperadorId     = operadorId,
            Estado         = EstadoSesion.Programada,
            ContextoTrivia = ContextoTrivia.Crear(preguntasOrdenadas)
        };
        sesion.RaiseDomainEvent(new SesionCreada(sesion.SesionId, TipoSesion.Trivia, operadorId));
        return sesion;
    }

    // ── Máquina de estados (patrón State) ──────────────────────
    // Programada → EnPreparacion → Activa ⇄ Pausada → Finalizada
    // Cancelada desde cualquier estado no terminal

    public void Iniciar()
    {
        if (Estado != EstadoSesion.EnPreparacion)
            throw new DomainException($"No se puede iniciar una sesión en estado {Estado}.");
        if (!_equipos.Any())
            throw new DomainException("La sesión necesita al menos un equipo registrado.");

        Estado     = EstadoSesion.Activa;
        IniciadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new SesionIniciada(SesionId, TipoSesion));
        RegistrarEvento("SesionIniciada", $"Sesión iniciada por {OperadorId.Valor}");
    }

    public void Pausar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException($"No se puede pausar una sesión en estado {Estado}.");
        Estado = EstadoSesion.Pausada;
        RaiseDomainEvent(new SesionPausada(SesionId, DateTime.UtcNow));
        RegistrarEvento("SesionPausada", string.Empty);
    }

    public void Reanudar()
    {
        if (Estado != EstadoSesion.Pausada)
            throw new DomainException($"No se puede reanudar una sesión en estado {Estado}.");
        Estado = EstadoSesion.Activa;
        RegistrarEvento("SesionReanudada", string.Empty);
    }

    public void Finalizar()
    {
        if (Estado is not (EstadoSesion.Activa or EstadoSesion.Pausada))
            throw new DomainException($"No se puede finalizar una sesión en estado {Estado}.");
        Estado       = EstadoSesion.Finalizada;
        FinalizadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new SesionFinalizada(SesionId, DateTime.UtcNow));
        RegistrarEvento("SesionFinalizada", string.Empty);
    }

    public void Cancelar(string motivo)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException($"No se puede cancelar una sesión en estado {Estado}.");
        Estado       = EstadoSesion.Cancelada;
        FinalizadaEn = DateTime.UtcNow;
        RegistrarEvento("SesionCancelada", motivo);
    }

    public EquipoSesion RegistrarEquipo(string nombre)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException("No se pueden registrar equipos en una sesión cerrada.");
        if (_equipos.Any(e => e.Nombre.Valor == nombre))
            throw new DomainException($"Ya existe un equipo con el nombre '{nombre}'.");

        var equipo = EquipoSesion.Crear(SesionId, nombre);
        _equipos.Add(equipo);
        RegistrarEvento("EquipoRegistrado", nombre);
        return equipo;
    }

    public void AplicarPenalizacion(EquipoId equipoId, Penalizacion penalizacion)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException("Solo se pueden aplicar penalizaciones en sesiones activas.");
        var equipo = ObtenerEquipo(equipoId);
        equipo.AplicarPenalizacion(penalizacion);
        RaiseDomainEvent(new PenalizacionAplicada(
            SesionId, equipoId, penalizacion.Puntos, penalizacion.Motivo,
            penalizacion.OperadorId, DateTime.UtcNow));
    }

    public bool EstaActiva() => Estado == EstadoSesion.Activa;

    private EquipoSesion ObtenerEquipo(EquipoId equipoId)
        => _equipos.FirstOrDefault(e => e.EquipoId == equipoId)
           ?? throw new DomainException($"El equipo {equipoId.Valor} no pertenece a esta sesión.");

    private void RegistrarEvento(string tipo, string payload)
        => _historialEventos.Add(EventoSesion.Crear(SesionId, tipo, payload));

    protected override bool IdEquals(Entity other)
        => other is Sesion s && s.SesionId == SesionId;
    protected override int GetIdHashCode() => SesionId.GetHashCode();
}
```

### 3.6 Value Object (hereda ValueObject, NO record)

```csharp
// Sesion/ValueObjects/Puntaje.cs
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

    public Puntaje Sumar(int cantidad) => new(Valor + cantidad);
    public Puntaje Restar(int cantidad) => new(Math.Max(0, Valor - cantidad)); // RB-24

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}

// Sesion/ValueObjects/CodigoAcceso.cs
public sealed class CodigoAcceso : ValueObject
{
    public string Valor { get; }

    private CodigoAcceso(string valor) => Valor = valor;

    public static CodigoAcceso Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El código de acceso no puede estar vacío.");
        return new CodigoAcceso(valor);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
```

### 3.7 Domain Event

```csharp
// Sesion/Events/SesionIniciada.cs
namespace Umbral.Domain.Sesion;

public sealed record SesionIniciada(SesionId SesionId, TipoSesion TipoSesion)
    : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}

// Shared/IDomainEvent.cs
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OcurridoEn { get; }
}
```

### 3.8 Strongly-Typed ID

```csharp
// Sesion/ValueObjects/SesionId.cs
namespace Umbral.Domain.Sesion;

public sealed record SesionId(Guid Valor)
{
    public static SesionId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
```

### 3.9 Repository Interface (port de salida)

```csharp
// Sesion/ISesionRepository.cs
namespace Umbral.Domain.Sesion;

public interface ISesionRepository
{
    Task<Sesion?> FindByIdAsync(SesionId id, CancellationToken ct = default);
    Task<IReadOnlyList<Sesion>> FindActivasAsync(CancellationToken ct = default);
    Task SaveAsync(Sesion sesion, CancellationToken ct = default);
}

// CatalogoBusquedaTesoro/Mision/IMisionRepository.cs
public interface IMisionRepository
{
    Task<Mision?> FindByIdAsync(MisionId id, CancellationToken ct = default);
    Task<IReadOnlyList<Mision>> FindActivasAsync(CancellationToken ct = default);
    Task SaveAsync(Mision mision, CancellationToken ct = default);
}
```

---

## 4. Estados válidos de Sesion

```
Programada ──→ EnPreparacion ──→ Activa ⇄ Pausada ──→ Finalizada
     │               │              │         │
     └───────────────┴──────────────┴─────────┴──→ Cancelada
```

| Desde | Puede ir a |
|-------|-----------|
| Programada | EnPreparacion, Cancelada |
| EnPreparacion | Activa (si hay equipos), Cancelada |
| Activa | Pausada, Finalizada, Cancelada |
| Pausada | Activa, Finalizada, Cancelada |
| Finalizada | — (terminal) |
| Cancelada | — (terminal) |

---

## 5. Reglas de modelado — UMBRAL

| # | Regla | Motivo |
|---|-------|--------|
| 1 | `TipoSesion` vive **solo** en `Sesion` (AR). | Evita dispersión del discriminador. |
| 2 | El BC CatalogoBusquedaTesoro tiene `Mision` como AR (Composite con `Etapa` → `Pista`). | Es la raíz de la jerarquía de misiones. |
| 3 | `ContextoBT` y `ContextoTrivia` son entidades separadas. Solo una existe por sesión. | Separación de responsabilidades por tipo. |
| 4 | Los BCs de catálogo **no se referencian directamente** desde `Sesion`. | Se usa `MisionSnapshot` (ACL inmutable) o solo el ID. |
| 5 | Los métodos de comportamiento **lanzan `DomainException`** ante invariantes rotas. | El dominio es el guardián de sus propias reglas. |
| 6 | Constructores privados; usar **factory methods** (`Crear`, `CrearBusquedaTesoro`, etc.). | Garantiza que el objeto nace en estado válido. |
| 7 | Los IDs son **strongly-typed** (`SesionId(Guid Valor)`, `MisionId(Guid Valor)`, …). | Evita mezcla accidental de Guids. |
| 8 | Solo el **AR expone métodos públicos**; entidades hijas tienen métodos `internal`. | Protege invariantes del agregado. |
| 9 | Domain Events se acumulan en el AR y se despachan **después de persistir**. | Evita efectos secundarios antes de confirmar el commit. |
| 10 | Value Objects heredan `ValueObject` con `GetEqualityComponents()`. **No usar `record`** para VOs con reglas de negocio. | Los records no validan en el constructor. |

---

## 6. Checklist al crear/modificar un agregado

```
□ ¿El AR tiene constructor privado y factory method(s)?
□ ¿Las invariantes de negocio se validan dentro del AR (no en el handler)?
□ ¿Se lanza DomainEvent por cada cambio significativo de estado?
□ ¿El ID es strongly-typed (record con propiedad Valor)?
□ ¿Las colecciones se exponen como IReadOnlyList<T>?
□ ¿Las entidades hijas se crean con métodos internal static?
□ ¿TipoSesion está SOLO en Sesion y en ninguna entidad hija?
□ ¿El repositorio solo recibe/devuelve el AR, no entidades hijas sueltas?
□ ¿Los Domain Events se limpian (ClearDomainEvents) después del dispatch?
□ ¿Los Value Objects heredan ValueObject (no son record simples)?
□ ¿Los estados de Sesion coinciden con el enum EstadoSesion completo?
```

---

## 7. Anti-patrones a evitar

```csharp
// ❌ MALO — AR en CatalogoBusquedaTesoro es PistaBusqueda (incorrecto)
// El AR correcto es Mision. Pista es Entity hija de Etapa.
public class PistaBusqueda : AggregateRoot { }  // ← NUNCA

// ✅ CORRECTO
public class Mision : AggregateRoot { }  // AR
// Mision → Etapa (Entity) → Pista (Entity) = Composite

// ❌ MALO — TipoSesion en Etapa
public class Etapa { public TipoSesion Tipo { get; set; } }  // ← NUNCA

// ❌ MALO — Value Object como record (sin validación de negocio)
public sealed record Puntaje(int Valor);  // ← no valida que Valor >= 0

// ✅ BUENO — Value Object con ValueObject base y validación
public sealed class Puntaje : ValueObject
{
    public int Valor { get; }
    private Puntaje(int valor) => Valor = valor;
    public static Puntaje Crear(int valor)
    {
        if (valor < 0) throw new DomainException("El puntaje no puede ser negativo.");
        return new Puntaje(valor);
    }
    protected override IEnumerable<object> GetEqualityComponents() { yield return Valor; }
}

// ❌ MALO — estado simplificado de Sesion (Borrador no existe)
public enum EstadoSesion { Borrador, Activa, Finalizada }  // ← incorrecto

// ✅ CORRECTO — estados completos según specs
public enum EstadoSesion
{
    Programada, EnPreparacion, Activa, Pausada, Finalizada, Cancelada
}

// ❌ MALO — AggregateRoot genérico
public abstract class AggregateRoot<TId> { }  // ← no es el diseño de este proyecto

// ✅ CORRECTO — AggregateRoot hereda Entity (sin genérico)
public abstract class AggregateRoot : Entity { }

// ❌ MALO — referencia cruzada entre BCs
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;  // en namespace Sesion ← NUNCA
public class Sesion { public Mision Mision { get; set; } }  // ← usar MisionSnapshot

// ✅ BUENO — ACL mediante snapshot inmutable
public class Sesion { public ContextoBusquedaTesoro? ContextoBT { get; private set; } }
// ContextoBT contiene MisionSnapshot (VO inmutable del catálogo)
```

---

## 8. Guía de uso para Cursor AI

Cuando el usuario pida modelar algo de dominio en UMBRAL:

1. **Identificar BC** → ¿CatalogoBusquedaTesoro, CatalogoTrivia o Sesion?
2. **Clasificar el concepto** → ¿AR, Entity, ValueObject, Domain Service, Domain Event?
3. **Para CatalogoBusquedaTesoro**: el AR es `Mision`; `Etapa` y `Pista` son entities hijas (Composite).
4. **Para Sesion**: el AR es `Sesion`; los equipos son `EquipoSesion` (no `Equipo`).
5. **Aplicar plantilla** de la sección 3 correspondiente.
6. **Verificar estados**: Sesion usa `Programada/EnPreparacion/Activa/Pausada/Finalizada/Cancelada`.
7. **Verificar reglas** de la tabla de la sección 5 y correr checklist de la sección 6.
8. **Advertir** si el código cae en algún anti-patrón de la sección 7.
9. **Generar el stub del repositorio** (interfaz en Domain, implementación en Infrastructure).
10. Proponer el **Domain Event** correspondiente si hay cambio de estado significativo.
