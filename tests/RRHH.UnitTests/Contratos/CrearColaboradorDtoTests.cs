using RRHH.Contratos.Colaboradores;

using RRHH.UnitTests.Comun;

using Xunit;

namespace RRHH.UnitTests.Contratos;

public class CrearColaboradorDtoTests : DtoTestsBase
{
    private static CrearColaboradorDto DtoValido() => new()
    {
        NombreCompleto = "Juan Perez",
        FechaNacimiento = new DateOnly(1990, 1, 1),
        Telefono = "5555-1234",
        Correo = "juan@test.com",
        Empresas = new List<AsociarEmpresaDto>
        {
            new() { EmpresaId = 1, FechaIngreso = new DateOnly(2020, 1, 1) }
        }
    };

    [Fact]
    public void Empresas_Obligatorias_ValidaCorrectamente()
    {
        AssertValido(DtoValido());
        AssertInvalido(DtoValido() with { Empresas = null }, "Empresas");
        AssertInvalido(DtoValido() with { Empresas = new List<AsociarEmpresaDto>() }, "Empresas");
    }
}
