# UMBRAL — Especificación de Calidad y Pruebas

> **HU canónicas:** numeración **HU-01…HU-40** del ERS (`docs/TRAZABILIDAD.md`).
> **Entrega 1 (alcance vigente):** `docs/entrega-1/PLAN.md` (reemplaza el antiguo
> "plan de 10 días" de esta spec). Seguimiento histórico por fases: `docs/fase-*/TRACKER.md`.

## 1. Estrategia general de pruebas

UMBRAL adopta una pirámide de pruebas con cuatro niveles:
                ┌─────────────┐
                │    E2E      │  ← Playwright (flujos completos)
               ┌┴─────────────┴┐
               │  Integración  │  ← Testcontainers (BD + API real)
              ┌┴───────────────┴┐
              │    Aplicación   │  ← xUnit + NSubstitute (handlers aislados)
             ┌┴─────────────────┴┐
             │      Dominio      │  ← xUnit puro (sin mocks)
             └───────────────────┘

| Nivel         | Herramienta               | Cobertura objetivo | Velocidad |
|---------------|---------------------------|--------------------|-----------|
| Dominio       | xUnit + FluentAssertions  | 100%               | < 1s      |
| Aplicación    | xUnit + NSubstitute       | ≥ 90%              | < 5s      |
| Integración   | xUnit + Testcontainers    | Flujos críticos    | < 60s     |
| E2E           | Playwright                | Flujo principal    | < 3 min   |

**Meta académica obligatoria:** cobertura total del backend ≥ 90% (**RNF-09**). Concurrencia trivia: **RNF-13**.

---

## 2. Proyectos de prueba

Estructura **implementada** en el repositorio (no renombrar sin consenso):

```
tests/
├── Umbral.Domain.Tests/          → dominio (sin mocks, sin BD)
├── Umbral.Application.Tests/     → handlers y validators (NSubstitute)
├── Umbral.Infrastructure.Tests/  → repositorios + EF + Testcontainers (PostgreSQL)
└── Umbral.API.Tests/             → controllers + WebApplicationFactory + Testcontainers
```

**Entrega 2 (pendiente):** `Umbral.E2E.Tests` con Playwright (flujos web completos).

Cada proyecto referencia solo lo que necesita:

```xml
<!-- Umbral.Domain.Tests.csproj -->
<ProjectReference Include="../src/backend/Umbral.Domain/Umbral.Domain.csproj" />
<PackageReference Include="xunit" />
<PackageReference Include="FluentAssertions" />

<!-- Umbral.Application.Tests.csproj -->
<ProjectReference Include="../src/backend/Umbral.Application/..." />
<ProjectReference Include="../src/backend/Umbral.Domain/..." />
<PackageReference Include="xunit" />
<PackageReference Include="NSubstitute" />
<PackageReference Include="FluentAssertions" />

<!-- Umbral.Infrastructure.Tests.csproj -->
<ProjectReference Include="../src/backend/Umbral.Infrastructure/..." />
<PackageReference Include="Testcontainers.PostgreSql" />
<PackageReference Include="FluentAssertions" />

<!-- Umbral.API.Tests.csproj -->
<ProjectReference Include="../src/backend/Umbral.API/..." />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
<PackageReference Include="Testcontainers.PostgreSql" />
<PackageReference Include="FluentAssertions" />
```

---

## 3. Convenciones de nomenclatura

### 3.1 Clases de prueba

[ClaseTesteada]Tests.cs

Ejemplos:
SesionTests.cs
ContextoBusquedaTesoroTests.cs
PuntajeTests.cs
CrearSesionCommandHandlerTests.cs
ValidacionEvidenciaServiceTests.cs

### 3.2 Métodos de prueba

[Metodo][Escenario][ResultadoEsperado]

Ejemplos:
```csharp
Iniciar_CuandoEstaEnPreparacion_CambiaEstadoAActiva()
Iniciar_CuandoYaEstaActiva_LanzaDomainException()
RegistrarEquipo_CuandoNombreDuplicado_LanzaDomainException()
Procesar_CuandoRespuestaCorrecta_RetornaPuntajePositivo()
Procesar_CuandoLlegaFueraDeTiempo_RetornaPuntajeCero()
```

