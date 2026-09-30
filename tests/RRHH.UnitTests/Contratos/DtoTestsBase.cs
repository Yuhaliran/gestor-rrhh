using System.ComponentModel.DataAnnotations;
using Xunit;

namespace RRHH.UnitTests.Contratos;

public abstract class DtoTestsBase
{
    protected static void AssertInvalido(object dto, string campoEsperado)
    {
        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        Assert.False(esValido);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(campoEsperado));
    }

    protected static void AssertValido(object dto)
    {
        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        Assert.True(esValido);
        Assert.Empty(validationResults);
    }
}
