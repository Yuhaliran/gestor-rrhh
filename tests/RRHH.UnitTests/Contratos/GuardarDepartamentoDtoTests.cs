using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Departamentos;

namespace RRHH.UnitTests.Contratos;

public class GuardarDepartamentoDtoTests
{
    private static bool EsValido(GuardarDepartamentoDto dto, out List<ValidationResult> resultados)
    {
        resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(dto);
        return Validator.TryValidateObject(dto, contexto, resultados, validateAllProperties: true);
    }

    private static GuardarDepartamentoDto DtoValido() => new GuardarDepartamentoDto
    {
        PaisId = 1,
        Nombre = "Departamento Válido"
    };

    [Theory]
    [InlineData(null, false)]
    [InlineData(1, true)]
    public void PaisId_Requerido_ValidaCorrectamente(int? paisId, bool esperado)
    {
        // Arrange
        var dto = DtoValido() with { PaisId = paisId };

        // Act
        var esValido = EsValido(dto, out _);

        // Assert
        Assert.Equal(esperado, esValido);
    }

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
}
