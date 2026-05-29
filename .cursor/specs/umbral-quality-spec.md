# UMBRAL — Especificación de Calidad y Pruebas

> **HU canónicas:** numeración **HU-01…HU-40** del ERS (`docs/TRAZABILIDAD.md`). La §14 usa esa numeración; el seguimiento de Fase 1 está en `docs/fase-1/TRACKER.md`.

## 1. Estrategia general de pruebas

UMBRAL adopta una pirámide de pruebas con cuatro niveles:
                ┌─────────────┐
                │    E2E      │  ← Playwright (flujos completos)
               ┌┴─────────────┴┐
               │  Integración  │  ← Testcontainers (BD + API real)
              ┌┴───────────────┴┐
              │    Aplicación   │  ← xUnit + Moq (handlers aislados)
             ┌┴─────────────────┴┐
             │      Dominio      │  ← xUnit puro (sin mocks)
             └───────────────────┘

| Nivel         | Herramienta               | Cobertura objetivo | Velocidad |
|---------------|---------------------------|--------------------|-----------|
| Dominio       | xUnit + FluentAssertions  | 100%               | < 1s      |
| Aplicación    | xUnit + Moq               | ≥ 90%              | < 5s      |
| Integración   | xUnit + Testcontainers    | Flujos críticos    | < 60s     |
| E2E           | Playwright                | Flujo principal    | < 3 min   |

**Meta académica obligatoria:** cobertura total del backend ≥ 90% (**RNF-09**). Concurrencia trivia: **RNF-13**.

---

## 2. Proyectos de prueba

> **Estado real del repo (vigente):** los proyectos de integración se
> implementaron como `Umbral.Infrastructure.Tests` (persistencia con
> Testcontainers) y `Umbral.API.Tests` (API real con `WebApplicationFactory`
> + Testcontainers). **No existe** `Umbral.E2E.Tests`; las pruebas E2E con
> Playwright se reprograman a **Entrega 2**. La estructura ideal de abajo se
> mantiene como referencia objetivo.

tests/
├── Umbral.Domain.Tests/          → pruebas de agregados, VOs y domain services
├── Umbral.Application.Tests/     → pruebas de handlers y behaviors
├── Umbral.Infrastructure.Tests/  → persistencia con PostgreSQL real (Testcontainers)
├── Umbral.API.Tests/             → API real (WebApplicationFactory + Testcontainers)
└── Umbral.E2E.Tests/             → (Entrega 2) end-to-end con Playwright

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
<PackageReference Include="Moq" />
<PackageReference Include="FluentAssertions" />

<!-- Umbral.Integration.Tests.csproj -->
<ProjectReference Include="../src/backend/Umbral.API/..." />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
<PackageReference Include="Testcontainers.PostgreSql" />
<PackageReference Include="FluentAssertions" />

