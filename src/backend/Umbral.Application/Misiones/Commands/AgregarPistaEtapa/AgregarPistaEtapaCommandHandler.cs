using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.AgregarPistaEtapa;

internal sealed class AgregarPistaEtapaCommandHandler
    : IRequestHandler<AgregarPistaEtapaCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;

    public AgregarPistaEtapaCommandHandler(IMisionRepository misionRepository) =>
        _misionRepository = misionRepository;

    public async Task<Result<Guid>> Handle(
        AgregarPistaEtapaCommand command,
        CancellationToken cancellationToken)
    {
        var mision = await _misionRepository.FindByIdAsync(new MisionId(command.MisionId), cancellationToken)
            ?? throw new NotFoundException("Misión", command.MisionId);

        var tipo = Enum.Parse<TipoLiberacion>(command.TipoLiberacion, ignoreCase: true);
        mision.AgregarPistaAEtapa(
            new EtapaId(command.EtapaId),
            command.Contenido,
            tipo,
            command.SegundosLiberacion);

        var etapa = mision.Etapas.OfType<EtapaBusquedaTesoro>()
            .First(e => e.EtapaId.Valor == command.EtapaId);
        var pistaId = etapa.Pistas.Last().PistaId.Valor;

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(pistaId);
    }
}
