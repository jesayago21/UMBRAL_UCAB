using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.CrearMision;

public sealed record CrearMisionCommand(
    string Nombre,
    IReadOnlyList<EtapaMisionInput> Etapas,
    bool Activar) : IRequest<Result<Guid>>;

public sealed record EtapaMisionInput(
    string TipoEtapa,
    int Orden,
    string? Descripcion,
    string? CodigoQrSolucion,
    IReadOnlyList<CrearPistaInput>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds);

public sealed record CrearPistaInput(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
