namespace Umbral.Domain.Sesion;

public sealed record RespuestaTriviaId(Guid Valor)
{
    public static RespuestaTriviaId Nuevo() => new(Guid.NewGuid());
}