<!-- Umbral.E2E.Tests.csproj -->
<PackageReference Include="Microsoft.Playwright" />
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
            .ConEquipo("Equipo Alpha")
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
    public void Iniciar_SinEquiposRegistrados_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinEquipos()
            .Build();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos un equipo*");
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

    // ── Equipos ───────────────────────────────────────────────────

    [Fact]
    public void RegistrarEquipo_CuandoNombreUnico_AgregaEquipo()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion).Build();

        // Act
        var equipo = sesion.RegistrarEquipo("Los Sabuesos");

        // Assert
        sesion.Equipos.Should().ContainSingle();
        equipo.Nombre.Valor.Should().Be("Los Sabuesos");
        equipo.CodigoAcceso.Should().NotBeNull();
    }

    [Fact]
    public void RegistrarEquipo_CuandoNombreDuplicado_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConEquipo("Los Sabuesos")
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
            .Activa().ConEquipo("Alpha").Build();
        var equipo     = sesion.Equipos.First();
        var penalizacion = new Penalizacion(
            10, "Trampa detectada", new UsuarioId(Guid.NewGuid()));

        equipo.SumarPuntaje(50);

        // Act
        sesion.AplicarPenalizacion(equipo.EquipoId, penalizacion);

        // Assert
        equipo.PuntajeTotal.Valor.Should().Be(40);
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
    private List<string>   _equipos    = [];
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
        _equipos.Add("Equipo Default");
        return this;
    }

    public SesionBuilder ConEquipo(string nombre)
    {
        _equipos.Add(nombre);
        return this;
    }

    public SesionBuilder SinEquipos()
    {
        _equipos = [];
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

        foreach (var nombre in _equipos)
            sesion.RegistrarEquipo(nombre);

        sesion.ClearDomainEvents();
        return sesion;
    }
}
```

---

## 6. Pruebas de Aplicación (Handlers)

Los handlers se prueban con repositorios falsos (Moq).
Nunca con base de datos real.

```csharp
public class CrearSesionBusquedaTesoroCommandHandlerTests
{
    private readonly Mock<ISesionRepository>  _sesionRepo  = new();
    private readonly Mock<IMisionRepository>  _misionRepo  = new();
    private readonly Mock<IEventPublisher>    _publisher   = new();
    private readonly CrearSesionBusquedaTesoroCommandHandler _sut;

    public CrearSesionBusquedaTesoroCommandHandlerTests()
    {
        _sut = new CrearSesionBusquedaTesoroCommandHandler(
            _sesionRepo.Object,
            _misionRepo.Object,
            _publisher.Object);
    }

