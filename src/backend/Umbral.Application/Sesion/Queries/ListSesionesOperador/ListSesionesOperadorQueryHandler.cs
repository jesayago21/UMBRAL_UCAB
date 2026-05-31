using MediatR;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Queries.ListSesionesOperador;

internal sealed class ListSesionesOperadorQueryHandler
    : IRequestHandler<ListSesionesOperadorQuery, IReadOnlyList<SesionResumenDto>>
{
    private readonly ISesionRepository _sesionRepository;

    public ListSesionesOperadorQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<IReadOnlyList<SesionResumenDto>> Handle(
        ListSesionesOperadorQuery query,
        CancellationToken cancellationToken)
    {
        var sesiones = await _sesionRepository.FindOperativasByOperadorAsync(
            new UsuarioId(query.OperadorId),
            cancellationToken);

        return sesiones
            .Select(s => s.ToResumen())
            .ToList();
    }
}
