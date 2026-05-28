using Microsoft.EntityFrameworkCore;
using Umbral.Infrastructure.Persistence;

namespace Umbral.Infrastructure.Auth;

public sealed class UsuarioAuthRepository : IUsuarioAuthRepository
{
    private readonly UmbralDbContext _dbContext;

    public UsuarioAuthRepository(UmbralDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UsuarioAuthDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        return user is null
            ? null
            : new UsuarioAuthDto(user.Id, user.Email, user.PasswordHash, user.Rol, user.Activo);
    }
}
