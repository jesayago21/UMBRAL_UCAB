using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.PausarSesion;

/// <summary>
/// HU-15 — Operador pausa la sesión (Activa → Pausada).
/// </summary>
public sealed record PausarSesionCommand(Guid SesionId) : IRequest<Result<Guid>>;
