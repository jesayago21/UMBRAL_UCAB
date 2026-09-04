using FluentValidation;

namespace Umbral.Application.Misiones.Commands.EditarPistaEtapa;

public sealed class EditarPistaEtapaValidator : AbstractValidator<EditarPistaEtapaCommand>
{
    public EditarPistaEtapaValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.EtapaId).NotEmpty();
        RuleFor(x => x.PistaId).NotEmpty();
        RuleFor(x => x.Contenido).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TipoLiberacion).NotEmpty();
    }
}
