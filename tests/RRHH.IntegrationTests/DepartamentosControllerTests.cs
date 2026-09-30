using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Municipios;
using RRHH.Contratos.Paises;

namespace RRHH.IntegrationTests;

public class DepartamentosControllerTests : PruebaIntegracionBase
{
    public DepartamentosControllerTests(FabricaApi fabrica) : base(fabrica) { }

    [Fact]
    public async Task Listar_SinBusqueda_DevuelvePagina()
    {
        // Arrange
        var requestUri = "/api/departamentos";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<DepartamentoDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, d => d.Id == 1 && d.Nombre == "Guatemala");
    }

    [Fact]
    public async Task Listar_Busqueda_DevuelveCoincidencia()
    {
        // Arrange
        var d1 = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Dep Buscar 1" };
        var d2 = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Dep Filtro 2" };
        var res1 = await Cliente.PostAsJsonAsync("/api/departamentos", d1, OpcionesJson, TestContext.Current.CancellationToken);
        res1.EnsureSuccessStatusCode();
        var res2 = await Cliente.PostAsJsonAsync("/api/departamentos", d2, OpcionesJson, TestContext.Current.CancellationToken);
        res2.EnsureSuccessStatusCode();

        var requestUri = "/api/departamentos?buscar=Buscar";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<DepartamentoDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, d => d.Nombre == "Dep Buscar 1");
        Assert.DoesNotContain(pagina.Elementos, d => d.Nombre == "Dep Filtro 2");
    }

    [Fact]
    public async Task Listar_PaginacionInvalida_Devuelve400()
    {
        // Arrange
        var requestUri = "/api/departamentos?tamanio=0";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_Valido_Devuelve201ConUbicacion()
    {
        // Arrange
        var dto = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Nuevo Departamento" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/departamentos", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<DepartamentoDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(creado);
        Assert.Equal(dto.Nombre, creado.Nombre);
        Assert.NotNull(respuesta.Headers.Location);
        Assert.EndsWith($"/api/departamentos/{creado.Id}", respuesta.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostDepartamento_DatosInvalidos_Devuelve400ConDetalle()
    {
        // Arrange
        var dto = new GuardarDepartamentoDto { PaisId = 0, Nombre = "" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/departamentos", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("Nombre"));
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/departamentos/9999";

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
        var dto = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Dep Actualizar" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/departamentos", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<DepartamentoDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoActualizar = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Dep Actualizado" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/departamentos/{creado!.Id}", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<DepartamentoDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(actualizado);
        Assert.Equal(dtoActualizar.Nombre, actualizado.Nombre);
    }

    [Fact]
    public async Task Actualizar_Inexistente_Devuelve404()
    {
        // Arrange
        var dtoActualizar = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Inexistente" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync("/api/departamentos/9999", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_CambiarPais_ErrorRN8()
    {
        // Arrange
        var dto = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Dep P1" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/departamentos", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<DepartamentoDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoPais = new GuardarPaisDto { Nombre = "El Salvador RN8", CodigoIso2 = "SV", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero };
        var postPais = await Cliente.PostAsJsonAsync("/api/paises", dtoPais, OpcionesJson, TestContext.Current.CancellationToken);
        postPais.EnsureSuccessStatusCode();
        var pais2 = await postPais.Content.ReadFromJsonAsync<PaisDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoActualizar = new GuardarDepartamentoDto { PaisId = pais2!.Id, Nombre = "Dep P1 Editado" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/departamentos/{creado!.Id}", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.Contains(problema!.Errors, e => e.Key == "PaisId");
    }

    [Fact]
    public async Task Eliminar_Existente_Devuelve204()
    {
        // Arrange
        var dto = new GuardarDepartamentoDto { PaisId = 1, Nombre = "Dep Eliminar" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/departamentos", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<DepartamentoDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/departamentos/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var getRespuesta = await Cliente.GetAsync($"/api/departamentos/{creado.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getRespuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/departamentos/9999";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_DepartamentoConMunicipios_Conflicto()
    {
        // Arrange
        var requestUri = "/api/departamentos/1";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await ExtraerProblemDetailsAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Equal(409, problema.Status);
    }

    [Fact]
    public async Task GetMunicipiosDeDepartamento_Existente_DevuelveLista()
    {
        // Arrange
        var requestUri = "/api/departamentos/1/municipios";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var lista = await respuesta.Content.ReadFromJsonAsync<List<MunicipioDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(lista);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task GetMunicipiosDeDepartamento_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/departamentos/9999/municipios";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
