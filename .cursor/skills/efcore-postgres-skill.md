# Skill: EF Core + PostgreSQL — Proyecto UMBRAL

> **Trazabilidad:** `docs/TRAZABILIDAD.md` · **RNF-02**, **RNF-13**.

## Propósito
Guía canónica para configurar Entity Framework Core con PostgreSQL en el
monolito hexagonal de UMBRAL. Cubre: DbContext, configuraciones Fluent API,
strongly-typed IDs, migraciones, Unit of Work, repositorios y convenciones
de nombrado para los tres Bounded Contexts.

---

## 1. Stack de persistencia

```
Umbral.Domain        → interfaces de repositorio (puertos)
Umbral.Application   → IUmbralDbContext (lectura), IUnitOfWork
Umbral.Infrastructure
  └── Persistence/
      ├── UmbralDbContext.cs
      ├── Configurations/          ← IEntityTypeConfiguration<T> por entidad
      │   ├── EjecucionSesion/
      │   ├── CatalogoBusquedaTesoro/
      │   └── CatalogoTrivia/
      ├── Repositories/            ← implementaciones de ISesionRepository, etc.
      ├── Migrations/              ← dotnet ef migrations add ...
      └── ValueConverters/         ← strongly-typed ID converters
```

**Paquetes NuGet requeridos:**

```xml
<!-- Umbral.Infrastructure.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.*" PrivateAssets="all" />
<PackageReference Include="EFCore.NamingConventions" Version="8.*" />
```

---

## 2. DbContext principal

```csharp
// Infrastructure/Persistence/UmbralDbContext.cs
namespace Umbral.Infrastructure.Persistence;

public sealed class UmbralDbContext : DbContext, IUmbralDbContext
{
    public UmbralDbContext(DbContextOptions<UmbralDbContext> options)
        : base(options) { }

    // ── EjecucionSesion ────────────────────────────────────────
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<Etapa> Etapas => Set<Etapa>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Participante> Participantes => Set<Participante>();
    public DbSet<ContextoBusquedaTesoro> ContextosBusquedaTesoro => Set<ContextoBusquedaTesoro>();
    public DbSet<ContextoTrivia> ContextosTrivia => Set<ContextoTrivia>();

    // ── CatalogoBusquedaTesoro ─────────────────────────────────
    public DbSet<PistaBusqueda> PistasBusqueda => Set<PistaBusqueda>();

    // ── CatalogoTrivia ─────────────────────────────────────────
    public DbSet<PreguntaTrivia> PreguntasTrivia => Set<PreguntaTrivia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica TODAS las IEntityTypeConfiguration del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UmbralDbContext).Assembly);

        // Convención global: snake_case en PostgreSQL
        modelBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Registro global de converters para strongly-typed IDs
        configurationBuilder
            .Properties<SesionId>()
            .HaveConversion<SesionIdConverter>();

        configurationBuilder
            .Properties<EtapaId>()
            .HaveConversion<EtapaIdConverter>();

        configurationBuilder
            .Properties<EquipoId>()
            .HaveConversion<EquipoIdConverter>();

        configurationBuilder
            .Properties<ParticipanteId>()
            .HaveConversion<ParticipanteIdConverter>();
    }
}

// Interfaz de lectura para queries (Application layer)
public interface IUmbralDbContext
{
    DbSet<Sesion> Sesiones { get; }
    DbSet<Etapa> Etapas { get; }
    DbSet<Equipo> Equipos { get; }
    DbSet<Participante> Participantes { get; }
    DbSet<ContextoBusquedaTesoro> ContextosBusquedaTesoro { get; }
    DbSet<ContextoTrivia> ContextosTrivia { get; }
    DbSet<PistaBusqueda> PistasBusqueda { get; }
    DbSet<PreguntaTrivia> PreguntasTrivia { get; }
}
```

---

## 3. Value Converters para Strongly-Typed IDs

