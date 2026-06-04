using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Queries.GetPreguntaById;

internal sealed class GetPreguntaByIdQueryHandler : IRequestHandler<GetPreguntaByIdQuery, PreguntaDto>
{
    private readonly IPreguntaRepository _preguntaRepository;

    public GetPreguntaByIdQueryHandler(IPreguntaRepository preguntaRepository)
    {
        _preguntaRepository = preguntaRepository;
    }

    public async Task<PreguntaDto> Handle(GetPreguntaByIdQuery query, CancellationToken cancellationToken)
    {
        var pregunta = await _preguntaRepository.FindByIdAsync(
            new PreguntaId(query.PreguntaId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Pregunta), query.PreguntaId);

        if (pregunta.Eliminada)
            throw new NotFoundException(nameof(Pregunta), query.PreguntaId);

        return pregunta.ToDto();
    }
}
