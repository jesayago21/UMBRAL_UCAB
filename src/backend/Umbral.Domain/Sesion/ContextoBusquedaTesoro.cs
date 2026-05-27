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

        if (snapshot.Etapas.Count == 0)
            throw new DomainException(
                "La misión debe tener al menos una etapa para iniciar una sesión.");

        return new ContextoBusquedaTesoro
        {
            MisionSnapshot   = snapshot,
            EtapaActualIndex = 0
        };
    }

    /// <summary>Etapa activa según <see cref="EtapaActualIndex"/> (RB-06).</summary>
    public EtapaSnapshot ObtenerEtapaActual()
    {
        if (EtapaActualIndex < 0 || EtapaActualIndex >= MisionSnapshot.Etapas.Count)
            throw new DomainException("No hay etapa activa en el contexto de la sesión.");

        return MisionSnapshot.Etapas[EtapaActualIndex];
    }

    public bool EsUltimaEtapa() =>
        EtapaActualIndex >= MisionSnapshot.Etapas.Count - 1;

    protected override bool IdEquals(Entity other) =>
        other is ContextoBusquedaTesoro c && c._id == _id;

    protected override int GetIdHashCode() => _id.GetHashCode();
}
