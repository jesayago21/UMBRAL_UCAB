using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Auth;
using Umbral.API.Extensions;
using Umbral.Application.IdentidadYAccesos.Commands.RegistroParticipante;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class RegistroParticipanteController : ControllerBase
{
    private readonly ISender _sender;

    public RegistroParticipanteController(ISender sender) => _sender = sender;

    /// <summary>
    /// Auto-registro público: crea cuenta Keycloak con rol Participante y contraseña inmediata.
    /// </summary>
    [HttpPost("registro-participante")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistroParticipanteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistroParticipanteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RegistroParticipanteCommand(
                request.Email,
                request.Username,
                request.Nombre,
                request.Apellido,
                request.Password),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            created => new CreatedResult(
                $"/api/v1/auth/registro-participante/{created.KeycloakUserId}",
                new RegistroParticipanteResponse(
                    created.KeycloakUserId,
                    created.Email,
                    created.Username)));
    }
}
