using FluentAssertions;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.CatalogoTrivia.Pregunta.Events;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.CatalogoTrivia;

/// <summary>
/// Tests del agregado Pregunta de Trivia (HU-24..27, RB-28).
/// Reglas: enunciado no vacío, ≥3 opciones, exactamente una correcta.
/// </summary>
public sealed class PreguntaTests
{
    private static List<OpcionRespuesta> OpcionesValidas() =>
    [
        OpcionRespuesta.Crear("París", esCorrecta: true),
        OpcionRespuesta.Crear("Madrid", esCorrecta: false),
        OpcionRespuesta.Crear("Roma", esCorrecta: false)
    ];

    // ── Crear ─────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_RetornaPreguntaNoEliminada()
    {
        // Act
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());

        // Assert
        pregunta.Enunciado.Should().Be("¿Capital de Francia?");
        pregunta.Dificultad.Should().Be(Dificultad.Facil);
        pregunta.Opciones.Should().HaveCount(3);
        pregunta.Eliminada.Should().BeFalse();
        pregunta.PreguntaId.Valor.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Crear_SinCategoria_QuedaSinCategoria()
    {
        // Act
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());

        // Assert
        pregunta.CategoriaId.Should().BeNull();
    }

    [Fact]
    public void Crear_ConCategoria_AsignaCategoria()
    {
        // Arrange
        var categoriaId = CategoriaId.Nuevo();

        // Act
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Media, OpcionesValidas(), categoriaId);

        // Assert
        pregunta.CategoriaId.Should().Be(categoriaId);
    }

    [Fact]
    public void Crear_ConDatosValidos_EmitePreguntaCreadaEvent()
    {
        // Act
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());

        // Assert
        pregunta.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PreguntaCreada>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConEnunciadoVacio_LanzaDomainException(string enunciado)
    {
        // Act
        var act = () => Pregunta.Crear(enunciado, Dificultad.Facil, OpcionesValidas());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Crear_ConMenosDeTresOpciones_LanzaDomainException()
    {
        // Arrange
        var dos = new List<OpcionRespuesta>
        {
            OpcionRespuesta.Crear("Sí", esCorrecta: true),
            OpcionRespuesta.Crear("No", esCorrecta: false)
        };

        // Act
        var act = () => Pregunta.Crear("¿Verdadero?", Dificultad.Facil, dos);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos 3 opciones*");
    }

    [Fact]
    public void Crear_SinOpcionCorrecta_LanzaDomainException()
    {
        // Arrange
        var sinCorrecta = new List<OpcionRespuesta>
        {
            OpcionRespuesta.Crear("A", esCorrecta: false),
            OpcionRespuesta.Crear("B", esCorrecta: false),
            OpcionRespuesta.Crear("C", esCorrecta: false)
        };

        // Act
        var act = () => Pregunta.Crear("¿Cuál?", Dificultad.Facil, sinCorrecta);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*una* correcta*");
    }

    [Fact]
    public void Crear_ConMasDeUnaOpcionCorrecta_LanzaDomainException()
    {
        // Arrange
        var dosCorrectas = new List<OpcionRespuesta>
        {
            OpcionRespuesta.Crear("A", esCorrecta: true),
            OpcionRespuesta.Crear("B", esCorrecta: true),
            OpcionRespuesta.Crear("C", esCorrecta: false)
        };

        // Act
        var act = () => Pregunta.Crear("¿Cuál?", Dificultad.Facil, dosCorrectas);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*una* correcta*");
    }

    // ── ModificarContenido ────────────────────────────────────────────────

    [Fact]
    public void ModificarContenido_ConDatosValidos_ActualizaCampos()
    {
        // Arrange
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());
        var nuevasOpciones = new List<OpcionRespuesta>
        {
            OpcionRespuesta.Crear("Berlín", esCorrecta: true),
            OpcionRespuesta.Crear("Múnich", esCorrecta: false),
            OpcionRespuesta.Crear("Hamburgo", esCorrecta: false)
        };

        // Act
        pregunta.ModificarContenido(
            "¿Capital de Alemania?", Dificultad.Dificil, nuevasOpciones);

        // Assert
        pregunta.Enunciado.Should().Be("¿Capital de Alemania?");
        pregunta.Dificultad.Should().Be(Dificultad.Dificil);
        pregunta.Opciones.Should().Contain(o => o.Texto == "Berlín" && o.EsCorrecta);
    }

    [Fact]
    public void ModificarContenido_SinOpcionCorrecta_LanzaDomainException()
    {
        // Arrange
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());
        var sinCorrecta = new List<OpcionRespuesta>
        {
            OpcionRespuesta.Crear("A", esCorrecta: false),
            OpcionRespuesta.Crear("B", esCorrecta: false),
            OpcionRespuesta.Crear("C", esCorrecta: false)
        };

        // Act
        var act = () => pregunta.ModificarContenido("¿X?", Dificultad.Facil, sinCorrecta);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Categoría (RB-14: queda "Sin Categoría" al quitarla) ──────────────

    [Fact]
    public void AsignarCategoria_EstableceLaCategoria()
    {
        // Arrange
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());
        var categoriaId = CategoriaId.Nuevo();

        // Act
        pregunta.AsignarCategoria(categoriaId);

        // Assert
        pregunta.CategoriaId.Should().Be(categoriaId);
    }

    [Fact]
    public void QuitarCategoria_DejaPreguntaSinCategoria()
    {
        // Arrange
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas(), CategoriaId.Nuevo());

        // Act
        pregunta.QuitarCategoria();

        // Assert
        pregunta.CategoriaId.Should().BeNull();
    }

    // ── Eliminar (soft delete) ────────────────────────────────────────────

    [Fact]
    public void Eliminar_MarcaPreguntaComoEliminada()
    {
        // Arrange
        var pregunta = Pregunta.Crear(
            "¿Capital de Francia?", Dificultad.Facil, OpcionesValidas());

        // Act
        pregunta.Eliminar();

        // Assert
        pregunta.Eliminada.Should().BeTrue();
    }
}
