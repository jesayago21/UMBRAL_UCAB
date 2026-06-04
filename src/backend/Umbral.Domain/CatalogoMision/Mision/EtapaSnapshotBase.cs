using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public abstract class EtapaSnapshotBase : ValueObject
{
    public EtapaId EtapaId { get; }
    public int Orden { get; }
    public abstract TipoEtapa Tipo { get; }

    protected EtapaSnapshotBase(EtapaId etapaId, int orden)
    {
        EtapaId = etapaId;
        Orden   = orden;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EtapaId;
        yield return Orden;
        yield return Tipo;
    }
}
