using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.ExpulsarParticipante;

/// <summary>HU-32 — operador expulsa participante de la sala de espera.</summary>
public sealed record ExpulsarParticipanteCommand(
    Guid SesionId,
    Guid ParticipanteId,
    string Motivo) : IRequest<Result<Unit>>;