### 3.3 Estructura AAA obligatoria

Toda prueba tiene exactamente estas tres secciones comentadas:

```csharp
[Fact]
public void Iniciar_CuandoEstaEnPreparacion_CambiaEstadoAActiva()
{
    // Arrange
    var sesion = SesionBuilder.EnPreparacion().Build();

    // Act
    sesion.Iniciar();

    // Assert
    sesion.Estado.Should().Be(EstadoSesion.Activa);
}
```

---

## 4. Pruebas de Dominio

Las pruebas del dominio son puras: sin mocks, sin base de datos, sin frameworks.
Solo instancian agregados y verifican comportamiento.

### 4.1 Agregado Sesion

```csharp
public class SesionTests
{
    // ── Ciclo de vida ──────────────────────────────────────────────

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConParticipante("Equipo Alpha")
            .Build();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
        sesion.IniciadaEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionIniciada);
    }

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.Activa)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Iniciar_CuandoEstadoNoEsEnPreparacion_LanzaDomainException(
        EstadoSesion estadoInvalido)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoInvalido)
            .Build();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Iniciar_SinParticipantesRegistrados_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinParticipantes()
            .Build();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos un participante*");
    }

    [Fact]
    public void Pausar_CuandoEstaActiva_CambiaEstadoAPausada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().Build();

        // Act
        sesion.Pausar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Pausada);
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionPausada);
    }

    [Fact]
    public void Finalizar_CuandoEstaActiva_CambiaEstadoYRegistraFecha()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Finalizar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        sesion.FinalizadaEn.Should().NotBeNull();
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionFinalizada);
    }

    // ── Participantes ───────────────────────────────────────────────────

    [Fact]
    public void RegistrarEquipo_CuandoNombreUnico_AgregaParticipante()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion).Build();

        // Act
        var participante = sesion.RegistrarEquipo("Los Sabuesos");

        // Assert
        sesion.Participantes.Should().ContainSingle();
        participante.Nombre.Valor.Should().Be("Los Sabuesos");
        participante.CodigoAcceso.Should().NotBeNull();
    }

    [Fact]
    public void RegistrarEquipo_CuandoNombreDuplicado_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConParticipante("Los Sabuesos")
            .Build();

        // Act
        var act = () => sesion.RegistrarEquipo("Los Sabuesos");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void RegistrarEquipo_CuandoSesionCerrada_LanzaDomainException(
        EstadoSesion estadoCerrado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoCerrado).Build();

        // Act
        var act = () => sesion.RegistrarEquipo("Nuevo Equipo");

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Penalizaciones ────────────────────────────────────────────

    [Fact]
    public void AplicarPenalizacion_CuandoSesionActiva_RestarPuntaje()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").Build();
        var participante     = sesion.Participantes.First();
        var penalizacion = new Penalizacion(
            10, "Trampa detectada", new UsuarioId(Guid.NewGuid()));

        participante.SumarPuntaje(50);

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, penalizacion);

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(40);
        sesion.DomainEvents.Should()
            .ContainSingle(e => e is PenalizacionAplicada);
    }
}
```

### 4.2 Value Object Puntaje

```csharp
public class PuntajeTests
{
    [Fact]
    public void Crear_CuandoValorNegativo_LanzaDomainException()
    {
        var act = () => Puntaje.Crear(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Restar_CuandoResultadoSeriaNegatvo_RetornaCero()
    {
        // Arrange
        var puntaje = Puntaje.Crear(10);

        // Act
        var resultado = puntaje.Restar(50);

        // Assert
        resultado.Valor.Should().Be(0);
    }

    [Fact]
    public void Sumar_RetornaValorCorrecto()
    {
        var puntaje   = Puntaje.Crear(30);
        var resultado = puntaje.Sumar(20);
        resultado.Valor.Should().Be(50);
    }
}
```

### 4.3 Domain Service — ValidacionRespuestaTriviaService

