using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RRHH.Api.Errores;
using RRHH.Application.Excepciones;

namespace RRHH.IntegrationTests.Errores;

public class ManejadorExcepcionesTests
{
    private readonly ManejadorExcepciones _manejador;
    private readonly DefaultHttpContext _ctx;
    private readonly MemoryStream _body;

    public ManejadorExcepcionesTests()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddLogging(); // Necesario para AddProblemDetails
        var provider = services.BuildServiceProvider();

        _manejador = new ManejadorExcepciones(provider.GetRequiredService<IProblemDetailsService>());

        _body = new MemoryStream();
        _ctx = new DefaultHttpContext
        {
            RequestServices = provider
        };
        _ctx.Request.Headers.Accept = "application/json";
        _ctx.Response.Body = _body;
    }

    private async Task<ProblemDetails?> LeerRespuesta()
    {
        _body.Position = 0;
        if (_body.Length == 0) return null;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(_body, options);
    }

    private async Task<HttpValidationProblemDetails?> LeerValidacion()
    {
        _body.Position = 0;
        if (_body.Length == 0) return null;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return await JsonSerializer.DeserializeAsync<HttpValidationProblemDetails>(_body, options);
    }

    [Fact]
    public async Task TryHandleAsync_NoEncontradoException_Responde404()
    {
        // Arrange
        var ex = new NoEncontradoException("Pais", 1);

        // Act
        var result = await _manejador.TryHandleAsync(_ctx, ex, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(404, _ctx.Response.StatusCode);
        var details = await LeerRespuesta();
        Assert.NotNull(details);
        Assert.Equal(404, details.Status);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictoException_Responde409()
    {
        // Arrange
        var ex = new ConflictoException("Conflicto");

        // Act
        var result = await _manejador.TryHandleAsync(_ctx, ex, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(409, _ctx.Response.StatusCode);
        var details = await LeerRespuesta();
        Assert.NotNull(details);
        Assert.Equal(409, details.Status);
    }

    [Fact]
    public async Task TryHandleAsync_DbUpdateException_Responde409SinDetalleInterno()
    {
        // Arrange
        var ex = new DbUpdateException("Mensaje interno secreto de la base de datos.");

        // Act
        var result = await _manejador.TryHandleAsync(_ctx, ex, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(409, _ctx.Response.StatusCode);
        var details = await LeerRespuesta();
        Assert.NotNull(details);
        Assert.Equal(409, details.Status);
        Assert.DoesNotContain("secreto", details.Detail ?? string.Empty);
    }

    [Fact]
    public async Task TryHandleAsync_ValidacionException_Responde400ConPropiedad()
    {
        // Arrange
        var ex = new ValidacionException("Nombre", "El nombre es invÃ¡lido");

        // Act
        var result = await _manejador.TryHandleAsync(_ctx, ex, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(400, _ctx.Response.StatusCode);
        var details = await LeerValidacion();
        Assert.NotNull(details);
        Assert.Equal(400, details.Status);
        Assert.Contains(details.Errors, e => e.Key == "Nombre" && e.Value.Contains("El nombre es invÃ¡lido"));
    }

    [Fact]
    public async Task TryHandleAsync_OtraExcepcion_DevuelveFalse()
    {
        // Arrange
        var ex = new Exception("Error genÃ©rico");

        // Act
        var result = await _manejador.TryHandleAsync(_ctx, ex, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
        var details = await LeerRespuesta();
        Assert.Null(details);
    }
}
