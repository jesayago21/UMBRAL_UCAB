using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Extensions;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize(Roles = "Administrador")]
public sealed class CrearCategoriaController : ControllerBase
{
    private readonly ISender _sender;

    public CrearCategoriaController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CrearCategoriaCommand(request.Nombre), cancellationToken);
        return result.ToActionResult(HttpContext,
            id => new CreatedResult($"/api/v1/categorias/{id}", new { id }));
    }
}
