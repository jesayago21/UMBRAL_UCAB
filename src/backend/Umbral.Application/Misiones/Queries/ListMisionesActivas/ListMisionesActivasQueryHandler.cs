using MediatR;
using Umbral.Application.Misiones.Models;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Application.Misiones.Queries.ListMisionesActivas;

internal sealed class ListMisionesActivasQueryHandler
    : IRequestHandler<ListMisionesActivasQuery, IReadOnlyList<MisionActivaDto>>
{
    private readonly IMisionRepository _misionRepository;

    public ListMisionesActivasQueryHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<IReadOnlyList<MisionActivaDto>> Handle(
        ListMisionesActivasQuery query,
        CancellationToken cancellationToken)
    {
        var misiones = await _misionRepository.FindActivasAsync(cancellationToken);

        return misiones
            .OrderBy(x => x.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(x => new MisionActivaDto(x.MisionId.Valor, x.Nombre))
            .ToList();
    }
}
