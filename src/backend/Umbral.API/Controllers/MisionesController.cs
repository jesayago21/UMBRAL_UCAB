using MediatR;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Misiones;
using Umbral.API.Extensions;
using Umbral.Application.Misiones.Commands.ActualizarMision;
using Umbral.Application.Misiones.Commands.CrearMision;
using Umbral.Application.Misiones.Commands.DesactivarMision;
using Umbral.Application.Misiones.Models;
using Umbral.Application.Misiones.Queries.GetMisionById;
using Umbral.Application.Misiones.Queries.ListMisiones;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/misiones")]
[Authorize]
public sealed class MisionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MisionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearMisionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Etapas is null)
            throw BuildValidationException(nameof(request.Etapas), "La misión debe incluir etapas.");

        var command = new CrearMisionCommand(
            request.Nombre,
            request.Etapas
                .Select(etapa => new CrearEtapaInput(
                    etapa.Descripcion,
                    etapa.CodigoQrSolucion,
                    (etapa.Pistas ?? []).Select(pista => new CrearPistaInput(
                        pista.Contenido,
                        pista.TipoLiberacion,
                        pista.SegundosLiberacion)).ToList()))
                .ToList(),
            request.Activar);

        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult(
            HttpContext,
            id => new CreatedResult($"/api/v1/misiones/{id}", new { id }));
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<MisionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre,
        [FromQuery] string? estado,
        CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new ListMisionesQuery(nombre, estado), cancellationToken);
        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(MisionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var mision = await _mediator.Send(new GetMisionByIdQuery(id), cancellationToken);
        return Ok(MapToResponse(mision));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarMisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ActualizarMisionCommand(id, request.Nombre, request.Activar),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DesactivarMisionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    private static MisionResponse MapToResponse(MisionDto mision) =>
        new(
            mision.Id,
            mision.Nombre,
            mision.Descripcion,
            mision.NivelDificultad,
            mision.TiempoMaximoSeg,
            mision.Estado,
            mision.TotalEtapas,
            mision.Etapas.Select(etapa => new EtapaMisionResponse(
                etapa.EtapaId,
                etapa.Orden,
                etapa.Descripcion,
                etapa.CodigoQrSolucion,
                etapa.Pistas.Select(pista => new PistaMisionResponse(
                    pista.PistaId,
                    pista.Contenido,
                    pista.TipoLiberacion,
                    pista.SegundosLiberacion)).ToList())).ToList());

    private static ValidationException BuildValidationException(string property, string error) =>
        new([new ValidationFailure(property, error)]);
}
