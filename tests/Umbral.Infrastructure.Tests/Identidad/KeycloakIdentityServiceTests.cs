using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Infrastructure.Identidad;

namespace Umbral.Infrastructure.Tests.Identidad;

public sealed class KeycloakIdentityServiceTests
{
    private static KeycloakIdentityService CreateSut(bool useDevStub = true) =>
        new(
            new HttpClient(),
            Options.Create(new KeycloakAdminOptions { UseDevStub = useDevStub }),
            NullLogger<KeycloakIdentityService>.Instance);

    [Fact]
    public async Task RegistrarEnIdentityServerAsync_UseDevStub_RetornaKeycloakUserId()
    {
        var sut = CreateSut();
        var email = EmailAddress.Create("stub@test.com");

        var id = await sut.RegistrarEnIdentityServerAsync(
            email,
            "stub_user",
            "Stub",
            "User",
            "Password1",
            [RolSistema.Operador]);

        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ListarUsuariosAsync_UseDevStub_RetornaUsuariosRegistrados()
    {
        var sut = CreateSut();
        var email = EmailAddress.Create("list_stub@test.com");
        await sut.RegistrarEnIdentityServerAsync(
            email, "list_user", "List", "User", "Password1", [RolSistema.Operador]);

        var list = await sut.ListarUsuariosAsync(0, 50);

        list.Should().Contain(u => u.Email == email.Value);
    }

    [Fact]
    public async Task ExisteEmailAsync_UseDevStub_DetectaDuplicado()
    {
        var sut = CreateSut();
        var email = EmailAddress.Create("dup_stub@test.com");
        await sut.RegistrarEnIdentityServerAsync(
            email, "dup_user", "Dup", "User", "Password1", [RolSistema.Operador]);

        var existe = await sut.ExisteEmailAsync(email);

        existe.Should().BeTrue();
    }

    [Fact]
    public async Task SincronizarRolesAsync_UseDevStub_CompletaSinHttp()
    {
        var sut = CreateSut();
        var email = EmailAddress.Create("roles_stub@test.com");
        var id = await sut.RegistrarEnIdentityServerAsync(
            email, "roles_user", "Roles", "User", "Password1", [RolSistema.Operador]);

        await sut.SincronizarRolesAsync(id, [RolSistema.Administrador]);

        var roles = await sut.ObtenerRolesAsync(id);
        roles.Should().ContainSingle().Which.Should().Be(RolSistema.Administrador);
    }

    [Fact]
    public async Task CambiarEstadoAsync_UseDevStub_ActualizaEstado()
    {
        var sut = CreateSut();
        var email = EmailAddress.Create("estado_stub@test.com");
        var id = await sut.RegistrarEnIdentityServerAsync(
            email, "estado_user", "Estado", "User", "Password1", [RolSistema.Operador]);

        await sut.CambiarEstadoAsync(id, habilitado: false);

        var usuario = await sut.ObtenerUsuarioPorIdAsync(id);
        usuario!.Estado.Should().Be(EstadoUsuario.Bloqueado);
    }

    [Fact]
    public async Task EliminarEnIdentityServerAsync_UseDevStub_EliminaUsuario()
    {
        var sut = CreateSut();
        var email = EmailAddress.Create("del_stub@test.com");
        var id = await sut.RegistrarEnIdentityServerAsync(
            email, "del_user", "Del", "User", "Password1", [RolSistema.Operador]);

        await sut.EliminarEnIdentityServerAsync(id);

        var usuario = await sut.ObtenerUsuarioPorIdAsync(id);
        usuario.Should().BeNull();
    }
}
