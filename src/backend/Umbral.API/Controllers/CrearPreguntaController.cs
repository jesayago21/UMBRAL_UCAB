using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Extensions;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/preguntas")]
[Authorize(Roles = "Administrador")]
public sealed class CrearPreguntaController : ControllerBase
{
    private readonly ISender _sender;

    public CrearPreguntaController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearPreguntaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CrearPreguntaCommand(
                request.Enunciado,
                request.Dificultad,
                request.CategoriaId,
                request.Opciones.Select(o => new OpcionRespuestaInput(o.Texto, o.EsCorrecta)).ToList()),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            id => new CreatedResult($"/api/v1/preguntas/{id}", new { id }));
    }
}
