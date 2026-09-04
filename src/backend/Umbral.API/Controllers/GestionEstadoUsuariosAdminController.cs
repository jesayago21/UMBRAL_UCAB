using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Usuarios;
using Umbral.API.Extensions;
using Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class GestionEstadoUsuariosAdminController : ControllerBase
{
    private readonly ISender _sender;

    public GestionEstadoUsuariosAdminController(ISender sender) => _sender = sender;

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ActualizarUsuarioCommand(
                id,
                request.Nombre,
                request.Apellido,
                request.Rol,
                request.NuevaPassword),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        [FromBody] CambiarEstadoUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CambiarEstadoUsuarioCommand(id, request.Accion),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new EliminarUsuarioCommand(id, ObtenerSolicitanteId()),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    private Guid ObtenerSolicitanteId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var gid)
            ? gid
            : TestAuthHandler.DefaultOperadorId;
}
