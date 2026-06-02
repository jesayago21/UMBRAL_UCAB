using MediatR;

namespace Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionParticipante;

public sealed record GetPreguntasTriviaSesionParticipanteQuery(
    Guid SesionId,
    Guid JugadorId) : IRequest<IReadOnlyList<PreguntaTriviaParticipanteDto>>;

public sealed record PreguntaTriviaParticipanteDto(
    int Orden,
    Guid Id,
    string Enunciado,
    string Dificultad,
    IReadOnlyList<string> Opciones);
