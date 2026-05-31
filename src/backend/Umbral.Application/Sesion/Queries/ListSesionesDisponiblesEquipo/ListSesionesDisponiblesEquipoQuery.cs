using MediatR;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Queries.ListSesionesDisponiblesEquipo;

public sealed record ListSesionesDisponiblesEquipoQuery(TipoSesion TipoSesion)
    : IRequest<IReadOnlyList<SesionDisponibleEquipoDto>>;

public sealed record SesionDisponibleEquipoDto(
    Guid Id,
    string Titulo,
    string Estado,
    int EquiposInscritos);
