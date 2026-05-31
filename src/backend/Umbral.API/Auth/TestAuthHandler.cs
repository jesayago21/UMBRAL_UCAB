using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Umbral.API.Auth;

/// <summary>
/// Esquema de autenticación para entornos Development/Testing (sin JWT real).
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string RoleHeaderName = "X-Test-Role";
    public const string UserIdHeaderName = "X-Test-User-Id";

    public static readonly Guid DefaultOperadorId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.TryGetValue(RoleHeaderName, out var roleHeader)
            ? roleHeader.ToString()
            : string.Empty;

        var userId = Request.Headers.TryGetValue(UserIdHeaderName, out var userIdHeader)
                     && Guid.TryParse(userIdHeader.ToString(), out var parsedUserId)
            ? parsedUserId
            : DefaultOperadorId;

        var claims = string.IsNullOrWhiteSpace(role)
            ? new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Operador"),
                new Claim(ClaimTypes.Role, "Administrador"),
                new Claim(ClaimTypes.Role, "EquipoParticipante")
            }
            : new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

        var identity  = new ClaimsIdentity(claims, SchemeName);
        var principal   = new ClaimsPrincipal(identity);
        var ticket      = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
