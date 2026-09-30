using Microsoft.EntityFrameworkCore;

using RRHH.Application.Comun;
using RRHH.Contratos.Comun;
using RRHH.UnitTests.Datos;

namespace RRHH.UnitTests.Comun;

public class PaginacionTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();

    [Fact]
    public async Task PaginarAsync_Pagina1De10_DevuelveLosPrimeros10()
    {
        // Arrange
        // Aseguramos que la base esté creada con los datos iniciales
        var c = new Consulta { Pagina = 1, Tamanio = 10 };

        // Act
        var departamentos = await _bd.Contexto.Departamentos
            .OrderBy(d => d.Id)
            .PaginarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, departamentos.Total);
        Assert.Equal(1, departamentos.Numero);
        Assert.Equal(10, departamentos.Tamanio);
        Assert.Equal(3, departamentos.TotalPaginas);
        Assert.Equal(10, departamentos.Elementos.Count);
        Assert.Equal(1, departamentos.Elementos[0].Id);
        Assert.Equal(10, departamentos.Elementos[9].Id);
    }

    [Fact]
    public async Task PaginarAsync_UltimaPagina_DevuelveElementosRestantes()
    {
        // Arrange
        var c = new Consulta { Pagina = 3, Tamanio = 10 };

        // Act
        var departamentos = await _bd.Contexto.Departamentos
            .OrderBy(d => d.Id)
            .PaginarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, departamentos.Total);
        Assert.Equal(3, departamentos.Numero);
        Assert.Equal(10, departamentos.Tamanio);
        Assert.Equal(3, departamentos.TotalPaginas);
        Assert.Equal(2, departamentos.Elementos.Count);
        Assert.Equal(21, departamentos.Elementos[0].Id);
        Assert.Equal(22, departamentos.Elementos[1].Id);
    }

    [Fact]
    public async Task PaginarAsync_PaginaFueraDeRango_DevuelveListaVacia()
    {
        // Arrange
        var c = new Consulta { Pagina = 4, Tamanio = 10 };

        // Act
        var departamentos = await _bd.Contexto.Departamentos
            .OrderBy(d => d.Id)
            .PaginarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, departamentos.Total);
        Assert.Empty(departamentos.Elementos);
    }

    [Fact]
    public async Task PaginarAsync_PaginaMuyGrande_DevuelveListaVaciaSinDesborde()
    {
        // Arrange
        var c = new Consulta { Pagina = int.MaxValue, Tamanio = 100 };

        // Act
        var departamentos = await _bd.Contexto.Departamentos
            .OrderBy(d => d.Id)
            .PaginarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, departamentos.Total);
        Assert.Empty(departamentos.Elementos);
    }

    public void Dispose() => _bd.Dispose();
}
