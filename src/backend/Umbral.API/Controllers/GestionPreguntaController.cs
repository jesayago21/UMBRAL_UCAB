using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Extensions;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.ActualizarPregunta;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.EliminarPregunta;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/preguntas")]
[Authorize(Roles = "Administrador")]
public sealed class GestionPreguntaController : ControllerBase
{
    private readonly ISender _sender;

    public GestionPreguntaController(ISender sender) => _sender = sender;

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarPreguntaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ActualizarPreguntaCommand(
                id,
                request.Enunciado,
                request.Dificultad,
                request.CategoriaId,
                request.Opciones.Select(o => new OpcionRespuestaInput(o.Texto, o.EsCorrecta)).ToList()),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new EliminarPreguntaCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }
}
