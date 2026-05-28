using MediatR;
using Umbral.Application.Misiones.Models;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Application.Misiones.Queries.ListMisiones;

internal sealed class ListMisionesQueryHandler : IRequestHandler<ListMisionesQuery, IReadOnlyList<MisionDto>>
{
    private readonly IMisionRepository _misionRepository;

    public ListMisionesQueryHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<IReadOnlyList<MisionDto>> Handle(ListMisionesQuery query, CancellationToken cancellationToken)
    {
        var misiones = await _misionRepository.FindAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.Nombre))
        {
            misiones = misiones
                .Where(x => x.Nombre.Contains(query.Nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            var estado = query.Estado.Trim().ToLowerInvariant();
            misiones = estado switch
            {
                "activa" => misiones.Where(x => x.Estado == EstadoMision.Activa).ToList(),
                "inactiva" => misiones.Where(x => x.Estado != EstadoMision.Activa).ToList(),
                _ => misiones
            };
        }

        return misiones
            .OrderBy(x => x.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.ToDto())
            .ToList();
    }
}