```csharp
public class ValidacionRespuestaTriviaServiceTests
{
    private readonly ValidacionRespuestaTriviaService _sut = new();

    [Fact]
    public void EsFueraDeTiempo_CuandoTimestampPosteriorAlCierre_RetornaTrue()
    {
        // Arrange
        var cierre    = DateTime.UtcNow;
        var timestamp = cierre.AddMilliseconds(200); // llegó tarde

        // Act
        var resultado = _sut.EsFueraDeTiempo(timestamp, cierre);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EsFueraDeTiempo_CuandoTimestampAnteriorAlCierre_RetornaFalse()
    {
        // Arrange
        var cierre    = DateTime.UtcNow;
        var timestamp = cierre.AddMilliseconds(-100); // llegó antes

        // Act
        var resultado = _sut.EsFueraDeTiempo(timestamp, cierre);

        // Assert
        resultado.Should().BeFalse();
    }
}
```

---

## 5. Builders de prueba

Patrón Builder obligatorio para construir entidades en pruebas.
Nunca repetir setup inline.

```csharp
// tests/Umbral.Domain.Tests/Builders/SesionBuilder.cs
public class SesionBuilder
{
    private TipoSesion     _tipo       = TipoSesion.BusquedaTesoro;
    private EstadoSesion   _estado     = EstadoSesion.Programada;
    private List<string>   _participantes    = [];
    private MisionSnapshot _snapshot   = MisionSnapshotFaker.Valido();
    private List<PreguntaId> _preguntas = [];

    public static SesionBuilder BusquedaTesoro() =>
        new() { _tipo = TipoSesion.BusquedaTesoro };

    public static SesionBuilder Trivia() =>
        new() { _tipo = TipoSesion.Trivia };

    public SesionBuilder ConEstado(EstadoSesion estado)
    {
        _estado = estado;
        return this;
    }

    public SesionBuilder Activa()
    {
        _estado = EstadoSesion.Activa;
        _participantes.Add("Equipo Default");
        return this;
    }

    public SesionBuilder ConParticipante(string nombre)
    {
        _participantes.Add(nombre);
        return this;
    }

    public SesionBuilder SinParticipantes()
    {
        _participantes = [];
        return this;
    }

    public Sesion Build()
    {
        var operadorId = new UsuarioId(Guid.NewGuid());

        Sesion sesion = _tipo == TipoSesion.BusquedaTesoro
            ? Sesion.CrearBusquedaTesoro(_snapshot, operadorId)
            : Sesion.CrearTrivia(
                _preguntas.Any()
                    ? _preguntas
                    : [new PreguntaId(Guid.NewGuid())],
                operadorId);

        // Forzar estado mediante reflection (solo en pruebas)
        if (_estado != EstadoSesion.Programada)
            typeof(Sesion)
                .GetProperty(nameof(Sesion.Estado))!
                .SetValue(sesion, _estado);

        foreach (var nombre in _participantes)
            sesion.RegistrarEquipo(nombre);

        sesion.ClearDomainEvents();
        return sesion;
    }
}
```

---

## 6. Pruebas de Aplicación (Handlers)

Los handlers se prueban con repositorios falsos (**NSubstitute**).
Nunca con base de datos real.

```csharp
public sealed class CrearSesionBusquedaTesoroCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IMisionRepository _misionRepo = Substitute.For<IMisionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly CrearSesionBusquedaTesoroCommandHandler _sut;

    public CrearSesionBusquedaTesoroCommandHandlerTests()
    {
        _sut = new CrearSesionBusquedaTesoroCommandHandler(
            _sesionRepo, _misionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoMisionActiva_CreaSesionYPublicaEvento()
    {
        // Arrange
        var mision = MisionTestBuilder.Activa();
        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(mision);

        var command = new CrearSesionBusquedaTesoroCommand(
            mision.MisionId.Valor, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _sesionRepo.Received(1).SaveAsync(
            Arg.Any<Sesion>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishBatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoMisionNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns((Mision?)null);

        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoMisionInactiva_LanzaDomainException()
    {
        // Arrange
        var mision = MisionTestBuilder.Inactiva();
        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(mision);

        var command = new CrearSesionBusquedaTesoroCommand(
            mision.MisionId.Valor, Guid.NewGuid());

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<Sesion>(), Arg.Any<CancellationToken>());
    }
}
```

