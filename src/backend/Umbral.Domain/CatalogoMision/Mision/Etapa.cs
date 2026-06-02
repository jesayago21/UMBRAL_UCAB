using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public abstract class Etapa : Entity
{
    public EtapaId EtapaId { get; protected set; } = default!;
    public MisionId MisionId { get; protected set; } = default!;
    public int Orden { get; protected set; }

    public abstract TipoEtapa Tipo { get; }

    protected override bool IdEquals(Entity other) =>
        other is Etapa e && e.EtapaId == EtapaId;

    protected override int GetIdHashCode() => EtapaId.GetHashCode();
}
