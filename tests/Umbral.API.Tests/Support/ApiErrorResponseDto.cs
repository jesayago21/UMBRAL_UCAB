namespace Umbral.API.Tests.Support;

public sealed class ApiErrorResponseDto
{
    public string Tipo { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public Dictionary<string, string[]>? Errores { get; init; }
    public string TraceId { get; init; } = string.Empty;
}
