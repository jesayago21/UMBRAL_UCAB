using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Commands.RegistroParticipante;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class RegistroParticipanteCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly RegistroParticipanteCommandHandler _sut;

    public RegistroParticipanteCommandHandlerTests()
        => _sut = new RegistroParticipanteCommandHandler(_identity);

    private static RegistroParticipanteCommand ComandoValido() =>
        new(
            "jugador@umbral.test",
            "jugador1",
            "Ana",
            "Pérez",
            "Umbral123!");

    [Fact]
    public async Task Handle_ConDatosValidos_RegistraParticipanteYRetornaOk()
    {
        var kcId = KeycloakUserId.From(Guid.NewGuid());
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarParticipanteEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(kcId);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.KeycloakUserId.Should().Be(kcId.Value);
        result.Value.Username.Should().Be("jugador1");
        await _identity.Received(1).RegistrarParticipanteEnIdentityServerAsync(
            Arg.Any<EmailAddress>(),
            "jugador1",
            "Ana",
            "Pérez",
            "Umbral123!",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailDuplicado_RetornaFail()
    {
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RB-36"));
        await _identity.DidNotReceive().RegistrarParticipanteEnIdentityServerAsync(
            Arg.Any<EmailAddress>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
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
    public async Task Handle_IdentityServerFalla_RetornaFail()
    {
        _identity.ExisteEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity.ExisteUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _identity
            .RegistrarParticipanteEnIdentityServerAsync(
                Arg.Any<EmailAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<KeycloakUserId>>(_ => throw new InvalidOperationException("Keycloak caído"));

        var result = await _sut.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Identity server"));
    }
}
