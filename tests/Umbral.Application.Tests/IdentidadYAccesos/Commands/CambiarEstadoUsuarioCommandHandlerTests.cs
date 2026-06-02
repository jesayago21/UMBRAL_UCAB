using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class CambiarEstadoUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly CambiarEstadoUsuarioCommandHandler _sut;

    public CambiarEstadoUsuarioCommandHandlerTests()
        => _sut = new CambiarEstadoUsuarioCommandHandler(_usuarios, _identity);

    [Fact]
    public async Task Handle_Bloquear_DeshabilitaEnKeycloakYGuarda()
    {
        var usuario = UsuarioTestBuilder.Administrador();
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new CambiarEstadoUsuarioCommand(usuario.Id.Valor, "Bloquear"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        usuario.Estado.Should().Be(EstadoUsuario.Bloqueado);
        await _identity.Received(1).CambiarEstadoAsync(
            usuario.KeycloakUserId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Activar_HabilitaEnKeycloakYGuarda()
    {
        var usuario = UsuarioTestBuilder.Administrador();
        usuario.Bloquear();
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new CambiarEstadoUsuarioCommand(usuario.Id.Valor, "activar"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        await _identity.Received(1).CambiarEstadoAsync(
            usuario.KeycloakUserId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AccionInvalida_RetornaFail()
    {
        var usuario = UsuarioTestBuilder.Administrador();
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new CambiarEstadoUsuarioCommand(usuario.Id.Valor, "Suspender"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns((UsuarioAdministrable?)null);

        var act = () => _sut.Handle(
            new CambiarEstadoUsuarioCommand(Guid.NewGuid(), "Bloquear"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
