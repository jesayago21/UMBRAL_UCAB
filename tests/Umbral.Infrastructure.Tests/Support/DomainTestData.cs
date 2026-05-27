using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;

namespace Umbral.Infrastructure.Tests.Support;

internal static class DomainTestData
{
    public static MisionSnapshot MisionSnapshotActiva(string nombre = "Misión integración")
    {
        var mision = Mision.Crear(nombre);
        mision.AgregarEtapa("Busca el árbol rojo", "QR-ARBOL-001");
        mision.AgregarEtapa("Encuentra la fuente", "QR-FUENTE-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return MisionSnapshot.Desde(mision);
    }

    public static Sesion SesionBusquedaTesoroActiva(string equipo = "Equipo Alpha")
    {
        var sesion = Sesion.CrearBusquedaTesoro(MisionSnapshotActiva(), UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        sesion.RegistrarEquipo(equipo);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static Mision MisionConEtapasYPistas(string nombre = "Misión persistencia")
    {
        var mision = Mision.Crear(nombre);
        mision.AgregarEtapa("Etapa 1", "QR-001");
        mision.Etapas[0].AgregarPista("Pista por tiempo", TipoLiberacion.PorTiempo, 30);
        mision.AgregarEtapa("Etapa 2", "QR-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return mision;
    }
}
