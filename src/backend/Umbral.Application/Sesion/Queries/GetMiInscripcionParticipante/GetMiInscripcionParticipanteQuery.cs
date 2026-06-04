using MediatR;
using Umbral.Application.Sesion.Models;

namespace Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;

public sealed record GetMiInscripcionParticipanteQuery(Guid JugadorId)
    : IRequest<MiInscripcionParticipanteDto?>;

public sealed record MiInscripcionParticipanteDto(
    Guid SesionId,
    string Titulo,
    Guid ParticipanteId,
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaSesionDto> Etapas);
