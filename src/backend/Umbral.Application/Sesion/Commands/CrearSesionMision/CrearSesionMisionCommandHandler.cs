using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion.Services;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.CrearSesionMision;

internal sealed class CrearSesionMisionCommandHandler
    : IRequestHandler<CrearSesionMisionCommand, Result<CrearSesionMisionResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IMisionRepository _misionRepository;
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IEventPublisher _eventPublisher;

    public CrearSesionMisionCommandHandler(
        ISesionRepository sesionRepository,
        IMisionRepository misionRepository,
        IPreguntaRepository preguntaRepository,
        ICategoriaRepository categoriaRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository    = sesionRepository;
        _misionRepository    = misionRepository;
        _preguntaRepository  = preguntaRepository;
        _categoriaRepository = categoriaRepository;
        _eventPublisher      = eventPublisher;
    }

    public async Task<Result<CrearSesionMisionResult>> Handle(
        CrearSesionMisionCommand command,
        CancellationToken cancellationToken)
    {
        var mision = await _misionRepository.FindByIdAsync(
                         new MisionId(command.MisionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        if (!mision.PuedeUsarseParaSesion())
            throw new DomainException(
                "La misión debe estar activa para crear una sesión (RB-01).");

        var snapshot = await MisionSnapshotFactory.CrearAsync(
            mision,
            _preguntaRepository,
            _categoriaRepository,
            cancellationToken);

        var sesion = SesionAR.CrearDesdeMision(
            snapshot,
            new UsuarioId(command.OperadorId));

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(sesion.DomainEvents, cancellationToken);
        sesion.ClearDomainEvents();

        return Result<CrearSesionMisionResult>.Ok(
            new CrearSesionMisionResult(
                sesion.SesionId.Valor,
                sesion.CodigoAcceso.Valor,
                mision.Nombre));
    }
}
