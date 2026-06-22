using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.AbandonarSesion;
using Umbral.Application.Sesion.Commands.UnirseSesion;
using Umbral.Application.Sesion.Models;
using Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;
using Umbral.Application.Sesion.Queries.ListSesionesDisponiblesParticipante;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Participante")]
public sealed class InscripcionParticipanteController : ControllerBase
{
    private readonly ISender _sender;

    public InscripcionParticipanteController(ISender sender) => _sender = sender;

    [HttpGet("disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<SesionDisponibleParticipanteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDisponibles(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new ListSesionesDisponiblesParticipanteQuery(null), cancellationToken);
        return Ok(items.Select(x =>
            new SesionDisponibleParticipanteResponse(x.Id, x.Titulo, x.Estado, x.ParticipantesInscritos)).ToList());
    }

    [HttpGet("mi-inscripcion")]
    [ProducesResponseType(typeof(MiInscripcionParticipanteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MiInscripcion(CancellationToken cancellationToken)
    {
        var inscripcion = await _sender.Send(
            new GetMiInscripcionParticipanteQuery(ObtenerJugadorId()),
            cancellationToken);

        if (inscripcion is null)
            return NoContent();

        return Ok(new MiInscripcionParticipanteResponse(
            inscripcion.SesionId,
            inscripcion.Titulo,
            inscripcion.ParticipanteId,
            inscripcion.Estado,
            inscripcion.TotalEtapas,
            inscripcion.Etapas.Select(MapEtapa).ToList()));
    }

    [HttpPost("{id:guid}/unirse")]
    [ProducesResponseType(typeof(UnirseSesionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Unirse(
        Guid id,
        [FromBody] UnirseSesionRequest request,
        CancellationToken cancellationToken)
    {
        var nombre = string.IsNullOrWhiteSpace(request.NombreParticipante)
            ? User.FindFirstValue("preferred_username")
              ?? User.FindFirstValue(ClaimTypes.Name)
              ?? "Participante"
            : request.NombreParticipante.Trim();

        var result = await _sender.Send(
            new UnirseSesionCommand(id, request.CodigoAcceso, ObtenerJugadorId(), nombre),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            joined => new CreatedResult(
                $"/api/v1/sesiones/{id}/participantes/{joined.ParticipanteId}",
                new UnirseSesionResponse(joined.ParticipanteId)));
    }

    [HttpPost("{id:guid}/abandonar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Abandonar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AbandonarSesionCommand(id, ObtenerJugadorId()),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    private Guid ObtenerJugadorId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : TestAuthHandler.DefaultParticipanteId;

    private static EtapaSesionResponse MapEtapa(EtapaSesionDto e) =>
        new(
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
            e.CategoriaIds);
}
