using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.AplicarPenalizacion;

/// <summary>
/// HU-16 — Operador aplica penalización a un equipo en sesión activa.
/// </summary>
public sealed record AplicarPenalizacionCommand(
    Guid SesionId,
    Guid EquipoId,
    int Puntos,
    string Motivo,
    Guid OperadorId) : IRequest<Result<Guid>>;
