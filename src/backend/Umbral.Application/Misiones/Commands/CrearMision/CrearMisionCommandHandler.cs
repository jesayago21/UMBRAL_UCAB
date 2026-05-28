using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Application.Misiones.Commands.CrearMision;

internal sealed class CrearMisionCommandHandler : IRequestHandler<CrearMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;

    public CrearMisionCommandHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<Result<Guid>> Handle(CrearMisionCommand command, CancellationToken cancellationToken)
    {
        var mision = Mision.Crear(command.Nombre);

        foreach (var etapa in command.Etapas)
        {
            mision.AgregarEtapa(etapa.Descripcion, etapa.CodigoQrSolucion);
            var etapaCreada = mision.Etapas.Last();

            foreach (var pista in etapa.Pistas)
            {
                var tipo = Enum.Parse<TipoLiberacion>(pista.TipoLiberacion, ignoreCase: true);
                etapaCreada.AgregarPista(pista.Contenido, tipo, pista.SegundosLiberacion);
            }
        }

        if (command.Activar)
            mision.Activar();

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(mision.MisionId.Valor);
    }
}
