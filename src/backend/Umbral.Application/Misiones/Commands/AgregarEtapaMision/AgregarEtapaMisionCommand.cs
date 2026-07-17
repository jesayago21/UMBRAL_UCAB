using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.AgregarEtapaMision;

public sealed record AgregarEtapaMisionCommand(
    Guid MisionId,
    string TipoEtapa,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<Guid>? CategoriaIds,
    IReadOnlyList<AgregarEtapaPistaInput>? Pistas,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null) : IRequest<Result<Guid>>;

public sealed record AgregarEtapaPistaInput(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
