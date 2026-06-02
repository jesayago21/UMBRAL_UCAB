using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;

public sealed record EliminarUsuarioCommand(Guid UsuarioId) : IRequest<Result<bool>>;
