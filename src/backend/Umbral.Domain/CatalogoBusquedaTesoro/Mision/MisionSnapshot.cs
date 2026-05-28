using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

/// <summary>
/// Copia inmutable de la misión en el momento de crear la sesión.
/// Los cambios posteriores a Mision NO afectan sesiones en curso (DA-05).
/// Es el contrato del Anti-Corruption Layer entre CatalogoBusquedaTesoro y el BC Sesion.
/// </summary>
public sealed class MisionSnapshot : ValueObject
{
    public MisionId MisionId { get; }
    public string Nombre { get; }
    public IReadOnlyList<EtapaSnapshot> Etapas { get; }

    private MisionSnapshot(
        MisionId misionId,
        string nombre,
        IReadOnlyList<EtapaSnapshot> etapas)
    {
        MisionId = misionId;
        Nombre   = nombre;
        Etapas   = etapas;
    }

    public static MisionSnapshot Desde(Mision mision)
    {
        ArgumentNullException.ThrowIfNull(mision);

        var etapas = mision.Etapas
            .OrderBy(e => e.Orden)
            .Select(EtapaSnapshot.Desde)
            .ToList()
            .AsReadOnly();

        return new MisionSnapshot(mision.MisionId, mision.Nombre, etapas);
    }

    /// <summary>Reconstitución desde persistencia (no usar en lógica de negocio).</summary>
    internal static MisionSnapshot Rehydrate(
        MisionId misionId,
        string nombre,
        IReadOnlyList<EtapaSnapshot> etapas) =>
        new(misionId, nombre, etapas);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MisionId;
        yield return Nombre;
    }
}
