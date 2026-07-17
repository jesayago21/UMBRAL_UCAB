using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Misiones.Commands.EditarPistaEtapa;

public sealed record EditarPistaEtapaCommand(
    Guid MisionId,
    Guid EtapaId,
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion) : IRequest<Result<Guid>>;
