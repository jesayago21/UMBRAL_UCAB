using FluentAssertions;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.IdentidadYAccesos;

public sealed class IdentidadValueObjectsTests
{
    [Fact]
    public void EmailAddress_Create_ConFormatoValido_NormalizaMinusculas()
    {
        var email = EmailAddress.Create("  Admin@UMBRAL.Test  ");

        email.Value.Should().Be("admin@umbral.test");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalido")]
    public void EmailAddress_Create_ConFormatoInvalido_LanzaDomainException(string raw)
    {
        var act = () => EmailAddress.Create(raw);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void KeycloakUserId_From_ConGuidValido_CreaInstancia()
    {
        var guid = Guid.NewGuid();
        var id = KeycloakUserId.From(guid);

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void KeycloakUserId_From_ConGuidVacio_LanzaDomainException()
    {
        var act = () => KeycloakUserId.From(Guid.Empty);

        act.Should().Throw<DomainException>();
    }
}
