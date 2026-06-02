using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Queries;

public sealed class ListUsuariosQueryHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly ListUsuariosQueryHandler _sut;

    public ListUsuariosQueryHandlerTests()
        => _sut = new ListUsuariosQueryHandler(_usuarios);

    [Fact]
    public async Task Handle_PaginaValida_CalculaSkipYRetornaDtos()
    {
        var usuario = UsuarioTestBuilder.Administrador();
        _usuarios
            .ListarAsync(10, 10, Arg.Any<CancellationToken>())
            .Returns([usuario]);

        var result = await _sut.Handle(new ListUsuariosQuery(2, 10), CancellationToken.None);

        result.Should().ContainSingle();
        await _usuarios.Received(1).ListarAsync(10, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PageCero_UsaPaginaUno()
    {
        _usuarios
            .ListarAsync(0, 50, Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Handle(new ListUsuariosQuery(0, 50), CancellationToken.None);

        await _usuarios.Received(1).ListarAsync(0, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PageSizeMayor100_LimitaA100()
    {
        _usuarios
            .ListarAsync(0, 100, Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Handle(new ListUsuariosQuery(1, 500), CancellationToken.None);

        await _usuarios.Received(1).ListarAsync(0, 100, Arg.Any<CancellationToken>());
    }
}
