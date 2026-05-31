using MediatR;
using Umbral.Application.Sesion.Models;

namespace Umbral.Application.Sesion.Queries.GetSesionOperador;

public sealed record GetSesionOperadorQuery(Guid SesionId) : IRequest<SesionDetalleDto>;
