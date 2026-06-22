using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class AsignarRolesUsuarioCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly AsignarRolesUsuarioCommandHandler _sut;

    public AsignarRolesUsuarioCommandHandlerTests()
        => _sut = new AsignarRolesUsuarioCommandHandler(_identity);

    [Fact]
    public async Task Handle_UsuarioExiste_SincronizaRolesEnKeycloak()
    {
        var usuario = UsuarioIdentidadTestBuilder.Operador();
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new AsignarRolesUsuarioCommand(usuario.KeycloakUserId.Value, ["Administrador"]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identity.Received(1).SincronizarRolesAsync(
            usuario.KeycloakUserId,
            Arg.Any<IReadOnlyList<Domain.IdentidadYAccesos.Enums.RolSistema>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.IdentidadYAccesos.UsuarioIdentidad?)null);

        var act = () => _sut.Handle(
            new AsignarRolesUsuarioCommand(Guid.NewGuid(), ["Operador"]),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
