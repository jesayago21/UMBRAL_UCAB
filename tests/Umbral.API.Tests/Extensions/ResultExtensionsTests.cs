using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbral.API.Extensions;
using Umbral.API.Models;
using Umbral.Application.Common.Models;

namespace Umbral.API.Tests.Extensions;

/// <summary>
/// Unit tests de <see cref="ResultExtensions"/>: mapeo de Result&lt;T&gt; a IActionResult.
/// </summary>
public sealed class ResultExtensionsTests
{
    private static HttpContext NuevoHttpContext()
        => new DefaultHttpContext { TraceIdentifier = "trace-123" };

    [Fact]
    public void ToOkResult_CuandoExito_DevuelveOkConValor()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<int>.Ok(42);

        // Act
        var action = result.ToOkResult(http);

        // Assert
        var ok = action.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(42);
    }

    [Fact]
    public void ToOkResult_CuandoFallo_DevuelveBadRequestConBusinessError()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<int>.Fail("error A", "error B");

        // Act
        var action = result.ToOkResult(http);

        // Assert
        var bad = action.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = bad.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        error.Tipo.Should().Be("BusinessError");
        error.TraceId.Should().Be("trace-123");
        error.Errores!["general"].Should().BeEquivalentTo("error A", "error B");
    }

    [Fact]
    public void ToCreatedResult_CuandoExito_DevuelveCreatedConLocationYValor()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<Guid>.Ok(Guid.NewGuid());

        // Act
        var action = result.ToCreatedResult(http, "/api/v1/recurso/1");

        // Assert
        var created = action.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("/api/v1/recurso/1");
        created.Value.Should().Be(result.Value);
    }

    [Fact]
    public void ToCreatedResult_CuandoExitoConResponseBody_UsaElBody()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<Guid>.Ok(Guid.NewGuid());
        var body = new { id = 1 };

        // Act
        var action = result.ToCreatedResult(http, "/api/v1/recurso/1", body);

        // Assert
        var created = action.Should().BeOfType<CreatedResult>().Subject;
        created.Value.Should().BeSameAs(body);
    }

    [Fact]
    public void ToCreatedResult_CuandoFallo_DevuelveBadRequest()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<Guid>.Fail("no creado");

        // Act
        var action = result.ToCreatedResult(http, "/api/v1/recurso/1");

        // Assert
        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ToNoContentResult_CuandoExito_Devuelve204()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<Guid>.Ok(Guid.NewGuid());

        // Act
        var action = result.ToNoContentResult(http);

        // Assert
        action.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToNoContentResult_CuandoFallo_DevuelveBadRequestConBusinessError()
    {
        // Arrange
        var http = NuevoHttpContext();
        var result = Result<Guid>.Fail("no se pudo");

        // Act
        var action = result.ToNoContentResult(http);

        // Assert
        var bad = action.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ApiErrorResponse>()
            .Which.Errores!["general"].Should().BeEquivalentTo("no se pudo");
    }
}
