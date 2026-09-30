using RRHH.Contratos.Comun;

namespace RRHH.Web.Pages.Shared;

public static class Listados
{
    // Consulta válida para la API aunque la URL traiga una página o un texto fuera de rango
    public static Consulta Consulta(int pagina, string? buscar) => new()
    {
        Pagina = Math.Max(1, pagina),
        Buscar = buscar is { Length: > 100 } ? buscar[..100] : buscar
    };
}
