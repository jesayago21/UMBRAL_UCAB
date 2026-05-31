using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionEquipo;

internal sealed class GetPreguntasTriviaSesionEquipoQueryHandler
    : IRequestHandler<GetPreguntasTriviaSesionEquipoQuery, IReadOnlyList<PreguntaTriviaEquipoDto>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IPreguntaRepository _preguntaRepository;

    public GetPreguntasTriviaSesionEquipoQueryHandler(
        ISesionRepository sesionRepository,
        IPreguntaRepository preguntaRepository)
    {
        _sesionRepository    = sesionRepository;
        _preguntaRepository  = preguntaRepository;
    }

    public async Task<IReadOnlyList<PreguntaTriviaEquipoDto>> Handle(
        GetPreguntasTriviaSesionEquipoQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        if (sesion.TipoSesion != TipoSesion.Trivia || sesion.ContextoTrivia is null)
            throw new DomainException("La sesión no es de tipo trivia.");

        var jugadorId = new UsuarioId(query.JugadorId);
        if (!sesion.Equipos.Any(e => e.JugadorId == jugadorId))
            throw new UnauthorizedAccessException(
                "Debes unirte a la sesión antes de ver las preguntas.");

        var idsOrdenados = sesion.ContextoTrivia.PreguntasOrdenadas;
        var preguntas    = await _preguntaRepository.FindByIdsAsync(idsOrdenados, cancellationToken);
        var porId        = preguntas.ToDictionary(p => p.PreguntaId.Valor);

        var resultado = new List<PreguntaTriviaEquipoDto>(idsOrdenados.Count);
        for (var i = 0; i < idsOrdenados.Count; i++)
        {
            var id = idsOrdenados[i].Valor;
            if (!porId.TryGetValue(id, out var pregunta) || pregunta.Eliminada)
                continue;

            resultado.Add(new PreguntaTriviaEquipoDto(
                i + 1,
                id,
                pregunta.Enunciado,
                pregunta.Dificultad.ToString(),
                pregunta.Opciones.Select(o => o.Texto).ToList()));
        }

        return resultado;
    }
}
