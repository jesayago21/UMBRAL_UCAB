using MediatR;
using Umbral.Application.Sesion.Models;

namespace Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;

public sealed record GetMiInscripcionParticipanteQuery(Guid JugadorId)
    : IRequest<MiInscripcionParticipanteDto?>;

/// <summary>Metadata de countdown PorTiempo sin revelar el contenido (HU-17 / RB-07).</summary>
public sealed record PistaPorTiempoPendienteDto(Guid PistaId, int SegundosLiberacion);

/// <summary>Penalización aplicada a este participante (motivo visible en tablero).</summary>
public sealed record PenalizacionParticipanteDto(
    int Puntos,
    string Motivo,
    DateTime OcurridoEn);

public sealed record MiInscripcionParticipanteDto(
    Guid SesionId,
    string Titulo,
    Guid ParticipanteId,
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaSesionDto> Etapas,
    DateTimeOffset? EtapaIniciadaEn = null,
    int SegundosPausaAcumulados = 0,
    DateTimeOffset? PausadaDesde = null,
    IReadOnlyList<PistaPorTiempoPendienteDto>? PistasPorTiempoPendientes = null,
    IReadOnlyList<PenalizacionParticipanteDto>? Penalizaciones = null);
