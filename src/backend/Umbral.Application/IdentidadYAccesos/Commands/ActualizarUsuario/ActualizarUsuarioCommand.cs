using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;

public sealed record ActualizarUsuarioCommand(
    Guid UsuarioId,
    string Nombre,
    string Apellido,
    string Rol,
    string? NuevaPassword) : IRequest<Result<bool>>;
