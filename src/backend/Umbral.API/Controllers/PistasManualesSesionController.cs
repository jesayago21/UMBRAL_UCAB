using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.LiberarPistaManual;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class PistasManualesSesionController : ControllerBase
{
    private readonly ISender _sender;

    public PistasManualesSesionController(ISender sender) => _sender = sender;

    [HttpPost("{id:guid}/pistas-manuales")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LiberarPistaManual(
        Guid id,
        [FromBody] LiberarPistaManualRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LiberarPistaManualCommand(
                id,
                request.Contenido,
                request.ParticipanteId),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }
}
