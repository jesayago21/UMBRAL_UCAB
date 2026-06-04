using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.EliminarPregunta;

internal sealed class EliminarPreguntaCommandHandler
    : IRequestHandler<EliminarPreguntaCommand, Result<Guid>>
{
    private readonly IPreguntaRepository _preguntaRepository;

    public EliminarPreguntaCommandHandler(IPreguntaRepository preguntaRepository)
    {
        _preguntaRepository = preguntaRepository;
    }

    public async Task<Result<Guid>> Handle(
        EliminarPreguntaCommand command,
        CancellationToken cancellationToken)
    {
        var pregunta = await _preguntaRepository.FindByIdAsync(
            new PreguntaId(command.PreguntaId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Pregunta), command.PreguntaId);

        if (pregunta.Eliminada)
            throw new NotFoundException(nameof(Pregunta), command.PreguntaId);

        pregunta.Eliminar();
        await _preguntaRepository.SaveAsync(pregunta, cancellationToken);

        return Result<Guid>.Ok(pregunta.PreguntaId.Valor);
    }
}
