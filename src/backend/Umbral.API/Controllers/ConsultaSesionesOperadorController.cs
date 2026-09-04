using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.Application.Sesion.Models;
using Umbral.Application.Sesion.Queries.GetSesionOperador;
using Umbral.Application.Sesion.Queries.ListSesionesOperador;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class ConsultaSesionesOperadorController : ControllerBase
{
    private readonly ISender _sender;

    public ConsultaSesionesOperadorController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SesionResumenResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new ListSesionesOperadorQuery(ObtenerOperadorId()), cancellationToken);
        return Ok(items.Select(MapResumen).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SesionDetalleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var sesion = await _sender.Send(new GetSesionOperadorQuery(id), cancellationToken);
        return Ok(MapDetalle(sesion));
    }

    private Guid ObtenerOperadorId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : TestAuthHandler.DefaultOperadorId;

    private static SesionResumenResponse MapResumen(SesionResumenDto dto) =>
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
            dto.EtapaActivaTipo,
            dto.MaxParticipantes);

    private static SesionDetalleResponse MapDetalle(SesionDetalleDto dto) =>
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
            dto.Etapas?.Select(MapEtapa).ToList(),
            dto.TriviaFase,
            dto.MaxParticipantes);

    private static EtapaSesionResponse MapEtapa(EtapaSesionDto e) =>
        new(
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
            e.RadioMetros,
            e.CodigoQrSolucion);
}
