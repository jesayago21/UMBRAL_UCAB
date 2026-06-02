using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class ParticipanteSesion : Entity
{
    public ParticipanteId ParticipanteId { get; private set; } = default!;
    public SesionId SesionId { get; private set; } = default!;
    public UsuarioId JugadorId { get; private set; } = default!;
    public NombreParticipante Nombre { get; private set; } = default!;
    public Puntaje PuntajeTotal { get; private set; } = default!;

    private ParticipanteSesion() { }

    internal static ParticipanteSesion Crear(SesionId sesionId, UsuarioId jugadorId, string nombre)
    {
        ArgumentNullException.ThrowIfNull(sesionId);
        ArgumentNullException.ThrowIfNull(jugadorId);

        return new ParticipanteSesion
        {
            ParticipanteId     = ParticipanteId.Nuevo(),
            SesionId     = sesionId,
            JugadorId    = jugadorId,
            Nombre       = NombreParticipante.Crear(nombre),
            PuntajeTotal = Puntaje.Zero(),
        };
    }

    public void SumarPuntaje(int puntos) =>
        PuntajeTotal = PuntajeTotal.Sumar(puntos);

    public void AplicarPenalizacion(Penalizacion penalizacion) =>
        PuntajeTotal = PuntajeTotal.Restar(penalizacion.Puntos);

    protected override bool IdEquals(Entity other) =>
        other is ParticipanteSesion e && e.ParticipanteId == ParticipanteId;

    protected override int GetIdHashCode() => ParticipanteId.GetHashCode();
}
