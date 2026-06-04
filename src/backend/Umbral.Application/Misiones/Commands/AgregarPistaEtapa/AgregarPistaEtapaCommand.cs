using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.AgregarPistaEtapa;

public sealed record AgregarPistaEtapaCommand(
    Guid MisionId,
    Guid EtapaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion) : IRequest<Result<Guid>>;
