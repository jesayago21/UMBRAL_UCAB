using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Umbral.API.Auth;

namespace Umbral.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddUmbralApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddProblemDetails();

        if (environment.IsEnvironment("Testing"))
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme    = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        }
        else
        {
            var keycloak = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
                ?? new KeycloakOptions();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = keycloak.Authority;
                    options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;

                    var validateAudience = !string.IsNullOrWhiteSpace(keycloak.Audience);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = keycloak.Authority,
                        ValidateAudience = validateAudience,
                        ValidAudience = keycloak.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = "preferred_username",
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = MapRealmRolesAsync
                    };
                });
        }

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Keycloak emite roles en el claim <c>realm_access.roles</c> (JSON anidado).
    /// ASP.NET Core no los reconoce como roles automáticamente, así que los
    /// proyectamos a <see cref="ClaimTypes.Role"/> tras validar el token.
    /// </summary>
    private static Task MapRealmRolesAsync(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return Task.CompletedTask;

        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
            return Task.CompletedTask;

        try
        {
            using var document = JsonDocument.Parse(realmAccess);
            if (document.RootElement.TryGetProperty("roles", out var roles)
                && roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var value = role.GetString();
                    if (!string.IsNullOrWhiteSpace(value)
                        && !identity.HasClaim(ClaimTypes.Role, value))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, value));
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Token con realm_access malformado: lo dejamos sin roles mapeados.
        }

        return Task.CompletedTask;
    }
}
