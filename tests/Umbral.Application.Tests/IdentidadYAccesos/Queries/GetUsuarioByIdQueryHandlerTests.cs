using FluentAssertions;
using NSubstitute;
using Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Queries;

public sealed class GetUsuarioByIdQueryHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly GetUsuarioByIdQueryHandler _sut;

    public GetUsuarioByIdQueryHandlerTests()
        => _sut = new GetUsuarioByIdQueryHandler(_usuarios);

    [Fact]
    public async Task Handle_Existe_RetornaDto()
    {
        var usuario = UsuarioTestBuilder.Administrador();
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns(usuario);

        var dto = await _sut.Handle(new GetUsuarioByIdQuery(usuario.Id.Valor), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Email.Should().Be("admin@umbral.test");
        dto.Roles.Should().Contain("Administrador");
    }

    [Fact]
    public async Task Handle_NoExiste_RetornaNull()
    {
        _usuarios
            .ObtenerPorIdAsync(Arg.Any<UsuarioAdministrableId>(), Arg.Any<CancellationToken>())
            .Returns((UsuarioAdministrable?)null);

        var dto = await _sut.Handle(new GetUsuarioByIdQuery(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }
}
