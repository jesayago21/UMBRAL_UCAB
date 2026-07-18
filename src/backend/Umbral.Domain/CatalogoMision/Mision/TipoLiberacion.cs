namespace Umbral.Domain.CatalogoMision.Mision;

public enum TipoLiberacion
{
    PorTiempo = 1,
    /// <summary>
    /// Se entrega automáticamente al iniciar la etapa (junto con el mapa).
    /// Nombre histórico en BD/API: PorGanador.
    /// </summary>
    PorGanador = 2,
}
