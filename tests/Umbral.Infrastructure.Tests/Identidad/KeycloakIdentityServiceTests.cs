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
    public async Task SincronizarRolesAsync_UseDevStub_CompletaSinHttp()
    {
        var sut = CreateSut();
        var userId = KeycloakUserId.From(Guid.NewGuid());

        var act = () => sut.SincronizarRolesAsync(userId, [RolSistema.Administrador]);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CambiarEstadoAsync_UseDevStub_NoLanza()
    {
        var sut = CreateSut();
        var userId = KeycloakUserId.From(Guid.NewGuid());

        await sut.CambiarEstadoAsync(userId, habilitado: false);
        await sut.EliminarEnIdentityServerAsync(userId);
    }
}
