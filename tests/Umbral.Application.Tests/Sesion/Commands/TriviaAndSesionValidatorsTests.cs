using FluentValidation.TestHelper;
using Umbral.Application.Misiones.Commands.EliminarEtapaMision;
using Umbral.Application.Sesion.Commands.CerrarPreguntaTrivia;
using Umbral.Application.Sesion.Commands.ExpulsarParticipante;
using Umbral.Application.Sesion.Commands.LanzarPreguntaTrivia;
using Umbral.Application.Sesion.Commands.LiberarPistaManual;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class TriviaAndSesionValidatorsTests
{
    [Fact]
    public void CerrarPreguntaTriviaValidator_SesionVacia_TieneError()
    {
        new CerrarPreguntaTriviaValidator()
            .TestValidate(new CerrarPreguntaTriviaCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.SesionId);
    }

    [Fact]
    public void LanzarPreguntaTriviaValidator_DuracionInvalida_TieneError()
    {
        new LanzarPreguntaTriviaValidator()
            .TestValidate(new LanzarPreguntaTriviaCommand(Guid.NewGuid(), 0))
            .ShouldHaveValidationErrorFor(x => x.DuracionSegundos);
    }

    [Fact]
    public void ExpulsarParticipanteValidator_MotivoVacio_TieneError()
    {
        new ExpulsarParticipanteValidator()
            .TestValidate(new ExpulsarParticipanteCommand(Guid.NewGuid(), Guid.NewGuid(), "  "))
            .ShouldHaveValidationErrorFor(x => x.Motivo);
    }

    [Fact]
    public void LiberarPistaManualValidator_ContenidoVacio_TieneError()
    {
        new LiberarPistaManualValidator()
            .TestValidate(new LiberarPistaManualCommand(Guid.NewGuid(), "  ", Guid.NewGuid()))
            .ShouldHaveValidationErrorFor(x => x.Contenido);
    }

    [Fact]
    public void EliminarEtapaMisionValidator_IdsVacios_TieneErrores()
    {
        var result = new EliminarEtapaMisionValidator()
            .TestValidate(new EliminarEtapaMisionCommand(Guid.Empty, Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.MisionId);
        result.ShouldHaveValidationErrorFor(x => x.EtapaId);
    }
}
