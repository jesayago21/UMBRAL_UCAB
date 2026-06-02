using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionParticipante;

internal sealed class GetPreguntasTriviaSesionParticipanteQueryHandler
    : IRequestHandler<GetPreguntasTriviaSesionParticipanteQuery, IReadOnlyList<PreguntaTriviaParticipanteDto>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IPreguntaRepository _preguntaRepository;

    public GetPreguntasTriviaSesionParticipanteQueryHandler(
        ISesionRepository sesionRepository,
        IPreguntaRepository preguntaRepository)
    {
        _sesionRepository    = sesionRepository;
        _preguntaRepository  = preguntaRepository;
    }

    public async Task<IReadOnlyList<PreguntaTriviaParticipanteDto>> Handle(
        GetPreguntasTriviaSesionParticipanteQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        var jugadorId = new UsuarioId(query.JugadorId);
        if (!sesion.Participantes.Any(e => e.JugadorId == jugadorId))
            throw new UnauthorizedAccessException(
                "Debes unirte a la sesión antes de ver las preguntas.");

        IReadOnlyList<PreguntaId> idsOrdenados = sesion.ContextoMision?.ObtenerEtapaActual() switch
        {
            EtapaTriviaSnapshot trivia => trivia.PreguntasOrdenadas,
            _ when sesion.ContextoTrivia is not null =>
                sesion.ContextoTrivia.PreguntasOrdenadas,
            _ => throw new DomainException(
                "La sesión no tiene una etapa trivia activa.")
        };
        var preguntas    = await _preguntaRepository.FindByIdsAsync(idsOrdenados, cancellationToken);
        var porId        = preguntas.ToDictionary(p => p.PreguntaId.Valor);

        var resultado = new List<PreguntaTriviaParticipanteDto>(idsOrdenados.Count);
        for (var i = 0; i < idsOrdenados.Count; i++)
        {
            var id = idsOrdenados[i].Valor;
            if (!porId.TryGetValue(id, out var pregunta) || pregunta.Eliminada)
                continue;

            resultado.Add(new PreguntaTriviaParticipanteDto(
                i + 1,
                id,
                pregunta.Enunciado,
                pregunta.Dificultad.ToString(),
                pregunta.Opciones.Select(o => o.Texto).ToList()));
        }

        return resultado;
    }
}
