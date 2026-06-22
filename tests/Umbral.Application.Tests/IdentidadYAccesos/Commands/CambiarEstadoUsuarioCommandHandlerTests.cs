using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class CambiarEstadoUsuarioCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly CambiarEstadoUsuarioCommandHandler _sut;

    public CambiarEstadoUsuarioCommandHandlerTests()
        => _sut = new CambiarEstadoUsuarioCommandHandler(_identity);

    [Fact]
    public async Task Handle_Bloquear_DeshabilitaEnKeycloak()
    {
        var usuario = UsuarioIdentidadTestBuilder.Administrador();
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new CambiarEstadoUsuarioCommand(usuario.KeycloakUserId.Value, "Bloquear"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identity.Received(1).CambiarEstadoAsync(
            usuario.KeycloakUserId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Activar_HabilitaEnKeycloak()
    {
        var usuario = UsuarioIdentidadTestBuilder.Administrador();
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new CambiarEstadoUsuarioCommand(usuario.KeycloakUserId.Value, "activar"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identity.Received(1).CambiarEstadoAsync(
            usuario.KeycloakUserId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AccionInvalida_RetornaFail()
    {
        var usuario = UsuarioIdentidadTestBuilder.Administrador();
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new CambiarEstadoUsuarioCommand(usuario.KeycloakUserId.Value, "Suspender"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.IdentidadYAccesos.UsuarioIdentidad?)null);

        var act = () => _sut.Handle(
            new CambiarEstadoUsuarioCommand(Guid.NewGuid(), "Bloquear"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
