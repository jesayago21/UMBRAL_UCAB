using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Misiones;
using Umbral.Application.Misiones.Models;
using Umbral.Application.Misiones.Queries.GetMisionById;
using Umbral.Application.Misiones.Queries.ListMisiones;
using Umbral.Application.Misiones.Queries.ListMisionesActivas;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/misiones")]
[Authorize]
public sealed class ConsultaMisionesController : ControllerBase
{
    private readonly ISender _sender;

    public ConsultaMisionesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<MisionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre,
        [FromQuery] string? estado,
        CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new ListMisionesQuery(nombre, estado), cancellationToken);
        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("activas")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<MisionActivaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarActivas(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new ListMisionesActivasQuery(), cancellationToken);
        return Ok(items.Select(x => new MisionActivaResponse(x.Id, x.Nombre)).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(MisionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var mision = await _sender.Send(new GetMisionByIdQuery(id), cancellationToken);
        return Ok(MapToResponse(mision));
    }

    private static MisionResponse MapToResponse(MisionDto mision) =>
        new(
            mision.Id,
            mision.Nombre,
            mision.Estado,
            mision.TotalEtapas,
            mision.Etapas.Select(etapa => new EtapaMisionResponse(
                etapa.EtapaId,
                etapa.Orden,
                etapa.TipoEtapa,
                etapa.Descripcion,
                etapa.CodigoQrSolucion,
                etapa.Pistas?.Select(pista => new PistaMisionResponse(
                    pista.PistaId,
                    pista.Contenido,
                    pista.TipoLiberacion,
                    pista.SegundosLiberacion)).ToList(),
                etapa.CategoriaIds,
                etapa.Latitud,
                etapa.Longitud,
                etapa.RadioMetros)).ToList());
}