```csharp
// Infrastructure/Persistence/ValueConverters/SesionIdConverter.cs
namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class SesionIdConverter
    : ValueConverter<SesionId, Guid>
{
    public SesionIdConverter()
        : base(
            id => id.Value,           // dominio → base de datos
            value => new SesionId(value))  // base de datos → dominio
    { }
}

// Repetir para: EtapaIdConverter, EquipoIdConverter, ParticipanteIdConverter,
// PistaBusquedaIdConverter, PreguntaTriviaIdConverter, etc.
```

---

## 4. Configuraciones Fluent API

### 4.1 Configuración de Sesion (Aggregate Root)

```csharp
// Infrastructure/Persistence/Configurations/EjecucionSesion/SesionConfiguration.cs
namespace Umbral.Infrastructure.Persistence.Configurations.EjecucionSesion;

public sealed class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("sesiones", "ejecucion_sesion");

        // ── Clave primaria ─────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion<SesionIdConverter>()
            .HasColumnName("id");

        // ── Propiedades escalares ──────────────────────────────
        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("nombre");

        builder.Property(s => s.Tipo)
            .IsRequired()
            .HasConversion<string>()   // almacena "BusquedaTesoro" | "Trivia"
            .HasMaxLength(30)
            .HasColumnName("tipo");

        builder.Property(s => s.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("estado");

        builder.Property(s => s.FechaInicio)
            .IsRequired()
            .HasColumnName("fecha_inicio");

        builder.Property(s => s.FechaFin)
            .HasColumnName("fecha_fin");

        // ── Colecciones (entidades hijas) ──────────────────────
        builder.HasMany(s => s.Etapas)
            .WithOne()
            .HasForeignKey(e => e.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Equipos)
            .WithOne()
            .HasForeignKey(e => e.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Contextos opcionales (0..1) ────────────────────────
        builder.HasOne(s => s.ContextoBusquedaTesoro)
            .WithOne()
            .HasForeignKey<ContextoBusquedaTesoro>(c => c.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ContextoTrivia)
            .WithOne()
            .HasForeignKey<ContextoTrivia>(c => c.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Ignorar colecciones de eventos de dominio ──────────
        builder.Ignore(s => s.DomainEvents);

        // ── Índices ────────────────────────────────────────────
        builder.HasIndex(s => s.Estado)
            .HasDatabaseName("ix_sesiones_estado");

        builder.HasIndex(s => s.FechaInicio)
            .HasDatabaseName("ix_sesiones_fecha_inicio");
    }
}
```

### 4.2 Configuración de Etapa

```csharp
// Configurations/EjecucionSesion/EtapaConfiguration.cs
public sealed class EtapaConfiguration : IEntityTypeConfiguration<Etapa>
{
    public void Configure(EntityTypeBuilder<Etapa> builder)
    {
        builder.ToTable("etapas", "ejecucion_sesion");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion<EtapaIdConverter>();

        builder.Property(e => e.SesionId)
            .HasConversion<SesionIdConverter>()
            .IsRequired();

        builder.Property(e => e.Orden)
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        // Restricción: orden único por sesión
        builder.HasIndex(e => new { e.SesionId, e.Orden })
            .IsUnique()
            .HasDatabaseName("ix_etapas_sesion_orden");
    }
}
```

### 4.3 Configuración de ContextoBusquedaTesoro

```csharp
// Configurations/EjecucionSesion/ContextoBusquedaTesoroConfiguration.cs
public sealed class ContextoBusquedaTesoroConfiguration
    : IEntityTypeConfiguration<ContextoBusquedaTesoro>
{
    public void Configure(EntityTypeBuilder<ContextoBusquedaTesoro> builder)
    {
        builder.ToTable("contextos_busqueda_tesoro", "ejecucion_sesion");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SesionId)
            .HasConversion<SesionIdConverter>()
            .IsRequired();

        builder.Property(c => c.TotalPistas)
            .IsRequired();

        builder.Property(c => c.PistasDescubiertas)
            .IsRequired()
            .HasDefaultValue(0);

        // Constraint: PistasDescubiertas <= TotalPistas (CHECK constraint)
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_ctx_busqueda_pistas",
            "pistas_descubiertas <= total_pistas"));
    }
}
```

