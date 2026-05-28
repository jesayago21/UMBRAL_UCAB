using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.DesactivarMision;

public sealed record DesactivarMisionCommand(Guid MisionId) : IRequest<Result<Guid>>;
