namespace Umbral.API.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    long ExpiresIn,
    string Role);
