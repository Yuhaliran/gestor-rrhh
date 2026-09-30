using Microsoft.EntityFrameworkCore;

using RRHH.Domain.Reglas;

namespace RRHH.UnitTests.Datos;

public class DatosInicialesTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();

    [Fact]
    public async Task Semilla_Pais_CargaGuatemalaConValoresPorDefecto()
    {
        // Arrange & Act
        var guatemala = await _bd.Contexto.Paises.FindAsync([1], TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(guatemala);
        Assert.Equal("Guatemala", guatemala.Nombre);
        Assert.Equal("GT", guatemala.CodigoIso2);
        Assert.Equal(18, guatemala.EdadMinima);
        Assert.Equal(100, guatemala.EdadMaxima);
        Assert.Equal(Regla29Febrero.VeintiochoDeFebrero, guatemala.Regla29Febrero);
    }

    [Theory]
    [InlineData(1, "Guatemala", "Guatemala")]
    [InlineData(2, "El Progreso", "Guastatoya")]
    [InlineData(3, "Sacatepéquez", "Antigua Guatemala")]
    [InlineData(4, "Chimaltenango", "Chimaltenango")]
    [InlineData(5, "Escuintla", "Escuintla")]
    [InlineData(6, "Santa Rosa", "Cuilapa")]
    [InlineData(7, "Sololá", "Sololá")]
    [InlineData(8, "Totonicapán", "Totonicapán")]
    [InlineData(9, "Quetzaltenango", "Quetzaltenango")]
    [InlineData(10, "Suchitepéquez", "Mazatenango")]
    [InlineData(11, "Retalhuleu", "Retalhuleu")]
    [InlineData(12, "San Marcos", "San Marcos")]
    [InlineData(13, "Huehuetenango", "Huehuetenango")]
    [InlineData(14, "Quiché", "Santa Cruz del Quiché")]
    [InlineData(15, "Baja Verapaz", "Salamá")]
    [InlineData(16, "Alta Verapaz", "Cobán")]
    [InlineData(17, "Petén", "Flores")]
    [InlineData(18, "Izabal", "Puerto Barrios")]
    [InlineData(19, "Zacapa", "Zacapa")]
    [InlineData(20, "Chiquimula", "Chiquimula")]
    [InlineData(21, "Jalapa", "Jalapa")]
    [InlineData(22, "Jutiapa", "Jutiapa")]
    public async Task Semilla_Departamentos_Carga22DepartamentosYCabeceras(int id, string nombreDepartamento, string nombreCabecera)
    {
        // Arrange & Act
        var departamento = await _bd.Contexto.Departamentos.FindAsync([id], TestContext.Current.CancellationToken);
        var cabecera = await _bd.Contexto.Municipios.FindAsync([id], TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(departamento);
        Assert.Equal(1, departamento.PaisId);
        Assert.Equal(nombreDepartamento, departamento.Nombre);

        Assert.NotNull(cabecera);
        Assert.Equal(id, cabecera.DepartamentoId);
        Assert.Equal(nombreCabecera, cabecera.Nombre);
    }

    [Fact]
    public async Task Semilla_Departamentos_CargaExactamente22()
    {
        // Arrange & Act
        var cantidad = await _bd.Contexto.Departamentos.CountAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, cantidad);
    }

    [Fact]
    public async Task Semilla_NoCargaEmpresasNiColaboradores()
    {
        // Arrange & Act
        var empresas = await _bd.Contexto.Empresas.AnyAsync(TestContext.Current.CancellationToken);
        var colaboradores = await _bd.Contexto.Colaboradores.AnyAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.False(empresas, "No deben existir empresas iniciales");
        Assert.False(colaboradores, "No deben existir colaboradores iniciales");
    }

    public void Dispose() => _bd.Dispose();
}
