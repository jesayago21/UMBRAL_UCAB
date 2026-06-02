using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Contexto unificado de ejecución de una sesión de misión (etapas polimórficas secuenciales).
/// </summary>
public sealed class ContextoMision : Entity
{
    private readonly Guid _id = Guid.NewGuid();

    public MisionId MisionId { get; private set; } = default!;
    public MisionSnapshot MisionSnapshot { get; private set; } = default!;
    public int EtapaActualIndex { get; private set; }
    public ParticipanteId? GanadorEtapaActualId { get; private set; }
    public int PreguntaTriviaActualIndex { get; private set; }

    private ContextoMision() { }

    internal static ContextoMision Crear(MisionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Etapas.Count == 0)
            throw new DomainException(
                "La misión debe tener al menos una etapa para iniciar una sesión.");

        return new ContextoMision
        {
            MisionId         = snapshot.MisionId,
            MisionSnapshot   = snapshot,
            EtapaActualIndex = 0,
            PreguntaTriviaActualIndex = 0
        };
    }

    internal static ContextoMision Rehydrate(
        MisionId misionId,
        MisionSnapshot snapshot,
        int etapaActualIndex,
        ParticipanteId? ganadorEtapaActualId,
        int preguntaTriviaActualIndex) =>
        new()
        {
            MisionId                  = misionId,
            MisionSnapshot            = snapshot,
            EtapaActualIndex          = etapaActualIndex,
            GanadorEtapaActualId      = ganadorEtapaActualId,
            PreguntaTriviaActualIndex = preguntaTriviaActualIndex
        };

    public EtapaSnapshotBase ObtenerEtapaActual()
    {
        if (EtapaActualIndex < 0 || EtapaActualIndex >= MisionSnapshot.Etapas.Count)
            throw new DomainException("No hay etapa activa en el contexto de la sesión.");

        return MisionSnapshot.Etapas[EtapaActualIndex];
    }

    public EtapaBusquedaTesoroSnapshot ObtenerEtapaBusquedaTesoroActual()
    {
        var etapa = ObtenerEtapaActual();
        if (etapa is not EtapaBusquedaTesoroSnapshot bt)
            throw new DomainException("La etapa activa no es de Búsqueda del Tesoro.");

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

    internal void AvanzarEtapa()
    {
        if (ObtenerEtapaActual() is EtapaBusquedaTesoroSnapshot && GanadorEtapaActualId is null)
            throw new DomainException(
                "No se puede avanzar de etapa BT sin un ganador registrado.");

        if (EsUltimaEtapa())
            throw new DomainException("No hay más etapas en la misión.");

        EtapaActualIndex++;
        GanadorEtapaActualId = null;
        PreguntaTriviaActualIndex = 0;
    }

    protected override bool IdEquals(Entity other) =>
        other is ContextoMision c && c._id == _id;

    protected override int GetIdHashCode() => _id.GetHashCode();
}
