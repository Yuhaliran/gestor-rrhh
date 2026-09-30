using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

using RRHH.Contratos.Colaboradores;
using RRHH.Contratos.Comun;
using RRHH.Contratos.Departamentos;
using RRHH.Contratos.Empresas;
using RRHH.Contratos.Municipios;
using RRHH.Contratos.Paises;

namespace RRHH.Web.Api;

// Cliente tipado de la API (CODIFICACION.md, «Web»): la web no tiene lógica de negocio, todo pasa
// por acá. Lecturas: el DTO, o null si no existe. Escrituras: un Resultado con el valor o el
// problema que devolvió la API. Un error no previsto (500, sin conexión) lanza una excepción.
public class ClienteRrhh(HttpClient http)
{
    // Como la API: nombres en camelCase y enums como texto
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Países
    public Task<Pagina<PaisDto>> ListarPaisesAsync(Consulta consulta, CancellationToken ct) =>
        LeerAsync<Pagina<PaisDto>>(ConConsulta("api/paises", consulta), ct);
    public Task<IReadOnlyList<PaisDto>> TodosLosPaisesAsync(CancellationToken ct) =>
        TodasLasPaginasAsync<PaisDto>("api/paises", ct);
    public Task<PaisDto?> ObtenerPaisAsync(int id, CancellationToken ct) =>
        LeerOpcionalAsync<PaisDto>($"api/paises/{id}", ct);
    public Task<Resultado<PaisDto>> CrearPaisAsync(GuardarPaisDto dto, CancellationToken ct) =>
        EnviarAsync<PaisDto>(HttpMethod.Post, "api/paises", dto, ct);
    public Task<Resultado<PaisDto>> ActualizarPaisAsync(int id, GuardarPaisDto dto, CancellationToken ct) =>
        EnviarAsync<PaisDto>(HttpMethod.Put, $"api/paises/{id}", dto, ct);
    public Task<Resultado> EliminarPaisAsync(int id, CancellationToken ct) =>
        EliminarAsync($"api/paises/{id}", ct);

    // Departamentos
    public Task<Pagina<DepartamentoDto>> ListarDepartamentosAsync(Consulta consulta, CancellationToken ct) =>
        LeerAsync<Pagina<DepartamentoDto>>(ConConsulta("api/departamentos", consulta), ct);
    public async Task<IReadOnlyList<DepartamentoDto>> DepartamentosDePaisAsync(int paisId, CancellationToken ct) =>
        await LeerOpcionalAsync<List<DepartamentoDto>>($"api/paises/{paisId}/departamentos", ct) ?? [];
    public Task<DepartamentoDto?> ObtenerDepartamentoAsync(int id, CancellationToken ct) =>
        LeerOpcionalAsync<DepartamentoDto>($"api/departamentos/{id}", ct);
    public Task<Resultado<DepartamentoDto>> CrearDepartamentoAsync(GuardarDepartamentoDto dto, CancellationToken ct) =>
        EnviarAsync<DepartamentoDto>(HttpMethod.Post, "api/departamentos", dto, ct);
    public Task<Resultado<DepartamentoDto>> ActualizarDepartamentoAsync(int id, GuardarDepartamentoDto dto, CancellationToken ct) =>
        EnviarAsync<DepartamentoDto>(HttpMethod.Put, $"api/departamentos/{id}", dto, ct);
    public Task<Resultado> EliminarDepartamentoAsync(int id, CancellationToken ct) =>
        EliminarAsync($"api/departamentos/{id}", ct);

    // Municipios
    public Task<Pagina<MunicipioDto>> ListarMunicipiosAsync(Consulta consulta, CancellationToken ct) =>
        LeerAsync<Pagina<MunicipioDto>>(ConConsulta("api/municipios", consulta), ct);
    public async Task<IReadOnlyList<MunicipioDto>> MunicipiosDeDepartamentoAsync(int departamentoId, CancellationToken ct) =>
        await LeerOpcionalAsync<List<MunicipioDto>>($"api/departamentos/{departamentoId}/municipios", ct) ?? [];
    public Task<MunicipioDto?> ObtenerMunicipioAsync(int id, CancellationToken ct) =>
        LeerOpcionalAsync<MunicipioDto>($"api/municipios/{id}", ct);
    public Task<Resultado<MunicipioDto>> CrearMunicipioAsync(GuardarMunicipioDto dto, CancellationToken ct) =>
        EnviarAsync<MunicipioDto>(HttpMethod.Post, "api/municipios", dto, ct);
    public Task<Resultado<MunicipioDto>> ActualizarMunicipioAsync(int id, GuardarMunicipioDto dto, CancellationToken ct) =>
        EnviarAsync<MunicipioDto>(HttpMethod.Put, $"api/municipios/{id}", dto, ct);
    public Task<Resultado> EliminarMunicipioAsync(int id, CancellationToken ct) =>
        EliminarAsync($"api/municipios/{id}", ct);

