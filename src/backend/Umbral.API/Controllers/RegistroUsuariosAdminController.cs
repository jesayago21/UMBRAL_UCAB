using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Usuarios;
using Umbral.API.Extensions;
using Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class RegistroUsuariosAdminController : ControllerBase
{
    private readonly ISender _sender;

    public RegistroUsuariosAdminController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(CrearUsuarioResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CrearUsuarioCommand(
                request.Email,
                request.Username,
                request.Nombre,
                request.Apellido,
                request.Roles),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            created => new CreatedResult(
                $"/api/v1/usuarios/{created.KeycloakUserId}",
                new CrearUsuarioResponse(
                    created.KeycloakUserId,
                    created.Email,
                    request.Username,
                    request.Nombre,
                    request.Apellido,
                    created.Estado,
                    created.Roles)));
    }
}
