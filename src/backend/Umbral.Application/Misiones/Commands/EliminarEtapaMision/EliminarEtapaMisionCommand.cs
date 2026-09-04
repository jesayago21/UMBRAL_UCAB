using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.EliminarEtapaMision;

public sealed record EliminarEtapaMisionCommand(
    Guid MisionId,
    Guid EtapaId) : IRequest<Result<Unit>>;
