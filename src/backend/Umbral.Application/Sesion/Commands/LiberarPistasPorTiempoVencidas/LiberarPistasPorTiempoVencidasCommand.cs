using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.LiberarPistasPorTiempoVencidas;

/// <summary>
/// Barrido de liberaciones PorTiempo vencidas en sesiones activas (HU-09 / RB-07).
/// Retorna la cantidad total de entregas realizadas.
/// </summary>
public sealed record LiberarPistasPorTiempoVencidasCommand(DateTimeOffset? Ahora = null)
    : IRequest<Result<int>>;
