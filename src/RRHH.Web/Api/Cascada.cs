namespace RRHH.Web.Api;

// Listas en cascada (RN6): el navegador pide las opciones a la web y la web, a la API. Así el
// navegador no necesita conocer la API.
public static class Cascada
{
    public record Opcion(int Id, string Nombre);

    public static void MapCascada(this IEndpointRouteBuilder app)
    {
        var cascada = app.MapGroup("/cascada");
        cascada.MapGet("/paises/{paisId:int}/departamentos", async (int paisId, ClienteRrhh api, CancellationToken ct) =>
            (await api.DepartamentosDePaisAsync(paisId, ct)).Select(d => new Opcion(d.Id, d.Nombre)));
        cascada.MapGet("/departamentos/{departamentoId:int}/municipios", async (int departamentoId, ClienteRrhh api, CancellationToken ct) =>
            (await api.MunicipiosDeDepartamentoAsync(departamentoId, ct)).Select(m => new Opcion(m.Id, m.Nombre)));
    }
}
