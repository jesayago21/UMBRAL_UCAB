using FluentAssertions;
using NSubstitute;
using Umbral.Application.Misiones.Queries.ListMisiones;
using Umbral.Domain.CatalogoMision.Mision;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Queries;

/// <summary>HU-02 — ListMisiones (Application): filtros por nombre y estado.</summary>
public sealed class ListMisionesQueryHandlerTests
{
    private readonly IMisionRepository _repo = Substitute.For<IMisionRepository>();
    private readonly ListMisionesQueryHandler _sut;

    public ListMisionesQueryHandlerTests()
        => _sut = new ListMisionesQueryHandler(_repo);

    private static Mision MisionActiva(string nombre)
    {
        var mision = Mision.Crear(nombre);
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-LM-001");
        mision.Activar();
        return mision;
    }

    private static Mision MisionInactiva(string nombre) => Mision.Crear(nombre);

    private void RepositorioDevuelve(params Mision[] misiones)
        => _repo.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(misiones.ToList());

    [Fact]
    public async Task Handle_SinFiltros_DevuelveTodasOrdenadasPorNombre()
    {
        // Arrange
        RepositorioDevuelve(MisionActiva("Zeta"), MisionInactiva("Alfa"));
        var query = new ListMisionesQuery(null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Nombre).Should().ContainInOrder("Alfa", "Zeta");
    }

    [Fact]
    public async Task Handle_ConFiltroNombre_DevuelveSoloCoincidencias()
    {
        // Arrange
        RepositorioDevuelve(MisionActiva("Búsqueda del tesoro"), MisionActiva("Trivia campus"));
        var query = new ListMisionesQuery("tesoro", null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle()
            .Which.Nombre.Should().Be("Búsqueda del tesoro");
    }

    [Fact]
    public async Task Handle_ConFiltroEstadoActiva_DevuelveSoloActivas()
    {
        // Arrange
        RepositorioDevuelve(MisionActiva("Activa uno"), MisionInactiva("Borrador uno"));
        var query = new ListMisionesQuery(null, "activa");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle()
            .Which.Nombre.Should().Be("Activa uno");
    }

    [Fact]
    public async Task Handle_ConFiltroEstadoInactiva_DevuelveSoloNoActivas()
    {
        // Arrange
        RepositorioDevuelve(MisionActiva("Activa dos"), MisionInactiva("Borrador dos"));
        var query = new ListMisionesQuery(null, "inactiva");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle()
            .Which.Nombre.Should().Be("Borrador dos");
    }

    [Fact]
    public async Task Handle_ConFiltroEstadoDesconocido_NoFiltraPorEstado()
    {
        // Arrange
        RepositorioDevuelve(MisionActiva("Activa tres"), MisionInactiva("Borrador tres"));
        var query = new ListMisionesQuery(null, "cualquiera");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}
