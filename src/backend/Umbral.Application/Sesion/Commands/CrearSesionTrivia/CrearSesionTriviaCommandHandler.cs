using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.CrearSesionTrivia;

internal sealed class CrearSesionTriviaCommandHandler
    : IRequestHandler<CrearSesionTriviaCommand, Result<CrearSesionTriviaResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly IEventPublisher _eventPublisher;

    public CrearSesionTriviaCommandHandler(
        ISesionRepository sesionRepository,
        ICategoriaRepository categoriaRepository,
        IPreguntaRepository preguntaRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository    = sesionRepository;
        _categoriaRepository = categoriaRepository;
        _preguntaRepository  = preguntaRepository;
        _eventPublisher      = eventPublisher;
    }

    public async Task<Result<CrearSesionTriviaResult>> Handle(
        CrearSesionTriviaCommand command,
        CancellationToken cancellationToken)
    {
        var idsUnicos = command.CategoriaIds.Distinct().ToList();
        var preguntasOrdenadas = new List<PreguntaId>();
        var nombresCategorias    = new List<string>();
        var vistos               = new HashSet<Guid>();

        foreach (var categoriaId in idsUnicos)
        {
            var categoria = await _categoriaRepository.FindByIdAsync(
                                new CategoriaId(categoriaId),
                                cancellationToken)
                            ?? throw new NotFoundException(nameof(Categoria), categoriaId);

            nombresCategorias.Add(categoria.Nombre);

            var preguntas = await _preguntaRepository.FindByCategoriaAsync(
                categoria.CategoriaId,
                cancellationToken);

            foreach (var pregunta in preguntas
                         .Where(p => !p.Eliminada && p.CategoriaId is not null)
                         .OrderBy(p => p.Enunciado, StringComparer.OrdinalIgnoreCase))
            {
                if (vistos.Add(pregunta.PreguntaId.Valor))
                    preguntasOrdenadas.Add(pregunta.PreguntaId);
            }
        }

        if (preguntasOrdenadas.Count == 0)
            throw new DomainException(
                "Las categorías seleccionadas no tienen preguntas activas con categoría asignada.");

        var snapshot = MisionSnapshot.SoloTrivia(
            preguntasOrdenadas,
            string.Join(", ", nombresCategorias));
        var sesion = SesionAR.CrearDesdeMision(
            snapshot,
            new UsuarioId(command.OperadorId));

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<CrearSesionTriviaResult>.Ok(
            new CrearSesionTriviaResult(
                sesion.SesionId.Valor,
                sesion.CodigoAcceso.Valor));
    }
}
