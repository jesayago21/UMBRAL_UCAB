using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Extensions;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.ActualizarCategoria;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.EliminarCategoria;
using Umbral.Application.CatalogoTrivia.Categorias.Queries.GetCategoriaById;
using Umbral.Application.CatalogoTrivia.Categorias.Queries.ListCategorias;
using Umbral.Application.CatalogoTrivia.Models;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize]
public sealed class CategoriasController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearCategoriaCommand(request.Nombre),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            id => new CreatedResult($"/api/v1/categorias/{id}", new { id }));
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Operador")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nombre,
        CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new ListCategoriasQuery(nombre), cancellationToken);
        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var categoria = await _mediator.Send(new GetCategoriaByIdQuery(id), cancellationToken);
        return Ok(MapToResponse(categoria));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarCategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ActualizarCategoriaCommand(id, request.Nombre),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EliminarCategoriaCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    private static CategoriaResponse MapToResponse(CategoriaDto categoria) =>
        new(categoria.Id, categoria.Nombre);
}
