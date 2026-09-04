using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.Application.Sesion.Queries.GetHistorialSesion;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class HistorialSesionController : ControllerBase
{
    private readonly ISender _sender;

    public HistorialSesionController(ISender sender) => _sender = sender;

    [HttpGet("{id:guid}/historial")]
    [ProducesResponseType(typeof(HistorialSesionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerHistorial(
        Guid id,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetHistorialSesionQuery(id, pagina, tamanoPagina),
            cancellationToken);

        return Ok(result);
    }
}
