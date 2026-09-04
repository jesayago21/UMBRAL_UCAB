namespace Umbral.API.Contracts.Auth;

public sealed record RegistroParticipanteRequest(
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Password);

public sealed record RegistroParticipanteResponse(
    Guid KeycloakUserId,
    string Email,
    string Username);