    // Empresas
    public Task<Pagina<EmpresaDto>> ListarEmpresasAsync(Consulta consulta, CancellationToken ct) =>
        LeerAsync<Pagina<EmpresaDto>>(ConConsulta("api/empresas", consulta), ct);
    public Task<IReadOnlyList<EmpresaDto>> TodasLasEmpresasAsync(CancellationToken ct) =>
        TodasLasPaginasAsync<EmpresaDto>("api/empresas", ct);
    public Task<EmpresaDto?> ObtenerEmpresaAsync(int id, CancellationToken ct) =>
        LeerOpcionalAsync<EmpresaDto>($"api/empresas/{id}", ct);
    public Task<Resultado<EmpresaDto>> CrearEmpresaAsync(GuardarEmpresaDto dto, CancellationToken ct) =>
        EnviarAsync<EmpresaDto>(HttpMethod.Post, "api/empresas", dto, ct);
    public Task<Resultado<EmpresaDto>> ActualizarEmpresaAsync(int id, GuardarEmpresaDto dto, CancellationToken ct) =>
        EnviarAsync<EmpresaDto>(HttpMethod.Put, $"api/empresas/{id}", dto, ct);
    public Task<Resultado> EliminarEmpresaAsync(int id, CancellationToken ct) =>
        EliminarAsync($"api/empresas/{id}", ct);
    public async Task<Pagina<ColaboradorDto>?> ColaboradoresDeEmpresaAsync(int empresaId, Consulta consulta, CancellationToken ct) =>
        await LeerOpcionalAsync<Pagina<ColaboradorDto>>(ConConsulta($"api/empresas/{empresaId}/colaboradores", consulta), ct);

    // Colaboradores
    public Task<Pagina<ColaboradorDto>> ListarColaboradoresAsync(Consulta consulta, CancellationToken ct) =>
        LeerAsync<Pagina<ColaboradorDto>>(ConConsulta("api/colaboradores", consulta), ct);
    public Task<ColaboradorDto?> ObtenerColaboradorAsync(int id, CancellationToken ct) =>
        LeerOpcionalAsync<ColaboradorDto>($"api/colaboradores/{id}", ct);
    public Task<Resultado<ColaboradorDto>> CrearColaboradorAsync(CrearColaboradorDto dto, CancellationToken ct) =>
        EnviarAsync<ColaboradorDto>(HttpMethod.Post, "api/colaboradores", dto, ct);
    public Task<Resultado<ColaboradorDto>> ActualizarColaboradorAsync(int id, GuardarColaboradorDto dto, CancellationToken ct) =>
        EnviarAsync<ColaboradorDto>(HttpMethod.Put, $"api/colaboradores/{id}", dto, ct);
    public Task<Resultado> EliminarColaboradorAsync(int id, CancellationToken ct) =>
        EliminarAsync($"api/colaboradores/{id}", ct);
    public Task<Resultado<ColaboradorDto>> AsociarEmpresaAsync(int id, AsociarEmpresaDto dto, CancellationToken ct) =>
        EnviarAsync<ColaboradorDto>(HttpMethod.Post, $"api/colaboradores/{id}/empresas", dto, ct);
    public Task<Resultado<ColaboradorDto>> ActualizarEmpresaDeColaboradorAsync(int id, int empresaId,
        GuardarEmpresaColaboradorDto dto, CancellationToken ct) =>
        EnviarAsync<ColaboradorDto>(HttpMethod.Put, $"api/colaboradores/{id}/empresas/{empresaId}", dto, ct);
    public Task<Resultado> QuitarEmpresaAsync(int id, int empresaId, CancellationToken ct) =>
        EliminarAsync($"api/colaboradores/{id}/empresas/{empresaId}", ct);

    // ?pagina=&tamanio=&buscar= (buscar sólo si tiene texto)
    private static string ConConsulta(string ruta, Consulta consulta)
    {
        var parametros = $"pagina={consulta.Pagina}&tamanio={consulta.Tamanio}";
        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            parametros += $"&buscar={Uri.EscapeDataString(consulta.Buscar)}";
        }
        return $"{ruta}?{parametros}";
    }

    // Para las listas de selección: recorre todas las páginas del listado, de a 100
    private async Task<IReadOnlyList<T>> TodasLasPaginasAsync<T>(string ruta, CancellationToken ct)
    {
        var todos = new List<T>();
        for (var numero = 1; ; numero++)
        {
            var pagina = await LeerAsync<Pagina<T>>(ConConsulta(ruta, new Consulta { Pagina = numero, Tamanio = 100 }), ct);
            todos.AddRange(pagina.Elementos);
            if (numero >= pagina.TotalPaginas)
            {
                return todos;
            }
        }
    }

    private async Task<T> LeerAsync<T>(string ruta, CancellationToken ct)
    {
        using var respuesta = await http.GetAsync(ruta, ct);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<T>(Json, ct))!;
    }

    private async Task<T?> LeerOpcionalAsync<T>(string ruta, CancellationToken ct) where T : class
    {
        using var respuesta = await http.GetAsync(ruta, ct);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    private async Task<Resultado<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object dto, CancellationToken ct)
    {
        using var pedido = new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(dto, dto.GetType(), options: Json) };
        using var respuesta = await http.SendAsync(pedido, ct);
        if (respuesta.IsSuccessStatusCode)
        {
            return new Resultado<T>(await respuesta.Content.ReadFromJsonAsync<T>(Json, ct), null);
        }
        return new Resultado<T>(default, await ProblemaAsync(respuesta, ct));
    }

    private async Task<Resultado> EliminarAsync(string ruta, CancellationToken ct)
    {
        using var respuesta = await http.DeleteAsync(ruta, ct);
        return new Resultado(respuesta.IsSuccessStatusCode ? null : await ProblemaAsync(respuesta, ct));
    }

    // Los rechazos previstos (400, 404, 409) llegan como ProblemDetails; lo demás es un error
    private static async Task<ValidationProblemDetails> ProblemaAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (respuesta.StatusCode is not (HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict))
        {
            respuesta.EnsureSuccessStatusCode();
        }
        return await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json, ct)
            ?? new ValidationProblemDetails { Status = (int)respuesta.StatusCode, Detail = respuesta.ReasonPhrase };
    }
}
