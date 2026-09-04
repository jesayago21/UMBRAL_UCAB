using FluentValidation;

namespace Umbral.Application.Misiones.Commands.EliminarPistaEtapa;

public sealed class EliminarPistaEtapaValidator : AbstractValidator<EliminarPistaEtapaCommand>
{
    public EliminarPistaEtapaValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.EtapaId).NotEmpty();
        RuleFor(x => x.PistaId).NotEmpty();
    }
}
