using MediatR;

namespace Umbral.Application.Sesion.Queries.GetRankingSesion;

/// <summary>
/// HU-21/HU-23 — Consulta ranking de sesión.
/// </summary>
public sealed record GetRankingSesionQuery(Guid SesionId) : IRequest<IReadOnlyList<PosicionRankingDto>>;
