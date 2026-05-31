using MediatR;
using Umbral.Application.Misiones.Models;

namespace Umbral.Application.Misiones.Queries.ListMisionesActivas;

public sealed record ListMisionesActivasQuery : IRequest<IReadOnlyList<MisionActivaDto>>;
