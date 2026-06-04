using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

/// <summary>
/// Copia inmutable de la misión al crear la sesión (RB-11).
/// </summary>
public sealed class MisionSnapshot : ValueObject
{
    public MisionId MisionId { get; }
    public string Nombre { get; }
    public IReadOnlyList<EtapaSnapshotBase> Etapas { get; }

    private MisionSnapshot(
        MisionId misionId,
        string nombre,
        IReadOnlyList<EtapaSnapshotBase> etapas)
    {
        MisionId = misionId;
        Nombre   = nombre;
        Etapas   = etapas;
    }

    public static MisionSnapshot DesdeSoloBusquedaTesoro(Mision mision) =>
        Desde(mision, new Dictionary<EtapaId, EtapaTriviaSnapshot>());

    /// <summary>
    /// Snapshot de misión con una sola etapa trivia (sesiones trivia legacy / ad-hoc).
    /// </summary>
    public static MisionSnapshot SoloTrivia(
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        string categoriasTitulo)
    {
        ArgumentNullException.ThrowIfNull(preguntasOrdenadas);

        if (preguntasOrdenadas.Count == 0)
            throw new DomainException(
                "La sesión trivia requiere al menos una pregunta.");

        var etapaTrivia = EtapaTriviaSnapshot.Rehydrate(
            EtapaId.Nuevo(),
            1,
            [],
            preguntasOrdenadas,
            categoriasTitulo);

        return Rehydrate(
            MisionId.Nuevo(),
            categoriasTitulo,
            [etapaTrivia]);
    }

    public static MisionSnapshot Desde(Mision mision, IReadOnlyDictionary<EtapaId, EtapaTriviaSnapshot> triviaResueltas)
    {
        ArgumentNullException.ThrowIfNull(mision);
        ArgumentNullException.ThrowIfNull(triviaResueltas);

        var etapas = new List<EtapaSnapshotBase>();

        foreach (var etapa in mision.Etapas.OrderBy(e => e.Orden))
        {
            switch (etapa)
            {
                case EtapaBusquedaTesoro bt:
                    etapas.Add(EtapaBusquedaTesoroSnapshot.Desde(bt));
                    break;
                case EtapaTrivia trivia:
                    if (!triviaResueltas.TryGetValue(trivia.EtapaId, out var triviaSnap))
                        throw new DomainException(
                            $"Falta resolver preguntas para la etapa trivia '{trivia.EtapaId}'.");
                    etapas.Add(triviaSnap);
                    break;
                default:
                    throw new DomainException($"Tipo de etapa no soportado: {etapa.GetType().Name}");
            }
        }

        if (etapas.Count == 0)
            throw new DomainException("La misión debe tener al menos una etapa.");

        return new MisionSnapshot(mision.MisionId, mision.Nombre, etapas.AsReadOnly());
    }

    internal static MisionSnapshot Rehydrate(
        MisionId misionId,
        string nombre,
        IReadOnlyList<EtapaSnapshotBase> etapas) =>
        new(misionId, nombre, etapas);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MisionId;
        yield return Nombre;
    }
}
