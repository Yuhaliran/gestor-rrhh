using System.ComponentModel.DataAnnotations;

using RRHH.Contratos.Empresas;

using Xunit;

namespace RRHH.UnitTests.Contratos;

public class GuardarEmpresaDtoTests
{
    private static void AssertInvalido(GuardarEmpresaDto dto, string campoEsperado)
    {
        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        Assert.False(esValido);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(campoEsperado));
    }

    private static void AssertValido(GuardarEmpresaDto dto)
    {
        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        Assert.True(esValido);
        Assert.Empty(validationResults);
    }

    private static GuardarEmpresaDto DtoValido() => new()
    {
        MunicipioId = 1,
        Nit = "123456",
        RazonSocial = "Razon Social",
        NombreComercial = "Nombre Comercial",
        Telefono = "5555-1234",
        Correo = "a@a.com"
    };

    [Fact]
    public void Validar_PorDefecto_EsValida()
    {
        AssertValido(DtoValido());
    }

    [Fact]
    public void MunicipioId_Requerido_ValidaCorrectamente()
    {
        AssertInvalido(DtoValido() with { MunicipioId = null }, "MunicipioId");
    }

    [Fact]
    public void Nit_Limites_ValidaCorrectamente()
    {
        AssertInvalido(DtoValido() with { Nit = null }, "Nit");
        AssertInvalido(DtoValido() with { Nit = string.Empty }, "Nit");
        AssertValido(DtoValido() with { Nit = new string('A', 20) });
        AssertInvalido(DtoValido() with { Nit = new string('A', 21) }, "Nit");
    }

    [Fact]
    public void RazonSocial_Limites_ValidaCorrectamente()
    {
        AssertInvalido(DtoValido() with { RazonSocial = null }, "RazonSocial");
        AssertInvalido(DtoValido() with { RazonSocial = string.Empty }, "RazonSocial");
        AssertValido(DtoValido() with { RazonSocial = new string('A', 200) });
        AssertInvalido(DtoValido() with { RazonSocial = new string('A', 201) }, "RazonSocial");
    }

    [Fact]
    public void NombreComercial_Limites_ValidaCorrectamente()
    {
        AssertInvalido(DtoValido() with { NombreComercial = null }, "NombreComercial");
        AssertInvalido(DtoValido() with { NombreComercial = string.Empty }, "NombreComercial");
        AssertValido(DtoValido() with { NombreComercial = new string('A', 200) });
        AssertInvalido(DtoValido() with { NombreComercial = new string('A', 201) }, "NombreComercial");
    }

    [Theory]
    [InlineData("5555-1234")]
    [InlineData("+502 5555 1234")]
    [InlineData("(502) 5555-1234")]
    public void Telefono_Validos_ValidaCorrectamente(string tel)
    {
        AssertValido(DtoValido() with { Telefono = tel });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hola")]
    [InlineData("123456")] // 6 chars (min is 7)
    [InlineData("123456789012345678901")] // 21 chars (max is 20)
    [InlineData("5555_1234")]
    public void Telefono_Invalidos_LanzaError(string? tel)
    {
        AssertInvalido(DtoValido() with { Telefono = tel }, "Telefono");
    }

    [Fact]
    public void Correo_LimitesYFormato_ValidaCorrectamente()
    {
        AssertInvalido(DtoValido() with { Correo = null }, "Correo");
        AssertInvalido(DtoValido() with { Correo = string.Empty }, "Correo");

        // 254 chars max
        var largoValido = new string('a', 244) + "@a.com"; // 244 + 6 = 250
        AssertValido(DtoValido() with { Correo = largoValido });

        var largoInvalido = new string('a', 250) + "@a.com"; // 256
        AssertInvalido(DtoValido() with { Correo = largoInvalido }, "Correo");
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("a@")]
    public void Correo_FormatosInvalidos_LanzaError(string email)
    {
        AssertInvalido(DtoValido() with { Correo = email }, "Correo");
    }
}
