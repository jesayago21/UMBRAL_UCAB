using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.AplicarPenalizacion;
using Umbral.Application.Sesion.Commands.CancelarSesion;
using Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;
using Umbral.Application.Sesion.Commands.FinalizarSesion;
using Umbral.Application.Sesion.Commands.IniciarSesion;
using Umbral.Application.Sesion.Commands.PausarSesion;
using Umbral.Application.Sesion.Commands.ReanudarSesion;
using Umbral.Application.Sesion.Commands.RegistrarEquipo;
using Umbral.Application.Sesion.Commands.SubmitEvidencia;
using Umbral.Application.Sesion.Queries.GetRankingSesion;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize]
public sealed class SesionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SesionesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("busqueda-tesoro")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(CrearSesionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearBusquedaTesoro(
        [FromBody] CrearSesionBusquedaTesoroRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearSesionBusquedaTesoroCommand(
                request.MisionId,
                ObtenerOperadorId()),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            id => new CreatedResult(
                $"/api/v1/sesiones/{id}",
                new CrearSesionResponse(id)));
    }

    [HttpPost("{id:guid}/equipos")]
    [Authorize(Roles = "Operador,Administrador")]
    [ProducesResponseType(typeof(RegistrarEquipoResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarEquipo(
        Guid id,
        [FromBody] RegistrarEquipoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegistrarEquipoCommand(id, request.NombreEquipo),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            equipo => new CreatedResult(
                $"/api/v1/sesiones/{id}/equipos/{equipo.EquipoId}",
                new RegistrarEquipoResponse(equipo.EquipoId, equipo.CodigoAcceso)));
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
                request.EquipoId,
                request.Puntos,
                request.Motivo,
                ObtenerOperadorId()),
            cancellationToken);

        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpPost("{id:guid}/evidencias")]
    [Authorize(Roles = "EquipoParticipante")]
    [ProducesResponseType(typeof(SubmitEvidenciaResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitEvidencia(
        Guid id,
        [FromBody] SubmitEvidenciaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitEvidenciaCommand(id, request.EquipoId, request.CodigoQr),
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

    [HttpGet("{id:guid}/ranking")]
    [ProducesResponseType(typeof(IReadOnlyList<PosicionRankingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRanking(Guid id, CancellationToken cancellationToken)
    {
        var ranking = await _mediator.Send(new GetRankingSesionQuery(id), cancellationToken);

        var response = ranking
            .Select(x => new PosicionRankingResponse(
                x.Posicion,
                x.EquipoId,
                x.NombreEquipo,
                x.PuntajeTotal))
            .ToList();

        return Ok(response);
    }

    private Guid ObtenerOperadorId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var operadorId)
            ? operadorId
            : TestAuthHandler.DefaultOperadorId;
    }
}
