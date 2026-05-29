using Umbral.Domain.CatalogoTrivia.Categoria.Events;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoTrivia.Categoria;

/// <summary>
/// Aggregate Root del BC CatalogoTrivia — categoría del banco de preguntas.
/// HU-28..31. Unicidad de nombre (RB-15) se valida en la capa de aplicación.
/// </summary>
public sealed class Categoria : AggregateRoot
{
    public CategoriaId CategoriaId { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public bool Eliminada { get; private set; }

    private Categoria() { }

    public static Categoria Crear(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la categoría no puede estar vacío.");

        var categoria = new Categoria
        {
            CategoriaId = CategoriaId.Nuevo(),
            Nombre      = nombre.Trim(),
            Eliminada   = false
        };
        categoria.RaiseDomainEvent(new CategoriaCreada(categoria.CategoriaId));
        return categoria;
    }

    public void Renombrar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la categoría no puede estar vacío.");

        Nombre = nombre.Trim();
    }

    public void Eliminar() => Eliminada = true;

    protected override bool IdEquals(Entity other) =>
        other is Categoria c && c.CategoriaId == CategoriaId;

    protected override int GetIdHashCode() => CategoriaId.GetHashCode();
}
