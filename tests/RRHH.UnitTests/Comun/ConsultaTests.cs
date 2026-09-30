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
    public void Validar_PorDefecto_EsValida()
    {
        // Arrange & Act
        var c = new Consulta();

        // Assert
        Assert.True(EsValida(c));
        Assert.Equal(1, c.Pagina);
        Assert.Equal(20, c.Tamanio);
    }

    [Fact]
    public void Validar_PaginaCero_EsInvalida()
    {
        // Arrange
        var c = new Consulta { Pagina = 0 };

        // Act & Assert
        Assert.False(EsValida(c));
    }

    [Fact]
    public void Validar_PaginaUno_EsValida()
    {
        // Arrange
        var c = new Consulta { Pagina = 1 };

        // Act & Assert
        Assert.True(EsValida(c));
    }

    [Fact]
    public void Validar_TamanioCero_EsInvalido()
    {
        // Arrange
        var c = new Consulta { Tamanio = 0 };

        // Act & Assert
        Assert.False(EsValida(c));
    }

    [Fact]
    public void Validar_TamanioUno_EsValido()
    {
        // Arrange
        var c = new Consulta { Tamanio = 1 };

        // Act & Assert
        Assert.True(EsValida(c));
    }

    [Fact]
    public void Validar_TamanioCien_EsValido()
    {
        // Arrange
        var c = new Consulta { Tamanio = 100 };

        // Act & Assert
        Assert.True(EsValida(c));
    }

    [Fact]
    public void Validar_TamanioCientoUno_EsInvalido()
    {
        // Arrange
        var c = new Consulta { Tamanio = 101 };

        // Act & Assert
        Assert.False(EsValida(c));
    }

    [Fact]
    public void Validar_BuscarCienCaracteres_EsValido()
    {
        // Arrange
        var c = new Consulta { Buscar = new string('a', 100) };

        // Act & Assert
        Assert.True(EsValida(c));
    }

    [Fact]
    public void Validar_BuscarCientoUnCaracteres_EsInvalido()
    {
        // Arrange
        var c = new Consulta { Buscar = new string('a', 101) };

        // Act & Assert
        Assert.False(EsValida(c));
    }

    [Fact]
    public void Validar_BuscarNulo_EsValido()
    {
        // Arrange
        var c = new Consulta { Buscar = null };

        // Act & Assert
        Assert.True(EsValida(c));
    }
}
