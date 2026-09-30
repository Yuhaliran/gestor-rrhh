using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Empresas;

namespace RRHH.IntegrationTests;

public class ColaboradoresControllerTests : PruebaIntegracionBase
{
    public ColaboradoresControllerTests(FabricaApi fabrica) : base(fabrica) { }

    private async Task<int> CrearEmpresaAsync(string nit, string correo)
    {
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = nit, RazonSocial = "RS", NombreComercial = "NC", Telefono = "11111111", Correo = correo };
        var res = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);
        res.EnsureSuccessStatusCode();
        var emp = await res.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);
        return emp!.Id;
    }

    [Fact]
    public async Task Listar_SinBusqueda_DevuelvePagina()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C1L", "c1l@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "C Lista", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "lista@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1), Puesto = "QA" } } };
        var postRes = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRes.EnsureSuccessStatusCode();

        var requestUri = "/api/colaboradores";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<ColaboradorDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, c => c.NombreCompleto == "C Lista");
    }

    [Fact]
    public async Task Listar_Busqueda_DevuelveCoincidencia()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C2L", "c2l@test.com");
        var c1 = new CrearColaboradorDto { NombreCompleto = "Buscar Lopez", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "b@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1), Puesto = "QA" } } };
        var c2 = new CrearColaboradorDto { NombreCompleto = "Filtro Perez", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "f@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1), Puesto = "QA" } } };
        var res1 = await Cliente.PostAsJsonAsync("/api/colaboradores", c1, OpcionesJson, TestContext.Current.CancellationToken);
        res1.EnsureSuccessStatusCode();
        var res2 = await Cliente.PostAsJsonAsync("/api/colaboradores", c2, OpcionesJson, TestContext.Current.CancellationToken);
        res2.EnsureSuccessStatusCode();

        var requestUri = "/api/colaboradores?buscar=Buscar";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<ColaboradorDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, c => c.NombreCompleto == "Buscar Lopez");
        Assert.DoesNotContain(pagina.Elementos, c => c.NombreCompleto == "Filtro Perez");
    }

    [Fact]
    public async Task Listar_PaginacionInvalida_Devuelve400()
    {
        // Arrange
        var requestUri = "/api/colaboradores?tamanio=0";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_Valido_Devuelve201ConUbicacion()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C1", "c1@test.com");
        var dto = new CrearColaboradorDto
        {
            NombreCompleto = "Ana Lopez",
            FechaNacimiento = new DateOnly(1995, 5, 5),
            Telefono = "12345678",
            Correo = "ana.lopez@test.com",
            Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1), Puesto = "QA" } }
        };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(creado);
        Assert.Equal(dto.NombreCompleto, creado.NombreCompleto);
        Assert.NotNull(respuesta.Headers.Location);
        Assert.EndsWith($"/api/colaboradores/{creado.Id}", respuesta.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostColaborador_DatosInvalidos_Devuelve400ConDetalle()
    {
        // Arrange
        var dto = new CrearColaboradorDto { NombreCompleto = "", Correo = "invalid", Telefono = "1", FechaNacimiento = new DateOnly(2000, 1, 1), Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = 1, FechaIngreso = new DateOnly(2021, 1, 1) } } };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("NombreCompleto"));
    }

    [Fact]
    public async Task Crear_ColaboradorSinEmpresas_Error()
    {
        // Arrange
        var dto = new CrearColaboradorDto { NombreCompleto = "Sin Empresa", Correo = "se@test.com", Telefono = "12345678", FechaNacimiento = new DateOnly(2000, 1, 1), Empresas = [] };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("Empresas"));
    }

    [Fact]
    public async Task Crear_CorreoDuplicado_Conflicto()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C2", "c2@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "A1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "dup@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var res1 = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        res1.EnsureSuccessStatusCode();

        var dtoDup = new CrearColaboradorDto { NombreCompleto = "A2", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "dup@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dtoDup, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetColaborador_Existente_DevuelveEdadYEmpresas()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C3", "c3@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "B1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "b1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRes = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRes.EnsureSuccessStatusCode();
        var creado = await postRes.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var getRes = await Cliente.GetAsync($"/api/colaboradores/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        getRes.EnsureSuccessStatusCode();
        var colab = await getRes.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(colab);

        // Reloj fijo: 2027-02-28. Nace 1995-05-05 -> Edad 31.
        Assert.Equal(31, colab.Edad);
        Assert.NotEmpty(colab.Empresas);
        var empColab = colab.Empresas[0];
        Assert.Equal(empId, empColab.EmpresaId);
        Assert.Equal("NC", empColab.NombreComercial);
        Assert.Equal("Guatemala", empColab.PaisNombre);
        Assert.Equal(new DateOnly(2021, 1, 1), empColab.FechaIngreso);
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/colaboradores/9999";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_Valido_Devuelve200()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C4", "c4@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "C1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "c1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoActualizar = new GuardarColaboradorDto { NombreCompleto = "C1 Edit", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "c1@test.com" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/colaboradores/{creado!.Id}", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(actualizado);
        Assert.Equal(dtoActualizar.NombreCompleto, actualizado.NombreCompleto);
    }

    [Fact]
    public async Task Actualizar_Inexistente_Devuelve404()
    {
        // Arrange
        var dtoActualizar = new GuardarColaboradorDto { NombreCompleto = "Inexistente", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "in@test.com" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync("/api/colaboradores/9999", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Existente_Devuelve204()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C5", "c5@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "D1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "d1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/colaboradores/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var getRes = await Cliente.GetAsync($"/api/colaboradores/{creado.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/colaboradores/9999";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task AsociarEmpresa_SegundaEmpresa_QuedaConDos()
    {
        // Arrange
        var empId1 = await CrearEmpresaAsync("C6A", "c6a@test.com");
        var empId2 = await CrearEmpresaAsync("C6B", "c6b@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "E1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "e1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId1, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var asocDto = new AsociarEmpresaDto { EmpresaId = empId2, FechaIngreso = new DateOnly(2022, 1, 1), Puesto = "Developer" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync($"/api/colaboradores/{creado!.Id}/empresas", asocDto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        var getRes = await Cliente.GetAsync($"/api/colaboradores/{creado.Id}", TestContext.Current.CancellationToken);
        var colab = await getRes.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.Equal(2, colab!.Empresas.Count);
    }

    [Fact]
    public async Task AsociarEmpresa_YaAsociada_Conflicto()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C7", "c7@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "F1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "f1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var asocDto = new AsociarEmpresaDto { EmpresaId = empId, FechaIngreso = new DateOnly(2022, 1, 1), Puesto = "Developer" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync($"/api/colaboradores/{creado!.Id}/empresas", asocDto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task QuitarEmpresa_UltimaEmpresa_Conflicto()
    {
        // Arrange
        await CrearEmpresaAsync("Extra1", "extra1@test.com");
        var empId = await CrearEmpresaAsync("C8", "c8@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "G1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "g1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        Assert.NotEqual(creado!.Id, empId);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/colaboradores/{creado.Id}/empresas/{empId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task EliminarEmpresa_ConDos_QuedaUnaYDevuelve204()
    {
        // Arrange
        await CrearEmpresaAsync("Extra2", "extra2@test.com");
        var empId1 = await CrearEmpresaAsync("C8A", "c8a@test.com");
        var empId2 = await CrearEmpresaAsync("C8B", "c8b@test.com");
        var dto = new CrearColaboradorDto
        {
            NombreCompleto = "G2",
            FechaNacimiento = new DateOnly(1995, 5, 5),
            Telefono = "12345678",
            Correo = "g2@test.com",
            Empresas = new List<AsociarEmpresaDto> {
                new() { EmpresaId = empId1, FechaIngreso = new DateOnly(2021, 1, 1) },
                new() { EmpresaId = empId2, FechaIngreso = new DateOnly(2021, 1, 1) }
            }
        };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        Assert.NotEqual(creado!.Id, empId1);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/colaboradores/{creado!.Id}/empresas/{empId1}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var getRes = await Cliente.GetAsync($"/api/colaboradores/{creado.Id}", TestContext.Current.CancellationToken);
        var colab = await getRes.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.Single(colab!.Empresas);
        Assert.Equal(empId2, colab.Empresas[0].EmpresaId);
    }

    [Fact]
    public async Task EliminarEmpresa_Inexistente_Devuelve404()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C9A", "c9a@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "G3", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "g3@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var requestUri = $"/api/colaboradores/{creado!.Id}/empresas/9999";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task EditarEmpresa_Inexistente_Devuelve404()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C9B", "c9b@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "G4", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "g4@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var putDto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(2021, 6, 1), Puesto = "Sr Dev" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/colaboradores/{creado!.Id}/empresas/9999", putDto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_FechaIngresoFutura_Error()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C9", "c9@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "H1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "h1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2100, 1, 1) } } };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.Contains(problema!.Errors, e => e.Key == "Empresas[0].FechaIngreso");
    }

    [Fact]
    public async Task Crear_DemasiadoJoven_ErrorRN4()
    {
        // Arrange
        var empId = await CrearEmpresaAsync("C11", "c11@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "Joven", FechaNacimiento = new DateOnly(2020, 1, 1), Telefono = "12345678", Correo = "joven@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1) } } };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.Contains(problema!.Errors, e => e.Key == "FechaNacimiento");
    }

    [Fact]
    public async Task EditarEmpresa_Valido_Devuelve200()
    {
        // Arrange
        await CrearEmpresaAsync("Extra3", "extra3@test.com");
        var empId = await CrearEmpresaAsync("C10", "c10@test.com");
        var dto = new CrearColaboradorDto { NombreCompleto = "I1", FechaNacimiento = new DateOnly(1995, 5, 5), Telefono = "12345678", Correo = "i1@test.com", Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = empId, FechaIngreso = new DateOnly(2021, 1, 1), Puesto = "Dev" } } };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/colaboradores", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);

        Assert.NotEqual(creado!.Id, empId);

        var putDto = new GuardarEmpresaColaboradorDto { FechaIngreso = new DateOnly(2021, 6, 1), Puesto = "Sr Dev" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/colaboradores/{creado!.Id}/empresas/{empId}", putDto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<ColaboradorDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(actualizado);
        Assert.Equal(putDto.Puesto, actualizado.Empresas[0].Puesto);
    }
}
