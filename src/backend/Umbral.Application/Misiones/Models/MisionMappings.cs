using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Application.Misiones.Models;

internal static class MisionMappings
{
    public static MisionDto ToDto(this Mision mision)
    {
        var etapas = mision.Etapas
            .OrderBy(x => x.Orden)
            .Select(etapa => new EtapaMisionDto(
                etapa.EtapaId.Valor,
                etapa.Orden,
                etapa.Descripcion,
                etapa.CodigoQRSolucion,
                etapa.Pistas
                    .Select(pista => new PistaMisionDto(
                        pista.PistaId.Valor,
                        pista.Contenido,
                        pista.TipoLiberacion.ToString(),
                        pista.SegundosLiberacion))
                    .ToList()))
            .ToList();

        return new MisionDto(
            mision.MisionId.Valor,
            mision.Nombre,
            mision.Nombre,
            "NoDefinida",
            0,
            mision.Estado == EstadoMision.Activa ? "Activa" : "Inactiva",
            etapas.Count,
            etapas);
    }
}
