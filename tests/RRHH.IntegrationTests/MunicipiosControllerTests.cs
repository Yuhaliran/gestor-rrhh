using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;
using RRHH.Contratos.Municipios;

namespace RRHH.IntegrationTests;

public class MunicipiosControllerTests : PruebaIntegracionBase
{
    public MunicipiosControllerTests(FabricaApi fabrica) : base(fabrica) { }

    [Fact]
    public async Task Listar_SinBusqueda_DevuelvePagina()
    {
        // Arrange
        var requestUri = "/api/municipios";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<MunicipioDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, m => m.Id == 1 && m.Nombre == "Guatemala");
    }

    [Fact]
    public async Task Listar_Busqueda_DevuelveCoincidencia()
    {
        // Arrange
        var m1 = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Mun Buscar 1" };
        var m2 = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Mun Filtro 2" };
        var res1 = await Cliente.PostAsJsonAsync("/api/municipios", m1, OpcionesJson, TestContext.Current.CancellationToken);
        res1.EnsureSuccessStatusCode();
        var res2 = await Cliente.PostAsJsonAsync("/api/municipios", m2, OpcionesJson, TestContext.Current.CancellationToken);
        res2.EnsureSuccessStatusCode();

        var requestUri = "/api/municipios?buscar=Buscar";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<MunicipioDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, m => m.Nombre == "Mun Buscar 1");
        Assert.DoesNotContain(pagina.Elementos, m => m.Nombre == "Mun Filtro 2");
    }

    [Fact]
    public async Task Listar_PaginacionInvalida_Devuelve400()
    {
        // Arrange
        var requestUri = "/api/municipios?tamanio=0";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_Valido_Devuelve201ConUbicacion()
    {
        // Arrange
        var dto = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Nuevo Municipio" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/municipios", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<MunicipioDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(creado);
        Assert.Equal(dto.Nombre, creado.Nombre);
        Assert.NotNull(respuesta.Headers.Location);
        Assert.EndsWith($"/api/municipios/{creado.Id}", respuesta.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostMunicipio_DatosInvalidos_Devuelve400ConDetalle()
    {
        // Arrange
        var dto = new GuardarMunicipioDto { DepartamentoId = 0, Nombre = "" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/municipios", dto, OpcionesJson, TestContext.Current.CancellationToken);

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
        var requestUri = "/api/municipios/9999";

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
        var dto = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Mun Actualizar" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/municipios", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<MunicipioDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoActualizar = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Mun Actualizado" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/municipios/{creado!.Id}", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<MunicipioDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(actualizado);
        Assert.Equal(dtoActualizar.Nombre, actualizado.Nombre);
    }

    [Fact]
    public async Task Actualizar_Inexistente_Devuelve404()
    {
        // Arrange
        var dtoActualizar = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Inexistente" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync("/api/municipios/9999", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Existente_Devuelve204()
    {
        // Arrange
        var dto = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Mun Eliminar" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/municipios", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<MunicipioDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/municipios/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var getRespuesta = await Cliente.GetAsync($"/api/municipios/{creado.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getRespuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/municipios/9999";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_MunicipioConEmpresas_Conflicto()
    {
        // Arrange
        var dto = new GuardarMunicipioDto { DepartamentoId = 1, Nombre = "Mun Con Empresa" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/municipios", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<MunicipioDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var empresaDto = new GuardarEmpresaDto
        {
            MunicipioId = creado!.Id,
            Nit = "12345",
            RazonSocial = "RS",
            NombreComercial = "NC",
            Telefono = "12345678",
            Correo = "test@test.com"
        };
        var resEmpresa = await Cliente.PostAsJsonAsync("/api/empresas", empresaDto, OpcionesJson, TestContext.Current.CancellationToken);
        resEmpresa.EnsureSuccessStatusCode();

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/municipios/{creado.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await ExtraerProblemDetailsAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Equal(409, problema.Status);
    }
}
