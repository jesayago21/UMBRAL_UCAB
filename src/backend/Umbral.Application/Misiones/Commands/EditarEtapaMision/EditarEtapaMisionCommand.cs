using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.EditarEtapaMision;

public sealed record EditarEtapaMisionCommand(
    Guid MisionId,
    Guid EtapaId,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<Guid>? CategoriaIds,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null) : IRequest<Result<Guid>>;
