using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Queries.GetPreguntaById;
using Umbral.Application.CatalogoTrivia.Preguntas.Queries.ListPreguntas;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/preguntas")]
[Authorize]
public sealed class ConsultaPreguntasController : ControllerBase
{
    private readonly ISender _sender;

    public ConsultaPreguntasController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "Administrador,Operador")]
    [ProducesResponseType(typeof(IReadOnlyList<PreguntaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? categoriaId,
        [FromQuery] string? dificultad,
        [FromQuery] string? enunciado,
        CancellationToken cancellationToken)
    {
        var items = await _sender.Send(
            new ListPreguntasQuery(categoriaId, dificultad, enunciado),
            cancellationToken);

        return Ok(items.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(PreguntaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var pregunta = await _sender.Send(new GetPreguntaByIdQuery(id), cancellationToken);
        return Ok(MapToResponse(pregunta));
    }

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
