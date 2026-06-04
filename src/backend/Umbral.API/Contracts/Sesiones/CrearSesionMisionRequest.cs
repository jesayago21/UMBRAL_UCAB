namespace Umbral.API.Contracts.Sesiones;

public sealed record CrearSesionMisionRequest(Guid MisionId, string NombreSesion);

public sealed record CrearSesionMisionResponse(
    Guid Id,
    string CodigoAcceso,
    string NombreSesion,
    string MisionNombre);
