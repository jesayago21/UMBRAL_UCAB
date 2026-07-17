using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;
using Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class ActualizarUsuarioCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly ActualizarUsuarioCommandHandler _sut;

    public ActualizarUsuarioCommandHandlerTests() =>
        _sut = new ActualizarUsuarioCommandHandler(_identity);

    [Fact]
    public async Task Handle_ConDatosValidos_ActualizaPerfilYRoles()
    {
        var usuario = UsuarioIdentidadTestBuilder.Operador();
        _identity.ObtenerUsuarioPorIdAsync(usuario.KeycloakUserId, Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new ActualizarUsuarioCommand(
                usuario.KeycloakUserId.Value,
                "Nuevo",
                "Apellido",
                "Administrador",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identity.Received(1).ActualizarPerfilAsync(
            usuario.KeycloakUserId, "Nuevo", "Apellido", Arg.Any<CancellationToken>());
        await _identity.Received(1).SincronizarRolesAsync(
            usuario.KeycloakUserId,
            Arg.Any<IReadOnlyList<RolSistema>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RolParticipante_RetornaFail()
    {
        var usuario = UsuarioIdentidadTestBuilder.Operador();
        _identity.ObtenerUsuarioPorIdAsync(usuario.KeycloakUserId, Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new ActualizarUsuarioCommand(
                usuario.KeycloakUserId.Value,
                "N",
                "A",
                "Participante",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-35"));
    }

    [Fact]
    public async Task Handle_UsuarioInexistente_LanzaNotFound()
    {
        _identity.ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns((UsuarioIdentidad?)null);

        var act = () => _sut.Handle(
            new ActualizarUsuarioCommand(Guid.NewGuid(), "N", "A", "Operador", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public sealed class EliminarUsuarioCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly EliminarUsuarioCommandHandler _sut;

    public EliminarUsuarioCommandHandlerTests() =>
        _sut = new EliminarUsuarioCommandHandler(_identity);

    [Fact]
    public async Task Handle_Operador_EliminaEnKeycloak()
    {
        var usuario = UsuarioIdentidadTestBuilder.Operador();
        _identity.ObtenerUsuarioPorIdAsync(usuario.KeycloakUserId, Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new EliminarUsuarioCommand(usuario.KeycloakUserId.Value),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identity.Received(1).EliminarEnIdentityServerAsync(
            usuario.KeycloakUserId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Administrador_RetornaFail()
    {
        var usuario = UsuarioIdentidadTestBuilder.Administrador();
        _identity.ObtenerUsuarioPorIdAsync(usuario.KeycloakUserId, Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new EliminarUsuarioCommand(usuario.KeycloakUserId.Value),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _identity.DidNotReceive().EliminarEnIdentityServerAsync(
            Arg.Any<KeycloakUserId>(),
            Arg.Any<CancellationToken>());
    }
}
