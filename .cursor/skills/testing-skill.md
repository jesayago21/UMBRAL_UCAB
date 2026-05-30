# Skill: Testing — Proyecto UMBRAL

> **Trazabilidad:** `docs/TRAZABILIDAD.md` · **RNF-09**. HUs: `docs/fase-1/TRACKER.md`.

## Propósito
Guía canónica de pruebas para UMBRAL. Cubre: Unit Tests de dominio y handlers
(xUnit + FluentAssertions + NSubstitute), Integration Tests de API (WebApplicationFactory),
E2E de repositorios con Testcontainers PostgreSQL, y pruebas de frontend
(Vitest + React Testing Library para web, Jest + RNTL para mobile).

---

## 1. Pirámide de pruebas en UMBRAL

```
          ╔══════════╗
          ║   E2E    ║  ← Pocos, lentos (Playwright / Detox)
          ╠══════════╣
          ║ Integr.  ║  ← Medios (WebApplicationFactory + Testcontainers)
          ╠══════════╣
          ║  Unit    ║  ← Muchos, rápidos (xUnit / Vitest)
          ╚══════════╝
```

**Objetivo de cobertura mínima por capa:**

| Capa | Tipo | Cobertura mínima |
|------|------|-----------------|
| Domain (Aggregates, VOs) | Unit | 90% |
| Application (Handlers, Validators) | Unit | 85% |
| Infrastructure (Repositorios) | Integration | 70% |
| API (Controllers, Hubs) | Integration | 70% |
| Frontend Web (componentes críticos) | Unit | 75% |
| Frontend Mobile (screens críticos) | Unit | 70% |

---

## 2. Estructura de proyectos de test

```
tests/
├── Umbral.Domain.Tests/
│   ├── EjecucionSesion/
│   │   ├── SesionTests.cs
│   │   ├── EtapaTests.cs
│   │   └── ContextoBusquedaTesoroTests.cs
│   ├── CatalogoTrivia/
│   │   └── PreguntaTriviaTests.cs
│   └── Shared/
│       └── PuntuacionTests.cs
│
├── Umbral.Application.Tests/
│   ├── EjecucionSesion/
│   │   ├── Commands/
│   │   │   ├── CrearSesionCommandHandlerTests.cs
│   │   │   ├── CrearSesionCommandValidatorTests.cs
│   │   │   └── IniciarSesionCommandHandlerTests.cs
│   │   └── Queries/
│   │       └── ObtenerSesionPorIdQueryHandlerTests.cs
│   └── Common/
│       └── Behaviors/
│           └── ValidationBehaviorTests.cs
│
├── Umbral.Infrastructure.Tests/
│   └── Persistence/
│       ├── SesionRepositoryTests.cs       ← Testcontainers
│       └── UmbralDbContextTests.cs
│
└── Umbral.API.Tests/
    ├── Controllers/
    │   └── SesionesControllerTests.cs     ← WebApplicationFactory
    └── Hubs/
        └── SesionHubTests.cs
```

---

## 3. Unit Tests — Dominio

### 3.1 Tests del Aggregate Root Sesion

