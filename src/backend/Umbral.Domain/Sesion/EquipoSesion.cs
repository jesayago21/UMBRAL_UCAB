using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class EquipoSesion : Entity
{
    public EquipoId EquipoId { get; private set; } = default!;
    public SesionId SesionId { get; private set; } = default!;
    public NombreEquipo Nombre { get; private set; } = default!;
    public CodigoAcceso CodigoAcceso { get; private set; } = default!;
    public Puntaje PuntajeTotal { get; private set; } = default!;

    private EquipoSesion() { }

    internal static EquipoSesion Crear(SesionId sesionId, string nombre)
    {
        ArgumentNullException.ThrowIfNull(sesionId);

        return new EquipoSesion
        {
            EquipoId      = EquipoId.Nuevo(),
            SesionId      = sesionId,
            Nombre        = NombreEquipo.Crear(nombre),
            CodigoAcceso  = CodigoAcceso.Generar(),
            PuntajeTotal  = Puntaje.Zero()
        };
    }

    public void SumarPuntaje(int puntos) =>
        PuntajeTotal = PuntajeTotal.Sumar(puntos);

    public void AplicarPenalizacion(Penalizacion penalizacion) =>
        PuntajeTotal = PuntajeTotal.Restar(penalizacion.Puntos);

    protected override bool IdEquals(Entity other) =>
        other is EquipoSesion e && e.EquipoId == EquipoId;

    protected override int GetIdHashCode() => EquipoId.GetHashCode();
}
