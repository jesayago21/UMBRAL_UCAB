using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.AplicarPenalizacion;

/// <summary>
/// HU-16 — Operador aplica penalización a un participante en sesión activa.
/// </summary>
public sealed record AplicarPenalizacionCommand(
    Guid SesionId,
    Guid ParticipanteId,
    int Puntos,
    string Motivo,
    Guid OperadorId) : IRequest<Result<Guid>>;
