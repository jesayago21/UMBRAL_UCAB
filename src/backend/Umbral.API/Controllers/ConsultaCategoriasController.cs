using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.Application.CatalogoTrivia.Categorias.Queries.GetCategoriaById;
using Umbral.Application.CatalogoTrivia.Categorias.Queries.ListCategorias;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize]
public sealed class ConsultaCategoriasController : ControllerBase
{
    private readonly ISender _sender;

    public ConsultaCategoriasController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "Administrador,Operador")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? nombre, CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new ListCategoriasQuery(nombre), cancellationToken);
        return Ok(items.Select(c => new CategoriaResponse(c.Id, c.Nombre)).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var categoria = await _sender.Send(new GetCategoriaByIdQuery(id), cancellationToken);
        return Ok(new CategoriaResponse(categoria.Id, categoria.Nombre));
    }
}
