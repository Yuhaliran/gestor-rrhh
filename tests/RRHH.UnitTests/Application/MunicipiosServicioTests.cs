using RRHH.Application.Excepciones;
using RRHH.Application.Servicios;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Municipios;
using RRHH.Domain.Entidades;
using RRHH.UnitTests.Datos;
using RRHH.Domain.Reglas;

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

    private async Task<Departamento> CrearDeptoPruebaAsync(string nombrePais, string nombreDepto)
    {
        var pais = new Pais
        {
            Nombre = nombrePais,
            CodigoIso2 = nombrePais.Substring(0, 2).ToUpper(),
            EdadMinima = 18,
            EdadMaxima = 100,
            Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
        };
        _bd.Contexto.Set<Pais>().Add(pais);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);

        var depto = new Departamento
        {
            PaisId = pais.Id,
            Nombre = nombreDepto
        };
        _bd.Contexto.Set<Departamento>().Add(depto);
        await _bd.Contexto.SaveChangesAsync(TestContext.Current.CancellationToken);

        return depto;
    }

    [Fact]
    public async Task ListarAsync_SinBusqueda_OrdenadoPorPaisDepartamentoYMunicipio()
    {
        // Arrange
        // Para que ordenar sólo por depto o muni dé otro resultado, cruzamos los nombres:
        var d1 = await CrearDeptoPruebaAsync("Pais B", "Depto Y");
        var d2 = await CrearDeptoPruebaAsync("Pais A", "Depto Z");

        await _servicio.CrearAsync(DtoBase(d1.Id) with { Nombre = "Muni 2" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(d1.Id) with { Nombre = "Muni 1" }, TestContext.Current.CancellationToken);

        await _servicio.CrearAsync(DtoBase(d2.Id) with { Nombre = "Muni 4" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(d2.Id) with { Nombre = "Muni 3" }, TestContext.Current.CancellationToken);

        var consulta = new Consulta { Pagina = 1, Tamanio = 100 };

        // Act
        var pagina = await _servicio.ListarAsync(consulta, TestContext.Current.CancellationToken);

        // Assert
        var creados = pagina.Elementos.Where(m => m.DepartamentoId == d1.Id || m.DepartamentoId == d2.Id).ToList();

        Assert.Equal(4, creados.Count);
        Assert.Equal("Pais A", creados[0].PaisNombre);
        Assert.Equal("Depto Z", creados[0].DepartamentoNombre);
        Assert.Equal("Muni 3", creados[0].Nombre);

        Assert.Equal("Pais A", creados[1].PaisNombre);
        Assert.Equal("Muni 4", creados[1].Nombre);

        Assert.Equal("Pais B", creados[2].PaisNombre);
        Assert.Equal("Depto Y", creados[2].DepartamentoNombre);
        Assert.Equal("Muni 1", creados[2].Nombre);

        Assert.Equal("Pais B", creados[3].PaisNombre);
        Assert.Equal("Muni 2", creados[3].Nombre);
    }

    [Fact]
    public async Task ListarAsync_Buscar_NoDistingueMayusculasPeroSiTildes()
    {
        // Arrange
        var d = await CrearDeptoPruebaAsync("Pais Busqueda", "Depto Busqueda");
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
        var d = await CrearDeptoPruebaAsync("Pais Vacio", "Depto Vacio");

        // Act
        var lista = await _servicio.ListarPorDepartamentoAsync(d.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(lista);
    }

    [Fact]
    public async Task ListarPorDepartamentoAsync_ConMunicipios_DevuelveOrdenado()
    {
        // Arrange
        var d = await CrearDeptoPruebaAsync("Pais Orden", "Depto Orden");
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
        var d = await CrearDeptoPruebaAsync("Pais Obtener", "Depto Obtener");
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
        var d = await CrearDeptoPruebaAsync("Pais 409", "Depto 409 A");
        await _servicio.CrearAsync(DtoBase(d.Id) with { Nombre = "Muni Unico" }, TestContext.Current.CancellationToken);

        var dtoDuplicado = DtoBase(d.Id) with { Nombre = "  MUNI unico  " };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dtoDuplicado, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_MismoNombreDiferenteTilde_Permitido()
    {
        // Arrange
        var d = await CrearDeptoPruebaAsync("Pais Tilde", "Depto Tilde");
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
        var d1 = await CrearDeptoPruebaAsync("Pais 200", "Depto 200 A");
        var d2 = await CrearDeptoPruebaAsync("Pais 200", "Depto 200 B");
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
        var d = await CrearDeptoPruebaAsync("Pais OK", "Depto OK");
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
        var d = await CrearDeptoPruebaAsync("Pais 404", "Depto 404");
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
        var d1 = await CrearDeptoPruebaAsync("Pais Origen", "Depto Origen");
        var d2 = await CrearDeptoPruebaAsync("Pais Destino", "Depto Destino");
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
        var d1 = await CrearDeptoPruebaAsync("Pais Act 400", "Depto 400 B1");
        var d2 = await CrearDeptoPruebaAsync("Pais Act 400", "Depto 400 B2");
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
        var d = await CrearDeptoPruebaAsync("Pais Act Mismo", "Depto Act Mismo");
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
        var d = await CrearDeptoPruebaAsync("Pais Act Propio", "Depto Act Propio");
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
        var d = await CrearDeptoPruebaAsync("Pais Del", "Depto Del");
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
        var d = await CrearDeptoPruebaAsync("Pais Emp", "Depto Emp");
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
