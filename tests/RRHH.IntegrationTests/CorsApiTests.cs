namespace RRHH.IntegrationTests;

public class CorsApiTests : PruebaIntegracionBase
{
    public CorsApiTests(FabricaApi fabrica) : base(fabrica) { }

    [Fact]
    public async Task Preflight_OrigenPermitido_DevuelveAccessControlAllowOrigin()
    {
        // Arrange
        using var peticion = new HttpRequestMessage(HttpMethod.Options, "/api/paises");
        peticion.Headers.Add("Origin", "http://localhost:4200");
        peticion.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var respuesta = await Cliente.SendAsync(peticion, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(respuesta.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Contains("http://localhost:4200", respuesta.Headers.GetValues("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_OrigenNoPermitido_NoDevuelveAccessControlAllowOrigin()
    {
        // Arrange
        using var peticion = new HttpRequestMessage(HttpMethod.Options, "/api/paises");
        peticion.Headers.Add("Origin", "http://otro-origen.com");
        peticion.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var respuesta = await Cliente.SendAsync(peticion, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(respuesta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Preflight_MetodosModificacionConContentType_DevuelveOrigenMetodoYEncabezadosPermitidos(string metodo)
    {
        // Arrange
        using var peticion = new HttpRequestMessage(HttpMethod.Options, "/api/paises");
        peticion.Headers.Add("Origin", "http://localhost:4200");
        peticion.Headers.Add("Access-Control-Request-Method", metodo);
        peticion.Headers.Add("Access-Control-Request-Headers", "content-type");

        // Act
        var respuesta = await Cliente.SendAsync(peticion, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(respuesta.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Contains("http://localhost:4200", respuesta.Headers.GetValues("Access-Control-Allow-Origin"));

        Assert.True(respuesta.Headers.Contains("Access-Control-Allow-Methods"));
        var metodosPermitidos = respuesta.Headers.GetValues("Access-Control-Allow-Methods")
            .SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains(metodo, metodosPermitidos, StringComparer.OrdinalIgnoreCase);

        Assert.True(respuesta.Headers.Contains("Access-Control-Allow-Headers"));
        var encabezadosPermitidos = respuesta.Headers.GetValues("Access-Control-Allow-Headers")
            .SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("content-type", encabezadosPermitidos, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Get_ConOrigenPermitido_ExponeEncabezadoLocation()
    {
        // Arrange
        using var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/paises");
        peticion.Headers.Add("Origin", "http://localhost:4200");

        // Act
        var respuesta = await Cliente.SendAsync(peticion, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(respuesta.Headers.Contains("Access-Control-Expose-Headers"));
        var encabezadosExpuestos = respuesta.Headers.GetValues("Access-Control-Expose-Headers")
            .SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("Location", encabezadosExpuestos, StringComparer.OrdinalIgnoreCase);
    }
}
