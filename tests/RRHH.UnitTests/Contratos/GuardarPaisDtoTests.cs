using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Paises;

namespace RRHH.UnitTests.Contratos;

public class GuardarPaisDtoTests
{
    private static bool EsValido(GuardarPaisDto dto, out List<ValidationResult> resultados)
    {
        resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(dto);
        return Validator.TryValidateObject(dto, contexto, resultados, validateAllProperties: true);
    }

    private static GuardarPaisDto DtoValido() => new GuardarPaisDto
    {
        Nombre = "País Válido",
        CodigoIso2 = "GT",
        EdadMinima = 18,
        EdadMaxima = 100,
        Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
    };

    [Theory]
    [InlineData(null, false)]
    [InlineData("a", true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Nombre_Limites_ValidaCorrectamente(object? valor, bool esperado)
    {
        // Arrange
        var nombre = valor is int longitud ? new string('a', longitud) : (string?)valor;
        var dto = DtoValido() with { Nombre = nombre };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.Equal(esperado, esValido);
    }

    [Theory]
    [InlineData("GT", true)]
    [InlineData("gt", true)]
    [InlineData("G", false)]
    [InlineData("GTM", false)]
    [InlineData("G1", false)]
    [InlineData(null, false)]
    public void CodigoIso2_Limites_ValidaCorrectamente(string? codigo, bool esperado)
    {
        // Arrange
        var dto = DtoValido() with { CodigoIso2 = codigo };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.Equal(esperado, esValido);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(18, true)]
    public void EdadMinima_Limites_ValidaCorrectamente(int? edad, bool esperado)
    {
        // Arrange
        var dto = DtoValido() with { EdadMinima = edad };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.Equal(esperado, esValido);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    public void EdadMaxima_Limites_ValidaCorrectamente(int? edad, bool esperado)
    {
        // Arrange
        var dto = DtoValido() with { EdadMaxima = edad };
        // Si es 0 y minima es 18, fallaría por lógica de mínima > máxima. Así que si la edad maxima no es nula, ajustamos mínima para que no falle por eso
        if (edad.HasValue && edad >= 0)
        {
            dto = dto with { EdadMinima = 0 };
        }

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.Equal(esperado, esValido);
    }

    [Fact]
    public void EdadMinima_IgualAMaxima_ValidaCorrectamente()
    {
        // Arrange
        var dto = DtoValido() with { EdadMinima = 18, EdadMaxima = 18 };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.True(esValido);
    }

    [Fact]
    public void EdadMinima_MayorAMaxima_InvalidoConErrorEnEdadMaxima()
    {
        // Arrange
        var dto = DtoValido() with { EdadMinima = 19, EdadMaxima = 18 };

        // Act
        var esValido = EsValido(dto, out var resultados);

        // Assert
        Assert.False(esValido);
        Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(GuardarPaisDto.EdadMaxima)));
    }

    [Fact]
    public void Regla29Febrero_Ausente_Invalido()
    {
        // Arrange
        var dto = DtoValido() with { Regla29Febrero = null };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.False(esValido);
    }

    [Fact]
    public void Regla29Febrero_Inexistente_Invalido()
    {
        // Arrange
        var dto = DtoValido() with { Regla29Febrero = (Regla29Febrero)99 };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.False(esValido);
    }
}
