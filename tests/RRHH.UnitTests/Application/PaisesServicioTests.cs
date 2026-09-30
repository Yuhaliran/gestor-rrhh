using RRHH.Application.Excepciones;
using RRHH.Application.Servicios;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Paises;
using RRHH.UnitTests.Datos;

namespace RRHH.UnitTests.Application;

public class PaisesServicioTests : IDisposable
{
    private readonly BaseDatosPrueba _bd = new();
    private readonly PaisesServicio _servicio;

    public PaisesServicioTests()
    {
        _servicio = new PaisesServicio(_bd.Contexto);
    }

    public void Dispose() => _bd.Dispose();

    private GuardarPaisDto DtoBase() => new GuardarPaisDto
    {
        Nombre = "País Prueba",
        CodigoIso2 = "XX",
        EdadMinima = 18,
        EdadMaxima = 100,
        Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero
    };

    [Fact]
    public async Task ListarAsync_SinBusqueda_OrdenadoPorNombreYPaginado()
    {
        // Act
        var c = new Consulta { Pagina = 1, Tamanio = 10 };
        var pagina = await _servicio.ListarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(pagina.Total >= 1);
        Assert.NotEmpty(pagina.Elementos);
        var nombres = pagina.Elementos.Select(p => p.Nombre).ToList();
        var ordenados = nombres.OrderBy(n => n).ToList();
        Assert.Equal(ordenados, nombres);
    }

    [Theory]
    [InlineData("guate")]
    [InlineData("gt")]
    [InlineData("GUATE")]
    [InlineData("GT")]
    public async Task ListarAsync_Buscar_NoDistingueMayusculas_EncuentraGuatemala(string buscar)
    {
        // Act
        var c = new Consulta { Buscar = buscar };
        var pagina = await _servicio.ListarAsync(c, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(pagina.Elementos, p => p.Nombre == "Guatemala");
    }

    [Fact]
    public async Task ObtenerAsync_Existente_DevuelveTodosLosDatos()
    {
        // Act
        var pais = await _servicio.ObtenerAsync(1, TestContext.Current.CancellationToken); // Guatemala está en datos iniciales

        // Assert
        Assert.NotNull(pais);
        Assert.Equal(1, pais.Id);
        Assert.Equal("Guatemala", pais.Nombre);
        Assert.Equal("GT", pais.CodigoIso2);
    }

    [Fact]
    public async Task ObtenerAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_DevuelvePaisConNombreSinEspaciosYCodigoMayusculas()
    {
        // Arrange
        var dto = DtoBase() with { Nombre = " El Salvador ", CodigoIso2 = "sv" };

        // Act
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(creado.Id > 1);
        Assert.Equal("El Salvador", creado.Nombre);
        Assert.Equal("SV", creado.CodigoIso2);
    }

    [Theory]
    [InlineData("GUATEMALA")]
    [InlineData(" guatemala ")]
    public async Task CrearAsync_NombreDuplicadoDiferentesMayusculasOEspacios_LanzaConflictoException(string nombre)
    {
        // Arrange
        var dto = DtoBase() with { Nombre = nombre };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_CodigoIsoDuplicadoEnMinusculas_LanzaConflictoException()
    {
        // Arrange
        var dto = DtoBase() with { CodigoIso2 = "gt" };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.CrearAsync(dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CrearAsync_NombreDifiereEnTilde_Permitido()
    {
        // Arrange (asumimos que "Guatemala" existe)
        var dto = DtoBase() with { Nombre = "Guatemálá" };

        // Act
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(creado.Id > 1);
        Assert.Equal("Guatemálá", creado.Nombre);
    }

    [Fact]
    public async Task ActualizarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Arrange
        var dto = DtoBase();

        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ActualizarAsync(999, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_DuplicarNombreDeOtroPais_LanzaConflictoException()
    {
        // Arrange
        var dto1 = DtoBase() with { Nombre = "Honduras", CodigoIso2 = "HN" };
        var creado = await _servicio.CrearAsync(dto1, TestContext.Current.CancellationToken);

        var dto2 = DtoBase() with { Nombre = "Guatemala" }; // Ya existe

        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.ActualizarAsync(creado.Id, dto2, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ActualizarAsync_MismoPaisConSuPropioNombreYOtroRangoDeEdad_Funciona()
    {
        // Arrange
        var dto = DtoBase() with { Nombre = "Belice", CodigoIso2 = "BZ" };
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);
        var dtoActualizar = dto with { EdadMinima = 21, EdadMaxima = 65 };

        // Act
        var actualizado = await _servicio.ActualizarAsync(creado.Id, dtoActualizar, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(creado.Id, actualizado.Id);
        Assert.Equal(21, actualizado.EdadMinima);
        Assert.Equal(65, actualizado.EdadMaxima);
    }

    [Fact]
    public async Task EliminarAsync_Inexistente_LanzaNoEncontradoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.EliminarAsync(999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EliminarAsync_GuatemalaQueTieneDepartamentos_LanzaConflictoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ConflictoException>(() => _servicio.EliminarAsync(1, TestContext.Current.CancellationToken)); // Guatemala
    }

    [Fact]
    public async Task EliminarAsync_PaisSinDepartamentos_LoElimina()
    {
        // Arrange
        var dto = DtoBase() with { Nombre = "Nicaragua", CodigoIso2 = "NI" };
        var creado = await _servicio.CrearAsync(dto, TestContext.Current.CancellationToken);

        // Act
        await _servicio.EliminarAsync(creado.Id, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<NoEncontradoException>(() => _servicio.ObtenerAsync(creado.Id, TestContext.Current.CancellationToken));
    }
}
