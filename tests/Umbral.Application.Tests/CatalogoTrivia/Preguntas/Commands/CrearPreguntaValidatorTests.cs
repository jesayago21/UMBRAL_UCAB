using FluentAssertions;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Preguntas.Commands;

public sealed class CrearPreguntaValidatorTests
{
    private readonly CrearPreguntaValidator _validator = new();

    private static CrearPreguntaCommand ComandoValido() => new(
        "¿Pregunta de prueba?",
        "Facil",
        null,
        [
            new OpcionRespuestaInput("A", true),
            new OpcionRespuestaInput("B", false),
            new OpcionRespuestaInput("C", false)
        ]);

    [Fact]
    public void Validar_ComandoCompleto_NoTieneErrores()
    {
        var result = _validator.Validate(ComandoValido());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_EnunciadoVacio_TieneError()
    {
        var command = ComandoValido() with { Enunciado = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Enunciado));
    }

    [Fact]
    public void Validar_DificultadInvalida_TieneError()
    {
        var command = ComandoValido() with { Dificultad = "Extrema" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Dificultad));
    }

    [Fact]
    public void Validar_MenosDeTresOpciones_TieneError()
    {
        var command = ComandoValido() with
        {
            Opciones =
            [
                new OpcionRespuestaInput("A", true),
                new OpcionRespuestaInput("B", false)
            ]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Opciones));
    }

    [Fact]
    public void Validar_SinOpcionCorrecta_TieneError()
    {
        var command = ComandoValido() with
        {
            Opciones =
            [
                new OpcionRespuestaInput("A", false),
                new OpcionRespuestaInput("B", false),
                new OpcionRespuestaInput("C", false)
            ]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
