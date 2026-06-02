using Microsoft.AspNetCore.Mvc;
using Umbral.API.Models;
using Umbral.Application.Common.Models;

namespace Umbral.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        HttpContext httpContext,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return new BadRequestObjectResult(ToBusinessError(httpContext, result.Errors));
    }

    public static IActionResult ToOkResult<T>(this Result<T> result, HttpContext httpContext)
        => result.ToActionResult(httpContext, value => new OkObjectResult(value));

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        HttpContext httpContext,
        string location,
        object? responseBody = null)
        => result.ToActionResult(
            httpContext,
            value => new CreatedResult(location, responseBody ?? value));

    public static IActionResult ToNoContentResult<T>(
        this Result<T> result,
        HttpContext httpContext)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return new BadRequestObjectResult(ToBusinessError(httpContext, result.Errors));
    }

    private static ApiErrorResponse ToBusinessError(
        HttpContext httpContext,
        IReadOnlyList<string> errors)
    {
        var detalle = errors.Count > 0 ? errors[0] : "La operación no pudo completarse.";
        return new()
        {
            Tipo    = "BusinessError",
            Mensaje = detalle,
            Errores = new Dictionary<string, string[]>
            {
                ["general"] = errors.ToArray()
            },
            TraceId = httpContext.TraceIdentifier
        };
    }
}
