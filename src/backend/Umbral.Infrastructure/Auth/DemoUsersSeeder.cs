using Microsoft.EntityFrameworkCore;
using Umbral.Infrastructure.Persistence;
using Umbral.Infrastructure.Persistence.Entities;

namespace Umbral.Infrastructure.Auth;

public sealed class DemoUsersSeeder
{
    public const string AdminEmail = "admin@umbral.local";
    public const string OperadorEmail = "operador@umbral.local";
    public const string EquipoEmail = "equipo@umbral.local";

    public const string DemoPassword = "Umbral123!";

    public async Task SeedAsync(UmbralDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Usuarios.AnyAsync(cancellationToken))
            return;

        var users = new[]
        {
            BuildUser(AdminEmail, "Administrador"),
            BuildUser(OperadorEmail, "Operador"),
            BuildUser(EquipoEmail, "EquipoParticipante")
        };

        await dbContext.Usuarios.AddRangeAsync(users, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Usuario BuildUser(string email, string rol) => new()
    {
        Id = Guid.NewGuid(),
        Email = email.ToLowerInvariant(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
        Rol = rol,
        Activo = true
    };
}
