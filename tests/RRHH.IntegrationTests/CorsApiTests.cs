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
}
