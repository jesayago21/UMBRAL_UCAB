namespace Umbral.Domain.CatalogoTrivia.Pregunta;

public sealed record PreguntaId(Guid Valor)
{
    public static PreguntaId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
