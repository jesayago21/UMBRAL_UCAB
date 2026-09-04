using MediatR;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Queries.ListSesionesDisponiblesParticipante;

public sealed record ListSesionesDisponiblesParticipanteQuery(TipoSesion? TipoSesion)
    : IRequest<IReadOnlyList<SesionDisponibleParticipanteDto>>;

public sealed record SesionDisponibleParticipanteDto(
    Guid Id,
    string Titulo,
    string Estado,
    int ParticipantesInscritos,
    int MaxParticipantes);
