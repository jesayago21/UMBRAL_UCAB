using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbral.API.Auth;
using Umbral.API.Extensions;

namespace Umbral.API.Tests.Auth;

/// <summary>
/// Unit tests del wiring de autenticación Keycloak (rama de producción) y del
/// mapeo de roles <c>realm_access.roles</c> a <see cref="ClaimTypes.Role"/>.
/// </summary>
public sealed class KeycloakWiringTests
{
    private static IServiceProvider BuildProviderProduccion(
        Dictionary<string, string?>? config = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config ?? new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8080/realms/umbral",
                ["Keycloak:Audience"] = "umbral-api",
                ["Keycloak:RequireHttpsMetadata"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUmbralApi(configuration, new FakeHostEnvironment());

        return services.BuildServiceProvider();
    }

    private static JwtBearerOptions ResolveJwtOptions(IServiceProvider provider)
        => provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

    private static async Task<ClaimsPrincipal> InvocarOnTokenValidatedAsync(
        JwtBearerOptions options,
        ClaimsPrincipal principal)
    {
        var context = new TokenValidatedContext(
            new DefaultHttpContext(),
            new AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                displayName: null,
                handlerType: typeof(JwtBearerHandler)),
            options)
        {
            Principal = principal
        };

        await options.Events.OnTokenValidated(context);
        return context.Principal!;
    }

    private static ClaimsPrincipal PrincipalConClaim(string tipo, string valor)
        => new(new ClaimsIdentity(new[] { new Claim(tipo, valor) }, "test"));

    // ── Wiring ─────────────────────────────────────────────────────

    [Fact]
    public void AddUmbralApi_EnProduccion_ConfiguraJwtBearerDesdeKeycloak()
    {
        // Arrange / Act
        var provider = BuildProviderProduccion();
        var options = ResolveJwtOptions(provider);

        // Assert
        options.Authority.Should().Be("http://localhost:8080/realms/umbral");
        options.RequireHttpsMetadata.Should().BeFalse();
        options.TokenValidationParameters.ValidateIssuer.Should().BeTrue();
        options.TokenValidationParameters.ValidateAudience.Should().BeTrue();
        options.TokenValidationParameters.ValidAudience.Should().Be("umbral-api");
        options.Events.OnTokenValidated.Should().NotBeNull();
    }

    [Fact]
    public void AddUmbralApi_EnProduccion_UsaJwtComoEsquemaPorDefecto()
    {
        // Arrange / Act
        var provider = BuildProviderProduccion();
        var authOptions = provider
            .GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        // Assert
        authOptions.DefaultAuthenticateScheme.Should().Be(JwtBearerDefaults.AuthenticationScheme);
        authOptions.DefaultChallengeScheme.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void AddUmbralApi_SinSeccionKeycloak_UsaValoresPorDefecto()
    {
        // Arrange / Act: sin claves Keycloak => se usa new KeycloakOptions()
        var provider = BuildProviderProduccion(new Dictionary<string, string?>());
        var options = ResolveJwtOptions(provider);

        // Assert
        options.Authority.Should().Be("http://localhost:8080/realms/umbral");
        options.TokenValidationParameters.ValidAudience.Should().Be("umbral-api");
    }

    [Fact]
    public void AddUmbralApi_ConMetadataAddress_ConfiguraJwtBearerMetadata()
    {
        var provider = BuildProviderProduccion(new Dictionary<string, string?>
        {
            ["Keycloak:Authority"] = "http://localhost:8080/realms/umbral",
            ["Keycloak:MetadataAddress"] = "http://keycloak:8080/realms/umbral",
            ["Keycloak:Audience"] = "umbral-api",
            ["Keycloak:RequireHttpsMetadata"] = "false"
        });
        var options = ResolveJwtOptions(provider);

        options.Authority.Should().Be("http://localhost:8080/realms/umbral");
        options.MetadataAddress.Should().Be(
            "http://keycloak:8080/realms/umbral/.well-known/openid-configuration");
    }

    [Fact]
    public void KeycloakOptions_PorDefecto_TieneValoresDeDesarrollo()
    {
        // Arrange / Act
        var options = new KeycloakOptions();

        // Assert
        options.Authority.Should().Be("http://localhost:8080/realms/umbral");
        options.Audience.Should().Be("umbral-api");
        options.RequireHttpsMetadata.Should().BeFalse();
        KeycloakOptions.SectionName.Should().Be("Keycloak");
    }

    // ── Mapeo de roles realm_access ────────────────────────────────

    [Fact]
    public async Task OnTokenValidated_ConRolesEnRealmAccess_LosProyectaComoRoleClaims()
    {
        // Arrange
        var options = ResolveJwtOptions(BuildProviderProduccion());
        var principal = PrincipalConClaim(
            "realm_access",
            """{ "roles": ["admin", "operador"] }""");

        // Act
        var resultado = await InvocarOnTokenValidatedAsync(options, principal);

        // Assert
        resultado.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo("admin", "operador");
    }

    [Fact]
    public async Task OnTokenValidated_SinRealmAccess_NoAgregaRoles()
    {
        // Arrange
        var options = ResolveJwtOptions(BuildProviderProduccion());
        var principal = PrincipalConClaim("preferred_username", "ana");

        // Act
        var resultado = await InvocarOnTokenValidatedAsync(options, principal);

        // Assert
        resultado.FindAll(ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task OnTokenValidated_ConRealmAccessMalformado_NoLanzaNiAgregaRoles()
    {
        // Arrange
        var options = ResolveJwtOptions(BuildProviderProduccion());
        var principal = PrincipalConClaim("realm_access", "{ esto no es json válido");

        // Act
        var resultado = await InvocarOnTokenValidatedAsync(options, principal);

        // Assert
        resultado.FindAll(ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task OnTokenValidated_CuandoRolesNoEsArray_NoAgregaRoles()
    {
        // Arrange
        var options = ResolveJwtOptions(BuildProviderProduccion());
        var principal = PrincipalConClaim("realm_access", """{ "roles": "admin" }""");

        // Act
        var resultado = await InvocarOnTokenValidatedAsync(options, principal);

        // Assert
        resultado.FindAll(ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task OnTokenValidated_SinIdentidadDeClaims_NoHaceNada()
    {
        // Arrange
        var options = ResolveJwtOptions(BuildProviderProduccion());
        var principal = new ClaimsPrincipal(); // sin identidades => Identity null

        // Act
        var act = async () => await InvocarOnTokenValidatedAsync(options, principal);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "Umbral.API.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
