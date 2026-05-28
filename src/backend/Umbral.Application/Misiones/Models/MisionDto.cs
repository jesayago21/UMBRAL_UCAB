namespace Umbral.Application.Misiones.Models;

public sealed record MisionDto(
    Guid Id,
    string Nombre,
    string Estado,
    IReadOnlyList<EtapaMisionDto> Etapas);

public sealed record EtapaMisionDto(
    Guid EtapaId,
    int Orden,
    string Descripcion,
    string CodigoQrSolucion,
    IReadOnlyList<PistaMisionDto> Pistas);

public sealed record PistaMisionDto(
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
