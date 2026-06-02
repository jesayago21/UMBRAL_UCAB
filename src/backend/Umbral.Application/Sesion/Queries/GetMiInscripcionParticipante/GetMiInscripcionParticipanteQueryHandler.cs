using MediatR;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;

internal sealed class GetMiInscripcionParticipanteQueryHandler
    : IRequestHandler<GetMiInscripcionParticipanteQuery, MiInscripcionParticipanteDto?>
{
    private readonly ISesionRepository _sesionRepository;

    public GetMiInscripcionParticipanteQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<MiInscripcionParticipanteDto?> Handle(
        GetMiInscripcionParticipanteQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindInscripcionAbiertaPorJugadorAsync(
            new UsuarioId(query.JugadorId),
            cancellationToken);

        if (sesion is null)
            return null;

        var participante = sesion.Participantes
            .FirstOrDefault(p => p.JugadorId.Valor == query.JugadorId);

        if (participante is null)
            return null;

        var titulo = sesion.Nombre;

        var detalle = sesion.ToDetalle();

        return new MiInscripcionParticipanteDto(
            sesion.SesionId.Valor,
            titulo,
            participante.ParticipanteId.Valor,
            sesion.Estado.ToString(),
            detalle.TotalEtapas,
            detalle.Etapas ?? Array.Empty<EtapaSesionDto>());
    }
}