### 4.4 Configuración de PistaBusqueda (CatalogoBusquedaTesoro BC)

```csharp
// Configurations/CatalogoBusquedaTesoro/PistaBusquedaConfiguration.cs
public sealed class PistaBusquedaConfiguration : IEntityTypeConfiguration<PistaBusqueda>
{
    public void Configure(EntityTypeBuilder<PistaBusqueda> builder)
    {
        builder.ToTable("pistas_busqueda", "catalogo_busqueda_tesoro");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Descripcion)
            .IsRequired()
            .HasMaxLength(1000);

        // Value Object Coordenada almacenado como owned entity
        builder.OwnsOne(p => p.Coordenada, coord =>
        {
            coord.Property(c => c.Latitud)
                .HasColumnName("coordenada_latitud")
                .HasPrecision(9, 6);

            coord.Property(c => c.Longitud)
                .HasColumnName("coordenada_longitud")
                .HasPrecision(9, 6);
        });

        builder.Property(p => p.Activa)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
```

### 4.5 Configuración de PreguntaTrivia (CatalogoTrivia BC)

```csharp
// Configurations/CatalogoTrivia/PreguntaTriviaConfiguration.cs
public sealed class PreguntaTriviaConfiguration : IEntityTypeConfiguration<PreguntaTrivia>
{
    public void Configure(EntityTypeBuilder<PreguntaTrivia> builder)
    {
        builder.ToTable("preguntas_trivia", "catalogo_trivia");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TextoPregunta)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Dificultad)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Colección de opciones como JSON column (PostgreSQL jsonb)
        builder.Property(p => p.Opciones)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(p => p.IndiceRespuestaCorrecta)
            .IsRequired();

        builder.HasIndex(p => p.Dificultad)
            .HasDatabaseName("ix_preguntas_trivia_dificultad");
    }
}
```

---

## 5. Unit of Work

```csharp
// Application/Common/Interfaces/IUnitOfWork.cs
namespace Umbral.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> GuardarAsync(CancellationToken ct = default);
}

// Infrastructure/Persistence/UnitOfWork.cs
namespace Umbral.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly UmbralDbContext _context;

    public UnitOfWork(UmbralDbContext context) => _context = context;

    public Task<int> GuardarAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
```

---

## 6. Repositorio — implementación de referencia

```csharp
// Infrastructure/Persistence/Repositories/SesionRepository.cs
namespace Umbral.Infrastructure.Persistence.Repositories;

internal sealed class SesionRepository : ISesionRepository
{
    private readonly UmbralDbContext _context;

    public SesionRepository(UmbralDbContext context) => _context = context;

    public async Task<Sesion?> ObtenerPorIdAsync(SesionId id, CancellationToken ct = default)
        => await _context.Sesiones
            .Include(s => s.Etapas)
            .Include(s => s.Equipos)
                .ThenInclude(e => e.Participantes)
            .Include(s => s.ContextoBusquedaTesoro)
            .Include(s => s.ContextoTrivia)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Sesion>> ListarActivasAsync(CancellationToken ct = default)
        => await _context.Sesiones
            .Where(s => s.Estado == EstadoSesion.Activa)
            .AsNoTracking()     // solo lectura, sin tracking
            .ToListAsync(ct);

    public async Task AgregarAsync(Sesion sesion, CancellationToken ct = default)
        => await _context.Sesiones.AddAsync(sesion, ct);

    public Task ActualizarAsync(Sesion sesion, CancellationToken ct = default)
    {
        _context.Sesiones.Update(sesion);
        return Task.CompletedTask;
    }
}
```

---

## 7. Registro de servicios (DI)

```csharp
// Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs
namespace Umbral.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL + EF Core ───────────────────────────────
        services.AddDbContext<UmbralDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(UmbralDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });

            // Solo en Development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // ── Interfaces ─────────────────────────────────────────
        services.AddScoped<IUmbralDbContext>(sp =>
            sp.GetRequiredService<UmbralDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositorios ───────────────────────────────────────
        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IPistaBusquedaRepository, PistaBusquedaRepository>();
        services.AddScoped<IPreguntaTriviaRepository, PreguntaTriviaRepository>();

        return services;
    }
}
```

