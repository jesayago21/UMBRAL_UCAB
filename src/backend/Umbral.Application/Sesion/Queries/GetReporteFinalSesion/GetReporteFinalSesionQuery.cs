using MediatR;
using Umbral.Application.Sesion.Queries.GetRankingSesion;

namespace Umbral.Application.Sesion.Queries.GetReporteFinalSesion;

public sealed record GetReporteFinalSesionQuery(Guid SesionId)
    : IRequest<ReporteFinalSesionDto>;

public sealed record ReporteFinalSesionDto(
    Guid SesionId,
    string Estado,
    DateTime? FinalizadaEn,
    IReadOnlyList<PosicionRankingDto> Ranking);
