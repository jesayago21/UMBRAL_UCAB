using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Contexto de ejecución de una sesión de tipo BusquedaTesoro.
/// Solo existe cuando TipoSesion == BusquedaTesoro.
/// Contiene la copia inmutable de la misión (MisionSnapshot) y el estado
/// de avance de etapas. Métodos de avance se añaden en iteraciones posteriores.
/// </summary>
public sealed class ContextoBusquedaTesoro : Entity
{
    private readonly Guid _id = Guid.NewGuid();

    public MisionSnapshot MisionSnapshot { get; private set; } = default!;
    public int EtapaActualIndex { get; private set; }

    private ContextoBusquedaTesoro() { }

    internal static ContextoBusquedaTesoro Crear(MisionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new ContextoBusquedaTesoro
        {
            MisionSnapshot   = snapshot,
            EtapaActualIndex = 0
        };
    }

    protected override bool IdEquals(Entity other) =>
        other is ContextoBusquedaTesoro c && c._id == _id;

    protected override int GetIdHashCode() => _id.GetHashCode();
}