```csharp
// tests/Umbral.Domain.Tests/EjecucionSesion/SesionTests.cs
namespace Umbral.Domain.Tests.EjecucionSesion;

public sealed class SesionTests
{
    // ── Crear ──────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_DebeRetornarSesionEnBorrador()
    {
        // Arrange
        var nombre = "Sesión Test";
        var tipo = TipoSesion.BusquedaTesoro;
        var fechaInicio = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        var sesion = Sesion.Crear(nombre, tipo, fechaInicio);

        // Assert
        sesion.Nombre.Should().Be(nombre);
        sesion.Tipo.Should().Be(tipo);
        sesion.Estado.Should().Be(EstadoSesion.Borrador);
        sesion.Id.Should().NotBeNull();
        sesion.Equipos.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_ConNombreVacio_DebeRlanzarExcepcion(string? nombre)
    {
        // Act
        var act = () => Sesion.Crear(nombre!, TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DebeRaisearSesionCreadaEvent()
    {
        // Act
        var sesion = Sesion.Crear("Test", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionCreadaEvent>();
    }

    // ── Iniciar ────────────────────────────────────────────────

    [Fact]
    public void Iniciar_ConEquipoRegistrado_DebeActivarSesion()
    {
        // Arrange
        var sesion = CrearSesionConEquipo();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionIniciadaEvent);
    }

    [Fact]
    public void Iniciar_SinEquipos_DebeRlanzarDomainException()
    {
        // Arrange
        var sesion = Sesion.Crear("Test", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos un equipo*");
    }

    [Fact]
    public void Iniciar_SesionYaActiva_DebeRlanzarDomainException()
    {
        // Arrange
        var sesion = CrearSesionConEquipo();
        sesion.Iniciar();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Finalizar ──────────────────────────────────────────────

    [Fact]
    public void Finalizar_SesionActiva_DebeFinalizarConFecha()
    {
        // Arrange
        var sesion = CrearSesionConEquipo();
        sesion.Iniciar();
        sesion.ClearDomainEvents();

        // Act
        sesion.Finalizar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        sesion.FechaFin.Should().NotBeNull();
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionFinalizadaEvent);
    }

    // ── Helpers ────────────────────────────────────────────────

    private static Sesion CrearSesionConEquipo()
    {
        var sesion = Sesion.Crear("Test", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));
        var equipo = Equipo.Crear(sesion.Id, "Equipo Alpha");
        sesion.AgregarEquipo(equipo);
        sesion.ClearDomainEvents();
        return sesion;
    }
}
```

### 3.2 Tests de Value Object

```csharp
// tests/Umbral.Domain.Tests/Shared/PuntuacionTests.cs
public sealed class PuntuacionTests
{
    [Fact]
    public void Crear_ConValorNegativo_DebeRlanzarDomainException()
    {
        var act = () => new Puntuacion(-1);
        act.Should().Throw<DomainException>().WithMessage("*negativa*");
    }

    [Fact]
    public void Sumar_DosValoresPositivos_DebeRetornarSuma()
    {
        var a = new Puntuacion(10);
        var b = new Puntuacion(5);
        a.Sumar(b).Valor.Should().Be(15);
    }

    [Fact]
    public void Cero_DebeRetornarValorCero()
    {
        Puntuacion.Cero.Valor.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Crear_ConValorValido_DebeCrearPuntuacion(int valor)
    {
        var p = new Puntuacion(valor);
        p.Valor.Should().Be(valor);
    }
}
```

---

## 4. Unit Tests — Application (Handlers)

### 4.1 Command Handler con mocks (NSubstitute)

```csharp
// tests/Umbral.Application.Tests/Sesion/Commands/CrearSesionCommandHandlerTests.cs
namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CrearSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly CrearSesionCommandHandler _handler;

    public CrearSesionCommandHandlerTests()
    {
        _handler = new CrearSesionCommandHandler(_sesionRepo, _uow, _publisher);
    }

    [Fact]
    public async Task Handle_ComandoValido_DebeAgregarSesionYRetornarId()
    {
        // Arrange
        var command = new CrearSesionCommand(
            misionId: Guid.NewGuid(),
            operadorId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _sesionRepo.Received(1).AgregarAsync(
            Arg.Any<Sesion>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).GuardarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComandoValido_DebeDespachrarIntegrationEvent()
    {
        // Arrange
        var command = new CrearSesionCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — se publicó el Integration Event después de persistir
        await _publisher.Received().PublicarAsync(
            Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
```

### 4.2 Validator Tests

```csharp
// tests/Umbral.Application.Tests/EjecucionSesion/Commands/CrearSesionCommandValidatorTests.cs
public sealed class CrearSesionCommandValidatorTests
{
    private readonly CrearSesionCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoCompleto_DebeSerValido()
    {
        var command = new CrearSesionCommand(
            "Sesión válida", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_NombreVacio_DebeRlanzarError()
    {
        var command = new CrearSesionCommand(
            "", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact]
    public void Validar_FechaEnElPasado_DebeRlanzarError()
    {
        var command = new CrearSesionCommand(
            "Test", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(-1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FechaInicio);
    }
}
```

---

## 5. Integration Tests — Repositorios con Testcontainers

