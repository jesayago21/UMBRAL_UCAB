using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.EliminarMision;

public sealed record EliminarMisionCommand(Guid MisionId) : IRequest<Result<Guid>>;
