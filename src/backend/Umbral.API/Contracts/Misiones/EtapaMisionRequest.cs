namespace Umbral.API.Contracts.Misiones;

public sealed record AgregarEtapaMisionRequest(
    string TipoEtapa,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<Guid>? CategoriaIds,
    IReadOnlyList<AgregarPistaEtapaRequest>? Pistas,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null);

public sealed record EditarEtapaMisionRequest(
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<Guid>? CategoriaIds,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null);
