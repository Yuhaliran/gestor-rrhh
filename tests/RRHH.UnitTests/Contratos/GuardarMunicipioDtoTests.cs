using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Municipios;

using Xunit;

namespace RRHH.UnitTests.Contratos;

public class GuardarMunicipioDtoTests
{
    private static GuardarMunicipioDto DtoBase => new()
    {
        DepartamentoId = 1,
        Nombre = "Un Municipio"
    };

    [Fact]
    public void DepartamentoId_Requerido_ValidaCorrectamente()
    {
        var dtoValido = DtoBase;
        var dtoAusente = DtoBase with { DepartamentoId = null };

        Assert.Empty(Validar(dtoValido));

        var erroresAusente = Validar(dtoAusente);
        Assert.Single(erroresAusente);
        Assert.Contains(erroresAusente, e => e.MemberNames.Contains("DepartamentoId"));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcde")]
    public void Nombre_Limites_ValidaCorrectamente(string nombre)
    {
        var dto = DtoBase with { Nombre = nombre };
        Assert.Empty(Validar(dto));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcde1")]
    public void Nombre_Invalido_ValidaCorrectamente(string? nombre)
    {
        var dto = DtoBase with { Nombre = nombre };
        var errores = Validar(dto);
        Assert.Single(errores);
        Assert.Contains(errores, e => e.MemberNames.Contains("Nombre"));
    }

    private static IList<ValidationResult> Validar(GuardarMunicipioDto dto)
    {
        var resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(dto);
        Validator.TryValidateObject(dto, contexto, resultados, validateAllProperties: true);
        return resultados;
    }
}