---

## 8. Connection string y configuración

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=umbral_db;Username=umbral_user;Password=umbral_pass"
  }
}

// appsettings.Development.json (Docker Compose local)
{
  "ConnectionStrings": {
    "Postgres": "Host=postgres;Port=5432;Database=umbral_db;Username=umbral_user;Password=umbral_pass"
  }
}
```

```yaml
# docker-compose.yml (fragmento)
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: umbral_db
      POSTGRES_USER: umbral_user
      POSTGRES_PASSWORD: umbral_pass
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U umbral_user -d umbral_db"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```

---

## 9. Migraciones — flujo de trabajo

```bash
# Desde la raíz del proyecto

# Crear migración
dotnet ef migrations add NombreMigracion \
  --project src/backend/Umbral.Infrastructure \
  --startup-project src/backend/Umbral.API \
  --output-dir Persistence/Migrations

# Aplicar migración
dotnet ef database update \
  --project src/backend/Umbral.Infrastructure \
  --startup-project src/backend/Umbral.API

# Revertir última migración
dotnet ef migrations remove \
  --project src/backend/Umbral.Infrastructure \
  --startup-project src/backend/Umbral.API

# Generar SQL sin aplicar (para revisión)
dotnet ef migrations script \
  --project src/backend/Umbral.Infrastructure \
  --startup-project src/backend/Umbral.API \
  --output migration.sql
```

**Convención de nombres de migración:**

| Tipo de cambio | Ejemplo |
|---|---|
| Nueva tabla | `AddTablaSesiones` |
| Nueva columna | `AddColumnaFechaFinSesion` |
| Índice | `AddIndexSesionEstado` |
| Relación | `AddRelacionSesionEtapa` |
| Datos semilla | `SeedDatosIniciales` |

---

## 10. Esquemas de base de datos por BC

```sql
-- Separación por esquemas PostgreSQL
CREATE SCHEMA IF NOT EXISTS ejecucion_sesion;
CREATE SCHEMA IF NOT EXISTS catalogo_busqueda_tesoro;
CREATE SCHEMA IF NOT EXISTS catalogo_trivia;
```

```csharp
// Convención en las configuraciones:
// EjecucionSesion BC   → schema "ejecucion_sesion"
// CatalogoBusqueda BC  → schema "catalogo_busqueda_tesoro"
// CatalogoTrivia BC    → schema "catalogo_trivia"
builder.ToTable("sesiones", "ejecucion_sesion");
builder.ToTable("pistas_busqueda", "catalogo_busqueda_tesoro");
builder.ToTable("preguntas_trivia", "catalogo_trivia");
```

---

## 11. Reglas EF Core — UMBRAL

| # | Regla | Motivo |
|---|-------|--------|
| 1 | Cada BC tiene su **schema propio** en PostgreSQL. | Aísla datos a nivel de BD igual que en código. |
| 2 | Toda propiedad de dominio se configura via **Fluent API**, no DataAnnotations. | DataAnnotations mezcla capas. |
| 3 | Strongly-typed IDs registrados como **ValueConverters** globales en `ConfigureConventions`. | Evita configuración manual en cada entidad. |
| 4 | Enums almacenados como **string** (`HasConversion<string>()`). | Legibilidad en BD; evita números mágicos. |
| 5 | Value Objects (Coordenada, Puntuacion) configurados con **`OwnsOne`**. | EF los mapea en columnas de la misma tabla. |
| 6 | Collections de objetos simples (Opciones de trivia) como **`jsonb`** en PostgreSQL. | Evita tabla adicional para listas cortas. |
| 7 | `DomainEvents` siempre ignorado con **`builder.Ignore()`**. | EF no debe persistir eventos de dominio. |
| 8 | Queries de solo lectura usan **`AsNoTracking()`**. | Rendimiento; no hay change tracking innecesario. |
| 9 | El repositorio carga el AR con todos sus hijos (**`Include` / `ThenInclude`**). | El AR debe estar completo para aplicar invariantes. |
| 10 | Migraciones se generan desde `Umbral.Infrastructure`, **no** desde el proyecto de tests. | Evita migraciones con dependencias de test. |

