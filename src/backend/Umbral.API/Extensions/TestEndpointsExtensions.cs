using FluentValidation;
using FluentValidation.Results;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.Shared;

namespace Umbral.API.Extensions;

/// <summary>
/// Rutas internas para probar el middleware de errores (solo Development/Testing).
/// </summary>
public static class TestEndpointsExtensions
{
    public static void MapUmbralTestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/__test/errors");

        group.MapGet("/validation", ThrowValidation);
        group.MapGet("/domain", ThrowDomain);
        group.MapGet("/notfound", ThrowNotFound);
        group.MapGet("/internal", ThrowInternal);
    }

    private static Task ThrowValidation(CancellationToken _)
    {
        throw new ValidationException(new[]
        {
            new ValidationFailure("misionId", "El identificador de la misión es obligatorio."),
            new ValidationFailure("nombre", "El nombre no puede estar vacío.")
        });
    }

    private static Task ThrowDomain(CancellationToken _)
        => throw new DomainException("Regla de dominio incumplida.");

    private static Task ThrowNotFound(CancellationToken _)
        => throw new NotFoundException("Mision", Guid.NewGuid());

    private static Task ThrowInternal(CancellationToken _)
        => throw new InvalidOperationException("Error simulado.");
}
