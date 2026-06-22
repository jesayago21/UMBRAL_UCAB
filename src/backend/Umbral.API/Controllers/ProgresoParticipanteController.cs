using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionParticipante;
using Umbral.Application.Sesion.Queries.GetSesionEtapasParticipante;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Participante")]
public sealed class ProgresoParticipanteController : ControllerBase
{
    private readonly ISender _sender;

    public ProgresoParticipanteController(ISender sender) => _sender = sender;

    [HttpGet("{id:guid}/etapas")]
    [ProducesResponseType(typeof(SesionEtapasParticipanteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEtapas(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(
            new GetSesionEtapasParticipanteQuery(id, ObtenerJugadorId()),
            cancellationToken);

        return Ok(new SesionEtapasParticipanteResponse(
            dto.Estado,
            dto.TotalEtapas,
            dto.Etapas.Select(e => new EtapaSesionResponse(
                e.Orden,
                e.TipoEtapa,
                e.Descripcion,
                e.EsActual,
                e.Pistas?
                    .Select(p => new PistaSesionResponse(
                        p.Contenido,
                        p.TipoLiberacion,
                        p.SegundosLiberacion))
                    .ToList(),
                e.CategoriaIds)).ToList()));
    }

    [HttpGet("{id:guid}/trivia/preguntas")]
    [ProducesResponseType(typeof(IReadOnlyList<PreguntaTriviaParticipanteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPreguntasTrivia(Guid id, CancellationToken cancellationToken)
    {
        var preguntas = await _sender.Send(
            new GetPreguntasTriviaSesionParticipanteQuery(id, ObtenerJugadorId()),
            cancellationToken);

        return Ok(preguntas
            .Select(p => new PreguntaTriviaParticipanteResponse(
                p.Orden,
                p.Id,
                p.Enunciado,
                p.Dificultad,
                p.Opciones))
            .ToList());
    }

    private Guid ObtenerJugadorId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : TestAuthHandler.DefaultParticipanteId;
}
