using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Usuarios;
using Umbral.API.Extensions;
using Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class GestionRolesAdminController : ControllerBase
{
    private readonly ISender _sender;

    public GestionRolesAdminController(ISender sender) => _sender = sender;

    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AsignarRoles(
        Guid id,
        [FromBody] AsignarRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AsignarRolesUsuarioCommand(id, request.Roles),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }
}
