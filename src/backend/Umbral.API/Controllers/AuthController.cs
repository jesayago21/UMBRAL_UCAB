using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;
using Umbral.API.Auth;
using Umbral.API.Contracts.Auth;
using Umbral.Infrastructure.Auth;

namespace Umbral.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUsuarioAuthRepository _usuarioAuthRepository;
    private readonly JwtTokenIssuer _jwtTokenIssuer;

    public AuthController(
        IUsuarioAuthRepository usuarioAuthRepository,
        JwtTokenIssuer jwtTokenIssuer)
    {
        _usuarioAuthRepository = usuarioAuthRepository;
        _jwtTokenIssuer = jwtTokenIssuer;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add(new ValidationFailure(nameof(request.Email), "El email es obligatorio."));
            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add(new ValidationFailure(nameof(request.Password), "La contraseña es obligatoria."));
            throw new ValidationException(errors);
        }

        var user = await _usuarioAuthRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.Activo)
            return Unauthorized();

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized();

        var token = _jwtTokenIssuer.CreateToken(user);
        var expiresIn = (long)Math.Round((token.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds);

        return Ok(new LoginResponse(
            token.AccessToken,
            "Bearer",
            Math.Max(expiresIn, 0),
            user.Rol));
    }
}
