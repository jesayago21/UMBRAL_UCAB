using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;

namespace Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;

public sealed record GetUsuarioByIdQuery(Guid Id) : IRequest<UsuarioDto?>;
