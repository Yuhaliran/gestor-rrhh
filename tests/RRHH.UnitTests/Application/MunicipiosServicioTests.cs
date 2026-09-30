using RRHH.Application.Excepciones;
using RRHH.Application.Servicios;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;
using RRHH.Domain.Entidades;
using RRHH.Domain.Reglas;
using RRHH.UnitTests.Datos;

using Xunit;

namespace RRHH.UnitTests.Application;

public class MunicipiosServicioTests : IDisposable
{
    private readonly BaseDatosPrueba _bd;
    private readonly MunicipiosServicio _servicio;

    public MunicipiosServicioTests()
    {
        _bd = new BaseDatosPrueba();
        _servicio = new MunicipiosServicio(_bd.Contexto);
    }

    public void Dispose()
    {
        _bd.Dispose();
        GC.SuppressFinalize(this);
    }

    private static GuardarMunicipioDto DtoBase(int deptoId) => new()
    {
        DepartamentoId = deptoId,
        Nombre = "Muni Prueba"
    };

    private async Task<Pais> CrearPaisPruebaAsync(string nombre, string codigo)
    {
        var pais = new Pais
        {
            Nombre = nombre,
            CodigoIso2 = codigo,
            EdadMinima = 18,
            EdadMaxima = 100,
            Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
        };
        _bd.Contexto.Set<Pais>().Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return pais;
    }

