using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Contracts.Sesiones;
using Umbral.Application.Sesion.Queries.GetRankingSesion;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
[Authorize]
public sealed class RankingSesionController : ControllerBase
{
    private readonly ISender _sender;

    public RankingSesionController(ISender sender) => _sender = sender;

    [HttpGet("{id:guid}/ranking")]
    [ProducesResponseType(typeof(IReadOnlyList<PosicionRankingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRanking(Guid id, CancellationToken cancellationToken)
    {
        var ranking = await _sender.Send(new GetRankingSesionQuery(id), cancellationToken);

        return Ok(ranking
            .Select(x => new PosicionRankingResponse(
                x.Posicion,
                x.ParticipanteId,
                x.NombreParticipante,
                x.PuntajeTotal))
            .ToList());
    }
}
