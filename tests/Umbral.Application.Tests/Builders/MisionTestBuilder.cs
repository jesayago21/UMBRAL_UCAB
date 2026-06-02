using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Application.Tests.Builders;

internal static class MisionTestBuilder
{
    public static Mision Activa()
    {
        var mision = Mision.Crear("Misión de prueba");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-TEST-001");
        mision.Activar();
        mision.ClearDomainEvents();
        return mision;
    }

    public static Mision Inactiva()
    {
        var mision = Mision.Crear("Misión borrador");
        mision.ClearDomainEvents();
        return mision;
    }

    public static Mision ActivaConDosEtapas()
    {
        var mision = Mision.Crear("Misión de dos etapas");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-ETAPA-001");
        mision.AgregarEtapaBusquedaTesoro("Etapa 2", "QR-ETAPA-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return mision;
    }
}
