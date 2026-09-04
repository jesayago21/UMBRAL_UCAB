using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Usuarios;
using Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;
using Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class ConsultaUsuariosAdminController : ControllerBase
{
    private readonly ISender _sender;

    public ConsultaUsuariosAdminController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var items = await _sender.Send(new ListUsuariosQuery(page, pageSize), cancellationToken);
        return Ok(items.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(new GetUsuarioByIdQuery(id), cancellationToken);
        if (usuario is null)
            return NotFound();
        return Ok(Map(usuario));
    }

    private static UsuarioResponse Map(Application.IdentidadYAccesos.Models.UsuarioDto dto) =>
        new(
            dto.KeycloakUserId,
            dto.Email,
            dto.Username,
            dto.Nombre,
            dto.Apellido,
            dto.Estado,
            dto.Roles);
}
