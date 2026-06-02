using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class EtapaTriviaSnapshot : EtapaSnapshotBase
{
    public IReadOnlyList<CategoriaId> CategoriaIds { get; }
    public IReadOnlyList<PreguntaId> PreguntasOrdenadas { get; }
    public string CategoriasTitulo { get; }

    public override TipoEtapa Tipo => TipoEtapa.Trivia;

    private EtapaTriviaSnapshot(
        EtapaId id,
        int orden,
        IReadOnlyList<CategoriaId> categoriaIds,
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        string categoriasTitulo)
        : base(id, orden)
    {
        CategoriaIds         = categoriaIds;
        PreguntasOrdenadas   = preguntasOrdenadas;
        CategoriasTitulo     = categoriasTitulo;
    }

    public static EtapaTriviaSnapshot DesdeEtapa(
        EtapaTrivia etapa,
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        string categoriasTitulo)
    {
        if (preguntasOrdenadas.Count == 0)
            throw new DomainException(
                "La etapa trivia requiere al menos una pregunta resuelta desde el banco.");

        return new EtapaTriviaSnapshot(
            etapa.EtapaId,
            etapa.Orden,
            etapa.CategoriaIds.ToList().AsReadOnly(),
            preguntasOrdenadas,
            categoriasTitulo);
    }

    internal static EtapaTriviaSnapshot Rehydrate(
        EtapaId id,
        int orden,
        IReadOnlyList<CategoriaId> categoriaIds,
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        string categoriasTitulo) =>
        new(id, orden, categoriaIds, preguntasOrdenadas, categoriasTitulo);
}
