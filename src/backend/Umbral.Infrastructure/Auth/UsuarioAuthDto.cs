namespace Umbral.Infrastructure.Auth;

public sealed record UsuarioAuthDto(
    Guid Id,
    string Email,
    string PasswordHash,
    string Rol,
    bool Activo);
