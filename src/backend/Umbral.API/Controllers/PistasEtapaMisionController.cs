using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Misiones;
using Umbral.API.Extensions;
using Umbral.Application.Misiones.Commands.AgregarPistaEtapa;
using Umbral.Application.Misiones.Commands.EditarPistaEtapa;
using Umbral.Application.Misiones.Commands.EliminarPistaEtapa;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/misiones")]
[Authorize(Roles = "Administrador")]
public sealed class PistasEtapaMisionController : ControllerBase
{
    private readonly ISender _sender;

    public PistasEtapaMisionController(ISender sender) => _sender = sender;

    [HttpPost("{misionId:guid}/etapas/{etapaId:guid}/pistas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AgregarPista(
        Guid misionId,
        Guid etapaId,
        [FromBody] AgregarPistaEtapaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AgregarPistaEtapaCommand(
                misionId,
                etapaId,
                request.Contenido,
                request.TipoLiberacion,
                request.SegundosLiberacion),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            id => new CreatedResult($"/api/v1/misiones/{misionId}", new { id }));
    }

    [HttpPut("{misionId:guid}/etapas/{etapaId:guid}/pistas/{pistaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EditarPista(
        Guid misionId,
        Guid etapaId,
        Guid pistaId,
        [FromBody] EditarPistaEtapaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new EditarPistaEtapaCommand(
                misionId,
                etapaId,
                pistaId,
                request.Contenido,
                request.TipoLiberacion,
                request.SegundosLiberacion),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpDelete("{misionId:guid}/etapas/{etapaId:guid}/pistas/{pistaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EliminarPista(
        Guid misionId,
        Guid etapaId,
        Guid pistaId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new EliminarPistaEtapaCommand(misionId, etapaId, pistaId),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }
}
