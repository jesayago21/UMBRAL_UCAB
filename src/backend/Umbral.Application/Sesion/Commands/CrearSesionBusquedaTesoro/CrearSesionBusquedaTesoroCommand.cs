using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;

/// <summary>
/// HU-12 — Operador crea sesión BusquedaTesoro desde una misión activa.
/// </summary>
public sealed record CrearSesionBusquedaTesoroCommand(
    Guid MisionId,
    Guid OperadorId) : IRequest<Result<Guid>>;
