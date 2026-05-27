using Microsoft.AspNetCore.Mvc;
using Umbral.API.Models;
using Umbral.Application.Common.Models;

namespace Umbral.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Tipo    = "BusinessError",
            Mensaje = "La operación no pudo completarse.",
            Errores = new Dictionary<string, string[]>
            {
                ["general"] = result.Errors.ToArray()
            },
            TraceId = string.Empty
        });
    }

    public static IActionResult ToOkResult<T>(this Result<T> result)
        => result.ToActionResult(value => new OkObjectResult(value));

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        object routeValues,
        object? responseBody = null)
        => result.ToActionResult(value =>
            new CreatedAtActionResult(
                actionName,
                null,
                routeValues,
                responseBody ?? value));
}