```csharp
// tests/Umbral.Infrastructure.Tests/Persistence/SesionRepositoryTests.cs
namespace Umbral.Infrastructure.Tests.Persistence;

[Collection("PostgreSQL")]
public sealed class SesionRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("umbral_test")
        .WithUsername("test_user")
        .WithPassword("test_pass")
        .Build();

    private UmbralDbContext _context = null!;
    private SesionRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<UmbralDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new UmbralDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new SesionRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task AgregarAsync_SesionValida_DebePersistrYRecuperar()
    {
        // Arrange
        var sesion = Sesion.Crear("Test", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));
        sesion.ClearDomainEvents();

        // Act
        await _repository.AgregarAsync(sesion);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();  // limpiar cache

        var recuperada = await _repository.ObtenerPorIdAsync(sesion.Id);

        // Assert
        recuperada.Should().NotBeNull();
        recuperada!.Nombre.Should().Be("Test");
        recuperada.Tipo.Should().Be(TipoSesion.Trivia);
        recuperada.Estado.Should().Be(EstadoSesion.Borrador);
    }

    [Fact]
    public async Task ListarActivasAsync_SoloDevuelveSesionesActivas()
    {
        // Arrange — crear sesiones en distintos estados
        var sesionBorrador = Sesion.Crear("Borrador", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));
        var sesionActiva = Sesion.Crear("Activa", TipoSesion.BusquedaTesoro, DateTimeOffset.UtcNow.AddDays(1));
        var equipo = Equipo.Crear(sesionActiva.Id, "Equipo");
        sesionActiva.AgregarEquipo(equipo);
        sesionActiva.Iniciar();

        foreach (var s in new[] { sesionBorrador, sesionActiva })
        {
            s.ClearDomainEvents();
            await _repository.AgregarAsync(s);
        }
        await _context.SaveChangesAsync();

        // Act
        var activas = await _repository.ListarActivasAsync();

        // Assert
        activas.Should().HaveCount(1);
        activas.Single().Nombre.Should().Be("Activa");
    }
}
```

---

## 6. Integration Tests — API con WebApplicationFactory

```csharp
// tests/Umbral.API.Tests/Controllers/SesionesControllerTests.cs
namespace Umbral.API.Tests.Controllers;

public sealed class SesionesControllerTests : IClassFixture<UmbralWebAppFactory>
{
    private readonly HttpClient _client;

    public SesionesControllerTests(UmbralWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_sesiones_ComandoValido_DebeRetornar201()
    {
        // Arrange
        var request = new
        {
            Nombre = "Sesión API Test",
            Tipo = "Trivia",
            FechaInicio = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sesiones", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CrearSesionResult>();
        result!.SesionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_sesiones_NombreVacio_DebeRetornar422()
    {
        var request = new { Nombre = "", Tipo = "Trivia", FechaInicio = DateTimeOffset.UtcNow.AddDays(1) };

        var response = await _client.PostAsJsonAsync("/api/sesiones", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GET_sesiones_id_SesionInexistente_DebeRetornar404()
    {
        var response = await _client.GetAsync($"/api/sesiones/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

// tests/Umbral.API.Tests/UmbralWebAppFactory.cs
public sealed class UmbralWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public new async Task DisposeAsync() => await _postgres.StopAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Reemplazar DbContext con la BD de test
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<UmbralDbContext>));

            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<UmbralDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Aplicar migraciones
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
            db.Database.Migrate();
        });
    }
}
```

---

## 7. Frontend Web — Vitest + React Testing Library

```typescript
// tests/web/components/SesionCard.test.tsx
import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { SesionCard } from "@/components/SesionCard";

describe("SesionCard", () => {
  const sesionMock = {
    id: "123",
    nombre: "Sesión Test",
    tipo: "Trivia" as const,
    estado: "Borrador" as const,
    totalEquipos: 3,
  };

  it("renderiza el nombre y tipo de sesión", () => {
    render(<SesionCard sesion={sesionMock} onIniciar={vi.fn()} />);

    expect(screen.getByText("Sesión Test")).toBeInTheDocument();
    expect(screen.getByText("Trivia")).toBeInTheDocument();
  });

  it("llama onIniciar al hacer clic en el botón", () => {
    const onIniciar = vi.fn();
    render(<SesionCard sesion={sesionMock} onIniciar={onIniciar} />);

    fireEvent.click(screen.getByRole("button", { name: /iniciar/i }));

    expect(onIniciar).toHaveBeenCalledWith("123");
  });

  it("deshabilita el botón iniciar si estado no es Borrador", () => {
    const sesionActiva = { ...sesionMock, estado: "Activa" as const };
    render(<SesionCard sesion={sesionActiva} onIniciar={vi.fn()} />);

    expect(screen.getByRole("button", { name: /iniciar/i })).toBeDisabled();
  });
});
```

