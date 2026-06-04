namespace Umbral.Application.Misiones.Models;

public sealed record MisionDto(
    Guid Id,
    string Nombre,
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaMisionDto> Etapas);

public sealed record EtapaMisionDto(
    Guid EtapaId,
    int Orden,
    string TipoEtapa,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<PistaMisionDto>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds);

public sealed record PistaMisionDto(
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
