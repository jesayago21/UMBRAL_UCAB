using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.EliminarPistaEtapa;

public sealed record EliminarPistaEtapaCommand(
    Guid MisionId,
    Guid EtapaId,
    Guid PistaId) : IRequest<Result<Unit>>;