---

## 7. Pruebas de Integración

Usan una base de datos PostgreSQL real levantada con Testcontainers.
Se ejecutan en CI dentro del job de backend.

```csharp
// tests/Umbral.API.Tests/SesionIntegrationTests.cs
public class SesionIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplaza el DbContext por uno apuntando al contenedor
                    var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<UmbralDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<UmbralDbContext>(opts =>
                        opts.UseNpgsql(_postgres.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();

        // Aplicar migraciones
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CrearSesion_CuandoDatosValidos_PersistEnBD()
    {
        // Arrange — primero crear una misión activa
        var misionId = await CrearMisionActivaAsync();

        var request = new { misionId };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content
            .ReadFromJsonAsync<CrearSesionResponse>();
        body!.Id.Should().NotBeEmpty();

        // Verificar persistencia real
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
        var sesion = await db.Sesiones.FindAsync(body.Id);

        sesion.Should().NotBeNull();
        sesion!.Estado.Should().Be(EstadoSesion.Programada);
        sesion.TipoSesion.Should().Be(TipoSesion.BusquedaTesoro);
    }

    [Fact]
    public async Task GetRanking_CuandoSesionConParticipantes_RetornaOrdenCorrecto()
    {
        // Arrange
        var sesionId = await CrearSesionConParticipantesAsync(
            ("Alpha", 150),
            ("Beta", 200),
            ("Gamma", 75));

        // Act
        var response = await _client
            .GetAsync($"/api/v1/sesiones/{sesionId}/ranking");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ranking = await response.Content
            .ReadFromJsonAsync<List<PosicionRankingDto>>();

        ranking![0].NombreParticipante.Should().Be("Beta");
        ranking[1].NombreParticipante.Should().Be("Alpha");
        ranking[2].NombreParticipante.Should().Be("Gamma");
    }

    private async Task<Guid> CrearMisionActivaAsync() { /* helper */ }
    private async Task<Guid> CrearSesionConParticipantesAsync(
        params (string nombre, int puntaje)[] participantes) { /* helper */ }
}
```

---

## 8. Pruebas E2E con Playwright

Cubren el flujo principal de punta a punta con el sistema completo levantado.

```csharp
// tests/Umbral.E2E.Tests/FlujoSesionBusquedaTesoroTests.cs
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class FlujoSesionBusquedaTesoroTests : PageTest
{
    private const string BaseUrl = "http://localhost:3000";

    [Test]
    public async Task FlujoCompleto_CrearSesion_RegistrarEquipo_EnviarEvidencia_VerRanking()
    {
        // 1. Operador inicia sesión
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("[data-testid=email]", "operador@umbral.com");
        await Page.FillAsync("[data-testid=password]", "test1234");
        await Page.ClickAsync("[data-testid=btn-login]");
        await Page.WaitForURLAsync("**/operador/sesiones");

        // 2. Crear sesión BusquedaTesoro
        await Page.ClickAsync("[data-testid=btn-nueva-sesion]");
        await Page.SelectOptionAsync("[data-testid=tipo-sesion]", "BusquedaTesoro");
        await Page.SelectOptionAsync("[data-testid=mision-select]", new[] { new SelectOptionValue { Label = "Misión Demo" } });
        await Page.ClickAsync("[data-testid=btn-crear-sesion]");

        var sesionUrl = Page.Url;
        var sesionId  = sesionUrl.Split('/').Last();

        // 3. Registrar equipo
        await Page.ClickAsync("[data-testid=btn-registrar-equipo]");
        await Page.FillAsync("[data-testid=nombre-equipo]", "Los Detectives");
        await Page.ClickAsync("[data-testid=btn-confirmar-equipo]");

        var codigoAcceso = await Page
            .TextContentAsync("[data-testid=codigo-acceso]");

        codigoAcceso.Should().NotBeNullOrEmpty();

        // 4. Iniciar sesión
        await Page.ClickAsync("[data-testid=btn-iniciar-sesion]");
        await Expect(Page.Locator("[data-testid=estado-sesion]"))
            .ToHaveTextAsync("Activa");

        // 5. Verificar ranking inicial en tiempo real
        var rankingItem = Page.Locator("[data-testid=ranking-item]").First;
        await Expect(rankingItem).ToContainTextAsync("Los Detectives");

        // 6. Finalizar sesión
        await Page.ClickAsync("[data-testid=btn-finalizar-sesion]");
        await Expect(Page.Locator("[data-testid=estado-sesion]"))
            .ToHaveTextAsync("Finalizada");
    }

    [Test]
    public async Task RankingActualizaEnTiempoReal_CuandoSeAplicaPenalizacion()
    {
        // Arrange — sesión activa con dos equipos
        var sesionId = await PrepararSesionActivaConDosParticipantes();

        await Page.GotoAsync($"{BaseUrl}/operador/sesiones/{sesionId}");

        // Act — aplicar penalización al primer equipo
        await Page.ClickAsync("[data-testid=btn-penalizar-0]");
        await Page.FillAsync("[data-testid=motivo-penalizacion]", "Trampa");
        await Page.FillAsync("[data-testid=puntos-penalizacion]", "20");
        await Page.ClickAsync("[data-testid=btn-confirmar-penalizacion]");

        // Assert — ranking actualizado sin recargar
        await Expect(Page.Locator("[data-testid=ranking-item]").Nth(0))
            .ToContainTextAsync("Equipo Beta", new() { Timeout = 3000 });
    }
}
```

