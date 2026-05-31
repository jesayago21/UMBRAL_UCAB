using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Contexto de ejecución de una sesión de tipo Trivia.
/// Solo existe cuando <see cref="TipoSesion"/> == Trivia.
/// </summary>
public sealed class ContextoTrivia : Entity
{
    private readonly Guid _id = Guid.NewGuid();

    public int PreguntaActualIndex { get; private set; }
    public DateTime? TimerCerradoEn { get; private set; }
    public IReadOnlyList<PreguntaId> PreguntasOrdenadas { get; private set; } = [];

    private ContextoTrivia() { }

    internal static ContextoTrivia Crear(IReadOnlyList<PreguntaId> preguntasOrdenadas)
    {
        ArgumentNullException.ThrowIfNull(preguntasOrdenadas);

        if (preguntasOrdenadas.Count == 0)
            throw new DomainException(
                "Una sesión de trivia necesita al menos una pregunta.");

        return new ContextoTrivia
        {
            PreguntasOrdenadas = preguntasOrdenadas.ToList(),
            PreguntaActualIndex = 0
        };
    }

    /// <summary>Reconstitución desde persistencia (no usar en lógica de negocio).</summary>
    internal static ContextoTrivia Rehydrate(
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        int preguntaActualIndex,
        DateTime? timerCerradoEn) =>
        new()
        {
            PreguntasOrdenadas  = preguntasOrdenadas.ToList(),
            PreguntaActualIndex = preguntaActualIndex,
            TimerCerradoEn      = timerCerradoEn
        };

    public int TotalPreguntas => PreguntasOrdenadas.Count;

    protected override bool IdEquals(Entity other) =>
        other is ContextoTrivia c && c._id == _id;

    protected override int GetIdHashCode() => _id.GetHashCode();
}
