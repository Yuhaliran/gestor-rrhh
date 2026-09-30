using RRHH.Contratos.Colaboradores;

using RRHH.UnitTests.Comun;
using Xunit;

namespace RRHH.UnitTests.Contratos;

public class GuardarColaboradorDtoTests : DtoTestsBase
{
    private static GuardarColaboradorDto DtoValido() => new()
    {
        NombreCompleto = "Juan Perez",
        FechaNacimiento = new DateOnly(1990, 1, 1),
        Telefono = "5555-1234",
        Correo = "juan@test.com"
    };

    [Fact]
    public void Requeridos_ValidanCorrectamente()
    {
        AssertInvalido(DtoValido() with { NombreCompleto = null }, "NombreCompleto");
        AssertInvalido(DtoValido() with { NombreCompleto = string.Empty }, "NombreCompleto");
        AssertInvalido(DtoValido() with { FechaNacimiento = null }, "FechaNacimiento");
        AssertInvalido(DtoValido() with { Telefono = null }, "Telefono");
        AssertInvalido(DtoValido() with { Correo = null }, "Correo");
    }

    [Fact]
    public void NombreCompleto_Limites_ValidaCorrectamente()
    {
        AssertValido(DtoValido() with { NombreCompleto = new string('a', 200) });
        AssertInvalido(DtoValido() with { NombreCompleto = new string('a', 201) }, "NombreCompleto");
    }

    [Fact]
    public void Correo_LimitesYFormato_ValidaCorrectamente()
    {
        var largoValido = new string('a', 248) + "@a.com"; // 254
        AssertValido(DtoValido() with { Correo = largoValido });

        var largoInvalido = new string('a', 249) + "@a.com"; // 255
        AssertInvalido(DtoValido() with { Correo = largoInvalido }, "Correo");
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("a@")]
    public void Correo_FormatosInvalidos_LanzaError(string email)
    {
        AssertInvalido(DtoValido() with { Correo = email }, "Correo");
    }

    [Theory]
    [InlineData("5555-1234")]
    [InlineData("+502 5555 1234")]
    [InlineData("(502) 5555-1234")]
    [InlineData("1234567")] // 7 chars (min)
    [InlineData("12345678901234567890")] // 20 chars (max)
    public void Telefono_Validos_ValidaCorrectamente(string tel)
    {
        AssertValido(DtoValido() with { Telefono = tel });
    }

    [Theory]
    [InlineData("hola")]
    [InlineData("123456")] // 6 chars (min is 7)
    [InlineData("123456789012345678901")] // 21 chars (max is 20)
    [InlineData("5555_1234")]
    public void Telefono_Invalidos_LanzaError(string tel)
    {
        AssertInvalido(DtoValido() with { Telefono = tel }, "Telefono");
    }
}
