using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

internal static class SesionTestHelpers
{
    public static ParticipanteSesion UnirParticipante(SesionAR sesion, string nombre, UsuarioId? jugador = null)
    {
        var j = jugador ?? UsuarioId.Nuevo();
        return sesion.UnirseParticipante(j, nombre, sesion.CodigoAcceso.Valor);
    }
}
