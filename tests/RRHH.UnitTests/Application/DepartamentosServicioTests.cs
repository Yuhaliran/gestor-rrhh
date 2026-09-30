using RRHH.Application.Excepciones;
using RRHH.Application.Servicios;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Paises;
using RRHH.UnitTests.Datos;

namespace RRHH.UnitTests.Application;

public class DepartamentosServicioTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();
    private readonly DepartamentosServicio _servicio;
    private readonly PaisesServicio _paisesServicio;

    public DepartamentosServicioTests()
    {
        _servicio = new DepartamentosServicio(_bd.Contexto);
        _paisesServicio = new PaisesServicio(_bd.Contexto);
    }

    public void Dispose() => _bd.Dispose();

    private async Task<PaisDto> CrearPaisPruebaAsync(string nombre, string codigo)
    {
        var dto = new GuardarPaisDto
        {
            Nombre = nombre,
            CodigoIso2 = codigo,
            EdadMinima = 18,
            EdadMaxima = 100,
            Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
        };
        return await _paisesServicio.CrearAsync(dto, TestContext.Current.CancellationToken);
    }

    private GuardarDepartamentoDto DtoBase(int paisId) => new GuardarDepartamentoDto
    {
        PaisId = paisId,
        Nombre = "Departamento Prueba"
    };

    [Fact]
    public async Task ListarAsync_SinBusqueda_OrdenadoPorPaisYNombre()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais Z", "PZ");
        var p2 = await CrearPaisPruebaAsync("Pais A", "PA");

        await _servicio.CrearAsync(DtoBase(p1.Id) with { Nombre = "Depto B" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(p1.Id) with { Nombre = "Depto A" }, TestContext.Current.CancellationToken);

        await _servicio.CrearAsync(DtoBase(p2.Id) with { Nombre = "Depto Z" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(p2.Id) with { Nombre = "Depto Y" }, TestContext.Current.CancellationToken);

        var c = new Consulta { Pagina = 1, Tamanio = 100 };

        // Act
        var pagina = await _servicio.ListarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        // Filtramos solo los de estos paises para evitar problemas con los iniciales (Guatemala)
        var creados = pagina.Elementos.Where(d => d.PaisId == p1.Id || d.PaisId == p2.Id).ToList();

        Assert.Equal(4, creados.Count);
        Assert.Equal("Pais A", creados[0].PaisNombre);
        Assert.Equal("Depto Y", creados[0].Nombre);

        Assert.Equal("Pais A", creados[1].PaisNombre);
        Assert.Equal("Depto Z", creados[1].Nombre);

        Assert.Equal("Pais Z", creados[2].PaisNombre);
        Assert.Equal("Depto A", creados[2].Nombre);

        Assert.Equal("Pais Z", creados[3].PaisNombre);
        Assert.Equal("Depto B", creados[3].Nombre);
    }

    [Theory]
    [InlineData("peten")]
    [InlineData("PETEN")]
    [InlineData("Petén")]
    public async Task ListarAsync_Buscar_NoDistingueMayusculasNiTildes(string buscar)
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Tildes", "TI");
        await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Petén" }, TestContext.Current.CancellationToken);

        var c = new Consulta { Buscar = buscar };

        // Act
        var pagina = await _servicio.ListarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(pagina.Elementos, d => d.Nombre == "Petén");
    }

    [Fact]
    public async Task ListarPorPaisAsync_PaisInexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ListarPorPaisAsync(999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListarPorPaisAsync_SinDepartamentos_DevuelveListaVacia()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Vacio", "PV");

        // Act
        var lista = await _servicio.ListarPorPaisAsync(p.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(lista);
    }

    [Fact]
    public async Task ListarPorPaisAsync_ConDepartamentos_DevuelveOrdenado()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Orden", "PO");
        await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Zeta" }, TestContext.Current.CancellationToken);
        await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Alfa" }, TestContext.Current.CancellationToken);

        // Act
        var lista = await _servicio.ListarPorPaisAsync(p.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, lista.Count);
        Assert.Equal("Alfa", lista[0].Nombre);
        Assert.Equal("Zeta", lista[1].Nombre);
    }

    [Fact]
    public async Task ObtenerAsync_Existente_DevuelveDatos()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Obtener", "OB");
        var creado = await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Depto 1" }, TestContext.Current.CancellationToken);

        // Act
        var depto = await _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(depto);
        Assert.Equal(creado.Id, depto.Id);
        Assert.Equal("Depto 1", depto.Nombre);
        Assert.Equal(p.Id, depto.PaisId);
        Assert.Equal(p.Nombre, depto.PaisNombre);
    }

    [Fact]
    public async Task ObtenerAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_PaisInexistente_LanzaValidacionExceptionEnPaisId()
    {
        // Arrange
        var dto = DtoBase(999);

        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal("PaisId", ex.Campo);
    }

    [Fact]
    public async Task CrearAsync_NombreDuplicadoEnMismoPais_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Conflicto", "CF");
        var dto = DtoBase(p.Id) with { Nombre = "Unico" };
        await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_GuardaSinEspacios()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Espacios", "PE");
        var dto = DtoBase(p.Id) with { Nombre = "  Con Espacios  " };

        // Act
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Con Espacios", creado.Nombre);
    }

    [Fact]
    public async Task ActualizarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Act 404", "A4");
        var dto = DtoBase(p.Id);

        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(999, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_PaisInexistente_LanzaValidacionExceptionEnPaisId()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Act 400", "A0");
        var creado = await _servicio.CrearAsync(DtoBase(p.Id), TestContext.Current.CancellationToken);

        var dtoInvalido = DtoBase(999);

        // Act
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.ActualizarAsync(creado.Id, dtoInvalido, TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal("PaisId", ex.Campo);
    }

    [Fact]
    public async Task ActualizarAsync_DuplicadoEnPaisDestino_LanzaConflictoException()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Act 409", "A9");
        await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Depto Existente" }, TestContext.Current.CancellationToken);
        var creado = await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Depto Nuevo" }, TestContext.Current.CancellationToken);

        var dtoDuplicado = DtoBase(p.Id) with { Nombre = "Depto Existente" };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(creado.Id, dtoDuplicado, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_CambioDePais_Exitoso()
    {
        // Arrange
        var p1 = await CrearPaisPruebaAsync("Pais Origen", "O1");
        var p2 = await CrearPaisPruebaAsync("Pais Destino", "D2");
        var creado = await _servicio.CrearAsync(DtoBase(p1.Id) with { Nombre = "Moviendo" }, TestContext.Current.CancellationToken);

        var dtoActualizar = DtoBase(p2.Id) with { Nombre = "Moviendo" };

        // Act
        var actualizado = await _servicio.ActualizarAsync(creado.Id, dtoActualizar, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(p2.Id, actualizado.PaisId);
        Assert.Equal("Moviendo", actualizado.Nombre);
    }

    [Fact]
    public async Task ActualizarAsync_MismoDepartamento_NoLanzaConflicto()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Mismo", "SM");
        var creado = await _servicio.CrearAsync(DtoBase(p.Id) with { Nombre = "Mismo Nombre" }, TestContext.Current.CancellationToken);

        var dto = DtoBase(p.Id) with { Nombre = "Mismo Nombre" };

        // Act
        var actualizado = await _servicio.ActualizarAsync(creado.Id, dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(creado.Id, actualizado.Id);
    }

    [Fact]
    public async Task EliminarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.EliminarAsync(999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_GuatemalaConMunicipios_LanzaConflictoException()
    {
        // Arrange: El departamento 1 (Guatemala) tiene municipios por defecto.

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.EliminarAsync(1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_SinMunicipios_LoElimina()
    {
        // Arrange
        var p = await CrearPaisPruebaAsync("Pais Borrar", "B1");
        var creado = await _servicio.CrearAsync(DtoBase(p.Id), TestContext.Current.CancellationToken);

        // Act
        await _servicio.EliminarAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken));
    }
}
