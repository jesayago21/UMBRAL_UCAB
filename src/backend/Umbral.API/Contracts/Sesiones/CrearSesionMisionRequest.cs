namespace Umbral.API.Contracts.Sesiones;

public sealed record CrearSesionMisionRequest(Guid MisionId);

public sealed record CrearSesionMisionResponse(
    Guid Id,
    string CodigoAcceso,
    string MisionNombre);