    private async Task<Departamento> CrearDepartamentoPruebaAsync(Pais pais, string nombre)
    {
        var depto = new Departamento
        {
            PaisId = pais.Id,
            Nombre = nombre
        };
        _bd.Contexto.Set<Departamento>().Add(depto);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);
        return depto;
    }

    [Fact]
    public async Task ListarAsync_SinBusqueda_OrdenadoPorPaisDepartamentoYMunicipio()
    {
        // Arrange
        // Un país con dos departamentos cuyo orden por nombre es inverso al de sus municipios
        var p = await CrearPaisPruebaAsync("Pais A", "PA");
        var d2 = await CrearDepartamentoPruebaAsync(p, "Depto B");
        var d1 = await CrearDepartamentoPruebaAsync(p, "Depto A");

        await _servicio.CrearAsync(DtoBase(d2.Id) with { Nombre = "Muni 1" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(d1.Id) with { Nombre = "Muni 2" }, TestContext.Current.CancellationToken);

        var p2 = await CrearPaisPruebaAsync("Pais B", "PB");
        var d3 = await CrearDepartamentoPruebaAsync(p2, "Depto 0");
        await _servicio.CrearAsync(DtoBase(d3.Id) with { Nombre = "Muni 3" }, TestContext.Current.CancellationToken);

        var consulta = new Consulta { Pagina = 1, Tamanio = 100 };

        // Act
        var pagina = await _servicio.ListarAsync(consulta, TestContext.Current.CancellationToken);

        // Assert
        var creados = pagina.Elementos.Where(m => m.DepartamentoId == d1.Id || m.DepartamentoId == d2.Id || m.DepartamentoId == d3.Id).ToList();

        Assert.Equal(3, creados.Count);
        Assert.Equal("Pais A", creados[0].PaisNombre);
        Assert.Equal("Depto A", creados[0].DepartamentoNombre);
        Assert.Equal("Muni 2", creados[0].Nombre);

        Assert.Equal("Pais A", creados[1].PaisNombre);
        Assert.Equal("Depto B", creados[1].DepartamentoNombre);
        Assert.Equal("Muni 1", creados[1].Nombre);

        Assert.Equal("Pais B", creados[2].PaisNombre);
        Assert.Equal("Depto 0", creados[2].DepartamentoNombre);
        Assert.Equal("Muni 3", creados[2].Nombre);
    }

    [Fact]
    public async Task ListarAsync_Buscar_NoDistingueMayusculasPeroSiTildes()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Busqueda", "BU");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Busqueda");
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Peten" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Petén" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Otro" }, TestContext.Current.CancellationToken);

        // Act
        var pagina1 = await _servicio.ListarAsync(new Consulta { Buscar = "PETEN", Pagina = 1, Tamanio = 10 }, TestContext.Current.CancellationToken);
        var pagina2 = await _servicio.ListarAsync(new Consulta { Buscar = "Petén", Pagina = 1, Tamanio = 10 }, TestContext.Current.CancellationToken);

        // Assert
        var res1 = pagina1.Elementos.Where(m => m.DepartamentoId == d.Id).ToList();
        Assert.Single(res1);
        Assert.Equal("Peten", res1[0].Nombre);

        var res2 = pagina2.Elementos.Where(m => m.DepartamentoId == d.Id).ToList();
        Assert.Single(res2);
        Assert.Equal("Petén", res2[0].Nombre);
    }

    [Fact]
    public async Task ListarPorDepartamentoAsync_DepartamentoInexistente_LanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ListarPorDepartamentoAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListarPorDepartamentoAsync_SinMunicipios_DevuelveListaVacia()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Vacio", "PV");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Vacio");

        // Act
        var lista = await _servicio.ListarPorDepartamentoAsync(d.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(lista);
    }

    [Fact]
    public async Task ListarPorDepartamentoAsync_ConMunicipios_DevuelveOrdenado()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Orden", "PO");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Orden");
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Zeta" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Alfa" }, TestContext.Current.CancellationToken);

        // Act
        var lista = await _servicio.ListarPorDepartamentoAsync(d.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, lista.Count);
        Assert.Equal("Alfa", lista[0].Nombre);
        Assert.Equal("Zeta", lista[1].Nombre);
    }

    [Fact]
    public async Task ObtenerAsync_Existente_DevuelveDatos()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Obtener", "PT");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Obtener");
        var creado = await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Obtener" }, TestContext.Current.CancellationToken);

        // Act
        var obtenido = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(creado.Id, obtenido.Id);
        Assert.Equal("Muni Obtener", obtenido.Nombre);
        Assert.Equal(d.Id, obtenido.DepartamentoId);
        Assert.Equal("Depto Obtener", obtenido.DepartamentoNombre);
        Assert.Equal(d.PaisId, obtenido.PaisId);
        Assert.Equal("Pais Obtener", obtenido.PaisNombre);
    }

    [Fact]
    public async Task ObtenerAsync_Inexistente_LanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_DepartamentoInexistente_LanzaValidacionExceptionEnDepartamentoId()
    {
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(DtoBase(9999), TestContext.Current.CancellationToken));
        Assert.Equal("DepartamentoId", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_NombreDuplicadoDiferentesMayusculasOEspacios_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais 409", "P9");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto 409 A");
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Unico" }, TestContext.Current.CancellationToken);

        var dtoDuplicado = DtoBase(d.Id) with { Nombre = "  MUNI unico  " };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dtoDuplicado, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_MismoNombreDiferenteTilde_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Tilde", "PT");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Tilde");
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni" }, TestContext.Current.CancellationToken);

        var dto = DtoBase(d.Id) with { Nombre = "Muní" };

        // Act
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Muní", creado.Nombre);
    }

    [Fact]
    public async Task CrearAsync_MismoNombreEnOtroDepartamento_Permitido()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais 200", "P2");
        var d1 = await CrearDepartamentoPruebaAsync(p, "Depto 200 A");
        var d2 = await CrearDepartamentoPruebaAsync(p, "Depto 200 B");
        await _servicio.CrearAsync(DtoBase(d1.Id) with { Nombre = "Muni Repetido" }, TestContext.Current.CancellationToken);

        var dto = DtoBase(d2.Id) with { Nombre = "Muni Repetido" };

        // Act
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Muni Repetido", creado.Nombre);
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_GuardaSinEspacios()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais OK", "PK");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto OK");
        var dto = DtoBase(d.Id) with { Nombre = "  Muni OK  " };

        // Act
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);
        var guardado = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Muni OK", creado.Nombre);
        Assert.Equal("Muni OK", guardado.Nombre);
    }

    [Fact]
    public async Task ActualizarAsync_Inexistente_LanzaNoEncontradoException()
    {
        var p = await CrearPaisPruebaAsync("Pais 404", "P4");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto 404");
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(9999, DtoBase(d.Id), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_InexistenteYDepartamentoInexistente_LanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(9999, DtoBase(9999), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_CambioDeDepartamento_LanzaValidacionExceptionEnDepartamentoId()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Mov", "PM");
        var d1 = await CrearDepartamentoPruebaAsync(p, "Depto Origen");
        var d2 = await CrearDepartamentoPruebaAsync(p, "Depto Destino");
        var creado = await _servicio.CrearAsync(DtoBase(d1.Id) with { Nombre = "Moviendo" }, TestContext.Current.CancellationToken);

        var dtoActualizar = DtoBase(d2.Id) with { Nombre = "Moviendo" };

        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, dtoActualizar, TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal("DepartamentoId", ex.Campo);

        var guardado = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);
        Assert.Equal(d1.Id, guardado.DepartamentoId);
        Assert.Equal("Moviendo", guardado.Nombre);
    }

    [Fact]
    public async Task ActualizarAsync_DuplicadoEnDepartamentoDestino_LanzaValidacionExceptionEnDepartamentoIdAntesQueConflicto()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Act 400", "A0");
        var d1 = await CrearDepartamentoPruebaAsync(p, "Depto 400 B1");
        var d2 = await CrearDepartamentoPruebaAsync(p, "Depto 400 B2");
        await _servicio.CrearAsync(DtoBase(d2.Id) with { Nombre = "Muni Existente" }, TestContext.Current.CancellationToken);
        var creado = await _servicio.CrearAsync(DtoBase(d1.Id) with { Nombre = "Muni Nuevo" }, TestContext.Current.CancellationToken);

        var dtoMoverDuplicado = DtoBase(d2.Id) with { Nombre = "Muni Existente" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, dtoMoverDuplicado, TestContext.Current.CancellationToken));
        Assert.Equal("DepartamentoId", ex.Campo);
    }

    [Fact]
    public async Task ActualizarAsync_NombreDuplicadoEnMismoDepartamento_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais M", "PM");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Act Mismo");
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Existente" }, TestContext.Current.CancellationToken);
        var creado = await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Nuevo" }, TestContext.Current.CancellationToken);

        var dtoActualizar = DtoBase(d.Id) with { Nombre = "Muni Existente" };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(creado.Id, dtoActualizar, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_MismoMunicipio_NoLanzaConflicto()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais P", "PP");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Act Propio");
        var creado = await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Propio" }, TestContext.Current.CancellationToken);

        var dtoActualizar = DtoBase(d.Id) with { Nombre = "Muni Propio" };

        // Act
        var actualizado = await _servicio.ActualizarAsync(creado.Id, dtoActualizar, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Muni Propio", actualizado.Nombre);
    }

    [Fact]
    public async Task EliminarAsync_Inexistente_LanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.EliminarAsync(9999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_SinEmpresas_SeElimina()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais D", "PD");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Del");
        var creado = await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Del" }, TestContext.Current.CancellationToken);

        // Act
        await _servicio.EliminarAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_ConEmpresa_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais E", "PE");
        var d = await CrearDepartamentoPruebaAsync(p, "Depto Emp");
        var creado = await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Emp" }, TestContext.Current.CancellationToken);

        var empresa = new Empresa
        {
            MunicipioId = creado.Id,
            Nit = "123",
            RazonSocial = "RS",
            NombreComercial = "NC",
            Telefono = "123",
            Correo = "a@a.com"
        };
        _bd.Contexto.Set<Empresa>().Add(empresa);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.EliminarAsync(creado.Id, TestContext.Current.CancellationToken));
    }
}
