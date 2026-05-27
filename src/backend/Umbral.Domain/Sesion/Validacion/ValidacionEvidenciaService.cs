namespace Umbral.Domain.Sesion.Validacion;

/// <summary>
/// Domain Service — validación base de evidencias QR (HU-18).
/// RB-06, RB-19, RB-22. Cadena completa (ganador único) en iter-06.
/// </summary>
public static class ValidacionEvidenciaService
{
    public static ResultadoValidacion Validar(Sesion sesion, CodigoQR codigoEscaneado)
    {
        ArgumentNullException.ThrowIfNull(sesion);
        ArgumentNullException.ThrowIfNull(codigoEscaneado);

        if (sesion.Estado != EstadoSesion.Activa)
            return ResultadoValidacion.Rechazada;

        var etapaActual = sesion.ContextoBT!.ObtenerEtapaActual();

        return codigoEscaneado.CoincideCon(etapaActual.CodigoQRSolucion)
            ? ResultadoValidacion.Valida
            : ResultadoValidacion.Invalida;
    }
}