---

## 9. Pruebas Frontend Web (Vitest)

```typescript
// umbral-web/src/components/shared/RankingList/RankingList.test.tsx
import { render, screen } from '@testing-library/react';
import { RankingList } from './RankingList';
import type { PosicionRankingDto } from '@/types/sesion.types';

const mockRanking: PosicionRankingDto[] = [
  { posicion: 1, nombreParticipante: 'Alpha', puntajeTotal: 200, tiempoAcumuladoMs: 5000 },
  { posicion: 2, nombreParticipante: 'Beta',  puntajeTotal: 150, tiempoAcumuladoMs: 6000 },
];

describe('RankingList', () => {
  it('renderiza todos los participantes en orden', () => {
    render(<RankingList ranking={mockRanking} />);

    expect(screen.getByText('Alpha')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
    expect(screen.getByText('200 pts')).toBeInTheDocument();
  });

  it('muestra spinner cuando isLoading es true', () => {
    render(<RankingList ranking={[]} loading />);
    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
  });

  it('muestra mensaje cuando ranking está vacío', () => {
    render(<RankingList ranking={[]} />);
    expect(screen.getByText(/sin participantes/i)).toBeInTheDocument();
  });
});
```

```typescript
// umbral-web/src/hooks/useTimer.test.ts
import { renderHook, act } from '@testing-library/react';
import { useTimer } from './useTimer';
import { vi } from 'vitest';

describe('useTimer', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('cuenta hacia cero y llama onExpira', () => {
    const onExpira = vi.fn();
    renderHook(() => useTimer({ duracionMs: 1000, onExpira }));

    act(() => vi.advanceTimersByTime(1100));

    expect(onExpira).toHaveBeenCalledOnce();
  });

  it('porcentaje comienza en 100 y baja', () => {
    const { result } = renderHook(() =>
      useTimer({ duracionMs: 1000 }));

    expect(result.current.porcentaje).toBe(100);

    act(() => vi.advanceTimersByTime(500));

    expect(result.current.porcentaje).toBeCloseTo(50, 0);
  });
});
```

---

## 10. Pruebas Mobile (Jest)

