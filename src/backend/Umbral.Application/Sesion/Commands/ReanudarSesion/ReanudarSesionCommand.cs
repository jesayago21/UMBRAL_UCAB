using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.ReanudarSesion;

/// <summary>
/// HU-15 — Operador reanuda la sesión (Pausada → Activa).
/// </summary>
public sealed record ReanudarSesionCommand(Guid SesionId) : IRequest<Result<Guid>>;
