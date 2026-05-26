# UMBRAL — Estándares de Código

## 1. C# / .NET 8

### 1.1 Estructura de archivos
- Un tipo público por archivo. El nombre del archivo = nombre de la clase.
- Namespaces basados en la carpeta: `Umbral.Domain.Sesion`, `Umbral.Application.Sesiones.Commands`.
- Usar `file-scoped namespaces` (sin llaves):
```csharp
  namespace Umbral.Domain.Sesion;

  public class Sesion { }
```

### 1.2 Clases y miembros
- `readonly` en todos los campos que no cambien tras construcción.
- `private` por defecto. Solo hacer público lo que realmente se necesita.
- Propiedades de entidades del dominio: getter público, setter `private`:
```csharp
  // ✅ Correcto
  public EstadoSesion Estado { get; private set; }

  // ❌ Prohibido
  public EstadoSesion Estado { get; set; }
```
- Constructores privados en entidades con factory method estático:
```csharp
  public static Sesion Crear(MisionSnapshot snapshot, UsuarioId operadorId)
  {
      // validaciones y creación
  }
```

### 1.3 Value Objects
- Siempre inmutables. Heredan de una clase base `ValueObject` con `EqualityComponents`.
- Se validan completamente en el constructor. Lanzan excepción de dominio si inválidos.
```csharp
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

### 1.4 Excepciones
- Las excepciones de dominio heredan de `DomainException` (definida en Domain).
- Las excepciones de aplicación heredan de `ApplicationException` (definida en Application).
- **Nunca** uses `Exception` genérico para representar errores de negocio.
- El middleware global en API captura y transforma a respuestas HTTP apropiadas:
  - `DomainException`       → 400 Bad Request
  - `NotFoundException`     → 404 Not Found
  - `UnauthorizedException` → 403 Forbidden
  - Cualquier otra          → 500 Internal Server Error

### 1.5 Async / Await
- Todos los métodos que tocan I/O son `async Task<T>`.
- Nunca uses `.Result` ni `.Wait()`. Provoca deadlocks.
- Siempre propaga `CancellationToken` desde el endpoint hasta el repositorio:
```csharp
  public async Task<Sesion?> FindByIdAsync(SesionId id, CancellationToken ct = default)
```

### 1.6 Null safety
- `nullable` habilitado en todos los proyectos (`<Nullable>enable</Nullable>`).
- Usa `?` solo cuando null es un valor legítimo del dominio.
- Nunca devuelvas `null` desde un repositorio: usa `Result<T>` o lanza `NotFoundException`.

### 1.7 Records para DTOs
- Los DTOs de Application (Request/Response) son `record`:
```csharp
  public record CrearSesionRequest(Guid MisionId, string NombreOperador);
  public record SesionResumenDto(Guid Id, string Estado, int TotalEquipos);
