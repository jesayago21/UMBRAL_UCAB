namespace Umbral.API.Contracts.Misiones;

public sealed record CrearMisionRequest(
    string Nombre,
    IReadOnlyList<CrearEtapaRequest> Etapas,
    bool Activar = false);

public sealed record CrearEtapaRequest(
    string TipoEtapa,
    int Orden,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<CrearPistaRequest>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null);

public sealed record CrearPistaRequest(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
