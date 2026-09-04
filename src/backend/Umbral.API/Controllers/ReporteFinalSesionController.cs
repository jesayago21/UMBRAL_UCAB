using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.Application.Sesion.Queries.GetReporteFinalSesion;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class ReporteFinalSesionController : ControllerBase
{
    private readonly ISender _sender;

    public ReporteFinalSesionController(ISender sender) => _sender = sender;

    [HttpGet("{id:guid}/reporte-final")]
    [ProducesResponseType(typeof(ReporteFinalSesionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerReporteFinal(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetReporteFinalSesionQuery(id),
            cancellationToken);

        return Ok(result);
    }
}
