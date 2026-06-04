using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Models;
using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Application.Misiones.Queries.GetMisionById;

internal sealed class GetMisionByIdQueryHandler : IRequestHandler<GetMisionByIdQuery, MisionDto>
{
    private readonly IMisionRepository _misionRepository;

    public GetMisionByIdQueryHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<MisionDto> Handle(GetMisionByIdQuery query, CancellationToken cancellationToken)
    {
        var mision = await _misionRepository.FindByIdAsync(new MisionId(query.MisionId), cancellationToken)
            ?? throw new NotFoundException(nameof(Mision), query.MisionId);

        return mision.ToDto();
    }
}