```typescript
// umbral-mobile/src/components/trivia/TriviaQuestion.test.tsx
import { render, fireEvent, screen } from '@testing-library/react-native';
import { TriviaQuestion } from './TriviaQuestion';

const preguntaMock = {
  preguntaId: 'p1',
  enunciado: '¿Cuál es la capital de Francia?',
  opciones: [
    { opcionId: 'o1', texto: 'Madrid' },
    { opcionId: 'o2', texto: 'París' },
  ],
  timerMs: 30000,
  lanzadaEn: new Date().toISOString(),
};

describe('TriviaQuestion', () => {
  it('renderiza el enunciado y las opciones', () => {
    render(
      <TriviaQuestion
        pregunta={preguntaMock}
        opcionSeleccionada={null}
        bloqueado={false}
        tiempoRestanteMs={30000}
        timerTotalMs={30000}
        onSeleccionar={() => {}}
      />
    );

    expect(screen.getByText('¿Cuál es la capital de Francia?'))
      .toBeTruthy();
    expect(screen.getByText('Madrid')).toBeTruthy();
    expect(screen.getByText('París')).toBeTruthy();
  });

  it('llama onSeleccionar al presionar una opción', () => {
    const onSeleccionar = jest.fn();

    render(
      <TriviaQuestion
        pregunta={preguntaMock}
        opcionSeleccionada={null}
        bloqueado={false}
        tiempoRestanteMs={30000}
        timerTotalMs={30000}
        onSeleccionar={onSeleccionar}
      />
    );

    fireEvent.press(screen.getByText('París'));

    expect(onSeleccionar).toHaveBeenCalledWith('o2');
  });

  it('deshabilita opciones cuando bloqueado es true', () => {
    const onSeleccionar = jest.fn();

    render(
      <TriviaQuestion
        pregunta={preguntaMock}
        opcionSeleccionada="o2"
        bloqueado={true}
        tiempoRestanteMs={0}
        timerTotalMs={30000}
        onSeleccionar={onSeleccionar}
      />
    );

    fireEvent.press(screen.getByText('Madrid'));
    expect(onSeleccionar).not.toHaveBeenCalled();
  });
});
```

---

## 11. Cobertura y quality gates

### 11.1 Configuración de cobertura (.NET)

```xml
<!-- Directory.Build.props en /tests -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <Threshold>90</Threshold>
  <ThresholdType>line</ThresholdType>
  <ThresholdStat>total</ThresholdStat>
  <ExcludeByFile>**/Migrations/**/*.cs</ExcludeByFile>
  <ExcludeByAttribute>GeneratedCodeAttribute</ExcludeByAttribute>
</PropertyGroup>
```

> **Nota:** estas props aplican cuando coverlet corre vía MSBuild. La **medición
> oficial del repo** (E1-1, E1-4, CI) usa el recolector VSTest — ver §11.1.b.

### 11.1.b Ejecución de cobertura backend (vigente en Entrega 1)

Artefactos en la raíz del repositorio:

| Artefacto | Rol |
|-----------|-----|
| `coverlet.runsettings` | Exclusiones del recolector `--collect:"XPlat Code Coverage"` |
| `tests/Directory.Build.props` | `coverlet.collector` compartido en los 4 proyectos de test |
| `scripts/run-coverage.ps1` | Orquesta test → XML → reporte HTML + gate opcional |

**Comando canónico (local y defensa):**

```powershell
.\scripts\run-coverage.ps1 -Threshold 90
# Con navegador: .\scripts\run-coverage.ps1 -Open
```

**Requisitos:** Docker en marcha (Testcontainers en Infrastructure/API), .NET 8,
`reportgenerator` (el script lo instala como dotnet global tool si falta).

**Salida:** `coverage/report/index.html` (gitignored; se regenera en cada corrida).

**Exclusiones en `coverlet.runsettings` (alineadas con §12):**

- `ExcludeByFile`: `**/Migrations/**/*.cs` — migraciones EF y `ModelSnapshot`.
- `ExcludeByAttribute`: `GeneratedCodeAttribute`, `CompilerGeneratedAttribute`,
  `ExcludeFromCodeCoverageAttribute` — boilerplate de `record` y artefactos de diseño.

**Convención `[ExcludeFromCodeCoverage]`:** factorías solo de CLI (p. ej.
`UmbralDbContextFactory`, `IDesignTimeDbContextFactory`) y wiring puro no probado
en runtime (p. ej. registro JWT Keycloak en producción — §13.1; los tests usan
`TestAuthHandler` en entorno `Testing`).

