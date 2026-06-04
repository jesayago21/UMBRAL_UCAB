namespace Umbral.Domain.CatalogoTrivia.Categoria;

public sealed record CategoriaId(Guid Valor)
{
    public static CategoriaId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