```

### 1.8 Pattern matching y expresiones
- Prefiere `switch expression` sobre `switch statement` para lógica de mapeo.
- Usa `is` pattern matching en lugar de casting explícito.

---

## 2. Organización de Commands y Queries

### 2.1 Estructura de carpetas en Application
Application/
└── Sesiones/
├── Commands/
│   ├── CrearSesion/
│   │   ├── CrearSesionCommand.cs
│   │   ├── CrearSesionCommandHandler.cs
│   │   └── CrearSesionCommandValidator.cs
│   └── IniciarSesion/
│       ├── IniciarSesionCommand.cs
│       ├── IniciarSesionCommandHandler.cs
│       └── IniciarSesionCommandValidator.cs
└── Queries/
├── GetSesionById/
│   ├── GetSesionByIdQuery.cs
│   └── GetSesionByIdQueryHandler.cs
└── GetRankingSesion/
├── GetRankingSesionQuery.cs
└── GetRankingSesionQueryHandler.cs

### 2.2 Anatomy de un Command Handler
```csharp
public sealed class CrearSesionCommandHandler
    : IRequestHandler<CrearSesionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IMisionRepository _misionRepository;
    private readonly IEventPublisher _eventPublisher;

    public CrearSesionCommandHandler(
        ISesionRepository sesionRepository,
        IMisionRepository misionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _misionRepository = misionRepository;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<Guid>> Handle(
        CrearSesionCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Obtener agregado
        var mision = await _misionRepository
            .FindByIdAsync(new MisionId(command.MisionId), cancellationToken)
            ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        // 2. Delegar al dominio
        var sesion = Sesion.Crear(MisionSnapshot.Desde(mision), new UsuarioId(command.OperadorId));

        // 3. Persistir
        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        // 4. Publicar eventos de dominio
        await _eventPublisher.PublishBatchAsync(sesion.DomainEvents, cancellationToken);

        return Result.Ok(sesion.SesionId.Valor);
    }
}
```

### 2.3 Anatomy de un Query Handler
```csharp
// Las queries pueden ir directamente contra el DbContext (read model)
// Sin pasar por repositorios ni agregados del dominio
public sealed class GetRankingSesionQueryHandler
    : IRequestHandler<GetRankingSesionQuery, List<PosicionRankingDto>>
{
    private readonly UmbralDbContext _dbContext;

    public GetRankingSesionQueryHandler(UmbralDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<List<PosicionRankingDto>> Handle(
        GetRankingSesionQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.EquiposSesion
            .Where(e => e.SesionId == query.SesionId)
            .OrderByDescending(e => e.PuntajeTotal)
            .Select(e => new PosicionRankingDto(e.Nombre, e.PuntajeTotal))
            .ToListAsync(cancellationToken);
    }
}
```

---

## 3. Entity Framework Core

- Una clase `IEntityTypeConfiguration<T>` por entidad, en carpeta `Infrastructure/Persistence/Configurations/`.
- **Nunca** configures el modelo con Data Annotations en las entidades del dominio.
  Toda configuración va en Fluent API.
- Nombres de tablas en `snake_case` y en español:
```csharp
  builder.ToTable("sesiones");
  builder.Property(s => s.Estado).HasColumnName("estado").HasConversion<string>();
```
- Las migraciones van en `Infrastructure/Persistence/Migrations/`.
- Nunca apliques migraciones automáticamente en producción desde código.

---

## 4. Validaciones con FluentValidation

- Un `AbstractValidator<TCommand>` por cada Command.
- Registrar todos los validators con el assembly scanner en el startup:
```csharp
  services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
```
- El pipeline behavior de MediatR ejecuta la validación antes del handler:
```csharp
  // ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
```
- Mensajes de error en español, claros y accionables:
```csharp
  RuleFor(x => x.MisionId)
      .NotEmpty().WithMessage("El identificador de la misión es obligatorio.")
      .Must(id => id != Guid.Empty).WithMessage("El identificador de la misión no es válido.");
```

---

## 5. Logging

- Usar `ILogger<T>` inyectado por constructor en handlers, middlewares y consumers.
- Niveles obligatorios:
  - `LogInformation` → inicio y fin de operaciones significativas.
  - `LogWarning`     → situaciones recuperables (validación fallida, reintento).
  - `LogError`       → excepciones no esperadas con stack trace.
- Incluir contexto estructurado:
```csharp
  _logger.LogInformation(
      "Sesion {SesionId} iniciada por operador {OperadorId}",
      sesion.SesionId.Valor, command.OperadorId);
```
- **Nunca** loguees datos sensibles (tokens, passwords, códigos QR completos).

---

## 6. TypeScript / React

### 6.1 Tipos
- `interface` para formas de objetos (props, DTOs, respuestas de API).
- `type` para uniones, aliases e intersecciones.
- `enum` solo para valores fijos del dominio que vienen del backend.
- Prohibido usar `any`. Usa `unknown` y type-guard si el tipo es incierto.

### 6.2 Componentes
- Funcionales siempre. Sin class components.
- Props tipadas con `interface` nombrada `[Componente]Props`.
- Extraer lógica con estado a custom hooks. El componente solo renderiza.
```tsx
  // ✅ Correcto
  const RankingBoard = ({ sesionId }: RankingBoardProps) => {
    const { ranking, isLoading } = useRanking(sesionId);
    return <RankingList items={ranking} loading={isLoading} />;
  };
```

### 6.3 Servicios HTTP
- Todas las llamadas van en `services/[modulo]Service.ts`.
- Usar `axios` con una instancia centralizada que incluye interceptores de auth y error.
- Los servicios devuelven tipos concretos, nunca `any`:
```ts
  export const sesionService = {
    getRanking: async (sesionId: string): Promise<PosicionRankingDto[]> => {
      const { data } = await apiClient.get(`/sesiones/${sesionId}/ranking`);
      return data;
    }
  };
```

### 6.4 WebSocket / SignalR
- La conexión SignalR vive en un hook: `useSessionSocket(sesionId)`.
- El hook gestiona connect/disconnect en `useEffect` con cleanup.
- Los eventos recibidos actualizan el estado local del hook, no el store global directamente.

---

## 7. Pruebas

### 7.1 Unitarias (xUnit + Moq)
```csharp
public class SesionTests
{
    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = SesionBuilder.ConEstado(EstadoSesion.EnPreparacion);

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionIniciada);
    }

    [Fact]
    public void Iniciar_CuandoYaEstaActiva_LanzaDomainException()
    {
        var sesion = SesionBuilder.ConEstado(EstadoSesion.Activa);
        var act = () => sesion.Iniciar();
        act.Should().Throw<DomainException>();
    }
}
```

### 7.2 Naming obligatorio
[Metodo][Escenario][ResultadoEsperado]

### 7.3 Estructura AAA
Toda prueba tiene exactamente tres secciones comentadas: `// Arrange`, `// Act`, `// Assert`.

### 7.4 Builders para entidades
Usar el patrón Builder para construir entidades en pruebas. Nunca repitas setup inline.
```csharp
// SesionBuilder.cs (solo en proyecto de pruebas)
public static class SesionBuilder
{
    public static Sesion ConEstado(EstadoSesion estado) { ... }
    public static Sesion ConEquipos(int cantidad) { ... }
}
```

---

## 8. Git

- Commits en español, imperativo, máximo 72 caracteres:
feat: agregar handler CrearSesionCommand
fix: corregir validación de código QR en Evidencia
test: cubrir casos de avance de etapa en Sesion
refactor: extraer Strategy de cálculo de puntaje

- Prefijos obligatorios: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`, `ci`.
- Una rama por funcionalidad: `feature/crear-sesion`, `fix/ranking-desempate`.
- Nunca hagas push directo a `main`. Todo por Pull Request.