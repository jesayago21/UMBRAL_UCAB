namespace Umbral.Domain.Sesion;

/// <summary>
/// Domain Service — puntaje BT (umbral-backend-spec.md §6.1 BusquedaTesoroStrategy).
/// </summary>
public static class CalculoPuntajeBusquedaService
{
    private const int PuntajeGanadorEtapa = 100;

    public static Puntaje Calcular(bool esGanador) =>
        esGanador ? Puntaje.Crear(PuntajeGanadorEtapa) : Puntaje.Zero();
}
