using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetRankingSesion;

internal sealed class GetRankingSesionQueryHandler
    : IRequestHandler<GetRankingSesionQuery, IReadOnlyList<PosicionRankingDto>>
{
    private readonly ISesionRepository _sesionRepository;

    public GetRankingSesionQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<IReadOnlyList<PosicionRankingDto>> Handle(
        GetRankingSesionQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        return RankingService.Calcular(sesion.Equipos)
            .Select(x => new PosicionRankingDto(
                x.Posicion,
                x.EquipoId.Valor,
                x.NombreEquipo,
                x.PuntajeTotal))
            .ToList();
    }
}
