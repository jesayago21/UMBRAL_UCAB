using Microsoft.EntityFrameworkCore;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly UmbralDbContext _db;

    public UsuarioRepository(UmbralDbContext db) => _db = db;

    public async Task<UsuarioAdministrable?> ObtenerPorIdAsync(
        UsuarioAdministrableId id,
        CancellationToken ct = default) =>
        await _db.UsuariosAdministrables.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<UsuarioAdministrable?> ObtenerPorUsernameAsync(
        string username,
        CancellationToken ct = default) =>
        await _db.UsuariosAdministrables.FirstOrDefaultAsync(
            x => x.Username.ToLower() == username.Trim().ToLowerInvariant(),
            ct);

    public async Task<UsuarioAdministrable?> ObtenerPorEmailAsync(
        EmailAddress email,
        CancellationToken ct = default) =>
        await _db.UsuariosAdministrables.FirstOrDefaultAsync(x => x.Email == email, ct);

    public async Task<UsuarioAdministrable?> ObtenerPorKeycloakIdAsync(
        KeycloakUserId id,
        CancellationToken ct = default) =>
        await _db.UsuariosAdministrables.FirstOrDefaultAsync(x => x.KeycloakUserId == id, ct);

    public Task<bool> ExisteEmailAsync(EmailAddress email, CancellationToken ct = default) =>
        _db.UsuariosAdministrables.AnyAsync(x => x.Email == email, ct);

    public Task<bool> ExisteUsernameAsync(string username, CancellationToken ct = default) =>
        _db.UsuariosAdministrables.AnyAsync(
            x => x.Username.ToLower() == username.Trim().ToLowerInvariant(), ct);

    public async Task GuardarAsync(UsuarioAdministrable usuario, CancellationToken ct = default)
    {
        var entry = _db.Entry(usuario);
        if (entry.State == EntityState.Detached)
            await _db.UsuariosAdministrables.AddAsync(usuario, ct);
        else
            // EF no detecta cambios in-place en _roles (lista + conversión a string).
            entry.Property<List<RolSistema>>("_roles").IsModified = true;

        await _db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(UsuarioAdministrable usuario, CancellationToken ct = default)
    {
        _db.UsuariosAdministrables.Remove(usuario);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<UsuarioAdministrable>> ListarAsync(
        int skip,
        int take,
        CancellationToken ct = default) =>
        await _db.UsuariosAdministrables
            .AsNoTracking()
            .OrderBy(x => x.Username)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
}
