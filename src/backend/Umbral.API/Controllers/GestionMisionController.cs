using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Misiones;
using Umbral.API.Extensions;
using Umbral.Application.Misiones.Commands.ActualizarMision;
using Umbral.Application.Misiones.Commands.EliminarMision;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/misiones")]
[Authorize(Roles = "Administrador")]
public sealed class GestionMisionController : ControllerBase
{
    private readonly ISender _sender;

    public GestionMisionController(ISender sender) => _sender = sender;

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarMisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ActualizarMisionCommand(id, request.Nombre, request.Activar),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new EliminarMisionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }
}
