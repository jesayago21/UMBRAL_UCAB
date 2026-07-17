using FluentValidation;
using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Application.Misiones.Commands.AgregarEtapaMision;

public sealed class AgregarEtapaMisionValidator : AbstractValidator<AgregarEtapaMisionCommand>
{
    public AgregarEtapaMisionValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.TipoEtapa).NotEmpty();
        RuleFor(x => x.TipoEtapa)
            .Must(t => t is "BusquedaTesoro" or "Trivia")
            .WithMessage("TipoEtapa debe ser BusquedaTesoro o Trivia.");

        When(x => string.Equals(x.TipoEtapa, "BusquedaTesoro", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Descripcion).NotEmpty();
            RuleFor(x => x.CodigoQrSolucion).NotEmpty();

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
        });

        When(x => string.Equals(x.TipoEtapa, "Trivia", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.CategoriaIds).NotEmpty()
                .WithMessage("Etapa trivia requiere al menos una categoría (RB-33).");
        });
    }

    private static bool UbicacionCompletaOVacia(AgregarEtapaMisionCommand x)
    {
        var alguno = x.Latitud.HasValue || x.Longitud.HasValue || x.RadioMetros.HasValue;
        var todos = x.Latitud.HasValue && x.Longitud.HasValue && x.RadioMetros.HasValue;
        return !alguno || todos;
    }
}
