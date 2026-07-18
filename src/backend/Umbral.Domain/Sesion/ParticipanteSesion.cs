using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class ParticipanteSesion : Entity
{
    public ParticipanteId ParticipanteId { get; private set; } = default!;
    public SesionId SesionId { get; private set; } = default!;
    public UsuarioId JugadorId { get; private set; } = default!;
    public NombreParticipante Nombre { get; private set; } = default!;
    public Puntaje PuntajeTotal { get; private set; } = default!;
    /// <summary>
    /// Penalización no cubierta por el puntaje actual. Se salda al ganar puntos
    /// antes de incrementar <see cref="PuntajeTotal"/> (ranking nunca negativo).
    /// </summary>
    public Puntaje DeudaPendiente { get; private set; } = default!;

    private ParticipanteSesion() { }

    internal static ParticipanteSesion Crear(SesionId sesionId, UsuarioId jugadorId, string nombre)
    {
        ArgumentNullException.ThrowIfNull(sesionId);
        ArgumentNullException.ThrowIfNull(jugadorId);

        return new ParticipanteSesion
        {
            ParticipanteId = ParticipanteId.Nuevo(),
            SesionId       = sesionId,
            JugadorId      = jugadorId,
            Nombre         = NombreParticipante.Crear(nombre),
            PuntajeTotal   = Puntaje.Zero(),
            DeudaPendiente = Puntaje.Zero(),
        };
    }

    public void SumarPuntaje(int puntos)
    {
        var deuda = DeudaPendiente.Valor;
        if (deuda == 0)
        {
            PuntajeTotal = PuntajeTotal.Sumar(puntos);
            return;
        }

        if (puntos <= deuda)
        {
            DeudaPendiente = DeudaPendiente.Restar(puntos);
            return;
        }

        DeudaPendiente = Puntaje.Zero();
        PuntajeTotal = PuntajeTotal.Sumar(puntos - deuda);
    }

    public void AplicarPenalizacion(Penalizacion penalizacion)
    {
        var puntos = penalizacion.Puntos;
        var disponible = PuntajeTotal.Valor;
        var restado = Math.Min(disponible, puntos);
        var faltante = puntos - restado;

        PuntajeTotal = PuntajeTotal.Restar(puntos);
        if (faltante > 0)
            DeudaPendiente = DeudaPendiente.Sumar(faltante);
    }

    protected override bool IdEquals(Entity other) =>
        other is ParticipanteSesion e && e.ParticipanteId == ParticipanteId;

    protected override int GetIdHashCode() => ParticipanteId.GetHashCode();
}
