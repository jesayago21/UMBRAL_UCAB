using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetEstadoTriviaSesion;

internal sealed class GetEstadoTriviaSesionQueryHandler
    : IRequestHandler<GetEstadoTriviaSesionQuery, EstadoTriviaSesionDto>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IPreguntaRepository _preguntaRepository;

    public GetEstadoTriviaSesionQueryHandler(
        ISesionRepository sesionRepository,
        IPreguntaRepository preguntaRepository)
    {
        _sesionRepository   = sesionRepository;
        _preguntaRepository = preguntaRepository;
    }

    public async Task<EstadoTriviaSesionDto> Handle(
        GetEstadoTriviaSesionQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        var jugadorId = new UsuarioId(query.JugadorId);
        var participante = sesion.Participantes.FirstOrDefault(e => e.JugadorId == jugadorId)
            ?? throw new UnauthorizedAccessException(
                "Debes unirte a la sesión antes de ver el estado de trivia.");

        if (sesion.ContextoMision is null)
            throw new DomainException("La sesión no tiene contexto de misión.");

        var ctx = sesion.ContextoMision;
        var trivia = ctx.ObtenerEtapaTriviaActual();
        var total = trivia.PreguntasOrdenadas.Count;
        var fase = ctx.ObtenerFaseTrivia();
        var index = ctx.PreguntaTriviaActualIndex;

        if (fase != FaseTrivia.PreguntaActiva)
        {
            return new EstadoTriviaSesionDto(
                fase.ToString(),
                null,
                null,
                null,
                null,
                null,
                null,
                fase == FaseTrivia.Transicion ? ctx.TransicionHasta : null,
                index,
                total,
                false,
                null);
        }

        var preguntaId = ctx.ObtenerPreguntaTriviaActualId();
        var pregunta = await _preguntaRepository.FindByIdAsync(preguntaId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Pregunta), preguntaId.Valor);

        var respuesta = sesion.RespuestasTrivia.FirstOrDefault(r =>
            r.ParticipanteId == participante.ParticipanteId
            && r.PreguntaId == preguntaId);

        return new EstadoTriviaSesionDto(
            fase.ToString(),
            index + 1,
            pregunta.PreguntaId.Valor,
            pregunta.Enunciado,
            pregunta.Dificultad.ToString(),
            pregunta.Opciones.Select(o => o.Texto).ToList(),
            ctx.TimerCerradoEn,
            null,
            index,
            total,
            respuesta is not null,
            respuesta?.IndiceOpcion,
            respuesta?.EsCorrecta,
            respuesta?.FueraDeTiempo,
            respuesta?.PuntosOtorgados,
            respuesta is not null ? participante.PuntajeTotal.Valor : null);
    }
}
