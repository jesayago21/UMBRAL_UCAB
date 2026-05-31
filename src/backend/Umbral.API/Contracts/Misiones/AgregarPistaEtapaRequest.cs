namespace Umbral.API.Contracts.Misiones;

public sealed record AgregarPistaEtapaRequest(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
