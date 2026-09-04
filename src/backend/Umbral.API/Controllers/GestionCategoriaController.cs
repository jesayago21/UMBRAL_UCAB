using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Extensions;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.ActualizarCategoria;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.EliminarCategoria;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize(Roles = "Administrador")]
public sealed class GestionCategoriaController : ControllerBase
{
    private readonly ISender _sender;

    public GestionCategoriaController(ISender sender) => _sender = sender;

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarCategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActualizarCategoriaCommand(id, request.Nombre), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new EliminarCategoriaCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }
}
