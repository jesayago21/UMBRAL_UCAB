using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.RegistrarEquipo;

/// <summary>
/// HU-13 — Operador inscribe un equipo en la sesión.
/// </summary>
public sealed record RegistrarEquipoCommand(
    Guid SesionId,
    string NombreEquipo) : IRequest<Result<RegistrarEquipoResult>>;
