using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Queries;

public sealed class GetUsuarioByIdQueryHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly GetUsuarioByIdQueryHandler _sut;

    public GetUsuarioByIdQueryHandlerTests()
        => _sut = new GetUsuarioByIdQueryHandler(_identity);

    [Fact]
    public async Task Handle_UsuarioExiste_RetornaDto()
    {
        var usuario = UsuarioIdentidadTestBuilder.Operador();
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var result = await _sut.Handle(
            new GetUsuarioByIdQuery(usuario.KeycloakUserId.Value),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be(usuario.Email);
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaNull()
    {
        _identity
            .ObtenerUsuarioPorIdAsync(Arg.Any<KeycloakUserId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.IdentidadYAccesos.UsuarioIdentidad?)null);

        var result = await _sut.Handle(
            new GetUsuarioByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeNull();
    }
}
