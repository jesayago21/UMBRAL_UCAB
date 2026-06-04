using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Usuarios;
using Umbral.API.Extensions;
using Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;
using Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;
using Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(CrearUsuarioResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CrearUsuarioCommand(
                request.Email,
                request.Username,
                request.Nombre,
                request.Apellido,
                request.PasswordTemporal,
                request.Roles),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            created => new CreatedResult(
                $"/api/v1/usuarios/{created.UsuarioId}",
                new CrearUsuarioResponse(
                    created.UsuarioId,
                    created.KeycloakUserId,
                    created.Email,
                    request.Username,
                    request.Nombre,
                    request.Apellido,
                    created.Estado,
                    request.Roles,
                    request.PasswordTemporal)));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var items = await _mediator.Send(new ListUsuariosQuery(page, pageSize), cancellationToken);
        return Ok(items.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _mediator.Send(new GetUsuarioByIdQuery(id), cancellationToken);
        if (usuario is null)
            return NotFound();
        return Ok(Map(usuario));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ActualizarUsuarioCommand(
                id,
                request.Nombre,
                request.Apellido,
                request.Rol,
                request.NuevaPassword),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EliminarUsuarioCommand(id), cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AsignarRoles(
        Guid id,
        [FromBody] AsignarRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AsignarRolesUsuarioCommand(id, request.Roles),
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
        var result = await _mediator.Send(
            new CambiarEstadoUsuarioCommand(id, request.Accion),
            cancellationToken);
        return result.ToNoContentResult(HttpContext);
    }

    private static UsuarioResponse Map(Application.IdentidadYAccesos.Models.UsuarioDto dto) =>
        new(
            dto.Id,
            dto.KeycloakUserId,
            dto.Email,
            dto.Username,
            dto.Nombre,
            dto.Apellido,
            dto.Estado,
            dto.Roles);
}
