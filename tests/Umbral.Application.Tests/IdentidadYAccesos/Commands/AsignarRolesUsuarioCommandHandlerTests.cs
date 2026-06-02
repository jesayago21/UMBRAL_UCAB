using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class AsignarRolesUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly AsignarRolesUsuarioCommandHandler _sut;

    public AsignarRolesUsuarioCommandHandlerTests()
        => _sut = new AsignarRolesUsuarioCommandHandler(_usuarios, _identity);

    [Fact]
    public async Task Handle_UsuarioExiste_AsignaRolesSincronizaYGuarda()
    {
        var usuario = UsuarioTestBuilder.Operador();
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new AsignarRolesUsuarioCommand(usuario.Id.Valor, ["Administrador"]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        usuario.Roles.Should().ContainSingle().Which.ToString().Should().Be("Administrador");
        await _identity.Received(1).SincronizarRolesAsync(
            usuario.KeycloakUserId,
            Arg.Any<IReadOnlyList<Domain.IdentidadYAccesos.Enums.RolSistema>>(),
            Arg.Any<CancellationToken>());
        await _usuarios.Received(1).GuardarAsync(usuario, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaNotFoundException()
    {
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns((UsuarioAdministrable?)null);

        var act = () => _sut.Handle(
            new AsignarRolesUsuarioCommand(Guid.NewGuid(), ["Operador"]),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
