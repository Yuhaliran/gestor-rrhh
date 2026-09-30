using RRHH.Contratos.Colaboradores;

using Xunit;

namespace RRHH.UnitTests.Contratos;

public class GuardarEmpresaColaboradorDtoTests : DtoTestsBase
{
    private static GuardarEmpresaColaboradorDto DtoValido() => new()
    {
        FechaIngreso = new DateOnly(2020, 1, 1),
        Puesto = "Desarrollador"
    };

    [Fact]
    public void Requeridos_ValidanCorrectamente()
    {
        AssertInvalido(DtoValido() with { FechaIngreso = null }, "FechaIngreso");
        AssertValido(DtoValido() with { Puesto = null }); // Opcional
    }

    [Fact]
    public void Puesto_Limites_ValidaCorrectamente()
    {
        AssertValido(DtoValido() with { Puesto = new string('a', 100) });
        AssertInvalido(DtoValido() with { Puesto = new string('a', 101) }, "Puesto");
    }
}
