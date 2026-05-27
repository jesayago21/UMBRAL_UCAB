using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.FinalizarSesion;

/// <summary>
/// HU-23 — Operador finaliza la sesión.
/// </summary>
public sealed record FinalizarSesionCommand(Guid SesionId) : IRequest<Result<Guid>>;
