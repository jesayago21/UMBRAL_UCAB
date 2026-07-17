using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.CerrarPreguntaTrivia;
using Umbral.Application.Sesion.Commands.LanzarPreguntaTrivia;

namespace Umbral.API.Controllers;

/// <summary>HU-33 — operador lanza / cierra rondas de trivia.</summary>
[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class TriviaOperadorController : ControllerBase
{
    private readonly ISender _sender;

    public TriviaOperadorController(ISender sender) => _sender = sender;

    [HttpPost("{id:guid}/trivia/lanzar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Lanzar(
        Guid id,
        [FromBody] LanzarPreguntaTriviaRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LanzarPreguntaTriviaCommand(id, request?.DuracionSegundos),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/trivia/cerrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cerrar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CerrarPreguntaTriviaCommand(id),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }
}
