using MediatR;

namespace Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionEquipo;

public sealed record GetPreguntasTriviaSesionEquipoQuery(
    Guid SesionId,
    Guid JugadorId) : IRequest<IReadOnlyList<PreguntaTriviaEquipoDto>>;

public sealed record PreguntaTriviaEquipoDto(
    int Orden,
    Guid Id,
    string Enunciado,
    string Dificultad,
    IReadOnlyList<string> Opciones);
