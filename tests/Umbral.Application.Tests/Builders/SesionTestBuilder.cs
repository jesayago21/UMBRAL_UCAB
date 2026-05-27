using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Builders;

internal static class SesionTestBuilder
{
    public static SesionAR EnPreparacionSinEquipos()
    {
        var sesion = SesionAR.CrearBusquedaTesoro(
            MisionSnapshot.Desde(MisionTestBuilder.Activa()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR Finalizada()
    {
        var sesion = EnPreparacionSinEquipos();
        sesion.RegistrarEquipo("EquipoDefault");
        sesion.Iniciar();
        sesion.Finalizar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR ConEquipo(string nombre)
    {
        var sesion = EnPreparacionSinEquipos();
        sesion.RegistrarEquipo(nombre);
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR Activa(string nombreEquipo = "EquipoDefault")
    {
        var sesion = ConEquipo(nombreEquipo);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static SesionAR Pausada(string nombreEquipo = "EquipoDefault")
    {
        var sesion = Activa(nombreEquipo);
        sesion.Pausar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
