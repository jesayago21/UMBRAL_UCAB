using FluentValidation;

namespace Umbral.Application.Sesion.Commands.LiberarPistasPorTiempoVencidas;

public sealed class LiberarPistasPorTiempoVencidasValidator
    : AbstractValidator<LiberarPistasPorTiempoVencidasCommand>
{
    public LiberarPistasPorTiempoVencidasValidator()
    {
        // Sin reglas de entrada: el command es un barrido del sistema (background).
    }
}
