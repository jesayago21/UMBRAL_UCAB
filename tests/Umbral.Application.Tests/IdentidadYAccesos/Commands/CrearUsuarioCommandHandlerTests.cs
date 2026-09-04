using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class CrearUsuarioCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly CrearUsuarioCommandHandler _sut;

    public CrearUsuarioCommandHandlerTests()
        => _sut = new CrearUsuarioCommandHandler(_identity);

    private static CrearUsuarioCommand ComandoValido() =>
        new(
            "nuevo@umbral.test",
            "nuevo_user",
            "Nuevo",
            "Usuario",
            ["Operador"]);

    [Fact]
    public async Task Handle_ConDatosValidos_RegistraEnKeycloakYRetornaOk()
    {
        var kcId = KeycloakUserId.From(Guid.NewGuid());
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RolSistema>>(),
                Arg.Any<CancellationToken>())
            .Returns(kcId);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.KeycloakUserId.Should().Be(kcId.Value);
        result.Value.Email.Should().Be("nuevo@umbral.test");
    }

    [Fact]
    public async Task Handle_EmailDuplicado_RetornaFail()
    {
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-36"));
        await _identity.DidNotReceive().RegistrarEnIdentityServerAsync(
            Arg.Any<EmailAddress>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<RolSistema>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsernameDuplicado_RetornaFail()
    {
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-36"));
    }

    [Fact]
    public async Task Handle_RolParticipante_RetornaFail_RB35()
    {
        var cmd = ComandoValido() with { Roles = ["Participante"] };

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-35"));
    }

    [Fact]
    public async Task Handle_IdentityServerFalla_RetornaFail()
    {
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RolSistema>>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<KeycloakUserId>>(_ => throw new InvalidOperationException("Keycloak caído"));

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Identity server"));
    }
}
