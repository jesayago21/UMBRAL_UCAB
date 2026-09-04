using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Usuarios;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/autenticacion")]
public sealed class AutenticacionController : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    public IActionResult ObtenerUsuarioActual()
    {
        var user = HttpContext.User;
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub")
                  ?? string.Empty;
        var username = user.FindFirstValue("preferred_username")
                       ?? user.Identity?.Name
                       ?? string.Empty;
        var email = user.FindFirstValue(ClaimTypes.Email)
                    ?? user.FindFirstValue("email")
                    ?? string.Empty;
        var roles = user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new MeResponse(sub, username, email, roles));
    }
}
