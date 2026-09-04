using FluentValidation;

namespace Umbral.Application.Misiones.Commands.EliminarEtapaMision;

public sealed class EliminarEtapaMisionValidator : AbstractValidator<EliminarEtapaMisionCommand>
{
    public EliminarEtapaMisionValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.EtapaId).NotEmpty();
    }
}
