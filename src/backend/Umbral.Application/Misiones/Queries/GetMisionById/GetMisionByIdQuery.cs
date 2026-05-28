using MediatR;
using Umbral.Application.Misiones.Models;

namespace Umbral.Application.Misiones.Queries.GetMisionById;

public sealed record GetMisionByIdQuery(Guid MisionId) : IRequest<MisionDto>;
