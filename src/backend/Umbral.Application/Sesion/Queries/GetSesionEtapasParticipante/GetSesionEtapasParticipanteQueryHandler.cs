using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetSesionEtapasParticipante;

internal sealed class GetSesionEtapasParticipanteQueryHandler
    : IRequestHandler<GetSesionEtapasParticipanteQuery, SesionEtapasParticipanteDto>
{
    private readonly ISesionRepository _sesionRepository;

    public GetSesionEtapasParticipanteQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<SesionEtapasParticipanteDto> Handle(
        GetSesionEtapasParticipanteQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(query.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), query.SesionId);

        var jugadorId = new UsuarioId(query.JugadorId);
        if (!sesion.Participantes.Any(e => e.JugadorId == jugadorId))
        {
            throw new UnauthorizedAccessException(
                "Debes unirte a la sesión antes de ver las etapas.");
        }

        var detalle = sesion.ToDetalle();
        var etapas  = detalle.Etapas ?? Array.Empty<EtapaSesionDto>();

        return new SesionEtapasParticipanteDto(
            detalle.Estado,
            detalle.TotalEtapas,
            etapas);
    }
}