    [Fact]
    public async Task Handle_CuandoMisionActiva_CreaSesionYPublicaEvento()
    {
        // Arrange
        var misionId = Guid.NewGuid();
        var mision   = MisionBuilder.Activa().Build();

        _misionRepo
            .Setup(r => r.FindByIdAsync(
                It.Is<MisionId>(id => id.Valor == misionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mision);

        _sesionRepo
            .Setup(r => r.SaveAsync(
                It.IsAny<Sesion>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CrearSesionBusquedaTesoroCommand(
            misionId, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeEmpty();

        _sesionRepo.Verify(r => r.SaveAsync(
            It.IsAny<Sesion>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _publisher.Verify(p => p.PublishBatchAsync(
            It.Is<IReadOnlyList<IDomainEvent>>(
                eventos => eventos.Any(e => e is SesionCreada)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoMisionNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _misionRepo
            .Setup(r => r.FindByIdAsync(
                It.IsAny<MisionId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mision?)null);

        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _sesionRepo.Verify(r => r.SaveAsync(
            It.IsAny<Sesion>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoMisionInactiva_LanzaDomainException()
    {
        // Arrange
        var mision = MisionBuilder.Inactiva().Build();

        _misionRepo
            .Setup(r => r.FindByIdAsync(
                It.IsAny<MisionId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mision);

        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
```

---

## 7. Pruebas de Integración

Usan una base de datos PostgreSQL real levantada con Testcontainers.
Se ejecutan en CI dentro del job de backend.

```csharp
// tests/Umbral.Integration.Tests/SesionIntegrationTests.cs
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
    public async Task GetRanking_CuandoSesionConEquipos_RetornaOrdenCorrecto()
    {
        // Arrange
        var sesionId = await CrearSesionConEquiposAsync(
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

        ranking![0].NombreEquipo.Should().Be("Beta");
        ranking[1].NombreEquipo.Should().Be("Alpha");
        ranking[2].NombreEquipo.Should().Be("Gamma");
    }

    private async Task<Guid> CrearMisionActivaAsync() { /* helper */ }
    private async Task<Guid> CrearSesionConEquiposAsync(
        params (string nombre, int puntaje)[] equipos) { /* helper */ }
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
        var sesionId = await PrepararSesionActivaConDosEquipos();

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
  { posicion: 1, nombreEquipo: 'Alpha', puntajeTotal: 200, tiempoAcumuladoMs: 5000 },
  { posicion: 2, nombreEquipo: 'Beta',  puntajeTotal: 150, tiempoAcumuladoMs: 6000 },
];

describe('RankingList', () => {
  it('renderiza todos los equipos en orden', () => {
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
    expect(screen.getByText(/sin equipos/i)).toBeInTheDocument();
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

> **Alcance reducido (vigente):** Entrega 1 demuestra **comunicación
> frontend ↔ backend ↔ persistencia** con **cobertura backend ≥ 90%**,
> usando **CRUD de Misiones + Login** como HU mínimas. El detalle de
> control está en `docs/entrega-1/PLAN.md`. Los puntos de "punta a punta
> con SignalR/RabbitMQ/mobile" se reprograman a **Entrega 2** (ver §7 del PLAN).

- [ ] Solución .NET con 4 proyectos y dependencias correctas verificadas
      por compilación.
- [ ] Pipeline CI corriendo y reportando cobertura.
- [ ] **Cobertura backend ≥ 90%** sobre el código implementado.
- [ ] Docker Compose levanta backend + PostgreSQL sin errores.
- [ ] **Frontend web mínimo** hace login real + CRUD de Misiones contra la API.
- [ ] Demostrable **403 por rol** (operador no crea misiones).
- [ ] README con instrucciones para levantar el entorno local.

#### Reprogramado a Entrega 2 (antes en Entrega 1)

- [ ] Flujo completo BusquedaTesoro demostrable de punta a punta.
- [ ] WebSocket actualiza ranking sin recargar la página.
- [ ] Al menos 2 consumers de RabbitMQ operativos.
- [ ] React Native muestra flujo mínimo del equipo participante.

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

### Entrega 1 — Flujo BusquedaTesoro conectado de punta a punta

> **Nota de alcance (vigente):** para la entrega académica, el demo se reduce
> a **Login + CRUD de Misiones** (las 3 capas conectadas). Las HU de gameplay,
> ranking en vivo, mobile y RabbitMQ de esta tabla quedan como backend listo
> pero **no demostrado** en Entrega 1; pasan a Entrega 2. Ver `docs/entrega-1/PLAN.md`.

| HU (ERS) | Descripción                                      | Capa                  | Modo | Fase dominio |
|----------|--------------------------------------------------|-----------------------|------|--------------|
| HU-01    | Crear / activar misión                           | Web Admin             | BT   | 🔶 iter soporte |
| HU-05    | Configurar nodos (etapas)                        | Web Admin             | BT   | 🔶 iter soporte |
| HU-06    | Registrar pistas en etapa                        | Web Admin             | BT   | 🔶 iter soporte |
| HU-12    | Crear sesión BusquedaTesoro desde misión activa  | Web Operador          | BT   | ✅ iter-01 |
| HU-13    | Inscripción de equipos                           | Web Operador          | BT   | ✅ iter-02 |
| HU-14    | Control de inicio de sesión                      | Web Operador          | BT   | ✅ iter-03 |
| HU-15    | Pausa y reanudación                              | Web Operador          | BT   | ✅ iter-03 |
| HU-16    | Aplicar penalización con motivo                  | Web Operador          | BT   | ✅ iter-04 |
| HU-11    | Equipo ve pistas habilitadas                     | React Native          | BT   | — |
| HU-17    | Tablero equipo en tiempo real                    | React Native          | BT   | — |
| HU-18    | Enviar evidencia QR                              | React Native          | BT   | ⬜ iter-05 |
| HU-19    | Validar ganador único + puntaje                  | Backend               | BT   | ⬜ iter-05/06 |
| HU-20    | Transición automática de etapa                   | Backend               | BT   | ⬜ iter-06 |
| HU-21    | Ranking en tiempo real                           | Web + React Native    | BT   | — |
| HU-23    | Cerrar sesión / reporte final                    | Web Operador          | BT   | 🔶 iter-07 |
| —        | Liberar pistas manualmente (RF-15)               | Web Operador          | BT   | — |
| —        | Consumer RabbitMQ recálculo (RF-19)              | Backend async         | BT   | — |

**Flujo demostrable en Entrega 1:**

Admin crea misión con etapas y pistas (Web)
↓
Operador crea sesión BT + registra equipo (Web)
↓
Operador inicia sesión (Web)
↓
Equipo se une con código de acceso (React Native)
↓
Equipo ve pistas + escanea/ingresa código QR (React Native)
↓
Backend valida evidencia → publica evento en RabbitMQ
↓
Consumer recalcula puntaje + Auditoría registra evento
↓
SignalR notifica a todos → ranking actualiza en vivo (Web + Native)
↓
Operador aplica penalización → ranking se reordena en tiempo real
↓
Operador finaliza sesión → estado final con ranking definitivo


**Script de demo para el profesor:**

[Web Admin]      Mostrar misión creada con etapas y pistas configuradas
[Web Operador]   Crear sesión BT → código de acceso generado
[React Native]   Equipo se une con el código → ve pantalla de juego
[Web Operador]   Iniciar sesión → estado cambia a Activa en tiempo real
[React Native]   Equipo ve pistas → ingresa/escanea QR
[Terminal/Logs]  Mostrar evento publicado en RabbitMQ + consumer procesando
[Web Operador]   Ranking actualiza automáticamente sin recargar (WebSocket)
[Web Operador]   Aplicar penalización → ranking se reordena en vivo
[Web Operador]   Finalizar sesión → estado Finalizada con ranking definitivo
[CI/Terminal]   Mostrar pruebas corriendo + reporte cobertura ≥ 90%

---

### Entrega 2 — Modo Trivia + completar BusquedaTesoro

| HU (ERS) | Descripción (resumen)                                | Capa               | Modo   |
|----------|------------------------------------------------------|--------------------|--------|
| HU-24–27 | Banco de preguntas (CRUD)                            | Web Admin          | Trivia | †
| HU-28–31 | Categorías de trivia                                 | Web Admin          | Trivia | †

> † **CRUD de Trivia (HU-24..31) adelantado a Entrega 1 como contingencia**
> (backend, sin gameplay). Ver `docs/entrega-1/PLAN.md §5.1`. El **modo Trivia
> jugable (HU-32..40)** permanece en Entrega 2.
| HU-32    | Crear sesión Trivia                                  | Web Operador       | Trivia |
| HU-33    | Sala de espera (equipos conectados)                  | Web Operador       | Trivia |
| HU-34–35 | Secuencia y envío de respuestas                    | Native + Backend   | Trivia |
| HU-36–39 | Procesamiento async, puntaje, ranking, transición  | Backend            | Trivia |
| HU-40    | Desempate por timestamp servidor                     | Backend            | Trivia |
| HU-09    | Liberación automática de pistas por tiempo (BT)      | Backend            | BT     |
| HU-22    | Historial de auditoría                               | Web Admin          | Ambos  |
| —        | E2E flujo BT completo                                | Playwright         | BT     |
| —        | E2E flujo Trivia completo                            | Playwright         | Trivia |

---

## 15. Plan de 10 días — Entrega 1

| Día     | Foco                          | Entregable clave                              |
|---------|-------------------------------|-----------------------------------------------|
| 1 – 2   | Dominio + pruebas             | Agregados, VOs, domain tests ≥ 90%            |
| 3 – 4   | Application + Infrastructure  | Handlers, validators, EF Core, repositorios   |
| 5 – 6   | API + RabbitMQ + SignalR      | Controllers, JWT, hubs, 2 consumers           |
| 7 – 8   | Frontend Web                  | Admin + Operador conectados con API real       |
| 9       | React Native mínimo           | Join + Dashboard BT + WebSocket               |
| 10      | Cierre y demo                 | CI verde, docker compose up, ensayo demo      |

### División sugerida 
Persona A (Cursor)  → genera código, pruebas, configuraciones
Persona B (Tú)      → dirige, revisa, corre, integra y corrige



