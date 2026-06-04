using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Application.IdentidadYAccesos.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;

public sealed record CrearUsuarioCommand(
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string PasswordTemporal,
    IReadOnlyList<string> Roles) : IRequest<Result<CrearUsuarioResult>>;
