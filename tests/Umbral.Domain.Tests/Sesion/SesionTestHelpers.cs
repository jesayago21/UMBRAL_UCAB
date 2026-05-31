using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

internal static class SesionTestHelpers
{
    public static EquipoSesion UnirEquipo(SesionAR sesion, string nombre, UsuarioId? jugador = null)
    {
        var j = jugador ?? UsuarioId.Nuevo();
        return sesion.UnirseEquipo(j, nombre, sesion.CodigoAcceso.Valor);
    }
}
