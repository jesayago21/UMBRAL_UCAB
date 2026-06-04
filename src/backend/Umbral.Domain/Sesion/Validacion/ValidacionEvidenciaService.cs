using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Domain.Sesion.Validacion;

/// <summary>
/// Domain Service — validación base de evidencias QR (HU-18).
/// RB-06, RB-19, RB-22.
/// </summary>
public static class ValidacionEvidenciaService
{
    public static ResultadoValidacion Validar(Sesion sesion, CodigoQR codigoEscaneado)
    {
        ArgumentNullException.ThrowIfNull(sesion);
        ArgumentNullException.ThrowIfNull(codigoEscaneado);

        if (sesion.Estado != EstadoSesion.Activa)
            return ResultadoValidacion.Rechazada;

        if (sesion.ContextoMision is null)
            return ResultadoValidacion.Rechazada;

        var etapaActual = sesion.ContextoMision.ObtenerEtapaActual();
        if (etapaActual is not EtapaBusquedaTesoroSnapshot bt)
            return ResultadoValidacion.Rechazada;

        return codigoEscaneado.CoincideCon(bt.CodigoQRSolucion)
            ? ResultadoValidacion.Valida
            : ResultadoValidacion.Invalida;
    }
}
