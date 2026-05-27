using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Equipo participante dentro de una sesión.
/// Iter 1: estructura mínima (id + nombre).
/// Iter 2 añadirá puntaje, penalizaciones y lógica de registro.
/// </summary>
public sealed class EquipoSesion : Entity<EquipoSesionId>
{
    public string Nombre { get; }

    private EquipoSesion(EquipoSesionId id, string nombre) : base(id)
    {
        Nombre = nombre;
    }

    public static EquipoSesion Crear(EquipoSesionId id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del equipo no puede estar vacío.");

        return new EquipoSesion(id, nombre.Trim());
    }
}