```typescript
// tests/web/hooks/useSesionesQuery.test.ts
import { renderHook, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { useSesionesActivas } from "@/hooks/useSesionesActivas";
import { server } from "../mocks/server";
import { http, HttpResponse } from "msw";

describe("useSesionesActivas", () => {
  it("retorna sesiones activas del servidor", async () => {
    server.use(
      http.get("/api/sesiones/activas", () =>
        HttpResponse.json([{ id: "1", nombre: "Test", estado: "Activa" }])
      )
    );

    const { result } = renderHook(() => useSesionesActivas());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toHaveLength(1);
    expect(result.current.data![0].nombre).toBe("Test");
  });

  it("maneja error del servidor", async () => {
    server.use(
      http.get("/api/sesiones/activas", () =>
        HttpResponse.json({ error: "Error" }, { status: 500 })
      )
    );

    const { result } = renderHook(() => useSesionesActivas());

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});
```

---

## 8. Frontend Mobile — Jest + React Native Testing Library

```typescript
// tests/mobile/screens/SesionScreen.test.tsx
import React from "react";
import { render, screen, fireEvent, waitFor } from "@testing-library/react-native";
import { SesionScreen } from "@/screens/SesionScreen";

describe("SesionScreen", () => {
  it("muestra mensaje de espera mientras carga", () => {
    render(
      <SesionScreen
        sesionId="123"
        equipoId="456"
        isLoading={true}
        etapaActual={null}
      />
    );

    expect(screen.getByText(/esperando/i)).toBeTruthy();
  });

  it("muestra pregunta de trivia cuando etapa está activa", async () => {
    const etapaMock = {
      id: "e1",
      orden: 1,
      descripcion: "¿Cuál es la capital de Venezuela?",
      estado: "Activa",
    };

    render(
      <SesionScreen
        sesionId="123"
        equipoId="456"
        isLoading={false}
        etapaActual={etapaMock}
      />
    );

    expect(screen.getByText("¿Cuál es la capital de Venezuela?")).toBeTruthy();
  });
});
```

---

## 9. Packages NuGet requeridos (backend tests)

```xml
<!-- tests/Umbral.Domain.Tests/Umbral.Domain.Tests.csproj -->
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />

<!-- tests/Umbral.Application.Tests/ -->
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="FluentValidation.TestHelper" Version="11.*" />

<!-- tests/Umbral.Infrastructure.Tests/ + Umbral.API.Tests/ -->
<PackageReference Include="Testcontainers.PostgreSql" Version="4.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.*" />
```

> **Cobertura:** ver `.cursor/specs/umbral-quality-spec.md` §11.1.b (`coverlet.runsettings`,
> `scripts/run-coverage.ps1`).

---

## 10. Packages npm requeridos (frontend tests)

```json
// package.json (web)
{
  "devDependencies": {
    "vitest": "^1.*",
    "@testing-library/react": "^14.*",
    "@testing-library/jest-dom": "^6.*",
    "@testing-library/user-event": "^14.*",
    "msw": "^2.*",
    "jsdom": "^24.*"
  }
}

// package.json (mobile)
{
  "devDependencies": {
    "jest": "^29.*",
    "@testing-library/react-native": "^12.*",
    "jest-expo": "~51.*"
  }
}
```

---

## 11. Reglas de testing — UMBRAL

