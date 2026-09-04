using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Respuesta confirmada de un participante a una pregunta trivia (HU-34).
/// </summary>
public sealed class RespuestaTrivia : Entity
{
    public RespuestaTriviaId RespuestaTriviaId { get; private set; } = default!;
    public SesionId SesionId { get; private set; } = default!;
    public ParticipanteId ParticipanteId { get; private set; } = default!;
    public PreguntaId PreguntaId { get; private set; } = default!;
    public int IndiceOpcion { get; private set; }
    public DateTime TimestampServidor { get; private set; }
    public bool FueraDeTiempo { get; private set; }
    public bool EsCorrecta { get; private set; }
    public int PuntosOtorgados { get; private set; }
    public long TiempoRespuestaMs { get; private set; }

    private RespuestaTrivia() { }

    internal static RespuestaTrivia Crear(
        SesionId sesionId,
        ParticipanteId participanteId,
        PreguntaId preguntaId,
        int indiceOpcion,
        DateTime timestampServidor,
        bool fueraDeTiempo,
        bool esCorrecta,
        int puntosOtorgados,
        long tiempoRespuestaMs)
    {
        ArgumentNullException.ThrowIfNull(sesionId);
        ArgumentNullException.ThrowIfNull(participanteId);
        ArgumentNullException.ThrowIfNull(preguntaId);

        if (indiceOpcion < 0)
            throw new DomainException("El índice de opción no puede ser negativo.");

        if (puntosOtorgados < 0)
            throw new DomainException("Los puntos otorgados no pueden ser negativos.");

        if (tiempoRespuestaMs < 0)
            throw new DomainException("El tiempo de respuesta no puede ser negativo.");

        return new RespuestaTrivia
        {
            RespuestaTriviaId  = RespuestaTriviaId.Nuevo(),
            SesionId           = sesionId,
            ParticipanteId     = participanteId,
            PreguntaId         = preguntaId,
            IndiceOpcion       = indiceOpcion,
            TimestampServidor  = timestampServidor,
            FueraDeTiempo      = fueraDeTiempo,
            EsCorrecta         = esCorrecta,
            PuntosOtorgados    = puntosOtorgados,
            TiempoRespuestaMs  = tiempoRespuestaMs
        };
    }

    protected override bool IdEquals(Entity other) =>
        other is RespuestaTrivia r && r.RespuestaTriviaId == RespuestaTriviaId;

    protected override int GetIdHashCode() => RespuestaTriviaId.GetHashCode();
}
