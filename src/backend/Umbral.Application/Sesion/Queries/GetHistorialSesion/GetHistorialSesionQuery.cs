using MediatR;

namespace Umbral.Application.Sesion.Queries.GetHistorialSesion;

public sealed record GetHistorialSesionQuery(
    Guid SesionId,
    int Pagina = 1,
    int TamanoPagina = 50) : IRequest<HistorialSesionDto>;

public sealed record HistorialSesionDto(
    Guid SesionId,
    int Pagina,
    int TamanoPagina,
    int TotalEventos,
    IReadOnlyList<EventoSesionDto> Eventos);

public sealed record EventoSesionDto(
    Guid EventoId,
    string Tipo,
    string Payload,
    DateTime OcurridoEn);
