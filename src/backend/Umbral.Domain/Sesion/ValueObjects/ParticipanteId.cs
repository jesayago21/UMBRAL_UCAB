namespace Umbral.Domain.Sesion;

public sealed record ParticipanteId(Guid Valor)
{
    public static ParticipanteId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
