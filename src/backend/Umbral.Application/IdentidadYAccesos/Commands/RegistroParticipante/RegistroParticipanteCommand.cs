using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.RegistroParticipante;

public sealed record RegistroParticipanteCommand(
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Password) : IRequest<Result<RegistroParticipanteResult>>;

public sealed record RegistroParticipanteResult(
    Guid KeycloakUserId,
    string Email,
    string Username);
