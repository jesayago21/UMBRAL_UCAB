namespace Umbral.API.Models;

/// <summary>
/// Contrato de error JSON (project-rules §5.3).
/// </summary>
public sealed class ApiErrorResponse
{
    public required string Tipo { get; init; }
    public required string Mensaje { get; init; }
    public IReadOnlyDictionary<string, string[]>? Errores { get; init; }
    public required string TraceId { get; init; }
}
