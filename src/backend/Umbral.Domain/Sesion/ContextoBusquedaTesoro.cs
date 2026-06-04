using Umbral.Domain.CatalogoMision.Mision;
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
    public ParticipanteId? GanadorEtapaActualId { get; private set; }

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

    /// <summary>Reconstitución desde persistencia (no usar en lógica de negocio).</summary>
    internal static ContextoBusquedaTesoro Rehydrate(
        MisionSnapshot snapshot,
        int etapaActualIndex,
        ParticipanteId? ganadorEtapaActualId) =>
        new()
        {
            MisionSnapshot         = snapshot,
            EtapaActualIndex       = etapaActualIndex,
            GanadorEtapaActualId   = ganadorEtapaActualId
        };

    /// <summary>Etapa activa según <see cref="EtapaActualIndex"/> (RB-06).</summary>
    public EtapaBusquedaTesoroSnapshot ObtenerEtapaActual()
    {
        if (EtapaActualIndex < 0 || EtapaActualIndex >= MisionSnapshot.Etapas.Count)
            throw new DomainException("No hay etapa activa en el contexto de la sesión.");

        var etapa = MisionSnapshot.Etapas[EtapaActualIndex];
        if (etapa is not EtapaBusquedaTesoroSnapshot bt)
            throw new DomainException("La etapa activa legacy no es de Búsqueda del Tesoro.");
        return bt;
    }

    public bool EsUltimaEtapa() =>
        EtapaActualIndex >= MisionSnapshot.Etapas.Count - 1;

    public bool YaHayGanadorEnEtapaActual() => GanadorEtapaActualId is not null;

    internal void RegistrarGanadorEtapa(ParticipanteId ganadorId)
    {
        ArgumentNullException.ThrowIfNull(ganadorId);

        if (GanadorEtapaActualId is not null)
            throw new DomainException("Ya existe un ganador en la etapa actual.");

        GanadorEtapaActualId = ganadorId;
    }

    /// <summary>Avanza al siguiente nodo tras completar la etapa actual (HU-20, RB-05).</summary>
    internal void AvanzarEtapa()
    {
        if (GanadorEtapaActualId is null)
            throw new DomainException(
                "No se puede avanzar de etapa sin un ganador registrado.");

        if (EsUltimaEtapa())
            throw new DomainException("No hay más etapas en la misión.");

        EtapaActualIndex++;
        GanadorEtapaActualId = null;
    }

    protected override bool IdEquals(Entity other) =>
        other is ContextoBusquedaTesoro c && c._id == _id;

    protected override int GetIdHashCode() => _id.GetHashCode();
}
