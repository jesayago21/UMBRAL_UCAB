using FluentAssertions;
using FluentValidation.TestHelper;
using Umbral.Application.IdentidadYAccesos.Commands.RegistroParticipante;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class RegistroParticipanteValidatorTests
{
    private readonly RegistroParticipanteValidator _sut = new();

    private static RegistroParticipanteCommand Valido() =>
        new("jugador@umbral.test", "jugador1", "Ana", "Pérez", "Umbral123!");

    [Fact]
    public void Validate_DatosValidos_SinErrores()
    {
        var result = _sut.TestValidate(Valido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmailInvalido_Error()
    {
        var result = _sut.TestValidate(Valido() with { Email = "no-es-email" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_PasswordCorta_Error()
    {
        var result = _sut.TestValidate(Valido() with { Password = "corta" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_UsernameVacio_Error()
    {
        var result = _sut.TestValidate(Valido() with { Username = "" });
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
