namespace Umbral.API.Contracts.Misiones;

public sealed record MisionResponse(
    Guid Id,
    string Nombre,
    string Descripcion,
    string NivelDificultad,
    int TiempoMaximoSeg,
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaMisionResponse> Etapas);

public sealed record EtapaMisionResponse(
    Guid EtapaId,
    int Orden,
    string Descripcion,
    string CodigoQrSolucion,
    IReadOnlyList<PistaMisionResponse> Pistas);

public sealed record PistaMisionResponse(
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
