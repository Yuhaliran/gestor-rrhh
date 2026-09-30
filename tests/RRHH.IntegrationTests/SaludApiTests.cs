using System.Net;

namespace RRHH.IntegrationTests;

public class SaludApiTests : PruebaIntegracionBase
{
    public SaludApiTests(FabricaApi fabrica) : base(fabrica) { }

    [Fact]
    public async Task GetHealth_DevuelveHealthy()
    {
        // Arrange
        var requestUri = "/health";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var contenido = await respuesta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Healthy", contenido);
    }
}
