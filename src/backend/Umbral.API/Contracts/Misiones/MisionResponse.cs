namespace Umbral.API.Contracts.Misiones;

public sealed record MisionResponse(
    Guid Id,
    string Nombre,
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaMisionResponse> Etapas);

public sealed record EtapaMisionResponse(
    Guid EtapaId,
    int Orden,
    string TipoEtapa,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<PistaMisionResponse>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds);

public sealed record PistaMisionResponse(
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
