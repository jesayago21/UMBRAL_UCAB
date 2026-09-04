using FluentValidation;
using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Application.Misiones.Commands.EditarEtapaMision;

public sealed class EditarEtapaMisionValidator : AbstractValidator<EditarEtapaMisionCommand>
{
    public EditarEtapaMisionValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.EtapaId).NotEmpty();
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Descripcion)
                || !string.IsNullOrWhiteSpace(x.CodigoQrSolucion)
                || (x.CategoriaIds is { Count: > 0 })
                || x.Latitud.HasValue
                || x.Longitud.HasValue
                || x.RadioMetros.HasValue)
            .WithMessage("Debe indicar datos de etapa BT (descripción/QR/mapa) o categorías de trivia.");

        RuleFor(x => x)
            .Must(UbicacionCompletaOVacia)
            .WithMessage("La ubicación del tesoro requiere latitud, longitud y radio juntos.");

        RuleFor(x => x.Latitud)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitud.HasValue);

        RuleFor(x => x.Longitud)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitud.HasValue);

        RuleFor(x => x.RadioMetros)
            .InclusiveBetween(EtapaBusquedaTesoro.RadioMetrosMinimo, EtapaBusquedaTesoro.RadioMetrosMaximo)
            .When(x => x.RadioMetros.HasValue);
    }

    private static bool UbicacionCompletaOVacia(EditarEtapaMisionCommand x)
    {
        var alguno = x.Latitud.HasValue || x.Longitud.HasValue || x.RadioMetros.HasValue;
        var todos = x.Latitud.HasValue && x.Longitud.HasValue && x.RadioMetros.HasValue;
        return !alguno || todos;
    }
}
