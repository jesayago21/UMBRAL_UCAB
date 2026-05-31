using MediatR;
using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Queries.ListSesionesDisponiblesEquipo;

internal sealed class ListSesionesDisponiblesEquipoQueryHandler
    : IRequestHandler<ListSesionesDisponiblesEquipoQuery, IReadOnlyList<SesionDisponibleEquipoDto>>
{
    private readonly ISesionRepository _sesionRepository;

    public ListSesionesDisponiblesEquipoQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<IReadOnlyList<SesionDisponibleEquipoDto>> Handle(
        ListSesionesDisponiblesEquipoQuery query,
        CancellationToken cancellationToken)
    {
        var sesiones = await _sesionRepository.FindDisponiblesParaEquipoAsync(
            query.TipoSesion,
            cancellationToken);

        return sesiones.Select(s =>
        {
            var titulo = s.TipoSesion switch
            {
                TipoSesion.Trivia when s.ContextoTrivia is not null =>
                    $"Trivia ({s.ContextoTrivia.TotalPreguntas} preguntas)",
                _ => s.ContextoBT?.MisionSnapshot.Nombre ?? "Sesión"
            };
            return new SesionDisponibleEquipoDto(
                s.SesionId.Valor,
                titulo,
                s.Estado.ToString(),
                s.Equipos.Count);
        }).ToList();
    }
}
