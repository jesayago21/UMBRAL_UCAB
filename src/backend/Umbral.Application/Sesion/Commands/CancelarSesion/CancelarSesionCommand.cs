using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.CancelarSesion;

/// <summary>
/// HU-23 — Operador cancela la sesión con motivo.
/// </summary>
public sealed record CancelarSesionCommand(
    Guid SesionId,
    string Motivo) : IRequest<Result<Guid>>;
