using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.ExpulsarParticipante;

namespace Umbral.API.Controllers;

/// <summary>HU-32 — expulsión de participante desde la sala de espera.</summary>
[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class ExpulsarParticipanteController : ControllerBase
{
    private readonly ISender _sender;

    public ExpulsarParticipanteController(ISender sender) => _sender = sender;

    [HttpDelete("{id:guid}/participantes/{participanteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Expulsar(
        Guid id,
        Guid participanteId,
        [FromBody] ExpulsarParticipanteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ExpulsarParticipanteCommand(id, participanteId, request.Motivo),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }
}
