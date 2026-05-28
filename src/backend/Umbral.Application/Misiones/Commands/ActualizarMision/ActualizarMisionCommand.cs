using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.ActualizarMision;

public sealed record ActualizarMisionCommand(
    Guid MisionId,
    string Nombre,
    bool? Activar) : IRequest<Result<Guid>>;
