namespace Umbral.API.Contracts.Misiones;

public sealed record ActualizarMisionRequest(
    string Nombre,
    bool? Activar);
