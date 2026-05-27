using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.IniciarSesion;

/// <summary>
/// HU-14 — Operador inicia la sesión (EnPreparacion → Activa). RB-18.
/// </summary>
public sealed record IniciarSesionCommand(Guid SesionId) : IRequest<Result<Guid>>;
