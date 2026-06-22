using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Queries;

public sealed class ListUsuariosQueryHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly ListUsuariosQueryHandler _sut;

    public ListUsuariosQueryHandlerTests()
        => _sut = new ListUsuariosQueryHandler(_identity);

    [Fact]
    public async Task Handle_PaginaValida_CalculaFirstYRetornaDtos()
    {
        var usuario = UsuarioIdentidadTestBuilder.Administrador();
        _identity
            .ListarUsuariosAsync(10, 10, Arg.Any<CancellationToken>())
            .Returns([usuario]);

        var result = await _sut.Handle(new ListUsuariosQuery(2, 10), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].KeycloakUserId.Should().Be(usuario.KeycloakUserId.Value);
        await _identity.Received(1).ListarUsuariosAsync(10, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PageCero_UsaPaginaUno()
    {
        _identity
            .ListarUsuariosAsync(0, 50, Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Handle(new ListUsuariosQuery(0, 50), CancellationToken.None);

        await _identity.Received(1).ListarUsuariosAsync(0, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PageSizeMayor100_LimitaA100()
    {
        _identity
            .ListarUsuariosAsync(0, 100, Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Handle(new ListUsuariosQuery(1, 500), CancellationToken.None);

        await _identity.Received(1).ListarUsuariosAsync(0, 100, Arg.Any<CancellationToken>());
    }
}
