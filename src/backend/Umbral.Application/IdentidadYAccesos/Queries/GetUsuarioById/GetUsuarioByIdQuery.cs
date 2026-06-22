using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;

namespace Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;

public sealed record GetUsuarioByIdQuery(Guid KeycloakUserId) : IRequest<UsuarioDto?>;
