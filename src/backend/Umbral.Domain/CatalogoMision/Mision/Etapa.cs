using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public abstract class Etapa : Entity
{
    public EtapaId EtapaId { get; protected set; } = default!;
    public MisionId MisionId { get; protected set; } = default!;
    public int Orden { get; protected set; }

    public abstract TipoEtapa Tipo { get; }

    internal void AsignarOrden(int orden)
    {
        if (orden < 1)
            throw new DomainException("El orden de la etapa debe ser al menos 1.");

        Orden = orden;
    }

    protected override bool IdEquals(Entity other) =>
        other is Etapa e && e.EtapaId == EtapaId;

    protected override int GetIdHashCode() => EtapaId.GetHashCode();
}
