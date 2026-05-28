namespace Umbral.Infrastructure.Auth;

public interface IUsuarioAuthRepository
{
    Task<UsuarioAuthDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}
