using Umbral.API.Middlewares;

namespace Umbral.API.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseUmbralExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
