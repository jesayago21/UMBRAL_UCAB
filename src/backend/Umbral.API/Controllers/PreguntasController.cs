using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Extensions;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.ActualizarPregunta;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.EliminarPregunta;
using Umbral.Application.CatalogoTrivia.Preguntas.Queries.GetPreguntaById;
using Umbral.Application.CatalogoTrivia.Preguntas.Queries.ListPreguntas;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/preguntas")]
[Authorize]
public sealed class PreguntasController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreguntasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearPreguntaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearPreguntaCommand(
                request.Enunciado,
                request.Dificultad,
                request.CategoriaId,
                MapOpciones(request.Opciones)),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            id => new CreatedResult($"/api/v1/preguntas/{id}", new { id }));
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Operador")]
    [ProducesResponseType(typeof(IReadOnlyList<PreguntaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? categoriaId,
        [FromQuery] string? dificultad,
        [FromQuery] string? enunciado,
        CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(
            new ListPreguntasQuery(categoriaId, dificultad, enunciado),
            cancellationToken);

        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreguntaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var pregunta = await _mediator.Send(new GetPreguntaByIdQuery(id), cancellationToken);
        return Ok(MapToResponse(pregunta));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarPreguntaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ActualizarPreguntaCommand(
                id,
                request.Enunciado,
                request.Dificultad,
                request.CategoriaId,
                MapOpciones(request.Opciones)),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EliminarPreguntaCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    private static IReadOnlyList<OpcionRespuestaInput> MapOpciones(
        IReadOnlyList<OpcionRespuestaRequest> opciones) =>
        opciones
            .Select(o => new OpcionRespuestaInput(o.Texto, o.EsCorrecta))
            .ToList();

    private static PreguntaResponse MapToResponse(PreguntaDto pregunta) =>
        new(
            pregunta.Id,
            pregunta.Enunciado,
            pregunta.Dificultad,
            pregunta.CategoriaId,
            pregunta.Opciones
                .Select(o => new OpcionRespuestaResponse(o.Texto, o.EsCorrecta))
                .ToList());
}