| # | Regla | Motivo |
|---|-------|--------|
| 1 | Los tests de dominio **no mockan nada**; prueban solo el comportamiento del AR. | El dominio es puro y no tiene dependencias externas. |
| 2 | Los tests de handlers **mockan repositorios e interfaces** con NSubstitute. | Aislar el handler de la infraestructura real. |
| 3 | Los tests de repositorios usan **Testcontainers** (PostgreSQL real). | Valida queries, conversores y migraciones reales. |
| 4 | Los tests de API usan **WebApplicationFactory** con BD de Testcontainers. | Prueba el stack completo sin infraestructura externa. |
| 5 | Cada test sigue el patrón **Arrange / Act / Assert** con comentarios explícitos. | Legibilidad y mantenibilidad del suite. |
| 6 | Los tests de dominio verifican **Domain Events** levantados por el AR. | Asegura que el AR comunica cambios correctamente. |
| 7 | Los nombres de tests siguen `MetodoOAccion_Condicion_ResultadoEsperado`. | Documentación ejecutable. |
| 8 | Los tests de Validator usan `FluentValidation.TestHelper` (`.TestValidate()`). | API idiomática para validación con FluentValidation. |
| 9 | Los tests de frontend usan **MSW** para interceptar llamadas HTTP. | No dependen de backend real; tests deterministas. |
| 10 | `_context.ChangeTracker.Clear()` después de persistir en tests de repositorio. | Evita que EF sirva desde caché en lugar de BD real. |

---

## 12. Checklist al crear tests para una nueva feature

```
□ ¿Hay tests de dominio para el AR o VO creado/modificado?
□ ¿Se testea el happy path Y los casos de error (DomainException)?
□ ¿Se verifica que el AR levanta los Domain Events correctos?
□ ¿Hay tests del Command Handler con mocks de repositorio e IUnitOfWork?
□ ¿Hay tests del Validator con TestValidate()?
□ ¿Hay integration test del repositorio con Testcontainers?
□ ¿Hay integration test del endpoint de API?
□ ¿Los tests de frontend cubren el componente/screen afectado?
□ ¿Los nombres de tests siguen el patrón Método_Condición_Resultado?
□ ¿Se ejecuta el suite completo sin fallos antes de hacer PR?
```

---

## 13. Anti-patrones a evitar

```csharp
// ❌ MALO — mockear el dominio en tests de dominio
var sesion = Substitute.For<Sesion>();  // ← nunca mockear entidades de dominio

// ✅ BUENO — usar el constructor real del dominio
var sesion = Sesion.Crear("Test", TipoSesion.Trivia, DateTimeOffset.UtcNow.AddDays(1));

// ❌ MALO — test sin Assert
[Fact]
public async Task Handle_DebeEjecutar()
{
    await _handler.Handle(command, CancellationToken.None);
    // ← sin Assert: el test siempre pasa
}

// ✅ BUENO — Assert explícito
[Fact]
public async Task Handle_ComandoValido_DebeAgregarSesion()
{
    await _handler.Handle(command, CancellationToken.None);
    await _sesionRepo.Received(1).AgregarAsync(Arg.Any<Sesion>(), Arg.Any<CancellationToken>());
}

// ❌ MALO — test frágil por dependencia de orden
[Fact]
public async Task Repositorio_AlAgregar_SesionDebeEstarEnBD()
{
    // Asume que hay datos de tests anteriores en la BD
    var sesiones = await _repository.ListarActivasAsync();
    sesiones.Should().HaveCount(3);  // ← número hardcodeado
}

// ✅ BUENO — cada test crea sus propios datos
[Fact]
public async Task ListarActivasAsync_ConUnaSesionActiva_DebeRetornarUna()
{
    var sesion = CrearYPersistirSesionActiva();
    var resultado = await _repository.ListarActivasAsync();
    resultado.Should().ContainSingle(s => s.Id == sesion.Id);
}
```

---

## 14. Guía de uso para Cursor AI

Cuando el usuario pida crear tests en UMBRAL:

1. **Identificar qué se está probando** → ¿dominio, handler, repositorio, API, frontend?
2. **Seleccionar la plantilla** de la sección correspondiente (3, 4, 5, 6, 7 u 8).
3. **Verificar que el test tiene Arrange/Act/Assert** con comentarios.
4. **Para dominio**: no mockear, verificar Domain Events levantados.
5. **Para handlers**: mockear con NSubstitute, verificar llamadas a repositorio y publicación de eventos.
6. **Para repositorios**: usar Testcontainers, llamar `ChangeTracker.Clear()` antes de verificar.
7. **Para API**: usar WebApplicationFactory con BD de Testcontainers.
8. **Nombre del test**: `Método_Condición_ResultadoEsperado`.
9. **Correr checklist** de la sección 12 y advertir si hay anti-patrones de la sección 13.
