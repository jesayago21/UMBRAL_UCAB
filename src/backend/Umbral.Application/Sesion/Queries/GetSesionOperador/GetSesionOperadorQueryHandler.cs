using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetSesionOperador;

internal sealed class GetSesionOperadorQueryHandler
    : IRequestHandler<GetSesionOperadorQuery, SesionDetalleDto>
{
    private readonly ISesionRepository _sesionRepository;

    public GetSesionOperadorQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<SesionDetalleDto> Handle(
        GetSesionOperadorQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        return sesion.ToDetalle();
    }
}
