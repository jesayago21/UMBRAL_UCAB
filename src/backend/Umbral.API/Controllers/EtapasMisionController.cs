using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Misiones;
using Umbral.API.Extensions;
using Umbral.Application.Misiones.Commands.AgregarEtapaMision;
using Umbral.Application.Misiones.Commands.EditarEtapaMision;
using Umbral.Application.Misiones.Commands.EliminarEtapaMision;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/misiones")]
[Authorize(Roles = "Administrador")]
public sealed class EtapasMisionController : ControllerBase
{
    private readonly ISender _sender;

    public EtapasMisionController(ISender sender) => _sender = sender;

    [HttpPost("{misionId:guid}/etapas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AgregarEtapa(
        Guid misionId,
        [FromBody] AgregarEtapaMisionRequest request,
        CancellationToken cancellationToken)
    {
        var pistas = request.Pistas?
            .Select(p => new AgregarEtapaPistaInput(
                p.Contenido,
                p.TipoLiberacion,
                p.SegundosLiberacion))
            .ToList();

        var result = await _sender.Send(
            new AgregarEtapaMisionCommand(
                misionId,
                request.TipoEtapa,
                request.Descripcion,
                request.CodigoQrSolucion,
                request.CategoriaIds,
                pistas,
                request.Latitud,
                request.Longitud,
                request.RadioMetros),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            id => new CreatedResult($"/api/v1/misiones/{misionId}/etapas/{id}", new { id }));
    }

    [HttpPut("{misionId:guid}/etapas/{etapaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EditarEtapa(
        Guid misionId,
        Guid etapaId,
        [FromBody] EditarEtapaMisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new EditarEtapaMisionCommand(
                misionId,
                etapaId,
                request.Descripcion,
                request.CodigoQrSolucion,
                request.CategoriaIds,
                request.Latitud,
                request.Longitud,
                request.RadioMetros),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpDelete("{misionId:guid}/etapas/{etapaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EliminarEtapa(
        Guid misionId,
        Guid etapaId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new EliminarEtapaMisionCommand(misionId, etapaId),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }
}
