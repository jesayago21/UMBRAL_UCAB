using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones;

internal static class TipoLiberacionParser
{
    /// <summary>
    /// Acepta PorTiempo, PorGanador (histórico) y AlInicio (alias de producto).
    /// </summary>
    public static TipoLiberacion Parse(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El tipo de liberación es obligatorio.");

        var normalizado = valor.Trim();
        if (normalizado.Equals("AlInicio", StringComparison.OrdinalIgnoreCase)
            || normalizado.Equals("PorGanador", StringComparison.OrdinalIgnoreCase))
            return TipoLiberacion.PorGanador;

        if (normalizado.Equals("PorTiempo", StringComparison.OrdinalIgnoreCase))
            return TipoLiberacion.PorTiempo;

        throw new DomainException(
            $"Tipo de liberación desconocido: '{valor}'. Use PorTiempo o AlInicio.");
    }
}
