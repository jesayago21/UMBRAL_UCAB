using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;
using Umbral.Application.Sesion.Commands.IniciarSesion;
using Umbral.Application.Sesion.Commands.PausarSesion;
using Umbral.Application.Sesion.Commands.ReanudarSesion;
using Umbral.Application.Sesion.Commands.RegistrarEquipo;

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

    private Guid ObtenerOperadorId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var operadorId)
            ? operadorId
            : TestAuthHandler.DefaultOperadorId;
    }
}
