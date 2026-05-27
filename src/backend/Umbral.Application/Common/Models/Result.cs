namespace Umbral.Application.Common.Models;

/// <summary>
/// Resultado de un caso de uso de comando (éxito o lista de errores).
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value     = value;
        Errors    = [];
    }

    private Result(IReadOnlyList<string> errors)
    {
        IsSuccess = false;
        Value     = default!;
        Errors    = errors;
    }

    public static Result<T> Ok(T value) => new(value);

    public static Result<T> Fail(params string[] errors) => new(errors);
}