**Gate RNF-09:** line coverage **total** backend ≥ 90%. El script con
`-Threshold 90` (PowerShell) o `--threshold 90` (bash) falla con exit code 1 si no
se cumple. **CI:** `.github/workflows/ci.yml` ejecuta el gate en cada push/PR.
Documentación operativa: `docs/entrega-1/iter-e1-01-cobertura-baseline.md`,
`docs/entrega-1/iter-e1-03-ci-coverage.md`.

**Comando manual equivalente:**

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

### 11.2 Configuración de cobertura (Vitest)

```typescript
// umbral-web/vite.config.ts
export default defineConfig({
  test: {
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov', 'html'],
      thresholds: {
        lines: 80,
        functions: 80,
        branches: 75,
      },
      exclude: [
        'src/main.tsx',
        'src/router/**',
        '**/*.d.ts',
      ],
    },
  },
});
```

### 11.3 Quality gates en CI

El pipeline falla si se incumple cualquiera de estas condiciones:

| Gate                              | Umbral  | Bloquea merge |
|-----------------------------------|---------|---------------|
| Cobertura líneas backend          | ≥ 90%   | ✅ Sí         |
| Cobertura líneas frontend web     | ≥ 80%   | ✅ Sí         |
| Build backend sin warnings        | 0 warn  | ✅ Sí         |
| TypeScript sin errores            | 0 error | ✅ Sí         |
| Pruebas unitarias backend         | 100% ✅ | ✅ Sí         |
| Pruebas unitarias frontend        | 100% ✅ | ✅ Sí         |
| Pruebas integración               | 100% ✅ | ✅ Sí         |
| E2E — flujo principal             | 100% ✅ | ✅ Sí         |
| Docker Compose smoke test         | healthy | ✅ Sí         |

---

## 12. Qué NO debe testearse

- Migraciones de EF Core (excluidas de cobertura).
- Factorías de diseño EF (`*DbContextFactory`) — marcar `[ExcludeFromCodeCoverage]`.
- `Program.cs` y configuración de DI.
- Clases generadas automáticamente.
- DTOs y records sin lógica.
- Métodos de extensión triviales (ej. `AddApplication()`).
- Configuraciones de Swagger.

---
## 13. Checklist de calidad por entregable

### Definición de "done" para cualquier HU en cualquier entrega

- [ ] Pruebas unitarias del dominio cubren la lógica de negocio de la HU.
- [ ] Pruebas unitarias del handler cubren: escenario feliz, no encontrado
      y error de dominio.
- [ ] Builder creado o extendido para las entidades nuevas de la HU.
- [ ] Prueba de integración cubre el endpoint principal de la HU.
- [ ] Cobertura backend total se mantiene en ≥ 90% tras agregar la HU.
- [ ] Todos los quality gates del pipeline siguen en verde.

---

### Entrega 1 — criterios transversales

> **Fuente única de alcance y orden de trabajo:** [`docs/entrega-1/PLAN.md`](../../docs/entrega-1/PLAN.md)
> (§4 alcance, §6.1 orden recomendado, §8 Definition of Done).

Resumen alineado al PLAN:

- [ ] Solución .NET con capas Domain / Application / Infrastructure / API y
      **4 proyectos de test** (`Domain`, `Application`, `Infrastructure`, `API`).
- [ ] Pipeline CI corriendo y reportando cobertura (**E1-3**).
- [ ] **Cobertura backend ≥ 90%** medida y cerrada (**E1-1**, **E1-4**).
- [ ] Docker Compose levanta backend + PostgreSQL sin errores.
- [ ] **Autenticación:** **Keycloak (OIDC)** — login real por rol vía realm
      `umbral`. La API valida tokens (resource server). Ver §13.1.
- [x] **Frontend web admin:** CRUD **Misiones** + CRUD **banco Trivia** (**E1-2**).
- [x] **Login OIDC** web (**E1-K4**).
- [ ] **Pantalla operador** mínima: sesión BT por REST, ranking poll (**E1-2b**).
- [ ] Demostrable **403 por rol** (operador no administra catálogo).
- [ ] *(Opcional)* **Mobile** solo login OIDC (**E1-M1**).
- [ ] README y guion de demo (**E1-5**, **E1-6**).

