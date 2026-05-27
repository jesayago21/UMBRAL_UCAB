namespace Umbral.Application.Sesion.Commands.RegistrarEquipo;

/// <summary>
/// Respuesta HU-13: identificador del equipo y código para unirse desde mobile.
/// </summary>
public sealed record RegistrarEquipoResult(
    Guid EquipoId,
    string CodigoAcceso);
