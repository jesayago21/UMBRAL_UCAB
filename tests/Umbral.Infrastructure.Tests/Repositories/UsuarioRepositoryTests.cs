using FluentAssertions;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class UsuarioRepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task GuardarAsync_Y_ObtenerPorIdAsync_RetornaUsuario()
    {
        var usuario = UsuarioAdministrable.Crear(
            KeycloakUserId.From(Guid.NewGuid()),
            EmailAddress.Create($"repo_{Guid.NewGuid():N}@test.com"),
            $"user_{Guid.NewGuid():N}"[..20],
            "Repo",
            "Test",
            [Domain.IdentidadYAccesos.Enums.RolSistema.Operador]);
        usuario.ClearDomainEvents();

        var sut = CreateRepository();
        await sut.GuardarAsync(usuario);

        var loaded = await sut.ObtenerPorIdAsync(usuario.Id);

        loaded.Should().NotBeNull();
        loaded!.Email.Should().Be(usuario.Email);
        loaded.Roles.Should().ContainSingle();
    }

    [Fact]
    public async Task ExisteEmailAsync_Y_ExisteUsernameAsync()
    {
        var email = EmailAddress.Create($"exists_{Guid.NewGuid():N}@test.com");
        var username = $"exists_{Guid.NewGuid():N}"[..20];
        var usuario = UsuarioAdministrable.Crear(
            KeycloakUserId.From(Guid.NewGuid()),
            email,
            username,
            "Existe",
            "Test",
            [Domain.IdentidadYAccesos.Enums.RolSistema.Administrador]);
        usuario.ClearDomainEvents();

        var sut = CreateRepository();
        await sut.GuardarAsync(usuario);

        (await sut.ExisteEmailAsync(email)).Should().BeTrue();
        (await sut.ExisteUsernameAsync(username)).Should().BeTrue();
        (await sut.ExisteEmailAsync(EmailAddress.Create("otro@test.com"))).Should().BeFalse();
    }

    [Fact]
    public async Task ListarAsync_RespetaSkipTake()
    {
        var sut = CreateRepository();
        var emailA = EmailAddress.Create($"a_{Guid.NewGuid():N}@test.com");
        var emailB = EmailAddress.Create($"b_{Guid.NewGuid():N}@test.com");

        await sut.GuardarAsync(CrearUsuario(emailA, $"a_{Guid.NewGuid():N}"[..20]));
        await sut.GuardarAsync(CrearUsuario(emailB, $"b_{Guid.NewGuid():N}"[..20]));

        var page = await sut.ListarAsync(0, 1);

        page.Should().HaveCount(1);
    }

    private static UsuarioAdministrable CrearUsuario(EmailAddress email, string username) =>
        UsuarioAdministrable.Crear(
            KeycloakUserId.From(Guid.NewGuid()),
            email,
            username,
            "List",
            "Test",
            [Domain.IdentidadYAccesos.Enums.RolSistema.Operador]);

    private UsuarioRepository CreateRepository()
    {
        var db = fixture.CreateDbContext();
        return new UsuarioRepository(db);
    }
}
