using MediatR;
using Umbral.Application.Sesion.Models;

namespace Umbral.Application.Sesion.Queries.ListSesionesOperador;

public sealed record ListSesionesOperadorQuery(Guid OperadorId)
    : IRequest<IReadOnlyList<SesionResumenDto>>;
