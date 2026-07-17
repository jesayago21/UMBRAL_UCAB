namespace Umbral.API.Contracts.Misiones;

public sealed record EditarPistaEtapaRequest(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
