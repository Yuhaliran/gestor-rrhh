using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Paises;

namespace RRHH.IntegrationTests;

public class PaisesControllerTests : PruebaIntegracionBase
{
    public PaisesControllerTests(FabricaApi fabrica) : base(fabrica) { }

    [Fact]
    public async Task Listar_SinBusqueda_DevuelvePagina()
    {
        // Arrange
        var requestUri = "/api/paises";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<PaisDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, p => p.Id == 1 && p.Nombre == "Guatemala");
    }

    [Fact]
    public async Task Listar_Busqueda_DevuelveCoincidencia()
    {
        // Arrange
        var requestUri = "/api/paises?buscar=guate";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<PaisDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, p => p.Nombre == "Guatemala");
    }

    [Fact]
    public async Task Listar_PaginacionInvalida_Devuelve400()
    {
        // Arrange
        var requestUri = "/api/paises?tamanio=0";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("Tamanio"));
    }

    [Fact]
    public async Task Crear_Valido_Devuelve201ConUbicacion()
    {
        // Arrange
        var dto = new GuardarPaisDto { Nombre = "El Salvador", CodigoIso2 = "SV", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/paises", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<PaisDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(creado);
        Assert.Equal(dto.Nombre, creado.Nombre);
        Assert.NotNull(respuesta.Headers.Location);
    }

    [Fact]
    public async Task PostPais_DatosInvalidos_Devuelve400ConDetalle()
    {
        // Arrange
        var dto = new GuardarPaisDto { Nombre = "", CodigoIso2 = "S", EdadMinima = -1, EdadMaxima = 0 };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/paises", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("Nombre"));
    }

    [Fact]
    public async Task Crear_Duplicado_Conflicto()
    {
        // Arrange
        var dto = new GuardarPaisDto { Nombre = "Guatemala", CodigoIso2 = "GT", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/paises", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await ExtraerProblemDetailsAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Equal(409, problema.Status);
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/paises/9999";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var problema = await ExtraerProblemDetailsAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Equal(404, problema.Status);
    }

    [Fact]
    public async Task Actualizar_Valido_Devuelve200()
    {
        // Arrange
        var dto = new GuardarPaisDto { Nombre = "Honduras", CodigoIso2 = "HN", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/paises", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<PaisDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoActualizar = new GuardarPaisDto { Nombre = "Honduras Editado", CodigoIso2 = "HN", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.PrimeroDeMarzo };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/paises/{creado!.Id}", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<PaisDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(actualizado);
        Assert.Equal(dtoActualizar.Nombre, actualizado.Nombre);
    }

    [Fact]
    public async Task Actualizar_Inexistente_Devuelve404()
    {
        // Arrange
        var dtoActualizar = new GuardarPaisDto { Nombre = "Inexistente", CodigoIso2 = "XX", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.PrimeroDeMarzo };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync("/api/paises/9999", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Existente_Devuelve204()
    {
        // Arrange
        var dto = new GuardarPaisDto { Nombre = "Belice", CodigoIso2 = "BZ", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/paises", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<PaisDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/paises/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var getRespuesta = await Cliente.GetAsync($"/api/paises/{creado.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getRespuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/paises/9999";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_PaisConDepartamentos_Conflicto()
    {
        // Arrange
        var requestUri = "/api/paises/1";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await ExtraerProblemDetailsAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Equal(409, problema.Status);
    }

    [Fact]
    public async Task ObtenerDepartamentos_Existente_DevuelveLista()
    {
        // Arrange
        var requestUri = "/api/paises/1/departamentos";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var lista = await respuesta.Content.ReadFromJsonAsync<List<DepartamentoDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(lista);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task ObtenerDepartamentos_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/paises/9999/departamentos";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
