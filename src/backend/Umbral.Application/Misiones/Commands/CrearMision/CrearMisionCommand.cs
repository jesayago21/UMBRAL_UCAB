using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.CrearMision;

public sealed record CrearMisionCommand(
    string Nombre,
    IReadOnlyList<CrearEtapaInput> Etapas,
    bool Activar) : IRequest<Result<Guid>>;

public sealed record CrearEtapaInput(
    string Descripcion,
    string CodigoQrSolucion,
    IReadOnlyList<CrearPistaInput> Pistas);

public sealed record CrearPistaInput(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);
