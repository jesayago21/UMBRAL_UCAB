using MediatR;
using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Queries.ListSesionesDisponiblesParticipante;

internal sealed class ListSesionesDisponiblesParticipanteQueryHandler
    : IRequestHandler<ListSesionesDisponiblesParticipanteQuery, IReadOnlyList<SesionDisponibleParticipanteDto>>
{
    private readonly ISesionRepository _sesionRepository;

    public ListSesionesDisponiblesParticipanteQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<IReadOnlyList<SesionDisponibleParticipanteDto>> Handle(
        ListSesionesDisponiblesParticipanteQuery query,
        CancellationToken cancellationToken)
    {
        var sesiones = await _sesionRepository.FindDisponiblesParaParticipanteAsync(
            query.TipoSesion,
            cancellationToken);

        return sesiones.Select(s =>
        {
            return new SesionDisponibleParticipanteDto(
                s.SesionId.Valor,
                s.Nombre,
                s.Estado.ToString(),
                s.Participantes.Count);
        }).ToList();
    }
}
