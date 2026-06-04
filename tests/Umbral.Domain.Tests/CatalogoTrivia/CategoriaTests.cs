using FluentAssertions;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Categoria.Events;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.CatalogoTrivia;

/// <summary>
/// Tests del agregado Categoria de Trivia (HU-28..31, RB-15).
/// </summary>
public sealed class CategoriaTests
{
    // ── Crear ─────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConNombreValido_RetornaCategoriaNoEliminada()
    {
        // Act
        var categoria = Categoria.Crear("Geografía");

        // Assert
        categoria.Nombre.Should().Be("Geografía");
        categoria.Eliminada.Should().BeFalse();
        categoria.CategoriaId.Valor.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Crear_ConNombreValido_EmiteCategoriaCreadaEvent()
    {
        // Act
        var categoria = Categoria.Crear("Historia");

        // Assert
        categoria.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CategoriaCreada>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConNombreVacio_LanzaDomainException(string nombre)
    {
        // Act
        var act = () => Categoria.Crear(nombre);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Crear_RecortaEspaciosDelNombre()
    {
        // Act
        var categoria = Categoria.Crear("  Ciencia  ");

        // Assert
        categoria.Nombre.Should().Be("Ciencia");
    }

    // ── Renombrar ─────────────────────────────────────────────────────────

    [Fact]
    public void Renombrar_ConNombreValido_ActualizaNombre()
    {
        // Arrange
        var categoria = Categoria.Crear("Geografia");

        // Act
        categoria.Renombrar("Geografía Mundial");

        // Assert
        categoria.Nombre.Should().Be("Geografía Mundial");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Renombrar_ConNombreVacio_LanzaDomainException(string nombre)
    {
        // Arrange
        var categoria = Categoria.Crear("Geografía");

        // Act
        var act = () => categoria.Renombrar(nombre);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Eliminar (soft delete) ────────────────────────────────────────────

    [Fact]
    public void Eliminar_MarcaCategoriaComoEliminada()
    {
        // Arrange
        var categoria = Categoria.Crear("Deportes");

        // Act
        categoria.Eliminar();

        // Assert
        categoria.Eliminada.Should().BeTrue();
    }
}
