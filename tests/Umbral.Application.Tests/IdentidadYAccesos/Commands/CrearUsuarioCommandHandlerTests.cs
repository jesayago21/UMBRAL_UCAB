using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class CrearUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly CrearUsuarioCommandHandler _sut;

    public CrearUsuarioCommandHandlerTests()
        => _sut = new CrearUsuarioCommandHandler(_usuarios, _identity);

    private static CrearUsuarioCommand ComandoValido() =>
        new(
            "nuevo@umbral.test",
            "nuevo_user",
            "Nuevo",
            "Usuario",
            "Temporal123",
            ["Operador"]);

    [Fact]
    public async Task Handle_ConDatosValidos_RegistraEnKeycloakGuardaYRetornaOk()
    {
        var kcId = KeycloakUserId.From(Guid.NewGuid());
        _usuarios.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _usuarios.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RolSistema>>(),
                Arg.Any<CancellationToken>())
            .Returns(kcId);

        UsuarioAdministrable? guardado = null;
        _usuarios
            .GuardarAsync(Arg.Do<UsuarioAdministrable>(u => guardado = u), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.KeycloakUserId.Should().Be(kcId.Value);
        guardado.Should().NotBeNull();
        guardado!.Email.Value.Should().Be("nuevo@umbral.test");
    }

    [Fact]
    public async Task Handle_EmailDuplicado_RetornaFail()
    {
        _usuarios.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-36"));
        await _identity.DidNotReceive().RegistrarEnIdentityServerAsync(
            Arg.Any<EmailAddress>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<RolSistema>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsernameDuplicado_RetornaFail()
    {
        _usuarios.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _usuarios.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-36"));
    }

    [Fact]
    public async Task Handle_IdentityServerFalla_RetornaFail()
    {
        _usuarios.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _usuarios.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RolSistema>>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<KeycloakUserId>>(_ => throw new InvalidOperationException("Keycloak caído"));

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Identity server"));
        await _usuarios.DidNotReceive().GuardarAsync(
            Arg.Any<UsuarioAdministrable>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GuardarFalla_CompensaEliminandoEnIdentity()
    {
        var kcId = KeycloakUserId.From(Guid.NewGuid());
        _usuarios.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _usuarios.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RolSistema>>(),
                Arg.Any<CancellationToken>())
            .Returns(kcId);
        _usuarios
            .GuardarAsync(Arg.Any<UsuarioAdministrable>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new Exception("BD caída"));

        var act = () => _sut.Handle(ComandoValido(), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        await _identity.Received(1).EliminarEnIdentityServerAsync(kcId, Arg.Any<CancellationToken>());
    }
}
