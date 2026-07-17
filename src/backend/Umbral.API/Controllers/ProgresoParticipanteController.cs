using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.SubmitRespuestaTrivia;
using Umbral.Application.Sesion.Queries.GetEstadoTriviaSesion;
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
                        p.PistaId,
                        p.Contenido,
                        p.TipoLiberacion,
                        p.SegundosLiberacion))
                    .ToList(),
                e.CategoriaIds,
                e.Latitud,
                e.Longitud,
                e.RadioMetros)).ToList()));
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

    /// <summary>HU-33 — estado sincronizado de la ronda trivia (late join / reconexión).</summary>
    [HttpGet("{id:guid}/trivia/estado")]
    [ProducesResponseType(typeof(EstadoTriviaSesionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEstadoTrivia(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(
            new GetEstadoTriviaSesionQuery(id, ObtenerJugadorId()),
            cancellationToken);

        return Ok(new EstadoTriviaSesionResponse(
            dto.Fase,
            dto.Orden,
            dto.PreguntaId,
            dto.Enunciado,
            dto.Dificultad,
            dto.Opciones,
            dto.TimerCerradoEnUtc,
            dto.TransicionHastaUtc,
            dto.PreguntaIndexActual,
            dto.TotalPreguntas,
            dto.YaRespondio,
            dto.IndiceOpcionSeleccionada,
            dto.UltimaRespuestaEsCorrecta,
            dto.UltimaRespuestaFueraDeTiempo,
            dto.UltimaRespuestaPuntos,
            dto.PuntajeTotalParticipante));
    }

    /// <summary>HU-34/35 — encola respuesta trivia (202 Accepted).</summary>
    [HttpPost("{id:guid}/trivia/respuestas")]
    [ProducesResponseType(typeof(SubmitRespuestaTriviaResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> EnviarRespuestaTrivia(
        Guid id,
        [FromBody] SubmitRespuestaTriviaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SubmitRespuestaTriviaCommand(
                id,
                ObtenerJugadorId(),
                request.PreguntaId,
                request.IndiceOpcion,
                request.DuracionTimerSegundos),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            value => new AcceptedResult(
                $"/api/v1/sesiones/{id}/trivia/estado",
                new SubmitRespuestaTriviaResponse(value.MessageId, value.Status)));
    }

    private Guid ObtenerJugadorId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : TestAuthHandler.DefaultParticipanteId;
}
