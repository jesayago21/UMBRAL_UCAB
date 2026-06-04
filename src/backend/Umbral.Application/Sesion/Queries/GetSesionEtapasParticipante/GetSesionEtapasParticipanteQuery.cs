using MediatR;
using Umbral.Application.Sesion.Models;

namespace Umbral.Application.Sesion.Queries.GetSesionEtapasParticipante;

public sealed record GetSesionEtapasParticipanteQuery(Guid SesionId, Guid JugadorId)
    : IRequest<SesionEtapasParticipanteDto>;

public sealed record SesionEtapasParticipanteDto(
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaSesionDto> Etapas);