---

## 12. Checklist al agregar una nueva entidad

```
□ ¿Tiene IEntityTypeConfiguration<T> propia en la carpeta del BC correspondiente?
□ ¿La tabla está en el schema correcto (ejecucion_sesion / catalogo_*)?
□ ¿El nombre de la tabla y columnas sigue snake_case?
□ ¿El strongly-typed ID tiene su ValueConverter registrado?
□ ¿Los enums se almacenan como string?
□ ¿Los Value Objects se configuran con OwnsOne?
□ ¿DomainEvents está en builder.Ignore()?
□ ¿Se creó migración después del cambio de configuración?
□ ¿Se verificó que la migración generada es correcta antes de aplicar?
□ ¿Las queries de lectura usan AsNoTracking()?
```

---

## 13. Anti-patrones a evitar

```csharp
// ❌ MALO — DataAnnotations en la entidad de dominio
public class Sesion
{
    [Key]
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; }  // ← mezcla capa dominio con capa de persistencia
}

// ✅ BUENO — configuración en IEntityTypeConfiguration
builder.Property(s => s.Nombre).IsRequired().HasMaxLength(200);

// ❌ MALO — Guid sin strongly-typed ID en la configuración
builder.HasKey(s => s.Id);  // Id es Guid directo

// ✅ BUENO — converter registrado globalmente
configurationBuilder.Properties<SesionId>().HaveConversion<SesionIdConverter>();

// ❌ MALO — repositorio sin Include de hijos
public async Task<Sesion?> ObtenerPorIdAsync(SesionId id, CancellationToken ct)
    => await _context.Sesiones.FindAsync(id, ct);  // sin Etapas, Equipos, etc.

// ✅ BUENO — AR cargado completo
public async Task<Sesion?> ObtenerPorIdAsync(SesionId id, CancellationToken ct)
    => await _context.Sesiones
        .Include(s => s.Etapas)
        .Include(s => s.Equipos).ThenInclude(e => e.Participantes)
        .Include(s => s.ContextoBusquedaTesoro)
        .Include(s => s.ContextoTrivia)
        .FirstOrDefaultAsync(s => s.Id == id, ct);

// ❌ MALO — schema por defecto "public" mezclando todos los BCs
builder.ToTable("sesiones");         // schema "public" (PostgreSQL default)

// ✅ BUENO — schema explícito por BC
builder.ToTable("sesiones", "ejecucion_sesion");

// ❌ MALO — enum almacenado como int
builder.Property(s => s.Tipo).HasConversion<int>();  // 1, 2 en BD

// ✅ BUENO — enum almacenado como string
builder.Property(s => s.Tipo).HasConversion<string>(); // "BusquedaTesoro", "Trivia"
```

---

## 14. Guía de uso para Cursor AI

Cuando el usuario pida configurar persistencia en UMBRAL:

1. **Identificar BC** → determinar el schema (`ejecucion_sesion`, `catalogo_busqueda_tesoro`, `catalogo_trivia`).
2. **Crear `IEntityTypeConfiguration<T>`** en la carpeta `Configurations/<BC>/`.
3. **Registrar el ValueConverter** del strongly-typed ID en `ConfigureConventions` si es nuevo.
4. **Configurar relaciones** entre AR y entidades hijas con `HasMany`/`HasOne`.
5. **Ignorar `DomainEvents`** con `builder.Ignore()`.
6. **Agregar al DbSet** correspondiente en `UmbralDbContext`.
7. **Crear el repositorio** en `Infrastructure/Persistence/Repositories/` implementando la interfaz de dominio.
8. **Registrar el repositorio** en `InfrastructureServiceExtensions`.
9. **Generar migración** con el comando de la sección 9.
10. **Correr checklist** de la sección 12 y advertir si hay anti-patrones de la sección 13.
