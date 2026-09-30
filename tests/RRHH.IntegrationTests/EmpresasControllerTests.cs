using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Empresas;
using RRHH.Contratos.Municipios;
using RRHH.Contratos.Paises;

namespace RRHH.IntegrationTests;

public class EmpresasControllerTests : PruebaIntegracionBase
{
    public EmpresasControllerTests(FabricaApi fabrica) : base(fabrica) { }

    [Fact]
    public async Task Listar_SinBusqueda_DevuelvePagina()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = "111199", RazonSocial = "RS Listar", NombreComercial = "NC", Telefono = "11111111", Correo = "emp@test.com" };
        var postRes = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRes.EnsureSuccessStatusCode();

        var requestUri = "/api/empresas";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<EmpresaDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, e => e.RazonSocial == "RS Listar");
    }

    [Fact]
    public async Task Listar_Busqueda_DevuelveCoincidencia()
    {
        // Arrange
        var e1 = new GuardarEmpresaDto { MunicipioId = 1, Nit = "222299", RazonSocial = "RS Buscar 1", NombreComercial = "NC", Telefono = "11111111", Correo = "emp1@test.com" };
        var e2 = new GuardarEmpresaDto { MunicipioId = 1, Nit = "333399", RazonSocial = "RS Filtro 2", NombreComercial = "NC", Telefono = "11111111", Correo = "emp2@test.com" };
        var res1 = await Cliente.PostAsJsonAsync("/api/empresas", e1, OpcionesJson, TestContext.Current.CancellationToken);
        res1.EnsureSuccessStatusCode();
        var res2 = await Cliente.PostAsJsonAsync("/api/empresas", e2, OpcionesJson, TestContext.Current.CancellationToken);
        res2.EnsureSuccessStatusCode();

        var requestUri = "/api/empresas?buscar=Buscar";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<EmpresaDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, e => e.RazonSocial == "RS Buscar 1");
        Assert.DoesNotContain(pagina.Elementos, e => e.RazonSocial == "RS Filtro 2");
    }

    [Fact]
    public async Task Listar_PaginacionInvalida_Devuelve400()
    {
        // Arrange
        var requestUri = "/api/empresas?tamanio=0";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_Valido_Devuelve201ConUbicacion()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = "111111", RazonSocial = "RS Valida", NombreComercial = "NC Valida", Telefono = "11111111", Correo = "empresa@test.com" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(creado);
        Assert.Equal(dto.Nit, creado.Nit);
        Assert.NotNull(respuesta.Headers.Location);
        Assert.EndsWith($"/api/empresas/{creado.Id}", respuesta.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostEmpresa_DatosInvalidos_Devuelve400ConDetalle()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 0, Nit = "", RazonSocial = "", NombreComercial = "", Telefono = "invalid", Correo = "invalid" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("Nit"));
    }

    [Fact]
    public async Task Crear_MunicipioInexistente_ErrorV4()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 9999, Nit = "111112", RazonSocial = "RS", NombreComercial = "NC", Telefono = "11111111", Correo = "emp@test.com" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await ExtraerValidationProblemAsync(respuesta);
        Assert.NotNull(problema);
        Assert.Contains(problema.Errors, e => e.Key.Contains("MunicipioId"));
    }

    [Fact]
    public async Task Crear_NitDuplicadoEnMismoPais_Conflicto()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = "222222", RazonSocial = "RS1", NombreComercial = "NC1", Telefono = "22222222", Correo = "empresa1@test.com" };
        var res1 = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);
        res1.EnsureSuccessStatusCode();

        var dtoDuplicado = new GuardarEmpresaDto { MunicipioId = 1, Nit = "222222", RazonSocial = "RS2", NombreComercial = "NC2", Telefono = "22222223", Correo = "empresa2@test.com" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/empresas", dtoDuplicado, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_NitRepetidoEnOtroPais_Creada()
    {
        // Arrange
        var pais = new GuardarPaisDto { Nombre = "Otro Pais", CodigoIso2 = "OP", EdadMinima = 18, EdadMaxima = 100, Regla29Febrero = Regla29Febrero.VeintiochoDeFebrero };
        var pRes = await Cliente.PostAsJsonAsync("/api/paises", pais, OpcionesJson, TestContext.Current.CancellationToken);
        pRes.EnsureSuccessStatusCode();
        var pCreado = await pRes.Content.ReadFromJsonAsync<PaisDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dep = new GuardarDepartamentoDto { PaisId = pCreado!.Id, Nombre = "Otro Dep" };
        var dRes = await Cliente.PostAsJsonAsync("/api/departamentos", dep, OpcionesJson, TestContext.Current.CancellationToken);
        dRes.EnsureSuccessStatusCode();
        var dCreado = await dRes.Content.ReadFromJsonAsync<DepartamentoDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var mun = new GuardarMunicipioDto { DepartamentoId = dCreado!.Id, Nombre = "Otro Mun" };
        var mRes = await Cliente.PostAsJsonAsync("/api/municipios", mun, OpcionesJson, TestContext.Current.CancellationToken);
        mRes.EnsureSuccessStatusCode();
        var mCreado = await mRes.Content.ReadFromJsonAsync<MunicipioDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dto1 = new GuardarEmpresaDto { MunicipioId = 1, Nit = "333333", RazonSocial = "RS1", NombreComercial = "NC1", Telefono = "33333333", Correo = "empresa3@test.com" };
        var eRes1 = await Cliente.PostAsJsonAsync("/api/empresas", dto1, OpcionesJson, TestContext.Current.CancellationToken);
        eRes1.EnsureSuccessStatusCode();

        var dto2 = new GuardarEmpresaDto { MunicipioId = mCreado!.Id, Nit = "333333", RazonSocial = "RS2", NombreComercial = "NC2", Telefono = "33333334", Correo = "empresa4@test.com" };

        // Act
        var respuesta = await Cliente.PostAsJsonAsync("/api/empresas", dto2, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetEmpresa_Existente_DevuelveGeografiaCompleta()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = "444444", RazonSocial = "RS", NombreComercial = "NC", Telefono = "44444444", Correo = "empresa5@test.com" };
        var postRes = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRes.EnsureSuccessStatusCode();
        var creado = await postRes.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var getRes = await Cliente.GetAsync($"/api/empresas/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        getRes.EnsureSuccessStatusCode();
        var empresa = await getRes.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(empresa);
        Assert.Equal(1, empresa.MunicipioId);
        Assert.Equal("Guatemala", empresa.MunicipioNombre);
        Assert.Equal(1, empresa.DepartamentoId);
        Assert.Equal("Guatemala", empresa.DepartamentoNombre);
        Assert.Equal(1, empresa.PaisId);
        Assert.Equal("Guatemala", empresa.PaisNombre);
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/empresas/9999";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_Valido_Devuelve200()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = "555555", RazonSocial = "RS", NombreComercial = "NC", Telefono = "55555555", Correo = "empresa6@test.com" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoActualizar = new GuardarEmpresaDto { MunicipioId = 1, Nit = "555555", RazonSocial = "RS Edit", NombreComercial = "NC Edit", Telefono = "55555555", Correo = "empresa6@test.com" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync($"/api/empresas/{creado!.Id}", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(actualizado);
        Assert.Equal(dtoActualizar.RazonSocial, actualizado.RazonSocial);
    }

    [Fact]
    public async Task Actualizar_Inexistente_Devuelve404()
    {
        // Arrange
        var dtoActualizar = new GuardarEmpresaDto { MunicipioId = 1, Nit = "555555", RazonSocial = "RS", NombreComercial = "NC", Telefono = "55555555", Correo = "empresa@test.com" };

        // Act
        var respuesta = await Cliente.PutAsJsonAsync("/api/empresas/9999", dtoActualizar, OpcionesJson, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Existente_Devuelve204()
    {
        // Arrange
        var dto = new GuardarEmpresaDto { MunicipioId = 1, Nit = "666666", RazonSocial = "RS", NombreComercial = "NC", Telefono = "66666666", Correo = "empresa7@test.com" };
        var postRespuesta = await Cliente.PostAsJsonAsync("/api/empresas", dto, OpcionesJson, TestContext.Current.CancellationToken);
        postRespuesta.EnsureSuccessStatusCode();
        var creado = await postRespuesta.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/empresas/{creado!.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var getRes = await Cliente.GetAsync($"/api/empresas/{creado.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/empresas/9999";

        // Act
        var respuesta = await Cliente.DeleteAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_EmpresaConColaboradores_Conflicto()
    {
        // Arrange
        var dtoEmp = new GuardarEmpresaDto { MunicipioId = 1, Nit = "777777", RazonSocial = "RS", NombreComercial = "NC", Telefono = "77777777", Correo = "empresa8@test.com" };
        var resEmp = await Cliente.PostAsJsonAsync("/api/empresas", dtoEmp, OpcionesJson, TestContext.Current.CancellationToken);
        resEmp.EnsureSuccessStatusCode();
        var emp = await resEmp.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoColab = new CrearColaboradorDto
        {
            NombreCompleto = "Juan Perez",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            Telefono = "12345678",
            Correo = "juan.perez@test.com",
            Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = emp!.Id, FechaIngreso = new DateOnly(2020, 1, 1), Puesto = "Developer" } }
        };
        var resCol = await Cliente.PostAsJsonAsync("/api/colaboradores", dtoColab, OpcionesJson, TestContext.Current.CancellationToken);
        resCol.EnsureSuccessStatusCode();

        // Act
        var respuesta = await Cliente.DeleteAsync($"/api/empresas/{emp.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task ObtenerColaboradores_Existente_DevuelveLista()
    {
        // Arrange
        var dtoEmp = new GuardarEmpresaDto { MunicipioId = 1, Nit = "888888", RazonSocial = "RS", NombreComercial = "NC", Telefono = "88888888", Correo = "empresa9@test.com" };
        var resEmp = await Cliente.PostAsJsonAsync("/api/empresas", dtoEmp, OpcionesJson, TestContext.Current.CancellationToken);
        resEmp.EnsureSuccessStatusCode();
        var emp = await resEmp.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoColab = new CrearColaboradorDto
        {
            NombreCompleto = "Pedro Perez",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            Telefono = "12345678",
            Correo = "pedro.perez@test.com",
            Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = emp!.Id, FechaIngreso = new DateOnly(2020, 1, 1), Puesto = "Developer" } }
        };
        var resCol = await Cliente.PostAsJsonAsync("/api/colaboradores", dtoColab, OpcionesJson, TestContext.Current.CancellationToken);
        resCol.EnsureSuccessStatusCode();

        var dtoEmp2 = new GuardarEmpresaDto { MunicipioId = 1, Nit = "888889", RazonSocial = "RS2", NombreComercial = "NC2", Telefono = "88888889", Correo = "empresa10@test.com" };
        var resEmp2 = await Cliente.PostAsJsonAsync("/api/empresas", dtoEmp2, OpcionesJson, TestContext.Current.CancellationToken);
        resEmp2.EnsureSuccessStatusCode();
        var emp2 = await resEmp2.Content.ReadFromJsonAsync<EmpresaDto>(OpcionesJson, TestContext.Current.CancellationToken);

        var dtoColab2 = new CrearColaboradorDto
        {
            NombreCompleto = "Otra Empresa",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            Telefono = "12345678",
            Correo = "otra@test.com",
            Empresas = new List<AsociarEmpresaDto> { new() { EmpresaId = emp2!.Id, FechaIngreso = new DateOnly(2020, 1, 1), Puesto = "Developer" } }
        };
        var resCol2 = await Cliente.PostAsJsonAsync("/api/colaboradores", dtoColab2, OpcionesJson, TestContext.Current.CancellationToken);
        resCol2.EnsureSuccessStatusCode();

        // Act
        var respuesta = await Cliente.GetAsync($"/api/empresas/{emp.Id}/colaboradores", TestContext.Current.CancellationToken);

        // Assert
        respuesta.EnsureSuccessStatusCode();
        var pagina = await respuesta.Content.ReadFromJsonAsync<Pagina<ColaboradorDto>>(OpcionesJson, TestContext.Current.CancellationToken);
        Assert.NotNull(pagina);
        Assert.Contains(pagina.Elementos, c => c.NombreCompleto == "Pedro Perez");
        Assert.DoesNotContain(pagina.Elementos, c => c.NombreCompleto == "Otra Empresa");
    }

    [Fact]
    public async Task ObtenerColaboradores_Inexistente_Devuelve404()
    {
        // Arrange
        var requestUri = "/api/empresas/9999/colaboradores";

        // Act
        var respuesta = await Cliente.GetAsync(requestUri, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
