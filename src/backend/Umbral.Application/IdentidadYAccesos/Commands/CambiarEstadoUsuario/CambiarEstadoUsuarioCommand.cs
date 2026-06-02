using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;

public sealed record CambiarEstadoUsuarioCommand(
    Guid UsuarioId,
    string Accion) : IRequest<Result<bool>>;
