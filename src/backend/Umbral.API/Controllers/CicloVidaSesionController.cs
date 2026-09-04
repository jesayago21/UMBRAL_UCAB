using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.AbrirInscripcionSesion;
using Umbral.Application.Sesion.Commands.CancelarSesion;
using Umbral.Application.Sesion.Commands.FinalizarSesion;
using Umbral.Application.Sesion.Commands.IniciarSesion;
using Umbral.Application.Sesion.Commands.PausarSesion;
using Umbral.Application.Sesion.Commands.ReanudarSesion;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class CicloVidaSesionController : ControllerBase
{
    private readonly ISender _sender;

    public CicloVidaSesionController(ISender sender) => _sender = sender;

    [HttpPost("{id:guid}/abrir-inscripcion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AbrirInscripcion(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AbrirInscripcionSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/iniciar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Iniciar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new IniciarSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/pausar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Pausar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PausarSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/reanudar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reanudar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReanudarSesionCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Finalizar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FinalizarSesionCommand(id), cancellationToken);
        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }

    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancelar(
        Guid id,
        [FromBody] CancelarSesionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelarSesionCommand(id, request.Motivo), cancellationToken);
        return result.ToActionResult(HttpContext, _ => new NoContentResult());
    }
}
