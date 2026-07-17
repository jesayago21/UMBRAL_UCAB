using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
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

                    if (!string.IsNullOrWhiteSpace(keycloak.MetadataAddress))
                    {
                        var metadata = keycloak.MetadataAddress.TrimEnd('/');
                        options.MetadataAddress = metadata.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? metadata + "/.well-known/openid-configuration"
                            : metadata;
                    }

                    var validateAudience = !string.IsNullOrWhiteSpace(keycloak.Audience);

                    var authority = keycloak.Authority?.TrimEnd('/') ?? string.Empty;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuers = string.IsNullOrEmpty(authority)
                            ? null
                            : [authority, authority + "/"],
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
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken)
                                && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = MapRealmRolesAsync,
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                            logger?.CreateLogger("JwtBearer").LogWarning(
                                context.Exception,
                                "JWT auth failed for {Method} {Path}",
                                context.HttpContext.Request.Method,
                                context.HttpContext.Request.Path);
                            return Task.CompletedTask;
                        }
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
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    if (!identity.HasClaim(ClaimTypes.Role, value))
                        identity.AddClaim(new Claim(ClaimTypes.Role, value));

                    // Realm legacy (rename Equipo → Participante): alias para [Authorize(Roles = "Participante")]
                    if (string.Equals(value, "EquipoParticipante", StringComparison.Ordinal)
                        && !identity.HasClaim(ClaimTypes.Role, "Participante"))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Participante"));
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
