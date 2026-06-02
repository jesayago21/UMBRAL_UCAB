using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.AbandonarSesion;
using Umbral.Application.Sesion.Commands.AbrirInscripcionSesion;
using Umbral.Application.Sesion.Commands.AplicarPenalizacion;
using Umbral.Application.Sesion.Commands.CancelarSesion;
using Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;
using Umbral.Application.Sesion.Commands.CrearSesionMision;
using Umbral.Application.Sesion.Commands.CrearSesionTrivia;
using Umbral.Application.Sesion.Commands.FinalizarSesion;
using Umbral.Application.Sesion.Commands.IniciarSesion;
using Umbral.Application.Sesion.Commands.PausarSesion;
using Umbral.Application.Sesion.Commands.ReanudarSesion;
using Umbral.Application.Sesion.Commands.SubmitEvidencia;
using Umbral.Application.Sesion.Commands.UnirseSesion;
using Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;
using Umbral.Application.Sesion.Queries.GetSesionEtapasParticipante;
using Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionParticipante;
using Umbral.Application.Sesion.Queries.GetRankingSesion;
using Umbral.Application.Sesion.Queries.GetSesionOperador;
using Umbral.Application.Sesion.Queries.ListSesionesDisponiblesParticipante;
using Umbral.Application.Sesion.Queries.ListSesionesOperador;
namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize]
public sealed class SesionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SesionesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<SesionResumenResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarOperativas(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(
            new ListSesionesOperadorQuery(ObtenerUsuarioId()),
            cancellationToken);

        return Ok(items.Select(MapResumen).ToList());
    }

    [HttpGet("mi-inscripcion")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(typeof(MiInscripcionParticipanteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MiInscripcion(CancellationToken cancellationToken)
    {
        var inscripcion = await _mediator.Send(
            new GetMiInscripcionParticipanteQuery(ObtenerUsuarioId()),
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

    [HttpGet("disponibles")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(typeof(IReadOnlyList<SesionDisponibleParticipanteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDisponibles(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(
            new ListSesionesDisponiblesParticipanteQuery(null),
            cancellationToken);

        return Ok(items.Select(x => new SesionDisponibleParticipanteResponse(
            x.Id, x.Titulo, x.Estado, x.ParticipantesInscritos)).ToList());
    }

    [HttpGet("disponibles/busqueda-tesoro")]
    [Authorize(Roles = "Participante")]
    [Obsolete("Usar GET /disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<SesionDisponibleParticipanteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ListarDisponiblesBusquedaTesoro(CancellationToken cancellationToken) =>
        ListarDisponibles(cancellationToken);

    [HttpGet("disponibles/trivia")]
    [Authorize(Roles = "Participante")]
    [Obsolete("Usar GET /disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<SesionDisponibleParticipanteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ListarDisponiblesTrivia(CancellationToken cancellationToken) =>
        ListarDisponibles(cancellationToken);

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(SesionDetalleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var sesion = await _mediator.Send(new GetSesionOperadorQuery(id), cancellationToken);
        return Ok(MapDetalle(sesion));
    }

    [HttpPost]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(CrearSesionMisionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearSesionMision(
        [FromBody] CrearSesionMisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearSesionMisionCommand(
                request.MisionId,
                ObtenerUsuarioId(),
                request.NombreSesion),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            created => new CreatedResult(
                $"/api/v1/sesiones/{created.SesionId}",
                new CrearSesionMisionResponse(
                    created.SesionId,
                    created.CodigoAcceso,
                    created.NombreSesion,
                    created.MisionNombre)));
    }

    [HttpPost("busqueda-tesoro")]
    [Authorize(Roles = "Operador,Administrador")]
    [Obsolete("Usar POST /api/v1/sesiones")]
    [ProducesResponseType(typeof(CrearSesionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearBusquedaTesoro(
        [FromBody] CrearSesionBusquedaTesoroRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearSesionBusquedaTesoroCommand(
                request.MisionId,
                ObtenerUsuarioId()),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            created => new CreatedResult(
                $"/api/v1/sesiones/{created.Id}",
                new CrearSesionResponse(created.Id, created.CodigoAcceso)));
    }

    [HttpPost("trivia")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(CrearSesionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearTrivia(
        [FromBody] CrearSesionTriviaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearSesionTriviaCommand(
                request.CategoriaIds,
                ObtenerUsuarioId()),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            created => new CreatedResult(
                $"/api/v1/sesiones/{created.Id}",
                new CrearSesionResponse(created.Id, created.CodigoAcceso)));
    }

    [HttpPost("{id:guid}/abrir-inscripcion")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AbrirInscripcion(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AbrirInscripcionSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/abandonar")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Abandonar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AbandonarSesionCommand(id, ObtenerUsuarioId()),
            cancellationToken);

        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/unirse")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(typeof(UnirseSesionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Unirse(
        Guid id,
        [FromBody] UnirseSesionRequest request,
        CancellationToken cancellationToken)
    {
        var nombre = string.IsNullOrWhiteSpace(request.NombreParticipante)
            ? ObtenerNombreJugador()
            : request.NombreParticipante.Trim();

        var result = await _mediator.Send(
            new UnirseSesionCommand(
                id,
                request.CodigoAcceso,
                ObtenerUsuarioId(),
                nombre),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            joined => new CreatedResult(
                $"/api/v1/sesiones/{id}/participantes/{joined.ParticipanteId}",
                new UnirseSesionResponse(joined.ParticipanteId)));
    }

    [HttpPost("{id:guid}/iniciar")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Iniciar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new IniciarSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/pausar")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Pausar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PausarSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/reanudar")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reanudar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReanudarSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/penalizaciones")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AplicarPenalizacion(
        Guid id,
        [FromBody] AplicarPenalizacionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AplicarPenalizacionCommand(
                id,
                request.ParticipanteId,
                request.Puntos,
                request.Motivo,
                ObtenerUsuarioId()),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpPost("{id:guid}/evidencias")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(typeof(SubmitEvidenciaResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitEvidencia(
        Guid id,
        [FromBody] SubmitEvidenciaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitEvidenciaCommand(id, request.ParticipanteId, request.CodigoQr),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            value => new CreatedResult(
                $"/api/v1/sesiones/{id}/evidencias/{value.EvidenciaId}",
                new SubmitEvidenciaResponse(
                    value.EvidenciaId,
                    value.Resultado.ToString())));
    }

    [HttpPost("{id:guid}/finalizar")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Finalizar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new FinalizarSesionCommand(id), cancellationToken);
        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancelar(
        Guid id,
        [FromBody] CancelarSesionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CancelarSesionCommand(id, request.Motivo),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpGet("{id:guid}/etapas")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(typeof(SesionEtapasParticipanteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEtapasParticipante(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(
            new GetSesionEtapasParticipanteQuery(id, ObtenerUsuarioId()),
            cancellationToken);

        return Ok(new SesionEtapasParticipanteResponse(
            dto.Estado,
            dto.TotalEtapas,
            dto.Etapas.Select(MapEtapa).ToList()));
    }

    [HttpGet("{id:guid}/trivia/preguntas")]
    [Authorize(Roles = "Participante")]
    [ProducesResponseType(typeof(IReadOnlyList<PreguntaTriviaParticipanteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPreguntasTriviaParticipante(
        Guid id,
        CancellationToken cancellationToken)
    {
        var preguntas = await _mediator.Send(
            new GetPreguntasTriviaSesionParticipanteQuery(id, ObtenerUsuarioId()),
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

    [HttpGet("{id:guid}/ranking")]
    [ProducesResponseType(typeof(IReadOnlyList<PosicionRankingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRanking(Guid id, CancellationToken cancellationToken)
    {
        var ranking = await _mediator.Send(new GetRankingSesionQuery(id), cancellationToken);

        var response = ranking
            .Select(x => new PosicionRankingResponse(
                x.Posicion,
                x.ParticipanteId,
                x.NombreParticipante,
                x.PuntajeTotal))
            .ToList();

        return Ok(response);
    }

    private static SesionResumenResponse MapResumen(Application.Sesion.Models.SesionResumenDto dto) =>
        new(
            dto.Id,
            dto.Nombre,
            dto.TipoSesion,
            dto.MisionId,
            dto.MisionNombre,
            dto.Estado,
            dto.ParticipantesCount,
            dto.IniciadaEn,
            dto.FinalizadaEn,
            dto.EtapaActualOrden,
            dto.TotalEtapas,
            dto.EtapaActualDescripcion,
            dto.EtapaActivaTipo);

    private static SesionDetalleResponse MapDetalle(Application.Sesion.Models.SesionDetalleDto dto) =>
        new(
            dto.Id,
            dto.Nombre,
            dto.TipoSesion,
            dto.MisionId,
            dto.MisionNombre,
            dto.Estado,
            dto.CodigoAcceso,
            dto.IniciadaEn,
            dto.FinalizadaEn,
            dto.EtapaActualOrden,
            dto.TotalEtapas,
            dto.EtapaActualDescripcion,
            dto.EtapaActivaTipo,
            dto.Participantes
                .Select(e => new ParticipanteSesionResponse(e.ParticipanteId, e.JugadorId, e.Nombre))
                .ToList(),
            dto.Etapas?.Select(MapEtapa).ToList());

    private static EtapaSesionResponse MapEtapa(Application.Sesion.Models.EtapaSesionDto e) =>
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

    private Guid ObtenerUsuarioId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : TestAuthHandler.DefaultOperadorId;
    }

    private string ObtenerNombreJugador() =>
        User.FindFirstValue("preferred_username")
        ?? User.FindFirstValue(ClaimTypes.Name)
        ?? "Participante";
}
