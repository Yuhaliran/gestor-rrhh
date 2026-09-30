using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Comun;

namespace RRHH.UnitTests.Comun;

public class ConsultaTests
{
    private static bool EsValida(Consulta c)
    {
        var ctx = new ValidationContext(c);
        return Validator.TryValidateObject(c, ctx, null, validateAllProperties: true);
    }

    [Fact]
    public void Consulta_PorDefecto_Valida()
    {
        // Arrange & Act
        var c = new Consulta();

        // Assert
        Assert.True(EsValida(c));
        Assert.Equal(1, c.Pagina);
        Assert.Equal(20, c.Tamanio);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void Consulta_Pagina_Limites(int pagina, bool esperado)
    {
        // Arrange
        var c = new Consulta { Pagina = pagina };

        // Act & Assert
        Assert.Equal(esperado, EsValida(c));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Consulta_Tamanio_Limites(int tamanio, bool esperado)
    {
        // Arrange
        var c = new Consulta { Tamanio = tamanio };

        // Act & Assert
        Assert.Equal(esperado, EsValida(c));
    }

    [Fact]
    public void Consulta_Buscar_Limites()
    {
        // Arrange
        var c100 = new Consulta { Buscar = new string('a', 100) };
        var c101 = new Consulta { Buscar = new string('a', 101) };
        var cNulo = new Consulta { Buscar = null };

        // Act & Assert
        Assert.True(EsValida(c100));
        Assert.False(EsValida(c101));
        Assert.True(EsValida(cNulo));
    }
}
