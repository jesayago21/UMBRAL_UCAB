using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Builders;

internal static class SesionTestBuilder
{
    public static SesionAR EnPreparacionSinParticipantes()
    {
        var sesion = SesionAR.CrearBusquedaTesoro(
            MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.Activa()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR Finalizada()
    {
        var sesion = EnPreparacionSinParticipantes();
        UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        sesion.Finalizar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR ConParticipante(string nombre)
    {
        var sesion = EnPreparacionSinParticipantes();
        UnirParticipante(sesion, nombre);
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR Activa(string nombreParticipante = "Alpha")
    {
        var sesion = ConParticipante(nombreParticipante);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR Pausada(string nombreParticipante = "Alpha")
    {
        var sesion = Activa(nombreParticipante);
        sesion.Pausar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR ActivaConParticipantes(params string[] nombresParticipantes)
    {
        var sesion = EnPreparacionSinParticipantes();
        foreach (var nombre in nombresParticipantes)
            UnirParticipante(sesion, nombre);

        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static string CodigoQrEtapaActual(SesionAR sesion)
        => sesion.ContextoMision!.ObtenerEtapaBusquedaTesoroActual().CodigoQRSolucion;

    public static SesionAR ActivaConParticipantesDosEtapas(params string[] nombresParticipantes)
    {
        var sesion = SesionAR.CrearBusquedaTesoro(
            MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.ActivaConDosEtapas()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        foreach (var nombre in nombresParticipantes)
            UnirParticipante(sesion, nombre);

        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    private static void UnirParticipante(SesionAR sesion, string nombre, UsuarioId? jugador = null)
    {
        var j = jugador ?? UsuarioId.Nuevo();
        sesion.UnirseParticipante(j, nombre, sesion.CodigoAcceso.Valor);
    }
}
