using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>
/// Se emite cuando una sesión en estado Preparacion pasa a Activa.
/// </summary>
public sealed record SesionIniciada(
    SesionId SesionId,
    DateTime OcurridoEn) : IDomainEvent;
