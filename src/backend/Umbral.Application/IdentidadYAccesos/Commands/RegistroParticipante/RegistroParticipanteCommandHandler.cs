using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Application.IdentidadYAccesos.Commands.RegistroParticipante;

internal sealed class RegistroParticipanteCommandHandler
    : IRequestHandler<RegistroParticipanteCommand, Result<RegistroParticipanteResult>>
{
    private readonly IIdentityService _identity;

    public RegistroParticipanteCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Result<RegistroParticipanteResult>> Handle(
        RegistroParticipanteCommand cmd,
        CancellationToken ct)
    {
        var email = EmailAddress.Create(cmd.Email);
        var username = cmd.Username.Trim();

        if (await _identity.ExisteEmailAsync(email, ct))
            return Result<RegistroParticipanteResult>.Fail("Email ya registrado (RB-36).");

        if (await _identity.ExisteUsernameAsync(username, ct))
            return Result<RegistroParticipanteResult>.Fail("Username ya registrado (RB-36).");

        try
        {
            var kcId = await _identity.RegistrarParticipanteEnIdentityServerAsync(
                email,
                username,
                cmd.Nombre.Trim(),
                cmd.Apellido.Trim(),
                cmd.Password,
                ct);

            return Result<RegistroParticipanteResult>.Ok(
                new RegistroParticipanteResult(kcId.Value, email.Value, username));
        }
        catch (Exception ex)
        {
            return Result<RegistroParticipanteResult>.Fail($"Identity server: {ex.Message}");
        }
    }
}
