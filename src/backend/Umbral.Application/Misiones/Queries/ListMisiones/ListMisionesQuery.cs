using MediatR;
using Umbral.Application.Misiones.Models;

namespace Umbral.Application.Misiones.Queries.ListMisiones;

public sealed record ListMisionesQuery(
    string? Nombre,
    string? Estado) : IRequest<IReadOnlyList<MisionDto>>;
