using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Misiones;
using Umbral.API.Extensions;
using Umbral.Application.Misiones.Commands.CrearMision;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/misiones")]
[Authorize(Roles = "Administrador")]
public sealed class CrearMisionController : ControllerBase
{
    private readonly ISender _sender;

    public CrearMisionController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearMisionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CrearMisionCommand(
            request.Nombre,
            (request.Etapas ?? [])
                .Select(etapa => new EtapaMisionInput(
                    etapa.TipoEtapa,
                    etapa.Orden,
                    etapa.Descripcion,
                    etapa.CodigoQrSolucion,
                    (etapa.Pistas ?? []).Select(pista =>
                        new CrearPistaInput(pista.Contenido, pista.TipoLiberacion, pista.SegundosLiberacion)).ToList(),
                    etapa.CategoriaIds,
                    etapa.Latitud,
                    etapa.Longitud,
                    etapa.RadioMetros))
                .ToList(),
            request.Activar);

        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext,
            id => new CreatedResult($"/api/v1/misiones/{id}", new { id }));
    }
}
