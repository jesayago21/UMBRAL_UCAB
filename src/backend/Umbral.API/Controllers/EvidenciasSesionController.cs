using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.SubmitEvidencia;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Participante")]
public sealed class EvidenciasSesionController : ControllerBase
{
    private readonly ISender _sender;

    public EvidenciasSesionController(ISender sender) => _sender = sender;

    [HttpPost("{id:guid}/evidencias")]
    [ProducesResponseType(typeof(SubmitEvidenciaResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Enviar(
        Guid id,
        [FromBody] SubmitEvidenciaRequest request,
        CancellationToken cancellationToken)
    {
        var jugadorId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var gid)
            ? gid
            : TestAuthHandler.DefaultParticipanteId;

        var result = await _sender.Send(
            new SubmitEvidenciaCommand(id, jugadorId, request.CodigoQr),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            value => new CreatedResult(
                $"/api/v1/sesiones/{id}/evidencias/{value.EvidenciaId}",
                new SubmitEvidenciaResponse(value.EvidenciaId, value.Resultado)));
    }
}
