using System.Text.Json;
using FluentValidation;
using Umbral.API.Models;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.Shared;

namespace Umbral.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, tipo, mensaje, errores) = MapException(ex);

        if (statusCode >= 500)
            _logger.LogError(ex, "Error interno no controlado");
        else
            _logger.LogWarning(ex, "Error controlado: {Tipo}", tipo);

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        var body = new ApiErrorResponse
        {
            Tipo     = tipo,
            Mensaje  = mensaje,
            Errores  = errores,
            TraceId  = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(body, JsonOptions);
    }

    private static (int StatusCode, string Tipo, string Mensaje, IReadOnlyDictionary<string, string[]>? Errores)
        MapException(Exception ex) =>
        ex switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "ValidationError",
                "La solicitud contiene errores de validación.",
                validation.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray())),

            DomainException domain => (
                StatusCodes.Status400BadRequest,
                "DomainError",
                domain.Message,
                null),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "NotFound",
                ex.Message,
                null),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ex.Message,
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "InternalServerError",
                "Ocurrió un error interno en el servidor.",
                null)
        };

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }
}