#### Reprogramado a Entrega 2

- [ ] Gameplay completo BT (evidencia QR mobile, penalización UI avanzada).
- [ ] Ranking en tiempo real / **SignalR** (E1 usa solo GET ranking + refresh).
- [ ] Consumers RabbitMQ en demo.
- [ ] **React Native** gameplay (participante participante); E1 solo login opcional.
- [ ] Modo Trivia jugable (HU-32..40).
- [ ] E2E Playwright (`Umbral.E2E.Tests`).

> Nota: **Keycloak (OIDC)** ya entra en **Entrega 1** (§13.1), no es pendiente de E2.

#### §13.1 Autenticación — Keycloak (OIDC), vigente en Entrega 1

La identidad se gestiona con **Keycloak** (a veces el participante lo llama "clickload";
**no** es load testing). El **JWT propio** anterior (`POST /auth/login`, BCrypt,
tabla `usuarios`) **se reemplaza** por Keycloak. Guía: `.cursor/skills/keycloak-auth-skill.md`.

| Aspecto | Decisión Entrega 1 |
|---------|--------------------|
| Flujo | Authorization Code + PKCE (frontend → login del realm) |
| Usuarios / roles | En el realm `umbral` (sin tabla `usuarios` propia) |
| API | Resource server: valida token por `Authority`/JWKS; no emite |
| Tests | Integración con **`TestAuthHandler`** (Keycloak no se levanta en CI); smoke manual |
| Cobertura | El wiring de Keycloak se excluye (configuración, como `Program.cs`) |

Demostrable: login real distinto admin/operador y **403 por rol** en el catálogo.

### Entrega 2 — criterios transversales

- [ ] Modo Trivia completo implementado y demostrable.
- [ ] Cobertura backend ≥ 90% sobre la totalidad del código.
- [ ] Pruebas E2E: flujo completo BusquedaTesoro y Trivia.
- [ ] Reporte HTML de cobertura incluido en el repo.
- [ ] Todos los quality gates del pipeline en verde.
- [ ] Docker Compose levanta el sistema completo sin intervención manual.
- [ ] Documentación de decisiones de prueba en la memoria técnica.

---

## 14. HU por entrega (numeración ERS)

### Entrega 1 — Catálogo admin + autenticación (ver PLAN)

> **Roadmap, orden de trabajo y demo:** [`docs/entrega-1/PLAN.md`](../../docs/entrega-1/PLAN.md).
> El antiguo flujo "BT punta a punta + plan de 10 días" de esta spec **no aplica** a E1.

**HU demostrables en Entrega 1 (frontend + API + persistencia):**

| HU | Descripción | Estado típico |
|----|-------------|---------------|
| — | Login por rol con **Keycloak (OIDC)** | API ✅; front ✅ (**E1-K4**) |
| HU-01..04 | CRUD Misiones | API ✅; front ✅ (**E1-2**) |
| HU-24..31 | CRUD Trivia (banco) | API ✅; front ✅ (**E1-2**) |
| HU-12..16 | Sesión BT operador (mínimo REST) | API ✅; front ⬜ (**E1-2b**) |

**Backend listo, UI E2:** evidencia QR, SignalR, trivia jugable, RabbitMQ demo.
**Mobile E1 opcional:** solo login (**E1-M1**).

**Script de demo:** ver §6 y §8 de `docs/entrega-1/PLAN.md`.

---

### Entrega 2 — Gameplay BT + Trivia jugable + E2E

| HU (ERS) | Descripción (resumen) | Modo |
|----------|----------------------|------|
| HU-09..23, HU-11, HU-17, HU-21 | Completar y demostrar BusquedaTesoro en vivo | BT |
| HU-32..40 | Sesión trivia, rondas, ranking, transición | Trivia |
| HU-22 | Historial de auditoría | Ambos |
| — | E2E Playwright (`Umbral.E2E.Tests`) | Ambos |

