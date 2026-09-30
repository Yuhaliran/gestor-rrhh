using RRHH.Contratos.Colaboradores;

using RRHH.UnitTests.Comun;

using Xunit;

namespace RRHH.UnitTests.Contratos;

public class AsociarEmpresaDtoTests : DtoTestsBase
{
    private static AsociarEmpresaDto DtoValido() => new()
    {
        EmpresaId = 1,
        FechaIngreso = new DateOnly(2020, 1, 1),
        Puesto = "Desarrollador"
    };

    [Fact]
    public void Requeridos_ValidanCorrectamente()
    {
        AssertInvalido(DtoValido() with { EmpresaId = null }, "EmpresaId");
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
