using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Auth;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Extensions;
using Umbral.Application.Sesion.Commands.CrearSesionMision;
using Umbral.Application.Sesion.Commands.CrearSesionTrivia;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize(Roles = "Operador,Administrador")]
public sealed class CrearSesionOperadorController : ControllerBase
{
    private readonly ISender _sender;

    public CrearSesionOperadorController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(CrearSesionMisionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearSesionMision(
        [FromBody] CrearSesionMisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CrearSesionMisionCommand(request.MisionId, ObtenerOperadorId(), request.NombreSesion),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            created => new CreatedResult(
                $"/api/v1/sesiones/{created.SesionId}",
                new CrearSesionMisionResponse(
                    created.SesionId,
                    created.CodigoAcceso,
                    created.NombreSesion,
                    created.MisionNombre)));
    }

    [HttpPost("trivia")]
    [ProducesResponseType(typeof(CrearSesionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearSesionTrivia(
        [FromBody] CrearSesionTriviaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CrearSesionTriviaCommand(request.CategoriaIds, ObtenerOperadorId()),
            cancellationToken);

        return result.ToActionResult(HttpContext,
            created => new CreatedResult(
                $"/api/v1/sesiones/{created.Id}",
                new CrearSesionResponse(created.Id, created.CodigoAcceso)));
    }

    private Guid ObtenerOperadorId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : TestAuthHandler.DefaultOperadorId;
}
