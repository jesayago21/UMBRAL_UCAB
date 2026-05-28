namespace Umbral.API.Contracts.Misiones;

public sealed record CrearMisionRequest(
    string Nombre,
    IReadOnlyList<CrearEtapaRequest> Etapas,
    bool Activar = false);

public sealed record CrearEtapaRequest(
    string Descripcion,
    string CodigoQrSolucion,
    IReadOnlyList<CrearPistaRequest> Pistas);

public sealed record CrearPistaRequest(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
