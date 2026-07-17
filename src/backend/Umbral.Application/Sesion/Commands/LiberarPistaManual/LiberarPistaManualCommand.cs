using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.LiberarPistaManual;

/// <summary>
/// RF-15 — Operador escribe una pista ad-hoc y la entrega a uno o a todos.
/// </summary>
public sealed record LiberarPistaManualCommand(
    Guid SesionId,
    string Contenido,
    Guid? ParticipanteId) : IRequest<Result<int>>;
