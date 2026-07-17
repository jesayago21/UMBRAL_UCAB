using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetRankingSesion;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetReporteFinalSesion;

internal sealed class GetReporteFinalSesionQueryHandler
    : IRequestHandler<GetReporteFinalSesionQuery, ReporteFinalSesionDto>
{
    private readonly ISesionRepository _sesionRepository;

    public GetReporteFinalSesionQueryHandler(ISesionRepository sesionRepository) =>
        _sesionRepository = sesionRepository;

    public async Task<ReporteFinalSesionDto> Handle(
        GetReporteFinalSesionQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        var ranking = sesion.ObtenerRankingFinal()
            .Select(x => new PosicionRankingDto(
                x.Posicion,
                x.ParticipanteId.Valor,
                x.NombreParticipante,
                x.PuntajeTotal,
                x.TiempoAcumuladoMs))
            .ToList();

        return new ReporteFinalSesionDto(
            query.SesionId,
            sesion.Estado.ToString(),
            sesion.FinalizadaEn,
            ranking);
    }
}
