using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.AplicarPenalizacion;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class PenalizacionesSesionController : ControllerBase
{
    private readonly ISender _sender;

    public PenalizacionesSesionController(ISender sender) => _sender = sender;

    [HttpPost("{id:guid}/penalizaciones")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AplicarPenalizacion(
        Guid id,
        [FromBody] AplicarPenalizacionRequest request,
        CancellationToken cancellationToken)
    {
        var operadorId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var gid)
            ? gid
            : TestAuthHandler.DefaultOperadorId;

        var result = await _sender.Send(
            new AplicarPenalizacionCommand(
                id,
                request.ParticipanteId,
                request.Puntos,
                request.Motivo,
                operadorId),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }
}
