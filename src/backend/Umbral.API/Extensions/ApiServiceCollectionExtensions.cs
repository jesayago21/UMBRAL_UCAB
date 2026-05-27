using Microsoft.AspNetCore.Authentication;
using Umbral.API.Auth;

namespace Umbral.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddUmbralApi(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddProblemDetails();

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
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

            services.AddAuthorization();
        }

        return services;
    }
}
