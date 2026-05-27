namespace Umbral.Domain.Sesion;

public sealed record UsuarioId(Guid Valor)
{
    public static UsuarioId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
