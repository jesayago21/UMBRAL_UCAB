using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetHistorialSesion;

internal sealed class GetHistorialSesionQueryHandler
    : IRequestHandler<GetHistorialSesionQuery, HistorialSesionDto>
{
    private readonly ISesionRepository _sesionRepository;

    public GetHistorialSesionQueryHandler(ISesionRepository sesionRepository) =>
        _sesionRepository = sesionRepository;

    public async Task<HistorialSesionDto> Handle(
        GetHistorialSesionQuery query,
        CancellationToken cancellationToken)
    {
        var sesionId = new SesionId(query.SesionId);
        _ = await _sesionRepository.FindByIdAsync(sesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        var pagina = Math.Max(1, query.Pagina);
        var tamano = Math.Clamp(query.TamanoPagina, 1, 200);

        var total = await _sesionRepository.CountEventosHistorialAsync(sesionId, cancellationToken);
        var eventos = await _sesionRepository.ListEventosHistorialAsync(
            sesionId,
            pagina,
            tamano,
            cancellationToken);

        return new HistorialSesionDto(
            query.SesionId,
            pagina,
            tamano,
            total,
            eventos.Select(e => new EventoSesionDto(
                e.EventoId,
                e.Tipo,
                e.Payload,
                e.OcurridoEn)).ToList());
    }
}
