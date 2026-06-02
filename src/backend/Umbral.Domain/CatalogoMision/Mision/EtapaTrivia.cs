using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class EtapaTrivia : Etapa
{
    internal List<Guid> CategoriaIdsStorage { get; private set; } = [];

    public IReadOnlyList<CategoriaId> CategoriaIds =>
        CategoriaIdsStorage.Select(g => new CategoriaId(g)).ToList();

    public override TipoEtapa Tipo => TipoEtapa.Trivia;

    private EtapaTrivia() { }

    internal static EtapaTrivia Crear(MisionId misionId, int orden, IReadOnlyList<CategoriaId> categoriaIds)
    {
        if (categoriaIds is null || categoriaIds.Count == 0)
            throw new DomainException(
                "Etapa trivia requiere al menos una categoría (RB-33).");

        var distinct = categoriaIds.Distinct().ToList();
        return new EtapaTrivia
        {
            EtapaId             = EtapaId.Nuevo(),
            MisionId            = misionId,
            Orden               = orden,
            CategoriaIdsStorage = distinct.Select(c => c.Valor).ToList()
        };
    }
}
